using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Fino receives the mixed light on the PG stage and celebrates its release.</summary>
public sealed class PepperGhostFinale : MonoBehaviour
{
    [SerializeField] private Animator fino;
    [SerializeField] private Camera presentationCamera;
    [SerializeField] private Transform arms;
    [SerializeField] private Transform head;
    [SerializeField] private ParticleSystem celebrationParticles;
    [SerializeField] private Material lightMaterial;
    [SerializeField, Min(1f)] private float stageDepth = 12f;
    [SerializeField, Min(0.1f)] private float receiveDuration = 2f;
    [SerializeField, Min(0.1f)] private float celebrationDuration = 2f;
    [SerializeField, Min(0.1f)] private float departureDuration = 1.8f;
    [SerializeField, Range(0f, 2f)] private float movementStrength = 1f;
    [SerializeField, Range(0f, 0.3f)] private float idleFloatHeight = 0.12f;
    private Vector3 savedPosition, savedScale, stagePosition;
    private Quaternion savedRotation, armRotation, headRotation;
    private Quaternion stageRotation;
    private bool savedAnimatorEnabled, posed;
    private bool departing;
    private float gesture, nod, bounce;
    private float lean, turn, sway, stageTime;
    private GameObject orb;
    private LineRenderer ring;
    private Material orbMaterial, ringMaterial;

