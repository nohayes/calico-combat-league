using System;
using System.Collections.Generic;
using UnityEngine;

// Milestone 30, Part 1/2: Street Fights are optional, randomized battles for
// grinding XP/coins between gym challenges. Opponents are generated on the
// fly from the player's own current stats and the existing move pool - no
// new combat system, no dedicated art (falls back to the same archetype
// placeholder pipeline every other opponent without art already uses).
public static class StreetFightGenerator
{
    static readonly string[] FirstNames =
    {
        "Parking Lot", "Alley Cat", "Sparring", "Concrete", "Sidewalk", "Late Night",
        "Corner Store", "Junkyard", "Rooftop", "Backalley", "Discount", "Off-Brand"
    };

    static readonly string[] LastNames =
    {
        "Pete", "Marco", "Steve", "Carl", "Sam", "Luis", "Tony", "Reggie", "Diego", "Mike", "Nico", "Hank"
    };

    static readonly ArchetypeType[] PortraitArchetypes =
    {
        ArchetypeType.Boxer, ArchetypeType.Wrestler, ArchetypeType.BjjSpecialist, ArchetypeType.MuayThaiFighter
    };

    public static StreetFightOpponent Generate(GameManager gm)
    {
        var rng = new System.Random();

        var difficulty = RollDifficulty(gm?.TotalGymsCleared ?? 0, rng);
        string name = $"{FirstNames[rng.Next(FirstNames.Length)]} {LastNames[rng.Next(LastNames.Length)]}";

        var playerStats = gm?.Player?.Stats;
        int baseLevel = Mathf.Max(1, playerStats?.Level ?? 1);
        float statScale = GetStatScale(difficulty, rng);

        int Scale(int baseValue) => Mathf.Max(1, Mathf.RoundToInt(baseValue * statScale));

        var stats = new FighterStats
        {
            Level = baseLevel,
            MaxHealth = Scale(playerStats?.MaxHealth ?? 100),
            MaxStamina = Scale(playerStats?.MaxStamina ?? 50),
            Strength = Scale(playerStats?.Strength ?? 10),
            Defense = Scale(playerStats?.Defense ?? 8),
            Speed = Scale(playerStats?.Speed ?? 10),
            Striking = Scale(playerStats?.Striking ?? 12),
            Grappling = Scale(playerStats?.Grappling ?? 10),
            Submission = Scale(playerStats?.Submission ?? 8)
        };
        stats.CurrentHealth = stats.MaxHealth;
        stats.CurrentStamina = stats.MaxStamina;

        GetRewardRange(difficulty, out int minXP, out int maxXP, out int minCoins, out int maxCoins);
        float levelFactor = 1f + (baseLevel - 1) * 0.12f;
        int rewardXP = Mathf.RoundToInt(rng.Next(minXP, maxXP + 1) * levelFactor);
        int rewardCoins = Mathf.RoundToInt(rng.Next(minCoins, maxCoins + 1) * levelFactor);

        var opponent = new OpponentInfo
        {
            OpponentId = $"street_fight_{Guid.NewGuid():N}",
            Name = name,
            Stats = stats,
            Moves = PickMoves(rng),
            RewardXP = rewardXP,
            RewardCoins = rewardCoins,
            Nickname = GetNickname(difficulty),
            Description = "A street fighter looking for some action.",
            Bio = "Just another local fighter looking to make a name for themself.",
            LossLine = "Alright, alright - you got me. Catch me on a better day.",
            WinLine = "Street rules, baby. Go home and train some more.",
            Quote = GetQuote(difficulty)
        };

        return new StreetFightOpponent
        {
            Opponent = opponent,
            Difficulty = difficulty,
            PortraitArchetype = PortraitArchetypes[rng.Next(PortraitArchetypes.Length)]
        };
    }

    // The fewer gym leaders the player has beaten, the more the roll leans easy.
    static StreetFightDifficulty RollDifficulty(int gymsCleared, System.Random rng)
    {
        int tier = Mathf.Clamp(gymsCleared, 0, 4);

        int[] weights;
        switch (tier)
        {
            case 0: weights = new[] { 55, 35, 8, 2 }; break;
            case 1: weights = new[] { 40, 40, 15, 5 }; break;
            case 2: weights = new[] { 25, 40, 25, 10 }; break;
            case 3: weights = new[] { 15, 35, 35, 15 }; break;
            default: weights = new[] { 10, 30, 35, 25 }; break;
        }

        int total = weights[0] + weights[1] + weights[2] + weights[3];
        int roll = rng.Next(0, total);
        if (roll < weights[0]) return StreetFightDifficulty.Easy;
        roll -= weights[0];
        if (roll < weights[1]) return StreetFightDifficulty.Normal;
        roll -= weights[1];
        if (roll < weights[2]) return StreetFightDifficulty.Hard;
        return StreetFightDifficulty.Dangerous;
    }

    static float GetStatScale(StreetFightDifficulty difficulty, System.Random rng)
    {
        switch (difficulty)
        {
            case StreetFightDifficulty.Easy: return Mathf.Lerp(0.7f, 0.85f, (float)rng.NextDouble());
            case StreetFightDifficulty.Normal: return Mathf.Lerp(0.9f, 1.05f, (float)rng.NextDouble());
            case StreetFightDifficulty.Hard: return Mathf.Lerp(1.1f, 1.3f, (float)rng.NextDouble());
            default: return Mathf.Lerp(1.35f, 1.6f, (float)rng.NextDouble());
        }
    }

    static void GetRewardRange(StreetFightDifficulty difficulty, out int minXP, out int maxXP, out int minCoins, out int maxCoins)
    {
        switch (difficulty)
        {
            case StreetFightDifficulty.Easy: minXP = 15; maxXP = 25; minCoins = 8; maxCoins = 15; break;
            case StreetFightDifficulty.Normal: minXP = 30; maxXP = 45; minCoins = 16; maxCoins = 26; break;
            case StreetFightDifficulty.Hard: minXP = 55; maxXP = 75; minCoins = 28; maxCoins = 42; break;
            default: minXP = 90; maxXP = 120; minCoins = 45; maxCoins = 65; break;
        }
    }

    // Flavor only - shown during the fight intro (Bio/Quote beats), never on
    // the pre-fight Street Fight screen, so it can't tip off the difficulty.
    static string GetNickname(StreetFightDifficulty difficulty)
    {
        switch (difficulty)
        {
            case StreetFightDifficulty.Easy: return "Local Scrapper";
            case StreetFightDifficulty.Normal: return "Regular";
            case StreetFightDifficulty.Hard: return "Known Problem";
            default: return "Local Legend";
        }
    }

    static string GetQuote(StreetFightDifficulty difficulty)
    {
        switch (difficulty)
        {
            case StreetFightDifficulty.Easy: return "Whoa, didn't expect a real fight tonight.";
            case StreetFightDifficulty.Normal: return "Let's see what you've got.";
            case StreetFightDifficulty.Hard: return "You picked the wrong corner tonight.";
            default: return "Last guy who stepped up here didn't walk away looking the same.";
        }
    }

    static List<MoveData> PickMoves(System.Random rng)
    {
        var pool = new List<MoveData>(MoveDatabase.All);
        var picked = new List<MoveData>();
        int count = Mathf.Min(4, pool.Count);
        for (int i = 0; i < count; i++)
        {
            int index = rng.Next(pool.Count);
            picked.Add(pool[index]);
            pool.RemoveAt(index);
        }
        return picked;
    }
}
