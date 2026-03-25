using UnityEngine;
using Unity.Netcode;

public class SkillProjectile : NetworkBehaviour
{
    public float speed = 10f;
    public int damage = 1;
    public float lifeTime = 5f;

    private bool hasHit = false;

    private void Start()
    {
        if (IsServer)
            Invoke(nameof(DespawnSelf), lifeTime);
    }

    private void Update()
    {
        if (!IsServer) return;

        transform.Translate(Vector2.left * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;
        if (hasHit) return;

        var runner = other.GetComponentInParent<RunnerController>();

        if (runner != null)
        {
            if (runner.NetworkObject == null || !runner.NetworkObject.IsSpawned)
                return;

            hasHit = true;

            runner.TakeDamage(damage);

            DespawnSelf();
        }
    }

    void DespawnSelf()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }
}