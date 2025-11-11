using UnityEngine;

/// <summary>
/// World interactable for weapon pickups.
/// Triggers UI on player collision (no button press needed).
/// Can be dropped from enemies or placed in world.
/// </summary>
[RequireComponent(typeof(Collider))]
public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon")]
    [Tooltip("The weapon this pickup contains")]
    public WeaponData weaponData;
    
    [Header("Visual")]
    [Tooltip("Icon display for the weapon (optional)")]
    public SpriteRenderer iconRenderer;
    
    [Header("Interaction")]
    [Tooltip("Cooldown after closing UI before can interact again (seconds)")]
    public float interactionCooldown = 3f;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private fields
    private float _lastInteractionTime = -999f;
    private bool _isPlayerInRange = false;
    private WeaponPickupUI _weaponPickupUI;
    
    void Start()
    {
        // Find the weapon pickup UI
        _weaponPickupUI = WeaponPickupUI.Instance;
        
        if (_weaponPickupUI == null)
        {
            Debug.LogError("WeaponPickup cannot find WeaponPickupUI in scene!", this);
        }
        
        // Setup trigger collider
        Collider collider = GetComponent<Collider>();
        collider.isTrigger = true;
        
        // Update icon if available
        if (iconRenderer != null && weaponData != null)
        {
            iconRenderer.sprite = weaponData.icon;
        }
        
        if (showDebugInfo)
            Debug.Log($"WeaponPickup initialized: {weaponData?.weaponName ?? "No weapon"}");
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        
        _isPlayerInRange = true;
        TryOpenUI();
    }
    
    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        
        _isPlayerInRange = false;
    }
    
    void Update()
    {
        // Continuously try to open UI while player is in range
        // This handles the case where UI was closed but player didn't leave trigger
        if (_isPlayerInRange)
        {
            TryOpenUI();
        }
        
        // Billboard icon to face camera
        if (iconRenderer != null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                iconRenderer.transform.LookAt(mainCam.transform);
                iconRenderer.transform.Rotate(0, 180, 0); // Face camera
            }
        }
    }
    
    void TryOpenUI()
    {
        // Check cooldown
        if (Time.time < _lastInteractionTime + interactionCooldown)
            return;
        
        // Check if UI is already open
        if (_weaponPickupUI != null && _weaponPickupUI.IsOpen)
            return;
        
        // Open UI
        OpenPickupUI();
    }
    
    void OpenPickupUI()
    {
        if (_weaponPickupUI == null)
        {
            Debug.LogError("Cannot open weapon pickup UI: WeaponPickupUI not found!");
            return;
        }
        
        if (weaponData == null)
        {
            Debug.LogError("Cannot open weapon pickup UI: No weapon data assigned!");
            return;
        }
        
        _lastInteractionTime = Time.time;
        
        // Show UI
        _weaponPickupUI.ShowPickup(weaponData, this);
        
        if (showDebugInfo)
            Debug.Log($"Opened pickup UI for {weaponData.weaponName}");
    }
    
    /// <summary>
    /// Called when weapon is taken from pickup (equip or retrofit)
    /// </summary>
    public void OnWeaponTaken()
    {
        if (showDebugInfo)
            Debug.Log($"Weapon taken: {weaponData.weaponName}. Destroying pickup.");
        
        // Destroy this pickup
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Called when player closes UI without taking weapon
    /// </summary>
    public void OnUIClosedWithoutTaking()
    {
        // Update cooldown so player doesn't immediately retrigger
        _lastInteractionTime = Time.time;
        
        if (showDebugInfo)
            Debug.Log($"UI closed without taking weapon. Cooldown started.");
    }
}
