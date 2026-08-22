# Main ECS migration M-024 delayed process semantics boundary

## Decision

Keep `Main.DelayedProcesses` and `Main.DelayedProcessesInGame` deferred. Both are mutable lists of
arbitrary `IEnumerator` instances advanced inline by the legacy frame loop. The first list is mixed
with client/UI and environment work; the second is explicitly gated by menu/pause state and also
shares the loop with internal callbacks and ambience. There is no source-backed server-only command
identity, schedule phase, cancellation contract or persistence representation for an arbitrary
enumerator.

The current ECS boundary remains `SimulationCommandQueue`: typed commands carry validation and
sequence ordering, and `SimulationTickSchedule` owns the named phases. No generic coroutine or
delegate queue is added to Simulation.

## Source scope

Version4 `Main.cs:242-244` declares both lists and `Main.cs:11629-11636` / `11733-11739` advances
and removes enumerators. The surrounding loop contains client callbacks, menu/pause conditions,
ambience and server update work, so a direct transplant would violate server authority and
deterministic phase ownership.

## Status

Unknown/deferred. A future card needs a caller inventory and one typed contract per server-owned
delayed operation, with explicit phase, cancellation, restart and replay semantics. Arbitrary legacy
enumerators remain unsupported.

The focused source inventory at
`Build/diagnostics/main-migration/task-9-delayed-process-semantics-boundary/20260822-090000/`
confirms that the two lists cross client/UI, pause/menu, ambience, weather and entity update
boundaries. The existing named tick schedule and typed command queue pass their focused verifier;
this is boundary evidence, not acceptance of delayed-process parity.
