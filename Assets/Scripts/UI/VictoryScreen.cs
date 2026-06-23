using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VictoryScreen : UIScreen
{
    readonly Text highlightText;
    readonly Text rewardText;
    readonly RectTransform victoryHeading;
    readonly RectTransform rewardCard;
    readonly CanvasGroup rewardGroup;
    readonly Image winnerSprite;
    readonly BattleFighterVisual winnerVisual;

    public VictoryScreen(Transform parent, GameManager gm) : base(parent, gm, "VictoryScreen", "victory")
    {
        // Landscape Conversion: the winner now stands full-height in a left
        // column (much larger than before) instead of a small box squeezed
        // between stacked text - heading/reward/button form a right column.
        var winnerRoot = UIFactory.CreateBattleFighter(Root.transform, "Winner",
            new Vector2(0.03f, 0.08f), new Vector2(0.38f, 0.92f), out winnerSprite);
        winnerVisual = winnerRoot.gameObject.AddComponent<BattleFighterVisual>();

        UIFactory.CreateCaption(Root.transform, "CALICO COMBAT LEAGUE | OFFICIAL RESULT",
            new Vector2(0.42f, 0.86f), new Vector2(0.97f, 0.92f), TextAnchor.MiddleCenter);
        victoryHeading = UIFactory.CreateHeading(Root.transform, "VICTORY!", new Vector2(0.42f, 0.70f), new Vector2(0.97f, 0.85f)).rectTransform;

        highlightText = UIFactory.CreateText(Root.transform, "", UIFactory.SubheadingSize, UIFactory.GoldColor, TextAnchor.MiddleCenter,
            new Vector2(0.42f, 0.58f), new Vector2(0.97f, 0.68f), FontStyle.Bold);

        rewardCard = UIFactory.CreateCard(Root.transform, "Reward", new Vector2(0.42f, 0.26f), new Vector2(0.97f, 0.55f));
        rewardGroup = rewardCard.gameObject.AddComponent<CanvasGroup>();
        rewardText = UIFactory.CreateText(rewardCard, "", UIFactory.BodySize, UIFactory.CreamColor, TextAnchor.MiddleCenter,
            new Vector2(0.03f, 0.03f), new Vector2(0.97f, 0.97f));

        UIFactory.CreateButton(Root.transform, "RETURN TO MAP", new Vector2(0.50f, 0.09f), new Vector2(0.89f, 0.21f),
            () => GM.ReturnToMap(), UIFactory.PositiveColor);
    }

    public void Refresh()
    {
        bool gymCleared = GM.LastVictoryUnlockedGym;
        bool moveUnlocked = !string.IsNullOrEmpty(GM.LastUnlockedMoveName);
        bool shadowVictory = GM.CurrentOpponentInfo?.OpponentId == GameManager.ShadowChampionId;

        if (shadowVictory) AudioManager.Instance?.PlayChampionVictory();
        else if (gymCleared) AudioManager.Instance?.PlayGymCleared();
        else AudioManager.Instance?.PlayVictory();

        PlayCelebration(shadowVictory ? 30 : (gymCleared ? 24 : 16));
        var headingText = victoryHeading.GetComponent<Text>();
        headingText.text = shadowVictory ? "YOU DEFEATED YOUR SHADOW" : "VICTORY!";
        PlayPulse(victoryHeading, shadowVictory ? 1.2f : (gymCleared ? 1.12f : 1.08f), shadowVictory ? 0.65f : 0.55f);
        PlayReveal(rewardGroup, rewardCard, 0.22f, 0.38f);

        highlightText.text = "";
        rewardText.text = "";

        if (GM.Player == null)
        {
            rewardText.text = "Victory!";
            return;
        }

        Color theme = IconFactory.GetArchetypeThemeColor(GM.Player.Archetype);
        winnerVisual.Initialize(winnerSprite, "player", GM.Player.Archetype, theme, faceRight: true);
        winnerVisual.PlayVictoryPose(champion: GM.HasBecomeChampion(), leader: false);

        RunAnimation(RewardRevealRoutine(moveUnlocked, gymCleared, shadowVictory, GM.TotalWins));
    }

    // Part 5 (Fight Night presentation): rewards tally one at a time instead of
    // appearing as a single static block, with the move/gym-clear highlights
    // landing last as the "big" beats. Reuses only the existing PlayPulse helper.
    // Milestone 26: a defeated Shadow Champion adds one more beat - the unique
    // title reveal - after the opponent's own parting line.
    IEnumerator RewardRevealRoutine(bool moveUnlocked, bool gymCleared, bool shadowVictory, int totalWins)
    {
        yield return new WaitForSecondsRealtime(0.3f);
        AppendRewardLine($"+{GM.LastRewardXP} XP");

        yield return new WaitForSecondsRealtime(0.24f);
        AppendRewardLine($"+{GM.LastRewardCoins} Coins");

        yield return new WaitForSecondsRealtime(0.24f);
        AppendRewardLine($"Level {GM.Player.Stats.Level}");

        if (moveUnlocked)
        {
            yield return new WaitForSecondsRealtime(0.3f);
            AppendHighlightLine($"NEW MOVE: {GM.LastUnlockedMoveName}!");
        }

        if (gymCleared)
        {
            yield return new WaitForSecondsRealtime(0.3f);
            AppendHighlightLine("GYM CLEARED!");
        }

        // Milestone 22: the opponent gets the last word - their reaction to losing.
        string lossLine = GM.CurrentOpponentInfo?.LossLine;
        if (!string.IsNullOrEmpty(lossLine))
        {
            yield return new WaitForSecondsRealtime(0.3f);
            AppendHighlightLine($"\"{lossLine}\" - {GM.CurrentOpponent.Name}");
        }

        if (shadowVictory)
        {
            yield return new WaitForSecondsRealtime(0.4f);
            AppendHighlightLine("TITLE EARNED: \"SHADOW SLAYER\"");
        }

        // Milestone 29, Part 3/5/6: the rival reacts to a gym checkpoint, or
        // occasionally chimes in to keep building anticipation. Never both -
        // a checkpoint clear is already the bigger beat for that fight.
        if (!shadowVictory)
        {
            string rivalLine = gymCleared ? RivalDatabase.GetGymClearedLine(GM.CurrentGym?.GymType ?? GymType.Boxing) : null;
            rivalLine ??= RivalDatabase.GetOccasionalVictoryLine(totalWins);
            if (!string.IsNullOrEmpty(rivalLine))
            {
                yield return new WaitForSecondsRealtime(0.35f);
                AppendHighlightLine($"{RivalDatabase.RivalName}: \"{rivalLine}\"");
            }
        }
    }

    void AppendRewardLine(string line)
    {
        rewardText.text = string.IsNullOrEmpty(rewardText.text) ? line : rewardText.text + "\n" + line;
        PlayPulse(rewardText.rectTransform, 1.06f, 0.22f);
    }

    void AppendHighlightLine(string line)
    {
        highlightText.text = string.IsNullOrEmpty(highlightText.text) ? line : highlightText.text + "\n" + line;
        PlayPulse(highlightText.rectTransform, 1.15f, 0.4f);
    }
}
