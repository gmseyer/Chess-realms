using UnityEngine;

public class ElementalTileExpiration : MonoBehaviour
{
    private int expirationTurn;
    private Game game;

    private void Start()
    {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
    }

    public void SetExpirationTurn(int turn)
    {
        expirationTurn = turn;
    }

    private void Update()
    {
        if (game == null) return;

        int currentTurn = game.GetTurnCount();
        if (currentTurn >= expirationTurn)
        {
            // Tile has expired, destroy it
            Debug.Log($"[ElementalTileExpiration] {gameObject.name} expired at turn {currentTurn}");
            
            // Clear the position on the game board
            Chessman chessman = GetComponent<Chessman>();
            if (chessman != null)
            {
                game.SetPositionEmpty(chessman.GetXBoard(), chessman.GetYBoard());
            }
            
            // Destroy the tile
            Destroy(gameObject);
        }
    }
}
