# System Decomposition Report: authoritative P11

partitionId: P11
taskId: AUTH-SYS-P11
sessionId: d80bd1ce886a4dea90fefa2bc74f46b7
inputReport: D:/TRbackup/NLTX/docs/migration/ledgers/authoritative-20-partitions/P11-Player-Presentation-Derived.md
prompt: D:/TRbackup/NLTX/docs/component-decomposition/review-round-1/design/2026-09-11-version4-authoritative-20-partition-prompts/2026-09-11-version4-P11-player-presentation-derived-public-decomposition.md
outputReport: D:/TRbackup/NLTX/docs/system-decomposition/reports/2026-09-18-system-decomposition-authoritative-P11-player-presentation-derived.md
designStatus: proposed
verificationStatus: not-run
sourceModified: false

## Scope and Evidence

This report covers only the 18 claimed P11 leaf groups under PlayerGameplay: 91 fields and 73 properties (164 members). The authoritative input inventory is the member-scope authority; this report does not decide owners for other partitions. The listed source declaration locations originate in that inventory. Version4 source was re-opened for behavior slices and representative writers, readers, network paths and lifecycle calls.

Evidence identities:

| Evidence | Identity and use | Status / limit |
|---|---|---|
| Version4 source | D:/TRbackup/Version4; Player.cs SHA256 E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86; Terraria.GameContent/PlayerEyeHelper.cs SHA256 0353B2AEF705FB3065674D95282132F2B4FDE331EB78E71096F4D5ABE9154E80 | Confirmed for the inspected snapshot; no repository revision identifier was established |
| Version4 CPG SQLite | D:/TRbackup/Version4-cpg-export/out-dop8-interproc.sqlite; manifest 6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364; project fingerprint 521CBD4C11E7EB731256C232350A54099F431D0E59EBDF3C38880281F169C11B; 967 shards | Read-only CpgEvidence.ps1 Query API used for symbol resolution, member uses and call sites. SourceSnapshotId is null, so the index cannot be asserted to match the current source hashes |
| CPG partition query | 164 members resolved to declaring types; 987 member-use facts: 268 Write, 20 ReadWrite, 698 AccessMode Unknown; 156 query results complete, 8 partial. netOffset yielded 14 uses (6 Write, 1 ReadWrite, 7 Unknown) in the partition-wide query | Complete means the selected index query completed, not that aliases, accessors, dynamic callers or runtime effects are closed |
| Targeted CPG calls | ResetNetOffsets -> Main, UpdateCameraPan -> Main, NewMessage -> Terraria.Chat/ChatHelper.cs; each returned a confirmed static call-site result for the requested shard. PlayerEyeHelper.Update call-site query returned partial with no rows | Source calls were manually rechecked. A zero or partial call-site result is not proof of no caller |
| Current NLTX | P11-shaped sources exist under D:/TRbackup/NLTX/src/NSSLC/Component/Player, including eye, camera, appearance, query and presentation systems/projections | Static search found focused-verifier references, but no production registration/call path within the searched scope. Reflection, external integration and callers outside that scope remain unknown |
| Prior P11 documents | docs/component-decomposition/review-round-2/2026-09-11-version4-p11-player-presentation-derived-component-design.md and the paired component-execution.md | These describe isolated C01-C18 seams and partial integration evidence. Their historical verifier/build claims were not rerun or promoted here |
| tModLoader public API | D:/TRbackup/tmodloader-api-docs-stable/index.html (v2026.07), class_player.html (Player: MountedCenter, ZoneDungeon and Male are get/set), struct_player_eye_helper.html (EyeFrameToShow get-only; Update(Player)) | Public surface cross-check only; does not establish Version4 private ownership or exact semantics |
| SS14 structural reference | C:/Users/shan/Downloads/ECS/space-station-14-master/Content.Server/Wires/WiresComponent.cs:6-66 and WiresSystem.cs:29-75, 300-315, 455-475, 600-620 | Supports only the organizational distinction between component state, event/update handling and output projection; no Terraria behavior was inferred |

