# Dome Core Server 90 Percent Implementation Plan

> **For Codex:** REQUIRED SUB-SKILL: Use `executing-plans` to implement this plan
> task-by-task. Do not treat a packet catalog, an API declaration, a client-side UI
> response, or a green build as gameplay completion without the evidence defined here.

**Goal:** Evolve `Terraria.Dome` from a verified ECS server slice into a server-authoritative,
multiplayer Terraria core whose explicitly scoped server behavior is at least 90 percent
complete relative to the retained `Version4` reference.

**Architecture:** `Terraria.Dome.Simulation` owns deterministic gameplay state,
commands, systems, snapshots, and world mutation. `Terraria.Dome.Server` owns sessions,
identity, tick scheduling, validation, persistence orchestration, interest management, and
outbound fan-out. `Terraria.Dome.Protocol.V1456` only translates between V1456 frames and
typed intents or immutable replication snapshots. No new implementation may reintroduce
`Main`, `Netplay.Clients`, `RemoteClient`, `NetMessage`, `MessageBuffer`, `whoAmI`, legacy
entity instances, or client-owned authority into Simulation.

**Tech Stack:** .NET 10, C#, Arch 2.1.0, TCP, Terraria V1456 binary frames, executable
verification projects, `System.IO.Compression`, deterministic serial MSBuild runs.

---

## 1. Scope and the 90 Percent Gate

### 1.1 What the percentage means

The percentage measures **core playable dedicated-server behavior**, not source line count,
number of V1456 message IDs named by a catalog, or client presentation behavior. A behavior
family only contributes to the score when all four proof columns are present:

| Proof column | Required evidence |
|---|---|
| Reference | A concrete `Version4` file, method, or message path establishes the server responsibility. |
| Authority | A Dome Simulation or Server type owns the mutable state and makes the decisive decision. |
| Projection | V1456 has a typed intent or outbound snapshot/frame that represents the authoritative result. |
| Execution | A deterministic executable verifier, loopback path, or replay proves both accepted and rejected behavior. |

A capability marked **partial**, **framed**, **compatibility pass-through**, **cataloged**, or
**planned** contributes zero completion points. A capability with only a local unit verifier
contributes at most half of its family weight until a loopback or replay verifies the
authoritative boundary.

The completion score is the sum of fully evidenced family weights. The program may claim
the 90 percent gate only when at least 90 weighted points are evidenced and no safety-critical
family below is absent. The first three safety-critical families are world authority and
tile mutation, player authority and collision, and session/PVS replication.

### 1.2 Excluded work

The following do not count toward this plan's denominator: client renderer, UI, input UX,
audio, shaders, achievements, Steam/social integration, localization, single-player menu
flow, tooling/injection convenience, and protocol messages whose only effect is client
presentation. They may be compatible adapters but not evidence of server completion.

Exact byte-for-byte compatibility with every `Version4` behavior, secret seed, mod hook,
or platform integration is also not required by this 90 percent plan. The server must instead
be honest about every unsupported action and reject it before state mutation.

### 1.3 Weighted capability matrix

| ID | Server family | Weight | Current status on 2026-08-13 | 90 percent evidence required |
|---|---|---:|---|---|
| A | Tick, session identity, command ordering, disconnect lifecycle | 8 | Partial: TCP session and one simulation loop exist | Stable command sequence, disconnect destruction, bounded queues, multi-session test |
| B | World tiles, section snapshots, section PVS, authoritative mutation | 12 | Partial: `WorldGrid`, initial section stream, version cursor exist | Mutation queue, validation, movement subscriptions, dirty-section fan-out |
| C | Tile collision, player movement, spawn, death and respawn | 10 | Partial: floor-only collision and movement exist | Tile collision, lifecycle state, respawn and client state projection |
| D | Player inventory, health/mana, item use, pickups and drops | 12 | Missing beyond minimal projectile fire | Inventory authority, item definitions, validation, item world lifecycle and V1456 output |
| E | NPC lifecycle, AI, combat, drops and PVS replication | 12 | Partial: one chase AI and projectile hit exist | Spawn/despawn, targeting, damage, drops, delta snapshots and loopback |
| F | Projectile lifecycle, collision, damage, ownership and PVS replication | 8 | Partial: local ECS projectile only | Tile/entity collision, owner rules, protocol delta/kill and multi-session verification |
| G | Containers, signs, tile entities and interaction ownership | 8 | Missing | Section-local registries, exclusive/open state, item transfer and sync/rejection tests |
| H | World persistence, metadata, deterministic base generation and recovery | 10 | Missing | Versioned save/load, atomic replacement, replay hash and malformed-file rejection |
| I | Liquids, wires, doors, actuators and tile-object updates | 8 | Missing | Bounded deterministic work queues, tile state, supported actions and PVS updates |
| J | Core events, time, progression, spawn selection and world rules | 6 | Missing | Deterministic rule state, server tick behavior and session projections |
| K | V1456 authority coverage and compatibility hardening | 6 | Partial: bootstrap plus frame catalog exist | Typed routes only for backed domains, malformed input rejection, connection isolation |

