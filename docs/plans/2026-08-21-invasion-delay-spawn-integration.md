# Invasion Delay Spawn Integration

## Source Boundary

`NPC.Spawner.ShouldSpawnInvasionEnemies` rejects an invasion before later position and NPC
selection logic when `invasionType <= 0`, `invasionDelay != 0` or `invasionSize <= 0`
(`NPC.cs:348-353`). Ordinary NPC candidates do not pass through this invasion-specific branch.

## Green Authority Route

`NpcSpawnCandidate.IsInvasionCandidate` marks the candidate's source branch. Such candidates must
carry an authoritative `NpcInvasionSpawnState`; `NpcSpawnEligibilitySystem` invokes the existing
`WorldInvasionSpawnEligibilitySystem` and rejects missing facts, nonzero delay, invalid type or
empty size before producing a spawn command. Ordinary candidates remain unchanged. A candidate
with delay zero reaches the existing deterministic commit path.

## Verification Scope

The focused NPC verifier covers blocked delay, accepted zero delay, missing facts and the unchanged
ordinary-candidate route. WorldRules and Persistence focused verifiers remain green after the
shared state/persistence changes. Two clean NPC verifier runs produce identical output hashes.
Simulation and Server Release builds report zero warnings and errors, and scoped diff check passes.

Evidence:
`Build/diagnostics/main-migration/task-9-invasion-delay-spawn-integration/20260821-233247/`

MainBoundary, root Release, complete invasion spawn production, NPC tables, random branches,
replication and client behavior remain outside focused-only validation.

## Deferred Branches

Complete invasion spawn producer, position/town fallback integration, NPC tables, random branches,
collision, replication and client behavior remain separate cards.
