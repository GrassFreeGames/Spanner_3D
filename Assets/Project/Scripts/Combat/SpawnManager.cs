using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages enemy spawning throughout the match based on GameTimer.
/// Spawns waves with escalating difficulty.
/// Place in scene under "--- MANAGERS ---"
/// </summary>
public class SpawnManager : MonoBehaviour
{
    [Header("Spawn Configuration")]
    [Tooltip("Enemy types that can be spawned")]
    public EnemyData[] availableEnemies;
    
    [Tooltip("Distance from player where enemies spawn")]
    public float spawnRadius = 15f;
    
    [Tooltip("Fixed ground level Y position (fallback if raycast fails)")]
    public float groundLevel = 0f;
    
    [Tooltip("Layer mask for ground detection (select Ground layer)")]
    public LayerMask groundLayerMask = 1; // Default layer
    
    [Tooltip("Height to start raycast from")]
    public float raycastStartHeight = 100f;
    
    [Header("Initial Wave Settings (at 10:00)")]
    [Tooltip("Number of enemies in first spawn wave")]
    public int initialEnemyCount = 2;
    
    [Tooltip("Time between spawns at start (in seconds)")]
    public float initialSpawnInterval = 2f;
    
    [Header("Escalation (per minute elapsed)")]
    [Tooltip("How many additional enemies per minute")]
    public int enemyIncreasePerMinute = 1;
    
    [Tooltip("How much faster spawns get per minute (in seconds)")]
    public float intervalDecreasePerMinute = 0.1f;
    
    [Header("References")]
    [Tooltip("Player transform for spawn positioning")]
    public Transform playerTransform;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private fields: _camelCase
    private float _nextSpawnTime;
    private int _currentMinute = 10;
    private List<GameObject> _activeEnemies = new List<GameObject>();
    
    // Singleton pattern
    private static SpawnManager _instance;
    public static SpawnManager Instance => _instance;
    
    // Properties: PascalCase
    public int ActiveEnemyCount => _activeEnemies.Count;
    