The CPG source-excerpt path could not be bound to this checkout because the index has no source snapshot identity. CPG facts are structural leads, not source truth. Key semantics below were checked against Version4 source. Static searches do not close reflection, dynamic dispatch, configuration, serializer callbacks, inbound extension hooks, exception paths, unload behavior, multi-world scheduling or runtime order.

## Prior Component Decomposition Reconciliation

The prior P11 component design records 18 isolated component/query/projection seams and implementationStatus: completed, while keeping evidenceStatus: partial and verificationStatus: partial. It explicitly leaves legacy ownership, scheduler ordering, network/save formats, renderer/chat integration, world-zone authority and cross-domain composition open. Its execution report contains historical focused-verifier results for subsets and pending items; this System session did not execute or independently validate them.

The current tree places the inspected sources under src/NSSLC/Component/Player. Prior documentation also referred to src/Player. This path difference is recorded as version/path drift; it is not resolved by assuming one tree is the canonical runtime. Existing C01-C18 names are candidates, not proof of registered runtime systems or unique writers. Query and projection seams remain useful only when inputs are immutable snapshots and outputs cannot write authority. In particular, prior query names need correction where the old property setter mutates state, returns a live mutable reference, or depends on mutable external objects.

## Conceptual Behaviors

| ConceptId | Observable behavior and invariant | State kind / proposed responsibility | Evidence |
|---|---|---|---|
| P11.PoseFrame | Derive and expose head/body/leg pose, frame offsets and rotation used by simulation-adjacent presentation; preserve update order and origin/velocity relationships | Mixed pose inputs and frame presentation state. Keep one coherent frame snapshot; no P11-wide writer until pose producers and scheduler are closed | Player update and draw paths; writer closure partial |
| P11.NetworkCamera | Smooth network position correction; calculate camera target, compare with last synchronized target and encode/decode the network value | Stateful correction/cache plus network projection. A query alone is insufficient; network adapter owns packet effects | Player.UpdateNetOffset, Main.UpdateCameraPan, NetMessage and MessageBuffer |
| P11.VisualProjection | Produce shadows, composite-arm pose, equipment colors, companions, shader and football draw inputs from committed state | Mostly transient cache/projection; do not persist by default or let render output write gameplay state | Player shadow methods, arm setters and visual consumers; complete consumer closure unknown |
| P11.Appearance | Accept appearance selection/color changes and provide values to network/save/render boundaries | Player customization state with external protocol adapters. Persistence and canonical writer unresolved | Player fields and packet paths; save serialization stubs in this snapshot |
| P11.DerivedPlayerView | Answer geometry, identity, zone, interaction, ability, item and mount reads from explicit snapshots | Query composition where pure. Mutating setters and live references become Commands or explicit adapters | Player property bodies; several inputs and owners cross partition |
| P11.EyeAnimation | Advance eye state/timer, select frame, respond to hurt/blind/sleep/status/environment inputs, reset at lifecycle boundaries | Stateful deterministic animation transition; proposed separate EyeAnimationSystem, with hurt/lifecycle events supplied by other owners | PlayerEyeHelper.Update and Player update/hurt calls |
| P11.OverheadMessage | Replace a transient message, parse/measure its text and color, decrement display time and expose a draw snapshot | Short-lived message state, state-update API and one-way render projection; chat/text integration remains external | OverheadMessage.NewMessage, ChatHelper caller and Player time decrement |

## State Ownership and Write Closure

The table enumerates the complete claimed member set by leaf group. “Partial” is deliberate where property setters, mutable references, multiple producers or external lifecycle paths prevent a unique owner conclusion. Proposed roles are not current runtime registrations.

