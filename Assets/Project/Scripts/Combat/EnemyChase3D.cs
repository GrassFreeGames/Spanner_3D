using UnityEngine;

/// <summary>
/// Simple enemy that chases the player.
/// Now supports physics-based knockback from projectiles.
/// </summary>
public class EnemyChase3D : MonoBehaviour
{
    [Header("Chase Behavior")]
    public Transform player;
    public float chaseSpeed = 3f;
    
    [Header("Physics")]
    [Tooltip("If using Rigidbody for knockback, set this to true")]
    public bool usePhysics = true;
    
    // Private fields: _camelCase
    private Rigidbody _rb;
    private float _knockbackEndTime = 0f;
    
    // Property to check if currently in knockback
    private bool IsInKnockback => Time.time < _knockbackEndTime;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        
        if (usePhysics && _rb == null)
        {
            Debug.LogWarning($"Enemy {gameObject.name} has usePhysics enabled but no Rigidbody component!", this);
        }
        
        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError($"Enemy {gameObject.name} cannot find player! Make sure player has 'Player' tag.", this);
            }
        }
    }

    void Update()
    {
        if (player == null) return;
        
        // Don't move if in knockback
        if (IsInKnockback) return;
        
        // Non-physics movement (for enemies without Rigidbody)
        if (!usePhysics || _rb == null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * chaseSpeed * Time.deltaTime;
            
            // Look at player
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;
        
        // Don't move if in knockback
        if (IsInKnockback) return;
        
        // Physics-based movement (for enemies with Rigidbody)
        if (usePhysics && _rb != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            Vector3 targetVelocity = direction * chaseSpeed;
            _rb.linearVelocity = new Vector3(targetVelocity.x, _rb.linearVelocity.y, targetVelocity.z);
            
            // Look at player
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        }
    }
    
    /// <summary>
    /// Apply knockback to this enemy. Called by projectiles.
    /// Direction should be normalized vector away from player.
    /// </summary>
    public void ApplyKnockback(Vector3 direction, float force, float duration)
    {
        if (_rb == null) return;
        
        // Set knockback end time
        _knockbackEndTime = Time.time + duration;
        
        // Stop current movement
        _rb.linearVelocity = Vector3.zero;
        
        // Apply knockback impulse (keep Y small to prevent flying)
        Vector3 knockbackForce = direction * force;
        knockbackForce.y = Mathf.Min(knockbackForce.y, force * 0.2f); // Limit upward force
        
        _rb.AddForce(knockbackForce, ForceMode.Impulse);
        
        // Temporarily increase drag for smooth deceleration
        StartCoroutine(KnockbackDecayCoroutine(duration));
    }
    
    private System.Collections.IEnumerator KnockbackDecayCoroutine(float duration)
    {
        // Store original drag
        float originalDrag = _rb.linearDamping;
        
        // Set high drag for natural deceleration (velocity decays automatically)
        _rb.linearDamping = 8f;
        
        // Wait for knockback duration
        yield return new WaitForSeconds(duration);
        
        // Restore original drag
        _rb.linearDamping = originalDrag;
        
        // Ensure velocity is cleared when knockback ends
        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x * 0.1f, _rb.linearVelocity.y, _rb.linearVelocity.z * 0.1f);
    }
}
