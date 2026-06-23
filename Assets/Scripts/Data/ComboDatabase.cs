using System.Collections.Generic;

// Milestone 31: starter combo set. Throwing the right move sequence turns the
// final move into a named "finish" with a damage bonus and a small stamina
// refund. Kept hidden from any UI - discovery happens only through the
// battle log and the small "current chain" readout, the same way a player
// would stumble onto a real combo by feel.
//
// Note: the brief's Muay Thai combo called for "Body Kick" and "Head Kick",
// which don't exist as moves in MoveDatabase. To avoid adding new moves (and
// the gym/progression unlock questions that would raise), Thai Barrage is
// built from existing Muay Thai strikes instead, ending in the gym's actual
// signature finisher - same escalating-kicks feel, zero new move data.
public static class ComboDatabase
{
    public static readonly ComboData OneTwoFinish = new ComboData(
        "one_two_finish", "One-Two Finish", "Jab -> Jab -> Cross",
        new[] { "jab", "jab", "cross" },
        "Set up the cross with a pair of jabs and it lands harder than it has any right to.",
        1.5f);

    public static readonly ComboData ThaiBarrage = new ComboData(
        "thai_barrage", "Thai Barrage", "Leg Kick -> Knee Strike -> Spinning Back Kick",
        new[] { "leg_kick", "knee_strike", "spinning_back_kick" },
        "Chop the legs, drive the knee, then spin through for the finish.",
        1.5f);

    public static readonly ComboData GroundControl = new ComboData(
        "ground_control", "Ground Control", "Double Leg -> Body Lock -> Ground Smash",
        new[] { "double_leg_takedown", "body_lock", "ground_smash" },
        "Take it down, lock it up, end it from the top.",
        1.5f);

    public static readonly ComboData SubmissionChain = new ComboData(
        "submission_chain", "Submission Chain", "Body Lock -> Armbar -> Ground Smash",
        new[] { "body_lock", "armbar", "ground_smash" },
        "The armbar attempt doesn't have to finish the fight when the follow-up does.",
        1.5f);

    // Future expansion point (Part 9): champion/secret/archetype-exclusive/
    // legendary combos just get added here - TryMatch needs no changes.
    public static readonly List<ComboData> All = new List<ComboData>
    {
        OneTwoFinish, ThaiBarrage, GroundControl, SubmissionChain
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
