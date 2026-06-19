[System.Serializable]
public class FighterStats
{
    public const int StatPointsPerLevel = 3;
    public const int TrainingCost = 50;
    public const int TrainingStatGain = 1;

    public int Level = 1;
    public int XP = 0;
    public int Coins = 0;
    public int StatPoints = 0;

    public int MaxHealth = 100;
    public int CurrentHealth;
    public int MaxStamina = 50;
    public int CurrentStamina;

    public int Strength = 10;
    public int Defense = 8;
    public int Speed = 10;
    public int Striking = 12;
    public int Grappling = 10;
    public int Submission = 8;

    public FighterStats()
    {
        CurrentHealth = MaxHealth;
        CurrentStamina = MaxStamina;
    }

    public bool IsKnockedOut => CurrentHealth <= 0;

    // Leveling curve lives in one place so it stays easy to tune.
    public int XPToNextLevel => Level * 100;

    public FighterStats Clone()
    {
        return new FighterStats
        {
            Level = Level,
            XP = XP,
            Coins = Coins,
            StatPoints = StatPoints,
            MaxHealth = MaxHealth,
            CurrentHealth = MaxHealth,
            MaxStamina = MaxStamina,
            CurrentStamina = MaxStamina,
            Strength = Strength,
            Defense = Defense,
            Speed = Speed,
            Striking = Striking,
            Grappling = Grappling,
            Submission = Submission
        };
    }

    public void ResetForBattle()
    {
        CurrentHealth = MaxHealth;
        CurrentStamina = MaxStamina;
    }

    public int GetPrimaryStatForMoveType(MoveType type)
    {
        switch (type)
        {
            case MoveType.Wrestling:
            case MoveType.Judo:
                return Grappling;
            case MoveType.BrazilianJiuJitsu:
                return Submission;
            case MoveType.GroundAndPound:
                return (Strength + Grappling) / 2;
            default:
                return Striking;
        }
    }

    public void AddXP(int amount)
    {
        XP += amount;
        while (XP >= XPToNextLevel)
        {
            XP -= XPToNextLevel;
            LevelUp();
        }
    }

    void LevelUp()
    {
        Level++;
        MaxHealth += 10;
        MaxStamina += 5;
        StatPoints += StatPointsPerLevel;
        CurrentHealth = MaxHealth;
        CurrentStamina = MaxStamina;
    }

    public int GetStat(StatKind kind)
    {
        switch (kind)
        {
            case StatKind.Strength: return Strength;
            case StatKind.Defense: return Defense;
            case StatKind.Speed: return Speed;
            case StatKind.Striking: return Striking;
            case StatKind.Grappling: return Grappling;
            case StatKind.Submission: return Submission;
            default: return 0;
        }
    }

    void AddToStat(StatKind kind, int amount)
    {
        switch (kind)
        {
            case StatKind.Strength: Strength += amount; break;
            case StatKind.Defense: Defense += amount; break;
            case StatKind.Speed: Speed += amount; break;
            case StatKind.Striking: Striking += amount; break;
            case StatKind.Grappling: Grappling += amount; break;
            case StatKind.Submission: Submission += amount; break;
        }
    }

    public bool SpendStatPoint(StatKind kind)
    {
        if (StatPoints <= 0) return false;
        StatPoints--;
        AddToStat(kind, 1);
        return true;
    }

    public bool TrainStat(StatKind kind)
    {
        if (Coins < TrainingCost) return false;
        Coins -= TrainingCost;
        AddToStat(kind, TrainingStatGain);
        return true;
    }
}
