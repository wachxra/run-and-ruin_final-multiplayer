using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Unity.Services.Lobbies;
using Unity.Services.Authentication;

public class SceneLoader : NetworkBehaviour
{
    public static SceneLoader Instance;

    private const string CharacterSelectScene = "CharacterSelect";
    private const string MenuScene = "Menu";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RestartGame()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            RestartGameServer();
        }
        else
        {
            RequestRestartServerRpc();
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void RequestRestartServerRpc()
    {
        RestartGameServer();
    }

    void RestartGameServer()
    {
        Debug.Log("Restart Game (Server)");

        ResetGameFlow();
        ResetAllPlayers();

        NetworkManager.Singleton.SceneManager.LoadScene(
            CharacterSelectScene,
            LoadSceneMode.Single
        );
    }

    public async void BackToMenu()
    {
        if (NetworkManager.Singleton == null)
        {
            SceneManager.LoadScene(MenuScene);
            return;
        }

        if (ClientSingleton.Instance.GameManager.CurrentLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(
                    ClientSingleton.Instance.GameManager.CurrentLobby.Id,
                    AuthenticationService.Instance.PlayerId
                );
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Host Back To Menu");

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.ClientId == NetworkManager.Singleton.LocalClientId)
                    continue;

                NetworkManager.Singleton.DisconnectClient(client.ClientId);
            }

            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene(MenuScene);
        }
        else
        {
            Debug.Log("Client Leave Lobby");

            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene(MenuScene);
        }
    }

    void BackToMenuServer()
    {
        if (!NetworkManager.Singleton.IsListening)
            return;

        Debug.Log("Host closing lobby");

        DisconnectClientRpc();

        NetworkManager.Singleton.Shutdown();

        SceneManager.LoadScene(MenuScene);
    }

    [ClientRpc]
    void DisconnectClientRpc()
    {
        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }

    void ResetGameFlow()
    {
        var flow = GameFlowManager.Instance;
        if (flow == null) return;

        flow.currentRound.Value = 1;
        flow.phase.Value = GamePhase.Menu;
        flow.countdownValue.Value = 0;
        flow.isRoleDecided.Value = false;

        flow.runner1TimeNet.Value = 0f;
        flow.runner2TimeNet.Value = 0f;

        flow.runner1ClientId.Value = 0;
        flow.runner2ClientId.Value = 0;

        flow.networkTimer.Value = 0f;
    }

    void ResetAllPlayers()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObj = client.PlayerObject;
            if (playerObj == null) continue;

            var player = playerObj.GetComponent<NetworkPlayer>();
            if (player == null) continue;

            player.selectedRunnerIndex.Value = -1;
            player.selectedTricksterIndex.Value = -1;

            player.isReady.Value = false;

            player.currentRole.Value = CharacterRole.Runner;
        }
    }
}