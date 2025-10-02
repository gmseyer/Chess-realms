using UnityEngine;

public class HealingBenedictionPlate : MonoBehaviour
{
    private Game game;
    private int x, y;
    private string player; // Store the player who cast the skill

    public void Setup(Game g, int tileX, int tileY)
    {
        game = g;
        x = tileX;
        y = tileY;
        
        // Get player from current turn
        player = g.GetCurrentPlayer();
    }

    private void OnMouseUp() 
{
    Debug.Log($"[HealingBenedictionPlate] Clicked at ({x},{y}) by {player} player");

    // ✅ NEW: Use CooldownManager instead of hasUsedHealingBenediction
    if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "HealingBenediction"))
    {
        Debug.LogWarning("[HealingBenedictionPlate] Skill already used — click ignored.");
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
        return;
    }

    // ✅ Get player-specific piece for revival
    string pieceToRevive = GetPieceForSpawn(x, y, player);
    if (pieceToRevive != null && game.GetPosition(x, y) == null)
    {
        game.Create(pieceToRevive, x, y);
        Debug.Log($"[HealingBenedictionPlate] Revived {pieceToRevive} at ({x},{y}) for {player} player");
        // Log skill usage
        if (SkillTracker.Instance != null)
        {
            SkillTracker.Instance.LogSkillUsage(player, "BISHOP", "HEALING BENEDICTION", 1);
        }
        
        // ✅ NEW: Start cooldown using CooldownManager
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.StartCooldown(player, "HealingBenediction", CooldownManager.CooldownType.OncePerBattle);
        }
        Debug.Log($"[HealingBenedictionPlate] Bishop state AFTER revive: cooldown started for {player}");

      //  game.NextTurn();
    }
    else
    {
        Debug.Log("[HealingBenedictionPlate] Tile occupied or invalid — no piece revived.");
    }

    foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
        Destroy(plate);
}

    // Player-aware piece spawning for revival
    private string GetPieceForSpawn(int x, int y, string player)
    {
        if (player == "white")
        {
            // White pieces (bottom 2 ranks: y=0,1)
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
        }
        else if (player == "black")
        {
            // Black pieces (top 2 ranks: y=6,7)
            if (y == 7)
            {
                switch (x)
                {
                    case 0: return "black_rook";
                    case 1: return "black_knight";
                    case 2: return "black_bishop";
                    case 3: return "black_queen";
                    case 4: return "black_king";
                    case 5: return "black_bishop";
                    case 6: return "black_knight";
                    case 7: return "black_rook";
                }
            }
            if (y == 6)
            {
                switch (x)
                {
                    case 0: return "black_pawn";
                    case 1: return "black_pawn1";
                    case 2: return "black_pawn2";
                    case 3: return "black_pawn3";
                    case 4: return "black_pawn4";
                    case 5: return "black_pawn5";
                    case 6: return "black_pawn6";
                    case 7: return "black_pawn7";
                }
            }
        }
        return null;
    }
}
