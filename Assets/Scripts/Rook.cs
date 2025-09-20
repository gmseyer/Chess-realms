using System;
using System.Collections.Generic;
using UnityEngine;

public class Rook : Pieces // ✅ now inherits from Pieces instead of MonoBehaviour
{
    public GameObject movePlatePrefab; // optional, keep if you want later
    public GameObject celestialOrbPrefab; // Add this field for the orb marker prefab
    
    // Celestial Orb tracking
    private static List<Vector2Int> celestialOrbLocations = new List<Vector2Int>();
    private static List<bool> celestialOrbCaptured = new List<bool>();
    private static List<GameObject> celestialOrbObjects = new List<GameObject>();

    // small helper used during testing
    public void TestSkill()
    {
        Debug.Log($"[Rook] TestSkill() called on {gameObject.name}");
    }

    // Call this to attempt the Royal Castling from UI
    public void AttemptRoyalCastling()
    {
        // get Chessman data (works if Chessman is on same object or parent)
        Chessman cm = GetComponentInParent<Chessman>();
        if (cm == null)
        {
            Debug.LogError("[RoyalCastling] No Chessman component found on this object/parents!");
            return;
        }

        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[RoyalCastling] Could not find GameController.");
            return;
        }

        string currentPlayer = game.GetCurrentPlayer();
        string rookPlayer = cm.GetPlayer();

        Debug.Log($"[RoyalCastling] Attempt by '{cm.name}' (player='{rookPlayer}'), turn owner='{currentPlayer}'");

        // turn check
        if (!string.Equals(rookPlayer, currentPlayer, StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log($"[RoyalCastling] It's not {rookPlayer}'s turn!");
            return;
        }

        // once-per-battle cooldown check
        // ✅ NEW: Use CooldownManager instead of hasUsedRoyalCastling
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(rookPlayer, "RoyalCastling"))
        {
            Debug.Log("[RoyalCastling] Skill is on cooldown - cannot use this battle.");
            return;
        }
       

        // find the king first (validate preconditions before spending SP)
        Chessman kingCm = FindKing(game, rookPlayer);
        if (kingCm == null)
        {
            Debug.LogWarning($"[RoyalCastling] No king found for player '{rookPlayer}'. Aborting.");
            return;
        }

        // SP cost
        const int cost = 2;
        if (!SkillManager.Instance.SpendPlayerSP(rookPlayer, cost))
        {
            Debug.LogWarning($"[RoyalCastling] Not enough SP for {rookPlayer} (cost {cost}).");
            return;
        }

        // Save positions
        int rookX = cm.GetXBoard();
        int rookY = cm.GetYBoard();
        int kingX = kingCm.GetXBoard();
        int kingY = kingCm.GetYBoard();

        Debug.Log($"[RoyalCastling] Swapping Rook ({rookX},{rookY}) with King ({kingX},{kingY})");

        // Clear board positions first
        game.ClearPosition(rookX, rookY);
        game.ClearPosition(kingX, kingY);

        // Move rook -> king pos
        cm.SetXBoard(kingX);
        cm.SetYBoard(kingY);
        cm.SetCoords();
        game.SetPositionAt(cm.gameObject, kingX, kingY);

        // Move king -> rook pos
        kingCm.SetXBoard(rookX);
        kingCm.SetYBoard(rookY);
        kingCm.SetCoords(); 
        game.SetPositionAt(kingCm.gameObject, rookX, rookY);

        // Clean up any move plates if needed
        cm.DestroyMovePlates();
        kingCm.DestroyMovePlates();

