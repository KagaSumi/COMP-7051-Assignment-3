using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("References")]
    public AudioSource musicSource;
    public VisualEffectsController visualEffects; // To check Day/Night/Fog
    public Transform player;
    public Transform enemy;

    [Header("Music Clips")]
    public AudioClip dayMusic;
    public AudioClip nightMusic;

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.P; // Button to start/stop music
    public float maxMusicDistance = 30f; // Distance where music is quietest
    public float minMusicDistance = 5f;  // Distance where music is loudest

    public bool isMusicPlaying = true;
    private bool wasNight = false; // Track state to detect changes

    void Start()
    {
        if (musicSource != null)
        {
            musicSource.loop = true;
            musicSource.clip = dayMusic;
            musicSource.Play();
        }
    }

    void Update()
    {
        HandleToggle();
        HandleDayNightSwap();
        HandleVolume();
    }

    // [5 Marks] Start/Stop background music
    void HandleToggle()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isMusicPlaying = !isMusicPlaying;

            if (isMusicPlaying) musicSource.UnPause();
            else musicSource.Pause();

            Debug.Log("Music Toggled: " + isMusicPlaying);
        }
    }

    // [5 Marks] Swap music based on Day/Night
    void HandleDayNightSwap()
    {
        if (visualEffects == null) return;

        // Check if the mode changed since last frame
        if (visualEffects.isNight != wasNight)
        {
            wasNight = visualEffects.isNight;

            // Swap the clip
            AudioClip newClip = visualEffects.isNight ? nightMusic : dayMusic;

            // Save current playback time to make transition smoother (optional)
            float time = musicSource.time;

            musicSource.clip = newClip;
            musicSource.time = time; // Try to resume at same beat (works if songs are same BPM)
            if (isMusicPlaying) musicSource.Play();
        }
    }
    public void SetMusicState(bool shouldPlay)
    {
        isMusicPlaying = shouldPlay;

        if (isMusicPlaying)
        {
            if (!musicSource.isPlaying) musicSource.Play();
            musicSource.UnPause();
        }
        else
        {
            musicSource.Pause();
        }
    }
    // [5 Marks] Fog Volume + [5 Marks] Proximity Volume
    void HandleVolume()
    {
        if (player == null || enemy == null || visualEffects == null) return;

        // 1. Calculate Base Volume based on PROXIMITY
        // Closer = Louder (1.0), Farther = Quieter (0.2)
        float distance = Vector3.Distance(player.position, enemy.position);

        // Math to map distance to volume 0.0 -> 1.0
        // Mathf.InverseLerp gives a % between min and max. 
        // We invert it (1 - x) because we want Close = Loud.
        float proximityVolume = 1.0f - Mathf.InverseLerp(minMusicDistance, maxMusicDistance, distance);

        // Clamp it so it never goes totally silent (keep it backgroundy, e.g., 0.2 minimum)
        proximityVolume = Mathf.Clamp(proximityVolume, 0.2f, 1.0f);

        // 2. Apply FOG Modifier
        // "If fog is on, change volume to HALF what it would be otherwise"
        float fogMultiplier = visualEffects.isFoggy ? 0.5f : 1.0f;

        // 3. Set Final Volume
        musicSource.volume = proximityVolume * fogMultiplier;
    }
}