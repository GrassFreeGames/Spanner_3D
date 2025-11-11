using UnityEngine;

/// <summary>
/// Weapon behavior for aura/field attacks.
/// Deals damage in pulses to all enemies within radius.
/// Displays as a flat disc on the ground (Megabonk style).
/// Used by: Static Field
/// </summary>
public class WeaponBehavior_Aura : WeaponBehaviorBase
{
    [Header("Visual Effect")]
    [Tooltip("Flat disc GameObject for the aura (should be child of player, rotated flat to ground)")]
    public GameObject auraDiscVFX;
    
    [Header("Disc Material (Optional Override)")]
    [Tooltip("Material for the aura disc. If not set here, uses material from WeaponData.")]
    public Material auraDiscMaterial;
    
    [Header("Pulse VFX")]
    [Tooltip("VFX prefab spawned on each damage pulse (optional)")]
    public GameObject pulseVFXPrefab;
    
    private float _tickTimer = 0f;
    
    protected override void OnInitialize()
    {
        // Get material from WeaponData first, then check component override
        if (auraDiscMaterial == null && _weapon.weaponData.auraDiscMaterial != null)
        {
            auraDiscMaterial = _weapon.weaponData.auraDiscMaterial;
        }
        
        // Try to find aura disc VFX if not assigned
        if (auraDiscVFX == null)
        {
            Transform existingDisc = _playerTransform.Find("StaticField_Disc");
            if (existingDisc != null)
            {
                auraDiscVFX = existingDisc.gameObject;
            }
            else
            {
                CreateAuraDiscVFX();
            }
        }
        
        // Update disc size
        UpdateDiscSize();
        
        if (pulseVFXPrefab == null)
        {
            pulseVFXPrefab = Resources.Load<GameObject>("Prefabs/VFX_ElectricPulse");
        }
    }
    
    void CreateAuraDiscVFX()
    {
        // Create a flat quad for the disc
        auraDiscVFX = GameObject.CreatePrimitive(PrimitiveType.Quad);
        auraDiscVFX.name = "StaticField_Disc";
        auraDiscVFX.transform.SetParent(_playerTransform);
        auraDiscVFX.transform.localPosition = new Vector3(0, 0.05f, 0); // Slightly above ground
        
        // Rotate to be flat on ground (facing up)
        auraDiscVFX.transform.localRotation = Quaternion.Euler(90, 0, 0);
        
        // Remove collider
        Collider collider = auraDiscVFX.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);
        
        // Setup material
        Renderer renderer = auraDiscVFX.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (auraDiscMaterial != null)
            {
                // Use assigned material (from WeaponData or component)
                renderer.material = auraDiscMaterial;
            }
            else
            {
                // Create default material with transparency
                Material mat = new Material(Shader.Find("Unlit/Transparent"));
                mat.color = new Color(0.3f, 0.7f, 1f, 0.4f); // Electric blue, semi-transparent
                renderer.material = mat;
            }
        }
        
        UpdateDiscSize();
    }
    
    void UpdateDiscSize()
    {
        if (auraDiscVFX == null)
            return;
        
        float range = _weapon.GetFinalRange();
        float diameter = range * 2f;
        
        // Scale the disc to match range
        // Quad is 1x1, so diameter scales it perfectly
        auraDiscVFX.transform.localScale = new Vector3(diameter, diameter, 1f);
    }
    
    public override bool Fire()
    {
        // Aura always fires (pulses) regardless of enemies
        // This is an untargeted AoE weapon
        PerformDamagePulse();
        return true;
    }
    
    void Update()
    {
        // Update disc size in case weapon tags changed
        UpdateDiscSize();
        
        // Make disc pulse visually
        if (auraDiscVFX != null)
        {
            Renderer renderer = auraDiscVFX.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Pulse alpha/brightness
                float pulse = Mathf.Sin(Time.time * 3f) * 0.15f + 0.85f;
                Color baseColor = renderer.material.color;
                baseColor.a = 0.4f * pulse;
                renderer.material.color = baseColor;
            }
        }
    }
    
    void PerformDamagePulse()
    {
        // Get range
        float range = _weapon.GetFinalRange();
        
        // Get all enemies in range
        Transform[] enemies = GetEnemiesInRange(range);
        
        if (enemies.Length == 0)
            return;
        
        float damage = GetFinalDamage();
        
        // Damage all enemies in range
        foreach (Transform enemyTransform in enemies)
        {
            Enemy enemy = enemyTransform.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
        
        // Spawn pulse VFX
        if (pulseVFXPrefab != null)
        {
            GameObject pulseVFX = Instantiate(pulseVFXPrefab, _playerTransform.position, Quaternion.identity);
            pulseVFX.transform.localScale = Vector3.one * (range * 2f);
            Destroy(pulseVFX, 0.5f);
        }
    }
    
    void OnDestroy()
    {
        // Clean up disc VFX
        if (auraDiscVFX != null)
        {
            Destroy(auraDiscVFX);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (_playerTransform == null || _weapon == null)
            return;
        
        // Draw aura radius
        float range = _weapon.GetFinalRange();
        Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.5f);
        Gizmos.DrawWireSphere(_playerTransform.position, range);
    }
}
