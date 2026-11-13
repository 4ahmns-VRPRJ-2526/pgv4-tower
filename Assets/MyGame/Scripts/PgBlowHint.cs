using UnityEngine;
using UnityEngine.UI;

/// <summary>Upper PG-stage blowing invitation; follows the rotor's actual blow input.</summary>
public sealed class PgBlowHint : MonoBehaviour
{
    [SerializeField] private GameManagerScript gameManager;
    [SerializeField] private Image childImage;
    [SerializeField] private Sprite airDot;
    [SerializeField, Min(0f)] private float idleDelay = 5f;
    [SerializeField, Min(0f)] private float repeatDelay = 4f;
    [SerializeField, Min(1f)] private float hintDuration = 4.2f;
    private readonly Image[] dots = new Image[3];
    private Vector3 originalScale;
    private Vector2 originalPosition;
    private Color imageColor;
    private float nextHint, startedAt = -1f, lastBlow;
    private bool mixing;
    public bool IsShowing => mixing && startedAt >= 0f;

    private void Awake()
    {
        if (childImage == null) return;
        originalScale = childImage.transform.localScale;
        originalPosition = childImage.rectTransform.anchoredPosition;
        imageColor = childImage.color;
        childImage.raycastTarget = false;
        for (int i = 0; i < dots.Length; i++)
        {
            GameObject obj = new GameObject("Luftpunkt " + (i + 1), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            obj.transform.SetParent(childImage.transform, false);
            dots[i] = obj.GetComponent<Image>();
            dots[i].sprite = airDot;
            dots[i].raycastTarget = false;
            dots[i].rectTransform.sizeDelta = Vector2.one * 22f;
        }
        Hide();
    }

    private void Update()
    {
        if (childImage == null) return;
        bool active = gameManager != null && gameManager.WindmillsActive;
        if (!active)
        {
            mixing = false;
            Hide();
            return;
        }
        if (!mixing)
        {
            mixing = true;
            lastBlow = RotorHub.LastBlowTime;
            nextHint = Time.unscaledTime + idleDelay;
        }
        if (Input.GetKey(KeyCode.Space) || RotorHub.LastBlowTime > lastBlow)
        {
            lastBlow = RotorHub.LastBlowTime;
            Hide();
            nextHint = Time.unscaledTime + idleDelay;
            return;
        }
        if (Time.unscaledTime < nextHint) return;
        if (startedAt < 0f) startedAt = Time.unscaledTime;
        float time = Time.unscaledTime - startedAt;
        if (time >= hintDuration)
        {
            Hide();
            nextHint = Time.unscaledTime + repeatDelay;
            return;
        }
        float fadeIn = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(time / 0.55f));
        float fadeOut = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((hintDuration - time) / 0.55f));
        float alpha = fadeIn * fadeOut;
        float first = Mathf.Clamp01((time - 0.8f) / 0.9f);
        float second = Mathf.Clamp01((time - 2.3f) / 0.9f);
        float breath = Mathf.Sin(first * Mathf.PI) + Mathf.Sin(second * Mathf.PI);
        childImage.color = new Color(imageColor.r, imageColor.g, imageColor.b, imageColor.a * alpha);
        childImage.transform.localScale = originalScale * (1f + breath * 0.035f);
        childImage.rectTransform.anchoredPosition = originalPosition + Vector2.right * breath * 6f;
        float puff = time < 2.1f ? first : second;
        for (int i = 0; i < dots.Length; i++)
        {
            float progress = Mathf.Clamp01((puff - i * 0.12f) / 0.76f);
            RectTransform rect = dots[i].rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.94f + progress * 0.14f, 0.1f + progress * 0.07f);
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one * Mathf.Lerp(0.5f, 1.2f, progress);
            dots[i].color = new Color(0.65f, 1f, 1f, Mathf.Sin(progress * Mathf.PI) * alpha * 0.7f);
        }
    }

    private void Hide()
    {
        startedAt = -1f;
        if (childImage == null) return;
        childImage.color = new Color(imageColor.r, imageColor.g, imageColor.b, 0f);
        childImage.rectTransform.anchoredPosition = originalPosition;
        childImage.transform.localScale = originalScale;
        foreach (Image dot in dots) if (dot != null) dot.color = Color.clear;
    }

    private void OnDisable()
    {
        mixing = false;
        Hide();
    }
}
