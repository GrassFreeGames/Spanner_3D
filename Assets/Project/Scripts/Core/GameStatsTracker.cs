using UnityEngine;

/// <summary>
/// Tracks game statistics during the match (kills, time alive, etc.)
/// Singleton accessible from anywhere.
/// </summary>
public class GameStatsTracker : MonoBehaviour
{
    // Private fields: _camelCase
    private int _enemiesKilled = 0;
    private float _timeAlive = 0f;
    private bool _isAlive = true;
    
    // Properties: PascalCase
    public int EnemiesKilled => _enemiesKilled;
    public float TimeAlive => _timeAlive;
    public bool IsAlive => _isAlive;
    
    // Singleton pattern
    private static GameStatsTracker _instance;
    public static GameStatsTracker Instance => _instance;
    
    void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple GameStatsTracker instances found! Destroying duplicate.", this);
            Destroy(this);
            return;
        }
        _instance = this;
    }
    
    void Update()
    {
        // Track time alive using GameTimer (excludes paused time)
        if (_isAlive && GameTimer.Instance != null)
        {
            // GameTimer counts down, so time alive = starting time - current time
            float startingTime = 600f; // 10 minutes
            _timeAlive = startingTime - GameTimer.Instance.CurrentTime;
        }
    }
    
    /// <summary>
    /// Increment enemy kill count
    /// </summary>
    public void RegisterKill()
    {
        _enemiesKilled++;
    }
    
    /// <summary>
    /// Mark player as dead
    /// </summary>
    public void RegisterDeath()
    {
        _isAlive = false;
    }
    
    /// <summary>
    /// Reset stats for new game
    /// </summary>
    public void ResetStats()
    {
        _enemiesKilled = 0;
        _timeAlive = 0f;
        _isAlive = true;
    }
}
