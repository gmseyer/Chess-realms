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
        }



            // ---------------------------------------------------------

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

    // ----------------- Move Chessman -----------------
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
}






    public void SetCoords(int x, int y)
    {
        matrixX = x;
        matrixY = y;
    }

    public void SetReference(GameObject obj)
    {
        reference = obj;
    }

    public GameObject GetReference()
    {
        return reference;
    }


    
}
