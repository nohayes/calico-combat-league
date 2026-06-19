using UnityEngine;
using UnityEngine.UI;

public class VictoryScreen : UIScreen
{
    readonly Text highlightText;
    readonly Text rewardText;

    public VictoryScreen(Transform parent, GameManager gm) : base(parent, gm, "VictoryScreen")
    {
        UIFactory.CreateHeading(Root.transform, "VICTORY!", new Vector2(0.1f, 0.7f), new Vector2(0.9f, 0.86f));

        highlightText = UIFactory.CreateText(Root.transform, "", UIFactory.SubheadingSize, UIFactory.GoldColor, TextAnchor.MiddleCenter,
            new Vector2(0.08f, 0.62f), new Vector2(0.92f, 0.69f), FontStyle.Bold);

        UIFactory.CreateCard(Root.transform, "Reward", new Vector2(0.1f, 0.38f), new Vector2(0.9f, 0.6f));
        rewardText = UIFactory.CreateText(Root.transform, "", UIFactory.BodySize, UIFactory.CreamColor, TextAnchor.MiddleCenter,
            new Vector2(0.12f, 0.39f), new Vector2(0.88f, 0.59f));

        UIFactory.CreateButton(Root.transform, "RETURN TO MAP", new Vector2(0.22f, 0.2f), new Vector2(0.78f, 0.31f),
            () => GM.ReturnToMap(), UIFactory.PositiveColor);
    }

    public void Refresh()
    {
        bool gymCleared = GM.LastVictoryUnlockedGym;
        bool moveUnlocked = !string.IsNullOrEmpty(GM.LastUnlockedMoveName);

        if (gymCleared) AudioManager.Instance?.PlayGymCleared();
        else AudioManager.Instance?.PlayVictory();

        if (GM.Player == null)
        {
            highlightText.text = "";
            rewardText.text = "Victory!";
            return;
        }

        if (moveUnlocked)
            highlightText.text = $"NEW MOVE: {GM.LastUnlockedMoveName}!";
        else if (gymCleared)
            highlightText.text = "GYM CLEARED!";
        else
            highlightText.text = "";

        rewardText.text = $"+{GM.LastRewardXP} XP   +{GM.LastRewardCoins} Coins\nLevel {GM.Player.Stats.Level}";
    }
}
