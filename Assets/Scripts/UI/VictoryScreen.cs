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

    // Milestone 59 (Mirror Match Reveal Moment). Heading/body are assigned
    // inside BuildMirrorMatchRevealPanel(), not the constructor body itself,
    // so they can't be readonly.
    readonly RectTransform mirrorMatchRevealPanel;
    Text mirrorMatchRevealHeading;
    Text mirrorMatchRevealBody;
    bool mirrorMatchRevealAdvanceRequested;

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
        // opponent's parting quote), and AtkinsonHyperlegible-Bold's glyphs
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

        // Milestone 50, Part 5/6: was PositiveColor (green) - a navigation
        // action, not a value comparison/reward; the actual reward values
        // above it already use Green where it belongs.
        UIFactory.CreateButton(Root.transform, "RETURN TO MAP", new Vector2(0.50f, 0.09f), new Vector2(0.89f, 0.21f),
            () => GM.ReturnToMap(), UIFactory.AccentOrange);

        // Milestone 33, Part 3/4: real dialogue-box rival encounters (gym
        // checkpoints, surprise intercepts) instead of the old single inline
        // text line - reuses the exact popup GymMapScreen's first-appearance
        // greeting already uses.
        rivalDialogue = UIFactory.CreateRivalDialogue(Root.transform);

        mirrorMatchRevealPanel = BuildMirrorMatchRevealPanel();
    }

    // Milestone 59, Part 3/4: same toggle-shown-panel approach as Milestones
    // 57/58's reveals, but deliberately toned down per Part 4 - cool
    // blue-grey instead of Championship Gold or Rival Purple, matching Mirror
    // Match's existing "quiet, strange, reflective" identity everywhere else
    // it appears (Gym Selection's banner, Battle's stage tint). Heading still
    // uses the headline font (HeadingSize); body uses the normal body font.
    RectTransform BuildMirrorMatchRevealPanel()
    {
        Color mirrorTone = new Color(0.55f, 0.6f, 0.68f, 1f);
        var panel = UIFactory.CreateCard(Root.transform, "MirrorMatchReveal", new Vector2(0.1f, 0.15f), new Vector2(0.9f, 0.85f),
            new Color(0.07f, 0.08f, 0.1f, 0.99f));

        mirrorMatchRevealHeading = UIFactory.CreateText(panel, "", UIFactory.HeadingSize, mirrorTone,
            TextAnchor.MiddleCenter, new Vector2(0.04f, 0.66f), new Vector2(0.96f, 0.92f), FontStyle.Bold);
        mirrorMatchRevealHeading.resizeTextForBestFit = true;
        mirrorMatchRevealHeading.resizeTextMinSize = 24;
        mirrorMatchRevealHeading.resizeTextMaxSize = UIFactory.HeadingSize;
        mirrorMatchRevealHeading.raycastTarget = false;

        mirrorMatchRevealBody = UIFactory.CreateText(panel, "", UIFactory.BodySize, UIFactory.CreamColor,
            TextAnchor.MiddleCenter, new Vector2(0.06f, 0.14f), new Vector2(0.94f, 0.64f));
        mirrorMatchRevealBody.resizeTextForBestFit = true;
        mirrorMatchRevealBody.resizeTextMinSize = 14;
        mirrorMatchRevealBody.resizeTextMaxSize = UIFactory.BodySize;
        mirrorMatchRevealBody.raycastTarget = false;

        var tapPrompt = UIFactory.CreateCaption(panel, "Tap to continue", new Vector2(0.06f, 0.02f), new Vector2(0.94f, 0.11f), TextAnchor.MiddleCenter);
        tapPrompt.raycastTarget = false;

        var tapButton = panel.gameObject.AddComponent<Button>();
        tapButton.transition = Selectable.Transition.None;
        tapButton.targetGraphic = panel.GetComponent<Image>();
        tapButton.onClick.AddListener(() => mirrorMatchRevealAdvanceRequested = true);

        panel.gameObject.SetActive(false);
        return panel;
    }

    public void Refresh()
    {
        // Milestone 59: defensive, same reasoning as the confirm/reveal
        // panel guards in ProfileScreen/ChampionshipScreen - guards against
        // ever re-entering this screen with the reveal still showing.
        mirrorMatchRevealPanel.gameObject.SetActive(false);

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
        // Milestone 46, Part 4: large character display - Victory Screen.
        UIFactory.ApplyPrestigeTattoo(winnerSprite, GM.PrestigeLevel);

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

        // Overnight Audit (Reward/Randomness): the occasional Street Fight
        // "lucky break" bonus GameManager.EndBattle rolled - 0 most fights,
        // so this line simply doesn't appear unless it actually hit.
        if (GM.LastLuckyBreakBonus > 0)
        {
            yield return new WaitForSecondsRealtime(0.2f);
            AppendHighlightLine($"LUCKY BREAK! +{GM.LastLuckyBreakBonus} Bonus Coins");
        }

        yield return new WaitForSecondsRealtime(0.24f);
        AppendRewardLine($"Level {GM.Player.Stats.Level}");

        // Milestone 45, Part 6: shown on every victory, not just Prestige-
        // related ones - consistent display wherever Prestige appears.
        yield return new WaitForSecondsRealtime(0.18f);
        AppendRewardLine(PrestigeSystem.FormatLevel(GM.PrestigeLevel));
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

        // Milestone 50, Part 1/2/3 (Record Celebrations): surfaces the
        // personal bests GameManager.EndBattle just detected, using the same
        // AppendHighlightLine/PlayPulse reveal every other big beat in this
        // routine already uses - no new popup system. Capped to the 2 most
        // important breaks so this can never spam alongside the highlight
        // beats below it, even on an exceptional fight.
        var brokenRecords = new System.Collections.Generic.List<(string Label, string Detail)>();
        if (GM.LastFightNewWinStreakRecord) brokenRecords.Add(("WIN STREAK EXTENDED", $"New Best Win Streak: {GM.BestWinStreak}"));
        if (GM.LastFightNewComboRecord) brokenRecords.Add(("COMBO MASTERCLASS", $"New Personal Record: {GM.MostCombosInOneFight} Combos In One Fight"));
        if (GM.LastFightNewCritRecord) brokenRecords.Add(("CRITICAL HIT SHOWCASE", $"New Personal Record: {GM.MostCriticalHitsInOneFight} Critical Hits In One Fight"));
        if (GM.LastStreetFightMilestone > 0) brokenRecords.Add(("CAREER MILESTONE", $"{GM.LastStreetFightMilestone} Street Fights Won!"));

        if (brokenRecords.Count > 0)
        {
            yield return new WaitForSecondsRealtime(0.25f);
            AudioManager.Instance?.PlayLevelUp();
            // Multiple records in the same fight is the rare, exceptional
            // case - call it out with its own headline rather than just
            // letting the individual lines pile up.
            if (brokenRecords.Count > 1) AppendHighlightLine("FIGHT OF THE NIGHT");

            int recordsShown = Mathf.Min(brokenRecords.Count, 2);
            for (int i = 0; i < recordsShown; i++)
            {
                yield return new WaitForSecondsRealtime(0.22f);
                AppendHighlightLine($"RECORD BROKEN: {brokenRecords[i].Label}\n{brokenRecords[i].Detail}");
            }
        }

        // Milestone 56, Part 6 (Career Highlight Celebrations): only the two
        // highlight types that have no existing equivalent beat - Milestone
        // 50's "CAREER MILESTONE" line above already covers Street Fight win
        // thresholds (including 50), so it isn't duplicated here. Rival/
        // Mirror Match defeats already get their own "TITLE EARNED" beats
        // further down, so those aren't duplicated either - see Part 6 of
        // the report for the full reasoning.
        if (GM.LastFightFirstGymCleared)
        {
            yield return new WaitForSecondsRealtime(0.25f);
            AudioManager.Instance?.PlayLevelUp();
            AppendHighlightLine("CAREER HIGHLIGHT\nFIRST GYM CLEARED");
        }
        if (GM.LastFightHit100Combos)
        {
            yield return new WaitForSecondsRealtime(0.25f);
            AudioManager.Instance?.PlayLevelUp();
            AppendHighlightLine("CAREER HIGHLIGHT\n100 COMBOS");
        }

        if (moveUnlocked)
        {
            yield return new WaitForSecondsRealtime(0.3f);
            AppendHighlightLine($"NEW MOVE: {GM.LastUnlockedMoveName}!");
        }

        if (gymCleared)
        {
            yield return new WaitForSecondsRealtime(0.3f);
            // Milestone 52, Part 1/4: the gym name + "CLEARED!" as the major
            // beat (AppendHighlightLine - Gold, MMA Champ, same as every
            // other big highlight here), followed by the gym's lesson line
            // as a quieter follow-up (AppendRewardLine - Cream,
            // AtkinsonHyperlegible, same as every other reward stat line).
            // Reuses both existing reveal patterns; no new UI. Championship's
            // GymInfo deliberately has no LessonText (it has its own screen
            // and beat already), so this naturally only ever fires for the
            // 4 regular gyms.
            AppendHighlightLine($"{GM.CurrentGym?.GymName.ToUpperInvariant()} CLEARED!");
            if (!string.IsNullOrEmpty(GM.CurrentGym?.LessonText))
            {
                yield return new WaitForSecondsRealtime(0.22f);
                AppendRewardLine(GM.CurrentGym.LessonText);
            }
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

        // Milestone 59, Part 2/7: TotalGameCompletions only increments inside
        // RecordShadowChampionVictory (GameManager.EndBattle), which only
        // runs once per genuinely new defeat - ==1 means this is the very
        // first Mirror Match win ever, across every Prestige cycle. The big
        // reveal below replaces (not duplicates) the "TRUE CHAMPION"/"TITLE
        // EARNED" lines for that one case; repeat victories keep those exact
        // original lines, unchanged.
        bool firstMirrorMatch = shadowVictory && GM.TotalGameCompletions == 1;

        // Milestone 44, Reward: the brief's own reward heading, shown as a
        // highlight beat the same way Rival's payoff lines are.
        if (shadowVictory && !firstMirrorMatch)
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

        if (shadowVictory && !firstMirrorMatch)
        {
            yield return new WaitForSecondsRealtime(0.4f);
            AppendHighlightLine("TITLE EARNED: \"TRUE CHAMPION\"");
        }

        // Milestone 59, Part 1/2/6: the full first-time-only reveal - shown
        // after the opponent's parting line (so it lands as the final,
        // quiet word) instead of replacing it. Tap-to-skip or a capped wait
        // (Part 3: target 3-5s), same as Milestones 57/58's reveals.
        if (firstMirrorMatch)
        {
            yield return new WaitForSecondsRealtime(0.4f);
            ShowMirrorMatchReveal();
            float elapsed = 0f;
            const float maxDuration = 4.5f;
            while (!mirrorMatchRevealAdvanceRequested && elapsed < maxDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            mirrorMatchRevealPanel.gameObject.SetActive(false);
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

    // Milestone 59, Part 1/4/5/6: the quiet, reflective first-time-only
    // reveal - title, the brief's own reflective lines, and a Hall of
    // Champions callout, all concise. PlayLevelUp reuses this game's
    // established "personal achievement" cue (Milestones 50/56/57/58)
    // rather than the louder PlayChampionVictory already played earlier in
    // Refresh() for every shadowVictory (repeat included).
    void ShowMirrorMatchReveal()
    {
        mirrorMatchRevealHeading.text = "TRUE CHAMPION\nYOU DEFEATED YOURSELF";
        mirrorMatchRevealBody.text =
            "You overcame every rival.\n" +
            "You mastered every lesson.\n" +
            "The final opponent was you.\n\n" +
            "HALL OF CHAMPIONS ENTRY RECORDED";

        mirrorMatchRevealAdvanceRequested = false;
        mirrorMatchRevealPanel.gameObject.SetActive(true);
        mirrorMatchRevealPanel.SetAsLastSibling();
        PlayPulse(mirrorMatchRevealPanel, 1.04f, 0.6f);
        AudioManager.Instance?.PlayLevelUp();
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
