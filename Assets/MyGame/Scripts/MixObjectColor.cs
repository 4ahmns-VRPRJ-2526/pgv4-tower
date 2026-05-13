using UnityEngine;

public enum ColorRGB
{
    Red,
    Green,
    Blue,
}

public enum PipeMode
{
    Increase,
    Decrease
}

public class MixObjectColor : MonoBehaviour
{
    [SerializeField] private PipeMode pipeMode;
    private Material objMaterial;

    [Header("Arrays für mehrere Objekte")]
    [SerializeField] private PepperOnlyManager[] pepperManagers;
    [SerializeField] private GameObject[] kuebelObjects;
    [SerializeField] private GameObject[] pipeObjects;

    [Header("Referenzen")]
    [SerializeField] private Liquid red, blue, green;
    [SerializeField] private ParticleSystem redParticleSystem, blueParticleSystem, greenParticleSystem;
    [SerializeField] private GameObject taskPoints;
    private ParticleSystem currentParticleSystem;

    // Werte für den Eingangsbereich
    [SerializeField] private float inputMarkerFull;
    [SerializeField] private float inputMarkerEmpty;

    [SerializeField] private float stepSize;

    // Werte für den Ausgangsbereich
    [SerializeField] private float outputMarkerFull = 255f;   // Voll
    [SerializeField] private float outputMarkerEmpty = 0f;     // Leer

    private Color32 mixedColor = Color.black;
    private ItemData peppersGhostData;

    private void Start()
    {
        peppersGhostData = Resources.Load<ItemData>("PeppersGhostData");

        if (pipeMode == PipeMode.Decrease)
        {
            inputMarkerFull = -0.34f;  // Voll
            inputMarkerEmpty = 1.63f;   // Leer

            pepperManagers = GameObject.FindObjectsOfType<PepperOnlyManager>();

            currentParticleSystem = null;
            redParticleSystem.Stop();
            blueParticleSystem.Stop();
            greenParticleSystem.Stop();
        }

        objMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mixedColor = new Color32(0, 0, 0, 255);
        objMaterial.color = mixedColor;

        // Array für Kübel - Sucht auch in Unterobjekten (Children)
        if (kuebelObjects != null)
        {
            for (int i = 0; i < kuebelObjects.Length; i++)
            {
                if (kuebelObjects[i] != null)
                {
                    MeshRenderer mr = kuebelObjects[i].GetComponentInChildren<MeshRenderer>();
                    if (mr != null) mr.material = objMaterial;
                }
            }
        }

        // Array für Pipes - Sucht auch in Unterobjekten (Children)
        if (pipeObjects != null)
        {
            for (int i = 0; i < pipeObjects.Length; i++)
            {
                if (pipeObjects[i] != null)
                {
                    MeshRenderer mr = pipeObjects[i].GetComponentInChildren<MeshRenderer>();
                    if (mr != null) mr.material = objMaterial;
                }
            }
        }

        if (pipeMode == PipeMode.Increase)
        {
            inputMarkerFull = 0.19f;  // Voll
            inputMarkerEmpty = 0.79f;   // Leer
            outputMarkerFull = 0f;   // Voll
            outputMarkerEmpty = 255f;     // Leer
            EmptyPipes();
        }
        else if (pipeMode == PipeMode.Decrease)
        {
            FillPipes();
        }
    }

    public Color32 GetPlayerMixedColor()
    {
        return mixedColor;
    }

