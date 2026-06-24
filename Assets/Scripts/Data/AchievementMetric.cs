// Each metric maps to a lifetime stat GameManager already tracks (or derives).
// Adding a new achievement type later means adding one value here plus a case
// in GameManager.GetAchievementProgress - no other system needs to change.
public enum AchievementMetric
{
    TotalWins,
    GymsCleared,
    MaxSingleHitDamage,
    SubmissionWins,
    CoinsSpent,
    MovesKnown,
    BecameChampion,
    DefeatedRival,
    DefeatedSecretFighter,
    DefeatedMirrorMatch,

    // Milestone 49 (Achievement Expansion)
    CombosTriggered,
    Clinches,
    Parries,
    StreetFightWins,
    HighestPrestigeReached,
    WonWithOneHP
}
