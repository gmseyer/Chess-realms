using UnityEngine;
using System.Collections.Generic;

public class King : MonoBehaviour
{
    public GameObject movePlatePrefab; // Move plate prefab for spawning movement tiles
    private Chessman chessman;
    private Game game;
    private int lastPawnCount = -1; // Track last known pawn count for optimization

    private void Awake()
    {
        // Cache Chessman reference
        chessman = GetComponent<Chessman>();
        if (chessman == null)
        {
            Debug.LogWarning("[King] Chessman component not found in Awake - will retry in Start");
        }
            
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
    }

    private void Start()
    {
        // Ensure Chessman reference is set
        if (chessman == null)
        {
            chessman = GetComponent<Chessman>();
            if (chessman == null)
            {
                // Use coroutine to retry after one frame (common pattern for initialization timing)
                StartCoroutine(InitializeAfterFrame());
                return;
            }
            else
            {
                Debug.Log($"[King] {gameObject.name} - Chessman component found in Start");
            }
        }
        
        // Ensure movePlatePrefab is assigned
        if (movePlatePrefab == null && game != null)
        {
            movePlatePrefab = game.movePlatePrefabReference;
            if (movePlatePrefab == null)
            {
                Debug.LogError($"[King] {gameObject.name} - MovePlate prefab not assigned!");
            }
        }
        
        Debug.Log($"[King] {gameObject.name} Last Stand passive initialized");
    }

    // Coroutine to initialize after one frame (handles timing issues)
    private System.Collections.IEnumerator InitializeAfterFrame()
    {
        yield return null; // Wait one frame
        
        chessman = GetComponent<Chessman>();
        if (chessman != null)
        {
            Debug.Log($"[King] {gameObject.name} - Chessman component found after frame delay");
            
            // Ensure movePlatePrefab is assigned
            
            if (movePlatePrefab == null && game != null)
            {
                movePlatePrefab = game.movePlatePrefabReference;
            }
            
            Debug.Log($"[King] {gameObject.name} Last Stand passive initialized (delayed)");
        }
        else
        {
            Debug.LogWarning($"[King] {gameObject.name} - Chessman component still not found after frame delay");
        }
    }

