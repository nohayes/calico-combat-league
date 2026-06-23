using UnityEngine;
using UnityEngine.UI;

public static class UIFactory
{
    // Calico-inspired palette: black, cream, orange, gold. Values live in
    // Core/VisualTheme.cs (the single source of truth) - these are aliases so
    // every existing UIFactory.XColor call site keeps working unchanged.
    public static readonly Color BackgroundColor = VisualTheme.BackgroundColor;
    public static readonly Color PanelColor = VisualTheme.PanelColor;
    public static readonly Color CardColor = VisualTheme.CardColor;
    public static readonly Color CreamColor = VisualTheme.CreamColor;
    public static readonly Color AccentOrange = VisualTheme.AccentOrange;
    public static readonly Color GoldColor = VisualTheme.GoldColor;
    public static readonly Color SecondaryColor = VisualTheme.SecondaryColor;
    public static readonly Color DangerColor = VisualTheme.DangerColor;
    public static readonly Color PositiveColor = VisualTheme.PositiveColor;
    public static readonly Color LockedColor = VisualTheme.LockedColor;
    public static readonly Color MutedTextColor = VisualTheme.MutedTextColor;

    public const int HeadingSize = VisualTheme.HeadingSize;
    public const int SubheadingSize = VisualTheme.SubheadingSize;
    public const int BodySize = VisualTheme.BodySize;
    public const int CaptionSize = VisualTheme.CaptionSize;
    public const int ButtonTextSize = VisualTheme.ButtonTextSize;

    static Font builtinFont;
    static Font BuiltinFont => builtinFont ??= Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    static Sprite roundedSprite;
    static Sprite RoundedSprite => roundedSprite ??= GenerateRoundedSprite();

    static Sprite circleSprite;
    public static Sprite CircleSprite => circleSprite ??= GenerateCircleSprite();

    // ---------- Panels & containers ----------

