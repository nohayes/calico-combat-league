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

        UIFactory.CreateCard(Root.transform, "Stats", new Vector2(0.06f, 0.32f), new Vector2(0.94f, 0.66f));
        statsText = UIFactory.CreateText(Root.transform, "", UIFactory.CaptionSize, UIFactory.CreamColor, TextAnchor.UpperLeft,
            new Vector2(0.09f, 0.33f), new Vector2(0.91f, 0.65f));

        UIFactory.CreateCard(Root.transform, "Status", new Vector2(0.06f, 0.16f), new Vector2(0.94f, 0.3f));
        statusText = UIFactory.CreateText(Root.transform, "", UIFactory.BodySize, UIFactory.GoldColor, TextAnchor.MiddleCenter,
            new Vector2(0.08f, 0.17f), new Vector2(0.92f, 0.29f), FontStyle.Bold);

        // Profile is the management hub: Stats, Moves, and Hall of Fame are all
        // reachable from here (Hall of Fame and Moves no longer have their own
        // buttons on the Home screen).
        UIFactory.CreateButton(Root.transform, "STATS", new Vector2(0.03f, 0.03f), new Vector2(0.25f, 0.13f),
            () => GM.ChangeState(GameState.StatsScreen), UIFactory.SecondaryColor);
        UIFactory.CreateButton(Root.transform, "MOVES", new Vector2(0.27f, 0.03f), new Vector2(0.49f, 0.13f),
            () => GM.ChangeState(GameState.MovesScreen), UIFactory.SecondaryColor);
        UIFactory.CreateButton(Root.transform, "HALL OF FAME", new Vector2(0.51f, 0.03f), new Vector2(0.73f, 0.13f),
            () => GM.ChangeState(GameState.HallOfChampionsScreen), UIFactory.SecondaryColor);
        UIFactory.CreateButton(Root.transform, "BACK", new Vector2(0.75f, 0.03f), new Vector2(0.97f, 0.13f),
            () => GM.ChangeState(GameState.GymMap), UIFactory.SecondaryColor);
    }

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
        UIFactory.AddDisciplineBadge(portraitFrame, IconFactory.GetArchetypeIconShape(GM.Player.Archetype), theme);

        bool isChampion = GM.HasBecomeChampion();
        SetChampionBadge(isChampion);

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

        statusText.text = isChampion ? "CHAMPION" : "Not yet champion";
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
