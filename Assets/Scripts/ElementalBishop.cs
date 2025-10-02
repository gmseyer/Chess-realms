using UnityEngine;
using System.Collections.Generic;
using TMPro;


public class ElementalBishop : MonoBehaviour
{



    public GameObject movePlatePrefab;
    public int turns; // start at 1 (or 0 if you prefer)

    public int skillPoints = 3; // ✅ Bishop-specific SP (can refill later)
    private int nextSkillAvailableTurn = 0; // ✅ Cooldown tracker

    private Dictionary<string, int> skillCooldowns = new Dictionary<string, int>();

    // TMP UI references (drag in from inspector)
    public TMP_Text infernalBrandCooldownText;
    public TMP_Text glacialPathCooldownText;  
    public TMP_Text stoneSentinelCooldownText;

    private Game game;
    private Chessman chessman; // ✅ Cache Chessman reference for player info

    private List<ActiveTile> activeTiles = new List<ActiveTile>();
    public int tileDuration = 5; // ✅ configurable duration for tiles
    
    // ✅ Marker tracking (both fire and ice)
    private List<Marker> activeMarkers = new List<Marker>();
    
    // 🔥❄️🌍 INVOCATION SYSTEM - Element Stack Tracking
    private int fireStack = 0;
    private int iceStack = 0;
    private int earthStack = 0;
    private const int INVOCATION_THRESHOLD = 5; // 5 stacks to transform

     // Helper methods - now get selected piece from UIManager (following Queen pattern)
    public void InfernalBrand() => CastSkillFromUI("tile_lava");
    public void GlacialPath() => CastSkillFromUI("tile_ice");
    public void StoneSentinel() => CastSkillFromUI("tile_earth");
    

    private void Awake()
    {
        // Cache Chessman reference (following Bishop pattern)
        chessman = GetComponent<Chessman>();
        if (chessman == null)
            Debug.LogError("[ElementalBishop] Missing Chessman component!");
    }

    private void Start()
    {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
    }

    // ✅ NEW: Get selected elemental bishop from UIManager first (following Queen pattern)
    public void CastSkillFromUI(string tileName)
    {
        // Get the selected elemental bishop from UIManager
        ElementalBishop selectedBishop = null;
        Chessman cm = null;
        
        if (UIManager.Instance != null && UIManager.Instance.selectedPiece != null)
        {
            GameObject selectedPiece = UIManager.Instance.selectedPiece;
            selectedBishop = selectedPiece.GetComponent<ElementalBishop>();
            cm = selectedPiece.GetComponent<Chessman>();
            
            if (selectedBishop == null || cm == null)
            {
                Debug.LogError($"[ElementalBishop] Selected piece {selectedPiece.name} is not an Elemental Bishop or missing Chessman component!");
                return;
            }
        }
        else
        {
            Debug.LogError("[ElementalBishop] No piece selected via UIManager!");
            return;
        }
        
        // Now call CastSkill on the selected elemental bishop
        selectedBishop.CastSkill(tileName);
    }




