using UnityEngine;
using System.Collections.Generic;

public class Knight : MonoBehaviour
{
    [Header("Prefabs & References")]
    public GameObject movePlatePrefab; // Assign in Inspector
    public GameObject royalKnightSummonPlatePrefab; // Assign in Inspector - dedicated prefab for royal knight summoning
    private Game game;

    [Header("Skill State")]
    // Removed hasUsedPhantomCharge - now using CooldownManager

    // Runtime-selected knight
    public static Knight ActiveKnight;
    [Header("Lunar Leap")]
    public int lunarLeapSPCost = 1;        // SP cost
    public bool isLunarLeapActive = false; // Tracks if this turn is Lunar Leap
    private bool canDoubleMove = false; 

    [Header("Lunar Leap Cooldown")]
    public int lunarLeapCooldownTurns = 10;  // how many turns until skill can be used again
    // Removed nextAvailableTurn - now using CooldownManager

   // Momentum (passive teleport after capture)
[Header("Knight's Momentum Passive")]
public int momentumCooldownTurns = 15;     // cooldown length (in turns)
// Removed nextMomentumAvailableTurn - now using CooldownManager

    [Header("Trial of Valor Promotion")]
    public int valorStacks = 0;                // Current valor stacks
    public int trialOfValorSPCost = 2;         // SP cost for Trial of Valor
    public bool trialOfValorActive = false;    // Track if Trial of Valor is active

    [Header("Chivalric Guard")]
    public static Knight chivalricGuardKnight = null; // Track which knight activated ChivalricGuard

// Check if the passive is ready (public helper other scripts can call)
public bool IsMomentumReady()
{
    if (game == null) return false;
    string player = GetComponent<Chessman>().GetPlayer();
    return CooldownManager.Instance == null || !CooldownManager.Instance.IsOnCooldown(player, "KnightsMomentum");
}

    private void Awake() 
    { 
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();

        if (movePlatePrefab == null)
            movePlatePrefab = game.movePlatePrefabReference;

        if (movePlatePrefab == null)
            Debug.LogError("[Knight] MovePlate prefab not assigned!");
    }

    // Called when player selects this knight
    private void OnMouseUp()
    {
        ActiveKnight = this;
        Debug.Log($"[Knight] Selected: {name}");
    }

    // ✅ Call this when Momentum activates
private void ShowFloatingText(string message)
{
    GameObject textObj = new GameObject("FloatingText");
    textObj.transform.position = transform.position + Vector3.up * 0.5f;

    TextMesh tm = textObj.AddComponent<TextMesh>();
    tm.text = message;
    tm.fontSize = 5;
    tm.color = Color.yellow;
    tm.alignment = TextAlignment.Center;
    tm.anchor = TextAnchor.MiddleCenter;

    StartCoroutine(FloatAndDestroy(textObj));
}

// ✅ Coroutine to make text float upwards a bit before disappearing
private System.Collections.IEnumerator FloatAndDestroy(GameObject textObj)
{
    Vector3 startPos = textObj.transform.position;
    Vector3 endPos = startPos + Vector3.up * 0.5f;
    float duration = 0.5f;
    float elapsed = 0f;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        textObj.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
        yield return null;
    }

