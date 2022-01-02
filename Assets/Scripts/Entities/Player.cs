using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
    public enum EntityState
    {
        Idle,
        Attacking,
        Dashing,
        Shoved,
        Super,
        Braking
    }

    [Header("Overview")]
    public EntityState m_State;
    public float m_Money;
    public float m_InvincibilityTime; // How long until you're vulnerable again.
    private float m_NextVulnerable;
    private Enemy m_LastHit;
    private bool m_Died;

    [Header("Shoot Parameters")]
    public GameObject m_BulletPrefab;
    public GameObject m_SoundWord;
    public bool m_Strafing;
    public float m_ShootCooldown;
    private float m_NextShot;

    [Header("Dash Parameters")]
    public float m_DashTime;
    public float m_DashSpeed;
    public float m_ImageDistance;
    public float m_DashCooldown;
    private float m_DashLeft;
    private Vector2 m_LastImageCord;
    private float m_NextDash;

    [Header("Super Dash Parameters")]
    public float m_SuperSpeed;
    [SerializeField] private Color m_FlashColor;
    [SerializeField] private float m_FlashSpeed;
    [SerializeField] private GameObject m_SuperDashEffect;
    private Color m_DefaultColor;
    private Color m_StartColor;
    private Color m_EndColor;
    private float m_FlashTimer;

    [Header("Brake Parameters")]
    public float m_BrakeSpeed;
    public float m_BrakeTime;
    private float m_BrakeLeft;

    [Header("Components")]
    [SerializeField] private AudioSource m_Voice;
    [SerializeField] private PlayerHealth m_HealthBar;
    [SerializeField] private MoneySum m_MoneyDisplay;
    [SerializeField] private PowerBar m_PowerBar;
    [SerializeField] private Transform m_Light;

    [Header("Sound Clips")]
    [SerializeField] private AudioClip m_DashSFX;
    [SerializeField] private AudioClip m_HurtSFX;
    [SerializeField] private AudioClip m_SuperDashSFX;
    [SerializeField] private AudioClip m_BrakeSFX;
    [SerializeField] private AudioClip[] m_HappyClips;
    [SerializeField] private AudioClip[] m_HurtClips;
    [SerializeField] private AudioClip[] m_BadlyHurtClips;

    void Awake()
    {
        m_Body = GetComponent<Rigidbody2D>();

        m_HealthBar.m_Player = this;
        m_PowerBar.m_Player = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_HealthBar.SetHealth();

        m_DefaultColor = m_Sprite.color;
        m_StartColor = m_DefaultColor;
        m_EndColor = m_FlashColor;
        m_SuperDashEffect.SetActive(false);

        m_FacingDir = Vector2.down; // Prevents the facing dir from being 0.
        m_Anim.SetFloat("Horizontal", m_FacingDir.x);
        m_Anim.SetFloat("Vertical", m_FacingDir.y);

        m_CanMove = true;
        IgnoreZombies(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(PauseManager.instance.m_IsPaused) return;

        switch (m_State)
        {
            case EntityState.Idle:
              // Movement
              Move();

              // Actions
              if(Input.GetKeyDown(KeyCode.X) && (Time.time > m_NextDash) && m_CanMove)
              {
                  m_Anim.SetTrigger("Dashing");
                  PlayAudio(m_DashSFX);
                  CreateDust();
                  AfterImagePool.instance.GetFromPool();
                  m_LastImageCord = m_Sprite.transform.position;
                  m_DashLeft = m_DashTime;
                  IgnoreZombies(true);
                  m_NextDash = Time.time + m_DashCooldown;

                  m_State = EntityState.Dashing;
              }

              if(Input.GetKey(KeyCode.Z) && m_CanMove)
              {
                  FireBullet();
                  m_State = EntityState.Attacking;
              }

              if(Input.GetKey(KeyCode.C) && m_CanMove && m_PowerBar.m_Charged)
              {
                  AfterImagePool.instance.GetFromPool();
                  m_LastImageCord = m_Sprite.transform.position;
                  IgnoreZombies(false);
                  PlayAudio(m_SuperDashSFX);
                  CameraShake();
                  m_PowerBar.ActivatePower();
              }
              break;
            case EntityState.Dashing:
              m_CanMove = false;
              m_DashLeft -= Time.deltaTime;

              if(Mathf.Abs(Vector2.Distance(m_Sprite.transform.position, m_LastImageCord)) >= m_ImageDistance)
              {
                  AfterImagePool.instance.GetFromPool();
                  m_LastImageCord = m_Sprite.transform.position;
              }

              if(m_DashLeft <= 0.0f)
              {
                  m_CanMove = true;
                  StopDust();
                  IgnoreZombies(false);
                  if(Input.GetKey(KeyCode.LeftShift))
                  {
                      m_FacingDir = -m_FacingDir;
                      m_Anim.SetFloat("Horizontal", m_FacingDir.x);
                      m_Anim.SetFloat("Vertical", m_FacingDir.y);
                  }
                  m_State = EntityState.Idle;
                  m_Anim.SetTrigger("Idle");
              }
              break;
            case EntityState.Attacking:
              // Movement
              Move();

              // Actions
              if(Input.GetKey(KeyCode.Z))
              {
                  FireBullet();
              }
              else
              {
                  m_Strafing = false;
                  m_Anim.SetTrigger("Idle");
                  m_State = EntityState.Idle;
              }
              break;
            case EntityState.Shoved:
              m_CanMove = false;
              m_Anim.SetTrigger("Hurt");
              ShakeSprite(0.02f);
              m_ShoveLeft -= Time.deltaTime;

              if(m_ShoveLeft <= 0)
              {
                  m_CanMove = true;
                  StopDust();
                  EffectsManager.instance.SpawnBloodStain(transform.position);
                  IgnoreZombies(false);

                  m_State = EntityState.Idle;
                  m_Anim.SetTrigger("Idle");
                  ShakeSprite(0.0f);

                  m_Strafing = false;
                  m_Anim.SetFloat("Horizontal", m_FacingDir.x);
                  m_Anim.SetFloat("Vertical", m_FacingDir.y);
              }
              break;
            case EntityState.Super:
              // Movement
              Move();
              m_SuperDashEffect.transform.right = m_FacingDir;

              if(Input.GetKeyDown(KeyCode.C) && m_CanMove)
              {
                  Revert();
              }

              m_Anim.SetTrigger("Walking");
              CreateDust();

              if(Mathf.Abs(Vector2.Distance(m_Sprite.transform.position, m_LastImageCord)) >= m_ImageDistance)
              {
                  AfterImagePool.instance.GetFromPool();
                  m_LastImageCord = m_Sprite.transform.position;
              }

              m_FlashTimer += m_FlashSpeed * Time.deltaTime;

              float r = Mathf.Lerp(m_StartColor.r, m_EndColor.r, m_FlashTimer);
              float g = Mathf.Lerp(m_StartColor.g, m_EndColor.g, m_FlashTimer);
              float b = Mathf.Lerp(m_StartColor.b, m_EndColor.b, m_FlashTimer);
              m_Sprite.color = new Color(r, g, b);

              if(m_FlashTimer > 1.0f)
              {
                  Color temp = m_StartColor;
                  m_StartColor = m_EndColor;
                  m_EndColor = temp;
                  m_FlashTimer = 0.0f;
              }
              break;
            case EntityState.Braking:
              m_CanMove = false;
              m_Anim.SetTrigger("Braking");
              ShakeSprite(0.02f);
              if(Mathf.Abs(Vector2.Distance(m_Sprite.transform.position, m_LastImageCord)) >= m_ImageDistance)
              {
                  AfterImagePool.instance.GetFromPool();
                  m_LastImageCord = m_Sprite.transform.position;
              }
              m_BrakeLeft -= Time.deltaTime;

              if(m_BrakeLeft <= 0)
              {
                  m_CanMove = true;
                  StopDust();
                  ShakeSprite(0.0f);
                  m_Anim.SetTrigger("Idle");
                  m_State = EntityState.Idle;
              }
              break;
        }
    }

    void FixedUpdate()
    {
        switch (m_State)
        {
            case EntityState.Idle:
              float mod = 1f;
              if(m_Strafing)
              {
                  mod = 0.5f;
              }
              m_Body.velocity = m_MovDir * (m_Speed * mod);
              break;
            case EntityState.Attacking:
              m_Body.velocity = m_MovDir * (m_Speed * 0.5f);
              break;
            case EntityState.Dashing:
              m_Body.velocity = m_FacingDir * m_DashSpeed;
              break;
            case EntityState.Shoved:
              m_Body.velocity = m_ShoveDir * m_ShoveSpeed;
              break;
            case EntityState.Super:
              if(m_CanMove)
                m_Body.velocity = m_FacingDir * m_SuperSpeed;
              break;
            case EntityState.Braking:
              m_Body.velocity = m_FacingDir * (m_BrakeSpeed * (m_BrakeLeft / m_BrakeTime));
              break;
        }
    }

    void Move()
    {
        if(m_CanMove)
        {
            m_Strafing = Input.GetKey(KeyCode.LeftShift);
            m_MovDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

            if(m_MovDir.x != 0f || m_MovDir.y != 0f  && m_CanMove)
            {
                m_Anim.SetTrigger("Walking");
                if(!m_Strafing)
                {
                    m_FacingDir = m_MovDir;
                }
            }
            else
            {
                m_Anim.SetTrigger("Idle");
            }

            if(!m_Strafing)
            {
                m_Anim.SetFloat("Horizontal", m_FacingDir.x);
                m_Anim.SetFloat("Vertical", m_FacingDir.y);
                m_Light.up = -m_FacingDir;
            }
        }
    }

    void FireBullet()
    {
        if(Time.time >= m_NextShot)
        {
            StartCoroutine(Squeeze(0.5f, 1.2f, 0.05f));
            Instantiate(m_SoundWord, transform.position, Quaternion.identity);

            Vector2 pos = new Vector2(transform.position.x + m_FacingDir.x, transform.position.y + m_FacingDir.y);
            GameObject obj = Instantiate(m_BulletPrefab, pos, Quaternion.identity) as GameObject;

            obj.transform.right = m_FacingDir;

            m_NextShot = Time.time + m_ShootCooldown;
        }
    }

    public void ChangeMoney(float addition)
    {
        m_Money += addition;
        m_MoneyDisplay.ChangeMoney(addition);
    }

    public void ChangePower(float addition)
    {
        m_PowerBar.IncreasePower(addition);
    }

    public void SuperDashActivate()
    {
        m_State = Player.EntityState.Super;
        m_SuperDashEffect.SetActive(true);
        PlayHappyVoice();
    }

    public void DePower()
    {
        if(m_State != EntityState.Super) return;

        m_SuperDashEffect.SetActive(false);
        m_Sprite.color = m_DefaultColor;
        m_StartColor = m_DefaultColor;
        m_EndColor = m_FlashColor;
        m_FlashTimer = 0.0f;

        PlayAudio(m_BrakeSFX);
        m_BrakeLeft = m_BrakeTime;
        m_State = EntityState.Braking;
    }

    public void Revert()
    {
        if(m_State != EntityState.Super) return;

        m_PowerBar.Revert();
    }

    private void IgnoreZombies(bool state)
    {
        Physics2D.IgnoreLayerCollision(6, 9, state);
    }

    public void PlayHappyVoice()
    {
        PlayVoice(m_HappyClips[Random.Range(0, m_HappyClips.Length)]);
    }

    public void PlayHurtVoice()
    {
        if(m_HP / m_MaxHP <= 0.25)
        {
            int i = Random.Range(0, 3);
            if(i == 0 && m_HP > 0)
            {
                PlayVoice(m_HurtClips[Random.Range(0, m_HurtClips.Length)]);
            }
            else
            {
                PlayVoice(m_BadlyHurtClips[Random.Range(0, m_BadlyHurtClips.Length)]);
            }
        }
        else
        {
            PlayVoice(m_HurtClips[Random.Range(0, m_HurtClips.Length)]);
        }
    }

    public void PlayVoice(AudioClip clip)
    {
        if(m_Voice.isPlaying == false)
        {
            m_Voice.clip = clip;
            m_Voice.pitch = Random.Range(0.9f, 1.1f);
            m_Voice.Play();
        }
    }

    public void RaiseHealth(int amount)
    {
        m_HP = Mathf.Clamp(m_HP + amount, 0, m_MaxHP);
        m_HealthBar.SetHealth();
    }

    public override void ReduceHealth(int amount)
    {
        m_HP = Mathf.Clamp(m_HP - amount, 0, m_MaxHP);
        m_HealthBar.SetHealth();
    }

    public override void Death()
    {
        EffectsManager.instance.SpawnParentEffect(EffectsManager.instance.m_DeathBlood, this.transform);
        if(m_Died == false)
        {
            m_Died = true;
            GameManager.instance.m_PlayerHasDied = true;

            IgnoreZombies(true);
            PlayHurtVoice();

            DungeonManager.instance.GameOver();
        }
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
            if(m_Died) return;

            Enemy e = collision.gameObject.GetComponent<Enemy>();
            if(m_State != EntityState.Super)
            {
                // Check if this enemy can hit Player.
                if(e == m_LastHit && Time.time < m_NextVulnerable) return;

                // Disable actions for a bit.
                m_Strafing = false;
                m_NextShot = Time.time + m_ShootCooldown;
                IgnoreZombies(true);
                m_NextVulnerable = Time.time + m_InvincibilityTime;

                // Do all the usual damage shit.
                int damage = e.m_Attack;
                float force = e.m_ShoveForce;
                Transform source = e.transform;
                Damaged(damage, force, source);
                m_LastHit = e;

                // Do more effect shit here.
                m_State = EntityState.Shoved;
                m_PowerBar.IncreasePower(0.05f);
                PlayHurtVoice();
                GameManager.instance.PlayOneShot(m_HurtSFX);
            }
            else
            {
                e.Damaged(e.m_MaxHP, m_ShoveForce, transform);
            }
            CameraShake();
        }
        else if(collision.gameObject.CompareTag("Obstruct"))
        {
            m_ShoveLeft = 0.0f;
            m_DashLeft = 0.0f;
            m_BrakeLeft = 0.0f;
        }
    }
}
