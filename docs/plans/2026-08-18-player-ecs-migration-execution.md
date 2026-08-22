# Player ECS Migration Execution Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Replace the legacy `Terraria.Player` runtime responsibilities with a server-authoritative ECS player core, then migrate the remaining player capabilities in independently verifiable slices.

**Architecture:** Use domain-owned components, systems, commands, events, and snapshots. The simulation owns mutable state; protocol and server layers validate input and project immutable snapshots; the legacy `Player.cs` remains a read-only behavior oracle and is never compiled into the new simulation.

**Tech Stack:** .NET 10, C#, Arch ECS 2.1, existing `Terraria.Dome.Simulation`, `Terraria.Dome.Server`, `Terraria.Dome.Protocol.V1456`, console verification projects, deterministic replay fixtures.

---

## Execution Rules

- Work from the repository root and use `-p:UseSharedCompilation=false` for
  serial build/run commands.
- Preserve unrelated worktree changes. The current checkout contains user and
  generated changes; only files named by a task may be staged for that task.
- Do not copy `D:\TRbackup\Version4物理删除了某些文件\Terraria\Player.cs`
  into `src/`. Read it only to classify behavior and define oracle fixtures.
- Do not introduce a `PlayerManager`, `PlayerHelper`, `PlayerData`, or a
  catch-all `PlayerComponent`.
- Do not move all existing components in one formatting-only change. New code
  follows the domain-first layout; existing locations move only when a task has
  a compiling compatibility step and a focused verification.
- A phase is incomplete until its focused verifier, the simulation build, and
  the relevant loopback/protocol verifier pass.

## Target Directory Shape

The target shape is incremental. Existing files may remain in their current
location until their owning task moves them with compatibility updates.

```text
src/Terraria.Dome.Simulation/
  Player/
    Components/
      PlayerTagComponent.cs
      PlayerIdentityComponent.cs
      PlayerLifecycleComponent.cs
      PlayerInputComponent.cs
      PlayerControlStateComponent.cs
      PlayerInteractionComponent.cs
    Systems/
      PlayerInputApplySystem.cs
      PlayerControlSystem.cs
      PlayerRespawnSystem.cs
      PlayerDeathSystem.cs
      PlayerVitalRegenSystem.cs
      PlayerItemUseSystem.cs
    Commands/
      DamagePlayerCommand.cs
      RespawnPlayerCommand.cs
      UsePlayerInteractionCommand.cs
    Events/
      PlayerDamagedEvent.cs
      PlayerDiedEvent.cs
      PlayerRespawnedEvent.cs
  Combat/
    Components/
      HealthComponent.cs
      ManaComponent.cs
      DefenseComponent.cs
      ImmunityComponent.cs
    Systems/
      DamageResolutionSystem.cs
      ImmunitySystem.cs
  Movement/
    Components/
      MovementIntentComponent.cs
      MotionLockComponent.cs
    Systems/
      MovementSystem.cs
      GravitySystem.cs
  Inventory/
    Components/
      InventoryComponent.cs
      SelectedItemComponent.cs
      EquipmentLoadoutComponent.cs
      ItemUseStateComponent.cs
    Systems/
      InventoryTransferSystem.cs
      ItemSelectionSystem.cs
      EquipmentStatSystem.cs
  StatusEffects/
    Components/BuffCollectionComponent.cs
    Systems/BuffDurationSystem.cs
    Systems/BuffEffectSystem.cs
  Snapshots/
    PlayerSnapshot.cs
    PlayerStateSnapshot.cs
    PlayerInventorySnapshot.cs
```

## Phase 0: Freeze the Legacy Behavior Baseline

**Purpose:** Make the physically deleted legacy file usable as evidence without
making it a source dependency.

**Files:**

- Create: `docs/migrations/player-legacy-behavior-map.md`
- Create: `docs/migrations/player-legacy-method-status.csv`
- Create: `Build/diagnostics/player-legacy-source-manifest.json`
- Test fixture: `Test/Terraria.Dome.PlayerSimulation.Verification/Fixtures/PlayerCoreScenarios.json`

