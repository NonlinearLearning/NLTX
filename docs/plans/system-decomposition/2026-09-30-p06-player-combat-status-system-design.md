# P06 Player Combat Status System Design

**Status:** `proposed`  
**Evidence:** `partial`  
**Verification:** `not-run`  
**Source modified:** `true` (core slice, including `PlayerLifeRegenQuery`)

This document turns the authoritative P06 System report into a design proposal and records a
small, explicitly bounded core implementation slice. It does not claim that the P06 slice has
been migrated, integrated, behaviorally equivalent, or deletion eligible.

The current continuation reads the settled report, the read-only CPG index, Version4, the complete
reference project, and the structural SS14 reference, then records the isolated core slice under
`src/NSSLC` and its explicit-closure verifier. It does not modify runner state, the Version4 tree,
or the complete reference project. A focused verifier run is preserved as historical evidence;
the latest `Resting` ordering assertion was added after that run and was not rebuilt or rerun in
this documentation pass. The document-level `Verification: not-run` state therefore remains the
only current verification claim for full P06 behavior.

## Scope And Claim

The design is limited to the claimed authoritative partition:

- partition: `P06`
- task: `AUTH-SYS-P06`
- runner session: `791756dc1f954ff3afcd84b5ef3fe296`
- claim: `PlayerGameplay`, 22 leaf groups, 254 fields, 0 properties
- authoritative report: `D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P06-player-combat-status.md`
- target migration area: `D:\TRbackup\NLTX\src\NSSLC`

The 254-member inventory is a scope boundary, not a statement that every member belongs in one
System. The report's ownership and API decisions remain candidates until an implementation reaches
the real runtime entry points and behavior tests.

## Evidence Order And Version Binding

Evidence is applied in this order:

1. `D:\TRbackup\Version4` is the authoritative P06 baseline. Direct source facts, call sites, and
   local method order from this tree outrank names, generated candidates, or framework examples.
2. The read-only CPG query API supplements relationship discovery. Its database is
   `D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite`, with manifest SHA-256
   `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`. The import is complete for
   967 shards, but `SourceSnapshotId=null`; selected-shard facts therefore remain bounded by the
   queried source range and do not close dynamic dispatch, aliases, reflection, hooks, event
   registration, callee effects, or scheduler order.
3. The complete reference project at
   `D:\TRbackup\无任何删减通过编译` supplements only files and members that also exist at the
   same path in Version4. It is evidence for recovering Version4's removed or empty bodies, not a
   second P06 inventory and not a migration target. Its solution and project are
   `TerrariaServer.sln` and `TerrariaServer.csproj`.
4. `C:\Users\shan\Downloads\ECS\space-station-14-master` is a structural reference only. Its
   Damageable and StatusEffect systems support the idea of a coordinating System with explicit
   snapshots and effects; their entity, scheduler, damage, and status semantics are not imported.

### Source identities

| Source | File | SHA-256 / binding | Use in this design |
|---|---|---|---|
| Version4 baseline | `Terraria/Player.cs` | `E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86` | authoritative local behavior and P06 members |
| Version4 baseline | `Terraria/MessageBuffer.cs` | `0F474225FA9D98C539F61249619273A44E73F69A4CF388B44258433A2D15D5EE` | inbound resource packet behavior |
| Version4 baseline | `Terraria/NetMessage.cs` | `87B596BD8467B9C470DD1F2AD6963EBADE257689F87395163AB136C149BEBC9A` | outbound resource packet behavior |
| Version4 baseline | `Terraria/Projectile.cs` | `97C3D68586AEF862411699E428F5C4B18CEE1F8051B684BE9F86FA7B60FAE83B` | selected damage callers and proc relationships |
| Complete reference supplement | `Terraria/Player.cs` | `367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C` | same-path body recovery only |
| Complete reference supplement | `Terraria/MessageBuffer.cs` | `48AABBBF4E0967964D97598E0168E9252ADD3DD868C66C6DA46AA87670EACAAB` | same-path packet comparison |
| Complete reference supplement | `Terraria/NetMessage.cs` | `F3066C50715D7C49B8BF2CC852B015303AD7F4C12ADC191C72FC0FE46181E7E2` | same-path packet comparison |

