using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementsScreen : UIScreen
{
    readonly Text headerText;
    readonly Transform listContainer;
    readonly List<GameObject> dynamicEntries = new List<GameObject>();

    public AchievementsScreen(Transform parent, GameManager gm) : base(parent, gm, "AchievementsScreen", "gym_map")
    {
        UIFactory.CreateHeading(Root.transform, "ACHIEVEMENTS", new Vector2(0.05f, 0.92f), new Vector2(0.95f, 0.99f));

        headerText = UIFactory.CreateText(Root.transform, "", UIFactory.SubheadingSize, UIFactory.GoldColor, TextAnchor.MiddleCenter,
            new Vector2(0.15f, 0.84f), new Vector2(0.85f, 0.91f));

        // Milestone 28: narrowed to a centered column (was edge-to-edge).
        listContainer = UIFactory.CreateContainer(Root.transform, new Vector2(0.15f, 0.14f), new Vector2(0.85f, 0.82f));

        UIFactory.CreateButton(Root.transform, "BACK", new Vector2(0.3f, 0.03f), new Vector2(0.7f, 0.12f),
            () => GM.ChangeState(GameState.GymMap), UIFactory.SecondaryColor);
    }

    public void Refresh()
    {
        foreach (var entry in dynamicEntries) Object.Destroy(entry);
        dynamicEntries.Clear();

        var achievements = AchievementDatabase.All;
        int unlockedCount = 0;
        foreach (var a in achievements) if (GM.IsAchievementUnlocked(a.Id)) unlockedCount++;
        headerText.text = $"{unlockedCount} / {achievements.Count} Unlocked";

        for (int i = 0; i < achievements.Count; i++)
        {
            BuildRow(achievements[i], i, achievements.Count);
        }
    }

    void BuildRow(AchievementData achievement, int index, int total)
    {
        float slotHeight = 1f / total;
        float padding = slotHeight * 0.12f;
        float yMax = 1f - index * slotHeight - padding;
        float yMin = 1f - (index + 1) * slotHeight + padding;

        bool unlocked = GM.IsAchievementUnlocked(achievement.Id);
        Color cardColor = unlocked ? new Color(0.16f, 0.32f, 0.18f, 1f) : UIFactory.LockedColor;

        var card = UIFactory.CreateCard(listContainer, $"Ach_{achievement.Id}", new Vector2(0f, yMin), new Vector2(1f, yMax), cardColor);
        dynamicEntries.Add(card.gameObject);

        string status = unlocked ? "UNLOCKED" : $"{Mathf.Min(GM.GetAchievementProgress(achievement.Metric), achievement.TargetValue)} / {achievement.TargetValue}";
        Color nameColor = unlocked ? UIFactory.GoldColor : UIFactory.CreamColor;

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconGo.transform.SetParent(card, false);
        var iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.02f, 0.2f);
        iconRt.anchorMax = new Vector2(0.13f, 0.8f);
        iconRt.offsetMin = Vector2.zero;
        iconRt.offsetMax = Vector2.zero;
        var iconImage = iconGo.GetComponent<Image>();
        var realIcon = ArtRegistry.GetAchievementIcon(achievement.Id);
        iconImage.sprite = realIcon != null ? realIcon : IconFactory.GetShapeSprite(IconFactory.GetAchievementIconShape(achievement.Metric));
        iconImage.preserveAspect = true;
        iconImage.color = realIcon != null
            ? (unlocked ? Color.white : new Color(0.45f, 0.45f, 0.45f, 0.7f))
            : (unlocked ? UIFactory.GoldColor : UIFactory.MutedTextColor);

        UIFactory.CreateText(card, IconFactory.GetAchievementCategory(achievement.Metric), UIFactory.CaptionSize,
            unlocked ? UIFactory.GoldColor : UIFactory.MutedTextColor, TextAnchor.MiddleLeft,
            new Vector2(0.17f, 0.7f), new Vector2(0.68f, 0.94f), FontStyle.Bold);

        UIFactory.CreateText(card, achievement.Name, UIFactory.BodySize, nameColor, TextAnchor.MiddleLeft,
            new Vector2(0.17f, 0.38f), new Vector2(0.68f, 0.72f), FontStyle.Bold);

        UIFactory.CreateCaption(card, achievement.Description, new Vector2(0.17f, 0.05f), new Vector2(0.68f, 0.4f));

        UIFactory.CreateText(card, status, UIFactory.CaptionSize, nameColor, TextAnchor.MiddleRight,
            new Vector2(0.68f, 0f), new Vector2(0.97f, 1f), FontStyle.Bold);
    }
}
