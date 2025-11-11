using UnityEngine;

/// <summary>
/// Weapon behavior for rocket spray attacks.
/// Fires projectiles in random 360° directions that explode on impact.
/// Used by: Chaos Rockets
/// </summary>
public class WeaponBehavior_RocketSpray : WeaponBehaviorBase
{
    private GameObject _rocketPrefab;
    private GameObject _explosionVFXPrefab;
    
    protected override void OnInitialize()
    {
        // Get rocket prefab from weapon data
        _rocketPrefab = _weapon.weaponData.projectilePrefab;
        
        if (_rocketPrefab == null)
        {
            Debug.LogError($"WeaponBehavior_RocketSpray: {_weapon.weaponData.weaponName} has no projectile prefab assigned in WeaponData!");
        }
        
        // Get explosion VFX from weapon data (persists across sessions!)
        _explosionVFXPrefab = _weapon.weaponData.explosionVFXPrefab;
        
        if (_explosionVFXPrefab == null)
        {
            Debug.LogWarning($"WeaponBehavior_RocketSpray: {_weapon.weaponData.weaponName} has no explosion VFX assigned. Will use default.");
        }
    }
    
    public override bool Fire()
    {
        if (_rocketPrefab == null)
        {
            Debug.LogError("Cannot fire rockets: no prefab assigned!");
            return false;
        }
        
        // Get final projectile count
        int rocketCount = _weapon.GetFinalProjectileCount();
        
        // Fire rockets in random directions
        // This is an untargeted weapon - always fires regardless of enemies
        for (int i = 0; i < rocketCount; i++)
        {
            FireRocket();
        }
        
        return true; // Always fires
    }
    
    void FireRocket()
    {
        // Spawn rocket at player position
        Vector3 spawnPos = GetSpawnPosition();
        GameObject rocketObj = Instantiate(_rocketPrefab, spawnPos, Quaternion.identity);
        
        // Configure rocket
        WeaponProjectile rocket = rocketObj.GetComponent<WeaponProjectile>();
        if (rocket != null)
        {
            // Base stats
            float directDamage = GetFinalDamage() * 0.3f; // Direct hit damage (low)
            float speed = 12f * _weapon.GetProjectileSpeedMultiplier();
            
            // Randomize lifetime (±50% of max range)
            float baseLifetime = _weapon.GetFinalRange() / speed;
            float lifetimeVariation = baseLifetime * 0.5f;
            float lifetime = baseLifetime + Random.Range(-lifetimeVariation, lifetimeVariation);
            lifetime = Mathf.Max(0.2f, lifetime); // Minimum lifetime
            
            // Explosion stats
            float explosionDamage = GetFinalDamage() * _weapon.GetExplosionDamageMultiplier();
            float explosionRadius = 3f * _weapon.GetExplosionRadiusMultiplier();
            
            // Initialize explosive projectile (VFX comes from WeaponData)
            rocket.InitializeExplosive(directDamage, speed, lifetime, explosionDamage, explosionRadius, _explosionVFXPrefab);
        }
        else
        {
            Debug.LogError("Rocket prefab is missing WeaponProjectile component!");
            Destroy(rocketObj);
        }
    }
}
