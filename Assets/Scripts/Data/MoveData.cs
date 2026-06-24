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

    // Milestone 41, Part 1/2: presentation-only tactical identity - Role is a
    // short, move-specific phrase ("Combo Starter", "Heavy Finisher") for the
    // Moves Screen's structured display; Category drives the small bracketed
    // tag ([Starter], [Finisher], ...). Neither affects combat math.
    public string Role;
    public MoveCategory Category;

    public MoveData(string id, string name, string description, MoveType type, int power, int accuracy, int staminaCost,
        MoveEffect effects = MoveEffect.None, int effectChance = 0, string role = "", MoveCategory category = MoveCategory.Pressure)
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
        Role = role;
        Category = category;
    }

    public bool HasEffect(MoveEffect effect) => (Effects & effect) != 0;
}
