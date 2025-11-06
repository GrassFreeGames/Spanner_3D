using UnityEngine;

/// <summary>
/// ScriptableObject that defines properties of an item.
/// Create new items by right-clicking in Project: Create > Spanner > Items > Item Data
/// </summary>
[CreateAssetMenu(fileName = "New_ItemData", menuName = "Spanner/Items/Item Data", order = 1)]
public class ItemData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Display name of the item")]
    public string itemName = "New Item";
    
    [Tooltip("Description shown in UI")]
    [TextArea(2, 4)]
    public string description = "Item description here";
    
    [Tooltip("Item icon for UI")]
    public Sprite icon;
    
    [Header("Rarity")]
    [Tooltip("0=Common(White), 1=Uncommon(Green), 2=Rare(Blue), 3=Epic(Purple)")]
    [Range(0, 3)]
    public int rarity = 0;
    
    [Header("Economy")]
    [Tooltip("Cost in credits when purchased from shop")]
    public int shopCost = 20;
    
    [Header("Stat Effects")]
    [Tooltip("Attack speed multiplier (1.5 = +50%)")]
    public float attackSpeedMultiplier = 1f;
    
    [Tooltip("Movement speed multiplier (1.3 = +30%)")]
    public float moveSpeedMultiplier = 1f;
    
    [Tooltip("Damage multiplier (2.0 = +100%)")]
    public float damageMultiplier = 1f;
    
    [Tooltip("Projectile speed multiplier")]
    public float projectileSpeedMultiplier = 1f;
    
    [Tooltip("Knockback multiplier")]
    public float knockbackMultiplier = 1f;
    
    [Tooltip("Max health flat bonus")]
    public float maxHealthBonus = 0f;
    
    [Tooltip("HP regen flat bonus (HP/second)")]
    public float hpRegenBonus = 0f;
    
    [Tooltip("Pickup range multiplier")]
    public float pickupRangeMultiplier = 1f;
    
    [Tooltip("Armor flat bonus (percent)")]
    public float armorBonus = 0f;
    
    [Tooltip("Lifesteal flat bonus (percent)")]
    public float lifestealBonus = 0f;
    
    [Tooltip("Crit chance flat bonus (percent)")]
    public float critChanceBonus = 0f;
    
    [Tooltip("Crit damage flat bonus (added to multiplier)")]
    public float critDamageBonus = 0f;
    
    /// <summary>
    /// Get rarity color for UI
    /// </summary>
    public Color GetRarityColor()
    {
        return rarity switch
        {
            0 => new Color(1f, 1f, 1f), // White - Common
            1 => new Color(0.3f, 1f, 0.3f), // Green - Uncommon
            2 => new Color(0.3f, 0.6f, 1f), // Blue - Rare
            3 => new Color(0.8f, 0.3f, 1f), // Purple - Epic
            _ => Color.white
        };
    }
    
    /// <summary>
    /// Get rarity name for UI
    /// </summary>
    public string GetRarityName()
    {
        return rarity switch
        {
            0 => "Common",
            1 => "Uncommon",
            2 => "Rare",
            3 => "Epic",
            _ => "Unknown"
        };
    }
    
    /// <summary>
    /// Get formatted description with stat changes
    /// </summary>
    public string GetFormattedDescription()
    {
        string result = description;
        
        // Add stat bonuses to description
        if (attackSpeedMultiplier != 1f)
        {
            float percent = (attackSpeedMultiplier - 1f) * 100f;
            result += $"\n+{percent:F0}% Attack Speed";
        }
        
        if (moveSpeedMultiplier != 1f)
        {
            float percent = (moveSpeedMultiplier - 1f) * 100f;
            result += $"\n+{percent:F0}% Movement Speed";
        }
        
        if (damageMultiplier != 1f)
        {
            float percent = (damageMultiplier - 1f) * 100f;
            result += $"\n+{percent:F0}% Damage";
        }
        
        if (projectileSpeedMultiplier != 1f)
        {
            float percent = (projectileSpeedMultiplier - 1f) * 100f;
            result += $"\n+{percent:F0}% Projectile Speed";
        }
        
        if (knockbackMultiplier != 1f)
        {
            float percent = (knockbackMultiplier - 1f) * 100f;
            result += $"\n+{percent:F0}% Knockback";
        }
        
        if (maxHealthBonus > 0f)
            result += $"\n+{maxHealthBonus:F0} Max Health";
        
        if (hpRegenBonus > 0f)
            result += $"\n+{hpRegenBonus:F1} HP/s";
        
        if (pickupRangeMultiplier != 1f)
        {
            float percent = (pickupRangeMultiplier - 1f) * 100f;
            result += $"\n+{percent:F0}% Pickup Range";
        }
        
        if (armorBonus > 0f)
            result += $"\n+{armorBonus:F0}% Armor";
        
        if (lifestealBonus > 0f)
            result += $"\n+{lifestealBonus:F0}% Lifesteal";
        
        if (critChanceBonus > 0f)
            result += $"\n+{critChanceBonus:F0}% Crit Chance";
        
        if (critDamageBonus > 0f)
            result += $"\n+{critDamageBonus:F2}x Crit Damage";
        
        return result;
    }
}
