using UnityEngine;

public class EndTurnPlate : MonoBehaviour
{
    private Game game;
    private int x, y;

    public void Setup(Game g, int tileX, int tileY)
    {
        game = g;
        x = tileX;
        y = tileY;
    }

    private void OnMouseUp()
    {
        Debug.Log($"[DivineOffering] Tile clicked — ending turn at ({x},{y})");

        game.Create("white_elemental_bishop", x, y);
        //game.NextTurn(); 
        // if this is activated, even when your spawning white elemental bishop, it is still white's turn

        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
    }
}
