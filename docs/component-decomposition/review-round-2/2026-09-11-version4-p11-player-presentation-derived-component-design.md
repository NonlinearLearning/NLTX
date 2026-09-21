# P11 Player Presentation and Derived Properties: Component Design

partitionId: P11
sessionId: 37c9b79cb0bf42489131c63004475fa9
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P11-Player-Presentation-Derived.md
designPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-p11-player-presentation-derived-component-design.md
evidenceStatus: partial
implementationStatus: completed
verificationStatus: partial
designCompletedComponents: [C01, C02, C03, C04, C05, C06, C07, C08, C09, C10, C11, C12, C13, C14, C15, C16, C17, C18]
completedComponents: [C17, C01, C02, C03, C04, C05, C06, C07, C08, C09, C10, C11, C12, C13, C14, C15, C16, C18]
currentComponent: none
pendingComponents: []
lastCheckpointUtc: 2026-09-12T06:57:14Z
evidence-gap: C01-C18 have serial focused isolated evidence for their declared verifier scopes. The P11 authority report and design table are count-complete at 91 fields + 73 properties = 164 unique rows, but P11 types are absent from docs/migration/ledgers/Version4-component-target-index.json, so canonical effective-component statistics do not yet register these isolated sources. The affected Terraria.Player project build is blocked by five pre-existing non-P11 Progression dependency errors, while all five existing P11 focused verifier projects pass. External writer/owner closure, scheduler ordering, legacy equivalence, persistence/network formats, renderer/chat protocol, geometry/world-zone/weather/event authority, variant catalog, inventory/storage ownership, C15 zero-denominator and talkNPC lifecycle, C17 integration with the actual Player spawn/removal call sites, and cross-domain integration remain unconfirmed. C17's isolated spawn/removal reset behavior is focused-verified at the component/system seam.
blocking-decision: C01-C18 remain isolated component/system/projection/query seams and are integration-blocked until legacy owners and scheduler contracts are approved; C01's `stepSpeed`, `gfxOffY`, and `fartKartCloudDelay` remain explicit input seams rather than claimed writers. C06 remains memory-only pending format evidence, C07 remains value-only pending registry evidence, C08 remains a one-way projection pending companion/effect owner evidence, C09 remains a pure snapshot query pending geometry authority evidence, C10 remains command-intent only pending appearance ownership, C11 remains a pure zone snapshot query plus deferred write command pending world-zone authority, C12 remains a pure non-contiguous `zone3` query plus deferred write command pending weather/depth authority, C13 remains a pure event/shopping query plus deferred write command pending world/event and shimmer authority, C14 remains a pure interaction query plus deferred vault command pending inventory/storage authority, C15 remains a pure composed query with explicit mount/weather inputs and no writer, C16 remains a pure runtime facade with adapter-owned scene/pose facts and no writer, C17 now has explicit isolated reset methods but actual legacy spawn/removal wiring, network ownership, and scheduler order remain outside the seam, and C18 remains an isolated projection/state seam pending rich-text protocol, chat/network adapter, renderer, scheduler, and legacy integration ownership.

## 1. Design Status and Scope

This document is a proposed decomposition of the 164 members in the P11 authoritative report.
The initial design pass did not create C# files or change the current NLTX implementation. The
implementation pass has now saved isolated source seams for C01-C18 and focused verification for
C01-C18 across the five dedicated verifier projects. This does not prove legacy integration,
behavior equivalence, network compatibility, or persistence. Cross-domain owners and adapters
remain explicitly open where the evidence ledger says `partial`.

In scope: player pose and animation state, camera/network presentation state, shadow and arm data,
visual effects, appearance customization, traversal and companion projections, derived player
queries, eye animation, and overhead messages.

Out of scope: combat authority, inventory/equipment authority, mount/vehicle authority, world
biome authority, network protocol implementation, save-file schema, and renderer implementation.
Those boundaries are integration inputs, not silently owned by P11.

## 2. Evidence Ledger

| source | version | query/evidence | result | evidenceStatus |
|---|---|---|---|---|
| `D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P11-Player-Presentation-Derived.md` | Version4 member inventory | 18 leaf groups, 91 fields, 73 properties, source rows 370..1498 | 164/164 rows match the runner count; no ID rows | confirmed |
| `D:\TRbackup\Version4\Terraria\Player.cs:43` | Version4 reference | `Player` declaration and embedded presentation fields | One aggregate owns gameplay, presentation, derived getters, network and persistence entry points | confirmed |
| `D:\TRbackup\Version4\Terraria\Player.cs:15728` | Version4 reference | `eyeHelper.Update(this)` in the player update path | Eye animation is stateful behavior invoked once per update | confirmed |
| `D:\TRbackup\Version4\Terraria.GameContent\PlayerEyeHelper.cs:24-153` | Version4 reference | `EyeState`, timer, frame projection, hurt transition | Internal state machine with player/world inputs and a mutable output | confirmed |
| `D:\TRbackup\Version4\Terraria\Player.cs:17775-17822` | Version4 reference | `UpdateNetOffset` and `ResetNetOffsets` | Network offset is updated and globally reset; it is not a passive projection | confirmed |
| `D:\TRbackup\Version4\Terraria\Player.cs:21798-21882` | Version4 reference | `Spawn` reset path | Pose, network offset, shadows, and spawn lifecycle are coupled at reset | confirmed |
| `D:\TRbackup\Version4\Terraria\Player.cs:22349` | Version4 reference | hurt path calls `BlinkBecausePlayerGotHurt` | Damage and eye presentation cross a P06/P11 seam | confirmed |
| `D:\TRbackup\Version4\Terraria\Player.cs:26394-26418` | Version4 reference | `SavePlayer`, `Serialize`, `Deserialize` | Save entry exists, but serialization bodies are empty in this snapshot | partial |
| `D:\TRbackup\无任何删减通过编译\Terraria\Player.cs:55348-55410,55750-55904` | complete reference supplement | `Serialize`/`Deserialize` for hair, skin variant, appearance colors, inventory, and void vault | Complete format and migration guards exist in the supplement; compatibility with the inspected Version4 snapshot is not proven | existing-evidence |
| `D:\TRbackup\Version4\Terraria\Main.cs:642,982,1440,1458` | Version4 reference | scene metrics, player registry, local-player projection | World/global and local-player access are shared external inputs | confirmed |
| `D:\TRbackup\NLTX\src\Player` | current NLTX | existing Player components | Identity, lifecycle, mount, inventory, ability, rest, and item-use components exist; no P11 runtime components | confirmed |
| `D:\TRbackup\NLTX\src\Content\AnimationDefinition.cs`, `ContentPresentationIndex.cs` | current NLTX | content-level animation and presentation definitions | Definition/catalog support exists, but it is not per-player presentation state | confirmed |
| `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Server\Wires\WiresComponent.cs:6-66` and `WiresSystem.cs:29-75,300-315,455-475,600-620` | SS14 reference | component state, event ingestion, update, Dirty/network boundary | Supports component/system separation only; no Terraria semantic authority | existing-evidence |
| `D:\TRbackup\tmodloader-api-docs-stable\index.html`, `class_player.html`, `class_main.html`, `struct_player_eye_helper.html` | tModLoader v2026.07 | `Player`, `Main.LocalPlayer`, `Main.player`, `PlayerEyeHelper.EyeFrameToShow` | Public API and type/member boundaries cross-checked; private Version4 ownership remains unresolved | partial |

The tModLoader mirror is API evidence, not a substitute for the Version4 private source. SS14 is
used only for ECS organization patterns. No external source is used to infer persistence,
replication, or execution order.

## 3. Boundary Model

```text
PlayerGameplay / Mobility / Combat / Inventory / Mount / World facts
        | read-only snapshots, explicit commands, and committed events
        v
P11 Owner Systems -> proposed Player* Components (transient or persistent by decision)
        | immutable presentation snapshot / query result
        +--> client Projection / Adapter / renderer boundary

P11 Queries are pure over explicit snapshots.
P11 Projections never write authority.
P11 network and persistence adapters are outside the component data types.
System order is a scheduler contract, never a file or directory ordering rule.
```

The initial proposed order is: (1) consume committed gameplay facts, (2) update pose and eye
state, (3) calculate pure derived queries, (4) commit presentation snapshots, (5) publish network
or client projections. The order is provisional until integration review assigns the shared owner.

## 4. Complete Member Inventory and Candidate Boundary

The following table covers every report row. Types and exact declarations remain authoritative in
the input report; source ranges identify the corresponding Version4 declaration region.

