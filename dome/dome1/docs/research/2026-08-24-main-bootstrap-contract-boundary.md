# Main Bootstrap Contract Boundary

Task 1 of `2026-08-24-main-tick-initialization-world-events-execution.md` is partially
qualified for the current server slice.

## Accepted

- `WorldBootstrapRequest` is immutable and carries world metadata, seed, dimensions, spawn,
  validated rules, generation status, and entity limits.
- `WorldBootstrap.Create` generates a deterministic zero-tick snapshot from the request.
- `WorldBootstrap.Restore` rejects identity/rule mismatches and preserves the snapshot tick;
  the first scheduled tick is `snapshot.TickNumber + 1`.
- The request seed initializes the Simulation world seed and event random stream; distinct seeds
  produce distinct initial random states, including through snapshot restore construction.
- Existing `CreateDefault` and WLD `Load` entry points remain compatible.

## Evidence

Fresh evidence: `Build/diagnostics/main-tick/task-1-bootstrap/20260824-091500/`.

- World.Server verifier: exit 0; malformed dimensions/spawn/rules, equivalent requests, restore
  boundary, section visibility and initial replication passed.
- TickOrder verifier: exit 0; named schedule, paused boundary, deterministic input ordering and
  world-event ordering passed.
- MainBoundary verifier: exit 0; 848 Simulation source files, 0 forbidden dependencies.
- Server Release build: exit 0, 0 warnings, 0 errors.

## Deferred

Entity limits are currently an immutable bootstrap contract but are not yet wired into every
domain registry. Definition registration, random event starts, arbitrary main-thread actions,
delayed processes, and full persistence/restart orchestration remain deferred until their caller
and owner contracts are recovered.