The planned score is 100. Recalculate this table after every completed family; do not infer
progress from the amount of code written.

## 2. Evidence and Design Sources

The retained server reference is `D:\TRbackup\Version4物理删除了某些文件`.
It supplies behavior evidence, not implementation templates:

| Reference path | Relevant responsibility |
|---|---|
| `Terraria\MessageBuffer.cs`, `case 17` | Decode tile action, reject out-of-world input, require loaded section, rate-limit, apply and broadcast mutation. |
| `Terraria\NetMessage.cs`, `SendSection` | Session-owned section sent state; tile payload precedes NPC and chest synchronization for that section. |
| `Terraria\RemoteClient.cs`, `CheckSection_ForClient` | Interest is driven by a player/session position and current section activation. |
| `Terraria\Player.cs`, `NPC.cs`, `Projectile.cs`, `Item.cs` | Behavior-family source maps, not classes to copy into the ECS model. |
| `Terraria\WorldGen.cs`, `Liquid.cs`, `Chest.cs`, `Sign.cs` | Future domain maps for generation, liquid work queues, containers and sign state. |
| `Terraria\IO\WorldFile.cs` and world metadata call sites | Persistence responsibility and recovery boundary. |

`C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Server\Chunking\ChunkingSystem.cs`
is the architectural precedent for computing per-session visible chunks from authoritative
transform state. It validates the direction `session -> viewer position -> visible section set`
rather than client-declared visibility or a global broadcast list.

The prior NLS work provides a transferable rule: migrate a behavior family as
`input/read snapshot -> system -> deterministic command -> commit -> new snapshot`, rather
than copying legacy static methods. This plan applies that rule to Dome without importing NLS
source or its build boundaries.

## 3. Non-Negotiable Invariants

1. Simulation must never reference Server, Protocol, sockets, V1456 IDs, session slots or
   persistence transports.
2. Protocol must never mutate `WorldGrid`, Arch `World`, inventories, entity registries or
   persistent files. It only parses intents and encodes supplied immutable data.
3. A session owns a server-created player handle. Incoming player slot, position, item state,
   damage result and target IDs are assertions to validate, never authority to trust.
4. All state-changing external inputs receive a monotonic server sequence and commit during a
   designated Simulation tick phase. Equal-tick ordering is stable by sequence, then domain
   key where necessary.
5. Every outbound state item has a visibility decision. A player without a loaded/visible
   section must not receive that section's tiles, containers, NPCs, items or projectile state.
6. Every accepted mutation changes a precise version or revision. Replication compares revisions
   and never emits duplicate unchanged state to one session.
7. Every rejected malformed, out-of-range, unseen, unauthorised or rate-limited input leaves
   the authoritative state unchanged and cannot disconnect unrelated sessions.
8. Persistence uses an explicit format version, atomic write/replace and load validation;
   failure cannot partially replace the in-memory authoritative world.
9. Existing user worktree changes are not reverted. Generated output stays under root `Build/`.
10. The repository build commands run from root with `-p:UseSharedCompilation=false`.

## 4. Target Module Map

```text
Terraria.Dome.Simulation
  World/          tiles, world objects, section revisions, world rule snapshots
  Commands/       deterministic domain commands, rejection-safe values
  Player/         components and systems for state, inventory, item use, respawn
  Npc/            capability components, spawn/AI/combat systems
  Projectile/     lifecycle and collision systems
  Tick/           ordered queues and command commit phase
  Snapshots/      immutable replication-neutral state

Terraria.Dome.Server
  Sessions/       connection to PlayerHandle, lifecycle, quotas, teardown
  Validation/     ranges, ownership, visibility and action budgets
  Replication/    subscriptions, revision cursors and outbound batch assembly
  Persistence/    world service, save coordinator, metadata and recovery
  Protocol/       translates typed protocol intents to server commands

Terraria.Dome.Protocol.V1456
  Packets/        typed codec DTOs and binary encode/decode
  Dispatch/       state-checked intent routing
  Session/        V1456 transition state only
  Protocol/       message identifiers, catalog and malformed-frame validation

Test
  Terraria.Dome.*.Verification
                 executable family, replay and loopback verification projects
```