| Leaf group | Complete claimed members | Proposed state/API role | Decision and evidence |
|---|---|---|---|
| PlayerPoseAndAnimationState (14 fields; Player.cs:1099-1126) | headRotation, bodyRotation, legRotation, headPosition, bodyPosition, legPosition, headVelocity, bodyVelocity, legVelocity, fullRotation, fullRotationOrigin, fartKartCloudDelay, gfxOffY, stepSpeed | Frame pose snapshot; explicit frame commit from gameplay/animation owner | partial; input and writer ordering cross owner |
| PlayerNetworkCameraState (3 fields; Player.cs:1128-1132) | netOffset, netCameraTarget, lastSyncedNetCameraTarget | Correction state and camera/network adapter snapshot | partial; simulation correction, camera policy and packet ownership differ |
| PlayerShadowAndArmPresentation (13 fields; Player.cs:1296-1342) | cursorItemIconReversed, runSoundDelay, shadowPos, shadowRotation, shadowOrigin, shadowDirection, shadowCount, skipAnimatingValuesInPlayerFrame, availableAdvancedShadowsCount, _advancedShadows, _lastAddedAvancedShadow, compositeFrontArm, compositeBackArm | Transient shadow cache; separate per-arm value; output projection | partial; UI/audio fields and all renderer consumers need closure |
| PlayerVisualAndShaderEffects (21 fields; Player.cs:1433-1473) | dontStarveShader, noirShader, eyebrellaCloud, yoraiz0rEye, yoraiz0rDarkness, hasUnicornHorn, hasAngelHalo, hasRainbowCursor, leinforsHair, musicBoxSilence, stardustMonolithShader, nebulaMonolithShader, vortexMonolithShader, solarMonolithShader, moonLordMonolithShader, bloodMoonMonolithShader, shimmerMonolithShader, CRTMonolithShader, retroMonolithShader, musicBox, overrideFishingBobber | Effect/projection inputs; catalogue and effect producers remain outside this decision | partial; mixed reset, equipment and rendering paths |
| PlayerFootballPresentationState (2 fields; Player.cs:2008-2010) | hasFootball, drawingFootball | Equipment-derived draw state and one-way projection | partial; equipment owner and draw lifecycle unresolved |
| PlayerAppearanceCustomizationState (10 fields; Player.cs:1947-1966) | hairDye, skinDyePacked, hairColor, skinColor, eyeColor, shirtColor, underShirtColor, pantsColor, shoeColor, hair | Customization state, explicit apply command, network/save adapters | partial; packet use is visible, but canonical writer and save format are unresolved |
| PlayerTraversalColorProjection (6 fields; Player.cs:2332-2348) | cWings, cCarpet, cFloatingTube, cGrapple, cMount, cMinecart | Read-only equipment/traversal color projection | partial; source capability, dye and registry closure unknown |
| PlayerAppearanceCompanionAndEffectProjection (11 fields; Player.cs:2350-2370) | cPet, cLight, cYorai, cPortableStool, cUnicornHorn, cAngelHalo, cBeard, cMinion, cLeinShampoo, cFlameWaker, cCoat | One-way companion/effect projection | partial; effect producers and client consumers unresolved |
| PlayerSpatialDerivedProperties (9 properties; Player.cs:2503-2581) | BlehOldPositionFixer, HeightOffsetHitboxCenter, HeightOffsetBoost, HitboxForBestiaryNearbyCheck, IsConsideredStandingStill, BaseHeight, MountedCenter, VisualPosition, CCed | Snapshot query for derivations; MountedCenter setter must map to a position command | partial; movement, mount, hitbox and status snapshot contracts unresolved |
| PlayerIdentityAndDerivedProperties (2 properties; Player.cs:2593-2613) | miscCounterNormalized, Male | Pure normalized counter query; Male setter maps to an appearance/identity command | partial; Male mutates skinVariant, so property is not a query-only contract |
| PlayerBiomeZoneProperties (16 properties; Player.cs:2617-2805) | ZoneDungeon, ZoneCorrupt, ZoneHallow, ZoneMeteor, ZoneJungle, ZoneSnow, ZoneCrimson, ZoneWaterCandle, ZonePeaceCandle, ZoneTowerSolar, ZoneTowerVortex, ZoneTowerNebula, ZoneTowerStardust, ZoneDesert, ZoneGlowshroom, ZoneUndergroundDesert | Read-only zone snapshot query plus explicit zone-bit commit API | partial; getters read zone1/zone2, setters write those packed values |
| PlayerVerticalAndWeatherZoneProperties (6 properties; Player.cs:2809-2879) | ZoneSkyHeight, ZoneOverworldHeight, ZoneUnderworldHeight, ZoneBeach, ZoneRain, ZoneSandstorm | Read-only snapshot query plus zone commit API | partial; getters/setters use selected zone3 slots, with a non-contiguous slot for underworld |
| PlayerEventAndShoppingZoneProperties (7 properties; Player.cs:2882-2955) | ZoneOldOneArmy, ZoneLihzhardTemple, ZoneGraveyard, ZoneShadowCandle, ZoneShimmer, ShoppingZone_AnyBiome, ShoppingZone_BelowSurface | Event-zone read model and pure shopping derivation over explicit input | partial; event-zone setters write packed zone4/zone5 state and shimmer has cross-domain consumers |
| PlayerInteractionAndSelectionProperties (9 properties; Player.cs:2958-3038) | Directions, selectedItem, HeldItem, ShouldFloatInWater, CanBeTalkedTo, IsVoidVaultEnabled, ReportedCameraPosition, TryingToHoverUp, TryingToHoverDown | Read query over immutable item/interaction/camera facts; selected-index and vault writes are Commands | partial; HeldItem returns the mutable inventory Item and vault setter writes a bit |
| PlayerAbilityAndPresentationProperties (12 properties; Player.cs:3042-3150) | UsingBiomeTorches, UsingSuperCart, bowEffectiveDamage, gunEffectiveDamage, specialistEffectiveDamage, CanUseBootFlyingAbilities, CanUseWingAbilities, ShouldNotDraw, talkNPC, isLockedToATile, PortalPhysicsEnabled, MountFishronSpecial | Composed queries over committed combat, mount, rest, scene and draw facts; setters translated to owner commands | partial; the two Using* setters write state and remaining source owners are distributed |
| PlayerItemMountAndRuntimeProperties (11 properties; Player.cs declaration rows in input inventory) | HasMinionRestTarget, ItemTimeIsZero, ItemAnimationJustStarted, UsingOrReusingItem, SceneMetrics, SpectatingCameraPosition, SlimeDontHyperJump, hasBreathingReed, IsRidingTracks, MouthPosition, HandPosition | Read-only query for copied values; adapters for world scene, item, mount and camera | partial; SceneMetrics and mutable object/reference semantics are not closed |
| PlayerEyeAnimationState (4 members; Terraria.GameContent/PlayerEyeHelper.cs) | _state, _timeInState, TimeToActDamaged, EyeFrameToShow | EyeAnimationComponent plus EyeAnimationSystem; frame is an output, not an independently writable authority | separate for the transition only; lifecycle and input event wiring remain partial |
| PlayerPresentationMessagesAndArms (8 members; Player.cs:455-488 and Player.CompositeArmData) | enabled, stretch, rotation, chatText, snippets, messageSize, timeLeft, color | Keep CompositeArmData values per arm; model overhead message as its own transient state with enqueue/tick/project APIs | partial; arm action producers, rich-text renderer and ChatHelper integration remain open |

