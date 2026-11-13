using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>A short colored light band travelling along the existing pipe's actual path.</summary>
public sealed class PipeColorPulse : MonoBehaviour
{
    [SerializeField] private LineRenderer pipe;
    [SerializeField] private Material pulseMaterial;
    [SerializeField, Min(0.1f)] private float duration = 2.4f;
    [SerializeField, Min(0.05f)] private float pulseLength = 2f;
    [SerializeField, Min(0.01f)] private float width = 0.34f;
    private LineRenderer core, halo;
    private Material coreMaterial, haloMaterial;
    private readonly List<Vector3> visiblePoints = new List<Vector3>();

    public IEnumerator Play(Color color, Vector3 mixingPosition)
    {
        ResetPulse();
        if (pipe == null) pipe = GetComponent<LineRenderer>();
        if (pipe == null || pipe.positionCount < 2)
        {
            Debug.LogWarning("PipeColorPulse: Röhre mit mindestens zwei Wegpunkten zuweisen.", this);
            yield break;
        }
        Vector3[] points = new Vector3[pipe.positionCount];
        pipe.GetPositions(points);
        if (!pipe.useWorldSpace)
            for (int i = 0; i < points.Length; i++) points[i] = pipe.transform.TransformPoint(points[i]);
        if ((points[points.Length - 1] - mixingPosition).sqrMagnitude < (points[0] - mixingPosition).sqrMagnitude)
            System.Array.Reverse(points);
        float[] distances = new float[points.Length];
        for (int i = 1; i < points.Length; i++)
            distances[i] = distances[i - 1] + Vector3.Distance(points[i - 1], points[i]);
        float total = distances[distances.Length - 1];
        if (total <= Mathf.Epsilon) yield break;
        if (!CreateVisuals()) yield break;
        color.a = 1f;
        coreMaterial.SetColor("_BaseColor", color);
        Color glow = Color.Lerp(color, Color.white, 0.3f);
        glow.a = 0.3f;
        haloMaterial.SetColor("_BaseColor", glow);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float head = (total + pulseLength) * elapsed / duration;
            float start = Mathf.Clamp(head - pulseLength, 0f, total);
            float end = Mathf.Clamp(head, 0f, total);
            visiblePoints.Clear();
            visiblePoints.Add(PointAt(points, distances, start));
            for (int i = 1; i < points.Length - 1; i++)
                if (distances[i] > start && distances[i] < end) visiblePoints.Add(points[i]);
            visiblePoints.Add(PointAt(points, distances, end));
            Vector3[] segment = visiblePoints.ToArray();
            core.positionCount = halo.positionCount = segment.Length;
            core.SetPositions(segment);
            halo.SetPositions(segment);
            core.enabled = halo.enabled = end - start > 0.001f;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        ResetPulse();
    }

    private static Vector3 PointAt(Vector3[] points, float[] distances, float distance)
    {
        for (int i = 1; i < points.Length; i++)
            if (distance <= distances[i])
                return Vector3.Lerp(points[i - 1], points[i], Mathf.InverseLerp(distances[i - 1], distances[i], distance));
        return points[points.Length - 1];
    }

    private bool CreateVisuals()
    {
        if (core != null) return true;
        Shader shader = pulseMaterial != null ? pulseMaterial.shader : Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            Debug.LogWarning("PipeColorPulse: URP/Unlit Shader fehlt.", this);
            return false;
        }
        coreMaterial = pulseMaterial != null ? new Material(pulseMaterial) : new Material(shader);
        coreMaterial.name = "Pipe Pulse Color (Runtime)";
        coreMaterial.renderQueue = (int)RenderQueue.Transparent + 10;
        coreMaterial.SetFloat("_ZWrite", 0f);
        haloMaterial = pulseMaterial != null ? new Material(pulseMaterial) : new Material(shader);
        haloMaterial.name = "Pipe Pulse Halo (Runtime)";
        haloMaterial.SetFloat("_Surface", 1f);
        haloMaterial.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        haloMaterial.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        haloMaterial.SetFloat("_ZWrite", 0f);
        haloMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        haloMaterial.renderQueue = (int)RenderQueue.Transparent + 11;
        core = CreateLine("ColorPulse", coreMaterial, width);
        halo = CreateLine("ColorPulseGlow", haloMaterial, width * 1.8f);
        return true;
    }

    private LineRenderer CreateLine(string name, Material material, float lineWidth)
    {
        GameObject obj = new GameObject(name);
        obj.layer = gameObject.layer;
        obj.transform.SetParent(transform, false);
        LineRenderer line = obj.AddComponent<LineRenderer>();
        line.sharedMaterial = material;
        line.useWorldSpace = true;
        line.widthMultiplier = lineWidth;
        line.numCornerVertices = 8;
        line.numCapVertices = 8;
        line.shadowCastingMode = ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.enabled = false;
        return line;
    }

    public void ResetPulse()
    {
        if (core != null) core.enabled = false;
        if (halo != null) halo.enabled = false;
    }

    private void OnDisable() => ResetPulse();

    private void OnDestroy()
    {
        if (coreMaterial != null) Destroy(coreMaterial);
        if (haloMaterial != null) Destroy(haloMaterial);
    }
}
