# Main Server ECS Migration Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Replace the server responsibilities of Version4/Terraria/Main.cs with explicit,
deterministic Dome ECS state and systems without importing legacy Main globals, client runtime,
rendering, UI, or local input dependencies.

**Architecture:** Terraria.Dome.Simulation owns world rules, simulation time, entity state,
commands, systems, deterministic tick ordering and immutable snapshots. Terraria.Dome.Server
owns process startup, sessions, persistence orchestration and protocol fan-out. Protocol only
translates typed input and snapshots. Main.cs remains a behavior reference, not a class to copy.

**Tech Stack:** .NET 10, C#, Arch 2.1.0, executable verifiers, serial Release builds with
-p:UseSharedCompilation=false.

---

## Scope and non-goals

The first target is the server subset of:
D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs

Migrate:

- simulation tick and phase ordering;
- world clock and day/night state;
- world seed, dimensions, spawn and progression state;
- server-owned entity lifecycle and typed domain stores;
- deterministic server world bootstrap;
- world events only with explicit authority and an executable verifier;
- pure calculations required by Simulation.

Exclude from Terraria.Dome.Simulation:

- graphics, camera, shaders, sprites, map rendering and GraphicsDeviceManager;
- UI, menus, windows, assets, audio, resource packs and platform integrations;
- keyboard, mouse, gamepad and local client input;
- Main.player, Main.npc, Main.projectile, Main.item, Main.tile, Main.rand, Main.time and
  Main.GameUpdateCount;
- universal MainState, EntityState, Manager, Helper or Data containers.

A legacy method is not complete merely because it has an ECS method. Every behavior family needs a
reference, an authority owner, a typed projection where relevant, and an executable verifier.

## Responsibility map

Line numbers refer to the current Version4 snapshot and must be refreshed in the ledger:

| Legacy region | Responsibility | Dome destination |
|---|---|---|
| Main.cs:90-1860 | client fields, assets, camera, UI and platform state | excluded |
| Main.cs:2179-2600 | world/server startup and dedicated-server setup | Server startup and DomeServer |
| Main.cs:3656-3980 | engine/entity initialization | Simulation bootstrap and Server startup |
| Main.cs:3980-7750 | static tile/NPC/projectile/item/buff tables | domain Definitions or existing registries |
| Main.cs:11559-12064 | main update loop and in-world update | tick schedule and domain systems |
| Main.cs:12458-12600 | weather and environment update | World systems |
| Main.cs:12958-13132 | invasion and event update | World progression systems |
| Main.cs:13132-14600 | server update, time and event transitions | clock, rules and progression systems |
| Main.cs:14639-14694 | pure damage calculations | Combat calculation types |

The ledger must record every Main.* read used by each server-relevant method. Client-only values
are excluded; gameplay values become explicit state, definitions, snapshots or commands.

## Target modules

~~~text
src/Terraria.Dome.Simulation
  World/
    WorldClock.cs
    WorldProgressionState.cs
    WorldRuleState.cs
    Systems/WorldClockSystem.cs
    Systems/WorldRuleSystem.cs
  Tick/
    SimulationTickSchedule.cs
    SimulationTickContext.cs
  Player/ Npc/ Projectile/ Items/ Combat/ Physics/ Snapshots/

src/Terraria.Dome.Server
  Startup/ Sessions/ Persistence/ Replication/

Test
  Terraria.Dome.MainBoundary.Verification/
  Terraria.Dome.WorldClock.Verification/
  Terraria.Dome.WorldBootstrap.Verification/
  Terraria.Dome.TickOrder.Verification/
  Terraria.Dome.WorldProgression.Verification/
~~~

Do not create all directories in advance. Add a domain folder only when its state, system and
verifier are part of the current batch. Keep DomeSimulation as a public facade, but make it
orchestration rather than another Main.cs.

## Invariants

1. Simulation has no reference to Server, Protocol, sockets, V1456 IDs, UI, XNA or file paths.
2. External input becomes a validated command; it never writes position, health, target,
   inventory or entity IDs directly.
3. Every mutation occurs at a designated tick phase with stable ordering.
4. Clock, random streams and progression state are part of persistence snapshots.
5. Rejected input leaves the authoritative snapshot and revisions unchanged.
6. Equal seed, initial snapshot and input trace produce equal snapshots.
7. Unsupported legacy behavior is explicit and rejected before mutation.
8. Generated output stays below repository Build, never under src.
9. Completion claims require fresh verifier output from the current source.

## Execution protocol

Every task follows this sequence:

