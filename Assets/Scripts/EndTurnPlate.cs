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
        game.NextTurn();

        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
    }
}
