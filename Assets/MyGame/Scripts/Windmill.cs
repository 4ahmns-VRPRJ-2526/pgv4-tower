using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class Windmill : MonoBehaviour
{
    private enum WindmillColors { RED, GREEN, BLUE };

    [Header("Settings")]
    [SerializeField] private WindmillColors color;
    [SerializeField] public RotorHub rotor;
    [SerializeField] private Light lampLight;
    [SerializeField] private Slider speedSlider;
    [SerializeField] private TMP_Text lockedText;
    [SerializeField] private AudioSource windmillEngine;
    [SerializeField] private StartSoundScript SoundScript;
    

    [Header("Glass Cable Settings")]
    [SerializeField] private Renderer cylinderRenderer;
    private Color baseColor;

    [SerializeField] public bool isWindmillSelected = false;
    private const float MAX_LIGHT_INTENSITY = 1f;

    [Header("Pulse Animation")]
    private Vector3 originalScale;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseMagnitude = 0.05f;

    private void Start()
    {
        if (!lampLight || !rotor || !speedSlider || !cylinderRenderer)
        {
            Debug.LogWarning("Windmill: Nicht alle Referenzen sind gesetzt.");
            return;
        }

        originalScale = transform.localScale;

        baseColor = GetColorFromEnum(color);

        Color clearGlass = new Color(0.9f, 0.9f, 0.9f, 0.15f);
        cylinderRenderer.material.color = clearGlass;

        cylinderRenderer.material.SetColor("_EmissionColor", Color.black);
        cylinderRenderer.material.DisableKeyword("_EMISSION");

        speedSlider.value = 0;

        ToggleLamp();
        SetLampColor(color);
    }

    private void Update()
    {
        UpdateUI();
        UpdateVisuals();

        if (isWindmillSelected)
        {
            rotor.RotateRotor(true);
            AnimatePulse();
            if (SoundScript != null && !SoundScript.enabled)
            {
                SoundScript.enabled = true;
            }

            if (windmillEngine != null && !windmillEngine.isPlaying)
            {
                windmillEngine.Play();
            }
        }
        else
        {
            rotor.RotateRotor(false);

            if (IsWindmillLocked())
            {
                ShowHideWindmill(false);
            }
            if (SoundScript.enabled == true) 
            {
                SoundScript.enabled = false;
            }

            ResetScale();
            if (windmillEngine.isPlaying) windmillEngine.Stop();
        }
    }

    private void UpdateVisuals()
    {
        if (lampLight != null && cylinderRenderer != null)
        {
            float factor = speedSlider.value / 255f;

            lampLight.intensity = Mathf.Lerp(0f, MAX_LIGHT_INTENSITY, factor);

            Color clearGlass = new Color(0.9f, 0.9f, 0.9f, 0.15f);
            Color fullColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0.7f);

            cylinderRenderer.material.color = Color.Lerp(clearGlass, fullColor, factor);

            if (factor > 0f)
            {
                cylinderRenderer.material.SetColor("_EmissionColor", baseColor * (factor * 3f));
                cylinderRenderer.material.EnableKeyword("_EMISSION");
            }
            else
            {
                cylinderRenderer.material.SetColor("_EmissionColor", Color.black);
                cylinderRenderer.material.DisableKeyword("_EMISSION");
            }
        }
    }

    private Color GetColorFromEnum(WindmillColors c)
    {
        switch (c)
        {
            case WindmillColors.RED: return Color.red;
            case WindmillColors.GREEN: return Color.green;
            case WindmillColors.BLUE: return Color.blue;
            default: return Color.white;
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

    private void SetLampColor(WindmillColors windmillColor)
    {
        switch (windmillColor)
        {
            case WindmillColors.RED: lampLight.color = Color.red; break;
            case WindmillColors.GREEN: lampLight.color = Color.green; break;
            case WindmillColors.BLUE: lampLight.color = Color.blue; break;
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
        if (lampLight.enabled) ToggleLamp();
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
        if (lockedText != null)
            lockedText.text = isWindmillSelected ? "Unlock" : "Lock";

        EventSystem.current.SetSelectedGameObject(null);
        WindmillManager manager = FindObjectOfType<WindmillManager>();
        if (manager != null)
        {
            manager.LockAllExcept(this);
        }
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


    public void HighlightLamp()
    {
        if (lampLight != null)
        {
            lampLight.enabled = true;
            SetLampColor(color);
            lampLight.intensity = 1f;
        }

        if (cylinderRenderer != null)
        {
            cylinderRenderer.material.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.4f);
            cylinderRenderer.material.SetColor("_EmissionColor", baseColor * 2f);
        }
    }

    public void DimLamp()
    {
        if (lampLight != null)
        {
            lampLight.enabled = true;
            lampLight.color = Color.gray;
            lampLight.intensity = 0.2f;
        }

        if (cylinderRenderer != null)
        {
            cylinderRenderer.material.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
            cylinderRenderer.material.SetColor("_EmissionColor", Color.black);
        }
    }
}