| unit | report members | Version4 declaration | proposed boundary | status |
|---|---|---|---|---|
| C01 PlayerPoseAndAnimationState | `headRotation`, `bodyRotation`, `legRotation`, `headPosition`, `bodyPosition`, `legPosition`, `headVelocity`, `bodyVelocity`, `legVelocity`, `fullRotation`, `fullRotationOrigin`, `fartKartCloudDelay`, `gfxOffY`, `stepSpeed` | `Player.cs:1099-1126` | `PlayerPoseAndAnimationStateComponent` + owner system; `fartKartCloudDelay` and `stepSpeed` require integration review | implemented-isolated |
| C02 PlayerNetworkCameraState | `netOffset`, `netCameraTarget`, `lastSyncedNetCameraTarget` | `Player.cs:1128-1132` | `PlayerNetworkCameraStateComponent` plus one-way camera/network projection | implemented-isolated |
| C03 PlayerShadowAndArmPresentation | `cursorItemIconReversed`, `runSoundDelay`, `shadowPos`, `shadowRotation`, `shadowOrigin`, `shadowDirection`, `shadowCount`, `skipAnimatingValuesInPlayerFrame`, `availableAdvancedShadowsCount`, `_advancedShadows`, `_lastAddedAvancedShadow`, `compositeFrontArm`, `compositeBackArm` | `Player.cs:1296-1342` | `PlayerShadowAndArmPresentationComponent` + presentation system; audio/UI fields remain explicit seams | implemented-isolated |
| C04 PlayerVisualAndShaderEffects | `dontStarveShader`, `noirShader`, `eyebrellaCloud`, `yoraiz0rEye`, `yoraiz0rDarkness`, `hasUnicornHorn`, `hasAngelHalo`, `hasRainbowCursor`, `leinforsHair`, `musicBoxSilence`, `stardustMonolithShader`, `nebulaMonolithShader`, `vortexMonolithShader`, `solarMonolithShader`, `moonLordMonolithShader`, `bloodMoonMonolithShader`, `shimmerMonolithShader`, `CRTMonolithShader`, `retroMonolithShader`, `musicBox`, `overrideFishingBobber` | `Player.cs:1433-1473` | `PlayerVisualAndShaderEffectsProjection`; `musicBox` and `overrideFishingBobber` stay integration-review seams | implemented-isolated |
| C05 PlayerFootballPresentationState | `hasFootball`, `drawingFootball` | `Player.cs:2008-2010` | `PlayerFootballPresentationStateComponent` + draw projection | implemented-isolated |
| C06 PlayerAppearanceCustomizationState | `hairDye`, `skinDyePacked`, `hairColor`, `skinColor`, `eyeColor`, `shirtColor`, `underShirtColor`, `pantsColor`, `shoeColor`, `hair` | `Player.cs:1947-1966` | `PlayerAppearanceCustomizationComponent` + persistence/network adapter after policy decision | implemented-isolated |
| C07 PlayerTraversalColorProjection | `cWings`, `cCarpet`, `cFloatingTube`, `cGrapple`, `cMount`, `cMinecart` | `Player.cs:2332-2348` | `PlayerTraversalColorProjection` (one-way client projection) | implemented-isolated |
| C08 PlayerAppearanceCompanionAndEffectProjection | `cPet`, `cLight`, `cYorai`, `cPortableStool`, `cUnicornHorn`, `cAngelHalo`, `cBeard`, `cMinion`, `cLeinShampoo`, `cFlameWaker`, `cCoat` | `Player.cs:2350-2370` | `PlayerAppearanceCompanionAndEffectProjection` (one-way client projection) | implemented-isolated |
| C09 PlayerSpatialDerivedProperties | `BlehOldPositionFixer`, `HeightOffsetHitboxCenter`, `HeightOffsetBoost`, `HitboxForBestiaryNearbyCheck`, `IsConsideredStandingStill`, `BaseHeight`, `MountedCenter`, `VisualPosition`, `CCed` | `Player.cs:2503-2581` | `PlayerSpatialDerivedPropertiesQuery` over explicit geometry/mount/status snapshots | implemented-isolated |
| C10 PlayerIdentityAndDerivedProperties | `miscCounterNormalized`, `Male` | `Player.cs:2593-2613` | `PlayerIdentityAndDerivedPropertiesQuery` for normalized read plus explicit gender/skin compatibility command; `Male` is not pure because its setter changes `skinVariant` | implemented-isolated |
| C11 PlayerBiomeZoneProperties | `ZoneDungeon`, `ZoneCorrupt`, `ZoneHallow`, `ZoneMeteor`, `ZoneJungle`, `ZoneSnow`, `ZoneCrimson`, `ZoneWaterCandle`, `ZonePeaceCandle`, `ZoneTowerSolar`, `ZoneTowerVortex`, `ZoneTowerNebula`, `ZoneTowerStardust`, `ZoneDesert`, `ZoneGlowshroom`, `ZoneUndergroundDesert` | `Player.cs:2617-2805` | `PlayerBiomeZonePropertiesQuery` for reads plus an explicit environment-zone commit seam; setters alias mutable `zone1/zone2` arrays and are not pure query operations | implemented-isolated |
| C12 PlayerVerticalAndWeatherZoneProperties | `ZoneSkyHeight`, `ZoneOverworldHeight`, `ZoneUnderworldHeight`, `ZoneBeach`, `ZoneRain`, `ZoneSandstorm` | `Player.cs:2809-2879` | `PlayerVerticalAndWeatherZonePropertiesQuery` for reads plus explicit zone-state commit seam; properties alias mutable `zone3` slots | implemented-isolated |
| C13 PlayerEventAndShoppingZoneProperties | `ZoneOldOneArmy`, `ZoneLihzhardTemple`, `ZoneGraveyard`, `ZoneShadowCandle`, `ZoneShimmer`, `ShoppingZone_AnyBiome`, `ShoppingZone_BelowSurface` | `Player.cs:2882-2955` | event-zone read Query plus explicit zone-state commit seam; shopping properties are pure calculations over zone/depth snapshots | implemented-isolated |
| C14 PlayerInteractionAndSelectionProperties | `Directions`, `selectedItem`, `HeldItem`, `ShouldFloatInWater`, `CanBeTalkedTo`, `IsVoidVaultEnabled`, `ReportedCameraPosition`, `TryingToHoverUp`, `TryingToHoverDown` | `Player.cs:2958-3038` | `PlayerInteractionAndSelectionPropertiesQuery` for reads plus explicit void-vault command seam; item selection, camera and hover inputs stay integration-review | implemented-isolated |
| C15 PlayerAbilityAndPresentationProperties | `UsingBiomeTorches`, `UsingSuperCart`, `bowEffectiveDamage`, `gunEffectiveDamage`, `specialistEffectiveDamage`, `CanUseBootFlyingAbilities`, `CanUseWingAbilities`, `ShouldNotDraw`, `talkNPC`, `isLockedToATile`, `PortalPhysicsEnabled`, `MountFishronSpecial` | `Player.cs:3042-3150` | composed read facade over combat, ability, equipment, draw, rest, teleport, and mount owners; setters for biome torches/super cart are explicit command seams | implemented-isolated |
| C16 PlayerItemMountAndRuntimeProperties | `HasMinionRestTarget`, `ItemTimeIsZero`, `ItemAnimationJustStarted`, `UsingOrReusingItem`, `SceneMetrics`, `SpectatingCameraPosition`, `SlimeDontHyperJump`, `hasBreathingReed`, `IsRidingTracks`, `MouthPosition`, `HandPosition` | `Player.cs:3152-3279` | composed pure query facade; item, mount/vehicle, camera, scene metrics, and hand-position calculations remain integration-review seams | implemented-isolated |
| C17 PlayerEyeAnimationState | `_state`, `_timeInState`, `TimeToActDamaged`, `EyeFrameToShow` | `PlayerEyeHelper.cs:24-30` | `PlayerEyeAnimationComponent` + `PlayerEyeAnimationSystem`; not a passive projection | implemented-isolated |
| C18 PlayerPresentationMessagesAndArms | `enabled`, `stretch`, `rotation`, `chatText`, `snippets`, `messageSize`, `timeLeft`, `color` | `Player.cs:226-230,457-465` | arm fields share C03's presentation snapshot; overhead fields use `PlayerOverheadMessageStateComponent/System` and `PlayerOverheadMessageProjection`; message expiry is a short-lived output state | implemented-isolated |

No row is dropped. C03/C18 deliberately share an arm snapshot boundary while retaining the
overhead-message projection as a separate output. C01, C02, C04, C14, C15, and C16 contain
cross-domain members that are mapped but not assigned a final cross-subsystem owner.

## 5. Non-negotiable Ownership Rules

- Proposed components store state only; they do not call clocks, random generators, logging,
  persistence, networking, or renderer APIs.
- Owner systems receive explicit tick/input/fact snapshots and are the only writers during the
  migration window. Queries are read-only and deterministic over their arguments.
- Adapters own Version4 field/API conversion, serialization, network encoding, and external type
  translation. Projections publish immutable output and cannot write back to components.
- `netOffset`, `cMount`, `cMinecart`, `stepSpeed`, `overrideFishingBobber`, `musicBox`, held item
  references, effective damage, mount predicates, and scene metrics are `crossSubsystemOwner:
  integration-review` until P03/P06/P07/P08/P09/P10/P12/P16/P17/P18/P19/P20 owners respond.
- Persistent appearance fields cannot be declared durable until SavePlayer serialization and load
  semantics are confirmed. Do not silently make transient presentation state persistent.

## 6. Risk, Evidence Gap, and Focused Verification Plan

The primary behavior risks are reset ordering at spawn/teleport, update ordering between gameplay
facts and presentation projections, shared array/reference mutation, duplicate network writes,
client projection becoming an authority, and serialization silently omitting appearance state.

Focused isolated verification has been executed for the saved implementation:

1. The source inventory audit confirmed all 164 report rows map exactly once to the C01-C18
   design table: 91 fields, 73 properties, 164 unique IDs, and no duplicate member names.
2. The C02-C15 verifier covers immutable projections, bounded shadow state, component transitions,
   pure spatial/zone/interaction/ability queries, and the documented command seams. It does not
   prove runtime writer uniqueness or scheduler ordering.
3. The C01 pose/reset verifier passed frame progression, early return, spawn reset, and teleport
   reset for the explicit pose boundary.
4. The C16 verifier passed item, mount, scene-metric, camera, track, and hand/mouth derived-query
   cases; the C17 verifier passed hurt hold, precedence, sleep, storm, and frame transitions.
5. The C18 verifier passed value copying, message replacement, expiry, spawn/removal cleanup, and
   one-way arm/message projection. These focused checks do not prove renderer or protocol parity.
6. Persistence/network round-trip, duplicate-packet, legacy-equivalence, scheduler, and full
   runtime integration verification remain unrun because the required external owners and format
   evidence are still missing.

`verificationStatus: partial` records that all five focused verifier projects and the affected
Player project built and passed for their isolated scopes. Runtime integration, network,
persistence, legacy-equivalence, and scheduler verification remain unverified.

## 7. Integration Handoff

The integration owner must decide: (a) whether P11 is a client projection capability or also owns
server-side transient state, (b) the single owner for network offset and camera target, (c) whether
appearance colors/hair are persisted and replicated, (d) the source of world/biome snapshots, and
(e) the scheduler contract around PlayerGameplay, PlayerMobility, PlayerCombat, MountVehicle, and
client presentation. Until then the design is review-ready but implementation-blocked.

## 8. Component Checkpoints

Detailed component sections are appended as each checkpoint is completed. The metadata at the top
of this file is updated together with the execution document after every checkpoint.

### C01 PlayerPoseAndAnimationState

- Proposed type: `PlayerPoseAndAnimationStateComponent`, `PlayerPoseAndAnimationStateSystem`,
  status `implemented-isolated`.
- Ownership: the system receives committed movement/gameplay facts and an explicit tick; it alone
  writes rotations, segment positions/velocities, full rotation/origin, `gfxOffY`, and the transient
  delay. `stepSpeed` remains `crossSubsystemOwner: integration-review` because mobility and mount
  code can define it.
- Lifecycle: create with a player entity, update during the explicit pose phase, reset during spawn
  and teleport paths. Do not persist frame-local pose until a save policy is evidenced.
- Seam: `IPlayerPoseInputSnapshot` in, immutable `PlayerPoseSnapshot` out; no renderer, clock,
  random, or network calls in the component. The adapter owns legacy field access during migration.
- Evidence: declarations `Player.cs:1099-1126`, frame mutation `Player.cs:10046-10048`, spawn reset
  `Player.cs:21817-21823`; complete readers/writers and cross-owner ordering remain partial.
- Verifier: compare frame rotation/offset updates, spawn reset, teleport reset, and early-return
  behavior against a focused deterministic fixture before removing legacy writes.
- Implementation checkpoint: `IPlayerPoseInputSnapshot`, `PlayerPoseInputSnapshot`,
  `PlayerPoseAndAnimationStateComponent`, `PlayerPoseAndAnimationSystem`, and
  `PlayerPoseSnapshot` are saved under `src/Player`. The owner applies explicit velocity and
  derived-value inputs, preserves the Version4 segment position/rotation progression, and exposes
  spawn/teleport reset methods without accessing `Player`, `Main`, renderer, network, or
  persistence APIs. `stepSpeed`, `gfxOffY`, and `fartKartCloudDelay` remain explicit input seams;
  their legacy writers are not claimed.

### C02 PlayerNetworkCameraState

- Types: `PlayerNetworkCameraStateComponent` plus `PlayerNetworkCameraProjection`, status
  `implemented-isolated`; the component is a transient boundary, not a network authority declaration.
