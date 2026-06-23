// Milestone 31: a hidden move-sequence bonus layered on top of existing
// combat. Sequences reference existing MoveData ids only - no new combat
// system, just a bonus applied to the final move when the trailing moves in
// a fighter's recent-move window match. Fields are deliberately generic
// (Name/Sequence/Bonus/Description) so future combos - champion, secret,
// archetype-exclusive, legendary - are just new entries here, no code changes.
public class ComboData
{
    public string Id;
    public string DisplayName;
    public string DisplaySequence;
    public string[] SequenceMoveIds;
    public string Description;
    public float DamageBonusMultiplier;
    public int StaminaRefund;

    public ComboData(string id, string displayName, string displaySequence, string[] sequenceMoveIds,
        string description, float damageBonusMultiplier, int staminaRefund = 3)
    {
        Id = id;
        DisplayName = displayName;
        DisplaySequence = displaySequence;
        SequenceMoveIds = sequenceMoveIds;
        Description = description;
        DamageBonusMultiplier = damageBonusMultiplier;
        StaminaRefund = staminaRefund;
    }
}
