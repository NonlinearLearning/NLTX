# NPC server persistence boundary

Server persistence format v33 (the current format) now carries the NPC typed authority needed by
the simulation restore path: `BehaviorId`, FloatingEye movement parameters,
`Faction`, and `Category`. Older formats remain readable and use the existing
defaults for fields that did not exist in their wire layout.

The focused persistence fixture proves a FloatingEye snapshot with town
faction/category survives `DomeStatePersistenceFormat.Write` and `Read`.
The serial Server build and persistence verifier also pass against the current
current format. The verifier covers projectile identity cursor legacy defaults,
NPC typed movement/faction, moon phase/game mode, strict recovery, and atomic
replacement; it exits with code 0 and emits four `PASS` lines.

This is persistence-format evidence only. It does not imply full NPC gameplay
parity: ranged projectile ownership, complete segment movement, boss AI,
invasion integration, town services, and complete loot tables remain deferred.
