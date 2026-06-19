using System.Collections.Generic;
using UnityEngine;

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

    public static Sprite GetGymBanner(string gymId) =>
        Load(string.IsNullOrEmpty(gymId) ? null : $"Art/Gyms/{gymId}_banner");

    public static Sprite GetGymBackground(string gymId) =>
        Load(string.IsNullOrEmpty(gymId) ? null : $"Art/Backgrounds/{gymId}_background");

    public static Sprite GetGymIcon(string gymId) =>
        Load(string.IsNullOrEmpty(gymId) ? null : $"Art/Icons/{gymId}_icon");

    public static Sprite GetLogo() => Load("Art/Logos/league_logo");

    public static Sprite GetChampionBelt() => Load("Art/Logos/champion_belt");

    public static Sprite GetBackground(string key) =>
        Load(string.IsNullOrEmpty(key) ? null : $"Art/Backgrounds/{key}");

    public static Sprite GetAchievementIcon(string achievementId) =>
        Load(string.IsNullOrEmpty(achievementId) ? null : $"Art/Icons/achievement_{achievementId}");

    public static Sprite GetItemIcon(string itemId) =>
        Load(string.IsNullOrEmpty(itemId) ? null : $"Art/Icons/item_{itemId}");

    public static Sprite GetMoveIcon(string moveId) =>
        Load(string.IsNullOrEmpty(moveId) ? null : $"Art/Icons/move_{moveId}");

    static Sprite Load(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;

        if (cache.TryGetValue(path, out var cached)) return cached;

        var sprite = Resources.Load<Sprite>(path);
        cache[path] = sprite;
        return sprite;
    }
}
