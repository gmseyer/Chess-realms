using UnityEngine;

public class SingularitySummonPlate : MonoBehaviour
{
    private Game game;
    private int x, y;
    private string targetPieceName;
    private string targetPlayer;
    private int chronomagusX;
    private int chronomagusY;
    private static bool chronomagusPlaced = false;

    public void Setup(Game g, int tileX, int tileY, string pieceName, string player, int chronoX, int chronoY)
    {
        game = g;
        x = tileX;
        y = tileY;
        targetPieceName = pieceName;
        targetPlayer = player;
        chronomagusX = chronoX;
        chronomagusY = chronoY;
    }

    private void OnMouseUp()
    {
        if (!chronomagusPlaced)
        {
            // Place Chronomagus first
            PlaceChronomagus();
        }
        else
        {
            // Place target piece
            PlaceTargetPiece();
        }
    }

    private void PlaceChronomagus()
    {
        Debug.Log($"[SingularitySummonPlate] Placing Chronomagus at ({x},{y})");
        
        // Determine Chronomagus name based on original player
        string chronomagusName = chronomagusX < 4 ? "white_chronomagus" : "black_chronomagus";
        
        // Create Chronomagus
        GameObject chronomagus = game.Create(chronomagusName, x, y);
        if (chronomagus != null)
        {
            Debug.Log($"[SingularitySummonPlate] Chronomagus recreated at ({x},{y})");
            chronomagusPlaced = true;
        }
        else
        {
            Debug.LogError($"[SingularitySummonPlate] Failed to create Chronomagus at ({x},{y})");
        }
    }

    private void PlaceTargetPiece()
    {
        Debug.Log($"[SingularitySummonPlate] Placing {targetPieceName} at ({x},{y})");
        
        // Create target piece
        GameObject targetPiece = game.Create(targetPieceName, x, y);
        if (targetPiece != null)
        {
            Debug.Log($"[SingularitySummonPlate] {targetPieceName} recreated at ({x},{y})");
            
            // Clean up all move plates
            foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
                Destroy(plate);
            
            // Reset flag for next use
            chronomagusPlaced = false;
            
            // End turn
            game.NextTurn();
        }
        else
        {
            Debug.LogError($"[SingularitySummonPlate] Failed to create {targetPieceName} at ({x},{y})");
        }
    }
}
