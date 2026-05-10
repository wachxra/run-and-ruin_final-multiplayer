#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class PlayFromBootstrap
{
    private const string BootstrapScenePath =
        "Assets/Scenes/F_Scenes/Bootstrap.unity";

    private const string LastSceneKey =
        "PlayFromBootstrap_LastScenePath";

    static PlayFromBootstrap()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            string currentScenePath =
                EditorSceneManager.GetActiveScene().path;

            if (currentScenePath == BootstrapScenePath)
                return;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorApplication.isPlaying = false;
                return;
            }

            EditorPrefs.SetString(LastSceneKey, currentScenePath);

            EditorSceneManager.OpenScene(BootstrapScenePath);
        }

        if (state == PlayModeStateChange.EnteredEditMode)
        {
            string lastScenePath =
                EditorPrefs.GetString(LastSceneKey, "");

            if (string.IsNullOrEmpty(lastScenePath))
                return;

            if (EditorSceneManager.GetActiveScene().path == lastScenePath)
                return;

            EditorSceneManager.OpenScene(lastScenePath);

            EditorPrefs.DeleteKey(LastSceneKey);
        }
    }
}
#endif