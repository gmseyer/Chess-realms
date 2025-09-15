using UnityEngine;

public class RookSkillButton : MonoBehaviour
{
    // Hook this to the UI button OnClick()
    public void OnClickRoyalCastlingButton()
    {
        if (UIManager.Instance == null)
        {
            Debug.LogError("[RookSkillButton] UIManager.Instance is null.");
            return;
        }

        GameObject selected = UIManager.Instance.selectedPiece;
        if (selected == null)
        {
            Debug.LogWarning("[RookSkillButton] No piece selected. Select a rook first.");
            return;
        }

        if (!selected.name.ToLower().Contains("rook"))
        {
            Debug.LogWarning($"[RookSkillButton] Selected piece '{selected.name}' is not a rook.");
            return;
        }

        // ensure Rook component exists (attach if missing)
        Rook rookScript = selected.GetComponent<Rook>();
        if (rookScript == null)
        {
            rookScript = selected.AddComponent<Rook>();
            Debug.Log($"[RookSkillButton] Rook.cs attached at runtime to {selected.name}");
        }

        rookScript.AttemptRoyalCastling();
    }
}
