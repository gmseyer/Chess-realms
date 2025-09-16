using UnityEngine;

public class Queen : Pieces
{
    // Cooldown for the passive (optional, e.g., 20 turns)
    private int passiveCooldownRemaining = 0;




    // Cooldown for RegalSafeguard active
private int regalSafeguardCooldown = 0;      // current turns left
    private const int regalSafeguardCooldownMax = 20; // example: 3 turns cooldown



    public GameObject movePlatePrefab;
      private Chessman chessman;
    private StatusManager statusManager;

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

         if (regalSafeguardCooldown > 0)
    {
        regalSafeguardCooldown--;
    }
    }
     void Awake()
    {
        // Cache references
        chessman = GetComponent<Chessman>();
        if (chessman == null)
            Debug.LogError("Queen: Missing Chessman component!");

        statusManager = GetComponent<StatusManager>();
        if (statusManager == null)
            statusManager = gameObject.AddComponent<StatusManager>(); // attach at runtime if missing
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
        passiveCooldownRemaining = 20; // i try 5 for testing
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



    public void RegalSafeguard()
    {
        // Prevent use if on cooldown
        if (regalSafeguardCooldown > 0)
        {
            Debug.Log($"Regal Safeguard is on cooldown for {regalSafeguardCooldown} more turn(s).");
            return;
        }


        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("RegalSafeguard: GameController not found.");
            return;
        }

        // Turn check
        string currentPlayer = game.GetCurrentPlayer();
        if (chessman.GetPlayer() != currentPlayer)
        {
            Debug.Log($"RegalSafeguard: It's {currentPlayer}'s turn. Can't use {chessman.name}.");
            return;
        }

        // Pay SP
        // Pay SP using SkillManager
        const int safeguardCost = 1;
        bool paid = SkillManager.Instance.SpendPlayerSP(currentPlayer, safeguardCost);

        if (!paid)
        {
            Debug.Log($"{currentPlayer} does not have enough SP for Regal Safeguard (cost {safeguardCost}).");
            return;
        }

        // 🔑 Try to sacrifice a pawn
        bool pawnSacrificed = TrySacrificePawn(game, currentPlayer);

        if (!pawnSacrificed)
        {
            Debug.Log("[Queen Skill] No pawns available to sacrifice → Regal Safeguard failed.");
            return;
        }

        // ✅ Apply invulnerable status until next turn
        int expiresOnTurn = game.turns + 1;
        statusManager.AddStatus(StatusType.Invulnerable, expiresOnTurn);

        Debug.Log($"{chessman.name} activated Regal Safeguard → sacrificed a pawn and is INVULNERABLE until turn {expiresOnTurn}");

        // End turn
        //chessman.DestroyMovePlates();  // removed it because main purpose of the skill is to offensive invulnerable
        // game.NextTurn();  // so basically after getting invul, the queen can attack normally and gain invul so she is safe until next turn
        
        regalSafeguardCooldown = regalSafeguardCooldownMax;
    }

    private bool TrySacrificePawn(Game game, string player)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = game.GetPosition(x, y);
                if (piece == null) continue;

                Chessman cm = piece.GetComponent<Chessman>();
                if (cm == null) continue;

                if (cm.GetPlayer() == player && piece.name.ToLower().Contains("pawn"))
                {
                    game.ClearPosition(x, y);
                    Destroy(piece);
                    Debug.Log($"[Queen Skill] Sacrificed {piece.name} at ({x},{y}) to protect the Queen!");
                    return true;
                }
            }
        }
        return false;
    }





}
