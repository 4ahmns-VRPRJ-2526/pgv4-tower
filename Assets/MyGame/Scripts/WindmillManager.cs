using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WindmillManager : MonoBehaviour
{
    public int resolutionWidth, resolutionHeight;

    [SerializeField] private Windmill[] windmills;
    [SerializeField] private GameObject _wallGoal;
    [SerializeField] private GameObject landscapeObjs;
    [SerializeField] private GameObject verticalStartCanvas;
    [SerializeField] private GameObject horizontalStartCanvas;
    [SerializeField] private GhostController ghostController;

    private GameManagerScript _cgsa;
    public Color32 windmillColor = new Color32(0, 0, 0, 255);

    private Windmill currentSelectedWindmill;
    private bool allWindmillsLocked = false;

    public Renderer glowingResultCube;
    [SerializeField] private Renderer mixingColorFace;
    [SerializeField] private Renderer mixingLightRim;
    private MaterialPropertyBlock mixingColorFaceProperties;
    private MaterialPropertyBlock mixingLightRimProperties;
    [Header("Leiser Magieklang")]
    [SerializeField] private AudioClip worldColorMagic;
    [SerializeField, Range(0f, 1f)] private float worldColorMagicVolume = 0.28f;
    private AudioSource worldColorMagicSource;

    [Header("ParticleSystems")]
    public ParticleSystem particlesTop;
    public GameObject[] objectsToColor;

    [Header("Abschluss: Leuchtturm und Blumen")]
    [SerializeField] private Transform lighthouseRoot;
    [SerializeField] private Transform lighthouseRoof;
    [SerializeField] private string roofObjectName = "model_part5";
    [Header("Leuchtturmlampe")]
    [SerializeField] private Renderer lighthouseLamp;
    [SerializeField] private string lampObjectName = "model_part1";
    [SerializeField, Min(0f)] private float lampPeakGlow = 4f;
    [SerializeField, Min(0f)] private float lampIdleGlow = 1.5f;
    [SerializeField, Min(0f)] private float lampLightRange = 6f;
    private Material[] lampOriginalMaterials;
    private Material[] lampGlowMaterials;
    private Light lampLight;
    private Color lampColor;
    [SerializeField, Min(0f)] private float roofLiftHeight = 2.5f;
    [SerializeField, Min(0.1f)] private float revealDuration = 2.5f;
    [SerializeField, Min(0.1f)] private float worldWaveDuration = 5.5f;
    [SerializeField, Min(0f)] private float resultHoldDuration = 4f;
    [SerializeField] private GameObject[] flowersToColor = new GameObject[0];
    [SerializeField] private WorldColorController worldColorController;
    [SerializeField] private PipeColorPulse pipeColorPulse;
    private bool worldWasGrayscale;
    private Vector3 roofStartPosition;
    private bool resultRevealed;
    private readonly List<Material> flowerMaterials = new List<Material>();
    private readonly List<Color> flowerStartColors = new List<Color>();

    private void Awake()
    {
        UpdateMixingColorFace();
        if (lighthouseRoof == null && lighthouseRoot != null)
        {
            foreach (Transform part in lighthouseRoot.GetComponentsInChildren<Transform>(true))
                if (part.name == roofObjectName) { lighthouseRoof = part; break; }
        }
        if (lighthouseRoof != null) roofStartPosition = lighthouseRoof.localPosition;
        if (lighthouseLamp == null && lighthouseRoot != null)
            foreach (Transform part in lighthouseRoot.GetComponentsInChildren<Transform>(true))
                if (part.name == lampObjectName) { lighthouseLamp = part.GetComponent<Renderer>(); break; }
        StartScreenHandHint.AttachTo(verticalStartCanvas);
        StartScreenHandHint.AttachTo(horizontalStartCanvas);

        resolutionWidth = Screen.width;
        resolutionHeight = Screen.height;

        if (resolutionWidth > 1080)
        {
            horizontalStartCanvas.SetActive(true);
        }
        else
        {
            verticalStartCanvas.SetActive(true);
        }

        particlesTop.enableEmission = false;
        _cgsa = GameObject.FindObjectOfType<GameManagerScript>();

        if (windmills.Length == 0)
        {
            Debug.LogError("WindmillManager: Keine Windmuehlen oder Farbwand zugewiesen!");
            return;
        }

    }

    public void BeginGameplay()
    {
        if (currentSelectedWindmill != null || windmills.Length == 0) return;
        currentSelectedWindmill = windmills[0];
        currentSelectedWindmill.SelectWindmill();
    }

    private void Update()
    {
        if (_cgsa != null && !_cgsa.WindmillsActive) return;
        UpdateWallColor();
        CheckIfAllLocked();
        UpdateColors();
    }

    private void UpdateColors()
    {
        UpdateMixingColorFace();
        foreach (GameObject obj in objectsToColor)
        {
            if (obj == null) continue;

            Renderer rend = obj.GetComponent<Renderer>();
            if (rend != null && rend.material.HasProperty("_Color"))
            {
                rend.material.color = windmillColor;
            }
        }

    }

    private void UpdateMixingColorFace()
    {
        if (mixingColorFace == null) return;
        if (mixingColorFaceProperties == null) mixingColorFaceProperties = new MaterialPropertyBlock();
        mixingColorFace.GetPropertyBlock(mixingColorFaceProperties);
        Color color = windmillColor;
        color.a = 1f;
        mixingColorFaceProperties.SetColor("_BaseColor", color);
        mixingColorFace.SetPropertyBlock(mixingColorFaceProperties);
        UpdateMixingRim(color, 1f);
    }

    private void UpdateMixingRim(Color color, float brightness)
    {
        if (mixingLightRim == null) return;
        if (mixingLightRimProperties == null) mixingLightRimProperties = new MaterialPropertyBlock();
        mixingLightRim.GetPropertyBlock(mixingLightRimProperties);
        Color glow = Color.Lerp(color, new Color(0.45f, 0.65f, 0.8f), 0.2f) * (0.65f * brightness);
        glow.a = 1f;
        mixingLightRimProperties.SetColor("_BaseColor", glow);
        mixingLightRim.SetPropertyBlock(mixingLightRimProperties);
    }

    public void HideStartScreens()
    {
        if (verticalStartCanvas != null) verticalStartCanvas.SetActive(false);
        if (horizontalStartCanvas != null) horizontalStartCanvas.SetActive(false);
    }

    public void ResetScene()
    {
        ResetReveal();
        if (_cgsa != null) _cgsa.ResetSelectedColor();
        windmillColor = new Color32(0, 0, 0, 255);
        UpdateMixingColorFace();

        foreach (var windmill in windmills)
        {
            windmill.ResetWindmill();
        }

        currentSelectedWindmill = null;
    }

    public Color CaptureMixedColor()
    {
        CombineLightSpeed();
        UpdateColors();
        return windmillColor;
    }

    private void CacheVisibleFlowerMaterials()
    {
        flowerMaterials.Clear();
        flowerStartColors.Clear();
        foreach (GameObject flower in flowersToColor)
        {
            if (flower == null) continue;
            foreach (Renderer renderer in flower.GetComponentsInChildren<Renderer>(true))
                foreach (Material material in renderer.materials)
                {
                    if (flowerMaterials.Contains(material) ||
                        (!material.HasProperty("_BaseColor") && !material.HasProperty("_Color"))) continue;
                    flowerMaterials.Add(material);
                    flowerStartColors.Add(ReadFlowerColor(material));
                }
        }
    }

    public IEnumerator RevealResult(Color mixedColor)
    {
        if (resultRevealed) yield break;
        resultRevealed = true;
        if (worldColorController == null)
            worldColorController = FindFirstObjectByType<WorldColorController>();
        if (worldColorController != null) worldWasGrayscale = worldColorController.IsGrayscale;
        if (mixingLightRim != null)
        {
            float flashTime = 0f;
            while (flashTime < 0.45f)
            {
                UpdateMixingRim(mixedColor, 1f + Mathf.Sin(flashTime / 0.45f * Mathf.PI) * 1.8f);
                flashTime += Time.unscaledDeltaTime;
                yield return null;
            }
            UpdateMixingRim(mixedColor, 1f);
        }
        if (pipeColorPulse != null)
            yield return pipeColorPulse.Play(mixedColor,
                glowingResultCube != null ? glowingResultCube.bounds.center : transform.position);
        mixedColor.a = 1f;
        Vector3 waveOrigin = lighthouseRoot != null ? lighthouseRoot.position : transform.position;
        BeginLampGlow(mixedColor);
        bool worldWave = worldColorController != null && worldColorController.BeginColorWave(waveOrigin, mixedColor, flowersToColor);
        PlayWorldColorMagic();
        flowerMaterials.Clear();
        flowerStartColors.Clear();
        if (!worldWave)
        {
            if (worldColorController != null) worldColorController.SetGrayscale(false);
            CacheVisibleFlowerMaterials();
        }
        windmillColor = mixedColor;
        if (lighthouseRoof == null)
            Debug.LogWarning("Leuchtturmdach fehlt: Lighthouse Roof im WindmillManager zuweisen.", this);
        Vector3 endPosition = roofStartPosition;
        if (lighthouseRoof != null)
            endPosition += lighthouseRoof.parent != null
                ? lighthouseRoof.parent.InverseTransformVector(Vector3.up * roofLiftHeight)
                : Vector3.up * roofLiftHeight;

        float elapsed = 0f;
        float totalDuration = worldWave ? Mathf.Max(revealDuration, worldWaveDuration + 0.25f) : revealDuration;
        while (elapsed < totalDuration)
        {
            float amount = Mathf.SmoothStep(0f, 1f, elapsed / revealDuration);
            ApplyReveal(amount, endPosition, mixedColor);
            float rise = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.15f, 1.4f, elapsed));
            float settle = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(revealDuration, totalDuration, elapsed));
            ApplyLampGlow(rise * Mathf.Lerp(lampPeakGlow, lampIdleGlow, settle));
            if (worldWave) worldColorController.UpdateColorWave(Mathf.Clamp01((elapsed - 0.25f) / worldWaveDuration));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        if (worldWave)
        {
            worldColorController.UpdateColorWave(1f);
            worldColorController.FinishColorWave();
            // Restore real materials before applying the lasting mixed flower color.
            CacheVisibleFlowerMaterials();
        }
        ApplyReveal(1f, endPosition, mixedColor);
        ApplyLampGlow(lampIdleGlow);
        if (resultHoldDuration > 0f) yield return new WaitForSecondsRealtime(resultHoldDuration);
    }

    private void ApplyReveal(float amount, Vector3 endPosition, Color mixedColor)
    {
        if (lighthouseRoof != null)
            lighthouseRoof.localPosition = Vector3.Lerp(roofStartPosition, endPosition, amount);
        for (int i = 0; i < flowerMaterials.Count; i++)
            WriteFlowerColor(flowerMaterials[i], Color.Lerp(flowerStartColors[i], mixedColor, amount));
    }

    private static Color ReadFlowerColor(Material material)
    {
        return material.GetColor(material.HasProperty("_BaseColor") ? "_BaseColor" : "_Color");
    }

    private static void WriteFlowerColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
    }

    public void ResetReveal()
    {
        if (worldColorMagicSource != null) worldColorMagicSource.Stop();
        ResetLampGlow();
        if (pipeColorPulse != null) pipeColorPulse.ResetPulse();
        if (lighthouseRoof != null) lighthouseRoof.localPosition = roofStartPosition;
        for (int i = 0; i < flowerMaterials.Count; i++) WriteFlowerColor(flowerMaterials[i], flowerStartColors[i]);
        if (resultRevealed && worldColorController != null)
            worldColorController.SetGrayscale(worldWasGrayscale);
        resultRevealed = false;
    }

    private void BeginLampGlow(Color color)
    {
        if (lighthouseLamp == null)
        {
            Debug.LogWarning("Leuchtturmlampe fehlt: Lighthouse Lamp im WindmillManager zuweisen.", this);
            return;
        }
        if (worldColorController != null) worldColorController.PreserveRendererForReveal(lighthouseLamp);
        lampOriginalMaterials = lighthouseLamp.sharedMaterials;
        lampGlowMaterials = lighthouseLamp.materials;
        lampColor = color;
        lampColor.a = 1f;
        foreach (Material material in lampGlowMaterials)
        {
            // Remove the baked blue tint from this lamp instance only.
            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", null);
            if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", null);
            WriteFlowerColor(material, lampColor);
            material.EnableKeyword("_EMISSION");
        }
        if (lampLight == null)
        {
            GameObject lightObject = new GameObject("Leuchtturm-Farblicht");
            lightObject.transform.SetParent(lighthouseRoot != null ? lighthouseRoot : transform, false);
            lightObject.transform.position = lighthouseLamp.bounds.center;
            lampLight = lightObject.AddComponent<Light>();
            lampLight.type = LightType.Point;
            lampLight.shadows = LightShadows.None;
        }
        lampLight.color = lampColor;
        lampLight.range = lampLightRange;
        ApplyLampGlow(0f);
    }

    private void ApplyLampGlow(float strength)
    {
        if (lampGlowMaterials == null) return;
        foreach (Material material in lampGlowMaterials)
            if (material != null && material.HasProperty("_EmissionColor"))
                material.SetColor("_EmissionColor", lampColor * strength);
        if (lampLight != null)
        {
            lampLight.intensity = strength * 0.6f;
            lampLight.enabled = strength > 0f;
        }
    }

    private void ResetLampGlow()
    {
        if (lighthouseLamp != null && lampOriginalMaterials != null)
            lighthouseLamp.sharedMaterials = lampOriginalMaterials;
        if (lampGlowMaterials != null)
            foreach (Material material in lampGlowMaterials) if (material != null) Destroy(material);
        lampGlowMaterials = null;
        lampOriginalMaterials = null;
        if (lampLight != null) lampLight.enabled = false;
    }

    private void OnDestroy()
    {
        ResetLampGlow();
        if (lampLight != null) Destroy(lampLight.gameObject);
        if (worldColorMagicSource != null) Destroy(worldColorMagicSource.gameObject);
    }

    private void PlayWorldColorMagic()
    {
        if (worldColorMagic == null) return;
        if (worldColorMagicSource == null)
        {
            GameObject audioObject = new GameObject("WorldColorMagicAudio");
            audioObject.transform.SetParent(transform, false);
            worldColorMagicSource = audioObject.AddComponent<AudioSource>();
            worldColorMagicSource.playOnAwake = false;
            worldColorMagicSource.spatialBlend = 0f;
            worldColorMagicSource.loop = false;
        }
        worldColorMagicSource.clip = worldColorMagic;
        worldColorMagicSource.volume = worldColorMagicVolume;
        worldColorMagicSource.Play();
    }

    private void UpdateWallColor()
    {
        CombineLightSpeed();
        particlesTop.startColor = windmillColor;
    }

    private void CombineLightSpeed()
    {
        if (windmills.Length > 0)
            windmillColor.r = (byte)windmills[0].GetCurrentSpeed();
        if (windmills.Length > 1)
            windmillColor.g = (byte)windmills[1].GetCurrentSpeed();
        if (windmills.Length > 2)
            windmillColor.b = (byte)windmills[2].GetCurrentSpeed();
    }

    public void LockAllExcept(Windmill clickedWindmill)
    {
        if (_cgsa != null && !_cgsa.WindmillsActive) return;
        if (clickedWindmill == currentSelectedWindmill)
        {
            clickedWindmill.ToggleRotationMode();
            clickedWindmill.isWindmillSelected = false;
            currentSelectedWindmill = null;
        }
        else
        {
            foreach (var windmill in windmills)
            {
                if (windmill == clickedWindmill)
                {
                    windmill.isWindmillSelected = true;
                    windmill.rotor.constRotationSpeed = -1f;
                    windmill.SelectWindmill();
                    currentSelectedWindmill = windmill;

                    int index = System.Array.IndexOf(windmills, windmill);
                    if (ghostController != null)
                    {
                        ghostController.FlyToWindmill(index);
                    }
                }
                else
                {
                    windmill.isWindmillSelected = false;
                    windmill.rotor.constRotationSpeed = windmill.rotor.currentSpeed;
                }
            }
        }
    }

    public void MoveGhostToCurrentWindmill()
    {
        if (ghostController == null || currentSelectedWindmill == null)
        {
            return;
        }

        int index = System.Array.IndexOf(windmills, currentSelectedWindmill);
        if (index >= 0)
        {
            ghostController.FlyToWindmill(index);
        }
    }

    private void CheckIfAllLocked()
    {
        if (allWindmillsLocked)
            return;

        bool allLocked = true;
        foreach (var windmill in windmills)
        {
            if (!windmill.IsWindmillLocked())
            {
                allLocked = false;
                break;
            }
        }

        if (allLocked)
        {
            allWindmillsLocked = true;
        }
    }

    public void LoadEndScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
