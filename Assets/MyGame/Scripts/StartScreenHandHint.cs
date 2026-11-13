using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Idle invitation using the hand's authored position as its tap target.</summary>
public sealed class StartScreenHandHint : MonoBehaviour, IPointerDownHandler
{
    [SerializeField, Min(0f)] private float idleDelay = 5f;
    [SerializeField, Min(0f)] private float repeatDelay = 4f;
    [SerializeField, Min(1f)] private float travelDistance = 600f;
    [SerializeField, Min(0f)] private float tapDepth = 2f;
    [SerializeField] private Button[] alternateTargets = new Button[0];
    private int targetIndex = -1;
    private bool animationStarted;

    private const float EnterDuration = 1.8f;
    private const float HoldDuration = 0.6f;
    private const float TapDuration = 0.7f;
    private const float ExitDuration = 1.6f;
    private const float CycleDuration = EnterDuration + HoldDuration + TapDuration + ExitDuration;

    private Transform hand;
    private SpriteRenderer[] sprites;
    private Color[] spriteColors;
    private CanvasGroup visibility;
    private Canvas canvas;
    private Button button;
    private Vector3 targetPosition;
    private Vector3 targetScale;
    private bool overlayCanvas;
    private float nextAnimationTime;
    private bool wasVisible;
    private bool dismissed;

    public static void AttachTo(GameObject startCanvas)
    {
        if (startCanvas == null) return;

        foreach (Button candidate in startCanvas.GetComponentsInChildren<Button>(true))
        {
            if (candidate.name != "ContinueButton") continue;
            foreach (Transform child in candidate.GetComponentsInChildren<Transform>(true))
            {
                if (!child.name.StartsWith("Verspielte Low-Poly-Tipp-Hand", System.StringComparison.Ordinal)) continue;
                StartScreenHandHint hint = child.GetComponent<StartScreenHandHint>();
                if (hint == null) hint = child.gameObject.AddComponent<StartScreenHandHint>();
                hint.Configure(child, candidate);
                break;
            }
        }
    }

    private void Awake()
    {
        Button parentButton = GetComponentInParent<Button>(true);
        if (parentButton != null) Configure(transform, parentButton);
    }

    private void Configure(Transform handTransform, Button continueButton)
    {
        if (hand != null) return;
        hand = handTransform;
        button = continueButton;
        canvas = GetComponentInParent<Canvas>(true);
        targetPosition = hand.localPosition;
        targetScale = hand.localScale;
        overlayCanvas = canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay;
        sprites = hand.GetComponentsInChildren<SpriteRenderer>(true);
        spriteColors = new Color[sprites.Length];
        for (int i = 0; i < sprites.Length; i++) spriteColors[i] = sprites[i].color;
        visibility = hand.GetComponent<CanvasGroup>();
        if (visibility == null) visibility = hand.gameObject.AddComponent<CanvasGroup>();
        visibility.blocksRaycasts = false;
        visibility.interactable = false;
        foreach (Graphic graphic in hand.GetComponentsInChildren<Graphic>(true))
            graphic.raycastTarget = false;
        button.onClick.AddListener(Dismiss);
        RestartDelay();
    }

    private void OnEnable()
    {
        dismissed = false;
        wasVisible = false;
        RestartDelay();
    }

