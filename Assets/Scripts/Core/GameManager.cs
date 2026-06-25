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

    // Milestone 32: presentation-only, set by BattleScreen right before EndBattle
    // is called - never saved, never affects progression, purely for the
    // Victory/Defeat screens' "Combo used" / "Turns survived" callouts.
    public int LastFightTurnCount { get; set; }
    public string LastComboUsed { get; set; }

    // Milestone 33, Part 4: presentation-only, set inside EndBattle itself
    // (right after AddXP) - the player's level if THIS fight's XP caused a
    // level-up, otherwise 0. Used only to decide whether to show a rival
    // level-milestone intercept on the Victory screen.
    public int LastVictoryLeveledUpTo { get; set; }

    // Overnight Audit (Reward/Randomness): presentation-only, same pattern as
    // the fields above - set in EndBattle's win branch, never saved. 0 unless
    // this fight's "lucky break" roll actually hit.
    public int LastLuckyBreakBonus { get; private set; }

    // Milestone 50, Part 1 (Record Celebrations): presentation-only, same
    // pattern as the fields above - set once per EndBattle call by comparing
    // this fight's per-fight counters against the lifetime peaks BEFORE they
    // get updated. False/0 on almost every fight; only true the moment a
    // record is actually broken. Never saved.
    public bool LastFightNewComboRecord { get; private set; }
    public bool LastFightNewCritRecord { get; private set; }
    public bool LastFightNewWinStreakRecord { get; private set; }
    public int LastStreetFightMilestone { get; private set; }

    // Milestone 56, Part 6 (Career Highlight Celebrations): same transient,
    // never-saved pattern as the fields above. LastStreetFightMilestone
    // (Milestone 50) already doubles as the "hit 50 Street Fight Wins"
    // signal, so only these two are new.
    public bool LastFightFirstGymCleared { get; private set; }
    public bool LastFightHit100Combos { get; private set; }

    // ---------- Lifetime stats (persist across StartNewGame/StartFreshGame - see Milestone 11) ----------

    public int TotalWins { get; private set; }
    public int TotalLosses { get; private set; }
    public int TotalBattles => TotalWins + TotalLosses;

    // Milestone 36: consecutive-win streak for the Tale of the Tape's "Win
    // Streak" readout - incremented on a win, reset on a loss, in EndBattle.
    public int CurrentWinStreak { get; private set; }

    // Milestone 47 (Career Records): the running peak of CurrentWinStreak -
    // never decreases, so it survives the loss that resets CurrentWinStreak.
    public int BestWinStreak { get; private set; }

    // Milestone 45 (Prestige / New Game+) - Part 9's "local score foundation."
    public int PrestigeLevel { get; private set; }
    public int TotalGameCompletions { get; private set; }
    public int HighestPrestigeReached { get; private set; }

    // Part 1: Mirror Match defeated is the official "Game Completed" condition.
    public bool IsGameCompleted => HasDefeatedShadowChampion;
    // Part 2: capped via the one centralized constant - never compare against
    // a literal 10 anywhere else.
    public bool CanPrestige => IsGameCompleted && PrestigeLevel < PrestigeSystem.MaxPrestigeLevel;

    public int TotalDamageDealt { get; private set; }
    public int TotalDamageTaken { get; private set; }
    public int TotalCoinsEarned { get; private set; }
    public int TotalCoinsSpent { get; private set; }
    public int TotalItemsUsed { get; private set; }
    public int MaxSingleHitDamage { get; private set; }
    public int SubmissionWins { get; private set; }

    // Milestone 49 (Combat Record Book): folded in from BattleSystem's
    // per-fight PLAYER-only counters at the end of every win/loss in
    // EndBattle - see the "Milestone 49" block there for exactly how.
    public int TotalCriticalHits { get; private set; }
    public int MostCriticalHitsInOneFight { get; private set; }
    public int TotalCombosTriggered { get; private set; }
    public int MostCombosInOneFight { get; private set; }
    public int TotalParries { get; private set; }
    public int SuccessfulParries { get; private set; }
    public int TotalClinches { get; private set; }
    public int TotalTakedownsLanded { get; private set; }
    public int StreetFightWins { get; private set; }
    // "Win with 1 HP" is a one-time accomplishment, not a counter - same
    // boolean-flag shape as HasBecomeChampion/HasDefeatedRival elsewhere.
    public bool HasWonWithOneHP { get; private set; }

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

    // Milestone 49, Part 1/2: "Rival Wins" and "Total Championships Won" are
    // both fully derivable from existing Hall of Champions entries - no new
    // save fields needed. RecordChampionLegacy adds an untitled entry per
    // championship; RecordRivalVictoryLegacy always titles its entry "Rival
    // Conqueror" - both fire once per genuinely new completion (per Prestige
    // cycle), so counting entries already gives an accurate lifetime total.
    public int RivalWinCount => CountHallOfChampionsTitled("Rival Conqueror");
    public int ChampionshipWinCount => CountHallOfChampionsTitled("");
    // Milestone 44 named this "True Champion"; pre-Milestone-44 saves may
    // still hold the old "Shadow Slayer" title - both count as Mirror Match
    // wins, same backward-compat reasoning used everywhere else this title
    // is checked.
    public int MirrorMatchWinCount => TotalGameCompletions;

    int CountHallOfChampionsTitled(string title)
    {
        int count = 0;
        foreach (var record in hallOfChampions)
            if ((record.Title ?? "") == title) count++;
        return count;
    }

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

    // Bug fix (World Polish Pass): this used to delete the save and wipe all
    // progress immediately on clicking "NEW GAME" - before the player had
    // even named their fighter or picked an archetype. Backing out of Fighter
    // Creation (or force-quitting) left the old save unrecoverably gone with
    // no new one created. Now this is just a navigation transition; the
    // actual destructive reset happens in StartNewGame, only once the player
    // confirms with BEGIN CAREER.
    public void StartFreshGame()
    {
        ChangeState(GameState.FighterCreation);
    }

    public void StartNewGame(string fighterName, ArchetypeType archetype)
    {
        string name = string.IsNullOrWhiteSpace(fighterName) ? "Fighter" : fighterName.Trim();
        SaveSystem.DeleteSave();
        defeatedOpponentIds.Clear();
        completedGymIds.Clear();
        inventory.Clear();
        combatBuffActive = false;
        activeCombatBuffAmount = 0;
        hasSeenRivalIntro = false;
        // Milestone 46: a true New Game (distinct from Prestige, which
        // deliberately keeps these) starts at Prestige 0 - without this, a
        // returning player's leftover in-memory PrestigeLevel from a
        // previous career could otherwise make Character Creation's tattoo
        // preview show a tattoo on a fighter who hasn't earned anything yet.
        PrestigeLevel = 0;
        TotalGameCompletions = 0;
        HighestPrestigeReached = 0;
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

    // Milestone 39: the Rival Showdown moved from gating the Championship Gym
    // to being the true final test AFTER it (see IsRivalFightReady below) - the
    // Championship Gym now unlocks on its normal prerequisite alone again,
    // same as every other gym.
    public bool IsGymUnlocked(GymInfo gym)
    {
        if (gym == null) return false;
        bool prerequisiteMet = string.IsNullOrEmpty(gym.RequiredGymId) || completedGymIds.Contains(gym.RequiredGymId);
        return prerequisiteMet;
    }

    // Milestone 39 (was Milestone 34's pre-Championship gate): true once the
    // player has actually become champion and hasn't yet beaten Scratch - the
    // window where GymSelectionScreen shows the Rival Showdown banner. This is
    // now the league's real final test, not a gatekeeper before one.
    public bool IsRivalFightReady()
    {
        return HasBecomeChampion() && !HasDefeatedRival;
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

        // Milestone 45, Part 5: the one shared chokepoint every opponent's
        // stats pass through before a fight starts - PrestigeSystem.ApplyScaling
        // is a no-op at PrestigeLevel 0, so this has zero effect until the
        // player actually prestiges. Stacks with the rematch boost above
        // (orthogonal concepts - "already beat this leader" vs "Nth full
        // playthrough" - rather than one superseding the other).
        PrestigeSystem.ApplyScaling(opponentStats, PrestigeLevel);

        CurrentOpponent = new FighterData(opponent.Name, opponentStats, opponent.Moves)
            { IsSmartFighter = opponent.IsSmartFighter, DefenseBiasPercent = opponent.DefenseBiasPercent, Personality = opponent.Personality };
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

        // Milestone 50, Part 1 (Record Celebrations): reset every call, and
        // capture the peaks BEFORE folding this fight's counters in below, so
        // the comparisons after the fold-in can tell whether a record was
        // ACTUALLY just broken (vs. merely tied or already held).
        LastFightNewComboRecord = false;
        LastFightNewCritRecord = false;
        LastFightNewWinStreakRecord = false;
        LastStreetFightMilestone = 0;
        LastFightFirstGymCleared = false;
        LastFightHit100Combos = false;
        int comboRecordBefore = MostCombosInOneFight;
        int critRecordBefore = MostCriticalHitsInOneFight;
        int winStreakRecordBefore = BestWinStreak;
        // Milestone 56, Part 6: "before" snapshot for the 100-combos Career
        // Highlight, same before/after comparison pattern as the records above.
        int totalCombosBefore = TotalCombosTriggered;

        // Milestone 49 (Combat Record Book): fold this fight's PLAYER-only
        // counters from BattleSystem into lifetime totals/peaks BEFORE the
        // win/loss split below - combos/crits/parries/clinches can all
        // happen in a fight the player ultimately loses, and "Total X"
        // stats should count every fight, not just wins. Cleanup() only
        // reverts status effects, never touches these counters, but
        // CurrentBattle is about to go out of scope after this method, so
        // this is the one and only place to capture them.
        if (CurrentBattle != null)
        {
            TotalCriticalHits += CurrentBattle.PlayerCriticalHits;
            if (CurrentBattle.PlayerCriticalHits > MostCriticalHitsInOneFight) MostCriticalHitsInOneFight = CurrentBattle.PlayerCriticalHits;
            TotalCombosTriggered += CurrentBattle.PlayerCombosTriggered;
            if (CurrentBattle.PlayerCombosTriggered > MostCombosInOneFight) MostCombosInOneFight = CurrentBattle.PlayerCombosTriggered;
            TotalParries += CurrentBattle.PlayerParriesAttempted;
            SuccessfulParries += CurrentBattle.PlayerParriesSucceeded;
            TotalClinches += CurrentBattle.PlayerClinches;
            TotalTakedownsLanded += CurrentBattle.PlayerTakedownsLanded;

            // Only a genuine break (not a fresh-from-zero tie) counts as a
            // celebration-worthy record - both sides of this comparison are
            // already updated above, so a fight with 0 combos can never
            // falsely claim a "record" of 0.
            LastFightNewComboRecord = MostCombosInOneFight > comboRecordBefore;
            LastFightNewCritRecord = MostCriticalHitsInOneFight > critRecordBefore;
            // Milestone 56, Part 6: a lifetime-total threshold crossing -
            // distinct from MostCombosInOneFight's per-fight peak, so this
            // can fire independently of (and won't double up with) the
            // Milestone 50 combo-record celebration above.
            LastFightHit100Combos = totalCombosBefore < 100 && TotalCombosTriggered >= 100;
        }
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
            int levelBeforeReward = Player.Stats.Level;
            Player.Stats.AddXP(LastRewardXP);
            // Milestone 33, Part 4: 0 unless THIS fight's XP actually crossed a
            // level boundary - must be computed here (right after AddXP, before
            // ChangeState fires VictoryScreen.Refresh synchronously) rather than
            // by the caller, which would still be reading the pre-reward level.
            LastVictoryLeveledUpTo = Player.Stats.Level > levelBeforeReward ? Player.Stats.Level : 0;
            Player.Stats.Coins += LastRewardCoins;
            TotalCoinsEarned += LastRewardCoins;

            // Overnight Audit (Reward/Randomness): a small, low-risk "lucky
            // break" bonus, Street Fight wins only (per the brief's own
            // example) - a 12% chance at a modest flat coin bonus on top of
            // the normal reward. Purely additive flavor, no new systems.
            LastLuckyBreakBonus = 0;
            if (CurrentGym?.GymId == "street_fight" && UnityEngine.Random.Range(0, 100) < 12)
            {
                LastLuckyBreakBonus = UnityEngine.Random.Range(15, 31);
                Player.Stats.Coins += LastLuckyBreakBonus;
                TotalCoinsEarned += LastLuckyBreakBonus;
            }

            TotalWins++;
            CurrentWinStreak++;
            if (CurrentWinStreak > BestWinStreak) BestWinStreak = CurrentWinStreak;
            LastFightNewWinStreakRecord = BestWinStreak > winStreakRecordBefore;
            if (submissionFinish) SubmissionWins++;
            if (Player.Stats.CurrentHealth == 1) HasWonWithOneHP = true;
            if (CurrentGym?.GymId == "street_fight")
            {
                StreetFightWins++;
                if (StreetFightWins == 10 || StreetFightWins == 25 || StreetFightWins == 50 || StreetFightWins == 100)
                    LastStreetFightMilestone = StreetFightWins;
            }
            // Bug fix (World Polish Pass): must be captured before adding to
            // defeatedOpponentIds below, or HasDefeatedShadowChampion would
            // always read true by the time RecordShadowChampionVictory's guard
            // checks it - silently allowing a duplicate Hall of Champions entry
            // on every repeat win, not just the first.
            bool alreadyDefeatedShadowChampion = HasDefeatedShadowChampion;
            // Milestone 39, Part 9: same before-the-Add capture, so a rival
            // rematch (the fight stays available to replay afterward) doesn't
            // add a duplicate Hall of Champions entry.
            bool alreadyDefeatedRival = HasDefeatedRival;
            defeatedOpponentIds.Add(CurrentOpponentInfo.OpponentId);

            bool becameChampionJustNow = false;
            if (CurrentGym?.Leader != null && CurrentGym.Leader.OpponentId == CurrentOpponentInfo.OpponentId)
            {
                bool wasAlreadyCompleted = completedGymIds.Contains(CurrentGym.GymId);
                completedGymIds.Add(CurrentGym.GymId);
                LastVictoryUnlockedGym = !wasAlreadyCompleted;
                // Milestone 56, Part 6: "First Gym Cleared" is meant as a
                // true once-ever Career Highlight, not a per-Prestige-cycle
                // one - gated on PrestigeLevel == 0 so it doesn't re-fire the
                // next time the player clears the first gym of a new league.
                if (LastVictoryUnlockedGym && GymDatabase.AllGyms.Count > 0 &&
                    CurrentGym.GymId == GymDatabase.AllGyms[0].GymId && PrestigeLevel == 0)
                    LastFightFirstGymCleared = true;

                TryUnlockGymMove(CurrentGym);

                if (!wasAlreadyCompleted && IsLastGym(CurrentGym))
                    becameChampionJustNow = true;
            }

            if (becameChampionJustNow) RecordChampionLegacy();
            if (CurrentOpponentInfo.OpponentId == ShadowChampionId && !alreadyDefeatedShadowChampion) RecordShadowChampionVictory();
            if (CurrentOpponentInfo.OpponentId == RivalFightOpponentId && !alreadyDefeatedRival) RecordRivalVictoryLegacy();

            CheckAchievements();
            SaveGame();
            ChangeState(becameChampionJustNow ? GameState.Championship : GameState.Victory);
        }
        else
        {
            // Milestone 32: a loss grants no reward, but LastRewardXP/Coins were
            // only ever assigned in the win branch above - without this they'd
            // still hold whatever the player's most recent WIN was, and the new
            // Defeat screen would show that stale number as if it were earned
            // from this fight.
            LastRewardXP = 0;
            LastRewardCoins = 0;
            LastVictoryLeveledUpTo = 0;
            TotalLosses++;
            CurrentWinStreak = 0;
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
    //
    // Milestone 44 (Mirror Match): repurposed in place rather than duplicated -
    // the OpponentId/GymId constants below are UNCHANGED from the original
    // Shadow Champion feature specifically so saves where a player already
    // has "shadow_champion" in defeatedOpponentIds keep reading as defeated.
    // Only the unlock gate, stat modifiers, dialogue, and presentation names
    // changed; the underlying ids did not.

    public const string ShadowChampionId = "shadow_champion";

    public bool HasDefeatedShadowChampion => defeatedOpponentIds.Contains(ShadowChampionId);

    // Milestone 44, Unlock Flow: now requires defeating Rival Scratch too,
    // not just becoming champion - "the true final test" comes after both.
    public void StartShadowChampionBattle()
    {
        if (Player == null || !HasBecomeChampion() || !HasDefeatedRival) return;

        var mirroredStats = Player.Stats.Clone();
        // Milestone 44, Mirror Opponent: "+10% HP, +10% Stamina, +5% combat
        // stats" per the brief - was +10% HP/Stamina/Speed-only before.
        mirroredStats.MaxHealth = Mathf.RoundToInt(mirroredStats.MaxHealth * 1.1f);
        mirroredStats.MaxStamina = Mathf.RoundToInt(mirroredStats.MaxStamina * 1.1f);
        mirroredStats.Strength = Mathf.RoundToInt(mirroredStats.Strength * 1.05f);
        mirroredStats.Defense = Mathf.RoundToInt(mirroredStats.Defense * 1.05f);
        mirroredStats.Speed = Mathf.RoundToInt(mirroredStats.Speed * 1.05f);
        mirroredStats.Striking = Mathf.RoundToInt(mirroredStats.Striking * 1.05f);
        mirroredStats.Grappling = Mathf.RoundToInt(mirroredStats.Grappling * 1.05f);
        mirroredStats.Submission = Mathf.RoundToInt(mirroredStats.Submission * 1.05f);
        mirroredStats.ResetForBattle();

        var shadowOpponent = new OpponentInfo
        {
            OpponentId = ShadowChampionId,
            Name = $"Shadow {Player.Name}",
            Stats = mirroredStats,
            // Milestone 44: same equipped moves as the player (unchanged) -
            // and now opts into the existing smart-fighter AI (combo-seeking,
            // stamina-efficient) the same way Rival Scratch already does, per
            // "use smart fighter behavior if available."
            Moves = new List<MoveData>(Player.EquippedMoves),
            RewardXP = 300,
            RewardCoins = 150,
            Nickname = "Your Reflection",
            // Milestone 44, Dialogue: pre-fight Bio/Quote shown as the existing
            // two tap-through beats every Fight Night intro already has;
            // Victory/Defeat use the existing LossLine/WinLine quote display.
            Quote = "Every mistake. Every habit. Every shortcut. I know them all.",
            Bio = "You beat everyone else.\nNow beat the fighter who got you here.",
            LossLine = "You are not the same fighter who started.\nThat is why you won.",
            WinLine = "You already know how to beat me.\nTry again.",
            IsSmartFighter = true,
            // Milestone 62, Part 3: Adaptive - per the brief, "use the
            // player's move set exactly as before," so this carries zero
            // behavioral bias (FighterPersonalityTraits.Get returns all
            // zeros for it) and exists purely as a presentation label.
            Personality = FighterPersonality.Adaptive
        };

        // GymType.Championship alone gives the fight the strongest existing
        // presentation tier; leaving Leader/Trainers null means EndBattle's
        // normal gym-completion bookkeeping is a no-op for this synthetic gym.
        CurrentGym = new GymInfo
        {
            GymId = "shadow_gym",
            GymName = "The Mirror Match",
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
            // Milestone 44: "Shadow Slayer" -> "True Champion" per the
            // brief's reward language. Existing saves that already earned
            // "Shadow Slayer" keep that historical record unchanged - only
            // new victories from here on get the new title.
            Title = "True Champion"
        });

        // Milestone 45, Part 9: this only runs once per genuinely new
        // completion - EndBattle's alreadyDefeatedShadowChampion guard
        // already prevents this whole method from re-firing on a repeat win
        // within the same Prestige cycle, so this counter can't double-count.
        TotalGameCompletions++;
    }

    // Milestone 45, Part 3/4: the Prestige action itself - confirmation is
    // owned by the calling UI (ProfileScreen); this method assumes the
    // player has already confirmed and just performs the reset/increment.
    public void PerformPrestige()
    {
        if (!CanPrestige) return;

        PrestigeLevel = Mathf.Min(PrestigeLevel + 1, PrestigeSystem.MaxPrestigeLevel);
        HighestPrestigeReached = Mathf.Max(HighestPrestigeReached, PrestigeLevel);

        // Part 4 - RESET: gym progress, defeated opponents (which is also
        // what HasBecomeChampion/HasDefeatedRival/HasDefeatedShadowChampion/
        // HasDefeatedSecretFighter all read from, so clearing this one set
        // already resets Championship/Rival/Mirror Match progress with no
        // separate flags to touch), and the current run's rival-intro flag
        // so the narrative beats play again from the top.
        completedGymIds.Clear();
        defeatedOpponentIds.Clear();
        hasSeenRivalIntro = false;

        // Part 4 - KEEP (everything NOT touched above): Level/XP/StatPoints/
        // Stats/Coins/Archetype (Player.Stats itself is never reset here),
        // KnownMoves/EquippedMoves, inventory, unlockedAchievementIds,
        // hallOfChampions, every lifetime statistic, and PrestigeLevel.

        // Part 7: a permanent record of the cycle just completed, same
        // existing ChampionRecord/Title infrastructure as Shadow Slayer/
        // Rival Conqueror/True Champion above - added once per Prestige
        // action, never spammed (this method only runs on an explicit,
        // confirmed player action).
        hallOfChampions.Add(new ChampionRecord
        {
            FighterName = Player.Name,
            Archetype = Player.Archetype.ToString(),
            FinalLevel = Player.Stats.Level,
            TotalWinsAtCompletion = TotalWins,
            CompletionDate = DateTime.Now.ToString("yyyy-MM-dd"),
            Title = $"Completed {PrestigeSystem.FormatLevel(PrestigeLevel)}"
        });

        // Milestone 49: Legend/Immortal key off HighestPrestigeReached, which
        // just changed above - check immediately so they unlock the moment
        // the player prestiges, not on whatever fight happens to come next.
        CheckAchievements();
        SaveGame();
        // Milestone 57, Part 1/7: no longer transitions to GymMap directly -
        // ProfileScreen now shows a short reveal moment first and changes
        // state itself once that reveal closes. All actual Prestige data
        // (level, reset, Hall of Champions entry, save) is already fully
        // committed above by this point regardless of how long the reveal
        // takes or whether the player taps through it early.
    }

    // Milestone 39, Part 9: the Rival Showdown's own permanent legacy entry -
    // same existing ChampionRecord/Title infrastructure as Shadow Slayer
    // above, no new fields or systems.
    void RecordRivalVictoryLegacy()
    {
        hallOfChampions.Add(new ChampionRecord
        {
            FighterName = Player.Name,
            Archetype = Player.Archetype.ToString(),
            FinalLevel = Player.Stats.Level,
            TotalWinsAtCompletion = TotalWins,
            CompletionDate = DateTime.Now.ToString("yyyy-MM-dd"),
            Title = "Rival Conqueror"
        });
    }

    // ---------- Rival Showdown (Milestone 34) ----------
    // The payoff to the Rival Ascension storyline (Milestone 33): a single
    // fixed fight against Rival Scratch, gating the Championship Gym (see
    // IsGymUnlocked above). Built the same way every other special fight in
    // this game is - a fixed OpponentInfo run through the existing StartBattle
    // pipeline. IsSmartFighter opts into BattleSystem's smarter move-choice
    // path (Part 5) without changing any other fighter's behavior.

    // Matches RivalDatabase.PortraitId exactly (not a separate "_fight" id) so
    // the battle-stage portrait lookup (keyed off OpponentId) and the rival
    // dialogue box's portrait lookup (keyed off PortraitId) both resolve to
    // the same art slot if dedicated Scratch artwork is ever added.
    public const string RivalFightOpponentId = RivalDatabase.PortraitId;

    public bool HasDefeatedRival => defeatedOpponentIds.Contains(RivalFightOpponentId);

    // Quick Fix (Secret Fighter): reuses the same defeatedOpponentIds HashSet
    // as HasDefeatedRival/HasDefeatedShadowChampion - no new save field.
    public bool HasDefeatedSecretFighter => defeatedOpponentIds.Contains(StreetFightGenerator.SecretFighterOpponentId);

    public void StartRivalFight()
    {
        if (Player == null) return;

        // Milestone 39: the rival is now the league's true final test, fought
        // after the Championship - stats/moves bumped to exceed even Champion
        // Volkov's (260/95/23/22/21/25/25/25) across the board, so this reads
        // as the strongest opponent in the game rather than a mid-game gate.
        // Moves mix in his strongest non-boxing options (still Boxer-themed:
        // Jab/Cross/Hook preserve the One-Two Finish combo) for a complete,
        // well-rounded final-boss kit instead of the old boxing-only set.
        var rivalOpponent = new OpponentInfo
        {
            OpponentId = RivalFightOpponentId,
            Name = RivalDatabase.RivalName,
            Stats = new FighterStats
            {
                MaxHealth = 270,
                CurrentHealth = 270,
                MaxStamina = 100,
                CurrentStamina = 100,
                Strength = 24,
                Defense = 23,
                Speed = 24,
                Striking = 27,
                Grappling = 20,
                Submission = 18
            },
            Moves = new List<MoveData> { MoveDatabase.Jab, MoveDatabase.Cross, MoveDatabase.Hook, MoveDatabase.SpinningBackKick, MoveDatabase.RearNakedChoke },
            RewardXP = 320,
            RewardCoins = 160,
            Nickname = RivalDatabase.GetShowdownNickname(),
            Quote = "Talent doesn't ask permission. Lucky for you, I brought enough for both of us.",
            Description = "The fighter the whole league has been comparing you to since day one.",
            Bio = "Same league, same gyms, same dream - except he's been one step ahead the whole time. Tonight, that ends.",
            LossLine = "Huh. Guess you earned it.",
            WinLine = "Still not there. Come back when you're ready.",
            IsSmartFighter = true,
            // Milestone 62, Part 3: "Veteran + Calculated" - Calculated is the
            // mechanical trait (efficient, leans defensive, no wasted
            // stamina); the veteran experience is already conveyed through
            // his stats/dialogue, so it doesn't need a second mechanical knob.
            Personality = FighterPersonality.Calculated
        };

        // A dedicated marker distinct from GymType.Championship - the Rival
        // Showdown gets its own presentation tier (Part 3) in BattleScreen, not
        // the Championship fight's aura/tint. A null Leader means EndBattle's
        // gym-completion bookkeeping is a no-op, same as every other
        // synthetic-gym fight (Street Fight, Shadow Champion).
        CurrentGym = new GymInfo
        {
            GymId = "rival_fight",
            GymName = "Rival Showdown",
            GymType = GymType.Boxing
        };

        StartBattle(rivalOpponent);
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
            case AchievementMetric.DefeatedRival: return HasDefeatedRival ? 1 : 0;
            case AchievementMetric.DefeatedSecretFighter: return HasDefeatedSecretFighter ? 1 : 0;
            case AchievementMetric.DefeatedMirrorMatch: return HasDefeatedShadowChampion ? 1 : 0;
            case AchievementMetric.CombosTriggered: return TotalCombosTriggered;
            case AchievementMetric.Clinches: return TotalClinches;
            case AchievementMetric.Parries: return TotalParries;
            case AchievementMetric.StreetFightWins: return StreetFightWins;
            case AchievementMetric.HighestPrestigeReached: return HighestPrestigeReached;
            case AchievementMetric.WonWithOneHP: return HasWonWithOneHP ? 1 : 0;
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
            HasSeenRivalIntro = hasSeenRivalIntro,
            CurrentWinStreak = CurrentWinStreak,
            BestWinStreak = BestWinStreak,
            PrestigeLevel = PrestigeLevel,
            TotalGameCompletions = TotalGameCompletions,
            HighestPrestigeReached = HighestPrestigeReached,
            TotalCriticalHits = TotalCriticalHits,
            MostCriticalHitsInOneFight = MostCriticalHitsInOneFight,
            TotalCombosTriggered = TotalCombosTriggered,
            MostCombosInOneFight = MostCombosInOneFight,
            TotalParries = TotalParries,
            SuccessfulParries = SuccessfulParries,
            TotalClinches = TotalClinches,
            TotalTakedownsLanded = TotalTakedownsLanded,
            StreetFightWins = StreetFightWins,
            HasWonWithOneHP = HasWonWithOneHP
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
        CurrentWinStreak = Mathf.Max(0, data.CurrentWinStreak);
        // Older saves predate this field (default 0) - never let it read
        // lower than the streak already in progress.
        BestWinStreak = Mathf.Max(Mathf.Max(0, data.BestWinStreak), CurrentWinStreak);
        PrestigeLevel = Mathf.Clamp(data.PrestigeLevel, 0, PrestigeSystem.MaxPrestigeLevel);
        TotalGameCompletions = Mathf.Max(0, data.TotalGameCompletions);
        HighestPrestigeReached = Mathf.Max(PrestigeLevel, Mathf.Max(0, data.HighestPrestigeReached));

        // Milestone 49 (Combat Record Book) - missing on older saves -> 0/false.
        TotalCriticalHits = Mathf.Max(0, data.TotalCriticalHits);
        MostCriticalHitsInOneFight = Mathf.Max(0, data.MostCriticalHitsInOneFight);
        TotalCombosTriggered = Mathf.Max(0, data.TotalCombosTriggered);
        MostCombosInOneFight = Mathf.Max(0, data.MostCombosInOneFight);
        TotalParries = Mathf.Max(0, data.TotalParries);
        SuccessfulParries = Mathf.Max(0, data.SuccessfulParries);
        TotalClinches = Mathf.Max(0, data.TotalClinches);
        TotalTakedownsLanded = Mathf.Max(0, data.TotalTakedownsLanded);
        StreetFightWins = Mathf.Max(0, data.StreetFightWins);
        HasWonWithOneHP = data.HasWonWithOneHP;

        unlockedAchievementIds.Clear();
        if (data.UnlockedAchievementIds != null)
            foreach (var id in data.UnlockedAchievementIds) unlockedAchievementIds.Add(id);

        hallOfChampions.Clear();
        if (data.HallOfChampions != null)
            hallOfChampions.AddRange(data.HallOfChampions);

        // Milestone 29: saves from before this milestone default to false here.
        // If the fighter already has progress, treat the rival as already met
        // instead of showing a "rookie" greeting mid-career.
        // Milestone 45: scoped to PrestigeLevel == 0 - this migration heuristic
        // only ever applied to pre-Prestige saves (PrestigeLevel didn't exist
        // before this milestone, so any such save is necessarily at 0).
        // Without this, hallOfChampions.Count > 0 (always true after a
        // Prestige cycle, since every cycle adds an entry) would silently
        // override the explicit "false" PerformPrestige just set, defeating
        // its own "see the rival's intro again in the new league" intent.
        hasSeenRivalIntro = data.HasSeenRivalIntro ||
            (PrestigeLevel == 0 && (TotalWins > 0 || completedGymIds.Count > 0 || hallOfChampions.Count > 0));

        // Milestone 39: the old Milestone 34 grandfather-in here assumed
        // becoming champion implied the rival was already beaten (true only
        // under the old pre-Championship gating it was patching for). Under
        // the new post-Championship ordering that assumption is backwards, so
        // it's removed - a save that's already champion but never fought
        // Scratch (including saves from before the rival fight existed at
        // all) now correctly sees the new Rival Showdown banner instead of
        // being silently credited with a fight that never happened. Nothing
        // already unlocked (Championship Gym, Shadow Champion, Hall of
        // Champions entries) is affected, since none of those depend on
        // HasDefeatedRival anymore.
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
