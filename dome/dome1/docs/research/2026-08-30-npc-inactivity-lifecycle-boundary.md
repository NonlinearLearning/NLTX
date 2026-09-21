# NPC inactivity lifecycle boundary (2026-08-30)

## Result

This batch verifies a narrow source-backed owner for the legacy NPC inactivity reads and its
timer gate. It is `verified` for the typed policy and lifecycle no-decrement branch, and remains
`partial/deferred` for the rest of `NPC.CheckActive` and full NPC parity.

## Legacy source evidence

Oracle: `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`.

- `NPC.cs:64323-64328` returns `true` only for `type == 668` in
  `DoesntDespawnToInactivityAndCountsNPCSlots`.
- `NPC.cs:64330-64436` returns `true` for 61 unconditional types. It also preserves type `139`
  only when `npcsFoundForCheckActive[134]` is set, and preserves types `552-563` and `566-578`
  only when `npcsFoundForCheckActive[548]` is set. Types `564` and `565` are in the unconditional
  branch between those conditional ranges.
- `NPC.cs:7129-7142` populates `npcsFoundForCheckActive` from active NPC instances each tick, so
  the companion facts are a runtime observation rather than a static definition capability.
- `NPC.cs:64438-64547` proves that the complete `CheckActive` method additionally depends on
  player rectangles, `Main` state, AI values, town/boss/event exceptions, `extraValue`, and worm
  segment cleanup. None of those dependencies are claimed here.

## ECS owner and boundary

`LegacyNpcInactivityRegistry` in `Npc/Definitions` owns the exact source type sets. It accepts an
NPC type and an explicit `IReadOnlySet<int>` of active typed net IDs, validates the `0..696` legacy
domain, rejects null/invalid observations, and exposes the type-668 slot-counting query separately.

`DomeSimulation.CreateActiveNpcTypes` derives the per-tick observation only from active
`NpcDefinitionComponent.NetId` values. Both existing lifecycle entry points pass the pure registry
result into `NpcLifecycleSystem.Advance`. The lifecycle gate skips only the positive-health
inactivity timer; lethal health still transitions to `Killed`, and `isImmortal` retains its
existing earlier priority. No NPC component, snapshot, protocol field, or save field was added.

## Verification evidence

All commands ran from the repository root with serial MSBuild settings:

- TDD RED: `Build/diagnostics/npc-complete/task-10-interaction/20260830-inactivity-lifecycle-red/verifier-build-red.log`, exit code `1`, with missing Registry/gate APIs.
- GREEN focused build: `.../20260830-inactivity-lifecycle-green/verifier-build.log`, exit code `0`.
- GREEN focused run: `.../20260830-inactivity-lifecycle-green/verifier.log`, exit code `0`, including
  `PASS: NPC source-backed inactivity registry and lifecycle gate`.
- Simulation Release build: `.../20260830-inactivity-lifecycle-green/simulation-build.log`, exit code `0`.
- Server Release build: `.../20260830-inactivity-lifecycle-green/server-build.log`, exit code `0`.
- NPC protocol shape build/run: `.../20260830-inactivity-lifecycle-green/npc-protocol-build.log` and `.../20260830-inactivity-lifecycle-green/npc-protocol.log`, both exit
  code `0`; the existing `SyncNPC` shape remains unchanged.
- Fresh rerun logs from the final revalidation are `verifier-build-rerun.log`,
  `verifier-rerun.log`, `npc-protocol-build-rerun.log`, `npc-protocol-rerun.log`,
  `simulation-build-rerun.log`, and `server-build-rerun.log` in the same directory.
- Final `git diff --check`, checkpoint JSON parse, and model-context JSON parse artifacts are kept
  beside these logs.

## Deferred boundary

Player `safeRange`/`activeRange` scans, `Main.player` state, type-specific AI and event/boss
exceptions, `noSpawnCycle`, revenge caching, `CheckActive_WormSegments`, collision/recovery,
network/persistence projection, `WorldGen.UnspawnHomelessNPC`, legacy source deletion, and full
NPC/AI parity remain `partial` or `deferred`. `canRemoveLegacyWorldGen` remains `false`.
