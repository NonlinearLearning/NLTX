# Main Bootstrap Entity Limits Boundary

## Accepted slice

`WorldBootstrapRequest` already validated `WorldEntityLimits`, but the result was previously
discarded. The bootstrap result now carries the validated limits into `DomeServer`. A new session
player is rejected before `DomeSimulation.CreatePlayer` when the configured maximum player count
is reached. `MaximumPlayers` is also bounded by the V1456 byte slot range so the host cannot accept
an impossible protocol configuration.

## Evidence

- World.Server verifier: exit `0`
- Server Release build: exit `0`, `0` warnings, `0` errors
- MainBoundary: exit `0`, `0` violations
- Evidence directory:
  `Build/diagnostics/main-tick/task-1-entity-limits/20260824-150000/`

## Deferred

NPC, projectile and world-item limits remain configuration contracts only; their complete registry
capacity and persistence orchestration still require domain-specific source and verifier evidence.
Universal definition initialization, random event starts, arbitrary main-thread actions and delayed
`IEnumerator` processes remain deferred.
