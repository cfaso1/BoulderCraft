using UnityEngine;
using System.Linq;
using System.Collections.Generic;


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
        // RuntimePreviewGenerator.BackgroundColor = new Color(0, 0, 0, 0); 
        // RuntimePreviewGenerator.OrthographicMode = true; 
        //
        // foreach (var item in allItems)
        // {
        //     Debug.Log("Generating icon for: " + item.itemName);
        //     if (item.holdPrefab != null && item.icon == null)
        //     {
        //         Texture2D preview = RuntimePreviewGenerator.GenerateModelPreview(
        //         item.holdPrefab.transform, 128, 128, true
        //     );
        //         item.icon = Sprite.Create(
        //             preview,
        //             new Rect(0, 0, preview.width, preview.height),
        //             new Vector2(0.5f, 0.5f)
        //         );
        //     }
        // }
    }

    public void SortByColor()
    {
        displayedItems = allItems.OrderBy(item =>
        {
            Color.RGBToHSV(item.primaryColor, out float h, out float s, out float v);
            return h;
        }).ToList();

        RefreshUI();
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

    void RefreshUI()
    {
        InventoryUI.Instance.Refresh(displayedItems);
    }
}
