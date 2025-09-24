using UnityEngine;

public class RussianRouletteTargetPlate : MonoBehaviour
{
    private Game game;
    private int x, y;
    private string targetPieceName;
    private Pawn castingPawn;

    public void Setup(Game gameRef, int posX, int posY, string pieceName, Pawn pawn)
    {
        game = gameRef;
        x = posX;
        y = posY;
        targetPieceName = pieceName;
        castingPawn = pawn;
    }

    private void OnMouseUp()
    {
        if (game == null || castingPawn == null) return;

        // Get the target piece
        GameObject targetPiece = game.GetPosition(x, y);
        if (targetPiece != null)
        {
            Chessman targetChessman = targetPiece.GetComponent<Chessman>();
            if (targetChessman != null && targetChessman.name == targetPieceName)
            {
                Debug.Log($"[Russian Roulette] Destroying enemy pawn: {targetPieceName} at ({x},{y})");
                
                // Destroy the target piece
                game.SetPositionEmpty(x, y);
                Destroy(targetPiece);
                
                // Clean up all move plates
                DestroyAllMovePlates();
                
                // End turn
                game.NextTurn();
            }
        }
    }

    private void DestroyAllMovePlates()
    {
        // Destroy all move plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
    }
}
