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
    readonly RivalDialogueBox rivalDialogue;

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
        // Quick Fix (Font Replacement Pass), Part 5: this box can accumulate
        // several appended lines (gym-cleared/unlock callouts plus the
        // opponent's parting quote), and PatrickHandSC-Regular's wider glyphs
        // raise the odds of overflowing this fairly tight box.
        highlightText.resizeTextForBestFit = true;
        highlightText.resizeTextMinSize = 14;
        highlightText.resizeTextMaxSize = UIFactory.SubheadingSize;

        rewardCard = UIFactory.CreateCard(Root.transform, "Reward", new Vector2(0.42f, 0.26f), new Vector2(0.97f, 0.55f));
        rewardGroup = rewardCard.gameObject.AddComponent<CanvasGroup>();
        rewardText = UIFactory.CreateText(rewardCard, "", UIFactory.BodySize, UIFactory.CreamColor, TextAnchor.MiddleCenter,
            new Vector2(0.03f, 0.03f), new Vector2(0.97f, 0.97f));
        // Typography pass: this accumulates several reward stat lines
        // (XP/Coins/Level/Turns/Combo) with no safety net previously - adding
        // the same best-fit protection every other accumulator text already has.
        rewardText.resizeTextForBestFit = true;
        rewardText.resizeTextMinSize = 14;
        rewardText.resizeTextMaxSize = UIFactory.BodySize;

        UIFactory.CreateButton(Root.transform, "RETURN TO MAP", new Vector2(0.50f, 0.09f), new Vector2(0.89f, 0.21f),
            () => GM.ReturnToMap(), UIFactory.PositiveColor);

        // Milestone 33, Part 3/4: real dialogue-box rival encounters (gym
        // checkpoints, surprise intercepts) instead of the old single inline
        // text line - reuses the exact popup GymMapScreen's first-appearance
        // greeting already uses.
        rivalDialogue = UIFactory.CreateRivalDialogue(Root.transform);
    }

    public void Refresh()
    {
        bool gymCleared = GM.LastVictoryUnlockedGym;
        bool moveUnlocked = !string.IsNullOrEmpty(GM.LastUnlockedMoveName);
        bool shadowVictory = GM.CurrentOpponentInfo?.OpponentId == GameManager.ShadowChampionId;
        // Milestone 34, Part 8: the Rival Showdown payoff.
        bool rivalVictory = GM.CurrentOpponentInfo?.OpponentId == GameManager.RivalFightOpponentId;

        // Milestone 35, Part 7: rival_victory.mp3 is now its own dedicated cue,
        // distinct from the Championship sound shadowVictory still uses.
        if (shadowVictory) AudioManager.Instance?.PlayChampionVictory();
        else if (rivalVictory) AudioManager.Instance?.PlayRivalVictory();
        else if (gymCleared) AudioManager.Instance?.PlayGymCleared();
        else AudioManager.Instance?.PlayVictory();

        // Milestone 44: Mirror Match's reward gets deliberately LESS
        // celebration than Rival's, not more - "quiet, strange, reflective"
        // instead of loud, per the brief's contrast between the two.
        PlayCelebration(shadowVictory ? 14 : (rivalVictory ? 26 : (gymCleared ? 24 : 16)));
        var headingText = victoryHeading.GetComponent<Text>();
        headingText.text = shadowVictory ? "YOU DEFEATED YOURSELF" : rivalVictory ? "RIVAL DEFEATED" : "VICTORY!";
        PlayPulse(victoryHeading, shadowVictory ? 1.1f : rivalVictory ? 1.18f : (gymCleared ? 1.12f : 1.08f), shadowVictory ? 0.5f : 0.55f);
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

        RunAnimation(RewardRevealRoutine(moveUnlocked, gymCleared, shadowVictory, rivalVictory, GM.TotalWins));
    }

    // Part 5 (Fight Night presentation): rewards tally one at a time instead of
    // appearing as a single static block, with the move/gym-clear highlights
    // landing last as the "big" beats. Reuses only the existing PlayPulse helper.
    // Milestone 26: a defeated Shadow Champion adds one more beat - the unique
    // title reveal - after the opponent's own parting line.
    IEnumerator RewardRevealRoutine(bool moveUnlocked, bool gymCleared, bool shadowVictory, bool rivalVictory, int totalWins)
    {
        yield return new WaitForSecondsRealtime(0.3f);
        AppendRewardLine($"+{GM.LastRewardXP} XP");

        yield return new WaitForSecondsRealtime(0.24f);
        AppendRewardLine($"+{GM.LastRewardCoins} Coins");

        yield return new WaitForSecondsRealtime(0.24f);
        AppendRewardLine($"Level {GM.Player.Stats.Level}");
        // Milestone 35, Part 5: GM.LastVictoryLeveledUpTo (added in Milestone
        // 34) is exactly "0 unless this fight's XP just crossed a level
        // boundary" - the right signal for a one-time level-up cue, since this
        // reward line itself shows the current level on every victory regardless.
        if (GM.LastVictoryLeveledUpTo > 0) AudioManager.Instance?.PlayLevelUp();

        // Milestone 32, Part 7: presentation-only stats handed off by
        // BattleScreen right before EndBattle - reuses the existing reward
        // tally pacing/animation, no new reward system.
        if (GM.LastFightTurnCount > 0)
        {
            yield return new WaitForSecondsRealtime(0.2f);
            AppendRewardLine($"Turns Survived: {GM.LastFightTurnCount}");
        }

        if (!string.IsNullOrEmpty(GM.LastComboUsed))
        {
            yield return new WaitForSecondsRealtime(0.2f);
            AppendRewardLine($"Combo Used: {GM.LastComboUsed}");
        }

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

        // Milestone 39, Part 7: the Rival Showdown is now the storyline's
        // actual closure (fought after the Championship, not a gate before
        // it) - real "you made it" payoff messaging instead of the old
        // "unlocked the next thing" framing.
        if (rivalVictory)
        {
            yield return new WaitForSecondsRealtime(0.3f);
            AppendHighlightLine("YOU FINALLY SURPASSED SCRATCH");
            yield return new WaitForSecondsRealtime(0.2f);
            AppendHighlightLine("THE BEST IN THE LEAGUE");
        }

        // Milestone 44, Reward: the brief's own reward heading, shown as a
        // highlight beat the same way Rival's payoff lines are.
        if (shadowVictory)
        {
            yield return new WaitForSecondsRealtime(0.3f);
            AppendHighlightLine("TRUE CHAMPION");
        }

        // Milestone 22: the opponent gets the last word - their reaction to losing.
        string lossLine = GM.CurrentOpponentInfo?.LossLine;
        if (!string.IsNullOrEmpty(lossLine) && !rivalVictory)
        {
            yield return new WaitForSecondsRealtime(0.3f);
            AppendHighlightLine($"\"{lossLine}\" - {GM.CurrentOpponent.Name}");
        }

        if (shadowVictory)
        {
            yield return new WaitForSecondsRealtime(0.4f);
            AppendHighlightLine("TITLE EARNED: \"TRUE CHAMPION\"");
        }

        // Milestone 39, Part 9: mirrors the Shadow Slayer reveal above - the
        // Hall of Champions title GameManager.RecordRivalVictoryLegacy just added.
        if (rivalVictory)
        {
            yield return new WaitForSecondsRealtime(0.4f);
            AppendHighlightLine("TITLE EARNED: \"RIVAL CONQUEROR\"");
        }

        // Milestone 33/34, Part 3/4/8: the rival reacts to defeating HIM (the
        // biggest beat) or, failing that, a gym checkpoint, or a surprise
        // intercept, or - failing all of those - just an occasional ambient
        // one-liner. Priority order, never more than one.
        if (rivalVictory)
        {
            yield return new WaitForSecondsRealtime(0.4f);
            rivalDialogue.Show(RivalDatabase.RivalName, RivalDatabase.RivalDefeatedLines);
        }
        else if (!shadowVictory)
        {
            string[] interceptLines = null;

            if (gymCleared)
            {
                interceptLines = RivalDatabase.GetGymClearedLines(GM.CurrentGym?.GymType ?? GymType.Boxing);
            }
            else if (IsStreetFight() && GM.CurrentStreetFightOpponent != null &&
                (GM.CurrentStreetFightOpponent.Difficulty == StreetFightDifficulty.Hard ||
                 GM.CurrentStreetFightOpponent.Difficulty == StreetFightDifficulty.Dangerous))
            {
                interceptLines = RivalDatabase.GetStreetFightInterceptLines();
            }
            else if (GM.LastVictoryLeveledUpTo > 0 && GM.LastVictoryLeveledUpTo % 5 == 0)
            {
                interceptLines = RivalDatabase.GetLevelMilestoneInterceptLines(GM.LastVictoryLeveledUpTo);
            }

            if (interceptLines != null)
            {
                yield return new WaitForSecondsRealtime(0.4f);
                rivalDialogue.Show(RivalDatabase.RivalName, interceptLines);
            }
            else
            {
                string occasionalLine = RivalDatabase.GetOccasionalVictoryLine(totalWins);
                if (!string.IsNullOrEmpty(occasionalLine))
                {
                    yield return new WaitForSecondsRealtime(0.35f);
                    AppendHighlightLine($"{RivalDatabase.RivalName}: \"{occasionalLine}\"");
                }
            }
        }
    }

    bool IsStreetFight() => GM.CurrentGym?.GymId == "street_fight";

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
