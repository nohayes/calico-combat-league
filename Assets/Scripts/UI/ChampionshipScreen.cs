using UnityEngine;
using UnityEngine.UI;

public class ChampionshipScreen : UIScreen
{
    readonly Text infoText;

    public ChampionshipScreen(Transform parent, GameManager gm) : base(parent, gm, "ChampionshipScreen", "championship")
    {
        UIFactory.CreateChampionBadge(Root.transform, new Vector2(0.3f, 0.78f), new Vector2(0.7f, 0.99f));

        UIFactory.CreateCaption(Root.transform, "CALICO COMBAT LEAGUE", new Vector2(0.1f, 0.735f), new Vector2(0.9f, 0.78f), TextAnchor.MiddleCenter);

        UIFactory.CreateText(Root.transform, "CHAMPION!", UIFactory.HeadingSize, UIFactory.GoldColor, TextAnchor.MiddleCenter,
            new Vector2(0.1f, 0.62f), new Vector2(0.9f, 0.73f), FontStyle.Bold);

        UIFactory.CreateCard(Root.transform, "ChampionInfo", new Vector2(0.1f, 0.23f), new Vector2(0.9f, 0.6f));
        infoText = UIFactory.CreateText(Root.transform, "", UIFactory.BodySize, UIFactory.CreamColor, TextAnchor.MiddleCenter,
            new Vector2(0.12f, 0.24f), new Vector2(0.88f, 0.59f));

        UIFactory.CreateButton(Root.transform, "CONTINUE", new Vector2(0.25f, 0.08f), new Vector2(0.75f, 0.19f),
            () => GM.ReturnToMap(), UIFactory.PositiveColor);
    }

    public void Refresh()
    {
        AudioManager.Instance?.PlayChampionVictory();
        PlayCelebration();

        if (GM.Player == null)
        {
            infoText.text = "Champion!";
            return;
        }

        var info = ArchetypeDatabase.GetByType(GM.Player.Archetype);
        string archetypeName = info != null ? info.DisplayName : "Fighter";

        infoText.text =
            $"{GM.Player.Name}\n{archetypeName}\n\n" +
            $"Final Level: {GM.Player.Stats.Level}\n" +
            $"Total Wins: {GM.TotalWins}\n" +
            $"Total Coins Earned: {GM.Player.Stats.Coins}\n\n" +
            "Every gym. Every leader. Every discipline.\n" +
            "You are the Calico Combat League Champion!";
    }
}
