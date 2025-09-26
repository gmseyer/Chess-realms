using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : MonoBehaviour
{
    private Chessman chessman;
    private Game game;
    public GameObject movePlatePrefab;
    
    private void Awake()
    {
        // Cache Chessman reference
        chessman = GetComponent<Chessman>(); 
        if (chessman == null)
            Debug.LogError("[Pawn] Missing Chessman component!");
            
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
    }

    // RoyalAcolyte Promotion - Check if pawn is on last rank
    public void RoyalAcolyte()
    {
        if (chessman == null)
        {
            Debug.LogError("[RoyalAcolyte] Missing Chessman reference!");
            return;
        }

        if (game == null)
        {
            Debug.LogError("[RoyalAcolyte] Missing Game reference!");
            return;
        }

        string player = chessman.GetPlayer();
        int currentY = chessman.GetYBoard();
        
        // Check if pawn is on last rank
        bool isOnLastRank = false;
        
        if (player == "white" && currentY == 7)
        {
            isOnLastRank = true;
        }
        else if (player == "black" && currentY == 0)
        {
            isOnLastRank = true;
        }

        // Check requirements and SP cost
        if (isOnLastRank)
        {
            // Check if player already has a royal pawn
            if (PlayerHasRoyalPawn(player))
            {
                Debug.Log($"[RoyalAcolyte] Cannot promote - {player} player already has 1 royal pawn. Only 1 royal pawn allowed per player.");
                return;
            }
            
            // Check SP cost (2 SP)
            if (!SkillManager.Instance.SpendPlayerSP(player, 2))
            { 
                Debug.LogWarning($"[RoyalAcolyte] Not enough SP to promote {player} pawn. Need 2 SP.");
                return;
            }
            
            Debug.Log($"[RoyalAcolyte] Can be pressed - {player} pawn is on last rank at y={currentY}");
            
            // Perform the promotion
            PromoteToRoyalPawn();
        }
        else
        {
            Debug.Log($"[RoyalAcolyte] Cannot press - {player} pawn needs to reach last rank (currently at y={currentY})");
        }
    }

    private bool PlayerHasRoyalPawn(string player)
    {
        // Find all royal pawns on the board
        Chessman[] allPieces = FindObjectsOfType<Chessman>();
        
        foreach (Chessman piece in allPieces)
        {
            if (piece != null && piece.GetPlayer() == player)
            {
                string pieceName = piece.name.ToLower();
                if (pieceName.Contains("royal_pawn"))
                {
                    Debug.Log($"[RoyalAcolyte] Found existing {player} royal pawn: {piece.name}");
                    return true;
                }
            }
        }
        
        return false;
    }

    private void PromoteToRoyalPawn() 
    {
        if (chessman == null || game == null)
        {
            Debug.LogError("[RoyalAcolyte] Missing references for promotion!");
            return;
        }

        string player = chessman.GetPlayer();
        int x = chessman.GetXBoard();
        int y = chessman.GetYBoard();
        
        // Determine the royal pawn name based on player
        string royalPawnName = (player == "white") ? "white_royal_pawn" : "black_royal_pawn";
        
        Debug.Log($"[RoyalAcolyte] Promoting {player} pawn at ({x},{y}) to {royalPawnName}");
        
        // Clear the current pawn's position
        game.SetPositionEmpty(x, y);
        
        // Destroy the current pawn
        Destroy(gameObject);
        
        // Create the royal pawn at the same position
        GameObject royalPawn = game.Create(royalPawnName, x, y);
        if (royalPawn != null)
        {
            Debug.Log($"[RoyalAcolyte] Successfully promoted to {royalPawnName} at ({x},{y})");
            
            // Trigger Echo() passive after successful promotion
            RoyalAcolyte royalAcolyte = royalPawn.GetComponent<RoyalAcolyte>();
            if (royalAcolyte != null)
            {
                royalAcolyte.Echo();
            }
            else
            {
                Debug.LogError($"[RoyalAcolyte] RoyalAcolyte component not found on {royalPawnName}!");
            }
        }
        else
        {
            Debug.LogError($"[RoyalAcolyte] Failed to create {royalPawnName} at ({x},{y})");
        }
    }

    // Pawn's Gambit skill - public method for UI button
    public void PawnsGambit()
    {
        if (game == null)
        {
            Debug.LogError("[PawnsGambit] Missing Game reference!");
            return;
        }

        string player = game.GetCurrentPlayer();
        
        // Check if royal acolyte is on board - if so, disable pawn skills
        if (global::RoyalAcolyte.IsRoyalAcolyteOnBoard(player))
        {
            Debug.Log($"[Pawn's Gambit] Cannot use - {player} royal acolyte is on the board! Pawn skills are disabled.");
            return;
        }
        
        // Check if SkillManager is available
        if (SkillManager.Instance == null)
        {
            Debug.LogError("[PawnsGambit] SkillManager instance not found!");
            return;
        }

        // Execute Pawn's Gambit skill
        bool success = SkillManager.Instance.ExecutePawnsGambit(player);
        
        if (success)
        {
            Debug.Log($"[PawnsGambit] {player} successfully used Pawn's Gambit!");
        }
        else
        {
            Debug.Log($"[PawnsGambit] {player} failed to use Pawn's Gambit!");
        }
    }

    // Russian Roulette skill - public method for UI button
    public void RussianRoulette()
    {
        if (game == null) 
        {
            Debug.LogError("[RussianRoulette] Missing Game reference!");
            return;
        }

        string player = game.GetCurrentPlayer();
        
        // Check if royal acolyte is on board - if so, disable pawn skills
        if (global::RoyalAcolyte.IsRoyalAcolyteOnBoard(player))
        {
            Debug.Log($"[Russian Roulette] Cannot use - {player} royal acolyte is on the board! Pawn skills are disabled.");
            return;
        }

        // Get Chessman reference - try multiple methods
        if (chessman == null)
        {
            chessman = GetComponent<Chessman>();
        }
        
        // If still null, try to get it from the selected piece
        if (chessman == null && UIManager.Instance != null && UIManager.Instance.selectedPiece != null)
        {
            chessman = UIManager.Instance.selectedPiece.GetComponent<Chessman>();
        }
        
        // If still null, try to find any pawn on the board
        if (chessman == null)
        {
            Chessman[] allPieces = FindObjectsOfType<Chessman>();
            foreach (Chessman piece in allPieces)
            {
                if (piece != null && piece.name.Contains("pawn") && !piece.name.Contains("royal_pawn") && !piece.name.Contains("wraith_pawn"))
                {
                    string currentPlayer = game.GetCurrentPlayer();
                    if (piece.GetPlayer() == currentPlayer)
                    {
                        chessman = piece;
                        break;
                    }
                }
            }
        }
        
        if (chessman == null)
        {
            Debug.LogError("[RussianRoulette] Could not find a valid pawn to use Russian Roulette!");
            return;
        }
        
        // Check if SkillManager is available
        if (SkillManager.Instance == null)
        {
            Debug.LogError("[RussianRoulette] SkillManager instance not found!");
            return;
        }

        // Check SP cost (1 SP)
        if (!SkillManager.Instance.SpendPlayerSP(player, 1))
        {
            Debug.LogWarning("[Russian Roulette] Not enough SP to use Russian Roulette. Need 1 SP.");
            return;
        }

        // Check cooldown (15 turns)
        if (SkillManager.Instance.IsSkillOnCooldown(player, SkillType.RussianRoulette))
        {
            Debug.LogWarning("[Russian Roulette] Skill is on cooldown!");
            // Refund SP since we can't use the skill
            SkillManager.Instance.AddPlayerSP(player, 1);
            return;
        }

        // Start cooldown
        SkillManager.Instance.StartCooldown(player, SkillType.RussianRoulette, 15);

        // Roll the dice! (1-6)
        int effect = Random.Range(1,6);
        Debug.Log($"[Russian Roulette] {player} rolled effect {effect}!");

        switch (effect)
        {
            case 1:
                Effect1_DestroySelf();
                break;
            case 2:
                Effect2_DestroyEnemyPawn();
                break;
            case 3:
                Effect3_StunEnemyPawns();
                break;
            case 4:
                Effect4_BountyAlliedPawns();
                break;
            case 5:
                Effect5_TransformToRook();
                break;
            case 6:
                Effect6_KingMovement();
                break;
        }

        
            

    }

    private void Effect1_DestroySelf()
    {
        Debug.Log("[Russian Roulette] Effect 1: Destroy the pawn itself!");
        
        if (chessman == null)
        {
            Debug.LogError("[Russian Roulette] Cannot destroy self - no chessman reference!");
            game.NextTurn();
            return;
        }
        
        int x = chessman.GetXBoard();
        int y = chessman.GetYBoard();
        
        // Clear position and destroy the chessman's gameObject
        game.SetPositionEmpty(x, y);
        Destroy(chessman.gameObject);
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
        // End turn
        game.NextTurn();
    }

    private void Effect2_DestroyEnemyPawn()
    {
        Debug.Log("[Russian Roulette] Effect 2: Destroy 1 pawn on enemy side!");
        
        if (chessman == null)
        {
            Debug.LogError("[Russian Roulette] Cannot destroy enemy pawn - no chessman reference!");
            game.NextTurn();
            return;
        }
         
        // Find all enemy pawns (regular pawns only)
        Chessman[] allPieces = FindObjectsOfType<Chessman>();
        List<Chessman> enemyPawns = new List<Chessman>();
        
        Debug.Log($"[Russian Roulette] Searching for enemy pawns. Current player: {chessman.GetPlayer()}");
        
        foreach (Chessman piece in allPieces)
        {
            if (piece != null)
            {
                Debug.Log($"[Russian Roulette] Checking piece: {piece.name}, Player: {piece.GetPlayer()}");
                
                // Check if it's a regular pawn (not royal or wraith)
                if (piece.name.Contains("pawn") && !piece.name.Contains("royal_pawn") && !piece.name.Contains("wraith_pawn"))
                {
                    Debug.Log($"[Russian Roulette] Found regular pawn: {piece.name}, Player: {piece.GetPlayer()}");
                    if (piece.GetPlayer() != chessman.GetPlayer())
                    {
                        enemyPawns.Add(piece);
                        Debug.Log($"[Russian Roulette] Added enemy pawn: {piece.name}");
                    }
                }
            }
        }
        
        Debug.Log($"[Russian Roulette] Found {enemyPawns.Count} enemy pawns");
        
        if (enemyPawns.Count == 0)
        {
            Debug.Log("[Russian Roulette] No enemy pawns found to destroy!");
            game.NextTurn();
            return;
        }
        
        // Generate target plates for all enemy pawns
        GenerateEnemyPawnTargetPlates(enemyPawns);
    }

    private void Effect3_StunEnemyPawns()
    {
        Debug.Log("[Russian Roulette] Effect 3: Stun random 3 enemy pawns for 1 turn!");
        
        // Find all enemy pawns (regular pawns only)
        Chessman[] allPieces = FindObjectsOfType<Chessman>();
        List<Chessman> enemyPawns = new List<Chessman>();
        
        foreach (Chessman piece in allPieces)
        {
            if (piece != null)
            {
                // Check if it's a regular pawn (not royal or wraith) and is enemy
                if (piece.name.Contains("pawn") && !piece.name.Contains("royal_pawn") && !piece.name.Contains("wraith_pawn"))
                {
                    if (piece.GetPlayer() != chessman.GetPlayer())
                    {
                        enemyPawns.Add(piece);
                        Debug.Log($"[Russian Roulette] Found enemy pawn: {piece.name}");
                    }
                }
            }
        }
        
        Debug.Log($"[Russian Roulette] Found {enemyPawns.Count} enemy pawns to potentially stun");
        
        // Select up to 3 random enemy pawns
        int count = Mathf.Min(3, enemyPawns.Count);
        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, enemyPawns.Count);
            Chessman selectedPawn = enemyPawns[randomIndex];
            enemyPawns.RemoveAt(randomIndex);
            
            // Add stunned status for 2 turns (1 turn + current turn)
            selectedPawn.statusManager.AddStatus(StatusType.Stunned, game.turns + 2);
            Debug.Log($"[Russian Roulette] Stunned {selectedPawn.name} for 2 turns!");
        }
        
        // End turn
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        game.NextTurn();
    }

    private void Effect4_BountyAlliedPawns()
    {
        Debug.Log("[Russian Roulette] Effect 4: Put Bounty 1 SP on 3 random allied pawns!");
        
        // Find all allied pawns (regular pawns only)
        Chessman[] allPieces = FindObjectsOfType<Chessman>();
        List<Chessman> alliedPawns = new List<Chessman>();
        
        foreach (Chessman piece in allPieces)
        {
            if (piece != null)
            {
                // Check if it's a regular pawn (not royal or wraith) and is allied
                if (piece.name.Contains("pawn") && !piece.name.Contains("royal_pawn") && !piece.name.Contains("wraith_pawn"))
                {
                    if (piece.GetPlayer() == chessman.GetPlayer())
                    {
                        alliedPawns.Add(piece);
                        Debug.Log($"[Russian Roulette] Found allied pawn: {piece.name}");
                    }
                }
            }
        }
        
        Debug.Log($"[Russian Roulette] Found {alliedPawns.Count} allied pawns to potentially give bounty");
        
        // Select up to 3 random allied pawns
        int count = Mathf.Min(3, alliedPawns.Count);
        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, alliedPawns.Count);
            Chessman selectedPawn = alliedPawns[randomIndex];
            alliedPawns.RemoveAt(randomIndex);
            
            // Add bounty status for 5 turns
            selectedPawn.statusManager.AddBountyStatus(1, game.turns + 5);
            Debug.Log($"[Russian Roulette] Added bounty to {selectedPawn.name} for 5 turns!");
        }
        
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // End turn
        game.NextTurn();
    }

    private void Effect5_TransformToRook()
    {
        Debug.Log("[Russian Roulette] Effect 5: Transform into rook!");
        
        if (chessman == null)
        {
            Debug.LogError("[Russian Roulette] Cannot transform - no chessman reference!");
            game.NextTurn();
            return;
        }
        
        string player = chessman.GetPlayer();
        int x = chessman.GetXBoard();
        int y = chessman.GetYBoard();
        
        // Determine the rook name based on player (regular rook only)
        string rookName = (player == "white") ? "white_rook" : "black_rook";
        
        // Clear the current pawn's position
        game.SetPositionEmpty(x, y);
        
        // Destroy the current pawn
        Destroy(chessman.gameObject);
        
        // Create the regular rook at the same position
        GameObject rook = game.Create(rookName, x, y);
        if (rook != null)
        {
            Debug.Log($"[Russian Roulette] Successfully transformed to regular {rookName} at ({x},{y})");
        }
        else
        {
            Debug.LogError($"[Russian Roulette] Failed to create {rookName} at ({x},{y})");
        }
        
         foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
        // End turn
       // game.NextTurn();
    }

    private void Effect6_KingMovement()
    {
        Debug.Log("[Russian Roulette] Effect 6: Gain King movements for 3 turns!");
        
        // Add KingMovement status for 3 turns
        chessman.statusManager.AddStatus(StatusType.KingMovement, game.turns + 3);
        
        // Activate the King Movement function
        chessman.ActivateKingMovement();
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
    chessman.UpdateVisualStatus();
        // End turn
        //game.NextTurn();
    }

    private void GenerateEnemyPawnTargetPlates(List<Chessman> enemyPawns)
    {
        Debug.Log($"[Russian Roulette] Starting to generate target plates for {enemyPawns.Count} enemy pawns");
        
        // Clear existing move plates
        GameObject[] existingPlates = GameObject.FindGameObjectsWithTag("MovePlate");
        Debug.Log($"[Russian Roulette] Found {existingPlates.Length} existing move plates to clear");
        foreach (GameObject plate in existingPlates)
            Destroy(plate);

        // Ensure movePlatePrefab is assigned
        if (movePlatePrefab == null)
        {
            Debug.LogError("[Russian Roulette] MovePlate prefab is null! Cannot create target plates.");
            return;
        }
        
        Debug.Log($"[Russian Roulette] Using movePlatePrefab: {movePlatePrefab.name}");

        // Generate target plates for each enemy pawn using the same method as other skills
        foreach (Chessman pawn in enemyPawns)
        {
            int x = pawn.GetXBoard();
            int y = pawn.GetYBoard();
            
            Debug.Log($"[Russian Roulette] Creating target plate for {pawn.name} at ({x},{y})");
            
            // Use the same positioning as other move plates
            float fx = x * 0.57f - 1.98f;
            float fy = y * 0.56f - 1.95f;

            // Create move plate using the assigned prefab
            GameObject mp = Instantiate(movePlatePrefab, new Vector3(fx, fy, -3f), Quaternion.identity);
            Debug.Log($"[Russian Roulette] Instantiated move plate at ({fx},{fy}) - GameObject: {mp.name}, Active: {mp.activeInHierarchy}");

            // Check if the object is immediately destroyed
            if (mp == null)
            {
                Debug.LogError($"[Russian Roulette] Move plate was immediately destroyed for {pawn.name}!");
                continue;
            }

            // Keep the MovePlate tag but add a custom component to identify our target plates
            Debug.Log($"[Russian Roulette] Keeping MovePlate tag but adding custom identifier");

            // Remove default MovePlate script
            MovePlate oldScript = mp.GetComponent<MovePlate>();
            if (oldScript != null) 
            {
                Destroy(oldScript);
                Debug.Log("[Russian Roulette] Removed default MovePlate script");
            }

            // Add RussianRouletteTargetPlate script
            RussianRouletteTargetPlate plate = mp.AddComponent<RussianRouletteTargetPlate>();
            plate.Setup(game, x, y, pawn.name, this);
            Debug.Log($"[Russian Roulette] Added RussianRouletteTargetPlate script");

            // Make target plates visually distinct (red)
            SpriteRenderer sr = mp.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.red;
                Debug.Log($"[Russian Roulette] Set target plate color to red");
            }
            else
            {
                Debug.LogWarning("[Russian Roulette] No SpriteRenderer found on move plate!");
            }
            
            // Check if the object is still alive after all modifications
            if (mp != null)
            {
                Debug.Log($"[Russian Roulette] Move plate for {pawn.name} is still alive after setup");
            }
            else
            {
                Debug.LogError($"[Russian Roulette] Move plate for {pawn.name} was destroyed during setup!");
            }
        }
        
        Debug.Log($"[Russian Roulette] Target plates generated for {enemyPawns.Count} enemy pawns! Select an enemy pawn to destroy.");
        
        // Check if plates are still there after a short delay
        StartCoroutine(CheckPlatesAfterDelay());
    }
    
    private System.Collections.IEnumerator CheckPlatesAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        
        // Check if plates are still there (look for plates with our custom component)
        GameObject[] allPlates = GameObject.FindGameObjectsWithTag("MovePlate");
        int targetPlateCount = 0;
        
        foreach (GameObject plate in allPlates)
        {
            if (plate != null)
            {
                RussianRouletteTargetPlate targetScript = plate.GetComponent<RussianRouletteTargetPlate>();
                if (targetScript != null)
                {
                    targetPlateCount++;
                    Debug.Log($"[Russian Roulette] Found target plate: {plate.name}, Active: {plate.activeInHierarchy}, Position: {plate.transform.position}");
                }
            }
        }
        
        Debug.Log($"[Russian Roulette] After delay: Found {targetPlateCount} Russian Roulette target plates in scene");
    }
}
