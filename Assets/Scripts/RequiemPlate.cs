using UnityEngine;

public class RequiemPlate : MonoBehaviour
{
    private GameObject royalBishop;
    private int x, y;
    private static int wraithPawnsCreated = 0;
    private static int maxWraithPawns = 2;
    private static string currentSkill = "SoulRequiem"; // Track which skill is active

    public void Setup(GameObject royalBishopRef, int tileX, int tileY)
    {
        royalBishop = royalBishopRef;
        x = tileX;
        y = tileY;
    }

    // Method to set which skill is currently active
    public static void SetCurrentSkill(string skillName)
    {
        currentSkill = skillName;
        Debug.Log($"[RequiemPlate] Current skill set to: {currentSkill}");
    }

    private void OnMouseUp()
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        
        // Handle different skills based on current skill
        if (currentSkill == "SoulRequiem")
        {
            HandleSoulRequiem(game);
        }
        else if (currentSkill == "SanctifiedRuin")
        {
            HandleSanctifiedRuin(game);
        }
        else
        {
            Debug.LogError($"[RequiemPlate] Unknown skill: {currentSkill}");
        }
    }

    private void HandleSoulRequiem(Game game)
    {
        // Check if we can still create wraith pawns
        if (wraithPawnsCreated >= maxWraithPawns)
        {
            Debug.LogWarning("[RequiemPlate] Maximum wraith pawns (2) already created!");
            return;
        }

        // Check if position is still empty
        if (game.GetPosition(x, y) != null)
        {
            Debug.LogWarning($"[RequiemPlate] Position ({x},{y}) is no longer empty!");
            return;
        }

        Debug.Log($"[RequiemPlate] Creating wraith pawn at ({x},{y}) - {wraithPawnsCreated + 1}/2");

        // Create the wraith pawn
        GameObject wraithPawn = game.Create("white_wraith_pawn", x, y);
        if (wraithPawn != null)
        {
            wraithPawnsCreated++;
            Debug.Log($"[RequiemPlate] Wraith pawn {wraithPawnsCreated}/2 created successfully at ({x},{y})");
            
            // If this is the second wraith pawn, end the turn
            if (wraithPawnsCreated >= maxWraithPawns)
            {
                EndSoulRequiemTurn();
            }
        }
        else
        {
            Debug.LogError($"[RequiemPlate] Failed to create wraith pawn at ({x},{y})");
        }
    }

    private void HandleSanctifiedRuin(Game game)
    {
        Debug.Log($"[RequiemPlate] SanctifiedRuin - Creating 3x3 Sacred Zone centered at ({x},{y})");
        
        // Create the 3x3 Sacred Zone centered on the selected tile
        CreateSacredZone(game, x, y);
        
        // Check for Wraith Pawns in the 3x3 zone
        if (CheckForWraithPawnsInZone(game, x, y))
        {
            // Combo triggered - explode Wraith Pawns and gain SP
            HandleSanctifiedRuinCombo(game, x, y);
        }
        else
        {
            // Normal behavior - just end turn
            EndSanctifiedRuinTurn();
        }
    }

    private void CreateSacredZone(Game game, int centerX, int centerY)
    {
        // Create 3x3 zone centered on the selected tile
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int tileX = centerX + dx;
                int tileY = centerY + dy;
                
                // Check if position is on board
                if (game.PositionOnBoard(tileX, tileY))
                {
                    // Create sanctuary marker (non-blocking)
                    CreateSanctuaryMarker(game, tileX, tileY);
                }
            }
        }
    }

    private void CreateSanctuaryMarker(Game game, int tileX, int tileY)
    {
        // Use the same positioning as move plates and celestial orbs
        float fx = tileX * 0.57f - 1.98f;
        float fy = tileY * 0.56f - 1.95f;
        
        // Create a new GameObject for the sanctuary marker
        GameObject sanctuaryMarker = new GameObject($"SanctuaryMarker_{tileX}_{tileY}");
        sanctuaryMarker.transform.position = new Vector3(fx, fy, -3f); // Same z-depth as move plates
        
        // Add SpriteRenderer with sanctuary sprite
        SpriteRenderer sr = sanctuaryMarker.AddComponent<SpriteRenderer>();
        sr.sprite = GetSanctuarySprite();
        sr.sortingOrder = 1; // Slightly above move plates
        
        // Add SanctuaryMarker script
        SanctuaryMarker marker = sanctuaryMarker.AddComponent<SanctuaryMarker>();
        marker.Setup(game, tileX, tileY, game.turns + 4); // 4 turn duration
        
        Debug.Log($"[RequiemPlate] Created sanctuary marker at ({tileX},{tileY}) - expires on turn {game.turns + 4}");
    }

    private Sprite GetSanctuarySprite()
    {
        // Find a piece with the sanctuary sprite to get the reference
        Chessman[] allPieces = FindObjectsOfType<Chessman>();
        foreach (Chessman piece in allPieces)
        {
            if (piece.tile_sanctuary != null)
            {
                return piece.tile_sanctuary;
            }
        }
        
        Debug.LogError("[RequiemPlate] Could not find sanctuary sprite reference!");
        return null;
    }

    private void EndSoulRequiemTurn()
    {
        Debug.Log("[RequiemPlate] Both wraith pawns created - ending Royal Bishop's turn");
        
        // Clean up all requiem plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
        
        // Reset counter for next use
        wraithPawnsCreated = 0;
        
        // End the Royal Bishop's turn
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        game.NextTurn();
        
        Debug.Log("[RequiemPlate] SoulRequiem skill completed successfully!");
    }

    private bool CheckForWraithPawnsInZone(Game game, int centerX, int centerY)
    {
        // Check 3x3 zone for Wraith Pawns
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int tileX = centerX + dx;
                int tileY = centerY + dy;
                
                if (game.PositionOnBoard(tileX, tileY))
                {
                    GameObject piece = game.GetPosition(tileX, tileY);
                    if (piece != null && piece.name.ToLower().Contains("wraith_pawn"))
                    {
                        Debug.Log($"[RequiemPlate] Found Wraith Pawn at ({tileX},{tileY}) - combo triggered!");
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void HandleSanctifiedRuinCombo(Game game, int centerX, int centerY)
    {
        Debug.Log("[RequiemPlate] SanctifiedRuin Combo triggered - exploding Wraith Pawns!");
        
        // Find and explode all Wraith Pawns in the 3x3 zone
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int tileX = centerX + dx;
                int tileY = centerY + dy;
                
                if (game.PositionOnBoard(tileX, tileY))
                {
                    GameObject piece = game.GetPosition(tileX, tileY);
                    if (piece != null && piece.name.ToLower().Contains("wraith_pawn"))
                    {
                        WraithPawn wraithPawn = piece.GetComponent<WraithPawn>();
                        if (wraithPawn != null)
                        {
                            Debug.Log($"[RequiemPlate] Exploding Wraith Pawn at ({tileX},{tileY})");
                            wraithPawn.OnCaptured(); // Call the public OnCaptured method
                            
                            // Manually destroy the Wraith Pawn after explosion
                            // (since we're not in normal capture flow)
                            DestroyWraithPawnAfterExplosion(game, piece, tileX, tileY);
                        }
                    }
                }
            }
        }
        
        // Gain 2 SP for the combo
        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.AddPlayerSP("white", 2);
            Debug.Log("[RequiemPlate] SanctifiedRuin Combo - gained 2 SP!");
        }
        
        // Destroy all sanctuary markers
        DestroyAllSanctuaryMarkers();
        
        // End the turn
        EndSanctifiedRuinTurn();
    }

    private void DestroyWraithPawnAfterExplosion(Game game, GameObject wraithPawn, int x, int y)
    {
        // Clear the position from the game board
        game.SetPositionEmpty(x, y);
        
        // Destroy the Wraith Pawn GameObject
        Destroy(wraithPawn);
        
        Debug.Log($"[RequiemPlate] Wraith Pawn destroyed after explosion at ({x},{y})");
    }

    private void DestroyAllSanctuaryMarkers()
    {
        SanctuaryMarker[] allMarkers = FindObjectsOfType<SanctuaryMarker>();
        foreach (SanctuaryMarker marker in allMarkers)
        {
            if (marker != null)
            {
                Debug.Log($"[RequiemPlate] Destroying sanctuary marker at ({marker.GetX()},{marker.GetY()})");
                Destroy(marker.gameObject);
            }
        }
    }

    private void EndSanctifiedRuinTurn()
    {
        Debug.Log("[RequiemPlate] Sacred Zone created - ending Royal Bishop's turn");
        
        // Clean up all requiem plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
        
        // End the Royal Bishop's turn
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        game.NextTurn();
        
        Debug.Log("[RequiemPlate] SanctifiedRuin skill completed successfully!");
    }

    // Static method to reset counter (can be called externally if needed)
    public static void ResetWraithPawnCounter()
    {
        wraithPawnsCreated = 0;
        Debug.Log("[RequiemPlate] Wraith pawn counter reset");
    }
}
