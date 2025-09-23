using UnityEngine;

public class SingularityManager : MonoBehaviour
{
    public static SingularityManager Instance { get; private set; }

    // Data for piece recreation
    private string targetPieceName;
    private string targetPlayer;
    private int chronomagusX;
    private int chronomagusY;
    private bool hasSingularityData = false;
    private int turnsUntilReappearance = 2;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetSingularityData(string pieceName, string player, int chronoX, int chronoY)
    {
        if (Instance == null)
        {
            Debug.LogError("[SingularityManager] Instance is null! Cannot set data.");
            return;
        }
        
        targetPieceName = pieceName;
        targetPlayer = player;
        chronomagusX = chronoX;
        chronomagusY = chronoY;
        hasSingularityData = true;
        turnsUntilReappearance = 4;
        
        Debug.Log($"[SingularityManager] Data stored: {targetPieceName} ({targetPlayer}) will reappear in {turnsUntilReappearance} turns");
    }

    public void OnTurnStart()
    {
        if (!hasSingularityData) return;

        turnsUntilReappearance--;
        Debug.Log($"[SingularityManager] Turns until reappearance: {turnsUntilReappearance}");

        if (turnsUntilReappearance <= 0)
        {
            // Time to recreate the pieces
            RecreateSingularityPieces();
            hasSingularityData = false;
        }
    }

    private void RecreateSingularityPieces()
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[SingularityManager] Game not found!");
            return;
        }

        Debug.Log("[SingularityManager] Creating summon plates for piece recreation...");

        // Generate summon plates on all empty tiles
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (game.GetPosition(x, y) == null)
                {
                    SpawnSingularitySummonPlate(game, x, y);
                }
            }
        }

        Debug.Log("[SingularityManager] Summon plates created! Click to place Chronomagus first, then target piece.");
    }

    private void SpawnSingularitySummonPlate(Game game, int x, int y)
    {
        float fx = x * 0.57f - 1.98f;
        float fy = y * 0.56f - 1.95f;

        GameObject mp = Instantiate(game.movePlatePrefabReference, new Vector3(fx, fy, -3f), Quaternion.identity);

        // Remove default MovePlate script
        MovePlate oldScript = mp.GetComponent<MovePlate>();
        if (oldScript != null) Destroy(oldScript);

        // Add SingularitySummonPlate script
        SingularitySummonPlate plate = mp.AddComponent<SingularitySummonPlate>();
        plate.Setup(game, x, y, targetPieceName, targetPlayer, chronomagusX, chronomagusY);

        // Make summon plates visually distinct (cyan)
        SpriteRenderer sr = mp.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.cyan;
        }
    }
}
