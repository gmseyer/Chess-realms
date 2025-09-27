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
    public GameObject whiteWraithPawnPanel;
    public GameObject whiteRoyalPawnPanel;
    public GameObject whiteSpectralHeraldPanel;
    public GameObject whiteChronomagusPanel;
    public GameObject blackChronomagusPanel;
    public GameObject whiteRoyalKnightPanel;
    public GameObject whiteMistKnightPanel;

    // Status Panel
    public GameObject statusPanel;
    public Transform statusIconParent; // Parent object to hold status sprites

    // Status Sprites
    public Sprite invulnerableSprite;
    public Sprite summonedSprite;
    public Sprite phaseSprite;
    public Sprite lockedSprite;
    public Sprite stunnedSprite;
    public Sprite etherealSprite;
    public Sprite soulbrandSprite;
    public Sprite bountySprite;
    public Sprite kingMovementSprite;
    public Sprite specialTileSprite;
    public Sprite solidBlockSprite;




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
        if (pieceName.Contains("chronomagus")) return whiteChronomagusPanel;
        if (pieceName.Contains("royal_knight")) return whiteRoyalKnightPanel;
        if (pieceName.Contains("spectral_herald")) return whiteSpectralHeraldPanel;
        if (pieceName.Contains("royal_pawn")) return whiteRoyalPawnPanel;
        if (pieceName.Contains("mist_knight")) return whiteMistKnightPanel;
        
        
        

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

    // Status Panel Management
    public void UpdateStatusPanel()
    {
        // Clear existing status icons
        ClearStatusIcons();

        // If no piece selected, don't update (panel visibility is controlled by piece panels)
        if (selectedPiece == null)
        {
            return;
        }

        // Get the selected piece's status manager
        Chessman chessman = selectedPiece.GetComponent<Chessman>();
        if (chessman == null || chessman.statusManager == null)
        {
            return;
        }

        // Get current turn
        Game game = GameObject.FindGameObjectWithTag("GameController")?.GetComponent<Game>();
        if (game == null)
        {
            return;
        }

        // Check each status type and create icons for active ones
        StatusType[] allStatusTypes = {
            StatusType.Invulnerable, StatusType.Summoned, StatusType.Phase, StatusType.Locked,
            StatusType.Stunned, StatusType.Ethereal, StatusType.Soulbrand, StatusType.Bounty,
            StatusType.KingMovement, StatusType.specialTile, StatusType.SolidBlock
        };

        foreach (StatusType statusType in allStatusTypes)
        {
            if (chessman.statusManager.HasStatus(statusType, game.turns))
            {
                CreateStatusIcon(statusType);
            }
        }
    }

    private void ClearStatusIcons()
    {
        if (statusIconParent == null) return;

        // Destroy all existing status icons
        foreach (Transform child in statusIconParent)
        {
            if (child != null)
                Destroy(child.gameObject);
        }
    }

    private void CreateStatusIcon(StatusType statusType)
    {
        if (statusIconParent == null) return;

        // Get the appropriate sprite for this status
        Sprite statusSprite = GetStatusSprite(statusType);
        if (statusSprite == null) return;

        // Create a new GameObject for the status icon
        GameObject statusIcon = new GameObject($"StatusIcon_{statusType}");
        statusIcon.transform.SetParent(statusIconParent, false);

        // Add Image component
        Image image = statusIcon.AddComponent<Image>();
        image.sprite = statusSprite;
        image.preserveAspect = true;

        // Set size (you can adjust this as needed)
        RectTransform rectTransform = statusIcon.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(50, 50); // 50x50 pixels
    }

    private Sprite GetStatusSprite(StatusType statusType)
    {
        switch (statusType)
        {
            case StatusType.Invulnerable: return invulnerableSprite;
            case StatusType.Summoned: return summonedSprite;
            case StatusType.Phase: return phaseSprite;
            case StatusType.Locked: return lockedSprite;
            case StatusType.Stunned: return stunnedSprite;
            case StatusType.Ethereal: return etherealSprite;
            case StatusType.Soulbrand: return soulbrandSprite;
            case StatusType.Bounty: return bountySprite;
            case StatusType.KingMovement: return kingMovementSprite;
            case StatusType.specialTile: return specialTileSprite;
            case StatusType.SolidBlock: return solidBlockSprite;
            default: return null;
        }
    }

    // Helper method to show status panel (called when any piece panel opens)
    public void ShowStatusPanel()
    {
        if (statusPanel != null)
        {
            statusPanel.SetActive(true);
            UpdateStatusPanel(); // Update the status icons
        }
    }

    // Helper method to hide status panel (called when any piece panel closes)
    public void HideStatusPanel()
    {
        if (statusPanel != null)
        {
            statusPanel.SetActive(false);
        }
    }

    // Test method to manually add statuses for testing
    [ContextMenu("Test Status Panel")]
    public void TestStatusPanel()
    {
        if (selectedPiece == null)
        {
            Debug.Log("[Status Panel Test] No piece selected. Please select a piece first.");
            return;
        }

        Chessman chessman = selectedPiece.GetComponent<Chessman>();
        if (chessman == null || chessman.statusManager == null)
        {
            Debug.Log("[Status Panel Test] No Chessman or StatusManager found on selected piece.");
            return;
        }

        Game game = GameObject.FindGameObjectWithTag("GameController")?.GetComponent<Game>();
        if (game == null)
        {
            Debug.Log("[Status Panel Test] Game controller not found.");
            return;
        }

        // Add some test statuses
        chessman.statusManager.AddStatus(StatusType.Invulnerable, game.turns + 5);
        chessman.statusManager.AddStatus(StatusType.Stunned, game.turns + 3);
        chessman.statusManager.AddBountyStatus(2, game.turns + 10);
        
        Debug.Log("[Status Panel Test] Added test statuses to selected piece. Check the status panel!");
    }


}
