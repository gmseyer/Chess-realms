using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public GameObject pawnPanel;
    public GameObject knightPanel;
    public GameObject bishopPanel;
    public GameObject rookPanel;
    public GameObject queenPanel;
    public GameObject kingPanel;

    public GameObject whiteElementalBishopPanel;
    public GameObject whiteArchBishopPanel;
    public GameObject whiteRoyalRookPanel;
    public GameObject whiteRoyalBishopPanel;




    // UI text that shows the current player's SP (assign this in the Inspector)
    public TMP_Text rookSPText;


// Optional: reference to the Fortify button so you can enable/disable it
public Button fortifyButton;

    
    [HideInInspector] public GameObject selectedPiece;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public GameObject GetPanelForPieceName(string pieceName)
    {
        if (pieceName.Contains("elemental_bishop")) return whiteElementalBishopPanel; // for Divine Offering
        if (pieceName.Contains("arch_bishop")) return whiteArchBishopPanel; // for Arch Bishop
        if (pieceName.Contains("royal_rook")) return whiteRoyalRookPanel; // for Royal Rook
        if (pieceName.StartsWith("white_pawn") || pieceName.StartsWith("black_pawn")) return pawnPanel;
        if (pieceName.Contains("royal_bishop")) return whiteRoyalBishopPanel; // for Royal Bishop
        if (pieceName.Contains("knight")) return knightPanel;
        if (pieceName.Contains("white_bishop")) return bishopPanel;
        if (pieceName.Contains("black_bishop")) return bishopPanel;
        if (pieceName.Contains("rook")) return rookPanel;
        if (pieceName.Contains("queen")) return queenPanel;
        if (pieceName.Contains("king")) return kingPanel;
        
        

        return null;
    }


    // call this from your Fortify button OnClick()

   

    



    public void UpdateSkillPointDisplay()
    {
        // find game controller
        Game game = GameObject.FindGameObjectWithTag("GameController")?.GetComponent<Game>();
        if (game == null)
        {
            if (rookSPText != null) rookSPText.text = "";
            return;
        }

        // If no selected piece, show current player's SP (optional) or blank
        if (selectedPiece == null)
        {
            if (rookSPText != null)
            {
                // Show SP for the player whose turn it is
                string cur = game.GetCurrentPlayer();
                int sp = game.GetPlayerSP(cur);
                rookSPText.text = cur + " SP: " + sp;
            }
            return;
        }

        // If selected piece exists, show the SP for the piece's owner (use Game)
        Chessman cm = selectedPiece.GetComponent<Chessman>();
        if (cm == null)
        {
            if (rookSPText != null) rookSPText.text = "";
            return;
        }

        // show the SP of the piece's owner (useful when selecting enemy piece maybe)
        string owner = cm.GetPlayer();
        int ownerSP = game.GetPlayerSP(owner);

        if (selectedPiece.name.Contains("rook") && rookSPText != null)
        {
            rookSPText.text = owner + " SP: " + ownerSP;
        }
        else
        {
            // optional: show current player's SP instead when non-rook selected
            // rookSPText.text = game.GetPlayerSP(game.GetCurrentPlayer()).ToString();
            if (rookSPText != null) rookSPText.text = owner + " SP: " + ownerSP;
        }
    }


}
