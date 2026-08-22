# Moon Phase And Day Boundary

## Source Audit

Legacy `Main.GetMoonPhase()` returns the stored `moonPhase` enum directly
(`Main.cs:1924-1929`). Legacy `Main.IsItDay()` first checks `remixWorld` and returns `false`
for that world variant; otherwise it returns `dayTime` (`Main.cs:12864-12873`).

The ECS clock already owns a validated `MoonPhase` and `IsDayTime`, but it has no
`remixWorld` state. Therefore the moon-phase projection is directly representable, while a
general `IsItDay()` compatibility claim is not source-backed for remix worlds.

## Decision

Keep the combined M-012 card blocked for full parity. Existing clock behavior remains unchanged:
normal worlds use the authoritative `WorldClock.IsDayTime`, and moon phase is range-validated.
Do not add a default `remixWorld` value or silently reinterpret it as ordinary day/night.
A future card must add a versioned world-variant owner or explicitly restrict the supported import
contract before projecting `IsItDay()`.

## Focused Evidence

Evidence: `Build/diagnostics/main-migration/task-9-moon-phase-day-boundary/20260822-005000/`.
The WorldClock verifier and scoped documentation diff check pass. Large suites and root Release
remain outside focused validation.