    void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning($"Multiple SpawnManager instances found! Destroying duplicate on {gameObject.name}");
            Destroy(this);
            return;
        }
        _instance = this;
        
        // Validate
        if (availableEnemies == null || availableEnemies.Length == 0)
        {
            Debug.LogError("SpawnManager has no available enemies assigned!", this);
            enabled = false;
            return;
        }
        
        // Auto-find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("SpawnManager cannot find player! Tag your player with 'Player' tag.", this);
                enabled = false;
                return;
            }
        }
    }
    
    void Start()
    {
        // Schedule first spawn immediately
        _nextSpawnTime = GameTimer.Instance.CurrentTime;
        
        if (showDebugInfo)
            Debug.Log($"SpawnManager initialized. First spawn at {GameTimer.FormatTime(_nextSpawnTime)}");
    }
    
    void Update()
    {
        // Clean up destroyed enemies from list
        _activeEnemies.RemoveAll(enemy => enemy == null);
        
        // Check if it's time to spawn
        float currentTime = GameTimer.Instance.CurrentTime;
        
        if (currentTime <= _nextSpawnTime)
        {
            SpawnWave();
            ScheduleNextSpawn();
        }
    }
    
    void SpawnWave()
    {
        // Calculate current difficulty based on time elapsed
        int minutesElapsed = GetMinutesElapsed();
        
        // Calculate wave size and interval
        int waveSize = initialEnemyCount + (minutesElapsed * enemyIncreasePerMinute);
        
        if (showDebugInfo)
            Debug.Log($"Spawning wave: {waveSize} enemies at {GameTimer.FormatTime(GameTimer.Instance.CurrentTime)}");
        
        // Spawn enemies
        for (int i = 0; i < waveSize; i++)
        {
            SpawnEnemy();
        }
    }
    
    void SpawnEnemy()
    {
        // Choose random enemy type
        EnemyData enemyData = availableEnemies[Random.Range(0, availableEnemies.Length)];
        
        if (enemyData.enemyPrefab == null)
        {
            Debug.LogError($"EnemyData '{enemyData.enemyName}' has no prefab assigned!", enemyData);
            return;
        }
        
        // Calculate spawn position (random point on circle around player)
        Vector3 spawnPosition = GetRandomSpawnPosition();
        
        // Instantiate enemy
        GameObject enemyObj = Instantiate(enemyData.enemyPrefab, spawnPosition, Quaternion.identity);
        
        // Initialize enemy
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.enemyData = enemyData;
            enemy.InitializeSpawn(spawnPosition);
        }
        else
        {
            Debug.LogError($"Enemy prefab '{enemyData.enemyName}' is missing Enemy component!", enemyObj);
            Destroy(enemyObj);
            return;
        }
        
        // Track active enemy
        _activeEnemies.Add(enemyObj);
        
        if (showDebugInfo)
            Debug.Log($"Spawned {enemyData.enemyName} at {spawnPosition}");
    }
    
    Vector3 GetRandomSpawnPosition()
    {
        // Random angle around player
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        
        // Calculate position on circle
        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * spawnRadius,
            0f,
            Mathf.Sin(angle) * spawnRadius
        );
        
        // XZ position relative to player
        Vector3 spawnXZ = new Vector3(
            playerTransform.position.x + offset.x,
            0f,
            playerTransform.position.z + offset.z
        );
        
        // Detect ground at this XZ position
        Vector3 groundPosition = GetGroundPosition(spawnXZ);
        
        return groundPosition;
    }
    
    Vector3 GetGroundPosition(Vector3 spawnXZ)
    {
        // Cast ray downward from above to find ground
        Vector3 rayStart = new Vector3(spawnXZ.x, raycastStartHeight, spawnXZ.z);
        RaycastHit hit;
        
        if (Physics.Raycast(rayStart, Vector3.down, out hit, raycastStartHeight * 2f, groundLayerMask))
        {
            if (showDebugInfo)
                Debug.Log($"Ground detected at Y={hit.point.y} for spawn position");
            
            return hit.point;
        }
        
        // Fallback to fixed ground level if raycast fails
        if (showDebugInfo)
            Debug.LogWarning($"Raycast failed at {spawnXZ}, using fallback ground level {groundLevel}");
        
        return new Vector3(spawnXZ.x, groundLevel, spawnXZ.z);
    }
    
    void ScheduleNextSpawn()
    {
        // Calculate current spawn interval based on minutes elapsed
        int minutesElapsed = GetMinutesElapsed();
        float currentInterval = Mathf.Max(0.1f, initialSpawnInterval - (minutesElapsed * intervalDecreasePerMinute));
        
        // Schedule next spawn
        _nextSpawnTime = GameTimer.Instance.CurrentTime - currentInterval;
        
        if (showDebugInfo)
            Debug.Log($"Next spawn in {currentInterval}s at {GameTimer.FormatTime(_nextSpawnTime)}");
    }
    
    int GetMinutesElapsed()
    {
        // Calculate how many minutes have passed (0-10)
        float startingTime = 600f; // 10 minutes
        float timeElapsed = startingTime - GameTimer.Instance.CurrentTime;
        return Mathf.FloorToInt(timeElapsed / 60f);
    }
    
    /// <summary>
    /// Get current wave difficulty stats
    /// </summary>
    public void GetCurrentWaveStats(out int enemyCount, out float spawnInterval)
    {
        int minutesElapsed = GetMinutesElapsed();
        enemyCount = initialEnemyCount + (minutesElapsed * enemyIncreasePerMinute);
        spawnInterval = Mathf.Max(0.1f, initialSpawnInterval - (minutesElapsed * intervalDecreasePerMinute));
    }
    
    /// <summary>
    /// Manually trigger a spawn wave (useful for testing or events)
    /// </summary>
    public void ForceSpawnWave()
    {
        SpawnWave();
    }
    
    /// <summary>
    /// Clear all active enemies
    /// </summary>
    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in _activeEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        _activeEnemies.Clear();
        
        if (showDebugInfo)
            Debug.Log("Cleared all enemies");
    }
}