The complete reference project has no authority to add files or fields to the P06 denominator. Any
reference-only file or member must be recorded as `outside-Version4-baseline` and excluded from the
254-member coverage calculation.

## Goals And Non-Goals

### Goals

- Establish one proposed authority for each P06 invariant and one commit point for each mutable
  resource or combat result.
- Map legacy `Player` entry points to conceptual behavior compositions instead of forcing a
  one-method-to-one-System translation.
- Preserve the source-observed tick order, accepted versus rejected hit behavior, resource caps,
  packet field order, and persistence version boundaries once those behaviors are executable.
- Make cross-partition decisions explicit and hand them to `integration-review` rather than hiding
  them inside a P06 System.
- Define an observation vector and a staged execution plan that can later produce real behavior
  evidence.

### Non-goals

- No claim that the partial core implementation is the production P06 route or a complete
  behavior migration.
- No changes to the Version4 baseline, the complete reference project, the runner state, the
  settled report, or the 254-member denominator.
- No claim that current `src/NSSLC` Systems are wired to the Version4 gameplay loop.
- No inferred behavior for an empty Version4 method body.
- No use of the complete reference project to enlarge P06 scope.
- No full P06 build, integration test, publish, or deletion gate is claimed here.

## Implemented Core Slice (Partial)

The following implementation was added under `D:\TRbackup\NLTX\src\NSSLC\Component\Player` as
an isolated core slice. It is a candidate composition, not evidence that the legacy runtime now
routes through it:

- `PlayerCombatResolutionSystem` is the single synchronous hit submission boundary for the
  slice. It validates event/source identity, source revision, duplicate event IDs, general
  immunity, source cooldown, and damage positivity before the commit point.
- `PlayerDamageEligibilityQuery` and `PlayerDamageMitigationQuery` are read-only rules. The
  mitigation fixture applies defense, endurance, critical doubling, and a minimum damage of one.
- Shadow dodge is consumed before the Vital write and produces no accepted proc or DPS fact.
- An accepted hit is the only path in this slice that writes `PlayerVitalStateComponent.StatLife`;
  it publishes committed proc and damage facts after the write and reports lethal state.
- `PlayerStatusEffectSystem` owns the selected 44-slot status representation for this slice. It
  handles known-definition application, immune rejection, duplicate refresh, additive caps, first
  known non-debuff eviction, explicit tick decrement, expiry compaction, and defensive snapshots.
- `PlayerManaRegenSystem` owns the ordinary delay/count/cap mana path for this slice. It leaves the
  Nebula increment branch and scheduler visibility as explicit inputs/gaps.
- `PlayerLifeRegenQuery` now computes the selected Version4 counter path without writing live life.
  It covers status penalties, timer modifiers, positive healing thresholds, ordinary negative
  damage plans, and the burned/suffocating special damage threshold. The later life commit remains
  unresolved because Version4 `HurtLifeRegen` is empty.
- Paladin recursion, P04 death/respawn handoff, full status effect catalog/effects, network,
  persistence, presentation, knockback, sound, and particle effects remain unimplemented or
  unresolved.

This slice covers a narrow combat-resolution path (approximately the requested 10% exploratory
scope). It must not be counted as 10% member coverage of the 254-field P06 inventory.

## Core Slice Verification Record

The focused verifier project is
`D:\TRbackup\NLTX\src\NSSLC\Component\PlayerCombatResolutionVerification\Terraria.PlayerCombatResolutionVerification.csproj`.
It uses an explicit source closure so unrelated Mount inputs in the full `Terraria.Player`
project do not become hidden dependencies.

