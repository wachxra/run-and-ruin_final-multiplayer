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

    public NetworkVariable<ulong> runner1ClientId = new NetworkVariable<ulong>();
    public NetworkVariable<ulong> runner2ClientId = new NetworkVariable<ulong>();

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

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TMPro.TextMeshProUGUI resultText;

    [Header("Skill Database")]
    public CharacterSkillDatabase skillDatabase;

    public NetworkVariable<float> networkTimer =
        new NetworkVariable<float>(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    public NetworkVariable<float> runner1TimeNet = new NetworkVariable<float>();
    public NetworkVariable<float> runner2TimeNet = new NetworkVariable<float>();

    private float roundTimer = 0f;
    private bool isTiming = false;

    IEnumerator RoundTimer()
    {
        roundTimer = 0f;
        isTiming = true;

        while (isTiming)
        {
            roundTimer += Time.deltaTime;
            networkTimer.Value = roundTimer;
            yield return null;
        }
    }

    public void RecordRunnerTime(ulong clientId)
    {
        if (!IsServer) return;

        if (currentRound.Value == 1)
            runner1TimeNet.Value = roundTimer;
        else
            runner2TimeNet.Value = roundTimer;

        isTiming = false;
    }

    public override void OnNetworkSpawn()
    {
        runner1TimeNet.OnValueChanged += OnTimeChanged;
        runner2TimeNet.OnValueChanged += OnTimeChanged;

        if (!IsServer) return;

        currentRound.Value = 1;
        phase.Value = GamePhase.LoadingGame;

        StartCoroutine(StartRound());
    }

    void OnTimeChanged(float oldVal, float newVal)
    {
        if (gameOverPanel != null && gameOverPanel.activeSelf)
        {
            UpdateResultUI();
        }
    }

    [ClientRpc]
    void SetLocalSkillUIClientRpc(CharacterRole role, int skillIndex, ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId)
            return;

        SkillUIController skillUI =
            FindFirstObjectByType<SkillUIController>(FindObjectsInactive.Include);

        if (skillUI == null || skillDatabase == null)
            return;

        skillUI.ClearSkill();

        CharacterSkillSO skill = null;

        if (role == CharacterRole.Runner)
        {
            if (skillIndex >= 0 && skillIndex < skillDatabase.runnerSkills.Length)
                skill = skillDatabase.runnerSkills[skillIndex];
        }
        else if (role == CharacterRole.Trickster)
        {
            if (skillIndex >= 0 && skillIndex < skillDatabase.tricksterSkills.Length)
                skill = skillDatabase.tricksterSkills[skillIndex];
        }

        if (skill != null)
        {
            skillUI.SetSkill(skill);
            skillUI.ResetCooldown();

            Debug.Log("SET LOCAL SKILL UI : " + role + " / " + skill.skillName);
        }
        else
        {
            Debug.LogWarning("SetLocalSkillUI failed | role: " + role + " | index: " + skillIndex);
        }
    }

    void UpdateResultUI()
    {
        string runner1Name = GetPlayerName(runner1ClientId.Value);
        string runner2Name = GetPlayerName(runner2ClientId.Value);
        string winner = GetWinnerName();

        if (resultText != null)
        {
            resultText.text =
                "Game Over\n" +
                "Winner: " + winner + "\n\n" +
                runner1Name + ": " + runner1TimeNet.Value.ToString("F2") + "s\n" +
                runner2Name + ": " + runner2TimeNet.Value.ToString("F2") + "s";
        }
    }

    IEnumerator StartRound()
    {
        if (!IsServer) yield break;

        isRoleDecided.Value = false;

        yield return new WaitUntil(() =>
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.PlayerObject == null)
                    return false;
            }

            return true;
        });

        AssignRoles();

        isRoleDecided.Value = true;

        ShowHUDClientRpc(false);

        ResetAllSkillCooldownUIClientRpc();

        yield return StartCoroutine(Countdown(5));

        SpawnRunner();

        SetupTricksterUI();

        ShowHUDClientRpc(true);

        yield return StartCoroutine(Countdown(3));

        phase.Value = currentRound.Value == 1
            ? GamePhase.Round1
            : GamePhase.Round2;

        StartCoroutine(RoundTimer());
    }

    public void EndRound()
    {
        if (!IsServer) return;

        phase.Value = GamePhase.RoundEnd;

        ResetAllSkillCooldownUIClientRpc();

        ShowHUDClientRpc(false);

        if (currentRound.Value == 1)
        {
            currentRound.Value = 2;
            StartCoroutine(StartNextRoundWithReset());
        }
        else
        {
            phase.Value = GamePhase.GameOver;
            ShowGameOverClientRpc();
        }
    }

    void AssignRoles()
    {
        if (!IsServer) return;

        var clients = NetworkManager.Singleton.ConnectedClientsList;

        if (clients.Count < 2)
            return;

        ulong selectedRunnerId = 0;

        if (currentRound.Value == 1)
        {
            int random = Random.Range(0, clients.Count);
            selectedRunnerId = clients[random].ClientId;
            runner1ClientId.Value = selectedRunnerId;
        }
        else
        {
            foreach (var client in clients)
            {
                if (client.ClientId != runner1ClientId.Value)
                {
                    selectedRunnerId = client.ClientId;
                    runner2ClientId.Value = selectedRunnerId;
                    break;
                }
            }
        }

        foreach (var client in clients)
        {
            if (client.PlayerObject == null)
            {
                Debug.LogWarning($"PlayerObject not ready for client {client.ClientId}");
                continue;
            }

            var player = client.PlayerObject.GetComponent<NetworkPlayer>();

            if (player == null)
            {
                Debug.LogWarning($"NetworkPlayer missing on client {client.ClientId}");
                continue;
            }

            if (client.ClientId == selectedRunnerId)
                player.currentRole.Value = CharacterRole.Runner;
            else
                player.currentRole.Value = CharacterRole.Trickster;
        }
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

            var netObj = character.GetComponent<NetworkObject>();

            netObj.SpawnWithOwnership(client.ClientId);

            character.transform.SetParent(client.PlayerObject.transform);

            SetLocalSkillUIClientRpc(
                CharacterRole.Runner,
                index,
                client.ClientId
            );
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

            SetLocalSkillUIClientRpc(
                CharacterRole.Trickster,
                index,
                client.ClientId
            );
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

    [ClientRpc]
    void ShowHUDClientRpc(bool show)
    {
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.ShowHUD(show);
        }
    }

    void ResetGameState()
    {
        if (!IsServer) return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObj = client.PlayerObject;

            if (playerObj == null) continue;

            var runner = playerObj.GetComponentInChildren<RunnerController>();

            if (runner != null)
            {
                var netObj = runner.GetComponent<NetworkObject>();

                if (netObj != null && netObj.IsSpawned)
                    netObj.Despawn();
            }
        }

        isRoleDecided.Value = false;
        countdownValue.Value = 0;

        HideAllTricksterUIClientRpc();
    }

    IEnumerator StartNextRoundWithReset()
    {
        yield return new WaitForSeconds(2f);

        ResetGameState();

        yield return new WaitForSeconds(0.5f);

        StartCoroutine(StartRound());
    }

    [ClientRpc]
    void ShowGameOverClientRpc()
    {
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.ShowHUD(false);
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        UpdateResultUI();
    }

    string GetWinnerName()
    {
        ulong winnerId;

        if (runner1TimeNet.Value > runner2TimeNet.Value)
        {
            winnerId = runner1ClientId.Value;
        }
        else
        {
            winnerId = runner2ClientId.Value;
        }

        return GetPlayerName(winnerId);
    }

    string GetPlayerName(ulong clientId)
    {
        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
            return "Unknown";

        var playerObj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        var netPlayer = playerObj.GetComponent<NetworkPlayer>();

        return netPlayer != null ? netPlayer.playerName.Value.ToString() : "Unknown";
    }

    [ClientRpc]
    void HideAllTricksterUIClientRpc()
    {
        foreach (var panel in tricksterUIPanels)
        {
            if (panel != null)
                panel.SetActive(false);
        }
    }

    [ClientRpc]
    void ResetAllSkillCooldownUIClientRpc()
    {
        SkillUIController skillUI =
            FindFirstObjectByType<SkillUIController>(FindObjectsInactive.Include);

        if (skillUI != null)
        {
            skillUI.ResetCooldown();
        }
    }
}