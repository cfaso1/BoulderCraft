using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed;
    public float drag;

    public float flightForce;
    public KeyCode upKey = KeyCode.Space;
    public KeyCode downKey = KeyCode.LeftShift;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;
    bool inputBlocked;

    Vector3 moveDirection;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        inputBlocked = SaveLoadUI.IsOpen || !InventoryUIToolkit.isHidden;
        if (inputBlocked)
        {
            horizontalInput = 0;
            verticalInput = 0;
        }
        else
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");
        }
        rb.linearDamping = drag;
        SpeedControl();
    }

    private void FixedUpdate()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        rb.AddForce(moveDirection * moveSpeed * 10f, ForceMode.Force);

        if (inputBlocked) return;
        if (Input.GetKey(upKey)) rb.AddForce(transform.up * flightForce, ForceMode.Force);
        if (Input.GetKey(downKey)) rb.AddForce(-transform.up * flightForce, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }
}
