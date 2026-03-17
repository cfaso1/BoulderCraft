using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class InventorySlot : MonoBehaviour
{
    public Image iconImage;
    public Image backgroundImage;
    public TextMeshProUGUI itemNameText;

    public void Setup(ItemData item)
    {
        iconImage.sprite = item.icon;
        backgroundImage.color = new Color(item.primaryColor.r*0.3f, item.primaryColor.g * 0.3f, item.primaryColor.b * 0.3f, 0.5f);
        itemNameText.text = item.itemName;
    }
}