    Destroy(textObj);
}

    public bool CanDoubleMove
    {
        get { return canDoubleMove; }
        set { canDoubleMove = value; }
    }

    // Called by UI button
    public static void OnPhantomChargeButtonClicked()
    {
        if (ActiveKnight == null)
        {
            Debug.LogWarning("[PhantomCharge] No knight selected!");
            return;
        }

        ActiveKnight.DoPhantomCharge();
    }

    private void DoPhantomCharge()
    {
        Chessman cm = GetComponent<Chessman>();
        string player = cm.GetPlayer();
        
        // ✅ NEW: Use CooldownManager instead of hasUsedPhantomCharge
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "PhantomCharge"))
        {
            Debug.Log("[PhantomCharge] Skill is on cooldown - cannot use this battle.");
            return;
        }

        // Remove old move plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // Spawn PhantomCharge tiles on empty positions
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (game.GetPosition(x, y) == null)
                    SpawnPhantomPlate(x, y);
            }
        }

        Debug.Log("[PhantomCharge] PhantomCharge tiles generated.");
    }

    private void SpawnPhantomPlate(int x, int y)
    {
       float fx = x * 0.57f - 1.98f;
         float fy = y * 0.56f - 1.95f;
        GameObject mp = Instantiate(movePlatePrefab, new Vector3(fx, fy, -3f), Quaternion.identity);

        // Remove default MovePlate script
        MovePlate old = mp.GetComponent<MovePlate>();
        if (old != null) Destroy(old);

        // Add PhantomChargePlate script
        PhantomChargePlate plate = mp.AddComponent<PhantomChargePlate>();
        plate.Setup(game, x, y, this);

        // Red tint for Phantom tiles
        SpriteRenderer sr = mp.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = Color.yellow;

        mp.tag = "MovePlate";
    }

    // Called by PhantomChargePlate when a tile is clicked
    public void ExecutePhantomCharge(int targetX, int targetY)
    {
        string player = GetComponent<Chessman>().GetPlayer();
        
        // ✅ NEW: Use CooldownManager instead of hasUsedPhantomCharge
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "PhantomCharge"))
        {
            Debug.LogWarning("[PhantomCharge] Already executed this battle.");
            return;
        }

        if (!SkillManager.Instance.SpendPlayerSP(player, 1))
        {
            Debug.LogWarning($"[PhantomCharge] Not enough SP for {player}.");
            return;
        }

        // ✅ NEW: Start cooldown using CooldownManager
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.StartCooldown(player, "PhantomCharge", CooldownManager.CooldownType.OncePerBattle);
        }
        Debug.Log($"[PhantomCharge] {name} spent 1 SP for Phantom Charge. Remaining SP: {SkillManager.Instance.GetPlayerSP(player)}");

        // Log skill usage
        if (SkillTracker.Instance != null)
        {
            SkillTracker.Instance.LogSkillUsage(player, name, "PHANTOM CHARGE", 1);
        }

        Chessman cm = GetComponent<Chessman>();

        // --- Update board array manually ---
        // Remove Knight from old position
        game.ClearPosition(cm.GetXBoard(), cm.GetYBoard());

        // Set Knight at new position
        cm.SetXBoard(targetX);
        cm.SetYBoard(targetY);
        cm.SetCoords();
        game.SetPositionAt(this.gameObject, targetX, targetY);

        Debug.Log($"[PhantomCharge] {name} moved to ({targetX},{targetY})");

        // Remove all move plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // End turn
        game.NextTurn();
    }

    //START OF LUNAR LEAP
    // Call this from your UI button
    public void OnLunarLeapButtonClicked()
    {
        if (ActiveKnight == null)
        {
            Debug.LogWarning("[LunarLeap] No knight selected!");
            return;
        }

        ActiveKnight.StartLunarLeap();
    }

    private void StartLunarLeap()
    {
        Chessman cm = GetComponent<Chessman>();
        string player = cm.GetPlayer();

        // ✅ NEW: Use CooldownManager instead of manual cooldown tracking
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "LunarLeap"))
        {
            Debug.LogWarning("[LunarLeap] Skill is on cooldown - cannot use yet.");
            return;
        }

        // Check SP
        if (!SkillManager.Instance.SpendPlayerSP(player, lunarLeapSPCost))
        {
            Debug.LogWarning("[LunarLeap] Not enough SP.");
            return;
        }

        isLunarLeapActive = true;

        // Remove old moveplates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // Spawn new Lunar Leap moveplates
        cm.LunarLeapMovePlate();

        Debug.Log($"[LunarLeap] {name} activated! Knight can move using new pattern this turn.");

        // ✅ NEW: Start cooldown using CooldownManager
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.StartCooldown(player, "LunarLeap", CooldownManager.CooldownType.TurnBased, lunarLeapCooldownTurns);
        }
        Debug.Log($"[LunarLeap] Activated. Will be available again in {lunarLeapCooldownTurns} turns.");
        
        if (SkillTracker.Instance != null)
        {
            SkillTracker.Instance.LogSkillUsage(player, cm.name, "LUNAR LEAP", lunarLeapSPCost);
        }
    }

    //START OF KNIGHT'S MOMENTUM
    // Called to spawn the momentum teleport tiles (call after a capture if ready)
