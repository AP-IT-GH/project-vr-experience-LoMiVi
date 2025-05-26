using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit;

public class LockMechanism : MonoBehaviour
{
    public GameObject door;
    private XRSocketInteractor socket;

    private void Start()
    {
        socket = GetComponent<XRSocketInteractor>();
        socket.selectEntered.AddListener(OnKeyInserted);
    }

    private void OnKeyInserted(SelectEnterEventArgs args)
    {
        GameObject insertedKey = args.interactableObject.transform.gameObject;
        if (insertedKey.CompareTag("key"))
        {
            OpenDoor();
        }
    }

    void OpenDoor()
    {
        door.transform.rotation *= Quaternion.Euler(0, 90, 0);
    }
}