    // Main skill casting function
    public void CastSkill(string tileName)
    {
        if (game == null)
        {
            Debug.LogError("[ElementalBishop] Game reference is missing!");
            return;
        }

        // ✅ Get player from Chessman component (following Bishop pattern)
        if (chessman == null)
        {
            chessman = GetComponent<Chessman>();
            if (chessman == null)
            {
                Debug.LogError("[ElementalBishop] No Chessman component found!");
                return;
            }
        }
        
        string player = chessman.GetPlayer();
        Debug.Log($"[ElementalBishop] Attempting {tileName} for {player} player...");

        int currentTurn = game.GetTurnCount(); // assumes Game has a turn counter
        if (skillCooldowns.TryGetValue(tileName, out int availableTurn) && currentTurn < availableTurn)
        {
            Debug.Log($"[ElementalBishop] {tileName} is still on cooldown for {player}! Available on turn {availableTurn}.");
            return;
        }

        // ✅ Deduct Skill Point using SkillManager with correct player
        if (!SkillManager.Instance.SpendPlayerSP(player, 1)) // Cost is 2 SP
        {
            Debug.LogWarning($"[ElementalBishop] Not enough Skill Points for {player}!");
            return;
        }
        
        // Log skill usage
        if (SkillTracker.Instance != null)
        {
            SkillTracker.Instance.LogSkillUsage(player, "ELEMENTAL BISHOP", tileName, 1);
        }

        // 🔥❄️🌍 UPDATE ELEMENT STACKS
        UpdateElementStacks(tileName);

        // ✅ Put skill on cooldown for 5 turns
        skillCooldowns[tileName] = currentTurn + 5;
        UpdateCooldownUI();

        // ✅ Spawn move plates on BOTH empty and occupied tiles
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                // ✅ Spawn move plates on ALL tiles (empty and occupied)
                SpawnMovePlate(game, x, y, tileName);
            }
        }

        Debug.Log($"[ElementalBishop] {tileName} skill activated for {player} (cooldown until turn {skillCooldowns[tileName]}).");
    }

    private void UpdateCooldownUI()
    {
        // ✅ Add null checks - UI text fields may not be assigned for all elemental bishops
        if (game == null) return;
        
        int currentTurn = game.GetTurnCount();

        if (infernalBrandCooldownText != null)
            infernalBrandCooldownText.text = GetCooldownText("tile_lava", currentTurn);
        if (glacialPathCooldownText != null)
            glacialPathCooldownText.text = GetCooldownText("tile_ice", currentTurn);
        if (stoneSentinelCooldownText != null)
            stoneSentinelCooldownText.text = GetCooldownText("tile_earth", currentTurn);
    }

    private string GetCooldownText(string skillName, int currentTurn)
    {
        if (!skillCooldowns.TryGetValue(skillName, out int availableTurn))
            return "READY"; // no cooldown yet

        int remaining = availableTurn - currentTurn;
        return (remaining > 0) ? $"CD: {remaining}" : "";
    }


    private void SpawnMovePlate(Game game, int x, int y, string tileName)
    {
        // ✅ Check if tile is occupied
        GameObject pieceAtPosition = game.GetPosition(x, y);
        if (pieceAtPosition != null)
        {
            // ✅ Tile is occupied - spawn move plate but with different behavior
            SpawnOccupiedTileMovePlate(game, x, y, tileName);
            return;
        }

        // ✅ Tile is empty - spawn normal move plate
        float fx = x * 0.57f - 1.98f;
        float fy = y * 0.56f - 1.95f;

        GameObject mp = Instantiate(movePlatePrefab, new Vector3(fx, fy, -3f), Quaternion.identity);

        MovePlate oldScript = mp.GetComponent<MovePlate>();
        if (oldScript != null) Destroy(oldScript);

        mp.AddComponent<SkillEndTurnPlate>().Setup(game, x, y, tileName);
    }

    // ✅ New method for occupied tiles (fire markers)
    private void SpawnOccupiedTileMovePlate(Game game, int x, int y, string tileName)
    {
        float fx = x * 0.57f - 1.98f;
        float fy = y * 0.56f - 1.95f;

        GameObject mp = Instantiate(movePlatePrefab, new Vector3(fx, fy, -3f), Quaternion.identity);


        MovePlate oldScript = mp.GetComponent<MovePlate>();
        if (oldScript != null) Destroy(oldScript);

        // ✅ Add OccupiedTilePlate component (handles both fire and ice markers)
        mp.AddComponent<OccupiedTilePlate>().Setup(game, x, y, tileName);
    }

   

    private class ActiveTile 
{
    public GameObject tileObject;
    public int expireTurn;
}

public void RegisterTile(GameObject tile)
{
    int expireOn = game.GetTurnCount() + tileDuration;
    activeTiles.Add(new ActiveTile { tileObject = tile, expireTurn = expireOn });
    Debug.Log($"[ElementalBishop] Registered {tile.name}, will expire on turn {expireOn}");
}