public void TriggerKnightsMomentum(bool ignoreCooldown = false)
{
    if (game == null)
    {
        Debug.LogError("[Momentum] Game reference missing!");
        return;
    }

    Chessman cm = GetComponent<Chessman>();
    string player = cm.GetPlayer();
    
    // ✅ NEW: Use CooldownManager instead of manual cooldown tracking
    // Allow bypassing cooldown for promotion case
    if (!ignoreCooldown && CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "KnightsMomentum"))
    {
        Debug.Log("[Momentum] Not ready - still on cooldown.");
        return;
    }

    // ✅ BOT MODE CHECK: Skip UI spawning if we're in bot mode
    BotModeManager botModeManager = FindObjectOfType<BotModeManager>();
    if (botModeManager != null && botModeManager.IsBotModeActive() && player == "black")
    {
        Debug.Log("[Momentum] Bot mode detected - skipping UI spawning, bot will handle momentum");
        return; // Let the bot handle momentum through its own system
    }

    // Remove existing moveplates
    foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
        Destroy(plate);

    // Spawn momentum plates on every empty tile (like PhantomCharge)
    for (int x = 0; x < 8; x++)
    {
        for (int y = 0; y < 4; y++)
        {
            if (game.GetPosition(x, y) == null)
                SpawnMomentumPlate(x, y);
        }
    }

    Debug.Log("[Momentum] Teleport tiles generated. Select destination.");
}

private void SpawnMomentumPlate(int x, int y, bool isForPromotion = false)
{
    float fx = x * 0.57f - 1.98f;
    float fy = y * 0.56f - 1.95f;

    GameObject mp = Instantiate(movePlatePrefab, new Vector3(fx, fy, -3f), Quaternion.identity);

    // Remove default MovePlate script
    MovePlate old = mp.GetComponent<MovePlate>();
    if (old != null) Destroy(old);

    // Add MomentumPlate script (defined below) and give it a reference to this Knight
    MomentumPlate plate = mp.AddComponent<MomentumPlate>();
    plate.Setup(game, x, y, this, isForPromotion);

    // Make momentum plates visually distinct (cyan for normal, green for promotion)
    SpriteRenderer sr = mp.GetComponent<SpriteRenderer>();
    if (sr != null)
        sr.color = isForPromotion ? Color.green : Color.cyan;

    mp.tag = "MovePlate";
}


// Called by MomentumPlate when player clicks a destination tile
public void ExecuteMomentumTeleport(int targetX, int targetY, bool startCooldown = true, bool skipNextTurn = false)
{
    // safety checks
    if (game == null)
    {
        Debug.LogError("[Momentum] Game reference missing on ExecuteMomentumTeleport.");
        return;
    }

    Chessman cm = GetComponent<Chessman>();
    if (cm == null)
    {
        Debug.LogError("[Momentum] No Chessman on this Knight!");
        return;
    }

    // Remove Knight from its old position on the board
    game.ClearPosition(cm.GetXBoard(), cm.GetYBoard());

    // Move Knight coordinates
    cm.SetXBoard(targetX);
    cm.SetYBoard(targetY);
    cm.SetCoords();

    // Place Knight on new position
    game.SetPositionAt(this.gameObject, targetX, targetY);

    Debug.Log($"[Momentum] {name} teleported to ({targetX},{targetY})");
    ShowFloatingText("MOVE+");

    // Log skill usage
    if (SkillTracker.Instance != null)
    {
        SkillTracker.Instance.LogSkillUsage(ActiveKnight.GetComponent<Chessman>().GetPlayer(), name, "KNIGHTS MOMENTUM", 0);
    }

    // ✅ NEW: Start cooldown using CooldownManager (only if not used for promotion)
    if (startCooldown && CooldownManager.Instance != null)
    {
        CooldownManager.Instance.StartCooldown(ActiveKnight.GetComponent<Chessman>().GetPlayer(), "KnightsMomentum", CooldownManager.CooldownType.TurnBased, momentumCooldownTurns);
        Debug.Log($"[Momentum] Next available in {momentumCooldownTurns} turns");
    }
    else if (!startCooldown)
    {
        Debug.Log("[Momentum] Promotion teleport - no cooldown started");
    }

    // Remove all moveplates (cleanup)
    foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
        Destroy(plate);

    // End turn (unless bot is handling it)
    if (!skipNextTurn)
    {
        game.NextTurn();
    }
    else
    {
        Debug.Log("[Momentum] Bot mode - skipping NextTurn(), bot will handle turn flow");
    }
}

