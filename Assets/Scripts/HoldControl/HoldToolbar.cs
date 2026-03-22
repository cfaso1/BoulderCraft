using UnityEngine;
using UnityEngine.UI;

public class HoldToolbar : MonoBehaviour
{
    [SerializeField] PlacementManager placementManager;
    [SerializeField] GameObject toolbarPanel;
    [SerializeField] Button duplicateButton;
    [SerializeField] Button deleteButton;
    [SerializeField] Button lockButton;
    [SerializeField] Image lockButtonIcon;
    [SerializeField] Sprite lockSprite;
    [SerializeField] Sprite unlockSprite;

    void Awake()
    {
        duplicateButton.onClick.AddListener(placementManager.DuplicateSelected);
        deleteButton.onClick.AddListener(placementManager.DeleteSelected);
        lockButton.onClick.AddListener(placementManager.ToggleLock);
        toolbarPanel.SetActive(false);
    }

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
    }
}
