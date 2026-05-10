#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class PlayFromBootstrap
{
    private const string BootstrapScenePath =
        "Assets/Scenes/F_Scenes/Bootstrap.unity";

    static PlayFromBootstrap()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingEditMode)
            return;

        if (EditorSceneManager.GetActiveScene().path ==
            BootstrapScenePath)
            return;

        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        EditorSceneManager.OpenScene(BootstrapScenePath);
    }
}
#endif