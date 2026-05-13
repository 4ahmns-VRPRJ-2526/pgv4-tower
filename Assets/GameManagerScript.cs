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



    // Liste der m�nnlichen Adjektive
    private string[] adjektiveMaskulin = new string[]
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

    // Liste der weiblicher Adjektive
    private string[] adjektiveNeutrum = new string[]
    {
        "verrücktes", "lustiges", "komisches", "schräges", "albernes", "zappeiges", "flippiges", "witziges", "seltsames", "spaßiges",
        "plumpes", "lautes", "überdrehtes", "durchgeknalltes", "schrilles", "cooles", "lässiges", "stylisches", "entspanntes", "smartes",
        "trendiges", "modernes", "souveränes", "chilliges", "lockeres", "selbstsicheres", "elegantes", "flinkes", "glänzendes", "wildes",
        "starkes", "mächtiges", "brutales", "rasendes", "donnerndes", "explosives", "brennendes", "unaufhaltbares", "tapferes", "mutiges",
        "furchtloses", "heldenhaftes", "krasses", "zähes", "unbesiegbares", "robustes", "hartes", "frostiges", "sonniges", "windiges",
        "stürmisches", "erdiges", "felsiges", "nebliges", "feuriges", "wasserreiches", "dunkles", "gruseliges", "finsteres", "geisterhaftes",
        "spukhaftes", "unheimliches", "totenstilles", "schattenhaftes", "kluges", "schlaues", "neugieriges", "cleveres", "tüftelndes",
        "logisches", "grübelndes", "analytisches", "gelehrtes", "nerdiges", "flauschiges", "zuckersüßes", "niedliches", "funkelndes",
        "kuscheliges", "fröhliches", "glitzerndes", "zartes", "hopsiges", "kulleriges", "buntes", "schimmerndes", "magisches", "verträumtes",
        "freundliches", "witzelndes", "geheimes", "mysteriöses", "unsichtbares", "silbernes", "goldenes", "stahlhartes", "verräterisches",
        "leuchtendes", "elektrisches", "mechanisches", "biestiges", "schlammiges", "kantiges", "schnelles", "leises", "aggressives",
        "geduldiges", "listiges", "gefährliches", "selbstloses", "freches", "verschrobenes", "verwegenes", "legendäres", "episches",
        "chaotisches", "geniales", "verpeiltes", "nasses", "trockenes", "blindes", "taubes", "wandelbares", "fliegendes", "tanzendes",
        "singendes", "brüllendes", "jagendes", "zitterndes", "schnarschendes", "gähnendes", "lachendes", "weinendes", "träumendes"
    };

    // Liste der sachlichen Adjektive
    private string[] adjektiveFeminin = new string[]
    {
        "verrückte", "lustige", "komische", "schräge", "alberne", "zappelige", "flippige", "witzige", "seltsame", "spaßige",
        "plumpe", "laute", "überdrehte", "durchgeknallte", "schrille", "coole", "lässige", "stylische", "entspannte", "smarte",
        "trendige", "moderne", "souveräne", "chillige", "lockere", "selbstsichere", "elegante", "flinke", "glänzende", "wilde",
        "starke", "mächtige", "brutale", "rasende", "donnernde", "explosive", "brennende", "unaufhaltbare", "tapfere", "mutige",
        "furchtlose", "heldenhafte", "krasse", "zähe", "unbesiegbare", "robuste", "harte", "frostige", "sonnige", "windige",
        "stürmische", "erdige", "felsige", "neblige", "feurige", "wasserreiche", "dunkle", "gruselige", "finstere", "geisterhafte",
        "spukhafte", "unheimliche", "totenstille", "schattenhafte", "kluge", "schlaue", "neugierige", "clevere", "tüftelnde",
        "logische", "grübelnde", "analytische", "gelehrte", "nerdige", "flauschige", "zuckersüße", "niedliche", "funkelnde",
        "kuschelige", "fröhliche", "glitzernde", "zarte", "hopsige", "kullerige", "bunte", "schimmernde", "magische", "verträumte",
        "freundliche", "witzelnde", "geheime", "mysteriöse", "unsichtbare", "silberne", "goldene", "stahlharte", "verräterische",
        "leuchtende", "elektrische", "mechanische", "biestige", "schlammige", "kantige", "schnelle", "leise", "aggressive",
        "geduldige", "listige", "gefährliche", "selbstlose", "freche", "verschrobene", "verwegene", "legendäre", "epische",
        "chaotische", "geniale", "verpeite", "nasse", "trockene", "blinde", "taube", "wandelbare", "fliegende", "tanzende",
        "singende", "brüllende", "jagende", "zitternde", "schnarchende", "gähnende", "lachende", "weinende", "träumende"
    };

    //Namen der Tiere
   private string[] tiere = new string[]
    {
        "M:Löwe", "M:Tiger", "M:Bär", "M:Wolf", "M:Fuchs", "M:Hirsch", "M:Eber", "M:Rabe", "M:Panther", "M:Adler",
        "M:Falke", "M:Geier", "M:Stier", "M:Hund", "M:Kater", "M:Hahn", "M:Pfau", "M:Widder", "M:Ziegenbock", "M:Dachs",
        "M:Marder", "M:Schakal", "M:Igel", "M:Hase", "M:Maulwurf", "M:Biber", "M:Otter", "M:Affe", "M:Gorilla", "M:Orang-Utan",
        "M:Schimpanse", "M:Elefant", "M:Wal", "M:Delphin", "M:Hai", "M:Krake", "M:Fisch", "M:Pavian", "M:Yak", "M:Kojote",
        "M:Büffel", "M:Elch", "M:Drache", "M:Greif", "M:Minotaurus", "M:Zentaur", "M:Werwolf",
        "M:Vogel", "M:Pinguin", "M:Strauß", "M:Kranich", "M:Schwan", "M:Spatz", "M:Specht", "M:Uhu", "M:Kauz", "M:Kondor",
        "M:Luchs", "M:Esel", "M:Ochse", "M:Frosch", "M:Kröterich", "M:Molch", "M:Leguan", "M:Iltis",
        "M:Käfer", "M:Skorpion", "M:Marienkäfer", "M:Schmetterling", "M:Rüsselkäfer",
        "N:Zebra", "N:Nashorn", "N:Mammut", "N:Wiesel", "N:Frettchen", "N:Kaninchen", "N:Kamel", "N:Pony", "N:Rind", "N:Maultier",
        "N:Käuzchen"
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
        string eintrag = tiere[Random.Range(0, tiere.Length)]; //Random Tier auswählen
        char genus = eintrag[0]; //"M" oder "N" als genus speichen
        string tierName = eintrag.Substring(2); //"M:" oder "N:" entfernen
        string adjektiv;

        if (genus == 'M')
            adjektiv = adjektiveMaskulin[Random.Range(0, adjektiveMaskulin.Length)];
        else
            adjektiv = adjektiveNeutrum[Random.Range(0, adjektiveNeutrum.Length)];

        randomName = adjektiv + " " + tierName;
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

            float similarity = GetColorSimilarityPercentage(_goalColour, wma.windmillColor);
            goalSphere.GetComponent<Renderer>().material.color = _goalColour;
            achievedSphere.GetComponent<Renderer>().material.color = wma.windmillColor;
            procentageText.text = similarity + "%";
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
        float rDiff = a.r - b.r;
        float gDiff = a.g - b.g;
        float bDiff = a.b - b.b;

        float distance = Mathf.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);
        float knappheit = 1f - (distance / Mathf.Sqrt(3f));

        return Mathf.Clamp((float)System.Math.Round(knappheit * 100f, 2), 0f, 100f);

    }
}

