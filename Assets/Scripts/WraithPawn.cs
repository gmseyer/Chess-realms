using UnityEngine;

public class WraithPawn : MonoBehaviour
{
    private Chessman chessman;
    private Game game;
    private int turnsUntilVanishing = 5;
    private bool hasExploded = false;
    
    // SoulboundCatalyst passive variables
    private static int soulboundCatalystCooldown = 0; // Shared cooldown for all wraith pawns per player

    private void Awake()
    {
        // Cache Chessman reference
        chessman = GetComponent<Chessman>();
        if (chessman == null)
        {
            Debug.LogWarning("[WraithPawn] Chessman component not found in Awake - will retry in Start");
        }
            
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
    }

    private void Start()
    {
        // Use a coroutine to ensure proper initialization order 
        StartCoroutine(InitializeWraithPawn());
    }

    private System.Collections.IEnumerator InitializeWraithPawn()
    {
        // Wait one frame to ensure all components are properly initialized
        yield return null;
        
        // Ensure Chessman reference is set
        if (chessman == null)
        {
            chessman = GetComponent<Chessman>();
            if (chessman == null)
            {
                Debug.LogError($"[WraithPawn] {gameObject.name} - Chessman component still missing after delay!");
                yield break;
            }
            else
            {
                Debug.Log($"[WraithPawn] {gameObject.name} - Chessman component found after delay");
            }
        }
        
        // Start the 5-turn timer from creation
        Debug.Log($"[WraithPawn] {gameObject.name} initialized - will vanish in {turnsUntilVanishing} turns");
        
        // Get current turn when created
        if (game != null)
        {
            int currentTurn = game.turns;
            Debug.Log($"[WraithPawn] {gameObject.name} created on turn {currentTurn}, will vanish on turn {currentTurn + 5}");
        }
    }

    // Call this method when turns change (we'll integrate this with the Game's turn system)
    public void OnTurnChanged()
    {
        // Check if this GameObject is still valid
        if (gameObject == null)
        {
            return; // GameObject has been destroyed
        }

        // Check if Chessman reference is available (initialization complete)
        if (chessman == null)
        {
            Debug.LogWarning($"[WraithPawn] {gameObject.name} - OnTurnChanged called before initialization complete, skipping");
            return;
        }

        if (game == null)
        {
            game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
            if (game == null)
            {
                Debug.LogError("[WraithPawn] Cannot find Game component!");
                return;
            }
        }

        // Decrease timer when it's the white player's turn (since wraith pawns are white)
        if (game.GetCurrentPlayer() == "white")
        {
            turnsUntilVanishing--;
            Debug.Log($"[WraithPawn] {gameObject.name} - {turnsUntilVanishing} turns until vanishing (current turn: {game.turns})");
            
            if (turnsUntilVanishing <= 0)
            {
                Vanish();
            }
        }
    }

    // Called when wraith pawn is captured
    public void OnCaptured()
    {
        if (hasExploded) return; // Prevent multiple explosions
        
        Debug.Log($"[WraithPawn] {gameObject.name} captured - triggering explosion!");
        Explode();
    }

    // Explode and destroy pawn pieces in 3x3 area
    private void Explode()
    {
        hasExploded = true;
        
        if (chessman == null)
        {
            Debug.LogError("[WraithPawn] Missing Chessman reference for explosion!");
            return;
        }

        // Get game reference if missing
        if (game == null)
        {
            game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
            if (game == null)
            {
                Debug.LogError("[WraithPawn] Cannot find Game component for explosion!");
                return;
            }
        }

        int centerX = chessman.GetXBoard();
        int centerY = chessman.GetYBoard();
        
        Debug.Log($"[WraithPawn] Explosion centered at ({centerX},{centerY})");
        
        // Check 3x3 area around the wraith pawn
        for (int x = centerX - 1; x <= centerX + 1; x++)
        {
            for (int y = centerY - 1; y <= centerY + 1; y++)
            {
                if (game.PositionOnBoard(x, y))
                {
                    GameObject piece = game.GetPosition(x, y);
                    if (piece != null && piece != gameObject) // Don't destroy self yet
                    {
                        // Check if piece has "pawn" in its name
                        if (piece.name.ToLower().Contains("pawn"))
                        {
                            Debug.Log($"[WraithPawn] Explosion destroying pawn: {piece.name} at ({x},{y})");
                            
                            // Clear position and destroy piece
                            game.SetPositionEmpty(x, y);
                            Destroy(piece);
                        }
                    }
                }
            }
        }
        
        // Don't destroy the wraith pawn itself here - let the normal capture flow handle it
        // The MovePlate will handle moving the attacking piece and cleaning up
        Debug.Log("[WraithPawn] Explosion completed - letting normal capture flow handle cleanup!");
    }

