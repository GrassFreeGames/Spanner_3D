using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays player's current credits in the HUD.
/// Attach to UI Canvas element with TextMeshProUGUI.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class CreditsUI : MonoBehaviour
{
    [Header("Display Settings")]
    [Tooltip("Prefix text before credit amount")]
    public string prefix = "Credits: ";
    
    [Tooltip("Color for credit text")]
    public Color creditColor = Color.yellow;
    
    [Header("Animation")]
    [Tooltip("Enable bounce animation when credits change")]
    public bool animateOnChange = true;
    
    [Tooltip("Scale multiplier for bounce animation")]
    public float bounceScale = 1.2f;
    
    [Tooltip("Duration of bounce animation")]
    public float bounceDuration = 0.2f;
    
    // Private fields: _camelCase
    private TextMeshProUGUI _creditText;
    private Vector3 _originalScale;
    private float _bounceTimer = 0f;
    
    void Awake()
    {
        _creditText = GetComponent<TextMeshProUGUI>();
        _creditText.color = creditColor;
        _originalScale = transform.localScale;
    }
    
    void Start()
    {
        // Subscribe to currency changes
        CurrencyManager currency = CurrencyManager.Instance;
        if (currency != null)
        {
            currency.OnCreditsChanged += OnCreditsChanged;
            
            // Set initial value
            UpdateDisplay(currency.CurrentCredits);
        }
        else
        {
            Debug.LogError("CreditsUI cannot find CurrencyManager!", this);
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        CurrencyManager currency = CurrencyManager.Instance;
        if (currency != null)
        {
            currency.OnCreditsChanged -= OnCreditsChanged;
        }
    }
    
    void Update()
    {
        // Handle bounce animation
        if (_bounceTimer > 0f)
        {
            _bounceTimer -= Time.unscaledDeltaTime;
            float progress = 1f - (_bounceTimer / bounceDuration);
            
            // Bounce in then out
            float scale = 1f;
            if (progress < 0.5f)
            {
                // Scale up
                scale = Mathf.Lerp(1f, bounceScale, progress * 2f);
            }
            else
            {
                // Scale down
                scale = Mathf.Lerp(bounceScale, 1f, (progress - 0.5f) * 2f);
            }
            
            transform.localScale = _originalScale * scale;
            
            if (_bounceTimer <= 0f)
            {
                transform.localScale = _originalScale;
            }
        }
    }
    
    /// <summary>
    /// Called when credits change
    /// </summary>
    void OnCreditsChanged(int newAmount)
    {
        UpdateDisplay(newAmount);
        
        // Trigger bounce animation
        if (animateOnChange)
        {
            _bounceTimer = bounceDuration;
        }
    }
    
    /// <summary>
    /// Update text display
    /// </summary>
    void UpdateDisplay(int credits)
    {
        if (_creditText != null)
        {
            _creditText.text = $"{prefix}{credits}";
        }
    }
}
