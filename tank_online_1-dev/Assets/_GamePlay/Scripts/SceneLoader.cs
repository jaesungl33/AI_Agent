using UnityEngine;

public class SceneLoader : Singleton<SceneLoader>
{
    protected override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// Loads a scene by name.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load.</param>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name cannot be null or empty.");
            return;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        Debug.Log($"Loading scene: {sceneName}");
    }

    /// <summary>
    /// Reloads the current active scene.
    /// </summary>
    public void ReloadCurrentScene()
    {
        var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        LoadScene(currentScene.name);
        Debug.Log($"Reloading current scene: {currentScene.name}");
    }
}
