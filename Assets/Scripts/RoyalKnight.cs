using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoyalKnight : MonoBehaviour
{
    private Game game;
    
    // Phantom Swap cooldown tracking (6 turns)
    private int phantomSwapCooldown = 0;
    private const int phantomSwapCooldownMax = 6;
    
    // Sacred Mist once per battle tracking
    private bool sacredMistUsed = false;
    
    // Oathbound Gambit once per battle tracking
    private bool oathboundGambitUsed = false;
    
    // Static flag to track if any OathboundGambit is currently active
    private static bool isOathboundGambitActive = false;
    private static int oathboundGambitEndTurn = 0;
    
    // MovePlate prefab reference for creating target plates
    public GameObject movePlatePrefab;

    private void Awake()
    {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
    }

    // Phantom Guard passive - leaves a Mist Knight illusion on previous square when moving
    public void PhantomGuard(int previousX, int previousY)
    {
        if (game == null)
        {
            Debug.LogError("[PhantomGuard] Missing Game reference!");
            return;
        }

        Debug.Log($"[PhantomGuard] Royal Knight moved from ({previousX},{previousY}) - activating Phantom Guard");

        // Check if there are existing white_mist_knight pieces and destroy them
        DestroyExistingMistKnights();

        // Create new mist knight at the Royal Knight's previous position
        CreateMistKnight(previousX, previousY);

        Debug.Log($"[PhantomGuard] Mist Knight illusion created at ({previousX},{previousY})");
    }

    private void DestroyExistingMistKnights()
    {
        // Find all existing mist knights on the board
        Chessman[] allPieces = FindObjectsOfType<Chessman>();
        
        foreach (Chessman piece in allPieces)
        {
            if (piece != null && piece.name == "white_mist_knight")
            {
                Debug.Log($"[PhantomGuard] Destroying existing mist knight at ({piece.GetXBoard()},{piece.GetYBoard()})");
                
                // Clear the position from the game board
                game.SetPositionEmpty(piece.GetXBoard(), piece.GetYBoard());
                
                // Destroy the GameObject
                Destroy(piece.gameObject);
            }
        }
    }

    private void CreateMistKnight(int x, int y)
    {
        // Check if the position is empty (it should be since the Royal Knight just moved from there)
        if (game.GetPosition(x, y) != null)
        {
            Debug.LogWarning($"[PhantomGuard] Position ({x},{y}) is not empty! Cannot create mist knight.");
            return;
        }

        // Create the mist knight using Game.Create
        GameObject mistKnight = game.Create("white_mist_knight", x, y);
        
        Debug.Log($"[PhantomGuard] Mist Knight created at ({x},{y})");
    }

    // Try to trigger Phantom Swap when Royal Knight is threatened with capture
    // Returns true if swap was successful, false if no mist knight available or on cooldown
    public bool TryTriggerPhantomSwap()
    {
        if (phantomSwapCooldown > 0)
        {
            Debug.Log($"[PhantomSwap] Phantom Swap is on cooldown for {phantomSwapCooldown} more turn(s).");
            return false;
        }

        if (game == null)
        {
            Debug.LogError("[PhantomSwap] Missing Game reference!");
            return false;
        }

        // Find existing mist knight on the board
        GameObject mistKnight = FindExistingMistKnight();
        if (mistKnight == null)
        {
            Debug.Log("[PhantomSwap] No mist knight found on board - cannot trigger Phantom Swap.");
            return false;
        }

        Chessman mistKnightChessman = mistKnight.GetComponent<Chessman>();
        if (mistKnightChessman == null)
        {
            Debug.LogError("[PhantomSwap] Mist knight has no Chessman component!");
            return false;
        }

        // Get mist knight's position
        int mistKnightX = mistKnightChessman.GetXBoard();
        int mistKnightY = mistKnightChessman.GetYBoard();

        // Get Royal Knight's current position
        Chessman royalKnightChessman = GetComponent<Chessman>();
        if (royalKnightChessman == null)
        {
            Debug.LogError("[PhantomSwap] Royal Knight has no Chessman component!");
            return false;
        }

        int royalKnightX = royalKnightChessman.GetXBoard();
        int royalKnightY = royalKnightChessman.GetYBoard();

        Debug.Log($"[PhantomSwap] Royal Knight at ({royalKnightX},{royalKnightY}) swapping with Mist Knight at ({mistKnightX},{mistKnightY})");

        // Perform the swap
        SwapPositions(royalKnightChessman, mistKnightChessman, mistKnightX, mistKnightY);

        // Destroy the mist knight (like Queen's pawn sacrifice)
        game.SetPositionEmpty(mistKnightX, mistKnightY);
        Destroy(mistKnight);

        // Set cooldown
        phantomSwapCooldown = phantomSwapCooldownMax;

        Debug.Log($"[PhantomSwap] Phantom Swap successful! Royal Knight moved to ({mistKnightX},{mistKnightY}), Mist Knight destroyed. Cooldown: {phantomSwapCooldownMax} turns.");
        return true;
    }

    private GameObject FindExistingMistKnight()
    {
        // Find existing mist knight on the board
        Chessman[] allPieces = FindObjectsOfType<Chessman>();
        
        foreach (Chessman piece in allPieces)
        {
            if (piece != null && piece.name == "white_mist_knight")
            {
                return piece.gameObject;
            }
        }
        
        return null;
    }

    private void SwapPositions(Chessman royalKnight, Chessman mistKnight, int mistKnightX, int mistKnightY)
    {
        // Get Royal Knight's current position
        int royalKnightX = royalKnight.GetXBoard();
        int royalKnightY = royalKnight.GetYBoard();

        // Step 1: Clear both positions
        game.SetPositionEmpty(royalKnightX, royalKnightY);
        game.SetPositionEmpty(mistKnightX, mistKnightY);

        // Step 2: Update Royal Knight coordinates and visual position
        royalKnight.SetXBoard(mistKnightX);
        royalKnight.SetYBoard(mistKnightY);
        royalKnight.SetCoords();

        // Step 3: Set Royal Knight at the new position
        game.SetPositionAt(royalKnight.gameObject, mistKnightX, mistKnightY);
    }

    // Check if Phantom Swap is available (not on cooldown)
    public bool IsPhantomSwapAvailable()
    {
        return phantomSwapCooldown <= 0;
    }

    // Reduce cooldown by 1 turn (call this at the start of each turn)
    public void ReducePhantomSwapCooldown()
    {
        if (phantomSwapCooldown > 0)
        {
            phantomSwapCooldown--;
            Debug.Log($"[PhantomSwap] Cooldown reduced to {phantomSwapCooldown} turns remaining.");
        }
    }

    // Clean up mist knight when Royal Knight is captured/destroyed
    public void OnRoyalKnightDestroyed()
    {
        if (game == null)
        {
            Debug.LogError("[PhantomGuard] Missing Game reference during cleanup!");
            return;
        }

        // Find and destroy any existing mist knight
        GameObject mistKnight = FindExistingMistKnight();
        if (mistKnight != null)
        {
            Chessman mistKnightChessman = mistKnight.GetComponent<Chessman>();
            if (mistKnightChessman != null)
            {
                int mistX = mistKnightChessman.GetXBoard();
                int mistY = mistKnightChessman.GetYBoard();
                
                Debug.Log($"[PhantomGuard] Royal Knight destroyed - cleaning up Mist Knight at ({mistX},{mistY})");
                
                // Clear position and destroy mist knight
                game.SetPositionEmpty(mistX, mistY);
                Destroy(mistKnight);
            }
        }
        else
        {
            Debug.Log("[PhantomGuard] Royal Knight destroyed - no Mist Knight found to clean up.");
        }
    }

    // Sacred Mist active skill - Give any allied piece the Phantom Guard buff for 2 turns
    // Cost: 2 SP, Once per battle
    public void SacredMist()
    {
        if (game == null)
        {
            Debug.LogError("[SacredMist] Missing Game reference!");
            return;
        }

        // Check if already used this battle
        if (sacredMistUsed)
        {
            Debug.Log("[SacredMist] Sacred Mist already used this battle!");
            return;
        }

        // Get current player (assume UI validation handles turn checking)
        string currentPlayer = game.GetCurrentPlayer();
        
        Debug.Log($"[SacredMist] Sacred Mist activated by {currentPlayer} Royal Knight! Current player: {currentPlayer}");

        // Check SP cost (2 SP)
        const int sacredMistCost = 2;
        if (SkillManager.Instance.GetPlayerSP(currentPlayer) < sacredMistCost)
        {
            Debug.LogWarning($"[SacredMist] Not enough SP for {currentPlayer} (cost {sacredMistCost}).");
            return;
        }

        // Deduct SP
        bool paid = SkillManager.Instance.SpendPlayerSP(currentPlayer, sacredMistCost);
        if (!paid)
        {
            Debug.LogWarning("[SacredMist] Failed to deduct SP!");
            return;
        }

        Debug.Log($"[SacredMist] Sacred Mist skill used! Cost: {sacredMistCost} SP");

        // Mark as used
        sacredMistUsed = true;

        // Generate target plates for allied pieces
        GenerateSacredMistTargetPlates(currentPlayer);
    }

    private void GenerateSacredMistTargetPlates(string playerColor)
    {
        // Clear existing move plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        Debug.Log($"[SacredMist] Generating target plates for {playerColor} allied pieces");

        int platesCreated = 0;

        // Generate plates on all allied pieces
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = game.GetPosition(x, y);
                if (piece != null && piece.name != "white_mist_knight" && piece.name != "white_royal_knight")
                {
                    Chessman pieceChessman = piece.GetComponent<Chessman>();
                    if (pieceChessman != null)
                    {
                        string pieceName = piece.name;
                        // Only target allied pieces (same player color)
                        if (pieceName.StartsWith(playerColor))
                        {
                            SpawnSacredMistTargetPlate(x, y, pieceName);
                            platesCreated++;
                        }
                    }
                }
            }
        }

        Debug.Log($"[SacredMist] Created {platesCreated} target plates for {playerColor} pieces. Select an allied piece to grant Phantom Guard buff.");
    }

    private void SpawnSacredMistTargetPlate(int x, int y, string targetPieceName)
    {
        if (movePlatePrefab == null)
        {
            Debug.LogError("[SacredMist] MovePlate prefab is null! Cannot create target plates.");
            return;
        }

        float fx = x * 0.57f - 1.98f;
        float fy = y * 0.56f - 1.95f;

        GameObject mp = Instantiate(movePlatePrefab, new Vector3(fx, fy, -3f), Quaternion.identity);

        // Remove default MovePlate script
        MovePlate oldScript = mp.GetComponent<MovePlate>();
        if (oldScript != null) Destroy(oldScript);

        // Add SacredMistPlate script
        SacredMistPlate plate = mp.AddComponent<SacredMistPlate>();
        plate.Setup(game, x, y, targetPieceName, this);

        // Make target plates visually distinct (white with 50% opacity)
        SpriteRenderer sr = mp.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = new Color(1f, 1f, 1f, 0.5f); // White with 50% opacity
        }

        Debug.Log($"[SacredMist] Created target plate for {targetPieceName} at ({x},{y})");
    }

    // Apply Phantom Guard buff to target piece (called from SacredMistPlate)
    public void ApplyPhantomGuardBuff(int x, int y, string targetPieceName)
    {
        GameObject targetPiece = game.GetPosition(x, y);
        if (targetPiece == null || targetPiece.name != targetPieceName)
        {
            Debug.LogError($"[SacredMist] Target piece not found or name mismatch at ({x},{y})!");
            return;
        }

        Chessman targetChessman = targetPiece.GetComponent<Chessman>();
        if (targetChessman == null)
        {
            Debug.LogError($"[SacredMist] Target piece has no Chessman component!");
            return;
        }

        // Apply Phantom Guard status for 2 turns
        int currentTurn = game.GetTurnCount();
        int expiresOnTurn = currentTurn + 2;
        
        targetChessman.statusManager.AddStatus(StatusType.PhantomGuard, expiresOnTurn);
        
        Debug.Log($"[SacredMist] Phantom Guard buff applied to {targetPieceName} at ({x},{y}) - expires on turn {expiresOnTurn}");

        // Clean up target plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // End turn
        game.NextTurn();
    }

    // Reset Sacred Mist usage for new battle
    public static void ResetSacredMistUsage()
    {
        RoyalKnight[] royalKnights = FindObjectsOfType<RoyalKnight>();
        foreach (RoyalKnight royalKnight in royalKnights)
        {
            if (royalKnight != null)
            {
                royalKnight.sacredMistUsed = false;
            }
        }
        Debug.Log("[SacredMist] Usage reset for all Royal Knights in new battle");
    }

    // Oathbound Gambit active skill - Select enemy piece for 1v1 duel
    // Cost: 1 SP, Once per battle, locks all other pieces for 4 turns
    public void OathboundGambit()
    {
        if (game == null)
        {
            Debug.LogError("[OathboundGambit] Missing Game reference!");
            return;
        }

        // Check if already used this battle
        if (oathboundGambitUsed)
        {
            Debug.Log("[OathboundGambit] Oathbound Gambit already used this battle!");
            return;
        }

        // Get current player
        string currentPlayer = game.GetCurrentPlayer();
        
        Debug.Log($"[OathboundGambit] Oathbound Gambit activated by {currentPlayer} Royal Knight!");

        // Check SP cost (1 SP)
        const int oathboundCost = 1;
        if (SkillManager.Instance.GetPlayerSP(currentPlayer) < oathboundCost)
        {
            Debug.LogWarning($"[OathboundGambit] Not enough SP for {currentPlayer} (cost {oathboundCost}).");
            return;
        }

        // Deduct SP
        bool paid = SkillManager.Instance.SpendPlayerSP(currentPlayer, oathboundCost);
        if (!paid)
        {
            Debug.LogWarning("[OathboundGambit] Failed to deduct SP!");
            return;
        }

        Debug.Log($"[OathboundGambit] Oathbound Gambit skill used! Cost: {oathboundCost} SP");

        // Mark as used
        oathboundGambitUsed = true;

        // Generate target plates for enemy pieces
        GenerateOathboundTargetPlates(currentPlayer);
    }

    private void GenerateOathboundTargetPlates(string playerColor)
    {
        // Clear existing move plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        Debug.Log($"[OathboundGambit] Generating target plates for enemy pieces (not {playerColor})");

        int platesCreated = 0;
        string enemyColor = (playerColor == "white") ? "black" : "white";

        // Generate plates on all enemy pieces (except kings)
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
                        // Only target enemy pieces, exclude kings
                        if (pieceName.StartsWith(enemyColor) && !pieceName.Contains("king"))
                        {
                            SpawnOathboundTargetPlate(x, y, pieceName);
                            platesCreated++;
                        }
                    }
                }
            }
        }

        Debug.Log($"[OathboundGambit] Created {platesCreated} target plates for {enemyColor} pieces (excluding kings). Select an enemy to challenge to a duel!");
    }

    private void SpawnOathboundTargetPlate(int x, int y, string targetPieceName)
    {
        if (movePlatePrefab == null)
        {
            Debug.LogError("[OathboundGambit] MovePlate prefab is null! Cannot create target plates.");
            return;
        }

        float fx = x * 0.57f - 1.98f;
        float fy = y * 0.56f - 1.95f;

        GameObject mp = Instantiate(movePlatePrefab, new Vector3(fx, fy, -3f), Quaternion.identity);

        // Remove default MovePlate script
        MovePlate oldScript = mp.GetComponent<MovePlate>();
        if (oldScript != null) Destroy(oldScript);

        // Add OathboundGambitPlate script
        OathboundGambitPlate plate = mp.AddComponent<OathboundGambitPlate>();
        plate.Setup(game, x, y, targetPieceName, this);

        // Make target plates visually distinct (red for enemy targeting)
        SpriteRenderer sr = mp.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.red;
        }

        Debug.Log($"[OathboundGambit] Created target plate for {targetPieceName} at ({x},{y})");
    }

    // Apply Oathbound Gambit effect - lock all pieces except Royal Knight and target
    public void ApplyOathboundGambit(int x, int y, string targetPieceName)
    {
        GameObject targetPiece = game.GetPosition(x, y);
        if (targetPiece == null || targetPiece.name != targetPieceName)
        {
            Debug.LogError($"[OathboundGambit] Target piece not found or name mismatch at ({x},{y})!");
            return;
        }

        Chessman targetChessman = targetPiece.GetComponent<Chessman>();
        if (targetChessman == null)
        {
            Debug.LogError($"[OathboundGambit] Target piece has no Chessman component!");
            return;
        }

        Debug.Log($"[OathboundGambit] Oathbound Gambit activated! Target: {targetPieceName} at ({x},{y})");

        // Set Oathbound Gambit as active
        isOathboundGambitActive = true;
        oathboundGambitEndTurn = game.GetTurnCount() + 6; // 6 turns duration
        Debug.Log($"[OathboundGambit] Duel will end on turn {oathboundGambitEndTurn}");

        // Lock all pieces except this Royal Knight and the target
        LockAllPiecesExceptDuelists(targetPiece);

        // Clean up target plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // End turn
        game.NextTurn();
    }

    private void LockAllPiecesExceptDuelists(GameObject oathTarget)
    {
        int currentTurn = game.GetTurnCount();
        int lockDuration = 6; // 4 turns
        int expiresOnTurn = currentTurn + lockDuration;

        Debug.Log($"[OathboundGambit] Locking all pieces except Royal Knight and {oathTarget.name} for {lockDuration} turns (until turn {expiresOnTurn})");

        // Find all pieces on the board
        Chessman[] allPieces = FindObjectsOfType<Chessman>();
        int lockedCount = 0;

        foreach (Chessman piece in allPieces)
        {
            if (piece == null) continue;

            // Skip the Royal Knight (by name)
            if (piece.name.Contains("royal_knight")) 
            {
                Debug.Log($"[OathboundGambit] Skipping Royal Knight {piece.name} - duelist remains free!");
                continue;
            }

            // Skip the oath target
            if (piece.gameObject == oathTarget) 
            {
                Debug.Log($"[OathboundGambit] Skipping oath target {piece.name} - duelist remains free!");
                continue;
            }

            // Skip tiles and neutral pieces
            if (piece.name.StartsWith("tile_") || piece.name.Contains("celestial_pillar")) continue;

            // Lock all other pieces with Invulnerable + Stunned
            piece.statusManager.AddStatus(StatusType.Invulnerable, expiresOnTurn);
            piece.statusManager.AddStatus(StatusType.Stunned, expiresOnTurn);
            lockedCount++;

            Debug.Log($"[OathboundGambit] Locked {piece.name} - cannot move or be attacked until turn {expiresOnTurn}");
        }

        Debug.Log($"[OathboundGambit] Oathbound duel initiated! Locked {lockedCount} pieces. Only Royal Knight and {oathTarget.name} can act for {lockDuration} turns!");
    }

    // Reset Oathbound Gambit usage for new battle
    public static void ResetOathboundGambitUsage()
    {
        RoyalKnight[] royalKnights = FindObjectsOfType<RoyalKnight>();
        foreach (RoyalKnight royalKnight in royalKnights)
        {
            if (royalKnight != null)
            {
                royalKnight.oathboundGambitUsed = false;
            }
        }
        isOathboundGambitActive = false;
        oathboundGambitEndTurn = 0;
        Debug.Log("[OathboundGambit] Usage reset for all Royal Knights in new battle");
    }

    // Check if Oathbound Gambit should end
    public static void CheckOathboundGambitExpiry()
    {
        if (isOathboundGambitActive)
        {
            Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
            if (game != null)
            {
                // Spawn 3 random lava tiles each turn during the duel
                SpawnRandomLavaTiles();
                
                // Check if duel should end
                if (game.GetTurnCount() >= oathboundGambitEndTurn)
                {
                    isOathboundGambitActive = false;
                    CleanupAllLavaTiles(game);
                    Debug.Log($"[OathboundGambit] Duel has ended on turn {game.GetTurnCount()}! Oathbound Gambit deactivated and lava cleaned up.");
                }
            }
        }
    }

    // Clean up all lava tiles when Oathbound Gambit ends
    private static void CleanupAllLavaTiles(Game game)
    {
        Debug.Log("[LavaCleanup] Cleaning up all lava tiles after Oathbound Gambit ends!");
        
        int lavaCleaned = 0;
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject pieceAtPos = game.GetPosition(x, y);
                if (pieceAtPos != null)
                {
                    Chessman chessman = pieceAtPos.GetComponent<Chessman>();
                    if (chessman != null && chessman.name == "tile_lava")
                    {
                        // Destroy lava tile
                        game.SetPositionEmpty(x, y);
                        Destroy(pieceAtPos);
                        lavaCleaned++;
                        Debug.Log($"[LavaCleanup] Lava tile destroyed at ({x},{y})");
                    }
                }
            }
        }
        
        Debug.Log($"[LavaCleanup] Cleanup complete! {lavaCleaned} lava tiles removed from battlefield.");
    }

    // Spawn 3 random lava tiles per turn during Oathbound Gambit
    public static void SpawnRandomLavaTiles()
    {
        if (!isOathboundGambitActive)
        {
            return; // Only spawn lava during Oathbound Gambit
        }

        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[LavaSpawn] Game reference not found!");
            return;
        }

        Debug.Log($"[LavaSpawn] Spawning 3 random lava tiles for turn {game.GetTurnCount()}!");

        // Find all empty tiles (no pieces, no elemental tiles, no mist knights)
        List<Vector2Int> emptyTiles = FindEmptyTilesForLava(game);
        
        if (emptyTiles.Count == 0)
        {
            Debug.Log("[LavaSpawn] No empty tiles available for lava spawning!");
            return;
        }

        // Spawn 3 random lava tiles
        int lavaToSpawn = Mathf.Min(3, emptyTiles.Count);
        for (int i = 0; i < lavaToSpawn; i++)
        {
            int randomIndex = Random.Range(0, emptyTiles.Count);
            Vector2Int lavaPos = emptyTiles[randomIndex];
            
            // Remove this position from available tiles to avoid duplicates
            emptyTiles.RemoveAt(randomIndex);
            
            // Create lava tile
            game.Create("tile_lava", lavaPos.x, lavaPos.y);
            Debug.Log($"[LavaSpawn] Lava tile created at ({lavaPos.x},{lavaPos.y}) - Royal Knight battlefield control!");
        }

        Debug.Log($"[LavaSpawn] {lavaToSpawn} lava tiles spawned - Royal Knight gains mobility advantage!");
    }

    private static List<Vector2Int> FindEmptyTilesForLava(Game game)
    {
        List<Vector2Int> emptyTiles = new List<Vector2Int>();

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject pieceAtPos = game.GetPosition(x, y);
                if (pieceAtPos == null)
                {
                    // Empty tile - valid for lava
                    emptyTiles.Add(new Vector2Int(x, y));
                }
                else
                {
                    Chessman chessman = pieceAtPos.GetComponent<Chessman>();
                    if (chessman != null)
                    {
                        // Check if it's an elemental tile or mist knight (skip these)
                        if (chessman.name.StartsWith("tile_") || 
                            chessman.name.Contains("mist_knight") ||
                            chessman.name.Contains("celestial_pillar"))
                        {
                            // Skip elemental tiles and mist knights - they block lava spawning
                            continue;
                        }
                        else
                        {
                            // Regular piece - skip this tile
                            continue;
                        }
                    }
                    else
                    {
                        // No chessman component - skip
                        continue;
                    }
                }
            }
        }

        Debug.Log($"[LavaSpawn] Found {emptyTiles.Count} empty tiles suitable for lava spawning");
        return emptyTiles;
    }
}
