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
    const int AiRecoverChancePercent = 60;
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

    public readonly FighterData Player;
    public readonly FighterData Opponent;

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
            if (result != BattleResult.Ongoing) return result;
            result = RunOpponentTurn(log);
        }
        else
        {
            result = RunOpponentTurn(log);
            if (result != BattleResult.Ongoing) return result;
            result = RunPlayerTurn(move, log);
        }
        return result;
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
            if (result != BattleResult.Ongoing) return result;
            result = RunOpponentTurn(log);
        }
        else
        {
            result = RunOpponentTurn(log);
            if (result != BattleResult.Ongoing) return result;
            result = RunPlayerRecoverTurn(log);
        }
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

        var move = wantsToRecover ? null : ChooseEnemyMove(Opponent);
        if (move == null)
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

        // Milestone 31, Part 6/7: the combo bonus only announces once the move
        // is confirmed to land - the move's own name stays in the hit line
        // below so BattleScreen's existing move-type lookup for hit animations
        // keeps working unchanged. The first line starts with the attacker's
        // name so BattleScreen's existing name-prefix routing puts the "COMBO!"
        // popup on the right side, whether it's the player or (per Part 8) the
        // AI landing it by chance.
        if (combo != null)
        {
            baseDamage *= combo.DamageBonusMultiplier;
            attacker.Stats.CurrentStamina = Mathf.Min(attacker.Stats.MaxStamina, attacker.Stats.CurrentStamina + combo.StaminaRefund);
            log.Add($"{attacker.Name} lands a COMBO! {combo.DisplayName}!");
            log.Add($"COMBO ACTIVATED: {combo.DisplayName}\n{move.Name} deals bonus damage. (+{combo.StaminaRefund} stamina)");
        }

        int damage = Mathf.Max(1, Mathf.RoundToInt(baseDamage));
        defender.Stats.CurrentHealth = Mathf.Max(0, defender.Stats.CurrentHealth - damage);

        log.Add(crit
            ? $"{attacker.Name} lands a CRITICAL {move.Name} for {damage} damage!"
            : $"{attacker.Name} hits {move.Name} for {damage} damage.");

        if (defender.Stats.IsKnockedOut) return;

        TryApplyMoveEffect(move, MoveEffect.Bleed, StatusEffectType.Bleed, defender, defenderEffects, log);
        TryApplyMoveEffect(move, MoveEffect.Stun, StatusEffectType.Stun, defender, defenderEffects, log);
        TryApplyMoveEffect(move, MoveEffect.DefenseReduction, StatusEffectType.DefenseDown, defender, defenderEffects, log);
        TryApplyMoveEffect(move, MoveEffect.SpeedReduction, StatusEffectType.SpeedDown, defender, defenderEffects, log);
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
