using UnityEngine;
using System.Collections;

/// <summary>
/// Chest interactable with opening animation and rarity roll.
/// Plays animation, reveals item, opens UI.
/// Attach to chest prefab with collider and visual mesh.
/// </summary>
public class Chest : InteractableBase
{
    [Header("Chest Settings")]
    [Tooltip("Item database to pull items from")]
    public ItemDatabase itemDatabase;
    
    [Tooltip("Is this a boss chest? (Guarantees rare/epic)")]
    public bool isBossChest = false;
    
    [Header("Animation Settings")]
    [Tooltip("Mesh renderer for chest visual")]
    public MeshRenderer chestMesh;
    
    [Tooltip("Duration of rarity roll animation")]
    public float rollDuration = 2f;
    
    [Tooltip("Duration of chest opening animation")]
    public float openDuration = 1f;
    
    [Tooltip("Scale multiplier during animation")]
    public float bounceScale = 1.2f;
    
    [Header("Audio")]
    [Tooltip("Wwise event for chest opening")]
    public AK.Wwise.Event openSound;
    
    [Tooltip("Wwise event for rarity reveal")]
    public AK.Wwise.Event revealSound;
    
    // Private fields
    private ItemData _rolledItem;
    private bool _isAnimating = false;
    private Material _chestMaterial;
    private Color _originalColor;
    
    protected override void Start()
    {
        // Set display name and action
        objectName = "Chest";
        actionVerb = "Open";
        
        base.Start();
        
        // Get chest material for color changes
        if (chestMesh != null)
        {
            _chestMaterial = chestMesh.material;
            _originalColor = _chestMaterial.color;
        }
    }
    
    /// <summary>
    /// Called when player presses E
    /// </summary>
    protected override void OnInteract()
    {
        if (_hasBeenUsed || _isAnimating)
        {
            if (showDebugInfo)
                Debug.Log("Chest already opened or animating");
            return;
        }
        
        // Mark as used
        MarkAsUsed();
        
        // Roll for item
        RollItem();
        
        // Start animation
        StartCoroutine(ChestOpeningSequence());
        
        if (showDebugInfo)
            Debug.Log($"Chest opened! Rolled: {_rolledItem.itemName} ({_rolledItem.GetRarityName()})");
    }
    
    /// <summary>
    /// Roll for random item
    /// </summary>
    void RollItem()
    {
        if (itemDatabase == null)
        {
            Debug.LogError("Chest has no ItemDatabase assigned!", this);
            return;
        }
        
        if (isBossChest)
        {
            _rolledItem = itemDatabase.GetRandomItemMinRarity(2); // Rare or Epic
        }
        else
        {
            _rolledItem = itemDatabase.GetRandomItem();
        }
    }
    
    /// <summary>
    /// Chest opening animation sequence
    /// </summary>
    IEnumerator ChestOpeningSequence()
    {
        _isAnimating = true;
        
        // Play opening sound
        if (openSound != null)
        {
            openSound.Post(gameObject);
        }
        
        // Phase 1: Rarity roll animation (flash colors)
        float rollTimer = 0f;
        Color targetColor = _rolledItem.GetRarityColor();
        
        while (rollTimer < rollDuration)
        {
            rollTimer += Time.unscaledDeltaTime;
            float progress = rollTimer / rollDuration;
            
            // Flash through rarity colors with increasing frequency
            float flashSpeed = Mathf.Lerp(2f, 10f, progress);
            float t = Mathf.PingPong(Time.unscaledTime * flashSpeed, 1f);
            
            // Cycle through colors
            Color[] colors = {
                new Color(1f, 1f, 1f),      // White (Common)
                new Color(0.3f, 1f, 0.3f),  // Green (Uncommon)
                new Color(0.3f, 0.6f, 1f),  // Blue (Rare)
                new Color(0.8f, 0.3f, 1f)   // Purple (Epic)
            };
            
            int colorIndex = Mathf.FloorToInt(t * colors.Length) % colors.Length;
            Color currentColor = colors[colorIndex];
            
            // Near end, bias toward target color
            if (progress > 0.7f)
            {
                float targetLerp = (progress - 0.7f) / 0.3f;
                currentColor = Color.Lerp(currentColor, targetColor, targetLerp);
            }
            
            if (_chestMaterial != null)
            {
                _chestMaterial.color = currentColor;
            }
            
            // Bounce scale
            float scale = 1f + Mathf.Sin(Time.unscaledTime * 5f) * 0.1f;
            transform.localScale = Vector3.one * scale;
            
            yield return null;
        }
        
        // Set final color to rarity
        if (_chestMaterial != null)
        {
            _chestMaterial.color = targetColor;
        }
        
        // Play reveal sound
        if (revealSound != null)
        {
            revealSound.Post(gameObject);
        }
        
        // Phase 2: Opening animation (scale up)
        float openTimer = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.one * bounceScale;
        
        while (openTimer < openDuration)
        {
            openTimer += Time.unscaledDeltaTime;
            float progress = openTimer / openDuration;
            
            // Scale up with easing
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
            transform.localScale = Vector3.Lerp(startScale, endScale, easedProgress);
            
            yield return null;
        }
        
        _isAnimating = false;
        
        // Show chest UI with item
        ChestUI chestUI = ChestUI.Instance;
        if (chestUI != null)
        {
            chestUI.ShowChest(_rolledItem);
        }
        else
        {
            Debug.LogError("Chest cannot find ChestUI!", this);
        }
        
        // Destroy chest after UI is shown
        if (showDebugInfo)
            Debug.Log("Chest destroyed after opening");
        
        // Wait a bit before destroying so animation is visible
        yield return new WaitForSecondsRealtime(0.5f);
        
        Destroy(gameObject);
    }
    
    void OnDestroy()
    {
        // Clean up material instance
        if (_chestMaterial != null)
        {
            Destroy(_chestMaterial);
        }
    }
}
