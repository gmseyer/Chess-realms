using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chessman : MonoBehaviour
{
    //References to objects in our Unity Scene
    public GameObject controller;
    public GameObject movePlate;

    private GameObject panelForThisPiece;


    //Position for this Chesspiece on the Board
    //The correct position will be set later
    protected int xBoard = -1;
    protected int yBoard = -1;

    //Variable for keeping track of the player it belongs to "black" or "white"
    protected string player;

    //References to all the possible Sprites that this Chesspiece could be
    public Sprite black_queen, black_knight, black_bishop, black_king, black_rook, black_pawn;
    public Sprite white_queen, white_knight, white_bishop, white_king, white_rook, white_pawn;

    //summoned units
     public Sprite white_elemental_bishop;

    //Elemental Tiles
     public Sprite tile_lava;
    public Sprite tile_ice;
    public Sprite tile_earth;

    [HideInInspector] public bool fortifyActive = false; // ---------- temporary status ----------
    [HideInInspector] public bool isInvulnerable = false;        // immune to attack
    [HideInInspector] public int invulnerableUntilTurn = -1;      // inclusive turn when it expires
    [HideInInspector] public StatusManager statusManager;
    [Header("Skill Points")]
    public int skillPoints = 4; // default SP per piece




    private void Start()
    {
        if (UIManager.Instance != null)
        {
            panelForThisPiece = UIManager.Instance.GetPanelForPieceName(name);
        }
        else
        {
            // fallback (only if you forgot to put UIManager in scene)
            panelForThisPiece = GameObject.Find(name.Contains("knight") ? "KnightPanel" : "PawnPanel");
        }
    }


    private void Awake()
    {
        statusManager = gameObject.AddComponent<StatusManager>();
    }


    //****************TEST FUNCTION ADD****************************

    private int lastX;
    private int lastY;
    

    public void CheckMoveTiles_Start()
    {
        lastX = GetXBoard();
        lastY = GetYBoard();
        Debug.Log($"{name} START position: ({lastX}, {lastY})");
    }

     public void CheckMoveTiles_End()
    {
        int newX = GetXBoard();
        int newY = GetYBoard();

        Debug.Log($"{name} END position: ({newX}, {newY})");

        if (newX != lastX || newY != lastY)
        {
            Debug.Log($"{name} MOVED from ({lastX},{lastY}) to ({newX},{newY})");
        }
        else
        {
            Debug.Log($"{name} DID NOT MOVE");
        }
    }









    public void Activate()
    {
        //Get the game controller
        controller = GameObject.FindGameObjectWithTag("GameController");

        //Take the instantiated location and adjust transform
        SetCoords();

        //Choose correct sprite based on piece's name
        if (this.name.StartsWith("black_pawn"))
        {
            this.GetComponent<SpriteRenderer>().sprite = black_pawn;
            player = "black";
        }
        else if (this.name.StartsWith("white_pawn"))
        {
            this.GetComponent<SpriteRenderer>().sprite = white_pawn;
            player = "white";
        }
        else
        {
            switch (this.name)
            {
                case "black_queen": this.GetComponent<SpriteRenderer>().sprite = black_queen; player = "black"; break;
                case "black_knight": this.GetComponent<SpriteRenderer>().sprite = black_knight; player = "black"; break;
                case "black_bishop": this.GetComponent<SpriteRenderer>().sprite = black_bishop; player = "black"; break;
                case "black_king": this.GetComponent<SpriteRenderer>().sprite = black_king; player = "black"; break;
                case "black_rook": this.GetComponent<SpriteRenderer>().sprite = black_rook; player = "black"; break;
                case "white_queen": this.GetComponent<SpriteRenderer>().sprite = white_queen; player = "white"; break;
                case "white_knight": this.GetComponent<SpriteRenderer>().sprite = white_knight; player = "white"; break;
                case "white_bishop": this.GetComponent<SpriteRenderer>().sprite = white_bishop; player = "white"; break;
                case "white_king": this.GetComponent<SpriteRenderer>().sprite = white_king; player = "white"; break;
                case "white_rook": this.GetComponent<SpriteRenderer>().sprite = white_rook; player = "white"; break;
                case "white_elemental_bishop": this.GetComponent<SpriteRenderer>().sprite = white_elemental_bishop; player = "white"; break;

                case "tile_lava": this.GetComponent<SpriteRenderer>().sprite = tile_lava; player = "neutral"; break;
                case "tile_ice": this.GetComponent<SpriteRenderer>().sprite = tile_ice; break;
                case "tile_earth": this.GetComponent<SpriteRenderer>().sprite = tile_earth; break;
            }
        }

        // Example for white king
        if (this.name == "white_king")
        {
            statusManager.AddStatus(StatusType.Invulnerable, 10); // invulnerable until end of turn 10
            isInvulnerable = true;
            invulnerableUntilTurn = 10;
            Debug.Log($"{name} is invulnerable until turn {invulnerableUntilTurn}");
        }
        else if (this.name == "black_king")
        {
            statusManager.AddStatus(StatusType.Invulnerable, 10); // invulnerable until end of turn 10
            isInvulnerable = true;
            invulnerableUntilTurn = 10;
            Debug.Log($"{name} is invulnerable until turn {invulnerableUntilTurn}");
        }
        else if (this.name == "tile_lava")
        {
            statusManager.AddStatus(StatusType.specialTile, 99); // special tile status
            Debug.Log($"{name} is a special tile.");

        }
        else if (this.name == "tile_ice")
        {
            statusManager.AddStatus(StatusType.specialTile, 99); // special tile status
            Debug.Log($"{name} is a special tile.");

        }
        else if (this.name == "tile_earth")
        {
            statusManager.AddStatus(StatusType.specialTile, 99); // special tile status
            statusManager.AddStatus(StatusType.Invulnerable,99); // immovable tile status
            Debug.Log($"{name} is a special tileeee.");

        }
       
        Debug.Log($"{name} activated at ({GetXBoard()}, {GetYBoard()})");

    }

    public void ActivateFortify()
    {
        fortifyActive = true;
        // remove old plates and show the new fortify plates immediately
        DestroyMovePlates();
        InitiateMovePlates();
    }

    // Called to clear fortify (when move completes)
    public void ClearFortify()
    {
        fortifyActive = false;
    }
    public void SetCoords()
    {
        //Get the board value in order to convert to xy coords
        float x = xBoard;
        float y = yBoard;

        //Adjust by variable offset
        x *= 0.66f;
        y *= 0.66f;

        //Add constants (pos 0,0)
        x += -2.3f;
        y += -2.3f;

        //Set actual unity values
        this.transform.position = new Vector3(x, y, -1.0f);
    }

    public int GetXBoard()
    {
        return xBoard;
    }

    public int GetYBoard()
    {
        return yBoard;
    }

    public void SetXBoard(int x)
    {
        xBoard = x;
    }

    public void SetYBoard(int y)
    {
        yBoard = y;
    }
    // Add to Chessman.cs (anywhere inside the class)
    public string GetPlayer()
    {
        return player;
    }

    private void OnMouseUp()
    {
        // Hide all panels first (safe check)
        if (UIManager.Instance != null)
        {
            UIManager.Instance.pawnPanel?.SetActive(false);
            UIManager.Instance.knightPanel?.SetActive(false);
            UIManager.Instance.bishopPanel?.SetActive(false);
            UIManager.Instance.rookPanel?.SetActive(false);
            UIManager.Instance.queenPanel?.SetActive(false);
            UIManager.Instance.kingPanel?.SetActive(false);
            UIManager.Instance.whiteElementalBishopPanel?.SetActive(false);
        }

        // Get reference to Game controller
        var game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        string currentPlayer = game.GetCurrentPlayer();

        // ✅ Check turn before proceeding
        bool isWhitePiece = name.StartsWith("white");
        bool isBlackPiece = name.StartsWith("black");

        if ((currentPlayer == "white" && !isWhitePiece) ||
            (currentPlayer == "black" && !isBlackPiece))
        {
            Debug.Log($"It's {currentPlayer}'s turn. {name} cannot move!");
            return; // ❌ Stop here - don't show panel or move plates
        }

        // ✅ Select the correct panel for this piece
        if (UIManager.Instance != null)
        {
            if (name.Contains("pawn"))
                panelForThisPiece = UIManager.Instance.pawnPanel;
            else if (name.Contains("elemental_bishop"))
                panelForThisPiece = UIManager.Instance.whiteElementalBishopPanel;
            else if (name.Contains("knight"))
                panelForThisPiece = UIManager.Instance.knightPanel;
            else if (name.Contains("bishop"))
                panelForThisPiece = UIManager.Instance.bishopPanel;
            else if (name.Contains("rook"))
                panelForThisPiece = UIManager.Instance.rookPanel;
            else if (name.Contains("queen"))
                panelForThisPiece = UIManager.Instance.queenPanel;
            else if (name.Contains("king"))
                panelForThisPiece = UIManager.Instance.kingPanel;
            
        }
        // store selected piece for UI buttons
        if (UIManager.Instance != null) UIManager.Instance.selectedPiece = this.gameObject;

        panelForThisPiece?.SetActive(true);

        // ✅ Remove old move plates
        DestroyMovePlates();

        // ✅ Create new move plates (only if it's this piece's turn)
        InitiateMovePlates();
        CheckMoveTiles_Start();
    }





    public void RecalculatePanel()
    {
        panelForThisPiece = UIManager.Instance.GetPanelForPieceName(name);
    }



    public void DestroyMovePlates()
    {
        //Destroy old MovePlates
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        for (int i = 0; i < movePlates.Length; i++)
        {
            Destroy(movePlates[i]); //Be careful with this function "Destroy" it is asynchronous
        }
    }

    public virtual void InitiateMovePlates()
    {

        if (this.name.StartsWith("black_pawn"))
        {
            PawnMovePlate(xBoard, yBoard - 1);
            if (yBoard == 6 && controller.GetComponent<Game>().GetPosition(xBoard, yBoard - 1) == null && controller.GetComponent<Game>().GetPosition(xBoard, yBoard - 2) == null)
            {
                Game sc = controller.GetComponent<Game>();
                if (sc.PositionOnBoard(xBoard, yBoard - 2) && sc.GetPosition(xBoard, yBoard - 2) == null)
                {
                    MovePlateSpawn(xBoard, yBoard - 2);
                }
            }
        }//black pawn end

        else if (this.name.StartsWith("white_pawn"))
        {
            PawnMovePlate(xBoard, yBoard + 1);
            if (yBoard == 1 && controller.GetComponent<Game>().GetPosition(xBoard, yBoard + 1) == null && controller.GetComponent<Game>().GetPosition(xBoard, yBoard + 2) == null)
            {
                Game sc = controller.GetComponent<Game>();
                if (sc.PositionOnBoard(xBoard, yBoard + 2) && sc.GetPosition(xBoard, yBoard + 2) == null)
                {
                    MovePlateSpawn(xBoard, yBoard + 2);
                }
            }
        }//white pawn end

        else
        {
            switch (this.name)
            {
                case "black_rook":
                case "white_rook":
                    if (fortifyActive)
                        SurroundMovePlate();
                    else
                    {
                        LineMovePlate(1, 0); LineMovePlate(-1, 0); LineMovePlate(0, 1); LineMovePlate(0, -1);
                    }
                    break;

                case "black_knight": LMovePlate(); break;
                case "white_knight": LMovePlate(); break;
                case "black_bishop": LineMovePlate(1, 1); LineMovePlate(-1, -1); LineMovePlate(-1, 1); LineMovePlate(1, -1); break;
                case "white_bishop": LineMovePlate(1, 1); LineMovePlate(-1, -1); LineMovePlate(-1, 1); LineMovePlate(1, -1); break;
                case "white_elemental_bishop": LineMovePlate(1, 1); LineMovePlate(-1, -1); LineMovePlate(-1, 1); LineMovePlate(1, -1); break;
                case "black_queen":
                    LineMovePlate(1, 0); LineMovePlate(-1, 0); LineMovePlate(0, 1); LineMovePlate(0, -1);
                    LineMovePlate(1, 1); LineMovePlate(-1, -1); LineMovePlate(-1, 1); LineMovePlate(1, -1); break;
                case "white_queen":
                    LineMovePlate(1, 0); LineMovePlate(-1, 0); LineMovePlate(0, 1); LineMovePlate(0, -1);
                    LineMovePlate(1, 1); LineMovePlate(-1, -1); LineMovePlate(-1, 1); LineMovePlate(1, -1); break;
                case "black_king": SurroundMovePlate(); break;
                case "white_king": SurroundMovePlate(); break;
            }
        }


    }

   public void LineMovePlate(int xIncrement, int yIncrement)
{
    Game sc = controller.GetComponent<Game>();

    int x = xBoard + xIncrement;
    int y = yBoard + yIncrement;

    while (sc.PositionOnBoard(x, y))
    {
        GameObject target = sc.GetPosition(x, y);

        // Check for special tile first
        if (target != null && target.GetComponent<StatusManager>()?.HasStatus(StatusType.specialTile, sc.turns) == true)
        {
            Debug.Log($"{target.name} is a special tile. Landing allowed, passing through.");
            MovePlateSpawn(x, y); // can land
            x += xIncrement;
            y += yIncrement;
            continue; // allow passing through
        }

        // If there is a piece
        if (target != null)
        {
            Chessman targetCm = target.GetComponent<Chessman>();
            if (targetCm != null)
            {
                if (targetCm.statusManager.HasStatus(StatusType.Invulnerable, sc.turns))
                {
                    Debug.Log($"{targetCm.name} is invulnerable. Skipping attack.");
                    break;
                }

                if (targetCm.player != player && !targetCm.isInvulnerable)
                {
                    Debug.Log($"{targetCm.name} is enemy. MovePlateAttackSpawn activated.");
                    MovePlateAttackSpawn(x, y);
                }
                else
                {
                    Debug.Log($"{targetCm.name} is friendly. Cannot move there.");
                }

                break; // stop after hitting a piece
            }
        }
        else
        {
            // Empty tile
            Debug.Log($"Empty tile at ({x},{y}). MovePlateSpawn activated.");
            MovePlateSpawn(x, y);
        }

        x += xIncrement;
        y += yIncrement;
    }
}



    public void LMovePlate()
    {
        PointMovePlate(xBoard + 1, yBoard + 2);
        PointMovePlate(xBoard - 1, yBoard + 2);
        PointMovePlate(xBoard + 2, yBoard + 1);
        PointMovePlate(xBoard + 2, yBoard - 1);
        PointMovePlate(xBoard + 1, yBoard - 2);
        PointMovePlate(xBoard - 1, yBoard - 2);
        PointMovePlate(xBoard - 2, yBoard + 1);
        PointMovePlate(xBoard - 2, yBoard - 1);
    }

    public void SurroundMovePlate()
    {
        PointMovePlate(xBoard, yBoard + 1);
        PointMovePlate(xBoard, yBoard - 1);

        PointMovePlate(xBoard - 1, yBoard + 0);
        PointMovePlate(xBoard - 1, yBoard - 1);
        PointMovePlate(xBoard - 1, yBoard + 1);
        PointMovePlate(xBoard + 1, yBoard + 0);
        PointMovePlate(xBoard + 1, yBoard - 1);
        PointMovePlate(xBoard + 1, yBoard + 1);
    }

    public void PointMovePlate(int x, int y)
{
    Game sc = controller.GetComponent<Game>();
    if (!sc.PositionOnBoard(x, y)) return;

    GameObject cp = sc.GetPosition(x, y);

    // Check special tile first
    if (cp != null && cp.GetComponent<StatusManager>()?.HasStatus(StatusType.specialTile, sc.turns) == true)
    {
        Debug.Log($"{cp.name} is a special tile. Landing allowed.");
        MovePlateSpawn(x, y);
        return; // do not attack
    }

    if (cp == null)
    {
        Debug.Log($"Empty tile at ({x},{y}). MovePlateSpawn activated.");
        MovePlateSpawn(x, y);
    }
    else
    {
        Chessman targetCm = cp.GetComponent<Chessman>();
        if (targetCm != null)
        {
            if (targetCm.statusManager.HasStatus(StatusType.Invulnerable, sc.turns))
            {
                Debug.Log($"{targetCm.name} is invulnerable. Skipping attack.");
                return;
            }

            if (targetCm.player != player && !targetCm.isInvulnerable)
            {
                Debug.Log($"{targetCm.name} is enemy. MovePlateAttackSpawn activated.");
                MovePlateAttackSpawn(x, y);
            }
            else
            {
                Debug.Log($"{targetCm.name} is friendly. Cannot move there.");
            }
        }
    }
}



    public void PawnMovePlate(int x, int y)
{
    Game sc = controller.GetComponent<Game>();
    if (!sc.PositionOnBoard(x, y)) return;

    // ---- Forward Move ----
    int forwardTiles = (yBoard == 1 && player == "white") || (yBoard == 6 && player == "black") ? 2 : 1;

    int stepY = player == "white" ? 1 : -1;
    int currentY = yBoard;

    for (int i = 1; i <= forwardTiles; i++)
    {
        currentY += stepY;
        if (!sc.PositionOnBoard(xBoard, currentY)) break;

        GameObject cp = sc.GetPosition(xBoard, currentY);

        // ✅ Check special tile first
        if (cp != null && cp.GetComponent<StatusManager>()?.HasStatus(StatusType.specialTile, sc.turns) == true)
        {
            Debug.Log($"{cp.name} is a special tile. Pawn can land here and keep checking next tile.");
            MovePlateSpawn(xBoard, currentY); // land here
            continue; // keep checking next tile for 2-tile move
        }

        if (cp == null)
        {
            Debug.Log($"Empty tile at ({xBoard},{currentY}). MovePlateSpawn activated for pawn forward.");
            MovePlateSpawn(xBoard, currentY);
        }
        else
        {
            Debug.Log($"{cp.name} is blocking pawn forward movement. Stop.");
            break; // stop if we hit a piece
        }
    }

    // ---- Diagonal Attacks ----
    int[] dx = { 1, -1 };
    foreach (int offset in dx)
    {
        int tx = xBoard + offset;
        int ty = yBoard + stepY;

        if (!sc.PositionOnBoard(tx, ty)) continue;

        GameObject target = sc.GetPosition(tx, ty);

        // Special tile diagonally → skip attack
        if (target != null && target.GetComponent<StatusManager>()?.HasStatus(StatusType.specialTile, sc.turns) == true)
        {
            Debug.Log($"{target.name} is a special tile on diagonal. Pawn cannot attack, skip.");
            continue;
        }

        if (target != null)
        {
            Chessman targetCm = target.GetComponent<Chessman>();
            if (targetCm != null)
            {
                if (targetCm.statusManager.HasStatus(StatusType.Invulnerable, sc.turns))
                {
                    Debug.Log($"{targetCm.name} is invulnerable. Skipping pawn attack.");
                    continue;
                }

                if (targetCm.player != player)
                {
                    Debug.Log($"{targetCm.name} is enemy. MovePlateAttackSpawn activated for pawn diagonal.");
                    MovePlateAttackSpawn(tx, ty);
                }
            }
        }
    }
}




    public void MovePlateSpawn(int matrixX, int matrixY)
    {
        //Get the board value in order to convert to xy coords
        float x = matrixX;
        float y = matrixY;

        //Adjust by variable offset
        x *= 0.66f;
        y *= 0.66f;

        //Add constants (pos 0,0)
        x += -2.3f;
        y += -2.3f;

        //Set actual unity values
        GameObject mp = Instantiate(movePlate, new Vector3(x, y, -3.0f), Quaternion.identity);

        MovePlate mpScript = mp.GetComponent<MovePlate>();
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(matrixX, matrixY);
    }

    public void MovePlateAttackSpawn(int matrixX, int matrixY)
    {
        //Get the board value in order to convert to xy coords
        float x = matrixX;
        float y = matrixY;

        //Adjust by variable offset
        x *= 0.66f;
        y *= 0.66f;

        //Add constants (pos 0,0)
        x += -2.3f;
        y += -2.3f;

        //Set actual unity values
        GameObject mp = Instantiate(movePlate, new Vector3(x, y, -3.0f), Quaternion.identity);

        MovePlate mpScript = mp.GetComponent<MovePlate>();
        mpScript.attack = true;
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(matrixX, matrixY);
    }
    
    
    public void SetPlayer(string p)
{
    player = p;
}



    


    
}
