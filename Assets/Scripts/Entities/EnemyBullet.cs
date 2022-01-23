using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("Parameters")]
    public int m_Speed;
    public float m_DestrucTime;
    private bool destroyed;
    private Transform target;

    [Header("Components")]
    [SerializeField] private AudioSource m_ShootSound;
    [SerializeField] private Rigidbody2D m_Body;
    [SerializeField] private SpriteRenderer m_Sprite;
    [SerializeField] private ParticleSystem m_Trail;
    [SerializeField] private GameObject m_Explosion;
    [SerializeField] private BoxCollider2D m_Collider;

    void Awake()
    {
        target = GameObject.FindWithTag("Player").transform;
    }

    void Start()
    {
        Invoke("SpawnExplosion", m_DestrucTime);
    }

    void FixedUpdate()
    {
        float step = m_Speed * Time.deltaTime;
        Vector2 targetPos = new Vector2(target.position.x, target.position.y);

        // move sprite towards the target location
        transform.position = Vector2.MoveTowards(transform.position, targetPos, step);
    }

    public void SpawnExplosion()
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

    void OnCollisionEnter2D(Collision2D col)
    {
        if(col.gameObject.CompareTag("Player") || col.gameObject.CompareTag("Obstruct"))
        {
            ShakeScreen();
            SpawnExplosion();
        }
    }
}
