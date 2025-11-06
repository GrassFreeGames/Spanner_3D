using UnityEngine;

/// <summary>
/// Shop interactable that offers 3 purchasable items.
/// Opens UI on interaction, destroys after purchase, persists if closed without purchase.
/// Attach to shop prefab with collider.
/// </summary>
public class Shop : InteractableBase
{
    [Header("Shop Settings")]
    [Tooltip("Item database to pull items from")]
    public ItemDatabase itemDatabase;
    
    [Tooltip("Number of items to offer")]
    public int itemCount = 3;
    
    [Header("Audio")]
    [Tooltip("Wwise event to play when item is purchased")]
    public AK.Wwise.Event purchaseSound;
    
    // Private fields
    private ItemData[] _itemOfferings;
    
    protected override void Start()
    {
        // Set display name and action
        objectName = "Shop";
        actionVerb = "Open";
        
        base.Start();
        
        // Generate item offerings
        GenerateOfferings();
    }
    
    /// <summary>
    /// Generate random items for this shop
    /// </summary>
    void GenerateOfferings()
    {
        if (itemDatabase == null)
        {
            Debug.LogError("Shop has no ItemDatabase assigned!", this);
            _itemOfferings = new ItemData[0];
            return;
        }
        
        _itemOfferings = itemDatabase.GetRandomItems(itemCount);
        
        if (showDebugInfo)
            Debug.Log($"Shop generated {_itemOfferings.Length} item offerings");
    }
    
    /// <summary>
    /// Called when player presses E
    /// </summary>
    protected override void OnInteract()
    {
        if (_hasBeenUsed)
        {
            if (showDebugInfo)
                Debug.Log("Shop already purchased from, cannot interact again");
            return;
        }
        
        // Open shop UI
        ShopUI shopUI = ShopUI.Instance;
        if (shopUI != null)
        {
            shopUI.ShowShop(_itemOfferings, this);
        }
        else
        {
            Debug.LogError("Shop cannot find ShopUI!", this);
        }
        
        if (showDebugInfo)
            Debug.Log("Shop opened");
    }
    
    /// <summary>
    /// Called by ShopUI when player purchases an item
    /// </summary>
    public void OnItemPurchased()
    {
        // Play purchase sound
        if (purchaseSound != null)
        {
            purchaseSound.Post(gameObject);
        }
        
        MarkAsUsed();
        
        // Destroy shop vendor after purchase
        if (showDebugInfo)
            Debug.Log("Shop destroyed after purchase");
        
        Destroy(gameObject);
    }
}
