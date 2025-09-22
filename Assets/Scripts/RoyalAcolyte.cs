using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoyalAcolyte : MonoBehaviour
{
    private Game game;

    private void Awake()
    {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
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
}
