using System;
using UnityEngine;

public class Rook : Pieces // ✅ now inherits from Pieces instead of MonoBehaviour
{
    public GameObject movePlatePrefab; // optional, keep if you want later

    // small helper used during testing
    public void TestSkill()
    {
        Debug.Log($"[Rook] TestSkill() called on {gameObject.name}");
    }

    // Call this to attempt the Royal Castling from UI
    public void AttemptRoyalCastling()
    {
        // get Chessman data (works if Chessman is on same object or parent)
        Chessman cm = GetComponentInParent<Chessman>();
        if (cm == null)
        {
            Debug.LogError("[RoyalCastling] No Chessman component found on this object/parents!");
            return;
        }

        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[RoyalCastling] Could not find GameController.");
            return;
        }

        string currentPlayer = game.GetCurrentPlayer();
        string rookPlayer = cm.GetPlayer();

        Debug.Log($"[RoyalCastling] Attempt by '{cm.name}' (player='{rookPlayer}'), turn owner='{currentPlayer}'");

        // turn check
        if (!string.Equals(rookPlayer, currentPlayer, StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log($"[RoyalCastling] It's not {rookPlayer}'s turn!");
            return;
        }

        // once-per-battle cooldown check
        // ✅ NEW: Use CooldownManager instead of hasUsedRoyalCastling
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(rookPlayer, "RoyalCastling"))
        {
            Debug.Log("[RoyalCastling] Skill is on cooldown - cannot use this battle.");
            return;
        }
       

        // find the king first (validate preconditions before spending SP)
        Chessman kingCm = FindKing(game, rookPlayer);
        if (kingCm == null)
        {
            Debug.LogWarning($"[RoyalCastling] No king found for player '{rookPlayer}'. Aborting.");
            return;
        }

        // SP cost
        const int cost = 2;
        if (!SkillManager.Instance.SpendPlayerSP(rookPlayer, cost))
        {
            Debug.LogWarning($"[RoyalCastling] Not enough SP for {rookPlayer} (cost {cost}).");
            return;
        }

        // Save positions
        int rookX = cm.GetXBoard();
        int rookY = cm.GetYBoard();
        int kingX = kingCm.GetXBoard();
        int kingY = kingCm.GetYBoard();

        Debug.Log($"[RoyalCastling] Swapping Rook ({rookX},{rookY}) with King ({kingX},{kingY})");

        // Clear board positions first
        game.ClearPosition(rookX, rookY);
        game.ClearPosition(kingX, kingY);

        // Move rook -> king pos
        cm.SetXBoard(kingX);
        cm.SetYBoard(kingY);
        cm.SetCoords();
        game.SetPositionAt(cm.gameObject, kingX, kingY);

        // Move king -> rook pos
        kingCm.SetXBoard(rookX);
        kingCm.SetYBoard(rookY);
        kingCm.SetCoords();
        game.SetPositionAt(kingCm.gameObject, rookX, rookY);

        // Clean up any move plates if needed
        cm.DestroyMovePlates();
        kingCm.DestroyMovePlates();

        // ✅ NEW: Start cooldown using CooldownManager
        // ✅ Start cooldown using CooldownManager (with null check)
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.StartCooldown(rookPlayer, "RoyalCastling", CooldownManager.CooldownType.OncePerBattle);
        }
        Debug.Log($"[RoyalCastling] Swap complete! Rook at ({kingX},{kingY}), King at ({rookX},{rookY})");
        
        if (SkillTracker.Instance != null)
    {
        SkillTracker.Instance.LogSkillUsage(currentPlayer, cm.name, "ROYAL CASTLING", cost);
    }
        // end turn
        game.NextTurn();
    }

    // helper to locate the king of a player
    private Chessman FindKing(Game game, string player)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = game.GetPosition(x, y);
                if (piece == null) continue;

                Chessman cm = piece.GetComponent<Chessman>();
                if (cm == null) continue;

                if (piece.name.ToLower().Contains("king") && cm.GetPlayer() == player)
                    return cm;
            }
        }
        return null;
    }


 // Call this from UI button for Fortify skill

