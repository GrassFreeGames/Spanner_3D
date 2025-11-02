using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// UI for displaying upgrade choices when player levels up.
/// Pauses game and shows 3 random upgrade options.
/// Attach to upgrade panel in Canvas.
/// </summary>
public class UpgradeUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Main upgrade panel (will be shown/hidden)")]
    public GameObject upgradePanel;
    
    [Tooltip("Upgrade option buttons (should have 3)")]
    public Button[] upgradeButtons;
    
    [Tooltip("Text components for upgrade names")]
    public TextMeshProUGUI[] upgradeNameTexts;
    
    [Tooltip("Text components for upgrade descriptions")]
    public TextMeshProUGUI[] upgradeDescTexts;
    
    [Tooltip("Re-roll button (optional)")]
    public Button rerollButton;
    
    [Header("Optional Icons")]
    [Tooltip("Image components for upgrade icons (optional)")]
    public Image[] upgradeIcons;
    
    // Private fields: _camelCase
    private UpgradeDefinition[] _currentOptions;
    private UpgradeManager _upgradeManager;
    
    // Singleton pattern
    private static UpgradeUI _instance;
    public static UpgradeUI Instance => _instance;
    
    void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple UpgradeUI instances found! Destroying duplicate.", this);
            Destroy(this);
            return;
        }
        _instance = this;
        
        // CRITICAL: Hide panel immediately in Awake, even if enabled in Inspector
        // This prevents the disabled GameObject bug
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
    }
    
    void Start()
    {
        _upgradeManager = UpgradeManager.Instance;
        
        if (_upgradeManager == null)
        {
            Debug.LogError("UpgradeUI cannot find UpgradeManager!", this);
            enabled = false;
            return;
        }
        
        // Setup upgrade button listeners
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            int index = i; // Capture for closure
            upgradeButtons[i].onClick.AddListener(() => SelectUpgrade(index));
        }
        
        // Setup reroll button listener
        if (rerollButton != null)
        {
            rerollButton.onClick.AddListener(RerollUpgrades);
        }
    }
    
    /// <summary>
    /// Show upgrade screen with 3 random options
    /// </summary>
    public void ShowUpgradeScreen()
    {
        // Pause game
        Time.timeScale = 0f;
        
        // Show and unlock cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Generate 3 random unique upgrades
        _currentOptions = GetRandomUpgrades(3);
        
        // Display upgrades
        for (int i = 0; i < _currentOptions.Length && i < upgradeButtons.Length; i++)
        {
            UpgradeDefinition upgrade = _currentOptions[i];
            int currentLevel = _upgradeManager.GetUpgradeLevel(upgrade.type);
            
            // Set name
            if (i < upgradeNameTexts.Length && upgradeNameTexts[i] != null)
            {
                upgradeNameTexts[i].text = upgrade.displayName;
            }
            
            // Set description with current level
            if (i < upgradeDescTexts.Length && upgradeDescTexts[i] != null)
            {
                upgradeDescTexts[i].text = upgrade.GetDescription(currentLevel);
            }
            
            // Set icon if available
            if (i < upgradeIcons.Length && upgradeIcons[i] != null && upgrade.icon != null)
            {
                upgradeIcons[i].sprite = upgrade.icon;
                upgradeIcons[i].enabled = true;
            }
            else if (i < upgradeIcons.Length && upgradeIcons[i] != null)
            {
                upgradeIcons[i].enabled = false;
            }
        }
        
        // Show panel - CRITICAL: SetActive must be called after all setup
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(true);
        }
        else
        {
            Debug.LogError("UpgradePanel is null! Cannot show upgrade screen.", this);
        }
        
        Debug.Log("Upgrade screen shown - game paused, cursor unlocked");
    }
    
    /// <summary>
    /// Hide upgrade screen and resume game
    /// </summary>
    public void HideUpgradeScreen()
    {
        // Hide panel
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
        
        // Resume game
        Time.timeScale = 1f;
        
        // Hide and lock cursor for gameplay
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        Debug.Log("Upgrade screen hidden - game resumed, cursor locked");
    }
    
    /// <summary>
    /// Handle upgrade selection
    /// </summary>
    void SelectUpgrade(int index)
    {
        if (index < 0 || index >= _currentOptions.Length)
        {
            Debug.LogError($"Invalid upgrade index: {index}");
            return;
        }
        
        UpgradeDefinition selectedUpgrade = _currentOptions[index];
        
        // Apply upgrade
        _upgradeManager.ApplyUpgrade(selectedUpgrade.type);
        
        Debug.Log($"Selected upgrade: {selectedUpgrade.displayName}");
        
        // Hide screen and resume game
        HideUpgradeScreen();
    }
    
    /// <summary>
    /// Re-roll upgrade choices
    /// </summary>
    void RerollUpgrades()
    {
        // Simply regenerate and display new options
        // Game stays paused, cursor stays visible
        _currentOptions = GetRandomUpgrades(3);
        
        // Update displayed upgrades
        for (int i = 0; i < _currentOptions.Length && i < upgradeButtons.Length; i++)
        {
            UpgradeDefinition upgrade = _currentOptions[i];
            int currentLevel = _upgradeManager.GetUpgradeLevel(upgrade.type);
            
            // Set name
            if (i < upgradeNameTexts.Length && upgradeNameTexts[i] != null)
            {
                upgradeNameTexts[i].text = upgrade.displayName;
            }
            
            // Set description with current level
            if (i < upgradeDescTexts.Length && upgradeDescTexts[i] != null)
            {
                upgradeDescTexts[i].text = upgrade.GetDescription(currentLevel);
            }
            
            // Set icon if available
            if (i < upgradeIcons.Length && upgradeIcons[i] != null && upgrade.icon != null)
            {
                upgradeIcons[i].sprite = upgrade.icon;
                upgradeIcons[i].enabled = true;
            }
            else if (i < upgradeIcons.Length && upgradeIcons[i] != null)
            {
                upgradeIcons[i].enabled = false;
            }
        }
        
        Debug.Log("Upgrades re-rolled");
    }
    
    /// <summary>
    /// Get N random unique upgrades
    /// </summary>
    UpgradeDefinition[] GetRandomUpgrades(int count)
    {
        UpgradeDefinition[] allUpgrades = UpgradeDefinition.GetAllUpgrades();
        
        // Shuffle array
        System.Random rng = new System.Random();
        for (int i = allUpgrades.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            UpgradeDefinition temp = allUpgrades[i];
            allUpgrades[i] = allUpgrades[j];
            allUpgrades[j] = temp;
        }
        
        // Take first N upgrades (guaranteed unique due to shuffle)
        UpgradeDefinition[] result = new UpgradeDefinition[Mathf.Min(count, allUpgrades.Length)];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = allUpgrades[i];
        }
        
        return result;
    }
}
