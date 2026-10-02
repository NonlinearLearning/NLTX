# Authoritative P04 Player Lifecycle And Interaction

partitionId: P04  
sourceSessionId: `910f8127b89041cc98bf563325fab350`  
designStatus: proposed  
evidenceStatus: partial  
verificationStatus: partial  

## Purpose And Scope

This document turns the P04 static decomposition report into a target System design. It covers
the report's 13 leaf groups and 112 members as a coverage boundary; those groups do not imply 13
Systems. The design remains proposed. It does not assert that the new APIs are connected or
behaviorally equivalent.

The source report is
[`2026-09-18-system-decomposition-authoritative-P04-player-lifecycle-interaction.md`](../../system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P04-player-lifecycle-interaction.md).
The implementation sequence and initial verification slice are in
[`authoritative-P04-player-lifecycle-interaction.md`](../execution/authoritative-P04-player-lifecycle-interaction.md).
The current slice has lifecycle/rest System implementations and declaration-only packet adapter
interfaces, including type 65 and type 118 input contracts. The affected Player project build and
five-check core verifier pass. The dead-tick check also covers the reference-derived world-join
timer decrease, expiry, and non-local guard; the death check covers the server spectating-stop
intent. Packet behavior, production callers, runtime scheduling, and Version4 parity remain
unverified. This design remains proposed; local compile/test results do not establish migration
success.

## Source Baselines

