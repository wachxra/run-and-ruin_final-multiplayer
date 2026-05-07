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
}