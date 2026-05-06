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
        if (okButton != null)
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
        SyncUIWithData();
    }

    void SetupToggles()
    {
        if (localPlayer == null) return;

        foreach (var t in runnerToggles)
            t.onValueChanged.RemoveAllListeners();

        foreach (var t in tricksterToggles)
            t.onValueChanged.RemoveAllListeners();

        for (int i = 0; i < runnerToggles.Length; i++)
        {
            int index = i;

            runnerToggles[i].isOn = false;

            runnerToggles[i].onValueChanged.AddListener((isOn) =>
            {
                if (isOn && localPlayer != null)
                    localPlayer.SetRunnerServerRpc(index);
            });
        }

        for (int i = 0; i < tricksterToggles.Length; i++)
        {
            int index = i;

            tricksterToggles[i].isOn = false;

            tricksterToggles[i].onValueChanged.AddListener((isOn) =>
            {
                if (isOn && localPlayer != null)
                    localPlayer.SetTricksterServerRpc(index);
            });
        }

        localPlayer.selectedRunnerIndex.OnValueChanged -= OnSelectionChanged;
        localPlayer.selectedTricksterIndex.OnValueChanged -= OnSelectionChanged;

        localPlayer.selectedRunnerIndex.OnValueChanged += OnSelectionChanged;
        localPlayer.selectedTricksterIndex.OnValueChanged += OnSelectionChanged;
    }

    void SyncUIWithData()
    {
        if (localPlayer == null) return;

        int runnerIndex = localPlayer.selectedRunnerIndex.Value;
        int tricksterIndex = localPlayer.selectedTricksterIndex.Value;

        if (runnerIndex >= 0 && runnerIndex < runnerToggles.Length)
            runnerToggles[runnerIndex].isOn = true;

        if (tricksterIndex >= 0 && tricksterIndex < tricksterToggles.Length)
            tricksterToggles[tricksterIndex].isOn = true;

        UpdateOKButton();
    }

    void OnSelectionChanged(int oldValue, int newValue)
    {
        if (this == null || gameObject == null) return;
        if (okButton == null) return;
        if (localPlayer == null) return;

        UpdateOKButton();
    }

    void UpdateOKButton()
    {
        if (okButton == null || localPlayer == null) return;

        okButton.interactable =
            localPlayer.selectedRunnerIndex.Value >= 0 &&
            localPlayer.selectedTricksterIndex.Value >= 0;
    }

    void OnDestroy()
    {
        if (localPlayer != null)
        {
            localPlayer.selectedRunnerIndex.OnValueChanged -= OnSelectionChanged;
            localPlayer.selectedTricksterIndex.OnValueChanged -= OnSelectionChanged;
        }
    }

    public void OnClickOK()
    {
        if (localPlayer == null) return;

        localPlayer.SetReadyServerRpc();

        if (okButton != null)
            okButton.interactable = false;
    }
}