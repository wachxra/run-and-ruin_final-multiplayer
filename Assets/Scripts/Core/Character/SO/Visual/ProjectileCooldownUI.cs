using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ProjectileCooldownUI : MonoBehaviour
{
    [Header("Root")]
    public GameObject rootPanel;

    [Header("UI")]
    public Image verticalFillImage;
    public TMP_Text cooldownText;

    private Coroutine cooldownRoutine;

    private void Awake()
    {
        Hide();
    }

    public void StartCooldown(float duration)
    {
        if (!IsLocalPlayerTrickster())
        {
            Hide();
            return;
        }

        if (cooldownRoutine != null)
            StopCoroutine(cooldownRoutine);

        cooldownRoutine = StartCoroutine(CooldownRoutine(duration));
    }

    IEnumerator CooldownRoutine(float duration)
    {
        Show();

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

        Hide();
    }

    void Show()
    {
        if (rootPanel != null)
            rootPanel.SetActive(true);
    }

    void Hide()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);

        if (verticalFillImage != null)
            verticalFillImage.fillAmount = 0f;

        if (cooldownText != null)
            cooldownText.text = "";
    }

    bool IsLocalPlayerTrickster()
    {
        if (Unity.Netcode.NetworkManager.Singleton == null)
            return false;

        if (Unity.Netcode.NetworkManager.Singleton.LocalClient == null)
            return false;

        if (Unity.Netcode.NetworkManager.Singleton.LocalClient.PlayerObject == null)
            return false;

        NetworkPlayer player =
            Unity.Netcode.NetworkManager.Singleton.LocalClient.PlayerObject
                .GetComponent<NetworkPlayer>();

        if (player == null)
            return false;

        return player.currentRole.Value == CharacterRole.Trickster;
    }
}