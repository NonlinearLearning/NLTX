# Player Blocked Core Unblock Execution

> Status: executed through a partial Phase 7 boundary (2026-08-29); this
> document records evidence for the eleven archived Player blockers. It is not
> a claim that any blocker is fully migrated.

## Objective

Turn each archived `Blocked` Player cluster into one of two evidence-backed
outcomes:

1. `accepted`: a server-owned contract, deterministic command/commit path,
   required persistence or protocol projection, and a focused verifier exist;
2. `deferred`: the missing source fact or authority boundary is reproduced and
   recorded, with no placeholder behavior added.

The source of truth for the inventory is:

- `docs/migrations/player-blocked-core-archive.md` (PB-001 through PB-011);
- `docs/migrations/player-legacy-method-status.csv` (cluster, source range,
  current status, and verifier/dependency);
- `docs/migrations/player-legacy-behavior-map.md` (ownership and status rules);
- `docs/cr/CR-2026-08-29-player-blocked-core-archive.md` (accepted archive
  decision and Phase 7 boundary).

The external `Terraria.Player` source remains a read-only oracle. It must not
be copied into `src/`, compiled by the Simulation project, or treated as a
runtime dependency.

## Non-Negotiable Boundaries

- The Simulation owns mutable state; Server owns session authority and
  persistence orchestration; Protocol projects immutable snapshots; client-only
  presentation stays delegated.
- Every mutation is `input -> validation -> typed command/event -> deterministic
  commit -> snapshot`. A DTO, Arch entity reference, legacy object, callback, or
  mutable global cannot cross the commit boundary.
- A PB entry stays `Blocked` until all of its included behavior families have an
  explicit owner. A verifier for one sub-family cannot promote the whole row.
- `Main.rand` ordering, arbitrary `Action`/`IEnumerator` scheduling, and
  client/UI state remain deferred unless an executable source trace and replay
  contract are recovered.
- Preserve the dirty worktree. Changes in a batch are limited to its declared
  write set. Do not perform broad formatting, source deletion, or generated
  output under `src/`.

## Shared Qualification Gate

Run this gate before opening any PB card. Record the output in a fresh directory
under `Build/diagnostics/player-blocked-core/<pb-id>/<run-id>/`.

1. Capture `git status --short` for every file in the proposed write set.
2. Hash the external oracle with `Get-FileHash ... -Algorithm SHA256` and
   record the exact source path and line range from the CSV.
3. Read the source member and its callers far enough to identify all mutable
   inputs, side effects, random consumers, persistence fields, and packet
   projections.
4. Name one authoritative owner and one stable identity for replay. If either
   is ambiguous, stop with `deferred`.
5. Define RED cases before editing: invalid owner, duplicate command, restart,
   out-of-range input, and the relevant unsupported/deferred branch.
6. Define the focused verifier and the adjacent boundary verifier. A green
   test is not evidence for untested source branches.

Each scenario record must contain:

```yaml
status: planned | accepted | deferred
archive_id: <PB-ID>
source: { path: ..., sha256: ..., lines: ... }
owner: ...
authority_input: ...
red: ...
accepted: ...
rejected: ...
persistence_or_projection: ...
deferred: ...
focused_verifier: ...
adjacent_verifier: ...
evidence: ...
```

## Dependency Waves

Do not run cards in parallel when they write the same contract. The ordering is:

| Wave | Cards | Required prerequisite | Output |
| --- | --- | --- | --- |
| 0 | all | source/owner/replay qualification | one RED scenario per PB entry |
| 1 | PB-002, PB-004, PB-006 | movement/world snapshot and command boundary | movement/environment/world interaction contracts |
| 2 | PB-003, PB-010 | persistent inventory import and executable item registry | validated inventory/item effects |
| 3 | PB-005, PB-007, PB-008, PB-011 | combat/stat event ordering and multiplayer authority | ordered stat and combat rules |
| 4 | PB-001, PB-009 | item/projectile/world-object contracts from earlier waves | specialized mechanics and death-drop outcomes |
| 5 | closure | all accepted/deferred evidence | status reconciliation and Phase 7 decision |

