# Invasion Warning Message Projection

## Source Boundary

`Main.InvasionWarning` selects a localized warning branch from invasion type, remaining size,
and travel position (`Main.cs:12958-13037`). The branch may broadcast chat, but this card stops at
the server-owned typed selection and does not introduce localized text, chat transport or packet
behavior.

## Green Authority Route

`WorldInvasionWarningMessageSystem.Resolve` maps completion, approaching, receding and arrival to
typed `WorldInvasionWarningKind` values. It preserves the source omission for Martian invasions
while they are still travelling. Invalid types, negative sizes and non-finite positions resolve to
`None` rather than producing a fabricated warning.

## Verification Scope

The WorldRules focused verifier covers each selected branch and the Martian travelling omission.
NPC focused verification and Simulation/Server Release builds pass with zero warnings and errors.
Two clean WorldRules replays are byte-identical, and scoped diff check exits `0`.

Evidence:
`Build/diagnostics/main-migration/task-9-invasion-warning-message/20260821-235450/`

Warning counter persistence, full travel runtime integration, localized text, chat broadcast,
network replication, MainBoundary, root Release and broad suites remain intentionally outside this
focused card. This card is `green`, not fully accepted.
