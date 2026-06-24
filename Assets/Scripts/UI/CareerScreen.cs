using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Milestone 47 (Career Records & Hall of Fame): a single trophy-room screen
// that pulls together accomplishments already tracked elsewhere (Prestige,
// Hall of Champions, lifetime stats) into one retrospective view. Reads
// existing GameManager/ArtRegistry/PrestigeSystem data only - no new
// progression, no new combat hooks, and only one new save field
// (GameManager.BestWinStreak) where no existing data could answer
// Part 2/6's "Best/Longest Win Streak" ask.
public class CareerScreen : UIScreen
{
    // Milestone 49, Part 4: NEWS added as a 6th tab. Milestone 56, Part 4:
    // HIGHLIGHTS added as a 7th - both reuse this same tab-switching
    // infrastructure entirely, so neither needed a new screen, GameState, or
    // nav button.
    readonly string[] tabNames = { "SUMMARY", "HALL OF FAME", "TATTOOS", "MILESTONES", "RECORDS", "NEWS", "HIGHLIGHTS" };
    readonly Image[] tabButtonImages;
    readonly RectTransform[] tabPanels;

    readonly Text summaryText;
    readonly Transform hallOfFameList;
    readonly Text hallOfFameEmptyText;
    readonly Text hallOfFameSummaryText;
    readonly Transform tattooGrid;
    readonly Text milestonesText;
    readonly Text recordsText;
    readonly Text newsText;
    readonly Text highlightsText;

    // Lightweight cap on how many headlines are actually shown - the feed
    // itself never stops growing as Hall of Champions grows, but most
    // recent/current news matters most; older history just scrolls off.
    const int MaxNewsHeadlines = 14;

    readonly List<GameObject> dynamicEntries = new List<GameObject>();

    public CareerScreen(Transform parent, GameManager gm) : base(parent, gm, "CareerScreen", "gym_map")
    {
        UIFactory.CreateHeading(Root.transform, "CAREER", new Vector2(0.05f, 0.91f), new Vector2(0.95f, 0.99f));

        // Tab row - five even columns, same math as ProfileScreen's button
        // rows. Hand-rolled here rather than a new generic UIFactory tab
        // system, since only this one screen needs it (credit conservation).
        tabButtonImages = new Image[tabNames.Length];
        tabPanels = new RectTransform[tabNames.Length];
        const float tabXMin = 0.04f, tabXMax = 0.96f, tabGap = 0.015f;
        float tabWidth = (tabXMax - tabXMin - (tabNames.Length - 1) * tabGap) / tabNames.Length;
        for (int i = 0; i < tabNames.Length; i++)
        {
            int index = i;
            float xMin = tabXMin + i * (tabWidth + tabGap);
            var button = UIFactory.CreateButton(Root.transform, tabNames[i], new Vector2(xMin, 0.81f), new Vector2(xMin + tabWidth, 0.89f),
                () => ShowTab(index), UIFactory.SecondaryColor);
            tabButtonImages[i] = button.GetComponent<Image>();

            var panel = UIFactory.CreateContainer(Root.transform, new Vector2(0.04f, 0.12f), new Vector2(0.96f, 0.79f));
            panel.gameObject.SetActive(false);
            tabPanels[i] = panel;
        }

        UIFactory.CreateButton(Root.transform, "BACK", new Vector2(0.39f, 0.02f), new Vector2(0.61f, 0.1f),
            () => GM.ChangeState(GameState.ProfileScreen), UIFactory.SecondaryColor, isBackAction: true);

        summaryText = BuildSummaryTab(tabPanels[0]);
        (hallOfFameList, hallOfFameEmptyText, hallOfFameSummaryText) = BuildHallOfFameTab(tabPanels[1]);
        tattooGrid = BuildTattooGalleryTab(tabPanels[2]);
        milestonesText = BuildMilestonesTab(tabPanels[3]);
        recordsText = BuildRecordsTab(tabPanels[4]);
        newsText = BuildNewsTab(tabPanels[5]);
        highlightsText = BuildHighlightsTab(tabPanels[6]);

        ShowTab(0);
    }

