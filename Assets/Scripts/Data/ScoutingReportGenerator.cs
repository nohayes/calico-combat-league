// Milestone 63 (Gym Scouting Reports): the one centralized place scouting
// text comes from - per Part 2, "do not scatter text across many files."
// Regular gyms derive their report from the gym leader's existing
// FighterPersonality (Milestone 62) via a small descriptor/expectation
// lookup below; the 4 special destinations get the brief's own fixed text,
// since their rosters mix multiple personalities and don't reduce cleanly
// to one (Part 2's own "if direct derivation is too messy" escape hatch).
public static class ScoutingReportGenerator
{
    // Milestone 63, Part 1/2: derived from the gym leader's personality -
    // Boxing/Muay Thai/Wrestling/BJJ leaders are assigned Aggressive/
    // PressureFighter/CounterFighter/SubmissionHunter (Milestone 62, Part 3),
    // which reproduces the brief's own 4 example reports exactly. Falls back
    // to a neutral line if a gym's leader somehow has no personality assigned.
    public static string GetGymReport(GymInfo gym)
    {
        if (gym == null) return "";

        // Championship's roster deliberately mixes several personalities
        // (Veteran/Calculated/Calm Technician) rather than sharing one -
        // a single derived line wouldn't represent it well, so it gets the
        // brief's own fixed report instead.
        if (gym.GymType == GymType.Championship) return ChampionshipReport;

        var personality = gym.Leader?.Personality ?? FighterPersonality.None;
        return $"{GetDescriptor(personality)}\nExpect {GetExpectation(personality)}.";
    }

    // Milestone 63, Part 5: the same leader personality, just the short
    // display name instead of the full report - reuses
    // FighterPersonalityTraits.GetDisplayName rather than re-deriving a name.
    public static string GetStyleLabel(GymInfo gym)
    {
        if (gym?.GymType == GymType.Championship) return "Elite";
        var personality = gym?.Leader?.Personality ?? FighterPersonality.None;
        return personality == FighterPersonality.None ? "Varied" : FighterPersonalityTraits.GetDisplayName(personality);
    }

    // Milestone 63, Part 3: the 4 special destinations - fixed text per the
    // brief's own examples, since each represents a unique one-off encounter
    // rather than a roster of opponents that could share a derived label.
    public const string ChampionshipReport = "Elite contenders.\nExpect complete fighters and title pressure.";
    public const string RivalReport = "Calculated rival.\nExpect smart combos and no wasted movement.";
    public const string MirrorMatchReport = "Your own reflection.\nExpect your habits turned against you.";
    public const string StreetFightReport = "Unknown opponent.\nExpect random danger and uneven rewards.";

    static string GetDescriptor(FighterPersonality personality)
    {
        switch (personality)
        {
            case FighterPersonality.Aggressive: return "Aggressive strikers.";
            case FighterPersonality.PressureFighter: return "Pressure fighters.";
            case FighterPersonality.CounterFighter: return "Control specialists.";
            case FighterPersonality.Patient: return "Patient grapplers.";
            case FighterPersonality.SubmissionHunter: return "Submission hunters.";
            case FighterPersonality.WrestlingSpecialist: return "Wrestling specialists.";
            case FighterPersonality.KnockoutArtist: return "Knockout artists.";
            case FighterPersonality.Showman: return "Flashy strikers.";
            case FighterPersonality.Veteran: return "Battle-tested veterans.";
            case FighterPersonality.CalmTechnician: return "Calm technicians.";
            case FighterPersonality.Calculated: return "Calculated fighters.";
            case FighterPersonality.WildBrawler: return "Wild brawlers.";
            case FighterPersonality.Adaptive: return "An adaptive mirror.";
            default: return "Well-rounded fighters.";
        }
    }

    static string GetExpectation(FighterPersonality personality)
    {
        switch (personality)
        {
            case FighterPersonality.Aggressive: return "fast combos and pressure";
            case FighterPersonality.PressureFighter: return "heavy damage and stamina pressure";
            case FighterPersonality.CounterFighter: return "clinches, parries, and takedowns";
            case FighterPersonality.Patient: return "clinches and a slow, controlled pace";
            case FighterPersonality.SubmissionHunter: return "patience, counters, and sudden finishes";
            case FighterPersonality.WrestlingSpecialist: return "takedowns and ground control";
            case FighterPersonality.KnockoutArtist: return "heavy finishers and high-risk power";
            case FighterPersonality.Showman: return "combo chains and crowd-pleasing offense";
            case FighterPersonality.Veteran: return "composed, balanced offense and defense";
            case FighterPersonality.CalmTechnician: return "efficient, low-risk exchanges";
            case FighterPersonality.Calculated: return "smart combos and no wasted movement";
            case FighterPersonality.WildBrawler: return "unpredictable, high-risk exchanges";
            case FighterPersonality.Adaptive: return "your own habits turned against you";
            default: return "a balanced mix of offense and defense";
        }
    }
}
