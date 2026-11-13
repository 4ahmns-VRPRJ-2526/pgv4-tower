using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Quiet upper-stage PG fish for the evolution menu only.</summary>
public sealed class HoloFishSwimmer : MonoBehaviour
{
    [SerializeField] private Camera pgCamera;
    [SerializeField] private Transform tail;
    [SerializeField] private Transform[] fins;
    [SerializeField] private Material trailMaterial;
    [SerializeField, Range(0.5f, 0.9f)] private float heightInPicture = 0.74f;
    [SerializeField, Min(0.05f)] private float swimSpeed = 0.28f;
    [SerializeField, Min(1f)] private float depth = 12f;
    private Quaternion tailPose;
    private Quaternion[] finPoses;
    private float time, sampleTimer;
    private bool departing;
    public bool IsDeparting => departing;
    private LineRenderer trail;
    private Vector3[] history = new Vector3[28];
    private int historyCount;

    private void Awake()
    {
        if (pgCamera == null)
            foreach (Camera camera in FindObjectsByType<Camera>(FindObjectsSortMode.None))
                if (camera.targetDisplay == 1) { pgCamera = camera; break; }
        if (pgCamera == null) return;
        if (Display.displays.Length > pgCamera.targetDisplay)
            Display.displays[pgCamera.targetDisplay].Activate();
        if (tail != null) tailPose = tail.localRotation;
        finPoses = new Quaternion[fins.Length];
        for (int i = 0; i < fins.Length; i++) if (fins[i] != null) finPoses[i] = fins[i].localRotation;
        if (trailMaterial != null)
        {
            GameObject obj = new GameObject("HoloFish Light Trail");
            obj.transform.SetParent(pgCamera.transform, false);
            trail = obj.AddComponent<LineRenderer>();
            trail.sharedMaterial = trailMaterial;
            trail.useWorldSpace = true;
            trail.widthCurve = AnimationCurve.Linear(0, 0.045f, 1, 0f);
            trail.numCornerVertices = 4;
            trail.shadowCastingMode = ShadowCastingMode.Off;
            trail.receiveShadows = false;
            Gradient gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.cyan, 0), new GradientColorKey(new Color(0.55f, 0.2f, 1f), 1) },
                new[] { new GradientAlphaKey(0.3f, 0), new GradientAlphaKey(0f, 1) });
            trail.colorGradient = gradient;
        }
    }

    private void Update()
    {
        if (pgCamera == null || departing) return;
        time += Time.unscaledDeltaTime;
        float t = time * swimSpeed;
        float d = depth + Mathf.Cos(t) * 0.8f;
        transform.position = pgCamera.ViewportToWorldPoint(new Vector3(0.5f + Mathf.Sin(t) * 0.2f,
            heightInPicture + Mathf.Sin(t * 1.7f) * 0.045f, d));
        float halfWidth = Vector3.Distance(pgCamera.ViewportToWorldPoint(new Vector3(.3f, .5f, depth)),
            pgCamera.ViewportToWorldPoint(new Vector3(.7f, .5f, depth)));
        float pictureHeight = Vector3.Distance(pgCamera.ViewportToWorldPoint(new Vector3(.5f, 0f, depth)),
            pgCamera.ViewportToWorldPoint(new Vector3(.5f, 1f, depth)));
        transform.localScale = Vector3.one * Mathf.Min(halfWidth, pictureHeight * 0.32f / 0.81f);
        float dx = Mathf.Cos(t) * halfWidth;
        float dz = -Mathf.Sin(t) * 0.8f;
        float yaw = Mathf.Atan2(-dz, dx) * Mathf.Rad2Deg;
        transform.rotation = pgCamera.transform.rotation * Quaternion.Euler(0, yaw, Mathf.Sin(t * 1.7f) * 5f);
        AnimateFins(time);
        UpdateTrail();
    }

    private void AnimateFins(float clock)
    {
        if (tail != null) tail.localRotation = tailPose * Quaternion.Euler(0, Mathf.Sin(clock * 6f) * 22f, 0);
        for (int i = 0; i < fins.Length; i++)
            if (fins[i] != null) fins[i].localRotation = finPoses[i] * Quaternion.Euler(
                Mathf.Sin(clock * 5f + i * 1.8f) * 12f, Mathf.Sin(clock * 4f + i) * 6f, 0);
    }

    private void UpdateTrail()
    {
        if (trail == null) return;
        sampleTimer += Time.unscaledDeltaTime;
        if (sampleTimer < 0.055f) return;
        sampleTimer = 0f;
        for (int i = history.Length - 1; i > 0; i--) history[i] = history[i - 1];
        history[0] = tail != null ? tail.position : transform.position;
        historyCount = Mathf.Min(historyCount + 1, history.Length);
        trail.positionCount = historyCount;
        for (int i = 0; i < historyCount; i++) trail.SetPosition(i, history[i]);
    }

    public IEnumerator SwimAway()
    {
        if (pgCamera == null) yield break;
        departing = true;
        Vector3 start = transform.position;
        Quaternion rotation = transform.rotation;
        Vector3 end = pgCamera.ViewportToWorldPoint(new Vector3(1.6f, heightInPicture + 0.12f, depth + 1f));
        float elapsed = 0f;
        while (elapsed < 1.3f)
        {
            float p = Mathf.Clamp01(elapsed / 1.3f);
            transform.position = Vector3.Lerp(start, end, p * p);
            transform.rotation = Quaternion.Slerp(rotation, pgCamera.transform.rotation, Mathf.Clamp01(p * 3f));
            AnimateFins(time + elapsed * 1.6f);
            UpdateTrail();
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        transform.position = end;
    }

    private void OnDestroy()
    {
        if (trail != null) Destroy(trail.gameObject);
    }
}
