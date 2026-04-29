using UnityEngine;
using UnityEngine.UI;

public class WindmillDynamicSpeed : MonoBehaviour
{
    [SerializeField] private Light lampLight; // Assign in Inspector
    [SerializeField] private float maxLightIntensity = 1f; // Maximum lamp brightness
    [SerializeField] private Slider speedSlider; // Assign in Inspector
    [SerializeField] private float maxRotationSpeed = 300f; // Maximum speed
    [SerializeField] private float acceleration = 50f; // Speed increase per second
    [SerializeField] private float deceleration = 30f; // Speed decrease per second
    private float currentSpeed = 0f; // Current rotation speed

    private void Update()
    {
        // Holding Space increases rotation speed
        if (Input.GetKey(KeyCode.Space))
        {
            currentSpeed += acceleration * Time.deltaTime;
        }
        else
        {
            // Slowly reduce speed when Space is not pressed
            currentSpeed -= deceleration * Time.deltaTime;
        }

        // Clamp speed between 0 and maxRotationSpeed
        currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxRotationSpeed);

        // Rotate the rotor hub
        transform.Rotate(Vector3.forward * currentSpeed * Time.deltaTime);

        if (speedSlider != null)
        {
            speedSlider.value = Mathf.Round(currentSpeed);
        }

        // Control light intensity based on speed
        if (lampLight != null)
        {
            lampLight.intensity = Mathf.Lerp(0f, maxLightIntensity, speedSlider.value / 255f);
        }
    }
}