**Actions:**

1. Record the legacy path, byte length, last-write time, SHA-256, and source
   retrieval command in the manifest.
2. Classify every public/protected method and state cluster into `Core`,
   `Inventory`, `Combat`, `StatusEffects`, `Interaction`, `Persistence`,
   `Replication`, `Presentation`, or `DeferredSpecialCase`.
3. Record one of `Migrated`, `Delegated`, `ClientOnly`, or `Blocked` for every
   classified cluster. `Blocked` entries must name the missing dependency.
4. Define deterministic fixtures for input movement, jump/gravity, damage,
   death/respawn, inventory use, and server-record restoration.

**Verification:**

```powershell
Get-FileHash 'D:\TRbackup\Version4物理删除了某些文件\Terraria\Player.cs' -Algorithm SHA256
rg -n 'Migrated|Delegated|ClientOnly|Blocked' docs\migrations\player-legacy-method-status.csv
```

Expected: the manifest identifies the exact source artifact, and every legacy
cluster has an explicit status. No generated build output is written under
`src/`.

## Phase 1: Establish Player Identity and Lifecycle Components

**Files:**

- Create: `src/Terraria.Dome.Simulation/Player/Components/PlayerIdentityComponent.cs`
- Move or compatibility-update: `src/Terraria.Dome.Simulation/Components/PlayerTagComponent.cs`
- Move or compatibility-update: `src/Terraria.Dome.Simulation/Player/Components/PlayerLifecycleComponent.cs`
- Modify: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- Modify: `src/Terraria.Dome.Simulation/Snapshots/PlayerSnapshot.cs`
- Modify: `src/Terraria.Dome.Simulation/Snapshots/PlayerStateSnapshot.cs`
- Create: `Test/Terraria.Dome.PlayerLifecycle.Verification/Terraria.Dome.PlayerLifecycle.Verification.csproj`
- Create: `Test/Terraria.Dome.PlayerLifecycle.Verification/Program.cs`
- Test: `Test/Terraria.Dome.PlayerLifecycle.Loopback.Verification/Program.cs`

**Actions:**

1. Store stable handle, assigned slot, and canonical account UUID in the
   identity component.
2. Keep active/inactive state, respawn countdown, and spawn point in the
   lifecycle component.
3. Make `CreatePlayer`, `DestroyPlayer`, `CreatePlayerStateSnapshot`, and
   `DestroySessionPlayerCommand` use the components as the single runtime
   source of truth.
4. Preserve the existing persistent account import/restore behavior; do not
   make session state the persistence record.

**Verification command:**

```powershell
dotnet run --project .\Test\Terraria.Dome.PlayerLifecycle.Verification\Terraria.Dome.PlayerLifecycle.Verification.csproj -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.PlayerLifecycle.Loopback.Verification\Terraria.Dome.PlayerLifecycle.Loopback.Verification.csproj -p:UseSharedCompilation=false
```

Expected: a created player has identity and spawn state, disconnect removes the
runtime entity without deleting the account record, and reconnect restores the
server-owned account.

## Phase 2: Normalize Input and Movement into a Deterministic Pipeline

**Files:**

