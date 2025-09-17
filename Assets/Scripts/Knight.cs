using UnityEngine;

public class Knight : MonoBehaviour
{
    [Header("Prefabs & References")]
    public GameObject movePlatePrefab; // Assign in Inspector
    private Game game;

    [Header("Skill State")]
    public bool hasUsedPhantomCharge = false;

    // Runtime-selected knight
    public static Knight ActiveKnight;
    [Header("Lunar Leap")]
    public int lunarLeapSPCost = 1;        // SP cost
    public bool isLunarLeapActive = false; // Tracks if this turn is Lunar Leap
    private bool canDoubleMove = false;

    [Header("Lunar Leap Cooldown")]
    public int lunarLeapCooldownTurns = 10;  // how many turns until skill can be used again
    private int nextAvailableTurn = 0;       // the turn when the skill can next be used

   // Momentum (passive teleport after capture)
[Header("Knight's Momentum Passive")]
public int momentumCooldownTurns = 15;     // cooldown length (in turns)
private int nextMomentumAvailableTurn = 0; // turn when momentum is next available

// Check if the passive is ready (public helper other scripts can call)
public bool IsMomentumReady()
{
    return game != null && game.GetTurnCount() >= nextMomentumAvailableTurn;
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
        if (hasUsedPhantomCharge)
        {
            Debug.Log("[PhantomCharge] Skill already used this battle.");
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
        if (hasUsedPhantomCharge)
        {
            Debug.LogWarning("[PhantomCharge] Already executed this battle.");
            return;
        }

        string player = GetComponent<Chessman>().GetPlayer();
        if (!SkillManager.Instance.SpendPlayerSP(player, 1))
        {
            Debug.LogWarning($"[PhantomCharge] Not enough SP for {player}.");
            return;
        }

        hasUsedPhantomCharge = true;
        Debug.Log($"[PhantomCharge] {name} spent 1 SP for Phantom Charge. Remaining SP: {SkillManager.Instance.GetPlayerSP(player)}");

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

        int currentTurn = game.GetTurnCount();  // assuming Game has a turn counter
        if (currentTurn < nextAvailableTurn)
        {
            Debug.LogWarning($"[LunarLeap] Skill on cooldown. Available on turn {nextAvailableTurn}.");
            return; // exit without activating Lunar Leap
        }


        Chessman cm = GetComponent<Chessman>();
        string player = cm.GetPlayer();

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

        nextAvailableTurn = currentTurn + lunarLeapCooldownTurns;
        Debug.Log($"[LunarLeap] Activated. Next available on turn {nextAvailableTurn}.");

    }





    //START OF KNIGHT'S MOMENTUM
    // Called to spawn the momentum teleport tiles (call after a capture if ready)
public void TriggerKnightsMomentum()
{
    if (game == null)
    {
        Debug.LogError("[Momentum] Game reference missing!");
        return;
    }

    int currentTurn = game.GetTurnCount();
    if (currentTurn < nextMomentumAvailableTurn)
    {
        Debug.Log($"[Momentum] Not ready until turn {nextMomentumAvailableTurn}.");
        return;
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

private void SpawnMomentumPlate(int x, int y)
{
    float fx = x * 0.57f - 1.98f;
    float fy = y * 0.56f - 1.95f;

    GameObject mp = Instantiate(movePlatePrefab, new Vector3(fx, fy, -3f), Quaternion.identity);

    // Remove default MovePlate script
    MovePlate old = mp.GetComponent<MovePlate>();
    if (old != null) Destroy(old);

    // Add MomentumPlate script (defined below) and give it a reference to this Knight
    MomentumPlate plate = mp.AddComponent<MomentumPlate>();
    plate.Setup(game, x, y, this);

    // Make momentum plates visually distinct (cyan)
    SpriteRenderer sr = mp.GetComponent<SpriteRenderer>();
    if (sr != null)
        sr.color = Color.cyan;

    mp.tag = "MovePlate";
}

// Called by MomentumPlate when player clicks a destination tile
public void ExecuteMomentumTeleport(int targetX, int targetY)
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


    // Start cooldown: next available turn = current turn + cooldown length
        nextMomentumAvailableTurn = game.GetTurnCount() + momentumCooldownTurns;
    Debug.Log($"[Momentum] Next available on turn {nextMomentumAvailableTurn}");

    // Remove all moveplates (cleanup)
    foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
        Destroy(plate);

    // End turn
    game.NextTurn();
}



}
