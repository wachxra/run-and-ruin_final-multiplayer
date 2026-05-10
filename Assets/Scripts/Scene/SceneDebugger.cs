using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDebugger : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
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
        Debug.Log($"[SCENE LOADED] {scene.name} | Mode: {mode}");
        Debug.Log(System.Environment.StackTrace);
    }
}