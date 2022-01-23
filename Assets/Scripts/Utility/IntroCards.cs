using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroCards : MonoBehaviour
{
    [Header("Intro Components")]
    [SerializeField] private GameObject m_IntroNewgrounds;
    [SerializeField] private GameObject m_IntroDevTeam;
    [SerializeField] private GameObject m_IntroTitle;

    [Header("Sound Components")]
    [SerializeField] private AudioSource m_SoundPlayer1;
    [SerializeField] private AudioSource m_SoundPlayer2;
    [SerializeField] private AudioClip m_LightFlickerSound;
    [SerializeField] private AudioClip m_MouseDragSound;
    [SerializeField] private AudioClip m_SpawnSound;
    [SerializeField] private AudioClip m_LandingSound;
    [SerializeField] private AudioClip m_TitleSound;
    [SerializeField] private AudioClip m_FlashSound;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("IntroSequence");
    }

    // Update is called once per frame
    void Update()
    {
        // If Player presses Z or Enter, then the intro will skip.
    }

    void PlaySoundEffect(AudioClip sound)
    {
        if(m_SoundPlayer1.isPlaying == false)
        {
            m_SoundPlayer1.clip = sound;
            m_SoundPlayer1.Play();
        }
        else
        {
            m_SoundPlayer2.clip = sound;
            m_SoundPlayer2.Play();
        }

    }

    void StopSoundEffect()
    {
        m_SoundPlayer1.Stop();
        m_SoundPlayer2.Stop();
    }

    public void ReadData()
    {
        GameManager.instance.InitializeData();
    }

    IEnumerator IntroSequence()
    {
        m_IntroDevTeam.SetActive(false);
        m_IntroTitle.SetActive(false);

        // Newgrounds Logo
        m_IntroNewgrounds.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        PlaySoundEffect(m_LightFlickerSound);
        yield return new WaitForSeconds(5/60f);
        StopSoundEffect();
        yield return new WaitForSeconds(5/60f);
        PlaySoundEffect(m_LightFlickerSound);
        yield return new WaitForSeconds(5/60f);
        StopSoundEffect();
        yield return new WaitForSeconds(0.4f);
        PlaySoundEffect(m_LightFlickerSound);
        yield return new WaitForSeconds(1.34f);
        StopSoundEffect();
        m_IntroNewgrounds.SetActive(false);

        // Dev Team Credits
        m_IntroDevTeam.SetActive(true);
        yield return new WaitForSeconds(5/60f);
        PlaySoundEffect(m_MouseDragSound);
        yield return new WaitForSeconds(1/6f);
        PlaySoundEffect(m_MouseDragSound);

        yield return new WaitForSeconds(11/60f);
        PlaySoundEffect(m_SpawnSound);
        yield return new WaitForSeconds(1/6f);
        PlaySoundEffect(m_SpawnSound);

        yield return new WaitForSeconds(14/60f);
        PlaySoundEffect(m_LandingSound);
        yield return new WaitForSeconds(1/6f);
        PlaySoundEffect(m_LandingSound);
        yield return new WaitForSeconds(1f);

        m_IntroDevTeam.SetActive(false);

        // Title Logo
        m_IntroTitle.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        PlaySoundEffect(m_TitleSound);
        yield return new WaitForSeconds(2/6f);
        PlaySoundEffect(m_TitleSound);
        yield return new WaitForSeconds(0.25f);
        // Play some kinda drag back sound.

        yield return new WaitForSeconds(0.4f);
        PlaySoundEffect(m_FlashSound);

        yield return new WaitForSeconds(0.5f);

        ReadData();

        yield return null;
    }
}
