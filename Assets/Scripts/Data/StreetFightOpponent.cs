// Milestone 30, Part 2: a generated Street Fight encounter. Wraps a normal
// OpponentInfo (so it flows through the existing BattleScreen/GameManager
// pipeline unchanged) plus the difficulty tier used to roll it, which the
// Street Fight screen deliberately never displays before the fight starts.
public class StreetFightOpponent
{
    public OpponentInfo Opponent;
    public StreetFightDifficulty Difficulty;
    public ArchetypeType PortraitArchetype;
}
