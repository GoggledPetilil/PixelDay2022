using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Vector2 m_StartPos;
    [SerializeField] private Vector2 m_EndPos;
    [SerializeField] private float m_TravelDuration;
    private float t;
    private Vector2 startPos;
    private Vector2 endPos;

    // Start is called before the first frame update
    void Start()
    {
        m_StartPos = transform.localPosition;
        startPos = m_StartPos;
        endPos = m_EndPos;
        t = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if(t < 1.0f)
        {
            t += Time.deltaTime / m_TravelDuration;
            transform.localPosition = Vector2.Lerp(startPos, endPos, t);
        }
        else
        {
            Vector2 tempStart = startPos;

            startPos = endPos;
            endPos = tempStart;
            t = 0.0f;
        }
    }
}
