using UnityEngine;

/// <summary>
/// Base enemy component that handles spawn animation and state.
/// Attach to all enemy prefabs.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Configuration")]
    public EnemyData enemyData;
    
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
        transform.position = groundPosition - Vector3.up * enemyData.spawnDepth;
        
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
        
        while (transform.position.y < _targetGroundPosition.y)
        {
            transform.position += Vector3.up * enemyData.riseSpeed * Time.deltaTime;
            
            // Clamp to not overshoot
            if (transform.position.y > _targetGroundPosition.y)
            {
                transform.position = new Vector3(
                    transform.position.x,
                    _targetGroundPosition.y,
                    transform.position.z
                );
            }
            
            yield return null;
        }
        
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
        
        // TODO: Death animation, VFX, drop loot, etc.
        
        // For now, just destroy
        Destroy(gameObject);
    }
}
