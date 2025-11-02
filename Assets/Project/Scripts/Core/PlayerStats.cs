using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Holds player stats like pickup radius, health, etc.
/// Attach to player GameObject.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerStats : MonoBehaviour
{
    [Header("Pickup")]
    [Tooltip("Radius around player where XP tokens start moving toward player")]
    public float pickupRadius = 3f;
    
    [Header("Health")]
    [Tooltip("Maximum health")]
    public float maxHealth = 100f;
    
    [Tooltip("Invulnerability time after taking damage (in seconds)")]
    public float invulnerabilityDuration = 0f; // 0 = no i-frames
    
    [Header("Enemy Kick")]
    [Tooltip("Force applied when taking damage from enemies")]
    public float enemyKickForce = 5f;
    
    [Tooltip("How long enemy kick lasts (brief stun)")]
    public float enemyKickDuration = 0.1f;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private fields: _camelCase
    private float _currentHealth;
    private bool _isAlive = true;
    private float _lastDamageTime = -999f;
    private Rigidbody _rb;
    private float _enemyKickEndTime = 0f;
    
    // Track last damage time per enemy to prevent spam damage
    private Dictionary<GameObject, float> _enemyDamageCooldowns = new Dictionary<GameObject, float>();
    private const float DAMAGE_COOLDOWN = 0.1f; // 10 hits per second max
    
    // Properties: PascalCase
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercent => _currentHealth / maxHealth;
    public bool IsAlive => _isAlive;
    public bool IsInvulnerable => Time.time < _lastDamageTime + invulnerabilityDuration;
    public bool IsInEnemyKick => Time.time < _enemyKickEndTime;
    
    // Singleton pattern for easy access
    private static PlayerStats _instance;
    public static PlayerStats Instance => _instance;
    
    void Awake()
    {
        _currentHealth = maxHealth;
        _rb = GetComponent<Rigidbody>();
        
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple PlayerStats instances found! Destroying duplicate.", this);
            Destroy(this);
            return;
        }
        _instance = this;
    }
    
    void Update()
    {
        // Clean up destroyed enemy references from cooldown dictionary
        List<GameObject> keysToRemove = new List<GameObject>();
        foreach (var key in _enemyDamageCooldowns.Keys)
        {
            if (key == null)
            {
                keysToRemove.Add(key);
            }
        }
        foreach (var key in keysToRemove)
        {
            _enemyDamageCooldowns.Remove(key);
        }
    }
    
    /// <summary>
    /// Take damage from an enemy. Returns true if damage was applied.
    /// sourcePosition is used to calculate knockback direction.
    /// </summary>
    public bool TakeDamage(float damage, GameObject source = null, Vector3? sourcePosition = null)
    {
        if (!_isAlive) return false;
        if (IsInvulnerable) return false;
        
        // Check per-enemy cooldown
        if (source != null)
        {
            if (_enemyDamageCooldowns.ContainsKey(source))
            {
                float timeSinceLastHit = Time.time - _enemyDamageCooldowns[source];
                if (timeSinceLastHit < DAMAGE_COOLDOWN)
                {
                    return false; // Still on cooldown for this enemy
                }
            }
            
            // Update cooldown for this enemy
            _enemyDamageCooldowns[source] = Time.time;
        }
        
        // Apply armor damage reduction
        UpgradeManager upgradeManager = UpgradeManager.Instance;
        if (upgradeManager != null)
        {
            damage = upgradeManager.ApplyArmor(damage);
        }
        
        // Apply damage
        _currentHealth -= damage;
        _lastDamageTime = Time.time;
        
        if (showDebugInfo)
            Debug.Log($"Player took {damage} damage. Health: {_currentHealth}/{maxHealth}");
        
        // Apply enemy kick
        if (sourcePosition.HasValue && _rb != null)
        {
            ApplyEnemyKick(sourcePosition.Value);
        }
        
        // Check for death
        if (_currentHealth <= 0)
        {
            Die();
        }
        
        return true;
    }
    
    /// <summary>
    /// Apply enemy kick force away from damage source
    /// </summary>
    void ApplyEnemyKick(Vector3 sourcePosition)
    {
        // Calculate direction away from source
        Vector3 kickDirection = (transform.position - sourcePosition).normalized;
        
        // Keep kick mostly horizontal (reduce Y component)
        kickDirection.y = Mathf.Min(kickDirection.y, 0.2f);
        kickDirection.Normalize();
        
        // Clear current velocity and apply kick impulse
        _rb.linearVelocity = Vector3.zero;
        _rb.AddForce(kickDirection * enemyKickForce, ForceMode.Impulse);
        
        // Set enemy kick end time
        _enemyKickEndTime = Time.time + enemyKickDuration;
        
        if (showDebugInfo)
            Debug.Log($"Player kicked back with force {enemyKickForce}!");
    }
    
    /// <summary>
    /// Heal the player
    /// </summary>
    public void Heal(float amount)
    {
        if (!_isAlive) return;
        
        _currentHealth = Mathf.Min(_currentHealth + amount, maxHealth);
        
        if (showDebugInfo)
            Debug.Log($"Player healed {amount}. Health: {_currentHealth}/{maxHealth}");
    }
    
    /// <summary>
    /// Player death
    /// </summary>
    void Die()
    {
        if (!_isAlive) return;
        
        _isAlive = false;
        _currentHealth = 0f;
        
        Debug.Log("💀 Player died!");
        
        // TODO: Trigger game over screen, death animation, etc.
        OnPlayerDeath();
    }
    
    /// <summary>
    /// Called when player dies. Hook for game over logic.
    /// </summary>
    void OnPlayerDeath()
    {
        // Pause game, show game over UI, etc.
        // For now, just log
        Debug.Log("Game Over! Implement game over screen here.");
        
        // Optional: Restart level after delay
        // Invoke("RestartLevel", 3f);
    }
    
    /// <summary>
    /// Visualize pickup radius in editor
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
