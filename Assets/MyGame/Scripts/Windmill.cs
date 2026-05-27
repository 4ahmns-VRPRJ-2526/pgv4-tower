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
    private const float MAX_LIGHT_INTENSITY = 5f; //höherer Wert für mehr Intensität
    private Color originalLampColor; //Originale Farbe

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

        SetLampColor(color);

        // originale Farbe speichern
        originalLampColor = lampLight.color;

        lampLight.enabled = false;

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
        if (lampLight != null && rotor != null)
        {
            float normalizedSpeed = rotor.currentSpeed / 255f;

            // sanfter Verlauf
            float curvedValue = Mathf.Pow(normalizedSpeed, 2f);

            // Intensität
            float targetIntensity = Mathf.Lerp(0.2f, MAX_LIGHT_INTENSITY, curvedValue);

            lampLight.intensity = Mathf.Lerp(
                lampLight.intensity,
                targetIntensity,
                Time.deltaTime * 5f
            );

            // Auch die Reichweite verändern
            lampLight.range = Mathf.Lerp(2f, 5f, curvedValue);
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

        // originale Farbe wiederherstellen
        lampLight.color = originalLampColor;
        lampLight.intensity = 1f;
    }

    public void DimLamp()
    {
        lampLight.enabled = true;

        // Farbe NICHT überschreiben
        lampLight.color = originalLampColor;

        // nur Intensität reduzieren
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
