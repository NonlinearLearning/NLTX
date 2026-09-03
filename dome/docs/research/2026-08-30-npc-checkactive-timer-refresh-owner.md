# NPC `CheckActive` screen-range timer-refresh owner (2026-08-30)

## Result

This batch adds a typed, pure owner for the screen-range timer reset in `NPC.CheckActive`. It is
verified as a narrow lifecycle query and remains `partial` for NPC migration. It does not claim
that the complete `CheckActive` method has moved to Simulation.

Overall status remains:

> **NPC field/property migration: partial.**

## Legacy source contract

The source oracle is `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`, SHA-256
`29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`. The relevant source order
is `NPC.cs:64446-64480`:

1. `DoesntDespawnToInactivityAndCountsNPCSlots()` is evaluated before the player loop. This is
   the type-668 slot-counting exception; it still scans players but returns before timer decrement.
2. For each active player, `NPC.cs:64476-64480` checks the separately constructed screen-range
   rectangle. A hit sets `timeLeft = activeTime` and `despawnEncouraged = false`.
3. The reset is repeated for every matching active player and is therefore idempotent. No hit,
   an inactive NPC, or the slot-counting branch leaves both fields unchanged.
4. `NPC.cs:6055` defines `activeTime = 750`. The geometry that produces the hit is owned by the
   separate `NpcActivityRangePolicy` boundary; this batch consumes its boolean result rather than
   reimplementing coordinate conversion.

## Typed owner

| Type | Responsibility |
| --- | --- |
| `NpcCheckActiveTimerRefreshInput` | Explicit NPC active state, screen-range hit, type-668 bypass, configured active time, current timer, and despawn flag. |
| `NpcCheckActiveTimerRefreshDecision` | Immutable refresh decision and resulting timer/flag values. |
| `NpcCheckActiveTimerRefreshPolicy` | Pure source-order query with `Version1456ActiveTime = 750`; rejects non-positive active-time configuration. |

The policy returns the caller's current timer and flag when any gate blocks refresh. When all gates
pass, it returns `ShouldRefresh = true`, the configured active time, and
`DespawnEncouraged = false`. It does not mutate a lifecycle component, player collection, or range
decision.

## Deliberate stop boundary

This owner is not wired into `DomeSimulation` or `NpcLifecycleSystem`, because the repository still
lacks a source-proven fixed `Main.player[0..254]` mirror and a complete deterministic
`CheckActive` commit chain. It does not add:

- pixel-to-tile conversion, player-slot storage, or `nearbyActiveNPCs` mutation;
- generic AI slots, type-690 behavior, boss/event rules, or type registries;
- `noSpawnCycle`, `active`, `life`, SyncNPC, revenge, worm cleanup, collision/recovery, or unspawn;
- network/persistence projection or complete NPC/AI parity.

Therefore full `CheckActive`, timer/deactivation integration, player accounting, event/Boss/AI
behavior, network/save parity, and legacy WorldGen deletion remain `partial`/`deferred`, and
`canRemoveLegacyWorldGen` remains `false`.

## Verification

- TDD RED: `20260830-checkactive-timer-refresh-red/npc-verifier-build-red.log` records exit `1`
  with the expected missing owner symbols.
- TDD GREEN: the Simulation, Protocol, NPC verifier, focused run, and Server Release logs in
  `20260830-checkactive-timer-refresh-green/` all record exit `0`; the focused run includes
  `PASS: NPC CheckActive screen-range timer-refresh policy`.
- Final rerun and hygiene evidence are under
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-timer-refresh-final/`.
- A fresh rerun after concurrent repository progress is under
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-timer-refresh-final-rerun-01/`;
  it again records all five serial gates at exit `0`, `19 PASS` verifier lines, and a boundary
  check that leaves the latest N6 active-batch state untouched.