1. Run read-only preflight and capture output in the task evidence directory.
2. Read the named Version4 methods and current Dome counterpart.
3. Add a focused executable verifier with accepted and rejected cases.
4. Run the verifier before implementation and record the RED result.
5. Implement the smallest complete authority path.
6. Run the focused verifier, affected verifiers and root Release build.
7. Run git diff --check and inspect generated paths.
8. Update progress.md with commands, exit codes, evidence paths and remaining gaps.
9. Commit only task files after the focused verifier and build are green.

Preflight:

~~~powershell
git status --short
git diff --check
dotnet sln Terraria.Dome.sln list
dotnet msbuild src\Terraria.Dome.Simulation\Terraria.Dome.Simulation.csproj -nologo -getProperty:TargetFramework -getProperty:BaseOutputPath -getProperty:BaseIntermediateOutputPath -p:UseSharedCompilation=false
~~~

Release build:

~~~powershell
dotnet build Terraria.Dome.sln -c Release -p:UseSharedCompilation=false
~~~

Focused verifier:

~~~powershell
dotnet run --project Test\<Verifier>\<Verifier>.csproj -c Release --no-build -p:UseSharedCompilation=false
~~~

Use a fresh Build/diagnostics/main-migration/<run-id>/ directory. Never reuse an old no-build
result as evidence for a changed source tree.

---

## Task 0: Freeze the reference and create the Main responsibility ledger

Files:
- Create: docs/migrations/main-server-responsibility-ledger.md
- Modify: progress.md
- Test: read-only inventory commands

### Step 1: Inventory the reference

Record size, line count, last-write time and SHA-256 for Main.cs and direct server dependencies:
WorldGen.cs, Player.cs, NPC.cs, Projectile.cs, Item.cs, MessageBuffer.cs, NetMessage.cs,
WorldFile.cs, Collision.cs, Liquid.cs, Wiring.cs, Chest.cs, Sign.cs, RemoteClient.cs and Tile.cs.

### Step 2: Classify every Main field and method

Use only these classifications:

- ServerState
- Definition
- SimulationSystem
- CommandInput
- Snapshot
- Projection
- ExcludedClient
- Unknown

### Step 3: Acceptance

Every server-relevant symbol has one destination or an explicit Unknown reason. Missing physical
source files are never treated as empty implementations.

### Step 4: Commit

~~~powershell
git add docs/migrations/main-server-responsibility-ledger.md progress.md
git commit -m "docs: inventory Main server responsibilities"
~~~

---

## Task 1: Add the server-only boundary verifier

Status: Complete on 2026-08-18. The focused verifier is registered in the solution and passed
against the current Simulation source; the root solution build timed out and is not treated as
verified.

Files:
- Create: Test/Terraria.Dome.MainBoundary.Verification/Terraria.Dome.MainBoundary.Verification.csproj
- Create: Test/Terraria.Dome.MainBoundary.Verification/Program.cs
- Modify: Terraria.Dome.sln

### Step 1: Write the failing verifier

Scan Simulation source and resolved project references. Fail on Terraria.Main, Main.player,
Main.npc, Main.tile, Main.rand, Microsoft.Xna, System.Windows.Forms, GraphicsDevice,
UserInterface, socket types or Protocol types.

### Step 2: Run RED

~~~powershell
dotnet run --project Test\Terraria.Dome.MainBoundary.Verification\Terraria.Dome.MainBoundary.Verification.csproj -c Release -p:UseSharedCompilation=false
~~~

Expected: report real source violations separately from ignored Build artifacts.

### Step 3: Implement the executable verifier

Print every violating path and exit non-zero. Do not silently pass when a directory is missing.

### Step 4: Run GREEN and commit

Run the focused verifier and root Release build. Commit only the project, verifier and solution
registration.

---

## Task 2: Extract explicit world clock state

Status: Complete on 2026-08-18. The focused clock, world-rule and boundary verifiers passed, the
Simulation Release build passed, and the root Release solution build passed with zero warnings
and zero errors.

Files:
- Create: src/Terraria.Dome.Simulation/World/WorldClock.cs
- Create: src/Terraria.Dome.Simulation/World/Systems/WorldClockSystem.cs
- Modify: src/Terraria.Dome.Simulation/Snapshots/DomeSimulationSnapshot.cs
- Modify: src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs
- Create: Test/Terraria.Dome.WorldClock.Verification/Terraria.Dome.WorldClock.Verification.csproj
- Create: Test/Terraria.Dome.WorldClock.Verification/Program.cs
- Modify: Terraria.Dome.sln

### Step 1: Write RED scenarios

