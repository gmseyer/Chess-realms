using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TurnUI : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI turnText;
    public AudioSource audioSource; // optional
    public AudioClip turnSound; // optional

    [Header("Animation Settings")]
    public float popScale = 1.3f; // how much it grows on turn
    public float popDuration = 0.3f; // duration of pop animation

    [Header("Flavor Texts")]
    [TextArea(2, 5)]
    public string[] flavorTexts = new string[]
    {
        "The battlefield grows tense...",
        "Darkness spreads across the field!",
        "Allies ready for battle!",
        "Enemies lurk in the shadows...",
        "Fate hangs in the balance!"
    };

    private int currentTurn = 0;
    private Vector3 originalScale;

    private void Awake()
    {
        if(turnText != null)
            originalScale = turnText.transform.localScale;
    }

    /// <summary>
    /// Call this whenever the turn changes
    /// </summary>
    public void UpdateTurn(int turnNumber)
    {
        currentTurn = turnNumber;

        if (turnText != null)
        {
            // Update text with flavor text
            string flavor = flavorTexts.Length > 0 ? flavorTexts[turnNumber % flavorTexts.Length] : "";
            turnText.text = $"Turn: {turnNumber}\n{flavor}";

            // Change color for fun (even turns green, odd yellow)
            turnText.color = (turnNumber % 2 == 0) ? Color.green : Color.yellow;

            // Animate pop
            StopAllCoroutines();
            StartCoroutine(PopAnimation());
        }

        // Play sound effect if assigned
        if(audioSource != null && turnSound != null)
        {
            audioSource.PlayOneShot(turnSound);
        }
    }

    private System.Collections.IEnumerator PopAnimation()
    {
        float timer = 0f;
        Vector3 targetScale = originalScale * popScale;

        // Scale up
        while(timer < popDuration / 2f)
        {
            timer += Time.deltaTime;
            turnText.transform.localScale = Vector3.Lerp(originalScale, targetScale, timer / (popDuration/2f));
            yield return null;
        }

        // Scale down
        timer = 0f;
        while(timer < popDuration / 2f)
        {
            timer += Time.deltaTime;
            turnText.transform.localScale = Vector3.Lerp(targetScale, originalScale, timer / (popDuration/2f));
            yield return null;
        }

        turnText.transform.localScale = originalScale;
    }
}
