# World Preparation Boundary

## Source Audit

Legacy `Main.UpdateWorldPreparationState` unconditionally assigns the client-only
`_worldPreparationState = Ready` (`Main.cs:2171-2176`) and is called from the client update loop
(`Main.cs:11623`). Its consumers gate client/UI update behavior (`Main.cs:11812`); no server
simulation fact is produced.

## Decision

Keep `WorldPreparationState` outside Simulation. The server equivalent is ready-by-construction:
`WorldBootstrap.Load` or `WorldBootstrap.CreateDefault` completes validated WLD import/default
world generation and returns a `WorldBootstrapResult` before `DomeServer` construction. No second
preparation state machine is introduced.

## Focused Evidence

WorldImport verifies deterministic default bootstrap and strict import/no-listener failure behavior.
The Server Release build passes with zero warnings and errors, and scoped diff check exits `0`.

Evidence:
`Build/diagnostics/main-migration/task-9-world-preparation-boundary/20260822-001918/`

Client loading/UI sequencing, MainBoundary, root Release and broad suites remain outside the
focused-only validation scope. The client state is intentionally excluded; the server bootstrap
boundary is covered narrowly.
