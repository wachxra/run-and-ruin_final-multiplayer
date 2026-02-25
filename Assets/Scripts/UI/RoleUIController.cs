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

        if (myId ==
            GameFlowManager.Instance.firstRunnerClientId.Value)
        {
            runnerUI.SetActive(true);
            tricksterUI.SetActive(false);
        }
        else
        {
            runnerUI.SetActive(false);
            tricksterUI.SetActive(true);
        }
    }
}