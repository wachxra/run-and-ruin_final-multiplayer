using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;

public class LobbyHostWatcher : MonoBehaviour
{
    public TMPro.TextMeshProUGUI joinNotifyText;
    public TMPro.TextMeshProUGUI playerCountText;

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

            UpdatePlayerCountText(currentLobby.Players.Count);

            InvokeRepeating(nameof(CheckLobbyUpdate), 2f, 2f);
        }
    }

    async void CheckLobbyUpdate()
    {
        if (currentLobby == null) return;

        try
        {
            currentLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);

            UpdatePlayerCountText(currentLobby.Players.Count);

            HashSet<string> currentPlayerIds = new HashSet<string>();

            foreach (var player in currentLobby.Players)
            {
                currentPlayerIds.Add(player.Id);

                if (!knownPlayerIds.Contains(player.Id))
                {
                    knownPlayerIds.Add(player.Id);

                    string playerName = "Player";

                    if (player.Data != null &&
                        player.Data.ContainsKey("PlayerName") &&
                        player.Data["PlayerName"] != null)
                    {
                        playerName = player.Data["PlayerName"].Value;
                    }

                    ShowJoinMessage(playerName);
                }
            }

            var playersToRemove = new List<string>();

            foreach (var id in knownPlayerIds)
            {
                if (!currentPlayerIds.Contains(id))
                {
                    playersToRemove.Add(id);
                }
            }

            foreach (var id in playersToRemove)
            {
                knownPlayerIds.Remove(id);
                OnPlayerLeft();
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    void UpdatePlayerCountText(int count)
    {
        if (playerCountText != null)
        {
            playerCountText.text = count + "/2";
        }
    }

    void OnPlayerLeft()
    {
        Debug.Log("A player left lobby");

        UpdatePlayerCountText(1);

        if (joinNotifyText != null)
        {
            joinNotifyText.text = "";
            joinNotifyText.gameObject.SetActive(false);
        }
    }

    void ShowJoinMessage(string playerName)
    {
        Debug.Log(playerName + " joined!");

        UpdatePlayerCountText(2);

        if (joinNotifyText != null)
        {
            joinNotifyText.gameObject.SetActive(true);
            joinNotifyText.text = playerName + " joined the lobby!";
        }
    }
}