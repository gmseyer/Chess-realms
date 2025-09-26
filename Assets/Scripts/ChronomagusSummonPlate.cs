using UnityEngine;

public class ChronomagusSummonPlate : MonoBehaviour
{
    private Game game;
    private int x, y;
    private string player;

    public void Setup(Game g, int tileX, int tileY, string playerName)
    {
        game = g;
        x = tileX;
        y = tileY; 
        player = playerName;
    }

    private void OnMouseUp()
    {
        string chronomagusName = (player == "white") ? "white_chronomagus" : "black_chronomagus";
        
        Debug.Log($"[ChronomagusSummonPlate] Summoning {chronomagusName} at ({x},{y})");
        
        // Create the Chronomagus
        GameObject chronomagus = game.Create(chronomagusName, x, y);
        if (chronomagus != null)
        {
            Debug.Log($"[ChronomagusSummonPlate] Successfully summoned {chronomagusName} at ({x},{y})");
        }
        else
        {
            Debug.LogError($"[ChronomagusSummonPlate] Failed to create {chronomagusName} at ({x},{y})");
        }

        // Clean up all summon plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
 
        // End the turn
        game.NextTurn();
        
        Debug.Log("[ChronomagusSummonPlate] Chronomagus promotion completed - turn ended!");
    }
}
