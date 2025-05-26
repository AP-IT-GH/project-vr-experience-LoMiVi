using Unity.VisualScripting;
using UnityEngine;

public class FlashlightToggle : MonoBehaviour
{
    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
