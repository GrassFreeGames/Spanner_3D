using UnityEngine;

/// <summary>
/// ScriptableObject that defines the base properties of a weapon.
/// Create new weapons by right-clicking in Project: Create > Spanner > Weapons > Weapon Data
/// </summary>
[CreateAssetMenu(fileName = "New_WeaponData", menuName = "Spanner/Weapons/Weapon Data", order = 1)]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Display name of the weapon")]
    public string weaponName = "New Weapon";
    
    [Tooltip("Description shown in UI")]
    [TextArea(2, 4)]
    public string description = "Weapon description here";
    
    [Tooltip("Weapon icon for UI")]
    public Sprite icon;
    
    [Header("Weapon Tags")]
    [Tooltip("Three tags that define this weapon's upgrade paths")]
    public WeaponTag tag1;
    public WeaponTag tag2;
    public WeaponTag tag3;
    
    [Header("Base Stats")]
    [Tooltip("Base damage per hit")]
    public float baseDamage = 10f;
    
    [Tooltip("Base attack interval (seconds between attacks/bursts)")]
    public float baseAttackInterval = 1f;
    
    [Tooltip("Base projectile count per burst (1 for single shot)")]
    public int baseProjectileCount = 1;
    
    [Tooltip("Base range/radius")]
    public float baseRange = 10f;
    
    [Header("Level Scaling")]
    [Tooltip("Damage increase per weapon level")]
    public float damagePerLevel = 2f;
    
    [Header("Weapon Type")]
    [Tooltip("Weapon behavior type (determines which component to use)")]
    public WeaponType weaponType;
    
    [Header("Projectile")]
    [Tooltip("Projectile prefab to spawn (only for Projectile and RocketSpray weapon types)")]
    public GameObject projectilePrefab;
    
    [Header("Visual Effects")]
    [Tooltip("Material for aura disc (only for Aura weapon type - Static Field)")]
    public Material auraDiscMaterial;
    
    [Tooltip("VFX prefab for melee slashes (only for Melee weapon type - Energy Sword)")]
    public GameObject slashVFXPrefab;
    
    [Tooltip("VFX prefab for explosions (only for RocketSpray weapon type - Chaos Rockets)")]
    public GameObject explosionVFXPrefab;
    
    [Header("Audio")]
    [Tooltip("Wwise event to play when weapon fires. Leave empty for silent weapons.")]
    public AK.Wwise.Event fireSound;
    
    /// <summary>
    /// Check if this weapon has a specific tag
    /// </summary>
    public bool HasTag(WeaponTag tag)
    {
        return tag1 == tag || tag2 == tag || tag3 == tag;
    }
    
    /// <summary>
    /// Get all three tags as an array
    /// </summary>
    public WeaponTag[] GetTags()
    {
        return new WeaponTag[] { tag1, tag2, tag3 };
    }
}

/// <summary>
/// Enum for weapon behavior types
/// </summary>
public enum WeaponType
{
    Projectile,     // Standard projectile weapon (Blaster, Canon)
    Melee,          // Melee attack (Energy Sword)
    RocketSpray,    // Random directional projectiles (Chaos Rockets)
    Aura            // Damage aura (Static Field)
}
