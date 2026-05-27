using UnityEngine;
using System.Collections.Generic;

public class FireflyColor : MonoBehaviour
{
    [Header("Renderer-Teile des Glühwürmchens")]
    [SerializeField] private Renderer[] fireflyRenderers; 

    [Header("Einstellungen")]
    [SerializeField] private float emissionIntensity = 2f;

    private WindmillManager windmillManager;
    private List<Material> targetMaterials = new List<Material>(); 
    private Color lastColor;

    void Start()
    {
        windmillManager = GameObject.FindObjectOfType<WindmillManager>();

        if (fireflyRenderers == null || fireflyRenderers.Length == 0)
        {
            Renderer localRenderer = GetComponent<Renderer>();
            if (localRenderer != null)
            {
                fireflyRenderers = new Renderer[] { localRenderer };
            }
        }

        if (fireflyRenderers != null)
        {
            foreach (Renderer rend in fireflyRenderers)
            {

                foreach (Material mat in rend.materials)
                {
                    if (mat.name.Contains("LightFireFly"))
                    {
                        targetMaterials.Add(mat); 
                    }
                }
            }
        }

    }

    void Update()
    {

        Color currentWindmillColor = windmillManager.windmillColor;

        if (currentWindmillColor != lastColor)
        {
            Color finalEmission = currentWindmillColor * emissionIntensity;

            foreach (Material mat in targetMaterials)
            {
                if (mat == null) continue;

                mat.color = currentWindmillColor;
                mat.SetColor("_EmissionColor", finalEmission);
            }

            lastColor = currentWindmillColor;
        }
    }
}