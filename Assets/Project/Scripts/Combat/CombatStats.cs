using UnityEngine;

/// <summary>
/// Calculates final combat stats by applying player modifiers to base weapon stats.
/// Follows damage calculation order: (Base + Flat) × Increased × More × Crit
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
    /// Formula: (Base + Flat) × Increased × Crit
    /// </summary>
    public float GetFinalDamage()
    {
        if (baseAttack == null) return 0f;
        
        UpgradeManager upgrades = UpgradeManager.Instance;
        if (upgrades == null) return baseAttack.damage;
        
        // Base damage from weapon
        float baseDamage = baseAttack.damage;
        
        // Flat additions (future: +5 damage items)
        float flatBonus = 0f;
        
        // Increased damage (additive: +20%, +20% = +40%)
        float increasedMultiplier = 1f + upgrades.GetDamageMultiplier();
        
        // Calculate: (Base + Flat) × Increased
        float finalDamage = (baseDamage + flatBonus) * increasedMultiplier;
        
        if (showDebugInfo)
            Debug.Log($"Final Damage: {finalDamage} = ({baseDamage} + {flatBonus}) × {increasedMultiplier}");
        
        return finalDamage;
    }
    
    /// <summary>
    /// Get final attack rate (attacks per second)
    /// </summary>
    public float GetFinalAttackRate()
    {
        if (baseAttack == null) return 0f;
        
        UpgradeManager upgrades = UpgradeManager.Instance;
        if (upgrades == null) return baseAttack.attacksPerSecond;
        
        float baseRate = baseAttack.attacksPerSecond;
        float increasedMultiplier = 1f + upgrades.GetAttackSpeedMultiplier();
        
        return baseRate * increasedMultiplier;
    }
    
    /// <summary>
    /// Get final projectile speed
    /// </summary>
    public float GetFinalProjectileSpeed()
    {
        if (baseAttack == null) return 0f;
        
        UpgradeManager upgrades = UpgradeManager.Instance;
        if (upgrades == null) return baseAttack.projectileSpeed;
        
        float baseSpeed = baseAttack.projectileSpeed;
        float increasedMultiplier = 1f + upgrades.GetProjectileSpeedMultiplier();
        
        return baseSpeed * increasedMultiplier;
    }
    
    /// <summary>
    /// Get final knockback force
    /// </summary>
    public float GetFinalKnockback()
    {
        if (baseAttack == null) return 0f;
        
        UpgradeManager upgrades = UpgradeManager.Instance;
        if (upgrades == null) return baseAttack.knockbackForce;
        
        float baseKnockback = baseAttack.knockbackForce;
        float increasedMultiplier = 1f + upgrades.GetKnockbackMultiplier();
        
        return baseKnockback * increasedMultiplier;
    }
    
    /// <summary>
    /// Roll for crit and return final damage (with crit if applicable)
    /// </summary>
    public float RollDamageWithCrit(out bool isCrit)
    {
        float baseDamage = GetFinalDamage();
        
        UpgradeManager upgrades = UpgradeManager.Instance;
        if (upgrades != null && upgrades.RollCrit(out float critMultiplier))
        {
            isCrit = true;
            return baseDamage * critMultiplier;
        }
        
        isCrit = false;
        return baseDamage;
    }
}
