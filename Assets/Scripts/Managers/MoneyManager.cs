using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager instance;

    [Header("Components")]
    [SerializeField] private TMP_Text m_TotalText;
    [SerializeField] private TMP_Text m_AdditionText;
    [SerializeField] private Animator m_Anim;
    [SerializeField] private AudioSource m_Audio;
    [SerializeField] public AudioClip m_MoneyCountSFX;
    private RectTransform m_MoneyTransform;
    private Vector2 m_MoneyOriginPos;

    private int m_DisplayedMoney; // This isn't how much the player actually has, just what is currently displayed.
    private int m_AddedMoney; // The money that will be added to the total sum.

    private float waitTime = 1f; // How long it'll wait before counting everything up.
    private float sumDuration = 0.25f; // How long it takes for everything to be counted up.

    void Awake()
    {
        instance = this;

        m_Anim = gameObject.GetComponent<Animator>();
        m_Audio = gameObject.GetComponent<AudioSource>();

        m_MoneyTransform = m_TotalText.GetComponent<RectTransform>();
        m_MoneyOriginPos = m_MoneyTransform.anchoredPosition;
    }

    void Start()
    {
        m_Audio.clip = m_MoneyCountSFX;

        m_DisplayedMoney = 0;

        m_TotalText.text = "$" + m_DisplayedMoney.ToString();
        m_AdditionText.text = "";
    }

    public void ChangeMoney(int addition)
    {
        StopAllCoroutines();
        m_Audio.Stop();
        m_AddedMoney += addition;
        m_AdditionText.text = "+" + m_AddedMoney.ToString("F0");
        StartCoroutine("MoneyCount");
    }

    IEnumerator MoneyCount()
    {
        yield return null;

        yield return new WaitForSeconds(waitTime);

        int startAdded = m_AddedMoney;
        int startTotal = m_DisplayedMoney;
        int endTotal = m_DisplayedMoney + m_AddedMoney;
        float t = 0.0f;
        m_Audio.Play();
        while(t <= 1)
        {
            t += Time.deltaTime / sumDuration;
            m_AddedMoney = Mathf.FloorToInt(Mathf.Lerp(startAdded, 0, t));
            m_DisplayedMoney = Mathf.FloorToInt(Mathf.Lerp(startTotal, endTotal, t));

            m_AdditionText.text = "+" + m_AddedMoney.ToString("F0");
            m_TotalText.text = "$" + m_DisplayedMoney.ToString();

            float power = Random.Range(-5, 5);
            float xPos = m_MoneyOriginPos.x + power;
            float yPos = m_MoneyOriginPos.y + power;
            m_MoneyTransform.anchoredPosition = new Vector2(xPos, yPos);

            yield return null;
        }
        m_Audio.Stop();
        m_AdditionText.text = "";
        m_MoneyTransform.anchoredPosition = m_MoneyOriginPos;
        yield return null;
    }
}
