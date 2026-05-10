using UnityEngine;

public class SkillUIController : MonoBehaviour
{
    [Header("UI")]
    public SkillCooldownUI cooldownUI;

    private CharacterSkillSO currentSkill;

    public CharacterSkillSO CurrentSkill => currentSkill;

    public void SetSkill(CharacterSkillSO skill)
    {
        if (skill == null)
        {
            Debug.LogWarning("SetSkill failed: skill is null");
            return;
        }

        currentSkill = skill;

        if (cooldownUI != null)
        {
            cooldownUI.SetSkill(skill);
        }
    }

    public void TriggerCooldown(float duration)
    {
        if (cooldownUI != null)
        {
            cooldownUI.StartCooldown(duration);
        }
    }

    public void ResetCooldown()
    {
        if (cooldownUI != null)
        {
            cooldownUI.ResetCooldown();
        }
    }

    public void ClearSkill()
    {
        if (cooldownUI != null)
        {
            cooldownUI.ClearSkill();
        }

        currentSkill = null;
    }

    public void RefreshCurrentSkill()
    {
        if (currentSkill == null)
            return;

        SetSkill(currentSkill);
    }

    public void RefreshSkillIcon()
    {
        if (currentSkill == null)
            return;

        if (cooldownUI == null)
            return;

        if (cooldownUI.skillIcon != null)
        {
            cooldownUI.skillIcon.gameObject.SetActive(true);
            cooldownUI.skillIcon.enabled = true;
            cooldownUI.skillIcon.sprite = currentSkill.icon;
            cooldownUI.skillIcon.color = Color.white;
            cooldownUI.skillIcon.preserveAspect = true;
        }
    }
}