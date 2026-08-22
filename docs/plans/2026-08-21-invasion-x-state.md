# Invasion X State

## Source Boundary

The legacy WLD layouts read and write `Main.invasionX` as a `Double` in the invasion state
record (`WorldFile.cs:1347, 2171, 3680`). The value is the travel position consumed by the
`UpdateInvasion` movement and NPC position guards. This card restores the persisted fact only;
random start-side selection, NPC tables, announcements and complete runtime travel integration
remain separate.

## Green Authority Route

`invasionX` now flows through `LegacyWorldMetadata`, `CompatibilityWorldMetadata`,
`CompatibilityToDomeProjection` and `WorldProgressionState`. It is appended to Dome persistence
as a v25 tail after the v24 invasion delay field. Older formats default it to `0` and do not
reinterpret previous bytes. The progression constructor rejects non-finite values.

## Verification Scope

The WorldImport focused RED identified all three missing owners before implementation. WorldImport,
WorldFile V319 and Persistence focused verifiers pass, including legacy persistence compatibility.
WorldRules and NPC focused verifiers pass; Simulation and Server Release builds report zero
warnings and errors. Two WorldImport replays are byte-identical.

Evidence:
`Build/diagnostics/main-migration/task-9-invasion-x-state/20260821-234954/`

MainBoundary, root Release, broad regressions, random start-side parity, NPC spawn tables,
announcements and full invasion travel runtime integration remain intentionally outside the focused
validation scope. This card is `green`, not fully accepted.
