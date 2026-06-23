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
    AudioClip gymClearedClip;
    AudioClip championVictoryClip;

    // Milestone 35: audio integration pass - new clips for events that
    // previously had no dedicated sound (or silently no-op'd because the
    // clip they looked for never existed).
    AudioClip hoverClip;
    AudioClip backClip;
    AudioClip lightStrikeClip;
    AudioClip heavyStrikeClip;
    AudioClip takedownClip;
    AudioClip submissionClip;
    AudioClip comboTriggerClip;
    AudioClip levelUpClip;
    AudioClip defeatClip1;
    AudioClip defeatClip2;
    AudioClip rivalEncounterClip;
    AudioClip rivalVictoryClip;

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

        // Preserve all pre-Milestone-14 SFX locations, preferring the newer
        // Milestone 35 filenames where one now actually exists.
        clickClip = LoadFirst("Audio/button_click", "Audio/click");
        hitClip = LoadClip("Audio/hit");
        criticalHitClip = LoadClip("Audio/critical_hit");
        victoryClip = LoadClip("Audio/victory");
        gymClearedClip = LoadClip("Audio/gym_cleared");
        // "champion_victory.mp3" never existed; the actual file added this
        // milestone is "Championship.mp3" (capitalized) - try a few casings
        // since asset lookups can be case-sensitive on some platforms.
        championVictoryClip = LoadFirst("Audio/Championship", "Audio/championship", "Audio/champion_victory");

        // Milestone 35: audit found two new asset filenames that don't match
        // the brief's own wording ("light_strike"/"heavy_strike") - the actual
        // files are "light_punch"/"heavy_punch". Wiring to what's really there.
        hoverClip = LoadClip("Audio/button_hover");
        backClip = LoadClip("Audio/button_back");
        lightStrikeClip = LoadClip("Audio/light_punch");
        heavyStrikeClip = LoadClip("Audio/heavy_punch");
        takedownClip = LoadClip("Audio/takedown");
        submissionClip = LoadClip("Audio/submission");
        comboTriggerClip = LoadClip("Audio/combo_trigger");
        levelUpClip = LoadClip("Audio/level_up");
        defeatClip1 = LoadClip("Audio/defeat1");
        defeatClip2 = LoadClip("Audio/defeat2");
        rivalEncounterClip = LoadClip("Audio/rival_encounter");
        rivalVictoryClip = LoadClip("Audio/rival_victory");
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

    // Milestone 35, Part 8: volume scales are relative to the single shared
    // sfxSource.volume (SfxOutputVolume) - UI quiet, combat medium, critical/
    // combo a bit louder, results most prominent. All through the one existing
    // AudioSource.PlayOneShot(clip, volumeScale) overload - no second mixer,
    // no new AudioSource.
    const float UiVolume = 0.55f;
    const float CombatVolume = 0.9f;
    const float EmphasisVolume = 1.15f;
    const float ResultVolume = 1.3f;

    public void PlayClick() => PlaySfx(clickClip, UiVolume + 0.1f);
    public void PlayHover() => PlaySfx(hoverClip, UiVolume);
    public void PlayBack() => PlaySfx(backClip != null ? backClip : clickClip, UiVolume + 0.1f);
    public void PlayHit() => PlaySfx(hitClip, CombatVolume);
    public void PlayCriticalHit() => PlaySfx(criticalHitClip != null ? criticalHitClip : hitClip, EmphasisVolume);
    public void PlayVictory() => PlaySfx(victoryClip, ResultVolume - 0.1f);
    public void PlayDefeat() => PlaySfx(PickDefeatClip(), CombatVolume + 0.1f);
    public void PlayGymCleared() => PlaySfx(gymClearedClip != null ? gymClearedClip : victoryClip, ResultVolume);
    public void PlayChampionVictory() => PlaySfx(championVictoryClip != null ? championVictoryClip : victoryClip, ResultVolume + 0.05f);

    // Part 3: move-type strike sounds.
    public void PlayLightStrike() => PlaySfx(lightStrikeClip != null ? lightStrikeClip : hitClip, CombatVolume);
    public void PlayHeavyStrike() => PlaySfx(heavyStrikeClip != null ? heavyStrikeClip : hitClip, CombatVolume);
    public void PlayTakedown() => PlaySfx(takedownClip != null ? takedownClip : hitClip, CombatVolume);
    public void PlaySubmissionMove() => PlaySfx(submissionClip != null ? submissionClip : hitClip, CombatVolume);

    // Part 4: combo trigger - fired once per activation by the caller (BattleScreen).
    public void PlayComboTrigger() => PlaySfx(comboTriggerClip != null ? comboTriggerClip : criticalHitClip, EmphasisVolume);

    // Part 5: level-up.
    public void PlayLevelUp() => PlaySfx(levelUpClip, ResultVolume);

    // Part 7: rival system.
    public void PlayRivalEncounter() => PlaySfx(rivalEncounterClip != null ? rivalEncounterClip : criticalHitClip, EmphasisVolume);
    public void PlayRivalVictory() => PlaySfx(rivalVictoryClip != null ? rivalVictoryClip : championVictoryClip, ResultVolume + 0.05f);

    // Part 6: alternates randomly between the two defeat clips; if only one
    // happens to be missing, falls back to whichever exists.
    AudioClip PickDefeatClip()
    {
        if (defeatClip1 != null && defeatClip2 != null)
            return Random.value < 0.5f ? defeatClip1 : defeatClip2;
        return defeatClip1 != null ? defeatClip1 : defeatClip2;
    }

    // Part 9: fail safely - a missing clip (null) or missing AudioSource is a
    // silent no-op, never an exception. volumeScale is clamped so a slightly
    // over-tuned category can never clip/exceed the source's own output volume.
    void PlaySfx(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, Mathf.Clamp(volumeScale, 0f, 1.5f));
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
