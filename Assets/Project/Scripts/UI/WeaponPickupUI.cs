using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI for weapon pickup flow.
/// Shows options: Equip to empty slot, Swap with equipped weapon, or Retrofit into equipped weapon.
/// Attach to Canvas.
/// </summary>
public class WeaponPickupUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Main panel that contains all pickup UI elements")]
    public GameObject mainPanel;
    
    [Tooltip("Weapon icon display")]
    public Image weaponIcon;
    
    [Tooltip("Weapon name text")]
    public TextMeshProUGUI weaponNameText;
    
    [Tooltip("Weapon description text")]
    public TextMeshProUGUI weaponDescriptionText;
    
    [Tooltip("Weapon tags display text")]
    public TextMeshProUGUI weaponTagsText;
    
    [Header("Action Buttons")]
    [Tooltip("Button to equip to empty slot")]
    public Button equipButton;
    
    [Tooltip("Text on equip button")]
    public TextMeshProUGUI equipButtonText;
    
    [Tooltip("Container for weapon slot buttons")]
    public Transform weaponSlotButtonContainer;
    
    [Tooltip("Prefab for weapon slot button")]
    public GameObject weaponSlotButtonPrefab;
    
    [Tooltip("Button to close UI without taking weapon")]
    public Button closeButton;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private fields
    private WeaponData _currentWeaponData;
    private WeaponPickup _currentPickup;
    private WeaponManager _weaponManager;
    private RetrofitUI _retrofitUI;
    
    // Properties
    public bool IsOpen => mainPanel != null && mainPanel.activeSelf;
    
    // Singleton pattern
    private static WeaponPickupUI _instance;
    public static WeaponPickupUI Instance => _instance;
    
    void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple WeaponPickupUI instances found! Destroying duplicate.", this);
            Destroy(this);
            return;
        }
        _instance = this;
        
        // Hide initially
        if (mainPanel != null)
            mainPanel.SetActive(false);
    }
    
    void Start()
    {
        _weaponManager = WeaponManager.Instance;
        _retrofitUI = RetrofitUI.Instance;
        
        // Setup button listeners
        if (equipButton != null)
            equipButton.onClick.AddListener(OnEquipButtonClicked);
        
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseButtonClicked);
    }
    
    /// <summary>
    /// Show weapon pickup UI
    /// </summary>
    public void ShowPickup(WeaponData weaponData, WeaponPickup pickup)
    {
        _currentWeaponData = weaponData;
        _currentPickup = pickup;
        
        // Update weapon info display
        UpdateWeaponInfo();
        
        // Update action buttons
        UpdateActionButtons();
        
        // Show panel
        mainPanel.SetActive(true);
        
        // Pause game
        Time.timeScale = 0f;
        
        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (showDebugInfo)
            Debug.Log($"Weapon pickup UI shown for {weaponData.weaponName}");
    }
    
    /// <summary>
    /// Hide weapon pickup UI
    /// </summary>
    public void HidePickup()
    {
        mainPanel.SetActive(false);
        
        // Resume game
        Time.timeScale = 1f;
        
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        _currentWeaponData = null;
        _currentPickup = null;
        
        if (showDebugInfo)
            Debug.Log("Weapon pickup UI hidden");
    }
    
    void UpdateWeaponInfo()
    {
        if (_currentWeaponData == null)
            return;
        
        // Update icon
        if (weaponIcon != null)
            weaponIcon.sprite = _currentWeaponData.icon;
        
        // Update name
        if (weaponNameText != null)
            weaponNameText.text = _currentWeaponData.weaponName;
        
        // Update description
        if (weaponDescriptionText != null)
            weaponDescriptionText.text = _currentWeaponData.description;
        
        // Update tags
        if (weaponTagsText != null)
        {
            WeaponTag[] tags = _currentWeaponData.GetTags();
            string tagsString = $"Tags: {tags[0]}, {tags[1]}, {tags[2]}";
            weaponTagsText.text = tagsString;
        }
    }
    
    void UpdateActionButtons()
    {
        if (_weaponManager == null)
            return;
        
        // Check if there's an empty slot
        int emptySlot = _weaponManager.FindEmptySlot();
        
        if (emptySlot >= 0)
        {
            // Show equip button
            if (equipButton != null)
            {
                equipButton.gameObject.SetActive(true);
                if (equipButtonText != null)
                    equipButtonText.text = $"Equip to Slot {emptySlot + 1}";
            }
        }
        else
        {
            // Hide equip button (all slots full)
            if (equipButton != null)
                equipButton.gameObject.SetActive(false);
        }
        
        // Create weapon slot buttons for swap/retrofit
        CreateWeaponSlotButtons();
    }
    
    void CreateWeaponSlotButtons()
    {
        if (weaponSlotButtonContainer == null || weaponSlotButtonPrefab == null)
            return;
        
        // Clear existing buttons
        foreach (Transform child in weaponSlotButtonContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create button for each equipped weapon
        WeaponInstance[] weapons = _weaponManager.GetAllWeapons();
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] == null)
                continue;
            
            CreateWeaponSlotButton(i, weapons[i]);
        }
    }
    
    void CreateWeaponSlotButton(int slotIndex, WeaponInstance weapon)
    {
        GameObject buttonObj = Instantiate(weaponSlotButtonPrefab, weaponSlotButtonContainer);
        
        // Setup button
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnWeaponSlotClicked(slotIndex));
        }
        
        // Update button text
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = $"Slot {slotIndex + 1}: {weapon.weaponData.weaponName} (Lv.{weapon.weaponLevel})";
        }
        
        // Update button icon
        Image buttonIcon = buttonObj.GetComponentInChildren<Image>();
        if (buttonIcon != null && weapon.weaponData.icon != null)
        {
            buttonIcon.sprite = weapon.weaponData.icon;
        }
    }
    
    void OnEquipButtonClicked()
    {
        if (_weaponManager == null || _currentWeaponData == null)
            return;
        
        int emptySlot = _weaponManager.FindEmptySlot();
        if (emptySlot < 0)
        {
            Debug.LogWarning("No empty slot available!");
            return;
        }
        
        // Equip weapon
        _weaponManager.EquipWeaponToSlot(_currentWeaponData, emptySlot);
        
        // Notify pickup
        if (_currentPickup != null)
            _currentPickup.OnWeaponTaken();
        
        // Close UI
        HidePickup();
        
        if (showDebugInfo)
            Debug.Log($"Equipped {_currentWeaponData.weaponName} to slot {emptySlot}");
    }
    
    void OnWeaponSlotClicked(int slotIndex)
    {
        WeaponInstance targetWeapon = _weaponManager.GetWeapon(slotIndex);
        if (targetWeapon == null)
        {
            Debug.LogWarning($"No weapon in slot {slotIndex}!");
            return;
        }
        
        // Check if same weapon type
        bool isSameWeapon = targetWeapon.weaponData == _currentWeaponData;
        
        if (isSameWeapon)
        {
            // Same weapon: Auto-retrofit all tags!
            _weaponManager.RetrofitWeapon(slotIndex, _currentWeaponData, WeaponTag.Area); // Tag doesn't matter for same weapon
            
            // Notify pickup
            if (_currentPickup != null)
                _currentPickup.OnWeaponTaken();
            
            // Close UI
            HidePickup();
            
            if (showDebugInfo)
                Debug.Log($"Retrofitted {_currentWeaponData.weaponName} into itself! All tags upgraded.");
        }
        else
        {
            // Different weapon: Show retrofit UI to choose tag OR offer swap
            ShowSwapOrRetrofitChoice(slotIndex, targetWeapon);
        }
    }
    
    void ShowSwapOrRetrofitChoice(int slotIndex, WeaponInstance targetWeapon)
    {
        // Hide this UI temporarily
        mainPanel.SetActive(false);
        
        // Show retrofit UI
        if (_retrofitUI != null)
        {
            _retrofitUI.ShowRetrofitChoice(slotIndex, targetWeapon, _currentWeaponData, _currentPickup, this);
        }
        else
        {
            Debug.LogError("RetrofitUI not found! Cannot show retrofit/swap choice.");
            HidePickup();
        }
    }
    
    void OnCloseButtonClicked()
    {
        // Notify pickup that UI was closed without taking weapon
        if (_currentPickup != null)
            _currentPickup.OnUIClosedWithoutTaking();
        
        HidePickup();
        
        if (showDebugInfo)
            Debug.Log("Weapon pickup UI closed without taking weapon");
    }
}
