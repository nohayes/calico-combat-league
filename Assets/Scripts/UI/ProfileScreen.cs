using UnityEngine;
using UnityEngine.UI;

public class ProfileScreen : UIScreen
{
    readonly RectTransform portraitFrame;
    readonly Image portraitImage;
    readonly Text headerText;
    readonly Text flavorQuoteText;
    readonly Text statsText;
    readonly Text statusText;
    readonly Button prestigeButton;
    readonly RectTransform prestigeConfirmPanel;

    public ProfileScreen(Transform parent, GameManager gm) : base(parent, gm, "ProfileScreen", "gym_map")
    {
        UIFactory.CreateHeading(Root.transform, "FIGHTER PROFILE", new Vector2(0.05f, 0.92f), new Vector2(0.95f, 0.99f));

        portraitFrame = UIFactory.CreateCard(Root.transform, "Portrait", new Vector2(0.06f, 0.74f), new Vector2(0.28f, 0.91f), UIFactory.BackgroundColor);

        var portraitGo = new GameObject("PortraitImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        portraitGo.transform.SetParent(portraitFrame, false);
        var portraitRt = portraitGo.GetComponent<RectTransform>();
        portraitRt.anchorMin = new Vector2(0.1f, 0.04f);
        portraitRt.anchorMax = new Vector2(0.9f, 0.96f);
        portraitRt.offsetMin = Vector2.zero;
        portraitRt.offsetMax = Vector2.zero;
        portraitImage = portraitGo.GetComponent<Image>();
        portraitImage.preserveAspect = true;

        headerText = UIFactory.CreateText(Root.transform, "", UIFactory.SubheadingSize, UIFactory.CreamColor, TextAnchor.MiddleLeft,
            new Vector2(0.31f, 0.74f), new Vector2(0.94f, 0.91f), FontStyle.Bold);

        flavorQuoteText = UIFactory.CreateText(Root.transform, "", UIFactory.CaptionSize, UIFactory.GoldColor, TextAnchor.MiddleCenter,
            new Vector2(0.06f, 0.68f), new Vector2(0.94f, 0.73f), FontStyle.Italic);
        // Quick Fix (Font Replacement Pass), Part 5: archetype flavor quotes
        // sit in a very short single-line band - PatrickHandSC-Regular's
        // wider glyphs raise the odds of wrapping past this box's height.
        flavorQuoteText.resizeTextForBestFit = true;
        flavorQuoteText.resizeTextMinSize = 10;
        flavorQuoteText.resizeTextMaxSize = UIFactory.CaptionSize;

        UIFactory.CreateCard(Root.transform, "Stats", new Vector2(0.06f, 0.32f), new Vector2(0.94f, 0.66f));
        statsText = UIFactory.CreateText(Root.transform, "", UIFactory.CaptionSize, UIFactory.CreamColor, TextAnchor.UpperLeft,
            new Vector2(0.09f, 0.33f), new Vector2(0.91f, 0.65f));

        UIFactory.CreateCard(Root.transform, "Status", new Vector2(0.06f, 0.16f), new Vector2(0.94f, 0.3f));
        statusText = UIFactory.CreateText(Root.transform, "", UIFactory.BodySize, UIFactory.GoldColor, TextAnchor.MiddleCenter,
            new Vector2(0.08f, 0.17f), new Vector2(0.92f, 0.29f), FontStyle.Bold);

        // Profile is the management hub: Stats, Moves, Career, and Hall of
        // Fame are all reachable from here (Hall of Fame and Moves no longer
        // have their own buttons on the Home screen).
        // Milestone 47, Part 1: row recomputed for 6 even columns (was 5) to
        // make room for CAREER - lowest-risk slot per the brief, since this
        // row already exists and just needed one more evenly-spaced column.
        // The standalone HALL OF FAME button/screen are left exactly as they
        // were ("do not remove existing screens") - Career's own Hall of Fame
        // tab is a cleaner presentation of the same underlying records, not a
        // replacement for the dedicated screen.
        UIFactory.CreateButton(Root.transform, "STATS", new Vector2(0.03f, 0.03f), new Vector2(0.172f, 0.13f),
            () => GM.ChangeState(GameState.StatsScreen), UIFactory.SecondaryColor);
        UIFactory.CreateButton(Root.transform, "MOVES", new Vector2(0.19f, 0.03f), new Vector2(0.332f, 0.13f),
            () => GM.ChangeState(GameState.MovesScreen), UIFactory.SecondaryColor);
        prestigeButton = UIFactory.CreateButton(Root.transform, "PRESTIGE", new Vector2(0.35f, 0.03f), new Vector2(0.492f, 0.13f),
            () => ShowPrestigeConfirm(), UIFactory.DangerColor);
        UIFactory.CreateButton(Root.transform, "CAREER", new Vector2(0.51f, 0.03f), new Vector2(0.652f, 0.13f),
            () => GM.ChangeState(GameState.CareerScreen), UIFactory.SecondaryColor);
        UIFactory.CreateButton(Root.transform, "HALL OF FAME", new Vector2(0.67f, 0.03f), new Vector2(0.812f, 0.13f),
            () => GM.ChangeState(GameState.HallOfChampionsScreen), UIFactory.SecondaryColor);
        UIFactory.CreateButton(Root.transform, "BACK", new Vector2(0.83f, 0.03f), new Vector2(0.97f, 0.13f),
            () => GM.ChangeState(GameState.GymMap), UIFactory.SecondaryColor, isBackAction: true);

        prestigeConfirmPanel = BuildPrestigeConfirmPanel();
    }

    // Milestone 45, Part 4: a major, irreversible action - requires an
    // explicit confirm step, built as a simple toggle-shown panel (same
    // pattern BattleScreen's item panel already uses) rather than a new
    // dialogue system. Lists exactly what's kept vs reset per the brief.
    RectTransform BuildPrestigeConfirmPanel()
    {
        var panel = UIFactory.CreateCard(Root.transform, "PrestigeConfirm", new Vector2(0.12f, 0.2f), new Vector2(0.88f, 0.8f),
            new Color(0.1f, 0.08f, 0.06f, 0.99f));

        UIFactory.CreateText(panel, "PRESTIGE - START THE NEXT LEAGUE", UIFactory.SubheadingSize, UIFactory.GoldColor,
            TextAnchor.MiddleCenter, new Vector2(0.04f, 0.84f), new Vector2(0.96f, 0.96f), FontStyle.Bold);

        UIFactory.CreateText(panel, "KEEP:  Level, XP, Archetype, Stats, Moves, Achievements,\nHall of Champions, Lifetime Statistics, Prestige Level",
            UIFactory.CaptionSize, UIFactory.PositiveColor, TextAnchor.MiddleCenter, new Vector2(0.06f, 0.56f), new Vector2(0.94f, 0.8f));

        UIFactory.CreateText(panel, "RESET:  Gym Progress, Defeated Opponents, Championship Progress,\nRival Progress, Mirror Match Progress, Current Run Progress",
            UIFactory.CaptionSize, UIFactory.DangerColor, TextAnchor.MiddleCenter, new Vector2(0.06f, 0.32f), new Vector2(0.94f, 0.56f));

        UIFactory.CreateButton(panel, "CONFIRM", new Vector2(0.1f, 0.06f), new Vector2(0.46f, 0.22f),
            () => { GM.PerformPrestige(); HidePrestigeConfirm(); Refresh(); }, UIFactory.DangerColor);
        UIFactory.CreateButton(panel, "CANCEL", new Vector2(0.54f, 0.06f), new Vector2(0.9f, 0.22f),
            () => HidePrestigeConfirm(), UIFactory.SecondaryColor, isBackAction: true);

        panel.gameObject.SetActive(false);
        return panel;
    }

    void ShowPrestigeConfirm()
    {
        if (!GM.CanPrestige) return;
        prestigeConfirmPanel.gameObject.SetActive(true);
        prestigeConfirmPanel.SetAsLastSibling();
    }

    void HidePrestigeConfirm() => prestigeConfirmPanel.gameObject.SetActive(false);

    public void Refresh()
    {
        if (GM.Player == null)
        {
            headerText.text = "No fighter data.";
            flavorQuoteText.text = "";
            statsText.text = "";
            statusText.text = "";
            return;
        }

        var info = ArchetypeDatabase.GetByType(GM.Player.Archetype);
        string archetypeName = info != null ? info.DisplayName : "Unspecified";

        headerText.text = $"{GM.Player.Name}\n{archetypeName}  -  Level {GM.Player.Stats.Level}";
        flavorQuoteText.text = info != null && !string.IsNullOrEmpty(info.FlavorQuote) ? $"\"{info.FlavorQuote}\"" : "";

        Color theme = IconFactory.GetArchetypeThemeColor(GM.Player.Archetype);
        UIFactory.SetFighterPortrait(portraitImage, "player", GM.Player.Archetype, theme);
        UIFactory.AddDisciplineBadge(portraitFrame, GM.Player.Archetype, theme);
        // Milestone 46, Part 4: large character display - Profile Screen.
        UIFactory.ApplyPrestigeTattoo(portraitImage, GM.PrestigeLevel);

        bool isChampion = GM.HasBecomeChampion();
        SetChampionBadge(isChampion);

        // Milestone 45, Part 3: only enabled (and only opens the confirm
        // panel) once Mirror Match is actually defeated.
        prestigeButton.interactable = GM.CanPrestige;
        HidePrestigeConfirm();

        int unlockedAchievements = 0;
        foreach (var a in AchievementDatabase.All) if (GM.IsAchievementUnlocked(a.Id)) unlockedAchievements++;

        statsText.text =
            $"Total Wins: {GM.TotalWins}        Total Losses: {GM.TotalLosses}\n" +
            $"Total Battles: {GM.TotalBattles}\n" +
            $"Damage Dealt: {GM.TotalDamageDealt}        Damage Taken: {GM.TotalDamageTaken}\n" +
            $"Coins Earned: {GM.TotalCoinsEarned}        Coins Spent: {GM.TotalCoinsSpent}\n" +
            $"Items Used: {GM.TotalItemsUsed}\n" +
            $"Gyms Cleared (this run): {GM.TotalGymsCleared} / {GymDatabase.AllGyms.Count}\n" +
            $"Achievements: {unlockedAchievements} / {AchievementDatabase.All.Count}";

        // Milestone 33, Part 1/2/6: the rival's recognizable identity (name,
        // archetype, motto) plus the Rival Tracker status and narrative record -
        // all read from RivalDatabase/GameManager, no new save state.
        string rivalArchetypeName = ArchetypeDatabase.GetByType(RivalDatabase.PortraitArchetype)?.DisplayName ?? "Fighter";
        // Milestone 45, Part 6: Prestige status shown alongside the existing
        // champion/rival summary - one consistent format (PrestigeSystem.FormatLevel)
        // plus the lightweight flavor label from Part 10, only when above 0.
        string prestigeLabel = PrestigeSystem.FormatLevel(GM.PrestigeLevel);
        string prestigeFlavor = PrestigeSystem.GetStatusLabel(GM.PrestigeLevel);
        string prestigeLine = string.IsNullOrEmpty(prestigeFlavor) ? prestigeLabel : $"{prestigeLabel}  -  {prestigeFlavor}";

        statusText.text = $"{(isChampion ? "CHAMPION" : "Not yet champion")}   -   {prestigeLine}\n" +
            $"RIVAL: {RivalDatabase.RivalName} ({rivalArchetypeName}) - {RivalDatabase.GetRivalStatus(GM)}\n" +
            $"Record: {RivalDatabase.GetRivalRecord(GM)}   -   \"{RivalDatabase.Motto}\"";
    }

    void SetChampionBadge(bool show)
    {
        var existing = portraitFrame.Find("ChampionBadge");
        if (existing != null) Object.Destroy(existing.gameObject);
        if (!show) return;

        var go = new GameObject("ChampionBadge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(portraitFrame, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.0f, 0.66f);
        rt.anchorMax = new Vector2(0.34f, 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var image = go.GetComponent<Image>();
        image.sprite = IconFactory.GetShapeSprite(IconShape.Star);
        image.color = UIFactory.GoldColor;
    }
}