// Trial of Valor - Knight promotion system
public void TrialOfValor()
{
    if (game == null)
    {
        Debug.LogError("[TrialOfValor] Missing Game reference!");
        return;
    }

    // Get the selected piece (current knight) - try multiple methods like Pawn skills
    Chessman selectedPiece = GetComponent<Chessman>();
    if (selectedPiece == null)
    {
        // Try to get from UIManager selected piece
        if (UIManager.Instance != null && UIManager.Instance.selectedPiece != null)
        {
            selectedPiece = UIManager.Instance.selectedPiece.GetComponent<Chessman>();
        }
    }
    
    if (selectedPiece == null)
    {
        // Try to find any active knight of the current player
        string currentPlayer = game.GetCurrentPlayer();
        Knight[] allKnights = FindObjectsOfType<Knight>();
        foreach (Knight knight in allKnights)
        {
            Chessman knightCm = knight.GetComponent<Chessman>();
            if (knightCm != null && knightCm.GetPlayer() == currentPlayer)
            {
                selectedPiece = knightCm;
                break;
            }
        }
    }
    
    if (selectedPiece == null)
    {
        Debug.LogError("[TrialOfValor] Could not find Chessman reference!");
        return;
    }

    string player = selectedPiece.GetPlayer();
    
    // Check SP cost
    if (!SkillManager.Instance.SpendPlayerSP(player, trialOfValorSPCost))
    {
        Debug.LogWarning($"[TrialOfValor] Not enough SP to use Trial of Valor. Need {trialOfValorSPCost} SP.");
        return;
    }
    
    // Get the Knight component from the selected piece
    Knight knightComponent = selectedPiece.GetComponent<Knight>();
    if (knightComponent == null)
    {
        Debug.LogError("[TrialOfValor] Selected piece is not a Knight!");
        return;
    }
    
    // Set valor stacks to 0 and activate Trial of Valor
    knightComponent.valorStacks = 0;
    knightComponent.trialOfValorActive = true;
    
    Debug.Log($"[TrialOfValor] {player} knight started Trial of Valor. Valor stacks: {knightComponent.valorStacks}/3");
}

// Called when this knight captures an enemy (add valor stack)
public void OnCaptureEnemy()
{
    if (trialOfValorActive && valorStacks < 3)
    {
        valorStacks++;
        Debug.Log($"[TrialOfValor] Knight gained valor stack from capture! Current: {valorStacks}/3");
        
        // Check if ready for promotion
        if (valorStacks == 3)
        {
            Debug.Log($"[TrialOfValor] Knight has 3 valor stacks! Ready for promotion to Royal Knight!");
            PromoteToRoyalKnight();
        }
    }
}

// Add valor charge from various sources (capture, check, adjacent ally death)
public void AddValorCharge(string source)
{
    if (trialOfValorActive && valorStacks < 3)
    {
        valorStacks++;
        Debug.Log($"[TrialOfValor] Knight gained valor charge from {source}! Current: {valorStacks}/3");
        
        // Check if ready for promotion
        if (valorStacks == 3)
        {
            Debug.Log($"[TrialOfValor] Knight has 3 valor charges! Ready for promotion to Royal Knight!");
            PromoteToRoyalKnight();
        }
    }
}

