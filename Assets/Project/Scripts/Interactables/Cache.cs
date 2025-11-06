using UnityEngine;

/// <summary>
/// Cache interactable that offers 3 free items.
/// Opens UI on interaction, player picks one, cache is consumed.
/// Attach to cache prefab with collider.
/// </summary>
public class Cache : InteractableBase
{
    [Header("Cache Settings")]
    [Tooltip("Item database to pull items from")]
    public ItemDatabase itemDatabase;
    
    [Tooltip("Number of items to offer")]
    public int itemCount = 3;
    
    [Header("Audio")]
    [Tooltip("Wwise event to play when item is collected")]
    public AK.Wwise.Event collectionSound;
    
    // Private fields
    private ItemData[] _itemOfferings;
    
    protected override void Start()
    {
        // Set display name and action
        objectName = "Cache";
        actionVerb = "Open";
        
        base.Start();
        
        // Generate item offerings
        GenerateOfferings();
    }
    
    /// <summary>
    /// Generate random items for this cache
    /// </summary>
    void GenerateOfferings()
    {
        if (itemDatabase == null)
        {
            Debug.LogError("Cache has no ItemDatabase assigned!", this);
            _itemOfferings = new ItemData[0];
            return;
        }
        
        _itemOfferings = itemDatabase.GetRandomItems(itemCount);
        
        if (showDebugInfo)
            Debug.Log($"Cache generated {_itemOfferings.Length} item offerings");
    }
    
    /// <summary>
    /// Called when player presses E
    /// </summary>
    protected override void OnInteract()
    {
        if (_hasBeenUsed)
        {
            if (showDebugInfo)
                Debug.Log("Cache already used, cannot interact again");
            return;
        }
        
        // Mark as used immediately (one-time use)
        MarkAsUsed();
        
        // Open cache UI
        CacheUI cacheUI = CacheUI.Instance;
        if (cacheUI != null)
        {
            cacheUI.ShowCache(_itemOfferings, this);
        }
        else
        {
            Debug.LogError("Cache cannot find CacheUI!", this);
        }
        
        if (showDebugInfo)
            Debug.Log("Cache opened");
    }
    
    /// <summary>
    /// Called by CacheUI when player selects an item
    /// </summary>
    public void OnItemSelected()
    {
        // Play collection sound
        if (collectionSound != null)
        {
            collectionSound.Post(gameObject);
        }
        
        // Destroy cache after selection
        if (showDebugInfo)
            Debug.Log("Cache destroyed after item selection");
        
        Destroy(gameObject);
    }
}
