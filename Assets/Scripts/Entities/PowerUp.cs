using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum Power
    {
        Rapid,
        Spread,
        Bounce,
        Shield,
    }

    public Power m_Power;

    void DestroySelf()
    {
        Destroy(this.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
          Player p = collision.gameObject.GetComponent<Player>();

          switch (m_Power)
          {
              case Power.Rapid:
                p.EnableRapidFire();
                break;
              case Power.Spread:
                p.EnableSpreader();
                break;
              case Power.Bounce:
                p.EnableBounce();
                break;
              case Power.Shield:
                p.EnableShield();
                break;
          }

          DestroySelf();
        }
    }
}
