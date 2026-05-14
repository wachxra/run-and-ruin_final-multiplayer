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
    private const string JoinCodeKey = "JoinCode";

    public async Task<bool> InitAsync()
    {
        await UnityServices.InitializeAsync();

        AuthState authState = await AuthenticationWrapper.DoAuth();

        if (authState == AuthState.Authenticated)
        {
            return true;
        }

        return false;
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(MenuSceneName);
    }

    public async Task StartClientAsync(string joinCode)
    {
        joinCode = joinCode.Trim().ToUpper();

        bool joinedLobby = false;

        try
        {
            joinedLobby = await JoinLobbyByRelayCodeAsync(joinCode);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }

        if (!joinedLobby)
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

        PlayerPrefs.SetString(JoinCodeKey, joinCode);
        PlayerPrefs.Save();

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        RelayServerData relayServerData = allocation.ToRelayServerData("dtls");
        transport.SetRelayServerData(relayServerData);

        UserData userData = new UserData
        {
            userName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name")
        };

        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.StartClient();
    }

    async Task<bool> JoinLobbyByRelayCodeAsync(string joinCode)
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
        {
            Debug.LogWarning("No lobby found with Relay JoinCode: " + joinCode);
            return false;
        }

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

        CurrentLobby =
            await LobbyService.Instance.JoinLobbyByIdAsync(
                response.Results[0].Id,
                joinOptions);

        return CurrentLobby != null;
    }
}