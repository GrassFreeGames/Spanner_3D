using UnityEngine;
using System.Linq;

/// <summary>
/// ScriptableObject database of all weapons.
/// Used for random weapon selection in drops and weapon crates.
/// Create via right-click: Create > Spanner > Weapons > Weapon Database
/// </summary>
[CreateAssetMenu(fileName = "WeaponDatabase", menuName = "Spanner/Weapons/Weapon Database", order = 0)]
public class WeaponDatabase : ScriptableObject
{
    [Header("All Weapons")]
    [Tooltip("Complete list of all weapons in the game")]
    public WeaponData[] allWeapons;
    
    /// <summary>
    /// Get a random weapon from the database
    /// </summary>
    public WeaponData GetRandomWeapon()
    {
        if (allWeapons == null || allWeapons.Length == 0)
        {
            Debug.LogError("WeaponDatabase has no weapons!");
            return null;
        }
        
        return allWeapons[Random.Range(0, allWeapons.Length)];
    }
    
    /// <summary>
    /// Get N unique random weapons
    /// </summary>
    public WeaponData[] GetRandomWeapons(int count)
    {
        if (allWeapons == null || allWeapons.Length == 0)
        {
            Debug.LogError("WeaponDatabase has no weapons!");
            return new WeaponData[0];
        }
        
        // Can't get more weapons than exist
        count = Mathf.Min(count, allWeapons.Length);
        
        // Create shuffled copy
        System.Collections.Generic.List<WeaponData> shuffled = new System.Collections.Generic.List<WeaponData>(allWeapons);
        
        // Fisher-Yates shuffle
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            WeaponData temp = shuffled[i];
            shuffled[i] = shuffled[j];
            shuffled[j] = temp;
        }
        
        // Take first N weapons
        WeaponData[] result = new WeaponData[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = shuffled[i];
        }
        
        return result;
    }
    
    /// <summary>
    /// Get weapon by name
    /// </summary>
    public WeaponData GetWeaponByName(string weaponName)
    {
        if (allWeapons == null || allWeapons.Length == 0)
            return null;
        
        return allWeapons.FirstOrDefault(w => w.weaponName == weaponName);
    }
}
