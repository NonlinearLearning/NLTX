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
