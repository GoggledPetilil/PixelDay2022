using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [Header("Parameters")]
    public Color m_Color;
    public int m_Speed;
    public float m_DestrucTime;
    private bool destroyed;

    [Header("Components")]
    [SerializeField] private AudioSource m_Audio;
    [SerializeField] private Rigidbody2D m_Body;
    [SerializeField] private SpriteRenderer m_Sprite;
    [SerializeField] private ParticleSystem m_Trail;
    [SerializeField] private GameObject m_Explosion;
    [SerializeField] private BoxCollider2D m_Collider;
    [SerializeField] private AudioClip m_ShootSound;

    void Start()
    {
        m_Audio.clip = m_ShootSound;
        m_Audio.Play();

        m_Trail.startColor = m_Color;
        m_Sprite.color = m_Color;

        Invoke("SpawnExplosion", m_DestrucTime);
    }

    void FixedUpdate()
    {
        m_Body.velocity = transform.right * m_Speed;
    }

    void SpawnExplosion()
    {
        if(destroyed) return;

        m_Sprite.enabled = false;
        m_Collider.enabled = false;
        destroyed = true;

        Instantiate(m_Explosion, transform.position, Quaternion.identity);

        m_Trail.Stop(); // The particle system will destroy the object once it's finished.
    }

    void ShakeScreen()
    {
        float m_shakeDur = 0.2f;
        float m_shakeMag = 0.4f;
        float m_shakePow = 0.5f;
        CameraManager.instance.ShakeCamera(m_shakeDur, m_shakeMag, m_shakePow);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.CompareTag("Enemy") || col.gameObject.CompareTag("Obstruct"))
        {
            ShakeScreen();
            SpawnExplosion();
        }
    }
}
