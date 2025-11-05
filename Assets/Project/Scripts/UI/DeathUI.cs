using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays "You Died" screen with "Okay :(" button.
/// Attach to death panel UI element.
/// </summary>
public class DeathUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Death panel (will be shown/hidden)")]
    public GameObject deathPanel;
    
    [Tooltip("Canvas Group for visibility control")]
    public CanvasGroup panelCanvasGroup;
    
    [Tooltip("'You Died' text")]
    public TextMeshProUGUI deathText;
    
    [Tooltip("Okay button")]
    public Button okayButton;
    
    [Tooltip("Okay button text (to set ':(' face)")]
    public TextMeshProUGUI okayButtonText;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Singleton pattern
    private static DeathUI _instance;
    public static DeathUI Instance => _instance;
    
    void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple DeathUI instances found! Destroying duplicate.", this);
            Destroy(this);
            return;
        }
        _instance = this;
        
        // Get or add CanvasGroup
        if (deathPanel != null)
        {
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = deathPanel.GetComponent<CanvasGroup>();
                if (panelCanvasGroup == null)
                {
                    panelCanvasGroup = deathPanel.AddComponent<CanvasGroup>();
                }
            }
            
            // Hide initially
            HidePanelImmediate();
        }
    }
    
    void Start()
    {
        // Setup button listener
        if (okayButton != null)
        {
            okayButton.onClick.AddListener(OnOkayClicked);
        }
        else
        {
            Debug.LogError("DeathUI is missing Okay button reference!", this);
        }
        
        // Set button text
        if (okayButtonText != null)
        {
            okayButtonText.text = "Okay :(";
        }
        
        // Set death text
        if (deathText != null)
        {
            deathText.text = "You Died";
        }
    }
    
    /// <summary>
    /// Show death screen
    /// </summary>
    public void ShowDeathScreen()
    {
        // Show panel
        ShowPanelImmediate();
        
        // Unlock and show cursor for button interaction
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        if (showDebugInfo)
            Debug.Log("Death screen shown - cursor unlocked and visible");
    }
    
    /// <summary>
    /// Hide death screen
    /// </summary>
    public void HideDeathScreen()
    {
        HidePanelImmediate();
        
        if (showDebugInfo)
            Debug.Log("Death screen hidden");
    }
    
    /// <summary>
    /// Handle "Okay :(" button click
    /// </summary>
    void OnOkayClicked()
    {
        if (showDebugInfo)
            Debug.Log("Okay button clicked - showing post-game stats");
        
        // Stop camera orbit
        DeathController deathController = DeathController.Instance;
        if (deathController != null)
        {
            deathController.StopCameraOrbit();
        }
        
        // Hide death screen
        HideDeathScreen();
        
        // Game stays paused (Time.timeScale = 0) for stats screen
        
        // Show post-game stats
        PostGameStatsUI statsUI = PostGameStatsUI.Instance;
        if (statsUI != null)
        {
            statsUI.ShowStatsScreen();
        }
        else
        {
            Debug.LogError("DeathUI cannot find PostGameStatsUI!", this);
        }
    }
    
    void HidePanelImmediate()
    {
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;
        }
    }
    
    void ShowPanelImmediate()
    {
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 1f;
            panelCanvasGroup.interactable = true;
            panelCanvasGroup.blocksRaycasts = true;
        }
    }
}
