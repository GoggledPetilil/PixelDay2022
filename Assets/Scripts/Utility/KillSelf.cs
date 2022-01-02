using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillSelf : MonoBehaviour
{
    [SerializeField] private float m_Time;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(this.gameObject, m_Time);
    }
}
