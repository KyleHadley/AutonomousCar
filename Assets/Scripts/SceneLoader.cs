using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {

    private void Start()
    {
        SceneManager.GetActiveScene();
    }

    // Loads the current scene of whatever scene name is input
    public void loadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Quits the game.
    /// </summary>
    static public void QuitGame()
    {
        UnityEngine.Application.Quit();
    }
}
