using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinCodeField;

    private string lastJoinCode = "";

    private void Start()
    {
        if (joinCodeField != null)
        {
            lastJoinCode = joinCodeField.text;
            joinCodeField.onValueChanged.AddListener(OnJoinCodeTyping);
        }
    }

    private void OnDestroy()
    {
        if (joinCodeField != null)
        {
            joinCodeField.onValueChanged.RemoveListener(OnJoinCodeTyping);
        }
    }

    private void OnJoinCodeTyping(string value)
    {
        if (value == lastJoinCode)
            return;

        lastJoinCode = value;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("click");
        }
    }

    public async void StartHost()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("click");

        await HostSingleton.Instance.GameManager.StartHostAsync();
    }

    public async void StartClient()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("click");

        await ClientSingleton.Instance.GameManager.StartClientAsync(joinCodeField.text);
    }
}