Partition-wide write evidence does not establish a unique owner per group. Some Zone properties directly mutate packed zone slots. Male writes skinVariant; MountedCenter can write position; IsVoidVaultEnabled writes a bit; UsingBiomeTorches and UsingSuperCart have setters with state writes; HeldItem exposes a mutable object. These must not be wrapped in apparently pure Queries. Arrays, mutable struct slots, nested item references and external SceneMetrics values require copying or an explicitly bounded adapter.

## Boundary Role and Decision

| Behavior slice | Decision | Proposed boundary | Rejected alternative |
|---|---|---|---|
| Eye state transition | separate | PlayerEyeAnimationSystem owns only the eye state/timer transition and frame result; explicit hurt/reset inputs arrive from lifecycle/combat owners | Keeping timer mutation inside a broad Player update owner obscures its independent state machine; moving hurt authority into animation would steal ownership |
| Per-frame pose and animation | partial | Keep a coherent pose snapshot and explicit commit seam; existing movement/action producers remain authoritative until their write sets and schedule are mapped | One presentation System owning pose, movement velocity and stepSpeed would combine separate invariants |
| Camera correction and synchronization | partial | Separate camera/network translation at its protocol adapter; expose immutable camera snapshots and explicit synchronization acknowledgement | Treating netOffset and both camera fields as passive projection would hide stateful correction and packet effects |
| Shadows, arms and equipment visuals | partial | Keep short-lived caches separate from arm pose and equipment-owned inputs; render consumes a one-way snapshot | One System per flag or one catch-all render System adds nodes without independent ownership evidence |
| Appearance customization | partial | Keep one cohesive customization value; mutations arrive as explicit commands; save/network boundaries translate external formats | Persisting every visual/effect flag or letting render projections update customization |
| Derived reads and zone flags | partial | Pure Queries consume frozen snapshots; setters become explicit commit Commands owned by environment/capability owner after integration review | Declaring the whole property surface pure because the old API uses getters |
| Overhead messages | partial | Separate transient message state from arm state; chat adapter constructs typed text input; projection emits immutable draw data | Combining chat parsing, timers, arm kinematics and rendering in one owner |

