using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Gym Selection Redesign: a centered 3x2 card grid (Boxing/Muay Thai/Wrestling
// on top, BJJ/Championship/Street Fight below) instead of the old stretched
// full-width row list. The old vertical "travel rail" avatar doesn't fit a
// grid and isn't part of the reference layout, so it's been removed; clicking
// a card now transitions directly instead of walking there first.
// Milestone 39: the Championship card is always a normal gym card now - the
// Rival Showdown (formerly slotted into that card while gating it) moved to
// its own banner below the grid, alongside the Shadow Gym's.
public class GymSelectionScreen : UIScreen
{
    readonly List<GameObject> dynamicEntries = new List<GameObject>();
    readonly Text rivalText;
    // Milestone 25, Part 5: ephemeral (not saved) tracking so a gym that just
    // became unlocked gets a one-time "reveal" pulse instead of silently
    // appearing as available - pure UI flourish, GameManager's unlock logic
    // is the single source of truth and is never touched.
    readonly HashSet<string> seenUnlockedGymIds = new HashSet<string>();

    // Milestone 33, Part 4: a brief rival intercept the first time a gym other
    // than the very first becomes newly unlocked. Set during the card-build
    // loop, consumed once at the end of Refresh() so it can't pop up mid-layout.
    readonly RivalDialogueBox rivalDialogue;
    GymInfo pendingInterceptGym;

    const int Columns = 3;
    const float GridXMin = 0.05f;
    const float GridXMax = 0.95f;
    // Milestone 54, Part 4/5: nudged down 0.025 from 0.78 to make room for
    // the new Current Goal banner below the heading - the grid itself,
    // column/row math, and card sizing are otherwise completely unchanged.
    const float GridTop = 0.755f;
    const float ColumnGap = 0.018f;
    const float RowGap = 0.035f;

    readonly Text goalText;

    public GymSelectionScreen(Transform parent, GameManager gm) : base(parent, gm, "GymSelectionScreen", "gym_map")
    {
        UIFactory.CreateHeading(Root.transform, "SELECT YOUR GYM", new Vector2(0.06f, 0.90f), new Vector2(0.94f, 0.98f));

        // Milestone 54, Part 4/8: current objective + a lightweight
        // completion tally, both derived live from existing progression
        // state - no new save fields. Gold, since this is the screen's most
        // important "what do I do next" header per the unified palette rules.
        goalText = UIFactory.CreateCaption(Root.transform, "", new Vector2(0.06f, 0.855f), new Vector2(0.94f, 0.895f), TextAnchor.MiddleCenter);
        goalText.color = UIFactory.GoldColor;
        goalText.fontStyle = FontStyle.Bold;
        goalText.resizeTextMinSize = 12;
        goalText.resizeTextMaxSize = UIFactory.BodySize;

        // Milestone 22, Part 7: a small recurring rival comments on your progress
        // while you pick your next gym. Reuses existing GameManager stats only.
        rivalText = UIFactory.CreateCaption(Root.transform, "", new Vector2(0.06f, 0.775f), new Vector2(0.94f, 0.845f), TextAnchor.MiddleCenter);
        // Typography pass: this carries real narrative weight (the rival
        // tracker status, including "this is it" right before the showdown
        // becomes available) but read as throwaway muted caption text.
        // Same box, just a brighter color and a higher best-fit ceiling.
        rivalText.color = UIFactory.GoldColor;
        rivalText.resizeTextMinSize = 13;
        rivalText.resizeTextMaxSize = UIFactory.BodySize;

        UIFactory.CreateButton(Root.transform, "BACK TO HOME", new Vector2(0.3f, 0.03f), new Vector2(0.7f, 0.12f),
            () => GM.ChangeState(GameState.GymMap), UIFactory.SecondaryColor, isBackAction: true);

        rivalDialogue = UIFactory.CreateRivalDialogue(Root.transform);
    }

    public void Refresh()
    {
        foreach (var entry in dynamicEntries) Object.Destroy(entry);
        dynamicEntries.Clear();
        pendingInterceptGym = null;

        var gyms = GymDatabase.AllGyms;
        if (gyms == null || gyms.Count == 0)
        {
            Debug.LogWarning("GymSelectionScreen.Refresh: no gyms found in GymDatabase.");
            return;
        }

        // Milestone 26: the Mirror Match banner (formerly "Shadow Gym") only
        // appears below the grid, never as a 7th grid card. Milestone 44: its
        // gate moved from "became champion" to "defeated Rival Scratch" - it's
        // now the true final test, available only after both the Championship
        // and the Rival are behind the player. Milestone 39: the Rival
        // Showdown banner lives in this same strip while it's still available.
        // Since Mirror Match now requires the Rival already defeated, the two
        // banners are mutually exclusive in practice - this never shows both -
        // but the stacking math is left in place rather than torn out, since
        // it's harmless and still correct if that ever changes.
        bool mirrorMatchReady = GM.HasDefeatedRival;
        bool rivalShowdownReady = GM.IsRivalFightReady();
        int bannerCount = (rivalShowdownReady ? 1 : 0) + (mirrorMatchReady ? 1 : 0);
        float gridBottom = bannerCount == 0 ? 0.16f : bannerCount == 1 ? 0.235f : 0.325f;

        for (int i = 0; i < gyms.Count; i++)
        {
            BuildGymCard(gyms[i], i, gridBottom);
        }
        BuildStreetFightCard(gyms.Count, gridBottom);

        // Rival Showdown takes the slot closer to the grid (encountered
        // first); Mirror Match keeps its original slot at the very bottom so
        // its position doesn't shift if the rival banner is ever visible too.
        const float bannerHeight = 0.065f;
        const float bannerGap = 0.02f;
        float lowerSlot = 0.16f;
        float upperSlot = lowerSlot + bannerHeight + bannerGap;
        if (rivalShowdownReady)
            BuildRivalShowdownBanner(new Vector2(0.05f, upperSlot), new Vector2(0.95f, upperSlot + bannerHeight));
        if (mirrorMatchReady)
            BuildShadowBanner(new Vector2(0.05f, lowerSlot), new Vector2(0.95f, lowerSlot + bannerHeight));

        // Milestone 54, Part 4/8: current objective + completion tally,
        // refreshed every time this screen is shown.
        goalText.text = $"CURRENT GOAL: {GetCurrentObjective()}\n{GetProgressionSummary()}";

        // Milestone 33, Part 2/5: the rival's existing progress quip plus the
        // Rival Tracker status, so this screen doubles as "world presence."
        rivalText.text = $"{RivalDatabase.RivalName}: \"{RivalDatabase.GetLine(GM)}\"\n{RivalDatabase.GetRivalStatus(GM)}";

        if (pendingInterceptGym != null)
        {
            var gym = pendingInterceptGym;
            pendingInterceptGym = null;
            RunAnimation(ShowGymInterceptDelayed(gym));
        }
    }

    // 3 columns x 2 fixed rows, evenly spaced and centered between GridXMin/Max.
    void GetCardAnchors(int index, float gridBottom, out Vector2 anchorMin, out Vector2 anchorMax)
    {
        int row = index / Columns;
        int col = index % Columns;

        float cardWidth = (GridXMax - GridXMin - (Columns - 1) * ColumnGap) / Columns;
        float xMin = GridXMin + col * (cardWidth + ColumnGap);

        float gridHeight = GridTop - gridBottom;
        float rowHeight = (gridHeight - RowGap) / 2f;
        float yMax = GridTop - row * (rowHeight + RowGap);

        anchorMin = new Vector2(xMin, yMax - rowHeight);
        anchorMax = new Vector2(xMin + cardWidth, yMax);
    }

    // Milestone 39: the Championship card is a normal gym card again - the
    // Rival Showdown moved out to its own banner below the grid (see Refresh/
    // BuildRivalShowdownBanner) since it's now a post-Championship encounter,
    // not a gate blocking this card.
    void BuildGymCard(GymInfo gym, int index, float gridBottom)
    {
        bool unlocked = GM.IsGymUnlocked(gym);
        bool completed = GM.IsGymCompleted(gym);

        string name = gym.GymName;
        string description = gym.Description;
        string tagline;
        Color taglineColor;
        Color accentColor;
        string badgeText;
        Sprite iconSprite;
        Color iconColor;
        bool interactable = unlocked;

        if (completed)
        {
            tagline = "Cleared";
            taglineColor = UIFactory.PositiveColor;
            accentColor = UIFactory.PositiveColor;
            // Milestone 54, Part 1: an explicit checkmark reads as "completed"
            // at a glance faster than the word "CLEARED" alone.
            badgeText = "✓ COMPLETED";
        }
        else if (unlocked)
        {
            tagline = !string.IsNullOrEmpty(gym.Motto) ? gym.Motto : "Available";
            taglineColor = IconFactory.GetGymThemeColor(gym.GymType);
            // Milestone 54, Part 7: Championship gets Gold instead of the
            // generic Action Orange while available - a small, existing-
            // palette cue that it's the league's most important destination,
            // distinct from a regular gym - without touching its card's
            // position, size, or any other gym's treatment.
            accentColor = gym.GymType == GymType.Championship ? UIFactory.GoldColor : UIFactory.AccentOrange;
            badgeText = "SELECTED";
        }
        else
        {
            tagline = "Locked";
            // Milestone 48A: was DangerColor (red) - a locked gym isn't a
            // negative outcome, just unavailable yet. Locked = Bronze.
            taglineColor = UIFactory.LockedColor;
            accentColor = UIFactory.LockedColor;
            badgeText = "LOCKED";
        }

        var realIcon = ArtRegistry.GetGymIcon(gym.GymId);
        iconSprite = realIcon != null ? realIcon : IconFactory.GetShapeSprite(IconFactory.GetGymIconShape(gym.GymType));
        iconColor = interactable ? Color.white : new Color(1f, 1f, 1f, 0.5f);

        // Milestone 54, Part 2/3/6: replaces the card's old single flavor
        // sentence with the denser progression info this milestone asks
        // for - personality tag, lesson learned, and reward earned/pending.
        // Reuses the exact same description text element/styling (no new
        // UI, no BuildCard signature change) - just different content.
        // Uses GymInfo.LessonText/UnlockMoveId directly (Milestone 52/53
        // data, no duplication) - Championship has neither a personality tag
        // nor a LessonText, so its block naturally reduces to just the
        // reward line.
        description = BuildGymInfoBlock(gym, completed);

        GetCardAnchors(index, gridBottom, out Vector2 anchorMin, out Vector2 anchorMax);
        var cardButton = BuildCard(anchorMin, anchorMax, accentColor, interactable,
            () => TravelToGym(gym),
            iconSprite, iconColor, name, tagline, taglineColor, description, badgeText, accentColor);

        if (interactable && seenUnlockedGymIds.Add(gym.GymId))
        {
            PlayPulse((RectTransform)cardButton.transform, 1.05f, 0.5f);
            // Milestone 33, Part 4: skip the very first gym - the rival's
            // FirstAppearanceLines greeting on the Home screen already covers
            // the start of the run.
            if (unlocked && index > 0) pendingInterceptGym = gym;
        }
    }

    // Milestone 54, Part 2/3/6: one combined info string per gym card -
    // personality tag (if any), lesson learned/pending (Milestone 52/53
    // data, read directly from GymInfo.LessonText, never duplicated), and
    // the move reward earned/pending (resolved from GymInfo.UnlockMoveId via
    // the existing MoveDatabase lookup). Each line only appears if there's
    // real data behind it, so Championship (no personality, no LessonText)
    // naturally shows just its reward line.
    static string BuildGymInfoBlock(GymInfo gym, bool completed)
    {
        var lines = new List<string>();

        string personality = GetGymPersonality(gym.GymType);
        if (!string.IsNullOrEmpty(personality)) lines.Add(personality);

        if (!string.IsNullOrEmpty(gym.LessonText))
            lines.Add(completed ? $"✓ Lesson: {gym.LessonText}" : "Lesson: Not Learned Yet");

        var rewardMove = MoveDatabase.GetById(gym.UnlockMoveId);
        if (rewardMove != null)
        {
            lines.Add(completed
                ? $"✓ Reward Earned: {rewardMove.Name.ToUpperInvariant()}"
                : $"Reward: {rewardMove.Name.ToUpperInvariant()}");
        }

        return string.Join("\n", lines);
    }

    // Milestone 54, Part 6: small, static personality tags - no new data,
    // no new systems, just naming the identity Milestone 51 already built.
    static string GetGymPersonality(GymType type)
    {
        switch (type)
        {
            case GymType.Boxing: return "Combo-Focused";
            case GymType.MuayThai: return "Pressure-Focused";
            case GymType.Wrestling: return "Control-Focused";
            case GymType.BrazilianJiuJitsu: return "Submission-Focused";
            default: return null;
        }
    }

    // Milestone 30 (relocation): Street Fight as a first-class progression
    // option alongside the gyms - same card style, always available, no lock
    // state, no badge (mirrors the reference layout exactly).
    void BuildStreetFightCard(int index, float gridBottom)
    {
        GetCardAnchors(index, gridBottom, out Vector2 anchorMin, out Vector2 anchorMax);
        var iconSprite = IconFactory.GetShapeSprite(IconShape.Diamond);

        BuildCard(anchorMin, anchorMax, UIFactory.AccentOrange, true,
            () => GM.ChangeState(GameState.StreetFight),
            iconSprite, UIFactory.CreamColor, "STREET FIGHT", null, UIFactory.MutedTextColor,
            "Random opponents.\nRisk and reward.\nTrain outside the gym system.", null, UIFactory.AccentOrange);
    }

    // Shared card chrome: a colored border card with an inset dark fill, an
    // icon, a name, an optional tagline, a description, and an optional status
    // badge pill. Built on CreateCardButton/CreateCard/CreateText only - no
    // new visual primitives. The border card IS the button, so hovering it
    // brightens the border itself (Unity's built-in Selectable tint) for the
    // "hover glow/outline" effect, and Selectable already dims it automatically
    // when interactable is false (the "locked cards are dimmed" requirement).
    Button BuildCard(Vector2 anchorMin, Vector2 anchorMax, Color borderColor, bool interactable,
        UnityEngine.Events.UnityAction onClick, Sprite iconSprite, Color iconColor,
        string name, string tagline, Color taglineColor, string description, string badgeText, Color badgeColor)
    {
        var border = UIFactory.CreateCardButton(Root.transform, name, anchorMin, anchorMax, onClick, borderColor);
        border.interactable = interactable;
        dynamicEntries.Add(border.gameObject);

        var fill = UIFactory.CreateCard(border.transform, name + "Fill", new Vector2(0.025f, 0.035f), new Vector2(0.975f, 0.965f),
            new Color(UIFactory.BackgroundColor.r, UIFactory.BackgroundColor.g, UIFactory.BackgroundColor.b, 0.97f));
        fill.GetComponent<Image>().raycastTarget = false;

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconGo.transform.SetParent(fill, false);
        var iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.05f, 0.66f);
        iconRt.anchorMax = new Vector2(0.22f, 0.92f);
        iconRt.offsetMin = Vector2.zero;
        iconRt.offsetMax = Vector2.zero;
        var iconImage = iconGo.GetComponent<Image>();
        iconImage.sprite = iconSprite;
        iconImage.color = iconColor;
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;

        var nameText = UIFactory.CreateText(fill, name, UIFactory.BodySize, UIFactory.CreamColor, TextAnchor.MiddleLeft,
            new Vector2(0.26f, 0.66f), new Vector2(0.97f, 0.92f), FontStyle.Bold);
        nameText.resizeTextForBestFit = true;
        nameText.resizeTextMinSize = 14;
        nameText.resizeTextMaxSize = UIFactory.BodySize;
        nameText.raycastTarget = false;

        float descTop = 0.6f;
        if (!string.IsNullOrEmpty(tagline))
        {
            var taglineText = UIFactory.CreateText(fill, tagline, UIFactory.CaptionSize, taglineColor, TextAnchor.UpperLeft,
                new Vector2(0.07f, 0.5f), new Vector2(0.95f, 0.62f), FontStyle.Bold);
            taglineText.raycastTarget = false;
            // Quick Fix (Font Replacement Pass), Part 5: mottos/taglines are
            // short but AtkinsonHyperlegible-Bold's glyphs can still push a
            // longer one (e.g. a gym motto) past this single-line band.
            taglineText.resizeTextForBestFit = true;
            taglineText.resizeTextMinSize = 12;
            taglineText.resizeTextMaxSize = UIFactory.CaptionSize;
            descTop = 0.5f;
        }

        var descText = UIFactory.CreateText(fill, description ?? "", UIFactory.CaptionSize, UIFactory.CreamColor, TextAnchor.UpperLeft,
            new Vector2(0.07f, 0.16f), new Vector2(0.95f, descTop));
        descText.raycastTarget = false;
        // Quick Fix (Font Replacement Pass), Part 5: gym descriptions are full
        // sentences in a fixed-height card region - guards against the wider
        // handwritten font wrapping to more lines than this card allows.
        descText.resizeTextForBestFit = true;
        descText.resizeTextMinSize = 12;
        descText.resizeTextMaxSize = UIFactory.CaptionSize;

        if (!string.IsNullOrEmpty(badgeText))
        {
            var badge = UIFactory.CreateCard(fill, "Badge", new Vector2(0.07f, 0.04f), new Vector2(0.5f, 0.13f),
                new Color(badgeColor.r, badgeColor.g, badgeColor.b, 0.22f));
            badge.GetComponent<Image>().raycastTarget = false;
            var badgeLabel = UIFactory.CreateText(badge, badgeText, UIFactory.CaptionSize, badgeColor, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, FontStyle.Bold);
            badgeLabel.resizeTextForBestFit = true;
            badgeLabel.resizeTextMinSize = 11;
            badgeLabel.resizeTextMaxSize = UIFactory.CaptionSize;
            badgeLabel.raycastTarget = false;
        }

        return border;
    }

    // A thin full-width strip below the grid rather than a 3rd grid row -
    // this is a rare, late-game secret, not part of the normal 6-card layout.
    // Milestone 39: now takes explicit anchors so it can sit in either the
    // single-banner or stacked-with-rival-banner slot (see Refresh).
    // Milestone 44: presentation renamed from "The Shadow Gym" to "Mirror
    // Match" - a quiet silver-grey instead of the old purple, matching the
    // brief's "quiet, strange, reflective" tone (distinct from the Rival
    // Showdown banner's loud violet just above it).
    void BuildShadowBanner(Vector2 anchorMin, Vector2 anchorMax)
    {
        bool defeated = GM.HasDefeatedShadowChampion;
        string tagline = defeated ? "Defeated - \"True Champion\" earned" : "The final test awaits...";
        Color color = defeated ? UIFactory.PositiveColor : new Color(0.55f, 0.6f, 0.68f, 1f);

        var button = UIFactory.CreateCardButton(Root.transform, "MirrorMatch", anchorMin, anchorMax,
            () => TravelToShadowChampion(), color);
        dynamicEntries.Add(button.gameObject);

        if (!defeated) PlayPulse((RectTransform)button.transform, 1.04f, 0.7f);

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconGo.transform.SetParent(button.transform, false);
        var iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.012f, 0.15f);
        iconRt.anchorMax = new Vector2(0.05f, 0.85f);
        iconRt.offsetMin = Vector2.zero;
        iconRt.offsetMax = Vector2.zero;
        var iconImage = iconGo.GetComponent<Image>();
        iconImage.sprite = IconFactory.GetShapeSprite(IconShape.Star);
        iconImage.color = UIFactory.GoldColor;
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;

        var label = UIFactory.CreateText(button.transform, $"MIRROR MATCH  -  {tagline}", UIFactory.BodySize, UIFactory.CreamColor,
            TextAnchor.MiddleLeft, new Vector2(0.07f, 0f), new Vector2(0.97f, 1f), FontStyle.Bold);
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = 12;
        label.resizeTextMaxSize = UIFactory.BodySize;
        label.raycastTarget = false;
    }

    // Milestone 39, Part 1/2: the league's true final test - appears once the
    // player becomes champion, replacing the old "blocks the Championship
    // card" presentation with its own banner below the grid (same pattern as
    // the Shadow Gym banner), and disappears once Scratch is defeated.
    void BuildRivalShowdownBanner(Vector2 anchorMin, Vector2 anchorMax)
    {
        var button = UIFactory.CreateCardButton(Root.transform, "RivalShowdown", anchorMin, anchorMax,
            () => TravelToRivalFight(), RivalDatabase.AccentColor);
        dynamicEntries.Add(button.gameObject);

        if (seenUnlockedGymIds.Add("rival_showdown_ready"))
            PlayPulse((RectTransform)button.transform, 1.06f, 0.7f);

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconGo.transform.SetParent(button.transform, false);
        var iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.012f, 0.15f);
        iconRt.anchorMax = new Vector2(0.05f, 0.85f);
        iconRt.offsetMin = Vector2.zero;
        iconRt.offsetMax = Vector2.zero;
        var iconImage = iconGo.GetComponent<Image>();
        iconImage.sprite = IconFactory.GetShapeSprite(IconShape.Star);
        iconImage.color = RivalDatabase.AccentColor;
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;

        var label = UIFactory.CreateText(button.transform, $"RIVAL SHOWDOWN  -  {RivalDatabase.RivalName} is waiting.",
            UIFactory.BodySize, UIFactory.CreamColor, TextAnchor.MiddleLeft, new Vector2(0.07f, 0f), new Vector2(0.97f, 1f), FontStyle.Bold);
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = 12;
        label.resizeTextMaxSize = UIFactory.BodySize;
        label.raycastTarget = false;
    }

    // Milestone 54, Part 4: derived live from existing progression state
    // (gym completion, Rival/Mirror Match flags) - no new save fields, no
    // tracked "current objective" of its own. Walking GymDatabase.AllGyms in
    // its existing RequiredGymId order means this naturally cascades through
    // exactly the same unlock chain the rest of the game already uses.
    string GetCurrentObjective()
    {
        foreach (var gym in GymDatabase.AllGyms)
        {
            if (GM.IsGymCompleted(gym)) continue;
            return GM.IsGymUnlocked(gym) ? $"Defeat {gym.GymName} Leader" : $"Clear earlier gyms to unlock {gym.GymName}";
        }

        if (GM.IsRivalFightReady()) return $"Defeat {RivalDatabase.RivalName}";
        if (GM.HasDefeatedRival && !GM.HasDefeatedShadowChampion) return "Complete the Mirror Match";
        if (GM.CanPrestige) return "Prestige to begin a new league";
        return "You've conquered the league - keep training.";
    }

    // Milestone 54, Part 8: a lightweight one-line tally - same data this
    // screen's cards and GetCurrentObjective already read, just counted.
    string GetProgressionSummary()
    {
        var gyms = GymDatabase.AllGyms;
        int gymsCompleted = 0, lessonsLearned = 0, lessonGymCount = 0, rewardsEarned = 0, rewardGymCount = 0;
        foreach (var gym in gyms)
        {
            bool completed = GM.IsGymCompleted(gym);
            if (completed) gymsCompleted++;
            if (!string.IsNullOrEmpty(gym.LessonText))
            {
                lessonGymCount++;
                if (completed) lessonsLearned++;
            }
            if (MoveDatabase.GetById(gym.UnlockMoveId) != null)
            {
                rewardGymCount++;
                if (completed) rewardsEarned++;
            }
        }

        return $"Gyms Completed: {gymsCompleted}/{gyms.Count}   |   " +
            $"Lessons Learned: {lessonsLearned}/{lessonGymCount}   |   " +
            $"Move Rewards Earned: {rewardsEarned}/{rewardGymCount}";
    }

    void TravelToGym(GymInfo gym) => GM.EnterGym(gym);

    // Milestone 34, Part 1/2: shows the showdown intro encounter, then starts
    // the fight directly when the player taps through the last line -
    // RivalDialogueBox's own onComplete callback, no extra coroutine needed.
    void TravelToRivalFight() =>
        rivalDialogue.Show(RivalDatabase.RivalName, RivalDatabase.ShowdownIntroLines, () => GM.StartRivalFight());

    void TravelToShadowChampion() => GM.StartShadowChampionBattle();

    // Milestone 33, Part 4: a short pause after the screen settles, same
    // pattern GymMapScreen's first-appearance greeting already uses.
    IEnumerator ShowGymInterceptDelayed(GymInfo gym)
    {
        yield return new WaitForSecondsRealtime(0.5f);
        rivalDialogue.Show(RivalDatabase.RivalName, RivalDatabase.GetGymInterceptLines(gym));
    }
}
