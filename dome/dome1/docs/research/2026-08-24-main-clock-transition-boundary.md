# Main Clock Transition Boundary

Task 4 has a bounded clock-transition slice. `WorldClockSystem` now returns an optional
`WorldClockTransition` fact for a day/night boundary, including `Dawn` or `Dusk`, authoritative
tick number, time of day, and moon phase. `DomeSimulation` clears the fact at the start of every
Tick, publishes at most one transition for the accepted Tick, and publishes none while paused.

The transition is deterministic from the immutable clock snapshot and rate. Restore at the same
boundary reproduces the same single fact on the first accepted Tick; no transition is persisted as
a durable event. Weather, random starts, client presentation, and full event-state persistence
remain outside this slice.

Evidence: `Build/diagnostics/main-tick/task-4-clock-transition/20260824-101500/`. The final
WorldClock, TickOrder, and MainBoundary runs used `--no-build` only after the changed Simulation
assembly had a serial Release build with zero warnings and errors. An earlier concurrent build
attempt recorded the known `CS2012` output-lock failure; it is not the final verification result.
