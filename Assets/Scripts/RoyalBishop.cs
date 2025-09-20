using UnityEngine;

public class RoyalBishop : MonoBehaviour
{
    public GameObject movePlatePrefab;
    private Game game;

    private void Start()
    {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
    }

    // Royal Bishop skills can be added here in the future
    // For now, it uses the same movement as a regular Bishop
    // but with enhanced abilities that can be implemented later
}
