using System.Collections.Generic;

// Stat deltas are applied on top of FighterStats' baseline defaults once, at
// fighter creation. Modest by design - see ArchetypeDatabase for the actual values.
public class ArchetypeInfo
{
    public ArchetypeType Type;
    public string DisplayName;
    public string Description;
    public string FlavorQuote;

    public int StrengthDelta;
    public int DefenseDelta;
    public int SpeedDelta;
    public int StrikingDelta;
    public int GrapplingDelta;
    public int SubmissionDelta;
    public int MaxHealthDelta;
    public int MaxStaminaDelta;

    public List<MoveData> StartingMoves;
}
