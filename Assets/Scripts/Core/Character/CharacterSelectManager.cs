using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class CharacterSelectManager : NetworkBehaviour
{
    public static CharacterSelectManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var player = client.PlayerObject.GetComponent<NetworkPlayer>();
            player.isReady.OnValueChanged += OnPlayerReadyChanged;
        }
    }

    private void OnPlayerReadyChanged(bool oldValue, bool newValue)
    {
        if (!IsServer) return;

        CheckAllReady();
    }

    public void CheckAllReady()
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count < 2)
            return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var player = client.PlayerObject.GetComponent<NetworkPlayer>();

            if (!player.isReady.Value)
                return;

            if (player.selectedRunnerIndex.Value < 0)
                return;

            if (player.selectedTricksterIndex.Value < 0)
                return;
        }

        StartCoroutine(LoadGameScene());
    }

    IEnumerator LoadGameScene()
    {
        yield return new WaitForSeconds(1f);

        NetworkManager.Singleton.SceneManager
            .LoadScene("Game",
                UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}