using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBishop : MonoBehaviour
{
    private Game game;
    private Chessman chessman;
   
    // Worldfire Ring tracking
    private static int worldfireRingEndTurn = -1;
    private static int worldfireRingStartTurn = -1;
    private static Chessman worldfireRingCaster = null;
    
    // Eternal Flame tracking
    private static bool eternalFlameTriggered = false;
    private static int altarEndTurn = -1;
    
    // Phoenix Pyre tracking
    private static bool phoenixPyreTriggered = false;
    
    void Start()
    {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        chessman = GetComponent<Chessman>();
    }
    
    /// <summary>
    /// Worldfire Ring - Fire Bishop becomes stunned and creates expanding fire aura
    /// </summary>
    public void WorldfireRing()
    {
        // Find the actual FireBishop piece on the board (not the script GameObject)
        Chessman fireBishopChessman = FindFireBishopPiece();
        if (fireBishopChessman == null)
        {
            Debug.LogError("[WorldfireRing] Could not find FireBishop piece on board!");
            return;
        }
        
        string player = fireBishopChessman.GetPlayer();
        
        // Check SP cost
        if (SkillManager.Instance.GetPlayerSP(player) < 2)
        {
            Debug.Log("[WorldfireRing] Not enough SP!");
            return;
        }
        
        // Check if Worldfire Ring is on cooldown
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "WorldfireRing"))
        {
            Debug.Log("[WorldfireRing] Skill is on cooldown!");
            return;
        }
        
        // Deduct SP
        SkillManager.Instance.SpendPlayerSP(player, 2);
        
        // Apply stunned status to Fire Bishop (5 turns)
        fireBishopChessman.statusManager.AddStatus(StatusType.Stunned, game.turns + 5);
        fireBishopChessman.UpdateVisualStatus();
        
        // Set Worldfire Ring tracking
        worldfireRingStartTurn = game.turns;
        worldfireRingEndTurn = game.turns + 5;
        worldfireRingCaster = fireBishopChessman;
        
        // Start cooldown (30 turns)
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.StartCooldown(player, "WorldfireRing", CooldownManager.CooldownType.TurnBased, 30);
        }
        
        Debug.Log($"[WorldfireRing] Activated! Fire Bishop is stunned for 5 turns. Fire aura will expand each turn!");
        
        // Trigger initial 3x3 aura
        TriggerFireAura(3);
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
        // End turn
        game.NextTurn();
    }
    
    /// <summary>
    /// Phoenix Pyre - Destroys Fire Bishop and triggers EternalFlame immediately with enhanced resurrection
    /// </summary>
    public void PhoenixPyre()
    {
        // Find the actual FireBishop piece on the board (not the script GameObject)
        Chessman fireBishopChessman = FindFireBishopPiece();
        if (fireBishopChessman == null)
        {
            Debug.LogError("[PhoenixPyre] Could not find FireBishop piece on board!");
            return;
        }
        
        string player = fireBishopChessman.GetPlayer();
        
        // Check SP cost (3 SP)
        if (SkillManager.Instance.GetPlayerSP(player) < 3)
        {
            Debug.Log("[PhoenixPyre] Not enough SP!");
            return;
        }
        
        // Check if Phoenix Pyre is on cooldown (15 turns)
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "PhoenixPyre"))
        {
            Debug.Log("[PhoenixPyre] Skill is on cooldown!");
            return;
        }
        
        // Deduct SP
        SkillManager.Instance.SpendPlayerSP(player, 3);
        
        // Start Phoenix Pyre cooldown (15 turns)
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.StartCooldown(player, "PhoenixPyre", CooldownManager.CooldownType.TurnBased, 15);
        }
        
        // Get Fire Bishop position
        int fireBishopX = fireBishopChessman.GetXBoard();
        int fireBishopY = fireBishopChessman.GetYBoard();
        
        // Mark Phoenix Pyre as triggered (bypasses EternalFlame cooldown)
        phoenixPyreTriggered = true;
        eternalFlameTriggered = true;
        
        // Set altar end turn to 3 turns from now (enhanced resurrection)
        altarEndTurn = game.turns + 3;
        
        // Destroy the Fire Bishop
        game.SetPositionEmpty(fireBishopX, fireBishopY);
        Destroy(fireBishopChessman.gameObject);
        
        // Create ashen pyre immediately at the same position
        GameObject ashenPyre = game.Create("white_ashen_pyre", fireBishopX, fireBishopY);
        if (ashenPyre != null)
        {
            Debug.Log($"[PhoenixPyre] 🔥🔥🔥 PHOENIX PYRE ACTIVATED! Fire Bishop sacrificed and ashen pyre created at ({fireBishopX},{fireBishopY})! Enhanced resurrection in 3 turns (turn {altarEndTurn}).");
        }
        else
        {
            Debug.LogError($"[PhoenixPyre] Failed to create ashen pyre at ({fireBishopX},{fireBishopY})!");
        }
        
        // Remove all move plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
            
        // End turn
        game.NextTurn();
    }
    
    /// <summary>
    /// Trigger fire aura with specified size (3x3, 5x5, or 7x7)
    /// </summary>
    private void TriggerFireAura(int auraSize)
    {
        if (worldfireRingCaster == null) return;
        
        int centerX = worldfireRingCaster.GetXBoard();
        int centerY = worldfireRingCaster.GetYBoard();
        int radius = auraSize / 2; // 1, 2, or 3
        
        int crippledCount = 0;
        
        // Check all tiles within the aura
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (dx == 0 && dy == 0) continue; // Skip center (Fire Bishop)
                
                int checkX = centerX + dx;
                int checkY = centerY + dy;
                
                // Check if position is within board bounds
                if (checkX >= 0 && checkX < 8 && checkY >= 0 && checkY < 8)
                {
                    GameObject pieceAtPos = game.GetPosition(checkX, checkY);
                    if (pieceAtPos != null)
                    {
                        Chessman enemyChessman = pieceAtPos.GetComponent<Chessman>();
                        if (enemyChessman != null && enemyChessman.GetPlayer() != worldfireRingCaster.GetPlayer())
                        {
                            // Apply crippled status (2 turns duration)
                            enemyChessman.statusManager.AddStatus(StatusType.Crippled, game.turns + 2);
                            enemyChessman.UpdateVisualStatus();
                            crippledCount++;
                        }
                    }
                }
            }
        }
        
        Debug.Log($"[WorldfireRing] {auraSize}x{auraSize} fire aura triggered! {crippledCount} enemies crippled.");
    }
    
    /// <summary>
    /// Check if Worldfire Ring is active and trigger appropriate aura
    /// </summary>
    public static void CheckWorldfireRingExpansion(int currentTurn)
    {
        if (worldfireRingEndTurn == -1 || currentTurn > worldfireRingEndTurn || worldfireRingCaster == null)
        {
            // Reset tracking if expired
            if (currentTurn > worldfireRingEndTurn)
            {
                worldfireRingEndTurn = -1;
                worldfireRingStartTurn = -1;
                worldfireRingCaster = null;
                Debug.Log("[WorldfireRing] Period ended.");
            }
            return;
        }
        
        // Calculate which turn of the 5-turn period we're in
        int turnInPeriod = currentTurn - worldfireRingStartTurn;
        
        // Trigger appropriate aura based on turn
        FireBishop fireBishopScript = worldfireRingCaster.GetComponent<FireBishop>();
        if (fireBishopScript != null)
        {
            switch (turnInPeriod)
            {
                case 1: // Turn 2 (after first turn)
                    fireBishopScript.TriggerFireAura(5); // 5x5 aura
                    break;
                case 3: // Turn 4 (after third turn)
                    fireBishopScript.TriggerFireAura(7); // 7x7 aura
                    break;
            }
        }
    }
    
    /// <summary>
    /// Find the actual FireBishop piece on the board
    /// </summary>
    private Chessman FindFireBishopPiece()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = game.GetPosition(x, y);
                if (piece != null && piece.name.ToLower().Contains("fire_bishop"))
                {
                    Chessman chessman = piece.GetComponent<Chessman>();
                    if (chessman != null)
                    {
                        return chessman;
                    }
                }
            }
        }
        return null;
    }
    
    /// <summary>
    /// Try to trigger Eternal Flame passive when Fire Bishop is destroyed
    /// </summary>
    public bool TryTriggerEternalFlame()
    {
        string player = "white"; // Fire Bishop is always white
        
        // Phoenix Pyre bypasses cooldown restrictions
        if (phoenixPyreTriggered)
        {
            Debug.Log("[EternalFlame] Phoenix Pyre triggered - bypassing cooldown restrictions!");
            return true; // Phoenix Pyre already handled the triggering
        }
        
        // Check if Eternal Flame is on cooldown (30 turns) - only for normal destruction
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "EternalFlame"))
        {
            Debug.Log("[EternalFlame] Still on cooldown - cannot trigger!");
            return false;
        }
        
        // Only trigger once per battle
        if (eternalFlameTriggered)
        {
            Debug.Log("[EternalFlame] Already triggered this battle!");
            return false;
        }
        
        eternalFlameTriggered = true;
        Debug.Log("[EternalFlame] Fire Bishop destroyed - Eternal Flame will activate next turn!");
        
        // Start 30-turn cooldown
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.StartCooldown(player, "EternalFlame", CooldownManager.CooldownType.TurnBased, 30);
        }
        
        // Eternal Flame will be triggered at the start of next turn
        return true;
    }
    
    /// <summary>
    /// Eternal Flame - Create summon plates on empty tiles at start of turn
    /// </summary>
    public static void TriggerEternalFlame()
    {
        if (!eternalFlameTriggered) return;
        
        // If Phoenix Pyre was triggered, the ashen pyre is already created
        // Don't create summon plates - just reset the trigger
        if (phoenixPyreTriggered)
        {
            Debug.Log("[EternalFlame] Phoenix Pyre already created ashen pyre - skipping summon plate creation!");
            eternalFlameTriggered = false; // Reset trigger to prevent further calls
            return;
        }
        
        Debug.Log("[EternalFlame] 🔥 ETERNAL FLAME ACTIVATED! Creating altar summon plates...");
        
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[EternalFlame] Could not find Game component!");
            return;
        }
        
        // Find all empty tiles on the board
        int emptyTileCount = 0;
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = game.GetPosition(x, y);
                if (piece == null)
                {
                    // Create summon plate for empty tile
                    CreateAltarSummonPlate(x, y);
                    emptyTileCount++;
                }
            }
        }
        
        Debug.Log($"[EternalFlame] Created {emptyTileCount} altar summon plates on empty tiles.");
    }
    
    /// <summary>
    /// Create a summon plate for altar placement
    /// </summary>
    private static void CreateAltarSummonPlate(int x, int y)
    {
        // Use the same positioning system as Knight.MomentumPlate
        float fx = x * 0.57f - 1.98f;
        float fy = y * 0.56f - 1.95f;
        
        GameObject plate = Instantiate(GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>().movePlatePrefabReference);
        plate.transform.position = new Vector3(fx, fy, -3f);
        
        // Set up the plate for altar summoning
        MovePlate mp = plate.GetComponent<MovePlate>();
        if (mp != null)
        {
            mp.SetReference(null); // No piece reference for altar summoning
            mp.SetCoords(x, y);
            
            // Use MovePlate tag but add a custom component to identify altar summon plates
            plate.tag = "MovePlate";
            
            // Add AltarSummonPlate component to identify this as an altar summon plate
            AltarSummonPlate altarPlate = plate.AddComponent<AltarSummonPlate>();
            altarPlate.Setup(x, y);
            
            // Change color to indicate altar summoning
            plate.GetComponent<SpriteRenderer>().color = new Color(1f, 0.5f, 0f, 0.75f); // Orange with transparency
            
            Debug.Log($"[EternalFlame] Created altar summon plate at ({x},{y}) with tag: {plate.tag}");
        }
        else
        {
            Debug.LogError($"[EternalFlame] Failed to get MovePlate component for altar summon plate at ({x},{y})!");
        }
    }
    
    /// <summary>
    /// Create altar at specified position
    /// </summary>
    public static void CreateAltar(int x, int y)
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[EternalFlame] Could not find Game component for altar creation!");
            return;
        }
        
        // Create the ashen pyre piece
        GameObject ashenPyre = game.Create("white_ashen_pyre", x, y);
        if (ashenPyre != null)
        {
            // Set ashen pyre duration (5 turns from now)
            altarEndTurn = game.turns + 5;
            
            // Don't add special tile status - let it behave as a normal white piece
            // The ashen pyre should be attackable by enemies like any other piece
            
            Debug.Log($"[EternalFlame] 🔥 ASHEN PYRE CREATED at ({x},{y})! Resurrection in 5 turns (turn {altarEndTurn}). Ashen pyre can be attacked by enemies.");
            
            // Reset eternal flame trigger
            eternalFlameTriggered = false;
        }
        else
        {
            Debug.LogError($"[EternalFlame] Failed to create altar at ({x},{y})!");
        }
    }
    
    /// <summary>
    /// Check altar expiration and resurrect Fire Bishop
    /// </summary>
    public static void CheckAltarReborn(int currentTurn)
    {
        if (altarEndTurn == -1 || currentTurn < altarEndTurn) return;
        
        Debug.Log("[EternalFlame] Checking for altar resurrection...");
        
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[EternalFlame] Could not find Game component for resurrection check!");
            return;
        }
        
        // Find ashen pyre on the board
        GameObject ashenPyre = null;
        int pyreX = -1, pyreY = -1;
        
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = game.GetPosition(x, y);
                if (piece != null && piece.name == "white_ashen_pyre")
                {
                    ashenPyre = piece;
                    pyreX = x;
                    pyreY = y;
                    break;
                }
            }
            if (ashenPyre != null) break;
        }
        
        if (ashenPyre != null)
        {
            // Ashen pyre still exists - resurrect Fire Bishop using ElementalBishop.Invocation pattern
            Debug.Log($"[EternalFlame] 🔥🔥🔥 ASHEN PYRE REBORN! Resurrecting Fire Bishop at ({pyreX},{pyreY})!");
            
            // ✅ EXACT ElementalBishop.Invocation pattern: Clear position, destroy ashen pyre, create new piece
            // Step 1: Get reference to the ashen pyre BEFORE clearing position
            GameObject pyrePiece = game.GetPosition(pyreX, pyreY);
            
            // Step 2: Clear the current position
            game.SetPositionEmpty(pyreX, pyreY);
            
            // Step 3: Destroy the ashen pyre
            if (pyrePiece != null)
            {
                Destroy(pyrePiece);
                Debug.Log($"[EternalFlame] Destroyed ashen pyre at ({pyreX},{pyreY})");
            }
            
            // Step 4: Create the new Fire Bishop at the same position
            GameObject newFireBishop = game.Create("white_fire_bishop", pyreX, pyreY);
            if (newFireBishop != null)
            {
                // Check if this was a Phoenix Pyre resurrection for enhanced abilities
                if (phoenixPyreTriggered)
                {
                    // Add Phoenix Resurrection status for enhanced movement abilities
                    StatusManager statusManager = newFireBishop.GetComponent<StatusManager>();
                    if (statusManager != null)
                    {
                        statusManager.AddStatus(StatusType.PhoenixResurrection, 999); // Permanent enhanced abilities
                        Debug.Log($"[EternalFlame] 🔥🔥🔥 PHOENIX RESURRECTION! Fire Bishop resurrected with enhanced movement abilities (combined SurroundMovePlate + LineMovePlate)!");
                    }
                    
                    // Reset Phoenix Pyre trigger
                    phoenixPyreTriggered = false;
                }
                else
                {
                    Debug.Log($"[EternalFlame] Fire Bishop successfully resurrected at ({pyreX},{pyreY})!");
                }
            }
            else
            {
                Debug.LogError($"[EternalFlame] Failed to resurrect Fire Bishop at ({pyreX},{pyreY})!");
            }
            
            // Reset altar tracking
            altarEndTurn = -1;
        }
        else
        {
            // Ashen pyre was destroyed - resurrection failed
            Debug.Log("[EternalFlame] Ashen pyre was destroyed - resurrection failed!");
            altarEndTurn = -1;
        }
    }
}