Verify fixed-tick advancement, day/night boundaries, paused time and continuation from an equal
snapshot. Include exact boundary tests for day start, night start and a full cycle.

### Step 2: Implement the minimal clock

Move only tick number, time of day, day/night state, pause policy and configured rate behind an
explicit WorldClock. Do not copy GameTime, rendering timers or frame counters.

### Step 3: Integrate the tick phase

Call WorldClockSystem from DomeSimulation.Tick in a named phase before gameplay systems. The system
must not read wall-clock time or process-global random state.

### Step 4: Verify and commit

Run the focused verifier, existing world-rule verifier and Release build.

---

## Task 3: Extract world metadata, seed and progression state

Status: Complete on 2026-08-19. Persistence, world import, world-rule and Main-boundary
verifiers passed. The root Release build passed with zero warnings and zero errors.

Files:
- Create: src/Terraria.Dome.Simulation/World/WorldProgressionState.cs
- Create: src/Terraria.Dome.Simulation/World/WorldRuleState.cs
- Modify: src/Terraria.Dome.Simulation/World/WorldMetadata.cs
- Modify: src/Terraria.Dome.Simulation/Snapshots/DomeSimulationSnapshot.cs
- Modify: src/Terraria.Dome.Server/Persistence/DomeStatePersistenceFormat.cs
- Modify: Test/Terraria.Dome.WorldImport.Verification/Program.cs
- Modify: Test/Terraria.Dome.Persistence.Verification/Program.cs

### Step 1: Write RED bootstrap and persistence cases

Use a fixed seed and metadata to assert equivalent initial snapshots, distinct seeds to assert
distinct generation state, invalid dimensions to reject, and persistence to preserve clock, seed,
spawn, world flags and progression.

### Step 2: Implement value-only world state

Move dimensions, spawn coordinates, seed flags, hard-mode/progression flags and event state into
named records/classes. Keep background, map and UI values out. Do not create a catch-all MainState.

### Step 3: Integrate restore and projection

Extend DomeSimulationSnapshot and DomeStatePersistenceFormat only for values with a verifier.
Malformed, duplicate and trailing data must reject without changing the live simulation.

### Step 4: Verify and commit

Run world bootstrap, persistence and world-rule verifiers, then Release build.

---

## Task 4: Split the Main update loop into explicit simulation phases

Status: Complete on 2026-08-19 for the focused Task 4 gate. The schedule is explicit and
deterministic, and the server now emits the complete player authority stream after spawn. The
remaining loopback failures listed in `progress.md` are separate lifecycle, default-world and
session-projection gaps; they are not used to claim this focused task green.

Files:
- Create: src/Terraria.Dome.Simulation/Tick/SimulationTickContext.cs
- Create: src/Terraria.Dome.Simulation/Tick/SimulationTickSchedule.cs
- Modify: src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs
- Create: Test/Terraria.Dome.TickOrder.Verification/Terraria.Dome.TickOrder.Verification.csproj
- Create: Test/Terraria.Dome.TickOrder.Verification/Program.cs

### Step 1: Write RED ordering assertions

Record phase names and assert the order: input, movement, collision, combat, structural commit
and snapshot publication. Assert stable ordering for equal-sequence commands.

### Step 2: Implement a named schedule

Start with the existing proven order:

~~~text
BeginTick
ApplyWorldClock
ApplyPlayerInputs
ApplyPlayerControl
ResolveTileCollision
SelectNpcTargets
ApplyNpcAi
MoveEntities
AdvanceProjectiles
ResolveCombat
CommitDomainCommands
PublishSnapshot
EndTick
~~~

Do not introduce a general scheduler framework. DomeSimulation.Tick orchestrates named systems.

### Step 3: Verify determinism

Run two simulations from the same snapshot and input batch. Compare phase trace, command order
and final snapshot. Reverse input collection order and assert the documented sequence key wins.

### Step 4: Verify and commit

Run tick-order, base, player, combat and world-rule verifiers.

---

## Task 5: Extract server entity lifecycle ownership

Status: Complete for the current source tree on 2026-08-19. Player, NPC, projectile and world-item
stores passed their focused ownership contracts. Player lifecycle and combat loopbacks passed after
the final serial Release build; the lifecycle verifier also passed three consecutive replay runs.

Files:
- Create: src/Terraria.Dome.Simulation/Players/PlayerStore.cs
- Create: src/Terraria.Dome.Simulation/Players/PlayerLifecycleSystem.cs
- Create: src/Terraria.Dome.Simulation/Npc/NpcStore.cs
- Create: src/Terraria.Dome.Simulation/Projectile/ProjectileStore.cs
- Create: src/Terraria.Dome.Simulation/Items/WorldItemStore.cs
- Modify: src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs
- Modify: src/Terraria.Dome.Simulation/Snapshots/SimulationSnapshot.cs
- Modify: affected verifier projects

