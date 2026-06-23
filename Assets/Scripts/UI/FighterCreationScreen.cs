using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FighterCreationScreen : UIScreen
{
    readonly InputField nameInput;
    readonly Transform archetypeContainer;
    readonly Text archetypeDescription;
    readonly List<GameObject> archetypeButtons = new List<GameObject>();
    ArchetypeType selectedArchetype;

    public FighterCreationScreen(Transform parent, GameManager gm) : base(parent, gm, "FighterCreationScreen", "main_menu")
    {
        UIFactory.CreateHeading(Root.transform, "CREATE YOUR FIGHTER", new Vector2(0.06f, 0.89f), new Vector2(0.94f, 0.97f));

        nameInput = UIFactory.CreateInputField(Root.transform, "Enter fighter name",
            new Vector2(0.12f, 0.79f), new Vector2(0.88f, 0.87f));

        UIFactory.CreateCaption(Root.transform, "Choose your archetype:",
            new Vector2(0.1f, 0.72f), new Vector2(0.9f, 0.78f), TextAnchor.MiddleCenter);

        archetypeContainer = UIFactory.CreateContainer(Root.transform, new Vector2(0.06f, 0.46f), new Vector2(0.94f, 0.71f));

        archetypeDescription = UIFactory.CreateCaption(Root.transform, "", new Vector2(0.08f, 0.34f), new Vector2(0.92f, 0.45f), TextAnchor.MiddleCenter);

        UIFactory.CreateButton(Root.transform, "BEGIN CAREER", new Vector2(0.22f, 0.16f), new Vector2(0.78f, 0.27f),
            () => GM.StartNewGame(nameInput.text, selectedArchetype));

        BuildArchetypeButtons();

        if (ArchetypeDatabase.All.Count > 0) SelectArchetype(ArchetypeDatabase.All[0].Type);
    }

    void BuildArchetypeButtons()
    {
        var archetypes = ArchetypeDatabase.All;
        for (int i = 0; i < archetypes.Count; i++)
        {
            var info = archetypes[i];
            float slotWidth = 1f / archetypes.Count;
            float padding = slotWidth * 0.08f;
            float xMin = i * slotWidth + padding;
            float xMax = (i + 1) * slotWidth - padding;

            var button = UIFactory.CreateButton(archetypeContainer, info.DisplayName, new Vector2(xMin, 0f), new Vector2(xMax, 1f),
                () => SelectArchetype(info.Type), UIFactory.SecondaryColor);

            var label = button.GetComponentInChildren<Text>();
            label.rectTransform.anchorMin = new Vector2(0.03f, 0.02f);
            label.rectTransform.anchorMax = new Vector2(0.97f, 0.34f);

            Color theme = IconFactory.GetArchetypeThemeColor(info.Type);
            UIFactory.CreateFighterThumbnail(button.transform, null, info.Type, theme,
                new Vector2(0.18f, 0.38f), new Vector2(0.82f, 0.96f));
            archetypeButtons.Add(button.gameObject);
        }
    }

    void SelectArchetype(ArchetypeType type)
    {
        selectedArchetype = type;
        var info = ArchetypeDatabase.GetByType(type);
        archetypeDescription.text = info != null
            ? (!string.IsNullOrEmpty(info.FlavorQuote) ? $"\"{info.FlavorQuote}\"\n{info.Description}" : info.Description)
            : "";

        var archetypes = ArchetypeDatabase.All;
        for (int i = 0; i < archetypes.Count && i < archetypeButtons.Count; i++)
        {
            var image = archetypeButtons[i].GetComponent<Image>();
            image.color = archetypes[i].Type == type ? UIFactory.AccentOrange : UIFactory.SecondaryColor;
        }
    }
}
