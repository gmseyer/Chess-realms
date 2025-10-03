using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple chess bot that makes random legal moves
/// Reuses existing move generation and execution logic
/// </summary>
public class ChessBot : MonoBehaviour
{
    [Header("Bot Settings")]
    public float thinkingDelay = 1f; // Delay between moves
    public bool debugMode = true;
    public bool useAdvancedThinking = true; // Enable 5-second thinking for depth calculations
    
    private Game game;
    private BotModeManager botManager;
    private ChessBotScoring scoringSystem; // Add scoring system reference
    private List<GameObject> temporaryMovePlates = new List<GameObject>();
    private bool isGeneratingMoves = false; // Flag to bypass bot mode check
    
    private void Start()
    {
        game = FindObjectOfType<Game>();
        botManager = FindObjectOfType<BotModeManager>();
        scoringSystem = FindObjectOfType<ChessBotScoring>();
        
        if (scoringSystem == null)
        {
            Debug.LogError("[ChessBot] ChessBotScoring not found! Please add ChessBotScoring to the scene.");
        }
        else
        {
            // Reset piece tracking for new game
            scoringSystem.ResetPieceTracking();
        }
        
        // Reset momentum cooldown for testing (remove this in production)
        if (debugMode)
        {
            Debug.Log("[ChessBot] Debug mode enabled - momentum cooldown should be available");
        }
    }
    
    /// <summary>
    /// Main bot turn handler - called when it's black's turn
    /// </summary>
    public void MakeBotMove()
    {
        if (debugMode)
            Debug.Log("[ChessBot] Bot is thinking...");
            
        StartCoroutine(ExecuteBotMove());
    }
    
    /// <summary>
    /// Execute bot move with delay
    /// </summary>
    private IEnumerator ExecuteBotMove()
    {
        // Calculate thinking time based on bot settings
        float actualThinkingTime = thinkingDelay;
        
        if (useAdvancedThinking && scoringSystem != null && scoringSystem.enableDepthSearch)
        {
            actualThinkingTime = scoringSystem.thinkingTime;
            if (debugMode)
                Debug.Log($"[ChessBot] Using advanced thinking time: {actualThinkingTime} seconds");
        }
        
        // Wait for thinking delay
        yield return new WaitForSeconds(actualThinkingTime);
        
        // Find all black pieces
        List<GameObject> blackPieces = FindAllBlackPieces();
        
        if (blackPieces.Count == 0)
        {
            Debug.Log("[ChessBot] No black pieces found!");
            yield break;
        }
        
        if (debugMode)
            Debug.Log($"[ChessBot] Found {blackPieces.Count} black pieces");
        
        // Generate all possible moves
        List<BotMove> possibleMoves = GenerateAllPossibleMoves(blackPieces);
        
        if (possibleMoves.Count == 0)
        {
            Debug.Log("[ChessBot] No possible moves found!");
            yield break;
        }
        
        if (debugMode)
            Debug.Log($"[ChessBot] Found {possibleMoves.Count} possible moves");
        
        // Use scoring system to find the best move
        BotMove selectedMove = null;
        
        if (scoringSystem != null)
        {
            selectedMove = scoringSystem.FindBestMove(possibleMoves);
        }
        else
        {
            // Fallback to random move if scoring system not available
            selectedMove = possibleMoves[Random.Range(0, possibleMoves.Count)];
            if (debugMode)
                Debug.Log("[ChessBot] Scoring system not available, using random move");
        }
        
        if (selectedMove == null)
        {
            Debug.LogError("[ChessBot] No move selected!");
            yield break;
        }
        
        if (debugMode)
            Debug.Log($"[ChessBot] Selected move: {selectedMove.pieceName} from ({selectedMove.fromX},{selectedMove.fromY}) to ({selectedMove.toX},{selectedMove.toY})");
        
        // Execute the move
        yield return StartCoroutine(ExecuteMove(selectedMove));
    }
    
