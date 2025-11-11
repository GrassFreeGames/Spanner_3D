using UnityEngine;

/// <summary>
/// Projectile for the weapon system.
/// Supports various projectile behaviors (homing, piercing, explosions).
/// Kinematic movement for precise control without physics interactions.
/// </summary>
[RequireComponent(typeof(Collider))]
public class WeaponProjectile : MonoBehaviour
{
    [Header("Sound Effects")]
    [Tooltip("Sound to play on explosion (for rockets)")]
    public AK.Wwise.Event explosionSound;
    
    [Tooltip("Sound to play when hitting an enemy (for cannon/piercing)")]
    public AK.Wwise.Event impactSound;
    
    // Projectile configuration
    private float _damage;
    private float _speed;
    private float _lifetime;
    private bool _isPiercing;
    private bool _isHoming;
    
    // Explosion configuration
    private bool _hasExplosion;
    private float _explosionDamage;
    private float _explosionRadius;
    private GameObject _explosionVFX;
    
    // Runtime state
    private Transform _target;
    private bool _hasHit = false;
    private Vector3 _velocity;
    private Vector3 _playerPosition;
    
    // Piercing tracking
    private System.Collections.Generic.HashSet<GameObject> _hitEnemies = new System.Collections.Generic.HashSet<GameObject>();
    
