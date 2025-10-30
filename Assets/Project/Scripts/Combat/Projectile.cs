using UnityEngine;

/// <summary>
/// Projectile that moves toward a target and deals damage on impact.
/// Attach to projectile prefabs and they will be initialized by AttackController.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    // Private fields: _camelCase
    private AttackData _attackData;
    private Transform _target;
    private Rigidbody _rb;
    private float _lifetime;
    private bool _hasHit = false;
    private Vector3 _playerPosition; // Store player position for knockback direction

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        
        // Configure rigidbody for projectile physics
        _rb.useGravity = false;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    /// <summary>
    /// Initialize the projectile with attack data and target.
    /// Called by AttackController after instantiation.
    /// </summary>
    public void Initialize(AttackData attackData, Transform target, Vector3 playerPosition)
    {
        _attackData = attackData;
        _target = target;
        _playerPosition = playerPosition;
        _lifetime = attackData.projectileLifetime;
        
        // Set initial velocity toward target
        if (_target != null)
        {
            Vector3 direction = (_target.position - transform.position).normalized;
            _rb.linearVelocity = direction * attackData.projectileSpeed;
            
            // Rotate to face direction of travel
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    void Update()
    {
        // Count down lifetime
        _lifetime -= Time.deltaTime;
        if (_lifetime <= 0f)
        {
            DestroyProjectile();
            return;
        }
        
        // Track target if still exists
        if (_target != null && !_hasHit)
        {
            Vector3 direction = (_target.position - transform.position).normalized;
            _rb.linearVelocity = direction * _attackData.projectileSpeed;
            
            // Rotate to face direction of travel
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Ignore if already hit something
        if (_hasHit) return;
        
        // Check if hit an enemy
        if (other.CompareTag("Enemy"))
        {
            _hasHit = true;
            
            // Apply damage (when health system is implemented)
            // For now, just log it
            Debug.Log($"Projectile hit {other.gameObject.name} for {_attackData.damage} damage");
            
            // Apply knockback
            ApplyKnockback(other);
            
            // Destroy projectile
            DestroyProjectile();
        }
    }

    void ApplyKnockback(Collider enemyCollider)
    {
        EnemyChase3D enemy = enemyCollider.GetComponent<EnemyChase3D>();
        if (enemy != null)
        {
            // Calculate knockback direction: from player through enemy (radial pushback)
            Vector3 knockbackDirection = (enemyCollider.transform.position - _playerPosition).normalized;
            
            // Flatten Y component for horizontal-only knockback
            knockbackDirection.y = 0;
            knockbackDirection.Normalize();
            
            // Apply knockback to enemy
            enemy.ApplyKnockback(knockbackDirection, _attackData.knockbackForce, _attackData.knockbackDuration);
            
            if (Debug.isDebugBuild)
                Debug.Log($"Applied {_attackData.knockbackForce} knockback to {enemyCollider.gameObject.name} for {_attackData.knockbackDuration}s");
        }
    }

    void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}
