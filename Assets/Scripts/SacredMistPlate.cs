using UnityEngine;

public class SacredMistPlate : MonoBehaviour
{
    private Game game;
    private int x, y;
    private string targetPieceName;
    private RoyalKnight royalKnight;

    public void Setup(Game g, int tileX, int tileY, string pieceName, RoyalKnight rk)
    {
        game = g;
        x = tileX;
        y = tileY;
        targetPieceName = pieceName;
        royalKnight = rk;
    }

    private void OnMouseUp()
    {
        if (game == null || royalKnight == null)
        {
            Debug.LogError("[SacredMistPlate] Missing references!");
            return;
        }

        Debug.Log($"[SacredMistPlate] Target selected: {targetPieceName} at ({x},{y})");

        // Apply Phantom Guard buff to the target piece
        royalKnight.ApplyPhantomGuardBuff(x, y, targetPieceName);

        // Hide UI panels
        HideAllUIPanels();
    }

    private void HideAllUIPanels()
    {
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
            UIManager.Instance.whiteRoyalKnightPanel?.SetActive(false);
            UIManager.Instance.whiteRoyalPawnPanel?.SetActive(false);
            UIManager.Instance.whiteSpectralHeraldPanel?.SetActive(false);
            UIManager.Instance.whiteChronomagusPanel?.SetActive(false);
            
            // Hide status panel when hiding all panels
            UIManager.Instance.HideStatusPanel();
            
            // Clear selected piece
            UIManager.Instance.selectedPiece = null;
        }
        if (SkillManagerTMP.Instance != null)
        {
            SkillManagerTMP.Instance.skillPanel?.SetActive(false);
        }
    }
}
