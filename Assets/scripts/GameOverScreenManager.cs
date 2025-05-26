using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreenManager : MonoBehaviour
{
    public static string LastSceneName;

    [SerializeField]
    private string mainMenuScene;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    public void TryAgain()
    {
        if (LastSceneName != null)
        {
            SceneManager.LoadScene(LastSceneName);
        }
        else
        {
            Debug.LogError("No scene assigned to load. Please fill in the name of a scene");
        }
    }

    public void MainMenu()
    {
        if (mainMenuScene != null)
        {
            SceneManager.LoadScene(mainMenuScene);
        }
        else
        {
            Debug.LogError("No scene assigned to load. Please fill in the name of a scene");
        }
    }
}
