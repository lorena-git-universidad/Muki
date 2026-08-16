using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;

    [TextArea]
    public string description;

    [Header("Visual")]
    public Sprite icon;

    [Header("World")]
    public GameObject worldPrefab;

    [Header("Inventory")]
    public int maxStack = 64;
}