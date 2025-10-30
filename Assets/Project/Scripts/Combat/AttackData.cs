using UnityEngine;

/// <summary>
/// ScriptableObject that defines the properties of an attack.
/// Create new attacks by right-clicking in Project: Create > Spanner > Combat > Attack Data
/// </summary>
[CreateAssetMenu(fileName = "New_AttackData", menuName = "Spanner/Combat/Attack Data", order = 1)]
public class AttackData : ScriptableObject
{
    [Header("Attack Identity")]
    public string attackName = "New Attack";
    
    [Header("Timing")]
    [Tooltip("Number of attacks per second")]
    public float attacksPerSecond = 2f;
    
    [Header("Projectile")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float projectileLifetime = 5f;
    
    [Header("Damage")]
    public float damage = 1f;
    
    [Header("Knockback")]
    public float knockbackForce = 3f;
    
    [Tooltip("Duration of knockback stun in seconds")]
    public float knockbackDuration = 0.25f;
    
    [Header("Audio")]
    [Tooltip("Wwise event to play when attack fires. Leave empty for silent attacks.")]
    public AK.Wwise.Event fireSound;
    
    [Header("Targeting")]
    [Tooltip("Maximum range to search for targets. 0 = unlimited")]
    public float maxTargetRange = 50f;
    
    // Property: PascalCase
    public float AttackInterval => 1f / attacksPerSecond;
}