An earlier core-only run used the repository wrapper with `--no-build --no-restore` and produced:

```text
PASS: player combat resolution core ownership, rejection and commit semantics
EXIT_CODE=0
```

The historical serial build completed with zero warnings and zero errors and produced
`D:\TRbackup\NLTX\Build\bin\Terraria.PlayerCombatResolutionVerification\Debug\net10.0\Terraria.PlayerCombatResolutionVerification.dll`.
That build predates the latest `Resting`-after-`3600` ordering assertion; the commands below are
recorded for reproducibility and were not executed again in this documentation pass.
The verifier covers mitigation (`100` damage, `10` defense, `0.25` endurance -> `67`), normal
commit, duplicate rejection, general immunity, shadow-dodge consumption, critical lethal damage,
  proc/DPS publication, status refresh/cap/immune rejection/full-table eviction/tick expiry, the
  life-regen query core (healing threshold, Poisoned, ordinary damage, and Burned plan), and
  ordinary mana delay/count/cap behavior. The current output is:

```text
PASS: player combat, status-slot, life-regen and mana-regen core semantics
EXIT_CODE=0
```

The `Resting` ordering assertion is present in the current verifier source but is not covered by
the historical output above. It remains `not-run` until a later explicitly authorized build/run.

This is narrow evidence only; the document-level `Verification` field therefore remains `not-run`
for full P06 behavior.

## Additional Relationship Evidence

The read-only CPG query was re-run for the status entry points. It resolved
`Player.AddBuff(int,int,bool)` exactly and returned 19 selected call sites across
`Terraria/Player.cs` and `Terraria/Projectile.cs`; it resolved `Player.DelBuff(int)` and its
selected callers. `UpdateBuffs(int)` had no matching call fact in the selected indexed range, so
that relationship remains `partial` and was checked against the Version4 and complete-reference
source rather than inferred from the zero result. The imported CPG snapshot is still unbound
(`SourceSnapshotId=null`), so these facts do not prove runtime closure or scheduler order.

A second read-only query pass checked the life-regen seam. `Find-CpgSymbols` resolved
`UpdateLifeRegen()` and `HurtLifeRegen(int)` exactly in `Terraria/Player.cs`.
`Find-CpgCallSites(HurtLifeRegen)` returned three exact static call sites in the selected
`Player.cs`/`MessageBuffer.cs`/`Projectile.cs` scope, all in `Player.cs`. The corresponding
`Find-CpgCallSites(UpdateLifeRegen)` query returned `partial` with zero facts and
`NoMatchingFactInScannedScope`; the direct source call at `Version4/Terraria/Player.cs:10847`
therefore remains the authority and the zero result is not treated as “no caller”.
`Get-CpgMemberUses(statLife)` returned 31 selected-scope facts, including `Write`, `ReadWrite`,
`Unknown`, and `partial` access classifications. These results retain the query manifest and
`SourceSnapshotId=null` limits above.

## Source-Observed Behavior

The following facts are local source observations, not proposed ECS behavior:

- `Player.Update` orders `ResetEffects` (`Player.cs:15207`), `UpdateBuffs` (`:15239`),
  `UpdateEquips` (`:15296`), `UpdateLifeRegen` (`:15599`), and `UpdateManaRegen` (`:15601`).
- `ResetEffects` (`:10272`) writes derived values including `statDefense` and `lifeRegen`.
- `AddBuff` (`:3541`), `DelBuff` (`:3697`), and `UpdateBuffs` (`:4300`) own local slot, immunity,
  replacement, and timer behavior.
- `Hurt` (`:22208`) coordinates eligibility, mitigation, resource changes, immunity, knockback,
  effects, and lethal handling. A Paladin path can re-enter `Hurt`.
- `UpdateLifeRegen` (`:10847`) computes counters and calls `HurtLifeRegen` (`:11238`). The
  Version4 body is empty, so the final life commit is `unknown` from the authoritative tree.
