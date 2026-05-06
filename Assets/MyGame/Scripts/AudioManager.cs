using UnityEngine;

public enum GameSounds
{
    Plopp = 0,
    Blow = 1,
    BlowAtStart = 2
}

public class AudioManager : MonoBehaviour
{
    private AudioSource audioSrc;
    [SerializeField] private AudioClip ploppClip, blowClip, blowAtStart;

    private void Start()
    {
        audioSrc = gameObject.GetComponent<AudioSource>();
    }

    public bool IsPlayingNow()
    {
        return audioSrc.isPlaying;
    }

    public void StopSound()
    {
        audioSrc.Stop();
    }

    public void PlaySound(GameSounds sound)
    {
        switch (sound)
        {
            case GameSounds.Plopp:
                audioSrc.clip = ploppClip;
                break;
            case GameSounds.Blow:
                audioSrc.clip = blowClip;
                break;
            case GameSounds.BlowAtStart:
                audioSrc.clip = blowClip;
                break;
        }

        audioSrc.PlayDelayed(0f);
    }
}
