using System.Collections.Generic;

// Milestone 56, Part 4/5 (Career Highlights Reel): a curated timeline of
// major accomplishments, generated fresh every time it's viewed from data
// the game already tracks (Hall of Champions, gym completion, lifetime
// stats) - no new save fields, no second copy of any of it. Distinct from
// NewsFeedGenerator (Milestone 49, "what's happening right now") - this is
// explicitly a history of milestones, not current state/flavor.
public static class CareerHighlightGenerator
{
    public static List<string> GenerateHighlights(GameManager gm)
    {
        var highlights = new List<string>();
        if (gm == null) return highlights;

        // First gym cleared - the earliest gym (in GymDatabase's existing
        // unlock order) the player has completed.
        foreach (var gym in GymDatabase.AllGyms)
        {
            if (gm.IsGymCompleted(gym))
            {
                highlights.Add($"First Gym Cleared - {gym.GymName}");
                break;
            }
        }

        // Hall of Champions-derived: earliest championship/rival/mirror
        // match entry, plus every Prestige cycle recorded - same title
        // matching CareerScreen's Milestones tab already uses.
        var records = gm.HallOfChampions;
        ChampionRecord firstChampionship = null, firstRival = null, firstMirror = null;
        var prestigeRecords = new List<ChampionRecord>();
        foreach (var r in records)
        {
            string title = r.Title ?? "";
            if (firstChampionship == null && title.Length == 0) firstChampionship = r;
            else if (firstRival == null && title == "Rival Conqueror") firstRival = r;
            else if (firstMirror == null && (title == "True Champion" || title == "Shadow Slayer")) firstMirror = r;
            else if (title.StartsWith("Completed Prestige")) prestigeRecords.Add(r);
        }
        if (firstChampionship != null) highlights.Add($"First Championship - {firstChampionship.CompletionDate}");
        if (firstRival != null) highlights.Add($"Rival Defeated - {firstRival.CompletionDate}");
        if (firstMirror != null) highlights.Add($"Mirror Match Defeated - {firstMirror.CompletionDate}");
        foreach (var p in prestigeRecords)
            highlights.Add($"{p.Title.Substring("Completed ".Length)} - {p.CompletionDate}");

        // Lifetime-statistic thresholds - no date attached (the stat itself
        // doesn't record when it crossed the line), just the accomplishment.
        if (gm.BestWinStreak >= 25) highlights.Add("25 Fight Win Streak");
        if (gm.StreetFightWins >= 50) highlights.Add("50 Street Fight Wins");
        if (gm.TotalCombosTriggered >= 100) highlights.Add("100 Combos");

        return highlights;
    }
}
