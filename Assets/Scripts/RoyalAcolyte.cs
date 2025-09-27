using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoyalAcolyte : MonoBehaviour
{
    private Game game;
    public static bool abyssalRiteUsed = false;
    private void Awake()
    {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
    }

    // Check if any royal acolyte of the specified player is on the board
    public static bool IsRoyalAcolyteOnBoard(string player)
    {
        Chessman[] allPieces = FindObjectsOfType<Chessman>();
        foreach (Chessman piece in allPieces)
        {
            if (piece != null && piece.name.Contains("royal_pawn") && piece.GetPlayer() == player)
            {
                return true;
            }
        }
        return false;
    }

    // Echo() - Summons Spectral Herald after royal pawn promotion
    public void Echo()
    {
        if (game == null)
        {
            Debug.LogError("[Echo] Missing Game reference!");
            return;
        }

        Debug.Log("[Echo] Spectral Herald summoning tiles activated!");
        
        // Create summon tiles on friendly side (pawn starting positions)
        CreateSpectralHeraldTiles();
    }

    private void CreateSpectralHeraldTiles()
    {
        // Clear existing move plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // Get the current player to determine friendly side
        string currentPlayer = game.GetCurrentPlayer();
        
        // Create summon tiles only on friendly side
        for (int x = 0; x < 8; x++)
        {
            if (currentPlayer == "white")
            {
                // White side: y = 1 (pawn starting row)
                if (game.GetPosition(x, 1) == null)
                {
                    CreateSpectralHeraldTile(x, 1, "white_spectral_herald");
                }
            }
            else if (currentPlayer == "black")
            {
                // Black side: y = 6 (pawn starting row)
                if (game.GetPosition(x, 6) == null)
                {
                    CreateSpectralHeraldTile(x, 6, "black_spectral_herald");
                }
            }
        }
        
        Debug.Log($"[Echo] Created Spectral Herald tiles for {currentPlayer} player only");
    }

    private void CreateSpectralHeraldTile(int x, int y, string pieceName)
    {
        float fx = x * 0.57f - 1.98f;
        float fy = y * 0.56f - 1.95f;

        GameObject mp = Instantiate(game.movePlatePrefabReference, new Vector3(fx, fy, -3f), Quaternion.identity);

        // Remove default MovePlate script
        MovePlate oldScript = mp.GetComponent<MovePlate>();
        if (oldScript != null) Destroy(oldScript);

        // Add SpectralHeraldPlate script
        mp.AddComponent<SpectralHeraldPlate>().Setup(game, x, y, pieceName);
        
        Debug.Log($"[Echo] Created Spectral Herald summon tile at ({x},{y}) for {pieceName}");
    }

    public void AbyssalRite()
    {
        if (game == null)
        {
            Debug.LogError("[AbyssalRite] Missing Game reference!");
            return;
        }

        // Check if already used this battle
        if (abyssalRiteUsed)
        {
            Debug.Log("[AbyssalRite] Skill already used this battle!");
            return;
        }

        // Find white_spectral_herald on the board
        Chessman[] allPieces = FindObjectsOfType<Chessman>();
        GameObject spectralHerald = null;
        
        foreach (Chessman piece in allPieces)
        {
            if (piece != null && piece.name == "white_spectral_herald")
            {
                spectralHerald = piece.gameObject;
                break;
            }
        }

        if (spectralHerald == null)
        {
            Debug.Log("[AbyssalRite] No white_spectral_herald found on board!");
            return;
        }

        // Get the spectral herald's location
        Chessman heraldChessman = spectralHerald.GetComponent<Chessman>();
        int x = heraldChessman.GetXBoard();
        int y = heraldChessman.GetYBoard();

        Debug.Log($"[AbyssalRite] Found white_spectral_herald at ({x},{y}) - destroying and creating tile_void");

        // Destroy the spectral herald
        game.SetPositionEmpty(x, y);
        Destroy(spectralHerald);

        // Create tile_void at the same location
        GameObject voidTile = game.Create("tile_void", x, y);

        // Mark skill as used
        abyssalRiteUsed = true;

        // Hide UI panels
        if (UIManager.Instance != null)
        {
            UIManager.Instance.pawnPanel?.SetActive(false);
            UIManager.Instance.knightPanel?.SetActive(false);
            UIManager.Instance.bishopPanel?.SetActive(false);
            UIManager.Instance.rookPanel?.SetActive(false);
            UIManager.Instance.queenPanel?.SetActive(false);
            UIManager.Instance.kingPanel?.SetActive(false);
            UIManager.Instance.whiteElementalBishopPanel?.SetActive(false);
            UIManager.Instance.whiteArchBishopPanel?.SetActive(false);
            UIManager.Instance.whiteRoyalRookPanel?.SetActive(false);
            UIManager.Instance.whiteRoyalBishopPanel?.SetActive(false);
            UIManager.Instance.whiteWraithPawnPanel?.SetActive(false);
            UIManager.Instance.whiteRoyalPawnPanel?.SetActive(false);
            UIManager.Instance.whiteSpectralHeraldPanel?.SetActive(false);
            UIManager.Instance.whiteChronomagusPanel?.SetActive(false);
            UIManager.Instance.whiteRoyalKnightPanel?.SetActive(false);
        }

        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
        // End turn
        game.NextTurn();

        Debug.Log("[AbyssalRite] Skill completed - spectral herald destroyed, tile_void created, turn ended");
    }

    // Reset the once-per-battle flag (call this when starting a new battle)
    public static void ResetAbyssalRiteUsage()
    {
        abyssalRiteUsed = false;
        Debug.Log("[AbyssalRite] Usage reset for new battle");
    }

   

   







}