The report does not add scheduler nodes for partial boundaries. A new System boundary is justified only by a distinct invariant, write set, lifecycle or effect contract. The 18 inventory groups are not 18 System nodes.

## System API and Legacy Behavior Mapping

The signatures below are conceptual proposals, not statements that current production callers use them. Resulting APIs must preserve return values, state deltas, emitted packets/events, order, visibility, scope, error behavior and idempotency.

| Legacy behavior / entry | Proposed API composition | State and output | Status |
|---|---|---|---|
| Player update -> PlayerEyeHelper.Update(Player); damage -> BlinkBecausePlayerGotHurt(); spawn/reset paths | PlayerEyeAnimationSystem.Update(component, immutable input), OnHurt(component, hurt event), ResetForSpawn/ResetForRemoval(component) | Unique eye state/timer writer; immutable EyeFrame snapshot | Source transition confirmed; caller closure, event ordering and lifecycle wiring partial |
| Player.UpdateNetOffset(fallThrough, ignorePlats); Player.ResetNetOffsets() | NetworkCorrection.Update(component, collision/motion snapshot); ResetAllOffsets(world scope); projection reads corrected offset | netOffset state delta, reset scope and correction output | Method/source confirmed; collision scope, reset admission and all callers partial |
| Main.UpdateCameraPan; Player.ReportedCameraPosition; NetMessage/MessageBuffer packet path | CameraTargetQuery over tracked-camera input; CameraStateSystem.SetDesiredTarget; NetworkAdapter.SendWhenChanged and acknowledge synchronized value; decoder emits update command | target snapshot, packet and last-synchronized cache | Static Main and protocol paths confirmed; authority, packet loss/retry and client/server rules partial |
| Player.SetCompositeArmFront/Back; item/use action call sites | SetCompositeArm command -> per-arm state owner -> immutable draw snapshot | arm enabled/stretch/rotation and gravity-direction normalization | Direct methods and representative callers confirmed; full producer list and commit order partial |
| Player.UpdateSocialShadow/UpdateAdvancedShadows and draw consumers | AdvanceShadowCache(frame snapshot); ProjectShadowFrame(snapshot) | bounded transient arrays/cache, draw-only result | Update methods confirmed; complete draw read closure, reset and allocation policy partial |
| Appearance fields, Male getter/setter, NetMessage/MessageBuffer appearance serialization | ApplyAppearance(command) -> AppearanceState; AppearanceSnapshotQuery; network/save adapters encode/decode external formats | canonical customization delta and external value projection | Male write and network evidence confirmed; persistence in this Version4 snapshot unknown |
| Zone getters/setters and MessageBuffer/NetMessage packed zone values | PlayerZoneSnapshotQuery; SetZoneFlags(command) committed by environment/zone owner; protocol adapters serialize packed bits | immutable zone snapshot or explicit packed-zone delta | getters/setters and protocol path confirmed; unique environmental writer and freshness barrier partial |
| Spatial/interaction/ability/item/mount derived properties | Typed queries over explicit immutable snapshots; commands for former setters; adapters copy mutable item/scene inputs | result snapshot, no writeback from Query | Per-property semantics mixed; query input ownership and alias boundaries partial |
| OverheadMessage.NewMessage(message, displayTime), ChatHelper call, Player chatOverhead.timeLeft decrement | ChatAdapter.CreateOverheadMessageInput -> OverheadMessageStateSystem.Enqueue/AdvanceTick -> OverheadMessageProjection.Project | one transient message, remaining ticks, measured size/color, draw snapshot | NewMessage caller and countdown confirmed; snippet parsing, message replacement/expiry semantics and renderer closure partial |

