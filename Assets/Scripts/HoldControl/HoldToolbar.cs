using UnityEngine;
using UnityEngine.UI;

public class HoldToolbar : MonoBehaviour
{
    [SerializeField] PlacementManager placementManager;
    [SerializeField] GameObject toolbarPanel;
    [SerializeField] AngleDragField angleDragField;
    [SerializeField] Button duplicateButton;
    [SerializeField] Button deleteButton;
    [SerializeField] Button rotateButton;
    [SerializeField] Button lockButton;
    [SerializeField] Image lockButtonIcon;
    [SerializeField] Sprite lockSprite;
    [SerializeField] Sprite unlockSprite;

    float _rotateStartAngle;

    void Awake()
    {
        angleDragField.OnDelta      += OnAngleDrag;
        angleDragField.OnDragStart  += OnRotateDragStart;
        angleDragField.OnDragEnd    += OnRotateDragEnd;
        duplicateButton.onClick.AddListener(placementManager.DuplicateSelected);
        deleteButton.onClick.AddListener(placementManager.DeleteSelected);
        lockButton.onClick.AddListener(placementManager.ToggleLock);
        toolbarPanel.SetActive(false);
    }

    void OnAngleDrag(float delta) { placementManager.AddRotation(delta); }

    void OnRotateDragStart()
    {
        _rotateStartAngle = placementManager.CurrentRotationAngle;
    }

    void OnRotateDragEnd()
    {
        float newAngle = placementManager.CurrentRotationAngle;
        if (Mathf.Abs(newAngle - _rotateStartAngle) < 0.01f) return;
        HoldBehavior behavior = placementManager.SelectedBehavior;
        if (behavior == null) return;
        UndoRedoManager.Instance?.PushToUndo(new RotateHoldCommand(behavior, _rotateStartAngle, newAngle, placementManager));
    }

    public bool IsRotating => angleDragField.IsDragging;

    public void Show(bool isLocked)
    {
        toolbarPanel.SetActive(true);
        UpdateLockState(isLocked);
    }

    public void Hide() { toolbarPanel.SetActive(false); }

    public void UpdateLockState(bool isLocked)
    {
        lockButtonIcon.sprite = isLocked ? unlockSprite : lockSprite;
        deleteButton.interactable = !isLocked;
        rotateButton.interactable = !isLocked;
    }
}
