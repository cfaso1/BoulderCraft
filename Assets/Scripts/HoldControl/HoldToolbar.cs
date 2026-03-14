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

    void Awake()
    {
        angleDragField.OnDelta += OnAngleDrag;
        duplicateButton.onClick.AddListener(placementManager.DuplicateSelected);
        deleteButton.onClick.AddListener(placementManager.DeleteSelected);
        lockButton.onClick.AddListener(placementManager.ToggleLock);
        toolbarPanel.SetActive(false);
    }

    void OnAngleDrag(float delta) { placementManager.AddRotation(delta); }

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
