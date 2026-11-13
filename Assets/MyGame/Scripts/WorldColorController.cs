using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple material-swap prototype. Does not change the skybox or source materials.
/// Other scripts must not recolor the selected renderers while grayscale is active.
/// </summary>
public class WorldColorController : MonoBehaviour
{
    [SerializeField] private bool collectOnStart = true;
    [Tooltip("Additional objects whose entire hierarchy stays unchanged, e.g. Fino or the Ghost mascot.")]
    [SerializeField] private Transform[] excludedRoots = new Transform[0];
    [Tooltip("Case-insensitive matches against renderer and parent names. LightCube preserves the mixing chamber.")]
    [SerializeField] private string[] excludedNameParts = { "windmill", "pipe", "rohr", "lightcube" };
    [SerializeField] private bool startInGrayscale = true;
    [Tooltip("Optional neutral URP/Lit material. A temporary gray material is created if empty.")]
    [SerializeField] private Material grayscaleMaterial;
    [Tooltip("Only these renderers are changed. Leave windmills, color flow, mixing face and UI out.")]
    [SerializeField] private Renderer[] environmentRenderers = new Renderer[0];

    private readonly Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    private Material generatedMaterial;
    private bool initialized;
    private bool isGrayscale; 
    public bool IsGrayscale => isGrayscale;
    [Header("Lichtwelle beim Abschluss")]
    [SerializeField] private Material colorWaveMaterial;
    [SerializeField, Min(0.1f)] private float waveSoftness = 5f;
    private readonly List<Material> waveMaterials = new List<Material>();
    private readonly HashSet<Renderer> revealPreservedRenderers = new HashSet<Renderer>();
    private bool waveActive;
    private float waveRadius;

    private void Start()
    {
        if (collectOnStart) CollectEnvironmentRenderers();
        initialized = true;
        SetGrayscale(startInGrayscale);
    }

