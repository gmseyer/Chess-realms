using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//cd /c/Users/acer/Downloads/Chess_App-master/Chess_App-master
 
public class Chessman : MonoBehaviour
{
    //References 
    public GameObject controller;
    public GameObject movePlate;
    private GameObject panelForThisPiece;

    // Add these fields inside the Chessman class
    private bool wasAttack = false;
    private string lastMoveNotation = "";
 
    // Position for this Chesspiece on the Board
    protected int xBoard = -1;
    protected int yBoard = -1;

    //Variable for keeping track of the player it belongs to "black" or "white"
    protected string player;

    //Normal Pieces
    public Sprite black_queen, black_knight, black_bishop, black_king, black_rook, black_pawn;
    public Sprite white_queen, white_knight, white_bishop, white_king, white_rook, white_pawn;

    //summoned units
    public Sprite white_elemental_bishop;
    public Sprite white_arch_bishop;

    //Royal Units
    public Sprite white_royal_pawn;
    public Sprite white_royal_rook;
    public Sprite white_royal_bishop;

    //Elemental Tiles
    public Sprite tile_lava;
    public Sprite tile_ice;
    public Sprite tile_earth;
    public Sprite celestial_pillar;

    [HideInInspector] public bool fortifyActive = false; 
    [HideInInspector] public bool isInvulnerable = false;        
    [HideInInspector] public int invulnerableUntilTurn = -1;      // inclusive turn when it expires
    [HideInInspector] public StatusManager statusManager;
    [HideInInspector] public Color originalColor; // Store original color for stunned pieces


    [Header("Skill Points")]
    public int skillPoints = 4; // not working
    private int lastX;
    private int lastY;


    //********************TEST FUNCTIONS********************

        // Add this right after the using statements at the top of Chessman.cs
public static class ChessNotation
{
    private static string[] files = { "a", "b", "c", "d", "e", "f", "g", "h" };
    private static string[] ranks = { "1", "2", "3", "4", "5", "6", "7", "8" };
    
   public static string BoardToNotation(int x, int y)
    {
        if (x < 0 || x > 7 || y < 0 || y > 7) return "invalid";
        return files[x] + ranks[y];
    }
    
    public static string GetPieceNotation(string pieceName)
    {
        if (pieceName.Contains("elemental_bishop")) return "EB";
        if (pieceName.Contains("royal_bishop")) return "RB";
        if (pieceName.Contains("royal_rook")) return "RR";
        if (pieceName.Contains("arch")) return "AB";
        if (pieceName.Contains("pawn")) return "P";
        if (pieceName.Contains("knight")) return "N";
        if (pieceName.Contains("bishop")) return "B";
        if (pieceName.Contains("rook")) return "R";
        if (pieceName.Contains("queen")) return "Q";
        if (pieceName.Contains("king")) return "K";
        
        return "?";
    }
}













     private void Awake()
    {
        statusManager = gameObject.AddComponent<StatusManager>();
    }

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



    // ********************BOARD FUNCTIONS********************
    public void UpdateVisualStatus()
    {
        Game game = controller?.GetComponent<Game>();
        if (game == null) return;
        bool isStunned = statusManager.HasStatus(StatusType.Stunned, game.turns);
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        bool isTemporalShifted = game.IsPlayerRestrictedToPawns(player) && !name.Contains("pawn");
        bool isEthereal = statusManager.HasStatus(StatusType.Ethereal, game.turns);
        
        if (sr != null)
        {
           if (isStunned)
{
    // Store original color if not already stored
    if (originalColor == Color.clear)
        originalColor = sr.color;
    // Set to yellow
    sr.color = Color.magenta;
}
else if (isEthereal)
{
    // Store original color if not already stored
    if (originalColor == Color.clear)
        originalColor = sr.color;
    // Set to green for Ethereal
    sr.color = Color.green;
}
else if (isTemporalShifted)
{
    // Store original color if not already stored
    if (originalColor == Color.clear)
        originalColor = sr.color;
    // Set to magenta for Temporal Shift
    sr.color = Color.magenta;
}
else
{
    // Restore original color when effects end
    if (originalColor != Color.clear)
        sr.color = originalColor;
}

        }

       
    }


