using System.Collections.Generic;

public static class MoveDatabase
{
    public static readonly MoveData Jab = new MoveData(
        "jab", "Jab", "A quick, light punch.", MoveType.Boxing, power: 8, accuracy: 90, staminaCost: 3);

    public static readonly MoveData Cross = new MoveData(
        "cross", "Cross", "A powerful straight punch.", MoveType.Boxing, power: 14, accuracy: 80, staminaCost: 6,
        effects: MoveEffect.CriticalHit, effectChance: 15);

    public static readonly MoveData LegKick = new MoveData(
        "leg_kick", "Leg Kick", "A low kick to the thigh.", MoveType.MuayThai, power: 12, accuracy: 75, staminaCost: 5,
        effects: MoveEffect.SpeedReduction, effectChance: 25);

    public static readonly MoveData DoubleLegTakedown = new MoveData(
        "double_leg_takedown", "Double Leg Takedown", "A wrestling takedown to the mat.", MoveType.Wrestling, power: 16, accuracy: 65, staminaCost: 10);

    public static readonly MoveData Hook = new MoveData(
        "hook", "Hook", "A looping punch thrown from the side.", MoveType.Boxing, power: 16, accuracy: 70, staminaCost: 7,
        effects: MoveEffect.CriticalHit, effectChance: 20);

    public static readonly MoveData BodyShot = new MoveData(
        "body_shot", "Body Shot", "A punch aimed at the ribs.", MoveType.Boxing, power: 10, accuracy: 85, staminaCost: 4,
        effects: MoveEffect.DefenseReduction, effectChance: 25);

    public static readonly MoveData PushKick = new MoveData(
        "push_kick", "Push Kick", "A pushing kick that creates distance.", MoveType.MuayThai, power: 9, accuracy: 88, staminaCost: 4);

    public static readonly MoveData KneeStrike = new MoveData(
        "knee_strike", "Knee Strike", "A driving knee to the body.", MoveType.MuayThai, power: 13, accuracy: 78, staminaCost: 6);

    public static readonly MoveData ElbowStrike = new MoveData(
        "elbow_strike", "Elbow Strike", "A sharp, cutting elbow strike.", MoveType.MuayThai, power: 17, accuracy: 65, staminaCost: 8,
        effects: MoveEffect.CriticalHit, effectChance: 25);

    public static readonly MoveData SpinningBackKick = new MoveData(
        "spinning_back_kick", "Spinning Back Kick", "A powerful spinning kick.", MoveType.MuayThai, power: 20, accuracy: 60, staminaCost: 11,
        effects: MoveEffect.CriticalHit | MoveEffect.Stun, effectChance: 22);

    public static readonly MoveData BodyLock = new MoveData(
        "body_lock", "Body Lock", "A clinch hold used to control the opponent.", MoveType.Wrestling, power: 10, accuracy: 85, staminaCost: 6);

    public static readonly MoveData Suplex = new MoveData(
        "suplex", "Suplex", "A explosive throw that slams the opponent to the mat.", MoveType.Wrestling, power: 18, accuracy: 60, staminaCost: 12,
        effects: MoveEffect.CriticalHit | MoveEffect.Stun, effectChance: 20);

    public static readonly MoveData GroundSmash = new MoveData(
        "ground_smash", "Ground Smash", "A heavy strike delivered from top position.", MoveType.GroundAndPound, power: 17, accuracy: 70, staminaCost: 9,
        effects: MoveEffect.CriticalHit, effectChance: 15);

    public static readonly MoveData Kimura = new MoveData(
        "kimura", "Kimura", "A shoulder lock submission.", MoveType.BrazilianJiuJitsu, power: 15, accuracy: 70, staminaCost: 8);

    public static readonly MoveData Armbar = new MoveData(
        "armbar", "Armbar", "A joint lock that isolates the arm.", MoveType.BrazilianJiuJitsu, power: 16, accuracy: 68, staminaCost: 9,
        effects: MoveEffect.CriticalHit, effectChance: 20);

    public static readonly MoveData TriangleChoke = new MoveData(
        "triangle_choke", "Triangle Choke", "A leg-based choke submission.", MoveType.BrazilianJiuJitsu, power: 19, accuracy: 62, staminaCost: 11,
        effects: MoveEffect.CriticalHit, effectChance: 25);

    public static readonly MoveData RearNakedChoke = new MoveData(
        "rear_naked_choke", "Rear Naked Choke", "A fight-ending choke from the back.", MoveType.BrazilianJiuJitsu, power: 22, accuracy: 58, staminaCost: 13,
        effects: MoveEffect.CriticalHit, effectChance: 30);

    public static readonly MoveData ElbowBarrage = new MoveData(
        "elbow_barrage", "Elbow Barrage", "A rapid series of cutting elbow strikes.", MoveType.MuayThai, power: 20, accuracy: 63, staminaCost: 10,
        effects: MoveEffect.CriticalHit | MoveEffect.Bleed, effectChance: 22);

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
