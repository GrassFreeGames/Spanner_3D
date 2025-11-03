using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays post-game statistics screen.
/// Shows time alive, kills, level, and upgrade details.
/// Attach to stats panel UI element.
/// </summary>
public class PostGameStatsUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Stats panel (will be shown/hidden)")]
    public GameObject statsPanel;
    
    [Tooltip("Canvas Group for visibility control")]
    public CanvasGroup panelCanvasGroup;
    
    [Header("Stat Text Fields")]
    [Tooltip("Time alive text")]
    public TextMeshProUGUI timeAliveText;
    
    [Tooltip("Enemies killed text")]
    public TextMeshProUGUI killsText;
    
    [Tooltip("Player level text")]
    public TextMeshProUGUI levelText;
    
    [Tooltip("Upgrades summary text")]
    public TextMeshProUGUI upgradesText;
    
    [Header("Buttons")]
    [Tooltip("Main menu button (optional, for future)")]
    public Button mainMenuButton;
    
    [Tooltip("Restart button (optional, for future)")]
    public Button restartButton;
    
    // Singleton pattern
    private static PostGameStatsUI _instance;
    public static PostGameStatsUI Instance => _instance;
    
    void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple PostGameStatsUI instances found! Destroying duplicate.", this);
            Destroy(this);
            return;
        }
        _instance = this;
        
        // Get or add CanvasGroup
        if (statsPanel != null)
        {
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = statsPanel.GetComponent<CanvasGroup>();
                if (panelCanvasGroup == null)
                {
                    panelCanvasGroup = statsPanel.AddComponent<CanvasGroup>();
                }
            }
            
            // Hide initially
            HidePanelImmediate();
        }
    }
    
    void Start()
    {
        // Setup button listeners (for future use)
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }
    }
    
    /// <summary>
    /// Show stats screen with current game statistics
    /// </summary>
    public void ShowStatsScreen()
    {
        // Gather stats
        GameStatsTracker stats = GameStatsTracker.Instance;
        ExperienceManager exp = ExperienceManager.Instance;
        UpgradeManager upgrades = UpgradeManager.Instance;
        
        // Time alive
        if (timeAliveText != null && stats != null)
        {
            int minutes = Mathf.FloorToInt(stats.TimeAlive / 60f);
            int seconds = Mathf.FloorToInt(stats.TimeAlive % 60f);
            timeAliveText.text = $"Time Alive: {minutes:00}:{seconds:00}";
        }
        
        // Kills
        if (killsText != null && stats != null)
        {
            killsText.text = $"Enemies Killed: {stats.EnemiesKilled}";
        }
        
        // Level
        if (levelText != null && exp != null)
        {
            levelText.text = $"Reached Level: {exp.CurrentLevel}";
        }
        
        // Upgrades summary
        if (upgradesText != null && upgrades != null)
        {
            upgradesText.text = BuildUpgradesSummary(upgrades);
        }
        
        // Show panel
        ShowPanelImmediate();
        
        // Cursor already unlocked from death screen
        
        Debug.Log("Post-game stats screen shown");
    }
    
    /// <summary>
    /// Build upgrades summary text
    /// </summary>
    string BuildUpgradesSummary(UpgradeManager upgrades)
    {
        string summary = "Upgrades:\n";
        
        // List all upgrades that were taken
        if (upgrades.MoveSpeedLevel > 0)
            summary += $"Movement Speed: Lv{upgrades.MoveSpeedLevel}\n";
        
        if (upgrades.AttackRateLevel > 0)
            summary += $"Attack Speed: Lv{upgrades.AttackRateLevel}\n";
        
        if (upgrades.ProjectileSpeedLevel > 0)
            summary += $"Projectile Speed: Lv{upgrades.ProjectileSpeedLevel}\n";
        
        if (upgrades.HpRegenLevel > 0)
            summary += $"HP Regen: Lv{upgrades.HpRegenLevel}\n";
        
        if (upgrades.HpTotalLevel > 0)
            summary += $"Max Health: Lv{upgrades.HpTotalLevel}\n";
        
        if (upgrades.KnockbackLevel > 0)
            summary += $"Knockback: Lv{upgrades.KnockbackLevel}\n";
        
        if (upgrades.DamageLevel > 0)
            summary += $"Damage: Lv{upgrades.DamageLevel}\n";
        
        if (upgrades.PickupRangeLevel > 0)
            summary += $"Pickup Range: Lv{upgrades.PickupRangeLevel}\n";
        
        if (upgrades.ArmorLevel > 0)
            summary += $"Armor: Lv{upgrades.ArmorLevel} ({upgrades.ArmorPercent}%)\n";
        
        if (upgrades.LifestealLevel > 0)
            summary += $"Lifesteal: Lv{upgrades.LifestealLevel} ({upgrades.LifestealPercent}%)\n";
        
        if (upgrades.CritChanceLevel > 0)
            summary += $"Crit Chance: Lv{upgrades.CritChanceLevel} ({upgrades.CritChancePercent}%)\n";
        
        if (upgrades.CritDamageLevel > 0)
            summary += $"Crit Damage: Lv{upgrades.CritDamageLevel} ({upgrades.CritDamageMultiplier:F2}x)\n";
        
        // If no upgrades taken
        if (summary == "Upgrades:\n")
            summary += "None taken";
        
        return summary;
    }
    
    /// <summary>
    /// Hide stats screen
    /// </summary>
    public void HideStatsScreen()
    {
        HidePanelImmediate();
    }
    
    void OnMainMenuClicked()
    {
        Debug.Log("Main Menu button clicked (not yet implemented)");
        // TODO: Load main menu scene
    }
    
    void OnRestartClicked()
    {
        Debug.Log("Restart button clicked (not yet implemented)");
        // TODO: Reload current scene
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
