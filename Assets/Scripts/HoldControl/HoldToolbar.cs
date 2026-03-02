using UnityEngine;
using UnityEngine.UI;

public class HoldToolbar : MonoBehaviour
{
    [SerializeField] PlacementManager placementManager;
    [SerializeField] GameObject toolbarPanel;
    [SerializeField] AngleDragField angleDragField;
    [SerializeField] Button duplicateButton;
    [SerializeField] Button deleteButton;

    void Awake()
    {
        angleDragField.OnDelta += OnAngleDrag;
        duplicateButton.onClick.AddListener(placementManager.DuplicateSelected);
        deleteButton.onClick.AddListener(placementManager.DeleteSelected);
        toolbarPanel.SetActive(false);
    }

    void OnAngleDrag(float delta) { placementManager.AddRotation(delta); }

    public bool IsRotating => angleDragField.IsDragging;

    public void Show() { toolbarPanel.SetActive(true); }
    public void Hide() { toolbarPanel.SetActive(false); }
}
