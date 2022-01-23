using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("GameManager Components")]
    public static GameManager instance;
    public AudioMixer m_AudioMixer;
    [SerializeField] private StartScript m_SaveManager;
    [SerializeField] private NGHelper m_NGHelper;
    [SerializeField] private Animator m_MedalAnimator;
    [SerializeField] private Animator m_SaveAnimator;
    [SerializeField] private Animator m_FadeAnimator;

    [Header("Audio Components")]
    [SerializeField] private GameObject m_OneShotAudio;
    [SerializeField] private AudioSource m_GeneralAudio;
    [SerializeField] private AudioSource m_CursorAudio;
    [SerializeField] private AudioSource m_SelectionAudio;
    [SerializeField] private AudioClip m_CursorSFX;
    [SerializeField] private AudioClip m_ConfirmSFX;
    [SerializeField] private AudioClip m_CancelSFX;
    [SerializeField] private AudioClip m_MedalSFX;

    [Header("Game Data")]
    public bool m_GameStarted;          // To indicate that the game has started, skipping the intro.
    public int m_SettingsChangedLevel;  // How many settings have been changed.
    public Accessory m_EquipedHat;      // The actual data for the equiped accessory.
    public int m_LevelToLoad;

    [Header("Settings Saved Data")]
    public int m_Money;
    public float m_MasterVolume;
    public float m_MusicVolume;
    public float m_SoundVolume;
    public int m_QualityLevel;
    public List<int> m_ItemIDs = new List<int>();
    public int m_EquipID;
    public int m_HighScore;
    public float m_ShakePower;
    public float m_CorpseDuration;
    public List<int> m_MedalIDs = new List<int>();

    void Awake()
    {
        if(GameManager.instance == null)
        {
            // Prepare GameManager
            instance = this;
            DontDestroyOnLoad(this.gameObject);

            // Set up GameManager components
            m_NGHelper = GameObject.Find("NGHelper").GetComponent<NGHelper>();
            m_MedalAnimator.gameObject.SetActive(false);
        }
        else
        {
            Destroy(this.gameObject);
        }

    }

    public void FadeIn()
    {
        m_FadeAnimator.SetTrigger("FadeIn");
    }

    public void FadeOut()
    {
        m_FadeAnimator.SetTrigger("FadeOut");
    }

    public void LoadLevel(int sceneID)
    {
        m_LevelToLoad = sceneID;

        StartCoroutine("LoadingLevel");
    }

    public AsyncOperation LoadLevelAsync(int sceneID)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneID);
        return asyncLoad;
    }

    public void PlayOneShot(AudioClip clip)
    {
        GameObject go = Instantiate(m_OneShotAudio) as GameObject;
        go.GetComponent<AudioSource>().clip = clip;
    }

    public void PlayOneShot(AudioClip clip, float volume, float pitch)
    {
        GameObject go = Instantiate(m_OneShotAudio) as GameObject;
        AudioSource auso = go.GetComponent<AudioSource>();
        auso.clip = clip;
        auso.volume = volume;
        auso.pitch = pitch;
    }

    public void PlaySound(AudioClip clip)
    {
        m_GeneralAudio.clip = clip;
        m_GeneralAudio.Play();
    }

    public void PlayCursorSFX()
    {
        m_CursorAudio.clip = m_CursorSFX;
        m_CursorAudio.Play();
    }

    public void PlayConfirmSFX()
    {
        m_SelectionAudio.clip = m_ConfirmSFX;
        m_SelectionAudio.Play();
    }

    public void PlayCancelSFX()
    {
        m_SelectionAudio.clip = m_CancelSFX;
        m_SelectionAudio.Play();
    }

    public void InitializeData()
    {
        LoadData();

        // Check special medals and stuff
        if(gItemIDs.Count < 1 || gItemIDs == null)
        {
            m_ItemIDs = new List<int>();
            m_ItemIDs.Add(1);
        }

        if(m_MedalIDs.Count < 1 || m_MedalIDs == null)
        {
            m_MedalIDs = new List<int>();
            UnlockMedal(67075);
        }

        string today = System.DateTime.Now.ToString("MM/dd");
        if(today == "01/23")
        {
            UnlockMedal(67038);
        }
    }

    public void SaveData()
    {
        m_SaveAnimator.SetTrigger("Saving");

        SaveSystem.SaveData(this);

        m_SettingsChangedLevel = 0;
    }

    public void LoadData()
    {
        PlayerData data = SaveSystem.LoadData();

        if(data == null) return;

        m_Money = data.money;
        m_MasterVolume = data.masterVolume;
        m_MusicVolume = data.musicVolume;
        m_SoundVolume = data.soundVolume;
        m_QualityLevel = data.qualityLevel;
        m_ItemIDs = data.itemIDs;
        m_EquipID = data.equipID;
        m_HighScore = data.highScore;
        m_ShakePower = data.shakePow;
        m_CorpseDuration = data.corpseDur;
        m_MedalIDs = data.medalIDs;

        m_AudioMixer.SetFloat("masterVolume", m_MasterVolume);
        m_AudioMixer.SetFloat("musicVolume", m_MusicVolume);
        m_AudioMixer.SetFloat("soundVolume", m_SoundVolume);
        QualitySettings.SetQualityLevel(m_QualityLevel, false);
    }

    public void SubmitScore(int scoreboardID, int score)
    {
        m_NGHelper.NGSubmitScore(11254, score);
    }

    public void UnlockMedal(int medalID)
    {
        m_NGHelper.unlockMedal(medalID);

        // The medal is always unlocked on Newgrounds, regardless.
        // This is just to celebrate it in-game.
        if(m_MedalIDs.Contains(medalID)) return;
        m_MedalIDs.Add(medalID);
        SaveData();

        m_MedalAnimator.gameObject.SetActive(true);
        m_MedalAnimator.SetTrigger("Unlock");
        PlaySound(m_MedalSFX);
    }

    public void GameOver()
    {
        LoadLevel(0);
    }

    IEnumerator LoadingLevel()
    {
        FadeOut();
        yield return new WaitForSeconds(0.6f);

        AsyncOperation asyncLoad = LoadLevelAsync(m_LevelToLoad);
        asyncLoad.allowSceneActivation = false;

        while(!asyncLoad.isDone)
        {
            if(asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}