    private void Update()
    {
        if (hand == null) return;

        bool visible = canvas != null && canvas.isActiveAndEnabled && button.IsInteractable();
        if (!visible || dismissed)
        {
            Hide();
            wasVisible = false;
            return;
        }
        if (!wasVisible)
        {
            RestartDelay();
            wasVisible = true;
        }

        // This project enables both input backends. Held input also postpones the hint.
        if (Input.anyKey || Input.touchCount > 0)
        {
            RestartDelay();
            return;
        }

        float elapsed = Time.unscaledTime - nextAnimationTime;
        if (elapsed < 0f) return;
        if (elapsed >= CycleDuration)
        {
            Hide();
            animationStarted = false;
            nextAnimationTime = Time.unscaledTime + repeatDelay;
            return;
        }


        if (!animationStarted)
        {
            if (alternateTargets.Length > 0 && !ChooseNextTarget())
            {
                nextAnimationTime = Time.unscaledTime + repeatDelay;
                return;
            }
            animationStarted = true;
        }

        float zOffset;
        if (elapsed < EnterDuration)
            zOffset = -travelDistance * (1f - Smooth(elapsed / EnterDuration));
        else if (elapsed < EnterDuration + HoldDuration)
            zOffset = 0f;
        else if (elapsed < EnterDuration + HoldDuration + TapDuration)
        {
            float phase = (elapsed - EnterDuration - HoldDuration) / TapDuration;
            zOffset = tapDepth * Mathf.Sin(phase * Mathf.PI);
        }
        else
        {
            float phase = (elapsed - EnterDuration - HoldDuration - TapDuration) / ExitDuration;
            zOffset = -travelDistance * Smooth(phase);
        }
        if (overlayCanvas)
        {
            // Overlay UI has no visible Z perspective: approach from below with a small scale change.
            hand.localPosition = targetPosition + (zOffset > 0f
                ? Vector3.down * zOffset : new Vector3(-zOffset * 0.25f, zOffset, 0f));
            hand.localScale = targetScale * (1f + Mathf.Clamp01(-zOffset / travelDistance) * 0.25f);
        }
        else hand.localPosition = targetPosition + Vector3.forward * zOffset;
        // Fade only at the outer end of the Z travel so the text is fully clear in pauses.
        SetAlpha(1f - Smooth(Mathf.InverseLerp(0.7f, 1f, Mathf.Abs(zOffset) / travelDistance)));
    }

    private static float Smooth(float t) => Mathf.SmoothStep(0f, 1f, t);

    public void OnPointerDown(PointerEventData eventData) => RestartDelay();

    private void RestartDelay()
    {
        Hide();
        animationStarted = false;
        nextAnimationTime = Time.unscaledTime + idleDelay;
    }

    private bool ChooseNextTarget()
    {
        Button fallback = null;
        for (int step = 1; step <= alternateTargets.Length; step++)
        {
            int index = (targetIndex + step) % alternateTargets.Length;
            Button candidate = alternateTargets[index];
            if (!TargetIsVisible(candidate)) continue;
            fallback = candidate;
            if (candidate == button) continue;
            targetIndex = index;
            button.onClick.RemoveListener(Dismiss);
            button = candidate;
            button.onClick.AddListener(Dismiss);
            hand.SetParent(button.transform, false);
            hand.localPosition = targetPosition;
            hand.localScale = targetScale;
            return true;
        }
        return fallback != null && fallback == button;
    }

    private bool TargetIsVisible(Button candidate)
    {
        if (candidate == null || !candidate.isActiveAndEnabled || !candidate.IsInteractable()) return false;
        RectTransform rect = candidate.transform as RectTransform;
        if (rect == null) return false;
        Camera camera = overlayCanvas ? null : canvas.worldCamera;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
        if (!new Rect(0, 0, Screen.width, Screen.height).Contains(screenPoint)) return false;
        foreach (RectMask2D mask in candidate.GetComponentsInParent<RectMask2D>())
            if (mask.isActiveAndEnabled && !RectTransformUtility.RectangleContainsScreenPoint(mask.rectTransform, screenPoint, camera)) return false;
        foreach (Mask mask in candidate.GetComponentsInParent<Mask>())
            if (mask.isActiveAndEnabled && !RectTransformUtility.RectangleContainsScreenPoint(mask.rectTransform, screenPoint, camera)) return false;
        return true;
    }

    private void Dismiss()
    {
        dismissed = true;
        Hide();
    }

    private void Hide()
    {
        SetAlpha(0f);
        if (hand != null) hand.localPosition = targetPosition;
        if (hand != null) hand.localScale = targetScale;
    }

    private void SetAlpha(float alpha)
    {
        if (visibility != null) visibility.alpha = alpha;
        if (sprites == null) return;
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] == null) continue;
            Color color = spriteColors[i];
            color.a *= alpha;
            sprites[i].color = color;
        }
    }

    private void OnDisable()
    {
        Hide();
        wasVisible = false;
    }

    private void OnDestroy()
    {
        if (button != null) button.onClick.RemoveListener(Dismiss);
    }
}
