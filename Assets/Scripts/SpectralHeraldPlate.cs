using UnityEngine;

public class SpectralHeraldPlate : MonoBehaviour
{
    private Game game;
    private int x, y;
    private string pieceName;

    public void Setup(Game g, int tileX, int tileY, string piece)
    {
        game = g;
        x = tileX;
        y = tileY;
        pieceName = piece;
    }

    private void OnMouseUp()
    {
        Debug.Log($"[SpectralHeraldPlate] Summoning {pieceName} at ({x},{y})");
        
        // Create the Spectral Herald
        GameObject spectralHerald = game.Create(pieceName, x, y);
        if (spectralHerald != null)
        {
            Debug.Log($"[SpectralHeraldPlate] Successfully summoned {pieceName} at ({x},{y})");
        }
        else
        {
            Debug.LogError($"[SpectralHeraldPlate] Failed to create {pieceName} at ({x},{y})");
        }

        // Clean up all summon plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // End the turn
        game.NextTurn();
        
        Debug.Log("[SpectralHeraldPlate] Echo skill completed - turn ended!");
    }
}
