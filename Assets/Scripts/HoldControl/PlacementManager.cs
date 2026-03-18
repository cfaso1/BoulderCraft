using UnityEngine;
using UnityEngine.EventSystems;

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance { get; private set; }
    public static bool IsPlacementMode { get; private set; }

    [SerializeField] LayerMask holdLayer;
    [SerializeField] LayerMask wallLayer;
    [SerializeField] LayerMask matLayer;
    [SerializeField] LayerMask volumeLayer;
    [SerializeField] float surfaceOffset = 0.01f;
    [SerializeField] HoldToolbar toolbar;
    [SerializeField] BoltGrid boltGrid;
    [SerializeField] Texture2D dragCursor;

    public bool SnapToGrid { get; private set; }

    GameObject selectedHold;
    HoldBehavior selectedBehavior;
    float holdRotationAngle = 0f;
    Vector3 lastWallNormal = Vector3.forward;
    Vector3 dragOffset = Vector3.zero;
    Transform lastDragSurface;
    bool isDragging;

    // Drag-start state captured on mouse-down select, used to build MoveHoldCommand on finalize.
    Vector3 _dragStartPos;
    Quaternion _dragStartRot;
    Transform _dragStartParent;
    float _dragStartAngle;
    Vector3 _dragStartNormal;

    // Public accessors used by commands and HoldToolbar.
    public GameObject SelectedHold     => selectedHold;
    public HoldBehavior SelectedBehavior => selectedBehavior;
    public float CurrentRotationAngle  => holdRotationAngle;

    void Awake() { Instance = this; }

    public static void SetPlacementMode(bool active) { IsPlacementMode = active; }

    void Update()
    {
        if (!IsPlacementMode) return;
        if (SaveLoadUI.IsOpen) return;

        bool ctrl  = Input.GetKey(KeyCode.LeftControl)  || Input.GetKey(KeyCode.RightControl);
        bool shift = Input.GetKey(KeyCode.LeftShift)    || Input.GetKey(KeyCode.RightShift);

        // Undo / Redo
        if (ctrl && Input.GetKeyDown(KeyCode.Z))
        {
            if (shift) UndoRedoManager.Instance?.Redo();
            else       UndoRedoManager.Instance?.Undo();
            return;
        }
        if (ctrl && Input.GetKeyDown(KeyCode.Y))
        {
            UndoRedoManager.Instance?.Redo();
            return;
        }

        // Toggle bolt snap
        if (Input.GetKeyDown(KeyCode.Tab))
            SnapToGrid = !SnapToGrid;

        // Deselect hold
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Deselect();
            return;
        }

        // Delete and duplicate hold
        if (selectedHold != null)
        {
            if (Input.GetKeyDown(KeyCode.Delete) && !selectedBehavior.isLocked) DeleteSelected();
            if (ctrl && Input.GetKeyDown(KeyCode.D)) DuplicateSelected();
        }

        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        // Finalize placement — parent hold to volume or unparent to container
        if (Input.GetMouseButtonUp(0) && isDragging && selectedHold != null)
        {
            isDragging = false;
            FinalizeHoldPlacement();
        }

        // Select hold or volume on click
        if (Input.GetMouseButtonDown(0) && !overUI)
        {
            isDragging = false;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, holdLayer | volumeLayer))
            {
                HoldBehavior behavior = hit.collider.GetComponentInParent<HoldBehavior>();
                if (behavior != null)
                {
                    Select(behavior.gameObject);
                    if (RaycastSurface(ray, out RaycastHit wallHit))
                    {
                        // Sync wall normal at select time so rotation works even without dragging first.
                        lastWallNormal = wallHit.normal;
                        selectedBehavior.lastWallNormal = wallHit.normal;

                        Vector3 rawOffset = selectedHold.transform.position - (wallHit.point + wallHit.normal * surfaceOffset);
                        dragOffset = Vector3.ProjectOnPlane(rawOffset, wallHit.normal);
                    }
                    else
                        dragOffset = Vector3.zero;

                    // Capture drag-start state for MoveHoldCommand.
                    _dragStartPos    = selectedHold.transform.position;
                    _dragStartRot    = selectedHold.transform.rotation;
                    _dragStartParent = selectedHold.transform.parent;
                    _dragStartAngle  = holdRotationAngle;
                    _dragStartNormal = lastWallNormal;
                }
                else
                    Deselect();
            }
            else
                Deselect();
        }

        // Move hold on drag
        if (Input.GetMouseButton(0) && selectedHold != null && !overUI && !toolbar.IsRotating && !selectedBehavior.isLocked)
        {
            isDragging = true;
            DragSelectedHold();
            Cursor.SetCursor(dragCursor, new Vector2(16, 18), CursorMode.Auto);
        }
        else if (!toolbar.IsRotating)
        {
            if (overUI)
                Cursor.SetCursor(PlayerCam.HoverCursor, new Vector2(11, 1), CursorMode.Auto);
            else
                Cursor.SetCursor(PlayerCam.DefaultCursor, new Vector2(7, 3), CursorMode.Auto);
        }
    }

    void DragSelectedHold()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!RaycastSurface(ray, out RaycastHit hit)) return;

        Vector3 normal = hit.normal;
        lastWallNormal = normal;
        selectedBehavior.lastWallNormal = normal;

        Vector3 refUp = Vector3.ProjectOnPlane(Vector3.up, normal).normalized;
        if (refUp.sqrMagnitude < 0.01f)
            refUp = Vector3.ProjectOnPlane(Vector3.forward, normal).normalized;

        Quaternion baseOrientation = Quaternion.LookRotation(refUp, normal) * Quaternion.Euler(-90f, 0f, 0f);
        Quaternion userRotation = Quaternion.AngleAxis(holdRotationAngle, normal);
        selectedHold.transform.rotation = userRotation * baseOrientation;

        Vector3 wallAlignedOffset = Vector3.ProjectOnPlane(dragOffset, normal);
        Vector3 rawPosition = hit.point + wallAlignedOffset;

        if (SnapToGrid && boltGrid != null)
        {
            Vector3 nearestBolt = boltGrid.FindNearestBolt(hit.point, normal, selectedHold.transform);
            rawPosition = Vector3.ProjectOnPlane(nearestBolt - hit.point, normal) + hit.point;
        }

        selectedHold.transform.position = rawPosition + normal * surfaceOffset;
        selectedBehavior.rotationAngle = holdRotationAngle;
        lastDragSurface = hit.collider.transform;
    }

    // Raycast against all placeable surfaces, skipping the selected object's own colliders.
    bool RaycastSurface(Ray ray, out RaycastHit hit)
    {
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, wallLayer | matLayer | volumeLayer);
        hit = default;
        float minDist = float.MaxValue;
        foreach (var h in hits)
        {
            if (h.distance < minDist && h.collider.GetComponentInParent<HoldBehavior>() != selectedBehavior)
            {
                minDist = h.distance;
                hit = h;
            }
        }
        return minDist < float.MaxValue;
    }

    void FinalizeHoldPlacement()
    {
        if (lastDragSurface == null) return;
        HoldBehavior volumeOnSurface = lastDragSurface.GetComponentInParent<HoldBehavior>();
        if (volumeOnSurface != null && volumeOnSurface.holdType == HoldType.Volume && volumeOnSurface != selectedBehavior)
            selectedHold.transform.SetParent(volumeOnSurface.transform, true);
        else
            selectedHold.transform.SetParent(null, true);

        // Push MoveHoldCommand only if the hold actually moved or changed parent.
        bool posChanged    = Vector3.Distance(selectedHold.transform.position, _dragStartPos) > 0.001f;
        bool parentChanged = selectedHold.transform.parent != _dragStartParent;
        if (posChanged || parentChanged)
        {
            UndoRedoManager.Instance?.PushToUndo(new MoveHoldCommand(
                selectedHold, this,
                _dragStartPos, _dragStartRot, _dragStartParent, _dragStartAngle, _dragStartNormal,
                selectedHold.transform.position, selectedHold.transform.rotation,
                selectedHold.transform.parent, selectedBehavior.rotationAngle, lastWallNormal
            ));
        }
    }

    void ApplyCurrentRotation()
    {
        if (selectedHold == null) return;
        selectedHold.transform.rotation = ComputeHoldRotation(holdRotationAngle, lastWallNormal);
        selectedBehavior.rotationAngle  = holdRotationAngle;
    }

    void Select(GameObject hold)
    {
        if (selectedHold == hold) return;
        Deselect();
        selectedHold = hold;
        selectedBehavior = hold.GetComponent<HoldBehavior>();
        holdRotationAngle = selectedBehavior.rotationAngle;
        selectedBehavior.SetHighlight(true);
        toolbar.Show(selectedBehavior.isLocked);
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
        if (selectedBehavior != null && selectedBehavior.isLocked) return;
        holdRotationAngle += delta;
        ApplyCurrentRotation();
    }

    public void ToggleLock()
    {
        if (selectedBehavior == null) return;
        UndoRedoManager.Instance?.Do(new LockToggleCommand(selectedBehavior, toolbar, this));
    }

    public void DuplicateSelected()
    {
        if (selectedHold == null) return;

        Vector3 normal = lastWallNormal;
        Vector3 refUp  = Vector3.ProjectOnPlane(Vector3.up, normal).normalized;
        if (refUp.sqrMagnitude < 0.01f) refUp = Vector3.ProjectOnPlane(Vector3.forward, normal).normalized;

        ItemData item = InventoryManager.Instance?.FindItemData(selectedBehavior.holdId);
        GameObject prefab = item?.holdPrefab;

        DuplicateHoldCommand cmd;
        if (prefab != null)
        {
            cmd = new DuplicateHoldCommand(
                prefab,
                selectedHold.transform.position + refUp * 0.1f,
                selectedHold.transform.rotation,
                selectedHold.transform.lossyScale,
                selectedHold.transform.parent,
                selectedBehavior.rotationAngle,
                lastWallNormal,
                this
            );
            UndoRedoManager.Instance?.Do(cmd);
        }
        else
        {
            // Fallback: instantiate directly from the selected instance.
            selectedBehavior.SetHighlight(false);
            GameObject copy = Instantiate(selectedHold, selectedHold.transform.position + refUp * 0.1f, selectedHold.transform.rotation);
            copy.transform.localScale = selectedHold.transform.lossyScale;
            copy.transform.SetParent(selectedHold.transform.parent, true);
            var b = copy.GetComponent<HoldBehavior>();
            b.isLocked = false;
            b.lastWallNormal = lastWallNormal;
            b.instanceSaveId = System.Guid.NewGuid().ToString();
            cmd = new DuplicateHoldCommand(copy, this);
            UndoRedoManager.Instance?.PushToUndo(cmd);
        }

        Select(cmd.SpawnedHold);
    }

    public void DeleteSelected()
    {
        if (selectedHold == null || selectedBehavior.isLocked) return;
        var cmd = new DeleteHoldCommand(selectedHold, this, toolbar);
        Deselect();
        UndoRedoManager.Instance?.Do(cmd);
    }

    public void ExitPlacementMode()
    {
        IsPlacementMode = false;
        Deselect();
    }

    // Called by commands after externally modifying the selected hold's state.
    public void ForceDeselect() => Deselect();

    // Re-reads rotation angle and wall normal from the selected HoldBehavior after an external state change.
    public void SyncSelectedState()
    {
        if (selectedBehavior == null) return;
        holdRotationAngle = selectedBehavior.rotationAngle;
        lastWallNormal    = selectedBehavior.lastWallNormal;
    }

    // Shared rotation math used by commands that need to recompute hold orientation without an active drag.
    public static Quaternion ComputeHoldRotation(float angle, Vector3 normal)
    {
        Vector3 refUp = Vector3.ProjectOnPlane(Vector3.up, normal).normalized;
        if (refUp.sqrMagnitude < 0.01f)
            refUp = Vector3.ProjectOnPlane(Vector3.forward, normal).normalized;
        return Quaternion.AngleAxis(angle, normal) * (Quaternion.LookRotation(refUp, normal) * Quaternion.Euler(-90f, 0f, 0f));
    }
}