Do not create generic `Manager`, `Helper`, `Utility`, `Data` or all-purpose `EntityState`
types. A type must name its gameplay responsibility, for example `TileChangeCommand`,
`SessionActionBudget`, `WorldItemReplicationCursor`, or `WorldSaveCoordinator`.

## 5. Execution Protocol for Every Batch

Each batch is independent, reversible at the source-file level, and must follow this order:

1. Read the named Version4 method(s), current Dome counterpart and previous verifier.
2. Add a narrow executable verifier to a dedicated project or the closest existing verifier.
3. Run that verifier before implementation and record the expected RED reason.
4. Implement the smallest complete authority path; do not add future-domain fields merely to
   reserve names.
5. Run the new verifier in Release with `--no-build` after a root Release build.
6. Run the affected prior verifier projects, then the full solution Release build.
7. Check `git diff --check`, resolved MSBuild output paths when build settings change, and
   generated files outside root `Build/`.
8. Update `progress.md` with exact commands, exit status, warnings, scope and remaining
   behavior gaps. Update this plan's matrix only when all four proof columns are present.

Standard verification command:

```powershell
dotnet build Terraria.Dome.sln -c Release -p:UseSharedCompilation=false
```

Before starting any batch, run this read-only preflight and retain its output in that batch's
progress entry. It prevents a previous green result, an unrelated dirty change, or a stale
reference path from being used as current evidence:

```powershell
git status --short
git diff --check
dotnet sln Terraria.Dome.sln list
```

For a batch that changes build policy or a project reference, also inspect resolved values and
the new reference graph before its first build:

```powershell
dotnet msbuild <project>.csproj -nologo -getProperty:BaseOutputPath `
  -getProperty:BaseIntermediateOutputPath -getItem:ProjectReference `
  -p:UseSharedCompilation=false
```

Run a focused verifier using this form:

```powershell
dotnet run --project Test\<Verifier>\<Verifier>.csproj -c Release --no-build `
  -p:UseSharedCompilation=false
