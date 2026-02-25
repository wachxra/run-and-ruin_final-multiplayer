using Unity.Collections;
using Unity.Netcode;

public class NetworkPlayer : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> playerName =
        new NetworkVariable<FixedString32Bytes>();

    public NetworkVariable<int> selectedRunnerIndex =
        new NetworkVariable<int>(-1);

    public NetworkVariable<int> selectedTricksterIndex =
        new NetworkVariable<int>(-1);

    public NetworkVariable<bool> isReady =
        new NetworkVariable<bool>(false);

    public NetworkVariable<int> score =
        new NetworkVariable<int>(0);

    public NetworkVariable<CharacterRole> currentRole =
        new NetworkVariable<CharacterRole>();

    [ServerRpc]
    public void SetRunnerServerRpc(int index)
    {
        selectedRunnerIndex.Value = index;
    }

    [ServerRpc]
    public void SetTricksterServerRpc(int index)
    {
        selectedTricksterIndex.Value = index;
    }

    [ServerRpc]
    public void SetReadyServerRpc()
    {
        if (selectedRunnerIndex.Value < 0) return;
        if (selectedTricksterIndex.Value < 0) return;

        isReady.Value = true;

        CharacterSelectManager.Instance.CheckAllReady();
    }
}