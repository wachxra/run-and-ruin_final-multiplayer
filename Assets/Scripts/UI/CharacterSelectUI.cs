using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using System.Collections;

public class CharacterSelectUI : MonoBehaviour
{
    [Header("Runner")]
    public Toggle[] runnerToggles;

    [Header("Trickster")]
    public Toggle[] tricksterToggles;

    [Header("Buttons")]
    public Button okButton;

    private NetworkPlayer localPlayer;

    void Awake()
    {
        foreach (var t in runnerToggles)
            t.isOn = false;

        foreach (var t in tricksterToggles)
            t.isOn = false;
    }

    void Start()
    {
        okButton.interactable = false;
        StartCoroutine(WaitForPlayerObject());
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

        SetupToggles();
    }

    void SetupToggles()
    {
        for (int i = 0; i < runnerToggles.Length; i++)
        {
            int index = i;

            runnerToggles[i].onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                    localPlayer.SetRunnerServerRpc(index);
            });
        }

        for (int i = 0; i < tricksterToggles.Length; i++)
        {
            int index = i;

            tricksterToggles[i].onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                    localPlayer.SetTricksterServerRpc(index);
            });
        }

        localPlayer.selectedRunnerIndex.OnValueChanged += OnSelectionChanged;
        localPlayer.selectedTricksterIndex.OnValueChanged += OnSelectionChanged;
    }

    void OnSelectionChanged(int oldValue, int newValue)
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