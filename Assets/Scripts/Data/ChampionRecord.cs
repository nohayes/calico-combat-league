// A permanent Hall of Champions entry. Stored as plain serializable fields so
// JsonUtility can save a List<ChampionRecord> directly inside SaveData.
[System.Serializable]
public class ChampionRecord
{
    public string FighterName;
    public string Archetype;
    public int FinalLevel;
    public int TotalWinsAtCompletion;
    public string CompletionDate;

    // Milestone 26: optional unique title (e.g. "Shadow Slayer"). Empty/null for
    // every existing record - JsonUtility fills it in as "" for older saves.
    public string Title;
}
