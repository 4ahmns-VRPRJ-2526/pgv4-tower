using UnityEngine;

public class StartSoundScript : MonoBehaviour 
{
    [SerializeField] private AudioSource audioSource;

    
    [Range(0f, 1f)]
    public float maxVolume = 1f;

    public float fadeInSpeed = 1f;   
    public float fadeOutSpeed = 1f;  

    private float currentVolume = 0f;

    void Start()
    {
        
        audioSource.volume = 0f;
        audioSource.loop = true; 
        audioSource.Play();      
    }

    void Update()
    {
       
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
}
