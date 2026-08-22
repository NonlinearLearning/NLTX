# Invasion Progress Projection

## Source Boundary

The legacy `Main.ReportInvasionProgress` path derives progress from
`invasionSizeStart - invasionSize`, uses `invasionSizeStart` as the maximum, and maps the
invasion type to icon `invasionType + 3` (`Terraria/Main.cs:12246-12261`). Client UI state,
network packet emission and display timers are outside this card.

## Green Authority Route

`WorldInvasionProgressProjectionSystem.Resolve` exposes only the deterministic numeric
projection. It returns unavailable when the invasion type is outside `1..4`, when the original
size is unknown (`InvasionSizeStart == 0`), or when the remaining size is inconsistent. It does
not fabricate a maximum for unknown legacy state.

## Verification Scope

The WorldRules focused verifier covers a valid type-2 projection and the unknown-original-size
sentinel. Persistence and NPC focused verifiers also pass because the shared world state and NPC
spawn integration remain compatible. Simulation and Server Release builds pass with zero warnings
and errors. Two clean WorldRules replays are byte-identical.

Evidence:
`Build/diagnostics/main-migration/task-9-invasion-progress-projection/20260821-233919/`

MainBoundary, root Release, broad regressions, client UI, network reporting and full invasion
lifecycle parity remain intentionally outside the focused-only validation scope. This card is
`green`, not fully accepted.
