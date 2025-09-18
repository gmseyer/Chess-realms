using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Chessman
{
    // Check if pawn can be promoted after moving
    public void CheckForPromotion()
    {
        bool hasReachedLastRank = false;
        
        if (player == "white" && GetYBoard() == 7)
        {
            hasReachedLastRank = true;
        }
        else if (player == "black" && GetYBoard() == 0)
        {
            hasReachedLastRank = true;
        }
        
        if (hasReachedLastRank)
        {
            Debug.Log($"[Pawn Promotion] {name} has reached the last rank! Royal Acolyte promotion is now available.");
        }
    }

    // Royal Acolyte Promotion Skill
    public void AttemptRoyalAcolytePromotion()
    {
        // Check if pawn has reached the last rank
        bool hasReachedLastRank = false;
        
        if (player == "white" && GetYBoard() == 7)
        {
            hasReachedLastRank = true;
        }
        else if (player == "black" && GetYBoard() == 0)
        {
            hasReachedLastRank = true;
        }
        
        if (hasReachedLastRank)
        {
            Debug.Log("Reached the last rank and Button Activated");
            // TODO: In future implementation, this is where we would:
            // 1. Check if player has 3 SP
            // 2. Transform pawn into Royal Acolyte
            // 3. Consume the turn
        }
        else
        {
            Debug.Log("Need to reach the last rank to promote");
        }
    }
}