// ✅ Marker methods (both fire and ice)
public void RegisterMarker(Marker marker)
{
    activeMarkers.Add(marker);
    string markerTypeName = (marker.markerType == MarkerType.Fire) ? "fire" : "ice";
    Debug.Log($"[ElementalBishop] Registered {markerTypeName} marker at ({marker.x},{marker.y})");
}

public void CheckMarkers()
{
    int currentTurn = game.GetTurnCount();
    
    for (int i = activeMarkers.Count - 1; i >= 0; i--)
    {
        Marker marker = activeMarkers[i];
        if (marker == null)
        {
            activeMarkers.RemoveAt(i);
            continue;
        }
        
        // Check if marker has expired
        if (marker.IsExpired(currentTurn))
        {
            string markerTypeName = (marker.markerType == MarkerType.Fire) ? "fire" : "ice";
            Debug.Log($"[ElementalBishop] {markerTypeName} marker at ({marker.x},{marker.y}) expired");
            Destroy(marker.gameObject);
            activeMarkers.RemoveAt(i);
            continue;
        }
        
        // Check if tracked piece moved away from marker (NOT handled by attack case)
        if (marker.HasTrackedPieceMovedAway() && !marker.wasHandledByAttack)
        {
            string markerTypeName = (marker.markerType == MarkerType.Fire) ? "fire" : "ice";
            string tileName = (marker.markerType == MarkerType.Fire) ? "lava tile" : "ice tile";
            Debug.Log($"[ElementalBishop] Tracked piece {marker.trackedPieceName} moved away from {markerTypeName} marker at ({marker.x},{marker.y}), converting to {tileName}");
            marker.ConvertToTile();
            activeMarkers.RemoveAt(i);
        }
    }
}


public void CheckAndDestroyExpiredTiles()
{
    int currentTurn = game.GetTurnCount();
    for (int i = activeTiles.Count - 1; i >= 0; i--)
    {
        if (currentTurn >= activeTiles[i].expireTurn)
        {
            if (activeTiles[i].tileObject != null)
            {
                Debug.Log($"[ElementalBishop] Destroying expired tile {activeTiles[i].tileObject.name}");
                Destroy(activeTiles[i].tileObject);
            }
            activeTiles.RemoveAt(i);
        }
    }
}

// 🔥❄️🌍 INVOCATION SYSTEM METHODS

private void UpdateElementStacks(string tileName)
{
    // Reset other element stacks and increment current element
    switch (tileName)
    {
        case "tile_lava": // Fire element - InfernalBrand
            fireStack++;
            iceStack = 0;
            earthStack = 0;
            Debug.Log($"[Invocation] Fire stack: {fireStack} (Ice: {iceStack}, Earth: {earthStack})");
            break;
            
        case "tile_ice": // Ice element - GlacialPath
            iceStack++;
            fireStack = 0;
            earthStack = 0;
            Debug.Log($"[Invocation] Ice stack: {iceStack} (Fire: {fireStack}, Earth: {earthStack})");
            break;
            
        case "tile_earth": // Earth element - StoneSentinel
            earthStack++;
            fireStack = 0;
            iceStack = 0;
            Debug.Log($"[Invocation] Earth stack: {earthStack} (Fire: {fireStack}, Ice: {iceStack})");
            break;
    }
}

// Public getters for UI to access stack information
public int GetFireStack() => fireStack;
public int GetIceStack() => iceStack;
public int GetEarthStack() => earthStack;
public bool IsInvocationReady() => fireStack >= INVOCATION_THRESHOLD || iceStack >= INVOCATION_THRESHOLD || earthStack >= INVOCATION_THRESHOLD;

