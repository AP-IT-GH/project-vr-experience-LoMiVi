using UnityEngine;

public class FlickerLight : MonoBehaviour
{
    public Light light;
    public float minIntensity = 0.8f;
    public float maxIntensity = 1f;
    public float intensityChangeSpeed = 10f;

    // Movement
    public float movementMagnitude = 0.005f;
    public float movementSpeed = 2f;
    private Vector3 targetPosition;
    private float nextMoveTime;

    private float targetIntensity;
    private Vector3 initialPosition;

    void Start()
    {
        if (light == null)
        {
            Debug.LogError("Light reference is not set!");
        }

        initialPosition = transform.position;
        targetPosition = initialPosition;
        targetIntensity = light.intensity;
    }

    void Update()
    {
        if (light != null)
        {
            light.intensity = Mathf.Lerp(light.intensity, targetIntensity, Time.deltaTime * intensityChangeSpeed);

            if (Time.time >= nextMoveTime)
            {
                SetRandomTargetPosition();
                SetRandomTargetIntensity();
                nextMoveTime = Time.time + Random.Range(0.2f, 0.5f);
            }

            SmoothMove();
        }
    }

    void SetRandomTargetPosition()
    {
        Vector3 randomOffset = new Vector3(
            Random.Range(-movementMagnitude, movementMagnitude),
            Random.Range(0, movementMagnitude),
            Random.Range(-movementMagnitude, movementMagnitude)
        );

        targetPosition = initialPosition + randomOffset;
    }

    void SetRandomTargetIntensity()
    {
        targetIntensity = Random.Range(minIntensity, maxIntensity);
    }

    void SmoothMove()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * movementSpeed);
    }
}
