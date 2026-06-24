using System.Collections.Generic;
using UnityEngine;

public enum FighterSpritePose
{
    Idle,
    Victory,
    Defeat
}

// Single point of lookup for every visual asset in the game. No other script
// should call Resources.Load for art directly - this keeps every path in one
// place, documented in Assets/Resources/Art/README.txt. Every method returns
// null if the asset isn't present yet; callers fall back to a generated
// placeholder (see IconFactory) so missing art never breaks the game.
//
// Results are cached after the first lookup (including misses, stored as
// null) so repeated calls - e.g. SetFighterPortrait running every battle
// Refresh - don't re-hit Resources.Load for the same key every time.
public static class ArtRegistry
{
    static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    public static Sprite GetFighterPortrait(string fighterId)
    {
        if (string.IsNullOrEmpty(fighterId)) return null;
        return Load(GetFighterArtOverride(fighterId) ?? $"Art/Fighters/{fighterId}");
    }

    // Milestone 35: Wrestling, BJJ, and Championship art arrived under filenames
    // that don't match those gyms' OpponentIds (e.g. wrestler_leader_1.png for
    // "wrestling_trainer_1", brazillian_jj_champ.png for "bjj_leader" - no
    // "_leader" in that one). Boxing/Muay Thai art already matches its
    // OpponentId exactly and needs no entry here. Same Load() pipeline either
    // way - this is a path override, not a second art system.
    static string GetFighterArtOverride(string fighterId)
    {
        switch (fighterId)
        {
            case "wrestling_trainer_1": return "Art/Fighters/wrestler_leader_1";
            case "wrestling_trainer_2": return "Art/Fighters/wrestler_leader_2";
            case "wrestling_trainer_3": return "Art/Fighters/wrestler_leader_3";
            case "wrestling_leader": return "Art/Fighters/wrestler_leader_champ";
            case "bjj_trainer_1": return "Art/Fighters/brazillian_jj_leader_1";
            case "bjj_trainer_2": return "Art/Fighters/brazillian_jj_leader_2";
            case "bjj_trainer_3": return "Art/Fighters/brazillian_jj_leader_3";
            case "bjj_leader": return "Art/Fighters/brazillian_jj_champ";
            // Championship progression order: trainer_1/2/3 then the leader.
            case "championship_trainer_1": return "Art/Fighters/fighter_champ_sean";
            case "championship_trainer_2": return "Art/Fighters/fighter_champ_islam";
            case "championship_trainer_3": return "Art/Fighters/fighter_champ_connor";
            case "championship_leader": return "Art/Fighters/fighter_champ_poatan";
            // Quick Fix: Rival Scratch's dedicated art - RivalDatabase.PortraitId
            // ("rival_scratch") is also GameManager.RivalFightOpponentId, so this
            // single override covers the dialogue box portrait, the Fight Night
            // intro, and the battle-stage sprite all at once.
            case RivalDatabase.PortraitId: return "Art/Fighters/fighter_rival";
            default: return null;
        }
    }

    public static Sprite GetArchetypePortrait(ArchetypeType archetype) =>
        archetype == ArchetypeType.Unspecified ? null : Load($"Art/Fighters/archetype_{archetype}");

    public static Sprite GetBattleSprite(string fighterId, ArchetypeType archetype, FighterSpritePose pose)
    {
        string poseName = pose.ToString().ToLowerInvariant();

        var exact = string.IsNullOrEmpty(fighterId) ? null : Load($"Art/Fighters/Battle/{fighterId}_{poseName}");
        if (exact != null) return exact;

        var archetypeSprite = archetype == ArchetypeType.Unspecified
            ? null
            : Load($"Art/Fighters/Battle/archetype_{archetype}_{poseName}");
        if (archetypeSprite != null) return archetypeSprite;

        if (pose != FighterSpritePose.Idle)
        {
            exact = string.IsNullOrEmpty(fighterId) ? null : Load($"Art/Fighters/Battle/{fighterId}_idle");
            if (exact != null) return exact;

            archetypeSprite = archetype == ArchetypeType.Unspecified
                ? null
                : Load($"Art/Fighters/Battle/archetype_{archetype}_idle");
            if (archetypeSprite != null) return archetypeSprite;
        }

        return null;
    }

