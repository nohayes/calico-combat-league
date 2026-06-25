// Milestone 62, Part 2: a small, fixed-size lookup from personality to a few
// numeric AI weighting biases - read once per decision in BattleSystem
// (no allocation, no search; Get() is a single switch). These are
// deliberately small ("slightly favor") additive nudges on top of the
// existing chance-based rolls, not a parallel decision system.
public readonly struct PersonalityProfile
{
    // + means more likely to attack instead of recovering/defending.
    public readonly int AttackBiasPercent;
    // + means more likely to choose Recover when the option comes up.
    public readonly int RecoverBiasPercent;
    // + means more likely to choose Parry/Clinch instead of attacking.
    public readonly int DefenseBiasPercent;
    // + means more likely to pick the cheapest affordable move instead of
    // the strongest one; negative means less likely (spends more freely).
    public readonly int ConserveStaminaBiasPercent;
    // If set, the AI leans toward picking an affordable move of this
    // category when one is available, before falling through to the
    // existing chance-based selection.
    public readonly MoveCategory? PreferredCategory;

    public PersonalityProfile(int attackBias, int recoverBias, int defenseBias, int conserveBias, MoveCategory? preferredCategory = null)
    {
        AttackBiasPercent = attackBias;
        RecoverBiasPercent = recoverBias;
        DefenseBiasPercent = defenseBias;
        ConserveStaminaBiasPercent = conserveBias;
        PreferredCategory = preferredCategory;
    }
}

public static class FighterPersonalityTraits
{
    static readonly PersonalityProfile Neutral = new PersonalityProfile(0, 0, 0, 0);

    public static PersonalityProfile Get(FighterPersonality personality)
    {
        switch (personality)
        {
            // Part 2: attacks more often, recovers less, parries less.
            case FighterPersonality.Aggressive:
                return new PersonalityProfile(attackBias: 15, recoverBias: -10, defenseBias: -10, conserveBias: -10);

            // Part 2: favors stamina-efficient attacks, keeps attacking when ahead.
            case FighterPersonality.PressureFighter:
                return new PersonalityProfile(attackBias: 10, recoverBias: -5, defenseBias: 0, conserveBias: -15, MoveCategory.Pressure);

            // Part 2: prefers Parry, waits for openings.
            case FighterPersonality.CounterFighter:
                return new PersonalityProfile(attackBias: -10, recoverBias: 0, defenseBias: 20, conserveBias: 0);

            // Part 2: recovers slightly earlier, clinches more, conserves stamina.
            case FighterPersonality.Patient:
                return new PersonalityProfile(attackBias: -10, recoverBias: 10, defenseBias: 15, conserveBias: 15);

            // Part 2: prioritizes submission moves, values grappling chains.
            case FighterPersonality.SubmissionHunter:
                return new PersonalityProfile(attackBias: 0, recoverBias: 0, defenseBias: 0, conserveBias: 0, MoveCategory.Submission);

            case FighterPersonality.WrestlingSpecialist:
                return new PersonalityProfile(attackBias: 0, recoverBias: 0, defenseBias: 10, conserveBias: 0, MoveCategory.Control);

            case FighterPersonality.KnockoutArtist:
                return new PersonalityProfile(attackBias: 10, recoverBias: -5, defenseBias: 0, conserveBias: -10, MoveCategory.Finisher);

            case FighterPersonality.Showman:
                return new PersonalityProfile(attackBias: 5, recoverBias: 0, defenseBias: -5, conserveBias: -5, MoveCategory.Combo);

            case FighterPersonality.Veteran:
                return new PersonalityProfile(attackBias: 0, recoverBias: 5, defenseBias: 10, conserveBias: 10);

            case FighterPersonality.CalmTechnician:
                return new PersonalityProfile(attackBias: -5, recoverBias: 5, defenseBias: 5, conserveBias: 15);

            case FighterPersonality.Calculated:
                return new PersonalityProfile(attackBias: 0, recoverBias: 0, defenseBias: 10, conserveBias: 15);

            // Part 2: ignores ideal stamina more often, less predictable.
            case FighterPersonality.WildBrawler:
                return new PersonalityProfile(attackBias: 15, recoverBias: -15, defenseBias: -15, conserveBias: -25);

            // Part 3: Mirror Match - "use the player's move set exactly as
            // before" means no behavioral bias at all, just a presentation label.
            case FighterPersonality.Adaptive:
                return Neutral;

            default:
                return Neutral;
        }
    }

    // Milestone 62, Part 4: human-readable label for the Fight Night intro -
    // "PressureFighter" -> "Pressure Fighter". A small fixed lookup, not a
    // reflection-based splitter, since there are only 14 values to name.
    public static string GetDisplayName(FighterPersonality personality)
    {
        switch (personality)
        {
            case FighterPersonality.Aggressive: return "Aggressive";
            case FighterPersonality.PressureFighter: return "Pressure Fighter";
            case FighterPersonality.CounterFighter: return "Counter Fighter";
            case FighterPersonality.Patient: return "Patient";
            case FighterPersonality.SubmissionHunter: return "Submission Hunter";
            case FighterPersonality.WrestlingSpecialist: return "Wrestling Specialist";
            case FighterPersonality.KnockoutArtist: return "Knockout Artist";
            case FighterPersonality.Showman: return "Showman";
            case FighterPersonality.Veteran: return "Veteran";
            case FighterPersonality.CalmTechnician: return "Calm Technician";
            case FighterPersonality.Calculated: return "Calculated";
            case FighterPersonality.WildBrawler: return "Wild Brawler";
            case FighterPersonality.Adaptive: return "Adaptive";
            default: return "";
        }
    }
}
