using UnityEngine;
using UnityEngine.UI;


public class InventorySlot : MonoBehaviour
{
    public Image iconImage;
    public Image backgroundImage;
    public Text itemNameText;

    public void Setup(ItemData item)
    {
        iconImage.sprite = item.icon;
        backgroundImage.color = new Color(item.primaryColor.r, item.primaryColor.g, item.primaryColor.b, 0.2f);
    }
}
