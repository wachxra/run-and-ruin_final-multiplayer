using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NameSelector : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private Button connectButton;
    [SerializeField] private int minNameLength = 1;
    [SerializeField] private int maxNameLength = 12;

    public const string PlayerNameKey = "PlayerName";
    private string lastText = "";

    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM("BGM_Gameplay");
        }

        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            return;
        }

        PlayerPrefs.DeleteKey(PlayerNameKey);
        PlayerPrefs.Save();

        nameField.text = "";
        lastText = nameField.text;

        nameField.onValueChanged.AddListener(OnNameTyping);

        HandleNameChanged();
    }

    public void HandleNameChanged()
    {
        connectButton.interactable =
            nameField.text.Length >= minNameLength &&
            nameField.text.Length <= maxNameLength;
    }

    public void Connect()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("click");
        }

        PlayerPrefs.SetString(PlayerNameKey, nameField.text);
        PlayerPrefs.Save();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    void OnNameTyping(string value)
    {
        HandleNameChanged();

        if (value != lastText)
        {
            lastText = value;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("click");
            }
        }
    }
}