using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Central audio service. Existing one-shot SFX paths remain compatible, while
// music uses two sources so game-state changes can crossfade without overlap.
// Missing clips are always safe: lookups fall back where possible, then no-op.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    const float MusicFadeDuration = 0.75f;
    const string MasterVolumeKey = "audio.masterVolume";
    const string MusicVolumeKey = "audio.musicVolume";
    const string SfxVolumeKey = "audio.sfxVolume";

    const float DefaultMasterVolume = 0.8f;
    const float DefaultMusicVolume = 0.65f;
    const float DefaultSfxVolume = 0.85f;

    readonly Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();

    AudioSource musicSourceA;
    AudioSource musicSourceB;
    AudioSource sfxSource;
    AudioClip currentMusicClip;
    Coroutine musicTransition;

    AudioClip clickClip;
    AudioClip hitClip;
    AudioClip criticalHitClip;
    AudioClip victoryClip;
    AudioClip defeatClip;
    AudioClip gymClearedClip;
    AudioClip championVictoryClip;

    public float MasterVolume { get; private set; }
    public float MusicVolume { get; private set; }
    public float SfxVolume { get; private set; }

    float MusicOutputVolume => MasterVolume * MusicVolume;
    float SfxOutputVolume => MasterVolume * SfxVolume;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        LoadVolumeSettings();

        musicSourceA = CreateMusicSource();
        musicSourceB = CreateMusicSource();

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.volume = SfxOutputVolume;

        // Preserve all pre-Milestone-14 SFX locations.
        clickClip = LoadClip("Audio/click");
        hitClip = LoadClip("Audio/hit");
        criticalHitClip = LoadClip("Audio/critical_hit");
        victoryClip = LoadClip("Audio/victory");
        defeatClip = LoadClip("Audio/defeat");
        gymClearedClip = LoadClip("Audio/gym_cleared");
        championVictoryClip = LoadClip("Audio/champion_victory");
    }

    AudioSource CreateMusicSource()
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;
        source.volume = 0f;
        return source;
    }

    public void PlayForState(GameState state, GymInfo gym = null)
    {
        switch (state)
        {
            case GameState.MainMenu:
            case GameState.FighterCreation:
            case GameState.Settings:
                PlayMusic("Audio/Music/main_menu", "Audio/theme-temo");
                break;

            case GameState.GymScreen:
                PlayGymMusic(gym);
                break;

            case GameState.Battle:
                if (gym != null && gym.GymType == GymType.Championship)
                    PlayMusic("Audio/Music/championship_battle", "Audio/Music/championship", "Audio/Music/battle", "Audio/theme-temo");
                else
                    PlayMusic("Audio/Music/battle", "Audio/theme-temo");
                break;

            case GameState.Victory:
                PlayMusic("Audio/Music/victory", "Audio/theme-temo");
                break;

            case GameState.Defeat:
                PlayMusic("Audio/Music/defeat", "Audio/theme-temo");
                break;

            case GameState.Championship:
                PlayMusic("Audio/Music/champion_celebration", "Audio/Music/championship", "Audio/Music/victory", "Audio/theme-temo");
                break;

            default:
                PlayMusic("Audio/Music/gym", "Audio/theme-temo");
                break;
        }
    }

    void PlayGymMusic(GymInfo gym)
    {
        if (gym != null && !string.IsNullOrEmpty(gym.GymId))
        {
            if (gym.GymType == GymType.Championship)
                PlayMusic("Audio/Music/championship", $"Audio/Music/{gym.GymId}", "Audio/Music/gym", "Audio/theme-temo");
            else
                PlayMusic($"Audio/Music/{gym.GymId}", "Audio/Music/gym", "Audio/theme-temo");
            return;
        }

        PlayMusic("Audio/Music/gym", "Audio/theme-temo");
    }

    void PlayMusic(params string[] resourcePaths)
    {
        var clip = LoadFirst(resourcePaths);
        if (clip == currentMusicClip && IsClipPlaying(clip)) return;

        currentMusicClip = clip;
        if (musicTransition != null) StopCoroutine(musicTransition);
        musicTransition = StartCoroutine(CrossfadeTo(clip, MusicFadeDuration));
    }

    IEnumerator CrossfadeTo(AudioClip nextClip, float duration)
    {
        var nextSource = FindSourcePlaying(nextClip);
        if (nextSource == null)
        {
            nextSource = GetDominantMusicSource() == musicSourceA ? musicSourceB : musicSourceA;
            nextSource.Stop();
            nextSource.clip = nextClip;
            nextSource.volume = 0f;
            if (nextClip != null) nextSource.Play();
        }

        var previousSource = nextSource == musicSourceA ? musicSourceB : musicSourceA;
        float nextStartVolume = nextSource.volume;
        float previousStartVolume = previousSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
            nextSource.volume = Mathf.Lerp(nextStartVolume, nextClip != null ? MusicOutputVolume : 0f, t);
            previousSource.volume = Mathf.Lerp(previousStartVolume, 0f, t);
            yield return null;
        }

        previousSource.Stop();
        previousSource.clip = null;
        previousSource.volume = 0f;
        nextSource.volume = nextClip != null ? MusicOutputVolume : 0f;
        musicTransition = null;
    }

    AudioSource GetDominantMusicSource()
    {
        if (!musicSourceA.isPlaying) return musicSourceB;
        if (!musicSourceB.isPlaying) return musicSourceA;
        return musicSourceA.volume >= musicSourceB.volume ? musicSourceA : musicSourceB;
    }

    AudioSource FindSourcePlaying(AudioClip clip)
    {
        if (clip == null) return null;
        if (musicSourceA.isPlaying && musicSourceA.clip == clip) return musicSourceA;
        if (musicSourceB.isPlaying && musicSourceB.clip == clip) return musicSourceB;
        return null;
    }

    bool IsClipPlaying(AudioClip clip) => FindSourcePlaying(clip) != null;

    AudioClip LoadFirst(params string[] paths)
    {
        for (int i = 0; i < paths.Length; i++)
        {
            var clip = LoadClip(paths[i]);
            if (clip != null) return clip;
        }
        return null;
    }

    AudioClip LoadClip(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        if (clipCache.TryGetValue(path, out var cached)) return cached;

        var clip = Resources.Load<AudioClip>(path);
        clipCache[path] = clip;
        return clip;
    }

    public void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
        ApplyVolumes();
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolume);
        ApplyVolumes();
        PlayerPrefs.Save();
    }

    public void SetSfxVolume(float value)
    {
        SfxVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SfxVolumeKey, SfxVolume);
        ApplyVolumes();
        PlayerPrefs.Save();
    }

    void LoadVolumeSettings()
    {
        MasterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, DefaultMasterVolume));
        MusicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, DefaultMusicVolume));
        SfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, DefaultSfxVolume));
    }

    void ApplyVolumes()
    {
        if (sfxSource != null) sfxSource.volume = SfxOutputVolume;

        if (musicTransition == null)
        {
            if (musicSourceA != null && musicSourceA.isPlaying) musicSourceA.volume = MusicOutputVolume;
            if (musicSourceB != null && musicSourceB.isPlaying) musicSourceB.volume = MusicOutputVolume;
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

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
