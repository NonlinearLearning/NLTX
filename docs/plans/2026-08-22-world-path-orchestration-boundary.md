# World Path Orchestration Boundary

## Source Audit

Legacy `Main.GetWorldPathFromName` sanitizes invalid filename characters, replaces spaces and
periods, selects local versus cloud roots, and appends a numeric suffix when a file already
exists (`Main.cs:2226-2253`). This method is filesystem policy and never belongs in Simulation.

## Current Server Owner

The Dome server accepts an explicit `--world` path through `ServerLaunchOptions`. `WorldBootstrap.Load`
requires a fully qualified path and delegates reading to `DomeWorldImportApplier`; import then
projects an immutable Simulation snapshot. This is the authoritative server boundary for the
current product contract.

## Decision

Accept the explicit-path server boundary narrowly. Do not copy legacy filename sanitization,
cloud storage, collision suffixing or interactive world selection into Simulation. Automatic
world-name-to-path resolution remains deferred to a separate persistence orchestration card if
the product requires it.

## Focused Evidence

Evidence: `Build/diagnostics/main-migration/task-9-world-path-boundary/20260822-013000/`.
WorldImport and World.Server verifiers cover argument/path/bootstrap behavior; scoped diff check
passes.
