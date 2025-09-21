using UnityEngine;
using System.Collections.Generic;

public class RoyalBishop : Pieces
{
    public GameObject movePlatePrefab;
    private Chessman chessman;
    private Game game;

    private void Awake()
    {
        // Cache Chessman reference (following Bishop pattern)
        chessman = GetComponent<Chessman>(); 
        if (chessman == null)
            Debug.LogError("[RoyalBishop] Missing Chessman component!");
            
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
    }

    // SoulRequiem Skill - Summon 2 Wraith Pawns in diagonal range
    public void SoulRequiem()
    {
        string player = "white"; // Royal Bishop is always white
        
        // Check cooldown (24 turns)
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "SoulRequiem"))
        {
            Debug.LogWarning("[SoulRequiem] Skill is on cooldown - cannot use.");
            return;
        }
        
        // Check SP cost (2 SP)
        if (!SkillManager.Instance.SpendPlayerSP(player, 2))
        { 
            Debug.LogWarning("[SoulRequiem] Not enough SP to cast.");
            return;
        }
        
        // Get the selected Royal Bishop (following existing Bishop pattern)
        RoyalBishop selectedRoyalBishop = null;
        if (UIManager.Instance != null && UIManager.Instance.selectedPiece != null)
        {
            GameObject selectedPiece = UIManager.Instance.selectedPiece;
            selectedRoyalBishop = selectedPiece.GetComponent<RoyalBishop>();
        }
        
        if (selectedRoyalBishop == null)
        {
            Debug.LogError("[SoulRequiem] No selected Royal Bishop found!");
            return;
        }
        
        // Remove existing move plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
        
        // Generate diagonal range plates (cast range)
        selectedRoyalBishop.GenerateSoulRequiemPlates();
        
        // Log skill usage
        if (SkillTracker.Instance != null)
        {
            SkillTracker.Instance.LogSkillUsage(player, "ROYAL BISHOP", "SOUL REQUIEM", 2);
        }
        
        // Start cooldown (24 turns)
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.StartCooldown(player, "SoulRequiem", CooldownManager.CooldownType.TurnBased, 24);
            Debug.Log("[SoulRequiem] Skill activated - now on cooldown for 24 turns!");
        }
        
        Debug.Log("[SoulRequiem] Skill activated - diagonal range plates generated!");
    }

    // Divinity Passive - gains 1 turn invulnerability when capturing pieces
    public void DivinityPassive(GameObject capturedPiece)
    {
        if (capturedPiece == null)
        {
            Debug.LogWarning("[Divinity] Captured piece is null!");
            return;
        }

        string player = chessman != null ? chessman.GetPlayer() : "white"; // Default to white for royal bishops

        // Check if Divinity is on cooldown (10 turns, shared across all Royal Bishops)
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "DivinityPassive"))
        {
            Debug.Log($"[Divinity] Passive is on cooldown for {player} - cannot gain invulnerability");
            return;
        }

        // Get game reference
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[Divinity] Cannot find Game component!");
            return;
        }

        // Add 1-turn invulnerability (starts next turn)
        Chessman royalBishopCm = GetComponent<Chessman>();
        if (royalBishopCm != null && royalBishopCm.statusManager != null)
        {
            int currentTurn = game.turns;
            int invulnerabilityEndTurn = currentTurn + 1; // 1 turn invulnerability starting next turn
            royalBishopCm.statusManager.AddStatus(StatusType.Invulnerable, invulnerabilityEndTurn);
            
            Debug.Log($"[Divinity] {player} Royal Bishop gained 1-turn invulnerability (until turn {invulnerabilityEndTurn}) for capturing {capturedPiece.name}");
            
            // Start 10-turn cooldown (shared across all Royal Bishops for this player)
            if (CooldownManager.Instance != null)
            {
                CooldownManager.Instance.StartCooldown(player, "DivinityPassive", CooldownManager.CooldownType.TurnBased, 10);
                Debug.Log($"[Divinity] 10-turn cooldown started for {player} Royal Bishops");
            }
            
            // Log skill usage
            if (SkillTracker.Instance != null)
            {
                SkillTracker.Instance.LogSkillUsage(player, "ROYAL BISHOP", "DIVINITY PASSIVE", 0); // 0 SP cost since it's passive
            }
        }
        else
        {
            Debug.LogError("[Divinity] Cannot find Chessman or StatusManager component on Royal Bishop!");
        }
    }
    
    // Generate diagonal range plates for wraith pawn summoning
    public void GenerateSoulRequiemPlates()
    {
        // Safety check for chessman reference
        if (chessman == null)
        {
            chessman = GetComponent<Chessman>();
            if (chessman == null)
            {
                Debug.LogError("[SoulRequiem] No Chessman component found!");
                return;
            }
        }
        
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        int x = chessman.GetXBoard();
        int y = chessman.GetYBoard();
        
        // Generate diagonal movement plates (like regular Bishop)
        RequiemLineMovePlate(1, 1);   // Up-Right
        RequiemLineMovePlate(-1, -1); // Down-Left
        RequiemLineMovePlate(-1, 1);  // Up-Left
        RequiemLineMovePlate(1, -1);  // Down-Right
        
        Debug.Log("[SoulRequiem] Requiem plates generated in diagonal range!");
    }
    
    // Helper method for requiem line movement (diagonal range)
    private void RequiemLineMovePlate(int xIncrement, int yIncrement)
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        int x = chessman.GetXBoard();
        int y = chessman.GetYBoard();
        
        while (game.PositionOnBoard(x + xIncrement, y + yIncrement))
        {
            x += xIncrement;
            y += yIncrement;
            
            GameObject target = game.GetPosition(x, y);
            if (target == null)
            {
                // Empty tile - can summon wraith pawn here
                SpawnRequiemPlate(x, y);
            }
            else
            {
                // Occupied tile - continue to check for empty tiles behind enemy
                continue;
            }
        }
    }
    
    // Helper method to spawn requiem plates
    private void SpawnRequiemPlate(int matrixX, int matrixY)
    {
        float x = matrixX * 0.57f - 1.98f;
        float y = matrixY * 0.56f - 1.95f;
        
        GameObject mp = Instantiate(movePlatePrefab, new Vector3(x, y, -3f), Quaternion.identity);
        
        // Remove default MovePlate script
        MovePlate oldScript = mp.GetComponent<MovePlate>();
        if (oldScript != null) Destroy(oldScript);
        
        // Add RequiemPlate script
        RequiemPlate requiemScript = mp.AddComponent<RequiemPlate>();
        requiemScript.Setup(gameObject, matrixX, matrixY);
        
        // Make requiem plates visually distinct (purple color)
        SpriteRenderer sr = mp.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.magenta;
        }
    }
}
