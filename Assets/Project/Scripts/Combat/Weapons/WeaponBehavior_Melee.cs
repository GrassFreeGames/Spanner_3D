using UnityEngine;

/// <summary>
/// Weapon behavior for melee slash attacks.
/// Alternates left-right, right-left slashes in a cone in front of player.
/// Always fires regardless of enemies (player swings constantly).
/// Used by: Energy Sword
/// </summary>
public class WeaponBehavior_Melee : WeaponBehaviorBase
{
    [Header("Melee VFX (Optional Override)")]
    [Tooltip("VFX prefab for slash effect. If not set here, uses VFX from WeaponData.")]
    public GameObject slashVFXPrefab;
    
    // Slash state
    private bool _isLeftSlash = true; // Alternates each attack
    
    protected override void OnInitialize()
    {
        // Get VFX from WeaponData first, then check component override
        if (slashVFXPrefab == null && _weapon.weaponData.slashVFXPrefab != null)
        {
            slashVFXPrefab = _weapon.weaponData.slashVFXPrefab;
        }
        
        // Try to find slash VFX in Resources if still not assigned
        if (slashVFXPrefab == null)
        {
            slashVFXPrefab = Resources.Load<GameObject>("Prefabs/VFX_EnergySlash");
        }
    }
    
    public override bool Fire()
    {
        // Energy Sword always fires - player swings constantly
        // This is different from other melee weapons which may check for targets
        
        // Get final range (slash distance)
        float range = _weapon.GetFinalRange();
        
        // Get slash cone angle (45 degrees base, increased by Area tag)
        float coneAngle = 45f;
        if (_weapon.weaponData.HasTag(WeaponTag.Area))
        {
            int areaLevel = _weapon.GetTagLevel(WeaponTag.Area);
            float areaBonus = WeaponTagHelper.CalculateTagValue(WeaponTag.Area, areaLevel, "AreaSize");
            coneAngle *= (1f + areaBonus);
        }
        
        // Perform slash (damages any enemies in cone)
        PerformSlash(range, coneAngle);
        
        // Spawn VFX (always shows slash visual)
        if (slashVFXPrefab != null)
        {
            SpawnSlashVFX(range, coneAngle);
        }
        
        // Alternate slash direction for next attack
        _isLeftSlash = !_isLeftSlash;
        
        return true; // Always fires
    }
    
    void PerformSlash(float range, float coneAngle)
    {
        // Get all enemies
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        if (allEnemies.Length == 0)
            return; // No damage dealt, but slash still happened
        
        float damage = GetFinalDamage();
        Vector3 playerPos = _playerTransform.position;
        Vector3 playerForward = _playerTransform.forward;
        
        // Get slash direction offset
        float slashAngleOffset = _isLeftSlash ? -coneAngle / 2f : coneAngle / 2f;
        Vector3 slashCenter = Quaternion.Euler(0, slashAngleOffset, 0) * playerForward;
        
        foreach (GameObject enemyObj in allEnemies)
        {
            Vector3 toEnemy = enemyObj.transform.position - playerPos;
            float distance = toEnemy.magnitude;
            
            // Check if in range
            if (distance > range)
                continue;
            
            // Check if in cone
            Vector3 toEnemyNormalized = toEnemy.normalized;
            float angleToEnemy = Vector3.Angle(playerForward, toEnemyNormalized);
            
            if (angleToEnemy <= coneAngle / 2f)
            {
                // Hit enemy
                Enemy enemy = enemyObj.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    
                    // Apply knockback if Heavy tag
                    if (_weapon.weaponData.HasTag(WeaponTag.Heavy))
                    {
                        EnemyChase3D chase = enemyObj.GetComponent<EnemyChase3D>();
                        if (chase != null)
                        {
                            float knockbackForce = 5f * _weapon.GetKnockbackMultiplier();
                            chase.ApplyKnockback(toEnemyNormalized, knockbackForce, 0.3f);
                        }
                    }
                }
            }
        }
    }
    
    void SpawnSlashVFX(float range, float coneAngle)
    {
        // Spawn slash VFX at player position
        Vector3 spawnPos = _playerTransform.position + _playerTransform.forward * (range * 0.5f);
        
        // Calculate rotation based on slash direction
        float slashRotation = _isLeftSlash ? -30f : 30f;
        Quaternion rotation = _playerTransform.rotation * Quaternion.Euler(0, slashRotation, 0);
        
        GameObject vfx = Instantiate(slashVFXPrefab, spawnPos, rotation);
        
        // Scale VFX based on range
        float scale = range / 3f; // Normalize to base range of 3
        vfx.transform.localScale = Vector3.one * scale;
        
        // Auto-destroy after 1 second
        Destroy(vfx, 1f);
    }
    
    void OnDrawGizmosSelected()
    {
        if (_playerTransform == null || _weapon == null)
            return;
        
        // Draw slash cone
        float range = _weapon.GetFinalRange();
        float coneAngle = 45f;
        
        if (_weapon.weaponData.HasTag(WeaponTag.Area))
        {
            int areaLevel = _weapon.GetTagLevel(WeaponTag.Area);
            float areaBonus = WeaponTagHelper.CalculateTagValue(WeaponTag.Area, areaLevel, "AreaSize");
            coneAngle *= (1f + areaBonus);
        }
        
        Vector3 playerPos = _playerTransform.position;
        Vector3 playerForward = _playerTransform.forward;
        
        // Draw left edge
        Vector3 leftEdge = Quaternion.Euler(0, -coneAngle / 2f, 0) * playerForward;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(playerPos, leftEdge * range);
        
        // Draw right edge
        Vector3 rightEdge = Quaternion.Euler(0, coneAngle / 2f, 0) * playerForward;
        Gizmos.DrawRay(playerPos, rightEdge * range);
        
        // Draw arc
        Gizmos.color = Color.yellow;
        Vector3 previousPoint = playerPos + leftEdge * range;
        for (int i = 1; i <= 10; i++)
        {
            float angle = Mathf.Lerp(-coneAngle / 2f, coneAngle / 2f, i / 10f);
            Vector3 direction = Quaternion.Euler(0, angle, 0) * playerForward;
            Vector3 point = playerPos + direction * range;
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }
    }
}
