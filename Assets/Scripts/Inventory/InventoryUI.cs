using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public Transform gridParent;

    void Awake() => Instance = this;

    void Start()
    {
        inventoryPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
            
    }

    public void ToggleInventory()
    {
        bool isOpen = inventoryPanel.activeSelf;
        inventoryPanel.SetActive(!isOpen);

        if (!isOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            InventoryManager.Instance.SortByColor();
        }

        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }


    public void Refresh(List<ItemData> items)
    {
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        foreach(var item in items)
        {
            var slot = Instantiate(slotPrefab, gridParent);
            slot.GetComponent<InventorySlot>().Setup(item);
        }
    }
}