    // Count allied pawns on the board
    private int CountAlliedPawns()
    {
        if (game == null) return 0;
        
        string player = chessman != null ? chessman.GetPlayer() : "white";
        int pawnCount = 0;
        
        // Count all pieces with "pawn" in their name for the current player
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = game.GetPosition(x, y);
                if (piece != null)
                {
                    Chessman pieceCm = piece.GetComponent<Chessman>();
                    if (pieceCm != null && pieceCm.GetPlayer() == player)
                    {
                        if (piece.name.ToLower().Contains("pawn"))
                        {
                            pawnCount++;
                        }
                    }
                }
            }
        }
        
        return pawnCount;
    }

    // Generate Last Stand movement based on pawn count
    public void GenerateLastStandMovePlates()
    {
        if (chessman == null || game == null)
        {
            Debug.LogError("[King] Missing references for Last Stand movement!");
            return;
        }

        int currentPawnCount = CountAlliedPawns();
        string player = chessman.GetPlayer();
        
        Debug.Log($"[King] {player} King - {currentPawnCount} allied pawns remaining, generating Last Stand movement");
        
        // Clear existing move plates first
        chessman.DestroyMovePlates();
        
        // Generate movement based on pawn count
        if (currentPawnCount >= 8)
        {
            // Normal SurroundMovePlate (current behavior)
            SurroundMovePlate();
            Debug.Log($"[King] {player} King using normal SurroundMovePlate (8+ pawns)");
        }
        else if (currentPawnCount >= 1)
        {
            // LineMovePlate with varying range
            int moveRange = 8 - currentPawnCount; // 7 pawns = 2 range, 6 pawns = 3 range, etc.
            
            if (currentPawnCount == 1)
            {
                // Queen-like movement (Bishop + Rook combined)
                GenerateQueenLikeMovement(moveRange);
                Debug.Log($"[King] {player} King using Queen-like movement (1 pawn)");
            }
            else
            {
                // 8-directional LineMovePlate with limited range
                GenerateLimitedLineMovement(moveRange);
                Debug.Log($"[King] {player} King using {moveRange}-tile range movement ({currentPawnCount} pawns)");
            }
        }
        else
        {
            // No pawns left - same as 1 pawn (Queen movement)
            GenerateQueenLikeMovement(8); // Full range Queen movement
            Debug.Log($"[King] {player} King using full Queen movement (0 pawns)");
        }
    }

    // Generate 8-directional LineMovePlate with limited range
    private void GenerateLimitedLineMovement(int maxRange)
    {
        // 8 directions: up, down, left, right, and 4 diagonals
        LineMovePlate(1, 0, maxRange);   // Right
        LineMovePlate(-1, 0, maxRange);  // Left
        LineMovePlate(0, 1, maxRange);   // Up
        LineMovePlate(0, -1, maxRange);  // Down
        LineMovePlate(1, 1, maxRange);   // Up-Right
        LineMovePlate(-1, 1, maxRange);  // Up-Left
        LineMovePlate(1, -1, maxRange);  // Down-Right
        LineMovePlate(-1, -1, maxRange); // Down-Left
    }

    // Generate Queen-like movement (Bishop + Rook combined)
    private void GenerateQueenLikeMovement(int maxRange)
    {
        // Rook movement (horizontal/vertical)
        LineMovePlate(1, 0, maxRange);   // Right
        LineMovePlate(-1, 0, maxRange);  // Left
        LineMovePlate(0, 1, maxRange);   // Up
        LineMovePlate(0, -1, maxRange);  // Down
        
        // Bishop movement (diagonal)
        LineMovePlate(1, 1, maxRange);   // Up-Right
        LineMovePlate(-1, -1, maxRange); // Down-Left
        LineMovePlate(-1, 1, maxRange);  // Up-Left
        LineMovePlate(1, -1, maxRange);  // Down-Right 
    }

    // Modified LineMovePlate with range limit (copied from Chessman.cs)
    private void LineMovePlate(int xIncrement, int yIncrement, int maxRange)
    {
        if (game == null || chessman == null) return;
        
        int x = chessman.GetXBoard();
        int y = chessman.GetYBoard();
        int tilesMoved = 0;
        string player = chessman.GetPlayer();
        
        while (game.PositionOnBoard(x + xIncrement, y + yIncrement) && tilesMoved < maxRange)
        {
            x += xIncrement;
            y += yIncrement;
            tilesMoved++;
            
            GameObject target = game.GetPosition(x, y);
            if (target == null)
            {
                // Empty tile - can move here
                MovePlateSpawn(x, y);
            }
            else
            {
                // Occupied tile - check if it's an enemy
                Chessman targetCm = target.GetComponent<Chessman>();
                if (targetCm != null && targetCm.GetPlayer() != player)
                {
                    // Enemy piece - can capture
                    if (!targetCm.isInvulnerable)
                    {
                        Debug.Log($"[King] {targetCm.name} is enemy. MovePlateAttackSpawn activated.");
                        MovePlateAttackSpawn(x, y);
                    }
                    else
                    {
                        Debug.Log($"[King] {targetCm.name} is invulnerable. Cannot attack.");
                    }
                }
                else
                {
                    Debug.Log($"[King] {targetCm?.name ?? "Unknown"} is friendly. Cannot move there.");
                }
                // Stop movement at first occupied tile (normal chess rules)
                break;
            }
        }
    }

    // SurroundMovePlate (copied from Chessman.cs)
    private void SurroundMovePlate()
    {
        if (game == null || chessman == null) return;
        
        int x = chessman.GetXBoard();
        int y = chessman.GetYBoard();
        
        // Check all 8 surrounding positions
        PointMovePlate(x + 1, y + 1);    // Up-Right
        PointMovePlate(x - 1, y + 1);    // Up-Left
        PointMovePlate(x + 1, y - 1);    // Down-Right
        PointMovePlate(x - 1, y - 1);    // Down-Left
        PointMovePlate(x + 1, y);        // Right
        PointMovePlate(x - 1, y);        // Left
        PointMovePlate(x, y + 1);        // Up
        PointMovePlate(x, y - 1);        // Down
    }

    // PointMovePlate (copied from Chessman.cs)
    private void PointMovePlate(int x, int y)
    {
        if (game == null || chessman == null) return;
        
        if (game.PositionOnBoard(x, y))
        {
            GameObject target = game.GetPosition(x, y);
            if (target == null)
            {
                // Empty tile - can move here
                MovePlateSpawn(x, y);
            }
            else
            {
                // Occupied tile - check if it's an enemy
                Chessman targetCm = target.GetComponent<Chessman>();
                if (targetCm != null && targetCm.GetPlayer() != chessman.GetPlayer())
                {
                    // Enemy piece - can capture
                    if (!targetCm.isInvulnerable)
                    {
                        Debug.Log($"[King] {targetCm.name} is enemy. MovePlateAttackSpawn activated.");
                        MovePlateAttackSpawn(x, y);
                    }
                    else
                    {
                        Debug.Log($"[King] {targetCm.name} is invulnerable. Cannot attack.");
                    }
                }
                else
                {
                    Debug.Log($"[King] {targetCm?.name ?? "Unknown"} is friendly. Cannot move there.");
                }
            }
        }
    }

    // MovePlateSpawn (copied from Chessman.cs)
    private void MovePlateSpawn(int matrixX, int matrixY)
    {
        if (chessman == null) return;
        
        if (movePlatePrefab == null)
        {
            Debug.LogError("[King] MovePlate prefab is null - cannot spawn move plate!");
            return;
        }
        
        float x = matrixX * 0.57f - 1.98f;
        float y = matrixY * 0.56f - 1.95f;
        
        GameObject mp = Instantiate(movePlatePrefab, new Vector3(x, y, -3f), Quaternion.identity);
        MovePlate movePlate = mp.GetComponent<MovePlate>();
        movePlate.SetReference(gameObject);
        movePlate.SetCoords(matrixX, matrixY);
    }

    // MovePlateAttackSpawn (copied from Chessman.cs)
    private void MovePlateAttackSpawn(int matrixX, int matrixY)
    {
        if (chessman == null) return;
        
        if (movePlatePrefab == null)
        {
            Debug.LogError("[King] MovePlate prefab is null - cannot spawn attack plate!");
            return;
        }
        
        float x = matrixX * 0.57f - 1.98f;
        float y = matrixY * 0.56f - 1.95f;
        
        GameObject mp = Instantiate(movePlatePrefab, new Vector3(x, y, -3f), Quaternion.identity);
        MovePlate movePlate = mp.GetComponent<MovePlate>();
        movePlate.attack = true;
        movePlate.SetReference(gameObject);
        movePlate.SetCoords(matrixX, matrixY);
    }

    // Public method to check if pawn count changed (called every turn)
    public void UpdateLastStandMovement()
    {
        if (chessman == null || game == null)
        {
            return; // Skip if references not ready
        }

        int currentPawnCount = CountAlliedPawns();
        
        // Only update if pawn count changed (optimization)
        if (currentPawnCount != lastPawnCount)
        {
            lastPawnCount = currentPawnCount;
            string player = chessman.GetPlayer();
            Debug.Log($"[King] {player} King pawn count changed to {currentPawnCount} - movement will update when selected");
        }
    }

    // Public method to force generate move plates (called when King is selected)
    public void ForceGenerateLastStandMovePlates()
    {
        GenerateLastStandMovePlates();
    }

    // Public method to get current pawn count
    public int GetCurrentPawnCount()
    {
        return CountAlliedPawns();
    }
}
