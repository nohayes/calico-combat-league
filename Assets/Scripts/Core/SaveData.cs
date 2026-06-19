using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public string FighterName;
    public int Level;
    public int XP;
    public int Coins;
    public int StatPoints;
    public int TotalWins;
    public ArchetypeType Archetype;

    public int MaxHealth;
    public int MaxStamina;
    public int Strength;
    public int Defense;
    public int Speed;
    public int Striking;
    public int Grappling;
    public int Submission;

    public List<string> KnownMoveIds = new List<string>();
    public List<string> EquippedMoveIds = new List<string>();
    public List<string> DefeatedOpponentIds = new List<string>();
    public List<string> CompletedGymIds = new List<string>();

    public List<string> InventoryItemIds = new List<string>();
    public List<int> InventoryItemQuantities = new List<int>();

    // Lifetime statistics (Milestone 11). Missing on older saves -> default 0, which is correct.
    public int TotalLosses;
    public int TotalDamageDealt;
    public int TotalDamageTaken;
    public int TotalCoinsEarned;
    public int TotalCoinsSpent;
    public int TotalItemsUsed;
    public int MaxSingleHitDamage;
    public int SubmissionWins;

    public List<string> UnlockedAchievementIds = new List<string>();

    // Persists across StartNewGame/StartFreshGame - a permanent meta-progression record.
    public List<ChampionRecord> HallOfChampions = new List<ChampionRecord>();
}
