using UnityEngine;

public class AltarSummonPlate : MonoBehaviour
{
    private int x, y;

    public void Setup(int tileX, int tileY)
    {
        x = tileX;
        y = tileY;
    }

    private void OnMouseUp()
    {
        Debug.Log($"[AltarSummonPlate] Altar summon plate clicked at ({x},{y})!");
        
        // Create altar at this position
        FireBishop.CreateAltar(x, y);
        
        // Destroy all altar summon plates
        AltarSummonPlate[] altarPlates = FindObjectsOfType<AltarSummonPlate>();
        foreach (AltarSummonPlate plate in altarPlates)
        {
            if (plate != null && plate.gameObject != null)
                Destroy(plate.gameObject);
        }
        
        // Hide UI panels
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
            UIManager.Instance.whiteIceBishopPanel?.SetActive(false);
            UIManager.Instance.whiteEarthBishopPanel?.SetActive(false);
            UIManager.Instance.whiteFireBishopPanel?.SetActive(false);
            
            // Hide status panel when hiding all panels
            UIManager.Instance.HideStatusPanel();
            
            // Clear selected piece
            UIManager.Instance.selectedPiece = null;
        }
        if (SkillManagerTMP.Instance != null)
        {
            SkillManagerTMP.Instance.skillPanel?.SetActive(false);
        }
        
        // End turn
        GameObject controller = GameObject.FindGameObjectWithTag("GameController");
        if (controller != null)
        {
            controller.GetComponent<Game>().NextTurn();
        }
    }
}
