using UnityEngine;
using UnityEngine.UI;

public class SettingsScreen : UIScreen
{
    const float VolumeStep = 0.05f;

    readonly Text masterValue;
    readonly Text musicValue;
    readonly Text sfxValue;
    readonly Text fullscreenValue;

    public SettingsScreen(Transform parent, GameManager gm) : base(parent, gm, "SettingsScreen", "gym_map")
    {
        UIFactory.CreateHeading(Root.transform, "AUDIO SETTINGS", new Vector2(0.05f, 0.86f), new Vector2(0.95f, 0.96f));
        UIFactory.CreateCaption(Root.transform, "CALICO COMBAT LEAGUE SOUND DESK",
            new Vector2(0.08f, 0.81f), new Vector2(0.92f, 0.86f), TextAnchor.MiddleCenter);

        masterValue = CreateVolumeRow("MASTER", 0.62f,
            () => AdjustMaster(-VolumeStep), () => AdjustMaster(VolumeStep));
        musicValue = CreateVolumeRow("MUSIC", 0.47f,
            () => AdjustMusic(-VolumeStep), () => AdjustMusic(VolumeStep));
        sfxValue = CreateVolumeRow("SFX", 0.32f,
            () => AdjustSfx(-VolumeStep), () => AdjustSfx(VolumeStep));

        // Milestone 24 (Windows Edition): small, safe desktop convenience - reuses
        // the same card/button helpers as the volume rows above.
        fullscreenValue = CreateToggleRow("FULLSCREEN", 0.17f, ToggleFullscreen);

        UIFactory.CreateCaption(Root.transform, "Settings are saved automatically.",
            new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.155f), TextAnchor.MiddleCenter);

        UIFactory.CreateButton(Root.transform, "BACK", new Vector2(0.3f, 0.02f), new Vector2(0.7f, 0.09f),
            () => GM.ChangeState(GameState.MainMenu), UIFactory.SecondaryColor);
    }

    Text CreateVolumeRow(string label, float yMin, UnityEngine.Events.UnityAction decrease,
        UnityEngine.Events.UnityAction increase)
    {
        var card = UIFactory.CreateCard(Root.transform, label, new Vector2(0.08f, yMin), new Vector2(0.92f, yMin + 0.13f));

        UIFactory.CreateSubheading(card, label, new Vector2(0.05f, 0f), new Vector2(0.38f, 1f));
        UIFactory.CreateButton(card, "-", new Vector2(0.42f, 0.18f), new Vector2(0.56f, 0.82f), decrease, UIFactory.SecondaryColor);
        var value = UIFactory.CreateText(card, "", UIFactory.BodySize, UIFactory.GoldColor, TextAnchor.MiddleCenter,
            new Vector2(0.57f, 0f), new Vector2(0.75f, 1f), FontStyle.Bold);
        UIFactory.CreateButton(card, "+", new Vector2(0.77f, 0.18f), new Vector2(0.93f, 0.82f), increase, UIFactory.AccentOrange);
        return value;
    }

    Text CreateToggleRow(string label, float yMin, UnityEngine.Events.UnityAction toggle)
    {
        var card = UIFactory.CreateCard(Root.transform, label, new Vector2(0.08f, yMin), new Vector2(0.92f, yMin + 0.13f));

        UIFactory.CreateSubheading(card, label, new Vector2(0.05f, 0f), new Vector2(0.5f, 1f));
        var value = UIFactory.CreateText(card, "", UIFactory.BodySize, UIFactory.GoldColor, TextAnchor.MiddleCenter,
            new Vector2(0.52f, 0.15f), new Vector2(0.73f, 0.85f), FontStyle.Bold);
        UIFactory.CreateButton(card, "TOGGLE", new Vector2(0.76f, 0.15f), new Vector2(0.95f, 0.85f), toggle, UIFactory.AccentOrange);
        return value;
    }

    public void Refresh()
    {
        var audio = AudioManager.Instance;
        if (audio == null)
        {
            masterValue.text = "--";
            musicValue.text = "--";
            sfxValue.text = "--";
        }
        else
        {
            masterValue.text = FormatPercent(audio.MasterVolume);
            musicValue.text = FormatPercent(audio.MusicVolume);
            sfxValue.text = FormatPercent(audio.SfxVolume);
        }

        fullscreenValue.text = Screen.fullScreen ? "ON" : "OFF";
    }

    void AdjustMaster(float delta)
    {
        var audio = AudioManager.Instance;
        if (audio == null) return;
        audio.SetMasterVolume(audio.MasterVolume + delta);
        Refresh();
    }

    void AdjustMusic(float delta)
    {
        var audio = AudioManager.Instance;
        if (audio == null) return;
        audio.SetMusicVolume(audio.MusicVolume + delta);
        Refresh();
    }

    void AdjustSfx(float delta)
    {
        var audio = AudioManager.Instance;
        if (audio == null) return;
        audio.SetSfxVolume(audio.SfxVolume + delta);
        Refresh();
    }

    void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
        Refresh();
    }

    static string FormatPercent(float value) => $"{Mathf.RoundToInt(value * 100f)}%";
}