    public static RectTransform CreatePanel(Transform parent, string name)
    {
        var go = new GameObject(SanitizeName(name), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        SetStretch(rt);
        go.GetComponent<Image>().color = PanelColor;
        return rt;
    }

    // A rounded sub-panel used to visually group content (header bars, list rows, etc).
    public static RectTransform CreateCard(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color? color = null)
    {
        var go = new GameObject(SanitizeName("Card_" + name), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var image = go.GetComponent<Image>();
        image.sprite = RoundedSprite;
        image.type = Image.Type.Sliced;
        image.color = color ?? CardColor;
        return rt;
    }

    public static RectTransform CreateContainer(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject("Container", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return rt;
    }

    // ---------- Text ----------

    public static Text CreateText(Transform parent, string content, int fontSize, Color color, TextAnchor alignment,
        Vector2 anchorMin, Vector2 anchorMax, FontStyle fontStyle = FontStyle.Normal)
    {
        var go = new GameObject(SanitizeName("Text_" + content), typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var text = go.AddComponent<Text>();
        text.font = BuiltinFont;
        text.text = content;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    // Semantic text helpers so screens share one consistent hierarchy instead of picking sizes ad hoc.
    public static Text CreateHeading(Transform parent, string content, Vector2 anchorMin, Vector2 anchorMax) =>
        CreateText(parent, content, HeadingSize, GoldColor, TextAnchor.MiddleCenter, anchorMin, anchorMax, FontStyle.Bold);

    public static Text CreateSubheading(Transform parent, string content, Vector2 anchorMin, Vector2 anchorMax) =>
        CreateText(parent, content, SubheadingSize, CreamColor, TextAnchor.MiddleCenter, anchorMin, anchorMax, FontStyle.Bold);

    public static Text CreateBody(Transform parent, string content, Vector2 anchorMin, Vector2 anchorMax, TextAnchor alignment = TextAnchor.MiddleLeft) =>
        CreateText(parent, content, BodySize, CreamColor, alignment, anchorMin, anchorMax);

    public static Text CreateCaption(Transform parent, string content, Vector2 anchorMin, Vector2 anchorMax, TextAnchor alignment = TextAnchor.MiddleLeft) =>
        CreateText(parent, content, CaptionSize, MutedTextColor, alignment, anchorMin, anchorMax);

    // ---------- Buttons ----------

    public static Button CreateButton(Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax,
        UnityEngine.Events.UnityAction onClick, Color? color = null)
    {
        var go = new GameObject(SanitizeName("Button_" + label), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var image = go.GetComponent<Image>();
        image.sprite = RoundedSprite;
        image.type = Image.Type.Sliced;
        image.color = color ?? AccentOrange;

        var btn = go.GetComponent<Button>();
        btn.targetGraphic = image;
        go.AddComponent<ButtonPunch>();

        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f);
        colors.pressedColor = new Color(0.72f, 0.72f, 0.72f, 1f);
        colors.selectedColor = Color.white;
        colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.55f);
        colors.fadeDuration = 0.08f;
        btn.colors = colors;

        btn.onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlayClick();
            onClick?.Invoke();
        });

        var text = CreateText(go.transform, label, ButtonTextSize, CreamColor, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, FontStyle.Bold);
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 14;
        text.resizeTextMaxSize = ButtonTextSize;

        return btn;
    }

    // ---------- Sliders ----------

    public static Slider CreateSlider(Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color fillColor)
    {
        var go = new GameObject("Slider", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var bgGo = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bgGo.transform.SetParent(go.transform, false);
        SetStretch(bgGo.GetComponent<RectTransform>());
        var bgImage = bgGo.GetComponent<Image>();
        bgImage.sprite = RoundedSprite;
        bgImage.type = Image.Type.Sliced;
        bgImage.color = new Color(0f, 0f, 0f, 0.45f);

        var fillAreaGo = new GameObject("FillArea", typeof(RectTransform));
        fillAreaGo.transform.SetParent(go.transform, false);
        SetStretch(fillAreaGo.GetComponent<RectTransform>());

        var delayedGo = new GameObject("DelayedFill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        delayedGo.transform.SetParent(fillAreaGo.transform, false);
        SetStretch(delayedGo.GetComponent<RectTransform>());
        var delayedImage = delayedGo.GetComponent<Image>();
        delayedImage.sprite = RoundedSprite;
        delayedImage.type = Image.Type.Filled;
        delayedImage.fillMethod = Image.FillMethod.Horizontal;
        delayedImage.fillOrigin = 0;
        delayedImage.fillAmount = 1f;
        delayedImage.color = Color.Lerp(fillColor, CreamColor, 0.38f);
        delayedImage.raycastTarget = false;

        var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fillGo.transform.SetParent(fillAreaGo.transform, false);
        var fillRt = fillGo.GetComponent<RectTransform>();
        SetStretch(fillRt);
        var fillImage = fillGo.GetComponent<Image>();
        fillImage.sprite = RoundedSprite;
        fillImage.type = Image.Type.Sliced;
        fillImage.color = fillColor;

        var slider = go.AddComponent<Slider>();
        slider.fillRect = fillRt;
        slider.targetGraphic = fillImage;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.interactable = false;
        var smooth = go.AddComponent<SmoothSlider>();
        smooth.Configure(fillImage, delayedImage, fillColor);
        return slider;
    }

    // ---------- Input fields ----------

    public static InputField CreateInputField(Transform parent, string placeholder, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject("InputField", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var fieldImage = go.GetComponent<Image>();
        fieldImage.sprite = RoundedSprite;
        fieldImage.type = Image.Type.Sliced;
        fieldImage.color = CreamColor;

        var textGo = new GameObject("Text", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        var textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(20, 5);
        textRt.offsetMax = new Vector2(-20, -5);
        var text = textGo.AddComponent<Text>();
        text.font = BuiltinFont;
        text.fontSize = BodySize;
        text.color = Color.black;
        text.alignment = TextAnchor.MiddleLeft;
        text.supportRichText = false;

        var placeholderGo = new GameObject("Placeholder", typeof(RectTransform));
        placeholderGo.transform.SetParent(go.transform, false);
        var placeholderRt = placeholderGo.GetComponent<RectTransform>();
        placeholderRt.anchorMin = Vector2.zero;
        placeholderRt.anchorMax = Vector2.one;
        placeholderRt.offsetMin = new Vector2(20, 5);
        placeholderRt.offsetMax = new Vector2(-20, -5);
        var placeholderText = placeholderGo.AddComponent<Text>();
        placeholderText.font = BuiltinFont;
        placeholderText.fontSize = BodySize;
        placeholderText.color = new Color(0f, 0f, 0f, 0.4f);
        placeholderText.text = placeholder;
        placeholderText.fontStyle = FontStyle.Italic;
        placeholderText.alignment = TextAnchor.MiddleLeft;

        var input = go.AddComponent<InputField>();
        input.textComponent = text;
        input.placeholder = placeholderText;
        input.characterLimit = 16;
        return input;
    }

    // ---------- Brand header ----------

    // Code-generated "logo": a calico-patched championship medallion plus the league
    // wordmark, flanked by small glove accents. If a real logo is ever dropped into
    // Resources/Art/Logos/league_logo, it's used instead with no code changes.
    public static RectTransform CreateBrandHeader(Transform parent, Vector2 anchorMin, Vector2 anchorMax, float scale = 1f)
    {
        var root = CreateContainer(parent, anchorMin, anchorMax);
        // Font growth is capped well below the badge/glove growth so a large scale
        // (e.g. a title-screen-sized logo) can't push the wordmark off-screen.
        float fontScale = Mathf.Min(scale, 1.5f);

        var realLogo = ArtRegistry.GetBanner();
        if (realLogo != null)
        {
            var logoGo = new GameObject("Logo", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            logoGo.transform.SetParent(root, false);
            var logoRt = logoGo.GetComponent<RectTransform>();
            logoRt.anchorMin = Vector2.zero;
            logoRt.anchorMax = Vector2.one;
            logoRt.offsetMin = Vector2.zero;
            logoRt.offsetMax = Vector2.zero;
            var logoImage = logoGo.GetComponent<Image>();
            logoImage.sprite = realLogo;
            logoImage.preserveAspect = true;
            return root;
        }

        // Outer gold ring + black ring, same as before, but the center now carries a
        // small calico "patch" pattern (three colored blobs) instead of one flat dot.
        var badgeGo = new GameObject("Badge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        badgeGo.transform.SetParent(root, false);
        var badgeRt = badgeGo.GetComponent<RectTransform>();
        badgeRt.anchorMin = new Vector2(0.5f, 0.64f);
        badgeRt.anchorMax = new Vector2(0.5f, 0.64f);
        badgeRt.sizeDelta = new Vector2(104, 104) * scale;
        var badgeImage = badgeGo.GetComponent<Image>();
        badgeImage.sprite = CircleSprite;
        badgeImage.color = GoldColor;

        var innerGo = new GameObject("BadgeInner", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        innerGo.transform.SetParent(badgeGo.transform, false);
        var innerRt = innerGo.GetComponent<RectTransform>();
        innerRt.anchorMin = Vector2.zero;
        innerRt.anchorMax = Vector2.one;
        innerRt.offsetMin = new Vector2(10, 10);
        innerRt.offsetMax = new Vector2(-10, -10);
        var innerImage = innerGo.GetComponent<Image>();
        innerImage.sprite = CircleSprite;
        innerImage.color = BackgroundColor;

        CreatePatch(innerGo.transform, new Vector2(0.30f, 0.62f), 0.55f, AccentOrange);
        CreatePatch(innerGo.transform, new Vector2(0.66f, 0.34f), 0.42f, CreamColor);
        CreatePatch(innerGo.transform, new Vector2(0.40f, 0.28f), 0.34f, new Color(0.08f, 0.07f, 0.07f));

        CreateText(root, "CALICO COMBAT LEAGUE", Mathf.RoundToInt(HeadingSize * fontScale), GoldColor, TextAnchor.MiddleCenter,
            new Vector2(0f, 0.18f), new Vector2(1f, 0.43f), FontStyle.Bold);

        // Small glove-like accents bracketing the wordmark for an MMA feel.
        CreateGloveDot(root, new Vector2(0.07f, 0.235f), 26f * scale);
        CreateGloveDot(root, new Vector2(0.93f, 0.235f), 26f * scale);

        CreateText(root, "Become the Champion", Mathf.RoundToInt(SubheadingSize * fontScale), CreamColor, TextAnchor.MiddleCenter,
            new Vector2(0f, 0f), new Vector2(1f, 0.18f));

        return root;
    }

    static void CreatePatch(Transform parent, Vector2 anchorCenter, float relativeSize, Color color)
    {
        var go = new GameObject("Patch", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorCenter;
        rt.anchorMax = anchorCenter;
        rt.sizeDelta = new Vector2(relativeSize, relativeSize) * 84f;
        var image = go.GetComponent<Image>();
        image.sprite = CircleSprite;
        image.color = color;
    }

    static void CreateGloveDot(Transform parent, Vector2 anchorCenter, float size)
    {
        var go = new GameObject("GloveDot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorCenter;
        rt.anchorMax = anchorCenter;
        rt.sizeDelta = new Vector2(size, size);
        var image = go.GetComponent<Image>();
        image.sprite = CircleSprite;
        image.color = AccentOrange;
    }

    // ---------- Fighter cards ----------

    // Builds a "fighter vs fighter" presentation card: a portrait frame on the left,
    // and an open info area on the right where the caller places name/level/bars.
    // Use SetFighterPortrait to fill the portrait with real art or a placeholder.
    public static RectTransform CreateFighterCard(Transform parent, string cardName, Vector2 anchorMin, Vector2 anchorMax,
        out Image portraitImage, out RectTransform infoArea)
    {
        var card = CreateCard(parent, "FighterCard_" + cardName, anchorMin, anchorMax, CardColor);

        var frame = CreateCard(card, "PortraitFrame", new Vector2(0.02f, 0.06f), new Vector2(0.28f, 0.94f), BackgroundColor);

        var portraitGo = new GameObject("Portrait", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        portraitGo.transform.SetParent(frame, false);
        var portraitRt = portraitGo.GetComponent<RectTransform>();
        portraitRt.anchorMin = new Vector2(0.12f, 0.1f);
        portraitRt.anchorMax = new Vector2(0.88f, 0.92f);
        portraitRt.offsetMin = Vector2.zero;
        portraitRt.offsetMax = Vector2.zero;
        portraitImage = portraitGo.GetComponent<Image>();
        portraitImage.preserveAspect = true;

        infoArea = CreateContainer(card, new Vector2(0.3f, 0f), new Vector2(0.98f, 1f));

        return card;
    }

    // Fills a fighter card's portrait following the priority chain from Milestone 13:
    // 1) a custom portrait for this exact fighterId, 2) a shared portrait for their
    // archetype, 3) the generated silhouette placeholder. Never leaves the image blank.
    public static void SetFighterPortrait(Image portraitImage, string fighterId, ArchetypeType archetype, Color themeColor)
    {
        var customPortrait = ArtRegistry.GetFighterPortrait(fighterId);
        if (customPortrait != null)
        {
            portraitImage.sprite = customPortrait;
            portraitImage.color = Color.white;
            return;
        }

        var archetypePortrait = ArtRegistry.GetArchetypePortrait(archetype);
        if (archetypePortrait != null)
        {
            portraitImage.sprite = archetypePortrait;
            portraitImage.color = Color.white;
            return;
        }

        portraitImage.sprite = IconFactory.GetFighterPlaceholderSprite(archetype);
        portraitImage.color = themeColor;
    }

    // Overload for opponents, who have no archetype - same priority chain minus that step.
    public static void SetFighterPortrait(Image portraitImage, string fighterId, Color themeColor) =>
        SetFighterPortrait(portraitImage, fighterId, ArchetypeType.Unspecified, themeColor);

    // Milestone 17 avatar fallback chain: 1) dedicated player avatar art, 2) shared
    // archetype avatar art, 3) the existing portrait chain (custom portrait ->
    // archetype portrait -> generated silhouette). The world-traveling avatar never
    // shows a broken image even with zero avatar-specific art supplied.
    public static void SetPlayerAvatar(Image avatarImage, ArchetypeType archetype, Color themeColor)
    {
        var customAvatar = ArtRegistry.GetPlayerAvatar();
        if (customAvatar != null)
        {
            avatarImage.sprite = customAvatar;
            avatarImage.color = Color.white;
            return;
        }

        var archetypeAvatar = ArtRegistry.GetArchetypeAvatar(archetype);
        if (archetypeAvatar != null)
        {
            avatarImage.sprite = archetypeAvatar;
            avatarImage.color = Color.white;
            return;
        }

        SetFighterPortrait(avatarImage, "player", archetype, themeColor);
    }

    // Same chain as SetPlayerAvatar, but checks for a dedicated walk-cycle sprite
    // first - used only while the avatar is traveling. Falls back to the regular
    // avatar chain when no walk art exists, so it's safe to call unconditionally.
    public static void SetPlayerAvatarWalk(Image avatarImage, ArchetypeType archetype, Color themeColor)
    {
        var customWalk = ArtRegistry.GetPlayerAvatarWalk();
        if (customWalk != null)
        {
            avatarImage.sprite = customWalk;
            avatarImage.color = Color.white;
            return;
        }

        var archetypeWalk = ArtRegistry.GetArchetypeAvatarWalk(archetype);
        if (archetypeWalk != null)
        {
            avatarImage.sprite = archetypeWalk;
            avatarImage.color = Color.white;
            return;
        }

        SetPlayerAvatar(avatarImage, archetype, themeColor);
    }

    public static void SetFighterBattleSprite(Image spriteImage, string fighterId, ArchetypeType archetype,
        FighterSpritePose pose, Color themeColor)
    {
        // The player's battle presence always uses the same world-avatar pipeline as
        // the Hub/Map/Gym Entry (player.png -> archetype avatar -> existing portrait
        // chain -> placeholder), instead of pose-specific battle sprites. Opponents
        // are unaffected - this only triggers for the literal "player" fighterId.
        if (fighterId == "player")
        {
            SetPlayerAvatar(spriteImage, archetype, themeColor);
            return;
        }

        var battleSprite = ArtRegistry.GetBattleSprite(fighterId, archetype, pose);
        if (battleSprite != null)
        {
            spriteImage.sprite = battleSprite;
            spriteImage.color = Color.white;
            return;
        }

        var portrait = ArtRegistry.GetFighterPortrait(fighterId);
        if (portrait == null) portrait = ArtRegistry.GetArchetypePortrait(archetype);
        if (portrait != null)
        {
            spriteImage.sprite = portrait;
            spriteImage.color = Color.white;
            return;
        }

        spriteImage.sprite = IconFactory.GetFighterPlaceholderSprite(archetype);
        spriteImage.color = themeColor;
    }

    public static RectTransform CreateBattleFighter(Transform parent, string fighterName, Vector2 anchorMin,
        Vector2 anchorMax, out Image spriteImage)
    {
        var root = CreateContainer(parent, anchorMin, anchorMax);
        root.gameObject.name = SanitizeName("Combatant_" + fighterName);

        var shadowGo = new GameObject("Shadow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        shadowGo.transform.SetParent(root, false);
        var shadowRt = shadowGo.GetComponent<RectTransform>();
        shadowRt.anchorMin = new Vector2(0.18f, 0.02f);
        shadowRt.anchorMax = new Vector2(0.82f, 0.18f);
        shadowRt.offsetMin = Vector2.zero;
        shadowRt.offsetMax = Vector2.zero;
        var shadow = shadowGo.GetComponent<Image>();
        shadow.sprite = CircleSprite;
        shadow.color = new Color(0f, 0f, 0f, 0.42f);
        shadow.raycastTarget = false;

        var spriteGo = new GameObject("FighterSprite", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        spriteGo.transform.SetParent(root, false);
        var spriteRt = spriteGo.GetComponent<RectTransform>();
        spriteRt.anchorMin = new Vector2(0.06f, 0.04f);
        spriteRt.anchorMax = new Vector2(0.94f, 1f);
        spriteRt.offsetMin = Vector2.zero;
        spriteRt.offsetMax = Vector2.zero;
        spriteImage = spriteGo.GetComponent<Image>();
        spriteImage.preserveAspect = true;
        spriteImage.raycastTarget = false;

        return root;
    }

    // Small world-traveling avatar marker (Milestone 17) shared by the Hub, Gym Map,
    // and Gym Entry presentation - a shadow plus a sprite image, the same split used
    // by CreateBattleFighter so PlayerAvatarVisual can animate the sprite (idle/step
    // bounce) independently from the root (anchor position).
    public static RectTransform CreateAvatarMarker(Transform parent, string markerName, Vector2 anchorMin, Vector2 anchorMax,
        out Image avatarImage)
    {
        var root = CreateContainer(parent, anchorMin, anchorMax);
        root.gameObject.name = SanitizeName("Avatar_" + markerName);

        var shadowGo = new GameObject("Shadow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        shadowGo.transform.SetParent(root, false);
        var shadowRt = shadowGo.GetComponent<RectTransform>();
        shadowRt.anchorMin = new Vector2(0.24f, 0f);
        shadowRt.anchorMax = new Vector2(0.76f, 0.07f);
        shadowRt.offsetMin = Vector2.zero;
        shadowRt.offsetMax = Vector2.zero;
        var shadow = shadowGo.GetComponent<Image>();
        shadow.sprite = CircleSprite;
        shadow.color = new Color(0f, 0f, 0f, 0.38f);
        shadow.raycastTarget = false;

        // Minimal inset (Milestone: tall portrait avatar) - just enough to keep the
        // sprite clear of the shadow ellipse at the feet. preserveAspect still
        // guarantees no stretching regardless of how tight this box is.
        var spriteGo = new GameObject("AvatarSprite", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        spriteGo.transform.SetParent(root, false);
        var spriteRt = spriteGo.GetComponent<RectTransform>();
        spriteRt.anchorMin = new Vector2(0.01f, 0.04f);
        spriteRt.anchorMax = new Vector2(0.99f, 1f);
        spriteRt.offsetMin = Vector2.zero;
        spriteRt.offsetMax = Vector2.zero;
        avatarImage = spriteGo.GetComponent<Image>();
        avatarImage.preserveAspect = true;
        avatarImage.raycastTarget = false;

        return root;
    }

    // Milestone 29: rival dialogue popup. Reuses CreateCard/CreateText and the
    // existing fighter-portrait pipeline (SetFighterPortrait) - click-to-advance
    // only, matching the battle intro's bio/quote beats so lines can't be missed.
    // Milestone 33, Part 9: added a "RIVAL ENCOUNTER" title card above the box
    // and the rival's own accent color/motto on the nameplate, so every
    // appearance reads as the same recognizable, distinct presence.
    public static RivalDialogueBox CreateRivalDialogue(Transform parent)
    {
        var card = CreateCard(parent, "RivalDialogue", new Vector2(0.24f, 0.32f), new Vector2(0.76f, 0.68f),
            new Color(0.07f, 0.06f, 0.06f, 0.97f));

        // Title card: a child of the card itself, anchored above its normal 0-1
        // bounds, so it fades/activates in lockstep with the card with zero
        // extra code in RivalDialogueBox.
        var titleTag = CreateText(card, "RIVAL ENCOUNTER", CaptionSize, RivalDatabase.AccentColor, TextAnchor.MiddleCenter,
            new Vector2(0f, 1.03f), new Vector2(1f, 1.13f), FontStyle.Bold);
        titleTag.raycastTarget = false;

        var portraitGo = new GameObject("Portrait", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        portraitGo.transform.SetParent(card, false);
        var portraitRt = portraitGo.GetComponent<RectTransform>();
        portraitRt.anchorMin = new Vector2(0.04f, 0.1f);
        portraitRt.anchorMax = new Vector2(0.27f, 0.9f);
        portraitRt.offsetMin = Vector2.zero;
        portraitRt.offsetMax = Vector2.zero;
        var portraitImage = portraitGo.GetComponent<Image>();
        portraitImage.preserveAspect = true;
        portraitImage.raycastTarget = false;
        Color rivalTheme = IconFactory.GetArchetypeThemeColor(RivalDatabase.PortraitArchetype);
        SetFighterPortrait(portraitImage, RivalDatabase.PortraitId, RivalDatabase.PortraitArchetype, rivalTheme);

        // Nameplate: rival's own accent color (not the generic gold used for
        // gym opponents) plus a small motto line, so the identity is
        // recognizable the same way every time the rival shows up.
        var nameText = CreateText(card, "", BodySize, RivalDatabase.AccentColor, TextAnchor.MiddleLeft,
            new Vector2(0.31f, 0.78f), new Vector2(0.97f, 0.88f), FontStyle.Bold);
        nameText.raycastTarget = false;

        var mottoText = CreateText(card, $"\"{RivalDatabase.Motto}\"", CaptionSize, MutedTextColor, TextAnchor.MiddleLeft,
            new Vector2(0.31f, 0.70f), new Vector2(0.97f, 0.77f), FontStyle.Italic);
        mottoText.raycastTarget = false;

        var lineText = CreateText(card, "", SubheadingSize, CreamColor, TextAnchor.MiddleLeft,
            new Vector2(0.31f, 0.18f), new Vector2(0.97f, 0.69f), FontStyle.Italic);
        lineText.raycastTarget = false;

        var tapPrompt = CreateText(card, "TAP TO CONTINUE ▸", CaptionSize, MutedTextColor, TextAnchor.MiddleRight,
            new Vector2(0.31f, 0.04f), new Vector2(0.97f, 0.16f), FontStyle.Italic);
        tapPrompt.raycastTarget = false;
        tapPrompt.gameObject.SetActive(false);

        var group = card.gameObject.AddComponent<CanvasGroup>();

        var box = card.gameObject.AddComponent<RivalDialogueBox>();
        box.Initialize(nameText, lineText, tapPrompt, group, card);

        var tapButton = card.gameObject.AddComponent<Button>();
        tapButton.transition = Selectable.Transition.None;
        tapButton.targetGraphic = card.GetComponent<Image>();
        tapButton.onClick.AddListener(box.OnTapped);

        card.gameObject.SetActive(false);
        return box;
    }

    public static RectTransform CreateFighterThumbnail(Transform parent, string fighterId, ArchetypeType archetype,
        Color themeColor, Vector2 anchorMin, Vector2 anchorMax)
    {
        var frame = CreateCard(parent, "PortraitThumbnail", anchorMin, anchorMax, BackgroundColor);
        frame.GetComponent<Image>().raycastTarget = false;

        var portraitGo = new GameObject("Portrait", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        portraitGo.transform.SetParent(frame, false);
        var portraitRt = portraitGo.GetComponent<RectTransform>();
        portraitRt.anchorMin = new Vector2(0.1f, 0.08f);
        portraitRt.anchorMax = new Vector2(0.9f, 0.92f);
        portraitRt.offsetMin = Vector2.zero;
        portraitRt.offsetMax = Vector2.zero;

        var portrait = portraitGo.GetComponent<Image>();
        portrait.preserveAspect = true;
        portrait.raycastTarget = false;
        SetFighterPortrait(portrait, fighterId, archetype, themeColor);
        return frame;
    }

    // Adds a full-bleed background image behind everything else on a screen if
    // ArtRegistry has one for this key; otherwise the screen's flat panel color
    // (already set by CreatePanel) remains the background - a safe, silent fallback.
    // Safe to call repeatedly (e.g. once per gym visit) - replaces any previous
    // background rather than stacking duplicates.
    public static void ApplyScreenBackground(GameObject root, string key)
    {
        var existing = root.transform.Find("Background");
        if (existing != null) Object.Destroy(existing.gameObject);

        var background = ArtRegistry.GetBackground(key);
        if (background == null) return;

        var go = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(root.transform, false);
        go.transform.SetAsFirstSibling();
        var rt = go.GetComponent<RectTransform>();
        SetStretch(rt);
        var image = go.GetComponent<Image>();
        image.sprite = background;
        image.color = Color.white;
        image.raycastTarget = false;
        // Milestone 26: existing backgrounds are portrait (9:16); the frame is now
        // landscape (16:9). preserveAspect shows the full image undistorted
        // (pillarboxed) instead of stretching it, with zero changes needed once
        // landscape-shaped (1920x1080) art replaces these - it will simply fill
        // edge-to-edge with no visible letterbox at that point.
        image.preserveAspect = true;

        // Keeps foreground text readable when future artwork is bright or detailed.
        var tintGo = new GameObject("ReadabilityTint", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        tintGo.transform.SetParent(go.transform, false);
        SetStretch(tintGo.GetComponent<RectTransform>());
        var tint = tintGo.GetComponent<Image>();
        tint.color = new Color(0.03f, 0.025f, 0.025f, 0.48f);
        tint.raycastTarget = false;
    }

    // Small discipline icon pinned to a portrait frame's corner - reuses the same
    // shape/color language as gym icons so a fighter's discipline reads at a glance.
    // Safe to call repeatedly (e.g. once per battle) - replaces any previous badge
    // rather than stacking duplicates, since the opponent's discipline can change.
    public static void AddDisciplineBadge(Transform portraitFrame, IconShape shape, Color color)
    {
        AddDisciplineBadgeSprite(portraitFrame, IconFactory.GetShapeSprite(shape), color);
    }

    // Milestone 30 (icon integration): archetype-specific overload that prefers
    // the official archetype icon art (boxer_icon, wrestler_icon, muay_thai_icon,
    // bjj_icon) when present, falling back to the generated shape badge otherwise -
    // same badge frame/positioning either way, so every existing call site that
    // only had a generated shape keeps working unchanged.
    public static void AddDisciplineBadge(Transform portraitFrame, ArchetypeType archetype, Color color)
    {
        var realIcon = ArtRegistry.GetArchetypeIcon(archetype);
        if (realIcon != null)
            AddDisciplineBadgeSprite(portraitFrame, realIcon, Color.white);
        else
            AddDisciplineBadgeSprite(portraitFrame, IconFactory.GetShapeSprite(IconFactory.GetArchetypeIconShape(archetype)), color);
    }

    static void AddDisciplineBadgeSprite(Transform portraitFrame, Sprite sprite, Color color)
    {
        var existing = portraitFrame.Find("DisciplineBadge");
        if (existing != null) Object.Destroy(existing.gameObject);

        var badgeGo = new GameObject("DisciplineBadge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        badgeGo.transform.SetParent(portraitFrame, false);
        var rt = badgeGo.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.62f, 0.02f);
        rt.anchorMax = new Vector2(0.98f, 0.34f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var ring = badgeGo.GetComponent<Image>();
        ring.sprite = CircleSprite;
        ring.color = BackgroundColor;

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconGo.transform.SetParent(badgeGo.transform, false);
        var iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.18f, 0.18f);
        iconRt.anchorMax = new Vector2(0.82f, 0.82f);
        iconRt.offsetMin = Vector2.zero;
        iconRt.offsetMax = Vector2.zero;
        var iconImage = iconGo.GetComponent<Image>();
        iconImage.sprite = sprite;
        iconImage.color = color;
        iconImage.preserveAspect = true;
    }

    // A more ornate medal for championship-tier moments (Championship screen, Hall
    // of Champions) - distinct from the standard league badge used elsewhere.
    public static RectTransform CreateChampionBadge(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        var root = CreateContainer(parent, anchorMin, anchorMax);

        var realBelt = ArtRegistry.GetChampionBelt();
        if (realBelt != null)
        {
            var beltGo = new GameObject("ChampionBelt", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            beltGo.transform.SetParent(root, false);
            SetStretch(beltGo.GetComponent<RectTransform>());
            var beltImage = beltGo.GetComponent<Image>();
            beltImage.sprite = realBelt;
            beltImage.preserveAspect = true;
            return root;
        }

        var ribbonLeft = CreateRibbonTail(root, -0.16f);
        var ribbonRight = CreateRibbonTail(root, 0.16f);

        var outerGo = new GameObject("MedalOuter", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        outerGo.transform.SetParent(root, false);
        var outerRt = outerGo.GetComponent<RectTransform>();
        outerRt.anchorMin = new Vector2(0.5f, 0.62f);
        outerRt.anchorMax = new Vector2(0.5f, 0.62f);
        outerRt.sizeDelta = new Vector2(132, 132);
        var outerImage = outerGo.GetComponent<Image>();
        outerImage.sprite = CircleSprite;
        outerImage.color = GoldColor;

        var innerGo = new GameObject("MedalInner", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        innerGo.transform.SetParent(outerGo.transform, false);
        var innerRt = innerGo.GetComponent<RectTransform>();
        innerRt.anchorMin = Vector2.zero;
        innerRt.anchorMax = Vector2.one;
        innerRt.offsetMin = new Vector2(10, 10);
        innerRt.offsetMax = new Vector2(-10, -10);
        var innerImage = innerGo.GetComponent<Image>();
        innerImage.sprite = CircleSprite;
        innerImage.color = BackgroundColor;

        var starGo = new GameObject("MedalStar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        starGo.transform.SetParent(innerGo.transform, false);
        var starRt = starGo.GetComponent<RectTransform>();
        starRt.anchorMin = Vector2.zero;
        starRt.anchorMax = Vector2.one;
        starRt.offsetMin = new Vector2(16, 16);
        starRt.offsetMax = new Vector2(-16, -16);
        var starImage = starGo.GetComponent<Image>();
        starImage.sprite = IconFactory.GetShapeSprite(IconShape.Star);
        starImage.color = GoldColor;

        ribbonLeft.SetAsFirstSibling();
        ribbonRight.SetAsFirstSibling();

        return root;
    }

    static RectTransform CreateRibbonTail(Transform parent, float xOffset)
    {
        var go = new GameObject("RibbonTail", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.3f);
        rt.anchorMax = new Vector2(0.5f, 0.3f);
        rt.sizeDelta = new Vector2(26, 70);
        rt.anchoredPosition = new Vector2(xOffset * 200f, 0f);
        rt.localRotation = Quaternion.Euler(0f, 0f, xOffset > 0 ? -12f : 12f);
        var image = go.GetComponent<Image>();
        image.color = DangerColor;
        return rt;
    }

    // ---------- Generated sprites ----------

    static Sprite GenerateRoundedSprite()
    {
        const int size = 64;
        const int radius = 18;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = x + 0.5f;
                float py = y + 0.5f;
                float nearestX = Mathf.Clamp(px, radius, size - radius);
                float nearestY = Mathf.Clamp(py, radius, size - radius);
                float dist = Mathf.Sqrt((px - nearestX) * (px - nearestX) + (py - nearestY) * (py - nearestY));
                float alpha = dist <= radius ? 1f : 0f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f,
            0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
    }

    static Sprite GenerateCircleSprite()
    {
        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
        float r = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x + 0.5f - r;
                float dy = y + 0.5f - r;
                float alpha = Mathf.Sqrt(dx * dx + dy * dy) <= r ? 1f : 0f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    static string SanitizeName(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "UIElement";
        string clean = raw.Replace("\n", " ");
        return clean.Length > 24 ? clean.Substring(0, 24) : clean;
    }

    static void SetStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