- Move or compatibility-update: `src/Terraria.Dome.Simulation/Components/PlayerInputComponent.cs`
- Move or compatibility-update: `src/Terraria.Dome.Simulation/Components/PlayerControlStateComponent.cs`
- Create: `src/Terraria.Dome.Simulation/Movement/Components/MovementIntentComponent.cs`
- Modify: `src/Terraria.Dome.Simulation/Player/Systems/PlayerInputApplySystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Player/Systems/PlayerControlSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Player/Systems/PlayerGravitySystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Movement/Systems/MovementSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Physics/Systems/TileCollisionSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Physics/Systems/GroundCollisionSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- Create: `Test/Terraria.Dome.PlayerSimulation.Verification/Terraria.Dome.PlayerSimulation.Verification.csproj`
- Create: `Test/Terraria.Dome.PlayerSimulation.Verification/Program.cs`
- Test: `Test/Terraria.Dome.Combat.Verification/Program.cs`

**Actions:**

1. Apply exactly one authoritative input value per player per tick; reject
   unknown handles and define duplicate-input behavior explicitly.
2. Convert input to movement intent and action requests before changing
   velocity. Do not let protocol DTOs enter physics systems.
3. Preserve facing, grounded state, jump impulse, gravity, tile collision, and
   deterministic system order.
4. Add replay fixtures that run the same initial snapshot and input sequence
   twice and compare every player snapshot field.

**Verification command:**

```powershell
dotnet run --project .\Test\Terraria.Dome.PlayerSimulation.Verification\Terraria.Dome.PlayerSimulation.Verification.csproj -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.Combat.Verification\Terraria.Dome.Combat.Verification.csproj -p:UseSharedCompilation=false
```

Expected: left/right input changes only the intended player, jumping requires
ground contact, gravity and collision are deterministic, and no system reads
legacy Terraria or XNA types.

## Phase 3: Migrate Vitals, Damage, Death, and Respawn

**Files:**

- Create: `src/Terraria.Dome.Simulation/Combat/Components/ManaComponent.cs`
- Create: `src/Terraria.Dome.Simulation/Combat/Components/DefenseComponent.cs`
- Create: `src/Terraria.Dome.Simulation/Combat/Components/ImmunityComponent.cs`
- Create: `src/Terraria.Dome.Simulation/Combat/Systems/DamageResolutionSystem.cs`
- Create: `src/Terraria.Dome.Simulation/Combat/Systems/ImmunitySystem.cs`
- Create: `src/Terraria.Dome.Simulation/Player/Systems/PlayerDeathSystem.cs`
- Create: `src/Terraria.Dome.Simulation/Player/Systems/PlayerRespawnSystem.cs`
- Create: `src/Terraria.Dome.Simulation/Player/Systems/PlayerVitalRegenSystem.cs`
- Create: `src/Terraria.Dome.Simulation/Player/Events/PlayerDamagedEvent.cs`
- Create: `src/Terraria.Dome.Simulation/Player/Events/PlayerDiedEvent.cs`
- Create: `src/Terraria.Dome.Simulation/Player/Events/PlayerRespawnedEvent.cs`
- Modify: `src/Terraria.Dome.Simulation/Player/Commands/DamagePlayerCommand.cs`
- Modify: `src/Terraria.Dome.Simulation/Player/Commands/RespawnPlayerCommand.cs`
- Modify: `src/Terraria.Dome.Simulation/Components/HealthComponent.cs`
- Modify: `src/Terraria.Dome.Simulation/Snapshots/PlayerStateSnapshot.cs`
- Test: `Test/Terraria.Dome.Combat.Verification/Program.cs`
- Test: `Test/Terraria.Dome.PlayerLifecycle.Loopback.Verification/Program.cs`

**Actions:**

1. Queue damage and resolve it in one deterministic system; callers must not
   mutate `HealthComponent` directly.
2. Emit death only on the alive-to-dead transition and make respawn consume a
   typed command after the configured delay.
3. Define health/mana bounds, regeneration delay, immunity windows, and
   duplicate damage ordering as explicit invariants.
4. Project health, maximum health, mana, active state, and respawn state into
   snapshots used by server replication.

**Verification command:**

```powershell
dotnet run --project .\Test\Terraria.Dome.Combat.Protocol.Verification\Terraria.Dome.Combat.Protocol.Verification.csproj -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.Combat.Loopback.Verification\Terraria.Dome.Combat.Loopback.Verification.csproj -p:UseSharedCompilation=false
```

Expected: valid damage changes health once, invalid targets are rejected,
death/respawn transitions are observable, and encoded vitals match the
post-commit snapshot.

## Phase 4: Split Inventory, Selection, Equipment, and Item Use

**Files:**

- Move or compatibility-update: `src/Terraria.Dome.Simulation/Items/InventoryComponent.cs`
- Create: `src/Terraria.Dome.Simulation/Inventory/Components/SelectedItemComponent.cs`
- Create: `src/Terraria.Dome.Simulation/Inventory/Components/EquipmentLoadoutComponent.cs`
- Create: `src/Terraria.Dome.Simulation/Inventory/Components/ItemUseStateComponent.cs`
- Create: `src/Terraria.Dome.Simulation/Inventory/Systems/ItemSelectionSystem.cs`
- Create: `src/Terraria.Dome.Simulation/Inventory/Systems/EquipmentStatSystem.cs`
- Create: `src/Terraria.Dome.Simulation/Player/Systems/PlayerItemUseSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Items/Systems/InventoryTransferSystem.cs`
- Modify: `src/Terraria.Dome.Simulation/Items/Commands/UseItemCommand.cs`
- Modify: `src/Terraria.Dome.Simulation/Items/Commands/PickupWorldItemCommand.cs`
- Modify: `src/Terraria.Dome.Simulation/Players/PlayerPersistentState.cs`
- Test: `Test/Terraria.Dome.Items.Verification/Program.cs`
- Test: `Test/Terraria.Dome.Items.Loopback.Verification/Program.cs`
- Test: `Test/Terraria.Dome.PlayerAuthority.Verification/Program.cs`

**Actions:**

1. Keep persistent state as the complete protocol-shaped record, including all
   supported equipment slots and unsupported item metadata.
2. Project only executable definitions into runtime inventory/equipment
   components; never silently discard imported slots.
3. Validate ownership, slot range, item definition, stack count, cooldown, and
   mana payment before emitting item-use effects.
4. Make selection, transfer, equipment stat refresh, and item use separate
   systems so item use cannot mutate account persistence directly.

**Verification command:**

```powershell
dotnet run --project .\Test\Terraria.Dome.Items.Verification\Terraria.Dome.Items.Verification.csproj -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.PlayerAuthority.Verification\Terraria.Dome.PlayerAuthority.Verification.csproj -p:UseSharedCompilation=false
```

Expected: a first UUID import preserves all protocol slots, runtime item use
only executes known definitions, and a later connection cannot overwrite the
server-owned record with different client equipment.

## Phase 5: Migrate Buffs and Shared Player Abilities

**Files:**

- Create: `src/Terraria.Dome.Simulation/StatusEffects/Components/BuffCollectionComponent.cs`
- Create: `src/Terraria.Dome.Simulation/StatusEffects/Systems/BuffDurationSystem.cs`
- Create: `src/Terraria.Dome.Simulation/StatusEffects/Systems/BuffEffectSystem.cs`
- Create: `src/Terraria.Dome.Simulation/Player/Components/PlayerInteractionComponent.cs`
- Create: `src/Terraria.Dome.Simulation/Player/Commands/UsePlayerInteractionCommand.cs`
- Modify: `src/Terraria.Dome.Simulation/Players/PlayerPersistentBuff.cs`
- Modify: `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- Test: `Test/Terraria.Dome.TileInteraction.Verification/Program.cs`
- Test: `Test/Terraria.Dome.WorldObjects.Verification/Program.cs`

