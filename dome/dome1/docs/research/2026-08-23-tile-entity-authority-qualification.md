# TileEntity authority qualification

## Decision

The first candidate reviewed against a complete source/runtime oracle is
`TETrainingDummy`. Its bounded Simulation authority slice is implemented, but
the complete candidate remains `Partial/deferred`; no generic opaque
TileEntity replacement is accepted.

## Complete oracle

| Artifact | Evidence |
|---|---|
| Complete source | `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Tile_Entities\TETrainingDummy.cs` |
| SHA-256 | `88088FFA7B24A1F6D998E471C20C662A98C642EEDE04B53B720CC4964B287F21` |
| Lines | 190 |
| Tile registration | type `378`; valid tile requires active tile, type `378`, `frameY == 0`, and `frameX % 36 == 0` |
| Persistent/runtime state | `npc` link, initial `-1`, `RequiresUpdates = true`; network payload writes/reads `Int16 npc` |
| Tick behavior | scans active player hitboxes, activates within an inflated `1600`-pixel range, deactivates when linked NPC is inactive, wrong type, or wrong AI coordinates |
| Side effects | creates NPC type `488`, sets AI coordinates to tile position, toggles NPC active state, emits legacy tile-entity update message `86` |
| Placement/removal | `Hook_AfterPlacement` and `NetPlaceEntityAttempt` use tile square message `87`; `TileEntity.Kill` removes by position/type |

## Current ECS comparison

| Required contract | Current state | Status |
|---|---|---|
| immutable tile-validity input | `TileEntityTrainingDummyValidityQuery` covers active/type `378`/`frameY == 0`/`frameX % 36 == 0`, with focused rejection cases | partial evidence |
| authoritative entity state | `TrainingDummyTileEntityState` parses non-opaque entity type `0` and the persisted little-endian `Int16 npc` link; runtime ownership is held by the Simulation owner | partial evidence |
| update system | Simulation tick scans active player hitboxes and evaluates valid TrainingDummy entities; automatic invalid-link cleanup is wired, but dedicated deactivation loopback remains open | partial evidence |
| typed NPC command/commit | `NpcSpawnSource.TileEntity` queues and commits NPC type `488`, then records the typed ownership link | partial evidence |
| persistence | typed parser, runtime projection, and fail-closed restart reconstruction are verified; link repair and full persistence/network parity remain open | partial evidence |
| protocol projection | compatibility serializer can represent legacy entity shapes, but server does not own a typed TrainingDummy update route | deferred |
| focused verifier | focused tick activation, ownership, restart, and projection checks pass; placement, deactivation loopback, and replication remain open | partial evidence |

## Incremental Evidence: Training Dummy Tile Validity

The source-backed tile-validity predicate is now isolated as
`TileEntityTrainingDummyValidityQuery.IsValid(WorldTile)`. It accepts only an
active tile of type `378`, with `frameY == 0` and `frameX % 36 == 0`. The focused
WorldObjects verifier covers one valid case and inactive, wrong-type,
misaligned-`frameX`, and nonzero-`frameY` rejection cases.

Evidence:

- Oracle SHA-256:
  `88088FFA7B24A1F6D998E471C20C662A98C642EEDE04B53B720CC4964B287F21`
- Implementation:
  `src/Terraria.Dome.Simulation/WorldObjects/Definitions/TileEntityTrainingDummyValidityQuery.cs`
- Verifier:
  `Test/Terraria.Dome.WorldObjects.Verification/Program.cs`
- Run artifact:
  `Build/diagnostics/server-ecs-convergence/P5-tileentity/20260822-training-dummy-validity/trace.md`

These slices close only the immutable tile-validity and typed-payload parsing
inputs. The activation/deactivation command path, NPC-488 ownership,
persistence continuation, protocol projection, and placement/removal loopback
rows remain `deferred`; the overall Training Dummy qualification and capability
family G therefore remain `Partial`.

The follow-up typed-state slice adds `TrainingDummyTileEntityState.TryRead`.
It accepts exactly the complete oracle's Training Dummy entity type (`0`) and
two-byte little-endian `Int16 npc` payload, while rejecting opaque and malformed
records. WorldObjects and Persistence verifiers pass, and the Simulation build
is warning-free. This parser is deliberately not an activation system and does
not validate that the referenced NPC is active, type `488`, or positioned at
the entity; those runtime contracts remain deferred.

The existing `TileEntityDefinitionRegistry` is an ID/name definition boundary only. It is not an
implementation of `TETrainingDummy`, and its presence does not satisfy this qualification.