- Ownership: `netOffset` is written by the pose/mobility owner and reset by the explicit lifecycle
  system; `netCameraTarget` and `lastSyncedNetCameraTarget` are handled by a camera/network adapter.
  No client projection may write either value.
- Lifecycle: initialize per player, update in the documented movement/camera phase, clear on spawn,
  teleport, disconnect, and entity removal after those paths are evidenced.
- Seam: immutable `PlayerCameraSnapshot` to camera and replication adapters; packet encoding,
  reconciliation, and local-player selection stay outside the component.
- Evidence: declarations `Player.cs:1128-1132`, decay/reset `Player.cs:17775-17822`, teleport reset
  `Player.cs:21764-21767`; replication readers/writers and ownership are still partial.
- Verifier: reset/decay invariant, local versus remote player selection, duplicate packet rejection,
  and proof that projection output cannot mutate the component.
- Implementation checkpoint: `PlayerNetworkCameraStateComponent`,
  `PlayerNetworkCameraInputSnapshot`, `PlayerCameraSnapshot`,
  `PlayerNetworkCameraStateSystem`, and `Presentation/PlayerNetworkCameraProjection` are saved.
  The system keeps offset decay, fake-offset input, camera-target state, synchronization marking,
  and spawn/teleport reset inside an explicit boundary. It does not call `Main`, `NetMessage`,
  collision, renderer, persistence, or packet APIs.

### C03 PlayerShadowAndArmPresentation

- Types: `PlayerShadowAndArmPresentationComponent` and owner system, status `implemented-isolated`;
  `CompositeArmData` is a value snapshot, not a reference to renderer state.
- Ownership: the system owns bounded shadow arrays, advanced-shadow cursor/count, arm data, and
  frame-animation flags. `cursorItemIconReversed` and `runSoundDelay` are output seams and need
  UI/audio owner confirmation rather than unrestricted writes.
- Lifecycle: allocate fixed capacities on entity creation, update during the pose/presentation phase,
  reset on spawn/teleport, and clear on removal. Preserve array length and ordering invariants.
- Seam: `PlayerShadowSnapshot` and `CompositeArmSnapshot` to projection; `EntityShadowInfo` and
  external drawing/audio types are translated in an adapter.
- Evidence: declarations `Player.cs:1296-1342`, shadow reset/use `Player.cs:21783-21787` and
  `Player.cs:9845-9859`, arm mutation `Player.cs:3352`; complete readers and renderer schedule partial.
- Verifier: fixed-size array bounds, reset count, advanced-shadow ring behavior, arm value semantics,
  and no mutation by client projection.
- Implementation checkpoint: `Presentation/PlayerShadowAndArmPresentationComponent`,
  `PlayerShadowAndArmPresentationSystem`, `PlayerShadowAndArmPresentationInput`,
  `PlayerShadowSnapshot`, `PlayerCompositeArmSnapshot`, `PlayerArmStretchAmount`, and
  `PlayerAdvancedShadowSlot` are saved. The component keeps fixed three-slot social shadows and
  sixty-slot advanced shadows, preserves the reference three-sample rotation and advanced-shadow
  cursor/count bounds, and copies all arrays into the immutable output. Renderer, audio, UI, and
  `EntityShadowInfo` conversion remain outside the component.

### C04 PlayerVisualAndShaderEffects

- Type: `PlayerVisualAndShaderEffectsProjection`, status `implemented-isolated`; this is output state,
  not a gameplay authority component.
- Ownership: a visual-effects system derives shader/halo/cursor flags from committed player facts;
  it publishes an immutable snapshot. `musicBox`, `musicBoxSilence`, and `overrideFishingBobber`
  remain `crossSubsystemOwner: integration-review` for audio and fishing owners.
- Lifecycle: rebuild or invalidate with equipment/status/appearance changes; clear on spawn and entity
  removal; no save-file ownership until persistence evidence exists.
- Seam: stable internal IDs and booleans in, renderer shader/audio/fishing adapters out. External
  shader registries and particle calls are effect ports, never component dependencies.
- Evidence: declarations `Player.cs:1433-1473`, an update mutation at `Player.cs:8096` and reset at
  `Player.cs:10457`; complete derivation and client/server split remain partial.
- Verifier: deterministic flag derivation, invalidation after equipment/status changes, and proof that
  a projection cannot set gameplay flags.
- Implementation checkpoint: `Presentation/PlayerVisualAndShaderEffectsInput`,
  `PlayerVisualAndShaderEffectsSnapshot`, and `PlayerVisualAndShaderEffectsProjection` are saved.
  The projection copies all 21 report fields into immutable output and does not derive shader IDs,
  call renderer globals, or write gameplay state. `musicBox` and `overrideFishingBobber` remain
  explicit audio/fishing integration inputs.

### C05 PlayerFootballPresentationState

- Types: `PlayerFootballPresentationStateComponent` plus draw projection, status `implemented-isolated`.
- Ownership: the owner system receives the football/equipment or event fact and writes `hasFootball`
  and `drawingFootball`; renderer reads only the output snapshot.
- Lifecycle: initialize false, update from the committed item/event state, clear on spawn, item loss,
  and entity removal. Persistence is deferred because the report does not establish ownership.
- Seam: `PlayerFootballPresentationSnapshot` to a client projection; no direct renderer or item
  mutation from the component.
- Evidence: declarations `Player.cs:2008-2010`; complete write/read and network evidence is partial.
- Verifier: transition table for acquire, lose, draw, spawn reset, duplicate event, and client-only
  projection.
- Implementation checkpoint: `Presentation/PlayerFootballPresentationInput`,
  `PlayerFootballPresentationStateComponent`, `PlayerFootballPresentationStateSystem`,
  `PlayerFootballPresentationSnapshot`, and `PlayerFootballPresentationProjection` are saved.
  The system deterministically separates possession from draw eligibility, handles item/event loss
  and spawn reset, and publishes output without accessing inventory, renderer, or network APIs.

### C06 PlayerAppearanceCustomizationState

- Type: `PlayerAppearanceCustomizationComponent`, status `implemented-isolated`; it is the only P11
  candidate treated as potentially persistent, pending explicit save/network policy.
- Ownership: appearance/customization command system writes hair, packed dye, and color fields; it
  consumes validated identity/equipment input and emits an immutable appearance snapshot.
- Lifecycle: load/create -> validate -> update -> snapshot; spawn must not reset chosen appearance,
  while entity removal releases transient adapters. Save/load ownership is not assumed.
- Seam: stable `Color`/dye value objects at the domain boundary; `PlayerFileData`, binary IO, packet
  code, and renderer types stay in adapters.
- Evidence: declarations `Player.cs:1947-1966`, save entry `Player.cs:26394-26418`; serialization
  and deserialization are empty in the inspected reference, so persistence is `partial`.
- Verifier: color/dye round-trip once format is supplied, invalid input rejection, no duplicate
  writes, client projection immutability, and spawn preservation.
- Implementation checkpoint: `PlayerAppearanceColor`, `PlayerAppearanceCustomizationInput`,
  `PlayerAppearanceCustomizationComponent`, `PlayerAppearanceCustomizationSystem`, and
  `PlayerAppearanceCustomizationSnapshot` are saved. The owner stores all 10 report fields and
  publishes a value snapshot using the documented Version4 default colors. Save-file and network
  adapters are intentionally absent because the inspected serialization evidence is incomplete.

### C07 PlayerTraversalColorProjection

- Proposed type: `PlayerTraversalColorProjection`, status `implemented-isolated`; projection only, with no
  authority over wings, carpet, grapple, mount, or minecart state.
- Ownership: an adapter derives the six color/variant IDs from committed traversal facts and emits
  a snapshot. P03 mount/vehicle and P07 mobility remain possible source owners.
- Lifecycle: invalidate on traversal equipment or mount changes, rebuild before draw, and clear on
  spawn/removal. IDs are not persisted by this projection.
- Seam: `TraversalPresentationSnapshot` out to the client renderer; no external color registry is
  called by the component or query.
- Evidence: declarations `Player.cs:2332-2348`; readers/writers, replication and cache invalidation
  are partial.
- Verifier: mapping stability, invalidation, no stale mount/minecart color, and one-way output.
- Implementation checkpoint: `Presentation/PlayerTraversalColorInput`,
  `PlayerTraversalColorSnapshot`, and `PlayerTraversalColorProjection` are saved. The projection
  copies the six report slots without owning mount, minecart, mobility, or color-registry state.

### C08 PlayerAppearanceCompanionAndEffectProjection

- Type: `PlayerAppearanceCompanionAndEffectProjection`, status `implemented-isolated`; projection only.
- Ownership: derive companion/effect IDs from committed equipment, buff, pet, and appearance facts;
  P05/P06/P08 and content definitions remain source owners.
- Lifecycle: invalidate when source facts change, clear on spawn/removal, and publish one immutable
  snapshot per presentation phase. No ID becomes persistent merely because it is displayed.
- Seam: `CompanionEffectPresentationSnapshot` to client projection; pet/light/effect services are
  adapters and cannot write player authority.
- Evidence: declarations `Player.cs:2350-2370`; full source readers, network rules, and invalidation
  ordering are partial.
- Verifier: source-to-ID mapping, removal invalidation, duplicate event handling, and no reverse write.

- Implementation checkpoint: `Presentation/PlayerAppearanceCompanionAndEffectInput`,
  `PlayerAppearanceCompanionAndEffectSnapshot`, and
  `PlayerAppearanceCompanionAndEffectProjection` are saved. The projection copies all 11
  authoritative companion/effect slots in report order and does not call pet, light, effect,
  renderer, content, network, or persistence services. Source writers, invalidation/despawn
  timing, client ownership, duplicate-event behavior, and legacy output parity remain unverified.

### C09 PlayerSpatialDerivedProperties

- Type: `PlayerSpatialDerivedPropertiesQuery`, status `implemented-isolated`; no spatial authority is
  created by the query.
- Ownership: pure calculation over explicit position, geometry, mount, movement, and status facts.
  `BlehOldPositionFixer` is treated as an input value until its historical compatibility role is
  confirmed; `CCed` remains a status seam.
- Lifecycle: no independent lifecycle or cache; recompute from the supplied snapshot. Any cache must
  declare invalidation on position, mount, gravity, dimensions, and status changes.
- Seam: `PlayerSpatialSnapshot` in, value results out; no entity lookup, global mutation, or renderer
  access inside the query.
- Evidence: declarations `Player.cs:2503-2581`; pure getter bodies are only partially audited for
  all readers and cache assumptions.
- Verifier: deterministic repeated evaluation, geometry edge cases, mount/offset cases, and no input
  mutation.

- Implementation checkpoint: `PlayerSpatialSnapshot`, `PlayerSpatialHitbox`, and
  `PlayerSpatialDerivedPropertiesQuery` are saved under `src/Player`. The query covers all nine
  report properties with explicit snapshot input, preserves mount-before-portable-stool precedence,
  Version4 integer hitbox conversion and 0.05 standing-still threshold, and leaves the
  `MountedCenter` setter outside the read-only boundary. No global lookup, cache, mutation, or
  renderer dependency was added.

