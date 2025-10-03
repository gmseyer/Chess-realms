using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// BotSkillHandler manages all bot-specific skills without interfering with original player mechanics
/// Uses static utility pattern for clean separation and easy access
/// </summary>
public static class BotSkillHandler
{
    private static bool isSummonInProgress = false;
    /// <summary>
    /// Handle Divine Offering skill for bot bishops
    /// This replicates the full functionality of Bishop.OnBishopButtonClick() but for bot use
    /// </summary>
    /// <param name="bishopPiece">The bishop piece (can be captured or alive)</param>
    public static void HandleDivineOffering(GameObject bishopPiece)
    {
        if (bishopPiece == null)
        {
            Debug.LogError("[BotSkillHandler] HandleDivineOffering: bishopPiece is null!");
            return;
        }
        
        Chessman bishopChessman = bishopPiece.GetComponent<Chessman>();
        if (bishopChessman == null)
        {
            Debug.LogError("[BotSkillHandler] HandleDivineOffering: No Chessman component found!");
            return;
        }
        
        string player = bishopChessman.GetPlayer();
        
        // Only handle bot bishops (black pieces)
        if (player != "black")
        {
            Debug.Log($"[BotSkillHandler] Divine Offering not for bot player: {player}");
            return;
        }
        
        Debug.Log($"[BotSkillHandler] 🎯 Handling Divine Offering for bot bishop: {bishopPiece.name}");
        
        // Get game reference
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[BotSkillHandler] Game controller not found!");
            return;
        }
        
