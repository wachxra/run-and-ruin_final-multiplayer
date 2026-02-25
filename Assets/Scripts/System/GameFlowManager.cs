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

    public NetworkVariable<GamePhase> phase =
        new NetworkVariable<GamePhase>(GamePhase.Round1);

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

        phase.Value = currentRound.Value == 1
            ? GamePhase.Round1
            : GamePhase.Round2;

        yield return new WaitForSeconds(1f);

        AssignRoles();

        yield return new WaitForSeconds(1f);

        StartCoroutine(StartCountdown());
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

    IEnumerator StartCountdown()
    {
        if (!IsServer) yield break;

        yield return new WaitForSeconds(3f);

        SpawnRunner();
    }

    void SpawnRunner()
    {
        if (!IsServer) return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var player =
                client.PlayerObject.GetComponent<NetworkPlayer>();

            if (player.currentRole.Value == CharacterRole.Runner)
            {
                var spawner =
                    FindFirstObjectByType<RunnerSpawner>();

                if (spawner != null)
                {
                    spawner.SpawnRunnerFor(
                        player.OwnerClientId,
                        player.selectedRunnerIndex.Value
                    );
                }
            }
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