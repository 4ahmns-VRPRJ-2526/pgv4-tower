using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Persistent touch launcher for the three stages of the PG project.</summary>
public sealed class VersionOverviewMenu : MonoBehaviour
{
    [SerializeField] private string overviewScene = "VersionOverview";
    [SerializeField] private string v1Scene = "PGV1_BallonScene";
    [SerializeField] private string v2Scene = "v2_TestScene";
    [SerializeField] private string v3Scene = "WindmillsRGB";
    [SerializeField] private GameObject overviewPanel;
    [SerializeField] private Button v1Button, v2Button, v3Button, overviewButton;
    [SerializeField] private TMP_Text status;
    [SerializeField] private CanvasScaler scaler;
    [SerializeField] private RectTransform[] cards;
    [SerializeField] private RectTransform title, subtitle, footer;
    [SerializeField] private RectTransform brand, evolution;
    private static VersionOverviewMenu instance;
    private bool loading;
    private int width, height;

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
        v1Button.onClick.AddListener(() => OpenVersion(v1Scene));
        v2Button.onClick.AddListener(() => OpenVersion(v2Scene));
        v3Button.onClick.AddListener(() => OpenVersion(v3Scene));
        overviewButton.onClick.AddListener(() => OpenVersion(overviewScene));
        SceneManager.sceneLoaded += SceneLoaded;
        Refresh();
        Layout();
    }

    private void Update()
    {
        if (instance != this) return;
        if (Screen.width != width || Screen.height != height) Layout();
        if (Input.GetKeyDown(KeyCode.Escape) && !overviewPanel.activeSelf) OpenVersion(overviewScene);
    }

    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        loading = false;
        Refresh();
    }

    private void Refresh()
    {
        bool overview = SceneManager.GetActiveScene().name == overviewScene;
        overviewPanel.SetActive(overview);
        overviewButton.gameObject.SetActive(!overview);
        v1Button.interactable = CanOpen(v1Scene);
        v2Button.interactable = CanOpen(v2Scene);
        v3Button.interactable = CanOpen(v3Scene);
        overviewButton.interactable = true;
        if (status != null) status.text = "Drei Versionen. Eine Idee, die weiterwächst.";
    }

    private static bool CanOpen(string scene) => !string.IsNullOrWhiteSpace(scene) && Application.CanStreamedLevelBeLoaded(scene);

    private void OpenVersion(string scene)
    {
        if (loading) return;
        if (!CanOpen(scene))
        {
            if (status != null) status.text = "Diese Version ist noch nicht eingebunden.";
            return;
        }
        loading = true;
        v1Button.interactable = v2Button.interactable = v3Button.interactable = overviewButton.interactable = false;
        if (status != null) status.text = "Version wird gestartet …";
        StartCoroutine(OpenAfterFishDeparture(scene));
    }

    private IEnumerator OpenAfterFishDeparture(string scene)
    {
        if (SceneManager.GetActiveScene().name == overviewScene)
        {
            HoloFishSwimmer fish = FindFirstObjectByType<HoloFishSwimmer>();
            if (fish != null) yield return fish.SwimAway();
        }
        SceneManager.LoadSceneAsync(scene);
    }

    private void Layout()
    {
        width = Screen.width;
        height = Screen.height;
        bool portrait = height > width;
        scaler.referenceResolution = portrait ? new Vector2(1080, 1920) : new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = portrait ? 0f : 1f;
        Place(title, new Vector2(.07f, .88f), new Vector2(.93f, .98f));
        Place(brand, new Vector2(.07f, .80f), new Vector2(.93f, .89f));
        Place(subtitle, new Vector2(.07f, .69f), new Vector2(.93f, .79f));
        Place(evolution, new Vector2(.07f, .615f), new Vector2(.93f, .675f));
        Place(footer, new Vector2(.07f, .025f), new Vector2(.93f, .085f));
        for (int i = 0; i < cards.Length; i++)
        {
            float bottom = .43f - i * .155f;
            Place(cards[i], new Vector2(.05f, bottom), new Vector2(.95f, bottom + .13f));
        }
    }

    private static void Place(RectTransform rect, Vector2 min, Vector2 max)
    {
        if (rect == null) return;
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
        if (instance == this) instance = null;
    }
}