- `UpdateManaRegen` (`:11239`) contains a visible delay/count/cap algorithm and writes `statMana`.
- `AllowShimmerDodge` (`:22568`) is empty in Version4, so the shimmer dodge oracle is `unknown`.
- `addDPS` (`:26245`) uses `DateTime.Now`; changing it to a game tick is a behavior decision, not
  a mechanical API rename.
- Version4 `NetMessage` writes life/max as `Int16` at `:513-514` and mana/max at `:957-958`.
  Version4 `MessageBuffer` reads life/max at `:778-784`, derives `dead` from life, and reads
  mana/max at `:1792-1795`.
- Version4 `InternalSavePlayerFile` (`:26417`), `Serialize` (`:26418`), and `Deserialize`
  (`:26455`) are empty. Their field set, version compatibility, and failure recovery are
  `unknown` in the authoritative tree.

The CPG query found the `Terraria.Player` surface (1,511 members), 31 selected-range uses of
`statLife`, and five selected-range static call sites for `Player.Hurt`. `Player.Hurt` callable
facts are `partial` with `CalleeEffectsNotExpanded`. These results are relationship evidence within
the selected shards, not a closed runtime graph.

## Proposed System Boundaries

Names below are design candidates. They do not assert that an identically named type is already
implemented or registered in `src/NSSLC`.

| Proposed boundary | Authority and responsibility | Explicitly out of its write set | Status |
|---|---|---|---|
| `PlayerCombatResolutionSystem` | One synchronous accepted-hit commit: eligibility result, mitigation result, vital delta, immunity/cooldown commit, accepted proc handoff, lethal handoff | Buff definitions, Item/NPC/Projectile object state, presentation, network encoding, persistence | `proposed`; effect closure `partial` |
| `PlayerStatusEffectSystem` | Buff/debuff slot commands, immunity/replacement, timer tick, expiry and status snapshot | Direct life/mana writes, item definitions, network packets, presentation frames | `proposed`; catalog and external effects `partial` |
| `PlayerCombatCapabilityRebuildSystem` | Reset derived combat capability and rebuild from immutable equipment/status/environment snapshots | Durable counters, vital resources, Item objects, random effects, network/persistence | `proposed`; inputs and scheduling `partial` |
| `PlayerManaRegenSystem` | Delay/count algorithm and bounded mana result, committed through the selected Vital authority | Life DoT commit, Buff slot ownership, packet serialization | `proposed`; unique Vital owner `partial` |
| `PlayerLifeRegenQuery` plus a later commit owner | Compute the visible `UpdateLifeRegen` result without inventing the missing `HurtLifeRegen` effect | No direct resource write until the source gap is resolved | `partial core implementation`; commit `unknown` |
| `PlayerDpsTelemetrySystem` | Record committed damage, preserve start/end/last-hit/damage semantics through a clock port, expose a read projection | Combat authority, wall-clock replacement policy, network/persistence authority | `proposed`; clock/readers `partial` |
| Protocol adapter | Encode/decode the existing life/mana packet shapes and route commands to the authoritative resource owner | Domain calculations and duplicate resource writes | `crossSubsystemOwner: integration-review` |
| Persistence adapter | Versioned player snapshot codec, encryption/file/cloud boundary, load validation and recovery | Combat rules and direct component mutation before validation | `crossSubsystemOwner: integration-review` |

Creating one System per inventory leaf is rejected. The 22 groups are an inventory and evidence
surface; they cross Item, Movement, Environment, NPC, Projectile, World, Presentation, Network,
and Persistence boundaries. A catch-all PlayerGameplay System is also rejected because it would
hide ownership and make a unique writer impossible to review.

## State Ownership And Write Rules

