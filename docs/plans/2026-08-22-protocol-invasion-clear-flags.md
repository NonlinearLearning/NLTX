# Protocol Invasion Clear Flags

## Source Boundary

The legacy `NetMessage` world-data projection maps `downedMartians` to `EventFlags8` bit 6 and
`downedPirates`, `downedFrost`, `downedGoblins` to `EventFlags10` bits 0, 1 and 2
(`NetMessage.cs:307, 321-323`). This card restores the server-owned projection into the existing
V1456 compatibility DTO and encoder; client runtime behavior and full network loopback remain
outside the card.

## Green Authority Route

`LegacyWorldDataContext.WithWorldState` derives the four event bits from
`WorldProgressionState` while preserving unrelated existing bits. `TerrariaV1456Compatibility`
continues to write the unchanged source field order and payload length.

## Verification Scope

The Protocol Compatibility focused verifier checks all four bits and confirms the WorldData payload
remains 173 bytes. WorldRules and Persistence focused verifiers pass, Simulation and Server Release
builds report zero warnings and errors, two protocol verifier replays are byte-identical, and scoped
diff check exits `0`.

Evidence:
`Build/diagnostics/main-migration/task-9-protocol-invasion-clear-flags/20260822-001313/`

Client runtime, full network loopback, achievement/announcement semantics, MainBoundary, root
Release and broad suites remain intentionally outside focused-only validation. This card is
`green`, not fully accepted.
