using System.Collections.Generic;
using UnityEngine;

public class EndlessBackgroundChunk : MonoBehaviour
{
    [Header("Chunk")]
    public GameObject chunkPrefab;

    [Header("Settings")]
    public float moveSpeed = 3f;
    public int startChunks = 3;

    [Header("Position")]
    public Vector2 spawnOffset;

    private readonly List<GameObject> chunks =
        new List<GameObject>();

    private float chunkWidth;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        if (chunkPrefab == null)
        {
            return;
        }

        chunkWidth = GetChunkWidth();

        for (int i = 0; i < startChunks; i++)
        {
            SpawnChunk(i * chunkWidth);
        }
    }

    private void Update()
    {
        MoveChunks();
        SpawnNewChunk();
        RemoveOldChunk();
    }

    void MoveChunks()
    {
        foreach (GameObject chunk in chunks)
        {
            if (chunk != null)
            {
                chunk.transform.position +=
                    Vector3.left *
                    moveSpeed *
                    Time.deltaTime;
            }
        }
    }

    void SpawnNewChunk()
    {
        if (chunks.Count == 0) return;

        GameObject lastChunk =
            chunks[chunks.Count - 1];

        float camRight =
            mainCamera.transform.position.x +
            mainCamera.orthographicSize *
            mainCamera.aspect;

        float lastRight =
            lastChunk.transform.position.x +
            chunkWidth / 2f;

        if (lastRight < camRight)
        {
            SpawnChunk(
                lastChunk.transform.position.x +
                chunkWidth
            );
        }
    }

    void RemoveOldChunk()
    {
        if (chunks.Count == 0) return;

        float camLeft =
            mainCamera.transform.position.x -
            mainCamera.orthographicSize *
            mainCamera.aspect;

        for (int i = chunks.Count - 1; i >= 0; i--)
        {
            GameObject chunk = chunks[i];

            if (chunk == null)
            {
                chunks.RemoveAt(i);
                continue;
            }

            float rightEdge =
                chunk.transform.position.x +
                chunkWidth / 2f;

            if (rightEdge < camLeft - 2f)
            {
                chunks.RemoveAt(i);
                Destroy(chunk);
            }
        }
    }

    void SpawnChunk(float xPos)
    {
        GameObject chunk = Instantiate(
            chunkPrefab,
            new Vector3(
                xPos + spawnOffset.x,
                transform.position.y + spawnOffset.y,
                transform.position.z),
            Quaternion.identity
        );

        chunk.transform.SetParent(transform);

        chunks.Add(chunk);
    }

    float GetChunkWidth()
    {
        SpriteRenderer[] renderers =
            chunkPrefab.GetComponentsInChildren<SpriteRenderer>();

        if (renderers.Length == 0)
        {
            return 20f;
        }

        Bounds bounds = renderers[0].bounds;

        foreach (SpriteRenderer sr in renderers)
        {
            bounds.Encapsulate(sr.bounds);
        }

        return bounds.size.x;
    }
}