### C10 PlayerIdentityAndDerivedProperties

- Types: `PlayerIdentityAndDerivedPropertiesQuery` and a compatibility command seam for
  `Male`, both status `implemented-isolated`; no new identity authority is declared.
- Ownership: `miscCounterNormalized` is a pure calculation. `Male` reads the variant catalog but its
  setter mutates `skinVariant`, so it must be treated as a command/compatibility facade until the
  Player appearance owner accepts that write.
- Lifecycle: normalized value is recomputed; gender/skin variant follows appearance load/update and
  must not be reset by presentation projection.
- Seam: explicit identity/appearance snapshot in, read result out; `SetMale`-style command out to the
  appearance owner. No query setter or hidden mutation.
- Evidence: `Player.cs:2593-2613` shows the calculation, catalog lookup, and setter mutation. Full
  call-site and persistence evidence remains partial.
- Verifier: normalized counter calculation, male/alternate-variant transitions, idempotent command,
  and proof that read queries do not mutate `skinVariant`.

- Implementation checkpoint: `PlayerIdentityAndDerivedPropertiesInput`,
  `PlayerIdentityAndDerivedPropertiesSnapshot`, `PlayerMaleChangeCommand`, and
  `PlayerIdentityAndDerivedPropertiesQuery` are saved under `src/Player`. The query preserves the
  `miscCounter / 300` calculation and returns the current variant without mutation; the male setter
  is represented by an explicit command intent that requests `AltGenderReference` only when the
  requested gender differs. No catalog lookup, appearance write, persistence, or network adapter
  was invented.

### C11 PlayerBiomeZoneProperties

- Types: `PlayerBiomeZonePropertiesQuery` for read-only results and a separate
  `PlayerBiomeZoneState`/commit seam, status `implemented-isolated`; the public setters remain compatibility
  commands until their writer is identified.
- Ownership: the 16 getters/setters expose slots in `zone1` and `zone2`; they are aliases over mutable
  state rather than independently derived booleans. A world/environment system may calculate them,
  but P11 does not claim that owner.
- Lifecycle: refresh after spatial/world metrics are committed, clear or rebuild on world/player
  lifecycle changes, and never cache without invalidation for tile, depth, event, or weather inputs.
- Seam: immutable `PlayerZoneSnapshot` into queries; explicit `SetZoneFlag` command to the eventual
  world/environment owner. No query writes the backing arrays.
- Evidence: `Player.cs:2617-2805` shows zone slot aliases and setters; the source of array writes and
  complete update ordering are not closed.
- Verifier: slot-to-property mapping, setter ownership, refresh invalidation, and pure repeated query
  evaluation, including simultaneous zone flags.

- Implementation checkpoint: `PlayerZoneSnapshot`, `PlayerBiomeZonePropertiesSnapshot`,
  `PlayerBiomeZonePropertiesQuery`, `PlayerZoneFlag`, and `PlayerZoneFlagChangeCommand` are saved
  under `src/Player`. The query copies all 16 `zone1`/`zone2` aliases in report order; the command
  carries a zone flag update for the eventual world/environment owner and does not mutate a backing
  array. No second world-zone authority or cache was introduced.

### C12 PlayerVerticalAndWeatherZoneProperties

- Types: `PlayerVerticalAndWeatherZonePropertiesQuery` and a shared zone-state commit
  seam, status `implemented-isolated`; no new `zone3` authority is created here.
- Ownership: the six properties map to `zone3` slots, including the non-contiguous indices 0, 1, 4,
  5, 6, and 7. Setters are mutable compatibility accessors, not query behavior.
- Lifecycle: recompute/commit after depth and weather facts, invalidate on position/world weather
  changes, and clear at world/player lifecycle boundaries as the owner decides.
- Seam: `VerticalWeatherSnapshot` is pure query input; write commands go to the environment owner.
- Evidence: `Player.cs:2809-2879` confirms slot mapping and setters; source writers and scheduling
  remain partial.
- Verifier: exact slot mapping, boundary values at height/beach/weather thresholds, and no query
  write-back.

- Implementation checkpoint: `PlayerVerticalAndWeatherZoneInput`,
  `PlayerVerticalAndWeatherZonePropertiesSnapshot`, `PlayerVerticalAndWeatherZonePropertiesQuery`,
  `PlayerVerticalAndWeatherZoneFlag`, and `PlayerVerticalAndWeatherZoneFlagChangeCommand` are
  saved under `src/Player`. The query preserves the non-contiguous `zone3` indices `0, 1, 4, 5, 6,
  7`; the command is only an environment-owner write intent. No weather, depth, or zone-array state
  is mutated by this boundary.

### C13 PlayerEventAndShoppingZoneProperties

- Types: `PlayerEventAndShoppingZonePropertiesQuery` and the shared zone-state commit
  seam, status `implemented-isolated`.
- Ownership: event-zone properties alias `zone4`/`zone5` slots and retain mutable compatibility
  setters. `ShoppingZone_AnyBiome` derives from several zone flags; `ShoppingZone_BelowSurface`
  derives from position and `Main.worldSurface`.
- Lifecycle: event flags refresh with event/terrain facts; shopping results recompute per query and
  must not cache without invalidation on zone, position, or world-surface changes.
- Seam: `EventShoppingSnapshot` in, pure shopping qualification out; event writes remain explicit
  commands to world/event owners. `ZoneShimmer` is an integration seam with shimmer/liquid owners.
- Evidence: `Player.cs:2882-2955` confirms aliases, composite biome logic, and world-global depth
  dependency; full writers and event ordering are partial.
- Verifier: exact slot mapping, AnyBiome truth table, below-surface threshold, event reset, and no
  query mutation.

- Implementation checkpoint: `PlayerEventAndShoppingZoneInput`,
  `PlayerEventAndShoppingZonePropertiesSnapshot`, `PlayerEventAndShoppingZonePropertiesQuery`,
  `PlayerEventZoneFlag`, and `PlayerEventZoneFlagChangeCommand` are saved under `src/Player`. The
  query copies the five `zone4`/`zone5` event slots, computes `ShoppingZone_AnyBiome` from the
  explicit biome flags, and computes `ShoppingZone_BelowSurface` from `Position.Y` and
  `WorldSurface`; event and shimmer changes remain explicit owner commands.

### C14 PlayerInteractionAndSelectionProperties

- Types: `PlayerInteractionAndSelectionPropertiesQuery` plus a command seam for
  `IsVoidVaultEnabled`, status `implemented-isolated`.
- Ownership: direction, selected slot, held item, float/talk/hover predicates, and reported camera
  position are read calculations. `IsVoidVaultEnabled` has a setter that mutates `voidVaultInfo[0]`;
  it must not be hidden inside a Query.
- Lifecycle: input and camera snapshots update per tick; held-item results follow inventory slot
  commit; vault state follows inventory/storage policy; all reset behavior requires evidence.
- Seam: `PlayerInteractionSnapshot` in, immutable predicates out; explicit selection/vault commands
  go to PlayerInput/Inventory/WorldStorage owners. `HeldItem` is a read-only view, not an item owner.
- Evidence: `Player.cs:2958-3038` confirms the getters, `inventory[selectedItem]` read, camera fallback,
  and void-vault setter; full call-site and authority evidence is partial.
- Verifier: selection bounds, held-item identity, hover truth table, talk gating, camera fallback,
  vault command idempotence, and no query writes.

- Implementation checkpoint: `PlayerInteractionAndSelectionPropertiesInput`,
  `PlayerInteractionAndSelectionPropertiesSnapshot`, `PlayerInteractionAndSelectionPropertiesQuery`,
  and `PlayerVoidVaultStateChangeCommand` are saved under `src/Player`. The pure query uses explicit
  direction, gravity, inventory, mount, lifecycle, camera, and hover inputs; an out-of-range selected
  slot yields `ItemEntityRef.None`, a missing camera target falls back to position, and vault changes
  remain an explicit command. No inventory, storage, mount, or camera owner is mutated.

### C15 PlayerAbilityAndPresentationProperties

- Proposed type: `PlayerAbilityAndPresentationPropertiesQuery`, status `implemented-isolated`; it is a facade
  over smaller owner queries, not a new cross-subsystem component.
- Ownership: effective damage belongs to combat inputs; flight ability to mobility; super cart and
  `MountFishronSpecial` to mount; `PortalPhysicsEnabled` to teleport; `isLockedToATile` to rest;
  `ShouldNotDraw` to presentation. `UsingBiomeTorches` and `UsingSuperCart` setters mutate backing
  state and must become explicit commands.
- Lifecycle: recompute from committed snapshots; no cache unless each source invalidation is explicit.
  `talkNPC` is a privately set compatibility value and requires interaction owner confirmation.
- Seam: composed `PlayerAbilityPresentationSnapshot` in, pure result values out; commands go to the
  relevant owner, never to this facade.
- Evidence: `Player.cs:3042-3150` confirms setter mutation, damage formulas, draw gating, private
  setter, rest/mount/teleport conditions; cross-subsystem writers and scheduler remain partial.
- Verifier: formula parity, ability gating, draw/talk/rest predicates, mount/teleport combinations,
  command ownership, and no facade mutation.

### C15 Implementation Checkpoint (2026-09-11T19:13:30Z)

- Source files saved under `src/Player/`: `PlayerAbilityAndPresentationPropertiesInput.cs`,
  `PlayerAbilityAndPresentationPropertiesSnapshot.cs`, and
  `PlayerAbilityAndPresentationPropertiesQuery.cs`.
- Core behavior: pure composition of the twelve report properties. Biome torch and Super Cart
  values preserve the unlock gates; effective damage uses the Version4 bow, gun, and specialist
  formulas; boot/wing, draw, talk-NPC, rest-lock, portal, and Fishron predicates consume only
  explicit inputs. `MountFishronSpecial` uses explicit life, liquid, dripping, counter, rain, and
  wind facts and does not access `Main` or `WorldGen`.
- Dependency impact: existing progression queries and preference command remain the write-owner
  boundary; combat, mobility, mount, rest, teleport, and presentation are explicit input owners.
  No legacy Player API, global state, persistence, network, renderer, or other partition source was
  changed.
- Evidence gap: zero-denominator behavior for `rangedMultDamage`, complete owner/call-site and
  `talkNPC` lifecycle closure, formula/gating truth-table verification, affected-project build,
  behavior equivalence, and scheduler integration remain unverified.
- Verification status: `not-run` for the C15 build and focused query verifier; source is saved and
  both documents are synchronized before starting C16.

### C16 PlayerItemMountAndRuntimeProperties

- Proposed type: `PlayerItemMountAndRuntimePropertiesQuery`, status `implemented-isolated`; it must remain a
  composed facade rather than a large authority component.
- Ownership: item timers/reuse and minion rest read PlayerItemUse/PlayerAbility facts; scene metrics
  come from the world scene owner; spectating camera consumes camera and another player's committed
  pose; slime/track/mouth/hand results consume MountVehicle and item/pose snapshots.
- Lifecycle: recompute per explicit snapshot; no persistent cache. `MouthPosition` and `HandPosition`
  are nullable output values and must preserve mount delegate override precedence.
