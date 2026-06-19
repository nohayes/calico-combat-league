using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GymScreen : UIScreen
{
    readonly Image banner;
    readonly Image bannerIcon;
    readonly Text bannerName;
    readonly Text bannerMotto;
    readonly Text bannerDescription;
    readonly Transform listContainer;
    readonly List<GameObject> dynamicEntries = new List<GameObject>();

    public GymScreen(Transform parent, GameManager gm) : base(parent, gm, "GymScreen")
    {
        var bannerRt = UIFactory.CreateCard(Root.transform, "Banner", new Vector2(0.05f, 0.78f), new Vector2(0.95f, 0.97f));
        banner = bannerRt.GetComponent<Image>();

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconGo.transform.SetParent(bannerRt, false);
        var iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.03f, 0.18f);
        iconRt.anchorMax = new Vector2(0.22f, 0.82f);
        iconRt.offsetMin = Vector2.zero;
        iconRt.offsetMax = Vector2.zero;
        bannerIcon = iconGo.GetComponent<Image>();

        bannerName = UIFactory.CreateText(bannerRt, "", UIFactory.SubheadingSize, UIFactory.CreamColor, TextAnchor.MiddleLeft,
            new Vector2(0.26f, 0.62f), new Vector2(0.97f, 0.94f), FontStyle.Bold);
        bannerMotto = UIFactory.CreateText(bannerRt, "", UIFactory.CaptionSize, UIFactory.GoldColor, TextAnchor.MiddleLeft,
            new Vector2(0.26f, 0.46f), new Vector2(0.97f, 0.6f), FontStyle.Italic);
        bannerDescription = UIFactory.CreateText(bannerRt, "", UIFactory.CaptionSize, UIFactory.CreamColor, TextAnchor.UpperLeft,
            new Vector2(0.26f, 0.06f), new Vector2(0.97f, 0.44f));

        listContainer = UIFactory.CreateContainer(Root.transform, new Vector2(0.08f, 0.14f), new Vector2(0.92f, 0.76f));

        UIFactory.CreateButton(Root.transform, "BACK TO MAP", new Vector2(0.25f, 0.04f), new Vector2(0.75f, 0.13f),
            () => GM.ChangeState(GameState.GymMap), UIFactory.SecondaryColor);
    }

    public void Refresh()
    {
        foreach (var entry in dynamicEntries) Object.Destroy(entry);
        dynamicEntries.Clear();

        var gym = GM.CurrentGym;
        if (gym == null || gym.Trainers == null || gym.Leader == null)
        {
            bannerName.text = "NO GYM SELECTED";
            bannerDescription.text = "";
            Debug.LogWarning("GymScreen.Refresh: current gym is missing or incomplete.");
            return;
        }

        UIFactory.ApplyScreenBackground(Root, $"{gym.GymId}_background");

        Color theme = IconFactory.GetGymThemeColor(gym.GymType);
        banner.color = theme;
        var realIcon = ArtRegistry.GetGymIcon(gym.GymId);
        bannerIcon.sprite = realIcon != null ? realIcon : IconFactory.GetShapeSprite(IconFactory.GetGymIconShape(gym.GymType));
        bannerIcon.color = UIFactory.CreamColor;

        bannerName.text = gym.GymName.ToUpper();
        bannerMotto.text = !string.IsNullOrEmpty(gym.Motto) ? $"\"{gym.Motto}\"" : "";
        bannerDescription.text = !string.IsNullOrEmpty(gym.Description) ? gym.Description : gym.GymType.ToString();

        int totalSlots = gym.Trainers.Count + 1;
        for (int i = 0; i < gym.Trainers.Count; i++)
        {
            var opponent = gym.Trainers[i];
            bool defeated = GM.IsOpponentDefeated(opponent);
            string nicknameTag = !string.IsNullOrEmpty(opponent.Nickname) ? $" \"{opponent.Nickname}\"" : "";
            string label = defeated ? $"{opponent.Name}{nicknameTag}  (Cleared)" : $"{opponent.Name}{nicknameTag}";
            Color color = defeated ? UIFactory.PositiveColor : UIFactory.AccentOrange;
            CreateOpponentButton(label, i, totalSlots, enabled: true, opponent: opponent, color: color);
        }

        bool leaderUnlocked = GM.IsLeaderUnlocked(gym);
        string rematchSuffix = GM.HasBecomeChampion() ? "  (Rematch)" : "";
        string leaderNickname = !string.IsNullOrEmpty(gym.Leader.Nickname) ? $" \"{gym.Leader.Nickname}\"" : "";
        string leaderLabel = leaderUnlocked ? $"LEADER: {gym.Leader.Name}{leaderNickname}{rematchSuffix}" : "LEADER (LOCKED)";
        Color leaderColor = leaderUnlocked ? UIFactory.GoldColor : UIFactory.LockedColor;
        CreateOpponentButton(leaderLabel, gym.Trainers.Count, totalSlots, enabled: leaderUnlocked, opponent: gym.Leader, color: leaderColor);
    }

    void CreateOpponentButton(string label, int index, int totalSlots, bool enabled, OpponentInfo opponent, Color color)
    {
        float slotHeight = 1f / totalSlots;
        float padding = slotHeight * 0.14f;
        float yMax = 1f - index * slotHeight - padding;
        float yMin = 1f - (index + 1) * slotHeight + padding;

        Color buttonColor = enabled ? color : UIFactory.LockedColor;
        var button = UIFactory.CreateButton(listContainer, label, new Vector2(0.05f, yMin), new Vector2(0.95f, yMax),
            () => GM.StartBattle(opponent), buttonColor);
        button.interactable = enabled;

        dynamicEntries.Add(button.gameObject);
    }
}
