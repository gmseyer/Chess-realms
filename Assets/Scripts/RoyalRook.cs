using UnityEngine;
using System.Collections.Generic;

public class RoyalRook : MonoBehaviour
{
    public GameObject movePlatePrefab;
    private Game game;
    private Chessman chessman; // ✅ Cache Chessman reference for player info
    // Removed: private static bool celestialPillarUsed - now using CooldownManager
    
    // Celestial Synergy passive skill
    private bool celestialSynergy = false;

    private void Awake()
    {
        // Cache Chessman reference (following Bishop pattern)
        chessman = GetComponent<Chessman>();
        if (chessman == null)
            Debug.LogError("[RoyalRook] Missing Chessman component!");
    }

    private void Start()
    {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
    }

    // Celestial Synergy passive skill - check for celestial pillars in 3x3 area
    public bool CheckCelestialSynergy()
    {
        if (game == null) return false;
        
        int rookX = GetComponent<Chessman>().GetXBoard();
        int rookY = GetComponent<Chessman>().GetYBoard();
        
        // Check 3x3 area around the Royal Rook
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int checkX = rookX + dx;
                int checkY = rookY + dy;
                
                if (game.PositionOnBoard(checkX, checkY))
                {
                    GameObject piece = game.GetPosition(checkX, checkY);
                    if (piece != null && piece.name == "celestial_pillar")
                    {
                        celestialSynergy = true;
                        Debug.Log($"[Royal Rook] Celestial Synergy activated! Found celestial pillar at ({checkX},{checkY})");
                        return true;
                    }
                }
            }
        }
        
        celestialSynergy = false;
        return false;
    }

    // Generate Queen-like move plates when Celestial Synergy is active 
    public void GenerateCelestialSynergyMovePlates()
    {
        if (game == null) return;
        
        // Queen movement pattern: horizontal, vertical, and diagonal lines
        // Horizontal and vertical (like Rook)
        LineMovePlate(1, 0);   // Right
        LineMovePlate(-1, 0);  // Left
        LineMovePlate(0, 1);   // Up
        LineMovePlate(0, -1);  // Down
        
        // Diagonal (like Bishop)
        LineMovePlate(1, 1);   // Up-Right
        LineMovePlate(-1, -1); // Down-Left
        LineMovePlate(-1, 1);  // Up-Left
        LineMovePlate(1, -1);  // Down-Right
        
        Debug.Log("[Royal Rook] Celestial Synergy move plates generated - Queen-like movement!");
    }

    // Helper method to generate line move plates (copied from Chessman.cs)
    private void LineMovePlate(int xIncrement, int yIncrement)
    {   
        
        if (game == null) return;
        
        int x = GetComponent<Chessman>().GetXBoard();
        int y = GetComponent<Chessman>().GetYBoard();
        
        while (game.PositionOnBoard(x + xIncrement, y + yIncrement))
        {
            x += xIncrement;
            y += yIncrement;
            
            GameObject target = game.GetPosition(x, y);
            if (target == null)
            {
                // Empty tile
                MovePlateSpawn(x, y);
            }
            else
            {
                // Occupied tile
                Chessman targetCm = target.GetComponent<Chessman>();
                if (targetCm != null)
                {
                    // Special tile (like celestial pillar)
                    if (targetCm.statusManager.HasStatus(StatusType.specialTile, game.turns))
                    {
                        MovePlateSpawn(x, y); // Can land on special tiles
                        x += xIncrement;
                        y += yIncrement;
                        continue;
                    }
                     if (targetCm.statusManager.HasStatus(StatusType.Invulnerable, game.turns))
                    {
                        Debug.Log($"{targetCm.name} is invulnerable. Skipping attack.");
                        break;
                    }
                    
                    // Enemy piece
                    if (targetCm.GetPlayer() != GetComponent<Chessman>().GetPlayer() && !targetCm.isInvulnerable)
                    {
                        MovePlateAttackSpawn(x, y);
                    }
                }
                break; // Stop at first occupied tile
            }
        }
    }

    // Helper method to spawn move plates (copied from Chessman.cs)
    private void MovePlateSpawn(int matrixX, int matrixY)
    {
        float x = matrixX * 0.57f - 1.98f;
        float y = matrixY * 0.56f - 1.95f;
        
        GameObject mp = Instantiate(movePlatePrefab, new Vector3(x, y, -3f), Quaternion.identity);
        MovePlate mpScript = mp.GetComponent<MovePlate>();
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(matrixX, matrixY);
    }

    // Helper method to spawn attack plates (copied from Chessman.cs)
    private void MovePlateAttackSpawn(int matrixX, int matrixY)
    {
        float x = matrixX * 0.57f - 1.98f;
        float y = matrixY * 0.56f - 1.95f;
        
        GameObject mp = Instantiate(movePlatePrefab, new Vector3(x, y, -3f), Quaternion.identity);
        MovePlate mpScript = mp.GetComponent<MovePlate>();
        mpScript.attack = true;
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(matrixX, matrixY);
    }

    // Celestial Pillar Skill
    public void CelestialPillar() 
    {
        // ✅ Get the selected royal rook from UIManager (following Bishop/Queen pattern)
        RoyalRook selectedRoyalRook = null;
        Chessman cm = null;
        
        if (UIManager.Instance != null && UIManager.Instance.selectedPiece != null)
        {
            GameObject selectedPiece = UIManager.Instance.selectedPiece;
            selectedRoyalRook = selectedPiece.GetComponent<RoyalRook>();
            cm = selectedPiece.GetComponent<Chessman>();
            
            if (selectedRoyalRook == null || cm == null)
            {
                Debug.LogError($"[Celestial Pillar] Selected piece {selectedPiece.name} is not a Royal Rook or missing Chessman component!");
                return;
            }
        }
        else
        {
            Debug.LogError("[Celestial Pillar] No piece selected via UIManager!");
            return;
        }
        
        string player = cm.GetPlayer();
        Debug.Log($"[Celestial Pillar] Attempting activation for {player} player...");
        
        // ✅ Check cooldown BEFORE spending SP (using CooldownManager)
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "CelestialPillar"))
        {
            Debug.LogWarning($"[Celestial Pillar] Skill is on cooldown for {player} — cannot use.");
            return;
        }
        
        // Check SP cost (2 SP)
        if (!SkillManager.Instance.SpendPlayerSP(player, 2))
        {
            Debug.LogWarning($"[Celestial Pillar] Not enough SP for {player} to use Celestial Pillar!");
            return;
        }
        
        // ✅ Set cooldown using CooldownManager
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.StartCooldown(player, "CelestialPillar", CooldownManager.CooldownType.OncePerBattle);
        }
        Debug.Log($"[Celestial Pillar] Cooldown activated for {player} - once per battle.");
        
        // Log skill usage
        if (SkillTracker.Instance != null)
        {
            SkillTracker.Instance.LogSkillUsage(player, "ROYAL ROOK", "CELESTIAL PILLAR", 2);
        }
        
        // Spawn move plates on all empty tiles
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (game.GetPosition(x, y) == null)
                    SpawnCelestialPillarPlate(game, x, y);
            }
        }

        Debug.Log($"[Celestial Pillar] Skill activated for {player} - choose a location!");
    }

    // Crushing Advance Skill
    public void CrushingAdvance()
    {
        // ✅ Get the selected royal rook from UIManager (following Bishop/Queen pattern)
        RoyalRook selectedRoyalRook = null;
        Chessman cm = null;
        
        if (UIManager.Instance != null && UIManager.Instance.selectedPiece != null)
        {
            GameObject selectedPiece = UIManager.Instance.selectedPiece;
            selectedRoyalRook = selectedPiece.GetComponent<RoyalRook>();
            cm = selectedPiece.GetComponent<Chessman>();
            
            if (selectedRoyalRook == null || cm == null)
            {
                Debug.LogError($"[Crushing Advance] Selected piece {selectedPiece.name} is not a Royal Rook or missing Chessman component!");
                return;
            }
        }
        else
        {
            Debug.LogError("[Crushing Advance] No piece selected via UIManager!");
            return;
        }
        
        string player = cm.GetPlayer();
        Debug.Log($"[Crushing Advance] Attempting activation for {player} player...");
        
        // ✅ Check cooldown BEFORE spending SP (using CooldownManager)
        if (CooldownManager.Instance != null && CooldownManager.Instance.IsOnCooldown(player, "CrushingAdvance"))
        {
            Debug.LogWarning($"[Crushing Advance] Skill is on cooldown for {player} — cannot use.");
            return;
        }
        
        // Check SP cost (2 SP)
        if (!SkillManager.Instance.SpendPlayerSP(player, 2))
        {
            Debug.LogWarning($"[Crushing Advance] Not enough SP for {player} to use Crushing Advance!");
            return;
        }
        
        // ✅ Set cooldown using CooldownManager
        if (CooldownManager.Instance != null)
        {
            CooldownManager.Instance.StartCooldown(player, "CrushingAdvance", CooldownManager.CooldownType.OncePerBattle);
        }
        Debug.Log($"[Crushing Advance] Cooldown activated for {player} - once per battle.");
        
        // Log skill usage
        if (SkillTracker.Instance != null)
        {
            SkillTracker.Instance.LogSkillUsage(player, "ROYAL ROOK", "CRUSHING ADVANCE", 2);
        }
        
        // Spawn direction selection plates
        selectedRoyalRook.SpawnCrushingAdvanceDirectionPlates();
        
        Debug.Log($"[Crushing Advance] Skill activated for {player} - choose your direction!");
    }

    private void SpawnCrushingAdvanceDirectionPlates()
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        
        // Get Royal Rook's position
        GameObject selectedPiece = UIManager.Instance.selectedPiece;
        if (selectedPiece == null)
        {
            Debug.LogError("[Crushing Advance] No piece selected!");
            return;
        }

        Chessman rookCm = selectedPiece.GetComponent<Chessman>();
        if (rookCm == null)
        {
            Debug.LogError("[Crushing Advance] No Chessman component found on selected piece!");
            return;
        }

        int rookX = rookCm.GetXBoard();
        int rookY = rookCm.GetYBoard();

        // Spawn plates in all 4 directions (4 tiles each)
        SpawnCrushingAdvanceDirection(game, rookX, rookY, 0, 1);   // Up
        SpawnCrushingAdvanceDirection(game, rookX, rookY, 0, -1);  // Down
        SpawnCrushingAdvanceDirection(game, rookX, rookY, 1, 0);   // Right
        SpawnCrushingAdvanceDirection(game, rookX, rookY, -1, 0);  // Left
    }

    private void SpawnCrushingAdvanceDirection(Game game, int startX, int startY, int xIncrement, int yIncrement)
    {
        for (int i = 1; i <= 4; i++) // 4 tiles per direction
        {
            int x = startX + (xIncrement * i);
            int y = startY + (yIncrement * i);

            // Check if position is on board
            if (!game.PositionOnBoard(x, y)) break;

            // Spawn the Crushing Advance plate
            SpawnCrushingAdvancePlate(game, x, y, i, xIncrement, yIncrement);
        }
    }

    private void SpawnCrushingAdvancePlate(Game game, int x, int y, int distance, int xIncrement, int yIncrement)
    {
        // Use the same positioning as other move plates
        float fx = x * 0.57f - 1.98f;
        float fy = y * 0.56f - 1.95f;

        GameObject mp = Instantiate(movePlatePrefab, new Vector3(fx, fy, -3f), Quaternion.identity);

        // Remove default MovePlate script
        MovePlate old = mp.GetComponent<MovePlate>();
        if (old != null) Destroy(old);

        // Add CrushingAdvancePlate script
        CrushingAdvancePlate plate = mp.AddComponent<CrushingAdvancePlate>();
        plate.Setup(game, x, y, distance, xIncrement, yIncrement);

        // Make crushing advance plates visually distinct (orange)
        SpriteRenderer sr = mp.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.yellow; // Orange color for crushing advance
        }
    }

    private void SpawnCelestialPillarPlate(Game game, int x, int y)
    {
        float fx = x * 0.57f - 1.98f;
        float fy = y * 0.56f - 1.95f;

        GameObject mp = Instantiate(movePlatePrefab, new Vector3(fx, fy, -3f), Quaternion.identity);

        // Remove default MovePlate script
        MovePlate oldScript = mp.GetComponent<MovePlate>();
        if (oldScript != null) Destroy(oldScript);

        // Add CelestialPillarPlate script
        CelestialPillarPlate plate = mp.AddComponent<CelestialPillarPlate>();
        plate.Setup(game, x, y);

        // Make celestial pillar plates visually distinct (purple)
        SpriteRenderer sr = mp.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.magenta; // Purple color for celestial pillar plates
        }
    }
}
