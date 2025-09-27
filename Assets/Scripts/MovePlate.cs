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
    
    // Castling properties
    private bool isCastling = false;
    private string castlingType = "";

    public void Start()
    {
        if (attack)
        {
            //Set to red
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        }
        else if (isCastling)
        {
            //Set to blue for castling
            gameObject.GetComponent<SpriteRenderer>().color = new Color(0.0f, 0.0f, 1.0f, 1.0f);
        }
    }

    public void SetCastling(bool castling)
    {
        isCastling = castling;
    }

    public void SetCastlingType(string type)
    {
        castlingType = type;
    }

    public void OnMouseUp()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");
        Chessman movingPiece = reference.GetComponent<Chessman>();
        Knight knightComponent = movingPiece.GetComponent<Knight>();

        // ----------------- Handle Attacks -----------------
        if (attack)
        {
            // ✅ CHECK FOR VOID TILES ON ATTACK PATH FIRST
            if (movingPiece.CheckVoidTileOnPath(movingPiece.GetXBoard(), movingPiece.GetYBoard(), matrixX, matrixY))
            {
                Debug.Log($"[VoidTile] {movingPiece.name} destroyed by void tile during attack attempt!");
                movingPiece.DestroyByVoidTile();
                
                // Hide UI panels
                HideAllUIPanels();
                
                // End turn
                controller.GetComponent<Game>().NextTurn();
                return; // Stop processing - piece destroyed by void
            }

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

                // ✅ Check for ice marker attack FIRST (before any other processing)
                if (CheckIceMarkerAttack(matrixX, matrixY, movingPiece, cp))
                {
                    return; // Stop processing if ice marker attack was handled
                }
                
                // ✅ Check for fire marker attack (tracked piece captured)
                CheckFireMarkerAttack(matrixX, matrixY, movingPiece, cp);

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

                // ----------------- ROYAL KNIGHT PHANTOM SWAP SECTION -----------------
                // Check if the defending piece is a Royal Knight and trigger Phantom Swap passive
                if (cp.name.ToLower().Contains("royal_knight"))
                {
                    Debug.Log($"[MovePlate] Royal Knight is about to be taken: {cp.name} at ({matrixX},{matrixY}) by {movingPiece.name}");

                    RoyalKnight royalKnight = cp.GetComponent<RoyalKnight>();
                    if (royalKnight != null)
                    {
                        bool phantomSwapActivated = royalKnight.TryTriggerPhantomSwap();

                        if (phantomSwapActivated)
                        {
                            Debug.Log("[MovePlate] Royal Knight survives thanks to Phantom Swap!");
                            // Cancel capture flow: Royal Knight swapped positions with Mist Knight
                            movingPiece.DestroyMovePlates();
                            movingPiece.ClearFortify();
                            movingPiece.CheckMoveTiles_End();
                            controller.GetComponent<Game>().NextTurn();
                            return; // stop further processing
                        }
                        else
                        {
                            // Phantom Swap failed (on cooldown or no mist knight) - Royal Knight will be captured
                            // Clean up any existing mist knight since the Royal Knight is being destroyed
                            Debug.Log("[MovePlate] Phantom Swap failed - Royal Knight will be captured, cleaning up Mist Knight");
                            royalKnight.OnRoyalKnightDestroyed();
                        }
                    }
                }  // --ROYAL KNIGHT PHANTOM SWAP END -------------------------------------

                if (cp.name == "white_king") controller.GetComponent<Game>().Winner("black");
                if (cp.name == "black_king") controller.GetComponent<Game>().Winner("white");

                // ----------------- ARCHBISHOP SOULBINDING CONQUEST SECTION -----------------
                // Check if the attacking piece is an Archbishop
                if (movingPiece.name.Contains("arch_bishop"))
                {
                    Debug.Log($"[MovePlate] Archbishop {movingPiece.name} captured {cp.name} - checking Soulbinding Conquest!");
                    
                    // Check if Soulbinding Conquest can be triggered (not already used)
                    if (!Archbishop.soulbindingConquestUsed)
                    {
                        // Store the original state
                        bool wasUsed = Archbishop.soulbindingConquestUsed;
                        string originalCapturedPiece = Archbishop.capturedPieceName;
                        
                        // Trigger Soulbinding Conquest passive
                        Archbishop.TriggerSoulbindingConquest(cp.name);
                        
                        // Check if the skill was actually triggered (state changed)
                        if (Archbishop.soulbindingConquestUsed && Archbishop.capturedPieceName == cp.name)
                        {
                            // Don't end turn yet - spawn summon plates instead
                            movingPiece.DestroyMovePlates();
                            movingPiece.ClearFortify();
                            movingPiece.CheckMoveTiles_End();
                            
                            // Spawn summon plates
                            Archbishop archbishop = movingPiece.GetComponent<Archbishop>();
                            if (archbishop != null)
                            {
                                archbishop.SpawnSoulbindingSummonPlates();
                            }
                            
                            // Destroy the captured piece
                            Destroy(cp);
                            
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
                            }
                            if (SkillManagerTMP.Instance != null)
                            {
                                SkillManagerTMP.Instance.skillPanel?.SetActive(false);
                            }
                            
                            return; // Stop processing - don't call NextTurn() yet
                        }
                        else
                        {
                            // Skill was not triggered (invalid piece), restore original state
                            Archbishop.soulbindingConquestUsed = wasUsed;
                            Archbishop.capturedPieceName = originalCapturedPiece;
                            Debug.Log($"[MovePlate] Soulbinding Conquest not triggered for {cp.name} - invalid piece type");
                        }
                    }
                    else
                    {
                        Debug.Log("[MovePlate] Soulbinding Conquest already used this battle - normal capture.");
                    }
                }
                // --ARCHBISHOP SOULBINDING CONQUEST END -------------------------------------

                // ----------------- CHRONOMAGUS SOULBINDING CONQUEST SECTION -----------------
                // Check if the attacking piece is a Chronomagus
                if (movingPiece.name.Contains("chronomagus"))
                {
                    Debug.Log($"[MovePlate] Chronomagus {movingPiece.name} captured {cp.name} - checking Soulbinding Conquest!");
                    
                    // Check if Chronomagus Soulbinding Conquest can be triggered (not already used)
                    if (Chronomagus.IsChronomagusSoulbindingAvailable())
                    {
                        // Trigger Chronomagus Soulbinding Conquest passive
                        Chronomagus.TriggerChronomagusSoulbindingConquest(cp.name);
                        
                        // Check if the skill was actually triggered
                        if (Chronomagus.IsChronomagusSoulbindingAvailable() == false) // If it's now used, it was triggered
                        {
                            // Don't end turn yet - spawn summon plates instead
                            movingPiece.DestroyMovePlates();
                            movingPiece.ClearFortify();
                            movingPiece.CheckMoveTiles_End();
                            
                            // Spawn summon plates
                            Chronomagus chronomagus = movingPiece.GetComponent<Chronomagus>();
                            if (chronomagus != null)
                            {
                                chronomagus.SpawnChronomagusSoulbindingSummonPlates();
                            }
                            
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
                            }
                            if (SkillManagerTMP.Instance != null)
                            {
                                SkillManagerTMP.Instance.skillPanel?.SetActive(false);
                            }
                            
                            return; // Stop processing - don't call NextTurn() yet
                        }
                        else
                        {
                            Debug.Log($"[MovePlate] Chronomagus Soulbinding Conquest not triggered for {cp.name} - invalid piece type");
                        }
                    }
                    else
                    {
                        Debug.Log("[MovePlate] Chronomagus Soulbinding Conquest already used this battle - normal capture.");
                    }
                }
                // --CHRONOMAGUS SOULBINDING CONQUEST END -------------------------------------

                // ---------- WRAITH PAWN EXPLOSION CHECK ----------
                bool isWraithPawn = cp.name.ToLower().Contains("wraith_pawn");
                if (isWraithPawn)
                {
                    WraithPawn wraithPawn = cp.GetComponent<WraithPawn>();
                    if (wraithPawn != null)
                    {
                        wraithPawn.OnCaptured(); // This will trigger explosion and destroy the piece
                        // Don't return early - continue with normal capture flow
                    }
                }

                // Check for Sacred Zone SP gain before destroying piece
                CheckSacredZoneSPGain(cp.GetComponent<Chessman>(), matrixX, matrixY);

                // Check for bounty SP gain before destroying piece
                CheckBountySPGain(cp.GetComponent<Chessman>(), movingPiece);

                // Only destroy the piece if it's not a wraith pawn (wraith pawn destroys itself in explosion)
                if (!isWraithPawn)
                {
                    Destroy(cp);
                }

                // ---------- QUEEN DESTROYED LOG ----------
                if (cp.name.ToLower().Contains("queen"))
                {
                    Debug.Log($"[MovePlate] Queen destroyed: {cp.name} at ({matrixX},{matrixY})");
                }


                Knight attackerKnight = reference.GetComponent<Knight>();
                if (attackerKnight != null)
                {
                    // Check for Trial of Valor - add valor charge on capture
                    attackerKnight.AddValorCharge("capture");
                    
                    // Check if knight promotion just happened (knight was destroyed)
                    if (attackerKnight == null || !attackerKnight.gameObject.activeInHierarchy)
                    {
                        Debug.Log("[MovePlate] Knight promotion detected - skipping cleanup to preserve summon plates");
                        return; // Skip cleanup to preserve Royal Knight summon plates
                    }
                    
                    // Only check momentum if knight is still alive (promotion takes priority)
                    if (attackerKnight.IsMomentumReady())
                    {
                        // prevent the usual NextTurn flow: spawn momentum teleport tiles and let player choose
                        Knight.ActiveKnight = attackerKnight; // keep it selected (useful)
                        attackerKnight.TriggerKnightsMomentum();
                        return; // IMPORTANT: stop further processing so the player can click momentum tile
                    }
                }

                // ----------------- Wraith Pawn Cleanup -----------------
                // If we captured a wraith pawn, destroy it now after the move is complete
                if (isWraithPawn && cp != null)
                {
                    Debug.Log($"[MovePlate] Cleaning up wraith pawn {cp.name} after move completion");
                    controller.GetComponent<Game>().SetPositionEmpty(matrixX, matrixY);
                    Destroy(cp);
                }

                // ----------------- SoulboundCatalyst Passive Check -----------------
                // Check if the attacking piece is a WraithPawn and trigger SoulboundCatalyst
                if (reference != null && reference.name.ToLower().Contains("wraith_pawn"))
                {
                    WraithPawn attackingWraithPawn = reference.GetComponent<WraithPawn>();
                    if (attackingWraithPawn != null && cp != null)
                    {
                        Debug.Log($"[MovePlate] WraithPawn {reference.name} captured {cp.name} - triggering SoulboundCatalyst");
                        attackingWraithPawn.SoulboundCatalyst(cp);
                    }
                }

                // ----------------- Divinity Passive Check -----------------
                // Check if the attacking piece is a Royal Bishop and trigger Divinity passive
                if (reference != null && reference.name.ToLower().Contains("royal_bishop"))
                {
                    RoyalBishop attackingRoyalBishop = reference.GetComponent<RoyalBishop>();
                    if (attackingRoyalBishop != null && cp != null)
                    {
                        Debug.Log($"[MovePlate] Royal Bishop {reference.name} captured {cp.name} - triggering Divinity passive");
                        attackingRoyalBishop.DivinityPassive(cp);
                    }
                }

                // ----------------- Phantom Guard Passive Check (BEFORE moving) -----------------
                // Check if the attacking piece is a Royal Knight and trigger Phantom Guard passive
                int originalX = movingPiece.GetXBoard();
                int originalY = movingPiece.GetYBoard();
                
                Debug.Log($"[MovePlate] Checking for Royal Knight attack - piece name: {reference?.name}");
                
                if (reference != null && reference.name.ToLower().Contains("royal_knight"))
                {
                    Debug.Log($"[MovePlate] Royal Knight detected: {reference.name}");
                    RoyalKnight attackingRoyalKnight = reference.GetComponent<RoyalKnight>(); 
                    if (attackingRoyalKnight != null)
                    {
                        Debug.Log($"[MovePlate] Royal Knight {reference.name} attacking - will trigger Phantom Guard at ({originalX},{originalY})");
                        // Note: We'll trigger PhantomGuard after the piece moves
                    }
                    else
                    {
                        Debug.LogWarning($"[MovePlate] Royal Knight component not found on {reference.name}!");
                    }
                }

                // ----------------- Move Attacker to Captured Position -----------------
                controller.GetComponent<Game>().SetPositionEmpty(
                    movingPiece.GetXBoard(),
                    movingPiece.GetYBoard()
                );

                movingPiece.SetXBoard(matrixX);
                movingPiece.SetYBoard(matrixY);
                movingPiece.SetCoords();

                controller.GetComponent<Game>().SetPosition(reference);

                // Mark piece as moved (for castling tracking)
                movingPiece.SetHasMoved(true);

                // ----------------- Phantom Guard Passive Trigger (AFTER moving) -----------------
                // Trigger Phantom Guard if the attacking piece is a Royal Knight
                if (reference != null && reference.name.ToLower().Contains("royal_knight"))
                {
                    RoyalKnight attackingRoyalKnight = reference.GetComponent<RoyalKnight>();
                    if (attackingRoyalKnight != null)
                    {
                        Debug.Log($"[MovePlate] Triggering Phantom Guard for Royal Knight attack from ({originalX},{originalY})");
                        attackingRoyalKnight.PhantomGuard(originalX, originalY);
                    }
                }

                // ----------------- Check Thunder Tile Effect AFTER Attack -----------------
                bool attackThunderEffectTriggered = CheckTileThunderEffect(movingPiece, matrixX, matrixY);

                // ----------------- Temporal Anchor Passive Check -----------------
                // Check if the captured piece was allied to a Chronomagus
                if (!attackThunderEffectTriggered)
                {
                    Debug.Log("[MovePlate] Calling CheckTemporalAnchorPassive...");
                    CheckTemporalAnchorPassive(cp, reference);
                }
                else
                {
                    Debug.Log("[MovePlate] Skipping Temporal Anchor check due to thunder effect");
                }

                // Clean up (skip if thunder effect triggered)
                if (!attackThunderEffectTriggered)
                {
                    movingPiece.DestroyMovePlates();
                    movingPiece.ClearFortify();
                    movingPiece.CheckMoveTiles_End();
                    
                    // Check for check before ending turn
                    controller.GetComponent<Game>().CheckForCheck();
                    
                    controller.GetComponent<Game>().NextTurn();
                }

                return; // Stop processing after attack
            }
        }
    
        // ----------------- Handle Castling -----------------
        if (isCastling)
        {
            HandleCastling(movingPiece);
            return; // Stop processing after castling
        }

        // ----------------- CHECK FOR VOID TILES ON MOVEMENT PATH FIRST -----------------
        if (movingPiece.CheckVoidTileOnPath(movingPiece.GetXBoard(), movingPiece.GetYBoard(), matrixX, matrixY))
        {
            Debug.Log($"[VoidTile] {movingPiece.name} destroyed by void tile during movement attempt!");
            movingPiece.DestroyByVoidTile();
            
            // Hide UI panels
            HideAllUIPanels();
            
            // End turn
            controller.GetComponent<Game>().NextTurn();
            return; // Stop processing - piece destroyed by void
        }

        // ----------------- Tile Effects Check (BEFORE moving) -----------------
        bool iceEffectTriggered = CheckTileIceEffect(movingPiece, matrixX, matrixY);
        bool lavaEffectTriggered = CheckTileLavaEffect(movingPiece, matrixX, matrixY);
        bool thunderEffectTriggered = CheckTileThunderEffect(movingPiece, matrixX, matrixY);

        // ----------------- Move Chessman (skip if tile effects triggered) -----------------
        if (!iceEffectTriggered && !lavaEffectTriggered && !thunderEffectTriggered)
        {
            // Store original position for Phantom Guard before moving
            int originalX = movingPiece.GetXBoard();
            int originalY = movingPiece.GetYBoard();

            controller.GetComponent<Game>().SetPositionEmpty(
                movingPiece.GetXBoard(),
                movingPiece.GetYBoard()
            );

            movingPiece.SetXBoard(matrixX);
            movingPiece.SetYBoard(matrixY);
            movingPiece.SetCoords();

            controller.GetComponent<Game>().SetPosition(reference);

            // Mark piece as moved (for castling tracking)
            movingPiece.SetHasMoved(true);

            // ----------------- Phantom Guard Passive Trigger (AFTER moving) -----------------
            // Trigger Phantom Guard if the moving piece is a Royal Knight
            Debug.Log($"[MovePlate] Checking for Royal Knight movement - piece name: {reference?.name}");
            
            if (reference != null && reference.name.ToLower().Contains("royal_knight"))
            {
                Debug.Log($"[MovePlate] Royal Knight detected for movement: {reference.name}");
                RoyalKnight movingRoyalKnight = reference.GetComponent<RoyalKnight>();
                if (movingRoyalKnight != null)
                {
                    Debug.Log($"[MovePlate] Triggering Phantom Guard for Royal Knight movement from ({originalX},{originalY})");
                    movingRoyalKnight.PhantomGuard(originalX, originalY);
                }
                else
                {
                    Debug.LogWarning($"[MovePlate] Royal Knight component not found on {reference.name}!");
                }
            }

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
            
            // Check for check before ending turn
            controller.GetComponent<Game>().CheckForCheck();
            
            controller.GetComponent<Game>().NextTurn();
        }
        else
        {
            // ----------------- Celestial Orb Capture Check -----------------
            // Check if a Rook moved and captured a celestial orb
            if (movingPiece.name.Contains("white_rook"))
            {
                Vector2Int rookPosition = new Vector2Int(matrixX, matrixY);
                Rook.CheckCelestialOrbCapture(rookPosition);
            }
            
            // Normal turn ending
            
            // Check for check before ending turn
            controller.GetComponent<Game>().CheckForCheck();
            
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
            UIManager.Instance.whiteRoyalKnightPanel?.SetActive(false);
            UIManager.Instance.whiteRoyalPawnPanel?.SetActive(false);
            UIManager.Instance.whiteSpectralHeraldPanel?.SetActive(false);
            UIManager.Instance.whiteChronomagusPanel?.SetActive(false);
            
            // Hide status panel when hiding all panels
            UIManager.Instance.HideStatusPanel();
            
            // Clear selected piece
            UIManager.Instance.selectedPiece = null;
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
                 game.SetPositionEmpty(movingPiece.GetXBoard(), movingPiece.GetYBoard());
            movingPiece.SetXBoard(x);
            movingPiece.SetYBoard(y);
            movingPiece.SetCoords();
            game.SetPosition(movingPiece.gameObject);
            
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
             game.SetPositionEmpty(movingPiece.GetXBoard(), movingPiece.GetYBoard());
            movingPiece.SetXBoard(x);
            movingPiece.SetYBoard(y);
            movingPiece.SetCoords();
            game.SetPosition(movingPiece.gameObject);
            
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

    private bool CheckTileThunderEffect(Chessman movingPiece, int x, int y)
    {
        Game game = controller.GetComponent<Game>();
        
        // Check if there's a tile_thunder at the destination
        GameObject tileAtPosition = game.GetPosition(x, y);
        if (tileAtPosition != null && tileAtPosition.name == "tile_thunder")
        {
            // Check if it's an immune piece (Kings, Elemental Bishop, Chronomagus)
            if (movingPiece.name == "white_elemental_bishop" || movingPiece.name == "white_king" || 
                movingPiece.name == "black_king" || movingPiece.name == "white_chronomagus" || 
                movingPiece.name == "black_chronomagus")
            {
                Debug.Log($"[Tile_Thunder] {movingPiece.name} is immune to thunder - tile disappears!");
                
                // Just destroy the thunder tile (no stun effect)
                DestroyThunderTile(game, x, y);
                 game.SetPositionEmpty(movingPiece.GetXBoard(), movingPiece.GetYBoard());
            movingPiece.SetXBoard(x);
            movingPiece.SetYBoard(y);
            movingPiece.SetCoords();
            game.SetPosition(movingPiece.gameObject);
            
                // Clean up and end turn
                movingPiece.DestroyMovePlates();
                movingPiece.ClearFortify();
                movingPiece.CheckMoveTiles_End();
                //game.NextTurn(); REMOVED CAUSING DOUBLE TURN ENDING
                
                return true; // Effect triggered, skip normal cleanup
            }
            
            // Apply stun effect to the piece (at original position)
            ApplyThunderStun(movingPiece, game);
            
            // Move the stunned piece to the thunder tile position
           
            // Destroy the thunder tile (one-use effect)
            DestroyThunderTile(game, x, y);
             game.SetPositionEmpty(movingPiece.GetXBoard(), movingPiece.GetYBoard());
            movingPiece.SetXBoard(x);
            movingPiece.SetYBoard(y);
            movingPiece.SetCoords();
            game.SetPosition(movingPiece.gameObject);
            
            
            // Clean up and end turn
            movingPiece.DestroyMovePlates(); 
            movingPiece.ClearFortify();
            movingPiece.CheckMoveTiles_End();

            //game.NextTurn();
            
            return true; // Effect triggered, skip normal cleanup
        }
        
        return false; // No thunder effect
    }

    private void ApplyThunderStun(Chessman movingPiece, Game game)
    {
        int currentTurn = game.GetTurnCount();
        int stunDuration = 2; // Stun for 2 turns
        
        // Apply stunned status using StatusManager
        StatusManager statusManager = movingPiece.GetComponent<StatusManager>();
        if (statusManager != null)
        {
            statusManager.AddStatus(StatusType.Stunned, currentTurn + stunDuration);
            Debug.Log($"[Tile_Thunder] {movingPiece.name} stunned for {stunDuration} turns (until turn {currentTurn + stunDuration})");
        }
        else
        {
            Debug.LogError($"[Tile_Thunder] StatusManager not found on {movingPiece.name}!");
        }
    }

    private void DestroyThunderTile(Game game, int x, int y)
    {
        // Get the thunder tile at the position
        GameObject thunderTile = game.GetPosition(x, y);
        if (thunderTile != null && thunderTile.name == "tile_thunder")
        {
            // Clear the position in the game board
            game.SetPositionEmpty(x, y);
            
            // Destroy the GameObject
            Destroy(thunderTile);
            
            Debug.Log($"[Tile_Thunder] Thunder tile destroyed at ({x},{y}) - one-use effect consumed!");
        }
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

    private void CheckSacredZoneSPGain(Chessman capturedPiece, int x, int y)
    {
        Game game = controller.GetComponent<Game>();
        if (game == null) return;

        // Check if the captured piece is an allied piece (white)
        if (capturedPiece.GetPlayer() != "white") return;

        // Find sanctuary markers at this position
        SanctuaryMarker[] allSanctuaryMarkers = FindObjectsOfType<SanctuaryMarker>();
        foreach (SanctuaryMarker marker in allSanctuaryMarkers)
        {
            if (marker.GetX() == x && marker.GetY() == y && marker.IsActive())
            {
                // This piece died on an active sanctuary marker
                marker.OnPieceDeath(capturedPiece);
                break; // Only one marker per position
            }
        }
    }

    private void CheckBountySPGain(Chessman capturedPiece, Chessman attacker)
    {
        Game game = controller.GetComponent<Game>();
        if (game == null) return;

        // Check if the captured piece has bounty status
        if (capturedPiece.statusManager.HasBounty(game.turns))
        {
            int bountyValue = capturedPiece.statusManager.GetBountyValue(game.turns);
            string attackerPlayer = attacker.GetPlayer();
            
            // Grant SP to the attacker's player
            if (SkillManager.Instance != null)
            {
                SkillManager.Instance.AddPlayerSP(attackerPlayer, bountyValue);
                Debug.Log($"[Bounty] {attacker.name} captured {capturedPiece.name} with bounty {bountyValue} SP! {attackerPlayer} gained {bountyValue} SP.");
            }
        }
    }

    private void HandleCastling(Chessman movingPiece)
    {
        Game game = controller.GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[MovePlate] Game component not found for castling!");
            return;
        }

        string player = movingPiece.GetPlayer();
        int kingX = movingPiece.GetXBoard();
        int kingY = movingPiece.GetYBoard();

        Debug.Log($"[MovePlate] Executing {castlingType} castling for {player} King");

        if (castlingType == "kingside")
        {
            // King-side castling: King moves 2 right, Rook moves to King's left
            GameObject rightRook = game.GetPosition(7, kingY);
            if (rightRook != null)
            {
                // Move King
                game.SetPositionEmpty(kingX, kingY);
                movingPiece.SetXBoard(kingX + 2);
                movingPiece.SetYBoard(kingY);
                movingPiece.SetCoords();
                game.SetPosition(movingPiece.gameObject);

                // Move Rook
                game.SetPositionEmpty(7, kingY);
                Chessman rookChessman = rightRook.GetComponent<Chessman>();
                rookChessman.SetXBoard(kingX + 1);
                rookChessman.SetYBoard(kingY);
                rookChessman.SetCoords();
                game.SetPosition(rightRook);

                // Mark both pieces as moved
                movingPiece.SetHasMoved(true);
                rookChessman.SetHasMoved(true);

                Debug.Log($"[MovePlate] King-side castling completed - King at ({kingX + 2},{kingY}), Rook at ({kingX + 1},{kingY})");
            }
        }
        else if (castlingType == "queenside")
        {
            // Queen-side castling: King moves 2 left, Rook moves to King's right
            GameObject leftRook = game.GetPosition(0, kingY);
            if (leftRook != null)
            {
                // Move King
                game.SetPositionEmpty(kingX, kingY);
                movingPiece.SetXBoard(kingX - 2);
                movingPiece.SetYBoard(kingY);
                movingPiece.SetCoords();
                game.SetPosition(movingPiece.gameObject);

                // Move Rook
                game.SetPositionEmpty(0, kingY);
                Chessman rookChessman = leftRook.GetComponent<Chessman>();
                rookChessman.SetXBoard(kingX - 1);
                rookChessman.SetYBoard(kingY);
                rookChessman.SetCoords();
                game.SetPosition(leftRook);

                // Mark both pieces as moved
                movingPiece.SetHasMoved(true);
                rookChessman.SetHasMoved(true);

                Debug.Log($"[MovePlate] Queen-side castling completed - King at ({kingX - 2},{kingY}), Rook at ({kingX - 1},{kingY})");
            }
        }

        // Clean up move plates and end turn
        movingPiece.DestroyMovePlates();
        
        // Check for check before ending turn
        game.CheckForCheck();
        
        game.NextTurn();
    }

    // Check Temporal Anchor passive
    private void CheckTemporalAnchorPassive(GameObject capturedPiece, GameObject attacker)
    {
        Debug.Log($"[MovePlate] CheckTemporalAnchorPassive called - Captured: {capturedPiece?.name}, Attacker: {attacker?.name}");
        
        if (capturedPiece == null || attacker == null) 
        {
            Debug.Log("[MovePlate] Temporal Anchor check failed - null pieces");
            return;
        }

        // Get the captured piece's player
        Chessman capturedChessman = capturedPiece.GetComponent<Chessman>();
        if (capturedChessman == null) 
        {
            Debug.Log("[MovePlate] Temporal Anchor check failed - no Chessman on captured piece");
            return;
        }

        string capturedPlayer = capturedChessman.GetPlayer();
        if (string.IsNullOrEmpty(capturedPlayer)) 
        {
            Debug.Log("[MovePlate] Temporal Anchor check failed - no player on captured piece");
            return;
        }

        Debug.Log($"[MovePlate] Captured piece player: {capturedPlayer}");

        // Find all Chronomagus pieces of the same player as the captured piece
        Chronomagus[] allChronomagus = FindObjectsOfType<Chronomagus>();
        Debug.Log($"[MovePlate] Found {allChronomagus.Length} Chronomagus pieces on board");
        
        foreach (Chronomagus chronomagus in allChronomagus)
        {
            Debug.Log($"[MovePlate] Processing Chronomagus: {chronomagus?.name}");
            
            if (chronomagus == null) 
            {
                Debug.Log("[MovePlate] Chronomagus is null, skipping");
                continue;
            }

            Chessman chronomagusChessman = chronomagus.GetComponent<Chessman>();
            if (chronomagusChessman == null) 
            {
                Debug.Log($"[MovePlate] No Chessman component on {chronomagus.name}, skipping");
                continue;
            }

            string chronomagusPlayer = chronomagusChessman.GetPlayer();
            Debug.Log($"[MovePlate] Checking Chronomagus {chronomagus.name} (player: {chronomagusPlayer})");

            // Check if this Chronomagus is allied to the captured piece
            if (chronomagusPlayer == capturedPlayer)
            {
                Debug.Log($"[MovePlate] Found allied Chronomagus! Checking if Temporal Anchor is available...");
                
                // Check if Temporal Anchor is available
                if (chronomagus.IsTemporalAnchorAvailable())
                {
                    Debug.Log($"[MovePlate] Temporal Anchor triggered! {capturedPiece.name} was captured by {attacker.name}");
                    chronomagus.TriggerTemporalAnchor(attacker);
                    return; // Only trigger once per capture
                }
                else
                {
                    Debug.Log($"[MovePlate] Temporal Anchor on cooldown for {chronomagus.name}");
                }
            }
            else
            {
                Debug.Log($"[MovePlate] Chronomagus {chronomagus.name} (player: {chronomagusPlayer}) is not allied to captured piece (player: {capturedPlayer})");
            }
        }
        
        Debug.Log("[MovePlate] No allied Chronomagus found or Temporal Anchor not available");
    }

    // ✅ Check for ice marker attack (tracked piece attacked - slide effect)
    private bool CheckIceMarkerAttack(int x, int y, Chessman attacker, GameObject defender)
    {
        // Find ice marker at this position
        Marker[] allMarkers = FindObjectsOfType<Marker>();
        foreach (Marker marker in allMarkers)
        {
            if (marker.x == x && marker.y == y && marker.trackedPieceName == defender.name && marker.markerType == MarkerType.Ice)
            {
                Debug.Log($"[IceMarker] Tracked piece {marker.trackedPieceName} attacked at ({x},{y}) - handling slide effect");
                
                // 1. Mark as handled by attack to prevent move case
                marker.wasHandledByAttack = true;
                
                // 2. Get the attacker's current position
                int attackerX = attacker.GetXBoard();
                int attackerY = attacker.GetYBoard();
                
                // 3. Find empty tiles around the defender (3x3 area)
                List<Vector2Int> emptyTiles = new List<Vector2Int>();
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int checkX = x + dx;
                        int checkY = y + dy;
                        
                        // Check if position is valid and empty
                        if (checkX >= 0 && checkX < 8 && checkY >= 0 && checkY < 8)
                        {
                            GameObject pieceAtPos = controller.GetComponent<Game>().GetPosition(checkX, checkY);
                            if (pieceAtPos == null)
                            {
                                emptyTiles.Add(new Vector2Int(checkX, checkY));
                            }
                        }
                    }
                }
                
                // 4. If no empty tiles found, don't slide (attack fails)
                if (emptyTiles.Count == 0)
                {
                    Debug.Log($"[IceMarker] No empty tiles around defender - attack fails");
                    return false;
                }
                
                // 5. Randomly select an empty tile
                Vector2Int targetTile = emptyTiles[Random.Range(0, emptyTiles.Count)];
                
                // 6. Move attacker to the random empty tile
                attacker.SetXBoard(targetTile.x);
                attacker.SetYBoard(targetTile.y);
                attacker.SetCoords();
                
                // 7. Update positions
                Game game = controller.GetComponent<Game>();
                game.SetPositionEmpty(attackerX, attackerY);
                game.SetPositionAt(attacker.gameObject, targetTile.x, targetTile.y);
                
                // 8. Destroy the ice marker
                Destroy(marker.gameObject);
                
                // 9. Clean up and end turn
                attacker.DestroyMovePlates();
                attacker.ClearFortify();
                attacker.CheckMoveTiles_End();
                
                // Check for check before ending turn
                game.CheckForCheck();
                
                game.NextTurn();
                
                Debug.Log($"[IceMarker] Attack case: Attacker slid to ({targetTile.x},{targetTile.y}), ice marker destroyed - turn ends");
                return true; // Attack was handled
            }
        }
        
        return false; // No ice marker found, attack not handled
    }

    // ✅ Check for fire marker attack (tracked piece captured)
    private void CheckFireMarkerAttack(int x, int y, Chessman attacker, GameObject defender)
    {
        // Find fire marker at this position
        Marker[] allMarkers = FindObjectsOfType<Marker>();
        foreach (Marker marker in allMarkers)
        {
            if (marker.x == x && marker.y == y && marker.trackedPieceName == defender.name && marker.markerType == MarkerType.Fire)
            {
                Debug.Log($"[FireMarker] Tracked piece {marker.trackedPieceName} captured at ({x},{y}) - handling attack case");
                
                // Mark as handled by attack to prevent double handling
                marker.wasHandledByAttack = true;
                
                // 1. Convert fire marker to lava tile (tracked piece captured)
                marker.ConvertToTile();
                
                // 2. Find the lava tile that was just created
                GameObject lavaTile = controller.GetComponent<Game>().GetPosition(x, y);
                if (lavaTile != null && lavaTile.name == "tile_lava")
                {
                    // 3. Destroy the lava tile immediately
                    controller.GetComponent<Game>().SetPositionEmpty(x, y);
                    Destroy(lavaTile);
                    Debug.Log($"[FireMarker] Lava tile destroyed at ({x},{y})");
                }
                
                // 4. Destroy both pieces
                Destroy(attacker.gameObject);
                Destroy(defender);
                
                // 5. Clear positions
                Game game = controller.GetComponent<Game>();
                game.SetPositionEmpty(attacker.GetXBoard(), attacker.GetYBoard());
                game.SetPositionEmpty(x, y);
                
                // Clean up and end turn
                attacker.DestroyMovePlates();
                attacker.ClearFortify();
                attacker.CheckMoveTiles_End();
                
                Debug.Log($"[FireMarker] Attack case: Both pieces destroyed, fire marker converted and destroyed - turn ends");
                return; // Stop further processing
            }
        }
    }

    // Helper function to hide all UI panels
    private void HideAllUIPanels()
    {
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
            UIManager.Instance.whiteRoyalKnightPanel?.SetActive(false);
            UIManager.Instance.whiteRoyalPawnPanel?.SetActive(false);
            UIManager.Instance.whiteSpectralHeraldPanel?.SetActive(false);
            UIManager.Instance.whiteChronomagusPanel?.SetActive(false);
            
            // Hide status panel when hiding all panels
            UIManager.Instance.HideStatusPanel();
            
            // Clear selected piece
            UIManager.Instance.selectedPiece = null;
        }
        if (SkillManagerTMP.Instance != null)
        {
            SkillManagerTMP.Instance.skillPanel?.SetActive(false);
        }
    }


} 
