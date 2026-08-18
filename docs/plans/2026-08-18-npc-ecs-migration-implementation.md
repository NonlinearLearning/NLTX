# NPC ECS Migration Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Migrate the approved first NPC behavior slice from the deleted Version4 `NPC.cs` source
shape into composable Arch ECS components and deterministic systems.

**Architecture:** `Terraria.Dome.Simulation` owns NPC components, definitions, commands, systems,
snapshots and deterministic commits. Shared movement, physics and combat capabilities remain in
their domains. Server and protocol projects consume immutable NPC snapshots and projection DTOs;
they never expose legacy `Terraria.NPC` or write authoritative NPC state.

**Tech Stack:** .NET 10, C#, Arch 2.1.0, existing `WorldGrid`, executable verification projects,
serial `dotnet` commands with `-p:UseSharedCompilation=false`.

---

## Scope and source map

The read-only source is `D:\TRbackup\Version4物理删除了某些文件\Terraria\NPC.cs`. The first slice
covers ordinary enemy spawn/target/chase/contact damage/death/loot, a town-NPC home skeleton, a
segmented-NPC relationship skeleton, replication snapshots and the `SyncNPC` projection. Boss
encounters, invasions, moon events, complete town services, all special `AI_###` methods, full
Buff behavior and complete Terraria parity remain explicitly excluded work packages.

Existing implementation anchors are `src\Terraria.Dome.Simulation\Simulation\DomeSimulation.cs`,
the current `Components\Npc*` files, `Combat\Commands\DamageNpcCommand.cs`,
`Snapshots\NpcSnapshot.cs`, `Snapshots\NpcReplicationSnapshot.cs`, `Physics\Systems\TileCollisionSystem.cs`
and `Loot\LootTable.cs`.

## Task 1: Freeze source and behavior coverage

**Files:**
- Create: `docs/research/2026-08-18-npc-migration-source-manifest.md`
- Create: `docs/research/2026-08-18-npc-migration-coverage.md`
- Test: `Test/Terraria.Dome.Npc.Verification/Program.cs`

**Step 1: Record source evidence**

Record the source path, SHA-256, line count, method count, AI marker count and the line anchors
for `Spawner`, `SetDefaults`, `AI`, `checkDead`, `NPCLoot` and `NewNPC`. The manifest is read-only
evidence and must not copy the source into `src`.

**Step 2: Define coverage records**

Create a table keyed by source behavior family with status `planned`, `partial`, `excluded` or
`verified`. Include an explicit reason for every excluded Boss/event/AI family.

**Step 3: Add the first failing verifier**

Assert that a deterministic simulation can create an ordinary NPC, select a player, move toward
the target, take damage, emit a death revision and create a deterministic drop. Build the verifier
and record the expected compile or assertion failure before implementation.

**Step 4: Commit the baseline contract**

Run the verifier once and commit only the source manifest, coverage document and failing verifier.

## Task 2: Add definitions and identity components

**Files:**
- Create: `src/Terraria.Dome.Simulation/Npc/Definitions/NpcDefinition.cs`
- Create: `src/Terraria.Dome.Simulation/Npc/Definitions/NpcDefinitionRegistry.cs`
- Create: `src/Terraria.Dome.Simulation/Npc/Components/NpcDefinitionComponent.cs`
- Create: `src/Terraria.Dome.Simulation/Npc/Components/NpcLifecycleComponent.cs`
- Create: `src/Terraria.Dome.Simulation/Npc/Components/NpcReplicationComponent.cs`
- Modify: `src/Terraria.Dome.Simulation/Simulation/NpcHandle.cs`

**Step 1: Write definition contract tests**

Verify that a definition has a stable definition ID, protocol NetId, maximum health, defense,
collider, behavior ID and loot table ID. Reject duplicate IDs and non-positive health.

**Step 2: Implement immutable definitions and registry**

