using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
    public enum EntityState
    {
        Idle,
        Attacking,
        Jumping,
    }

    [Header("Overview")]
    public EntityState m_State;
    public Color m_Color;
    public int m_Money;
    public float m_InvincibilityTime; // How long until you're vulnerable again.
    private float m_NextVulnerable;
    private bool m_Died;

    [Header("Shoot Parameters")]
    public GameObject m_BulletPrefab;
    public float m_ShootCooldown;
    public Transform m_BulletPoint;
    private float m_NextShot;

    [Header("Jump Parameters")]
    public float m_JumpDelay = 0.25f;
    private float m_JumpTimer;

    [Header("Components")]
    [SerializeField] private ParticleSystem m_AfterImage;
    [SerializeField] private GameObject m_Gun;
    [SerializeField] private SpriteRenderer m_GunSprite;
    [SerializeField] private Animator m_GunAnim;

    [Header("Sound Clips")]
    [SerializeField] private AudioClip m_HurtSFX;

    void Awake()
    {
        m_Body = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        m_FacingDir = Vector2.right; // Prevents the facing dir from being 0.
        m_Gun.transform.localPosition = m_FacingDir;
        m_Gun.transform.right = m_FacingDir;

        m_CanMove = true;

        m_Sprite.color = m_Color;
        m_AfterImage.startColor = m_Color;
    }

    // Update is called once per frame
    void Update()
    {
        //if(PauseManager.instance.m_IsPaused) return;

        CheckGround();
        JumpPhysics();

        switch (m_State)
        {
            case EntityState.Idle:
              // Movement
              Move();

              // Firing
              if(Input.GetKey(KeyCode.Z) && m_CanMove)
              {
                  FireBullet();
              }

              // Jumping
              if(Input.GetKeyDown(KeyCode.X) && m_CanMove)
              {
                  m_JumpTimer = Time.time + m_JumpDelay;
              }
              break;
        }
    }

    void FixedUpdate()
    {
        if(m_JumpTimer > Time.time && m_OnGround)
        {
            Jump();
        }

        switch (m_State)
        {
            case EntityState.Idle:
              m_Body.velocity = new Vector2(m_MovDir.x * (m_Speed), m_Body.velocity.y);
              break;
        }
    }

    void Move()
    {
        if(m_CanMove)
        {
            m_MovDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

            if((m_MovDir.x != 0f || m_MovDir.y != 0f) && m_CanMove)
            {
                TurnCharacter();
            }

            if(m_Body.velocity.x != 0.0f && m_Body.velocity.y == 0.0f)
            {
                CreateWalkDust();
            }
            else
            {
                StopWalkDust();
            }
        }
    }

    void Jump()
    {
        StartCoroutine(Squeeze(0.5f, 1.5f, 0.05f));
        m_Body.velocity = new Vector2(m_Body.velocity.x, 0);
        m_Body.AddForce(Vector2.up * m_JumpForce, ForceMode2D.Impulse);
        m_JumpTimer = 0.0f;
        CreateLandDust();
    }

    public void JumpPhysics()
    {
        if(m_OnGround)
        {
            m_Body.gravityScale = 0f;
        }
        else
        {
            m_Body.gravityScale = m_Gravity;
            m_Body.drag = m_LinearDrag * 0.15f;
            if(m_Body.velocity.y < 0)
            {
                m_Body.gravityScale = m_Gravity * m_FallMulti;
            }
            else if(m_Body.velocity.y > 0 && !Input.GetKey(KeyCode.X))
            {
                m_Body.gravityScale = m_Gravity * (m_FallMulti / 2);
            }
        }
    }

    void FireBullet()
    {
        if(Time.time >= m_NextShot)
        {
            m_GunAnim.SetTrigger("Fire");

            Vector2 pos = new Vector2(m_BulletPoint.position.x, m_BulletPoint.position.y);
            GameObject obj = Instantiate(m_BulletPrefab, pos, Quaternion.identity) as GameObject;

            obj.transform.right = m_FacingDir;
            //obj.GetComponent<PlayerBullet>().m_Color = m_Color;

            m_NextShot = Time.time + m_ShootCooldown;
        }
    }

    void TurnCharacter()
    {
        m_FacingDir = m_MovDir;

        if(m_FacingDir.x < 0)
        {
            m_Sprite.flipX = true;
        }
        else if(m_FacingDir.x > 0)
        {
            m_Sprite.flipX = false;
        }

        if(m_Sprite.flipX == true && m_FacingDir.y != 0.0f)
        {
            m_GunSprite.flipY = true;
        }
        else
        {
            m_GunSprite.flipY = false;
        }

        m_Gun.transform.localPosition = m_FacingDir;
        m_Gun.transform.right = m_FacingDir;
    }

    public void ChangeMoney(int addition)
    {
        m_Money += addition;
    }

    public override void Death()
    {

    }

    void CameraShake()
    {
        float m_shakeDur = 0.2f;
        float m_shakeMag = 0.8f;
        float m_shakePow = 0.9f;
        CameraManager.instance.ShakeCamera(m_shakeDur, m_shakeMag, m_shakePow);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Enemy"))
        {
            Death();
        }
    }
}
