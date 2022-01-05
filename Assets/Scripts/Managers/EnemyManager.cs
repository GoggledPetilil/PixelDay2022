using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;

    [Header("Spawn Info")]
    public int m_MaxEnemies;
    public float m_SpawnTime;
    private float m_NextSpawn;
    public List<Enemy> m_AllEnemies = new List<Enemy>();
    public GameObject[] m_AllSpawnPoints;

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] m_EnemyPrefabs;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        m_NextSpawn = Time.time + 2f;
        m_AllSpawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
    }

    void Update()
    {
        if(Time.time > m_NextSpawn && m_AllEnemies.Count < m_MaxEnemies)
        {
            StartCoroutine("SpawnEnemy");
        }
    }

    IEnumerator SpawnEnemy()
    {
        m_NextSpawn = Time.time + (m_SpawnTime * Random.Range(1.0f, 1.2f));

        int re = Random.Range(0, m_EnemyPrefabs.Length);
        int rs = Random.Range(0, m_AllSpawnPoints.Length);
        Vector2 pos = m_AllSpawnPoints[rs].transform.position;

        m_AllSpawnPoints[rs].GetComponent<Animator>().SetTrigger("Spawn");
        yield return new WaitForSeconds(0.4f);
        Instantiate(m_EnemyPrefabs[re], pos, Quaternion.identity);

        yield return null;
    }
}
