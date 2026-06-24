using System.Collections.Generic;

// Milestone 49, Part 4 (CCL News Feed): headlines generated fresh every time
// they're viewed, purely from data the game already tracks (Hall of
// Champions, completed gyms, Mirror Match availability, the existing Rival
// Tracker status line) - no new save fields, no persistence, no networking.
// "Purely local," per the brief.
public static class NewsFeedGenerator
{
    public static List<string> GenerateHeadlines(GameManager gm)
    {
        var headlines = new List<string>();
        if (gm?.Player == null) return headlines;
        string name = gm.Player.Name;

        // "Current state" lines lead, like a real news feed's top stories -
        // history (Hall of Champions) follows below as the archive.

        // Mirror Match availability - a real development not yet reflected
        // by any Hall of Champions entry until it's actually beaten.
        if (gm.HasDefeatedRival && !gm.HasDefeatedShadowChampion)
            headlines.Add("Mirror Match challenger emerges.");

        // Rival ambient flavor - reuses the existing Rival Tracker status
        // line (already shown on Profile/Gym Selection) rather than
        // inventing a new rival simulation.
        headlines.Add($"{RivalDatabase.RivalName}: {RivalDatabase.GetRivalStatus(gm)}");

        // Street Fight flavor - one ambient line once the player has ever
        // won one, not per-win, to stay lightweight.
        if (gm.StreetFightWins > 0)
            headlines.Add("Street fighter shocks local crowd.");

        // Completed gyms (excluding the championship gym - already covered
        // by its own Hall of Champions headline below, with no duplicate).
        var gyms = GymDatabase.AllGyms;
        for (int i = 0; i < gyms.Count - 1; i++)
        {
            if (gm.IsGymCompleted(gyms[i]))
                headlines.Add($"{name} defeats {gyms[i].GymName} Champion.");
        }

        // History - one headline per Hall of Champions record, most recent
        // first (the list is already in chronological order).
        var records = gm.HallOfChampions;
        for (int i = records.Count - 1; i >= 0; i--)
            headlines.Add(HeadlineFor(records[i], name));

        return headlines;
    }

    static string HeadlineFor(ChampionRecord record, string name)
    {
        string title = record.Title ?? "";
        if (title == "Rival Conqueror") return $"{name} defeats {RivalDatabase.RivalName}.";
        // Milestone 44 renamed this "True Champion"; pre-Milestone-44 saves
        // may still hold the old "Shadow Slayer" title for past entries.
        if (title == "True Champion" || title == "Shadow Slayer") return $"{name} conquers the Mirror Match.";
        if (title.StartsWith("Completed Prestige")) return $"{title.Substring("Completed ".Length)} achieved.";
        return $"{name} becomes League Champion.";
    }
}
