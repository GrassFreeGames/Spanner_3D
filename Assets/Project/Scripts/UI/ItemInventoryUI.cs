using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Displays player item inventory at bottom of screen.
/// Shows item icons with stack counts (x2, x3, etc).
/// Attach to Canvas with horizontal layout.
/// </summary>
public class ItemInventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Parent container for item slots (should have HorizontalLayoutGroup)")]
    public Transform itemSlotContainer;
    
    [Tooltip("Prefab for individual item slots")]
    public GameObject itemSlotPrefab;
    
    [Header("Layout")]
    [Tooltip("Spacing between item slots")]
    public float slotSpacing = 10f;
    
    [Tooltip("Size of item slot icons")]
    public float slotSize = 64f;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Active item slots
    private Dictionary<ItemData, GameObject> _activeSlots = new Dictionary<ItemData, GameObject>();
    
    // Singleton pattern
    private static ItemInventoryUI _instance;
    public static ItemInventoryUI Instance => _instance;
    
    void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple ItemInventoryUI instances found! Destroying duplicate.", this);
            Destroy(this);
            return;
        }
        _instance = this;
    }
    
    void Start()
    {
        // Setup layout group if not already configured
        SetupLayoutGroup();
        
        // Subscribe to inventory changes
        ItemManager itemManager = ItemManager.Instance;
        if (itemManager != null)
        {
            itemManager.OnInventoryChanged += RefreshInventoryDisplay;
        }
        else
        {
            Debug.LogError("ItemInventoryUI cannot find ItemManager!", this);
        }
        
        // Initial refresh
        RefreshInventoryDisplay();
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        ItemManager itemManager = ItemManager.Instance;
        if (itemManager != null)
        {
            itemManager.OnInventoryChanged -= RefreshInventoryDisplay;
        }
    }
    
    /// <summary>
    /// Setup horizontal layout group on container
    /// </summary>
    void SetupLayoutGroup()
    {
        if (itemSlotContainer == null)
        {
            Debug.LogError("ItemInventoryUI is missing itemSlotContainer reference!", this);
            return;
        }
        
        HorizontalLayoutGroup layoutGroup = itemSlotContainer.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = itemSlotContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        }
        
        layoutGroup.spacing = slotSpacing;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;
    }
    
    /// <summary>
    /// Refresh the entire inventory display
    /// </summary>
    void RefreshInventoryDisplay()
    {
        ItemManager itemManager = ItemManager.Instance;
        if (itemManager == null) return;
        
        Dictionary<ItemData, int> inventory = itemManager.GetAllItems();
        
        // Remove slots for items no longer in inventory
        List<ItemData> toRemove = new List<ItemData>();
        foreach (var kvp in _activeSlots)
        {
            if (!inventory.ContainsKey(kvp.Key))
            {
                Destroy(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var item in toRemove)
        {
            _activeSlots.Remove(item);
        }
        
        // Update or create slots for items in inventory
        foreach (var kvp in inventory)
        {
            ItemData item = kvp.Key;
            int quantity = kvp.Value;
            
            if (_activeSlots.ContainsKey(item))
            {
                // Update existing slot
                UpdateItemSlot(_activeSlots[item], item, quantity);
            }
            else
            {
                // Create new slot
                GameObject slotObj = CreateItemSlot(item, quantity);
                _activeSlots[item] = slotObj;
            }
        }
        
        if (showDebugInfo)
            Debug.Log($"Inventory UI refreshed. Showing {_activeSlots.Count} items.");
    }
    
    /// <summary>
    /// Create a new item slot UI element
    /// </summary>
    GameObject CreateItemSlot(ItemData item, int quantity)
    {
        GameObject slotObj;
        
        if (itemSlotPrefab != null)
        {
            // Use prefab
            slotObj = Instantiate(itemSlotPrefab, itemSlotContainer);
        }
        else
        {
            // Create from scratch
            slotObj = new GameObject($"ItemSlot_{item.itemName}");
            slotObj.transform.SetParent(itemSlotContainer);
            
            // Add Image component for icon
            Image iconImage = slotObj.AddComponent<Image>();
            iconImage.sprite = item.icon;
            iconImage.preserveAspect = true;
            
            // Set size
            RectTransform rt = slotObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(slotSize, slotSize);
            
            // Add background panel
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(slotObj.transform);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            bgObj.transform.SetAsFirstSibling(); // Behind icon
            
            // Add stack count text
            GameObject textObj = new GameObject("StackCount");
            textObj.transform.SetParent(slotObj.transform);
            TextMeshProUGUI stackText = textObj.AddComponent<TextMeshProUGUI>();
            stackText.alignment = TextAlignmentOptions.BottomRight;
            stackText.fontSize = 18;
            stackText.fontStyle = FontStyles.Bold;
            stackText.color = Color.white;
            stackText.enableAutoSizing = false;
            
            // Position in bottom-right corner
            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0.5f, 0f);
            textRt.anchorMax = new Vector2(1f, 0.5f);
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
        }
        
        // Update slot content
        UpdateItemSlot(slotObj, item, quantity);
        
        return slotObj;
    }
    
    /// <summary>
    /// Update item slot with current quantity
    /// </summary>
    void UpdateItemSlot(GameObject slotObj, ItemData item, int quantity)
    {
        if (slotObj == null) return;
        
        // Update icon
        Image iconImage = slotObj.GetComponent<Image>();
        if (iconImage != null)
        {
            iconImage.sprite = item.icon;
        }
        
        // Update stack count text
        TextMeshProUGUI stackText = slotObj.GetComponentInChildren<TextMeshProUGUI>();
        if (stackText != null)
        {
            if (quantity > 1)
            {
                stackText.text = $"x{quantity}";
                stackText.gameObject.SetActive(true);
            }
            else
            {
                stackText.gameObject.SetActive(false);
            }
        }
        
        // Update background color to match rarity
        Transform bgTransform = slotObj.transform.Find("Background");
        if (bgTransform != null)
        {
            Image bgImage = bgTransform.GetComponent<Image>();
            if (bgImage != null)
            {
                Color rarityColor = item.GetRarityColor();
                rarityColor.a = 0.3f; // Semi-transparent
                bgImage.color = rarityColor;
            }
        }
    }
}
