using UnityEngine;

// Single source of truth for the game's visual palette and typography scale.
// UIFactory's color/size constants alias these values rather than redefining
// them, so changing the game's look means editing this one file - nothing
// that builds UI (screens, cards, buttons) needs to change.
public static class VisualTheme
{
    // ---------- Colors ----------

    // Primary (brand surfaces)
    public static readonly Color BackgroundColor = new Color(0.07f, 0.06f, 0.06f, 1f);
    public static readonly Color PanelColor = new Color(0.07f, 0.06f, 0.06f, 1f);
    public static readonly Color CardColor = new Color(0.14f, 0.12f, 0.11f, 1f);

    // Secondary (text + neutral chrome)
    public static readonly Color CreamColor = new Color(0.96f, 0.93f, 0.85f, 1f);
    public static readonly Color SecondaryColor = new Color(0.26f, 0.23f, 0.20f, 1f);
    public static readonly Color MutedTextColor = new Color(0.78f, 0.74f, 0.68f, 1f);
    public static readonly Color LockedColor = new Color(0.17f, 0.16f, 0.15f, 1f);

    // Accent (calls to action / status)
    public static readonly Color AccentOrange = new Color(0.93f, 0.46f, 0.09f, 1f);
    public static readonly Color GoldColor = new Color(0.86f, 0.69f, 0.20f, 1f);
    public static readonly Color DangerColor = new Color(0.58f, 0.16f, 0.14f, 1f);
    public static readonly Color PositiveColor = new Color(0.20f, 0.45f, 0.22f, 1f);

    // ---------- Typography scale (point sizes against the 1080x1920 reference canvas) ----------

    public const int HeadingSize = 42;
    public const int SubheadingSize = 26;
    public const int BodySize = 22;
    public const int CaptionSize = 18;
    public const int ButtonTextSize = 28;

    // ---------- Shared style rules ----------

    public const float StandardCornerRadius = 18f; // px, at the 64px sprite resolution UIFactory generates panels/buttons at
    public const float ButtonFadeDuration = 0.08f;
}
