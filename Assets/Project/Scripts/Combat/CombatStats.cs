using UnityEngine;

/// <summary>
/// Calculates final combat stats by applying player modifiers to base weapon stats.
/// Follows damage calculation order: (Base + Flat) × Increased × Items × Crit
/// Items and upgrades stack multiplicatively.
/// Never modifies ScriptableObject data.
/// </summary>
public class CombatStats : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Base attack data (never modified)")]
    public AttackData baseAttack;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Singleton pattern
    private static CombatStats _instance;
    public static CombatStats Instance => _instance;
    
    void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple CombatStats instances found! Destroying duplicate.", this);
            Destroy(this);
            return;
        }
        _instance = this;
    }
    
    /// <summary>
    /// Get final damage after all modifiers
    /// Formula: (Base + Flat) × Increased × Items × Crit
    /// </summary>
    public float GetFinalDamage()
    {
        if (baseAttack == null) return 0f;
        
        UpgradeManager upgrades = UpgradeManager.Instance;
        ItemManager items = ItemManager.Instance;
        
        if (upgrades == null) return baseAttack.damage;
        
        // Base damage from weapon
        float baseDamage = baseAttack.damage;
        
        // Flat additions (future: +5 damage items)
        float flatBonus = 0f;
        
        // Increased damage from upgrades (additive: +20%, +20% = +40%)
        float increasedMultiplier = 1f + upgrades.GetDamageMultiplier();
        
        // Item multiplier (multiplicative stacking)
        float itemMultiplier = items != null ? items.TotalDamageMultiplier : 1f;
        
        // Calculate: (Base + Flat) × Increased × Items
        float finalDamage = (baseDamage + flatBonus) * increasedMultiplier * itemMultiplier;
        
        if (showDebugInfo)
            Debug.Log($"Final Damage: {finalDamage} = ({baseDamage} + {flatBonus}) × {increasedMultiplier} × {itemMultiplier}");
        
        return finalDamage;
    }
    
    /// <summary>
    /// Get final attack rate (attacks per second)
    /// </summary>
    public float GetFinalAttackRate()
    {
        if (baseAttack == null) return 0f;
        
        UpgradeManager upgrades = UpgradeManager.Instance;
        ItemManager items = ItemManager.Instance;
        
        if (upgrades == null) return baseAttack.attacksPerSecond;
        
        float baseRate = baseAttack.attacksPerSecond;
        float increasedMultiplier = 1f + upgrades.GetAttackSpeedMultiplier();
        float itemMultiplier = items != null ? items.TotalAttackSpeedMultiplier : 1f;
        
        return baseRate * increasedMultiplier * itemMultiplier;
    }
    
    /// <summary>
    /// Get final projectile speed
    /// </summary>
    public float GetFinalProjectileSpeed()
    {
        if (baseAttack == null) return 0f;
        
        UpgradeManager upgrades = UpgradeManager.Instance;
        ItemManager items = ItemManager.Instance;
        
        if (upgrades == null) return baseAttack.projectileSpeed;
        
        float baseSpeed = baseAttack.projectileSpeed;
        float increasedMultiplier = 1f + upgrades.GetProjectileSpeedMultiplier();
        float itemMultiplier = items != null ? items.TotalProjectileSpeedMultiplier : 1f;
        
        return baseSpeed * increasedMultiplier * itemMultiplier;
    }
    
    /// <summary>
    /// Get final knockback force
    /// </summary>
    public float GetFinalKnockback()
    {
        if (baseAttack == null) return 0f;
        
        UpgradeManager upgrades = UpgradeManager.Instance;
        ItemManager items = ItemManager.Instance;
        
        if (upgrades == null) return baseAttack.knockbackForce;
        
        float baseKnockback = baseAttack.knockbackForce;
        float increasedMultiplier = 1f + upgrades.GetKnockbackMultiplier();
        float itemMultiplier = items != null ? items.TotalKnockbackMultiplier : 1f;
        
        return baseKnockback * increasedMultiplier * itemMultiplier;
    }
    
    /// <summary>
    /// Roll for crit and return final damage (with crit if applicable)
    /// Includes item bonuses to crit chance and damage
    /// </summary>
    public float RollDamageWithCrit(out bool isCrit)
    {
        float baseDamage = GetFinalDamage();
        
        UpgradeManager upgrades = UpgradeManager.Instance;
        ItemManager items = ItemManager.Instance;
        
        // Calculate total crit chance (upgrades + items)
        float critChance = 0f;
        if (upgrades != null)
            critChance += upgrades.CritChancePercent;
        if (items != null)
            critChance += items.TotalCritChanceBonus;
        
        // Roll for crit
        if (critChance > 0f && Random.Range(0f, 100f) < critChance)
        {
            isCrit = true;
            
            // Calculate total crit damage (upgrades + items)
            float critMultiplier = 1f;
            if (upgrades != null)
                critMultiplier = upgrades.CritDamageMultiplier;
            if (items != null)
                critMultiplier += items.TotalCritDamageBonus;
            
            return baseDamage * critMultiplier;
        }
        
        isCrit = false;
        return baseDamage;
    }
}
