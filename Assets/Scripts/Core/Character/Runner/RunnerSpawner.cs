using Unity.Netcode;
using UnityEngine;

public class RunnerSpawner : NetworkBehaviour
{
    public GameObject[] runnerPrefabs;
    public Transform spawnPoint;

    public void SpawnRunnerFor(ulong clientId, int characterIndex)
    {
        if (!IsServer) return;

        GameObject prefab = runnerPrefabs[characterIndex];

        GameObject obj =
            Instantiate(prefab, spawnPoint.position, Quaternion.identity);

        obj.GetComponent<NetworkObject>()
            .SpawnAsPlayerObject(clientId);
    }
}