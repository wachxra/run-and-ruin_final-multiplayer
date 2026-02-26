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

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();

        if (animator == null)
            Debug.LogError("Animator not found on Runner");
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
            float height = Mathf.Sin(progress * Mathf.PI) * jumpHeight;

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

    private SpriteRenderer sprite;
    private bool isSlowed = false;
    private float actionSpeedMultiplier = 1f;

    private void OnEnable()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
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
}