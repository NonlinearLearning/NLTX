# NPC `CheckActive` timeout deactivation and spawn-cycle gate owner (2026-08-30)

## Result

This batch adds a typed, pure owner for the timeout/deactivation decision in `NPC.CheckActive`
and an explicit consume-once owner for the static `noSpawnCycle` marker. It is verified as a
narrow lifecycle/spawn query and remains `partial` for NPC migration. It does not claim that
`CheckActive`, `SpawnNPC`, or the NPC lifecycle pipeline has moved to Simulation.

Overall status remains:

> **NPC field/property migration: partial.**

## Legacy source contract

The source oracle is `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`, SHA-256
`29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`.

The relevant source order is:

- `NPC.cs:6053` declares the static `noSpawnCycle` marker, initially `false`.
- `NPC.cs:64529` decrements `timeLeft` after the player-loop keep-alive checks.
- `NPC.cs:64530-64533` clears the keep-alive result when the decremented timer is non-positive.
- `NPC.cs:64534-64545` enters the deactivation branch when no keep-alive remains, sets
  `noSpawnCycle = true`, `active = false`, and `life = 0`, then sends SyncNPC, optionally caches
  revenge state, and cleans worm segments.
- `NPC.cs:66513-66519` checks `noSpawnCycle` before `RevengeManager.CheckRespawns()` and
  `Spawner.SpawnNPC()`. A marked cycle is cleared and returned from exactly once.

`checkDead` contains another `noSpawnCycle = true` write at `NPC.cs:64670`. That writer is not
part of this batch; the typed owner covers only the timeout writer and its consume-once gate.

The deactivation owner therefore treats `noSpawnCycle` as a world-level spawn-cycle gate, not an
NPC entity field. It receives the already-separated keep-alive and slot-counting results; it does
not reconstruct the fixed `Main.player[0..254]` scan.

## Typed owners

| Type | Responsibility |
| --- | --- |
| `NpcCheckActiveDeactivationInput` | Explicit active state, keep-alive result, type-668 slot-counting bypass, timer, and life. |
| `NpcCheckActiveDeactivationDecision` | Immutable post-decrement timer, active/life result, and skipped-cycle intent. |
| `NpcCheckActiveDeactivationPolicy` | Pure source-order decrement/deactivation query with fail-closed lifecycle validation. |
| `NpcSpawnCycleStateComponent` | Explicit mutable world gate that marks and consumes one skipped spawn cycle. |

For a non-slot-counting active NPC, the policy decrements `TimeLeft` once. It keeps the NPC active
only when a keep-alive result remains and the decremented timer is positive. Otherwise it returns
`IsActive=false`, `Life=0`, and `ShouldSkipNextSpawnCycle=true`. Inactive and type-668
slot-counting inputs are returned unchanged without processing. Negative timer/life values are
rejected rather than allowed to flow into an unchecked transition.

The deactivation policy is pure. `NpcSpawnCycleStateComponent` is intentionally mutable, but its
mark/consume operations mutate only its own explicit gate state.

## Verification

- TDD RED: `20260830-checkactive-deactivation-red/npc-verifier-build-red.log` records exit `1`
  with the expected missing deactivation/gate symbols.
- TDD GREEN: the Simulation and NPC verifier builds in
  `20260830-checkactive-deactivation-green/` exit `0`; the verifier run exits `0` with 20 PASS
  lines, including the new timeout/deactivation gate.
- Protocol and Server Release builds also exit `0` with zero warnings/errors in the same green
  directory.
- Fresh final serial gates, source hash/line audit, style check, and `git diff --check` are under
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-deactivation-final/`.
  The final directory also contains the Flowstate document check, state-boundary JSON, and
  `verification-status.txt` for this batch.
- A fresh post-documentation rerun is recorded under
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-deactivation-final-rerun-02/`.
  Its `exit-codes.txt`, `simulation-build.log`, `protocol-build.log`,
  `npc-verifier-build.log`, `npc-verifier-run.log`, and `server-build.log` record all five
  serial gates at exit `0`; the verifier run again has 20 `PASS` lines and no `FAIL`/`ERROR`
  lines. The latest Flowstate document gate is recorded in
  `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkactive-deactivation-final/flowstate-docs-verification-rerun.log`
  as `PASS docs=420 manifest=420 buildReferences=1234`.

## Deliberate stop boundary

This owner is not wired into `DomeSimulation`, `NpcLifecycleSystem`, or the Simulation spawn loop.
The separate `checkDead` writer at `NPC.cs:64670` also remains deferred.
The batch does not add fixed player-slot storage, `nearbyActiveNPCs`, generic AI/type-690/boss/event
rules, SyncNPC projection, `RevengeManager.CacheEnemy`, worm cleanup, collision/recovery,
persistence, network scheduling, or full NPC parity. Existing lifecycle timeout and death owners
remain separate until an end-to-end source-aligned commit chain is proven. `canRemoveLegacyWorldGen`
remains `false` and all complete `CheckActive`/NPC migration claims remain deferred.