    void Awake()
    {
        // Ensure collider is trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        
        // Remove Rigidbody if it exists (we'll handle movement manually)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Destroy(rb);
        }
    }
    
    /// <summary>
    /// Initialize standard homing projectile
    /// </summary>
    public void Initialize(float damage, float speed, float lifetime, Transform target, Vector3 playerPosition, bool isHoming = true)
    {
        _damage = damage;
        _speed = speed;
        _lifetime = lifetime;
        _target = target;
        _playerPosition = playerPosition;
        _isHoming = isHoming;
        _isPiercing = false;
        _hasExplosion = false;
        
        SetInitialVelocity();
    }
    
    /// <summary>
    /// Initialize piercing projectile
    /// </summary>
    public void InitializePiercing(float damage, float speed, float lifetime, Vector3 direction, bool isHoming = false)
    {
        _damage = damage;
        _speed = speed;
        _lifetime = lifetime;
        _playerPosition = transform.position;
        _isHoming = isHoming;
        _isPiercing = true;
        _hasExplosion = false;
        
        // Set velocity in direction
        _velocity = direction.normalized * _speed;
        transform.rotation = Quaternion.LookRotation(direction);
    }
    
    /// <summary>
    /// Initialize explosive projectile (for Chaos Rockets)
    /// </summary>
    public void InitializeExplosive(float damage, float speed, float lifetime, float explosionDamage, float explosionRadius, GameObject explosionVFX = null)
    {
        _damage = damage;
        _speed = speed;
        _lifetime = lifetime;
        _playerPosition = transform.position;
        _isHoming = false;
        _isPiercing = false;
        _hasExplosion = true;
        _explosionDamage = explosionDamage;
        _explosionRadius = explosionRadius;
        _explosionVFX = explosionVFX;
        
        // Set random direction (flat to ground)
        Vector3 randomDirection = Quaternion.Euler(0, Random.Range(0f, 360f), 0) * Vector3.forward;
        _velocity = randomDirection * _speed;
        transform.rotation = Quaternion.LookRotation(randomDirection);
    }
    
    void SetInitialVelocity()
    {
        if (_target != null)
        {
            Vector3 direction = (_target.position - transform.position).normalized;
            _velocity = direction * _speed;
            transform.rotation = Quaternion.LookRotation(direction);
        }
        else
        {
            // No target, fly forward
            _velocity = transform.forward * _speed;
        }
    }
    
    void Update()
    {
        // Count down lifetime
        _lifetime -= Time.deltaTime;
        if (_lifetime <= 0f)
        {
            // Explode if explosive projectile
            if (_hasExplosion)
            {
                Explode();
            }
            
            DestroyProjectile();
            return;
        }
        
        // Track target if homing
        if (_isHoming && _target != null && !_hasHit)
        {
            Vector3 direction = (_target.position - transform.position).normalized;
            _velocity = direction * _speed;
            transform.rotation = Quaternion.LookRotation(direction);
        }
        
        // Move projectile manually (kinematic movement)
        transform.position += _velocity * Time.deltaTime;
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Ignore triggers
        if (other.isTrigger)
            return;
        
        // Check if hit ground/obstacle/terrain (optional tags - won't error if not defined)
        if (IsGroundOrObstacle(other))
        {
            if (_hasExplosion)
            {
                Explode();
            }
            DestroyProjectile();
            return;
        }
        
        // Check if hit an enemy
        if (other.CompareTag("Enemy"))
        {
            // Piercing projectiles can hit multiple enemies
            if (_isPiercing)
            {
                // Don't hit same enemy twice
                if (_hitEnemies.Contains(other.gameObject))
                    return;
                
                _hitEnemies.Add(other.gameObject);
                ApplyDamage(other);
                
                // Play impact sound for piercing hits (cannon)
                if (impactSound != null)
                {
                    impactSound.Post(gameObject);
                }
                
                // Don't destroy, keep flying
                return;
            }
            
            // Non-piercing projectiles
            if (_hasHit) return;
            _hasHit = true;
            
            // Apply direct hit damage (if not explosive, or if explosive also does direct damage)
            if (_damage > 0)
            {
                ApplyDamage(other);
                
                // Play impact sound for non-explosive hits
                if (!_hasExplosion && impactSound != null)
                {
                    impactSound.Post(gameObject);
                }
            }
            
            // Explode if explosive
            if (_hasExplosion)
            {
                Explode();
            }
            
            DestroyProjectile();
        }
    }
    
    /// <summary>
    /// Check if collider is ground/obstacle (safe - won't error if tags don't exist)
    /// </summary>
    bool IsGroundOrObstacle(Collider other)
    {
        // Check common ground/terrain indicators
        string tag = other.tag;
        
        // Check tags (safe even if not defined)
        if (tag == "Ground" || tag == "Obstacle" || tag == "Terrain")
            return true;
        
        // Check layer (optional - won't error if layer doesn't exist)
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer != -1 && other.gameObject.layer == groundLayer)
            return true;
        
        // Check by name as fallback (common naming conventions)
        string name = other.gameObject.name.ToLower();
        if (name.Contains("ground") || name.Contains("terrain") || name.Contains("floor"))
            return true;
        
        return false;
    }
    
    void ApplyDamage(Collider enemyCollider)
    {
        Enemy enemy = enemyCollider.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(_damage);
        }
    }
    
    void Explode()
    {
        // Play explosion sound
        if (explosionSound != null)
        {
            explosionSound.Post(gameObject);
        }
        
        // Spawn VFX if available
        if (_explosionVFX != null)
        {
            GameObject vfx = Instantiate(_explosionVFX, transform.position, Quaternion.identity);
            
            // Scale VFX to match explosion radius
            vfx.transform.localScale = Vector3.one * (_explosionRadius * 2f);
            
            Destroy(vfx, 2f);
        }
        else
        {
            // Create default explosion VFX if none provided
            CreateDefaultExplosionVFX();
        }
        
        // Deal AoE damage
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _explosionRadius);
        foreach (Collider hit in hitColliders)
        {
            if (hit.CompareTag("Enemy"))
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(_explosionDamage);
                }
            }
        }
    }
    
    void CreateDefaultExplosionVFX()
    {
        // Create a simple expanding sphere for explosion visualization
        GameObject explosionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        explosionSphere.transform.position = transform.position;
        explosionSphere.transform.localScale = Vector3.one * (_explosionRadius * 2f);
        
        // Remove collider
        Collider col = explosionSphere.GetComponent<Collider>();
        if (col != null)
            Destroy(col);
        
        // Make it orange/red
        Renderer renderer = explosionSphere.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1f, 0.5f, 0f, 0.6f); // Orange
            mat.SetFloat("_Mode", 3); // Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            renderer.material = mat;
        }
        
        // Destroy after brief moment
        Destroy(explosionSphere, 0.3f);
    }
    
    void DestroyProjectile()
    {
        Destroy(gameObject);
    }
    
    void OnDrawGizmos()
    {
        if (_hasExplosion)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f); // Orange
            Gizmos.DrawWireSphere(transform.position, _explosionRadius);
        }
    }
}
