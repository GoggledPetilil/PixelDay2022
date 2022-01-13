using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HatShop : MonoBehaviour
{
    public List<Accessory> m_Items = new List<Accessory>();
    public int m_Selected;

    [Header("UI Components")]
    [SerializeField] private TMP_Text m_MoneyHeld;
    [SerializeField] private TMP_Text m_HatName;
    [SerializeField] private Image m_HatDisplay;
    [SerializeField] private TMP_Text m_HatPrice;
    [SerializeField] private TMP_Text m_BuyText;
    private bool canEquip;

    void Start()
    {
        m_MoneyHeld.text = "$" + GameManager.instance.m_Money.ToString();

        StartOnEquip();
    }

    void StartOnEquip()
    {
        // Start on the current equiped item.
        if(GameManager.instance.m_EquipID > 0 && GameManager.instance.m_EquipID < m_Items.Count + 1)
        {
            for(int i = 0; i < m_Items.Count; i++)
            {
                if(m_Items[i].ID == GameManager.instance.m_EquipID)
                {
                    m_Selected = i;
                    AdjustDisplay();
                    return;
                }
            }
        }
        else
        {
            m_Selected = 0;
            AdjustDisplay();
        }
    }

    public void FarLeftClick()
    {
        m_Selected = 0;
        AdjustDisplay();
    }

    public void LeftClick()
    {
        m_Selected--;
        AdjustDisplay();
    }

    public void RightClick()
    {
        m_Selected++;
        AdjustDisplay();
    }

    public void FarRightClick()
    {
        m_Selected = m_Items.Count - 1;
        AdjustDisplay();
    }

    public void EquipBuy()
    {
        // Check if the item has already been obtained
        if(canEquip)
        {
            GameManager.instance.PlayConfirmSFX();
            GameManager.instance.m_EquipID = m_Items[m_Selected].ID;
            EquipItem();
        }
        else if(GameManager.instance.m_ItemIDs.Contains(m_Items[m_Selected].ID) == false)
        {
            // Buy the item
            if(GameManager.instance.m_Money < m_Items[m_Selected].price)
            {
                // Not enough cash
                GameManager.instance.PlayCancelSFX();
                StartCoroutine("MoneyShake");
            }
            else
            {
                // Adjust Cash
                GameManager.instance.PlayConfirmSFX();
                GameManager.instance.m_Money -= m_Items[m_Selected].price;
                m_MoneyHeld.text = "$" + GameManager.instance.m_Money.ToString();

                // Equip the item
                int id = m_Items[m_Selected].ID;
                GameManager.instance.m_ItemIDs.Add(id);
                GameManager.instance.m_EquipID = id;
                EquipItem();

                // Adjust the UI
                m_HatPrice.text = "";
                m_BuyText.text = "Unequip";
                canEquip = false;

                GameManager.instance.SaveData();
            }
        }
        else
        {
            // Unequip the item.
            GameManager.instance.PlayConfirmSFX();
            GameManager.instance.m_EquipedHat = null;
            GameManager.instance.m_EquipID = 0;

            m_HatPrice.text = "";
            m_BuyText.text = "Equip";
            canEquip = true;
        }
    }

    public void EquipItem()
    {
        if(GameManager.instance.m_ItemIDs.Contains(GameManager.instance.m_EquipID) == false) return;

        foreach(Accessory hat in m_Items)
        {
            if(hat.ID == GameManager.instance.m_EquipID)
            {
                GameManager.instance.m_EquipedHat = hat;
                m_BuyText.text = "Unequip";
                canEquip = false;
                return;
            }
        }
    }

    void AdjustDisplay()
    {
        if(m_Selected < 0)
        {
            m_Selected = m_Items.Count - 1;
        }
        else if(m_Selected > m_Items.Count - 1)
        {
            m_Selected = 0;
        }

        Accessory hat = m_Items[m_Selected];
        RectTransform hatTransform = m_HatDisplay.GetComponent<RectTransform>();

        m_HatName.text = hat.name;
        m_HatDisplay.sprite = hat.sprite;
        hatTransform.anchoredPosition = hat.offset * 100f;

        // Check if the item has already been obtained
        if(GameManager.instance.m_ItemIDs.Contains(hat.ID))
        {
            if(GameManager.instance.m_EquipedHat == hat)
            {
                m_HatPrice.text = "";
                m_BuyText.text = "Unequip";
                canEquip = false;
            }
            else
            {
                m_HatPrice.text = "";
                m_BuyText.text = "Equip";
                canEquip = true;
            }
        }
        else
        {
            m_HatPrice.text = "$" + hat.price.ToString("F0");
            m_BuyText.text = "Buy";
            canEquip = false;
        }
    }

    IEnumerator MoneyShake()
    {
        RectTransform moneyTransform = m_MoneyHeld.GetComponent<RectTransform>();
        Vector3 originPos = moneyTransform.anchoredPosition;
        float shakePow = 2f;
        float t = 0.0f;
        float shakeDur = 0.5f;
        while(t < 1f)
        {
            t += Time.deltaTime / shakeDur;

            float offsetX = Random.Range(-1f, 1f) * shakePow;
            float offsetY = Random.Range(-1f, 1f) * shakePow;
            moneyTransform.anchoredPosition = new Vector2(originPos.x + offsetX, originPos.y + offsetY);

            yield return null;
        }

        moneyTransform.anchoredPosition = originPos;
        yield return null;
    }
}
