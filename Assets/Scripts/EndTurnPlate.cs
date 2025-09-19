using UnityEngine;

public class EndTurnPlate : MonoBehaviour
{

    //*******FOR DIVINE OFFERING*******
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

        // In Rook.cs, in AttemptFortify() method:
if (SkillTracker.Instance != null)
{
    SkillTracker.Instance.LogSkillUsage(game.GetCurrentPlayer(), pieceName, "DIVINE OFFERING", 0);
}
    }
}