    public static Sprite GetGymBanner(string gymId) =>
        Load(string.IsNullOrEmpty(gymId) ? null : $"Art/Gyms/{gymId}_banner");

    public static Sprite GetGymBackground(string gymId) =>
        Load(string.IsNullOrEmpty(gymId) ? null : $"Art/Backgrounds/{gymId}_background");

    public static Sprite GetGymIcon(string gymId) =>
        Load(string.IsNullOrEmpty(gymId) ? null : $"Art/Icons/{gymId}_icon");

    // Milestone 30 (icon integration): official archetype icons supplied as
    // fixed filenames (not the {id}_icon convention GetGymIcon uses, since
    // these aren't keyed off a database id list).
    public static Sprite GetArchetypeIcon(ArchetypeType archetype)
    {
        switch (archetype)
        {
            case ArchetypeType.Boxer: return Load("Art/Icons/boxer_icon");
            case ArchetypeType.Wrestler: return Load("Art/Icons/wrestler_icon");
            case ArchetypeType.MuayThaiFighter: return Load("Art/Icons/muay_thai_icon");
            case ArchetypeType.BjjSpecialist: return Load("Art/Icons/bjj_icon");
            default: return null;
        }
    }

    public static Sprite GetLogo() => Load("Art/Logos/league_logo");

    // Single-image replacement for the generated badge+title+subtitle header
    // (see UIFactory.CreateBrandHeader). Falls back to the generated header if missing.
    public static Sprite GetBanner() => Load("Art/Logos/banner");

    public static Sprite GetChampionBelt() => Load("Art/Logos/champion_belt");

    public static Sprite GetBackground(string key) =>
        Load(string.IsNullOrEmpty(key) ? null : $"Art/Backgrounds/{key}");

    public static Sprite GetAchievementIcon(string achievementId) =>
        Load(string.IsNullOrEmpty(achievementId) ? null : $"Art/Icons/achievement_{achievementId}");

    public static Sprite GetAudioIcon() => Load("Art/Icons/audio");

    public static Sprite GetItemIcon(string itemId) =>
        Load(string.IsNullOrEmpty(itemId) ? null : $"Art/Icons/item_{itemId}");

    public static Sprite GetMoveIcon(string moveId) =>
        Load(string.IsNullOrEmpty(moveId) ? null : $"Art/Icons/move_{moveId}");

    // Milestone 46: the one centralized Prestige-tattoo lookup (Part 2/6) -
    // Prestige 0, or any level beyond what's actually been uploaded, simply
    // has no matching file, so Load's existing null-on-missing-asset
    // behavior already means "no tattoo" with zero special-casing here.
    // Adding prestige_11.png etc. later needs no code change anywhere.
    public static Sprite GetPrestigeTattoo(int prestigeLevel) =>
        Load($"Art/PrestigeTattoos/prestige_{prestigeLevel}");

    // Milestone 17: the world-traveling avatar (Hub, Gym Map, Gym Entry) has its own
    // art slot, distinct from in-battle portraits/sprites, so a future artist can
    // give the avatar a dedicated look without touching battle presentation.
    public static Sprite GetPlayerAvatar() => Load("Art/Avatar/player");

    public static Sprite GetArchetypeAvatar(ArchetypeType archetype) =>
        archetype == ArchetypeType.Unspecified ? null : Load($"Art/Avatar/archetype_{archetype}");

    // Optional dedicated walk-cycle art, used only while the avatar is traveling
    // (Gym Map, Gym Entry). Falls back to the idle avatar chain when absent, so
    // supplying these is purely additive polish, never required.
    public static Sprite GetPlayerAvatarWalk() => Load("Art/Avatar/player_walk");

    public static Sprite GetArchetypeAvatarWalk(ArchetypeType archetype) =>
        archetype == ArchetypeType.Unspecified ? null : Load($"Art/Avatar/archetype_{archetype}_walk");

    static Sprite Load(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;

        if (cache.TryGetValue(path, out var cached)) return cached;

        var sprite = Resources.Load<Sprite>(path);
        cache[path] = sprite;
        return sprite;
    }
}
