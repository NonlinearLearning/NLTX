# Main Projectile Registry Boundary

## Accepted slice

The supported projectile family is owned by `ProjectileDefinitionRegistry`. Its input is
materialized once, duplicate and invalid definitions are rejected during construction, the
definition map is wrapped in `ReadOnlyDictionary`, and `OrderedDefinitions` preserves the explicit
registration order used by startup composition. No aggregate `InitializeAlmostEverything` was
introduced.

The same bounded contract now applies to `NpcDefinitionRegistry`: its source-order input is
materialized, its map is read-only, and `OrderedDefinitions` provides stable registration order.

`ItemDefinitionRegistry` now follows the same contract while retaining its existing cross-reference
validation for use, combat, placement, recovery, and extractinator definitions.

## Evidence

- Combat verifier: exit `0`
- NPC verifier: exit `0`
- Item definitions verifier: exit `0`
- Simulation Release build: exit `0`, `0` warnings, `0` errors
- MainBoundary: exit `0`, `0` violations
- Evidence directory:
  `Build/diagnostics/main-tick/task-2-projectile-registry/20260824-151000/`

## Deferred

Complete legacy projectile type/AI tables, projectile entity-capacity wiring, and client/content
initializers remain deferred until their source-backed owner and consumer contracts are available.
