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

    // Milestone 58 (First Championship Reveal Moment). Heading/body are
    // assigned inside BuildChampionshipRevealPanel(), not the constructor
    // body itself, so they can't be readonly.
    readonly RectTransform championshipRevealPanel;
    Text championshipRevealHeading;
    Text championshipRevealBody;
    bool championshipRevealAdvanceRequested;

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
        // Milestone 60 (Release Audit): this accumulates several lines
        // (name/archetype/level/wins, rival note, opponent's parting quote)
        // with no overflow safety net - same protection every other
        // accumulator text in this UI already has.
        infoText.resizeTextForBestFit = true;
        infoText.resizeTextMinSize = 14;
        infoText.resizeTextMaxSize = UIFactory.BodySize;

        // Milestone 50, Part 5/6: was PositiveColor (green) - a navigation
        // action, not a value comparison/reward.
        UIFactory.CreateButton(Root.transform, "CONTINUE", new Vector2(0.40f, 0.02f), new Vector2(0.97f, 0.09f),
            () => GM.ReturnToMap(), UIFactory.AccentOrange);

        // Milestone 39, Part 1: the storyline's big reveal - Scratch
        // interrupting what would otherwise read as "the end."
        rivalDialogue = UIFactory.CreateRivalDialogue(Root.transform);

        championshipRevealPanel = BuildChampionshipRevealPanel();
    }

    // Milestone 58, Part 2/8: same toggle-shown-panel approach as Milestone
    // 57's Prestige reveal - not a new screen, not a new system. Centered
    // card, Gold/MMA-Champ heading, Cream body, tap-anywhere-to-skip (same
    // pattern BattleScreen's intro card and ProfileScreen's Prestige reveal
    // both already use).
    RectTransform BuildChampionshipRevealPanel()
    {
        var panel = UIFactory.CreateCard(Root.transform, "ChampionshipReveal", new Vector2(0.1f, 0.15f), new Vector2(0.9f, 0.85f),
            new Color(UIFactory.BackgroundColor.r, UIFactory.BackgroundColor.g, UIFactory.BackgroundColor.b, 0.99f));

        championshipRevealHeading = UIFactory.CreateText(panel, "", UIFactory.HeadingSize, UIFactory.GoldColor,
            TextAnchor.MiddleCenter, new Vector2(0.04f, 0.66f), new Vector2(0.96f, 0.92f), FontStyle.Bold);
        championshipRevealHeading.resizeTextForBestFit = true;
        championshipRevealHeading.resizeTextMinSize = 24;
        championshipRevealHeading.resizeTextMaxSize = UIFactory.HeadingSize;
        championshipRevealHeading.raycastTarget = false;

        championshipRevealBody = UIFactory.CreateText(panel, "", UIFactory.BodySize, UIFactory.CreamColor,
            TextAnchor.MiddleCenter, new Vector2(0.06f, 0.14f), new Vector2(0.94f, 0.64f));
        championshipRevealBody.resizeTextForBestFit = true;
        championshipRevealBody.resizeTextMinSize = 14;
        championshipRevealBody.resizeTextMaxSize = UIFactory.BodySize;
        championshipRevealBody.raycastTarget = false;

        var tapPrompt = UIFactory.CreateCaption(panel, "Tap to continue", new Vector2(0.06f, 0.02f), new Vector2(0.94f, 0.11f), TextAnchor.MiddleCenter);
        tapPrompt.raycastTarget = false;

        var tapButton = panel.gameObject.AddComponent<Button>();
        tapButton.transition = Selectable.Transition.None;
        tapButton.targetGraphic = panel.GetComponent<Image>();
        tapButton.onClick.AddListener(() => championshipRevealAdvanceRequested = true);

        panel.gameObject.SetActive(false);
        return panel;
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

        // Milestone 58, Part 3/7: ChampionshipWinCount==1 means the Hall of
        // Champions entry RecordChampionLegacy just added (before this
        // screen was reached) is the very first one ever, across every
        // Prestige cycle - same gate Milestone 56 used for its inline
        // "CAREER HIGHLIGHT" line, which this reveal now replaces rather
        // than duplicates (Part 7 - no duplicate notifications).
        bool firstChampionship = GM.ChampionshipWinCount == 1;
        if (firstChampionship)
        {
            ShowChampionshipReveal(archetypeName);
        }
        // Milestone 39, Part 1: the real multi-stage rival arrival event -
        // fires once per Prestige cycle's first championship win. Milestone
        // 58: for a first-ever championship, deferred until the new reveal
        // closes (see ChampionshipRevealRoutine) so the two beats don't
        // visually collide; repeat-cycle championships keep this exact
        // original immediate timing, unchanged.
        else if (!GM.HasDefeatedRival)
        {
            RunAnimation(ShowRivalArrivalDelayed());
        }
    }

    // Milestone 58, Part 1/4/6: the big "I became champion" moment - title,
    // fighter identity, and a Hall of Champions callout, all concise.
    void ShowChampionshipReveal(string archetypeName)
    {
        championshipRevealHeading.text = "UNDISPUTED CHAMPION\nCALICO COMBAT LEAGUE";
        championshipRevealBody.text =
            $"{GM.Player.Name}  -  {archetypeName}\n\n" +
            "You conquered every gym.\n" +
            "You defeated every leader.\n" +
            "You reached the top.\n\n" +
            "HALL OF CHAMPIONS ENTRY RECORDED";

        championshipRevealPanel.gameObject.SetActive(true);
        championshipRevealPanel.SetAsLastSibling();
        PlayPulse(championshipRevealPanel, 1.05f, 0.5f);
        // PlayChampionVictory() already fired at the top of Refresh() -
        // its timing already lines up with this reveal appearing, so it
        // isn't played a second time here.
        RunAnimation(ChampionshipRevealRoutine());
    }

    IEnumerator ChampionshipRevealRoutine()
    {
        championshipRevealAdvanceRequested = false;
        float elapsed = 0f;
        // Part 2: target 3-5s - capped at the top of that range, or sooner via tap.
        const float maxDuration = 4.5f;
        while (!championshipRevealAdvanceRequested && elapsed < maxDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        championshipRevealPanel.gameObject.SetActive(false);

        if (!GM.HasDefeatedRival)
            RunAnimation(ShowRivalArrivalDelayed());
    }

    IEnumerator ShowRivalArrivalDelayed()
    {
        yield return new WaitForSecondsRealtime(0.8f);
        rivalDialogue.Show(RivalDatabase.RivalName, RivalDatabase.ShowdownArrivalLines);
    }
}
