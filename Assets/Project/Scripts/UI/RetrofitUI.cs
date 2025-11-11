using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI for weapon retrofit flow.
/// Shows option to swap or retrofit, then tag selection for retrofit.
/// Attach to Canvas.
/// </summary>
public class RetrofitUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Main panel that contains all retrofit UI elements")]
    public GameObject mainPanel;
    
    [Header("Weapon Display")]
    [Tooltip("Icon for equipped weapon (target)")]
    public Image equippedWeaponIcon;
    
    [Tooltip("Name for equipped weapon")]
    public TextMeshProUGUI equippedWeaponNameText;
    
    [Tooltip("Level for equipped weapon")]
    public TextMeshProUGUI equippedWeaponLevelText;
    
    [Tooltip("Icon for new weapon (sacrifice)")]
    public Image newWeaponIcon;
    
    [Tooltip("Name for new weapon")]
    public TextMeshProUGUI newWeaponNameText;
    
    [Header("Action Buttons")]
    [Tooltip("Button to swap weapons")]
    public Button swapButton;
    
    [Tooltip("Button to retrofit (goes to tag selection)")]
    public Button retrofitButton;
    
    [Tooltip("Button to cancel")]
    public Button cancelButton;
    
    [Header("Tag Selection")]
    [Tooltip("Panel for tag selection (hidden initially)")]
    public GameObject tagSelectionPanel;
    
    [Tooltip("Container for tag buttons")]
    public Transform tagButtonContainer;
    
    [Tooltip("Prefab for tag selection button")]
    public GameObject tagButtonPrefab;
    
    [Tooltip("Button to cancel tag selection")]
    public Button cancelTagSelectionButton;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private fields
    private int _targetSlotIndex;
    private WeaponInstance _targetWeapon;
    private WeaponData _sacrificeWeapon;
    private WeaponPickup _weaponPickup;
    private WeaponPickupUI _weaponPickupUI;
    private WeaponManager _weaponManager;
    
    // Singleton pattern
    private static RetrofitUI _instance;
    public static RetrofitUI Instance => _instance;
    
    void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple RetrofitUI instances found! Destroying duplicate.", this);
            Destroy(this);
            return;
        }
        _instance = this;
        
        // Hide initially
        if (mainPanel != null)
            mainPanel.SetActive(false);
        
        if (tagSelectionPanel != null)
            tagSelectionPanel.SetActive(false);
    }
    
    void Start()
    {
        _weaponManager = WeaponManager.Instance;
        
        // Setup button listeners
        if (swapButton != null)
            swapButton.onClick.AddListener(OnSwapButtonClicked);
        
        if (retrofitButton != null)
            retrofitButton.onClick.AddListener(OnRetrofitButtonClicked);
        
        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelButtonClicked);
        
        if (cancelTagSelectionButton != null)
            cancelTagSelectionButton.onClick.AddListener(OnCancelTagSelectionClicked);
    }
    
    /// <summary>
    /// Show retrofit/swap choice UI
    /// </summary>
    public void ShowRetrofitChoice(int slotIndex, WeaponInstance targetWeapon, WeaponData sacrificeWeapon, WeaponPickup weaponPickup, WeaponPickupUI weaponPickupUI)
    {
        _targetSlotIndex = slotIndex;
        _targetWeapon = targetWeapon;
        _sacrificeWeapon = sacrificeWeapon;
        _weaponPickup = weaponPickup;
        _weaponPickupUI = weaponPickupUI;
        
        // Update weapon display
        UpdateWeaponDisplay();
        
        // Show main panel, hide tag selection
        mainPanel.SetActive(true);
        if (tagSelectionPanel != null)
            tagSelectionPanel.SetActive(false);
        
        // Game should already be paused from WeaponPickupUI
        // Cursor should already be unlocked
        
        if (showDebugInfo)
            Debug.Log($"Showing retrofit choice: {targetWeapon.weaponData.weaponName} vs {sacrificeWeapon.weaponName}");
    }
    
    /// <summary>
    /// Hide retrofit UI completely
    /// </summary>
    public void HideRetrofit()
    {
        mainPanel.SetActive(false);
        if (tagSelectionPanel != null)
            tagSelectionPanel.SetActive(false);
        
        _targetWeapon = null;
        _sacrificeWeapon = null;
        _weaponPickup = null;
        _weaponPickupUI = null;
        
        if (showDebugInfo)
            Debug.Log("Retrofit UI hidden");
    }
    
    void UpdateWeaponDisplay()
    {
        // Equipped weapon (target)
        if (equippedWeaponIcon != null)
            equippedWeaponIcon.sprite = _targetWeapon.weaponData.icon;
        
        if (equippedWeaponNameText != null)
            equippedWeaponNameText.text = _targetWeapon.weaponData.weaponName;
        
        if (equippedWeaponLevelText != null)
            equippedWeaponLevelText.text = $"Level {_targetWeapon.weaponLevel}";
        
        // New weapon (sacrifice)
        if (newWeaponIcon != null)
            newWeaponIcon.sprite = _sacrificeWeapon.icon;
        
        if (newWeaponNameText != null)
            newWeaponNameText.text = _sacrificeWeapon.weaponName;
    }
    
    void OnSwapButtonClicked()
    {
        if (_weaponManager == null)
            return;
        
        // Swap weapon in slot
        _weaponManager.SwapWeapon(_targetSlotIndex, _sacrificeWeapon);
        
        // Notify pickup
        if (_weaponPickup != null)
            _weaponPickup.OnWeaponTaken();
        
        // Close all UIs
        HideRetrofit();
        if (_weaponPickupUI != null)
            _weaponPickupUI.HidePickup();
        
        if (showDebugInfo)
            Debug.Log($"Swapped weapon in slot {_targetSlotIndex}");
    }
    
    void OnRetrofitButtonClicked()
    {
        // Show tag selection
        ShowTagSelection();
        
        if (showDebugInfo)
            Debug.Log("Showing tag selection for retrofit");
    }
    
    void ShowTagSelection()
    {
        // Hide main choice panel
        if (mainPanel != null)
            mainPanel.SetActive(false);
        
        // Show tag selection panel
        if (tagSelectionPanel != null)
            tagSelectionPanel.SetActive(true);
        
        // Create tag buttons
        CreateTagButtons();
    }
    
    void CreateTagButtons()
    {
        if (tagButtonContainer == null || tagButtonPrefab == null)
            return;
        
        // Clear existing buttons
        foreach (Transform child in tagButtonContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Get tags from target weapon
        WeaponTag[] tags = _targetWeapon.weaponData.GetTags();
        
        // Create button for each tag
        foreach (WeaponTag tag in tags)
        {
            CreateTagButton(tag);
        }
    }
    
    void CreateTagButton(WeaponTag tag)
    {
        GameObject buttonObj = Instantiate(tagButtonPrefab, tagButtonContainer);
        
        // Setup button
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnTagSelected(tag));
        }
        
        // Get current and next level for this tag
        int currentLevel = _targetWeapon.GetTagLevel(tag);
        int nextLevel = currentLevel + 1;
        
        // Get level up text
        string levelUpText = WeaponTagHelper.GetLevelUpText(tag, currentLevel, nextLevel, _targetWeapon.weaponData);
        
        // Update button text
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = levelUpText;
        }
    }
    
    void OnTagSelected(WeaponTag selectedTag)
    {
        if (_weaponManager == null)
            return;
        
        // Retrofit weapon with selected tag
        _weaponManager.RetrofitWeapon(_targetSlotIndex, _sacrificeWeapon, selectedTag);
        
        // Notify pickup
        if (_weaponPickup != null)
            _weaponPickup.OnWeaponTaken();
        
        // Close all UIs
        HideRetrofit();
        if (_weaponPickupUI != null)
            _weaponPickupUI.HidePickup();
        
        if (showDebugInfo)
            Debug.Log($"Retrofitted with tag: {selectedTag}");
    }
    
    void OnCancelButtonClicked()
    {
        // Go back to weapon pickup UI
        HideRetrofit();
        if (_weaponPickupUI != null)
        {
            _weaponPickupUI.ShowPickup(_sacrificeWeapon, _weaponPickup);
        }
        
        if (showDebugInfo)
            Debug.Log("Cancelled retrofit, returning to pickup UI");
    }
    
    void OnCancelTagSelectionClicked()
    {
        // Go back to swap/retrofit choice
        if (tagSelectionPanel != null)
            tagSelectionPanel.SetActive(false);
        
        if (mainPanel != null)
            mainPanel.SetActive(true);
        
        if (showDebugInfo)
            Debug.Log("Cancelled tag selection, returning to swap/retrofit choice");
    }
}
