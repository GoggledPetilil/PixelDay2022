using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QualityList : ListScroll
{
    void Start()
    {
        m_Selected = m_Value;
        UpdateDisplay();
    }

    void Update()
    {
        ScrollThroughList();
    }

    public override void SelectionEvent()
    {
        base.SelectionEvent();

        QualitySettings.SetQualityLevel(m_Value, false);

        if(GameManager.instance.m_QualityLevel != m_Value && m_ValueChanged == false)
        {
            m_ValueChanged = true;
            GameManager.instance.m_SettingsChangedLevel++;
        }
        else if(GameManager.instance.m_QualityLevel == m_Value)
        {
            m_ValueChanged = false;
            GameManager.instance.m_SettingsChangedLevel--;
        }
    }

    public override void LoadSavedSettings()
    {
        m_Value = GameManager.instance.m_QualityLevel;
        QualitySettings.SetQualityLevel(m_Value, false);
    }

    public override void SaveSettings()
    {
        GameManager.instance.m_QualityLevel = m_Value;
        base.SaveSettings();
    }
}
