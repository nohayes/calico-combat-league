using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChampionshipScreen : UIScreen
{
    readonly RectTransform championPortraitFrame;
    readonly Image championPortrait;
    readonly Text infoText;
    readonly RectTransform championHeading;
    readonly RectTransform infoCard;
    readonly CanvasGroup infoGroup;
    readonly BattleFighterVisual championVisual;
    readonly RivalDialogueBox rivalDialogue;

    public ChampionshipScreen(Transform parent, GameManager gm) : base(parent, gm, "ChampionshipScreen", "championship")
    {
        // Landscape Conversion: champion portrait moves out into its own
        // full-height left column (much larger), with the info card as a
        // matching right column - compact badge/heading stay centered up top.
        UIFactory.CreateChampionBadge(Root.transform, new Vector2(0.36f, 0.80f), new Vector2(0.64f, 0.99f));

        UIFactory.CreateCaption(Root.transform, "CALICO COMBAT LEAGUE | OFFICIAL CHAMPIONSHIP RESULT",
            new Vector2(0.1f, 0.75f), new Vector2(0.9f, 0.80f), TextAnchor.MiddleCenter);

        championHeading = UIFactory.CreateText(Root.transform, "UNDISPUTED CHAMPION", UIFactory.HeadingSize, UIFactory.GoldColor, TextAnchor.MiddleCenter,
            new Vector2(0.1f, 0.60f), new Vector2(0.9f, 0.75f), FontStyle.Bold).rectTransform;

        championPortraitFrame = UIFactory.CreateCard(Root.transform, "ChampionPortrait", new Vector2(0.03f, 0.10f), new Vector2(0.37f, 0.58f), UIFactory.BackgroundColor);

        var portraitGo = new GameObject("Portrait", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        portraitGo.transform.SetParent(championPortraitFrame, false);
        var portraitRt = portraitGo.GetComponent<RectTransform>();
        portraitRt.anchorMin = new Vector2(0.1f, 0.05f);
        portraitRt.anchorMax = new Vector2(0.9f, 0.95f);
        portraitRt.offsetMin = Vector2.zero;
        portraitRt.offsetMax = Vector2.zero;
        championPortrait = portraitGo.GetComponent<Image>();
        championPortrait.preserveAspect = true;
        championVisual = championPortraitFrame.gameObject.AddComponent<BattleFighterVisual>();

        infoCard = UIFactory.CreateCard(Root.transform, "ChampionInfo", new Vector2(0.40f, 0.10f), new Vector2(0.97f, 0.58f));
        infoGroup = infoCard.gameObject.AddComponent<CanvasGroup>();
        infoText = UIFactory.CreateText(infoCard, "", UIFactory.BodySize, UIFactory.CreamColor, TextAnchor.MiddleCenter,
            new Vector2(0.04f, 0.04f), new Vector2(0.97f, 0.96f));

        UIFactory.CreateButton(Root.transform, "CONTINUE", new Vector2(0.40f, 0.02f), new Vector2(0.97f, 0.09f),
            () => GM.ReturnToMap(), UIFactory.PositiveColor);

        // Milestone 39, Part 1: the storyline's big reveal - Scratch
        // interrupting what would otherwise read as "the end."
        rivalDialogue = UIFactory.CreateRivalDialogue(Root.transform);
    }

    public void Refresh()
    {
        AudioManager.Instance?.PlayChampionVictory();
        PlayCelebration(32);
        PlayPulse(championHeading, 1.13f, 0.7f);
        PlayReveal(infoGroup, infoCard, 0.28f, 0.45f);

        if (GM.Player == null)
        {
            infoText.text = "Champion!";
            return;
        }

        var info = ArchetypeDatabase.GetByType(GM.Player.Archetype);
        string archetypeName = info != null ? info.DisplayName : "Fighter";
        Color theme = IconFactory.GetArchetypeThemeColor(GM.Player.Archetype);
        championVisual.Initialize(championPortrait, "player", GM.Player.Archetype, theme, faceRight: true);
        championVisual.PlayVictoryPose(champion: true, leader: false);
        UIFactory.AddDisciplineBadge(championPortraitFrame, GM.Player.Archetype, theme);

        infoText.text =
            $"{GM.Player.Name}\n{archetypeName}\n" +
            $"Final Level: {GM.Player.Stats.Level}\n" +
            $"Total Wins: {GM.TotalWins}\n" +
            "Every gym. Every leader. Every discipline.\n" +
            "You are the Calico Combat League Champion!";

        // Milestone 22: the dethroned champion's special parting line.
        string lossLine = GM.CurrentOpponentInfo?.LossLine;
        if (!string.IsNullOrEmpty(lossLine) && GM.CurrentOpponent != null)
            infoText.text += $"\n\n\"{lossLine}\" - {GM.CurrentOpponent.Name}";

        // Milestone 33, Part 5: world-presence line - the rival is already
        // waiting, reinforcing the showdown tease right after it.
        infoText.text += $"\n\nWord is {RivalDatabase.RivalName} already qualified for the Finals.";

        // Milestone 39, Part 1: replaces the old static one-line tease
        // (GetShowdownLine) with the real multi-stage arrival event - fires
        // once, since this screen itself is only ever reached once per
        // playthrough (becoming champion a second time routes to Victory,
        // not here). Only relevant if the rival fight hasn't happened yet -
        // a save that already defeated Scratch before reaching this screen
        // for the first time (see GameManager's save-compatibility notes)
        // skips the tease and goes straight to "you're the champion."
        if (!GM.HasDefeatedRival)
            RunAnimation(ShowRivalArrivalDelayed());
    }

    IEnumerator ShowRivalArrivalDelayed()
    {
        yield return new WaitForSecondsRealtime(0.8f);
        rivalDialogue.Show(RivalDatabase.RivalName, RivalDatabase.ShowdownArrivalLines);
    }
}
