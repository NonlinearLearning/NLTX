# NPC `checkDead` special transform/spawn transition owner (2026-08-30)

## Result

This batch adds a typed, pure owner for the four source-backed special transitions at the
beginning of `NPC.checkDead`. It preserves the state reset and one child-spawn intent for the
396/397 branch without touching `Main`, random state, loot, events, or network transport. The
owner is verified as a narrow death-transition contract and remains `partial` for NPC migration.

Overall status remains:

> **NPC field/property migration: partial.**

## Legacy source contract

The source oracle is `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`. The current full
source SHA-256 is
`29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17`.
The immutable transition excerpt (`NPC.cs:64587-64628`) has UTF-8/LF-normalized SHA-256
`90DC3CC1EB2D284D97546F7D5AF57300B1428DDAE11EFFDC45465842A4E43F75`.

The relevant source order is:

1. `NPC.cs:64587-64602` handles types `397` and `396`. When `ai[0] != -2f`, it sets
   `ai[0] = -2f`, restores `life = lifeMax`, sets `netUpdate = true` and
   `dontTakeDamage = true`, then requests type `400` at the integer-truncated NPC center.
   The new child receives the parent's `ai[3]` and a `netUpdate` intent. The branch returns even
   when `ai[0]` is already `-2f`.
2. `NPC.cs:64603-64610` handles type `398`. When `ai[0] != 2f`, it sets `ai[0] = 2f`, restores
   `life = lifeMax`, sets `netUpdate = true` and `dontTakeDamage = true`, then returns.
3. `NPC.cs:64611-64619` handles types `517`, `422`, `507`, and `493`. When `ai[2] != 1f`, it
   sets `ai[2] = 1f`, `ai[1] = 0f`, restores `life = lifeMax`, sets `dontTakeDamage = true`
   and `netUpdate = true`, then returns.
4. `NPC.cs:64620-64628` handles type `548`. When `ai[1] != 1f`, it sets `ai[1] = 1f`,
   `ai[0] = 0f`, restores `life = lifeMax`, sets `dontTakeDamageFromHostiles = true` and
   `netUpdate = true`, then returns.

The preceding `checkDead` qualification (`NPC.cs:64575-64577`) is a separate owner from Batch
N3.134. This owner accepts only its already-qualified `life <= 0` input, so it does not reorder or
duplicate that entry guard.

## Typed owner

The implementation is in `src/Terraria.Dome.Simulation/Npc/Systems`:

| Type | Responsibility |
| --- | --- |
| `NpcCheckDeadSpecialTransitionInput` | Qualified NPC identity, type, life/maximum-health, `ai[0..3]`, center, and existing flags. |
| `NpcCheckDeadSpecialTransitionState` | Immutable post-transition life, AI values, damage flags, and net-update intent. |
| `NpcCheckDeadSpecialTransitionKind` | Explicit branch classification for none, 396/397, 398, 517-family, and 548. |
| `NpcCheckDeadSpawnIntent` | Source-attributed type-400 child request, integer-truncated position, child `ai[3]`, and child net-update intent. |
| `NpcCheckDeadSpecialTransitionDecision` | Whether `checkDead` should return, branch kind, resulting state, and optional child request. |
| `NpcCheckDeadSpecialTransitionPolicy` | Pure source-order transition query with bounded/finiteness validation. |

The policy requires a valid NPC handle, a Version-1456 NPC type in `0..696`, positive maximum
health, qualified non-positive life, finite AI values and a center representable by the source
integer cast. It preserves existing flags for unrelated or already-terminal branches. The
396/397 child position uses truncation toward zero, matching `(int)base.Center.X/Y`; the intent is
not an allocator result and does not claim that a slot was created. For these two types the
decision reports `ShouldReturnFromCheckDead` even when `ai[0]` is already `-2f`, because the
legacy outer branch returns unconditionally. The other three transition families return only
when their source predicate changes state; an already-terminal value falls through to later
`checkDead` logic.

## Verification

- TDD RED: `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkdead-special-transition-red/`
  records verifier build exit `1`, with only missing special-transition owner/API symbols.
- TDD GREEN: `Build/diagnostics/npc-complete/task-10-interaction/20260830-checkdead-special-transition-green/`
  records the focused verifier build and run at exit `0`; the run includes
  `PASS: NPC checkDead special transform/spawn policy` and no `FAIL`/`ERROR` lines.
- Fresh serial Simulation, Protocol, NPC verifier, and Server Release rebuilds, the Release NPC
  verifier run, source/line audit, style check, and `git diff --check` are recorded under
  `20260830-checkdead-special-transition-final-rerun-03/`.
- The new research file and this batch's diagnostic directory are intentionally not added to the
  Flowstate manifests in this slice. `manifest-reconciliation-deferred.log` records the current
  count delta and the separate-batch decision; `GenerateFlowstateManifest.ps1` was not run.

## Deliberate stop boundary

This owner is not wired into `DomeSimulation`, `NpcDeathSystem`, `NpcLifecycleSystem`, or the
spawn allocator. It does not mutate `NpcBehaviorStateComponent`, `NpcLifecycleComponent`,
`Main.npc`, or network state, and it does not perform the type-400 spawn, child initialization,
message-23 send, Good World branch, town tombstone/announcement, loot, event progression,
invasion accounting, or final `active=false` transition. The separate `noSpawnCycle` writer at
`NPC.cs:64670` remains deferred. Complete `checkDead`, death/loot integration, AI/event/boss
parity, protocol/persistence projection, legacy deletion, and `canRemoveLegacyWorldGen` remain
`partial`/`deferred`.
