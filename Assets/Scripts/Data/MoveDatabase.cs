using System.Collections.Generic;

public static class MoveDatabase
{
    // Milestone 40, Part 6: descriptions rewritten to call out each move's
    // tactical role (starter/pressure/finisher/control) so players can read a
    // move list and recognize a purpose, not just a flavor line. No power,
    // accuracy, stamina cost, or effect chance changed - text only.
    // Milestone 41, Part 1/2: Role/Category added on top - short structured
    // tags for the Moves Screen and battle log, same reasoning as above.
    public static readonly MoveData Jab = new MoveData(
        "jab", "Jab", "Cheap and reliable - a combo starter, not a finisher.", MoveType.Boxing, power: 8, accuracy: 90, staminaCost: 3,
        role: "Combo Starter", category: MoveCategory.Starter);

    public static readonly MoveData Cross = new MoveData(
        "cross", "Cross", "A powerful straight punch - your reliable medium-damage strike.", MoveType.Boxing, power: 14, accuracy: 80, staminaCost: 6,
        effects: MoveEffect.CriticalHit, effectChance: 15, role: "Combo Finisher", category: MoveCategory.Combo);

    public static readonly MoveData LegKick = new MoveData(
        "leg_kick", "Leg Kick", "Chops away at their speed - keep kicking and they'll struggle to keep up.", MoveType.MuayThai, power: 12, accuracy: 75, staminaCost: 5,
        effects: MoveEffect.SpeedReduction, effectChance: 25, role: "Speed Pressure", category: MoveCategory.Pressure);

    public static readonly MoveData DoubleLegTakedown = new MoveData(
        "double_leg_takedown", "Double Leg Takedown", "A heavy takedown that seizes control of the pace - costly, but it dictates terms.", MoveType.Wrestling, power: 16, accuracy: 65, staminaCost: 10,
        role: "Grappling Control", category: MoveCategory.Control);

    public static readonly MoveData Hook = new MoveData(
        "hook", "Hook", "A heavy looping punch built to end fights - your hardest boxing finisher.", MoveType.Boxing, power: 16, accuracy: 70, staminaCost: 7,
        effects: MoveEffect.CriticalHit, effectChance: 20, role: "Heavy Finisher", category: MoveCategory.Finisher);

    public static readonly MoveData BodyShot = new MoveData(
        "body_shot", "Body Shot", "Wears down their guard - soften them up before the bigger shots land.", MoveType.Boxing, power: 10, accuracy: 85, staminaCost: 4,
        effects: MoveEffect.DefenseReduction, effectChance: 25, role: "Defense Pressure", category: MoveCategory.Pressure);

    public static readonly MoveData PushKick = new MoveData(
        "push_kick", "Push Kick", "Cheap, safe, and always available - creates distance without much risk.", MoveType.MuayThai, power: 9, accuracy: 88, staminaCost: 4,
        role: "Distance Control", category: MoveCategory.Defensive);

    public static readonly MoveData KneeStrike = new MoveData(
        "knee_strike", "Knee Strike", "Solid, consistent power - the dependable Muay Thai workhorse.", MoveType.MuayThai, power: 13, accuracy: 78, staminaCost: 6,
        role: "Workhorse Pressure", category: MoveCategory.Pressure);

    public static readonly MoveData ElbowStrike = new MoveData(
        "elbow_strike", "Elbow Strike", "Sharp and cutting, with real knockout odds - high risk, high reward.", MoveType.MuayThai, power: 17, accuracy: 65, staminaCost: 8,
        effects: MoveEffect.CriticalHit, effectChance: 25, role: "High-Risk Pressure", category: MoveCategory.Pressure);

    public static readonly MoveData SpinningBackKick = new MoveData(
        "spinning_back_kick", "Spinning Back Kick", "Can knock them down and lock them up in one motion - the Muay Thai signature finisher.", MoveType.MuayThai, power: 20, accuracy: 60, staminaCost: 11,
        effects: MoveEffect.CriticalHit | MoveEffect.Stun, effectChance: 22, role: "Signature Finisher", category: MoveCategory.Finisher);

    public static readonly MoveData BodyLock = new MoveData(
        "body_lock", "Body Lock", "Controls the pace without much risk - reliable, low-power grappling pressure.", MoveType.Wrestling, power: 10, accuracy: 85, staminaCost: 6,
        role: "Clinch Control", category: MoveCategory.Control);

