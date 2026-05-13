using UnityEngine;

public class DestroyAfterAnimation : MonoBehaviour
{
    public float destroyTime = 1f;

    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }
}