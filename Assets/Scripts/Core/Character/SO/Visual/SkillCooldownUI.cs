using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SkillCooldownUI : MonoBehaviour
{
    [Header("UI")]
    public Image skillIcon;
    public Image cooldownOverlay;
    public TMP_Text cooldownText;

    private CharacterSkillSO currentSkill;
    private Coroutine cooldownRoutine;

    public void SetSkill(CharacterSkillSO skill)
    {
        currentSkill = skill;

        if (skillIcon != null)
        {
            skillIcon.gameObject.SetActive(true);
            skillIcon.enabled = true;
            skillIcon.sprite = skill != null ? skill.icon : null;
            skillIcon.color = Color.white;
            skillIcon.preserveAspect = true;
        }

        ResetCooldown();
    }

    public void StartCooldown(float duration)
    {
        if (duration <= 0f) return;

        if (cooldownRoutine != null)
        {
            StopCoroutine(cooldownRoutine);
        }

        cooldownRoutine = StartCoroutine(CooldownRoutine(duration));
    }

    IEnumerator CooldownRoutine(float duration)
    {
        float timer = duration;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;

            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = timer / duration;
            }

            if (cooldownText != null)
            {
                cooldownText.text = Mathf.Ceil(timer).ToString();
            }

            yield return null;
        }

        ResetCooldown();
    }

    public void ResetCooldown()
    {
        if (cooldownRoutine != null)
        {
            StopCoroutine(cooldownRoutine);
            cooldownRoutine = null;
        }

        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = 0f;
        }

        if (cooldownText != null)
        {
            cooldownText.text = "";
        }
    }

    public void ClearSkill()
    {
        ResetCooldown();

        currentSkill = null;

        if (skillIcon != null)
        {
            skillIcon.sprite = null;
        }
    }
}