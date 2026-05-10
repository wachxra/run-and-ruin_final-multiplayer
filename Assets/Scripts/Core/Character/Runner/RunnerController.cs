using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class RunnerController : NetworkBehaviour
{
    private Animator animator;

    private bool isJumping = false;
    private bool isSliding = false;

    private Vector3 startPosition;

    [Header("Jump Settings")]
    public float jumpHeight = 2f;
    public float jumpDuration = 0.5f;

    [Header("Slide Settings")]
    public float slideOffset = 0.5f;
    public float slideDuration = 0.5f;

    [Header("Skill")]
    public CharacterSkillSO skill;

    private float lastSkillTime = -999f;

    private bool hasShield = false;
    private bool hasReflect = false;
    private bool isInvisible = false;

    private int blockedLane = -1;

    private SpriteRenderer sprite;

    private bool isSlowed = false;
    private float actionSpeedMultiplier = 1f;

    [Header("Health")]
    public int maxHP = 3;

    public NetworkVariable<int> currentHP =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();

        if (animator == null)
            Debug.LogError("Animator not found on Runner");
    }

    private void OnEnable()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        startPosition = transform.position;

        if (animator != null)
            animator.SetBool("Run", true);
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (GameFlowManager.Instance == null ||
            (GameFlowManager.Instance.phase.Value != GamePhase.Round1 &&
             GameFlowManager.Instance.phase.Value != GamePhase.Round2))
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isJumping && !isSliding)
                JumpServerRpc();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (!isSliding && !isJumping)
                SlideServerRpc();
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            TryUseSkillServerRpc();
        }
    }

    [ServerRpc]
    void TryUseSkillServerRpc()
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
            case SkillType.StopTime:
                StartCoroutine(StopTimeRoutine());
                break;

            case SkillType.ScreenBlock:
                BlockRandomLaneClientRpc();
                break;

            case SkillType.Reflect:
                if (!hasReflect)
                {
                    hasReflect = true;
                    StartCoroutine(ReflectRoutine());
                }
                break;

            case SkillType.Shield:
                if (!hasShield)
                {
                    hasShield = true;
                    StartCoroutine(ShieldRoutine());
                }
                break;

            case SkillType.Invisible:
                if (!isInvisible)
                {
                    StartCoroutine(InvisibleRoutine());
                }
                break;
        }
    }

    IEnumerator StopTimeRoutine()
    {
        var allProjectiles = FindObjectsByType<SkillProjectile>(
            FindObjectsSortMode.None);

        foreach (var p in allProjectiles)
        {
            p.SetFreeze(true);
        }

        yield return new WaitForSeconds(3f);

        foreach (var p in allProjectiles)
        {
            if (p != null)
                p.SetFreeze(false);
        }
    }

    IEnumerator ReflectRoutine()
    {
        yield return new WaitForSeconds(skill.duration);

        hasReflect = false;
    }

    IEnumerator ShieldRoutine()
    {
        yield return new WaitForSeconds(10f);

        hasShield = false;
    }

    IEnumerator InvisibleRoutine()
    {
        isInvisible = true;

        SetInvisibleClientRpc(true);

        yield return new WaitForSeconds(skill.duration);

        isInvisible = false;

        SetInvisibleClientRpc(false);
    }

    [ClientRpc]
    void SetInvisibleClientRpc(bool state)
    {
        if (sprite == null) return;

        if (IsOwner)
            return;

        Color c = sprite.color;

        c.a = state ? 0f : 1f;

        sprite.color = c;
    }

    [ClientRpc]
    void BlockRandomLaneClientRpc()
    {
        blockedLane = Random.Range(1, 4);

        Debug.Log("Blocked Lane : " + blockedLane);

        StartCoroutine(UnblockLaneRoutine());
    }

    IEnumerator UnblockLaneRoutine()
    {
        yield return new WaitForSeconds(3f);

        blockedLane = -1;

        Debug.Log("Lane Unblocked");
    }

    public bool IsLaneBlocked(int lane)
    {
        return blockedLane == lane;
    }

    public bool HasReflect()
    {
        return hasReflect;
    }

    [ServerRpc]
    void JumpServerRpc()
    {
        if (isJumping || isSliding) return;

        StartCoroutine(JumpRoutine());

        JumpClientRpc();
    }

    [ClientRpc]
    void JumpClientRpc()
    {
        if (animator != null)
            animator.SetTrigger("Jump");
    }

    IEnumerator JumpRoutine()
    {
        isJumping = true;

        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            float progress = elapsed / jumpDuration;

            float height =
                Mathf.Sin(progress * Mathf.PI) * jumpHeight;

            transform.position = new Vector3(
                startPosition.x,
                startPosition.y + height,
                startPosition.z
            );

            elapsed += Time.deltaTime * actionSpeedMultiplier;

            yield return null;
        }

        transform.position = startPosition;

        isJumping = false;
    }

    [ServerRpc]
    void SlideServerRpc()
    {
        if (isSliding || isJumping) return;

        StartCoroutine(SlideRoutine());

        SlideClientRpc();
    }

    [ClientRpc]
    void SlideClientRpc()
    {
        if (animator != null)
            animator.SetTrigger("Slide");
    }

    IEnumerator SlideRoutine()
    {
        isSliding = true;

        transform.position = new Vector3(
            startPosition.x,
            startPosition.y - slideOffset,
            startPosition.z
        );

        float timer = 0f;

        while (timer < slideDuration)
        {
            timer += Time.deltaTime * actionSpeedMultiplier;

            yield return null;
        }

        transform.position = startPosition;

        isSliding = false;
    }

    public void ForceJump()
    {
        if (!IsServer) return;

        if (isJumping || isSliding) return;

        StartCoroutine(JumpRoutine());

        JumpClientRpc();
    }

    public void ForceSlide()
    {
        if (!IsServer) return;

        if (isSliding || isJumping) return;

        StartCoroutine(SlideRoutine());

        SlideClientRpc();
    }

    public void ApplySlow(float duration)
    {
        if (!IsServer) return;

        if (isSlowed) return;

        StartCoroutine(SlowRoutine(duration));
    }

    IEnumerator SlowRoutine(float duration)
    {
        isSlowed = true;

        actionSpeedMultiplier = 0.5f;

        ApplySlowVisualClientRpc(true);

        yield return new WaitForSeconds(duration);

        actionSpeedMultiplier = 1f;

        ApplySlowVisualClientRpc(false);

        isSlowed = false;
    }

    [ClientRpc]
    void ApplySlowVisualClientRpc(bool state)
    {
        if (sprite == null) return;

        if (state)
        {
            sprite.color = Color.black;

            if (animator != null)
                animator.speed = 0.5f;
        }
        else
        {
            sprite.color = Color.white;

            if (animator != null)
                animator.speed = 1f;
        }
    }

    void OnHPChanged(int oldHP, int newHP)
    {
        if (RunnerHUD.Instance != null)
        {
            RunnerHUD.Instance.SetHearts(newHP, maxHP);
        }
    }

    public void TakeDamage(int dmg)
    {
        if (!IsServer) return;

        if (hasShield)
        {
            hasShield = false;
            return;
        }

        if (NetworkObject == null || !NetworkObject.IsSpawned)
            return;

        currentHP.Value -= dmg;

        if (currentHP.Value < 0)
            currentHP.Value = 0;

        if (currentHP.Value <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        GameFlowManager.Instance.RecordRunnerTime(OwnerClientId);

        GameFlowManager.Instance.EndRound();

        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            currentHP.Value = maxHP;

        currentHP.OnValueChanged += OnHPChanged;

        OnHPChanged(0, currentHP.Value);

        if (IsOwner)
        {
            if (RunnerHUD.Instance != null)
                RunnerHUD.Instance.ShowHUD(true);
        }
    }
}