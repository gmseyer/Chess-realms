using UnityEngine;

public class RequiemPlate : MonoBehaviour
{
    private GameObject royalBishop;
    private int x, y;
    private static int wraithPawnsCreated = 0;
    private static int maxWraithPawns = 2;

    public void Setup(GameObject royalBishopRef, int tileX, int tileY)
    {
        royalBishop = royalBishopRef;
        x = tileX;
        y = tileY;
    }

    private void OnMouseUp()
    {
        // Check if we can still create wraith pawns
        if (wraithPawnsCreated >= maxWraithPawns)
        {
            Debug.LogWarning("[RequiemPlate] Maximum wraith pawns (2) already created!");
            return;
        }

        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        
        // Check if position is still empty
        if (game.GetPosition(x, y) != null)
        {
            Debug.LogWarning($"[RequiemPlate] Position ({x},{y}) is no longer empty!");
            return;
        }

        Debug.Log($"[RequiemPlate] Creating wraith pawn at ({x},{y}) - {wraithPawnsCreated + 1}/2");

        // Create the wraith pawn
        GameObject wraithPawn = game.Create("white_wraith_pawn", x, y);
        if (wraithPawn != null)
        {
            wraithPawnsCreated++;
            Debug.Log($"[RequiemPlate] Wraith pawn {wraithPawnsCreated}/2 created successfully at ({x},{y})");
            
            // If this is the second wraith pawn, end the turn
            if (wraithPawnsCreated >= maxWraithPawns)
            {
                EndSoulRequiemTurn();
            }
        }
        else
        {
            Debug.LogError($"[RequiemPlate] Failed to create wraith pawn at ({x},{y})");
        }
    }

    private void EndSoulRequiemTurn()
    {
        Debug.Log("[RequiemPlate] Both wraith pawns created - ending Royal Bishop's turn");
        
        // Clean up all requiem plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
        
        // Reset counter for next use
        wraithPawnsCreated = 0;
        
        // End the Royal Bishop's turn
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        game.NextTurn();
        
        Debug.Log("[RequiemPlate] SoulRequiem skill completed successfully!");
    }

    // Static method to reset counter (can be called externally if needed)
    public static void ResetWraithPawnCounter()
    {
        wraithPawnsCreated = 0;
        Debug.Log("[RequiemPlate] Wraith pawn counter reset");
    }
}
