using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.Collections.Generic;

[System.Serializable]
public class Skill
{   
   
    public string skillName = "Placeholder Skill";
    public string skillType = "Active"; // Can be Active/Passive
    public int spCost = 0;
    public float cooldown = 0f;
    public int duration = 0; // Duration in turns if applicable
    public string limitation = "None";
    public Sprite icon; // Skill icon for UI

    [TextArea(3, 10)]
    public string effect = "Effect description here";

    public UnityEvent onActivate; // Function to run on confirmation
}

public class SkillManagerTMP : MonoBehaviour
{

     public static SkillManagerTMP Instance { get; private set; }
    [Header("All Skills List")]
    public List<Skill> skills = new List<Skill>();

    [Header("UI References (TMP)")]
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI spCostText;
    public TextMeshProUGUI cooldownText;
    public TextMeshProUGUI limitationText;
    public TextMeshProUGUI effectText;
    public TextMeshProUGUI durationText;
    public TextMeshProUGUI skillTypeText;

    public Image skillIconImage;

    [Header("Skill Panel")]
    public GameObject skillPanel;
    public Button confirmButton;
    public Button cancelButton;

    private Skill currentSkill;

    /// <summary>
    /// Call this from a skill button to open the skill panel for that skill
    /// </summary>
    /// <param name="index">Index of the skill in the list</param>
    public void OpenSkillPanel(int index)
    {
        if (index < 0 || index >= skills.Count)
        {
            Debug.LogWarning("Skill index out of range!");
            return;
        }

        currentSkill = skills[index];
        SetUI(currentSkill);

        skillPanel.SetActive(true);

        // Cancel button closes panel
        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() => skillPanel.SetActive(false));

        // Confirm button executes the skill
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            currentSkill.onActivate?.Invoke();
            skillPanel.SetActive(false);
        });
    }
    
     private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
   private void SetUI(Skill skill)
    {
        if (skillNameText != null) skillNameText.text = skill.skillName;
        if (spCostText != null) spCostText.text = $"SP: {skill.spCost}";
        if (cooldownText != null) cooldownText.text = $"CD: {skill.cooldown}";
        if (limitationText != null) limitationText.text = $"LIMITATION: {skill.limitation}";
        if (effectText != null) effectText.text = skill.effect;
        if (durationText != null) durationText.text = skill.duration > 0 ? $"DURATION: {skill.duration} TURNS" : "";
        if (skillTypeText != null) skillTypeText.text = skill.skillType;

        // ✅ Set the skill icon
        if (skillIconImage != null)
        {
            if (skill.icon != null)
                skillIconImage.sprite = skill.icon; // use the icon from the skill
            else
                skillIconImage.sprite = null; // clear if no icon assigned
        }
    }

}
