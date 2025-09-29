using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceBishop : MonoBehaviour
{
    private Game game;
    private Chessman chessman;
    
    void Start()
    {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        chessman = GetComponent<Chessman>();
    }
    
    // Cryostasis Surge Passive - triggers after moving
    public void CryostasisSurge()
    {
        string player = chessman.GetPlayer();
        
        // Check cooldown using CooldownManager
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "CryostasisSurge"))
        {
            Debug.Log("[CryostasisSurge] Skill is on cooldown!");
            return;
        }
        
        Debug.Log("[CryostasisSurge] Activating Cryostasis Surge!");
        
        // Change all move plates to blue color
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        foreach (GameObject plate in movePlates)
        {
            SpriteRenderer sr = plate.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.blue;
                Debug.Log("[CryostasisSurge] Changed move plate color to blue");
            }
        }
        
        // Check 8 surrounding tiles for enemies
        List<string> capturedEnemies = new List<string>();
        
        if (chessman != null)
        {
            int x = chessman.GetXBoard();
            int y = chessman.GetYBoard();
            
            // Check all 8 directions around the IceBishop
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue; // Skip center tile (IceBishop itself)
                    
                    int checkX = x + dx;
                    int checkY = y + dy;
                    
                    // Check if position is within board bounds
                    if (checkX >= 0 && checkX < 8 && checkY >= 0 && checkY < 8)
                    {
                        GameObject pieceAtPos = game.GetPosition(checkX, checkY);
                        if (pieceAtPos != null)
                        {
                            Chessman enemyChessman = pieceAtPos.GetComponent<Chessman>();
                            if (enemyChessman != null && enemyChessman.GetPlayer() != chessman.GetPlayer())
                            {
                                capturedEnemies.Add(pieceAtPos.name);
                                Debug.Log($"[CryostasisSurge] Found enemy: {pieceAtPos.name} at ({checkX},{checkY})");
                            }
                        }
                    }
                }
            }
        }
        
        // Debug log all captured enemies and apply frozen status
        if (capturedEnemies.Count > 0)
        {
            string enemyList = string.Join(", ", capturedEnemies);
            Debug.Log($"Captured enemies: {enemyList}");
            
            // Apply frozen status to all adjacent enemies (4 turns duration)
            foreach (string enemyName in capturedEnemies)
            {
                // Find the enemy piece GameObject
                for (int checkX = 0; checkX < 8; checkX++)
                {
                    for (int checkY = 0; checkY < 8; checkY++)
                    {
                        GameObject pieceAtPos = game.GetPosition(checkX, checkY);
                        if (pieceAtPos != null && pieceAtPos.name == enemyName)
                        {
                            Chessman enemyChessman = pieceAtPos.GetComponent<Chessman>();
                            if (enemyChessman != null)
                            {
                                enemyChessman.statusManager.AddStatus(StatusType.Frozen, game.turns + 4);
                                Debug.Log($"[CryostasisSurge] Applied Frozen status to {enemyName} for 4 turns");
                            }
                            break;
                        }
                    }
                }
            }
            
            // Only start cooldown if enemies were actually frozen
            if (CooldownManager.Instance != null)
            {
                CooldownManager.Instance.StartCooldown(player, "CryostasisSurge", CooldownManager.CooldownType.TurnBased, 20);
                Debug.Log("[CryostasisSurge] Enemies frozen - cooldown started (20 turns)");
            }
        }
        else
        {
            Debug.Log("Captured enemies: None found - no cooldown started");
        }
        
        // End turn normally (always end turn regardless of whether enemies were frozen)
        
    }
    
    /// <summary>
    /// Try to trigger "Glacial Mirror" passive when IceBishop is about to be captured
    /// </summary>
    /// <returns>True if Glacial Mirror was triggered, false otherwise</returns>
    public bool TryTriggerGlacialMirror()
    {
        string player = chessman.GetPlayer();
        
        // Check if Glacial Mirror is on cooldown
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "GlacialMirror"))
        {
            Debug.Log("[GlacialMirror] Skill is on cooldown - cannot use this battle.");
            return false;
        }
        
        Debug.Log($"[GlacialMirror] {gameObject.name} activates Glacial Mirror! Attack negated, entering frozen state for 4 turns.");
        
        // Apply frozen status to self (4 turns duration)
        chessman.statusManager.AddStatus(StatusType.Frozen, game.turns + 4);
        
        // Update visual status immediately for real-time effect
        chessman.UpdateVisualStatus();
        
        // Start cooldown (30 turns)
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.StartCooldown(player, "GlacialMirror", CooldownManager.CooldownType.TurnBased, 30);
            Debug.Log("[GlacialMirror] Cooldown started - 30 turns until next use");
        }
        
        return true;
    }
}
