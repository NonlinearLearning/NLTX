# Main Thread Action Boundary

## Source Audit

Legacy `Main.QueueMainThreadAction` stores arbitrary `Action` delegates in a concurrent queue and
executes them at the end of the client update (`Main.cs:11541-11553, 11577`). Current call sites
include `WorldGen` section completion (`WorldGen.cs:10529`) and background world transformation
follow-ups (`WorldGen.cs:26122`), crossing client section/UI and server/background generation
responsibilities.

## Decision

Do not add a generic delegate queue to Simulation. `SimulationCommandQueue` remains typed by domain
command and is deterministic, inspectable and authority-owned. Client section/UI callbacks stay in
the host/client boundary; server background-generation follow-ups require separately typed command
contracts when their source semantics are recovered.

This leaves M-014 explicitly `unknown/deferred`, rather than falsely claiming that arbitrary legacy
actions have an ECS owner.

## Focused Evidence

TickOrder verifies the typed command queue's deterministic ordering and phase boundaries. The
Wiring/Liquid/Chest contracts verifier confirms shared typed command contracts remain deterministic
and commit-safe. Both exit `0`; scoped diff check exits `0`.

Evidence:
`Build/diagnostics/main-migration/task-9-main-thread-action-boundary/20260822-001736/`
