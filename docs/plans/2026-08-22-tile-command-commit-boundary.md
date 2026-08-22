# Tile Command Commit Boundary

## Source Boundary

Legacy `Main.tile[,]` is a mutable global array consumed by WorldGen, wiring, item placement,
liquid and network framing code. It cannot be copied into Simulation as a shared mutable owner.

## Current Authority

`TileChangeCommand` is the value command boundary and `TileChangeCommitSystem` is the single
commit owner. The system sorts commands deterministically by sequence/key, rejects negative or
repeated sequences, out-of-world coordinates and invalid kinds, applies the tile mutation and
increments only affected section revisions. Higher-level systems enqueue commands rather than
mutating the grid directly.

## Decision

Accept the command/commit sub-boundary narrowly. This does not claim complete tile parity:
multitile framing, liquid simulation, WorldGen-specific kill/place rules, network packet side
effects and all legacy tile flags remain separate cards.

## Focused Evidence

Evidence: `Build/diagnostics/main-migration/task-9-tile-command-commit-boundary/20260822-015000/`.
TileInteraction and WorldRules verifiers cover deterministic commits, invalid command rejection
and section revision behavior; scoped diff check passes.
