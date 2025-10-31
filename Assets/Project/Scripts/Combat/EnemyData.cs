using UnityEngine;

/// <summary>
/// ScriptableObject that defines properties of an enemy type.
/// Create new enemies by right-clicking in Project: Create > Spanner > Enemies > Enemy Data
/// </summary>
[CreateAssetMenu(fileName = "New_EnemyData", menuName = "Spanner/Enemies/Enemy Data", order = 1)]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyName = "New Enemy";
    
    [Header("Prefab")]
    [Tooltip("Complete enemy prefab with model, animations, components")]
    public GameObject enemyPrefab;
    
    [Header("Stats")]
    public float maxHealth = 10f;
    public float moveSpeed = 3f;
    public float damage = 1f;
    
    [Header("Spawn Animation")]
    [Tooltip("How far below ground to start spawn")]
    public float spawnDepth = 2f;
    
    [Tooltip("Scale multiplier for spawn depth (use for larger enemies)")]
    public float sizeMultiplier = 1f;
    
    [Tooltip("How fast enemy rises from ground")]
    public float riseSpeed = 2f;
    
    [Tooltip("Pause duration at ground level before activating")]
    public float pauseDuration = 0.25f;
    
    [Header("Audio")]
    [Tooltip("Wwise event when enemy spawns")]
    public AK.Wwise.Event spawnSound;
    
    [Header("Rewards")]
    [Tooltip("XP token prefab to drop when killed")]
    public GameObject xpTokenPrefab;
    
    [Tooltip("Experience/score value when killed")]
    public int scoreValue = 10;
}
