using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays player XP bar and level at top of screen.
/// Attach to UI Canvas.
/// </summary>
public class ExperienceUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Image component for XP bar fill")]
    public Image expBarFill;
    
    [Tooltip("Text displaying current level")]
    public TextMeshProUGUI levelText;
    
    [Tooltip("Text displaying XP progress (optional)")]
    public TextMeshProUGUI expText;
    
    [Header("Animation")]
    [Tooltip("How fast bar fills (lerp speed)")]
    public float fillSpeed = 5f;
    
    // Private fields: _camelCase
    private float _targetFillAmount;
    private ExperienceManager _expManager;
    
    void Start()
    {
        _expManager = ExperienceManager.Instance;
        
        if (_expManager == null)
        {
            Debug.LogError("ExperienceUI cannot find ExperienceManager!", this);
            enabled = false;
            return;
        }
        
        if (expBarFill == null)
        {
            Debug.LogError("ExperienceUI missing expBarFill reference!", this);
            enabled = false;
            return;
        }
        
        // Initialize bar
        expBarFill.fillAmount = 0f;
        _targetFillAmount = 0f;
    }
    
    void Update()
    {
        if (_expManager == null) return;
        
        // Update target fill amount
        _targetFillAmount = _expManager.ExpProgress;
        
        // Smoothly lerp bar fill
        expBarFill.fillAmount = Mathf.Lerp(expBarFill.fillAmount, _targetFillAmount, fillSpeed * Time.deltaTime);
        
        // Update level text
        if (levelText != null)
        {
            levelText.text = $"Level {_expManager.CurrentLevel}";
        }
        
        // Update XP text (optional)
        if (expText != null)
        {
            expText.text = $"{Mathf.FloorToInt(_expManager.CurrentExp)} / {_expManager.ExpToNextLevel} XP";
        }
    }
}
