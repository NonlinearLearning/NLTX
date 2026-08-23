# Remaining ECS Migration Summary and TODOs

> **For Claude:** Continue implementation with executing-plans task-by-task. This file is the
> consolidated backlog and completion boundary for the remaining ECS migration work.

**Goal:** Complete the remaining source-backed Terraria server ECS migration without restoring
legacy Main ownership, while keeping unsupported or unproven behavior explicit and fail-closed.

**Architecture:** Terraria.Dome.Simulation owns authoritative gameplay state, immutable snapshots,
deterministic systems and command commits. Server-side projects own WLD I/O, persistence orchestration
and protocol projection. Client-only behavior, transport concerns and legacy global state must not
leak back into Simulation.

**Tech Stack:** .NET 10, C#, Arch ECS, immutable snapshots, deterministic replay fixtures, focused
verification executables and MainBoundary analysis.

---

## 1. Final Objective

The migration is complete only when:

1. Every server-relevant legacy Main.cs responsibility is classified as accepted, intentionally
   excluded, explicitly unsupported, or assigned to a concrete migration card.
2. Each accepted responsibility has one authoritative ECS owner, an explicit command/state route,
   source-member provenance and focused evidence.
3. Player, NPC, projectile, item, world-object, liquid, wiring and world-event lifecycles have
   deterministic transitions, persistence where applicable, and invalid/duplicate input rejection.
4. WLD fields are imported only when their source meaning and destination are exact. Unknown or
   source-dependent branches remain unavailable rather than defaulted.
5. Tick order, sequence allocation, revision allocation and random streams are deterministic and
   restart-stable for every supported domain.
6. Simulation remains free of legacy Main, client, transport, protocol and Server dependencies.
7. One final acceptance record is generated after the large migration target is complete; narrow
   cards do not create individual Markdown records.

This is not a percentage-complete claim. A green focused verifier proves only the behavior it covers.

## 2. Completed and Verified Narrow Scope

- Player command/lifecycle basics, item use and Well Fed state separation.
- NPC definition validation, spawn identity, target/chase baseline, death revision and deterministic loot.
- Projectile spawn identity, owner validation, collision/lifetime boundaries and revision fail-closed paths.
- Item inventory/use/placement/drop, world-item spawn/motion/pickup/destroy/stacking and persistence identity boundaries.
- Chest, sign and Training Dummy world-object ownership, mutation and persistence guards.
- Liquid input, propagation, merge, commit, panic mode and retry-budget atomicity.
- Wiring input validation and deterministic tile-command routes.
- World clock overflow protection, event random range handling, time-rate and invasion-size arithmetic bounds.
- WorldGeneration runtime/random snapshots, monotonic stages, checkpoint consistency and sequence exhaustion handling.
- Door/Trapdoor/TallGate, chest, sign, tile-entity, Player and NPC allocator exhaustion guards.
- Loot quantity ranges covering int.MaxValue without integer-range overflow.
- MainBoundary currently reports 791 Simulation source files and zero forbidden dependencies.

Recent focused gates that have passed include Items, NPC, Combat, Liquid, WorldObjects, WorldRules,
WorldGeneration, TickOrder, PlayerLifecycle, Simulation Release build and MainBoundary. These are
targeted gates, not full client/server regression evidence.

## 3. Remaining Execution Queue

Execute in dependency order. Each item requires a source oracle, a falsifiable focused test, the
smallest authoritative route, deterministic replay where relevant, MainBoundary and scoped diff
check. Do not add a new Markdown file for an individual item.

### A. Contract and ownership foundations

- **M-001 InitializeAlmostEverything:** split remaining initializer responsibilities into real
  independent families. Do not create an aggregate initializer or copy Main fields.
- **M-014 MainThreadAction:** define typed, caller-specific, replayable contracts. Do not merge
  different ownership or phase semantics into Queue of Action.
- **M-024 DelayedProcesses:** define typed IEnumerator phase, cancellation and restart semantics before
  migrating delayed work.
- Finish executable classification of all server-relevant Main members and connect each accepted
  member to an ECS owner or explicit deferred card.

### B. World state and WLD recovery

- **WLD clock:** decide the source-backed contract for fractional Main.time versus the current Int32
  clock. Do not import WLD time/day-time until round-trip semantics are proven.
