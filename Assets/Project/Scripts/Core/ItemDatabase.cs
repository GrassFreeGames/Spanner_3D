using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ScriptableObject database of all available items.
/// Used for random item selection in chests, shops, and caches.
/// Create via right-click: Create > Spanner > Items > Item Database
/// </summary>
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Spanner/Items/Item Database", order = 0)]
public class ItemDatabase : ScriptableObject
{
    [Header("All Items")]
    [Tooltip("Complete list of all items in the game")]
    public ItemData[] allItems;
    
    [Header("Rarity Weights")]
    [Tooltip("Weight for Common (Rarity 0) items in random rolls")]
    public float commonWeight = 50f;
    
    [Tooltip("Weight for Uncommon (Rarity 1) items in random rolls")]
    public float uncommonWeight = 30f;
    
    [Tooltip("Weight for Rare (Rarity 2) items in random rolls")]
    public float rareWeight = 15f;
    
    [Tooltip("Weight for Epic (Rarity 3) items in random rolls")]
    public float epicWeight = 5f;
    
    /// <summary>
    /// Get random item based on rarity weights
    /// </summary>
    public ItemData GetRandomItem()
    {
        if (allItems == null || allItems.Length == 0)
        {
            Debug.LogError("ItemDatabase has no items!");
            return null;
        }
        
        // Roll for rarity first
        int rarity = RollRarity(false);
        
        // Get all items of that rarity
        ItemData[] itemsOfRarity = allItems.Where(item => item.rarity == rarity).ToArray();
        
        // Fallback: if no items of that rarity, use any item
        if (itemsOfRarity.Length == 0)
        {
            Debug.LogWarning($"No items found for rarity {rarity}. Using random item.");
            return allItems[Random.Range(0, allItems.Length)];
        }
        
        // Return random item of that rarity
        return itemsOfRarity[Random.Range(0, itemsOfRarity.Length)];
    }
    
    /// <summary>
    /// Get random item with guaranteed minimum rarity (for boss chests)
    /// </summary>
    public ItemData GetRandomItemMinRarity(int minRarity)
    {
        if (allItems == null || allItems.Length == 0)
        {
            Debug.LogError("ItemDatabase has no items!");
            return null;
        }
        
        // Roll for rarity with boss bonus
        int rarity = RollRarity(true);
        
        // Ensure minimum rarity
        rarity = Mathf.Max(rarity, minRarity);
        
        // Get all items of that rarity
        ItemData[] itemsOfRarity = allItems.Where(item => item.rarity == rarity).ToArray();
        
        // Fallback: if no items of that rarity, use any item of min rarity or higher
        if (itemsOfRarity.Length == 0)
        {
            itemsOfRarity = allItems.Where(item => item.rarity >= minRarity).ToArray();
            
            if (itemsOfRarity.Length == 0)
            {
                Debug.LogWarning($"No items found for min rarity {minRarity}. Using random item.");
                return allItems[Random.Range(0, allItems.Length)];
            }
        }
        
        // Return random item of that rarity
        return itemsOfRarity[Random.Range(0, itemsOfRarity.Length)];
    }
    
    /// <summary>
    /// Get N unique random items
    /// </summary>
    public ItemData[] GetRandomItems(int count)
    {
        if (allItems == null || allItems.Length == 0)
        {
            Debug.LogError("ItemDatabase has no items!");
            return new ItemData[0];
        }
        
        // Can't get more items than exist
        count = Mathf.Min(count, allItems.Length);
        
        // Create shuffled copy
        List<ItemData> shuffled = new List<ItemData>(allItems);
        
        // Fisher-Yates shuffle
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            ItemData temp = shuffled[i];
            shuffled[i] = shuffled[j];
            shuffled[j] = temp;
        }
        
        // Take first N items
        ItemData[] result = new ItemData[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = shuffled[i];
        }
        
        return result;
    }
    
    /// <summary>
    /// Roll for rarity based on weights
    /// </summary>
    int RollRarity(bool isBossChest)
    {
        if (isBossChest)
        {
            // Boss chest: 50% Rare, 50% Epic
            return Random.Range(0f, 1f) < 0.5f ? 2 : 3;
        }
        
        // Normal roll with weights
        float totalWeight = commonWeight + uncommonWeight + rareWeight + epicWeight;
        float roll = Random.Range(0f, totalWeight);
        
        if (roll < commonWeight)
            return 0; // Common
        else if (roll < commonWeight + uncommonWeight)
            return 1; // Uncommon
        else if (roll < commonWeight + uncommonWeight + rareWeight)
            return 2; // Rare
        else
            return 3; // Epic
    }
}
