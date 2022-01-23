using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class MenuController : MonoBehaviour
{
    [Header("Menu Controller")]
    [SerializeField] protected EventSystem m_EventSystem;
    [SerializeField] protected AudioMixer m_AudioMixer;
    [SerializeField] protected GameObject m_SelectedObject; // The interactable that the game is supposed to have selected.
    [SerializeField] private AudioSource m_MusicPlayer;
    [SerializeField] private AudioClip m_MenuMusic;

    [Header("Intro Components")]
    [SerializeField] private IntroCards m_IntroAnimation;
    [SerializeField] private float m_IntroDuration;

    [Header("Main Menu Components")]
    [SerializeField] protected GameObject m_MainTab;
    [SerializeField] protected GameObject m_MainFirstObj;
    [SerializeField] private TMP_Text m_HighScoreText;

    [Header("Settings Components")]
    [SerializeField] protected GameObject m_SettingsTab;
    [SerializeField] protected GameObject m_SettingsContent;    // The interactables under the Settings tab.
    [SerializeField] protected GameObject m_SettingsFirstObj;
    protected RectTransform m_SettingsTransform;
    protected List<Selectable> m_SettingsElements = new List<Selectable>();

    [Header("Settings Interactables")]
    [SerializeField] protected Slider m_MasterSlider;
    [SerializeField] protected Slider m_MusicSlider;
    [SerializeField] protected Slider m_SoundSlider;
    [SerializeField] protected Slider m_ShakeSlider;
    [SerializeField] protected QualityList m_QualityList;
    private bool m_ChangedMaster;
    private bool m_ChangedMusic;
    private bool m_ChangedSound;

    [Header("Avatar Components")]
    [SerializeField] protected GameObject m_AvatarTab;
    [SerializeField] protected GameObject m_AvatarFirstObj;

    [Header("Avatar Interactables")]
    [SerializeField] private Image m_CharacterImage;
    [SerializeField] private HatShop m_HatShop;

    void Start()
    {
        m_AudioMixer = GameManager.instance.m_AudioMixer;

        m_EventSystem.SetSelectedGameObject(null);
        m_SelectedObject = m_EventSystem.firstSelectedGameObject;

        m_SettingsTransform = m_SettingsContent.GetComponent<RectTransform>();
        m_SettingsContent.GetComponentsInChildren(m_SettingsElements);

        LoadSavedSettings();
        m_HatShop.EquipItem();

        if(GameManager.instance.m_GameStarted)
        {
            m_IntroAnimation.gameObject.SetActive(false);
            EnableMainMenu();
            PlayMenuMusic();
            GameManager.instance.FadeIn();
        }
        else
        {
            m_MusicPlayer.Stop();
            m_MainTab.SetActive(false);
            m_SettingsTab.SetActive(false);
            m_AvatarTab.SetActive(false);

            StartCoroutine("IntroSequence");
        }
    }

    void Update()
    {
        CheckSelected();
    }

    protected void CheckSelected()
    {
        if(m_EventSystem.currentSelectedGameObject == null)
        {
            m_EventSystem.SetSelectedGameObject(m_SelectedObject);
        }
        else if(m_EventSystem.currentSelectedGameObject != m_SelectedObject)
        {
            // Because this only activates when the currentSelectedObject != null,
            // m_SelectedObject will never be null.
            m_SelectedObject = m_EventSystem.currentSelectedGameObject;
            CursorSound();
        }
    }

    protected void MenuControls()
    {
        if(Input.GetButtonUp("Cancel") && m_MainTab.activeSelf == false)
        {
            EnableMainMenu();
            CancelSound();
        }

        if(m_SettingsTab.activeSelf)
        {
            if(m_EventSystem.currentSelectedGameObject != m_SelectedObject && m_EventSystem.currentSelectedGameObject != null)
            {
                int i = m_SettingsElements.IndexOf(m_EventSystem.currentSelectedGameObject.GetComponent<Selectable>());
                if(i < 0)
                {
                    foreach(Selectable parent in m_SettingsElements)
                    {
                        if(m_EventSystem.currentSelectedGameObject.transform.IsChildOf(parent.transform))
                        {
                            i = m_SettingsElements.IndexOf(parent);
                            return;
                        }
                    }
                }
            }
        }
    }

    public void CursorSound()
    {
        GameManager.instance.PlayCursorSFX();
    }

    public void ConfirmSound()
    {
        GameManager.instance.PlayConfirmSFX();
    }

    public void CancelSound()
    {
        GameManager.instance.PlayCancelSFX();
    }

    public void PlayMenuMusic()
    {
        m_MusicPlayer.loop = true;
        m_MusicPlayer.clip = m_MenuMusic;
        m_MusicPlayer.Play();
    }

    protected void LoadSavedSettings()
    {
        m_MasterSlider.value = GameManager.instance.m_MasterVolume;
        m_MusicSlider.value = GameManager.instance.m_MusicVolume;
        m_SoundSlider.value = GameManager.instance.m_SoundVolume;
        m_QualityList.LoadSavedSettings();

        m_ChangedMaster = false;
        m_ChangedMusic = false;
        m_ChangedSound = false;
        GameManager.instance.m_SettingsChangedLevel = 0;
    }

    protected void SaveSettings()
    {
        GameManager.instance.m_MasterVolume = m_MasterSlider.value;
        GameManager.instance.m_MusicVolume = m_MusicSlider.value;
        GameManager.instance.m_SoundVolume = m_SoundSlider.value;
        GameManager.instance.m_ShakePower = m_ShakeSlider.value;
        m_QualityList.SaveSettings();

        m_ChangedMaster = false;
        m_ChangedMusic = false;
        m_ChangedSound = false;
        GameManager.instance.SaveData();
    }

    public void StartGame()
    {
        GameManager.instance.LoadLevel(1);
    }

    public virtual void EnableMainMenu()
    {
        m_MainTab.SetActive(true);
        m_SettingsTab.SetActive(false);
        m_AvatarTab.SetActive(false);

        m_EventSystem.SetSelectedGameObject(m_MainFirstObj);

        SaveSettings();
        GameManager.instance.SaveData();

        m_HighScoreText.text = "Your High-Score:\n" + GameManager.instance.m_HighScore;
    }

    public virtual void EnableSettingsMenu()
    {
        m_SettingsTab.SetActive(true);
        m_MainTab.SetActive(false);
        m_AvatarTab.SetActive(false);

        m_EventSystem.SetSelectedGameObject(m_SettingsFirstObj);
    }

    public virtual void EnableAvatarMenu()
    {
        m_AvatarTab.SetActive(true);
        m_MainTab.SetActive(false);
        m_SettingsTab.SetActive(false);

        m_EventSystem.SetSelectedGameObject(m_AvatarFirstObj);
    }

    public void SetMasterVolume(float volume)
    {
        Mathf.Round(volume);
        m_AudioMixer.SetFloat("masterVolume", volume);

        if(GameManager.instance.m_MasterVolume != volume && m_ChangedMaster == false)
        {
            GameManager.instance.m_SettingsChangedLevel++;
            m_ChangedMaster = true;
        }
        else if(GameManager.instance.m_MasterVolume == volume)
        {
            GameManager.instance.m_SettingsChangedLevel--;
            m_ChangedMaster = false;
        }
    }

    public void SetMusicVolume(float volume)
    {
        Mathf.Round(volume);
        m_AudioMixer.SetFloat("musicVolume", volume);

        if(GameManager.instance.m_MusicVolume != volume && m_ChangedMusic == false)
        {
            GameManager.instance.m_SettingsChangedLevel++;
            m_ChangedMusic = true;
        }
        else if(GameManager.instance.m_MusicVolume == volume)
        {
            GameManager.instance.m_SettingsChangedLevel--;
            m_ChangedMusic = false;
        }
    }

    public void SetSoundVolume(float volume)
    {
        Mathf.Round(volume);
        m_AudioMixer.SetFloat("soundVolume", volume);

        if(GameManager.instance.m_SoundVolume != volume && m_ChangedSound == false)
        {
            GameManager.instance.m_SettingsChangedLevel++;
            m_ChangedSound = true;
        }
        else if(GameManager.instance.m_SoundVolume == volume)
        {
            GameManager.instance.m_SettingsChangedLevel--;
            m_ChangedSound = false;
        }
    }

    public void SetShakePow(float amount)
    {
        GameManager.instance.m_ShakePower = amount;
    }

    IEnumerator IntroSequence()
    {
        m_IntroAnimation.gameObject.SetActive(true);

        yield return new WaitForSeconds(m_IntroDuration);

        EnableMainMenu();
        PlayMenuMusic();
        yield return null;
    }
}
