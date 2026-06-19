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
        go.AddComponent<SmoothSlider>();
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
    public static RectTransform CreateBrandHeader(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        var root = CreateContainer(parent, anchorMin, anchorMax);

        var realLogo = ArtRegistry.GetLogo();
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
        badgeRt.sizeDelta = new Vector2(104, 104);
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

        CreateText(root, "CALICO COMBAT LEAGUE", HeadingSize, GoldColor, TextAnchor.MiddleCenter,
            new Vector2(0f, 0.18f), new Vector2(1f, 0.43f), FontStyle.Bold);

        // Small glove-like accents bracketing the wordmark for an MMA feel.
        CreateGloveDot(root, new Vector2(0.07f, 0.235f), 26f);
        CreateGloveDot(root, new Vector2(0.93f, 0.235f), 26f);

        CreateText(root, "Become the Champion", SubheadingSize, CreamColor, TextAnchor.MiddleCenter,
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

        portraitImage.sprite = IconFactory.GetSilhouetteSprite();
        portraitImage.color = themeColor;
    }

    // Overload for opponents, who have no archetype - same priority chain minus that step.
    public static void SetFighterPortrait(Image portraitImage, string fighterId, Color themeColor) =>
        SetFighterPortrait(portraitImage, fighterId, ArchetypeType.Unspecified, themeColor);

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
    }

    // Small discipline icon pinned to a portrait frame's corner - reuses the same
    // shape/color language as gym icons so a fighter's discipline reads at a glance.
    // Safe to call repeatedly (e.g. once per battle) - replaces any previous badge
    // rather than stacking duplicates, since the opponent's discipline can change.
    public static void AddDisciplineBadge(Transform portraitFrame, IconShape shape, Color color)
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
        iconImage.sprite = IconFactory.GetShapeSprite(shape);
        iconImage.color = color;
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
