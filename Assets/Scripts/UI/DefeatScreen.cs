using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DefeatScreen : UIScreen
{
    readonly RectTransform defeatHeading;
    readonly RectTransform messageRect;
    readonly CanvasGroup messageGroup;
    readonly Image defeatedSprite;
    readonly BattleFighterVisual defeatedVisual;
    readonly Text opponentLineText;
    readonly Text rewardText;
    readonly Text tipText;
    readonly RivalDialogueBox rivalDialogue;

    // Milestone 32, Part 8: rotated by lifetime loss count so the same phrase
    // doesn't show every single time - no new save state, just an index into
    // a fixed array.
    static readonly string[] EncouragingHeadlines = { "BETTER LUCK NEXT TIME", "BACK TO TRAINING", "COME BACK STRONGER" };

    static readonly string[] Tips =
    {
        "Tip: Recover when your stamina runs low instead of forcing a heavy move.",
        "Tip: Cheap moves add up - try chaining Jab, Jab, Cross for a damage bonus.",
        "Tip: Street Fights are good practice and grinding before a gym leader."
    };

    public DefeatScreen(Transform parent, GameManager gm) : base(parent, gm, "DefeatScreen", "defeat")
    {
        // Landscape Conversion: defeated fighter full-height in a left column;
        // heading/messages/button form a right column.
        var fighterRoot = UIFactory.CreateBattleFighter(Root.transform, "DefeatedFighter",
            new Vector2(0.03f, 0.08f), new Vector2(0.38f, 0.92f), out defeatedSprite);
        defeatedVisual = fighterRoot.gameObject.AddComponent<BattleFighterVisual>();

        UIFactory.CreateCaption(Root.transform, "CALICO COMBAT LEAGUE | OFFICIAL RESULT",
            new Vector2(0.42f, 0.80f), new Vector2(0.97f, 0.86f), TextAnchor.MiddleCenter);
        defeatHeading = UIFactory.CreateText(Root.transform, "DEFEATED", UIFactory.HeadingSize, UIFactory.DangerColor, TextAnchor.MiddleCenter,
            new Vector2(0.42f, 0.64f), new Vector2(0.97f, 0.80f), FontStyle.Bold).rectTransform;

        var message = UIFactory.CreateBody(Root.transform, "Not this time. Heal up and try again.",
            new Vector2(0.42f, 0.50f), new Vector2(0.97f, 0.60f), TextAnchor.MiddleCenter);
        messageRect = message.rectTransform;
        messageGroup = message.gameObject.AddComponent<CanvasGroup>();

        tipText = UIFactory.CreateCaption(Root.transform, "",
            new Vector2(0.42f, 0.40f), new Vector2(0.97f, 0.49f), TextAnchor.MiddleCenter);

        // Milestone 22: the opponent's reaction to beating the player.
        opponentLineText = UIFactory.CreateCaption(Root.transform, "", new Vector2(0.42f, 0.30f), new Vector2(0.97f, 0.38f), TextAnchor.MiddleCenter);
        opponentLineText.color = UIFactory.GoldColor;
        // Typography pass: 8pt was the smallest text anywhere in the app -
        // below comfortable reading size for an actual sentence of dialogue.
        // Raised to a still-protective but legible floor; the WinLine quote
        // itself stays as-is (no content changes).
        opponentLineText.resizeTextMinSize = 12;

        // Milestone 32, Part 8: XP/coins earned, if any - currently a defeat
        // always grants 0 of both, so this stays hidden in practice, but it's
        // correct and ready if a future milestone ever adds a consolation reward.
        rewardText = UIFactory.CreateCaption(Root.transform, "", new Vector2(0.42f, 0.23f), new Vector2(0.97f, 0.29f), TextAnchor.MiddleCenter);
        rewardText.color = UIFactory.MutedTextColor;

        UIFactory.CreateButton(Root.transform, "RETURN TO MAP", new Vector2(0.50f, 0.10f), new Vector2(0.89f, 0.22f),
            () => GM.ReturnToMap(), UIFactory.SecondaryColor);

        // Milestone 34, Part 9: the Rival Showdown's defeat payoff - does not
        // softlock anything, the Rival Showdown row simply stays available to
        // retry from Gym Selection.
        rivalDialogue = UIFactory.CreateRivalDialogue(Root.transform);
    }

    public void Refresh()
    {
        AudioManager.Instance?.PlayDefeat();
        PlayExhausted(defeatHeading);
        PlayReveal(messageGroup, messageRect, 0.2f, 0.35f);

        // Milestone 32, Part 8: rotate the headline so a loss reads as
        // encouraging rather than just "DEFEATED" every time.
        int variant = Mathf.Abs(GM.TotalLosses - 1) % EncouragingHeadlines.Length;
        defeatHeading.GetComponent<Text>().text = EncouragingHeadlines[variant];

        string tip = Tips[Mathf.Abs(GM.TotalLosses) % Tips.Length];
        tipText.text = $"No penalty - your fighter is fully rested back at the map.\n{tip}";

        if (GM.LastRewardXP > 0 || GM.LastRewardCoins > 0)
            rewardText.text = $"Earned: +{GM.LastRewardXP} XP, +{GM.LastRewardCoins} Coins";
        else
            rewardText.text = "";

        bool rivalDefeat = GM.CurrentOpponentInfo?.OpponentId == GameManager.RivalFightOpponentId;

        string winLine = GM.CurrentOpponentInfo?.WinLine;
        opponentLineText.text = !rivalDefeat && !string.IsNullOrEmpty(winLine) && GM.CurrentOpponent != null
            ? $"\"{winLine}\" - {GM.CurrentOpponent.Name}"
            : "";

        if (rivalDefeat)
            RunAnimation(ShowRivalVictoryDelayed());

        if (GM.Player != null)
        {
            Color theme = IconFactory.GetArchetypeThemeColor(GM.Player.Archetype);
            defeatedVisual.Initialize(defeatedSprite, "player", GM.Player.Archetype, theme, faceRight: true);
            defeatedVisual.PlayDefeatPose();
        }
    }

    // Milestone 34, Part 9: a short pause after the screen's own reveal
    // finishes, same pattern every other rival dialogue trigger uses.
    IEnumerator ShowRivalVictoryDelayed()
    {
        yield return new WaitForSecondsRealtime(0.4f);
        rivalDialogue.Show(RivalDatabase.RivalName, RivalDatabase.RivalVictoryLines);
    }
}
