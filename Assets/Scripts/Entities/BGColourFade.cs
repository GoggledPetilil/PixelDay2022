using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGColourFade : MonoBehaviour
{
    public SpriteRenderer m_Sprite;
    public float m_FadeDur;
    public float m_Alpha;
    public Color[] m_Colours;
    private int currentIndex;
    private int nextIndex;
    private float t;

    void Start()
    {
        if(m_Colours.Length < 2) Destroy(this);

        nextIndex = currentIndex + 1;
    }

    void Update()
    {
        if(t < 1f)
        {
            t += Time.deltaTime / m_FadeDur;
            Color c = Color.Lerp(m_Colours[currentIndex], m_Colours[nextIndex], t);
            m_Sprite.color = new Color(c.r, c.g, c.b, m_Alpha);
        }
        else
        {
            t = 0.0f;
            UpdateIndex();
        }
    }

    void UpdateIndex()
    {
        currentIndex++;
        if(currentIndex > m_Colours.Length - 1)
        {
            currentIndex = 0;
        }

        nextIndex = currentIndex + 1;
        if(nextIndex > m_Colours.Length - 1)
        {
            nextIndex = 0;
        }
    }
}
