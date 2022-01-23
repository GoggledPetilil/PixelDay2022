using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Pathfinding;

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
    public float m_ChangeDuration;    // How long it takes for the stage to move into position.
    public float m_ScaleDuration;     // How long it takes for the stage to scale to proper size.
    private float m_NextChange;
    private GameObject m_CurrentLayout;
    private int m_CurrentLayoutIndex;

    [Header("Power-Up Components")]
    public List<GameObject> m_PowerUps = new List<GameObject>();
    public float m_PowerUpFrequency;
    private float m_NextPowerUp;
    private bool spawning;    // The game is currently spawning an object.

    [Header("Timer Components")]
    public bool m_TimerActive;
    [SerializeField] private TMP_Text m_TimerText;

    [Header("Music Components")]
    [SerializeField] private AudioSource m_MusicPlayer;
    [SerializeField] private AudioClip[] m_GameplayMusic;
    [SerializeField] private AudioClip m_ResultsMusic;

    [Header("Sound Components")]
    [SerializeField] private AudioSource m_StageEarthquake;
    [SerializeField] private AudioSource m_Audio;
    [SerializeField] private AudioClip m_ShowSound;
    [SerializeField] private AudioClip m_CountingSound;
    [SerializeField] private AudioClip m_NewRecordSound;

    [Header("Results Components")]
    [SerializeField] protected EventSystem m_EventSystem;
    [SerializeField] private GameObject m_ResultsScreen;
    [SerializeField] private GameObject m_TimeResults;
    [SerializeField] private GameObject m_HitsResults;
    [SerializeField] private GameObject m_ComboResults;
    [SerializeField] private GameObject m_ScoreResults;
    [SerializeField] private GameObject m_NewHighScore;
    [SerializeField] private GameObject m_ButtonsResults;
    [SerializeField] private TMP_Text m_EndTimeText;
    [SerializeField] private TMP_Text m_HitsText;
    [SerializeField] private TMP_Text m_ComboText;
    [SerializeField] private TMP_Text m_ScoreText;
    [SerializeField] private Button m_FirstSelectButton;
    private float pauseBetweenText = 0.4f;   // The small pause between text showing up.
    private float scoreCountTime = 1.5f;     // How long it takes for the score to tally up.
    private bool resultsEnded;               // If true then the results have ended.
    private bool levelEnded;                 // The player has ended the level.

    void Awake()
    {
        instance = this;
        m_MusicPlayer.clip = m_GameplayMusic[Random.Range(0, m_GameplayMusic.Length)];
    }

    void Start()
    {
        m_ResultsScreen.SetActive(false);
        m_TimeResults.SetActive(false);
        m_HitsResults.SetActive(false);
        m_ComboResults.SetActive(false);
        m_ScoreResults.SetActive(false);
        m_NewHighScore.SetActive(false);
        m_ButtonsResults.SetActive(false);

        m_Money = GameManager.instance.m_Money;

        m_MusicPlayer.loop = true;
        m_MusicPlayer.Play();

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

            if(Time.time > m_NextChange && spawning == false)
            {
                m_NextChange = Time.time + (m_LayoutChangeRate * Random.Range(0.75f, 1.25f));
                ChangeLayout();
            }

            if(Time.time > m_NextPowerUp)
            {
                m_NextPowerUp = Time.time + (m_PowerUpFrequency * Random.Range(0.90f, 1.5f));
                SpawnPower();
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
        m_NextChange = Time.time + 0.2f;
        m_NextPowerUp = Time.time + m_PowerUpFrequency;

        m_GameStarted = true;
        TimerActive(true);

        GameManager.instance.FadeIn();
        GameManager.instance.m_GameStarted = true;
    }

    public void StopMusic()
    {
        m_MusicPlayer.Stop();
    }

    public void EndLevel(int nextScene)
    {
        if(levelEnded == true) return;

        levelEnded = true;
        GameManager.instance.m_Money = this.m_Money;
        int score = FinalScore();
        GameManager.instance.SubmitScore(11254, score);
        if(GameManager.instance.m_HighScore < score)
        {
            GameManager.instance.m_HighScore = score;
        }
        GameManager.instance.SaveData();

        GameManager.instance.LoadLevel(nextScene);
    }

    public int FinalScore()
    {
        int score = Mathf.FloorToInt((m_Timer * 10) + (m_BestCombo * 100) + (m_TotalHits * 10));
        return score;
    }

    public void SpawnPower()
    {
        if(m_PowerUps.Count < 1) return;

        StartCoroutine("SpawnPowerUp");
    }

    public void GameOver()
    {
        StartCoroutine("GameOverScreen");
    }

    public void ChangeLayout()
    {
        StartCoroutine("LayoutChange");
    }

    void CreateGrid()
    {
        AstarData data = AstarPath.active.astarData;

        AstarPath.active.Scan();
    }

    IEnumerator SpawnPowerUp()
    {
        spawning = true;
        GameObject[] points = GameObject.FindGameObjectsWithTag("Respawn");
        int randItem = Random.Range(0, m_PowerUps.Count - 1);
        int randPos = Random.Range(0, points.Length - 1);

        Animator anim = points[randPos].GetComponent<Animator>();
        if(anim != null)
        {
            anim.SetTrigger("Spawn");
            yield return new WaitForSeconds(0.4f);
        }

        Instantiate(m_PowerUps[randItem], points[randPos].transform.position, Quaternion.identity);

        m_PowerUps.RemoveAt(randItem);
        spawning = false;

        yield return null;
    }

    IEnumerator LayoutChange()
    {
        EnemyManager.instance.m_StopSpawning = true;
        EnemyManager.instance.ClearSpawnPoints();

        Vector3 originPos = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 newPos = new Vector3(0.0f, 15.0f, 0.0f);
        Vector3 originScale = Vector3.one;
        float scaleSize = 0.3f;
        Vector3 backScale = new Vector3(scaleSize, scaleSize, 1.0f);
        float t = 0.0f;

        // Destroy old layout, if it exists
        if(m_CurrentLayout != null)
        {
            // Make old stage shake, to warn the player.
            List<ParticleSystem> dustParticles = new List<ParticleSystem>();
            dustParticles.AddRange(m_CurrentLayout.GetComponentsInChildren<ParticleSystem>());
            foreach(ParticleSystem dust in dustParticles)
            {
                dust.Play();
            }
            m_StageEarthquake.Play();
            float shakeDuration = 2f;
            while(t < 1f)
            {
                t += Time.deltaTime / shakeDuration;
                float offsetX = Random.Range(-0.2f, 0.2f);
                float offsetY = Random.Range(-0.2f, 0.2f);
                m_CurrentLayout.transform.position = new Vector3(0.0f + offsetX, 0.0f + offsetY, 0.0f);
                yield return null;
            }
            foreach(ParticleSystem dust in dustParticles)
            {
                dust.Stop();
            }
            m_StageEarthquake.Stop();
            t = 0.0f;

            Vector3 oldPos = new Vector3(0.0f, -15.0f, 0.0f); // Where the stage will go.
            // Disable colliders.
            foreach(Collider2D c in m_CurrentLayout.GetComponentsInChildren<Collider2D>())
            {
                c.enabled = false;
            }
            // Moving stage back
            while(t < 1f)
            {
                t += Time.deltaTime / m_ScaleDuration;
                m_CurrentLayout.transform.localScale = Vector3.Lerp(originScale, backScale, t);
                yield return null;
            }
            t = 0.0f;
            yield return new WaitForSeconds(0.15f);
            // Move stage down.
            while(t < 1f)
            {
                t += Time.deltaTime / m_ChangeDuration;
                m_CurrentLayout.transform.position = Vector3.Lerp(originPos, oldPos, t);
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
        GameObject layout = Instantiate(m_AllLayouts[r], newPos, Quaternion.identity) as GameObject;
        layout.transform.localScale = backScale;
        m_CurrentLayout = layout;
        m_CurrentLayoutIndex = r;
        foreach(Collider2D c in m_CurrentLayout.GetComponentsInChildren<Collider2D>())
        {
            c.enabled = false;
        }
        t = 0.0f;
        // Moving stage into position.
        while(t < 1f)
        {
            t += Time.deltaTime / m_ChangeDuration;
            m_CurrentLayout.transform.position = Vector3.Lerp(newPos, originPos, t);
            yield return null;
        }
        t = 0.0f;
        yield return new WaitForSeconds(0.15f);
        // Scaling stage into view.
        while(t < 1f)
        {
            t += Time.deltaTime / m_ScaleDuration;
            m_CurrentLayout.transform.localScale = Vector3.Lerp(backScale, originScale, t);
            yield return null;
        }
        // Enabling colliders.
        foreach(Collider2D c in m_CurrentLayout.GetComponentsInChildren<Collider2D>())
        {
            c.enabled = true;
        }

        EnemyManager.instance.m_StopSpawning = false;
        EnemyManager.instance.GetAllSpawnPoints();
        CreateGrid();

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

        // Check if any achievements have been unlocked.
        CheckMedalConditions();

        // Control buttons
        m_ButtonsResults.SetActive(true);
        m_EventSystem.SetSelectedGameObject(m_FirstSelectButton.gameObject);

        resultsEnded = true;

        yield return null;
    }

    void CheckMedalConditions()
    {
        int score = FinalScore();

        if(score < 100)
        {
            GameManager.instance.UnlockMedal(67029);
        }

        if(score < 1000)
        {
            GameManager.instance.UnlockMedal(67031);
        }

        if(score >= 10000)
        {
            GameManager.instance.UnlockMedal(67032);
        }

        if(m_BestCombo >= 100)
        {
            GameManager.instance.UnlockMedal(67035);
        }

        if(m_Timer >= 300)
        {
            GameManager.instance.UnlockMedal(67036);
        }

        if(m_BestCombo >= 20 && m_TotalHits >= 20 && m_TotalHits == m_BestCombo)
        {
            GameManager.instance.UnlockMedal(67037);
        }
    }
}
