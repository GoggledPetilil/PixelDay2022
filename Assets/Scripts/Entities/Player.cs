using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
    public enum EntityState
    {
        Idle,
        Jumping,
    }

    [Header("Overview")]
    public EntityState m_State;
    private bool m_Died;

    [Header("Shoot Parameters")]
    public GameObject m_BulletPrefab;
    public float m_ShootCooldown;
    public Transform m_BulletPoint;
    private float m_NextShot;

    [Header("Components")]
    [SerializeField] private Animator m_BodyAnim;
    [SerializeField] private SpriteRenderer m_BodySprite;
    [SerializeField] private SpriteRenderer m_HatSprite;
    [SerializeField] private ParticleSystem m_AfterImage;
    [SerializeField] private ParticleSystem m_SelfExplosion;
    [SerializeField] private GameObject m_Gun;
    [SerializeField] private SpriteRenderer m_GunSprite;
    [SerializeField] private Animator m_GunAnim;

    [Header("Tutorial Assets")]
    [SerializeField] private GameObject m_TutorialObject;
    private bool moved;
    private bool shot;
    private bool jumped;
    private bool tutorialDone;

    void Awake()
    {
        m_Body = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        EquipHat();

        m_FacingDir = Vector2.right; // Prevents the facing dir from being 0.
        m_Gun.transform.localPosition = m_FacingDir;
        m_Gun.transform.right = m_FacingDir;
        m_SelfExplosion.gameObject.SetActive(false);
        m_TutorialObject.SetActive(true);

        m_CanMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(PauseManager.instance.m_IsPaused) return;

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
                  if(!shot) shot = true;
                  FireBullet();
              }

              // Jumping
              if(Input.GetKeyDown(KeyCode.X) && m_CanMove)
              {
                  if(!jumped) jumped = true;
                  m_JumpTimer = Time.time + m_JumpDelay;
              }
              break;
        }

        if(!tutorialDone && moved && shot && jumped)
        {
            m_TutorialObject.SetActive(false);
            tutorialDone = true;
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
            if(!moved) moved = true;
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

    void FireBullet()
    {
        if(Time.time >= m_NextShot)
        {
            m_GunAnim.SetTrigger("Fire");

            Vector2 pos = new Vector2(m_BulletPoint.position.x, m_BulletPoint.position.y);
            GameObject obj = Instantiate(m_BulletPrefab, pos, Quaternion.identity) as GameObject;

            obj.transform.right = m_FacingDir;

            m_NextShot = Time.time + m_ShootCooldown;
        }
    }

    public override void TurnCharacter()
    {
        base.TurnCharacter();

        m_BodyAnim.SetFloat("LookDir", m_FacingDir.y);

        if(m_Flipped == true && m_FacingDir.y != 0.0f)
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

    void CameraShake()
    {
        float m_shakeDur = 0.2f;
        float m_shakeMag = 0.8f;
        float m_shakePow = 0.9f;
        CameraManager.instance.ShakeCamera(m_shakeDur, m_shakeMag, m_shakePow);
    }

    void EquipHat()
    {
        Accessory hat = GameManager.instance.m_EquipedHat;
        if(hat == null)
        {
            m_HatSprite.gameObject.SetActive(false);
        }
        else
        {
            m_HatSprite.sprite = hat.sprite;
            m_HatSprite.transform.position = hat.offset;
            m_HatSprite.gameObject.SetActive(true);
        }
    }

    public override void Death()
    {
        // Stop Gameplay
        CameraManager.instance.ActivateDeathCam();
        GameplayManager.instance.TimerActive(false);
        GameplayManager.instance.StopMusic();
        ComboManager.instance.BreakCombo();
        EnemyManager.instance.DeleteAllEnemies();
        EnemyManager.instance.m_StopSpawning = true;
        PlayDeathSound();

        // Player death state
        m_CanMove = false;
        this.gameObject.layer = 9;
        foreach(Transform trans in gameObject.GetComponentsInChildren<Transform>())
        {
            trans.gameObject.layer = 9;
        }
        m_Gun.SetActive(false);

        // "Ragdoll"
        m_Body.sharedMaterial = m_BounceMaterial;
        m_Body.constraints = RigidbodyConstraints2D.None;
        m_Body.gravityScale = 1f;

        // Blast Off
        float offsetX = Random.Range(-0.1f, 0.1f);
        float offsetY = Random.Range(-0.1f, 0.1f);
        Vector2 force = new Vector2(-m_FacingDir.x + offsetX, 0.5f + offsetY);
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, Random.Range(10.0f, 45.0f));
        CreateWalkDust();
        m_Body.AddForce(force * 42f, ForceMode2D.Impulse);

        // Die
        Invoke("GameOver", 3f);
    }

    void GameOver()
    {
        m_Sprite.SetActive(false);
        m_AfterImage.Stop();
        m_LandDust.gameObject.SetActive(false);
        m_SelfExplosion.gameObject.SetActive(true);
        m_SelfExplosion.Play();
        m_Audio.volume = 0.0f;

        GameplayManager.instance.GameOver();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Enemy") && m_HP > 0)
        {
            m_HP = 0;
            Death();
        }
    }
}
