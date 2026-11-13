using UnityEngine;

/// <summary>One quiet loop for the entire swarm, rather than six overlapping sounds.</summary>
public sealed class FireflyAmbience : MonoBehaviour
{
    [SerializeField] private AudioClip flies;
    [SerializeField, Range(0f, 1f)] private float volume = 0.035f;
    [SerializeField, Min(0.1f)] private float fadeInDuration = 2f;
    private AudioSource source;

    private void OnEnable()
    {
        if (flies == null) return;
        if (source == null) source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.clip = flies;
        source.loop = true;
        source.spatialBlend = 0f;
        source.volume = 0f;
        source.priority = 200;
        source.Play();
    }

    private void Update()
    {
        if (source != null)
            source.volume = Mathf.MoveTowards(source.volume, volume, Time.unscaledDeltaTime * volume / fadeInDuration);
    }

    private void OnDisable()
    {
        if (source != null) source.Stop();
    }
}
