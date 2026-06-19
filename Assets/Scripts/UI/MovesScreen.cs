using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovesScreen : UIScreen
{
    readonly Transform equippedContainer;
    readonly Transform knownContainer;
    readonly List<GameObject> dynamicEntries = new List<GameObject>();

    public MovesScreen(Transform parent, GameManager gm) : base(parent, gm, "MovesScreen")
    {
        UIFactory.CreateHeading(Root.transform, "MOVES", new Vector2(0.06f, 0.9f), new Vector2(0.94f, 0.98f));

        UIFactory.CreateCaption(Root.transform, "EQUIPPED  (tap to unequip)", new Vector2(0.05f, 0.82f), new Vector2(0.95f, 0.88f));
        equippedContainer = UIFactory.CreateContainer(Root.transform, new Vector2(0.05f, 0.55f), new Vector2(0.95f, 0.82f));

        UIFactory.CreateCaption(Root.transform, "KNOWN MOVES  (tap to equip)", new Vector2(0.05f, 0.46f), new Vector2(0.95f, 0.52f));
        knownContainer = UIFactory.CreateContainer(Root.transform, new Vector2(0.05f, 0.15f), new Vector2(0.95f, 0.46f));

        UIFactory.CreateButton(Root.transform, "BACK", new Vector2(0.3f, 0.03f), new Vector2(0.7f, 0.12f),
            () => GM.ChangeState(GameState.GymMap), UIFactory.SecondaryColor);
    }

    public void Refresh()
    {
        foreach (var entry in dynamicEntries) Object.Destroy(entry);
        dynamicEntries.Clear();

        if (GM.Player == null)
        {
            Debug.LogWarning("MovesScreen.Refresh: no player data.");
            return;
        }

        var equipped = GM.Player.EquippedMoves;
        for (int i = 0; i < equipped.Count; i++)
        {
            var move = equipped[i];
            var button = BuildRow(equippedContainer, $"{move.Name}  ({move.StaminaCost} stam)", i, equipped.Count,
                () => { GM.Player.UnequipMove(move); Refresh(); }, UIFactory.PositiveColor, move);
            button.interactable = equipped.Count > 1;
        }

        var known = GM.Player.KnownMoves;
        for (int i = 0; i < known.Count; i++)
        {
            var move = known[i];
            bool isEquipped = equipped.Contains(move);
            string label = isEquipped ? $"{move.Name}  (Equipped)" : move.Name;
            bool canEquip = !isEquipped && equipped.Count < 4;

            Color color = isEquipped ? UIFactory.LockedColor : UIFactory.AccentOrange;
            var button = BuildRow(knownContainer, label, i, known.Count,
                () => { GM.Player.EquipMove(move); Refresh(); }, color, move);
            button.interactable = canEquip;
        }
    }

    Button BuildRow(Transform container, string label, int index, int totalSlots,
        UnityEngine.Events.UnityAction onClick, Color color, MoveData move)
    {
        float slotHeight = 1f / Mathf.Max(totalSlots, 1);
        float padding = slotHeight * 0.12f;
        float yMax = 1f - index * slotHeight - padding;
        float yMin = 1f - (index + 1) * slotHeight + padding;

        var button = UIFactory.CreateButton(container, label, new Vector2(0.05f, yMin), new Vector2(0.95f, yMax), onClick, color);
        dynamicEntries.Add(button.gameObject);

        // Make room for a small move-type icon on the left of the auto-generated label.
        var labelText = button.GetComponentInChildren<Text>();
        labelText.rectTransform.anchorMin = new Vector2(0.16f, 0f);
        labelText.rectTransform.anchorMax = new Vector2(0.97f, 1f);

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconGo.transform.SetParent(button.transform, false);
        var iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.03f, 0.22f);
        iconRt.anchorMax = new Vector2(0.14f, 0.78f);
        iconRt.offsetMin = Vector2.zero;
        iconRt.offsetMax = Vector2.zero;
        var iconImage = iconGo.GetComponent<Image>();
        var realIcon = ArtRegistry.GetMoveIcon(move.Id);
        iconImage.sprite = realIcon != null ? realIcon : IconFactory.GetShapeSprite(IconFactory.GetMoveTypeIconShape(move.Type));
        iconImage.color = UIFactory.CreamColor;

        return button;
    }
}