    public void CheckMoveTiles_Start()
    {
        lastX = GetXBoard();
        lastY = GetYBoard();
         wasAttack = false; // Reset attack flag
        Debug.Log($"{name} START position: ({lastX}, {lastY})");
    }

    public void CheckMoveTiles_End()
{
    int newX = GetXBoard();
    int newY = GetYBoard();

    Debug.Log($"{name} END position: ({newX}, {newY})");

    if (newX != lastX || newY != lastY)
    {
        string fromSquare = ChessNotation.BoardToNotation(lastX, lastY);
        string toSquare = ChessNotation.BoardToNotation(newX, newY);
        string pieceType = ChessNotation.GetPieceNotation(name);
        
        // Generate basic notation
        string notation = $"{pieceType}{fromSquare} to {toSquare}";
        lastMoveNotation = notation;
        Game game = controller?.GetComponent<Game>();
if (game != null)
{
    game.AddMoveToHistory(notation);
}

        Debug.Log($"{name} MOVED: {notation}");
    }
    else
    {
        Debug.Log($"{name} DID NOT MOVE");
    }
    // Add this line in CheckMoveTiles_End() after generating notation

}

    public void SetCoords() //new constants for 1440 x 3040 resolution, positioning of pieces on the board
    {
        //Get the board value in order to convert to xy coords
        float x = xBoard;
        float y = yBoard;

        //Adjust by variable offset
        x *= 0.57f;
        y *= 0.56f;

        //Add constants (pos 0,0)
        x += -1.99f;
        y += -1.94f; 


        
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

    // IMPORTANT FUNCTION
    public string GetPlayer()
    {
        return player;
    }

     public void SetPlayer(string p)
    {
        player = p;
    }

    public void RecalculatePanel()
    {
        panelForThisPiece = UIManager.Instance.GetPanelForPieceName(name);
    }


    // ********************PIECE SPECIFIC FUNCTIONS********************
   
    public void ActivateFortify() 
    {
        fortifyActive = true;
        DestroyMovePlates();
        InitiateMovePlates();
    }
    public void ClearFortify()
    {
        fortifyActive = false;
    }




    //******************* LOGIC FUNCTIONS********************

    public void Activate() 
    {
        controller = GameObject.FindGameObjectWithTag("GameController");
        SetCoords(); // setting position of pieces on board

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

                //Summoned Units
                case "white_elemental_bishop": this.GetComponent<SpriteRenderer>().sprite = white_elemental_bishop; player = "white"; break;
                case "white_arch_bishop": this.GetComponent<SpriteRenderer>().sprite = white_arch_bishop; player = "white"; break;
                //Royal Units
                case "white_royal_pawn": this.GetComponent<SpriteRenderer>().sprite = white_royal_pawn; player = "white"; break;
                case "white_royal_rook": this.GetComponent<SpriteRenderer>().sprite = white_royal_rook; player = "white"; break;
                case "white_royal_bishop": this.GetComponent<SpriteRenderer>().sprite = white_royal_bishop; player = "white"; break;
                //Elemental Tiles
                case "tile_lava": this.GetComponent<SpriteRenderer>().sprite = tile_lava; player = "neutral"; break;
                case "tile_ice": this.GetComponent<SpriteRenderer>().sprite = tile_ice; break;
                case "tile_earth": this.GetComponent<SpriteRenderer>().sprite = tile_earth; player = "neutral"; break;
                case "celestial_pillar": this.GetComponent<SpriteRenderer>().sprite = celestial_pillar; player = "neutral"; break;
            }
        }

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
            statusManager.AddStatus(StatusType.Invulnerable, 99); // immovable tile status
            Debug.Log($"{name} is a special tileeee.");

        }
        UpdateVisualStatus();
    }


    private void OnMouseUp() //on click panels
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
            UIManager.Instance.whiteArchBishopPanel?.SetActive(false);
            UIManager.Instance.whiteRoyalRookPanel?.SetActive(false);
            UIManager.Instance.whiteRoyalBishopPanel?.SetActive(false);
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
            else if (name.Contains("arch_bishop"))
                panelForThisPiece = UIManager.Instance.whiteArchBishopPanel;
            else if (name.Contains("royal_rook"))
                panelForThisPiece = UIManager.Instance.whiteRoyalRookPanel;
            else if (name.Contains("royal_bishop"))
                panelForThisPiece = UIManager.Instance.whiteRoyalBishopPanel;
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

        DestroyMovePlates(); // ✅ Remove old move plates
        InitiateMovePlates(); // ✅ Create new move plates (only if it's this piece's turn)
        CheckMoveTiles_Start();
    }


    public virtual void InitiateMovePlates()
    {
        Game game = controller.GetComponent<Game>();

        if (game.IsPlayerRestrictedToPawns(player))
        {
            if (!name.Contains("pawn"))
            {
                Debug.Log($"[TemporalShift] {name} cannot move this turn.");
                return; // no move plates
            }
        }
        // In the InitiateMovePlates() method, add this check after the turn check:

        // Check if piece is stunned
        if (statusManager.HasStatus(StatusType.Stunned, game.turns))
        {
            Debug.Log($"[Stunned] {name} is stunned and cannot move this turn.");
            return; // no move plates
        }

        if (this.name.StartsWith("black_pawn"))
        {
            PawnMovePlate(xBoard, yBoard - 1);
            if (yBoard == 6 && game.GetPosition(xBoard, yBoard - 1) == null && game.GetPosition(xBoard, yBoard - 2) == null)
            {
                if (game.PositionOnBoard(xBoard, yBoard - 2) && game.GetPosition(xBoard, yBoard - 2) == null)
                {
                    MovePlateSpawn(xBoard, yBoard - 2);
                }
            }
        }

        else if (this.name.StartsWith("white_pawn"))
        {
            PawnMovePlate(xBoard, yBoard + 1);
            if (yBoard == 1 && game.GetPosition(xBoard, yBoard + 1) == null && game.GetPosition(xBoard, yBoard + 2) == null)
            {
                if (game.PositionOnBoard(xBoard, yBoard + 2) && game.GetPosition(xBoard, yBoard + 2) == null)
                {
                    MovePlateSpawn(xBoard, yBoard + 2);
                }
            }
        }

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
                case "white_royal_rook":
                    if (fortifyActive)
                        SurroundMovePlate();
                    else
                    {
                        // Check for Celestial Synergy passive skill
                        RoyalRook royalRook = GetComponent<RoyalRook>();
                        if (royalRook != null && royalRook.CheckCelestialSynergy())
                        {
                            // Queen-like movement when Celestial Synergy is active
                            royalRook.GenerateCelestialSynergyMovePlates();
                        }
                        else
                        {
                            // Normal Rook movement
                            LineMovePlate(1, 0); LineMovePlate(-1, 0); LineMovePlate(0, 1); LineMovePlate(0, -1);
                        }
                    }
                    break;

                case "black_knight": LMovePlate(); break;
                case "white_knight": LMovePlate(); break;
                case "black_bishop": LineMovePlate(1, 1); LineMovePlate(-1, -1); LineMovePlate(-1, 1); LineMovePlate(1, -1); break;
                case "white_bishop": 
                    // Check for Ethereal status
                    if (statusManager.HasStatus(StatusType.Ethereal, game.turns))
                    {
                        // Use ethereal movement (can pass through any piece)
                        Bishop bishop = GetComponent<Bishop>();
                        if (bishop != null)
                        {
                            bishop.GenerateEtherealMovePlates();
                        }
                        else
                        {
                            // Fallback to normal movement if Bishop component not found
                            LineMovePlate(1, 1); LineMovePlate(-1, -1); LineMovePlate(-1, 1); LineMovePlate(1, -1);
                        }
                    }
                    else
                    {
                        // Normal Bishop movement
                        LineMovePlate(1, 1); LineMovePlate(-1, -1); LineMovePlate(-1, 1); LineMovePlate(1, -1);
                    }
                    break;
                case "white_elemental_bishop": LineMovePlate(1, 1); LineMovePlate(-1, -1); LineMovePlate(-1, 1); LineMovePlate(1, -1); break;
                case "white_arch_bishop": LineMovePlate(1, 1); LineMovePlate(-1, -1); LineMovePlate(-1, 1); LineMovePlate(1, -1); break;
                case "white_royal_bishop": LineMovePlate(1, 1); LineMovePlate(-1, -1); LineMovePlate(-1, 1); LineMovePlate(1, -1); break;
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
    } // END OF INITIATEMOVEPLATES


    public void DestroyMovePlates()
    {   UpdateVisualStatus();
        //Destroy old MovePlates
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        for (int i = 0; i < movePlates.Length; i++)
        {
            Destroy(movePlates[i]); //Be careful with this function "Destroy" it is asynchronous
        }
    }

    // ******************** MOVEMENT LOGIC FUNCTIONS********************
    public void LineMovePlate(int xIncrement, int yIncrement)
    {
        Game sc = controller.GetComponent<Game>();

        int x = xBoard + xIncrement;
        int y = yBoard + yIncrement;

        while (sc.PositionOnBoard(x, y))
        {
            GameObject target = sc.GetPosition(x, y);

            if (target != null)
            {
                Chessman targetCm = target.GetComponent<Chessman>();
                if (targetCm != null)
                {
                    // Treat tile_earth as solid/invulnerable (except for Elemental Bishop)
                    if (targetCm.name == "tile_earth")
                    {
                        // Check if this is an Elemental Bishop (can pass through boulders)
                        if (this.name == "white_elemental_bishop" || name.Contains("king"))
                        {
                            Debug.Log($"{this.name} can pass through {targetCm.name}. Continuing movement.");
                            x += xIncrement;
                            y += yIncrement;
                            continue; // pass through and continue
                        }
                        else
                        {
                            Debug.Log($"{targetCm.name} is a solid block. Cannot pass or land.");
                            break; // stop movement
                        }
                    }

                    // Special tile like lava/ice: can land and pass
                    if (targetCm.statusManager.HasStatus(StatusType.specialTile, sc.turns))
                    {
                        Debug.Log($"{targetCm.name} is a special tile. Landing allowed, passing through.");
                        MovePlateSpawn(x, y); // can land
                        x += xIncrement;
                        y += yIncrement;
                        continue;
                    }

                    // Regular invulnerable piece
                    if (targetCm.statusManager.HasStatus(StatusType.Invulnerable, sc.turns))
                    {
                        Debug.Log($"{targetCm.name} is invulnerable. Skipping attack.");
                        break;
                    }

                    // Enemy piece
                    if (targetCm.player != player && !targetCm.isInvulnerable)
                    {
                        Debug.Log($"{targetCm.name} is enemy. MovePlateAttackSpawn activated.");
                        MovePlateAttackSpawn(x, y);
                    }
                    else
                    {
                        Debug.Log($"{targetCm.name} is friendly. Cannot move there.");
                    }

                    break; // stop after hitting any piece
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

    public void LunarLeapMovePlate()
    {
        int x = xBoard;
        int y = yBoard;

        // Top rank
        PointMovePlate(x - 1, y + 2);
        PointMovePlate(x, y + 2);
        PointMovePlate(x + 1, y + 2);

        // Top middle
        for (int i = -2; i <= 2; i++)
            PointMovePlate(x + i, y + 1);

        // Center rank
        for (int i = -2; i <= 2; i++)
            PointMovePlate(x + i, y);

        // Lower middle
        for (int i = -2; i <= 2; i++)
            PointMovePlate(x + i, y - 1);

        // Lower rank
        PointMovePlate(x - 1, y - 2);
        PointMovePlate(x, y - 2);
        PointMovePlate(x + 1, y - 2);
    }


    public void PointMovePlate(int x, int y)
    {
        Game sc = controller.GetComponent<Game>();
        if (!sc.PositionOnBoard(x, y)) return;

        GameObject cp = sc.GetPosition(x, y);

        if (cp != null)
        {
            Chessman targetCm = cp.GetComponent<Chessman>();
            if (targetCm != null)
            {
                // Check for tile_earth → solid block (except for Elemental Bishop)
                if (targetCm.name == "tile_earth")
                {
                    // Check if this is an Elemental Bishop (can pass through boulders)
                    if (this.name == "white_elemental_bishop" || this.name == "white_king" || this.name == "black_king")
                    {
                        Debug.Log($"{this.name} can pass through {targetCm.name}. Continuing movement.");
                        return; // pass through but don't land
                    }
                    else
                    {
                        Debug.Log($"{targetCm.name} is a solid block. Cannot move here.");
                        return; // cannot land or pass
                    }
                }

                // Special tile like lava/ice → can land
                if (targetCm.statusManager.HasStatus(StatusType.specialTile, sc.turns))
                {
                    Debug.Log($"{targetCm.name} is a special tile. Landing allowed.");
                    MovePlateSpawn(x, y);
                    return;
                }

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

                return;
            }
        }

        // Empty tile
        //  Debug.Log($"Empty tile at ({x},{y}). MovePlateSpawn activated.");
        MovePlateSpawn(x, y);
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

            if (cp != null)
            {
                Chessman targetCm = cp.GetComponent<Chessman>(); 
                if (targetCm != null)
                {
                    // tile_earth → block movement (except for Elemental Bishop)
                    if (targetCm.name == "tile_earth")
                    {
                        // Check if this is an Elemental Bishop (can pass through boulders)
                        if (this.name == "white_elemental_bishop" || this.name == "white_king" || this.name == "black_king")
                        {
                            Debug.Log($"{this.name} can pass through {targetCm.name}. Continuing movement.");
                            continue; // pass through and continue
                        }
                        else
                        {
                            Debug.Log($"{targetCm.name} is a solid block. Pawn cannot move forward.");
                            break; // stop movement
                        }
                    }

                    // Special tile → can land and continue checking
                    if (targetCm.statusManager.HasStatus(StatusType.specialTile, sc.turns))
                    {
                        Debug.Log($"{targetCm.name} is a special tile. Pawn can land here.");
                        MovePlateSpawn(xBoard, currentY);
                        continue;
                    }

                    // Other pieces
                    Debug.Log($"{targetCm.name} is blocking pawn forward movement. Stop.");
                    break;
                }
            }
            else
            {
                Debug.Log($"Empty tile at ({xBoard},{currentY}). MovePlateSpawn activated for pawn forward.");
                MovePlateSpawn(xBoard, currentY);
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

            if (target != null)
            {
                Chessman targetCm = target.GetComponent<Chessman>();
                if (targetCm != null)
                {
                    // tile_earth → cannot attack (except for Elemental Bishop)
                    if (targetCm.name == "tile_earth")
                    {
                        // Check if this is an Elemental Bishop (can pass through boulders)
                        if (this.name == "white_elemental_bishop" || this.name == "white_king" || this.name == "black_king")
                        {
                            Debug.Log($"{this.name} can pass through {targetCm.name}. Continuing movement.");
                            continue; // pass through and continue
                        }
                        else
                        {
                            Debug.Log($"{targetCm.name} is a solid block. Pawn cannot attack.");
                            continue;
                        }
                    }

                    // Special tile → skip attack
                    if (targetCm.statusManager.HasStatus(StatusType.specialTile, sc.turns))
                    {
                        Debug.Log($"{targetCm.name} is a special tile on diagonal. Pawn cannot attack, skip.");
                        continue;
                    }

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


        x *= 0.57f;
        y *= 0.56f;

        //Add constants (pos 0,0)
        x += -1.98f;
        y += -1.95f; 

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
         x *= 0.57f;
        y *= 0.56f;

        //Add constants (pos 0,0)
        x += -1.98f;
        y += -1.95f; 

        //Set actual unity values
        GameObject mp = Instantiate(movePlate, new Vector3(x, y, -3.0f), Quaternion.identity);

        MovePlate mpScript = mp.GetComponent<MovePlate>();
        mpScript.attack = true;
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(matrixX, matrixY);
    }

}
