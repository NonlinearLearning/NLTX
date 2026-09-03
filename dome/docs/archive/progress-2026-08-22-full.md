# Project Context

Last updated: 2026-08-13

`Terraria.Dome.sln` is the build entry point for a minimal, authoritative Arch ECS
Dome. It targets `net10.0` and consists of:

- `Terraria.Dome.Simulation`: Arch World, gameplay components, systems, commands
  and immutable snapshots.
- `Terraria.Dome.Transport`: retained private line-delimited JSON DTOs; it is no longer the
  DomeClient network contract.
- `Terraria.Dome.Server`: loopback TCP host and session-to-server-owned Player
  mapping.
- Real Terraria client validation: the source-built `Terraria.exe` connects through the capture
  proxy; current reproducible evidence is retained under `Build/diagnostics`.
- `Terraria.Dome.Verification`: executable behavior and loopback verification.

The implementation deliberately does not introduce a replacement `Entity.cs`,
`EntityState`, legacy `Main` arrays, `whoAmI`, or untyped `ai[]` into the new
simulation. The current V1456 implementation covers the verified world-entry and player-control
path; its message catalog does not claim every Terraria gameplay message is semantically handled.

Verified from repository root on 2026-08-13:

```text
dotnet restore Terraria.Dome.sln -p:UseSharedCompilation=false
dotnet build Terraria.Dome.sln --no-restore -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.Verification\Terraria.Dome.Verification.csproj -p:UseSharedCompilation=false
```

The build completed with `0` warnings and `0` errors. The verifier proved Player
movement, jump/ground collision, Npc target/chase, Projectile damage/despawn and a
real loopback Client/Server interaction. All generated artifacts are under
`Build/bin`, `Build/obj`, `Build/generated`, or `Build/packages`.

# 2026-08-13 Authoritative WorldGrid and section replication

The Dome server now owns a bounded `4200 x 1200` `WorldGrid` using Terraria's
`200 x 150` section units. The grid exposes immutable section snapshots and
per-section versions. V1456 message 10 `TileSection` frames are now encoded from
those snapshots, including tile active/type values and legacy-compatible RLE, not
only from the prior fixed empty-section payload.

`DomeServer` owns the world and creates a per-session section version cursor. On
SpawnTileData it selects the requested spawn's `5 x 3` section neighborhood,
streams only snapshots that session has not received at the current version, and
preserves the V1456 bootstrap order `StatusTextSize -> TileSection* -> InitialSpawn`.

Verified from repository root in `Release` on 2026-08-13:

```text
dotnet build Terraria.Dome.sln -c Release -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.Verification\Terraria.Dome.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.World.Verification\Terraria.Dome.World.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.World.Protocol.Verification\Terraria.Dome.World.Protocol.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.World.Server.Verification\Terraria.Dome.World.Server.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.World.Loopback.Verification\Terraria.Dome.World.Loopback.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
```

The build completed with `0` warnings and `0` errors. The independent verifiers
proved bounded tile mutation, immutable snapshots, section-local versions, V1456
tile decoding, version-aware session visibility, spawn-neighborhood selection, and
real loopback world entry using a Server-owned tile instead of a fixed empty
section.

This is the first world/replication slice, not a full world server. It does not
yet decode V1456 TileManipulation into server-validated commands; persist worlds;
or synchronize section-local NPCs, world items, chests, signs, liquids, or tile
entities. Those remain explicit next domains rather than implied protocol support.
# 2026-08-13 DomeClient V1456 session (removed)

- This historical entry describes the former synthetic V1456 console client. It was removed in
  favor of real-client connection validation; current executable evidence is recorded separately
  under `Build/diagnostics`.

# 2026-08-14 Core-server completion evidence baseline

Batch 0 now has a machine-readable manifest at
`docs/server-completion/completion-manifest.json`, a weighted matrix at
`docs/server-completion/capability-matrix.md`, and an executable verifier in
`Test/Terraria.Dome.Completion.Verification`.

The verifier requires all 11 families to total exactly 100 points and rejects an `evidenced`
family unless reference, authority, projection and executable verifier fields are all present.
The initial Batch 0 manifest intentionally reported `0` evidenced points; partial and missing
families did not count toward the 90-percent gate. The current scored state is recorded below.

# 2026-08-14 Batches 1 and 2: authoritative world interaction and moving PVS

The V1456 `TileManipulation` message (`17`) now has a typed intent that decodes only `KillTile`
and `PlaceTile`; actions `2..23` and malformed envelopes are rejected. Protocol parsing remains
mutation-free. The Server attaches a monotonic sequence, validates the server-owned player,
range, visible section and per-tick action budget, then enqueues a value-only `TileChangeCommand`.
`WorldGrid` commits ordered changes at the tick boundary and increments only the affected section
revision.

Each active session now owns a replication cursor and serial write gate. PVS is recomputed from
the server player snapshot every tick, with newly entered sections streamed once, departed cursors
removed, and unchanged revisions omitted. Session disconnect enqueues server teardown that removes
the player and disposes its replication state during the simulation tick.

The executable completion manifest now scores family B at 12/100. Family A remains partial because
the current server does not yet have full queue/load hardening and family C remains partial until
player lifecycle receives an approved V1456 projection and loopback evidence.

# 2026-08-14 Batch 7 foundation: world snapshots, recovery and deterministic base generation

`Terraria.Dome.Simulation` now exports an immutable `WorldGridSnapshot` containing explicit
`WorldMetadata`, `WorldSeed`, copied tiles and section versions. It can construct an independent
`WorldGrid` candidate from that value-only snapshot; neither type introduces Server, Protocol,
socket or file-system dependencies into Simulation.

`Terraria.Dome.Server.Persistence` owns a versioned binary format with a magic value, strict
dimensions/name/version checks, explicit tile and section-version payloads, and trailing-byte
rejection. `WorldSaveCoordinator` writes to a same-directory temporary file, flushes it, then
atomically moves or replaces the destination. `TryLoad` parses a complete independent candidate;
a failed parse returns no snapshot and therefore cannot mutate a live server world. A restarted
`DomeServer(WorldGrid)` may intentionally adopt a recovered candidate during construction.

The Simulation-only `WorldGenerationPipeline` currently has two deliberately small named passes:
`SurfaceAndGround` and `SpawnClearing`. It is deterministic for equivalent metadata and seed, and
is explicitly not a byte-compatible Terraria `WorldGen` replacement.

Verified from the repository root in `Release` on 2026-08-14:

```text
dotnet run --project Test\Terraria.Dome.Persistence.Verification\Terraria.Dome.Persistence.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet build Terraria.Dome.sln -c Release -p:UseSharedCompilation=false -m:1
```

The persistence verifier proves metadata/tile/section-version round-trip, candidate-based restart
adoption, truncated/version/dimension rejection, replacement of an existing file and directory
target failure without target loss. It also proves equal seed requests hash identically, different
seeds produce a distinct base-world hash, generated ground exists and the configured spawn area is
cleared. The build completed with `0` warnings and `0` errors.

Family H remains `missing` and contributes `0` points: world objects, inventory/world-item/NPC
state, save/restart two-session reload evidence, and a documented recovery policy remain absent.

# 2026-08-14 Protocol authority audit: reject unbacked bootstrap assertions

The V1456 catalog no longer labels `SyncEquipment`, `PlayerLifeMana`,
`ItemRotationAndAnimation`, `PlayerBuffs`, `PlayerUuid` or `SyncLoadout` as compatibility
pass-through. They had no Dome Simulation state, Server validator, command or projection: the
previous session implementation only retained their raw payloads. The dispatcher now rejects each
as unsupported, and the removed synthetic client did not send them during connection. This deliberately narrows
the accepted V1456 bootstrap sequence to the state that Dome actually models.

The base verification executable now injects every removed message after `SyncPlayer` and proves
that it is rejected, while its V1456 client/server loopback remains active with the reduced
bootstrap sequence. This is protocol-hardening evidence only; family K remains partial because
replay, slow-reader and full malformed/rapid-reconnect coverage are still absent.

# 2026-08-14 Batch 5: authoritative NPC, projectile, combat and PVS replication

The Dome now supports one intentionally small, documented combat family: a server-owned
`DomeChaserNpc` and player-owned `DomeBoltProjectile`. Simulation assigns stable replication IDs,
server-owned revisions, types, transforms, velocities, active state and sections. The session host
creates the first NPC for an empty world only; later session activation no longer creates NPCs,
preserving world ownership instead of coupling entities to clients.

Player fire remains an input assertion. The Simulation tick applies its server-owned cooldown,
creates the projectile, and resolves swept projectile paths against tiles and NPCs. Swept collision
prevents a 4-unit-per-tick bolt from tunneling through a one-tile NPC. Hits commit authoritative
damage and an inactive projectile tombstone. NPC death commits an inactive revision and one
deterministic seeded world-item drop through `LootTable`.

V1456 messages 23, 27 and 29 are now `ServerToClient` / `Handled` for this backed state subset.
Typed encoders project only server-held identity, position, velocity, type, health/damage and
owner fields. `CombatReplicationAssembler` uses the existing session PVS plus per-ID revision
cursor: hidden active entities are not emitted, unchanged snapshots are suppressed, and a client
that previously saw an entity receives its final projectile kill or NPC inactive revision.

Verified in `Release` on 2026-08-14:

```text
dotnet run --project Test\Terraria.Dome.Combat.Verification\Terraria.Dome.Combat.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.Combat.Protocol.Verification\Terraria.Dome.Combat.Protocol.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.Combat.Loopback.Verification\Terraria.Dome.Combat.Loopback.Verification.csproj -c Release -p:UseSharedCompilation=false
```

The first verifier covers stable records, revisions, cooldown, swept NPC/tile collision and
deterministic loot. The second covers typed V1456 23/27/29 frames and PVS cursor semantics. The
third runs attacker, eligible observer and hidden sessions: the observer receives NPC/projectile
spawn, hit/despawn and NPC death; the hidden session does not receive the target; exactly one
server-owned item drop exists. Completion families E and F are now evidenced for 20 points. The
overall completion score is 32/100; broad NPC/projectile parity is still not claimed.

# 2026-08-14 Batch 4: authoritative inventory, item use, pickup and SyncItem projection

The item path now has an explicit host validation boundary. V1456 `PlayerControls` remains the
only accepted client assertion for item use: the server validates the session-owned active player
and selected inventory slot, then Simulation consumes the selected type-1 health item, restores
authoritative health, decrements the stack once, and applies a named use cooldown. Client position,
stack, item type and health are never accepted as authority.

World drops use a deterministic server-side proximity pickup pass ordered by world-item replication
ID and `PlayerHandle`; the first eligible player receives the stack and the item revision becomes an
inactive tombstone. V1456 message 21 `SyncItem` is `ServerToClient` only and projects the immutable
world-item snapshot through the existing session PVS/revision cursor. A client-sent message 21 is
rejected by the dispatcher before it can enqueue a Simulation command.

The retained reference is `Version4` `MessageBuffer.cs:case 5,case 21` and `NetMessage.cs:case 5,case
21`. The item unit verifier proves stack limits, selected-slot validation, deterministic pickup race,
use effect, stack decrement and cooldown. The two-session loopback verifier proves an eligible
observer receives the item, a single winner acquires it, the world item becomes inactive, and a
forged client `SyncItem` does not create state.

Verified from the repository root in `Release` on 2026-08-14:

```text
dotnet build Terraria.Dome.sln -c Release -p:UseSharedCompilation=false -m:1
dotnet run --project Test\\Terraria.Dome.Items.Verification\\Terraria.Dome.Items.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
dotnet run --project Test\\Terraria.Dome.Items.Loopback.Verification\\Terraria.Dome.Items.Loopback.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
dotnet run --project Test\\Terraria.Dome.Completion.Verification\\Terraria.Dome.Completion.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
```

All commands exited `0`; the Release build reported `0` warnings and `0` errors. The structured
completion verifier now reports 44/100 points. Family D is evidenced for this deliberately scoped
item family; broad Terraria inventory, mana, equipment, chest and every item-use parity remain
outside the claim.

# 2026-08-14 Batch 3 continuation: player health projection boundary

The player replication path now also emits typed V1456 message 16 `PlayerLifeMana` frames from
the server-owned `HealthComponent` (`statLife` and `statLifeMax`), alongside the existing
`PlayerActive` and position projection. The retained reference is `Version4` `NetMessage.cs:case
16`; the protocol encoder has explicit signed-16-bit range checks and no client health value is
accepted as authority.

`Terraria.Dome.PlayerAuthority.Verification` now proves the exact message-16 payload in addition to
Simulation tile collision, death and respawn behavior. The existing session verifier still proves
authoritative PVS movement and disconnect teardown. Family C remains partial until a real network
death/respawn loopback is verified.

# 2026-08-14 Batch 3: authoritative player death, contact damage and automatic respawn

The player lifecycle now has a complete scoped network execution path. The server-owned
`DomeChaserNpc` applies deterministic contact damage when its collider overlaps an active player.
Death commits `Health=0`, `PlayerLifecycleComponent.IsActive=false` and a named respawn delay;
after the delay, Simulation automatically respawns at the server-created spawn point and restores
maximum health. Client `PlayerControls` position fields remain assertions and cannot overwrite the
authoritative transform or spawn point.

The V1456 projection emits `PlayerActive` and typed `PlayerLifeMana` message 16 frames from the
immutable server player replication state. The retained references are Version4 `Player.cs`
movement/lifecycle paths and `MessageBuffer.cs:cases 13,14,16`. A real three-session loopback now
proves an observer receives the target player's death and automatic respawn, a hidden session does
not receive those lifecycle frames, and a forged position assertion does not mutate server state.

Verified from the repository root in `Release` on 2026-08-14:

```text
dotnet build Test\\Terraria.Dome.PlayerLifecycle.Loopback.Verification\\Terraria.Dome.PlayerLifecycle.Loopback.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test\\Terraria.Dome.PlayerAuthority.Verification\\Terraria.Dome.PlayerAuthority.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
dotnet run --project Test\\Terraria.Dome.PlayerLifecycle.Loopback.Verification\\Terraria.Dome.PlayerLifecycle.Loopback.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
```

All commands exited `0`; the focused build reported `0` warnings and `0` errors. Family C is now
evidenced for the deliberately scoped player lifecycle and contributes 10 points. Full Terraria
player parity, client-requested respawn, mana, equipment and death-reason compatibility remain
outside this scoped claim.

# 2026-08-14 Batch 7 continuation: entity-aware save, recovery and restart loopback

Persistence now covers a value-only `DomeSimulationSnapshot` in addition to the existing
`WorldGridSnapshot`. The versioned binary state stores world metadata/tiles/section revisions,
simulation tick, stable NPC replication records and world-item replication records. Simulation
rebuilds a fresh Arch world from that snapshot; no Arch `Entity`, session slot, player handle,
socket or protocol object is persisted. `DomeServer` can adopt the recovered snapshot, while
connected players are intentionally recreated by new sessions.

`DomeStateSaveCoordinator` uses the same-directory temporary file, flush-to-disk and atomic
replacement policy as the base world save path. The persistence verifier covers entity/item
round-trip and trailing-byte rejection. The save/restart/reload loopback starts a new server from
the recovered state and proves a newly connected player receives restored NPC and item projections.

Verified in `Release` on 2026-08-14:

```text
dotnet build Test\\Terraria.Dome.Persistence.Loopback.Verification\\Terraria.Dome.Persistence.Loopback.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test\\Terraria.Dome.Persistence.Verification\\Terraria.Dome.Persistence.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
dotnet run --project Test\\Terraria.Dome.Persistence.Loopback.Verification\\Terraria.Dome.Persistence.Loopback.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
```

All commands exited `0`; the focused build reported `0` warnings and `0` errors. Family H is
evidenced for the scoped world/entity persistence model and contributes 10 points. Binary
compatibility with Terraria `.wld`, inventory/player-session persistence and every legacy world
object remain outside this claim.

# 2026-08-14 Batch 8: session ordering and authoritative lifecycle evidence

Family A is now evidenced for the scoped server core. A real TCP session sends two V1456 tile
mutation frames and the verifier proves that both commands commit in monotonic server sequence
order. Existing load-flood, bounded-queue, malformed-session isolation, disconnect teardown and
replacement-session verifiers remain green, so one noisy or malformed session cannot halt the
simulation or affect another session.

The lifecycle slice from Batch 3 is counted in Family C, and the entity-aware save/restart slice
from Batch 7 is counted in Family H. The structured completion verifier reports `72/100` points
after the A manifest update.

Verified from the repository root in `Release` on 2026-08-14:

```text
dotnet run --project Test\\Terraria.Dome.Completion.Verification\\Terraria.Dome.Completion.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
dotnet build Test\\Terraria.Dome.TileInteraction.Verification\\Terraria.Dome.TileInteraction.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test\\Terraria.Dome.TileInteraction.Verification\\Terraria.Dome.TileInteraction.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
```

The completion verifier passed with `100` manifest points and `72` evidenced points. The focused
tile interaction build and verifier both exited `0` with no reported errors.

# 2026-08-14 Batch 9: bounded deterministic world-rule projection

The scoped J family now has one complete deterministic rule: Simulation advances a day/night
clock at each tick boundary and exports an immutable `WorldRuleSnapshot`. V1456 message 18
`SetTime` is server-to-client only; a client-provided frame is rejected before it can reach a
world mutation path. `SessionReplicationState` sends the current rule snapshot once on session
activation, then uses a 600-tick cursor cadence to avoid high-frequency fan-out competing with
real-time replication.

The adjacent combat loopback now waits for server-confirmed NPC death, with an explicit 360-input
upper bound, instead of assuming a fixed number of network frames equals a fixed number of
simulation ticks. This preserves the same authoritative combat requirement while accounting for
independent outbound state families.

Verified from the repository root in `Release` on 2026-08-14:

```text
dotnet build Terraria.Dome.sln -c Release -p:UseSharedCompilation=false -m:1
dotnet run --project Test\\Terraria.Dome.WorldRules.Verification\\Terraria.Dome.WorldRules.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
dotnet run --project Test\\Terraria.Dome.WorldRules.Loopback.Verification\\Terraria.Dome.WorldRules.Loopback.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
dotnet run --project Test\\Terraria.Dome.Combat.Loopback.Verification\\Terraria.Dome.Combat.Loopback.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
```

The Release build and all listed verifiers exited `0`, with the build reporting `0` warnings and
`0` errors. Family J is evidenced for the scoped deterministic time/rule projection and adds
6 points. Broader invasions, bosses, spawn tables and Terraria event parity remain outside this
claim.

# 2026-08-14 Batch 6 partial: section-local chest open ownership

Family G now has a partial but executable chest path. Stable section-local chest IDs and item
slots live in Simulation; exclusive open ownership uses `PlayerHandle`, not a server session
slot. The Server validates player range, asserted coordinate and visible section, then maps only
the winning opener to V1456 `RequestChestOpen` message 31 and `SyncChestItem` message 32. A
two-session loopback proves exactly one opener receives all 40 slot projections and an unseen
session receives none.

This remains partial and contributes `0`: chest item transfer and sign mutation are now backed by
unit and two-session loopback proof, but tile-entity persistence and reconnect resynchronization
still need their own authority and loopback evidence.

# 2026-08-14 Batch 10: bounded door mechanics and expanded world objects

Family I now has a complete scoped door mechanic. Simulation owns stable door IDs, tile location,
open state and monotonic revisions. Server validates the asserted coordinate, active session,
authoritative player range and visible section before toggling. V1456 message 19 is typed for
client requests and server state projection; liquid, wire and actuator actions remain explicitly
unsupported rather than being accepted as no-ops.

Family G remains partial but its backed surface is broader: chest item transfer now moves a whole
server-owned stack only for the current `PlayerHandle` opener, sign text has a bounded authoritative
revision path, and the two-session loopback proves chest contention, transfer exclusivity, sign
mutation, PVS filtering and server-only projections. Reconnect resynchronization is not counted;
the current session teardown path still needs a fresh explicit EOF/close proof before G can be
evidenced.

Verified from the repository root in `Release` on 2026-08-14:

```text
dotnet build Terraria.Dome.sln -c Release -p:UseSharedCompilation=false -m:1
dotnet run --project Test\\Terraria.Dome.WorldMechanics.Verification\\Terraria.Dome.WorldMechanics.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
dotnet run --project Test\\Terraria.Dome.WorldMechanics.Loopback.Verification\\Terraria.Dome.WorldMechanics.Loopback.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
dotnet run --project Test\\Terraria.Dome.WorldObjects.Verification\\Terraria.Dome.WorldObjects.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
dotnet run --project Test\\Terraria.Dome.WorldObjects.Loopback.Verification\\Terraria.Dome.WorldObjects.Loopback.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
```

The focused door and world-object verifiers exited `0`; the root Release build reported `0`
warnings and `0` errors. The structured completion score is now `86/100`. The remaining gap to
the 90-percent gate is deliberate: G reconnect teardown/resynchronization and K full hardening
evidence are still missing.

# 2026-08-14 Batch 10: V1456 authority and connection hardening

Family K is now evidenced for the retained dedicated-server protocol scope. The dispatcher only
routes backed typed intents and rejects unbacked frames before simulation mutation. `DomeServer`
keeps its incoming command queue bounded. `SessionReplicationState` now owns a bounded
per-session asynchronous replication queue and a write worker with a per-frame deadline, so a
slow peer exhausts only its own budget; the server removes that failed session while healthy peers
and simulation ticks continue.

The dedicated hardening loopback verifier proves malformed-frame connection isolation, bounded
packet flood with continuing simulation progress, PVS-scoped slow-reader isolation, and four
rapid disconnect/reconnect handshakes followed by a complete fifth session. The protocol verifier
also checks catalog coverage, backed route dispatch and unsupported bootstrap rejection.

Verified from the repository root in `Release` on 2026-08-14:

```text
dotnet run --project Test\\Terraria.Dome.Hardening.Loopback.Verification\\Terraria.Dome.Hardening.Loopback.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
dotnet run --project Test\\Terraria.Dome.Verification\\Terraria.Dome.Verification.csproj -c Release --no-build -p:UseSharedCompilation=false
```

Both commands exited `0`. Family K adds `6` evidenced points, raising the structured score to
`92/100`; family G remains partial and contributes `0` because full tile-entity persistence and
explicit reconnect/resynchronization remain out of evidence scope.

# 2026-08-15 Full-client KillProjectile disconnect repair

The real full-client frame trace in `Build/diagnostics/terraria-7777-frame-trace.log` completed
the full world-entry sequence, including `WorldData`, `StatusTextSize`, 15 initial `TileSection`
frames, `InitialSpawn`, player activation and active synchronization. It then sent
`C2S length=6 id=29 payload=010005`; the server-side stream ended immediately afterward. This
matches a missing direction and dispatch route, not a malformed tile section.

`D:\TRbackup\Version4物理删除了某些文件\Terraria\MessageBuffer.cs` case 29 establishes the
reference wire layout: `Int16 projectile identity` plus `Byte owner`. The legacy server replaces
the claimed owner with `whoAmI`, finds only an active projectile with that owner and identity,
then broadcasts the authoritative result. Dome previously cataloged message 29 as
server-to-client only and closed the session before that ownership boundary could be evaluated.

The repair adds `ClientProjectileTermination`, requires an exact three-byte message-29 payload,
requires an active session and validates that its owner equals the server-assigned player slot.
The current Dome simulation has no authority mapping for client-created `SyncProjectile` records,
so a valid client termination is accepted as a compatibility notification only. It cannot delete
or broadcast a server-owned projectile. A forged owner or trailing payload remains a protocol
failure and closes only the offending session.

The default Release output was occupied by the live trace server, so verification used the
isolated project configuration `KillProjectile`; this preserves the repository-managed
`Build/bin/<project>/<configuration>/` layout. The root solution has no such configuration, so
the verification built the touched server project rather than modifying solution metadata.

Verified from the repository root on 2026-08-15:

```text
dotnet build src\Terraria.Dome.Server\Terraria.Dome.Server.csproj -c KillProjectile -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.Combat.Protocol.Verification\Terraria.Dome.Combat.Protocol.Verification.csproj -c KillProjectile -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.FullClientBootstrap.Verification\Terraria.Dome.FullClientBootstrap.Verification.csproj -c KillProjectile --no-build -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.Hardening.Loopback.Verification\Terraria.Dome.Hardening.Loopback.Verification.csproj -c KillProjectile -p:UseSharedCompilation=false
```

All four commands exited `0`. The server build reported `0` warnings and `0` errors. The full
client bootstrap loopback sends the captured message-29 shape after world entry and proves a
subsequent Ping response. The hardening loopback remained green for malformed-frame isolation,
flood bounds, slow-reader isolation and reconnect availability.

Deployment was also updated: the old Release server process listening on `127.0.0.1:7778` was
replaced with `Build\bin\Terraria.Dome.Server\KillProjectile\net10.0\Terraria.Dome.Server.exe`
on the same endpoint. Its startup record is
`Build/diagnostics/kill-projectile-v1-server.log`; the pre-existing `127.0.0.1:7777` frame proxy
was left running. A fresh GUI-client reconnect was not initiated automatically, but it will now
follow the existing proxy path to the repaired server binary.

# 2026-08-15 Full-client active SyncEquipment disconnect repair

After the message-29 compatibility repair, the same original full-client trace reached a new
deterministic boundary. The active session accepted `C2S length=6 id=29 payload=010004`, then the
client emitted consecutive 9-byte `SyncEquipment` frames: `C2S length=12 id=5` for slots `0`
through `5`. The server immediately wrote `S2C EOF`. The following client frames for slots `6`,
`7`, `9`, `10`, `13`, `18`, and `29` remained visible in the proxy trace after that EOF.

The dispatcher had routed every message-5 frame through its bootstrap route. That route calls
`TerrariaSession.AcceptPlayerEquipment`, which correctly permits only
`PlayerProfileReceived`; a world-entered session is `Active`, so a valid active update became a
session-ending `InvalidDataException`.

`D:\TRbackup\Version4物理删除了某些文件\Terraria\MessageBuffer.cs` case `5` is the reference:
it reads player slot, item slot, stack, prefix, type, and flags; overrides the claimed player slot
with `whoAmI`; then conditionally relays selected slots. Dome now has a separate active
`SyncEquipment` path. It requires `Active`, decodes the exact existing 9-byte layout, validates the
claimed player slot against the assigned slot, and returns
`ActiveSynchronizationAccepted`. It intentionally does not write the frozen bootstrap equipment,
mutate persisted inventory, enqueue a simulation command, or relay client-supplied inventory data:
Dome still lacks an authoritative active-inventory mutation model.

The protocol tests prove owned active equipment leaves the session active, forged player slots are
rejected, trailing bytes remain invalid, and an active all-zero equipment update does not rewrite
the original frozen bootstrap equipment. The full-client verifier now sends the observed
`KillProjectile -> Ping -> SyncEquipment slots 0..6 -> Ping` sequence. It also has an explicit
`--external-port <1-65535>` mode, retaining its default in-process server mode while allowing the
same verifier to validate the deployed server through the existing proxy.

Verified from the repository root on 2026-08-15:

```text
dotnet build src\Terraria.Dome.Server\Terraria.Dome.Server.csproj -c KillProjectile -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.Combat.Protocol.Verification\Terraria.Dome.Combat.Protocol.Verification.csproj -c KillProjectile -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.FullClientBootstrap.Verification\Terraria.Dome.FullClientBootstrap.Verification.csproj -c KillProjectile -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.Verification\Terraria.Dome.Verification.csproj -c KillProjectile -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.Hardening.Loopback.Verification\Terraria.Dome.Hardening.Loopback.Verification.csproj -c KillProjectile -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.FullClientBootstrap.Verification\Terraria.Dome.FullClientBootstrap.Verification.csproj -c KillProjectile --no-build -p:UseSharedCompilation=false -- --external-port 7777
```

All commands exited `0`; the server build reported `0` warnings and `0` errors. The deployed
server is PID `13328`, listening on `127.0.0.1:7778` from
`Build\bin\Terraria.Dome.Server\KillProjectile\net10.0\Terraria.Dome.Server.exe`. Its startup
output is `Build\diagnostics\active-equipment-v1-server.stdout.log`; stderr is empty. The existing
proxy remains PID `21408` on `127.0.0.1:7777`.

The deployed external-port verification wrote a new exact-frame trace at
`Build\diagnostics\terraria-7777-frame-trace.log`. It contains
`1786768691.822221 C2S id=29`, then the consecutive owned `C2S id=5` slot frames at
`1786768691.823869` through `1786768691.824019`, followed by a second
`1786768691.824069 C2S id=154` and `1786768691.824159 S2C id=154`. No `S2C EOF` occurs at that
boundary. The existing GUI Terraria process did not automatically reconnect after the old server
was stopped, so this is deployment/proxy evidence using the full-client verifier's exact sequence,
not a claim of a new GUI reconnection. The running server is ready to capture the next GUI boundary
when it reconnects.

# 2026-08-15 Full-client active PlayerLifeMana disconnect repair

The next GUI-client trace was captured after the active `SyncEquipment` repair. It passed the
earlier message-29 and message-5 boundaries, continued through active player controls, zone,
buff, projectile, and Ping traffic, then ended immediately after:

```text
1786769908.588731 C2S length=8 id=16 payload=04FC00F401
1786769908.589014 S2C EOF
```

Message `16` is `PlayerLifeMana`. The payload is the existing exact five-byte layout:
`Byte player slot`, `Int16 current life`, and `Int16 maximum life`. The GUI trace used assigned
slot `4`, current life `252`, and maximum life `500`. This isolated a second bootstrap-only
dispatcher route: active message `16` entered `RouteBootstrap`, where
`TerrariaSession.AcceptPlayerLifeMana` correctly rejected any state other than
`PlayerProfileReceived`.

`D:\TRbackup\Version4物理删除了某些文件\Terraria\MessageBuffer.cs` case `16` reads that layout,
replaces the claimed slot with `whoAmI`, applies life/max-life and broadcasts the authoritative
player state. Its adjacent case `42` follows the same rule for mana. Dome now provides separate
active-session acceptance methods for both frames. They require `Active`, decode the pre-existing
exact layouts, and validate that the packet slot equals the server-assigned slot. They return
`ActiveSynchronizationAccepted` without mutating frozen bootstrap life/mana, persisted player
state, or simulation state, and without relaying client-supplied vital values. This preserves
Dome's server-authoritative simulation boundary while maintaining client protocol compatibility.

The regression coverage adds owned active life and mana acceptance, forged active life-slot
rejection, and a frozen-bootstrap invariant: active vital values deliberately differ from the
initial values and cannot overwrite the original `PlayerBootstrapState`. The full-client verifier
now verifies `KillProjectile -> Ping -> SyncEquipment slots 0..6 -> Ping -> PlayerLifeMana ->
PlayerMana -> Ping` in both its default in-process mode and the existing proxy's external-port
mode.

Verified from the repository root on 2026-08-15:

```text
dotnet build src\Terraria.Dome.Server\Terraria.Dome.Server.csproj -c KillProjectile -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.Combat.Protocol.Verification\Terraria.Dome.Combat.Protocol.Verification.csproj -c KillProjectile -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.FullClientBootstrap.Verification\Terraria.Dome.FullClientBootstrap.Verification.csproj -c KillProjectile -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.Verification\Terraria.Dome.Verification.csproj -c KillProjectile -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.Hardening.Loopback.Verification\Terraria.Dome.Hardening.Loopback.Verification.csproj -c KillProjectile -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.FullClientBootstrap.Verification\Terraria.Dome.FullClientBootstrap.Verification.csproj -c KillProjectile --no-build -p:UseSharedCompilation=false -- --external-port 7777
```

All commands exited `0`; the server build reported `0` warnings and `0` errors. The current
deployed server is PID `3164`, listening on `127.0.0.1:7778` from
`Build\bin\Terraria.Dome.Server\KillProjectile\net10.0\Terraria.Dome.Server.exe`. Its startup
log is `Build\diagnostics\active-vitals-v1-server.stdout.log`; stderr is empty. The proxy remains
PID `21408` on `127.0.0.1:7777`.

The deployed external-port trace crosses the repaired boundary without EOF:

```text
1786770248.152651 C2S length=12 id=5 payload=010000000000000000
1786770248.152825 C2S length=12 id=5 payload=010600000000000000
1786770248.153471 C2S length=8 id=16 payload=0101001400
1786770248.153524 C2S length=8 id=42 payload=0101001400
1786770248.153534 C2S length=3 id=154 payload=
1786770248.153834 S2C length=3 id=154 payload=
```

The GUI client has not automatically reconnected after replacing the prior server process, so the
new trace is exact deployed/proxy evidence from the full-client verifier rather than a claim that
the GUI has already crossed active life/mana on this build. The service remains running and the
same proxy will capture the next GUI boundary when a reconnection occurs.

# 2026-08-15 Legacy connection equipment projection and serialized outbound recovery

The full-client connection projection now matches Version4 `SyncConnectedPlayer`:
`SyncEquipment` frames cover only `0..98` and `900..989`, for `189` frames. Dome still persists
all `990` player-account slots; bank, trash and other non-connection slots are deliberately not
sent during initial connection. The full-client verifier now sends active zone, buff, control,
projectile and Ping traffic immediately after `PlayerSpawn`, while authority projection is in
flight. It verifies the exact projection set, a contiguous completion stream, and the queued Ping
acknowledgement.

That replay found that dispatcher responses and WorldData bypassed the session replication FIFO.
They can no longer interleave with initial authority frames: post-handshake outbound frames now
use the same `SessionReplicationState` writer. Focused player-authority, full-client, combat
protocol and hardening loopback verifiers all exited `0`; `dotnet build Terraria.Dome.sln
-p:UseSharedCompilation=false -m:1` also exited `0` with `0` warnings and `0` errors. This is
loopback protocol evidence, not a claim of a fresh GUI client reconnect. The complete evidence
record is `docs/server-completion/2026-08-15-legacy-connection-equipment-projection-recovery.md`.

# 2026-08-16 Complete client/server raw packet transcript

The source-built complete client from `D:\TRbackup\客户端\bin\Debug\net40\Terraria.exe` was
run against the complete 1.4.5.6 server baseline
`D:\TRbackup\无任何删减通过编译\bin\Debug\net40\TerrariaServer.exe` through the new
lossless TCP recorder. The server used an isolated generated world on backend port `7797`;
the recorder listened on `127.0.0.1:7798`. The physically reduced `D:\TRbackup\Version4物理删除了某些文件`
tree was read-only source provenance for `NetMessage.cs` and `MessageBuffer.cs` case indexes,
not treated as an executable server.

The successful `join-stable` result reports `success=true`, `25364ms`, `playerSlot=0`, and
`netMode=1`. The raw trace contains `1557` complete frames: `360` C2S and `1197` S2C. The
largest frame is `11182` bytes, `18` payloads exceed 64 bytes, and an independent parser found
zero length-prefix, frame-length, message-ID, payload, or truncation errors. No `preview` field
is emitted. The trace and deterministic source index are in
`Build/diagnostics/full-client-server-20260816/trace.jsonl` and `summary.json`; the full
evidence record is `docs/server-completion/2026-08-16-full-client-server-packet-trace.md`.

The recorder's six Python tests passed. The existing Dome runtime on `7777/7778` remained
untouched; the temporary `7797/7798` processes were stopped after capture.

# 2026-08-18 Main server ECS migration Task 0

Task 0 of `docs/plans/2026-08-18-main-server-ecs-migration-implementation.md` completed as a
read-only reference inventory plus a server responsibility ledger. No C# source, project file,
solution item or runtime behavior was changed by this task.

The ledger is `docs/migrations/main-server-responsibility-ledger.md`. It classifies the server
subset of `Main.cs` as explicit world state, definitions, systems, commands, snapshots or
projections; lists client-only responsibilities as excluded; and leaves missing or ambiguous
reference behavior as `Unknown`.

Reference facts captured from `D:\TRbackup\Version4物理删除了某些文件`:

- The directory exists and contains `980` C# files, including `70` files under `Terraria`.
- It has no `.git` metadata, so physical deletion cannot be recovered from local Git history.
- `Terraria\Main.cs` is `13,996` lines and `379,274` bytes.
- The captured Main SHA-256 is `844A862B4EF863FF848227C1D682E89DAB799EEAD48070ADEA07565EC933254D`.
- Full dependency sizes, line counts, timestamps and SHA-256 values are in
  `Build/diagnostics/main-migration/task-0/reference-inventory.json`.

Preflight results from the repository root:

```text
git status --short                         exit 0; existing worktree is dirty
git diff --check                           exit 0; Git emitted existing line-ending warnings
dotnet sln Terraria.Dome.sln list          exit 0
dotnet msbuild Simulation.csproj -get...   exit 0; TargetFramework=net10.0
                                             BaseOutputPath=Build/bin/Terraria.Dome.Simulation
                                             BaseIntermediateOutputPath=Build/obj/Terraria.Dome.Simulation
```

The exact preflight outputs are under `Build/diagnostics/main-migration/task-0/`. Because the
worktree already contains unrelated modifications and untracked migration files, they are not
attributed to Task 0.

# 2026-08-18 Main server ECS migration Task 1

Task 1 added a server-only boundary verifier at
`Test/Terraria.Dome.MainBoundary.Verification/Program.cs` and registered it in
`Terraria.Dome.sln`. The verifier checks non-generated Simulation `.cs` files and the
Simulation project references. It rejects legacy `Terraria.Main` state, client-only XNA/UI
types, transport/socket types, and direct Protocol/Server dependencies. It also fails when the
repository, Simulation directory, or Simulation project is missing.

Focused evidence:

```text
dotnet run --project Test\Terraria.Dome.MainBoundary.Verification\Terraria.Dome.MainBoundary.Verification.csproj -c Release -p:UseSharedCompilation=false
exit 0; checked 166 source files; violations 0

dotnet build src\Terraria.Dome.Simulation\Terraria.Dome.Simulation.csproj -c Release -p:UseSharedCompilation=false
exit 0; 0 warnings; 0 errors
```

The command output is retained in `Build/diagnostics/main-migration/task-1/` as
`main-boundary-verifier.txt` and `simulation-release-build.txt`. A root Release build was also
started with `dotnet build Terraria.Dome.sln -c Release -p:UseSharedCompilation=false -m:1`, but
the command timed out after 124 seconds without a compiler error; this remains unverified and is
not reported as a pass.

# 2026-08-18 Main server ECS migration Task 2

Task 2 added explicit `WorldClock` state and a named `WorldClockSystem` phase. The clock owns
tick number, time of day, day/night state, pause policy, configured update rate and cycle lengths.
`DomeSimulationSnapshot` now carries a `WorldClockSnapshot`, and restored simulations continue
from the same clock state. `DomeSimulation.Tick` advances the clock before gameplay systems and
returns without gameplay mutation while the clock is paused.

Task 2 implementation files:

- `src/Terraria.Dome.Simulation/World/WorldClock.cs`
- `src/Terraria.Dome.Simulation/World/Systems/WorldClockSystem.cs`
- `src/Terraria.Dome.Simulation/Snapshots/DomeSimulationSnapshot.cs`
- `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
- `Test/Terraria.Dome.WorldClock.Verification/Program.cs`
- `Test/Terraria.Dome.WorldClock.Verification/Terraria.Dome.WorldClock.Verification.csproj`

Fresh focused evidence before the worktree changed again:

```text
dotnet run --project Test\Terraria.Dome.WorldClock.Verification\Terraria.Dome.WorldClock.Verification.csproj -c Release -p:UseSharedCompilation=false
exit 0; fixed tick rate, day/night boundaries, full cycle, pause/continuation,
simulation tick phase and snapshot continuation all passed

dotnet build src\Terraria.Dome.Simulation\Terraria.Dome.Simulation.csproj -c Release -p:UseSharedCompilation=false
exit 0; 0 warnings; 0 errors

dotnet run --project Test\Terraria.Dome.WorldRules.Verification\Terraria.Dome.WorldRules.Verification.csproj -c Release -p:UseSharedCompilation=false
exit 0; deterministic time and client SetTime rejection passed

dotnet run --project Test\Terraria.Dome.MainBoundary.Verification\Terraria.Dome.MainBoundary.Verification.csproj -c Release -p:UseSharedCompilation=false
exit 0; checked 255 source files; violations 0
```

The earlier green capture and the later drift are retained in
`Build/diagnostics/main-migration/task-2/`. The worktree subsequently stabilized and the current
green evidence is `20260818-233126-world-clock.txt`, `20260818-233126-world-rules.txt` and
`20260818-233126-main-boundary.txt`; each exited `0`. The root Release command then completed
with `0` warnings and `0` errors. Task 2 is complete for the current worktree.

# 2026-08-19 Main server ECS migration Task 3

Task 3 moved world metadata extensions, rules and progression into explicit immutable Simulation
state. `WorldMetadata` now includes a seed variant and random-stream version, with validation for
world identity, full-section dimensions and spawn bounds. `WorldRuleState` models difficulty,
expert/master rules and Crimson selection. `WorldProgressionState` models verified boss,
hard-mode, event and invasion values. `DomeSimulationSnapshot` now carries both state records.

`DomeStatePersistenceFormat` is version 4. A V4 save restores the full clock, extended metadata,
rules and progression through a reconstructed `WorldGridSnapshot`. Earlier V1-V3 saves remain
readable by receiving explicit default clock, rule and progression values. The object payload
format remains independently versioned so its existing chest, sign, tile-entity and opaque
compatibility records are not accidentally discarded while loading a V3 state file.

Fresh focused evidence is retained under `Build/diagnostics/main-migration/task-3/`:

```text
20260819-014217-Terraria.Dome.Persistence.Verification.txt
exit 0; world persistence round-trip, strict recovery and atomic replacement passed

20260819-014217-Terraria.Dome.WorldImport.Verification.txt
exit 0; strict world import options and no-listener failure lifecycle passed

20260819-014217-Terraria.Dome.WorldRules.Verification.txt
exit 0; deterministic clock advancement and rejected client SetTime assertions passed

20260819-014217-Terraria.Dome.MainBoundary.Verification.txt
exit 0; checked 300 Simulation source files; violations 0

dotnet build Terraria.Dome.sln -c Release -p:UseSharedCompilation=false -m:1
exit 0; 0 warnings; 0 errors
```

Task 3 is complete for the recorded source state. The next planned batch is Task 4: replace the
implicit `Main.Update` ordering with a named, deterministic Simulation tick schedule.

# 2026-08-19 Main server ECS migration Task 4

Task 4 made the Simulation tick order explicit and deterministic. `DomeSimulation.Tick` now
records named phase boundaries and publishes the last phase trace and snapshot for verification.
The current authoritative order is:

```text
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
```

Player tile collision deliberately precedes NPC target selection because target and contact
systems consume the player's resolved authoritative position. Equal-sequence mechanism commands
use `Sequence -> MechanismId -> SourceId`; reversing independent input collection order therefore
does not change the phase trace or resulting snapshot. Paused ticks record only the clock boundary
and do not publish a gameplay snapshot.

The spawn completion path in
`src/Terraria.Dome.Server/Protocol/TerrariaProtocolSessionHost.cs` now retains the
`PlayerInitialProjection` and sends, in order, `PlayerActive`, `SyncPlayer`, `PlayerControls`,
the persistent life/mana/buff/loadout/equipment frames, then NPC/host/greeting/completion frames.
This closes the server-authority gap that caused the initial PlayerAuthority verifier failure.

Fresh focused evidence under `Build/diagnostics/main-migration/task-4/`:

```text
20260819-PlayerAuthority-red-current.txt
exit -532462766; complete authority stream was absent before the host fix

20260819-root-build-authority-fix.txt
exit 0; 0 warnings; 0 errors; full Terraria.Dome solution Release build

20260819-PlayerAuthority-green-after-stream-fix.txt
exit 0; tile authority, lifecycle projections, health projection and UUID-owned account
authority all passed

20260819-TickOrder-final.txt
exit 0; named schedule, paused tick, command ordering and reversed-input determinism passed

20260819-DomeVerification-final.txt
exit 0; protocol boundaries, movement, jump/collision, NPC targeting, projectile lifecycle,
handshake, isolation, concurrent sessions and bootstrap validation passed

20260819-PlayerPhysics-final.txt
exit 0; falling players stop above solid tiles

20260819-WorldClock-final.txt
exit 0; fixed rate, day/night boundaries, full cycle, pause/continuation and snapshot continuation
passed

20260819-WorldRules-final.txt
exit 0; deterministic time and rejected client SetTime passed

20260819-MainBoundary-final.txt
exit 0; 305 Simulation source files checked, 0 forbidden dependencies

20260819-Combat-final.txt
exit 0; bounded/ordered combat records, collision, cooldown and loot passed

20260819-CombatProtocol-final.txt
exit 0; combat projection and PVS revision cursors passed

20260819-WorldImport-final.txt
exit 0; strict import options and no-listener failure lifecycle passed

20260819-Persistence-final.txt
exit 0; persistence round-trip, strict recovery and atomic replacement passed
```

The following broader loopback checks remain red and are explicitly not folded into the Task 4
green claim: `PlayerLifecycle.Loopback` does not observe owner death/respawn, `Combat.Loopback`
does not observe the expected combat health transition, `FullClientBootstrap` rejects the current
default-world NPC fixture, and `SessionReplication` does not complete its projection stream.
Their fresh outputs are `20260819-PlayerLifecycleLoopback-second-run.txt`,
`20260819-CombatLoopback-after-stream-fix.txt`, `20260819-FullClientBootstrap-after-stream-fix.txt`
and `20260819-SessionReplication-after-stream-fix.txt`. These are follow-up Task 5/server
replication work, not evidence to discard or silently downgrade.

Task 4 is complete for its focused gate. Task 5 is now underway: player, NPC, projectile and
world-item lifecycle ownership has been extracted into domain-specific stores; see the Task 5
evidence sections below for its current focused gate and remaining loopback gap.

# 2026-08-19 NPC ECS migration Tasks 7-10

The approved first NPC slice now includes typed town-home and segment composition, immutable NPC
state snapshots, SyncNPC projection/codec fixtures, an explicit twelve-stage NPC pipeline
registration and a legacy dependency boundary verifier. This is not a claim of complete Terraria
NPC parity: Bosses, invasions, world events, complete `AI_###` families, Buff behavior, complete
town services and the old networking implementation remain excluded behavior families.

Task 7 added `NpcHomeComponent`, `NpcSegmentComponent`, `NpcHomeSystem` and
`NpcSegmentLifecycleSystem`. The composition verifier confirms town home state and root/parent/
child relationships restore from values without an NPC subclass or Arch entity identity leakage.

Task 8 added `NpcStateSnapshot`, server-side `NpcReplicationAssembler`, `NpcSyncPacket`,
`NpcStateProjector` and `NpcSyncPacketCodec`. The protocol verifier covers snapshot restore,
typed AI projection, unsupported behavior reporting, sparse AI, short/int life-width cases,
inactive stable identity and an exact SyncNPC update frame fixture.

Task 9 added `NpcSystemPipeline` with the registered order: eligibility, spawn commit, target,
behavior, movement intent, movement/collision, contact, damage, lifecycle, death, loot and
replication. The main NPC verifier asserts the exact registration sequence. The current tick now
executes the spawn-through-movement range before projectile movement, then executes contact,
damage, lifecycle, death, loot and replication in the second range; NPC spawn, despawn and damage
compatibility commits are no longer called from the generic domain-command commit block.

Task 10 added `Test/Terraria.Dome.Npc.Boundary.Verification`. Fresh root-level evidence:

```text
dotnet build src/Terraria.Dome.Simulation/Terraria.Dome.Simulation.csproj -p:UseSharedCompilation=false
exit 0; 0 warnings; 0 errors

dotnet run --project Test/Terraria.Dome.Npc.Verification/Terraria.Dome.Npc.Verification.csproj -p:UseSharedCompilation=false
exit 0; target/chase, spawn, combat, death and deterministic loot passed

dotnet run --project Test/Terraria.Dome.Npc.Composition.Verification/Terraria.Dome.Npc.Composition.Verification.csproj -p:UseSharedCompilation=false
exit 0; town-home and segment composition snapshot/restore passed

dotnet run --project Test/Terraria.Dome.Npc.Protocol.Verification/Terraria.Dome.Npc.Protocol.Verification.csproj -p:UseSharedCompilation=false
exit 0; NPC state snapshot and SyncNPC projection/codec fixtures passed

dotnet run --project Test/Terraria.Dome.Npc.Boundary.Verification/Terraria.Dome.Npc.Boundary.Verification.csproj -p:UseSharedCompilation=false
exit 0; checked 353 Simulation source files; legacy NPC violations 0
```

`dotnet msbuild` resolved `BaseOutputPath` to `Build/bin/Terraria.Dome.Simulation`,
`BaseIntermediateOutputPath` to `Build/obj/Terraria.Dome.Simulation`, and generated source output
to `Build/generated/Terraria.Dome.Simulation/Debug/net10.0`. The built Simulation DLL and
intermediates were observed below these locations.

# 2026-08-19 Projectile ECS migration gate

The selected Projectile ECS slice is **partial and verified**. It adds typed definition,
behavior, stable network identity, penetration, lifetime, damage and replication state for the
`linear` and `gravity` behavior definitions. Combat resolution uses deterministic candidate
ordering and a single commit boundary; message 27 projects typed state through sparse flags and
message 29 validates the stable owner plus identity pair.

The read-only legacy inventory was regenerated from
`D:\TRbackup\Version4物理删除了某些文件\Terraria\Projectile.cs` with SHA-256
`97C3D68586AEF862411699E428F5C4B18CEE1F8051B684BE9F86FA7B60FAE83B`. It records 54,799
lines, 790 `aiStyle` assignments and 206 distinct legacy values. Only `linear` and `gravity` are
counted as migrated. Every other legacy behavior family remains explicitly unsupported.

Fresh Release evidence is in
`Build/diagnostics/projectile-migration-20260818/verification.json` and
`docs/plans/2026-08-18-projectile-ecs-migration-evidence.md`. The Simulation and Server rebuilds
completed with 0 warnings and 0 errors. Combat, combat-protocol, completion, hardening-loopback
and full-client-bootstrap verifiers exited 0; combat loopback exited 0 twice. The final hardening
run covers malformed, flood, disconnect, slow-reader and rapid-reconnect isolation. The Simulation
legacy-dependency scan found zero matches for `Terraria.Projectile`, `Main.`, `NetMessage`,
`SoundEngine`, `AI(` and `Update(`.

The legacy `Projectile.cs` was not modified or deleted. The migration integration gate is green
for the selected slice but remains partial by design. Full Terraria projectile parity remains out
of scope until every remaining behavior family has typed state, a fixture, protocol projection
and current verification evidence.

# 2026-08-19 Main server ECS migration Task 5 current gate

Task 5 extracted domain-specific lifecycle ownership into `PlayerStore`, `PlayerLifecycleSystem`,
`NpcStore`, `ProjectileStore` and `WorldItemStore`. `DomeSimulation` now uses these stores for
server-owned handles, replication IDs and world-item tombstones. The equipment facade was also
completed for the existing Item verifier: `QueueEquipItem` commits on the simulation tick and
`CreateEquipmentSnapshot` exposes immutable typed state without replacing an occupied slot.

The current source tree was revalidated after the implementation changed. Fresh evidence is under
`Build/diagnostics/main-migration/task-5/`:

```text
20260819-PlayerOwnership-current.txt
exit 0; player ownership, lifecycle and snapshot contracts passed

20260819-Items-current.txt
exit 0; inventory, world-item, item-use, equipment and immutable snapshot contracts passed

20260819-Npc-current.txt
exit 0; NPC source contract, target/chase, death revision and deterministic loot passed

20260819-Combat-current.txt
exit 0; bounded player vitals, combat records, collision, cooldown and loot passed

20260819-Dome-current-after-gravity-fix.txt
exit 0; protocol, movement, jump/ground collision, NPC chase, projectile lifecycle and sessions passed

20260819-PlayerAuthority-current.txt
exit 0; tile collision, player death/respawn simulation authority and UUID state restoration passed

20260819-MainBoundary-current.txt
exit 0; checked 315 Simulation source files, 0 forbidden dependencies

20260819-root-build-current-retry.txt
exit 0; Release solution build, 0 warnings, 0 errors

20260819-FullClientBootstrap-current-binary.txt
exit 0; full-client bootstrap completed without source-player authority replay

20260819-SessionReplication-current-binary.txt
exit 0; player movement, PVS cursor, disconnect/replacement and immutable snapshot replication passed
```

During the gate, `Dome.Verification` exposed an independent regression in the changed
`PlayerGravitySystem`: clamping downward velocity to `-1` prevented the existing eight-tick jump
fixture from reaching the floor. The clamp was removed, restoring the previously verified gravity
behavior; the fresh Dome verifier then passed. The first root build also observed a transient
missing `System.Linq`/reference-output failure during concurrent work; the current direct
PlayerAuthority verifier and subsequent serial root build both passed, so that stale failure is not
treated as a source defect.

The initial broad loopback observation (superseded by the final revalidation below) was:

```text
20260819-PlayerLifecycleLoopback-current.txt
exit -532462766; observer did not receive owner death/respawn
  Death=False, Respawn=False, Tick=163, SimulationFault=none, LastSessionFault=none

20260819-CombatLoopback-current.txt
exit -532462766; observer did not receive complete combat lifecycle
  Npc=67, Projectile=17, Kill=1, observed NPC health remained 100
```

These failures are cross-session behavior gaps, not evidence that the Store contracts failed. The
next batch must trace the contact-damage/death projection and the combat fixture's authoritative
NPC damage path before Task 5 can be accepted. No claim of complete Main migration is made.

## Task 5 final current-source revalidation

The source tree was re-built and the focused binaries were run again after the concurrent verifier
changes settled:

```text
20260819-root-build-final-retry-3.txt
exit 0; Terraria.Dome.sln Release build, 0 warnings, 0 errors

20260819-PlayerOwnership-final-binary.txt
exit 0; player ownership, lifecycle and snapshot contracts passed

20260819-Items-final-binary.txt
exit 0; item inventory, pickup, equipment, persistence and compatibility contracts passed

20260819-Npc-final-binary.txt
exit 0; NPC source, target/chase, death revision and deterministic loot passed

20260819-Combat-final-binary.txt
exit 0; bounded player vitals and authoritative combat records passed

20260819-Dome-final-binary.txt
exit 0; protocol, movement, jump/ground, NPC chase and projectile lifecycle passed

20260819-MainBoundary-final-binary.txt
exit 0; checked 320 Simulation source files, 0 forbidden dependencies

20260819-CombatLoopback-final-binary.txt
exit 0; two-session combat is PVS-limited and produces one server-owned drop

20260819-PlayerLifecycleLoopback-final-binary.txt
exit -532462766; observer did not receive authoritative death and respawn
  Tick=168, Players=1:25:True,2:25:True,3:25:True, Npcs=1:2100.0,1.0,
  SimulationFault=none, LastSessionFault=none
```

The earlier two-loopback-red note is superseded by the following accepted revalidation. Combat
loopback is green, and PlayerLifecycle loopback now passes after its fixture and server state
settled; three direct replay runs also passed. The direct Combat verifier now asserts that an
overlapping ordinary NPC produces exactly one authoritative contact-damage event. Task 5 is
accepted for the current source tree and the next Main migration batch is Task 6 server bootstrap.

```text
20260819-ContactDamage-red.txt
exit 0; direct overlapping hostile-NPC contact damage assertion passed

20260819-PlayerLifecycle-diagnostic.txt
exit 0; player death, automatic respawn and PVS limitation passed

20260819-PlayerLifecycle-repeat-1.txt
20260819-PlayerLifecycle-repeat-2.txt
20260819-PlayerLifecycle-repeat-3.txt
each exit 0; player death, automatic respawn and PVS limitation passed

20260819-root-build-after-lifecycle-green.txt
exit 0; Terraria.Dome.sln Release build, 0 warnings, 0 errors

20260819-PlayerLifecycle-focused-accepted.txt
exit 0; player identity, lifecycle and account ownership are separated

20260819-PlayerLifecycleLoopback-accepted.txt
exit 0; player death and automatic respawn are authoritative and PVS-limited
```

# 2026-08-19 Item ECS migration Tasks 5-9 current gate

The item migration slice is `PARTIAL` with a fresh scoped evidence bundle at
`Build/evidence/item-ecs/final-verification.txt` and
`Build/evidence/item-ecs/final-manifest.json`. The authoritative Simulation path now covers inventory
instance revisions, world-item spawn/motion/pickup, item use and cooldown, ammunition, placement,
equipment conflict/vanity state, deterministic drops, prefix and one-time variant commands, immutable
snapshots, and complete source-inventory instance metadata.

The Server projects inventory and equipment snapshots through V1456 `SyncEquipment` frames. Equipment
uses the Version4 armor base slot (`59`) and resolves its item payload from the source inventory snapshot.
The owner-only replication loop is revision-deduplicated. Focused loopback evidence also sends forged
client `SyncItem` and `SyncEquipment` frames; neither mutates Simulation-owned state.

Persistence is format version 6. World item instance state remains format version 5, while player account
items now preserve `VariantId`, `Dye`, `Paint` and `NameOverride`; versions 1-5 remain readable with
default account instance metadata. The persistence verifier passed world and account round-trip,
strict trailing-byte recovery and atomic replacement.

Task 8 added three data-only compatibility adapters and the exact legacy reference audit. The query for
`Terraria.Item`, `Main.item`, `Player.inventory`, `ContentSamples` and `ItemID.Sets` returned zero matches
under `src/Terraria.Dome.Simulation`. The member mapping has 52 deferred and 11 compatibility members;
classification is complete, but deferred/UI members and full Version4 parity are not claimed. The old
Version4 `Item.cs` source baseline remains intact. A source artifact scan still finds 78 pre-existing
`src/**/Build/obj` paths; they were preserved rather than broadly removed.

The Task 6 drop rule path was re-audited after the initial gate: `ItemDropCondition` values now gate
eligible rules, explicit non-negative `ChainId` values select one result by deterministic total weight,
and master mode satisfies expert-only drops. Variant conditions use the same immutable context contract.
Items, Simulation, Server, Loopback, Protocol Compatibility and Persistence were rerun after this change;
all exited `0` with the scoped builds reporting `0` warnings and `0` errors.

The equipment lifecycle now also has an authoritative `UnequipItemCommand`: occupied slots still reject
replacement, an accepted unequip increments the equipment revision and clears the snapshot slot, and a
later equip can use a different source inventory slot. The Items verifier covers this sequence, followed
by fresh Simulation/Server builds and Loopback, Protocol Compatibility and Persistence runs.

The latest current-gate pass also closes the committed-event publication gap for the migrated item paths.
`DomeSimulation` retains immutable per-tick projections for accepted item use, equipment, prefix, world
item creation/pickup, and NPC loot drops. The buffers clear at `BeginTick`; rejected use/ammo/equipment/
prefix/pickup paths produce no event. `SpawnWorldItem` records a creation fact immediately because it
commits synchronously. The full Items, Definitions, Loopback, Protocol Compatibility and Persistence
matrix was rerun with `FixtureHostBuild=false`; every command exited `0`, and both Release builds had
`0` warnings and `0` errors. This strengthens the scoped evidence but does not alter the migration's
`PARTIAL` status or its documented deferred legacy behavior and source-artifact boundary.

The current execution pass closes the remaining named item-domain command and event contracts from the
confirmed design. `TransferItemCommand`, `SplitItemStackCommand`, `MergeItemStackCommand` and
`DropItemCommand` commit exact, atomic quantity changes through `InventoryCommandSystem`; rejected
capacity and incompatible-instance requests leave inventory and event buffers unchanged. Player drops
produce the committed inventory change, world-item creation and `ItemDroppedEvent` facts together.
`WorldItemDestroySystem` now retains inactive revisioned tombstones, and the Items Loopback verifier
observes their PVS-limited V1456 `SyncItem` projection. `InventoryChangedEvent` publishes immutable
per-slot results for item transactions. Definitions now retain dimensions, value and rarity, while item
use validates mana cost and covers channel, animation and `ItemRecoveryDefinition` health/mana behavior.
Requirement-by-requirement status is recorded in `Build/evidence/item-ecs/requirements-audit.md`; the
migration remains `PARTIAL` for the same documented artifact and deferred-parity boundaries.

The current Item ECS gate also closes a prefix-authority gap discovered during the requirements audit.
Nonzero `ApplyItemPrefixCommand` values are no longer accepted merely because they fit in `ushort`:
`ItemPrefixDefinition` holds the immutable server-defined eligibility set, and Simulation type `3`
currently allows only prefix `7`. The focused Items verifier first failed when direct prefix `6` mutated
state, then passed after both direct and queued unregistered-prefix cases left state, inventory revision
and committed prefix/inventory events unchanged. Prefix `0` remains a server-authoritative reset even
for items without any nonzero eligible prefixes. Fresh scoped Simulation/Server builds and the
Definitions, Items, Loopback, Protocol Compatibility and Persistence verifiers all exited `0` with
the builds reporting `0` warnings and `0` errors. This strengthens Task 6's server slice only; the
migration stays `PARTIAL` for deferred members and full Version4 parity.

The same migration pass moves `WorldItemStore` from an aggregate-value dictionary to a private
`ReplicationId -> Arch Entity` map. Each world-item entity carries the aggregate projection plus the
typed `ItemStackComponent`, `ItemInstanceStateComponent`, `ItemWorldStateComponent` and
`ItemOwnershipComponent`; Arch entities do not cross into Server, persistence or network APIs. The
Items verifier was first red because world-item spawn created no corresponding entity, then proved
spawn and destroyed-tombstone component synchronization. Persistence was then red because restored
active items held the default world-state component; restore now reconstructs active and revision from
the persisted replication snapshot while resetting non-persisted transient fields. Fresh Simulation/
Server builds plus Definitions, Items, Loopback, Protocol Compatibility and Persistence verification
all exited `0`, with both builds reporting `0` warnings and `0` errors. This contributes Task 3/4
runtime convergence evidence only; full Version4 parity remains `PARTIAL`.

The next Item ECS batch closes world-item lifecycle state that was previously only declared. Spawn now
accepts a non-negative pickup delay; the current tick checks that delay before pickup, then decrements it
deterministically with monotonic aggregate/world-state revisions. A committed pickup records the winning
player and ownership revision on the private Arch entity. `ItemReplicationSnapshot` carries the world
state, and Dome persistence format 8 writes all six fields while readers for versions 1-7 reconstruct
compatible active/revision state. Items covers delay expiry and ownership, Persistence covers format 8
round-trip and legacy reads, and the existing loopback/protocol gates remain in scope. This is still
bounded Task 3/4 runtime evidence; deferred members and full Version4 parity remain `PARTIAL`.

The following Task 3 slice attaches `InventoryComponent` directly to every player Arch entity while
retaining the existing `_inventories` lookup as an index to that same object. The Items verifier first
failed because the queried player entity had no inventory component, then passed after reference
identity was established. This keeps inventory commands, snapshots and persistence on one authoritative
instance and does not expose Arch entities across Server boundaries. Full Item parity remains
`PARTIAL`.

The world-item race audit then found and closed a real partial-stack bug: two pickup commands for one
replication ID could previously both commit in one tick when the first player had room for only part of
the stack. A per-tick winner set now blocks later commands after the first accepted quantity, while a
fully rejected command does not consume the winner slot. The Items verifier covers the first player
receiving one unit, the second receiving none, the world stack retaining one unit, and exactly one
pickup event. This strengthens the server-authoritative Task 4 slice without changing the documented
`PARTIAL` parity boundary.

The latest Item definition pass closes an ammunition-contract ambiguity. The focused Definitions
verifier first accepted an item whose `Use` and `Combat` definitions both consumed ammunition but
required different item types; the runtime resolver would otherwise silently prefer `Use`. The
definition compiler now rejects that contradictory pair during registry construction, while allowing
the existing single-source ammunition contract. Focused Definitions, Items, Loopback, Protocol
Compatibility and Persistence verifiers, the Simulation and Server builds, and the solution Release
build all exited `0` with `0` warnings and `0` errors. This is a bounded definition-validation slice;
deferred members, the retained Version4 source baseline and full Terraria item parity remain
`PARTIAL`.

The next Item placement slice closes a previously unused definition branch. `ItemPlacementDefinition`
now requires exactly one valid tile or wall type; negative wall types and simultaneous tile/wall
contracts are rejected by the definition compiler. `ItemPlacementSystem` emits `Place` for tile-only
items and `SetWall` for wall-only items. Simulation type `6` is a wall-only fixture: it can place a
wall behind an active tile, consumes one item after the committed tile change, and rejects an already
occupied wall without changing inventory. Definitions, Items, Loopback, Protocol Compatibility and
Persistence, both scoped builds, and the solution Release build all passed after this slice. Deferred
members, the retained Version4 baseline, and the source artifact boundary keep the migration
`PARTIAL`.

The following Item use slice closes the previously ignored `ItemRecoveryDefinition` buff branch.
Recovery now validates `BuffType` and `BuffDurationTicks` as an atomic pair and carries explicit
consumable behavior. A valid buff-only item produces a typed `ItemUseResult`/`ItemUsedEvent`, checks
capacity before consuming inventory, and commits the duration to the player entity's
`BuffCollectionComponent`; malformed type/duration pairs leave use state unchanged. The focused
Definitions and Items verifiers, both scoped builds, the five-project Item matrix, and the solution
Release build all passed. Full Terraria parity, deferred members, and artifact cleanup remain
`PARTIAL`.

The next Item use slice closes the definition-selected projectile gap. `ShootType` and `ShootSpeed`
were previously only used to classify an action, while projectile requests used a hard-coded type and
velocity. Accepted item uses now validate the projectile definition, enqueue its selected type and
speed through the existing Projectile ECS command, and publish the actual type in replication. The
Items verifier first failed for the ranged fixture, then passed after proving projectile type `2` and
speed `6.0`; the full five-project Item matrix, scoped Simulation/Server builds and solution Release
build all exited `0` with no warnings or errors. Full Terraria parity and deferred boundaries remain
`PARTIAL`.

The next Item use slice closes the pure Combat projectile fallback. `ItemCombatDefinition` projectile
metadata was previously inert unless a separate `Use` definition supplied a shoot action. Items with
no `Use` now qualify when Combat declares a projectile; type, speed and damage flow through the same
authoritative Projectile ECS spawn path. The focused Items verifier proves a Combat-only fixture emits
type `2`, speed `5.0` and damage `14`; Definitions, Items, Loopback, Protocol Compatibility and
Persistence, both scoped builds and the solution Release build all passed with zero warnings/errors.
Full Terraria parity and deferred boundaries remain `PARTIAL`.

# 2026-08-19 Item ECS passive world-item stacking and persistence compatibility accepted

The next Item ECS slice adds server-owned passive stacking. `WorldItemStackingSystem` runs during
command commit, selects the lower replication ID as the deterministic receiver, and merges only
active, nearby, same-section items with matching type and instance metadata. Unique items, items at
capacity, pickup-delayed items and items already merged in the current tick are rejected. Receiver
capacity is capped by the immutable definition, donor remainder stays authoritative (or becomes an
inactive tombstone), both revisions advance, `LastMergeTick` is recorded, and Arch components are
synchronized. The focused Items verifier proves `95 + 10` becomes `99 + 6` and proves delayed items
do not merge before pickup delay expiry.

The same verification pass corrected the version-10 persistence compatibility fixture. Its previous
layout rewrite accidentally removed the ten progression booleans between the v11 wind fields and the
v12 Lantern Night field, causing strict trailing-data rejection. The fixture now removes only the two
wind floats and Lantern Night bool while preserving the intervening legacy fields. Definitions, Items,
Loopback, Protocol Compatibility and Persistence all passed; Simulation, Server and solution Release
builds passed with zero warnings and zero errors. That verification used Dome persistence format v12,
with wind at v11 and Lantern Night at v12. Full Terraria parity, deferred members and artifact-boundary work keep
the migration `PARTIAL`.

# 2026-08-19 Main ECS migration Task 6 server bootstrap accepted

Task 6 is accepted for the current source tree. Its falsifiable proposition is: two calls to
`WorldBootstrap.CreateDefault()` create equal metadata and first-tile state, retain exactly 23
server-owned chests, preserve spawn `(2100, 300)`, reject a missing `.wld` path, and do not recreate
those chests when `DomeServer()` consumes its default bootstrap result. The initial focused RED failed
because `WorldBootstrap` and `WorldBootstrapResult` did not exist. The final implementation puts
default metadata, generation and chest construction in `Server/Startup/WorldBootstrap.cs`; it returns
only a `DomeSimulationSnapshot` plus an origin flag, creates neither sockets nor client state.

`DomeServer()` now consumes that result, and `Program.cs` consumes `WorldBootstrap.Load(...)` before
opening its listener. The prior `DomeServer.CreateDefaultWorld` and default-chest construction paths
are removed. Default NPC creation remains in the session initialization path because its placement
depends on the server-selected player spawn; it is not part of world bootstrap and is not duplicated.

Fresh evidence is under `Build/diagnostics/main-migration/task-6/`:

```text
world-import-final.txt
exit 0; strict import options, deterministic default bootstrap, missing-file rejection and
  no duplicate default chests passed

persistence-final.txt
exit 0; world persistence round-trip, strict recovery and atomic replacement passed

world-server-final.txt
exit 0; server-owned section visibility versions and initial replication passed

full-client-bootstrap-final.txt
exit 0; full-client bootstrap completes without source-player authority replay

main-boundary-final.txt
exit 0; checked 321 Simulation source files, 0 forbidden dependencies

root-release-build-final.txt
exit 0; Terraria.Dome.sln Release build, 0 warnings, 0 errors
```

The next Main migration task is Task 7. It must model one weather or progression event family at a
time as deterministic state transitions with accepted, rejected, persistence and projection evidence.

# 2026-08-19 Main ECS migration Task 7 BloodMoon slice accepted

The first Task 7 event-family slice is `BloodMoon`, sourced from Version4
`Terraria/Main.cs:13868-13910` and V1456 `NetMessage.cs:case 7`. It is deliberately not a broad
weather, invasion or moon-event port. `WorldProgressionState.IsBloodMoon` is now Simulation-owned;
`WorldEventStartCommand(BloodMoon, sequence)` is accepted only during an unpaused night, commits in
the `ApplyWorldClock` phase through `WorldProgressionSystem`, rejects duplicate/daytime/unknown input
without changing progression, and clears deterministically at the day boundary. Clock and progression
are emitted by `CreatePersistenceSnapshot` and restored by `DomeSimulation(snapshot)`.

The Server no longer freezes WorldData state at construction. `TerrariaProtocolSessionHost` receives a
context factory; `DomeServer.CreateWorldDataContext()` combines the current Simulation clock and
progression with immutable metadata. V1456 WorldData flag bit 1 now represents authoritative blood
moon state, matching the Version4 `NetMessage.cs:case 7` writer. No client packet starts the event.

The focused verifier was first RED because `DomeServer.CreateWorldDataContext()` did not exist. Its
final scenarios cover daytime rejection, duplicate/unknown rejection, nighttime start, dawn cleanup,
snapshot continuation, direct WorldData bit projection, and a real host loopback WorldData response.
The loopback fixture uses the required `4200 x 1200` section-capable world; a smaller `400 x 300`
fixture was rejected by the existing 5x3 initial-section contract rather than by the event system.

Fresh evidence is in `Build/diagnostics/main-migration/task-7-blood-moon/`:

```text
world-rules-final.txt
exit 0; deterministic clock, client SetTime rejection, BloodMoon projection and state machine passed

world-rules-loopback-final.txt
exit 0; real session received BloodMoon WorldData and completed world entry

persistence-final.txt
exit 0; persisted progression restores into DomeSimulation

protocol-compatibility-final.txt
exit 0; V1456 byte-length contracts remain compatible

full-client-bootstrap-final.txt
exit 0; full client bootstrap completed without source-player authority replay

main-boundary-final.txt
exit 0; checked 338 Simulation source files, 0 forbidden dependencies

root-release-build-final.txt
exit 0; Terraria.Dome.sln Release build, 0 warnings, 0 errors
```

Task 7 remained `IN PROGRESS` after this slice: rain, slime rain, eclipse, invasion, meteor and other progression
families are not implied by this result. The next batch must choose one of those families and repeat
the same accepted/rejected/persistence/projection loop.

# 2026-08-19 Main ECS migration Task 7 Eclipse slice accepted

The second Task 7 event-family slice is `Eclipse`, sourced from Version4
`Terraria/Main.cs:13886-13951` and V1456 `NetMessage.cs:case 7`. It models only the supported
authority transition: a server-owned request can begin Eclipse during daytime when
`IsHardMode && DefeatedMechanicalBoss`; it is rejected outside that capability boundary and ends at
night. The random `rand.Next(20)` decision, announcement, achievements, monster spawn table and other
legacy moon-event side effects remain outside this slice.

`WorldProgressionState` now contains `DefeatedMechanicalBoss` as explicit prerequisite state. The
binary persistence writer is version 7: it writes that bool immediately after
`DefeatedWallOfFlesh`; the reader preserves the v1-v6 layout and supplies `false` for the new value
until a v7 payload is read. `WorldProgressionSystem` keeps Eclipse active only during daytime and
keeps it mutually ordered with the existing BloodMoon cleanup. `LegacyWorldDataContext.WithWorldState`
projects Eclipse into WorldData bit 2 (daytime Eclipse flags are `0b101`).

The focused verifier was RED because neither the named progression prerequisite nor the Eclipse event
kind existed. The accepted verifier covers qualified start, unqualified rejection, night cleanup,
snapshot continuation and the exact WorldData flag. Persistence uses a v7 round-trip with the new
prerequisite set. Fresh evidence is under `Build/diagnostics/main-migration/task-7-eclipse/`:

```text
world-rules focused run
exit 0; BloodMoon and Eclipse deterministic state scenarios passed

world-rules-loopback.txt
exit 0; real world entry remains stable

persistence verifier
exit 0; v7 progression round-trip and strict recovery passed

protocol-compatibility.txt
exit 0; V1456 compatibility length contracts passed

full-client-bootstrap.txt
exit 0; full-client bootstrap passed

main-boundary.txt
exit 0; checked 339 Simulation source files, 0 forbidden dependencies

root-release-build.txt
exit 0; Terraria.Dome.sln Release build, 0 warnings, 0 errors
```

Task 7 remains `IN PROGRESS`: rain, slime rain, invasion, meteor and all unsupported world event
families still need their own transitions and evidence.

# 2026-08-19 Main ECS migration Task 7 Invasion slice accepted

The third Task 7 event-family slice is `Invasion`, referenced from Version4
`Terraria/Main.cs:12215-12254`, `12962-13040`, `13047-13128`, and V1456
`NetMessage.cs:case 7`. The accepted scope is deliberately limited to server-owned deterministic
state: a validated start request contains a supported type and positive size; a validated progress
request depletes that size; completion is normalized to `(InvasionType=0, InvasionSize=0)`; and
WorldData projects only the invasion type. NPC waves, message 78 progress frames, random triggers,
announcements, UI, and client-authoritative starts are explicitly not part of this slice.

`WorldInvasionStartCommand` accepts types `1..4`, positive sizes and non-negative sequences.
`WorldInvasionProgressCommand` accepts positive amounts and non-negative sequences.
`DomeSimulation` owns the request queues and rejects invalid, duplicate-active, inactive or completed
requests before mutation. `WorldProgressionSystem` commits the state during `ApplyWorldClock`, orders
event cleanup before invasion work, and normalizes either a zero type or zero size to the completed
pair. `DomeServer` exposes queue entry points without allowing the protocol to write progression
state. `LegacyWorldDataContext.WithWorldState` rejects unrepresentable type values and projects the
accepted type through `LegacyWorldProgressionState.InvasionType`.

The focused verifier was initially RED because the invasion commands and queue APIs did not exist.
Its accepted cases cover start `(2,40)`, invalid type rejection, duplicate-start rejection, tick
commit, progress `(40 -> 25)`, snapshot continuation, WorldData projection, completion `(0,0)`, and
post-completion progress rejection. Fresh final evidence is in
`Build/diagnostics/main-migration/task-7-invasion/20260819-013459/`:

```text
world-rules-final.txt
exit 0; BloodMoon, Eclipse and Invasion deterministic state scenarios passed

persistence-final.txt
exit 0; persistence round-trip and strict recovery passed

world-rules-loopback-final.txt
exit 0; stable world entry passed

protocol-compatibility-final.txt
exit 0; V1456 byte-length and isolation contracts passed

full-client-bootstrap-final.txt
exit 0; full-client bootstrap completed without source-player authority replay

main-boundary-final.txt
exit 0; checked 342 Simulation source files, 0 forbidden dependencies

root-release-build-final.txt
exit 0; Terraria.Dome.sln Release build, 0 warnings, 0 errors
```

Two failed preliminary executions are not treated as behavior failures: the first Persistence run
observed a transient output-file lock and passed after the process exited; the first
FullClientBootstrap run passed `-m:1` through `dotnet run` as an application argument. The execution
handbook now omits that argument for `dotnet run` and retains serial invocation plus `-m:1` for the
root build. Task 7 remains `IN PROGRESS`: rain, slime rain, meteor and all other unsupported event
families require their own source, authority, behavior and evolution evidence.

# 2026-08-19 Main ECS migration Task 7 Rain slice started, partial

The next Task 7 event family is `Rain`, sourced from Version4 `Main.cs:13236-13339` and
`13388-13523`, with WorldData's `maxRaining` projection in `NetMessage.cs:277-281`. The active scope
is deterministic server-owned rain only: an explicit duration and strength request, duplicate and
invalid-request rejection, consumption by `WorldClockSnapshot.TicksPerUpdate`, expiration to a
canonical clear state, and snapshot continuation. Random duration/strength selection, coin rain,
Lantern Night, wind, cloud rendering, automatic random starts, Slime Rain and Meteor remain outside
this slice.

The focused RED initially failed because `WorldRainStartCommand`, `TryQueueWorldRain`, observable rain
state and `WorldWeatherSystem` did not exist. The minimal authority path now queues a valid command in
`DomeSimulation`, commits it in `ApplyWorldClock` through `WorldWeatherSystem`, and stores the result
in `WorldRuleState`. The focused verifier passes invalid duration/strength rejection, duplicate
rejection, initial commit, deterministic depletion, direct snapshot continuation and normalization.

While recompiling the new source, the build exposed three pre-existing `CS0118` errors in untracked
entity modules where `Arch.Core.World` was shadowed by the project namespace. The affected
`PlayerLifecycleSystem`, `NpcMovementIntentSystem` and `NpcSpawnCommitSystem` now use the existing
`ArchWorld` disambiguation pattern. After that repair, the focused world-rules verifier and the serial
root Release build passed with zero warnings and zero errors.

This Rain slice is **not accepted**. Before it can be accepted, add a format-versioned binary
persistence round-trip with v1-v7 recovery, project active rain strength to the existing V1456
WorldData `MaximumRaining` field, verify the real-host WorldData loopback behavior, run affected
Persistence/Protocol/FullClientBootstrap/MainBoundary gates and save fresh evidence under
`Build/diagnostics/main-migration/task-7-rain/`. Task 7 remains `IN PROGRESS`.

# 2026-08-19 Main ECS migration Task 7 Rain slice accepted

The Rain behavior family is accepted for the current source tree. It is referenced from Version4
`Terraria/Main.cs:13236-13339` (`StopRain`, `StartRain`, `ChangeRain`), `13388-13523` (duration
consumption in `UpdateTime`), and `Terraria/NetMessage.cs:277-281` (inactive rain projects
`maxRaining=0.0f`). The accepted scope is explicit, deterministic, server-owned rain only: a valid
duration and strength request commits in `ApplyWorldClock`; invalid and duplicate requests are
rejected; each tick consumes the duration using `WorldClockSnapshot.TicksPerUpdate`; and expiration
normalizes to a clear state with zero duration and strength.

`WorldWeatherSystem` remains the single state-transition owner. `DomeSimulation` accepts only
`WorldRainStartCommand` requests and does not expose direct mutable `WorldRuleState`. The state is
carried in the immutable persistence snapshot. `DomeStatePersistenceFormat` is now v9: it appends
`RainTimeTicks` and `RainStrength` after the pre-existing four WorldRule fields. V1-V3 snapshots
retain default rules; v4-v8 preserve their old rule layout and restore canonical clear rain; v9
reads the new fields. The Persistence verifier proves a v9 active-rain round-trip and a hand-built
v8 payload recovery.

`DomeServer.CreateWorldDataContext()` now passes the authoritative `WorldRuleState` through the
existing V1456 adapter. An active rain state maps to the already-defined
`LegacyWorldBackgroundState.MaximumRaining` field; no WorldData length or field order changes. The
real TCP WorldRules loopback fixture reads the WorldData payload and verifies the `0.5f` rain float,
alongside its existing BloodMoon flag assertion.

The focused RED first failed because binary persistence omitted rain fields and WorldData used the
default background. The accepted current-source evidence is retained in
`Build/diagnostics/main-migration/task-7-rain/20260819-020437/`:

```text
world-rules-final.txt
exit 0; deterministic Rain state machine, rejection and WorldData projection passed

persistence-final.txt
exit 0; v9 active-rain round-trip, v8 clear-rain recovery and strict persistence checks passed

world-rules-loopback-final.txt
exit 0; real session received the authoritative MaximumRaining WorldData field

protocol-compatibility-final.txt
exit 0; V1456 packet-length and field-order compatibility passed

full-client-bootstrap-final.txt
exit 0; full client bootstrap completed without source-player authority replay

main-boundary-final.txt
exit 0; checked 347 Simulation source files, 0 forbidden dependencies

root-release-build-final.txt
exit 0; Terraria.Dome.sln Release build, 0 warnings, 0 errors
```

Task 7 remains `IN PROGRESS`. This result does not include automatic random weather, legacy global
random ordering, coin rain, Lantern Night, wind, cloud rendering, Slime Rain, Meteor, NPC spawn
effects, announcements, UI, or any client-authoritative weather operation. The next batch must read
its Version4 reference and accept exactly one remaining event family under the same evidence loop.

# 2026-08-19 Main ECS migration Task 7 Slime Rain slice accepted

The next accepted Task 7 family is `Slime Rain`, referenced from Version4 `Terraria/Main.cs:13341-13386`
for start/stop, `13415-13430` for time consumption, and `Terraria/NetMessage.cs:303` for the
WorldData `bitsByte8[2]` projection. Its deliberate scope is only an explicit, deterministic,
server-owned duration request. The legacy random duration, start probability, negative cooldown,
remix and world-surface gates, warning timer, chat notification, slime spawn table, NPC slots and
King Slime behavior remain outside this slice.

`WorldSlimeRainStartCommand` validates a positive duration and non-negative sequence. The only
authority path is `DomeSimulation.TryQueueWorldSlimeRain` followed by `WorldProgressionSystem` in
`ApplyWorldClock`. A request is rejected when ordinary rain is active or queued, when slime rain is
already active or queued, or when its command is invalid. Conversely, ordinary Rain cannot be queued
while Slime Rain is active or pending. A start commits without consuming duration in that same tick;
subsequent ticks subtract `WorldClockSnapshot.TicksPerUpdate`, and zero is the canonical stopped state.

`WorldProgressionState` now owns `SlimeRainTimeTicks` and exposes `IsSlimeRaining`. The persistence
format is v10 and appends that duration after the v9 progression layout; v1-v9 state restores clear
slime rain. During this version increment, the existing world-item instance reader was corrected to
gate its v8 fields by `ItemWorldStateFormatVersion`, not the new total format version, preserving v8
item instance layout. The V1456 adapter maps active Slime Rain to `LegacyWorldProgressionState.EventFlags3`
bit 2, matching `NetMessage.cs:bitsByte8[2]` without changing WorldData length or field order.

The RED first failed because the command, queue API, progression state and projection were absent.
The accepted current-source evidence is retained under
`Build/diagnostics/main-migration/task-7-slime-rain/20260819-021430/`:

```text
world-rules-final.txt
exit 0; start/rejection, Rain mutual exclusion, tick depletion, continuation and projection passed

persistence-final.txt
exit 0; v10 Slime Rain progression round-trip and strict persistence checks passed

world-rules-loopback-final.txt
exit 0; real session received EventFlags3 bit 2, plus existing rain and BloodMoon fields

protocol-compatibility-final.txt
exit 0; V1456 packet length and field-order compatibility passed

full-client-bootstrap-final.txt
exit 0; full client bootstrap completed without source-player authority replay

main-boundary-final.txt
exit 0; checked 349 Simulation source files, 0 forbidden dependencies

root-release-build-final.txt
exit 0; Terraria.Dome.sln Release build, 0 warnings, 0 errors
```

Task 7 remains `IN PROGRESS`. Meteor, random weather and Slime Rain scheduling, negative cooldown,
warning/announcement, NPC spawn effects, King Slime behavior, UI and all client-authoritative starts
remain unsupported. The next migration batch must select exactly one of these remaining behavior
families and repeat the same source, authority, behavior and evolution evidence loop.

# 2026-08-19 Item ECS equipment-state entity ownership batch, partial

The next bounded Item ECS convergence gap was player equipment state. The previous implementation
kept `Dictionary<PlayerHandle, Dictionary<ItemEquipmentSlot, ItemEquipmentStateComponent>>` outside
the Arch entity, while inventory and equipment-loadout state were already entity-backed. A focused
TDD verifier was first run in the RED state and failed because the required
`EquipmentStateCollectionComponent` did not exist.

The minimal implementation adds `EquipmentStateCollectionComponent` under
`src/Terraria.Dome.Simulation/Inventory/Components/` and attaches one instance to each player Arch
entity. `CommitItemEquipment`, `CommitItemUnequipment` and `CreateEquipmentSnapshot` now use that
component; the former `_equipmentStates` dictionary and its lifecycle cleanup are removed. The
component exposes a read-only state view while retaining controlled add/remove operations, owns the
monotonically increasing equipment revision, and rejects the `None` equipment slot invariant.

The verifier was deliberately extended with a revision-identity assertion; that second RED run failed
because the new component had no `Revision`, then the revision was moved into the component and the
player-handle `_equipmentRevisions` dictionary was removed. The focused verifier then passed and queried
the player entity directly after an accepted equip to
prove that the entity-owned collection and immutable snapshot contain the same state. Full serial
verification also passed with zero build warnings/errors:

```text
Terraria.Dome.Simulation build                  exit 0; 0 warnings; 0 errors
Terraria.Dome.Server build                      exit 0; 0 warnings; 0 errors
Items.Definitions.Verification                  exit 0
Items.Verification                              exit 0
Items.Loopback.Verification                     exit 0
Protocol.Compatibility.Verification             exit 0
Persistence.Verification                       exit 0
```

This batch remains **PARTIAL**. Version4 `Item.cs`, the 52 deferred members, 11 compatibility
members, artifact-boundary findings and full Terraria item parity remain outside the accepted scope.

# 2026-08-19 Main ECS migration Task 7 Meteor impact slice accepted

The accepted Meteor behavior is the deterministic, server-owned impact command, not the entire old
meteor scheduler. Version4 references are `Main.cs:13750-13764` and `13982-14016`,
`WorldGen.cs:5910-6030` and `6030-6245`, and the old TileSquare replication call at
`NetMessage.cs:6217`. The legacy random trigger, `Main.rand` ordering, meteor shower ambience,
announcement, and full terrain/framing special cases remain explicitly outside this slice.

`WorldMeteorImpactCommand` carries an explicit `(X, Y, Sequence)` intent. `DomeSimulation` is the
only authority entry point. `WorldMeteorImpactSystem` rejects invalid or unsafe bounds, active player
or NPC safety intersections, protected tile IDs, and a reached Meteorite cap. It emits deterministic
ordered `TileChangeCommand` values for an inner crater and outer Tile ID 37 Meteorite ring; the
existing `TileChangeCommitSystem` commits them during `CommitDomainCommands`. No Protocol or Server
code writes tiles directly.

The first WorldRules run was intentionally RED because the command and queue API did not exist. The
accepted current-source evidence is in
`Build/diagnostics/main-migration/task-7-meteor/20260819-025654/`:

```text
world-rules-final.txt
exit 0; deterministic tile set, section-version change, snapshot continuation and rejection paths passed

persistence-final.txt
exit 0; Meteorite tile and section state survived Dome persistence round-trip

world-rules-loopback-final.txt
exit 0; real TCP requested section contained the authoritative Meteorite Tile ID 37

protocol-compatibility-final.txt
exit 0; V1456 length and isolation contracts passed

full-client-bootstrap-final.txt
exit 0; full-client bootstrap completed without source-player authority replay

main-boundary-final.txt
exit 0; checked 353 Simulation source files, 0 forbidden dependencies

root-release-build-final.txt
exit 0; Terraria.Dome.sln Release build, 0 warnings, 0 errors
```

Task 7 remains **IN PROGRESS**. Remaining Meteor scheduler/shower behavior, random weather,
Slime Rain scheduling and NPC effects, announcements, UI, and client-authoritative operations still
require separate source, authority, behavior and evolution evidence.

# 2026-08-19 Main ECS migration Task 7 deterministic wind target slice accepted

The accepted Wind behavior is the server-owned target/current state transition extracted from
Version4 `Main.cs:12446-12579` (`ResetWindCounter` and `UpdateWeather`), with the existing V1456
WorldData target field referenced at `NetMessage.cs:258`. Random wind direction changes, extreme
wind counters, cloud updates, storm music and client visuals remain outside this slice.

`WorldWindChangeCommand` validates a finite target in `[-0.8, 0.8]`. The only authority path is
`DomeSimulation.TryQueueWorldWind`; `WorldWeatherSystem` applies the target in `ApplyWorldClock`
and advances `WindSpeedCurrent` with a deterministic per-tick smoothing step. Invalid, duplicate,
NaN and out-of-range requests are rejected without mutation. `WorldRuleState` carries both values,
and `LegacyWorldDataContext.WithWorldState` projects the target through the pre-existing
`Background.WindSpeedTarget` field.

Persistence is v11: target/current are appended after the v9 rain fields. The current-source
Persistence verifier proves v11 non-zero round-trip and rewrites a current payload to the v10 layout
to verify old snapshots default the new wind fields to zero while retaining rain and Slime Rain.
Fresh full-gate evidence is in
`Build/diagnostics/main-migration/task-7-wind/20260819-031500/`:

```text
world-rules-final.txt
exit 0; deterministic wind validation, smoothing, projection and snapshot continuation passed

persistence-final.txt
exit 0; v11 wind round-trip and v10 no-wind compatibility passed

world-rules-loopback-final.txt
exit 0; real TCP WorldData carried WindSpeedTarget and the existing Meteorite section replication

protocol-compatibility-final.txt
exit 0; V1456 length and isolation contracts passed

full-client-bootstrap-final.txt
exit 0; full-client bootstrap completed without source-player authority replay

main-boundary-final.txt
exit 0; checked 357 Simulation source files, 0 forbidden dependencies

root-release-build-final.txt
exit 0; Terraria.Dome.sln Release build, 0 warnings, 0 errors
```

Task 7 remains **IN PROGRESS**. Meteor scheduler/shower behavior, random weather and wind parity,
clouds, Lantern Night, NPC effects, announcements, UI and client-authoritative operations still
require separate migration slices.

# 2026-08-19 Main ECS migration Task 7 Lantern Night slice accepted

The accepted Lantern Night behavior is the smallest server-owned state machine supported by the
remaining source and wire evidence. Version4 references are `Main.cs:13392-13399`,
`Main.cs:13756-13767`, `Main.cs:13897-13898` and `Main.cs:13530`; the existing V1456 projection is
`NetMessage.cs:332`, where `bitsByte11[1]` carries the Lantern Night flag. The deleted `LanternNight`
class was not recovered, so its random eligibility, rewards, luck changes, NPC behavior and other
unknown logic remain explicitly unsupported.

`WorldEventKind.LanternNight` is accepted only through the night-time
`DomeSimulation.TryQueueWorldEvent` authority path. `WorldProgressionSystem` starts the event once,
rejects duplicates, and clears it on a daytime tick. `WorldProgressionState.IsLanternNight` is
projected through `LegacyWorldDataContext.EventFlags11` bit 1; Protocol and Server do not mutate
Simulation state.

Persistence is v12. The format appends one Lantern Night Boolean after `IsEclipse`; v1-v11 restore
`false`. The verifier proves v12 round-trip, v11 compatibility with Wind preserved and Lantern Night
cleared, and v10 compatibility with both Wind fields defaulted while Rain and Slime Rain remain
intact. Fresh full-gate evidence is in
`Build/diagnostics/main-migration/task-7-lantern-night/20260819-032916/`:

```text
world-rules-final.txt
exit 0; deterministic Lantern Night state, duplicate rejection, daytime cleanup and projection passed

persistence-final.txt
exit 0; v12 round-trip, v11 Lantern Night compatibility and v10 Wind/Lantern compatibility passed

world-rules-loopback-final.txt
exit 0; real TCP section replication and stable world entry checks passed

protocol-compatibility-final.txt
exit 0; V1456 packet-length and isolation contracts passed

full-client-bootstrap-final.txt
exit 0; full client bootstrap completed without source-player authority replay

main-boundary-final.txt
exit 0; checked 358 Simulation source files, 0 forbidden dependencies

root-release-build-final.txt
exit 0; Terraria.Dome.sln Release build, 0 warnings, 0 errors
```

Task 7 remains **IN PROGRESS**. Lantern Night random eligibility, rewards, NPC effects, announcements,
cloud/rain/wind coupling, music, client visuals, Meteor scheduling, random weather, and all
client-authoritative operations still require separate source-backed migration slices.

# 2026-08-19 Main ECS migration Task 7 Meteor schedule window slice accepted

The accepted Meteor behavior is the deterministic, server-owned pending window represented by the
remaining `WorldGen.spawnMeteor` source state, not automatic meteor probability or impact placement.
Version4 references are `Main.cs:13750-13764` for night initialization,
`Main.cs:13982-14016` for `HandleMeteorFall`, and `WorldGen.cs:4163` for the flag definition. The
legacy code clears the pending flag only after `time > 16200`; the `15000..16200` ambience path is
client presentation and remains outside Simulation.

`WorldMeteorScheduleCommand(sequence)` is accepted only at night for worlds with
`DefeatedEaterOrBrain`. Invalid, duplicate, same-tick duplicate, daytime and unqualified requests
are rejected. `WorldMeteorScheduleSystem` applies accepted requests during `ApplyWorldClock`, retains
the state at `TimeOfDay == 16200`, and clears it at `16201`. `IsMeteorScheduled` survives a snapshot
continuation. No Protocol field was added and no Protocol/Server code mutates Simulation state.

Persistence is v13. The format appends one `IsMeteorScheduled` Boolean after the existing progression
fields; v1-v12 default it to `false`. The verifier proves v13 round-trip, v12 compatibility with
Meteor schedule cleared, v11 compatibility with Wind preserved, and v10 compatibility with Wind and
Lantern Night cleared while retaining Rain and Slime Rain.

Fresh full-gate evidence is in
`Build/diagnostics/main-migration/task-7-meteor-schedule/20260819-034057/`:

```text
world-rules-final.txt
exit 0; deterministic schedule qualification, duplicate rejection, cutoff and snapshot continuation passed

persistence-final.txt
exit 0; v13 round-trip plus v12/v11/v10 compatibility passed

world-rules-loopback-final.txt
exit 0; real TCP section replication and stable world entry checks passed

protocol-compatibility-final.txt
exit 0; V1456 packet-length and isolation contracts passed

full-client-bootstrap-final.txt
exit 0; full client bootstrap completed without source-player authority replay

main-boundary-final.txt
exit 0; checked 360 Simulation source files, 0 forbidden dependencies

root-release-build-final.txt
exit 0; Terraria.Dome.sln Release build, 0 warnings, 0 errors
```

Task 7 remains **IN PROGRESS**. Automatic `Main.rand.Next(50)` scheduling, `WorldGen.dropMeteor`
candidate search and terrain rules, `StartMeteorShower`, ambience, announcements, extra TileSquare
projection, random weather parity and client-authoritative operations still require separate
source-backed migration slices.

# 2026-08-19 Main ECS migration Task 7 scheduled Meteor resolution slice accepted

This slice connects the accepted pending Meteor schedule to the existing deterministic
`WorldMeteorImpactSystem`. Version4 `Main.cs:13982-14016` consumes `WorldGen.spawnMeteor` after
`time > 16200`; the migration uses an explicit, already-authorized
`WorldMeteorImpactCommand(x, y, sequence)` as the resolution target so it does not call the deleted
legacy random stream or claim `WorldGen.dropMeteor` parity.

`TryQueueScheduledWorldMeteorImpact` accepts only when daytime is strictly past the source cutoff, a
pending Meteor schedule exists, and the impact passes the existing world-bound, entity-safety,
protected-Tile and Meteorite-cap checks. Cutoff-before, unscheduled, duplicate-resolution and direct
impact bypass paths are rejected. `ResolveScheduledWorldMeteorImpacts` clears pending state during
`ApplyWorldClock`, then reuses the existing deterministic `TileChangeCommand` commit path; the
focused verifier observes Meteorite Tile ID 37 and the cleared schedule after the tick.

Fresh full-gate evidence is in
`Build/diagnostics/main-migration/task-7-meteor-resolution/20260819-035804/`:

```text
world-rules-final.txt
exit 0; scheduled-resolution authority, cutoff rejection, duplicate rejection and TileChange commit passed

persistence-final.txt
exit 0; current v14 persistence and v13/v12/v11/v10 compatibility passed

world-rules-loopback-final.txt
exit 0; real TCP section replication and stable world entry checks passed

protocol-compatibility-final.txt
exit 0; V1456 packet-length and isolation contracts passed

full-client-bootstrap-final.txt
exit 0; full client bootstrap completed without source-player authority replay

main-boundary-final.txt
exit 0; checked 360 Simulation source files, 0 forbidden dependencies

root-release-build-final.txt
exit 0; Terraria.Dome.sln Release build, 0 warnings, 0 errors
```

Task 7 remains **IN PROGRESS**. Random meteor candidate search, terrain/liquid/framing parity,
Meteor showers, ambience, announcements, extra TileSquare projection, random weather and
client-authoritative operations remain unsupported.

# 2026-08-19 Main ECS migration Task 7 Lantern Night rain suppression slice accepted

This slice migrates the explicit Version4 interaction at `Main.cs:13433-13437`: when
`LanternNight.LanternsUp` is active, the weather update calls `StopRain()`. The scope is ordinary
Rain only; Slime Rain, cloud background, music, announcements and the deleted LanternNight
eligibility algorithm remain outside this slice.

When `WorldProgressionState.IsLanternNight` is active, `DomeSimulation.TryQueueWorldRain` rejects
new ordinary Rain requests. `WorldWeatherSystem.Advance` then normalizes existing Rain to
`RainTimeTicks=0` and `RainStrength=0` during `ApplyWorldClock`, while Wind and Slime Rain behavior
remain unchanged. Protocol and Server do not mutate world rules.

Fresh full-gate evidence is in
`Build/diagnostics/main-migration/task-7-lantern-rain-suppression/20260819-040231/`:

```text
world-rules-final.txt
exit 0; Lantern Night active-Rain clearing and Rain authority rejection passed with all world-rule regressions

persistence-final.txt
exit 0; current persistence and all historical compatibility fixtures passed

world-rules-loopback-final.txt
exit 0; real TCP section replication and stable world entry checks passed

protocol-compatibility-final.txt
exit 0; V1456 packet-length and isolation contracts passed

full-client-bootstrap-final.txt
exit 0; full client bootstrap completed without source-player authority replay

main-boundary-final.txt
exit 0; checked 360 Simulation source files, 0 forbidden dependencies

root-release-build-final.txt
exit 0; Terraria.Dome.sln Release build, 0 warnings, 0 errors
```

Task 7 remains **IN PROGRESS**. Slime Rain interaction, cloud background, StopRain presentation effects,
random event eligibility, Meteor random placement/showers,
random weather parity and client-authoritative operations still require separate source-backed slices.

# 2026-08-19 Main ECS migration Task 7 Wind rain coupling slice accepted

This slice migrates the deterministic rain-strength coupling from Version4 `Main.cs:12464-12479`.
The legacy transition computes `effectiveTarget = windSpeedTarget * (1f + 5f / 9f * maxRaining)`
before smoothing `WindSpeedCurrent`. The ECS implementation exposes
`WorldRuleState.AdvanceWind(ticksPerUpdate, rainStrength)` and has `WorldWeatherSystem` pass the
authoritative immutable `RainStrength`; `WindSpeedTarget` bounds and the existing WorldData
projection remain unchanged. No persistence or protocol field was added.

The focused verifier first reproduced RED when clear and rainy states produced identical wind
transitions, then passed GREEN after the coupling was implemented. It also covers determinism,
bounds and snapshot projection. Random direction changes, extreme-wind counters, cloud/background,
music, client visuals and the legacy random weather stream remain unsupported.

Fresh full-gate evidence is in
`Build/diagnostics/main-migration/task-7-wind-rain-coupling/20260819-041750/`:

```text
world-rules-final.txt
exit 0; deterministic weather, wind/rain coupling and all WorldRules regressions passed

persistence-final.txt
exit 0; world persistence round-trip, strict recovery and atomic replacement passed

world-rules-loopback-final.txt
exit 0; real TCP replication and stable world-entry projection checks passed

protocol-compatibility-final.txt
exit 0; V1456 length contracts and unsupported-wiring isolation passed

full-client-bootstrap-final.txt
exit 0; full client bootstrap completed without source-player authority replay

main-boundary-final.txt
exit 0; checked 361 Simulation source files, 0 forbidden dependencies

root-release-build-final.txt
exit 0; Terraria.Dome.sln Release build, 0 warnings, 0 errors
```

Task 7 remains **IN PROGRESS**. The next slice must obtain an independent oracle before implementing
random wind direction changes, extreme-wind counters, or Meteor random placement/showers.

# 2026-08-19 Main ECS migration Task 7 Slime Rain negative cooldown slice accepted

This slice migrates the explicit stop/cooldown behavior from Version4 `Main.cs:13366-13380` and
`Main.cs:13423-13428`. The legacy random cooldown generation remains unsupported; the server receives
an explicit, replayable `WorldSlimeRainStopCommand(cooldownTicks, sequence)`.

Only active Slime Rain accepts the stop command. `WorldProgressionSystem` commits
`SlimeRainTimeTicks=0` and the requested cooldown during `ApplyWorldClock`; each tick decrements
`SlimeRainCooldownTicks` by `TicksPerUpdate`, rejects new starts while positive, and permits a new
start at zero. Active duration and cooldown are mutually exclusive. No WorldData field was added.

Persistence is v15: one cooldown `Int32` is appended after existing progression fields; v1-v14
default it to zero. The verifier proves v15 cooldown round-trip and v14 compatibility preserving
Meteor schedule while defaulting cooldown, together with the existing v13/v12/v11/v10 layouts.

Fresh full-gate evidence is in
`Build/diagnostics/main-migration/task-7-slime-rain-cooldown/20260819-041221/`:

```text
world-rules-final.txt
exit 0; Slime Rain stop, cooldown decrement, restart gating and all world-rule regressions passed

persistence-final.txt
exit 0; v15 cooldown round-trip, v14 defaulting and historical compatibility passed

world-rules-loopback-final.txt
exit 0; real TCP section replication and stable world entry checks passed

protocol-compatibility-final.txt
exit 0; V1456 packet-length and isolation contracts passed

full-client-bootstrap-final.txt
exit 0; full client bootstrap completed without source-player authority replay

main-boundary-final.txt
exit 0; checked 361 Simulation source files, 0 forbidden dependencies

root-release-build-final.txt
exit 0; Terraria.Dome.sln Release build, 0 warnings, 0 errors
```

Task 7 remains **IN PROGRESS**. Random cooldown distributions, warning/announcement timing, automatic
Slime Rain starts, NPC waves, King Slime, cloud/weather parity, Meteor random placement/showers and
client-authoritative operations remain unsupported.

# 2026-08-19 Item ECS inventory entity ownership batch, partial

The next remaining Item ECS shadow was `_inventories`, a `PlayerHandle`-keyed dictionary that still
held the same `InventoryComponent` reference already attached to each player Arch entity. A focused
test was extended to replace the entity's inventory component with `World.Set`; the old implementation
correctly went RED by returning the stale dictionary object from `GetInventory`.

The implementation removes `_inventories` and resolves inventory directly from the player entity in
all Simulation paths: account import and synchronization, public inventory access, inventory and player
snapshots, selected-slot input, item use, chest transfer, drops, placement, equipment, prefixes,
variants, and world-item pickup. The test now passes and proves `GetInventory` observes an Arch
component replacement rather than a player-handle shadow.

The focused Items verifier, both affected Release builds, and the full Definitions, Items, Loopback,
Protocol Compatibility and Persistence matrix all passed with zero build warnings/errors. Overall
migration status remains **PARTIAL**.

As an additional completion gate, the repository-root `Terraria.Dome.sln` Release build was rerun
serially with `UseSharedCompilation=false` and `FixtureHostBuild=false`; all solution projects built
to `Build/bin` with exit 0, zero warnings and zero errors.

# 2026-08-19 Item ECS artifact boundary and repository regression gate

The Item ECS completion audit removed the remaining source-tree artifact boundary without deleting
user data: five exact, untracked `src/**/Build/obj` directories were moved intact into
`Build/obj/src-artifact-archive/`. A post-move Simulation Release build wrote to `Build/bin` and
confirmed zero files under `src/**/Build/(bin|obj|generated)`. The broader repository verifier also
exposed and accepted a focused PlayerGravitySystem correction: fall velocity is no longer clamped to
`-1`, so the existing eight-tick jump test reaches the ground through the collision system. Item
Definitions, Items, Loopback, Protocol Compatibility, Persistence, Completion and the broader Dome
verifier all pass; deferred Version4/UI members and complete Terraria parity keep the migration
`PARTIAL`.

The next Item ECS reconciliation slice corrected two stale member-map assignments. `favorited` was
already server-instance state, while `newAndShiny` was persisted in account/protocol models but was
silently discarded by Simulation import, snapshots and server projection. Both now belong to
`ItemInstanceStateComponent`; legacy account import, runtime account synchronization, Inventory and
Equipment V1456 frames, and world-item persistence retain them. Dome persistence format v14 appends
world-item `IsNewAndShiny`; a synthetic v13 payload verifies compatible defaulting. The corresponding
Item, Loopback and Persistence verifiers pass. Deferred count is now 50; overall status remains
`PARTIAL` for the explicitly excluded UI/client/full-parity scope.

The next mapping audit corrected six more stale `Deferred` assignments. `crit` and
`armorPenetration` are immutable `ItemCombatDefinition` fields, and the legacy mutually exclusive
`melee`, `magic`, `ranged` and `summon` flags are represented by the single validated
`ItemDamageClass` enum. `ItemDefinitionCompiler` now rejects undefined enum values in addition to
negative combat values; the Definitions verifier proves valid ranged metadata survives registration
and the invalid cases reject. The 237-entry member mapping remains index-identical to
`item-members.json`, while Deferred count falls to 44. Full Terraria parity remains `PARTIAL`.

The next Item equipment audit found that immutable `ItemEquipmentDefinition.Defense` was represented
in snapshots but never applied to the authoritative player combat state. `EquipmentStatSystem` now
recalculates each player's `DefenseComponent` after equipment commit from the player Arch entity's
equipment collection, inventory source slots and immutable definitions. The Items verifier was first
made RED for a head item with defense `5`; it now proves equip applies `5` and unequip returns to `0`.
Vanity, missing source stacks and mismatched definitions contribute no defense. This completes the
currently defined equipment defense route; `lifeRegen` and other remaining item behavior still require
separate design-backed slices, so migration status stays `PARTIAL`.

The following mapping-only audit reconciled three stale deferred assignments with existing server
contracts. `buffTime` is `ItemRecoveryDefinition.BuffDurationTicks`, `active` is represented by
empty `ItemStackComponent` and `ItemWorldStateComponent.IsActive` invariants, and `OriginalRarity`
is retained by immutable `ItemDefinition.Rarity`. Existing Definitions, Items and persistence
evidence already covered these contracts, so no runtime code or behavior changed. The 237-entry
mapping remains index-identical, Deferred count falls from 44 to 41, and the migration remains
`PARTIAL` for the remaining deferred/compatibility and full-parity boundaries.

The current source tree also carries a v15 Dome persistence extension for Slime Rain cooldown after
the v14 item `IsNewAndShiny` field. Item evidence now records v15 as current, v14 as the compatible
pre-cooldown layout, and versions 1-14 as readable; this status correction does not change the Item
runtime boundary or the migration's `PARTIAL` state.

# 2026-08-19 Main ECS migration Task 7 Slime Rain warning timer slice accepted

This slice migrates the deterministic warning countdown from Version4 `Main.cs:14019-14038`, with
its start/stop initialization at `Main.cs:13341-13363` and `Main.cs:13366-13385`. The legacy delay is
`slimeWarningDelay = 420`; each update decrements a positive timer once, and expiry observes whether
Slime Rain is currently active. `WorldSlimeRainStartCommand` and `WorldSlimeRainStopCommand` now carry
an explicit `Announce` flag. `WorldProgressionSystem` owns the countdown in `ApplyWorldClock`, while
`DomeSimulation` emits one `WorldSlimeRainWarningEvent(IsSlimeRaining)` on the positive-to-zero edge.

The focused verifier first went RED because the command overload, warning state and event API did not
exist. GREEN proves announced start and stop warnings, active and inactive expiry events, single
emission, same-tick initial decrement, and `announce=false` suppression. The warning countdown is
transient and is intentionally omitted from v15 persistence; the Persistence verifier proves the
Slime Rain duration survives round-trip while warning defaults to zero after reload. No WorldData or
V1456 packet field was added.

Fresh full-gate evidence is in
`Build/diagnostics/main-migration/task-7-slime-rain-warning/20260819-043037/`:

```text
world-rules-red.txt
exit 1; expected compile failures for the missing warning command/state/event API

world-rules-final.txt
exit 0; warning countdown, active/inactive expiry events and all WorldRules regressions passed

persistence-final.txt
exit 0; strict persistence, historical compatibility and transient-warning contract passed

world-rules-loopback-final.txt
exit 0; real TCP replication and stable world-entry projection checks passed

protocol-compatibility-final.txt
exit 0; V1456 length contracts and unsupported-wiring isolation passed

full-client-bootstrap-final.txt
exit 0; full client bootstrap completed without source-player authority replay

main-boundary-final.txt
exit 0; checked 362 Simulation source files, 0 forbidden dependencies

root-release-build-final.txt
exit 0; Terraria.Dome.sln Release build, 0 warnings, 0 errors
```

Task 7 remains **IN PROGRESS**. Localized `Lang.gen[74/75]` text, colors and ChatHelper client
broadcast remain unsupported because the current Dome has no accepted server-to-client chat output
contract. Automatic Slime Rain starts, NPC waves, King Slime and Meteor random placement/showers still
require separate source-backed slices.

The next Item ECS behavior slice closes the server-authoritative `lifeRegen` gap. Version4 accumulates
legacy `lifeRegen` units into `lifeRegenCount` and heals once per `120` units; the current Dome slice
retains its verified default rate as `60` units per tick and adds immutable
`ItemEquipmentDefinition.LifeRegen` contributions during the existing entity-owned equipment-stat
recalculation. The Items verifier was first RED because a type 4 equipped item did not accelerate
recovery; it now proves the `LifeRegen: 60` contribution after damage and its removal on unequip.
Definitions, Items, Combat, Loopback, Protocol Compatibility, Persistence, WorldRules, Dome and the
solution Release build all passed with zero warnings/errors. The member map remains index-identical,
Deferred count falls from 41 to 40, and the migration remains `PARTIAL`.

The fresh Persistence regression closes the restore boundary for this behavior slice without
expanding the save format: `DomeSimulationSnapshot` still persists account and inventory values,
while online equipment remains connection-owned ECS state. After loading the v15 snapshot, the
verifier recreates a player from the restored account, equips its persisted type 4 item, and observes
`HealthRegenerationComponent.EquipmentRegenUnitsPerTick == 60` after the authoritative equipment
commit. Definitions, Items, Combat, Items Loopback, Protocol Compatibility, Persistence, WorldRules,
Dome, Simulation, Server and solution Release checks were rerun; all exited `0`, with builds at
zero warnings and zero errors. Migration status remains `PARTIAL` with 40 Deferred and 11
Compatibility members, and the Version4 `Item.cs` baseline remains retained.

# 2026-08-19 Item ECS inventory exploit-boundary slice accepted

Version4 `Item.FixAgainstExploit` at `Item.cs:48633-48650` is now represented by the server-owned
`ItemInventorySanitizationSystem`. During account restoration and persistence synchronization, the
system sanitizes reconstructed/projected runtime slots by clearing unknown or empty entries, clamping
quantities to the immutable item definition's stack limit, and resetting prefixes that the definition
does not authorize. Non-hotbar account slots remain preserved by the existing persistence contract.
The Persistence verifier was
first made RED with a restored type 4 item carrying quantity `99` and prefix `7`; GREEN proves the
runtime inventory exposes quantity `1` and prefix `0` before equipment commit. This is deliberately
an inventory trust-boundary slice and does not add a new persistence field or change world-item
presentation behavior.

Definitions, Items, Persistence, Items Loopback, Protocol Compatibility, WorldRules, Dome,
Simulation, Server and solution Release checks remain the required verification surface. The member
map remains index-identical with 39 Deferred and 11 Compatibility assignments; full Version4 Item
parity and client/UI behavior remain `PARTIAL`, and the legacy `Version4/Item.cs` baseline remains
retained.

# 2026-08-19 Item ECS immutable price-conversion slice accepted

The pure Version4 item-economy calculation is now represented by `ItemPriceSystem`, without moving
NPC shop or client presentation responsibilities into Simulation. `buyPrice` converts platinum,
gold, silver and copper to the legacy copper-unit value; `sellPrice` is exactly five times that value.
The system retains the four unit constants and the seven static legacy sell-price presets used by
Item defaults, with negative and overflowing inputs rejected rather than silently wrapping. The
Definitions verifier was RED until `ItemPriceSystem` existed; GREEN proves the mixed-denomination
formula, all seven presets, and invalid-input rejection. Definitions, Items, Persistence, Items
Loopback, Protocol Compatibility, Simulation, Server and solution Release all passed, with the three
builds reporting zero warnings and zero errors.

The member map remains index-identical with 28 Deferred and 10 Compatibility assignments. NPC shop
transactions, special currencies, prices in UI and the remaining client-specific Item behavior stay
outside this server-authoritative slice; full Version4 Item parity remains `PARTIAL` and the legacy
`Version4/Item.cs` baseline remains retained.

# 2026-08-19 Main ECS migration Task 7 Lantern Night same-tick Rain ordering slice accepted

This slice migrates the ordering visible in Version4 `Main.cs:13392-13438`: `UpdateTime` checks
`LanternNight.LanternsUp` while processing Rain before the later `LanternNight.UpdateTime` call at
`Main.cs:13530`. A pending, already-authorized Lantern Night intent therefore participates in the
current weather authority. `DomeSimulation` now passes current-or-pending Lantern Night to
`WorldWeatherSystem`, so existing Rain clears immediately and a same-tick ordinary Rain start does
not win. Progression submission remains in `WorldProgressionSystem.ApplyWorldClock`.

The focused verifier first went RED: the old ECS path reduced existing Rain from 3 to 2 and accepted
the Rain/Lantern start conflict. GREEN proves existing Rain suppression and same-tick Rain-start
rejection while preserving the existing Lantern Night state machine. No persistence, WorldData or
V1456 protocol field changed.

Fresh full-gate evidence is in
`Build/diagnostics/main-migration/task-7-lantern-same-tick-ordering/20260819-044056/`:

```text
world-rules-red.txt
runtime failure; old path retained Rain and accepted the same-tick conflict

world-rules-final.txt
exit 0; same-tick Rain ordering and all WorldRules regressions passed

persistence-final.txt
exit 0; world persistence round-trip, strict recovery and historical compatibility passed

world-rules-loopback-final.txt
exit 0; real TCP replication and stable world-entry projection checks passed

protocol-compatibility-final.txt
exit 0; V1456 length contracts and unsupported-wiring isolation passed

full-client-bootstrap-final.txt
exit 0; full client bootstrap completed without source-player authority replay

main-boundary-final.txt
exit 0; checked 362 Simulation source files, 0 forbidden dependencies

root-release-build-final.txt
exit 0; Terraria.Dome.sln Release build, 0 warnings, 0 errors
```

Task 7 remains **IN PROGRESS**. Lantern Night random eligibility, `NextNightIsLanternNight`,
cooldown, Genuine/Manual persistence, cloud/music effects, client `NetMessage.SendData(7)`, the
deleted `NaturalAttempt`, automatic Slime Rain starts, NPC waves, King Slime and Meteor random
placement/showers remain unsupported and require separate source-backed slices.

# 2026-08-19 Main ECS migration Task 7 Lantern Night schedule state slice accepted

Source: complete archive `Terraria.GameContent.Events/LanternNight.cs:91-116` establishes that
`NextNightIsLanternNight` is consumed once during a startable night, clears on consumption and starts
the genuine event; complete archive `Terraria/NPC.cs:79968-79984` establishes that first-cleared NPC
events set it; Version4 `Terraria.IO/WorldFile.cs:175-178`, `1093-1096`, `1419-1422` and
`2370-2382` establish world persistence. The three source hashes, sizes and timestamps are frozen in
`Build/diagnostics/main-migration/task-7-lantern-schedule/20260819-050007/source-manifest.txt`.

Proposition: a server-authorized valid schedule command records
`WorldProgressionState.IsNextNightLanternNight` during daytime; it is consumed exactly once by
`WorldProgressionSystem` on an eligible night, yielding active Lantern Night and no remaining
schedule. Duplicate/negative commands reject. v16 retains the state and v15 defaults it false without
altering V1456 WorldData layout.

Authority: `WorldLanternNightScheduleCommand(Sequence)` ->
`DomeSimulation.TryQueueWorldLanternNightSchedule` validation/private queue ->
`WorldProgressionSystem.TryScheduleLanternNight` and `TryStartScheduledLanternNight` -> immutable
`WorldProgressionState` snapshot. The command has no `MessageBuffer`, V1456 dispatcher or Server
request call site; it is not a client input path.

RED: `world-rules-red.txt` exit 1 after verifier corrected its clock-fixture parameter names; missing
command, queue API and snapshot state were the remaining compiler errors. `persistence-red.txt` exit
nonzero because the v15 writer discarded the new state.

GREEN: `world-rules-final.txt` exit 0; it proves daytime retention, night-only one-time consumption,
no later-night restart and duplicate/negative command rejection.

Persistence: `DomeStatePersistenceFormat` is v16. It appends the schedule Boolean after v15 Slime
Rain cooldown; V15 and older default it false. `persistence-final.txt` exit 0 proves v16 round-trip,
v15 strict-layout recovery and existing v10-v14 layout fixtures. The strict trailing-data rejection
remains enabled.

Loopback: `world-rules-loopback-final.txt` exit 0. Protocol:
`protocol-compatibility-final.txt` exit 0; no WorldData field or V1456 length changed. Bootstrap:
`full-client-bootstrap-final.txt` exit 0. Boundary: `main-boundary-final.txt` exit 0, 366 Simulation
files checked, 0 violations. Release:
`root-release-build-final.txt` exit 0, `Terraria.Dome.sln` Release, 0 warnings and 0 errors.

Accepted boundary: server-owned Lantern Night schedule state, deterministic phase consumption,
persistence and historical format compatibility. Deferred/Unknown: the NPC first-event hook,
`NaturalAttempt` random/eligibility/cooldown, genuine/manual behavior, rewards, NPC effects, chat,
music, cloud and client `NetMessage.SendData(7)` remain outside this slice. Task 7 remains
**IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 9 DamageVar pure contract accepted

Source: Version4 `Terraria/Main.cs:14639-14675`, confirmed against complete archive
`Terraria/Main.cs:67025-67061`; known Projectile callers are `Projectile.cs:11877` and `13238`.
Source hashes and call-site evidence are frozen in
`Build/diagnostics/main-migration/task-9-damage-var/20260819-123700/source-manifest.txt`.

Proposition: `DamageVariationSystem` preserves the old pure algorithm: one base variation in the
default path; a chance read followed by one extra variation for positive or negative luck; max/min
selection respectively; `Math.Round` to the final integer; and zero random consumption with direct
truncation when variation is disabled. `IDamageVariationRandom` is caller-owned and exposes only the
two required random operations.

Authority decision: this is a pure contract only. The current Dome `_worldSeed` is fixed and is not a
combat stream; existing random implementations serve loot or world generation and no source-backed
player luck component or persisted combat random cursor exists. Therefore the system is not attached
to projectile/player tick paths, no PvP or protocol field is invented, and no missing luck/random state
is defaulted.

RED: `npc-damage-var-red.txt` exit `1` for the missing random interface. GREEN:
`npc-damage-var-green.txt` exit `0`; scripted random fixtures verified base, positive luck, negative
luck and disabled branches. Final NPC, Combat, Persistence, loopback, Protocol, bootstrap,
MainBoundary and root Release gates are all exit `0` under the same evidence directory. MainBoundary
reports 388 Simulation files and zero violations; root Release reports 0 warnings and 0 errors.

Accepted boundary: exact pure `DamageVar` algorithm and explicit random input interface. Deferred /
Unknown: combat random stream ownership and persistence, old `Main.rand` sequence parity, player luck
authority, Projectile DamageVar integration, NoDamageVar server configuration authority, and the
complete critical-hit/hit-effect pipeline. Task 9 remains **IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 9 moon phase and gameplay day classification accepted

Source: Version4 `Terraria/Main.cs:1924-1929` (`GetMoonPhase`) and `12864-12873` (`IsItDay`),
`Terraria.Enums/MoonPhase.cs`, and `Main.cs:13921-13924` dawn rollover. Source hashes and searches
are frozen in
`Build/diagnostics/main-migration/task-9-moon-phase/20260819-131056/source-manifest.txt`.

Proposition: `WorldMoonPhase` preserves legacy numeric order for every valid `0..7` value;
`WorldTimeClassificationSystem.GetMoonPhase` rejects anything else. `IsGameplayDayTime` returns
`isDayTime` only outside a remix world and always false in remix worlds, exactly as legacy `IsItDay`.

Authority: both are pure input classifications. Current `WorldClockSnapshot` does not own a moon
phase; `WorldMetadata.SeedVariant` is not yet a source-backed remix-world interpretation. The new
system does not mutate or read the clock, does not infer a flag from a string, and does not attach to
existing NPC/Player/event systems.

RED: `npc-moon-phase-red.txt` exit `1` only for missing classification symbols. GREEN:
`npc-moon-phase-final.txt` exit `0`, including enum ordering, invalid phase rejection, normal day/night
and remix override. WorldRules, Persistence, TCP loopback, Protocol compatibility and full bootstrap
all exit `0`; MainBoundary reports 390 files and zero violations. Serial root Release exits `0` with
0 warnings and 0 errors.

Accepted boundary: pure moon phase projection and legacy gameplay-day classification. Deferred /
Unknown: persisted moon phase, dawn rollover, world-file or wire projection, explicit remix metadata
authority, and all actual NPC/Player behavior integrations. Task 9 remains **IN PROGRESS**.

Next action: establish a source-backed `NpcGameEventClearedCommand` authority route from the complete
`NPC.OnGameEventClearedForTheFirstTime` oracle to the accepted Lantern Night schedule command, without
implementing NaturalAttempt randomness.

# 2026-08-19 Main ECS migration Task 7 NPC first-event Lantern Night authority slice accepted

Source: complete archive `Terraria/NPC.cs:79956-79966` proves that a first-time handler runs only
after an event flag transitions false -> true. `NPC.cs:79968-79994` then schedules Lantern Night for
all switch paths except `gameEventId` 4, 21 and 22. Its SHA-256 is frozen in
`Build/diagnostics/main-migration/task-7-npc-first-event-lantern-schedule/source-manifest.txt`.

Proposition: an upstream-proven `NpcGameEventFirstClearCommand(gameEventId, sequence)` schedules
next-night Lantern Night once for eligible nonnegative IDs; duplicate/negative commands reject and
legacy exceptions 4/21/22 do not mutate the schedule.

Authority: `NpcGameEventFirstClearCommand` ->
`DomeSimulation.TryQueueNpcGameEventFirstClear` -> private
`TryQueueWorldLanternNightSchedule` -> existing `WorldProgressionSystem` schedule/consume state
machine -> immutable `WorldProgressionState`. The generic schedule queue API is private; search finds
no Protocol, MessageBuffer or Server request caller for the public NPC command entry.

RED: `world-rules-red.txt` exit 1, expected missing NPC command and authority API compile errors.
GREEN: `world-rules-final.txt` exit 0; it proves eligible ID 1 scheduling, excluded ID 4 rejection,
duplicate and negative validation, plus all previous world-rule regressions.

Persistence: unchanged v16 schedule state from the preceding slice; `persistence-final.txt` exit 0.
Loopback: `world-rules-loopback-final.txt` exit 0. Protocol:
`protocol-compatibility-final.txt` exit 0, no packet layout change. Bootstrap:
`full-client-bootstrap-final.txt` exit 0. Boundary: `main-boundary-final.txt` exit 0, 369 files, 0
violations. Release: `root-release-build-final.txt` exit 0, `Terraria.Dome.sln` Release with 0 warnings
and 0 errors.

Accepted boundary: a typed server-only NPC first-event command is the only public entry to Lantern
Night schedule submission and preserves legacy switch exceptions. Deferred/Unknown: no actual ECS NPC
death/event-flag lifecycle emits this command yet; boss flags, credits roll, Plantera bulbs,
dual-dungeon walls, NaturalAttempt random eligibility/cooldown, rewards, client effects and chat remain
unmigrated. Task 7 remains **IN PROGRESS**.

Next action: inspect the existing `NpcDeathSystem` source and Version4 NPC death/event flag call sites;
only create a producer mapping when a specific supported NPC death has a source-backed legacy event ID
and durable first-clear state contract.

# 2026-08-19 NPC first-event producer audit

The requested follow-up audit found no valid current ECS death -> game-event producer mapping.
`NpcDeathSystem` accepts only handle/health/activity/position/loot-table input, and its
`_npcDeathSystem` / `_pendingNpcDeaths` fields currently have no `DomeSimulation` production or
consumption path. Current `NpcDefinition` samples lack a source-backed legacy NPC type ->
`SetEventFlagCleared` game-event ID map. Version4 `NPC.cs:80441-80718` requires exactly those legacy
type, boss-condition and prior-event-flag facts.

No code was added from this audit. `NpcGameEventFirstClearCommand` remains a typed server authority
contract, not an assertion that generic NPC damage/death has been migrated. The next Task 7 candidate
is the complete `LanternNight.NaturalAttempt` cooldown/eligibility branch, scoped first to an
independent state/command proposition before any random or boss-related logic.

# 2026-08-19 Item ECS fresh Task 9 verification refresh

The Item ECS verification matrix was rerun from the repository root: Definitions, Items, Items
Loopback, Protocol Compatibility and Persistence verifiers all exited `0`. The Simulation and Server
Release builds also exited `0` with zero warnings and errors. The Task 8 source audit found zero
`Terraria.Item`, `Main.item`, `Player.inventory`, `ContentSamples` or `ItemID.Sets` references under
`src/Terraria.Dome.Simulation`, and the source artifact audit found zero `bin`, `obj` or `generated`
paths beneath `src`.

The root solution requires an additional serial MSBuild constraint in this workspace:
`dotnet build Terraria.Dome.sln -c Release -m:1 -p:UseSharedCompilation=false
-p:FixtureHostBuild=false`. Without `-m:1`, projects race through the shared repository `Build/bin`
output root and WorldRules can resolve a stale Simulation reference assembly. The non-serial run
reproduced eight missing Lantern Night API compile errors; the `-m:1` rerun built the complete
solution with exit `0`, zero warnings and zero errors without source changes.

This updates evidence only. The Item ECS migration remains **PARTIAL**: the 28 Deferred and 10
Compatibility members retain their documented boundaries, Version4 `Terraria.Item.cs` remains
read-only, and full Terraria item parity is not claimed.

# 2026-08-19 Main ECS migration Task 7 Lantern Night cooldown state slice accepted

Source: complete archive `Terraria.GameContent.Events/LanternNight.cs:91-115` proves that a positive
`LanternNightsOnCooldown` is decremented only after `LanternsCanStart()` succeeds, then random natural
eligibility and random cooldown initialization occur. Version4 `Terraria.IO/WorldFile.cs:1419-1422`
and `2370-2382` prove the persisted Int32 and zero default for older world versions. Frozen source
metadata is in `Build/diagnostics/main-migration/task-7-lantern-cooldown/20260819-113103/source-manifest.txt`.

Proposition: `WorldProgressionState.LanternNightCooldownTicks` is nonnegative state; v17 round-trip
retains it and v16/v15/older layouts default it to zero without changing V1456 wire fields.

Authority: value-only progression snapshot -> `DomeStatePersistenceFormat` v17 append/read; all
existing `WorldProgressionState.With...` methods preserve the value. No random or NPC producer was
invented for this state.

RED: `persistence-red.txt` exit 1; missing cooldown constructor parameter and property were the only
new contract failures.

GREEN: `world-rules-final.txt` exit 0 and `persistence-final.txt` exit 0. Persistence proves v17
round-trip, v16 compatibility default zero, and strict v10-v16 layout recovery.

Loopback: `world-rules-loopback-final.txt` exit 0. Protocol:
`protocol-compatibility-final.txt` exit 0; no WorldData or V1456 packet layout changed. Bootstrap:
`full-client-bootstrap-final.txt` exit 0. Boundary: `main-boundary-final.txt` exit 0, 370 files,
0 violations. Release: `root-release-build-final.txt` exit 0, 0 warnings and 0 errors.

Accepted boundary: explicit nonnegative cooldown state, v17 persistence and historical defaults.
Deferred/Unknown: cooldown producer, decrement phase, `LanternsCanStart` eligibility, random
`Next(14)`/`Next(5,11)`, Boss/MoonLord/Pumpkin/Snow conditions, genuine/manual effects and client
presentation. Task 7 remains **IN PROGRESS**.

Next action: establish a RED for one deterministic cooldown-attempt phase only after an explicit
server eligibility contract exists; do not decrement per tick or infer random parity from this slice.

# 2026-08-19 Main ECS migration Task 7 Lantern Night eligibility contract slice accepted

Source: complete archive `Terraria.GameContent.Events/LanternNight.cs:54-89`. `LanternsCanPersist`
requires nighttime plus `LanternsCanStart`; the latter rejects scheduled Meteor, Blood Moon, Pumpkin
Moon, Snow Moon, nonzero invasion, nonzero Moon Lord countdown, active boss and active legacy NPC types
13 through 15. Source hash/line range is frozen in
`Build/diagnostics/main-migration/task-7-lantern-eligibility/20260819-114525/source-manifest.txt`.

Proposition: immutable `LanternNightEligibilitySnapshot` plus existing progression state produces the
same pure start/persist decision for every archive guard, including inactive-boss acceptance and daytime
persist rejection.

Authority: `LanternNightEligibilitySnapshot` is an explicit input value; pure
`LanternNightEligibilitySystem` owns the predicate. It is not attached to `DomeSimulation.Tick`, not
persisted, not projected and does not synthesize unowned Pumpkin/Snow/MoonLord/Boss values.

RED: `world-rules-red.txt` exit 1 due to missing eligibility types. GREEN:
`world-rules-final.txt` exit 0, covers all seven guards, active/inactive boss and day/night persist.
Persistence, loopback, protocol compatibility and full-client-bootstrap all exit 0. MainBoundary:
`main-boundary-final.txt` exit 0, 373 files and 0 violations. Root Release: exit 0, 0 warnings and 0
errors.

Accepted boundary: exact pure archive eligibility rule. Deferred/Unknown: authoritative state producers
for Pumpkin/Snow Moon, Moon Lord countdown and active boss snapshots; phase placement, cooldown
decrement, random attempt and automatic Lantern Night start. Task 7 remains **IN PROGRESS**.

Next action: audit current NPC store/components for a source-backed boss-active snapshot. Do not attach
the eligibility system to the tick until all non-NPC inputs also have explicit authority owners.

# 2026-08-19 Lantern Night eligibility input authority audit

Current `DomeSimulation` NPC state can expose lifecycle active status plus DefinitionId/NetId, but
`NpcDefinition` has no `IsBoss` member and the only registered definitions are fixture/chaser/town
samples without source-backed legacy type 13-15 or boss mapping. Pumpkin Moon, Snow Moon and Moon Lord
countdown have no Simulation state, command or system. Only Meteor/Blood Moon/Invasion are already
authoritative progression state.

No NaturalAttempt tick integration was made. Defaulting absent boss/Pumpkin/Snow/MoonLord inputs would
contradict the archive eligibility guard. The next independent slice is persisted Manual/Genuine Lantern
state from archive `LanternNight` / Version4 `WorldFile`, not cooldown decrement or random eligibility.

# 2026-08-19 Lantern Night Manual/Genuine authority gate

The dedicated source recovery gate is `MANUAL_AUTHORITY_UNPROVEN`, not an implementation failure.
Fresh evidence is under
`Build/diagnostics/main-migration/task-7-lantern-manual-genuine/20260819-120052/`.

Complete archive `Terraria.GameContent.Events/LanternNight.cs:119-134` proves that
`ToggleManualLanterns` mutates `ManualLanterns` on the non-client runtime and conditionally sends
WorldData. A complete archive caller search has no caller other than this method definition; Version4
has no `ToggleManualLanterns` match. `MessageBuffer.cs:475-590` proves that WorldData's manual bit is
only read in the `Main.netMode == 1` client-consumption path. It is not a client-to-server request
authority. `NetMessage.cs:334-340` projects effective `LanternsUp`, while Version4
`WorldFile.cs:1419-1422` and `2370-2382` separately save/restore Genuine, Manual and next-night state.

No Manual toggle command, Protocol dispatcher, persistence v18 field or mapping of current v17
`IsLanternNight` to Manual/Genuine was added. The existing effective state is used by generic world
event requests, the accepted schedule path, Rain suppression, persistence and V1456 projection, but
has no source-backed mode provenance. Task 7 remains **IN PROGRESS**. The completed authority gate is
recorded in `docs/plans/2026-08-19-lantern-night-manual-genuine-migration-execution.md`; future work
must either recover a server producer or make an explicit compatibility architecture decision.

# 2026-08-19 Main ECS migration Task 7 normal-event eligibility contract slice accepted

Source: Version4 `Terraria/Main.cs:13724-13733`, with the identical complete archive method at
`Terraria/Main.cs:66094-66103`. In `UpdateTime`, its return value initializes `stopEvents` before
ordinary world-event start branches. Source SHA-256 and the current no-implementation search are frozen
in `Build/diagnostics/main-migration/task-7-normal-event-eligibility/20260819-120353/`.

Proposition: the immutable `NormalEventEligibilitySnapshot` and an explicit effective Lantern Night
value return `false` only when Lunar Apocalypse is inactive, no legacy NPC type 398 is active, Moon
Lord countdown is zero and Lantern Night is inactive. Each one of Lantern Night, Lunar Apocalypse,
type 398 and a positive countdown independently blocks ordinary event starts. A negative countdown is
rejected.

Authority: `NormalEventEligibilitySnapshot` is an explicit pure input;
`NormalEventEligibilitySystem.ShouldBlockNormalEvents` owns the exact legacy predicate. It is not
attached to `DomeSimulation.Tick`, persisted, projected, or populated with invented defaults. Current
Simulation has no authoritative producers for Lunar Apocalypse, active type 398 or Moon Lord countdown.

RED: `world-rules-red.txt` exit `1`, with only the expected missing
`NormalEventEligibilitySnapshot` and `NormalEventEligibilitySystem` compile errors. GREEN:
`world-rules-final.txt` exit `0`, including the complete predicate truth table and existing WorldRules
regressions.

Persistence: no format change; `persistence-final.txt` exit `0`. Loopback:
`world-rules-loopback-final.txt` exit `0`. Protocol: `protocol-compatibility-final.txt` exit `0`, no
wire-layout change. Bootstrap: `full-client-bootstrap-final.txt` exit `0`. Boundary:
`main-boundary-final.txt` exit `0`, 381 Simulation files and 0 violations. Release:
`root-release-build-final.txt` exit `0`, `Terraria.Dome.sln` Release with 0 warnings and 0 errors.

Accepted boundary: exact pure `ShouldNormalEventsBeAbleToStart` decision contract. Deferred/Unknown:
authoritative producers for its three non-Lantern inputs; phase integration into ordinary event
scheduling; all random event selection and presentation effects. Task 7 remains **IN PROGRESS**.

Next action: inspect Version4 `UpdateTime` branches that consume `stopEvents`, then select one
deterministic, source-backed state transition whose input authority exists or can be isolated as a pure
contract without setting missing world facts to defaults.

# 2026-08-19 Main ECS migration Task 7 King Slime readiness contract slice accepted

Source: Version4 `Terraria/Main.cs:13710-13722`, with the same complete archive implementation at
`Terraria/Main.cs:66055-66067`. Version4 consumers are the ordinary event branch at
`Main.cs:13508-13513` and remix NPC death logic at `Terraria/NPC.cs:65681-65684`. Source hashes,
callers and current no-implementation search are frozen in
`Build/diagnostics/main-migration/task-7-king-slime-readiness/20260819-121052/`.

Proposition: `KingSlimeReadinessSystem.HasReadyPlayer` returns true exactly when at least one active
immutable player snapshot has maximum health strictly greater than 140 and defense strictly greater
than 8. Empty, inactive, `140/9` and `141/8` snapshots reject; `141/9` accepts. Negative health or
defense snapshot values reject.

Authority: `KingSlimePlayerSnapshot` is a pure input contract and
`KingSlimeReadinessSystem` owns the predicate. It is not attached to current `DomeSimulation` player
state, persisted, projected, or allowed to consume a mutable Protocol player slot. Current Simulation
has lifecycle and health components, but no authoritative source-backed equivalent of legacy
`Player.statDefense`; it cannot yet construct this snapshot without a separate equipment-stat authority
slice.

RED: `world-rules-red.txt` exit `1`, only missing `KingSlimeReadinessSystem` and
`KingSlimePlayerSnapshot` compile errors. GREEN: `world-rules-final.txt` exit `0`, including all
strict-threshold, inactive and invalid-value cases plus the preceding WorldRules contracts.

Persistence: no format change; `persistence-final.txt` exit `0`. Loopback:
`world-rules-loopback-final.txt` exit `0`. Protocol: `protocol-compatibility-final.txt` exit `0`, no
wire-layout change. Bootstrap: `full-client-bootstrap-final.txt` exit `0`. Boundary:
`main-boundary-final.txt` exit `0`, 383 Simulation files and 0 violations. Release:
`root-release-build-final.txt` exit `0`, `Terraria.Dome.sln` Release with 0 warnings and 0 errors.

Accepted boundary: exact pure `AnyPlayerReadyToFightKingSlime` threshold contract. Deferred/Unknown:
the source-backed authoritative maximum-health/defense snapshot producer; ordinary King Slime random
scheduling; remix NPC-death spawn, boss state and client presentation. Task 7 remains **IN PROGRESS**.

Next action: audit the existing player equipment-stat output and persistence/snapshot path for whether
it represents the legacy server `statDefense` contract. Only connect King Slime readiness if the exact
maximum-health and defense facts can be produced without reading Protocol/client state directly.

# 2026-08-19 King Slime player-stat authority audit

`Build/diagnostics/main-migration/task-7-king-slime-readiness/20260819-121052/`
`player-stat-authority-audit.txt` records the source/current comparison. `DomeSimulation` imports
`PlayerPersistentState.MaximumLife` into `HealthComponent.Maximum` and owns lifecycle activity;
`RecalculateEquipmentStats` calls `EquipmentStatSystem`, which assigns `DefenseComponent.Value` from
the sum of supported non-vanity equipment definitions. These are usable current Dome facts.

They are not complete source-backed legacy `Player.statDefense`: Version4 `Player.cs` starts from
zero then applies buffs, armor/accessory effects, prefixes, consumables, stance effects and other
modifiers before clamping nonnegative. Current `EquipmentStatSystem` has no corresponding buff/prefix/
stance terms. Do not connect `KingSlimeReadinessSystem` to ordinary random scheduling or NPC death
spawn until a full player-stat authority contract is migrated. This leaves Task 7 **IN PROGRESS** and
does not invalidate the accepted pure predicate.

# 2026-08-19 Main ECS migration Task 9 damage calculation slice accepted

Source: Version4 `Terraria/Main.cs:14670-14712`, confirmed against the complete archive at
`Terraria/Main.cs:67056-67098`. `CalculateDamageNPCsTake` and
`CalculateDamagePlayersTakeInPVP` use `max(1, damage - defense * 0.5)`; normal player damage uses
the same rule, Expert uses `0.75` defense and Master uses full defense. Version4 Player/NPC callers
perform the final integer health mutation after the Main calculation. Provenance, caller searches and
source hashes are frozen in
`Build/diagnostics/main-migration/task-9-damage-calculation/20260819-122505/source-manifest.txt`.

Proposition: `DamageCalculationSystem` returns the legacy double result and independently maps it to
the current authoritative integer health mutation by truncating toward zero. Therefore `20/5` yields
NPC/normal/PvP `17.5 -> 17`, Expert `16.25 -> 16`, Master `15 -> 15`; a positive input cannot be
reduced below 1. Direct NPC commands, projectile NPC commands and non-PvP player commands use the
same contract while retaining their existing command validation, immunity, lifecycle and replication
responsibilities.

Authority: `DamageTargetKind` distinguishes only source-backed formula families. `WorldRuleState`
owns Expert/Master selection. `DamageResolutionSystem` owns health, immunity and clamp behavior; the
named calculation system owns no mutable Simulation state. Projectile damage no longer bypasses NPC
defense. There is no new PvP queue or protocol authority: PvP is a verified pure calculation only.

RED: `npc-damage-red.txt` exit `1` due exclusively to missing `DamageCalculationSystem` and
`DamageTargetKind`. GREEN: `npc-damage-final.txt` and `combat-damage-final.txt` exit `0`. The Combat
verifier constructs an authoritative 5-defense player through the existing Item 4 equip command and
restores WorldRuleState from a persistence snapshot; normal, Expert and Master commits leave health
83, 84 and 85 respectively.

Evolution/compatibility: no persistence format, V1456 packet layout or bootstrap contract changed.
`persistence-final.txt`, `world-rules-loopback-final.txt`, `protocol-compatibility-final.txt` and
`full-client-bootstrap-final.txt` all exit `0`. `main-boundary-final.txt` reports 386 files and zero
violations. Serial `root-release-build-final.txt` exits `0` with 0 warnings and 0 errors.

Accepted boundary: deterministic Main mitigation formula and current Dome integer health application
for direct NPC, projectile NPC and non-PvP player paths. Deferred/Unknown: `DamageVar`, damage random
stream parity, actual PvP request/authority, knockback, broader hit immunity semantics, and full
legacy player defense assembly from buffs, prefixes, consumables and stances. Task 9 remains
**IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 tile solidity basic collision slice accepted

Source: Version4 `Terraria/Collision.cs:1633-1795` establishes that `TileCollision` derives ordinary
blocking from `Main.tileSolid`; top surfaces are handled separately and are explicitly excluded from
the side and reverse-vertical collision branches. The frozen source hash and line-level conclusion are
in `Build/diagnostics/main-migration/task-8-tile-solidity/20260819-132253/source-manifest.txt`.

Proposition: the current axis-aligned physics collider blocks an active, known ordinary solid tile,
does not block an active known non-solid tile, does not make a known platform a horizontal wall, and
keeps the map boundary blocking. An active unknown tile ID is rejected before `IsGrounded`, position or
velocity change. Since slope and half-brick geometry is not migrated in this slice, a known ordinary
solid slope/half-brick deliberately retains the existing whole-tile fallback rather than becoming
passable through reuse of a world-generation query.

Authority: `TileCollisionSystem` now owns one injected `TileDefinitionRegistry`; its default path is
the existing 753-ID Version4 registry and it reads no `Main` static table. It classifies an active,
known `BlocksLiquid && !IsPlatform` tile as the current ordinary whole-tile collider. The existing
registry data and `LiquidPropagationSystem` are not altered. A sweep prevalidation rejects unknown
active tile IDs before any physics commit.

RED evidence: `physics-red.txt` exits nonzero because active Type 4 was incorrectly treated as a
wall; `unknown-tile-red.txt` exits nonzero because Type 753 was silently passable; and
`slope-preservation-red.txt` exits nonzero because the first implementation accidentally made a known
slope/half-brick passable. The final implementation keeps that unported geometry fallback rather than
claiming complete `Collision` parity.

GREEN evidence in `Build/diagnostics/main-migration/task-8-tile-solidity/20260819-132253/`:
`physics-final.txt`, `npc-final.txt`, `liquid-final.txt`, and `main-boundary-final.txt` all exit `0`.
MainBoundary checked 400 Simulation source files with zero violations. Serial
`root-release-build-final.txt` exits `0` with 0 warnings and 0 errors. No persistence format,
Protocol packet, TCP loopback or bootstrap behavior changed, so those unrelated gates are not used as
substitutes for the focused physics proof.

Accepted boundary: definition-backed ordinary whole-tile collision classification and explicit unknown
active tile rejection. Deferred/Unknown: platform landing/fall-through direction and frame semantics,
slope and half-brick geometry, actuators, doors, NPC-specific collision, liquid collision and full
Terraria `Collision` parity. Task 8 remains **IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 controlDown authority slice accepted

Source: Version4 `MessageBuffer.cs:665-671` maps PlayerControls first control-byte bit 1 to
`player.controlDown`; `NetMessage.cs:445-449` writes the same bit. `Player.cs:17474` derives
platform `fallThrough` from `controlDown`, which reaches `Collision.cs:1727`. SHA-256 manifests and
the exact source summary are in
`Build/diagnostics/main-migration/task-8-down-input/20260819-135600/source-manifest.txt`.

Authority: this bit already exists in the V1456 PlayerControls fixed payload. The migration retains
the 14-byte payload and field order, projecting bit 1 through `PlayerControlIntent.Down`,
`DomeServer`, `PlayerInput`, `PlayerInputComponent`, and the player-only call to
`TileCollisionSystem.MoveAndResolve`. NPC calls retain the default false. `ClientInputFrame` has no
production references, so it is not treated as a second authority path.

RED: `protocol-red.txt` fails because PlayerControlIntent initially had no `Down` field. A temporary
parallel verifier attempt produced the known `CS2012` shared output lock and was discarded; all final
gates were rerun serially. The new protocol assertion initially compared the whole control byte and
caught the coexisting FacingRight bit; it was corrected to assert bit 1 specifically without changing
the wire requirement.

Proposition: decoding and encoding a V1456 PlayerControls frame retains `Down` at control bit 1 while
the payload remains 14 bytes. A `SimulationInputBatch` with `Down=true` reaches same-tick platform
collision and lets the player pass through the top, while existing default landing and pure fall-through
contracts remain green.

GREEN evidence in `Build/diagnostics/main-migration/task-8-down-input/20260819-135600/`:
`protocol-final.txt`, `physics-final.txt`, `player-lifecycle-loopback-final.txt`,
`world-rules-loopback-final.txt`, `full-client-bootstrap-final.txt`, `npc-final.txt`, and
`liquid-final.txt` all exit `0`. `main-boundary-final.txt` reports 413 Simulation files and zero
violations. Serial `root-release-build-final.txt` exits `0` with 0 warnings and 0 errors.

Accepted boundary: V1456 controlDown bit authority through the actual server-to-Simulation player
collision path. Deferred/Unknown: mount/grapple/pulley/reverse-gravity forced fall-through, `fall2`
lifecycle, non-default platform frames, slope/half-brick geometry, actuators, doors and full Terraria
collision parity. Task 8 remains **IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 fall-through pure contract accepted

Source: Version4 `Collision.cs:1727` skips a `tileSolidTop` landing only when
`fallThrough && (Velocity.Y <= 1f || fall2)`. Version4 `Player.cs:14311-14321` passes the caller's
fallThrough into TileCollision; `Player.cs:17474-17480` derives it from `controlDown` and additional
mount, grapple, pulley and reverse-gravity producers. The source hash and authority audit are in
`Build/diagnostics/main-migration/task-8-fall-through-contract/20260819-134513/source-manifest.txt`.

Proposition: the extracted pure rule returns skip/collide behavior exactly for low velocity,
above-threshold velocity, `fall2`, non-platform and non-top-frame cases. It is not attached to the
current tick because Dome has no `controlDown`, mount, grapple, pulley or reverse-gravity authority in
`PlayerInput`, `ClientInputFrame` or the current protocol intent.

RED: `physics-red.txt` first failed on the missing `PlatformCollisionRuleSystem`; the next run caught
an inverted `fall2` expectation, and the verifier was corrected without weakening the source assertion.
GREEN: `physics-final.txt` prints the pure contract pass plus the existing collision scenarios. NPC,
Liquid and MainBoundary final outputs all exit `0`; MainBoundary currently scans 404 Simulation files
with zero violations. Serial `root-release-build-final.txt` exits `0` with 0 warnings and 0 errors.

Accepted boundary: source-faithful pure fall-through predicate in
`Physics/Systems/PlatformCollisionRuleSystem.cs`. Deferred/Unknown: adding `Down/controlDown` to
client input and wire protocol, mount/grapple/pulley producers, per-player fall-through state, actual
tick integration, fall2 lifecycle and reverse gravity. Task 8 remains **IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 platform-top collision slice accepted

Source: Version4 `Terraria/Collision.cs:1633-1795` includes `tileSolidTop` tiles in the collision
candidate when `frameY == 0`; the downward branch resolves a collider arriving from above unless
`fallThrough` (or the low-velocity `fall2` condition) allows passage. The horizontal branches and
the upward branch explicitly require `!Main.tileSolidTop[type]`. The source hash and frozen summary
are in `Build/diagnostics/main-migration/task-8-platform-top/20260819-133545/source-manifest.txt`.

Proposition: with the current Simulation's default movement path, a known active platform catches a
collider falling in the negative-Y direction, sets its Y velocity to zero and `IsGrounded=true`, while
the same platform remains passable from the side and from below. No fall-through command or input is
invented because the current Simulation has no authoritative producer for that legacy input.

RED: `physics-red.txt` exits nonzero because the platform was passed through while falling from above.
The first implementation hit the wrong vertical branch and was rejected by the same assertion; after
correcting the branch, `physics-final.txt` exits `0` and covers falling landing, side passage, upward
passage, ordinary solid, non-solid, boundary, slope fallback and unknown active type rejection.

Authority and evolution: `TileCollisionSystem.IsLandingTile` reuses the existing Version4
`TileDefinitionRegistry`, requires an active known definition, and admits a platform only for the
default top frame (`FrameY == 0`). It does not alter registry data, persistence, protocol, bootstrap,
or the existing liquid consumer. `npc-final.txt`, `liquid-final.txt`, and `main-boundary-final.txt`
all exit `0`; MainBoundary reports 400 files and 0 violations. Serial
`root-release-build-final.txt` exits `0` with 0 warnings and 0 errors.

Accepted boundary: default platform-top landing plus explicit side/upward pass-through. Deferred/Unknown:
fall-through input authority and command, `fall2`, gravDir `-1`, platform frame families beyond the
default top frame, slopes/half-bricks, actuators, doors, NPC collision differences and complete
Terraria `Collision` parity. Task 8 remains **IN PROGRESS**.
2026-08-19 Item Extractinator ECS slice: implemented immutable Version4 mode/trade registry,
deterministic direct player use, and a separate Wiring/Chest trigger path with frame normalization,
reverse slot selection, locked/open chest rejection and 60-tick cooldown. Definitions, Items,
Wiring, WorldObjects, Items Loopback, Protocol Compatibility and Persistence verifiers pass, as do
the Simulation, Server and serial solution Release builds. The equipment regeneration assertion was
corrected to derive its expected recovery from the defense-adjusted post-damage health; no gameplay
rule changed.

# 2026-08-19 Main ECS migration Task 8 half-brick runtime collision geometry accepted

Source: Version4 `Terraria/Collision.cs:1683-1691` translates an active half-brick collision
rectangle downward by 8 pixels and reduces its height to 8 pixels. The frozen source SHA-256 and
excerpt are in `Build/diagnostics/main-migration/task-8-half-brick/20260819-142432/`
`source-manifest.txt`.

RED: `physics-red.txt` in `20260819-141923` exits nonzero because the earlier Dome whole-tile
fallback stopped a falling collider at the wrong half-brick top. The focused verifier now covers
top landing, upper-half horizontal passage, lower-half horizontal block, upward physical lower edge,
and standing alignment, while retaining existing full-block, platform, non-solid, unknown-ID and
nonzero-slope fallback checks.

Authority: `TileCollisionSystem` exclusively derives the internal runtime bounds. For a known
ordinary unsloped half-brick it exposes `[tileY, tileY + 0.5)` in Dome's unit-grid orientation;
falling/standing use the resulting top, upward collision retains the lower edge, and horizontal
movement tests actual vertical overlap. No WorldGeneration query, protocol input, persistence field,
Server path, definition-table rewrite or NPC-specific rule was introduced.

GREEN: `Build/diagnostics/main-migration/task-8-half-brick/20260819-142432/` records PlayerPhysics,
NPC and Liquid verifiers at exit `0`; MainBoundary exit `0` with 428 Simulation files and zero
violations; serial root Release exit `0`, 0 warnings and 0 errors; `git diff --check` exit `0`.

Accepted boundary: unsloped half-brick runtime collision geometry. Deferred/Unknown: all slope
geometry, actuator and door collision, special platform frame rules, forced fall-through/fall2,
NPC-specific collision decisions and complete legacy `Collision` parity. Task 8 remains
**IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 player top-slope contact accepted

Source: Version4 `Player.cs:14294-14308` calls `TileCollision` and then
`Player.cs:14324-14355` calls `SlopingCollision`; the latter delegates the diagonal contact
behavior to `Collision.SlopeCollision` (`Collision.cs:1228-1469`). Source hash and frozen excerpts
are in `Build/diagnostics/main-migration/task-8-player-top-slope/20260819-143832/source-manifest.txt`.

RED: `20260819-143146/physics-red.txt` fails on the Slope 1 top-contact proposition because the
old whole-tile fallback resolves the wrong height. The final verifier covers Slope 1 and Slope 2
surface contact, actual `DomeSimulation.Tick` player composition, unknown active slope rejection,
and all prior solidity, half-brick, platform and fall-through assertions.

Authority: `TopSlopeContactSystem` is a deep Simulation module with one mutation interface. It
resolves only active known ordinary Slope 1/2 tiles for positive-gravity players. The player path
defers those two shapes in `TileCollisionSystem`, then invokes the module after ordinary movement;
NPC calls keep the accepted whole-tile fallback. Unknown active IDs fail closed before module state
mutation.

GREEN: `Build/diagnostics/main-migration/task-8-player-top-slope/20260819-143832/` records
PlayerPhysics, PlayerLifecycle loopback, NPC and Liquid verifiers at exit `0`; MainBoundary exit
`0` with 432 Simulation files and zero violations; serial root Release exit `0`, 0 warnings and
0 errors; `git diff --check` exit `0`.

Accepted boundary: positive-gravity player Slope 1/2 diagonal top-surface contact and its actual
tick ordering. Deferred/Unknown: bottom slopes 3/4, ceilings, hoik, StepUp/StepDown, stairFall,
platform slopes, reverse gravity, mount/grapple/pulley, NPC-specific slope behavior, actuator/door
special cases and complete legacy `Collision.SlopeCollision` parity. Task 8 remains **IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 bottom-slope defer pure contract accepted

Source: Version4 `Collision.cs:1697-1706` sets the slope 3/4 defer flag from previous Y,
absolute horizontal velocity and the collider's left/right boundary. The frozen source hash and
excerpt are in `Build/diagnostics/main-migration/task-8-bottom-slope-contract/20260819-144319/`
`source-manifest.txt`.

RED: `20260819-144152/physics-red.txt` exits `1` because the focused verifier initially referenced
the missing `BottomSlopeCollisionRuleSystem`. After implementation, the verifier covers slope 3,
slope 4, threshold rejection and non-slope rejection.

Authority: `BottomSlopeCollisionRuleSystem.ShouldDeferWholeTileCollision` is a pure Simulation
contract only. It is intentionally not wired into `TileCollisionSystem`: the complete legacy
bottom-slope behavior also depends on `SlopeCollision`, movement direction and StepUp/StepDown
semantics that do not yet have an equivalent Dome owner.

GREEN: `Build/diagnostics/main-migration/task-8-bottom-slope-contract/20260819-144319/` records
PlayerPhysics, NPC and Liquid exit `0`; MainBoundary exit `0` with 434 Simulation files and zero
violations; serial root Release exit `0`, 0 warnings and 0 errors; `git diff --check` exit `0`.

Accepted boundary: source-faithful pure bottom-slope defer predicate. Deferred/Unknown: runtime
bottom-slope/ceiling geometry, StepUp/StepDown, hoik, reverse gravity, NPC slope behavior,
actuator/door/special collision and complete legacy slope parity. Task 8 remains **IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 active-inactive collision candidate contract accepted

Source: Version4 `Collision.cs:1661-1665` skips null, inactive and `inActive` tiles before tile
solidity/shape logic. This source establishes `IsInactive` as the collision candidate exclusion;
it does not establish `IsActuated` alone as a collision exclusion. The frozen source manifest is
`Build/diagnostics/main-migration/task-8-tile-activation-contract/20260819-144857/`
`source-manifest.txt`.

RED: the PlayerPhysics verifier initially references the missing
`TileCollisionActivationRuleSystem`, producing `physics-red.txt` exit `1`. GREEN: its active,
inactive and absent assertions pass in `physics-final.txt`.

Authority: `TileCollisionActivationRuleSystem.ShouldParticipate` is a pure rule that formalizes
the existing `TileCollisionSystem` gate. No actuator lifecycle, closed-door definition set,
ignoreDoors caller, tile transition command or wire/persistence change was invented.

Final evidence in the same directory: PlayerPhysics, MainBoundary and serial root Release exit `0`;
MainBoundary scanned 436 Simulation files with zero violations; Release has 0 warnings and
0 errors; `git diff --check` exits `0`.

Accepted boundary: active/inactive collision-candidate predicate. Deferred/Unknown: actuator state
transitions, closed doors and ignoreDoors authority, special platform flags, bottom-slope runtime
geometry, NPC-specific collision and complete legacy collision parity. Task 8 remains
**IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 actuator authority audit deferred

The current Wiring actuator pipeline is not a Version4 actuator migration. Version4
`Wiring.cs:395-427` toggles an actuator tile between `inActive()` and its reactivated state, while
`Collision.cs:1661-1665` consumes `inActive` as a collision-candidate exclusion. In this
physical-deletion source checkout, `DeActive` and `ReActive` bodies are stubs, so their complete
state, frame, networking and multi-tile effects cannot be guessed.

Dome instead sends `ActuatorCommandSystem` output through generic `TileChangeCommand(Place/Kill)`;
the generic commit replaces a tile with a fresh active tile or `default`. It neither requires
`WorldTile.IsActuated` nor changes `WorldTile.IsInactive`, and would erase state required for a
faithful actuator toggle. Existing Wiring loopback only proves a logic gate creates that generic
command, not actuator collision pass-through or restoration.

No source or verifier change was made. A local, no-deletion `v1.4.5.6` companion source was then
recovered at `D:\TRbackup\无任何删减通过编译\Terraria\Wiring.cs` (SHA-256
`704BFD8029E29FCF64BFDF4150A14D599D7489D3F58462F485B47A557BDF3FDE`). It proves `ReActive` preserves
the tile and clears inactive, but `DeActive` additionally depends on solid/not-really-solid,
seven special tile IDs, the tile above, `PreventsActuationUnder`, `CanKillTile`, and type-226
world/progression state. Those predicates currently lack complete Dome owners, so the next batch is
not a blind `SetInactive` edit: it must create a tile-preserving transition contract that models
every claimed predicate and rejects/defer unsupported ones. Its RED/GREEN matrix must cover valid
toggle, restore, non-actuator rejection, unknown footprint, replay/revision and
persistence/replication. Task 8 remains **IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 Type 1 actuator inactive-state slice accepted

The first source-complete actuator slice uses the local no-deletion v1.4.5.6 companion source,
`Wiring.cs:3302-3345`, frozen in
`Build/diagnostics/main-migration/task-8-actuator-state/20260819-153058/source-manifest.txt`.
It accepts only the exact DeActive short-circuit available without the unported `CanKillTile` path:
an active Type 1 actuator tile with no active tile above. ReActive unconditionally clears inactive.

`ActuatorCommandSystem` now reads the authoritative `WorldGrid`, requires `IsActive` and
`IsActuated`, derives the next state from current `IsInactive`, and emits a tile-preserving
`SetInactive` command. `TileChangeCommitSystem` changes only `WorldTile.IsInactive`; it does not
Place or Kill the target. A footprint with any unsupported coordinate is rejected before emitting
commands. The actual Dome wiring phase remains the producer and the normal world commit is the
consumer.

RED recorded the missing world/state command contract. GREEN covers tile field preservation,
deactivate/restore, missing actuator-bit rejection with no section revision mutation, active-above
rejection, atomic mixed-footprint rejection, collision pass-through/restoration after commit, and
a live pressure-plate/logic-gate/DomeSimulation tick loopback. Wiring, PlayerPhysics,
WiringLiquidChest loopback, Persistence and NPC verifiers all exited `0`; MainBoundary scanned
445 Simulation files with 0 violations; the serial root Release build exited `0` with 0 warnings
and 0 errors; `git diff --check` exited `0`.

This accepts only Type 1/no-active-above inactive-state transition. Type 226, other type
classification, `NotReallySolid`, the seven special types, active-above
`PreventsActuationUnder`/`CanKillTile`, multi-tile definitions, frame consequences and legacy
TileSquare timing remain deferred. Task 8 remains **IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 actuator eligibility rule accepted

`ActuatorDeactivationRuleSystem.ShouldDeactivate` preserves the full recovered Version4
`Wiring.DeActive` boolean predicate as a pure contract: active and actuator bits, Type 226
world/progression guard, solid/not-really-solid classification, seven special tile IDs, and the
active-above `PreventsActuationUnder`/`CanKillTile` branch.

The Type 1 producer uses the rule for its supported inactive-above path. For active-above tiles,
`CanKillTile` is unowned and passed as false, so the branch rejects instead of guessing. The
focused verifier caught and rejected the earlier placeholder `canKillTile=true`.

Fresh evidence is under
`Build/diagnostics/main-migration/task-8-actuator-rule/` (current run directory): Wiring,
PlayerPhysics, WiringLiquidChest loopback, Persistence and NPC verifiers passed; MainBoundary
scanned 451 files with 0 violations; serial root Release passed with 0 warnings and 0 errors; diff
check passed. Full Type 226/world-surface ownership, definition registries, `CanKillTile`, special
tile behavior, multi-tile framing and legacy TileSquare timing remain deferred. Task 8 remains
**IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 CanKillTile world-object protection contract accepted

`ActuatorCanKillTileRuleSystem.CanKill` preserves the Version4 `WorldGen.CanKillTile` base guards
and protection inputs without coupling Simulation to legacy world objects. It rejects out-of-world,
missing/inactive targets, wall 350, protected active-above tree/special/frame relations,
boulder-chest, locked-door, multi-tile and chest protection. The focused Wiring verifier proves each
class independently.

Evidence: `Build/diagnostics/main-migration/task-8-can-kill-rule/20260819-155734/`; Wiring,
PlayerPhysics, Persistence and NPC passed; MainBoundary scanned 455 files with 0 violations; root
Release passed with 0 warnings/errors; diff check passed. This is contract-only: active-above
actuator runtime remains fail-closed until Chest/Door and the remaining tree/frame/boulder/multi-tile
owners are projected with source-backed tests. Task 8 remains **IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 Chest.CanDestroyChest inventory rule accepted

The no-deletion v1.4.5.6 `Chest.CanDestroyChest` source is frozen at
`Build/diagnostics/main-migration/task-8-chest-destruction/20260819-160641/source-manifest.txt`.
`ChestDestructionRuleSystem.CanDestroy` preserves its exact rule: an absent or empty chest is
destroyable; any slot with a positive-type, positive-quantity item blocks destruction. It does not
misclassify locked or open chest state as the legacy condition.

The Wiring verifier proves the owner result and its `isChestBlocked` projection into the CanKillTile
contract. No coordinate/footprint adapter exists yet, so active-above actuator runtime remains
fail-closed. PlayerPhysics, Persistence and NPC passed; MainBoundary scanned 458 files with 0
violations; serial Release passed with 0 warnings/errors; diff check passed. Task 8 remains
**IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 CanKillTile protection contract accepted

The recovered no-deletion v1.4.5.6 `WorldGen.CanKillTile` source is frozen at
`Build/diagnostics/main-migration/task-8-can-kill-rule/20260819-155734/source-manifest.txt`
(SHA-256 `B9F7834CE1BC68C1DD9C656574A2272DB6F79E1407D934E1ADA33EDC3C930F82`).
`ActuatorCanKillTileRuleSystem.CanKill` is a pure contract that explicitly models base world/tile
guards plus protected active-above tree/special/frame relations, boulder-chest, locked-door,
multi-tile and chest protections. The Wiring verifier proves each guard and protection class.

It is intentionally not yet attached to active-above actuator runtime authority: not all source
relations have a Dome owner, so the producer remains fail-closed. PlayerPhysics, Persistence and
NPC regressions passed; MainBoundary scanned 455 Simulation files with 0 violations; serial Release
passed with 0 warnings/errors; diff check passed. Next batch should project existing chest/door
owners into this rule before enabling any active-above actuator case. Task 8 remains **IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 legacy chest origin query accepted

The no-deletion V1456 oracle proves that `WorldGen.CanKillTile` derives an exact chest coordinate
from the candidate tile and its frames: Types 21/467 use
`x - (frameX / 18 % 2), y - frameY / 18`; Type 88 uses
`x - (frameX / 18 % 3), y - frameY / 18`. The legacy MessageBuffer destruction paths and WorldFile
Type 88 creation conditions corroborate the horizontal frame widths and top-left coordinate use.

`LegacyChestOriginQuery.TryGetOrigin` now preserves that calculation as a pure Simulation query. It
accepts only active Types 21, 467 and 88 with non-negative, 18-pixel-aligned frames; inactive,
unsupported, negative and misaligned inputs reject without any lookup or mutation. The initial
Wiring verifier failed at compile time because this explicit contract did not exist; final focused
coverage proves all three source formulas and rejection cases.

Evidence is `Build/diagnostics/main-migration/task-8-chest-footprint/20260819-162117/`. Wiring,
Wiring/Liquid/Chest loopback, Persistence, PlayerPhysics and NPC all exit 0; MainBoundary scans 465
Simulation files with 0 violations; serial root Release exits 0 with 0 warnings/errors; diff check
exits 0.

This is not a chest protection runtime projection. Current `CreateChest(x, y)` ownership has not
been proven to mean imported legacy origin, so no index lookup, `isChestBlocked` producer, active-
above actuator behavior, persistence/import redefinition, or other `CanKillTile` protection class
was enabled. Task 8 remains **IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 chest destruction protection projection accepted

Current source proves the previously deferred coordinate ownership chain: `WldChestReader` reads
legacy chest `(X,Y)`, the compatibility projections preserve it, `ChestPersistentState` retains it,
and `DomeSimulation.RestoreChest` installs that exact coordinate in `ChestIndexSystem`.

`LegacyChestDestructionQuery.TryGetIsChestBlocked` now composes the accepted frame-origin contract,
the exact index and `ChestDestructionRuleSystem`. It permits no chest or an empty chest at the
origin, blocks a populated chest at that origin, ignores a populated chest at an adjacent coordinate,
and returns failure instead of permission for malformed/unsupported frames or inconsistent index
state. The Wiring RED captured the missing query; final focused coverage proves each stated branch.

Evidence is
`Build/diagnostics/main-migration/task-8-chest-protection-projection/20260819-162655/`. Wiring,
Wiring/Liquid/Chest loopback, Persistence, PlayerPhysics and NPC exit 0; MainBoundary scans 466
Simulation files with 0 violations; serial root Release exits 0 with 0 warnings/errors; diff check
exits 0.

This does not enable the actuator active-above branch. Tree/special/frame, boulder, locked-door and
multi-tile `CanKillTile` facts remain unowned and therefore continue to fail closed. Task 8 remains
**IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 locked-door tile rule accepted

The no-deletion V1456 oracle's `WorldGen.IsLockedDoor(Tile)` at `WorldGen.cs:70324-70340` is a
pure tile/frame predicate: Type 10, `frameY` inclusive range 594 through 646, and `frameX` below
54. `WorldGen.CanKillTile` invokes it for Type 10 at `WorldGen.cs:63297-63305`. It does not use
Dome-style door entities, open state, mechanism state, or player inventory.

`LockedDoorRuleSystem.IsLocked` preserves those exact conditions. The Wiring verifier's initial RED
was the missing rule; final coverage proves both inclusive boundaries, the frame-X rejection,
wrong-type rejection and the source-faithful inactive-tile result. Evidence is
`Build/diagnostics/main-migration/task-8-locked-door-rule/20260819-163225/`. Wiring,
Wiring/Liquid/Chest loopback, Persistence, PlayerPhysics and NPC exit 0; MainBoundary scans 469
Simulation files with 0 violations; serial root Release exits 0 with 0 warnings/errors; diff check
exits 0.

This is a pure rule only. It is not yet connected to active-above actuator runtime, because that
producer still lacks authoritative tree/special/frame, boulder and multi-tile facts. A regular
`DoorSnapshot` is not being misclassified as a legacy locked door. Task 8 remains **IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 boulder chest protection relation accepted

Version4 `WorldGen.CheckBoulderChest` (`WorldGen.cs:49518-49538`) first derives a boulder-relative
pair of above-tile coordinates, then blocks when either side has the source breakability reason with
container scanning. `BoulderChestProtectionRuleSystem` now preserves that coordinate and OR
aggregation contract with explicit `isBoulder`, left and right blocker facts. It rejects inactive,
negative and non-grid frame state.

The first implementation incorrectly corrected an absolute x coordinate rather than the negative
frame offset. The focused verifier exposed the defect; the corrected rule now follows the source
statement order. Evidence is
`Build/diagnostics/main-migration/task-8-boulder-chest-rule/20260819-163910/`. Wiring,
Wiring/Liquid/Chest loopback, Persistence, PlayerPhysics and NPC exit 0; MainBoundary scans 470
Simulation files with 0 violations; serial root Release exits 0 with 0 warnings/errors; diff check
exits 0.

This does not classify boulder IDs, reduce container semantics to `BlocksLiquid`, implement all
`CheckTileBreakability_HasReasonToReturnEarly` inputs, or enable actuator active-above runtime.
Those source facts remain fail closed. Task 8 remains **IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 tile breakability protection rule accepted

The no-deletion V1456 `WorldGen.CheckTileBreakability_HasReasonToReturnEarly`
(`WorldGen.cs:63474-63498`) is now represented by the pure
`TileBreakabilityProtectionRuleSystem`. The exact rule protects Type 77 outside hardmode and the
recovered `TileID.Sets.PreventsTileRemovalIfOnTopOfIt` IDs when the ignored type differs; it also
protects the source-derived locked-door predicate and, only when requested, recovered container
types. The rule intentionally omits `targetTile.IsActive`: the legacy method does too, while the
enclosing `CanKillTile` contract owns its active, bounds and wall guards.

The Wiring verifier recorded a missing-system RED, then covers every source branch. Evidence is
`Build/diagnostics/main-migration/task-8-tile-breakability-rule/20260819-164549/`. Wiring,
Wiring/Liquid/Chest loopback, Persistence, PlayerPhysics and NPC exit 0; MainBoundary scans 471
Simulation files with 0 violations; serial root Release exits 0 with 0 warnings/errors.

This accepts the pure early-return predicate only. Runtime boulder classification, a world-grid
adapter, and complete actuator active-above authority remain absent; unowned tree, special, frame
and multi-tile facts remain fail closed. Task 8 remains **IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 boulder type classification accepted

The recovered no-deletion V1456 `TileID.Sets.Boulders` declaration at `TileID.cs:197` is the exact
source authority for boulder IDs: `138, 484, 664, 665, 711, 712, 713, 714, 715, 716`. Its frozen
manifest and SHA-256 (`686F5340E5C71F940BCB3C5E30AB9125D032BAFDE991A34129FAB26662528296`) are in
`Build/diagnostics/main-migration/task-8-boulder-type-rule/20260819-165200/source-manifest.txt`.

`LegacyBoulderRuleSystem.IsBoulder` is a pure exact-set classifier. The Wiring verifier captured a
missing-symbol RED, then proved all ten IDs and nearby non-ID rejection. Wiring, Wiring/Liquid/Chest
loopback, Persistence, PlayerPhysics and NPC all exit 0; MainBoundary scans 474 Simulation files
with 0 violations; serial root Release exits 0 with 0 warnings/errors; `git diff --check` exits 0.

Only classification is accepted. No world-grid lookup, active/frame/bounds validation, boulder chest
composition or actuator active-above behavior was enabled. The remaining unknown protection facts
continue to fail closed. Task 8 remains **IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 PreventsActuationUnder type classification accepted

The recovered V1456 `TileID.Sets.PreventsActuationUnder` set at `TileID.cs:315` is exactly
`21, 467, 26, 77, 88, 470, 475, 237, 597, 441, 468`. The source hash and declaration are frozen
in `Build/diagnostics/main-migration/task-8-actuation-set-rule/20260819-170800/source-manifest.txt`.
`LegacyActuationProtectionRuleSystem.PreventsActuationUnder` preserves it as a pure classifier.

The Wiring RED captured the missing classifier; final coverage proves all eleven IDs and ordinary-ID
rejection. Wiring, Wiring/Liquid/Chest loopback, Persistence, PlayerPhysics and NPC exit 0;
MainBoundary scans 484 Simulation files with 0 violations; serial root Release exits 0 with 0
warnings/errors.

Only the source set is accepted. No actuator runtime, world-grid projection, Type 226 authority or
complete `CanKillTile` producer was enabled. Task 8 remains **IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 Type 235 multi-tile world-grid query accepted

The V1456 Type 235 `CanKillTile` branch derives `originX = x - (frameX % 54) / 18` and evaluates
three upper footprint tiles. `LegacyMultiTileProtectionQuery.TryGetIsBlocked` is now the read-only
WorldGrid projection: active Type 235 and 18-aligned frames are required, all three source cells are
bounds checked, and the accepted multi-tile protection rule evaluates the cells. Invalid or missing
state returns no fact rather than permission. Source evidence is frozen in
`Build/diagnostics/main-migration/task-8-multitile-query/20260819-171000/source-manifest.txt`.

The Wiring verifier recorded the missing query RED, then proves origin projection, protected and
ordinary footprints, and out-of-bounds rejection. Wiring, Wiring/Liquid/Chest loopback, Persistence,
PlayerPhysics and NPC exit 0; MainBoundary scans 487 Simulation files with 0 violations; serial
root Release exits 0 with 0 warnings/errors.

This is a read-only Type 235 projection, not a complete `CanKillTile` producer or actuator runtime
integration. Those paths remain fail closed. Task 8 remains **IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 composed CanKillTile read query accepted

`LegacyCanKillTileQuery.TryGetCanKill` now composes the source-backed `WorldGen.CanKillTile`
contract: world bounds, existence, active and wall guards; active-above tree/special predicates;
boulder chest and Type 235 world-grid projections; locked-door classification; and exact chest
inventory/origin projection. It delegates only the final boolean to
`ActuatorCanKillTileRuleSystem` and returns no fact when a required dependent projection is unknown.
Evidence is `Build/diagnostics/main-migration/task-8-can-kill-query/20260819-171200/`, with source
manifest for `WorldGen.cs:63226-63334`.

The Wiring verifier captured the missing query RED, then proves ordinary acceptance, active-above
tree rejection, locked-door rejection and bounds rejection. Focused GREEN and the post-refactor
serial root Release rerun both exit 0; Release has 0 warnings/errors. MainBoundary in the same batch
scanned 490 Simulation files with 0 violations.

This accepts the composed read query only. `ActuatorCommandSystem` does not call it yet; complete
runtime timing and unsupported caller facts remain fail closed. Task 8 remains **IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 active-above Type 1 actuator query integration accepted

The V1456 `Wiring.DeActive` branch (`Wiring.cs:3302-3338`) permits a solid actuator target with an
active upper tile only when that upper type is not `PreventsActuationUnder` and `CanKillTile(i,j)`
succeeds. `ActuatorCommandSystem` now receives an explicit optional `canKillTileQuery`; an omitted
callback retains fail-closed behavior. For active upper tiles it first applies the exact accepted
`PreventsActuationUnder` set, then invokes the callback. `DomeSimulation` supplies the callback from
its authoritative WorldGrid, hardmode progression, chest store and index through the composed
`LegacyCanKillTileQuery`.

The Wiring verifier recorded the missing callback RED, then proves authorized active-above Type 1
deactivation and protected-above rejection. Evidence is
`Build/diagnostics/main-migration/task-8-active-above-actuator/20260819-171500/`. Wiring,
Wiring/Liquid/Chest loopback, Persistence, PlayerPhysics and NPC exit 0; MainBoundary scans 492
Simulation files with 0 violations; serial root Release exits 0 with 0 warnings/errors.

Accepted scope is only the current Type 1 active-above path. Type 226, complete tile definition
authority, source framing/network timing and full actuator parity remain **IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 boulder chest world-grid protection query accepted

The recovered V1456 `WorldGen.CheckBoulderChest` source (`WorldGen.cs:49518-49538`) derives two
above coordinates from the boulder's frame and checks each with the boulder type as `ignoreType` and
`scanForContainer=true`. `LegacyBoulderChestProtectionQuery` now performs that projection against
the authoritative `WorldGrid`: it requires an in-bounds active boulder, validates the exact boulder
ID set and frame relation, bounds-checks both upper tiles, and composes the accepted breakability
rule. It returns a known blocked/unblocked fact only after all reads succeed; invalid or unknown
inputs return no fact and cannot become permission.

The Wiring verifier recorded the missing-query RED, then proves protected-container blocking,
ordinary upper tiles, non-boulder and out-of-bounds rejection. Evidence is
`Build/diagnostics/main-migration/task-8-boulder-protection-query/20260819-165500/`. Wiring,
Wiring/Liquid/Chest loopback, Persistence, PlayerPhysics and NPC exit 0; MainBoundary scans 475
Simulation files with 0 violations; serial root Release exits 0 with 0 warnings/errors.

This is a read-only boulder protection projection, not a complete `CanKillTile` authority producer.
It is not connected to active-above actuator runtime; tree/special/frame/multi-tile and other
unowned protection categories remain fail closed. Task 8 remains **IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 tree-trunk type classification accepted

The recovered V1456 `TileID.Sets.IsATreeTrunk` set at `TileID.cs:163` is exactly
`5, 72, 583, 584, 585, 586, 587, 588, 589, 596, 616, 634`. The source hash and snippet are
recorded in `Build/diagnostics/main-migration/task-8-tree-trunk-rule/20260819-170000/source-manifest.txt`.
`LegacyTreeTrunkRuleSystem.IsTreeTrunk` preserves the set as a pure classifier.

The Wiring verifier captured the missing classifier RED, then proved all twelve IDs and nearby
non-trunk rejection. Wiring, Wiring/Liquid/Chest loopback, Persistence, PlayerPhysics and NPC exit
0; MainBoundary scans 476 Simulation files with 0 violations; serial root Release exits 0 with
0 warnings/errors.

This accepts classification only. Tree frame exceptions, upper-tile projection, special/multi-tile
relations and active-above actuator runtime remain unowned and fail closed. Task 8 remains
**IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 tree-trunk frame protection predicate accepted

The V1456 `WorldGen.CanKillTile` tree branch (`WorldGen.cs:63252-63265`) is now represented by
`TreeTrunkProtectionRuleSystem.ShouldProtectAbove`. For a source-classified active-above tree trunk,
it protects only when the candidate type differs, neither exact frame exception applies
(`frameX=66, frameY 0..44`; `frameX=88, frameY 66..110`), and `frameY < 198`. The frozen source
manifest is `Build/diagnostics/main-migration/task-8-tree-frame-rule/20260819-170200/source-manifest.txt`.

The Wiring RED initially exposed the missing predicate; focused coverage then caught and corrected
the strict `frameY=198` boundary. Wiring, Wiring/Liquid/Chest loopback, Persistence, PlayerPhysics
and NPC exit 0; MainBoundary scans 478 Simulation files with 0 violations; serial root Release
exits 0 with 0 warnings/errors.

Only the pure predicate is accepted. No runtime upper-tile adapter, composed `CanKillTile` producer,
or active-above actuator behavior was enabled. Special and multi-tile relations remain fail closed.
Task 8 remains **IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 special active-above protection predicate accepted

The V1456 `WorldGen.CanKillTile` active-above special branch (`WorldGen.cs:63260-63291`) is now
represented by `SpecialTileProtectionRuleSystem.ShouldProtectAbove`. With a differing candidate
type and active upper tile, Type 323 protects frame X 66 or 220; Types 21, 26, 72, 77, 88, 467 and
488 always protect; Type 80 protects frame columns 0, 1, 4 and 5. Source evidence is frozen in
`Build/diagnostics/main-migration/task-8-special-rule/20260819-170400/source-manifest.txt`.

The Wiring verifier captured the missing-symbol RED, then proves all branch and rejection cases.
Wiring, Wiring/Liquid/Chest loopback, Persistence, PlayerPhysics and NPC exit 0; MainBoundary scans
481 Simulation files with 0 violations; serial root Release exits 0 with 0 warnings/errors.

This accepts the pure special predicate only. It is not connected to a world-grid adapter,
`CanKillTile` producer or active-above actuator runtime; multi-tile and other unowned facts remain
fail closed. Task 8 remains **IN PROGRESS**.

# 2026-08-19 Main ECS migration Task 8 Type 235 multi-tile protection predicate accepted

The V1456 `WorldGen.CanKillTile` Type 235 branch (`WorldGen.cs:63300-63315`) checks three upper
footprint cells, skips inactive cells, and rejects if any active cell satisfies the source
breakability predicate with `scanForContainer=true`. `MultiTileProtectionRuleSystem.IsBlocked`
preserves the pure candidate-Type, exact-three-cell and OR aggregation contract. Source evidence is
frozen in `Build/diagnostics/main-migration/task-8-multitile-rule/20260819-170600/source-manifest.txt`.

The Wiring verifier recorded the missing-system RED, then proves blocked, ordinary, inactive and
incomplete-footprint cases. Wiring, Wiring/Liquid/Chest loopback, Persistence, PlayerPhysics and NPC
exit 0; MainBoundary scans 482 Simulation files with 0 violations; serial root Release exits 0 with
0 warnings/errors.

Only the pure Type 235 predicate is accepted. Runtime frame-origin lookup, world-grid adapter,
complete `CanKillTile` producer and active-above actuator behavior remain fail closed. Task 8 remains
**IN PROGRESS**.

# 2026-08-20 Main ECS migration Task 8 world-surface metadata preservation accepted

The V319 WLD reader has an independent source `double worldSurface`; it is not derivable from
world dimensions or the spawn coordinate. This batch carries that exact value through
`LegacyWorldMetadata`, `CompatibilityWorldMetadata`, `CompatibilityToDomeProjection` and the
immutable `WorldMetadata.WorldSurface` contract. The field is nullable: imported WLD values are
present, while generated worlds and evidence-free historical snapshots are explicitly unknown.

`WorldPersistenceFormat` advances from v2 to v3 and writes a presence flag plus the source double.
Its v1/v2 readers preserve `null`. `DomeStatePersistenceFormat` advances from v17 to v18 and
appends the optional field after the existing progression payload; v1-v17 layouts retain their
field interpretation and restore `null`. The Persistence verifier captures RED for the missing
metadata contract and then proves exact `123.5` embedded/outer round-trip, v1/v2 embedded recovery,
v17 outer recovery and strict trailing-byte rejection. The WorldImport verifier proves the actual
V319 model -> Compatibility -> Dome chain.

Fresh evidence is
`Build/diagnostics/main-migration/task-8-world-surface-metadata/20260820-033834/`. WorldImport,
Persistence, Wiring, Wiring/Liquid/Chest loopback, PlayerPhysics and NPC all exit `0`; MainBoundary
checks 500 Simulation files with 0 violations; the serial root Release build exits `0` with 0
warnings and 0 errors. `git diff --check` was run after this entry.

Accepted scope is only the source value's import and persistence authority. Type 226 actuator
runtime, guessed world-surface defaults and full tile-definition parity are not accepted; Type 226
continues to fail closed. Task 8 remains **IN PROGRESS**.

# 2026-08-20 Main ECS migration Task 8 Type 226 actuator runtime slice accepted

The source-backed Type 226 branch of `Wiring.DeActive` now runs through the ECS actuator command
path. `LegacyActuatorTileDefinitionRuleSystem` records only the proven facts needed here: Type 1 and
226 are solid; V1456 `NotReallySolid` is exactly `{10, 387, 388}`. The existing liquid/collision
definition flags are not treated as a substitute for `Main.tileSolid`.

`ActuatorCommandSystem` accepts optional authoritative `WorldMetadata` and
`WorldProgressionState`. Type 226 rejects below `WorldSurface` until `DefeatedPlantera`, permits
above `WorldSurface`, and rejects whenever `WorldSurface` is unavailable. `DomeSimulation` carries
metadata from snapshot restore or persistence snapshot creation into the wiring phase; accepted
changes remain `TileChangeCommand` values committed by the existing world boundary.

The Wiring verifier's real RED was the missing metadata/progression API. GREEN proves below-surface
pre-Plantera rejection, below-surface post-Plantera acceptance, above-surface acceptance and
unknown-surface fail-closed behavior, alongside the prior Type 1 and active-above cases. Evidence
is under `Build/diagnostics/main-migration/task-8-type226-actuator/`; final serial gates include
Persistence, WorldImport, Wiring, Wiring/Liquid/Chest loopback, PlayerPhysics, NPC, MainBoundary
(504 files, 0 violations), and root Release (`0 warnings, 0 errors`).

Accepted scope is only the Type 1/Type 226 actuator definition and Type 226 surface/progression
runtime slice. The full tileSolid table, all actuator tile types, SquareTileFrame/network timing and
complete legacy actuator parity remain deferred. Task 8 remains **IN PROGRESS**.

# 2026-08-20 Main ECS migration Task 8 special actuator non-actuated set accepted

The explicit V1456 `Wiring.DeActive` switch cases `314, 379, 386, 387, 388, 389, 476` are now
source-backed known non-actuated targets in `LegacyActuatorTileDefinitionRuleSystem`. The existing
`NotReallySolid` meaning for `10, 387, 388` remains separate; no liquid or collision flag is
reinterpreted as `Main.tileSolid`.

The Wiring verifier recorded RED for the missing Type 314 classification, then proves all seven
special IDs reject actuator `SetInactive` commands without world mutation. Persistence, Wiring/
Liquid/Chest loopback, PlayerPhysics and NPC exit `0`; MainBoundary checks 506 Simulation files with
0 violations; serial root Release exits `0` with `0 warnings, 0 errors`. Evidence:
`Build/diagnostics/main-migration/task-8-special-actuator/20260820-040847/`.

Accepted scope is only these seven explicit switch cases. Full `Main.tileSolid` coverage, other
actuator definitions, SquareTileFrame/TileSquare timing and complete actuator parity remain
deferred. Task 8 remains **IN PROGRESS**.

# 2026-08-20 Main ECS migration Task 8 source-backed Type 2 actuator classification accepted

The no-deletion V1456 `Main.cs` initialization path explicitly assigns `tileSolid[2] = true`
(around `Main.cs:8010-8025`). `LegacyActuatorTileDefinitionRuleSystem` now records only this
individual Type 2 fact, while keeping `NotReallySolid` independent and unknown types fail closed.

The Wiring verifier captured RED for the missing Type 2 classification, then proves Type 2
classification and one state-preserving `SetInactive` command for an active, actuated tile with no
active tile above. Persistence, Wiring/Liquid/Chest loopback, PlayerPhysics and NPC exit `0`;
MainBoundary scans 507 Simulation files with 0 violations; serial root Release exits `0` with
0 warnings and 0 errors. Evidence:
`Build/diagnostics/main-migration/task-8-type2-actuator/20260820-041700/`.

Accepted scope is only Type 2 and the already-authoritative no-active-above path. The complete
`Main.tileSolid`/`tileSolidTop` tables, runtime rewrites, other tile IDs, framing, TileSquare timing
and full actuator parity remain deferred. Task 8 remains **IN PROGRESS**.

# 2026-08-20 Main ECS migration Task 8 V1456 static tile-solid definition accepted

`Main.Initialize_TileAndNPCData2` and `Main.Initialize_TileAndNPCData1` define the V1456 base
`tileSolid` state. `TileDefinitionRegistry.CreateVersion4Base` already preserves all 324 final
static true facts and the six explicit false facts `3, 4, 5, 11, 110, 634`; the three source loops
`255..268`, `435..439` and `727..732` are expanded into that immutable Simulation definition.
`LegacyActuatorTileDefinitionRuleSystem` now consumes this shared authority rather than duplicating
the table.

The Wiring verifier records RED for missing static Type 0, then enumerates all source true and
false facts. Its clean replay output is identical. Persistence, Wiring/Liquid/Chest loopback,
PlayerPhysics and NPC exit `0`; MainBoundary scans 508 Simulation files with 0 violations; serial
root Release exits `0` with `0 warnings, 0 errors`. Evidence:
`Build/diagnostics/main-migration/task-8-static-solid-definition/20260820-042319/`.

This is the initialization-time base table, not a lifecycle migration. In particular,
`Main.DoUpdateInWorld`'s temporary `tileSolid[379]` override, `tileSolidTop`, framing, TileSquare
timing and full actuator parity remain deferred. Task 8 remains **IN PROGRESS**.

# 2026-08-20 Main ECS migration Task 9 server-owned moon phase state accepted

V1456 `Main.UpdateTime_StartDay` increments `moonPhase` at dawn and wraps at `8`.
`WorldClock` now owns a validated `0..7` byte that changes only on the night-to-day transition;
its immutable snapshot carries the value through Simulation persistence and server projection.

`DomeStatePersistenceFormat` v19 appends moon phase after the v18 world-surface tail. Versions
v1-v18 remain exact prefixes and restore phase `0`; invalid and truncated v19 payloads reject.
`DomeServer.CreateWorldDataContext()` projects the immutable phase through the existing V1456
MoonPhase field without changing packet layout.

The batch captured missing-state, lost-persistence and default-projection REDs. GREEN proves dawn
`7 -> 0`, pause preservation, invalid phase rejection, v19/v18 recovery, malformed payload
rejection and real WorldData byte projection. The focused replay is identical. WorldRules,
Persistence, WorldRules TCP loopback, Protocol Compatibility and FullClientBootstrap exit `0`;
MainBoundary scans 512 Simulation files with 0 violations; serial root Release has `0 warnings,
0 errors`. Evidence: `Build/diagnostics/main-migration/task-9-moon-phase-state/20260820-043724/`.

Accepted scope is moon-phase state, dawn transition, persistence and WorldData projection. Remix
semantics, moon visuals/types, event eligibility, NPC/player consumers and legacy random-stream
ordering remain deferred. Task 9 remains **IN PROGRESS**.

# 2026-08-20 Main ECS migration Task 9 WLD moon phase import accepted

Legacy WLD Headers persist `_tempMoonPhase` as an `Int32` immediately after saved time and
day-time. The V319 reader now validates `0..7`, carries the source value through immutable legacy
and compatibility metadata, and creates the authoritative Dome clock snapshot with that phase.
Malformed WLD values `-1` and `8` reject before a Dome snapshot can be made; no seed-based phase
fallback was added.

The WLD verifier captured the missing metadata-member RED, then proves V1-to-V319 Header parsing,
valid phase preservation, invalid pointer-table phase rejection and recorded differential oracle
parity. WorldImport, Persistence and Protocol Compatibility exit `0`; WorldRules replay is
identical; MainBoundary scans 516 Simulation files with 0 violations; serial root Release exits
`0` with `0 warnings, 0 errors`. Evidence:
`Build/diagnostics/main-migration/task-9-wld-moon-phase-import/20260820-045545/`.

This accepts WLD moon-phase import only. Legacy time, blood moon, eclipse, moon type, rendering,
event eligibility and gameplay consumers remain deferred. Task 9 remains **IN PROGRESS**.

# 2026-08-20 Main ECS migration Task 9 WLD Blood Moon and Eclipse import accepted

Legacy WLD Headers write and read the `BloodMoon` and `Eclipse` Booleans after moon phase. The V319
reader now preserves them through immutable legacy and compatibility metadata into the authoritative
`WorldProgressionState`; no Server-side mutation or day/night-based normalization was added.

The parser verifier captured the missing metadata-member RED, then proves v69 default Eclipse,
v70 persisted Eclipse, v319 flag preservation and differential-oracle parity. WorldImport,
Persistence and Protocol Compatibility exit `0`; WorldRules replay is identical; MainBoundary scans
517 Simulation files with 0 violations; serial root Release exits `0` with `0 warnings, 0 errors`.
Evidence: `Build/diagnostics/main-migration/task-9-wld-event-flags-import/20260820-133913/`.

WLD `Double time` plus `Boolean dayTime` remain explicitly deferred: the current authoritative
clock is integral, while source V1456 can produce fractional time. No rounding/truncation fallback
was introduced. Task 9 remains **IN PROGRESS**.

# 2026-08-20 Main ECS migration Task 9 WLD hard-mode import accepted

Legacy WLD persists `Main.hardMode` directly after `shadowOrbCount` and `altarCount`. The V319
reader now preserves this Boolean through immutable legacy and compatibility metadata into the
authoritative `WorldProgressionState.IsHardMode`; no boss, ore, invasion or later Header state is
claimed by this narrow batch.

The parser verifier recorded the missing metadata-member RED, then proves the v22 default and v23
stored-field boundary, v319 preservation and differential-oracle parity. WorldImport, Persistence
and Protocol Compatibility exit `0`; WorldRules replay is identical; MainBoundary scans 518
Simulation files with 0 violations; serial root Release exits `0` with `0 warnings, 0 errors`.
Evidence: `Build/diagnostics/main-migration/task-9-wld-hardmode-import/20260820-134904/`.

Task 9 remains **IN PROGRESS**. WLD time/day-time, boss flags, ore tiers, invasion, weather and
other gameplay state still require separately sourced batches.

# 2026-08-20 Main ECS migration Task 9 WLD progression facts import accepted

The V319 WLD reader now preserves exactly six direct source progression flags:
`downedBoss1`, `downedBoss2`, `downedBoss3`, `downedMechBossAny`, `downedPlantBoss` and
`downedGolemBoss`. They cross immutable legacy/compatibility metadata into the matching
authoritative `WorldProgressionState` facts. Queen Bee, individual mechanical bosses, Slime King
and Wall of Flesh were not inferred.

The parser verifier records RED for the missing metadata facts and then proves the v319 true-value
fixture, V1-V319 layout matrix and differential Oracle. WorldImport, Persistence and Protocol
Compatibility exit `0`; WorldRules replay is identical; MainBoundary scans 520 Simulation files
with 0 violations; serial root Release exits `0` with `0 warnings, 0 errors`. Evidence:
`Build/diagnostics/main-migration/task-9-wld-progression-facts-import/20260820-140022/`.

Task 9 remains **IN PROGRESS**. WLD time/day-time, Queen Bee, individual mechanical bosses, Slime
King, Wall of Flesh, ore tiers, invasion, weather and other Header state stay deferred.

# 2026-08-20 Main ECS migration Task 9 WLD Crimson-world import accepted

The V319 WLD reader now preserves the direct `WorldGen.crimson` Header Boolean through immutable
legacy and compatibility metadata into the authoritative `WorldRuleState.IsCrimsonWorld`. The
field is not derived from seed, biome tiles, backgrounds, or progression facts.

The parser verifier captured the missing metadata-member RED, then proves the v55 default/v56
serialized-field boundary, v319 true value, V1-V319 parser matrix and differential Oracle.
WorldImport, Persistence and Protocol Compatibility exit `0`; WorldRules replay is identical;
MainBoundary scans 522 Simulation files with 0 violations; serial root Release exits `0` with
`0 warnings, 0 errors`. Evidence:
`Build/diagnostics/main-migration/task-9-wld-crimson-import/20260820-000000/`.

Task 9 remains **IN PROGRESS**. Crimson biome generation, Corruption/Crimson selection, WLD
time/day-time, background values and other Header state stay deferred.

# 2026-08-20 Main ECS migration Task 9 WLD invasion size and type import accepted

The V319 WLD reader now preserves direct `Main.invasionSize` and `Main.invasionType` Header facts
through immutable legacy/compatibility metadata into the matching authoritative
`WorldProgressionState` fields. The reader consumes but does not invent owners for
`invasionDelay` and `invasionX`.

The parser verifier captured missing facts as RED, then proves v1/v319 positive values, the
V1-V319 parser matrix and differential Oracle. WorldImport proves the full projection plus
fail-closed negative-size validation. Persistence and Protocol Compatibility exit `0`; WorldRules
replay is identical; MainBoundary scans 522 Simulation files with 0 violations; serial root Release
exits `0` with `0 warnings, 0 errors`. Evidence:
`Build/diagnostics/main-migration/task-9-wld-invasion-import/20260820-000001/`.

Task 9 remains **IN PROGRESS**. Invasion delay/X/start size/spawn recovery, weather,
time/day-time and other Header state stay deferred.

# 2026-08-20 Main ECS migration Task 9 WLD game-mode import accepted

The WLD import now preserves Classic, Expert, Master and Journey identity in the immutable
`WorldRuleState.GameMode`. Its projection follows V1456 base derivation: Expert and Master set the
corresponding difficulty flags, while Journey remains `Difficulty=0`, non-expert and non-master.
This prevents treating GameMode 3 as Master merely because it is numerically higher.

The parser verifier captured missing state as RED, then proves v112 Expert, v208 Master and v319
Journey layouts plus the V1-V319 parser matrix and differential Oracle. WorldImport proves all four
base mappings and invalid mode rejection. Persistence v20 round-trips Journey, restores Classic
for v1-v19 and rejects invalid/truncated v20 values. Protocol Compatibility exits `0`; WorldRules
replay is identical; MainBoundary scans 530 Simulation files with 0 violations; serial root Release
exits `0` with `0 warnings, 0 errors`. Evidence:
`Build/diagnostics/main-migration/task-9-wld-game-mode-import/20260820-000003/`.

Task 9 remains **IN PROGRESS**. Journey creative override, secret-seed effective-difficulty
modifiers, dependent gameplay systems and other Header fields stay deferred.

# 2026-08-20 Main ECS migration Task 9 WLD pending-meteor schedule import accepted

The WLD readers now preserve only the direct `WorldGen.spawnMeteor` Header Boolean through
immutable legacy/compatibility metadata into `WorldProgressionState.IsMeteorScheduled`. The source
field is stored at `Terraria.IO/WorldFile.cs:1339` and read by modern and legacy paths at `2156`
and `3670`; no runtime scheduling, landing search or meteor impact is invoked by this import.

The parser/version-matrix and differential Oracle, WorldImport, Persistence and Protocol
Compatibility verifiers all exit `0`. WorldRules replayed twice with the same PASS set; MainBoundary
scans 534 Simulation files with 0 violations; the serial solution Release build exits `0` with
`0 warnings, 0 errors`. Evidence:
`Build/diagnostics/main-migration/task-9-wld-meteor-schedule-import/20260820-000004/`.

Task 9 remains **IN PROGRESS**. Source `shadowOrbSmashed`, orb/altar counters, random schedule
selection, landing/impact behavior and client ambience remain separate deferred branches.

# 2026-08-20 Main ECS migration Task 9 lossless weather-state representation accepted

The Simulation weather state now distinguishes raw `IsRaining`, `RainTimeTicks`, current
`RainStrength` and `MaximumRainStrength`, matching the independent source WLD facts without yet
reading a WLD weather record. `WithRain` retains current deterministic runtime transitions;
`WithRawRain` is restricted to immutable restoration semantics.

Persistence format v21 appends raw activity and maximum strength after v20 GameMode. The focused
RED showed the missing constructor/member, then GREEN proves an inactive saved state with retained
duration and distinct maximum strength. Persistence proves v21 round-trip and v20 derived recovery;
WorldRules replayed twice, WorldImport, Protocol Compatibility, MainBoundary and serial Release
all exit `0`. MainBoundary scans 546 Simulation files with no violations; Release reports
`0 warnings, 0 errors`. Evidence:
`Build/diagnostics/main-migration/task-9-weather-state-model/20260820-000006/`.

Task 9 remains **IN PROGRESS**. WLD rain field parsing/projection, `FixEndlessRainWorlds()` and
random weather behavior remain separate source-backed cards.

# 2026-08-20 Main ECS migration Task 9 WLD rain import deferred

The WLD Header reads `raining`, `rainTime` and `maxRaining` directly, then applies the source
`FixEndlessRainWorlds()` rule. Import is not accepted yet: the ECS runtime `RainStrength` used by
wind smoothing has no proven one-to-one relationship with WLD `maxRaining`, and the compatibility
model does not retain the secret-seed membership required by the v317-and-earlier repair branch.
The representation prerequisite is accepted separately; ambiguous rain mapping remains fail-closed.
Evidence: `Build/diagnostics/main-migration/task-9-wld-rain-import/20260820-000007/`.

# 2026-08-20 Main ECS migration Task 9 WLD clock import deferred

The WLD Header persists `_tempTime` as `Double` and `_tempDayTime` as `Boolean`, while the current
authoritative `WorldClock` and V1456 SetTime projection use `Int32 TimeOfDay`. The source does not
provide a general lossless conversion from arbitrary saved fractional values. WLD time/day-time
therefore remains fail-closed until a source-backed integral proof or a versioned fractional clock
representation is implemented. Evidence:
`Build/diagnostics/main-migration/task-9-world-clock-representation/20260820-000008/`.

# 2026-08-20 Main ECS migration Task 9 domain-scoped random stream accepted

The Simulation now owns a deterministic `WorldEventRandomState` with explicit advancing state,
snapshot ownership and persistence continuity. Dome persistence v22 stores the exact state;
v1-v21 use a documented world-seed fallback. The batch repaired historical fixtures after strict
trailing-data rejection exposed the new v22 tail, and verifies exact round-trip, fallback,
truncation rejection and `DomeSimulation` snapshot continuation.

WorldRules replayed twice with the same PASS set; Persistence, WorldImport and Protocol
Compatibility exit `0`; MainBoundary scans 559 Simulation files with no violations; serial Release
reports `0 warnings, 0 errors`. Evidence:
`Build/diagnostics/main-migration/task-9-random-stream-contract/20260820-000009/`.

Task 9 remains **IN PROGRESS**. This does not claim global `Main.rand` parity, WLD rain/wind
import, probabilistic meteor scheduling or replacement of existing item/loot random paths.

# 2026-08-20 Main ECS migration Task 9 modern WLD wind target accepted

WLD versions `>=62` now preserve the saved `windSpeedTarget` through the parser, compatibility
metadata and authoritative Simulation state; `WindSpeedCurrent` is initialized to the same value,
matching the legacy load relation. Versions `<62` remain explicitly unknown because their source
calls `WorldGen.RandomizeWeather()` using the legacy `genRand` stream and no compatible replay
contract exists.

Parser version-boundary and oracle checks, WorldImport, WorldCompatibility and Persistence pass.
WorldRules replayed twice with identical PASS output; MainBoundary scans 563 Simulation files with
no violations; serial Release reports `0 warnings, 0 errors`. Evidence:
`Build/diagnostics/main-migration/task-9-wld-wind-import/20260820-000010/`.

Task 9 remains **IN PROGRESS**. Old-layout random weather, cloud count, rain import and client
presentation remain deferred.

# 2026-08-20 Main ECS migration Task 9 WLD rain raw metadata accepted

The WLD parser and compatibility metadata now retain nullable `IsRaining`, `RainTimeTicks` and
`MaximumRainStrength` values from layouts that contain them. This is a lossless parser boundary,
not a runtime rain import: the source only provides `maxRaining`, while ECS `RainStrength` is the
current wind-smoothing value and their equality is not proven. `FixEndlessRainWorlds()` secret-seed
repair remains deferred for the same reason that the required membership is not in compatibility
metadata.

Parser version-boundary/oracle checks and the serial Release build pass with 0 warnings and 0
errors. Evidence:
`Build/diagnostics/main-migration/task-9-wld-rain-raw-metadata/20260820-000011/`.

Task 9 remains **IN PROGRESS**. Runtime rain projection, repair branches and weather scheduling
remain deferred.

# 2026-08-21 Main ECS migration Task 9 invasion size policy accepted narrowly

Added `WorldInvasionSizeSystem` with the exact source `StartInvasion` formulas: types 1/2 resolve
to `80 + 40 * qualifiedPlayers`, type 3 to `120 + 60 * qualifiedPlayers`, and type 4 to
`160 + 40 * qualifiedPlayers`. Nonpositive qualified-player counts and invalid types reject before
calculation. The policy is pure and is not yet folded into the legacy no-size command route.

Evidence: `Build/diagnostics/main-migration/task-9-invasion-size-policy/20260821-222745/`.
WorldRules focused verification, Simulation Release build and scoped diff check exit `0`; the build
reports `0 warnings, 0 errors`. Command integration, random side, travel/delay/warning, named clear
flags, NPC tables, MainBoundary, broad regressions and root solution Release remain deferred.

# 2026-08-20 Main ECS migration Task 9 Main responsibility coverage accepted

Added a 30-row source-anchored Main member coverage matrix and connected it to the MainBoundary
verifier. The gate now checks classifications, source anchors, owner fields and required domains,
while explicitly excluding client rendering/UI/input/assets from Simulation ownership.

MainBoundary passes with 571 Simulation files and no violations. Evidence:
`Build/diagnostics/main-migration/task-9-main-member-coverage/20260820-000012/`.

Task 9 remains **IN PROGRESS**. This is auditable responsibility accounting, not a gameplay-parity
or migration-completion claim; entity gap cards, tick-order work and unknown rows remain open.

# 2026-08-20 Main ECS migration Task 9 invasion lifecycle deferred

Source tracing shows that legacy invasion lifecycle owns delay, travel X, original size, warning
timing, qualified-player sizing, type-specific clear flags, `Main.rand` side selection and NPC
damage/announcement effects. Current ECS owns only type/remaining size and cannot claim that its
completion normalization reproduces `UpdateInvasion`.

The coverage matrix now marks this responsibility `unknown`; the state-model decision and exact
deferred branches are recorded in
`Build/diagnostics/main-migration/task-9-invasion-lifecycle-decision/20260820-000013/`.

Task 9 remains **IN PROGRESS**. The next invasion card must establish state ownership and prove one
persisted travel transition before adding completion or random-start behavior.

# 2026-08-20 Main ECS migration Task 9 tick-order coverage accepted

Added a source-to-phase coverage matrix for `Main.Update`, `UpdateTime`, world update and invasion
ordering. It distinguishes the proven named ECS schedule from unsupported legacy loop parity and
keeps world update, invasion travel, entity/loot ordering and server-host timing explicit.
Evidence: `Build/diagnostics/main-migration/task-9-tick-order-coverage/20260820-000014/`.

Task 9 remains **IN PROGRESS**. The mapping is a coverage/audit card; it does not claim full Main
loop parity.

# 2026-08-21 Main ECS migration Task 9 Player lifecycle coverage accepted

Player responsibility coverage is accepted for the verified server-authoritative subset: identity,
ownership, lifecycle snapshots, tile collision, death/respawn, UUID restore protection, typed V1456
projections and PVS-limited loopback respawn.

PlayerOwnership, PlayerAuthority and PlayerLifecycle Loopback verifiers all exit `0`. Evidence:
`Build/diagnostics/main-migration/task-9-player-lifecycle-coverage/20260821-000015/`.

Task 9 remains **IN PROGRESS**. Full `Player.cs` parity and unverified player capability families
remain open.

# 2026-08-21 Main ECS migration Task 9 NPC lifecycle gap recorded

The direct NPC Combat verifier and protocol projection pass, but the two-session Combat Loopback
verifier fails reproducibly with `Items=0` after the NPC death trace. This exposes an integration
gap in loot/world-item visibility or pickup timing. The Main NPC responsibility remains `unknown`;
spawn eligibility, AI families, type definitions and clear/loot side effects require separate cards.

Evidence: `Build/diagnostics/main-migration/task-9-npc-combat-lifecycle-gap/20260821-000016/`.

# 2026-08-21 Main ECS migration Task 9 NPC combat death-to-loot slice accepted narrowly

The prior `Items=0` loopback failure is resolved in the current source tree. The authoritative
Simulation now evaluates an inactive zero-health NPC with no lifecycle reason exactly once, records
the killed transition, and commits the server-owned world-item drop without reactivating the NPC.
The loopback verifier still requires one visible drop and PVS isolation; it was not weakened.

Fresh evidence in `Build/diagnostics/main-migration/task-9-npc-combat-lifecycle-gap/20260821-000018/`:
two independent Combat Loopback runs, direct Combat, Combat Protocol, TickOrder, MainBoundary (579
files, 0 violations), and serial root Release all pass. The Release build reports 0 warnings and
0 errors. This accepts only the lethal damage -> death publication -> deterministic loot -> PVS
visibility route.

The Main NPC responsibility remains **unknown** for spawn eligibility, tile selection, type defaults,
AI families, global-random ordering, NPC-table drops and broader clear/progression effects; those
remain separate cards.

# 2026-08-21 Main ECS migration Task 9 WLD Header disposition matrix accepted

Recorded the source wire-order reads for the v1-v87 and v88-v319 WLD headers in
`docs/research/2026-08-20-wld-header-disposition-matrix.md`. Each field is classified as
accepted, candidate, blocked, no-owner or client-only and is mapped to one existing immutable
owner or an explicit prerequisite. The matrix keeps fractional time, old-layout random weather,
ore tiers, orb/altar counters, invasion travel, cloud state and unaudited later records out of the
runtime import path.

Parser and WorldImport pass on the unchanged runtime source tree. MainBoundary scans 579 Simulation
files with 0 violations, and the serial root Release build reports 0 warnings and 0 errors. Scoped
diff check for the new matrix/card is clean; full worktree diff check still reports pre-existing
trailing whitespace in `src/Terraria.WorldFile.V319/LegacyReference/WorldFile.cs`.
Evidence: `Build/diagnostics/main-migration/task-9-wld-header-matrix/20260821-000017/`.

Task 9 remains **IN PROGRESS**. The next card must select one identity field and add a
version-boundary fixture RED before changing runtime ownership.

# 2026-08-21 Main ECS migration Task 9 NPC spawn authority guard accepted narrowly

Added the source-backed, immutable `NpcSpawnSnapshot.SpawnAuthorityEnabled` guard. When the
server-side spawn authority fact is disabled, `NpcSpawnEligibilitySystem` returns no commands before
candidate evaluation, matching the initial `NPC.Spawner.CanSpawnEnemiesNear` rejection boundary.
This does not derive the fact from client input and does not claim legacy spawn-rate, tile-search,
biome, event-table or `Main.rand` parity.

Focused NPC verifier passes, the Simulation Release build passes with 0 warnings and 0 errors, and
the scoped diff check passes. Per the current validation scope, MainBoundary, loopback, broad domain
regressions and root solution Release were not run. Evidence:
`Build/diagnostics/main-migration/task-9-npc-spawn-authority/20260821-000019/`.

Task 9 remains **IN PROGRESS**. Player-readiness derivation, `FindSpawnTile`, spawn-rate calculation,
zone/event predicates and NPC table selection remain separate deferred cards.

# 2026-08-21 Main ECS migration Task 9 projectile owner authority guard accepted narrowly

Added `DomeSimulation.QueueProjectileSpawn` and a commit-time owner lifecycle guard. A projectile
command is accepted only when its `PlayerHandle` resolves to an active Simulation player; unknown or
inactive owners produce no entity, replication identity or snapshot. This closes the forged-owner
authority boundary without claiming projectile behavior parity.

The focused Combat verifier passes, the Simulation Release build passes with 0 warnings and 0 errors,
and the scoped diff check passes. Large regressions, loopback, MainBoundary and root solution build
remain intentionally unrun. Evidence:
`Build/diagnostics/main-migration/task-9-projectile-owner-authority/20260821-000021/`.

Projectile motion/collision, damage, lifetime, type tables and full replication remain separate cards.

# 2026-08-21 Main ECS migration Task 9 projectile numeric input guard accepted narrowly

The projectile commit route now rejects non-finite X/Y position, initial vertical velocity and
projectile speed values before allocating an entity or replication identity. The focused Combat
verifier demonstrates both forged-owner rejection and `NaN` position rejection.

Combat focused verification, Simulation Release build (0 warnings, 0 errors), and scoped diff check
pass. Large regressions, loopback, MainBoundary and root solution build remain intentionally unrun.
Evidence: `Build/diagnostics/main-migration/task-9-projectile-owner-authority/20260821-000022/`.

# 2026-08-21 Main ECS migration Task 9 projectile lifetime guard accepted narrowly

Extended the projectile commit guard to reject non-positive `LifetimeTicks`. The focused Combat
verifier now covers forged owner, non-finite position and non-positive lifetime rejection. Simulation
Release build remains 0 warnings and 0 errors, and scoped diff check passes.

Evidence: `Build/diagnostics/main-migration/task-9-projectile-owner-authority/20260821-000023/`.
Large regressions, loopback, MainBoundary and root solution build remain intentionally unrun.

# 2026-08-21 Main ECS migration Task 9 projectile penetration guard accepted narrowly

The projectile commit route rejects `MaximumPenetration == 0` and values below `-1` before entity
and replication allocation, while preserving legacy `-1` infinite penetration. The focused Combat
verifier covers forged owner, non-finite position, lifetime and penetration rejection paths plus
the valid `-1` sentinel.

Combat focused verification, Simulation Release build (0 warnings, 0 errors), and scoped diff check
pass. Combat loopback, MainBoundary, broad regressions and root solution build remain intentionally
unrun. Evidence:
`Build/diagnostics/main-migration/task-9-projectile-owner-authority/20260821-000028/`.

# 2026-08-21 Main ECS migration Task 9 projectile inactive-owner guard accepted narrowly

Extended the projectile focused trace to cover an existing but dead player owner. The commit route
rejects both unknown and inactive owners before entity or replication allocation, alongside the
existing numeric input guards.

Combat focused verification, Simulation Release build (0 warnings, 0 errors), and scoped diff check
pass. Combat loopback, MainBoundary, broad regressions and root solution build remain intentionally
unrun. Evidence:
`Build/diagnostics/main-migration/task-9-projectile-owner-authority/20260821-000030/`.

# 2026-08-21 Main ECS migration Task 9 world-item spawn integrity guard accepted narrowly

`WorldItemSpawnSystem` now rejects non-finite world-item X/Y positions before allocating a
replication identity or active component. The focused Items verifier covers the `NaN` rejection,
Simulation Release build passes with 0 warnings and 0 errors, and scoped diff check passes.

Evidence: `Build/diagnostics/main-migration/task-9-item-world-spawn-integrity/20260821-000024/`.
Item loopback, MainBoundary, broad regressions and root solution build remain intentionally unrun.

# 2026-08-21 Main ECS migration Task 9 world-item motion integrity guard accepted narrowly

`WorldItemMotionSystem.TryMove` now rejects non-finite target X/Y values before changing the item
position, section or revision. The focused Items verifier proves `NaN` motion rejection while the
existing valid revisioned move remains green.

Items focused verification, Simulation Release build (0 warnings, 0 errors), and scoped diff check
pass. Item loopback, MainBoundary, broad regressions and root solution build remain intentionally
unrun. Evidence:
`Build/diagnostics/main-migration/task-9-item-world-motion-integrity/20260821-000026/`.

# 2026-08-21 Main ECS migration Task 9 inactive world-item pickup guard accepted narrowly

`DomeSimulation.CommitWorldItemPickups` now requires the command player to resolve to an active
`PlayerLifecycleComponent` before inventory transfer. A dead/inactive player can submit a pickup
command, but the world item remains active and no pickup event or inventory mutation is published.

The focused Items verifier, Simulation Release build (0 warnings, 0 errors), and scoped diff check
pass. Item loopback, MainBoundary, broad regressions and root solution build remain intentionally
unrun. Evidence:
`Build/diagnostics/main-migration/task-9-item-pickup-authority/20260821-000025/`.

# 2026-08-21 Main ECS migration Task 9 NPC spawn coordinate guard accepted narrowly

Extended the NPC spawn eligibility guard to reject non-finite candidate coordinates before command
emission. This preserves the source-backed invariant that `FindSpawnTile` produces a concrete tile
location, without pretending to implement its 50-attempt search, screen exclusion or biome rules.

Focused NPC verification, Simulation Release build (0 warnings, 0 errors), and scoped diff check all
pass. Evidence: `Build/diagnostics/main-migration/task-9-npc-spawn-authority/20260821-000020/`.
Large regression suites remain intentionally unrun under the current validation scope.

# 2026-08-21 Main ECS migration Task 9 world-item pickup range integrity guard accepted narrowly

`WorldItemPickupSystem.TryPickup` now rejects non-finite and negative pickup ranges before distance
comparison. The focused Items verifier proves that a distant item cannot be picked up through a
`NaN` range; valid pickup behavior remains green.

Items focused verification, Simulation Release build (0 warnings, 0 errors), and scoped diff check
pass. Item loopback, MainBoundary, broad regressions and root solution build remain intentionally
unrun. Evidence:
`Build/diagnostics/main-migration/task-9-item-pickup-integrity/20260821-000027/`.

# 2026-08-21 Main ECS migration Task 9 world-item pickup player-position guard accepted narrowly

`WorldItemPickupSystem.TryPickup` now rejects non-finite player X/Y coordinates before distance
comparison. This closes the second NaN bypass at the pickup boundary; valid range and inventory
transfer behavior remains green.

Items focused verification, Simulation Release build (0 warnings, 0 errors), and scoped diff check
pass. Item loopback, MainBoundary, broad regressions and root solution build remain intentionally
unrun. Evidence:
`Build/diagnostics/main-migration/task-9-item-pickup-integrity/20260821-000029/`.

# 2026-08-21 Main ECS migration Task 9 entity coverage ledger refreshed

The Main responsibility matrix now reflects the accepted narrow Projectile and Item authority
evidence instead of leaving both families as wholly planned. M-020 records projectile owner and
input-integrity guards; M-021 records world-item spawn, motion, pickup-range and active-player
pickup guards. Neither row claims full entity parity: projectile behavior/lifecycle and item
inventory, stacking, persistence and complete loopback remain open.

This is an accounting update only; no broad verifier or root build was run.

# 2026-08-21 Main ECS migration Task 9 focused smoke remains green

After the cumulative NPC, Projectile and Item authority/integrity changes, the key domain
verifiers were rerun serially: NPC, Combat, Items and TileInteraction all exit `0`. The TileInteraction
project includes its existing focused loopback assertions and also passes. This smoke confirms the
current source tree is coherent across the touched domains; it is not a full migration acceptance.

Evidence: `Build/diagnostics/main-migration/task-9-focused-smoke/20260821-000031/`.
MainBoundary, root Release and broad regression gates remain unrun by the current validation scope.

# 2026-08-21 Main ECS migration Task 9 projectile penetration sentinel corrected

Source audit found that legacy `Projectile.cs` uses `penetrate = -1` repeatedly for infinite
penetration. The earlier `MaximumPenetration <= 0` guard was too strict and has been corrected:
`-1` is accepted, `0` and values below `-1` are rejected. The focused Combat verifier proves both
rejection and acceptance paths.

Evidence: `Build/diagnostics/main-migration/task-9-projectile-owner-authority/20260821-000031/`.

# 2026-08-21 Main ECS migration Task 9 projectile velocity input integrity verified narrowly

Extended the focused Combat verifier with two commit-boundary rejection traces: a projectile
with `InitialVelocityY = NaN` and a projectile with `ProjectileSpeed = PositiveInfinity` produce
no entity or replication snapshot. The existing authoritative route already rejects both values
before definition lookup and identity allocation; this card records the executable coverage.

The focused Combat verifier passes, the Simulation Release build reports 0 warnings and 0 errors,
and the scoped diff check passes. Evidence:
`Build/diagnostics/main-migration/task-9-projectile-velocity-integrity/20260821-034305/`.
Full projectile motion/type-table/lifetime/replication parity, MainBoundary, broad regressions and
root solution Release remain deferred and intentionally unrun under the current validation scope.

# 2026-08-21 Main ECS migration Task 9 NPC spawn readiness predicate accepted narrowly

The source `NPC.Spawner.CanSpawnEnemiesNear(Player)` rejection boundary is now represented as an
immutable `NpcSpawnCandidate.CanSpawnEnemiesNear` fact. `NpcSpawnEligibilitySystem` rejects a
candidate when that fact is false before definition, budget or replication identity processing.
The field defaults to true for existing callers; this card consumes the fact but does not derive it
from client input or claim the source player/zone implementation.

The focused NPC verifier covers the false-readiness rejection and existing valid/invalid paths.
The Simulation Release build reports 0 warnings and 0 errors, and the scoped diff check passes.
Evidence: `Build/diagnostics/main-migration/task-9-npc-spawn-readiness/20260821-034908/`.
Player readiness projection, inactive/dead player derivation, spawn-rate calculation, tile search,
screen exclusion, biome/event predicates, NPC tables, MainBoundary, broad regressions and root
solution Release remain deferred and intentionally unrun.

# 2026-08-21 Main ECS migration Task 9 invasion completion event accepted narrowly

The authoritative invasion progression route now publishes one
`WorldInvasionCompletedEvent(InvasionType)` when an active invasion reaches its normalized zero
state. The event is emitted from the previous typed state before the cleared state is exposed,
cleared at the next tick boundary, and exposed through `DomeServer` for server consumers.

The source boundary is `Main.UpdateInvasion` (`Main.cs:12958-13033`), where completion is type-aware
before `invasionType` and delay state are cleared. WorldRules focused verification passes, the
affected Simulation and Server Release builds report 0 warnings and 0 errors, and the scoped diff
check passes. Evidence:
`Build/diagnostics/main-migration/task-9-invasion-completion-event/20260821-035937/`.
This does not add named downed-event flags or persistence fields; travel/delay/warning, qualified
players, random start side, NPC tracking, announcements, achievements, MainBoundary, broad
regressions and root solution Release remain deferred and intentionally unrun.

# 2026-08-21 Main ECS migration Task 9 invasion start eligibility contract accepted narrowly

Added `WorldInvasionStartEligibilitySystem` over immutable `WorldInvasionPlayerSnapshot` values.
The predicate accepts only invasion types 1-4 and requires at least one active player with
`MaximumHealth >= 200`, matching the source `Main.StartInvasion` threshold. This is a pure contract
card: it is not wired to a client-provided count or to the existing command route until an
authoritative player-stat snapshot producer exists.

The WorldRules focused verifier covers inactive/under-threshold rejection, the exact 200 boundary
and invalid type rejection. Evidence:
`Build/diagnostics/main-migration/task-9-invasion-start-eligibility/20260821-222028/`.
WorldRules and Simulation Release verification/build plus scoped diff check exit `0`; the build has
`0 warnings, 0 errors`. Player-stat production, size derivation, random side, command integration,
MainBoundary, broad regressions and root solution Release remain deferred and intentionally unrun.

# 2026-08-21 Main ECS migration Task 9 invasion snapshot-gated start route accepted narrowly

Added an overload of `DomeSimulation.TryQueueWorldInvasion` and `DomeServer.TryQueueWorldInvasion`
that requires an immutable `IReadOnlyList<WorldInvasionPlayerSnapshot>`. Queue insertion is gated by
`WorldInvasionStartEligibilitySystem`; active `MaximumHealth=199` rejects and active `200` accepts.
The route never trusts a client-provided qualified-player count. The existing no-snapshot overload is
left unchanged until a production player-stat source can populate the snapshot.

Evidence: `Build/diagnostics/main-migration/task-9-invasion-start-eligibility/20260821-222253/`.
WorldRules, Simulation Release, Server Release and scoped diff check exit `0`; builds report `0
warnings, 0 errors`. Player-stat production, size derivation, random side, legacy route
integration, MainBoundary, broad regressions and root solution Release remain deferred.

# 2026-08-21 Main ECS migration Task 9 invasion player readiness producer accepted narrowly

`DomeSimulation.CreateWorldInvasionPlayerSnapshots` now projects active lifecycle and authoritative
`HealthComponent.Maximum` values into immutable `WorldInvasionPlayerSnapshot` facts. The server
exposes the same projection and the snapshot-gated invasion start route consumes it without trusting
a client count. The focused verifier proves producer output and the 199/200 readiness boundary.

Evidence refreshed at:
`Build/diagnostics/main-migration/task-9-invasion-start-eligibility/20260821-222458/`.
WorldRules, Simulation Release, Server Release and scoped diff check exit `0`; builds report `0
warnings, 0 errors`. Invasion size derivation, random side, legacy no-snapshot compatibility,
travel/delay/warning, named clear flags, MainBoundary, broad regressions and root solution Release
remain deferred.

# 2026-08-21 Main ECS migration Task 9 tick-order invasion boundary accepted narrowly

The TickOrder verifier now proves one source-backed cross-domain ordering invariant: a completed
invasion event is observable from the world phase, and the recorded `ApplyWorldClock` phase precedes
`AdvanceProjectiles` in the same tick. The fixture uses a one-unit invasion and does not alter the
production phase schedule.

Evidence: `Build/diagnostics/main-migration/task-9-tick-order-invasion/20260821-220650/`.
TickOrder focused verification, Simulation Release build and scoped diff check exit `0`; the build
reports `0 warnings, 0 errors`. Other Main subphase ordering, invasion travel/NPC ordering,
MainBoundary, broad regressions and root solution Release remain deferred and intentionally unrun.

# 2026-08-21 Main ECS migration Task 9 invasion progress duplicate guard refreshed

The invasion command boundary now rejects a second pending `WorldInvasionProgressCommand` with the
same nonnegative sequence before queue insertion. The focused WorldRules verifier covers this
duplicate rejection alongside the typed completion event and existing deterministic progress,
persistence and WorldData projection.

Evidence refreshed at:
`Build/diagnostics/main-migration/task-9-invasion-completion-event/20260821-220216/`.
WorldRules, Simulation Release, Server Release and scoped diff check all exit `0`; all reportable
behavior outside typed completion and duplicate command authority remains deferred.

# 2026-08-21 Main ECS migration Task 9 projectile lifetime authority accepted narrowly

The projectile movement phase now uses the existing `ProjectileLifetimeSystem.Advance` as the sole
counter mutation and expiry decision. This matches the retained source `Projectile.cs:15232-15236`
contract (`timeLeft--`, then `Kill()` at zero) and avoids a second inline lifetime implementation.
The focused Combat verifier covers the spawn-tick boundary and a `LifetimeTicks = 1` projectile
producing a stable inactive tombstone on its first movement tick.

Combat focused verification passes, the Simulation Release build reports 0 warnings and 0 errors,
and the scoped diff check passes. Evidence:
`Build/diagnostics/main-migration/task-9-projectile-lifetime-authority/20260821-035521/`.
Extra-update/type-specific lifetime changes, collision/damage/penetration parity, complete
persistence/replication behavior, MainBoundary, broad regressions and root solution Release remain
deferred and intentionally unrun.

# 2026-08-21 Main ECS migration Task 9 invasion derived start route accepted narrowly

Added a snapshot-gated start overload that accepts only invasion type, sequence and authoritative
player snapshots. `DomeSimulation` counts qualified players and derives size internally through
`WorldInvasionSizeSystem`; a type-3 start with two qualified active players produces size `240` in
the focused verifier. The caller cannot submit a contradictory size through this route.

Evidence: `Build/diagnostics/main-migration/task-9-invasion-derived-start/20260821-222937/`.
WorldRules, Simulation Release, Server Release and scoped diff check exit `0`; builds report `0
warnings, 0 errors`. Random side, travel/delay/warning, named clear flags, NPC tables, legacy
no-snapshot compatibility, MainBoundary, broad regressions and root solution Release remain
deferred.

# 2026-08-23 Main ECS migration Task 9 reduced-scope player authority recheck

The single focused PlayerAuthority verifier exits `0` for tile-authoritative collision, simulation
death/respawn, typed V1456 player projections, UUID-owned restore and client overwrite rejection.
Evidence: `Build/diagnostics/main-migration/task-9-player-authority-boundary/20260823-000000/`.

This refreshes M-018 only. Complete player parity, legacy client arrays and the full replication /
session matrix remain deferred.

# 2026-08-22 WorldGen reduced-scope housing room occupancy mapping

The bounded `Housing_CheckIfInRoom` slice is now represented by
`HousingRoomOccupancyQuery.Contains`. It preserves explicit coordinate membership while mutable
legacy `roomTiles` ownership, room scanning, and scheduling remain deferred. The WorldGen inventory
is now `227 Partial / 457 Unmapped`; legacy removal remains disabled. Focused verifier coverage
exercises present and absent room coordinates.

# 2026-08-22 WorldGen reduced-scope housing home-spot mapping

The bounded `ScoreRoom_CanBeHomeSpot` slice is now represented by
`HousingHomeSpotQuery.IsEligible`. It preserves the active type-379 rejection over explicit
`WorldTile` input while room scoring, Main ownership, scans, and NPC scheduling remain deferred.
The WorldGen inventory is now `225 Partial / 459 Unmapped`; legacy removal remains disabled.
Focused verifier coverage exercises inactive, allowed, and rejected tiles.

# 2026-08-22 WorldGen reduced-scope room-needs mapping

The bounded `RoomNeeds` slice is now represented by `RoomNeedsQuery.Evaluate` and
`RoomNeedsResult`. It preserves chair, table, door, torch and can-spawn classification over
explicit read-only Tile sets while legacy room flags, registry ownership, scoring, and NPC
scheduling remain deferred. The WorldGen inventory is now `226 Partial / 458 Unmapped`; legacy
removal remains disabled. Focused verifier coverage exercises complete and incomplete rooms.

# 2026-08-22 WorldGen reduced-scope housing bounds mapping

The bounded `Housing_GetTestedRoomBounds` slice is now represented by
`HousingTestBoundsQuery.Calculate` and `HousingTestBounds`. It preserves fixed room-bound
expansion and world-size clamps over explicit inputs while room globals, housing scans, out
mutation, and NPC scheduling remain deferred. The WorldGen inventory is now `224 Partial / 460
Unmapped`; legacy removal remains disabled. Focused verifier coverage exercises normal and clamped
rooms.

# 2026-08-22 WorldGen reduced-scope tile merge cull application mapping

The bounded `TileMergeCullCache.Cull` slice is now represented by
`TileMergeCullApplyQuery.Apply`. It preserves eight-neighbor cull-mask application as an immutable
`TileMergeNeighbors` result while leaving legacy ref mutation and cache ownership deferred. The
WorldGen inventory is now `220 Partial / 464 Unmapped`; legacy removal remains disabled. Focused
verifier coverage exercises all eight mask positions.

# 2026-08-22 WorldGen reduced-scope spawn-area classification mapping

The bounded `IsConsideredTheSpawnArea` slice is now represented by
`SpawnAreaClassificationQuery.IsConsidered`. It preserves remix, randomized/no-surface, and
ordinary surface branches using explicit inputs; Main/GenVars state ownership remains deferred.
The WorldGen inventory is now `221 Partial / 463 Unmapped`; legacy removal remains disabled.
Focused verifier coverage exercises all three rule families.

# 2026-08-22 WorldGen reduced-scope tile category count mapping

The bounded `GetTileTypeCountByCategory` slice is now represented by
`TileTypeCategoryCountQuery.Evaluate` and `TileScanGroupKind`. It preserves the fixed formulas for
None, Corruption, Crimson, Hallow, and TotalGoodEvil over explicit read-only counts. Tile scanning,
legacy enum ownership, and count mutation remain deferred. The WorldGen inventory is now
`222 Partial / 462 Unmapped`; legacy removal remains disabled. Focused verifier coverage exercises
the category formulas.

# 2026-08-22 WorldGen reduced-scope tile type area count mapping

The bounded `CountTileTypesInArea` slice is now represented by
`TileTypeCountAreaQuery.Count`. It counts active Tile types over an explicit immutable snapshot
rectangle into a new vector, preserving legacy active-tile semantics without caller-owned array
mutation. The WorldGen inventory is now `223 Partial / 461 Unmapped`; legacy removal remains
disabled. Focused verifier coverage exercises active and inactive tiles.

# 2026-08-22 WorldGen reduced-scope vine framing mapping

The existing `VineFrameQuery` and `VineFrameCommandSystem` now have explicit provenance for the
legacy `CheckVines` method. The mapping covers support type groups, bottom-slope rejection,
replacement type intent, and unsupported kill intent; `SquareTileFrame`, `KillTile`, and host tile
mutation remain deferred. The WorldGen inventory is now `216 Partial / 468 Unmapped`; legacy
removal remains disabled. Existing verifier coverage exercises conversion, supported, and
unsupported vine cases.

# 2026-08-22 WorldGen reduced-scope square tile frame request mapping

The bounded `SquareTileFrame` slice is now represented by
`SquareTileFrameRequestQuery.CreateRequests`. It preserves the nine-point row-major request
topology while leaving `resetFrame`, actual frame calculation, and host Tile mutation deferred.
The WorldGen inventory is now `217 Partial / 467 Unmapped`; legacy removal remains disabled.
Focused verifier coverage exercises the full nine-point order.

# 2026-08-22 WorldGen reduced-scope range frame coordinate mapping

The bounded `RangeFrame` slice is now represented by
`RangeFrameCoordinateQuery.CreateCoordinates`. It preserves the expanded rectangle and legacy
column-major traversal while leaving `MapUpdateQueue`, TileFrame, Framing.WallFrame, and host
mutation deferred. The WorldGen inventory is now `219 Partial / 465 Unmapped`; legacy removal
remains disabled. Focused verifier coverage exercises a non-square expanded range.

# 2026-08-22 WorldGen reduced-scope square wall frame topology mapping

The bounded `SquareWallFrame` slice is now represented by
`SquareWallFrameRequestQuery.CreateCoordinates` and `WallFrameCoordinate`. It preserves the
nine-point row-major wall coordinate topology while leaving `resetFrame`, `Framing.WallFrame`, and
host wall mutation deferred. The WorldGen inventory is now `218 Partial / 466 Unmapped`; legacy
removal remains disabled. Focused verifier coverage exercises the full nine-point order.

# 2026-08-22 WorldGen reduced-scope merge-culling mapping

The bounded `GetTileMergeCulling` slice is now represented by
`TileMergeCullingQuery.Evaluate` and `TileMergeCullMask`. It compares the center tile's
invisible-block state with all eight neighbors and takes the legacy host visibility decision as an
explicit input. The WorldGen inventory is now `208 Partial / 476 Unmapped`; legacy removal remains
disabled. Existing focused verifier coverage exercises both culling and the show-invisible bypass.

# 2026-08-22 WorldGen reduced-scope tree-type classification

The bounded `IsTreeType` slice is now represented by
`TreeTypeClassificationQuery.IsTreeType`, which takes the legacy trunk registry as an explicit
read-only set and preserves the negative-input false branch. The WorldGen inventory is now
`209 Partial / 475 Unmapped`; legacy removal remains disabled. Focused verifier coverage exercises
registered, negative, and non-registered inputs.

# 2026-08-22 WorldGen reduced-scope paint-color mapping

The bounded `paintColor` slice is now represented by `PaintColorQuery.GetColor` and
`PaintColorValue`, preserving the fixed legacy identifier-to-RGBA table including alpha `150` for
paint id `30`. Paint/coating effects and host Color application remain deferred. The WorldGen
inventory is now `210 Partial / 474 Unmapped`; legacy removal remains disabled. Focused verifier
coverage exercises primary, alpha-bearing, and default color mappings.

# 2026-08-22 WorldGen reduced-scope coating-color mapping

The bounded `coatingColor` slice is now represented by `CoatingColorQuery.GetColor` and
`CoatingColorValue`, preserving the two fixed coating colors and transparent default. Coating
application and mutable color-list behavior remain deferred. The WorldGen inventory is now
`211 Partial / 473 Unmapped`; legacy removal remains disabled. Focused verifier coverage exercises
both coating ids and the default branch.

# 2026-08-22 WorldGen reduced-scope forest background style mapping

The bounded `SetForestBGSet` slice is now represented by `ForestBackgroundSetQuery.Evaluate` and
`ForestBackgroundSet`. All fixed legacy styles are returned as immutable mountain/tree set values;
caller-owned array mutation and host background application remain deferred. The WorldGen inventory
is now `213 Partial / 471 Unmapped`; legacy removal remains disabled. Focused verifier coverage
exercises a regular style, the default branch, and a special style.

# 2026-08-22 WorldGen reduced-scope hollow-tree foliage mapping

The bounded `GetHollowTreeFoliageStyle` slice is now represented by
`HollowTreeFoliageStyleQuery.GetStyle`. It preserves the legacy hallow-background mapping for
styles `2`, `3`, `4`, and the default branch while taking background state as explicit input. The
WorldGen inventory is now `214 Partial / 470 Unmapped`; legacy removal remains disabled. Focused
verifier coverage exercises all mapped branches.

# 2026-08-22 WorldGen reduced-scope pile invalidity mapping

The bounded `InvalidTileForPilesOrSpeleothems` slice is now represented by
`PilesOrSpeleothemsInvalidityQuery.Evaluate`. It preserves the two-tile world margin, active-tile
guard, and explicit boulder registry predicate; destruction and registry ownership remain
deferred. The WorldGen inventory is now `215 Partial / 469 Unmapped`; legacy removal remains
disabled. Focused verifier coverage exercises valid, inactive, and boundary inputs.

# 2026-08-22 WorldGen provenance consistency audit

The static provenance audit now confirms all `216` path-bearing inventory target members resolve
to existing files. The method map contains exactly `684` rows (`215 Partial / 469 Unmapped`) and
its line/name/status keys match the inventory with `0` mismatches. This is static evidence only;
build, verifier execution, and runtime differential remain paused by the proposal boundary.

# 2026-08-22 WorldGen reduced-scope coating selection mapping

The bounded `coatingColors` slice is now represented by `CoatingColorSelectionQuery.Evaluate` and
`CoatingColorSelection`. It preserves fullbright/invisible block and wall selection and the null
tile empty result without exposing the legacy mutable `List<Color>`. The WorldGen inventory is now
`212 Partial / 472 Unmapped`; legacy removal remains disabled. Focused verifier coverage exercises
block, wall, and null-tile branches.

# 2026-08-22 WorldGen remaining execution Stage B contract status update

The proposal now records that the bounded TileFrame contract is implemented: immutable request /
result types, pending-mutation overlay ordering, ordinary-solid frame evaluation, affected
coordinates, and `TileFrameCommandSystem` command generation are present. The verifier source has
deterministic assertions for repeated evaluation, while full frame-important, liquid, tree, vine,
multi-tile, and runtime parity behavior remain deferred. This is a documentation/status correction,
not a claim that build or verifier execution has passed.

# 2026-08-22 Main ECS migration Task 9 reduced-scope completion manifest audit

The focused Completion verifier exits `0`: manifest weights total 100 points and the evidenced
core-server score is 92 points. Evidence:
`Build/diagnostics/main-migration/task-9-acceptance-review/20260822-230000/`.

The 92-point value is an accounting score, not a Task 9 or migration-completion percentage. M-001
planned work and M-007/M-008/M-009/M-014/M-024 unknown/deferred branches remain open.

# 2026-08-23 Main ECS migration Task 9 initializer disposition rechecked

The source-backed M-001 inventory confirms `Initialize_AlmostEverything` is orchestration across
independent static-data, server-adapter, client/content and liquid/domain families. Existing entity,
item and selected child cards do not establish complete initializer parity, so M-001 remains
`planned`; no generic `Simulation.InitializeAlmostEverything` owner is introduced. Evidence:
`Build/diagnostics/main-migration/task-9-main-member-coverage/20260823-010000/initializer-disposition-focused.txt`.

# 2026-08-23 Main ECS migration Task 9 reduced WorldGeneration verification

The `WorldGeneration.Verification --reduced` mode exits `0` after running 32 of 81 test sections
(39.5%). It covers tile neighborhood/state/wire/rope queries, bounded deterministic point/gem/moss
policies, and tree profile/suitability/eligibility/preparation/command boundaries. Evidence:
`Build/diagnostics/main-migration/task-9-world-generation-reduced/20260823-020000/`.

The remaining 49 sections, complete generation parity, liquid/ore/structure transaction coverage
and client effects were intentionally not run under the reduced validation policy.

# 2026-08-23 Main ECS migration Task 9 MainBoundary after reduced WorldGeneration child

After the reduced WorldGeneration child, the focused MainBoundary verifier exits `0`: 693
Simulation source files checked and zero forbidden `Main`, client, transport, Protocol or Server
dependencies. Evidence:
`Build/diagnostics/main-migration/task-9-acceptance-review/20260823-030000/`.

This is an isolation gate only; the remaining WorldGeneration sections and open Task 9 matrix rows
remain unverified or deferred.

# 2026-08-23 Main ECS migration Task 9 latest acceptance checkpoint

The latest independent checkpoint corrects the MainBoundary evidence to 693 Simulation files after
the reduced WorldGeneration and WorldGrid children. It retains all 30 matrix classifications,
including M-001 planned and M-007/M-008/M-009/M-014/M-024 unknown/deferred. Evidence:
`Build/diagnostics/main-migration/task-9-acceptance-review/20260823-050000/status.txt`.

The reduced WorldGeneration run covered 32 of 81 sections (39.5%); no broad regression or root
Release claim is made.

# 2026-08-23 Main ECS migration Task 9 affected Simulation build recheck

The affected `Terraria.Dome.Simulation` Release build exits `0` with `0` warnings and `0` errors
after the reduced WorldGeneration/WorldGrid child work. Evidence:
`Build/diagnostics/main-migration/task-9-acceptance-review/20260823-060000/simulation-build-focused.txt`.

This is a compile-graph gate for the affected project only; it does not replace the root solution
Release build or establish full Task 9 acceptance.

# 2026-08-23 Main ECS migration Task 9 remaining-work queue fixed

The remaining M-001/M-007/M-008/M-009/M-014/M-024 work is now recorded with explicit prerequisites
and prohibitions. The queue preserves source-backed unknown/deferred boundaries and prevents unsafe
generic initialization, clock conversion, invasion cadence substitution, Action queues or coroutine
queues. Evidence:
`Build/diagnostics/main-migration/task-9-acceptance-review/20260823-070000/remaining-work-queue.txt`.

# 2026-08-23 Main ECS migration Task 9 reduced-scope WorldGrid recheck

The single focused WorldGrid verifier exits `0` for tile-to-section mapping, immutable snapshots,
per-section revision isolation and out-of-bounds mutation rejection. Evidence:
`Build/diagnostics/main-migration/task-9-tile-command-commit-boundary/20260823-040000/world-grid-focused.txt`.

This is an M-022 foundational child acceptance only. Complete tile framing/liquid/flags, network
cadence and loopback coverage remain deferred.

# 2026-08-23 Main ECS migration Task 9 focused acceptance checkpoint refreshed

The independent checkpoint records 30 matrix rows, 685-file MainBoundary isolation with zero
violations, a 100-point completion manifest with 92 evidenced points, and all remaining planned,
unknown, deferred and excluded families. Evidence:
`Build/diagnostics/main-migration/task-9-acceptance-review/20260823-000000/status.txt`.

This is an evidence/accounting checkpoint under the reduced validation policy, not overall Task 9
completion or a migration percentage claim.

# 2026-08-21 Main ECS migration Task 9 invasion travel policy accepted narrowly

Added `WorldInvasionTravelSystem` for the source `UpdateInvasion` movement rule. Each step moves
toward `spawnTileX` by `max(dayRate, 1)` and clamps exactly to the target when the step crosses it;
non-finite position/rate inputs reject. The focused WorldRules verifier covers both directions, the
minimum step and arrival clamp.

Evidence: `Build/diagnostics/main-migration/task-9-invasion-travel-policy/20260821-223233/`.
WorldRules focused verification, Simulation Release build and scoped diff check exit `0`; the build
reports `0 warnings, 0 errors`. InvasionX persistence, warning counter/chat, random start side,
runtime integration, MainBoundary, broad regressions and root solution Release remain deferred.

# 2026-08-21 Main ECS migration Task 9 invasion warning policy accepted narrowly

Added `WorldInvasionWarningSystem` for the source `UpdateInvasion` warning counter. A moving
invasion decrements the counter; reaching zero resets it to `3600` and emits a warning request;
arrival emits a warning request without changing the counter. The focused WorldRules verifier
covers countdown, reset and arrival behavior.

Evidence: `Build/diagnostics/main-migration/task-9-invasion-warning-policy/20260821-223420/`.
WorldRules focused verification, Simulation Release build and scoped diff check exit `0`; the build
reports `0 warnings, 0 errors`. Chat/client projection, warning text, persistence and runtime
travel integration, MainBoundary, broad regressions and root solution Release remain deferred.

# 2026-08-21 Main ECS migration Task 9 invasion clear-flag mapping accepted narrowly

Added `WorldInvasionClearFlagSystem` for the source `UpdateInvasion` completion mapping. Invasion
types 1, 2, 3 and 4 resolve to Goblins, Frost, Pirates and Martians respectively; invalid types
reject. This card records only the typed mapping and does not mutate or persist named progression
flags.

Evidence: `Build/diagnostics/main-migration/task-9-invasion-clear-flags/20260821-223620/`.
WorldRules focused verification, Simulation Release build and scoped diff check exit `0`; the build
reports `0 warnings, 0 errors`. Named flag ownership/persistence, achievements, announcements,
MainBoundary, broad regressions and root solution Release remain deferred.

# 2026-08-21 Main ECS migration Task 9 invasion deterministic start position accepted narrowly

Added `WorldInvasionStartPositionSystem` for the deterministic type-4 branch in
`Main.StartInvasion` (`Main.cs:13047-13072`). Martian invasions resolve their initial position to
`spawnTileX - 1.0`; types 1-3 retain an unresolved result because their legacy branch consumes
`Main.rand` and the authoritative random-stream boundary is not established. The policy is
isolated and does not yet mutate or persist invasion runtime state.

Evidence: `Build/diagnostics/main-migration/task-9-invasion-start-position/20260821-224117/`.
WorldRules focused verification, Simulation Release, Server Release and scoped diff check exit `0`;
both builds report `0 warnings, 0 errors`. A full-tree diff check still reports pre-existing
trailing whitespace in unrelated `WorldFile.cs`; no broad regression, MainBoundary or root
solution build was run.

# 2026-08-21 Main ECS migration Task 9 invasion delay spawn integration green narrowly

Connected authoritative invasion facts to the NPC spawn eligibility route. An
`NpcSpawnCandidate` marked `IsInvasionCandidate` now requires `NpcInvasionSpawnState`; nonzero
delay, invalid type/size or missing facts are rejected through `WorldInvasionSpawnEligibilitySystem`.
Ordinary NPC candidates remain unaffected, and a delay-zero invasion candidate reaches the existing
spawn command path.

Evidence: `Build/diagnostics/main-migration/task-9-invasion-delay-spawn-integration/20260821-233247/`.
NPC, WorldRules and Persistence focused verifiers exit `0`; two NPC replay outputs are
byte-identical. Simulation and Server Release builds report `0 warnings, 0 errors`, and scoped diff
check exits `0`. MainBoundary, root Release, complete invasion spawn production and broad suites
remain intentionally unrun, so this card is `green` rather than full acceptance.

# 2026-08-21 Main ECS migration Task 9 invasion delay state owner green narrowly

Connected the accepted delay policy to Simulation state. `WorldProgressionState.InvasionDelayTicks`
is decremented only when `WorldProgressionSystem.Advance` receives the authoritative dawn boundary
(`IsDayTime && TimeOfDay == 0`); midday ticks do not decrement it, and invasion completion clears
it. The field is appended as a v24 persistence tail after the existing v23 invasion-size-start
field. No nonzero initialization source was invented.

Evidence: `Build/diagnostics/main-migration/task-9-invasion-delay-state/20260821-232729/`.
WorldRules, Persistence and NPC focused verifiers exit `0`; two WorldRules replay outputs are
byte-identical. Simulation and Server Release builds report `0 warnings, 0 errors`, and scoped diff
check exits `0`. MainBoundary, root Release, broad suites and delay-driven NPC spawn integration
remain intentionally unrun, so this card is `green` rather than full acceptance.

# 2026-08-21 Main ECS migration Task 9 invasion delay day-start policy green narrowly

Added `WorldInvasionDelaySystem.AdvanceAtDayStart` from `Main.UpdateTime_StartDay`
(`Main.cs:13899-13901`). Positive `invasionDelay` decrements once per day-start policy call, zero
remains zero, and negative state is rejected. The source completion clear at
`Main.UpdateInvasion:12989-12991` is recorded, but delay initialization and runtime wiring remain
unclaimed because no authoritative nonzero assignment was recovered.

Evidence: `Build/diagnostics/main-migration/task-9-invasion-delay-policy/20260821-232034/`.
WorldRules and NPC focused verifiers exit `0`; two WorldRules replay outputs are byte-identical.
Simulation and Server Release builds report `0 warnings, 0 errors`, and scoped diff check exits
`0`. MainBoundary, root Release and broad suites remain intentionally unrun, so this card is
`green` rather than full acceptance.

# 2026-08-21 Main ECS migration Task 9 invasion town-NPC fallback guard green narrowly

Added `WorldInvasionTownFallbackGuardSystem` from `NPC.Spawner.ShouldSpawnInvasionEnemies`
(`NPC.cs:358-370`). The predicate preserves the deterministic preconditions for the fallback:
town-NPC identity, inclusive `maxTilesX / 2 +/- 5` invasion center zone, and strict distance below
3000 pixels. It does not consume the source `Main.rand.Next(3)` branch or enumerate NPC state.

Evidence: `Build/diagnostics/main-migration/task-9-invasion-town-fallback-guard/20260821-231611/`.
WorldRules and NPC focused verifiers exit `0`; two WorldRules replay outputs are byte-identical.
Simulation and Server Release builds report `0 warnings, 0 errors`, and scoped diff check exits
`0`. MainBoundary, root Release and broad suites remain intentionally unrun, so this card is
`green` rather than full acceptance.

# 2026-08-21 Main ECS migration Task 9 invasion position guard green narrowly

Added `WorldInvasionPositionGuardSystem` from `NPC.Spawner.ShouldSpawnInvasionEnemies`
(`NPC.cs:352-366`) and the source `sHeight = 1200` constant. The predicate preserves the source
surface condition and strict `invasionX * 16 +/- 3000` X window, with explicit non-finite input
rejection. Town-NPC fallback and its random branch remain outside this card.

Evidence: `Build/diagnostics/main-migration/task-9-invasion-position-guard/20260821-230021/`.
WorldRules and NPC focused verifiers exit `0`; two WorldRules replays are byte-identical.
Simulation and Server Release builds report `0 warnings, 0 errors`, and scoped diff check exits
`0`. MainBoundary, root Release and broad suites remain intentionally unrun, so this card is
`green` rather than full acceptance.

# 2026-08-21 Main ECS migration Task 9 invasion spawn guard green narrowly

Added `WorldInvasionSpawnEligibilitySystem` from `NPC.Spawner.ShouldSpawnInvasionEnemies`
(`NPC.cs:348-353`). The predicate accepts only a positive invasion type and remaining size with
`invasionDelay == 0`; all other combinations preserve rejection. This card intentionally stops
before source position, world-surface, town-NPC fallback and NPC-table logic.

Evidence: `Build/diagnostics/main-migration/task-9-invasion-spawn-guard/20260821-230021/`.
WorldRules and NPC focused verifiers exit `0`; two WorldRules replay outputs are byte-identical.
Simulation and Server Release builds report `0 warnings, 0 errors`, and scoped diff check exits
`0`. MainBoundary, root Release and broad suites remain intentionally unrun, so the card is
`green` rather than full acceptance. The next candidate is a separately sourced position/range
guard, not full NPC invasion spawning.

# 2026-08-21 Main ECS migration Task 9 invasion original-size state green narrowly

Added `WorldProgressionState.InvasionSizeStart` from the source `Main.StartInvasion` assignment
(`Main.cs:13047-13072`). The authoritative progression route initializes the original size at
start, preserves it while `InvasionSize` decreases, and clears it when the invasion completes.
Pre-v23 snapshots restore `0` as an explicit unknown sentinel; v23 appends the field after the
existing world-event random state so older layouts are not reinterpreted.

Evidence: `Build/diagnostics/main-migration/task-9-invasion-size-start/20260821-225419/`.
WorldRules and Persistence focused verifiers exit `0`; two WorldRules replays have identical
SHA-256 output, Simulation/Server Release builds report `0 warnings, 0 errors`, and scoped diff
check exits `0`. The card remains `green`, not fully accepted, because MainBoundary/root Release
and broad suites were intentionally not run under the current validation scope. Position, delay,
warning, random-side, NPC-table and named progression behavior remain deferred.

# 2026-08-21 Main ECS migration Task 9 invasion completion typed clear event accepted narrowly

`WorldInvasionCompletedEvent` now carries both the source `InvasionType` and its typed
`WorldInvasionClearFlag`. A type-2 completion focused trace verifies the event carries `Frost`.
This connects the accepted source mapping to the runtime completion event without mutating or
persisting named progression flags.

Evidence: `Build/diagnostics/main-migration/task-9-invasion-clear-event/20260821-223756/`.
WorldRules, Simulation Release, Server Release and scoped diff check exit `0`; builds report `0
warnings, 0 errors`. Named progression flag ownership, persistence, achievements, announcements,
MainBoundary, broad regressions and root solution Release remain deferred.
# 2026-08-21 Main ECS migration Task 9 invasion progress projection green narrowly

Added `WorldInvasionProgressProjectionSystem` from `Main.ReportInvasionProgress`
(`Main.cs:12246-12261`). The pure projection computes progress as original size minus remaining
size, preserves the original size as the maximum, and maps the invasion type to icon
`invasionType + 3`. When `InvasionSizeStart == 0`, the result remains explicitly unavailable
instead of fabricating the legacy fallback maximum. Invalid type and inconsistent size states are
also rejected before projection. Client UI, `NetMessage.SendData(78)` and display timers remain
deferred.

Evidence:
`Build/diagnostics/main-migration/task-9-invasion-progress-projection/20260821-233919/`.
WorldRules, Persistence and NPC focused verifiers exit `0`; two WorldRules replay outputs have
identical SHA-256 `E3B4AFE45945F82A2EEE0AC3FC6BC6791263E78AA9B772F77B3FDA94F0A70319`.
Simulation and Server Release builds exit `0`; MainBoundary, root Release and broad suites remain
intentionally unrun, so this card is `green` rather than full acceptance.

# 2026-08-21 Main ECS migration Task 9 invasion X state green narrowly

Restored the legacy WLD `Main.invasionX` double through
`LegacyWorldMetadata -> CompatibilityWorldMetadata -> CompatibilityToDomeProjection ->
WorldProgressionState`. The v319 and legacy readers now retain the source double instead of
discarding it. Dome persistence advances to v25 and appends `InvasionX` after the v24 delay tail;
older formats default to `0` without reinterpreting earlier bytes. Non-finite progression values
are rejected. Random start-side selection, NPC spawn tables, announcements and full travel runtime
integration remain separate deferred branches.

Evidence:
`Build/diagnostics/main-migration/task-9-invasion-x-state/20260821-234954/`.
WorldImport, WorldFile V319, Persistence, WorldRules and NPC focused verifiers exit `0`; two
WorldImport replay outputs have identical SHA-256
`9EDF22FBE560F33A2621DAC7CFFD879A88B997EF556821393E0B5E3FE5E68024`.
Simulation and Server Release builds exit `0`, and scoped diff check exits `0`. MainBoundary, root
Release and broad suites remain intentionally unrun, so this card is `green` rather than full
acceptance.

# 2026-08-22 Main ECS migration Task 9 protocol invasion clear flags green narrowly

Restored the server-owned V1456 WorldData projection for named invasion clear flags from the
legacy `NetMessage` mapping: `EventFlags8` bit 6 carries Martians and `EventFlags10` bits 0, 1, 2
carry Pirates, Frost and Goblins. The existing field order and 173-byte payload length remain
unchanged; unrelated event bits are preserved. Client runtime and full network loopback behavior
remain separate cards.

Evidence:
`Build/diagnostics/main-migration/task-9-protocol-invasion-clear-flags/20260822-001313/`.
Protocol Compatibility, WorldRules and Persistence focused verifiers exit `0`; two protocol
replay outputs have identical SHA-256
`8048F2D9421947BC514BCD0A14C35BD3460A6F30EB4586D586957841119C428C`.
Simulation and Server Release builds exit `0`, and scoped diff check exits `0`. MainBoundary, root
Release, full loopback and broad suites remain intentionally unrun, so this card is `green` rather
than full acceptance.

# 2026-08-22 Main ECS migration Task 9 WLD invasion clear flags green narrowly

Restored WLD named invasion clear flags through the reader and compatibility path. V319 reads
`downedGoblins`, `downedFrost` and `downedPirates` from the source header boolean group, while the
later-field reader captures `downedMartians` from the source post-banner event-flag group. Legacy
versions default flags that did not yet exist to false. All four values now project into Dome
progression alongside the completion-owned named facts. Protocol bit packing, client projection,
achievements and announcements remain separate cards.

Evidence:
`Build/diagnostics/main-migration/task-9-wld-invasion-clear-flags/20260822-000856/`.
WorldImport, WorldFile V319, Persistence, WorldRules and NPC focused verifiers exit `0`; two
WorldImport replay outputs have identical SHA-256
`9EDF22FBE560F33A2621DAC7CFFD879A88B997EF556821393E0B5E3FE5E68024`.
Simulation and Server Release builds exit `0`, and scoped diff check exits `0`. MainBoundary, root
Release and broad suites remain intentionally unrun, so this card is `green` rather than full
acceptance.

# 2026-08-22 Main ECS migration Task 9 coverage ledger refreshed after invasion flag cards

Updated M-009 in `docs/research/2026-08-20-main-member-coverage-matrix.md` to reflect the current
source-backed narrow cards: invasion type/size, qualified-player readiness, travel/delay/position/
warning policies, progress projection, typed completion, named clear flags, WLD import and V1456
flag projection are now accounted for. Full runtime travel integration, random start semantics,
NPC tables, player-stat production and complete invasion lifecycle parity remain open, so M-009
stays `unknown` rather than being promoted to accepted.

# 2026-08-22 Main ECS migration Task 9 MainThreadAction boundary decision

Audited `Main.QueueMainThreadAction/ConsumeAllMainThreadActions`
(`Main.cs:11541-11553,11577`). The legacy `ConcurrentQueue<Action>` mixes client section/UI work
with background WorldGen follow-ups (`WorldGen.cs:10529,26122`), so arbitrary delegates have no
single deterministic server authority. M-014 remains explicitly `unknown/deferred`; no generic
Action queue was added to Simulation. The existing typed `SimulationCommandQueue` remains the
server-owned boundary, while future WorldGen follow-ups require separately typed contracts.

Evidence:
`Build/diagnostics/main-migration/task-9-main-thread-action-boundary/20260822-001736/`.
TickOrder and Wiring/Liquid/Chest command-contract focused verifiers exit `0`, and scoped diff
check exits `0`. No MainBoundary, root Release or broad suites were run.

# 2026-08-22 Main ECS migration Task 9 world preparation boundary decision

Audited `Main.UpdateWorldPreparationState` (`Main.cs:2171-2176,11623,11812`). The source method
only marks the client/UI `_worldPreparationState` as `Ready`; it does not own server gameplay
state. M-015 is now explicitly `intentionally excluded` for client preparation. The server route
is ready-by-construction through `WorldBootstrap.Load/CreateDefault`, which completes validated
import or default generation before constructing `DomeServer`; no duplicate preparation state
machine was added.

Evidence:
`Build/diagnostics/main-migration/task-9-world-preparation-boundary/20260822-001918/`.
WorldImport focused verification and Server Release build exit `0`; scoped diff check exits `0`.
Client loading/UI sequencing, MainBoundary, root Release and broad suites remain outside the
focused-only validation scope.

# 2026-08-22 Main ECS migration Task 9 WLD clock fraction boundary blocked

Reconfirmed M-007 against the source: WLD stores `_tempTime` as `Double` and `_tempDayTime` as
`Boolean` (`WorldFile.cs:1312-1313,2125-2126`), while legacy time advances with `time += dayRate`
(`Main.cs:13525-13527`). The current ECS clock is integral `Int32 TimeOfDay`; no source-backed
truncation, rounding or fractional restart contract exists. M-007 is therefore marked `blocked`
for the current representation, with fail-closed WLD time/day-time import. A future versioned
fractional clock must prove packet, event-boundary and restart behavior before import.

Evidence:
`Build/diagnostics/main-migration/task-9-wld-clock-fraction-boundary/20260822-002106/`.
WorldClock and WorldRules focused verifiers exit `0`; scoped diff check exits `0`. MainBoundary,
root Release and broad suites remain intentionally unrun.

# 2026-08-21 Main ECS migration Task 9 invasion warning message projection green narrowly

Added `WorldInvasionWarningMessageSystem` from `Main.InvasionWarning`
(`Main.cs:12958-13037`). The pure server-side projection maps completion, approaching, receding
and arrival to typed warning kinds, and preserves the source behavior that Martian invasions have
no warning while travelling. Invalid type, negative size and non-finite position inputs resolve to
`None`. Localized text, chat broadcast, network packets, warning-counter persistence and full
travel integration remain deferred.

Evidence:
`Build/diagnostics/main-migration/task-9-invasion-warning-message/20260821-235450/`.
WorldRules and NPC focused verifiers exit `0`; two WorldRules replay outputs have identical SHA-256
`1EE7E302EC596FE239E52B7D7EBBE6699020E0DFC5602BBBC666BAA8C736F2A8`. Simulation and Server
Release builds exit `0`, and scoped diff check exits `0`. MainBoundary, root Release and broad
suites remain intentionally unrun, so this card is `green` rather than full acceptance.

# 2026-08-22 Main ECS migration Task 9 invasion clear flags state green narrowly

Promoted the typed invasion completion mapping into authoritative named progression state.
`WorldProgressionState` now owns `DefeatedGoblins`, `DefeatedFrost`, `DefeatedPirates` and
`DefeatedMartians`; completion normalization resolves the flag once, sets the matching fact and
publishes the typed completion event. Dome persistence advances to v26 and appends the four flags
after the v25 `InvasionX` tail; older formats default them to false. WLD flag import,
achievements, announcements and protocol/client projection remain separate cards.

Evidence:
`Build/diagnostics/main-migration/task-9-invasion-clear-flags-state/20260822-000208/`.
WorldRules, Persistence and NPC focused verifiers exit `0`; two WorldRules replay outputs have
identical SHA-256 `1EE7E302EC596FE239E52B7D7EBBE6699020E0DFC5602BBBC666BAA8C736F2A8`.
Simulation and Server Release builds exit `0`, and scoped diff check exits `0`. MainBoundary, root
Release and broad suites remain intentionally unrun, so this card is `green` rather than full
acceptance.
# 2026-08-22 Main ECS migration Task 9 WLD rain runtime boundary blocked

M-008 was audited against the legacy `UpdateWeather`, `StartRain`, `StopRain` and
`ChangeRain` paths. WLD `raining`, `rainTime` and `maxRaining` remain losslessly available in
the parser and compatibility metadata, but `CompatibilityToDomeProjection` intentionally
does not turn them into authoritative `WorldRuleState` rain transitions. Legacy
`maxRaining` feeds wind smoothing, and `FixEndlessRainWorlds()` has a versioned
secret-seed exception; neither contract is represented well enough to import without
inventing behavior.

The focused decision is **blocked** at the source-model boundary. Evidence is in
`Build/diagnostics/main-migration/task-9-wld-rain-runtime-boundary/20260822-003000/`.
WorldImport, WorldRules and the scoped diff check all exit `0`. MainBoundary, root Release,
loopback and broad regression suites were not run under the focused-validation request.

# 2026-08-22 Main ECS migration Task 9 WLD Header disposition matrix recorded

Task 1.1 is recorded in `docs/research/2026-08-20-wld-header-disposition-matrix.md`. The
matrix enumerates the modern and legacy Header reads, version boundaries, unique immutable
owners and explicit `candidate`, `blocked`, `no-owner` and `client-only` dispositions. It
references the existing parser fixtures for rain, wind, progression, meteor and layout offset
continuity without adding gameplay code.

# 2026-08-22 Main ECS migration Task 9 moon phase/day boundary blocked

M-012 was audited against `Main.GetMoonPhase()` and `Main.IsItDay()`. The stored moon phase has
an exact `WorldClock` owner, but `IsItDay()` returns false for `remixWorld` regardless of the
ordinary `dayTime` value. The current ECS clock has no world-variant state, so it would be
incorrect to claim full day predicate parity or invent a default variant during import.

Evidence: `Build/diagnostics/main-migration/task-9-moon-phase-day-boundary/20260822-005000/`.
WorldClock focused validation and scoped documentation diff check are recorded as exit `0`.

# 2026-08-22 Main ECS migration Task 9 moon phase/day rule accepted narrowly

The pure M-012 rule is already owned by `WorldTimeClassificationSystem`. Its moon-phase
conversion validates the source range 0..7, and its day predicate takes `isRemixWorld`
explicitly so the legacy override is preserved without inventing import state. The WLD source
variant itself remains a separate deferred import concern.

Evidence: `Build/diagnostics/main-migration/task-9-moon-phase-day-boundary/20260822-005500/`.
NPC and WorldClock focused verifiers exit `0`; scoped diff check exits `0`. MainBoundary, root
Release and broad suites remain intentionally unrun.

# 2026-08-22 Main ECS migration Task 9 meteor impact resolution accepted narrowly

M-011 is split at the source authority boundary. `WorldMeteorImpactSystem` accepts an explicit
impact coordinate and performs deterministic bounds, active-entity safety, protected-tile and
meteorite-cap checks before emitting tile commands. The focused WorldRules trace proves the
impact tile set is deterministic across peers and survives snapshot continuation; Persistence
proves the committed world state remains present after reload.

Legacy `HandleMeteorFall` scheduling, `WorldGen.dropMeteor()` candidate search, low-tiles
meteor-shower fallback, ambience presentation and global random ordering remain deferred.
Evidence: `Build/diagnostics/main-migration/task-9-meteor-impact-resolution/20260822-010000/`.

Evidence:
`Build/diagnostics/main-migration/task-9-wld-header-matrix/20260822-004000/`.

# 2026-08-22 Main ECS migration Task 9 combat damage rules accepted narrowly

M-013's pure source formulas are accounted for by existing Simulation owners. NPC mitigation is
`max(1, rawDamage - defense * 0.5)`, player difficulty multipliers remain explicit in
`DamageCalculationSystem`, and `DamageVariationSystem` preserves legacy +/-15% steps, luck
max/min selection and the disabled-variation branch through an explicit random contract.

Evidence: `Build/diagnostics/main-migration/task-9-combat-damage-rules/20260822-011000/`.
Combat focused verification and scoped diff check exit `0`. Weapon/type tables, critical and
immunity modifiers, hitbox/knockback behavior and global random ordering remain deferred.

# 2026-08-22 Main ECS migration Task 9 Slime Rain eligibility boundary accepted narrowly

M-010's existing state machine covers explicit start/stop authority, ordinary Rain mutual
exclusion, duplicate and invalid requests, active-only stop, cooldown/restart gating, warning
events, persistence and snapshot continuation. Legacy `StartSlimeRain` also depends on
`remixWorld` and `isThereAWorldSurface`; those are not currently authoritative Simulation inputs,
so the corresponding import guards remain deferred rather than defaulted.

Evidence: `Build/diagnostics/main-migration/task-9-slime-rain-eligibility-boundary/20260822-012000/`.
WorldRules, Persistence and scoped diff checks exit `0`. Random duration/cooldown and client chat
side effects remain outside this narrow acceptance.

# 2026-08-22 Main ECS migration Task 9 world path orchestration boundary accepted narrowly

M-016 is accepted at the current server contract: `ServerLaunchOptions` receives an explicit
`--world` path, `WorldBootstrap.Load` requires it to be fully qualified, and
`DomeWorldImportApplier` owns the WLD-to-immutable-snapshot import. Filesystem naming,
collision suffixing, cloud/local roots and interactive selection remain outside Simulation and
are deferred rather than reimplemented.

Evidence: `Build/diagnostics/main-migration/task-9-world-path-boundary/20260822-013000/`.
WorldImport, World.Server and scoped diff checks exit `0`.

# 2026-08-22 Main ECS migration Task 9 world save/load boundary accepted narrowly

M-017's value-only persistence boundary is accepted. WLD input is owned by the Server import
adapter; grid and full Simulation snapshots use the versioned persistence formats, with strict
recovery, atomic replacement, old-format handling and restart adoption covered by focused tests.
Legacy WLD write-back, async loading progress/UI, cloud saves and server-exit scheduling remain
Server orchestration work and are not copied into Simulation.

Evidence: `Build/diagnostics/main-migration/task-9-world-save-load-boundary/20260822-014000/`.
Persistence, WorldImport and scoped diff checks exit `0`.

# 2026-08-22 Main ECS migration Task 9 tile command commit boundary accepted narrowly

M-022's mutable `Main.tile[,]` ownership is replaced only at the command/commit boundary.
`TileChangeCommitSystem` deterministically orders immutable commands, rejects negative or
duplicate sequences, invalid kinds and out-of-world coordinates, applies mutations and updates
only affected section revisions. Higher-level domains enqueue commands and do not own a mutable
global tile array.

Evidence: `Build/diagnostics/main-migration/task-9-tile-command-commit-boundary/20260822-015000/`.
TileInteraction, WorldRules and scoped diff checks exit `0`. Multitile framing, liquids, legacy
WorldGen rules, network side effects and complete tile-flag parity remain deferred.

# 2026-08-22 Main ECS migration Task 9 tick order world phase boundary accepted narrowly

M-005 is accepted only for the named ECS schedule and its falsifiable invariants. The current
TickOrder verifier proves exact phase traces, paused-tick short-circuit, deterministic
equal-sequence command ordering and world progression completion before projectile advancement.
Legacy `DoUpdateInWorld` remains a mixed loop with entity, WorldGen, network and presentation
work; complete Main loop parity is not claimed.

Evidence: `Build/diagnostics/main-migration/task-9-tick-order-world-phase-boundary/20260822-016000/`.
TickOrder, WorldRules and scoped diff checks exit `0`.

# 2026-08-22 Main ECS migration Task 9 server tick boundary accepted narrowly

M-006's server host boundary is owned by `DomeServer.SimulationLoopAsync`: bounded protocol
command draining, input normalization, ECS tick, tile commit, immutable snapshot publication,
domain replication and network-isolation flush occur in one server-owned loop. Queue overload is
bounded before simulation mutation.

Legacy `UpdateServer`'s 3600/900/5 cadence, client timeout arrays, spam updates and periodic
WorldData broadcast timing remain deferred. Evidence:
`Build/diagnostics/main-migration/task-9-server-tick-boundary/20260822-017000/`.
NetworkIsolation, World.Server and scoped diff checks exit `0`.

# 2026-08-22 Main ECS migration Task 9 server bootstrap boundary accepted narrowly

M-004 is accepted at the server startup boundary. `ServerLaunchOptions` validates the explicit
absolute `.wld` path and port; `WorldBootstrap.Load` performs strict import before the listener is
opened; `WorldBootstrap.CreateDefault` produces deterministic metadata, tile state and 23
server-owned chests; and `DomeServer` consumes the result without recreating those objects.

Evidence: `Build/diagnostics/main-migration/task-9-server-bootstrap-boundary/20260822-005000/`.
The focused WorldImport verifier and scoped diff check exit `0`. Full legacy `DedServ` parity,
interactive world/new-world menus, cloud/path naming policy, port forwarding, password handling,
save scheduling and 3600/900/5 server cadence remain deferred under focused-only validation.

# 2026-08-22 Main ECS migration Task 9 delayed process semantics deferred

M-024 remains `unknown/deferred` after tracing `Main.DelayedProcesses` and
`Main.DelayedProcessesInGame`. They are arbitrary `IEnumerator` lists advanced inline in a mixed
client/UI, ambience and server frame loop, with no source-backed server-only identity, phase,
cancellation or restart contract. The migration therefore keeps typed `SimulationCommandQueue` and
named tick phases as the authority boundary and adds no generic coroutine queue.

Evidence: `Build/diagnostics/main-migration/task-9-delayed-process-boundary/20260822-005500/`.
The focused TickOrder verifier and scoped diff check exit `0`; caller inventory and typed delayed
operation cards remain future work.

# 2026-08-22 Main ECS migration Task 9 item definition registration boundary accepted narrowly

M-003 is accepted only for the current supported server item behavior. The immutable
`ItemDefinitionRegistry` and `ItemDefinitionCompiler` enforce unique/non-zero types, stack limits,
dependent references, ammunition consistency and the supported combat, placement, recovery,
equipment and Extractinator contracts. The focused Definitions verifier exits `0`.

Version4 `Initialize_Items` still creates the complete `ItemID` table and populates staff/claw and
client-facing metadata arrays; those defaults, visual/UI families and unsupported item types remain
deferred under the item-specific migration plan. Evidence:
`Build/diagnostics/main-migration/task-9-item-definition-registration-boundary/20260822-010000/`.

# 2026-08-22 Main ECS migration Task 9 entity ownership initialization boundary accepted narrowly

M-002 is accepted only at the entity ownership boundary. Domain-specific Player, NPC, Projectile
and WorldItem stores keep stable value identities separate from internal Arch entities; Player
ownership/lifecycle verification proves duplicate and idempotent store behavior, inactive snapshot
tombstones and destruction cleanup, while the NPC boundary verifier scans 597 Simulation source
files and finds zero forbidden legacy NPC dependencies.

Evidence: `Build/diagnostics/main-migration/task-9-entity-ownership-initialization-boundary/20260822-010500/`.
Full entity lifecycle, AI/motion/collision/spawn-table parity and the legacy client arrays for dust,
rain, cloud, gore, combat text and popup text remain deferred.

# 2026-08-22 Main ECS migration Task 9 domain-scoped random stream accepted narrowly

M-023 is accepted only for the server-owned world-event random stream. `WorldEventRandomState`
advances through explicit bounded transitions, survives `DomeSimulationSnapshot`, and is persisted
exactly by the v22 tail; older formats use the documented seed fallback and malformed tails reject.
The source global `Main.rand` call order is not recoverable as a durable contract, so legacy global
ordering, item/loot random paths and probabilistic events remain deferred.

# 2026-08-22 Main ECS migration Task 9 Initialize_AlmostEverything disposition recorded

M-001 remains `planned` after decomposing `Initialize_AlmostEverything` into domain families. The
source entry point mixes already-covered entity and item slices with tile/NPC/projectile static
tables, liquid buffers, content databases, WorldGen hooks, network startup and client-only catalogs.
No generic Simulation initializer was added. Each unresolved family requires its own source-backed
owner and focused verifier.

Evidence: `docs/research/2026-08-22-main-initialize-almost-everything-disposition.md` and
`Build/diagnostics/main-migration/task-9-main-initialize-disposition/20260822-011000/scenario.yaml`.

# 2026-08-22 Main ECS migration Task 9 invasion travel integration boundary deferred

The source `UpdateInvasion` advances `invasionX` by `max(dayRate, 1f)` and drives warning cadence.
The ECS currently has a pure `WorldInvasionTravelSystem`, but `DomeSimulation.Tick` does not integrate
it and `WorldClock.TicksPerUpdate` is not source-proven equivalent to legacy `dayRate`. Because M-007
also lacks a lossless fractional time contract, the travel transition remains deferred instead of
inventing movement speed or arrival timing.

Evidence: `Build/diagnostics/main-migration/task-9-invasion-travel-integration-boundary/20260822-012000/`.
WorldRules focused verification exits `0`; full invasion lifecycle remains `unknown`.

# 2026-08-22 Focused validation scope reduced to approximately 40 percent

For the remaining execution of `docs/plans/2026-08-20-remaining-ecs-migration-execution.md`,
validation is intentionally reduced to approximately 40 percent of the original test workload.
Each card keeps only its smallest decisive focused verifier or an existing evidence review. Duplicate
replays, broad regression matrices, loopback suites, MainBoundary and root Release are not run unless
the user expands the scope. This is a validation-scope reduction only; it does not remove tests or
promote unverified behavior to accepted status.

The WorldGeneration verifier provides the concrete reduced entry point:
`--reduced` runs 32 of its 81 independent `PASS` sections (39.5%, rounded to the requested
40% scope) and exits at the explicit boundary. A fresh source count confirmed all 81 sections
remain present; no test source was deleted or disabled. The full verifier remains available
without `--reduced`, while build, runtime differential and broad regression execution stay
paused by the migration proposal.

# 2026-08-22 WorldGen remaining execution CheckOnTable1x1 partial mapping

`CheckOnTable1x1` now has a pure `OnTable1x1ValidationQuery` over immutable snapshots. The slice
covers unsupported half-brick/top-slope shapes, platform-side join input, ordinary solid support,
and the type-78 bottom-slope branch. Legacy `AnchorValid(Table)`, `Main.tileTable` registry lookup,
`KillTile`, and host-side effects remain deferred. Inventory and deletion-gate counts are now
`184 Partial / 500 Unmapped`, with `canRemoveLegacyWorldGen=false`.

The full WorldGeneration verifier contains a deterministic contract check after the reduced exit
boundary; it was not run in this turn because the proposal keeps build, verifier, and runtime
differential execution paused. Static checks passed for JSON parsing, forbidden Simulation
dependencies, new-file width/trailing whitespace, and `git diff --check`.

# 2026-08-22 WorldGen remaining execution CheckSunflower partial mapping

`CheckSunflower` now has a pure `SunflowerValidationQuery` over immutable snapshots. The slice
covers the 2x4 footprint, frame-column/row consistency, allowed ground IDs, and solid support.
`destroyObject`, `KillTile`, item drops, and recursive framing remain deferred. Inventory and
deletion-gate counts are now `185 Partial / 499 Unmapped`; `canRemoveLegacyWorldGen=false`.

# 2026-08-22 WorldGen remaining execution CheckGnome partial mapping

`CheckGnome` now has a pure `GnomeValidationQuery` over immutable snapshots. The slice covers
the type-567 1x2 frame footprint and solid-or-platform ground support. `destroyObject`, `KillTile`,
item drops, and recursive framing remain deferred. Inventory and deletion-gate counts are now
`186 Partial / 498 Unmapped`; `canRemoveLegacyWorldGen=false`.

# 2026-08-22 WorldGen remaining execution CheckAnchor partial mapping

`CheckAnchor` now has a pure `AnchorOrientationValidationQuery` over immutable snapshots. The
slice covers deterministic bottom/top/left/right attachment selection and explicit wall fallback.
The empty `ConsideredSolidTileForAnchor` helper, frame mutation, and host-side effects remain
deferred. Inventory and deletion-gate counts are now `187 Partial / 497 Unmapped`;
`canRemoveLegacyWorldGen=false`.

# 2026-08-22 WorldGen remaining execution CheckStinkbugBlocker partial mapping

`CheckStinkbugBlocker` now has a pure `StinkbugBlockerValidationQuery` that reuses the anchor
orientation contract and preserves the legacy horizontal-style handling as explicit result data.
The empty considered-solid helper, frame mutation, and `KillTile` remain deferred. Inventory and
deletion-gate counts are now `188 Partial / 496 Unmapped`; `canRemoveLegacyWorldGen=false`.

# 2026-08-22 WorldGen remaining execution CheckChand partial mapping

`CheckChand` now has a pure `ChandelierValidationQuery` over immutable snapshots. The slice
covers the type-dependent 3-or-4-wide, height-3 active/type footprint and upper solid support.
Style bands, random drops, `destroyObject`, and `KillTile` remain deferred. Inventory and
deletion-gate counts are now `189 Partial / 495 Unmapped`; `canRemoveLegacyWorldGen=false`.

# 2026-08-22 WorldGen remaining execution CheckPot partial mapping

`CheckPot` now has a pure `PotValidationQuery` that reuses the generic 2x2 footprint/support
contract and records the type-653 bottom-slope branch explicitly. Style/sound bands, random gore
and drops, `destroyObject`, `KillTile`, and recursive framing remain deferred. Inventory and
deletion-gate counts are now `190 Partial / 494 Unmapped`; `canRemoveLegacyWorldGen=false`.

# 2026-08-22 WorldGen remaining execution CheckPalmTree partial mapping

`CheckPalmTree` now has a pure `PalmTreeValidationQuery` over immutable snapshots. The slice
covers ground-type normalization for palm-compatible types, support eligibility, special frame
constraints, and deterministic frame-correction intent. Random frame selection, `KillTile`, and
recursive framing remain deferred. Inventory and deletion-gate counts are now
`191 Partial / 493 Unmapped`; `canRemoveLegacyWorldGen=false`.

# 2026-08-22 WorldGen remaining execution CheckTree partial mapping

`CheckTree` now has a pure `TreeFrameValidationQuery` over immutable snapshots. The slice covers
ordinary tree ground normalization, cardinal tree neighbors, and bounded branch-frame selection.
Complete tree settings, `KillTile`, random tree framing, and recursive framing remain deferred.
Inventory and deletion-gate counts are now `192 Partial / 492 Unmapped`;
`canRemoveLegacyWorldGen=false`.

# 2026-08-22 WorldGen remaining execution CheckTreeWithSettings partial mapping

`CheckTreeWithSettings` now has a pure `TreeSettingsValidationQuery` with an explicit injected
ground-valid predicate. The slice covers below-tile classification and cardinal same-tree neighbors;
style-specific frame mutation, `KillTile`, and recursive framing remain deferred. Inventory and
deletion-gate counts are now `193 Partial / 491 Unmapped`; `canRemoveLegacyWorldGen=false`.

# 2026-08-22 WorldGen remaining execution CheckSpecialTownNPCSpawningConditions partial mapping

`CheckSpecialTownNPCSpawningConditions` now has a pure `SpecialTownNpcSpawningQuery` with explicit
inputs for NPC type, Truffle unlock/surface state, mushroom tile count, and threshold. Non-Truffle
allowance and Truffle eligibility are covered; room scanning, NPC scheduling, and host state remain
deferred. Inventory and deletion-gate counts are now `194 Partial / 490 Unmapped`;
`canRemoveLegacyWorldGen=false`.

# 2026-08-22 WorldGen remaining execution CheckAchievement_RealEstateAndTownSlimes partial mapping

`CheckAchievement_RealEstateAndTownSlimes` now has a pure `TownAchievementEligibilityQuery` over
an explicit active-NPC type collection. The real-estate and town-slime required sets are counted
without importing Main/NPC arrays; achievement notifications and host side effects remain deferred.
Inventory and deletion-gate counts are now `195 Partial / 489 Unmapped`;
`canRemoveLegacyWorldGen=false`.

# 2026-08-22 WorldGen remaining execution checkUnderground partial mapping

`checkUnderground` now has a pure `UndergroundClassificationQuery` over immutable snapshots. The
slice covers the deep/shallow shortcuts and bounded 120x3 solid-density scan with explicit world
surface, tile definitions, and wall fallback inputs. Legacy Main fields and exception-swallowing
behavior remain deferred. Inventory and deletion-gate counts are now `196 Partial / 488 Unmapped`;
`canRemoveLegacyWorldGen=false`.

# 2026-08-22 WorldGen remaining execution CheckRoom partial mapping

`CheckRoom` now has a pure `RoomBoundaryValidationQuery` for world-edge, room-tile bounds,
tile-count, and room-size gates. Recursive room scanning, feedback callbacks, housing/wall
registries, NPC scheduling, and host state remain deferred. Inventory and deletion-gate counts are
now `197 Partial / 487 Unmapped`; `canRemoveLegacyWorldGen=false`.

# 2026-08-22 WorldGen remaining execution CheckInputForSecretSeed partial mapping

`CheckInputForSecretSeed` now has a pure `SecretSeedInputQuery` for alphanumeric normalization,
display-input sanitization, and explicit plaintext/code candidate matching. The `Secrets.ToSecret`
cryptographic transform, legacy SecretSeed mutation, sounds, and host side effects remain deferred.
Inventory and deletion-gate counts are now `198 Partial / 486 Unmapped`;
`canRemoveLegacyWorldGen=false`.

# 2026-08-22 WorldGen remaining execution IsBackgroundConsideredTheSame partial mapping

`IsBackgroundConsideredTheSame` now maps to pure `BackgroundEquivalenceQuery.AreEquivalent`.
The fixed legacy groups (3/31, 5/51, 7/71-73) and exact-match fallback are covered without
runtime state or side effects. Inventory and deletion-gate counts are now
`199 Partial / 485 Unmapped`; `canRemoveLegacyWorldGen=false`.

# 2026-08-22 WorldGen remaining execution GetNextJungleChestItem partial mapping

`GetNextJungleChestItem` now has a pure `JungleChestItemSelectionQuery` for the deterministic
four-item rotation and next-counter projection. Rare random-item overrides and legacy counter
mutation remain deferred. Inventory and deletion-gate counts are now `200 Partial / 484 Unmapped`;
`canRemoveLegacyWorldGen=false`.

# 2026-08-22 WorldGen remaining execution SecretSeed.Check partial mapping

The `SecretSeed.Check` entry now has a pure `SecretSeedCodeCheckQuery` for input normalization,
empty-input rejection, and explicit expected-code comparison. The `Secrets.ToSecret` transform,
registry lookup, and SecretSeed mutation remain deferred. Inventory and deletion-gate counts are
now `201 Partial / 483 Unmapped`; `canRemoveLegacyWorldGen=false`.

# 2026-08-22 WorldGen remaining execution tile solidity override projections

`SetBoulderSolidity` and `SetCrackedBrickSolidity` now share a pure
`TileSolidityOverrideQuery` projection of their fixed Tile ID sets and requested solid value.
`Main.tileSolid` mutation remains deferred. Inventory and deletion-gate counts are now
`203 Partial / 481 Unmapped`; `canRemoveLegacyWorldGen=false`.

# 2026-08-22 WorldGen remaining execution alchemy herb harvestability partial mapping

`IsAlchemyPlantHarvestable` and `IsHarvestableHerbWithSeed` now have pure Simulation predicates
with explicit day/night, weather, time, surface, and alchemy-result inputs. Type 83/84 seed rules
and styles 0/1/3/4/5 are covered; Main-backed environment reads remain deferred. Inventory and
deletion-gate counts are now `205 Partial / 479 Unmapped`; `canRemoveLegacyWorldGen=false`.

# 2026-08-22 WorldGen remaining execution IsChestRigged partial mapping

`IsChestRigged` now maps to pure `ChestRiggingQuery.IsRigged`, covering the type-467 and
frame-band-4 classification without Main/chest mutation. Inventory and deletion-gate counts are
now `206 Partial / 478 Unmapped`; `canRemoveLegacyWorldGen=false`.

# 2026-08-22 WorldGen remaining execution IsThereASpawnablePrioritizedTownNPC partial mapping

`IsThereASpawnablePrioritizedTownNPC` now has a pure `TownNpcSpawnSelectorQuery` over explicit
candidate state and occupant order. Occupant priority, room/pet fallback, and prioritized fallback
are covered; TownManager discovery, Main NPC arrays, room scanning, and mutation remain deferred.
Inventory and deletion-gate counts are now `207 Partial / 477 Unmapped`;
`canRemoveLegacyWorldGen=false`.

The weather-state representation received one reduced-scope persistence recheck:
`PASS: world persistence round-trip, strict recovery and atomic replacement` (exit `0`). This
confirms the v21 raw representation only; WLD rain projection and `FixEndlessRainWorlds()` repair
semantics remain blocked as recorded by M-008.

# 2026-08-22 Main ECS migration Task 9 static Tile definition boundary accepted narrowly

The source-derived `TileDefinitionRegistry` now has a focused evidence child under M-001. Its
Version4 registry covers all 753 Tile IDs and supplies immutable solid/platform/no-attach and
water/lava destruction facts to supported Simulation consumers; unknown definitions fail closed.
The Liquid focused verifier exits `0`.

Evidence: `Build/diagnostics/main-migration/task-9-static-tile-definition-boundary/20260822-013000/`.
Legacy mutable TileID/Main arrays, framing/animation/multi-tile data and complete NPC static tables
remain deferred; M-001's mixed initializer is still `planned`.

# 2026-08-22 Main ECS migration Task 9 supported projectile definition boundary accepted narrowly

The current supported projectile slice uses immutable typed `ProjectileDefinition` values resolved
through `ProjectileDefinitionRegistry` before lifecycle and combat processing. The focused Combat
verifier exits `0` for bounded player vitals, authoritative damage, collision, cooldown and loot.

Evidence: `Build/diagnostics/main-migration/task-9-supported-projectile-definition-boundary/20260822-014000/`.
Complete `ProjectileID` defaults, `InitializeStaticThings`, AI styles, frame tables and hostile/hook
arrays remain deferred; M-001's mixed initializer remains `planned`.

# 2026-08-22 Main ECS migration Task 9 supported NPC definition boundary accepted narrowly

The current supported NPC slice uses immutable typed `NpcDefinition` records and
`NpcDefinitionRegistry` before target/behavior/death/loot processing. The focused NPC verifier exits
`0` for the source contract, target/chase baseline, death revision and deterministic loot.

Evidence: `Build/diagnostics/main-migration/task-9-supported-npc-definition-boundary/20260822-015000/`.
Complete NPC `SetDefaults`/static tables, boss/event identity mapping, AI families and spawn tables
remain deferred; M-001's mixed initializer and M-019 full lifecycle remain open.

Evidence: `docs/plans/2026-08-20-simulation-random-stream-contract.md` and
`Build/diagnostics/main-migration/task-9-random-stream-contract/20260820-000009/scenario.yaml`.

# 2026-08-22 Main ECS migration Task 9 Slime Rain metadata eligibility closed narrowly

M-010 now carries the source-backed `remixWorld` bit from WLD v249+ through compatibility
projection and `WorldMetadata`. `WorldSlimeRainEligibilitySystem` rejects Remix worlds, missing or
invalid surfaces and unknown variants; the unique `DomeSimulation.TryQueueWorldSlimeRain` route
enforces that boundary whenever imported metadata is known. WorldPersistence v4 and embedded Dome
world payloads preserve the value across restart, while older formats recover it as unknown.

Focused WorldRules, WorldImport, WorldFile parser and Persistence verifiers exit `0`. Evidence:
`Build/diagnostics/main-migration/task-9-slime-rain-eligibility-boundary/20260822-080000/`,
`20260822-070000/`, `20260822-050000/`.

Random duration/cooldown generation, client announcements, complete weather scheduling and legacy
NPC/client behavior remain deferred. This is a narrow M-010 child acceptance, not full event parity.

The parser boundary is also covered by a real version fixture: v248 restores Remix as unknown and
v249 restores the header bit as `true`. Evidence:
`Build/diagnostics/main-migration/task-9-slime-rain-eligibility-boundary/20260822-110000/`.

Persistence compatibility was rechecked after the v4 extension: a v3 fixture retains its existing
WorldSurface and restores IsRemixWorld as unknown, while v4 retains the optional Remix marker.
Evidence: `Build/diagnostics/main-migration/task-9-slime-rain-eligibility-boundary/20260822-120000/`.

# 2026-08-22 Main ECS migration Task 9 MainBoundary classification contract repaired

The Main coverage matrix used `blocked` and `intentionally excluded` labels that the executable
MainBoundary gate does not accept. Those rows now use the allowed `unknown` and `excluded` statuses,
with blocker/exclusion semantics retained in the evidence column. MainBoundary focused verification
now scans 640 Simulation files with 0 violations and exits `0`.

Evidence: `Build/diagnostics/main-migration/task-9-main-member-coverage/20260822-130000/`.
This repairs Task 5.1 accounting consistency only; it does not claim gameplay parity or root Release.

The M-010 coverage-matrix row now reflects the implemented v249+ Remix/surface guard path instead
of the superseded deferred wording. MainBoundary recheck scans 644 Simulation files with 0
violations and exits `0`.
Evidence: `Build/diagnostics/main-migration/task-9-main-member-coverage/20260822-140000/`.

# 2026-08-22 Main ECS migration Task 9 focused acceptance review refreshed

The current 30-row responsibility matrix was independently checked under the reduced validation
policy. MainBoundary classifications are valid and its latest focused run reports 644 Simulation
files with 0 violations. Narrowly accepted rows and remaining planned/unknown/blocked/excluded rows
are listed in the review artifact; no open row was promoted by absence of failures.

Evidence: `Build/diagnostics/main-migration/task-9-acceptance-review/20260822-150000/status.txt`.

# 2026-08-22 Main ECS migration Task 9 delayed-process semantics retained deferred

M-024 remains unknown because `Main.DelayedProcesses` and `DelayedProcessesInGame` advance arbitrary
`IEnumerator` instances across client/UI, pause/menu, ambience, weather and entity boundaries. The
typed Simulation command queue and named tick schedule remain the authority boundary; no generic
coroutine or delegate queue was added. TickOrder focused verification exits `0`.

Evidence: `Build/diagnostics/main-migration/task-9-delayed-process-semantics-boundary/20260822-090000/`.

The caller inventory confirms no in-tree `.Add` sites for either public DelayedProcesses list, while
the two MainThreadAction callers have different host/client and background-generation owners.
Evidence: `docs/research/2026-08-22-main-queue-caller-inventory.md`.

# 2026-08-22 Main ECS migration Task 9 Slime Rain affected-project build rechecked

The affected Server dependency chain builds in Release with `0 warnings, 0 errors` after the
Slime Rain metadata, WorldPersistence v4 and authoritative command-boundary changes. Evidence:
`Build/diagnostics/main-migration/task-9-slime-rain-eligibility-boundary/20260822-100000/`.
This is an affected-project gate only; it does not claim the root solution Release build or full
Task 9 acceptance.

# 2026-08-22 Main ECS migration Task 9 reduced-scope static Tile child recheck

The single focused Liquid verifier for the static Tile definition child exits `0` under the
reduced validation policy. It covers the 753-ID registry, liquid death tables, TileObject
overrides, bounded propagation/commit/replication, settle retries and runtime Liquid Panic.
Evidence: `Build/diagnostics/main-migration/task-9-static-tile-definition-boundary/20260822-160000/`.

The verifier initially exposed a current compile regression in `UnderwaterPlantValidationQuery`:
the cast was applied before multiplication, leaving an `int` assignment to `short` (CS0266).
The minimal fix moves the checked conversion around the complete multiplication expression. This
is a focused child verification only; it does not claim broad Liquid or Task 9 parity.

# 2026-08-22 Main ECS migration Task 9 reduced-scope item definition recheck

The single focused Item Definitions verifier exits `0` under the reduced validation policy and
confirms the immutable supported-server registry accepts valid definitions while rejecting invalid
references. Evidence: `Build/diagnostics/main-migration/task-9-item-definition-registration-boundary/20260822-170000/`.

This refreshes M-003 evidence only. Complete Version4 `Initialize_Items` defaults, staff/claw
arrays, visual/UI metadata and unsupported item families remain deferred; no domain-complete item
parity claim is made.

# 2026-08-22 Main ECS migration Task 9 reduced-scope player ownership recheck

The single focused Player Ownership verifier exits `0` under the reduced validation policy and
confirms stable player ownership, lifecycle and snapshot contracts. Evidence:
`Build/diagnostics/main-migration/task-9-entity-ownership-initialization-boundary/20260822-180000/`.

This refreshes the M-002 ownership boundary only. Full player behavior, entity lifecycle, physics,
replication and client-array parity remain deferred.

# 2026-08-22 Main ECS migration Task 9 reduced-scope NPC boundary recheck

The single focused NPC Boundary verifier exits `0`, scanning 652 Simulation source files with zero
legacy NPC dependency violations. Evidence:
`Build/diagnostics/main-migration/task-9-supported-npc-definition-boundary/20260822-190000/`.

The current checkout has no matching Projectile definition verifier project, so the historical
Projectile evidence was not reused and no Projectile pass is claimed. NPC SetDefaults/static tables,
boss/event mappings, AI families, spawn tables and full lifecycle parity remain deferred.

# 2026-08-22 Main ECS migration Task 9 reduced-scope MainBoundary recheck

The focused MainBoundary isolation gate exits `0`: 654 Simulation source files were checked and
zero forbidden `Main`, client, transport, Protocol or Server dependencies were found. Evidence:
`Build/diagnostics/main-migration/task-9-acceptance-review/20260822-200000/`.

This is an architectural isolation gate for the reduced validation batch, not proof of full Task 9
behavioral parity or root solution Release readiness.

# 2026-08-22 Main ECS migration Task 9 reduced-scope NPC lifecycle recheck

The single focused NPC verifier exits `0` for the source contract, target/chase baseline, death
revision and deterministic loot boundary. Evidence:
`Build/diagnostics/main-migration/task-9-npc-combat-lifecycle-gap/20260822-003000/`.

This is an M-019 narrow child acceptance only. Complete NPC static tables, boss/event mappings, AI
families, spawn tables and full lifecycle parity remain deferred.

# 2026-08-22 Main ECS migration Task 9 reduced-scope NPC composition recheck

The single focused NPC Composition verifier exits `0` for town-home intent/snapshot restore,
segment root/child validation, child-before-root death order and component-only composition.
Evidence: `Build/diagnostics/main-migration/task-9-npc-combat-lifecycle-gap/20260822-150000/`.

This is an M-019/M-002 composition child acceptance only. Complete NPC AI families, static tables,
segmented lifecycle, spawn tables and replication parity remain deferred.

# 2026-08-22 Main ECS migration Task 9 reduced-scope player lifecycle recheck

The single focused PlayerLifecycle verifier exits `0` for one-shot death, configured respawn delay,
authoritative respawn reset, UUID account retention and separation of runtime identity from persistent
account ownership. Evidence:
`Build/diagnostics/main-migration/task-9-player-authority-boundary/20260822-160000/player-lifecycle-focused.txt`.

This is an M-018 lifecycle child acceptance only. Complete player parity and loopback/session
replication remain deferred.

# 2026-08-22 Main ECS migration Task 9 reduced-scope player physics recheck

The single focused PlayerPhysics verifier exits `0` for solid/unknown tile handling, half-bricks,
slopes, actuator inactive collision, platform fall-through and Down input propagation. Evidence:
`Build/diagnostics/main-migration/task-9-player-authority-boundary/20260822-170000/player-physics-focused.txt`.

This is an M-018 physics child acceptance only. Complete movement/collision parity and the full
physics loopback/session matrix remain deferred.

# 2026-08-22 Main ECS migration Task 9 reduced-scope NPC protocol recheck

The single focused NPC Protocol verifier exits `0` for NPC state snapshot round-trip and typed
SyncNPC projection/codec fixtures. Evidence:
`Build/diagnostics/main-migration/task-9-npc-combat-lifecycle-gap/20260822-180000/npc-protocol-focused.txt`.

This is an M-019 protocol child acceptance only. Complete NPC AI/spawn tables, full replication
matrix and client presentation parity remain deferred.

# 2026-08-22 Main ECS migration Task 9 reduced-scope wiring recheck

The single focused Wiring verifier exits `0` for bounded wire traversal, mechanisms, actuators,
pumps, pressure plates, atomic Extractinator chest normalization, locked-chest rejection and
source-backed chest origin/destruction projection. Evidence:
`Build/diagnostics/main-migration/task-9-wiring-boundary/20260822-210000/wiring-focused.txt`.

This is an M-022/M-014 typed wiring child acceptance only. Arbitrary `MainThreadAction` queue
semantics, complete wiring network behavior and client presentation remain deferred/unknown.

# 2026-08-22 Main ECS migration Task 9 reduced-scope player input recheck

The single focused PlayerSimulation verifier exits `0` for deterministic input replay and duplicate
input rejection, with replay hash `F22348FB624EC325A2728373AAB0B207EC915E9B720143575094A4AE447AE872`.
Evidence: `Build/diagnostics/main-migration/task-9-player-authority-boundary/20260822-190000/player-simulation-focused.txt`.

This is an M-018 input-sequencing child acceptance only. Complete movement/collision parity and the
full player lifecycle/replication matrix remain deferred.

# 2026-08-22 Main ECS migration Task 9 reduced-scope combat protocol recheck

The single focused Combat.Protocol verifier exits `0` for typed NPC/projectile/item V1456 wire
projections, sparse projectile flags, client termination ownership, combat catalog authority and
PVS revision cursors. Evidence:
`Build/diagnostics/main-migration/task-9-combat-protocol-boundary/20260822-200000/combat-protocol-focused.txt`.

This is a narrow projection acceptance only. Complete packet parity, client behavior and the full
combat/projectile loopback matrix remain deferred.

# 2026-08-22 Main ECS migration Task 9 reduced-scope item/world-item recheck

The single focused Items verifier exits `0` for inventory authority, bounded world-item spawn/motion
and pickup, item use/placement/equipment, immutable snapshots, compatibility adapters and atomic
Extractinator behavior. Evidence:
`Build/diagnostics/main-migration/task-9-item-world-item-integrity/20260822-010000/`.

This is an M-021 narrow child acceptance only. Complete legacy item tables, client presentation and
the full item loopback/session matrix remain deferred.

# 2026-08-22 Main ECS migration Task 9 reduced-scope projectile authority recheck

The single focused Combat verifier exits `0` for projectile owner/integrity rejection, finite
velocity inputs, lifetime/penetration validation, authoritative expiry, revisioned replication,
sweep collision and fire cooldown. Evidence:
`Build/diagnostics/main-migration/task-9-projectile-owner-authority/20260822-020000/combat-focused.txt`.

This is an M-020 narrow child acceptance only. Complete projectile type definitions, full
motion/collision/damage lifecycle parity and the projectile loopback/session matrix remain deferred.

# 2026-08-22 Main ECS migration Task 9 reduced-scope Tile interaction recheck

The single focused TileInteraction verifier exits `0` for typed tile commands, deterministic
sequence-order commits, invalid-action rejection, TileSquare/liquid projection and visible-session
replication. Evidence:
`Build/diagnostics/main-migration/task-9-tile-command-commit-boundary/20260822-030000/tile-interaction-focused.txt`.

This is an M-022 narrow child acceptance only. Complete legacy framing/animation tables, all tile
flags and network cadence remain deferred.

# 2026-08-22 Main ECS migration Task 9 reduced-scope protocol compatibility recheck

The single focused Protocol Compatibility verifier exits `0` for V1456 compatibility length
contracts and the typed NetModules/unsupported-wiring isolation boundary. Evidence:
`Build/diagnostics/main-migration/task-9-protocol-compatibility-boundary/20260822-060000/`.

This is a narrow projection acceptance only. Complete packet parity, client presentation and
unsupported wiring message behavior remain deferred.

# 2026-08-22 Main ECS migration Task 9 reduced-scope world-object recheck

The single focused WorldObjects verifier exits `0` for chest ownership/range/indexing/destruction,
typed chest transfer and V1456 projection, plus sign range/revision/capacity/projection. Evidence:
`Build/diagnostics/main-migration/task-9-world-object-authority/20260822-070000/`.

This is an M-022 world-object child acceptance only. Complete tile framing, liquid simulation,
legacy object tables and network cadence remain deferred.

# 2026-08-22 Main ECS migration Task 9 reduced-scope world protocol recheck

The single focused World.Protocol verifier exits `0` for authoritative complex TileSection encoding,
source-shaped sign tails and snapshot-based initial world stream construction. Evidence:
`Build/diagnostics/main-migration/task-9-world-object-authority/20260822-110000/`.

This is an M-022/M-006 protocol child acceptance only. Complete V1456 packet parity, client
presentation and periodic network cadence remain deferred.

# 2026-08-22 Main ECS migration Task 9 reduced-scope world mechanics recheck

The single focused WorldMechanics verifier exits `0` for bounded door range/revision, source-shaped
ToggleDoorState projection and trapdoor/tall-gate footprint/frame transitions. Evidence:
`Build/diagnostics/main-migration/task-9-world-object-authority/20260822-120000/`.

This is an M-022 world-object mechanics child acceptance only. Complete legacy framing/animation
tables, unsupported object types and network cadence remain deferred.

# 2026-08-22 Main ECS migration Task 9 reduced-scope WLD compatibility recheck

The single focused WorldCompatibility verifier exits `0` for immutable WLD state projection,
column-major tile coordinate preservation, unsupported-record diagnostics and immutable collection
boundaries. Evidence:
`Build/diagnostics/main-migration/task-9-wld-header-matrix/20260822-130000/compatibility-focused.txt`.

This is a narrow compatibility projection acceptance only. Fractional WLD clock conversion, rain
repair/runtime mapping and complete legacy header behavior remain unknown or deferred.

# 2026-08-22 Main ECS migration Task 9 reduced-scope WLD parser recheck

The single focused WorldFile V319 verifier exits `0` for runtime graph exclusion of legacy source
evidence, the historical version matrix and the recorded differential oracle. Evidence:
`Build/diagnostics/main-migration/task-9-wld-header-matrix/20260822-140000/`.

This is a parser boundary acceptance only. Fractional clock conversion, unresolved rain repair/
runtime mapping and complete legacy WorldFile write-back remain unknown or deferred.

# 2026-08-22 Main ECS migration Task 9 reduced-scope TickOrder recheck

The single focused TickOrder verifier exits `0` for named phase scheduling, paused short-circuit,
equal-sequence deterministic ordering, reversed independent inputs and world progression completion
before projectile advancement. Evidence:
`Build/diagnostics/main-migration/task-9-tick-order-coverage/20260822-080000/`.

This refreshes M-005 only. Complete Main.Update parity, WorldGen cadence, network cadence and
client presentation remain deferred.

# 2026-08-22 Main ECS migration event lifecycle focused refresh

The source-backed event state machines were run twice serially through WorldRules. Both runs
exited `0` and passed the invasion start/size/delay/position/travel/warning/progress/completion
routes, typed clear flags, Slime Rain eligibility/warning/cooldown/stop, Lantern Night scheduling
and rain suppression, explicit meteor impact safety/commit, wind/rain state and domain random-state
checks. Evidence: `Build/diagnostics/main-migration/task-10-event-lifecycle-refresh/20260822-154000/`.

This accepts only the supported event slices. Non-Martian random start parity, full invasion NPC
tables/damage tracking, player-stat production, `FakeLoadInvasionStart`, localized chat/achievement
and client presentation, automatic meteor global `Main.rand` parity, and complete WorldGen side
effects remain deferred.

# 2026-08-22 Main ECS migration world mechanics focused refresh

The bounded door/world-mechanics route was run serially twice. Both WorldMechanics verifier runs
exited `0`, covering deterministic door state and revision, source `ToggleDoorState` projection,
and trapdoor/tall-gate footprint and frame preservation. Evidence:
`Build/diagnostics/main-migration/task-10-world-mechanics-refresh/20260822-153000/`.

This is a narrow M-022 world-object child. Complete WorldGen door placement/kill rules, tile
framing and animation tables, remaining Wiring families, liquid environment side effects, client
animation/audio and full network cadence remain deferred.

# 2026-08-22 Main ECS migration tick-order reconciliation

The named Simulation tick schedule was re-run serially twice after the NPC, Projectile, Item and
Tile narrow cards. Both TickOrder verifier runs exited `0`, covering active/paused phase traces,
deterministic equal-sequence command ordering, reversed independent inputs, invasion completion
before projectile advancement, world-rate resolution before clock/travel, and frozen-rate behavior.
Evidence: `Build/diagnostics/main-migration/task-10-tick-order-reconciliation/20260822-151500/`.

The first attempted pair was accidentally launched in parallel and produced a compiler output-lock
`CS2012`; it was discarded and the required serial reruns passed. This card accepts only the
supported phase relations. Complete Main loop order, monolithic WorldGen.UpdateWorld ordering,
server-host cadence, client rendering and unsupported entity behavior order remain deferred.

# 2026-08-22 Main ECS migration Task 9 reduced-scope server tick isolation recheck

The single focused NetworkIsolation verifier exits `0` with all 10 isolation checks passing.
Evidence: `Build/diagnostics/main-migration/task-9-server-tick-boundary/20260822-090000/`.

This is an M-006 narrow child acceptance only. Exact legacy host cadence, client timeout arrays,
spam updates and periodic WorldData broadcast timing remain deferred.

# 2026-08-22 Main ECS migration Task 9 reduced-scope server world-stream recheck

The single focused World.Server verifier exits `0` for server-owned section visibility versions,
spawn-center section selection and initial V1456 world section replication. Evidence:
`Build/diagnostics/main-migration/task-9-server-tick-boundary/20260822-100000/`.

This is an M-004/M-006 server-stream child acceptance only. Complete client activation, packet
cadence and periodic WorldData timing remain deferred.

# 2026-08-22 Main ECS migration Task 9 reduced-scope world persistence recheck

The single focused Persistence verifier exits `0` for value-only world snapshot round-trip, strict
recovery validation and atomic replacement. Evidence:
`Build/diagnostics/main-migration/task-9-world-save-load-boundary/20260822-040000/`.

This refreshes M-017 only. WLD write-back parity, save scheduling, legacy host cadence and the full
persistence loopback/session matrix remain deferred.

# 2026-08-22 Main ECS migration Task 9 reduced-scope WorldRules recheck

The single focused WorldRules verifier exits `0` across the supported source-backed rule set:
clock authority, blood moon/eclipse, invasion guards and bounded progression, rain/raw weather,
world-event randomness, Slime Rain, meteor, wind, Lantern Night and normal event guards. Evidence:
`Build/diagnostics/main-migration/task-9-world-rules-focused/20260822-050000/`.

This is a collection of narrow rule acceptances, not full event parity. Invasion NPC tables and
complete runtime travel integration, global legacy random ordering, client announcements/ambience
and unsupported weather repair branches remain deferred or unknown.

# 2026-08-22 Main ECS migration Task 9 reduced-scope MainBoundary final batch recheck

The focused MainBoundary isolation gate exits `0` after the current batch: 685 Simulation source
files were checked and zero forbidden `Main`, client, transport, Protocol or Server dependencies
were found. Evidence:
`Build/diagnostics/main-migration/task-9-acceptance-review/20260822-220000/`.

This is an architectural isolation gate only. It does not claim root Release readiness or full Task
9 behavioral parity.

# 2026-08-22 Main ECS migration Task 9 reduced-scope server import recheck

The single focused WorldImport verifier exits `0` for strict import options and the no-listener
failure lifecycle. Evidence:
`Build/diagnostics/main-migration/task-9-server-bootstrap-boundary/20260822-220000/`.

This refreshes the M-004 server bootstrap/import boundary only. Interactive menus, cloud/path
policy, port forwarding, password handling, save scheduling and legacy host cadence remain
deferred.

# 2026-08-22 Main ECS migration Task 9 reduced-scope WorldClock recheck

The single focused WorldClock verifier exits `0` for fixed tick rate, day/night boundaries, full
cycle, pause/continuation, simulation tick phase and snapshot continuation. Evidence:
`Build/diagnostics/main-migration/task-9-wld-clock-fraction-boundary/20260822-210000/`.

M-007 remains `unknown` for WLD import: the legacy file stores Double time while the current
authoritative clock and protocol projection are Int32. No truncation, rounding or fractional
restart contract was invented. M-008 similarly remains `unknown` for unresolved WLD rain runtime
mapping and secret-seed repair membership.

# 2026-08-22 Main ECS migration Task 9 reduced-scope player authority recheck

The single focused PlayerAuthority verifier exits `0` for tile-authoritative collision, simulation
death/respawn, typed V1456 player projections, UUID-owned restore and client overwrite rejection.
Evidence: `Build/diagnostics/main-migration/task-9-player-authority-boundary/20260822-230000/`.

This refreshes M-018 only. Complete player parity, legacy client arrays and the full replication /
session matrix remain deferred.

# 2026-08-23 Main ECS migration open-responsibilities Task 1

Task 1 completed its source-order accounting checkpoint for
`Initialize_AlmostEverything` using Version4 `Main.cs:3732-3859` (SHA-256
`844A862B4EF863FF848227C1D682E89DAB799EEAD48070ADEA07565EC933254D`). The owner matrix records
59 source-order call families and distinguishes Simulation, Server/Protocol, Client/content and
deferred owners. Existing child-card coverage was not promoted to aggregate M-001 acceptance.

The focused MainBoundary verifier exited `0` with 764 Simulation source files and zero forbidden
dependencies. Evidence:
`Build/diagnostics/main-migration/task-9-main-member-coverage/20260823-120000/`.

M-001 remains `planned`: no unresolved family had a unique source-backed server owner and
falsifiable predicate in this card, so no aggregate initializer or guessed implementation was
added. The coverage matrix remains unchanged. Next card: Task 2, M-007 fractional WLD clock.

# 2026-08-23 Main ECS migration open-responsibilities Task 2

Task 2 audited the Version4 WLD clock contract. `WorldFile.cs` writes and reads `_tempTime` as
`Double`, and `Main.UpdateTime` advances it by `dayRate`; the current ECS snapshot stores only
`Int32 TimeOfDay`. The explicit `1.5` RED fixture therefore proves that no lossless conversion
exists in the current representation. No truncation, rounding, default or silent rejection was
introduced, and M-007 remains `unknown` pending a versioned fractional clock contract.

The WLD V319 focused verifier exited `0`. The WorldClock focused verifier could not compile due
to the existing/unowned `CoatingColorSelectionQuery.cs:7` `WorldTile` CS0246 error; this card is
therefore not accepted. Evidence:
`Build/diagnostics/main-migration/task-9-wld-clock-fraction-boundary/20260823-123000/`.

# 2026-08-23 Main ECS migration open-responsibilities Task 3

Task 3 audited the Version4 raw rain contract and repair branches. The parser already retains
`raining`, `rainTime` and `maxRaining` losslessly, but `maxRaining` feeds legacy wind smoothing as
a saved maximum and cannot be assumed to equal the ECS current `RainStrength`. The historical
`FixEndlessRainWorlds` branch also depends on version, a long timer and active
`rainsForAYear` secret-seed membership that compatibility metadata does not carry.

Both focused WorldImport and WorldRules verifiers were blocked by the existing/unowned
`CoatingColorSelectionQuery.cs:7` `WorldTile` CS0246 error. M-008 remains `unknown`; no runtime
mapping, repair default or source-unsupported normalization was added. Evidence:
`Build/diagnostics/main-migration/task-9-wld-rain-runtime-boundary/20260823-124500/`.

# 2026-08-23 Main ECS migration open-responsibilities Task 4

Task 4 confirmed that legacy invasion travel consumes mutable `Main.dayRate` directly. Its source
is `UpdateTimeRate`, which branches for fast-forward, time freeze, configured target rate,
sleeping-player acceleration and game-menu fallback. `WorldClock.TicksPerUpdate` is not a
source-proven equivalent, so no travel integration or substitute rate was added.

The focused WorldRules and TickOrder verifiers were both blocked by the existing/unowned
`CoatingColorSelectionQuery.cs:7` `WorldTile` CS0246 error. M-009 remains `unknown` for full
travel integration. Evidence:
`Build/diagnostics/main-migration/task-9-invasion-travel-integration-boundary/20260823-130000/`.

# 2026-08-23 Main ECS migration open-responsibilities Task 5

Task 5 split the two source-backed `Main.QueueMainThreadAction` callers. The section-loaded action
belongs to host/client section state, while the other is an arbitrary WorldGen background
continuation delegate. Neither has a source-backed Simulation identity, deterministic phase,
cancellation, persistence/restart or projection contract; a generic `Queue<Action>` would violate
the typed Simulation command boundary.

M-014 remains `unknown` for arbitrary callers; no command was created. The caller contract matrix
and evidence are at
`Build/diagnostics/main-migration/task-9-main-thread-action-boundary/20260823-131500/`.

# 2026-08-23 Main ECS migration open-responsibilities Task 6

Task 6 audited both public delayed-process lists. Version4 contains no in-tree `.Add` caller for
either `List<IEnumerator>`; the lists are advanced by reverse `MoveNext` scans in distinct update
phases, and in-game processes are skipped through the pause short-circuit. An arbitrary iterator
cannot provide a typed owner, ordering key, cancellation, persistence/restart state or deterministic
replay identity, and public mutability leaves external/plugin callers possible.

M-024 remains `unknown/deferred`; no generic coroutine queue or guessed typed process was added.
Evidence:
`Build/diagnostics/main-migration/task-9-delayed-process-semantics-boundary/20260823-133000/`.

# 2026-08-23 Main ECS migration open-responsibilities Task 7

The batch acceptance review completed without promoting any open responsibility merely because an
accounting gate passed. M-001 remains `planned`; M-007, M-008, M-009 and M-014 remain `unknown`;
M-024 remains `unknown/deferred`. Each outcome has a source anchor, explicit owner/authority gap
and per-card evidence. The MainBoundary focused gate passed (764 Simulation files, 0 violations)
and the WLD V319 verifier passed.

WorldClock, WorldImport, WorldRules and TickOrder focused verifiers remain blocked before test
execution by the unowned `CoatingColorSelectionQuery.cs:7` `WorldTile` CS0246 error. The scoped
batch `git diff --check` passed. A whole-worktree diff check remains blocked only by existing
trailing whitespace in `LegacyReference/WorldFile.cs`, which this batch did not modify. Review
evidence: `Build/diagnostics/main-migration/task-9-acceptance-review/20260823-140000/`.

# 2026-08-22 Main ECS migration open-responsibilities focused acceptance refresh

The focused acceptance refresh revalidated the active Version4 source hashes and retained every
open responsibility without promotion: M-001 is `planned`; M-007, M-008, M-009 and M-014 are
`unknown`; M-024 is `unknown/deferred`. M-009 evidence is
`Build/diagnostics/main-migration/task-9-invasion-travel-integration-boundary/20260822-113939/`;
M-014 and M-024 contract evidence is respectively under
`task-9-main-thread-action-boundary/20260822-114109/` and
`task-9-delayed-process-semantics-boundary/20260822-114109/`.

The focused MainBoundary verifier exited `0` with 764 Simulation source files and zero
violations. The scoped `git diff --check` exited `0`. The WLD V319 verifier remains green;
WorldClock, WorldImport, WorldRules and TickOrder remain blocked before test execution by the
existing/unowned `CoatingColorSelectionQuery.cs:7` `WorldTile` CS0246 error. This refresh did not
run root Release or broad regression suites under the reduced validation scope. Evidence:
`Build/diagnostics/main-migration/task-9-acceptance-review/20260822-114342/`.

# 2026-08-22 Blocked ECS migration B-001 and B-004 unblocks

B-001 restores the focused Simulation build boundary: the untracked WorldGeneration
`CoatingColorSelectionQuery` now imports the actual `WorldTile` namespace. The Simulation Release
build, WorldClock verifier and MainBoundary verifier all exit `0`; the separate untracked
WorldGeneration verifier remains blocked by four unrelated compilation errors in its `Program.cs`
before its existing coating assertion can run. Evidence:
`Build/diagnostics/main-migration/task-9-build-ownership-boundary/20260822-150000/`.

B-004 accepts only lossless WLD raw-rain recovery. A complete modern
`raining`/`rainTime`/`maxRaining` triple now projects to `IsRaining`, `RainTimeTicks` and
`MaximumRainStrength`, without treating the saved maximum as runtime `RainStrength`. The existing
raw-weather persistence format preserves those independent facts. The pre-v318 long-rain repair
predicate is fail-closed because active `rainsForAYear` membership is absent from the compatibility
input; affected imports are rejected rather than repaired or normalized. WorldImport, WorldRules,
Persistence, MainBoundary and the scoped diff check exit `0`. Evidence:
`Build/diagnostics/main-migration/task-9-wld-rain-runtime-boundary/20260822-151500/`.

The remainder remains explicit: B-002 requires approval of a versioned mutable time-rate snapshot;
B-003 needs a fractional clock representation or an integral invariant; B-005 has no isolated
initializer family; B-006 has no typed/replayable queue caller; and B-007 cannot reproduce the
global `Main.rand` trace. These statuses are source-audited, not defaults or broad completion
claims.

# 2026-08-22 Main ECS migration B-002 time-rate travel authority accepted narrowly

The approved `WorldTimeRateSnapshot` route is now evidenced as an immutable, version-27-persisted
server authority. `WorldTimeRatePolicy` retains the `Main.UpdateTimeRate` precedence for
fast-forward (`60`), configured rate, all-active-player sleeping acceleration, freeze (`0`) and
game-menu fallback (`1`). `DomeSimulation` resolves that snapshot in `ApplyWorldClock` before
`WorldInvasionTravelSystem` consumes `max(rate, 1)`; `TicksPerUpdate` is not used as a substitute.

Focused TickOrder, Persistence, WorldRules and MainBoundary gates all exited `0`; MainBoundary
checked 766 Simulation source files with zero violations. M-009 remains `unknown` for its complete
lifecycle: random starts, NPC tables, player-stat production, warning/chat presentation and full
legacy parity are not accepted by this card. Evidence:
`Build/diagnostics/main-migration/task-9-world-time-rate-authority/20260822-130821/`.

# 2026-08-22 Main ECS migration Task 10 open-responsibilities reduced acceptance review

The reduced Task 10 review reconciles the remaining initializer, host-queue, delayed-process,
randomness, entity-lifecycle, world-object, and event cards against current source anchors and
focused evidence. M-001 remains `planned`; M-014 remains `unknown/deferred`; M-024 remains
`unknown/deferred`; B-007 remains `explicit-deferred` because the global `Main.rand` order cannot
be restored from the server snapshot. No aggregate initializer, `Action` queue, coroutine queue,
or probabilistic meteor scheduler was added.

PlayerLifecycle, NPC, Items, Combat/Projectile, World, WorldRules, Completion, and MainBoundary
focused verifiers all exited `0`. MainBoundary checked 767 Simulation files with zero forbidden
dependencies. The accepted scope is limited to the narrow typed owners documented by the cards;
complete NPC AI/table parity, projectile type/motion parity, complete item SetDefaults parity,
full tile/network cadence, random event starts, global RNG order, chat/client presentation, and
full invasion parity remain explicitly deferred.

Evidence: `Build/diagnostics/main-migration/task-10-open-responsibilities-review/20260822-160000/`.
This is a reduced acceptance review, not a claim of complete Main migration or a percentage.

# 2026-08-22 Main ECS migration M-009 rate/travel source reconciliation

The previously stale M-009 travel boundary is reconciled with the current source tree. The
versioned `WorldTimeRateSnapshot` and `WorldTimeRatePolicy` are now consumed by
`DomeSimulation` in `ApplyWorldClock`, and `WorldInvasionTravelSystem` advances the persisted
`InvasionX` using the source `max(rate, 1)` rule. TickOrder and WorldRules focused verifiers both
exited `0`; MainBoundary exited `0` with 767 Simulation files and zero violations. Evidence:
`Build/diagnostics/main-migration/task-10-invasion-travel-rate-integration/20260822-142500/`.

This promotes only the bounded rate/travel child route. M-009 remains deferred for non-Martian
random start side, NPC spawn tables and AI, qualified-player stat production, complete warning/chat
and client presentation, damage tracking, and full invasion lifecycle parity. M-014/M-024 remain
deferred because no typed/replayable caller contract is present; B-007 remains deferred because
the legacy global `Main.rand` ordering oracle is still unavailable.

# 2026-08-22 Main ECS migration M-019 NPC lifecycle focused refresh

The existing NPC child route was re-run twice from the current worktree. Both runs exited `0`
with `PASS: NPC source contract, target/chase baseline, death revision and deterministic loot`.
The evidence covers bounded definition identity, spawn eligibility rejection, target/chase state,
contact/combat, death publication, replication revision and deterministic first-table loot.
Evidence: `Build/diagnostics/main-migration/task-10-npc-lifecycle-refresh/20260822-143000/`.

This does not promote complete NPC lifecycle parity. Boss/invasion tables, all `AI_###` families,
buff/status behavior, full town services, global-random spawn selection, and client/protocol side
effects remain deferred under the M-019 child boundary.

# 2026-08-22 Main ECS migration M-020 Projectile lifecycle focused refresh

The current Projectile authority slice was run twice from the current worktree. Both Combat
verifier runs exited `0`, covering forged/inactive owner rejection, non-finite input rejection,
one-tick lifetime expiry, typed linear/gravity behavior, swept tile/NPC collision, damage and
penetration rules, and stable replication revisions. Evidence:
`Build/diagnostics/main-migration/task-10-projectile-lifecycle-refresh/20260822-144000/`.

This remains a narrow M-020 acceptance. Complete projectile `SetDefaults`/type tables, all
`AI_###` families, minion/sentry/whip/grapple/boss behavior, special geometry and liquid parity,
full V1456 message 27/29 parity, and client/audio effects remain deferred.

# 2026-08-22 Main ECS migration M-021 Item lifecycle focused refresh

The current Item authority route was run twice from the current worktree. Both Items verifier runs
exited `0`; all 25 focused assertions passed, covering stack and unique-instance invariants,
instance metadata, world-item spawn/motion/destroy/pickup, one-winner pickup races, use/cooldown,
mana/ammo, equipment, placement, deterministic drops/prefixes/variants, Extractinator atomicity,
UUID account import, persistence projection and legacy compatibility adapters. Evidence:
`Build/diagnostics/main-migration/task-10-item-lifecycle-refresh/20260822-145000/`.

This remains a narrow M-021 acceptance. Complete `ItemID`/`SetDefaults` and prefix/variant tables,
client/UI/audio behavior, complete network field parity and unsupported `Item.cs` families remain
deferred.

# 2026-08-22 Main ECS migration M-022 Tile command commit focused refresh

The Tile interaction authority route was run twice from the current worktree. Both verifier runs
exited `0`, covering typed tile commands, deterministic sequence/coordinate ordering, repeated or
out-of-world/invalid-kind rejection, no mutation on rejected actions, monotonic server sequence
commit, TileSquare/liquid environment projection and visible-session loopback replication. Evidence:
`Build/diagnostics/main-migration/task-10-tile-command-commit-refresh/20260822-150000/`.

This remains a narrow M-022 acceptance. Complete multitile framing/animation, full liquid simulation,
legacy tile flags and table rewrites, WorldGen-specific KillTile/place rules, network cadence and
client presentation remain deferred.

# 2026-08-22 Main ECS migration B-007 legacy RNG oracle audit

The frozen night-start source was traced from `Main.UpdateTime_StartNight` through the meteor
observation. The ordered precondition calls are `Star.NightSetup`, `NPC.setFireFlyChance`,
`BirthdayParty.CheckNight`, `LanternNight.CheckNight` and `MysticLogFairiesEvent.StartNight`; the
meteor `Main.rand.Next(50)` occurs only after those boundaries. `NPC.setFireFlyChance` is the first
non-recoverable consumer because it mixes global `Main.rand` with `WorldGen.genRand` and mutable
world/NPC state. Later eye/hard-boss/NPC branches consume the same global stream as well.

The executable source trace therefore rejects a closed legacy oracle rather than treating
`WorldEventRandomState` as global parity. Automatic meteor probability remains explicit-deferred;
the typed meteor schedule command and deterministic impact route remain supported. Evidence:
`Build/diagnostics/main-migration/task-10-meteor-rng-oracle-audit/20260822-155000/`; research:
`docs/research/2026-08-22-main-meteor-global-rng-boundary.md`.

# 2026-08-22 Server ECS convergence P4/P9 audit refresh

The full bounded WorldGen verifier and the Simulation Release build both exited `0`. The fresh
P9 trace is at `Build/diagnostics/server-ecs-convergence/P9-worldgen/20260822-1900/trace.md`.
This does not establish complete legacy WorldGen parity: the deletion gate remains false and the
complete differential still reports `5,040,000 / 5,040,000` tile mismatches.

The Version3-to-Version4 physical deletion ledger now contains all 535 removed C# paths with
namespace, declaration count, byte count, SHA-256, retained-source reference counts, and a
conservative classification: 413 `ClientOnly`, 70 `ServerRelevant`, 50 `SharedDefinition`,
2 `ReplacedWithEvidence`, and 0 `Unknown`. The P4 completeness gate exits `1` because all 70
server-relevant rows are explicitly deferred: removed-source anchors and reasons are recorded,
but no replacement owner/state/command/protocol/verifier evidence is claimed for those rows. See
`docs/migrations/version4-physical-deletion-ledger.csv` and
`Build/diagnostics/server-ecs-convergence/P4-deletion/20260822-1930/gate-run.md`.

# 2026-08-22 Server ECS convergence G chest revision child

The Simulation chest transfer path now accepts an optional expected chest revision and rejects a
stale transfer before mutating inventory or chest slots. The focused WorldObjects verifier proves
matching-revision commit, stale-revision rejection, and one revision increment; the Simulation and
WorldObjects verification projects build with zero warnings and errors. Evidence:
`Build/diagnostics/server-ecs-convergence/P5-worldobjects/20260822-2200/trace.md`.

# 2026-08-24 Task 7 reduced acceptance review

Reconciled the current narrow entity cards and remaining blockers in
`Build/diagnostics/main-migration/task-10-open-responsibilities-review/20260824-024000/`.
The latest MainBoundary run checked 774 Simulation source files with 0 violations; changed-card
NPC, Combat and Items verifiers remain green and the scoped diff gate exits 0. The review keeps
M-001, M-014/M-024, B-007, complete NPC/projectile/item lifecycle, random starts and
client/presentation branches incomplete. No root Release build or full regression was run under
the reduced validation policy.

# 2026-08-24 Player facing input boundary

Closed a concrete forged-input gap: `PlayerInputApplySystem` now rejects `Facing` values outside
`-1, 0, 1` before clearing/applying the batch. The PlayerSimulation verifier covers the rejection
and existing deterministic replay path. Evidence:
`docs/research/2026-08-24-player-facing-input-boundary.md` and
`Build/diagnostics/main-migration/task-5-player-facing-input/20260824-026000/`.

# 2026-08-24 Invasion progress clamp boundary

Reconciled the source NPC invasion damage decrement with the existing typed progression route.
Legacy `NPC.cs:64765-64800` clamps oversized damage to zero; it does not reject it. The existing
WorldRules verifier covers positive progress, duplicate pending sequence rejection, oversized
completion, persistence continuation and one typed completion event. Evidence:
`docs/research/2026-08-24-invasion-progress-clamp-boundary.md` and
`Build/diagnostics/main-migration/task-10-invasion-progress-clamp/20260824-027000/`.
No new upper-bound rejection or random NPC damage approximation was added.

# 2026-08-24 Player input envelope qualification

Audited the player input path. Existing Simulation guards reject unknown players and duplicate
same-player inputs and filter inactive players; protocol command sequencing remains owned by
`DomeServer`. `SimulationInputBatch` has no persisted replay sequence, while legacy release/timing
control state is client/protocol behavior. No second input queue or fabricated sequence envelope was
added. Evidence: `docs/research/2026-08-24-player-input-envelope-boundary.md` and
`Build/diagnostics/main-migration/task-5-player-input-envelope/20260824-025000/`.

# 2026-08-22 Server ECS convergence deletion and WorldGen gate refresh

Completion verification and the bounded WorldGeneration verifier both exit `0`.
The physical-deletion ledger remains intentionally blocked: 535 rows are inventoried,
69 `ServerRelevant` rows remain deferred, and 3 rows have source-backed replacements.
The complete WorldGen differential still reports 5,040,000 mismatched tiles and
`canRemoveLegacyWorldGen=false`. A fresh summary is recorded at
`Build/diagnostics/server-ecs-convergence/P-final/20260822-1643/summary.md`.

The earlier 2026-08-23 intermittent PlayerAuthority note is historical; subsequent queue-fix
evidence and current reruns supersede it with five consecutive exit-0 runs.

# 2026-08-22 Server ECS convergence enum classification refresh

Complete-oracle review proved `HouseType`, `PillarType`, `WindowType`, and
`DungeonDropTrapType` are closed enum definitions with no state, mutation,
persistence, or protocol responsibility. They are now `SharedDefinition` rows,
not `ServerRelevant`. Current ledger counts are 416 `ClientOnly`, 57
`ServerRelevant`, 59 `SharedDefinition`, 3 `ReplacedWithEvidence`, and 0
`Unknown`; the physical-deletion gate remains closed because all 57
server-relevant rows are deferred.

# 2026-08-23 Version4 deletion ledger Ping replacement evidence

The removed `Terraria.Net/Ping.cs` behavior now has complete-oracle evidence from
`D:\TRbackup\无任何删减通过编译\Terraria.Net\Ping.cs` (SHA-256
`DDDA64A77F9DB414A5DB8375D4B212FABD03B662530FF2C26A7D114FD414ACF1`). The current typed
`TerrariaPacketDispatcher.RoutePing`/`TerrariaPacketCodec` path validates the empty payload and
returns a server-owned session ping frame; Protocol Compatibility, NetworkIsolation and
SessionReplication verifiers cover the route. It is accepted as a transient protocol replacement
with no gameplay persistence field. The regenerated ledger is now `413 ClientOnly`, `69
ServerRelevant`, `50 SharedDefinition`, `3 ReplacedWithEvidence`, `0 Unknown`; the deletion gate
still fails because 69 server-relevant rows remain deferred.
This is only a bounded child acceptance. The existing V1456 `ChestTransferIntent` wire shape does
not carry an expected revision, and tile-entity authoritative mutation/projection remains absent;
capability family G stays `Partial` with score `0`.

The completion manifest and capability matrix were reconciled against fresh current-worktree
regression: families C, E, H, and J are `evidenced` again, and the weighted core capability score
is `92`, not a source-migration percentage. The focused trace is
`Build/diagnostics/server-ecs-convergence/P-final/20260822-2015/focused-regression.md`.

# 2026-08-22 Main ECS migration open-blocker qualification re-audit

M-001, M-014 and M-024 were re-audited against the frozen Main/WorldGen source hashes. M-001 still
has no unresolved initializer family with a unique owner, bounded calls, observable state and a
falsifiable server predicate. M-014 still has only the section-loading caller at
`WorldGen.cs:10529` and the caller-supplied background-generation follow-up at `WorldGen.cs:26122`,
with no typed/replayable contract. M-024 has no source-tree `.Add` caller for either public
`List<IEnumerator>`, so no named process contract can be recovered. All three remain explicitly
planned or deferred; no generic initializer, `Queue<Action>` or coroutine scheduler was added.
Evidence: `Build/diagnostics/main-migration/task-10-open-blocker-reaudit/20260822-160000/`;
research: `docs/research/2026-08-22-open-blocker-qualification-reaudit.md`.

# 2026-08-22 Main ECS migration blocked-unblock acceptance review

The B-001 through B-007 review reconciles current source anchors, focused evidence and the
remaining dependency graph. B-002/M-009, B-003/M-007 and B-004/M-008 are accepted only as bounded
children: rate/travel ordering, lossless fractional clock persistence/projection, and independent
WLD rain facts respectively. The M-008 `FixEndlessRainWorlds` branch remains fail-closed without a
canonical `rainsForAYear` seed owner. B-005/M-001 remains planned; B-006/M-014/M-024 remains
unknown/deferred; B-007 remains explicit-deferred because the first mixed global RNG consumer is
not reproducible. Evidence: `Build/diagnostics/main-migration/task-10-blocked-unblock-review/20260822-161500/status.txt`.

# 2026-08-22 Main ECS migration M-001 TorchID definition child

The fixed server-visible part of `Terraria.ID.TorchID.Initialize` now has an independent owner:
`Terraria.Dome.Simulation.WorldGeneration.Definitions.TorchDefinitionRegistry`. It preserves the
source `Count = 24`, all 24 `Dust` mappings and the 11 `IsABiomeTorch` IDs, while rejecting negative
and out-of-range IDs. `TorchColor` providers remain deferred because they depend on client
presentation and `Main.mouseTextColor`; no aggregate initializer was introduced. The reduced
WorldGeneration verifier executed 32/81 sections and passed; MainBoundary checked 769 files with
zero violations. Evidence: `Build/diagnostics/main-migration/task-10-torch-definition-child/20260822-163000/`;
research: `docs/research/2026-08-22-torch-id-definition-boundary.md`.

# 2026-08-22 Main ECS migration M-001 TileEntity definition child

The source `TileEntity.InitializeAll`/`TileEntitiesManager.RegisterAll` order now has an
independent immutable `WorldObjects` definition owner. Type IDs `0..10` and all eleven prototype
names are preserved, and unknown type `11` is rejected. This card does not implement payloads,
placement, updates, pylon/leash behavior or aggregate initialization. The reduced WorldGeneration
verifier passed 32/81 sections; MainBoundary checked 771 files with zero violations. Evidence:
`Build/diagnostics/main-migration/task-10-tile-entity-definition-child/20260822-164000/`;
research: `docs/research/2026-08-22-tile-entity-definition-boundary.md`.

# 2026-08-22 Main ECS migration M-001 LeashedEntity qualification audit

`LeashedEntity.Registry.RegisterAll` has a deterministic source registration sequence (sentinel
type 0, then kite/critter types 1..19), but it is not eligible for a Simulation definition child.
The source owner also controls section streaming, PVS activation, spawn/despawn/update/draw and
NetModule synchronization, with no server persistence/replay contract in the current tree. The
candidate therefore remains `unknown/deferred`; no ID-only registry was added. Evidence:
`Build/diagnostics/main-migration/task-10-leashed-definition-qualification/20260822-165000/`;
research: `docs/research/2026-08-22-leashed-entity-definition-boundary.md`.

# 2026-08-22 Main ECS migration M-001 NPCInteractions qualification audit

`NPCInteractions.Initialize` was audited as a mixed registration family: 25 shop entries and 18
actions spanning shop/economy, chat/UI, player stats, quests, housing, crafting and reforging.
There is no single Simulation owner or source-backed replay/persistence predicate for the family,
so it remains `unknown/deferred`; no generic interaction registry or aggregate initializer was
added. Evidence: `Build/diagnostics/main-migration/task-10-npc-interactions-qualification/20260822-170000/`;
research: `docs/research/2026-08-22-npc-interactions-qualification-boundary.md`.

# 2026-08-22 Main ECS migration M-001 fish-drop qualification audit

`FishDropRuleList` was audited against `GameContentFishDropPopulator.Populate` and the
`Main.cs:3769-3771` publication route. The source family is a conditional weighted evaluator over
biome/region, height, lava/honey, crates/junk, progression, remix and Angler quest context; it is
not a fixed definition table. The current Simulation has no fishing authority or replay/persistence
contract, so the family remains `unknown/deferred`. It was not merged into NPC loot or generic item
drops. Evidence: `Build/diagnostics/main-migration/task-10-fish-drop-qualification/20260822-171000/`;
research: `docs/research/2026-08-22-fish-drop-qualification-boundary.md`.
# 2026-08-23 Main ECS migration M-001 ArmorSetBonuses qualification audit

`ArmorSetBonuses.Initialize/BuildLookup` is not a fixed identity-only table. Its registrations
store `ArmorSetEffect` delegates, `BuildLookup` builds the item-indexed lookup, and
`GetCompleteSet` dispatches the selected effect into `Player`. The source benefits mutate combat
stats, buffs, set flags, cooldowns, achievements and lighting, while descriptions and callbacks
cross the presentation boundary. The current Simulation has no typed/replayable armor-set effect
contract or persistence owner for those semantics, so this family remains `unknown/deferred`;
no armor-set registry, generic initializer or effect queue was added. Evidence:
`Build/diagnostics/main-migration/task-10-armor-set-qualification/20260823-130000/`;
research: `docs/research/2026-08-23-armor-set-qualification-boundary.md`;
execution proposal: `docs/plans/2026-08-23-armor-set-qualification-execution.md`.
# 2026-08-23 Main ECS migration M-001 ShopHelper qualification audit

`ShopHelper` remains `unknown/deferred`. The initializer constructs a personality database and
biome objects, while `GetShoppingSettings` consumes mutable Player/NPC context and returns both a
price adjustment and localized happiness presentation. The current tree has no server-owned shop
session, stock revision, typed happiness/price query context, persistence/replay contract or
loopback command boundary. No generic ShopHelper replacement or `NpcInteractionRegistry` was
added. Evidence:
`Build/diagnostics/main-migration/task-10-shop-helper-qualification/20260823-131500/`;
research: `docs/research/2026-08-23-shop-helper-qualification-boundary.md`.
# 2026-08-23 Main ECS migration M-001 TeleportPylonsSystem qualification audit

`TeleportPylonsSystem` has a bounded class surface, but it is not an identity-only definition
family. The source rebuilds a mutable pylon list from tile entities, broadcasts add/remove
differences, handles join synchronization and cooldown updates, and also contains client dust
presentation. The current tree has pylon tile-entity compatibility and packet decoding, but no
authoritative pylon collection, revisioned commit/replay contract, join snapshot owner or teleport
command authority. The family remains `unknown/deferred`; no registry or generic initializer was
added. Evidence:
`Build/diagnostics/main-migration/task-10-teleport-pylon-qualification/20260823-133000/`;
research: `docs/research/2026-08-23-teleport-pylon-qualification-boundary.md`.
# 2026-08-23 Main ECS migration M-001 ContentSamples item-repair qualification audit

`ContentSamples.FixItemsAfterRecipesAreAdded` remains `unknown/deferred`. The source enumerates
the complete `ItemsByType` catalog and calls `Item.Refresh(onlyIfVariantChanged: false)` after
recipe setup. This is a full-catalog mutable repair pass, not a fixed identity definition; the
current item authority has no complete recipe ordering, variant ownership, refresh idempotence or
persistence/replay contract. No generic post-recipe repair pass was added and M-003 was not
expanded to claim full catalog parity. Evidence:
`Build/diagnostics/main-migration/task-10-content-samples-repair-qualification/20260823-134500/`;
research: `docs/research/2026-08-23-content-samples-item-repair-qualification.md`.
# 2026-08-23 Main ECS migration qualification batch acceptance review

四张 M-001 family qualification cards（ArmorSetBonuses、ShopHelper、TeleportPylonsSystem、
ContentSamples item repair）已完成独立 acceptance review。所有 source/hash/authority 证据
一致，MainBoundary 检查 771 个 Simulation 源文件且 0 violations，reduced Items verifier
退出 0，各卡 scoped diff check 退出 0。四张卡均为 documentation-only `unknown/deferred`，
没有新增 runtime registry、generic initializer、Action queue 或 coroutine scheduler；M-001
仍为 planned，M-014/M-024 及其余生命周期、table、random-start、client/presentation 范围
继续保留为 open work。Review evidence:
`Build/diagnostics/main-migration/task-10-acceptance-review/20260823-150000/status.txt`;
research: `docs/research/2026-08-23-main-migration-batch-acceptance-review.md`.
# 2026-08-23 Build ownership boundary B-001

The previously blocked WorldClock focused build is now unblocked by retaining the existing
`Terraria.Dome.Simulation.WorldModel` binding in the untracked
`CoatingColorSelectionQuery.cs`; it uses the real tracked `WorldTile` type and adds no surrogate,
alias or duplicate. WorldClock verification passed all 8 scenarios, MainBoundary checked 771
Simulation source files with 0 violations, and scoped diff check exited 0. This accepts only the
build-ownership/type-binding boundary; it does not accept complete WorldGeneration behavior or
any remaining M-001/M-014/M-024 scope. Evidence:
`Build/diagnostics/main-migration/task-10-build-ownership-boundary/20260823-160000/`.
# 2026-08-23 B-001 affected verifier expansion

After the minimal `WorldTile` binding repair, the affected focused gates were rerun: WorldImport,
WorldRules and TickOrder all exited 0; WorldGeneration reduced verification exited 0 with 32/81
sections (39.5%). This confirms the build boundary is usable across the clock/import/rules/tick and
selected generation query chain. It does not close complete WorldGeneration parity, random ordering,
NPC/projectile/item tables or client branches. Evidence:
`Build/diagnostics/main-migration/task-10-build-ownership-boundary/20260823-160000/affected-verifiers.txt`.
# 2026-08-23 NPC worm segment lifecycle child

The NPC lifecycle slice now has a narrow source-backed worm follow-up route. The source
`NPC.CheckActive_WormSegments` behavior is represented by `NpcSegmentLifecycleSystem` following a
validated reciprocal child chain and emitting typed `DespawnNpcCommand` values only for a caller-
classified active worm segment. This avoids guessing the incomplete NPC type/AI table. Composition
and NPC focused verifiers passed, MainBoundary checked 771 files with 0 violations, and scoped diff
check exited 0. Full worm construction, AI mapping, float `ai[0]` projection, network cadence,
loot/random behavior and complete NPC lifecycle remain deferred. Evidence:
`Build/diagnostics/main-migration/task-10-npc-worm-segment-lifecycle/20260823-170000/`;
research: `docs/research/2026-08-23-npc-worm-segment-followup-boundary.md`.
# 2026-08-23 Projectile penetration lifecycle child

The Projectile lifecycle now has explicit evidence for the source-visible one-penetration
boundary: a typed projectile applies one authoritative NPC damage event and becomes inactive after
the committed hit. This matches the generic Version4 penetration decrement/stop branch without
guessing projectile-specific immunity or AI behavior. Combat verification and MainBoundary passed,
and scoped diff check exited 0. Internal penetration remains intentionally outside the current
replication snapshot; local/static immunity, special branches, random penetration, transforms,
bounces, full type tables and client cadence remain deferred. Evidence:
`Build/diagnostics/main-migration/task-10-projectile-penetration-lifecycle/20260823-180000/`;
research: `docs/research/2026-08-23-projectile-penetration-lifecycle-boundary.md`.
# 2026-08-23 Player Kill/Respawn lifecycle boundary

Player lifecycle source audit confirms the existing server-owned death/respawn route covers the
narrow transition: one-shot inactive state, bounded respawn timer, inactive input rejection and
health/position/velocity restoration, with persistent account state kept separate. The source
`Player.KillMe` side effects for drops, PVP/PVE counters, achievements, timestamps, random dust/gore,
mount, child-safety, buffs and presentation remain separate deferred branches. PlayerLifecycle
verification passed. The first PlayerAuthority run timed out in the existing network bootstrap
helper, but a clean rerun exited 0 and completed the UUID bootstrap/overwrite rejection; the first
result is retained as a timing-flake observation. MainBoundary and scoped diff check passed. Evidence:
`Build/diagnostics/main-migration/task-10-player-kill-respawn-lifecycle/20260823-190000/`;
research: `docs/research/2026-08-23-player-kill-respawn-boundary.md`.
# 2026-08-23 Item.NewItem qualification boundary

`Item.NewItem` remains `unknown/deferred`. Version4 combines seasonal type remaps, cached spawn
slot aggregation, legacy slot reuse, `Item.SetDefaults`/prefix, wet collision, global `Main.rand`
velocity and network/client state. The existing deterministic `CreateWorldItemCommand` is a
separate server primitive and is not claimed as `NewItem` parity. Reduced Items verification and
MainBoundary (771 files, 0 violations) passed; scoped diff check exited 0. Evidence:
`Build/diagnostics/main-migration/task-10-item-new-item-qualification/20260823-200000/`;
research: `docs/research/2026-08-23-item-new-item-qualification-boundary.md`.
# 2026-08-23 blocked responsibility execution proposal and NPC death/loot qualification

Added `docs/plans/2026-08-23-blocked-responsibilities-execution.md` to execute M-001, M-014,
M-024 and B-007 as independent typed-owner cards under the reduced 40% verification scope.
The first card records the narrow NPC death/loot boundary in
`docs/research/2026-08-23-npc-death-loot-qualification-boundary.md` and
`Build/diagnostics/main-migration/task-10-npc-death-loot-qualification/20260823-210000/`.
The existing NPC verifier passed; MainBoundary checked 771 Simulation files with 0 violations;
scoped diff check passed. Full `NPC.checkDead/NPCLoot` parity, NPC/loot tables, global random
ordering, restart-persistent death identity, network and presentation branches remain
`unknown/deferred`. M-001/M-014/M-024/B-007 remain open.

# 2026-08-23 M-001 coverage matrix reconciliation

Reconciled `docs/research/2026-08-22-initialize-almost-everything-owner-matrix.md` with the
existing source-backed child evidence: `TileEntity.InitializeAll` is `covered` by the immutable
`TileEntityDefinitionRegistry` (IDs `0..10`, unknown `11` rejected), and `TorchID.Initialize` is
`covered` by `TorchDefinitionRegistry` (24 IDs, Dust and biome facts, invalid IDs rejected).
These are narrow registration children only; tile-entity payload/update/persistence, torch
color/light, complete tables and the aggregate M-001 initializer remain deferred/planned.

# 2026-08-23 B-007 legacy Main.rand trace re-audit

Replayed the source trace for `Main.UpdateTime_StartNight` and recorded fresh hashes, call order,
and the first non-recoverable consumer in
`Build/diagnostics/main-migration/task-10-meteor-rng-oracle-audit/20260823-220000/`. The
WorldRules verifier passed its domain-scoped random stream and qualified meteor command checks,
but this is not evidence of global `Main.rand` ordering. B-007 remains `explicit-deferred`; no
automatic meteor probability, landing search, shower placement or client ambience was added.

# 2026-08-23 NPC death loot restart boundary

The NPC lifecycle now records an already committed inactive `Killed` handle as published during
snapshot restore. The focused NPC verifier proves that a killed NPC produces one world-item drop,
then a restored tick keeps the drop count unchanged. MainBoundary checked 771 Simulation files
with 0 violations and scoped diff check passed. This accepts only the restart no-duplicate marker;
complete NPC death/loot branches, tables, random parity and client/network presentation remain
deferred. Evidence: `Build/diagnostics/main-migration/task-10-npc-death-loot-restart/20260823-230000/`;
research: `docs/research/2026-08-23-npc-death-loot-restart-boundary.md`.

# 2026-08-24 Projectile tile-stop lifecycle boundary

Recorded the source-backed generic projectile tile collision boundary in
`docs/research/2026-08-24-projectile-tile-stop-boundary.md` and
`Build/diagnostics/main-migration/task-10-projectile-tile-stop/20260824-000000/`. The Combat
verifier confirms a solid authoritative tile despawns the swept projectile before it can damage
the NPC behind it; MainBoundary checked 771 files with 0 violations and scoped diff check passed.
Exact TileCollision response, wet/slope/type-specific branches, special tile rules and projectile
save/restart semantics remain deferred.

# 2026-08-24 NPC death tick-order boundary

Extended `Test/Terraria.Dome.TickOrder.Verification` with a source-backed entity ordering
assertion: `ResolveCombat` precedes `CommitDomainCommands`, which precedes `PublishSnapshot`; the
completed tick exposes inactive NPC replication and exactly one deterministic loot item. The
focused TickOrder verifier passed, MainBoundary checked 771 files with 0 violations, and scoped
diff check passed. This accepts only the supported death/loot ordering; special death branches,
tables, global random, event progression and presentation remain deferred. Evidence:
`Build/diagnostics/main-migration/task-10-npc-death-tick-order/20260824-001000/`;
research: `docs/research/2026-08-24-npc-death-tick-order-boundary.md`.

# 2026-08-24 World-item stack merge boundary

Recorded the source-backed narrow merge child in
`docs/research/2026-08-24-world-item-stack-merge-boundary.md` and
`Build/diagnostics/main-migration/task-10-world-item-stack-merge/20260824-002000/`. Existing
Items verification confirms deterministic compatible stack merge, bounded quantities, revision
updates and same-tick guards; MainBoundary checked 771 files with 0 violations and scoped diff
check passed. Passive eligibility, owner reservation, shimmer, exact source distance,
interpolation and network updates remain deferred.

# 2026-08-24 World-item owner reservation boundary

Added the source-backed owner authorization predicate from Version4 `Main.cs:13162-13194` and
`WorldItem.cs:257-345`. `ItemWorldStateComponent` now preserves the legacy `255` unreserved
sentinel plus a reserved player id/age, and `WorldItemPickupSystem` rejects a non-reserved player
before inventory mutation. The focused Items verifier passed, MainBoundary checked 773 Simulation
files with 0 violations, and the scoped diff check passed. Evidence:
`Build/diagnostics/main-migration/task-11-item-owner-reservation/20260824-017000/`;
research: `docs/research/2026-08-24-world-item-owner-reservation-boundary.md`.

This accepts authorization only. Nearest-owner selection, inactive-owner re-evaluation, the
300-tick `FindOwner` schedule, network reservation messages, shimmer/enemy pickup and persistence
remain explicitly deferred.

# 2026-08-24 World-item reservation persistence qualification

Audited the legacy save and runtime surfaces before extending Dome persistence. `WorldItem.ResetStats`
and `SetDefaultsBringOver` reset reservation state, while `Main` maintains it only in the runtime
tick loop. The V319 WLD writer has chest item serialization but no world-item reservation fields;
therefore no reservation bytes were added to the Dome format and no restart parity claim was made.
Evidence: `docs/research/2026-08-24-world-item-reservation-persistence-boundary.md` and
`Build/diagnostics/main-migration/task-11-item-owner-reservation-persistence/20260824-018000/`.

This branch remains explicitly deferred pending a server-session/restart contract. The in-memory
authorization card remains accepted independently.

# 2026-08-24 World-item shimmer and encumbrance pickup qualification

Audited `Player.GrabItems` and `WorldItem` enemy-pickup guards. The remaining source predicate
depends on `shimmerTime`, `shimmered` velocity, `noGrabDelay`, `ItemID.Sets.IgnoresEncumberingStone`,
enemy timers and type-specific coin/special pickup rules. The current ECS has no authoritative
state or definition owner for those facts, so no boolean approximation or default table entry was
added. Evidence: `docs/research/2026-08-24-world-item-shimmer-pickup-boundary.md` and
`Build/diagnostics/main-migration/task-11-item-shimmer-pickup/20260824-019000/`.

The generic pickup delay and reservation authorization remain independently accepted; shimmer,
encumbrance and enemy pickup branches remain deferred.

# 2026-08-24 NPC valid-target ghost boundary

Added the source-backed player-target validity guard from `NPC.HasValidTarget`:
`NpcTargetSelectionSystem` now rejects ghost candidates before nearest-distance selection while
preserving stable-id tie breaking. The NPC focused verifier, MainBoundary and scoped diff gate are
captured in `Build/diagnostics/main-migration/task-10-npc-valid-target-ghost/20260824-020000/`;
research: `docs/research/2026-08-24-npc-valid-target-ghost-boundary.md`.

NPC-target ids, type-specific targeting, line-of-sight and AI target changes remain deferred.

# 2026-08-24 NPC talk authority qualification

Audited `NPC.CanBeTalkedTo` and the V1456 `SyncTalkNPC` route. The source predicate requires
`isLikeATownNPC`, `aiStyle == 7`, zero vertical velocity and, in the town-pet variant, an
`NPCID.Sets.IsTownPet` definition. `TerrariaSession.AcceptClientTalkNpc` only consumes packet
shape/ownership and explicitly has no Dome conversation authority. No interaction success or
chat/shop side effect was inferred. Evidence:
`docs/research/2026-08-24-npc-talk-authority-boundary.md` and
`Build/diagnostics/main-migration/task-10-npc-talk-authority/20260824-021000/`.

# 2026-08-24 Projectile friendly-target boundary

Added the generic `Projectile.friendly` gate before NPC damage candidate creation. A typed
`ProjectileTargetEligibilitySystem` now allows only friendly projectiles into the NPC damage
route; hostile/player damage and reflected/type-specific branches remain separate. Evidence:
`docs/research/2026-08-24-projectile-friendly-target-boundary.md` and
`Build/diagnostics/main-migration/task-10-projectile-friendly-target/20260824-022000/`.

# 2026-08-24 Projectile hostile-player qualification

Audited `Projectile.Damage_CanDealDamage` and the PVP/player branch. The source gate depends on
many projectile type, `aiStyle`, `ai` and `localAI` states plus owner, collision and immunity
contracts. The current ECS lacks those tables and a generic hostile-player collision owner, so it
does not infer player damage from `Hostile` alone. Evidence:
`docs/research/2026-08-24-projectile-hostile-player-boundary.md` and
`Build/diagnostics/main-migration/task-10-projectile-hostile-player/20260824-023000/`.

# 2026-08-24 M-024 delayed-process caller inventory

Re-audited Version4 `Main.DelayedProcesses` and `DelayedProcessesInGame` with source hash and
exact phase ranges. A complete `.cs` tree search found declarations plus indexed `MoveNext`/
`Remove` consumption only, with no `.Add` caller or recoverable process identity. M-024 remains
`unknown/deferred`; no generic coroutine scheduler was added. MainBoundary checked 771 files with
0 violations and scoped diff check passed. Evidence:
`Build/diagnostics/main-migration/task-10-delayed-process-caller-inventory/20260824-003000/`;
research: `docs/research/2026-08-24-delayed-process-caller-inventory.md`.

# 2026-08-24 M-014 MainThreadAction caller inventory

Re-audited `Main.QueueMainThreadAction` and its two source callers with fresh Main/WorldGen
hashes and exact lines. The queue drains arbitrary delegates after `DoUpdate`; section loading is
host/client state and background transform follow-up is WorldGeneration/Server orchestration.
Neither caller has typed identity, cancellation, retry, persistence/restart or protocol semantics,
so M-014 remains `unknown/deferred` and no arbitrary Simulation action queue was added.
MainBoundary checked 771 files with 0 violations and scoped diff check passed. Evidence:
`Build/diagnostics/main-migration/task-10-main-thread-action-caller-inventory/20260824-004000/`;
research: `docs/research/2026-08-24-main-thread-action-caller-inventory.md`.

# 2026-08-24 Main member coverage matrix evidence links

Updated `docs/research/2026-08-20-main-member-coverage-matrix.md` to link the recent NPC death
restart/tick-order, projectile penetration/tile-stop, item stack-merge, M-014 caller inventory
and M-024 caller inventory cards. The accounting statuses remain unchanged: M-001 is still
deferred/planned at aggregate level, M-014/M-024 remain deferred, and accepted rows describe only
narrow owned predicates rather than complete domain parity.

# 2026-08-24 WLD header disposition reconciliation

Reconciled `docs/research/2026-08-20-wld-header-disposition-matrix.md` with the accepted WLD
clock/rain cards: the v1+ Double time/day-time pair is now `accepted` through the versioned
fractional `WorldClock` owner, and the v53+ rain activity/duration/maximum triple is now
`accepted` as independent raw facts. The `FixEndlessRainWorlds` secret-seed repair membership,
ore tiers, and no-owner/client-only header groups remain blocked or deferred; no unsupported
default was introduced.
Evidence: `Build/diagnostics/main-migration/task-9-wld-header-matrix/20260824-005000/`.

# 2026-08-24 WLD generator version qualification

Accepted one direct WLD identity field: v179+ `WorldGeneratorVersion` now flows from the
header parser through compatibility projection into immutable `WorldMetadata`, with an optional
append-only v29 persistence tail. Pre-v179 absence remains unknown. UUID, ore tiers,
secret-seed repair and generator-specific runtime behavior remain deferred; this is not a
random-stream or full WLD parity claim. Focused parser, WorldImport and Persistence verifiers
passed. Evidence: `Build/diagnostics/main-migration/task-9-wld-generator-version/20260824-010000/`.

# 2026-08-24 WLD ore-tier qualification boundary

Audited the source ore-tier group and recorded the blocker rather than importing defaults. v216+
stores Copper/Iron/Silver/Gold tile IDs directly, while older layouts set them to `-1` and
`CheckSavedOreTiers()` repairs missing values by counting paired tile types in the loaded world.
The current ECS model has no typed immutable saved-tier owner or source-backed repair phase, so
the group remains explicitly blocked; parser alignment and version-matrix evidence remain valid.
Evidence: `docs/research/2026-08-24-wld-ore-tier-qualification-boundary.md`;
`Build/diagnostics/main-migration/task-9-wld-ore-tier-qualification/20260824-011000/`.

# 2026-08-24 secret-seed endless-rain repair boundary

Recorded the exact `FixEndlessRainWorlds` predicate and its dependency on canonical seed-text
parsing plus the `rainsForAYear` secret-seed registry. The current Compatibility snapshot does
not provide either input, so old-layout endless-rain repair remains blocked and fail-closed;
numeric seed, generator version and Remix flag are not used as substitutes. Existing WorldImport
verification covers rejection without context and explicit non-secret repair. Evidence:
`docs/research/2026-08-24-secret-seed-rain-repair-boundary.md`;
`Build/diagnostics/main-migration/task-9-secret-seed-rain-repair/20260824-012000/`.

# 2026-08-24 Main tick-order reconciliation

Recorded the source order `UpdateTime -> WorldGen.UpdateWorld -> UpdateInvasion -> UpdateServer`
and the earlier projectile/item updates without importing the whole Main loop. The accepted ECS
relation is narrower: `ApplyWorldClock` resolves the supported clock/rate state before invasion
travel, and domain commits precede snapshot publication. The complete WorldGen owner and legacy
projectile/item-before-time parity remain partial/unknown. Evidence:
`docs/research/2026-08-24-main-tick-order-reconciliation.md`;
`Build/diagnostics/main-migration/task-5-tick-order-reconciliation/20260824-013000/`.

# 2026-08-24 WLD UUID identity qualification

Accepted the v181+ WLD `UniqueId` direct identity route. The UUID now flows through nullable
legacy/compatibility metadata into immutable `WorldMetadata`, V1456 `WorldData`, and an optional
v30 Dome persistence tail. Pre-v181 absence remains unknown; no UUID generation, map filename
policy or client storage was introduced. Parser, WorldImport and Persistence focused verifiers
passed. Evidence: `docs/research/2026-08-24-wld-uuid-identity-boundary.md`;
`Build/diagnostics/main-migration/task-9-wld-uuid-identity/20260824-014000/`.

# 2026-08-24 WLD raw seed-text identity

Accepted raw v179+ seed-text recovery through nullable legacy/compatibility metadata and
`WorldMetadata.SeedText`, with append-only Dome persistence v31. The card intentionally does not
parse secret seeds, expose raw text on protocol, or infer `rainsForAYear`; secret-seed identity
and repair remain deferred. Parser, WorldImport and Persistence focused verifiers passed.
Evidence: `docs/research/2026-08-24-wld-seed-text-qualification-boundary.md`;
`Build/diagnostics/main-migration/task-9-wld-seed-text-qualification/20260824-015000/`.

# 2026-08-24 secret-seed crypto predicate audit

Audited the exact source `WorldGen.SecretSeed.Check` path. It normalizes each seed-code segment,
then uses `Secrets.ToSecret` with BCrypt raw crypt operations, a fixed salt, cost 4 and a 1000
swap loop before comparing the encoded `rainsForAYear` code. The Simulation has no BCrypt or
source-compatible crypto owner, so no plain-text or hand-rolled approximation was added; the
repair branch remains explicit-context/fail-closed. Evidence:
`docs/research/2026-08-24-secret-seed-rain-repair-boundary.md`;
`Build/diagnostics/main-migration/task-9-secret-seed-rain-repair/20260824-012000/`.

# 2026-08-24 NPC player-readiness spawn boundary

Added a typed `NpcSpawnPlayerReadiness` snapshot and source-backed
`NpcSpawnPlayerReadinessQuery` for `NPC.Spawner.CanSpawnEnemiesNear`: inactive/dead players,
Journey per-player disabled spawn power and Moon Lord neighborhood are rejected; ready players
are accepted. The NPC verifier passed. Spawn tile/rate formulas, player stat gates, type tables
and broader lifecycle remain deferred. Evidence:
`docs/research/2026-08-24-npc-player-readiness-boundary.md`;
`Build/diagnostics/main-migration/task-10-npc-player-readiness/20260824-016000/`.

# 2026-08-24 WLD source-oracle drift reconciled

The isolated `WorldFile.cs` copy had the same normalized content and line count as the canonical
Version4 source but differed only in trailing whitespace, causing the exact SHA gate to stop the
parser verifier. The isolated copy was synchronized from the canonical source; its hash now
matches `92DF79BB7AE89393327AE9EF6B7B78762909E5B2E197C0F3FCCFE709E152F289`, and the parser
verifier is green again. No verifier relaxation or semantic source change was made.

# 2026-08-24 Remaining ECS blocker review refresh

Refreshed the reduced acceptance review after the player-facing input and invasion progress
clamp cards. The focused PlayerSimulation, WorldRules, NPC, Combat and Items verifiers remain
green, and MainBoundary remains at 775 checked Simulation files with 0 violations. The review
still records `incomplete`: M-001, M-014, M-024, B-007, M-007, M-008 and M-009 lack the required
source-backed contracts or ordering oracles, while complete entity/event lifecycle, static NPC/
projectile/item tables, random starts and client/presentation branches remain deferred. No
aggregate initializer, arbitrary action queue, generic delayed-process scheduler, or fabricated
legacy random parity was introduced. Evidence:
`Build/diagnostics/main-migration/task-10-open-responsibilities-review/20260824-028000/status.txt`.

# 2026-08-24 Player respawn timer boundary

Added a source-backed lifecycle guard: respawn delays above legacy `respawnTimerMax` (3600 ticks)
are rejected, and a forged negative timer cannot trigger respawn or mutate runtime state. The
existing PlayerLifecycle verifier covers the new invalid cases. Full `KillMe` inventory, death
reason, achievement, presentation, network and client respawn branches remain deferred. Evidence:
`docs/research/2026-08-24-player-respawn-timer-boundary.md`.

Focused evidence: `Build/diagnostics/main-migration/task-10-player-respawn-timer/20260824-030000/`;
MainBoundary checked 776 Simulation files with 0 violations and scoped diff check exited 0.

# 2026-08-24 Player respawn tick clamp boundary

The player lifecycle tick owner now clamps a forged negative inactive respawn timer to zero before
the countdown path and does not enqueue an automatic respawn for that invalid state. Positive
timers retain one-tick decrement and zero-transition scheduling. Hardcore/ghost, client-requested
respawn, death presentation, inventory, network and other `UpdateDead` branches remain deferred.
Evidence: `docs/research/2026-08-24-player-respawn-tick-clamp-boundary.md`.

Focused evidence: `Build/diagnostics/main-migration/task-10-player-respawn-tick-clamp/20260824-033000/`;
MainBoundary checked 782 Simulation files with 0 violations and scoped diff check exited 0.

# 2026-08-24 Projectile penetration domain boundary

The projectile penetration component now enforces the source-compatible domain: positive counts or
the `-1` infinite sentinel. Zero and values below `-1` fail before component state is created,
closing composition/restore bypasses around the existing spawn validator. Type/AI-specific
immunity, bounce/transform, random penetration and client/network cadence remain deferred.
Evidence: `docs/research/2026-08-24-projectile-penetration-domain-boundary.md`.

Focused evidence: `Build/diagnostics/main-migration/task-10-projectile-penetration-domain/20260824-034000/`;
MainBoundary checked 783 Simulation files with 0 violations and scoped diff check exited 0.

# 2026-08-24 Projectile lifetime domain boundary

The projectile lifetime component now rejects zero and negative remaining ticks at construction,
closing composition/restore bypasses around the existing positive spawn lifetime validator. The
one-tick expiry and replication boundary remains unchanged. Type-specific/random lifetime changes,
AI, network and client presentation remain deferred. Evidence:
`docs/research/2026-08-24-projectile-lifetime-domain-boundary.md`.

Focused evidence: `Build/diagnostics/main-migration/task-10-projectile-lifetime-domain/20260824-035000/`;
MainBoundary remained at 783 Simulation files with 0 violations and scoped diff check exited 0.

# 2026-08-24 Item-use timer clamp boundary

Aligned the item-use tick owner with the source negative animation/time normalization: negative
cooldown and animation ticks now clamp to zero before channel-state evaluation, preventing a forged
negative animation timer from keeping `IsChanneling` active indefinitely. Valid positive cooldowns
retain deterministic decrement behavior. Full item style/reuse/input/client branches remain
deferred. Evidence: `docs/research/2026-08-24-item-use-timer-clamp-boundary.md`.

Focused evidence: `Build/diagnostics/main-migration/task-10-item-use-timer-clamp/20260824-036000/`;
MainBoundary remained at 783 Simulation files with 0 violations and scoped diff check exited 0.

# 2026-08-24 NPC death input boundary

The NPC death publisher now fails closed for invalid handle, non-finite position and missing loot
table identity before emitting a typed death fact. Valid active zero-health transitions remain
unchanged. Full `checkDead` reasons, type-specific loot, bosses, achievements, network/random and
client branches remain deferred. Evidence: `docs/research/2026-08-24-npc-death-input-boundary.md`.

# 2026-08-24 NPC worm trigger lifecycle boundary

The NPC segment owner now enforces the source entry condition for `CheckActive_WormSegments`:
an active, living trigger cannot emit child despawn commands. Reciprocal graph validation,
caller-supplied worm classification, cycle guards and typed `Killed` commands remain intact.
The NPC composition verifier and MainBoundary passed; full NPC type/AI mapping, worm construction,
loot, network and client cadence remain deferred. Evidence:
`Build/diagnostics/main-migration/task-10-npc-worm-trigger-boundary/20260824-031000/`.

# 2026-08-24 NPC time-left lifecycle boundary

Aligned the typed NPC lifecycle owner with the source `CheckActive` rule: after decrement, every
`TimeLeft <= 0` state becomes inactive. A forged negative timer now produces a timed-out lifecycle
transition instead of leaving the NPC active. Type-specific active times, AI/spawn tables, revenge
state, network synchronization, worm construction, loot and client presentation remain deferred.
Evidence: `docs/research/2026-08-24-npc-timeleft-boundary.md`.

Focused evidence: `Build/diagnostics/main-migration/task-10-npc-timeleft-boundary/20260824-032000/`;
MainBoundary checked 780 Simulation files with 0 violations and scoped diff check exited 0.

# 2026-08-24 NPC death input boundary

The NPC death publisher now fails closed for invalid handle, non-finite position and missing loot
table identity before emitting a typed death fact. Valid active zero-health transitions remain
unchanged. Full `checkDead` reasons, type-specific loot, bosses, achievements, network/random and
client branches remain deferred. Evidence:
`docs/research/2026-08-24-npc-death-input-boundary.md`.

Focused evidence: `Build/diagnostics/main-migration/task-10-npc-death-input-boundary/20260824-037000/`;
MainBoundary checked 784 Simulation files with 0 violations and scoped diff check exited 0.

# 2026-08-24 Item-drop position boundary

The deterministic item-drop owner now rejects non-finite spawn positions before selecting or
creating world-item commands. Existing source identity, tick, spawn-source, weighted-chain and
deterministic drop behavior remains unchanged. Complete `Item.NewItem` source classification,
item tables, shimmer/encumbrance, network and client branches remain deferred. Evidence:
`docs/research/2026-08-24-item-drop-position-boundary.md`.

Focused evidence: `Build/diagnostics/main-migration/task-10-item-drop-position-boundary/20260824-038000/`;
Simulation build and Items verifier exited 0, MainBoundary checked 784 files with 0 violations,
and scoped diff check exited 0.

# 2026-08-24 World-item player identity boundary

World-item reservation authorization now rejects invalid player handles before evaluating reserved
or unreserved pickup. Valid unreserved players and matching reserved owners remain accepted, while
non-owners remain rejected. Owner selection cadence, shimmer, encumbrance, enemy pickup, network
reservation and restart persistence remain deferred. Evidence:
`docs/research/2026-08-24-world-item-player-identity-boundary.md`.

# 2026-08-24 World-item stack geometry boundary

The world-item stack owner now rejects non-finite receiver/donor positions and merge distance before
distance arithmetic or mutation. Finite compatible merges retain deterministic ordering, stack
limits and revision updates. Legacy passive threshold/interpolation, shimmer, owner cadence,
network and full item lifecycle remain deferred. Evidence:
`docs/research/2026-08-24-world-item-stack-geometry-boundary.md`.

Focused evidence: `Build/diagnostics/main-migration/task-10-world-item-stack-geometry/20260824-043000/`;
Items verifier and MainBoundary exited 0, MainBoundary checked 785 files with 0 violations. The
run observed pre-existing nullable warnings in `DomeSimulation.cs:1297,1323`; they are outside
this card and do not change the accepted narrow predicate.

# 2026-08-24 Item lifecycle coverage reconciliation

Updated M-021 in the Main responsibility matrix to include the accepted world-item stack geometry
boundary. The row still explicitly defers nearest-owner selection, re-evaluation, persistence,
shimmer/encumbrance/enemy pickup and complete item lifecycle parity.

# 2026-08-24 Tick-order acceptance slice

The existing named tick schedule was revalidated as a focused Task 5.3 acceptance slice. Ten
assertions passed: stable phases, paused-tick boundary, deterministic equal-sequence ordering,
world-rate and invasion ordering, projectile phase placement, combat commit before publication,
and NPC death/loot visibility before snapshot publication. This does not claim exact legacy Main
ordering for unsupported client/content/random branches. Evidence:
`Build/diagnostics/main-migration/task-12-tick-order-acceptance/20260824-040000/`.

# 2026-08-24 Main coverage matrix reconciliation

Refreshed Task 5.1 accounting rows M-005, M-018, M-019, M-020 and M-021 with the latest narrow
tick-order, player, NPC, projectile and item evidence. The update preserves explicit deferred
branches and does not promote M-001, M-014 or M-024, complete random/table/lifecycle parity, or
client/presentation behavior. MainBoundary checked 785 Simulation files with 0 violations and the
matrix scoped diff check exited 0. Evidence:
`Build/diagnostics/main-migration/task-10-coverage-matrix-reconciliation/20260824-041000/`.

# 2026-08-24 Coverage accounting gate

Revalidated the executable MainBoundary coverage gate after the matrix reconciliation. The gate
recognized 30 M responsibility rows, required columns, accepted/deferred/excluded classifications,
non-empty source/owner fields and all required server domains, while Simulation remained free of
forbidden Main/client/transport/Protocol/Server dependencies. This is an accounting gate only;
deferred physical deletions and unresolved M-001/M-014/M-024, lifecycle/table/random/client gaps
remain open. Evidence:
`Build/diagnostics/main-migration/task-10-coverage-gate/20260824-042000/`.

# 2026-08-24 Projectile definition registry boundary

`ProjectileDefinitionRegistry` now validates supported immutable definitions before registration:
positive type/behavior IDs, non-negative damage, positive lifetime, finite positive collider and
positive-or-`-1` penetration. Complete legacy type/AI defaults and reflected/hostile/client tables
remain deferred. Evidence:
`docs/research/2026-08-24-projectile-definition-registry-boundary.md` and
`Build/diagnostics/main-migration/task-10-projectile-definition-registry-boundary/20260824-082000/`.

# 2026-08-24 World-item store position boundary

`WorldItemStore.Add` now rejects non-finite coordinates before creating its Arch-backed entry,
closing the direct store bypass around `WorldItemSpawnSystem`. Positive identity, duplicate map
ownership and inactive tombstones remain unchanged. Owner search, persistence cadence,
shimmer/encumbrance/enemy pickup and network/client branches remain deferred. Evidence:
`docs/research/2026-08-24-world-item-store-position-boundary.md` and
`Build/diagnostics/main-migration/task-11-world-item-store-position-boundary/20260824-080000/`.

# 2026-08-24 Tile command sequence overflow boundary

`TileChangeCommitSystem` now rejects `long.MaxValue` command sequences before tile mutation, avoiding
overflow when computing `NextSequence`. The WorldGeneration verifier and deterministic atomic
commit checks passed. Complete Main tile update, network and client branches remain deferred.
Evidence: `docs/research/2026-08-24-tile-sequence-overflow-boundary.md` and
`Build/diagnostics/main-migration/task-12-tile-sequence-overflow-boundary/20260824-078000/`.

# 2026-08-24 Entity store handle boundary

`PlayerStore.Add` and `NpcStore.Add` now reject default/non-positive domain handles before ownership
map mutation. Duplicate positive handles remain rejected. Full slot allocation, lifecycle and
client/network behavior remain deferred. Evidence:
`docs/research/2026-08-24-entity-store-handle-boundary.md` and
`Build/diagnostics/main-migration/task-10-entity-store-handle-boundary/20260824-076000/`.

# 2026-08-24 Projectile replication scalar boundary

Projectile replication now rejects non-positive replication IDs and negative revisions before
projection. Live entity and snapshot field semantics remain unchanged. Packet cadence, type/AI,
collision, network and client branches remain deferred. Evidence:
`docs/research/2026-08-24-projectile-replication-scalar-boundary.md` and
`Build/diagnostics/main-migration/task-10-projectile-replication-scalar-boundary/20260824-074000/`.

# 2026-08-24 Projectile spawn input boundary

`ProjectileSpawnSystem.Spawn` now validates direct owner/identity, finite numeric, lifetime,
penetration-domain and type-match inputs before Arch allocation. Valid component composition remains
unchanged. Complete projectile type/AI defaults, owner routing, network cadence and client
presentation remain deferred. Evidence:
`docs/research/2026-08-24-projectile-spawn-input-boundary.md` and
`Build/diagnostics/main-migration/task-10-projectile-spawn-input-boundary/20260824-072000/`.

# 2026-08-24 Player lifecycle identity boundary

`PlayerLifecycleSystem.Advance` now skips stale or destroyed Arch entities before lifecycle
component access, preventing a stale ownership mapping from aborting the tick or scheduling a
respawn. Live countdown and negative-timer clamp behavior remain unchanged. Full `UpdateDead`,
presentation/network and inventory branches remain deferred. Evidence:
`docs/research/2026-08-24-player-lifecycle-identity-boundary.md` and
`Build/diagnostics/main-migration/task-10-player-lifecycle-identity-boundary/20260824-070000/`.

# 2026-08-24 World-item store owner boundary

`WorldItemStore.RecordPickup` now rejects default/non-positive player handles before writing
`ItemOwnershipComponent`. Valid owner revision/source projection remains unchanged. Owner search
cadence, reservation re-evaluation, restart persistence, shimmer/encumbrance/enemy pickup and
client/network effects remain deferred. Evidence:
`docs/research/2026-08-24-world-item-store-owner-boundary.md` and
`Build/diagnostics/main-migration/task-11-world-item-store-owner-boundary/20260824-068000/`.

# 2026-08-24 Projectile replication identity boundary

`ProjectileReplicationSystem.Project` now rejects absent or destroyed Arch entities explicitly
before component projection. Live replication fields remain unchanged. Replication cadence, packet
parity, complete type/AI, collision, network and client branches remain deferred. Evidence:
`docs/research/2026-08-24-projectile-replication-identity-boundary.md` and
`Build/diagnostics/main-migration/task-10-projectile-replication-identity-boundary/20260824-066000/`.

# 2026-08-24 Projectile identity boundary

Projectile behavior and lifetime systems now return before component access for absent or destroyed
Arch entities. Live behavior/lifetime semantics remain unchanged. Complete projectile AI/type
tables, collision/reflection, network cadence and client presentation remain deferred. Evidence:
`docs/research/2026-08-24-projectile-identity-boundary.md` and
`Build/diagnostics/main-migration/task-10-projectile-identity-boundary/20260824-064000/`.

# 2026-08-24 NPC spawn position boundary

`NpcSpawnCommitSystem` now rejects non-finite spawn coordinates even when called directly, before
Arch entity allocation. The NPC verifier passed; definition, replication, difficulty and occupancy
checks remain unchanged. Complete spawn search/rate formulas, type tables, random starts and client
effects remain deferred. Evidence:
`docs/research/2026-08-24-npc-spawn-position-boundary.md` and
`Build/diagnostics/main-migration/task-10-npc-spawn-position-boundary/20260824-062000/`.

# 2026-08-24 NPC movement identity boundary

`NpcMovementIntentSystem` now returns before component access when given a default or destroyed
Arch entity. Live entities retain typed behavior and movement-intent projection. Complete AI,
static tables, collision/LOS, lifecycle and client/network effects remain deferred. Evidence:
`docs/research/2026-08-24-npc-movement-identity-boundary.md` and
`Build/diagnostics/main-migration/task-10-npc-movement-identity-boundary/20260824-060000/`.

# 2026-08-24 NPC contact input boundary

NPC contact damage now fails closed for invalid NPC/player handles, inactive or cooldown players,
non-finite positions and non-positive/non-finite collider geometry before AABB intersection. Valid
overlap behavior remains unchanged. Type-specific immunity, knockback, difficulty, LOS and
presentation/network effects remain deferred. Evidence:
`docs/research/2026-08-24-npc-contact-input-boundary.md` and
`Build/diagnostics/main-migration/task-10-npc-contact-input-boundary/20260824-058000/`.

# 2026-08-24 NPC behavior input boundary

The supported NPC chase/town-home behavior owner now rejects non-finite NPC or target positions;
the home movement owner rejects non-finite current position before mutating intent or timeout.
The NPC verifier passed. Complete AI families, pathfinding, home search/teleport, collision/LOS,
type tables and client behavior remain deferred. Evidence:
`docs/research/2026-08-24-npc-behavior-input-boundary.md` and
`Build/diagnostics/main-migration/task-10-npc-behavior-input-boundary/20260824-056000/`.

# 2026-08-24 Projectile motion input boundary

Linear and gravity projectile behaviors now reject non-finite transform/velocity state and
negative behavior ticks before mutation. Valid movement equations and phase updates remain
unchanged. Complete type/AI tables, collision/reflection/bounce, random motion, network cadence
and client presentation remain deferred. Evidence:
`docs/research/2026-08-24-projectile-motion-input-boundary.md` and
`Build/diagnostics/main-migration/task-10-projectile-motion-input-boundary/20260824-054000/`.

# 2026-08-24 World-item pickup position boundary

World-item pickup now rejects non-finite item positions before distance arithmetic. The focused
Items verifier passed; the existing player/range/active/reservation gates remain intact. Nearest
owner cadence, re-evaluation, shimmer/encumbrance/enemy pickup and complete item tables/lifecycle
remain deferred. Evidence:
`docs/research/2026-08-24-world-item-pickup-position-boundary.md` and
`Build/diagnostics/main-migration/task-11-item-pickup-position-boundary/20260824-052000/`.

# 2026-08-24 NPC encoded target routing boundary

Added `NpcTargetRoutingSystem` for the source-backed encoded NPC target input contract: only
`target >= 300 && target < 300 + maximumNpcCount` is eligible when the NPC target family is
supported; translation subtracts 300 and requires an active, non-default candidate. The NPC
verifier passed. Type-table ownership, target acquisition, AI changes, line-of-sight and complete
NPC lifecycle remain deferred. Evidence:
`docs/research/2026-08-24-npc-target-routing-boundary.md` and
`Build/diagnostics/main-migration/task-10-npc-target-routing-boundary/20260824-050000/`.

# 2026-08-24 NPC target-selection input boundary

Added a narrow fail-closed boundary to `NpcTargetSelectionSystem`: default entity identities,
non-positive stable player ids, inactive/dead/ghost candidates and non-finite NPC or candidate
positions are rejected before nearest-target distance and stable-id tie breaking. The focused NPC
verifier and MainBoundary passed (`786` Simulation files, `0` violations). Legacy NPC-target
routing (`target >= 300`), type-specific target rules, line-of-sight, AI tables, conversation and
client/presentation branches remain deferred. Evidence is in
`docs/research/2026-08-24-npc-target-input-boundary.md` and
`Build/diagnostics/main-migration/task-10-npc-target-input-boundary/20260824-044000/`.

# 2026-08-24 Reduced review evidence reconciliation

Synchronized the reduced acceptance review's MainBoundary count with the latest coverage-gate
run: 785 Simulation files and 0 violations. Accepted child cards and deferred blocker rows remain
unchanged; this is evidence maintenance only and does not expand the approximately 40 percent
validation scope. The review continues to report `incomplete`.
# 2026-08-23 Server ECS convergence current qualification audit

The current worktree retains the accepted Simulation chest expected-revision guard and its
server-session cursor integration. `WorldObjects`, `Persistence`, `WorldRules`, `WorldClock`,
`WorldImport`, `MainBoundary`, `PlayerLifecycle` loopback, `Combat` loopback, and Completion
verifiers have fresh exit-0 evidence. PlayerAuthority remains intermittent in the current
bootstrap path: repeated runs can time out waiting for `WorldData` after the complete bootstrap
packet sequence, so it is not claimed green for this audit. The focused trace is
`Build/diagnostics/server-ecs-convergence/P5-worldobjects/20260822-2200/trace.md`.

TileEntity qualification is now source-backed and explicit in
`docs/research/2026-08-23-tile-entity-authority-qualification.md`. Complete `TETrainingDummy`
behavior depends on typed NPC-488 linkage, tile validity, activation/deactivation and network
side effects that have no current ECS owner; it remains deferred. Capability family G remains
`Partial` with score `0`. Physical deletion and WorldGen gates remain independently false.

# 2026-08-23 Server ECS convergence bootstrap queue fix

The intermittent PlayerAuthority `WorldData` bootstrap failure was traced to
`DomeServer.FlushNetworkIsolationOutboundAsync` awaiting session socket-drain completion inside
the Simulation tick loop. It now enqueues frames into the existing bounded session writer and
observes faulted completion tasks without blocking protocol command consumption. The fix preserves
queue-full/failed-writer checks and does not change V1456 frame layout. Fresh evidence records a
five-run PlayerAuthority repetition with all runs exit `0`, FullClientBootstrap exit `0`, and
PlayerLifecycle, WorldObjects, Hardening, and SessionReplication loopbacks exit `0`:
`Build/diagnostics/server-ecs-convergence/P5-worldobjects/20260822-2200/trace.md`.
# 2026-08-22 Initializer responsibility-family re-audit

Re-audited `Main.Initialize_AlmostEverything` source order against the current Simulation owners.
No new independent server initializer family has a complete source-backed owner, observable state,
and restart contract, so no aggregate initializer was added and M-001 remains deferred/planned.
Unresolved LeashedEntity, NPC interaction, fish-drop, pylon/shop, armor/repair, world-hook and chat
families remain explicitly open. Evidence:
`docs/research/2026-08-22-initializer-responsibility-family-inventory.md` and
`Build/diagnostics/main-migration/task-13-initializer-family-inventory/20260822-090000/`.
