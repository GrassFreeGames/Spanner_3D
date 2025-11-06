using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages player item inventory and calculates total item bonuses.
/// Items stack multiplicatively with upgrades.
/// Place in scene under "--- MANAGERS ---"
/// </summary>
public class ItemManager : MonoBehaviour
{
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Item inventory: ItemData → quantity
    private Dictionary<ItemData, int> _inventory = new Dictionary<ItemData, int>();
    
    // Cached totals (recalculated when inventory changes)
    private float _totalAttackSpeedMultiplier = 1f;
    private float _totalMoveSpeedMultiplier = 1f;
    private float _totalDamageMultiplier = 1f;
    private float _totalProjectileSpeedMultiplier = 1f;
    private float _totalKnockbackMultiplier = 1f;
    private float _totalMaxHealthBonus = 0f;
    private float _totalHpRegenBonus = 0f;
    private float _totalPickupRangeMultiplier = 1f;
    private float _totalArmorBonus = 0f;
    private float _totalLifestealBonus = 0f;
    private float _totalCritChanceBonus = 0f;
    private float _totalCritDamageBonus = 0f;
    
    // Properties: PascalCase
    public float TotalAttackSpeedMultiplier => _totalAttackSpeedMultiplier;
    public float TotalMoveSpeedMultiplier => _totalMoveSpeedMultiplier;
    public float TotalDamageMultiplier => _totalDamageMultiplier;
    public float TotalProjectileSpeedMultiplier => _totalProjectileSpeedMultiplier;
    public float TotalKnockbackMultiplier => _totalKnockbackMultiplier;
    public float TotalMaxHealthBonus => _totalMaxHealthBonus;
    public float TotalHpRegenBonus => _totalHpRegenBonus;
    public float TotalPickupRangeMultiplier => _totalPickupRangeMultiplier;
    public float TotalArmorBonus => _totalArmorBonus;
    public float TotalLifestealBonus => _totalLifestealBonus;
    public float TotalCritChanceBonus => _totalCritChanceBonus;
    public float TotalCritDamageBonus => _totalCritDamageBonus;
    
    // Events for UI updates
    public event System.Action OnInventoryChanged;
    
    // Singleton pattern
    private static ItemManager _instance;
    public static ItemManager Instance => _instance;
    
