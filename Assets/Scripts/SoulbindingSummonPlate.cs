using UnityEngine;

public class SoulbindingSummonPlate : MonoBehaviour
{
    private Game game;
    private int x, y;
    private string pieceToSummon;

    public void Setup(Game g, int tileX, int tileY, string pieceName)
    {
        game = g;
        x = tileX;
        y = tileY;
        pieceToSummon = pieceName;
    }

    private void OnMouseUp() 
    {
        Debug.Log($"[SoulbindingSummonPlate] Clicked at ({x},{y}) to summon {pieceToSummon}");

        // Check if tile is still empty
        if (game.GetPosition(x, y) != null)
        {
            Debug.Log("[SoulbindingSummonPlate] Tile is occupied — summon cancelled.");
            return;
        }

        // Convert captured piece to white version
        string whitePieceName = ConvertToWhitePiece(pieceToSummon);
        
        // Check if conversion was successful
        if (whitePieceName == null)
        {
            Debug.LogError($"[SoulbindingSummonPlate] Cannot summon {pieceToSummon} - invalid piece type!");
            
            // Clean up all summon plates
            foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
                Destroy(plate);
            
            // End the Archbishop's turn
            game.NextTurn();
            return;
        }
        
        // Create the white version of the captured piece on the chosen tile
        GameObject summonedPiece = game.Create(whitePieceName, x, y);
        
        if (summonedPiece != null)
        {
            Debug.Log($"[SoulbindingSummonPlate] Successfully summoned {whitePieceName} (converted from {pieceToSummon}) at ({x},{y})");
            
            // Log skill usage if SkillTracker is available
            if (SkillTracker.Instance != null)
            {
                string currentPlayer = game.GetCurrentPlayer();
                SkillTracker.Instance.LogSkillUsage(currentPlayer, "ARCHBISHOP", "SOULBINDING CONQUEST", 0);
            }
        }
        else
        {
            Debug.LogError($"[SoulbindingSummonPlate] Failed to create {pieceToSummon} at ({x},{y})");
        }

        // Clean up all summon plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // End the Archbishop's turn
        game.NextTurn();
    }

    private string ConvertToWhitePiece(string capturedPiece)
    {
        // Convert any captured piece to its white equivalent
        if (capturedPiece.Contains("pawn"))
        {
            // For pawns, we need to determine which white pawn to create
            // Since we don't have specific pawn numbers, we'll create a generic white_pawn
            return "white_pawn";
        }
        else if (capturedPiece.Contains("knight"))
        {
            return "white_knight";
        }
        else if (capturedPiece.Contains("rook"))
        {
            return "white_rook";
        }
        else if (capturedPiece.Contains("bishop"))
        {
            return "white_bishop";
        }
        
        // If we reach here, the piece is not valid for summoning (queen, king, etc.)
        Debug.LogError($"[SoulbindingSummonPlate] Invalid piece for summoning: {capturedPiece}. This should not happen!");
        return null; // Return null to indicate error
    }
}
