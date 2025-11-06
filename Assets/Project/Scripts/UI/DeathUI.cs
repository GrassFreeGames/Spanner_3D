using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Displays "You Died" screen with "Okay :(" button.
/// Attach to death panel UI element.
/// </summary>
public class DeathUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Death panel (will be shown/hidden)")]
    public GameObject deathPanel;
    
    [Tooltip("Canvas Group for visibility control")]
    public CanvasGroup panelCanvasGroup;
    
    [Tooltip("'You Died' text")]
    public TextMeshProUGUI deathText;
    
    [Tooltip("Okay button")]
    public Button okayButton;
    
    [Tooltip("Okay button text (to set ':(' face)")]
    public TextMeshProUGUI okayButtonText;
    
    [Header("Items Display")]
    [Tooltip("Panel showing items collected during run")]
    public GameObject itemsPanel;
    
    [Tooltip("Container for item slots (should have VerticalLayoutGroup)")]
    public Transform itemsContainer;
    
    [Tooltip("Header text for items section")]
    public TextMeshProUGUI itemsHeaderText;
    
    [Header("Layout Settings")]
    [Tooltip("Size of item icons")]
    public float itemIconSize = 48f;
    
    [Tooltip("Spacing between items")]
    public float itemSpacing = 5f;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Singleton pattern
    private static DeathUI _instance;
    public static DeathUI Instance => _instance;
    
    void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple DeathUI instances found! Destroying duplicate.", this);
            Destroy(this);
            return;
        }
        _instance = this;
        
        // Get or add CanvasGroup
        if (deathPanel != null)
        {
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = deathPanel.GetComponent<CanvasGroup>();
                if (panelCanvasGroup == null)
                {
                    panelCanvasGroup = deathPanel.AddComponent<CanvasGroup>();
                }
            }
            
            // Hide initially
            HidePanelImmediate();
        }
    }
    
    void Start()
    {
        // Setup button listener
        if (okayButton != null)
        {
            okayButton.onClick.AddListener(OnOkayClicked);
        }
        else
        {
            Debug.LogError("DeathUI is missing Okay button reference!", this);
        }
        
        // Set button text
        if (okayButtonText != null)
        {
            okayButtonText.text = "Okay :(";
        }
        
        // Set death text
        if (deathText != null)
        {
            deathText.text = "You Died";
        }
        
        // Set items header text
        if (itemsHeaderText != null)
        {
            itemsHeaderText.text = "Items Collected:";
        }
        
        // Setup items container layout
        if (itemsContainer != null)
        {
            VerticalLayoutGroup layoutGroup = itemsContainer.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = itemsContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            }
            
            layoutGroup.spacing = itemSpacing;
            layoutGroup.childAlignment = TextAnchor.UpperLeft;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
        }
    }
    
    /// <summary>
    /// Show death screen
    /// </summary>
    public void ShowDeathScreen()
    {
        // Populate items display
        PopulateItemsDisplay();
        
        // Show panel
        ShowPanelImmediate();
        
        // Unlock and show cursor for button interaction
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        if (showDebugInfo)
            Debug.Log("Death screen shown - cursor unlocked and visible");
    }
    
    /// <summary>
    /// Hide death screen
    /// </summary>
    public void HideDeathScreen()
    {
        HidePanelImmediate();
        
        if (showDebugInfo)
            Debug.Log("Death screen hidden");
    }
    
    /// <summary>
    /// Handle "Okay :(" button click
    /// </summary>
    void OnOkayClicked()
    {
        if (showDebugInfo)
            Debug.Log("Okay button clicked - showing post-game stats");
        
        // Stop camera orbit
        DeathController deathController = DeathController.Instance;
        if (deathController != null)
        {
            deathController.StopCameraOrbit();
        }
        
        // Hide death screen
        HideDeathScreen();
        
        // Game stays paused (Time.timeScale = 0) for stats screen
        
        // Show post-game stats
        PostGameStatsUI statsUI = PostGameStatsUI.Instance;
        if (statsUI != null)
        {
            statsUI.ShowStatsScreen();
        }
        else
        {
            Debug.LogError("DeathUI cannot find PostGameStatsUI!", this);
        }
    }
    
    /// <summary>
    /// Populate the items display with collected items
    /// </summary>
    void PopulateItemsDisplay()
    {
        if (itemsContainer == null)
        {
            if (showDebugInfo)
                Debug.LogWarning("DeathUI has no items container assigned, skipping items display");
            return;
        }
        
        // Clear existing items
        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Get items from ItemManager
        ItemManager itemManager = ItemManager.Instance;
        if (itemManager == null)
        {
            if (showDebugInfo)
                Debug.LogWarning("DeathUI cannot find ItemManager, no items to display");
            
            // Hide items panel if no items
            if (itemsPanel != null)
                itemsPanel.SetActive(false);
            
            return;
        }
        
        Dictionary<ItemData, int> items = itemManager.GetAllItems();
        
        // If no items collected, hide panel or show "None" message
        if (items.Count == 0)
        {
            if (itemsPanel != null)
                itemsPanel.SetActive(true);
            
            // Create "None" text
            GameObject noneObj = new GameObject("NoItemsText");
            noneObj.transform.SetParent(itemsContainer);
            
            TextMeshProUGUI noneText = noneObj.AddComponent<TextMeshProUGUI>();
            noneText.text = "None";
            noneText.fontSize = 20;
            noneText.color = new Color(0.7f, 0.7f, 0.7f);
            noneText.alignment = TextAlignmentOptions.Left;
            
            return;
        }
        
        // Show items panel
        if (itemsPanel != null)
            itemsPanel.SetActive(true);
        
        // Create item entries
        foreach (var kvp in items)
        {
            ItemData item = kvp.Key;
            int quantity = kvp.Value;
            
            CreateItemEntry(item, quantity);
        }
        
        if (showDebugInfo)
            Debug.Log($"DeathUI populated with {items.Count} item types");
    }
    
    /// <summary>
    /// Create a single item entry in the list
    /// </summary>
    void CreateItemEntry(ItemData item, int quantity)
    {
        // Create container for this item
        GameObject entryObj = new GameObject($"Item_{item.itemName}");
        entryObj.transform.SetParent(itemsContainer);
        
        // Add horizontal layout for icon + text
        HorizontalLayoutGroup layout = entryObj.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        
        // Create icon
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(entryObj.transform);
        
        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.sprite = item.icon;
        iconImage.preserveAspect = true;
        
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(itemIconSize, itemIconSize);
        
        // Create background for icon with rarity color
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(iconObj.transform);
        
        Image bgImage = bgObj.AddComponent<Image>();
        Color rarityColor = item.GetRarityColor();
        rarityColor.a = 0.3f;
        bgImage.color = rarityColor;
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgObj.transform.SetAsFirstSibling();
        
        // Create text (name + stack count)
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(entryObj.transform);
        
        TextMeshProUGUI itemText = textObj.AddComponent<TextMeshProUGUI>();
        if (quantity > 1)
        {
            itemText.text = $"{item.itemName} <color=#FFD700>x{quantity}</color>";
        }
        else
        {
            itemText.text = item.itemName;
        }
        itemText.fontSize = 20;
        itemText.color = Color.white;
        itemText.alignment = TextAlignmentOptions.Left;
        
        // Set layout element for proper sizing
        LayoutElement layoutElement = entryObj.AddComponent<LayoutElement>();
        layoutElement.minHeight = itemIconSize;
        layoutElement.preferredHeight = itemIconSize;
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