Adapter rules: Query consumes copies or immutable interfaces; it cannot mutate domain state. Projection is one-way and does not decide gameplay or equipment ownership. Network/save/chat/renderer integration is explicit at the boundary. Commands validate scope and are committed by one owner; this partition does not assign a shared owner.

## Call and Dependency DAG

Solid source/API-confirmed edges are written as arrows. Partial/unknown edges are intentionally not presented as a closed DAG.

- Main update phase -> Main.UpdateCameraPan -> LocalPlayer.netCameraTarget -> NetMessage packet encoding -> MessageBuffer decode -> netCameraTarget: confirmed for the inspected static path. NetMessage also updates lastSyncedNetCameraTarget after sending when its target-difference condition holds. Packet delivery, retry and server/client authority are partial.
- Main.BlackFadeCameraTeleport -> Player.ResetNetOffsets -> player-array netOffset clearing: confirmed by source re-read. Other callers and concurrency with player updates are partial.
- Player update -> Player.UpdateNetOffset and Player.UpdateAdvancedShadows: confirmed call sites in Player.cs; every path and phase barrier unknown.
- Player update -> sitting/sleeping updates -> eyeHelper.Update(this): source order confirmed at Player.cs:15728; static CPG call-site query for PlayerEyeHelper.Update was partial with zero rows, so inbound closure is partial.
- Player hurt handling -> eyeHelper.BlinkBecausePlayerGotHurt: source-confirmed at Player.cs:22349. Whether damage events can invoke it outside this path is unknown.
- Player.ResetEffects and spawn/reset -> numerous effect/presentation fields: reset entry points confirmed; per-field reset completeness is partial.
- Player.UpdateSocialShadow -> shadowPos/shadowCount; Player.UpdateAdvancedShadows -> advanced shadow cache; arm methods -> CompositeArmData: writers confirmed by source methods, render consumers and external producer closure partial.
- ChatHelper -> OverheadMessage.NewMessage; Player update decrements chatOverhead.timeLeft: static CPG call site confirmed and source lines Player.cs:14966-14968 confirm tick mutation. Render consumers and thread/scope semantics unknown.
- NetMessage writes zone1..zone5 and MessageBuffer restores them at lines 932-936 and 1722-1727 respectively: confirmed for packed network fields. The source that calculates every zone bit and its phase relative to spatial/environment updates is unknown.
- Current NLTX component/query/System declarations -> focused verifier source references: confirmed as files/search results. Runtime registration, legacy entry routing and live consumer edges are unknown.

