using UnityEngine;

public sealed class ResultBarMarker : MonoBehaviour
{
    [SerializeField] private RectTransform marker;

    public void SetValue(float value)
    {
        if (marker == null) return;
        Vector2 min = marker.anchorMin;
        Vector2 max = marker.anchorMax;
        min.x = max.x = Mathf.Clamp01(value);
        marker.anchorMin = min;
        marker.anchorMax = max;
    }
}
