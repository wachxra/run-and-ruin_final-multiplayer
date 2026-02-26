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

            elapsed += Time.deltaTime;
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

        yield return new WaitForSeconds(slideDuration);

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
}