    public static readonly MoveData Suplex = new MoveData(
        "suplex", "Suplex", "An explosive throw that can flatten and stun in one motion - the wrestling finisher.", MoveType.Wrestling, power: 18, accuracy: 60, staminaCost: 12,
        effects: MoveEffect.CriticalHit | MoveEffect.Stun, effectChance: 20, role: "Wrestling Finisher", category: MoveCategory.Finisher);

    public static readonly MoveData GroundSmash = new MoveData(
        "ground_smash", "Ground Smash", "Once the fight hits the mat, this is how you end it.", MoveType.GroundAndPound, power: 17, accuracy: 70, staminaCost: 9,
        effects: MoveEffect.CriticalHit, effectChance: 15, role: "Ground Control", category: MoveCategory.Control);

    public static readonly MoveData Kimura = new MoveData(
        "kimura", "Kimura", "Steady, reliable power - the BJJ fundamental.", MoveType.BrazilianJiuJitsu, power: 15, accuracy: 70, staminaCost: 8,
        role: "Submission Threat", category: MoveCategory.Submission);

    public static readonly MoveData Armbar = new MoveData(
        "armbar", "Armbar", "Isolates the arm - more dangerous than it looks.", MoveType.BrazilianJiuJitsu, power: 16, accuracy: 68, staminaCost: 9,
        effects: MoveEffect.CriticalHit, effectChance: 20, role: "Submission Threat", category: MoveCategory.Submission);

    public static readonly MoveData TriangleChoke = new MoveData(
        "triangle_choke", "Triangle Choke", "Serious finishing power - the signature BJJ submission.", MoveType.BrazilianJiuJitsu, power: 19, accuracy: 62, staminaCost: 11,
        effects: MoveEffect.CriticalHit, effectChance: 25, role: "Submission Finisher", category: MoveCategory.Submission);

    public static readonly MoveData RearNakedChoke = new MoveData(
        "rear_naked_choke", "Rear Naked Choke", "The hardest-hitting move in the league - and the steepest stamina cost to match.", MoveType.BrazilianJiuJitsu, power: 22, accuracy: 58, staminaCost: 13,
        effects: MoveEffect.CriticalHit, effectChance: 30, role: "Fight-Ending Submission", category: MoveCategory.Submission);

    public static readonly MoveData ElbowBarrage = new MoveData(
        "elbow_barrage", "Elbow Barrage", "A rapid flurry that opens cuts as fast as it lands - built for sustained, bloody pressure.", MoveType.MuayThai, power: 20, accuracy: 63, staminaCost: 10,
        effects: MoveEffect.CriticalHit | MoveEffect.Bleed, effectChance: 22, role: "Bleed Pressure", category: MoveCategory.Pressure);

    public static readonly List<MoveData> All = new List<MoveData>
    {
        Jab, Cross, LegKick, DoubleLegTakedown, Hook, BodyShot, PushKick, KneeStrike, ElbowStrike, SpinningBackKick,
        BodyLock, Suplex, GroundSmash, Kimura, Armbar, TriangleChoke, RearNakedChoke, ElbowBarrage
    };

    public static MoveData GetById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        foreach (var move in All)
        {
            if (move.Id == id) return move;
        }
        return null;
    }

    public static List<MoveData> StartingMoves => new List<MoveData> { Jab, Cross, LegKick, DoubleLegTakedown };

    public static List<MoveData> BoxingTrainerMoves => new List<MoveData> { Jab, Cross, Hook, BodyShot };

    public static List<MoveData> MuayThaiTrainerMoves => new List<MoveData> { LegKick, PushKick, KneeStrike, ElbowStrike };

    public static List<MoveData> MuayThaiLeaderMoves => new List<MoveData> { LegKick, KneeStrike, ElbowStrike, SpinningBackKick };

    public static List<MoveData> WrestlingTrainerMoves => new List<MoveData> { DoubleLegTakedown, BodyLock, GroundSmash };

    public static List<MoveData> WrestlingLeaderMoves => new List<MoveData> { DoubleLegTakedown, BodyLock, GroundSmash, Suplex };

    public static List<MoveData> BjjTrainerMoves => new List<MoveData> { Kimura, Armbar, TriangleChoke };

    public static List<MoveData> BjjLeaderMoves => new List<MoveData> { Kimura, Armbar, TriangleChoke, RearNakedChoke };

    public static List<MoveData> ChampionshipTrainerMoves => new List<MoveData> { Hook, SpinningBackKick, Suplex, Kimura };

    public static List<MoveData> ChampionshipLeaderMoves => new List<MoveData> { Hook, SpinningBackKick, Suplex, Kimura, ElbowBarrage };
}
