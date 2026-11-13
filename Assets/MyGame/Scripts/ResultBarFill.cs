using UnityEngine;
using UnityEngine.UI;

/// <summary>Solid color fill without stretching a decorative sprite.</summary>
public sealed class ResultBarFill : Image
{
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        if (fillAmount <= 0f) return;
        Rect rect = GetPixelAdjustedRect();
        float right = Mathf.Lerp(rect.xMin, rect.xMax, fillAmount);
        mesh.AddVert(new Vector3(rect.xMin, rect.yMin), color, Vector2.zero);
        mesh.AddVert(new Vector3(rect.xMin, rect.yMax), color, Vector2.zero);
        mesh.AddVert(new Vector3(right, rect.yMax), color, Vector2.zero);
        mesh.AddVert(new Vector3(right, rect.yMin), color, Vector2.zero);
        mesh.AddTriangle(0, 1, 2);
        mesh.AddTriangle(2, 3, 0);
    }
}
