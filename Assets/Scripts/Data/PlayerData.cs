using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int money;
    public float masterVolume;
    public float musicVolume;
    public float soundVolume;
    public int qualityLevel;
    public List<int> itemIDs;
    public int equipID;
    public int highScore;
    public float shakePow;
    public float corpseDur;
    public List<int> medalIDs;

    public PlayerData(GameManager data)
    {
        money = data.m_Money;
        masterVolume = data.m_MasterVolume;
        musicVolume = data.m_MusicVolume;
        soundVolume = data.m_SoundVolume;
        qualityLevel = data.m_QualityLevel;
        itemIDs = data.m_ItemIDs;
        equipID = data.m_EquipID;
        highScore = data.m_HighScore;
        shakePow = data.m_ShakePower;
        corpseDur = data.m_CorpseDuration;
        medalIDs = data.m_MedalIDs;
    }
}