The implementation must select one authoritative Vital representation from the existing NLTX
candidates (`PlayerVitalComponent`, `PlayerVitalState`, `PlayerVitalStateComponent`, or another
explicitly reviewed type). The report observed multiple candidate shapes and no closed production
writer. The following rules apply regardless of the eventual type name:

1. `statLife`, `statLifeMax`, `statLifeMax2`, `statMana`, `statManaMax`, and `statManaMax2` have one
   committed writer per world/session scope.
2. A Query, Projection, Network adapter, or Persistence adapter never writes a live authority.
3. A rejected or dodged hit cannot commit accepted-hit proc, life, immunity, telemetry, or packet
   effects unless the legacy behavior explicitly does so.
4. A status tick cannot bypass the Vital commit boundary by mutating a shared component directly.
5. A capability rebuild can clear and rebuild only derived fields; it cannot reset durable counters,
   inventory state, death state, or a resource without an explicit lifecycle command.
6. Recursive Paladin damage uses an explicit source/target and idempotency contract. It must not
   create a second hidden damage writer.
7. Network and persistence use immutable committed snapshots. Their IDs are domain/session IDs and
   are never confused with the runner `sessionId` used only for partition settlement.

The complete reference's recovered serializer writes life/max and mana/max near the beginning of a
version-319 encrypted player record, then serializes equipment, inventory, banks, buffs, death
state, creative data, loadouts, and dialogue. That is a protocol fact for the supplement; it does
not authorize moving those fields into the P06 live owner or claiming Version4 persistence parity.

## Legacy API To Proposed Composition

| Legacy entry | Proposed composition | Required observations | Evidence status |
|---|---|---|---|
| `Player.Hurt(reason, damage, direction, pvp, quiet, crit, cooldownCounter, dodgeable)` | `PlayerDamageEligibilityQuery` -> mitigation Query -> `PlayerCombatResolutionSystem.ResolveHit` -> committed result -> P04/status/item/projectile/network/presentation adapters | early returns, final damage and rounding, resource delta, immune timer, knockback, recursive calls, effect count/order, errors | source local behavior `confirmed`; complete effect closure `partial`; new composition `proposed` |
| `Player.KillMe(...)` from lethal damage | Vital result -> `PlayerLethalDamageCommitted` -> P04 lifecycle/death System | lethal threshold, death side effects, respawn/reset scope, recursion and duplicate handling | `crossSubsystemOwner: integration-review` |
| `AddBuff` / `DelBuff` / `UpdateBuffs` | `PlayerStatusEffectSystem.Apply/Remove/Tick` + catalog Query + packet adapter | immunity, replacement, slot eviction, timer decrement/expiry, derived status visibility | source local behavior `confirmed`; catalog/effects `partial`; composition `proposed` |
| `ResetEffects` / `UpdateEquips` | `PlayerCombatCapabilityRebuildSystem.BeginTick` -> immutable capability snapshot | reset-before-rebuild, same-frame visibility and durable counter preservation | local order `confirmed`; scheduler and input closure `partial` |
| `UpdateLifeRegen` / `HurtLifeRegen` | `PlayerLifeRegenQuery` -> explicit result -> reviewed Vital commit | counters and branch order; final life delta, combat text, spectating and death effects | core query `partial`; final commit `unknown`; complete reference `full-reference-supplemented` |
| `UpdateManaRegen` | `PlayerManaRegenSystem.Tick` -> Vital command/result | delay modifiers, count threshold, cap, standing/grappling/buff inputs, visibility | source algorithm `confirmed`; composition and writer `proposed` |
| `AllowShimmerDodge` inside `Hurt` | `PlayerShimmerDodgeQuery` or resolution subrule, only after reference/version decision | dodgeable gate, cooldown gate, source entity rules, consumed cooldown and effects | Version4 `unknown`; complete reference `full-reference-supplemented`; behavior still `not-run` |
| `addDPS(dmg)` | `PlayerDpsTelemetrySystem.RecordCommittedDamage(event, ClockPort)` -> projection | first/last hit, start/end times, accumulation, reset and wall-clock semantics | local body `confirmed`; clock/readers `partial`; composition `proposed` |
| `NetMessage`/`MessageBuffer` life and mana packets | protocol adapter <-> committed resource snapshot | `Int16` widths, packet order, identity/authority, life<=0 -> `dead`, max-life floor, relay rules | same-path packet shape `confirmed`; authority/relay closure `partial` |
| `SavePlayer`/`InternalSavePlayerFile`/`Serialize`/`Deserialize` | persistence adapter + versioned snapshot codec + validated restore command | encrypted version, metadata, field order, omitted/transient fields, unknown release handling, backup/cloud/error recovery | Version4 `unknown`; complete reference `full-reference-supplemented`; parity `not-run` |

