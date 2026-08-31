# Invasion Position Guard

## Source Boundary

`NPC.Spawner.ShouldSpawnInvasionEnemies` evaluates the surface condition and then checks the
candidate X coordinate against `invasionX * 16 +/- 3000` (`NPC.cs:352-366`). The frozen source
defines `sHeight` as `1200` (`NPC.cs:6620`). The X comparison is an open interval. A separate
town-NPC fallback, including `Main.rand.Next(3)`, follows this guard and is not part of this card.

## Green Narrow Route

`WorldInvasionPositionGuardSystem.IsWithinSpawnWindow` owns only the deterministic surface and
position predicate. It rejects non-finite inputs, requires
`positionY < worldSurface * 16 + 1200 || spawnTileY > worldSurface`, and then requires the X
coordinate to be strictly inside the 3000-pixel window around `invasionX * 16`.

No NPC list, collision query, random fallback, spawn table or client behavior is inferred.

## Verification Scope

Verification: WorldRules/NPC focused runs and two clean replays pass; Simulation/Server Release
builds are warning-free and the scoped diff check passes.

Evidence:
`Build/diagnostics/main-migration/task-9-invasion-position-guard/20260821-230021/`

MainBoundary, root Release, broad regressions and full NPC invasion spawning remain outside the
focused-only validation scope.

## Deferred Branches

Town-NPC fallback, fallback randomness, collision, NPC tables, complete spawn selection,
replication, client behavior and complete invasion position persistence remain separate cards.