**Actions:**

1. Define bounded buff identity, duration, source, and effect application.
2. Separate persistent bootstrap buffs from runtime effect state.
3. Validate player reach and ownership before translating tile, chest, sign, or
   door requests into world commands.
4. Keep world mutation in the existing deterministic world command/commit path.

**Verification command:**

```powershell
dotnet run --project .\Test\Terraria.Dome.TileInteraction.Verification\Terraria.Dome.TileInteraction.Verification.csproj -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.WorldObjects.Verification\Terraria.Dome.WorldObjects.Verification.csproj -p:UseSharedCompilation=false
```

Expected: invalid reach, inactive players, and forged owners are rejected;
valid interaction produces deterministic world commands and updated snapshots.

## Phase 6: Complete Protocol Projection and Client Delegation

**Files:**

- Create or modify: `src/Terraria.Dome.Server/Replication/PlayerStateProjection.cs`
- Create or modify: `src/Terraria.Dome.Server/Replication/PlayerBootstrapProjection.cs`
- Modify: `src/Terraria.Dome.Server/Replication/PlayerReplicationState.cs`
- Modify: `src/Terraria.Dome.Server/DomeServer.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Dispatch/TerrariaPacketDispatcher.cs`
- Modify: `src/Terraria.Dome.Protocol.V1456/Compatibility/LegacyPlayerControlsProjection.cs`
- Test: `Test/Terraria.Dome.SessionReplication.Verification/Program.cs`
- Test: `Test/Terraria.Dome.FullClientBootstrap.Verification/Program.cs`

