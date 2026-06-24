using UnityEngine;

// Single source of truth for the game's visual palette and typography scale.
// UIFactory's color/size constants alias these values rather than redefining
// them, so changing the game's look means editing this one file - nothing
// that builds UI (screens, cards, buttons) needs to change.
public static class VisualTheme
{
    // ---------- Colors ----------

    // Milestone 48A (UI Color Unification Pass): single unified palette,
    // replacing the previously ad-hoc-but-similar dark/orange/green/red
    // tones with the brief's exact values. Aliased into UIFactory, so every
    // screen built on those constants picks up the new palette automatically
    // with no further edits - same mechanism as the earlier typography pass.
    // Rule: Gold = importance, Orange = action, Green = positive, Red =
    // negative, Bronze = locked. Purple (RivalDatabase.AccentColor) is kept
    // exactly as-is per the brief - rival identity only, untouched here.

    // Primary (brand surfaces) - three background tones, darkest to lightest,
    // preserving the existing Background/Panel <= Card lightness ordering.
    public static readonly Color BackgroundColor = new Color(0.0784f, 0.0627f, 0.0588f, 1f); // #14100F
    public static readonly Color PanelColor = new Color(0.1333f, 0.1059f, 0.0941f, 1f);       // #221B18
    public static readonly Color CardColor = new Color(0.1765f, 0.1412f, 0.1216f, 1f);        // #2D241F

    // Secondary (text + neutral chrome)
    public static readonly Color CreamColor = new Color(0.9059f, 0.8667f, 0.7843f, 1f);  // #E7DDC8 - Text Cream
    public static readonly Color SecondaryColor = new Color(0.26f, 0.23f, 0.20f, 1f);
    public static readonly Color MutedTextColor = new Color(0.78f, 0.74f, 0.68f, 1f);
    public static readonly Color LockedColor = new Color(0.4784f, 0.4000f, 0.3216f, 1f);  // #7A6652 - Locked Bronze

    // Accent (calls to action / status)
    public static readonly Color AccentOrange = new Color(0.8510f, 0.4157f, 0.1098f, 1f); // #D96A1C - Action Orange
    public static readonly Color GoldColor = new Color(0.8471f, 0.6510f, 0.2353f, 1f);    // #D8A63C - Primary Gold
    public static readonly Color DangerColor = new Color(0.7725f, 0.3255f, 0.3020f, 1f);  // #C5534D - Danger Red
    public static readonly Color PositiveColor = new Color(0.3725f, 0.6824f, 0.3725f, 1f); // #5FAE5F - Success Green

    // ---------- Typography scale (point sizes against the 1080x1920 reference canvas) ----------

    // Font Size Fix: the typography pass added best-fit min/max bounds but
    // left these source-of-truth constants untouched - best-fit only shrinks
    // *down* from whatever max it's given, so capping at the old sizes meant
    // most text (anything that already fit) rendered completely unchanged.
    // Raised the constants themselves instead; every CreateText/CreateHeading/
    // CreateButton call site, and every best-fit max that already referenced
    // these constants, scales up automatically with no further edits.
    public const int HeadingSize = 56;     // was 42 (+33%, major titles)
    public const int SubheadingSize = 32;  // was 26 (+23%, screen headers/dialogue)
    public const int BodySize = 26;        // was 22 (+18%, battle log/body)
    public const int CaptionSize = 20;     // was 18 (+11%, small captions)
    public const int ButtonTextSize = 34;  // was 28 (+21%, buttons)

    // ---------- Shared style rules ----------

    public const float StandardCornerRadius = 18f; // px, at the 64px sprite resolution UIFactory generates panels/buttons at
    public const float ButtonFadeDuration = 0.08f;
}
