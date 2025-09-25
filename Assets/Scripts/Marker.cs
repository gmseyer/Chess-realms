using UnityEngine;

public enum MarkerType
{
    Fire,
    Ice
}

public class Marker : MonoBehaviour
{
    public int x, y;
    public int expireTurn;
    public string tileName; // "tile_lava" or "tile_ice" for when it converts
    public string trackedPieceName; // Track the specific piece this marker was placed on
    public bool wasHandledByAttack = false; // Flag to prevent double handling
    public MarkerType markerType; // Fire or Ice
    
    private Game game;
    
    public void Setup(Game gameRef, int posX, int posY, string tile, int expirationTurn, string pieceName, MarkerType type)
    {
        game = gameRef;
        x = posX;
        y = posY;
        tileName = tile;
        expireTurn = expirationTurn;
        trackedPieceName = pieceName;
        markerType = type;
    }
    
    // Check if this marker still has the tracked piece on it
    public bool HasTrackedPieceOnTile()
    {
        if (game == null) return false;
        GameObject piece = game.GetPosition(x, y);
        return piece != null && piece.name == trackedPieceName;
    }
    
    // Check if the tracked piece has left (either moved or been captured)
    public bool HasTrackedPieceLeft()
    {
        if (game == null) return true;
        GameObject piece = game.GetPosition(x, y);
        return piece == null || piece.name != trackedPieceName;
    }
    
    // Check if tracked piece moved away (not captured)
    public bool HasTrackedPieceMovedAway()
    {
        if (game == null) return true;
        GameObject piece = game.GetPosition(x, y);
        // Piece moved away if tile is empty or has a different piece
        return piece == null || piece.name != trackedPieceName;
    }
    
    // Check if tracked piece was captured (tile is empty)
    public bool WasTrackedPieceCaptured()
    {
        if (game == null) return true;
        GameObject piece = game.GetPosition(x, y);
        // Piece was captured if tile is completely empty
        return piece == null;
    }
    
    // Convert this marker to its corresponding tile
    public void ConvertToTile()
    {
        if (game == null) return;
        
        // Create the tile at this position
        GameObject tile = game.Create(tileName, x, y);
        
        // Register the tile with ElementalBishop for duration tracking
        ElementalBishop eb = FindObjectOfType<ElementalBishop>();
        if (eb != null)
        {
            eb.RegisterTile(tile);
            Debug.Log($"[Marker] {tileName} registered with ElementalBishop for duration tracking");
        }
        else
        {
            Debug.LogError($"[Marker] Could not find ElementalBishop to register {tileName}!");
        }
        
        // Destroy this marker
        Destroy(gameObject);
        
        Debug.Log($"[Marker] Converted to {tileName} at ({x},{y}) with 5-turn duration");
    }
    
    // Check if this marker has expired
    public bool IsExpired(int currentTurn)
    {
        return currentTurn >= expireTurn;
    }
}
