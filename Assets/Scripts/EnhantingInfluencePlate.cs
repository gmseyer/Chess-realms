using UnityEngine;
using System.Collections.Generic;

public class EnchantingInfluencePlate : MonoBehaviour
{
    private Game game;
    private int x, y;
    private Queen queen;

    public void Setup(Game gameRef, int xPos, int yPos, Queen queenRef)
    {
        game = gameRef;
        x = xPos;
        y = yPos;
        queen = queenRef;
    }

    public void OnMouseUp()
    {
        Debug.Log($"[Enchanting Influence] Tile clicked at ({x},{y})");
        
        // Check if there's a piece at this position
        GameObject pieceAtPosition = game.GetPosition(x, y);
        
        if (pieceAtPosition == null)
        {
            // No piece here, just destroy moveplates and return
            Debug.Log("[Enchanting Influence] No piece at this position. Destroying moveplates.");
            DestroyAllMovePlates();
            return;
        }
        
        // Get the piece's Chessman component
        Chessman targetCm = pieceAtPosition.GetComponent<Chessman>();
        if (targetCm == null)
        {
            Debug.Log("[Enchanting Influence] No Chessman component found. Destroying moveplates.");
            DestroyAllMovePlates();
            return;
        }
        
        // Get current player to check if it's an enemy
        string currentPlayer = game.GetCurrentPlayer();
        
        // Check if it's an enemy piece
        if (targetCm.GetPlayer() == currentPlayer)
        {
            Debug.Log($"[Enchanting Influence] {targetCm.name} is friendly. Destroying moveplates.");
            DestroyAllMovePlates();
            return;
        }
        
        // Check if it's a King or Queen (exclude them)
        if (targetCm.name.Contains("king") || targetCm.name.Contains("queen"))
        {
            Debug.Log($"[Enchanting Influence] Cannot target {targetCm.name} (King or Queen). Destroying moveplates.");
            DestroyAllMovePlates();
            return;
        }
        
        // It's an enemy piece that's not King or Queen - proceed with random movement
        Debug.Log($"[Enchanting Influence] Targeting enemy {targetCm.name} for random movement.");
        
        // Find empty tiles around the target piece
        List<Vector2Int> emptyTiles = FindEmptyTilesAround(x, y);
        
        if (emptyTiles.Count == 0)
        {
            Debug.Log("[Enchanting Influence] No empty tiles around target. Destroying moveplates.");
            DestroyAllMovePlates();
            return;
        }
        
        // Randomly pick one of the empty tiles
        Vector2Int randomTile = emptyTiles[Random.Range(0, emptyTiles.Count)];
        
        // Move the enemy piece to the random position
        MoveEnemyPiece(pieceAtPosition, targetCm, randomTile.x, randomTile.y);
        
        // NOW deduct SP after successful move
       const int enchantingInfluenceCost = 2;
SkillManager.Instance.SpendPlayerSP(currentPlayer, enchantingInfluenceCost);
Debug.Log($"[Enchanting Influence] SP deducted: {enchantingInfluenceCost}");

// Mark skill as used (once-per-battle)
queen.hasUsedEnchantingInfluence = true;
Debug.Log("[Enchanting Influence] Skill marked as used for this battle.");

// Destroy moveplates immediately after moving enemy
DestroyAllMovePlates();

// End the Queen's turn
game.NextTurn();
    }
    
    private List<Vector2Int> FindEmptyTilesAround(int centerX, int centerY)
    {
        List<Vector2Int> emptyTiles = new List<Vector2Int>();
        
        // Check all 8 directions around the target piece
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue; // Skip the center tile
                
                int checkX = centerX + dx;
                int checkY = centerY + dy;
                
                // Check if position is on board
                if (!game.PositionOnBoard(checkX, checkY)) continue;
                
                // Check if position is empty
                if (game.GetPosition(checkX, checkY) == null)
                {
                    emptyTiles.Add(new Vector2Int(checkX, checkY));
                }
            }
        }
        
        Debug.Log($"[Enchanting Influence] Found {emptyTiles.Count} empty tiles around ({centerX},{centerY})");
        return emptyTiles;
    }
    
    private void MoveEnemyPiece(GameObject piece, Chessman targetCm, int newX, int newY)
    {
        // Clear the old position
        game.ClearPosition(targetCm.GetXBoard(), targetCm.GetYBoard());
        
        // Update the piece's coordinates
        targetCm.SetXBoard(newX);
        targetCm.SetYBoard(newY);
        targetCm.SetCoords(); // Update visual position
        
        // Set the piece at the new position
        game.SetPositionAt(piece, newX, newY);
        
        Debug.Log($"[Enchanting Influence] Moved {targetCm.name} from ({targetCm.GetXBoard()},{targetCm.GetYBoard()}) to ({newX},{newY})");
    }
    
    private void DestroyAllMovePlates()
    {
        // Destroy all move plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
    }
}