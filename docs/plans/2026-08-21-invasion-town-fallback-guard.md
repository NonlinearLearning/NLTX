# Invasion Town-NPC Fallback Guard

## Source Boundary

After the direct invasion-position window, `NPC.Spawner.ShouldSpawnInvasionEnemies` enters a
fallback only when `invasionX` is within `maxTilesX / 2 +/- 5`. It then considers only town NPCs
whose center is within an open 3000-pixel distance of the candidate position
(`NPC.cs:358-370`). The source consumes `Main.rand.Next(3)` after these facts; that random branch
is intentionally excluded.

## Green Narrow Route

`WorldInvasionTownFallbackGuardSystem.IsEligible` captures the deterministic preconditions:
the candidate must be a town NPC, `invasionX` must be in the inclusive center interval, and the
candidate must be strictly less than 3000 pixels from the town NPC center. Non-finite coordinates
and invalid world widths reject. It performs no enumeration, random draw, collision check or spawn
commit.

## Verification Scope

Verification: WorldRules/NPC focused runs and two clean replays pass; Simulation/Server Release
builds are warning-free and the scoped diff check passes.

Evidence:
`Build/diagnostics/main-migration/task-9-invasion-town-fallback-guard/20260821-231611/`

MainBoundary, root Release, broad regressions and complete NPC invasion spawning remain outside the
focused-only validation scope.

## Deferred Branches

`Main.rand.Next(3)`, NPC enumeration, collision, spawn tables, full spawn selection, replication,
client behavior and complete invasion lifecycle integration remain separate cards.
