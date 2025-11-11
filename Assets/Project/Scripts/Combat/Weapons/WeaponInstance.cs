using UnityEngine;

/// <summary>
/// Runtime instance of a weapon with levels and tag upgrades.
/// Stores current state separate from the base WeaponData ScriptableObject.
/// </summary>
[System.Serializable]
public class WeaponInstance
{
    // Base weapon data (never modified)
    public WeaponData weaponData;
    
    // Current levels
    public int weaponLevel = 1;
    public int tag1Level = 0;
    public int tag2Level = 0;
    public int tag3Level = 0;
    
    // Constructor
    public WeaponInstance(WeaponData data)
    {
        weaponData = data;
        weaponLevel = 1;
        tag1Level = 0;
        tag2Level = 0;
        tag3Level = 0;
    }
    
    /// <summary>
    /// Get tag level by tag type
    /// </summary>
    public int GetTagLevel(WeaponTag tag)
    {
        if (weaponData.tag1 == tag) return tag1Level;
        if (weaponData.tag2 == tag) return tag2Level;
        if (weaponData.tag3 == tag) return tag3Level;
        return 0;
    }
    
    /// <summary>
    /// Set tag level by tag type
    /// </summary>
    public void SetTagLevel(WeaponTag tag, int level)
    {
        if (weaponData.tag1 == tag) tag1Level = level;
        else if (weaponData.tag2 == tag) tag2Level = level;
        else if (weaponData.tag3 == tag) tag3Level = level;
    }
    
    /// <summary>
    /// Level up a specific tag
    /// </summary>
    public void UpgradeTag(WeaponTag tag)
    {
        int currentLevel = GetTagLevel(tag);
        SetTagLevel(tag, currentLevel + 1);
    }
    
    /// <summary>
    /// Level up all tags (for same-weapon Retrofit)
    /// </summary>
    public void UpgradeAllTags()
    {
        tag1Level++;
        tag2Level++;
        tag3Level++;
    }
    
    /// <summary>
    /// Level up the weapon itself (increases base damage)
    /// </summary>
    public void LevelUpWeapon()
    {
        weaponLevel++;
    }
    
    /// <summary>
    /// Get final damage with weapon level and Heavy tag
    /// </summary>
    public float GetFinalDamage()
    {
        // Base damage + level scaling
        float damage = weaponData.baseDamage + (weaponLevel - 1) * weaponData.damagePerLevel;
        
        // Apply Heavy tag bonus
        if (weaponData.HasTag(WeaponTag.Heavy))
        {
            int heavyLevel = GetTagLevel(WeaponTag.Heavy);
            float heavyBonus = WeaponTagHelper.CalculateTagValue(WeaponTag.Heavy, heavyLevel, "Damage");
            damage *= (1f + heavyBonus);
        }
        
        return damage;
    }
    
    /// <summary>
    /// Get final attack interval with Rapid tag and Heavy tag
    /// </summary>
    public float GetFinalAttackInterval()
    {
        float interval = weaponData.baseAttackInterval;
        
        // Apply Rapid tag bonus (reduces interval)
        if (weaponData.HasTag(WeaponTag.Rapid))
        {
            int rapidLevel = GetTagLevel(WeaponTag.Rapid);
            float rapidBonus = WeaponTagHelper.CalculateTagValue(WeaponTag.Rapid, rapidLevel, "AttackSpeed");
            interval /= (1f + rapidBonus);
        }
        
        // Apply Heavy tag penalty (increases interval)
        if (weaponData.HasTag(WeaponTag.Heavy))
        {
            int heavyLevel = GetTagLevel(WeaponTag.Heavy);
            // Heavy reduces attack speed by 5% per level
            float heavyPenalty = heavyLevel * 0.05f;
            interval *= (1f + heavyPenalty);
        }
        
        return interval;
    }
    
    /// <summary>
    /// Get final projectile count with Projectile tag
    /// </summary>
    public int GetFinalProjectileCount()
    {
        int count = weaponData.baseProjectileCount;
        
        // Apply Projectile tag bonus
        if (weaponData.HasTag(WeaponTag.Projectile))
        {
            int projectileLevel = GetTagLevel(WeaponTag.Projectile);
            int bonus = Mathf.FloorToInt(WeaponTagHelper.CalculateTagValue(WeaponTag.Projectile, projectileLevel, "ProjectileCount"));
            count += bonus;
        }
        
        return count;
    }
    
    /// <summary>
    /// Get final range with Area tag
    /// </summary>
    public float GetFinalRange()
    {
        float range = weaponData.baseRange;
        
        // Apply Area tag bonus
        if (weaponData.HasTag(WeaponTag.Area))
        {
            int areaLevel = GetTagLevel(WeaponTag.Area);
            float areaBonus = WeaponTagHelper.CalculateTagValue(WeaponTag.Area, areaLevel, "Range");
            range *= (1f + areaBonus);
        }
        
        return range;
    }
    
    /// <summary>
    /// Get projectile speed multiplier with Energy tag
    /// </summary>
    public float GetProjectileSpeedMultiplier()
    {
        if (!weaponData.HasTag(WeaponTag.Energy)) return 1f;
        
        int energyLevel = GetTagLevel(WeaponTag.Energy);
        float energyBonus = WeaponTagHelper.CalculateTagValue(WeaponTag.Energy, energyLevel, "ProjectileSpeed");
        return 1f + energyBonus;
    }
    
    /// <summary>
    /// Get duration bonus with Beam tag
    /// </summary>
    public float GetDurationBonus()
    {
        if (!weaponData.HasTag(WeaponTag.Beam)) return 0f;
        
        int beamLevel = GetTagLevel(WeaponTag.Beam);
        return WeaponTagHelper.CalculateTagValue(WeaponTag.Beam, beamLevel, "Duration");
    }
    
    /// <summary>
    /// Get explosion damage multiplier with Explosive tag
    /// </summary>
    public float GetExplosionDamageMultiplier()
    {
        if (!weaponData.HasTag(WeaponTag.Explosive)) return 1f;
        
        int explosiveLevel = GetTagLevel(WeaponTag.Explosive);
        float explosiveBonus = WeaponTagHelper.CalculateTagValue(WeaponTag.Explosive, explosiveLevel, "ExplosionDamage");
        return 1f + explosiveBonus;
    }
    
    /// <summary>
    /// Get explosion radius multiplier with Explosive tag
    /// </summary>
    public float GetExplosionRadiusMultiplier()
    {
        if (!weaponData.HasTag(WeaponTag.Explosive)) return 1f;
        
        int explosiveLevel = GetTagLevel(WeaponTag.Explosive);
        float explosiveBonus = WeaponTagHelper.CalculateTagValue(WeaponTag.Explosive, explosiveLevel, "ExplosionRadius");
        return 1f + explosiveBonus;
    }
    
    /// <summary>
    /// Get knockback multiplier with Heavy tag
    /// </summary>
    public float GetKnockbackMultiplier()
    {
        if (!weaponData.HasTag(WeaponTag.Heavy)) return 1f;
        
        int heavyLevel = GetTagLevel(WeaponTag.Heavy);
        float heavyBonus = WeaponTagHelper.CalculateTagValue(WeaponTag.Heavy, heavyLevel, "Knockback");
        return 1f + heavyBonus;
    }
}