        // ✅ NEW: Start cooldown using CooldownManager
        // ✅ Start cooldown using CooldownManager (with null check)
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.StartCooldown(rookPlayer, "RoyalCastling", CooldownManager.CooldownType.OncePerBattle);
        }
        Debug.Log($"[RoyalCastling] Swap complete! Rook at ({kingX},{kingY}), King at ({rookX},{rookY})");
        
        if (SkillTracker.Instance != null)
    {
        SkillTracker.Instance.LogSkillUsage(currentPlayer, cm.name, "ROYAL CASTLING", cost);
    }
        // end turn
        game.NextTurn();
    }

    // helper to locate the king of a player
    private Chessman FindKing(Game game, string player)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = game.GetPosition(x, y);
                if (piece == null) continue;

                Chessman cm = piece.GetComponent<Chessman>();
                if (cm == null) continue;

                if (piece.name.ToLower().Contains("king") && cm.GetPlayer() == player)
                    return cm;
            }
        }
        return null;
    }


 // Call this from UI button for Fortify skill

public void AttemptFortify()
{
    // ✅ ONCE PER ROOK LIMITATION CHECK
   


    // Get the selected piece from UIManager
    if (UIManager.Instance == null)
    {
        Debug.Log("FortifySelected: UIManager not found.");
        return;
    }

    GameObject selectedPiece = UIManager.Instance.selectedPiece;
    if (selectedPiece == null)
    {
        Debug.Log("FortifySelected: no piece selected.");
        return;
    }

    if (!selectedPiece.name.Contains("rook"))
    {
        Debug.Log("FortifySelected: selected piece is not a rook.");
        return;
    }

    Chessman rookCm = selectedPiece.GetComponent<Chessman>();
    if (rookCm == null)
    {
        Debug.Log("FortifySelected: selected object has no Chessman.");
        return;
    }

    Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
    if (game == null)
    {
        Debug.LogError("FortifySelected: GameController not found.");
        return;
    }

    // Make sure it's the owner's turn
    string currentPlayer = game.GetCurrentPlayer();
    if (rookCm.GetPlayer() != currentPlayer)
    {
        Debug.Log($"FortifySelected: it's {currentPlayer}'s turn. Can't use {rookCm.name}.");
        return;
    }

    const int fortifyCost = 1;

    // ✅ UPDATED: Use SkillManager instead of Game.SpendPlayerSP
    if (SkillManager.Instance == null)
    {
        Debug.LogError("FortifySelected: SkillManager not found.");
        return;
    }
    // ✅ Check if CooldownManager exists before using it
    if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(currentPlayer, "Fortify"))
    {
        Debug.Log("FortifySelected: Fortify is on co oldown - cannot use yet.");
        return;
    }

    bool paid = SkillManager.Instance.SpendPlayerSP(currentPlayer, fortifyCost);
    if (!paid)
    {
        Debug.Log($"{currentPlayer} does not have enough SP to use Fortify (cost {fortifyCost}).");
        return;
    }

     if (SkillTracker.Instance != null)
    {
        SkillTracker.Instance.LogSkillUsage(currentPlayer, rookCm.name, "FORTIFY", fortifyCost);
    }
    Debug.Log($"{currentPlayer} paid {fortifyCost} SP for Fortify. Remaining SP: {SkillManager.Instance.GetPlayerSP(currentPlayer)}");

    // APPLY EFFECT: make allied pieces around rook invulnerable
    int cx = rookCm.GetXBoard();
    int cy = rookCm.GetYBoard();

    for (int dx = -1; dx <= 1; dx++)
    {
        for (int dy = -1; dy <= 1; dy++)
        {
            if (dx == 0 && dy == 0) continue;
            int tx = cx + dx;
            int ty = cy + dy;
            if (!game.PositionOnBoard(tx, ty)) continue;

            GameObject target = game.GetPosition(tx, ty);
            if (target == null) continue;

            Chessman targetCm = target.GetComponent<Chessman>();
            if (targetCm == null) continue;

            if (targetCm.GetPlayer() == rookCm.GetPlayer())
            {
                targetCm.isInvulnerable = true;
                targetCm.invulnerableUntilTurn = game.turns + 2;
                Debug.Log($"{targetCm.name} is now invulnerable until turn {targetCm.invulnerableUntilTurn}");
            }
        }
    }

    // ✅ MARK AS USED - Once per rook limitation
   // ✅ NEW: Start cooldown using CooldownManager (2 turns)
    // ✅ Start cooldown using CooldownManager (with null check)
    if (CooldownManager.Instance != null)
    {
        CooldownManager.Instance.StartCooldown(currentPlayer, "Fortify", CooldownManager.CooldownType.TurnBased, 4);
    }
