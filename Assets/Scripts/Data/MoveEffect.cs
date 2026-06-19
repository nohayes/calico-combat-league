using System;

[Flags]
public enum MoveEffect
{
    None = 0,
    CriticalHit = 1 << 0,
    Bleed = 1 << 1,
    Stun = 1 << 2,
    DefenseReduction = 1 << 3,
    SpeedReduction = 1 << 4,
    SubmissionBonus = 1 << 5,
    KnockdownChance = 1 << 6
}
