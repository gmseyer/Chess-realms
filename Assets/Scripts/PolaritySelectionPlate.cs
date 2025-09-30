using UnityEngine;

public class PolaritySelectionPlate : MonoBehaviour
{
    private int x, y;

    public void Setup(int tileX, int tileY)
    {
        x = tileX;
        y = tileY;
    }

    private void OnMouseUp()
    {
        Debug.Log($"[PolaritySelectionPlate] Polarity target selected at ({x},{y})!");

        // Get the piece at this position
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[PolaritySelectionPlate] Could not find Game component!");
            return;
        }

        GameObject selectedPiece = game.GetPosition(x, y);
        if (selectedPiece == null)
        {
            Debug.LogError("[PolaritySelectionPlate] No piece found at selected position!");
            return;
        }

        Chessman selectedChessman = selectedPiece.GetComponent<Chessman>();
        if (selectedChessman == null)
        {
            Debug.LogError("[PolaritySelectionPlate] No Chessman component found!");
            return;
        }

        string pieceName = selectedChessman.name;
        bool hasStoneSentinel = selectedChessman.statusManager.HasStatus(StatusType.StoneSentinel, game.turns);

        // Validate target (Terra Ward or piece with Stone Sentinel)
        if (pieceName != "tile_terra_ward" && !hasStoneSentinel)
        {
            Debug.LogError($"[PolaritySelectionPlate] Invalid target! Must be Terra Ward or piece with Stone Sentinel status.");
            return;
        }

        // Check if this is self-propelling (EarthboundBishop with Stone Sentinel)
        bool isSelfPropelling = (pieceName == "white_earth_bishop" && hasStoneSentinel);

        if (isSelfPropelling)
        {
            Debug.Log("[PolaritySelectionPlate] Self-propelling EarthboundBishop selected!");
        }
        else
        {
            Debug.Log($"[PolaritySelectionPlate] {pieceName} selected for Polarity movement!");
        }

        // Store the selected piece position for later use
        EarthboundBishop.selectedPolarityPiecePosition = new Vector2Int(x, y);

        // Destroy all selection plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // Generate Queen-like movement paths from selected piece
        EarthboundBishop.GeneratePolarityMovePlates(x, y, isSelfPropelling);

        Debug.Log($"[PolaritySelectionPlate] Generated Queen-like movement paths from ({x},{y})");
    }
}
