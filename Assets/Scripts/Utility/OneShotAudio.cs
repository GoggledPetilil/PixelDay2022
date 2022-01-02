using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneShotAudio : MonoBehaviour
{
    [SerializeField] private AudioSource m_Audio;
    private bool m_Started;

    void Awake()
    {
        m_Audio = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        m_Audio.Play();
        m_Started = true;
    }

    void Update()
    {
        if(m_Audio.clip != null && m_Audio.isPlaying == false && m_Started)
        {
            Destroy(this.gameObject);
        }
    }
}
