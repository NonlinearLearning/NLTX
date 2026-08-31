# Main Tick Trace Boundary

Task 3 has a bounded executable trace contract. `SimulationTickTrace` records the completed
world tick, input sequence range, command count sampled immediately before commit, published
event count, and immutable phase order. Paused ticks publish only the documented
`BeginTick -> ApplyWorldClock -> EndTick` trace and retain tick number zero.

The existing named `SimulationTickSchedule` remains authoritative; this slice does not infer or
change legacy ordering. Input sequence metadata is explicit on `SimulationInputBatch`; callers
that do not provide a range use `0..0`.

Evidence is kept under the fresh task directory after the final serial rerun. The trace verifier
covers normal, paused, deterministic input, invasion ordering, and NPC death command/event
counts. Full legacy phase parity, arbitrary caller sequencing, and unmodeled event families remain
deferred.
