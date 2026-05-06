using UnityEngine;
using System.IO;

public enum BallonStates
{
    Start,
    OnPipe,
    WaitingForInput,
    Input,
    Flying,
}
public class Balloon : MonoBehaviour
{
    public PepperOnlyManager pepperOnlyManager;
    public GameObject[] balloons;
    public GameObject sign;
    //public ColorButtons colorButtons;

    public BallonStates state;
    public AudioManager audioManager;
    public GameObject cubeHalten;

    private float scaleFactor = 1.001f;
    private float lerpSpeed = 1.0f;
    private float inflationSpeed = 0.5f;
    public float upwardForce = 0.1f;

    private float scaleFactorOnPipe = 0.5f;
    private float scaleFactorFlying = 3.0f;
    private Vector3 targetScale;
    private Rigidbody rb;
    
    GameObject model;

    public ParticleSystem particles;
    float timer = 3f;
    public float runningTimerPlatzen, runningTimerParticles;

    private Vector3 velocity;
    private bool inflating = false;
    private int pipeAudioPlayed = 0;
    public Vector3 maxScaleBallon = Vector3.one * 4f;
    bool reset = false, waiting = false;
    string filePath;
    int counter = 0;
    // Start is called before the first frame update
    
    void Start()
    {
        filePath = Application.dataPath + "/ballonDatei"+GenerateTimestamp()+".txt";
        state = BallonStates.Start;

        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();

        rb = gameObject.GetComponent<Rigidbody>();
        
        particles = gameObject.GetComponentInParent<Transform>().GetComponentInChildren<ParticleSystem>();
        particles.Stop();

        runningTimerPlatzen = timer;

        if (Display.displays.Length > 1)
        {
            // Activate additionally display 1 
            // (second monitor) connected to the system.
            Display.displays[1].Activate();
        }

        transform.localScale = Vector3.zero;
    }

    private void StartArtefact()
    {
        if (!reset)
        {
            Debug.Log("");
            cubeHalten.SetActive(true);
            inflating = true;
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.zero;
            SelectRandomBalloon();
            state = BallonStates.OnPipe;
            reset = true;
            waiting = false;
        }
       
    }

    public void SelectRandomBalloon()
    {
        foreach(GameObject obj in balloons)
        {
            obj.SetActive(false);
        }

        int rnd = (int)Random.Range(0, balloons.Length);

        balloons[rnd].SetActive(true);
        model = balloons[rnd];
        pepperOnlyManager.SetCurrentBalloon(model);

        //if (colorButtons == null)
        //{
        //    colorButtons = FindObjectOfType<ColorButtons>(true);
        //    Debug.Log("colorButtons null");
        //}

        //colorButtons.SetBalloon(model);

    }

    public void SetTargetScale()
    {
        switch (state)
        {
            case BallonStates.Start:
                targetScale = Vector3.zero;
                break;
            case BallonStates.OnPipe:
                targetScale = Vector3.one * scaleFactorOnPipe;
                break;
            case BallonStates.Input:
                targetScale = Vector3.one * scaleFactorFlying;
                break;
            case BallonStates.Flying:
                targetScale = Vector3.one * scaleFactorFlying;
                break;
        }
    }

    private void ShowInfo()
    {
        sign.SetActive(true);
    }

    private void HideInfo()
    {
        sign.SetActive(false);
    }

    public static string GenerateTimestamp()
    {
        System.DateTime now = System.DateTime.Now;
        string timestamp = now.ToString("yyyy-MM-dd_HH-mm-ss");
        return timestamp;
    }

