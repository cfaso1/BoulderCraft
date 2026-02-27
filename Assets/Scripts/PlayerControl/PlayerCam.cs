using UnityEngine;

public class PlayerCam : MonoBehaviour
{

    public float sensX;
    public float sensY;

    public Transform orientation;

    float xRotation;
    float yRotation;
    float xRotationSaved;
    float yRotationSaved;
    bool cameraFree;
    bool skipNextInput;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cameraFree = true;
        skipNextInput = false;
    }

    private void Update()
    {
        if (cameraFree) MoveCamera();
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleCursor();
            // TODO Toggle UI
        }
    }

    public void MoveCamera()
    {
        // Get mouse inputs
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        // If the mouse has moved, move the camera
        if (!(mouseX == 0 && mouseY == 0))
        {
            // Use original rotation after exiting menu
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

            // Move camera
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
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            cameraFree = false;
            xRotationSaved = xRotation;
            yRotationSaved = yRotation;
            skipNextInput = true;
        }
    }
}
