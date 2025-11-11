using UnityEngine;

/// <summary>
/// Simple script to rotate an object continuously.
/// Perfect for pickup items that need to spin.
/// Attach to any GameObject you want to rotate.
/// </summary>
public class SimpleRotate : MonoBehaviour
{
    [Header("Rotation")]
    [Tooltip("Rotation speed in degrees per second")]
    public float rotationSpeed = 60f;
    
    [Tooltip("Rotation axis (default Y = spin upright like a coin)")]
    public Vector3 rotationAxis = Vector3.up;
    
    void Update()
    {
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
    }
}
