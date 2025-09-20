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
    Debug.Log($"[HealingBenedictionPlate] Clicked at ({x},{y})");

    Bishop bishop = FindObjectOfType<Bishop>();
    if (bishop == null)
    {
        Debug.LogError("[HealingBenedictionPlate] No Bishop found in scene!");
        return;
    }

    // ✅ NEW: Use CooldownManager instead of hasUsedHealingBenediction
    string player = bishop.GetComponent<Chessman>().GetPlayer();
    if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "HealingBenediction"))
    {
        Debug.LogWarning("[HealingBenedictionPlate] Skill already used — click ignored.");
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
        return;
    }

    string pieceToRevive = GetWhitePieceForSpawn(x, y);
    if (pieceToRevive != null && game.GetPosition(x, y) == null)
    {
        game.Create(pieceToRevive, x, y);
        Debug.Log($"[HealingBenedictionPlate] Revived {pieceToRevive} at ({x},{y})");
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

        game.NextTurn();
    }
    else
    {
        Debug.Log("[HealingBenedictionPlate] Tile occupied or invalid — no piece revived.");
    }

    foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
        Destroy(plate);
}

    private string GetWhitePieceForSpawn(int x, int y)
    {
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
        return null;
    }
}
