# Invasion Clear Flags State

## Source Boundary

`Main.UpdateInvasion` sets one named NPC progression flag when an invasion reaches zero
(`Main.cs:12970-12986`). The corresponding WLD fields are independently serialized by
`WorldFile` (`WorldFile.cs:1334-1382, 2151-2267, 3655-3667`). This card implements the completion
transition and authoritative state owner only; WLD flag import, achievements, announcements and
protocol projection remain separate.

## Green Authority Route

`WorldProgressionState` now owns `DefeatedGoblins`, `DefeatedFrost`, `DefeatedPirates` and
`DefeatedMartians`. When `DomeSimulation` normalizes a completed invasion, it resolves the typed
clear flag once, sets the matching named fact, and publishes the existing typed completion event.
The four flags are appended to Dome persistence as a v26 tail after the v25 `InvasionX` field;
older formats restore false without reinterpreting prior bytes.

## Verification Scope

WorldRules verifies a type-2 completion sets only `DefeatedFrost`; Persistence verifies the named
flag and travel state survive restart, including legacy compatibility fixtures. NPC focused
verification and Simulation/Server Release builds pass with zero warnings and errors. Two clean
WorldRules replays are byte-identical, and scoped diff check exits `0`.

Evidence:
`Build/diagnostics/main-migration/task-9-invasion-clear-flags-state/20260822-000208/`

WLD flag import, achievements, announcements, NetMessage/client projection, MainBoundary, root
Release and broad suites remain intentionally outside focused-only validation. This card is
`green`, not fully accepted.
