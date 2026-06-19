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
    player's own portrait uses "player.png" (used on the Battle Screen and
    Fighter Profile screen). Until a real portrait exists, the fighter shows
    a generated silhouette tinted by discipline, with a small corner badge
    (see IconFactory.GetArchetypeIconShape / GetGymIconShape) indicating their
    style - a Nickname/Quote/Description also exist per opponent for flavor
    text (Data/OpponentInfo.cs) and need no art at all.

Art/Gyms/{GymId}_banner.png
    Wide banner image for a gym's roster screen, e.g. "boxing_gym_banner.png".
    GymIds are defined in Data/GymDatabase.cs (e.g. boxing_gym, muaythai_gym,
    wrestling_gym, bjj_gym, championship_gym).

Art/Icons/{GymId}_icon.png
    Small square icon representing a gym's discipline, used on the Gym Map
    and Gym Screen banner. Falls back to a generated shape per GymType
    (see IconFactory.GetGymIconShape).

Art/Logos/league_logo.png
    The Calico Combat League logo/wordmark. Falls back to the generated
    medallion badge built by UIFactory.CreateBrandHeader. The Championship
    and Hall of Champions screens use a separate, more ornate generated medal
    (UIFactory.CreateChampionBadge) for that distinct "you made it" moment -
    there is currently no override path for that one, since it's meant to
    read as a different occasion from the everyday league logo.

Each gym also carries a short Motto and History string, and each gym leader
(and most trainers) carry a Nickname, Quote, and Description - all defined in
Data/GymDatabase.cs. These are pure text/lore content, not art, and need no
file of any kind.

Art/UI/
    Reserved for general chrome (buttons, frames, panel textures) if the
    generated rounded-rectangle sprites in UIFactory are ever replaced with
    hand-drawn art.

Art/Backgrounds/{key}.png
    Full-screen background art, looked up by a simple string key (e.g. a
    screen name). None are wired in yet; ArtRegistry.GetBackground(key) is
    ready for use whenever this is needed.
