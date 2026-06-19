using System.Collections.Generic;

public class FighterData
{
    public string Name;
    public FighterStats Stats;
    public List<MoveData> KnownMoves;
    public List<MoveData> EquippedMoves;
    public ArchetypeType Archetype = ArchetypeType.Unspecified;

    public FighterData(string name, FighterStats stats, List<MoveData> knownMoves, List<MoveData> equippedMoves = null)
    {
        Name = name;
        Stats = stats;
        KnownMoves = knownMoves;
        EquippedMoves = equippedMoves ?? BuildDefaultEquipped(knownMoves);
    }

    public static FighterData CreateNewPlayer(string name, ArchetypeType archetype)
    {
        var archetypeInfo = ArchetypeDatabase.GetByType(archetype);
        var stats = new FighterStats();

        List<MoveData> moves;
        if (archetypeInfo != null)
        {
            stats.Strength += archetypeInfo.StrengthDelta;
            stats.Defense += archetypeInfo.DefenseDelta;
            stats.Speed += archetypeInfo.SpeedDelta;
            stats.Striking += archetypeInfo.StrikingDelta;
            stats.Grappling += archetypeInfo.GrapplingDelta;
            stats.Submission += archetypeInfo.SubmissionDelta;
            stats.MaxHealth += archetypeInfo.MaxHealthDelta;
            stats.MaxStamina += archetypeInfo.MaxStaminaDelta;
            stats.ResetForBattle();

            moves = archetypeInfo.StartingMoves != null && archetypeInfo.StartingMoves.Count > 0
                ? new List<MoveData>(archetypeInfo.StartingMoves)
                : new List<MoveData>(MoveDatabase.StartingMoves);
        }
        else
        {
            moves = new List<MoveData>(MoveDatabase.StartingMoves);
        }

        return new FighterData(name, stats, moves) { Archetype = archetype };
    }

    public bool EquipMove(MoveData move)
    {
        if (move == null || EquippedMoves.Contains(move) || EquippedMoves.Count >= 4) return false;
        EquippedMoves.Add(move);
        return true;
    }

    public bool UnequipMove(MoveData move)
    {
        if (move == null || EquippedMoves.Count <= 1) return false;
        return EquippedMoves.Remove(move);
    }

    static List<MoveData> BuildDefaultEquipped(List<MoveData> knownMoves)
    {
        var result = new List<MoveData>();
        for (int i = 0; i < knownMoves.Count && i < 4; i++) result.Add(knownMoves[i]);
        return result;
    }
}
