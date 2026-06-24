using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Character Creation Redesign: four fighter sprites standing side-by-side
// (each its own spotlight + shadow + idle bob) instead of four buttons with
// small icons inside boxes - closer to a fighting game's character select
// than a menu. The archetype description panel below keeps its existing
// behavior unchanged.
public class FighterCreationScreen : UIScreen
{
    readonly InputField nameInput;
    readonly Text archetypeDescription;
    readonly List<FighterSlot> slots = new List<FighterSlot>();
    ArchetypeType selectedArchetype;

    // Curated stock sprites for this screen only - not a general "archetype
    // portrait" convention (that's ArtRegistry.GetArchetypePortrait), just a
    // specific pick of existing fighter art to stand in for each archetype
    // on the select screen.
    static string GetSelectFighterId(ArchetypeType type)
    {
        switch (type)
        {
            case ArchetypeType.Boxer: return "boxing_leader";
            case ArchetypeType.Wrestler: return "fighter_11";
            case ArchetypeType.BjjSpecialist: return "fighter_3";
            case ArchetypeType.MuayThaiFighter: return "muaythai_leader";
            default: return null;
        }
    }

    class FighterSlot
    {
        public ArchetypeType Type;
        public Image AmbientGlow;
        public Image OutlineGlow;
        public RectTransform ShadowRect;
        public RectTransform SpriteRect;
        public RectTransform RootRect;
        public float IdlePhase;
    }

    // Root is still inactive at construction time (UIScreen's base ctor
    // deactivates it after building), and Unity can't start a coroutine on an
    // inactive GameObject - so the idle-bob routines are started lazily on
    // the first Refresh() (called once Root is actually active) instead of
    // here. Guarded so re-entering this screen later doesn't stack a second
    // set of coroutines - Unity already pauses/resumes the existing ones
    // automatically whenever Root is deactivated/reactivated.
    bool idleStarted;

    public FighterCreationScreen(Transform parent, GameManager gm) : base(parent, gm, "FighterCreationScreen", "main_menu")
    {
        UIFactory.CreateHeading(Root.transform, "CREATE YOUR FIGHTER", new Vector2(0.06f, 0.89f), new Vector2(0.94f, 0.97f));

        nameInput = UIFactory.CreateInputField(Root.transform, "Enter fighter name",
            new Vector2(0.12f, 0.79f), new Vector2(0.88f, 0.87f));

        UIFactory.CreateCaption(Root.transform, "Choose your fighter:",
            new Vector2(0.1f, 0.74f), new Vector2(0.9f, 0.78f), TextAnchor.MiddleCenter);

        archetypeDescription = UIFactory.CreateCaption(Root.transform, "", new Vector2(0.08f, 0.18f), new Vector2(0.92f, 0.30f), TextAnchor.MiddleCenter);

        UIFactory.CreateButton(Root.transform, "BEGIN CAREER", new Vector2(0.22f, 0.09f), new Vector2(0.78f, 0.17f),
            () => GM.StartNewGame(nameInput.text, selectedArchetype));

        // Bug fix (World Polish Pass): this screen previously had no way back -
        // safe to keep since StartFreshGame no longer destroys the old save
        // before the player confirms a new fighter (see GameManager).
        UIFactory.CreateButton(Root.transform, "BACK", new Vector2(0.22f, 0.01f), new Vector2(0.78f, 0.08f),
            () => GM.ChangeState(GameState.MainMenu), UIFactory.SecondaryColor, isBackAction: true);

        BuildFighterSlots();

        if (ArchetypeDatabase.All.Count > 0) SelectArchetype(ArchetypeDatabase.All[0].Type);
    }

    void BuildFighterSlots()
    {
        var archetypes = ArchetypeDatabase.All;
        const float xMin = 0.04f, xMax = 0.96f, gap = 0.02f;
        float columnWidth = (xMax - xMin - (archetypes.Count - 1) * gap) / archetypes.Count;

        for (int i = 0; i < archetypes.Count; i++)
        {
            var info = archetypes[i];
            float slotXMin = xMin + i * (columnWidth + gap);
            var slotContainer = UIFactory.CreateContainer(Root.transform,
                new Vector2(slotXMin, 0.32f), new Vector2(slotXMin + columnWidth, 0.73f));

            slots.Add(BuildFighterSlot(slotContainer, info, i));
        }
    }

