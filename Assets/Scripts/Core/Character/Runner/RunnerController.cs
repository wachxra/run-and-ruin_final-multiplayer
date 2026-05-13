using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class RunnerController : NetworkBehaviour
{
    private Animator animator;

    private bool isJumping = false;
    private bool isSliding = false;
    private bool canJumpDuringSlide = false;
    private bool isSlideHeld = false;

    private Coroutine slideRoutine;
    private Coroutine slidePauseRoutine;

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
    public float slidePauseTime = 0.25f;

    [Header("Slide Collider")]
    public BoxCollider2D bodyCollider;
    public Vector2 normalColliderSize = new Vector2(1f, 2f);
    public Vector2 normalColliderOffset = new Vector2(0f, 0f);
    public Vector2 slideColliderSize = new Vector2(1f, 1f);
    public Vector2 slideColliderOffset = new Vector2(0f, -0.5f);

    [Header("Skill")]
    public CharacterSkillSO skill;
    private SkillFeedbackController feedback;

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

        feedback = GetComponent<SkillFeedbackController>();

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
                StartSlideServerRpc();
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            if (isSliding)
                StopSlideServerRpc();
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

        PlayRunnerUltimateClientRpc();

        ActivateSkill();

        TriggerSkillCooldownClientRpc(
            OwnerClientId,
            skill.cooldown);
    }

    [ClientRpc]
    void PlayRunnerUltimateClientRpc()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("runner_ultimate");
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
            case SkillType.StopTime:
                if (feedback != null)
                    feedback.PlayRunnerSkillStart(skill.skillType, skill.duration);

                StartCoroutine(StopTimeRoutine());
                break;

            case SkillType.ScreenBlock:
                if (feedback != null)
                {
                    feedback.PlayRunnerSkillStart(
                        skill.skillType,
                        skill.duration);
                }

                BlockRandomLane();
                break;

            case SkillType.Reflect:
                if (!hasReflect)
                {
                    hasReflect = true;

                    if (feedback != null)
                        feedback.PlayRunnerSkillStart(skill.skillType, skill.duration);

                    StartCoroutine(ReflectRoutine());
                }
                break;

            case SkillType.Shield:
                if (!hasShield)
                {
                    hasShield = true;

                    if (feedback != null)
                        feedback.PlayRunnerSkillStart(skill.skillType, skill.duration);

                    StartCoroutine(ShieldRoutine());
                }
                break;

            case SkillType.Invisible:
                if (!isInvisible)
                {
                    if (feedback != null)
                        feedback.PlayRunnerSkillStart(skill.skillType, skill.duration);

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

        if (feedback != null)
            feedback.PlayRunnerSkillEnd(SkillType.Reflect);
    }

    IEnumerator ShieldRoutine()
    {
        yield return new WaitForSeconds(10f);

        hasShield = false;

        if (feedback != null)
            feedback.PlayRunnerSkillEnd(SkillType.Shield);
    }

    IEnumerator InvisibleRoutine()
    {
        isInvisible = true;

        SetInvisibleClientRpc(true);

        yield return new WaitForSeconds(skill.duration);

        isInvisible = false;

        SetInvisibleClientRpc(false);

        if (feedback != null)
            feedback.PlayRunnerSkillEnd(SkillType.Invisible);
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

    void BlockRandomLane()
    {
        blockedLane = Random.Range(1, 4);

        Debug.Log("Blocked Lane : " + blockedLane);

        if (feedback != null)
        {
            feedback.PlayScreenBlockLane(
                blockedLane,
                skill.duration);
        }

        BlockLaneClientRpc(blockedLane);

        StartCoroutine(UnblockLaneRoutine());
    }

    [ClientRpc]
    void BlockLaneClientRpc(int lane)
    {
        blockedLane = lane;
    }

    IEnumerator UnblockLaneRoutine()
    {
        yield return new WaitForSeconds(skill.duration);

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
        if (slidePauseRoutine != null)
        {
            StopCoroutine(slidePauseRoutine);
            slidePauseRoutine = null;
        }

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
            isSlideHeld = false;

            StopSlideForJumpClientRpc();

            SetSlideCollider(false);

            isSliding = false;
            canJumpDuringSlide = false;
        }

        StartCoroutine(JumpRoutine());

        JumpClientRpc();
    }

    [ClientRpc]
    void JumpClientRpc()
    {
        if (animator != null)
            animator.SetTrigger("Jump");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("jump");
        }
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
    void StartSlideServerRpc()
    {
        if (isSliding || isJumping) return;

        isSlideHeld = true;

        slideRoutine = StartCoroutine(SlideRoutine());

        StartSlideClientRpc();
    }

    [ServerRpc]
    void StopSlideServerRpc()
    {
        if (!isSliding) return;

        isSlideHeld = false;

        ResumeSlideClientRpc();
    }

    [ClientRpc]
    void StartSlideClientRpc()
    {
        isSliding = true;
        isSlideHeld = true;
        SetSlideCollider(true);

        if (animator == null) return;

        if (slidePauseRoutine != null)
        {
            StopCoroutine(slidePauseRoutine);
            slidePauseRoutine = null;
        }

        animator.speed = 1f;
        animator.SetTrigger("Slide");

        slidePauseRoutine = StartCoroutine(PauseSlideAnimationRoutine());
    }

    IEnumerator PauseSlideAnimationRoutine()
    {
        float timer = 0f;

        while (timer < slidePauseTime)
        {
            timer += Time.deltaTime * actionSpeedMultiplier;
            yield return null;
        }

        if (animator != null)
        {
            animator.speed = 0f;
        }

        slidePauseRoutine = null;
    }

    [ClientRpc]
    void ResumeSlideClientRpc()
    {
        isSlideHeld = false;
        SetSlideCollider(false);

        if (slidePauseRoutine != null)
        {
            StopCoroutine(slidePauseRoutine);
            slidePauseRoutine = null;
        }

        if (animator != null)
        {
            animator.speed = 1f;
        }

        isSliding = false;
        canJumpDuringSlide = false;
    }

    IEnumerator SlideRoutine()
    {
        isSliding = true;
        canJumpDuringSlide = false;

        SetSlideCollider(true);

        float unlockTimer = 0f;

        while (isSlideHeld)
        {
            unlockTimer += Time.deltaTime * actionSpeedMultiplier;

            if (unlockTimer >= slideInputUnlockTime)
            {
                canJumpDuringSlide = true;
            }

            yield return null;
        }

        SetSlideCollider(false);

        isSliding = false;
        canJumpDuringSlide = false;

        slideRoutine = null;
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

        isSlideHeld = true;

        slideRoutine = StartCoroutine(ForceSlideRoutine());

        StartSlideClientRpc();
    }

    IEnumerator ForceSlideRoutine()
    {
        yield return SlideRoutine();

        ResumeSlideClientRpc();
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

        if (feedback != null)
            feedback.PlaySlowStart(duration);

        ApplySlowVisualClientRpc(true);

        yield return new WaitForSeconds(duration);

        actionSpeedMultiplier = 1f;

        ApplySlowVisualClientRpc(false);

        if (feedback != null)
            feedback.PlaySlowEnd();

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

            if (feedback != null)
                feedback.PlayShieldBreak();

            return;
        }

        if (NetworkObject == null || !NetworkObject.IsSpawned)
            return;

        PlayGetHitClientRpc();
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

    [ClientRpc]
    void PlayGetHitClientRpc()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("get_hit");
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