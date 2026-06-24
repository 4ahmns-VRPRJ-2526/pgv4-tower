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

    public GameObject InfoButtonNew;

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

    [Header("Finished Game Feedback")]
    [SerializeField] Image[] goalColorImages;
    [SerializeField] Image[] mixedColorImages;

    [SerializeField] TMP_Text[] goalRTexts;
    [SerializeField] TMP_Text[] goalGTexts;
    [SerializeField] TMP_Text[] goalBTexts;

    [SerializeField] TMP_Text[] mixedRTexts;
    [SerializeField] TMP_Text[] mixedGTexts;
    [SerializeField] TMP_Text[] mixedBTexts;

    [SerializeField] Image[] goalRBars;
    [SerializeField] Image[] goalGBars;
    [SerializeField] Image[] goalBBars;

    [SerializeField] Image[] mixedRBars;
    [SerializeField] Image[] mixedGBars;
    [SerializeField] Image[] mixedBBars;

    [SerializeField] TMP_Text[] percentageTexts;

    public GameObject finishedGameCanvas;
    public GameObject finishedGameCanvasLandscape;

    bool alreadyPulled = false;
    public int resolutionWidth, resolutionHeight;
    public bool screenHorizontal;

    [Header("SelectedColor")]
    public Image selectedColorEmpty;
    public GameObject selectedColorCanvas;

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

    public void ActivateSelectedColorCanvas()
    {
        if (selectedColorCanvas != null)
        {
            selectedColorCanvas.SetActive(true);
        }
    }

    public void SelectColorGoal(int a)
    {
        GameObject caller = EventSystem.current.currentSelectedGameObject;
        if (caller != null && selectedColorEmpty != null)
        {
            Image buttonImage = caller.GetComponent<Image>();
            if (buttonImage != null)
            {
                selectedColorEmpty.sprite = buttonImage.sprite;
                selectedColorEmpty.preserveAspect = true;
            }
        }

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

        if (selectedColorCanvas != null)
        {
            selectedColorCanvas.SetActive(true);
        }

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

                InfoButtonNew.SetActive(true);

                StartCoroutine(TextWait());
                hasShownText = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) && !alreadyPulled)
        {
            
            if (ItIsHorizontal())
            {
               finishedGameCanvasLandscape.SetActive(true);
            }
            else
            {
               finishedGameCanvas.SetActive(true);
            }

                achievedSphereParent.SetActive(true);
            goalSphereParent.SetActive(true);

            alreadyPulled = true;

            Color mixedColor = wma.windmillColor;
            float similarity = ColorMatchUtility.CalculatePerceptualMatchPercent(
                mixedColor,
                _goalColour
            );

            goalSphere.GetComponent<Renderer>().material.color = _goalColour;

            achievedSphere.GetComponent<Renderer>().material.color =
                mixedColor;

            UpdateFinishedGameFeedback(_goalColour, mixedColor, similarity);
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

    public void DisableInfoText()
    {
        textCanvas.SetActive(false);

        if (textCanvasLandscape != null)
        {
            textCanvasLandscape.SetActive(false);
        }
    }

    private void UpdateFinishedGameFeedback(
        Color goalColor,
        Color mixedColor,
        float similarity)
    {
        foreach (var text in percentageTexts)
        {
            if (text != null)
                text.text = similarity.ToString("0.##") + "%";
        }

        foreach (var image in goalColorImages)
        {
            SetImageColor(image, goalColor);
        }

        foreach (var image in mixedColorImages)
        {
            SetImageColor(image, mixedColor);
        }

        UpdateChannel(goalRTexts, goalRBars, goalColor.r, Color.red);
        UpdateChannel(goalGTexts, goalGBars, goalColor.g, Color.green);
        UpdateChannel(goalBTexts, goalBBars, goalColor.b, Color.blue);

        UpdateChannel(mixedRTexts, mixedRBars, mixedColor.r, Color.red);
        UpdateChannel(mixedGTexts, mixedGBars, mixedColor.g, Color.green);
        UpdateChannel(mixedBTexts, mixedBBars, mixedColor.b, Color.blue);
    }
    private void UpdateChannel(
    TMP_Text[] texts,
    Image[] bars,
    float value,
    Color barColor)
    {
        float clampedValue = Mathf.Clamp01(value);

        foreach (var text in texts)
        {
            if (text != null)
                text.text = Mathf.RoundToInt(clampedValue * 255f).ToString();
        }

        foreach (var bar in bars)
        {
            if (bar != null)
            {
                bar.fillAmount = clampedValue;
                bar.color = barColor;
            }
        }
    }

    private void SetImageColor(Image image, Color color)
    {
        if (image == null)
        {
            return;
        }

        color.a = 1f;
        image.color = color;
    }

    private void SetColorChannel(TMP_Text text, Image bar, float value, Color barColor)
    {
        float clampedValue = Mathf.Clamp01(value);

        if (text != null)
        {
            text.text = Mathf.RoundToInt(clampedValue * 255f).ToString();
        }

        if (bar != null)
        {
            bar.fillAmount = clampedValue;
            barColor.a = bar.color.a > 0f ? bar.color.a : 1f;
            bar.color = barColor;
        }
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

    private void EnsureInfoTextButton(GameObject parentCanvas)
    {
        if (parentCanvas == null)
        {
            return;
        }

        Transform existingButton = parentCanvas.transform.Find("InfoTextButton");
        GameObject buttonObject = existingButton != null
            ? existingButton.gameObject
            : new GameObject("InfoTextButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));

        buttonObject.transform.SetParent(parentCanvas.transform, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = ItIsHorizontal() ? new Vector2(40f, -480f) : new Vector2(40f, -870f);
        buttonRect.sizeDelta = new Vector2(70f, 60f);

        Image buttonImage = buttonObject.GetComponent<Image>();
        if (buttonImage == null)
        {
            buttonImage = buttonObject.AddComponent<Image>();
        }

        buttonImage.color = Color.white;

        Button button = buttonObject.GetComponent<Button>();
        if (button == null)
        {
            button = buttonObject.AddComponent<Button>();
        }

        button.onClick.RemoveListener(DisableInfoText);
        button.onClick.AddListener(DisableInfoText);

        Text oldText = buttonObject.GetComponentInChildren<Text>();
        if (oldText != null)
        {
            Destroy(oldText.gameObject);
        }

        Transform existingCheckmark = buttonObject.transform.Find("Checkmark");
        GameObject textObject = existingCheckmark != null
            ? existingCheckmark.gameObject
            : new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));

        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI checkmarkText = textObject.GetComponent<TextMeshProUGUI>();
        if (checkmarkText == null)
        {
            checkmarkText = textObject.AddComponent<TextMeshProUGUI>();
        }

        checkmarkText.text = "\u2714";
        checkmarkText.fontSize = 42;
        checkmarkText.alignment = TextAlignmentOptions.Center;
        checkmarkText.color = new Color(0f, 0.82f, 0.03f, 1f);
    }
}