Use an immutable `NpcDefinition` and a registry that rejects duplicates and unknown IDs. Do not
place per-instance fields or `float[] ai` in the definition.

**Step 3: Implement lifecycle and replication state**

Represent active/despawn state, remaining lifetime, replication ID and revision as components.
Keep protocol throttling state out of the gameplay definition.

**Step 4: Run focused verification**

Run the definition verifier and confirm unknown definitions fail before entity creation.

## Task 3: Split NPC target and behavior state

**Files:**
- Create: `src/Terraria.Dome.Simulation/Npc/Components/NpcBehaviorStateComponent.cs`
- Create: `src/Terraria.Dome.Simulation/Npc/Components/NpcTargetComponent.cs`
- Create: `src/Terraria.Dome.Simulation/Npc/Components/NpcSpawnStateComponent.cs`
- Create: `src/Terraria.Dome.Simulation/Npc/Systems/NpcTargetSelectionSystem.cs`
- Create: `src/Terraria.Dome.Simulation/Npc/Systems/NpcBehaviorSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Components/NpcTargetComponent.cs`
- Modify: `src/Terraria.Dome.Simulation/Components/NpcAiStateComponent.cs`

**Step 1: Add target-selection tests**

Cover nearest active player, invalid/dead target replacement, no-target behavior and deterministic
tie breaking by stable player handle.

**Step 2: Add typed behavior-state tests**

Cover ordinary chase state and town-home state. Assert that behavior state contains typed fields,
not an `ai` array or an unbounded dictionary.

**Step 3: Implement target and behavior systems**

Read a stable player/NPC snapshot, write target and movement-intent results, and avoid structural
world mutation inside the query. Preserve the current Dome behavior as the first compatibility
baseline.

**Step 4: Run pure simulation verification**

Run the NPC verifier and compare position/target snapshots against the pre-split baseline.

## Task 4: Move spawn logic behind commands

**Files:**
- Create: `src/Terraria.Dome.Simulation/Npc/Commands/SpawnNpcCommand.cs`
- Create: `src/Terraria.Dome.Simulation/Npc/Commands/DespawnNpcCommand.cs`
- Create: `src/Terraria.Dome.Simulation/Npc/Systems/NpcSpawnEligibilitySystem.cs`
- Create: `src/Terraria.Dome.Simulation/Npc/Systems/NpcSpawnCommitSystem.cs`
- Create: `src/Terraria.Dome.Simulation/Npc/Snapshots/NpcSpawnSnapshot.cs`
- Modify: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`

**Step 1: Write spawn failure cases**

Cover occupied tiles, invalid definitions, exhausted spawn budget, protected slots and duplicate
replication IDs. These cases must produce no partially initialized entity.

**Step 2: Implement eligibility as a pure command producer**

Translate the first ordinary-spawn rules from `NPC.Spawner` into a bounded snapshot-based system.
Do not port the entire `Spawner` class or read `Main` globals.

**Step 3: Implement one deterministic commit point**

Validate each command, create the entity, attach all required components and assign the next stable
NPC handle/replication ID. Reject invalid commands without mutating the world.

**Step 4: Run spawn/replay verification**

Run the same seed and inputs twice and require identical spawn command and snapshot sequences.

## Task 5: Extract movement and contact effects

**Files:**
- Create: `src/Terraria.Dome.Simulation/Movement/Components/MovementIntentComponent.cs`
- Create: `src/Terraria.Dome.Simulation/Npc/Systems/NpcMovementIntentSystem.cs`
- Create: `src/Terraria.Dome.Simulation/Npc/Systems/NpcContactEffectSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Physics/Systems/TileCollisionSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`

**Step 1: Add movement and collision assertions**

Verify chase movement reduces target distance, solid tiles stop the NPC, and contact damage is
emitted exactly once per configured cooldown.

**Step 2: Implement movement-intent translation**

Convert typed chase/home behavior into velocity intent. Let the existing shared movement and tile
collision systems resolve position and grounded state.

**Step 3: Implement contact command production**

Produce damage commands from collider overlap. Do not subtract health in the contact system.

**Step 4: Run movement verification**

Run focused executable verification and record exact tick numbers for movement, collision and
contact damage.

## Task 6: Add combat, lifecycle, death and deterministic loot

**Files:**
- Create: `src/Terraria.Dome.Simulation/Combat/Components/DefenseComponent.cs`
- Create: `src/Terraria.Dome.Simulation/Combat/Components/HitImmunityComponent.cs`
- Create: `src/Terraria.Dome.Simulation/Npc/Systems/NpcLifecycleSystem.cs`
- Create: `src/Terraria.Dome.Simulation/Npc/Systems/NpcDeathSystem.cs`
- Create: `src/Terraria.Dome.Simulation/Npc/Systems/NpcLootSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Combat/Commands/DamageNpcCommand.cs`
- Modify: `src/Terraria.Dome.Simulation/Loot/LootTable.cs`
- Modify: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`