- Seam: `PlayerItemMountRuntimeSnapshot` in, pure outputs out; `Main.PlayerSceneMetrics`, mount
  delegates, inventory, and renderer offsets are adapters or integration inputs.
- Evidence: `Player.cs:3152-3265+` confirms item formulas, static scene metric access, spectating
  lookup, mount conditions, delegate overrides, and global offset table dependency; full call graph
  is partial.
- Verifier: timer boundaries, scene metric identity, spectating fallback, mount delegate precedence,
  track/slime predicates, nullable hand/mouth output, and query purity.

### C16 Implementation Checkpoint (2026-09-11T19:16:00Z)

- Source files saved under `src/Player/`: `PlayerSceneMetricsSnapshot.cs`,
  `PlayerSpectatingCameraTargetSnapshot.cs`, `PlayerItemMountAndRuntimePropertiesInput.cs`,
  `PlayerItemMountAndRuntimePropertiesSnapshot.cs`, and
  `PlayerItemMountAndRuntimePropertiesQuery.cs`. Focused verifier files are under
  `src/PlayerItemMountRuntimeVerification/`.
- Core behavior: all eleven report properties are exposed through a pure query. Item-time,
  animation-start, reuse, rest-target, slime, breathing-reed, and track predicates preserve the
  Version4 conditions. Scene metrics are passed through an explicit immutable snapshot; spectator
  camera math preserves bottom, gfx offset, and net offset. Mount mouth/hand overrides take
  precedence only while mounted, with explicit fallback values otherwise.
- Dependency impact: no `Main`, inventory object graph, mount delegate object, renderer, scene owner,
  network/persistence adapter, legacy Player API, or other partition source was changed. External
  scene/pose/mount adapters remain responsible for supplying snapshot facts and computed fallbacks.
- Evidence gap: the complete `SceneMetrics` field catalog, spectating registry invalidation,
  `MountID.Sets.DontHoldItems`, mount delegate lifecycle, and Version4 `OffsetsPlayerOnhand`/
  `ApplyItemPositionOffsetFromMount` parity remain unconfirmed; behavior equivalence and scheduler
  integration remain unverified.
- Verification: RED verifier build exited 1 with expected missing C16 type diagnostics. GREEN build
  used `& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\\src\\PlayerItemMountRuntimeVerification\\Terraria.PlayerItemMountRuntimeVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')`, exit code 0, 0 warnings, 0 errors; artifacts are under `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` and `Build/bin/Terraria.PlayerItemMountRuntimeVerification/Debug/net10.0/Terraria.PlayerItemMountRuntimeVerification.dll`. Focused run used the same wrapper with `run --project ... --no-build --no-restore`, exit code 0, output `PASS: player item, mount and runtime derived query`.

### C17 PlayerEyeAnimationState

- Proposed types: `PlayerEyeAnimationComponent` and `PlayerEyeAnimationSystem`, now saved as an
  isolated implementation;
  `TimeToActDamaged` is a named behavior constant, not mutable component state, and the eye-frame
  output is a presentation projection.
- Ownership: the system owns `_state` and `_timeInState`; it consumes blindness, sleep, health,
  tipsy/poison, storm, wall, and item-animation facts. `BlinkBecausePlayerGotHurt` is an explicit
  event/command seam from combat and must not be simulated by a renderer.
- Lifecycle: initialize normal state, update once per player tick, reset/invalidate on spawn and
  removal through explicit system methods, and retain the 20-tick hurt hold and sleep-time
  assignment semantics. The actual legacy Player spawn/removal call sites remain an integration
  seam and are not claimed by this component.
- Seam: `EyeInputSnapshot` plus `PlayerHurtPresentationEvent` in, `EyeFrameToShow` in an immutable
  output snapshot; no global `Main` lookup in the system except through an injected weather input.
- Evidence: `PlayerEyeHelper.cs:24-30,32-77,79-153`, update call `Player.cs:15728`, hurt trigger
  `Player.cs:22349`; this confirms a state machine, while complete lifecycle and client ownership
  remain partial.
- Verifier: deterministic state transition table, 20-tick hurt hold, sleep timer override, storm
  suppression behind back wall, blindness precedence, output-only projection, and spawn/removal
  reset isolation.

### C18 PlayerPresentationMessagesAndArms

- Types: arm data remains in the C03 presentation snapshot; `PlayerOverheadMessageInput`,
  `PlayerOverheadMessageSnapshot`, and `PlayerOverheadMessageProjection` are saved as an
  `implemented-isolated` projection seam. `PlayerOverheadMessageStateComponent` and
  `PlayerOverheadMessageStateSystem` now provide the isolated short-lived message state owner.
- Ownership: arm fields are written by the presentation/pose owner. `chatText`, `snippets`, size,
  color, and `timeLeft` are message lifecycle/output data; chat parsing and rendering stay adapters.
  This does not own chat protocol or player gameplay state.
- Lifecycle: create/replace message through the state system, decrement expiry in the explicit
  presentation tick, publish an immutable output, and clear at expiry/spawn/removal. The state
  owner does not parse chat or claim the external chat protocol.
- Seam: `CompositeArmSnapshot` and `OverheadMessageSnapshot` to client projection; `TextSnippet[]`
  must be copied or made immutable so an output cannot mutate authority.
- Evidence: nested declarations `Player.cs:226-230,457-465`, expiry mutation `Player.cs:14966-14969`,
  and arm write `Player.cs:3352`; protocol, reader, writer, and rendering lifecycle are partial.
- Verifier: arm value-copy behavior, message replacement/expiry, snippet ownership, size/color output,
  replacement isolation, one-way chat projection, presentation tick decrement, and spawn/removal
  cleanup. The real chat protocol remains an integration evidence gap.

## 9. Checkpoint Audit

The table below is the consolidated current audit. Older checkpoint sections retain the state that
was true when each checkpoint was written; their historical `not-run` labels are superseded by the
verification records in checkpoints 29, 30, 32, and 34.

## 10. Implementation Checkpoint (2026-09-12)

- C17 source files added under `src/Player/Animation/`:
  `PlayerEyeAnimationState.cs`, `PlayerEyeAnimationComponent.cs`,
  `PlayerEyeAnimationInput.cs`, `PlayerEyeHurtPresentationEvent.cs`,
  `PlayerEyeAnimationSnapshot.cs`, and `PlayerEyeAnimationSystem.cs`.
- The implementation owns only explicit eye state, state time, and frame output. It accepts an
  explicit input snapshot and hurt event; it does not read `Main`, Player, renderer, network, or
  persistence state and does not register or replace the legacy helper.
- Core behavior encoded from `PlayerEyeHelper.cs`: blindness precedence, 20-tick hurt hold,
  sleep-time assignment, moderate-damage/tipsy/poison/storm precedence, back-wall storm
  suppression, and deterministic frame calculations.
- The isolated implementation status is `completed` for C01-C18. C17 is in
  `completedComponents` after the affected-project build and focused smoke checks passed; it
  remains integration-limited until a legacy owner is available.
- Dependency impact: new files are isolated to `Terraria.Player.Animation`; no existing source,
  project file, registration key, public legacy API, or cross-domain owner was changed.
- Rollback: remove only the six new C17 files if the focused verifier or integration review rejects
  the state precedence, timer semantics, or output boundary.
- Verification evidence:
  - Build command: `& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\\src\\Player\\Terraria.Player.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')`.
  - Result: exit code `0`; `0` warnings; `0` errors; output
    `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` exists.
  - Focused smoke: PowerShell loaded that DLL and passed normal blinking, the 20-tick hurt hold,
    blindness precedence, sleep timer assignment, and back-wall storm suppression; exit code `0`.

This checkpoint was saved after the C17 source edit and focused verification. C17 verification is
partial for the isolated seam; legacy integration, network, persistence, and runtime scheduling
remain unverified, and the listed gaps and decisions remain active.

## 35. Implementation Checkpoint (2026-09-12T01:44:46Z)

- C17 lifecycle reset source is now saved under src/Player/Animation/:
  PlayerEyeAnimationComponent.cs and PlayerEyeAnimationSystem.cs. The focused verifier was
  extended in src/PlayerEyeAnimationVerification/Program.cs.
- Core behavior: ResetForSpawn and ResetForRemoval both clear the short-lived eye state through
  the component owner, restoring NormalBlinking, TimeInState = 0, and EyeFrameToShow = 0. The
  verifier establishes a hurt-blink state before each reset and confirms that neither spawn nor
  removal leaks the hurt state, timer, or closed-eye frame.
- Dependency impact: the change remains inside Terraria.Player.Animation and its focused verifier.
  No legacy Player, PlayerEyeHelper, renderer, network, persistence, project registration, or
  cross-partition source was changed. The system does not infer or claim the actual legacy
  spawn/removal call-site wiring.
- TDD evidence: before the production edit, the lifecycle verifier build failed as expected with
  two CS1061 errors because PlayerEyeAnimationSystem had no ResetForSpawn or ResetForRemoval.
  After the minimal production edit, the serial build returned exit code 0 with 0 warnings and
  0 errors.
- Focused verification: the serial wrapper run with --no-build --no-restore returned exit code 0
  and emitted PASS: player eye animation state, precedence, hurt hold, frame projection, and
  lifecycle reset.
- Verification status: partial. The isolated lifecycle seam is verified; actual legacy
  spawn/removal wiring, network ownership, scheduler ordering, and full behavior equivalence
  remain evidence gaps.

| checkpoint | completed/current transition | evidence-gap | blocking-decision | verificationStatus |
|---|---|---|---|---|
| C01 | source saved -> C02 | pose readers/writers, mobility owner, scheduler, and legacy equivalence incomplete | pose owner and reset scheduler | partial |
| C02 | source saved -> C03 | camera/network packet ownership, local/remote selection, and reset parity incomplete | net offset and camera authority | partial |
| C03 | source saved -> C04 | shadow/arm readers, advanced-shadow adapter, and renderer schedule incomplete | fixed arrays and arm owner | partial |
| C04 | source saved -> C05 | shader/audio/fishing consumers, invalidation, and client/server ownership incomplete | client projection versus authority | partial |
| C05 | source saved -> C06 | football event/replication ownership and legacy transition parity incomplete | item/event source owner | partial |
| C06 | source saved -> C07 | Version4 snapshot serialization is empty in the inspected source; persistence/network policy incomplete | appearance persistence/network policy | partial |
| C07 | completed -> C08 | traversal color writer, invalidation, and registry mapping incomplete | mount/mobility color owner | partial |
| C08 | completed -> C09 | companion/effect lifecycle, invalidation, and client ownership incomplete | effect projection boundary | partial |
| C09 | completed -> C10 | geometry authority, cache invalidation, and setter compatibility incomplete | spatial snapshot authority | partial |
| C10 | completed -> C11 | `Male` setter and skin variant owner incomplete | command versus query compatibility | partial |
| C11 | source saved -> C12 | zone1/zone2 writers, invalidation, and world scheduler incomplete | shared zone-state owner | partial |
| C12 | source saved -> C13 | non-contiguous zone3 mapping, weather/depth writers, and invalidation incomplete | shared zone-state owner | partial |
| C13 | source saved -> C14 | zone4/zone5 writers, shimmer/world-surface ownership, and event reset incomplete | event/liquid integration owner | partial |
| C14 | source saved -> C15 | inventory bounds, mount/camera/vault owners, and input lifecycle incomplete | inventory/storage/input owners | partial |
| C15 | completed -> C16 | mixed ability writers, zero-denominator behavior, talkNPC lifecycle, and order incomplete | cross-domain query facade | partial |
| C16 | completed -> C17 | mount delegates, scene metric authority, and nullable output parity incomplete | composed query inputs | partial |
| C17 | source saved -> focused verifier passed | eye lifecycle/network ownership incomplete | combat event and eye system remain outside this isolated seam | partial |
| C18 | source saved -> none | rich-text message protocol, tick/cleanup owner, and renderer integration incomplete | chat/output lifecycle owner | partial |

