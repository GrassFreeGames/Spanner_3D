using UnityEngine;
using System.Linq; // Added for the .Except method used in GetDroppableWeapons

/// <summary>
/// ScriptableObject database that defines the pool of weapons available for enemy drops.
/// This allows you to restrict which weapons an enemy type can drop, or define a global pool.
/// Create via right-click: Create > Spanner > Weapons > Weapon Drop Database
/// </summary>
[CreateAssetMenu(fileName = "WeaponDropDatabase", menuName = "Spanner/Weapons/Weapon Drop Database", order = 0)]
public class WeaponDropDatabase : ScriptableObject
{
    [System.Serializable]
    public class DroppableWeapon
    {
        [Tooltip("The WeaponData ScriptableObject that can be dropped.")]
        public WeaponData weaponData;

        [Tooltip("The relative weight/chance of this weapon being chosen from the pool (higher is more likely).")]
        [Range(1, 100)]
        public int dropWeight = 10;
    }

    [Header("Droppable Weapons Pool")]
    [Tooltip("List of weapons available for dropping and their relative drop weights.")]
    public DroppableWeapon[] droppableWeapons;

    /// <summary>
    /// Selects a random WeaponData from the defined droppable pool, taking weights into account.
    /// Used by Enemy.cs to determine which weapon to drop on death.
    /// </summary>
    /// <returns>A randomly selected WeaponData object, or null if the pool is empty or selection fails.</returns>
    public WeaponData GetRandomWeapon()
    {
        if (droppableWeapons == null || droppableWeapons.Length == 0)
        {
            Debug.LogWarning("WeaponDropDatabase is empty! Cannot drop a weapon.", this);
            return null;
        }

        // Calculate total weight for weighted random selection
        int totalWeight = droppableWeapons.Sum(w => w.dropWeight);

        // Safety check against zero total weight
        if (totalWeight <= 0)
        {
            Debug.LogError("All weapons have a drop weight of zero! Cannot select a weapon.", this);
            return null;
        }

        // Choose a random value within the total weight range
        int randomValue = Random.Range(0, totalWeight);

        // Iterate through the list and subtract weights until randomValue is reached
        int runningTotal = 0;
        foreach (var weaponEntry in droppableWeapons)
        {
            runningTotal += weaponEntry.dropWeight;
            if (randomValue < runningTotal)
            {
                // This is the chosen weapon
                return weaponEntry.weaponData;
            }
        }

        // Fallback (should not be reached if weights are positive)
        return null;
    }

    /// <summary>
    /// Returns a list of all unique WeaponData objects that this database can drop.
    /// Useful for UI display or checking what is available.
    /// </summary>
    public WeaponData[] GetDroppableWeapons()
    {
        if (droppableWeapons == null) return new WeaponData[0];

        return droppableWeapons
            .Where(w => w.weaponData != null)
            .Select(w => w.weaponData)
            .Distinct()
            .ToArray();
    }
}