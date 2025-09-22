using UnityEngine;

public class SanctuaryMarker : MonoBehaviour
{
    private Game game;
    private int x, y;
    private int expirationTurn;

    public void Setup(Game g, int tileX, int tileY, int turns)
    {
        game = g;
        x = tileX; 
        y = tileY;
        expirationTurn = turns;
    }

    public int GetX() { return x; }
    public int GetY() { return y; }
    public int GetExpirationTurn() { return expirationTurn; }

    // Check if this sanctuary marker is still active
    public bool IsActive()
    {
        if (game == null) return false;
        return game.turns < expirationTurn;
    }

    // Called when a piece dies on this tile
    public void OnPieceDeath(Chessman capturedPiece)
    {
        if (!IsActive()) return;

        // Check if the captured piece is an allied piece (white)
        if (capturedPiece.GetPlayer() != "white") return;

        // Gain 1 SP for the white player using SkillManager
        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.AddPlayerSP("white", 1);
            Debug.Log($"[Sanctuary Marker] Allied piece {capturedPiece.name} died in Sacred Zone at ({x},{y}) - gained 1 SP!");
        }
    }

    // Check if this marker should be destroyed due to expiration
    public void CheckExpiration()
    {
        if (!IsActive())
        {
            Debug.Log($"[Sanctuary Marker] Sanctuary marker at ({x},{y}) expired on turn {game.turns}");
            Destroy(gameObject);
        }
    }
}
