using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class TricksterSkillController : NetworkBehaviour
{
    public GameObject projectilePrefab;

    [Header("Projectile Prefabs")]
    public GameObject cannonBallPrefab;
    public GameObject bombPrefab;
    public GameObject knifePrefab;

    public GameObject glassPrefab;
    public GameObject barrelPrefab;

    public Transform[] firePoints;

    [Header("Skill")]
    public CharacterSkillSO skill;

    private float lastSkillTime = -999f;

    private SkillUIController skillUI;

    [Header("Projectile Cooldown")]
    public float projectileCooldown = 1f;

    private float lastProjectileSkillTime = -999f;

    void Update()
    {
        if (!IsOwner) return;

        var player = GetComponent<NetworkPlayer>();

        if (player == null ||
            player.currentRole.Value != CharacterRole.Trickster)
            return;

        if (GameFlowManager.Instance == null ||
            (GameFlowManager.Instance.phase.Value != GamePhase.Round1 &&
             GameFlowManager.Instance.phase.Value != GamePhase.Round2))
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1) ||
            Input.GetKeyDown(KeyCode.Keypad1))
        {
            UseSkillServerRpc(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) ||
            Input.GetKeyDown(KeyCode.Keypad2))
        {
            UseSkillServerRpc(2);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) ||
            Input.GetKeyDown(KeyCode.Keypad3))
        {
            UseSkillServerRpc(3);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            UseSpecialSkillServerRpc();
        }
    }

    [ServerRpc]
    void UseSpecialSkillServerRpc()
    {
        if (skill == null) return;

        if (Time.time - lastSkillTime < skill.cooldown)
            return;

        lastSkillTime = Time.time;

        ActivateSkill();

        TriggerSkillCooldownClientRpc(
            OwnerClientId,
            skill.cooldown);
    }

    [ClientRpc]
    void TriggerSkillCooldownClientRpc(ulong ownerId, float duration)
    {
        if (NetworkManager.Singleton.LocalClientId != ownerId)
            return;

        if (skillUI == null)
        {
            skillUI = FindFirstObjectByType<SkillUIController>(FindObjectsInactive.Include);
        }

        if (skillUI != null)
        {
            if (skill != null)
            {
                skillUI.SetSkill(skill);
            }

            skillUI.TriggerCooldown(duration);
        }
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
        }
    }

    IEnumerator CannonRoutine()
    {
        int randomLane = Random.Range(0, 3);

        for (int i = 0; i < 3; i++)
        {
            SpawnProjectile(randomLane);

            yield return new WaitForSeconds(1f);
        }
    }

    [ClientRpc]
    void BlurAllRunnerClientRpc()
    {
        var blur = FindFirstObjectByType<ScreenBlurEffect>();

        if (blur != null)
        {
            blur.PlayBlur(5f);
        }
    }

    [ServerRpc]
    void UseSkillServerRpc(
        int skillIndex,
        ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        var senderPlayer =
            NetworkManager.Singleton
            .ConnectedClients[senderId]
            .PlayerObject
            .GetComponent<NetworkPlayer>();

        if (senderPlayer.currentRole.Value != CharacterRole.Trickster)
            return;

        if (Time.time - lastProjectileSkillTime < projectileCooldown)
            return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var runner =
                client.PlayerObject.GetComponentInChildren<RunnerController>();

            if (runner != null)
            {
                if (runner.IsLaneBlocked(skillIndex))
                {
                    Debug.Log("Lane Blocked");
                    return;
                }
            }
        }

        lastProjectileSkillTime = Time.time;

        SpawnProjectile(skillIndex - 1);
    }

    void SpawnProjectile(int lane)
    {
        if (!IsServer) return;

        var spawnPoint = firePoints[lane];

        GameObject prefabToSpawn = projectilePrefab;

        if (skill.skillType == SkillType.Cannon)
        {
            if (lane == 0)
                prefabToSpawn = cannonBallPrefab;

            else if (lane == 1)
                prefabToSpawn = bombPrefab;

            else
                prefabToSpawn = knifePrefab;
        }
        else if (skill.skillType == SkillType.Blur)
        {
            if (lane == 0)
                prefabToSpawn = glassPrefab;

            else if (lane == 1)
                prefabToSpawn = barrelPrefab;

            else
                prefabToSpawn = knifePrefab;
        }

        var obj = Instantiate(
            prefabToSpawn,
            spawnPoint.position,
            Quaternion.identity);

        var projectile = obj.GetComponent<SkillProjectile>();

        var effect = obj.GetComponent<ProjectileEffectData>();

        if (effect != null)
        {
            projectile.speed = effect.moveSpeed;
            projectile.damage = effect.damage;

            if (effect.followRunnerSpeed)
            {
                projectile.speed += 4f;
            }
        }

        obj.GetComponent<NetworkObject>().Spawn();
    }

    void SetupSkillUI()
    {
        if (!IsOwner) return;

        skillUI = FindFirstObjectByType<SkillUIController>(FindObjectsInactive.Include);

        if (skillUI != null && skill != null)
        {
            skillUI.ClearSkill();
            skillUI.SetSkill(skill);

            Debug.Log("TRICKSTER SET SKILL UI : " + skill.skillName);
        }
        else
        {
            Debug.LogWarning("Trickster SetupSkillUI failed | skillUI: " + skillUI + " | skill: " + skill);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        skillUI = FindFirstObjectByType<SkillUIController>(FindObjectsInactive.Include);
    }

    public void RefreshSkillUI()
    {
        if (!IsOwner) return;

        skillUI = FindFirstObjectByType<SkillUIController>(FindObjectsInactive.Include);

        if (skillUI != null && skill != null)
        {
            skillUI.ClearSkill();
            skillUI.SetSkill(skill);

            Debug.Log("TRICKSTER REFRESH SKILL UI : " + skill.skillName);
        }
        else
        {
            Debug.LogWarning("Trickster RefreshSkillUI failed | skillUI: " + skillUI + " | skill: " + skill);
        }
    }

    [ClientRpc]
    public void RefreshSkillUIClientRpc(ulong ownerId)
    {
        if (NetworkManager.Singleton.LocalClientId != ownerId)
            return;

        RefreshSkillUI();
    }
}