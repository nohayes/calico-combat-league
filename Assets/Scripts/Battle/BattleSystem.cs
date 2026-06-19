using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystem
{
    const int StaminaRegenPerTurn = 8;

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
            ResolveMove(Player, Opponent, move, opponentEffects, log);
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

        var move = ChooseEnemyMove(Opponent);
        if (move == null)
        {
            log.Add($"{Opponent.Name} is too exhausted to attack!");
        }
        else
        {
            Opponent.Stats.CurrentStamina -= move.StaminaCost;
            ResolveMove(Opponent, Player, move, playerEffects, log);
        }

        RecoverStamina(Opponent, log);
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
        effects.Add(new ActiveStatusEffect { Type = type, RemainingTurns = GetDuration(type), Magnitude = magnitude });

        switch (type)
        {
            case StatusEffectType.Bleed:
                log.Add($"{fighter.Name} is bleeding!");
                break;
            case StatusEffectType.Stun:
                log.Add($"{fighter.Name} is stunned!");
                break;
            case StatusEffectType.DefenseDown:
                fighter.Stats.Defense = Mathf.Max(1, fighter.Stats.Defense - magnitude);
                log.Add($"{fighter.Name}'s defense drops!");
                break;
            case StatusEffectType.SpeedDown:
                fighter.Stats.Speed = Mathf.Max(1, fighter.Stats.Speed - magnitude);
                log.Add($"{fighter.Name}'s speed drops!");
                break;
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

    void ResolveMove(FighterData attacker, FighterData defender, MoveData move, List<ActiveStatusEffect> defenderEffects, List<string> log)
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

        // Occasionally conserve stamina even when a stronger move is affordable.
        if (rng.Next(0, 100) < AiConserveStaminaChancePercent) return cheapest;

        float staminaRatio = (float)enemy.Stats.CurrentStamina / enemy.Stats.MaxStamina;
        if (staminaRatio < AiLowStaminaRatioThreshold) return cheapest;

        // Otherwise lean toward the stronger half of what's affordable, with some variety.
        affordableMovesBuffer.Sort((a, b) => b.Power.CompareTo(a.Power));
        int topCount = Mathf.Max(1, Mathf.CeilToInt(affordableMovesBuffer.Count * AiTopMovePoolFraction));
        return affordableMovesBuffer[rng.Next(topCount)];
    }

    void RecoverStamina(FighterData fighter, List<string> log)
    {
        if (fighter.Stats.CurrentStamina >= fighter.Stats.MaxStamina) return;

        int recovered = Mathf.Min(StaminaRegenPerTurn, fighter.Stats.MaxStamina - fighter.Stats.CurrentStamina);
        fighter.Stats.CurrentStamina += recovered;
        log.Add($"{fighter.Name} recovers {recovered} stamina.");
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
