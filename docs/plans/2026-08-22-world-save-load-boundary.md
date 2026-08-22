# World Save And Load Boundary

## Source Audit

Legacy server flow loads a WLD asynchronously through `WorldGen.serverLoadWorld()` and saves
through `WorldFile.SaveWorld()` at console command and server-exit sites
(`Main.cs:3083-3100`, `3190-3194`, `3313-3316`). The same file path also serves client world
listing and presentation state.

## Current Owners

The Dome server owns WLD input through `WorldBootstrap.Load` and `DomeWorldImportApplier`.
Authoritative value snapshots are written/read through `WorldPersistenceFormat` for the grid and
`DomeStatePersistenceFormat` for the full Simulation snapshot. Restart construction from a
recovered snapshot is already verified.

## Decision

Accept the value-only snapshot and restart boundary narrowly. Do not add WLD serialization,
async progress/UI, cloud saves or server-exit scheduling to Simulation. Full WLD write-back and
operational save scheduling remain separate Server orchestration work.

## Focused Evidence

Evidence: `Build/diagnostics/main-migration/task-9-world-save-load-boundary/20260822-014000/`.
Persistence and WorldImport verifiers cover round-trip, strict recovery, atomic replacement,
old-format defaults and restart adoption; scoped diff check passes.

The embedded `WorldPersistenceFormat` has since advanced to v4 to carry nullable WLD
`IsRemixWorld` metadata required by the Slime Rain eligibility boundary. v1-v3 readers remain
compatible: v3 keeps its existing `WorldSurface`, while the absent Remix marker restores as
unknown.
