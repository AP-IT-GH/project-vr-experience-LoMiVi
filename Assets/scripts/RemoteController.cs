using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using UnityEngine.Video;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.UIElements;

public class RemoteController : MonoBehaviour
{
    public GameObject[] colorChangingObjects;  // Objects to change color
    public InputActionReference buttonPressAction; // Assign in Inspector
    public Color pressColor = Color.red;
    public float colorDuration = 0.5f;

    public GameObject tvScreen; // Assign TV screen object
    private VideoPlayer videoPlayer;
    private bool isTVOn = false;

    private Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();
    private bool isHeld = false;
    private XRGrabInteractable grabInteractable;

    public Material tvOnMaterial;  // Assign an unlit material
    public Material tvOffMaterial; // Assign a black or dark material

    private Renderer tvRenderer;

    void Start()
    {
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

        grabInteractable = GetComponent<XRGrabInteractable>();

        // Subscribe to grab events
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        isHeld = true;
        buttonPressAction.action.performed += OnButtonPress;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        isHeld = false;
        buttonPressAction.action.performed -= OnButtonPress;
    }

    void OnButtonPress(InputAction.CallbackContext context)
    {
        if (!isHeld) return;

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
            videoPlayer.Play();
        }
        else
        {
            tvRenderer.material = tvOffMaterial; // Apply the dark material
            videoPlayer.Stop();
        }
    }
}
