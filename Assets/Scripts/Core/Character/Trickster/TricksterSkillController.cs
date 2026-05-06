using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class TricksterSkillController : NetworkBehaviour
{
    public GameObject projectilePrefab;
    public Transform[] firePoints;

    [Header("Skill")]
    public CharacterSkillSO skill;
    private float lastSkillTime = -999f;

    private SkillUIController skillUI;

    private Dictionary<int, float> lastSkillUseTime = new Dictionary<int, float>();

    [Header("Projectile Skill Cooldown")]
    public float skill1Cooldown = 1f;
    public float skill2Cooldown = 1f;
    public float skill3Cooldown = 1f;

    void Update()
    {
        if (!IsOwner) return;

        var player = GetComponent<NetworkPlayer>();
        if (player == null || player.currentRole.Value != CharacterRole.Trickster)
            return;

        if (GameFlowManager.Instance == null ||
        (GameFlowManager.Instance.phase.Value != GamePhase.Round1 &&
         GameFlowManager.Instance.phase.Value != GamePhase.Round2))
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            UseSkillServerRpc(1);

        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            UseSkillServerRpc(2);

        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            UseSkillServerRpc(3);

        if (Input.GetKeyDown(KeyCode.K))
            UseSpecialSkillServerRpc();
    }

    [ServerRpc]
    void UseSpecialSkillServerRpc()
    {
        if (skill == null) return;
        if (Time.time - lastSkillTime < skill.cooldown) return;

        lastSkillTime = Time.time;
        ActivateSkill();
    }

    void ActivateSkill()
    {
        switch (skill.skillType)
        {
            case SkillType.Cannon:
                StartCoroutine(CannonRoutine());
                break;

            case SkillType.Blur:
                BlurAllRunnerClientRpc();
                break;

            case SkillType.SlowAll:
                SlowAllRunners();
                break;

            case SkillType.MultiShot:
                MultiShot();
                break;

            case SkillType.Trap:
                SpawnProjectile(1);
                break;
        }
    }

    IEnumerator CannonRoutine()
    {
        for (int i = 0; i < 3; i++)
        {
            SpawnProjectile(i);
            yield return new WaitForSeconds(0.5f);
        }
    }

    void MultiShot()
    {
        SpawnProjectile(0);
        SpawnProjectile(1);
        SpawnProjectile(2);
    }

    void SlowAllRunners()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var runner = client.PlayerObject.GetComponentInChildren<RunnerController>();
            if (runner != null)
            {
                runner.ApplySlow(skill.duration);
            }
        }
    }

    [ClientRpc]
    void BlurAllRunnerClientRpc()
    {
        Debug.Log("Blur Screen");
    }

    [ServerRpc]
    void UseSkillServerRpc(int skillIndex, ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        var senderPlayer = NetworkManager.Singleton
            .ConnectedClients[senderId]
            .PlayerObject
            .GetComponent<NetworkPlayer>();

        if (senderPlayer.currentRole.Value != CharacterRole.Trickster)
            return;

        float cooldown = GetCooldown(skillIndex);

        if (lastSkillUseTime.ContainsKey(skillIndex))
        {
            if (Time.time - lastSkillUseTime[skillIndex] < cooldown)
                return;
        }

        lastSkillUseTime[skillIndex] = Time.time;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var player = client.PlayerObject.GetComponent<NetworkPlayer>();

            if (player.currentRole.Value == CharacterRole.Runner)
            {
                if (skillIndex == 1)
                    SpawnProjectile(0);
                else if (skillIndex == 2)
                    SpawnProjectile(1);
                else if (skillIndex == 3)
                    SpawnProjectile(2);
            }
        }
    }

    float GetCooldown(int skillIndex)
    {
        switch (skillIndex)
        {
            case 1:
                return skill1Cooldown > 0 ? skill1Cooldown : (skill != null ? skill.cooldown : 1f);
            case 2:
                return skill2Cooldown > 0 ? skill2Cooldown : (skill != null ? skill.cooldown : 1f);
            case 3:
                return skill3Cooldown > 0 ? skill3Cooldown : (skill != null ? skill.cooldown : 1f);
        }

        return 1f;
    }

    void SpawnProjectile(int lane)
    {
        if (!IsServer) return;

        var spawnPoint = firePoints[lane];

        var obj = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
        obj.GetComponent<NetworkObject>().Spawn();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            skillUI = FindFirstObjectByType<SkillUIController>();

            if (skillUI != null && skill != null)
            {
                skillUI.SetSkill(skill);
            }
        }
    }
}