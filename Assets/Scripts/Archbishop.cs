using UnityEngine;

public class Archbishop : MonoBehaviour
{
    private Game game;
    private Chessman chessman; // ✅ Cache Chessman reference for player info
    // Removed: private static bool temporalShiftUsed - now using CooldownManager
    // Removed: public static bool eternityPierceUsed - now using CooldownManager
    // Removed: public static bool soulbindingConquestUsed - now using CooldownManager per player
    public static string capturedPieceName = ""; // Store the name of the captured piece
    public static string capturedPiecePlayer = ""; // Store which player captured the piece

    public GameObject movePlatePrefab; // Add this field

    private void Awake()
    {
        // Cache Chessman reference (following Bishop pattern)
        chessman = GetComponent<Chessman>();
        if (chessman == null)
            Debug.LogError("[Archbishop] Missing Chessman component!");
    }

    private void Start() 
    {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
    } 

    public void TemporalShiftButton()
    {
        // ✅ Get the selected archbishop from UIManager (following Queen/Bishop pattern)
        Archbishop selectedArchbishop = null;
        Chessman cm = null;
        
        if (UIManager.Instance != null && UIManager.Instance.selectedPiece != null)
        {
            GameObject selectedPiece = UIManager.Instance.selectedPiece;
            selectedArchbishop = selectedPiece.GetComponent<Archbishop>();
            cm = selectedPiece.GetComponent<Chessman>();
            
            if (selectedArchbishop == null || cm == null)
            {
                Debug.LogError($"[Temporal Shift] Selected piece {selectedPiece.name} is not an Archbishop or missing Chessman component!");
                return;
            }
        }
        else
        {
            Debug.LogError("[Temporal Shift] No piece selected via UIManager!");
            return;
        }
        
        string player = cm.GetPlayer();
        string enemyPlayer = (player == "white") ? "black" : "white";
        Debug.Log($"[Temporal Shift] Attempting activation for {player} player...");
        
        // ✅ Check cooldown BEFORE spending SP (using CooldownManager)
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "TemporalShift"))
        {
            Debug.LogWarning($"[Temporal Shift] Skill is on cooldown for {player} — cannot use.");
            return;
        }

        // ✅ Deduct SP from correct player
        if (!SkillManager.Instance.SpendPlayerSP(player, 2)) 
        {
            Debug.LogWarning($"[Temporal Shift] Not enough SP for {player} to use Temporal Shift!");
            return;
        }

        // ✅ Set cooldown using CooldownManager
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.StartCooldown(player, "TemporalShift", CooldownManager.CooldownType.OncePerBattle);
        }
        Debug.Log($"[Temporal Shift] Skill activated by {player} - restricting {enemyPlayer} player!");

        // Restrict enemy player to pawns only for 1 turn
        game.SetPlayerRestriction(enemyPlayer, 1);
        
        // Log skill usage
        if (SkillTracker.Instance != null)
        {
            SkillTracker.Instance.LogSkillUsage(player, "ARCHBISHOP", "TEMPORAL SHIFT", 2);
        }

        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        game.NextTurn();
        
        // Update visual status of all pieces on the board immediately 
        Chessman[] allPieces = FindObjectsOfType<Chessman>();
        foreach (Chessman piece in allPieces)
        {
            piece.UpdateVisualStatus();
        }
    }

    // Eternity Pierce skill
    public void TriggerEternityPierce()
    {
        // ✅ Get the selected archbishop from UIManager (following TemporalShift pattern)
        Archbishop selectedArchbishop = null;
        Chessman cm = null;
        
        if (UIManager.Instance != null && UIManager.Instance.selectedPiece != null)
        {
            GameObject selectedPiece = UIManager.Instance.selectedPiece;
            selectedArchbishop = selectedPiece.GetComponent<Archbishop>();
            cm = selectedPiece.GetComponent<Chessman>();
            
            if (selectedArchbishop == null || cm == null)
            {
                Debug.LogError($"[Eternity Pierce] Selected piece {selectedPiece.name} is not an Archbishop or missing Chessman component!");
                return;
            }
        }
        else
        {
            Debug.LogError("[Eternity Pierce] No piece selected via UIManager!");
            return;
        }
        
        string player = cm.GetPlayer();
        Debug.Log($"[Eternity Pierce] Attempting activation for {player} player...");
        
        // ✅ Check cooldown BEFORE spending SP (using CooldownManager)
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "EternityPierce"))
        {
            Debug.LogWarning($"[Eternity Pierce] Skill is on cooldown for {player} — cannot use.");
            return;
        }

        // Get game reference
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();

        // Check SP cost (minimum 1 SP) - but don't spend yet, plates will handle SP cost
        if (SkillManager.Instance.GetPlayerSP(player) < 1)
        {
            Debug.LogWarning($"[Eternity Pierce] Not enough SP for {player} (minimum 1 SP).");
            return;
        }

        // Remove existing moveplates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // Spawn Eternity Pierce plates in all 4 diagonal directions
        SpawnEternityPiercePlates(game, player);

        Debug.Log($"[Eternity Pierce] Direction selection tiles generated for {player}. Choose your firing direction.");
    }

    private void SpawnEternityPiercePlates(Game game, string player)
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
        SpawnEternityPierceDirection(game, archbishopX, archbishopY, 1, 1, player);   // NE
        SpawnEternityPierceDirection(game, archbishopX, archbishopY, 1, -1, player);  // SE
        SpawnEternityPierceDirection(game, archbishopX, archbishopY, -1, 1, player);  // NW
        SpawnEternityPierceDirection(game, archbishopX, archbishopY, -1, -1, player); // SW
    }

    private void SpawnEternityPierceDirection(Game game, int startX, int startY, int xIncrement, int yIncrement, string player)
    {
        for (int i = 1; i <= 3; i++) // Only 3 tiles per direction
        {
            int x = startX + (xIncrement * i);
            int y = startY + (yIncrement * i);

            // Check if position is on board
            if (!game.PositionOnBoard(x, y)) break;

            // Spawn the Eternity Pierce plate
            SpawnEternityPiercePlate(game, x, y, i, player); // i = distance (1st, 2nd, 3rd tile)
        }
    }

    private void SpawnEternityPiercePlate(Game game, int x, int y, int distance, string player)
    {
        // Use the same positioning as other move plates
        float fx = x * 0.57f - 1.98f;
    float fy = y * 0.56f - 1.95f;

        GameObject mp = Instantiate(movePlatePrefab, new Vector3(fx, fy, -3f), Quaternion.identity);

        // Remove default MovePlate script
        MovePlate old = mp.GetComponent<MovePlate>();
        if (old != null) Destroy(old);

        // Add EternityPiercePlate script with player info
        EternityPiercePlate plate = mp.AddComponent<EternityPiercePlate>();
        plate.Setup(game, x, y, player, distance);

        // Make eternity pierce plates visually distinct (red)
        SpriteRenderer sr = mp.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.red; // Red color for eternity pierce
        }


    }

    // Soulbinding Conquest passive skill (player-aware)
    public static void TriggerSoulbindingConquest(string capturedPiece, string player)
    {
        // ✅ Check if already used this battle using CooldownManager (player-specific)
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "SoulbindingConquest"))
        {
            Debug.LogWarning($"[Soulbinding Conquest] Already used this battle for {player} — skill blocked.");
            return;
        }

        // Check if captured piece is valid for summoning 
        if (!IsValidPieceForSummoning(capturedPiece))
        {
            Debug.Log($"[Soulbinding Conquest] {capturedPiece} is not valid for summoning.");
            return;
        }

        // Store the captured piece name, player, and mark as used
        capturedPieceName = capturedPiece;
        capturedPiecePlayer = player;
        
        // ✅ Set cooldown using CooldownManager
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.StartCooldown(player, "SoulbindingConquest", CooldownManager.CooldownType.OncePerBattle);
        }

        Debug.Log($"[Soulbinding Conquest] {player} Archbishop captured {capturedPiece} - summoning tiles will be created!");
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

        // Add SoulbindingSummonPlate script with current player info
        string currentPlayer = game.GetCurrentPlayer();
        SoulbindingSummonPlate plate = mp.AddComponent<SoulbindingSummonPlate>();
        plate.Setup(game, x, y, capturedPieceName, currentPlayer);

        // Make summon plates visually distinct (green)
        SpriteRenderer sr = mp.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.green; // Green color for summon plates
        }
    }
}