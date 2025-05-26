using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitch : MonoBehaviour
{

    [SerializeField]
    private string sceneName; // Stel de naam van de sc�ne in via de Inspector

    private void Start()
    {
        PlayerController.hasKey = false;
    }

    void OnTriggerEnter(Collider other)
    {
        // Controleer of het de speler is die de trigger raakt
        if (other.CompareTag("Player"))
        {
            // Controleer of de speler de sleutel heeft
            if (PlayerController.hasKey)
            {
                // Laad de sc�ne als de speler de sleutel heeft
                if (!string.IsNullOrEmpty(sceneName))
                {
                    SceneManager.LoadScene(sceneName);
                }
                else
                {
                    Debug.LogWarning("Geen scene ingesteld in de Inspector!");
                }
            }
            else
            {
                Debug.Log("De speler heeft geen sleutel. Sc�ne wordt niet geladen.");
            }
        }
    }
}