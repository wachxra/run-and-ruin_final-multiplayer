using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class RoleUIController : MonoBehaviour
{
    public GameObject runnerUI;
    public GameObject tricksterUI;

    void Start()
    {
        StartCoroutine(SetupUI());
    }

    IEnumerator SetupUI()
    {
        while (GameFlowManager.Instance == null)
            yield return null;

        while (!GameFlowManager.Instance.isRoleDecided.Value)
            yield return null;

        ulong myId = NetworkManager.Singleton.LocalClientId;

        bool isRunner =
            myId == GameFlowManager.Instance.firstRunnerClientId.Value;

        runnerUI.SetActive(isRunner);
        tricksterUI.SetActive(!isRunner);

        while (GameFlowManager.Instance.countdownValue.Value > 0)
            yield return null;

        runnerUI.SetActive(false);
        tricksterUI.SetActive(false);
    }
}