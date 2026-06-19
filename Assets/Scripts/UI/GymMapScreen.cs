using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GymMapScreen : UIScreen
{
    readonly Transform listContainer;
    readonly List<GameObject> dynamicEntries = new List<GameObject>();

    public GymMapScreen(Transform parent, GameManager gm) : base(parent, gm, "GymMapScreen", "gym_map")
    {
        UIFactory.CreateBrandHeader(Root.transform, new Vector2(0.05f, 0.86f), new Vector2(0.95f, 0.99f));

        UIFactory.CreateButton(Root.transform, "MOVES", new Vector2(0.05f, 0.78f), new Vector2(0.34f, 0.85f),
            () => GM.ChangeState(GameState.MovesScreen), UIFactory.SecondaryColor);

        UIFactory.CreateButton(Root.transform, "STATS", new Vector2(0.36f, 0.78f), new Vector2(0.65f, 0.85f),
            () => GM.ChangeState(GameState.StatsScreen), UIFactory.SecondaryColor);

        UIFactory.CreateButton(Root.transform, "SHOP", new Vector2(0.67f, 0.78f), new Vector2(0.95f, 0.85f),
            () => GM.ChangeState(GameState.ShopScreen), UIFactory.SecondaryColor);

        UIFactory.CreateButton(Root.transform, "PROFILE", new Vector2(0.05f, 0.705f), new Vector2(0.34f, 0.775f),
            () => GM.ChangeState(GameState.ProfileScreen), UIFactory.SecondaryColor);

        UIFactory.CreateButton(Root.transform, "ACHIEVEMENTS", new Vector2(0.36f, 0.705f), new Vector2(0.65f, 0.775f),
            () => GM.ChangeState(GameState.AchievementsScreen), UIFactory.SecondaryColor);

        UIFactory.CreateButton(Root.transform, "HALL OF FAME", new Vector2(0.67f, 0.705f), new Vector2(0.95f, 0.775f),
            () => GM.ChangeState(GameState.HallOfChampionsScreen), UIFactory.SecondaryColor);

        listContainer = UIFactory.CreateContainer(Root.transform, new Vector2(0.08f, 0.05f), new Vector2(0.92f, 0.69f));
    }

    public void Refresh()
    {
        foreach (var entry in dynamicEntries) Object.Destroy(entry);
        dynamicEntries.Clear();

        var gyms = GymDatabase.AllGyms;
        if (gyms == null || gyms.Count == 0)
        {
            Debug.LogWarning("GymMapScreen.Refresh: no gyms found in GymDatabase.");
            return;
        }

        for (int i = 0; i < gyms.Count; i++)
        {
            BuildGymRow(gyms[i], i, gyms.Count);
        }
    }

    void BuildGymRow(GymInfo gym, int index, int totalGyms)
    {
        bool unlocked = GM.IsGymUnlocked(gym);
        bool completed = GM.IsGymCompleted(gym);
        string tagline = completed ? "Cleared" : (unlocked && !string.IsNullOrEmpty(gym.Motto) ? gym.Motto : (unlocked ? "Available" : "Locked"));
        string label = $"{gym.GymName}\n{tagline}";

        float slotHeight = 1f / totalGyms;
        float padding = slotHeight * 0.14f;
        float yMax = 1f - index * slotHeight - padding;
        float yMin = 1f - (index + 1) * slotHeight + padding;

        Color color = completed ? UIFactory.PositiveColor : (unlocked ? IconFactory.GetGymThemeColor(gym.GymType) : UIFactory.LockedColor);
        var button = UIFactory.CreateButton(listContainer, label, new Vector2(0.06f, yMin), new Vector2(0.94f, yMax),
            () => GM.EnterGym(gym), color);
        button.interactable = unlocked;
        dynamicEntries.Add(button.gameObject);

        // Make room for an icon on the left of the auto-generated label.
        var labelText = button.GetComponentInChildren<Text>();
        labelText.rectTransform.anchorMin = new Vector2(0.24f, 0f);
        labelText.rectTransform.anchorMax = new Vector2(0.97f, 1f);
        labelText.alignment = TextAnchor.MiddleLeft;

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconGo.transform.SetParent(button.transform, false);
        var iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.04f, 0.18f);
        iconRt.anchorMax = new Vector2(0.2f, 0.82f);
        iconRt.offsetMin = Vector2.zero;
        iconRt.offsetMax = Vector2.zero;
        var iconImage = iconGo.GetComponent<Image>();
        var realIcon = ArtRegistry.GetGymIcon(gym.GymId);
        iconImage.sprite = realIcon != null ? realIcon : IconFactory.GetShapeSprite(IconFactory.GetGymIconShape(gym.GymType));
        iconImage.color = unlocked ? Color.white : new Color(1f, 1f, 1f, 0.5f);
    }
}
