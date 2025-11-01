using UnityEngine;

/// <summary>
/// Base class for all PowerToken pickups (Magnet, Shield, Bombs, etc.)
/// Attach to power token prefabs. NO COLLIDER NEEDED.
/// </summary>
public abstract class PowerToken : MonoBehaviour
{
    [Header("Token Properties")]
    [Tooltip("Speed token moves toward player")]
    public float moveSpeed = 8f;
    
    [Tooltip("Distance to player before token is collected")]
    public float collectDistance = 0.5f;
    
    [Header("Visual")]
    [Tooltip("Rotation speed for visual effect")]
    public float rotationSpeed = 90f;
    
    // Private fields: _camelCase
    private Transform _playerTransform;
    private bool _isBeingPulled = false;
    
    void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("PowerToken cannot find player! Tag your player with 'Player' tag.", this);
        }
    }
    
    void Update()
    {
        if (_playerTransform == null) return;
        
        // Rotate for visual effect
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
        
        // Check if player is in pickup radius
        if (!_isBeingPulled)
        {
            PlayerStats playerStats = _playerTransform.GetComponent<PlayerStats>();
            if (playerStats != null && distanceToPlayer <= playerStats.pickupRadius)
            {
                _isBeingPulled = true;
            }
        }
        
        // Move toward player if being pulled
        if (_isBeingPulled)
        {
            // Move toward player
            Vector3 direction = (_playerTransform.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            
            // Check if close enough to collect
            if (distanceToPlayer <= collectDistance)
            {
                CollectToken();
            }
        }
    }
    
    void CollectToken()
    {
        // Trigger power effect
        OnCollected(_playerTransform.gameObject);
        
        // Destroy token
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Override this method to define what happens when token is collected
    /// </summary>
    protected abstract void OnCollected(GameObject player);
}
