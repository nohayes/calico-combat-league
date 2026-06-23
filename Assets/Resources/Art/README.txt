Calico Combat League - Art Pipeline
====================================

All visual assets are looked up through Core/ArtRegistry.cs via Resources.Load.
No other script hardcodes an asset path. If a file below is missing, the
matching ArtRegistry method returns null and the calling UI code falls back
to a generated placeholder (see UI/IconFactory.cs) - the game never breaks
because art is missing.

To add real art, drop a Sprite-imported image at the exact path below. No
code changes are required; the next time that screen refreshes it will pick
up the new sprite automatically.

Art/Fighters/{OpponentId}.png
    Portrait for a specific opponent, e.g. "boxing_trainer_1.png",
    "boxing_leader.png". OpponentIds are defined in Data/GymDatabase.cs. The
    player's own portrait uses "player.png" (used on the Battle, Profile, and
    Championship screens). Gym roster rows also use these exact opponent IDs.
    Until real portraits exist, fighters show cached discipline-specific
    silhouettes tinted by archetype/gym, with a small corner badge
    (see IconFactory.GetArchetypeIconShape / GetGymIconShape) indicating their
    style - a Nickname/Quote/Description also exist per opponent for flavor
    text (Data/OpponentInfo.cs) and need no art at all.

Art/Fighters/Battle/{id}_{pose}.png
    Full-body 1024x1536 battle sprites for idle, victory, and defeat poses.
    IDs use player, an exact OpponentId, or archetype_{ArchetypeType}. Author
    sprites facing right; opponent art is mirrored in the UI. Requested pose
    art falls back through archetype/idle sprites, portraits, and finally the
    generated silhouette, so the arena never displays a broken fighter.

Art/Gyms/{GymId}_banner.png
    Wide banner image for a gym's roster screen, e.g. "boxing_gym_banner.png".
    GymIds are defined in Data/GymDatabase.cs (e.g. boxing_gym, muaythai_gym,
    wrestling_gym, bjj_gym, championship_gym).

Art/Icons/{GymId}_icon.png
    Small square gym logo/crest representing its discipline, used on the Gym
    Map and Gym Screen banner. Falls back to a generated shape per GymType
    (see IconFactory.GetGymIconShape).

Art/Logos/league_logo.png
    The Calico Combat League logo/wordmark. Falls back to the generated
    medallion badge built by UIFactory.CreateBrandHeader.

Art/Logos/champion_belt.png
    Championship branding used by UIFactory.CreateChampionBadge. Falls back
    to the generated gold medal and ribbon treatment.

Each gym also carries a short Motto and History string, and each gym leader
(and most trainers) carry a Nickname, Quote, and Description - all defined in
Data/GymDatabase.cs. These are pure text/lore content, not art, and need no
file of any kind.

Art/UI/
    Reserved for general chrome (buttons, frames, panel textures) if the
    generated rounded-rectangle sprites in UIFactory are ever replaced with
    hand-drawn art.

Art/Avatar/player.png
Art/Avatar/archetype_{ArchetypeType}.png
    The world-traveling avatar shown outside of battle (Hub, Gym Map, Gym Entry -
    Milestone 17). Separate from Fighters/ so the avatar can get its own look
    without touching in-battle portraits or sprites. Priority: player avatar ->
    archetype avatar -> existing portrait chain (Art/Fighters/...) -> generated
    silhouette. The Hub and Gym Map screens reuse the existing main_menu/gym_map
    Background keys above for their own backdrops - no separate "Hub Art" or
    "Map Art" asset path is needed for that.

Art/Backgrounds/{key}.png
    Full-screen background art, looked up by a simple string key (e.g. a
    screen name). Wired keys include main_menu, gym_map, battle, victory,
    defeat, championship, and {GymId}_background for each gym roster. A dark
    readability tint is generated above every supplied background.

Art/Icons/achievement_{AchievementId}.png
Art/Icons/item_{ItemId}.png
Art/Icons/move_{MoveId}.png
    Data-driven square icons for achievements, shop items, and moves. Exact
    IDs come from their corresponding static databases. Generated category or
    discipline shapes are used when an icon is absent.

PERFORMANCE
    ArtRegistry caches every Resources lookup, including missing assets, so
    repeated screen refreshes do not reload files. IconFactory also caches all
    generated icons and archetype silhouettes. Keep source textures near the
    documented display sizes and disable mip maps for UI-only sprites.
