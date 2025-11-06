using UnityEngine;
using TMPro;

/// <summary>
/// Base class for interactable objects (Chest, Shop, Cache).
/// Handles "Press E to [Action]" prompt and interaction detection.
/// Inherit from this class to create specific interactables.
/// </summary>
[RequireComponent(typeof(Collider))]
public abstract class InteractableBase : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("Display name for prompt (e.g., 'Chest', 'Shop', 'Cache')")]
    public string objectName = "Object";
    
    [Tooltip("Action verb for prompt (e.g., 'Open', 'Use')")]
    public string actionVerb = "Interact";
    
    [Tooltip("Interaction range from player")]
    public float interactionRange = 3f;
    
    [Header("UI References")]
    [Tooltip("World-space canvas for prompt text")]
    public Canvas promptCanvas;
    
    [Tooltip("Text for interaction prompt")]
    public TextMeshProUGUI promptText;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Protected fields: _camelCase (accessible by subclasses)
    protected bool _isPlayerInRange = false;
    protected bool _hasBeenUsed = false;
    protected Transform _playerTransform;
    protected Camera _mainCamera;
    
    // Properties
    public bool IsPlayerInRange => _isPlayerInRange;
    public bool HasBeenUsed => _hasBeenUsed;
    
    protected virtual void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }
        else
        {
            Debug.LogError($"InteractableBase on {gameObject.name} cannot find Player!", this);
        }
        
        // Cache main camera for billboard
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            Debug.LogWarning($"InteractableBase on {gameObject.name} cannot find Main Camera! Prompt won't billboard.", this);
        }
        
        // Setup trigger collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        
        // Setup prompt
        UpdatePromptText();
        HidePrompt();
    }
    
    protected virtual void Update()
    {
        // Check if player is in range (backup check in case triggers fail)
        CheckPlayerDistance();
        
        // Handle E key input
        if (_isPlayerInRange && !_hasBeenUsed && Input.GetKeyDown(KeyCode.E))
        {
            OnInteract();
        }
    }
    
    /// <summary>
    /// Billboard the prompt canvas to always face the camera
    /// </summary>
    protected virtual void LateUpdate()
    {
        // Only billboard when prompt is visible and camera exists
        if (promptCanvas != null && promptCanvas.gameObject.activeSelf && _mainCamera != null)
        {
            // Make canvas face the camera
            promptCanvas.transform.LookAt(promptCanvas.transform.position + _mainCamera.transform.forward);
        }
    }
    
    /// <summary>
    /// Check distance to player (backup to trigger system)
    /// </summary>
    void CheckPlayerDistance()
    {
        if (_playerTransform == null) return;
        
        float distance = Vector3.Distance(transform.position, _playerTransform.position);
        bool inRange = distance <= interactionRange;
        
        // Update range state if changed
        if (inRange != _isPlayerInRange)
        {
            _isPlayerInRange = inRange;
            
            if (_isPlayerInRange && !_hasBeenUsed)
            {
                ShowPrompt();
            }
            else
            {
                HidePrompt();
            }
        }
    }
    
    /// <summary>
    /// Called when player enters interaction trigger
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !_hasBeenUsed)
        {
            _isPlayerInRange = true;
            ShowPrompt();
            
            if (showDebugInfo)
                Debug.Log($"Player entered {objectName} interaction range");
        }
    }
    
    /// <summary>
    /// Called when player exits interaction trigger
    /// </summary>
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = false;
            HidePrompt();
            
            if (showDebugInfo)
                Debug.Log($"Player left {objectName} interaction range");
        }
    }
    
    /// <summary>
    /// Show interaction prompt
    /// </summary>
    protected void ShowPrompt()
    {
        if (promptCanvas != null)
        {
            promptCanvas.gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// Hide interaction prompt
    /// </summary>
    protected void HidePrompt()
    {
        if (promptCanvas != null)
        {
            promptCanvas.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Update prompt text based on object name and action
    /// </summary>
    protected void UpdatePromptText()
    {
        if (promptText != null)
        {
            promptText.text = $"Press E to {actionVerb} {objectName}";
        }
    }
    
    /// <summary>
    /// Called when player presses E while in range
    /// Override in subclasses to implement specific behavior
    /// </summary>
    protected abstract void OnInteract();
    
    /// <summary>
    /// Mark this interactable as used (prevents further interaction)
    /// </summary>
    protected void MarkAsUsed()
    {
        _hasBeenUsed = true;
        HidePrompt();
        
        if (showDebugInfo)
            Debug.Log($"{objectName} marked as used");
    }
    
    /// <summary>
    /// Visualize interaction range in editor
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
