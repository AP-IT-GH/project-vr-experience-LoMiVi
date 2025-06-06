using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Video;

public class RemoteController : MonoBehaviour
{
    public GameObject[] colorChangingObjects;  // Objects to change color
    public Color pressColor = Color.red;
    public float colorDuration = 0.5f;

    public GameObject tvScreen; // Assign TV screen object
    private VideoPlayer videoPlayer;
    private bool isTVOn = false;

    private Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();

    public Material tvOnMaterial;  // Assign an unlit material
    public Material tvOffMaterial; // Assign a black or dark material
    public List<GameObject> tvLights;

    private Renderer tvRenderer;

    void Start()
    {
        foreach (var light in tvLights)
        {
            light.SetActive(false);
        }
        // Store original colors of each object
        foreach (GameObject obj in colorChangingObjects)
        {
            Renderer objRenderer = obj?.GetComponent<Renderer>();
            if (objRenderer != null)
            {
                originalColors[objRenderer] = objRenderer.material.color;
            }
        }

        // Get the VideoPlayer component from the TV screen
        if (tvScreen != null)
        {
            videoPlayer = tvScreen.GetComponent<VideoPlayer>();
            tvRenderer = tvScreen.GetComponent<Renderer>();

            if (videoPlayer != null)
                videoPlayer.Stop(); // Ensure TV starts off

            if (tvRenderer != null && tvOffMaterial != null)
                tvRenderer.material = tvOffMaterial; // Start with TV off material
        }
    }

    public void OnButtonPress()
    {
        // Change object colors
        foreach (GameObject obj in colorChangingObjects)
        {
            Renderer objRenderer = obj?.GetComponent<Renderer>();
            if (objRenderer != null)
            {
                objRenderer.material.color = pressColor;
            }
        }
        Invoke(nameof(ResetObjectsColor), colorDuration);

        // Toggle TV on/off
        ToggleTV();
    }

    void ResetObjectsColor()
    {
        foreach (var entry in originalColors)
        {
            if (entry.Key != null)
                entry.Key.material.color = entry.Value;
        }
    }

    void ToggleTV()
    {
        if (videoPlayer == null || tvRenderer == null) return;

        isTVOn = !isTVOn;

        if (isTVOn)
        {
            tvRenderer.material = tvOnMaterial; // Apply the unlit material
            foreach (var light in tvLights)
            {
                light.SetActive(true);
            }
            videoPlayer.Play();
        }
        else
        {
            tvRenderer.material = tvOffMaterial; // Apply the dark material
            foreach (var light in tvLights)
            {
                light.SetActive(false);
            }
            videoPlayer.Stop();
        }
    }
}
