using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SceneLoader : NetworkBehaviour
{
    /*public static SceneLoader Instance;

    private Stack<string> sceneHistory = new Stack<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single)
        {
            if (sceneHistory.Count == 0 || sceneHistory.Peek() != scene.name)
            {
                sceneHistory.Push(scene.name);
            }
        }
    }

    public void RestartGame()
    {
        if (IsServer)
        {
            RestartGameClientRpc();
        }
        else
        {
            RestartGameServerRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void RestartGameServerRpc()
    {
        RestartGameClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void RestartGameClientRpc()
    {
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.IsServer)
        {
            GameFlowManager.Instance.ResetGameFlow();
        }

        sceneHistory.Clear();
        SceneManager.LoadScene("CharacterSelect");
    }


    public void GoBack()
    {
        if (IsServer)
        {
            HandleBack();
        }
        else
        {
            GoBackServerRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void GoBackServerRpc()
    {
        HandleBack();
    }

    private void HandleBack()
    {
        string current = SceneManager.GetActiveScene().name;
        string targetScene = "Menu";

        if (current == "CharacterSelect")
        {
            targetScene = "Menu";
        }
        else if (sceneHistory.Count > 1)
        {
            sceneHistory.Pop();
            targetScene = sceneHistory.Pop();
        }

        LoadSceneClientRpc(targetScene);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void LoadSceneClientRpc(string sceneName)
    {
        sceneHistory.Clear();
        SceneManager.LoadScene(sceneName);
    }*/
}