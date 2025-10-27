using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement3D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Rotation")]
    [Tooltip("How fast the player rotates to face movement direction")]
    public float rotationSpeed = 10f;

    [Header("Jump")]
    public float jumpForce = 5f;

    [Header("References")]
    public Transform cameraTransform;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // CRITICAL: Freeze rotation so character stays upright
        rb.freezeRotation = true;

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        // Read input
        float horizontal = 0;
        float vertical = 0;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed) horizontal = -1;
            if (Keyboard.current.dKey.isPressed) horizontal = 1;
            if (Keyboard.current.wKey.isPressed) vertical = 1;
            if (Keyboard.current.sKey.isPressed) vertical = -1;
        }

        // Calculate movement direction relative to camera
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        moveDirection = (forward * vertical + right * horizontal).normalized;

        // Handle jump input
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        }
    }

    void FixedUpdate()
    {
        // Apply movement - always keep Y velocity for gravity/jumping
        Vector3 targetVelocity = moveDirection * moveSpeed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

        // Rotate to face movement direction (only on Y axis)
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        // Check if any contact point is on a walkable surface (not too steep)
        foreach (ContactPoint contact in collision.contacts)
        {
            // Check the angle of the surface normal
            // If the surface is pointing mostly upward, it's ground
            float angle = Vector3.Dot(contact.normal, Vector3.up);

            // 0.7 means surfaces up to ~45 degrees are walkable
            // Lower value = can walk on steeper surfaces
            // Higher value = only flat surfaces are walkable
            if (angle > 0.7f)
            {
                isGrounded = true;
                return;
            }
        }

        // If we got here, no walkable surface was found
        isGrounded = false;
    }

    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}
}}