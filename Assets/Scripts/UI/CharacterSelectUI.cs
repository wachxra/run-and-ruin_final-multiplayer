using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;

public class CharacterSelectUI : MonoBehaviour
{
    [Header("Runner Toggles")]
    public Toggle[] runnerToggles;

    [Header("Trickster Toggles")]
    public Toggle[] tricksterToggles;

    [Header("Buttons")]
    public Button okButton;

    private NetworkPlayer localPlayer;

    void Start()
    {
        okButton.interactable = false;
        StartCoroutine(WaitForPlayerObject());
    }

    void Awake()
    {
        foreach (var t in runnerToggles)
            t.isOn = false;

        foreach (var t in tricksterToggles)
            t.isOn = false;
    }

    IEnumerator WaitForPlayerObject()
    {
        while (NetworkManager.Singleton == null ||
               NetworkManager.Singleton.LocalClient == null ||
               NetworkManager.Singleton.LocalClient.PlayerObject == null)
        {
            yield return null;
        }

        localPlayer = NetworkManager.Singleton
            .LocalClient
            .PlayerObject
            .GetComponent<NetworkPlayer>();

        localPlayer.selectedRunnerIndex.OnValueChanged += OnSelectionChanged;
        localPlayer.selectedTricksterIndex.OnValueChanged += OnSelectionChanged;

        SetupToggles();
    }

    void SetupToggles()
    {
        foreach (var t in runnerToggles)
            t.SetIsOnWithoutNotify(false);

        foreach (var t in tricksterToggles)
            t.SetIsOnWithoutNotify(false);

        for (int i = 0; i < runnerToggles.Length; i++)
        {
            int index = i;
            runnerToggles[i].onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                    SelectRunner(index);
            });
        }

        for (int i = 0; i < tricksterToggles.Length; i++)
        {
            int index = i;
            tricksterToggles[i].onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                    SelectTrickster(index);
            });
        }
    }

    void SelectRunner(int index)
    {
        if (localPlayer == null) return;

        localPlayer.SetRunnerServerRpc(index);
    }

    void SelectTrickster(int index)
    {
        if (localPlayer == null) return;

        localPlayer.SetTricksterServerRpc(index);
    }

    void OnSelectionChanged(int oldValue, int newValue)
    {
        CheckReadyCondition();
    }

    void CheckReadyCondition()
    {
        if (localPlayer.selectedRunnerIndex.Value >= 0 &&
            localPlayer.selectedTricksterIndex.Value >= 0)
        {
            okButton.interactable = true;
        }
        else
        {
            okButton.interactable = false;
        }
    }

    public void OnClickOK()
    {
        if (localPlayer == null) return;

        localPlayer.SetReadyServerRpc();
        okButton.interactable = false;
    }
}