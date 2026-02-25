/*using Unity.Netcode;
using UnityEngine;

public class TricksterController : NetworkBehaviour
{
    [SerializeField] private GameObject tricksterUI;

    private NetworkPlayer localPlayer;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        localPlayer = GetComponent<NetworkPlayer>();

        if (localPlayer.currentRole.Value == CharacterRole.Trickster)
        {
            tricksterUI.SetActive(true);
        }
        else
        {
            tricksterUI.SetActive(false);
        }
    }
}*/