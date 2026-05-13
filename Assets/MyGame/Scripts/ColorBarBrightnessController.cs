using UnityEngine;
using UnityEngine.UI;

public class ColorBarBrightnessController : MonoBehaviour
{
    public Light lightComponent;
    public Slider sliderComponent;
    public float intensityDevider = 100;

    void Update()
    {
        AssignLightIntensity();
    }

    private void AssignLightIntensity()
    {
        lightComponent.intensity = sliderComponent.value / intensityDevider;
    }
}
