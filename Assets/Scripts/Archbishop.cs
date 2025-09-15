using UnityEngine;

public class Archbishop : MonoBehaviour
{
    private Game game;
    private static bool temporalShiftUsed = false; // ✅ shared flag for whole battle

    private void Start()
    {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
    }

    public void TemporalShiftButton()
    {
        // ✅ Prevent multiple uses per battle
        if (temporalShiftUsed)
        {
            Debug.LogWarning("[Archbishop] Temporal Shift already used this battle!");
            return;
        }

        // ✅ Deduct SP
        if (!SkillManager.Instance.SpendPlayerSP("white", 2))
        {
            Debug.LogWarning("[Archbishop] Not enough SP to use Temporal Shift!");
            return;
        }

        temporalShiftUsed = true; // ✅ Mark as used
        Debug.Log("[Archbishop] Temporal Shift activated by white!");

        game.SetPlayerRestriction("black", 1);

        foreach (GameObject plate in GameObject.FindGameObjectsWithTag("MovePlate"))
            Destroy(plate);

        game.NextTurn();
    }
}
