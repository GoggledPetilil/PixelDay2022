using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("Parameters")]
    public int m_MaxHP;
    public int m_HP;
    public int m_Speed;
    public int m_JumpForce;
    protected bool m_Flipped;

    [Header("Move Physics")]
    public Vector2 m_MovDir;
    public Vector2 m_FacingDir;
    public bool m_CanMove = true;
    public PhysicsMaterial2D m_BounceMaterial;

    [Header("Jump Parameters")]
    public float m_JumpDelay = 0.25f;
    protected float m_JumpTimer;

    [Header("Jump Physics")]
    public bool m_OnGround = false;
    public float m_LinearDrag = 4f;
    public float m_Gravity = 1f;
    public float m_FallMulti = 5f;
    public Vector3 m_CollOffset;

    [Header("Flip Parameters")]
    public bool m_CanFlip;
    public float m_TurnFlipDur;
    private bool m_IsFacingLeft;

    [Header("Components")]
    [SerializeField] protected Animator m_Anim;
    [SerializeField] protected AudioSource m_Audio;
    [SerializeField] protected AudioSource m_DeathAudio;
    [SerializeField] protected Rigidbody2D m_Body;
    [SerializeField] protected GameObject m_Sprite;
    [SerializeField] protected LayerMask m_GroundLayer;

    [Header("Particles")]
    [SerializeField] private ParticleSystem m_WalkDust;
    [SerializeField] protected ParticleSystem m_LandDust;

    [Header("Sound Effects")]
    [SerializeField] protected AudioClip m_JumpSound;
    [SerializeField] protected AudioClip m_LandSound;
    [SerializeField] protected AudioClip m_BumpSound;
    [SerializeField] protected AudioClip m_DeathSound;

    public virtual void ReduceHealth(int amount)
    {
        m_HP = Mathf.Clamp(m_HP - amount, 0, m_MaxHP);
    }

    protected void CreateWalkDust()
    {
        m_WalkDust.Play();
    }

    protected void StopWalkDust()
    {
        m_WalkDust.Stop();
    }

    protected void CreateLandDust()
    {
        m_LandDust.Play();
    }

    public void Damaged(int value, float force, Transform source)
    {
        ReduceHealth(value);

        StartCoroutine(Squeeze(0.5f, 1.2f, 0.15f));
        if(m_HP <= 0)
        {
            Death();
        }
    }

    public virtual void Death()
    {
        // Just so it can get called here.
    }

    protected void PlayAudio(AudioClip clip)
    {
        m_Audio.clip = clip;
        m_Audio.Play();
    }

    protected void PlayDeathSound()
    {
        m_DeathAudio.clip = m_DeathSound;
        m_DeathAudio.Play();
    }

    public void FreezeMovement(bool state)
    {
        if(state == true)
        {
            m_Body.constraints = RigidbodyConstraints2D.FreezeAll;
            StopWalkDust();
        }
        else
        {
            m_Body.constraints = RigidbodyConstraints2D.None;
        }
        m_Body.constraints = RigidbodyConstraints2D.FreezeRotation;
        m_MovDir = Vector2.zero;
        m_CanMove = !state;
    }

    public void CheckGround()
    {
        bool wasGrounded = m_OnGround;

        float rayLength = 0.6f;
        m_OnGround = Physics2D.Raycast(transform.position + m_CollOffset, Vector2.down, rayLength, m_GroundLayer) ||
        Physics2D.Raycast(transform.position - m_CollOffset, Vector2.down, rayLength, m_GroundLayer);

        if(!wasGrounded && m_OnGround)
        {
            StartCoroutine(Squeeze(1.5f, 0.8f, 0.05f));
            CreateLandDust();
            PlayAudio(m_LandSound);
        }
    }

    public void Jump()
    {
        StartCoroutine(Squeeze(0.5f, 1.5f, 0.05f));
        m_Body.velocity = new Vector2(m_Body.velocity.x, 0);
        m_Body.AddForce(Vector2.up * m_JumpForce, ForceMode2D.Impulse);
        m_JumpTimer = 0.0f;
        CreateLandDust();
        PlayAudio(m_JumpSound);
    }

    public void JumpPhysics()
    {
        if(m_HP < 1) return;

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

    public virtual void TurnCharacter()
    {
        m_FacingDir = m_MovDir;

        if(m_FacingDir.x < 0 && m_Flipped != true)
        {
            FlipSprite(true);
        }
        else if(m_FacingDir.x > 0 && m_Flipped != false)
        {
            FlipSprite(false);
        }
    }

    public void FlipSprite(bool state)
    {
        float startY = 180.0f;
        float endY = 0.0f;
        if(state == true)
        {
            startY = 0.0f;
            endY = 180.0f;
        }

        m_Flipped = state;
        if(m_CanFlip == false) return;
        
        StopCoroutine(FlipCharacter(startY, endY));
        StartCoroutine(FlipCharacter(startY, endY));
    }

    protected Vector2 TurnTo(Transform obj)
    {
        Vector2 diff = (transform.position - obj.position).normalized;
        Vector2 turn = new Vector2(Mathf.Round(diff.x / 0.5f) * 0.5f, Mathf.Round(diff.y / 0.5f) * 0.5f);

        Vector2 newVector = turn;
        return newVector;
    }

    public void ShakeSprite(float power)
    {
        Transform spriteHolder = m_Sprite.transform.parent;
        float offSetX = Random.Range(-1f, 1f) * power;
        float offSetY = Random.Range(-1f, 1f) * power;

        spriteHolder.localPosition = new Vector2(0.0f + offSetX, 0.0f + offSetY);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Obstruct") && m_HP < 1)
        {
            PlayAudio(m_BumpSound);
        }
    }

    public IEnumerator Squeeze(float squeezeX, float squeezeY, float sec)
    {
        Vector3 originSize = Vector3.one;
        Vector3 newSize = new Vector3(squeezeX, squeezeY, originSize.z);
        float t = 0f;
        while (t <= 1f)
        {
            t += Time.deltaTime / sec;
            m_Sprite.transform.localScale = Vector3.Lerp(originSize, newSize, t);
            yield return null;
        }
        t = 0f;
        while (t <= 1f)
        {
            t += Time.deltaTime / sec;
            m_Sprite.transform.localScale = Vector3.Lerp(newSize, originSize, t);
            yield return null;
        }
    }

    public IEnumerator FlipCharacter(float startRot, float endRot)
    {
        float startY = startRot;
        float endY = endRot;
        float t = 0.0f;
        while(t < 1f)
        {
            t += Time.deltaTime / m_TurnFlipDur;
            float rotY = Mathf.Lerp(startY, endY, t);
            m_Sprite.transform.rotation = Quaternion.Euler(m_Sprite.transform.rotation.x, rotY, m_Sprite.transform.rotation.z);
            yield return null;
        }

        yield return null;
    }
}
