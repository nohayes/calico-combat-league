using System.Collections.Generic;

// Adding a new archetype later only requires one more entry here (plus an enum
// value) - no other code references a fixed archetype count.
public static class ArchetypeDatabase
{
    public static readonly List<ArchetypeInfo> All = new List<ArchetypeInfo>
    {
        new ArchetypeInfo
        {
            Type = ArchetypeType.Boxer,
            DisplayName = "Boxer",
            Description = "Fast hands, quick feet. Strong striking, weaker on the ground.",
            FlavorQuote = "Feet first, hands faster.",
            StrikingDelta = 3,
            SpeedDelta = 2,
            GrapplingDelta = -3,
            SubmissionDelta = -2,
            StartingMoves = new List<MoveData> { MoveDatabase.Jab, MoveDatabase.Cross, MoveDatabase.BodyShot, MoveDatabase.DoubleLegTakedown }
        },
        new ArchetypeInfo
        {
            Type = ArchetypeType.Wrestler,
            DisplayName = "Wrestler",
            Description = "Built to control. Tough defense and takedowns, lighter striking.",
            FlavorQuote = "If I can touch you, I can take you down.",
            DefenseDelta = 3,
            GrapplingDelta = 3,
            StrikingDelta = -3,
            SubmissionDelta = -3,
            StartingMoves = new List<MoveData> { MoveDatabase.DoubleLegTakedown, MoveDatabase.BodyLock, MoveDatabase.GroundSmash, MoveDatabase.Jab }
        },
        new ArchetypeInfo
        {
            Type = ArchetypeType.BjjSpecialist,
            DisplayName = "BJJ Specialist",
            Description = "Technical and patient. Elite submissions, fragile in a brawl.",
            FlavorQuote = "Give me your back. Give me the fight.",
            SubmissionDelta = 5,
            DefenseDelta = 2,
            StrikingDelta = -4,
            SpeedDelta = -3,
            StartingMoves = new List<MoveData> { MoveDatabase.Kimura, MoveDatabase.Armbar, MoveDatabase.TriangleChoke, MoveDatabase.Jab }
        },
        new ArchetypeInfo
        {
            Type = ArchetypeType.MuayThaiFighter,
            DisplayName = "Muay Thai Fighter",
            Description = "The art of eight limbs. Tireless and precise, but lighter on defense.",
            FlavorQuote = "Every limb is a weapon. Use all eight.",
            MaxStaminaDelta = 15,
            StrikingDelta = 2,
            SpeedDelta = 1,
            DefenseDelta = -2,
            StartingMoves = new List<MoveData> { MoveDatabase.LegKick, MoveDatabase.PushKick, MoveDatabase.KneeStrike, MoveDatabase.ElbowStrike }
        }
    };

    public static ArchetypeInfo GetByType(ArchetypeType type)
    {
        foreach (var archetype in All)
            if (archetype.Type == type) return archetype;
        return null;
    }
}
