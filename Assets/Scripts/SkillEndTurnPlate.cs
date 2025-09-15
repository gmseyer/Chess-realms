using UnityEngine;

public class SkillEndTurnPlate : MonoBehaviour
{
    // FOR ELEMENTAL BISHOP SKILL TILES
    private Game game;
    private int x, y;
    private string tileName;

    public void Setup(Game g, int tileX, int tileY, string name)
    {
        game = g;
        x = tileX;
        y = tileY;
        tileName = name;
    }

    private void OnMouseUp()
    {
        Debug.Log($"[Skill] Tile clicked at ({x},{y}) to spawn {tileName}");

       if (!string.IsNullOrEmpty(tileName))
{
    GameObject newTile = game.Create(tileName, x, y);
    ElementalBishop eb = FindObjectOfType<ElementalBishop>();
    if (eb != null)
        eb.RegisterTile(newTile);
}


        if (UIManager.Instance != null)
        {

            UIManager.Instance.whiteElementalBishopPanel?.SetActive(false);
        }
        game.NextTurn();

        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);
    }
}


//WORK IN PROGRESS - DO NOT DELETE
// ADDING DURATION FOR ELEMENTAL TILES
// HEHE
