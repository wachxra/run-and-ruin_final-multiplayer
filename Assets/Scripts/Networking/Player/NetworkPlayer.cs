using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> playerName =
        new NetworkVariable<FixedString32Bytes>();

    public NetworkVariable<int> selectedRunnerIndex =
        new NetworkVariable<int>(-1);

    public NetworkVariable<int> selectedTricksterIndex =
        new NetworkVariable<int>(-1);

    public NetworkVariable<bool> isReady =
        new NetworkVariable<bool>(false);

    public NetworkVariable<int> score =
        new NetworkVariable<int>(0);

    public NetworkVariable<CharacterRole> currentRole =
        new NetworkVariable<CharacterRole>();

    [Header("Skill Database")]
    public CharacterSkillDatabase skillDatabase;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            string name = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Player");
            SetNameServerRpc(name);
        }
    }

    [ServerRpc]
    void SetNameServerRpc(string name)
    {
        playerName.Value = name;
    }

    [ServerRpc]
    public void SetRunnerServerRpc(int index)
    {
        selectedRunnerIndex.Value = index;

        AssignRunnerSkill();
    }

    [ServerRpc]
    public void SetTricksterServerRpc(int index)
    {
        selectedTricksterIndex.Value = index;

        AssignTricksterSkill();
    }

    [ServerRpc]
    public void SetReadyServerRpc()
    {
        if (selectedRunnerIndex.Value < 0) return;
        if (selectedTricksterIndex.Value < 0) return;

        isReady.Value = true;

        CharacterSelectManager.Instance.CheckAllReady();
    }

    void AssignRunnerSkill()
    {
        if (skillDatabase == null) return;

        int index = selectedRunnerIndex.Value;

        if (index < 0 || index >= skillDatabase.runnerSkills.Length) return;

        var skill = skillDatabase.runnerSkills[index];

        var runner = GetComponentInChildren<RunnerController>();
        if (runner != null)
        {
            runner.skill = skill;
        }
    }

    void AssignTricksterSkill()
    {
        if (skillDatabase == null) return;

        int index = selectedTricksterIndex.Value;

        if (index < 0 || index >= skillDatabase.tricksterSkills.Length) return;

        var skill = skillDatabase.tricksterSkills[index];

        var trickster = GetComponent<TricksterSkillController>();
        if (trickster != null)
        {
            trickster.skill = skill;
        }
    }
}