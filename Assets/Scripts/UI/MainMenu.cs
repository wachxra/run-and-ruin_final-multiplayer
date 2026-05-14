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
            joinCodeField.text = joinCodeField.text.ToUpper();
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
        string upperValue = value.ToUpper();

        if (value != upperValue)
        {
            joinCodeField.SetTextWithoutNotify(upperValue);
            joinCodeField.caretPosition = upperValue.Length;
            value = upperValue;
        }

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

        if (joinCodeField == null)
            return;

        string joinCode = joinCodeField.text.Trim().ToUpper();

        if (string.IsNullOrEmpty(joinCode))
            return;

        joinCodeField.SetTextWithoutNotify(joinCode);

        await ClientSingleton.Instance.GameManager.StartClientAsync(joinCode);
    }
}