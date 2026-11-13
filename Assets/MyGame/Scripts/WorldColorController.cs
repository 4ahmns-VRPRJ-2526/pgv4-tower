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
        if (isGrayscale != startInGrayscale)
            SetGrayscale(startInGrayscale);
    }

    public void SetGrayscale(bool enabled)
    {
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
