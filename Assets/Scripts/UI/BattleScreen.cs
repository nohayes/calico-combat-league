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
    readonly Text[] moveLabels;
    readonly Button itemButton;
    readonly Button recoverButton;
    readonly Transform itemContainer;
    readonly RectTransform introCard;
    readonly CanvasGroup introGroup;
    readonly Text introText;
    readonly Text introBillingText;
    readonly Text introAnnouncementText;
    readonly RectTransform introMatchupGroup;
    readonly Image introPlayerPortrait;
    readonly Image introOpponentPortrait;
    readonly Text introPlayerName;
    readonly Text introOpponentName;
    readonly RectTransform introTapeGroup;
    readonly Text[] tapePlayerValues = new Text[5];
    readonly Text[] tapeOpponentValues = new Text[5];
    readonly Text introTapPrompt;
    bool introSkipRequested;
    bool waitingForDialogueTap;
    bool dialogueAdvanceRequested;
    readonly List<GameObject> itemEntries = new List<GameObject>();
    readonly List<string> log = new List<string>();
    bool showingItems;
    bool animatingTurn;

    static readonly string[] TapeOfTheTapeLabels = { "LEVEL", "HEALTH", "STRENGTH", "DEFENSE", "SPEED" };

    static readonly Color HealthColor = new Color(0.62f, 0.13f, 0.12f, 1f);
    static readonly Color StaminaColor = new Color(0.18f, 0.5f, 0.62f, 1f);
    static readonly Color CritColor = new Color(1f, 0.65f, 0.1f, 1f);
    static readonly Color HitFlashColor = new Color(0.8f, 0.15f, 0.12f, 1f);
    static readonly Color HealFlashColor = new Color(0.3f, 0.7f, 0.3f, 0.6f);
    static readonly Color MissColor = new Color(0.75f, 0.75f, 0.75f, 1f);

    public BattleScreen(Transform parent, GameManager gm) : base(parent, gm, "BattleScreen", "battle")
    {
        fightBilling = UIFactory.CreateText(Root.transform, "", UIFactory.CaptionSize, UIFactory.GoldColor,
            TextAnchor.MiddleCenter, new Vector2(0.04f, 0.966f), new Vector2(0.96f, 0.997f), FontStyle.Bold);

        // Landscape Conversion (Milestone 26): Player | VS | Opponent across the
        // full width, with each fighter's name/portrait/bars sitting directly
        // beneath their own side and the battle log filling the center column -
        // the layout the brief asks for, using 16:9 width instead of stacking.
        var opponentCard = UIFactory.CreateFighterCard(Root.transform, "Opponent",
            new Vector2(0.66f, 0.155f), new Vector2(0.98f, 0.295f), out opponentPortrait, out var opponentInfo);
        opponentFx = AttachFx(opponentCard);

        opponentName = UIFactory.CreateText(opponentInfo, "", UIFactory.SubheadingSize, UIFactory.CreamColor, TextAnchor.MiddleLeft,
            new Vector2(0f, 0.62f), new Vector2(1f, 1f), FontStyle.Bold);
        BuildStatRow(opponentInfo, new Vector2(0f, 0.34f), new Vector2(1f, 0.6f), "HP", HealthColor, out opponentHealth, out opponentHealthValue);
        BuildStatRow(opponentInfo, new Vector2(0f, 0.06f), new Vector2(1f, 0.32f), "STM", StaminaColor, out opponentStamina, out opponentStaminaValue);

        opponentEffectsText = UIFactory.CreateCaption(Root.transform, "", new Vector2(0.66f, 0.135f), new Vector2(0.98f, 0.153f));
        opponentEffectsText.color = UIFactory.GoldColor;

        var playerCard = UIFactory.CreateFighterCard(Root.transform, "Player",
            new Vector2(0.02f, 0.155f), new Vector2(0.34f, 0.295f), out playerPortrait, out var playerInfo);
        playerFx = AttachFx(playerCard);

        playerName = UIFactory.CreateText(playerInfo, "", UIFactory.SubheadingSize, UIFactory.CreamColor, TextAnchor.MiddleLeft,
            new Vector2(0f, 0.62f), new Vector2(1f, 1f), FontStyle.Bold);
        BuildStatRow(playerInfo, new Vector2(0f, 0.34f), new Vector2(1f, 0.6f), "HP", new Color(0.2f, 0.55f, 0.2f), out playerHealth, out playerHealthValue);
        BuildStatRow(playerInfo, new Vector2(0f, 0.06f), new Vector2(1f, 0.32f), "STM", StaminaColor, out playerStamina, out playerStaminaValue);

        playerEffectsText = UIFactory.CreateCaption(Root.transform, "", new Vector2(0.02f, 0.135f), new Vector2(0.34f, 0.153f));
        playerEffectsText.color = UIFactory.GoldColor;

        stageCard = UIFactory.CreateCard(Root.transform, "FightStage", new Vector2(0.02f, 0.32f), new Vector2(0.98f, 0.94f),
            new Color(0.08f, 0.07f, 0.07f, 0.88f));
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

        playerCombatantRoot = UIFactory.CreateBattleFighter(stageCard, "Player",
            new Vector2(0.04f, 0.03f), new Vector2(0.48f, 0.98f), out playerBattleSprite);
        opponentCombatantRoot = UIFactory.CreateBattleFighter(stageCard, "Opponent",
            new Vector2(0.52f, 0.03f), new Vector2(0.96f, 0.98f), out opponentBattleSprite);
        playerCombatant = playerCombatantRoot.gameObject.AddComponent<BattleFighterVisual>();
        opponentCombatant = opponentCombatantRoot.gameObject.AddComponent<BattleFighterVisual>();

        UIFactory.CreateText(stageCard, "VS", UIFactory.SubheadingSize, UIFactory.GoldColor, TextAnchor.MiddleCenter,
            new Vector2(0.45f, 0.5f), new Vector2(0.55f, 0.72f), FontStyle.Bold);

        // Battle log now lives in the center column, at the same height band as
        // each fighter's name/bars on either side of it.
        UIFactory.CreateCard(Root.transform, "LogBackdrop", new Vector2(0.36f, 0.155f), new Vector2(0.64f, 0.295f),
            new Color(0.06f, 0.05f, 0.05f, 0.88f));
        // Milestone 28: bumped from CaptionSize - on a typical 16:9 laptop display
        // the canvas scale factor lands well under 1.0, so the log was reading
        // smaller than intended for the most important screen in the game.
        logText = UIFactory.CreateText(Root.transform, "", UIFactory.BodySize, UIFactory.CreamColor, TextAnchor.UpperLeft,
            new Vector2(0.37f, 0.16f), new Vector2(0.63f, 0.285f));

        itemButton = UIFactory.CreateButton(Root.transform, "ITEMS", new Vector2(0.30f, 0.115f), new Vector2(0.46f, 0.148f),
            () => ToggleItemPanel(), UIFactory.SecondaryColor);

        // Milestone 30, Part 5: Recover spends the player's turn for a much
        // bigger stamina gain than passive regen alone - the counterweight to
        // the lowered regen rate below.
        recoverButton = UIFactory.CreateButton(Root.transform, "RECOVER", new Vector2(0.54f, 0.115f), new Vector2(0.70f, 0.148f),
            () => OnRecoverSelected(), UIFactory.PositiveColor);

        itemContainer = UIFactory.CreateContainer(Root.transform, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.105f));
        itemContainer.gameObject.SetActive(false);

        // Move buttons in a single row across the bottom (4 across) instead of a
        // 2x2 grid - 16:9 width comfortably fits all four side by side.
        moveButtons = new Button[4];
        moveLabels = new Text[4];
        for (int i = 0; i < 4; i++)
        {
            int index = i;
            float xMin = 0.02f + i * 0.245f;
            float xMax = xMin + 0.225f;

            var btn = UIFactory.CreateButton(Root.transform, "Move", new Vector2(xMin, 0.02f), new Vector2(xMax, 0.105f),
                () => OnMoveSelected(index));
            moveButtons[i] = btn;
            moveLabels[i] = btn.GetComponentInChildren<Text>();
        }

        introCard = UIFactory.CreateCard(Root.transform, "FightIntroduction",
            new Vector2(0.06f, 0.155f), new Vector2(0.94f, 0.94f), new Color(0.08f, 0.07f, 0.07f, 0.98f));
        introGroup = introCard.gameObject.AddComponent<CanvasGroup>();

        // Tap-to-skip: the card itself is a Button, but every child Graphic inside it
        // needs raycastTarget off or it would intercept the click first.
        var skipButton = introCard.gameObject.AddComponent<Button>();
        skipButton.transition = Selectable.Transition.None;
        skipButton.targetGraphic = introCard.GetComponent<Image>();
        skipButton.onClick.AddListener(SkipIntro);

        introBillingText = UIFactory.CreateText(introCard, "", UIFactory.SubheadingSize, UIFactory.GoldColor,
            TextAnchor.MiddleCenter, new Vector2(0.04f, 0.86f), new Vector2(0.96f, 0.97f), FontStyle.Bold);
        introBillingText.raycastTarget = false;
        introAnnouncementText = UIFactory.CreateText(introCard, "", UIFactory.CaptionSize, UIFactory.MutedTextColor,
            TextAnchor.MiddleCenter, new Vector2(0.04f, 0.79f), new Vector2(0.96f, 0.86f));
        introAnnouncementText.raycastTarget = false;

        introMatchupGroup = UIFactory.CreateContainer(introCard, new Vector2(0f, 0.3f), new Vector2(1f, 0.78f));

        var playerPortraitGo = new GameObject("IntroPlayerPortrait", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        playerPortraitGo.transform.SetParent(introMatchupGroup, false);
        var playerPortraitRt = playerPortraitGo.GetComponent<RectTransform>();
        playerPortraitRt.anchorMin = new Vector2(0.06f, 0.4f);
        playerPortraitRt.anchorMax = new Vector2(0.3f, 0.78f);
        playerPortraitRt.offsetMin = Vector2.zero;
        playerPortraitRt.offsetMax = Vector2.zero;
        introPlayerPortrait = playerPortraitGo.GetComponent<Image>();
        introPlayerPortrait.preserveAspect = true;
        introPlayerPortrait.raycastTarget = false;

        var opponentPortraitGo = new GameObject("IntroOpponentPortrait", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        opponentPortraitGo.transform.SetParent(introMatchupGroup, false);
        var opponentPortraitRt = opponentPortraitGo.GetComponent<RectTransform>();
        opponentPortraitRt.anchorMin = new Vector2(0.7f, 0.4f);
        opponentPortraitRt.anchorMax = new Vector2(0.94f, 0.78f);
        opponentPortraitRt.offsetMin = Vector2.zero;
        opponentPortraitRt.offsetMax = Vector2.zero;
        introOpponentPortrait = opponentPortraitGo.GetComponent<Image>();
        introOpponentPortrait.preserveAspect = true;
        introOpponentPortrait.raycastTarget = false;

        introPlayerName = UIFactory.CreateText(introMatchupGroup, "", UIFactory.CaptionSize, UIFactory.CreamColor,
            TextAnchor.MiddleCenter, new Vector2(0.02f, 0.3f), new Vector2(0.34f, 0.4f), FontStyle.Bold);
        introPlayerName.raycastTarget = false;
        introOpponentName = UIFactory.CreateText(introMatchupGroup, "", UIFactory.CaptionSize, UIFactory.CreamColor,
            TextAnchor.MiddleCenter, new Vector2(0.66f, 0.3f), new Vector2(0.98f, 0.4f), FontStyle.Bold);
        introOpponentName.raycastTarget = false;

        var introVsText = UIFactory.CreateText(introMatchupGroup, "VS", UIFactory.SubheadingSize, UIFactory.GoldColor,
            TextAnchor.MiddleCenter, new Vector2(0.42f, 0.5f), new Vector2(0.58f, 0.68f), FontStyle.Bold);
        introVsText.raycastTarget = false;

        introTapeGroup = UIFactory.CreateContainer(introCard, new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.78f));
        for (int i = 0; i < TapeOfTheTapeLabels.Length; i++)
        {
            float slot = 1f / TapeOfTheTapeLabels.Length;
            float yMax = 1f - i * slot - slot * 0.07f;
            float yMin = yMax - slot * 0.86f;

            tapePlayerValues[i] = UIFactory.CreateText(introTapeGroup, "", UIFactory.BodySize, UIFactory.CreamColor,
                TextAnchor.MiddleLeft, new Vector2(0f, yMin), new Vector2(0.32f, yMax), FontStyle.Bold);
            tapePlayerValues[i].raycastTarget = false;

            var tapeLabel = UIFactory.CreateText(introTapeGroup, TapeOfTheTapeLabels[i], UIFactory.CaptionSize, UIFactory.GoldColor,
                TextAnchor.MiddleCenter, new Vector2(0.32f, yMin), new Vector2(0.68f, yMax), FontStyle.Bold);
            tapeLabel.raycastTarget = false;

            tapeOpponentValues[i] = UIFactory.CreateText(introTapeGroup, "", UIFactory.BodySize, UIFactory.CreamColor,
                TextAnchor.MiddleRight, new Vector2(0.68f, yMin), new Vector2(1f, yMax), FontStyle.Bold);
            tapeOpponentValues[i].raycastTarget = false;
        }
        introTapeGroup.gameObject.SetActive(false);

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

    static void BuildStatRow(Transform parent, Vector2 anchorMin, Vector2 anchorMax, string label, Color barColor,
        out Slider slider, out Text valueText)
    {
        var row = UIFactory.CreateContainer(parent, anchorMin, anchorMax);
        UIFactory.CreateCaption(row, label, new Vector2(0f, 0f), new Vector2(0.18f, 1f));
        slider = UIFactory.CreateSlider(row, new Vector2(0.2f, 0f), new Vector2(1f, 1f), barColor);
        if (label == "STM") slider.GetComponent<SmoothSlider>().SetLowEmphasis(true);
        valueText = UIFactory.CreateText(row, "", UIFactory.CaptionSize, UIFactory.CreamColor, TextAnchor.MiddleRight,
            new Vector2(0.2f, 0f), new Vector2(0.98f, 1f));
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
        showingItems = false;
        animatingTurn = true;
        itemContainer.gameObject.SetActive(false);
        playerFx.ClearPopups();
        opponentFx.ClearPopups();

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
        Color opponentTheme = GM.CurrentGym != null ? IconFactory.GetGymThemeColor(GM.CurrentGym.GymType) : UIFactory.AccentOrange;
        ArchetypeType opponentArchetype = GM.CurrentGym != null
            ? IconFactory.GetPortraitArchetype(GM.CurrentGym.GymType)
            : ArchetypeType.Unspecified;
        UIFactory.SetPlayerAvatar(playerPortrait, GM.Player.Archetype, playerTheme);
        UIFactory.SetFighterPortrait(opponentPortrait, GM.CurrentOpponentInfo?.OpponentId, opponentArchetype, opponentTheme);

        UIFactory.AddDisciplineBadge(playerPortrait.transform.parent, IconFactory.GetArchetypeIconShape(GM.Player.Archetype), playerTheme);
        if (GM.CurrentGym != null)
            UIFactory.AddDisciplineBadge(opponentPortrait.transform.parent, IconFactory.GetGymIconShape(GM.CurrentGym.GymType), opponentTheme);

        // Reuse the same sprites/colors already resolved above for the intro screen's
        // own portraits, instead of looking them up a second time.
        introPlayerPortrait.sprite = playerPortrait.sprite;
        introPlayerPortrait.color = playerPortrait.color;
        introOpponentPortrait.sprite = opponentPortrait.sprite;
        introOpponentPortrait.color = opponentPortrait.color;
        introPlayerName.text = GM.Player.Name;
        introOpponentName.text = GM.CurrentOpponent.Name;

        bool championshipFight = IsChampionshipFight();
        bool leaderFight = IsLeaderFight();
        stageCard.GetComponent<Image>().color = championshipFight
            ? new Color(0.16f, 0.12f, 0.04f, 0.94f)
            : new Color(0.08f, 0.07f, 0.07f, 0.88f);
        opponentAura.gameObject.SetActive(championshipFight || leaderFight);
        playerCombatantRoot.anchorMin = championshipFight ? new Vector2(0.01f, 0.01f) : new Vector2(0.04f, 0.03f);
        playerCombatantRoot.anchorMax = championshipFight ? new Vector2(0.49f, 1f) : new Vector2(0.48f, 0.98f);
        opponentCombatantRoot.anchorMin = championshipFight ? new Vector2(0.51f, 0.01f) : new Vector2(0.52f, 0.03f);
        opponentCombatantRoot.anchorMax = championshipFight ? new Vector2(0.99f, 1f) : new Vector2(0.96f, 0.98f);
        playerCombatant.Initialize(playerBattleSprite, "player", GM.Player.Archetype, playerTheme, faceRight: true);
        opponentCombatant.Initialize(opponentBattleSprite, GM.CurrentOpponentInfo?.OpponentId, opponentArchetype,
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
                moveLabels[i].text = $"{moves[i].Name}\n({moves[i].StaminaCost} stam)";
        }

        UpdateMoveButtonStates();
        itemButton.interactable = false;
        recoverButton.interactable = false;

        introCard.SetAsLastSibling();
        introCard.GetComponent<Image>().color = championshipFight
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

        bool championship = IsChampionshipFight();
        bool leaderFight = IsLeaderFight() && !championship;
        bool rematch = leaderFight && GM.HasBecomeChampion();

        introBillingText.text = GetFightBilling();
        introAnnouncementText.text = GetFightAnnouncement(championship, leaderFight, rematch);
        introText.text = "";
        introMatchupGroup.gameObject.SetActive(true);
        introTapeGroup.gameObject.SetActive(false);

        yield return FadeGroup(0f, 1f, 0.16f);
        yield return SlideIntroPortraitsIn();
        yield return WaitSkippable(championship ? 0.5f : leaderFight ? 0.35f : 0.22f);

        PopulateTapeOfTheTape();
        introMatchupGroup.gameObject.SetActive(false);
        introTapeGroup.gameObject.SetActive(true);
        yield return WaitSkippable(championship ? 0.85f : leaderFight ? 0.65f : 0.5f);
        introTapeGroup.gameObject.SetActive(false);

        // Personality beats (Milestone 22/23): a bio spotlight, then the opponent's
        // pre-fight trash talk - both reuse introText, both require an explicit tap
        // to advance (Part 5) instead of an auto-timed skip, with a portrait pop
        // (Part 7) and a larger font (Part 4) to make them feel noticeable.
        introText.fontSize = UIFactory.SubheadingSize + 4;
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

        introText.text = championship ? "CHAMPIONSHIP BOUT" : leaderFight ? "LEADER CHALLENGE" : "READY";
        introText.color = UIFactory.GoldColor;
        PlayPulse(introText.rectTransform, championship ? 1.18f : leaderFight ? 1.12f : 1.08f, 0.3f);
        yield return WaitSkippable(championship ? 0.55f : leaderFight ? 0.4f : 0.28f);

        introText.text = "FIGHT!";
        introText.color = UIFactory.CreamColor;
        PlayPulse(introText.rectTransform, 1.2f, 0.24f);
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

    // Tale of the Tape (Part 2): existing stat data only, no new stats.
    void PopulateTapeOfTheTape()
    {
        var p = GM.Player.Stats;
        var o = GM.CurrentOpponent.Stats;
        SetTapeRow(0, p.Level, o.Level);
        SetTapeRow(1, p.MaxHealth, o.MaxHealth);
        SetTapeRow(2, p.Strength, o.Strength);
        SetTapeRow(3, p.Defense, o.Defense);
        SetTapeRow(4, p.Speed, o.Speed);
    }

    void SetTapeRow(int index, int playerValue, int opponentValue)
    {
        tapePlayerValues[index].text = playerValue.ToString();
        tapeOpponentValues[index].text = opponentValue.ToString();
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

    string GetFightBilling()
    {
        var gym = GM.CurrentGym;
        var opponent = GM.CurrentOpponentInfo;
        bool isLeader = gym?.Leader != null && opponent != null && gym.Leader.OpponentId == opponent.OpponentId;

        if (gym != null && gym.GymType == GymType.Championship && isLeader)
            return "CHAMPIONSHIP BOUT";
        if (isLeader)
            return gym != null ? $"{gym.GymName.ToUpper()} MAIN EVENT" : "MAIN EVENT";
        return gym != null ? $"{gym.GymName.ToUpper()} SHOWDOWN" : "TONIGHT'S FEATURED MATCHUP";
    }

    static string GetFightAnnouncement(bool championship, bool leaderFight, bool rematch)
    {
        if (championship) return "Championship fight!";
        if (leaderFight) return rematch ? "Contender matchup!" : "Gym leader challenge!";
        return "Tonight's featured bout!";
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
        UpdateMoveButtonStates();

        int playerHpBefore = GM.Player.Stats.CurrentHealth;
        int opponentHpBefore = GM.CurrentOpponent.Stats.CurrentHealth;

        var turnLog = new List<string>();
        var result = GM.CurrentBattle.PlayerUseMove(move, turnLog);

        int dealt = Mathf.Max(0, opponentHpBefore - GM.CurrentOpponent.Stats.CurrentHealth);
        int taken = Mathf.Max(0, playerHpBefore - GM.Player.Stats.CurrentHealth);
        GM.RecordCombatStats(dealt, taken);

        AppendLog(turnLog);
        UpdateBars();
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
        UpdateMoveButtonStates();

        int playerHpBefore = GM.Player.Stats.CurrentHealth;
        int opponentHpBefore = GM.CurrentOpponent.Stats.CurrentHealth;

        var turnLog = new List<string>();
        var result = GM.CurrentBattle.PlayerRecover(turnLog);

        int dealt = Mathf.Max(0, opponentHpBefore - GM.CurrentOpponent.Stats.CurrentHealth);
        int taken = Mathf.Max(0, playerHpBefore - GM.Player.Stats.CurrentHealth);
        GM.RecordCombatStats(dealt, taken);

        AppendLog(turnLog);
        UpdateBars();
        RunAnimation(PlayTurnFeedback(turnLog, result, null));
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

            var button = UIFactory.CreateButton(itemContainer, $"{entry.Item.Name} x{entry.Quantity}",
                new Vector2(0.05f, yMin), new Vector2(0.95f, yMax), () => OnItemSelected(entry.Item.Id), UIFactory.PositiveColor);
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
                ProcessSingleLine(line);
                yield return new WaitForSecondsRealtime(critical ? 0.3f : 0.16f);
            }
            else
            {
                ProcessSingleLine(line);
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
            GM.EndBattle(result, submissionFinish);
            yield break;
        }

        yield return new WaitForSecondsRealtime(0.08f);

        animatingTurn = false;
        itemButton.interactable = true;
        UpdateMoveButtonStates();
        UpdateRecoverButtonState();
    }

    bool TryGetAttack(string line, MoveData selectedMove, out bool attackerIsPlayer, out MoveData move, out bool critical)
    {
        attackerIsPlayer = line.StartsWith(GM.Player.Name);
        bool attackerIsOpponent = !attackerIsPlayer && GM.CurrentOpponent != null && line.StartsWith(GM.CurrentOpponent.Name);
        bool isResolution = line.Contains("but misses!") || line.Contains(" hits ") || line.Contains("CRITICAL");
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
        return line.Contains("is bleeding!") || line.Contains("is stunned!") ||
            line.Contains("defense drops!") || line.Contains("speed drops!") ||
            line.Contains("COMBO DISCOVERED") || line.Contains("catches their breath");
    }

    void ProcessSingleLine(string line)
    {
        // Milestone 30, Part 7: the combo announcement doesn't start with a
        // fighter's name, so it's handled before the name-prefix gate below.
        // Reuses the existing crit popup/sound - no new FX system.
        if (line.StartsWith("COMBO DISCOVERED!"))
        {
            AudioManager.Instance?.PlayCriticalHit();
            playerFx.SpawnPopup("COMBO!", CritColor, true);
            return;
        }

        bool namedIsPlayer = line.StartsWith(GM.Player.Name);
        bool namedIsOpponent = !namedIsPlayer && GM.CurrentOpponent != null && line.StartsWith(GM.CurrentOpponent.Name);
        if (!namedIsPlayer && !namedIsOpponent) return;

        if (line.Contains("CRITICAL") && line.Contains("damage!"))
        {
            AudioManager.Instance?.PlayCriticalHit();
            ShowHitFeedback(onOpponentSide: namedIsPlayer, ExtractNumber(line), crit: true);
        }
        else if (line.Contains(" hits ") && line.Contains("damage."))
        {
            AudioManager.Instance?.PlayHit();
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
        else if (line.Contains("is stunned!")) ShowStatusFeedback(namedIsOpponent, StatusEffectType.Stun);
        else if (line.Contains("defense drops!")) ShowStatusFeedback(namedIsOpponent, StatusEffectType.DefenseDown);
        else if (line.Contains("speed drops!")) ShowStatusFeedback(namedIsOpponent, StatusEffectType.SpeedDown);
        else if (line.Contains("heals") && line.Contains("health.")) ShowHealFeedback(ExtractNumber(line));
        else if (line.Contains("catches their breath") && line.Contains("recovers"))
            ShowStaminaFeedback(isPlayer: namedIsPlayer, ExtractNumber(line));
    }

    void ShowStaminaFeedback(bool isPlayer, int amount)
    {
        var fx = isPlayer ? playerFx : opponentFx;
        fx.SpawnPopup($"+{amount} STM", StaminaColor, false);
        fx.Flash(new Color(StaminaColor.r, StaminaColor.g, StaminaColor.b, 0.5f));
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
        playerFx.SpawnPopup($"+{amount}", new Color(0.4f, 0.85f, 0.4f), false);
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

    // Appends only the new lines instead of rejoining the whole history every turn,
    // so a long battle's log doesn't get more expensive to update as it grows.
    void AppendLog(List<string> lines)
    {
        if (lines.Count == 0) return;

        var formatted = new List<string>(lines.Count);
        foreach (var line in lines) formatted.Add(FormatLogLine(line));
        log.AddRange(formatted);

        while (log.Count > 4) log.RemoveAt(0);
        logText.text = string.Join("\n", log);
    }

    // Colors key moments (crit, miss, exhausted, status, regen) so the log reads at a glance.
    static string FormatLogLine(string line)
    {
        if (line.Contains("COMBO DISCOVERED")) return $"<b><color=#FFD24D>{line}</color></b>";
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
