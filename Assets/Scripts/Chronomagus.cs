using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chronomagus : MonoBehaviour
{
    private static int countdownStartTurn = -1; // When both pieces reached corners
    private static bool isCountdownActive = false;
    private static string countdownPlayer = ""; // Which player initiated the countdown
    
    private Game game;

    private void Awake()
    {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
    }

    // Check if Chronomagus promotion is available
    public static bool IsChronomagusAvailable(string player)
    {
        if (!isCountdownActive || countdownPlayer != player)
            return false;
            
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
            return false;
            
        int currentTurn = game.turns;
        int turnsElapsed = currentTurn - countdownStartTurn;
        
        return turnsElapsed >= 3;
    }

    // Check if both pieces are on corners and start countdown
    public static void CheckCornerPositioning()
    {
        // Find Archbishop and Elemental Bishop
        GameObject archbishop = FindPiece("arch_bishop");
        GameObject elementalBishop = FindPiece("elemental_bishop");
        
        if (archbishop == null || elementalBishop == null)
        {
            ResetCountdown();
            return;
        }
        
        // Check if they're on the same player's side
        Chessman archCm = archbishop.GetComponent<Chessman>();
        Chessman elemCm = elementalBishop.GetComponent<Chessman>();
        
        if (archCm == null || elemCm == null || archCm.GetPlayer() != elemCm.GetPlayer())
        {
            ResetCountdown();
            return;
        }
        
        string player = archCm.GetPlayer();
        
        // Check if both are on corners
        if (IsOnCorner(archbishop) && IsOnCorner(elementalBishop))
        {
            if (!isCountdownActive || countdownPlayer != player)
            {
                StartCountdown(player);
            }
        }
        else
        {
            ResetCountdown();
        }
    }

    private static GameObject FindPiece(string pieceName)
    {
        Chessman[] allPieces = FindObjectsOfType<Chessman>();
        foreach (Chessman piece in allPieces)
        {
            if (piece != null && piece.name.ToLower().Contains(pieceName))
            {
                return piece.gameObject;
            }
        }
        return null;
    }

    private static bool IsOnCorner(GameObject piece)
    {
        if (piece == null) return false;
        
        Chessman cm = piece.GetComponent<Chessman>();
        if (cm == null) return false;
        
        int x = cm.GetXBoard();
        int y = cm.GetYBoard();
        
        // Check if on any of the 4 corners: (0,0), (0,7), (7,0), (7,7)
        return (x == 0 && y == 0) || (x == 0 && y == 7) || (x == 7 && y == 0) || (x == 7 && y == 7);
    }

    private static void StartCountdown(string player)
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[Chronomagus] Game component not found!");
            return;
        }
        
        isCountdownActive = true;
        countdownPlayer = player;
        countdownStartTurn = game.turns;
        
        Debug.Log($"[Chronomagus] Countdown started for {player} player at turn {countdownStartTurn}. Both pieces on corners!");
    }

    private static void ResetCountdown()
    {
        if (isCountdownActive)
        {
            Debug.Log($"[Chronomagus] Countdown reset - pieces no longer on corners or different players");
        }
        
        isCountdownActive = false;
        countdownPlayer = "";
        countdownStartTurn = -1;
    }

    // Main Chronomagus promotion method
    public void ChronomagusPromotion()
    {
        if (!IsChronomagusAvailable(countdownPlayer))
        {
            Debug.LogWarning("[Chronomagus] Promotion not available - requirements not met!");
            return;
        }

        Debug.Log("[Chronomagus] Starting Chronomagus promotion!");
        
        // Find and destroy both pieces
        GameObject archbishop = FindPiece("arch_bishop");
        GameObject elementalBishop = FindPiece("elemental_bishop");
        
        if (archbishop != null)
        {
            Chessman archCm = archbishop.GetComponent<Chessman>();
            if (archCm != null)
            {
                game.SetPositionEmpty(archCm.GetXBoard(), archCm.GetYBoard());
                Destroy(archbishop);
                Debug.Log("[Chronomagus] Archbishop destroyed");
            }
        }
        
        if (elementalBishop != null)
        {
            Chessman elemCm = elementalBishop.GetComponent<Chessman>();
            if (elemCm != null)
            {
                game.SetPositionEmpty(elemCm.GetXBoard(), elemCm.GetYBoard());
                Destroy(elementalBishop);
                Debug.Log("[Chronomagus] Elemental Bishop destroyed");
            }
        }
        
        // Create summon tiles for Chronomagus placement
        CreateChronomagusSummonTiles();
        
        // Reset countdown
        ResetCountdown();
    }

    private void CreateChronomagusSummonTiles()
    {
        // Clear existing move plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // Create summon tiles on all empty squares
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (game.GetPosition(x, y) == null)
                {
                    CreateChronomagusSummonTile(x, y);
                }
            }
        }
        
        Debug.Log("[Chronomagus] Chronomagus summon tiles created on all empty squares");
    }

    private void CreateChronomagusSummonTile(int x, int y)
    {
        float fx = x * 0.57f - 1.98f;
        float fy = y * 0.56f - 1.95f;

        GameObject mp = Instantiate(game.movePlatePrefabReference, new Vector3(fx, fy, -3f), Quaternion.identity);

        // Remove default MovePlate script
        MovePlate oldScript = mp.GetComponent<MovePlate>();
        if (oldScript != null) Destroy(oldScript);

        // Add ChronomagusSummonPlate script
        mp.AddComponent<ChronomagusSummonPlate>().Setup(game, x, y, countdownPlayer);
        
        Debug.Log($"[Chronomagus] Created summon tile at ({x},{y})");
    }
}
