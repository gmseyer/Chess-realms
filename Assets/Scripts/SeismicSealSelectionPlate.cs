using UnityEngine;

public class SeismicSealSelectionPlate : MonoBehaviour
{
    private int x, y;

    public void Setup(int tileX, int tileY)
    {
        x = tileX;
        y = tileY;
    }

    private void OnMouseUp()
    {
        Debug.Log($"[SeismicSealSelectionPlate] Seismic Seal target selected at ({x},{y})!");

        // Get the piece at this position
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (game == null)
        {
            Debug.LogError("[SeismicSealSelectionPlate] Could not find Game component!");
            return;
        }

        GameObject targetPiece = game.GetPosition(x, y);
        
        // Determine target type and apply appropriate effect
        if (targetPiece == null)
        {
            // Empty tile - create Terra Ward
            EarthboundBishop.CreateSeismicTerraWard(x, y);
        }
        else
        {
            Chessman targetChessman = targetPiece.GetComponent<Chessman>();
            if (targetChessman != null)
            {
                string pieceName = targetChessman.name;
                
                if (pieceName == "white_earth_bishop")
                {
                    // Self-cast - give tile_earth properties
                    EarthboundBishop.ApplySeismicSealSelfCast(targetChessman);
                }
                else if (pieceName == "tile_terra_ward")
                {
                    // Terra Ward target - 3x3 adjacent area effect
                    EarthboundBishop.ApplySeismicSealTerraWardEffect(x, y);
                }
                else
                {
                    // Regular piece - give Stone Sentinel status
                    EarthboundBishop.ApplySeismicSealStoneSentinel(targetChessman);
                }
            }
        }

        // Destroy all selection plates
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // Hide UI panels
        HideAllUIPanels();

        // End turn
        Game gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
        if (gameController != null)
        {
            gameController.NextTurn();
        }
        else
        {
            Debug.LogError("[SeismicSealSelectionPlate] Could not find Game component to end turn!");
        }
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
            UIManager.Instance.whiteIceBishopPanel?.SetActive(false);
            UIManager.Instance.whiteEarthBishopPanel?.SetActive(false);
            UIManager.Instance.whiteFireBishopPanel?.SetActive(false);
            UIManager.Instance.whiteChronomagusPanel?.SetActive(false);
            UIManager.Instance.whiteRoyalRookPanel?.SetActive(false);
            UIManager.Instance.whiteRoyalBishopPanel?.SetActive(false);
            UIManager.Instance.blackChronomagusPanel?.SetActive(false);
        }
    }
}
