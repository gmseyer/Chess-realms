using UnityEngine;

public class ChronomagusSkillButton : MonoBehaviour
{
    // Hook this to the UI button OnClick() for Chronomagus
    public void OnClickChronomagusButton()
    {
        if (UIManager.Instance == null)
        {
            Debug.LogError("[ChronomagusSkillButton] UIManager.Instance is null.");
            return;
        } 

        GameObject selected = UIManager.Instance.selectedPiece;
        if (selected == null)
        {
            Debug.LogWarning("[ChronomagusSkillButton] No piece selected. Select a piece first.");
            return;
        }

        // Get the current player
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        string currentPlayer = game.GetCurrentPlayer();

        // Check if Chronomagus promotion is available for current player
        if (!Chronomagus.IsChronomagusAvailable(currentPlayer))
        {
            Debug.LogWarning($"[ChronomagusSkillButton] Chronomagus promotion not available for {currentPlayer} player!");
            return;
        }

        // Find any Chronomagus component to call the promotion
        Chronomagus chronomagus = FindObjectOfType<Chronomagus>();
        if (chronomagus == null)
        {
            Debug.LogError("[ChronomagusSkillButton] No Chronomagus component found!");
            return;
        }

        // Call the Chronomagus promotion
        chronomagus.ChronomagusPromotion();
    }
}
