using UnityEngine;

public class SceneUIButtons : MonoBehaviour
{
    public void Restart()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.RestartGame();
        }
        else
        {
            Debug.LogWarning("SceneLoader not found");
        }
    }

    public void Back()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.BackToMenu();
        }
        else
        {
            Debug.LogWarning("SceneLoader not found");
        }
    }
}