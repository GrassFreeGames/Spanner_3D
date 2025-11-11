using UnityEngine;

/// <summary>
/// Animated weapon crate pickup for the weapon system.
/// Uses the Weapon Crate Animated asset from Unity Asset Store.
/// Bounces on the ground until player interacts, then opens to reveal weapon.
/// </summary>
[RequireComponent(typeof(Collider))]
public class WeaponCratePickup : MonoBehaviour
{
    [Header("Weapon")]
    [Tooltip("The weapon this crate contains")]
    public WeaponData weaponData;
    
    [Header("Animation")]
    [Tooltip("Animator component (should have Open/Close/Idle states)")]
    public Animator crateAnimator;
    
    [Header("Bounce Settings")]
    [Tooltip("Enable bouncing/wobbling animation")]
    public bool enableBounce = true;
    
    [Tooltip("How high the crate bounces")]
    public float bounceHeight = 0.3f;
    
    [Tooltip("How fast the bounce animation plays")]
    public float bounceSpeed = 2f;
    
    [Tooltip("Rotation wobble amount (degrees)")]
    public float wobbleAmount = 5f;
    
    [Header("Interaction")]
    [Tooltip("Interaction prompt text")]
    public string interactionPrompt = "Press E to Open Crate";
    
    [Tooltip("Interaction range")]
    public float interactionRange = 3f;
    
    [Tooltip("Cooldown after opening before destroying (seconds)")]
    public float destroyDelay = 2f;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private fields
    private bool _isPlayerInRange = false;
    private bool _isOpened = false;
    private Transform _playerTransform;
    private WeaponPickupUI _weaponPickupUI;
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private float _bounceTime = 0f;
    
    // UI Text (optional - for showing prompt)
    private GameObject _promptUI;
    
    void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }
        
        // Find weapon pickup UI (optional - for future when UI is ready)
        _weaponPickupUI = FindObjectOfType<WeaponPickupUI>();
        
        // Setup trigger collider
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        
        // Get animator if not assigned
        if (crateAnimator == null)
        {
            crateAnimator = GetComponent<Animator>();
        }
        
        // Store starting position/rotation for bounce animation
        _startPosition = transform.position;
        _startRotation = transform.rotation;
        
        // Randomize bounce start time so multiple crates don't sync
        _bounceTime = Random.Range(0f, 100f);
        
        if (showDebugInfo)
            Debug.Log($"WeaponCratePickup initialized: {weaponData?.weaponName ?? "No weapon assigned"}");
    }
    
    void Update()
    {
        // Bounce/wobble animation (only if not opened)
        if (enableBounce && !_isOpened)
        {
            AnimateBounce();
        }
        
        // Check for player interaction
        if (_isPlayerInRange && !_isOpened)
        {
            // Check for E key press
            if (Input.GetKeyDown(KeyCode.E))
            {
                OpenCrate();
            }
        }
    }
    
    void AnimateBounce()
    {
        _bounceTime += Time.deltaTime * bounceSpeed;
        
        // Vertical bounce
        float bounceOffset = Mathf.Abs(Mathf.Sin(_bounceTime)) * bounceHeight;
        transform.position = _startPosition + Vector3.up * bounceOffset;
        
        // Rotation wobble
        float wobbleX = Mathf.Sin(_bounceTime * 1.3f) * wobbleAmount;
        float wobbleZ = Mathf.Cos(_bounceTime * 0.8f) * wobbleAmount;
        transform.rotation = _startRotation * Quaternion.Euler(wobbleX, 0, wobbleZ);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = true;
            ShowPrompt();
            
            if (showDebugInfo)
                Debug.Log("Player in range of weapon crate");
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = false;
            HidePrompt();
            
            if (showDebugInfo)
                Debug.Log("Player left weapon crate range");
        }
    }
    
    void OpenCrate()
    {
        if (_isOpened) return;
        
        _isOpened = true;
        
        // Stop bouncing - return to ground position
        if (enableBounce)
        {
            transform.position = _startPosition;
            transform.rotation = _startRotation;
        }
        
        // Play open animation
        if (crateAnimator != null)
        {
            crateAnimator.SetTrigger("Open");
            
            if (showDebugInfo)
                Debug.Log("Playing crate open animation");
        }
        
        // Hide prompt
        HidePrompt();
        
        // TODO: Show weapon pickup UI when it's ready
        // For now, just log what weapon would be shown
        if (weaponData != null)
        {
            Debug.Log($"<color=cyan>CRATE OPENED! Weapon inside: {weaponData.weaponName}</color>");
            
            // When UI is ready, uncomment this:
            /*
            if (_weaponPickupUI != null)
            {
                _weaponPickupUI.ShowPickup(weaponData, this);
            }
            */
        }
        else
        {
            Debug.LogWarning("Crate has no weapon assigned!");
        }
        
        // Schedule destruction
        Invoke(nameof(DestroyCrate), destroyDelay);
    }
    
    void ShowPrompt()
    {
        // TODO: Create a simple TextMeshPro prompt that follows the crate
        // For now, just debug log
        if (showDebugInfo)
            Debug.Log($"[Prompt] {interactionPrompt}");
        
        // Simple implementation - you can replace with better UI later
        // This could create a world-space canvas with text above the crate
    }
    
    void HidePrompt()
    {
        // TODO: Hide the prompt UI
        if (_promptUI != null)
        {
            Destroy(_promptUI);
        }
    }
    
    void DestroyCrate()
    {
        if (showDebugInfo)
            Debug.Log($"Destroying weapon crate: {weaponData?.weaponName}");
        
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Called when weapon is taken from crate (when UI is implemented)
    /// </summary>
    public void OnWeaponTaken()
    {
        if (showDebugInfo)
            Debug.Log($"Weapon taken from crate: {weaponData.weaponName}");
        
        DestroyCrate();
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw interaction range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
