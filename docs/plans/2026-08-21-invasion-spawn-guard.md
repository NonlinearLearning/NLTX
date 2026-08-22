# Invasion Spawn Eligibility Guard

## Source Boundary

`NPC.Spawner.ShouldSpawnInvasionEnemies` begins with three server-side guards in the frozen
legacy source (`D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs:348-353`):
`invasionType > 0`, `invasionDelay == 0`, and `invasionSize > 0`. Only after those checks does the
source evaluate position, world-surface and town-NPC fallback behavior.

## Green Narrow Route

`WorldInvasionSpawnEligibilitySystem.CanSpawn` captures exactly those three preconditions as a
pure Simulation predicate. It accepts a positive active invasion with zero delay and rejects
non-positive type, non-positive remaining size, or any non-zero delay, including negative delay.
No position, collision, random town-NPC, NPC table or spawn-count behavior is inferred.

## Verification Scope

The focused WorldRules verifier covers accepted and rejected guard combinations. The existing NPC
focused verifier remains green. Two WorldRules replays have identical output hashes. Simulation and
Server Release builds report zero warnings and errors, and the scoped diff check passes.

Evidence:
`Build/diagnostics/main-migration/task-9-invasion-spawn-guard/20260821-230021/`

MainBoundary, root Release, large regression suites and full NPC spawn integration remain outside
the focused-only validation scope.

## Deferred Branches

Invasion position/range checks, world-surface checks, town-NPC fallback randomness, collision,
spawn tables, complete NPC selection, replication and client behavior remain separate cards.