    public void StartParticles(ColorRGB color)
    {
        switch (color)
        {
            case ColorRGB.Red:
                if (PipeEmpty(ColorRGB.Red)) return;
                currentParticleSystem = redParticleSystem;
                break;
            case ColorRGB.Blue:
                if (PipeEmpty(ColorRGB.Blue)) return;
                currentParticleSystem = blueParticleSystem;
                break;
            case ColorRGB.Green:
                if (PipeEmpty(ColorRGB.Green)) return;
                currentParticleSystem = greenParticleSystem;
                break;
        }

        if (currentParticleSystem.isPlaying)
        {
            currentParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        currentParticleSystem.Play();
    }

    public void StopParticles()
    {
        if (currentParticleSystem == null) return;

        if (currentParticleSystem.isPlaying)
        {
            currentParticleSystem.Stop();
            currentParticleSystem.Clear();
        }
    }

    private byte GetByte(byte channelByte)
    {
        int result = 255 - channelByte;
        result = Mathf.Clamp(result, 0, 255);
        return (byte)result;
    }

    public bool PipeEmpty(ColorRGB pipe)
    {
        switch (pipe)
        {
            case ColorRGB.Red:
                return red.fillAmount >= inputMarkerEmpty;
            case ColorRGB.Green:
                return green.fillAmount >= inputMarkerEmpty;
            case ColorRGB.Blue:
                return blue.fillAmount >= inputMarkerEmpty;
            default:
                return false;
        }
    }

    public void DecreaseFillState(ColorRGB color)
    {
        byte r = GetByte((byte)MapRange(red.fillAmount));
        byte g = GetByte((byte)MapRange(green.fillAmount));
        byte b = GetByte((byte)MapRange(blue.fillAmount));

        switch (color)
        {
            case ColorRGB.Red:
                if (red.fillAmount <= inputMarkerEmpty)
                {
                    red.fillAmount += stepSize;
                    int result = 255 - (byte)MapRange(red.fillAmount);
                    result = Mathf.Clamp(result, 0, 255);
                    r = (byte)result;
                }
                break;
            case ColorRGB.Blue:
                if (blue.fillAmount <= inputMarkerEmpty)
                {
                    blue.fillAmount += stepSize;
                    int result = 255 - (byte)MapRange(blue.fillAmount);
                    result = Mathf.Clamp(result, 0, 255);
                    b = (byte)result;
                }
                break;
            case ColorRGB.Green:
                if (green.fillAmount <= inputMarkerEmpty)
                {
                    green.fillAmount += stepSize;
                    int result = 255 - (byte)MapRange(green.fillAmount);
                    result = Mathf.Clamp(result, 0, 255);
                    g = (byte)result;
                }
                break;
        }

        mixedColor = new Color32(r, g, b, 255);
        ApplyColorToArrays();
    }

    public void IncreaseFillState(ColorRGB color)
    {
        byte r = GetByte((byte)MapRange(red.fillAmount));
        byte g = GetByte((byte)MapRange(green.fillAmount));
        byte b = GetByte((byte)MapRange(blue.fillAmount));

        switch (color)
        {
            case ColorRGB.Red:
                if (red.fillAmount >= inputMarkerFull)
                {
                    red.fillAmount -= stepSize;
                    int result = 255 - (byte)MapRange(red.fillAmount);
                    result = Mathf.Clamp(result, 0, 255);
                    r = (byte)result;
                }
                break;
            case ColorRGB.Blue:
                if (blue.fillAmount >= inputMarkerFull)
                {
                    blue.fillAmount -= stepSize;
                    int result = 255 - (byte)MapRange(blue.fillAmount);
                    result = Mathf.Clamp(result, 0, 255);
                    b = (byte)result;
                }
                break;
            case ColorRGB.Green:
                if (green.fillAmount >= inputMarkerFull)
                {
                    green.fillAmount -= stepSize;
                    int result = 255 - (byte)MapRange(green.fillAmount);
                    result = Mathf.Clamp(result, 0, 255);
                    g = (byte)result;
                }
                break;
        }

        mixedColor = new Color32(r, g, b, 255);
        ApplyColorToArrays();
    }

    public float MapRange(float inputValue)
    {
        return outputMarkerFull + (inputValue - inputMarkerFull) * (outputMarkerEmpty - outputMarkerFull) / (inputMarkerEmpty - inputMarkerFull);
    }

    public void FillPipes()
    {
        red.fillAmount = blue.fillAmount = green.fillAmount = inputMarkerFull;
    }

    public void EmptyPipes()
    {
        red.fillAmount = blue.fillAmount = green.fillAmount = inputMarkerEmpty;
    }

    public void ResetPipeStation()
    {
        mixedColor = peppersGhostData.taskColors[(int)TaskToDo.None];
        ApplyColorToArrays();

        if (pipeMode == PipeMode.Decrease)
        {
            FillPipes();
        }
        else if (pipeMode == PipeMode.Increase)
        {
            EmptyPipes();
        }
    }

    private void Update()
    {
        if (pipeMode == PipeMode.Decrease)
        {
            if (redParticleSystem.isPlaying && PipeEmpty(ColorRGB.Red)) redParticleSystem.Stop();
            if (greenParticleSystem.isPlaying && PipeEmpty(ColorRGB.Green)) greenParticleSystem.Stop();
            if (blueParticleSystem.isPlaying && PipeEmpty(ColorRGB.Blue)) blueParticleSystem.Stop();
        }
    }

    private void ApplyColorToArrays()
    {
        if (kuebelObjects != null)
        {
            for (int i = 0; i < kuebelObjects.Length; i++)
            {
                if (kuebelObjects[i] != null)
                {
                    MeshRenderer mr = kuebelObjects[i].GetComponentInChildren<MeshRenderer>();
                    if (mr != null) mr.material.color = mixedColor;
                }
            }
        }

        if (pipeObjects != null)
        {
            for (int i = 0; i < pipeObjects.Length; i++)
            {
                if (pipeObjects[i] != null)
                {
                    MeshRenderer mr = pipeObjects[i].GetComponentInChildren<MeshRenderer>();
                    if (mr != null) mr.material.color = mixedColor;
                }
            }
        }
    }
}