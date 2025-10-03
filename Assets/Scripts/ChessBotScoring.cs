using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles scoring system for chess bot moves
/// Provides piece values and move evaluation
/// </summary>
public class ChessBotScoring : MonoBehaviour
{
    [Header("Piece Values")]
    public int pawnValue = 1;
    public int bishopValue = 3;
    public int knightValue = 3;
    public int rookValue = 5;
    public int queenValue = 9;
    public int kingValue = 100;
    
    [Header("Center Control")]
    public int centerMoveScore = 2;
    public int regularMoveScore = 1;
    
    [Header("Trading System")]
    public bool enableTradingEvaluation = true;
    public float riskMultiplier = 0.8f; // How much to penalize risky moves
    
    [Header("Piece Development")]
    public bool enableDevelopmentPriority = true;
    public int openingMoveThreshold = 10; // First 10 moves are "opening"
    public int pawnDevelopmentBonus = 1; // Reduced from 3 to 1
    public int knightDevelopmentBonus = 1; // Reduced from 2 to 1
    public int bishopDevelopmentBonus = 1;
    public int queenEarlyMovePenalty = -5; // Penalty for moving queen too early
    
    [Header("Position Penalties")]
    public bool enableEdgePenalty = true;
    public int edgeMovePenalty = -2; // Penalty for moving to edge files (a,b,g,h)
    
    [Header("2-Depth Search")]
    public bool enableDepthSearch = true;
    public float thinkingTime = 5f; // Time to think for depth calculations
    public int maxDepthMovesToEvaluate = 20; // Limit moves to prevent crashes
    
    [Header("Defensive System")]
    public bool enableDefensiveScoring = true;
    public int defensiveMoveBonus = 5; // Bonus for moving piece out of danger
    public int defensiveCaptureBonus = 3; // Bonus for capturing attacking piece
    
    [Header("King Safety")]
    public bool enableKingSafety = true;
    public int castlingBonus = 4; // Bonus for castling
    public int kingSafetyBonus = 2; // Bonus for moves that improve king safety
    public int exposedKingPenalty = -3; // Penalty for exposed king
    public int checkAvoidanceBonus = 6; // Bonus for avoiding check
    
    [Header("Piece Mobility")]
    public bool enableMobilityScoring = true;
    public int mobilityBonus = 1; // Bonus per legal move available
    public int outpostBonus = 3; // Bonus for pieces on protected central squares
    
    [Header("Advanced Opening Principles")]
    public bool enableAdvancedOpening = true;
    public int centerControlBonus = 4; // Bonus for controlling e4, d4, e5, d5
    public int samePieceMovePenalty = -2; // Penalty for moving same piece twice
    public int edgePawnEarlyPenalty = -3; // Penalty for moving edge pawns (a/h) early
    public int queenBeforeMove7Penalty = -8; // Heavy penalty for queen before move 7-8
    
    [Header("Debug")]
    public bool debugMode = true;
    
    [Header("Opening Book")]
    public bool enableOpeningBook = true;
    public int maxOpeningMoves = 6; // Use opening book for first 6 moves (3 per player)
    
    // Opening book data
    private Dictionary<string, string[]> openingBook;
    private List<string> gameHistory; // Track moves played in notation
    
    // Track piece movement history for opening principles
    private Dictionary<string, int> pieceMoveCount = new Dictionary<string, int>();
    private int gameMoves = 0;
    
    /// <summary>
    /// Get the value of a piece based on its name
    /// </summary>
    public int GetPieceValue(string pieceName)
    {
        string name = pieceName.ToLower();
        
        if (name.Contains("pawn"))
            return pawnValue;
        else if (name.Contains("bishop"))
            return bishopValue;
        else if (name.Contains("knight"))
            return knightValue;
        else if (name.Contains("rook"))
            return rookValue;
        else if (name.Contains("queen"))
            return queenValue;
        else if (name.Contains("king"))
            return kingValue;
        else
        {
            if (debugMode)
                Debug.LogWarning($"[ChessBotScoring] Unknown piece type: {pieceName}");
            return 0;
        }
    }
    
    /// <summary>
    /// Calculate the score for a potential capture move
    /// </summary>
    public int CalculateCaptureScore(GameObject targetPiece)
    {
        if (targetPiece == null)
            return 0;
            
        Chessman targetChessman = targetPiece.GetComponent<Chessman>();
        if (targetChessman == null)
            return 0;
            
        int pieceValue = GetPieceValue(targetChessman.name);
        
        if (debugMode)
            Debug.Log($"[ChessBotScoring] Capture score for {targetChessman.name}: {pieceValue}");
            
        return pieceValue;
    }
    
    /// <summary>
    /// Check if a position is in the center of the board (x=3,4,5 and y=3,4,5)
    /// </summary>
    private bool IsCenterPosition(int x, int y)
    {
        return (x >= 3 && x <= 5) && (y >= 3 && y <= 5);
    }
    
    /// <summary>
    /// Calculate the score for moving towards center
    /// </summary>
    private int CalculateCenterScore(int fromX, int fromY, int toX, int toY)
    {
        bool fromCenter = IsCenterPosition(fromX, fromY);
        bool toCenter = IsCenterPosition(toX, toY);
        
        // If moving from outside center to center, give bonus
        if (!fromCenter && toCenter)
        {
            return centerMoveScore;
        }
        
        // If moving within center or staying in center, give regular score
        if (toCenter)
        {
            return regularMoveScore;
        }
        
        // If moving away from center, no bonus
        return 0;
    }
    
