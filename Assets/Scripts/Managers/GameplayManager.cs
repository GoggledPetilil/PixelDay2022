using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager instance;

    [Header("Gameplay Variables")]
    public bool m_GameStarted;
    public float m_Timer;
    public int m_TotalHits;
    public int m_BestCombo;
    public int m_Money;

    [Header("Level Components")]
    public GameObject[] m_AllLayouts;
    public float m_LayoutChangeRate;
    public float m_ChangeDuration;
    private float m_NextChange;
    private GameObject m_CurrentLayout;
    private int m_CurrentLayoutIndex;

    [Header("Timer Components")]
    public bool m_TimerActive;
    [SerializeField] private TMP_Text m_TimerText;

    [Header("Audio Components")]
    [SerializeField] private AudioSource m_Audio;
    [SerializeField] private AudioClip m_ShowSound;
    [SerializeField] private AudioClip m_CountingSound;
    [SerializeField] private AudioClip m_NewRecordSound;
    [SerializeField] private AudioSource m_MusicPlayer;
    [SerializeField] private AudioClip m_GameplayMusic;
    [SerializeField] private AudioClip m_ResultsMusic;

    [Header("Results Components")]
    [SerializeField] private GameObject m_ResultsScreen;
    [SerializeField] private GameObject m_TimeResults;
    [SerializeField] private GameObject m_HitsResults;
    [SerializeField] private GameObject m_ComboResults;
    [SerializeField] private GameObject m_ScoreResults;
    [SerializeField] private GameObject m_NewHighScore;
    [SerializeField] private TMP_Text m_EndTimeText;
    [SerializeField] private TMP_Text m_HitsText;
    [SerializeField] private TMP_Text m_ComboText;
    [SerializeField] private TMP_Text m_ScoreText;
    private float pauseBetweenText = 0.4f;   // The small pause between text showing up.
    private float scoreCountTime = 1.5f;     // How long it takes for the score to tally up.
    private bool resultsEnded;               // If true then the results have ended.
    private bool levelEnded;                 // The player has ended the level.

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        m_ResultsScreen.SetActive(false);
        m_TimeResults.SetActive(false);
        m_HitsResults.SetActive(false);
        m_ComboResults.SetActive(false);
        m_ScoreResults.SetActive(false);
        m_NewHighScore.SetActive(false);

        m_Money = GameManager.instance.m_Money;

        StartGame();
    }

    void Update()
    {
        if(m_TimerActive)
        {
            m_Timer = m_Timer += Time.deltaTime;
            var timeSpan = System.TimeSpan.FromSeconds(m_Timer);
            m_TimerText.text = timeSpan.Hours.ToString("00") + ":" +
                                timeSpan.Minutes.ToString("00") + ":" +
                                timeSpan.Seconds.ToString("00") + "." +
                                (timeSpan.Milliseconds / 10).ToString("00");

            if(Time.time > m_NextChange)
            {
                m_NextChange = Time.time + (m_LayoutChangeRate * Random.Range(0.75f, 1.25f));
                ChangeLayout();
            }
        }

        if(resultsEnded)
        {
            if(Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
            {
                if(levelEnded) return;

                GameManager.instance.PlayConfirmSFX();
                EndLevel();
            }
        }
    }

    void PlayAudio(AudioClip clip)
    {
        m_Audio.clip = clip;
        m_Audio.Play();
    }

    public void TimerActive(bool state)
    {
        m_TimerActive = state;
    }

    public void StartGame()
    {
        m_GameStarted = true;
        TimerActive(true);
        m_MusicPlayer.clip = m_GameplayMusic;
        m_MusicPlayer.loop = true;
        m_MusicPlayer.Play();
        GameManager.instance.FadeIn();
        GameManager.instance.m_GameStarted = true;
    }

    public void StopMusic()
    {
        m_MusicPlayer.Stop();
    }

    public void EndLevel()
    {
        levelEnded = true;
        GameManager.instance.m_Money = this.m_Money;
        int score = FinalScore();
        if(GameManager.instance.m_HighScore < score)
        {
            GameManager.instance.m_HighScore = score;
        }
        GameManager.instance.SaveData();

        GameManager.instance.LoadLevel(0);
    }

    public int FinalScore()
    {
        int score = Mathf.FloorToInt((m_Timer * 10) + (m_BestCombo * 100) + (m_TotalHits * 10));
        return score;
    }

    public void GameOver()
    {
        StartCoroutine("GameOverScreen");
    }

    public void ChangeLayout()
    {
        StartCoroutine("LayoutChange");
    }

    IEnumerator LayoutChange()
    {
        EnemyManager.instance.m_StopSpawning = true;
        EnemyManager.instance.ClearSpawnPoints();
        Vector3 startSize = Vector3.one;
        Vector3 endSize = Vector3.zero;
        float t = 0.0f;

        // Destroy old layout, if it exists
        if(m_CurrentLayout != null)
        {
            while(t < 1f)
            {
                t += Time.deltaTime / m_ChangeDuration;
                m_CurrentLayout.transform.localScale = Vector3.Lerp(startSize, endSize, t);
                yield return null;
            }
            Destroy(m_CurrentLayout);
            m_CurrentLayout = null;
        }

        // Get a random layout
        int r = Random.Range(0, m_AllLayouts.Length - 1);
        if(r == m_CurrentLayoutIndex)
        {
            // Prevent new layout from being the same as the last one.
            if(r == m_AllLayouts.Length - 1)
            {
                r = 0;
            }
            else if(r == 0)
            {
                r = m_AllLayouts.Length - 1;
            }
            else
            {
                r++;
            }
        }

        // SpawnLayout
        GameObject layout = Instantiate(m_AllLayouts[r], Vector3.zero, Quaternion.identity) as GameObject;
        layout.transform.localScale = endSize;
        m_CurrentLayout = layout;
        m_CurrentLayoutIndex = r;
        t = 0.0f;
        while(t < 1f)
        {
            t += Time.deltaTime / m_ChangeDuration;
            m_CurrentLayout.transform.localScale = Vector3.Lerp(endSize, startSize, t);
            yield return null;
        }

        EnemyManager.instance.m_StopSpawning = false;
        EnemyManager.instance.GetAllSpawnPoints();

        yield return null;
    }

    IEnumerator GameOverScreen()
    {
        yield return new WaitForSeconds(1f);

        // Results Box
        m_ResultsScreen.SetActive(true);
        m_MusicPlayer.clip = m_ResultsMusic;
        m_MusicPlayer.loop = true;
        m_MusicPlayer.Play();
        PlayAudio(m_ShowSound);
        yield return new WaitForSeconds(pauseBetweenText);

        // Time
        m_TimeResults.SetActive(true);
        var timeSpan = System.TimeSpan.FromSeconds(m_Timer);
        m_EndTimeText.text = timeSpan.Hours.ToString("00") + ":" +
                            timeSpan.Minutes.ToString("00") + ":" +
                            timeSpan.Seconds.ToString("00") + "." +
                            (timeSpan.Milliseconds / 10).ToString("00");
                            PlayAudio(m_ShowSound);
        yield return new WaitForSeconds(pauseBetweenText);

        // Total Hits
        m_HitsResults.SetActive(true);
        m_HitsText.text = m_TotalHits.ToString();
        PlayAudio(m_ShowSound);
        yield return new WaitForSeconds(pauseBetweenText);

        // Best Combo
        m_ComboResults.SetActive(true);
        m_ComboText.text = m_BestCombo.ToString();
        PlayAudio(m_ShowSound);
        yield return new WaitForSeconds(pauseBetweenText);

        // Score
        m_ScoreResults.SetActive(true);
        int finalScore = FinalScore();
        m_Audio.loop = true;
        PlayAudio(m_CountingSound);
        float t = 0.0f;
        while(t < 1f)
        {
            t += Time.deltaTime / scoreCountTime;
            float score = Mathf.Lerp(0.0f, finalScore, t);
            m_ScoreText.text = Mathf.FloorToInt(score).ToString();

            if(Input.GetKeyDown(KeyCode.Z) || finalScore < 10)
            {
                t = 1f;
            }
            yield return null;
        }
        m_Audio.loop = false;
        m_Audio.Stop();

        // High Score
        if(finalScore > GameManager.instance.m_HighScore)
        {
            m_NewHighScore.SetActive(true);
            PlayAudio(m_NewRecordSound);
        }

        resultsEnded = true;

        yield return null;
    }
}