`PlayerDamageEligibilityQuery` and related names in the table describe conceptual behavior. The
current NLTX query and resolver code is not evidence that this composition is production-connected
or behavior-equivalent.

## Phase, Barrier, And Dependency Proposal

The only confirmed order is the order inside Version4 `Player.Update`. A future scheduler must
prove an equivalent visibility contract rather than infer one from file or registration order.

```text
network / gameplay commands + equipment/status/environment snapshots
  -> capability rebuild (derived snapshot)
  -> status tick snapshot
  -> damage eligibility + mitigation
  -> one accepted-hit commit
       -> vital/immunity/proc result
       -> P04 lethal handoff
       -> status/item/projectile/presentation/network adapters
  -> mana regen commit
  -> life regen result and reviewed life commit
  -> DPS telemetry from committed damage
  -> immutable network/persistence projections
```

Candidate barriers:

- `capability-rebuild` must precede any same-frame hit eligibility or regen read that consumes
  derived modifiers.
- `status-tick` must publish a stable snapshot before regen reads it; status expiry must not be
  visible half-way through a damage commit.
- `accepted-hit-commit` must precede telemetry and accepted-hit projections.
- `vital-commit` must precede resource packet encoding and persistence snapshots.
- P04 owns death/respawn lifecycle; P06 must hand off lethal results before reset or destroy.

These are proposed barriers. Runtime registration, parallel stages, event subscribers, exception
edges, multi-world scope, and unload paths remain `unknown` until source and target integration are
implemented and inspected.

## Complete Reference Supplement Decisions

| Version4 gap | Complete reference observation | Design consequence |
|---|---|---|
| `HurtLifeRegen` empty at `Player.cs:11238` | `Player.cs:19749-19756` subtracts `statLife`, emits `CombatText.LifeRegen`, and calls `SetOrRequestSpectating(-1)` | The proposed life commit must model these effects as an explicit result/effect boundary. Mark `full-reference-supplemented`, not Version4-confirmed or migrated. |
| `AllowShimmerDodge` empty at `:22568` | `Player.cs:39105-39130` rejects non-dodgeable and cooldown `1`; accepts no causing entity; rejects active boss/invasion/can-hit-past-shimmer NPCs and configured projectiles | Preserve this as a candidate oracle with source/version provenance. Entity identity, table contents, and runtime effects still require integration and behavior tests. |
| save methods empty at `:26417-26455` | `Player.cs:55313-55345` backs up, encrypts version `319`, writes metadata and serialized player data; `:55348-55531` writes resources, equipment, inventory, banks, buffs, death, creative/loadout and dialogue; `:55750` begins release-aware deserialize | Define a versioned adapter and restore validation. Do not infer that every serialized field belongs to P06 or that the target has parity. |
| life/mana packet implementation gap | Complete reference `MessageBuffer.cs` reads life/max as `Int16`, sets `dead` from life, and reads mana/max; `NetMessage.cs` writes the same `Int16` pairs in the same packet cases | Protocol shape is a same-path supplement. Authority, relay, identity checks, and complete packet closure stay `partial`. |

