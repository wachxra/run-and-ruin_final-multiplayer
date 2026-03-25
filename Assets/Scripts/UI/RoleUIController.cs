using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class RoleUIController : MonoBehaviour
{
    public GameObject runnerUI;
    public GameObject tricksterUI;

    private NetworkPlayer myPlayer;
    private bool uiShownThisRound = false;

    void Start()
    {
        StartCoroutine(UIFlowLoop());
    }

    IEnumerator UIFlowLoop()
    {
        while (true)
        {
            yield return new WaitUntil(() => GameFlowManager.Instance != null);

            yield return new WaitUntil(() =>
            {
                myPlayer = NetworkManager.Singleton.SpawnManager
                    .GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId)?
                    .GetComponent<NetworkPlayer>();
                return myPlayer != null;
            });

            yield return new WaitUntil(() => GameFlowManager.Instance.isRoleDecided.Value);

            if (!uiShownThisRound)
            {
                runnerUI.SetActive(myPlayer.currentRole.Value == CharacterRole.Runner);
                tricksterUI.SetActive(myPlayer.currentRole.Value == CharacterRole.Trickster);

                uiShownThisRound = true;

                yield return new WaitUntil(() => GameFlowManager.Instance.countdownValue.Value == 0);

                runnerUI.SetActive(false);
                tricksterUI.SetActive(false);
            }

            yield return new WaitUntil(() => GameFlowManager.Instance.phase.Value == GamePhase.RoundEnd);

            uiShownThisRound = false;
        }
    }
}