# WorldGen Tunnels Boundary

**Date:** 2026-08-31  
**Flowstate:** `N6` / `in_progress_with_deferred_findings`  
**Status:** `completed_partial`

The legacy `Tunnels` pass is captured at
`Build/worldgen-oracle/legacy-instrumented-source/Terraria/WorldGen.cs:12053-12117`.
Its source guard excludes Skyblock, no-surface, and surface-is-desert worlds. It computes
`floor(width * 0.0015)` attempts, multiplies the truncated count by `1.5` in Remix, selects ten
surface points, and emits paired TileRunner calls with type `0`, `addTile=true`, strength range
`[5,8)`, step range `[6,9)`, and horizontal speeds `-2` and `2` with vertical speed `-0.3`.

The new standalone owner is
`src/Terraria.Dome.Simulation/WorldGeneration/LegacyTunnelsPass.cs`, with its immutable source
definition in `LegacyTunnelsPassDefinition.cs`. It preserves the pass-level guards, density,
candidate interval/center rules, deterministic random draw order, and source-attributed typed
commands. A small probe passed with `656` commands after a zero-warning Simulation build; a
follow-up probe also passed all three explicit deny-generation guards. The ten-year candidate
range correction is now explicit: non-Remix anniversary worlds draw from
`[floor(width*0.2), floor(width*0.8))`.
Invocation metadata now records the two pre-traversal strength/step draws, keeping diagnostics
aligned with the actual random stream.

The owner is now integrated into `WorldGenerationPipeline` for profile-backed generation, with
explicit request fields for no-surface, surface-is-desert, and tenth-anniversary rules. A pipeline
probe passed after the Simulation build. `GenVars.numTunnels/tunnelX` history is not yet published
in the state contract. Full TileRunner traversal, active sand retry parity, frame/liquid effects,
aggregate cave ordering, WLD parity, legacy deletion, and 44 ServerRelevant rows remain deferred.
