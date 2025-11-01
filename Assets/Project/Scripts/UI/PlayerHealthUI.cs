using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays player health bar and health text.
/// Attach to UI Canvas or health bar panel.
/// </summary>
public class PlayerHealthUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Image component for health bar fill")]
    public Image healthBarFill;
    
    [Tooltip("Text displaying current/max health (optional)")]
    public TextMeshProUGUI healthText;
    
    [Header("Colors")]
    [Tooltip("Health bar color when above 50%")]
    public Color healthyColor = Color.green;
    
    [Tooltip("Health bar color when below 50%")]
    public Color damagedColor = Color.yellow;
    
    [Tooltip("Health bar color when below 25%")]
    public Color criticalColor = Color.red;
    
    [Header("Animation")]
    [Tooltip("How fast bar fills/depletes (lerp speed)")]
    public float fillSpeed = 5f;
    
    // Private fields: _camelCase
    private float _targetFillAmount;
    private PlayerStats _playerStats;
    
    void Start()
    {
        _playerStats = PlayerStats.Instance;
        
        if (_playerStats == null)
        {
            Debug.LogError("PlayerHealthUI cannot find PlayerStats!", this);
            enabled = false;
            return;
        }
        
        if (healthBarFill == null)
        {
            Debug.LogError("PlayerHealthUI missing healthBarFill reference!", this);
            enabled = false;
            return;
        }
        
        // Initialize bar to full
        healthBarFill.fillAmount = 1f;
        _targetFillAmount = 1f;
    }
    
    void Update()
    {
        if (_playerStats == null) return;
        
        // Update target fill amount
        _targetFillAmount = _playerStats.HealthPercent;
        
        // Smoothly lerp bar fill
        healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, _targetFillAmount, fillSpeed * Time.deltaTime);
        
        // Update health bar color based on health percentage
        UpdateHealthBarColor(_playerStats.HealthPercent);
        
        // Update health text (optional)
        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(_playerStats.CurrentHealth)} / {_playerStats.MaxHealth}";
        }
    }
    
    void UpdateHealthBarColor(float healthPercent)
    {
        if (healthPercent > 0.5f)
        {
            // Healthy - green
            healthBarFill.color = healthyColor;
        }
        else if (healthPercent > 0.25f)
        {
            // Damaged - yellow
            healthBarFill.color = damagedColor;
        }
        else
        {
            // Critical - red
            healthBarFill.color = criticalColor;
        }
    }
}
