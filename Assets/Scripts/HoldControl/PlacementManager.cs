using UnityEngine;
using UnityEngine.EventSystems;

public class PlacementManager : MonoBehaviour
{
    public static bool IsPlacementMode { get; private set; }

    [SerializeField] LayerMask holdLayer;
    [SerializeField] LayerMask wallLayer;
    [SerializeField] LayerMask matLayer;
    [SerializeField] float surfaceOffset = 0.01f;
    [SerializeField] HoldToolbar toolbar;

    GameObject selectedHold;
    HoldBehavior selectedBehavior;
    float holdRotationAngle = 0f;
    Vector3 lastWallNormal = Vector3.forward;
    Vector3 dragOffset = Vector3.zero;

    public float HoldRotationAngle => holdRotationAngle;

    public static void SetPlacementMode(bool active) { IsPlacementMode = active; }

    void Update()
    {
        if (!IsPlacementMode) return;

        // Deselect hold
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Deselect();
            return;
        }

        // Delete and dupllicate hold
        if (selectedHold != null)
        {
            if (Input.GetKeyDown(KeyCode.Delete)) DeleteSelected();
            if (Input.GetKeyDown(KeyCode.D) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
                DuplicateSelected();
        }

        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        // Select hold on click
        if (Input.GetMouseButtonDown(0) && !overUI)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, holdLayer))
            {
                Select(hit.collider.gameObject);
                if (Physics.Raycast(ray, out RaycastHit wallHit, Mathf.Infinity, wallLayer | matLayer))
                {
                    Vector3 rawOffset = selectedHold.transform.position - (wallHit.point + wallHit.normal * surfaceOffset);
                    dragOffset = Vector3.ProjectOnPlane(rawOffset, wallHit.normal);
                }
                else
                    dragOffset = Vector3.zero;
            }
            else
                Deselect();
        }

        // Move hold on drag
        if (Input.GetMouseButton(0) && selectedHold != null && !overUI && !toolbar.IsRotating)
            DragSelectedHold();
    }

    void DragSelectedHold()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, wallLayer | matLayer)) return;

        Vector3 normal = hit.normal;
        lastWallNormal = normal;

        Vector3 refUp = Vector3.ProjectOnPlane(Vector3.up, normal).normalized;
        if (refUp.sqrMagnitude < 0.01f)
            refUp = Vector3.ProjectOnPlane(Vector3.forward, normal).normalized;

        Quaternion baseOrientation = Quaternion.LookRotation(refUp, normal) * Quaternion.Euler(-90f, 0f, 0f);
        Quaternion userRotation = Quaternion.AngleAxis(holdRotationAngle, normal);
        selectedHold.transform.rotation = userRotation * baseOrientation;

        Vector3 wallAlignedOffset = Vector3.ProjectOnPlane(dragOffset, normal);
        selectedHold.transform.position = hit.point + normal * surfaceOffset + wallAlignedOffset;
        selectedBehavior.rotationAngle = holdRotationAngle;
    }

    void ApplyCurrentRotation()
    {
        if (selectedHold == null) return;
        Vector3 normal = lastWallNormal;
        Vector3 refUp = Vector3.ProjectOnPlane(Vector3.up, normal).normalized;
        if (refUp.sqrMagnitude < 0.01f)
            refUp = Vector3.ProjectOnPlane(Vector3.forward, normal).normalized;
        Quaternion baseOrientation = Quaternion.LookRotation(refUp, normal) * Quaternion.Euler(-90f, 0f, 0f);
        selectedHold.transform.rotation = Quaternion.AngleAxis(holdRotationAngle, normal) * baseOrientation;
        selectedBehavior.rotationAngle = holdRotationAngle;
    }

    void Select(GameObject hold)
    {
        if (selectedHold == hold) return;
        Deselect();
        selectedHold = hold;
        selectedBehavior = hold.GetComponent<HoldBehavior>();
        holdRotationAngle = selectedBehavior.rotationAngle;
        selectedBehavior.SetHighlight(true);
        toolbar.Show();
    }

    void Deselect()
    {
        if (selectedHold == null) return;
        selectedBehavior.SetHighlight(false);
        selectedHold = null;
        selectedBehavior = null;
        toolbar.Hide();
    }

    public void AddRotation(float delta)
    {
        holdRotationAngle += delta;
        ApplyCurrentRotation();
    }

    public void DuplicateSelected()
    {
        if (selectedHold == null) return;
        selectedBehavior.SetHighlight(false);
        Vector3 normal = lastWallNormal;
        Vector3 refUp = Vector3.ProjectOnPlane(Vector3.up, normal).normalized;
        if (refUp.sqrMagnitude < 0.01f) refUp = Vector3.ProjectOnPlane(Vector3.forward, normal).normalized;
        GameObject copy = Instantiate(selectedHold, selectedHold.transform.position + refUp * 0.1f, selectedHold.transform.rotation);
        copy.transform.localScale = selectedHold.transform.localScale;
        Select(copy);
    }

    public void DeleteSelected()
    {
        if (selectedHold == null) return;
        GameObject toDestroy = selectedHold;
        Deselect();
        Destroy(toDestroy);
    }

    public void ExitPlacementMode()
    {
        IsPlacementMode = false;
        Deselect();
    }
}
