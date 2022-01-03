using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void Death()
    {
        this.gameObject.tag = "Untagged";
        m_Body.sharedMaterial = m_BounceMaterial;

        m_Body.constraints = RigidbodyConstraints2D.None;

        float offsetX = Random.Range(-0.1f, 0.1f);
        float offsetY = Random.Range(-0.1f, 0.1f);
        Vector2 source = TurnTo(GameObject.FindWithTag("Player").transform);
        if(source.y < 0)
        {
            source.y = -7f;
        }
        else
        {
            source.y = 7f;
        }
        Vector2 force = new Vector2(source.x + offsetX, source.y + offsetY);

        m_Body.AddForce(force * 42f, ForceMode2D.Impulse);

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
