using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class TricksterSkillController : NetworkBehaviour
{
    public GameObject projectilePrefab;

    [Header("Pirate King Prefabs")]
    public GameObject cannonBallPrefab;
    public GameObject bombPrefab;
    public GameObject knifePrefab;

    [Header("Bartender Prefabs")]
    public GameObject glassPrefab;
    public GameObject barrelPrefab;
    public GameObject bartenderKnifePrefab;

    [Header("Scientist Prefabs")]
    public GameObject poisonBottlePrefab;
    public GameObject chemicalBombPrefab;
    public GameObject poisonNeedlePrefab;

    [Header("Speedster Prefabs")]
    public GameObject fastStarPrefab;
    public GameObject fastEnergyPrefab;
    public GameObject fastKnifePrefab;

    [Header("Shadow Prefabs")]
    public GameObject shadowBallPrefab;
    public GameObject shadowTrapPrefab;
    public GameObject shadowKnifePrefab;

    public Transform[] firePoints;

    [Header("Skill")]
    public CharacterSkillSO skill;
    private SkillFeedbackController feedback;

    private float lastSkillTime = -999f;

    [Header("Projectile Cooldown")]
    public float projectileCooldown = 1f;

    private float lastProjectileSkillTime = -999f;

    [Header("Speedster Skill")]
    public float rapidFireCooldownMultiplier = 0.5f;
    public float rapidProjectileSpeedMultiplier = 2f;
    private bool isRapidFireActive = false;

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

    private void Awake()
    {
        feedback = GetComponent<SkillFeedbackController>();
    }

    [ServerRpc]
    void UseSpecialSkillServerRpc()
    {
        if (skill == null) return;

        if (Time.time - lastSkillTime < skill.cooldown)
            return;

        if (skill.skillType == SkillType.Trap)
        {
            if (!HasAnyProjectile())
                return;
        }

        lastSkillTime = Time.time;
        PlayTricksterUltimateClientRpc();

        ActivateSkill();

        TriggerSkillCooldownClientRpc(
            OwnerClientId,
            skill.cooldown);
    }

    bool HasAnyProjectile()
    {
        SkillProjectile[] projectiles =
            FindObjectsByType<SkillProjectile>(FindObjectsSortMode.None);

        foreach (SkillProjectile projectile in projectiles)
        {
            if (projectile == null) continue;

            if (projectile.NetworkObject != null &&
                projectile.NetworkObject.IsSpawned)
            {
                return true;
            }
        }

        return false;
    }

    [ClientRpc]
    void PlayTricksterUltimateClientRpc()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("trickster_ultimate");
        }
    }

    [ClientRpc]
    void TriggerSkillCooldownClientRpc(ulong ownerId, float duration)
    {
        if (NetworkManager.Singleton.LocalClientId != ownerId)
            return;

        SkillUIController skillUI =
            FindFirstObjectByType<SkillUIController>(FindObjectsInactive.Include);

        if (skillUI != null)
        {
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
                BlurAllRunnerClientRpc(skill.duration);
                break;

            case SkillType.SlowAll:
                SlowAllRunners();
                break;

            case SkillType.MultiShot:
                StartCoroutine(RapidFireRoutine());
                break;

            case SkillType.Trap:
                StartCoroutine(HideProjectilesRoutine());
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

    void SlowAllRunners()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var runner =
                client.PlayerObject.GetComponentInChildren<RunnerController>();

            if (runner != null)
            {
                runner.ApplySlow(skill.duration);
            }
        }
    }

    IEnumerator RapidFireRoutine()
    {
        isRapidFireActive = true;

        yield return new WaitForSeconds(skill.duration);

        isRapidFireActive = false;
    }

    IEnumerator HideProjectilesRoutine()
    {
        SkillProjectile[] allProjectiles =
            FindObjectsByType<SkillProjectile>(FindObjectsSortMode.None);

        List<ulong> validProjectileIds = new List<ulong>();

        foreach (SkillProjectile projectile in allProjectiles)
        {
            if (projectile == null) continue;

            NetworkObject netObj = projectile.GetComponent<NetworkObject>();

            if (netObj == null || !netObj.IsSpawned)
                continue;

            validProjectileIds.Add(netObj.NetworkObjectId);
        }

        if (validProjectileIds.Count == 0)
            yield break;

        int randomIndex = Random.Range(0, validProjectileIds.Count);
        ulong targetId = validProjectileIds[randomIndex];

        Debug.Log("Hide Projectile ID: " + targetId);

        SetProjectileVisibleClientRpc(targetId, false);

        yield return new WaitForSeconds(skill.duration);

        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(targetId))
        {
            SetProjectileVisibleClientRpc(targetId, true);
        }
    }

    [ClientRpc]
    void SetProjectileVisibleClientRpc(ulong projectileNetworkObjectId, bool state)
    {
        if (NetworkManager.Singleton == null)
            return;

        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects
            .TryGetValue(projectileNetworkObjectId, out NetworkObject networkObject))
            return;

        Renderer[] renderers =
            networkObject.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = state;
        }
    }

    void SetAllProjectilesFreeze(bool state)
    {
        SkillProjectile[] projectiles =
            FindObjectsByType<SkillProjectile>(FindObjectsSortMode.None);

        foreach (SkillProjectile projectile in projectiles)
        {
            if (projectile != null)
            {
                projectile.SetFreeze(state);
            }
        }
    }

    [ClientRpc]
    void SetAllProjectilesVisibleClientRpc(bool state)
    {
        SkillProjectile[] projectiles =
            FindObjectsByType<SkillProjectile>(FindObjectsSortMode.None);

        foreach (SkillProjectile projectile in projectiles)
        {
            if (projectile == null) continue;

            SpriteRenderer sprite =
                projectile.GetComponentInChildren<SpriteRenderer>();

            if (sprite != null)
            {
                sprite.enabled = state;
            }
        }
    }

    [ClientRpc]
    void BlurAllRunnerClientRpc(float duration)
    {
        var blur = FindFirstObjectByType<ScreenBlurEffect>();

        if (blur != null)
        {
            blur.PlayBlur(duration);
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

        float currentProjectileCooldown = projectileCooldown;

        if (isRapidFireActive)
        {
            currentProjectileCooldown *= rapidFireCooldownMultiplier;
        }

        if (Time.time - lastProjectileSkillTime < currentProjectileCooldown)
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

        TriggerProjectileCooldownClientRpc(
            senderId,
            currentProjectileCooldown);

        SpawnProjectile(skillIndex - 1);
    }

    [ClientRpc]
    void PlayDeployItemClientRpc()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("deploy_items");
        }
    }

    void SpawnProjectile(int lane)
    {
        if (!IsServer) return;

        if (firePoints == null || firePoints.Length <= lane)
            return;

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
                prefabToSpawn = bartenderKnifePrefab;
        }
        else if (skill.skillType == SkillType.SlowAll)
        {
            if (lane == 0)
                prefabToSpawn = poisonBottlePrefab;

            else if (lane == 1)
                prefabToSpawn = chemicalBombPrefab;

            else
                prefabToSpawn = poisonNeedlePrefab;
        }
        else if (skill.skillType == SkillType.MultiShot)
        {
            if (lane == 0)
                prefabToSpawn = fastStarPrefab;

            else if (lane == 1)
                prefabToSpawn = fastEnergyPrefab;

            else
                prefabToSpawn = fastKnifePrefab;
        }
        else if (skill.skillType == SkillType.Trap)
        {
            if (lane == 0)
                prefabToSpawn = shadowBallPrefab;

            else if (lane == 1)
                prefabToSpawn = shadowTrapPrefab;

            else
                prefabToSpawn = shadowKnifePrefab;
        }

        if (prefabToSpawn == null)
            return;

        PlayDeployItemClientRpc();

        var obj = Instantiate(
            prefabToSpawn,
            spawnPoint.position,
            Quaternion.identity);

        var projectile = obj.GetComponent<SkillProjectile>();

        var effect = obj.GetComponent<ProjectileEffectData>();

        if (effect != null && projectile != null)
        {
            projectile.speed = effect.moveSpeed;
            projectile.damage = effect.damage;

            if (effect.followRunnerSpeed)
            {
                projectile.speed += 4f;
            }
        }

        if (projectile != null)
        {
            if (skill.skillType == SkillType.MultiShot &&
                isRapidFireActive)
            {
                projectile.speed *= rapidProjectileSpeedMultiplier;
            }
        }

        obj.GetComponent<NetworkObject>().Spawn();
    }

    [ClientRpc]
    void TriggerProjectileCooldownClientRpc(
    ulong ownerId,
    float duration)
    {
        if (NetworkManager.Singleton.LocalClientId != ownerId)
            return;

        ProjectileCooldownUI cooldownUI =
            FindFirstObjectByType<ProjectileCooldownUI>(
                FindObjectsInactive.Include);

        if (cooldownUI != null)
        {
            cooldownUI.StartCooldown(duration);
        }
    }
}