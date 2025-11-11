using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Controls automatic firing of all equipped weapons.
/// Replaces AttackController with support for multiple weapon types.
/// Attach to CharacterPlayer.
/// </summary>
public class WeaponController : MonoBehaviour
{
    [Header("Spawn Point")]
    [Tooltip("Where projectiles spawn from. If null, uses this object's position.")]
    public Transform projectileSpawnPoint;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private fields: _camelCase
    private Transform _cachedTransform;
    private CombatStats _combatStats;
    private WeaponManager _weaponManager;
    
    // Track attack timers for each weapon slot
    private float[] _attackTimers;
    
    // Active weapon behavior components
    private Dictionary<int, WeaponBehaviorBase> _weaponBehaviors = new Dictionary<int, WeaponBehaviorBase>();

    void Start()
    {
        _cachedTransform = transform;
        _combatStats = CombatStats.Instance;
        _weaponManager = WeaponManager.Instance;
        
        if (_weaponManager == null)
        {
            Debug.LogError("WeaponController cannot find WeaponManager!", this);
            enabled = false;
            return;
        }
        
        // Initialize timers
        _attackTimers = new float[_weaponManager.maxWeaponSlots];
        
        // Subscribe to weapon changes
        _weaponManager.OnWeaponsChanged += OnWeaponsChanged;
        
        // Initialize weapons
        OnWeaponsChanged();
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (_weaponManager != null)
        {
            _weaponManager.OnWeaponsChanged -= OnWeaponsChanged;
        }
    }
    
    /// <summary>
    /// Called when equipped weapons change
    /// </summary>
    void OnWeaponsChanged()
    {
        // Clean up old weapon behaviors
        foreach (var behavior in _weaponBehaviors.Values)
        {
            if (behavior != null)
                Destroy(behavior);
        }
        _weaponBehaviors.Clear();
        
        // Create new weapon behaviors
        WeaponInstance[] weapons = _weaponManager.GetAllWeapons();
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
            {
                CreateWeaponBehavior(i, weapons[i]);
            }
        }
        
        if (showDebugInfo)
            Debug.Log($"Weapons updated. Active weapons: {_weaponBehaviors.Count}");
    }
    
    /// <summary>
    /// Create appropriate weapon behavior component for weapon type
    /// </summary>
    void CreateWeaponBehavior(int slotIndex, WeaponInstance weapon)
    {
        WeaponBehaviorBase behavior = null;
        
        switch (weapon.weaponData.weaponType)
        {
            case WeaponType.Projectile:
                behavior = gameObject.AddComponent<WeaponBehavior_Projectile>();
                break;
                
            case WeaponType.Melee:
                behavior = gameObject.AddComponent<WeaponBehavior_Melee>();
                break;
                
            case WeaponType.RocketSpray:
                behavior = gameObject.AddComponent<WeaponBehavior_RocketSpray>();
                break;
                
            case WeaponType.Aura:
                behavior = gameObject.AddComponent<WeaponBehavior_Aura>();
                break;
                
            default:
                Debug.LogError($"Unknown weapon type: {weapon.weaponData.weaponType}");
                return;
        }
        
        if (behavior != null)
        {
            behavior.Initialize(weapon, _cachedTransform, projectileSpawnPoint);
            _weaponBehaviors[slotIndex] = behavior;
            
            if (showDebugInfo)
                Debug.Log($"Created {weapon.weaponData.weaponType} behavior for {weapon.weaponData.weaponName}");
        }
    }

    void Update()
    {
        WeaponInstance[] weapons = _weaponManager.GetAllWeapons();
        
        // Update timers and fire weapons
        for (int i = 0; i < weapons.Length; i++)
        {
            WeaponInstance weapon = weapons[i];
            if (weapon == null) continue;
            
            // Get weapon behavior
            if (!_weaponBehaviors.ContainsKey(i)) continue;
            WeaponBehaviorBase behavior = _weaponBehaviors[i];
            if (behavior == null) continue;
            
            // Count down to next attack
            _attackTimers[i] += Time.deltaTime;
            
            // Calculate attack interval with weapon tags, upgrades, and items
            float attackInterval = CalculateFinalAttackInterval(weapon);
            
            // Check if ready to attack
            if (_attackTimers[i] >= attackInterval)
            {
                // Try to fire weapon - returns false if no valid targets (for targeted weapons)
                bool didFire = behavior.Fire();
                
                if (didFire)
                {
                    // Only reset cooldown and play sound if weapon actually fired
                    _attackTimers[i] = 0f;
                    
                    // Play fire sound (EXCEPT for Aura weapons - they handle their own sound)
                    if (weapon.weaponData.weaponType != WeaponType.Aura)
                    {
                        if (weapon.weaponData.fireSound != null)
                        {
                            weapon.weaponData.fireSound.Post(gameObject);
                        }
                    }
                    
                    if (showDebugInfo)
                        Debug.Log($"{weapon.weaponData.weaponName} fired (interval: {attackInterval:F2}s)");
                }
                else
                {
                    // Weapon didn't fire (no targets) - don't reset cooldown fully
                    // Instead, set timer to 90% so it checks again soon
                    _attackTimers[i] = attackInterval * 0.9f;
                    
                    if (showDebugInfo)
                        Debug.Log($"{weapon.weaponData.weaponName} no targets - checking again soon");
                }
            }
        }
    }
    
    /// <summary>
    /// Calculate final attack interval with all modifiers
    /// (weapon tags, upgrades, items)
    /// </summary>
    float CalculateFinalAttackInterval(WeaponInstance weapon)
    {
        // Start with weapon's final interval (includes Rapid and Heavy tags)
        float interval = weapon.GetFinalAttackInterval();
        
        // Apply upgrade manager's attack speed multiplier
        UpgradeManager upgrades = UpgradeManager.Instance;
        if (upgrades != null)
        {
            float upgradeMultiplier = 1f + upgrades.GetAttackSpeedMultiplier();
            interval /= upgradeMultiplier;
        }
        
        // Apply item manager's attack speed multiplier
        ItemManager items = ItemManager.Instance;
        if (items != null)
        {
            float itemMultiplier = items.TotalAttackSpeedMultiplier;
            interval /= itemMultiplier;
        }
        
        return interval;
    }
}
