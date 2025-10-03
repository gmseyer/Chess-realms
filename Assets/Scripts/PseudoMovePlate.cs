using UnityEngine;
using System.Collections;

/// <summary>
/// PseudoMovePlate handles bot-specific move logic without cluttering the original MovePlate.cs
/// This ensures clean separation between player and bot mechanics
/// </summary>
public static class PseudoMovePlate
{
    /// <summary>
    /// Handle bot-specific move logic after a capture
    /// This is called from the original MovePlate.cs when the bot makes a move
    /// </summary>
    /// <param name="attackerPiece">The piece that made the capture</param>
    /// <param name="capturedPiece">The piece that was captured</param>
    /// <param name="fromX">From X position</param>
    /// <param name="fromY">From Y position</param>
    /// <param name="toX">To X position</param>
    /// <param name="toY">To Y position</param>
    public static void HandleBotMove(GameObject attackerPiece, GameObject capturedPiece, int fromX, int fromY, int toX, int toY)
    {
        if (attackerPiece == null) return;
        
        Chessman attackerChessman = attackerPiece.GetComponent<Chessman>();
        if (attackerChessman == null) return;
        
        string attackerPlayer = attackerChessman.GetPlayer();
        
        // Only handle bot moves (black pieces)
        if (attackerPlayer != "black") return;
        
        Debug.Log($"[PseudoMovePlate] Handling bot move for {attackerPiece.name}");
        
        // Handle different piece types and their skills
        if (attackerPiece.name.Contains("knight"))
        {
            // Handle Knight Momentum (already implemented in ChessBot)
            // This will be called by the existing system
        }
        else if (attackerPiece.name.Contains("bishop"))
        {
            // Handle Bishop Divine Offering
            BotSkillHandler.HandleDivineOffering(attackerPiece);
        }
        // Add more piece types as needed
        
        // Future: Add other piece skills here
        // else if (attackerChessman.GetPieceName().Contains("rook"))
        // else if (attackerChessman.GetPieceName().Contains("queen"))
        // etc.
    }
    
    /// <summary>
    /// Handle bot-specific capture logic
    /// Called when a piece is captured to trigger any death-related skills
    /// </summary>
    /// <param name="capturedPiece">The piece that was captured</param>
    /// <param name="attackerPiece">The piece that captured it</param>
    public static void HandleBotCapture(GameObject capturedPiece, GameObject attackerPiece)
    {
        if (capturedPiece == null) 
        {
            Debug.LogWarning("[PseudoMovePlate] HandleBotCapture: capturedPiece is null!");
            return;
        }
        
        Chessman capturedChessman = capturedPiece.GetComponent<Chessman>();
        if (capturedChessman == null) 
        {
            Debug.LogWarning("[PseudoMovePlate] HandleBotCapture: No Chessman component on captured piece!");
            return;
        }
        
        string capturedPlayer = capturedChessman.GetPlayer();
        
        // Only handle bot pieces being captured (black pieces)
        if (capturedPlayer != "black") 
        {
            Debug.Log($"[PseudoMovePlate] HandleBotCapture: Captured piece is not bot's piece ({capturedPlayer})");
            return;
        }
        
        Debug.Log($"[PseudoMovePlate] ✅ Handling bot capture of {capturedPiece.name} by {attackerPiece?.name}");
        
        // Handle death-related skills
        if (capturedPiece.name.Contains("bishop"))
        {
            Debug.Log("[PseudoMovePlate] ✅ Bishop captured - triggering Divine Offering!");
            // Handle Bishop Divine Offering on death
            BotSkillHandler.HandleDivineOffering(capturedPiece);
        }
        else
        {
            Debug.Log($"[PseudoMovePlate] No special death skills for {capturedPiece.name}");
        }
        // Add more death skills as needed
    }
}
