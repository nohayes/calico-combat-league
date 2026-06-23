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

    // Personality flavor (Milestone 22). Also optional/empty-safe. Bio is shown
    // in the pre-fight intro; LossLine is what this opponent says when the player
    // beats them (shown on Victory); WinLine is what they say when they beat the
    // player (shown on Defeat).
    public string Bio;
    public string LossLine;
    public string WinLine;

    // Milestone 34, Part 5: opt-in flag forwarded onto the FighterData
    // BattleSystem actually reads. False for every opponent except Rival Scratch.
    public bool IsSmartFighter;
}
