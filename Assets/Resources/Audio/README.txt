Calico Combat League - Audio Asset Pipeline
=============================================

AudioManager loads clips from this Resources/Audio folder by convention.
Missing files are safe and produce no errors. Music lookups use documented
fallbacks, while specialized SFX fall back to their closest general sound.
No code changes are needed when correctly named clips are added.

MUSIC
-----
Place music in Assets/Resources/Audio/Music using any Unity-supported audio
format. OGG is recommended for mobile builds.

Music/main_menu.ogg             Main menu and fighter creation
Music/gym.ogg                   Generic gym/map fallback
Music/boxing_gym.ogg            Boxing Gym
Music/muaythai_gym.ogg          Muay Thai Gym
Music/wrestling_gym.ogg         Wrestling Gym
Music/bjj_gym.ogg               BJJ Academy
Music/championship_gym.ogg      Championship Gym venue
Music/battle.ogg                Standard fights
Music/championship.ogg          Championship atmosphere fallback
Music/championship_battle.ogg   Championship fights
Music/victory.ogg               Victory result
Music/defeat.ogg                Defeat result
Music/champion_celebration.ogg  Champion celebration

The legacy Audio/music clip is still supported as the final fallback for all
music cues. New projects should use the Music folder names above.

Music import recommendations:
- Load Type: Streaming for full-length tracks.
- Compression Format: Vorbis, Quality 65-75 for mobile.
- Sample Rate Setting: Optimize Sample Rate.
- Keep stereo unless the source is intentionally mono.
- Author clean, seamless loop points. AudioManager loops music clips.
- Leave Normalize disabled and master consistently outside Unity.
- Recommended delivered peak: about -3 dBFS, with no clipping.

SOUND EFFECTS
-------------
Place SFX directly in Assets/Resources/Audio. WAV is recommended for short
effects; OGG is acceptable for longer ambience or celebration sounds.

click.wav             Every UI button press
hit.wav               Standard combat impact
critical_hit.wav      Critical impact; falls back to hit
victory.wav           Victory sting
defeat.wav            Defeat sting
gym_cleared.wav       Gym-clear sting; falls back to victory
champion_victory.wav  Champion sting; falls back to victory

SFX import recommendations:
- Load Type: Decompress On Load for short, frequently used effects.
- Compression: PCM/ADPCM for very short effects, Vorbis for longer stings.
- Force To Mono for centered UI and impact sounds when stereo adds no value.
- Recommended delivered peak: -3 to -1 dBFS, with no clipping.
- Keep silence tightly trimmed so impacts and button feedback feel immediate.

RUNTIME MIX
-----------
Default settings are Master 80%, Music 65%, and SFX 85%. The effective source
level is Master multiplied by its category level. Players can change all three
values from Audio Settings; values persist independently of fighter save data.

AudioManager caches each Resources lookup, uses two music sources only while
crossfading, then stops and clears the previous source. It never intentionally
plays duplicate music loops.
