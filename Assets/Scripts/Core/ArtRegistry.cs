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

    public static Sprite GetFighterPortrait(string fighterId) =>
        Load(string.IsNullOrEmpty(fighterId) ? null : $"Art/Fighters/{fighterId}");

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

    public static Sprite GetLogo() => Load("Art/Logos/league_logo");

    // Single-image replacement for the generated badge+title+subtitle header
    // (see UIFactory.CreateBrandHeader). Falls back to the generated header if missing.
    public static Sprite GetBanner() => Load("Art/Logos/banner");

    public static Sprite GetChampionBelt() => Load("Art/Logos/champion_belt");

    public static Sprite GetBackground(string key) =>
        Load(string.IsNullOrEmpty(key) ? null : $"Art/Backgrounds/{key}");

    public static Sprite GetAchievementIcon(string achievementId) =>
        Load(string.IsNullOrEmpty(achievementId) ? null : $"Art/Icons/achievement_{achievementId}");

    public static Sprite GetItemIcon(string itemId) =>
        Load(string.IsNullOrEmpty(itemId) ? null : $"Art/Icons/item_{itemId}");

    public static Sprite GetMoveIcon(string moveId) =>
        Load(string.IsNullOrEmpty(moveId) ? null : $"Art/Icons/move_{moveId}");

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
