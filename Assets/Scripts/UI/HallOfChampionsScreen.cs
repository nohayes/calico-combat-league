using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HallOfChampionsScreen : UIScreen
{
    readonly Transform listContainer;
    readonly Text emptyText;
    readonly List<GameObject> dynamicEntries = new List<GameObject>();

    public HallOfChampionsScreen(Transform parent, GameManager gm) : base(parent, gm, "HallOfChampionsScreen", "gym_map")
    {
        UIFactory.CreateHeading(Root.transform, "HALL OF CHAMPIONS", new Vector2(0.05f, 0.92f), new Vector2(0.95f, 0.99f));

        // Milestone 28: narrowed to a centered column (was edge-to-edge).
        listContainer = UIFactory.CreateContainer(Root.transform, new Vector2(0.15f, 0.14f), new Vector2(0.85f, 0.9f));

        emptyText = UIFactory.CreateCaption(Root.transform, "No champions yet - be the first!",
            new Vector2(0.15f, 0.45f), new Vector2(0.85f, 0.55f), TextAnchor.MiddleCenter);
        emptyText.gameObject.SetActive(false);

        UIFactory.CreateButton(Root.transform, "BACK", new Vector2(0.05f, 0.03f), new Vector2(0.46f, 0.12f),
            () => GM.ChangeState(GameState.GymMap), UIFactory.SecondaryColor);
        UIFactory.CreateButton(Root.transform, "ACHIEVEMENTS", new Vector2(0.54f, 0.03f), new Vector2(0.95f, 0.12f),
            () => GM.ChangeState(GameState.AchievementsScreen), UIFactory.SecondaryColor);
    }

    public void Refresh()
    {
        foreach (var entry in dynamicEntries) Object.Destroy(entry);
        dynamicEntries.Clear();

        var records = GM.HallOfChampions;
        emptyText.gameObject.SetActive(records.Count == 0);
        if (records.Count == 0) return;

        // Most recent champion first.
        for (int i = records.Count - 1, row = 0; i >= 0; i--, row++)
        {
            BuildRow(records[i], row, records.Count);
        }
    }

    void BuildRow(ChampionRecord record, int index, int total)
    {
        float slotHeight = 1f / total;
        float padding = slotHeight * 0.12f;
        float yMax = 1f - index * slotHeight - padding;
        float yMin = 1f - (index + 1) * slotHeight + padding;

        var card = UIFactory.CreateCard(listContainer, $"Champ_{index}", new Vector2(0f, yMin), new Vector2(1f, yMax));
        dynamicEntries.Add(card.gameObject);

        var medalGo = new GameObject("Medal", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        medalGo.transform.SetParent(card, false);
        var medalRt = medalGo.GetComponent<RectTransform>();
        medalRt.anchorMin = new Vector2(0.02f, 0.18f);
        medalRt.anchorMax = new Vector2(0.13f, 0.82f);
        medalRt.offsetMin = Vector2.zero;
        medalRt.offsetMax = Vector2.zero;
        var medalImage = medalGo.GetComponent<Image>();
        medalImage.sprite = IconFactory.GetShapeSprite(IconShape.Star);
        medalImage.color = index == 0 ? UIFactory.GoldColor : UIFactory.MutedTextColor;

        string nameLine = !string.IsNullOrEmpty(record.Title) ? $"{record.FighterName}  \"{record.Title}\"" : record.FighterName;
        UIFactory.CreateText(card, nameLine, UIFactory.BodySize, UIFactory.GoldColor, TextAnchor.MiddleLeft,
            new Vector2(0.17f, 0.5f), new Vector2(0.6f, 0.92f), FontStyle.Bold);

        UIFactory.CreateCaption(card, $"{record.Archetype}  -  Level {record.FinalLevel}", new Vector2(0.17f, 0.08f), new Vector2(0.6f, 0.5f));

        UIFactory.CreateCaption(card, record.CompletionDate, new Vector2(0.6f, 0f), new Vector2(0.97f, 1f), TextAnchor.MiddleRight);
    }
}
