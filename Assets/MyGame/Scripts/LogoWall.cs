using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogoWall : MonoBehaviour
{
    public GameObject cubePrefab; // Cube-Prefab, das im Inspector zugewiesen wird
    public int width = 4; // Anzahl der Cubes in der Breite (x-Achse)
    public int height = 5; // Anzahl der Cubes in der Höhe (y-Achse)
    public float spacing = 1.1f; // Abstand zwischen den Cubes
    public Color emissionColor = new Color(1.0f, 0.5f, 0.2f); // Basisfarbe für die Emission
    public float emissionIntensity = 5.0f; // HDR-Intensität für die Emission
    public ItemData peppersGhostData;
    private Department[] depts;
    public GameObject[,] cubeGrid; // 2Dim-Array zur Speicherung der Cubes

    private void Start()
    {
        cubeGrid = new GameObject[width, height];
        peppersGhostData = Resources.Load<ItemData>("PeppersGhostData");
        depts = peppersGhostData.departments;
        BuildWall();
    }

    private void BuildWall()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Berechne die Position des Cubes
                Vector3 position = new Vector3(x * spacing, y * spacing, 0);

                // Cube an der berechneten Position instanziieren
                GameObject cube = Instantiate(cubePrefab, position, Quaternion.identity, transform);
                cube.name = "cube" + x + y;
                cubeGrid[x, y] = cube;

                // Emissionsfarbe einstellen
                Renderer cubeRenderer = cube.GetComponent<Renderer>();
                if (cubeRenderer != null)
                {
                    // Setze die HDR-Emission-Farbe und aktiviere den Emissionskanal
                    Color hdrEmissionColor = emissionColor * emissionIntensity;
                    cubeRenderer.material.SetColor("_EmissionColor", hdrEmissionColor);
                    cubeRenderer.material.EnableKeyword("_EMISSION");
                }

            }
            
        }

        foreach (Department elem in depts)
        {
            Color c = elem.rgbColor;
            
            cubeGrid[elem.x, elem.y].GetComponent<Renderer>().material.SetColor("_BaseColor", Color.black);
            cubeGrid[elem.x, elem.y].GetComponent<Renderer>().material.SetColor("_EmissionColor", c*0.7f);
            cubeGrid[elem.x, elem.y].GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
        }
    }
}