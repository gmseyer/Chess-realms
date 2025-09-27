using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoyalKnight : MonoBehaviour
{
    private Game game;
    
    // Phantom Swap cooldown tracking (6 turns)
    private int phantomSwapCooldown = 0;
    private const int phantomSwapCooldownMax = 6;

    private void Awake()
    {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
    }

    // Phantom Guard passive - leaves a Mist Knight illusion on previous square when moving
    public void PhantomGuard(int previousX, int previousY)
    {
        if (game == null)
        {
            Debug.LogError("[PhantomGuard] Missing Game reference!");
            return;
        }

        Debug.Log($"[PhantomGuard] Royal Knight moved from ({previousX},{previousY}) - activating Phantom Guard");

        // Check if there are existing white_mist_knight pieces and destroy them
        DestroyExistingMistKnights();

        // Create new mist knight at the Royal Knight's previous position
        CreateMistKnight(previousX, previousY);

        Debug.Log($"[PhantomGuard] Mist Knight illusion created at ({previousX},{previousY})");
    }

    private void DestroyExistingMistKnights()
    {
        // Find all existing mist knights on the board
        Chessman[] allPieces = FindObjectsOfType<Chessman>();
        
        foreach (Chessman piece in allPieces)
        {
            if (piece != null && piece.name == "white_mist_knight")
            {
                Debug.Log($"[PhantomGuard] Destroying existing mist knight at ({piece.GetXBoard()},{piece.GetYBoard()})");
                
                // Clear the position from the game board
                game.SetPositionEmpty(piece.GetXBoard(), piece.GetYBoard());
                
                // Destroy the GameObject
                Destroy(piece.gameObject);
            }
        }
    }

    private void CreateMistKnight(int x, int y)
    {
        // Check if the position is empty (it should be since the Royal Knight just moved from there)
        if (game.GetPosition(x, y) != null)
        {
            Debug.LogWarning($"[PhantomGuard] Position ({x},{y}) is not empty! Cannot create mist knight.");
            return;
        }

        // Create the mist knight using Game.Create
        GameObject mistKnight = game.Create("white_mist_knight", x, y);
        
        Debug.Log($"[PhantomGuard] Mist Knight created at ({x},{y})");
    }

    // Try to trigger Phantom Swap when Royal Knight is threatened with capture
    // Returns true if swap was successful, false if no mist knight available or on cooldown
    public bool TryTriggerPhantomSwap()
    {
        if (phantomSwapCooldown > 0)
        {
            Debug.Log($"[PhantomSwap] Phantom Swap is on cooldown for {phantomSwapCooldown} more turn(s).");
            return false;
        }

        if (game == null)
        {
            Debug.LogError("[PhantomSwap] Missing Game reference!");
            return false;
        }

        // Find existing mist knight on the board
        GameObject mistKnight = FindExistingMistKnight();
        if (mistKnight == null)
        {
            Debug.Log("[PhantomSwap] No mist knight found on board - cannot trigger Phantom Swap.");
            return false;
        }

        Chessman mistKnightChessman = mistKnight.GetComponent<Chessman>();
        if (mistKnightChessman == null)
        {
            Debug.LogError("[PhantomSwap] Mist knight has no Chessman component!");
            return false;
        }

        // Get mist knight's position
        int mistKnightX = mistKnightChessman.GetXBoard();
        int mistKnightY = mistKnightChessman.GetYBoard();

        // Get Royal Knight's current position
        Chessman royalKnightChessman = GetComponent<Chessman>();
        if (royalKnightChessman == null)
        {
            Debug.LogError("[PhantomSwap] Royal Knight has no Chessman component!");
            return false;
        }

        int royalKnightX = royalKnightChessman.GetXBoard();
        int royalKnightY = royalKnightChessman.GetYBoard();

        Debug.Log($"[PhantomSwap] Royal Knight at ({royalKnightX},{royalKnightY}) swapping with Mist Knight at ({mistKnightX},{mistKnightY})");

        // Perform the swap
        SwapPositions(royalKnightChessman, mistKnightChessman, mistKnightX, mistKnightY);

        // Destroy the mist knight (like Queen's pawn sacrifice)
        game.SetPositionEmpty(mistKnightX, mistKnightY);
        Destroy(mistKnight);

        // Set cooldown
        phantomSwapCooldown = phantomSwapCooldownMax;

        Debug.Log($"[PhantomSwap] Phantom Swap successful! Royal Knight moved to ({mistKnightX},{mistKnightY}), Mist Knight destroyed. Cooldown: {phantomSwapCooldownMax} turns.");
        return true;
    }

    private GameObject FindExistingMistKnight()
    {
        // Find existing mist knight on the board
        Chessman[] allPieces = FindObjectsOfType<Chessman>();
        
        foreach (Chessman piece in allPieces)
        {
            if (piece != null && piece.name == "white_mist_knight")
            {
                return piece.gameObject;
            }
        }
        
        return null;
    }

    private void SwapPositions(Chessman royalKnight, Chessman mistKnight, int mistKnightX, int mistKnightY)
    {
        // Clear Royal Knight's current position
        game.SetPositionEmpty(royalKnight.GetXBoard(), royalKnight.GetYBoard());

        // Move Royal Knight to mist knight's position
        royalKnight.SetXBoard(mistKnightX);
        royalKnight.SetYBoard(mistKnightY);
        royalKnight.SetCoords();

        // Set Royal Knight at the new position (mist knight will be destroyed after this)
        game.SetPosition(royalKnight.gameObject);
    }

    // Check if Phantom Swap is available (not on cooldown)
    public bool IsPhantomSwapAvailable()
    {
        return phantomSwapCooldown <= 0;
    }

    // Reduce cooldown by 1 turn (call this at the start of each turn)
    public void ReducePhantomSwapCooldown()
    {
        if (phantomSwapCooldown > 0)
        {
            phantomSwapCooldown--;
            Debug.Log($"[PhantomSwap] Cooldown reduced to {phantomSwapCooldown} turns remaining.");
        }
    }

    // Clean up mist knight when Royal Knight is captured/destroyed
    public void OnRoyalKnightDestroyed()
    {
        if (game == null)
        {
            Debug.LogError("[PhantomGuard] Missing Game reference during cleanup!");
            return;
        }

        // Find and destroy any existing mist knight
        GameObject mistKnight = FindExistingMistKnight();
        if (mistKnight != null)
        {
            Chessman mistKnightChessman = mistKnight.GetComponent<Chessman>();
            if (mistKnightChessman != null)
            {
                int mistX = mistKnightChessman.GetXBoard();
                int mistY = mistKnightChessman.GetYBoard();
                
                Debug.Log($"[PhantomGuard] Royal Knight destroyed - cleaning up Mist Knight at ({mistX},{mistY})");
                
                // Clear position and destroy mist knight
                game.SetPositionEmpty(mistX, mistY);
                Destroy(mistKnight);
            }
        }
        else
        {
            Debug.Log("[PhantomGuard] Royal Knight destroyed - no Mist Knight found to clean up.");
        }
    }
}
