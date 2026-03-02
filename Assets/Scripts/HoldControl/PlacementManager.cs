using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public static bool IsPlacementMode { get; private set; }

    [SerializeField] LayerMask holdLayer;
    [SerializeField] LayerMask wallLayer;
    [SerializeField] LayerMask matLayer;
    [SerializeField] float surfaceOffset = 0.02f;

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

        if (Input.GetMouseButton(0) && selectedHold != null)
            DragSelectedHold();
    }

    void DragSelectedHold()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, wallLayer | matLayer)) return;

        Vector3 normal = hit.normal;
        Vector3 refUp = Vector3.ProjectOnPlane(Vector3.up, normal).normalized;
        if (refUp.sqrMagnitude < 0.01f)
            refUp = Vector3.ProjectOnPlane(Vector3.forward, normal).normalized;

        // Euler corrects base direction
        selectedHold.transform.rotation = Quaternion.LookRotation(refUp, normal) * Quaternion.Euler(-90f, 0f, 0f);
        selectedHold.transform.position = hit.point + normal * surfaceOffset;
    }

    void Select(GameObject hold)
    {
        if (selectedHold == hold) return;
        Deselect();
        selectedHold = hold;
        selectedHold.GetComponent<HoldBehavior>().SetHighlight(true);
    }

    void Deselect()
    {
        if (selectedHold == null) return;
        selectedHold.GetComponent<HoldBehavior>().SetHighlight(false);
        selectedHold = null;
    }
}
