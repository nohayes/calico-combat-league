using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsScreen : UIScreen
{
    static readonly StatKind[] AllStats =
    {
        StatKind.Strength, StatKind.Defense, StatKind.Speed,
        StatKind.Striking, StatKind.Grappling, StatKind.Submission
    };

    readonly Text headerText;
    readonly Transform statsContainer;
    readonly List<GameObject> dynamicEntries = new List<GameObject>();

    public StatsScreen(Transform parent, GameManager gm) : base(parent, gm, "StatsScreen", "gym_map")
    {
        UIFactory.CreateHeading(Root.transform, "FIGHTER STATS", new Vector2(0.05f, 0.92f), new Vector2(0.76f, 0.99f));

        // Milestone 28: narrowed to a centered column (was edge-to-edge, a
        // portrait-era width that reads as an empty stretched strip on 16:9).
        UIFactory.CreateCard(Root.transform, "Summary", new Vector2(0.15f, 0.78f), new Vector2(0.85f, 0.91f));
        headerText = UIFactory.CreateText(Root.transform, "", UIFactory.BodySize, UIFactory.CreamColor, TextAnchor.MiddleLeft,
            new Vector2(0.18f, 0.78f), new Vector2(0.82f, 0.91f));

        statsContainer = UIFactory.CreateContainer(Root.transform, new Vector2(0.15f, 0.14f), new Vector2(0.85f, 0.76f));

        UIFactory.CreateButton(Root.transform, "BACK", new Vector2(0.3f, 0.03f), new Vector2(0.7f, 0.12f),
            () => GM.ChangeState(GameState.GymMap), UIFactory.SecondaryColor, isBackAction: true);
    }

    public void Refresh()
    {
        foreach (var entry in dynamicEntries) Object.Destroy(entry);
        dynamicEntries.Clear();

        if (GM.Player == null)
        {
            headerText.text = "No fighter data.";
            return;
        }

        var stats = GM.Player.Stats;
        headerText.text =
            $"Level {stats.Level}    XP: {stats.XP} / {stats.XPToNextLevel}\nCoins: {stats.Coins}    Stat Points: {stats.StatPoints}";

        for (int i = 0; i < AllStats.Length; i++)
        {
            BuildStatRow(AllStats[i], i);
        }
    }

    void BuildStatRow(StatKind kind, int index)
    {
        var stats = GM.Player.Stats;
        float slotHeight = 1f / AllStats.Length;
        float padding = slotHeight * 0.15f;
        float yMax = 1f - index * slotHeight - padding;
        float yMin = 1f - (index + 1) * slotHeight + padding;

        var card = UIFactory.CreateCard(statsContainer, $"Row_{kind}", new Vector2(0f, yMin), new Vector2(1f, yMax));
        dynamicEntries.Add(card.gameObject);

        var label = UIFactory.CreateText(card, $"{kind}\n{stats.GetStat(kind)}", UIFactory.BodySize, UIFactory.CreamColor, TextAnchor.MiddleLeft,
            new Vector2(0.04f, 0f), new Vector2(0.42f, 1f));
        dynamicEntries.Add(label.gameObject);

        var pointButton = UIFactory.CreateButton(card, "+1 Point", new Vector2(0.45f, 0.12f), new Vector2(0.7f, 0.88f),
            () =>
            {
                if (GM.Player.Stats.SpendStatPoint(kind)) GM.SaveGame();
                Refresh();
            }, UIFactory.AccentOrange);
        pointButton.interactable = stats.StatPoints > 0;
        // Overnight Audit: a dimmed button alone didn't say WHY it's
        // unclickable - swapping the label to name the blocker directly
        // (no stat points) is clearer than making the player cross-reference
        // the header's "Stat Points: X" line themselves.
        if (!pointButton.interactable) pointButton.GetComponentInChildren<Text>().text = "No Points";
        dynamicEntries.Add(pointButton.gameObject);

        bool canAfford = stats.Coins >= FighterStats.TrainingCost;
        // Milestone 50, Part 5/6: was PositiveColor (green) - training is an
        // action, not a value comparison/reward.
        var trainButton = UIFactory.CreateButton(card, $"Train {FighterStats.TrainingCost}c", new Vector2(0.73f, 0.12f), new Vector2(0.98f, 0.88f),
            () =>
            {
                if (GM.Player.Stats.TrainStat(kind))
                {
                    GM.RecordCoinsSpent(FighterStats.TrainingCost);
                    GM.SaveGame();
                }
                Refresh();
            }, UIFactory.AccentOrange);
        trainButton.interactable = canAfford;
        if (!canAfford) trainButton.GetComponentInChildren<Text>().text = $"Need {FighterStats.TrainingCost}c";
        dynamicEntries.Add(trainButton.gameObject);
    }
}
