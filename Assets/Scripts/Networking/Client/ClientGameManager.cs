using System;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Authentication;

public class ClientGameManager
{
    public Lobby CurrentLobby;

    private JoinAllocation allocation;
    private const string MenuSceneName = "Menu";
    private const string GameSceneName = "CharacterSelect";
    private const string JoinCodeKey = "JoinCode";

    private string pendingJoinCode = "";
    private string pendingLobbyId = "";

    public async Task<bool> InitAsync()
    {
        await UnityServices.InitializeAsync();

        AuthState authState = await AuthenticationWrapper.DoAuth();

        return authState == AuthState.Authenticated;
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(MenuSceneName);
    }

    public async Task StartClientAsync(string joinCode)
    {
        joinCode = joinCode.Trim().ToUpper();

        Lobby targetLobby = await FindLobbyByRelayCodeAsync(joinCode);

        if (targetLobby == null)
        {
            Debug.LogWarning("Cannot join. No host lobby found with code: " + joinCode);
            return;
        }

        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }

        pendingJoinCode = joinCode;
        pendingLobbyId = targetLobby.Id;

        UnityTransport transport =
            NetworkManager.Singleton.GetComponent<UnityTransport>();

        RelayServerData relayServerData =
            allocation.ToRelayServerData("dtls");

        transport.SetRelayServerData(relayServerData);

        UserData userData = new UserData
        {
            userName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name")
        };

        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        bool started = NetworkManager.Singleton.StartClient();

        if (!started)
        {
            Debug.LogWarning("StartClient failed");
            return;
        }
    }

    private async void OnClientConnected(ulong clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId)
            return;

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;

        try
        {
            CurrentLobby = await JoinLobbyByIdAsync(pendingLobbyId);

            PlayerPrefs.SetString(JoinCodeKey, pendingJoinCode);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

        if (SceneManager.GetActiveScene().name != GameSceneName)
        {
            SceneManager.LoadScene(GameSceneName);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId)
            return;

        Debug.LogWarning("Client disconnected from host.");
    }

    async Task<Lobby> FindLobbyByRelayCodeAsync(string joinCode)
    {
        QueryLobbiesOptions options = new QueryLobbiesOptions
        {
            Count = 1,
            Filters = new List<QueryFilter>
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.S1,
                    op: QueryFilter.OpOptions.EQ,
                    value: joinCode)
            }
        };

        QueryResponse response =
            await LobbyService.Instance.QueryLobbiesAsync(options);

        if (response.Results == null || response.Results.Count == 0)
            return null;

        return response.Results[0];
    }

    async Task<Lobby> JoinLobbyByIdAsync(string lobbyId)
    {
        string playerName =
            PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Player");

        JoinLobbyByIdOptions joinOptions = new JoinLobbyByIdOptions
        {
            Player = new Player(
                id: AuthenticationService.Instance.PlayerId,
                data: new Dictionary<string, PlayerDataObject>
                {
                    {
                        "PlayerName",
                        new PlayerDataObject(
                            PlayerDataObject.VisibilityOptions.Public,
                            playerName)
                    }
                })
        };

        return await LobbyService.Instance.JoinLobbyByIdAsync(
            lobbyId,
            joinOptions);
    }
}