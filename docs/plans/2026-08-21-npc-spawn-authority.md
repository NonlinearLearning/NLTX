# NPC Spawn Authority Slice

## Source Boundary

The isolated `Terraria/NPC.cs` oracle calls `CanSpawnEnemiesNear(Player)` from
`NPC.cs:185-224` and defines the initial server-side rejection guards at `NPC.cs:256-272`:
inactive/dead players and explicitly disabled spawn authority cannot enter natural spawn
evaluation. The remainder of `TrySpawnAnNPC`, `FindSpawnTile`, zone selection, event tables and
`Main.rand` selection is outside this card.

## Accepted Narrow Route

`NpcSpawnSnapshot.SpawnAuthorityEnabled` is an immutable input fact. When it is false,
`NpcSpawnEligibilitySystem` returns no spawn commands before evaluating candidates. Each
`NpcSpawnCandidate` also carries the source `CanSpawnEnemiesNear(Player)` result; a false result is
rejected before definition, budget or identity processing. The new candidate field defaults to true
for existing callers, preserving prior command-level behavior. Candidate positions must also be
finite; non-finite coordinates are rejected before command emission. These are guard contracts, not
a claim that ECS reproduces the legacy player-zone or random spawn algorithm.

## Deferred Behavior

Player readiness derivation and projection of inactive/dead player state remain deferred; this card
only consumes the explicit candidate fact. Spawn-rate calculation, tile search, screen exclusion,
biome/event predicates, invasion/NPC tables and exact `Main.rand` order also remain deferred. They
require separate source-backed state and deterministic random-stream cards.

## Verification Scope

The NPC verifier covers accepted and rejected candidate paths, including disabled spawn authority and
a candidate with `CanSpawnEnemiesNear = false`.
The Simulation project builds in Release with zero warnings and errors. Full MainBoundary, loopback,
affected domain regressions and root solution Release build are intentionally not run in this phase.
