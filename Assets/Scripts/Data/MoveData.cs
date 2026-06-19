[System.Serializable]
public class MoveData
{
    public string Id;
    public string Name;
    public string Description;
    public MoveType Type;
    public int Power;
    public int Accuracy;
    public int StaminaCost;
    public MoveEffect Effects;
    public int EffectChance;

    public MoveData(string id, string name, string description, MoveType type, int power, int accuracy, int staminaCost,
        MoveEffect effects = MoveEffect.None, int effectChance = 0)
    {
        Id = id;
        Name = name;
        Description = description;
        Type = type;
        Power = power;
        Accuracy = accuracy;
        StaminaCost = staminaCost;
        Effects = effects;
        EffectChance = effectChance;
    }

    public bool HasEffect(MoveEffect effect) => (Effects & effect) != 0;
}
