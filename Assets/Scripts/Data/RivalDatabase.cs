// Milestone 22, Part 7 created one small recurring rival who comments on the
// player's progress. Milestone 29 gives that rival an actual presence: a
// first-appearance greeting, gym-checkpoint trash talk, and a showdown tease
// at the championship. Still pure presentation - reads existing GameManager
// stats only, no new fights, no save schema beyond a single per-run bool flag.
public static class RivalDatabase
{
    public const string RivalName = "Rival Scratch";

    // Reuses the existing fighter-portrait pipeline (ArtRegistry/SetFighterPortrait)
    // for the rival's dialogue-box portrait - falls back to an IconFactory
    // placeholder like every opponent without dedicated art already does.
    public const string PortraitId = "rival_scratch";
    public const ArchetypeType PortraitArchetype = ArchetypeType.Boxer;

    // Part 2: shown once per run, the first time the player reaches the Home screen.
    public static readonly string[] FirstAppearanceLines =
    {
        "Another rookie? Great. Just what this league needed.",
        "Name's Scratch. Remember it - you'll be chasing it for a while. See you at the top... if you make it that far."
    };

    // Part 3: gym-checkpoint trash talk, keyed to the four base disciplines.
    // Championship is intentionally excluded here - that moment gets its own
    // showdown tease via GetShowdownLine, since becoming champion is a bigger beat.
    public static string GetGymClearedLine(GymType gymType)
    {
        switch (gymType)
        {
            case GymType.Boxing:
                return "Boxing gym, huh? Cute. Mine took half the time.";
            case GymType.MuayThai:
                return "Still standing after the leg kicks? Not bad. Not GOOD. But not bad.";
            case GymType.Wrestling:
                return "Huh. You might actually make this interesting.";
            case GymType.BrazilianJiuJitsu:
                return "Four gyms down. I'm starting to remember your name. Don't get used to it.";
            default:
                return null;
        }
    }

    // Part 6: shown once on the Championship screen - anticipation, not a fight.
    public static string GetShowdownLine()
    {
        return "...So you really did it. Champion. Don't get comfortable - I didn't train this hard to fight someone ELSE for that belt. You and me. Soon.";
    }

    // Part 5/6: an occasional, non-gym-clear comment on the Victory screen, paced
    // off the existing TotalWins counter so it needs no new saved state and never
    // fires every single fight. Returns null on the fights that should stay quiet.
    public static string GetOccasionalVictoryLine(int totalWins)
    {
        if (totalWins <= 0 || totalWins % 5 != 0) return null;
        return "You're finally catching up. Don't lose before we meet.";
    }

    // Milestone 22, Part 7: the original passive line shown while picking a gym.
    public static string GetLine(GameManager gm)
    {
        if (gm == null) return "";
        if (gm.HasBecomeChampion())
            return "...Fine. You're the real deal. For now. See you at the championship, for real this time.";
        if (gm.TotalGymsCleared > 0)
            return "Okay, you're for real. Don't get comfortable.";
        if (gm.TotalWins > 0)
            return "Not bad. Still think I could take you, though.";
        return "Heard there's a new fighter in town. We'll see.";
    }
}
