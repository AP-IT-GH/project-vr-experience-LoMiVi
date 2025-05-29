using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public LayerMask visibilityMask;

    private Camera playerCamera;
    private void Start()
    {
        playerCamera = GetComponentInChildren<Camera>(true); // true = include inactive
        if (playerCamera != null && playerCamera.name == "MainCamera")
        {
            Debug.Log("Found MainCamera!");
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "enemy")
        {
            Debug.Log("Player hit enemy: Restarting level...");
            Restart();
        }
    }

    void Update()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("enemy");

        foreach (GameObject enemy in enemies)
        {
            if (IsVisible(enemy))
            {
                // Example: call a method on a script attached to the enemy
                StatueAgent statueAgent = enemy.GetComponent<StatueAgent>();
                if (statueAgent != null)
                {
                    statueAgent.FreezeMovement();
                }
            }
            else
            {
                StatueAgent statueAgent = enemy.GetComponent<StatueAgent>();
                if (statueAgent != null)
                {
                    statueAgent.UnfreezeMovement();
                }
            }
        }
    }

    bool IsVisible(GameObject target)
    {
        BoxCollider box = target.GetComponent<BoxCollider>();
        if (box == null) return false;

        // Get world space center and extents
        Vector3 center = box.transform.TransformPoint(box.center);
        Vector3 extents = Vector3.Scale(box.size * 0.5f, box.transform.lossyScale);

        // Calculate all 8 corners of the box
        Vector3[] corners = new Vector3[8];
        int i = 0;
        for (int x = -1; x <= 1; x += 2)
        {
            for (int y = -1; y <= 1; y += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    Vector3 localCorner = new Vector3(x * extents.x, y * extents.y, z * extents.z);
                    corners[i++] = center + box.transform.rotation * localCorner;
                }
            }
        }

        // Check each corner for visibility
        foreach (Vector3 corner in corners)
        {
            Vector3 viewportPoint = playerCamera.WorldToViewportPoint(corner);

            if (viewportPoint.z > 0 && viewportPoint.x >= 0 && viewportPoint.x <= 1 && viewportPoint.y >= 0 && viewportPoint.y <= 1)
            {
                Vector3 direction = corner - playerCamera.transform.position;
                if (Physics.Raycast(playerCamera.transform.position, direction, out RaycastHit hit, Mathf.Infinity, ~visibilityMask))
                {
                    if (hit.collider.gameObject == target)
                    {
                        return true; // At least one corner is visible
                    }
                }
            }
        }

        return false;
    }
}
