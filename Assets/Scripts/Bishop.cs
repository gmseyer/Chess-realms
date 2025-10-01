using UnityEngine;
using System.Collections.Generic;

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

        // ✅ Spawn ELEMENTAL BISHOP plates (player-specific positioning)
        if (player == "white")
        {
            // White player: bottom 3-4 ranks
            for (int x = 4; x < 8; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    if (game.GetPosition(x, y) == null)
                    {
                        SpawnTile(game, x, y, elementalSummonPlatePrefab, "white_elemental_bishop");
                        Debug.Log($"[DivineOffering] Spawning ELEMENTAL bishop plate at ({x},{y})");
                    }
                }
            }

            // White player: bottom 1-2 ranks for archbishop
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++) 
                {
                    if (game.GetPosition(x, y) == null)
                    {
                        SpawnTile(game, x, y, archbishopSummonPlatePrefab, "white_arch_bishop");
                        Debug.Log($"[DivineOffering] Spawning ARCHBISHOP plate at ({x},{y})");
                    }
                }
            }
        }
        else if (player == "black")
        {
            // Black player: top 3-4 ranks
            for (int x = 4; x < 8; x++)
            {
                for (int y = 4; y < 8; y++)
                {
                    if (game.GetPosition(x, y) == null)
                    {
                        SpawnTile(game, x, y, elementalSummonPlatePrefab, "black_elemental_bishop");
                        Debug.Log($"[DivineOffering] Spawning ELEMENTAL bishop plate at ({x},{y})");
                    }
                }
            }

            // Black player: top 1-2 ranks for archbishop
            for (int x = 0; x < 4; x++)
            {
                for (int y = 4; y < 8; y++) 
                {
                    if (game.GetPosition(x, y) == null)
                    {
                        SpawnTile(game, x, y, archbishopSummonPlatePrefab, "black_arch_bishop");
                        Debug.Log($"[DivineOffering] Spawning ARCHBISHOP plate at ({x},{y})");
                    }
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

        // Player-specific starting positions for revival plates
        Vector2Int[] startPositions;
        if (player == "white")
        {
            startPositions = new Vector2Int[]
            {
                new Vector2Int(0, 0), new Vector2Int(7, 0),
                new Vector2Int(1, 0), new Vector2Int(6, 0),
                new Vector2Int(0, 1), new Vector2Int(1, 1),
                new Vector2Int(2, 1), new Vector2Int(3, 1),
                new Vector2Int(4, 1), new Vector2Int(5, 1),
                new Vector2Int(6, 1), new Vector2Int(7, 1)
            };
        }
        else // black player
        {
            startPositions = new Vector2Int[]
            {
                new Vector2Int(0, 7), new Vector2Int(7, 7),
                new Vector2Int(1, 7), new Vector2Int(6, 7),
                new Vector2Int(0, 6), new Vector2Int(1, 6),
                new Vector2Int(2, 6), new Vector2Int(3, 6),
                new Vector2Int(4, 6), new Vector2Int(5, 6),
                new Vector2Int(6, 6), new Vector2Int(7, 6)
            };
        }
    
        int platesSpawned = 0;
        foreach (Vector2Int pos in startPositions)
        {
            if (game.GetPosition(pos.x, pos.y) == null)
            {
                SpawnHealingPlate(game, pos.x, pos.y);
                platesSpawned++;
            }
        }

        Debug.Log($"[HealingBenediction] Spawned {platesSpawned} revival plates for {player} player.");
        
        // ✅ NOTE: Cooldown is NOT set here! It's set when clicking a plate to revive a piece.
        // This allows the player to cancel without using the skill.
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
    // ✅ Get the selected bishop from UIManager (following RoyalBishop pattern)
    Bishop selectedBishop = null;
    Chessman cm = null;
    
    if (UIManager.Instance != null && UIManager.Instance.selectedPiece != null)
    {
        GameObject selectedPiece = UIManager.Instance.selectedPiece;
        selectedBishop = selectedPiece.GetComponent<Bishop>();
        cm = selectedPiece.GetComponent<Chessman>();
        
        if (selectedBishop == null || cm == null)
        {
            Debug.LogError($"[HealingBenediction] Selected piece {selectedPiece.name} is not a regular Bishop or missing Chessman component!");
            return;
        }
    }
    else
    {
        Debug.LogError("[HealingBenediction] No piece selected via UIManager!");
        return;
    }
    
    string player = cm.GetPlayer();
    Debug.Log($"[HealingBenediction] Attempting activation for {player} player from {selectedBishop.gameObject.name}...");

    // ✅ NEW: Use CooldownManager instead of SkillManager cooldown
    if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "HealingBenediction"))
    {
        Debug.LogWarning($"[HealingBenediction] Skill is on cooldown for {player} — cannot use.");
        return;
    }

    // 2️⃣ Try spend SP
    if (!SkillManager.Instance.SpendPlayerSP(player, 1))
    {
        Debug.LogWarning($"[HealingBenediction] Not enough SP for {player} to cast.");
        return;
    }

    // 3️⃣ Activate skill on the selected bishop
    // Update cached chessman reference for other methods
    selectedBishop.chessman = cm;
    selectedBishop.HealingBenedictionWithoutCooldownCheck();
    
    // End turn (with null check)
    if (Game.Instance != null)
    {
        Game.Instance.NextTurn();
    }
    else
    {
        // Fallback to finding Game via tag
        GameObject controller = GameObject.FindGameObjectWithTag("GameController");
        if (controller != null)
        {
            controller.GetComponent<Game>().NextTurn();
        }
    }
    
    Debug.Log($"[HealingBenediction] Skill activated successfully for {player} player!");
}

    // Original HealingBenediction logic without cooldown check (for internal use)
    private void HealingBenedictionWithoutCooldownCheck()
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();

        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // Get player from the current bishop
        string player = "white"; // Default fallback
        if (chessman != null)
        {
            player = chessman.GetPlayer();
        }

        // Player-specific starting positions for revival plates
        Vector2Int[] startPositions;
        if (player == "white")
        {
            startPositions = new Vector2Int[]
            {
                new Vector2Int(0, 0), new Vector2Int(7, 0),
                new Vector2Int(1, 0), new Vector2Int(6, 0),
                new Vector2Int(0, 1), new Vector2Int(1, 1),
                new Vector2Int(2, 1), new Vector2Int(3, 1),
                new Vector2Int(4, 1), new Vector2Int(5, 1),
                new Vector2Int(6, 1), new Vector2Int(7, 1)
            };
        }
        else // black player
        {
            startPositions = new Vector2Int[]
            {
                new Vector2Int(0, 7), new Vector2Int(7, 7),
                new Vector2Int(1, 7), new Vector2Int(6, 7),
                new Vector2Int(0, 6), new Vector2Int(1, 6),
                new Vector2Int(2, 6), new Vector2Int(3, 6),
                new Vector2Int(4, 6), new Vector2Int(5, 6),
                new Vector2Int(6, 6), new Vector2Int(7, 6)
            };
        }
    
        int platesSpawned = 0;
        foreach (Vector2Int pos in startPositions)
        {
            if (game.GetPosition(pos.x, pos.y) == null)
            {
                SpawnHealingPlate(game, pos.x, pos.y);
                platesSpawned++;
            }
        }

        Debug.Log($"[HealingBenediction] Spawned {platesSpawned} revival plates for {player} player.");
        
        // ✅ NOTE: Cooldown is NOT set here! It's set when clicking a plate to revive a piece.
        // This allows the player to cancel without using the skill (just the SP cost is spent).
    }

    // Celestial Summon: Sacrifice function
    public void Sacrifice()
    {
        // ✅ Get the selected bishop from UIManager (following HealingBenediction pattern)
        Bishop selectedBishop = null;
        Chessman cm = null;
        
        if (UIManager.Instance != null && UIManager.Instance.selectedPiece != null)
        {
            GameObject selectedPiece = UIManager.Instance.selectedPiece;
            selectedBishop = selectedPiece.GetComponent<Bishop>();
            cm = selectedPiece.GetComponent<Chessman>();
            
            if (selectedBishop == null || cm == null)
            {
                Debug.LogError($"[Celestial Summon] Selected piece {selectedPiece.name} is not a regular Bishop or missing Chessman component!");
                return;
            }
        }
        else
        {
            Debug.LogError("[Celestial Summon] No piece selected via UIManager!");
            return;
        }
        
        string player = cm.GetPlayer();
        Debug.Log($"[Celestial Summon] Attempting activation for {player} player from {selectedBishop.gameObject.name}...");
        
        // ✅ Check cooldown BEFORE spending SP
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "CelestialSummon"))
        {
            Debug.LogWarning($"[Celestial Summon] Skill is on cooldown for {player} — cannot use.");
            return;
        }
        
        // ✅ SP cost check (1 SP as per requirements)
        const int celestialSummonCost = 1;
        if (!SkillManager.Instance.SpendPlayerSP(player, celestialSummonCost))
        {
            Debug.LogWarning($"[Celestial Summon] Not enough SP for {player} to cast (cost {celestialSummonCost}).");
            return;
        }
        
        // Get bishop position
        int bishopX = cm.GetXBoard();
        int bishopY = cm.GetYBoard();
        
        // Get game reference
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        
        // Remove move plates (using Chessman method)
        cm.DestroyMovePlates();
        
        // Clear the position on the board
        game.ClearPosition(bishopX, bishopY);
        
        // Destroy the selected bishop GameObject
        Destroy(selectedBishop.gameObject);
        
        // Generate summon tiles on empty squares (player-specific)
        GenerateCelestialSummonTiles(game, player);
        
        // ✅ NOTE: Cooldown is NOT set here! It's set in CelestialSummonPlate after summoning pawns.
        // This allows cancellation if no valid summons.
        
        Debug.Log($"[Celestial Summon] Bishop sacrificed at ({bishopX},{bishopY}) for {player} player! Summon tiles generated. SP cost: {celestialSummonCost}");
    }
    
    // Generate tiles for Celestial Summon (player-specific positioning)
    private void GenerateCelestialSummonTiles(Game game, string player)
    {
        // Destroy existing plates first
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // Generate summon tiles on empty squares (player-specific)
        if (player == "white")
        {
            // White player: bottom half of board (y=0-3)
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    if (game.GetPosition(x, y) == null)
                    {
                        SpawnCelestialSummonTile(game, x, y, player);
                        Debug.Log($"[Celestial Summon] Spawning summon tile for {player} at ({x},{y})");
                    }
                }
            }
        }
        else if (player == "black")
        {
            // Black player: top half of board (y=4-7)
            for (int x = 0; x < 8; x++)
            {
                for (int y = 4; y < 8; y++)
                {
                    if (game.GetPosition(x, y) == null)
                    {
                        SpawnCelestialSummonTile(game, x, y, player);
                        Debug.Log($"[Celestial Summon] Spawning summon tile for {player} at ({x},{y})");
                    }
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

    // Wraithform Ascension Skill
    public void WraithformAscension()
    {
        // ✅ Get the selected bishop from UIManager (following Queen/Bishop pattern)
        Bishop selectedBishop = null;
        Chessman cm = null;
        
        if (UIManager.Instance != null && UIManager.Instance.selectedPiece != null)
        {
            GameObject selectedPiece = UIManager.Instance.selectedPiece;
            selectedBishop = selectedPiece.GetComponent<Bishop>();
            cm = selectedPiece.GetComponent<Chessman>();
            
            if (selectedBishop == null || cm == null)
            {
                Debug.LogError($"[WraithformAscension] Selected piece {selectedPiece.name} is not a Bishop or missing Chessman component!");
                return;
            }
        }
        else
        {
            Debug.LogError("[WraithformAscension] No piece selected via UIManager!");
            return;
        }
        
        string player = cm.GetPlayer();
        Debug.Log($"[WraithformAscension] Attempting activation for {player} player...");
        
        // ✅ Check cooldown BEFORE spending SP (using CooldownManager)
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "WraithformAscension"))
        {
            Debug.LogWarning($"[WraithformAscension] Skill is on cooldown for {player} — cannot use.");
            return;
        }
        
        // Check SP cost (2 SP)
        if (!SkillManager.Instance.SpendPlayerSP(player, 2))
        {
            Debug.LogWarning($"[WraithformAscension] Not enough SP for {player} to cast.");
            return;
        }
        
        // ✅ Set cooldown using CooldownManager
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.StartCooldown(player, "WraithformAscension", CooldownManager.CooldownType.OncePerBattle);
        }
        Debug.Log($"[WraithformAscension] Cooldown activated for {player} - once per battle.");
        
        // Get current turn and calculate expiration
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        int currentTurn = game.turns;
        int expirationTurn = currentTurn + 14;
        
        // Add Ethereal status to the selected Bishop
        if (cm != null && cm.statusManager != null)
        {
            cm.statusManager.AddStatus(StatusType.Ethereal, expirationTurn);
            Debug.Log($"[WraithformAscension] {player} Bishop gained Ethereal status until turn {expirationTurn}");
        }
        else
        {
            Debug.LogError("[WraithformAscension] Could not find Chessman or StatusManager component on selected Bishop!");
            return;
        }
        
        // Log skill usage
        if (SkillTracker.Instance != null)
        {
            SkillTracker.Instance.LogSkillUsage(player, "BISHOP", "WRAITHFORM ASCENSION", 2);
        }
        
        Debug.Log($"[WraithformAscension] Skill activated successfully for {player}!");
        Chessman[] allPieces = FindObjectsOfType<Chessman>();
            
        foreach (Chessman piece in allPieces)
        {
            piece.UpdateVisualStatus();
            foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
                Destroy(plate);
        }
    } 

    // Ethereal movement method - can pass through any piece but only land on empty tiles
    public void GenerateEtherealMovePlates()
    {
        // ✅ Safety check for chessman reference (following existing Bishop pattern)
        if (chessman == null)
        {
            chessman = GetComponent<Chessman>();
            if (chessman == null)
            {
                Debug.LogError("[EtherealMovement] No Chessman component found!");
                return;
            }
        }
        
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        int currentTurn = game.turns;
        
        // Check if Bishop has Ethereal status
        if (!chessman.statusManager.HasStatus(StatusType.Ethereal, currentTurn))
        {
            Debug.LogWarning("[EtherealMovement] Bishop does not have Ethereal status!");
            return;
        }
        
        // Generate diagonal movement plates (like normal Bishop)
        EtherealLineMovePlate(1, 1);   // Up-Right
        EtherealLineMovePlate(-1, -1); // Down-Left
        EtherealLineMovePlate(-1, 1);  // Up-Left
        EtherealLineMovePlate(1, -1);  // Down-Right
        
        Debug.Log("[EtherealMovement] Ethereal move plates generated - can pass through any piece!");
    }

    // Helper method for ethereal line movement
    private void EtherealLineMovePlate(int xIncrement, int yIncrement)
    {
        // ✅ Safety check for chessman reference (following existing Bishop pattern)
        if (chessman == null)
        {
            chessman = GetComponent<Chessman>();
            if (chessman == null)
            {
                Debug.LogError("[EtherealLineMovePlate] No Chessman component found!");
                return;
            }
        }
        
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        int startX = chessman.GetXBoard();
        int startY = chessman.GetYBoard();
        int x = startX;
        int y = startY;
        
        while (game.PositionOnBoard(x + xIncrement, y + yIncrement))
        {
            x += xIncrement;
            y += yIncrement;
            
            GameObject target = game.GetPosition(x, y);
            if (target == null)
            {
                // Empty tile - can land here
                // Calculate enemies passed through on the actual path to this destination
                List<GameObject> passedEnemies = GetEnemiesOnPath(startX, startY, x, y, xIncrement, yIncrement);
                EtherealMovePlateSpawn(x, y, passedEnemies);
            }
            else
            {
                // Occupied tile - continue to next tile to check for empty destination
                continue;
            }
        }
    }
    
    // Helper method to get enemies on the actual path between start and destination
    private List<GameObject> GetEnemiesOnPath(int startX, int startY, int destX, int destY, int xIncrement, int yIncrement)
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        List<GameObject> passedEnemies = new List<GameObject>();
        
        // Walk along the path from start to destination
        int currentX = startX + xIncrement;
        int currentY = startY + yIncrement;
        
        while (currentX != destX || currentY != destY)
        {
            GameObject target = game.GetPosition(currentX, currentY);
            if (target != null)
            {
                Chessman targetCm = target.GetComponent<Chessman>();
                if (targetCm != null && targetCm.GetPlayer() != chessman.GetPlayer())
                {
                    // Check if enemy is royalty (king or queen)
                    if (!target.name.ToLower().Contains("king") && !target.name.ToLower().Contains("queen"))
                    {
                        passedEnemies.Add(target);
                        Debug.Log($"[EtherealMovement] Bishop will pass through enemy: {target.name} at ({currentX},{currentY})");
                    }
                    else
                    {
                        Debug.Log($"[EtherealMovement] Bishop cannot affect royalty: {target.name} at ({currentX},{currentY})");
                    }
                }
            }
            
            currentX += xIncrement;
            currentY += yIncrement;
        }
        
        return passedEnemies;
    }

    // Helper method to spawn ethereal move plates
    private void EtherealMovePlateSpawn(int matrixX, int matrixY, List<GameObject> passedEnemies)
    {
        float x = matrixX * 0.57f - 1.98f;
        float y = matrixY * 0.56f - 1.95f;
        
        GameObject mp = Instantiate(movePlatePrefab, new Vector3(x, y, -3f), Quaternion.identity);
        
        // Remove default MovePlate script
        MovePlate oldScript = mp.GetComponent<MovePlate>();
        if (oldScript != null) Destroy(oldScript);
        
        // Add EtherealMovePlate script
        EtherealMovePlate etherealScript = mp.AddComponent<EtherealMovePlate>();
        etherealScript.Setup(gameObject, matrixX, matrixY, passedEnemies);
        
        // Make ethereal move plates visually distinct (cyan color)
        SpriteRenderer sr = mp.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.cyan;
        }

    
    }

}
