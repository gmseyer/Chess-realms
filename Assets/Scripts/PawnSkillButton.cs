using UnityEngine;

public class PawnSkillButton : MonoBehaviour
{
    // Hook this to the UI button OnClick() for Royal Acolyte promotion
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

        if (!selected.name.ToLower().Contains("pawn"))
        {
            Debug.LogWarning($"[PawnSkillButton] Selected piece '{selected.name}' is not a pawn.");
            return;
        }

        // Ensure Pawn component exists (attach if missing)
        Pawn pawnScript = selected.GetComponent<Pawn>();
        if (pawnScript == null)
        {
            pawnScript = selected.AddComponent<Pawn>();
            Debug.Log($"[PawnSkillButton] Pawn.cs attached at runtime to {selected.name}");
        }

        pawnScript.AttemptRoyalAcolytePromotion();
    }
}
