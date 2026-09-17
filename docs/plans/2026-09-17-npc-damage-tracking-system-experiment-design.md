# NPC Damage Tracking System Experiment Design

**Status:** Experimental branch. This document records a focused ECS boundary
experiment; it does not claim a complete Version4 migration, runtime behavioral
equivalence, or test execution. This continuation intentionally adds no test
code and runs no tests.

## Scope And Evidence

The selected authoritative member is P12/C17, `NpcDamageRuntimeTracking`,
from the completed P12 report:

`docs/组件文档/迁移参考表/Version4权威模拟系统字段属性逐成员源码声明-20分区/P12-NPC-Combat-Network-Damage.md`

P12 currently reports `189/189` observed members and session
`dec7d02830c14d7bbba328dc0317cef3`. The C17 member set is the nine fields
`_activeTrackers`, `_recentFinishedTrackers`, `MAX_RECENT_TRACKERS`,
`EXTRA_RECENT_TRACKER_EXPIRY_TIME`, `_list`, `_worldCredit`, `_lastAttacker`,
`_ticks`, and `_lastHitTime`, plus `IsEmpty`, `Duration`, and
`TimeSinceLastHit`.

The read-only Version4 source is
`D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs`, with
SHA-256
`1B6812046FE624470BF51B48EE542EB8836D270CA60EC77BCCAB5A9F9E3DD8B0`.
The source establishes these concrete behaviors:

- active and recent registries are process-static in Version4;
- recent history is capped at three and expires only while more than one
  recent tracker remains, using a strict `>` comparison against `54000`;
- contributor entries preserve first-seen order, aggregate world damage, and
  update last attacker and last-hit time;
- damage credit is recorded after the caller has chosen the accepted amount;
  the caller-side `Math.Min(damage, npc.life)` is outside this experiment;
- Boss/composite strategies stay active while any configured member type is
  active, and `OnBossKilled` records killed state;
- `InvasionDamageTracker.IncludeDamageFor` is currently an empty generated
  body (`return new bool();`), so invasion attribution is not guessed here.

The actual Dome call closure is in the untracked working copy
`dome/dome1`. `DomeSimulation.CommitNpcDamageCommands` currently discards the
resolved `appliedAmount`; lifecycle, persistence, network, and main-loop
integration are therefore evidence gaps for this experiment.

## Boundary Decision

The experiment compares three choices:

1. **Keep:** retain the current legacy-shaped tracker boundary. This preserves
   the static registry problem and does not exercise the requested system
   split.
2. **Partial:** divide one class into source files. This improves navigation
   but creates no independent ownership or review seam.
3. **Separate:** use one world/session-scoped `NpcDamageTrackingSystem` as the
   registry and commit owner, one `NpcDamageEncounterTracker` per encounter,
   explicit strategy objects for Boss/composite behavior, and defensive
   snapshots for readers.

Option 3 is selected. The system owns active/recent registry state and the
encounter tracker owns the reused `EncounterDamageCreditComponent` state. A
second credit component is deliberately not introduced.

## Target Ownership

`NpcDamageTrackingSystem` owns:

- the active and recent tracker lists;
- encounter identity allocation within one world/session instance;
- acceptance of already-resolved positive `appliedAmount` values;
- `MarkKilled`, explicit `StopTracking`, time advancement, recent cap/expiry,
  reset, and snapshot projection;
- the sole mutation path into an encounter tracker.

`NpcDamageEncounterTracker` owns:

- one encounter's `EncounterDamageCreditComponent`;
- ordered contributor credit, world damage, last contributor, start/last-hit
  ticks, lifecycle, and revision;
- `IsEmpty`, `Duration`, and time-since-last-hit calculation;
- forwarding the explicit kill signal to its strategy.

`INpcDamageTrackingStrategy` owns only dynamic target grouping and killed-state
policy. `NpcDamageSingleTypeStrategy` and
`NpcDamageCompositeStrategy` are concrete implementations. A strategy factory
must return a fresh strategy for each newly created encounter; it receives the
initial `NpcTypeId` and has no access to `Main`, static NPC arrays, a clock, or
I/O.

The existing `EncounterDamageCreditComponent` remains the canonical per-
encounter component. Its new controlled mutation method validates lifecycle
and tick ordering, while its credit projection is defensively copied.

