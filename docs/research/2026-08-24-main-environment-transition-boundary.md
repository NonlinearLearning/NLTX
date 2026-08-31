# Main Environment Transition Boundary

Task 5 has a bounded server-owned environment fact. `WorldWeatherSystem.AdvanceWithTransition`
returns the resulting authoritative rain and wind state plus one transient
`WorldEnvironmentTransition`. The Simulation publishes it after applying the current Tick's
clock/rule inputs, so rain start, rain continuation/expiration, wind target/current, and Lantern
Night same-Tick suppression are observable without a client callback.

The transition also carries the source sequence for accepted rain-start or wind-change inputs.
Clock-only continuation and uncommanded stop paths retain `Sequence: -1`.

The fact is not a replacement for durable rule state. Random weather starts, legacy random stream
parity, cloud/ambience presentation, and full event projection remain deferred.

Evidence: `Build/diagnostics/main-tick/task-5-environment-transition/20260824-104500/` and
`Build/diagnostics/main-tick/task-5-environment-fact-identity/20260828-200000/`.
WorldRules, WorldRules loopback, MainBoundary, and serial Simulation Release build all exited 0;
the build reported 0 warnings and 0 errors.