## Incremental Evidence: NPC Link Validity

`TrainingDummyNpcLinkValidityQuery` now captures the oracle's link predicate
without owning the NPC lifecycle. Its explicit input contains `IsActive`, NPC
type, and the two AI tile coordinates. A link is valid only when the persisted
`NpcId` is non-negative, the NPC is active, its type is `488`, and both AI
coordinates equal the Training Dummy tile position. Focused cases cover
inactive, wrong-type, wrong-X, wrong-Y, and unlinked (`NpcId == -1`) states.

This is validation-only. It does not create or deactivate NPCs, change
`NpcLifecycleComponent`, emit protocol frames, or repair stale links.

## Incremental Evidence: Activation Eligibility

`TrainingDummyActivationEligibilityQuery` now models the complete oracle's
player-range check with an explicit player hitbox input. It uses the entity
rectangle `(tileX * 16, tileY * 16, 32, 48)`, inflates it by `1600` pixels,
ignores inactive players, and performs rectangle intersection. Focused cases
cover an active nearby player, inactive player, and X/Y positions outside the
inflated bounds.

This remains an eligibility predicate only. It does not scan the authoritative
player store, enforce the oracle's `npcSlotsFull` cache, create NPC type `488`,
assign AI coordinates, or emit update message `86`.

## Incremental Evidence: Activation Decision

`TrainingDummyActivationDecisionQuery` composes the source-backed guards in
their observable order: an existing non-negative `NpcId` prevents reactivation;
`npcSlotsFull` prevents a new attempt; active player hitboxes are then checked
with the inflated-rectangle query; and the no-player case is rejected. The
focused verifier covers nearby activation, slot-full suppression, and already
linked suppression.

The result is a decision value only. The current `SpawnNpcCommand` identifies
an NPC by `DefinitionId`, while the complete Training Dummy oracle identifies
the target by NPC net type `488`; no mapping between those identities is
proven. Consequently no NPC spawn command or commit is emitted by this slice.

`TrainingDummyDeactivationDecisionQuery` covers the complementary source rule:
a non-negative persisted link requests deactivation exactly when the linked
NPC fails the validated active/type/AI-coordinate predicate; an unlinked entity
does not request deactivation. This is still decision-only and does not mutate
lifecycle state, clear `NpcId`, or emit message `86`.

## Incremental Evidence: NPC 488 Definition Boundary

The complete NPC oracle's `SetDefaults(488)` branch supplies a source-backed
identity and subset of definition fields: net type `488`, width `18`, height
`40`, defense `0`, life max `1000`, and stationary/special behavior. The
Simulation now has a dedicated `NpcBehaviorId.TrainingDummy` stationary branch,
and the NPC verifier proves the `DefinitionId=488`/`NetId=488` field contract
and zero-velocity behavior.

This does not yet register the definition in `DomeSimulation` or claim full
parity. The oracle also requires `aiStyle=92`, `immortal=true`,
`netAlways=true`, AI coordinate assignment, and a tile-entity-owned lifecycle;
the current `NpcDefinition`/commit path does not represent all of those fields.

The authority model now carries `AiStyle`, `IsImmortal`, and `AlwaysReplicate`
through `NpcAuthorityComponent` during spawn and restore. NPC lifecycle
consumption honors `IsImmortal` by preventing death and timeout transitions;
damage resolution remains active, matching the distinction between Terraria's
immortal target and an immune target. `NpcSpawnSource.TileEntity` is available
for a future typed activation command. The definition is still not registered
in the default `DomeSimulation` catalog because AI coordinate binding and
tile-entity ownership are not complete.

`TrainingDummyOwnershipState` now provides a typed, revisioned transition
boundary independent of the opaque persistence dictionary. `TryLink` requires
an unlinked entity, a valid `NpcHandle`, and the source-backed NPC link
predicate; it creates revision `1`. `TryClear` requires matching entity
identity/position and an invalid linked NPC, then clears ownership and advances
the revision. Invalid handles and mismatched links are rejected. This is not
yet wired into `DomeSimulation._tileEntities` or the Arch NPC store, so it does
not claim runtime ownership or protocol publication.

The transition is now guarded by the Simulation owner through
`DomeSimulation.TryLinkTrainingDummyNpc` and
`DomeSimulation.TryClearTrainingDummyNpc`. The owner verifies the persisted
entity record, active replication identity, and NPC net type `488` before
delegating to the typed transition; duplicate links are rejected, and clear
revisions are retained in the runtime ownership store. A snapshot-backed
focused fixture proves link revision `1`, duplicate rejection, and clear
revision `2`. The generic payload is not rewritten and V1456 message `86` is
not emitted by this boundary.

