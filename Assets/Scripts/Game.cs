using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour
{

     public static Game Instance; //added for easy access to Game instance, mostly for SkillManager
    public GameObject chesspiece;
    //public GameObject pawnPrefab;

    //public Pawn selectedPawn;

    public int turns = 0;
    public TurnUI turnUI;
    [SerializeField] Text turnText;
    //Matrices needed, positions of each of the GameObjects
    //Also separate arrays for the players in order to easily keep track of them all
    //Keep in mind that the same objects are going to be in "positions" and "playerBlack"/"playerWhite"
    private GameObject[,] positions = new GameObject[8, 8];
    private GameObject[] playerBlack = new GameObject[16];
    private GameObject[] playerWhite = new GameObject[16];

    private GameObject[] playerNeutral = new GameObject[32]; // for future use

    //current turn
    private string currentPlayer = "white";

    //Game Ending
    private bool gameOver = false;
    public int whiteSkillPoints = 4;
    public int blackSkillPoints = 4;
    public GameObject movePlatePrefabReference; 
    
    public string restrictedToPawnsPlayer = null;   // "white" / "black" 
    public int restrictionExpiresOnTurn = -1;       // until (inclusive) this turn
    


  

   
    public void Start()
    {
        playerWhite = new GameObject[] {
            Create("white_rook", 0, 0), Create("white_knight", 1, 0),
            Create("white_bishop", 2, 0), Create("white_queen", 3, 0), Create("white_king", 4, 0),
            Create("white_bishop", 5, 0), Create("white_knight", 6, 0), Create("white_rook", 7, 0),
            

            Create("white_pawn", 0, 1), Create("white_pawn1", 1, 1), Create("white_pawn2", 2, 1),
             Create("white_pawn3", 3, 1), Create("white_pawn4", 4, 1), Create("white_pawn5", 5, 1),
             Create("white_pawn6", 6, 1), Create("white_pawn7", 7, 1)
            };

        playerBlack = new GameObject[] { Create("black_rook", 0, 7), Create("black_knight",1,7),
            Create("black_bishop",2,7), Create("black_queen",3,7), Create("black_king",4,7),
            Create("black_bishop",5,7), Create("black_knight",6,7), Create("black_rook",7,7),

            Create("black_pawn", 0, 6), Create("black_pawn1", 1, 6), Create("black_pawn2", 2, 6),
            Create("black_pawn3", 3, 6), Create("black_pawn4", 4, 6), Create("black_pawn5", 5, 6),
            Create("black_pawn6", 6, 6), Create("black_pawn7", 7, 6)
            };

        playerNeutral = new GameObject[] {

        };

        //Set all piece positions on the positions board
        for (int i = 0; i < playerBlack.Length; i++)
        {
            SetPosition(playerBlack[i]);
            SetPosition(playerWhite[i]);
            SetPosition(playerNeutral[i]);
        }



    }

    public GameObject Create(string name, int x, int y)
    {
        GameObject obj;
         obj = Instantiate(chesspiece, new Vector3(0, 0, -1), Quaternion.identity);
        Chessman cm = obj.GetComponent<Chessman>();
        cm.controller = this.gameObject; // Also assign controller for other pieces!
        cm.name = name;
        cm.SetXBoard(x);
        cm.SetYBoard(y);


        cm.SetCoords();
        SetPosition(obj);

        if (name.StartsWith("white"))
        cm.SetPlayer("white");
    else if (name.StartsWith("black"))
        cm.SetPlayer("black");
    else if (name.StartsWith("tile"))
        cm.SetPlayer("neutral");

        else if (name == "tile_earth")
{
    cm.SetPlayer("neutral");          // neutral tile
    cm.statusManager.AddStatus(StatusType.Invulnerable, 999); // never expires
    cm.statusManager.AddStatus(StatusType.SolidBlock, 999);   // blocks movement
}   


        if (name.Contains("bishop"))
        {
            if (obj.GetComponent<Bishop>() == null)
            {
                Bishop b = obj.AddComponent<Bishop>();
                // ✅ assign prefab from a central reference
                b.movePlatePrefab = movePlatePrefabReference;
            }
        }
        if (name.Contains("knight"))
        {
            if (obj.GetComponent<Knight>() == null)
            {
        Knight k = obj.AddComponent<Knight>();
        k.movePlatePrefab = movePlatePrefabReference;
            }
        }

        if (name.Contains("queen"))
        {
            if (obj.GetComponent<Queen>() == null)
            {
                Queen q = obj.AddComponent<Queen>();
                q.movePlatePrefab = movePlatePrefabReference;
            }
        }
   

        cm.Activate();
        return obj;
    }

    public void SetPosition(GameObject obj)
    {
        Chessman cm = obj.GetComponent<Chessman>();

        //Overwrites either empty space or whatever was there
        positions[cm.GetXBoard(), cm.GetYBoard()] = obj;
    }

    public void SetPositionEmpty(int x, int y)
    {
        positions[x, y] = null;
    }

    public GameObject GetPosition(int x, int y)
    {
        return positions[x, y];
    }

    public bool PositionOnBoard(int x, int y)
    {
        if (x < 0 || y < 0 || x >= positions.GetLength(0) || y >= positions.GetLength(1)) return false;
        return true;
    }

    public string GetCurrentPlayer()
    {
        return currentPlayer;
    }

    public bool IsGameOver()
    {
        return gameOver;
    }

   public void NextTurn()
{
    // Switch player
    currentPlayer = (currentPlayer == "white") ? "black" : "white";
    turns++;

    ClearExpiredRestrictions();
    ResetAllPieceTurnFlags();
    ClearExpiredStatuses();

    // Update the Turn UI with player
    if(TurnUI.Instance != null)
        TurnUI.Instance.UpdateTurn(turns, currentPlayer);

    // Elemental Bishop cleanup
    ElementalBishop eb = FindObjectOfType<ElementalBishop>();
    if(eb != null)
        eb.CheckAndDestroyExpiredTiles();
}


    //for Skill Manager
    public int GetTurnCount()
{
    return turns;
}

    public void Update()
    {
        if (gameOver == true && Input.GetMouseButtonDown(0))
        {
            gameOver = false;

            //Using UnityEngine.SceneManagement is needed here
            SceneManager.LoadScene("Game"); //Restarts the game by loading the scene over again
        }
    }

    public void Winner(string playerWinner)
    {
        gameOver = true;

        //Using UnityEngine.UI is needed here
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().enabled = true;
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().text = playerWinner + " is the winner";

        GameObject.FindGameObjectWithTag("RestartText").GetComponent<Text>().enabled = true;
    }


    public void SetPlayerRestriction(string player, int durationTurns)
{
    restrictedToPawnsPlayer = player;
    restrictionExpiresOnTurn = turns + durationTurns;
    Debug.Log($"[Game] Player '{player}' restricted to pawns until turn {restrictionExpiresOnTurn} (current turn {turns})");
}

    // Query helper


public bool IsPlayerRestrictedToPawns(string player)
    {
        if (restrictedToPawnsPlayer == null) return false;
        // restriction is active while current turns <= expire turn
        return restrictedToPawnsPlayer == player && turns <= restrictionExpiresOnTurn;
    }

// Call this periodically (we'll call it in NextTurn) to clear expired restriction
private void ClearExpiredRestrictions()
{
    if (restrictedToPawnsPlayer != null && turns > restrictionExpiresOnTurn)
    {
        Debug.Log($"[Game] Temporal Shift expired for {restrictedToPawnsPlayer} at turn {turns}");
        restrictedToPawnsPlayer = null;
        restrictionExpiresOnTurn = -1;
    }
}


    private void ClearExpiredStatuses()
    {
        // iterate all positions on the board
        for (int x = 0; x < positions.GetLength(0); x++)
        {
            for (int y = 0; y < positions.GetLength(1); y++)
            {
                GameObject obj = positions[x, y];
                if (obj == null) continue;
                Chessman cm = obj.GetComponent<Chessman>();
                if (cm == null) continue;
                if (cm.isInvulnerable && turns >= cm.invulnerableUntilTurn)
                {
                    cm.isInvulnerable = false;
                    cm.invulnerableUntilTurn = -1;
                    Debug.Log($"{cm.name} invulnerability expired at turn {turns}");
                }
            }
        }
    }
    //still helpers
    public void ClearPosition(int x, int y)
{
    positions[x, y] = null;
}

// Call this at the start of each player's turn
public void ResetAllPieceTurnFlags()
{
    Chessman[] allPieces = GameObject.FindObjectsOfType<Chessman>();
    foreach (Chessman pieceCm in allPieces)
    {
        Pieces pieceScript = pieceCm.GetComponent<Pieces>();
        if (pieceScript != null)
        {
            pieceScript.ResetTurnFlags();
        }
    }
}




// Sets a GameObject at a specific position
public void SetPositionAt(GameObject obj, int x, int y)
{
    positions[x, y] = obj;
}

// Get remaining SP for a given player string ("white" or "black")
    public int GetPlayerSP(string player)
    {
        return (player == "white") ? whiteSkillPoints : blackSkillPoints;
    }

    // Try to deduct, return true if successful
    public bool SpendPlayerSP(string player, int amount)
    {
        if (player == "white")
        {
            if (whiteSkillPoints >= amount)
            {
                whiteSkillPoints -= amount;
                return true;
            }
            return false;
        }
        else
        {
            if (blackSkillPoints >= amount)
            {
                blackSkillPoints -= amount;
                return true;
            }
            return false;
        }
    }






}
