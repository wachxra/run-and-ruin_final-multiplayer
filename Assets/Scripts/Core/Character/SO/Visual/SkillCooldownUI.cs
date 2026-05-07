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
            skillIcon.sprite = skill.icon;
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

    public void StartCooldown(float duration)
    {
        if (cooldownRoutine != null)
        {
            StopCoroutine(cooldownRoutine);
        }

        cooldownRoutine =
            StartCoroutine(CooldownRoutine(duration));
    }

    IEnumerator CooldownRoutine(float duration)
    {
        float timer = duration;

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount =
                    timer / duration;
            }

            if (cooldownText != null)
            {
                cooldownText.text =
                    Mathf.Ceil(timer).ToString();
            }

            yield return null;
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
}