```

Never use an old successful build as evidence for new code. Do not stop or reuse a running
Debug server process; use Release builds and ephemeral loopback ports.

### 5.1 Matrix update rule

Only Batch 0's structured completion manifest may change a family score. A score update must
name the retained reference location, the owning Dome type, the protocol intent/snapshot or
state `NotApplicable` rationale, and the exact fresh verifier command plus exit status. If one
item is missing, leave the family at zero or partial status. A family with a local verifier but
without loopback/replay evidence may be described as partially verified, but it cannot add its
full weight to the 90 percent numerator.

## 6. Program Batches

### Batch 0: Baseline and Score Harness

**Purpose:** Make progress measurable before expanding behavior.

**Files:**
- Create: `docs\plans\2026-08-13-dome-core-server-90-percent-implementation.md`
- Create: `docs\server-completion\capability-matrix.md`
- Create: `Test\Terraria.Dome.Completion.Verification\Terraria.Dome.Completion.Verification.csproj`
- Create: `Test\Terraria.Dome.Completion.Verification\Program.cs`
- Modify: `Terraria.Dome.sln`
- Modify: `progress.md`

**Steps:**
1. Copy the weighted matrix from this plan into `docs\server-completion\capability-matrix.md`
   with columns for reference, authority type, projection type, verifier command, status and
   score.
2. Add a failing Completion verifier that reads an in-repo, structured completion manifest
   rather than parsing prose. It must reject a family that declares points without all evidence
   fields.
3. Implement a small explicit manifest model, stored in source rather than generated output.
   It reports the current validated score as low/partial, not 90.
4. Verify the score verifier, root Release build and that all produced paths remain under
   root `Build/`.

**Acceptance:** The plan and machine-readable matrix agree on the total of 100. The verifier
rejects unsupported claims and reports current score honestly.

### Batch 1: Authoritative Tile Manipulation and Dirty Section Fan-Out

**Reference:** `MessageBuffer.cs:case 17`, `NetMessage.cs:SendTileSquare`,
`RemoteClient.cs:CheckSection_ForClient`.

**Files:**
- Create: `src\Terraria.Dome.Protocol.V1456\Packets\TileManipulationIntent.cs`
- Create: `src\Terraria.Dome.Simulation\Commands\TileChangeCommand.cs`
- Create: `src\Terraria.Dome.Simulation\World\TileChangeKind.cs`
- Create: `src\Terraria.Dome.Server\Validation\TileInteractionValidator.cs`
- Create: `src\Terraria.Dome.Server\Replication\SessionReplicationState.cs`
- Create: `Test\Terraria.Dome.TileInteraction.Verification\Terraria.Dome.TileInteraction.Verification.csproj`
- Create: `Test\Terraria.Dome.TileInteraction.Verification\Program.cs`
- Modify: `Terraria.Dome.sln`
- Modify: `Terraria.Dome.Protocol.V1456\Protocol\TerrariaMessageId.cs`
- Modify: `Terraria.Dome.Protocol.V1456\Protocol\TerrariaMessageCatalog.cs`
- Modify: `Terraria.Dome.Protocol.V1456\Packets\TerrariaPacketCodec.cs`
- Modify: `Terraria.Dome.Protocol.V1456\Dispatch\TerrariaPacketDispatcher.cs`
- Modify: `Terraria.Dome.Protocol.V1456\Dispatch\TerrariaPacketDispatchResult.cs`
- Modify: `Terraria.Dome.Protocol.V1456\Session\TerrariaSession.cs`
- Modify: `src\Terraria.Dome.Server\Protocol\TerrariaProtocolCommand.cs`
- Modify: `src\Terraria.Dome.Server\Protocol\TerrariaProtocolSessionHost.cs`
- Modify: `src\Terraria.Dome.Server\DomeServer.cs`
- Modify: `src\Terraria.Dome.Server\Replication\WorldSectionReplication.cs`
- Modify: `src\Terraria.Dome.Simulation\World\WorldGrid.cs`
- Modify: `src\Terraria.Dome.Simulation\Tick\SimulationCommandQueue.cs`

**Steps:**
1. Write RED protocol assertions for the exact message-17 envelope. Decode only action `0`
   (`KillTile`) and `1` (`PlaceTile`) in this batch; actions `2..23` must fail explicitly.
2. Write RED Simulation assertions that enqueued commands commit in sequence order at the tick
   boundary, mutate exactly one tile, and increment only its containing section revision.
3. Write RED Server assertions for invalid coordinate, invisible section, unauthorised player,
   over-range target, unsupported action and action-budget exhaustion. Each must leave the
   `WorldGrid` unchanged.
4. Implement intent parsing without a reference from Protocol to Server or Simulation mutation.
5. Implement a value-only `TileChangeCommand` and a Simulation-owned commit entry point. Use
   server sequence number then X/Y/kind ordering. Do not call `WorldGrid.TrySetTile` directly
   from a socket read.
6. Bind session to server PlayerHandle. Derive tile-space interaction range from authoritative
   player snapshot, not the packet's position. Start with named constants and cover exact
   boundary tests.
7. Replace the initial-only replication cursor with a session replication state that keeps the
   visible section set and sent revision. On a completed tick, collect dirty visible snapshots,
   encode TileSection frames and write them to only eligible sessions.
8. Write a two-session loopback verifier: both sessions load a shared section; session A makes
   one valid mutation; both receive exactly one changed TileSection; session C without that
   section receives none. Decode payloads to assert the new authoritative tile value.

**Acceptance:** A V1456 message-17 mutation is validated, committed deterministically and
replicated only to eligible sessions. No unsupported action becomes a silent no-op success.

### Batch 2: Session Lifecycle, Moving PVS and Entity Replication Foundation

**Reference:** `RemoteClient.CheckSection`, `NetMessage.SendSection`, `NetMessage.SendData`
paths for entity synchronization.

**Files:**
- Create: `src\Terraria.Dome.Server\Sessions\ServerSession.cs`
- Create: `src\Terraria.Dome.Server\Sessions\ServerSessionRegistry.cs`
- Create: `src\Terraria.Dome.Server\Replication\SectionVisibilitySelector.cs`
- Create: `src\Terraria.Dome.Server\Replication\EntityReplicationCursor.cs`
- Create: `src\Terraria.Dome.Simulation\Snapshots\EntityReplicationSnapshot.cs`
- Create: `Test\Terraria.Dome.SessionReplication.Verification\Terraria.Dome.SessionReplication.Verification.csproj`
- Modify: `DomeServer.cs`, `TerrariaProtocolSessionHost.cs`, `WorldSectionReplication.cs`
- Modify: existing world and loopback verifier projects

**Steps:**
1. Write RED tests for session creation, server-owned player association, disconnect teardown,
   player destruction and cursor cleanup.
2. Add a visible section selector that derives a bounded section neighborhood from authoritative
   player position each tick. It must add newly visible sections, retain still visible sections,
   and remove departed sections.
3. Use the SS14 chunking precedent only for the per-session PVS shape; keep the Dome's fixed
   `200 x 150` section dimensions and avoid SS14 framework dependencies.
4. Add entity replication-neutral snapshots with stable server replication IDs, active revision
   and section location. Do not expose Arch `Entity` values to Protocol.
5. Verify move-across-section, disconnected cursor cleanup, new section initial stream and no
   duplicate emission for unchanged entities.

**Acceptance:** Interest management follows authoritative player movement and no session or
entity replication cursor survives disconnect.

### Batch 3: Tile Collision, Player Lifecycle and Authoritative Player State

**Reference:** relevant movement, spawn, hurt and death paths in `Player.cs` and
`MessageBuffer.cs`; existing `GroundCollisionSystem` is only a floor placeholder.

**Files:**
- Create: `src\Terraria.Dome.Simulation\Physics\Systems\TileCollisionSystem.cs`
- Create: `src\Terraria.Dome.Simulation\Player\Components\PlayerLifecycleComponent.cs`
- Create: `src\Terraria.Dome.Simulation\Player\Commands\RespawnPlayerCommand.cs`
- Create: `src\Terraria.Dome.Simulation\Snapshots\PlayerStateSnapshot.cs`
- Create: `src\Terraria.Dome.Server\Validation\PlayerStateValidator.cs`
- Create: `Test\Terraria.Dome.PlayerAuthority.Verification\Terraria.Dome.PlayerAuthority.Verification.csproj`
- Modify: Simulation movement/player systems, `DomeSimulation.cs`, V1456 player codecs and host

**Steps:**
1. Write RED tile collision tests: land on a solid tile, hit ceiling, stop at wall, and reject
   client position correction as authority.
2. Implement axis-separated, bounded collision against `WorldGrid` active tiles. Keep tile to
   simulation-coordinate conversion in one named component or service.
3. Write RED tests for damage, death state, respawn timer, server-chosen spawn location and
   active/inactive player replication.
4. Implement lifecycle commands in the tick queue; session controls cannot revive a player or
   overwrite server health.
5. Encode only the V1456 state fields backed by the new player snapshot; malformed or early
   client updates must be rejected in the session host.

**Acceptance:** Player movement and lifecycle are tile-authoritative, replayable and visible
to other eligible sessions.

### Batch 4: Inventory, Item Use, Pickups and World Items

**Reference:** `Item.cs`, `Player.cs`, MessageBuffer item messages `21`, `22`, `42` and item
ownership paths. This batch establishes a server item model before broad combat effects.

**Files:**
- Create: `src\Terraria.Dome.Simulation\Items\ItemStack.cs`
- Create: `src\Terraria.Dome.Simulation\Items\InventoryComponent.cs`
- Create: `src\Terraria.Dome.Simulation\Items\WorldItemComponent.cs`
- Create: `src\Terraria.Dome.Simulation\Items\Commands\UseItemCommand.cs`
- Create: `src\Terraria.Dome.Simulation\Items\Systems\ItemUseSystem.cs`
- Create: `src\Terraria.Dome.Server\Validation\ItemInteractionValidator.cs`
- Create: `src\Terraria.Dome.Protocol.V1456\Packets\ItemReplicationSnapshot.cs`
- Create: `Test\Terraria.Dome.Items.Verification\Terraria.Dome.Items.Verification.csproj`
- Modify: Simulation snapshots, Server replication, V1456 catalog/codecs and solution

**Steps:**
1. Define a deliberately small server item definition registry with stable IDs, stack limits,
   use category and authoritative effects. Do not copy `Item.cs` wholesale.
2. Write RED tests for inventory slot validation, stack split/merge, authoritative selected slot,
   pickup ownership, despawn and duplicate pickup rejection.
3. Implement inventory and world item commands in deterministic tick order.
4. Add V1456 projections only for item snapshots and intents whose state exists in Dome. Reject
   every unbacked message.
5. Verify two players racing for one item, an invalid selected slot, and PVS-limited item
   replication through loopback.

**Acceptance:** Items are server owned, transferable, replicated by visibility and cannot be
created by client packet assertions.

### Batch 5: NPC, Projectile, Combat and Loot Completion

**Reference:** `NPC.cs`, `Projectile.cs`, `NetMessage.SendSection` NPC sync, messages `23`,
`27`, `28`, `29`.

**Files:**
- Create: `src\Terraria.Dome.Simulation\Npc\NpcSpawnCommand.cs`
- Create: `src\Terraria.Dome.Simulation\Npc\NpcReplicationComponent.cs`
- Create: `src\Terraria.Dome.Simulation\Projectile\Systems\ProjectileCollisionSystem.cs`
- Create: `src\Terraria.Dome.Simulation\Combat\DamageResolutionSystem.cs`
- Create: `src\Terraria.Dome.Simulation\Loot\LootTable.cs`
- Create: `src\Terraria.Dome.Server\Replication\NpcReplicationAssembler.cs`
- Create: `src\Terraria.Dome.Server\Replication\ProjectileReplicationAssembler.cs`
- Create: `Test\Terraria.Dome.Combat.Verification\Terraria.Dome.Combat.Verification.csproj`
- Modify: current NPC/projectile systems, snapshots, V1456 codec/catalog, Server host

**Steps:**
1. Replace one-off NPC/projectile local snapshot assumptions with stable replication records:
   replication ID, type, transform, velocity, active revision and section.
2. Write RED tests for spawn, despawn, target selection, entity damage, immunity/cooldown,
   tile collision, projectile kill and deterministic loot roll from a seeded source.
3. Implement a small documented family of NPC AI and projectile types before generalizing.
   Each new family needs a reference map and replay verifier.
4. Encode message `23`, `27` and `29` only after their state records exist. Ensure entering a
   section sends its active entities and later revisions are deltas.
5. Run two-session combat loopback: one session causes combat, only eligible viewers receive
   state/damage/despawn, and the resulting drop has one server owner.

**Acceptance:** NPC, projectile, combat and loot behavior form an authoritative world loop,
not merely local simulation effects.

### Batch 6: Containers, Signs and Tile Entities

**Reference:** `Chest.cs`, `Sign.cs`, MessageBuffer messages `31`, `32`, `34`, `42`, and
`NetMessage.SyncChestContentsForSection`.

**Files:**
- Create: `src\Terraria.Dome.Simulation\WorldObjects\ChestComponent.cs`
- Create: `src\Terraria.Dome.Simulation\WorldObjects\SignComponent.cs`
- Create: `src\Terraria.Dome.Simulation\WorldObjects\TileEntityComponent.cs`
- Create: `src\Terraria.Dome.Simulation\WorldObjects\Commands\MoveContainerItemCommand.cs`
- Create: `src\Terraria.Dome.Server\Validation\ContainerInteractionValidator.cs`
- Create: `Test\Terraria.Dome.WorldObjects.Verification\Terraria.Dome.WorldObjects.Verification.csproj`
- Modify: WorldGrid/world object registry, replication, V1456 codecs and section stream

**Steps:**
1. Model containers and signs as section-local authoritative objects with stable IDs; do not
   store references in tiles or legacy static arrays.
2. Write RED tests for section-bound object discovery, interaction distance, one active opener
   policy where required, validated inventory transfer and disconnect closure.
3. Add initial section object synchronization after TileSection and later object-specific deltas.
4. Implement typed V1456 packet routes only for backed chest/sign operations and reject all
   object requests outside visible range.
5. Verify two player chest contention, sign changes, movement out of range and reconnect
   resynchronization.

**Acceptance:** Section-owned interactive state is authoritative and replication safe.

### Batch 7: Persistence, Metadata and Deterministic Base World Generation

**Reference:** `WorldFile` call sites, `WorldGen.cs`, world metadata in `Main.cs`. The goal is
recoverable Dome state, not an unverified byte-for-byte `.wld` clone.

**Files:**
- Create: `src\Terraria.Dome.Simulation\World\WorldMetadata.cs`
- Create: `src\Terraria.Dome.Simulation\World\WorldSeed.cs`
- Create: `src\Terraria.Dome.Simulation\WorldGeneration\WorldGenerationRequest.cs`
- Create: `src\Terraria.Dome.Simulation\WorldGeneration\WorldGenerationPipeline.cs`
- Create: `src\Terraria.Dome.Server\Persistence\WorldSaveCoordinator.cs`
- Create: `src\Terraria.Dome.Server\Persistence\WorldPersistenceFormat.cs`
- Create: `src\Terraria.Dome.Server\Persistence\WorldRecoveryResult.cs`
- Create: `Test\Terraria.Dome.Persistence.Verification\Terraria.Dome.Persistence.Verification.csproj`
- Modify: Server construction/startup, world snapshot APIs, solution and progress

**Steps:**
1. Write RED round-trip tests for metadata, tiles, world objects, items, NPCs and deterministic
   seed state. Test malformed version, truncated payload and interrupted atomic replace.
2. Implement versioned persistence using a temporary file plus atomic replace. All IO lives in
   Server; Simulation provides immutable export/import values only.
3. Implement a small deterministic base generator with named passes: surface/ground, spawn
   clearing, caves/ore only when independently tested. Keep a stable seed-to-world hash test.
4. Write a save, restart and two-session reload loopback test. Confirm the saved tile and
   section revision are visible after reload.
5. Add backup/recovery policy and failure telemetry that does not mutate a live world on a
   failed load.

**Acceptance:** A world can be created, saved, recovered and replayed safely, with deterministic
base generation evidence.

### Batch 8: Liquid, Wiring, Doors and Tile Object Mechanics

**Reference:** `Liquid.cs`, `WorldGen` tile action paths, message-17 actions `2..23`.

**Files:**
- Create: `src\Terraria.Dome.Simulation\World\LiquidCell.cs`
- Create: `src\Terraria.Dome.Simulation\World\LiquidWorkQueue.cs`
- Create: `src\Terraria.Dome.Simulation\World\WireComponent.cs`
- Create: `src\Terraria.Dome.Simulation\World\DoorStateComponent.cs`
- Create: `src\Terraria.Dome.Simulation\World\Commands\ToggleDoorCommand.cs`
- Create: `Test\Terraria.Dome.WorldMechanics.Verification\Terraria.Dome.WorldMechanics.Verification.csproj`
- Modify: world tile state/snapshot encoding, TileManipulation validation, replication and save

**Steps:**
1. Extend `WorldTile` only after a specific mechanic needs a field. Use versioned snapshot and
   persistence migrations; do not create a speculative monolithic tile structure.
2. Write RED deterministic work-budget tests: equivalent ordered inputs lead to identical
   liquid/wire results and no tick exceeds its configured work budget.
3. Add supported tile action kinds one at a time: wall, wire, door, actuator, slope. Each needs
   validation, commit, revision and V1456 output before the next action is enabled.
4. Add liquid state only with section-local dirty signaling and explicit client projection.
5. Verify adjacent-section effects, changed PVS boundaries and save/load preservation.

**Acceptance:** Common world mechanics are bounded, deterministic, persisted and replicated.

### Batch 9: World Rules, Events, Spawning and Progression

**Reference:** time/event, spawn and boss paths in `Main.cs`, `WorldGen.cs`, `NPC.cs`, and
message catalog entries. Avoid importing client FX/events that lack server state.

**Files:**
- Create: `src\Terraria.Dome.Simulation\WorldRules\WorldTimeComponent.cs`
- Create: `src\Terraria.Dome.Simulation\WorldRules\WorldEventComponent.cs`
- Create: `src\Terraria.Dome.Simulation\Npc\Systems\NpcSpawnSystem.cs`
- Create: `src\Terraria.Dome.Server\Replication\WorldRuleReplicationAssembler.cs`
- Create: `Test\Terraria.Dome.WorldRules.Verification\Terraria.Dome.WorldRules.Verification.csproj`
- Modify: tick pipeline, snapshots, V1456 projection, persistence and completion matrix

**Steps:**
1. Define a small explicit rules snapshot: game time, day/night, hardmode/progression flags,
   invasion/event state, deterministic spawn seed and active event revisions.
2. Write RED tests for ticked time, valid event transition, rejection of client-forced event
   state and deterministic spawn selection based on rules and world state.
3. Implement each supported event as a named state machine with legal transitions, snapshot and
   persistence migration.
4. Add V1456 outputs only for the supported rule state. Unsupported client request packets are
   rejected without advancing an event.
5. Verify save/load mid-event, reconnect projection and multi-session event visibility.

**Acceptance:** Core server world progression is explicit, deterministic and recoverable.

### Batch 10: Protocol Hardening, Replay, Load and 90 Percent Audit

**Purpose:** Close safety and evidence gaps rather than inflating feature count.

**Files:**
- Create: `src\Terraria.Dome.Server\Diagnostics\ServerReplayRecord.cs`
- Create: `src\Terraria.Dome.Server\Diagnostics\ServerReplayRunner.cs`
- Create: `Test\Terraria.Dome.Replay.Verification\Terraria.Dome.Replay.Verification.csproj`
- Create: `Test\Terraria.Dome.Load.Verification\Terraria.Dome.Load.Verification.csproj`
- Modify: V1456 catalog, all replication assemblers, completion manifest, capability matrix,
  `progress.md`

**Steps:**
1. Audit every V1456 message marked `Handled`, `Framed` or `CompatibilityPassThrough`. Reclassify
   any path lacking a backed authority model as unsupported; do not preserve optimistic labels.
2. Record sequence-numbered server intents and compare replayed immutable snapshots/hashes for
   a deterministic fixed tick scenario.
3. Add malformed-frame, rapid reconnect, invalid-slot, packet flood, queue-budget and
   slow-reader isolation verifiers. A failing session must not block the simulation or unrelated
   sessions.
4. Add load scenarios for many visible sections/entities and verify that outbound work is
   bounded per tick without duplicate revision emission.
5. Run the completion verifier. For every requested point, inspect source, reference link,
   projection and execution evidence. If score is below 90 or a safety-critical family is
   incomplete, return to the owning batch rather than declaring broad completion.
6. Publish final evidence in `progress.md`: exact Git revision, commands, exit codes, warnings,
   score report, unsupported features and known compatibility differences.

**Acceptance:** The 90 percent claim is backed by machine-readable coverage, executable
replay/loopback evidence and an explicit unsupported-feature inventory.

## 7. Required Tests by Boundary

| Boundary | Minimum negative cases | Minimum positive cases |
|---|---|---|
| Frame parser | truncated length, oversized length, illegal state, server-only inbound message | exact known V1456 frame decode/encode |
| Session | spoofed player slot, duplicate profile, inactive gameplay command, disconnect mid-write | one session maps to one server player and cleans up |
| World mutation | out of world, unloaded section, over range, unsupported action, quota exceeded | valid tile action commits on one deterministic tick |
| Replication | not visible, unchanged revision, slow reader, disconnected session | first visible state then exactly one revision delta |
| Player/items | invalid slot, duplicate pickup, client health overwrite | server-owned movement/use/pickup/lifecycle |
| Combat | invalid target/owner, duplicate damage, expired projectile | deterministic hit/damage/death/drop outcome |
| Containers | not visible, range violation, concurrent conflicting transfer | valid open/transfer/resync |
| Persistence | bad version, truncation, interrupted replacement | save/restart/reload equivalence |
| Mechanics | unbounded liquid work, invalid wire/door target | bounded deterministic mechanism result |

## 8. File and Dependency Guardrails

- New Simulation code may reference only Simulation and permitted NuGet dependencies. It may not
  reference a protocol packet DTO, `TcpClient`, `NetworkStream`, `File`, Server or Client.
- Protocol may reference immutable Simulation value/snapshot types only when a codec needs them.
  It cannot reference `DomeServer` or mutable `WorldGrid` APIs.
- Server may reference Simulation and Protocol. It owns queues, IO and session-specific cursors.
- Test projects may reference only the layers they need to observe. A protocol test should not
  start a socket unless it validates a protocol-host boundary.
- Preserve one core type per C# file, alphabetized `using` directives, 2-space indentation,
  line width of 100 and braces for all control flow.
- Do not add broad `Build` directories below `src` or `Test`. If stale project-local output
  appears, first enumerate exact paths, confirm they are generated for the current task and
  remove only those paths.

## 9. Commit and Review Cadence

Use small commits after a batch's focused verifier is green. Suggested messages:

```text
test: add tile interaction authority verification
feat: commit validated tile changes during simulation tick
feat: replicate dirty world sections per session
feat: add authoritative player tile collision
feat: add server-owned item and pickup lifecycle
feat: replicate combat entities by section visibility
feat: persist versioned dome worlds
test: audit replay and server completion evidence
```

Before each commit run `git diff --check`, the focused verifier and an appropriate root Release
build. Before a milestone claim run every verifier project registered in the solution, then read
the completion matrix row by row. Never fold user-owned Client/TestControl work into a feature
commit without deliberately reviewing it.

## 10. Current Starting Point and Next Executable Task

The starting evidence is the 2026-08-13 Release verification recorded in `progress.md`:
`WorldGrid`, immutable section snapshots, V1456 compressed TileSection encoding, session section
revision cursors, initial `5 x 3` section streaming, DomeClient world entry and basic ECS
movement/NPC/projectile loops pass independently.

The immediate next task is **Batch 0, Task 1** followed by **Batch 1**. Batch 1 must begin with
message-17 protocol RED tests, not with direct `WorldGrid` writes from the socket host. That
sequence establishes the authority seam required by all later world mechanics and prevents
protocol coverage from being mistaken for server gameplay coverage.

## 11. Explicit Non-Claims Until Proven

Until the relevant batch passes its acceptance and evidence gate, the project must not claim:

- full Terraria protocol compatibility;
- full tile/action compatibility;
- authoritative multiplayer inventory, NPC, projectile, container, liquid or event behavior;
- world persistence or `.wld` compatibility;
- deterministic world generation parity;
- 90 percent core server completion.

The plan is executable work authorization and an audit framework. It is not evidence that any
future batch has already been implemented.
