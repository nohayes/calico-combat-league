using System.Collections.Generic;

// Milestone 30, Part 6: hidden combos. Throwing the right move sequence turns
// the final move into a named "finish" with a damage bonus. Kept hidden from
// any UI - discovery happens only through the battle log, the same way a
// player would stumble onto a real combo by feel.
public static class ComboDatabase
{
    public static readonly ComboData BoxingCombo = new ComboData(
        "boxing_combo", "Boxing Combo Finish", "Jab -> Cross -> Hook",
        new[] { "jab", "cross", "hook" }, 1.5f);

    public static readonly ComboData MuayThaiCombo = new ComboData(
        "muaythai_combo", "Muay Thai Combo Finish", "Leg Kick -> Push Kick -> Knee Strike",
        new[] { "leg_kick", "push_kick", "knee_strike" }, 1.5f);

    public static readonly ComboData WrestlingCombo = new ComboData(
        "wrestling_combo", "Wrestling Combo Finish", "Double Leg -> Body Lock -> Suplex",
        new[] { "double_leg_takedown", "body_lock", "suplex" }, 1.5f);

    public static readonly ComboData BjjCombo = new ComboData(
        "bjj_combo", "BJJ Combo Finish", "Double Leg -> Kimura -> Armbar",
        new[] { "double_leg_takedown", "kimura", "armbar" }, 1.5f);

    public static readonly ComboData GroundAndPoundCombo = new ComboData(
        "gnp_combo", "Ground and Pound Combo Finish", "Double Leg -> Ground Smash",
        new[] { "double_leg_takedown", "ground_smash" }, 1.4f);

    // Longest sequences first: if a 3-move combo and a shorter one could both
    // match the same trailing moves, the more specific one should win.
    public static readonly List<ComboData> All = new List<ComboData>
    {
        BoxingCombo, MuayThaiCombo, WrestlingCombo, BjjCombo, GroundAndPoundCombo
    };

    public static ComboData TryMatch(List<string> recentMoveIds)
    {
        for (int i = 0; i < All.Count; i++)
        {
            var combo = All[i];
            var sequence = combo.SequenceMoveIds;
            if (recentMoveIds.Count < sequence.Length) continue;

            int offset = recentMoveIds.Count - sequence.Length;
            bool matches = true;
            for (int j = 0; j < sequence.Length; j++)
            {
                if (recentMoveIds[offset + j] != sequence[j]) { matches = false; break; }
            }
            if (matches) return combo;
        }
        return null;
    }
}
