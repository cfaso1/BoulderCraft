using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public static bool IsPlacementMode { get; private set; }
    [SerializeField] LayerMask holdLayer;
    GameObject selectedHold;

    public static void SetPlacementMode(bool active)
    {
        IsPlacementMode = active;
    }

    void Update()
    {
        if (!IsPlacementMode) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Deselect();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, holdLayer))
                Select(hit.collider.gameObject);
            else
                Deselect();
        }
    }

    void Select(GameObject hold)
    {
        if (selectedHold == hold) return;
        Deselect();
        selectedHold = hold;
        // TODO: call selectedHold.GetComponent<HoldBehavior>().SetHighlight(true)
    }

    void Deselect()
    {
        if (selectedHold == null) return;
        // TODO: call selectedHold.GetComponent<HoldBehavior>().SetHighlight(false)
        selectedHold = null;
    }
}
