using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Enemy : Entity
{
    public enum EntityState
    {
        Idle,
        Jumping,
    }

    [Header("Overview")]
    public EntityState m_State;
    public bool m_CanFly;

    [Header("Enemy Parameters")]
    public float m_JumpDistance; // Distance until enemy jumps.
    public float m_JumpRaycast; // Length of the raycast to check for ceilings.

    [Header("Pathfinding")]
    public Transform m_Target;
    private Vector2 m_TargetPos;
    private Vector2 m_TargetOffset;
    public float m_NextWaypointDistance = 3f;
    private Path m_Path;
    private int m_CurrentWaypoint = 0;
    private bool m_ReachedEndPath = false;
    Seeker m_Seeker;

    void Awake()
    {
        m_Seeker = GetComponent<Seeker>();
    }

    void Start()
    {
        EnemyManager.instance.m_AllEnemies.Add(this);
        m_Sprite.transform.localScale = Vector3.zero;

        m_Target = GameObject.FindWithTag("Player").transform;
        InvokeRepeating("UpdatePath", 0f, 0.5f);

        StartCoroutine("SpawnAnimation");
    }

    void Update()
    {
        if(PauseManager.instance.m_IsPaused) return;

        CheckGround();
        JumpPhysics();

        switch (m_State)
        {
            case EntityState.Idle:
              // Movement
              MovementLogic();

              //Jump Check
              float distToPlayer = Vector2.Distance(m_Body.position, m_TargetPos);
              RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, m_JumpRaycast, m_GroundLayer);
              if(m_CanMove && !m_CanFly && distToPlayer < m_JumpDistance && m_TargetPos.y > transform.position.y && hit.collider == null)
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
              Movement();
              break;
        }
    }

    void Movement()
    {
        if(m_CanMove == false || m_Path == null) return;

        // Check if completed waypoint
        if(m_CurrentWaypoint >= m_Path.vectorPath.Count - 1)
        {
            m_ReachedEndPath = true;
            return;
        }
        else
        {
            m_ReachedEndPath = false;
        }

        // Actual Movement
        if(m_CanFly)
        {
            m_Body.velocity = m_MovDir * m_Speed;
        }
        else
        {
            m_Body.velocity = new Vector2(m_MovDir.x * (m_Speed), m_Body.velocity.y);
        }

        // Check if reached current Waypoint
        float distance = Vector2.Distance(m_Body.position, m_Path.vectorPath[m_CurrentWaypoint]);
        if(distance < m_NextWaypointDistance)
        {
            m_CurrentWaypoint++;
        }
    }

    void MovementLogic()
    {
        if(m_Path == null)
            return;

        // This just changes graphics mainly, make it look pretty.
        if(m_CanMove)
        {
            m_MovDir = ((Vector2)m_Path.vectorPath[m_CurrentWaypoint] - m_Body.position).normalized;

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

    void UpdatePath()
    {
        if(m_Seeker.IsDone())
        {
            m_TargetPos = m_Target.position + (Vector3)m_TargetOffset;
            m_Seeker.StartPath(m_Body.position, m_TargetPos, OnPathComplete);
        }
    }

    void OnPathComplete(Path p)
    {
        if(!p.error)
        {
            m_Path = p;
            m_CurrentWaypoint = 1;
        }
    }

    public override void Death()
    {
        // Not harmful anymore
        EnemyManager.instance.m_AllEnemies.Remove(this);
        this.gameObject.tag = "Untagged";
        m_CanMove = false;
        gameObject.layer = 8; // This is now on layer DeadEntity

        // "Ragdoll"
        m_Body.sharedMaterial = m_BounceMaterial;
        m_Body.constraints = RigidbodyConstraints2D.None;
        m_Body.gravityScale = 1f;

        // Blast Off
        float offsetX = Random.Range(-0.1f, 0.1f);
        float offsetY = Random.Range(-0.1f, 0.1f);
        Vector2 source = TurnTo(GameObject.FindWithTag("Player").transform);
        if(source.y < 0)
        {
            source.y = -0.5f;
        }
        else
        {
            source.y = 0.5f;
        }
        source.y = source.y * Random.Range(1.0f, 2.0f);
        Vector2 force = new Vector2(source.x + offsetX, source.y + offsetY);
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, Random.Range(10.0f, 45.0f));
        CreateWalkDust();
        m_Body.AddForce(force * 42f, ForceMode2D.Impulse);

        // Die
        Invoke("Disappear", 5f);
    }

    void Disappear()
    {
        StartCoroutine("DeathAnimation");
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.CompareTag("Bullet"))
        {
            if(m_HP < 1) return;

            ComboManager.instance.IncreaseCombo();

            ReduceHealth(1);
            if(m_HP < 1)
            {
                Death();
            }
        }
    }

    IEnumerator SpawnAnimation()
    {
        yield return null;
        BoxCollider2D col = GetComponent<BoxCollider2D>();

        FreezeMovement(true);
        m_Body.gravityScale = 0f;
        col.enabled = false;

        Vector3 startSize = new Vector3(0.0f, 0.0f, 1.0f);
        Vector3 endSize = Vector3.one;
        float t = 0f;
        float spawnSpeed = 0.2f;
        while(t < 1f)
        {
            t += Time.deltaTime / spawnSpeed;
            m_Sprite.transform.localScale = Vector3.Lerp(startSize, endSize, t);
            yield return null;
        }

        col.enabled = true;
        m_Body.gravityScale = m_Gravity;
        FreezeMovement(false);

        yield return null;
    }

    IEnumerator DeathAnimation()
    {
        yield return null;
        Vector3 originSize = Vector3.one;
        Vector3 newSize = new Vector3(0.0f, 0.0f, originSize.z);
        float t = 0f;
        float deathSpeed = 1f;
        while (t <= 1f)
        {
            t += Time.deltaTime / deathSpeed;
            m_Sprite.transform.localScale = Vector3.Lerp(originSize, newSize, t);
            yield return null;
        }
        Destroy(this.gameObject);
        yield return null;
    }
}
