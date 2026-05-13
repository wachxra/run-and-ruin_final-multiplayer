using UnityEngine;

public class DestroyAfterAnimation : MonoBehaviour
{
    private float destroyTime = 1f;

    public void SetDestroyTime(float time)
    {
        destroyTime = time;
    }

    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }

}