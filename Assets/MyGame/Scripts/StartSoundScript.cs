using UnityEngine;

public class StartSoundScript : MonoBehaviour 
{
    [SerializeField] private AudioSource audioSource;

    
    [Range(0f, 1f)]
    public float maxVolume = 1f;

    public float fadeInSpeed = 1f;   
    public float fadeOutSpeed = 1f;  

    private float currentVolume = 0f;

    private GameManagerScript gameManager;

    private void Awake() => gameManager = FindFirstObjectByType<GameManagerScript>();

    void OnEnable()
    {
        if (audioSource == null) return;
        currentVolume = 0f;
        audioSource.volume = 0f;
        audioSource.loop = true; 
        audioSource.Play();      
    }

    void Update()
    {
        if (audioSource == null) return;
        if (gameManager != null && !gameManager.WindmillsActive)
        {
            StopAndReset();
            return;
        }
        if (!audioSource.isPlaying) audioSource.Play();
       
        if (Input.GetKey(KeyCode.Space))
        {
            currentVolume += fadeInSpeed * Time.deltaTime;
        }
        
        else
        {
            currentVolume -= fadeOutSpeed * Time.deltaTime;
        }

        
        currentVolume = Mathf.Clamp(currentVolume, 0f, maxVolume);

        
        audioSource.volume = currentVolume;
    }

    public void StopAndReset()
    {
        currentVolume = 0f;
        if (audioSource == null) return;
        audioSource.volume = 0f;
        audioSource.Stop();
    }

    private void OnDisable() => StopAndReset();
}