// Promote knight to royal knight using Bishop.cs summoning pattern
private void PromoteToRoyalKnight()
{
    Chessman selectedPiece = GetComponent<Chessman>();
    if (selectedPiece == null || game == null)
    {
        Debug.LogError("[TrialOfValor] Missing references for promotion!");
        return;
    }

    string player = selectedPiece.GetPlayer();
    int x = selectedPiece.GetXBoard();
    int y = selectedPiece.GetYBoard();
    
    Debug.Log($"[TrialOfValor] {player} knight has 3 valor stacks! Destroying knight and spawning summon plates");
    
    // Clear the current knight's position
    game.SetPositionEmpty(x, y);
    
    // Destroy the current knight
    Destroy(gameObject);
    
    // Spawn royal knight summon plates on ALL empty tiles (8x8 board)
    string royalKnightName = (player == "white") ? "white_royal_knight" : "black_royal_knight";
    int platesCreated = 0;
    
    for (int tileX = 0; tileX < 8; tileX++)
    {
        for (int tileY = 0; tileY < 8; tileY++) 
        {
            if (game.GetPosition(tileX, tileY) == null)
            {
                SpawnRoyalKnightSummonPlateStatic(game, tileX, tileY, royalKnightName);
                platesCreated++;
                Debug.Log($"[TrialOfValor] Spawning ROYAL KNIGHT plate at ({tileX},{tileY}) - Total: {platesCreated}");
            }
        }
    }
    
    Debug.Log($"[TrialOfValor] Created {platesCreated} summon plates total");
}

// Spawn summon plate for royal knight (using Bishop.cs SpawnTile pattern)
private void SpawnRoyalKnightSummonPlate(Game game, int x, int y, string pieceName)
{
    // Use dedicated royal knight summon plate prefab if available
    GameObject prefabToUse = royalKnightSummonPlatePrefab;
    if (prefabToUse == null)
    {
        prefabToUse = movePlatePrefab;
    }
    if (prefabToUse == null)
    {
        prefabToUse = game.movePlatePrefabReference;
    }
    
    if (prefabToUse == null)
    {
        Debug.LogError($"[TrialOfValor] ERROR: No prefab available for {pieceName} at ({x},{y})!");
        return;
    }
    
    float fx = x * 0.57f - 1.98f;
    float fy = y * 0.56f - 1.95f;
    GameObject mp = Instantiate(prefabToUse, new Vector3(fx, fy, -3f), Quaternion.identity);

    Debug.Log($"[TrialOfValor] INSTANTIATED GameObject for {pieceName} at ({x},{y}) - Name: {mp.name}, Active: {mp.activeInHierarchy}");

    MovePlate oldScript = mp.GetComponent<MovePlate>();
    if (oldScript != null) 
    {
        Debug.Log($"[TrialOfValor] Removing default MovePlate script from {mp.name}");
        Destroy(oldScript); 
    }

    mp.AddComponent<RoyalKnightSummonPlate>().Setup(game, x, y, pieceName);
    Debug.Log($"[TrialOfValor] Added RoyalKnightSummonPlate component to {mp.name}");
    
    // Make the plate visible with a distinct color
    mp.tag = "MovePlate";
    SpriteRenderer sr = mp.GetComponent<SpriteRenderer>();
    if (sr != null)
    {
        sr.color = Color.green; // Green for royal knight summon plates
        Debug.Log($"[TrialOfValor] Set green color for summon plate at ({x},{y})");
    }
    else
    {
        Debug.LogError($"[TrialOfValor] No SpriteRenderer found on summon plate at ({x},{y})!");
    }
    
    Debug.Log($"[TrialOfValor] FINAL: Created summon plate for {pieceName} at ({x},{y}) - Name: {mp.name}, Position: {mp.transform.position}, Active: {mp.activeInHierarchy}");
    
    // Start a coroutine to check if the plate gets destroyed
    game.StartCoroutine(CheckPlateSurvival(mp, x, y, pieceName));
}