## 11. Implementation Checkpoint (2026-09-11T17:56:15Z)

- C01 source files saved under `src/Player/`: `IPlayerPoseInputSnapshot.cs`,
  `PlayerPoseInputSnapshot.cs`, `PlayerPoseAndAnimationStateComponent.cs`,
  `PlayerPoseAndAnimationSystem.cs`, and `PlayerPoseSnapshot.cs`.
- Core behavior implemented: explicit input boundary, three segment position updates, rotation
  updates from horizontal segment velocity, vertical acceleration, horizontal damping, and
  spawn/teleport pose reset. `PlayerPoseSnapshot` is a value output and cannot write the component.
- Dependency impact: the new files only depend on `System.Numerics` and the existing
  `SimulationTick` value type. No old Player field, registration key, renderer, network, save, or
  cross-partition source was changed.
- Evidence gap: the Version4 call graph has multiple writers for `gfxOffY`, `stepSpeed`, and
  `fartKartCloudDelay`; this implementation accepts those values only through the explicit input
  seam and does not invent their mobility or mount owner. Full legacy reader/writer inventory,
  scheduler integration, and behavior equivalence remain unverified.
- Verification status: `partial` for the isolated source; C01 build and focused reset/frame smoke
  verification are pending and must be recorded before this component can be called fully verified.
- Rollback: remove only the five C01 source files if the focused verifier rejects progression or
  reset semantics; retain legacy fields and writers until the integration owner is confirmed.

## 12. Implementation Checkpoint (2026-09-11T18:04:00Z)

- C02 source files saved under `src/Player/`: `PlayerNetworkCameraStateComponent.cs`,
  `PlayerNetworkCameraInputSnapshot.cs`, `PlayerCameraSnapshot.cs`,
  `PlayerNetworkCameraStateSystem.cs`, and `Presentation/PlayerNetworkCameraProjection.cs`.
- Core behavior implemented: explicit net/camera input, Version4-compatible minimum offset
  threshold and fractional decay, collision-difference adjustment input, fake-offset override,
  target synchronization marker, and spawn/teleport reset.
- Dependency impact: no legacy `Player`, `Main`, `NetMessage`, renderer, persistence, project file,
  registration key, or other partition source was changed. Network packet encoding and local-player
  selection remain outside this projection.
- Evidence gap: collision adapter semantics, packet readers/writers, local/remote selection,
  scheduler ordering, and full behavior-equivalence verification remain unverified.
- Verification status: `partial` for the isolated source; C02 build and focused decay/reset smoke
  verification are pending.

## 13. Implementation Checkpoint (2026-09-11T18:12:00Z)

- C03 source files saved under `src/Player/Presentation/`: `PlayerArmStretchAmount.cs`,
  `PlayerCompositeArmSnapshot.cs`, `PlayerAdvancedShadowSlot.cs`, `PlayerShadowSnapshot.cs`,
  `PlayerShadowAndArmPresentationInput.cs`, `PlayerShadowAndArmPresentationComponent.cs`, and
  `PlayerShadowAndArmPresentationSystem.cs`.
- Core behavior: fixed three-slot social-shadow sampling, fixed sixty-slot advanced-shadow ring,
  count cap, arm value input, cursor/sound/frame flags, reset seam, and copied snapshot output.
- Dependency impact: no legacy Player aggregate, renderer, audio, UI, `EntityShadowInfo`, project
  registration, network, persistence, or other partition source was changed.
- Evidence gap: full shadow readers, advanced-shadow adapter conversion, renderer/audio schedule,
  and legacy arm rotation call-site parity remain unverified.
- Verification status: `partial` for the isolated source; C03 build and focused array/ring/arm
  immutability smoke verification are pending.

## 14. Implementation Checkpoint (2026-09-11T18:20:00Z)

- C04 source files saved under `src/Player/Presentation/`: `PlayerVisualAndShaderEffectsInput.cs`,
  `PlayerVisualAndShaderEffectsSnapshot.cs`, and `PlayerVisualAndShaderEffectsProjection.cs`.
- Core behavior: one-way, immutable projection of all 21 `PlayerVisualAndShaderEffects` fields;
  shader/halo/cursor/audio/fishing consumers receive values without a reverse write path.
- Dependency impact: no renderer, shader registry, audio, fishing, network, persistence, legacy
  Player aggregate, project registration, or other partition source was changed.
- Evidence gap: source invalidation, client/server split, shader registry conversion, audio owner,
  fishing owner, and legacy consumer parity remain unverified.
- Verification status: `partial` for the isolated source; C04 build and focused projection
  immutability verification are pending.

## 15. Implementation Checkpoint (2026-09-11T18:28:00Z)

- C05 source files saved under `src/Player/Presentation/`: `PlayerFootballPresentationInput.cs`,
  `PlayerFootballPresentationStateComponent.cs`, `PlayerFootballPresentationStateSystem.cs`,
  `PlayerFootballPresentationSnapshot.cs`, and `PlayerFootballPresentationProjection.cs`.
- Core behavior: possession state, draw eligibility, animation suppression, item-loss/spawn reset,
  and immutable draw output.
- Dependency impact: no inventory/equipment owner, renderer, network, persistence, project
  registration, legacy Player API, or other partition source was changed.
- Evidence gap: football item/event readers, duplicate-event semantics, network replication, renderer
  schedule, and legacy draw-call parity remain unverified.
- Verification status: `partial` for the isolated source; C05 build and focused transition/reset
  smoke verification are pending.

## 16. Implementation Checkpoint (2026-09-11T18:36:00Z)

- C06 source files saved under `src/Player/`: `PlayerAppearanceColor.cs`,
  `PlayerAppearanceCustomizationInput.cs`, `PlayerAppearanceCustomizationComponent.cs`,
  `PlayerAppearanceCustomizationSystem.cs`, and `PlayerAppearanceCustomizationSnapshot.cs`.
- Core behavior: explicit in-memory appearance update and immutable snapshot covering hair, packed
  skin dye, hair/skin/eye/shirt/under-shirt/pants/shoe colors, and hair style; Version4 default
  colors are preserved.
- Dependency impact: no save-file, packet, renderer, identity, equipment, project registration,
  legacy Player API, or cross-partition source was changed.
- Evidence gap: input range validation, spawn/clone behavior, serialization round-trip, network
  compatibility, and renderer parity remain unverified.
- Verification status: `partial` for the isolated source; C06 build and focused value/snapshot
  verification are pending.

## 19. Implementation Checkpoint (2026-09-12T03:30:00Z)

- C10 source files saved under `src/Player/`: `PlayerIdentityAndDerivedPropertiesInput.cs`,
  `PlayerIdentityAndDerivedPropertiesSnapshot.cs`, `PlayerMaleChangeCommand.cs`, and
  `PlayerIdentityAndDerivedPropertiesQuery.cs`.
- Core behavior: pure `miscCounterNormalized` calculation, explicit current-gender/skin-variant
  snapshot, and idempotent male-change command intent matching the inspected Version4 setter branch.
  The query never changes `skinVariant`; a command consumer must apply the returned variant.
- Dependency impact: no PlayerVariantID catalog, appearance owner, persistence/network adapter,
  renderer, project registration, legacy Player API, or cross-partition source was changed.
- Evidence gap: catalog value mapping, all setter call sites, appearance command owner, persistence,
  network compatibility, and behavior-equivalence remain unverified.
- Verification status: `not-run` for the C10 affected-project build and focused query/command smoke;
  source checkpoint is saved and C11 must not begin until this checkpoint is persisted.

## 18. Implementation Checkpoint (2026-09-12T03:24:00Z)

- C09 source files saved under `src/Player/`: `PlayerSpatialSnapshot.cs`, `PlayerSpatialHitbox.cs`,
  and `PlayerSpatialDerivedPropertiesQuery.cs`.
- Core behavior: pure calculations for `BlehOldPositionFixer`, `HeightOffsetHitboxCenter`,
  `HeightOffsetBoost`, `HitboxForBestiaryNearbyCheck`, `IsConsideredStandingStill`, `BaseHeight`,
  `MountedCenter`, `VisualPosition`, and `CCed`; `MountedCenter` is read-only in this query seam.
- Dependency impact: no mount, portable-stool, movement, status, world-global, renderer, legacy
  Player API, project registration, or cross-partition source was changed. No cache or setter command
  was introduced.
- Evidence gap: complete geometry snapshot ownership, mount/stool input authority, all call sites,
  setter compatibility, and edge-case behavior equivalence remain unverified.
- Verification status: `not-run` for the C09 affected-project build and focused query verifier; the
  source checkpoint is saved and the result must be verified before integration claims.

## 17. Implementation Checkpoint (2026-09-12T03:18:00Z)

- C08 source files saved under `src/Player/Presentation/`:
  `PlayerAppearanceCompanionAndEffectInput.cs`,
  `PlayerAppearanceCompanionAndEffectSnapshot.cs`, and
  `PlayerAppearanceCompanionAndEffectProjection.cs`.
- Core behavior: one-way value projection of `cPet`, `cLight`, `cYorai`, `cPortableStool`,
  `cUnicornHorn`, `cAngelHalo`, `cBeard`, `cMinion`, `cLeinShampoo`, `cFlameWaker`, and `cCoat`.
- Dependency impact: no equipment/buff/pet owner, content catalog, renderer, effect service,
  network, persistence, project registration, legacy Player API, or cross-partition source was
  changed.
- Evidence gap: source writers, invalidation and despawn ordering, duplicate-event handling,
  client/server ownership, renderer parity, and scheduler integration remain unverified.
- Verification status: `partial` for the isolated source; affected-project build and focused
  projection verification are pending before the result can be claimed beyond source saved.

## 18. Implementation Checkpoint (2026-09-11T18:43:00Z)

- C11 source files saved under `src/Player/`: `PlayerZoneSnapshot.cs`,
  `PlayerBiomeZonePropertiesSnapshot.cs`, `PlayerBiomeZonePropertiesQuery.cs`, `PlayerZoneFlag.cs`,
  and `PlayerZoneFlagChangeCommand.cs`.
- Core behavior: explicit immutable 16-slot zone input and pure output mapping for every C11 report
  property; zone changes are represented as a command intent and do not write `zone1` or `zone2`.