    public IEnumerator ReceiveColor(Color color)
    {
        if (fino == null || presentationCamera == null || lightMaterial == null) yield break;
        PrepareStage();
        CreateEffects(color);
        orb.SetActive(true);
        Vector3 ballPosition = StagePoint(0.67f, 0.43f);
        float elapsed = 0f;
        while (elapsed < receiveDuration)
        {
            float p = Mathf.Clamp01(elapsed / receiveDuration);
            float appear = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, 0.45f, p));
            float release = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.7f, 1f, p));
            float anticipation = Mathf.Sin(p * Mathf.PI);
            gesture = anticipation * 38f + Mathf.Sin(p * Mathf.PI * 2f) * 12f;
            nod = anticipation * 16f;
            lean = -anticipation * 10f;
            turn = anticipation * 22f;
            sway = Mathf.Sin(p * Mathf.PI * 2f) * 0.25f;
            bounce = anticipation * 0.18f;
            orb.transform.position = Vector3.Lerp(ballPosition, StagePoint(0.88f, 0.7f), release)
                + presentationCamera.transform.up * (Mathf.Sin(elapsed * 3f) * 0.08f);
            orb.transform.localScale = Vector3.one * (0.75f * appear * (1f - release));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        orb.SetActive(false);
        ClearGesture();
    }

    public IEnumerator Celebrate(Color color)
    {
        if (!posed || ring == null) yield break;
        if (celebrationParticles != null)
        {
            var burst = new ParticleSystem.EmitParams { startColor = color, velocity = Vector3.up * 1.5f };
            celebrationParticles.Emit(burst, 35);
        }
        ring.enabled = true;
        float elapsed = 0f;
        while (elapsed < celebrationDuration)
        {
            float p = Mathf.Clamp01(elapsed / celebrationDuration);
            float envelope = Mathf.Sin(p * Mathf.PI);
            gesture = Mathf.Sin(p * Mathf.PI * 4f) * envelope * 42f;
            nod = Mathf.Sin(p * Mathf.PI * 4f) * envelope * 14f;
            bounce = Mathf.Abs(Mathf.Sin(p * Mathf.PI * 3f)) * envelope * 0.65f;
            lean = Mathf.Sin(p * Mathf.PI * 6f) * envelope * 12f;
            turn = Mathf.Sin(p * Mathf.PI * 2f) * envelope * 25f;
            sway = Mathf.Sin(p * Mathf.PI * 4f) * envelope * 0.35f;
            float radius = Mathf.Lerp(0.35f, 3.4f, Mathf.SmoothStep(0f, 1f, p));
            Color glow = Color.Lerp(color, Color.white, 0.2f);
            glow.a = (1f - p) * 0.75f;
            ringMaterial.SetColor("_BaseColor", glow);
            Vector3 center = StagePoint(0.5f, 0.43f);
            for (int i = 0; i < ring.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2f / ring.positionCount;
                ring.SetPosition(i, center + presentationCamera.transform.right * Mathf.Cos(angle) * radius
                    + presentationCamera.transform.up * Mathf.Sin(angle) * radius);
            }
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        ring.enabled = false;
        ClearGesture();
    }

    private Vector3 StagePoint(float x, float y) => presentationCamera.ViewportToWorldPoint(new Vector3(x, y, stageDepth));

    public IEnumerator FlyAway()
    {
        if (!posed || fino == null || presentationCamera == null) yield break;
        departing = true;
        if (orb != null) orb.SetActive(false);
        if (ring != null) ring.enabled = false;
        Transform actor = fino.transform;
        Vector3 start = actor.position;
        Quaternion rotation = actor.rotation;
        Vector3 scale = actor.localScale;
        Vector3 end = presentationCamera.ViewportToWorldPoint(new Vector3(1.5f, 1.4f, stageDepth + 5f));
        float elapsed = 0f;
        while (elapsed < departureDuration)
        {
            float p = Mathf.Clamp01(elapsed / departureDuration);
            float travel = p * p;
            actor.position = Vector3.Lerp(start, end, travel) + presentationCamera.transform.up * Mathf.Sin(p * Mathf.PI) * 0.45f;
            actor.rotation = rotation * Quaternion.Euler(-12f * p, -25f * p, -18f * Mathf.Sin(p * Mathf.PI));
            actor.localScale = scale * Mathf.Lerp(1f, 0.45f, travel);
            if (arms != null) arms.localRotation = armRotation * Quaternion.Euler(Mathf.Sin(p * Mathf.PI * 8f) * 28f, 0, 10f);
            if (head != null) head.localRotation = headRotation * Quaternion.Euler(-12f * p, 0, 0);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        actor.position = end;
        actor.localScale = scale * 0.45f;
    }

    private void PrepareStage()
    {
        if (posed) return;
        Transform actor = fino.transform;
        savedPosition = actor.localPosition;
        savedRotation = actor.localRotation;
        savedScale = actor.localScale;
        savedAnimatorEnabled = fino.enabled;
        if (arms != null) armRotation = arms.localRotation;
        if (head != null) headRotation = head.localRotation;
        fino.enabled = false;
        actor.rotation = presentationCamera.transform.rotation * Quaternion.Euler(0, 180, 0);
        Renderer[] renderers = actor.GetComponentsInChildren<Renderer>(true);
        Bounds bounds = new Bounds(actor.position, Vector3.zero);
        bool found = false;
        foreach (Renderer renderer in renderers)
        {
            if (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer)) continue;
            if (!found) { bounds = renderer.bounds; found = true; } else bounds.Encapsulate(renderer.bounds);
        }
        Vector3 center = StagePoint(0.47f, 0.44f);
        float targetHeight = Vector3.Distance(StagePoint(0.5f, 0.2f), StagePoint(0.5f, 0.75f));
        float scale = found && bounds.size.y > 0.01f ? Mathf.Clamp(targetHeight / bounds.size.y, 0.2f, 4f) : 1f;
        Vector3 centerOffset = (bounds.center - actor.position) * scale;
        actor.localScale = savedScale * scale;
        actor.position = center - centerOffset;
        stagePosition = actor.position;
        stageRotation = actor.rotation;
        stageTime = 0f;
        posed = true;
    }

    private void LateUpdate()
    {
        if (!posed || fino == null || departing) return;
        stageTime += Time.unscaledDeltaTime;
        float floating = Mathf.Sin(stageTime * 1.7f) * idleFloatHeight;
        float idleSway = Mathf.Sin(stageTime * 1.1f) * 0.06f;
        fino.transform.position = stagePosition +
            presentationCamera.transform.up * ((bounce + floating) * movementStrength) +
            presentationCamera.transform.right * ((sway + idleSway) * movementStrength);
        fino.transform.rotation = stageRotation * Quaternion.Euler(
            lean * movementStrength, turn * movementStrength,
            (Mathf.Sin(stageTime * 1.3f) * 2f - lean * 0.35f) * movementStrength);
        if (arms != null) arms.localRotation = armRotation * Quaternion.Euler(gesture * movementStrength, 0, gesture * 0.35f * movementStrength);
        if (head != null) head.localRotation = headRotation * Quaternion.Euler(nod * movementStrength, -nod * 0.5f * movementStrength, 0);
    }

    private void CreateEffects(Color color)
    {
        if (orb == null)
        {
            orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "Fino-Farbkugel";
            orb.transform.SetParent(transform, false);
            Destroy(orb.GetComponent<Collider>());
            orbMaterial = new Material(lightMaterial) { name = "PG Mixed Light (Runtime)" };
            Renderer renderer = orb.GetComponent<Renderer>();
            renderer.sharedMaterial = orbMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            GameObject ringObject = new GameObject("Fino-Jubelring");
            ringObject.transform.SetParent(transform, false);
            ring = ringObject.AddComponent<LineRenderer>();
            ringMaterial = new Material(lightMaterial) { name = "PG Celebration Ring (Runtime)" };
            ringMaterial.SetFloat("_Surface", 1f);
            ringMaterial.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            ringMaterial.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            ringMaterial.SetFloat("_ZWrite", 0f);
            ringMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            ringMaterial.renderQueue = (int)RenderQueue.Transparent;
            ring.sharedMaterial = ringMaterial;
            ring.useWorldSpace = true;
            ring.loop = true;
            ring.positionCount = 64;
            ring.widthMultiplier = 0.09f;
            ring.shadowCastingMode = ShadowCastingMode.Off;
            ring.receiveShadows = false;
            ring.enabled = false;
        }
        color.a = 1f;
        orbMaterial.SetColor("_BaseColor", color * 1.25f);
    }

    public void ResetFinale()
    {
        if (orb != null) orb.SetActive(false);
        if (ring != null) ring.enabled = false;
        if (posed && fino != null)
        {
            fino.transform.localPosition = savedPosition;
            fino.transform.localRotation = savedRotation;
            fino.transform.localScale = savedScale;
            if (arms != null) arms.localRotation = armRotation;
            if (head != null) head.localRotation = headRotation;
            fino.enabled = savedAnimatorEnabled;
        }
        posed = false;
        departing = false;
        ClearGesture();
    }

    private void ClearGesture() => gesture = nod = bounce = lean = turn = sway = 0f;

    private void OnDisable() => ResetFinale();
    private void OnDestroy()
    {
        ResetFinale();
        if (orbMaterial != null) Destroy(orbMaterial);
        if (ringMaterial != null) Destroy(ringMaterial);
    }
}
