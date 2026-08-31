# Main TileRunner Loop Registry Boundary

`LegacyTileRunnerPassLoopContractDefinition.CreateDefault()` now returns a stable read-only
projection for the three supported invocation loops. The existing density calculations,
remix multiplier behavior, execution ledger, and transactional command checks remain unchanged.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-tile-runner-loop-registry/20260824-240500/summary.txt`
- `Build/diagnostics/main-tick/task-2-tile-runner-loop-registry/20260824-240500/verifier-build.log`
- `Build/diagnostics/main-tick/task-2-tile-runner-loop-registry/20260824-241000/summary.txt`
- `Build/diagnostics/main-tick/task-2-tile-runner-loop-registry/20260824-241000/world-generation-verifier-rerun.log`

This is bounded TileRunner loop registration coverage, not complete historical random or cave
execution parity.