    private void OnApplicationQuit()
    {
        try
        {
            // Versuche, die Datei zu öffnen und zu schreiben
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Schreibe den Parameter in die Datei
                writer.WriteLine(counter);
            }

            Debug.Log("Parameter erfolgreich in die Datei geschrieben.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Fehler beim Schreiben in die Datei: " + e.Message);
        }
    }

    private void Inflating()
    {
        SetTargetScale();

        switch (state)
        {
            case BallonStates.Start:
                Invoke("StartArtefact", 2f);
                break;
            case BallonStates.OnPipe:

                if (!sign.activeSelf) ShowInfo();

                if (!audioManager.IsPlayingNow() && pipeAudioPlayed == 0)
                {
                    audioManager.PlaySound(GameSounds.BlowAtStart);
                    pipeAudioPlayed++;
                }

                // Verwende SmoothDamp, um die Skalierung reibungslos zu ändern
                InterpolateSmoothDamp();

                // Prüfe, ob die Skalierung nahe genug am Ziel ist
                if (Vector3.Distance(transform.localScale, targetScale) <= 0.01f)
                {
                    inflating = false;
                    state = BallonStates.WaitingForInput;
                    pipeAudioPlayed--;
                }
                
                break;

            case BallonStates.WaitingForInput:
                

                if (pipeAudioPlayed > 0)
                {
                    pipeAudioPlayed--;
                    audioManager.StopSound();
                }
                if (!sign.activeSelf) ShowInfo();

                if (waiting) return;
                particles.Stop();
                runningTimerParticles = 2;
                runningTimerPlatzen = 3;
                Debug.Log("--------------------------------");
                waiting = true;
                
                break;
            case BallonStates.Input:

                if (sign.activeSelf) HideInfo();

                if (!audioManager.IsPlayingNow() && pipeAudioPlayed == 0)
                    {
                        audioManager.PlaySound(GameSounds.BlowAtStart);
                        pipeAudioPlayed++;
                    }

                    Inflate();

                    // Prüfe, ob die Skalierung nahe genug am Ziel ist
                    if (Vector3.Distance(transform.localScale, maxScaleBallon) <= 0.01f)
                    {
                        inflating = false;
                        state = BallonStates.Flying;
                    counter++;
                    Debug.Log(counter);

                    pipeAudioPlayed--;
                    }
                    
                    break;
            case BallonStates.Flying:
                reset = waiting = false;
                
                break;
        }

    }

    private void Inflate()
    {
        targetScale *= scaleFactor;
        InterpolateSmoothDamp();
    }

    private void Deflate()
    {
        targetScale /= scaleFactor;
        InterpolateLerp();
    }

    private void InterpolateLerp()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale, 
            targetScale, 
            Time.deltaTime * lerpSpeed);
    }

    private void InterpolateSmoothDamp()
    {
        transform.localScale = Vector3.SmoothDamp(
            transform.localScale,
            targetScale,
            ref velocity,
            inflationSpeed);
    }

    private void Update()
    {
        Inflating();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (state != BallonStates.WaitingForInput) return;
            if(state != BallonStates.Input)
            {
                state = BallonStates.Input;
                    return;
            }
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            if (state == BallonStates.Input)
            {
                state = BallonStates.WaitingForInput;
                inflating = false;
                return;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            Application.Quit();
        }

        if(state == BallonStates.Flying) runningTimerPlatzen -= Time.deltaTime;
        if (runningTimerPlatzen < 0f)
        {
            cubeHalten.SetActive(false);
        }

        if (particles.isPlaying)
        {
            runningTimerParticles -= Time.deltaTime;
            if (runningTimerParticles < 0f)
            {
                particles.Stop();
                runningTimerParticles = 2f;
            }
        }

        if (state == BallonStates.Flying)
        {
            rb.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == "TriggerCubePlatzen")
        {
            particles.Play();
            audioManager.PlaySound(GameSounds.Plopp);
            model.GetComponent<Renderer>().material.color = Color.white;
            model.SetActive(false);
            state = BallonStates.Start;
            pepperOnlyManager.SwitchCanvas();
            //MissionIDNetworkBehaviour bh = FindAnyObjectByType<MissionIDNetworkBehaviour>();
            //bh.SendTransition(true, Color.yellow);
            Debug.Log("Gesendet");
        }

    }

}
