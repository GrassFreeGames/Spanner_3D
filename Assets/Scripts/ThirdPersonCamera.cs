using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    
    [Header("Camera Distance")]
    public float distance = 5f;
    public float minDistance = 2f;
    public float maxDistance = 10f;
    
    [Header("Camera Height")]
    public float height = 2f;
    
    [Header("Mouse Sensitivity")]
    public float mouseSensitivity = 2f;
    
    [Header("Rotation Limits")]
    [Tooltip("Minimum vertical angle (looking down)")]
    public float minVerticalAngle = -30f;
    
    [Tooltip("Maximum vertical angle (looking up)")]
    public float maxVerticalAngle = 60f;
    
    [Header("Smoothing")]
    [Range(0f, 0.3f)]
    public float positionSmoothTime = 0.1f;
    
    [Range(0f, 0.3f)]
    public float rotationSmoothTime = 0.05f;
    
    [Header("Look At Offset")]
    public Vector3 lookAtOffset = new Vector3(0, 1.5f, 0);
    
    // Private variables
    private float currentHorizontalAngle = 0f;
    private float currentVerticalAngle = 20f;
    private Vector3 currentVelocity;
    private float rotationVelocityX;
    private float rotationVelocityY;

    void Start()
    {
        // Lock and hide cursor for better gameplay feel
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Initialize angles based on starting position if needed
        if (target != null)
        {
            Vector3 angles = transform.eulerAngles;
            currentHorizontalAngle = angles.y;
            currentVerticalAngle = angles.x;
        }
    }

    void Update()
    {
        // Allow escape key to unlock cursor for testing
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    void LateUpdate()
    {
        if (target == null) return;
        
        // Get mouse input
        float mouseX = 0;
        float mouseY = 0;
        
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            mouseX = mouseDelta.x;
            mouseY = mouseDelta.y;
        }
        
        // Update rotation angles based on mouse input
        // Mouse delta is already frame-independent, so don't multiply by Time.deltaTime
        currentHorizontalAngle += mouseX * mouseSensitivity * 0.1f;
        currentVerticalAngle -= mouseY * mouseSensitivity * 0.1f;
        
        // Normalize horizontal angle to prevent wrapping issues
        // Keep angle between -180 and 180 degrees
        currentHorizontalAngle = Mathf.Repeat(currentHorizontalAngle + 180f, 360f) - 180f;
        
        // Clamp vertical angle to prevent camera flipping
        currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);
        
        // Calculate desired camera position using spherical coordinates
        Quaternion rotation = Quaternion.Euler(currentVerticalAngle, currentHorizontalAngle, 0);
        Vector3 offset = rotation * new Vector3(0, height, -distance);
        Vector3 desiredPosition = target.position + offset;
        
        // Smooth camera position
        if (positionSmoothTime > 0)
        {
            transform.position = Vector3.SmoothDamp(
                transform.position, 
                desiredPosition, 
                ref currentVelocity, 
                positionSmoothTime
            );
        }
        else
        {
            transform.position = desiredPosition;
        }
        
        // Look at target
        Vector3 lookAtPoint = target.position + lookAtOffset;
        transform.LookAt(lookAtPoint);
    }
}