    /// <summary>
    /// Calculate the risk of losing a piece after making a move
    /// This is a simplified version - in a full implementation, you'd check all enemy pieces
    /// </summary>
    private int CalculateRiskScore(BotMove move)
    {
        if (!enableTradingEvaluation || move.piece == null)
            return 0;
            
        Chessman movingPiece = move.piece.GetComponent<Chessman>();
        if (movingPiece == null)
            return 0;
            
        // Get the value of the piece that might be lost
        int pieceValue = GetPieceValue(movingPiece.name);
        
        // Simplified risk calculation - in reality, you'd check if any enemy piece can attack this position
        // For now, we'll use a basic heuristic based on piece value and position
        bool isInDangerZone = IsPositionInDangerZone(move.toX, move.toY);
        
        if (isInDangerZone)
        {
            // Apply risk multiplier to the piece value
            int riskScore = Mathf.RoundToInt(pieceValue * riskMultiplier);
            
            if (debugMode)
                Debug.Log($"[ChessBotScoring] Risk score for {movingPiece.name} at ({move.toX},{move.toY}): -{riskScore}");
                
            return -riskScore; // Negative because it's a penalty
        }
        
        return 0;
    }
    
    /// <summary>
    /// Check if a position is in a danger zone by looking for actual threats
    /// </summary>
    private bool IsPositionInDangerZone(int x, int y)
    {
        // Check if any opponent piece can attack this position
        Chessman[] allChessmen = FindObjectsOfType<Chessman>();
        string currentPlayer = "black"; // Bot is always black
        
        foreach (Chessman chessman in allChessmen)
        {
            if (chessman == null || chessman.GetPlayer() == currentPlayer) continue;
            
            // Get possible moves for this opponent piece
            List<BotMove> possibleMoves = GenerateMovesForPiece(chessman.gameObject);
            
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
    /// Calculate the net trading value of a move
    /// Takes into account what you gain vs what you might lose
    /// </summary>
    private int CalculateTradingValue(BotMove move)
    {
        int tradingValue = 0;
        
        // What you gain from the move
        int gainValue = 0;
        if (move.isAttack && move.target != null)
        {
            gainValue = CalculateCaptureScore(move.target);
        }
        else
        {
            gainValue = CalculateCenterScore(move.fromX, move.fromY, move.toX, move.toY);
        }
        
        // What you might lose (risk of losing your piece)
        int riskValue = CalculateRiskScore(move);
        
        // Net value = what you gain - what you might lose
        tradingValue = gainValue + riskValue;
        
        if (debugMode && enableTradingEvaluation)
        {
            Debug.Log($"[ChessBotScoring] Trading evaluation for {move.pieceName}: Gain={gainValue}, Risk={riskValue}, Net={tradingValue}");
        }
        
        return tradingValue;
    }
    
    /// <summary>
    /// Check if a piece is still in its starting position
    /// </summary>
    private bool IsPieceInStartingPosition(BotMove move)
    {
        if (move.piece == null) return false;
        
        Chessman chessman = move.piece.GetComponent<Chessman>();
        if (chessman == null) return false;
        
        string player = chessman.GetPlayer();
        bool isWhite = player == "white";
        
        // Check if piece is still on starting rank
        int startingRank = isWhite ? 0 : 7;
        int pawnRank = isWhite ? 1 : 6;
        
        // For pawns, check if on pawn rank
        if (move.pieceName.ToLower().Contains("pawn"))
        {
            return move.fromY == pawnRank;
        }
        
        // For other pieces, check if on back rank
        return move.fromY == startingRank;
    }
    
    /// <summary>
    /// Calculate development bonus for opening moves
    /// </summary>
    private int CalculateDevelopmentBonus(BotMove move, int currentTurn)
    {
        if (!enableDevelopmentPriority || currentTurn > openingMoveThreshold)
            return 0;
            
        if (!IsPieceInStartingPosition(move))
            return 0; // Already developed
            
        string pieceName = move.pieceName.ToLower();
        int developmentBonus = 0;
        
        // Priority order: Pawns > Knights > Bishops > Queen (penalty)
        if (pieceName.Contains("pawn"))
        {
            developmentBonus = pawnDevelopmentBonus;
        }
        else if (pieceName.Contains("knight"))
        {
            developmentBonus = knightDevelopmentBonus;
        }
        else if (pieceName.Contains("bishop"))
        {
            developmentBonus = bishopDevelopmentBonus;
        }
        else if (pieceName.Contains("queen"))
        {
            // Penalty for moving queen too early
            developmentBonus = queenEarlyMovePenalty;
        }
        
        if (debugMode && developmentBonus != 0)
        {
            Debug.Log($"[ChessBotScoring] Development bonus for {move.pieceName}: {developmentBonus} (turn {currentTurn})");
        }
        
        return developmentBonus;
    }
    
    /// <summary>
    /// Check if the current turn is in the opening phase
    /// </summary>
    private int GetCurrentTurn()
    {
        Game game = FindObjectOfType<Game>();
        return game != null ? game.turns : 0;
    }
    
    /// <summary>
    /// Check if a position is on the edge files (a,b,g,h)
    /// Edge files reduce piece activity and should be avoided
    /// </summary>
    private bool IsEdgePosition(int x, int y)
    {
        // Files a(0), b(1), g(6), h(7) are edge files
        return (x == 0 || x == 1 || x == 6 || x == 7);
    }
    
    /// <summary>
    /// Calculate penalty for moving to edge positions
    /// </summary>
    private int CalculateEdgePenalty(BotMove move)
    {
        if (!enableEdgePenalty || move.piece == null)
            return 0;
            
        // Only apply edge penalty to the piece that's actually moving
        // Check if the destination is on an edge file
        if (IsEdgePosition(move.toX, move.toY))
        {
            if (debugMode)
                Debug.Log($"[ChessBotScoring] Edge penalty for {move.pieceName} moving to ({move.toX},{move.toY}): {edgeMovePenalty}");
                
            return edgeMovePenalty;
        }
        
        return 0;
    }
    
    /// <summary>
    /// Calculate 2-depth evaluation for capture moves
    /// Checks if opponent can recapture with a better trade
    /// </summary>
    private int CalculateDepth2Score(BotMove move)
    {
        if (!enableDepthSearch || !move.isAttack || move.target == null)
            return 0;
            
        // Get the value of what we're capturing
        int ourGain = CalculateCaptureScore(move.target);
        
        // Simulate the move and check if opponent can recapture
        int opponentRecaptureValue = SimulateOpponentRecapture(move);
        
        // Net value = what we gain - what they can take back
        int netValue = ourGain - opponentRecaptureValue;
        
        if (debugMode)
        {
            Debug.Log($"[ChessBotScoring] Depth-2: {move.pieceName} captures {move.target.name} (+{ourGain}), opponent can recapture with {opponentRecaptureValue} value = Net: {netValue}");
        }
        
        return netValue;
    }
    
    /// <summary>
    /// Simulate opponent's best recapture after our move
    /// </summary>
    private int SimulateOpponentRecapture(BotMove ourMove)
    {
        if (ourMove.target == null) return 0;
        
        // Get the piece that would be captured
        Chessman capturedPiece = ourMove.target.GetComponent<Chessman>();
        if (capturedPiece == null) return 0;
        
        // Get all pieces that could potentially recapture
        List<GameObject> opponentPieces = GetOpponentPieces(capturedPiece.GetPlayer());
        
        int bestRecaptureValue = 0;
        
        // Check each opponent piece to see if it can recapture
        foreach (GameObject piece in opponentPieces)
        {
            if (piece == null) continue;
            
            Chessman chessman = piece.GetComponent<Chessman>();
            if (chessman == null) continue;
            
            // Get all possible moves for this piece
            List<BotMove> possibleMoves = GenerateMovesForPiece(piece);
            
            // Check if any move captures our piece at the target square
            foreach (BotMove opponentMove in possibleMoves)
            {
                if (opponentMove.isAttack && opponentMove.toX == ourMove.toX && opponentMove.toY == ourMove.toY)
                {
                    // This piece can recapture - get its value
                    int recaptureValue = GetPieceValue(chessman.name);
                    
                    if (recaptureValue > bestRecaptureValue)
                    {
                        bestRecaptureValue = recaptureValue;
                    }
                }
            }
        }
        
        return bestRecaptureValue;
    }
    
    /// <summary>
    /// Get all pieces belonging to the opponent
    /// </summary>
    private List<GameObject> GetOpponentPieces(string currentPlayer)
    {
        List<GameObject> opponentPieces = new List<GameObject>();
        string opponentPlayer = currentPlayer == "white" ? "black" : "white";
        
        // Find all pieces on the board using GameObject.FindObjectsOfType
        Chessman[] allChessmen = FindObjectsOfType<Chessman>();
        
        foreach (Chessman chessman in allChessmen)
        {
            if (chessman != null && chessman.GetPlayer() == opponentPlayer)
            {
                opponentPieces.Add(chessman.gameObject);
            }
        }
        
        return opponentPieces;
    }
    
    /// <summary>
    /// Generate possible moves for a specific piece (simplified version)
    /// </summary>
    private List<BotMove> GenerateMovesForPiece(GameObject piece)
    {
        List<BotMove> moves = new List<BotMove>();
        
        if (piece == null) return moves;
        
        Chessman chessman = piece.GetComponent<Chessman>();
        if (chessman == null) return moves;
        
        // Get current position
        int currentX = chessman.GetXBoard();
        int currentY = chessman.GetYBoard();
        
        // Simplified move generation - just check adjacent squares for captures
        // This is a basic implementation to avoid complexity
        int[] dx = { -1, -1, -1, 0, 0, 1, 1, 1 };
        int[] dy = { -1, 0, 1, -1, 1, -1, 0, 1 };
        
        for (int i = 0; i < dx.Length; i++)
        {
            int newX = currentX + dx[i];
            int newY = currentY + dy[i];
            
            // Check if square is on board
            if (newX >= 0 && newX < 8 && newY >= 0 && newY < 8)
            {
                // Check if there's an opponent piece there
                GameObject targetPiece = GetPieceAt(newX, newY);
                if (targetPiece != null)
                {
                    Chessman targetChessman = targetPiece.GetComponent<Chessman>();
                    if (targetChessman != null && targetChessman.GetPlayer() != chessman.GetPlayer())
                    {
                        // This is a potential capture
                        moves.Add(new BotMove(piece, currentX, currentY, newX, newY, true, targetPiece));
                    }
                }
            }
        }
        
        return moves;
    }
    
    /// <summary>
    /// Get piece at specific coordinates (simplified version)
    /// </summary>
    private GameObject GetPieceAt(int x, int y)
    {
        Chessman[] allChessmen = FindObjectsOfType<Chessman>();
        
        foreach (Chessman chessman in allChessmen)
        {
            if (chessman != null && chessman.GetXBoard() == x && chessman.GetYBoard() == y)
            {
                return chessman.gameObject;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Check if a piece is currently under attack
    /// </summary>
    private bool IsPieceUnderAttack(GameObject piece)
    {
        if (piece == null) return false;
        
        Chessman chessman = piece.GetComponent<Chessman>();
        if (chessman == null) return false;
        
        int pieceX = chessman.GetXBoard();
        int pieceY = chessman.GetYBoard();
        string piecePlayer = chessman.GetPlayer();
        string opponentPlayer = piecePlayer == "white" ? "black" : "white";
        
        // Check if any opponent piece can attack this position
        List<GameObject> opponentPieces = GetOpponentPieces(piecePlayer);
        
        foreach (GameObject opponentPiece in opponentPieces)
        {
            if (opponentPiece == null) continue;
            
            Chessman opponentChessman = opponentPiece.GetComponent<Chessman>();
            if (opponentChessman == null) continue;
            
            // Get possible moves for this opponent piece
            List<BotMove> possibleMoves = GenerateMovesForPiece(opponentPiece);
            
            // Check if any move can capture our piece
            foreach (BotMove move in possibleMoves)
            {
                if (move.isAttack && move.toX == pieceX && move.toY == pieceY)
                {
                    if (debugMode)
                        Debug.Log($"[ChessBotScoring] {piece.name} is under attack by {opponentPiece.name}");
                    return true;
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Calculate defensive bonus for a move
    /// </summary>
    private int CalculateDefensiveBonus(BotMove move)
    {
        if (!enableDefensiveScoring || move.piece == null)
            return 0;
            
        int defensiveBonus = 0;
        
        // Check if the piece is currently under attack
        bool isUnderAttack = IsPieceUnderAttack(move.piece);
        
        if (isUnderAttack)
        {
            // Moving the piece out of danger
            defensiveBonus += defensiveMoveBonus;
            
            if (debugMode)
                Debug.Log($"[ChessBotScoring] Defensive move bonus for {move.pieceName}: +{defensiveMoveBonus} (escaping attack)");
        }
        
        // Check if this move captures an attacking piece
        if (move.isAttack && move.target != null)
        {
            bool targetWasAttacking = IsPieceUnderAttack(move.target);
            
            if (targetWasAttacking)
            {
                defensiveBonus += defensiveCaptureBonus;
                
                if (debugMode)
                    Debug.Log($"[ChessBotScoring] Defensive capture bonus for {move.pieceName}: +{defensiveCaptureBonus} (capturing attacker)");
            }
        }
        
        return defensiveBonus;
    }
    
    /// <summary>
    /// Check if a move is castling
    /// </summary>
    private bool IsCastlingMove(BotMove move)
    {
        if (move.piece == null) return false;
        
        Chessman chessman = move.piece.GetComponent<Chessman>();
        if (chessman == null) return false;
        
        // Check if it's a king move
        if (!move.pieceName.ToLower().Contains("king")) return false;
        
        // Check if king moves 2 squares horizontally (castling)
        int horizontalDistance = Mathf.Abs(move.toX - move.fromX);
        int verticalDistance = Mathf.Abs(move.toY - move.fromY);
        
        // Castling: king moves 2 squares horizontally, 0 vertically
        return horizontalDistance == 2 && verticalDistance == 0;
    }
    
    /// <summary>
    /// Check if king is currently in check
    /// </summary>
    private bool IsKingInCheck(string player)
    {
        // Find the king for this player
        GameObject king = FindKing(player);
        if (king == null) return false;
        
        // Check if king is under attack
        return IsPieceUnderAttack(king);
    }
    
    /// <summary>
    /// Find the king for a specific player
    /// </summary>
    private GameObject FindKing(string player)
    {
        Chessman[] allChessmen = FindObjectsOfType<Chessman>();
        
        foreach (Chessman chessman in allChessmen)
        {
            if (chessman != null && chessman.GetPlayer() == player && chessman.name.ToLower().Contains("king"))
            {
                return chessman.gameObject;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Check if a move puts the king in check
    /// </summary>
    private bool MovePutsKingInCheck(BotMove move)
    {
        if (move.piece == null) return false;
        
        Chessman chessman = move.piece.GetComponent<Chessman>();
        if (chessman == null) return false;
        
        string player = chessman.GetPlayer();
        
        // If this is a king move, check if the destination is under attack
        if (move.pieceName.ToLower().Contains("king"))
        {
            // Temporarily simulate the move and check if new position is under attack
            // For simplicity, we'll check if the destination square is under attack
            GameObject pieceAtDestination = GetPieceAt(move.toX, move.toY);
            
            // If there's a piece at destination, we're capturing it, so it's safe
            if (pieceAtDestination != null) return false;
            
            // Check if destination square is under attack by opponent
            List<GameObject> opponentPieces = GetOpponentPieces(player);
            
            foreach (GameObject opponentPiece in opponentPieces)
            {
                if (opponentPiece == null) continue;
                
                List<BotMove> possibleMoves = GenerateMovesForPiece(opponentPiece);
                
                foreach (BotMove opponentMove in possibleMoves)
                {
                    if (opponentMove.isAttack && opponentMove.toX == move.toX && opponentMove.toY == move.toY)
                    {
                        return true; // Destination is under attack
                    }
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Calculate king safety bonus for a move
    /// </summary>
    private int CalculateKingSafetyBonus(BotMove move)
    {
        if (!enableKingSafety || move.piece == null)
            return 0;
            
        int kingSafetyScore = 0;
        
        Chessman chessman = move.piece.GetComponent<Chessman>();
        if (chessman == null) return 0;
        
        string player = chessman.GetPlayer();
        
        // Check for castling
        if (IsCastlingMove(move))
        {
            kingSafetyScore += castlingBonus;
            
            if (debugMode)
                Debug.Log($"[ChessBotScoring] Castling bonus for {move.pieceName}: +{castlingBonus}");
        }
        
        // Check if move avoids putting king in check
        bool kingCurrentlyInCheck = IsKingInCheck(player);
        bool movePutsKingInCheck = MovePutsKingInCheck(move);
        
        if (kingCurrentlyInCheck && !movePutsKingInCheck)
        {
            kingSafetyScore += checkAvoidanceBonus;
            
            if (debugMode)
                Debug.Log($"[ChessBotScoring] Check avoidance bonus for {move.pieceName}: +{checkAvoidanceBonus}");
        }
        else if (movePutsKingInCheck)
        {
            kingSafetyScore += exposedKingPenalty;
            
            if (debugMode)
                Debug.Log($"[ChessBotScoring] Exposed king penalty for {move.pieceName}: {exposedKingPenalty}");
        }
        
        // General king safety improvement (simplified)
        if (move.pieceName.ToLower().Contains("king"))
        {
            // Moving king towards safety (edges are generally safer for king)
            bool movingTowardsEdge = (move.fromX == 3 || move.fromX == 4) && (move.toX == 2 || move.toX == 5);
            if (movingTowardsEdge)
            {
                kingSafetyScore += kingSafetyBonus;
                
                if (debugMode)
                    Debug.Log($"[ChessBotScoring] King safety improvement for {move.pieceName}: +{kingSafetyBonus}");
            }
        }
        
        return kingSafetyScore;
    }
    
    /// <summary>
    /// Calculate mobility bonus for a move
    /// </summary>
    private int CalculateMobilityBonus(BotMove move)
    {
        if (!enableMobilityScoring || move.piece == null)
            return 0;
            
        int mobilityScore = 0;
        
        // Count legal moves available to this piece after the move
        List<BotMove> possibleMoves = GenerateMovesForPiece(move.piece);
        int legalMoves = possibleMoves.Count;
        
        // Bonus for having more legal moves (mobility)
        mobilityScore += legalMoves * mobilityBonus;
        
        // Check for outpost bonus (protected central square)
        if (IsOutpostPosition(move.toX, move.toY, move.piece))
        {
            mobilityScore += outpostBonus;
            
            if (debugMode)
                Debug.Log($"[ChessBotScoring] Outpost bonus for {move.pieceName} at ({move.toX},{move.toY}): +{outpostBonus}");
        }
        
        if (debugMode && mobilityScore > 0)
        {
            Debug.Log($"[ChessBotScoring] Mobility bonus for {move.pieceName}: +{mobilityScore} ({legalMoves} moves)");
        }
        
        return mobilityScore;
    }
    
    /// <summary>
    /// Check if a position is an outpost (protected central square)
    /// </summary>
    private bool IsOutpostPosition(int x, int y, GameObject piece)
    {
        if (piece == null) return false;
        
        Chessman chessman = piece.GetComponent<Chessman>();
        if (chessman == null) return false;
        
        string player = chessman.GetPlayer();
        string pieceName = chessman.name.ToLower();
        
        // Only knights and bishops can be outposts
        if (!pieceName.Contains("knight") && !pieceName.Contains("bishop"))
            return false;
        
        // Check if position is in center
        bool inCenter = IsCenterPosition(x, y);
        if (!inCenter) return false;
        
        // Check if position is protected by own pawns (simplified)
        bool isProtected = IsPositionProtectedByPawns(x, y, player);
        
        return isProtected;
    }
    
    /// <summary>
    /// Check if a position is protected by pawns (simplified)
    /// </summary>
    private bool IsPositionProtectedByPawns(int x, int y, string player)
    {
        // Get all pawns for this player
        Chessman[] allChessmen = FindObjectsOfType<Chessman>();
        
        foreach (Chessman chessman in allChessmen)
        {
            if (chessman == null) continue;
            if (chessman.GetPlayer() != player) continue;
            if (!chessman.name.ToLower().Contains("pawn")) continue;
            
            int pawnX = chessman.GetXBoard();
            int pawnY = chessman.GetYBoard();
            
            // Check if pawn can protect this square
            bool isWhite = player == "white";
            int pawnDirection = isWhite ? 1 : -1; // White pawns move up (y+), black move down (y-)
            
            // Pawns protect diagonally
            if (pawnY == y - pawnDirection && (pawnX == x - 1 || pawnX == x + 1))
            {
                return true; // Position is protected by a pawn
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Check if a position is one of the key center squares (e4, d4, e5, d5)
    /// </summary>
    private bool IsKeyCenterSquare(int x, int y)
    {
        // e4 = (4,3), d4 = (3,3), e5 = (4,4), d5 = (3,4) for white
        // e5 = (4,3), d5 = (3,3), e4 = (4,4), d4 = (3,4) for black
        return (x == 3 || x == 4) && (y == 3 || y == 4);
    }
    
    /// <summary>
    /// Check if a piece is an edge pawn (a or h file)
    /// </summary>
    private bool IsEdgePawn(BotMove move)
    {
        if (move.piece == null) return false;
        
        string pieceName = move.pieceName.ToLower();
        if (!pieceName.Contains("pawn")) return false;
        
        // Check if pawn is on a-file (x=0) or h-file (x=7)
        return move.fromX == 0 || move.fromX == 7;
    }
    
    /// <summary>
    /// Track piece movement and calculate advanced opening penalties
    /// </summary>
    private int CalculateAdvancedOpeningBonus(BotMove move)
    {
        if (!enableAdvancedOpening || move.piece == null)
            return 0;
            
        int openingScore = 0;
        string pieceName = move.pieceName;
        
        // Get current game move count
        int currentTurn = GetCurrentTurn();
        
        // 1. Center control bonus (e4, d4, e5, d5)
        if (IsKeyCenterSquare(move.toX, move.toY))
        {
            openingScore += centerControlBonus;
            
            if (debugMode)
                Debug.Log($"[ChessBotScoring] Key center control bonus for {pieceName}: +{centerControlBonus}");
        }
        
        // 2. Same piece move penalty (only apply if this piece has moved before in this game)
        if (pieceMoveCount.ContainsKey(pieceName) && pieceMoveCount[pieceName] > 0 && currentTurn <= 15)
        {
            openingScore += samePieceMovePenalty;
            
            if (debugMode)
                Debug.Log($"[ChessBotScoring] Same piece penalty for {pieceName}: {samePieceMovePenalty} (moved {pieceMoveCount[pieceName]} times before)");
        }
        
        // 3. Edge pawn early movement penalty
        if (IsEdgePawn(move) && currentTurn <= 10)
        {
            openingScore += edgePawnEarlyPenalty;
            
            if (debugMode)
                Debug.Log($"[ChessBotScoring] Edge pawn early penalty for {pieceName}: {edgePawnEarlyPenalty}");
        }
        
        // 4. Queen early movement penalty (before move 7-8)
        if (pieceName.ToLower().Contains("queen") && currentTurn <= 7)
        {
            openingScore += queenBeforeMove7Penalty;
            
            if (debugMode)
                Debug.Log($"[ChessBotScoring] Queen early penalty for {pieceName}: {queenBeforeMove7Penalty} (turn {currentTurn})");
        }
        
        return openingScore;
    }
    
    /// <summary>
    /// Record that a piece has moved (call this after a move is actually made)
    /// </summary>
    public void RecordPieceMove(string pieceName)
    {
        if (!pieceMoveCount.ContainsKey(pieceName))
        {
            pieceMoveCount[pieceName] = 0;
        }
        pieceMoveCount[pieceName]++;
        
        if (debugMode)
            Debug.Log($"[ChessBotScoring] Recorded move for {pieceName} (total moves: {pieceMoveCount[pieceName]})");
    }
    
    /// <summary>
    /// Record a move in algebraic notation for opening book
    /// </summary>
    public void RecordMoveNotation(int fromX, int fromY, int toX, int toY)
    {
        // Convert coordinates to algebraic notation (e.g., "e2e4")
        char fromFile = (char)('a' + fromX);
        char toFile = (char)('a' + toX);
        string moveNotation = $"{fromFile}{fromY + 1}{toFile}{toY + 1}";
        
        RecordMoveInHistory(moveNotation);
    }
    
    /// <summary>
    /// Reset piece movement tracking (call this at game start)
    /// </summary>
    public void ResetPieceTracking()
    {
        pieceMoveCount.Clear();
        gameMoves = 0;
        
        // Initialize opening book if not already done
        if (openingBook == null)
        {
            InitializeOpeningBook();
            gameHistory = new List<string>();
        }
        
        // Reset game history for new game
        ResetGameHistory();
        
        if (debugMode)
            Debug.Log("[ChessBotScoring] Piece movement tracking reset");
    }
    
    /// <summary>
    /// Calculate the total score for a bot move
    /// </summary>
    public int CalculateMoveScore(BotMove move)
    {
        int totalScore = 0;
        
        // PRIORITY 1: Capture moves get evaluated first (tactical opportunities)
        if (move.isAttack && move.target != null)
        {
            if (enableDepthSearch)
            {
                // Use 2-depth evaluation for captures
                totalScore = CalculateDepth2Score(move);
                
                if (debugMode)
                    Debug.Log($"[ChessBotScoring] CAPTURE - {move.pieceName} to ({move.toX},{move.toY}) - Depth-2 score: {totalScore}");
            }
            else
            {
                // Simple capture evaluation
                int captureScore = CalculateCaptureScore(move.target);
                totalScore += captureScore;
                
                if (debugMode)
                    Debug.Log($"[ChessBotScoring] CAPTURE - {move.pieceName} to ({move.toX},{move.toY}) - Capture score: {captureScore}");
            }
        }
        else
        {
            // PRIORITY 2: Non-capture moves (positional)
            if (enableTradingEvaluation)
            {
                // Use trading value system (considers both gains and risks)
                totalScore = CalculateTradingValue(move);
                
                if (debugMode)
                    Debug.Log($"[ChessBotScoring] POSITIONAL - {move.pieceName} to ({move.toX},{move.toY}) - Trading value: {totalScore}");
            }
            else
            {
                // Non-capture move - check center control
                int centerScore = CalculateCenterScore(move.fromX, move.fromY, move.toX, move.toY);
                totalScore += centerScore;
                
                if (debugMode)
                    Debug.Log($"[ChessBotScoring] POSITIONAL - {move.pieceName} to ({move.toX},{move.toY}) - Center score: {centerScore}");
            }
            
            // Add development bonus for non-capture moves only
            int currentTurn = GetCurrentTurn();
            int developmentBonus = CalculateDevelopmentBonus(move, currentTurn);
            totalScore += developmentBonus;
        }
        
        // Add defensive bonus for moves that save pieces or capture attackers
        int defensiveBonus = CalculateDefensiveBonus(move);
        totalScore += defensiveBonus;
        
        // Add king safety bonus for moves that improve king safety
        int kingSafetyBonus = CalculateKingSafetyBonus(move);
        totalScore += kingSafetyBonus;
        
        // Add mobility bonus for active piece positions
        int mobilityBonus = CalculateMobilityBonus(move);
        totalScore += mobilityBonus;
        
        // Add advanced opening principles bonus/penalty
        int advancedOpeningBonus = CalculateAdvancedOpeningBonus(move);
        totalScore += advancedOpeningBonus;
        
        // Add edge penalty for all moves
        int edgePenalty = CalculateEdgePenalty(move);
        totalScore += edgePenalty;
        
        if (debugMode)
        {
            string moveType = move.isAttack ? "CAPTURE" : "POSITIONAL";
            string defensiveText = defensiveBonus > 0 ? $" + {defensiveBonus} (defense)" : "";
            string kingText = kingSafetyBonus > 0 ? $" + {kingSafetyBonus} (king)" : kingSafetyBonus < 0 ? $" {kingSafetyBonus} (king)" : "";
            string mobilityText = mobilityBonus > 0 ? $" + {mobilityBonus} (mobility)" : "";
            string openingText = advancedOpeningBonus > 0 ? $" + {advancedOpeningBonus} (opening)" : advancedOpeningBonus < 0 ? $" {advancedOpeningBonus} (opening)" : "";
            string edgeText = edgePenalty < 0 ? $" {edgePenalty} (edge)" : "";
            Debug.Log($"[ChessBotScoring] {moveType} - Final score for {move.pieceName}: {totalScore}{defensiveText}{kingText}{mobilityText}{openingText}{edgeText}");
        }
        
        return totalScore;
    }
    
    /// <summary>
    /// Find the best move from a list of possible moves
    /// </summary>
    public BotMove FindBestMove(List<BotMove> possibleMoves)
    {
        if (possibleMoves == null || possibleMoves.Count == 0)
            return null;
        
        // Try opening book first
        if (TryGetOpeningMove(out BotMove openingMove))
        {
            if (debugMode)
                Debug.Log($"[ChessBotScoring] Using opening book move: {openingMove.pieceName} to ({openingMove.toX},{openingMove.toY})");
            return openingMove;
        }
            
        BotMove bestMove = possibleMoves[0];
        int bestScore = CalculateMoveScore(bestMove);
        
        if (debugMode)
            Debug.Log($"[ChessBotScoring] Evaluating {possibleMoves.Count} possible moves...");
        
        foreach (BotMove move in possibleMoves)
        {
            int moveScore = CalculateMoveScore(move);
            
            if (moveScore > bestScore)
            {
                bestScore = moveScore;
                bestMove = move;
            }
            
            if (debugMode)
                Debug.Log($"[ChessBotScoring] Move {move.pieceName} to ({move.toX},{move.toY}) - Score: {moveScore}");
        }
        
        if (debugMode)
            Debug.Log($"[ChessBotScoring] Best move: {bestMove.pieceName} to ({bestMove.toX},{bestMove.toY}) - Score: {bestScore}");
        
        return bestMove;
    }
    
    /// <summary>
    /// Get all capture moves from a list of possible moves
    /// </summary>
    public List<BotMove> GetCaptureMoves(List<BotMove> possibleMoves)
    {
        List<BotMove> captureMoves = new List<BotMove>();
        
        foreach (BotMove move in possibleMoves)
        {
            if (move.isAttack && move.target != null)
            {
                captureMoves.Add(move);
            }
        }
        
        return captureMoves;
    }
    
    /// <summary>
    /// Get all non-capture moves from a list of possible moves
    /// </summary>
    public List<BotMove> GetNonCaptureMoves(List<BotMove> possibleMoves)
    {
        List<BotMove> nonCaptureMoves = new List<BotMove>();
        
        foreach (BotMove move in possibleMoves)
        {
            if (!move.isAttack || move.target == null)
            {
                nonCaptureMoves.Add(move);
            }
        }
        
        return nonCaptureMoves;
    }
    
    /// <summary>
    /// Initialize the opening book with common chess openings
    /// </summary>
    private void InitializeOpeningBook()
    {
        openingBook = new Dictionary<string, string[]>();
        
        // Opening 1: King's Pawn Game (e4)
        openingBook["e2e4"] = new string[] { "e7e5", "c7c5", "e7e6", "d7d5" };
        openingBook["e2e4_e7e5"] = new string[] { "g1f3", "b1c3", "f2f4" };
        openingBook["e2e4_c7c5"] = new string[] { "g1f3", "b1c3", "c2c3" };
        openingBook["e2e4_e7e6"] = new string[] { "d2d4", "g1f3" };
        openingBook["e2e4_d7d5"] = new string[] { "e4d5", "e4e5" };
        
        // Opening 2: Queen's Pawn Game (d4)
        openingBook["d2d4"] = new string[] { "d7d5", "g8f6", "f7f5" };
        openingBook["d2d4_d7d5"] = new string[] { "c2c4", "g1f3", "e2e3" };
        openingBook["d2d4_g8f6"] = new string[] { "c2c4", "g1f3" };
        openingBook["d2d4_f7f5"] = new string[] { "g1f3", "c2c4" };
        
        // Opening 3: King's Knight Game (Nf3)
        openingBook["g1f3"] = new string[] { "d7d5", "g8f6", "c7c5" };
        openingBook["g1f3_d7d5"] = new string[] { "d2d4", "c2c4" };
        openingBook["g1f3_g8f6"] = new string[] { "d2d4", "c2c4" };
        openingBook["g1f3_c7c5"] = new string[] { "d2d4", "c2c4" };
        
        // Second move responses for black
        openingBook["e2e4_e7e5_g1f3"] = new string[] { "b8c6", "g8f6" };
        openingBook["e2e4_e7e5_b1c3"] = new string[] { "b8c6", "g8f6" };
        openingBook["e2e4_c7c5_g1f3"] = new string[] { "d7d6", "e7e6", "g8f6" };
        openingBook["d2d4_d7d5_c2c4"] = new string[] { "e7e6", "c7c6", "d5c4" };
        openingBook["d2d4_g8f6_c2c4"] = new string[] { "e7e6", "g7g6", "c7c5" };
        
        if (debugMode)
            Debug.Log("[ChessBotScoring] Opening book initialized with " + openingBook.Count + " positions");
    }
    
    /// <summary>
    /// Try to get a move from the opening book
    /// </summary>
    public bool TryGetOpeningMove(out BotMove bestMove)
    {
        bestMove = null;
        
        if (!enableOpeningBook || gameHistory.Count >= maxOpeningMoves)
        {
            return false;
        }
        
        // Build position key from game history
        string positionKey = string.Join("_", gameHistory);
        
        // Look up in opening book
        if (openingBook.ContainsKey(positionKey))
        {
            string[] possibleMoves = openingBook[positionKey];
            string selectedMove = possibleMoves[Random.Range(0, possibleMoves.Length)];
            
            // Convert notation to BotMove
            bestMove = ConvertNotationToBotMove(selectedMove);
            
            if (debugMode)
                Debug.Log($"[ChessBotScoring] Opening book move: {selectedMove} from position {positionKey}");
            
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Record a move in the game history for opening book lookup
    /// </summary>
    public void RecordMoveInHistory(string moveNotation)
    {
        if (gameHistory.Count < maxOpeningMoves)
        {
            gameHistory.Add(moveNotation);
            
            if (debugMode)
                Debug.Log($"[ChessBotScoring] Recorded move: {moveNotation}, History: {string.Join("_", gameHistory)}");
        }
    }
    
    /// <summary>
    /// Reset the game history for a new game
    /// </summary>
    public void ResetGameHistory()
    {
        gameHistory.Clear();
        
        if (debugMode)
            Debug.Log("[ChessBotScoring] Game history reset for new game");
    }
    
    /// <summary>
    /// Convert algebraic notation to BotMove
    /// </summary>
    private BotMove ConvertNotationToBotMove(string notation)
    {
        if (notation.Length < 4) return null;
        
        // Parse notation like "e2e4"
        int fromX = notation[0] - 'a'; // a=0, b=1, etc.
        int fromY = notation[1] - '1'; // 1=0, 2=1, etc.
        int toX = notation[2] - 'a';
        int toY = notation[3] - '1';
        
        // Get game reference
        Game gameRef = FindObjectOfType<Game>();
        if (gameRef == null) return null;
        
        // Find the piece at the from position
        GameObject piece = gameRef.GetPosition(fromX, fromY);
        if (piece == null) return null;
        
        // Check if it's an attack
        GameObject target = gameRef.GetPosition(toX, toY);
        bool isAttack = target != null;
        
        // Create BotMove using proper constructor
        return new BotMove(piece, fromX, fromY, toX, toY, isAttack, target);
    }
}
