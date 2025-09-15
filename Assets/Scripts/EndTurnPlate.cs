using UnityEngine;

public class EndTurnPlate : MonoBehaviour
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
        Debug.Log($"[DivineOffering] Spawning {pieceName} at ({x},{y})");
        game.Create(pieceName, x, y);

        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate") )
            Destroy(plate);
    }
}
