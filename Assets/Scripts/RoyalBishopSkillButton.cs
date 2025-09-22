using UnityEngine;

public class RoyalBishopSkillButton : MonoBehaviour
{
    // Hook this to the UI button OnClick() for SoulRequiem
    public void OnClickSoulRequiemButton()
    {
        if (UIManager.Instance == null)
        {
            Debug.LogError("[RoyalBishopSkillButton] UIManager.Instance is null.");
            return;
        }

        GameObject selected = UIManager.Instance.selectedPiece;
        if (selected == null)
        {
            Debug.LogWarning("[RoyalBishopSkillButton] No piece selected. Select a Royal Bishop first.");
            return;
        }

        // Check if selected piece is a Royal Bishop
        if (!selected.name.ToLower().Contains("royal_bishop"))
        {
            Debug.LogWarning($"[RoyalBishopSkillButton] Selected piece '{selected.name}' is not a Royal Bishop.");
            return;
        }

        // Get the Royal Bishop component from the selected piece
        RoyalBishop royalBishopScript = selected.GetComponent<RoyalBishop>();
        if (royalBishopScript == null)
        {
            Debug.LogError($"[RoyalBishopSkillButton] RoyalBishop component not found on {selected.name}!");
            return;
        }

        // Call the SoulRequiem method on the selected Royal Bishop
        royalBishopScript.SoulRequiem();
    }

    // Hook this to the UI button OnClick() for SanctifiedRuin
    public void OnClickSanctifiedRuinButton()
    {
        if (UIManager.Instance == null)
        {
            Debug.LogError("[RoyalBishopSkillButton] UIManager.Instance is null.");
            return;
        }

        GameObject selected = UIManager.Instance.selectedPiece;
        if (selected == null)
        {
            Debug.LogWarning("[RoyalBishopSkillButton] No piece selected. Select a Royal Bishop first.");
            return;
        }

        // Check if selected piece is a Royal Bishop
        if (!selected.name.ToLower().Contains("royal_bishop"))
        {
            Debug.LogWarning($"[RoyalBishopSkillButton] Selected piece '{selected.name}' is not a Royal Bishop.");
            return;
        }

        // Get the Royal Bishop component from the selected piece
        RoyalBishop royalBishopScript = selected.GetComponent<RoyalBishop>();
        if (royalBishopScript == null)
        {
            Debug.LogError($"[RoyalBishopSkillButton] RoyalBishop component not found on {selected.name}!");
            return;
        }

        // Call the SanctifiedRuin method on the selected Royal Bishop
        royalBishopScript.SanctifiedRuin();
    }
}