## Contract

The system requires callers to establish a current tick with
`AdvanceTo(tick, activeNpcTypes)`. `TryRecordAppliedDamage` then requires the
same tick. A future or non-monotonic tick is rejected, making the phase
contract visible instead of silently advancing time during damage commit.

The public API is intentionally narrow:

- `TryRecordAppliedDamage(type, contributor, appliedAmount, tick)` records only
  positive, already-accepted damage and creates an active tracker on demand;
- `AdvanceTo(tick, activeNpcTypes)` applies target activity, active-to-recent
  transfer, recent cap, and expiry;
- `MarkKilled(type, tick)` updates the first matching active encounter;
- `StartTracking(type, strategy)` supports explicit event-started encounters;
- `StopTracking(encounterId, tick)` is idempotent and returns `false` when the
  encounter is no longer active;
- `GetActiveSnapshots` and `GetRecentSnapshots` return new arrays whose credit
  memories are copied from component state;
- `Reset` clears registries and preserves the caller-owned world clock.

The snapshot contains encounter identity, initial type, credit entries,
world damage, last contributor, start/last-hit ticks, `Duration`,
`TimeSinceLastHit`, `IsEmpty`, lifecycle, active/recent flags, killed state,
and revision. Snapshot mutation cannot write back to the system.

## Read/Write/Emit Sets

| Boundary | Read set | Write set | Emit set |
| --- | --- | --- | --- |
| `NpcDamageTrackingSystem` | explicit tick, active NPC type set, applied damage event, contributor identity, strategy factory | active/recent registry, encounter IDs, tracker lifecycle | immutable snapshots; no network or file effect |
| `NpcDamageEncounterTracker` | contributor, accepted amount, tick, strategy result | one `EncounterDamageCreditComponent` | snapshot data to owner system |
| strategy | NPC type and explicit active-type set | killed flag only | boolean policy result |
| `EncounterDamageCreditComponent` | contributor, positive amount, hit tick | ordered credit array, world damage, last contributor, last-hit tick, revision/lifecycle | defensive credit memory |

There is no random source, external clock read, file I/O, network write,
persistence write, logging callback, or background task in the experiment.
The caller owns health resolution and must pass the amount actually accepted.

## Scheduling And Failure

The intended schedule is:

```text
NPC identity/health and damage resolution
  -> accepted appliedAmount
  -> NpcDamageTrackingSystem.TryRecordAppliedDamage
  -> lifecycle/death result
  -> NpcDamageTrackingSystem.MarkKilled or AdvanceTo
  -> credit snapshot / later adapter
```

The damage-resolution commit must happen before a credit record; lifecycle
activity must be known before the next `AdvanceTo`; snapshot consumers run
after the system's commit point. Calls within one tick are ordered by their
explicit call order. No new scheduler phase or command queue is invented by
this prototype.

Invalid contributor provenance, invalid types, future ticks, backwards ticks,
and a strategy that does not include its initial type are rejected. A zero or
negative accepted amount is ignored without creating a tracker. Overflow in
credit or revision arithmetic is allowed to fail before the corresponding
mutation is committed. Reset drops in-memory tracker state; no persistent or
network state is changed.

## Migration And Rollback

This is a focused source experiment. The safe migration order for real Dome
integration is: capture the accepted amount, feed an isolated tracker system
in shadow mode, compare snapshots against a legacy golden trace, switch the
single credit commit owner at a barrier, then add persistence/network
projections. Health, loot, UI, localization, and packet side effects remain
outside this branch.

Rollback is removal/revert of the experiment commit and reactivation of the
legacy writer. A production rollback would also need to discard or version
shadow tracker state and define compensation for any already-published credit
projection. Because the current experiment has no external effect, its local
rollback is loss of in-memory snapshots only.

The Version4 deletion gate is not applicable: the old tracker is not removed,
the Dome main loop is not switched, and network/persistence equivalence is not
verified.

## Verification Scope

Per the current task scope, no focused verifier or test code was added and no
test command was run. Evidence for this continuation is limited to source
review against the P12 report and Version4 snapshot, plus a serial build of
`src/Npc/Terraria.Npc.csproj`. That build checks type and project closure only;
it does not prove local state-transition invariants, Version4 equivalence, Dome
integration, network/persistence behavior, or deletion readiness.