An individual card may be reopened independently when its prerequisites are
already present. A wave is not a release milestone.

## Execution Cards

### PB-001: Specialized Mechanics

**Scope:** golf, fishing visuals, wings, ropes, mounts, emotes, and mod hooks.

**Oracle and owner:** use the `SpecializedMechanics` CSV range (`52000-634315`)
and refresh the source hash. Split the range into separate source-backed
families. Server rules belong in the existing movement, world-generation,
item, or player domains; visuals, emotes, and hooks belong in the client or
compatibility adapter. Do not create a catch-all specialized component.

**Write set:** one definition/policy per accepted family under its owning
domain; typed server commands only where a client request exists; corresponding
snapshot/projection and a focused verifier. No write set may include arbitrary
mod callbacks or presentation state.

**RED:** unknown item/mount/golf identity, forged owner, client-only visual
request, callback with no restart identity, duplicate request, and a family
whose source has multiple plausible owners.

**GREEN:** each included family has source lines, one owner, deterministic
replay, invalid-input rejection, and protocol/client delegation evidence. The
card remains `deferred` for any family without those facts.

**Verifiers:** the narrow domain verifier plus `PlayerAuthority.Verification`;
use `FullClientBootstrap.Verification` only for delegated presentation output.

### PB-002: Deferred Special Movement

**Scope:** wall climb, wallslide, carpet, double jump, wing, grapple, and sticky
movement (`12558-13832`).

**Write set:** `MovementIntentComponent` extensions, named movement capability
definitions, collision queries, and typed movement commands in
`src/Terraria.Dome.Simulation/Movement` or `Player`. Do not alter the existing
gravity path until the capability's ordering is proven.

**RED:** jump/double-jump without the required capability, grapple from an
invalid anchor, movement through a solid tile, duplicate input in one tick,
and a client-provided velocity. Test each capability independently.

**GREEN:** same initial snapshot and input sequence produce identical snapshots;
ground contact, collision, cooldown/fatigue, and capability consumption are
explicit; protocol input is converted to intent before physics. Unsupported
capabilities stay rejected rather than treated as ordinary jump.

**Verifiers:** `PlayerPhysics.Verification`, `PlayerSimulation.Verification`,
and `PlayerAuthority.Verification` when capability ownership is networked.

### PB-003: Deferred Inventory Modes

**Scope:** locked inventory, stacking mode, and Void Bag semantics
(`13993-14026`).

**Write set:** inventory mode component/policy, persistent import/restore
mapping, validated inventory commands, and snapshot/projection fields only if
the V1.4.5 contract requires them.

**RED:** locked-slot mutation, stack overflow/underflow, Void Bag transfer by a
non-owner, unknown item definition, stale session overwriting persistent data,
and a mode value that cannot survive restart.

**GREEN:** all imported slots are preserved; runtime uses only executable item
definitions; mode transitions are server-authoritative and deterministic;
invalid commands are rejected with no partial mutation; save/restart retains
the mode or explicitly records it as unsupported.

**Verifiers:** `Items.Verification`, `PlayerAuthority.Verification`, and the
relevant persistence verifier.

### PB-004: Deferred Environmental Movement

**Scope:** drowning, slope-down, and wet collision (`14040-14144`).

**Write set:** liquid/environment read snapshots, named collision policies,
player environmental state, and typed damage or movement commands. Keep tile and
liquid mutation in the existing world command/commit path.

**RED:** non-finite liquid values, stale tile reads, drowning while out of
liquid, slope movement that tunnels through a tile, duplicate environmental
damage, and client-authored wet state.

**GREEN:** a read-only environment snapshot drives deterministic collision and
drowning; damage ordering is explicit; restart/replay gives identical result;
world mutation is committed once and projected afterward.

**Verifiers:** `PlayerPhysics.Verification`, `Liquid.Verification`, and
`Combat.Loopback.Verification` when drowning emits damage.