    [ContextMenu("Collect Environment Renderers")]
    public void CollectEnvironmentRenderers()
    {
        bool reapply = Application.isPlaying && isGrayscale;
        if (reapply)
        {
            RestoreMaterials();
            isGrayscale = false;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
            UnityEditor.Undo.RecordObject(this, "Collect Environment Renderers");
#endif

        var found = new List<Renderer>();
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
        {
            foreach (Renderer candidate in root.GetComponentsInChildren<Renderer>(true))
            {
                // Keep particle effects and UI independent from the world material swap.
                if (!(candidate is MeshRenderer) && !(candidate is SkinnedMeshRenderer)) continue;
                if (candidate.GetComponentInParent<Canvas>(true) != null) continue;
                if (IsExcluded(candidate.transform)) continue;
                found.Add(candidate);
            }
        }
        environmentRenderers = found.ToArray();

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif
        Debug.Log($"WorldColorController: Collected {environmentRenderers.Length} environment renderers.", this);
        if (reapply) SetGrayscale(true);
    }

    private bool IsExcluded(Transform target)
    {
        foreach (Transform root in excludedRoots)
            if (root != null && (target == root || target.IsChildOf(root))) return true;

        for (Transform current = target; current != null; current = current.parent)
        {
            if (current.GetComponent<Windmill>() != null) return true;
            // Built-in exceptions also apply to components already serialized in scenes.
            if (current.GetComponent<GhostController>() != null ||
                current.name.Equals("GhostExport", System.StringComparison.OrdinalIgnoreCase) ||
                current.name.IndexOf("fino", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            foreach (string namePart in excludedNameParts)
                if (!string.IsNullOrWhiteSpace(namePart) &&
                    current.name.IndexOf(namePart, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
        }
        return false;
    }

    private void Update()
    {
        // Also permits toggling the checkbox in the Inspector during Play Mode.
        if (!waveActive && isGrayscale != startInGrayscale)
            SetGrayscale(startInGrayscale);
    }

    public void SetGrayscale(bool enabled)
    {
        if (waveActive)
        {
            RestoreMaterials();
            isGrayscale = false;
        }
        startInGrayscale = enabled;
        if (!Application.isPlaying) return;

        if (!enabled)
        {
            RestoreMaterials();
            isGrayscale = false;
            return;
        }

        if (isGrayscale) return;

        Material replacement = grayscaleMaterial;
        if (replacement == null)
        {
            if (generatedMaterial == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    Debug.LogError("WorldColorController: URP/Lit not found. Assign a gray material in the Inspector.", this);
                    startInGrayscale = false;
                    return;
                }

                generatedMaterial = new Material(shader) { name = "World Gray (Runtime)" };
                generatedMaterial.SetColor("_BaseColor", new Color(0.65f, 0.65f, 0.65f, 1f));
                generatedMaterial.SetFloat("_Metallic", 0f);
                generatedMaterial.SetFloat("_Smoothness", 0.2f);
            }
            replacement = generatedMaterial;
        }

        originalMaterials.Clear();
        foreach (Renderer target in environmentRenderers)
        {
            if (target == null || originalMaterials.ContainsKey(target)) continue;

            Material[] originals = target.sharedMaterials;
            originalMaterials.Add(target, originals);
            Material[] replacements = new Material[originals.Length];
            for (int i = 0; i < replacements.Length; i++)
                replacements[i] = replacement;
            target.sharedMaterials = replacements;
        }
        isGrayscale = true;
    }

    private void RestoreMaterials()
    {
        foreach (KeyValuePair<Renderer, Material[]> entry in originalMaterials)
            if (entry.Key != null) entry.Key.sharedMaterials = entry.Value;
        originalMaterials.Clear();
        foreach (Material material in waveMaterials) if (material != null) Destroy(material);
        waveMaterials.Clear();
        waveActive = false;
        revealPreservedRenderers.Clear();
    }

    public bool BeginColorWave(Vector3 origin, Color mixedColor, GameObject[] flowers)
    {
        if (colorWaveMaterial == null || !colorWaveMaterial.shader.isSupported)
        {
            Debug.LogWarning("WorldColorController: Lichtwellen-Material fehlt oder Shader wird nicht unterstützt; direkter Farbwechsel.", this);
            return false;
        }
        var flowerRenderers = new HashSet<Renderer>();
        foreach (GameObject flower in flowers)
            if (flower != null)
                foreach (Renderer renderer in flower.GetComponentsInChildren<Renderer>(true)) flowerRenderers.Add(renderer);
        if (originalMaterials.Count == 0)
            foreach (Renderer renderer in environmentRenderers)
                if (renderer != null && !revealPreservedRenderers.Contains(renderer) && !originalMaterials.ContainsKey(renderer))
                    originalMaterials.Add(renderer, renderer.sharedMaterials);
        waveRadius = waveSoftness;
        foreach (var entry in originalMaterials)
        {
            if (entry.Key == null) continue;
            Bounds bounds = entry.Key.bounds;
            Vector2 delta = new Vector2(bounds.center.x - origin.x, bounds.center.z - origin.z);
            waveRadius = Mathf.Max(waveRadius, delta.magnitude + new Vector2(bounds.extents.x, bounds.extents.z).magnitude);
            Material[] materials = new Material[entry.Value.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                Material original = entry.Value[i];
                Material material = new Material(colorWaveMaterial) { name = "World Color Wave (Runtime)" };
                Color target = Color.white;
                if (original != null)
                {
                    string colorProperty = original.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
                    if (original.HasProperty(colorProperty)) target = original.GetColor(colorProperty);
                    string textureProperty = original.HasProperty("_BaseMap") ? "_BaseMap" : "_MainTex";
                    if (original.HasProperty(textureProperty))
                    {
                        material.SetTexture("_BaseMap", original.GetTexture(textureProperty));
                        material.SetTextureScale("_BaseMap", original.GetTextureScale(textureProperty));
                        material.SetTextureOffset("_BaseMap", original.GetTextureOffset(textureProperty));
                    }
                }
                if (flowerRenderers.Contains(entry.Key)) target = mixedColor;
                material.SetColor("_BaseColor", target);
                material.SetColor("_GrayColor", isGrayscale ? new Color(0.65f, 0.65f, 0.65f) : target);
                materials[i] = material;
                waveMaterials.Add(material);
            }
            entry.Key.sharedMaterials = materials;
        }
        Shader.SetGlobalVector("_WorldWaveOrigin", origin);
        Shader.SetGlobalColor("_WorldWaveColor", mixedColor);
        Shader.SetGlobalFloat("_WorldWaveSoftness", waveSoftness);
        waveActive = true;
        UpdateColorWave(0f);
        return true;
    }

    public void UpdateColorWave(float progress)
    {
        if (!waveActive) return;
        Shader.SetGlobalFloat("_WorldWaveRadius", Mathf.Lerp(-waveSoftness, waveRadius + waveSoftness * 2f, Mathf.Clamp01(progress)));
        float rise = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, 0.12f, progress));
        float fade = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.8f, 1f, progress));
        Shader.SetGlobalFloat("_WorldWaveGlow", rise * fade);
    }

    public void FinishColorWave() => SetGrayscale(false);

    public void PreserveRendererForReveal(Renderer renderer)
    {
        if (renderer == null) return;
        revealPreservedRenderers.Add(renderer);
        if (originalMaterials.TryGetValue(renderer, out Material[] materials))
        {
            renderer.sharedMaterials = materials;
            originalMaterials.Remove(renderer);
        }
    }

    private void OnDisable()
    {
        RestoreMaterials();
        isGrayscale = false;
    }

    private void OnEnable()
    {
        if (initialized) SetGrayscale(startInGrayscale);
    }

    private void OnDestroy()
    {
        RestoreMaterials();
        if (generatedMaterial != null) Destroy(generatedMaterial);
    }
}
