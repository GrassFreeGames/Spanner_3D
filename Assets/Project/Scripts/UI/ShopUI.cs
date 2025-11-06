using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI for shop interface showing 3 purchasable items.
/// Pauses game, shows items with costs, allows purchase or close.
/// Attach to shop panel in Canvas.
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Main shop panel (will be shown/hidden)")]
    public GameObject shopPanel;
    
    [Tooltip("Canvas Group for visibility control")]
    public CanvasGroup panelCanvasGroup;
    
    [Tooltip("Item option buttons (should have 3)")]
    public Button[] itemButtons;
    
    [Tooltip("Text components for item names")]
    public TextMeshProUGUI[] itemNameTexts;
    
    [Tooltip("Text components for item descriptions")]
    public TextMeshProUGUI[] itemDescTexts;
    
    [Tooltip("Text components for item costs")]
    public TextMeshProUGUI[] itemCostTexts;
    
    [Tooltip("Image components for item icons")]
    public Image[] itemIcons;
    
    [Tooltip("Background images for rarity colors")]
    public Image[] itemBackgrounds;
    
    [Tooltip("Close button")]
    public Button closeButton;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private fields
    private ItemData[] _currentItems;
    private Shop _currentShop;
    
    // Singleton pattern
    private static ShopUI _instance;
    public static ShopUI Instance => _instance;
    
    void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple ShopUI instances found! Destroying duplicate.", this);
            Destroy(this);
            return;
        }
        _instance = this;
        
        // Get or add CanvasGroup
        if (shopPanel != null)
        {
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = shopPanel.GetComponent<CanvasGroup>();
                if (panelCanvasGroup == null)
                {
                    panelCanvasGroup = shopPanel.AddComponent<CanvasGroup>();
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
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseShop);
        }
    }
    
    /// <summary>
    /// Show shop UI with 3 item options
    /// </summary>
    public void ShowShop(ItemData[] items, Shop shop)
    {
        if (items == null || items.Length != 3)
        {
            Debug.LogError("ShopUI requires exactly 3 items!");
            return;
        }
        
        _currentItems = items;
        _currentShop = shop;
        
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
            
            // Set cost
            if (i < itemCostTexts.Length && itemCostTexts[i] != null)
            {
                itemCostTexts[i].text = $"Cost: {item.shopCost} Credits";
                
                // Check if player can afford
                CurrencyManager currency = CurrencyManager.Instance;
                if (currency != null && !currency.CanAfford(item.shopCost))
                {
                    itemCostTexts[i].color = Color.red;
                }
                else
                {
                    itemCostTexts[i].color = Color.white;
                }
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
            
            // Update button interactability
            CurrencyManager currencyCheck = CurrencyManager.Instance;
            if (itemButtons[i] != null && currencyCheck != null)
            {
                itemButtons[i].interactable = currencyCheck.CanAfford(item.shopCost);
            }
        }
        
        // Show panel
        ShowPanelImmediate();
        
        if (showDebugInfo)
            Debug.Log("Shop UI opened - game paused");
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
        
        // Check if player can afford
        CurrencyManager currency = CurrencyManager.Instance;
        if (currency == null || !currency.CanAfford(selectedItem.shopCost))
        {
            if (showDebugInfo)
                Debug.Log($"Cannot afford {selectedItem.itemName}");
            return;
        }
        
        // Purchase item
        if (currency.SpendCredits(selectedItem.shopCost))
        {
            // Add item to inventory
            ItemManager items = ItemManager.Instance;
            if (items != null)
            {
                items.AddItem(selectedItem);
            }
            
            if (showDebugInfo)
                Debug.Log($"Purchased: {selectedItem.itemName} for {selectedItem.shopCost} credits");
            
            // Notify shop of purchase
            if (_currentShop != null)
            {
                _currentShop.OnItemPurchased();
            }
            
            // Close shop
            CloseShop();
        }
    }
    
    /// <summary>
    /// Close shop without purchasing
    /// </summary>
    void CloseShop()
    {
        // Hide panel
        HidePanelImmediate();
        
        // Resume game
        Time.timeScale = 1f;
        
        // Hide and lock cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        if (showDebugInfo)
            Debug.Log("Shop UI closed - game resumed");
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
