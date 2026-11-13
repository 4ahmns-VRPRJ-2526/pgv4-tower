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
        canvas = GetComponentInParent<Canvas>();
        targetPosition = hand.localPosition;
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
            nextAnimationTime = Time.unscaledTime + repeatDelay;
            return;
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
        hand.localPosition = targetPosition + Vector3.forward * zOffset;
        // Fade only at the outer end of the Z travel so the text is fully clear in pauses.
        SetAlpha(1f - Smooth(Mathf.InverseLerp(0.7f, 1f, Mathf.Abs(zOffset) / travelDistance)));
    }

    private static float Smooth(float t) => Mathf.SmoothStep(0f, 1f, t);

    public void OnPointerDown(PointerEventData eventData) => RestartDelay();

    private void RestartDelay()
    {
        Hide();
        nextAnimationTime = Time.unscaledTime + idleDelay;
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
