using System.IO;
using UnityEditor;
using UnityEngine;

public static class GltfMaterialToUrpConverter
{
    private const string SourceShaderName = "glTF/PbrMetallicRoughness";
    private const string TargetShaderName = "Universal Render Pipeline/Lit";
    private const string OutputFolder = "Assets/ConvertedMaterials";

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
        int convertedCount = 0;

        foreach (string guid in materialGuids)
        {
            string materialPath = AssetDatabase.GUIDToAssetPath(guid);
            Material sourceMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

            if (sourceMaterial == null || sourceMaterial.shader == null)
            {
                continue;
            }

            if (sourceMaterial.shader.name != SourceShaderName)
            {
                continue;
            }

            Material urpMaterial = new Material(urpLitShader)
            {
                name = sourceMaterial.name + "_URP"
            };

            CopyBaseColor(sourceMaterial, urpMaterial);
            CopyBaseMap(sourceMaterial, urpMaterial);
            CopyMetallic(sourceMaterial, urpMaterial);
            CopySmoothnessFromRoughness(sourceMaterial, urpMaterial);

            string assetPath = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(OutputFolder, urpMaterial.name + ".mat").Replace("\\", "/"));

            AssetDatabase.CreateAsset(urpMaterial, assetPath);
            convertedCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Converted {convertedCount} glTF materials to editable URP material copies.");
    }

    private static void EnsureOutputFolderExists()
    {
        if (!AssetDatabase.IsValidFolder(OutputFolder))
        {
            AssetDatabase.CreateFolder("Assets", "ConvertedMaterials");
        }
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

    private static void CopyBaseMap(Material sourceMaterial, Material targetMaterial)
    {
        string sourceTextureProperty = GetFirstExistingProperty(
            sourceMaterial,
            "_BaseColorMap",
            "_BaseColorTexture",
            "_baseColorTexture",
            "_BaseMap",
            "_MainTex");

        if (!string.IsNullOrEmpty(sourceTextureProperty) && targetMaterial.HasProperty("_BaseMap"))
        {
            Texture texture = sourceMaterial.GetTexture(sourceTextureProperty);
            targetMaterial.SetTexture("_BaseMap", texture);
            targetMaterial.SetTextureScale("_BaseMap", sourceMaterial.GetTextureScale(sourceTextureProperty));
            targetMaterial.SetTextureOffset("_BaseMap", sourceMaterial.GetTextureOffset(sourceTextureProperty));
        }
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
}
