using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAccessory", menuName = "Accessory")]
public class Accessory : ScriptableObject
{
    [Header("Data")]
    public int ID;
    public new string name;
    public Sprite sprite;
    public int price;
    [TextArea(1, 3)]
    public string description;
    public Vector2 offset;
}
