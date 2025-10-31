using UnityEngine;

/// <summary>
/// XP token that enemies drop. Gets pulled toward player when in range.
/// Attach to flat 2D token prefab.
/// </summary>
public class XP_Token : MonoBehaviour
{
    [Header("Token Properties")]
    [Tooltip("XP value this token grants")]
    public int expValue = 1;
    
    [Header("Movement")]
    [Tooltip("Speed token moves toward player")]
    public float moveSpeed = 8f;
    
    [Tooltip("Distance to player before token is collected")]
    public float collectDistance = 0.5f;
    
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
            Debug.LogError("XP_Token cannot find player! Tag your player with 'Player' tag.", this);
        }
    }
    
    void Update()
    {
        if (_playerTransform == null) return;
        
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
        // Grant XP to player
        ExperienceManager expManager = ExperienceManager.Instance;
        if (expManager != null)
        {
            expManager.AddExperience(expValue);
        }
        
        // Destroy token
        Destroy(gameObject);
    }
}
