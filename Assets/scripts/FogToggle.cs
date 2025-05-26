using UnityEngine;

public class FogToggle : MonoBehaviour
{
    public bool isFogEnabledOutside = true; // Fog enabled outside
    public bool isFogEnabledInside = false; // Fog disabled inside

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RenderSettings.fog = isFogEnabledInside;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RenderSettings.fog = isFogEnabledOutside;
        }
    }
}
