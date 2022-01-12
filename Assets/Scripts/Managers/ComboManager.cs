using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComboManager : MonoBehaviour
{
    public static ComboManager instance;

    [Header("Components")]
    [SerializeField] private AudioSource m_Audio;
    [SerializeField] private AudioClip m_CountingSound;
    [SerializeField] private GameObject m_UIHolder;
    [SerializeField] private TMP_Text m_NumText;
    [SerializeField] private TMP_Text m_RatingText;
    [SerializeField] private Image m_FillImage;
    private Player m_Player;

    private int comboNum;
    private float increaseTime = 0.1f;  // How long it takes for the count to increase.
    private float sizeIncrease = 1.5f;  // How big the count gets when it increases.

    [SerializeField] private float timerDur = 2.0f;      // How long it takes for the timer to go down.
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
        m_RatingText.text = "";
        m_UIHolder.SetActive(false);
        m_Audio.clip = m_CountingSound;
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

    public void BreakCombo()
    {
        StopAllCoroutines();
        timerDur = 0.01f;
        waitTime = 0.01f;
        fadeTime = 0.01f;
        StartCoroutine("Timer");
    }

    IEnumerator Timer()
    {
        yield return null;

        // Play Sound
        float startPitch = 0.9f;
        m_Audio.pitch = Mathf.Clamp(startPitch + comboNum / 10f, startPitch, 2f);
        m_Audio.Play();

        // Increase the number & Rate it
        Vector3 originSize = Vector3.one;
        Vector3 newSize = new Vector3(sizeIncrease, sizeIncrease, originSize.z);

        string originRating = "";
        if(m_RatingText.transform.localScale == Vector3.one)
        {
            originRating = m_RatingText.text;
        }
        string rating = ComboRating();
        if(originRating != rating) m_RatingText.text = rating;

        float t = 0.0f;
        while(t <= 1)
        {
            t += Time.deltaTime / increaseTime;
            m_NumText.transform.localScale = Vector3.Lerp(newSize, originSize, t);
            if(originRating != rating)
            {
                m_RatingText.transform.localScale = Vector3.Lerp(newSize, originSize, t);
            }

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
        m_RatingText.text = "";
        GameplayManager.instance.m_TotalHits += comboNum;
        if(GameplayManager.instance.m_BestCombo < comboNum) GameplayManager.instance.m_BestCombo = comboNum;
        comboNum = 0;

        yield return null;
    }

    private string ComboRating()
    {
        string rating = "";
        if(comboNum < 5)
        {
            rating = "";
        }
        else if(comboNum < 10)
        {
            rating = "OK";
        }
        else if(comboNum < 15)
        {
            rating = "GOOD";
        }
        else if(comboNum < 20)
        {
            rating = "GREAT";
        }
        else if(comboNum < 25)
        {
            rating = "AWESOME";
        }
        else if(comboNum < 30)
        {
            rating = "SEXY";
        }
        else if(comboNum < 35)
        {
            rating = "GROOOOVY";
        }
        else if(comboNum < 40)
        {
            rating = "RAD";
        }
        else if(comboNum < 45)
        {
            rating = "KILLIN IT";
        }
        else if(comboNum < 50)
        {
            rating = "YOOOOO";
        }
        else if(comboNum < 55)
        {
            rating = "BRO";
        }
        else if(comboNum < 60)
        {
            rating = "KING";
        }
        else if(comboNum < 65)
        {
            rating = "BASED AF";
        }
        else if(comboNum < 70)
        {
            rating = "CHAD";
        }
        else if(comboNum < 75)
        {
            rating = "DEADLY";
        }
        else if(comboNum < 80)
        {
            rating = "CHAOTIC";
        }
        else if(comboNum < 85)
        {
            rating = "BRUTAL";
        }
        else if(comboNum < 90)
        {
            rating = "AMAZING";
        }
        else if(comboNum < 95)
        {
            rating = "SPECTACULAR";
        }
        else if(comboNum < 100)
        {
            rating = "SSUPERIOR";
        }
        else if(comboNum < 105)
        {
            rating = "SSSTYLISH";
        }
        else
        {
            rating = "U DONE?";
        }
        return rating;
    }
}
