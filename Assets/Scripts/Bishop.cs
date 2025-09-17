using UnityEngine;

public class Bishop : MonoBehaviour
{
    public GameObject movePlatePrefab;
    public GameObject elementalSummonPlatePrefab;
    public GameObject archbishopSummonPlatePrefab;
    [Header("Prefabs (Auto-Loaded)")]
    public bool hasUsedHealingBenediction = false; // Once-per-battle mark

    public int matrixX;
    public int matrixY;

    private void Awake()
    {
        // Auto-load if not assigned in Inspector
        if (elementalSummonPlatePrefab == null)
            elementalSummonPlatePrefab = Resources.Load<GameObject>("Prefabs/ElementalSummonPlate");

        if (archbishopSummonPlatePrefab == null)
            archbishopSummonPlatePrefab = Resources.Load<GameObject>("Prefabs/ArchbishopSummonPlate");

        if (elementalSummonPlatePrefab == null)
            Debug.LogError("[Bishop] Could not load ElementalSummonPlate from Resources!");

        if (archbishopSummonPlatePrefab == null)
            Debug.LogError("[Bishop] Could not load ArchbishopSummonPlate from Resources!");
    }

    public void OnBishopButtonClick()
    {
        Game game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();

        if (elementalSummonPlatePrefab == null)
            Debug.LogError("[Bishop] Elemental Summon Plate Prefab is NOT assigned!");
        if (archbishopSummonPlatePrefab == null)
            Debug.LogError("[Bishop] Archbishop Summon Plate Prefab is NOT assigned!");



        // ✅ Destroy existing plates first
        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        // ✅ Spawn ELEMENTAL BISHOP plates (bottom 3-4 ranks)
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                if (game.GetPosition(x, y) == null)
                {
                    SpawnTile(game, x, y, elementalSummonPlatePrefab, "white_elemental_bishop");
                    Debug.Log($"[DivineOffering] Spawning ELEMENTAL bishop plate at ({x},{y})");
                }
            }
        }

        // ✅ Spawn ARCHBISHOP plates (bottom 1-2 ranks)
        for (int x = 0; x < 8; x++)
        {
            for (int y = 2; y < 4; y++)
            {
                if (game.GetPosition(x, y) == null)
                {
                    SpawnTile(game, x, y, archbishopSummonPlatePrefab, "white_arch_bishop");
                    Debug.Log($"[DivineOffering] Spawning ARCHBISHOP plate at ({x},{y})");
                }
            }
        }
    }


    private void SpawnTile(Game game, int x, int y, GameObject prefab, string pieceName)
    {

        if (prefab == null)
        {
            Debug.LogError($"[Bishop] ERROR: Prefab is NULL for {pieceName} at ({x},{y})!");
            return;
        }
        float fx = x * 0.66f - 2.3f;
        float fy = y * 0.66f - 2.3f;

        GameObject mp = Instantiate(prefab, new Vector3(fx, fy, -3f), Quaternion.identity);

        MovePlate oldScript = mp.GetComponent<MovePlate>();
        if (oldScript != null) Destroy(oldScript);

        mp.AddComponent<EndTurnPlate>().Setup(game, x, y, pieceName);
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

    //START OF TESTING OF SACRIFICE
   



}