**Step 1: Write combat and death tests**

Cover defense reduction, zero/negative damage rejection, immunity cooldown, death exactly once,
inactive revision publication and deterministic loot for the same world seed.

**Step 2: Implement command-based damage resolution**

Resolve damage from validated commands, update health and immunity, and emit a death event when
health reaches zero. Keep source identity in the command for later authority checks.

**Step 3: Implement lifecycle and death systems**

Separate despawn/despawn-timeout from death. A death system must not directly call protocol code;
it emits loot, event and replication commands.

**Step 4: Implement loot definition lookup**

Replace the single hard-coded roll with a small immutable loot definition contract while retaining
the existing deterministic seed behavior for the first NPC type.

**Step 5: Run combat verification**

Require one damage, one death event, one inactive revision and the expected world item snapshot.

## Task 7: Add town and segment skeletons

**Files:**
- Create: `src/Terraria.Dome.Simulation/Npc/Components/NpcHomeComponent.cs`
- Create: `src/Terraria.Dome.Simulation/Npc/Components/NpcSegmentComponent.cs`
- Create: `src/Terraria.Dome.Simulation/Npc/Systems/NpcHomeSystem.cs`
- Create: `src/Terraria.Dome.Simulation/Npc/Systems/NpcSegmentLifecycleSystem.cs`
- Create: `Test/Terraria.Dome.Npc.Composition.Verification/Program.cs`

**Step 1: Write composition tests**

Create one town NPC and one two-segment NPC. Assert that neither requires an NPC subclass and that
the segment root/parent/child relationship is stable through snapshot and restore.

**Step 2: Implement home state**

Store home tile, homeless state and timeout as component state. Implement only the first walking/
returning behavior; leave housing validation and services excluded.

**Step 3: Implement segment lifecycle**

Validate root ownership, parent existence, segment index and death ordering. Defer complete worm and
Boss-specific AI to a separate behavior-family package.

**Step 4: Run composition verification**

Run the composition verifier and require deterministic root/segment snapshots.

## Task 8: Build NPC snapshots and protocol projection

**Files:**
- Create: `src/Terraria.Dome.Simulation/Npc/Snapshots/NpcStateSnapshot.cs`
- Create: `src/Terraria.Dome.Server/Replication/NpcReplicationAssembler.cs`
- Create: `src/Terraria.Dome.Protocol.V1456/Npc/NpcSyncPacket.cs`
- Create: `src/Terraria.Dome.Protocol.V1456/Npc/NpcSyncPacketCodec.cs`
- Create: `src/Terraria.Dome.Protocol.V1456/Npc/NpcStateProjector.cs`
- Modify: `src/Terraria.Dome.Simulation/Snapshots/NpcReplicationSnapshot.cs`
- Modify: `src/Terraria.Dome.Simulation/Snapshots/DomeSimulationSnapshot.cs`

**Step 1: Add snapshot round-trip tests**

