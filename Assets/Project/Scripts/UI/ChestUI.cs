using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI for chest reveal showing the rolled item.
/// Displays item after chest animation, allows collection.
/// Attach to chest panel in Canvas.
/// </summary>
public class ChestUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Main chest panel (will be shown/hidden)")]
    public GameObject chestPanel;
    
    [Tooltip("Canvas Group for visibility control")]
    public CanvasGroup panelCanvasGroup;
    
    [Tooltip("Item name text")]
    public TextMeshProUGUI itemNameText;
    
    [Tooltip("Item description text")]
    public TextMeshProUGUI itemDescText;
    
    [Tooltip("Item icon")]
    public Image itemIcon;
    
    [Tooltip("Background image for rarity color")]
    public Image itemBackground;
    
    [Tooltip("Collect button")]
    public Button collectButton;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private fields
    private ItemData _currentItem;
    
    // Singleton pattern
    private static ChestUI _instance;
    public static ChestUI Instance => _instance;
    
    void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple ChestUI instances found! Destroying duplicate.", this);
            Destroy(this);
            return;
        }
        _instance = this;
        
        // Get or add CanvasGroup
        if (chestPanel != null)
        {
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = chestPanel.GetComponent<CanvasGroup>();
                if (panelCanvasGroup == null)
                {
                    panelCanvasGroup = chestPanel.AddComponent<CanvasGroup>();
                }
            }
            
            // Hide initially
            HidePanelImmediate();
        }
    }
    
    void Start()
    {
        // Setup button listener
        if (collectButton != null)
        {
            collectButton.onClick.AddListener(CollectItem);
        }
    }
    
    /// <summary>
    /// Show chest UI with revealed item
    /// </summary>
    public void ShowChest(ItemData item)
    {
        if (item == null)
        {
            Debug.LogError("ChestUI received null item!");
            return;
        }
        
        _currentItem = item;
        
        // Pause game
        Time.timeScale = 0f;
        
        // Show and unlock cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Display item
        if (itemNameText != null)
        {
            itemNameText.text = $"{item.itemName} ({item.GetRarityName()})";
        }
        
        if (itemDescText != null)
        {
            itemDescText.text = item.GetFormattedDescription();
        }
        
        if (itemIcon != null)
        {
            itemIcon.sprite = item.icon;
        }
        
        if (itemBackground != null)
        {
            itemBackground.color = item.GetRarityColor();
        }
        
        // Show panel
        ShowPanelImmediate();
        
        if (showDebugInfo)
            Debug.Log($"Chest UI opened with {item.itemName} - game paused");
    }
    
    /// <summary>
    /// Handle collect button
    /// </summary>
    void CollectItem()
    {
        if (_currentItem == null)
        {
            Debug.LogError("No item to collect!");
            return;
        }
        
        // Add item to inventory
        ItemManager items = ItemManager.Instance;
        if (items != null)
        {
            items.AddItem(_currentItem);
        }
        
        if (showDebugInfo)
            Debug.Log($"Collected from chest: {_currentItem.itemName}");
        
        // Close chest
        CloseChest();
    }
    
    /// <summary>
    /// Close chest UI
    /// </summary>
    void CloseChest()
    {
        // Hide panel
        HidePanelImmediate();
        
        // Resume game
        Time.timeScale = 1f;
        
        // Hide and lock cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        if (showDebugInfo)
            Debug.Log("Chest UI closed - game resumed");
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
