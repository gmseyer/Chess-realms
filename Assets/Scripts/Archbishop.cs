using UnityEngine;

public class Archbishop : MonoBehaviour
{
    private Game game;
    private static bool temporalShiftUsed = false; // ✅ shared flag for whole battle
    public static bool eternityPierceUsed = false; // ✅ shared flag for whole battle
    public static bool soulbindingConquestUsed = false; // ✅ shared flag for whole battle
    public static string capturedPieceName = ""; // Store the name of the captured piece

    public GameObject movePlatePrefab; // Add this field

    private void Start() 
    {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
    }

    public void TemporalShiftButton()
    {
        // ✅ Prevent multiple uses per battle
        if (temporalShiftUsed)
        {
            Debug.LogWarning("[Archbishop] Temporal Shift already used this battle!");
            return;
        }

        // ✅ Deduct SP
        if (!SkillManager.Instance.SpendPlayerSP("white", 2))
        {
            Debug.LogWarning("[Archbishop] Not enough SP to use Temporal Shift!");
            return;
        }

        temporalShiftUsed = true; // ✅ Mark as used
        Debug.Log("[Archbishop] Temporal Shift activated by white!");

        game.SetPlayerRestriction("black", 1);

        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        game.NextTurn();
    }

    // Eternity Pierce skill
    public void TriggerEternityPierce()
    {
        // Check if already used this battle
        if (eternityPierceUsed)
        {
            Debug.LogWarning("[Eternity Pierce] Already used this battle — skill blocked.");
            return;
        }

        // Get game reference (following Queen pattern)
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        
        // Get current player
        string currentPlayer = game.GetCurrentPlayer();

        // Check SP cost (minimum 1 SP)
        if (SkillManager.Instance.GetPlayerSP(currentPlayer) < 1)
        {
            Debug.LogWarning($"[Eternity Pierce] Not enough SP for {currentPlayer} (minimum 1 SP).");
            return;
        }

        // Remove existing moveplates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // Spawn Eternity Pierce plates in all 4 diagonal directions
        SpawnEternityPiercePlates(game);

        Debug.Log("[Eternity Pierce] Direction selection tiles generated. Choose your firing direction.");
    }

    private void SpawnEternityPiercePlates(Game game)
    {
        // Get Archbishop's position using UIManager pattern (following Queen/Bishop pattern)
        GameObject selectedPiece = UIManager.Instance.selectedPiece;
        if (selectedPiece == null)
        {
            Debug.LogError("[Eternity Pierce] No piece selected!");
            return;
        }

        Chessman archbishopCm = selectedPiece.GetComponent<Chessman>();
        if (archbishopCm == null)
        {
            Debug.LogError("[Eternity Pierce] No Chessman component found on selected piece!");
            return;
        }

        int archbishopX = archbishopCm.GetXBoard();
        int archbishopY = archbishopCm.GetYBoard();

        // Spawn plates in all 4 diagonal directions (3 tiles each)
        SpawnEternityPierceDirection(game, archbishopX, archbishopY, 1, 1);   // NE
        SpawnEternityPierceDirection(game, archbishopX, archbishopY, 1, -1);  // SE
        SpawnEternityPierceDirection(game, archbishopX, archbishopY, -1, 1);  // NW
        SpawnEternityPierceDirection(game, archbishopX, archbishopY, -1, -1); // SW
    }

    private void SpawnEternityPierceDirection(Game game, int startX, int startY, int xIncrement, int yIncrement)
    {
        for (int i = 1; i <= 3; i++) // Only 3 tiles per direction
        {
            int x = startX + (xIncrement * i);
            int y = startY + (yIncrement * i);

            // Check if position is on board
            if (!game.PositionOnBoard(x, y)) break;

            // Spawn the Eternity Pierce plate
            SpawnEternityPiercePlate(game, x, y, i); // i = distance (1st, 2nd, 3rd tile)
        }
    }