    void ShowTab(int index)
    {
        AudioManager.Instance?.PlayClick();
        for (int i = 0; i < tabPanels.Length; i++)
        {
            tabPanels[i].gameObject.SetActive(i == index);
            tabButtonImages[i].color = i == index ? UIFactory.GoldColor : UIFactory.SecondaryColor;
        }
    }

    static Text CreateSectionTitle(Transform parent, string content, Vector2 anchorMin, Vector2 anchorMax)
    {
        // Part 7: section titles use AbolitionTest-Rough - the only
        // existing constant mapped to that font is ButtonTextSize, which is
        // too small for a section header here, so the font is set explicitly
        // after creation (same override pattern used elsewhere in this UI).
        var text = UIFactory.CreateText(parent, content, UIFactory.SubheadingSize, UIFactory.GoldColor,
            TextAnchor.MiddleCenter, anchorMin, anchorMax, FontStyle.Bold);
        text.font = UIFactory.UiFont;
        return text;
    }

    // ---------- Part 2: Career Summary ----------

    Text BuildSummaryTab(Transform panel)
    {
        CreateSectionTitle(panel, "CAREER SUMMARY", new Vector2(0.04f, 0.88f), new Vector2(0.96f, 1f));

        var card = UIFactory.CreateCard(panel, "Summary", new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.85f));
        var text = UIFactory.CreateText(card, "", UIFactory.BodySize, UIFactory.CreamColor, TextAnchor.UpperLeft,
            new Vector2(0.05f, 0.04f), new Vector2(0.95f, 0.96f));
        return text;
    }

    void RefreshSummaryTab()
    {
        float winPct = GM.TotalBattles > 0 ? GM.TotalWins * 100f / GM.TotalBattles : 0f;

        summaryText.text =
            $"Current Prestige: {PrestigeSystem.FormatLevel(GM.PrestigeLevel)}\n" +
            $"Highest Prestige Reached: {PrestigeSystem.FormatLevel(GM.HighestPrestigeReached)}\n" +
            $"Total Game Completions: {GM.TotalGameCompletions}\n\n" +
            $"Current Win Streak: {GM.CurrentWinStreak}\n" +
            $"Best Win Streak: {GM.BestWinStreak}\n\n" +
            $"Total Wins: {GM.TotalWins}\n" +
            $"Total Losses: {GM.TotalLosses}\n" +
            $"Win Percentage: {winPct:F1}%\n\n" +
            $"Total Coins Earned: {GM.TotalCoinsEarned}";
    }

    // ---------- Part 3: Hall of Champions (cleaner display) ----------

    (Transform, Text, Text) BuildHallOfFameTab(Transform panel)
    {
        CreateSectionTitle(panel, "HALL OF CHAMPIONS", new Vector2(0.04f, 0.88f), new Vector2(0.96f, 1f));

        // Milestone 49, Part 6: a one-line career summary above the list -
        // championship/rival/mirror match counts, highest Prestige - all
        // derived from existing data, no new records created.
        var summary = UIFactory.CreateCaption(panel, "", new Vector2(0.04f, 0.78f), new Vector2(0.96f, 0.87f), TextAnchor.MiddleCenter);
        summary.color = UIFactory.GoldColor;

        var list = UIFactory.CreateContainer(panel, new Vector2(0.0f, 0.02f), new Vector2(1f, 0.76f));
        var empty = UIFactory.CreateCaption(panel, "No titles earned yet - get out there and fight!",
            new Vector2(0.04f, 0.36f), new Vector2(0.96f, 0.46f), TextAnchor.MiddleCenter);
        empty.gameObject.SetActive(false);
        return (list, empty, summary);
    }

    void RefreshHallOfFameTab()
    {
        var records = GM.HallOfChampions;
        hallOfFameSummaryText.text = $"Championships: {GM.ChampionshipWinCount}   |   Rival Wins: {GM.RivalWinCount}   |   " +
            $"Mirror Match Wins: {GM.MirrorMatchWinCount}   |   Highest Prestige: {PrestigeSystem.FormatLevel(GM.HighestPrestigeReached)}";

        hallOfFameEmptyText.gameObject.SetActive(records.Count == 0);
        if (records.Count == 0) return;

        // Most recent first, same convention as the standalone Hall of
        // Champions screen this reuses data from.
        for (int i = records.Count - 1, row = 0; i >= 0; i--, row++)
            BuildHallOfFameRow(records[i], i + 1, row, records.Count);
    }

    void BuildHallOfFameRow(ChampionRecord record, int order, int row, int total)
    {
        float slotHeight = 1f / total;
        float padding = slotHeight * 0.1f;
        float yMax = 1f - row * slotHeight - padding;
        float yMin = 1f - (row + 1) * slotHeight + padding;

        var card = UIFactory.CreateCard(hallOfFameList, $"Title_{row}", new Vector2(0f, yMin), new Vector2(1f, yMax));
        dynamicEntries.Add(card.gameObject);

        var medalGo = new GameObject("Medal", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        medalGo.transform.SetParent(card, false);
        var medalRt = medalGo.GetComponent<RectTransform>();
        medalRt.anchorMin = new Vector2(0.015f, 0.2f);
        medalRt.anchorMax = new Vector2(0.09f, 0.8f);
        medalRt.offsetMin = Vector2.zero;
        medalRt.offsetMax = Vector2.zero;
        var medalImage = medalGo.GetComponent<Image>();
        medalImage.sprite = IconFactory.GetShapeSprite(IconShape.Star);
        medalImage.color = row == 0 ? UIFactory.GoldColor : UIFactory.MutedTextColor;

        string title = string.IsNullOrEmpty(record.Title) ? "Champion" : record.Title;
        UIFactory.CreateText(card, $"#{order}  {title}", UIFactory.BodySize, UIFactory.GoldColor, TextAnchor.MiddleLeft,
            new Vector2(0.11f, 0.48f), new Vector2(0.64f, 0.9f), FontStyle.Bold);

        UIFactory.CreateCaption(card, $"{record.FighterName} - {record.Archetype} - Lv.{record.FinalLevel}  ({record.TotalWinsAtCompletion} wins)",
            new Vector2(0.11f, 0.08f), new Vector2(0.64f, 0.48f));

        UIFactory.CreateCaption(card, record.CompletionDate, new Vector2(0.66f, 0f), new Vector2(0.985f, 1f), TextAnchor.MiddleRight);
    }

    // ---------- Part 4: Tattoo Gallery ----------

    Transform BuildTattooGalleryTab(Transform panel)
    {
        CreateSectionTitle(panel, "TATTOO GALLERY", new Vector2(0.04f, 0.88f), new Vector2(0.96f, 1f));
        return UIFactory.CreateContainer(panel, new Vector2(0.0f, 0.0f), new Vector2(1f, 0.86f));
    }

    void RefreshTattooGalleryTab()
    {
        // Part 6: driven by PrestigeSystem.MaxPrestigeLevel, not a literal
        // "10" - adding more tattoo assets later (up to that cap) needs no
        // change here, and a slot with no uploaded asset yet still renders
        // safely as a locked placeholder rather than breaking the grid.
        int total = PrestigeSystem.MaxPrestigeLevel;
        int columns = Mathf.Min(5, total);
        int rows = Mathf.CeilToInt(total / (float)columns);
        const float colGap = 0.02f, rowGap = 0.05f;
        float cellW = (0.96f - (columns - 1) * colGap) / columns;
        float cellH = (0.84f - (rows - 1) * rowGap) / rows;

        for (int i = 0; i < total; i++)
        {
            int level = i + 1;
            int col = i % columns;
            int row = i / columns;
            float xMin = 0.02f + col * (cellW + colGap);
            float xMax = xMin + cellW;
            float yMax = 0.84f - row * (cellH + rowGap);
            float yMin = yMax - cellH;
            BuildTattooSlot(level, new Vector2(xMin, yMin), new Vector2(xMax, yMax));
        }
    }

    void BuildTattooSlot(int level, Vector2 anchorMin, Vector2 anchorMax)
    {
        bool unlocked = GM.HighestPrestigeReached >= level;
        bool isCurrent = GM.PrestigeLevel == level;
        Sprite sprite = ArtRegistry.GetPrestigeTattoo(level);

        // Milestone 48A: derived from the unified theme instead of separately
        // hand-picked browns - Gold for the current (most important) level,
        // a darkened Bronze for locked (consistent with AchievementsScreen's
        // locked-row treatment), plain Background for unlocked-but-not-current.
        Color cardColor = isCurrent ? new Color(UIFactory.GoldColor.r * 0.5f, UIFactory.GoldColor.g * 0.5f, UIFactory.GoldColor.b * 0.5f, 1f)
            : unlocked ? UIFactory.BackgroundColor
            : new Color(UIFactory.LockedColor.r * 0.35f, UIFactory.LockedColor.g * 0.35f, UIFactory.LockedColor.b * 0.35f, 0.85f);

        var card = UIFactory.CreateCard(tattooGrid, $"Tattoo_{level}", anchorMin, anchorMax, cardColor);
        dynamicEntries.Add(card.gameObject);

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconGo.transform.SetParent(card, false);
        var iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.18f, 0.34f);
        iconRt.anchorMax = new Vector2(0.82f, 0.94f);
        iconRt.offsetMin = Vector2.zero;
        iconRt.offsetMax = Vector2.zero;
        var iconImage = iconGo.GetComponent<Image>();
        iconImage.preserveAspect = true;
        if (sprite != null)
        {
            iconImage.sprite = sprite;
            iconImage.color = unlocked ? Color.white : new Color(0.35f, 0.35f, 0.35f, 0.6f);
        }
        else
        {
            // Asset for this Prestige level hasn't been uploaded yet - a
            // dim placeholder rather than an empty hole in the grid.
            iconImage.sprite = IconFactory.GetShapeSprite(IconShape.Circle);
            iconImage.color = new Color(0.25f, 0.25f, 0.25f, 0.35f);
        }

        UIFactory.CreateCaption(card, PrestigeSystem.FormatLevel(level), new Vector2(0.02f, 0.18f), new Vector2(0.98f, 0.34f), TextAnchor.MiddleCenter);

        string statusLine = isCurrent ? "CURRENT" : unlocked ? "" : "LOCKED";
        if (!string.IsNullOrEmpty(statusLine))
        {
            // Milestone 48A: "LOCKED" now uses the unified Locked Bronze
            // (was MutedTextColor) to match GymScreen/GymSelectionScreen's
            // locked-state labels.
            var status = UIFactory.CreateText(card, statusLine, UIFactory.CaptionSize, isCurrent ? UIFactory.GoldColor : UIFactory.LockedColor,
                TextAnchor.MiddleCenter, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.18f));
            status.resizeTextForBestFit = true;
            status.resizeTextMinSize = 8;
            status.resizeTextMaxSize = UIFactory.CaptionSize;
        }
    }

    // ---------- Part 5: Career Milestones ----------

    Text BuildMilestonesTab(Transform panel)
    {
        CreateSectionTitle(panel, "CAREER MILESTONES", new Vector2(0.04f, 0.88f), new Vector2(0.96f, 1f));
        var card = UIFactory.CreateCard(panel, "Milestones", new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.85f));
        var text = UIFactory.CreateText(card, "", UIFactory.BodySize, UIFactory.CreamColor, TextAnchor.UpperLeft,
            new Vector2(0.05f, 0.04f), new Vector2(0.95f, 0.96f));
        // Milestone 53, Part 1: Lessons Learned adds several more lines below
        // the existing milestones - same overflow safety net every other
        // accumulator text in this UI already uses.
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 12;
        text.resizeTextMaxSize = UIFactory.BodySize;
        return text;
    }

    void RefreshMilestonesTab()
    {
        var records = GM.HallOfChampions;
        ChampionRecord firstChampionship = FindFirst(records, r => string.IsNullOrEmpty(r.Title));
        ChampionRecord firstRival = FindFirst(records, r => r.Title == "Rival Conqueror");
        // "Shadow Slayer" is the pre-Mirror-Match title - old saves that
        // earned it before Milestone 44 still count as this milestone.
        ChampionRecord firstMirrorMatch = FindFirst(records, r => r.Title == "True Champion" || r.Title == "Shadow Slayer");
        ChampionRecord firstPrestige = FindFirst(records, r => r.Title != null && r.Title.StartsWith("Completed Prestige"));

        milestonesText.text =
            $"First Championship: {Describe(firstChampionship, GM.HasBecomeChampion())}\n" +
            $"First Rival Victory: {Describe(firstRival, GM.HasDefeatedRival)}\n" +
            $"First Mirror Match Victory: {Describe(firstMirrorMatch, GM.HasDefeatedShadowChampion)}\n" +
            $"First Prestige: {Describe(firstPrestige, GM.PrestigeLevel > 0)}\n\n" +
            $"Highest Prestige Reached: {PrestigeSystem.FormatLevel(GM.HighestPrestigeReached)}\n" +
            // Milestone 49, Part 2: both fully derived from Hall of Champions -
            // no new save fields.
            $"Total Championships Won: {GM.ChampionshipWinCount}\n" +
            $"Game Completions: {GM.TotalGameCompletions}\n\n" +
            BuildLessonsLearnedSection();
    }

    // Milestone 53 (Career Lessons Recap): reads GymInfo.LessonText (added in
    // Milestone 52) and GM.IsGymCompleted (existing, completedGymIds-backed)
    // directly - no new save fields, no second copy of the lesson text
    // anywhere. Filtering on "has a LessonText" rather than a hardcoded gym
    // list means Championship (which deliberately has none - it has its own
    // screen and beat already) is automatically excluded, and any future
    // lesson-bearing gym would automatically be included with no code change.
    string BuildLessonsLearnedSection()
    {
        var lessonGyms = new List<GymInfo>();
        foreach (var gym in GymDatabase.AllGyms)
            if (!string.IsNullOrEmpty(gym.LessonText)) lessonGyms.Add(gym);

        int completed = 0;
        foreach (var gym in lessonGyms)
            if (GM.IsGymCompleted(gym)) completed++;

        // Milestone 53, Part 4: rich-text color tags - same pattern already
        // used throughout BattleScreen's log - so completed/incomplete lines
        // can carry the unified Gold/Locked-Bronze meaning within this one
        // Text block, with no new dynamic UI elements.
        var sb = new System.Text.StringBuilder();
        sb.Append($"<b><color=#D8A63C>LESSONS LEARNED ({completed}/{lessonGyms.Count})</color></b>\n");
        foreach (var gym in lessonGyms)
        {
            bool done = GM.IsGymCompleted(gym);
            string hex = done ? "D8A63C" : "7A6652";
            string status = done ? $"✓ {gym.LessonText}" : "Not learned yet";
            sb.Append($"{gym.GymName}\n<color=#{hex}>{status}</color>\n");
        }
        return sb.ToString();
    }

    static ChampionRecord FindFirst(IReadOnlyList<ChampionRecord> records, System.Predicate<ChampionRecord> match)
    {
        for (int i = 0; i < records.Count; i++)
            if (match(records[i])) return records[i];
        return null;
    }

    static string Describe(ChampionRecord record, bool achievedFallback)
    {
        if (record != null) return $"Achieved - {record.CompletionDate}";
        return achievedFallback ? "Achieved" : "Not yet achieved";
    }

    // ---------- Part 6: Record Book ----------

    Text BuildRecordsTab(Transform panel)
    {
        CreateSectionTitle(panel, "RECORD BOOK", new Vector2(0.04f, 0.88f), new Vector2(0.96f, 1f));
        var card = UIFactory.CreateCard(panel, "Records", new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.85f));
        var text = UIFactory.CreateText(card, "", UIFactory.BodySize, UIFactory.CreamColor, TextAnchor.UpperLeft,
            new Vector2(0.05f, 0.04f), new Vector2(0.95f, 0.96f));
        // Milestone 49, Part 1: significantly more lines than this card had
        // before - same overflow safety net every other accumulator text in
        // this UI already uses.
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 12;
        text.resizeTextMaxSize = UIFactory.BodySize;
        return text;
    }

    void RefreshRecordsTab()
    {
        // Milestone 49, Part 1 (Combat Record Book): the previously
        // "Future Expansion" critical-hit/combo lines are now real, tracked
        // by BattleSystem and folded into GameManager every fight (see
        // GameManager.EndBattle). Submission/Rival/Mirror Match/Street Fight
        // wins all reuse existing data - no duplicate tracking.
        recordsText.text =
            $"Total Critical Hits: {GM.TotalCriticalHits}\n" +
            $"Most Critical Hits In One Fight: {GM.MostCriticalHitsInOneFight}\n" +
            $"Total Combos Triggered: {GM.TotalCombosTriggered}\n" +
            $"Most Combos In One Fight: {GM.MostCombosInOneFight}\n" +
            $"Total Parries: {GM.TotalParries}\n" +
            $"Successful Parries: {GM.SuccessfulParries}\n" +
            $"Total Clinches: {GM.TotalClinches}\n" +
            $"Total Takedowns Landed: {GM.TotalTakedownsLanded}\n" +
            $"Total Submission Wins: {GM.SubmissionWins}\n" +
            $"Street Fight Wins: {GM.StreetFightWins}\n" +
            $"Rival Wins: {GM.RivalWinCount}\n" +
            $"Mirror Match Wins: {GM.MirrorMatchWinCount}\n\n" +
            $"Longest Win Streak: {GM.BestWinStreak}\n" +
            $"Most Damage in a Single Hit: {GM.MaxSingleHitDamage}\n" +
            $"Most Fights Won: {GM.TotalWins}\n" +
            $"Most Coins Earned (Lifetime): {GM.TotalCoinsEarned}";
    }

    // ---------- Part 4: CCL News Feed ----------

    Text BuildNewsTab(Transform panel)
    {
        CreateSectionTitle(panel, "CCL NEWS FEED", new Vector2(0.04f, 0.88f), new Vector2(0.96f, 1f));
        var card = UIFactory.CreateCard(panel, "News", new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.85f));
        var text = UIFactory.CreateText(card, "", UIFactory.BodySize, UIFactory.CreamColor, TextAnchor.UpperLeft,
            new Vector2(0.05f, 0.04f), new Vector2(0.95f, 0.96f));
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 12;
        text.resizeTextMaxSize = UIFactory.BodySize;
        return text;
    }

    void RefreshNewsTab()
    {
        var headlines = NewsFeedGenerator.GenerateHeadlines(GM);
        if (headlines.Count == 0)
        {
            newsText.text = "No news yet - get out there and make some.";
            return;
        }

        int shown = Mathf.Min(headlines.Count, MaxNewsHeadlines);
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < shown; i++)
        {
            if (i > 0) sb.Append('\n');
            sb.Append("- ").Append(headlines[i]);
        }
        newsText.text = sb.ToString();
    }

    // ---------- Part 4/5: Career Highlights Reel ----------

    Text BuildHighlightsTab(Transform panel)
    {
        CreateSectionTitle(panel, "CAREER HIGHLIGHTS", new Vector2(0.04f, 0.88f), new Vector2(0.96f, 1f));
        var card = UIFactory.CreateCard(panel, "Highlights", new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.85f));
        var text = UIFactory.CreateText(card, "", UIFactory.BodySize, UIFactory.CreamColor, TextAnchor.UpperLeft,
            new Vector2(0.05f, 0.04f), new Vector2(0.95f, 0.96f));
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 12;
        text.resizeTextMaxSize = UIFactory.BodySize;
        return text;
    }

    void RefreshHighlightsTab()
    {
        var highlights = CareerHighlightGenerator.GenerateHighlights(GM);
        if (highlights.Count == 0)
        {
            highlightsText.text = "No major highlights yet - get out there and make history.";
            return;
        }

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < highlights.Count; i++)
        {
            if (i > 0) sb.Append('\n');
            sb.Append("- ").Append(highlights[i]);
        }
        highlightsText.text = sb.ToString();
    }

    public void Refresh()
    {
        foreach (var entry in dynamicEntries) Object.Destroy(entry);
        dynamicEntries.Clear();

        RefreshSummaryTab();
        RefreshHallOfFameTab();
        RefreshTattooGalleryTab();
        RefreshMilestonesTab();
        RefreshRecordsTab();
        RefreshNewsTab();
        RefreshHighlightsTab();

        ShowTab(0);
    }
}
