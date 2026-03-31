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
    [SerializeField] Texture2D rotateCursor;
    [SerializeField] float rotateSensitivity = 1f;

    public bool SnapToGrid { get; private set; }

    GameObject selectedHold;
    HoldBehavior selectedBehavior;
    float holdRotationAngle;
    Vector3 lastWallNormal = Vector3.forward;
    Vector3 dragOffset;
    Transform lastDragSurface;
    bool isDragging;
    bool isRotating;
    float rotateStartAngle;
    Vector2 lastRotatePointerPos;
    GameObject ghostHold;
    ItemData pendingItem;
    bool isSpawning;

    // Drag-start state captured on mouse-down select, used to build MoveHoldCommand on finalize.
    HoldState dragStartState;

    // Public accessors used by commands.
    public GameObject SelectedHold => selectedHold;
    public HoldBehavior SelectedBehavior => selectedBehavior;

    void Awake() { Instance = this; }

    public static void SetPlacementMode(bool active) { IsPlacementMode = active; }

    void Update()
    {

        // Handle ghost spawning from inventory
        if (isSpawning)
        {
            UpdateGhostPosition();

            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                ConfirmSpawn();

            if (Input.GetKeyDown(KeyCode.Escape))
                CancelSpawn();

            return; // Skip normal placement logic while spawning
        }

        if (!IsPlacementMode) return;
        if (!IsPlacementMode) return;
        if (SaveLoadUI.IsOpen || !InventoryUIToolkit.isHidden) return;

        bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // Undo / Redo
        if (ctrl && Input.GetKeyDown(KeyCode.Z))
        {
            if (shift) UndoRedoManager.Instance?.Redo();
            else UndoRedoManager.Instance?.Undo();
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

        // Right-click rotation
        HandleRotation();

        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        // Finalize placement — parent hold to volume or unparent to container
        if (Input.GetMouseButtonUp(0) && isDragging && selectedHold != null)
        {
            isDragging = false;
            FinalizeHoldPlacement();
        }

        // Select hold or volume on click
        if (Input.GetMouseButtonDown(0) && !overUI && !isRotating)
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
                    dragStartState = new HoldState(selectedHold.transform, selectedBehavior);
                }
                else
                    Deselect();
            }
            else
                Deselect();
        }

        // Move hold on drag
        if (Input.GetMouseButton(0) && selectedHold != null && !overUI && !isRotating && !selectedBehavior.isLocked)
        {
            isDragging = true;
            DragSelectedHold();
            Cursor.SetCursor(dragCursor, new Vector2(16, 18), CursorMode.Auto);
        }
        else if (!isRotating)
        {
            if (overUI)
                Cursor.SetCursor(PlayerCam.HoverCursor, new Vector2(11, 1), CursorMode.Auto);
            else
                Cursor.SetCursor(PlayerCam.DefaultCursor, new Vector2(7, 3), CursorMode.Auto);
        }
    }

    void HandleRotation()
    {
        if (Input.GetMouseButtonDown(1) && selectedBehavior != null && !selectedBehavior.isLocked)
        {
            isRotating = true;
            rotateStartAngle = holdRotationAngle;
            lastRotatePointerPos = Input.mousePosition;
            Cursor.SetCursor(rotateCursor, new Vector2(16, 16), CursorMode.Auto);
        }

        if (Input.GetMouseButton(1) && isRotating)
        {
            float delta = (Input.mousePosition.x - lastRotatePointerPos.x) * rotateSensitivity;
            lastRotatePointerPos = Input.mousePosition;
            AddRotation(delta);
        }

        if (Input.GetMouseButtonUp(1) && isRotating)
        {
            isRotating = false;
            Cursor.SetCursor(PlayerCam.DefaultCursor, new Vector2(7, 3), CursorMode.Auto);

            if (selectedBehavior != null && Mathf.Abs(holdRotationAngle - rotateStartAngle) > 0.01f)
                UndoRedoManager.Instance?.Record(new RotateHoldCommand(selectedBehavior, rotateStartAngle, holdRotationAngle, this));
        }
    }

    void DragSelectedHold()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!RaycastSurface(ray, out RaycastHit hit)) return;

        Vector3 normal = hit.normal;
        lastWallNormal = normal;
        selectedBehavior.lastWallNormal = normal;

        selectedHold.transform.rotation = HoldBehavior.ComputeRotation(holdRotationAngle, normal);

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

        // Record MoveHoldCommand only if the hold actually moved or changed parent.
        var toState = new HoldState(selectedHold.transform, selectedBehavior);
        bool posChanged = Vector3.Distance(toState.position, dragStartState.position) > 0.001f;
        bool parentChanged = toState.parent != dragStartState.parent;
        if (posChanged || parentChanged)
            UndoRedoManager.Instance?.Record(new MoveHoldCommand(selectedHold, selectedBehavior, this, dragStartState, toState));
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

    public void Deselect()
    {
        if (selectedHold == null) return;
        selectedBehavior.SetHighlight(false);
        selectedHold = null;
        selectedBehavior = null;
        toolbar.Hide();
    }

    void AddRotation(float delta)
    {
        if (selectedBehavior == null || selectedBehavior.isLocked) return;
        holdRotationAngle += delta;
        selectedHold.transform.rotation = HoldBehavior.ComputeRotation(holdRotationAngle, lastWallNormal);
        selectedBehavior.rotationAngle = holdRotationAngle;
    }

    public void ToggleLock()
    {
        if (selectedBehavior == null) return;
        UndoRedoManager.Instance?.Do(new LockToggleCommand(selectedBehavior, toolbar, this));
    }

    public void DuplicateSelected()
    {
        if (selectedHold == null) return;
        var cmd = new DuplicateHoldCommand(selectedHold, selectedBehavior, lastWallNormal, this);
        UndoRedoManager.Instance?.Do(cmd);
        Select(cmd.SpawnedHold);
    }

    public void DeleteSelected()
    {
        if (selectedHold == null || selectedBehavior.isLocked) return;
        UndoRedoManager.Instance?.Do(new DeleteHoldCommand(selectedHold, selectedBehavior, this));
    }

    public void ExitPlacementMode()
    {
        IsPlacementMode = false;
        Deselect();
    }

    public void SyncSelectedState()
    {
        if (selectedBehavior == null) return;
        holdRotationAngle = selectedBehavior.rotationAngle;
        lastWallNormal = selectedBehavior.lastWallNormal;
    }




    public void BeginSpawnFromInventory(ItemData item)
    {
        SetPlacementMode(true);
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
        // Clean up any existing ghost
        if (ghostHold != null) Destroy(ghostHold);

        pendingItem = item;
        isSpawning = true;

        // Create ghost preview
        ghostHold = Instantiate(item.holdPrefab);
        ghostHold.name = "GhostHold";

        // Make it transparent
        SetGhostTransparency(ghostHold, 0.4f);

        // Disable colliders so it doesn't interfere with raycasting
        foreach (var col in ghostHold.GetComponentsInChildren<Collider>())
            col.enabled = false;

        // Close inventory
        InventoryUIToolkit.Instance.ToggleInventory();

        Debug.Log("Spawning: " + item.itemName);
    }

    void SetGhostTransparency(GameObject obj, float alpha)
    {
        foreach (var rend in obj.GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in rend.materials)
            {
                // Switch to transparent rendering mode
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                Color c = mat.color;
                mat.color = new Color(c.r, c.g, c.b, alpha);
            }
        }
    }

    void UpdateGhostPosition()
    {
        if (ghostHold == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!RaycastSurface(ray, out RaycastHit hit)) return;

        Vector3 normal = hit.normal;
        Vector3 position = hit.point + normal * surfaceOffset;

        // Snap to bolt grid if enabled
        if (SnapToGrid && boltGrid != null)
        {
            Vector3 nearestBolt = boltGrid.FindNearestBolt(hit.point, normal);
            position = nearestBolt + normal * surfaceOffset;
        }

        ghostHold.transform.position = position;
        ghostHold.transform.rotation = HoldBehavior.ComputeRotation(0f, normal);
        lastWallNormal = normal;
    }

    void ConfirmSpawn()
    {
        if (ghostHold == null || pendingItem == null) return;

        // Spawn the real hold using existing command system
        var spawnBehavior = ghostHold.GetComponent<HoldBehavior>();
        var cmd = new SpawnHoldCommand(
            pendingItem.holdPrefab,
            ghostHold.transform.position,
            ghostHold.transform.rotation,
            ghostHold.transform.localScale,
            lastWallNormal,
            GetHoldContainer()
        );
        UndoRedoManager.Instance?.Record(cmd);
        cmd.Execute();

        CancelSpawn();
    }

    void CancelSpawn()
    {
        if (ghostHold != null) Destroy(ghostHold);
        ghostHold = null;
        pendingItem = null;
        isSpawning = false;
        FindObjectOfType<PlayerCam>()?.RestoreCameraControl();
    }

    Transform GetHoldContainer()
    {
        // Returns the parent transform where holds are stored
        // Based on your hierarchy this looks like it's under Showcase
        GameObject showcase = GameObject.Find("Showcase");
        return showcase != null ? showcase.transform : null;
    }
}


