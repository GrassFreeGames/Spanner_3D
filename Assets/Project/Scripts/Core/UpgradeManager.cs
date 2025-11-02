using UnityEngine;

/// <summary>
/// Manages all player upgrades and stat modifications.
/// Tracks upgrade levels and applies multipliers to player systems.
/// Place in scene under "--- MANAGERS ---"
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Player movement component")]
    public PlayerMovement3D playerMovement;
    
    [Tooltip("Player stats component")]
    public PlayerStats playerStats;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Upgrade levels: track how many times each upgrade has been taken
    private int _moveSpeedLevel = 0;
    private int _attackRateLevel = 0;
    private int _projectileSpeedLevel = 0;
    private int _hpRegenLevel = 0;
    private int _hpTotalLevel = 0;
    private int _knockbackLevel = 0;
    private int _damageLevel = 0;
    private int _pickupRangeLevel = 0;
    private int _armorLevel = 0;
    private int _lifestealLevel = 0;
    private int _critChanceLevel = 0;
    private int _critDamageLevel = 0;
    
    // Base values (stored once at start)
    private float _baseMoveSpeed;
    private float _baseMaxHealth;
    private float _basePickupRange;
    
    // Properties: PascalCase (for UI display)
    public int MoveSpeedLevel => _moveSpeedLevel;
    public int AttackRateLevel => _attackRateLevel;
    public int ProjectileSpeedLevel => _projectileSpeedLevel;
    public int HpRegenLevel => _hpRegenLevel;
    public int HpTotalLevel => _hpTotalLevel;
    public int KnockbackLevel => _knockbackLevel;
    public int DamageLevel => _damageLevel;
    public int PickupRangeLevel => _pickupRangeLevel;
    public int ArmorLevel => _armorLevel;
    public int LifestealLevel => _lifestealLevel;
    public int CritChanceLevel => _critChanceLevel;
    public int CritDamageLevel => _critDamageLevel;
    
    // Calculated values
    public float ArmorPercent => _armorLevel * 5f; // 5% per level
    public float LifestealPercent => _lifestealLevel * 10f; // 10% per level
    public float CritChancePercent => _critChanceLevel * 10f; // 10% per level
    public float CritDamageMultiplier => 2.0f + (_critDamageLevel * 0.15f); // 2.0x base + 0.15 per level
    
    // Singleton pattern
    private static UpgradeManager _instance;
    public static UpgradeManager Instance => _instance;
    
    void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple UpgradeManager instances found! Destroying duplicate.", this);
            Destroy(this);
            return;
        }
        _instance = this;
    }
    
    void Start()
    {
        // Auto-find references if not assigned
        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<PlayerMovement3D>();
        
        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();
        
        // Store base values for stats we directly modify
        if (playerMovement != null)
            _baseMoveSpeed = playerMovement.moveSpeed;
        
        if (playerStats != null)
        {
            _baseMaxHealth = playerStats.maxHealth;
            _basePickupRange = playerStats.pickupRadius;
        }
        
        if (showDebugInfo)
            Debug.Log("UpgradeManager initialized - storing only multipliers, not modifying base stats");
    }
    
    void Update()
    {
        // Apply HP regeneration
        if (_hpRegenLevel > 0 && playerStats != null)
        {
            float regenAmount = _hpRegenLevel * Time.deltaTime; // 1 HP/s per level
            playerStats.Heal(regenAmount);
        }
    }
    
    /// <summary>
    /// Apply an upgrade by type
    /// </summary>
    public void ApplyUpgrade(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.MoveSpeed:
                _moveSpeedLevel++;
                if (playerMovement != null)
                    playerMovement.moveSpeed = _baseMoveSpeed + (_moveSpeedLevel * 2f);
                break;
                
            case UpgradeType.AttackRate:
                _attackRateLevel++;
                // Multiplier applied via CombatStats.GetFinalAttackRate()
                break;
                
            case UpgradeType.ProjectileSpeed:
                _projectileSpeedLevel++;
                // Multiplier applied via CombatStats.GetFinalProjectileSpeed()
                break;
                
            case UpgradeType.HpRegen:
                _hpRegenLevel++;
                break;
                
            case UpgradeType.HpTotal:
                _hpTotalLevel++;
                if (playerStats != null)
                {
                    float healthIncrease = 20f; // +20 HP per level
                    playerStats.maxHealth = _baseMaxHealth + (_hpTotalLevel * healthIncrease);
                    playerStats.Heal(healthIncrease); // Also heal the increased amount
                }
                break;
                
            case UpgradeType.Knockback:
                _knockbackLevel++;
                // Multiplier applied via CombatStats.GetFinalKnockback()
                break;
                
            case UpgradeType.Damage:
                _damageLevel++;
                // Multiplier applied via CombatStats.GetFinalDamage()
                break;
                
            case UpgradeType.PickupRange:
                _pickupRangeLevel++;
                if (playerStats != null)
                    playerStats.pickupRadius = _basePickupRange * (1f + _pickupRangeLevel * 0.2f);
                break;
                
            case UpgradeType.Armor:
                _armorLevel++;
                break;
                
            case UpgradeType.Lifesteal:
                _lifestealLevel++;
                break;
                
            case UpgradeType.CritChance:
                _critChanceLevel++;
                break;
                
            case UpgradeType.CritDamage:
                _critDamageLevel++;
                break;
        }
        
        if (showDebugInfo)
            Debug.Log($"Applied upgrade: {type} (Level {GetUpgradeLevel(type)})");
    }
    
    /// <summary>
    /// Get current level of an upgrade type
    /// </summary>
    public int GetUpgradeLevel(UpgradeType type)
    {
        return type switch
        {
            UpgradeType.MoveSpeed => _moveSpeedLevel,
            UpgradeType.AttackRate => _attackRateLevel,
            UpgradeType.ProjectileSpeed => _projectileSpeedLevel,
            UpgradeType.HpRegen => _hpRegenLevel,
            UpgradeType.HpTotal => _hpTotalLevel,
            UpgradeType.Knockback => _knockbackLevel,
            UpgradeType.Damage => _damageLevel,
            UpgradeType.PickupRange => _pickupRangeLevel,
            UpgradeType.Armor => _armorLevel,
            UpgradeType.Lifesteal => _lifestealLevel,
            UpgradeType.CritChance => _critChanceLevel,
            UpgradeType.CritDamage => _critDamageLevel,
            _ => 0
        };
    }
    
    /// <summary>
    /// Calculate final damage with armor reduction
    /// </summary>
    public float ApplyArmor(float incomingDamage)
    {
        float damageReduction = 1f - (ArmorPercent / 100f);
        return incomingDamage * Mathf.Max(0f, damageReduction);
    }
    
    /// <summary>
    /// Get damage multiplier (0.2 per level = 20% per level)
    /// </summary>
    public float GetDamageMultiplier()
    {
        return _damageLevel * 0.2f; // 20% per level
    }
    
    /// <summary>
    /// Get attack speed multiplier (0.2 per level = 20% per level)
    /// </summary>
    public float GetAttackSpeedMultiplier()
    {
        return _attackRateLevel * 0.2f; // 20% per level
    }
    
    /// <summary>
    /// Get projectile speed multiplier (0.1 per level = 10% per level)
    /// </summary>
    public float GetProjectileSpeedMultiplier()
    {
        return _projectileSpeedLevel * 0.1f; // 10% per level
    }
    
    /// <summary>
    /// Get knockback multiplier (0.2 per level = 20% per level)
    /// </summary>
    public float GetKnockbackMultiplier()
    {
        return _knockbackLevel * 0.2f; // 20% per level
    }
    
    /// <summary>
    /// Check if lifesteal should trigger and return heal amount
    /// </summary>
    public float CheckLifesteal()
    {
        if (_lifestealLevel == 0) return 0f;
        
        float lifestealPercent = LifestealPercent;
        int guaranteedHeals = Mathf.FloorToInt(lifestealPercent / 100f);
        float remainingChance = lifestealPercent % 100f;
        
        float totalHeal = guaranteedHeals;
        
        if (Random.Range(0f, 100f) < remainingChance)
            totalHeal += 1f;
        
        return totalHeal;
    }
    
    /// <summary>
    /// Check if attack should crit and return damage multiplier
    /// </summary>
    public bool RollCrit(out float damageMultiplier)
    {
        damageMultiplier = 1f;
        
        if (_critChanceLevel == 0) return false;
        
        if (Random.Range(0f, 100f) < CritChancePercent)
        {
            damageMultiplier = CritDamageMultiplier;
            return true;
        }
        
        return false;
    }
}

/// <summary>
/// Enum for all upgrade types
/// </summary>
public enum UpgradeType
{
    MoveSpeed,
    AttackRate,
    ProjectileSpeed,
    HpRegen,
    HpTotal,
    Knockback,
    Damage,
    PickupRange,
    Armor,
    Lifesteal,
    CritChance,
    CritDamage
}
