# Main Tick Final Gate Boundary

## Fresh evidence

The focused final gate passed for TickOrder, WorldClock, WorldRules, NetworkIsolation, Persistence,
Persistence loopback, MainBoundary, FullClientBootstrap, and the serial Simulation Release build.
Evidence is under `Build/diagnostics/main-tick/task-12-final-gate/20260824-140000/`.

The persistence verifier's moon/game-mode check now validates the current serialized stream
directly after the V34/NPC typed tail changes invalidated the old hand-built byte offsets. The
rebuilt verifier exits `0` and confirms current-format round-trip behavior.

The focused gates are qualified. A fresh serial `Terraria.Dome.sln` Release build with shared
compilation disabled completed successfully with zero warnings and zero errors. The final build
tail is recorded in `root-solution-build-final.log`; the captured process completed in 00:09:02.76.

The remaining event and host slices retain their source-backed partial status. Random starts,
client presentation, unknown initializer families, arbitrary delegates, and delayed process
collections remain deferred.

## Fresh follow-up gate

After the registry, entity-capacity, Liquid-rule, and FixtureNpcType invariant changes, a fresh
serial root Release build and focused TickOrder, WorldRules, World.Server, and MainBoundary runs
all exited `0`. `git diff --check` also exited `0`.

Evidence: `Build/diagnostics/main-tick/task-12-final-gate/20260824-170000/`.

## 2026-08-25 persistence repair gate

The reduced Task 12 gate initially exposed an NPC persistence defect: current-format NPC
replication omitted `DefinitionId` and `MaximumHealth`, so restore derived fields that conflicted
with registered definitions. The format now writes those fields in version 34, while older format
versions retain their existing read layout. Compatibility import also emits complete known-NPC
definition fields instead of relying on the lossy fallback.

Focused evidence:

- `Build/diagnostics/main-tick/task-12-gate/20260825-063000/summary.txt`
- `Build/diagnostics/main-tick/task-12-gate/20260825-063000/persistence-verifier.log`
- `Build/diagnostics/main-tick/task-12-gate/20260825-070000/summary.txt`
- `Build/diagnostics/main-tick/task-12-gate/20260825-070000/persistence-loopback.log`

## 2026-08-25 serial root and host gate

The serial `Terraria.Dome.sln` Release build completed with exit code 0, zero warnings, and zero
errors. MainBoundary, TickOrder, WorldClock, WorldRules, World.Server, Persistence,
Persistence loopback, NetworkIsolation, and FullClientBootstrap all have fresh exit-0 evidence.
FullClientBootstrap was run with an empty application argument list because its verifier accepts
only no arguments or an explicit `--external-port` option.

Evidence:

- `Build/diagnostics/main-tick/task-12-gate/20260825-080000/summary.txt`
- `Build/diagnostics/main-tick/task-12-gate/20260825-080000/root-solution-build.log`
- `Build/diagnostics/main-tick/task-12-gate/20260825-083000/summary.txt`
- `Build/diagnostics/main-tick/task-12-gate/20260825-084000/summary.txt`
- `Build/diagnostics/main-tick/task-12-gate/20260825-084000/full-client-bootstrap.log`

The fresh physical-deletion ledger check reports `rows=535`, `missingColumns=0`,
`invalidRows=0`, `serverRelevantWithoutEvidence=0`, `serverRelevantDeferred=44`, and
`unknownRows=0`. The safety gate remains exit `1` by design because those 44 server-relevant rows
are explicitly deferred.

Evidence: `Build/diagnostics/main-tick/task-12-gate/20260825-090000/`.

The phase matrix was refreshed to mark the already evidenced server-update/host boundary as
accepted, while retaining the legacy `WorldGen.UpdateWorld` and projectile/item-before-time
relations as partial. This avoids treating an existing host verifier as a planned-but-unowned row.

## 2026-08-25 post-registry root regression

After the subsequent WorldGeneration registry slices, the serial root Release build and fresh
MainBoundary, WorldGeneration, and WorldRules build/run checks all exited 0. The root build log
reported no warnings or errors.

Evidence: `Build/diagnostics/main-tick/task-12-gate/20260825-140000/`.

## 2026-08-25 persistence and host recheck

Fresh Persistence, Persistence loopback, NetworkIsolation, and FullClientBootstrap build/run
checks all exited 0 after the registry and NPC persistence changes. FullClientBootstrap was run
with an empty application argument list as required by its verifier contract.

Evidence: `Build/diagnostics/main-tick/task-12-gate/20260825-160000/`.

## 2026-08-25 final current-tree gate

The current worktree, including the latest 3x3 support registry, passes the serial root Release
build and fresh MainBoundary, WorldGeneration, WorldRules, and Persistence verifier runs.

