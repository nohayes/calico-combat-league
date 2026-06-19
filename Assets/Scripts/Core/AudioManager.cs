using UnityEngine;

// Clips are loaded from Resources/Audio by convention. If a file isn't there yet,
// Resources.Load returns null and every Play method silently no-ops - audio is
// entirely optional and the game must run identically without any clips present.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    AudioSource musicSource;
    AudioSource sfxSource;

    AudioClip musicClip;
    AudioClip clickClip;
    AudioClip hitClip;
    AudioClip criticalHitClip;
    AudioClip victoryClip;
    AudioClip defeatClip;
    AudioClip gymClearedClip;
    AudioClip championVictoryClip;

    void Awake()
    {
        Instance = this;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = 0.4f;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.volume = 0.8f;

        musicClip = Resources.Load<AudioClip>("Audio/music");
        clickClip = Resources.Load<AudioClip>("Audio/click");
        hitClip = Resources.Load<AudioClip>("Audio/hit");
        criticalHitClip = Resources.Load<AudioClip>("Audio/critical_hit");
        victoryClip = Resources.Load<AudioClip>("Audio/victory");
        defeatClip = Resources.Load<AudioClip>("Audio/defeat");
        gymClearedClip = Resources.Load<AudioClip>("Audio/gym_cleared");
        championVictoryClip = Resources.Load<AudioClip>("Audio/champion_victory");

        if (musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.Play();
        }
    }

    public void PlayClick() => PlaySfx(clickClip);
    public void PlayHit() => PlaySfx(hitClip);
    public void PlayCriticalHit() => PlaySfx(criticalHitClip != null ? criticalHitClip : hitClip);
    public void PlayVictory() => PlaySfx(victoryClip);
    public void PlayDefeat() => PlaySfx(defeatClip);
    public void PlayGymCleared() => PlaySfx(gymClearedClip != null ? gymClearedClip : victoryClip);
    public void PlayChampionVictory() => PlaySfx(championVictoryClip != null ? championVictoryClip : victoryClip);

    void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }
}
