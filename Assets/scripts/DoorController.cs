using UnityEngine;

public class DoorController : MonoBehaviour
{
    public float openAngle = 135f;
    public float closeAngle = 0f;
    public float openSpeed = 2f;
    public float bounceDuration = 0.5f;
    public bool isOpen = false;

    private Quaternion targetRotation;
    private float animationTime = 0f;
    private bool isClosing = false;
    private bool isOpening = false;
    private bool isAnimating = false;

    void Start()
    {
        if (isOpen)
        {
            targetRotation = Quaternion.Euler(0, openAngle, 0);
            transform.localRotation = targetRotation;
            isOpening = false;
        }
        else
        {
            targetRotation = Quaternion.Euler(0, closeAngle, 0);
            transform.localRotation = targetRotation;
            isClosing = false;
        }
    }

    void Update()
    {
        if (isClosing)
        {
            isAnimating = true;
            // Perform the bounce animation
            animationTime += Time.deltaTime / bounceDuration;
            if (animationTime < 1f)
            {
                float easedTime = EaseOutBounce(animationTime);
                float bounceAngle = Mathf.Lerp(openAngle, closeAngle, easedTime);
                transform.localRotation = Quaternion.Euler(0, bounceAngle, 0);
            }
            else
            {
                // End the bounce animation
                isClosing = false;
                transform.localRotation = Quaternion.Euler(0, closeAngle, 0);
                isAnimating = false;
            }
        }
        else
        {
            if (isOpening)
            {
                isAnimating = true;
                // Smoothly rotate the door to the target rotation
                transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * openSpeed);

                // Check if close enough to stop animating
                if (Quaternion.Angle(transform.localRotation, targetRotation) < 1f)
                {
                    isOpening = false;
                    isAnimating = false;
                }
            }
        }
    }

    public void ToggleDoor()
    {
        if (isOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }

    public void OpenDoor()
    {
        if (!isAnimating)
        {
            targetRotation = Quaternion.Euler(0, openAngle, 0);
            isOpen = true;
            isOpening = true;
        }
    }

    public void CloseDoor()
    {
        if (!isAnimating)
        {

            targetRotation = Quaternion.Euler(0, closeAngle, 0);
            isOpen = false;

            // Start the bounce animation
            animationTime = 0f;
            isClosing = true;
        }
    }

    public void ShakeDoor()
    {
        if (!isAnimating)
        {
            StartCoroutine(ShakeCoroutine());
        }
    }

    private System.Collections.IEnumerator ShakeCoroutine()
    {
        isAnimating = true;

        float shakeDuration = 0.5f;
        float shakeMagnitude = 2f;
        float elapsedTime = 0f;

        Quaternion originalRotation = transform.localRotation;

        while (elapsedTime < shakeDuration)
        {
            elapsedTime += Time.deltaTime;

            float angleOffset = Mathf.Sin(elapsedTime * Mathf.PI * 10) * shakeMagnitude; // Oscillates back and forth
            transform.localRotation = originalRotation * Quaternion.Euler(0, angleOffset, 0);

            yield return null;
        }

        transform.localRotation = originalRotation;
        isAnimating = false;
    }

    private float EaseOutBounce(float t)
    {
        if (t < 1 / 2.75f)
        {
            return 7.5625f * t * t;
        }
        else if (t < 2 / 2.75f)
        {
            t -= 1.5f / 2.75f;
            return 7.5625f * t * t + 0.75f;
        }
        else if (t < 2.5f / 2.75f)
        {
            t -= 2.25f / 2.75f;
            return 7.5625f * t * t + 0.9375f;
        }
        else
        {
            t -= 2.625f / 2.75f;
            return 7.5625f * t * t + 0.984375f;
        }
    }
}
