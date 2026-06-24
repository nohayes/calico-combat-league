using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystem
{
    // Milestone 30, Part 4: lowered from 8. At 8/turn, every move costing 8 or
    // less stamina (most of the roster) was free to spam forever - regen
    // outpaced the cost. At 5/turn, only the cheapest jabs/pokes are fully
    // sustainable; anything mid-tier or heavier drains over a few turns,
    // which is what makes the new Recover action (below) actually matter.
    const int StaminaRegenPerTurn = 5;
    const int RecoverActionAmount = 18;
    // Overnight Audit: 60 -> 50. AI at low stamina was recovering more
    // often than not, making it read as predictably defensive in that state;
    // nudged down so it's more willing to keep pushing offense instead.
    const int AiRecoverChancePercent = 50;
    const int MaxComboTrackLength = 3;

    const int BleedDuration = 3;
    const int BleedDamagePerTurn = 4;
    const int StunDuration = 2;
    const int StunSkipChancePercent = 50;
    const int DefenseDownDuration = 3;
    const int DefenseDownAmount = 4;
    const int SpeedDownDuration = 3;
    const int SpeedDownAmount = 4;

    const int AiConserveStaminaChancePercent = 20;
    const float AiLowStaminaRatioThreshold = 0.3f;
    const float AiTopMovePoolFraction = 0.5f;

    // Milestone 40: Parry/Clinch tuning. Parry is the high-risk/high-reward
    // option (a real chance of taking zero damage and gaining stamina, but
    // also a real chance of getting nothing); Clinch is the reliable one
    // (always reduces damage and restores some stamina, never a full block).
    const int ParryBaseChancePercent = 15;
    const int ParryDefenseScalingPercent = 2;
    const int ParryMaxSuccessChancePercent = 60;
    const int ParryPartialChancePercent = 30;
    const int ParryFullBlockStaminaGain = 12;
    const int ParryPartialBlockStaminaGain = 6;
    const float ClinchDamageMultiplier = 0.6f;
    const int ClinchStaminaGain = 10;

    // Milestone 40, Part 4: AI defensive-stance tuning - layered on top of (not
    // replacing) the existing Recover roll, so low stamina gets two chances to
    // produce a non-attack turn instead of one.
    const int AiDefenseBaseChancePercent = 8;
    const int AiDefenseLowStaminaBonusPercent = 22;
    const int AiDefenseLosingBadlyBonusPercent = 18;
    const int AiDefenseComboPressureBonusPercent = 30;

    public readonly FighterData Player;
    public readonly FighterData Opponent;

    // Milestone 49 (Combat Record Book): per-fight PLAYER-only counters,
    // read once by GameManager.EndBattle right after CurrentBattle.Cleanup()
    // (which only reverts status effects and never touches these) and folded
    // into lifetime totals there - same handoff pattern as the existing
    // RecentPlayerMoveIds read-only exposure. Opponent actions are never
    // counted; these track the player's own accomplishments only.
    public int PlayerCriticalHits { get; private set; }
    public int PlayerCombosTriggered { get; private set; }
    public int PlayerParriesAttempted { get; private set; }
    public int PlayerParriesSucceeded { get; private set; }
    public int PlayerClinches { get; private set; }
    public int PlayerTakedownsLanded { get; private set; }

    readonly System.Random rng = new System.Random();
    readonly List<ActiveStatusEffect> playerEffects = new List<ActiveStatusEffect>();
    readonly List<ActiveStatusEffect> opponentEffects = new List<ActiveStatusEffect>();

    // Reused across AI turns to avoid allocating a fresh list every move choice.
    readonly List<MoveData> affordableMovesBuffer = new List<MoveData>();

    // Milestone 34, Part 5/6: reused by ChooseSmartEnemyMove's combo lookahead.
    readonly List<string> probeMoveIdsBuffer = new List<string>();

    // Milestone 31, Part 1: trailing window of each fighter's own recent move
    // ids, used to detect combos. The AI's move selection (ChooseEnemyMove)
    // stays completely untouched - it has no idea combos exist - but if it
    // happens to land on a real sequence by chance, Part 8 says it should
    // still get the bonus, so both fighters get the same lightweight tracker.
    readonly List<string> recentPlayerMoveIds = new List<string>();
    readonly List<string> recentOpponentMoveIds = new List<string>();

    // Milestone 40: which defensive stance (if any) each fighter declared for
    // the current round - set by the round-orchestrating Player*/RunOpponentTurn
    // methods, checked by ResolveMove when the OTHER fighter's attack resolves,
    // and cleared at the end of every round regardless of whether it was used.
    StanceType playerStance = StanceType.None;
    StanceType opponentStance = StanceType.None;

    // Milestone 31, Part 5: lets BattleScreen show the player's in-progress
    // chain ("Jab -> Jab") without exposing any other battle internals.
    public IReadOnlyList<string> RecentPlayerMoveIds => recentPlayerMoveIds;

    public BattleSystem(FighterData player, FighterData opponent)
    {
        Player = player;
        Opponent = opponent;
        Player.Stats.ResetForBattle();
        Opponent.Stats.ResetForBattle();
    }

    public IReadOnlyList<ActiveStatusEffect> GetEffects(FighterData fighter)
    {
        if (fighter == Player) return playerEffects;
        if (fighter == Opponent) return opponentEffects;
        return Array.Empty<ActiveStatusEffect>();
    }

    public BattleResult PlayerUseMove(MoveData move, List<string> log)
    {
        bool playerActsFirst = RollsFirst(Player.Stats.Speed, Opponent.Stats.Speed);
        log.Add(playerActsFirst ? $"{Player.Name} acts first this round." : $"{Opponent.Name} acts first this round.");

        BattleResult result;
        if (playerActsFirst)
        {
            result = RunPlayerTurn(move, log);
            if (result != BattleResult.Ongoing) { ClearStances(); return result; }
            result = RunOpponentTurn(log);
        }
        else
        {
            result = RunOpponentTurn(log);
            if (result != BattleResult.Ongoing) { ClearStances(); return result; }
            result = RunPlayerTurn(move, log);
        }
        ClearStances();
        return result;
    }

    // Milestone 40, Part 1: PARRY - the high-risk/high-reward defensive option.
    // Mirrors PlayerUseMove/PlayerRecover's turn-order structure exactly; the
    // stance is set before either half of the round resolves and cleared
    // after both have, so it protects against the opponent's attack this
    // round regardless of which side acts first.
    public BattleResult PlayerParry(List<string> log)
    {
        PlayerParriesAttempted++;
        bool playerActsFirst = RollsFirst(Player.Stats.Speed, Opponent.Stats.Speed);
        log.Add(playerActsFirst ? $"{Player.Name} acts first this round." : $"{Opponent.Name} acts first this round.");
        playerStance = StanceType.Parry;

        BattleResult result;
        if (playerActsFirst)
        {
            result = RunPlayerStanceTurn(StanceType.Parry, log);
            if (result != BattleResult.Ongoing) { ClearStances(); return result; }
            result = RunOpponentTurn(log);
        }
        else
        {
            result = RunOpponentTurn(log);
            if (result != BattleResult.Ongoing) { ClearStances(); return result; }
            result = RunPlayerStanceTurn(StanceType.Parry, log);
        }
        ClearStances();
        return result;
    }

    // Milestone 40, Part 1: CLINCH - the reliable defensive option. Same
    // structure as PlayerParry, different stance.
    public BattleResult PlayerClinch(List<string> log)
    {
        PlayerClinches++;
        bool playerActsFirst = RollsFirst(Player.Stats.Speed, Opponent.Stats.Speed);
        log.Add(playerActsFirst ? $"{Player.Name} acts first this round." : $"{Opponent.Name} acts first this round.");
        playerStance = StanceType.Clinch;

        BattleResult result;
        if (playerActsFirst)
        {
            result = RunPlayerStanceTurn(StanceType.Clinch, log);
            if (result != BattleResult.Ongoing) { ClearStances(); return result; }
            result = RunOpponentTurn(log);
        }
        else
        {
            result = RunOpponentTurn(log);
            if (result != BattleResult.Ongoing) { ClearStances(); return result; }
            result = RunPlayerStanceTurn(StanceType.Clinch, log);
        }
        ClearStances();
        return result;
    }

    void ClearStances()
    {
        playerStance = StanceType.None;
        opponentStance = StanceType.None;
    }

    // Milestone 30, Part 5: lets the player spend their turn recovering a much
    // larger chunk of stamina than passive regen alone provides. Mirrors
    // PlayerUseMove's turn-order structure exactly, just with no move chosen.
    public BattleResult PlayerRecover(List<string> log)
    {
        bool playerActsFirst = RollsFirst(Player.Stats.Speed, Opponent.Stats.Speed);
        log.Add(playerActsFirst ? $"{Player.Name} acts first this round." : $"{Opponent.Name} acts first this round.");

        BattleResult result;
        if (playerActsFirst)
        {
            result = RunPlayerRecoverTurn(log);
            if (result != BattleResult.Ongoing) { ClearStances(); return result; }
            result = RunOpponentTurn(log);
        }
        else
        {
            result = RunOpponentTurn(log);
            if (result != BattleResult.Ongoing) { ClearStances(); return result; }
            result = RunPlayerRecoverTurn(log);
        }
        ClearStances();
        return result;
    }

    BattleResult RunPlayerRecoverTurn(List<string> log)
    {
        if (TickEffects(Player, playerEffects, log)) return CheckResult();

        if (TryConsumeStun(Player, playerEffects, log))
        {
            RecoverStamina(Player, log);
            return CheckResult();
        }

        // Deliberately resting breaks an in-progress combo - same as not
        // following through on the sequence.
        recentPlayerMoveIds.Clear();
        PerformRecover(Player, log);
        return CheckResult();
    }

    // Milestone 40, Part 1: the player's half of a Parry/Clinch round -
    // declares the stance (already set by the caller) and otherwise behaves
    // like RunPlayerRecoverTurn (no attack, breaks their own combo-in-progress
    // the same way deliberately resting does). The actual damage-reduction/
    // stamina-gain/combo-break payoff happens later in ResolveMove, when (and
    // if) the opponent's attack lands against this stance this round.
    BattleResult RunPlayerStanceTurn(StanceType stance, List<string> log)
    {
        if (TickEffects(Player, playerEffects, log)) return CheckResult();

        if (TryConsumeStun(Player, playerEffects, log))
        {
            RecoverStamina(Player, log);
            return CheckResult();
        }

        recentPlayerMoveIds.Clear();
        log.Add(stance == StanceType.Parry
            ? $"{Player.Name} braces to parry the next strike."
            : $"{Player.Name} clinches up, slowing the pace.");
        RecoverStamina(Player, log);
        return CheckResult();
    }

    // Speed difference nudges who goes first, but never guarantees it.
    bool RollsFirst(int actorSpeed, int otherSpeed)
    {
        int diff = actorSpeed - otherSpeed;
        int chance = 50 + Mathf.Clamp(diff * 2, -40, 40);
        return rng.Next(0, 100) < chance;
    }

    BattleResult RunPlayerTurn(MoveData move, List<string> log)
    {
        if (TickEffects(Player, playerEffects, log)) return CheckResult();

        if (TryConsumeStun(Player, playerEffects, log))
        {
            RecoverStamina(Player, log);
            return CheckResult();
        }

        if (move == null)
        {
            log.Add($"{Player.Name} hesitates and does nothing.");
        }
        else if (move.StaminaCost > Player.Stats.CurrentStamina)
        {
            log.Add($"{Player.Name} is too exhausted to use {move.Name}!");
        }
        else
        {
            Player.Stats.CurrentStamina -= move.StaminaCost;

            recentPlayerMoveIds.Add(move.Id);
            if (recentPlayerMoveIds.Count > MaxComboTrackLength) recentPlayerMoveIds.RemoveAt(0);
            var combo = ComboDatabase.TryMatch(recentPlayerMoveIds);
            if (combo != null) recentPlayerMoveIds.Clear();

            ResolveMove(Player, Opponent, move, opponentEffects, log, combo);
        }

        RecoverStamina(Player, log);
        return CheckResult();
    }

    BattleResult RunOpponentTurn(List<string> log)
    {
        if (TickEffects(Opponent, opponentEffects, log)) return CheckResult();

        if (TryConsumeStun(Opponent, opponentEffects, log))
        {
            RecoverStamina(Opponent, log);
            return CheckResult();
        }

        // Milestone 30, Part 5: the AI can also choose to recover when low on
        // stamina, instead of just sitting at low stamina behind passive regen.
        float staminaRatio = (float)Opponent.Stats.CurrentStamina / Opponent.Stats.MaxStamina;
        bool wantsToRecover = staminaRatio < AiLowStaminaRatioThreshold && rng.Next(0, 100) < AiRecoverChancePercent;

        // Milestone 40, Part 4: a second, complementary roll for a defensive
        // stance instead of attacking - more likely at low stamina, when
        // losing badly, or when the player looks like they're mid-combo.
        // Layered after the recover roll (not instead of it) so very low
        // stamina still gets two chances at a non-attack turn, and capped so
        // the AI is never defensive every single turn (Part 4's instruction).
        bool aiLosingBadly = Opponent.Stats.CurrentHealth < Opponent.Stats.MaxHealth * 0.4f
            && Opponent.Stats.CurrentHealth < Player.Stats.CurrentHealth;
        bool facingComboPressure = IsBuildingTowardCombo(recentPlayerMoveIds);
        // Milestone 51, Part 3/5: opponent-specific additive bias (0 for
        // everyone except Wrestling gym fighters) - reinforces that gym's
        // "control" identity by having its fighters lean on Parry/Clinch
        // more, so the player has to learn those mechanics to get past them.
        int defenseChance = AiDefenseBaseChancePercent
            + (staminaRatio < AiLowStaminaRatioThreshold ? AiDefenseLowStaminaBonusPercent : 0)
            + (aiLosingBadly ? AiDefenseLosingBadlyBonusPercent : 0)
            + (facingComboPressure ? AiDefenseComboPressureBonusPercent : 0)
            + Opponent.DefenseBiasPercent;
        bool wantsToDefend = !wantsToRecover && rng.Next(0, 100) < defenseChance;

        var move = (wantsToRecover || wantsToDefend) ? null : ChooseEnemyMove(Opponent);
        if (wantsToDefend)
        {
            // Combo pressure or low stamina call for the reliable Clinch
            // (breaks the chain / banks stamina for certain); otherwise the AI
            // gambles on a Parry, same as a player might.
            opponentStance = (facingComboPressure || staminaRatio < AiLowStaminaRatioThreshold) ? StanceType.Clinch : StanceType.Parry;
            recentOpponentMoveIds.Clear();
            log.Add(opponentStance == StanceType.Parry
                ? $"{Opponent.Name} braces to parry the next strike."
                : $"{Opponent.Name} clinches up, slowing the pace.");
            RecoverStamina(Opponent, log);
        }
        else if (move == null)
        {
            recentOpponentMoveIds.Clear();
            PerformRecover(Opponent, log);
        }
        else
        {
            Opponent.Stats.CurrentStamina -= move.StaminaCost;

            // Milestone 31, Part 8: the AI has no idea combos exist - it never
            // chooses moves to set one up - but if ChooseEnemyMove's ordinary
            // logic happens to land on a real sequence, it still gets the bonus.
            recentOpponentMoveIds.Add(move.Id);
            if (recentOpponentMoveIds.Count > MaxComboTrackLength) recentOpponentMoveIds.RemoveAt(0);
            var aiCombo = ComboDatabase.TryMatch(recentOpponentMoveIds);
            if (aiCombo != null) recentOpponentMoveIds.Clear();

            ResolveMove(Opponent, Player, move, playerEffects, log, aiCombo);
            RecoverStamina(Opponent, log);
        }

        return CheckResult();
    }

    // Milestone 40, Part 4: detects whether recentIds' tail matches the start
    // (but not the whole) of any known combo sequence - "the player just
    // threw the first move or two of a combo." Read-only probe, no tracking
    // changes; reused by the AI's defensive-stance decision above.
    static bool IsBuildingTowardCombo(List<string> recentIds)
    {
        if (recentIds.Count == 0) return false;
        foreach (var combo in ComboDatabase.All)
        {
            var seq = combo.SequenceMoveIds;
            int checkLen = Mathf.Min(recentIds.Count, seq.Length - 1);
            if (checkLen <= 0) continue;

            bool matches = true;
            for (int i = 0; i < checkLen; i++)
            {
                if (recentIds[recentIds.Count - checkLen + i] != seq[i]) { matches = false; break; }
            }
            if (matches) return true;
        }
        return false;
    }

    // Applies start-of-turn Bleed damage and expires any effects whose duration has ended.
    // Returns true if the fighter was knocked out by bleed this tick.
    bool TickEffects(FighterData fighter, List<ActiveStatusEffect> effects, List<string> log)
    {
        for (int i = effects.Count - 1; i >= 0; i--)
        {
            var effect = effects[i];

            if (effect.Type == StatusEffectType.Bleed)
            {
                fighter.Stats.CurrentHealth = Mathf.Max(0, fighter.Stats.CurrentHealth - effect.Magnitude);
                log.Add($"{fighter.Name} takes {effect.Magnitude} bleed damage.");
            }

            effect.RemainingTurns--;
            if (effect.RemainingTurns <= 0)
            {
                ExpireEffect(fighter, effect, log);
                effects.RemoveAt(i);
            }
        }

        return fighter.Stats.IsKnockedOut;
    }

    bool TryConsumeStun(FighterData fighter, List<ActiveStatusEffect> effects, List<string> log)
    {
        bool isStunned = false;
        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i].Type == StatusEffectType.Stun) { isStunned = true; break; }
        }
        if (!isStunned) return false;

        if (rng.Next(0, 100) < StunSkipChancePercent)
        {
            log.Add($"{fighter.Name} is stunned and cannot act!");
            return true;
        }
        return false;
    }

    void ExpireEffect(FighterData fighter, ActiveStatusEffect effect, List<string> log)
    {
        switch (effect.Type)
        {
            case StatusEffectType.DefenseDown:
                fighter.Stats.Defense += effect.Magnitude;
                log.Add($"{fighter.Name}'s defense returns to normal.");
                break;
            case StatusEffectType.SpeedDown:
                fighter.Stats.Speed += effect.Magnitude;
                log.Add($"{fighter.Name}'s speed returns to normal.");
                break;
            case StatusEffectType.Stun:
                log.Add($"{fighter.Name} shakes off the stun.");
                break;
            case StatusEffectType.Bleed:
                log.Add($"{fighter.Name} stops bleeding.");
                break;
        }
    }

    void ApplyEffect(FighterData fighter, List<ActiveStatusEffect> effects, StatusEffectType type, List<string> log)
    {
        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i].Type == type)
            {
                effects[i].RemainingTurns = GetDuration(type);
                return;
            }
        }

        int magnitude = GetMagnitude(type);
        var effect = new ActiveStatusEffect { Type = type, RemainingTurns = GetDuration(type), Magnitude = magnitude };
        effects.Add(effect);

        switch (type)
        {
            case StatusEffectType.Bleed:
                log.Add($"{fighter.Name} is bleeding!");
                break;
            case StatusEffectType.Stun:
                log.Add($"{fighter.Name} is stunned!");
                break;
            case StatusEffectType.DefenseDown:
            {
                int before = fighter.Stats.Defense;
                fighter.Stats.Defense = Mathf.Max(1, before - magnitude);
                effect.Magnitude = before - fighter.Stats.Defense;
                log.Add($"{fighter.Name}'s defense drops!");
                break;
            }
            case StatusEffectType.SpeedDown:
            {
                int before = fighter.Stats.Speed;
                fighter.Stats.Speed = Mathf.Max(1, before - magnitude);
                effect.Magnitude = before - fighter.Stats.Speed;
                log.Add($"{fighter.Name}'s speed drops!");
                break;
            }
        }
    }

    static int GetDuration(StatusEffectType type)
    {
        switch (type)
        {
            case StatusEffectType.Bleed: return BleedDuration;
            case StatusEffectType.Stun: return StunDuration;
            case StatusEffectType.DefenseDown: return DefenseDownDuration;
            case StatusEffectType.SpeedDown: return SpeedDownDuration;
            default: return 0;
        }
    }

    static int GetMagnitude(StatusEffectType type)
    {
        switch (type)
        {
            case StatusEffectType.Bleed: return BleedDamagePerTurn;
            case StatusEffectType.DefenseDown: return DefenseDownAmount;
            case StatusEffectType.SpeedDown: return SpeedDownAmount;
            default: return 0;
        }
    }

    void ResolveMove(FighterData attacker, FighterData defender, MoveData move, List<ActiveStatusEffect> defenderEffects, List<string> log, ComboData combo = null)
    {
        int hitRoll = rng.Next(0, 100);
        if (hitRoll >= move.Accuracy)
        {
            log.Add($"{attacker.Name} attempts {move.Name} but misses!");
            return;
        }

        int attackStat = attacker.Stats.GetPrimaryStatForMoveType(move.Type);
        float baseDamage = move.Power * (attackStat / 10f) - defender.Stats.Defense * 0.5f;

        bool crit = move.HasEffect(MoveEffect.CriticalHit) && rng.Next(0, 100) < move.EffectChance;
        if (crit) baseDamage *= 1.5f;
        if (crit && attacker == Player) PlayerCriticalHits++;

        // Milestone 31, Part 6/7: the combo bonus only announces once the move
        // is confirmed to land - the move's own name stays in the hit line
        // below so BattleScreen's existing move-type lookup for hit animations
        // keeps working unchanged. The first line starts with the attacker's
        // name so BattleScreen's existing name-prefix routing puts the "COMBO!"
        // popup on the right side, whether it's the player or (per Part 8) the
        // AI landing it by chance.
        if (combo != null)
        {
            if (attacker == Player) PlayerCombosTriggered++;
            baseDamage *= combo.DamageBonusMultiplier;
            attacker.Stats.CurrentStamina = Mathf.Min(attacker.Stats.MaxStamina, attacker.Stats.CurrentStamina + combo.StaminaRefund);
            log.Add($"{attacker.Name} lands a COMBO! {combo.DisplayName}!");
            log.Add($"COMBO ACTIVATED: {combo.DisplayName}\n{move.Name} deals bonus damage. (+{combo.StaminaRefund} stamina)");
        }

        // Milestone 40, Part 1/2/3/5: the defender's declared stance (if any)
        // reduces or negates this hit, restores some of their stamina, and -
        // per Part 5 - can interrupt the attacker's in-progress combo chain.
        // Checked after the combo bonus above so a successful block reduces
        // the full hit (combo bonus included), not just the move's base power.
        StanceType defenderStance = defender == Player ? playerStance : opponentStance;
        var attackerRecentIds = attacker == Player ? recentPlayerMoveIds : recentOpponentMoveIds;
        bool fullyBlocked = false;

        if (defenderStance == StanceType.Parry)
        {
            int successChance = Mathf.Min(ParryMaxSuccessChancePercent, ParryBaseChancePercent + defender.Stats.Defense * ParryDefenseScalingPercent);
            int parryRoll = rng.Next(0, 100);
            if (parryRoll < successChance)
            {
                fullyBlocked = true;
                baseDamage = 0f;
                if (defender == Player) PlayerParriesSucceeded++;
                defender.Stats.CurrentStamina = Mathf.Min(defender.Stats.MaxStamina, defender.Stats.CurrentStamina + ParryFullBlockStaminaGain);
                log.Add($"{defender.Name} reads it perfectly! PARRY!");
            }
            else if (parryRoll < successChance + ParryPartialChancePercent)
            {
                baseDamage *= 0.5f;
                defender.Stats.CurrentStamina = Mathf.Min(defender.Stats.MaxStamina, defender.Stats.CurrentStamina + ParryPartialBlockStaminaGain);
                log.Add($"{defender.Name} catches part of it. PARTIAL BLOCK!");
            }
            // else: parry failed - normal damage, no extra line.
        }
        else if (defenderStance == StanceType.Clinch)
        {
            baseDamage *= ClinchDamageMultiplier;
            defender.Stats.CurrentStamina = Mathf.Min(defender.Stats.MaxStamina, defender.Stats.CurrentStamina + ClinchStaminaGain);
            log.Add($"{defender.Name} smothers it in the clinch. CLINCH SUCCESS!");
        }

        bool shouldBreakCombo = fullyBlocked || defenderStance == StanceType.Clinch;
        if (shouldBreakCombo)
        {
            bool hadProgress = combo == null && attackerRecentIds.Count > 0;
            attackerRecentIds.Clear();
            if (hadProgress) log.Add($"{attacker.Name}'s rhythm is broken! COMBO INTERRUPTED!");
        }

        int damage = fullyBlocked ? 0 : Mathf.Max(1, Mathf.RoundToInt(baseDamage));
        defender.Stats.CurrentHealth = Mathf.Max(0, defender.Stats.CurrentHealth - damage);

        // Milestone 49: a landed Control-category move (Double Leg Takedown
        // etc.) is the closest existing concept to "takedown" - only counts
        // if it actually connected, not a blocked attempt.
        if (attacker == Player && !fullyBlocked && move.Category == MoveCategory.Control) PlayerTakedownsLanded++;

        // Milestone 41, Part 3: an MMA-themed clause appended after the exact
        // existing sentence (numbers and all) - appended, not replaced, so
        // every substring BattleScreen's log parsing already keys off
        // ("damage!"/"damage."/"CRITICAL"/" hits ") still appears intact.
        log.Add(fullyBlocked
            ? $"{attacker.Name}'s {move.Name} is completely blocked!"
            : crit
                ? $"{attacker.Name} lands a CRITICAL {move.Name} for {damage} damage! {GetCombatFlavor(move)}"
                : $"{attacker.Name} hits {move.Name} for {damage} damage. {GetCombatFlavor(move)}");

        // A fully blocked strike never connected - none of its secondary
        // effects (bleed/stun/etc.) should land either.
        if (fullyBlocked) return;
        if (defender.Stats.IsKnockedOut) return;

        TryApplyMoveEffect(move, MoveEffect.Bleed, StatusEffectType.Bleed, defender, defenderEffects, log);
        TryApplyMoveEffect(move, MoveEffect.Stun, StatusEffectType.Stun, defender, defenderEffects, log);
        TryApplyMoveEffect(move, MoveEffect.DefenseReduction, StatusEffectType.DefenseDown, defender, defenderEffects, log);
        TryApplyMoveEffect(move, MoveEffect.SpeedReduction, StatusEffectType.SpeedDown, defender, defenderEffects, log);
    }

    // Milestone 41, Part 3: a short MMA-themed clause per move, derived from
    // its existing Category/Effects (no new data) - "Jab lands clean.",
    // "Leg Kick slows the opponent.", "Hook connects as a finisher.",
    // "Double Leg secures control." Same phrasing for every move sharing a
    // category, but the move's own name keeps each line feeling specific.
    static string GetCombatFlavor(MoveData move)
    {
        switch (move.Category)
        {
            case MoveCategory.Starter:
            case MoveCategory.Combo:
                return $"{move.Name} lands clean.";
            case MoveCategory.Finisher:
                return $"{move.Name} connects as a finisher.";
            case MoveCategory.Pressure:
                if (move.HasEffect(MoveEffect.SpeedReduction)) return $"{move.Name} slows the opponent.";
                if (move.HasEffect(MoveEffect.DefenseReduction)) return $"{move.Name} cracks the guard open.";
                if (move.HasEffect(MoveEffect.Bleed)) return $"{move.Name} opens the cut wider.";
                return $"{move.Name} wears them down.";
            case MoveCategory.Control:
                return $"{move.Name} secures control.";
            case MoveCategory.Submission:
                return $"{move.Name} hunts for the tap.";
            case MoveCategory.Defensive:
                return $"{move.Name} creates space.";
            default:
                return "";
        }
    }

    void TryApplyMoveEffect(MoveData move, MoveEffect flag, StatusEffectType statusType, FighterData defender,
        List<ActiveStatusEffect> defenderEffects, List<string> log)
    {
        if (!move.HasEffect(flag)) return;
        if (rng.Next(0, 100) >= move.EffectChance) return;

        ApplyEffect(defender, defenderEffects, statusType, log);
    }

    MoveData ChooseEnemyMove(FighterData enemy)
    {
        affordableMovesBuffer.Clear();
        MoveData cheapest = null;
        var knownMoves = enemy.KnownMoves;
        for (int i = 0; i < knownMoves.Count; i++)
        {
            var m = knownMoves[i];
            if (m.StaminaCost > enemy.Stats.CurrentStamina) continue;

            affordableMovesBuffer.Add(m);
            if (cheapest == null || m.StaminaCost < cheapest.StaminaCost) cheapest = m;
        }
        if (affordableMovesBuffer.Count == 0) return null;

        // Milestone 34, Part 5/6: an opt-in smarter policy for specific
        // opponents (Rival Scratch) only - every other fighter keeps the
        // exact chance-based behavior below, completely unchanged.
        if (enemy.IsSmartFighter) return ChooseSmartEnemyMove(cheapest);

        // Occasionally conserve stamina even when a stronger move is affordable.
        if (rng.Next(0, 100) < AiConserveStaminaChancePercent) return cheapest;

        float staminaRatio = (float)enemy.Stats.CurrentStamina / enemy.Stats.MaxStamina;
        if (staminaRatio < AiLowStaminaRatioThreshold) return cheapest;

        // Otherwise lean toward the stronger half of what's affordable, with some variety.
        affordableMovesBuffer.Sort((a, b) => b.Power.CompareTo(a.Power));
        int topCount = Mathf.Max(1, Mathf.CeilToInt(affordableMovesBuffer.Count * AiTopMovePoolFraction));
        return affordableMovesBuffer[rng.Next(topCount)];
    }

    // Milestone 34, Part 5/6: reuses affordableMovesBuffer (already built by the
    // caller) and the same recentOpponentMoveIds/ComboDatabase the incidental-AI-
    // combo support from Milestone 31 already added - no new tracking, no new
    // combat math, just a better decision over the same affordable-moves list.
    MoveData ChooseSmartEnemyMove(MoveData cheapest)
    {
        // Prioritize finishing a combo (Part 6) - if any affordable move would
        // complete a known sequence, take it immediately for the bonus.
        for (int i = 0; i < affordableMovesBuffer.Count; i++)
        {
            var candidate = affordableMovesBuffer[i];
            probeMoveIdsBuffer.Clear();
            probeMoveIdsBuffer.AddRange(recentOpponentMoveIds);
            probeMoveIdsBuffer.Add(candidate.Id);
            if (ComboDatabase.TryMatch(probeMoveIdsBuffer) != null) return candidate;
        }

        // Never waste a heavy move at low stamina (Part 5) - deterministic,
        // not just "sometimes" like the generic AI's chance roll.
        float staminaRatio = (float)Opponent.Stats.CurrentStamina / Opponent.Stats.MaxStamina;
        if (staminaRatio < AiLowStaminaRatioThreshold) return cheapest;

        // Efficient stamina usage (Part 5): best power-per-stamina among what's
        // affordable, instead of a random pick from the top half by raw power.
        MoveData best = affordableMovesBuffer[0];
        float bestRatio = best.Power / (float)Mathf.Max(1, best.StaminaCost);
        for (int i = 1; i < affordableMovesBuffer.Count; i++)
        {
            var m = affordableMovesBuffer[i];
            float ratio = m.Power / (float)Mathf.Max(1, m.StaminaCost);
            if (ratio > bestRatio) { best = m; bestRatio = ratio; }
        }
        return best;
    }

    void RecoverStamina(FighterData fighter, List<string> log)
    {
        if (fighter.Stats.CurrentStamina >= fighter.Stats.MaxStamina) return;

        int recovered = Mathf.Min(StaminaRegenPerTurn, fighter.Stats.MaxStamina - fighter.Stats.CurrentStamina);
        fighter.Stats.CurrentStamina += recovered;
        log.Add($"{fighter.Name} recovers {recovered} stamina.");
    }

    // Milestone 30, Part 5: the deliberate Recover action/AI choice - a much
    // bigger one-time gain than passive regen, replacing it for that turn.
    void PerformRecover(FighterData fighter, List<string> log)
    {
        int recovered = Mathf.Min(RecoverActionAmount, fighter.Stats.MaxStamina - fighter.Stats.CurrentStamina);
        fighter.Stats.CurrentStamina += recovered;
        log.Add(recovered > 0
            ? $"{fighter.Name} catches their breath and recovers {recovered} stamina."
            : $"{fighter.Name} is already at full stamina.");
    }

    BattleResult CheckResult()
    {
        if (Opponent.Stats.IsKnockedOut) return BattleResult.PlayerWon;
        if (Player.Stats.IsKnockedOut) return BattleResult.PlayerLost;
        return BattleResult.Ongoing;
    }

    // Reverts any still-active temporary stat modifiers. Call when the battle ends
    // so Defense/Speed Down can never leak past the fight that applied them.
    public void Cleanup()
    {
        RevertAll(Player, playerEffects);
        RevertAll(Opponent, opponentEffects);
    }

    void RevertAll(FighterData fighter, List<ActiveStatusEffect> effects)
    {
        foreach (var effect in effects)
        {
            if (effect.Type == StatusEffectType.DefenseDown) fighter.Stats.Defense += effect.Magnitude;
            else if (effect.Type == StatusEffectType.SpeedDown) fighter.Stats.Speed += effect.Magnitude;
        }
        effects.Clear();
    }
}