## Incremental Evidence: Runtime Snapshot Projection

`DomeSimulation.CreateTileEntitySnapshots()` now projects a linked
TrainingDummy ownership state through `TrainingDummyPersistenceProjection`.
The focused owner fixture observes the source-compatible two-byte little-endian
payload `[9, 0]` after linking NPC handle `9`, and `[255, 255]` after the
invalid link is cleared. This proves runtime snapshot output, while restore
reconstruction, automatic tick activation/deactivation, tile removal, and
V1456 message `86` publication remain deferred.

## Incremental Evidence: Restart Ownership Reconstruction

The Simulation constructor now attempts to reconstruct typed TrainingDummy
ownership from a persisted non-negative `npc` payload. Reconstruction is
fail-closed: it requires an existing active NPC replication with type `488` and
an exact position match to the entity tile coordinates. The focused fixture
round-trips a linked dummy, observes payload `[9, 0]`, and successfully performs
the restored typed clear transition at revision `2`. A missing, inactive,
wrong-type, or coordinate-mismatched NPC leaves ownership unlinked rather than
inventing a relationship. Automatic activation/deactivation, NPC creation,
cleanup, tile removal, and V1456 message `86` remain deferred.

## Incremental Evidence: Tick Activation and NPC Commit

`DomeSimulation` now evaluates valid TrainingDummy entities during the
authoritative tick after player movement preparation and before the NPC spawn
commit pipeline. It converts active player entities into typed hitboxes, applies
the source-backed activation decision, and queues a `SpawnNpcCommand` with
`NpcSpawnSource.TileEntity` and definition/net type `488`. The spawn commit
creates the NPC and immediately records the typed ownership link. TrainingDummy
NPC movement is stationary, so the generic gravity/collision path does not move
the target away from its tile coordinates.

The focused tick fixture proves one active nearby player causes NPC `488` to be
created at `(12, 14)` and the persisted ownership payload to become `[1, 0]`.
The full NPC, Persistence, and WorldObjects Loopback verifiers also pass. The
despawn commit now clears the matching tile-entity ownership in the same
deterministic commit; the fixture also proves NPC inactive plus payload
`[255, 255]` after a queued type-488 despawn. The complete server path still
lacks tile placement/removal commands, observer publication, and message `86`.

## Incremental Evidence: V1456 TileEntity Projection

The V1456 adapter now exposes the source wire boundaries for the supported
TrainingDummy state. `TileEntitySharing` (`86`) encodes the entity id, present
flag, type `0`, id, tile coordinates, and the two-byte little-endian NPC
payload. The removal form encodes the id and a false present flag. The
`TileEntityPlacement` (`87`) form encodes the signed tile coordinates and type
`0`. A per-session cursor projects only changed type-0 entities in visible
sections and emits removal frames for previously sent entities that disappear.

The focused verifier validates all message fields and the Protocol/Server
Release builds pass, including a cursor fixture for initial, unchanged,
changed, and removed states. Initial section bootstrap, placement
authorization and client-side handling remain outside this bounded projection;
the server-owned observer loopback is covered separately, and the complete
TrainingDummy qualification is still `Partial`.

## Incremental Evidence: Initial Section Ownership

The initial world stream now carries supported type-0 TrainingDummy entities in
the V1456 `TileSection` payload for their section. The session cursor marks
those entities as sent while constructing the initial stream, so the following
incremental replication pass does not duplicate them. Entities outside the
initial sections remain eligible for the normal visible-section delta path.

Requested sections use the same section filter and cursor marking, so a client
requesting a previously unseen section receives its typed TrainingDummy state
inside the `TileSection` frame without a duplicate follow-up sharing frame.

Protocol, Server, WorldObjects, and FullClientBootstrap verifiers pass. This
does not add client placement authorization or prove a real client can mutate
TrainingDummy state; those protocol ownership gates remain deferred.

## Incremental Evidence: Tile Interaction Placement/Removal Owner

The Simulation owner now exposes typed `TryPlaceTrainingDummy` and
`TryRemoveTrainingDummy` operations. Placement requires a valid world tile,
type `378`, frame `(0, 0)`, and no existing entity at the coordinates; it
creates type `0` state with payload `Int16 -1`. Removal is position-based,
idempotently guarded, and queues cleanup for an owned NPC. The server's existing
validated `TileManipulation` path invokes these operations only after the world
tile commit, so a client cannot create an entity before the tile is authoritative.

