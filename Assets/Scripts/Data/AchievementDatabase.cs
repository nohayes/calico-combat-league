using System.Collections.Generic;

// Adding a future achievement only requires one more entry here - GameManager
// evaluates every entry generically by its Metric, with no per-achievement code.
public static class AchievementDatabase
{
    public static readonly List<AchievementData> All = new List<AchievementData>
    {
        new AchievementData
        {
            Id = "first_victory",
            Name = "First Victory",
            Description = "Win your first battle.",
            Metric = AchievementMetric.TotalWins,
            TargetValue = 1
        },
        new AchievementData
        {
            Id = "gym_crusher",
            Name = "Gym Crusher",
            Description = "Defeat your first gym leader.",
            Metric = AchievementMetric.GymsCleared,
            TargetValue = 1
        },
        new AchievementData
        {
            Id = "knockout_artist",
            Name = "Knockout Artist",
            Description = "Deal 30 or more damage in a single exchange.",
            Metric = AchievementMetric.MaxSingleHitDamage,
            TargetValue = 30
        },
        new AchievementData
        {
            Id = "submission_master",
            Name = "Submission Master",
            Description = "Win a fight with a submission-ending move.",
            Metric = AchievementMetric.SubmissionWins,
            TargetValue = 1
        },
        new AchievementData
        {
            Id = "rival_defeated",
            Name = "Talent Doesn't Ask Permission",
            Description = "Defeat Rival Scratch and earn your shot at the Championship.",
            Metric = AchievementMetric.DefeatedRival,
            TargetValue = 1
        },
        new AchievementData
        {
            Id = "champion",
            Name = "Champion",
            Description = "Defeat the final gym and become champion.",
            Metric = AchievementMetric.BecameChampion,
            TargetValue = 1
        },
        new AchievementData
        {
            Id = "secret_fighter_defeated",
            Name = "The Stranger",
            Description = "Defeat the mysterious Street Fight opponent who only appears after you become champion.",
            Metric = AchievementMetric.DefeatedSecretFighter,
            TargetValue = 1
        },
        new AchievementData
        {
            Id = "big_spender",
            Name = "Big Spender",
            Description = "Spend 200 coins total on training and items.",
            Metric = AchievementMetric.CoinsSpent,
            TargetValue = 200
        },
        new AchievementData
        {
            Id = "collector",
            Name = "Collector",
            Description = "Know 8 or more moves at once.",
            Metric = AchievementMetric.MovesKnown,
            TargetValue = 8
        }
    };

    public static AchievementData GetById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        foreach (var achievement in All)
            if (achievement.Id == id) return achievement;
        return null;
    }
}