### PB-005: Deferred Luck and Counters

**Scope:** luck factor calculation and miscellaneous counter ordering
(`17869-17985`).

**Write set:** source-backed stat inputs, immutable luck calculation policy,
counter component, ordered stat system, and replay fixtures. Do not introduce a
global random source or infer missing luck inputs from defaults.

**RED:** missing source factor, negative/NaN factor, update after snapshot
projection, duplicate tick consumption, and counter values changed by client
input.

**GREEN:** equations and clamp rules match the oracle; each counter has one
writer and documented tick order; replay hashes match; persistence either
round-trips the value or explicitly marks it unavailable.

**Verifiers:** `Combat.Verification`, `PlayerSimulation.Verification`, and
`PlayerAuthority.Verification` for server-owned stat input.

### PB-006: Deferred Teleport and Spawn

**Scope:** teleport and all spawn-position/clear-area helpers
(`21731-22046`).

**Write set:** authoritative teleport/spawn request, validated world query,
typed commit command, spawn result snapshot, and protocol projection where
applicable. Keep spawn placement separate from player lifecycle identity.

**RED:** forged target player, invalid destination, unsafe spawn area, unloaded
world, duplicate teleport, and client-supplied final position.

**GREEN:** server computes and commits a valid destination atomically; clear-area
and collision rules are deterministic; reconnect/restart preserves the chosen
spawn policy; loopback packet output reflects the committed snapshot.

**Verifiers:** `PlayerLifecycle.Loopback.Verification`,
`PlayerAuthority.Verification`, and `WorldObjects.Verification` or the focused
world interaction verifier for the destination query.

### PB-007: Deferred Defensive Abilities

**Scope:** Shadow Dodge, Brain of Confusion, and Ninja Dodge
(`22096-22116`).

**Write set:** ordered immunity/evasion events, ability cooldown/consumption
components, combat resolution integration, and snapshot fields only when
replicated by protocol.

**RED:** ability proc without an equipped source, proc after cooldown expiry is
ignored, multiple procs for one hit, damage applied before evasion decision,
and client-authored immunity.

**GREEN:** one incoming hit produces one ordered decision; ability ownership,
cooldown, immunity window, and random/effect input are source-backed; replay and
loopback agree; unsupported proc branches remain deferred.

**Verifiers:** `Combat.Verification`, `Combat.Protocol.Verification`, and
`Combat.Loopback.Verification`.

### PB-008: Deferred Ally Defense

**Scope:** Paladin shield ally defense and targeting (`22197-22207`).

**Write set:** multiplayer defense authority query, target-selection policy,
ordered protection event, and replication projection if another player must
observe the result.

**RED:** protecting a non-ally, out-of-range target, stale position, duplicate
protection event, self-target ambiguity, and a client choosing the protected
player.

**GREEN:** server selects the target from current snapshots, applies one
protection result in deterministic order, rejects forged ownership, and emits
stable loopback bytes for all affected players.

**Verifiers:** `Combat.Loopback.Verification`, `PlayerAuthority.Verification`,
and `SessionReplication.Verification`.

### PB-009: Deferred Death Drops

**Scope:** tombstone/drop behavior (`22771-22771`).

**Write set:** death-drop rule definition, atomic death-drop command/event,
world-object persistence projection, and duplicate-death replay fixture. The
existing `SignTombstone`/world-object ownership must be reused where it matches
the source contract.

**RED:** drop on a non-terminal despawn, duplicate drop after reconnect,
partial inventory mutation, invalid world position, or drop generated from a
client death claim.

**GREEN:** exactly one accepted terminal death produces an atomic persisted drop
or an explicit source-backed no-drop result; restart does not duplicate it;
inventory, world object, and replication snapshots agree.

**Verifiers:** `Combat.Loopback.Verification`, `WorldObjects.Verification`, and
the persistence verifier.

### PB-010: Deferred Special Item Effects

**Scope:** biome torch styles, melee scale, summon/projectile effects, Spore Sac,
Volatile Gelatin, hit rules, and item checks (`23107-23435`).

