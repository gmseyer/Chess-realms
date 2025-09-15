using UnityEngine;

public class Bishop : MonoBehaviour
{
    public GameObject movePlatePrefab; 
     public bool hasUsedHealingBenediction = false; // ✅ Once-per-battle mark
    
    public void OnBishopButtonClick() //divine offering, cant change for some reason hehehhe
    {
        
        
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();

        // ✅ Destroy old move plates first
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // ✅ Loop all tiles and spawn plates on empty ones
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 4; y++)
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
        Debug.Log($"[HealingBenediction] Attempting activation... HasUsed? {hasUsedHealingBenediction}");

        if (hasUsedHealingBenediction)
        {
            Debug.Log("[HealingBenediction] Already used — skill blocked.");
            return;
        }

        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();

        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        Vector2Int[] whiteStartPositions = new Vector2Int[]
        {
            new Vector2Int(0, 0), new Vector2Int(7, 0),
            new Vector2Int(1, 0), new Vector2Int(6, 0),
            new Vector2Int(0, 1), new Vector2Int(1, 1),
            new Vector2Int(2, 1), new Vector2Int(3, 1),
            new Vector2Int(4, 1), new Vector2Int(5, 1),
            new Vector2Int(6, 1), new Vector2Int(7, 1)
        };

        int platesSpawned = 0;
        foreach (Vector2Int pos in whiteStartPositions)
        {
            if (game.GetPosition(pos.x, pos.y) == null)
            {
                SpawnHealingPlate(game, pos.x, pos.y);
                platesSpawned++;
            }
        }

        Debug.Log($"[HealingBenediction] Spawned {platesSpawned} revival plates.");
    }

    private void SpawnHealingPlate(Game game, int x, int y)
    {
        float fx = x * 0.66f + -2.3f;
        float fy = y * 0.66f + -2.3f;

        GameObject mp = Instantiate(movePlatePrefab, new Vector3(fx, fy, -3f), Quaternion.identity);

        MovePlate oldScript = mp.GetComponent<MovePlate>();
        if (oldScript != null) Destroy(oldScript);

        mp.AddComponent<HealingBenedictionPlate>().Setup(game, x, y);
    }


    //test
    public void TestHealingBenedictionWithSP()
{
    string player = "white"; // Bishop is always white in this test

    // 1️⃣ Check if skill is on cooldown
    if (SkillManager.Instance.IsSkillOnCooldown(player, SkillType.HealingBenediction))
    {
        Debug.LogWarning("[HealingBenediction] Skill is on cooldown — cannot use.");
        return;
    }

    // 2️⃣ Try spend SP
    if (!SkillManager.Instance.SpendPlayerSP(player, 1))
    {
        Debug.LogWarning("[HealingBenediction] Not enough SP to cast.");
        return;
    }

    // 3️⃣ Activate skill (your existing logic)
    HealingBenediction();

    // 4️⃣ Start cooldown (e.g. 3 turns)
    SkillManager.Instance.StartCooldown(player, SkillType.HealingBenediction, 3);

    Debug.Log("[HealingBenediction] Skill activated successfully!");
}


}
