using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform cameraPosition;

    private void Update()
    {
        if (InventoryUI.Instance != null && InventoryUI.Instance.inventoryPanel.activeSelf)
            return;
        transform.position = cameraPosition.position;
    }
}
