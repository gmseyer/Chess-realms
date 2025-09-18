using UnityEngine;

public class CelestialSummonPlate : MonoBehaviour
{
    private Game game;
    private int x, y;
    private string player;
    private static int pawnsSummoned = 0;
    private const int MAX_PAWNS = 3;

    public void Setup(Game gameRef, int xPos, int yPos, string playerName)
    {
        game = gameRef;
        x = xPos;
        y = yPos;
        player = playerName;
    }

    public void OnMouseUp()
    {
        // Check if we can still summon pawns
        if (pawnsSummoned >= MAX_PAWNS)
        {
            Debug.Log($"[Celestial Summon] Maximum of {MAX_PAWNS} pawns already summoned!");
            return;
        }

        // Check if there are already 8 or more white pawns on the board
        if (CountWhitePawns() >= 8)
        {
            Debug.Log("[Celestial Summon] Maximum of 8 white pawns already on board! Stopping summoning.");
            EndCelestialSummon();
            return;
        }

        // Check if position is still empty
        if (game.GetPosition(x, y) != null)
        {
            Debug.Log($"[Celestial Summon] Position ({x},{y}) is no longer empty!");
            return;
        }

        // Summon a pawn at this position
        SummonPawn(x, y, player);
        
        // Increment counter
        pawnsSummoned++;
        
        Debug.Log($"[Celestial Summon] Summoned pawn {pawnsSummoned}/{MAX_PAWNS} at ({x},{y})");

        // If we've reached the maximum, end the turn
        if (pawnsSummoned >= MAX_PAWNS)
        {
            Debug.Log("[Celestial Summon] All 3 pawns summoned! Ending turn.");
            EndCelestialSummon();
        }
    }

    private void SummonPawn(int x, int y, string player)
    {
        // Create pawn name based on player
        string pawnName = $"{player}_pawn_celestial_{pawnsSummoned + 1}";
        
        // Create the pawn using Game.Create method
        GameObject pawn = game.Create(pawnName, x, y);
        
        if (pawn != null)
        {
            Debug.Log($"[Celestial Summon] Successfully created {pawnName} at ({x},{y})");
        }
        else
        {
            Debug.LogError($"[Celestial Summon] Failed to create {pawnName} at ({x},{y})");
        }
    }

    private int CountWhitePawns()
    {
        int whitePawnCount = 0;
        
        // Scan the entire board for white pawns
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = game.GetPosition(x, y);
                if (piece == null) continue;
                
                // Check if this piece is a white pawn
                if (piece.name.ToLower().Contains("white") && piece.name.ToLower().Contains("pawn"))
                {
                    whitePawnCount++;
                }
            }
        }
        
        Debug.Log($"[Celestial Summon] Current white pawn count: {whitePawnCount}");
        return whitePawnCount;
    }

    private void EndCelestialSummon()
    {
        // Destroy all remaining summon tiles
        GameObject[] summonTiles = GameObject.FindGameObjectsWithTag("MovePlate");
        foreach (GameObject tile in summonTiles)
        {
            if (tile.GetComponent<CelestialSummonPlate>() != null)
            {
                Destroy(tile);
            }
        }
        
        // Reset counter for next time
        pawnsSummoned = 0;
        
        // End the turn
        game.NextTurn();
        
        Debug.Log("[Celestial Summon] Celestial Summon completed! Turn ended.");
        // In Rook.cs, in AttemptFortify() method:
if (SkillTracker.Instance != null)
{
    SkillTracker.Instance.LogSkillUsage(game.GetCurrentPlayer(), "BISHOP", "CELESTIAL SUMMON", 2);
}

    }
}
