using UnityEngine;
using UnityEngine.UI;

/// <summary>A cut-corner, faceted sign matching the game's low-poly artwork.</summary>
public sealed class LowPolyDialogImage : Image
{
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Rect r = GetPixelAdjustedRect();
        float cut = Mathf.Min(r.width, r.height) * 0.18f;
        Vector2[] edge = {
            new Vector2(r.xMin + cut, r.yMin), new Vector2(r.xMin, r.yMin + cut),
            new Vector2(r.xMin, r.yMax - cut), new Vector2(r.xMin + cut, r.yMax),
            new Vector2(r.xMax - cut, r.yMax), new Vector2(r.xMax, r.yMax - cut),
            new Vector2(r.xMax, r.yMin + cut), new Vector2(r.xMax - cut, r.yMin)
        };
        float[] light = { 0.72f, 0.86f, 1.14f, 1.22f, 1.08f, 0.93f, 0.78f, 0.69f };
        Vector2 center = r.center + new Vector2(r.width * 0.09f, r.height * 0.07f);
        for (int i = 0; i < edge.Length; i++)
        {
            Color shade = color * light[i];
            shade.a = color.a;
            int start = mesh.currentVertCount;
            mesh.AddVert(center, color, Vector2.zero);
            mesh.AddVert(edge[i], shade, Vector2.zero);
            mesh.AddVert(edge[(i + 1) % edge.Length], shade, Vector2.zero);
            mesh.AddTriangle(start, start + 1, start + 2);
        }
    }
}
