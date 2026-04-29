using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WindmillManager : MonoBehaviour
{
    [SerializeField] Windmill[] windmills;
    [SerializeField] GameObject _wallGoal;


    private GameManagerScript _cgsa;
    public Color32 windmillColor = new Color32(0, 0, 0, 255);

    private Windmill currentSelectedWindmill;
    private bool allWindmillsLocked = false;

    [Header("ParticleSystems")]
    public ParticleSystem particlesTop;
    public GameObject[] objectsToColor;



    private void Start()
    {
        particlesTop.enableEmission = false;
        _cgsa = GameObject.FindObjectOfType<GameManagerScript>();


        if (windmills.Length == 0)
        {
            Debug.LogError("WindmillManager: Keine Windmühlen oder Farbwand zugewiesen!");
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

    void UpdateColors()
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
            //Sperret die Mill wenn nochmal geclicked
            clickedWindmill.ToggleRotationMode();
            clickedWindmill.isWindmillSelected = false;
            currentSelectedWindmill = null;
        }
        else
        {
            //Sperrt alle außer this
            foreach (var windmill in windmills)
            {
                if (windmill == clickedWindmill)
                {
                    windmill.isWindmillSelected = true;
                    windmill.rotor.constRotationSpeed = -1f;
                    windmill.SelectWindmill();
                    currentSelectedWindmill = windmill;
                }
                else
                {
                    windmill.isWindmillSelected = false;
                    windmill.rotor.constRotationSpeed = windmill.rotor.currentSpeed;
                }
            }
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
