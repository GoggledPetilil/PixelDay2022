using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    public static EffectsManager instance;

    [Header("Particle Effects")]
    public GameObject m_Explosion;

    void Awake()
    {
        instance = this;
    }

    public void SpawnPosEffect(GameObject effect, Vector3 pos)
    {
        Instantiate(effect, pos, Quaternion.identity);
    }

    public void SpawnParentEffect(GameObject effect, Transform parent)
    {
        Instantiate(effect, parent);
    }
}
