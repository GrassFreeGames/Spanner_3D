using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Weapon behavior for standard projectile weapons.
/// Fires homing or piercing projectiles at enemies.
/// Blaster cycles through multiple closest enemies based on attack speed.
/// Canon fires piercing shots - flat on level ground, angled on slopes/elevation changes.
/// Used by: Blaster, Canon
/// </summary>
public class WeaponBehavior_Projectile : WeaponBehaviorBase
{
    private GameObject _projectilePrefab;
    
    // Blaster target cycling
    private int _currentTargetIndex = 0;
    private List<Transform> _targetPool = new List<Transform>();
    private float _lastAttackSpeed = 0f;
    
    [Header("Canon Settings")]
    [Tooltip("Vertical distance threshold for flat firing. If target is within this Y distance, shot fires flat.")]
    public float flatFireThreshold = 2f;
    
    protected override void OnInitialize()
    {
        // Get projectile prefab from weapon data
        _projectilePrefab = _weapon.weaponData.projectilePrefab;
        
        if (_projectilePrefab == null)
        {
            Debug.LogError($"WeaponBehavior_Projectile: {_weapon.weaponData.weaponName} has no projectile prefab assigned in WeaponData!");
        }
    }
    
    public override bool Fire()
    {
        if (_projectilePrefab == null)
        {
            Debug.LogError("Cannot fire projectile: no prefab assigned!");
            return false;
        }
        
        // Get final range (for targeting)
        float range = _weapon.GetFinalRange();
        
        // Check if there are any enemies in range BEFORE attempting to fire
        Transform testTarget = GetClosestEnemy(range);
        if (testTarget == null)
        {
            // No enemies in range - don't fire, don't play sound, don't reset cooldown
            return false;
        }
        
        // Get final projectile count
        int projectileCount = _weapon.GetFinalProjectileCount();
        
        // Determine if piercing (Canon has Beam tag)
        bool isPiercing = _weapon.weaponData.HasTag(WeaponTag.Beam);
        
        // Fire projectiles
        bool anyProjectileFired = false;
        for (int i = 0; i < projectileCount; i++)
        {
            bool fired;
            if (isPiercing)
            {
                fired = FirePiercingProjectile(range);
            }
            else
            {
                fired = FireHomingProjectile(range);
            }
            
            if (fired)
                anyProjectileFired = true;
        }
        
        return anyProjectileFired;
    }
    
    bool FireHomingProjectile(float maxRange)
    {
        // Calculate current attack speed multiplier
        float attackSpeedMultiplier = CalculateAttackSpeedMultiplier();
        
        // Determine target pool size based on attack speed
        // Base: 4 targets at attack speed ≤2.0
        // +1 target per 1.0 attack speed above 2.0
        int targetPoolSize = 4;
        if (attackSpeedMultiplier > 2.0f)
        {
            int bonusTargets = Mathf.FloorToInt(attackSpeedMultiplier - 2.0f);
            targetPoolSize += bonusTargets;
        }
        
        // Rebuild target pool if attack speed changed significantly
        if (Mathf.Abs(_lastAttackSpeed - attackSpeedMultiplier) > 0.5f)
        {
            _currentTargetIndex = 0;
            _targetPool.Clear();
        }
        _lastAttackSpeed = attackSpeedMultiplier;
        
        // Rebuild target pool if empty or targets are invalid
        if (_targetPool.Count == 0 || !IsTargetPoolValid())
        {
            RebuildTargetPool(maxRange, targetPoolSize);
            _currentTargetIndex = 0;
        }
        
        // Get next target from pool
        Transform target = null;
        if (_targetPool.Count > 0)
        {
            target = _targetPool[_currentTargetIndex];
            
            // Cycle to next target
            _currentTargetIndex = (_currentTargetIndex + 1) % _targetPool.Count;
        }
        
        if (target == null)
        {
            // No valid targets
            return false;
        }
        
        // Spawn projectile
        Vector3 spawnPos = GetSpawnPosition();
        GameObject projectileObj = Instantiate(_projectilePrefab, spawnPos, Quaternion.identity);
        
        // Configure projectile
        WeaponProjectile projectile = projectileObj.GetComponent<WeaponProjectile>();
        if (projectile != null)
        {
            float damage = GetFinalDamage();
            float speed = 15f * _weapon.GetProjectileSpeedMultiplier();
            float lifetime = 5f + _weapon.GetDurationBonus();
            
            projectile.Initialize(damage, speed, lifetime, target, _playerTransform.position, isHoming: true);
            return true;
        }
        else
        {
            Debug.LogError("Projectile prefab is missing WeaponProjectile component!");
            Destroy(projectileObj);
            return false;
        }
    }
    
