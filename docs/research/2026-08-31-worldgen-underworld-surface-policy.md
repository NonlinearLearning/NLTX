# WorldGen Underworld surface policy boundary

The Underworld source pass begins with a surface-column top at
`maxTilesY - Next(150, 190)`, advances it per column with `Next(-3, 4)`, and
clamps the top to `[maxTilesY - 190, maxTilesY - 160]`. The
`notTheBeesAndForTheWorthyNoCelebration` branch uses an effective top 30 tiles
higher for its wall/spider-cave region. The later lava shelf writes use the
fixed rows `maxTilesY - 145` and `maxTilesY - 144` when the tile is empty.

The typed policy and command owners are:

- `LegacyUnderworldSurfaceColumnPolicy`
- `LegacyUnderworldLavaShelfPolicy`
- `LegacyUnderworldSurfacePass`
- `LegacyUnderworldLavaColumnPass`
- `LegacyUnderworldLiquidCaveScheduler`
- `LegacyUnderworldLiquidCavePass`
- `LegacyUnderworldLowerScheduler`
- `LegacyUnderworldLowerPass`
- `LegacyUnderworldLavaShelfPass`
- `LegacyQuickWaterScanPolicy`

The focused boundary probe verifies the initialization range, deterministic
advance/clamp behavior, the NotTheBees offset, and both shelf coordinates.
It also verifies typed ash/clear and lava command emission, liquid commit, and
the Skyblock guard. The pipeline invokes these normal-world surface and lava
phases after Webs. Secret-seed state
is not currently present in `WorldGenerationRequest`, so the integrated path
does not enable the NotTheBees branch. The scheduler boundary also composes the
source-order `Next(13)` liquid-cave origins with surface, paired-surface, and
evil runner factories. `LegacyUnderworldLiquidCavePass` now projects those
requests through shared-RNG TileRunner traversal and typed tile commit.
Ash/evil/obsidian liquid side effects, quick-water ordering, remaining
Underworld mutation, and WLD parity remain deferred. The two-row lava shelf is
now emitted as typed lava liquid commands and committed after the lower pass;
`LegacyQuickWaterScanPolicy` now captures the source scan window (`Y=3..height-3`,
descending; `X=4..width-5`) and is focused-verified. The in-place
`SettleWaterAt` mutation semantics and `Liquid.QuickWater(-2)` commit ordering
remain deferred.
The lower scheduler boundary is also verified: it emits one lower-evil request
per column, optional `width * 2` drunk/Remix extra-evil requests, and
`floor(width * height * 0.0008)` obsidian requests in source order. Lower
TileRunner traversal and the Remix mound remain deferred.
The lower pass now projects the scheduled evil/obsidian requests through the
shared-RNG TileRunner traversal and typed tile commit. QuickWater, liquid side
effects, and Remix mound behavior remain deferred.

The following `MountainCaveOpenings` boundary is also typed:
`LegacyMountainCaveOpeningScheduler` consumes published cave history and
schedules one radius in `[40,50)` per cave, with a Skyblock guard.
`CaveOpenater` and `Cavinator` tile mutation remain deferred.

`LegacyCaveOpenaterPass` now covers the bounded opening tunnel: source random
width/direction/velocity updates, center wall and clearability guards, circular
kill emission, and typed command commit are focused-verified. `Cavinator`
recursion, dual-dungeon protection, and source-correct post-Dungeon pipeline
ordering remain deferred.

`LegacyCavinatorPass` now models source step counts, circle variation, drift
clamps, recursive continuation above `rockLayer + 50`, and typed kill commits.
Its per-tile dual-dungeon detection and source-correct post-Dungeon ordering
remain deferred.

`LegacyMountainCaveOpeningsPass` now preserves the source per-cave random order:
`CaveOpenater`, then `Next(40,50)`, then `Cavinator`. A focused fresh-snapshot
probe verifies that both typed command sources are emitted. Pipeline placement
and per-tile dual-dungeon protection remain deferred.
