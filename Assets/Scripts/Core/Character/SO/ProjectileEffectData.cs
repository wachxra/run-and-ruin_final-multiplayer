using UnityEngine;

public class ProjectileEffectData : MonoBehaviour
{
    [Header("Projectile")]
    public float moveSpeed = 10f;

    public int damage = 1;

    [Header("Effect")]
    public bool followRunnerSpeed = false;
}