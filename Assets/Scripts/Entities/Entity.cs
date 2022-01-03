using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("Parameters")]
    public int m_MaxHP;
    public int m_HP;
    public int m_Attack;
    public int m_Speed;
    public int m_JumpForce;

    [Header("Knockback Parameters")]
    public Vector2 m_ShoveDir;
    public float m_ShoveForce; // How far this go pushes others.
    protected float m_ShoveSpeed; // How far this go is pushed back.
    protected float m_ShoveLeft;

    [Header("Move Physics")]
    public Vector2 m_MovDir;
    public Vector2 m_FacingDir;
    public bool m_CanMove = true;
    public PhysicsMaterial2D m_BounceMaterial;

    [Header("Jump Physics")]
    public bool m_OnGround = false;
    public float m_LinearDrag = 4f;
    public float m_Gravity = 1f;
    public float m_FallMulti = 5f;
    public Vector3 m_CollOffset;

    [Header("Components")]
    [SerializeField] protected Animator m_Anim;
    [SerializeField] protected AudioSource m_Audio;
    [SerializeField] protected Rigidbody2D m_Body;
    [SerializeField] protected SpriteRenderer m_Sprite;
    [SerializeField] protected LayerMask m_GroundLayer;

    [Header("Particles")]
    [SerializeField] private ParticleSystem m_WalkDust;
    [SerializeField] private ParticleSystem m_LandDust;

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

        //CreateDust();
        m_ShoveLeft = 0.2f;
        m_ShoveSpeed = force;
        //m_ShoveDir = TurnTo(source);
        m_FacingDir = new Vector2(Mathf.Round(-m_ShoveDir.x / 0.5f) * 0.5f, Mathf.Round(-m_ShoveDir.y / 0.5f) * 0.5f);

        StartCoroutine(Squeeze(0.5f, 1.2f, 0.15f));
        //EffectsManager.instance.SpawnParentEffect(EffectsManager.instance.m_BloodParticles, transform);
        //EffectsManager.instance.SpawnBloodStain(transform.position);
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
        }
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
}
