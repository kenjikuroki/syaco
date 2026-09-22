# Shako Punch — playable prototype

Unity 6000.3.9f1 / URP. Mobile-oriented one-hit mantis shrimp duel prototype.

Open `Assets/Mantis/Generated/Duel.unity` and enter Play mode.

- WASD: movement; J / Space: attack; K: feint; L: parry; R: restart.
- Demo keys: 1 guard, 2 parry, 3 knockout, 4 guard break.
- Attack costs 10 guard; blocked attacks retain auto guard during return motion.
- Missed attacks expose a 0.20-second opening; parried attacks have a longer opening.
- Successful parry leads into slow motion; counterattack restores normal speed.
- CPU opponent only; online matchmaking is not implemented.

This checkpoint preserves the current combat, compact parry, arm-first recoil and short effects before the planned character redesign and six interchangeable body parts.

`Assets`, `Packages`, and `ProjectSettings` are tracked. Unity caches, local reports and builds are excluded. Windows build entry point: `DuelBuilder.Build` (requires an activated Unity Editor).

Third-party art: Matthew Guz, Hit Effects FREE, included under its original Unity Asset Store license. No relicensing of third-party assets is intended.
