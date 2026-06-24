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
    readonly string[] tabNames = { "SUMMARY", "HALL OF FAME", "TATTOOS", "MILESTONES", "RECORDS" };
    readonly Image[] tabButtonImages;
    readonly RectTransform[] tabPanels;

    readonly Text summaryText;
    readonly Transform hallOfFameList;
    readonly Text hallOfFameEmptyText;
    readonly Transform tattooGrid;
    readonly Text milestonesText;
    readonly Text recordsText;

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
        (hallOfFameList, hallOfFameEmptyText) = BuildHallOfFameTab(tabPanels[1]);
        tattooGrid = BuildTattooGalleryTab(tabPanels[2]);
        milestonesText = BuildMilestonesTab(tabPanels[3]);
        recordsText = BuildRecordsTab(tabPanels[4]);

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

    (Transform, Text) BuildHallOfFameTab(Transform panel)
    {
        CreateSectionTitle(panel, "HALL OF CHAMPIONS", new Vector2(0.04f, 0.88f), new Vector2(0.96f, 1f));

        var list = UIFactory.CreateContainer(panel, new Vector2(0.0f, 0.02f), new Vector2(1f, 0.85f));
        var empty = UIFactory.CreateCaption(panel, "No titles earned yet - get out there and fight!",
            new Vector2(0.04f, 0.4f), new Vector2(0.96f, 0.5f), TextAnchor.MiddleCenter);
        empty.gameObject.SetActive(false);
        return (list, empty);
    }

    void RefreshHallOfFameTab()
    {
        var records = GM.HallOfChampions;
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
        return UIFactory.CreateText(card, "", UIFactory.BodySize, UIFactory.CreamColor, TextAnchor.UpperLeft,
            new Vector2(0.05f, 0.04f), new Vector2(0.95f, 0.96f));
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
            $"Game Completions: {GM.TotalGameCompletions}";
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
        return UIFactory.CreateText(card, "", UIFactory.BodySize, UIFactory.CreamColor, TextAnchor.UpperLeft,
            new Vector2(0.05f, 0.04f), new Vector2(0.95f, 0.96f));
    }

    void RefreshRecordsTab()
    {
        recordsText.text =
            $"Longest Win Streak: {GM.BestWinStreak}\n" +
            $"Most Damage in a Single Hit: {GM.MaxSingleHitDamage}\n" +
            $"Most Fights Won: {GM.TotalWins}\n" +
            $"Most Coins Earned (Lifetime): {GM.TotalCoinsEarned}\n" +
            $"Most Submissions: {GM.SubmissionWins}\n\n" +
            $"Most Critical Hits: Future Expansion\n" +
            $"Most Combos Triggered: Future Expansion";
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

        ShowTab(0);
    }
}