// Invocation button method - called by UI button
public void Invocation()
{
    Debug.Log("[Invocation] Invocation button pressed!");
    
    // Get the selected ElementalBishop from UIManager
    ElementalBishop selectedBishop = null;
    Chessman cm = null;
    if (UIManager.Instance != null && UIManager.Instance.selectedPiece != null)
    {
        GameObject selectedPiece = UIManager.Instance.selectedPiece;
        selectedBishop = selectedPiece.GetComponent<ElementalBishop>();
        cm = selectedPiece.GetComponent<Chessman>();
        if (selectedBishop == null || cm == null)
        {
            Debug.LogError($"[Invocation] Selected piece {selectedPiece.name} is not an ElementalBishop or missing Chessman component!");
            return;
        }
    }
    else
    {
        Debug.LogError("[Invocation] No piece selected via UIManager!");
        return;
    }
    
    string player = cm.GetPlayer();
    Debug.Log($"[Invocation] Current stacks - Fire: {selectedBishop.fireStack}, Ice: {selectedBishop.iceStack}, Earth: {selectedBishop.earthStack}");
    
    // Check which stack has 5 or more
    string newPieceName = "";
    string invocationType = "";
    
    if (selectedBishop.fireStack >= INVOCATION_THRESHOLD)
    {
        newPieceName = $"{player}_fire_bishop";
        invocationType = "🔥 FIRE INVOCATION";
    }
    else if (selectedBishop.iceStack >= INVOCATION_THRESHOLD)
    {
        newPieceName = $"{player}_ice_bishop";
        invocationType = "❄️ ICE INVOCATION";
    }
    else if (selectedBishop.earthStack >= INVOCATION_THRESHOLD)
    {
        newPieceName = $"{player}_earth_bishop"; 
        invocationType = "🌍 EARTH INVOCATION";
    }
    else
    {
        Debug.Log("[Invocation] No stack has reached 5 - doing nothing");
        return;
    }
    
    Debug.Log($"[Invocation] {invocationType} - Transforming into {newPieceName}");
    
    // Use Pawn.cs promotion logic for transformation
    selectedBishop.TransformElementalBishop(newPieceName, invocationType);
}

private void TransformElementalBishop(string newPieceName, string invocationType)
{
    if (game == null)
    {
        Debug.LogError("[Invocation] Missing Game reference!");
        return;
    }

    // Get the selected ElementalBishop from UIManager to get position and player
    ElementalBishop selectedBishop = null;
    Chessman cm = null;
    if (UIManager.Instance != null && UIManager.Instance.selectedPiece != null)
    {
        GameObject selectedPiece = UIManager.Instance.selectedPiece;
        selectedBishop = selectedPiece.GetComponent<ElementalBishop>();
        cm = selectedPiece.GetComponent<Chessman>();
        if (selectedBishop == null || cm == null)
        {
            Debug.LogError($"[Invocation] Selected piece {selectedPiece.name} is not an ElementalBishop or missing Chessman component!");
            return;
        }
    }
    else
    {
        Debug.LogError("[Invocation] No piece selected via UIManager!");
        return;
    }
    
    string player = cm.GetPlayer();
    int x = cm.GetXBoard();
    int y = cm.GetYBoard();
    
    Debug.Log($"[Invocation] Found {player}_elemental_bishop at ({x},{y})");
    
    Debug.Log($"[Invocation] {invocationType} - Transforming Elemental Bishop at ({x},{y}) into {newPieceName}");
    
    // ✅ EXACT Pawn.cs promotion pattern: Clear position, destroy old piece, create new piece
    // Step 1: Get reference to the ElementalBishop piece BEFORE clearing position
    GameObject elementalBishopPiece = game.GetPosition(x, y);
    
    // Step 2: Clear the current position
    game.SetPositionEmpty(x, y);
    
    // Step 3: Destroy the current Elemental Bishop piece
    if (elementalBishopPiece != null)
    {
        Destroy(elementalBishopPiece);
        Debug.Log($"[Invocation] Destroyed ElementalBishop piece at ({x},{y})");
    }
    
    // Step 4: Create the new specialized bishop at the same position
    GameObject newBishop = game.Create(newPieceName, x, y);
    if (newBishop != null)
    {
        Debug.Log($"[Invocation] {invocationType} SUCCESS! {newPieceName} created at ({x},{y})");
    }
    else
    {
        Debug.LogError($"[Invocation] Failed to create {newPieceName} at ({x},{y})");
    }
         foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
    // End turn (following the pattern)
    game.NextTurn();
}

}