    /// <summary>
    /// Calculate current attack speed multiplier from all sources
    /// </summary>
    float CalculateAttackSpeedMultiplier()
    {
        float multiplier = 1f;
        
        // Weapon tags (Rapid tag increases attack speed)
        if (_weapon.weaponData.HasTag(WeaponTag.Rapid))
        {
            int rapidLevel = _weapon.GetTagLevel(WeaponTag.Rapid);
            float rapidBonus = WeaponTagHelper.CalculateTagValue(WeaponTag.Rapid, rapidLevel, "AttackSpeed");
            multiplier *= (1f + rapidBonus);
        }
        
        // Upgrade manager
        UpgradeManager upgrades = UpgradeManager.Instance;
        if (upgrades != null)
        {
            multiplier *= (1f + upgrades.GetAttackSpeedMultiplier());
        }
        
        // Item manager
        ItemManager items = ItemManager.Instance;
        if (items != null)
        {
            multiplier *= items.TotalAttackSpeedMultiplier;
        }
        
        return multiplier;
    }
    
    /// <summary>
    /// Check if current target pool is still valid (all targets alive and in range)
    /// </summary>
    bool IsTargetPoolValid()
    {
        foreach (Transform target in _targetPool)
        {
            if (target == null)
                return false;
        }
        return true;
    }
    
    /// <summary>
    /// Rebuild the target pool with closest enemies
    /// </summary>
    void RebuildTargetPool(float maxRange, int poolSize)
    {
        _targetPool.Clear();
        
        // Get all enemies
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (allEnemies.Length == 0)
            return;
        
        // Sort by distance
        List<Transform> sortedEnemies = new List<Transform>();
        Vector3 playerPos = _playerTransform.position;
        
        foreach (GameObject enemyObj in allEnemies)
        {
            float distance = Vector3.Distance(playerPos, enemyObj.transform.position);
            if (distance <= maxRange)
            {
                sortedEnemies.Add(enemyObj.transform);
            }
        }
        
        // Sort by distance
        sortedEnemies.Sort((a, b) =>
        {
            float distA = Vector3.Distance(playerPos, a.position);
            float distB = Vector3.Distance(playerPos, b.position);
            return distA.CompareTo(distB);
        });
        
        // Take closest N enemies
        int count = Mathf.Min(poolSize, sortedEnemies.Count);
        for (int i = 0; i < count; i++)
        {
            _targetPool.Add(sortedEnemies[i]);
        }
    }
    
    bool FirePiercingProjectile(float maxRange)
    {
        // Get target for initial direction
        Transform target = GetClosestEnemy(maxRange);
        
        Vector3 spawnPos = GetSpawnPosition();
        Vector3 direction;
        
        if (target != null)
        {
            // Calculate direction to target
            Vector3 rawDirection = (target.position - spawnPos).normalized;
            
            // Calculate vertical distance between spawn and target
            float verticalDistance = Mathf.Abs(target.position.y - spawnPos.y);
            
            // If target is on roughly the same elevation (flat ground), flatten the shot
            if (verticalDistance <= flatFireThreshold)
            {
                // Zero out Y component to fire flat
                direction = new Vector3(rawDirection.x, 0f, rawDirection.z).normalized;
                
                // Safety check: if somehow the horizontal direction is zero, use forward
                if (direction.magnitude < 0.01f)
                {
                    direction = _playerTransform.forward;
                }
            }
            else
            {
                // Target is significantly higher/lower (slope, aerial) - use full 3D direction
                direction = rawDirection;
            }
        }
        else
        {
            // No target - don't fire
            return false;
        }
        
        // Spawn projectile
        GameObject projectileObj = Instantiate(_projectilePrefab, spawnPos, Quaternion.identity);
        
        // Configure projectile
        WeaponProjectile projectile = projectileObj.GetComponent<WeaponProjectile>();
        if (projectile != null)
        {
            float damage = GetFinalDamage();
            float speed = 20f * _weapon.GetProjectileSpeedMultiplier();
            float lifetime = 5f + _weapon.GetDurationBonus();
            
            projectile.InitializePiercing(damage, speed, lifetime, direction, isHoming: false);
            return true;
        }
        else
        {
            Debug.LogError("Projectile prefab is missing WeaponProjectile component!");
            Destroy(projectileObj);
            return false;
        }
    }
}