Reference-only implementation details not present at the same Version4 path are tagged
`outside-Version4-baseline` and excluded from the claim. A future source comparison may change the
design, but it must update evidence and behavior status rather than silently upgrading this plan.

## Lifecycle, Errors, And Side Effects

- **Create/activate:** choose one Vital and one status authority per world/session. Initialize
  defaults from source evidence; do not use component defaults as a substitute for constructor and
  load behavior (`partial`).
- **Tick:** preserve reset -> buff -> equip -> life regen -> mana regen visibility. A reordered
  tick is a behavior change requiring an explicit decision and test.
- **Accepted hit:** commit one result with source identity, target identity, final amount, and
  accepted/rejected reason. Emit downstream effects only after the commit point.
- **Rejected hit:** return a reason and leave accepted-hit state unchanged unless the old branch
  proves a rejection side effect.
- **Recursive hit:** use a bounded, explicit recursion/duplicate policy. Partial failure must not
  be retried as a second accepted hit without an idempotency key.
- **Persistence:** validate version, metadata, encryption/decryption, field bounds, and restore
  result before applying live state. Backup/cloud/file failures require an explicit error result;
  they are not hidden in a Projection.
- **Network:** validate player/entity identity and authority before writing a resource snapshot.
  Preserve packet widths and ordering; do not let decode bypass the resource owner.
- **External effects:** combat text, sound, particle, spectating, random, wall-clock, and cloud I/O
  are ports/adapters. Their exact timing and retry behavior are unresolved until integration review.

## Observation Contract

Future old/new comparisons must capture:

```text
Observation = (
  return_or_error,
  authoritative_state_delta,
  emitted_events_and_external_effects,
  order_and_visibility,
  lifecycle_and_scope,
  retry_and_idempotency
)
```

Required scenarios include rejected/dodged/immune hits, ordinary and critical mitigation, lethal
and recursive Paladin damage, buff replacement/expiry, reset-plus-equips-plus-regen ordering,
life/mana caps, life/mana packet round trips, save/load releases and failures, and DPS clock/reset
behavior. A focused verifier, compile result, or static API map may support a narrow rule but cannot
be labeled `migration-success`.

## Open Gaps And Integration Handoff

The following remain blockers or cross-partition decisions:

- CPG source binding is `unknown` because `SourceSnapshotId=null`; selected-range facts are
  `partial` and do not close aliases, dynamic calls, hooks, or all writers.
- Version4 life commit, shimmer dodge, and persistence were empty; complete-reference evidence is
  `full-reference-supplemented`, not authoritative Version4 execution evidence.
- The unique NLTX Vital/Status representation is selected only for the isolated core slice;
  production registration and complete effect ownership remain unresolved.
- P04 owns death/respawn; P07/P08 own movement/environment; P09 owns item/equipment definitions;
  P11 owns presentation; P12/P15 own shared NPC/projectile combat; Network and Persistence own
  protocol/storage boundaries. Every unresolved shared owner is
  `crossSubsystemOwner: integration-review`.
- Full behavior, exception, retry, multi-world, unload, and scheduler evidence is `unknown` until
  the real target path is exercised.

## Decision Record

The added complete-reference path is a `moderate` evidence-scope change recorded from the user
requirement: `完整参考项目"D:\TRbackup\无任何删减通过编译"`. It changes the evidence source and
future build gate, but does not authorize code changes in that reference tree or a new P06 member
set. The design keeps the original claim, marks recovered bodies as `full-reference-supplemented`,
and leaves full-P06 verification `not-run`. The isolated core slice is recorded separately above.

## Current Verification Statement

The source and CPG inspection, the isolated core implementation, and its focused verifier run are
recorded above. No full target integration, full reference-project compilation, complete behavior
suite, or deletion gate was run. The report and this design must not be cited as migration success;
the document-level verification state remains `not-run`.
