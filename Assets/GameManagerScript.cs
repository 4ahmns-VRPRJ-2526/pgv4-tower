using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.ParticleSystemJobs;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManagerScript : MonoBehaviour
{
    [Header("Windmills")]
    public GameObject[] objectsWithScripts;

    [Header("Ui")]
    public Selectable[] uiElementsToDisable;

    [Header("Colour")]
    public GameObject colourCanvas;
    public Color _goalColour;
    public Color[] _colorsArray;

    [Header("RandomName")]
    public GameObject randomNameCanvas;
    public string randomName;
    public TMP_Text randomNameText;

    // NEU: Text oben im Color Selection Menu
    public TMP_Text colorMenuNameText;

    public Animator ghostAnimator;
    public WindmillManager wma;
    public GameObject textCanvas;
    AnimatorStateInfo stateInfo;
    private bool hasShownText = false;

    [SerializeField] GameObject goalSphere;
    [SerializeField] GameObject achievedSphere;

    public GameObject goalSphereParent;
    public GameObject achievedSphereParent;

    [SerializeField] TMP_Text procentageText;

    public GameObject finishedGameCanvas;

    bool alreadyPulled = false;

    // Liste der männlichen Adjektive
    private string[] adjektive = new string[]
    {
        "verrückter", "lustiger", "komischer", "schräger", "alberner",
        "zappeliger", "flippiger", "witziger", "seltsamer", "spaßiger"
    };

    // Liste der Tiere
    private string[] tiere = new string[]
    {
        "Löwe", "Tiger", "Bär", "Wolf", "Fuchs",
        "Hirsch", "Eber", "Rabe", "Panther", "Adler"
    };

    void Start()
    {
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
        }

        wma = GameObject.FindObjectOfType<WindmillManager>();

        var emission = wma.particlesTop.emission;
        emission.enabled = false;

        foreach (GameObject obj in objectsWithScripts)
        {
            MonoBehaviour[] scripts = obj.GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour script in scripts)
            {
                if (script == this) continue;

                script.enabled = false;
            }
        }

        foreach (Selectable ui in uiElementsToDisable)
        {
            ui.interactable = false;
        }

        // NEU: Name am Anfang unsichtbar machen
        if (colorMenuNameText != null)
        {
            colorMenuNameText.text = "";
        }
    }

    public void ActivateRandomName()
    {
        randomNameCanvas.SetActive(true);

        GameObject callerx = EventSystem.current.currentSelectedGameObject;

        if (callerx != null)
        {
            callerx.SetActive(false);
        }
    }

    public void ChooseRandomName()
    {
        randomName =
            adjektive[Random.Range(0, adjektive.Length)] + " " +
            tiere[Random.Range(0, tiere.Length)];

        // Random Name Canvas Text setzen
        randomNameText.text = randomName;

        // NEU: Gleichen Namen im Color Menu anzeigen
        colorMenuNameText.text = randomName;

        StartCoroutine(WaitTime());
    }

    public void ActivateCoulorCanvas()
    {
        colourCanvas.SetActive(true);
    }

    public void SelectColorGoal(int a)
    {
        ghostAnimator.SetTrigger("TrigGhost");

        wma.particlesTop.enableEmission = true;

        foreach (GameObject obj in objectsWithScripts)
        {
            MonoBehaviour[] scripts = obj.GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour script in scripts)
            {
                if (script == this) continue;

                script.enabled = true;
            }
        }

        foreach (Selectable ui in uiElementsToDisable)
        {
            ui.interactable = true;
        }

        colourCanvas.SetActive(false);

        // NEU: Namen verstecken wenn Canvas geschlossen wird
        colorMenuNameText.text = "";

        _goalColour = _colorsArray[a];
    }

    void Update()
    {
        if (ghostAnimator != null)
        {
            stateInfo = ghostAnimator.GetCurrentAnimatorStateInfo(0);

            if (!hasShownText &&
                stateInfo.IsName("Anim2") &&
                stateInfo.normalizedTime >= 1.0f)
            {
                textCanvas.SetActive(true);

                StartCoroutine(TextWait());

                hasShownText = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) && !alreadyPulled)
        {
            finishedGameCanvas.SetActive(true);

            achievedSphereParent.SetActive(true);
            goalSphereParent.SetActive(true);

            alreadyPulled = true;

            float similarity =
                GetColorSimilarityPercentage(
                    _goalColour,
                    wma.windmillColor
                );

            goalSphere.GetComponent<Renderer>().material.color = _goalColour;

            achievedSphere.GetComponent<Renderer>().material.color =
                wma.windmillColor;

            procentageText.text = similarity + "%";
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) && alreadyPulled)
        {
            SceneManager.LoadScene(
                SceneManager.GetActiveScene().buildIndex
            );
        }
    }

    public IEnumerator WaitTime()
    {
        yield return new WaitForSeconds(2);

        randomNameCanvas.SetActive(false);

        ActivateCoulorCanvas();
    }

    public IEnumerator TextWait()
    {
        yield return new WaitForSeconds(15);

        textCanvas.SetActive(false);
    }

    float GetColorSimilarityPercentage(Color a, Color b)
    {
        float rDiff = a.r - b.r;
        float gDiff = a.g - b.g;
        float bDiff = a.b - b.b;

        float distance =
            Mathf.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);

        float knappheit =
            1f - (distance / Mathf.Sqrt(3f));

        return Mathf.Clamp(
            (float)System.Math.Round(knappheit * 100f, 2),
            0f,
            100f
        );
    }
}