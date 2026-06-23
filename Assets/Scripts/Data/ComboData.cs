// Milestone 30, Part 6: a hidden move-sequence bonus. Sequences reference
// existing MoveData ids only - no new combat system, just a bonus applied to
// the final move's damage when the trailing moves match.
public class ComboData
{
    public string Id;
    public string DisplayName;
    public string DisplaySequence;
    public string[] SequenceMoveIds;
    public float DamageBonusMultiplier;

    public ComboData(string id, string displayName, string displaySequence, string[] sequenceMoveIds, float damageBonusMultiplier)
    {
        Id = id;
        DisplayName = displayName;
        DisplaySequence = displaySequence;
        SequenceMoveIds = sequenceMoveIds;
        DamageBonusMultiplier = damageBonusMultiplier;
    }
}
