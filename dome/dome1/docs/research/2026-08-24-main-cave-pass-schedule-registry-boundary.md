# Main Cave Pass Schedule Registry Boundary

`LegacyCavePassContractDefinition.CreateDefaultSchedule()` now returns a stable read-only
registration result for the five supported cave passes: Mountain, Dirt Layer, Rock Layer,
Surface, and Wavy caves. The existing coordinator and verifier retain source order, random
reset, TileRunner, and mutation predicates.

Focused evidence:

- `Build/diagnostics/main-tick/task-2-cave-schedule-registry/20260824-234500/summary.txt`
- `Build/diagnostics/main-tick/task-2-cave-schedule-registry/20260824-234500/verifier-build.log`
- `Build/diagnostics/main-tick/task-2-cave-schedule-registry/20260824-234500/world-generation-verifier.log`

This is bounded cave schedule registration coverage, not complete historical cave orchestration
or legacy random parity.
