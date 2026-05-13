using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class RunnerController : NetworkBehaviour
{
    private Animator animator;

    private bool isJumping = false;
    private bool isSliding = false;
    private bool canJumpDuringSlide = false;

    private Vector3 startPosition;

    [Header("Jump Settings")]
    public float jumpHeight = 2f;
    public float jumpDuration = 0.5f;

    [Header("Slide Settings")]
    public float slideOffset = 0.5f;
    public float slideDuration = 0.5f;
    public float slideInputUnlockTime = 0.4f;

    [Header("Animation")]
    public float slideAnimationLength = 0.5f;

    [Header("Slide Collider")]
    public BoxCollider2D bodyCollider;
    public Vector2 normalColliderSize = new Vector2(1f, 2f);
    public Vector2 normalColliderOffset = new Vector2(0f, 0f);
    public Vector2 slideColliderSize = new Vector2(1f, 1f);
    public Vector2 slideColliderOffset = new Vector2(0f, -0.5f);

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

        if (bodyCollider == null)
            bodyCollider = GetComponent<BoxCollider2D>();
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
            if (!isJumping)
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

    [ClientRpc]
    void StopSlideForJumpClientRpc()
    {
        if (animator != null)
        {
            animator.speed = 1f;
            animator.ResetTrigger("Slide");
            animator.SetBool("Run", true);
        }
    }

    [ServerRpc]
    void JumpServerRpc()
    {
        if (isJumping) return;

        if (isSliding && !canJumpDuringSlide)
            return;

        if (isSliding)
        {
            StopSlideForJumpClientRpc();
            SetSlideCollider(false);
            isSliding = false;
        }

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
        if (animator == null) return;

        float animSpeed =
            slideAnimationLength / slideDuration;

        animator.speed = animSpeed;

        animator.SetTrigger("Slide");
    }

    IEnumerator SlideRoutine()
    {
        isSliding = true;
        canJumpDuringSlide = false;

        SetSlideCollider(true);

        float timer = 0f;

        while (timer < slideDuration)
        {
            timer += Time.deltaTime * actionSpeedMultiplier;

            if (timer >= slideInputUnlockTime)
            {
                canJumpDuringSlide = true;
            }

            if (!isSliding)
                yield break;

            yield return null;
        }

        SetSlideCollider(false);

        if (animator != null)
        {
            animator.speed = 1f;
        }

        isSliding = false;
        canJumpDuringSlide = false;
    }

    void SetSlideCollider(bool slide)
    {
        if (bodyCollider == null) return;

        if (slide)
        {
            bodyCollider.size = slideColliderSize;
            bodyCollider.offset = slideColliderOffset;
        }
        else
        {
            bodyCollider.size = normalColliderSize;
            bodyCollider.offset = normalColliderOffset;
        }
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
        if (animator == null) return;

        if (state)
        {
            animator.speed = 0.5f;
        }
        else
        {
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

        ForceUpdateRunnerHUDClientRpc(
            OwnerClientId,
            currentHP.Value,
            maxHP
        );

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

    [ClientRpc]
    void ForceUpdateRunnerHUDClientRpc(
    ulong targetClientId,
    int current,
    int max)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId)
            return;

        if (RunnerHUD.Instance != null)
        {
            RunnerHUD.Instance.SetHearts(current, max);
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