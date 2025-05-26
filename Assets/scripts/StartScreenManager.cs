using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenManager : MonoBehaviour
{
    [SerializeField]
    private string sceneToLoad;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    public void StartGame()
    {
        if (sceneToLoad != null)
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("No scene assigned to load. Please fill in the name of a scene");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game called (does not work in the editor).");
    }
}