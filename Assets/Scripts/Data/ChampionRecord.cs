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
}
