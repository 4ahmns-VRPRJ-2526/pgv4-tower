using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class Windmill : MonoBehaviour
{
    private enum WindmillColors { RED, GREEN, BLUE };

    [SerializeField] private WindmillColors color;
    [SerializeField] public RotorHub rotor;
    [SerializeField] private Light lampLight;
    [SerializeField] private Slider speedSlider;
    [SerializeField] private TMP_Text lockedText;
    [SerializeField] private AudioSource windmillEngine;

    [SerializeField] public bool isWindmillSelected = false;
    private const float MAX_LIGHT_INTENSITY = 1f;

    // Für pulsierende Animation
    private Vector3 originalScale;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseMagnitude = 0.05f;

    private void Start()
    {
        if (!lampLight || !rotor || !speedSlider)
        {
            Debug.LogWarning("Windmill: Nicht alle Referenzen sind gesetzt.");
            return;
        }

        originalScale = transform.localScale;

        ToggleLamp();
        SetLampColor(color);
        
    }

    private void Update()
    {
        UpdateUI();
        UpdateLightIntensity();

        if (isWindmillSelected)
        {
            rotor.RotateRotor(true);
            AnimatePulse();
            windmillEngine.Play();
        }
        else
        {
            rotor.RotateRotor(false);

            if (IsWindmillLocked())
            {
                ShowHideWindmill(false);
            }

            ResetScale();
        }
    }

    public void ShowHideWindmill(bool hide)
    {
        WindmillShowHide manager = FindObjectOfType<WindmillShowHide>();
        if (manager != null && hide)
        {
            manager.ShowOnly(this);
        }
    }

    public void ToggleRotationMode()
    {
        isWindmillSelected = false;
        rotor.constRotationSpeed = rotor.currentSpeed;
    }

    public int GetCurrentSpeed()
    {
        return rotor.GetCurrentSpeed();
    }

    private void ToggleLamp()
    {
        if (lampLight != null)
        {
            lampLight.enabled = !lampLight.enabled;
        }
    }

    private void UpdateLightIntensity()
    {
        if (lampLight != null)
        {
            lampLight.intensity = Mathf.Lerp(0f, MAX_LIGHT_INTENSITY, speedSlider.value / 255f);
        }
    }

    private void SetLampColor(WindmillColors windmillColor)
    {
        switch (windmillColor)
        {
            case WindmillColors.RED:
                lampLight.color = Color.red;
                break;
            case WindmillColors.GREEN:
                lampLight.color = Color.green;
                break;
            case WindmillColors.BLUE:
                lampLight.color = Color.blue;
                break;
        }
    }

    public void SelectWindmill()
    {
        isWindmillSelected = true;

        if (!lampLight.isActiveAndEnabled)
        {
            lampLight.enabled = true;
            ShowHideWindmill(true);
        }
    }

    public void ResetWindmill()
    {
        rotor.constRotationSpeed = -1;
        isWindmillSelected = false;
        rotor.currentSpeed = 0;
        speedSlider.value = 0;
        ToggleLamp();
        ResetScale();
    }

    public bool IsWindmillLocked()
    {
        return !isWindmillSelected && rotor.constRotationSpeed != -1;
    }

    private void UpdateUI()
    {
        if (isWindmillSelected && speedSlider != null)
        {
            speedSlider.value = Mathf.Round(rotor.currentSpeed);
        }
    }

    public void ToggleLockStatus()
    {
        lockedText.text = isWindmillSelected ? "Unlock" : "Lock";
        EventSystem.current.SetSelectedGameObject(null);
        WindmillManager manager = FindObjectOfType<WindmillManager>();
        if (manager != null)
        {
            manager.LockAllExcept(this);
        }
    }

    public void HighlightLamp()
    {
        lampLight.enabled = true;
        SetLampColor(color);
        lampLight.intensity = 1f;
    }

    public void DimLamp()
    {
        lampLight.enabled = true;
        lampLight.color = Color.gray;
        lampLight.intensity = 0.2f;
    }

    private void AnimatePulse()
    {
        float scaleFactor = 1 + Mathf.Sin(Time.time * pulseSpeed) * pulseMagnitude;
        transform.localScale = originalScale * scaleFactor;
    }

    private void ResetScale()
    {
        transform.localScale = originalScale;
    }
}
