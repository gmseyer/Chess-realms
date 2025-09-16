using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TurnUI : MonoBehaviour
{
    public static TurnUI Instance; // Singleton reference

    [Header("References")]
    public TextMeshProUGUI turnText;
    public AudioSource audioSource; // optional
    public AudioClip turnSound; // optional

    [Header("Animation Settings")]
    public float popScale = 1.3f;
    public float popDuration = 0.3f;

    [Header("UI Background")]
    public Image turnBackground; // Assign a panel/image behind the turn text in inspector
    public Color whiteTurnColor = new Color(1f, 1f, 1f, 0.2f); // semi-transparent white
    public Color blackTurnColor = new Color(0f, 0f, 0f, 0.2f); // semi-transparent black

    private int currentTurn = 0;
    private Vector3 originalScale;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // only one instance allowed
            return;
        }
        Instance = this;

        if (turnText != null)
            originalScale = turnText.transform.localScale;
    }

    /// <summary>
    /// Call this whenever the turn changes
    /// </summary>
    /// <param name="turnNumber">Current turn number</param>
    /// <param name="currentPlayer">"White" or "Black"</param>
    public void UpdateTurn(int turnNumber, string currentPlayer)
    {
        currentTurn = turnNumber;

        if (turnText != null)
        {
            // Show turn number and current player
            turnText.text = $"TURN: {turnNumber} - {currentPlayer}";

            // Color: White = light gray, Black = dark gray
            turnText.color = currentPlayer.ToLower() == "white" ? Color.white : Color.black;
            if (turnBackground != null)
            {
                turnBackground.color = currentPlayer.ToLower() == "white" ? whiteTurnColor : blackTurnColor;
            }

            // Animate
            StopAllCoroutines();
            StartCoroutine(PopAnimation());
        }

        // Play sound if assigned
        if (audioSource != null && turnSound != null)
        {
            audioSource.PlayOneShot(turnSound);
        }
    }

    private System.Collections.IEnumerator PopAnimation()
    {
        float timer = 0f;
        Vector3 targetScale = originalScale * popScale;

        // Scale up
        while (timer < popDuration / 2f)
        {
            timer += Time.deltaTime;
            turnText.transform.localScale = Vector3.Lerp(originalScale, targetScale, timer / (popDuration / 2f));
            yield return null;
        }

        // Scale down
        timer = 0f;
        while (timer < popDuration / 2f)
        {
            timer += Time.deltaTime;
            turnText.transform.localScale = Vector3.Lerp(targetScale, originalScale, timer / (popDuration / 2f));
            yield return null;
        }

        turnText.transform.localScale = originalScale;
    }
    

    








}