Verify that NPC definition, transform, velocity, health, target, lifecycle, revision and section
state survive snapshot creation and restore without Arch entity identity leakage.

**Step 2: Define projection DTOs**

Define the fields required by `SyncNPC`, including sparse AI flags. The DTO must contain no Arch
types and no legacy `NPC` reference.

**Step 3: Implement typed AI projection**

Map each supported first-slice behavior to the legacy four-slot wire representation in the projector
only. Unsupported behavior IDs must return a documented unsupported status instead of guessing.

**Step 4: Implement codec fixtures**

Add byte-level fixtures for spawn, update, life-width encoding, inactive revision and stable identity.
Run them against the protocol project serially.

## Task 9: Register the explicit tick pipeline

**Files:**
- Create: `src/Terraria.Dome.Simulation/Npc/NpcSystemPipeline.cs`
- Modify: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- Modify: `Test/Terraria.Dome.Npc.Verification/Program.cs`

**Step 1: Add order assertions**

Record system names and assert the exact order: eligibility, spawn commit, target, behavior,
movement intent, movement/collision, contact, damage, lifecycle, death, loot, replication.

**Step 2: Implement pipeline registration**

Register concrete systems explicitly in the simulation constructor or bootstrap boundary. Do not
derive order from namespaces, reflection or dictionary iteration.

**Step 3: Remove duplicated NPC work from `DomeSimulation`**

Move one behavior family at a time. Preserve public simulation APIs only where current verification
projects require them; route their implementation through the new systems.

**Step 4: Run the full first-slice verifier**

Run pure simulation, composition, snapshot and protocol verification projects serially and record
all exit codes and warnings.

## Task 10: Enforce the legacy dependency boundary

**Files:**
- Create: `Test/Terraria.Dome.Npc.Boundary.Verification/Program.cs`
- Modify: `docs/research/2026-08-18-npc-migration-coverage.md`
- Modify: `progress.md`

**Step 1: Add source-boundary checks**

Scan `src/Terraria.Dome.Simulation` for `Terraria.NPC`, `Main.npc`, `NetMessage`, `MessageBuffer`,
`ai[` and `localAI[`. Fail on new runtime references; allow the projector's documented wire-field
mapping only outside gameplay systems.

**Step 2: Run repository-root build checks**

Run the affected projects from the repository root with `-p:UseSharedCompilation=false`. Verify
that outputs remain below `Build/bin` and intermediates below `Build/obj`.

**Step 3: Update coverage and progress**

Record verified batches, remaining excluded behavior families, command exit statuses and warnings.
Do not describe the result as complete Terraria NPC parity.

**Step 4: Commit the first-slice migration**

Commit only the approved NPC migration files, tests and evidence documents. Preserve unrelated
working-tree changes.

## Suggested verification commands

Run serially from `D:\TRbackup\NLTX` after each relevant task:

```powershell
dotnet build .\src\Terraria.Dome.Simulation\Terraria.Dome.Simulation.csproj `
  -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.Npc.Verification\Terraria.Dome.Npc.Verification.csproj `
  -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.Npc.Composition.Verification\Terraria.Dome.Npc.Composition.Verification.csproj `
  -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.Npc.Boundary.Verification\Terraria.Dome.Npc.Boundary.Verification.csproj `
  -p:UseSharedCompilation=false
```

The projects in this plan do not yet exist; their first expected result is a deliberate failure
until the corresponding task creates them. Every later claim must include the actual exit status.

## Execution notes

- Keep each task in a focused commit; do not mix NPC migration with unrelated dirty-tree changes.
- Do not restore `Version4` `NPC.cs` or add a legacy NPC adapter to Simulation.
- Use the repository's Google C# constraints: PascalCase types/files, `Npc`/`Ai`/`Id` naming,
  2-space indentation, braces on control flow, 100-column maximum and read-only input contracts.
- For any behavior-family expansion, add a new coverage row, source mapping, failing test, minimal
  implementation, replay evidence and protocol fixture before marking it verified.