No topological order is inferred from file layout or component names. Candidate dependencies requiring scheduler review are committed movement/world facts before zone/spatial query snapshots; gameplay hurt before eye update when same-tick visibility is required; arm/action commit before draw projection; message enqueue before timer advancement/render read; and camera target selection before network encode. Exact phase and tick convention are unknown until the full owner scheduler is inspected.

## Lifecycle and Side Effects

| Phase / effect | Version4 evidence | Proposed contract and open edge |
|---|---|---|
| Create / initialize | Player fields have inline defaults; eye helper and cache instances are per Player | Initialize deterministic empty/default state. Exact construction and pooling paths outside inspected code are unknown |
| Activate / spawn | Player.Spawn resets several pose, offset and shadow values; eye helper reset wiring was not completely located | Reset all transient state through explicit lifecycle command once; repeated reset should be idempotent. Eye reset callers need closure |
| Update | Player update advances eye state, overhead-message lifetime, network correction and shadow data in distinct portions of Player.cs | Preserve tick count and ordering; whether update is client-only/server-only or parallel is unknown |
| End / destroy / removal | Prior isolated components expose reset-for-removal seams; actual Version4 removal/disconnect hooks were not closed | Clear player-scoped transient caches, pending messages and synchronization state. Exact removal route and stale entity handling are unknown |
| Rebuild / reset effects | ResetEffects runs at Player update entry and elsewhere; appearance/effect flags have reset behavior | Separate per-tick effect reset from durable customization and projection cache reset; field-by-field reset map partial |
| Network | Camera packet path and packed zone/appearance members are visible in NetMessage/MessageBuffer | Preserve packet layout, conditions, authority and acknowledgement state. Retry/duplicate/order behavior not proved |
| Persistence / clone | Player.Serialize at Player.cs:26418 is an empty body in this snapshot; Deserialize at :26455 is a stub; visual clone invokes them | Save/load format and round-trip behavior unknown. Do not persist caches or claim appearance save compatibility |
| Exception / retry | No complete retry/rollback contract located for setters, packets, chat parse or rendering | Error behavior, packet retry and partially committed operations unknown |
| Multi-world / session | Main.player/global local-player references and static camera state cross the Player seam | World/session scoping and simultaneous-world behavior unknown; integration-review required |
| External effects | NetMessage writes packets; ChatHelper parses/forwards text; render/UI consumers read projections; some state is associated with audio/UI | Keep effects behind adapters; exact effect multiplicity and ordering beyond inspected paths partial |

## Integration Handoff

Every unresolved cross-partition owner is assigned crossSubsystemOwner: integration-review. This report does not make final owner decisions for:

- pose inputs shared with movement, gravity, action/animation and player lifecycle;
- camera correction, packet authority, acknowledgement and session replication;
- appearance identity, save/network formats, equipment dyes and content registries;
- zone-bit production, SceneMetrics freshness, weather, event and shimmer authority;
- ability, item selection, mutable inventory references, mount/rest facts and interaction policy;
- hurt event ordering and lifecycle reset wiring for eye animation;
- arm pose producers from item/action behavior, renderer consumption and visual caches;
- chat transport, rich-text/snippet parsing, message lifetime scope and renderer output;
- Entity/player references, snapshots, public compatibility and scheduler barriers.

The integration review should obtain inbound and outbound call closure, including reflection/extension hooks, serializers, network dispatch, render registration, lifecycle removal and phase scheduling. Until those edges are resolved, only the local eye transition is a candidate independent System; it is not approved as a production owner.

## Migration Behavior Contract

Proposed migration is incremental and compatibility-first:

