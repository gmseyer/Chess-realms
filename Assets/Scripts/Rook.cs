using System;  // <-- Add this at the top of your file
using UnityEngine;

public class Rook : Chessman
{
    // Public method to call from a Button
    public void OnClickRoyalCastling()
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        string currentPlayer = game.GetCurrentPlayer();

        // Debug: see exact player strings
        Debug.Log($"CurrentPlayer='{currentPlayer}', Rook Player='{GetPlayer()}'");

        // Only allow castling if it's this rook's turn
        if (!string.Equals(GetPlayer(), currentPlayer, StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log($"It's not {GetPlayer()}'s turn!");
            return;
        }

        // Call the RoyalCastling function
        RoyalCastling(game, currentPlayer);
    }

    // RoyalCastling that swaps this rook with its king
    public void RoyalCastling(Game game, string player)
    {
        // Find the king of the current player
        Chessman kingCm = FindKing(game, player);
        if (kingCm == null)
        {
            Debug.LogWarning("RoyalCastling: No king found for " + player);
            return;
        }

        // This rook is the one clicked
        Chessman rookCm = this;

        // Check SP
        const int cost = 1;
        if (!game.SpendPlayerSP(player, cost))
        {
            Debug.Log($"{player} does not have enough SP for Royal Castling (cost {cost}).");
            return;
        }

        // Save current positions
        int rookX = rookCm.GetXBoard();
        int rookY = rookCm.GetYBoard();
        int kingX = kingCm.GetXBoard();
        int kingY = kingCm.GetYBoard();

        // Clear old positions in the board array
        game.SetPositionEmpty(rookX, rookY);
        game.SetPositionEmpty(kingX, kingY);

        // Swap positions
        rookCm.SetXBoard(kingX);
        rookCm.SetYBoard(kingY);
        rookCm.SetCoords();
        game.SetPosition(rookCm.gameObject);

        kingCm.SetXBoard(rookX);
        kingCm.SetYBoard(rookY);
        kingCm.SetCoords();
        game.SetPosition(kingCm.gameObject);

        Debug.Log($"Royal Castling executed: {rookCm.name} swapped with {kingCm.name}");

        // Destroy move plates if needed
        rookCm.DestroyMovePlates();

        // End turn
        game.NextTurn();
    }

    // Helper: find king for current player
    private Chessman FindKing(Game game, string player)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = game.GetPosition(x, y);
                if (piece == null) continue;
                Chessman cm = piece.GetComponent<Chessman>();
                if (cm != null && piece.name.ToLower().Contains("king") && cm.GetPlayer() == player)
                    return cm;
            }
        }
        return null;
    }
}
