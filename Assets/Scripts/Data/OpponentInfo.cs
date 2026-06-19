using System.Collections.Generic;

public class OpponentInfo
{
    public string OpponentId;
    public string Name;
    public FighterStats Stats;
    public List<MoveData> Moves;
    public int RewardXP;
    public int RewardCoins;

    // Presentation-only flavor (Milestone 12). All optional - empty/null is fine
    // and simply means the UI skips that line.
    public string Nickname;
    public string Quote;
    public string Description;
}
