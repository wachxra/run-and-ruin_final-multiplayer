using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SkillProjectile : NetworkBehaviour
{
    public static readonly List<SkillProjectile> ActiveProjectiles =
        new List<SkillProjectile>();

    public float speed = 10f;
    public int damage = 1;
    public float lifeTime = 5f;

    private bool hasHit = false;

    private bool isStopped = false;
    private bool isReflected = false;

    private bool canHitTrickster = false;

    private bool applySlow = false;
    private float slowDuration = 3f;

    public bool IsHiddenByShadow { get; private set; }

    public override void OnNetworkSpawn()
    {
        if (IsServer && !ActiveProjectiles.Contains(this))
        {
            ActiveProjectiles.Add(this);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            ActiveProjectiles.Remove(this);
        }
    }

    public void SetShadowHidden(bool state)
    {
        IsHiddenByShadow = state;
    }

    public void SetFreeze(bool state)
    {
        isStopped = state;
    }

    public void SetSlow(float duration)
    {
        applySlow = true;
        slowDuration = duration;
    }

    public void Reflect()
    {
        isReflected = true;
        canHitTrickster = true;
    }

    private void Start()
    {
        if (IsServer)
            Invoke(nameof(DespawnSelf), lifeTime);
    }

    private void Update()
    {
        if (!IsServer) return;

        if (isStopped) return;

        Vector2 dir = isReflected ? Vector2.right : Vector2.left;

        transform.Translate(dir * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (hasHit) return;

        var runner = other.GetComponentInParent<RunnerController>();

        if (runner != null)
        {
            if (runner.skill != null)
            {
                if (runner.skill.hitVFX != null)
                {
                    Instantiate(
                        runner.skill.hitVFX,
                        transform.position,
                        Quaternion.identity);
                }
            }

            if (runner.NetworkObject == null ||
                !runner.NetworkObject.IsSpawned)
                return;

            if (runner.HasReflect())
            {
                Reflect();

                SkillFeedbackController feedback =
                    runner.GetComponent<SkillFeedbackController>();

                if (feedback != null)
                    feedback.PlayReflectHit(transform.position);

                return;
            }

            hasHit = true;

            if (applySlow)
            {
                runner.ApplySlow(slowDuration);
            }

            runner.TakeDamage(damage);

            DespawnSelf();

            return;
        }

        if (canHitTrickster)
        {
            var trickster = other.GetComponentInParent<NetworkPlayer>();

            if (trickster != null)
            {
                if (trickster.currentRole.Value == CharacterRole.Trickster)
                {
                    hasHit = true;

                    DespawnSelf();
                }
            }
        }
    }

    void DespawnSelf()
    {
        if (NetworkObject != null &&
            NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }
}