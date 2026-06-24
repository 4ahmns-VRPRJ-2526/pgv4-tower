using UnityEngine;
using UnityEngine.SceneManagement;

public class WindmillManager : MonoBehaviour
{
    public int resolutionWidth, resolutionHeight;

    [SerializeField] private Windmill[] windmills;
    [SerializeField] private GameObject _wallGoal;
    [SerializeField] private GameObject landscapeObjs;
    [SerializeField] private GameObject verticalStartCanvas;
    [SerializeField] private GameObject horizontalStartCanvas;
    [SerializeField] private GhostController ghostController;

    private GameManagerScript _cgsa;
    public Color32 windmillColor = new Color32(0, 0, 0, 255);

    private Windmill currentSelectedWindmill;
    private bool allWindmillsLocked = false;

    public Renderer glowingResultCube;

    [Header("ParticleSystems")]
    public ParticleSystem particlesTop;
    public GameObject[] objectsToColor;

    private void Start()
    {
        resolutionWidth = Screen.width;
        resolutionHeight = Screen.height;

        if (resolutionWidth > 1080)
        {
            horizontalStartCanvas.SetActive(true);
        }
        else
        {
            verticalStartCanvas.SetActive(true);
        }

        particlesTop.enableEmission = false;
        _cgsa = GameObject.FindObjectOfType<GameManagerScript>();

        if (windmills.Length == 0)
        {
            Debug.LogError("WindmillManager: Keine Windmuehlen oder Farbwand zugewiesen!");
            return;
        }

        currentSelectedWindmill = windmills[0];
        currentSelectedWindmill.SelectWindmill();
    }

    private void Update()
    {
        UpdateWallColor();
        CheckIfAllLocked();
        UpdateColors();
    }

    private void UpdateColors()
    {
        foreach (GameObject obj in objectsToColor)
        {
            if (obj == null) continue;

            Renderer rend = obj.GetComponent<Renderer>();
            if (rend != null && rend.material.HasProperty("_Color"))
            {
                rend.material.color = windmillColor;
            }
        }

        if (glowingResultCube != null)
        {
            glowingResultCube.material.color = windmillColor;

            Color emissionColor = windmillColor;
            glowingResultCube.material.SetColor("_EmissionColor", emissionColor);
            glowingResultCube.material.EnableKeyword("_EMISSION");
        }
    }

    public void ResetScene()
    {
        windmillColor = new Color32(0, 0, 0, 255);

        foreach (var windmill in windmills)
        {
            windmill.ResetWindmill();
        }

        currentSelectedWindmill = windmills[0];
        currentSelectedWindmill.SelectWindmill();
    }

    private void UpdateWallColor()
    {
        CombineLightSpeed();
        particlesTop.startColor = windmillColor;
    }

    private void CombineLightSpeed()
    {
        if (windmills.Length > 0)
            windmillColor.r = (byte)windmills[0].GetCurrentSpeed();
        if (windmills.Length > 1)
            windmillColor.g = (byte)windmills[1].GetCurrentSpeed();
        if (windmills.Length > 2)
            windmillColor.b = (byte)windmills[2].GetCurrentSpeed();
    }

    public void LockAllExcept(Windmill clickedWindmill)
    {
        if (clickedWindmill == currentSelectedWindmill)
        {
            clickedWindmill.ToggleRotationMode();
            clickedWindmill.isWindmillSelected = false;
            currentSelectedWindmill = null;
        }
        else
        {
            foreach (var windmill in windmills)
            {
                if (windmill == clickedWindmill)
                {
                    windmill.isWindmillSelected = true;
                    windmill.rotor.constRotationSpeed = -1f;
                    windmill.SelectWindmill();
                    currentSelectedWindmill = windmill;

                    int index = System.Array.IndexOf(windmills, windmill);
                    if (ghostController != null)
                    {
                        ghostController.FlyToWindmill(index);
                    }
                }
                else
                {
                    windmill.isWindmillSelected = false;
                    windmill.rotor.constRotationSpeed = windmill.rotor.currentSpeed;
                }
            }
        }
    }

    public void MoveGhostToCurrentWindmill()
    {
        if (ghostController == null || currentSelectedWindmill == null)
        {
            return;
        }

        int index = System.Array.IndexOf(windmills, currentSelectedWindmill);
        if (index >= 0)
        {
            ghostController.FlyToWindmill(index);
        }
    }

    private void CheckIfAllLocked()
    {
        if (allWindmillsLocked)
            return;

        bool allLocked = true;
        foreach (var windmill in windmills)
        {
            if (!windmill.IsWindmillLocked())
            {
                allLocked = false;
                break;
            }
        }

        if (allLocked)
        {
            allWindmillsLocked = true;
        }
    }

    public void LoadEndScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
