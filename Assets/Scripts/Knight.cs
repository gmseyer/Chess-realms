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
        float fx = x * 0.66f - 2.3f;
        float fy = y * 0.66f - 2.3f;

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

}
