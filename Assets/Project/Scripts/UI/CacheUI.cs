using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI for cache interface showing 3 free items.
/// Pauses game, shows items, allows one selection, auto-closes.
/// Attach to cache panel in Canvas.
/// </summary>
public class CacheUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Main cache panel (will be shown/hidden)")]
    public GameObject cachePanel;
    
    [Tooltip("Canvas Group for visibility control")]
    public CanvasGroup panelCanvasGroup;
    
    [Tooltip("Item option buttons (should have 3)")]
    public Button[] itemButtons;
    
    [Tooltip("Text components for item names")]
    public TextMeshProUGUI[] itemNameTexts;
    
    [Tooltip("Text components for item descriptions")]
    public TextMeshProUGUI[] itemDescTexts;
    
    [Tooltip("Image components for item icons")]
    public Image[] itemIcons;
    
    [Tooltip("Background images for rarity colors")]
    public Image[] itemBackgrounds;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private fields
    private ItemData[] _currentItems;
    private Cache _currentCache;
    
    // Singleton pattern
    private static CacheUI _instance;
    public static CacheUI Instance => _instance;
    
    void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple CacheUI instances found! Destroying duplicate.", this);
            Destroy(this);
            return;
        }
        _instance = this;
        
        // Get or add CanvasGroup
        if (cachePanel != null)
        {
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = cachePanel.GetComponent<CanvasGroup>();
                if (panelCanvasGroup == null)
                {
                    panelCanvasGroup = cachePanel.AddComponent<CanvasGroup>();
                }
            }
            
            // Hide initially
            HidePanelImmediate();
        }
    }
    
    void Start()
    {
        // Setup button listeners
        for (int i = 0; i < itemButtons.Length; i++)
        {
            int index = i; // Capture for closure
            itemButtons[i].onClick.AddListener(() => SelectItem(index));
        }
    }
    
    /// <summary>
    /// Show cache UI with 3 free item options
    /// </summary>
    public void ShowCache(ItemData[] items, Cache cache)
    {
        if (items == null || items.Length != 3)
        {
            Debug.LogError("CacheUI requires exactly 3 items!");
            return;
        }
        
        _currentItems = items;
        _currentCache = cache;
        
        // Pause game
        Time.timeScale = 0f;
        
        // Show and unlock cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Display items
        for (int i = 0; i < 3 && i < itemButtons.Length; i++)
        {
            ItemData item = items[i];
            
            // Set name
            if (i < itemNameTexts.Length && itemNameTexts[i] != null)
            {
                itemNameTexts[i].text = $"{item.itemName} ({item.GetRarityName()})";
            }
            
            // Set description
            if (i < itemDescTexts.Length && itemDescTexts[i] != null)
            {
                itemDescTexts[i].text = item.GetFormattedDescription();
            }
            
            // Set icon
            if (i < itemIcons.Length && itemIcons[i] != null)
            {
                itemIcons[i].sprite = item.icon;
            }
            
            // Set background color to rarity
            if (i < itemBackgrounds.Length && itemBackgrounds[i] != null)
            {
                itemBackgrounds[i].color = item.GetRarityColor();
            }
        }
        
        // Show panel
        ShowPanelImmediate();
        
        if (showDebugInfo)
            Debug.Log("Cache UI opened - game paused");
    }
    
    /// <summary>
    /// Handle item selection
    /// </summary>
    void SelectItem(int index)
    {
        if (index < 0 || index >= _currentItems.Length)
        {
            Debug.LogError($"Invalid item index: {index}");
            return;
        }
        
        ItemData selectedItem = _currentItems[index];
        
        // Add item to inventory (free!)
        ItemManager items = ItemManager.Instance;
        if (items != null)
        {
            items.AddItem(selectedItem);
        }
        
        if (showDebugInfo)
            Debug.Log($"Selected from cache: {selectedItem.itemName}");
        
        // Notify cache of selection
        if (_currentCache != null)
        {
            _currentCache.OnItemSelected();
        }
        
        // Close cache (auto-close after selection)
        CloseCache();
    }
    
    /// <summary>
    /// Close cache
    /// </summary>
    void CloseCache()
    {
        // Hide panel
        HidePanelImmediate();
        
        // Resume game
        Time.timeScale = 1f;
        
        // Hide and lock cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        if (showDebugInfo)
            Debug.Log("Cache UI closed - game resumed");
    }
    
    void HidePanelImmediate()
    {
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;
        }
    }
    
    void ShowPanelImmediate()
    {
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 1f;
            panelCanvasGroup.interactable = true;
            panelCanvasGroup.blocksRaycasts = true;
        }
    }
}
