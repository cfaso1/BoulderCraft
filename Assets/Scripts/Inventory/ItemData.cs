using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName= "Inventory/Item")]
public class ItemData: ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public Color primaryColor;
    public string colorCaregory;
    public GameObject holdPrefab;
}