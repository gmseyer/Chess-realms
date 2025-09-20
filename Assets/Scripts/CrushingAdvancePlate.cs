using UnityEngine;
using System.Collections.Generic;

public class CrushingAdvancePlate : MonoBehaviour
{
    private Game game;
    private int x, y;
    private int distance;
    private int xIncrement, yIncrement;

    public void Setup(Game g, int tileX, int tileY, int dist, int xInc, int yInc)
    {
        game = g;
        x = tileX;
        y = tileY;
        distance = dist;
        xIncrement = xInc;
        yIncrement = yInc;
    }

    private void OnMouseUp()
    {
        Debug.Log($"[CrushingAdvancePlate] Clicked at ({x},{y}) - distance {distance}");
        
        // Get the Royal Rook's current position
        GameObject selectedPiece = UIManager.Instance.selectedPiece;
        if (selectedPiece == null)
        {
            Debug.LogError("[CrushingAdvancePlate] No piece selected!");
            return;
        }

        Chessman rookCm = selectedPiece.GetComponent<Chessman>();
        if (rookCm == null)
        {
            Debug.LogError("[CrushingAdvancePlate] No Chessman component found!");
            return;
        }

        int rookX = rookCm.GetXBoard();
        int rookY = rookCm.GetYBoard();

        // Execute crushing advance
        ExecuteCrushingAdvance(selectedPiece, rookX, rookY, x, y);

        // Clean up all move plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // End the Royal Rook's turn
        game.NextTurn();
    }

    private void ExecuteCrushingAdvance(GameObject rook, int startX, int startY, int endX, int endY)
    {
        Debug.Log($"[CrushingAdvancePlate] Executing crushing advance from ({startX},{startY}) to ({endX},{endY})");
        
        // Get all pieces in the path
        List<GameObject> piecesInPath = GetPiecesInPath(startX, startY, endX, endY);
        
        // Move or destroy pieces in the path
        foreach (GameObject piece in piecesInPath)
        {
            if (piece != null)
            {
                Vector2Int newPosition = FindNearestEmptyTile(piece, startX, startY, endX, endY);
                if (newPosition != Vector2Int.zero)
                {
                    // Move the piece
                    MovePieceToPosition(piece, newPosition);
                    Debug.Log($"[CrushingAdvancePlate] Moved {piece.name} to ({newPosition.x},{newPosition.y})");
                }
                else
                {
                    // Destroy the piece
                    Destroy(piece);
                    Debug.Log($"[CrushingAdvancePlate] Destroyed {piece.name} - no empty space available");
                }
            }
        }
        
        // Move the Royal Rook to the chosen position
        MoveRoyalRookToPosition(rook, endX, endY);
    }

    private List<GameObject> GetPiecesInPath(int startX, int startY, int endX, int endY)
    {
        List<GameObject> pieces = new List<GameObject>();
        
        // Calculate direction
        int xDir = (endX > startX) ? 1 : (endX < startX) ? -1 : 0;
        int yDir = (endY > startY) ? 1 : (endY < startY) ? -1 : 0;
        
        // Check each tile in the path
        for (int i = 1; i <= distance; i++)
        {
            int checkX = startX + (xDir * i);
            int checkY = startY + (yDir * i);
            
            if (game.PositionOnBoard(checkX, checkY))
            {
                GameObject piece = game.GetPosition(checkX, checkY);
                if (piece != null && piece.name != "celestial_pillar")
                {
                    pieces.Add(piece);
                }
            }
        }
        
        return pieces;
    }

    private Vector2Int FindNearestEmptyTile(GameObject piece, int startX, int startY, int endX, int endY)
    {
        // Calculate direction to exclude the path
        int xDir = (endX > startX) ? 1 : (endX < startX) ? -1 : 0;
        int yDir = (endY > startY) ? 1 : (endY < startY) ? -1 : 0;
        
        // Get piece's current position
        Chessman pieceCm = piece.GetComponent<Chessman>();
        if (pieceCm == null) return Vector2Int.zero;
        
        int pieceX = pieceCm.GetXBoard();
        int pieceY = pieceCm.GetYBoard();
        
        // Search in 3x3 radius around the piece
        for (int radius = 1; radius <= 3; radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int checkX = pieceX + dx;
                    int checkY = pieceY + dy;
                    
                    // Skip if not on board
                    if (!game.PositionOnBoard(checkX, checkY)) continue;
                    
                    // Skip if position is empty
                    if (game.GetPosition(checkX, checkY) != null) continue;
                    
                    // Skip if position is in the crushing advance path
                    if (IsInPath(checkX, checkY, startX, startY, endX, endY)) continue;
                    
                    return new Vector2Int(checkX, checkY);
                }
            }
        }
        
        return Vector2Int.zero; // No empty tile found
    }

    private bool IsInPath(int checkX, int checkY, int startX, int startY, int endX, int endY)
    {
        // Calculate direction
        int xDir = (endX > startX) ? 1 : (endX < startX) ? -1 : 0;
        int yDir = (endY > startY) ? 1 : (endY < startY) ? -1 : 0;
        
        // Check if position is in the path
        for (int i = 1; i <= distance; i++)
        {
            int pathX = startX + (xDir * i);
            int pathY = startY + (yDir * i);
            
            if (checkX == pathX && checkY == pathY)
                return true;
        }
        
        return false;
    }

    private void MovePieceToPosition(GameObject piece, Vector2Int newPos)
    {
        // Clear the old position
        Chessman pieceCm = piece.GetComponent<Chessman>();
        if (pieceCm != null)
        {
            game.SetPositionEmpty(pieceCm.GetXBoard(), pieceCm.GetYBoard());
            
            // Update the piece's coordinates
            pieceCm.SetXBoard(newPos.x);
            pieceCm.SetYBoard(newPos.y);
            pieceCm.SetCoords(); // Update visual position
        }
        
        // Set the piece at the new position
        game.SetPosition(piece);
    }

    private void MoveRoyalRookToPosition(GameObject rook, int newX, int newY)
    {
        // Clear the old position
        Chessman rookCm = rook.GetComponent<Chessman>();
        if (rookCm != null)
        {
            game.SetPositionEmpty(rookCm.GetXBoard(), rookCm.GetYBoard());
            
            // Update the rook's coordinates
            rookCm.SetXBoard(newX);
            rookCm.SetYBoard(newY);
            rookCm.SetCoords(); // Update visual position
        }
        
        // Set the rook at the new position
        game.SetPosition(rook);
        
        Debug.Log($"[CrushingAdvancePlate] Royal Rook moved to ({newX},{newY})");
    }
}
