using UnityEngine;

public class BishopSkillButton : MonoBehaviour
{
    // Hook this to the UI button OnClick() for Celestial Summon
    public void OnClickCelestialSummonButton()
    {
        if (UIManager.Instance == null)
        {
            Debug.LogError("[BishopSkillButton] UIManager.Instance is null.");
            return;
        }

        GameObject selected = UIManager.Instance.selectedPiece;
        if (selected == null)
        {
            Debug.LogWarning("[BishopSkillButton] No piece selected. Select a bishop first.");
            return;
        }

        // Check if selected piece is a bishop (but not archbishop or elemental bishop)
        if (!selected.name.ToLower().Contains("bishop") || 
            selected.name.ToLower().Contains("arch") || 
            selected.name.ToLower().Contains("elemental"))
        {
            Debug.LogWarning($"[BishopSkillButton] Selected piece '{selected.name}' is not a regular bishop.");
            return;
        }

        // Get the Bishop component from the selected piece
        Bishop bishopScript = selected.GetComponent<Bishop>();
        if (bishopScript == null)
        {
            Debug.LogError($"[BishopSkillButton] Bishop component not found on {selected.name}!");
            return;
        }

        // Call the Sacrifice method on the selected bishop
        bishopScript.Sacrifice();
    }
}
