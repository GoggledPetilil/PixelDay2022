using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance;

    [Header("Pause Components")]
    public bool m_IsPaused;
    public GameObject m_PauseCanvas;

    [Header("Audio Components")]
    [SerializeField] private AudioSource m_Audio;
    [SerializeField] private AudioClip m_PauseSound;
    [SerializeField] private AudioClip m_ResumeSound;

    void Awake()
    {
        instance = this;
        m_Audio = GetComponent<AudioSource>();
    }

    void Start()
    {
        Resume();
        if(m_PauseCanvas.activeSelf) m_PauseCanvas.SetActive(false);
    }

    void Update()
    {
        if(!GameplayManager.instance.m_GameStarted) return;

        if(Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if(m_IsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        if(m_IsPaused == false) return;

        Time.timeScale = 1f;
        m_IsPaused = false;
        m_PauseCanvas.SetActive(false);
        GameManager.instance.m_AudioMixer.SetFloat("musicVolume", GameManager.instance.m_MusicVolume);

        //m_Audio.clip = m_ResumeSound;
        //m_Audio.Play();
    }

    public void Pause()
    {
        if(m_IsPaused == true) return;

        Time.timeScale = 0f;
        m_IsPaused = true;
        m_PauseCanvas.SetActive(true);
        GameManager.instance.m_AudioMixer.SetFloat("musicVolume", -80f);

        //m_Audio.clip = m_PauseSound;
        //m_Audio.Play();
    }
}
