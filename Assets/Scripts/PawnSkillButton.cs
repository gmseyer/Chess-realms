using UnityEngine;

public class PawnSkillButton : MonoBehaviour
{
    // Hook this to the UI button OnClick() for RoyalAcolyte
    public void OnClickRoyalAcolyteButton()
    {
        if (UIManager.Instance == null)
        {
            Debug.LogError("[PawnSkillButton] UIManager.Instance is null.");
            return;
        }

        GameObject selected = UIManager.Instance.selectedPiece;
        if (selected == null)
        {
            Debug.LogWarning("[PawnSkillButton] No piece selected. Select a pawn first.");
            return;
        }
 
        // Check if selected piece is a pawn (but not wraith pawn)
        if (!selected.name.ToLower().Contains("pawn") || selected.name.ToLower().Contains("wraith_pawn"))
        {
            Debug.LogWarning($"[PawnSkillButton] Selected piece '{selected.name}' is not a regular pawn.");
            return;
        }

        // Get the Pawn component from the selected piece
        Pawn pawnScript = selected.GetComponent<Pawn>();
        if (pawnScript == null)
        {
            Debug.LogError($"[PawnSkillButton] Pawn component not found on {selected.name}!");
            return;
        }

        // Call the RoyalAcolyte method on the selected pawn
        pawnScript.RoyalAcolyte();
    }
}