The report's CPG queries use the Version4 SQLite index with manifest
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`. On 2026-10-01 the read-only
Query API was reinitialized and returned `ImportStatus=complete`, 967 shards, project fingerprint
`521CBD4C11E7EB731256C232350A54099F431D0E59EBDF3C38880281F169C11B`, and
`SourceSnapshotId=null`. `Find-CpgCallSites(Spawn)` returned 4 selected-path sites (2 in
`Player.cs`, 2 in `MessageBuffer.cs`); `StopVanityActions` returned 6 sites in `Player.cs`.
Member-use queries over `Player.cs` and `MessageBuffer.cs` returned 17 `active`, 21 `dead`, 3
`deadTime`, 4 `pvpDeath`, 11 `spectating`, and 8 `respawnTimer` facts. Confirmed write directions
were 3, 4, 1, 2, 4, and 4 respectively; other access directions remain `partial`/`Unknown`.
`complete` describes the selected indexed query, not a closed caller graph or runtime scheduling.
The manifest does not bind those facts to the current Version4 file hashes.

The designated SS14 reference was read for ECS shape only. `Content.Shared/Humanoid/HumanoidProfileSystem.cs`
mutates the owned component, marks it dirty, and raises a change event; it does not make the caller
a second component writer. The P04 packet-facing types follow that boundary at the declaration
level only: adapter and route-query interfaces have signatures, but no packet decisions or effects
are implemented.

The 2026-10-01 follow-up query resolved `AdjustRespawnTimerForWorldJoining` with one selected
`Player.cs` call site and `Spawn` with four selected sites across `Player.cs` and
`MessageBuffer.cs`. `UpdateDead` call-site lookup remained `partial /
NoMatchingFactInScannedScope`; direct source confirms its `Player.Update` caller in both reviewed
trees. The `lastTimePlayerWasSaved` field-use query also remained partial with no indexed fact in
the selected Version4 paths. The complete reference body directly reads that field in the local,
dead, nonzero-save-time guard. The CPG snapshot has no source snapshot ID, and Version4's
`AdjustRespawnTimerForWorldJoining` body is empty, so that formula is reference-backed and proposed
for the target rather than confirmed Version4 behavior. In the reviewed Version4 `Spawn` sequence,
the no-op adjustment leaves the existing `dead` value unchanged before the caller derives its
death-preservation flag; the complete reference can clear it after offline countdown expiry.

The same read-only Query API pass checked the type 12 spawn boundary. `Find-CpgSymbols(Spawn)`
returned the `void(Terraria.PlayerSpawnContext)` method in `Terraria/Player.cs`; selected-path
`Find-CpgCallSites` returned four confirmed `CallTargets` (two in `MessageBuffer.cs`, two in
`Player.cs`). `SpawnX` and `SpawnY` field symbols were confirmed in the indexed `Player.cs`
shard. `Get-CpgMemberUses` returned five selected facts for each field: the two `MessageBuffer`
assignments are confirmed writes, the `NetMessage` reads are partial/unknown, and the
`Player.Spawn` references are partial with a data-flow candidate to `Spawn_SetPosition(int,int)`.
These facts confirm the wire-to-legacy-field shape only; they do not close slot/session binding,
coordinate validation, world position selection, or the effect closure of `Spawn`.
`Get-CpgCallableFacts(Spawn)` is `partial` with `CalleeEffectsNotExpanded`, so the selected
operation facts must not be read as a complete spawn side-effect graph.

Direct source comparison adds the protocol difference that the index cannot decide. The complete
reference `MessageBuffer.cs:898-939` uses the declared byte slot on clients and replaces it with
the authenticated `whoAmI` slot only when `Main.netMode == 2`. Version4
`MessageBuffer.cs:604-642` contains an unconditional `num144 = whoAmI` before the same field
writes. Both trees read signed 16-bit `SpawnX`/`SpawnY`, 32-bit `respawnTimer`, two signed
16-bit death counts, a byte team, and a byte `PlayerSpawnContext`, then call `Player.Spawn`.
The complete reference is `D:\TRbackup\无任何删减通过编译`; its `Player.cs:38031-38130`
and Version4 `Player.cs:21798-21866` both consume a pair of nonnegative coordinates before
team/world fallback, but the surrounding client and entry-world effects differ. The local type 12
API records the decoded fields only; authenticated-slot selection, coordinate validation, Version4
parity, and production reachability remain `unknown`.

The type 13 follow-up used the read-only Query API against the same manifest. `GetData` resolved to
`Terraria.MessageBuffer.GetData:void(int,int,int)` and returned one selected-path caller in
`Terraria/NetMessage.cs`; the selected call-site result is complete only for that scope. The
`sitting`, `petting`, and `sleeping` fields resolved to `Player.cs` members, and `isPetting` has a
confirmed write in `MessageBuffer.cs`. `isSitting` access direction stayed `Unknown` in the
selected member-use result, while the invocation of
`SetIsSleepingAndAdjustPlayerRotation(Player,bool)` was returned as `partial` with callee effects
unexpanded. These facts confirm the decoded rest-bit boundary but do not close packet decoding,
session binding, pose rotation, or the type 13 broadcast path.

Direct comparison of Version4 `Terraria/MessageBuffer.cs:652-738` with the complete reference
`Terraria/MessageBuffer.cs:949-1050` confirms four `BitsByte` groups, the rest bits at
`bitsByte18[2]`, `bitsByte18[4]`, `bitsByte18[5]`, and `bitsByte19[0]`, and the server-only
`whoAmI` remapping in the complete reference. The complete reference sleeping helper changes
rotation and clears its visual bed offset on a state transition; those pose and presentation
effects are outside P04. `IPlayerRestPacket13Adapter.Apply` and
`IPlayerPacket13RouteQuery.Evaluate` currently declare the API boundary only. They do not route
self echoes, resolve authenticated slots, commit rest state, or calculate a broadcast decision.
The rest System remains the proposed owner for activity state; packet integration has no production
`MessageBuffer` caller and no type 13 parity claim.

The slot bound was rechecked against the complete reference `Terraria/Main.cs:1088`, which declares
`maxPlayers = 255`; its `Player[]` allocation has length 256 for storage, but gameplay loops use the
255-player limit. This is reference evidence for a future slot policy only: the packet API
declarations do not reject slot `255` or establish client/server slot remapping. Session binding,
the accepted wire range, and packet reachability remain `unknown`.

`CanSpectate` resolves as a Version4 `Player.cs` method and its indexed call-site query returns one
selected call. Direct source inspection shows another call in `Player.Update`, so the indexed
caller list is not closed. The reviewed Version4 and complete-reference method bodies match:
negative or self slots return true; other candidates must be active; a dead candidate is allowed
only when its `whoAmI` is the observer's current target and `deadTime < 180`.

The same Query API recheck returned `partial` with `NoMatchingFactInScannedScope` for
`PetAnimal`, `PetMount`, `SitDown`, and `StartSleeping` in their expected Version4 source paths.
The entry decisions below use the complete-reference bodies only; their Version4 implementation
and runtime reachability remain `unknown`.

The complete reference implements `ClosestSpectatablePlayerTo` by scanning slots `0..254`,
excluding the observer, applying `CanSpectate`, and comparing each player's `Center` to the camera
point with squared distance. Its strict less-than comparison keeps the first slot on a tie, and
`SpectateNextClosestPlayer` passes the selected slot (including `-1`) to
`SetOrRequestSpectating`. Version4's `SpectateNextClosestPlayer` body is empty and the CPG symbol
query for `ClosestSpectatablePlayerTo` returned `partial / NoMatchingFactInScannedScope`; these
reference rules are therefore not confirmed Version4 behavior. The local pure selection API below
is proposed from the complete reference and has no production caller.

The complete reference's `SpectateNextPlayer` starts from the current target, or the observer slot
when there is no target, advances by the caller's `-1`/`+1` step with 255-slot wraparound, and stops
before revisiting its starting slot. It returns false without committing when offline or when no
eligible candidate exists; only a found slot is passed to `SetOrRequestSpectating`. The Version4
CPG symbol query returned `partial / NoMatchingFactInScannedScope`, and direct inspection found no
`SpectateNextPlayer` declaration in the selected Version4 `Player.cs`. The local
`FindNextSpectatablePlayer` extracts only candidate selection; offline gating and state/network
effects remain adapter responsibilities, and target parity is `unknown`.

Source anchors in the designated complete reference are `Terraria/Player.cs:17424`
(`CanSpectate`), `Terraria/Player.cs:17473` (`ClosestSpectatablePlayerTo`), and
`Terraria/Player.cs:17528` (`SpectateNextClosestPlayer`); center and squared-distance behavior are
at `Terraria/Entity.cs:50` and `Terraria/Entity.cs:206`. The Version4 empty fallback is at
`D:\TRbackup\Version4\Terraria\Player.cs:10100`.
The complete-reference next-target rule and its caller are at `Terraria/Player.cs:17494`,
`Terraria/Player.cs:17404`, and `Terraria/Player.cs:44217`.

`SetOrRequestSpectating` exposes a mode-boundary difference. Version4's indexed method and direct
source perform self normalization, same-target early return, target write, eligibility check,
fallback call, section check, and type 150 publication without a visible network-mode branch; its
fallback body is empty. The complete reference uses separate client and server branches: a local
client request may set the observer as a temporary target and sends type 150, while the server
commits the requested target before validation, requests fallback on rejection, and only checks
section/broadcasts for an accepted target. The CPG call-site query returned four selected sites
across `Player.cs` and `MessageBuffer.cs`; the type 150 handlers also directly write spectating
state. The new `RequestSpectatingTarget` models the complete-reference branch and returns effect
intents, but target parity and full writer closure remain `unknown`.

On 2026-10-01, `Find-CpgSymbols(SetOrRequestSpectating)` returned one confirmed symbol and
`Find-CpgCallSites` returned four confirmed `CallTargets` in the selected Player and
MessageBuffer shards. `Get-CpgCallableFacts` remained `partial` with
`CalleeEffectsNotExpanded`; it describes only the direct method body. The query for
`SpectateNextPlayer` remained `partial / NoMatchingFactInScannedScope`. The manifest has no
source snapshot ID, so the indexed facts do not establish that these results describe the current
Version4 source hashes.

Selected-path call-site queries returned six `StopVanityActions` sites in `Player.cs`, four
`StopPettingAnimal` sites across `Player.cs` and `Mount.cs`, and five `StopSleeping` sites across
`Player.cs` and `PlayerSleepingHelper.cs`. These indexed sets are bounded and do not close runtime
dispatch or callers outside the selected shards.

`UpdateDead` resolves in the Version4 `Player.cs` shard, but its selected-path call-site query is
`partial / NoMatchingFactInScannedScope`. Direct source inspection confirms one `Player.Update`
branch invokes it only when `dead` is true in both Version4 and the complete reference. The selected
member-use query returned three `deadTime` and eight `respawnTimer` uses. The latest read-only CPG
recheck resolved the `UpdateDead` symbol as `complete`, but its selected Player call-site query
remained `partial / NoMatchingFactInScannedScope`; callable facts remained `partial` with
`CalleeEffectsNotExpanded`. The manifest's `SourceSnapshotId` is null. Direct source inspection
therefore remains necessary: the complete reference increments `deadTime` once per call, decrements
the hardcore respawn timer only when its starting value is positive, and otherwise leaves a
nonpositive timer unchanged while requesting ghost state only for local/server authority. Its
normal branch clamps a one-tick decrement and requests local spawn at expiry. Version4's reviewed
`UpdateDead` body omits the normal local Spawn branch, does not have the same ghost authority guard,
and decrements the hardcore timer only while positive. Keep the expiry effect contract proposed from
the complete reference and do not infer it from the CPG zero-hit result.

The follow-up Query API resolved the Version4 `ghost` field and returned confirmed assignment-left
uses in `Player.cs` and `MessageBuffer.cs`, plus partial read uses in the selected scope. The query
does not close all writers or the runtime caller graph. NLTX already has one `PlayerGhostStateComponent`
owner, so `PlayerDeadTickAdapter` consumes only the reference-backed `ShouldBecomeGhost` intent and
uses that component's current `Ghost` value as the authoritative repeat guard. Respawn invocation,
inventory UI, spawn position and network publication remain outside this adapter.

The user-designated complete reference tree is `D:\TRbackup\无任何删减通过编译`. It has no Git
metadata. Its root includes `TerrariaServer.sln` and `TerrariaServer.csproj` targeting `net40`; its
`Main.cs` labels the source `v1.4.5.6`, matching Version4's version label. This is a complete source
reference for the checked paths, not proof of exact provenance or behavioral parity. Its full
solution was compiled during an earlier document pass with 85 warnings and 0 errors; that build did
not modify reference source. The following file hashes identify the read source:

| Reference file | SHA-256 |
|---|---|
| `Terraria/Player.cs` | `367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C` |
| `Terraria/Mount.cs` | `3F94D523F50A44498BB3E8FC7F98FA18AD11DE5EA1E65AFAD6E0D81F05040964` |
| `Terraria/Main.cs` | `E24E61C9903BB7995F47E36C51EDBE43481B643226861B717020877E7E22B63F` |
| `Terraria/MessageBuffer.cs` | `48AABBBF4E0967964D97598E0168E9252ADD3DD868C66C6DA46AA87670EACAAB` |
| `Terraria.GameContent/PlayerPettingInfo.cs` | `29F83EB75BD37B9F5CE961663E66E18BAAB0E3D141EE462F60F40179B07DD356` |
| `Terraria.GameContent/PlayerSittingHelper.cs` | `251804312E1580032A94D4CCF5EB142232FC7BC197283B88BF4E0AF8FD6347F6` |
| `Terraria.GameContent/PlayerSleepingHelper.cs` | `95B6D2ED60D73E73FCB2E15309ED67E219ADEF96BE9EB24C25AEAC8FBE721EFA` |
| `Terraria.IO/PlayerFileData.cs` | `2AF8A5326C30EEEC54CEB6D981561CDC2187518387A7A34BFA18068A2771F88E` |

The complete reference tree contains implementations for bed wake-up reasons, bed-style offsets,
team spawn routing, and player serialization that are stubs in the Version4 source cited by the
report. These are useful behavior references, not proof that the indexed Version4 source or
`src/NSSLC` has matching behavior. Its earlier full solution build through the repository serial
wrapper had 85 warnings and no errors; output was written under
`D:\TRbackup\NLTX\Build\bin\CompleteReference\`. The reference source was not modified.

The reviewed `D:\TRbackup\Version4` files had SHA-256
`E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86` for `Terraria/Player.cs`
and `0F474225FA9D98C539F61249619273A44E73F69A4CF388B44258433A2D15D5EE` for
`Terraria/MessageBuffer.cs`, and
`2DED2B174731DDDD003BCD1B03F83853D0AD39183CD3683B4C5289459AEDCEFC` for `Terraria/Mount.cs`.
The CPG manifest still has `SourceSnapshotId=null`, so these hashes do not bind the current files to
the indexed facts.

The read-only CPG API was initialized against that manifest and queried for selected member uses
and direct call sites. In the selected `Player.cs` and `MessageBuffer.cs` shards it returned 17
`active` uses (3 confirmed writes, 14 partial accesses), 21 `dead` uses (4 confirmed writes, 17
partial accesses), and 8 `respawnTimer` uses (4 confirmed writes, 4 partial accesses). Direct
call-site queries returned 3 `KillMe`, 4 `Spawn`, 11 `Teleport`, and 6 `StopVanityActions` sites.
These are bounded indexed facts, not a closed writer or caller inventory. The CPG source-excerpt
API returned `unknown` because the indexed source files are unavailable beside the database; the
selected methods and protocol branches were read directly from both source trees instead.

The 2026-10-01 read-only Query API follow-up resolved `Player.Teleport` as
`void(Vector2,int,int)` and `Player.KillMe` as
`void(PlayerDeathReason,double,int,bool)`. Selected `Find-CpgCallSites` results returned 11
`Teleport` sites and 3 `KillMe` sites across `Player.cs` and `MessageBuffer.cs`; these are indexed
selected-path results only. `PlayerDeathReason` resolved in its indexed declaration shard. The
index has no `SourceSnapshotId`, so the current Version4 and complete-reference sources were read
directly for the protocol branches and payload definitions.

## System Boundaries

| Boundary | Proposed responsibility | Explicit exclusions |
|---|---|---|
| `PlayerLifecycleSystem` | Coordinate activation, death resolution, spawn, and return through synchronous APIs. Maintain one writer for each lifecycle fact. | Combat drops, death penalties, inventory operations, network protocol, persistence I/O, and movement remain with their existing domain owners. |
| `PlayerDeadTickAdapter` | Commit a lifecycle ghost-expiry intent to `PlayerGhostStateComponent`, using that owner as the repeat guard. | No `UpdateDead` caller, respawn invocation, inventory UI, spawn position, or network effect is connected here. |
| `PlayerRestInteractionSystem` | Own the petting/sitting/sleeping activity flag set, sleep timer, and synchronous stop result. | Tile lookup, target validation, world scheduling, packet publication, and presentation effects stay with adapters or adjacent owners. |
| `IPlayerRestPacket13Adapter` | Declare the type 13 input-to-rest boundary. | The interface does not decode, validate, route, mutate rest state, or choose broadcast behavior. Packet/session ownership, movement, mount, camera, pose rotation, and publication remain outside this API-only slice. |
| `IPlayerTeleportPacket65Adapter` | Declare a handoff for decoded type 65 fields, including raw selector flags, wire slot, position, style, optional extra info, sender context, and network mode. | No entity classification, slot remapping, teleport commit, acknowledgement update, relay, or packet effect is implemented; Network and Teleportation/Movement ownership remains `integration-review`. |
| `IPlayerDeathPacket118Adapter` | Declare a handoff for decoded type 118 fields and all optional death-reason source fields. | No slot remapping, death resolution, reason conversion, kill effect, or death publication is implemented; Session/Network and Combat/DeathPenalty composition remains `integration-review`. |
| Teleportation/Movement composition | Accept a P04 transition request and coordinate rest cleanup with the established teleport/movement owner. | P04 does not claim position, portal/pylon, pressure-plate, or acknowledgement ownership. Final owner is `integration-review`. |
| Existing domain systems | Continue to own containers, TileEntity anchors, shops, doors, item timing, combat, wiring, projectiles, mounts, and presentation. | No catch-all `PlayerWorldInteractionSystem` or `PlayerRuntimeSystem`. |
| Deferred payload/projection | Keep `ItemCheckContext` invocation-local; keep rabbit order frames as a presentation candidate. | No persistent component is inferred from a local payload or presentation helper. |

System boundaries describe behavior responsibility; they do not imply exclusive reads of every
component. Each authoritative invariant still has one writer. Cross-owner atomicity needs an
explicit synchronous commit contract.

## Authority And State

`active` is written by spawn and the network active-state path in the complete reference.
`MessageBuffer` type 14 publishes connect/disconnect hooks only when the value changes. The
Version4 type 14 branch has an unconditional `break` before those writes, so its target execution
path is `unknown`. The complete reference tree keeps petting, sitting, and sleeping as independent
flags. `PetMount` sets petting without first calling `StopVanityActions`, while `PetAnimal`,
successful `SitDown`, and `StartSleeping` call that cleanup before activating their own state. A
single mutually exclusive enum would erase this distinction.

The NLTX rest slice uses `PlayerRestComponent.Activities` as one writer-owned flag set. The
petting, sitting, and sleeping components hold only their interaction-specific details; they do not
carry parallel active booleans. The flag set can represent concurrent states without claiming that
every combination is reachable through validated gameplay input. Source-specific entry adapters
must preserve which legacy path clears existing activities first.

The partial `PlayerLifecycleSystem` now owns local connection-state transitions through
`PlayerIdentityComponent.ConnectionState`, lifecycle phase and respawn timing through
`PlayerLifecycleComponent`, and committed death classification/count/location/time through
`PlayerDeathRecordComponent`. `IsActive`, `IsDead`, and `CanRespawn` are derived projections;
`PlayerIdentityState` and `PlayerLifecycleState` are immutable snapshots. The unused duplicate
`PlayerLifecycleStage` and `PlayerDeathRespawnComponent` candidates were removed after a source
search found no C# consumers. This establishes a proposed local write boundary only; no gameplay
adapter or external consumer inventory has been verified.

Coin loss fields remain on `PlayerDeathRecordComponent`, but `ResolveDeath` does not commit them.
Their value depends on later inventory/drop effects, whose integration order and failure behavior
remain outside this partial System slice.

Before full integration, review must resolve the remaining production owners:

- `PlayerLifecycleSystem` is the proposed local writer for connection transitions, lifecycle phase,
  respawn timing, spectating target, and death facts. Session identity, player slot, persistent
  character identity, and ECS entity identity remain separate values; their validated binding and
  the production owner remain `integration-review`.
- One rest owner commits the active-activity flag set. Petting, sitting, and sleeping detail
  components do not independently commit active state. Entry adapters choose the source-backed
  cleanup sequence; the owner does not impose global mutual exclusion.
- Save/network/presentation snapshots consume committed state and never write back to authority.
- Teleport position and pending network acknowledgements each remain with their designated
  Movement/Teleportation and Network owners.

Production ownership of the session/slot/entity binding and the runtime entry path remains
`integration-review`. The flag-set representation is a proposed local contract supported by the
complete reference tree; its compatibility with the indexed Version4 target and reachability of
concurrent combinations remain `unknown`.

## API Composition

The following rows describe synchronous API composition. The rest and lifecycle methods exist as
partial local System slices but are not connected to gameplay callers. Spawn has a partial local
state-commit API; teleport composition remains a design candidate.

| API candidate | Input and decision | Commit owner and result |
|---|---|---|
| `CanSpectate` | Requested player slot, observer slot/current target, and the resolved candidate's `whoAmI`, active/dead state, and elapsed-death ticks. | `PlayerLifecycleSystem` evaluates the sentinel/self, active, and selected-dead linger rules as a pure query. Slot lookup and invalid array-index behavior stay with the adapter. |
| `FindClosestSpectatablePlayer` | A candidate snapshot for each valid legacy slot, including `Player.Center`, plus observer/current-target slots and the camera point. | `PlayerLifecycleSystem` filters with `CanSpectate` and returns the slot with minimum squared distance; ties select the lower slot to preserve the reference's ascending scan, and no candidate returns `-1`. The adapter owns snapshot resolution and any subsequent `SetOrRequestSpectating` network/local effects. This is a complete-reference-derived proposal, not confirmed Version4 parity. |
| `FindNextSpectatablePlayer` | Candidate snapshots, observer slot, current target slot, direction `-1` or `+1`, and the `includeSelf` decision. | The pure query follows the legacy 255-slot cyclic order, starts from the current target or observer slot, and returns the first eligible slot. It returns `null` on an exhausted scan, so the adapter can preserve the old no-commit/false result. The online-mode guard and `SetOrRequestSpectating` call remain outside; Version4 target behavior is `unknown`. |
| `RequestSpectatingTarget` | Requested slot, observer slot, network mode, local-player identity, and resolved target eligibility facts for the server path. | `PlayerLifecycleSystem` normalizes self to `-1` and commits server target state before returning either a fallback intent or accepted broadcast/section-check intents. A local client only stages clear/self state and returns a request intent. Adapters execute packet, fallback, and section effects. This branch contract is complete-reference-derived; Version4 method and type 150 handler behavior differ, so target parity remains `unknown`. |
| `IPlayerSpectatingNetworkAdapter.Apply` | Decoded type 150 fields, authenticated sender identity, addressed slot, and resolved target facts. | Function declaration only. Packet validation, lifecycle mutation, and effect selection are not implemented; sender/session ownership, Version4 policy, and production caller remain `unknown`. |
| `SetConnectionState` | Validated session/slot input mapped to `Inactive`, `Active`, or `Disconnecting`, with source identified as spawn or network type 14. | `PlayerLifecycleSystem` commits `PlayerIdentityComponent.ConnectionState` once. It returns a connect/disconnect hook intent only for a type 14 active-value transition; spawn activation does not synthesize connection hooks. Network hook execution and binding remain adapter work. |
| `IPlayerConnectionNetworkAdapter.Apply` | Decoded type 14 active value, addressed slot, and network mode. | Function declaration only. `SetConnectionState` remains the lifecycle owner API, but packet validation, owner invocation, hook execution, and mode policy are not implemented; Version4 case 14 reachability and production wiring remain `unknown`. |
| `ResolveDeath` | Pre-evaluated creative/practice guard results, PvP request, position/time snapshot, resolved respawn ticks, rest context, and explicit network mode. | `PlayerLifecycleSystem` rejects creative/practice/already-dead inputs, synchronously stops rest, commits lifecycle phase and death facts, then applies the existing spectating-stop policy and returns its state/effect intent. Packet publication remains external; damage, drops, penalties, messaging, and persistence are not implemented here. |
| `CommitSpawn` | Spawn context, the saved-time binary value, a UTC `DateTime` snapshot, and local-player/broadcast context. The UTC kind is required only when the local dead-player adjustment is applied. | For a world join that is already dead, `PlayerLifecycleSystem` adjusts the timer only for the local player with a nonzero saved-time value, before synchronous rest cleanup. It subtracts the elapsed interval clamped to 0–1000 seconds and converted at 60 ticks/second, bounded by the current timer using the reference `Utils.Clamp` comparison order. It preserves death only while the adjusted timer is nonzero; otherwise it clears lifecycle death and the PvP-death marker, activates without connection hooks, and clears spectating. Before clearing that marker it returns `ShouldApplyPvpDeathRecovery` for the vital/immunity owner; P04 does not write those components and no consumer is connected. Spawn position/team routing, network and entry-world effects remain outside this slice. |
| `IPlayerSpawnNetworkAdapter.Apply` | Decoded type 12 fields and the lifecycle/rest components required by the proposed composition. | Function declaration only. Slot selection, field validation, component writes, and `CommitSpawn` invocation are not implemented; Version4 client/server slot policy and production reachability remain `unknown`. |
| `IPlayerVitalPacket16Adapter.Apply` | Decoded type 16 life fields, resolved player identity, the P06 `PlayerVitalStateComponent`, and the P04 `PlayerLifecycleComponent`. | Function declaration only. The interface declares the cross-owner input boundary; it does not validate/remap slots, write vitals, derive lifecycle phase, or publish packets. Version4 slot policy, target caller, and atomic visibility contract remain `unknown`. |
| `IPlayerRestPacket13Adapter.Apply` | Decoded rest projection, sender/session context, and network mode. | Function declaration only. `CanRelayToPeers` and `ShouldBroadcast` are data-contract fields; no route, rest mutation, relay gate, or packet publication is implemented. Version4 target relay policy remains `unknown`. |
| `IPlayerTeleportPacket65Adapter.Apply` | `PlayerTeleportPacket65Input` carries the signed wire entity slot, local/authenticated sender context, network mode, raw selector flags, position, style, and optional extra info. | Function declaration only. The flags are not interpreted and no player/NPC routing, teleport, acknowledgement, or relay behavior is supplied. Version4's unconditional sender-slot replacement differs from the complete reference's server-only replacement; target policy remains `unknown`. |
| `IPlayerDeathPacket118Adapter.Apply` | `PlayerDeathPacket118Input` carries the byte wire player slot, sender/mode context, damage, decoded hit direction, PvP flag, and `PlayerDeathReasonPacketInput` optional fields. | Function declaration only. It does not call `ResolveDeath`, retain or publish a kill reason, remap the player slot, or send death data. Version4's unconditional sender-slot replacement/send differs from the complete reference's server-only branches; target policy remains `unknown`. |
| `AdvanceDeadTick` | One non-ghost dead update plus hardcore/no-respawn classification, local/server authority, and cursor-item presence. | `PlayerLifecycleSystem` increments elapsed-death ticks. Normal respawn clamps a one-tick decrement; hardcore decrements only when the starting timer is positive and preserves nonpositive values while returning the authority-gated ghost intent. It returns a local-spawn intent at normal expiry; it does not mutate Ghost, inventory UI, or spawn position. This matches the complete-reference timer branch, while Version4 expiry effects remain `unknown`. |
| `PlayerDeadTickAdapter.Apply` | Lifecycle state, the ghost owner, and a dead-tick input snapshot. | Replaces the input ghost bit with the owner component's current value, delegates timer/intent calculation to `PlayerLifecycleSystem`, and commits only `ShouldBecomeGhost` to `PlayerGhostStateComponent`. Respawn, inventory UI, spawn position and network effects remain external; no production `UpdateDead` caller is connected. |
| `DecideEntry` / `PrepareEntry` | Explicit rest entry source, eligibility result, active-state snapshot, and resolved-position comparison. | `DecideEntry` is a pure query and rejects ineligible input for every source, including MountPet. `PrepareEntry` applies the selected current-activity stop or full rest cleanup synchronously and returns stop-packet intents. The adapter publishes those intents immediately before later movement/entry effects. This is reference-backed policy only; the Version4 API query had no matching symbol fact for the four entry methods. |
| `CommitEntry` / `BeginPetting` | A prepared decision, caller-owned petting target details, completed target/movement effects, and validated seat/bed commit snapshots. | Petting commits only the activity bit; target identity remains adapter-owned. Sitting/sleeping commit their anchor, facing, features/offset, stack index, and activity in the Rest System. Position, grapple/dismount effects, and achievement/presentation work stay with the adapter. |
| `BeginSitting` / `BeginSleeping` / `StopAll` | Validated seat/bed detail snapshot, tick and network-context inputs. | The source paths for sitting, sleeping, and petting an animal synchronously stop existing vanity actions first. `StopAll` clears petting, sitting, then sleeping and returns separate packet intents for the network adapter. |
| `AdvanceSleepTimer` | Current rest activity and an explicit `hasReasonToActUp` input derived from the sleeping helper's world/item checks. | While sleeping, it increments once and resets to zero when the reason input is true without ending sleep; while inactive, it keeps the timer at zero. The complete reference implements the wake-reason checks, but Version4's helper body is a stub returning false, so target policy remains `unknown`. |
| `RequestPlayerTeleport` | Player identity, destination, style, extra info, and acknowledgement context. | Teleportation/Movement integration commits position and transition state once; P04 only requires synchronous rest cleanup before the legacy teleport sequence. |

### Query API Recheck

The fresh read-only CPG Query API pass returned four selected `SetOrRequestSpectating` call sites,
four selected `Spawn` call sites, and one selected `GetData` call site. `GetData`'s result covers
only the selected `MessageBuffer.cs` shard; the API's selected paths do not establish a complete
packet caller graph. The earlier pass returned two selected
`SetIsSleepingAndAdjustPlayerRotation` call sites.
The `isSleeping` member-use query returned 13 facts (one confirmed assignment write and 12
`partial`/`Unknown` accesses). `UpdateDead` still had no matching selected `Player.cs` call-site
fact, while its callable facts remained `partial / CalleeEffectsNotExpanded`. These results are
bounded to the selected indexed paths; the manifest has no source snapshot ID.

The latest read-only query resolved `pvpDeath` as a complete field symbol and returned four member
uses in the selected `Player.cs` and `MessageBuffer.cs` paths: two confirmed writes and two
`Unknown` access directions. The result does not close all writers or bind the index to current
source hashes. Direct inspection of the complete reference confirms that a non-preserved Spawn
clears this marker after its PvP recovery branch. `CommitSpawn` therefore returns the recovery intent
before clearing it, while application to life/immunity state remains an external owner handoff.

The follow-up query resolved `PlayerSleepingHelper.UpdateState`,
`DoesPlayerHaveReasonToActUpInBed`, and `PlayerSittingHelper.UpdateSitting` as complete symbols;
their callable facts remain `partial / CalleeEffectsNotExpanded`. The selected call-site query found
one `DoesPlayerHaveReasonToActUpInBed` caller in `PlayerSleepingHelper.cs`, while `UpdateState` and
`UpdateSitting` call-site searches returned `partial / NoMatchingFactInScannedScope`. Direct source
inspection confirms both Version4 and the complete reference call the wake-reason helper from
`UpdateState`; the Version4 helper returns false, while the complete reference checks NPC danger,
blood moon/eclipses, and item animation. The new timer API accepts that result as an explicit input;
the target policy and broader tick reachability remain `unknown`.

### Type 13 Relay Evidence And API Boundary

The complete reference type 13 handler relays an applied packet only when `Main.netMode == 2`
and `Netplay.Clients[whoAmI].State == 10`, using the effective player slot in
`TrySendData(13, ...)`. The reviewed Version4 handler instead assigns `num210 = whoAmI` without
a mode guard and has no matching relay block in the inspected case body. This is a direct source
difference, not proof of which behavior the migration target should adopt.

`CanRelayToPeers` and `ShouldBroadcast` remain input/output data-contract fields. The adapter and
route query are interface declarations only; they do not select the sender, apply rest state,
evaluate join state, or publish packets. The reference relay condition and Version4's inspected
absence are source facts, while the target relay policy, session owner, and production caller remain
`unknown`. Packet behavior is intentionally outside the 5-check core verifier.

### Type 65 Teleport And Type 118 Death Packet Declarations

The Query API resolved the Version4 `Teleport` and `KillMe` signatures and returned 11 and 3
selected call sites respectively in the chosen `Player.cs` and `MessageBuffer.cs` shards. Direct
source comparison shows type 65 reads selector flags, a signed 16-bit entity slot, a vector
position, a byte style, and optional 32-bit extra info. Its flag-derived branches include player
teleport, NPC teleport, server-directed player teleport, and acknowledgement decrement. The complete
reference replaces the declared slot with `whoAmI` only on the server, whereas Version4 replaces
it unconditionally. The two trees also differ in network-mode guards around relay,
acknowledgement, section-check, and nearby-player chat effects.

Type 118 reads a byte player slot, a `PlayerDeathReason` payload, signed 16-bit damage, a byte
direction adjusted by minus one, and the first bit of a `BitsByte` PvP flag. `PlayerDeathReason`
contains eight optional fields: player, NPC, and projectile-local indexes; other-source index;
projectile type; item type; item prefix; and custom reason text. The new
`PlayerDeathReasonPacketInput` preserves those optional values instead of dropping death
attribution. Both handlers replace the wire slot with `whoAmI` unconditionally in Version4; the
complete reference does so only in server mode and only publishes the death packet on the server.

`IPlayerTeleportPacket65Adapter.Apply` and `IPlayerDeathPacket118Adapter.Apply` are abstract
declarations only. Their inputs are data contracts; no decoding, validation, slot binding, route,
teleport/death commit, acknowledgement, relay, or packet publication is present. Packet reachability,
session ownership, target policy, and the applicable Version4/complete-reference branch remain
`unknown`. These declarations receive compile coverage only; the focused core verifier does not
exercise packet behavior.

Query candidates (`PlayerLifecycleSnapshotQuery`, `PlayerDeathRecordQuery`,
`PlayerRestStateQuery`, and `PlayerSpawnEligibilityQuery`) must be deterministic over explicit
snapshots. Tile/entity resolution that touches registries, lazy tile state, time, cache, or output
parameters stays behind an adapter until proven pure. No queued command object, general message
bus, or delayed rest-stop queue is justified by the observed synchronous calls.

## Observed Ordering To Preserve

The complete reference tree shows `Main.DoUpdateInWorld` clearing sitting/sleeping stack anchors,
skipping inactive player slots, and then calling `Player.Update`. Inside `Player.Update`, rest
capabilities update in this order: petting, sitting, sleeping. Afterward Main counts an active,
non-ghost player as fully asleep using the sleeping helper. An ECS schedule must preserve these
observation points; file or registration order is not scheduling evidence.

The reference `Player.StopVanityActions` calls petting stop, sitting stop, then sleeping stop.
Sitting and sleeping clear their own indexes, offsets/details, and conditionally send packet 13
only for the local player when broadcast is enabled. Repeated stops do not re-send those packets
because those helpers act only when their capability is active. Petting stop clears its active
flag. Spawn, death, and teleport call this cleanup synchronously before proceeding. The NLTX rest
System preserves this order and returns sitting/sleeping packet intents; sending packet 13 remains a
network adapter responsibility.

Other source-backed behavior that constrains later parity work:

- In the complete reference, `PetAnimal` invokes `StopVanityActions` before activating petting,
  while `PetMount` sets petting without doing that cleanup; a repeated mount-pet input stops
  petting only. `Mount.TryPettingMount` gates the call on pause/menu state, horizontal speed,
  pointer hit, right-click state, and projectile interaction blocking. `PetMount` itself does not
  repeat those caller checks; `EntryInput.IsEligible` carries the caller's validation result and
  `DecideEntry` rejects it when false. `PetAnimal` and successful sitting and
  sleeping starts first validate their target and same-position toggle, then synchronously call
  `StopVanityActions` before later movement/effect steps. `DecideEntry` captures these source
  decisions from explicit adapter inputs. `PrepareEntry` applies only the selected activity stop
  or full rest cleanup; `CommitEntry` writes the new activity bit after caller-owned movement and
  target effects. CPG lookup for `PetAnimal`, `PetMount`, `SitDown`, and `StartSleeping` in the
  expected Version4 paths returned `partial` with `NoMatchingFactInScannedScope`; target
  entry/precondition behavior and semantic revision match remain `unknown`. Keep a flag set capable
  of representing overlap; do not claim every combination is reachable or globally exclusive.
- Death has creative/practice/already-dead early returns, stops rest before the death writes, then
  records PvP/PvE counts, position/time, coin/drop and notification effects before its local-player
  save branch. The partial `ResolveDeath` API preserves the guard order, synchronous rest cleanup,
  and initial state commit from explicit inputs. The `PracticeModeReset` call result is supplied by
  an outer boundary because its effect closure has not been mapped; drop/coin results, network
  publication, and local save remain unimplemented.
- Dead updates increment elapsed-death ticks once per `Player.Update` dead branch in both reviewed
  source trees. The complete reference clamps the normal respawn countdown each tick, but in its
  hardcore branch decrements only when the starting timer is positive and leaves an expired
  nonpositive timer unchanged. `AdvanceDeadTick` now preserves that branch distinction and returns
  the reference expiry intent for synchronous consumption. Version4 omits the local normal-respawn
  call and does not have the same hardcore ghost authority guard; target expiry behavior remains
  `unknown`.
- Spawn resets selected life, immunity, position, velocity, fall, and spectating state in a
  context-dependent order. For world joining, the complete reference adjusts the respawn timer
  first and preserves `dead` only when the player remains dead afterward. It then synchronously
  calls `StopVanityActions`; the non-preserved branch clears death elapsed time and the PvP-death
  marker, while `active=true` and `spectating=-1` are committed in either branch. NLTX now computes
  this selected state transition in `PlayerLifecycleSystem.CommitSpawn` from explicit time values
  and returns the pre-clear PvP recovery intent for the external vital/immunity owner. No consumer is
  connected, and those components remain outside P04.
  Version4's adjustment method body is empty and leaves the preexisting `dead` value for the outer
  Spawn branch; the reference-derived offline expiry is proposed, not confirmed target parity.
  NLTX still has no gameplay caller or time adapter. Team-spawn selection, position,
  health, immunity, network, and entry-world effects remain unimplemented. The complete reference
  has team-spawn selection logic; Version4 does not provide that implementation in the reviewed
  body, so route parity remains open.
- Teleport preserves style-specific grapple/shimmer/portal behavior, runs entry effects before
  position commit, updates pressure-plate state around movement, and runs exit effects before
  setting visual transition fields. The selected CPG query returned 11 `Teleport` call sites in
  `Player.cs` and `MessageBuffer.cs`; this is not a runtime caller closure. The complete reference
  adds local-player camera/biome refresh effects after position commit that are absent from the
  reviewed Version4 method. CPG symbol queries for those two helpers returned
  `partial / NoMatchingFactInScannedScope`; direct inspection confirms only that the calls are not
  in the reviewed Version4 method body, not that no other target path can invoke them. Treat them as
  reference-version differences, not as confirmed target behavior. The empty catch block is present
  in both reviewed bodies; error and partial-commit behavior still needs compatibility review.
- The complete reference has player `Serialize`/`Deserialize` bodies and a player release marker.
  Field-by-field P04 persistence mapping and recovery semantics have not been extracted here and
  remain `unknown`. `WorldFile` is not a substitute for player persistence.
- Sleeping reaches the fully-asleep state at 120 ticks. Wake-up checks in the complete reference
  include danger, blood-moon/eclipses, item animation, input, mount, bed eligibility, and stack
  capacity. Tile/target snapshots and effect order still need an adapter contract.

## Cross-System Handoff

`MessageBuffer` is an input adapter, not a second lifecycle writer. In the complete reference,
packet type 12 writes spawn, respawn, death-count, and team inputs before invoking spawn; the local
`IPlayerSpawnNetworkAdapter` declares that boundary only; it does not delegate a state commit to
the lifecycle owner. A concrete implementation must be connected to a real decoder only after
slot/session ownership and the Version4 client/server remapping are resolved. Type 13
writes player input/rest-related state; type 14 changes active and invokes connect/disconnect hooks
only on a transition; type 16 writes life and derives dead state. The Version4 source differs in
the slot-remapping and active-path guards for cases 12/14/16, including the case 14 early `break`;
the selected CPG facts confirm field-use locations but not runtime reachability. A future adapter
must translate authenticated slot/session input into the single owner API before legacy direct
writes can be removed. `IPlayerVitalPacket16Adapter.Apply` now declares the packet boundary across
the existing vital and lifecycle owners; it has no implementation or target policy.

Types 65 and 118 now have declaration-only adapter boundaries. Type 65 delegates position and
transition commits to Teleportation/Movement, while network acknowledgement ownership remains with
Network. Type 118 carries the full death-reason payload into a future Session/Network to
Combat/DeathPenalty composition; `PlayerLifecycleSystem.ResolveDeath` remains a separate lifecycle
owner API and is not wired to this packet adapter. Neither adapter has a production `MessageBuffer`
caller. Slot/session binding and the choice between the differing source branch guards remain
`unknown`.

The selected CPG member-use queries returned 25 `statLife`, 9 `statLifeMax`, and 21 `dead` facts in
`Player.cs` and `MessageBuffer.cs`. Each field has two assignment-left write facts in the selected
`MessageBuffer.cs` shard. Direct source inspection identifies the type 16 writes and the
`statLifeMax` minimum clamp; other member uses and the complete writer/caller graph remain
`partial`/`unknown`.

Type 150 is also a lifecycle input. Both reviewed trees route nonnegative server requests through
`SetOrRequestSpectating`, while the negative-target branch writes `spectating = -1` and broadcasts
directly. The complete reference additionally projects received server updates into non-local
client players (and into the local player when it already has a spectating target). These
packet-specific stop/receive paths remain behind the declaration-only
`IPlayerSpectatingNetworkAdapter.Apply`; sender/session validation, target resolution, lifecycle
mutation, packet publication, and fallback/section effects remain unimplemented or with the
Network owner. Version4 compatibility remains `unknown`.

P04 must obtain integration decisions from Session/Network, Combat/DeathPenalty, Items, Movement/
Teleportation, WorldInteraction, Commerce, P09 containers, and P11 Presentation. Readers and
writers outside the report's selected source paths remain `partial` or `unknown`; no cross-domain
owner is reassigned here.

## Blocking Unknowns

- Which System owns the slot/session/persistent-ID-to-entity binding and the production `active`
  commit, and whether Version4 case 14's early `break` is intentional source behavior.
- Whether Version4 case 12/16 server slot remapping differs intentionally from the complete
  reference, and the target caller/reachability conditions for these writes. The type 12 API
  declaration has no slot or coordinate policy; those decisions remain `unknown`.
- Whether Version4's mode-free `SetOrRequestSpectating` behavior or the complete reference's
  client/server split is the required target contract; type 150 contains direct writes in both
  trees. The new request/stop/receive APIs have no production packet adapter caller, so sender
  validation, write ownership, and target behavior remain `unknown`.
- Whether the complete reference tree is the same semantic revision as `D:\TRbackup\Version4`;
  the trees have no shared source snapshot identity in the CPG manifest.
- P04 member-by-member player persistence mapping, format compatibility, save failure recovery,
  and restore identity binding.
- Dynamic callers, event/hook entry points, multi-world/session scope, scheduler phases, and all
  direct/indirect writers beyond the selected CPG paths.
- Petting activation caller/preconditions and whether every `PetMount` path excludes sitting or
  sleeping remain `unknown`; do not assert full rest-mode exclusivity until this is closed.
- Runtime behavior of the current NLTX tree: local rest and lifecycle Systems have no gameplay
  caller or schedule; `PlayerDeadTickAdapter` is not connected to `Player.Update`. The partial death System does not execute death effects or
  coin-record updates. `CommitSpawn` covers only selected lifecycle fields; the type 12 adapter has
  no real `MessageBuffer` caller, timestamp/session adapter, team-spawn route, movement reset, or
  entry-world effects.

The CPG query API result and complete reference code narrow these gaps but do not close them. The
local rest and lifecycle state Systems are not connected to gameplay entry points, world scheduling,
or packet publication. The reference Space Station 14
`Content.Shared/Hands/EntitySystems/SharedHandsSystem.cs` was read as a structure example: its
system owns dependency/event wiring while collaborator systems execute effects. It provides no
Terraria behavior evidence and does not justify adding a MessageBuffer dependency here.

## Acceptance Boundary

This design remains `proposed`. It can advance only after the authority collisions are resolved,
the new APIs are connected to real callers, and focused behavior tests cover the initial core
slice. No code in `src/NSSLC` is declared migrated by this document.
