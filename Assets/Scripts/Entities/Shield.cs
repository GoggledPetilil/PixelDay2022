using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    [SerializeField] private ParticleSystem m_Explosion;

    void DestroySelf()
    {
        m_Explosion.Play();
        this.gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = col.gameObject.GetComponent<Enemy>();
            if(enemy != null) enemy.Death();
            DestroySelf();
        }
        else if(col.gameObject.CompareTag("EnemyBullet"))
        {
            EnemyBullet enemy = col.gameObject.GetComponent<EnemyBullet>();
            if(enemy != null) enemy.SpawnExplosion();
            DestroySelf();
        }
    }
}