// Static version for use during promotion (doesn't need Knight component)
public static void SpawnRoyalKnightSummonPlateStatic(Game game, int x, int y, string pieceName)
{
    // Try to find a Knight component to get the dedicated prefab
    Knight[] allKnights = FindObjectsOfType<Knight>();
    GameObject prefabToUse = null;
    
    foreach (Knight knight in allKnights)
    {
        if (knight.royalKnightSummonPlatePrefab != null)
        {
            prefabToUse = knight.royalKnightSummonPlatePrefab;
            break;
        }
    }
    
    // Fallback to regular moveplate prefab if dedicated prefab not found
    if (prefabToUse == null)
    {
        prefabToUse = game.movePlatePrefabReference;
        Debug.LogWarning($"[TrialOfValor] Using fallback moveplate prefab for {pieceName} at ({x},{y})");
    }
    
    if (prefabToUse == null)
    {
        Debug.LogError($"[TrialOfValor] ERROR: No prefab available for {pieceName} at ({x},{y})!");
        return;
    }
    
    float fx = x * 0.57f - 1.98f;
    float fy = y * 0.56f - 1.95f;
    GameObject mp = Instantiate(prefabToUse, new Vector3(fx, fy, -3f), Quaternion.identity);


    MovePlate oldScript = mp.GetComponent<MovePlate>();
    if (oldScript != null) 
    {
        Debug.Log($"[TrialOfValor] STATIC: Removing default MovePlate script from {mp.name}");
        Destroy(oldScript);
    }

    mp.AddComponent<RoyalKnightSummonPlate>().Setup(game, x, y, pieceName);
    Debug.Log($"[TrialOfValor] STATIC: Added RoyalKnightSummonPlate component to {mp.name}");
    
    // Make the plate visible with a distinct color
    mp.tag = "MovePlate";
    SpriteRenderer sr = mp.GetComponent<SpriteRenderer>();
   
        
    // Start a coroutine to check if the plate gets destroyed
    game.StartCoroutine(CheckPlateSurvivalStatic(mp, x, y, pieceName));
}

// Coroutine to check if a summon plate survives
private System.Collections.IEnumerator CheckPlateSurvival(GameObject plate, int x, int y, string pieceName)
{
    yield return new WaitForSeconds(0.1f); // Wait 0.1 seconds
    
    yield return new WaitForSeconds(1.0f); // Wait another 1 second
    
}

