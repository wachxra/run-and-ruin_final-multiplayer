using Unity.Netcode;
using UnityEngine;
using System.Collections;

public enum GamePhase
{
    Bootstrap,
    Menu,
    CharacterSelect,
    LoadingGame,
    Round1,
    Round2,
    RoundEnd,
    GameOver
}

public enum CharacterRole
{
    Runner,
    Trickster
}

public class GameFlowManager : NetworkBehaviour
{
    public static GameFlowManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public NetworkVariable<bool> isRoleDecided =
        new NetworkVariable<bool>(false);

    public NetworkVariable<ulong> firstRunnerClientId =
        new NetworkVariable<ulong>();

    public NetworkVariable<int> currentRound =
        new NetworkVariable<int>(1);

    public NetworkVariable<int> countdownValue =
        new NetworkVariable<int>(0);

    public NetworkVariable<GamePhase> phase =
        new NetworkVariable<GamePhase>(GamePhase.Round1);

    [Header("Character Prefabs")]
    public GameObject[] runnerPrefabs;

    [Header("Trickster UI Panels")]
    public GameObject[] tricksterUIPanels;

    [Header("Spawn Points")]
    public Transform runnerSpawnPoint;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentRound.Value = 1;
            StartCoroutine(StartRound());
        }
    }

    IEnumerator StartRound()
    {
        if (!IsServer) yield break;

        isRoleDecided.Value = false;

        yield return new WaitForSeconds(1f);

        AssignRoles();

        isRoleDecided.Value = true;

        yield return StartCoroutine(Countdown(5));

        SpawnRunner();

        SetupTricksterUI();

        yield return StartCoroutine(Countdown(3));

        phase.Value = currentRound.Value == 1
            ? GamePhase.Round1
            : GamePhase.Round2;
    }

    void AssignRoles()
    {
        if (!IsServer) return;

        var clients = NetworkManager.Singleton.ConnectedClientsList;

        if (clients.Count < 2)
            return;

        if (currentRound.Value == 1)
        {
            int random = Random.Range(0, clients.Count);
            firstRunnerClientId.Value = clients[random].ClientId;
        }
        else
        {
            foreach (var client in clients)
            {
                if (client.ClientId != firstRunnerClientId.Value)
                {
                    firstRunnerClientId.Value = client.ClientId;
                    break;
                }
            }
        }

        foreach (var client in clients)
        {
            var player =
                client.PlayerObject.GetComponent<NetworkPlayer>();

            if (client.ClientId == firstRunnerClientId.Value)
                player.currentRole.Value = CharacterRole.Runner;
            else
                player.currentRole.Value = CharacterRole.Trickster;
        }

        isRoleDecided.Value = true;
    }

    IEnumerator Countdown(int seconds)
    {
        for (int i = seconds; i > 0; i--)
        {
            countdownValue.Value = i;
            yield return new WaitForSeconds(1f);
        }

        countdownValue.Value = 0;
    }

    void SpawnRunner()
    {
        if (!IsServer) return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var player =
                client.PlayerObject.GetComponent<NetworkPlayer>();

            if (player.currentRole.Value != CharacterRole.Runner)
                continue;

            int index = player.selectedRunnerIndex.Value;

            if (index < 0 || index >= runnerPrefabs.Length)
                continue;

            var character = Instantiate(
                runnerPrefabs[index],
                runnerSpawnPoint.position,
                Quaternion.identity
            );

            character.GetComponent<NetworkObject>()
                     .SpawnAsPlayerObject(client.ClientId);
        }
    }


    void SetupTricksterUI()
    {
        if (!IsServer) return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var player =
                client.PlayerObject.GetComponent<NetworkPlayer>();

            if (player.currentRole.Value != CharacterRole.Trickster)
                continue;

            int index = player.selectedTricksterIndex.Value;

            if (index < 0 || index >= tricksterUIPanels.Length)
                continue;

            ShowTricksterUIClientRpc(index, client.ClientId);
        }
    }

    [ClientRpc]
    void ShowTricksterUIClientRpc(int index, ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId)
            return;

        for (int i = 0; i < tricksterUIPanels.Length; i++)
        {
            tricksterUIPanels[i].SetActive(i == index);
        }
    }

    public void EndRound()
    {
        if (!IsServer) return;

        phase.Value = GamePhase.RoundEnd;

        if (currentRound.Value == 1)
        {
            currentRound.Value = 2;
            StartCoroutine(StartRound());
        }
        else
        {
            phase.Value = GamePhase.GameOver;
        }
    }
}