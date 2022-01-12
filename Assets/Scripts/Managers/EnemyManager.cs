using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;

    [Header("Spawn Info")]
    public bool m_StopSpawning;
    public int m_MaxEnemies;
    public float m_SpawnTime;
    private float m_NextSpawn;
    public List<Enemy> m_AllEnemies = new List<Enemy>();
    public List<Transform> m_AllSpawnPoints = new List<Transform>();

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] m_EnemyPrefabs;
    [SerializeField] private Sprite[] m_EnemyFaces;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        m_NextSpawn = Time.time + 5f;
    }

    void Update()
    {
        if(Time.time > m_NextSpawn && m_AllEnemies.Count < m_MaxEnemies && !m_StopSpawning)
        {
            StartCoroutine("SpawnEnemy");
        }
    }

    public void GetAllSpawnPoints()
    {
        foreach(GameObject point in GameObject.FindGameObjectsWithTag("Respawn"))
        {
            m_AllSpawnPoints.Add(point.transform);
        }
    }

    public void ClearSpawnPoints()
    {
        m_AllSpawnPoints.Clear();
    }

    public void DeleteAllEnemies()
    {
        foreach(Enemy enemy in m_AllEnemies)
        {
            Destroy(enemy);
        }
    }

    IEnumerator SpawnEnemy()
    {
        m_NextSpawn = Time.time + (m_SpawnTime * Random.Range(0.5f, 1.5f));

        int re = Random.Range(0, m_EnemyPrefabs.Length);
        int rs = Random.Range(0, m_AllSpawnPoints.Count - 1);
        Vector2 pos = m_AllSpawnPoints[rs].position;

        m_AllSpawnPoints[rs].GetComponent<Animator>().SetTrigger("Spawn");
        yield return new WaitForSeconds(0.4f);
        GameObject go = Instantiate(m_EnemyPrefabs[re], pos, Quaternion.identity) as GameObject;
        Sprite face = m_EnemyFaces[Random.Range(0, m_EnemyFaces.Length - 1)];
        go.GetComponent<Enemy>().UpdateFace(face);

        yield return null;
    }
}
