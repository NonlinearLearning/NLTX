# Moon Phase And Day Rule

## Source And Owner

`Main.GetMoonPhase()` returns the stored `moonPhase` value
(`Main.cs:1924-1929`). `Main.IsItDay()` returns `false` for `remixWorld` and otherwise
returns `dayTime` (`Main.cs:12864-12873`). The existing authoritative owner is
`WorldTimeClassificationSystem`: `GetMoonPhase(int)` validates the source range 0..7 and
`IsGameplayDayTime(bool, bool)` preserves the remix override explicitly.

## Decision

Accept the pure rule card narrowly. The rule takes `isRemixWorld` as an explicit input and does
not invent a world variant or silently import one. WLD import of the variant itself remains
outside this card and must be handled by a separate metadata/import decision.

## Focused Evidence

Evidence: `Build/diagnostics/main-migration/task-9-moon-phase-day-boundary/20260822-005500/`.
The NPC verifier covers the classification rule and the WorldClock verifier covers the stored
phase and day/night boundary behavior. Both pass; the scoped documentation diff check passes.
MainBoundary, root Release and broad suites remain outside focused validation.
