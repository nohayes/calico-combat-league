using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HallOfChampionsScreen : UIScreen
{
    readonly Transform listContainer;
    readonly Text emptyText;
    readonly List<GameObject> dynamicEntries = new List<GameObject>();

    public HallOfChampionsScreen(Transform parent, GameManager gm) : base(parent, gm, "HallOfChampionsScreen")
    {
        UIFactory.CreateHeading(Root.transform, "HALL OF CHAMPIONS", new Vector2(0.05f, 0.92f), new Vector2(0.95f, 0.99f));

        listContainer = UIFactory.CreateContainer(Root.transform, new Vector2(0.05f, 0.14f), new Vector2(0.95f, 0.9f));

        emptyText = UIFactory.CreateCaption(Root.transform, "No champions yet - be the first!",
            new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.55f), TextAnchor.MiddleCenter);
        emptyText.gameObject.SetActive(false);

        UIFactory.CreateButton(Root.transform, "BACK", new Vector2(0.3f, 0.03f), new Vector2(0.7f, 0.12f),
            () => GM.ChangeState(GameState.GymMap), UIFactory.SecondaryColor);
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

        UIFactory.CreateText(card, record.FighterName, UIFactory.BodySize, UIFactory.GoldColor, TextAnchor.MiddleLeft,
            new Vector2(0.17f, 0.5f), new Vector2(0.6f, 0.92f), FontStyle.Bold);

        UIFactory.CreateCaption(card, $"{record.Archetype}  -  Level {record.FinalLevel}", new Vector2(0.17f, 0.08f), new Vector2(0.6f, 0.5f));

        UIFactory.CreateCaption(card, record.CompletionDate, new Vector2(0.6f, 0f), new Vector2(0.97f, 1f), TextAnchor.MiddleRight);
    }
}
