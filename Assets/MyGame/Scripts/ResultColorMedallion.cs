using UnityEngine;
using UnityEngine.UI;

/// <summary>Editable gold polygon rim around an accurate, solid result color.</summary>
public sealed class ResultColorMedallion : Image
{
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Rect rect = GetPixelAdjustedRect();
        Vector2 center = rect.center;
        float radius = Mathf.Min(rect.width, rect.height) * 0.5f;
        const int segments = 24;
        for (int i = 0; i < segments; i++)
        {
            float a = i * Mathf.PI * 2f / segments;
            float b = (i + 1) * Mathf.PI * 2f / segments;
            Vector2 outerA = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
            Vector2 outerB = center + new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * radius;
            Vector2 innerA = center + (outerA - center) * 0.86f;
            Vector2 innerB = center + (outerB - center) * 0.86f;
            Color gold = i % 3 == 0 ? new Color32(255, 225, 108, 255) :
                i % 3 == 1 ? new Color32(255, 192, 35, 255) : new Color32(204, 133, 18, 255);
            int start = mesh.currentVertCount;
            mesh.AddVert(outerA, gold, Vector2.zero);
            mesh.AddVert(outerB, gold, Vector2.zero);
            mesh.AddVert(innerB, gold, Vector2.zero);
            mesh.AddVert(innerA, gold, Vector2.zero);
            mesh.AddTriangle(start, start + 1, start + 2);
            mesh.AddTriangle(start, start + 2, start + 3);
            start = mesh.currentVertCount;
            mesh.AddVert(center, color, Vector2.zero);
            mesh.AddVert(innerA, color, Vector2.zero);
            mesh.AddVert(innerB, color, Vector2.zero);
            mesh.AddTriangle(start, start + 1, start + 2);
        }
    }
}