        // Check cooldown
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "DivineOffering"))
        {
            Debug.Log("[BotSkillHandler] Divine Offering is on cooldown - cannot use this battle.");
            return;
        }
        
        // Get Bishop component to access prefabs
        Bishop bishopComponent = bishopPiece.GetComponent<Bishop>();
        if (bishopComponent == null)
        {
            Debug.LogError("[BotSkillHandler] No Bishop component found on piece!");
            return;
        }
        
        // Get prefabs directly from Bishop component
        GameObject elementalSummonPlatePrefab = bishopComponent.elementalSummonPlatePrefab;
        GameObject archbishopSummonPlatePrefab = bishopComponent.archbishopSummonPlatePrefab;
        
        if (elementalSummonPlatePrefab == null || archbishopSummonPlatePrefab == null)
        {
            Debug.LogError("[BotSkillHandler] Could not get summon plate prefabs!");
            return;
        }
        
        // Clean up existing plates
        CleanupExistingPlates();
        
        // Generate available positions for bot (black player) WITHOUT creating plates yet
        List<Vector2Int> availablePositions = GetAvailableSummonPositions(game);
        
        if (availablePositions.Count == 0)
        {
            Debug.Log("[BotSkillHandler] No available positions for Divine Offering!");
            return;
        }
        
        // Bot randomly selects a summon plate
        Vector2Int selectedPosition = SelectRandomBotSummonPlate(availablePositions);
        
        // Execute the summon
        ExecuteBotSummon(game, selectedPosition, elementalSummonPlatePrefab, archbishopSummonPlatePrefab);
        
        // Handle cooldown
        HandleCooldown(player);
        
        Debug.Log("[BotSkillHandler] ✅ Divine Offering completed successfully! Bot summoned a piece and ended turn.");
    }
    
    /// <summary>
    /// Handle Divine Offering skill for bot bishops with prefabs passed directly
    /// This version is called when the bishop piece is about to be destroyed
    /// </summary>
    /// <param name="player">The player (should be "black")</param>
    /// <param name="elementalSummonPlatePrefab">Elemental summon plate prefab</param>
    /// <param name="archbishopSummonPlatePrefab">Archbishop summon plate prefab</param>
    public static void HandleDivineOfferingWithPrefabs(string player, GameObject elementalSummonPlatePrefab, GameObject archbishopSummonPlatePrefab)
    {
        if (string.IsNullOrEmpty(player))
        {
            Debug.LogError("[BotSkillHandler] HandleDivineOfferingWithPrefabs: player is null or empty!");
            return;
        }
        
        // Only handle bot bishops (black pieces)
        if (player != "black")
        {
            Debug.Log($"[BotSkillHandler] Divine Offering not for bot player: {player}");
            return;
        }
        
        Debug.Log($"[BotSkillHandler] 🎯 Handling Divine Offering for bot bishop with prefabs");
        
        // Check if summon is already in progress
        if (isSummonInProgress)
        {
            Debug.Log("[BotSkillHandler] Summon already in progress - skipping");
            return;
        }
        
        // Set summon in progress flag
        isSummonInProgress = true;
        
        // Get game reference
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[BotSkillHandler] Game controller not found!");
            isSummonInProgress = false; // Reset flag on error
            return;
        }
        
        // Check cooldown
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "DivineOffering"))
        {
            Debug.Log("[BotSkillHandler] Divine Offering is on cooldown - cannot use this battle.");
            isSummonInProgress = false; // Reset flag
            return;
        }
        
        // Validate prefabs
        if (elementalSummonPlatePrefab == null || archbishopSummonPlatePrefab == null)
        {
            Debug.LogError("[BotSkillHandler] Could not get summon plate prefabs!");
            isSummonInProgress = false; // Reset flag
            return;
        }
        
        // Generate available positions for bot (black player)
        List<Vector2Int> availablePositions = GetAvailableSummonPositions(game);
        
        Debug.Log($"[BotSkillHandler] Found {availablePositions.Count} available positions for Divine Offering");
        
        if (availablePositions.Count == 0)
        {
            Debug.Log("[BotSkillHandler] No available positions for Divine Offering!");
            isSummonInProgress = false; // Reset flag
            return;
        }
        
        // Bot randomly selects a summon plate
        Vector2Int selectedPosition = SelectRandomBotSummonPlate(availablePositions);
        Debug.Log($"[BotSkillHandler] Bot selected position: ({selectedPosition.x},{selectedPosition.y})");
        
        // Execute the summon
        Debug.Log("[BotSkillHandler] Executing bot summon...");
        ExecuteBotSummon(game, selectedPosition, elementalSummonPlatePrefab, archbishopSummonPlatePrefab);
        
        // Handle cooldown
        HandleCooldown(player);
        
        Debug.Log("[BotSkillHandler] ✅ Divine Offering completed successfully! Bot summoned a piece and ended turn.");
    }
    
    
    /// <summary>
    /// Clean up existing move plates
    /// </summary>
    private static void CleanupExistingPlates()
    {
        GameObject[] existingPlates = GameObject.FindGameObjectsWithTag("MovePlate");
        foreach (GameObject plate in existingPlates)
        {
            if (plate != null)
                Object.Destroy(plate);
        }
    }
    
    /// <summary>
    /// Get available summon positions for bot (black player) WITHOUT creating plates
    /// Returns list of available positions
    /// </summary>
    private static List<Vector2Int> GetAvailableSummonPositions(Game game)
    {
        List<Vector2Int> availablePositions = new List<Vector2Int>();
        
        // Black player: top ranks (y = 4-7)
        
        // Elemental bishop: x = 4-7, y = 4-7
        for (int x = 4; x < 8; x++)
        {
            for (int y = 4; y < 8; y++)
            {
                if (game.GetPosition(x, y) == null)
                {
                    availablePositions.Add(new Vector2Int(x, y));
                    Debug.Log($"[BotSkillHandler] Found available ELEMENTAL bishop position at ({x},{y})");
                }
            }
        }
        
        // Archbishop: x = 0-3, y = 4-7
        for (int x = 0; x < 4; x++)
        {
            for (int y = 4; y < 8; y++)
            {
                if (game.GetPosition(x, y) == null)
                {
                    availablePositions.Add(new Vector2Int(x, y));
                    Debug.Log($"[BotSkillHandler] Found available ARCHBISHOP position at ({x},{y})");
                }
            }
        }
        
        return availablePositions;
    }
    
    /// <summary>
    /// Bot randomly selects a summon plate from available positions
    /// </summary>
    private static Vector2Int SelectRandomBotSummonPlate(List<Vector2Int> availablePositions)
    {
        if (availablePositions.Count == 0)
        {
            Debug.LogError("[BotSkillHandler] No available positions to select from!");
            return Vector2Int.zero;
        }
        
        int randomIndex = Random.Range(0, availablePositions.Count);
        Vector2Int selectedPosition = availablePositions[randomIndex];
        
        Debug.Log($"[BotSkillHandler] Bot selected position: ({selectedPosition.x},{selectedPosition.y})");
        
        return selectedPosition;
    }
    
    /// <summary>
    /// Execute the bot's summon choice - directly create the piece without UI plates
    /// </summary>
    private static void ExecuteBotSummon(Game game, Vector2Int selectedPosition, GameObject elementalPrefab, GameObject archbishopPrefab)
    {
        // Determine which type was selected based on position
        string pieceName;
        
        if (selectedPosition.x >= 4) // Elemental bishop area
        {
            pieceName = "black_elemental_bishop";
            Debug.Log($"[BotSkillHandler] Bot chose ELEMENTAL bishop at ({selectedPosition.x},{selectedPosition.y})");
        }
        else // Archbishop area
        {
            pieceName = "black_arch_bishop";
            Debug.Log($"[BotSkillHandler] Bot chose ARCHBISHOP at ({selectedPosition.x},{selectedPosition.y})");
        }
        
        // Directly create the chosen piece (no summon plate needed)
        Debug.Log($"[BotSkillHandler] Creating {pieceName} at ({selectedPosition.x},{selectedPosition.y})");
        game.Create(pieceName, selectedPosition.x, selectedPosition.y);
        
        // Log skill usage
        if (SkillTracker.Instance != null)
        {
            SkillTracker.Instance.LogSkillUsage(game.GetCurrentPlayer(), pieceName, "DIVINE OFFERING", 0);
        }
        
        // Reset summon flag
        isSummonInProgress = false;
        Debug.Log("[BotSkillHandler] Summon completed - flag reset");
        
        // End turn
        Debug.Log("[BotSkillHandler] Ending turn...");
        game.NextTurn();
    }
    
    /// <summary>
    /// Handle cooldown for Divine Offering
    /// </summary>
    private static void HandleCooldown(string player)
    {
        if (CooldownManager.Instance != null)
        {
            // If not initialized yet, set it up for 2 uses per battle
            if (!CooldownManager.Instance.IsOnCooldown(player, "DivineOffering"))
            {
                CooldownManager.Instance.StartCooldown(player, "DivineOffering", CooldownManager.CooldownType.UsesPerBattle, 2);
            }
            // Consume one use
            CooldownManager.Instance.ConsumeUse(player, "DivineOffering");
        }
        Debug.Log("[BotSkillHandler] Divine Offering skill activated - one use consumed.");
    }
    
    /// <summary>
    /// Reset the summon in progress flag - call this when summon completes
    /// </summary>
    public static void ResetSummonFlag()
    {
        isSummonInProgress = false;
        Debug.Log("[BotSkillHandler] Summon flag reset");
    }
    
    /// <summary>
    /// Check if a summon is currently in progress
    /// </summary>
    public static bool IsSummonInProgress()
    {
        return isSummonInProgress;
    }
}
