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
    private bool hasMoved = false; // Track if piece has moved (for castling)
 
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
    public Sprite white_wraith_pawn;

    public Sprite white_spectral_herald;
    public Sprite black_spectral_herald;

    public Sprite white_ice_bishop;
    public Sprite white_earth_bishop;
    public Sprite white_fire_bishop;
    

    //Royal Units
    public Sprite white_royal_pawn;
    public Sprite white_royal_rook;
    public Sprite white_royal_bishop;
    public Sprite white_royal_knight;
    public Sprite white_mist_knight;
    

    public Sprite white_chronomagus;
    public Sprite black_chronomagus;

    public Sprite black_royal_pawn;

    //Elemental Tiles
    public Sprite tile_lava;
    public Sprite tile_ice;
    public Sprite tile_earth;
    public Sprite tile_thunder;
    public Sprite tile_void;
    
    public Sprite celestial_pillar;

    public Sprite tile_sanctuary;

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
        if (pieceName.Contains("chronomagus")) return "CM";
        if(pieceName.Contains("royal_knight")) return "RK";
        if (pieceName.Contains("royal_pawn")) return "RP";
        if (pieceName.Contains("spectral_herald")) return "SH";
        if (pieceName.Contains("elemental_bishop")) return "EB";
        if (pieceName.Contains("ice_bishop")) return "EIB";
        if (pieceName.Contains("earth_bishop")) return "EEB";
        if (pieceName.Contains("fire_bishop")) return "EFB";
        if (pieceName.Contains("royal_bishop")) return "RB";
        if (pieceName.Contains("royal_rook")) return "RR";
        if (pieceName.Contains("wraith_pawn")) return "WP";
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
        bool isFrozen = statusManager.HasStatus(StatusType.Frozen, game.turns);
        bool isCrippled = statusManager.HasStatus(StatusType.Crippled, game.turns);
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        bool isTemporalShifted = game.IsPlayerRestrictedToPawns(player) && !name.Contains("pawn");
        bool isEthereal = statusManager.HasStatus(StatusType.Ethereal, game.turns);
        bool hasBounty = statusManager.HasBounty(game.turns);
        bool hasKingMovement = statusManager.HasStatus(StatusType.KingMovement, game.turns);
        
        if (sr != null)
        {
           if (isFrozen)
{
    // Store original color if not already stored
    if (originalColor == Color.clear)
        originalColor = sr.color;
    // Set to blue for Frozen
    sr.color = Color.blue;
}
           else if (isCrippled)
{
    // Store original color if not already stored
    if (originalColor == Color.clear)
        originalColor = sr.color;
    // Set to orange for Crippled
    sr.color = new Color(1.0f, 0.5f, 0.0f, 1.0f); // Orange
}
           else if (isStunned)
{
    // Store original color if not already stored
    if (originalColor == Color.clear)
        originalColor = sr.color;
    // Set to magenta for Stunned
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
        else if (hasBounty)
{
    // Store original color if not already stored
    if (originalColor == Color.clear)
        originalColor = sr.color;
    // Set to yellow for Bounty
    sr.color = Color.yellow;
}
else if (hasKingMovement)
{
    // Store original color if not already stored
    if (originalColor == Color.clear)
        originalColor = sr.color;
    // Set to cyan for King Movement
    sr.color = Color.cyan;
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

    public bool GetHasMoved()
    {
        return hasMoved;
    }

    public void SetHasMoved(bool moved)
    {
        hasMoved = moved;
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
                case "white_ice_bishop": this.GetComponent<SpriteRenderer>().sprite = white_ice_bishop; player = "white"; break;
                case "white_earth_bishop": this.GetComponent<SpriteRenderer>().sprite = white_earth_bishop; player = "white"; break;
                case "white_fire_bishop": this.GetComponent<SpriteRenderer>().sprite = white_fire_bishop; player = "white"; break;
                case "white_arch_bishop": this.GetComponent<SpriteRenderer>().sprite = white_arch_bishop; player = "white"; break;
                case "white_wraith_pawn": this.GetComponent<SpriteRenderer>().sprite = white_wraith_pawn; player = "white"; break;
                case "white_spectral_herald": this.GetComponent<SpriteRenderer>().sprite = white_spectral_herald; player = "white"; break;
                case "black_spectral_herald": this.GetComponent<SpriteRenderer>().sprite = black_spectral_herald; player = "black"; break;
                //Royal Units
                case "white_royal_pawn": this.GetComponent<SpriteRenderer>().sprite = white_royal_pawn; player = "white"; break;
                case "white_royal_rook": this.GetComponent<SpriteRenderer>().sprite = white_royal_rook; player = "white"; break;
                case "white_royal_bishop": this.GetComponent<SpriteRenderer>().sprite = white_royal_bishop; player = "white"; break;
                case "white_royal_knight": this.GetComponent<SpriteRenderer>().sprite = white_royal_knight; player = "white"; break;
                case "white_mist_knight": this.GetComponent<SpriteRenderer>().sprite = white_mist_knight; player = "white"; break;
                case "black_royal_pawn": this.GetComponent<SpriteRenderer>().sprite = black_royal_pawn; player = "black"; break;
                case "white_chronomagus": this.GetComponent<SpriteRenderer>().sprite = white_chronomagus; player = "white"; break;
                case "black_chronomagus": this.GetComponent<SpriteRenderer>().sprite = black_chronomagus; player = "black"; break;
                //Elemental Tiles
                case "tile_lava": this.GetComponent<SpriteRenderer>().sprite = tile_lava; player = "neutral"; break;
                case "tile_ice": this.GetComponent<SpriteRenderer>().sprite = tile_ice; break;
                case "tile_earth": this.GetComponent<SpriteRenderer>().sprite = tile_earth; player = "neutral"; break;
                case "tile_thunder": this.GetComponent<SpriteRenderer>().sprite = tile_thunder; player = "neutral"; break;
                case "tile_void": this.GetComponent<SpriteRenderer>().sprite = tile_void; player = "neutral"; break;
                case "tile_sanctuary": this.GetComponent<SpriteRenderer>().sprite = tile_sanctuary; player = "neutral"; break;
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
        else if (this.name == "tile_thunder")
        {
            statusManager.AddStatus(StatusType.specialTile, 99); // special tile status
            Debug.Log($"{name} is a special tile.");
        }
        else if (this.name == "tile_void")
        {
            statusManager.AddStatus(StatusType.specialTile, 999); // permanent special tile status
            Debug.Log($"{name} is a void tile - destroys any piece that enters or passes through.");
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
            UIManager.Instance.whiteWraithPawnPanel?.SetActive(false);
            UIManager.Instance.whiteRoyalPawnPanel?.SetActive(false);
            UIManager.Instance.whiteSpectralHeraldPanel?.SetActive(false);
            UIManager.Instance.whiteChronomagusPanel?.SetActive(false);
            UIManager.Instance.whiteRoyalKnightPanel?.SetActive(false);
            UIManager.Instance.whiteMistKnightPanel?.SetActive(false);
            UIManager.Instance.whiteIceBishopPanel?.SetActive(false);
            UIManager.Instance.whiteEarthBishopPanel?.SetActive(false);
            UIManager.Instance.whiteFireBishopPanel?.SetActive(false);
            
            // Hide status panel when hiding all panels
            UIManager.Instance.HideStatusPanel();
        }

        // Get reference to Game controller
        var game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        string currentPlayer = game.GetCurrentPlayer();

        // ✅ Select the correct panel for this piece (show panels regardless of turn)
        if (UIManager.Instance != null)
        {
            if (name.Contains("wraith_pawn"))
                panelForThisPiece = UIManager.Instance.whiteWraithPawnPanel;
            else if (name.Contains("chronomagus"))
            {
                if (name.Contains("white"))
                    panelForThisPiece = UIManager.Instance.whiteChronomagusPanel;
                else if (name.Contains("black"))
                    panelForThisPiece = UIManager.Instance.blackChronomagusPanel;
            }
            else if (name.Contains("royal_knight"))
                panelForThisPiece = UIManager.Instance.whiteRoyalKnightPanel;
            else if (name.Contains("mist_knight"))
                panelForThisPiece = UIManager.Instance.whiteMistKnightPanel;
            else if (name.Contains("spectral_herald"))
                panelForThisPiece = UIManager.Instance.whiteSpectralHeraldPanel;
            else if (name.Contains("royal_pawn"))
                panelForThisPiece = UIManager.Instance.whiteRoyalPawnPanel;
            else if (name.Contains("pawn"))
                panelForThisPiece = UIManager.Instance.pawnPanel;
            else if (name.Contains("elemental_bishop") )
                panelForThisPiece = UIManager.Instance.whiteElementalBishopPanel;
            else if (name.Contains("ice_bishop"))
                panelForThisPiece = UIManager.Instance.whiteIceBishopPanel;
            else if (name.Contains("earth_bishop"))
                panelForThisPiece = UIManager.Instance.whiteEarthBishopPanel;
            else if (name.Contains("fire_bishop"))
                panelForThisPiece = UIManager.Instance.whiteFireBishopPanel;
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
        
        // store selected piece for UI buttons and show panels
        if (UIManager.Instance != null) UIManager.Instance.selectedPiece = this.gameObject;
        panelForThisPiece?.SetActive(true);
        
        // Show status panel when piece panel opens (regardless of turn)
        if (UIManager.Instance != null) UIManager.Instance.ShowStatusPanel();

        // ✅ Check turn before proceeding with movement
        bool isWhitePiece = name.StartsWith("white");
        bool isBlackPiece = name.StartsWith("black");

        if ((currentPlayer == "white" && !isWhitePiece) ||
            (currentPlayer == "black" && !isBlackPiece))
        {
            Debug.Log($"It's {currentPlayer}'s turn. {name} cannot move!");
            return; // ❌ Stop here - don't show move plates, but panels are already shown
        }

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

        // Check if piece is frozen
        if (statusManager.HasStatus(StatusType.Frozen, game.turns))
        {
            Debug.Log($"[Frozen] {name} is frozen - generating unfreeze move plate.");
            // Generate a move plate on the frozen piece's own location for unfreezing
            MovePlateSpawn(xBoard, yBoard);
            return; // Only unfreeze move plate, no other move plates
        }

        // Check if piece is crippled - pawns are immune, others handled in LineMovePlate
        if (statusManager.HasStatus(StatusType.Crippled, game.turns))
        {
            // Pawns are immune to crippled effect
            if (name.ToLower().Contains("pawn"))
            {
                Debug.Log($"[Crippled] {name} is immune to crippled effect.");
                return; // No movement plates at all
            }
            
            // For other pieces, crippled movement is handled in LineMovePlate method
            Debug.Log($"[Crippled] {name} is crippled - movement will be limited to 1 tile per direction.");
        }

        if (this.name.StartsWith("black_pawn"))
        {
            // Check for King Movement status (Russian Roulette effect)
            if (statusManager.HasStatus(StatusType.KingMovement, game.turns))
            {
                Debug.Log($"[King Movement] {this.name} gains King-style movement from Russian Roulette!");
                SurroundMovePlate(); // King-style movement (8 directions)
            }
            // Check for Radiant Presence passive
            else if (HasAlliedRoyalPawn("black"))
            {
                Debug.Log($"[Radiant Presence] {this.name} gains Crown Step - King-style movement!");
                SurroundMovePlate(); // King-style movement (8 directions)
            }
            else
            {
                // Normal pawn movement
                PawnMovePlate(xBoard, yBoard - 1);
                if (yBoard == 6 && game.GetPosition(xBoard, yBoard - 1) == null && game.GetPosition(xBoard, yBoard - 2) == null)
                {
                    if (game.PositionOnBoard(xBoard, yBoard - 2) && game.GetPosition(xBoard, yBoard - 2) == null)
                    {
                        MovePlateSpawn(xBoard, yBoard - 2);
                    }
                }
            }
        }

        else if (this.name.StartsWith("white_pawn")|| this.name.StartsWith("white_wraith_pawn"))
        {
            // Check for King Movement status (Russian Roulette effect)
            if (statusManager.HasStatus(StatusType.KingMovement, game.turns))
            {
                Debug.Log($"[King Movement] {this.name} gains King-style movement from Russian Roulette!");
                SurroundMovePlate(); // King-style movement (8 directions)
            }
            // Check for Radiant Presence passive
            else if (HasAlliedRoyalPawn("white"))
            {
                Debug.Log($"[Radiant Presence] {this.name} gains Crown Step - King-style movement!");
                SurroundMovePlate(); // King-style movement (8 directions)
            }
            else
            {
                // Normal pawn movement
                PawnMovePlate(xBoard, yBoard + 1);
                if (yBoard == 1 && game.GetPosition(xBoard, yBoard + 1) == null && game.GetPosition(xBoard, yBoard + 2) == null)
                {
                    if (game.PositionOnBoard(xBoard, yBoard + 2) && game.GetPosition(xBoard, yBoard + 2) == null)
                    {
                        MovePlateSpawn(xBoard, yBoard + 2);
                    }
                }
            }
        }
        else if (this.name.StartsWith("white_royal_pawn")){
            // Royal pawns always have Crown Step (King-style movement)
            Debug.Log($"[Radiant Presence] {this.name} has Crown Step - King-style movement!");
            SurroundMovePlate(); // King-style movement (8 directions)
        }
        else if (this.name.StartsWith("black_royal_pawn")){
            // Royal pawns always have Crown Step (King-style movement)
            Debug.Log($"[Radiant Presence] {this.name} has Crown Step - King-style movement!");
            SurroundMovePlate(); // King-style movement (8 directions)
        }

        else
        {
            switch (this.name)
            {
                case "black_rook":
                case "white_rook":
                case "white_spectral_herald":
                case "black_spectral_herald":
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
                case "white_royal_knight": LMovePlate(); break;
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
                case "white_elemental_bishop": 
                case "white_ice_bishop": 
                case "white_earth_bishop": 
                case "white_fire_bishop": LineMovePlate(1, 1); LineMovePlate(-1, -1); LineMovePlate(-1, 1); LineMovePlate(1, -1); break;
                case "white_arch_bishop": LineMovePlate(1, 1); LineMovePlate(-1, -1); LineMovePlate(-1, 1); LineMovePlate(1, -1); break;
                case "white_royal_bishop": LineMovePlate(1, 1); LineMovePlate(-1, -1); LineMovePlate(-1, 1); LineMovePlate(1, -1); break;
                case "white_chronomagus": LineMovePlate(1, 1); LineMovePlate(-1, -1); LineMovePlate(-1, 1); LineMovePlate(1, -1); break;
                case "black_chronomagus": LineMovePlate(1, 1); LineMovePlate(-1, -1); LineMovePlate(-1, 1); LineMovePlate(1, -1); break;
                case "black_queen":
                    LineMovePlate(1, 0); LineMovePlate(-1, 0); LineMovePlate(0, 1); LineMovePlate(0, -1);
                    LineMovePlate(1, 1); LineMovePlate(-1, -1); LineMovePlate(-1, 1); LineMovePlate(1, -1); break;
                case "white_queen":
                    LineMovePlate(1, 0); LineMovePlate(-1, 0); LineMovePlate(0, 1); LineMovePlate(0, -1);
                    LineMovePlate(1, 1); LineMovePlate(-1, -1); LineMovePlate(-1, 1); LineMovePlate(1, -1); break;
                case "black_king": 
                case "white_king":
                    // Use King's Last Stand passive system
                    King kingComponent = GetComponent<King>();
                    if (kingComponent != null)
                    {
                        kingComponent.ForceGenerateLastStandMovePlates();
                    }
                    else
                    {
                        // Fallback to normal movement if King component not found
                        SurroundMovePlate();
                    }
                    break;
            }
        }
    } // END OF INITIATEMOVEPLATES



    public void DestroyMovePlates()
    {   UpdateVisualStatus();
        //Destroy old MovePlates (but not Russian Roulette target plates or Royal Knight summon plates)
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        for (int i = 0; i < movePlates.Length; i++)
        {
            // Don't destroy plates that have RussianRouletteTargetPlate or RoyalKnightSummonPlate components
            if (movePlates[i] != null && 
                movePlates[i].GetComponent<RussianRouletteTargetPlate>() == null &&
                movePlates[i].GetComponent<RoyalKnightSummonPlate>() == null)
            {
                Destroy(movePlates[i]); //Be careful with this function "Destroy" it is asynchronous
            }
        }
    }

    // ******************** MOVEMENT LOGIC FUNCTIONS********************
    public void LineMovePlate(int xIncrement, int yIncrement)
    {
        Game sc = controller.GetComponent<Game>();

        int x = xBoard + xIncrement;
        int y = yBoard + yIncrement;
        
        // Check if piece is crippled - limit movement to 1 tile only
        bool isCrippled = statusManager.HasStatus(StatusType.Crippled, sc.turns);
        int maxMoves = isCrippled ? 1 : 7; // 1 tile if crippled, otherwise full board range
        int movesMade = 0;

        while (sc.PositionOnBoard(x, y) && movesMade < maxMoves)
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
                            movesMade++;
                            continue; // pass through and continue
                        }
                        else
                        {
                            Debug.Log($"{targetCm.name} is a solid block. Cannot pass or land.");
                            break; // stop movement
                        }
                    }
                    if(targetCm.name == "celestial_pillar"){
                        if(this.name == "white_chronomagus" || this.name == "black_chronomagus"){
                             x += xIncrement;
                            y += yIncrement;
                            movesMade++;
                            continue; // pass through and continue
                        }
                        else{
                            Debug.Log($"{targetCm.name} is a celestial pillar. Cannot move here.");
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
                        movesMade++;
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
            movesMade++;
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

    // Check if there's an allied royal pawn on the board (for Radiant Presence passive)
    private bool HasAlliedRoyalPawn(string player)
    {
        Chessman[] allPieces = FindObjectsOfType<Chessman>();
        
        foreach (Chessman piece in allPieces)
        {
            if (piece != null && piece.GetPlayer() == player)
            {
                string pieceName = piece.name.ToLower();
                if (pieceName.Contains("royal_pawn"))
                {
                    return true;
                }
            }
        }
        
        return false;
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
                    if (this.name == "white_elemental_bishop" || this.name == "white_king" || this.name == "black_king" || this.name == "white_chronomagus" || this.name == "black_chronomagus")
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
                        if (this.name == "white_elemental_bishop" || this.name == "white_king" || this.name == "black_king" || this.name == "white_chronomagus" || this.name == "black_chronomagus")
                        {
                          //  Debug.Log($"{this.name} can pass through {targetCm.name}. Continuing movement.");
                            continue; // pass through and continue
                        }
                        else
                        {
                           // Debug.Log($"{targetCm.name} is a solid block. Pawn cannot move forward.");
                            break; // stop movement
                        }
                    }

                    // Special tile → can land and continue checking
                    if (targetCm.statusManager.HasStatus(StatusType.specialTile, sc.turns))
                    {
                       // Debug.Log($"{targetCm.name} is a special tile. Pawn can land here.");
                        MovePlateSpawn(xBoard, currentY);
                        continue;
                    }

                    // Other pieces
                  //  Debug.Log($"{targetCm.name} is blocking pawn forward movement. Stop.");
                    break;
                }
            }
            else
            {
               // Debug.Log($"Empty tile at ({xBoard},{currentY}). MovePlateSpawn activated for pawn forward.");
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
                        if (this.name == "white_elemental_bishop" || this.name == "white_king" || this.name == "black_king" || this.name == "white_chronomagus" || this.name == "black_chronomagus")
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

    // King Movement function for Russian Roulette
    public void ActivateKingMovement()
    {
        Debug.Log($"[King Movement] {gameObject.name} activated King Movement from Russian Roulette!");
        // The status is already added in the Pawn script, this is just for activation logging
    }

    // Check if there's a void tile on the path from (fromX, fromY) to (toX, toY)
    public bool CheckVoidTileOnPath(int fromX, int fromY, int toX, int toY)
    {
        Game sc = controller.GetComponent<Game>();
        
        int deltaX = toX - fromX;
        int deltaY = toY - fromY;
        
        // Determine the direction (normalize to -1, 0, or 1)
        int stepX = deltaX == 0 ? 0 : (deltaX > 0 ? 1 : -1);
        int stepY = deltaY == 0 ? 0 : (deltaY > 0 ? 1 : -1);
        
        // Check each position along the path (including destination)
        int currentX = fromX + stepX;
        int currentY = fromY + stepY;
        
        while (sc.PositionOnBoard(currentX, currentY))
        {
            // Check if there's a void tile at this position
            GameObject pieceAtPos = sc.GetPosition(currentX, currentY);
            if (pieceAtPos != null && pieceAtPos.name == "tile_void")
            {
                Debug.Log($"[VoidTile] Void tile detected on path at ({currentX},{currentY})");
                return true; // Void tile found on path
            }
            
            // If we reached the destination, stop checking
            if (currentX == toX && currentY == toY)
                break;
                
            // Move to next position
            currentX += stepX;
            currentY += stepY;
        }
        
        return false; // No void tile found on path
    }

    // Destroy this piece due to void tile interaction
    public void DestroyByVoidTile()
    {
        Game sc = controller.GetComponent<Game>();
        
        // Clear the piece's position from the game board
        sc.SetPositionEmpty(GetXBoard(), GetYBoard());
        
        // Clean up any remaining move plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
        
        // Destroy the piece GameObject
        Debug.Log($"[VoidTile] {name} destroyed by void tile!");
        Destroy(gameObject);
    }

}
