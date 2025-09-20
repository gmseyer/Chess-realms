using UnityEngine;

public class Bishop : Pieces
{
    public GameObject movePlatePrefab; 
    public GameObject elementalSummonPlatePrefab; 
    public GameObject archbishopSummonPlatePrefab; 
    [Header("Prefabs (Auto-Loaded)")]
    // Removed hasUsedHealingBenediction and hasUsedCelestialSummon - now using CooldownManager
    
    // Cache Chessman reference like Queen does
    private Chessman chessman;
 
    private void Awake()
    {
        // Cache Chessman reference (following Queen pattern)
        chessman = GetComponent<Chessman>();
        if (chessman == null)
            Debug.LogError("[Bishop] Missing Chessman component!");
            
        // Auto-load if not assigned in Inspector
        if (elementalSummonPlatePrefab == null)
            elementalSummonPlatePrefab = Resources.Load<GameObject>("Prefabs/ElementalSummonPlate");

        if (archbishopSummonPlatePrefab == null)
            archbishopSummonPlatePrefab = Resources.Load<GameObject>("Prefabs/ArchbishopSummonPlate");

        if (elementalSummonPlatePrefab == null)
            Debug.LogError("[Bishop] Could not load ElementalSummonPlate from Resources!");

        if (archbishopSummonPlatePrefab == null)
            Debug.LogError("[Bishop] Could not load ArchbishopSummonPlate from Resources!");
    }


    //*******************start divine offering*******************
    public void OnBishopButtonClick() //responsible for summoning that is called upon bishops death
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        
        // ✅ Safety check for chessman reference
        if (chessman == null)
        {
            chessman = GetComponent<Chessman>();
            if (chessman == null)
            {
                Debug.LogError("[DivineOffering] No Chessman component found!");
                return;
            }
        }
        
