using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovesScreen : UIScreen
{
    readonly Transform equippedContainer;
    readonly Transform knownContainer;
    readonly List<GameObject> dynamicEntries = new List<GameObject>();

    public MovesScreen(Transform parent, GameManager gm) : base(parent, gm, "MovesScreen", "gym_map")
    {
        UIFactory.CreateHeading(Root.transform, "MOVES", new Vector2(0.06f, 0.9f), new Vector2(0.74f, 0.98f));

        // Milestone 41, Part 6: containers widened slightly and the gap
        // between caption and container tightened, reclaiming a few points of
        // vertical space for each row now that rows carry three lines of
        // content (name/cost, tag+role, description) instead of one.
        UIFactory.CreateCaption(Root.transform, "EQUIPPED  (tap to unequip)", new Vector2(0.12f, 0.84f), new Vector2(0.88f, 0.89f));
        equippedContainer = UIFactory.CreateContainer(Root.transform, new Vector2(0.12f, 0.56f), new Vector2(0.88f, 0.84f));

        UIFactory.CreateCaption(Root.transform, "KNOWN MOVES  (tap to equip)", new Vector2(0.12f, 0.50f), new Vector2(0.88f, 0.55f));
        knownContainer = UIFactory.CreateContainer(Root.transform, new Vector2(0.12f, 0.15f), new Vector2(0.88f, 0.50f));

        UIFactory.CreateButton(Root.transform, "BACK", new Vector2(0.3f, 0.03f), new Vector2(0.7f, 0.12f),
            () => GM.ChangeState(GameState.GymMap), UIFactory.SecondaryColor, isBackAction: true);
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
            var button = BuildRow(equippedContainer, i, equipped.Count, move, isEquipped: true,
                interactable: equipped.Count > 1, () => { GM.Player.UnequipMove(move); Refresh(); });
            dynamicEntries.Add(button.gameObject);
        }

        var known = GM.Player.KnownMoves;
        for (int i = 0; i < known.Count; i++)
        {
            var move = known[i];
            bool isEquipped = equipped.Contains(move);
            bool canEquip = !isEquipped && equipped.Count < 4;

            var button = BuildRow(knownContainer, i, known.Count, move, isEquipped,
                interactable: canEquip, () => { GM.Player.EquipMove(move); Refresh(); });
            dynamicEntries.Add(button.gameObject);
        }
    }

    // Milestone 41, Part 1/2/6: each row now shows Name + Stamina Cost (line 1),
    // a colored [Tag] + short Role phrase (line 2), and the full Description
    // (line 3) - reuses CreateCardButton/CreateCard/CreateText exactly the way
    // GymSelectionScreen's BuildCard already does, no new visual primitives.
    Button BuildRow(Transform container, int index, int totalSlots, MoveData move, bool isEquipped, bool interactable,
        UnityEngine.Events.UnityAction onClick)
    {
        float slotHeight = 1f / Mathf.Max(totalSlots, 1);
        float padding = slotHeight * 0.08f;
        float yMax = 1f - index * slotHeight - padding;
        float yMin = 1f - (index + 1) * slotHeight + padding;

        Color tagColor = IconFactory.GetMoveCategoryColor(move.Category);
        var border = UIFactory.CreateCardButton(container, move.Name, new Vector2(0f, yMin), new Vector2(1f, yMax), onClick, tagColor);
        border.interactable = interactable;

        var fill = UIFactory.CreateCard(border.transform, move.Name + "Fill", new Vector2(0.01f, 0.07f), new Vector2(0.99f, 0.93f),
            new Color(0.07f, 0.06f, 0.06f, 0.96f));
        fill.GetComponent<Image>().raycastTarget = false;

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconGo.transform.SetParent(fill, false);
        var iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.015f, 0.2f);
        iconRt.anchorMax = new Vector2(0.1f, 0.8f);
        iconRt.offsetMin = Vector2.zero;
        iconRt.offsetMax = Vector2.zero;
        var iconImage = iconGo.GetComponent<Image>();
        var realIcon = ArtRegistry.GetMoveIcon(move.Id);
        iconImage.sprite = realIcon != null ? realIcon : IconFactory.GetShapeSprite(IconFactory.GetMoveTypeIconShape(move.Type));
        iconImage.preserveAspect = true;
        iconImage.color = realIcon != null ? Color.white : IconFactory.GetMoveTypeThemeColor(move.Type);
        iconImage.raycastTarget = false;

        // Line 1: name + stamina cost + equipped marker - the primary, most
        // important line, biggest text in the row.
        string headline = isEquipped ? $"{move.Name}  ({move.StaminaCost} stam)  -  EQUIPPED" : $"{move.Name}  ({move.StaminaCost} stam)";
        var nameText = UIFactory.CreateText(fill, headline, UIFactory.BodySize, UIFactory.CreamColor, TextAnchor.MiddleLeft,
            new Vector2(0.13f, 0.64f), new Vector2(0.98f, 0.95f), FontStyle.Bold);
        nameText.resizeTextForBestFit = true;
        nameText.resizeTextMinSize = 11;
        nameText.resizeTextMaxSize = UIFactory.BodySize;
        nameText.raycastTarget = false;

        // Line 2: [Tag] + Role - the tactical-identity line (Part 1/2), tinted
        // by category so the bracketed tag reads as a small, scannable badge.
        string tagLine = $"[{IconFactory.GetMoveCategoryLabel(move.Category)}]  {move.Role}";
        var roleText = UIFactory.CreateText(fill, tagLine, UIFactory.CaptionSize, tagColor, TextAnchor.MiddleLeft,
            new Vector2(0.13f, 0.34f), new Vector2(0.98f, 0.62f), FontStyle.Bold);
        roleText.resizeTextForBestFit = true;
        roleText.resizeTextMinSize = 9;
        roleText.resizeTextMaxSize = UIFactory.CaptionSize;
        roleText.raycastTarget = false;

        // Line 3: the full flavor/tactical description.
        var descText = UIFactory.CreateText(fill, move.Description, UIFactory.CaptionSize, UIFactory.MutedTextColor, TextAnchor.MiddleLeft,
            new Vector2(0.13f, 0.04f), new Vector2(0.98f, 0.32f));
        descText.resizeTextForBestFit = true;
        descText.resizeTextMinSize = 8;
        descText.resizeTextMaxSize = UIFactory.CaptionSize;
        descText.raycastTarget = false;

        return border;
    }
}
