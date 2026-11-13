using UnityEngine;

/// <summary>Ambient flight around the authored position, with independent wing beats.</summary>
public sealed class FireflyFlight : MonoBehaviour
{
    [SerializeField] private Transform wings;
    [SerializeField] private Vector3 flightRadius = new Vector3(0.65f, 0.3f, 0.45f);
    [SerializeField, Min(0f)] private float flightSpeed = 0.7f;
    [SerializeField, Min(0f)] private float wingBeatsPerSecond = 9f;
    [SerializeField, Range(0.1f, 1f)] private float foldedWingScale = 0.5f;
    [SerializeField] private float phase;
    private Vector3 homePosition, wingScale;
    private Quaternion homeRotation, wingRotation;
    private int spanAxis;
    private float elapsed;

    private void Awake()
    {
        homePosition = transform.localPosition;
        homeRotation = transform.localRotation;
        if (wings == null)
            foreach (Transform child in GetComponentsInChildren<Transform>(true))
                if (child.name == "Wings") { wings = child; break; }
        if (wings == null) return;
        wingScale = wings.localScale;
        wingRotation = wings.localRotation;
        MeshFilter filter = wings.GetComponent<MeshFilter>();
        Vector3 size = filter != null && filter.sharedMesh != null ? filter.sharedMesh.bounds.size : Vector3.right;
        spanAxis = size.y > size.x ? 1 : 0;
        if (size.z > size[spanAxis]) spanAxis = 2;
    }

    private void OnEnable() => elapsed = 0f;

    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = elapsed * flightSpeed;
        float fade = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed));
        Vector3 drift = new Vector3(
            Mathf.Sin(t * 0.8f + phase),
            Mathf.Sin(t * 1.7f + phase * 1.3f),
            Mathf.Sin(t * 0.57f + phase * 1.8f));
        transform.localPosition = homePosition + Vector3.Scale(drift, flightRadius) * fade;
        transform.localRotation = homeRotation * Quaternion.Euler(
            Mathf.Sin(t * 1.3f + phase) * 5f * fade,
            Mathf.Sin(t * 0.6f + phase) * 25f * fade,
            Mathf.Sin(t * 1.1f + phase) * 7f * fade);
        if (wings == null) return;
        float beat = elapsed * wingBeatsPerSecond * Mathf.PI * 2f + phase;
        // A single mesh contains both wings. Compress its span symmetrically to fold them.
        Vector3 scale = wingScale;
        scale[spanAxis] *= Mathf.Lerp(foldedWingScale, 1f, (Mathf.Sin(beat) + 1f) * 0.5f);
        wings.localScale = scale;
        wings.localRotation = wingRotation * Quaternion.AngleAxis(Mathf.Sin(beat) * 7f, Vector3.forward);
    }

    private void OnDisable()
    {
        transform.localPosition = homePosition;
        transform.localRotation = homeRotation;
        if (wings == null) return;
        wings.localScale = wingScale;
        wings.localRotation = wingRotation;
    }
}
