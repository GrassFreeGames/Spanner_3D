using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Central game timer that counts down from a starting time.
/// Other systems can subscribe to events or query current time.
/// Place in scene under "--- MANAGERS ---"
/// </summary>
public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("Starting time in seconds (600 = 10 minutes)")]
    public float startingTime = 600f;
    
    [Tooltip("Timer starts immediately on scene load")]
    public bool startOnAwake = true;
    
    [Header("Events")]
    [Tooltip("Fired when timer reaches exactly 0")]
    public UnityEvent onTimerReachedZero;
    
    [Tooltip("Fired when timer goes negative")]
    public UnityEvent onTimerNegative;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private fields: _camelCase
    private float _currentTime;
    private bool _isRunning = false;
    private bool _hasReachedZero = false;
    
    // Singleton pattern for easy access
    private static GameTimer _instance;
    public static GameTimer Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameTimer>();
                if (_instance == null)
                {
                    Debug.LogError("GameTimer not found in scene! Add GameTimer component to a GameObject.");
                }
            }
            return _instance;
        }
    }
    
    // Properties: PascalCase
    public float CurrentTime => _currentTime;
    public bool IsRunning => _isRunning;
    public bool IsNegative => _currentTime < 0;
    public bool HasReachedZero => _hasReachedZero;
    
    void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning($"Multiple GameTimer instances found! Destroying duplicate on {gameObject.name}");
            Destroy(this);
            return;
        }
        _instance = this;
        
        // Initialize
        _currentTime = startingTime;
        
        if (startOnAwake)
        {
            StartTimer();
        }
    }
    
    void Update()
    {
        if (!_isRunning) return;
        
        // Count down time
        _currentTime -= Time.deltaTime;
        
        // Check for zero crossing
        if (!_hasReachedZero && _currentTime <= 0f)
        {
            _hasReachedZero = true;
            onTimerReachedZero?.Invoke();
            
            if (showDebugInfo)
                Debug.Log("GameTimer reached zero!");
        }
        
        // Check for negative
        if (_hasReachedZero && _currentTime < 0f)
        {
            onTimerNegative?.Invoke();
            
            if (showDebugInfo && Mathf.FloorToInt(-_currentTime) % 10 == 0) // Log every 10 seconds in negative
                Debug.Log($"GameTimer negative: {FormatTime(_currentTime)}");
        }
    }
    
    /// <summary>
    /// Start the timer counting down
    /// </summary>
    public void StartTimer()
    {
        _isRunning = true;
        
        if (showDebugInfo)
            Debug.Log("GameTimer started");
    }
    
    /// <summary>
    /// Pause the timer
    /// </summary>
    public void PauseTimer()
    {
        _isRunning = false;
        
        if (showDebugInfo)
            Debug.Log("GameTimer paused");
    }
    
    /// <summary>
    /// Resume the timer
    /// </summary>
    public void ResumeTimer()
    {
        _isRunning = true;
        
        if (showDebugInfo)
            Debug.Log("GameTimer resumed");
    }
    
    /// <summary>
    /// Reset timer to starting time
    /// </summary>
    public void ResetTimer()
    {
        _currentTime = startingTime;
        _hasReachedZero = false;
        
        if (showDebugInfo)
            Debug.Log("GameTimer reset");
    }
    
    /// <summary>
    /// Set timer to specific time
    /// </summary>
    public void SetTime(float time)
    {
        _currentTime = time;
        _hasReachedZero = _currentTime <= 0;
        
        if (showDebugInfo)
            Debug.Log($"GameTimer set to {FormatTime(time)}");
    }
    
    /// <summary>
    /// Add time to current timer (can be negative to subtract)
    /// </summary>
    public void AddTime(float additionalTime)
    {
        _currentTime += additionalTime;
        
        if (showDebugInfo)
            Debug.Log($"GameTimer adjusted by {additionalTime}s, now at {FormatTime(_currentTime)}");
    }
    
    /// <summary>
    /// Format time as MM:SS string
    /// </summary>
    public static string FormatTime(float timeInSeconds)
    {
        bool isNegative = timeInSeconds < 0;
        float absTime = Mathf.Abs(timeInSeconds);
        
        int minutes = Mathf.FloorToInt(absTime / 60f);
        int seconds = Mathf.FloorToInt(absTime % 60f);
        
        string sign = isNegative ? "-" : "";
        return $"{sign}{minutes:0}:{seconds:00}";
    }
    
    /// <summary>
    /// Get time remaining in minutes (for spawn triggers, etc.)
    /// </summary>
    public float GetMinutesRemaining()
    {
        return _currentTime / 60f;
    }
    
    /// <summary>
    /// Check if we've passed a specific time threshold
    /// Useful for spawn triggers: HasPassedTime(480) = "has 8 minutes passed?"
    /// </summary>
    public bool HasPassedTime(float thresholdTime)
    {
        return _currentTime <= (startingTime - thresholdTime);
    }
}
