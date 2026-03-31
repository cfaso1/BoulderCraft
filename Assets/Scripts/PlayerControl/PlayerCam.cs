using UnityEngine;
public class PlayerCam : MonoBehaviour
{
    public static PlayerCam Instance { get; private set; } // Add this
    public float sensX;
    public float sensY;
    public Transform orientation;
    [SerializeField] PlacementManager placementManager;
    [SerializeField] Texture2D defaultCursor;
    [SerializeField] Texture2D hoverCursor;
    public static Texture2D DefaultCursor { get; private set; }
    public static Texture2D HoverCursor { get; private set; }
    float xRotation;
    float yRotation;
    float xRotationSaved;
    float yRotationSaved;
    public bool cameraFree; // Changed to public
    bool skipNextInput;

    private void Start()
    {
        Instance = this; // Add this
        DefaultCursor = defaultCursor;
        HoverCursor = hoverCursor;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cameraFree = true;
        skipNextInput = false;
    }

    private void Update()
    {
        if (SaveLoadUI.IsOpen || !InventoryUIToolkit.isHidden) return;
        if (cameraFree) MoveCamera();
        if (Input.GetKeyDown(KeyCode.E))
            ToggleCursor();
    }

    void MoveCamera()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;
        if (!(mouseX == 0 && mouseY == 0))
        {
            if (skipNextInput)
            {
                xRotation = xRotationSaved;
                yRotation = yRotationSaved;
                skipNextInput = false;
            }
            else
            {
                yRotation += mouseX;
                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            }
            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        }
    }

    public void ToggleCursor()
    {
        if (Cursor.visible)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            cameraFree = true;
            placementManager.ExitPlacementMode();
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Cursor.SetCursor(defaultCursor, new Vector2(7, 3), CursorMode.Auto);
            cameraFree = false;
            xRotationSaved = xRotation;
            yRotationSaved = yRotation;
            skipNextInput = true;
            PlacementManager.SetPlacementMode(true);
        }
    }

    public void RestoreCameraControl()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cameraFree = true;
        xRotationSaved = xRotation;
        yRotationSaved = yRotation;
        skipNextInput = true;
    }
}