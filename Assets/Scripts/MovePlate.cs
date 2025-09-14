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

    // Destroy the victim Chesspiece if this is an attack
    if (attack)
    {
        GameObject cp = controller.GetComponent<Game>().GetPosition(matrixX, matrixY);
        if (cp != null)
        {
            Chessman targetCm = cp.GetComponent<Chessman>();

            // ✅ Special check for bishop capture
          if (cp.name == "white_bishop")
{
    Debug.Log("white_bishop was captured! Checking for Bishop component...");
    Bishop bishop = cp.GetComponent<Bishop>();

    if (bishop != null && !targetCm.isInvulnerable)
    {
        Debug.Log("✅ Bishop component found — triggering DivineOffering()");

        // ✅ First: remove bishop from board properly
        controller.GetComponent<Game>().SetPositionEmpty(matrixX, matrixY); 
        Destroy(cp); // <-- actually remove the bishop GameObject

        // ✅ Then: trigger the divine offering tiles
        bishop.OnBishopButtonClick();

        // ✅ Stop further processing so we don't also call NextTurn here
        return;
    }
    else
    {
        Debug.LogError("❌ Bishop component NOT found on captured piece!");
    }
}


            // Existing invulnerability check
            if (targetCm != null && targetCm.isInvulnerable)
            {
                Debug.Log($"{targetCm.name} is invulnerable — attack cancelled.");
                return;
            }

            if (cp.name == "white_king") controller.GetComponent<Game>().Winner("black");
            if (cp.name == "black_king") controller.GetComponent<Game>().Winner("white");

            Destroy(cp);
        }
    }

    // ✅ Normal move logic (runs ONLY if no bishop special ability triggered)
    controller.GetComponent<Game>().SetPositionEmpty(
        reference.GetComponent<Chessman>().GetXBoard(),
        reference.GetComponent<Chessman>().GetYBoard()
    );

    reference.GetComponent<Chessman>().SetXBoard(matrixX);
    reference.GetComponent<Chessman>().SetYBoard(matrixY);
    reference.GetComponent<Chessman>().SetCoords();

    controller.GetComponent<Game>().SetPosition(reference);

    controller.GetComponent<Game>().NextTurn();

    reference.GetComponent<Chessman>().DestroyMovePlates();
    reference.GetComponent<Chessman>().ClearFortify();

    Chessman movingPiece = reference.GetComponent<Chessman>();
    movingPiece.CheckMoveTiles_End();

    if (UIManager.Instance != null)
    {
        UIManager.Instance.pawnPanel?.SetActive(false);
        UIManager.Instance.knightPanel?.SetActive(false);
        UIManager.Instance.bishopPanel?.SetActive(false);
        UIManager.Instance.rookPanel?.SetActive(false);
        UIManager.Instance.queenPanel?.SetActive(false);
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
