using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ProjectileCooldownUI : MonoBehaviour
{
    public Image verticalFillImage;
    public TMP_Text cooldownText;

    private Coroutine cooldownRoutine;

    public void StartCooldown(float duration)
    {
        if (cooldownRoutine != null)
            StopCoroutine(cooldownRoutine);

        cooldownRoutine = StartCoroutine(CooldownRoutine(duration));
    }

    IEnumerator CooldownRoutine(float duration)
    {
        float timer = duration;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;

            if (verticalFillImage != null)
                verticalFillImage.fillAmount = timer / duration;

            if (cooldownText != null)
                cooldownText.text = Mathf.Ceil(timer).ToString();

            yield return null;
        }

        if (verticalFillImage != null)
            verticalFillImage.fillAmount = 0f;

        if (cooldownText != null)
            cooldownText.text = "";
    }
}