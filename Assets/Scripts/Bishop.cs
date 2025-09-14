using UnityEngine;

public class Bishop : MonoBehaviour
{
    public GameObject movePlatePrefab; 

    
    public void OnBishopButtonClick() //divine offering, cant change for some reason hehehhe
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();

        // ✅ Destroy old move plates first
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // ✅ Loop all tiles and spawn plates on empty ones
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (game.GetPosition(x, y) == null)
                {
                    SpawnTile(game, x, y);
                }
            }
        }
    }

    private void SpawnTile(Game game, int x, int y)
    {
        float fx = x * 0.66f + -2.3f;
        float fy = y * 0.66f + -2.3f;

        GameObject mp = Instantiate(movePlatePrefab, new Vector3(fx, fy, -3f), Quaternion.identity);

        MovePlate oldScript = mp.GetComponent<MovePlate>();
        if (oldScript != null) Destroy(oldScript);

        mp.AddComponent<EndTurnPlate>().Setup(game, x, y); // ✅ pass x,y
    }

     public void HealingBenediction()
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();

        // ✅ Destroy old move plates first
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // ✅ Hardcode white starting positions (only pawn, rook, knight)
        Vector2Int[] whiteStartPositions = new Vector2Int[]
        {
            new Vector2Int(0, 0), // rook
            new Vector2Int(7, 0), // rook
            new Vector2Int(1, 0), // knight
            new Vector2Int(6, 0), // knight
            new Vector2Int(0, 1), // pawns
            new Vector2Int(1, 1),
            new Vector2Int(2, 1),
            new Vector2Int(3, 1),
            new Vector2Int(4, 1),
            new Vector2Int(5, 1),
            new Vector2Int(6, 1),
            new Vector2Int(7, 1)
        };

        foreach (Vector2Int pos in whiteStartPositions)
        {
            if (game.GetPosition(pos.x, pos.y) == null) // ✅ Only empty tiles
            {
                SpawnHealingPlate(game, pos.x, pos.y);
            }
        }

        Debug.Log("[HealingBenediction] Spawn plates generated on empty white spawn tiles.");
    }

    private void SpawnHealingPlate(Game game, int x, int y)
    {
        float fx = x * 0.66f + -2.3f;
        float fy = y * 0.66f + -2.3f;

        GameObject mp = Instantiate(movePlatePrefab, new Vector3(fx, fy, -3f), Quaternion.identity);

        MovePlate oldScript = mp.GetComponent<MovePlate>();
        if (oldScript != null) Destroy(oldScript);

        // ✅ Add our custom plate for revival (we’ll add this later)
        mp.AddComponent<HealingBenedictionPlate>().Setup(game, x, y);
    }




}
