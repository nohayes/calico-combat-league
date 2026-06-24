using UnityEngine;

// Milestone 36: presentation-only generators for the Tale of the Tape -
// nicknames, records/win streaks, and broadcast-style flavor stats (age/
// reach/style). Everything here is derived on the fly from data that already
// exists (GameManager lifetime stats, ArchetypeType, gym tier, Street Fight
// difficulty) - nothing new is persisted beyond GameManager.CurrentWinStreak,
// and none of this is ever read by combat.
public static class FightPresentationGenerator
{
    // ---------- Player nickname (Part 2) ----------
    // Generated from Archetype + Level/progression rather than stored, so it
    // grows with the player automatically and never needs migration.

    public static string GetPlayerNickname(GameManager gm)
    {
        if (gm?.Player == null) return "The Challenger";

        bool champion = gm.HasBecomeChampion();
        bool proven = gm.TotalGymsCleared >= 2;

        switch (gm.Player.Archetype)
        {
            case ArchetypeType.Boxer:
                return champion ? "The Undisputed" : proven ? "The Finisher" : "Fast Hands";
            case ArchetypeType.Wrestler:
                return champion ? "The Undisputed" : proven ? "The Destroyer" : "The Grinder";
            case ArchetypeType.BjjSpecialist:
                return champion ? "The Undisputed" : proven ? "The Submission Machine" : "The Technician";
            case ArchetypeType.MuayThaiFighter:
                return champion ? "The Undisputed" : proven ? "The Nightmare" : "The Alley Cat";
            default:
                return "The Challenger";
        }
    }

    // ---------- Records & win streaks (Part 3) ----------

    public static string FormatRecord(int wins, int losses) => $"{wins}-{losses}";

    public static string FormatStreak(int streak) => streak >= 2 ? $"{streak} Fight Win Streak" : "";

    // Opponent records are generated, not stored - deterministic per opponent
    // id (same fighter always shows the same record) and scaled by existing
    // tier signals so tougher opponents read as tougher with no new save data.
    public static void GetGymOpponentRecord(GymType gymType, bool isLeader, string opponentId, out int wins, out int losses, out int streak)
    {
        int tier = (int)gymType; // Boxing=0 .. Championship=4
        var rng = new System.Random(SeedFor(opponentId));

        wins = 9 + tier * 5 + (isLeader ? 8 : 0) + rng.Next(0, 5);
        losses = Mathf.Max(0, (isLeader ? 1 : 2) + rng.Next(0, 2) - tier / 2);
        streak = 2 + tier + (isLeader ? 3 : 0) + rng.Next(0, 3);
    }

    public static void GetStreetFighterRecord(StreetFightDifficulty difficulty, string opponentId, out int wins, out int losses, out int streak)
    {
        int tier = (int)difficulty; // Easy=0 .. Dangerous=3
        var rng = new System.Random(SeedFor(opponentId + difficulty));

        wins = 3 + tier * 4 + rng.Next(0, 4);
        losses = Mathf.Max(0, 4 - tier + rng.Next(0, 3));
        streak = tier >= 2 ? 1 + tier + rng.Next(0, 2) : 0;
    }

    // Milestone 39, Part 4: the Rival Showdown now always happens after the
    // Championship, so TotalGymsCleared is always maxed and no longer a
    // useful scale - the player's own level varies per playthrough instead,
    // keeping this "generated, not hardcoded" while reliably reading as
    // legitimate-final-boss tier (lands right around the brief's own example
    // of a 42-3 record with an 18-fight streak at a typical endgame level).
    public static void GetRivalRecord(GameManager gm, out int wins, out int losses, out int streak)
    {
        int level = gm?.Player?.Stats.Level ?? 20;
        wins = 34 + Mathf.Clamp(level / 3, 0, 14);
        losses = 2 + (level >= 25 ? 1 : 0);
        streak = 14 + Mathf.Clamp(level / 5, 0, 8);
    }

    // Shadow Champion / The Stranger - presented as effectively flawless.
    public static void GetSpecialOpponentRecord(string opponentId, out int wins, out int losses, out int streak)
    {
        var rng = new System.Random(SeedFor(opponentId));
        wins = 20 + rng.Next(0, 10);
        losses = 0;
        streak = wins;
    }

    // ---------- Flavor stats (Part 5) ----------
    // Deterministic per fighter (same seed key always returns the same
    // values) so a fighter's profile doesn't change every time it's viewed.

    public static void GetFlavorStats(ArchetypeType archetype, string seedKey, bool impressive, out int age, out int reachInches, out string style)
    {
        var rng = new System.Random(SeedFor(seedKey + archetype));

        var info = ArchetypeDatabase.GetByType(archetype);
        style = info != null ? info.DisplayName : "Fighter";

        age = 23 + rng.Next(0, 11); // 23-33

        switch (archetype)
        {
            case ArchetypeType.Boxer: reachInches = 72 + rng.Next(0, 4); break; // long reach
            case ArchetypeType.MuayThaiFighter: reachInches = 74 + rng.Next(0, 5); break; // tall frame, longest reach
            case ArchetypeType.Wrestler: reachInches = 68 + rng.Next(0, 3); break; // heavier build, shorter reach
            case ArchetypeType.BjjSpecialist: reachInches = 70 + rng.Next(0, 4); break; // technical specialist, average build
            default: reachInches = 70 + rng.Next(0, 4); break;
        }

        // Rival/Championship-tier profiles read as more impressive across the board.
        if (impressive) reachInches += 2;
    }

    public static string FormatFlavorLine(int age, int reachInches, string style) =>
        $"Age {age}  |  Reach {reachInches}\"  |  {style}";

    static int SeedFor(object key) => (key?.ToString() ?? "").GetHashCode();
}
