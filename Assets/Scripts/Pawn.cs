using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : MonoBehaviour
{
    private Chessman chessman;
    private Game game;

    private void Awake()
    {
        // Cache Chessman reference
        chessman = GetComponent<Chessman>(); 
        if (chessman == null)
            Debug.LogError("[Pawn] Missing Chessman component!");
            
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
    }

    // RoyalAcolyte Promotion - Check if pawn is on last rank
    public void RoyalAcolyte()
    {
        if (chessman == null)
        {
            Debug.LogError("[RoyalAcolyte] Missing Chessman reference!");
            return;
        }

        if (game == null)
        {
            Debug.LogError("[RoyalAcolyte] Missing Game reference!");
            return;
        }

        string player = chessman.GetPlayer();
        int currentY = chessman.GetYBoard();
        
        // Check if pawn is on last rank
        bool isOnLastRank = false;
        
        if (player == "white" && currentY == 7)
        {
            isOnLastRank = true;
        }
        else if (player == "black" && currentY == 0)
        {
            isOnLastRank = true;
        }

        // Check requirements and SP cost
        if (isOnLastRank)
        {
            // Check if player already has a royal pawn
            if (PlayerHasRoyalPawn(player))
            {
                Debug.Log($"[RoyalAcolyte] Cannot promote - {player} player already has 1 royal pawn. Only 1 royal pawn allowed per player.");
                return;
            }
            
            // Check SP cost (2 SP)
            if (!SkillManager.Instance.SpendPlayerSP(player, 2))
            { 
                Debug.LogWarning($"[RoyalAcolyte] Not enough SP to promote {player} pawn. Need 2 SP.");
                return;
            }
            
            Debug.Log($"[RoyalAcolyte] Can be pressed - {player} pawn is on last rank at y={currentY}");
            
            // Perform the promotion
            PromoteToRoyalPawn();
        }
        else
        {
            Debug.Log($"[RoyalAcolyte] Cannot press - {player} pawn needs to reach last rank (currently at y={currentY})");
        }
    }

    private bool PlayerHasRoyalPawn(string player)
    {
        // Find all royal pawns on the board
        Chessman[] allPieces = FindObjectsOfType<Chessman>();
        
        foreach (Chessman piece in allPieces)
        {
            if (piece != null && piece.GetPlayer() == player)
            {
                string pieceName = piece.name.ToLower();
                if (pieceName.Contains("royal_pawn"))
                {
                    Debug.Log($"[RoyalAcolyte] Found existing {player} royal pawn: {piece.name}");
                    return true;
                }
            }
        }
        
        return false;
    }

    private void PromoteToRoyalPawn()
    {
        if (chessman == null || game == null)
        {
            Debug.LogError("[RoyalAcolyte] Missing references for promotion!");
            return;
        }

        string player = chessman.GetPlayer();
        int x = chessman.GetXBoard();
        int y = chessman.GetYBoard();
        
        // Determine the royal pawn name based on player
        string royalPawnName = (player == "white") ? "white_royal_pawn" : "black_royal_pawn";
        
        Debug.Log($"[RoyalAcolyte] Promoting {player} pawn at ({x},{y}) to {royalPawnName}");
        
        // Clear the current pawn's position
        game.SetPositionEmpty(x, y);
        
        // Destroy the current pawn
        Destroy(gameObject);
        
        // Create the royal pawn at the same position
        GameObject royalPawn = game.Create(royalPawnName, x, y);
        if (royalPawn != null)
        {
            Debug.Log($"[RoyalAcolyte] Successfully promoted to {royalPawnName} at ({x},{y})");
            
            // Trigger Echo() passive after successful promotion
            RoyalAcolyte royalAcolyte = royalPawn.GetComponent<RoyalAcolyte>();
            if (royalAcolyte != null)
            {
                royalAcolyte.Echo();
            }
            else
            {
                Debug.LogError($"[RoyalAcolyte] RoyalAcolyte component not found on {royalPawnName}!");
            }
        }
        else
        {
            Debug.LogError($"[RoyalAcolyte] Failed to create {royalPawnName} at ({x},{y})");
        }
    }
}
