using UnityEngine;

/// <summary>
/// Holds player stats like pickup radius, health, etc.
/// Attach to player GameObject.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("Pickup")]
    [Tooltip("Radius around player where XP tokens start moving toward player")]
    public float pickupRadius = 3f;
    
    [Header("Health (TODO)")]
    public float maxHealth = 100f;
    
    // Private fields: _camelCase
    private float _currentHealth;
    
    // Properties: PascalCase
    public float CurrentHealth => _currentHealth;
    
    void Awake()
    {
        _currentHealth = maxHealth;
    }
    
    // Singleton pattern for easy access
    private static PlayerStats _instance;
    public static PlayerStats Instance => _instance;
    
    void Start()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple PlayerStats instances found! Destroying duplicate.", this);
            Destroy(this);
            return;
        }
        _instance = this;
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
