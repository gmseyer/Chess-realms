using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlate : MonoBehaviour
{ 
    //Some functions will need reference to the controller
    public GameObject controller;
    //The Chesspiece that was tapped to create this MovePlate
    GameObject reference = null;

    //Location on the board
    int matrixX;
    int matrixY;

    //false: movement, true: attacking
    public bool attack = false;

    public void Start()
    {
        if (attack)
        {
            //Set to red
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        }
    }

    public void OnMouseUp()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");
        Chessman movingPiece = reference.GetComponent<Chessman>();
        Knight knightComponent = movingPiece.GetComponent<Knight>();

        // ----------------- Handle Attacks -----------------
        if (attack)
        {
            GameObject cp = controller.GetComponent<Game>().GetPosition(matrixX, matrixY);
            if (cp != null)
            {
                Chessman targetCm = cp.GetComponent<Chessman>();

                // Special check for bishop capture
                if (cp.name == "white_bishop")
                {
                    Bishop bishop = cp.GetComponent<Bishop>();

                    if (bishop != null && !targetCm.isInvulnerable)
                    {
                        controller.GetComponent<Game>().SetPositionEmpty(matrixX, matrixY);
                        Destroy(cp);

                        bishop.OnBishopButtonClick();
                        return; // Stop processing further
                    }
                }

                if (targetCm != null && targetCm.isInvulnerable)
                {
                    Debug.Log($"{targetCm.name} is invulnerable — attack cancelled.");
                    return;
                }
                // ----------------- QUEEN PASSIVE SECTION -----------------
                // <-- NEW: debug log when queen is about to be taken -->
                if (cp.name.ToLower().Contains("queen"))
                {
                    Debug.Log($"[MovePlate] Queen is about to be taken: {cp.name} at ({matrixX},{matrixY}) by {movingPiece.name}");

                    Queen queen = cp.GetComponent<Queen>();
                    if (queen != null)
                    {
                        bool passiveActivated = queen.TryTriggerGloryForTheQueen();

                        if (passiveActivated)
                        {
                            Debug.Log("[MovePlate] Queen survives thanks to Glory for the Queen!");
                            // Cancel capture flow: queen not destroyed
                            movingPiece.DestroyMovePlates();
                            movingPiece.ClearFortify();
                            movingPiece.CheckMoveTiles_End();
                            controller.GetComponent<Game>().NextTurn();
                            return; // stop further processing
                        }
                    }
                }  // --QUEEN PASSIVE END -------------------------------------

                if (cp.name == "white_king") controller.GetComponent<Game>().Winner("black");
                if (cp.name == "black_king") controller.GetComponent<Game>().Winner("white");

                Destroy(cp);

                // ---------- QUEEN DESTROYED LOG ----------
                if (cp.name.ToLower().Contains("queen"))
                {
                    Debug.Log($"[MovePlate] Queen destroyed: {cp.name} at ({matrixX},{matrixY})");
                }


                Knight attackerKnight = reference.GetComponent<Knight>();
                if (attackerKnight != null && attackerKnight.IsMomentumReady())
                {
                    // prevent the usual NextTurn flow: spawn momentum teleport tiles and let player choose
                    Knight.ActiveKnight = attackerKnight; // keep it selected (useful)
                    attackerKnight.TriggerKnightsMomentum();
                    return; // IMPORTANT: stop further processing so the player can click momentum tile
                }


            }
        }

        // ----------------- Tile Effects Check (BEFORE moving) -----------------
        bool iceEffectTriggered = CheckTileIceEffect(movingPiece, matrixX, matrixY);
        bool lavaEffectTriggered = CheckTileLavaEffect(movingPiece, matrixX, matrixY);

        // ----------------- Move Chessman (skip if tile effects triggered) -----------------
        if (!iceEffectTriggered && !lavaEffectTriggered)
        {
        controller.GetComponent<Game>().SetPositionEmpty(
            movingPiece.GetXBoard(),
            movingPiece.GetYBoard()
        );

        movingPiece.SetXBoard(matrixX);
        movingPiece.SetYBoard(matrixY);
        movingPiece.SetCoords();

        controller.GetComponent<Game>().SetPosition(reference);

        movingPiece.DestroyMovePlates();
        movingPiece.ClearFortify();
        movingPiece.CheckMoveTiles_End();
        }

        // ----------------- Lunar Leap Check -----------------
        if (knightComponent != null && knightComponent.CanDoubleMove)
        {
            // If Lunar Leap was active, disable it after this move
            knightComponent.CanDoubleMove = false;

            Debug.Log("[LunarLeap] Knight finished Lunar Leap — turn ends.");
            controller.GetComponent<Game>().NextTurn();
        }
        else
        {
            // Normal turn ending
            controller.GetComponent<Game>().NextTurn();
        }

        // ----------------- Hide UI Panels -----------------
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

        }
        if (SkillManagerTMP.Instance != null)
        {
            SkillManagerTMP.Instance.skillPanel?.SetActive(false);
        }
}






    public void SetCoords(int x, int y)
    {
        matrixX = x;
        matrixY = y;
    }

    public int GetMatrixX()
    {
        return matrixX;
    }

    public int GetMatrixY()
    {
        return matrixY;
    }

    public void SetReference(GameObject obj)
    {
        reference = obj;
    }

    public GameObject GetReference()
    {
        return reference;
    }

    private bool CheckTileIceEffect(Chessman movingPiece, int x, int y)
    {
        Game game = controller.GetComponent<Game>();
        
        // Check if there's a tile_ice at the destination
        GameObject tileAtPosition = game.GetPosition(x, y);
        if (tileAtPosition != null && tileAtPosition.name == "tile_ice")
        {
            // Check if it's an Elemental Bishop (immune to ice)
            if (movingPiece.name == "white_elemental_bishop" || movingPiece.name == "white_king" || movingPiece.name == "black_king")
            {
                Debug.Log($"[Tile_Ice] {movingPiece.name} is immune to ice - tile disappears!");
                
                // Just destroy the ice tile (no random movement)
                DestroyIceTile(game, x, y);
                
                // Clean up and end turn
                movingPiece.DestroyMovePlates();
                movingPiece.ClearFortify();
                movingPiece.CheckMoveTiles_End();
                
                return true; // Ice effect triggered (but piece stays in place)
            }
            
            Debug.Log($"[Tile_Ice] {movingPiece.name} landed on ice tile at ({x},{y}) - triggering random movement!");
            
            // Find empty tiles around the current position
            List<Vector2Int> emptyTiles = FindEmptyTilesAround(game, x, y);
            
            if (emptyTiles.Count > 0)
            {
                // Randomly pick one of the empty tiles
                Vector2Int randomTile = emptyTiles[Random.Range(0, emptyTiles.Count)];
                
                // Move the piece to the random position
                MovePieceRandomly(movingPiece, randomTile.x, randomTile.y, game);
                
                Debug.Log($"[Tile_Ice] {movingPiece.name} randomly moved to ({randomTile.x},{randomTile.y})");
                
                // Destroy the ice tile (one-use effect)
                DestroyIceTile(game, x, y);
                
                // Clean up and end turn after ice effect
                movingPiece.DestroyMovePlates();
                movingPiece.ClearFortify();
                movingPiece.CheckMoveTiles_End();
                //game.NextTurn(); 
                
                return true; // Ice effect triggered
            }
            else
            {
                Debug.Log("[Tile_Ice] No empty tiles around - piece stays on ice tile");
                
                // Destroy the ice tile (one-use effect)
                DestroyIceTile(game, x, y);
                
                // Clean up and end turn even if no movement
                movingPiece.DestroyMovePlates();
                movingPiece.ClearFortify();
                movingPiece.CheckMoveTiles_End();
                //game.NextTurn();
                
                return true; // Ice effect triggered (even if no movement)
            }
        }
        
        return false; // No ice effect
    }
    
    private bool CheckTileLavaEffect(Chessman movingPiece, int x, int y)
{
    Game game = controller.GetComponent<Game>();
    
    // Check if there's a tile_lava at the destination
    GameObject tileAtPosition = game.GetPosition(x, y);
    if (tileAtPosition != null && tileAtPosition.name == "tile_lava")
    {
        // Check if it's an Elemental Bishop (immune to lava)
        if (movingPiece.name == "white_elemental_bishop" || movingPiece.name == "white_king" || movingPiece.name == "black_king")
        {
            Debug.Log($"[Tile_Lava] {movingPiece.name} is immune to lava - tile disappears!");
            
            // Just destroy the lava tile (no piece destruction)
            DestroyLavaTile(game, x, y);
            
            // Clean up and end turn
            movingPiece.DestroyMovePlates();
            movingPiece.ClearFortify();
            movingPiece.CheckMoveTiles_End();
            
            return true; // Lava effect triggered (but piece survives)
        }
        else
        {
            // Normal lava effect - destroy piece
            Debug.Log($"[Tile_Lava] {movingPiece.name} stepped on lava tile at ({x},{y}) - INSTANT DESTRUCTION!");
            
            DestroyPiece(movingPiece, game);
            DestroyLavaTile(game, x, y);
            
            movingPiece.DestroyMovePlates();
            movingPiece.ClearFortify();
            movingPiece.CheckMoveTiles_End();
            
            return true; // Lava effect triggered
        }
    }
    
    return false; // No lava effect
}
    
    private List<Vector2Int> FindEmptyTilesAround(Game game, int centerX, int centerY)
    {
        List<Vector2Int> emptyTiles = new List<Vector2Int>();
        
        // Check all 8 directions around the current position
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue; // Skip the center tile
                
                int checkX = centerX + dx;
                int checkY = centerY + dy;
                
                // Check if position is on board
                if (!game.PositionOnBoard(checkX, checkY)) continue;
                
                // Check if position is empty
                if (game.GetPosition(checkX, checkY) == null)
                {
                    emptyTiles.Add(new Vector2Int(checkX, checkY));
                }
            }
        }
        
        Debug.Log($"[Tile_Ice] Found {emptyTiles.Count} empty tiles around ({centerX},{centerY})");
        return emptyTiles;
    }
    
    private void MovePieceRandomly(Chessman piece, int newX, int newY, Game game)
    {
        // Clear the old position
        game.ClearPosition(piece.GetXBoard(), piece.GetYBoard());
        
        // Update the piece's coordinates
        piece.SetXBoard(newX);
        piece.SetYBoard(newY);
        piece.SetCoords(); // Update visual position
        
        // Set the piece at the new position
        game.SetPositionAt(piece.gameObject, newX, newY);
        
        Debug.Log($"[Tile_Ice] Moved {piece.name} to ({newX},{newY})");
    }
    
    private void DestroyIceTile(Game game, int x, int y)
    {
        // Get the ice tile at the position
        GameObject iceTile = game.GetPosition(x, y);
        if (iceTile != null && iceTile.name == "tile_ice")
        {
            // Clear the position in the game board
            game.ClearPosition(x, y);
            
            // Destroy the GameObject
            Destroy(iceTile);
            
            Debug.Log($"[Tile_Ice] Ice tile destroyed at ({x},{y}) - one-use effect consumed!");
        }
    }
    
    private void DestroyLavaTile(Game game, int x, int y)
    {
        // Get the lava tile at the position
        GameObject lavaTile = game.GetPosition(x, y);
        if (lavaTile != null && lavaTile.name == "tile_lava")
        {
            // Clear the position in the game board
            game.ClearPosition(x, y);
            
            // Destroy the GameObject
            Destroy(lavaTile);
            
            Debug.Log($"[Tile_Lava] Lava tile destroyed at ({x},{y}) - one-use effect consumed!");
        }
    }
    
    private void DestroyPiece(Chessman piece, Game game)
    {
        // Clear the piece's position from the game board
        game.ClearPosition(piece.GetXBoard(), piece.GetYBoard());
        
        // Destroy the piece GameObject
        Destroy(piece.gameObject);
        
        Debug.Log($"[Tile_Lava] {piece.name} destroyed by lava!");
    }   
}
