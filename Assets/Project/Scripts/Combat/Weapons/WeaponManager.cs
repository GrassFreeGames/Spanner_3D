using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages player's equipped weapons.
/// Handles weapon slots, equipping, swapping, and retrofitting.
/// Place in scene under "--- MANAGERS ---"
/// </summary>
public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Slots")]
    [Tooltip("Number of weapon slots (default 2, can increase later)")]
    public int maxWeaponSlots = 2;
    
    [Header("Starting Weapons")]
    [Tooltip("Weapons player starts with (can be empty)")]
    public WeaponData[] startingWeapons;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Equipped weapons (null = empty slot)
    private WeaponInstance[] _equippedWeapons;
    
    // Events for UI updates
    public event System.Action OnWeaponsChanged;
    
    // Singleton pattern
    private static WeaponManager _instance;
    public static WeaponManager Instance => _instance;
    
    void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple WeaponManager instances found! Destroying duplicate.", this);
            Destroy(this);
            return;
        }
        _instance = this;
        
        // Initialize weapon slots
        _equippedWeapons = new WeaponInstance[maxWeaponSlots];
    }
    
    void Start()
    {
        // Equip starting weapons
        if (startingWeapons != null)
        {
            for (int i = 0; i < startingWeapons.Length && i < maxWeaponSlots; i++)
            {
                if (startingWeapons[i] != null)
                {
                    EquipWeaponToSlot(startingWeapons[i], i);
                }
            }
        }
        
        if (showDebugInfo)
            Debug.Log($"WeaponManager initialized with {maxWeaponSlots} slots");
    }
    
    /// <summary>
    /// Get weapon in a specific slot (null if empty)
    /// </summary>
    public WeaponInstance GetWeapon(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxWeaponSlots) return null;
        return _equippedWeapons[slotIndex];
    }
    
    /// <summary>
    /// Get all equipped weapons (including nulls for empty slots)
    /// </summary>
    public WeaponInstance[] GetAllWeapons()
    {
        return _equippedWeapons;
    }
    
    /// <summary>
    /// Find first empty slot index, or -1 if all full
    /// </summary>
    public int FindEmptySlot()
    {
        for (int i = 0; i < maxWeaponSlots; i++)
        {
            if (_equippedWeapons[i] == null)
                return i;
        }
        return -1;
    }
    
    /// <summary>
    /// Check if player has any equipped weapon matching this WeaponData
    /// </summary>
    public int FindEquippedWeaponSlot(WeaponData weaponData)
    {
        for (int i = 0; i < maxWeaponSlots; i++)
        {
            if (_equippedWeapons[i] != null && _equippedWeapons[i].weaponData == weaponData)
                return i;
        }
        return -1;
    }
    
    /// <summary>
    /// Equip a weapon to a specific slot
    /// </summary>
    public void EquipWeaponToSlot(WeaponData weaponData, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxWeaponSlots)
        {
            Debug.LogError($"Invalid slot index: {slotIndex}");
            return;
        }
        
        if (weaponData == null)
        {
            Debug.LogError("Attempted to equip null weapon!");
            return;
        }
        
        // Create new weapon instance
        _equippedWeapons[slotIndex] = new WeaponInstance(weaponData);
        
        // Notify listeners
        OnWeaponsChanged?.Invoke();
        
        if (showDebugInfo)
            Debug.Log($"Equipped {weaponData.weaponName} to slot {slotIndex}");
    }
    
    /// <summary>
    /// Swap weapon in slot with new weapon
    /// </summary>
    public void SwapWeapon(int slotIndex, WeaponData newWeapon)
    {
        if (slotIndex < 0 || slotIndex >= maxWeaponSlots)
        {
            Debug.LogError($"Invalid slot index: {slotIndex}");
            return;
        }
        
        WeaponInstance oldWeapon = _equippedWeapons[slotIndex];
        
        // Replace with new weapon
        _equippedWeapons[slotIndex] = new WeaponInstance(newWeapon);
        
        // Notify listeners
        OnWeaponsChanged?.Invoke();
        
        if (showDebugInfo)
        {
            string oldName = oldWeapon != null ? oldWeapon.weaponData.weaponName : "empty";
            Debug.Log($"Swapped {oldName} for {newWeapon.weaponName} in slot {slotIndex}");
        }
    }
    
    /// <summary>
    /// Retrofit a weapon into an equipped weapon (level up + tag upgrade)
    /// </summary>
    public void RetrofitWeapon(int targetSlotIndex, WeaponData sacrificeWeapon, WeaponTag upgradeTag)
    {
        if (targetSlotIndex < 0 || targetSlotIndex >= maxWeaponSlots)
        {
            Debug.LogError($"Invalid slot index: {targetSlotIndex}");
            return;
        }
        
        WeaponInstance targetWeapon = _equippedWeapons[targetSlotIndex];
        if (targetWeapon == null)
        {
            Debug.LogError($"No weapon in slot {targetSlotIndex} to retrofit!");
            return;
        }
        
        // Level up the weapon
        targetWeapon.LevelUpWeapon();
        
        // Check if same weapon type
        bool isSameWeapon = targetWeapon.weaponData == sacrificeWeapon;
        
        if (isSameWeapon)
        {
            // Same weapon: upgrade all tags!
            targetWeapon.UpgradeAllTags();
            
            if (showDebugInfo)
                Debug.Log($"Retrofitted {sacrificeWeapon.weaponName} into itself! All tags upgraded.");
        }
        else
        {
            // Different weapon: upgrade only selected tag
            targetWeapon.UpgradeTag(upgradeTag);
            
            if (showDebugInfo)
                Debug.Log($"Retrofitted {sacrificeWeapon.weaponName} into {targetWeapon.weaponData.weaponName}. Upgraded {upgradeTag} tag.");
        }
        
        // Notify listeners
        OnWeaponsChanged?.Invoke();
    }
    
    /// <summary>
    /// Remove weapon from slot
    /// </summary>
    public void UnequipWeapon(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxWeaponSlots)
        {
            Debug.LogError($"Invalid slot index: {slotIndex}");
            return;
        }
        
        _equippedWeapons[slotIndex] = null;
        OnWeaponsChanged?.Invoke();
        
        if (showDebugInfo)
            Debug.Log($"Unequipped weapon from slot {slotIndex}");
    }
    
    /// <summary>
    /// Get count of equipped weapons
    /// </summary>
    public int GetEquippedWeaponCount()
    {
        int count = 0;
        for (int i = 0; i < maxWeaponSlots; i++)
        {
            if (_equippedWeapons[i] != null)
                count++;
        }
        return count;
    }
    
    /// <summary>
    /// Clear all weapons (for testing/reset)
    /// </summary>
    public void ClearAllWeapons()
    {
        for (int i = 0; i < maxWeaponSlots; i++)
        {
            _equippedWeapons[i] = null;
        }
        OnWeaponsChanged?.Invoke();
        
        if (showDebugInfo)
            Debug.Log("All weapons cleared");
    }
}
