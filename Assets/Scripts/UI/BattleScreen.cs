using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleScreen : UIScreen
{
    readonly Image opponentPortrait;
    readonly Image playerPortrait;
    readonly FighterCardFX opponentFx;
    readonly FighterCardFX playerFx;
    readonly Text playerName;
    readonly Text opponentName;
    readonly Text fightBilling;
    readonly Text playerHealthValue;
    readonly Text playerStaminaValue;
    readonly Text opponentHealthValue;
    readonly Text opponentStaminaValue;
    readonly Text playerEffectsText;
    readonly Text opponentEffectsText;
    readonly Text logText;
    readonly ScrollRect logScroll;
    readonly Text chainText;
    readonly RectTransform stageCard;
    readonly CanvasGroup stageGroup;
    readonly RectTransform playerCombatantRoot;
    readonly RectTransform opponentCombatantRoot;
    readonly Image playerBattleSprite;
    readonly Image opponentBattleSprite;
    readonly Image opponentAura;
    readonly BattleFighterVisual playerCombatant;
    readonly BattleFighterVisual opponentCombatant;
    readonly Slider playerHealth;
    readonly Slider playerStamina;
    readonly Slider opponentHealth;
    readonly Slider opponentStamina;
    readonly Button[] moveButtons;
    // Milestone 64, Part 5/6: was one auto-fit Text per button (name+cost+tag
    // all crammed onto 2 lines at one uniform size, in UiFont/Abolition-Rough -
    // a condensed display face suited to short hype headlines, not dense
    // multi-line button content). Split into 3 explicitly-tiered Texts so
    // size hierarchy is guaranteed (not just "whatever best-fit lands on"),
    // and built via BodySize/CaptionSize so they resolve to DialogueFont
    // (AtkinsonHyperlegible-Bold) through UIFactory's existing font-by-size
    // lookup - already in the project, already the most legible-at-small-
    // sizes option of the three, no new font introduced.
    readonly Text[] moveNameTexts;
    readonly Text[] moveCostTexts;
    readonly Text[] moveTagTexts;
    readonly Button itemButton;
    readonly Button recoverButton;
    readonly Button parryButton;
    readonly Button clinchButton;
    readonly Transform itemContainer;
    readonly RectTransform introCard;
    readonly CanvasGroup introGroup;
    readonly Text introText;
    readonly Text introBillingText;
    readonly Text introAnnouncementText;
    readonly RectTransform introMatchupGroup;
    readonly Image introPlayerPortrait;
    readonly Image introOpponentPortrait;
    // Milestone 64, Part 2/5: the old single 5-line introPlayerName/
    // introOpponentName block is split into 3 tiered elements per side -
    // Name (largest), Nickname, and Details (archetype/level/Prestige +
    // record/streak + age/reach/style, all "supporting information" per
    // Part 5's own tier list) - same data as before, just visually tiered
    // instead of one undifferentiated block.
    readonly Text introPlayerNameText;
    readonly Text introPlayerNicknameText;
    readonly Text introPlayerDetailsText;
    readonly Text introOpponentNameText;
    readonly Text introOpponentNicknameText;
    readonly Text introOpponentDetailsText;
    readonly RectTransform introTapeGroup;
    readonly Text[] tapePlayerValues = new Text[8];
    readonly Text[] tapeOpponentValues = new Text[8];
    readonly Text introTapPrompt;
    // Milestone 36, Part 1/9: the Tale of the Tape stat comparison now requires
    // an explicit click rather than auto-advancing - this button only appears
    // for that one beat.
    readonly Button introContinueButton;
    bool introContinueClicked;
    bool introSkipRequested;
    bool waitingForDialogueTap;
    bool dialogueAdvanceRequested;
    readonly List<GameObject> itemEntries = new List<GameObject>();
    readonly List<string> log = new List<string>();
    bool showingItems;
    bool animatingTurn;

    // Milestone 43: which RivalDatabase.ShowdownTaunts indices have already
    // fired this fight - cleared every Refresh() so each fresh fight (or
    // rematch) gets all six available again, and each one shows at most once.
    readonly HashSet<int> usedTauntIndices = new HashSet<int>();
    const int TauntPlayerStruggling = 0;
    const int TauntRivalDefense = 1;
    const int TauntRivalStruggling = 2;
    const int TauntPlayerCombo = 3;
    const int TauntRivalCombo = 4;
    const int TauntFinalPhase = 5;
    // Milestone 62, Part 8: Scratch's own personality-driven reaction to
    // landing a critical hit on the player - same once-per-fight mechanism.
    const int TauntRivalCriticalHit = 6;

    // Milestone 32: the opponent's "portrait archetype" used for both the
    // battle-stage portrait and the intro's Tale of the Tape archetype row -
    // computed once per Refresh() rather than re-derived in multiple places.
    ArchetypeType introOpponentArchetype;

    // Milestone 32, Part 7/8: presentation-only counters for the Victory/Defeat
    // screens - never saved, reset every Refresh(), read by GameManager just
    // before EndBattle is called.
    int turnsThisFight;
    string lastComboNameThisFight;

    // Milestone 62, Part 6/7/12: a simple turn-gap cooldown so commentary
    // never fires two turns in a row - reset every Refresh() like every
    // other per-fight counter above. Starts high so a strong trigger can
    // still fire on an early turn rather than always waiting out a cooldown
    // that hasn't actually been "used" yet.
    int turnsSinceLastCommentary = 99;

    // Milestone 36, Part 4: LEVEL/ARCHETYPE/RECORD moved into the identity
    // block above this table (alongside the new nickname/streak/flavor text),
    // so this table is now exactly the brief's 8 combat stats. The row-build
    // loop in the constructor is still parameterized by this array's length.
    static readonly string[] TapeOfTheTapeLabels =
        { "HP", "STAMINA", "STRENGTH", "DEFENSE", "SPEED", "STRIKING", "GRAPPLING", "SUBMISSION" };

    static readonly Color HealthColor = new Color(0.62f, 0.13f, 0.12f, 1f);
    static readonly Color StaminaColor = new Color(0.18f, 0.5f, 0.62f, 1f);
    static readonly Color CritColor = new Color(1f, 0.65f, 0.1f, 1f);
    static readonly Color HitFlashColor = new Color(0.8f, 0.15f, 0.12f, 1f);
    static readonly Color HealFlashColor = new Color(0.3f, 0.7f, 0.3f, 0.6f);
    static readonly Color MissColor = new Color(0.75f, 0.75f, 0.75f, 1f);

    // Milestone 37: the five battle arena backgrounds, keyed exactly to their
    // filenames under Art/Backgrounds/ - ArtRegistry.GetBackground(key) already
    // loads Art/Backgrounds/{key} generically, so no new ArtRegistry method is
    // needed for any of these.
    const string BackgroundStandard = "battle_background";
    const string BackgroundGymChampion = "battle_background_gym";
    const string BackgroundStreetFight = "battle_background_street";
    const string BackgroundChampionship = "battle_background_champ";
    const string BackgroundFinale = "battle_background_finale";

    public BattleScreen(Transform parent, GameManager gm) : base(parent, gm, "BattleScreen", BackgroundStandard)
    {
        fightBilling = UIFactory.CreateText(Root.transform, "", UIFactory.CaptionSize, UIFactory.GoldColor,
            TextAnchor.MiddleCenter, new Vector2(0.04f, 0.966f), new Vector2(0.96f, 0.997f), FontStyle.Bold);
        // Typography pass: this is a persistent match-title reminder during
        // the whole fight - was capped at the smallest text tier in the app.
        // Best-fit lets it render larger when the billing text is short
        // (most of the time) while still safely shrinking for longer titles,
        // without changing this strip's existing box.
        fightBilling.resizeTextForBestFit = true;
        fightBilling.resizeTextMinSize = 16;
        fightBilling.resizeTextMaxSize = 28;

        // Landscape Conversion (Milestone 26): Player | VS | Opponent across the
        // full width, with each fighter's name/portrait/bars sitting directly
        // beneath their own side and the battle log filling the center column -
        // the layout the brief asks for, using 16:9 width instead of stacking.
        // Milestone 64, Part 3: card height grown 0.14 -> 0.163 (0.155-0.318,
        // was 0.155-0.295) to match the enlarged log panel's new height
        // exactly - the three center-row elements (player card / log /
        // opponent card) now share the same top and bottom edges instead of
        // the cards looking short next to a taller log.
        var opponentCard = UIFactory.CreateFighterCard(Root.transform, "Opponent",
            new Vector2(0.66f, 0.155f), new Vector2(0.98f, 0.318f), out opponentPortrait, out var opponentInfo);
        opponentFx = AttachFx(opponentCard);

        // Milestone 64, Part 3/10: best-fit added so a long name+nickname+level
        // string shrinks to fit on one line instead of wrapping and overflowing
        // into the stat rows below (the "long names/nicknames clipping" bug -
        // this Text previously had no overflow safety at all).
        opponentName = UIFactory.CreateText(opponentInfo, "", UIFactory.SubheadingSize, UIFactory.CreamColor, TextAnchor.MiddleLeft,
            new Vector2(0f, 0.62f), new Vector2(1f, 1f), FontStyle.Bold);
        opponentName.resizeTextForBestFit = true;
        opponentName.resizeTextMinSize = 14;
        opponentName.resizeTextMaxSize = UIFactory.SubheadingSize;
        BuildStatRow(opponentInfo, new Vector2(0f, 0.34f), new Vector2(1f, 0.6f), "HP", HealthColor, out opponentHealth, out opponentHealthValue);
        BuildStatRow(opponentInfo, new Vector2(0f, 0.06f), new Vector2(1f, 0.32f), "STM", StaminaColor, out opponentStamina, out opponentStaminaValue);

        // Milestone 64, Part 9/10: moved into the 0.70-0.82 gap between
        // recoverButton and clinchButton - the old 0.66-0.98 box overlapped
        // both recoverButton and clinchButton whenever an effect was active
        // (same Y band as the button row below).
        opponentEffectsText = UIFactory.CreateCaption(Root.transform, "", new Vector2(0.70f, 0.135f), new Vector2(0.82f, 0.153f));
        opponentEffectsText.color = UIFactory.GoldColor;

        var playerCard = UIFactory.CreateFighterCard(Root.transform, "Player",
            new Vector2(0.02f, 0.155f), new Vector2(0.34f, 0.318f), out playerPortrait, out var playerInfo);
        playerFx = AttachFx(playerCard);

        playerName = UIFactory.CreateText(playerInfo, "", UIFactory.SubheadingSize, UIFactory.CreamColor, TextAnchor.MiddleLeft,
            new Vector2(0f, 0.62f), new Vector2(1f, 1f), FontStyle.Bold);
        playerName.resizeTextForBestFit = true;
        playerName.resizeTextMinSize = 14;
        playerName.resizeTextMaxSize = UIFactory.SubheadingSize;
        // Milestone 50, Part 8: unified to the same HealthColor the opponent's
        // bar already uses (was a separate ad-hoc green) - one fighter's HP
        // bar shouldn't be a different hue than the other's for the same stat.
        BuildStatRow(playerInfo, new Vector2(0f, 0.34f), new Vector2(1f, 0.6f), "HP", HealthColor, out playerHealth, out playerHealthValue);
        BuildStatRow(playerInfo, new Vector2(0f, 0.06f), new Vector2(1f, 0.32f), "STM", StaminaColor, out playerStamina, out playerStaminaValue);

        // Milestone 64, Part 9/10: moved into the 0.18-0.30 gap between
        // parryButton and itemButton - the old 0.02-0.34 box overlapped both
        // whenever an effect was active.
        playerEffectsText = UIFactory.CreateCaption(Root.transform, "", new Vector2(0.18f, 0.135f), new Vector2(0.30f, 0.153f));
        playerEffectsText.color = UIFactory.GoldColor;

        stageCard = UIFactory.CreateCard(Root.transform, "FightStage", new Vector2(0.02f, 0.32f), new Vector2(0.98f, 0.94f),
            new Color(UIFactory.BackgroundColor.r, UIFactory.BackgroundColor.g, UIFactory.BackgroundColor.b, 0.88f));
        stageGroup = stageCard.gameObject.AddComponent<CanvasGroup>();

        var auraGo = new GameObject("ChampionAura", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        auraGo.transform.SetParent(stageCard, false);
        var auraRt = auraGo.GetComponent<RectTransform>();
        auraRt.anchorMin = new Vector2(0.6f, 0.08f);
        auraRt.anchorMax = new Vector2(0.92f, 0.94f);
        auraRt.offsetMin = Vector2.zero;
        auraRt.offsetMax = Vector2.zero;
        opponentAura = auraGo.GetComponent<Image>();
        opponentAura.sprite = UIFactory.CircleSprite;
        opponentAura.color = new Color(UIFactory.GoldColor.r, UIFactory.GoldColor.g, UIFactory.GoldColor.b, 0.22f);
        opponentAura.raycastTarget = false;

        // Milestone 64, Part 8: matches the enlarged baseline box Refresh()
        // applies every fight (see the championshipFight ternary below) -
        // this initial value only matters for the brief instant before the
        // first Refresh() runs.
        playerCombatantRoot = UIFactory.CreateBattleFighter(stageCard, "Player",
            new Vector2(0.02f, 0.01f), new Vector2(0.48f, 0.99f), out playerBattleSprite);
        opponentCombatantRoot = UIFactory.CreateBattleFighter(stageCard, "Opponent",
            new Vector2(0.52f, 0.01f), new Vector2(0.98f, 0.99f), out opponentBattleSprite);
        playerCombatant = playerCombatantRoot.gameObject.AddComponent<BattleFighterVisual>();
        opponentCombatant = opponentCombatantRoot.gameObject.AddComponent<BattleFighterVisual>();

        UIFactory.CreateText(stageCard, "VS", UIFactory.SubheadingSize, UIFactory.GoldColor, TextAnchor.MiddleCenter,
            new Vector2(0.45f, 0.5f), new Vector2(0.55f, 0.72f), FontStyle.Bold);

        // Milestone 64, Part 2: the log panel now spans the full old log+chain
        // band (was two separate elements, 0.155-0.275 and 0.277-0.295, with a
        // small gap between) as one taller, slightly wider card - chainText
        // becomes its header row instead of a separate floating caption, and
        // the scrollable text area below it gets real padding instead of a
        // ~1% margin. Height: 0.163 (was 0.12, +36%). Width: 0.31 (was 0.28).
        var logCard = UIFactory.CreateCard(Root.transform, "LogBackdrop", new Vector2(0.345f, 0.155f), new Vector2(0.655f, 0.318f),
            new Color(UIFactory.BackgroundColor.r, UIFactory.BackgroundColor.g, UIFactory.BackgroundColor.b, 0.88f));

        // Milestone 31, Part 5: a small, unobtrusive readout of the player's
        // in-progress move chain (e.g. "Jab -> Jab"), so combos can be found
        // by noticing the pattern rather than only by reading a wiki. Now the
        // log card's own header row instead of a separate element above it.
        chainText = UIFactory.CreateCaption(logCard, "", new Vector2(0.04f, 0.84f), new Vector2(0.96f, 0.98f), TextAnchor.MiddleCenter);
        chainText.color = UIFactory.MutedTextColor;
        chainText.fontStyle = FontStyle.Italic;

        // Milestone 64, Part 2: a real scroll view - was a fixed Text that
        // silently dropped everything past the last 4 lines. Viewport clips
        // to the padded inner area (4% side margins, 3% bottom), Content
        // (logText itself) grows downward via ContentSizeFitter as lines are
        // added, and AppendLog snaps verticalNormalizedPosition to 0 (bottom)
        // on every update so the newest line is always the one in view.
        var logViewportGo = new GameObject("LogViewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
        logViewportGo.transform.SetParent(logCard, false);
        var logViewportRt = (RectTransform)logViewportGo.transform;
        logViewportRt.anchorMin = new Vector2(0.04f, 0.03f);
        logViewportRt.anchorMax = new Vector2(0.96f, 0.80f);
        logViewportRt.offsetMin = Vector2.zero;
        logViewportRt.offsetMax = Vector2.zero;
        var logViewportImage = logViewportGo.GetComponent<Image>();
        logViewportImage.color = new Color(0f, 0f, 0f, 0.001f);

        // Milestone 28: bumped from CaptionSize - on a typical 16:9 laptop display
        // the canvas scale factor lands well under 1.0, so the log was reading
        // smaller than intended for the most important screen in the game.
        logText = UIFactory.CreateText(logViewportRt, "", UIFactory.BodySize, UIFactory.CreamColor, TextAnchor.UpperLeft,
            Vector2.zero, Vector2.one);
        var logTextRt = logText.rectTransform;
        logTextRt.anchorMin = new Vector2(0f, 1f);
        logTextRt.anchorMax = new Vector2(1f, 1f);
        logTextRt.pivot = new Vector2(0.5f, 1f);
        logTextRt.sizeDelta = new Vector2(0f, 0f);
        logTextRt.anchoredPosition = Vector2.zero;
        var logFitter = logText.gameObject.AddComponent<ContentSizeFitter>();
        logFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        logScroll = logCard.gameObject.AddComponent<ScrollRect>();
        logScroll.viewport = logViewportRt;
        logScroll.content = logTextRt;
        logScroll.horizontal = false;
        logScroll.vertical = true;
        logScroll.movementType = ScrollRect.MovementType.Clamped;
        logScroll.scrollSensitivity = 18f;

        itemButton = UIFactory.CreateButton(Root.transform, "ITEMS", new Vector2(0.30f, 0.115f), new Vector2(0.46f, 0.148f),
            () => ToggleItemPanel(), UIFactory.SecondaryColor);

        // Milestone 30, Part 5: Recover spends the player's turn for a much
        // bigger stamina gain than passive regen alone - the counterweight to
        // the lowered regen rate below.
        // Milestone 64, Part 7: was AccentOrange (flat) - orange across 5 of
        // the 9 action-row buttons (Recover + 4 moves) at once is what made
        // the row read as "orange-dominated". Now the same dark neutral body
        // as Items/Parry/Clinch, with orange reserved for hover/selected via
        // ApplyActionHoverTint below - gold stays the untouched premium
        // accent elsewhere (HUD labels, VS, etc), not a button body color.
        recoverButton = UIFactory.CreateButton(Root.transform, "RECOVER", new Vector2(0.54f, 0.115f), new Vector2(0.70f, 0.148f),
            () => OnRecoverSelected(), UIFactory.SecondaryColor);
        ApplyActionHoverTint(recoverButton);

        // Milestone 40, Part 1: PARRY/CLINCH - two universal defensive actions
        // sitting in the side margins of the same button row (0.02-0.30 and
        // 0.70-0.98 were both empty), so no existing button needs to move.
        parryButton = UIFactory.CreateButton(Root.transform, "PARRY", new Vector2(0.02f, 0.115f), new Vector2(0.18f, 0.148f),
            () => OnParrySelected(), UIFactory.SecondaryColor);
        clinchButton = UIFactory.CreateButton(Root.transform, "CLINCH", new Vector2(0.82f, 0.115f), new Vector2(0.98f, 0.148f),
            () => OnClinchSelected(), UIFactory.SecondaryColor);

        itemContainer = UIFactory.CreateContainer(Root.transform, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.105f));
        itemContainer.gameObject.SetActive(false);

        // Milestone 64, Part 5/7: move buttons rebuilt on CreateCardButton (the
        // same interactive shell - hover/press tint, ButtonPunch, click sound -
        // but without the single auto-fit Text) so Name/Cost/Tag can be 3
        // separately-sized children instead of one blob. Dark neutral body to
        // match Recover/Items/Parry/Clinch; small internal padding (6% each
        // side) on every tier instead of text running edge to edge.
        moveButtons = new Button[4];
        moveNameTexts = new Text[4];
        moveCostTexts = new Text[4];
        moveTagTexts = new Text[4];
        for (int i = 0; i < 4; i++)
        {
            int index = i;
            float xMin = 0.02f + i * 0.245f;
            float xMax = xMin + 0.225f;

            var btn = UIFactory.CreateCardButton(Root.transform, "Move" + i, new Vector2(xMin, 0.02f), new Vector2(xMax, 0.105f),
                () => OnMoveSelected(index), UIFactory.SecondaryColor);
            ApplyActionHoverTint(btn);
            moveButtons[i] = btn;

            moveNameTexts[i] = UIFactory.CreateText(btn.transform, "", UIFactory.BodySize, UIFactory.CreamColor,
                TextAnchor.MiddleCenter, new Vector2(0.06f, 0.56f), new Vector2(0.94f, 0.92f), FontStyle.Bold);
            moveNameTexts[i].resizeTextForBestFit = true;
            moveNameTexts[i].resizeTextMinSize = 16;
            moveNameTexts[i].resizeTextMaxSize = UIFactory.BodySize;

            moveCostTexts[i] = UIFactory.CreateText(btn.transform, "", UIFactory.CaptionSize, UIFactory.GoldColor,
                TextAnchor.MiddleCenter, new Vector2(0.06f, 0.30f), new Vector2(0.94f, 0.54f));
            moveCostTexts[i].resizeTextForBestFit = true;
            moveCostTexts[i].resizeTextMinSize = 11;
            moveCostTexts[i].resizeTextMaxSize = 15;

            moveTagTexts[i] = UIFactory.CreateText(btn.transform, "", UIFactory.CaptionSize, UIFactory.MutedTextColor,
                TextAnchor.MiddleCenter, new Vector2(0.06f, 0.06f), new Vector2(0.94f, 0.28f), FontStyle.Italic);
            moveTagTexts[i].resizeTextForBestFit = true;
            moveTagTexts[i].resizeTextMinSize = 8;
            moveTagTexts[i].resizeTextMaxSize = 11;
        }

        introCard = UIFactory.CreateCard(Root.transform, "FightIntroduction",
            new Vector2(0.06f, 0.155f), new Vector2(0.94f, 0.94f),
            new Color(UIFactory.BackgroundColor.r, UIFactory.BackgroundColor.g, UIFactory.BackgroundColor.b, 0.98f));
        introGroup = introCard.gameObject.AddComponent<CanvasGroup>();

        // Tap-to-skip: the card itself is a Button, but every child Graphic inside it
        // needs raycastTarget off or it would intercept the click first.
        var skipButton = introCard.gameObject.AddComponent<Button>();
        skipButton.transition = Selectable.Transition.None;
        skipButton.targetGraphic = introCard.GetComponent<Image>();
        skipButton.onClick.AddListener(SkipIntro);

        // Milestone 64, Part 6: header tightened into one cohesive title
        // block (was y 0.79-0.97, height 0.18) - freed height goes toward
        // the enlarged fighter cards below.
        introBillingText = UIFactory.CreateText(introCard, "", UIFactory.SubheadingSize, UIFactory.GoldColor,
            TextAnchor.MiddleCenter, new Vector2(0.04f, 0.895f), new Vector2(0.96f, 0.97f), FontStyle.Bold);
        introBillingText.raycastTarget = false;
        introAnnouncementText = UIFactory.CreateText(introCard, "", UIFactory.CaptionSize, UIFactory.MutedTextColor,
            TextAnchor.MiddleCenter, new Vector2(0.04f, 0.84f), new Vector2(0.96f, 0.895f));
        introAnnouncementText.raycastTarget = false;

        // Milestone 64, Part 1/2/4: the three-column face-off - LEFT (player
        // sprite + tiered identity), CENTER (VS), RIGHT (opponent sprite +
        // tiered identity). Was y 0.555-0.79 (height 0.235); now 0.40-0.82
        // (height 0.42, ~79% taller) so both the sprite and identity block
        // below it can grow proportionally (Part 2/4's "noticeably larger").
        introMatchupGroup = UIFactory.CreateContainer(introCard, new Vector2(0f, 0.40f), new Vector2(1f, 0.82f));

        var playerPortraitGo = new GameObject("IntroPlayerPortrait", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        playerPortraitGo.transform.SetParent(introMatchupGroup, false);
        var playerPortraitRt = playerPortraitGo.GetComponent<RectTransform>();
        playerPortraitRt.anchorMin = new Vector2(0.02f, 0.30f);
        playerPortraitRt.anchorMax = new Vector2(0.34f, 1f);
        playerPortraitRt.offsetMin = Vector2.zero;
        playerPortraitRt.offsetMax = Vector2.zero;
        introPlayerPortrait = playerPortraitGo.GetComponent<Image>();
        introPlayerPortrait.preserveAspect = true;
        introPlayerPortrait.raycastTarget = false;

        var opponentPortraitGo = new GameObject("IntroOpponentPortrait", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        opponentPortraitGo.transform.SetParent(introMatchupGroup, false);
        var opponentPortraitRt = opponentPortraitGo.GetComponent<RectTransform>();
        opponentPortraitRt.anchorMin = new Vector2(0.66f, 0.30f);
        opponentPortraitRt.anchorMax = new Vector2(0.98f, 1f);
        opponentPortraitRt.offsetMin = Vector2.zero;
        opponentPortraitRt.offsetMax = Vector2.zero;
        introOpponentPortrait = opponentPortraitGo.GetComponent<Image>();
        introOpponentPortrait.preserveAspect = true;
        introOpponentPortrait.raycastTarget = false;

        // Milestone 64, Part 2/5: split into 3 tiers (Name / Nickname /
        // Details) instead of one undifferentiated 5-line block - same data,
        // real typographic hierarchy. Name uses BodySize (a clear step up
        // from the old CaptionSize-for-everything), Nickname sits between
        // the two, Details (archetype/level/Prestige + record/streak +
        // age/reach/style) is the smallest "supporting information" tier.
        introPlayerNameText = UIFactory.CreateText(introMatchupGroup, "", UIFactory.BodySize, UIFactory.CreamColor,
            TextAnchor.MiddleCenter, new Vector2(0.0f, 0.20f), new Vector2(0.36f, 0.30f), FontStyle.Bold);
        introPlayerNameText.raycastTarget = false;
        introPlayerNameText.resizeTextForBestFit = true;
        introPlayerNameText.resizeTextMinSize = 14;
        introPlayerNameText.resizeTextMaxSize = UIFactory.BodySize;

        introPlayerNicknameText = UIFactory.CreateText(introMatchupGroup, "", UIFactory.CaptionSize, UIFactory.GoldColor,
            TextAnchor.MiddleCenter, new Vector2(0.0f, 0.12f), new Vector2(0.36f, 0.20f), FontStyle.Italic);
        introPlayerNicknameText.raycastTarget = false;
        introPlayerNicknameText.resizeTextForBestFit = true;
        introPlayerNicknameText.resizeTextMinSize = 11;
        introPlayerNicknameText.resizeTextMaxSize = UIFactory.CaptionSize;

        introPlayerDetailsText = UIFactory.CreateText(introMatchupGroup, "", UIFactory.CaptionSize, UIFactory.MutedTextColor,
            TextAnchor.UpperCenter, new Vector2(0.0f, 0.0f), new Vector2(0.36f, 0.12f));
        introPlayerDetailsText.raycastTarget = false;
        introPlayerDetailsText.resizeTextForBestFit = true;
        introPlayerDetailsText.resizeTextMinSize = 9;
        introPlayerDetailsText.resizeTextMaxSize = UIFactory.CaptionSize;

        introOpponentNameText = UIFactory.CreateText(introMatchupGroup, "", UIFactory.BodySize, UIFactory.CreamColor,
            TextAnchor.MiddleCenter, new Vector2(0.64f, 0.20f), new Vector2(1f, 0.30f), FontStyle.Bold);
        introOpponentNameText.raycastTarget = false;
        introOpponentNameText.resizeTextForBestFit = true;
        introOpponentNameText.resizeTextMinSize = 14;
        introOpponentNameText.resizeTextMaxSize = UIFactory.BodySize;

        introOpponentNicknameText = UIFactory.CreateText(introMatchupGroup, "", UIFactory.CaptionSize, UIFactory.GoldColor,
            TextAnchor.MiddleCenter, new Vector2(0.64f, 0.12f), new Vector2(1f, 0.20f), FontStyle.Italic);
        introOpponentNicknameText.raycastTarget = false;
        introOpponentNicknameText.resizeTextForBestFit = true;
        introOpponentNicknameText.resizeTextMinSize = 11;
        introOpponentNicknameText.resizeTextMaxSize = UIFactory.CaptionSize;

        introOpponentDetailsText = UIFactory.CreateText(introMatchupGroup, "", UIFactory.CaptionSize, UIFactory.MutedTextColor,
            TextAnchor.UpperCenter, new Vector2(0.64f, 0.0f), new Vector2(1f, 0.12f));
        introOpponentDetailsText.raycastTarget = false;
        introOpponentDetailsText.resizeTextForBestFit = true;
        introOpponentDetailsText.resizeTextMinSize = 9;
        introOpponentDetailsText.resizeTextMaxSize = UIFactory.CaptionSize;

        // Milestone 64, Part 7: anchored in the gap between the two sprite
        // columns, vertically centered on the sprite zone instead of
        // floating - feels like it belongs between the two cards.
        var introVsText = UIFactory.CreateText(introMatchupGroup, "VS", UIFactory.SubheadingSize, UIFactory.GoldColor,
            TextAnchor.MiddleCenter, new Vector2(0.42f, 0.55f), new Vector2(0.58f, 0.80f), FontStyle.Bold);
        introVsText.raycastTarget = false;

        // Milestone 64, Part 3: now y 0.12-0.38 (was 0.135-0.535) - shorter
        // overall, since the fix here is horizontal (values right next to
        // labels, see SetTapeRow's TextAnchor change below), not vertical.
        introTapeGroup = UIFactory.CreateContainer(introCard, new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.38f));
        for (int i = 0; i < TapeOfTheTapeLabels.Length; i++)
        {
            float slot = 1f / TapeOfTheTapeLabels.Length;
            float yMax = 1f - i * slot - slot * 0.07f;
            float yMin = yMax - slot * 0.86f;

            // Milestone 64, Part 3: anchor flipped from MiddleLeft to
            // MiddleRight - the value now sits at the box's right edge,
            // directly against the label's left edge, instead of at the
            // box's far-left (screen edge), which is what created the
            // "numbers far from labels" gap the brief called out.
            tapePlayerValues[i] = UIFactory.CreateText(introTapeGroup, "", UIFactory.BodySize, UIFactory.CreamColor,
                TextAnchor.MiddleRight, new Vector2(0f, yMin), new Vector2(0.32f, yMax), FontStyle.Bold);
            tapePlayerValues[i].raycastTarget = false;

            var tapeLabel = UIFactory.CreateText(introTapeGroup, TapeOfTheTapeLabels[i], UIFactory.CaptionSize, UIFactory.GoldColor,
                TextAnchor.MiddleCenter, new Vector2(0.32f, yMin), new Vector2(0.68f, yMax), FontStyle.Bold);
            tapeLabel.raycastTarget = false;

            // Same flip, mirrored: MiddleRight -> MiddleLeft, so this value
            // sits directly against the label's right edge.
            tapeOpponentValues[i] = UIFactory.CreateText(introTapeGroup, "", UIFactory.BodySize, UIFactory.CreamColor,
                TextAnchor.MiddleLeft, new Vector2(0.68f, yMin), new Vector2(1f, yMax), FontStyle.Bold);
            tapeOpponentValues[i].raycastTarget = false;
        }
        introTapeGroup.gameObject.SetActive(false);

        // Milestone 36, Part 1/9: the only explicit "click to proceed" gate in
        // the whole intro - shown solely during the Tale of the Tape beat.
        // Milestone 50, Part 5/6: was PositiveColor (green) - this is the
        // intro's "click to proceed" action, not a value comparison/reward.
        introContinueButton = UIFactory.CreateButton(introCard, "CONTINUE", new Vector2(0.36f, 0.025f), new Vector2(0.64f, 0.11f),
            () => introContinueClicked = true, UIFactory.AccentOrange);
        introContinueButton.gameObject.SetActive(false);

        introText = UIFactory.CreateText(introCard, "", UIFactory.SubheadingSize, UIFactory.CreamColor,
            TextAnchor.MiddleCenter, new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.92f), FontStyle.Bold);
        introText.raycastTarget = false;

        // Milestone 23, Part 5: explicit acknowledgment prompt shown only during
        // dialogue beats (bio/quote), so a stray tap can't blow past important lines.
        introTapPrompt = UIFactory.CreateText(introCard, "TAP TO CONTINUE ▸", UIFactory.CaptionSize, UIFactory.MutedTextColor,
            TextAnchor.MiddleCenter, new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.08f), FontStyle.Italic);
        introTapPrompt.raycastTarget = false;
        introTapPrompt.gameObject.SetActive(false);

        introCard.gameObject.SetActive(false);
    }

    void SkipIntro()
    {
        AudioManager.Instance?.PlayClick();
        if (waitingForDialogueTap)
        {
            dialogueAdvanceRequested = true;
            waitingForDialogueTap = false;
        }
        else
        {
            introSkipRequested = true;
        }
    }

    // Milestone 64, Part 7: orange is reserved for hover/selected instead of
    // being the button's resting body color. Selectable's color tinting
    // multiplies the target Graphic's already-assigned color by the
    // ColorBlock entry for the current state (normalColor=white is already
    // a no-op 1x multiplier on every existing button), so dividing
    // AccentOrange by the button's dark neutral base gives the exact per-
    // channel multiplier that turns it orange on hover/selected without
    // touching CreateButton/CreateCardButton's shared defaults used by every
    // other screen in the game.
    static void ApplyActionHoverTint(Button btn)
    {
        Color baseColor = btn.targetGraphic.color;
        Color tint = new Color(
            UIFactory.AccentOrange.r / Mathf.Max(baseColor.r, 0.02f),
            UIFactory.AccentOrange.g / Mathf.Max(baseColor.g, 0.02f),
            UIFactory.AccentOrange.b / Mathf.Max(baseColor.b, 0.02f),
            1f);
        var colors = btn.colors;
        colors.highlightedColor = tint;
        colors.selectedColor = tint;
        btn.colors = colors;
    }

    static FighterCardFX AttachFx(RectTransform card)
    {
        var anchorGo = new GameObject("PopupAnchor", typeof(RectTransform));
        anchorGo.transform.SetParent(card, false);
        var anchorRt = anchorGo.GetComponent<RectTransform>();
        anchorRt.anchorMin = new Vector2(0.65f, 0.5f);
        anchorRt.anchorMax = new Vector2(0.65f, 0.5f);
        anchorRt.sizeDelta = Vector2.zero;

        var fx = card.gameObject.AddComponent<FighterCardFX>();
        fx.Initialize(card.GetComponent<Image>(), anchorGo.transform);
        return fx;
    }

    // Milestone 64, Part 4: label now bold (was the same weight as every
    // other muted caption in the game - easy to skim past) and the value
    // moved off the bar into its own dedicated zone (was overlaid directly
    // on top of the slider's right end, competing with the fill color for
    // legibility) - label / bar / value now read as three clearly separate
    // zones left to right instead of two overlapping ones.
    static void BuildStatRow(Transform parent, Vector2 anchorMin, Vector2 anchorMax, string label, Color barColor,
        out Slider slider, out Text valueText)
    {
        var row = UIFactory.CreateContainer(parent, anchorMin, anchorMax);
        var labelText = UIFactory.CreateText(row, label, UIFactory.CaptionSize, UIFactory.MutedTextColor, TextAnchor.MiddleLeft,
            new Vector2(0f, 0f), new Vector2(0.16f, 1f), FontStyle.Bold);
        labelText.resizeTextForBestFit = true;
        labelText.resizeTextMinSize = 9;
        labelText.resizeTextMaxSize = UIFactory.CaptionSize;
        slider = UIFactory.CreateSlider(row, new Vector2(0.18f, 0f), new Vector2(0.76f, 1f), barColor);
        if (label == "STM") slider.GetComponent<SmoothSlider>().SetLowEmphasis(true);
        valueText = UIFactory.CreateText(row, "", UIFactory.CaptionSize, UIFactory.CreamColor, TextAnchor.MiddleRight,
            new Vector2(0.78f, 0f), new Vector2(1f, 1f), FontStyle.Bold);
    }

    public void Refresh()
    {
        if (GM.Player == null || GM.CurrentOpponent == null || GM.CurrentBattle == null)
        {
            Debug.LogWarning("BattleScreen.Refresh: no active battle context.");
            return;
        }

        log.Clear();
        logText.text = "";
        chainText.text = "";
        showingItems = false;
        animatingTurn = true;
        itemContainer.gameObject.SetActive(false);
        playerFx.ClearPopups();
        opponentFx.ClearPopups();
        turnsThisFight = 0;
        lastComboNameThisFight = null;
        turnsSinceLastCommentary = 99;
        usedTauntIndices.Clear();

        // Names, portraits and the intro quote only change once per battle, so they're set here rather than every turn.
        string opponentNickname = GM.CurrentOpponentInfo != null && !string.IsNullOrEmpty(GM.CurrentOpponentInfo.Nickname)
            ? $" \"{GM.CurrentOpponentInfo.Nickname}\""
            : "";
        playerName.text = $"{GM.Player.Name}   Lv.{GM.Player.Stats.Level}";
        opponentName.text = GM.CurrentOpponentInfo != null
            ? $"{GM.CurrentOpponent.Name}{opponentNickname}   Lv.{GM.CurrentOpponent.Stats.Level}"
            : GM.CurrentOpponent.Name;
        fightBilling.text = GetFightBilling();

        Color playerTheme = IconFactory.GetArchetypeThemeColor(GM.Player.Archetype);
        // Milestone 44: Mirror Match is a literal reflection of the player -
        // use the player's own archetype (sprite, theme color, silhouette
        // fallback all key off this) instead of falling through to the
        // synthetic gym's Championship-type default, which used to render it
        // as a generic untinted silhouette.
        bool mirrorFight = IsMirrorMatch();
        Color opponentTheme = mirrorFight
            ? playerTheme
            : (GM.CurrentGym != null ? IconFactory.GetGymThemeColor(GM.CurrentGym.GymType) : UIFactory.AccentOrange);
        // Milestone 32 fix: Street Fight opponents carry their own randomly
        // rolled portrait archetype (set when the opponent was generated) -
        // previously this always fell through to the synthetic gym's
        // GymType.Boxing default, so every Street Fight opponent's portrait
        // silently showed as a Boxer regardless of what was actually rolled.
        introOpponentArchetype = IsStreetFight() && GM.CurrentStreetFightOpponent != null
            ? GM.CurrentStreetFightOpponent.PortraitArchetype
            : mirrorFight
                ? GM.Player.Archetype
                : (GM.CurrentGym != null ? IconFactory.GetPortraitArchetype(GM.CurrentGym.GymType) : ArchetypeType.Unspecified);
        UIFactory.SetPlayerAvatar(playerPortrait, GM.Player.Archetype, playerTheme);
        UIFactory.SetFighterPortrait(opponentPortrait, GM.CurrentOpponentInfo?.OpponentId, introOpponentArchetype, opponentTheme);

        UIFactory.AddDisciplineBadge(playerPortrait.transform.parent, GM.Player.Archetype, playerTheme);
        if (GM.CurrentGym != null)
            UIFactory.AddDisciplineBadge(opponentPortrait.transform.parent, IconFactory.GetGymIconShape(GM.CurrentGym.GymType), opponentTheme);

        // Reuse the same sprites/colors already resolved above for the intro screen's
        // own portraits, instead of looking them up a second time.
        introPlayerPortrait.sprite = playerPortrait.sprite;
        introPlayerPortrait.color = playerPortrait.color;
        introOpponentPortrait.sprite = opponentPortrait.sprite;
        introOpponentPortrait.color = opponentPortrait.color;
        // Milestone 46, Part 4: large character display - Tale of the Tape.
        // Player only - the tattoo represents the player's own career, not
        // an opponent's (Mirror Match's opponent is presentational only here).
        UIFactory.ApplyPrestigeTattoo(introPlayerPortrait, GM.PrestigeLevel);
        RefreshIdentityText();

        bool championshipFight = IsChampionshipFight();
        bool leaderFight = IsLeaderFight();
        bool rivalFight = IsRivalFight();
        ApplyArenaBackground();
        // Milestone 34, Part 3: the Rival Showdown gets its own violet tint -
        // Scratch's accent color - distinct from the Championship fight's gold/
        // amber and a plain trainer fight's neutral dark stage.
        // Milestone 44: Mirror Match gets its own cool, quiet blue-grey tint -
        // deliberately calmer than Rival's loud violet or Championship's gold,
        // matching its "quiet, strange, reflective" brief.
        stageCard.GetComponent<Image>().color = mirrorFight
            ? new Color(0.08f, 0.09f, 0.13f, 0.95f)
            : rivalFight
                ? new Color(0.14f, 0.09f, 0.18f, 0.94f)
                : championshipFight
                    ? new Color(0.16f, 0.12f, 0.04f, 0.94f)
                    : new Color(0.08f, 0.07f, 0.07f, 0.88f);
        opponentAura.gameObject.SetActive(championshipFight || leaderFight || rivalFight);
        opponentAura.color = mirrorFight
            ? new Color(0.6f, 0.68f, 0.78f, 0.22f)
            : rivalFight
                ? new Color(RivalDatabase.AccentColor.r, RivalDatabase.AccentColor.g, RivalDatabase.AccentColor.b, 0.26f)
                : new Color(UIFactory.GoldColor.r, UIFactory.GoldColor.g, UIFactory.GoldColor.b, 0.22f);
        // Milestone 64, Part 8: baseline (non-championship) fighters grown
        // closer to the championship box (was 0.04-0.48/0.03-0.98, a visibly
        // smaller, more inset box than championship's 0.01-0.49/0.01-1.0 for
        // no presentational reason) - championship still reads slightly
        // bigger (the intentional "this fight feels bigger" distinction is
        // kept), but regular fights no longer leave extra unused margin.
        playerCombatantRoot.anchorMin = championshipFight ? new Vector2(0.01f, 0.01f) : new Vector2(0.02f, 0.01f);
        playerCombatantRoot.anchorMax = championshipFight ? new Vector2(0.49f, 1f) : new Vector2(0.48f, 0.99f);
        opponentCombatantRoot.anchorMin = championshipFight ? new Vector2(0.51f, 0.01f) : new Vector2(0.52f, 0.01f);
        opponentCombatantRoot.anchorMax = championshipFight ? new Vector2(0.99f, 1f) : new Vector2(0.98f, 0.99f);
        playerCombatant.Initialize(playerBattleSprite, "player", GM.Player.Archetype, playerTheme, faceRight: true);
        opponentCombatant.Initialize(opponentBattleSprite, GM.CurrentOpponentInfo?.OpponentId, introOpponentArchetype,
            opponentTheme, faceRight: false);
        stageGroup.alpha = 0f;

        if (GM.CurrentOpponentInfo != null && !string.IsNullOrEmpty(GM.CurrentOpponentInfo.Quote))
        {
            string speaker = !string.IsNullOrEmpty(GM.CurrentOpponentInfo.Nickname) ? GM.CurrentOpponentInfo.Nickname : GM.CurrentOpponent.Name;
            log.Add($"<i><color=#C8C2B4>\"{GM.CurrentOpponentInfo.Quote}\" - {speaker}</color></i>");
            logText.text = log[0];
        }

        UpdateBars(instant: true);

        var moves = GM.Player.EquippedMoves;
        for (int i = 0; i < moveButtons.Length; i++)
        {
            bool hasMove = i < moves.Count;
            moveButtons[i].gameObject.SetActive(hasMove);
            if (hasMove)
            {
                var move = moves[i];
                // Milestone 41, Part 1/2: the lightweight [Tag] now travels with
                // the move into the move-selection buttons too, not just the
                // Moves Screen. Milestone 64, Part 5: split across the 3 tiered
                // Texts instead of one rich-text string - the tag's category
                // color now sets moveTagTexts[i].color directly rather than an
                // inline <color> tag, since it's a standalone element now.
                moveNameTexts[i].text = move.Name;
                moveCostTexts[i].text = $"{move.StaminaCost} Stamina";
                moveTagTexts[i].text = IconFactory.GetMoveCategoryLabel(move.Category);
                moveTagTexts[i].color = IconFactory.GetMoveCategoryColor(move.Category);
            }
        }

        UpdateMoveButtonStates();
        itemButton.interactable = false;
        recoverButton.interactable = false;
        parryButton.interactable = false;
        clinchButton.interactable = false;

        introCard.SetAsLastSibling();
        introCard.GetComponent<Image>().color = mirrorFight
            ? new Color(0.07f, 0.08f, 0.12f, 0.99f)
            : rivalFight
                ? new Color(0.16f, 0.1f, 0.2f, 0.98f)
                : championshipFight
                    ? new Color(0.2f, 0.15f, 0.05f, 0.98f)
                    : new Color(0.08f, 0.07f, 0.07f, 0.98f);
        RunAnimation(FightIntroRoutine());
    }

    // Fight Night presentation (Milestone 21): billing + announcement + portrait
    // slide-in + Tale of the Tape + Ready/Fight beats, scaled by fight tier
    // (trainer < leader < championship). Tap-anywhere-on-the-card skips straight
    // through via introSkipRequested, which every wait in here respects.
    IEnumerator FightIntroRoutine()
    {
        introSkipRequested = false;
        introCard.gameObject.SetActive(true);
        stageGroup.alpha = 0f;
        introGroup.alpha = 0f;
        introGroup.blocksRaycasts = true;

        // Milestone 44: Mirror Match shares GymType.Championship with the
        // real Championship Gym (no Leader is set, which already keeps it
        // out of the "CHAMPIONSHIP BOUT" leader-match branch elsewhere) but
        // still needs to be excluded here so it gets its own billing/hype
        // text instead of "CHAMPIONSHIP BOUT"/"Championship fight!".
        bool introMirrorFight = IsMirrorMatch();
        bool championship = IsChampionshipFight() && !introMirrorFight;
        bool leaderFight = IsLeaderFight() && !championship;
        bool rematch = leaderFight && GM.HasBecomeChampion();

        // Milestone 56, Part 1/2/7: the event name + event type now lead the
        // existing billing text (was just GetFightBilling() alone) - same
        // Gold/MMA-Champ text element, same reveal timing, no new UI. The
        // existing GetFightBilling()/GetFightAnnouncement() lines still
        // follow exactly as before, so this only adds two lines on top.
        introBillingText.text = $"{FightPromotionGenerator.GetEventName(GM)}\n{FightPromotionGenerator.GetEventType(GM)}\n{GetFightBilling()}";
        // Milestone 56, Part 3: a brief walkout line (role + name + record)
        // for major fights only - prepended onto the existing announcement
        // text, same element, same timing, no new beat/delay.
        string walkout = GetWalkoutLine();
        // Milestone 62, Part 4: the opponent's personality, shown the same
        // lightweight way - one extra line, only when one is actually
        // assigned (None shows nothing, e.g. regular gym trainers that
        // didn't get one). One-time per fight, not a per-turn cost.
        string personalityLine = GM.CurrentOpponentInfo != null && GM.CurrentOpponentInfo.Personality != FighterPersonality.None
            ? $"STYLE: {FighterPersonalityTraits.GetDisplayName(GM.CurrentOpponentInfo.Personality)}"
            : "";
        string announcement = GetFightAnnouncement(championship, leaderFight, rematch, IsStreetFight(), IsRivalFight(), introMirrorFight);
        var introLines = new List<string>();
        if (!string.IsNullOrEmpty(walkout)) introLines.Add(walkout);
        if (!string.IsNullOrEmpty(personalityLine)) introLines.Add(personalityLine);
        introLines.Add(announcement);
        introAnnouncementText.text = string.Join("\n", introLines);
        introText.text = "";
        introMatchupGroup.gameObject.SetActive(true);
        introTapeGroup.gameObject.SetActive(false);

        yield return FadeGroup(0f, 1f, 0.16f);
        yield return SlideIntroPortraitsIn();
        yield return WaitSkippable(championship ? 0.5f : leaderFight ? 0.35f : 0.22f);

        // Milestone 36, Part 1/8/9: the Tale of the Tape - identity strip and
        // stat table together, on screen at once, gated behind an explicit
        // CONTINUE click rather than an auto-timed skip. introSkipRequested
        // (tap-anywhere) is intentionally NOT honored here - Part 1 explicitly
        // forbids auto-skipping this screen.
        PopulateTapeOfTheTape();
        introTapeGroup.gameObject.SetActive(true);
        introContinueClicked = false;
        introContinueButton.gameObject.SetActive(true);
        while (!introContinueClicked)
            yield return null;
        introContinueButton.gameObject.SetActive(false);
        introMatchupGroup.gameObject.SetActive(false);
        introTapeGroup.gameObject.SetActive(false);

        // Personality beats (Milestone 22/23): a bio spotlight, then the opponent's
        // pre-fight trash talk - both reuse introText, both require an explicit tap
        // to advance (Part 5) instead of an auto-timed skip, with a portrait pop
        // (Part 7) and a larger font (Part 4) to make them feel noticeable.
        // Font System Overhaul, Part 2: this is the one Text in the game reused
        // for both dialogue AND dramatic hype beats - changing .fontSize alone
        // never changes .font (that's only set once, at creation), so the swap
        // has to be explicit here rather than inferred globally.
        introText.fontSize = UIFactory.SubheadingSize + 4;
        introText.font = UIFactory.DialogueFont;

        // Milestone 32, Part 10: the Street Fight risk/reward reveal. Difficulty
        // stays hidden on StreetFightScreen itself (the player takes the risk by
        // choosing to fight at all) - it's only safe to reveal now, after the
        // player has already committed and the fight is actually starting.
        if (IsStreetFight() && GM.CurrentStreetFightOpponent != null && GM.CurrentOpponentInfo != null)
        {
            var sf = GM.CurrentStreetFightOpponent;
            introText.text = $"DIFFICULTY: {sf.Difficulty.ToString().ToUpper()}\n" +
                $"Reward: {GM.CurrentOpponentInfo.RewardXP} XP / {GM.CurrentOpponentInfo.RewardCoins} Coins\n" +
                $"{GetStreetFightFlavor(sf.Difficulty)}";
            introText.color = UIFactory.GoldColor;
            PlayPulse(introText.rectTransform, 1.15f, 0.3f);
            yield return WaitForDialogueTap();
        }

        string bio = GM.CurrentOpponentInfo?.Bio;
        if (!string.IsNullOrEmpty(bio))
        {
            introText.text = $"{GM.CurrentOpponent.Name}\n{bio}";
            introText.color = UIFactory.CreamColor;
            PlayPulse(introOpponentPortrait.rectTransform, 1.15f, 0.3f);
            yield return WaitForDialogueTap();
        }

        string preFightQuote = GM.CurrentOpponentInfo?.Quote;
        if (!string.IsNullOrEmpty(preFightQuote))
        {
            introText.text = $"<i>\"{preFightQuote}\"</i>\n- {GM.CurrentOpponent.Name}";
            introText.color = UIFactory.GoldColor;
            PlayPulse(introOpponentPortrait.rectTransform, 1.18f, 0.32f);
            yield return WaitForDialogueTap();
        }
        introText.fontSize = UIFactory.SubheadingSize;
        introText.font = UIFactory.HeadlineFont;

        bool rivalFightHype = IsRivalFight();
        introText.text = introMirrorFight ? "MIRROR MATCH" : championship ? "CHAMPIONSHIP BOUT" : rivalFightHype ? "RIVAL SHOWDOWN" : leaderFight ? "LEADER CHALLENGE" : IsStreetFight() ? "STREET FIGHT" : "READY";
        // Milestone 34, Part 3: the rival fight's hype beat uses Scratch's own
        // accent color instead of the usual gold, so this moment reads as
        // distinct even at a glance. Milestone 44: Mirror Match uses a quiet
        // silver instead of gold/violet - deliberately understated next to
        // Rival's loud energy.
        introText.color = introMirrorFight ? new Color(0.75f, 0.78f, 0.85f, 1f) : rivalFightHype ? RivalDatabase.AccentColor : UIFactory.GoldColor;
        PlayPulse(introText.rectTransform, championship ? 1.18f : rivalFightHype ? 1.16f : leaderFight ? 1.12f : introMirrorFight ? 1.1f : 1.08f, 0.3f);
        yield return WaitSkippable(championship ? 0.55f : rivalFightHype ? 0.45f : leaderFight ? 0.4f : 0.28f);

        introText.text = "FIGHT!";
        introText.color = UIFactory.CreamColor;
        PlayPulse(introText.rectTransform, 1.2f, 0.24f);
        // Milestone 44: no confetti for Mirror Match - its quiet/reflective
        // tone calls for restraint instead of a festive celebration burst.
        if (championship) PlayCelebration(20);
        yield return WaitSkippable(0.26f);

        float elapsed = 0f;
        while (elapsed < 0.14f)
        {
            elapsed += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(elapsed / 0.14f);
            introGroup.alpha = 1f - p;
            stageGroup.alpha = p;
            yield return null;
        }

        stageGroup.alpha = 1f;
        introGroup.blocksRaycasts = false;
        introCard.gameObject.SetActive(false);
        animatingTurn = false;
        itemButton.interactable = true;
        UpdateMoveButtonStates();
        UpdateRecoverButtonState();
        UpdateDefenseButtonStates();
    }

    IEnumerator FadeGroup(float from, float to, float duration)
    {
        introGroup.alpha = from;
        float t = 0f;
        while (t < duration && !introSkipRequested)
        {
            t += Time.unscaledDeltaTime;
            introGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
            yield return null;
        }
        introGroup.alpha = to;
    }

    IEnumerator SlideIntroPortraitsIn()
    {
        var playerRt = introPlayerPortrait.rectTransform;
        var opponentRt = introOpponentPortrait.rectTransform;
        Vector2 playerStart = new Vector2(-220f, 0f);
        Vector2 opponentStart = new Vector2(220f, 0f);
        playerRt.anchoredPosition = playerStart;
        opponentRt.anchoredPosition = opponentStart;

        const float duration = 0.22f;
        float t = 0f;
        while (t < duration && !introSkipRequested)
        {
            t += Time.unscaledDeltaTime;
            float p = 1f - (1f - Mathf.Clamp01(t / duration)) * (1f - Mathf.Clamp01(t / duration));
            playerRt.anchoredPosition = Vector2.Lerp(playerStart, Vector2.zero, p);
            opponentRt.anchoredPosition = Vector2.Lerp(opponentStart, Vector2.zero, p);
            yield return null;
        }
        playerRt.anchoredPosition = Vector2.zero;
        opponentRt.anchoredPosition = Vector2.zero;
    }

    IEnumerator WaitSkippable(float duration)
    {
        float t = 0f;
        while (t < duration && !introSkipRequested)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    // Part 5: dialogue beats wait indefinitely for an explicit tap (no timeout),
    // so important lines can't be swept away by the generic skip-everything tap.
    IEnumerator WaitForDialogueTap()
    {
        dialogueAdvanceRequested = false;
        waitingForDialogueTap = true;
        introTapPrompt.gameObject.SetActive(true);
        while (!dialogueAdvanceRequested)
            yield return null;
        introTapPrompt.gameObject.SetActive(false);
    }

    // Milestone 36, Part 4: the brief's exact 8-stat comparison (HP, Stamina,
    // Strength, Defense, Speed, Striking, Grappling, Submission), each row
    // color-coded green/red/neutral for whichever side has the better value.
    // LEVEL/ARCHETYPE/RECORD (Milestone 32) now live in the identity strip's
    // text instead, populated by RefreshIdentityText - existing stat data
    // only, still no new combat stats.
    void PopulateTapeOfTheTape()
    {
        var p = GM.Player.Stats;
        var o = GM.CurrentOpponent.Stats;
        SetTapeRow(0, p.MaxHealth, o.MaxHealth);
        SetTapeRow(1, p.MaxStamina, o.MaxStamina);
        SetTapeRow(2, p.Strength, o.Strength);
        SetTapeRow(3, p.Defense, o.Defense);
        SetTapeRow(4, p.Speed, o.Speed);
        SetTapeRow(5, p.Striking, o.Striking);
        SetTapeRow(6, p.Grappling, o.Grappling);
        SetTapeRow(7, p.Submission, o.Submission);
    }

    void SetTapeRow(int index, int playerValue, int opponentValue)
    {
        tapePlayerValues[index].text = playerValue.ToString();
        tapeOpponentValues[index].text = opponentValue.ToString();

        // Typography pass: UIFactory.PositiveColor/DangerColor are tuned for
        // button backgrounds, not small text on the same near-black fill -
        // too dark to read at a glance, which defeats the point of a
        // scannable better/worse comparison. Milestone 50, Part 5/9: derived
        // (brightened) from those same unified theme constants instead of
        // fully independent values, so this stays in the same Green/Red
        // family as every other stat-comparison use in the game while
        // keeping the contrast boost small text on a dark fill needs.
        Color neutral = UIFactory.CreamColor;
        Color better = new Color(
            Mathf.Min(1f, UIFactory.PositiveColor.r * 1.3f),
            Mathf.Min(1f, UIFactory.PositiveColor.g * 1.3f),
            Mathf.Min(1f, UIFactory.PositiveColor.b * 1.3f), 1f);
        Color worse = new Color(
            Mathf.Min(1f, UIFactory.DangerColor.r * 1.2f),
            Mathf.Min(1f, UIFactory.DangerColor.g * 1.2f),
            Mathf.Min(1f, UIFactory.DangerColor.b * 1.2f), 1f);

        if (playerValue > opponentValue) { tapePlayerValues[index].color = better; tapeOpponentValues[index].color = worse; }
        else if (playerValue < opponentValue) { tapePlayerValues[index].color = worse; tapeOpponentValues[index].color = better; }
        else { tapePlayerValues[index].color = neutral; tapeOpponentValues[index].color = neutral; }
    }

    // Milestone 36, Parts 2/3/5: the face-off identity block - name+nickname,
    // archetype+level, record+win streak, and broadcast flavor stats (age/
    // reach/style) for each side. Player record/streak come from
    // GameManager's real lifetime counters; opponent record/streak are
    // generated (presentation-only, never saved) from the fight's existing
    // tier signals. Flavor stats are generated from archetype and are purely
    // cosmetic - never read by combat, never editable.
    void RefreshIdentityText()
    {
        var gm = GM;
        string playerNickname = FightPresentationGenerator.GetPlayerNickname(gm);
        string playerArchetype = ArchetypeDatabase.GetByType(gm.Player.Archetype)?.DisplayName ?? "Fighter";
        string playerRecord = FightPresentationGenerator.FormatRecord(gm.TotalWins, gm.TotalLosses);
        string playerStreak = FightPresentationGenerator.FormatStreak(gm.CurrentWinStreak);
        FightPresentationGenerator.GetFlavorStats(gm.Player.Archetype, "player_" + gm.Player.Archetype, gm.HasBecomeChampion(),
            out int playerAge, out int playerReach, out string playerStyle);

        // Milestone 64, Part 2/5: same exact data as before (Milestone 45's
        // Prestige line included), just split across the 3 tiered fields
        // instead of crammed into one block - Name gets its own prominent
        // line, Nickname its own styled line, everything else (archetype/
        // level/Prestige, record/streak, flavor stats) groups into Details.
        introPlayerNameText.text = gm.Player.Name;
        introPlayerNicknameText.text = string.IsNullOrEmpty(playerNickname) ? "" : $"\"{playerNickname}\"";
        introPlayerDetailsText.text =
            $"{playerArchetype}  -  Lv.{gm.Player.Stats.Level}  -  {PrestigeSystem.FormatLevel(gm.PrestigeLevel)}\n" +
            $"{playerRecord}{(string.IsNullOrEmpty(playerStreak) ? "" : "  (" + playerStreak + ")")}\n" +
            FightPresentationGenerator.FormatFlavorLine(playerAge, playerReach, playerStyle);

        var opponentInfo = gm.CurrentOpponentInfo;
        string opponentNickname = opponentInfo != null && !string.IsNullOrEmpty(opponentInfo.Nickname) ? opponentInfo.Nickname : "";
        string opponentArchetype = ArchetypeDatabase.GetByType(introOpponentArchetype)?.DisplayName ?? "Fighter";
        GetOpponentRecordAndStreak(out int opponentWins, out int opponentLosses, out int opponentStreak);
        string opponentRecord = FightPresentationGenerator.FormatRecord(opponentWins, opponentLosses);
        string opponentStreakText = FightPresentationGenerator.FormatStreak(opponentStreak);
        bool impressiveOpponent = IsChampionshipFight() || IsRivalFight();
        FightPresentationGenerator.GetFlavorStats(introOpponentArchetype, opponentInfo?.OpponentId ?? "opponent", impressiveOpponent,
            out int opponentAge, out int opponentReach, out string opponentStyle);

        introOpponentNameText.text = gm.CurrentOpponent.Name;
        introOpponentNicknameText.text = string.IsNullOrEmpty(opponentNickname) ? "" : $"\"{opponentNickname}\"";
        introOpponentDetailsText.text =
            $"{opponentArchetype}  -  Lv.{gm.CurrentOpponent.Stats.Level}\n" +
            $"{opponentRecord}{(string.IsNullOrEmpty(opponentStreakText) ? "" : "  (" + opponentStreakText + ")")}\n" +
            FightPresentationGenerator.FormatFlavorLine(opponentAge, opponentReach, opponentStyle);
    }

    // Milestone 36, Part 3/7: dispatches to the right generator based on the
    // existing fight-type signals BattleScreen already computes elsewhere
    // (championship/leader/street/rival/shadow/secret) - mirrors how
    // GetFightBilling already branches on the same set of fight types.
    void GetOpponentRecordAndStreak(out int wins, out int losses, out int streak)
    {
        string id = GM.CurrentOpponentInfo?.OpponentId ?? "";

        if (IsRivalFight())
        {
            FightPresentationGenerator.GetRivalRecord(GM, out wins, out losses, out streak);
        }
        else if (id == GameManager.ShadowChampionId || id == StreetFightGenerator.SecretFighterOpponentId)
        {
            FightPresentationGenerator.GetSpecialOpponentRecord(id, out wins, out losses, out streak);
        }
        else if (IsStreetFight() && GM.CurrentStreetFightOpponent != null)
        {
            FightPresentationGenerator.GetStreetFighterRecord(GM.CurrentStreetFightOpponent.Difficulty, id, out wins, out losses, out streak);
        }
        else if (GM.CurrentGym != null)
        {
            FightPresentationGenerator.GetGymOpponentRecord(GM.CurrentGym.GymType, IsLeaderFight(), id, out wins, out losses, out streak);
        }
        else
        {
            wins = 5; losses = 3; streak = 0;
        }
    }

    // Milestone 37, Part 1/2/8: reuses UIFactory.ApplyScreenBackground/
    // ArtRegistry.GetBackground - the exact same Art/Backgrounds/{key}
    // pipeline every other screen background already goes through - rather
    // than adding a second background system. Targeting stageCard (the
    // "FightStage" card) instead of Root means the arena image sits behind
    // the aura/combatants/VS text/log but the HP bars, fighter cards, item
    // panel and move buttons (all siblings of stageCard under Root, not
    // children of it) are untouched and stay above it. ApplyScreenBackground
    // already re-parents its "Background" child as first sibling on every
    // call, so this is safe to call again on every Refresh() with a new key.
    // Falls back to the standard arena if the specific key's sprite can't be
    // found, and ApplyScreenBackground itself already no-ops safely (no
    // exception) if even that one is missing - so this can never crash.
    void ApplyArenaBackground()
    {
        string key = GetBattleBackgroundKey();
        if (ArtRegistry.GetBackground(key) == null) key = BackgroundStandard;
        // Milestone 64, Part 1: stageCard is a wide, short strip (most of the
        // screen's width but well under its full height) - much flatter than
        // the ~16:9 source art, so the old preserveAspect "contain" fit left
        // large empty bars on both sides showing the card's own dark fill
        // through (the "black panels" this part calls out). coverFit scales
        // the art to fill the strip completely instead; RectMask2D clips the
        // resulting vertical overflow so it never spills onto the fighter
        // cards/log/buttons that sit just outside stageCard.
        if (stageCard.GetComponent<RectMask2D>() == null) stageCard.gameObject.AddComponent<RectMask2D>();
        UIFactory.ApplyScreenBackground(stageCard.gameObject, key, coverFit: true);
    }

    // Milestone 37, Parts 2-6: most-specific fight type first - the Shadow
    // Champion's synthetic gym is also GymType.Championship, so it has to be
    // checked before the generic Championship Gym check, same ordering
    // principle GetFightBilling already relies on.
    string GetBattleBackgroundKey()
    {
        // Milestone 39, Part 2 / Milestone 44: the finale background is for
        // the game's true climactic fights - Rival Showdown and, now that
        // it's unlocked only after the Rival is defeated, Mirror Match too.
        if (IsRivalFight() || IsMirrorMatch())
            return BackgroundFinale;
        if (IsChampionshipFight())
            return BackgroundChampionship;
        if (IsStreetFight())
            return BackgroundStreetFight;
        if (IsLeaderFight())
            return BackgroundGymChampion;
        return BackgroundStandard;
    }

    bool IsChampionshipFight()
    {
        return GM.CurrentGym != null && GM.CurrentGym.GymType == GymType.Championship;
    }

    bool IsLeaderFight()
    {
        return GM.CurrentGym?.Leader != null && GM.CurrentOpponentInfo != null &&
            GM.CurrentGym.Leader.OpponentId == GM.CurrentOpponentInfo.OpponentId;
    }

    // Milestone 32, Part 10: matches the synthetic GymInfo GameManager.StartStreetFight
    // creates (GymId "street_fight", no Leader) - same marker GameManager itself
    // relies on for "this fight shouldn't touch gym progression."
    bool IsStreetFight() => GM.CurrentGym?.GymId == "street_fight";

    // Milestone 34, Part 3: matches the synthetic GymInfo GameManager.StartRivalFight
    // creates (GymId "rival_fight", GymType.Boxing, no Leader) - deliberately
    // NOT GymType.Championship, so this fight gets its own presentation tier
    // instead of inheriting the Championship fight's aura/tint.
    bool IsRivalFight() => GM.CurrentGym?.GymId == "rival_fight";

    // Milestone 44: the Mirror Match shares its OpponentId with the existing
    // Shadow Champion system it was repurposed from - GameManager.ShadowChampionId
    // is unchanged for save compatibility even though the player-facing name,
    // dialogue, and presentation are now "Mirror Match."
    bool IsMirrorMatch() => GM.CurrentOpponentInfo?.OpponentId == GameManager.ShadowChampionId;

    // Milestone 43: short mid-fight taunts, Rival Showdown only. Reads the
    // SAME turn log BattleSystem already produces (combo/parry/clinch lines
    // are already detected elsewhere in this file for popups) plus the
    // current HP ratios already on GM.Player/GM.CurrentOpponent - no changes
    // to BattleSystem, no new dialogue system. At most one taunt per turn
    // (first matching, not-yet-used trigger in priority order below), and
    // each of the six only ever fires once per fight.
    void MaybeInjectRivalTaunt(List<string> turnLog)
    {
        if (!IsRivalFight() || GM.Player == null || GM.CurrentOpponent == null) return;

        float playerRatio = (float)GM.Player.Stats.CurrentHealth / GM.Player.Stats.MaxHealth;
        float rivalRatio = (float)GM.CurrentOpponent.Stats.CurrentHealth / GM.CurrentOpponent.Stats.MaxHealth;

        int triggered = -1;
        if (!usedTauntIndices.Contains(TauntFinalPhase) && (playerRatio < 0.2f || rivalRatio < 0.2f))
            triggered = TauntFinalPhase;
        else if (!usedTauntIndices.Contains(TauntRivalCriticalHit) && LogHasCriticalHitBy(turnLog, GM.CurrentOpponent.Name))
            triggered = TauntRivalCriticalHit;
        else if (!usedTauntIndices.Contains(TauntRivalCombo) && LogHasComboBy(turnLog, GM.CurrentOpponent.Name))
            triggered = TauntRivalCombo;
        else if (!usedTauntIndices.Contains(TauntPlayerCombo) && LogHasComboBy(turnLog, GM.Player.Name))
            triggered = TauntPlayerCombo;
        else if (!usedTauntIndices.Contains(TauntRivalStruggling) && rivalRatio < 0.5f)
            triggered = TauntRivalStruggling;
        else if (!usedTauntIndices.Contains(TauntPlayerStruggling) && playerRatio < 0.5f)
            triggered = TauntPlayerStruggling;
        else if (!usedTauntIndices.Contains(TauntRivalDefense) && LogHasFighterDefense(turnLog, GM.CurrentOpponent.Name))
            triggered = TauntRivalDefense;

        if (triggered < 0) return;
        usedTauntIndices.Add(triggered);

        // Same rich-text convention the opponent's pre-fight quote already
        // uses (Refresh(), below) - just the rival's own accent color instead
        // of the generic muted one, and no name-prefix so this never gets
        // mistaken for an attack-resolution line by TryGetAttack/ProcessSingleLine.
        string hex = ColorUtility.ToHtmlStringRGB(RivalDatabase.AccentColor);
        turnLog.Add($"<i><color=#{hex}>\"{RivalDatabase.ShowdownTaunts[triggered]}\" - {RivalDatabase.RivalName}</color></i>");
    }

    static bool LogHasComboBy(List<string> log, string fighterName)
    {
        for (int i = 0; i < log.Count; i++)
            if (log[i].StartsWith(fighterName) && log[i].Contains("lands a COMBO!")) return true;
        return false;
    }

    // Milestone 62, Part 8: same name-prefixed substring-match pattern as
    // LogHasComboBy above - "lands a CRITICAL" is the exact phrase
    // ResolveMove's crit hit-line already uses.
    static bool LogHasCriticalHitBy(List<string> log, string fighterName)
    {
        for (int i = 0; i < log.Count; i++)
            if (log[i].StartsWith(fighterName) && log[i].Contains("lands a CRITICAL")) return true;
        return false;
    }

    static bool LogHasFighterDefense(List<string> log, string fighterName)
    {
        for (int i = 0; i < log.Count; i++)
            if (log[i].StartsWith(fighterName) && (log[i].Contains("PARRY!") || log[i].Contains("CLINCH SUCCESS!"))) return true;
        return false;
    }

    // Milestone 62, Part 5/6/7/12: lightweight "crowd commentary" - reuses
    // the exact rich-text/log-append pattern the rival taunts above already
    // use (no new popup system, no risk to BattleScreen's existing keyword
    // parsing - verified none of these lines contain any of FormatLogLine's
    // or TryGetAttack's trigger substrings). Gated by a turn cooldown plus a
    // probability roll scaled by fight progress, so it's occasional, not
    // constant, and ramps up as the fight gets more dramatic (Part 7).
    // Skipped for Rival fights entirely - MaybeInjectRivalTaunt above
    // already owns that fight's commentary; duplicating it here would spam.
    static readonly string[] ComboCommentary = { "Huge combination!", "What a sequence!", "The crowd loves that!" };
    static readonly string[] CriticalHitCommentary = { "That shot landed clean!", "The crowd felt that one!", "Devastating!" };
    static readonly string[] ParryCommentary = { "Beautiful counter!", "Excellent defense!", "Read that perfectly!" };
    static readonly string[] ClinchCommentary = { "Smart clinch work!", "Smothering the attack!" };
    static readonly string[] HugeDamageCommentary = { "The crowd loves it!", "That's going to leave a mark!" };
    static readonly string[] LowHealthCommentary = { "One mistake could end it!", "This is getting dangerous!" };
    static readonly string[] CloseFinishCommentary = { "This fight has turned into a war!", "Either fighter could finish this!" };

    // Milestone 62, Part 9: Mirror Match gets its own much quieter,
    // introspective pool instead - "avoid crowd hype" per the brief.
    static readonly string[] MirrorMatchCommentary =
    {
        "You know every weakness.",
        "No one knows your habits better.",
        "One fighter refuses to break.",
        "Every move, already familiar."
    };

    const int HugeDamageThreshold = 18;

    void MaybeInjectCommentary(List<string> turnLog, int dealt, int taken)
    {
        if (IsRivalFight() || GM.Player == null || GM.CurrentOpponent == null) return;
        turnsSinceLastCommentary++;
        if (turnsSinceLastCommentary < 2) return;

        if (IsMirrorMatch())
        {
            MaybeInjectMirrorMatchCommentary(turnLog);
            return;
        }

        float playerRatio = (float)GM.Player.Stats.CurrentHealth / GM.Player.Stats.MaxHealth;
        float opponentRatio = (float)GM.CurrentOpponent.Stats.CurrentHealth / GM.CurrentOpponent.Stats.MaxHealth;
        bool eitherLow = playerRatio < 0.2f || opponentRatio < 0.2f;
        bool closeFinish = playerRatio < 0.3f && opponentRatio < 0.3f;

        string[] pool;
        if (closeFinish) pool = CloseFinishCommentary;
        else if (eitherLow) pool = LowHealthCommentary;
        else if (LogContains(turnLog, "lands a COMBO!")) pool = ComboCommentary;
        else if (LogContains(turnLog, "lands a CRITICAL")) pool = CriticalHitCommentary;
        else if (Mathf.Max(dealt, taken) >= HugeDamageThreshold) pool = HugeDamageCommentary;
        else if (LogContains(turnLog, "PARRY!")) pool = ParryCommentary;
        else if (LogContains(turnLog, "CLINCH SUCCESS!")) pool = ClinchCommentary;
        else return;

        // Part 7: crowd energy - almost none early, occasional by mid-fight,
        // more energetic once either fighter is in real danger.
        int chance = turnsThisFight <= 3 ? 8 : turnsThisFight <= 8 ? 18 : 30;
        if (eitherLow || closeFinish) chance += 15;
        if (UnityEngine.Random.Range(0, 100) >= chance) return;

        turnsSinceLastCommentary = 0;
        string line = pool[UnityEngine.Random.Range(0, pool.Length)];
        turnLog.Add($"<i><color=#C8C2B4>{line}</color></i>");
    }

    void MaybeInjectMirrorMatchCommentary(List<string> turnLog)
    {
        bool meaningfulMoment = LogContains(turnLog, "lands a COMBO!") || LogContains(turnLog, "lands a CRITICAL");
        float playerRatio = (float)GM.Player.Stats.CurrentHealth / GM.Player.Stats.MaxHealth;
        float opponentRatio = (float)GM.CurrentOpponent.Stats.CurrentHealth / GM.CurrentOpponent.Stats.MaxHealth;
        bool eitherLow = playerRatio < 0.2f || opponentRatio < 0.2f;
        if (!meaningfulMoment && !eitherLow) return;

        // Part 9: deliberately rare - no crowd-energy ramp, just an
        // occasional quiet observation.
        if (UnityEngine.Random.Range(0, 100) >= 12) return;

        turnsSinceLastCommentary = 0;
        string line = MirrorMatchCommentary[UnityEngine.Random.Range(0, MirrorMatchCommentary.Length)];
        turnLog.Add($"<i><color=#C8C2B4>{line}</color></i>");
    }

    static bool LogContains(List<string> log, string keyword)
    {
        for (int i = 0; i < log.Count; i++)
            if (log[i].Contains(keyword)) return true;
        return false;
    }

    // Milestone 56, Part 3 (Walkouts): a short role/name/record line for the
    // 4 fight categories the brief calls out (gym leader, championship,
    // rival, mirror match) - empty for everything else (regular trainers,
    // Street Fight), since walkouts are explicitly major-fights-only. Reuses
    // FightPresentationGenerator's existing deterministic record generators
    // (Milestone 36) - no new data, no new save fields.
    string GetWalkoutLine()
    {
        if (GM.CurrentOpponentInfo == null || GM.CurrentOpponent == null) return "";

        if (IsMirrorMatch())
        {
            FightPresentationGenerator.GetSpecialOpponentRecord(GM.CurrentOpponentInfo.OpponentId, out int w, out int l, out _);
            return $"THE REFLECTION\n{GM.CurrentOpponent.Name}  -  Record {w}-{l}";
        }
        if (IsRivalFight())
        {
            FightPresentationGenerator.GetRivalRecord(GM, out int w, out int l, out _);
            return $"THE RIVAL\n{RivalDatabase.RivalName}  -  Record {w}-{l}";
        }
        bool isLeader = IsLeaderFight();
        if (isLeader && GM.CurrentGym != null)
        {
            bool championship = GM.CurrentGym.GymType == GymType.Championship;
            FightPresentationGenerator.GetGymOpponentRecord(GM.CurrentGym.GymType, true, GM.CurrentOpponentInfo.OpponentId, out int w, out int l, out _);
            return $"{(championship ? "THE CHAMPION" : "THE GYM LEADER")}\n{GM.CurrentOpponent.Name}  -  Record {w}-{l}";
        }
        return "";
    }

    string GetFightBilling()
    {
        var gym = GM.CurrentGym;
        var opponent = GM.CurrentOpponentInfo;
        bool isLeader = gym?.Leader != null && opponent != null && gym.Leader.OpponentId == opponent.OpponentId;

        if (gym != null && gym.GymType == GymType.Championship && isLeader)
            return "CHAMPIONSHIP BOUT";
        if (IsRivalFight())
            return "RIVAL SHOWDOWN";
        if (IsMirrorMatch())
            return "MIRROR MATCH";
        if (IsStreetFight())
            return "STREET FIGHT";
        if (isLeader)
            return gym != null ? $"{gym.GymName.ToUpper()} MAIN EVENT" : "MAIN EVENT";
        return gym != null ? $"{gym.GymName.ToUpper()} SHOWDOWN" : "TONIGHT'S FEATURED MATCHUP";
    }

    static string GetFightAnnouncement(bool championship, bool leaderFight, bool rematch, bool streetFight, bool rivalFight, bool mirrorFight = false)
    {
        if (mirrorFight) return "No more opponents left. Just yourself.";
        if (championship) return "Championship fight!";
        if (rivalFight) return "The fight you've been waiting for.";
        if (streetFight) return "Random opponent. Anything can happen.";
        if (leaderFight) return rematch ? "Contender matchup!" : "Gym leader challenge!";
        return "Tonight's featured bout!";
    }

    // Milestone 32, Part 10: flavor only, shown during the intro reveal beat -
    // after the player has already committed to the fight from StreetFightScreen.
    static string GetStreetFightFlavor(StreetFightDifficulty difficulty)
    {
        switch (difficulty)
        {
            case StreetFightDifficulty.Easy: return "Looks manageable.";
            case StreetFightDifficulty.Normal: return "Anything can happen.";
            case StreetFightDifficulty.Hard: return "This one's gonna hurt.";
            default: return "...This looks dangerous.";
        }
    }

    void UpdateBars(bool instant = false)
    {
        var p = GM.Player.Stats;
        var o = GM.CurrentOpponent.Stats;

        playerHealth.GetComponent<SmoothSlider>().SetValue((float)p.CurrentHealth / p.MaxHealth, instant);
        playerStamina.GetComponent<SmoothSlider>().SetValue((float)p.CurrentStamina / p.MaxStamina, instant);
        opponentHealth.GetComponent<SmoothSlider>().SetValue((float)o.CurrentHealth / o.MaxHealth, instant);
        opponentStamina.GetComponent<SmoothSlider>().SetValue((float)o.CurrentStamina / o.MaxStamina, instant);

        playerHealthValue.text = $"{p.CurrentHealth}/{p.MaxHealth}";
        playerStaminaValue.text = $"{p.CurrentStamina}/{p.MaxStamina}";
        opponentHealthValue.text = $"{o.CurrentHealth}/{o.MaxHealth}";
        opponentStaminaValue.text = $"{o.CurrentStamina}/{o.MaxStamina}";

        playerEffectsText.text = FormatEffects(GM.Player);
        opponentEffectsText.text = FormatEffects(GM.CurrentOpponent);
    }

    // Milestone 31, Part 5: reads BattleSystem's own tracker rather than
    // duplicating it - empties itself naturally once a combo fires (the
    // tracker clears) or the player breaks the chain with Recover.
    void UpdateComboChainDisplay()
    {
        var ids = GM.CurrentBattle?.RecentPlayerMoveIds;
        if (ids == null || ids.Count == 0)
        {
            chainText.text = "";
            return;
        }

        var names = new List<string>(ids.Count);
        foreach (var id in ids)
        {
            var move = MoveDatabase.GetById(id);
            names.Add(move != null ? move.Name : id);
        }
        chainText.text = "Chain: " + string.Join(" -> ", names);
    }

    // Milestone 32, Part 7: parses the log text for the combo-trigger line
    // BattleSystem already emits, the same way the rest of this file reads
    // combat feedback from log strings - no new combo-system hooks needed.
    // Whichever fighter's combo appears last in the turn "wins" the credit;
    // since combos are rare and this is presentation-only, that's fine.
    void CaptureComboFromLog(List<string> lines)
    {
        foreach (var line in lines)
        {
            if (!line.Contains("lands a COMBO!")) continue;
            foreach (var combo in ComboDatabase.All)
            {
                if (line.Contains(combo.DisplayName))
                {
                    lastComboNameThisFight = combo.DisplayName;
                    break;
                }
            }
        }
    }

    string FormatEffects(FighterData fighter)
    {
        var effects = GM.CurrentBattle.GetEffects(fighter);
        if (effects.Count == 0) return "";

        var parts = new List<string>();
        foreach (var effect in effects)
            parts.Add($"<color=#{EffectColorHex(effect.Type)}>{effect.Type} ({effect.RemainingTurns})</color>");
        return string.Join("   ", parts);
    }

    // Matches the flash colors already used for these statuses in FighterCardFX/
    // BattleFighterVisual, so the effect text reads at a glance instead of as flat gold.
    static string EffectColorHex(StatusEffectType type)
    {
        switch (type)
        {
            case StatusEffectType.Bleed: return "E0584D";
            case StatusEffectType.Stun: return "E8C24D";
            case StatusEffectType.DefenseDown: return "C98F4D";
            case StatusEffectType.SpeedDown: return "6FA0E0";
            default: return "C8C2B4";
        }
    }

    void UpdateMoveButtonStates()
    {
        var moves = GM.Player.EquippedMoves;
        int stamina = GM.Player.Stats.CurrentStamina;
        for (int i = 0; i < moveButtons.Length; i++)
        {
            if (i >= moves.Count) continue;
            moveButtons[i].interactable = !animatingTurn && moves[i].StaminaCost <= stamina;
        }
    }

    void OnMoveSelected(int index)
    {
        if (animatingTurn) return;
        if (GM.CurrentBattle == null || GM.Player == null)
        {
            Debug.LogWarning("OnMoveSelected: no active battle.");
            return;
        }

        var moves = GM.Player.EquippedMoves;
        if (index >= moves.Count) return;

        var move = moves[index];
        if (move.StaminaCost > GM.Player.Stats.CurrentStamina) return;

        animatingTurn = true;
        itemButton.interactable = false;
        recoverButton.interactable = false;
        parryButton.interactable = false;
        clinchButton.interactable = false;
        UpdateMoveButtonStates();

        int playerHpBefore = GM.Player.Stats.CurrentHealth;
        int opponentHpBefore = GM.CurrentOpponent.Stats.CurrentHealth;

        var turnLog = new List<string>();
        var result = GM.CurrentBattle.PlayerUseMove(move, turnLog);
        turnsThisFight++;
        CaptureComboFromLog(turnLog);

        int dealt = Mathf.Max(0, opponentHpBefore - GM.CurrentOpponent.Stats.CurrentHealth);
        int taken = Mathf.Max(0, playerHpBefore - GM.Player.Stats.CurrentHealth);
        GM.RecordCombatStats(dealt, taken);

        MaybeInjectRivalTaunt(turnLog);
        MaybeInjectCommentary(turnLog, dealt, taken);
        AppendLog(turnLog);
        UpdateBars();
        UpdateComboChainDisplay();
        RunAnimation(PlayTurnFeedback(turnLog, result, move));
    }

    void OnRecoverSelected()
    {
        if (animatingTurn) return;
        if (GM.CurrentBattle == null || GM.Player == null) return;
        if (GM.Player.Stats.CurrentStamina >= GM.Player.Stats.MaxStamina) return;

        animatingTurn = true;
        itemButton.interactable = false;
        recoverButton.interactable = false;
        parryButton.interactable = false;
        clinchButton.interactable = false;
        UpdateMoveButtonStates();

        int playerHpBefore = GM.Player.Stats.CurrentHealth;
        int opponentHpBefore = GM.CurrentOpponent.Stats.CurrentHealth;

        var turnLog = new List<string>();
        var result = GM.CurrentBattle.PlayerRecover(turnLog);
        turnsThisFight++;
        CaptureComboFromLog(turnLog);

        int dealt = Mathf.Max(0, opponentHpBefore - GM.CurrentOpponent.Stats.CurrentHealth);
        int taken = Mathf.Max(0, playerHpBefore - GM.Player.Stats.CurrentHealth);
        GM.RecordCombatStats(dealt, taken);

        MaybeInjectRivalTaunt(turnLog);
        MaybeInjectCommentary(turnLog, dealt, taken);
        AppendLog(turnLog);
        UpdateBars();
        UpdateComboChainDisplay();
        RunAnimation(PlayTurnFeedback(turnLog, result, null));
    }

    // Milestone 40, Part 1: PARRY and CLINCH - mirror OnRecoverSelected's
    // shape exactly (no move, no stamina-cost gate since both are always
    // available per the brief), just calling the matching BattleSystem method.
    void OnParrySelected()
    {
        if (animatingTurn) return;
        if (GM.CurrentBattle == null || GM.Player == null) return;

        animatingTurn = true;
        itemButton.interactable = false;
        recoverButton.interactable = false;
        parryButton.interactable = false;
        clinchButton.interactable = false;
        UpdateMoveButtonStates();

        int playerHpBefore = GM.Player.Stats.CurrentHealth;
        int opponentHpBefore = GM.CurrentOpponent.Stats.CurrentHealth;

        var turnLog = new List<string>();
        var result = GM.CurrentBattle.PlayerParry(turnLog);
        turnsThisFight++;
        CaptureComboFromLog(turnLog);

        int dealt = Mathf.Max(0, opponentHpBefore - GM.CurrentOpponent.Stats.CurrentHealth);
        int taken = Mathf.Max(0, playerHpBefore - GM.Player.Stats.CurrentHealth);
        GM.RecordCombatStats(dealt, taken);

        MaybeInjectRivalTaunt(turnLog);
        MaybeInjectCommentary(turnLog, dealt, taken);
        AppendLog(turnLog);
        UpdateBars();
        UpdateComboChainDisplay();
        RunAnimation(PlayTurnFeedback(turnLog, result, null));
    }

    void OnClinchSelected()
    {
        if (animatingTurn) return;
        if (GM.CurrentBattle == null || GM.Player == null) return;

        animatingTurn = true;
        itemButton.interactable = false;
        recoverButton.interactable = false;
        parryButton.interactable = false;
        clinchButton.interactable = false;
        UpdateMoveButtonStates();

        int playerHpBefore = GM.Player.Stats.CurrentHealth;
        int opponentHpBefore = GM.CurrentOpponent.Stats.CurrentHealth;

        var turnLog = new List<string>();
        var result = GM.CurrentBattle.PlayerClinch(turnLog);
        turnsThisFight++;
        CaptureComboFromLog(turnLog);

        int dealt = Mathf.Max(0, opponentHpBefore - GM.CurrentOpponent.Stats.CurrentHealth);
        int taken = Mathf.Max(0, playerHpBefore - GM.Player.Stats.CurrentHealth);
        GM.RecordCombatStats(dealt, taken);

        MaybeInjectRivalTaunt(turnLog);
        MaybeInjectCommentary(turnLog, dealt, taken);
        AppendLog(turnLog);
        UpdateBars();
        UpdateComboChainDisplay();
        RunAnimation(PlayTurnFeedback(turnLog, result, null));
    }

    void UpdateDefenseButtonStates()
    {
        parryButton.interactable = !animatingTurn;
        clinchButton.interactable = !animatingTurn;
    }

    void UpdateRecoverButtonState()
    {
        if (GM.Player == null) return;
        recoverButton.interactable = !animatingTurn && GM.Player.Stats.CurrentStamina < GM.Player.Stats.MaxStamina;
    }

    void ToggleItemPanel()
    {
        if (animatingTurn) return;
        if (GM.Player == null) return;

        showingItems = !showingItems;

        var moves = GM.Player.EquippedMoves;
        for (int i = 0; i < moveButtons.Length; i++)
            moveButtons[i].gameObject.SetActive(!showingItems && i < moves.Count);

        itemContainer.gameObject.SetActive(showingItems);
        if (showingItems) RefreshItemPanel();
    }

    void RefreshItemPanel()
    {
        foreach (var entry in itemEntries) Object.Destroy(entry);
        itemEntries.Clear();

        var entries = GM.GetInventoryEntries();
        if (entries.Count == 0)
        {
            var emptyLabel = UIFactory.CreateCaption(itemContainer, "No items owned.", Vector2.zero, Vector2.one, TextAnchor.MiddleCenter);
            itemEntries.Add(emptyLabel.gameObject);
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            float slotHeight = 1f / entries.Count;
            float padding = slotHeight * 0.1f;
            float yMax = 1f - i * slotHeight - padding;
            float yMin = 1f - (i + 1) * slotHeight + padding;

            // Milestone 50, Part 5/6: was PositiveColor (green) - using an
            // item is an action, not a value comparison/reward.
            var button = UIFactory.CreateButton(itemContainer, $"{entry.Item.Name} x{entry.Quantity}",
                new Vector2(0.05f, yMin), new Vector2(0.95f, yMax), () => OnItemSelected(entry.Item.Id), UIFactory.AccentOrange);
            itemEntries.Add(button.gameObject);
        }
    }

    void OnItemSelected(string itemId)
    {
        string logLine = GM.UseItem(itemId);
        if (logLine == null) return;

        var lines = new List<string> { logLine };
        ProcessSingleLine(logLine);
        AppendLog(lines);
        UpdateBars();
        UpdateMoveButtonStates();
        RefreshItemPanel();
    }

    // ---------- Combat feedback (parsed from the battle log; BattleSystem itself is untouched) ----------

    IEnumerator PlayTurnFeedback(List<string> lines, BattleResult result, MoveData selectedMove)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            if (TryGetAttack(line, selectedMove, out bool attackerIsPlayer, out MoveData move, out bool critical))
            {
                var attackerFx = attackerIsPlayer ? playerFx : opponentFx;
                var attackerVisual = attackerIsPlayer ? playerCombatant : opponentCombatant;
                attackerFx.PlayAttack(move != null ? move.Type : MoveType.Boxing, upward: attackerIsPlayer);
                attackerVisual.PlayAttack(move != null ? move.Type : MoveType.Boxing);
                yield return new WaitForSecondsRealtime(move != null && IsGrapplingMove(move.Type) ? 0.12f : 0.075f);

                if (critical) PlayTimeImpact(0.28f, 0.12f);
                ProcessSingleLine(line, move);
                yield return new WaitForSecondsRealtime(critical ? 0.3f : 0.16f);
            }
            else
            {
                ProcessSingleLine(line, null);
                if (IsStatusFeedbackLine(line)) yield return new WaitForSecondsRealtime(0.08f);
            }
        }

        if (result != BattleResult.Ongoing)
        {
            bool playerWon = result == BattleResult.PlayerWon;
            bool championshipMainEvent = IsChampionshipFight() && IsLeaderFight();
            if (playerWon)
            {
                playerCombatant.PlayVictoryPose(champion: championshipMainEvent, leader: false);
                opponentCombatant.PlayDefeatPose();
            }
            else
            {
                opponentCombatant.PlayVictoryPose(champion: championshipMainEvent, leader: IsLeaderFight());
                playerCombatant.PlayDefeatPose();
            }
            yield return new WaitForSecondsRealtime(IsChampionshipFight() ? 0.9f : 0.68f);

            // selectedMove is null on a Recover turn - guard before checking
            // its Type (the player can still technically "win" on a Recover
            // turn if a bleed tick finishes the opponent during their turn).
            bool submissionFinish = result == BattleResult.PlayerWon && selectedMove != null && selectedMove.Type == MoveType.BrazilianJiuJitsu;

            // Milestone 32, Part 7/8: hand off this fight's presentation-only
            // stats so Victory/Defeat can show them.
            GM.LastFightTurnCount = turnsThisFight;
            GM.LastComboUsed = lastComboNameThisFight;

            GM.EndBattle(result, submissionFinish);
            yield break;
        }

        yield return new WaitForSecondsRealtime(0.08f);

        animatingTurn = false;
        itemButton.interactable = true;
        UpdateMoveButtonStates();
        UpdateRecoverButtonState();
        UpdateDefenseButtonStates();
    }

    bool TryGetAttack(string line, MoveData selectedMove, out bool attackerIsPlayer, out MoveData move, out bool critical)
    {
        attackerIsPlayer = line.StartsWith(GM.Player.Name);
        bool attackerIsOpponent = !attackerIsPlayer && GM.CurrentOpponent != null && line.StartsWith(GM.CurrentOpponent.Name);
        // Milestone 40, Part 3: a fully-parried hit still names the attacker
        // and still throws the move - just without the hit-flash/damage a
        // connecting strike gets (the PARRY! line right before it already
        // covers the defender's own feedback).
        bool isResolution = line.Contains("but misses!") || line.Contains(" hits ") || line.Contains("CRITICAL") ||
            line.Contains("completely blocked!");
        critical = line.Contains("CRITICAL");
        move = null;

        if ((!attackerIsPlayer && !attackerIsOpponent) || !isResolution) return false;
        if (attackerIsPlayer && selectedMove != null && line.Contains(selectedMove.Name)) move = selectedMove;
        if (move == null)
        {
            for (int i = 0; i < MoveDatabase.All.Count; i++)
            {
                if (!line.Contains(MoveDatabase.All[i].Name)) continue;
                move = MoveDatabase.All[i];
                break;
            }
        }
        return true;
    }

    static bool IsGrapplingMove(MoveType type)
    {
        return type == MoveType.Wrestling || type == MoveType.Judo ||
            type == MoveType.BrazilianJiuJitsu || type == MoveType.GroundAndPound;
    }

    static bool IsStatusFeedbackLine(string line)
    {
        return line.Contains("is bleeding!") || line.Contains("is stunned") ||
            line.Contains("defense drops!") || line.Contains("speed drops!") ||
            line.Contains("lands a COMBO!") || line.Contains("catches their breath") ||
            line.Contains("PARRY!") || line.Contains("PARTIAL BLOCK!") ||
            line.Contains("CLINCH SUCCESS!") || line.Contains("COMBO INTERRUPTED!");
    }

    // Milestone 35, Part 3: move is optional - only the attack-resolution path
    // in PlayTurnFeedback has one resolved; everything else (status lines,
    // item-use lines) passes null and falls back to the generic hit sound.
    void ProcessSingleLine(string line, MoveData move = null)
    {
        bool namedIsPlayer = line.StartsWith(GM.Player.Name);
        bool namedIsOpponent = !namedIsPlayer && GM.CurrentOpponent != null && line.StartsWith(GM.CurrentOpponent.Name);
        if (!namedIsPlayer && !namedIsOpponent) return;

        // Milestone 31, Part 4/8: routed by the same name-prefix check as every
        // other line, so the "COMBO!" popup lands on whichever side actually
        // triggered it - the player, or (per Part 8) the AI landing one by chance.
        // Milestone 35, Part 4: combo_trigger.mp3 instead of the old crit-sound
        // placeholder - this line is only ever emitted once per activation
        // (BattleSystem.ResolveMove adds it exactly once inside its combo
        // branch), so the sound can't double-fire for the same combo.
        if (line.Contains("lands a COMBO!"))
        {
            // Milestone 41, Part 4: shows the actual combo name ("ONE-TWO
            // FINISH") instead of a generic "COMBO!" - the recipe itself
            // stays hidden (nothing here reveals the move sequence), but a
            // named, memorable payoff is the whole point of "I want to do
            // that again." Reuses the existing emphasized popup + a brief
            // time-impact beat, the same weight a critical hit already gets.
            AudioManager.Instance?.PlayComboTrigger();
            var fx = namedIsPlayer ? playerFx : opponentFx;
            string comboName = ExtractComboName(line);
            fx.SpawnPopup(string.IsNullOrEmpty(comboName) ? "COMBO!" : comboName.ToUpperInvariant(), CritColor, true);
            PlayTimeImpact(0.22f, 0.1f);
        }
        else if (line.Contains("CRITICAL") && line.Contains("damage!"))
        {
            AudioManager.Instance?.PlayCriticalHit();
            ShowHitFeedback(onOpponentSide: namedIsPlayer, ExtractNumber(line), crit: true);
        }
        else if (line.Contains(" hits ") && line.Contains("damage."))
        {
            // Milestone 35, Part 3: move-type-specific strike sound instead of
            // one generic hit clip - falls back to PlayHit if move is null
            // (status-damage lines, items) or its type isn't explicitly mapped.
            PlayStrikeSound(move);
            ShowHitFeedback(onOpponentSide: namedIsPlayer, ExtractNumber(line), crit: false);
        }
        else if (line.Contains("but misses!"))
        {
            ShowMissFeedback(onOpponentSide: namedIsPlayer);
        }
        else if (line.Contains("bleed damage."))
        {
            ShowHitFeedback(onOpponentSide: namedIsOpponent, ExtractNumber(line), crit: false, isStatusDamage: true);
        }
        else if (line.Contains("is bleeding!")) ShowStatusFeedback(namedIsOpponent, StatusEffectType.Bleed);
        // Bug fix (World Polish Pass): the turn-skip message ("is stunned and
        // cannot act!") didn't match the old "is stunned!" check, so a stun
        // that actually skipped a turn showed no popup/audio at all - only
        // the stun's initial application did. Matching on the unadorned
        // substring catches both messages.
        else if (line.Contains("is stunned")) ShowStatusFeedback(namedIsOpponent, StatusEffectType.Stun);
        else if (line.Contains("defense drops!")) ShowStatusFeedback(namedIsOpponent, StatusEffectType.DefenseDown);
        else if (line.Contains("speed drops!")) ShowStatusFeedback(namedIsOpponent, StatusEffectType.SpeedDown);
        else if (line.Contains("heals") && line.Contains("health.")) ShowHealFeedback(ExtractNumber(line));
        else if (line.Contains("catches their breath") && line.Contains("recovers"))
            ShowStaminaFeedback(isPlayer: namedIsPlayer, ExtractNumber(line));
        // Milestone 40, Part 3: these four lines all name whichever fighter the
        // line is ABOUT (the defender for PARRY/PARTIAL BLOCK/CLINCH SUCCESS,
        // the attacker whose chain just broke for COMBO INTERRUPTED) - so the
        // feedback always lands on that fighter's own side.
        // Milestone 41, Part 7: labels tightened so each one answers "what
        // happened" on sight - PARRY now states the result (0 damage) instead
        // of just naming the action, PARTIAL BLOCK matches its own log
        // keyword exactly instead of the more ambiguous "BLOCKED", and COMBO
        // BROKEN reads as a direct consequence rather than a vague label.
        else if (line.Contains("PARRY!")) ShowDefenseFeedback(onOpponentSide: !namedIsPlayer, "PARRY! 0 DMG", UIFactory.PositiveColor);
        else if (line.Contains("PARTIAL BLOCK!")) ShowDefenseFeedback(onOpponentSide: !namedIsPlayer, "PARTIAL BLOCK", UIFactory.GoldColor);
        else if (line.Contains("CLINCH SUCCESS!")) ShowDefenseFeedback(onOpponentSide: !namedIsPlayer, "CLINCH!", UIFactory.SecondaryColor);
        else if (line.Contains("COMBO INTERRUPTED!")) ShowDefenseFeedback(onOpponentSide: !namedIsPlayer, "COMBO BROKEN!", CritColor);
    }

    // Milestone 40, Part 3: a small, generic popup for the new defensive
    // feedback lines - reuses FighterCardFX.SpawnPopup the same way every
    // other feedback method already does, no new FX primitives.
    void ShowDefenseFeedback(bool onOpponentSide, string label, Color color)
    {
        var fx = onOpponentSide ? opponentFx : playerFx;
        fx.SpawnPopup(label, color, false);
    }

    void ShowStaminaFeedback(bool isPlayer, int amount)
    {
        var fx = isPlayer ? playerFx : opponentFx;
        fx.SpawnPopup($"+{amount} STM", StaminaColor, false);
        fx.Flash(new Color(StaminaColor.r, StaminaColor.g, StaminaColor.b, 0.5f));
    }

    // Milestone 35, Part 3: maps each existing move to the audio brief's
    // strike categories by id - explicit rather than inferred from MoveType,
    // since the brief calls out specific move names (e.g. Hook is "heavy"
    // despite being MoveType.Boxing same as the "light" Jab/Cross). Any move
    // not listed (including future ones) safely falls back to the generic hit.
    static void PlayStrikeSound(MoveData move)
    {
        if (move == null) { AudioManager.Instance?.PlayHit(); return; }

        switch (move.Id)
        {
            case "jab":
            case "cross":
            case "leg_kick":
            case "push_kick":
            case "body_shot":
                AudioManager.Instance?.PlayLightStrike();
                break;
            case "hook":
            case "elbow_strike":
            case "elbow_barrage":
            case "knee_strike":
            case "ground_smash":
            case "spinning_back_kick":
                AudioManager.Instance?.PlayHeavyStrike();
                break;
            case "double_leg_takedown":
            case "body_lock":
            case "suplex":
                AudioManager.Instance?.PlayTakedown();
                break;
            case "kimura":
            case "armbar":
            case "triangle_choke":
            case "rear_naked_choke":
                AudioManager.Instance?.PlaySubmissionMove();
                break;
            default:
                AudioManager.Instance?.PlayHit();
                break;
        }
    }

    void ShowHitFeedback(bool onOpponentSide, int damage, bool crit, bool isStatusDamage = false)
    {
        var fx = onOpponentSide ? opponentFx : playerFx;
        Color popupColor = crit ? CritColor : (isStatusDamage ? UIFactory.GoldColor : Color.white);
        string text = crit ? $"-{damage}!" : $"-{damage}";
        fx.SpawnPopup(text, popupColor, crit);
        fx.Flash(crit ? CritColor : HitFlashColor);
        float weight = Mathf.Clamp(damage / 10f, 0.75f, 3f);
        fx.Recoil(weight, upward: onOpponentSide, critical: crit);
        (onOpponentSide ? opponentCombatant : playerCombatant).Recoil(weight, crit);
    }

    void ShowMissFeedback(bool onOpponentSide)
    {
        var fx = onOpponentSide ? opponentFx : playerFx;
        fx.SpawnPopup("MISS", MissColor, false);
        fx.Dodge(right: onOpponentSide);
        (onOpponentSide ? opponentCombatant : playerCombatant).Dodge();
    }

    void ShowStatusFeedback(bool onOpponentSide, StatusEffectType type)
    {
        var fx = onOpponentSide ? opponentFx : playerFx;
        string label;
        Color color;
        switch (type)
        {
            case StatusEffectType.Bleed:
                label = "BLEED";
                color = new Color(0.9f, 0.2f, 0.18f, 1f);
                break;
            case StatusEffectType.Stun:
                label = "STUN * * *";
                color = UIFactory.GoldColor;
                break;
            case StatusEffectType.DefenseDown:
                label = "SHIELD CRACK";
                color = new Color(0.8f, 0.55f, 0.3f, 1f);
                break;
            default:
                label = "SLOWED";
                color = new Color(0.45f, 0.65f, 0.95f, 1f);
                break;
        }
        fx.SpawnPopup(label, color, false);
        fx.PlayStatus(type);
        (onOpponentSide ? opponentCombatant : playerCombatant).PlayStatus(type);
    }

    void ShowHealFeedback(int amount)
    {
        // Milestone 50, Part 5: a healed amount is exactly the "positive
        // value" case the unified Green is for - same brightened-PositiveColor
        // derivation as the Tale of the Tape's better/worse comparison.
        playerFx.SpawnPopup($"+{amount}", new Color(
            Mathf.Min(1f, UIFactory.PositiveColor.r * 1.3f),
            Mathf.Min(1f, UIFactory.PositiveColor.g * 1.3f),
            Mathf.Min(1f, UIFactory.PositiveColor.b * 1.3f)), false);
        playerFx.Flash(HealFlashColor);
    }

    static int ExtractNumber(string line)
    {
        int start = -1;
        for (int i = 0; i < line.Length; i++)
        {
            if (char.IsDigit(line[i])) { start = i; break; }
        }
        if (start < 0) return 0;

        int end = start;
        while (end < line.Length && char.IsDigit(line[end])) end++;
        return int.Parse(line.Substring(start, end - start));
    }

    // Milestone 41, Part 4: pulls the combo's own display name out of
    // BattleSystem's "{Attacker} lands a COMBO! {DisplayName}!" line - the
    // name was already in the log text, just never shown in the popup itself.
    static string ExtractComboName(string line)
    {
        const string marker = "lands a COMBO! ";
        int idx = line.IndexOf(marker);
        if (idx < 0) return null;
        string rest = line.Substring(idx + marker.Length);
        return rest.EndsWith("!") ? rest.Substring(0, rest.Length - 1) : rest;
    }

    // Appends only the new lines instead of rejoining the whole history every turn,
    // so a long battle's log doesn't get more expensive to update as it grows.
    // Milestone 64, Part 2: cap raised from 4 to 60 now that the log panel is
    // a real ScrollRect - long fights keep real scrollback instead of losing
    // everything past the last 4 lines, and ScrollLogToBottom keeps the
    // newest line in view exactly like the old fixed-4 behavior did.
    void AppendLog(List<string> lines)
    {
        if (lines.Count == 0) return;

        var formatted = new List<string>(lines.Count);
        foreach (var line in lines) formatted.Add(FormatLogLine(line));
        log.AddRange(formatted);

        while (log.Count > 60) log.RemoveAt(0);
        logText.text = string.Join("\n", log);
        ScrollLogToBottom();
    }

    // Milestone 64, Part 2: forces an immediate layout pass so the
    // ContentSizeFitter-driven content height is current before reading/
    // setting scroll position - without this, verticalNormalizedPosition
    // would be computed against last frame's (shorter) content height.
    void ScrollLogToBottom()
    {
        Canvas.ForceUpdateCanvases();
        logScroll.verticalNormalizedPosition = 0f;
    }

    // Colors key moments (crit, miss, exhausted, status, regen) so the log reads at a glance.
    static string FormatLogLine(string line)
    {
        if (line.Contains("COMBO ACTIVATED") || line.Contains("lands a COMBO!")) return $"<b><color=#FFD24D>{line}</color></b>";
        if (line.Contains("CRITICAL")) return $"<b><color=#FFD24D>{line}</color></b>";
        if (line.Contains("misses")) return $"<i><color=#9A9A9A>{line}</color></i>";
        if (line.Contains("too exhausted")) return $"<color=#E06A60>{line}</color>";
        if (line.Contains("stunned") || line.Contains("bleed") || line.Contains("drops") || line.Contains("Bleeding") || line.Contains("is bleeding"))
            return $"<color=#E0B33D>{line}</color>";
        if (line.Contains("recovers")) return $"<color=#7FBF7F>{line}</color>";
        if (line.Contains("acts first")) return $"<color=#C8C2B4><i>{line}</i></color>";
        return line;
    }
}
