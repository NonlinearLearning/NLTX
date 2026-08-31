# Main Tick Phase Coverage Matrix

**Reference:** `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs`

**SHA-256:** `844A862B4EF863FF848227C1D682E89DAB799EEAD48070ADEA07565EC933254D`

**Refreshed line count:** `14,735` (evidence: `Build/diagnostics/main-tick/task-0-baseline/20260825-230000/main-source-baseline.json`)

This matrix compares source responsibility ordering with the ECS named schedule. It does not claim
whole-loop order parity: legacy `Main.Update` mixes client work, exception handling, network
projection, world state, entity arrays and presentation. A mapping is accepted only where the
server authority relation is explicit; unsupported source regions remain unclassified rather than
being placed into a convenient ECS phase.

| Source anchor | Legacy order fact | ECS phase / owner | Status | Boundary |
|---|---|---|---|---|
| `Main.cs:11994-12012` | `UpdateTime` is called before `WorldGen.UpdateWorld` and `UpdateInvasion` | `ApplyWorldClock`: `WorldClockSystem`, `WorldWeatherSystem`, `WorldProgressionSystem` | partial | ECS does not claim full `WorldGen.UpdateWorld` parity |
| `Main.cs:11956-11988` | projectile and item array updates precede `UpdateTime` | `AdvanceProjectiles` / item systems are present, but order is not claimed | partial | Current schedule deliberately does not assert legacy array-loop parity; see `docs/research/2026-08-24-main-tick-order-reconciliation.md` |
| `Main.cs:12010-12012` | `WorldGen.UpdateWorld` precedes `UpdateInvasion` | supported invasion travel follows `ApplyWorldClock`; WorldGen remains unowned | partial | Only the supported invasion relation is accepted; complete WorldGen responsibility remains unknown |
| `Main.cs:13525-13536` | clock rate is computed, time advances, then time-dependent event systems run | `ApplyWorldClock`: immutable clock then rule/progression transitions | accepted | applies only to supported clock/rule transitions |
| `Main.cs:12958-13033` | invasion update uses post-time `dayRate` and spawn position | `WorldInvasionTravelSystem` after clock-rate resolution | accepted | Bounded `max(rate, 1)` travel and arrival warning have focused tick-order evidence |
| `Main.cs:11994-12012` | server update occurs after world/invasion update | `CommitDomainCommands` then `PublishSnapshot`, with `DomeServer` host loop owning Tick | accepted | host timing is adapter-boundary evidence; complete reconnect/protocol restart orchestration remains deferred |
| `Main.cs:12030-12048` | chest frames, ambient wind, camera and rendering follow server update | excluded / no Simulation phase | excluded | presentation must not enter Simulation |

## Current Proven Invariants

1. An active ECS tick executes the fixed schedule asserted by
   `Terraria.Dome.TickOrder.Verification`.
2. A paused ECS tick stops after `ApplyWorldClock` and publishes no gameplay snapshot.
3. Equivalent independent player inputs produce the same named phase trace and player snapshots.
4. Within the supported weather/progression slice, weather transition is evaluated from the current
   clock and the current/pending Lantern Night state before the progression transition commits.

The fourth invariant is an ECS authority decision, not proof that every legacy `UpdateTime` side
effect uses that same position. It is retained because the existing Lantern Night/rain card proves
the same-tick suppression predicate.

## Open Phase Work

- Restore `WorldGen.UpdateWorld` responsibility by source-backed domains, not as a monolithic phase.
- Establish invasion travel phase after its immutable state model exists.
- Compare entity death/loot/replication ordering per entity card.
- Extend server host/protocol timing coverage to reconnect and restart orchestration; the base Tick host boundary is accepted by `docs/research/2026-08-24-main-host-timing-boundary.md`.
- Do not move client rendering, ambient wind or camera behavior into Simulation.
