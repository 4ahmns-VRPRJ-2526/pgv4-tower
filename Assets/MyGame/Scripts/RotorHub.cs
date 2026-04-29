using UnityEngine;

public class RotorHub : MonoBehaviour
{
    private const float MAX_ROTATION_SPEED = 255f; // Maximum speed
    private const float UNSET_CONST_ROTATION = -1f; // Constant Rotation not set

    [SerializeField] private float acceleration = 50f; // Speed increase per second
    [SerializeField] private float deceleration = 30f; // Speed decrease per second

    public float currentSpeed = 0f; // Current rotation speed
    public float constRotationSpeed = -1f;


    public void RotateRotor(bool windmillSelected)
    {
        if (!windmillSelected)
        {
            if (constRotationSpeed != -1)
            {
                transform.Rotate(Vector3.forward, constRotationSpeed * Time.deltaTime, Space.World);

            }
            return;
        }

        
        RotateAtDynamicSpeed(Input.GetKey(KeyCode.Space));
        transform.Rotate(Vector3.forward, currentSpeed * Time.deltaTime, Space.World);

    }


    public int GetCurrentSpeed()
    {
        return (int)Mathf.Clamp(currentSpeed, 0f, MAX_ROTATION_SPEED);
    }


    public void RotateAtDynamicSpeed(bool isKeyPressed)
    {
        // Holding Space increases rotation speed
        if (isKeyPressed)
        {
            currentSpeed += acceleration * Time.deltaTime;
        }
        else
        {
            // Slowly reduce speed when Space is not pressed
            currentSpeed -= deceleration * Time.deltaTime;
        }

        // Clamp speed between 0 and maxRotationSpeed
        currentSpeed = Mathf.Clamp(currentSpeed, 0f, MAX_ROTATION_SPEED);
    }
}