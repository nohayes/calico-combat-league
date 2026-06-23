using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public FighterData Player { get; private set; }
    public GymInfo CurrentGym { get; private set; }
    public FighterData CurrentOpponent { get; private set; }
    public OpponentInfo CurrentOpponentInfo { get; private set; }
    public BattleSystem CurrentBattle { get; private set; }
    public GameState State { get; private set; } = GameState.MainMenu;

    public int LastRewardXP { get; private set; }
    public int LastRewardCoins { get; private set; }
    public bool LastVictoryUnlockedGym { get; private set; }
    public string LastUnlockedMoveName { get; private set; }

    // ---------- Lifetime stats (persist across StartNewGame/StartFreshGame - see Milestone 11) ----------

    public int TotalWins { get; private set; }
    public int TotalLosses { get; private set; }
    public int TotalBattles => TotalWins + TotalLosses;
    public int TotalDamageDealt { get; private set; }
    public int TotalDamageTaken { get; private set; }
    public int TotalCoinsEarned { get; private set; }
    public int TotalCoinsSpent { get; private set; }
    public int TotalItemsUsed { get; private set; }
    public int MaxSingleHitDamage { get; private set; }
    public int SubmissionWins { get; private set; }

    // Gyms cleared by the CURRENT fighter (resets per playthrough, unlike the lifetime stats above).
    public int TotalGymsCleared => completedGymIds.Count;

    // Milestone 29: per-playthrough flag controlling the rival's one-time first-appearance line.
    public bool HasSeenRivalIntro => hasSeenRivalIntro;
    bool hasSeenRivalIntro;

    // Milestone 30, Part 1: the currently-rolled Street Fight encounter. Pure
    // runtime state - never saved, regenerated fresh each time the player
    // opens the Street Fight screen or hits Reroll.
    public StreetFightOpponent CurrentStreetFightOpponent { get; private set; }

    readonly HashSet<string> unlockedAchievementIds = new HashSet<string>();
    readonly List<ChampionRecord> hallOfChampions = new List<ChampionRecord>();

    public IReadOnlyList<ChampionRecord> HallOfChampions => hallOfChampions;

    readonly HashSet<string> defeatedOpponentIds = new HashSet<string>();
    readonly HashSet<string> completedGymIds = new HashSet<string>();
    readonly Dictionary<string, int> inventory = new Dictionary<string, int>();

    bool combatBuffActive;
    int activeCombatBuffAmount;
    bool currentBattleIsRematchBoosted;

    const float RematchStatMultiplier = 1.3f;
    const float RematchRewardMultiplier = 1.5f;

    public event Action<GameState> OnStateChanged;

    void Awake()
    {
        Instance = this;
        LoadGame();
    }

    public void ChangeState(GameState newState)
    {
        State = newState;
        OnStateChanged?.Invoke(newState);
    }

    public void ContinueGame()
    {
        if (Player == null)
        {
            Debug.LogWarning("ContinueGame called with no loaded player.");
            return;
        }
        ChangeState(GameState.GymMap);
    }

    public void StartFreshGame()
    {
        SaveSystem.DeleteSave();
        Player = null;
        defeatedOpponentIds.Clear();
        completedGymIds.Clear();
        inventory.Clear();
        combatBuffActive = false;
        activeCombatBuffAmount = 0;
        hasSeenRivalIntro = false;
        ChangeState(GameState.FighterCreation);
    }

    public void StartNewGame(string fighterName, ArchetypeType archetype)
    {
        string name = string.IsNullOrWhiteSpace(fighterName) ? "Fighter" : fighterName.Trim();
        defeatedOpponentIds.Clear();
        completedGymIds.Clear();
        inventory.Clear();
        combatBuffActive = false;
        activeCombatBuffAmount = 0;
        hasSeenRivalIntro = false;
        Player = FighterData.CreateNewPlayer(name, archetype);
        SaveGame();
        ChangeState(GameState.GymMap);
    }

    // Milestone 29: called once by GymMapScreen when it shows the rival's
    // first-appearance line, so a re-Refresh (e.g. returning from another
    // screen) never shows it twice in the same run.
    public void MarkRivalIntroSeen()
    {
        if (hasSeenRivalIntro) return;
        hasSeenRivalIntro = true;
        SaveGame();
    }

    public bool IsGymUnlocked(GymInfo gym)
    {
        if (gym == null) return false;
        return string.IsNullOrEmpty(gym.RequiredGymId) || completedGymIds.Contains(gym.RequiredGymId);
    }

    public bool IsGymCompleted(GymInfo gym)
    {
        return gym != null && completedGymIds.Contains(gym.GymId);
    }

    public bool IsOpponentDefeated(OpponentInfo opponent)
    {
        return opponent != null && defeatedOpponentIds.Contains(opponent.OpponentId);
    }

    public bool IsLeaderUnlocked(GymInfo gym)
    {
        if (gym?.Trainers == null) return false;
        return gym.Trainers.All(trainer => defeatedOpponentIds.Contains(trainer.OpponentId));
    }

    public void EnterGym(GymInfo gym)
    {
        if (gym == null)
        {
            Debug.LogWarning("EnterGym called with a null gym.");
            return;
        }
        if (!IsGymUnlocked(gym))
        {
            Debug.LogWarning($"Tried to enter locked gym '{gym.GymId}'.");
            return;
        }

        CurrentGym = gym;
        ChangeState(GameState.GymScreen);
    }

    // Milestone 30, Part 1: a randomized optional battle outside gym progression.
    // Uses a synthetic GymInfo with no Leader/RequiredGymId - the same pattern
    // the Shadow Champion fight already established - so EndBattle's gym-clear
    // and championship bookkeeping naturally no-ops for it. Nothing here writes
    // to completedGymIds, so gym progression is untouched.
    public void RollStreetFightOpponent()
    {
        CurrentStreetFightOpponent = StreetFightGenerator.Generate(this);
    }

    public void StartStreetFight()
    {
        if (Player == null || CurrentStreetFightOpponent == null) return;

        CurrentGym = new GymInfo { GymId = "street_fight", GymName = "Street Fight", GymType = GymType.Boxing };
        StartBattle(CurrentStreetFightOpponent.Opponent);
    }

    public void StartBattle(OpponentInfo opponent)
    {
        if (Player == null)
        {
            Debug.LogWarning("StartBattle called before a player fighter exists.");
            return;
        }
        if (opponent == null)
        {
            Debug.LogWarning("StartBattle called with a null opponent.");
            return;
        }
        if (opponent.Stats == null || opponent.Moves == null || opponent.Moves.Count == 0)
        {
            Debug.LogWarning($"Opponent '{opponent.OpponentId}' is missing stats or moves.");
            return;
        }

        CurrentOpponentInfo = opponent;

        var opponentStats = opponent.Stats.Clone();
        currentBattleIsRematchBoosted = IsAnyGymLeader(opponent) && HasBecomeChampion();
        if (currentBattleIsRematchBoosted)
        {
            opponentStats.MaxHealth = Mathf.RoundToInt(opponentStats.MaxHealth * RematchStatMultiplier);
            opponentStats.MaxStamina = Mathf.RoundToInt(opponentStats.MaxStamina * RematchStatMultiplier);
            opponentStats.Strength = Mathf.RoundToInt(opponentStats.Strength * RematchStatMultiplier);
            opponentStats.Defense = Mathf.RoundToInt(opponentStats.Defense * RematchStatMultiplier);
            opponentStats.Speed = Mathf.RoundToInt(opponentStats.Speed * RematchStatMultiplier);
            opponentStats.Striking = Mathf.RoundToInt(opponentStats.Striking * RematchStatMultiplier);
            opponentStats.Grappling = Mathf.RoundToInt(opponentStats.Grappling * RematchStatMultiplier);
            opponentStats.Submission = Mathf.RoundToInt(opponentStats.Submission * RematchStatMultiplier);
            opponentStats.ResetForBattle();
        }

        CurrentOpponent = new FighterData(opponent.Name, opponentStats, opponent.Moves);
        CurrentBattle = new BattleSystem(Player, CurrentOpponent);
        ChangeState(GameState.Battle);
    }

    public bool HasBecomeChampion()
    {
        var gyms = GymDatabase.AllGyms;
        return gyms.Count > 0 && IsGymCompleted(gyms[gyms.Count - 1]);
    }

    static bool IsAnyGymLeader(OpponentInfo opponent)
    {
        if (opponent == null) return false;
        foreach (var gym in GymDatabase.AllGyms)
            if (gym.Leader != null && gym.Leader.OpponentId == opponent.OpponentId) return true;
        return false;
    }

    public void EndBattle(BattleResult result, bool submissionFinish = false)
    {
        if (Player == null || CurrentOpponentInfo == null)
        {
            Debug.LogWarning("EndBattle called with no active battle context.");
            return;
        }

        RevertCombatBuff();
        CurrentBattle?.Cleanup();

        LastVictoryUnlockedGym = false;
        LastUnlockedMoveName = null;

        if (result == BattleResult.PlayerWon)
        {
            LastRewardXP = CurrentOpponentInfo.RewardXP;
            LastRewardCoins = CurrentOpponentInfo.RewardCoins;
            if (currentBattleIsRematchBoosted)
            {
                LastRewardXP = Mathf.RoundToInt(LastRewardXP * RematchRewardMultiplier);
                LastRewardCoins = Mathf.RoundToInt(LastRewardCoins * RematchRewardMultiplier);
            }
            Player.Stats.AddXP(LastRewardXP);
            Player.Stats.Coins += LastRewardCoins;
            TotalCoinsEarned += LastRewardCoins;
            TotalWins++;
            if (submissionFinish) SubmissionWins++;
            defeatedOpponentIds.Add(CurrentOpponentInfo.OpponentId);

            bool becameChampionJustNow = false;
            if (CurrentGym?.Leader != null && CurrentGym.Leader.OpponentId == CurrentOpponentInfo.OpponentId)
            {
                bool wasAlreadyCompleted = completedGymIds.Contains(CurrentGym.GymId);
                completedGymIds.Add(CurrentGym.GymId);
                LastVictoryUnlockedGym = !wasAlreadyCompleted;

                TryUnlockGymMove(CurrentGym);

                if (!wasAlreadyCompleted && IsLastGym(CurrentGym))
                    becameChampionJustNow = true;
            }

            if (becameChampionJustNow) RecordChampionLegacy();
            if (CurrentOpponentInfo.OpponentId == ShadowChampionId) RecordShadowChampionVictory();

            CheckAchievements();
            SaveGame();
            ChangeState(becameChampionJustNow ? GameState.Championship : GameState.Victory);
        }
        else
        {
            TotalLosses++;
            CheckAchievements();
            SaveGame();
            ChangeState(GameState.Defeat);
        }
    }

    void RecordChampionLegacy()
    {
        hallOfChampions.Add(new ChampionRecord
        {
            FighterName = Player.Name,
            Archetype = Player.Archetype.ToString(),
            FinalLevel = Player.Stats.Level,
            TotalWinsAtCompletion = TotalWins,
            CompletionDate = DateTime.Now.ToString("yyyy-MM-dd")
        });
    }

    // ---------- Shadow Champion (Milestone 26) ----------
    // A secret post-championship fight against a mirror of the player's own
    // current fighter. Built entirely on existing systems: StartBattle already
    // does everything a fight needs once given an OpponentInfo, and
    // GymType.Championship already gets the strongest Fight Night presentation
    // tier (see BattleScreen) with zero extra code.

    public const string ShadowChampionId = "shadow_champion";

    public bool HasDefeatedShadowChampion => defeatedOpponentIds.Contains(ShadowChampionId);

    public void StartShadowChampionBattle()
    {
        if (Player == null || !HasBecomeChampion()) return;

        var mirroredStats = Player.Stats.Clone();
        mirroredStats.MaxHealth = Mathf.RoundToInt(mirroredStats.MaxHealth * 1.1f);
        mirroredStats.MaxStamina = Mathf.RoundToInt(mirroredStats.MaxStamina * 1.1f);
        mirroredStats.Speed = Mathf.RoundToInt(mirroredStats.Speed * 1.1f);
        mirroredStats.ResetForBattle();

        var shadowOpponent = new OpponentInfo
        {
            OpponentId = ShadowChampionId,
            Name = $"Shadow {Player.Name}",
            Stats = mirroredStats,
            Moves = new List<MoveData>(Player.EquippedMoves),
            RewardXP = 300,
            RewardCoins = 150,
            Nickname = "Your Reflection",
            Quote = "Every move you know, I know. Every habit you have, I have. The only question left is which one of us blinks first.",
            Bio = $"A perfect reflection of {Player.Name} - same hands, same instincts, same scars. It has studied every fight you've ever won. Now it wants to win one of its own.",
            LossLine = "...Huh. Didn't expect to lose to myself. Good. Be better than this version of you.",
            WinLine = "Of course I won. I'm you, on your best day. Come back when you're better than your best day."
        };

        // GymType.Championship alone gives the fight the strongest existing
        // presentation tier; leaving Leader/Trainers null means EndBattle's
        // normal gym-completion bookkeeping is a no-op for this synthetic gym.
        CurrentGym = new GymInfo
        {
            GymId = "shadow_gym",
            GymName = "The Shadow Gym",
            GymType = GymType.Championship
        };

        StartBattle(shadowOpponent);
    }

    void RecordShadowChampionVictory()
    {
        hallOfChampions.Add(new ChampionRecord
        {
            FighterName = Player.Name,
            Archetype = Player.Archetype.ToString(),
            FinalLevel = Player.Stats.Level,
            TotalWinsAtCompletion = TotalWins,
            CompletionDate = DateTime.Now.ToString("yyyy-MM-dd"),
            Title = "Shadow Slayer"
        });
    }

    void TryUnlockGymMove(GymInfo gym)
    {
        if (string.IsNullOrEmpty(gym.UnlockMoveId)) return;

        var move = MoveDatabase.GetById(gym.UnlockMoveId);
        if (move == null || Player.KnownMoves.Contains(move)) return;

        Player.KnownMoves.Add(move);
        LastUnlockedMoveName = move.Name;
    }

    static bool IsLastGym(GymInfo gym)
    {
        var gyms = GymDatabase.AllGyms;
        return gym != null && gyms.Count > 0 && gyms[gyms.Count - 1].GymId == gym.GymId;
    }

    public void ReturnToMap()
    {
        Player?.Stats.ResetForBattle();
        ChangeState(GameState.GymMap);
    }

    // ---------- Combat stat reporting (called by BattleScreen; BattleSystem itself is untouched) ----------

    public void RecordCombatStats(int damageDealt, int damageTaken)
    {
        TotalDamageDealt += damageDealt;
        TotalDamageTaken += damageTaken;
        if (damageDealt > MaxSingleHitDamage) MaxSingleHitDamage = damageDealt;
        CheckAchievements();
    }

    public void RecordCoinsSpent(int amount)
    {
        TotalCoinsSpent += amount;
        CheckAchievements();
    }

    // ---------- Achievements ----------

    public bool IsAchievementUnlocked(string id) => unlockedAchievementIds.Contains(id);

    public int GetAchievementProgress(AchievementMetric metric)
    {
        switch (metric)
        {
            case AchievementMetric.TotalWins: return TotalWins;
            case AchievementMetric.GymsCleared: return TotalGymsCleared;
            case AchievementMetric.MaxSingleHitDamage: return MaxSingleHitDamage;
            case AchievementMetric.SubmissionWins: return SubmissionWins;
            case AchievementMetric.CoinsSpent: return TotalCoinsSpent;
            case AchievementMetric.MovesKnown: return Player?.KnownMoves.Count ?? 0;
            case AchievementMetric.BecameChampion: return HasBecomeChampion() ? 1 : 0;
            default: return 0;
        }
    }

    void CheckAchievements()
    {
        foreach (var achievement in AchievementDatabase.All)
        {
            if (unlockedAchievementIds.Contains(achievement.Id)) continue;
            if (GetAchievementProgress(achievement.Metric) >= achievement.TargetValue)
                unlockedAchievementIds.Add(achievement.Id);
        }
    }

    // ---------- Items / Shop ----------

    public int GetItemQuantity(string itemId)
    {
        return inventory.TryGetValue(itemId, out int qty) ? qty : 0;
    }

    public List<InventoryEntry> GetInventoryEntries()
    {
        var result = new List<InventoryEntry>();
        foreach (var kvp in inventory)
        {
            if (kvp.Value <= 0) continue;
            var item = ItemDatabase.GetById(kvp.Key);
            if (item != null) result.Add(new InventoryEntry { Item = item, Quantity = kvp.Value });
        }
        return result;
    }

    public bool BuyItem(string itemId)
    {
        if (Player == null)
        {
            Debug.LogWarning("BuyItem called with no player.");
            return false;
        }

        var item = ItemDatabase.GetById(itemId);
        if (item == null)
        {
            Debug.LogWarning($"BuyItem: unknown item '{itemId}'.");
            return false;
        }

        if (Player.Stats.Coins < item.Cost) return false;

        Player.Stats.Coins -= item.Cost;
        TotalCoinsSpent += item.Cost;
        inventory[itemId] = GetItemQuantity(itemId) + 1;
        CheckAchievements();
        SaveGame();
        return true;
    }

    // Returns a battle-log line describing what happened, or null if nothing happened.
    public string UseItem(string itemId)
    {
        if (Player == null) return null;
        if (GetItemQuantity(itemId) <= 0) return null;

        var item = ItemDatabase.GetById(itemId);
        if (item == null) return null;

        if (item.EffectType == ItemEffectType.RestoreStamina &&
            Player.Stats.CurrentStamina >= Player.Stats.MaxStamina)
            return $"{Player.Name} cannot use {item.Name}: stamina is already full.";

        if (item.EffectType == ItemEffectType.RestoreHealth &&
            Player.Stats.CurrentHealth >= Player.Stats.MaxHealth)
            return $"{Player.Name} cannot use {item.Name}: health is already full.";

        string logLine = ApplyItemEffect(item);
        if (logLine == null) return null;

        inventory[itemId] = GetItemQuantity(itemId) - 1;
        TotalItemsUsed++;
        CheckAchievements();
        SaveGame();
        return logLine;
    }

    string ApplyItemEffect(ItemData item)
    {
        switch (item.EffectType)
        {
            case ItemEffectType.RestoreStamina:
            {
                int before = Player.Stats.CurrentStamina;
                Player.Stats.CurrentStamina = Mathf.Min(Player.Stats.MaxStamina, Player.Stats.CurrentStamina + item.EffectAmount);
                int restored = Player.Stats.CurrentStamina - before;
                return $"{Player.Name} uses {item.Name} and recovers {restored} stamina.";
            }
            case ItemEffectType.RestoreHealth:
            {
                int before = Player.Stats.CurrentHealth;
                Player.Stats.CurrentHealth = Mathf.Min(Player.Stats.MaxHealth, Player.Stats.CurrentHealth + item.EffectAmount);
                int restored = Player.Stats.CurrentHealth - before;
                return $"{Player.Name} uses {item.Name} and heals {restored} health.";
            }
            case ItemEffectType.CombatBuff:
            {
                if (combatBuffActive) return null;
                Player.Stats.Strength += item.EffectAmount;
                Player.Stats.Striking += item.EffectAmount;
                Player.Stats.Grappling += item.EffectAmount;
                Player.Stats.Submission += item.EffectAmount;
                combatBuffActive = true;
                activeCombatBuffAmount = item.EffectAmount;
                return $"{Player.Name} uses {item.Name}! Combat stats boosted for this battle.";
            }
            default:
                return null;
        }
    }

    void RevertCombatBuff()
    {
        if (!combatBuffActive) return;

        Player.Stats.Strength -= activeCombatBuffAmount;
        Player.Stats.Striking -= activeCombatBuffAmount;
        Player.Stats.Grappling -= activeCombatBuffAmount;
        Player.Stats.Submission -= activeCombatBuffAmount;
        combatBuffActive = false;
        activeCombatBuffAmount = 0;
    }

    // ---------- Save / Load ----------

    public void SaveGame()
    {
        if (Player == null)
        {
            Debug.LogWarning("SaveGame called with no player to save.");
            return;
        }

        var invIds = new List<string>();
        var invQuantities = new List<int>();
        foreach (var kvp in inventory)
        {
            if (kvp.Value <= 0) continue;
            invIds.Add(kvp.Key);
            invQuantities.Add(kvp.Value);
        }

        var data = new SaveData
        {
            FighterName = Player.Name,
            Level = Player.Stats.Level,
            XP = Player.Stats.XP,
            Coins = Player.Stats.Coins,
            StatPoints = Player.Stats.StatPoints,
            TotalWins = TotalWins,
            Archetype = Player.Archetype,
            MaxHealth = Player.Stats.MaxHealth,
            MaxStamina = Player.Stats.MaxStamina,
            Strength = Player.Stats.Strength - (combatBuffActive ? activeCombatBuffAmount : 0),
            Defense = Player.Stats.Defense,
            Speed = Player.Stats.Speed,
            Striking = Player.Stats.Striking - (combatBuffActive ? activeCombatBuffAmount : 0),
            Grappling = Player.Stats.Grappling - (combatBuffActive ? activeCombatBuffAmount : 0),
            Submission = Player.Stats.Submission - (combatBuffActive ? activeCombatBuffAmount : 0),
            KnownMoveIds = Player.KnownMoves.Select(m => m.Id).ToList(),
            EquippedMoveIds = Player.EquippedMoves.Select(m => m.Id).ToList(),
            DefeatedOpponentIds = defeatedOpponentIds.ToList(),
            CompletedGymIds = completedGymIds.ToList(),
            InventoryItemIds = invIds,
            InventoryItemQuantities = invQuantities,
            TotalLosses = TotalLosses,
            TotalDamageDealt = TotalDamageDealt,
            TotalDamageTaken = TotalDamageTaken,
            TotalCoinsEarned = TotalCoinsEarned,
            TotalCoinsSpent = TotalCoinsSpent,
            TotalItemsUsed = TotalItemsUsed,
            MaxSingleHitDamage = MaxSingleHitDamage,
            SubmissionWins = SubmissionWins,
            UnlockedAchievementIds = unlockedAchievementIds.ToList(),
            HallOfChampions = hallOfChampions.ToList(),
            HasSeenRivalIntro = hasSeenRivalIntro
        };

        SaveSystem.Save(data);
    }

    bool LoadGame()
    {
        var data = SaveSystem.Load();
        if (data == null) return false;

        var stats = new FighterStats
        {
            Level = Mathf.Max(1, data.Level),
            XP = data.XP,
            Coins = data.Coins,
            StatPoints = Mathf.Max(0, data.StatPoints),
            MaxHealth = data.MaxHealth > 0 ? data.MaxHealth : 100,
            MaxStamina = data.MaxStamina > 0 ? data.MaxStamina : 50,
            Strength = data.Strength,
            Defense = data.Defense,
            Speed = data.Speed,
            Striking = data.Striking,
            Grappling = data.Grappling,
            Submission = data.Submission
        };
        stats.ResetForBattle();
        TotalWins = Mathf.Max(0, data.TotalWins);

        var knownMoves = ResolveMoveIds(data.KnownMoveIds);
        if (knownMoves.Count == 0) knownMoves = new List<MoveData>(MoveDatabase.StartingMoves);

        var equippedMoves = ResolveMoveIds(data.EquippedMoveIds);
        if (equippedMoves.Count == 0)
        {
            equippedMoves = new List<MoveData>();
            for (int i = 0; i < knownMoves.Count && i < 4; i++) equippedMoves.Add(knownMoves[i]);
        }

        string fighterName = string.IsNullOrWhiteSpace(data.FighterName) ? "Fighter" : data.FighterName;
        Player = new FighterData(fighterName, stats, knownMoves, equippedMoves) { Archetype = data.Archetype };

        defeatedOpponentIds.Clear();
        if (data.DefeatedOpponentIds != null)
            foreach (var id in data.DefeatedOpponentIds) defeatedOpponentIds.Add(id);

        completedGymIds.Clear();
        if (data.CompletedGymIds != null)
            foreach (var id in data.CompletedGymIds) completedGymIds.Add(id);

        inventory.Clear();
        if (data.InventoryItemIds != null && data.InventoryItemQuantities != null)
        {
            int count = Mathf.Min(data.InventoryItemIds.Count, data.InventoryItemQuantities.Count);
            for (int i = 0; i < count; i++)
            {
                if (data.InventoryItemQuantities[i] > 0 && ItemDatabase.GetById(data.InventoryItemIds[i]) != null)
                    inventory[data.InventoryItemIds[i]] = data.InventoryItemQuantities[i];
            }
        }

        combatBuffActive = false;
        activeCombatBuffAmount = 0;

        // Lifetime stats - older saves simply default every new field to 0, which is correct.
        TotalLosses = Mathf.Max(0, data.TotalLosses);
        TotalDamageDealt = Mathf.Max(0, data.TotalDamageDealt);
        TotalDamageTaken = Mathf.Max(0, data.TotalDamageTaken);
        TotalCoinsEarned = Mathf.Max(0, data.TotalCoinsEarned);
        TotalCoinsSpent = Mathf.Max(0, data.TotalCoinsSpent);
        TotalItemsUsed = Mathf.Max(0, data.TotalItemsUsed);
        MaxSingleHitDamage = Mathf.Max(0, data.MaxSingleHitDamage);
        SubmissionWins = Mathf.Max(0, data.SubmissionWins);

        unlockedAchievementIds.Clear();
        if (data.UnlockedAchievementIds != null)
            foreach (var id in data.UnlockedAchievementIds) unlockedAchievementIds.Add(id);

        hallOfChampions.Clear();
        if (data.HallOfChampions != null)
            hallOfChampions.AddRange(data.HallOfChampions);

        // Milestone 29: saves from before this milestone default to false here.
        // If the fighter already has progress, treat the rival as already met
        // instead of showing a "rookie" greeting mid-career.
        hasSeenRivalIntro = data.HasSeenRivalIntro || TotalWins > 0 || completedGymIds.Count > 0 || hallOfChampions.Count > 0;

        return true;
    }

    static List<MoveData> ResolveMoveIds(List<string> ids)
    {
        var result = new List<MoveData>();
        if (ids == null) return result;

        foreach (var id in ids)
        {
            var move = MoveDatabase.GetById(id);
            if (move != null) result.Add(move);
        }
        return result;
    }
}
