using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceBishop : MonoBehaviour
{
    private Game game;
    private Chessman chessman;
    
    // AbsoluteZero tracking
    private static int absoluteZeroEndTurn = -1;
    private static List<AbsoluteZeroPiece> piecesAtAbsoluteZeroStart = new List<AbsoluteZeroPiece>();
    
    // Frostbound tracking
    private static int frostboundEndTurn = -1;
    
    // Helper class to track pieces by position and name
    private class AbsoluteZeroPiece
    {
        public string pieceName;
        public int x;
        public int y;
        public bool hasMovedDuringPeriod;
        
        public AbsoluteZeroPiece(string name, int x, int y)
        {
            this.pieceName = name;
            this.x = x;
            this.y = y;
            this.hasMovedDuringPeriod = false;
        }
    }
    
    void Start()
    {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        chessman = GetComponent<Chessman>();
    }
    
    // Cryostasis Surge Passive - triggers after moving
    public void CryostasisSurge()
    {
        // Find the actual IceBishop piece on the board (not the script GameObject)
        Chessman iceBishopChessman = FindIceBishopPiece();
        if (iceBishopChessman == null)
        {
            Debug.LogError("[CryostasisSurge] Could not find IceBishop piece on board!");
            return;
        }
        
        string player = iceBishopChessman.GetPlayer();
        
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
        
        if (iceBishopChessman != null)
        {
            int x = iceBishopChessman.GetXBoard();
            int y = iceBishopChessman.GetYBoard();
            
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
                            if (enemyChessman != null && enemyChessman.GetPlayer() != iceBishopChessman.GetPlayer())
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
        // Find the actual IceBishop piece on the board (not the script GameObject)
        Chessman iceBishopChessman = FindIceBishopPiece();
        if (iceBishopChessman == null)
        {
            Debug.LogError("[GlacialMirror] Could not find IceBishop piece on board!");
            return false;
        }
        
        string player = iceBishopChessman.GetPlayer();
        
        // Check if Glacial Mirror is on cooldown
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "GlacialMirror"))
        {
            Debug.Log("[GlacialMirror] Skill is on cooldown - cannot use this battle.");
            return false;
        }
        
        Debug.Log($"[GlacialMirror] {iceBishopChessman.name} activates Glacial Mirror! Attack negated, entering frozen state for 4 turns.");
        
        // Apply frozen status to self (4 turns duration)
        iceBishopChessman.statusManager.AddStatus(StatusType.Frozen, game.turns + 4);
        
        // Update visual status immediately for real-time effect
        iceBishopChessman.UpdateVisualStatus();
        
        // Start cooldown (30 turns)
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.StartCooldown(player, "GlacialMirror", CooldownManager.CooldownType.TurnBased, 30);
            Debug.Log("[GlacialMirror] Cooldown started - 30 turns until next use");
        }
        
        return true;
    }
    
    /// <summary>
    /// AbsoluteZero - Freezes all pieces that don't move during 14 turns
    /// </summary>
    public void AbsoluteZero()
    {
        // Find the actual IceBishop piece on the board (not the script GameObject)
        Chessman iceBishopChessman = FindIceBishopPiece();
        if (iceBishopChessman == null)
        {
            Debug.LogError("[AbsoluteZero] Could not find IceBishop piece on board!");
            return;
        }
        
        string player = iceBishopChessman.GetPlayer();
        
        // Check SP cost
        if (SkillManager.Instance.GetPlayerSP(player) < 2)
        {
            Debug.Log("[AbsoluteZero] Not enough SP!");
            return;
        }
        
        // Check if already used this battle
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "AbsoluteZero"))
        {
            Debug.Log("[AbsoluteZero] Already used this battle!");
            return;
        }
        
        // Deduct SP
        SkillManager.Instance.SpendPlayerSP(player, 2);
        
        // Record all pieces on board at the start (by position)
        piecesAtAbsoluteZeroStart.Clear();
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = game.GetPosition(x, y);
                if (piece != null)
                {
                    piecesAtAbsoluteZeroStart.Add(new AbsoluteZeroPiece(piece.name, x, y));
                }
            }
        }
        
        // Set end turn (14 turns from now)
        absoluteZeroEndTurn = game.turns + 14;
        
        // Start cooldown (once per battle)
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.StartCooldown(player, "AbsoluteZero", CooldownManager.CooldownType.OncePerBattle);
        }
        
        Debug.Log($"[AbsoluteZero] Cast! All pieces must move at least once in the next 14 turns or be frozen!");
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
        // End turn
        game.NextTurn();
    }
    
    /// <summary>
    /// Check if AbsoluteZero period has ended and apply frozen to non-movers
    /// </summary>
    public static void CheckAbsoluteZeroExpiry(Game game)
    {
        if (absoluteZeroEndTurn == -1 || game.turns < absoluteZeroEndTurn)
            return;
        
        // AbsoluteZero period has ended
        int frozenCount = 0;
        
        foreach (AbsoluteZeroPiece trackedPiece in piecesAtAbsoluteZeroStart)
        {
            // Check if there's still a piece at the original position
            GameObject pieceAtPosition = game.GetPosition(trackedPiece.x, trackedPiece.y);
            
            // If no piece at original position, it moved (or was captured)
            if (pieceAtPosition == null)
            {
                continue; // Piece moved, so it's safe
            }
            
            // If there's a different piece at the position, the original moved
            if (pieceAtPosition.name != trackedPiece.pieceName)
            {
                continue; // Different piece here, original moved
            }
            
            // Same piece at same position - it didn't move during AbsoluteZero period
            Chessman chessman = pieceAtPosition.GetComponent<Chessman>();
            if (chessman != null)
            {
                // IceBishop is immune to AbsoluteZero effects
                if (trackedPiece.pieceName.ToLower().Contains("ice_bishop"))
                {
                    continue; // Skip freezing IceBishop
                }
                
                // Piece didn't move during AbsoluteZero period - freeze it
                chessman.statusManager.AddStatus(StatusType.Frozen, game.turns + 4);
                chessman.UpdateVisualStatus();
                frozenCount++;
            }
        }
        
        Debug.Log($"[AbsoluteZero] Period ended! {frozenCount} pieces frozen for not moving.");
        
        // Reset tracking
        absoluteZeroEndTurn = -1;
        piecesAtAbsoluteZeroStart.Clear();
    }
    
    /// <summary>
    /// Find a piece by name on the board
    /// </summary>
    private static GameObject FindPieceByName(Game game, string pieceName)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = game.GetPosition(x, y);
                if (piece != null && piece.name == pieceName)
                {
                    return piece;
                }
            }
        }
        return null;
    }
    
    /// <summary>
    /// Find the actual IceBishop piece on the board
    /// </summary>
    private Chessman FindIceBishopPiece()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = game.GetPosition(x, y);
                if (piece != null && piece.name.ToLower().Contains("ice_bishop"))
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
    
    /// <summary>
    /// Frostbound - Freezes any piece that uses an active skill for 4 turns
    /// </summary>
    public void Frostbound()
    {
        // Find the actual IceBishop piece on the board (not the script GameObject)
        Chessman iceBishopChessman = FindIceBishopPiece();
        if (iceBishopChessman == null)
        {
            Debug.LogError("[Frostbound] Could not find IceBishop piece on board!");
            return;
        }
        
        string player = iceBishopChessman.GetPlayer();
        
        // Check SP cost
        if (SkillManager.Instance.GetPlayerSP(player) < 2)
        {
            Debug.Log("[Frostbound] Not enough SP!");
            return;
        }
        
        // Check if Frostbound is on cooldown
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "Frostbound"))
        {
            Debug.Log("[Frostbound] Skill is on cooldown!");
            return;
        }
        
        // Deduct SP
        SkillManager.Instance.SpendPlayerSP(player, 2);
        
        // Set Frostbound duration (4 turns)
        frostboundEndTurn = game.turns + 4;
        
        // Start cooldown (15 turns)
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.StartCooldown(player, "Frostbound", CooldownManager.CooldownType.TurnBased, 15);
        }
        
        Debug.Log($"[Frostbound] Activated! Any piece using active skills will be frozen for the next 4 turns!");
        
        // End turn
        game.NextTurn();
    }
    
    /// <summary>
    /// Check if Frostbound is currently active
    /// </summary>
    public static bool IsFrostboundActive(int currentTurn)
    {
        return frostboundEndTurn != -1 && currentTurn <= frostboundEndTurn;
    }
    
    /// <summary>
    /// Check if Frostbound period has ended and reset tracking
    /// </summary>
    public static void CheckFrostboundExpiry(int currentTurn)
    {
        if (frostboundEndTurn != -1 && currentTurn > frostboundEndTurn)
        {
            Debug.Log("[Frostbound] Period ended - no longer active.");
            frostboundEndTurn = -1;
        }
    }
}