Evidence: `Build/diagnostics/main-tick/task-12-gate/20260825-180000/`.

The subsequent tree-frame ground registry also leaves the serial root Release build green.
Evidence: `Build/diagnostics/main-tick/task-12-gate/20260825-210000/`.

After the vine registry was shared between framing consumers, the serial root Release build and
fresh MainBoundary, WorldGeneration, and WorldRules runs remained green.
Evidence: `Build/diagnostics/main-tick/task-12-gate/20260826-000000/`.

The tree-leaf pass-style FrozenDictionary also leaves the serial root Release build green.
Evidence: `Build/diagnostics/main-tick/task-12-gate/20260827-010000/`.

The cactus ground registry shared by frame and validation consumers also leaves the serial root
Release build green. Evidence: `Build/diagnostics/main-tick/task-12-gate/20260827-030000/`.

The pumpkin-specific 2x2 support registry also leaves the serial root Release build green, with
zero warnings and zero errors. Evidence: `Build/diagnostics/main-tick/task-12-gate/20260827-050000/`.

The shared conversion-sand registry and its default query overloads also leave the serial root
Release build green, with zero warnings and zero errors. Evidence:
`Build/diagnostics/main-tick/task-12-gate/20260827-070000/`.

The shared tree-leaf checked-type registry and default query overloads also leave the serial root
Release build green, with zero warnings and zero errors. Evidence:
`Build/diagnostics/main-tick/task-12-gate/20260827-090000/`.

The Tile 136 beam and tree-trunk support registries also leave the serial root Release build
green, with zero warnings and zero errors. Evidence:
`Build/diagnostics/main-tick/task-12-gate/20260827-110000/`.

The centralized boulder registry and its WorldGeneration default paths also leave the serial root
Release build green, with zero warnings and zero errors. Evidence:
`Build/diagnostics/main-tick/task-12-gate/20260827-130000/`.

The RoomNeeds static registries and default classifier path also leave the serial root Release
build green, with zero warnings and zero errors. Evidence:
`Build/diagnostics/main-tick/task-12-gate/20260827-150000/`.

The RoomNeeds table registry is also reused by the Tile 2x1 default validation path. After the
typed `ushort` projection was added, the focused verifier and serial root Release build remained
green. Evidence: `Build/diagnostics/main-tick/task-2-room-needs-registry/20260827-160000/` and
`Build/diagnostics/main-tick/task-12-gate/20260827-170000/`.

The Wiring tree-trunk registry now delegates to the WorldGeneration tree-trunk owner; the Wiring
focused verifier and serial root Release build remain green with zero warnings and zero errors.
Evidence: `Build/diagnostics/main-tick/task-2-tree-trunk-centralization/20260827-180000/` and
`Build/diagnostics/main-tick/task-12-gate/20260827-190000/`.

Tree classification and branch/root frame default overloads now reuse the same owner, including
cached `int` projection for the classification API. After the compile fix, focused WorldGeneration
verification and serial root Release build both passed. Evidence:
`Build/diagnostics/main-tick/task-2-tree-trunk-centralization/20260827-200000/` and
`Build/diagnostics/main-tick/task-12-gate/20260827-210000/`.

The first root attempt in that gate correctly failed on the untyped `ushort`/`int` projection
(`root_solution_build=1`); the subsequent rerun after adding the cached `int` projection passed
(`root_solution_build_rerun=0`).

The common-sapling registry and its ordinary/underground tree consumers also leave the serial root
Release build green with zero warnings and zero errors. Evidence:
`Build/diagnostics/main-tick/task-2-common-sapling-registry/20260827-220000/` and
`Build/diagnostics/main-tick/task-12-gate/20260827-230000/`.

The ordinary-tree trunk command path now reuses the same common-sapling owner; focused
WorldGeneration verification and the serial root Release build remain green. Evidence:
`Build/diagnostics/main-tick/task-2-common-sapling-registry/20260827-240000/` and
`Build/diagnostics/main-tick/task-12-gate/20260827-250000/`.

The verifier now guards that every legacy tree profile sapling belongs to the common-sapling
registry; focused WorldGeneration verification and the serial root build remain green. Evidence:
`Build/diagnostics/main-tick/task-2-common-sapling-registry/20260828-000000/` and
`Build/diagnostics/main-tick/task-12-gate/20260828-010000/`.

Task 0 source baseline was refreshed without changing the reference hash: `Main.cs` is 14,735
lines and 379,274 bytes at SHA-256
`844A862B4EF863FF848227C1D682E89DAB799EEAD48070ADEA07565EC933254D`.
