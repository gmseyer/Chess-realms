using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chronomagus : MonoBehaviour
{
    private static int countdownStartTurn = -1; // When both pieces reached corners
    private static bool isCountdownActive = false;
    private static string countdownPlayer = ""; // Which player initiated the countdown
    
    public GameObject movePlatePrefab;
    private Game game;

    private void Awake()
    {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
    }

    // Check if Chronomagus promotion is available
    public static bool IsChronomagusAvailable(string player)
    {
        if (!isCountdownActive || countdownPlayer != player)
            return false;
            
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
            return false;
            
        int currentTurn = game.turns;
        int turnsElapsed = currentTurn - countdownStartTurn;
        
        return turnsElapsed >= 3;
    }

    // Check if both pieces are on corners and start countdown
    public static void CheckCornerPositioning()
    {
        // Find Archbishop and Elemental Bishop
        GameObject archbishop = FindPiece("arch_bishop");
        GameObject elementalBishop = FindPiece("elemental_bishop");
        
        if (archbishop == null || elementalBishop == null)
        {
            ResetCountdown();
            return;
        }
        
        // Check if they're on the same player's side
        Chessman archCm = archbishop.GetComponent<Chessman>();
        Chessman elemCm = elementalBishop.GetComponent<Chessman>();
        
        if (archCm == null || elemCm == null || archCm.GetPlayer() != elemCm.GetPlayer())
        {
            ResetCountdown();
            return;
        }
        
        string player = archCm.GetPlayer();
        
        // Check if both are on corners
        if (IsOnCorner(archbishop) && IsOnCorner(elementalBishop))
        {
            if (!isCountdownActive || countdownPlayer != player)
            {
                StartCountdown(player);
            }
        }
        else
        {
            ResetCountdown();
        }
    }

    private static GameObject FindPiece(string pieceName)
    {
        Chessman[] allPieces = FindObjectsOfType<Chessman>();
        foreach (Chessman piece in allPieces)
        {
            if (piece != null && piece.name.ToLower().Contains(pieceName))
            {
                return piece.gameObject;
            }
        }
        return null;
    }

    private static bool IsOnCorner(GameObject piece)
    {
        if (piece == null) return false;
        
        Chessman cm = piece.GetComponent<Chessman>();
        if (cm == null) return false;
        
        int x = cm.GetXBoard();
        int y = cm.GetYBoard();
        
        // Check if on any of the 4 corners: (0,0), (0,7), (7,0), (7,7)
        return (x == 0 && y == 0) || (x == 0 && y == 7) || (x == 7 && y == 0) || (x == 7 && y == 7);
    }

    private static void StartCountdown(string player)
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[Chronomagus] Game component not found!");
            return;
        }
        
        isCountdownActive = true;
        countdownPlayer = player;
        countdownStartTurn = game.turns;
        
        Debug.Log($"[Chronomagus] Countdown started for {player} player at turn {countdownStartTurn}. Both pieces on corners!");
    }

    private static void ResetCountdown()
    {
        if (isCountdownActive)
        {
            Debug.Log($"[Chronomagus] Countdown reset - pieces no longer on corners or different players");
        }
        
        isCountdownActive = false;
        countdownPlayer = "";
        countdownStartTurn = -1;
    }

    // Main Chronomagus promotion method
    public void ChronomagusPromotion()
    {
        if (!IsChronomagusAvailable(countdownPlayer))
        {
            Debug.LogWarning("[Chronomagus] Promotion not available - requirements not met!");
            return;
        }

        Debug.Log("[Chronomagus] Starting Chronomagus promotion!");
        
        // Find and destroy both pieces
        GameObject archbishop = FindPiece("arch_bishop");
        GameObject elementalBishop = FindPiece("elemental_bishop");
        
        if (archbishop != null)
        {
            Chessman archCm = archbishop.GetComponent<Chessman>();
            if (archCm != null)
            {
                game.SetPositionEmpty(archCm.GetXBoard(), archCm.GetYBoard());
                Destroy(archbishop);
                Debug.Log("[Chronomagus] Archbishop destroyed");
            }
        }
        
        if (elementalBishop != null)
        {
            Chessman elemCm = elementalBishop.GetComponent<Chessman>();
            if (elemCm != null)
            {
                game.SetPositionEmpty(elemCm.GetXBoard(), elemCm.GetYBoard());
                Destroy(elementalBishop);
                Debug.Log("[Chronomagus] Elemental Bishop destroyed");
            }
        }
        
        // Create summon tiles for Chronomagus placement
        CreateChronomagusSummonTiles();
        
        // Reset countdown
        ResetCountdown();
    }

    private void CreateChronomagusSummonTiles()
    {
        // Clear existing move plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // Create summon tiles on all empty squares
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (game.GetPosition(x, y) == null)
                {
                    CreateChronomagusSummonTile(x, y);
                }
            }
        }
        
        Debug.Log("[Chronomagus] Chronomagus summon tiles created on all empty squares");
    }

    private void CreateChronomagusSummonTile(int x, int y)
    {
        float fx = x * 0.57f - 1.98f;
        float fy = y * 0.56f - 1.95f;

        GameObject mp = Instantiate(game.movePlatePrefabReference, new Vector3(fx, fy, -3f), Quaternion.identity);

        // Remove default MovePlate script
        MovePlate oldScript = mp.GetComponent<MovePlate>();
        if (oldScript != null) Destroy(oldScript);

        // Add ChronomagusSummonPlate script
        mp.AddComponent<ChronomagusSummonPlate>().Setup(game, x, y, countdownPlayer);
        
        Debug.Log($"[Chronomagus] Created summon tile at ({x},{y})");
    }

    // Prismatic Convergence skill

    // Unstable Nexus skill
    public void UnstableNexus()
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[Chronomagus] Game not found!");
            return;
        }

        int currentTurn = game.GetTurnCount();
        string currentPlayer = game.GetCurrentPlayer();

        // Check SP cost
        if (!SkillManager.Instance.SpendPlayerSP(currentPlayer, 2))
        {
            Debug.LogWarning("[Chronomagus] Not enough Skill Points for Unstable Nexus!");
            return;
        }

        // Check cooldown
        if (unstableNexusCooldown > currentTurn)
        {
            Debug.LogWarning($"[Chronomagus] Unstable Nexus on cooldown until turn {unstableNexusCooldown}!");
            return;
        }

        // Set cooldown
        unstableNexusCooldown = currentTurn + 20;

        // Log skill usage
        if (SkillTracker.Instance != null)
        {
            SkillTracker.Instance.LogSkillUsage(currentPlayer, "CHRONOMAGUS", "Unstable Nexus", 2);
        }

        // Find all empty tiles on the board
        List<Vector2Int> emptyTiles = new List<Vector2Int>();
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (game.GetPosition(x, y) == null)
                {
                    emptyTiles.Add(new Vector2Int(x, y));
                }
            }
        }

        // Check if we have enough empty tiles
        int tilesToCreate = Mathf.Min(9, emptyTiles.Count);
        if (tilesToCreate == 0)
        {
            Debug.LogWarning("[Chronomagus] No empty tiles available for Unstable Nexus!");
            return;
        }

        // Shuffle the empty tiles list
        for (int i = 0; i < emptyTiles.Count; i++)
        {
            Vector2Int temp = emptyTiles[i];
            int randomIndex = Random.Range(i, emptyTiles.Count);
            emptyTiles[i] = emptyTiles[randomIndex];
            emptyTiles[randomIndex] = temp;
        }

        // Define element types
        string[] elementTypes = { "tile_lava", "tile_thunder", "tile_ice", "celestial_pillar" };
        int[] elementCounts = { 0, 0, 0, 0 }; // Track count for each element
        int maxPerElement = 3;

        // Create elemental tiles
        for (int i = 0; i < tilesToCreate; i++)
        {
            Vector2Int tilePos = emptyTiles[i];
            
            // Choose random element type
            string elementType;
            do
            {
                int randomElement = Random.Range(0, elementTypes.Length);
                elementType = elementTypes[randomElement];
            } while (elementCounts[System.Array.IndexOf(elementTypes, elementType)] >= maxPerElement);

            // Increment count for this element
            elementCounts[System.Array.IndexOf(elementTypes, elementType)]++;

            // Create the elemental tile
            GameObject elementalTile = game.Create(elementType, tilePos.x, tilePos.y);
            if (elementalTile != null)
            {
                Debug.Log($"[Chronomagus] Created {elementType} at ({tilePos.x},{tilePos.y})");
                
                // Register tile for expiration (3 turns)
                RegisterElementalTile(elementalTile, currentTurn + 3);
            }
        }

        Debug.Log($"[Chronomagus] Unstable Nexus activated! Created {tilesToCreate} elemental tiles (cooldown until turn {unstableNexusCooldown})");
        

        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
        // End turn
        game.NextTurn();


    }

    // Register elemental tile for expiration
    private void RegisterElementalTile(GameObject tile, int expirationTurn)
    {
        // Add a component to track expiration
        ElementalTileExpiration expiration = tile.AddComponent<ElementalTileExpiration>();
        expiration.SetExpirationTurn(expirationTurn);
    }

    // Cooldown tracking
    private int unstableNexusCooldown = 0;

    // Check if Unstable Nexus is available
    public bool IsUnstableNexusAvailable()
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null) return false;
        
        int currentTurn = game.GetTurnCount();
        return unstableNexusCooldown <= currentTurn;
    }

    // Get cooldown text for UI
    public string GetUnstableNexusCooldownText()
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null) return "N/A";
        
        int currentTurn = game.GetTurnCount();
        int turnsLeft = unstableNexusCooldown - currentTurn;
        
        if (turnsLeft <= 0)
            return "Ready";
        else
            return $"{turnsLeft} turns";
    }

    // Singularity skill
    public void Singularity()
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[Chronomagus] Game not found!");
            return;
        }

        int currentTurn = game.GetTurnCount();
        string currentPlayer = game.GetCurrentPlayer();

        // Check SP cost
        if (!SkillManager.Instance.SpendPlayerSP(currentPlayer, 2))
        {
            Debug.LogWarning("[Chronomagus] Not enough Skill Points for Singularity!");
            return;
        }

        // Check once-per-battle cooldown
        if (singularityUsed)
        {
            Debug.LogWarning("[Chronomagus] Singularity already used this battle!");
            return;
        }

        // Set once-per-battle flag
        singularityUsed = true;

        // Log skill usage
        if (SkillTracker.Instance != null)
        {
            SkillTracker.Instance.LogSkillUsage(currentPlayer, "CHRONOMAGUS", "Singularity", 2);
        }

        // Generate move plates on all pieces (allies and enemies) except kings
        GenerateSingularityTargetPlates(game);

        Debug.Log("[Chronomagus] Singularity activated! Choose a target piece (except kings).");
    }

    // Generate move plates for target selection
    private void GenerateSingularityTargetPlates(Game game)
    {
        // Clear existing move plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // Generate plates on all pieces except kings
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = game.GetPosition(x, y);
                if (piece != null)
                {
                    Chessman pieceChessman = piece.GetComponent<Chessman>();
                    if (pieceChessman != null)
                    {
                        string pieceName = piece.name;
                        // Only allow pieces that start with "white" or "black" and are not kings
                        if ((pieceName.StartsWith("white") || pieceName.StartsWith("black")) && 
                            !pieceName.Contains("king"))
                        {
                            SpawnSingularityTargetPlate(game, x, y, pieceName);
                        }
                    }
                }
            }
        }
    }

    // Spawn target selection plate
    private void SpawnSingularityTargetPlate(Game game, int x, int y, string targetPieceName)
    {
        float fx = x * 0.57f - 1.98f;
        float fy = y * 0.56f - 1.95f;

        GameObject mp = Instantiate(movePlatePrefab, new Vector3(fx, fy, -3f), Quaternion.identity);

        // Remove default MovePlate script
        MovePlate oldScript = mp.GetComponent<MovePlate>();
        if (oldScript != null) Destroy(oldScript);

        // Add SingularityTargetPlate script
        SingularityTargetPlate plate = mp.AddComponent<SingularityTargetPlate>();
        plate.Setup(game, x, y, targetPieceName);

        // Make target plates visually distinct (purple)
        SpriteRenderer sr = mp.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.magenta;
        }
    }

    // Once-per-battle tracking
    private bool singularityUsed = false;

    // Check if Singularity is available
    public bool IsSingularityAvailable()
    {
        return !singularityUsed;
    }

    // Get cooldown text for UI
    public string GetSingularityCooldownText()
    {
        if (singularityUsed)
            return "Used";
        else
            return "Ready";
    }

}
