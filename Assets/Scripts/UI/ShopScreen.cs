using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopScreen : UIScreen
{
    readonly Text headerText;
    readonly Transform listContainer;
    readonly List<GameObject> dynamicEntries = new List<GameObject>();
    readonly Image avatarImage;
    readonly PlayerAvatarVisual avatarVisual;

    public ShopScreen(Transform parent, GameManager gm) : base(parent, gm, "ShopScreen", "gym_map")
    {
        UIFactory.CreateHeading(Root.transform, "SHOP", new Vector2(0.05f, 0.9f), new Vector2(0.76f, 0.98f));

        // Milestone 25, Part 4: small fighter-identity badge.
        var avatarMarker = UIFactory.CreateAvatarMarker(Root.transform, "Player", new Vector2(0.79f, 0.9f), new Vector2(0.95f, 0.98f), out avatarImage);
        avatarVisual = avatarMarker.gameObject.AddComponent<PlayerAvatarVisual>();

        headerText = UIFactory.CreateText(Root.transform, "", UIFactory.SubheadingSize, UIFactory.GoldColor, TextAnchor.MiddleCenter,
            new Vector2(0.15f, 0.82f), new Vector2(0.85f, 0.89f));

        // Milestone 28: narrowed to a centered column (was edge-to-edge).
        listContainer = UIFactory.CreateContainer(Root.transform, new Vector2(0.15f, 0.15f), new Vector2(0.85f, 0.8f));

        UIFactory.CreateButton(Root.transform, "BACK", new Vector2(0.3f, 0.03f), new Vector2(0.7f, 0.12f),
            () => GM.ChangeState(GameState.GymMap), UIFactory.SecondaryColor);
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

        headerText.text = $"Coins: {GM.Player.Stats.Coins}";

        Color theme = IconFactory.GetArchetypeThemeColor(GM.Player.Archetype);
        avatarVisual.Initialize(avatarImage, GM.Player.Archetype, theme, faceRight: true);

        var items = ItemDatabase.All;
        for (int i = 0; i < items.Count; i++)
        {
            BuildItemRow(items[i], i, items.Count);
        }
    }

    void BuildItemRow(ItemData item, int index, int totalSlots)
    {
        float slotHeight = 1f / totalSlots;
        float padding = slotHeight * 0.12f;
        float yMax = 1f - index * slotHeight - padding;
        float yMin = 1f - (index + 1) * slotHeight + padding;

        var card = UIFactory.CreateCard(listContainer, $"Item_{item.Id}", new Vector2(0f, yMin), new Vector2(1f, yMax));
        dynamicEntries.Add(card.gameObject);

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconGo.transform.SetParent(card, false);
        var iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.02f, 0.2f);
        iconRt.anchorMax = new Vector2(0.13f, 0.8f);
        iconRt.offsetMin = Vector2.zero;
        iconRt.offsetMax = Vector2.zero;
        var iconImage = iconGo.GetComponent<Image>();
        var realIcon = ArtRegistry.GetItemIcon(item.Id);
        iconImage.sprite = realIcon != null ? realIcon : IconFactory.GetShapeSprite(IconFactory.GetItemIconShape(item.EffectType));
        iconImage.color = UIFactory.AccentOrange;

        int owned = GM.GetItemQuantity(item.Id);
        var label = UIFactory.CreateText(card, $"{item.Name}  (owned: {owned})\n{item.Description}", UIFactory.CaptionSize,
            UIFactory.CreamColor, TextAnchor.MiddleLeft, new Vector2(0.17f, 0f), new Vector2(0.6f, 1f));
        dynamicEntries.Add(label.gameObject);

        var buyButton = UIFactory.CreateButton(card, $"Buy {item.Cost}c", new Vector2(0.63f, 0.14f), new Vector2(0.97f, 0.86f),
            () => { GM.BuyItem(item.Id); Refresh(); }, UIFactory.PositiveColor);
        buyButton.interactable = GM.Player.Stats.Coins >= item.Cost;
        dynamicEntries.Add(buyButton.gameObject);
    }
}