    /// <summary>
    /// Find all black pieces on the board
    /// </summary>
    private List<GameObject> FindAllBlackPieces()
    {
        List<GameObject> blackPieces = new List<GameObject>();
        
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject piece = game.GetPosition(x, y);
                if (piece != null && piece.name.StartsWith("black"))
                {
                    blackPieces.Add(piece);
                }
            }
        }
        
        return blackPieces;
    }
    
    /// <summary>
    /// Generate all possible moves for all black pieces
    /// </summary>
    private List<BotMove> GenerateAllPossibleMoves(List<GameObject> blackPieces)
    {
        List<BotMove> allMoves = new List<BotMove>();
        
        foreach (GameObject piece in blackPieces)
        {
            Chessman chessman = piece.GetComponent<Chessman>();
            if (chessman != null)
            {
                List<BotMove> pieceMoves = GenerateMovesForPiece(chessman);
                allMoves.AddRange(pieceMoves);
            }
        }
        
        return allMoves;
    }
    
    /// <summary>
    /// Generate moves for a specific piece by temporarily enabling move plates
    /// </summary>
    private List<BotMove> GenerateMovesForPiece(Chessman piece)
    {
        List<BotMove> moves = new List<BotMove>();
        
        if (debugMode)
            Debug.Log($"[ChessBot] Generating moves for {piece.name}");
        
        // Set flag to bypass bot mode check
        isGeneratingMoves = true;
        
        // Temporarily enable move plates for this piece
        piece.InitiateMovePlates();
        
        // Find all move plates that were created
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        
        foreach (GameObject movePlate in movePlates)
        {
            MovePlate mpScript = movePlate.GetComponent<MovePlate>();
            if (mpScript != null)
            {
                // Check if this move plate belongs to our piece
                GameObject reference = mpScript.GetReference();
                if (reference == piece.gameObject)
                {
                    int toX = mpScript.GetMatrixX();
                    int toY = mpScript.GetMatrixY();
                    bool isAttack = mpScript.attack;
                    
                    // Find target if it's an attack
                    GameObject target = null;
                    if (isAttack)
                    {
                        target = game.GetPosition(toX, toY);
                    }
                    
                    // Create bot move
                    BotMove move = new BotMove(
                        piece.gameObject,
                        piece.GetXBoard(),
                        piece.GetYBoard(),
                        toX,
                        toY,
                        isAttack,
                        target
                    );
                    
                    moves.Add(move);
                    
                    if (debugMode)
                        Debug.Log($"[ChessBot] Found move: {piece.name} to ({toX},{toY}) - Attack: {isAttack}");
                }
            }
        }
        
        // Reset flag
        isGeneratingMoves = false;
        
        // Clean up move plates
        CleanupMovePlates();
        
        return moves;
    }
    
    /// <summary>
    /// Execute a selected move by simulating MovePlate click
    /// </summary>
    private IEnumerator ExecuteMove(BotMove move)
    {
        if (debugMode)
            Debug.Log($"[ChessBot] Executing move: {move.pieceName} to ({move.toX},{move.toY})");
        
        // Find the piece
        Chessman piece = move.piece.GetComponent<Chessman>();
        if (piece == null)
        {
            Debug.LogError($"[ChessBot] Chessman component not found on {move.piece.name}");
            yield break;
        }
        
        // Set flag to bypass bot mode check
        isGeneratingMoves = true;
        
        // Generate move plates for this piece
        piece.InitiateMovePlates();
        
        // Find the specific move plate we want to click
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        GameObject targetMovePlate = null;
        
        foreach (GameObject movePlate in movePlates)
        {
            MovePlate mpScript = movePlate.GetComponent<MovePlate>();
            if (mpScript != null)
            {
                GameObject reference = mpScript.GetReference();
                if (reference == piece.gameObject)
                {
                    int toX = mpScript.GetMatrixX();
                    int toY = mpScript.GetMatrixY();
                    bool isAttack = mpScript.attack;
                    
                    // Check if this is the move we want
                    if (toX == move.toX && toY == move.toY && isAttack == move.isAttack)
                    {
                        targetMovePlate = movePlate;
                        break;
                    }
                }
            }
        }
        
        if (targetMovePlate != null)
        {
            if (debugMode)
                Debug.Log($"[ChessBot] Found target move plate, simulating click");
            
            // Simulate clicking the move plate
            targetMovePlate.GetComponent<MovePlate>().OnMouseUp();
            
            // Wait a frame for the move to complete
            yield return null;
        }
        else
        {
            Debug.LogError($"[ChessBot] Could not find target move plate for move to ({move.toX},{move.toY})");
        }
        
        // Reset flag
        isGeneratingMoves = false;
        
        // Record the piece move for tracking
        if (scoringSystem != null)
        {
            scoringSystem.RecordPieceMove(move.pieceName);
            scoringSystem.RecordMoveNotation(move.fromX, move.fromY, move.toX, move.toY);
            
        }
        
        // Note: Knight momentum is now handled directly in MovePlate.cs when capture occurs
        
        // Clean up any remaining move plates
        CleanupMovePlates();
    }
    
    /// <summary>
    /// Handle bot-specific momentum system after knight captures
    /// </summary>
    private IEnumerator HandleBotMomentum(GameObject knightPiece)
    {
        if (debugMode)
            Debug.Log("[ChessBot] HandleBotMomentum called!");
        
        // Check cooldown status for debugging
        if (debugMode)
        {
            Chessman chessmanComponent = knightPiece.GetComponent<Chessman>();
            if (chessmanComponent != null)
            {
                string player = chessmanComponent.GetPlayer();
                if (CooldownManager.Instance != null)
                {
                    bool onCooldown = CooldownManager.Instance.IsOnCooldown(player, "KnightsMomentum");
                    Debug.Log($"[ChessBot] CooldownManager check: {player} KnightsMomentum on cooldown = {onCooldown}");
                }
                else
                {
                    Debug.Log("[ChessBot] CooldownManager.Instance is null!");
                }
            }
        }
        
        if (knightPiece == null)
        {
            if (debugMode)
                Debug.Log("[ChessBot] No knight piece for momentum");
            yield break;
        }
        
        Knight knight = knightPiece.GetComponent<Knight>();
        if (knight == null)
        {
            if (debugMode)
                Debug.Log("[ChessBot] No Knight component found");
            yield break;
        }
        
        // Check if momentum is ready
        bool momentumReady = knight.IsMomentumReady();
        if (debugMode)
            Debug.Log($"[ChessBot] Knight momentum check: IsMomentumReady() = {momentumReady}");
        
        if (!momentumReady)
        {
            if (debugMode)
                Debug.Log("[ChessBot] Knight momentum not ready (on cooldown)");
            yield break;
        }
        
        if (debugMode)
            Debug.Log("[ChessBot] Knight momentum is ready! Evaluating best destination...");
        
        // Find all possible momentum destinations (empty squares in first 4 ranks)
        List<Vector2Int> possibleDestinations = new List<Vector2Int>();
        
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 4; y++) // Only first 4 ranks like player momentum
            {
                if (game.GetPosition(x, y) == null) // Empty square
                {
                    possibleDestinations.Add(new Vector2Int(x, y));
                }
            }
        }
        
        if (possibleDestinations.Count == 0)
        {
            if (debugMode)
                Debug.Log("[ChessBot] No valid momentum destinations found");
            yield break;
        }
        
        // Score all possible destinations
        Vector2Int bestDestination = possibleDestinations[0];
        int bestScore = int.MinValue;
        
        foreach (Vector2Int destination in possibleDestinations)
        {
            int score = ScoreMomentumDestination(knightPiece, destination.x, destination.y);
            
            if (score > bestScore)
            {
                bestScore = score;
                bestDestination = destination;
            }
            
            if (debugMode)
                Debug.Log($"[ChessBot] Destination ({destination.x},{destination.y}) score: {score}");
        }
        
        // Safety check: Don't choose negative scores unless no better option exists
        if (bestScore < 0 && possibleDestinations.Count > 1)
        {
            if (debugMode)
                Debug.LogWarning($"[ChessBot] WARNING: Best score is negative ({bestScore})! Looking for safer alternatives...");
            
            // Try to find a non-negative score
            Vector2Int saferDestination = possibleDestinations[0];
            int saferScore = int.MinValue;
            
            foreach (Vector2Int destination in possibleDestinations)
            {
                int score = ScoreMomentumDestination(knightPiece, destination.x, destination.y);
                if (score >= 0 && score > saferScore)
                {
                    saferScore = score;
                    saferDestination = destination;
                }
            }
            
            if (saferScore >= 0)
            {
                bestDestination = saferDestination;
                bestScore = saferScore;
                if (debugMode)
                    Debug.Log($"[ChessBot] Found safer destination ({saferDestination.x},{saferDestination.y}) with score: {saferScore}");
            }
        }
        
        if (debugMode)
            Debug.Log($"[ChessBot] Best momentum destination: ({bestDestination.x},{bestDestination.y}) with score: {bestScore}");
        
        // Execute the momentum teleport
        yield return new WaitForSeconds(0.3f); // Small delay for visual effect
        
        if (debugMode)
            Debug.Log($"[ChessBot] Executing momentum teleport to ({bestDestination.x},{bestDestination.y})");
        
        // Get knight's current position before teleport
        Chessman knightChessman = knightPiece.GetComponent<Chessman>();
        if (knightChessman != null)
        {
            if (debugMode)
                Debug.Log($"[ChessBot] Knight current position: ({knightChessman.GetXBoard()},{knightChessman.GetYBoard()})");
        }
        
        // Call the knight's momentum teleport directly
        knight.ExecuteMomentumTeleport(bestDestination.x, bestDestination.y, startCooldown: true, skipNextTurn: true);
        
        // Check knight's position after teleport
        yield return new WaitForSeconds(0.1f);
        if (knightChessman != null)
        {
            if (debugMode)
                Debug.Log($"[ChessBot] Knight position after teleport: ({knightChessman.GetXBoard()},{knightChessman.GetYBoard()})");
        }
        
        if (debugMode)
            Debug.Log("[ChessBot] Bot momentum teleport completed!");
        
        // End the turn after momentum is complete
        if (game != null)
        {
            if (debugMode)
                Debug.Log("[ChessBot] Ending turn after momentum teleport");
            game.NextTurn();
        }
    }
    
    /// <summary>
    /// Handle bot momentum directly (called from MovePlate)
    /// </summary>
    public void HandleBotMomentumDirectly(GameObject knightPiece)
    {
        if (debugMode)
            Debug.Log("[ChessBot] HandleBotMomentumDirectly called from MovePlate!");
        
        // Start the momentum coroutine
        StartCoroutine(HandleBotMomentumDirectlyCoroutine(knightPiece));
    }
    
    private IEnumerator HandleBotMomentumDirectlyCoroutine(GameObject knightPiece)
    {
        // Wait a moment for the capture to complete
        yield return new WaitForSeconds(0.1f);
        
        // Clean up any move plates that might have appeared
        CleanupMovePlates();
        
        // Call the main momentum handler
        yield return StartCoroutine(HandleBotMomentum(knightPiece));
    }
    
    /// <summary>
    /// Static method to handle bot momentum (called from MovePlate)
    /// </summary>
    public static void TriggerBotMomentum(GameObject knightPiece)
    {
        ChessBot chessBot = FindObjectOfType<ChessBot>();
        if (chessBot != null)
        {
            Debug.Log("[ChessBot] TriggerBotMomentum called from MovePlate!");
            chessBot.StartCoroutine(chessBot.HandleBotMomentum(knightPiece));
        }
        else
        {
            Debug.LogError("[ChessBot] ChessBot not found for momentum!");
        }
    }
    
    /// <summary>
    /// Score a momentum destination for the bot
    /// </summary>
    private int ScoreMomentumDestination(GameObject knightPiece, int x, int y)
    {
        int score = 0;
        
        // 1. Center control bonus (e4, d4, e5, d5) - HIGHEST PRIORITY
        if ((x == 3 || x == 4) && (y == 3 || y == 4))
        {
            score += 8; // Very high bonus for key center squares
            if (debugMode)
                Debug.Log($"[ChessBot] KEY CENTER bonus for ({x},{y}): +8");
        }
        else if ((x >= 2 && x <= 5) && (y >= 2 && y <= 5))
        {
            score += 4; // High bonus for center area
            if (debugMode)
                Debug.Log($"[ChessBot] Center area bonus for ({x},{y}): +4");
        }
        
        // 2. Safety check - CRITICAL: avoid squares under attack
        if (IsPositionUnderAttack(x, y, "black"))
        {
            score -= 25; // Very heavy penalty for blundering (enemy can take for free)
            if (debugMode)
                Debug.Log($"[ChessBot] BLUNDER penalty for ({x},{y}): -25 (under attack)");
        }
        else
        {
            score += 3; // Good bonus for safe squares
            if (debugMode)
                Debug.Log($"[ChessBot] Safety bonus for ({x},{y}): +3");
        }
        
        // 2.5. Additional safety check - avoid squares that can be easily attacked next turn
        if (IsPositionEasilyAttacked(x, y, "black"))
        {
            score -= 10; // Penalty for positions that can be easily attacked
            if (debugMode)
                Debug.Log($"[ChessBot] EASY ATTACK penalty for ({x},{y}): -10 (can be easily attacked)");
        }
        
        // 3. Edge penalty - discourage edge positions
        if (x == 0 || x == 1 || x == 6 || x == 7)
        {
            score -= 3; // Penalty for edge positions
            if (debugMode)
                Debug.Log($"[ChessBot] Edge penalty for ({x},{y}): -3");
        }
        
        // 4. Mobility bonus (how many moves available from this position)
        int mobility = CalculateMobilityFromPosition(x, y, knightPiece);
        score += mobility;
        
        if (debugMode)
            Debug.Log($"[ChessBot] Mobility for ({x},{y}): +{mobility}");
        
        // 5. Attack potential - bonus for squares that can attack enemy pieces (with risk consideration)
        int attackPotential = CalculateAttackPotentialWithRisk(x, y, knightPiece);
        score += attackPotential;
        
        if (attackPotential > 0 && debugMode)
            Debug.Log($"[ChessBot] Attack potential for ({x},{y}): +{attackPotential}");
        else if (attackPotential < 0 && debugMode)
            Debug.Log($"[ChessBot] Attack risk for ({x},{y}): {attackPotential} (risky attack)");
        
        // 6. Outpost bonus - squares protected by pawns
        if (IsPositionProtectedByPawns(x, y, "black"))
        {
            score += 2;
            if (debugMode)
                Debug.Log($"[ChessBot] Outpost bonus for ({x},{y}): +2");
        }
        
        if (debugMode)
            Debug.Log($"[ChessBot] FINAL SCORE for ({x},{y}): {score}");
        
        // Extra warning for risky positions
        if (score < 0 && debugMode)
        {
            Debug.LogWarning($"[ChessBot] WARNING: Position ({x},{y}) has NEGATIVE score: {score} - This is risky!");
        }
        
        return score;
    }
    
    /// <summary>
    /// Check if a position is under attack by opponent pieces
    /// </summary>
    private bool IsPositionUnderAttack(int x, int y, string defendingPlayer)
    {
        string attackingPlayer = defendingPlayer == "white" ? "black" : "white";
        
        Chessman[] allChessmen = FindObjectsOfType<Chessman>();
        
        foreach (Chessman chessman in allChessmen)
        {
            if (chessman == null || chessman.GetPlayer() != attackingPlayer) continue;
            
            // Get possible moves for this attacking piece
            List<BotMove> possibleMoves = GenerateMovesForPiece(chessman);
            
            // Check if any move can attack the target position
            foreach (BotMove move in possibleMoves)
            {
                if (move.isAttack && move.toX == x && move.toY == y)
                {
                    return true; // Position is under attack
                }
            }
        }
        
        return false; // Position is safe
    }
    
    /// <summary>
    /// Calculate mobility (number of legal moves) from a position
    /// </summary>
    private int CalculateMobilityFromPosition(int x, int y, GameObject knightPiece)
    {
        // Simulate knight movement patterns from this position
        int[] knightMoves = { -2, -1, 1, 2 };
        int legalMoves = 0;
        
        foreach (int dx in knightMoves)
        {
            foreach (int dy in knightMoves)
            {
                if (Mathf.Abs(dx) != Mathf.Abs(dy)) // Valid knight move
                {
                    int newX = x + dx;
                    int newY = y + dy;
                    
                    // Check if move is on board
                    if (newX >= 0 && newX < 8 && newY >= 0 && newY < 8)
                    {
                        // Check if destination is empty or contains enemy piece
                        GameObject pieceAtDestination = game.GetPosition(newX, newY);
                        if (pieceAtDestination == null || 
                            (pieceAtDestination.GetComponent<Chessman>() != null && 
                             pieceAtDestination.GetComponent<Chessman>().GetPlayer() == "white"))
                        {
                            legalMoves++;
                        }
                    }
                }
            }
        }
        
        return legalMoves;
    }
    
    /// <summary>
    /// Calculate attack potential from a position
    /// </summary>
    private int CalculateAttackPotential(int x, int y, GameObject knightPiece)
    {
        int attackPotential = 0;
        
        // Simulate knight moves and check for enemy pieces
        int[] knightMoves = { -2, -1, 1, 2 };
        
        foreach (int dx in knightMoves)
        {
            foreach (int dy in knightMoves)
            {
                if (Mathf.Abs(dx) != Mathf.Abs(dy)) // Valid knight move
                {
                    int newX = x + dx;
                    int newY = y + dy;
                    
                    // Check if move is on board
                    if (newX >= 0 && newX < 8 && newY >= 0 && newY < 8) 
                    {
                        // Check if destination contains enemy piece
                        GameObject pieceAtDestination = game.GetPosition(newX, newY);
                        if (pieceAtDestination != null)
                        {
                            Chessman enemyPiece = pieceAtDestination.GetComponent<Chessman>();
                            if (enemyPiece != null && enemyPiece.GetPlayer() == "white")
                            {
                                // Add value based on piece type
                                string pieceName = pieceAtDestination.name.ToLower();
                                if (pieceName.Contains("pawn")) attackPotential += 1;
                                else if (pieceName.Contains("bishop") || pieceName.Contains("knight")) attackPotential += 2;
                                else if (pieceName.Contains("rook")) attackPotential += 3;
                                else if (pieceName.Contains("queen")) attackPotential += 4;
                                else if (pieceName.Contains("king")) attackPotential += 5;
                            }
                        }
                    }
                }
            }
        }
        
        return attackPotential;
    }
    
    /// <summary>
    /// Check if a position is protected by pawns
    /// </summary>
    private bool IsPositionProtectedByPawns(int x, int y, string player)
    {
        // Check for black pawns protecting this position
        if (player == "black")
        {
            // Check diagonally below (pawns protect from below)
            for (int dy = 1; dy <= 2; dy++)
            {
                for (int dx = -1; dx <= 1; dx += 2)
                {
                    int checkX = x + dx;
                    int checkY = y - dy;
                    
                    if (checkX >= 0 && checkX < 8 && checkY >= 0 && checkY < 8)
                    {
                        GameObject piece = game.GetPosition(checkX, checkY);
                        if (piece != null)
                        {
                            Chessman chessman = piece.GetComponent<Chessman>();
                            if (chessman != null && chessman.GetPlayer() == "black" && 
                                piece.name.ToLower().Contains("pawn"))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Check if a position can be easily attacked next turn
    /// </summary>
    private bool IsPositionEasilyAttacked(int x, int y, string defendingPlayer)
    {
        string attackingPlayer = defendingPlayer == "white" ? "black" : "white";
        
        Chessman[] allChessmen = FindObjectsOfType<Chessman>();
        
        foreach (Chessman chessman in allChessmen)
        {
            if (chessman == null || chessman.GetPlayer() != attackingPlayer) continue;
            
            // Check if this piece can easily attack the position
            List<BotMove> possibleMoves = GenerateMovesForPiece(chessman);
            
            foreach (BotMove move in possibleMoves)
            {
                // If any piece can attack this position in one move
                if (move.toX == x && move.toY == y)
                {
                    // Check if the attacking piece is valuable (not a pawn)
                    string pieceName = chessman.name.ToLower();
                    if (!pieceName.Contains("pawn"))
                    {
                        return true; // Can be easily attacked by valuable piece
                    }
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Calculate attack potential with risk consideration
    /// </summary>
    private int CalculateAttackPotentialWithRisk(int x, int y, GameObject knightPiece)
    {
        int attackPotential = 0;
        int riskPenalty = 0;
        
        // Simulate knight moves and check for enemy pieces
        int[] knightMoves = { -2, -1, 1, 2 };
        
        foreach (int dx in knightMoves)
        {
            foreach (int dy in knightMoves)
            {
                if (Mathf.Abs(dx) != Mathf.Abs(dy)) // Valid knight move
                {
                    int newX = x + dx;
                    int newY = y + dy;
                    
                    // Check if move is on board
                    if (newX >= 0 && newX < 8 && newY >= 0 && newY < 8)
                    {
                        // Check if destination contains enemy piece
                        GameObject pieceAtDestination = game.GetPosition(newX, newY);
                        if (pieceAtDestination != null)
                        {
                            Chessman enemyPiece = pieceAtDestination.GetComponent<Chessman>();
                            if (enemyPiece != null && enemyPiece.GetPlayer() == "white")
                            {
                                // Add value based on piece type
                                string pieceName = pieceAtDestination.name.ToLower();
                                int pieceValue = 0;
                                
                                if (pieceName.Contains("pawn")) pieceValue = 1;
                                else if (pieceName.Contains("bishop") || pieceName.Contains("knight")) pieceValue = 3;
                                else if (pieceName.Contains("rook")) pieceValue = 5;
                                else if (pieceName.Contains("queen")) pieceValue = 9;
                                else if (pieceName.Contains("king")) pieceValue = 10; // High value but risky
                                
                                // Check if attacking this piece puts us at risk
                                if (IsPositionUnderAttack(newX, newY, "white")) // Check if enemy can defend
                                {
                                    // Reduce value if enemy can defend
                                    pieceValue = pieceValue / 2;
                                    if (pieceName.Contains("king"))
                                    {
                                        pieceValue = -5; // Attacking defended king is very risky
                                    }
                                }
                                
                                attackPotential += pieceValue;
                                
                                // Special risk for attacking king
                                if (pieceName.Contains("king"))
                                {
                                    // Check if we can be easily attacked after moving to king
                                    if (IsPositionEasilyAttacked(newX, newY, "black"))
                                    {
                                        riskPenalty -= 15; // Heavy penalty for risky king attack
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        
        return attackPotential + riskPenalty;
    }
    
    /// <summary>
    /// Clean up temporary move plates
    /// </summary>
    private void CleanupMovePlates()
    {
        // Find all MovePlate components and clean up non-special ones
        MovePlate[] allMovePlates = FindObjectsOfType<MovePlate>();
        foreach (MovePlate movePlate in allMovePlates)
        {
            if (movePlate == null || movePlate.gameObject == null) continue;
            
            // Don't destroy special move plates
            if (movePlate.GetComponent<RussianRouletteTargetPlate>() == null &&
                movePlate.GetComponent<RoyalKnightSummonPlate>() == null)
            {
                Destroy(movePlate.gameObject);
            }
        }
    }
    
    /// <summary>
    /// Check if bot is currently generating moves (bypasses bot mode check)
    /// </summary>
    public bool IsGeneratingMoves()
    {
        return isGeneratingMoves;
    }
}

/// <summary>
/// Represents a possible bot move
/// </summary>
[System.Serializable]
public class BotMove
{
    public GameObject piece;
    public string pieceName;
    public int fromX, fromY;
    public int toX, toY;
    public bool isAttack;
    public GameObject target;
    
    public BotMove(GameObject piece, int fromX, int fromY, int toX, int toY, bool isAttack = false, GameObject target = null)
    {
        this.piece = piece;
        this.pieceName = piece.name;
        this.fromX = fromX;
        this.fromY = fromY;
        this.toX = toX;
        this.toY = toY;
        this.isAttack = isAttack;
        this.target = target;
    }
}
