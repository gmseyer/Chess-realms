using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBishop : MonoBehaviour
{
    private Game game;
    private Chessman chessman;
    
    // Worldfire Ring tracking
    private static int worldfireRingEndTurn = -1;
    private static int worldfireRingStartTurn = -1;
    private static Chessman worldfireRingCaster = null;
    
    void Start()
    {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        chessman = GetComponent<Chessman>();
    }
    
    /// <summary>
    /// Worldfire Ring - Fire Bishop becomes stunned and creates expanding fire aura
    /// </summary>
    public void WorldfireRing()
    {
        // Find the actual FireBishop piece on the board (not the script GameObject)
        Chessman fireBishopChessman = FindFireBishopPiece();
        if (fireBishopChessman == null)
        {
            Debug.LogError("[WorldfireRing] Could not find FireBishop piece on board!");
            return;
        }
        
        string player = fireBishopChessman.GetPlayer();
        
        // Check SP cost
        if (SkillManager.Instance.GetPlayerSP(player) < 2)
        {
            Debug.Log("[WorldfireRing] Not enough SP!");
            return;
        }
        
        // Check if Worldfire Ring is on cooldown
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "WorldfireRing"))
        {
            Debug.Log("[WorldfireRing] Skill is on cooldown!");
            return;
        }
        
        // Deduct SP
        SkillManager.Instance.SpendPlayerSP(player, 2);
        
        // Apply stunned status to Fire Bishop (5 turns)
        fireBishopChessman.statusManager.AddStatus(StatusType.Stunned, game.turns + 5);
        fireBishopChessman.UpdateVisualStatus();
        
        // Set Worldfire Ring tracking
        worldfireRingStartTurn = game.turns;
        worldfireRingEndTurn = game.turns + 5;
        worldfireRingCaster = fireBishopChessman;
        
        // Start cooldown (30 turns)
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.StartCooldown(player, "WorldfireRing", CooldownManager.CooldownType.TurnBased, 30);
        }
        
        Debug.Log($"[WorldfireRing] Activated! Fire Bishop is stunned for 5 turns. Fire aura will expand each turn!");
        
        // Trigger initial 3x3 aura
        TriggerFireAura(3);
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
        // End turn
        game.NextTurn();
    }
    
    /// <summary>
    /// Trigger fire aura with specified size (3x3, 5x5, or 7x7)
    /// </summary>
    private void TriggerFireAura(int auraSize)
    {
        if (worldfireRingCaster == null) return;
        
        int centerX = worldfireRingCaster.GetXBoard();
        int centerY = worldfireRingCaster.GetYBoard();
        int radius = auraSize / 2; // 1, 2, or 3
        
        int crippledCount = 0;
        
        // Check all tiles within the aura
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (dx == 0 && dy == 0) continue; // Skip center (Fire Bishop)
                
                int checkX = centerX + dx;
                int checkY = centerY + dy;
                
                // Check if position is within board bounds
                if (checkX >= 0 && checkX < 8 && checkY >= 0 && checkY < 8)
                {
                    GameObject pieceAtPos = game.GetPosition(checkX, checkY);
                    if (pieceAtPos != null)
                    {
                        Chessman enemyChessman = pieceAtPos.GetComponent<Chessman>();
                        if (enemyChessman != null && enemyChessman.GetPlayer() != worldfireRingCaster.GetPlayer())
                        {
                            // Apply crippled status (2 turns duration)
                            enemyChessman.statusManager.AddStatus(StatusType.Crippled, game.turns + 2);
                            enemyChessman.UpdateVisualStatus();
                            crippledCount++;
                        }
                    }
                }
            }
        }
        
        Debug.Log($"[WorldfireRing] {auraSize}x{auraSize} fire aura triggered! {crippledCount} enemies crippled.");
    }
    
    /// <summary>
    /// Check if Worldfire Ring is active and trigger appropriate aura
    /// </summary>
    public static void CheckWorldfireRingExpansion(int currentTurn)
    {
        if (worldfireRingEndTurn == -1 || currentTurn > worldfireRingEndTurn || worldfireRingCaster == null)
        {
            // Reset tracking if expired
            if (currentTurn > worldfireRingEndTurn)
            {
                worldfireRingEndTurn = -1;
                worldfireRingStartTurn = -1;
                worldfireRingCaster = null;
                Debug.Log("[WorldfireRing] Period ended.");
            }
            return;
        }
        
        // Calculate which turn of the 5-turn period we're in
        int turnInPeriod = currentTurn - worldfireRingStartTurn;
        
        // Trigger appropriate aura based on turn
        FireBishop fireBishopScript = worldfireRingCaster.GetComponent<FireBishop>();
        if (fireBishopScript != null)
        {
            switch (turnInPeriod)
            {
                case 1: // Turn 2 (after first turn)
                    fireBishopScript.TriggerFireAura(5); // 5x5 aura
                    break;
                case 3: // Turn 4 (after third turn)
                    fireBishopScript.TriggerFireAura(7); // 7x7 aura
                    break;
            }
        }
    }
    
    /// <summary>
    /// Find the actual FireBishop piece on the board
    /// </summary>
    private Chessman FindFireBishopPiece()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = game.GetPosition(x, y);
                if (piece != null && piece.name.ToLower().Contains("fire_bishop"))
                {
                    Chessman chessman = piece.GetComponent<Chessman>();
                    if (chessman != null)
                    {
                        return chessman;
                    }
                }
            }
        }
        return null;
    }
}
