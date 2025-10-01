using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FireBishop : MonoBehaviour
{
    private Game game;
    private Chessman chessman;
   
    // ✅ Player-aware Worldfire Ring tracking
    private static Dictionary<string, int> worldfireRingEndTurn = new Dictionary<string, int>();
    private static Dictionary<string, int> worldfireRingStartTurn = new Dictionary<string, int>();
    private static Dictionary<string, Chessman> worldfireRingCaster = new Dictionary<string, Chessman>();
    
    // ✅ Player-aware Eternal Flame tracking
    private static Dictionary<string, bool> eternalFlameTriggered = new Dictionary<string, bool>();
    private static Dictionary<string, int> altarEndTurn = new Dictionary<string, int>();
    
    // ✅ Player-aware Phoenix Pyre tracking
    private static Dictionary<string, bool> phoenixPyreTriggered = new Dictionary<string, bool>();
    
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
        // ✅ Get the selected fire bishop from UIManager (following Archbishop pattern)
        FireBishop selectedFireBishop = null;
        Chessman fireBishopChessman = null;
        
        if (UIManager.Instance != null && UIManager.Instance.selectedPiece != null)
        {
            GameObject selectedPiece = UIManager.Instance.selectedPiece;
            selectedFireBishop = selectedPiece.GetComponent<FireBishop>();
            fireBishopChessman = selectedPiece.GetComponent<Chessman>();
            
            if (selectedFireBishop == null || fireBishopChessman == null)
            {
                Debug.LogError($"[WorldfireRing] Selected piece {selectedPiece.name} is not a FireBishop or missing Chessman component!");
                return;
            }
        }
        else
        {
            Debug.LogError("[WorldfireRing] No piece selected via UIManager!");
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
        
        // ✅ Set player-specific Worldfire Ring tracking
        worldfireRingStartTurn[player] = game.turns;
        worldfireRingEndTurn[player] = game.turns + 5;
        worldfireRingCaster[player] = fireBishopChessman;
        
        // Start cooldown (30 turns)
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.StartCooldown(player, "WorldfireRing", CooldownManager.CooldownType.TurnBased, 30);
        }
        
        Debug.Log($"[WorldfireRing] Activated! {player} Fire Bishop is stunned for 5 turns. Fire aura will expand each turn!");
        
        // ✅ Trigger initial 3x3 aura using the selected fire bishop
        selectedFireBishop.TriggerFireAura(3);
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
        // ✅ Get the selected fire bishop from UIManager (following Archbishop pattern)
        FireBishop selectedFireBishop = null;
        Chessman fireBishopChessman = null;
        
        if (UIManager.Instance != null && UIManager.Instance.selectedPiece != null)
        {
            GameObject selectedPiece = UIManager.Instance.selectedPiece;
            selectedFireBishop = selectedPiece.GetComponent<FireBishop>();
            fireBishopChessman = selectedPiece.GetComponent<Chessman>();
            
            if (selectedFireBishop == null || fireBishopChessman == null)
            {
                Debug.LogError($"[PhoenixPyre] Selected piece {selectedPiece.name} is not a FireBishop or missing Chessman component!");
                return;
            }
        }
        else
        {
            Debug.LogError("[PhoenixPyre] No piece selected via UIManager!");
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
        
        // ✅ Mark Phoenix Pyre as triggered for this player (bypasses EternalFlame cooldown)
        phoenixPyreTriggered[player] = true;
        eternalFlameTriggered[player] = true;
        
        // ✅ Set altar end turn to 3 turns from now (enhanced resurrection) for this player
        altarEndTurn[player] = game.turns + 3;
        
        // Destroy the Fire Bishop
        game.SetPositionEmpty(fireBishopX, fireBishopY);
        Destroy(fireBishopChessman.gameObject);
        
        // ✅ Create player-specific ashen pyre immediately at the same position
        string ashenPyreName = $"{player}_ashen_pyre";
        GameObject ashenPyre = game.Create(ashenPyreName, fireBishopX, fireBishopY);
        if (ashenPyre != null)
        {
            Debug.Log($"[PhoenixPyre] 🔥🔥🔥 PHOENIX PYRE ACTIVATED! {player} Fire Bishop sacrificed and ashen pyre created at ({fireBishopX},{fireBishopY})! Enhanced resurrection in 3 turns (turn {altarEndTurn[player]}).");
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
        // ✅ Use the current fire bishop instance instead of static variable
        if (chessman == null) return;
        
        int centerX = chessman.GetXBoard();
        int centerY = chessman.GetYBoard();
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
                        if (enemyChessman != null && enemyChessman.GetPlayer() != chessman.GetPlayer())
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
        // ✅ Check all players for active Worldfire Ring
        foreach (var kvp in worldfireRingEndTurn.ToList())
        {
            string player = kvp.Key;
            int endTurn = kvp.Value;
            
            if (endTurn == -1 || currentTurn > endTurn || !worldfireRingCaster.ContainsKey(player) || worldfireRingCaster[player] == null)
            {
                // Reset tracking if expired
                if (currentTurn > endTurn)
                {
                    worldfireRingEndTurn[player] = -1;
                    worldfireRingStartTurn[player] = -1;
                    worldfireRingCaster[player] = null;
                    Debug.Log($"[WorldfireRing] {player} period ended.");
                }
                continue;
            }
            
            // Calculate which turn of the 5-turn period we're in for this player
            int turnInPeriod = currentTurn - worldfireRingStartTurn[player];
            
            // ✅ Trigger appropriate aura based on turn
            FireBishop fireBishopScript = worldfireRingCaster[player].gameObject.GetComponent<FireBishop>();
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
        // ✅ Get the actual player from the fire bishop piece
        string player = chessman.GetPlayer();
        
        // Phoenix Pyre bypasses cooldown restrictions
        if (phoenixPyreTriggered.ContainsKey(player) && phoenixPyreTriggered[player])
        {
            Debug.Log($"[EternalFlame] {player} Phoenix Pyre triggered - bypassing cooldown restrictions!");
            return true; // Phoenix Pyre already handled the triggering
        }
        
        // Check if Eternal Flame is on cooldown (30 turns) - only for normal destruction
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "EternalFlame"))
        {
            Debug.Log("[EternalFlame] Still on cooldown - cannot trigger!");
            return false;
        }
        
        // ✅ Only trigger once per battle for this player
        if (eternalFlameTriggered.ContainsKey(player) && eternalFlameTriggered[player])
        {
            Debug.Log($"[EternalFlame] {player} already triggered this battle!");
            return false;
        }
        
        eternalFlameTriggered[player] = true;
        Debug.Log($"[EternalFlame] {player} Fire Bishop destroyed - Eternal Flame will activate next turn!");
        
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
        // ✅ Check all players for Eternal Flame triggers
        bool anyPlayerTriggered = false;
        foreach (var kvp in eternalFlameTriggered)
        {
            if (kvp.Value)
            {
                anyPlayerTriggered = true;
                break;
            }
        }
        
        if (!anyPlayerTriggered) return;
        
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[EternalFlame] Could not find Game component!");
            return;
        }
        
        // ✅ Process each player's Eternal Flame trigger
        foreach (var kvp in eternalFlameTriggered.ToList())
        {
            string player = kvp.Key;
            bool triggered = kvp.Value;
            
            if (!triggered) continue;
            
            // If Phoenix Pyre was triggered for this player, the ashen pyre is already created
            if (phoenixPyreTriggered.ContainsKey(player) && phoenixPyreTriggered[player])
            {
                Debug.Log($"[EternalFlame] {player} Phoenix Pyre already created ashen pyre - skipping summon plate creation!");
                eternalFlameTriggered[player] = false; // Reset trigger to prevent further calls
                continue;
            }
            
            Debug.Log($"[EternalFlame] 🔥 {player} ETERNAL FLAME ACTIVATED! Creating altar summon plates...");
            
            // Find all empty tiles on the board for this player
            int emptyTileCount = 0;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    GameObject piece = game.GetPosition(x, y);
                    if (piece == null)
                    {
                        // ✅ Create summon plate for empty tile with player info
                        CreateAltarSummonPlate(x, y, player);
                        emptyTileCount++;
                    }
                }
            }
            
            Debug.Log($"[EternalFlame] Created {emptyTileCount} altar summon plates for {player} on empty tiles.");
            
            // Reset trigger for this player
            eternalFlameTriggered[player] = false;
        }
    }
    
    /// <summary>
    /// Create a summon plate for altar placement
    /// </summary>
    private static void CreateAltarSummonPlate(int x, int y, string player)
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
            
            // ✅ Add AltarSummonPlate component with player info
            AltarSummonPlate altarPlate = plate.AddComponent<AltarSummonPlate>();
            altarPlate.Setup(x, y, player);
            
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
    public static void CreateAltar(int x, int y, string player)
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[EternalFlame] Could not find Game component for altar creation!");
            return;
        }
        
        // ✅ Create player-specific ashen pyre piece
        string ashenPyreName = $"{player}_ashen_pyre";
        GameObject ashenPyre = game.Create(ashenPyreName, x, y);
        if (ashenPyre != null)
        {
            // ✅ Set ashen pyre duration (5 turns from now) for this player
            altarEndTurn[player] = game.turns + 5;
            
            // Don't add special tile status - let it behave as a normal piece
            // The ashen pyre should be attackable by enemies like any other piece
            
            Debug.Log($"[EternalFlame] 🔥 {player} ASHEN PYRE CREATED at ({x},{y})! Resurrection in 5 turns (turn {altarEndTurn[player]}). Ashen pyre can be attacked by enemies.");
            
            // ✅ Reset eternal flame trigger for this player
            eternalFlameTriggered[player] = false;
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
        // ✅ Check all players for altar resurrection
        bool anyPlayerReady = false;
        foreach (var kvp in altarEndTurn)
        {
            if (kvp.Value != -1 && currentTurn >= kvp.Value)
            {
                anyPlayerReady = true;
                break;
            }
        }
        
        if (!anyPlayerReady) return;
        
        Debug.Log("[EternalFlame] Checking for altar resurrection...");
        
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[EternalFlame] Could not find Game component for resurrection check!");
            return;
        }
        
        // ✅ Process each player's altar resurrection
        foreach (var kvp in altarEndTurn.ToList())
        {
            string player = kvp.Key;
            int endTurn = kvp.Value;
            
            if (endTurn == -1 || currentTurn < endTurn) continue;
            
            // Find ashen pyre on the board for this player
            GameObject ashenPyre = null;
            int pyreX = -1, pyreY = -1;
            
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    GameObject piece = game.GetPosition(x, y);
                    if (piece != null && piece.name == $"{player}_ashen_pyre")
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
            
            // ✅ Step 4: Create the new player-specific Fire Bishop at the same position
            string fireBishopName = $"{player}_fire_bishop";
            GameObject newFireBishop = game.Create(fireBishopName, pyreX, pyreY);
            if (newFireBishop != null)
            {
                // Check if this was a Phoenix Pyre resurrection for enhanced abilities
                if (phoenixPyreTriggered.ContainsKey(player) && phoenixPyreTriggered[player])
                {
                    // Add Phoenix Resurrection status for enhanced movement abilities
                    StatusManager statusManager = newFireBishop.GetComponent<StatusManager>();
                    if (statusManager != null)
                    {
                        statusManager.AddStatus(StatusType.PhoenixResurrection, 999); // Permanent enhanced abilities
                        Debug.Log($"[EternalFlame] 🔥🔥🔥 {player} PHOENIX RESURRECTION! Fire Bishop resurrected with enhanced movement abilities (combined SurroundMovePlate + LineMovePlate)!");
                    }
                    
                    // Reset Phoenix Pyre trigger for this player
                    phoenixPyreTriggered[player] = false;
                }
                else
                {
                    Debug.Log($"[EternalFlame] {player} Fire Bishop successfully resurrected at ({pyreX},{pyreY})!");
                }
            }
            else
            {
                Debug.LogError($"[EternalFlame] Failed to resurrect {player} Fire Bishop at ({pyreX},{pyreY})!");
            }
            
            // ✅ Reset altar tracking for this player
            altarEndTurn[player] = -1;
        }
        else
        {
            // Ashen pyre was destroyed - resurrection failed
            Debug.Log($"[EternalFlame] {player} ashen pyre was destroyed - resurrection failed!");
            altarEndTurn[player] = -1;
        }
        }
    }
}
