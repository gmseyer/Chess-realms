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

    [Header("Enchanting Influence")]
    public bool hasUsedEnchantingInfluence = false;
   



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
                    // In Rook.cs, in AttemptFortify() method:
if (SkillTracker.Instance != null)
{
    SkillTracker.Instance.LogSkillUsage(owner, "QUEEN", "GLORY FOR THE QUEEN", 0);
}
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
        // In Rook.cs, in AttemptFortify() method:
if (SkillTracker.Instance != null)
{
    SkillTracker.Instance.LogSkillUsage(currentPlayer, "QUEEN", "REGAL SAFEGUARD", 1);
}
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

    // start of Enchanting Influence

        // Add this before the closing brace of the Queen class
// Enchanting Influence skill - Step 1: Generate tiles around the board
public void TriggerEnchantingInfluence()
{
    // Check if already used this battle
    if (hasUsedEnchantingInfluence)
    {
        Debug.LogWarning("[Enchanting Influence] Already used this battle — skill blocked.");
        return;
    }

    // Get game reference
    Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
    
    // Get current player
    string currentPlayer = game.GetCurrentPlayer();
    
    // Check SP cost (1 SP) - but don't deduct yet, just check if we can afford it
    const int enchantingInfluenceCost = 1;
    if (SkillManager.Instance.GetPlayerSP(currentPlayer) < enchantingInfluenceCost)
    {
        Debug.LogWarning($"[Enchanting Influence] Not enough SP for {currentPlayer} (cost {enchantingInfluenceCost}).");
        return;
    }

    // Remove existing moveplates
    foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
        Destroy(plate);

    // Spawn enchanting influence plates on every tile on the board
    for (int x = 0; x < 8; x++)
    {
        for (int y = 0; y < 8; y++)
        {
            SpawnEnchantingInfluencePlate(x, y, game);
        }
    }

    Debug.Log("[Enchanting Influence] Selection tiles generated. Select an enemy piece.");
}


private void SpawnEnchantingInfluencePlate(int x, int y, Game game)
{
    // Use the same positioning as other move plates
   float fx = x * 0.57f - 1.98f;
    float fy = y * 0.56f - 1.95f;

    GameObject mp = Instantiate(movePlatePrefab, new Vector3(fx, fy, -3f), Quaternion.identity);

    // Remove default MovePlate script
    MovePlate old = mp.GetComponent<MovePlate>();
    if (old != null) Destroy(old);

    // Add EnchantingInfluencePlate script
    EnchantingInfluencePlate plate = mp.AddComponent<EnchantingInfluencePlate>();
    plate.Setup(game, x, y, this);

    // Make enchanting influence plates visually distinct (purple)
    SpriteRenderer sr = mp.GetComponent<SpriteRenderer>();
    if (sr != null)
    {
        sr.color = Color.magenta; // Purple color for enchanting influence
    }
}


// Call this from UI button for Regal Safeguard skill
public void AttemptRegalSafeguard()
{
    // Get the selected piece from UIManager
    if (UIManager.Instance == null)
    {
        Debug.Log("RegalSafeguardSelected: UIManager not found.");
        return;
    }

    GameObject selectedPiece = UIManager.Instance.selectedPiece;
    if (selectedPiece == null)
    {
        Debug.Log("RegalSafeguardSelected: no piece selected.");
        return;
    }

    if (!selectedPiece.name.Contains("queen"))
    {
        Debug.Log("RegalSafeguardSelected: selected piece is not a queen.");
        return;
    }

    Queen queenScript = selectedPiece.GetComponent<Queen>();
    if (queenScript == null)
    {
        Debug.LogError("RegalSafeguardSelected: selected piece has no Queen script.");
        return;
    }

    queenScript.RegalSafeguard();

    // update SP display
    if (UIManager.Instance != null)
    {
        UIManager.Instance.UpdateSkillPointDisplay();
    }
}

}