    // Vanish after 5 turns
    private void Vanish()
    {
        Debug.Log($"[WraithPawn] {gameObject.name} attempting to vanish...");
        
        // Get game reference if missing
        if (game == null)
        {
            game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
            if (game == null)
            {
                Debug.LogError("[WraithPawn] Cannot find Game component for vanishing!");
                return;
            }
        }

        // Try to get chessman reference if missing
        if (chessman == null)
        {
            chessman = GetComponent<Chessman>();
            if (chessman == null)
            {
                Debug.LogError("[WraithPawn] Cannot find Chessman component for vanishing!");
                // Try to find the piece on the board by name and destroy it
                DestroyWraithPawnBySearch();
                return;
            }
        }

        int x = chessman.GetXBoard();
        int y = chessman.GetYBoard();
        
        Debug.Log($"[WraithPawn] {gameObject.name} vanishing after 5 turns at ({x},{y})");
        
        // Clear position and destroy piece
        game.SetPositionEmpty(x, y);
        Destroy(gameObject);
    }

    // Fallback method to destroy wraith pawn by searching the board
    private void DestroyWraithPawnBySearch()
    {
        Debug.Log($"[WraithPawn] Searching board for {gameObject.name} to destroy...");
        
        // Search all positions on the board
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = game.GetPosition(x, y);
                if (piece != null && piece == gameObject)
                {
                    Debug.Log($"[WraithPawn] Found {gameObject.name} at ({x},{y}) - clearing position and destroying");
                    game.SetPositionEmpty(x, y);
                    Destroy(gameObject);
                    return;
                }
            }
        }
        
        // If not found on board, just destroy the GameObject
        Debug.Log($"[WraithPawn] {gameObject.name} not found on board - destroying directly");
        Destroy(gameObject);
    }

    // Override OnDestroy to clean up
    private void OnDestroy()
    {
        // Stop any running coroutines
        StopAllCoroutines();
    }

    // Public method to get remaining turns (for UI display if needed)
    public int GetTurnsUntilVanishing()
    {
        return turnsUntilVanishing;
    }

    // SoulboundCatalyst passive - gains SP when capturing enemy pieces
    public void SoulboundCatalyst(GameObject capturedPiece)
    {
        if (capturedPiece == null)
        {
            Debug.LogWarning("[SoulboundCatalyst] Captured piece is null!");
            return;
        }

        // Check if cooldown is active
        if (game != null && game.turns <= soulboundCatalystCooldown)
        {
            Debug.Log($"[SoulboundCatalyst] On cooldown until turn {soulboundCatalystCooldown} (current: {game.turns})");
            return;
        }

        string pieceName = capturedPiece.name.ToLower();
        string player = chessman != null ? chessman.GetPlayer() : "white"; // Default to white for wraith pawns
        int spGained = 0;

        // Determine SP gain based on captured piece type
        if (pieceName.Contains("rook") || pieceName.Contains("bishop") || pieceName.Contains("knight"))
        {
            spGained = 1;
            Debug.Log($"[SoulboundCatalyst] {player} captured {capturedPiece.name} - gained 1 SP");
        }
        else if (pieceName.Contains("queen") || pieceName.Contains("king") || 
                 pieceName.Contains("archbishop") || pieceName.Contains("royal") ||
                 pieceName.Contains("elemental") || pieceName.Contains("celestial"))
        {
            spGained = 2;
            Debug.Log($"[SoulboundCatalyst] {player} captured {capturedPiece.name} - gained 2 SP");
        }
        else if (pieceName.Contains("pawn"))
        {
            Debug.Log($"[SoulboundCatalyst] {player} captured {capturedPiece.name} - no SP gained (pawn)");
            return; // No SP for pawns, but still start cooldown
        }
        else
        {
            // Default case for any other piece types
            spGained = 2;
            Debug.Log($"[SoulboundCatalyst] {player} captured {capturedPiece.name} - gained 2 SP (default)");
        }

        // Add SP to player
        if (spGained > 0 && SkillManager.Instance != null)
        {
            SkillManager.Instance.AddPlayerSP(player, spGained);
            
            // Start 20-turn cooldown
            if (game != null)
            {
                soulboundCatalystCooldown = game.turns + 20;
                Debug.Log($"[SoulboundCatalyst] Cooldown started - available again on turn {soulboundCatalystCooldown}");
            }
            
            // Log skill usage
            if (SkillTracker.Instance != null)
            {
                SkillTracker.Instance.LogSkillUsage(player, "WRAITH PAWN", "SOULBOUND CATALYST", spGained);
            }
        }
        else if (spGained == 0)
        {
            // Still start cooldown even if no SP gained
            if (game != null)
            {
                soulboundCatalystCooldown = game.turns + 20;
                Debug.Log($"[SoulboundCatalyst] Cooldown started (no SP gained) - available again on turn {soulboundCatalystCooldown}");
            }
        }
    }

    // Public method to check if SoulboundCatalyst is on cooldown
    public static bool IsSoulboundCatalystOnCooldown(Game game)
    {
        if (game == null) return false;
        return game.turns <= soulboundCatalystCooldown;
    }

    // Public method to get remaining cooldown turns
    public static int GetSoulboundCatalystCooldownRemaining(Game game)
    {
        if (game == null) return 0;
        int remaining = soulboundCatalystCooldown - game.turns;
        return Mathf.Max(0, remaining);
    }
}
