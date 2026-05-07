using UnityEngine;

public class SkillUIController : MonoBehaviour
{
    [Header("UI")]
    public SkillCooldownUI cooldownUI;

    private CharacterSkillSO currentSkill;

    public CharacterSkillSO CurrentSkill => currentSkill;

    public void SetSkill(CharacterSkillSO skill)
    {
        currentSkill = skill;

        if (cooldownUI != null)
        {
            cooldownUI.SetSkill(skill);
        }

        Debug.Log("SET HUD SKILL : " + skill.skillName);
    }

    public void TriggerCooldown(float duration)
    {
        if (cooldownUI != null)
        {
            cooldownUI.StartCooldown(duration);
        }
    }

    public void ClearSkill()
    {
        currentSkill = null;

        if (cooldownUI != null)
        {
            cooldownUI.skillIcon.sprite = null;
            cooldownUI.cooldownOverlay.fillAmount = 0f;
            cooldownUI.cooldownText.text = "";
        }
    }
}