[System.Serializable]
public class ItemData
{
    public string Id;
    public string Name;
    public string Description;
    public int Cost;
    public ItemEffectType EffectType;
    public int EffectAmount;

    public ItemData(string id, string name, string description, int cost, ItemEffectType effectType, int effectAmount)
    {
        Id = id;
        Name = name;
        Description = description;
        Cost = cost;
        EffectType = effectType;
        EffectAmount = effectAmount;
    }
}