**Actions:**

1. Make every player packet projection consume typed snapshots or persistent
   projections, never Arch entities or legacy objects.
2. Preserve player slot ownership, exact packet layouts, bootstrap order,
   equipment slot widths, and server-record-wins behavior.
3. Mark drawing, camera, audio, dust, UI, social cosmetics, and other
   client-only methods as delegated in the behavior map; do not add them to the
   simulation assembly.
4. Use the real-client fixture host only after focused protocol verifiers pass.

**Verification command:**

```powershell
dotnet run --project .\Test\Terraria.Dome.SessionReplication.Verification\Terraria.Dome.SessionReplication.Verification.csproj -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.FullClientBootstrap.Verification\Terraria.Dome.FullClientBootstrap.Verification.csproj -p:UseSharedCompilation=false
```

Expected: the captured bootstrap reaches world entry, active-session ownership
is enforced, and replication bytes are derived from server-owned snapshots.

## Phase 7: Full Regression and Removal Gate

**Files:**

- Modify: `docs/migrations/player-legacy-method-status.csv`
- Modify: `docs/migrations/player-legacy-behavior-map.md`
- Create: `Build/diagnostics/player-ecs-migration-evidence.json`

**Actions:**

1. Run the simulation build and all player, combat, items, world-object,
   protocol, session, and full-client verification projects serially.
2. Compare deterministic replay hashes for the committed core scenarios.
3. Confirm no simulation source references the legacy `Terraria.Player` or
   client-only framework types.
4. Update every behavior status and attach the verifier name or explicit
   blocker.
5. Only after all removal-gate checks pass, delete a compatibility adapter or
   old duplicate implementation. Physical deletion of the external legacy
   source is outside this repository and must not be used as proof of parity.

**Verification commands:**

```powershell
dotnet build .\src\Terraria.Dome.Simulation\Terraria.Dome.Simulation.csproj -p:UseSharedCompilation=false
dotnet build .\Terraria.Dome.sln -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.PlayerAuthority.Verification\Terraria.Dome.PlayerAuthority.Verification.csproj -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.SessionReplication.Verification\Terraria.Dome.SessionReplication.Verification.csproj -p:UseSharedCompilation=false
dotnet run --project .\Test\Terraria.Dome.FullClientBootstrap.Verification\Terraria.Dome.FullClientBootstrap.Verification.csproj -p:UseSharedCompilation=false
```

Expected: exit code 0 for every command. Report warnings separately; a green
focused verifier does not prove behavior coverage unless the behavior map and
evidence manifest are complete.

## Rollback and Stop Conditions

Pause a phase immediately when any of the following occurs:

- a command bypasses the simulation commit boundary;
- a server-owned persistent record can be overwritten by active-session input;
- a protocol projection reads mutable Arch storage after the tick boundary;
- a large deletion has no method-status evidence or target-derived reference;
- a focused verifier passes only because an unknown packet or invalid owner is
  silently ignored;
- deterministic replay diverges without a documented rule change.

Rollback is phase-local: disable the new system registration, retain the typed
input/command adapter, and restore the previous projection path. Do not reset
the worktree, delete broad `Build/` directories, or revert unrelated user
changes. Persistent format changes require a backward-compatible reader before
the new writer is enabled.

## Completion Definition

The migration is complete only when the behavior map has no unexplained
`Blocked` core entries, the server owns all mutable player state, protocol
compatibility is verified from snapshots, deterministic replay is stable, and
client-only responsibilities are explicitly delegated rather than hidden in
simulation systems.
