using UnityEngine;

public static class ColorMatchUtility
{
    /// <summary>
    /// Gibt zurueck, wie gut current zur target-Farbe passt.
    /// 0 = gar nicht passend, 100 = perfekt passend.
    /// Bewertet HSV mit zusaetzlicher Helligkeitslogik.
    /// </summary>
    public static float CalculatePerceptualMatchPercent(Color current, Color target)
    {
        Color.RGBToHSV(current, out float currentHue, out float currentSaturation, out float currentValue);
        Color.RGBToHSV(target, out float targetHue, out float targetSaturation, out float targetValue);

        // Sonderfall: aktueller Mix ist fast schwarz, Ziel ist aber sichtbar.
        // Dann soll es nicht "halb richtig" wirken.
        if (currentValue < 0.05f && targetValue > 0.20f)
        {
            return 0f;
        }

        // Sonderfall: Ziel ist fast schwarz.
        // Dann ist vor allem die Helligkeit entscheidend.
        if (targetValue < 0.05f)
        {
            float blackDistance = Mathf.Clamp01(currentValue / 1f);
            return (1f - blackDistance) * 100f;
        }

        // Hue-Distanz: Hue ist ein Kreis. 0 und 1 liegen wieder nahe beieinander.
        float hueDistance = Mathf.Abs(Mathf.DeltaAngle(currentHue * 360f, targetHue * 360f)) / 180f;

        // Saettigungs- und Helligkeitsabstand
        float saturationDistance = Mathf.Abs(currentSaturation - targetSaturation);
        float valueDistance = Mathf.Abs(currentValue - targetValue);

        // Wenn eine der Farben sehr wenig Saettigung hat, ist Hue weniger aussagekraeftig.
        // Beispiel: Grau/Schwarz/Weiss haben keinen klaren Farbton.
        float hueWeight = 0.50f;
        if (currentSaturation < 0.15f || targetSaturation < 0.15f)
        {
            hueWeight = 0.10f;
        }

        float saturationWeight = 0.20f;
        float valueWeight = 0.30f;

        // Gewichte normalisieren, falls hueWeight reduziert wurde.
        float weightSum = hueWeight + saturationWeight + valueWeight;

        float combinedDistance =
            (hueDistance * hueWeight +
             saturationDistance * saturationWeight +
             valueDistance * valueWeight) / weightSum;

        float match = 1f - combinedDistance;
        return Mathf.Clamp01(match) * 100f;
    }
}
