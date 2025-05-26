using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static bool hasKey = false;

    public static void SetKey(bool value)
    {
        hasKey = value;
    }
}
