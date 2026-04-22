using UnityEngine;
using Unity.Netcode;

public class TricksterSkillController : NetworkBehaviour
{
    public GameObject projectilePrefab;
    public Transform[] firePoints;

    void Update()
    {
        if (!IsOwner) return;

        var player = GetComponent<NetworkPlayer>();
        if (player == null || player.currentRole.Value != CharacterRole.Trickster)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            UseSkillServerRpc(1);

        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            UseSkillServerRpc(2);

        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            UseSkillServerRpc(3);
    }

    [ServerRpc]
    void UseSkillServerRpc(int skillIndex, ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        var senderPlayer = NetworkManager.Singleton
            .ConnectedClients[senderId]
            .PlayerObject
            .GetComponent<NetworkPlayer>();

        if (senderPlayer.currentRole.Value != CharacterRole.Trickster)
            return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var player = client.PlayerObject.GetComponent<NetworkPlayer>();

            if (player.currentRole.Value == CharacterRole.Runner)
            {
                var runner = client.PlayerObject
                    .GetComponentInChildren<RunnerController>();

                if (runner == null) continue;

                if (skillIndex == 1)
                    SpawnProjectile(0);
                else if (skillIndex == 2)
                    SpawnProjectile(1);
                else if (skillIndex == 3)
                    SpawnProjectile(2);
            }
        }
    }

    void SpawnProjectile(int lane)
    {
        if (!IsServer) return;

        var spawnPoint = firePoints[lane];

        var obj = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
        obj.GetComponent<NetworkObject>().Spawn();
    }
}