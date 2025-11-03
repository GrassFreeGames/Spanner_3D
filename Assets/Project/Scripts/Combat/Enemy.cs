using UnityEngine;

/// <summary>
/// Base enemy component that handles spawn animation and state.
/// Attach to all enemy prefabs.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Configuration")]
    public EnemyData enemyData;
    
    [Header("Ground Detection")]
    [Tooltip("Layer mask for ground detection (assign Ground/Terrain layer)")]
    public LayerMask groundLayerMask = 1; // Default layer
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Enemy states
    public enum EnemyState
    {
        Spawning,   // Rising from ground
        Paused,     // Brief pause at ground level
        Active,     // Normal behavior
        Dead        // Defeated
    }
    
    // Private fields: _camelCase
    private EnemyState _currentState = EnemyState.Spawning;
    private float _currentHealth;
    private Vector3 _targetGroundPosition;
    private EnemyChase3D _chaseComponent;
    private Rigidbody _rb;
    private Collider _collider;
    
    // Properties: PascalCase
    public EnemyState CurrentState => _currentState;
    public float CurrentHealth => _currentHealth;
    public bool IsActive => _currentState == EnemyState.Active;
    public bool IsSpawning => _currentState == EnemyState.Spawning || _currentState == EnemyState.Paused;
    
    void Awake()
    {
        _chaseComponent = GetComponent<EnemyChase3D>();
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        
        if (enemyData != null)
        {
            _currentHealth = enemyData.maxHealth;
            
            // Apply stats to chase component if it exists
            if (_chaseComponent != null)
            {
                _chaseComponent.chaseSpeed = enemyData.moveSpeed;
            }
        }
    }
    
    /// <summary>
    /// Initialize enemy spawn at specific position.
    /// Called by EnemySpawner.
    /// </summary>
    public void InitializeSpawn(Vector3 groundPosition)
    {
        _targetGroundPosition = groundPosition;
        _currentState = EnemyState.Spawning;
        
        // Disable chase behavior during spawn
        if (_chaseComponent != null)
        {
            _chaseComponent.enabled = false;
        }
        
        // Disable physics during spawn - prevents falling/collisions
        if (_rb != null)
        {
            _rb.isKinematic = true;
        }
        
        // Disable collider during spawn - prevents damage/interactions
        if (_collider != null)
        {
            _collider.enabled = false;
        }
        
        // Position below ground
        float adjustedSpawnDepth = enemyData.spawnDepth * enemyData.sizeMultiplier;
        transform.position = groundPosition - Vector3.up * adjustedSpawnDepth;
        
        // Play spawn sound
        if (enemyData.spawnSound != null)
        {
            enemyData.spawnSound.Post(gameObject);
        }
        
        // Start spawn animation
        StartCoroutine(SpawnAnimationCoroutine());
        
        if (showDebugInfo)
            Debug.Log($"{enemyData.enemyName} spawning at {groundPosition}");
    }
    
    System.Collections.IEnumerator SpawnAnimationCoroutine()
    {
        // Phase 1: Rise from ground
        _currentState = EnemyState.Spawning;
        
        while (transform.position.y < _targetGroundPosition.y - 0.01f)
        {
            // Move up, scaled by size for consistent timing
            float riseAmount = enemyData.riseSpeed * enemyData.sizeMultiplier * Time.deltaTime;
            transform.position += Vector3.up * riseAmount;
            
            // Clamp to target if we'd overshoot
            if (transform.position.y >= _targetGroundPosition.y)
            {
                transform.position = new Vector3(
                    transform.position.x,
                    _targetGroundPosition.y,
                    transform.position.z
                );
                break;
            }
            
            yield return null;
        }
        
        // Ensure we're exactly at ground level
        transform.position = new Vector3(
            transform.position.x,
            _targetGroundPosition.y,
            transform.position.z
        );
        
        // Phase 2: Pause at ground level
        _currentState = EnemyState.Paused;
        
        if (showDebugInfo)
            Debug.Log($"{enemyData.enemyName} paused at ground level");
        
        yield return new WaitForSeconds(enemyData.pauseDuration);
        
        // Phase 3: Activate
        _currentState = EnemyState.Active;
        
        // Enable chase behavior
        if (_chaseComponent != null)
        {
            _chaseComponent.enabled = true;
        }
        
        // Enable physics - now affected by gravity/forces
        if (_rb != null)
        {
            _rb.isKinematic = false;
        }
        
        // Enable collider - now can take damage and interact
        if (_collider != null)
        {
            _collider.enabled = true;
        }
        
        if (showDebugInfo)
            Debug.Log($"{enemyData.enemyName} activated!");
    }
    
    /// <summary>
    /// Take damage from player attacks
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (_currentState == EnemyState.Dead) return;
        
        _currentHealth -= damage;
        
        if (showDebugInfo)
            Debug.Log($"{enemyData.enemyName} took {damage} damage. Health: {_currentHealth}/{enemyData.maxHealth}");
        
        if (_currentHealth <= 0)
        {
            Die();
        }
    }
    
    /// <summary>
    /// Enemy death
    /// </summary>
    void Die()
    {
        _currentState = EnemyState.Dead;
        
        if (showDebugInfo)
            Debug.Log($"{enemyData.enemyName} died!");
        
        // Register kill with stats tracker
        GameStatsTracker statsTracker = GameStatsTracker.Instance;
        if (statsTracker != null)
        {
            statsTracker.RegisterKill();
        }
        
        // Get ground position below enemy with hover offset
        Vector3 dropPosition = GetGroundPositionBelow(transform.position);
        
        // Drop XP token at ground level (with hover offset)
        if (enemyData.xpTokenPrefab != null)
        {
            Instantiate(enemyData.xpTokenPrefab, dropPosition, Quaternion.identity);
        }
        
        // 1% chance to drop PowerToken_Magnet
        if (enemyData.powerTokenPrefab != null && Random.Range(0f, 1f) <= enemyData.powerTokenDropChance)
        {
            // Offset slightly so tokens don't overlap
            Vector3 powerTokenPosition = dropPosition + new Vector3(0.5f, 0f, 0f);
            Instantiate(enemyData.powerTokenPrefab, powerTokenPosition, Quaternion.identity);
            
            if (showDebugInfo)
                Debug.Log($"{enemyData.enemyName} dropped a PowerToken!");
        }
        
        // TODO: Death animation, VFX, etc.
        
        // Destroy enemy
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Detect collision with player and deal damage
    /// </summary>
    void OnCollisionStay(Collision collision)
    {
        // Only deal damage when active (not during spawn)
        if (_currentState != EnemyState.Active) return;
        
        // Check if hit player
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                // Try to deal damage (respects per-enemy cooldown)
                // Pass enemy position for knockback calculation
                bool damageDealt = playerStats.TakeDamage(enemyData.damage, gameObject, transform.position);
                
                if (showDebugInfo && damageDealt)
                    Debug.Log($"{enemyData.enemyName} hit player for {enemyData.damage} damage!");
            }
        }
    }
    
    /// <summary>
    /// Raycast down from position to find ground level, with hover offset.
    /// Only detects ground layer, ignores enemies and items.
    /// </summary>
    Vector3 GetGroundPositionBelow(Vector3 position)
    {
        RaycastHit hit;
        Vector3 rayStart = position;
        float hoverHeight = enemyData != null ? enemyData.tokenHoverHeight : 0.3f;
        
        // Cast downward to find ground (ONLY check ground layer, ignore enemies/items)
        if (Physics.Raycast(rayStart, Vector3.down, out hit, 200f, groundLayerMask))
        {
            if (showDebugInfo)
                Debug.Log($"Ground found at Y={hit.point.y}, spawning token at Y={hit.point.y + hoverHeight}");
            
            return hit.point + Vector3.up * hoverHeight;
        }
        
        // Fallback: use Y=0 with hover height
        if (showDebugInfo)
            Debug.LogWarning($"No ground found below {position}, using fallback Y=0. Check groundLayerMask!");
        
        return new Vector3(position.x, hoverHeight, position.z);
    }
}
