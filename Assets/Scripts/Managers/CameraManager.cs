using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
      public static CameraManager instance;

      [Header("Camera Follow")]
      private bool m_Finished;
      private bool m_LockCam;

      [Header("Camera Shake")]
      public float m_Duration;
      public float m_Magnitude;
      public float m_Power;
      private Vector3 m_OriginPos;

      void Awake()
      {
          instance = this;
      }

      void Start()
      {
          m_OriginPos = this.transform.position;
      }


      public void LockCamera(bool state)
      {
          m_LockCam = state;
      }

      public void ShakeCamera(float duration, float magnitude, float power)
      {
          m_Duration = duration;
          m_Magnitude = magnitude;
          m_Power = power;

          LockCamera(true);
          InvokeRepeating("CamShaking", 0f, 0.005f);
          Invoke("CamStopShaking", m_Duration);
      }

      void CamShaking()
      {
          float x = Random.Range(-1f, 1f) * (m_Magnitude * m_Power);
          float y = Random.Range(-1f, 1f) * (m_Magnitude * m_Power);
          transform.position = new Vector3(m_OriginPos.x + x, m_OriginPos.y + y, m_OriginPos.z);
      }

      void CamStopShaking()
      {
          CancelInvoke("CamShaking");
          LockCamera(false);
          transform.position = m_OriginPos;
      }
}
