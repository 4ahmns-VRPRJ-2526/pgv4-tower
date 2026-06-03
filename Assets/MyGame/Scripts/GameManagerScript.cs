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
    public GameObject colourCanvasLandscape;
    public Color _goalColour;
    public Color[] _colorsArray;

    [Header("RandomName")]
    public GameObject randomNameCanvas;
    public GameObject randomNameCanvasLandscape;
    public string randomName;
    public TMP_Text randomNameText;
    public TMP_Text randomNameTextLandscape;
    public TMP_Text GenerationsLeftText;
    public TMP_Text GenerationsLeftTextLandscape;
    private int numberOfNameGenerations;
    private int numberOfGenerationsLeft;
    public int numberOfNameGenerationsIsLimitedTo = 3;
    public Button generateNameButton;
    public Button chooseNameButton;
    public Button generateNameButtonLandscape;
    public Button chooseNameButtonLandscape;

    // NEU: Text oben im Color Selection Menu
    public TMP_Text colorMenuNameText;
    public TMP_Text colorMenuNameTextLandscape;

    public Animator ghostAnimator;
    public WindmillManager wma;
    public GameObject textCanvas;
    public GameObject textCanvasLandscape;
    AnimatorStateInfo stateInfo;
    private bool hasShownText = false;

    [SerializeField] GameObject goalSphere;
    [SerializeField] GameObject achievedSphere;

    public GameObject goalSphereParent;
    public GameObject achievedSphereParent;

    [SerializeField] TMP_Text procentageText;

    public GameObject finishedGameCanvas;

    bool alreadyPulled = false;
    public int resolutionWidth, resolutionHeight;
    public bool screenHorizontal;

    // Liste der männlichen Adjektive
    private string[] adjektive = new string[]
    {
        "verrückter", "lustiger", "komischer", "schräger", "alberner", "zappeliger", "flippiger", "witziger", "seltsamer", "spaßiger",
        "plumper", "lauter", "überdrehter", "durchgeknallter", "schriller", "cooler", "lässiger", "stylischer", "entspannter", "smarter",
        "trendiger", "moderner", "souveräner", "chilliger", "lockerer", "selbstsicherer", "eleganter", "flinker", "glänzender", "wilder",
        "starker", "mächtiger", "brutaler", "rasender", "donnernder", "explosiver", "brennender", "unaufhaltbarer", "tapferer", "mutiger",
        "furchtloser", "heldenhafter", "krasser", "zäher", "unbesiegbarer", "robuster", "harter", "frostiger", "sonniger", "windiger",
        "stürmischer", "erdiger", "felsiger", "nebliger", "feuriger", "wasserreicher", "dunkler", "gruseliger", "finsterer", "geisterhafter",
        "spukhafter", "unheimlicher", "totenstiller", "schattenhafter", "kluger", "schlauer", "neugieriger", "cleverer", "tüftelnder",
        "logischer", "grübelnder", "analytischer", "gelehrter", "nerdiger", "flauschiger", "zuckersüßer", "niedlicher", "funkelnder",
        "kuscheliger", "fröhlicher", "glitzernder", "zarter", "hopsiger", "kulleriger", "bunter", "schimmernder", "magischer", "verträumter",
        "freundlicher", "witzelnder", "geheimer", "mysteriöser", "unsichtbarer", "silberner", "goldener", "stahlharter", "verräterischer",
        "leuchtender", "elektrischer", "mechanischer", "biestiger", "schlammiger", "kantiger", "schneller", "leiser", "aggressiver",
        "geduldiger", "listiger", "gefährlicher", "selbstloser", "frecher", "verschrobener", "verwegener", "legendärer", "epischer",
        "chaotischer", "genialer", "verpeilter", "nasser", "trockener", "blinder", "tauber", "wandelbarer", "fliegender", "tanzender",
        "singender", "brüllender", "jagender", "zitternder", "schnarchender", "gähnender", "lachender", "weinender", "träumender"
    };

    // Liste der männlichen Tiere
    private string[] tiere = new string[]
    {
        "Löwe", "Tiger", "Bär", "Wolf", "Fuchs", "Hirsch", "Eber", "Rabe", "Panther", "Adler",
        "Falke", "Geier", "Stier", "Hund", "Kater", "Hahn", "Pfau", "Widder", "Ziegenbock", "Dachs",
        "Marder", "Schakal", "Igel", "Hase", "Maulwurf", "Biber", "Otter", "Affe", "Gorilla", "Orang-Utan",
        "Schimpanse", "Elefant", "Wal", "Delphin", "Hai", "Krake", "Fisch", "Pavian", "Yak", "Kojote",
        "Büffel", "Zebra", "Nashorn", "Elch", "Mammut", "Drache", "Greif", "Minotaurus", "Zentaur", "Werwolf",
        "Vogel", "Pinguin", "Strauß", "Kranich", "Schwan", "Spatz", "Specht", "Uhu", "Kauz", "Kondor",
        "Luchs", "Wiesel", "Frettchen", "Kaninchen", "Kamel", "Esel", "Pony", "Ochse", "Rind", "Maultier",
        "Frosch", "Kröterich", "Molch", "Leguan", "Iltis", "Käfer", "Skorpion", "Marienkäfer", "Schmetterling",
        "Käuzchen", "Rüsselkäfer"
    };

    void Start()
    {
        numberOfNameGenerations = 0;
        numberOfGenerationsLeft = numberOfNameGenerationsIsLimitedTo;
        UpdateGenerationTexts();
        SetChooseButtons(false);

        randomNameCanvas.SetActive(false);
        if (randomNameCanvasLandscape != null)
        {
            randomNameCanvasLandscape.SetActive(false);
        }

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

        SetColorMenuNameTexts("");
    }

    public bool ItIsHorizontal()
    {
        resolutionWidth = Screen.width;
        resolutionHeight = Screen.height;
        return resolutionWidth > 1080;
    }

    public void ActivateRandomName()
    {
        bool horizontal = ItIsHorizontal();
        randomNameCanvas.SetActive(!horizontal);

        if (randomNameCanvasLandscape != null)
        {
            randomNameCanvasLandscape.SetActive(horizontal);
        }

        GameObject callerx = EventSystem.current.currentSelectedGameObject;

        if (callerx != null)
        {
            callerx.SetActive(false);
        }
    }

    public void ChooseRandomName()
    {
        if (numberOfNameGenerations < numberOfNameGenerationsIsLimitedTo)
        {
            randomName = adjektive[Random.Range(0, adjektive.Length)] + " " + tiere[Random.Range(0, tiere.Length)];
            SetRandomNameTexts(randomName);

            numberOfNameGenerations += 1;
            numberOfGenerationsLeft = numberOfNameGenerationsIsLimitedTo - numberOfNameGenerations;
            UpdateGenerationTexts();
        }

        if (numberOfNameGenerations >= numberOfNameGenerationsIsLimitedTo)
        {
            SetGenerateButtons(false);
        }

        SetChooseButtons(numberOfNameGenerations > 0 && !string.IsNullOrEmpty(randomName));
    }

    public void ChooseThisName()
    {
        if (numberOfNameGenerations > 0 && !string.IsNullOrEmpty(randomName))
        {
            SetColorMenuNameTexts(randomName + ",");

            randomNameCanvas.SetActive(false);

            if (randomNameCanvasLandscape != null)
            {
                randomNameCanvasLandscape.SetActive(false);
            }

            ActivateCoulorCanvas();
        }
    }

    public void ActivateCoulorCanvas()
    {
        bool horizontal = ItIsHorizontal();
        colourCanvas.SetActive(!horizontal);

        if (colourCanvasLandscape != null)
        {
            colourCanvasLandscape.SetActive(horizontal);
        }
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

        if (colourCanvasLandscape != null)
        {
            colourCanvasLandscape.SetActive(false);
        }

        SetColorMenuNameTexts("");

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
                if (ItIsHorizontal() && textCanvasLandscape != null)
                {
                    textCanvasLandscape.SetActive(true);
                }
                else
                {
                    textCanvas.SetActive(true);
                }

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

        if (Input.GetKeyDown(KeyCode.Q))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    public IEnumerator WaitTime()
    {
        yield return new WaitForSeconds(2);

        randomNameCanvas.SetActive(false);

        if (randomNameCanvasLandscape != null)
        {
            randomNameCanvasLandscape.SetActive(false);
        }

        ActivateCoulorCanvas();
    }

    public IEnumerator TextWait()
    {
        yield return new WaitForSeconds(15);

        textCanvas.SetActive(false);

        if (textCanvasLandscape != null)
        {
            textCanvasLandscape.SetActive(false);
        }
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

    private void SetRandomNameTexts(string value)
    {
        randomNameText.text = value;

        if (randomNameTextLandscape != null)
        {
            randomNameTextLandscape.text = value;
        }
    }

    private void UpdateGenerationTexts()
    {
        string value = numberOfGenerationsLeft.ToString();

        if (GenerationsLeftText != null)
        {
            GenerationsLeftText.text = value;
        }

        if (GenerationsLeftTextLandscape != null)
        {
            GenerationsLeftTextLandscape.text = value;
        }
    }

    private void SetGenerateButtons(bool interactable)
    {
        if (generateNameButton != null)
        {
            generateNameButton.interactable = interactable;
        }

        if (generateNameButtonLandscape != null)
        {
            generateNameButtonLandscape.interactable = interactable;
        }
    }

    private void SetChooseButtons(bool interactable)
    {
        if (chooseNameButton != null)
        {
            chooseNameButton.interactable = interactable;
        }

        if (chooseNameButtonLandscape != null)
        {
            chooseNameButtonLandscape.interactable = interactable;
        }
    }

    private void SetColorMenuNameTexts(string value)
    {
        if (colorMenuNameText != null)
        {
            colorMenuNameText.text = value;
        }

        if (colorMenuNameTextLandscape != null)
        {
            colorMenuNameTextLandscape.text = value;
        }
    }
}
