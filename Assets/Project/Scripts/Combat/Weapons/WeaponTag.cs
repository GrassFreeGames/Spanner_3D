using UnityEngine;

/// <summary>
/// Defines weapon tags and their effects when upgraded.
/// Each weapon has 3 tags that can be amplified through Retrofit.
/// </summary>
public enum WeaponTag
{
    Area,       // Increases size, range, or area of effect
    Projectile, // Increases number of projectiles
    Explosive,  // Increases splash damage or bounces
    Energy,     // Increases projectile speed or weapon turn rate
    Beam,       // Increases duration
    Rapid,      // Increases attack speed, tick rate, or reduces cooldown
    Heavy       // Increases knockback or damage at cost of attack speed
}

/// <summary>
/// Helper class for weapon tag effects and descriptions
/// </summary>
public static class WeaponTagHelper
{
    /// <summary>
    /// Get the effect description for a tag at a specific level
    /// </summary>
    public static string GetTagDescription(WeaponTag tag)
    {
        return tag switch
        {
            WeaponTag.Area => "Increases size, range, or area of effect",
            WeaponTag.Projectile => "Increases number of projectiles",
            WeaponTag.Explosive => "Increases splash damage or bounces",
            WeaponTag.Energy => "Increases projectile speed or weapon turn rate",
            WeaponTag.Beam => "Increases duration",
            WeaponTag.Rapid => "Increases attack speed or tick rate",
            WeaponTag.Heavy => "Increases knockback and damage, reduces attack speed",
            _ => "Unknown tag"
        };
    }
    
    /// <summary>
    /// Calculate stat value for a tag at a given level.
    /// Most stats scale linearly, but some have diminishing returns.
    /// </summary>
    public static float CalculateTagValue(WeaponTag tag, int level, string statName)
    {
        if (level <= 0) return 0f;
        
        switch (statName)
        {
            case "AttackSpeed":
                // +7% per level (linear)
                return level * 0.07f;
                
            case "Damage":
                // +10% per level (linear)
                return level * 0.10f;
                
            case "Range":
                // +15% per level (linear)
                return level * 0.15f;
                
            case "AreaSize":
                // +12% per level (linear)
                return level * 0.12f;
                
            case "ProjectileCount":
                // +1 projectile per level
                return level;
                
            case "ProjectileSpeed":
                // +10% per level (linear)
                return level * 0.10f;
                
            case "Duration":
                // +0.5 seconds per level
                return level * 0.5f;
                
            case "Knockback":
                // +15% per level (linear)
                return level * 0.15f;
                
            case "ExplosionDamage":
                // +12% per level (linear)
                return level * 0.12f;
                
            case "ExplosionRadius":
                // +10% per level (linear)
                return level * 0.10f;
                
            case "Armor":
                // Hyperbolic function with 80% cap
                // Formula: 80 * (1 - 1/(1 + level * 0.15))
                return 80f * (1f - 1f / (1f + level * 0.15f));
                
            default:
                Debug.LogWarning($"Unknown stat name: {statName}");
                return 0f;
        }
    }
    
    /// <summary>
    /// Get formatted display text for a tag level change
    /// </summary>
    public static string GetLevelUpText(WeaponTag tag, int currentLevel, int newLevel, WeaponData weaponData)
    {
        switch (tag)
        {
            case WeaponTag.Area:
                float currentRange = CalculateTagValue(tag, currentLevel, "Range");
                float newRange = CalculateTagValue(tag, newLevel, "Range");
                return $"Area Level {currentLevel} → {newLevel}\n+{currentRange * 100:F0}% Range → +{newRange * 100:F0}% Range";
                
            case WeaponTag.Projectile:
                float currentCount = CalculateTagValue(tag, currentLevel, "ProjectileCount");
                float newCount = CalculateTagValue(tag, newLevel, "ProjectileCount");
                return $"Projectile Level {currentLevel} → {newLevel}\n+{currentCount:F0} Projectiles → +{newCount:F0} Projectiles";
                
            case WeaponTag.Explosive:
                float currentExpDmg = CalculateTagValue(tag, currentLevel, "ExplosionDamage");
                float newExpDmg = CalculateTagValue(tag, newLevel, "ExplosionDamage");
                return $"Explosive Level {currentLevel} → {newLevel}\n+{currentExpDmg * 100:F0}% Explosion Damage → +{newExpDmg * 100:F0}% Explosion Damage";
                
            case WeaponTag.Energy:
                float currentSpeed = CalculateTagValue(tag, currentLevel, "ProjectileSpeed");
                float newSpeed = CalculateTagValue(tag, newLevel, "ProjectileSpeed");
                return $"Energy Level {currentLevel} → {newLevel}\n+{currentSpeed * 100:F0}% Projectile Speed → +{newSpeed * 100:F0}% Projectile Speed";
                
            case WeaponTag.Beam:
                float currentDuration = CalculateTagValue(tag, currentLevel, "Duration");
                float newDuration = CalculateTagValue(tag, newLevel, "Duration");
                return $"Beam Level {currentLevel} → {newLevel}\n+{currentDuration:F1}s Duration → +{newDuration:F1}s Duration";
                
            case WeaponTag.Rapid:
                float currentAtkSpd = CalculateTagValue(tag, currentLevel, "AttackSpeed");
                float newAtkSpd = CalculateTagValue(tag, newLevel, "AttackSpeed");
                return $"Rapid Level {currentLevel} → {newLevel}\n+{currentAtkSpd * 100:F0}% Attack Speed → +{newAtkSpd * 100:F0}% Attack Speed";
                
            case WeaponTag.Heavy:
                float currentDmg = CalculateTagValue(tag, currentLevel, "Damage");
                float newDmg = CalculateTagValue(tag, newLevel, "Damage");
                return $"Heavy Level {currentLevel} → {newLevel}\n+{currentDmg * 100:F0}% Damage → +{newDmg * 100:F0}% Damage";
                
            default:
                return $"{tag} Level {currentLevel} → {newLevel}";
        }
    }
}