**Write set:** executable item definitions, item-use/effect commands, combat or
projectile policy, authority validation, and protocol projection only for
observable results. Client hold/animation/style remains delegated unless a
server rule is proven.

**RED:** unknown definition, invalid slot/stack/mana, item effect without
ownership, projectile spawned before validation, client-only style treated as a
server effect, and duplicate use in one cooldown window.

**GREEN:** definition compilation is explicit; validation happens before any
mutation; effects are deterministic and replayable; spawned projectiles carry
server-owned identity and owner; unsupported visual branches remain delegated.

**Verifiers:** `Items.Verification`, `Combat.Verification`,
`Combat.Loopback.Verification`, and the projectile verifier when an effect spawns
one.

### PB-011: Deferred Banner and Melee Rules

**Scope:** banner buffs, melee cooldowns, NPC hit rules, jellyfish damage, and
attack cooldown (`23908-23974`).

**Write set:** banner/equipment definition lookup, ordered melee eligibility and
cooldown systems, typed damage command integration, and replication projection
for observable buffs/cooldowns.

**RED:** banner buff from the wrong NPC type, melee hit through immunity or
range, duplicate cooldown reset, jellyfish damage without source contact, and
client-authored attack result.

**GREEN:** all hit predicates are evaluated from committed snapshots; cooldown
and immunity ordering is stable; one hit produces one damage result; NPC/player
loopback projections agree; missing banner or special-NPC tables remain
explicitly deferred.

**Verifiers:** `Combat.Verification`, `Combat.Loopback.Verification`,
`Npc.Verification`, and `SessionReplication.Verification` when projected.

## Verification Contract

For each card, run serially from the repository root with
`-p:UseSharedCompilation=false`:

1. the named focused verifier;
2. at most one adjacent verifier listed on the card;
3. `Test/Terraria.Dome.MainBoundary.Verification` if the write set touches a
   shared player/server boundary;
4. `git diff --check -- <declared write set>`.

For a card that changes Simulation code, also run:

```powershell
dotnet build .\src\Terraria.Dome.Simulation\Terraria.Dome.Simulation.csproj -p:UseSharedCompilation=false --no-restore
```

Record exit code, warning count, and fresh evidence paths. Do not run the full
solution, full-client matrix, or Release gate as part of an individual card.
Those belong to Phase 7 after all cards have been reconciled.

## Rollback and Stop Conditions

Stop and mark the card `deferred` when:

- source evidence does not identify a unique owner or stable replay identity;
- a mutation bypasses typed command/commit or a projection reads mutable ECS
  storage after the tick;
- a persistent server record can be overwritten by session input;
- a verifier passes by silently ignoring an invalid owner, packet, or target;
- deterministic replay diverges without a documented source rule change;
- a proposed deletion lacks method-status evidence and target references.

Rollback is phase-local: disable the new system registration, retain the typed
adapter, and restore the previous projection/compatibility path. Do not reset
the worktree or remove broad `Build/` directories. Persistent format changes
require a backward-compatible reader before enabling a new writer.

## Closure and Phase 7 Decision

At closure, update each PB row in the CSV and behavior map with:

- final status (`accepted` or `deferred`);
- source hash and line range;
- owner and verifier names;
- evidence directory;
- explicitly deferred source branches.

Then run the existing Player Phase 7 gate from
`docs/plans/2026-08-18-player-ecs-migration-execution.md`. The migration may
only claim full completion when no core blocker is unexplained, all required
snapshot/protocol/replay evidence is green, and client-only branches are
delegated. If any PB entry remains deferred, publish a release boundary that
names the entry and keeps the overall Player migration `partial`, not
`complete`.

## Definition of Done

This plan is complete when PB-001 through PB-011 each have either an accepted
implementation card with fresh evidence or a reproducible deferred boundary,
and the status map, archive, and Phase 7 decision agree. It does not by itself
authorize implementation of all eleven cards or imply behavior parity.