// Static version of the coroutine
public static System.Collections.IEnumerator CheckPlateSurvivalStatic(GameObject plate, int x, int y, string pieceName)
{
    yield return new WaitForSeconds(0.1f); // Wait 0.1 seconds



    yield return new WaitForSeconds(1.0f); // Wait another 1 second


}

    /// <summary>
    /// Chivalric Guard - Apply Guard status to allied pieces in 5x5 area
    /// </summary>
    public void ChivalricGuard()
    {
        // Get the knight that this skill is being used on - try multiple methods like TrialOfValor
        Chessman selectedPiece = GetComponent<Chessman>();
        if (selectedPiece == null)
        {
            // Try to get from UIManager selected piece
            if (UIManager.Instance != null && UIManager.Instance.selectedPiece != null)
            {
                selectedPiece = UIManager.Instance.selectedPiece.GetComponent<Chessman>();
            }
        }

        if (selectedPiece == null)
        {
            // Try to find any active knight of the current player
            string currentPlayer = game.GetCurrentPlayer();
            Knight[] knightsOnBoard = FindObjectsOfType<Knight>();
            foreach (Knight knight in knightsOnBoard)
            {
                Chessman knightCm = knight.GetComponent<Chessman>();
                if (knightCm != null && knightCm.GetPlayer() == currentPlayer)
                {
                    selectedPiece = knightCm;
                    break;
                }
            }
        }

        if (selectedPiece == null)
        {
            Debug.LogError("[ChivalricGuard] Could not find Chessman reference!");
            return;
        }

        string player = selectedPiece.GetPlayer();

        // Check SP cost (1 SP)
        if (SkillManager.Instance.GetPlayerSP(player) < 1)
        {
            Debug.Log("[ChivalricGuard] Not enough SP!");
            return;
        }

        // Check if Chivalric Guard is on cooldown (10 turns)
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "ChivalricGuard"))
        {
            Debug.Log("[ChivalricGuard] Skill is on cooldown!");
            return;
        }

        // Deduct SP
        SkillManager.Instance.SpendPlayerSP(player, 1);

        // Start Chivalric Guard cooldown (10 turns)
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.StartCooldown(player, "ChivalricGuard", CooldownManager.CooldownType.TurnBased, 10);
        }

        // Get knight's position and player
        int knightX = selectedPiece.GetXBoard();
        int knightY = selectedPiece.GetYBoard();
        string knightPlayer = selectedPiece.GetPlayer();

        // Remove all existing move plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // Find the Knight instance that corresponds to the selected piece
        Knight knightInstance = null;
        Knight[] allKnightComponents = FindObjectsOfType<Knight>();
        foreach (Knight knight in allKnightComponents)
        {
            Chessman knightCm = knight.GetComponent<Chessman>();
            if (knightCm != null &&
                knightCm.GetXBoard() == knightX &&
                knightCm.GetYBoard() == knightY &&
                knightCm.GetPlayer() == knightPlayer)
            {
                knightInstance = knight;
                break;
            }
        }

        if (knightInstance == null)
        {
            Debug.LogError("[ChivalricGuard] Could not find Knight instance for the selected piece!");
            return;
        }

        // Set this knight as the one who activated ChivalricGuard
        chivalricGuardKnight = knightInstance;

        // Create 5x5 selection area centered on knight
        CreateChivalricGuardSelectionPlates(knightX, knightY, knightPlayer, knightInstance);

        Debug.Log($"[ChivalricGuard] 🛡️ CHIVALRIC GUARD ACTIVATED by {knightInstance.name}! Select allied piece to guard...");
    }


    /// <summary>
    /// Create 5x5 selection area centered on knight for Chivalric Guard
    /// </summary>
    private static void CreateChivalricGuardSelectionPlates(int centerX, int centerY, string knightPlayer, Knight knightInstance)
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[ChivalricGuard] Could not find Game component for selection plates!");
            return;
        }

        int selectionPlateCount = 0;

        // Create 5x5 area centered on knight
        for (int dx = -2; dx <= 2; dx++)
        {
            for (int dy = -2; dy <= 2; dy++)
            {
                int x = centerX + dx;
                int y = centerY + dy;

                // Check if position is within board bounds
                if (x >= 0 && x < 8 && y >= 0 && y < 8)
                {
                    GameObject piece = game.GetPosition(x, y);
                    if (piece != null)
                    {
                        Chessman chessman = piece.GetComponent<Chessman>();
                        if (chessman != null)
                        {
                            string pieceName = chessman.name;

                            // Check if this is an allied piece (same player, not knight itself, not other knights)
                            if (chessman.GetPlayer() == knightPlayer &&
                                !pieceName.Contains("knight"))
                            {
                                CreateChivalricGuardSelectionPlate(x, y, knightInstance);
                                selectionPlateCount++;
                            }
                        }
                    }
                }
            }
        }

        Debug.Log($"[ChivalricGuard] Created {selectionPlateCount} selection plates for allied pieces");
    }

    /// <summary>
    /// Find an active knight piece on the board (for skill activation)
    /// </summary>
    private static Chessman FindActiveKnightPiece()
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[ChivalricGuard] Could not find Game component!");
            return null;
        }

        // First try to find a knight that matches the current player's turn
        string currentPlayer = game.GetCurrentPlayer();

        // Search for Knight on the board (white_knight or black_knight)
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = game.GetPosition(x, y);
                if (piece != null && (piece.name == "white_knight" || piece.name == "black_knight"))
                {
                    Chessman chessman = piece.GetComponent<Chessman>();
                    if (chessman != null && chessman.GetPlayer() == currentPlayer)
                    {
                        Debug.Log($"[ChivalricGuard] Found active Knight at ({x},{y}) for player {currentPlayer}");
                        return chessman;
                    }
                }
            }
        }

        // If no knight found for current player, try to find any knight
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = game.GetPosition(x, y);
                if (piece != null && (piece.name == "white_knight" || piece.name == "black_knight"))
                {
                    Chessman chessman = piece.GetComponent<Chessman>();
                    if (chessman != null)
                    {
                        Debug.Log($"[ChivalricGuard] Found Knight at ({x},{y}) as fallback");
                        return chessman;
                    }
                }
            }
        }

        Debug.LogError("[ChivalricGuard] No Knight found on board!");
        return null;
    }

    /// <summary>
    /// Create a single selection plate for Chivalric Guard targeting
    /// </summary>
    private static void CreateChivalricGuardSelectionPlate(int x, int y, Knight knightInstance)
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null) return;

        float fx = x * 0.57f - 1.98f;
        float fy = y * 0.56f - 1.95f;

        GameObject plate = Instantiate(game.movePlatePrefabReference);
        plate.transform.position = new Vector3(fx, fy, -3f);

        // Add ChivalricGuardSelectionPlate component
        ChivalricGuardSelectionPlate guardPlate = plate.AddComponent<ChivalricGuardSelectionPlate>();
        guardPlate.Setup(x, y, knightInstance);

        // Set visual appearance (blue for guard/protection theme)
        plate.GetComponent<SpriteRenderer>().color = new Color(0f, 0.5f, 1f, 0.75f);

        Debug.Log($"[ChivalricGuard] Created selection plate at ({x},{y})");
    }

    /// <summary>
    /// Apply Guard status to target piece (instance method)
    /// </summary>
    public void ApplyGuardStatusToPiece(Chessman targetChessman)
    {
        if (targetChessman == null)
        {
            Debug.LogError("[ChivalricGuard] Target piece is null!");
            return;
        }

        // Get the knight's player for SP deduction and cooldown
        string knightPlayer = GetComponent<Chessman>()?.GetPlayer();
        if (knightPlayer == null)
        {
            Debug.LogError("[ChivalricGuard] Could not determine knight's player!");
            return;
        }

        // Get current turn from Game component
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[ChivalricGuard] Could not find Game component for turn tracking!");
            return;
        }

        StatusManager targetStatus = targetChessman.GetComponent<StatusManager>();
        if (targetStatus != null)
        {
            // Apply Guard status for 3 turns
            targetStatus.AddStatus(StatusType.Guard, game.turns + 3);

            Debug.Log($"[ChivalricGuard] 🛡️ GUARD status applied to {targetChessman.name} for 3 turns!");
        }
        else
        {
            Debug.LogError($"[ChivalricGuard] Could not find StatusManager on target piece!");
        }
    }

    /// <summary>
    /// Apply Guard status to target piece (static method for backward compatibility)
    /// </summary>
    public static void ApplyGuardStatus(Chessman targetChessman)
    {
        // Find any active knight and use it
        Knight activeKnight = FindActiveKnightInstance();
        if (activeKnight != null)
        {
            activeKnight.ApplyGuardStatusToPiece(targetChessman);
        }
        else
        {
            Debug.LogError("[ChivalricGuard] No active knight found!");
        }
    }

    /// <summary>
    /// Find an active knight instance on the board
    /// </summary>
    private static Knight FindActiveKnightInstance()
    {
        Knight[] knightComponents = FindObjectsOfType<Knight>();
        if (knightComponents.Length > 0)
        {
            return knightComponents[0]; // Return first available knight
        }
        return null;
    }

    /// <summary>
    /// Clear the ChivalricGuard knight reference (call when skill is cancelled or game resets)
    /// </summary>
    public static void ClearChivalricGuardReference()
    {
        chivalricGuardKnight = null;
        Debug.Log("[ChivalricGuard] Knight reference cleared.");
    }
}