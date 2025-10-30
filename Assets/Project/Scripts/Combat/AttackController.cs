using UnityEngine;

/// <summary>
/// Controls automatic attacking at a regular interval.
/// Cycles through nearest enemies based on attack speed.
/// Attach to CharacterPlayer and assign an AttackData asset.
/// </summary>
public class AttackController : MonoBehaviour
{
    [Header("Attack Configuration")]
    [Tooltip("The attack data to use. Can be swapped for testing different attacks.")]
    public AttackData currentAttack;
    
    [Header("Spawn Point")]
    [Tooltip("Where projectiles spawn from. If null, uses this object's position.")]
    public Transform projectileSpawnPoint;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private fields: _camelCase
    private float _attackTimer = 0f;
    private Transform _cachedTransform;
    private int _currentTargetIndex = 0;

    void Start()
    {
        _cachedTransform = transform;
        
        // Validate setup
        if (currentAttack == null)
        {
            Debug.LogWarning($"AttackController on {gameObject.name} has no AttackData assigned!", this);
            enabled = false;
            return;
        }
        
        if (currentAttack.projectilePrefab == null)
        {
            Debug.LogError($"AttackData '{currentAttack.attackName}' has no projectile prefab assigned!", currentAttack);
            enabled = false;
            return;
        }
        
        // Start with attack ready
        _attackTimer = currentAttack.AttackInterval;
    }

    void Update()
    {
        if (currentAttack == null) return;
        
        // Count down to next attack
        _attackTimer += Time.deltaTime;
        
        // Check if ready to attack
        if (_attackTimer >= currentAttack.AttackInterval)
        {
            TriggerAttack();
            _attackTimer = 0f;
        }
    }

    void TriggerAttack()
    {
        // Get list of closest enemies
        Transform[] closestEnemies = GetClosestEnemies();
        
        if (closestEnemies.Length == 0)
        {
            if (showDebugInfo)
                Debug.Log($"{currentAttack.attackName}: No enemies in range");
            return;
        }
        
        // Get target from cycling list
        Transform target = closestEnemies[_currentTargetIndex];
        
        // Increment index for next attack and wrap around
        _currentTargetIndex = (_currentTargetIndex + 1) % closestEnemies.Length;
        
        // Play fire sound (if assigned)
        if (currentAttack.fireSound != null)
        {
            currentAttack.fireSound.Post(gameObject);
        }
        
        // Spawn projectile
        Vector3 spawnPosition = projectileSpawnPoint != null 
            ? projectileSpawnPoint.position 
            : _cachedTransform.position;
            
        GameObject projectileObj = Instantiate(
            currentAttack.projectilePrefab, 
            spawnPosition, 
            Quaternion.identity
        );
        
        // Configure projectile
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(currentAttack, target, _cachedTransform.position);
        }
        else
        {
            Debug.LogError($"Projectile prefab is missing Projectile component!", projectileObj);
            Destroy(projectileObj);
        }
        
        if (showDebugInfo)
            Debug.Log($"{currentAttack.attackName} fired at {target.name} (target {_currentTargetIndex}/{closestEnemies.Length})");
    }

    Transform[] GetClosestEnemies()
    {
        // Find all enemies in scene
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        if (allEnemies.Length == 0)
            return new Transform[0];
        
        // Calculate how many enemies to target based on attack speed
        int targetCount = CalculateTargetCount();
        
        // Limit to actual number of enemies available
        targetCount = Mathf.Min(targetCount, allEnemies.Length);
        
        // Create array of enemy transforms with distances
        System.Collections.Generic.List<(Transform enemy, float distance)> enemyDistances = 
            new System.Collections.Generic.List<(Transform, float)>();
        
        Vector3 currentPosition = _cachedTransform.position;
        
        foreach (GameObject enemyObj in allEnemies)
        {
            float distance = Vector3.Distance(currentPosition, enemyObj.transform.position);
            
            // Check range limit if set
            if (currentAttack.maxTargetRange > 0 && distance > currentAttack.maxTargetRange)
                continue;
            
            enemyDistances.Add((enemyObj.transform, distance));
        }
        
        // Sort by distance
        enemyDistances.Sort((a, b) => a.distance.CompareTo(b.distance));
        
        // Take the closest N enemies
        Transform[] result = new Transform[Mathf.Min(targetCount, enemyDistances.Count)];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = enemyDistances[i].enemy;
        }
        
        // Reset index if it's beyond the current enemy count
        if (_currentTargetIndex >= result.Length)
            _currentTargetIndex = 0;
        
        return result;
    }

    int CalculateTargetCount()
    {
        // Base: 4 enemies for attack speed <= 2.0
        // +1 enemy for each full 1.0 attack speed above 2.0
        // Examples:
        // 2.0 or below -> 4 enemies
        // 2.1 to 2.99 -> 5 enemies
        // 3.0 to 3.99 -> 6 enemies
        // 4.0 to 4.99 -> 7 enemies
        
        float attacksPerSecond = currentAttack.attacksPerSecond;
        
        if (attacksPerSecond <= 2.0f)
            return 4;
        else
            return 5 + Mathf.FloorToInt(attacksPerSecond - 2.0f);
    }
    
    // Public method to manually trigger attack (useful for testing or special abilities)
    public void ForceAttack()
    {
        TriggerAttack();
    }
    
    // Public method to change attack at runtime
    public void SetAttack(AttackData newAttack)
    {
        currentAttack = newAttack;
        _attackTimer = 0f; // Reset timer
        _currentTargetIndex = 0; // Reset cycling
    }
}
