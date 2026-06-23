using UnityEngine;

// Milestone 22, Part 7 created one small recurring rival who comments on the
// player's progress. Milestone 29 gave that rival a first-appearance greeting,
// gym-checkpoint trash talk, and a showdown tease. Milestone 33 turns that into
// an actual ongoing presence: a recognizable identity (Part 1), a status the
// player can check in on (Part 2), real dialogue-box checkpoint encounters
// (Part 3), surprise intercepts (Part 4), small world-presence lines on other
// screens (Part 5), a narrative-only record (Part 6), and cutaway flavor
// (Part 7) - all still pure presentation. No new fights, no new save fields.
public static class RivalDatabase
{
    public const string RivalName = "Rival Scratch";

    // Part 1: rival identity. Archetype reuses the existing portrait pipeline
    // (no new data needed - ArchetypeDatabase already has a display name for it).
    // Personality isn't a literal UI label - it's the voice every line below is
    // written in: confident, sharp-tongued, never cruel, quietly fair.
    public const string PortraitId = "rival_scratch";
    public const ArchetypeType PortraitArchetype = ArchetypeType.Boxer;
    public const string Motto = "Talent doesn't ask permission.";
    public const string Personality = "Confident, sharp-tongued, and quietly impressed every time you don't lose.";

    // Part 9: a distinct accent color for the rival's nameplate/title card,
    // separate from the warm gold/orange used for gyms and rewards.
    public static readonly Color AccentColor = new Color(0.68f, 0.42f, 0.88f, 1f);

    // Part 2: shown once per run, the first time the player reaches the Home screen.
    public static readonly string[] FirstAppearanceLines =
    {
        "Another rookie? Great. Just what this league needed.",
        "Name's Scratch. Remember it - you'll be chasing it for a while. See you at the top... if you make it that far."
    };

    // Part 3: gym-checkpoint dialogue-box encounters. BJJ's also leans into
    // Part 8 (building toward the eventual fight) since it's the last
    // checkpoint before the Championship.
    public static string[] GetGymClearedLines(GymType gymType)
    {
        switch (gymType)
        {
            case GymType.Boxing:
                return new[] { "You survived. That's more than I expected." };
            case GymType.MuayThai:
                return new[] { "Still chasing me?" };
            case GymType.Wrestling:
                return new[] { "You're getting closer." };
            case GymType.BrazilianJiuJitsu:
                return new[] { "Don't lose now. That would be embarrassing.", "See you at the Championship." };
            default:
                return null;
        }
    }

    // Part 6: a narrative-only record - not simulated, just scaled to always
    // read as "a little ahead of you" so the rival feels skilled, not unbeatable.
    public static string GetRivalRecord(GameManager gm)
    {
        int gymsCleared = gm?.TotalGymsCleared ?? 0;
        int wins = 8 + gymsCleared * 3;
        int losses = gymsCleared >= 4 ? 1 : 0;
        return $"{wins} Win{(wins == 1 ? "" : "s")}, {losses} Loss{(losses == 1 ? "" : "es")}";
    }

    // Part 2/5/7: the Rival Tracker status line - shown on Profile, Gym
    // Selection, and Championship. Tracks one discipline ahead of the
    // player's own progress, with an occasional cutaway in place of the
    // normal status to keep it feeling alive rather than a static label.
    public static string GetRivalStatus(GameManager gm)
    {
        if (gm == null) return "Somewhere, training.";
        if (gm.HasBecomeChampion()) return "Waiting at the Finals.";

        if (gm.TotalWins > 0 && gm.TotalWins % 7 == 0)
            return GetCutawayLine(gm.TotalWins);

        int tier = gm.TotalGymsCleared;
        if (tier > 4) tier = 4;
        switch (tier)
        {
            case 0: return "Training at the Boxing Gym.";
            case 1: return "Training at the Muay Thai Gym.";
            case 2: return "Training at the Wrestling Gym.";
            case 3: return "Preparing for the BJJ Academy.";
            default: return "Preparing for the Championship.";
        }
    }

    static readonly string[] CutawayLines =
    {
        "Word is the rival just won another title bout.",
        "Heard the rival cleared a gym without breaking a sweat.",
        "Rumor has it the rival just signed a sponsorship deal."
    };

    static string GetCutawayLine(int totalWins)
    {
        return CutawayLines[(totalWins / 7 - 1) % CutawayLines.Length];
    }

    // Part 4: surprise intercepts - brief, no battle. "Before entering a gym"
    // fires the first time a given gym becomes newly unlocked (skipping the
    // very first gym, since FirstAppearanceLines already covers the start).
    public static string[] GetGymInterceptLines(GymInfo gym)
    {
        string gymName = gym?.GymName ?? "the gym";
        return new[] { $"Heading to the {gymName}? Good luck. You'll need it." };
    }

    public static string[] GetStreetFightInterceptLines()
    {
        return new[] { "Heard you survived a rough one out there. Don't get cocky." };
    }

    public static string[] GetLevelMilestoneInterceptLines(int level)
    {
        return new[] { $"Level {level} already? ...Fine. Maybe you're not hopeless." };
    }

    // Part 6: shown once on the Championship screen - anticipation, not a fight.
    public static string GetShowdownLine()
    {
        return "...So you really did it. Champion. Don't get comfortable - I didn't train this hard to fight someone ELSE for that belt. You and me. Soon.";
    }

    // Part 5/8: an occasional, non-gym-clear comment on the Victory screen, paced
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
