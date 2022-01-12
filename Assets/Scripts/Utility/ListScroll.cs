using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ListScroll : MonoBehaviour
{
    protected EventSystem m_EventSystem;

    [Header("List Components")]
    public int m_Value;
    public List<string> m_Options = new List<string>();
    [SerializeField] private TMP_Text m_SelectedText;
    protected int m_Selected;
    protected bool m_ValueChanged;

    [Header("Colours")]
    [SerializeField] private Color m_NormalColour;
    [SerializeField] private Color m_HighlightedColour;
    [SerializeField] private Color m_SelectedColour;

    protected void ScrollThroughList()
    {
        if(m_EventSystem == null) m_EventSystem = EventSystem.current;
        if(m_EventSystem.currentSelectedGameObject != this.gameObject)
        {
            ReturnDisplay();
            return;
        }
        else if(m_SelectedText.color == m_NormalColour)
        {
            UpdateDisplay();
        }

        if(Input.GetButtonDown("Horizontal") && Input.GetAxisRaw("Horizontal") > 0)
        {
            GameManager.instance.PlayCursorSFX();
            UpdateSelected(+1);
        }
        else if(Input.GetButtonDown("Horizontal") && Input.GetAxisRaw("Horizontal") < 0)
        {
            GameManager.instance.PlayCursorSFX();
            UpdateSelected(-1);
        }
    }

    private void UpdateSelected(int value)
    {
        m_Selected += value;
        if(m_Selected > m_Options.Count - 1)
        {
            m_Selected = 0;
        }
        else if(m_Selected < 0)
        {
            m_Selected = m_Options.Count - 1;
        }
        UpdateDisplay();
    }

    protected void UpdateDisplay()
    {
        m_SelectedText.text = m_Options[m_Selected];

        if(m_Selected == m_Value)
        {
            m_SelectedText.color = m_SelectedColour;
        }
        else
        {
            m_SelectedText.color = m_HighlightedColour;
        }
    }

    private void ReturnDisplay()
    {
        if(m_SelectedText.text != m_Options[m_Value] || m_SelectedText.color != m_NormalColour || m_Selected != m_Value)
        {
            m_SelectedText.text = m_Options[m_Value];
            m_SelectedText.color = m_NormalColour;
            m_Selected = m_Value;
        }

        if(m_ValueChanged)
        {
            m_ValueChanged = false;
        }
    }

    public virtual void SelectionEvent()
    {
        // GameManager plays sound here

        m_Value = m_Selected;
        UpdateDisplay();
    }

    public virtual void LoadSavedSettings()
    {
        // Stuff
    }

    public virtual void SaveSettings()
    {
        m_ValueChanged = false;
        GameManager.instance.SaveData();
    }
}
