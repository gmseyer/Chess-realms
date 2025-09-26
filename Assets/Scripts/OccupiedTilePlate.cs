using UnityEngine;

public class OccupiedTilePlate : MonoBehaviour
{
    private Game game;
    private int x, y;
    private string tileName;

    public void Setup(Game g, int tileX, int tileY, string name)
    {
        game = g;
        x = tileX;
        y = tileY;
        tileName = name;
    }

    private void OnMouseUp()
    {
        // Determine marker type based on tile name
        MarkerType markerType = (tileName == "tile_lava") ? MarkerType.Fire : MarkerType.Ice;
        string markerTypeName = (markerType == MarkerType.Fire) ? "fire" : "ice";
        
        Debug.Log($"[OccupiedTilePlate] Placing {markerTypeName} marker at ({x},{y})");
        
        // Create marker at this position
        CreateMarker(markerType);
        
        // Hide the ElementalBishop panel
        if (UIManager.Instance != null)
        {
            UIManager.Instance.whiteElementalBishopPanel?.SetActive(false);
        }
        
        // End turn (same as SkillEndTurnPlate)
        game.NextTurn();
        
        // Destroy all move plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
    }
    
    private void CreateMarker(MarkerType markerType)
    {
        // Get current turn and calculate expiration (5 turns like tiles)
        int currentTurn = game.GetTurnCount();
        int expirationTurn = currentTurn + 5;
        
        // Create marker GameObject (not a piece, just visual marker)
        string markerTypeName = (markerType == MarkerType.Fire) ? "Fire" : "Ice";
        GameObject marker = new GameObject($"{markerTypeName}Marker_{x}_{y}");
        marker.transform.position = new Vector3(x * 0.57f - 1.98f, y * 0.56f - 1.95f, -2f); // Slightly above the piece
        
        // Get the piece name at this position
        GameObject pieceAtPosition = game.GetPosition(x, y);
        string pieceName = pieceAtPosition != null ? pieceAtPosition.name : "unknown";
        
        // Add Marker component
        Marker markerScript = marker.AddComponent<Marker>();
        markerScript.Setup(game, x, y, tileName, expirationTurn, pieceName, markerType);
        
        // Add sprite renderer for visual
        SpriteRenderer sr = marker.AddComponent<SpriteRenderer>();
        
        // Try to load the appropriate marker sprite from Resources
        Sprite markerSprite = null;
        string spriteName = "";
        
        if (markerType == MarkerType.Fire)
        {
            markerSprite = Resources.Load<Sprite>("FireMarker");
            spriteName = "FireMarker";
        }
        else if (markerType == MarkerType.Ice)
        {
            markerSprite = Resources.Load<Sprite>("IceMarker");
            spriteName = "IceMarker";
        }
        else
        {
            markerSprite = Resources.Load<Sprite>("ElementalMarker");
            spriteName = "ElementalMarker";
        }
        
        if (markerSprite != null)
        {
            sr.sprite = markerSprite;
            // Scale down to match fallback sprite size (64x64)
            marker.transform.localScale = new Vector3(0.1f, 0.1f, 1f);
            Debug.Log($"[OccupiedTilePlate] Using {spriteName} sprite for {markerTypeName} marker");
        }
        else
        {
            // Fallback to colored square if sprite not found
            Debug.LogWarning($"[OccupiedTilePlate] {spriteName} sprite not found in Resources folder, using fallback");
            Texture2D texture = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];
            Color markerColor = (markerType == MarkerType.Fire) ? Color.red : Color.blue;
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = markerColor;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            
            Sprite fallbackSprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
            sr.sprite = fallbackSprite;
        }
        
        // Set color and transparency
        sr.color = new Color(1f, 1f, 1f, 0.8f); // White with 80% opacity (preserves sprite colors)
        sr.sortingOrder = 1; // Above pieces
        
        // Register with ElementalBishop for tracking
        ElementalBishop eb = FindObjectOfType<ElementalBishop>();
        if (eb != null)
        {
            eb.RegisterMarker(markerScript);
        }
        
        Debug.Log($"[OccupiedTilePlate] {markerTypeName} marker created at ({x},{y}), expires on turn {expirationTurn}");
    } 
}