using System.Collections.Generic; //basic unity features like input and keyCode
using System.Diagnostics;
using System.Linq;
using UnityEngine;


public class InventoryManager : MonoBehaviour
{
    public List<ItemData> allItems = new List<ItemData>();
    private List<ItemData> displayedItems = new List<ItemData>();
    public static InventoryManager Instance;


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GenerateIcons();
    }

    void GenerateIcons()
    {
           RuntimePreviewGenerator.BackgroundColor = new Color(0, 0, 0, 0); 
           RuntimePreviewGenerator.OrthographicMode = true;
           RuntimePreviewGenerator.MarkTextureNonReadable = false;


        foreach (var item in allItems)
        {
            if (item == null || item.holdPrefab == null) continue;

            // Read data directly from HoldBehavior
            HoldBehavior holdBehavior = item.holdPrefab.GetComponent<HoldBehavior>();
            if (holdBehavior != null)
            {
                UnityEngine.Debug.Log(item.itemName + " | prefab: " + item.holdPrefab.name + " | holdId: " + holdBehavior.holdId + " | type: " + holdBehavior.holdType);
            }
            if (holdBehavior != null)
            {
                // Auto set name from hold type and size
                item.itemName = holdBehavior.holdType + " - " + holdBehavior.holdSize;
                // Auto set color from hold
                item.primaryColor = holdBehavior.holdColor;
                item.colorCategory = holdBehavior.holdType.ToString();
            }

            item.icon = null;

            Texture2D preview = RuntimePreviewGenerator.GenerateModelPreview(
                item.holdPrefab.transform, 128, 128, true
            );

            if (preview != null)
            {
                item.icon = Sprite.Create(
                    preview,
                    new Rect(0, 0, preview.width, preview.height),
                    new Vector2(0.5f, 0.5f)
                );
            }
        }
    }

    public void SortByColor()
    {
        displayedItems = allItems.OrderBy(item =>
        {
            Color.RGBToHSV(item.primaryColor, out float h, out float s, out float v);
            return h;
        }).ToList();

    }

    public void SearchByName(string query)
    {
        if (string.IsNullOrEmpty(query))
            displayedItems = new List<ItemData>(allItems);
        else
        {
            displayedItems = allItems.Where(allItems => allItems.itemName.ToLower().Contains(query.ToLower())).ToList();
        }

        SortByColor();
    }

    public void GenerateIconsAndRefresh()
    {
        GenerateIcons();
    }

    public List<ItemData> GetItems()
    {
        if (displayedItems.Count == 0)
            displayedItems = new List<ItemData>(allItems);
        return displayedItems;
    }

    public List<ItemData> GetItemsByType(string type)
    {
        return allItems.Where(i =>
        i.holdPrefab != null &&
        i.holdPrefab.GetComponent<HoldBehavior>() != null &&
        i.holdPrefab.GetComponent<HoldBehavior>().holdType.ToString().ToUpper() == type.ToUpper()
    ).ToList();
    }

    public ItemData FindItemData(string holdId)
    {
        foreach (var item in allItems)
        {
            if (item?.holdPrefab == null) continue;
            var b = item.holdPrefab.GetComponent<HoldBehavior>();
            if (b != null && b.holdId == holdId) return item;
        }
        return null;
    }
}
