using UnityEngine;

// Milestone 45: centralized Prestige (New Game+) logic - the level cap,
// opponent scaling, and display formatting all live here so nothing about
// Prestige is hardcoded or duplicated at each call site.
public static class PrestigeSystem
{
    // Part 2: the one place MaxPrestigeLevel is defined - every check against
    // it (GameManager.CanPrestige, the Prestige button's enabled state, etc.)
    // references this constant instead of a literal 10.
    public const int MaxPrestigeLevel = 10;

    const float HpStaminaPerLevel = 0.10f;
    const float CombatStatPerLevel = 0.05f;

    static readonly string[] RomanNumerals =
    {
        "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X"
    };

    // Part 5: the single shared scaling function every opponent's stats pass
    // through (called once, from GameManager.StartBattle) - no per-opponent
    // edits anywhere. +10% HP/Stamina and +5% combat stats per Prestige level,
    // e.g. Prestige 3 = +30% HP/Stamina, +15% combat stats, compounding
    // linearly with level (not exponentially) per the brief's own example.
    public static void ApplyScaling(FighterStats stats, int prestigeLevel)
    {
        if (stats == null || prestigeLevel <= 0) return;

        float hpStaminaMultiplier = 1f + HpStaminaPerLevel * prestigeLevel;
        float combatMultiplier = 1f + CombatStatPerLevel * prestigeLevel;

        stats.MaxHealth = Mathf.RoundToInt(stats.MaxHealth * hpStaminaMultiplier);
        stats.MaxStamina = Mathf.RoundToInt(stats.MaxStamina * hpStaminaMultiplier);
        stats.Strength = Mathf.RoundToInt(stats.Strength * combatMultiplier);
        stats.Defense = Mathf.RoundToInt(stats.Defense * combatMultiplier);
        stats.Speed = Mathf.RoundToInt(stats.Speed * combatMultiplier);
        stats.Striking = Mathf.RoundToInt(stats.Striking * combatMultiplier);
        stats.Grappling = Mathf.RoundToInt(stats.Grappling * combatMultiplier);
        stats.Submission = Mathf.RoundToInt(stats.Submission * combatMultiplier);
        stats.ResetForBattle();
    }

    // Part 6: one consistent display format used everywhere Prestige is
    // shown - "Prestige 0" for the base game, Roman numerals from there.
    public static string FormatLevel(int level)
    {
        if (level <= 0) return "Prestige 0";
        int index = Mathf.Clamp(level, 1, RomanNumerals.Length) - 1;
        return $"Prestige {RomanNumerals[index]}";
    }

    // Part 10: lightweight, text-only status flavor - no new art, no tattoos,
    // just a tiered label that scales with how far the player has pushed Prestige.
    public static string GetStatusLabel(int level)
    {
        if (level <= 0) return "";
        if (level >= 7) return "League Legend";
        if (level >= 4) return "League Veteran";
        return $"{level + 1}x Champion";
    }
}