### Step 1: Write RED lifecycle cases

Verify server-created handles, unique replication IDs, duplicate destruction safety, disconnect
cleanup, inactive tombstones and snapshot exclusion of destroyed entities.

### Step 2: Extract one store at a time

Start with players because Server already maps session slots to PlayerHandle. Then extract NPC,
Projectile and WorldItem ownership. Stores may wrap Arch queries and typed dictionaries, but must
remain domain-specific and must not become a generic entity manager.

### Step 3: Preserve snapshot contracts

Keep existing PlayerSnapshot, NpcReplicationSnapshot, ProjectileReplicationSnapshot and
WorldItemSnapshot stable unless a verifier requires a field. Protocol never sees Arch entities.

### Step 4: Verify and commit each store separately

Run player, combat, item and loopback verifiers after each store. Do not combine all stores.

---

## Task 6: Move Main server bootstrap into Dome Server

Files:
- Modify: src/Terraria.Dome.Server/Startup/ServerLaunchOptions.cs
- Modify: src/Terraria.Dome.Server/Program.cs
- Modify: src/Terraria.Dome.Server/DomeServer.cs
- Create: src/Terraria.Dome.Server/Startup/WorldBootstrap.cs
- Modify: Test/Terraria.Dome.WorldImport.Verification/Program.cs

### Step 1: Write RED startup cases

Verify absolute world paths, explicit ports, deterministic default-world setup, server-owned spawn
selection and rejection of missing or malformed world input.

### Step 2: Implement the bootstrap boundary

Move only dedicated-server responsibilities from Main.DedServ, autoCreate, SetWorld, SetWorldName
and configuration methods. WorldBootstrap returns value-only metadata and a DomeSimulationSnapshot;
it does not create sockets or read client state.

### Step 3: Remove duplicate construction

There must be one default-world bootstrap path. Existing DomeServer.CreateDefaultWorld and
default object/NPC creation must not run twice.

### Step 4: Verify and commit

Run world import, persistence, server startup and full-client bootstrap verifiers.

---

## Task 7: Extract world rules and event families incrementally

Files:
- Create: src/Terraria.Dome.Simulation/World/Systems/WorldWeatherSystem.cs
- Create: src/Terraria.Dome.Simulation/World/Systems/WorldProgressionSystem.cs
- Modify: src/Terraria.Dome.Simulation/World/WorldRuleState.cs
- Modify: src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs
- Modify: Test/Terraria.Dome.WorldRules.Verification/Program.cs
- Modify: Test/Terraria.Dome.WorldRules.Loopback.Verification/Program.cs

### Step 1: Start with deterministic weather and time rules

Cover UpdateWeather, day/night transitions and the current world-rule projection. Each system
declares its input snapshot, output command/event and state revision.

### Step 2: Add progression as explicit state machines

For invasion, slime rain, meteor and moon events, first model state transitions and rejection
conditions. Do not port UI, audio, particles or global-array side effects.

### Step 3: Add one family per task

Each family gets a seed fixture, accepted transition, invalid transition, save/reload case and
outbound projection where applicable. Unsupported families remain rejected and documented.

### Step 4: Verify and commit

Run world-rule unit and loopback verifiers after each family. Keep manifest status partial until
all evidence columns exist.

---

## Task 8: Move only used static data into domain definitions

Files:
- Create: src/Terraria.Dome.Simulation/Items/Definitions/ItemDefinition.cs
- Create: src/Terraria.Dome.Simulation/Projectile/Definitions/ProjectileDefinition.cs
- Create: src/Terraria.Dome.Simulation/Npc/Definitions/NpcDefinition.cs
- Create: src/Terraria.Dome.Simulation/World/Definitions/TileDefinition.cs
- Modify: existing item, projectile, NPC and tile registries
- Modify: Test/Terraria.Dome.Items.Verification/Program.cs
- Modify: Test/Terraria.Dome.Combat.Verification/Program.cs

### Step 1: Inventory actual consumers

Identify which Main initialization tables are read by implemented behavior. Do not migrate the
complete tile, projectile, buff or item arrays merely because they exist in Main.cs.

### Step 2: Define immutable domain rules

Move only rules needed by supported behavior: collision flags, base damage, stack limits,
projectile lifetime, NPC base health and drop rules. Runtime values remain Components or Snapshots.

### Step 3: Verify registry determinism

