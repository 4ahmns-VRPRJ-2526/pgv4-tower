using UnityEngine;

/// <summary>Continuous swimming deformation that keeps the original fish and fins connected.</summary>
public sealed class HoloFishBodyWave : MonoBehaviour
{
    [SerializeField] private MeshFilter body;
    [SerializeField, Range(0f, 0.1f)] private float tailSway = 0.045f;
    [SerializeField, Range(0f, 0.03f)] private float finFlutter = 0.009f;
    private Mesh animatedMesh;
    private Vector3[] rest, posed;
    private float[] tailWeights, finWeights;
    private float elapsed, nextFrame;

    private void Awake()
    {
        if (body == null) body = GetComponent<MeshFilter>();
        if (body == null || body.sharedMesh == null) return;
        animatedMesh = body.mesh;
        animatedMesh.MarkDynamic();
        rest = animatedMesh.vertices;
        posed = new Vector3[rest.Length];
        tailWeights = new float[rest.Length];
        finWeights = new float[rest.Length];
        Bounds bounds = animatedMesh.bounds;
        for (int i = 0; i < rest.Length; i++)
        {
            float tail = 1f - Mathf.InverseLerp(bounds.min.x, bounds.max.x, rest[i].x);
            tailWeights[i] = tail * tail;
            finWeights[i] = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.08f, 0.3f, Mathf.Abs(rest[i].y)));
        }
        bounds.Expand(0.2f);
        animatedMesh.bounds = bounds;
    }

    private void Update()
    {
        if (animatedMesh == null) return;
        elapsed += Time.unscaledDeltaTime;
        if (elapsed < nextFrame) return;
        nextFrame = elapsed + 1f / 45f;
        for (int i = 0; i < rest.Length; i++)
        {
            Vector3 point = rest[i];
            point.z += Mathf.Sin(elapsed * 6f + point.x * 5f) * tailWeights[i] * tailSway;
            point.z += Mathf.Sin(elapsed * 5f + point.x * 4f + point.y * 3f) * finWeights[i] * finFlutter;
            posed[i] = point;
        }
        animatedMesh.vertices = posed;
    }

    private void OnDestroy()
    {
        if (animatedMesh != null) Destroy(animatedMesh);
    }
}