public void AttemptFortify()
{
    // ✅ ONCE PER ROOK LIMITATION CHECK
   


    // Get the selected piece from UIManager
    if (UIManager.Instance == null)
    {
        Debug.Log("FortifySelected: UIManager not found.");
        return;
    }

    GameObject selectedPiece = UIManager.Instance.selectedPiece;
    if (selectedPiece == null)
    {
        Debug.Log("FortifySelected: no piece selected.");
        return;
    }

    if (!selectedPiece.name.Contains("rook"))
    {
        Debug.Log("FortifySelected: selected piece is not a rook.");
        return;
    }

    Chessman rookCm = selectedPiece.GetComponent<Chessman>();
    if (rookCm == null)
    {
        Debug.Log("FortifySelected: selected object has no Chessman.");
        return;
    }

    Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
    if (game == null)
    {
        Debug.LogError("FortifySelected: GameController not found.");
        return;
    }

    // Make sure it's the owner's turn
    string currentPlayer = game.GetCurrentPlayer();
    if (rookCm.GetPlayer() != currentPlayer)
    {
        Debug.Log($"FortifySelected: it's {currentPlayer}'s turn. Can't use {rookCm.name}.");
        return;
    }

    const int fortifyCost = 1;

    // ✅ UPDATED: Use SkillManager instead of Game.SpendPlayerSP
    if (SkillManager.Instance == null)
    {
        Debug.LogError("FortifySelected: SkillManager not found.");
        return;
    }
    // ✅ Check if CooldownManager exists before using it
    if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(currentPlayer, "Fortify"))
    {
        Debug.Log("FortifySelected: Fortify is on co oldown - cannot use yet.");
        return;
    }

    bool paid = SkillManager.Instance.SpendPlayerSP(currentPlayer, fortifyCost);
    if (!paid)
    {
        Debug.Log($"{currentPlayer} does not have enough SP to use Fortify (cost {fortifyCost}).");
        return;
    }

     if (SkillTracker.Instance != null)
    {
        SkillTracker.Instance.LogSkillUsage(currentPlayer, rookCm.name, "FORTIFY", fortifyCost);
    }
    Debug.Log($"{currentPlayer} paid {fortifyCost} SP for Fortify. Remaining SP: {SkillManager.Instance.GetPlayerSP(currentPlayer)}");

    // APPLY EFFECT: make allied pieces around rook invulnerable
    int cx = rookCm.GetXBoard();
    int cy = rookCm.GetYBoard();

    for (int dx = -1; dx <= 1; dx++)
    {
        for (int dy = -1; dy <= 1; dy++)
        {
            if (dx == 0 && dy == 0) continue;
            int tx = cx + dx;
            int ty = cy + dy;
            if (!game.PositionOnBoard(tx, ty)) continue;

            GameObject target = game.GetPosition(tx, ty);
            if (target == null) continue;

            Chessman targetCm = target.GetComponent<Chessman>();
            if (targetCm == null) continue;

            if (targetCm.GetPlayer() == rookCm.GetPlayer())
            {
                targetCm.isInvulnerable = true;
                targetCm.invulnerableUntilTurn = game.turns + 2;
                Debug.Log($"{targetCm.name} is now invulnerable until turn {targetCm.invulnerableUntilTurn}");
            }
        }
    }

    // ✅ MARK AS USED - Once per rook limitation
   // ✅ NEW: Start cooldown using CooldownManager (2 turns)
    // ✅ Start cooldown using CooldownManager (with null check)
    if (CooldownManager.Instance != null)
    {
        CooldownManager.Instance.StartCooldown(currentPlayer, "Fortify", CooldownManager.CooldownType.TurnBased, 4);
    }
Debug.Log($"{rookCm.name} has used Fortify and will be available again in 2 turns.");

    // tidy up and end turn
    rookCm.DestroyMovePlates();
    // update the UI SP readout
    if (UIManager.Instance != null)
    {
        UIManager.Instance.UpdateSkillPointDisplay();
    }
    game.NextTurn();
}


}