Assert unique IDs, stable lookup, bounded values and identical definition hashes. Unknown IDs must
be rejected or represented as an explicit opaque compatibility record.

### Step 4: Verify and commit

Run definition and affected behavior verifiers. Do not add a generic GameData registry.

---

## Task 9: Extract pure calculations and remove Main reads

Files:
- Create: src/Terraria.Dome.Simulation/Combat/DamageCalculator.cs
- Create: src/Terraria.Dome.Simulation/World/WorldCoordinateRules.cs
- Modify: affected systems and DomeSimulation.cs
- Create: Test/Terraria.Dome.Calculation.Verification/Terraria.Dome.Calculation.Verification.csproj
- Create: Test/Terraria.Dome.Calculation.Verification/Program.cs

### Step 1: Write RED calculation cases

Cover damage, defense, range, world-boundary, section-coordinate and spawn-coordinate behavior
using explicit arguments only.

### Step 2: Implement pure functions

Move only functions expressible from parameters and immutable definitions. Do not copy methods
that implicitly read static arrays, global random state or client state.

### Step 3: Verify and commit

Compare boundary and representative values against reference behavior. Record intentional
differences in the ledger instead of adding hidden compatibility reads.

---

## Task 10: Reduce DomeSimulation to orchestration

Files:
- Modify: src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs
- Modify: src/Terraria.Dome.Simulation/Tick/SimulationTickSchedule.cs
- Modify: affected domain systems and snapshots
- Modify: Test/Terraria.Dome.Verification/Program.cs

### Step 1: Measure before editing

Record method count, line count and fields in DomeSimulation.cs. No behavior change is allowed
in this structural task.

### Step 2: Move implementation behind systems and stores

Keep public facade methods such as CreateSnapshot, CreatePersistenceSnapshot, CreatePlayer and
Tick. Move private domain implementation into named types with explicit contracts.

### Step 3: Verify behavioral parity

Run all existing executable verifiers for player authority, combat, items, world objects,
world rules, persistence, protocol and loopback. Compare fresh snapshot hashes for fixed fixtures
before and after extraction.

### Step 4: Commit structural batch

Do not mix new gameplay behavior, protocol expansion or generated-output cleanup into this commit.

---

## Task 11: Final boundary and acceptance audit

Files:
- Modify: docs/server-completion/completion-manifest.json only from fresh evidence
- Modify: docs/server-completion/capability-matrix.md
- Modify: progress.md
- Modify: responsibility ledger
- Test: every verifier project in Terraria.Dome.sln

### Step 1: Run source boundary scans

Confirm Simulation has no legacy Main or client dependency and no generated files are treated as
source. Confirm all Build output is below root Build.

### Step 2: Run the complete Release evidence set

~~~powershell
dotnet build Terraria.Dome.sln -c Release -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.MainBoundary.Verification\Terraria.Dome.MainBoundary.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.Verification\Terraria.Dome.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.WorldRules.Verification\Terraria.Dome.WorldRules.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.Persistence.Verification\Terraria.Dome.Persistence.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.Persistence.Loopback.Verification\Terraria.Dome.Persistence.Loopback.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.Hardening.Loopback.Verification\Terraria.Dome.Hardening.Loopback.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.Completion.Verification\Terraria.Dome.Completion.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
~~~

Run remaining solution verifiers serially when their family changes. Capture exit codes, warnings,
artifact paths and source/reference hashes.

### Step 3: Update status honestly

Mark a family evidenced only when fresh output contains reference, authority, projection and
execution evidence. A missing physical reference remains unknown or partial.

### Step 4: Commit documentation

~~~powershell
git diff --check
git add docs/migrations docs/server-completion progress.md
git commit -m "docs: record Main server ECS migration evidence"
~~~

## Stop conditions and rollback

Stop instead of widening scope when the relevant Version4 file is missing, the same root failure
repeats three times, a change requires adding a legacy Main dependency to Simulation, a system
needs client presentation state, equivalent replays diverge, attribution is blocked by unrelated
dirty files, or a verifier passes only after weakening an authority/rejection assertion.

Rollback is task-scoped. Revert only current task files or restore the previous task commit. Do
not use broad git reset --hard, git clean or recursive deletion.

## Definition of complete migration slice

A slice is complete only when its ledger row has a Dome authority owner; state is represented by
explicit world/component/definition/snapshot types; mutations occur through a named system and
deterministic command phase; accepted and rejected behavior has an executable verifier; persistence
and protocol projections are covered where relevant; Simulation has no Main or client dependency;
and fresh Release evidence plus remaining unsupported behavior are recorded in progress.md.
