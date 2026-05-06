using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    public float scrollSpeed = 3f;
    public bool isGameOver = false;

    private float panelWidth;
    private Vector3 startPos;
    private Transform clone;

    void Start()
    {
        panelWidth = GetComponent<SpriteRenderer>().bounds.size.x;
        startPos = transform.position;

        // Clone ตัวเองไปวางต่อทางขวาทันที
        clone = Instantiate(gameObject, new Vector3(
            startPos.x + panelWidth,
            startPos.y,
            startPos.z
        ), Quaternion.identity).transform;

        // ไม่ให้ Clone สร้าง Clone ซ้ำ
        Destroy(clone.GetComponent<BackgroundScroller>());
    }

    void Update()
    {
        if (isGameOver) return;

        float move = scrollSpeed * Time.deltaTime;
        transform.position += Vector3.left * move;
        clone.position += Vector3.left * move;

        // ถ้าหลุดซ้ายให้วนกลับไปต่อท้าย Clone
        if (transform.position.x + panelWidth / 2f < startPos.x - panelWidth)
        {
            transform.position = new Vector3(
                clone.position.x + panelWidth,
                transform.position.y,
                transform.position.z
            );
        }

        if (clone.position.x + panelWidth / 2f < startPos.x - panelWidth)
        {
            clone.position = new Vector3(
                transform.position.x + panelWidth,
                clone.position.y,
                clone.position.z
            );
        }
    }

    public void StopScroll() => isGameOver = true;
    public void StartScroll() => isGameOver = false;
}
    