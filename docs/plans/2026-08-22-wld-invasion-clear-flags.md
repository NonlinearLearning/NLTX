# WLD Invasion Clear Flags

## Source Boundary

The V319 and legacy WLD layouts serialize `downedGoblins`, `downedFrost`, `downedPirates` and
`downedMartians` as separate booleans (`WorldFile.cs:1334-1382, 2151-2267, 3655-3667`). The
readers previously discarded these values. This card restores the reader and compatibility
projection path only; protocol bit packing, client projection, achievements and announcements are
outside the card.

## Green Authority Route

The header reader captures the three pre-invasion flags from the source boolean group. The later
field reader captures `downedMartians` from the first post-banner event-flag group. Legacy versions
only expose flags introduced by their historical version and default absent flags to false. All
four values flow through `LegacyWorldMetadata`, `CompatibilityWorldMetadata` and the Dome
progression projection.

## Verification Scope

WorldImport verifies all four flags survive compatibility projection. WorldFile V319, Persistence,
WorldRules and NPC focused verifiers pass. Simulation and Server Release builds pass with zero
warnings and errors. Two WorldImport replays are byte-identical and scoped diff check exits `0`.

Evidence:
`Build/diagnostics/main-migration/task-9-wld-invasion-clear-flags/20260822-000856/`

Protocol bit packing, client projection, full real-WLD fixture values, MainBoundary, root Release
and broad suites remain intentionally outside focused-only validation. This card is `green`, not
fully accepted.