1. Freeze relevant Version4/current-NLTX source identities and produce per-concept read/write/call-site inventories, including dynamic and lifecycle entry points.
2. Define immutable inputs and observation vectors before moving writers. Keep existing facade signatures as adapters only after their setter/reference semantics are explicit.
3. Move the eye state machine first as a bounded candidate, wiring hurt, update, spawn and removal events through one writer. Do not change tick timing or frame selection.
4. Migrate derived reads into snapshot Queries; split mutating property setters into Commands and replace mutable reference returns with bounded adapters or stable value snapshots.
5. Migrate camera, zone and appearance wire formats only with packet/save round-trip and authority contracts. Preserve packed bit indexes and send/change conditions.
6. Move transient shadow, arm and overhead-message state behind owner commands and one-way projections. Keep chat parsing, audio/UI and renderer effects at adapters.
7. Remove a legacy path only after required behavior tests call the new owner and composition and compare the full observation vector.

For every slice, compare:

Observation = (return/error, authoritative state delta, emitted events and external effects, order and visibility, lifecycle and scope, retry and idempotency).

The legacy entry-to-composition mappings above are proposals; call compatibility, wire compatibility and semantic compatibility remain separate acceptance criteria. No old facade removal, shadow write, parallel writer or rollback switch is authorized or implied by this report.

## Evidence Gaps and Blocking Decisions

| Gap / decision | Status | Blocks |
|---|---|---|
| CPG manifest has SourceSnapshotId null; exact index-to-source alignment | partial | Treating index counts as current-source proof |
| Full reader/writer and inbound closure for all 164 members, including dynamic/reflection/configuration/accessor aliases | unknown | Final unique owner and compatibility mapping |
| Zone production, setter call sites and scheduler phase relative to environment/scene metrics | partial / unknown | Zone writer and freshness barrier |
| Appearance persistence format; current Version4 Serialize/Deserialize bodies are stubs | unknown | Save/load adapter and persistence claim |
| Packet authority, retry, delivery, server/client ownership and old/new decode interop | partial | Network migration |
| Shadow/draw consumers and cache invalidation/reset | partial | Projection lifecycle and no-render mode |
| Composite arm producers and gravity/direction ordering | partial | Arm owner and item-action composition |
| Overhead text/snippet parser, replacement, timer tick convention and renderer consumer set | partial | Chat adapter and message lifetime contract |
| Query snapshots for mutable Item, SceneMetrics, mount, inventory and identity values | unknown | Pure-query contract and alias safety |
| Current NLTX production system registration and actual route from legacy Player APIs | unknown | Any migration-complete or runtime-owner claim |
| Lifecycle removal, disconnect, unload, pooled Player reuse, multiple worlds and exception paths | unknown | Cleanup and session-scope contract |

Blocking decision: do not promote any cross-owner slice to a final System owner or claim a closed dependency DAG until the listed integration evidence exists. designStatus remains proposed.

## Verification Plan

No build, test, runtime, focused behavior verifier or migration verifier was run in this System session. The read-only CPG Query API and source inspection are evidence gathering, not behavior verification. verificationStatus: not-run.

Before implementation acceptance, prepare focused tests (not executed here) for:

- Eye state transitions at timer boundaries, hurt hold, blind/sleep/status inputs, spawn/removal reset, same-tick event ordering and deterministic frame output.
- Pose and network correction observation vectors, reset scope, duplicate packet, packet loss/retry and camera target synchronization condition.
- Camera/zone/appearance serialization round trips against Version4 packet/save formats, packed bit compatibility and authority.
- Pure Query determinism/no writeback, setter Commands, alias safety for HeldItem/SceneMetrics, and zone stale-snapshot rejection.
- Shadow/arm projection after update/reset, gravity/direction conventions, empty renderer and lifecycle cleanup.
- Overhead message parse/replacement/expiry tick boundaries, rich-text measurement and draw visibility; side effects and replacement multiplicity.
- Full production integration proving calls enter the new owner/composition, unique writes, scheduler barriers, exception handling, disconnect/removal, multi-world scope and legacy compatibility.

A local verifier or successful compile alone cannot establish migration success. Migration success remains unclaimed; the runner will settle only this report task.

