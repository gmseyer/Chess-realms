using UnityEngine;

public class RoyalKnightSummonPlate : MonoBehaviour
{
    private Game game;
    private int x, y;
    private string pieceName;

    public void Setup(Game g, int tileX, int tileY, string name)
    {
        game = g;
        x = tileX;
        y = tileY;
        pieceName = name;
    }

    private void OnMouseUp()
    {
        Debug.Log($"[RoyalKnightSummonPlate] Summoning {pieceName} at ({x},{y})");
        
        // Create the royal knight at this position
        GameObject royalKnight = game.Create(pieceName, x, y);
        if (royalKnight != null)
        {
            Debug.Log($"[RoyalKnightSummonPlate] Successfully summoned {pieceName} at ({x},{y})");
        }
        else
        {
            Debug.LogError($"[RoyalKnightSummonPlate] Failed to create {pieceName} at ({x},{y})");
        }
        
        // Destroy all move plates (but preserve other Royal Knight summon plates until after creation)
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
        {
            // Don't destroy other Royal Knight summon plates yet
            if (plate.GetComponent<RoyalKnightSummonPlate>() == null)
            {
                Destroy(plate);
            }
        }
        
        // Now destroy all Royal Knight summon plates after the royal knight is created
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
        {
            if (plate.GetComponent<RoyalKnightSummonPlate>() != null)
            {
                Destroy(plate);
            }
        }
        
        // End turn
       // game.NextTurn();
    }
}
