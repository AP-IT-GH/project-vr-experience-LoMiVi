using UnityEngine;
using UnityEngine.SceneManagement;

public class KeyGameState : MonoBehaviour
{
    public string nextScene;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over!");
        SceneManager.LoadScene(nextScene);
    }
}