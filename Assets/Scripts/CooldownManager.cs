using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Simple cooldown system that handles both turn-based and once-per-battle cooldowns
/// </summary>
public class CooldownManager : MonoBehaviour
{
    public static CooldownManager Instance;

    // Cooldown types
    public enum CooldownType
    {
        TurnBased,      // Can be used again after X turns
        OncePerBattle,  // Can only be used once per battle
        UsesPerBattle   // Can be used X times per battle
    }
 

    // Storage for cooldowns
    private Dictionary<string, Dictionary<string, CooldownData>> cooldowns = 
        new Dictionary<string, Dictionary<string, CooldownData>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Check if a skill/ability is on cooldown
    /// </summary>
    /// <param name="player">Player using the skill ("white" or "black")</param>
    /// <param name="skillName">Name of the skill/ability</param>
    /// <returns>True if on cooldown, false if available</returns>
    /// 
    /// 
    public bool IsOnCooldown(string player, string skillName)
    {
        if (!cooldowns.ContainsKey(player) || !cooldowns[player].ContainsKey(skillName))
            return false;

        CooldownData data = cooldowns[player][skillName];
        
        if (data.type == CooldownType.OncePerBattle)
        {
            return data.isUsed;
        }
        else if (data.type == CooldownType.UsesPerBattle)
        {
            return data.usesRemaining <= 0;
        }
        else if (data.type == CooldownType.TurnBased)
        {
            Game game = FindObjectOfType<Game>();
            if (game != null)
            {
                return game.turns < data.availableAfterTurn;
            }
        }
        
        return false;
    }

    /// <summary>
    /// Start a cooldown for a skill/ability
    /// </summary>
    /// <param name="player">Player using the skill</param>
    /// <param name="skillName">Name of the skill/ability</param>
    /// <param name="type">Type of cooldown</param>
    /// <param name="duration">Duration in turns (only used for TurnBased cooldowns)</param>
    /// 
    public void StartCooldown(string player, string skillName, CooldownType type, int duration = 0)
    {
        if (!cooldowns.ContainsKey(player))
            cooldowns[player] = new Dictionary<string, CooldownData>();

        Game game = FindObjectOfType<Game>();
        int currentTurn = game != null ? game.turns : 0;

        CooldownData data = new CooldownData
        {
            type = type,
            isUsed = (type == CooldownType.OncePerBattle),
            availableAfterTurn = currentTurn + duration,
            usesRemaining = (type == CooldownType.UsesPerBattle) ? duration : 0
        };

        cooldowns[player][skillName] = data;

        if (type == CooldownType.OncePerBattle)
        {
            Debug.Log($"[CooldownManager] {skillName} for {player} is now ONCE PER BATTLE - used this battle");
        }
        else if (type == CooldownType.UsesPerBattle)
        {
            Debug.Log($"[CooldownManager] {skillName} for {player} can be used {duration} times per battle");
        }
        else
        {
            Debug.Log($"[CooldownManager] {skillName} for {player} is on cooldown until turn {data.availableAfterTurn}");
        }
    }

    /// <summary>
    /// Reset all cooldowns for a player (useful when starting a new battle)
    /// </summary>
    /// <param name="player">Player to reset cooldowns for</param>
    public void ResetPlayerCooldowns(string player)
    {
        if (cooldowns.ContainsKey(player))
        {
            cooldowns[player].Clear();
            Debug.Log($"[CooldownManager] Reset all cooldowns for {player}");
        }
    }

    /// <summary>
    /// Reset all cooldowns for all players
    /// </summary>
    public void ResetAllCooldowns()
    {
        cooldowns.Clear();
        Debug.Log("[CooldownManager] Reset all cooldowns for all players");
    }

    /// <summary>
    /// Consume one use of a UsesPerBattle skill
    /// </summary>
    /// <param name="player">Player using the skill</param>
    /// <param name="skillName">Name of the skill/ability</param>
    /// <returns>True if use was consumed, false if no uses remaining</returns>
    public bool ConsumeUse(string player, string skillName)
    {
        if (!cooldowns.ContainsKey(player) || !cooldowns[player].ContainsKey(skillName))
            return false;

        CooldownData data = cooldowns[player][skillName];
        
        if (data.type == CooldownType.UsesPerBattle && data.usesRemaining > 0)
        {
            data.usesRemaining--;
            Debug.Log($"[CooldownManager] {skillName} for {player} used. {data.usesRemaining} uses remaining.");
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// Get cooldown information for debugging
    /// </summary>
    /// <param name="player">Player to check</param>
    /// <param name="skillName">Skill to check</param>
    /// <returns>Cooldown data or null if not found</returns>
    /// 
    public CooldownData GetCooldownInfo(string player, string skillName)
    {
        if (cooldowns.ContainsKey(player) && cooldowns[player].ContainsKey(skillName))
            return cooldowns[player][skillName];
        return null;
    }

    /// <summary>
    /// Data structure to store cooldown information
    /// </summary>
    [System.Serializable]
    public class CooldownData
    {
        public CooldownType type;
        public bool isUsed;              // For OncePerBattle cooldowns
        public int availableAfterTurn;   // For TurnBased cooldowns
        public int usesRemaining;       // For UsesPerBattle cooldowns
    }
}
