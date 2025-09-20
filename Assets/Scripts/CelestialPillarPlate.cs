using UnityEngine;
using System.Collections;

public class CelestialPillarPlate : MonoBehaviour
{
    private Game game;
    private int x, y;

    public void Setup(Game g, int tileX, int tileY)
    {
        game = g;
        x = tileX;
        y = tileY;
    }

    private void OnMouseUp()
    {
        Debug.Log($"[CelestialPillarPlate] Clicked at ({x},{y})");
        
        // Check if tile is still empty
        if (game.GetPosition(x, y) != null)
        {
            Debug.Log("[CelestialPillarPlate] Tile is occupied — pillar not created.");
            return;
        }

        // Create the celestial pillar
        GameObject pillar = game.Create("celestial_pillar", x, y);
        
        if (pillar != null)
        {
            Debug.Log($"[CelestialPillarPlate] Successfully created celestial pillar at ({x},{y})");
            
            // Trigger shockwave effect
            TriggerShockwave(x, y);
        }
        else
        {
            Debug.LogError($"[CelestialPillarPlate] Failed to create celestial pillar at ({x},{y})");
        }

        // Clean up all move plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // End the Royal Rook's turn
        
    }

    private void TriggerShockwave(int pillarX, int pillarY)
    {
        Debug.Log($"[CelestialPillarPlate] Triggering shockwave from pillar at ({pillarX},{pillarY})");
        
        // Define the 8 positions around the pillar (clockwise from top-left)
        // Position 1: top-left, 2: top-center, 3: top-right, 4: middle-right
        // Position 5: pillar (center), 6: middle-left, 7: bottom-left, 8: bottom-center, 9: bottom-right
        Vector2Int[] positions = {
            new Vector2Int(pillarX - 1, pillarY + 1), // 1: top-left
            new Vector2Int(pillarX, pillarY + 1),     // 2: top-center
            new Vector2Int(pillarX + 1, pillarY + 1), // 3: top-right
            new Vector2Int(pillarX + 1, pillarY),     // 4: middle-right
            new Vector2Int(pillarX + 1, pillarY - 1), // 6: bottom-right
            new Vector2Int(pillarX, pillarY - 1),     // 8: bottom-center
            new Vector2Int(pillarX - 1, pillarY - 1), // 7: bottom-left
            new Vector2Int(pillarX - 1, pillarY)      // 6: middle-left
        };
        
        // Define push directions for each position (away from center)
        Vector2Int[] pushDirections = {
            new Vector2Int(-1, 1),   // 1: top-left -> push diagonally away
            new Vector2Int(0, 1),    // 2: top-center -> push vertically up
            new Vector2Int(1, 1),    // 3: top-right -> push diagonally away
            new Vector2Int(1, 0),    // 4: middle-right -> push horizontally right
            new Vector2Int(1, -1),   // 6: bottom-right -> push diagonally away
            new Vector2Int(0, -1),   // 8: bottom-center -> push vertically down
            new Vector2Int(-1, -1),  // 7: bottom-left -> push diagonally away
            new Vector2Int(-1, 0)    // 6: middle-left -> push horizontally left
        };
        
        // Process each position clockwise
        for (int i = 0; i < positions.Length; i++)
        {
            Vector2Int pos = positions[i];
            Vector2Int pushDir = pushDirections[i];
            
            // Check if position is on board
            if (!game.PositionOnBoard(pos.x, pos.y))
                continue;
                
            // Check if there's a piece at this position
            GameObject piece = game.GetPosition(pos.x, pos.y);
            if (piece == null)
                continue;
                
            // Skip the pillar itself (shouldn't happen but safety check)
            if (piece.name == "celestial_pillar")
                continue;
                
            // Calculate target position
            Vector2Int targetPos = new Vector2Int(pos.x + pushDir.x, pos.y + pushDir.y);
            
            // Check if target position is valid (on board and empty)
            if (game.PositionOnBoard(targetPos.x, targetPos.y) && game.GetPosition(targetPos.x, targetPos.y) == null)
            {
                // Move the piece
                MovePieceToPosition(piece, pos, targetPos);
                Debug.Log($"[CelestialPillarPlate] Pushed {piece.name} from ({pos.x},{pos.y}) to ({targetPos.x},{targetPos.y})");
            }
            else
            {
                Debug.Log($"[CelestialPillarPlate] Cannot push {piece.name} from ({pos.x},{pos.y}) - target position blocked");
            }
        }
        game.NextTurn();
    }
    
    private void MovePieceToPosition(GameObject piece, Vector2Int fromPos, Vector2Int toPos)
    {
        // Clear the old position
        game.SetPositionEmpty(fromPos.x, fromPos.y);
        
        // Update the piece's coordinates
        Chessman pieceCm = piece.GetComponent<Chessman>();
        if (pieceCm != null)
        {
            pieceCm.SetXBoard(toPos.x);
            pieceCm.SetYBoard(toPos.y);
            pieceCm.SetCoords(); // Update visual position
        }
        
        // Set the piece at the new position
        game.SetPosition(piece);
    }
}
