Calico Combat League - Audio Pipeline
=======================================

AudioManager (Assets/Scripts/Core/AudioManager.cs) loads every clip via
Resources.Load<AudioClip> by a fixed name. If a file isn't here yet,
Resources.Load returns null and the matching Play method silently no-ops -
the game runs identically with or without audio.

To add real audio, drop an imported AudioClip at the exact name below (any
Unity-supported format: wav, mp3, ogg). No code changes are required.

music.wav             Looping background music, starts automatically on boot.
click.wav             Played on every button press (wired through UIFactory).
hit.wav               Played when a move resolves in battle.
critical_hit.wav      Played on a critical hit (falls back to hit.wav if missing).
victory.wav           Played on the Victory screen.
defeat.wav            Played on the Defeat screen.
gym_cleared.wav       Played on Victory when that win clears a gym (falls back to victory.wav).
champion_victory.wav  Played on the Championship screen (falls back to victory.wav).
