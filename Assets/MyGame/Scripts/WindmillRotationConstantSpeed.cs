using UnityEngine;

public class WindmillRotationConstantSpeed : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 100f; // Speed of rotation
    private bool isRotating = false; // Toggle for rotation

    private void Update()
    {
        // Check if Space key is pressed to toggle rotation
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isRotating = !isRotating;
        }

        // Rotate the rotors if enabled
        if (isRotating)
        {
            transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }
    }
}
