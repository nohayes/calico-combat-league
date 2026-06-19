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
    readonly Text playerHealthValue;
    readonly Text playerStaminaValue;
    readonly Text opponentHealthValue;
    readonly Text opponentStaminaValue;
    readonly Text playerEffectsText;
    readonly Text opponentEffectsText;
    readonly Text logText;
    readonly Slider playerHealth;
    readonly Slider playerStamina;
    readonly Slider opponentHealth;
    readonly Slider opponentStamina;
    readonly Button[] moveButtons;
    readonly Text[] moveLabels;
    readonly Transform itemContainer;
    readonly List<GameObject> itemEntries = new List<GameObject>();
    readonly List<string> log = new List<string>();
    bool showingItems;

    static readonly Color HealthColor = new Color(0.62f, 0.13f, 0.12f, 1f);
    static readonly Color StaminaColor = new Color(0.18f, 0.5f, 0.62f, 1f);
    static readonly Color CritColor = new Color(1f, 0.65f, 0.1f, 1f);
    static readonly Color HitFlashColor = new Color(0.8f, 0.15f, 0.12f, 1f);
    static readonly Color StatusFlashColor = new Color(0.7f, 0.55f, 0.15f, 0.7f);
    static readonly Color HealFlashColor = new Color(0.3f, 0.7f, 0.3f, 0.6f);
    static readonly Color MissColor = new Color(0.75f, 0.75f, 0.75f, 1f);

    public BattleScreen(Transform parent, GameManager gm) : base(parent, gm, "BattleScreen", "battle")
    {
        var opponentCard = UIFactory.CreateFighterCard(Root.transform, "Opponent",
            new Vector2(0.04f, 0.775f), new Vector2(0.96f, 0.965f), out opponentPortrait, out var opponentInfo);
        opponentFx = AttachFx(opponentCard);

        opponentName = UIFactory.CreateText(opponentInfo, "", UIFactory.SubheadingSize, UIFactory.CreamColor, TextAnchor.MiddleLeft,
            new Vector2(0f, 0.62f), new Vector2(1f, 1f), FontStyle.Bold);
        BuildStatRow(opponentInfo, new Vector2(0f, 0.34f), new Vector2(1f, 0.6f), "HP", HealthColor, out opponentHealth, out opponentHealthValue);
        BuildStatRow(opponentInfo, new Vector2(0f, 0.06f), new Vector2(1f, 0.32f), "STM", StaminaColor, out opponentStamina, out opponentStaminaValue);

        opponentEffectsText = UIFactory.CreateCaption(Root.transform, "", new Vector2(0.05f, 0.755f), new Vector2(0.95f, 0.773f));
        opponentEffectsText.color = UIFactory.GoldColor;

        var playerCard = UIFactory.CreateFighterCard(Root.transform, "Player",
            new Vector2(0.04f, 0.555f), new Vector2(0.96f, 0.745f), out playerPortrait, out var playerInfo);
        playerFx = AttachFx(playerCard);

        playerName = UIFactory.CreateText(playerInfo, "", UIFactory.SubheadingSize, UIFactory.CreamColor, TextAnchor.MiddleLeft,
            new Vector2(0f, 0.62f), new Vector2(1f, 1f), FontStyle.Bold);
        BuildStatRow(playerInfo, new Vector2(0f, 0.34f), new Vector2(1f, 0.6f), "HP", new Color(0.2f, 0.55f, 0.2f), out playerHealth, out playerHealthValue);
        BuildStatRow(playerInfo, new Vector2(0f, 0.06f), new Vector2(1f, 0.32f), "STM", StaminaColor, out playerStamina, out playerStaminaValue);

        playerEffectsText = UIFactory.CreateCaption(Root.transform, "", new Vector2(0.05f, 0.535f), new Vector2(0.95f, 0.553f));
        playerEffectsText.color = UIFactory.GoldColor;

        UIFactory.CreateCard(Root.transform, "LogBackdrop", new Vector2(0.05f, 0.30f), new Vector2(0.95f, 0.525f));
        logText = UIFactory.CreateText(Root.transform, "", UIFactory.CaptionSize, UIFactory.CreamColor, TextAnchor.UpperLeft,
            new Vector2(0.07f, 0.31f), new Vector2(0.93f, 0.515f));

        UIFactory.CreateButton(Root.transform, "ITEMS", new Vector2(0.3f, 0.255f), new Vector2(0.7f, 0.295f),
            () => ToggleItemPanel(), UIFactory.SecondaryColor);

        itemContainer = UIFactory.CreateContainer(Root.transform, new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.24f));
        itemContainer.gameObject.SetActive(false);

        moveButtons = new Button[4];
        moveLabels = new Text[4];
        for (int i = 0; i < 4; i++)
        {
            int index = i;
            float xMin = (i % 2 == 0) ? 0.05f : 0.52f;
            float xMax = (i % 2 == 0) ? 0.48f : 0.95f;
            float yMin = (i < 2) ? 0.13f : 0.02f;
            float yMax = (i < 2) ? 0.24f : 0.13f;

            var btn = UIFactory.CreateButton(Root.transform, "Move", new Vector2(xMin, yMin), new Vector2(xMax, yMax),
                () => OnMoveSelected(index));
            moveButtons[i] = btn;
            moveLabels[i] = btn.GetComponentInChildren<Text>();
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

        Color playerTheme = IconFactory.GetArchetypeThemeColor(GM.Player.Archetype);
        Color opponentTheme = GM.CurrentGym != null ? IconFactory.GetGymThemeColor(GM.CurrentGym.GymType) : UIFactory.AccentOrange;
        UIFactory.SetFighterPortrait(playerPortrait, "player", GM.Player.Archetype, playerTheme);
        UIFactory.SetFighterPortrait(opponentPortrait, GM.CurrentOpponentInfo?.OpponentId, ArchetypeType.Unspecified, opponentTheme);

        UIFactory.AddDisciplineBadge(playerPortrait.transform.parent, IconFactory.GetArchetypeIconShape(GM.Player.Archetype), playerTheme);
        if (GM.CurrentGym != null)
            UIFactory.AddDisciplineBadge(opponentPortrait.transform.parent, IconFactory.GetGymIconShape(GM.CurrentGym.GymType), opponentTheme);

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
            parts.Add($"{effect.Type} ({effect.RemainingTurns})");
        return string.Join("   ", parts);
    }

    void UpdateMoveButtonStates()
    {
        var moves = GM.Player.EquippedMoves;
        int stamina = GM.Player.Stats.CurrentStamina;
        for (int i = 0; i < moveButtons.Length; i++)
        {
            if (i >= moves.Count) continue;
            moveButtons[i].interactable = moves[i].StaminaCost <= stamina;
        }
    }

    void OnMoveSelected(int index)
    {
        if (GM.CurrentBattle == null || GM.Player == null)
        {
            Debug.LogWarning("OnMoveSelected: no active battle.");
            return;
        }

        var moves = GM.Player.EquippedMoves;
        if (index >= moves.Count) return;

        var move = moves[index];
        if (move.StaminaCost > GM.Player.Stats.CurrentStamina) return;

        int playerHpBefore = GM.Player.Stats.CurrentHealth;
        int opponentHpBefore = GM.CurrentOpponent.Stats.CurrentHealth;

        var turnLog = new List<string>();
        var result = GM.CurrentBattle.PlayerUseMove(move, turnLog);

        int dealt = Mathf.Max(0, opponentHpBefore - GM.CurrentOpponent.Stats.CurrentHealth);
        int taken = Mathf.Max(0, playerHpBefore - GM.Player.Stats.CurrentHealth);
        GM.RecordCombatStats(dealt, taken);

        ProcessCombatFeedback(turnLog);
        AppendLog(turnLog);
        UpdateBars();
        UpdateMoveButtonStates();

        if (result != BattleResult.Ongoing)
        {
            bool submissionFinish = result == BattleResult.PlayerWon && move.Type == MoveType.BrazilianJiuJitsu;
            GM.EndBattle(result, submissionFinish);
        }
    }

    void ToggleItemPanel()
    {
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
        ProcessCombatFeedback(lines);
        AppendLog(lines);
        UpdateBars();
        UpdateMoveButtonStates();
        RefreshItemPanel();
    }

    // ---------- Combat feedback (parsed from the battle log; BattleSystem itself is untouched) ----------

    void ProcessCombatFeedback(List<string> lines)
    {
        foreach (var line in lines) ProcessSingleLine(line);
    }

    void ProcessSingleLine(string line)
    {
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
        else if (line.Contains("is bleeding!")) ShowStatusFeedback(namedIsOpponent, "BLEED");
        else if (line.Contains("is stunned!")) ShowStatusFeedback(namedIsOpponent, "STUN");
        else if (line.Contains("defense drops!")) ShowStatusFeedback(namedIsOpponent, "DEF DOWN");
        else if (line.Contains("speed drops!")) ShowStatusFeedback(namedIsOpponent, "SPD DOWN");
        else if (line.Contains("heals") && line.Contains("health.")) ShowHealFeedback(ExtractNumber(line));
    }

    void ShowHitFeedback(bool onOpponentSide, int damage, bool crit, bool isStatusDamage = false)
    {
        var fx = onOpponentSide ? opponentFx : playerFx;
        Color popupColor = crit ? CritColor : (isStatusDamage ? UIFactory.GoldColor : Color.white);
        string text = crit ? $"-{damage}!" : $"-{damage}";
        fx.SpawnPopup(text, popupColor, crit);
        fx.Flash(crit ? CritColor : HitFlashColor);
        fx.Shake(crit ? 10f : 5f);
    }

    void ShowMissFeedback(bool onOpponentSide)
    {
        var fx = onOpponentSide ? opponentFx : playerFx;
        fx.SpawnPopup("MISS", MissColor, false);
    }

    void ShowStatusFeedback(bool onOpponentSide, string label)
    {
        var fx = onOpponentSide ? opponentFx : playerFx;
        fx.SpawnPopup(label, UIFactory.GoldColor, false);
        fx.Flash(StatusFlashColor);
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

        string newText = string.Join("\n", formatted);
        logText.text = logText.text.Length > 0 ? logText.text + "\n" + newText : newText;
    }

    // Colors key moments (crit, miss, exhausted, status, regen) so the log reads at a glance.
    static string FormatLogLine(string line)
    {
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
