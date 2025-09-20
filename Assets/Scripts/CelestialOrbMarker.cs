using UnityEngine;

public class CelestialOrbMarker : MonoBehaviour
{
    private Game game;
    private int x, y;

    public void Setup(Game g, int tileX, int tileY)
    {
        game = g;
        x = tileX; 
        y = tileY;
    }

    public int GetX() { return x; }
    public int GetY() { return y; }

    private void OnMouseUp()
    {
        Debug.Log($"[CelestialOrbMarker] Celestial Orb clicked at ({x},{y})");
        
        // Check if there's a Rook on this tile
        GameObject pieceAtPosition = game.GetPosition(x, y);
        if (pieceAtPosition != null && pieceAtPosition.name.Contains("rook"))
        {
            // Rook captured this orb
            Debug.Log($"[CelestialOrbMarker] Rook {pieceAtPosition.name} captured Celestial Orb!");
            
            // Destroy this orb
            Destroy(gameObject);
            
            // TODO: Check if all orbs are captured for promotion
        }
        else
        {
            Debug.Log("[CelestialOrbMarker] No Rook on this tile - orb not captured");
        }
    }
}
