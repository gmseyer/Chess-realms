using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthboundBishop : MonoBehaviour
{
    private Game game;
    private Chessman chessman;
    
    // Terra Ward tracking - player-aware
    private static Dictionary<string, GameObject> currentTerraWard = new Dictionary<string, GameObject>();
    private static Dictionary<string, int> terraWardX = new Dictionary<string, int>();
    private static Dictionary<string, int> terraWardY = new Dictionary<string, int>();
    
    // Seismic Seal Terra Ward tracking (max 3)
    private static List<GameObject> seismicTerraWards = new List<GameObject>();
    private static List<Vector2Int> seismicTerraWardPositions = new List<Vector2Int>();

    // Polarity tracking
    public static Vector2Int selectedPolarityPiecePosition = new Vector2Int(-1, -1);
    
    void Start()
    {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        chessman = GetComponent<Chessman>();
    }

    void Update()
    {
        
    }
    
    /// <summary>
    /// EarthenGenesis - Create Terra Ward after movement
    /// Called from MovePlate.cs after EarthboundBishop moves
    /// </summary>
    public static void CreateTerraWard(int bishopX, int bishopY, string player)
    {
        // Destroy existing Terra Ward for this player if it exists
        if (currentTerraWard.ContainsKey(player) && currentTerraWard[player] != null)
        {
            Debug.Log($"[EarthenGenesis] Destroying existing Terra Ward for {player} at ({terraWardX[player]},{terraWardY[player]})");
            Destroy(currentTerraWard[player]);
            currentTerraWard[player] = null;
        }
        
        // Find a random adjacent tile for the new Terra Ward
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[EarthenGenesis] Could not find Game component!");
            return;
        }
        
        // Get all adjacent positions
        List<Vector2Int> adjacentPositions = new List<Vector2Int>();
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue; // Skip center position
                
                int checkX = bishopX + dx;
                int checkY = bishopY + dy;
                
                // Check if position is within board bounds and empty
                if (checkX >= 0 && checkX < 8 && checkY >= 0 && checkY < 8)
                {
                    GameObject piece = game.GetPosition(checkX, checkY);
                    if (piece == null)
                    {
                        adjacentPositions.Add(new Vector2Int(checkX, checkY));
                    }
                }
            }
        }
        
        // Create Terra Ward on a random adjacent empty tile
        if (adjacentPositions.Count > 0)
        {
            Vector2Int randomPosition = adjacentPositions[Random.Range(0, adjacentPositions.Count)];
            int wardX = randomPosition.x;
            int wardY = randomPosition.y;
            
            // Create the Terra Ward
            GameObject terraWard = game.Create("tile_terra_ward", wardX, wardY);
            if (terraWard != null)
            {
                // Add permanent status to Terra Ward (lasts until end of battle)
                StatusManager wardStatus = terraWard.GetComponent<StatusManager>();
                if (wardStatus != null)
                {
                    wardStatus.AddStatus(StatusType.SolidBlock, 999); // Permanent solid block
                    wardStatus.AddStatus(StatusType.specialTile, 999); // Special tile status
                }
                
                // Track the Terra Ward for this player
                currentTerraWard[player] = terraWard;
                terraWardX[player] = wardX;
                terraWardY[player] = wardY;
                
                Debug.Log($"[EarthenGenesis] 🌍 TERRA WARD CREATED for {player} at ({wardX},{wardY})! Lasts until end of battle.");
            }
            else
            {
                Debug.LogError($"[EarthenGenesis] Failed to create Terra Ward at ({wardX},{wardY})!");
            }
        }
        else
        {
            Debug.Log("[EarthenGenesis] No adjacent empty tiles found for Terra Ward creation.");
        }
    }
    
    /// <summary>
    /// Check if EarthboundBishop can phase through stone-type tiles
    /// </summary>
    public static bool CanPhaseThroughStone(string pieceName)
    {
        // EarthboundBishop can phase through stone-type tiles
        return pieceName == "tile_earth" || 
               pieceName == "tile_terra_ward" || 
               pieceName == "celestial_pillar";
    }
    
    /// <summary>
    /// Seismic Seal - Active skill with multiple targeting modes
    /// </summary>
    public void SeismicSeal()
    {
        // ✅ Get the selected earthbound bishop from UIManager (following Archbishop pattern)
        EarthboundBishop selectedEarthboundBishop = null;
        Chessman earthboundBishopChessman = null;
        
        if (UIManager.Instance != null && UIManager.Instance.selectedPiece != null)
        {
            GameObject selectedPiece = UIManager.Instance.selectedPiece;
            selectedEarthboundBishop = selectedPiece.GetComponent<EarthboundBishop>();
            earthboundBishopChessman = selectedPiece.GetComponent<Chessman>();
            
            if (selectedEarthboundBishop == null || earthboundBishopChessman == null)
            {
                Debug.LogError($"[SeismicSeal] Selected piece {selectedPiece.name} is not an EarthboundBishop or missing Chessman component!");
                return;
            }
        }
        else
        {
            Debug.LogError("[SeismicSeal] No piece selected via UIManager!");
            return;
        }
        
        string player = earthboundBishopChessman.GetPlayer();
        
        // Check SP cost (2 SP)
        if (SkillManager.Instance.GetPlayerSP(player) < 2)
        {
            Debug.Log("[SeismicSeal] Not enough SP!");
            return;
        }
        
        // Check if Seismic Seal is on cooldown (8 turns)
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "SeismicSeal"))
        {
            Debug.Log("[SeismicSeal] Skill is on cooldown!");
            return;
        }
        
        // Deduct SP
        SkillManager.Instance.SpendPlayerSP(player, 2);
        
        // Start Seismic Seal cooldown (8 turns)
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.StartCooldown(player, "SeismicSeal", CooldownManager.CooldownType.TurnBased, 8);
        }
        
        // Remove all existing move plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
        
        // Create selection plates on all board tiles
        CreateSeismicSealSelectionPlates();
        
        Debug.Log("[SeismicSeal] 🌍 SEISMIC SEAL ACTIVATED! Select target tile...");
    }
    
    
    /// <summary>
    /// Create selection plates on all board tiles for Seismic Seal targeting
    /// </summary>
    private static void CreateSeismicSealSelectionPlates()
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[SeismicSeal] Could not find Game component for selection plates!");
            return;
        }
        
        int selectionPlateCount = 0;
        
        // Create selection plates on all board tiles
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                // Create selection plate at this position
                CreateSeismicSealSelectionPlate(x, y);
                selectionPlateCount++;
            }
        }
        
        Debug.Log($"[SeismicSeal] Created {selectionPlateCount} selection plates for targeting.");
    }
    
    /// <summary>
    /// Create a single selection plate for Seismic Seal targeting
    /// </summary>
    private static void CreateSeismicSealSelectionPlate(int x, int y)
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null) return;
        
        float fx = x * 0.57f - 1.98f;
        float fy = y * 0.56f - 1.95f;
        
        GameObject plate = Instantiate(game.movePlatePrefabReference);
        plate.transform.position = new Vector3(fx, fy, -3f);
        
        // Add SeismicSealSelectionPlate component
        SeismicSealSelectionPlate seismicPlate = plate.AddComponent<SeismicSealSelectionPlate>();
        seismicPlate.Setup(x, y);
        
        // Set visual appearance (green for earth/seismic theme)
        plate.GetComponent<SpriteRenderer>().color = new Color(0f, 0.8f, 0f, 0.75f);
        
        Debug.Log($"[SeismicSeal] Created selection plate at ({x},{y})");
    }
    
    /// <summary>
    /// Create Terra Ward for Seismic Seal (with max 3 limit)
    /// </summary>
    public static void CreateSeismicTerraWard(int x, int y)
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[SeismicSeal] Could not find Game component for Terra Ward creation!");
            return;
        }
        
        // Check if we already have 3 Terra Wards (max limit)
        if (seismicTerraWards.Count >= 3)
        {
            // Destroy the oldest Terra Ward (first in list)
            if (seismicTerraWards.Count > 0)
            {
                GameObject oldestWard = seismicTerraWards[0];
                Vector2Int oldestPosition = seismicTerraWardPositions[0];
                
                // Clear position and destroy
                game.SetPositionEmpty(oldestPosition.x, oldestPosition.y);
                Destroy(oldestWard);
                
                // Remove from tracking lists
                seismicTerraWards.RemoveAt(0);
                seismicTerraWardPositions.RemoveAt(0);
                
                Debug.Log($"[SeismicSeal] Destroyed oldest Terra Ward at ({oldestPosition.x},{oldestPosition.y}) to make room for new one.");
            }
        }
        
        // Create the new Terra Ward
        GameObject terraWard = game.Create("tile_terra_ward", x, y);
        if (terraWard != null)
        {
            // Add permanent status to Terra Ward (lasts until end of battle)
            StatusManager wardStatus = terraWard.GetComponent<StatusManager>();
            if (wardStatus != null)
            {
                wardStatus.AddStatus(StatusType.SolidBlock, 999); // Permanent solid block
                wardStatus.AddStatus(StatusType.specialTile, 999); // Special tile status
            }
            
            // Track the Terra Ward
            seismicTerraWards.Add(terraWard);
            seismicTerraWardPositions.Add(new Vector2Int(x, y));
            
            Debug.Log($"[SeismicSeal] 🌍 TERRA WARD CREATED at ({x},{y})! Total Terra Wards: {seismicTerraWards.Count}/3");
        }
        else
        {
            Debug.LogError($"[SeismicSeal] Failed to create Terra Ward at ({x},{y})!");
        }
    }
    
    /// <summary>
    /// Apply Stone Sentinel status to target piece
    /// </summary>
    public static void ApplySeismicSealStoneSentinel(Chessman targetChessman)
    {
        if (targetChessman == null)
        {
            Debug.LogError("[SeismicSeal] Target piece is null!");
            return;
        }
        
        // Get current turn from Game component
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[SeismicSeal] Could not find Game component for turn tracking!");
            return;
        }
        
        StatusManager targetStatus = targetChessman.GetComponent<StatusManager>();
        if (targetStatus != null)
        {
            // Apply Stone Sentinel status for 2 turns
            targetStatus.AddStatus(StatusType.StoneSentinel, game.turns + 2);
            
            Debug.Log($"[SeismicSeal] 🗿 STONE SENTINEL applied to {targetChessman.name} for 2 turns!");
        }
        else
        {
            Debug.LogError($"[SeismicSeal] Could not find StatusManager on target piece!");
        }
    }
    
    /// <summary>
    /// Apply Terra Ward 3x3 adjacent area effect
    /// </summary>
    public static void ApplySeismicSealTerraWardEffect(int centerX, int centerY)
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[SeismicSeal] Could not find Game component for Terra Ward effect!");
            return;
        }
        
        int affectedPieces = 0;
        
        // Check 8 adjacent tiles around the Terra Ward
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue; // Skip center (Terra Ward itself)
                
                int checkX = centerX + dx;
                int checkY = centerY + dy;
                
                // Check if position is within board bounds
                if (checkX >= 0 && checkX < 8 && checkY >= 0 && checkY < 8)
                {
                    GameObject piece = game.GetPosition(checkX, checkY);
                    if (piece != null)
                    {
                        Chessman pieceChessman = piece.GetComponent<Chessman>();
                        if (pieceChessman != null)
                        {
                            // Apply Stone Sentinel to any piece in adjacent area
                            ApplySeismicSealStoneSentinel(pieceChessman);
                            affectedPieces++;
                        }
                    }
                }
            }
        }
        
        Debug.Log($"[SeismicSeal] 🌍 TERRA WARD EFFECT applied to {affectedPieces} pieces in adjacent area!");
    }
    
    /// <summary>
    /// Apply self-cast effect (tile_earth property with movement but no attack)
    /// </summary>
    public static void ApplySeismicSealSelfCast(Chessman earthboundBishop)
    {
        if (earthboundBishop == null || !earthboundBishop.name.ToLower().Contains("earth_bishop"))
        {
            Debug.LogError("[SeismicSeal] Invalid target for self-cast!");
            return;
        }

        // Get current turn from Game component
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[SeismicSeal] Could not find Game component for turn tracking!");
            return;
        }

        StatusManager status = earthboundBishop.GetComponent<StatusManager>();
        if (status != null)
        {
            // Apply Stone Sentinel status for 4 turns (self-cast duration)
            status.AddStatus(StatusType.StoneSentinel, game.turns + 4);

            Debug.Log($"[SeismicSeal] 🗿 SELF-CAST applied to EarthboundBishop for 4 turns! Can move but cannot attack.");
        }
        else
        {
            Debug.LogError($"[SeismicSeal] Could not find StatusManager on EarthboundBishop!");
        }
    }

    /// <summary>
    /// Polarity - Move Terra Wards or Stone Sentinel pieces along Queen-like paths
    /// </summary>
    public void Polarity()
    {
        // ✅ Get the selected earthbound bishop from UIManager (following Archbishop pattern)
        EarthboundBishop selectedEarthboundBishop = null;
        Chessman earthboundBishopChessman = null;
        
        if (UIManager.Instance != null && UIManager.Instance.selectedPiece != null)
        {
            GameObject selectedPiece = UIManager.Instance.selectedPiece;
            selectedEarthboundBishop = selectedPiece.GetComponent<EarthboundBishop>();
            earthboundBishopChessman = selectedPiece.GetComponent<Chessman>();
            
            if (selectedEarthboundBishop == null || earthboundBishopChessman == null)
            {
                Debug.LogError($"[Polarity] Selected piece {selectedPiece.name} is not an EarthboundBishop or missing Chessman component!");
                return;
            }
        }
        else
        {
            Debug.LogError("[Polarity] No piece selected via UIManager!");
            return;
        }

        string player = earthboundBishopChessman.GetPlayer();

        // Check SP cost (2 SP)
        if (SkillManager.Instance.GetPlayerSP(player) < 2)
        {
            Debug.Log("[Polarity] Not enough SP!");
            return;
        }

        // Check if Polarity is on cooldown (10 turns)
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "Polarity"))
        {
            Debug.Log("[Polarity] Skill is on cooldown!");
            return;
        }

        // Deduct SP
        SkillManager.Instance.SpendPlayerSP(player, 2);

        // Start Polarity cooldown (10 turns)
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.StartCooldown(player, "Polarity", CooldownManager.CooldownType.TurnBased, 10);
        }

        // Remove all existing move plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // Create selection plates for Terra Wards and Stone Sentinel pieces
        CreatePolaritySelectionPlates();

        Debug.Log("[Polarity] ⚡ POLARITY ACTIVATED! Select Terra Ward or Stone Sentinel piece...");
    }

    /// <summary>
    /// Create selection plates for Polarity targeting (Terra Wards and Stone Sentinel pieces)
    /// </summary>
    private static void CreatePolaritySelectionPlates()
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[Polarity] Could not find Game component for selection plates!");
            return;
        }

        int selectionPlateCount = 0;

        // Create selection plates on Terra Wards and Stone Sentinel pieces
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = game.GetPosition(x, y);
                if (piece != null)
                {
                    Chessman chessman = piece.GetComponent<Chessman>();
                    if (chessman != null)
                    {
                        string pieceName = chessman.name;
                        bool hasStoneSentinel = chessman.statusManager.HasStatus(StatusType.StoneSentinel, game.turns);

                        // Filter out unwanted tile types
                        bool isExcludedTile = (pieceName.StartsWith("tile") && pieceName != "tile_terra_ward") ||
                                             pieceName == "white_ashen_pyre" ||
                                             pieceName == "celestial_pillar";

                        // Check if this is a valid target (Terra Ward, Stone Sentinel pieces, or EarthboundBishop)
                        if (!isExcludedTile && (pieceName == "tile_terra_ward" || hasStoneSentinel))
                        {
                            CreatePolaritySelectionPlate(x, y);
                            selectionPlateCount++;
                        }
                    }
                }
            }
        }

        Debug.Log($"[Polarity] Created {selectionPlateCount} selection plates for targeting.");
    }

    /// <summary>
    /// Create a single selection plate for Polarity targeting
    /// </summary>
    private static void CreatePolaritySelectionPlate(int x, int y)
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null) return;

        float fx = x * 0.57f - 1.98f;
        float fy = y * 0.56f - 1.95f;

        GameObject plate = Instantiate(game.movePlatePrefabReference);
        plate.transform.position = new Vector3(fx, fy, -3f);

        // Add PolaritySelectionPlate component
        PolaritySelectionPlate polarityPlate = plate.AddComponent<PolaritySelectionPlate>();
        polarityPlate.Setup(x, y);

        // Set visual appearance (purple for polarity/magnetic theme)
        plate.GetComponent<SpriteRenderer>().color = new Color(0.8f, 0f, 0.8f, 0.75f);

        Debug.Log($"[Polarity] Created selection plate at ({x},{y})");
    }

    /// <summary>
    /// Generate Queen-like movement paths for Polarity from selected piece
    /// </summary>
    public static void GeneratePolarityMovePlates(int startX, int startY, bool isSelfPropelling)
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[Polarity] Could not find Game component for move plate generation!");
            return;
        }

        int movePlateCount = 0;

        // Generate Queen-like movement (straight lines + diagonals)
        // Horizontal and Vertical (Rook movement)
        GeneratePolarityLine(startX, startY, 1, 0, game, ref movePlateCount);   // Right
        GeneratePolarityLine(startX, startY, -1, 0, game, ref movePlateCount);  // Left
        GeneratePolarityLine(startX, startY, 0, 1, game, ref movePlateCount);   // Up
        GeneratePolarityLine(startX, startY, 0, -1, game, ref movePlateCount);  // Down

        // Diagonal (Bishop movement)
        GeneratePolarityLine(startX, startY, 1, 1, game, ref movePlateCount);   // Up-Right
        GeneratePolarityLine(startX, startY, -1, -1, game, ref movePlateCount); // Down-Left
        GeneratePolarityLine(startX, startY, -1, 1, game, ref movePlateCount);  // Up-Left
        GeneratePolarityLine(startX, startY, 1, -1, game, ref movePlateCount);  // Down-Right

        Debug.Log($"[Polarity] Generated {movePlateCount} move plates for Polarity paths");
    }

    /// <summary>
    /// Generate a line of move plates in a specific direction for Polarity
    /// </summary>
    private static void GeneratePolarityLine(int startX, int startY, int dx, int dy, Game game, ref int movePlateCount)
    {
        int x = startX + dx;
        int y = startY + dy;

        while (game.PositionOnBoard(x, y))
        {
            GameObject existingPiece = game.GetPosition(x, y);

            // Can move to empty tiles or through Stone Sentinel pieces (but not land on them)
            if (existingPiece == null)
            {
                // Empty tile - can land here
                CreatePolarityMovePlate(x, y);
                movePlateCount++;
            }
            else
            {
                Chessman existingChessman = existingPiece.GetComponent<Chessman>();
                if (existingChessman != null)
                {
                    // Check if it's a Stone Sentinel piece (can pass through but not land)
                    if (existingChessman.statusManager.HasStatus(StatusType.StoneSentinel, game.turns))
                    {
                        // Can pass through Stone Sentinel pieces but not land
                        // Continue to next tile in line
                    }
                    else
                    {
                        // Hit a non-Stone Sentinel piece - cannot pass through
                        break;
                    }
                }
                else
                {
                    // Hit a special tile or other object - cannot pass through
                    break;
                }
            }

            x += dx;
            y += dy;
        }
    }

    /// <summary>
    /// Create a single move plate for Polarity destination selection
    /// </summary>
    private static void CreatePolarityMovePlate(int x, int y)
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null) return;

        float fx = x * 0.57f - 1.98f;
        float fy = y * 0.56f - 1.95f;

        GameObject plate = Instantiate(game.movePlatePrefabReference);
        plate.transform.position = new Vector3(fx, fy, -3f);

        // Add PolarityMovePlate component
        PolarityMovePlate polarityMovePlate = plate.AddComponent<PolarityMovePlate>();
        polarityMovePlate.Setup(x, y);

        // Set visual appearance (blue for polarity paths)
        plate.GetComponent<SpriteRenderer>().color = new Color(0f, 0.5f, 1f, 0.75f);

        Debug.Log($"[Polarity] Created move plate at ({x},{y})");
    }
}
