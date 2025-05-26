using UnityEngine;

public class DoorTriggerZone : MonoBehaviour
{
    public DoorController door;
    private bool IsTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !IsTriggered)
        {
            door.CloseDoor();
            IsTriggered = true;
        }
    }
}
