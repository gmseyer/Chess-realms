using UnityEngine;

public class HealingBenedictionPlate : MonoBehaviour
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
        Debug.Log($"[HealingBenediction] Tile clicked — ({x},{y})");

        // 1️⃣ Figure out which piece belongs here
        string pieceToRevive = GetWhitePieceForSpawn(x, y);
        if (pieceToRevive != null && game.GetPosition(x, y) == null)
        {
            // 2️⃣ Revive the piece
            game.Create(pieceToRevive, x, y);
            Debug.Log($"Revived {pieceToRevive} at ({x},{y})");

            // 3️⃣ End turn after revival
            game.NextTurn();
        }
        else
        {
            Debug.Log("No piece revived — either tile is not a spawn or already occupied.");
        }

        // 4️⃣ Destroy all move plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
    }


     private string GetWhitePieceForSpawn(int x, int y)
    {
        // White back rank
        if (y == 0)
        {
            switch (x)
            {
                case 0: return "white_rook";
                case 1: return "white_knight";
                case 2: return "white_bishop";
                case 3: return "white_queen";
                case 4: return "white_king";
                case 5: return "white_bishop";
                case 6: return "white_knight";
                case 7: return "white_rook";
            }
        }
        // White pawns
        if (y == 1)
        {
            switch (x)
            {
                case 0: return "white_pawn";
                case 1: return "white_pawn1";
                case 2: return "white_pawn2";
                case 3: return "white_pawn3";
                case 4: return "white_pawn4";
                case 5: return "white_pawn5";
                case 6: return "white_pawn6";
                case 7: return "white_pawn7";
            }
        }
        return null; // not a white spawn tile
    }
}
