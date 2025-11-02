using UnityEngine;

/// <summary>
/// Defines display information for an upgrade type.
/// Used by UpgradeUI to show upgrade options.
/// </summary>
[System.Serializable]
public class UpgradeDefinition
{
    public UpgradeType type;
    public string displayName;
    public string description;
    public Sprite icon; // Optional icon for UI
    
    /// <summary>
    /// Get description with current stats filled in
    /// </summary>
    public string GetDescription(int currentLevel)
    {
        return type switch
        {
            UpgradeType.MoveSpeed => $"+2 Movement Speed\nCurrent: Level {currentLevel}",
            UpgradeType.AttackRate => $"+20% Attack Speed\nCurrent: Level {currentLevel}",
            UpgradeType.ProjectileSpeed => $"+10% Projectile Speed\nCurrent: Level {currentLevel}",
            UpgradeType.HpRegen => $"+1 HP/second Regeneration\nCurrent: {currentLevel} HP/s",
            UpgradeType.HpTotal => $"+20 Maximum HP\nCurrent: Level {currentLevel}",
            UpgradeType.Knockback => $"+20% Knockback Force\nCurrent: Level {currentLevel}",
            UpgradeType.Damage => $"+20% Damage\nCurrent: Level {currentLevel}",
            UpgradeType.PickupRange => $"+20% Pickup Radius\nCurrent: Level {currentLevel}",
            UpgradeType.Armor => $"+5% Damage Reduction\nCurrent: {currentLevel * 5}% Armor",
            UpgradeType.Lifesteal => $"+10% Lifesteal\nCurrent: {currentLevel * 10}% Lifesteal",
            UpgradeType.CritChance => $"+10% Critical Hit Chance\nCurrent: {currentLevel * 10}% Crit",
            UpgradeType.CritDamage => $"+15% Critical Damage\nCurrent: {2.0f + currentLevel * 0.15f:F2}x Crit Damage",
            _ => description
        };
    }
    
    /// <summary>
    /// Get all available upgrade definitions
    /// </summary>
    public static UpgradeDefinition[] GetAllUpgrades()
    {
        return new UpgradeDefinition[]
        {
            new UpgradeDefinition 
            { 
                type = UpgradeType.MoveSpeed, 
                displayName = "Movement Speed",
                description = "+2 Movement Speed"
            },
            new UpgradeDefinition 
            { 
                type = UpgradeType.AttackRate, 
                displayName = "Attack Speed",
                description = "+20% Attack Rate"
            },
            new UpgradeDefinition 
            { 
                type = UpgradeType.ProjectileSpeed, 
                displayName = "Projectile Speed",
                description = "+10% Projectile Speed"
            },
            new UpgradeDefinition 
            { 
                type = UpgradeType.HpRegen, 
                displayName = "HP Regeneration",
                description = "+1 HP/second"
            },
            new UpgradeDefinition 
            { 
                type = UpgradeType.HpTotal, 
                displayName = "Max Health",
                description = "+20 Maximum HP"
            },
            new UpgradeDefinition 
            { 
                type = UpgradeType.Knockback, 
                displayName = "Knockback",
                description = "+20% Knockback Force"
            },
            new UpgradeDefinition 
            { 
                type = UpgradeType.Damage, 
                displayName = "Damage",
                description = "+20% Attack Damage"
            },
            new UpgradeDefinition 
            { 
                type = UpgradeType.PickupRange, 
                displayName = "Pickup Range",
                description = "+20% Pickup Radius"
            },
            new UpgradeDefinition 
            { 
                type = UpgradeType.Armor, 
                displayName = "Armor",
                description = "+5% Damage Reduction"
            },
            new UpgradeDefinition 
            { 
                type = UpgradeType.Lifesteal, 
                displayName = "Lifesteal",
                description = "+10% Lifesteal Chance"
            },
            new UpgradeDefinition 
            { 
                type = UpgradeType.CritChance, 
                displayName = "Critical Chance",
                description = "+10% Crit Chance"
            },
            new UpgradeDefinition 
            { 
                type = UpgradeType.CritDamage, 
                displayName = "Critical Damage",
                description = "+15% Crit Damage"
            }
        };
    }
}