The focused WorldObjects verifier proves duplicate placement rejection, typed
initial state, idempotent removal, and the Server build passes. Direct client
`87` placement remains rejected/deferred; placement is accepted only through
the server-owned tile interaction path.

## Acceptance boundary

This candidate may become accepted only after a typed snapshot/component, a source-backed tile
validity query, a deterministic activation/deactivation command path, NPC ownership and cleanup,
restart-preserving persistence semantics, V1456 projection, and focused plus loopback evidence all
exist. Until then G remains `Partial` and no weighted capability score is changed.

## Current Execution Evidence: 2026-08-22 22:30

The latest bounded chain is implemented and rerun under
`Build/diagnostics/server-ecs-convergence/P-final/20260822-2230/`:

- Simulation and Server Release builds pass with zero warnings and errors.
- WorldObjects, Persistence, Protocol Compatibility, FullClientBootstrap,
  MainBoundary, Completion, and WorldObjects Loopback verifiers all exit `0`.
- The typed owner is reached through the validated server tile interaction path and a bounded
  inbound message `87` route. The inbound route is limited to active-session, visible-section,
  type-0, tile-validity and duplicate guards; complete client authorization remains deferred.

The qualification therefore remains `Partial`. The server-owned observer
loopback is still a bounded path; source-backed authorization semantics and
client mutation behavior for inbound `87` are still required before family G can
receive score or be treated as complete.

## Incremental Evidence: Server-Owned TCP Loopback

The focused `WorldObjects.Loopback.Verification` now covers the implemented
authority path end to end. A visible TCP session sends the validated
`TileManipulation` placement for tile type `378`; the server commits the tile,
creates the typed type-0 entity, and publishes one message `86` present frame
with the expected coordinates and `npc=-1`. A hidden session in another section
receives no frame. The visible session then sends the validated kill operation;
the server publishes the matching message `86` removal frame and clears both the
authoritative tile and persistence snapshot.

The complete oracle does establish the basic wire direction for client message
`87`: `TETrainingDummy.Hook_AfterPlacement` sends it from a client and the
server-side `MessageBuffer` decodes it before calling `TileEntity.PlaceEntityNet`.
That evidence is recorded at
`Build/diagnostics/server-ecs-convergence/P5-tileentity/20260822-training-dummy-message87-direction.json`.
The current ECS server now accepts only the bounded route described above. The source supplies
bounds/duplicate guards but no interaction-distance predicate; complete range semantics,
explicit rejection feedback, and all tile-entity types remain outside this contract.

The protocol layer now has a typed `TileEntityPlacementIntent` decoder with
exact five-byte payload validation. The focused verifier covers valid
coordinates/type `0` and malformed-length rejection; the fresh run is recorded
at `Build/diagnostics/server-ecs-convergence/P5-tileentity/20260822-training-dummy-inbound-decode/trace.txt`.
The inbound boundary is now wired through the dispatcher, session host,
`DomeNetworkUpdateBridge`, and a typed `PlaceTileEntityCommand`. The server
requires an active player slot, entity type `0`, in-world coordinates, and a
visible section before invoking the Simulation owner. The direct Release build
is recorded at
`Build/diagnostics/server-ecs-convergence/P5-tileentity/20260822-training-dummy-inbound-route/server-build.txt`.
This closes parsing, bounded enqueue/guard behavior, and the visible-session TCP loopback.
It does not prove source-backed interaction range, explicit rejection feedback, or complete
tile-entity-family ownership, so it does not promote the full inbound mutation contract.
The loopback therefore strengthens the bounded evidence but does not promote
TrainingDummy or family G from `Partial`.

The raw TCP loopback now exercises the bounded inbound route. A visible session
places a prepared valid type-378 tile through message 87 and receives one typed
message-86 state frame; a duplicate message 87 produces no second entity; and
an active wrong-type tile is rejected. A foreign hidden session cannot place a
visible-section entity, and the linked state survives disconnect of the placing
client. Fresh output is recorded at
`Build/diagnostics/server-ecs-convergence/P5-tileentity/20260822-training-dummy-inbound-route/loopback.txt`.
The oracle's message-87 receive branch has no interaction-distance predicate;
range enforcement is therefore not source-backed for this packet. The same
loopback reconnects a new TCP session after the placing client closes and
observes the persisted TrainingDummy projection. The complete oracle has no
explicit rejection frame for message 87: failed bounds/duplicate checks return
silently. That fact and the current silent-drop route are recorded at
`Build/diagnostics/server-ecs-convergence/P5-tileentity/20260822-training-dummy-message87-rejection-contract.json`.
No synthetic rejection frame is required by this contract.