Debug.Log($"{rookCm.name} has used Fortify and will be available again in 2 turns.");

    // tidy up and end turn
    rookCm.DestroyMovePlates();
    // update the UI SP readout
    if (UIManager.Instance != null)
    {
        UIManager.Instance.UpdateSkillPointDisplay();
    }
    game.NextTurn();
}

// Royal Rook Promotion Skill
public void royalRook()
{
    Debug.Log("[Royal Rook] Promotion skill activated!");
    
    // Clear any existing celestial orbs
    ClearCelestialOrbs();
    
    Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
    
    // Get all empty tiles on the board
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
    
    Debug.Log($"[Royal Rook] Found {emptyTiles.Count} empty tiles on the board");
    
    if (emptyTiles.Count < 3)
    {
        Debug.LogWarning("[Royal Rook] Not enough empty tiles to spawn 3 Celestial Orbs!");
        return;
    }
    
    // Choose 3 random empty tiles
    List<Vector2Int> selectedTiles = new List<Vector2Int>();
    List<Vector2Int> availableTiles = new List<Vector2Int>(emptyTiles);
    
    for (int i = 0; i < 3; i++)
    {
        int randomIndex = UnityEngine.Random.Range(0, availableTiles.Count);
        Vector2Int selectedTile = availableTiles[randomIndex];
        selectedTiles.Add(selectedTile);
        availableTiles.RemoveAt(randomIndex);
    }
    
    // Spawn Celestial Orb markers on selected tiles
    foreach (Vector2Int tile in selectedTiles)
    {
        GameObject orb = SpawnCelestialOrb(game, tile.x, tile.y);
        if (orb != null)
        {
            celestialOrbLocations.Add(tile);
            celestialOrbCaptured.Add(false);
            celestialOrbObjects.Add(orb);
        }
    }
    
    Debug.Log($"[Royal Rook] Spawned 3 Celestial Orbs on tiles: {string.Join(", ", selectedTiles)}");
    
    // End the Rook's turn
    
    
    
    // Hide UI panels before ending turn
    if (UIManager.Instance != null)
    {
        UIManager.Instance.pawnPanel?.SetActive(false);
        UIManager.Instance.knightPanel?.SetActive(false);
        UIManager.Instance.bishopPanel?.SetActive(false);
        UIManager.Instance.rookPanel?.SetActive(false);
        UIManager.Instance.queenPanel?.SetActive(false);
        UIManager.Instance.kingPanel?.SetActive(false);
        UIManager.Instance.whiteElementalBishopPanel?.SetActive(false);
        UIManager.Instance.whiteArchBishopPanel?.SetActive(false);
    }
    
    Debug.Log("[Royal Rook] Ending turn after casting skill");
    game.NextTurn();
   foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
           Destroy(plate); 
}

private GameObject SpawnCelestialOrb(Game game, int x, int y)
{
    // Use the same positioning as move plates
    float fx = x * 0.57f - 1.98f;
    float fy = y * 0.56f - 1.95f;
    
    // Instantiate the prefab instead of creating new GameObject
    if (celestialOrbPrefab != null)
    {
        GameObject orb = Instantiate(celestialOrbPrefab, new Vector3(fx, fy, -3f), Quaternion.identity);
        
        // Debug: Check if orb has a sprite
        SpriteRenderer sr = orb.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Debug.Log($"[Royal Rook] Orb has sprite: {sr.sprite?.name ?? "NULL"}");
        }
        else
        {
            Debug.LogWarning("[Royal Rook] Orb prefab has no SpriteRenderer component!");
        }
        
        // Add CelestialOrbMarker script if it doesn't exist
        CelestialOrbMarker marker = orb.GetComponent<CelestialOrbMarker>();
        if (marker == null)
        {
            marker = orb.AddComponent<CelestialOrbMarker>();
        }
        marker.Setup(game, x, y);
        
        Debug.Log($"[Royal Rook] Celestial Orb spawned at ({x},{y}) using prefab - Position: {orb.transform.position}");
        return orb;
    }
    else
    {
        Debug.LogError("[Royal Rook] Celestial Orb prefab not assigned!");
        return null;
    }
}

