using UnityEngine; //gives access to basic functions like input amd KeyCode
using UnityEngine.UIElements;//UI toolkit library functions like button/label
using System.Collections.Generic;//Lists
using System.Linq;//Allows .ToList() which converts queries into lists
using System.Collections;

public class InventoryUIToolkit : MonoBehaviour //allows for event functions to be used (start()/update())
{
    public UIDocument uiDocument; //holds the UXML file for UI toolkit
    private VisualElement root; //Like the body tag in HTML
    private VisualElement grid; //what will actually hold the entire hold slots
    private Button closeButton;//stores a button to exit
    private List<Button> filterButtons = new List<Button>(); //stores different types of buttons
    private string activeFilter = "ALL";//tracks filter being used
    public static bool isHidden=true;

    IEnumerator Start()
    {
        Debug.Log("InventoryUIToolkit Start!");
        yield return null;
        root = uiDocument.rootVisualElement.Q("inventory-root");

        if (root == null)
        {
            Debug.LogError("inventory-root not found!");
            yield break;
        }
        else
        {
            Debug.Log("inventory-root found!");
        }
        grid = root.Q("inventory-grid");
        closeButton = root.Q<Button>("close-button");//searches for a button element

        closeButton.clicked += ToggleInventory;//adds toggleInventory to events for button

        // Wire up filter buttons
        foreach (var btn in root.Query<Button>(className: "filter-btn").ToList()) //looks through all buttons with the filter-btn class
        {
            filterButtons.Add(btn);
            string filter = btn.text;
            btn.clicked += () => OnFilterClicked(btn, filter);
        }

        // Start hidden
        root.style.display = DisplayStyle.None;

        // Generate icons then populate
        InventoryManager.Instance.GenerateIconsAndRefresh();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            ToggleInventory();
    }

    public void ToggleInventory()
    {
        isHidden = root.style.display == DisplayStyle.None; //is inventory hidden?
        root.style.display = isHidden ? DisplayStyle.Flex : DisplayStyle.None;//flexbox (CSS) hides if true shows if false

        UnityEngine.Cursor.lockState = isHidden ? CursorLockMode.None : CursorLockMode.Locked;
        UnityEngine.Cursor.visible = isHidden;

        if (isHidden)
            Refresh(InventoryManager.Instance.GetItems());//only refreshes grid when inventory is opened
    }

    void OnFilterClicked(Button clicked, string filter)
    {
        activeFilter = filter;

        foreach (var btn in filterButtons)
            btn.RemoveFromClassList("filter-active");
        clicked.AddToClassList("filter-active");

        var items = filter == "ALL"
            ? InventoryManager.Instance.GetItems()
            : InventoryManager.Instance.GetItemsByType(filter);

        Refresh(items);
    }

    public void Refresh(List<ItemData> items)
    {
        grid.Clear();

        foreach (var item in items)
        {
            var slot = new VisualElement();
            slot.AddToClassList("slot");

            // Tint slot with hold color
            Color c = item.primaryColor;
            slot.style.backgroundColor = new StyleColor(
                new Color(c.r * 0.35f, c.g * 0.35f, c.b * 0.35f, 0.9f)
            );

            // Icon
            var icon = new VisualElement();
            icon.AddToClassList("slot-icon");
            if (item.icon != null)
                icon.style.backgroundImage = new StyleBackground(item.icon);
            slot.Add(icon);

            // Hold name
            var nameLabel = new Label(item.itemName);
            nameLabel.AddToClassList("slot-name");
            slot.Add(nameLabel);

            // Hover effects
            slot.RegisterCallback<MouseOverEvent>(e => {
                slot.AddToClassList("slot-hover");
                ShowTooltip(item, slot);
            });
            slot.RegisterCallback<MouseOutEvent>(e => {
                slot.RemoveFromClassList("slot-hover");
                HideTooltip();
            });

            grid.Add(slot);
        }
    }

    void ShowTooltip(ItemData item, VisualElement slot)
    {
        var tooltip = root.Q("tooltip");
        if (tooltip == null) return;

        root.Q<Label>("tooltip-name").text = item.itemName;
        root.Q<Label>("tooltip-type").text = item.colorCategory;
        tooltip.style.display = DisplayStyle.Flex;
    }

    void HideTooltip()
    {
        var tooltip = root.Q("tooltip");
        if (tooltip == null) return;
        tooltip.style.display = DisplayStyle.None;
    }
}