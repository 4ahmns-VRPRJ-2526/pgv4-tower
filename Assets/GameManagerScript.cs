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

    [Header("Result UI")]

    // Kreise

    [SerializeField] Image compareLeftHalf;
    [SerializeField] Image compareRightHalf;

    // RGB Bars Ziel
    [SerializeField] Image goalRBar;
    [SerializeField] Image goalGBar;
    [SerializeField] Image goalBBar;

    // RGB Bars Mix
    [SerializeField] Image mixRBar;
    [SerializeField] Image mixGBar;
    [SerializeField] Image mixBBar;

    // RGB Texte Ziel
    [SerializeField] TMP_Text goalRText;
    [SerializeField] TMP_Text goalGText;
    [SerializeField] TMP_Text goalBText;

    // RGB Texte Mix
    [SerializeField] TMP_Text mixRText;
    [SerializeField] TMP_Text mixGText;
    [SerializeField] TMP_Text mixBText;


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
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate(); // Display 2 aktivieren
        }
        var emission = wma.particlesTop.emission;
        wma = GameObject.FindObjectOfType<WindmillManager>();
        emission.enabled = false;
        foreach (GameObject obj in objectsWithScripts)
        {
            MonoBehaviour[] scripts = obj.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                if (script == this) continue; //failsafe wenn Script is Self
                script.enabled = false;
            }
        }

        foreach (Selectable ui in uiElementsToDisable)
        {
            ui.interactable = false;
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
        randomName = adjektive[Random.Range(0, adjektive.Length)] + " " + tiere[Random.Range(0, tiere.Length)];
        randomNameText.text = randomName;
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
                if (script == this) continue; //failsafe wenn Script is Self
                script.enabled = true;
            }
        }

        foreach (Selectable ui in uiElementsToDisable)
        {
            ui.interactable = true;
        }

        colourCanvas.SetActive(false);

        _goalColour = _colorsArray[a];

    }
    void Update()
    {
        if (ghostAnimator != null)
        {
            stateInfo = ghostAnimator.GetCurrentAnimatorStateInfo(0);
            if (!hasShownText && stateInfo.IsName("Anim2") && stateInfo.normalizedTime >= 1.0f)
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

            Color mixed = wma.windmillColor;

            float similarity = GetColorSimilarityPercentage(_goalColour, mixed);

            procentageText.text = similarity.ToString("F2") + "%";



            compareLeftHalf.color = new Color(
                _goalColour.r,
                _goalColour.g,
                _goalColour.b,
                1f
            );

            compareRightHalf.color = new Color(
                mixed.r,
                mixed.g,
                mixed.b,
                1f
            );



            int goalR = Mathf.RoundToInt(_goalColour.r * 255);
            int goalG = Mathf.RoundToInt(_goalColour.g * 255);
            int goalB = Mathf.RoundToInt(_goalColour.b * 255);



            int mixR = Mathf.RoundToInt(mixed.r * 255);
            int mixG = Mathf.RoundToInt(mixed.g * 255);
            int mixB = Mathf.RoundToInt(mixed.b * 255);



            goalRText.text = goalR.ToString();
            goalGText.text = goalG.ToString();
            goalBText.text = goalB.ToString();

            mixRText.text = mixR.ToString();
            mixGText.text = mixG.ToString();
            mixBText.text = mixB.ToString();




            goalRBar.fillAmount = goalR / 255f;
            goalGBar.fillAmount = goalG / 255f;
            goalBBar.fillAmount = goalB / 255f;

            mixRBar.fillAmount = mixR / 255f;
            mixGBar.fillAmount = mixG / 255f;
            mixBBar.fillAmount = mixB / 255f;


            goalRBar.color = Color.red;
            goalGBar.color = Color.green;
            goalBBar.color = Color.blue;

            mixRBar.color = Color.red;
            mixGBar.color = Color.green;
            mixBBar.color = Color.blue;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) && alreadyPulled)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

        float r1 = a.r * 255f;
        float g1 = a.g * 255f;
        float b1 = a.b * 255f;

        float r2 = b.r * 255f;
        float g2 = b.g * 255f;
        float b2 = b.b * 255f;

        float rDiff = r1 - r2;
        float gDiff = g1 - g2;
        float bDiff = b1 - b2;

        float distance = Mathf.Sqrt(
            rDiff * rDiff +
            gDiff * gDiff +
            bDiff * bDiff
        );


        float maxDistance = Mathf.Sqrt(
            255f * 255f +
            255f * 255f +
            255f * 255f
        );


        float similarity = 1f - (distance / maxDistance);


        return Mathf.Clamp(
            (float)System.Math.Round(similarity * 100f, 2),
            0f,
            100f
        );
    }
}