    void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple ItemManager instances found! Destroying duplicate.", this);
            Destroy(this);
            return;
        }
        _instance = this;
    }
    
    /// <summary>
    /// Add an item to inventory
    /// </summary>
    public void AddItem(ItemData item)
    {
        if (item == null)
        {
            Debug.LogError("Attempted to add null item to inventory!");
            return;
        }
        
        // Add or increment
        if (_inventory.ContainsKey(item))
            _inventory[item]++;
        else
            _inventory[item] = 1;
        
        // Recalculate totals
        RecalculateTotals();
        
        // Apply immediate effects (health, pickup range, etc.)
        ApplyImmediateEffects(item);
        
        // Notify UI
        OnInventoryChanged?.Invoke();
        
        if (showDebugInfo)
            Debug.Log($"Added item: {item.itemName} (now have {_inventory[item]})");
    }
    
    /// <summary>
    /// Get quantity of specific item
    /// </summary>
    public int GetItemCount(ItemData item)
    {
        if (item == null) return 0;
        return _inventory.ContainsKey(item) ? _inventory[item] : 0;
    }
    
    /// <summary>
    /// Get all items in inventory
    /// </summary>
    public Dictionary<ItemData, int> GetAllItems()
    {
        return new Dictionary<ItemData, int>(_inventory);
    }
    
    /// <summary>
    /// Recalculate all stat totals from items
    /// Items stack multiplicatively: (1 + bonus1) * (1 + bonus2) * ...
    /// </summary>
    void RecalculateTotals()
    {
        // Reset to base values
        _totalAttackSpeedMultiplier = 1f;
        _totalMoveSpeedMultiplier = 1f;
        _totalDamageMultiplier = 1f;
        _totalProjectileSpeedMultiplier = 1f;
        _totalKnockbackMultiplier = 1f;
        _totalMaxHealthBonus = 0f;
        _totalHpRegenBonus = 0f;
        _totalPickupRangeMultiplier = 1f;
        _totalArmorBonus = 0f;
        _totalLifestealBonus = 0f;
        _totalCritChanceBonus = 0f;
        _totalCritDamageBonus = 0f;
        
        // Stack each item's effects
        foreach (var kvp in _inventory)
        {
            ItemData item = kvp.Key;
            int quantity = kvp.Value;
            
            // Apply each copy of the item
            for (int i = 0; i < quantity; i++)
            {
                // Multiplicative stacking: multiply each bonus
                _totalAttackSpeedMultiplier *= item.attackSpeedMultiplier;
                _totalMoveSpeedMultiplier *= item.moveSpeedMultiplier;
                _totalDamageMultiplier *= item.damageMultiplier;
                _totalProjectileSpeedMultiplier *= item.projectileSpeedMultiplier;
                _totalKnockbackMultiplier *= item.knockbackMultiplier;
                _totalPickupRangeMultiplier *= item.pickupRangeMultiplier;
                
                // Flat bonuses: add each copy
                _totalMaxHealthBonus += item.maxHealthBonus;
                _totalHpRegenBonus += item.hpRegenBonus;
                _totalArmorBonus += item.armorBonus;
                _totalLifestealBonus += item.lifestealBonus;
                _totalCritChanceBonus += item.critChanceBonus;
                _totalCritDamageBonus += item.critDamageBonus;
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Item Totals Recalculated:");
            Debug.Log($"  Attack Speed: {_totalAttackSpeedMultiplier:F2}x");
            Debug.Log($"  Move Speed: {_totalMoveSpeedMultiplier:F2}x");
            Debug.Log($"  Damage: {_totalDamageMultiplier:F2}x");
            Debug.Log($"  Max Health: +{_totalMaxHealthBonus}");
            Debug.Log($"  HP Regen: +{_totalHpRegenBonus}/s");
        }
    }
    
    /// <summary>
    /// Apply immediate effects when item is added (health, pickup range, etc.)
    /// </summary>
    void ApplyImmediateEffects(ItemData item)
    {
        PlayerStats playerStats = PlayerStats.Instance;
        
        if (playerStats != null)
        {
            // Max health increase
            if (item.maxHealthBonus > 0f)
            {
                playerStats.maxHealth += item.maxHealthBonus;
                playerStats.Heal(item.maxHealthBonus); // Heal the increased amount
            }
            
            // Pickup range (multiply by item's multiplier)
            if (item.pickupRangeMultiplier != 1f)
            {
                playerStats.pickupRadius *= item.pickupRangeMultiplier;
            }
        }
        
        // Movement speed (multiply by item's multiplier)
        if (item.moveSpeedMultiplier != 1f)
        {
            PlayerMovement3D movement = FindFirstObjectByType<PlayerMovement3D>();
            if (movement != null)
            {
                movement.moveSpeed *= item.moveSpeedMultiplier;
            }
        }
    }
    
    void Update()
    {
        // Apply HP regen from items
        if (_totalHpRegenBonus > 0f)
        {
            PlayerStats playerStats = PlayerStats.Instance;
            if (playerStats != null && playerStats.IsAlive)
            {
                playerStats.Heal(_totalHpRegenBonus * Time.deltaTime);
            }
        }
    }
    
    /// <summary>
    /// Clear all items (for testing/reset)
    /// </summary>
    public void ClearInventory()
    {
        _inventory.Clear();
        RecalculateTotals();
        OnInventoryChanged?.Invoke();
        
        if (showDebugInfo)
            Debug.Log("Inventory cleared");
    }
}