// Clear all existing celestial orbs
private void ClearCelestialOrbs()
{
    foreach (GameObject orb in celestialOrbObjects)
    {
        if (orb != null)
        {
            Destroy(orb);
        }
    }
    celestialOrbLocations.Clear();
    celestialOrbCaptured.Clear();
    celestialOrbObjects.Clear();
}

// Check if Rook is on a celestial orb location
public static void CheckCelestialOrbCapture(Vector2Int rookPosition)
{
    for (int i = 0; i < celestialOrbLocations.Count; i++)
    {
        if (celestialOrbLocations[i] == rookPosition && !celestialOrbCaptured[i])
        {
            // Rook captured this orb
            celestialOrbCaptured[i] = true;
            
            // Change orb color to green
            if (i < celestialOrbObjects.Count && celestialOrbObjects[i] != null)
            {
                SpriteRenderer sr = celestialOrbObjects[i].GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = Color.green;
                }
            }
            
            Debug.Log($"[Royal Rook] Celestial Orb captured at ({rookPosition.x},{rookPosition.y})!");
            
            // Check if all orbs are captured
            bool allCaptured = true;
            for (int j = 0; j < celestialOrbCaptured.Count; j++)
            {
                if (!celestialOrbCaptured[j])
                {
                    allCaptured = false;
                    break;
                }
            }
            
            if (allCaptured)
            {
                Debug.Log("[Royal Rook] Ready for promotion!");
                
                // Find the Rook that captured the last orb
                GameObject rookToPromote = FindRookAtPosition(rookPosition);
                if (rookToPromote != null)
                {
                    // Get the position of the last orb captured
                    Vector2Int promotionPosition = rookPosition;
                    
                    // Destroy the current Rook
                    Destroy(rookToPromote);
                    Debug.Log($"[Royal Rook] Destroyed Rook at ({promotionPosition.x},{promotionPosition.y})");
                    
                    // Destroy all celestial orbs
                    foreach (GameObject orb in celestialOrbObjects)
                    {
                        if (orb != null)
                        {
                            Destroy(orb);
                        }
                    }
                    Debug.Log("[Royal Rook] Destroyed all celestial orbs");
                    
                    // Create white_royal_rook at the last orb's position
                    Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
                    GameObject royalRook = game.Create("white_royal_rook", promotionPosition.x, promotionPosition.y);
                    
                    if (royalRook != null)
                    {
                        Debug.Log($"[Royal Rook] Created white_royal_rook at ({promotionPosition.x},{promotionPosition.y})");
                    }
                    
                    // Clear celestial orb tracking
                    celestialOrbLocations.Clear();
                    celestialOrbCaptured.Clear();
                    celestialOrbObjects.Clear();
                    
                  
                }
                else
                {
                    Debug.LogError("[Royal Rook] Could not find Rook to promote!");
                }
            }
            
            break;
        }
    }
}

// Helper method to find the Rook at a specific position
private static GameObject FindRookAtPosition(Vector2Int position)
{
    Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
    GameObject pieceAtPosition = game.GetPosition(position.x, position.y);
    
    if (pieceAtPosition != null && pieceAtPosition.name.Contains("rook"))
    {
        return pieceAtPosition;
    }
    
    return null;
}


}