    FighterSlot BuildFighterSlot(Transform slotContainer, ArchetypeInfo info, int index)
    {
        Color theme = IconFactory.GetArchetypeThemeColor(info.Type);

        // Ambient spotlight - soft, always-on glow behind the fighter (the
        // "MMA arena spotlight" feel). Brightened on selection.
        var glowGo = new GameObject("Glow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        glowGo.transform.SetParent(slotContainer, false);
        var glowRt = glowGo.GetComponent<RectTransform>();
        glowRt.anchorMin = new Vector2(0f, 0.05f);
        glowRt.anchorMax = new Vector2(1f, 1f);
        glowRt.offsetMin = Vector2.zero;
        glowRt.offsetMax = Vector2.zero;
        var glowImage = glowGo.GetComponent<Image>();
        glowImage.sprite = UIFactory.GlowSprite;
        glowImage.color = new Color(1f, 0.78f, 0.32f, 0.16f);
        glowImage.raycastTarget = false;

        // Selection outline - a tighter, brighter glow hugging the sprite,
        // invisible until selected.
        var outlineGo = new GameObject("OutlineGlow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        outlineGo.transform.SetParent(slotContainer, false);
        var outlineRt = outlineGo.GetComponent<RectTransform>();
        outlineRt.anchorMin = new Vector2(0.12f, 0.18f);
        outlineRt.anchorMax = new Vector2(0.88f, 0.98f);
        outlineRt.offsetMin = Vector2.zero;
        outlineRt.offsetMax = Vector2.zero;
        var outlineImage = outlineGo.GetComponent<Image>();
        outlineImage.sprite = UIFactory.GlowSprite;
        outlineImage.color = new Color(1f, 0.85f, 0.4f, 0f);
        outlineImage.raycastTarget = false;

        // Oval shadow at the fighter's feet - same flattened-circle trick
        // CreateBattleFighter/CreateAvatarMarker already use.
        var shadowGo = new GameObject("Shadow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        shadowGo.transform.SetParent(slotContainer, false);
        var shadowRt = shadowGo.GetComponent<RectTransform>();
        shadowRt.anchorMin = new Vector2(0.26f, 0.20f);
        shadowRt.anchorMax = new Vector2(0.74f, 0.28f);
        shadowRt.offsetMin = Vector2.zero;
        shadowRt.offsetMax = Vector2.zero;
        var shadowImage = shadowGo.GetComponent<Image>();
        shadowImage.sprite = UIFactory.CircleSprite;
        shadowImage.color = new Color(0f, 0f, 0f, 0.4f);
        shadowImage.raycastTarget = false;

        // The fighter sprite itself - the star of the slot.
        var spriteGo = new GameObject("FighterSprite", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        spriteGo.transform.SetParent(slotContainer, false);
        var spriteRt = spriteGo.GetComponent<RectTransform>();
        spriteRt.anchorMin = new Vector2(0.08f, 0.24f);
        spriteRt.anchorMax = new Vector2(0.92f, 0.97f);
        spriteRt.offsetMin = Vector2.zero;
        spriteRt.offsetMax = Vector2.zero;
        var spriteImage = spriteGo.GetComponent<Image>();
        spriteImage.preserveAspect = true;
        spriteImage.raycastTarget = false;
        UIFactory.SetFighterPortrait(spriteImage, GetSelectFighterId(info.Type), info.Type, theme);
        // Milestone 46, Part 4: "Character Creation Preview (if technically
        // safe)" - technically safe (same shared helper, no archetype
        // dependency), but this screen is only ever reached via a true New
        // Game, which now resets PrestigeLevel to 0 (see GameManager), so in
        // practice this will never actually show a tattoo. Wired anyway for
        // consistency in case that ever changes.
        UIFactory.ApplyPrestigeTattoo(spriteImage, GM.PrestigeLevel);

        // Name, centered, directly under the fighter.
        var nameText = UIFactory.CreateText(slotContainer, info.DisplayName, UIFactory.BodySize, UIFactory.CreamColor,
            TextAnchor.MiddleCenter, new Vector2(0.0f, 0.12f), new Vector2(1f, 0.24f), FontStyle.Bold);
        nameText.resizeTextForBestFit = true;
        nameText.resizeTextMinSize = 14;
        nameText.resizeTextMaxSize = UIFactory.BodySize;
        nameText.raycastTarget = false;

        // Small archetype description under the name.
        var descText = UIFactory.CreateText(slotContainer, info.Description, UIFactory.CaptionSize, UIFactory.MutedTextColor,
            TextAnchor.MiddleCenter, new Vector2(0.02f, 0.0f), new Vector2(0.98f, 0.12f));
        descText.resizeTextForBestFit = true;
        descText.resizeTextMinSize = 9;
        descText.resizeTextMaxSize = UIFactory.CaptionSize;
        descText.raycastTarget = false;

        // The sprite itself is the selectable element - one transparent
        // click-catcher spanning the whole slot, on top, is simpler and more
        // forgiving to click than hit-testing the sprite's silhouette.
        var clickGo = new GameObject("ClickArea", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        clickGo.transform.SetParent(slotContainer, false);
        var clickRt = clickGo.GetComponent<RectTransform>();
        clickRt.anchorMin = Vector2.zero;
        clickRt.anchorMax = Vector2.one;
        clickRt.offsetMin = Vector2.zero;
        clickRt.offsetMax = Vector2.zero;
        var clickImage = clickGo.GetComponent<Image>();
        clickImage.color = new Color(0f, 0f, 0f, 0f);
        var clickButton = clickGo.GetComponent<Button>();
        clickButton.transition = Selectable.Transition.None;
        clickButton.targetGraphic = clickImage;
        clickButton.onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlayClick();
            SelectArchetype(info.Type);
        });

        return new FighterSlot
        {
            Type = info.Type,
            AmbientGlow = glowImage,
            OutlineGlow = outlineImage,
            ShadowRect = shadowRt,
            SpriteRect = spriteRt,
            RootRect = (RectTransform)slotContainer,
            IdlePhase = index * (Mathf.PI * 0.5f)
        };
    }

    // Called by UIManager whenever this screen becomes the active state.
    // Technical requirement: reuses the existing coroutine-on-screen pattern
    // (RunAnimation -> ScreenTransitionRunner) instead of adding a
    // MonoBehaviour Update() loop per fighter - same approach PlayPulse and
    // every other screen animation already uses. Started here (not in the
    // constructor) since Root must be active for StartCoroutine to work.
    public void Refresh()
    {
        if (idleStarted) return;
        idleStarted = true;
        for (int i = 0; i < slots.Count; i++)
            RunAnimation(IdleBobRoutine(slots[i].SpriteRect, slots[i].IdlePhase));
    }

    // Idle animation: a slow, continuous vertical sine bob only - no scale,
    // no rotation, no shake, per the brief. anchoredPosition starts at
    // whatever the sprite's stretched layout already resolved to, so this
    // only ever adds a small Y offset on top of it.
    IEnumerator IdleBobRoutine(RectTransform spriteRect, float phase)
    {
        const float bobAmount = 4f; // 3-5px per the brief
        const float speed = 2.2f; // slow cycle
        Vector2 basePos = spriteRect.anchoredPosition;
        float t = phase;
        while (true)
        {
            if (spriteRect == null) yield break;
            t += Time.unscaledDeltaTime * speed;
            spriteRect.anchoredPosition = basePos + new Vector2(0f, Mathf.Sin(t) * bobAmount);
            yield return null;
        }
    }

    void SelectArchetype(ArchetypeType type)
    {
        selectedArchetype = type;
        var info = ArchetypeDatabase.GetByType(type);
        archetypeDescription.text = info != null
            ? (!string.IsNullOrEmpty(info.FlavorQuote) ? $"\"{info.FlavorQuote}\"\n{info.Description}" : info.Description)
            : "";

        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            bool selected = slot.Type == type;

            slot.AmbientGlow.color = selected ? new Color(1f, 0.78f, 0.32f, 0.34f) : new Color(1f, 0.78f, 0.32f, 0.16f);
            slot.OutlineGlow.color = selected ? new Color(1f, 0.85f, 0.4f, 0.55f) : new Color(1f, 0.85f, 0.4f, 0f);
            slot.ShadowRect.localScale = selected ? Vector3.one * 1.15f : Vector3.one;
            slot.RootRect.localScale = selected ? Vector3.one * 1.05f : Vector3.one;
        }
    }
}
