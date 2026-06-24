// Milestone 56, Part 1/7 (Fight Promotions & Event Types): presentation-only
// generators for Fight Night's event name and event-type billing, in the
// same spirit as FightPresentationGenerator's nicknames/records - everything
// here is derived live from existing GameManager/GymInfo/OpponentInfo state,
// nothing persisted, nothing read by combat.
public static class FightPromotionGenerator
{
    // Generic pool for ordinary fights (trainers, gym leaders, Street Fight).
    // The 3 singular story fights below get fixed, narratively-fitting names
    // instead of a random pick from this pool.
    static readonly string[] EventNamePool =
    {
        "CALICO COMBAT {0}", "FIGHT NIGHT {0}", "ROAD TO GLORY", "RISE OF A CHAMPION",
        "NO MERCY", "PROVING GROUNDS", "THE GAUNTLET", "ALL OR NOTHING"
    };

    // Milestone 56, Part 1: deterministic per (opponent, fight count) pair -
    // TotalBattles only changes once EndBattle resolves, so re-entering or
    // refreshing this same fight before it ends always reproduces the same
    // name, with no save field needed to remember it.
    public static string GetEventName(GameManager gm)
    {
        if (gm?.CurrentOpponentInfo == null) return "FIGHT NIGHT";

        if (gm.CurrentOpponentInfo.OpponentId == GameManager.ShadowChampionId) return "THE FINAL ROUND";
        if (gm.CurrentGym?.GymId == "rival_fight") return "SHOWDOWN SATURDAY";
        if (IsChampionshipLeaderMatch(gm)) return "CHAMPIONSHIP NIGHT";

        int eventNumber = gm.TotalBattles + 1;
        string key = gm.CurrentOpponentInfo.OpponentId + eventNumber;
        var rng = new System.Random(key.GetHashCode());
        string template = EventNamePool[rng.Next(EventNamePool.Length)];
        return template.Contains("{0}") ? string.Format(template, eventNumber) : template;
    }

    // Milestone 56, Part 7: billing tier only - distinct from GetEventName
    // (the "show title") and BattleScreen's existing GetFightBilling (which
    // still drives its own separate "{GYM} MAIN EVENT" style text) - this is
    // purely the short type label the brief asks for.
    public static string GetEventType(GameManager gm)
    {
        if (gm?.CurrentOpponentInfo == null) return "FEATURE BOUT";

        if (gm.CurrentOpponentInfo.OpponentId == GameManager.ShadowChampionId) return "FINAL TEST";
        if (gm.CurrentGym?.GymId == "rival_fight") return "GRUDGE MATCH";
        if (gm.CurrentGym?.GymId == "street_fight") return "UNDERCARD";
        if (IsChampionshipLeaderMatch(gm)) return "TITLE FIGHT";
        if (IsLeaderMatch(gm)) return "MAIN EVENT";
        return "FEATURE BOUT";
    }

    static bool IsLeaderMatch(GameManager gm) =>
        gm.CurrentGym?.Leader != null && gm.CurrentOpponentInfo != null && gm.CurrentGym.Leader.OpponentId == gm.CurrentOpponentInfo.OpponentId;

    static bool IsChampionshipLeaderMatch(GameManager gm) =>
        gm.CurrentGym?.GymType == GymType.Championship && IsLeaderMatch(gm);
}
