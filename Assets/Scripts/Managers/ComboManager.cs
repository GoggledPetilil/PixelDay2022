using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComboManager : MonoBehaviour
{
    public static ComboManager instance;

    [Header("Components")]
    private Player m_Player;
    [SerializeField] private GameObject m_UIHolder;
    [SerializeField] private TMP_Text m_NumText;
    [SerializeField] private Image m_FillImage;

    private int comboNum;
    private float increaseTime = 0.1f;  // How long it takes for the count to increase.
    private float sizeIncrease = 1.5f;  // How big the count gets when it increases.

    private float timerDur = 1.2f;      // How long it takes for the timer to go down.
    private float waitTime = 1.0f;      // How long until the combo count fades out.
    private float fadeTime = 0.2f;      // How long until the combo count has faded out.

    void Awake()
    {
        instance = this;
        m_Player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    void Start()
    {
        m_NumText.text = "";
        m_UIHolder.SetActive(false);
    }

    public void IncreaseCombo()
    {
        StopAllCoroutines();
        comboNum = Mathf.Clamp(comboNum + 1, 0, 9999);
        m_NumText.text = comboNum.ToString();
        m_FillImage.fillAmount = 1.0f;

        m_UIHolder.SetActive(true);
        StartCoroutine("Timer");
    }

    IEnumerator Timer()
    {
        yield return null;

        // Increase the number
        Vector3 originSize = Vector3.one;
        Vector3 newSize = new Vector3(sizeIncrease, sizeIncrease, originSize.z);
        float t = 0.0f;
        while(t <= 1)
        {
            t += Time.deltaTime / increaseTime;
            m_NumText.transform.localScale = Vector3.Lerp(newSize, originSize, t);

            yield return null;
        }

        // Have timer go down
        t = 0.0f;
        while(t <= 1)
        {
            t += Time.deltaTime / timerDur;
            m_FillImage.fillAmount = Mathf.Lerp(1.0f, 0.0f, t);

            yield return null;
        }

        float moneyMade = Mathf.Clamp((comboNum * comboNum) / 2, 0, 999999999);
        MoneyManager.instance.ChangeMoney(Mathf.FloorToInt(moneyMade));
        m_UIHolder.SetActive(false);
        m_NumText.text = "";
        comboNum = 0;

        yield return null;
    }
}
