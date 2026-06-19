using System.Collections.Generic;

public static class ItemDatabase
{
    public static readonly ItemData SmallEnergyDrink = new ItemData(
        "small_energy_drink", "Small Energy Drink", "Restores a small amount of stamina.",
        cost: 30, effectType: ItemEffectType.RestoreStamina, effectAmount: 20);

    public static readonly ItemData LargeEnergyDrink = new ItemData(
        "large_energy_drink", "Large Energy Drink", "Restores a large amount of stamina.",
        cost: 55, effectType: ItemEffectType.RestoreStamina, effectAmount: 40);

    public static readonly ItemData Bandage = new ItemData(
        "bandage", "Bandage", "Restores health.",
        cost: 40, effectType: ItemEffectType.RestoreHealth, effectAmount: 25);

    public static readonly ItemData ProteinShake = new ItemData(
        "protein_shake", "Protein Shake", "Temporarily boosts combat stats for one battle.",
        cost: 75, effectType: ItemEffectType.CombatBuff, effectAmount: 4);

    public static readonly List<ItemData> All = new List<ItemData>
    {
        SmallEnergyDrink, LargeEnergyDrink, Bandage, ProteinShake
    };

    public static ItemData GetById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        foreach (var item in All)
        {
            if (item.Id == id) return item;
        }
        return null;
    }
}
