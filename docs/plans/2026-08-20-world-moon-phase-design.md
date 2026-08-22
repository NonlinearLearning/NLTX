# World Moon Phase Migration Design

## Decision

Move the V1456 moon phase into `WorldClock` as a constrained `byte` state. The state advances only
when the authoritative clock crosses from night to day. It is persisted in a new append-only Dome
state version and projected through the existing V1456 WorldData `MoonPhase` field.

## Source Contract

The no-deletion V1456 oracle at `D:\TRbackup\无任何删减通过编译\Terraria\Main.cs:66264-66301`
increments `moonPhase` in `UpdateTime_StartDay` and wraps values at 8. The same source uses the
integer in the V1456 WorldData path. Existing `WorldMoonPhase` preserves the V1456 enum order.

## Authority

```text
WorldClockSystem.Tick
  -> WorldClock.Advance
  -> dawn transition increments MoonPhase modulo 8
  -> WorldClockSnapshot
  -> DomeSimulationSnapshot
  -> DomeStatePersistenceFormat v19
  -> DomeServer.CreateWorldDataContext
  -> LegacyWorldDataContext.WithWorldState
  -> V1456 WorldData payload
```

No protocol handler, persistence reader, verifier, or server adapter may mutate moon phase.
Invalid snapshot values fail closed before any state is restored.

## Compatibility

`DomeStatePersistenceFormat` v19 appends one byte after the existing v18 world-surface field.
Readers of versions 1 through 18 restore moon phase as `0`, the only source-neutral default for
historical Dome snapshots that did not record a phase. New v19 payloads reject truncated or invalid
phase bytes and retain the existing trailing-data rejection.

## Scope

Accepted behavior is limited to dawn increment, modulo-8 wrap, pause preservation, snapshot restore,
persistence compatibility, and existing WorldData projection. This does not migrate remix-world
gameplay-day semantics, moon-type visuals, automatic event eligibility, NPC/Player consumers, or
the original `Main.rand` stream.

