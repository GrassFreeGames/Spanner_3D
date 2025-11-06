using UnityEngine;

/// <summary>
/// Manages player currency (credits).
/// Awards credits from enemy kills, used for shop purchases.
/// Place in scene under "--- MANAGERS ---"
/// </summary>
public class CurrencyManager : MonoBehaviour
{
    [Header("Starting Currency")]
    [Tooltip("Credits player starts with")]
    public int startingCredits = 0;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private fields: _camelCase
    private int _currentCredits;
    
    // Properties: PascalCase
    public int CurrentCredits => _currentCredits;
    
    // Events for UI updates
    public event System.Action<int> OnCreditsChanged;
    
    // Singleton pattern
    private static CurrencyManager _instance;
    public static CurrencyManager Instance => _instance;
    
    void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple CurrencyManager instances found! Destroying duplicate.", this);
            Destroy(this);
            return;
        }
        _instance = this;
    }
    
    void Start()
    {
        _currentCredits = startingCredits;
        
        if (showDebugInfo)
            Debug.Log($"CurrencyManager initialized. Starting credits: {_currentCredits}");
    }
    
    /// <summary>
    /// Add credits (e.g., from enemy kill)
    /// </summary>
    public void AddCredits(int amount)
    {
        if (amount <= 0) return;
        
        _currentCredits += amount;
        
        // Notify listeners
        OnCreditsChanged?.Invoke(_currentCredits);
        
        if (showDebugInfo)
            Debug.Log($"Gained {amount} credits. Total: {_currentCredits}");
    }
    
    /// <summary>
    /// Spend credits (e.g., shop purchase). Returns true if successful.
    /// </summary>
    public bool SpendCredits(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("Attempted to spend 0 or negative credits!");
            return false;
        }
        
        if (_currentCredits < amount)
        {
            if (showDebugInfo)
                Debug.Log($"Not enough credits! Need {amount}, have {_currentCredits}");
            return false;
        }
        
        _currentCredits -= amount;
        
        // Notify listeners
        OnCreditsChanged?.Invoke(_currentCredits);
        
        if (showDebugInfo)
            Debug.Log($"Spent {amount} credits. Remaining: {_currentCredits}");
        
        return true;
    }
    
    /// <summary>
    /// Check if player can afford an amount
    /// </summary>
    public bool CanAfford(int amount)
    {
        return _currentCredits >= amount;
    }
    
    /// <summary>
    /// Reset credits (for testing/new game)
    /// </summary>
    public void ResetCredits()
    {
        _currentCredits = startingCredits;
        OnCreditsChanged?.Invoke(_currentCredits);
        
        if (showDebugInfo)
            Debug.Log($"Credits reset to {_currentCredits}");
    }
}
