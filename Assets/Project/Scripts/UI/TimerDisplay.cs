using UnityEngine;
using TMPro;

/// <summary>
/// UI component that displays the GameTimer.
/// Attach to a UI GameObject with TextMeshProUGUI component.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class TimerDisplay : MonoBehaviour
{
    [Header("Display Settings")]
    [Tooltip("Color when time is positive")]
    public Color positiveColor = Color.white;
    
    [Tooltip("Color when time is negative")]
    public Color negativeColor = Color.red;
    
    [Header("Optional References")]
    [Tooltip("If null, will auto-find GameTimer.Instance")]
    public GameTimer gameTimer;
    
    // Private fields: _camelCase
    private TextMeshProUGUI _timerText;
    private bool _wasNegative = false;
    
    void Awake()
    {
        _timerText = GetComponent<TextMeshProUGUI>();
        
        // Auto-find GameTimer if not assigned
        if (gameTimer == null)
        {
            gameTimer = GameTimer.Instance;
        }
        
        // Validate
        if (gameTimer == null)
        {
            Debug.LogError("TimerDisplay cannot find GameTimer! Make sure GameTimer exists in scene.", this);
            enabled = false;
            return;
        }
        
        // Set initial color
        _timerText.color = positiveColor;
    }
    
    void Update()
    {
        if (gameTimer == null || _timerText == null) return;
        
        // Update timer text
        float currentTime = gameTimer.CurrentTime;
        _timerText.text = GameTimer.FormatTime(currentTime);
        
        // Update color when crossing into negative
        bool isNegative = currentTime < 0;
        if (isNegative != _wasNegative)
        {
            _timerText.color = isNegative ? negativeColor : positiveColor;
            _wasNegative = isNegative;
        }
    }
}
