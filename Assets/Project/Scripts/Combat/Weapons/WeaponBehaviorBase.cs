using UnityEngine;

/// <summary>
/// Abstract base class for all weapon behavior types.
/// Each weapon type (Projectile, Melee, RocketSpray, Aura) extends this.
/// </summary>
public abstract class WeaponBehaviorBase : MonoBehaviour
{
    // Weapon instance this behavior is controlling
    protected WeaponInstance _weapon;
    
    // Cached references
    protected Transform _playerTransform;
    protected Transform _spawnPoint;
    protected CombatStats _combatStats;
    
    /// <summary>
    /// Initialize the weapon behavior
    /// </summary>
    public virtual void Initialize(WeaponInstance weapon, Transform playerTransform, Transform spawnPoint)
    {
        _weapon = weapon;
        _playerTransform = playerTransform;
        _spawnPoint = spawnPoint;
        _combatStats = CombatStats.Instance;
        
        OnInitialize();
    }
    
    /// <summary>
    /// Called after initialization. Override for weapon-specific setup.
    /// </summary>
    protected virtual void OnInitialize()
    {
        // Override in derived classes
    }
    
    /// <summary>
    /// Fire the weapon. Called by WeaponController when attack timer is ready.
    /// </summary>
    /// <returns>True if weapon actually fired, false if no valid targets (prevents sound/cooldown)</returns>
    public abstract bool Fire();
    
    /// <summary>
    /// Get final damage with all modifiers (weapon level, tags, upgrades, items)
    /// </summary>
    protected float GetFinalDamage()
    {
        // Start with weapon's base damage (includes weapon level and Heavy tag)
        float damage = _weapon.GetFinalDamage();
        
        // Apply upgrade manager's damage multiplier
        UpgradeManager upgrades = UpgradeManager.Instance;
        if (upgrades != null)
        {
            float upgradeMultiplier = 1f + upgrades.GetDamageMultiplier();
            damage *= upgradeMultiplier;
        }
        
        // Apply item manager's damage multiplier
        ItemManager items = ItemManager.Instance;
        if (items != null)
        {
            damage *= items.TotalDamageMultiplier;
        }
        
        // Apply CombatStats for additional modifiers
        if (_combatStats != null && _combatStats.baseAttack != null)
        {
            // CombatStats provides additional modifiers from the base attack system
            float combatMultiplier = 1f + (_combatStats.GetFinalDamage() / _combatStats.baseAttack.damage - 1f);
            damage *= combatMultiplier;
        }
        
        return damage;
    }
    
    /// <summary>
    /// Get spawn position for projectiles
    /// </summary>
    protected Vector3 GetSpawnPosition()
    {
        return _spawnPoint != null ? _spawnPoint.position : _playerTransform.position;
    }
    
    /// <summary>
    /// Get closest enemy within range
    /// </summary>
    protected Transform GetClosestEnemy(float maxRange = 0f)
    {
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        if (allEnemies.Length == 0)
            return null;
        
        Transform closest = null;
        float closestDistance = float.MaxValue;
        
        Vector3 currentPosition = _playerTransform.position;
        
        foreach (GameObject enemyObj in allEnemies)
        {
            float distance = Vector3.Distance(currentPosition, enemyObj.transform.position);
            
            // Check range limit if set
            if (maxRange > 0 && distance > maxRange)
                continue;
            
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = enemyObj.transform;
            }
        }
        
        return closest;
    }
    
    /// <summary>
    /// Get all enemies within range
    /// </summary>
    protected Transform[] GetEnemiesInRange(float range)
    {
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        if (allEnemies.Length == 0)
            return new Transform[0];
        
        System.Collections.Generic.List<Transform> enemiesInRange = new System.Collections.Generic.List<Transform>();
        Vector3 currentPosition = _playerTransform.position;
        
        foreach (GameObject enemyObj in allEnemies)
        {
            float distance = Vector3.Distance(currentPosition, enemyObj.transform.position);
            
            if (distance <= range)
            {
                enemiesInRange.Add(enemyObj.transform);
            }
        }
        
        return enemiesInRange.ToArray();
    }
}