- Dependency impact: no world/environment authority, zone-array owner, cache, persistence/network
  path, legacy Player API, project registration, or cross-partition source was changed.
- Evidence gap: complete zone writer inventory, lifecycle invalidation, setter compatibility,
  world scheduler order, affected-project build, and focused repeated-query/mapping verification
  remain unverified.
- Verification status: `not-run` for C11; source is saved and both documents are synchronized before
  starting C12.

## 19. Implementation Checkpoint (2026-09-11T18:52:00Z)

- C12 source files saved under `src/Player/`: `PlayerVerticalAndWeatherZoneInput.cs`,
  `PlayerVerticalAndWeatherZonePropertiesSnapshot.cs`,
  `PlayerVerticalAndWeatherZonePropertiesQuery.cs`, `PlayerVerticalAndWeatherZoneFlag.cs`, and
  `PlayerVerticalAndWeatherZoneFlagChangeCommand.cs`.
- Core behavior: explicit immutable six-slot weather/height input and pure output mapping for
  `ZoneSkyHeight`, `ZoneOverworldHeight`, `ZoneUnderworldHeight`, `ZoneBeach`, `ZoneRain`, and
  `ZoneSandstorm`, preserving Version4's `zone3` indices `0, 1, 4, 5, 6, 7`. The command is an
  explicit write intent and does not mutate world weather or zone arrays.
- Dependency impact: no world/weather owner, zone-array state, cache, persistence/network path,
  legacy Player API, project registration, or cross-partition source was changed.
- Evidence gap: non-contiguous slot mapping, threshold behavior, complete writer/lifecycle inventory,
  invalidation, scheduler order, affected-project build, and focused query verification remain
  unverified.
- Verification status: `not-run` for C12; source is saved and both documents are synchronized before
  starting C13.

## 20. Implementation Checkpoint (2026-09-11T19:02:00Z)

- C13 source files saved under `src/Player/`: `PlayerEventAndShoppingZoneInput.cs`,
  `PlayerEventAndShoppingZonePropertiesSnapshot.cs`, `PlayerEventAndShoppingZonePropertiesQuery.cs`,
  `PlayerEventZoneFlag.cs`, and `PlayerEventZoneFlagChangeCommand.cs`.
- Core behavior: explicit event/biome/position/world-surface input; pure projection of the five
  `zone4`/`zone5` event slots, Version4-compatible `ShoppingZone_AnyBiome` truth calculation, and
  `ShoppingZone_BelowSurface` threshold calculation. Event and shimmer changes are command intents
  only and do not mutate world state.
- Dependency impact: no world-event, shimmer/liquid, zone-array, persistence/network, renderer,
  legacy Player API, project registration, or cross-partition source was changed.
- Evidence gap: complete zone4/zone5 writers, event reset ordering, shimmer ownership, world-surface
  authority, invalidation, affected-project build, and focused truth-table verification remain
  unverified.
- Verification status: `not-run` for C13; source is saved and both documents are synchronized before
  starting C14.

## 21. Implementation Checkpoint (2026-09-11T19:12:00Z)

- C14 source files saved under `src/Player/`: `PlayerInteractionAndSelectionPropertiesInput.cs`,
  `PlayerInteractionAndSelectionPropertiesSnapshot.cs`,
  `PlayerInteractionAndSelectionPropertiesQuery.cs`, and `PlayerVoidVaultStateChangeCommand.cs`.
- Core behavior: explicit pure calculations for directions, selected item, held-item identity,
  floating/talk/hover predicates, void-vault output, reported camera fallback, and bounded inventory
  reads. Invalid selected slots return `ItemEntityRef.None`; no `Item` object or global camera state is
  accessed.
- Dependency impact: no inventory/storage owner, mount owner, camera/network adapter, vault writer,
  renderer, persistence, legacy Player API, project registration, or cross-partition source changed.
- Evidence gap: complete selection bounds and call-site semantics, mount type authority, vault writer,
  camera target lifecycle, invalidation, affected-project build, and focused interaction truth-table
  verification remain unverified.
- Verification status: `not-run` for C14; source is saved and both documents are synchronized before
  starting C15.

## 22. Implementation Checkpoint (2026-09-11T20:58:26Z)

- C18 source files saved under `src/Player/Presentation/`:
  `PlayerOverheadMessageInput.cs`, `PlayerOverheadMessageSnapshot.cs`, and
  `PlayerOverheadMessageProjection.cs`. The existing C03
  `PlayerCompositeArmSnapshot` remains the shared arm value boundary; no duplicate arm authority
  was introduced.
- Core behavior: the projection carries the two arm snapshots, chat text, parsed snippet strings,
  message size, expiry ticks, and appearance color into an immutable output. The snapshot copies
  the snippet sequence into a read-only collection, preserves replacement values per projection,
  and exposes `IsVisible` only when `TimeLeft > 0`. It does not decrement time, parse chat input,
  mutate arm authority, or write any player state.
- Focused verifier files saved under `src/PlayerPresentationMessageVerification/`:
  `Terraria.PlayerPresentationMessageVerification.csproj` and `Program.cs`. The verifier covers
  arm value equality, all message values, snippet order and non-aliasing, expiry visibility, and
  replacement isolation.
- Dependency impact: the projection depends on `System.Numerics`, the existing Player presentation
  value types, and `PlayerAppearanceColor`; it does not add a Content or renderer project
  reference, protocol encoder, chat parser, persistence/network path, registration key, legacy
  Player API, or cross-partition source change.
- Verification evidence:
  - Build command: `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\src\PlayerPresentationMessageVerification\Terraria.PlayerPresentationMessageVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`.
  - Build result: exit code `0`, `0` warnings, `0` errors. Confirmed artifacts are
    `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` and
    `Build/bin/Terraria.PlayerPresentationMessageVerification/Debug/net10.0/Terraria.PlayerPresentationMessageVerification.dll`.
  - Focused command: `& '.\Build\bin\Terraria.PlayerPresentationMessageVerification\Debug\net10.0\Terraria.PlayerPresentationMessageVerification.exe'; $code=$LASTEXITCODE; 'EXIT_CODE=' + $code; exit $code`.
  - Focused result: exit code `0`; output `PASS: player overhead message and arm projection`.
  - A new `dotnet run` was not started because unrelated orphan `/nodeReuse:true` MSBuild nodes
    remained active in the shared checkout; the already-built verifier executable was run directly
    without touching `Build/obj`.
- Evidence gap: Version4 `TextSnippet` rich-text semantics, chat/network adapter ownership,
  message tick decrement and spawn/removal cleanup owner, renderer integration, legacy output
  parity, and scheduler ordering remain unverified. C18 is complete only as an isolated projection
  implementation checkpoint.
- Verification status: `partial`; C18 source and focused verifier evidence are present, while
  cross-domain integration remains open.

## 23. Implementation Checkpoint (2026-09-11T21:09:38Z)

- C18 lifecycle source is now saved under `src/Player/Presentation/`:
  `PlayerOverheadMessageStateComponent.cs`, `PlayerOverheadMessageStateSystem.cs`, and the
  state-aware overload in `PlayerOverheadMessageProjection.cs`. The existing arm snapshot and
  message input/snapshot remain the public value boundaries.
- Core behavior: `ReplaceMessage` copies the message text and snippet sequence into the
  component; `AdvancePresentationTick` decrements positive `timeLeft` once per explicit tick and
  leaves an expired value at zero; `ResetForSpawn` and `ResetForRemoval` clear all short-lived
  message fields. Projection reads the component and emits the existing immutable snapshot without
  writing state.
- Focused verifier source was extended in
  `src/PlayerPresentationMessageVerification/Program.cs` to cover state replacement, defensive
  snippet ownership, tick decrement, expiry, spawn reset, and removal reset. The project file was
  not changed.
- Dependency impact: the lifecycle seam depends only on the existing Player presentation value
  types, `System.Numerics`, and BCL collection copying. It does not add a chat parser, network or
  persistence adapter, renderer dependency, registration key, legacy Player API, or cross-partition
  source change.
- Verification status at this checkpoint: `partial`; the prior projection evidence remained
  recorded, but the lifecycle source still required a fresh serial build and focused run at that
  time. The later 21:24:42Z checkpoint records the fresh build and focused result.
- Evidence gap and blocking decision: Version4 `TextSnippet` rich-text semantics, external chat
  protocol ownership, renderer integration, legacy output parity, and scheduler integration remain
  unverified. The in-memory lifecycle owner is deliberately isolated until those owners are
  confirmed; no protocol or renderer behavior is inferred.

## 24. Implementation Checkpoint (2026-09-11T21:24:42Z)

- The C18 lifecycle checkpoint from 21:09:38Z was rebuilt and focused-verified after the verifier
  input-alias assertion was corrected. The correction restores the original snippet input before
  `ReplaceMessage`, then mutates the caller-owned list after replacement to test state ownership;
  production C18 behavior was not changed by this test correction.
- Verification evidence:
  - Before the correction, the new verifier failed at `Program.cs:42` with
    `The state must retain snippet order.` The failure was caused by the verifier mutating the
    shared input list before using it for state replacement, not by a production assertion or
    compiler error.
  - Serial build command:
    `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\src\PlayerPresentationMessageVerification\Terraria.PlayerPresentationMessageVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); exit $LASTEXITCODE"`.
  - Build result: exit code `0`, `0` warnings, `0` errors. Artifacts confirmed under
    `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` and
    `Build/bin/Terraria.PlayerPresentationMessageVerification/Debug/net10.0/`.
  - Focused command:
    `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('run', '--project', '.\src\PlayerPresentationMessageVerification\Terraria.PlayerPresentationMessageVerification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); exit $LASTEXITCODE"`.
  - Focused result: exit code `0`; output `PASS: player overhead message state and arm projection`.
- Current C18 evidence now covers arm value-copy, message value preservation, replacement,
  defensive snippet ownership at projection and state boundaries, one-tick decrement, zero expiry,
  and spawn/removal cleanup. Rich-text `TextSnippet` protocol fidelity, external chat/network
  adapter ownership, renderer integration, legacy output parity, and scheduler integration remain
  evidence gaps.
- Verification status: `partial`; the isolated C18 implementation and focused verifier are green,
  but cross-domain integration is intentionally not claimed.

## 25. Verification Checkpoint (2026-09-11T21:41:57Z)

- C01 focused verifier source is saved under `src/PlayerPoseVerification/`:
  `Program.cs` and `Terraria.PlayerPoseVerification.csproj`.
- The verifier covers the confirmed Version4 pose behavior: one-frame segment position and
  rotation updates, horizontal damping, vertical acceleration, explicit early-return behavior,
  and spawn/teleport pose reset. It treats `stepSpeed`, `gfxOffY`, and
  `fartKartCloudDelay` as explicit input seams and does not claim their external writers.
- Verification status: `not-run` for the new C01 verifier until the required serial build and
  focused run complete. C01's external owner, scheduler ordering, and legacy equivalence remain
  evidence gaps.

## 26. Verification Checkpoint (2026-09-11T21:49:00Z)

- C01 focused verifier build command:
  `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\src\PlayerPoseVerification\Terraria.PlayerPoseVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); exit $LASTEXITCODE"`.
