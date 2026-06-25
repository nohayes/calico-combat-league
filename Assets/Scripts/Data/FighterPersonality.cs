// Milestone 62, Part 1: descriptors that slightly influence existing AI
// weighting - deliberately not classes/archetypes, just a label plus a small
// set of numeric biases (see FighterPersonalityTraits). None is the default
// for any opponent that doesn't need one (regular trainers still get one per
// Part 3, but the type itself stays optional/zero-cost for anything that
// doesn't set it).
public enum FighterPersonality
{
    None,
    Aggressive,
    PressureFighter,
    CounterFighter,
    Patient,
    SubmissionHunter,
    WrestlingSpecialist,
    KnockoutArtist,
    Showman,
    Veteran,
    CalmTechnician,
    Calculated,
    WildBrawler,
    Adaptive
}
