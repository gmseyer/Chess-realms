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
        if (pieceName.StartsWith("white_pawn") || pieceName.StartsWith("black_pawn")) return pawnPanel;
        if (pieceName.Contains("knight")) return knightPanel;
        if (pieceName.Contains("white_bishop")) return bishopPanel;
        if (pieceName.Contains("black_bishop")) return bishopPanel;
        if (pieceName.Contains("rook")) return rookPanel;
        if (pieceName.Contains("queen")) return queenPanel;
        if (pieceName.Contains("king")) return kingPanel;
        
        

        return null;
    }


    // call this from your Fortify button OnClick()

    public void FortifySelected()
    {
        if (selectedPiece == null)
        {
            Debug.Log("FortifySelected: no piece selected.");
            return;
        }

        if (!selectedPiece.name.Contains("rook"))
        {
            Debug.Log("FortifySelected: selected piece is not a rook.");
            return;
        }

        Chessman rookCm = selectedPiece.GetComponent<Chessman>();
        if (rookCm == null)
        {
            Debug.Log("FortifySelected: selected object has no Chessman.");
            return;
        }

        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("FortifySelected: GameController not found.");
            return;
        }

        // Make sure it's the owner's turn
        string currentPlayer = game.GetCurrentPlayer();
        if (rookCm.GetPlayer() != currentPlayer)
        {
            Debug.Log($"FortifySelected: it's {currentPlayer}'s turn. Can't use {rookCm.name}.");
            return;
        }

        const int fortifyCost = 1;

        // Ask Game to spend player's SP
        bool paid = game.SpendPlayerSP(currentPlayer, fortifyCost);
        if (!paid)
        {
            Debug.Log($"{currentPlayer} does not have enough SP to use Fortify (cost {fortifyCost}).");
            return;
        }

        Debug.Log($"{currentPlayer} paid {fortifyCost} SP for Fortify. Remaining SP: {game.GetPlayerSP(currentPlayer)}");

        // APPLY EFFECT: make allied pieces around rook invulnerable
        int cx = rookCm.GetXBoard();
        int cy = rookCm.GetYBoard();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int tx = cx + dx;
                int ty = cy + dy;
                if (!game.PositionOnBoard(tx, ty)) continue;

                GameObject target = game.GetPosition(tx, ty);
                if (target == null) continue;

                Chessman targetCm = target.GetComponent<Chessman>();
                if (targetCm == null) continue;

                if (targetCm.GetPlayer() == rookCm.GetPlayer())
                {
                    targetCm.isInvulnerable = true;
                    targetCm.invulnerableUntilTurn = game.turns + 2;
                    Debug.Log($"{targetCm.name} is now invulnerable until turn {targetCm.invulnerableUntilTurn}");
                }
            }
        }

        // tidy up and end turn
        rookCm.DestroyMovePlates();
        // update the UI SP readout
        UpdateSkillPointDisplay();
        game.NextTurn();
    }

    public void RegalSafeguardSelected()
{
    if (selectedPiece == null)
    {
        Debug.Log("RegalSafeguardSelected: no piece selected.");
        return;
    }

    if (!selectedPiece.name.Contains("queen"))
    {
        Debug.Log("RegalSafeguardSelected: selected piece is not a queen.");
        return;
    }

    Queen queenScript = selectedPiece.GetComponent<Queen>();
    if (queenScript == null)
    {
        Debug.LogError("RegalSafeguardSelected: selected piece has no Queen script.");
        return;
    }

    queenScript.RegalSafeguard();

    // update SP display
    UpdateSkillPointDisplay();
}



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
