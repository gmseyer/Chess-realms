using UnityEngine;
using System.Collections.Generic;

public class King : MonoBehaviour
{
    public GameObject movePlatePrefab; // Move plate prefab for spawning movement tiles
    private Chessman chessman;
    private Game game;
    private int lastPawnCount = -1; // Track last known pawn count for optimization
    
    // Monarch Shield passive variables
    private int monarchShieldCooldown = 0; // 8-turn cooldown between shield activations
    private bool hasMonarchShield = false; // Whether King currently has Monarch Shield active
    private int monarchShieldExpiresOnTurn = -1; // Turn when Monarch Shield expires

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
        int currentTurn = game.turns;
        
        // Last Stand only activates on turn 25 or above
        if (currentTurn < 25) 
        {
            // Use normal King movement + castling before turn 25
            SurroundMovePlate();
            GenerateCastlingMoves();
            Debug.Log($"[King] {player} King using normal movement + castling (turn {currentTurn} < 25)");
            return;
        }
        
        Debug.Log($"[King] {player} King - {currentPawnCount} allied pawns remaining, generating Last Stand movement (turn {currentTurn})");
        
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

    // Count all pieces for the King's player
    private int CountAlliedPieces()
    {
        if (game == null || chessman == null) return 0;
        
        string player = chessman.GetPlayer();
        int pieceCount = 0;
        
        // Count all pieces for the current player
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
                        pieceCount++;
                    }
                }
            }
        }
        
        return pieceCount;
    }

    // Count all pieces for the opponent
    private int CountEnemyPieces()
    {
        if (game == null || chessman == null) return 0;
        
        string player = chessman.GetPlayer();
        string enemyPlayer = (player == "white") ? "black" : "white";
        int pieceCount = 0;
        
        // Count all pieces for the enemy player
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = game.GetPosition(x, y);
                if (piece != null)
                {
                    Chessman pieceCm = piece.GetComponent<Chessman>();
                    if (pieceCm != null && pieceCm.GetPlayer() == enemyPlayer)
                    {
                        pieceCount++;
                    }
                }
            }
        }
        
        return pieceCount;
    }

    // Check if King has fewer pieces than opponent
    private bool HasFewerPiecesThanOpponent()
    {
        int alliedPieces = CountAlliedPieces();
        int enemyPieces = CountEnemyPieces();
        
        Debug.Log($"[King] {chessman.GetPlayer()} King: {alliedPieces} pieces vs {enemyPieces} enemy pieces");
        return alliedPieces < enemyPieces;
    }

    // Activate Monarch Shield (2-turn invulnerability)
    private void ActivateMonarchShield()
    {
        if (chessman == null || game == null) return;
        
        hasMonarchShield = true;
        monarchShieldExpiresOnTurn = game.turns + 2; // 2-turn duration
        
        // Add invulnerable status using StatusManager
        StatusManager statusManager = chessman.GetComponent<StatusManager>();
        if (statusManager != null)
        {
            statusManager.AddStatus(StatusType.Invulnerable, monarchShieldExpiresOnTurn);
        }
        
        // Also set the Chessman invulnerable flag for compatibility
        chessman.isInvulnerable = true;
        chessman.invulnerableUntilTurn = monarchShieldExpiresOnTurn;
        
        Debug.Log($"[King] {chessman.GetPlayer()} King activated Monarch Shield until turn {monarchShieldExpiresOnTurn}");
    }

    // Check and update Monarch Shield passive
    public void UpdateMonarchShield()
    {
        if (chessman == null || game == null) return;
        
        int currentTurn = game.turns;
        string player = chessman.GetPlayer();
        
        // Only activate after turn 40
        if (currentTurn < 40) return;
        
        // Decrease cooldown
        if (monarchShieldCooldown > 0)
        {
            monarchShieldCooldown--;
        }
        
        // Check if Monarch Shield expired
        if (hasMonarchShield && currentTurn >= monarchShieldExpiresOnTurn)
        {
            hasMonarchShield = false;
            monarchShieldExpiresOnTurn = -1;
            Debug.Log($"[King] {player} King Monarch Shield expired at turn {currentTurn}");
        }
        
        // Check if we can activate Monarch Shield
        if (monarchShieldCooldown <= 0 && !hasMonarchShield)
        {
            if (HasFewerPiecesThanOpponent())
            {
                ActivateMonarchShield();
                monarchShieldCooldown = 8; // 8-turn cooldown
                Debug.Log($"[King] {player} King Monarch Shield activated! Cooldown: 8 turns");
            }
        }
    }

    // Generate castling move plates
    private void GenerateCastlingMoves()
    {
        if (chessman == null || game == null)
        {
            Debug.LogError("[King] Missing references for castling!");
            return;
        }

        string player = chessman.GetPlayer();
        int x = chessman.GetXBoard();
        int y = chessman.GetYBoard();

        // Check if King has moved
        if (chessman.GetHasMoved())
        {
            Debug.Log($"[King] {player} King has moved - castling not available");
            return;
        }

        // Check if King is in check
        if (IsKingInCheck())
        {
            Debug.Log($"[King] {player} King is in check - castling not available");
            return;
        }

        // Check King-side castling (right side)
        if (CanCastleKingSide())
        {
            CreateCastlingMovePlate(x + 2, y, "kingside");
            Debug.Log($"[King] {player} King-side castling available");
        }

        // Check Queen-side castling (left side)
        if (CanCastleQueenSide())
        {
            CreateCastlingMovePlate(x - 2, y, "queenside");
            Debug.Log($"[King] {player} Queen-side castling available");
        }
    }

    private bool CanCastleKingSide()
    {
        string player = chessman.GetPlayer();
        int x = chessman.GetXBoard();
        int y = chessman.GetYBoard();

        // Check if right rook exists and hasn't moved
        GameObject rightRook = game.GetPosition(7, y);
        if (rightRook == null) return false;

        Chessman rookChessman = rightRook.GetComponent<Chessman>();
        if (rookChessman == null || rookChessman.GetPlayer() != player || rookChessman.GetHasMoved())
            return false;

        // Check if squares between King and Rook are empty
        if (game.GetPosition(x + 1, y) != null || game.GetPosition(x + 2, y) != null)
            return false;

        // Check if King would move through check
        if (IsSquareUnderAttack(x + 1, y) || IsSquareUnderAttack(x + 2, y))
            return false;

        return true;
    }

    private bool CanCastleQueenSide()
    {
        string player = chessman.GetPlayer();
        int x = chessman.GetXBoard();
        int y = chessman.GetYBoard();

        // Check if left rook exists and hasn't moved
        GameObject leftRook = game.GetPosition(0, y);
        if (leftRook == null) return false;

        Chessman rookChessman = leftRook.GetComponent<Chessman>();
        if (rookChessman == null || rookChessman.GetPlayer() != player || rookChessman.GetHasMoved())
            return false;

        // Check if squares between King and Rook are empty
        if (game.GetPosition(x - 1, y) != null || game.GetPosition(x - 2, y) != null || game.GetPosition(x - 3, y) != null)
            return false;

        // Check if King would move through check
        if (IsSquareUnderAttack(x - 1, y) || IsSquareUnderAttack(x - 2, y))
            return false;

        return true;
    }

    private bool IsKingInCheck()
    {
        // Simple check detection - check if any enemy piece can attack the King
        string player = chessman.GetPlayer();
        int x = chessman.GetXBoard();
        int y = chessman.GetYBoard();

        Chessman[] allPieces = FindObjectsOfType<Chessman>();
        foreach (Chessman piece in allPieces)
        {
            if (piece != null && piece.GetPlayer() != player)
            {
                if (CanPieceAttackSquare(piece, x, y))
                    return true;
            }
        }
        return false;
    }

    private bool IsSquareUnderAttack(int x, int y)
    {
        string player = chessman.GetPlayer();

        Chessman[] allPieces = FindObjectsOfType<Chessman>();
        foreach (Chessman piece in allPieces)
        {
            if (piece != null && piece.GetPlayer() != player)
            {
                if (CanPieceAttackSquare(piece, x, y))
                    return true;
            }
        }
        return false;
    }

    private bool CanPieceAttackSquare(Chessman piece, int targetX, int targetY)
    {
        // This is a simplified version - in a full implementation, you'd check each piece type's attack pattern
        // For now, we'll use a basic distance check for most pieces
        int pieceX = piece.GetXBoard();
        int pieceY = piece.GetYBoard();

        // Basic attack range check (this is simplified - real chess has complex rules)
        int distance = Mathf.Abs(pieceX - targetX) + Mathf.Abs(pieceY - targetY);
        return distance == 1; // Adjacent squares
    }

    private void CreateCastlingMovePlate(int x, int y, string castlingType)
    {
        float fx = x * 0.57f - 1.98f;
        float fy = y * 0.56f - 1.95f;

        GameObject mp = Instantiate(chessman.movePlate, new Vector3(fx, fy, -3f), Quaternion.identity);
        MovePlate movePlateScript = mp.GetComponent<MovePlate>();
        if (movePlateScript != null)
        {
            // Set special properties for castling
            movePlateScript.SetCastling(true);
            movePlateScript.SetCastlingType(castlingType);
            
            // Set the reference to the King (this is crucial!)
            movePlateScript.SetReference(chessman.gameObject);
        }

        Debug.Log($"[King] Created {castlingType} castling move plate at ({x},{y})");
    }
}
