using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class GltfMaterialToUrpConverter
{
    private const string SourceShaderName = "glTF/PbrMetallicRoughness";
    private const string TargetShaderName = "Universal Render Pipeline/Lit";
    private const string OutputFolder = "Assets/ConvertedMaterials";
    private const string UmgebungGltfPath = "Assets/MyGame/Implement/Umgebung25.6/Umgebung25.6/Umgebung25.6.gltf";
    private const string UmgebungOutputFolder = "Assets/ConvertedMaterials/Umgebung25_6";
    private const string WindmillGltfPath = "Assets/MyGame/Implement/Windmuehle28.5_v2/Windmuehle28.5_v2/WindmuehleExport.gltf";
    private const string WindmillOutputFolder = "Assets/ConvertedMaterials/Windmills";
    private const string FireflyGltfPath = "Assets/MyGame/Implement/Firefly11.6/Firefly11.6/FireflyExport.gltf";
    private const string FireflyOutputFolder = "Assets/ConvertedMaterials/Fireflies";
    private const string GeistGltfPath = "Assets/MyGame/Implement/Geist/Geist/GhostExport.gltf";
    private const string GeistOutputFolder = "Assets/ConvertedMaterials/Geist";
    private static readonly Dictionary<string, GltfRoot> GltfCache = new Dictionary<string, GltfRoot>();

    [MenuItem("Tools/Materials/Convert glTF Materials to URP Copies")]
    public static void ConvertGltfMaterialsToUrpCopies()
    {
        Shader urpLitShader = Shader.Find(TargetShaderName);
        if (urpLitShader == null)
        {
            Debug.LogError($"URP Lit Shader not found: {TargetShaderName}");
            return;
        }

        EnsureOutputFolderExists();

        string[] materialGuids = AssetDatabase.FindAssets("t:Material");
        HashSet<string> processedPaths = new HashSet<string>();
        HashSet<string> convertedGltfPaths = new HashSet<string>();
        int convertedCount = 0;

        foreach (string guid in materialGuids)
        {
            string materialPath = AssetDatabase.GUIDToAssetPath(guid);
            if (!processedPaths.Add(materialPath))
            {
                continue;
            }

            Object[] assetsAtPath = AssetDatabase.LoadAllAssetsAtPath(materialPath);
            bool convertedAnyAtPath = false;

            foreach (Object asset in assetsAtPath)
            {
                if (asset is not Material sourceMaterial)
                {
                    continue;
                }

                if (sourceMaterial.shader == null || sourceMaterial.shader.name != SourceShaderName)
                {
                    continue;
                }

                Material urpMaterial = new Material(urpLitShader)
                {
                    name = sourceMaterial.name + "_URP"
                };

                CopyBaseColor(sourceMaterial, urpMaterial);
                if (!CopyBaseMap(sourceMaterial, urpMaterial))
                {
                    CopyBaseMapFromGltf(materialPath, sourceMaterial, urpMaterial);
                }

                CopyMetallic(sourceMaterial, urpMaterial);
                CopySmoothnessFromRoughness(sourceMaterial, urpMaterial);

                string assetPath = AssetDatabase.GenerateUniqueAssetPath(
                    Path.Combine(OutputFolder, urpMaterial.name + ".mat").Replace("\\", "/"));

                AssetDatabase.CreateAsset(urpMaterial, assetPath);
                convertedCount++;
                convertedAnyAtPath = true;
            }

            if (convertedAnyAtPath && Path.GetExtension(materialPath).ToLowerInvariant() == ".gltf")
            {
                convertedGltfPaths.Add(materialPath);
            }
        }

        foreach (string assetPath in AssetDatabase.GetAllAssetPaths())
        {
            if (Path.GetExtension(assetPath).ToLowerInvariant() != ".gltf" || convertedGltfPaths.Contains(assetPath))
            {
                continue;
            }

            convertedCount += ConvertGltfMaterialsFromJson(assetPath, urpLitShader);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Converted {convertedCount} glTF materials to editable URP material copies.");
    }

    [MenuItem("Tools/Materials/Convert and Assign Umgebung Materials")]
    public static void ConvertAndAssignUmgebungMaterials()
    {
        ConvertAndAssignGltfMaterials(UmgebungGltfPath, UmgebungOutputFolder, "Umgebung");
    }

    [MenuItem("Tools/Materials/Convert and Assign Windmill Materials")]
    public static void ConvertAndAssignWindmillMaterials()
    {
        ConvertAndAssignGltfMaterials(WindmillGltfPath, WindmillOutputFolder, "Windmill");
    }

    [MenuItem("Tools/Materials/Convert and Assign Firefly Materials")]
    public static void ConvertAndAssignFireflyMaterials()
    {
        ConvertAndAssignGltfMaterials(FireflyGltfPath, FireflyOutputFolder, "Firefly");
    }

    [MenuItem("Tools/Materials/Convert and Assign Geist Materials")]
    public static void ConvertAndAssignGeistMaterials()
    {
        ConvertAndAssignGltfMaterials(GeistGltfPath, GeistOutputFolder, "Geist");
    }

    [MenuItem("Tools/Materials/Assign Umgebung Materials In Open Scene")]
    public static void AssignUmgebungMaterialsInOpenScene()
    {
        AssignGltfMaterialsInOpenScene(UmgebungGltfPath, UmgebungOutputFolder, "Umgebung");
    }

    [MenuItem("Tools/Materials/Assign Windmill Materials In Open Scene")]
    public static void AssignWindmillMaterialsInOpenScene()
    {
        AssignGltfMaterialsInOpenScene(WindmillGltfPath, WindmillOutputFolder, "Windmill");
    }

    [MenuItem("Tools/Materials/Assign Firefly Materials In Open Scene")]
    public static void AssignFireflyMaterialsInOpenScene()
    {
        AssignGltfMaterialsInOpenScene(FireflyGltfPath, FireflyOutputFolder, "Firefly");
    }

    [MenuItem("Tools/Materials/Assign Geist Materials In Open Scene")]
    public static void AssignGeistMaterialsInOpenScene()
    {
        AssignGltfMaterialsInOpenScene(GeistGltfPath, GeistOutputFolder, "Geist");
    }

    private static void ConvertAndAssignGltfMaterials(string gltfPath, string outputFolder, string label)
    {
        Shader urpLitShader = Shader.Find(TargetShaderName);
        if (urpLitShader == null)
        {
            Debug.LogError($"URP Lit Shader not found: {TargetShaderName}");
            return;
        }

        GltfRoot gltf = LoadGltf(gltfPath);
        if (gltf == null || gltf.materials == null)
        {
            Debug.LogError($"Could not read glTF materials from: {gltfPath}");
            return;
        }

        EnsureOutputFolderExists();
        EnsureFolderExists(outputFolder);

        AssetImporter importer = AssetImporter.GetAtPath(gltfPath);
        if (importer == null)
        {
            Debug.LogError($"Could not get importer for: {gltfPath}");
            return;
        }

        int convertedCount = 0;
        int assignedCount = 0;

        foreach (GltfMaterial gltfMaterial in gltf.materials)
        {
            if (gltfMaterial == null || string.IsNullOrEmpty(gltfMaterial.name))
            {
                continue;
            }

            Material urpMaterial = CreateOrUpdateUrpMaterialFromGltf(gltfPath, gltfMaterial, urpLitShader, outputFolder);
            if (urpMaterial == null)
            {
                continue;
            }

            AssetImporter.SourceAssetIdentifier sourceIdentifier =
                new AssetImporter.SourceAssetIdentifier(typeof(Material), gltfMaterial.name);

            importer.AddRemap(sourceIdentifier, urpMaterial);
            convertedCount++;
            assignedCount++;
        }

        AssetDatabase.SaveAssets();
        importer.SaveAndReimport();
        AssetDatabase.Refresh();

        Debug.Log($"Converted {convertedCount} {label} materials and assigned {assignedCount} remaps to {gltfPath}.");
    }

    private static void AssignGltfMaterialsInOpenScene(string gltfPath, string outputFolder, string label)
    {
        ConvertAndAssignGltfMaterials(gltfPath, outputFolder, label);

        Dictionary<string, Material> materialMap = BuildGltfMaterialMap(gltfPath, outputFolder);
        if (materialMap.Count == 0)
        {
            Debug.LogWarning($"No converted {label} URP materials found.");
            return;
        }

        Renderer[] renderers = GetTargetRenderers();
        int rendererCount = 0;
        int slotCount = 0;

        foreach (Renderer renderer in renderers)
        {
            Material[] sharedMaterials = renderer.sharedMaterials;
            bool changed = false;

            for (int i = 0; i < sharedMaterials.Length; i++)
            {
                Material currentMaterial = sharedMaterials[i];
                if (currentMaterial == null)
                {
                    continue;
                }

                string materialKey = NormalizeMaterialName(currentMaterial.name);
                if (!materialMap.TryGetValue(materialKey, out Material replacementMaterial))
                {
                    continue;
                }

                if (currentMaterial == replacementMaterial)
                {
                    continue;
                }

                sharedMaterials[i] = replacementMaterial;
                changed = true;
                slotCount++;
            }

            if (!changed)
            {
                continue;
            }

            Undo.RecordObject(renderer, $"Assign {label} URP Materials");
            renderer.sharedMaterials = sharedMaterials;
            EditorUtility.SetDirty(renderer);
            EditorSceneManager.MarkSceneDirty(renderer.gameObject.scene);
            rendererCount++;
        }

        Debug.Log($"Assigned {label} URP materials on {rendererCount} renderers and {slotCount} material slots.");
    }

    private static void EnsureOutputFolderExists()
    {
        if (!AssetDatabase.IsValidFolder(OutputFolder))
        {
            AssetDatabase.CreateFolder("Assets", "ConvertedMaterials");
        }
    }

    private static void EnsureFolderExists(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string parent = Path.GetDirectoryName(folderPath).Replace("\\", "/");
        string folderName = Path.GetFileName(folderPath);

        if (!AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolderExists(parent);
        }

        AssetDatabase.CreateFolder(parent, folderName);
    }

    private static int ConvertGltfMaterialsFromJson(string materialPath, Shader urpLitShader)
    {
        GltfRoot gltf = LoadGltf(materialPath);
        if (gltf == null || gltf.materials == null)
        {
            return 0;
        }

        int convertedCount = 0;

        foreach (GltfMaterial gltfMaterial in gltf.materials)
        {
            if (gltfMaterial == null || string.IsNullOrEmpty(gltfMaterial.name))
            {
                continue;
            }

            Material urpMaterial = new Material(urpLitShader)
            {
                name = gltfMaterial.name + "_URP"
            };

            ApplyGltfMaterialToUrpMaterial(materialPath, gltfMaterial, urpMaterial);

            string assetPath = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(OutputFolder, urpMaterial.name + ".mat").Replace("\\", "/"));

            AssetDatabase.CreateAsset(urpMaterial, assetPath);
            convertedCount++;
        }

        return convertedCount;
    }

    private static Material CreateOrUpdateUrpMaterialFromGltf(
        string materialPath,
        GltfMaterial gltfMaterial,
        Shader urpLitShader,
        string outputFolder)
    {
        string materialName = gltfMaterial.name + "_URP";
        string assetPath = Path.Combine(outputFolder, SanitizeFileName(materialName) + ".mat").Replace("\\", "/");
        Material urpMaterial = AssetDatabase.LoadAssetAtPath<Material>(assetPath);

        if (urpMaterial == null)
        {
            urpMaterial = new Material(urpLitShader)
            {
                name = materialName
            };

            AssetDatabase.CreateAsset(urpMaterial, assetPath);
        }
        else
        {
            urpMaterial.shader = urpLitShader;
        }

        ApplyGltfMaterialToUrpMaterial(materialPath, gltfMaterial, urpMaterial);
        EditorUtility.SetDirty(urpMaterial);
        return urpMaterial;
    }

    private static Dictionary<string, Material> BuildGltfMaterialMap(string gltfPath, string outputFolder)
    {
        Dictionary<string, Material> materialMap = new Dictionary<string, Material>();
        GltfRoot gltf = LoadGltf(gltfPath);
        if (gltf == null || gltf.materials == null)
        {
            return materialMap;
        }

        HashSet<string> gltfMaterialNames = new HashSet<string>();
        foreach (GltfMaterial gltfMaterial in gltf.materials)
        {
            if (gltfMaterial != null && !string.IsNullOrEmpty(gltfMaterial.name))
            {
                gltfMaterialNames.Add(gltfMaterial.name);
            }
        }

        string[] materialGuids = AssetDatabase.FindAssets("t:Material");
        foreach (string guid in materialGuids)
        {
            string materialPath = AssetDatabase.GUIDToAssetPath(guid);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null || material.shader == null || material.shader.name != TargetShaderName)
            {
                continue;
            }

            string materialKey = NormalizeMaterialName(material.name);
            if (!gltfMaterialNames.Contains(materialKey))
            {
                continue;
            }

            if (!materialMap.ContainsKey(materialKey) || materialPath.StartsWith(outputFolder))
            {
                materialMap[materialKey] = material;
            }
        }

        return materialMap;
    }

    private static Renderer[] GetTargetRenderers()
    {
        GameObject[] selectedGameObjects = Selection.gameObjects;
        if (selectedGameObjects != null && selectedGameObjects.Length > 0)
        {
            List<Renderer> selectedRenderers = new List<Renderer>();
            foreach (GameObject selectedGameObject in selectedGameObjects)
            {
                selectedRenderers.AddRange(selectedGameObject.GetComponentsInChildren<Renderer>(true));
            }

            return selectedRenderers.ToArray();
        }

        return Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }

    private static void ApplyGltfMaterialToUrpMaterial(string materialPath, GltfMaterial gltfMaterial, Material targetMaterial)
    {
        CopyBaseColorFromGltf(gltfMaterial, targetMaterial);
        CopyBaseMapFromGltf(materialPath, gltfMaterial, targetMaterial);
        CopyMetallicFromGltf(gltfMaterial, targetMaterial);
        CopySmoothnessFromGltf(gltfMaterial, targetMaterial);
        CopyDoubleSidedFromGltf(gltfMaterial, targetMaterial);
    }

    private static void CopyBaseColor(Material sourceMaterial, Material targetMaterial)
    {
        Color color = GetColor(
            sourceMaterial,
            Color.white,
            "_BaseColor",
            "_BaseColorFactor",
            "_baseColorFactor",
            "_Color");

        if (targetMaterial.HasProperty("_BaseColor"))
        {
            targetMaterial.SetColor("_BaseColor", color);
        }
    }

    private static bool CopyBaseMap(Material sourceMaterial, Material targetMaterial)
    {
        string sourceTextureProperty = GetFirstExistingProperty(
            sourceMaterial,
            "_BaseColorMap",
            "_BaseColorTexture",
            "_baseColorTexture",
            "_BaseMap",
            "_MainTex");

        if (string.IsNullOrEmpty(sourceTextureProperty) || !targetMaterial.HasProperty("_BaseMap"))
        {
            return false;
        }

        Texture texture = sourceMaterial.GetTexture(sourceTextureProperty);
        if (texture == null)
        {
            return false;
        }

        targetMaterial.SetTexture("_BaseMap", texture);
        targetMaterial.SetTextureScale("_BaseMap", sourceMaterial.GetTextureScale(sourceTextureProperty));
        targetMaterial.SetTextureOffset("_BaseMap", sourceMaterial.GetTextureOffset(sourceTextureProperty));
        return true;
    }

    private static void CopyBaseMapFromGltf(string materialPath, Material sourceMaterial, Material targetMaterial)
    {
        if (!targetMaterial.HasProperty("_BaseMap") || Path.GetExtension(materialPath).ToLowerInvariant() != ".gltf")
        {
            return;
        }

        GltfRoot gltf = LoadGltf(materialPath);
        if (gltf == null || gltf.materials == null || gltf.textures == null || gltf.images == null)
        {
            return;
        }

        GltfMaterial gltfMaterial = null;
        foreach (GltfMaterial material in gltf.materials)
        {
            if (material != null && material.name == sourceMaterial.name)
            {
                gltfMaterial = material;
                break;
            }
        }

        int textureIndex = gltfMaterial?.pbrMetallicRoughness?.baseColorTexture?.index ?? -1;
        if (textureIndex < 0 || textureIndex >= gltf.textures.Length)
        {
            return;
        }

        int imageIndex = gltf.textures[textureIndex].source;
        if (imageIndex < 0 || imageIndex >= gltf.images.Length || string.IsNullOrEmpty(gltf.images[imageIndex].uri))
        {
            return;
        }

        string texturePath = Path.Combine(Path.GetDirectoryName(materialPath), gltf.images[imageIndex].uri).Replace("\\", "/");
        Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(texturePath);
        if (texture != null)
        {
            targetMaterial.SetTexture("_BaseMap", texture);
        }
    }

    private static void CopyBaseColorFromGltf(GltfMaterial gltfMaterial, Material targetMaterial)
    {
        float[] baseColorFactor = gltfMaterial.pbrMetallicRoughness?.baseColorFactor;
        if (baseColorFactor == null || baseColorFactor.Length < 3 || !targetMaterial.HasProperty("_BaseColor"))
        {
            return;
        }

        float alpha = baseColorFactor.Length > 3 ? baseColorFactor[3] : 1f;
        targetMaterial.SetColor(
            "_BaseColor",
            new Color(baseColorFactor[0], baseColorFactor[1], baseColorFactor[2], alpha));
    }

    private static void CopyBaseMapFromGltf(string materialPath, GltfMaterial gltfMaterial, Material targetMaterial)
    {
        int textureIndex = gltfMaterial.pbrMetallicRoughness?.baseColorTexture?.index ?? -1;
        SetBaseMapFromGltfTextureIndex(materialPath, textureIndex, targetMaterial);
    }

    private static void SetBaseMapFromGltfTextureIndex(string materialPath, int textureIndex, Material targetMaterial)
    {
        if (!targetMaterial.HasProperty("_BaseMap") || textureIndex < 0)
        {
            return;
        }

        GltfRoot gltf = LoadGltf(materialPath);
        if (gltf == null || gltf.textures == null || gltf.images == null || textureIndex >= gltf.textures.Length)
        {
            return;
        }

        int imageIndex = gltf.textures[textureIndex].source;
        if (imageIndex < 0 || imageIndex >= gltf.images.Length || string.IsNullOrEmpty(gltf.images[imageIndex].uri))
        {
            return;
        }

        string texturePath = Path.Combine(Path.GetDirectoryName(materialPath), gltf.images[imageIndex].uri).Replace("\\", "/");
        Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(texturePath);
        if (texture != null)
        {
            targetMaterial.SetTexture("_BaseMap", texture);
        }
    }

    private static void CopyMetallicFromGltf(GltfMaterial gltfMaterial, Material targetMaterial)
    {
        if (targetMaterial.HasProperty("_Metallic"))
        {
            targetMaterial.SetFloat("_Metallic", gltfMaterial.pbrMetallicRoughness?.metallicFactor ?? 0f);
        }
    }

    private static void CopySmoothnessFromGltf(GltfMaterial gltfMaterial, Material targetMaterial)
    {
        if (targetMaterial.HasProperty("_Smoothness"))
        {
            float roughness = gltfMaterial.pbrMetallicRoughness?.roughnessFactor ?? 0.5f;
            targetMaterial.SetFloat("_Smoothness", 1f - Mathf.Clamp01(roughness));
        }
    }

    private static void CopyDoubleSidedFromGltf(GltfMaterial gltfMaterial, Material targetMaterial)
    {
        if (gltfMaterial.doubleSided && targetMaterial.HasProperty("_Cull"))
        {
            targetMaterial.SetFloat("_Cull", 0f);
        }
    }

    private static GltfRoot LoadGltf(string materialPath)
    {
        if (GltfCache.TryGetValue(materialPath, out GltfRoot cachedGltf))
        {
            return cachedGltf;
        }

        string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), materialPath);
        if (!File.Exists(absolutePath))
        {
            return null;
        }

        GltfRoot gltf = JsonUtility.FromJson<GltfRoot>(File.ReadAllText(absolutePath));
        GltfCache[materialPath] = gltf;
        return gltf;
    }

    private static void CopyMetallic(Material sourceMaterial, Material targetMaterial)
    {
        float metallic = GetFloat(
            sourceMaterial,
            0f,
            "_Metallic",
            "_MetallicFactor",
            "_metallicFactor");

        if (targetMaterial.HasProperty("_Metallic"))
        {
            targetMaterial.SetFloat("_Metallic", metallic);
        }
    }

    private static void CopySmoothnessFromRoughness(Material sourceMaterial, Material targetMaterial)
    {
        float smoothness;
        string roughnessProperty = GetFirstExistingProperty(
            sourceMaterial,
            "_RoughnessFactor",
            "_roughnessFactor",
            "_Roughness",
            "_roughness");

        if (!string.IsNullOrEmpty(roughnessProperty))
        {
            smoothness = 1f - Mathf.Clamp01(sourceMaterial.GetFloat(roughnessProperty));
        }
        else
        {
            smoothness = GetFloat(sourceMaterial, 0.5f, "_Smoothness", "_Glossiness");
        }

        if (targetMaterial.HasProperty("_Smoothness"))
        {
            targetMaterial.SetFloat("_Smoothness", Mathf.Clamp01(smoothness));
        }
    }

    private static Color GetColor(Material material, Color fallback, params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            if (material.HasProperty(propertyName))
            {
                return material.GetColor(propertyName);
            }
        }

        return fallback;
    }

    private static float GetFloat(Material material, float fallback, params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            if (material.HasProperty(propertyName))
            {
                return material.GetFloat(propertyName);
            }
        }

        return fallback;
    }

    private static string GetFirstExistingProperty(Material material, params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            if (material.HasProperty(propertyName))
            {
                return propertyName;
            }
        }

        return null;
    }

    private static string SanitizeFileName(string fileName)
    {
        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(invalidChar, '_');
        }

        return fileName;
    }

    private static string NormalizeMaterialName(string materialName)
    {
        if (string.IsNullOrEmpty(materialName))
        {
            return string.Empty;
        }

        materialName = materialName.Replace(" (Instance)", string.Empty);

        int urpIndex = materialName.IndexOf("_URP", System.StringComparison.Ordinal);
        if (urpIndex >= 0)
        {
            materialName = materialName.Substring(0, urpIndex);
        }

        return materialName.Trim();
    }

    [System.Serializable]
    private class GltfRoot
    {
        public GltfMaterial[] materials;
        public GltfTexture[] textures;
        public GltfImage[] images;
    }

    [System.Serializable]
    private class GltfMaterial
    {
        public string name;
        public bool doubleSided;
        public GltfPbrMetallicRoughness pbrMetallicRoughness;
    }

    [System.Serializable]
    private class GltfPbrMetallicRoughness
    {
        public GltfTextureInfo baseColorTexture;
        public float[] baseColorFactor;
        public float metallicFactor = 0f;
        public float roughnessFactor = 0.5f;
    }

    [System.Serializable]
    private class GltfTextureInfo
    {
        public int index = -1;
    }

    [System.Serializable]
    private class GltfTexture
    {
        public int source = -1;
    }

    [System.Serializable]
    private class GltfImage
    {
        public string uri;
    }
}
