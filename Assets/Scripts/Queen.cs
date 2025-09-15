using UnityEngine;

public class Queen : Pieces
{
    // Cooldown for the passive (optional, e.g., 20 turns)
    private int passiveCooldownRemaining = 0;
    public GameObject movePlatePrefab;
    /// <summary>
    /// Call this at the start of the turn to decrease cooldowns
    /// </summary>
    public override void ResetTurnFlags()
    {
        if (passiveCooldownRemaining > 0)
        {
            passiveCooldownRemaining--;
            Debug.Log($"[Queen] Passive cooldown: {passiveCooldownRemaining} turns remaining."); //does not print
        }
    }

    /// <summary>
    /// Check if the Glory for the Queen passive is ready to trigger
    /// </summary>
    public bool IsPassiveAvailable()
    {
        return passiveCooldownRemaining <= 0;
    }

    /// <summary>
    /// Trigger the passive and set cooldown
    /// </summary>
    public void TriggerPassiveCooldown()
    {
        passiveCooldownRemaining = 5; // i try 5 for testing
        Debug.Log($"[Queen] Glory for the Queen triggered! Cooldown set."); 
    }

    /// <summary>
    /// Try to trigger "Glory for the Queen" passive.
    /// Returns true if a pawn was sacrificed and the queen survives; false if no pawn was available.
    /// </summary>
    
    public bool TryTriggerGloryForTheQueen()
    {
        if (!IsPassiveAvailable())
        {
            Debug.Log("[Queen Passive] Passive on cooldown — cannot trigger.");
            return false;
        }

        Chessman cm = GetComponent<Chessman>();
        if (cm == null)
        {
            Debug.LogError("[Queen Passive] No Chessman component found on Queen.");
            return false;
        }

        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[Queen Passive] Could not find Game controller.");
            return false;
        }

        string owner = cm.GetPlayer(); // "white" or "black"
        int pawnRow = owner == "white" ? 1 : 6;

        Debug.Log($"[Queen Passive] Searching for {owner} pawns at row {pawnRow}...");

        for (int x = 0; x < 8; x++)
        {
            GameObject piece = game.GetPosition(x, pawnRow);
            if (piece == null) continue;

            if (piece.name.ToLower().Contains("pawn"))
            {
                Chessman pawnCm = piece.GetComponent<Chessman>();
                if (pawnCm != null && pawnCm.GetPlayer() == owner)
                {
                    Debug.Log($"[Queen Passive] Sacrificing {piece.name} at ({x},{pawnRow}) instead of the queen!");
                    game.ClearPosition(x, pawnRow);
                    Destroy(piece);

                    TriggerPassiveCooldown(); // set cooldown
                    return true;
                }
            }
        }

        Debug.Log("[Queen Passive] No pawns available to sacrifice.");
        return false;
    }
}
