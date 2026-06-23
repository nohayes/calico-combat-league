using UnityEngine;
using UnityEngine.UI;

// Milestone 30, Part 1/3: Street Fights are optional, randomized battles for
// grinding XP/coins between gym challenges - reached from the Home screen.
// Difficulty is deliberately never shown here (the player is taking a risk by
// choosing this option) - the reward range shown is the full possible spread
// across every difficulty tier, not a hint about the specific opponent rolled.
public class StreetFightScreen : UIScreen
{
    readonly Text opponentNameText;
    readonly Image opponentPortrait;
    readonly Text worldPresenceText;

    public StreetFightScreen(Transform parent, GameManager gm) : base(parent, gm, "StreetFightScreen", "gym_map")
    {
        UIFactory.CreateHeading(Root.transform, "STREET FIGHT", new Vector2(0.15f, 0.86f), new Vector2(0.85f, 0.97f));

        UIFactory.CreateCaption(Root.transform, "Random opponent. Random danger. Real rewards.",
            new Vector2(0.15f, 0.79f), new Vector2(0.85f, 0.85f), TextAnchor.MiddleCenter);

        var card = UIFactory.CreateCard(Root.transform, "StreetFightOpponent", new Vector2(0.28f, 0.34f), new Vector2(0.72f, 0.77f));

        var portraitGo = new GameObject("Portrait", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        portraitGo.transform.SetParent(card, false);
        var portraitRt = portraitGo.GetComponent<RectTransform>();
        portraitRt.anchorMin = new Vector2(0.36f, 0.4f);
        portraitRt.anchorMax = new Vector2(0.64f, 0.95f);
        portraitRt.offsetMin = Vector2.zero;
        portraitRt.offsetMax = Vector2.zero;
        opponentPortrait = portraitGo.GetComponent<Image>();
        opponentPortrait.preserveAspect = true;

        opponentNameText = UIFactory.CreateText(card, "", UIFactory.SubheadingSize, UIFactory.GoldColor, TextAnchor.MiddleCenter,
            new Vector2(0.04f, 0.26f), new Vector2(0.96f, 0.38f), FontStyle.Bold);

        var riskText = UIFactory.CreateCaption(card, "Their skill is unknown until the bell rings.\nCould be an easy night. Could be a war.",
            new Vector2(0.06f, 0.03f), new Vector2(0.94f, 0.24f), TextAnchor.MiddleCenter);
        riskText.color = UIFactory.MutedTextColor;

        var rewardRangeText = UIFactory.CreateCaption(Root.transform, "Possible rewards: 15-120+ XP, 8-65+ Coins - tougher fights pay more.",
            new Vector2(0.15f, 0.24f), new Vector2(0.85f, 0.32f), TextAnchor.MiddleCenter);
        rewardRangeText.color = UIFactory.MutedTextColor;

        worldPresenceText = UIFactory.CreateCaption(Root.transform, "",
            new Vector2(0.15f, 0.18f), new Vector2(0.85f, 0.24f), TextAnchor.MiddleCenter);

        UIFactory.CreateButton(Root.transform, "START FIGHT", new Vector2(0.42f, 0.10f), new Vector2(0.85f, 0.165f),
            OnStartFight, UIFactory.DangerColor);
        UIFactory.CreateButton(Root.transform, "FIND ANOTHER OPPONENT", new Vector2(0.15f, 0.10f), new Vector2(0.40f, 0.165f),
            OnReroll, UIFactory.SecondaryColor);

        UIFactory.CreateButton(Root.transform, "BACK", new Vector2(0.40f, 0.02f), new Vector2(0.60f, 0.09f),
            () => GM.ChangeState(GameState.GymMap), UIFactory.SecondaryColor);
    }

    public void Refresh()
    {
        if (GM.Player == null) return;

        // Milestone 33, Part 5: world presence - folded into the existing tip
        // caption rather than adding a new element.
        worldPresenceText.text = $"Great for training before gym leaders.\nPeople are talking about {RivalDatabase.RivalName}.";

        Reroll();
    }

    void OnReroll()
    {
        AudioManager.Instance?.PlayClick();
        Reroll();
    }

    void Reroll()
    {
        GM.RollStreetFightOpponent();
        var rolled = GM.CurrentStreetFightOpponent;
        if (rolled?.Opponent == null) return;

        opponentNameText.text = rolled.Opponent.Name;
        Color theme = IconFactory.GetArchetypeThemeColor(rolled.PortraitArchetype);
        UIFactory.SetFighterPortrait(opponentPortrait, rolled.Opponent.OpponentId, rolled.PortraitArchetype, theme);
        PlayPulse(opponentNameText.rectTransform, 1.06f, 0.25f);
    }

    void OnStartFight()
    {
        GM.StartStreetFight();
    }
}
