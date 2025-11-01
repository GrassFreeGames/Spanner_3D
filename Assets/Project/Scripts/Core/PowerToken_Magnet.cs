using UnityEngine;

/// <summary>
/// Magnet PowerToken - pulls all XP tokens on the map toward the player.
/// Attach to PowerToken_Magnet prefab.
/// </summary>
public class PowerToken_Magnet : PowerToken
{
    [Header("Magnet Settings")]
    [Tooltip("Visual effect for magnet activation (optional)")]
    public GameObject magnetEffectPrefab;
    
    protected override void OnCollected(GameObject player)
    {
        Debug.Log("🧲 Magnet activated! Pulling all XP tokens!");
        
        // Find all XP tokens in the scene
        XP_Token[] allTokens = FindObjectsOfType<XP_Token>();
        
        // Force all tokens to be pulled toward player
        foreach (XP_Token token in allTokens)
        {
            token.ForcePull();
        }
        
        // Spawn visual effect if assigned
        if (magnetEffectPrefab != null && player != null)
        {
            Instantiate(magnetEffectPrefab, player.transform.position, Quaternion.identity, player.transform);
        }
        
        Debug.Log($"Magnet pulled {allTokens.Length} XP tokens!");
    }
}
