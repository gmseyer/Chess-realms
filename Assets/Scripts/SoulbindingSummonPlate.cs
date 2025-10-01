using UnityEngine;

public class SoulbindingSummonPlate : MonoBehaviour
{
    private Game game;
    private int x, y;
    private string pieceToSummon;
    private string player; // Store the player who will summon

    public void Setup(Game g, int tileX, int tileY, string pieceName, string playerName)
    {
        game = g;
        x = tileX;
        y = tileY;
        pieceToSummon = pieceName;
        player = playerName;
    }

    private void OnMouseUp() 
    {
        Debug.Log($"[SoulbindingSummonPlate] Clicked at ({x},{y}) to summon {pieceToSummon} for {player} player");

        // Check if tile is still empty
        if (game.GetPosition(x, y) != null)
        {
            Debug.Log("[SoulbindingSummonPlate] Tile is occupied — summon cancelled.");
            return;
        }

        // Convert captured piece to current player's version
        string playerPieceName = ConvertToPlayerPiece(pieceToSummon, player);
        
        // Check if conversion was successful
        if (playerPieceName == null)
        {
            Debug.LogError($"[SoulbindingSummonPlate] Cannot summon {pieceToSummon} - invalid piece type!");
            
            // Clean up all summon plates
            foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
                Destroy(plate);
            
            // End the Archbishop's turn
            game.NextTurn();
            return;
        }
        
        // Create the current player's version of the captured piece on the chosen tile
        GameObject summonedPiece = game.Create(playerPieceName, x, y);
        
        if (summonedPiece != null)
        {
            Debug.Log($"[SoulbindingSummonPlate] Successfully summoned {playerPieceName} (converted from {pieceToSummon}) at ({x},{y}) for {player} player");
            
            // Log skill usage if SkillTracker is available
            if (SkillTracker.Instance != null)
            {
                SkillTracker.Instance.LogSkillUsage(player, "ARCHBISHOP", "SOULBINDING CONQUEST", 0);
            }
        }
        else
        {
            Debug.LogError($"[SoulbindingSummonPlate] Failed to create {playerPieceName} at ({x},{y})");
        }

        // Clean up all summon plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // End the Archbishop's turn
        game.NextTurn();
    }

    // ✅ Convert captured piece to current player's version (player-aware)
    private string ConvertToPlayerPiece(string capturedPiece, string player)
    {
        // Convert any captured piece to the current player's equivalent
        if (capturedPiece.Contains("pawn"))
        {
            // For pawns, create a generic pawn for the current player
            return $"{player}_pawn";
        }
        else if (capturedPiece.Contains("knight"))
        {
            return $"{player}_knight";
        }
        else if (capturedPiece.Contains("rook"))
        {
            return $"{player}_rook";
        }
        else if (capturedPiece.Contains("bishop"))
        {
            return $"{player}_bishop";
        }
        
        // If we reach here, the piece is not valid for summoning (queen, king, etc.)
        Debug.LogError($"[SoulbindingSummonPlate] Invalid piece for summoning: {capturedPiece}. This should not happen!");
        return null; // Return null to indicate error
    }
}
