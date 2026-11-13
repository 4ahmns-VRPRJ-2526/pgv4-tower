using TMPro;
using UnityEngine;

public sealed class PgAdventureName : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private PgBlowHint blowHint;
    [SerializeField, Min(1f)] private float introductionDuration = 5f;
    [SerializeField, Min(1f)] private float completionDuration = 8f;
    private string adventureName;
    private float shownAt, hideAt;
    private bool showing;

    private void Awake() => Hide();

    public void Introduce(string name)
    {
        adventureName = string.IsNullOrWhiteSpace(name) ? "" : name.Trim();
        if (adventureName.Length == 0) return;
        adventureName = char.ToUpper(adventureName[0]) + adventureName.Substring(1);
        Show("Du bist:\n" + adventureName, introductionDuration);
    }

    public void Celebrate()
    {
        if (string.IsNullOrEmpty(adventureName)) return;
        Show(adventureName + ",\ndeine Farbe macht die Welt bunt!", completionDuration);
    }

    private void Show(string message, float duration)
    {
        if (label == null) return;
        label.text = message;
        shownAt = Time.unscaledTime;
        hideAt = shownAt + duration;
        showing = true;
    }

    private void Update()
    {
        if (label == null) return;
        if (!showing || Time.unscaledTime >= hideAt)
        {
            Hide();
            return;
        }
        if (blowHint != null && blowHint.IsShowing)
        {
            label.alpha = 0f;
            return;
        }
        float fade = Mathf.Min((Time.unscaledTime - shownAt) / 0.35f, (hideAt - Time.unscaledTime) / 0.5f);
        label.alpha = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(fade));
    }

    public void Hide()
    {
        showing = false;
        if (label != null) label.alpha = 0f;
    }

    private void OnDisable() => Hide();
}