- **Weather model/import:** preserve independent raining, rainTime and maxRaining; implement only
  FixEndlessRainWorlds branches with an available source/seed oracle.
- **Wind:** restore modern direct fields only when current-value relations are source-backed; keep
  old-layout randomization unavailable without a compatible random-stream proof.
- **Random streams:** define named persisted server streams for supported domains and explicitly defer
  global legacy Main.rand ordering parity.
- **WLD identity/header fields:** consume only fields with one exact owner and version-boundary evidence.

### C. World events

- **B-007 Main.rand ordering oracle:** recover an executable source trace or retain probabilistic legacy
  scheduling as deferred.
- **Invasion lifecycle:** complete delay, travel authority, start size, progression, clear/recovery and
  persistence transitions. Random starts, NPC spawn tables and client announcements remain separate.
- Close one source predicate or transition per Slime Rain, Lantern Night and weather card.
- Retain direct meteor pending-schedule import; add random scheduling only with replayable source order.

### D. Entity lifecycle completion

- **Player:** close movement/collision, input sequencing, respawn and persistence gaps.
- **NPC:** complete supported AI families, spawn tables, combat/death/loot branches and replication;
  unsupported types must fail closed.
- **Projectile:** close ownership, motion, collision, damage, lifetime and replication gaps per definition family.
- **Item:** close remaining static definition tables, use/placement/drop/persistence branches and
  world-item lifecycle cases.
- **World objects/tiles:** keep all mutations behind deterministic command commits and typed ownership.

Every accepted entity card needs accepted, forged, duplicate, invalid-order and identity-boundary
traces. Persistence and two-session PVS evidence are required when persisted or replicated.

### E. Tick order and integration

- Reconcile source-derived phase ordering per domain instead of importing the whole legacy Main loop.
- Add same-tick competing-command replay checks and ordered commit/snapshot hashes.
- Define typed sequence allocators for remaining liquid and wiring internal sequence paths before
  changing their many direct increment sites.
- Treat phase conflicts as architecture decisions backed by source evidence.

## 4. Explicit Deferred/Blocked Items

- No new independent initializer responsibility family for M-001 yet.
- No typed/replayable caller contract for M-014 or M-024 yet.
- No legacy global Main.rand ordering oracle for B-007.
- WLD Double-time fractional contract is not losslessly mapped to the current Int32 clock.
- maxRaining runtime mapping and secret-seed repair evidence are incomplete.
- No source-backed dynamic invasion travel authority equivalent to Main.dayRate for all branches.
- Random invasion starts, NPC spawn tables, complete invasion lifecycle and client/chat effects remain incomplete.
- Complete NPC/projectile/item static tables and all AI/type branches remain incomplete.
- Full client/presentation branches, complete entity/event lifecycle evidence and broad two-session
  replication coverage remain incomplete.

A deferred item may be reopened only after its missing oracle, contract or source trace is available.

## 5. Verification Policy

The current policy is focused rather than a full regression run:

~~~powershell
dotnet run --project Test\Terraria.Dome.MainBoundary.Verification\Terraria.Dome.MainBoundary.Verification.csproj -c Release -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false --no-restore
dotnet build src\Terraria.Dome.Simulation\Terraria.Dome.Simulation.csproj -c Release -m:1 -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false --no-restore
~~~

For a changed domain, run its focused verifier plus MainBoundary and scoped git diff --check. Use
two deterministic replays when a card changes persistence, random state, event scheduling or phase
ordering. Never present a focused green result as full migration completion.

## 6. Documentation and Acceptance Rule

- Do not create docs/research Markdown or a per-card progress note for each narrow fix.
- Do not repeatedly rewrite progress.md for small boundary cards.
- Preserve existing historical Markdown and user changes.
- After the large migration target is genuinely complete, create one final acceptance summary containing
  source hashes, accepted cards, focused outputs, replay outputs, MainBoundary, build output, deferred
  branches and final coverage classification.

## 7. Definition of Done

The objective is done only when the final acceptance summary proves Section 1, every item in Section 3
is accepted or explicitly justified as unavailable by a source-backed decision, and no server-relevant
legacy responsibility is unclassified. Until then, keep the goal active and report partial progress
without completion language.