    private void SpawnEternityPiercePlate(Game game, int x, int y, int distance)
    {
        // Use the same positioning as other move plates
        float fx = x * 0.57f - 1.98f;
    float fy = y * 0.56f - 1.95f;

        GameObject mp = Instantiate(movePlatePrefab, new Vector3(fx, fy, -3f), Quaternion.identity);

        // Remove default MovePlate script
        MovePlate old = mp.GetComponent<MovePlate>();
        if (old != null) Destroy(old);

        // Add EternityPiercePlate script
        EternityPiercePlate plate = mp.AddComponent<EternityPiercePlate>();
        plate.Setup(game, x, y, this, distance);

        // Make eternity pierce plates visually distinct (red)
        SpriteRenderer sr = mp.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.red; // Red color for eternity pierce
        }
    }

    // Soulbinding Conquest passive skill
    public static void TriggerSoulbindingConquest(string capturedPiece)
    {
        // Check if already used this battle
        if (soulbindingConquestUsed)
        {
            Debug.LogWarning("[Soulbinding Conquest] Already used this battle — skill blocked.");
            return;
        }

        // Check if captured piece is valid for summoning
        if (!IsValidPieceForSummoning(capturedPiece))
        {
            Debug.Log($"[Soulbinding Conquest] {capturedPiece} is not valid for summoning.");
            return;
        }

        // Store the captured piece name and mark as used
        capturedPieceName = capturedPiece;
        soulbindingConquestUsed = true;

        Debug.Log($"[Soulbinding Conquest] Captured {capturedPiece} - summoning tiles will be created!");
    }

    private static bool IsValidPieceForSummoning(string pieceName)
    {
        return pieceName.Contains("pawn") || 
               pieceName.Contains("knight") || 
               pieceName.Contains("rook") || 
               pieceName.Contains("bishop");
    }

    public void SpawnSoulbindingSummonPlates()
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        string currentPlayer = game.GetCurrentPlayer();

        // Get all vacant tiles on the player's side
        Vector2Int[] playerSidePositions = GetPlayerSidePositions(currentPlayer);
        
        int platesSpawned = 0;
        foreach (Vector2Int pos in playerSidePositions)
        {
            if (game.GetPosition(pos.x, pos.y) == null)
            {
                SpawnSoulbindingPlate(game, pos.x, pos.y);
                platesSpawned++;
            }
        }

        Debug.Log($"[Soulbinding Conquest] Spawned {platesSpawned} summon plates for {capturedPieceName}.");
    }

    private Vector2Int[] GetPlayerSidePositions(string player)
    {
        if (player == "white")
        {
            return new Vector2Int[]
            {
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(3, 0),
                new Vector2Int(4, 0), new Vector2Int(5, 0), new Vector2Int(6, 0), new Vector2Int(7, 0),
                new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(3, 1),
                new Vector2Int(4, 1), new Vector2Int(5, 1), new Vector2Int(6, 1), new Vector2Int(7, 1)
            };
        }
        else
        {
            return new Vector2Int[]
            {
                new Vector2Int(0, 6), new Vector2Int(1, 6), new Vector2Int(2, 6), new Vector2Int(3, 6),
                new Vector2Int(4, 6), new Vector2Int(5, 6), new Vector2Int(6, 6), new Vector2Int(7, 6),
                new Vector2Int(0, 7), new Vector2Int(1, 7), new Vector2Int(2, 7), new Vector2Int(3, 7),
                new Vector2Int(4, 7), new Vector2Int(5, 7), new Vector2Int(6, 7), new Vector2Int(7, 7)
            };
        }
    }

    private void SpawnSoulbindingPlate(Game game, int x, int y)
    {
        float fx = x * 0.57f - 1.98f;
        float fy = y * 0.56f - 1.95f;

        GameObject mp = Instantiate(movePlatePrefab, new Vector3(fx, fy, -3f), Quaternion.identity);

        // Remove default MovePlate script
        MovePlate oldScript = mp.GetComponent<MovePlate>();
        if (oldScript != null) Destroy(oldScript);

        // Add SoulbindingSummonPlate script
        SoulbindingSummonPlate plate = mp.AddComponent<SoulbindingSummonPlate>();
        plate.Setup(game, x, y, capturedPieceName);

        // Make summon plates visually distinct (green)
        SpriteRenderer sr = mp.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.green; // Green color for summon plates
        }
    }
}