using TMPro;
using UnityEngine;

public sealed class PgLearningWords : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private TMP_Text heading;
    [SerializeField, Min(1f)] private float readingTime = 4f;
    [SerializeField, Min(0.1f)] private float fadeDuration = 0.55f;
    [SerializeField] private string[] topics = {
        "Programmieren mit C#", "3D-Modellierung mit Blender", "Spiele entwickeln mit Unity",
        "Licht und Farben verstehen", "Animationen gestalten", "Sounds zum Leben erwecken",
        "Interaktion mit Sensoren", "Versionierung mit Git", "Im Team entwickeln",
        "Ideen testen und verbessern", "Idee → Modell → Code\nInteraktion → Erlebnis"
    };
    private HoloFishSwimmer fish;
    private float elapsed;
    private int shownIndex = -1;

    private void OnEnable()
    {
        elapsed = 0f;
        shownIndex = -1;
        SetAlpha(0f);
    }

    private void Update()
    {
        if (label == null || topics == null || topics.Length == 0) return;
        if (fish == null) fish = FindFirstObjectByType<HoloFishSwimmer>();
        if (fish != null && fish.IsDeparting)
        {
            SetAlpha(Mathf.MoveTowards(label.alpha, 0f, Time.unscaledDeltaTime / fadeDuration));
            return;
        }
        elapsed += Time.unscaledDeltaTime;
        float cycle = readingTime + fadeDuration * 2f;
        int index = Mathf.FloorToInt(elapsed / cycle) % topics.Length;
        if (index != shownIndex)
        {
            label.text = topics[index];
            shownIndex = index;
        }
        float phase = elapsed % cycle;
        float fade = Mathf.Clamp01(Mathf.Min(phase / fadeDuration, (cycle - phase) / fadeDuration));
        SetAlpha(Mathf.SmoothStep(0f, 1f, fade));
    }

    private void SetAlpha(float alpha)
    {
        if (label != null) label.alpha = alpha;
        if (heading != null) heading.alpha = alpha * 0.7f;
    }

    private void OnDisable() => SetAlpha(0f);
}