        // ✅ NEW: Use CooldownManager for twice-per-battle cooldown
        string player = chessman.GetPlayer();
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "DivineOffering"))
        {
            Debug.Log("[DivineOffering] Skill is on cooldown - cannot use this battle.");
            return;
        }

        if (elementalSummonPlatePrefab == null)
            Debug.LogError("[Bishop] Elemental Summon Plate Prefab is NOT assigned!");
        if (archbishopSummonPlatePrefab == null)
            Debug.LogError("[Bishop] Archbishop Summon Plate Prefab is NOT assigned!");



        // ✅ Destroy existing plates first
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // ✅ Spawn ELEMENTAL BISHOP plates (bottom 3-4 ranks)
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                if (game.GetPosition(x, y) == null)
                {
                    SpawnTile(game, x, y, elementalSummonPlatePrefab, "white_elemental_bishop");
                    Debug.Log($"[DivineOffering] Spawning ELEMENTAL bishop plate at ({x},{y})");
                }
            }
        }

        // ✅ Spawn ARCHBISHOP plates (bottom 1-2 ranks)
        for (int x = 0; x < 8; x++)
        {
            for (int y = 2; y < 4; y++)
            {
                if (game.GetPosition(x, y) == null)
                {
                    SpawnTile(game, x, y, archbishopSummonPlatePrefab, "white_arch_bishop");
                    Debug.Log($"[DivineOffering] Spawning ARCHBISHOP plate at ({x},{y})");
                }
            }
        }
        
        // ✅ NEW: Initialize or consume use for Divine Offering (2 uses per battle)
        if (CooldownManager.Instance != null)
        {
            // If not initialized yet, set it up for 2 uses per battle
            if (!CooldownManager.Instance.IsOnCooldown(player, "DivineOffering"))
            {
                CooldownManager.Instance.StartCooldown(player, "DivineOffering", CooldownManager.CooldownType.UsesPerBattle, 2);
            }
            // Consume one use
            CooldownManager.Instance.ConsumeUse(player, "DivineOffering");
        }
        Debug.Log("[DivineOffering] Skill activated - one use consumed.");
    }


     private void SpawnTile(Game game, int x, int y, GameObject prefab, string pieceName)
    {

            if (prefab == null)
    {
        Debug.LogError($"[Bishop] ERROR: Prefab is NULL for {pieceName} at ({x},{y})!");
        return;
    }
        float fx = x * 0.57f - 1.98f;
        float fy = y * 0.56f - 1.95f;

        GameObject mp = Instantiate(prefab, new Vector3(fx, fy, -3f), Quaternion.identity);

        MovePlate oldScript = mp.GetComponent<MovePlate>();
        if (oldScript != null) Destroy(oldScript);

        mp.AddComponent<EndTurnPlate>().Setup(game, x, y, pieceName);
    }

     public void HealingBenediction()
    {
        // ✅ Safety check for chessman reference
        if (chessman == null)
        {
            chessman = GetComponent<Chessman>();
            if (chessman == null)
            {
                Debug.LogError("[HealingBenediction] No Chessman component found!");
                return;
            }
        }
        
        string player = chessman.GetPlayer();
        Debug.Log($"[HealingBenediction] Attempting activation for {player}...");

        // ✅ NEW: Use CooldownManager instead of hasUsedHealingBenediction
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "HealingBenediction"))
        {
            Debug.Log("[HealingBenediction] Already used — skill blocked.");
            return;
        }

        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();

        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        Vector2Int[] whiteStartPositions = new Vector2Int[]
        {
            new Vector2Int(0, 0), new Vector2Int(7, 0),
            new Vector2Int(1, 0), new Vector2Int(6, 0),
            new Vector2Int(0, 1), new Vector2Int(1, 1),
            new Vector2Int(2, 1), new Vector2Int(3, 1),
            new Vector2Int(4, 1), new Vector2Int(5, 1),
            new Vector2Int(6, 1), new Vector2Int(7, 1)
        };
    
        int platesSpawned = 0;
        foreach (Vector2Int pos in whiteStartPositions)
        {
            if (game.GetPosition(pos.x, pos.y) == null)
            {
                SpawnHealingPlate(game, pos.x, pos.y);
                platesSpawned++;
            }
        }

        Debug.Log($"[HealingBenediction] Spawned {platesSpawned} revival plates.");
        
        // ✅ NEW: Start cooldown using CooldownManager
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.StartCooldown(player, "HealingBenediction", CooldownManager.CooldownType.OncePerBattle);
        }
        Debug.Log("[HealingBenediction] Skill activated - now on cooldown for this battle.");
    }

    private void SpawnHealingPlate(Game game, int x, int y)
    {
        float fx = x * 0.57f - 1.98f;
    float fy = y * 0.56f - 1.95f;

        GameObject mp = Instantiate(movePlatePrefab, new Vector3(fx, fy, -3f), Quaternion.identity);

        MovePlate oldScript = mp.GetComponent<MovePlate>();
        if (oldScript != null) Destroy(oldScript);

        mp.AddComponent<HealingBenedictionPlate>().Setup(game, x, y);
    }


    //test
    public void TestHealingBenedictionWithSP() //actual skill
{
    string player = "white"; // Bishop is always white in this test

    // ✅ NEW: Use CooldownManager instead of SkillManager cooldown
    if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "HealingBenediction"))
    {
        Debug.LogWarning("[HealingBenediction] Skill is on cooldown — cannot use.");
        return;
    }

    // 2️⃣ Try spend SP
    if (!SkillManager.Instance.SpendPlayerSP(player, 1))
    {
        Debug.LogWarning("[HealingBenediction] Not enough SP to cast.");
        return;
    }

    // 3️⃣ Activate skill (your existing logic) - but skip the cooldown check since we already did it
    HealingBenedictionWithoutCooldownCheck();

    Debug.Log("[HealingBenediction] Skill activated successfully!");
}

    // Original HealingBenediction logic without cooldown check (for internal use)
    private void HealingBenedictionWithoutCooldownCheck()
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();

        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        Vector2Int[] whiteStartPositions = new Vector2Int[]
        {
            new Vector2Int(0, 0), new Vector2Int(7, 0),
            new Vector2Int(1, 0), new Vector2Int(6, 0),
            new Vector2Int(0, 1), new Vector2Int(1, 1),
            new Vector2Int(2, 1), new Vector2Int(3, 1),
            new Vector2Int(4, 1), new Vector2Int(5, 1),
            new Vector2Int(6, 1), new Vector2Int(7, 1)
        };
    
        int platesSpawned = 0;
        foreach (Vector2Int pos in whiteStartPositions)
        {
            if (game.GetPosition(pos.x, pos.y) == null)
            {
                SpawnHealingPlate(game, pos.x, pos.y);
                platesSpawned++;
            }
        }

        Debug.Log($"[HealingBenediction] Spawned {platesSpawned} revival plates.");
        
        // ✅ NEW: Start cooldown using CooldownManager
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.StartCooldown("white", "HealingBenediction", CooldownManager.CooldownType.OncePerBattle);
        }
        Debug.Log("[HealingBenediction] Skill activated - now on cooldown for this battle.");
    }

    // Celestial Summon: Sacrifice function
    public void Sacrifice()
    {
        // Get game reference
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        
        // Get current player
        string currentPlayer = game.GetCurrentPlayer();
        
        // ✅ NEW: Use CooldownManager instead of hasUsedCelestialSummon
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(currentPlayer, "CelestialSummon"))
        {
            Debug.LogWarning("[Celestial Summon] Already used this battle — skill blocked.");
            return;
        }
        
        // SP cost check (2 SP as specified)
        const int celestialSummonCost = 2;
        if (!SkillManager.Instance.SpendPlayerSP(currentPlayer, celestialSummonCost))
        {
            Debug.LogWarning($"[Celestial Summon] Not enough SP for {currentPlayer} (cost {celestialSummonCost}).");
            return;
        }
        
        // Find the selected bishop from UIManager 
        Bishop selectedBishop = null;
        int bishopX = -1, bishopY = -1;
        
        if (UIManager.Instance != null && UIManager.Instance.selectedPiece != null)
        {
            // Use the selected piece from UI
            GameObject selectedPiece = UIManager.Instance.selectedPiece;
            selectedBishop = selectedPiece.GetComponent<Bishop>();
            
            if (selectedBishop != null)
            {
                Chessman selectedCm = selectedPiece.GetComponent<Chessman>();
                if (selectedCm != null)
                {
                    bishopX = selectedCm.GetXBoard();
                    bishopY = selectedCm.GetYBoard();
                }
            }
        }
        
        // Fallback: search for any bishop if no selection
        if (selectedBishop == null)
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    GameObject piece = game.GetPosition(x, y);
                    if (piece == null) continue;
                    
                    Bishop tempBishop = piece.GetComponent<Bishop>();
                    if (tempBishop != null)
                    {
                        Chessman tempCm = piece.GetComponent<Chessman>();
                        if (tempCm != null && tempCm.GetPlayer() == currentPlayer)
                        {
                            selectedBishop = tempBishop;
                            bishopX = x;
                            bishopY = y;
                            break;
                        }
                    }
                }
                if (selectedBishop != null) break;
            }
        }
        
        if (selectedBishop == null)
        {
            Debug.LogError("[Celestial Summon] No bishop found for current player on the board!");
            return;
        }
        
        // Remove move plates (using Chessman method)
        Chessman bishopChessman = selectedBishop.GetComponent<Chessman>();
        if (bishopChessman != null)
        {
            bishopChessman.DestroyMovePlates();
        }
        
        // Clear the position on the board
        game.ClearPosition(bishopX, bishopY);
        
        // Destroy the selected bishop GameObject
        Destroy(selectedBishop.gameObject);
        
        // Generate summon tiles on empty squares (like OnBishopButtonClick)
        GenerateCelestialSummonTiles(game, currentPlayer);
        
        // ✅ NEW: Start cooldown using CooldownManager
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.StartCooldown(currentPlayer, "CelestialSummon", CooldownManager.CooldownType.OncePerBattle);
        }
        
        Debug.Log($"[Celestial Summon] Bishop sacrificed at ({bishopX},{bishopY}) for {currentPlayer} player! Summon tiles generated. SP cost: {celestialSummonCost}");
    }
    
    // Generate tiles for Celestial Summon (based on OnBishopButtonClick pattern)
    private void GenerateCelestialSummonTiles(Game game, string player)
    {
        // Destroy existing plates first
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // Generate summon tiles on all empty squares
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 4; y++) //i made it for limitations
            {
                if (game.GetPosition(x, y) == null)
                {
                    SpawnCelestialSummonTile(game, x, y, player);
                    Debug.Log($"[Celestial Summon] Spawning summon tile at ({x},{y})");
                }
            }
        }
    }
    
    // Spawn individual summon tile (based on SpawnTile pattern)
    private void SpawnCelestialSummonTile(Game game, int x, int y, string player)
    {
        if (movePlatePrefab == null)
        {
            Debug.LogError($"[Celestial Summon] MovePlate prefab is NULL at ({x},{y})!");
            return;
        }
        
        float fx = x * 0.57f - 1.98f;
    float fy = y * 0.56f - 1.95f;

        GameObject mp = Instantiate(movePlatePrefab, new Vector3(fx, fy, -3f), Quaternion.identity);

        MovePlate oldScript = mp.GetComponent<MovePlate>();
        if (oldScript != null) Destroy(oldScript);

        // Add CelestialSummonPlate component instead of EndTurnPlate
        mp.AddComponent<CelestialSummonPlate>().Setup(game, x, y, player);
    }
 




}
