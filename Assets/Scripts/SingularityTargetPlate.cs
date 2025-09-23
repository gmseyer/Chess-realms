using UnityEngine;

public class SingularityTargetPlate : MonoBehaviour
{
    private Game game;
    private int x, y;
    private string targetPieceName;

    public void Setup(Game g, int tileX, int tileY, string pieceName)
    {
        game = g;
        x = tileX;
        y = tileY;
        targetPieceName = pieceName;
    }

    private void OnMouseUp()
    {
        Debug.Log($"[SingularityTargetPlate] Target selected: {targetPieceName} at ({x},{y})");
        
        // Get the Chronomagus piece
        GameObject chronomagus = null;
        for (int cx = 0; cx < 8; cx++)
        {
            for (int cy = 0; cy < 8; cy++)
            {
                GameObject piece = game.GetPosition(cx, cy);
                if (piece != null && piece.name.Contains("chronomagus"))
                {
                    chronomagus = piece;
                    break;
                }
            }
            if (chronomagus != null) break;
        }

        if (chronomagus == null)
        {
            Debug.LogError("[SingularityTargetPlate] Chronomagus not found!");
            return;
        }

        // Get target piece
        GameObject targetPiece = game.GetPosition(x, y);
        if (targetPiece == null)
        {
            Debug.LogError("[SingularityTargetPlate] Target piece not found!");
            return;
        }

        // Store target piece info for recreation
        string targetPlayer = targetPiece.GetComponent<Chessman>().GetPlayer();
        int chronomagusX = chronomagus.GetComponent<Chessman>().GetXBoard();
        int chronomagusY = chronomagus.GetComponent<Chessman>().GetYBoard();

        // Store information for recreation
        // Ensure SingularityManager exists
        if (SingularityManager.Instance == null)
        {
            GameObject managerObj = new GameObject("SingularityManager");
            managerObj.AddComponent<SingularityManager>();
        }
        SingularityManager.Instance.SetSingularityData(targetPieceName, targetPlayer, chronomagusX, chronomagusY);

        // Destroy both pieces
        game.SetPositionEmpty(chronomagusX, chronomagusY);
        game.SetPositionEmpty(x, y);
        Destroy(chronomagus);
        Destroy(targetPiece);

        // Clean up all move plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        Debug.Log($"[SingularityTargetPlate] Both pieces removed from board. They will reappear in 2 turns.");
        
        // End turn
        game.NextTurn();
    }
}
