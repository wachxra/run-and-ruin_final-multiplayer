using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class CharacterSelectManager : NetworkBehaviour
{
    public static CharacterSelectManager Instance;

    private bool isDestroyed = false;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null) continue;

            var player = client.PlayerObject.GetComponent<NetworkPlayer>();
            if (player == null) continue;

            player.isReady.OnValueChanged += OnPlayerReadyChanged;
        }
    }

    private void OnPlayerReadyChanged(bool oldValue, bool newValue)
    {
        if (!IsServer) return;
        if (isDestroyed) return;
        if (this == null) return;

        CheckAllReady();
    }

    public void CheckAllReady()
    {
        if (!IsServer) return;
        if (isDestroyed) return;
        if (this == null) return;

        if (NetworkManager.Singleton == null) return;

        var clients = NetworkManager.Singleton.ConnectedClientsList;

        if (clients.Count < 2)
            return;

        foreach (var client in clients)
        {
            if (client.PlayerObject == null) return;

            var player = client.PlayerObject.GetComponent<NetworkPlayer>();
            if (player == null) return;

            if (!player.isReady.Value)
                return;

            if (player.selectedRunnerIndex.Value < 0)
                return;

            if (player.selectedTricksterIndex.Value < 0)
                return;
        }

        if (!isDestroyed && this != null && gameObject.activeInHierarchy)
        {
            StartCoroutine(LoadGameScene());
        }
    }

    IEnumerator LoadGameScene()
    {
        yield return new WaitForSeconds(1f);

        if (isDestroyed) yield break;
        if (NetworkManager.Singleton == null) yield break;

        NetworkManager.Singleton.SceneManager.LoadScene(
            "Game",
            UnityEngine.SceneManagement.LoadSceneMode.Single
        );
    }

    public override void OnNetworkDespawn()
    {
        Cleanup();
    }

    private void OnSceneDestroy()
    {
        Cleanup();
    }

    void Cleanup()
    {
        if (isDestroyed) return;

        isDestroyed = true;

        if (NetworkManager.Singleton == null) return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null) continue;

            var player = client.PlayerObject.GetComponent<NetworkPlayer>();
            if (player == null) continue;

            player.isReady.OnValueChanged -= OnPlayerReadyChanged;
        }
    }
}