- Build result: exit code `0`, `0` warnings, `0` errors. Confirmed artifacts:
  `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` and
  `Build/bin/Terraria.PlayerPoseVerification/Debug/net10.0/Terraria.PlayerPoseVerification.dll`.
- C01 focused run command:
  `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('run', '--project', '.\src\PlayerPoseVerification\Terraria.PlayerPoseVerification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); exit $LASTEXITCODE"`.
- Focused result: exit code `0`; output `PASS: player pose progression and reset`.
- C01 verification is green for the isolated pose seam. External writers, scheduler ordering,
  legacy behavior equivalence, and cross-domain integration remain explicitly unverified.

## 27. Verification Checkpoint (2026-09-11T21:52:20Z)

- The C02-C15 focused verifier source is saved under `src/PlayerPresentationDerivedVerification/`:
  `Program.cs` and `Terraria.PlayerPresentationDerivedVerification.csproj`.
- The verifier covers C02 offset decay/target reset, C03 bounded shadow-ring and arm snapshots,
  C04/C07/C08 immutable value projections, C05 football transitions, C06 customization defaults
  and commit, C09 spatial threshold/precedence calculations, C10 normalization and command shape,
  C11-C13 slot and shopping mappings, C14 selection/camera/hover truth tables, and C15 formulas
  and ability/draw gates. It does not claim external owner, persistence, network, renderer, or
  scheduler behavior.
- Actual modified source files: `src/PlayerPresentationDerivedVerification/Program.cs` and
  `src/PlayerPresentationDerivedVerification/Terraria.PlayerPresentationDerivedVerification.csproj`.
- Dependency impact: the verifier references only `Terraria.Player`; it does not modify production
  state, register a runtime system, add a legacy API, or change any P11 authority boundary.
- Verification status: `partial`; the verifier source is saved and the serial build/focused run are
  pending. C02-C15 behavior is not claimed green until those commands produce evidence.

## 28. Verification Checkpoint (2026-09-11T21:59:45Z)

- C02's focused verifier now also projects a `PlayerCameraSnapshot`, changes the component, and
  asserts that the prior projection retains its offset value. This directly checks the one-way
  projection boundary without introducing packet or local-player semantics.
- The modified verifier file is `src/PlayerPresentationDerivedVerification/Program.cs`.
- Verification status remains `partial`; the C02-C15 verifier still requires the serial build and
  focused run. Duplicate packet rejection and local/remote selection remain evidence gaps because
  the current boundary has no protocol or player-registry authority.

## 29. Verification Checkpoint (2026-09-11T22:05:30Z)

- The current C02-C15 focused verifier, including the C02 projection immutability assertion, was
  serially built through Build/Tools/Invoke-SerialDotnet.ps1:
  pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\src\PlayerPresentationDerivedVerification\Terraria.PlayerPresentationDerivedVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); exit $LASTEXITCODE"
- Build result: exit code 0, 0 warnings, 0 errors. Confirmed artifacts are under
  Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll and
  Build/bin/Terraria.PlayerPresentationDerivedVerification/Debug/net10.0/Terraria.PlayerPresentationDerivedVerification.dll.
- Focused run used the serial wrapper with --no-build --no-restore and returned exit code 0:
  PASS: P11 C01-C15 focused component verification.
- C02-C15 isolated behavior evidence is green for the verifier's declared scope. External owner,
  renderer, persistence/network, scheduler, and legacy-equivalence gaps remain explicit.
- completedComponents, currentComponent, and pendingComponents remain unchanged because all C01-C18
  source checkpoints were already saved; verificationStatus remains partial.

## 30. Verification Checkpoint (2026-09-11T22:11:01Z)

- The existing C16 focused verifier was run through Build/Tools/Invoke-SerialDotnet.ps1 with
  --no-build --no-restore and returned exit code 0:
  PASS: player item, mount and runtime derived query.
- The existing C18 focused verifier was run through Build/Tools/Invoke-SerialDotnet.ps1 with
  --no-build --no-restore and returned exit code 0:
  PASS: player overhead message state and arm projection.
- These runs used the current Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll.
  They confirm isolated C16/C18 verifier behavior only; external owner, scheduler, renderer,
  protocol, and legacy-equivalence gaps remain unchanged.
- completedComponents, currentComponent, pendingComponents, and implementationStatus remain
  unchanged; verificationStatus remains partial.

## 31. Verification Checkpoint (2026-09-11T22:17:20Z)

- The C17 focused verifier source is saved under src/PlayerEyeAnimationVerification/:
  Program.cs and Terraria.PlayerEyeAnimationVerification.csproj.
- It covers normal blinking phase boundaries, the twenty-tick hurt hold, blindness and status
  precedence, sleep timer/frame thresholds, storm exposure, and back-wall suppression. It uses
  only explicit Terraria.Player.Animation inputs and does not claim legacy helper ownership.
- Actual modified source files: src/PlayerEyeAnimationVerification/Program.cs and
  src/PlayerEyeAnimationVerification/Terraria.PlayerEyeAnimationVerification.csproj.
- Verification status: partial; the verifier source is saved and its required serial build/run
  are pending. completedComponents, currentComponent, and pendingComponents remain unchanged
  because C17 source was already saved; lifecycle reset, legacy integration, and scheduler
  behavior remain evidence gaps.

## 32. Verification Checkpoint (2026-09-11T22:20:29Z)

- The C17 focused verifier was serially built through Build/Tools/Invoke-SerialDotnet.ps1:
  pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\src\PlayerEyeAnimationVerification\Terraria.PlayerEyeAnimationVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); exit $LASTEXITCODE"
- Build result: exit code 0, 0 warnings, 0 errors. Artifact confirmed under
  Build/bin/Terraria.PlayerEyeAnimationVerification/Debug/net10.0/Terraria.PlayerEyeAnimationVerification.dll;
  the referenced Player artifact is under Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll.
- Focused run used the serial wrapper with --no-build --no-restore and returned exit code 0:
  PASS: player eye animation state, precedence, hurt hold, and frame projection.
- C17 isolated state-machine evidence is green for normal blinking, hurt hold, precedence, sleep
  frames, storm exposure, and back-wall suppression. Version4 lifecycle reset ownership, legacy
  helper integration, and scheduler behavior remain evidence gaps.
- completedComponents, currentComponent, and pendingComponents remain unchanged; implementationStatus
  remains completed and verificationStatus remains partial.

## 33. Verification Checkpoint (2026-09-11T22:24:32Z)

- The P11 authority report was audited against the member-row contract:
  REPORT_ROWS=164, FIELDS=91, PROPERTIES=73, TOTAL=164, ID_MIN=370,
  ID_MAX=1498, UNIQUE_IDS=164, DUPLICATE_ID_GROUPS=0, and
  UNIQUE_MEMBER_NAMES=164.
- The C01-C18 member table in this design was independently counted at 18 rows and 164 member
  tokens, matching the authority report. No field/property row is missing from the P11 report or
  the P11 design grouping.
- The repository-wide statistics remain Terraria/Player.cs = 1106 and
  Version4 retained fields/properties = 7201 according to the read-only total-statistics
  Markdown. Those numbers are a whole-repository baseline, not an additional P11 count.
- docs/migration/ledgers/Version4-component-target-index.json contains zero P11 type-name hits.
  This is a canonical target-index registration/integration gap; it is not evidence that the
  P11 authority report omitted a member. The index and total-statistics documents were not edited.
- C17 focused verifier evidence is already recorded in checkpoint 32; the current verifier passed
  with exit code 0 and the stated isolated state-machine scope.

## 34. Verification Checkpoint (2026-09-11T22:37:09Z)

- The five P11 focused verifier projects were rebuilt serially after the required process check
  found no active `dotnet.exe` or `csc.exe` owner in the shared checkout. Each build used
  `Build/Tools/Invoke-SerialDotnet.ps1` with `-m:1`, `-nr:false`,
  `UseSharedCompilation=false`, `MSBuildNodeReuse=false`, and `BuildInParallel=false`.
- Build results were exit code `0`, `0` warnings, and `0` errors for every project. Artifacts were
  confirmed under `Build/bin/` for `Terraria.PlayerPoseVerification`,
  `Terraria.PlayerPresentationDerivedVerification`, `Terraria.PlayerItemMountRuntimeVerification`,
  `Terraria.PlayerPresentationMessageVerification`, and `Terraria.PlayerEyeAnimationVerification`;
  each project also rebuilt the affected `Terraria.Player` project reference at
  `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll`.
- The same five projects then ran serially through the wrapper with `--no-build --no-restore`.
  Every run returned exit code `0`: C01 pose/reset, C02-C15 derived/presentation components, C16
  item/mount/runtime query, C18 overhead message/arms, and C17 eye animation all emitted their
  declared `PASS` result. The exact commands, projects, outputs, and artifact paths are recorded
  in Section 6 of the execution document.
- The consolidated member and implementation state remains:
  `completedComponents=[C17,C01,C02,C03,C04,C05,C06,C07,C08,C09,C10,C11,C12,C13,C14,C15,C16,C18]`,
  `currentComponent=none`, and `pendingComponents=[]`.
- Focused evidence is green only for the declared isolated scopes. Legacy integration, writer
  uniqueness across partitions, scheduler ordering, renderer/chat protocol parity, network and
  persistence compatibility, and cross-domain authority ownership remain unverified. The
  canonical target-index omission recorded in checkpoint 33 remains unchanged and was not edited.

## 36. Version4 Runner Checkpoint (2026-09-12T06:57:14Z)

- Runner binding for this execution is `partition=P11`, `sessionId=37c9b79cb0bf42489131c63004475fa9`,
  and the authoritative report remains
  `D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P11-Player-Presentation-Derived.md`.
- C01-C18 already have component source in `src/Player` and remain in `completedComponents`;
  `currentComponent=none` and `pendingComponents=[]`. No new `src` file was created or modified
  in this runner session. The existing component source changes were preserved as-is.
- The five existing focused verifier projects all returned exit code `0` with their declared PASS
  output: C01 pose/reset, C02-C15 presentation/derived components, C16 item/mount/runtime query,
  C18 overhead message/arms, and C17 eye animation. Their artifacts are under `Build/bin/`.
- The affected `src/Player/Terraria.Player.csproj` build was attempted through the required serial
  wrapper and returned exit code `1`, with `0` warnings and `5` errors. All five errors are outside
  P11 in existing `src/Player/Progression` code: `PlayerMinionCapacityCommitSystem.cs` is missing
  `Terraria.Relationships`, and `SubmitMinionCapacityDeltaCommand.cs` is missing
  `Terraria.Projectile`, `Terraria.Relationships`, `EntityReference`, and
  `ProjectileIdentityComponent`; the project currently lacks the required references. This task
  did not modify those files or the project file.
- No tests, systems, queries, commands, adapters, projections, events, project files, build scripts,
  or other non-component code were modified. Full Player build, cross-domain integration, legacy
  equivalence, scheduler ordering, network/persistence compatibility, and renderer/chat protocol
  parity remain unverified. `verificationStatus=partial` is therefore retained.
