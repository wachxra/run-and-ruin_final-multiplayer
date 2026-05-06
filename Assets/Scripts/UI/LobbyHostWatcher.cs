using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;

public class LobbyHostWatcher : MonoBehaviour
{
    public TMPro.TextMeshProUGUI joinNotifyText;

    private Lobby currentLobby;
    private HashSet<string> knownPlayerIds = new HashSet<string>();

    private async void Start()
    {
        await Task.Delay(1000);

        currentLobby = ClientSingleton.Instance.GameManager.CurrentLobby;

        if (currentLobby != null)
        {
            foreach (var player in currentLobby.Players)
            {
                knownPlayerIds.Add(player.Id);
            }

            InvokeRepeating(nameof(CheckLobbyUpdate), 2f, 2f);
        }
    }

    async void CheckLobbyUpdate()
    {
        if (currentLobby == null) return;

        try
        {
            currentLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);

            foreach (var player in currentLobby.Players)
            {
                if (!knownPlayerIds.Contains(player.Id))
                {
                    knownPlayerIds.Add(player.Id);

                    string playerName = player.Data["PlayerName"].Value;

                    ShowJoinMessage(playerName);
                }
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    void ShowJoinMessage(string playerName)
    {
        Debug.Log(playerName + " joined!");

        if (joinNotifyText != null)
        {
            joinNotifyText.text = playerName + " joined the lobby!";
        }
    }
}