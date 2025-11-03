using UnityEngine;

/// <summary>
/// Manages player experience, leveling, and XP requirements.
/// Place in scene under "--- MANAGERS ---"
/// </summary>
public class ExperienceManager : MonoBehaviour
{
    [Header("Level Settings")]
    [Tooltip("Starting level")]
    public int startingLevel = 1;
    
    [Tooltip("XP required to reach level 2")]
    public float baseExpRequirement = 15f;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private fields: _camelCase
    private int _currentLevel;
    private float _currentExp;
    private float _expToNextLevel;
    
    // Properties: PascalCase
    public int CurrentLevel => _currentLevel;
    public float CurrentExp => _currentExp;
    public float ExpToNextLevel => _expToNextLevel;
    public float ExpProgress => _currentExp / _expToNextLevel; // 0-1 for UI bar
    
    // Singleton pattern
    private static ExperienceManager _instance;
    public static ExperienceManager Instance => _instance;
    
    void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple ExperienceManager instances found! Destroying duplicate.", this);
            Destroy(this);
            return;
        }
        _instance = this;
    }
    
    void Start()
    {
        _currentLevel = startingLevel;
        _currentExp = 0f;
        _expToNextLevel = baseExpRequirement;
        
        if (showDebugInfo)
            Debug.Log($"ExperienceManager initialized. Level {_currentLevel}, need {_expToNextLevel} XP to level up.");
    }
    
    /// <summary>
    /// Add experience points to player
    /// </summary>
    public void AddExperience(float amount)
    {
        _currentExp += amount;
        
        if (showDebugInfo)
            Debug.Log($"Gained {amount} XP. Current: {_currentExp}/{_expToNextLevel}");
        
        // Check for level up
        while (_currentExp >= _expToNextLevel)
        {
            LevelUp();
        }
    }
    
    /// <summary>
    /// Level up the player
    /// </summary>
    void LevelUp()
    {
        // Subtract XP cost
        _currentExp -= _expToNextLevel;
        
        // Increase level
        _currentLevel++;
        
        // Calculate next level requirement
        // Formula: (currentLevelReq * 1.05) + 5
        _expToNextLevel = (_expToNextLevel * 1.05f) + 5f;
        _expToNextLevel = Mathf.Round(_expToNextLevel); // Round to whole number
        
        if (showDebugInfo)
            Debug.Log($"LEVEL UP! Now level {_currentLevel}. Need {_expToNextLevel} XP for next level.");
        
        // TODO: Trigger level up rewards/upgrades here
        OnLevelUp();
    }
    
    /// <summary>
    /// Called when player levels up. Hook for upgrade system.
    /// </summary>
    void OnLevelUp()
    {
        // Show upgrade selection UI
        UpgradeUI upgradeUI = UpgradeUI.Instance;
        if (upgradeUI != null)
        {
            upgradeUI.ShowUpgradeScreen();
        }
        else
        {
            Debug.LogWarning("No UpgradeUI found! Player leveled up but can't select upgrade.");
        }
        
        Debug.Log($"🎉 Level {_currentLevel} reached!");
    }
    
    /// <summary>
    /// Get XP requirement for a specific level
    /// </summary>
    public float GetExpRequirementForLevel(int level)
    {
        if (level <= 1) return 0f;
        
        float requirement = baseExpRequirement;
        for (int i = 2; i < level; i++)
        {
            requirement = (requirement * 1.02f) + 2f;
            requirement = Mathf.Round(requirement);
        }
        
        return requirement;
    }
}
