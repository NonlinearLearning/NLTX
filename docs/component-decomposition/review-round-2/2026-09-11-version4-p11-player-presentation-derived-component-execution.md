# P11 Player Presentation and Derived Properties: Component Execution Plan

partitionId: P11
sessionId: 37c9b79cb0bf42489131c63004475fa9
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P11-Player-Presentation-Derived.md
designPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-p11-player-presentation-derived-component-design.md
executionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-p11-player-presentation-derived-component-execution.md
executionStatus: completed
implementationStatus: completed
verificationStatus: partial
evidenceStatus: partial
designCompletedComponents: [C01, C02, C03, C04, C05, C06, C07, C08, C09, C10, C11, C12, C13, C14, C15, C16, C17, C18]
completedComponents: [C17, C01, C02, C03, C04, C05, C06, C07, C08, C09, C10, C11, C12, C13, C14, C15, C16, C18]
currentComponent: none
pendingComponents: []
lastCheckpointUtc: 2026-09-12T06:57:14Z
evidence-gap: C01-C18 have serial focused isolated evidence for their declared verifier scopes. The P11 authority report and design table are count-complete at 91 fields + 73 properties = 164 unique rows, but P11 types are absent from docs/migration/ledgers/Version4-component-target-index.json, so canonical effective-component statistics do not yet register these isolated sources. The affected Terraria.Player project build is blocked by five pre-existing non-P11 Progression dependency errors, while all five existing P11 focused verifier projects pass. External writer/owner closure, scheduler ordering, legacy equivalence, persistence/network formats, renderer/chat protocol, geometry/world-zone/weather/event authority, variant catalog, inventory/storage ownership, C15 zero-denominator and talkNPC lifecycle, C17 integration with the actual Player spawn/removal call sites, and cross-domain integration remain unconfirmed. C17's isolated spawn/removal reset behavior is focused-verified at the component/system seam.
blocking-decision: C01-C18 remain isolated component/system/projection/query seams and are integration-blocked until legacy owners and scheduler contracts are approved; C01's `stepSpeed`, `gfxOffY`, and `fartKartCloudDelay` remain explicit input seams rather than claimed writers. C06 remains memory-only pending format evidence, C07 remains value-only pending registry evidence, C08 remains a one-way projection pending companion/effect owner evidence, C09 remains a pure snapshot query pending geometry authority evidence, C10 remains command-intent only pending appearance ownership, C11 remains a pure zone snapshot query plus deferred write command pending world-zone authority, C12 remains a pure non-contiguous `zone3` query plus deferred write command pending weather/depth authority, C13 remains a pure event/shopping query plus deferred write command pending world/event and shimmer authority, C14 remains a pure interaction query plus deferred vault command pending inventory/storage authority, C15 remains a pure composed query with explicit mount/weather inputs and no writer, C16 remains a pure runtime facade with adapter-owned scene/pose facts and no writer, C17 now has explicit isolated reset methods but actual legacy spawn/removal wiring, network ownership, and scheduler order remain outside the seam, and C18 remains an isolated projection/state seam pending rich-text protocol, chat/network adapter, renderer, scheduler, and legacy integration ownership.

## 1. Plan Boundary

This is a completed isolated implementation sequence for C01-C18. All C01-C18 source seams have
affected-project build and focused verifier evidence across the five dedicated verifier projects.
Runtime, network, persistence, scheduler, and behavior-equivalence results remain partial. All
cross-domain owners and paths remain subject to the design document and integration review.

The plan changes only future ECS organization under domain-first paths. It does not authorize edits
to Version4, the authoritative report, other session documents, or the current NLTX C# source.

## 2. Proposed File Organization

Use the existing `src/Player` domain root for small Player capabilities. Add a deeper directory only
after the boundary has independent tests or stable scale. Proposed future paths are domain-first:

| responsibility | proposed path | namespace | reason |
|---|---|---|---|
| player state | `src/Player/Player*Component.cs` | `Terraria.Player` | existing Player domain and one public type per file |
| player queries | `src/Player/Player*Query.cs` | `Terraria.Player` | queries live beside the capability they read |
| presentation projection | `src/Player/Presentation/Player*Projection.cs` | `Terraria.Player.Presentation` | stable one-way client/output boundary; create only when scale justifies it |
| animation system | `src/Player/Animation/PlayerEyeAnimationSystem.cs` | `Terraria.Player.Animation` | independent state transition and focused verifier |
| persistence/network adapters | `src/Player/Adapters/` | `Terraria.Player.Adapters` | external protocol and serialization types stay at the boundary |

No `Shared/Components/`, `Common/`, `Misc/`, or file-order scheduling is proposed.

## 3. Global Migration Invariants

- Keep old public names and namespaces behind an adapter until the focused verifier passes.
- Introduce one owner writer before removing the old write path; never dual-write authority.
- Use immutable snapshots for projection and query inputs; keep arrays and reference values bounded.
- Make scheduler dependencies explicit (facts committed before queries, queries before projections).
- Migrate one checkpoint at a time and keep each commit revertible.
- Do not migrate persistence or network fields until their Version4 formats and packet readers are
  evidenced; mark those steps blocked instead of inventing compatibility data.

## 4. Verification Commands and Results

The five affected verifier projects were built serially after checking that no active `dotnet.exe`
or `csc.exe` process owned the checkout. Every build returned exit code `0`, with `0` warnings and
`0` errors; all artifacts were written under `Build/bin/`.

The exact build invocation shape, executed once for each project below, was:

```powershell
& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @(
  'build', '<project>', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false'
)
```

Built projects and artifacts:

| project | result | artifact |
|---|---|---|
| `src/PlayerPoseVerification/Terraria.PlayerPoseVerification.csproj` | exit `0`, 0 warnings, 0 errors | `Build/bin/Terraria.PlayerPoseVerification/Debug/net10.0/Terraria.PlayerPoseVerification.dll` |
| `src/PlayerPresentationDerivedVerification/Terraria.PlayerPresentationDerivedVerification.csproj` | exit `0`, 0 warnings, 0 errors | `Build/bin/Terraria.PlayerPresentationDerivedVerification/Debug/net10.0/Terraria.PlayerPresentationDerivedVerification.dll` |
| `src/PlayerItemMountRuntimeVerification/Terraria.PlayerItemMountRuntimeVerification.csproj` | exit `0`, 0 warnings, 0 errors | `Build/bin/Terraria.PlayerItemMountRuntimeVerification/Debug/net10.0/Terraria.PlayerItemMountRuntimeVerification.dll` |
| `src/PlayerPresentationMessageVerification/Terraria.PlayerPresentationMessageVerification.csproj` | exit `0`, 0 warnings, 0 errors | `Build/bin/Terraria.PlayerPresentationMessageVerification/Debug/net10.0/Terraria.PlayerPresentationMessageVerification.dll` |
| `src/PlayerEyeAnimationVerification/Terraria.PlayerEyeAnimationVerification.csproj` | exit `0`, 0 warnings, 0 errors | `Build/bin/Terraria.PlayerEyeAnimationVerification/Debug/net10.0/Terraria.PlayerEyeAnimationVerification.dll` |

Each verifier then ran serially with the wrapper and `--no-build --no-restore`; all returned exit
code `0`:

| scope | output |
|---|---|
| C01 pose/reset | `PASS: player pose progression and reset` |
| C02-C15 presentation and derived components | `PASS: P11 C01-C15 focused component verification` |
| C16 item/mount/runtime query | `PASS: player item, mount and runtime derived query` |
| C18 overhead message and arms | `PASS: player overhead message state and arm projection` |
| C17 eye animation | `PASS: player eye animation state, precedence, hurt hold, frame projection, and lifecycle reset` |

These results prove only the isolated source boundaries covered by the verifiers. They do not prove
legacy integration, renderer/chat protocol parity, network or persistence compatibility, writer
uniqueness across partitions, or scheduler behavior.

## 5. Component Execution Checkpoints

Detailed steps are appended as each component checkpoint is completed. The metadata at the top of
this file is updated together with the design document after every checkpoint.

### C01 PlayerPoseAndAnimationState

- Proposed files: `src/Player/PlayerPoseAndAnimationStateComponent.cs` and
  `src/Player/PlayerPoseAndAnimationStateSystem.cs`, namespace `Terraria.Player`.
- Sequence: add the immutable input snapshot and component; route one legacy pose write through the
  owner system; add reset/early-return tests; switch readers to the snapshot; remove the adapter only
  after the verifier passes.
- Dependency impact: reads PlayerGameplay/PlayerMobility facts and may consume MountVehicle facts;
  it must not own movement, mount, or renderer state. `stepSpeed` and `fartKartCloudDelay` require
  integration review before file moves.
- Scheduler contract: movement facts committed -> pose system -> pose snapshot -> queries/projection.
- Rollback: restore the legacy owner and delete only the newly introduced proposed files if any
  verifier observes a changed reset, frame, or teleport invariant.

### C02 PlayerNetworkCameraState

- Proposed files: `src/Player/PlayerNetworkCameraStateComponent.cs` and
  `src/Player/Presentation/PlayerNetworkCameraProjection.cs`, namespace `Terraria.Player` or
  `Terraria.Player.Presentation` as the project boundary requires.
- Sequence: model the snapshot without serialization; route `UpdateNetOffset` through its owner;
  add camera projection tests; add protocol adapter only after packet evidence is supplied.
- Dependency impact: movement/teleport writes `netOffset`; camera and network adapters consume the
  snapshot. `Main.player`, `Main.LocalPlayer`, and scene metrics remain external inputs.
- Scheduler contract: movement/teleport commit -> offset decay/reset -> camera snapshot -> network
  or client projection. Do not infer order from files.
- Rollback: keep the adapter and legacy fields if reset, local/remote selection, or packet tests fail;
  no persistence format change is allowed in this step.

### C03 PlayerShadowAndArmPresentation

- Proposed files: `src/Player/Presentation/PlayerShadowAndArmPresentationComponent.cs` and
  `src/Player/Presentation/PlayerShadowAndArmPresentationSystem.cs`; add the Presentation directory
  only if the existing project now has enough stable projection files to justify it.
- Sequence: introduce fixed-size value state; route shadow reset/update; migrate composite-arm writes;
  add output adapters for renderer/audio/UI; remove legacy writes only after all consumers are listed.
- Dependency impact: pose, mount, item-use, and draw paths are inputs; renderer/audio/UI are outputs.
  `EntityShadowInfo` remains an adapter type and does not leak into authority.
- Scheduler contract: pose commit -> shadow/arm system -> immutable draw/audio snapshots -> output
  adapters. Reset must occur before the first post-spawn projection.
- Rollback: restore legacy arrays and arm writes if any array ordering, arm rotation, sound timing, or
  projection immutability check differs.

### C04 PlayerVisualAndShaderEffects

- Proposed files: `src/Player/Presentation/PlayerVisualAndShaderEffectsProjection.cs` and its
  owner system; namespace `Terraria.Player.Presentation`.
- Sequence: define the snapshot and invalidation inputs; route one shader flag; add renderer/audio
  adapter tests; migrate remaining flags; leave music/fishing fields deferred until owner decisions.
- Dependency impact: consumes appearance, equipment, status, mount, and fishing snapshots; depends on
  content shader IDs through an adapter and must not reference renderer globals in the component.
- Scheduler contract: committed facts -> visual derivation -> immutable output -> renderer/audio adapter.
- Rollback: revert the projection route while retaining content definitions if flag invalidation or
  server/client separation cannot be proven.

### C05 PlayerFootballPresentationState

- Proposed files: `src/Player/Presentation/PlayerFootballPresentationStateComponent.cs` and
  `PlayerFootballPresentationProjection.cs`, one core public type per file.
- Sequence: add a boolean transition component; connect the authoritative item/event command; test
  duplicate and loss transitions; publish the draw snapshot; retain the legacy fields through the
  compatibility window.
- Dependency impact: consumes Item/Equipment facts and produces client draw output; it does not own
  item inventory or football gameplay rules.
- Scheduler contract: item/event commit -> football state owner -> draw projection.
- Rollback: disable the new projection and restore legacy draw reads if acquire/loss/reset parity or
  duplicate-event behavior fails.

### C06 PlayerAppearanceCustomizationState

- Proposed file: `src/Player/PlayerAppearanceCustomizationComponent.cs`; proposed adapters under
  `src/Player/Adapters/` only after the persistence/network format decision.
- Sequence: define value validation and snapshot; add a non-persistent in-memory path; prove spawn
  and clone behavior; implement serialization/network adapter only with authoritative format evidence.
- Dependency impact: identity, equipment, dye/content definitions, save storage, and network are
  external boundaries. Do not make `Color` or packed dye fields globally shared state.
- Scheduler contract: validated command -> appearance owner -> committed snapshot -> visual and
  network projections; load must precede the first projection.
- Rollback: keep the legacy fields and remove only the new adapter if save round-trip, packet, or
  appearance preservation tests are unavailable or fail.

### C07 PlayerTraversalColorProjection

- Proposed file: `src/Player/Presentation/PlayerTraversalColorProjection.cs` with an adapter for
  mount/vehicle and traversal IDs.
- Sequence: define the six-ID read-only snapshot; connect existing traversal facts; add stale-value
  invalidation tests; route renderer reads; do not add persistence or network writes.
- Dependency impact: consumes P03/P07 facts and content color definitions; it must not depend on
  renderer global state or mutate mount/minecart components.
- Scheduler contract: traversal/mount commit -> projection calculation -> client render adapter.
- Rollback: restore direct legacy reads if IDs become stale or the adapter changes draw selection.

### C08 PlayerAppearanceCompanionAndEffectProjection

- Proposed file: `src/Player/Presentation/PlayerAppearanceCompanionAndEffectProjection.cs`.
- Sequence: define source snapshot and ID mapping; connect content/equipment facts; add invalidation
  and despawn tests; migrate client reads; retain legacy fields until the output verifier passes.
- Dependency impact: reads P05/P06/P08 and Content catalogs; no dependency from those authority
  modules back to this projection.
- Scheduler contract: committed source facts -> effect projection -> client adapter.
- Rollback: restore legacy output if companion removal, effect invalidation, or client-only behavior
  diverges.

### C09 PlayerSpatialDerivedProperties

- Proposed file: `src/Player/PlayerSpatialDerivedPropertiesQuery.cs`.
- Sequence: define `PlayerSpatialSnapshot`; implement pure calculations; characterize geometry and
  mount edge cases; migrate one call site at a time; add cache only with explicit invalidation.
- Dependency impact: reads Player position/size, mobility, mount, and status snapshots; it writes no
  component and must not reach `Main` or renderer globals.
- Scheduler contract: all source facts committed before query evaluation; query results are ephemeral
  or immutable output, never a later authority input without a declared snapshot boundary.
- Rollback: retain legacy getters if any result or edge-case fixture changes, and do not add caching.

### C10 PlayerIdentityAndDerivedProperties

- Proposed files: `src/Player/PlayerIdentityAndDerivedPropertiesQuery.cs` and a future explicit
  appearance command adapter; do not add a setter to the query.
- Sequence: migrate normalized reads; characterize `Male` setter call sites; route variant changes
  through the appearance owner; then test idempotence and persistence once format evidence exists.
- Dependency impact: reads `miscCounter` and `skinVariant`/catalog facts; writes only through the
  appearance command boundary. It must not become a second identity or appearance owner.
- Scheduler contract: appearance load/command commit -> identity query -> projections.
- Rollback: retain the legacy `Male` facade if variant transitions or compatibility callers differ.

### C11 PlayerBiomeZoneProperties

- Proposed files: `src/Player/PlayerBiomeZonePropertiesQuery.cs` and, only after owner review,
  `src/WorldSession/PlayerZoneStateCommitSystem.cs` or an existing world-domain owner.
- Sequence: freeze the 16 slot mapping; inventory all zone-array writers; expose immutable read
  snapshots; migrate queries; route setter callers as commands; add invalidation tests.
- Dependency impact: world terrain, depth, events, weather, and spatial systems write source facts;
  P11 must not create a second world authority or duplicate zone arrays.
- Scheduler contract: world/spatial facts commit -> zone state commit -> P11 query -> presentation or
  interaction projection.
- Rollback: retain legacy aliases if a slot mapping or writer ownership is not proven; no cache or
  persistence change is allowed in this checkpoint.

- Implementation checkpoint: add `src/Player/PlayerZoneSnapshot.cs`,
  `PlayerBiomeZonePropertiesSnapshot.cs`, `PlayerBiomeZonePropertiesQuery.cs`, `PlayerZoneFlag.cs`,
  and `PlayerZoneFlagChangeCommand.cs`. The pure query preserves the 16 `zone1`/`zone2` slot aliases
  and the command is only an explicit write intent for the eventual world/environment owner. No
  backing array or world state is mutated in P11.

### C12 PlayerVerticalAndWeatherZoneProperties

- Proposed file: `src/Player/PlayerVerticalAndWeatherZonePropertiesQuery.cs`; reuse the eventual
  world zone-state commit seam rather than adding a second component.
- Sequence: characterize the non-contiguous `zone3` mapping; inventory writers; add immutable input
  snapshot and threshold tests; migrate reads; route setters as explicit commands.
- Dependency impact: reads world height, beach, rain, and sandstorm facts; it must not own world
  weather or duplicate the zone arrays.
- Scheduler contract: world/weather commit -> zone state -> pure query -> consumers.
- Rollback: restore aliases if threshold or slot behavior changes; avoid caching until invalidation
  is tested.

- Implementation checkpoint: add `src/Player/PlayerVerticalAndWeatherZoneInput.cs`,
  `PlayerVerticalAndWeatherZonePropertiesSnapshot.cs`,
  `PlayerVerticalAndWeatherZonePropertiesQuery.cs`, `PlayerVerticalAndWeatherZoneFlag.cs`, and
  `PlayerVerticalAndWeatherZoneFlagChangeCommand.cs`. The query preserves `zone3` indices `0, 1, 4,
  5, 6, 7`; the command is only an explicit environment-owner write intent and does not duplicate
  zone or weather state.

### C13 PlayerEventAndShoppingZoneProperties

- Proposed file: `src/Player/PlayerEventAndShoppingZonePropertiesQuery.cs`; reuse the world/event
  zone-state owner for setter compatibility commands.
- Sequence: freeze zone4/zone5 mapping; inventory event writers; add AnyBiome and depth truth-table
  tests; migrate reads; route setters through explicit event-zone commands.
- Dependency impact: consumes world event, shimmer/liquid, biome, and world-surface snapshots;
  P11 must not own event progression or world depth.
- Scheduler contract: event/world facts commit -> zone state -> shopping/event query -> consumers.
- Rollback: retain legacy aliases if event reset, shimmer, or depth behavior is unproven.

- Implementation checkpoint: add `src/Player/PlayerEventAndShoppingZoneInput.cs`,
  `PlayerEventAndShoppingZonePropertiesSnapshot.cs`, `PlayerEventAndShoppingZonePropertiesQuery.cs`,
  `PlayerEventZoneFlag.cs`, and `PlayerEventZoneFlagChangeCommand.cs`. The query copies the five
  event slots, evaluates `ShoppingZone_AnyBiome` from explicit biome inputs, and evaluates
  `ShoppingZone_BelowSurface` from explicit position/world-surface inputs. Event and shimmer changes
  remain commands for external owners.

### C14 PlayerInteractionAndSelectionProperties

- Proposed file: `src/Player/PlayerInteractionAndSelectionPropertiesQuery.cs`; a future vault
  command belongs with storage/inventory ownership, not in the query file.
- Sequence: define explicit position/input/inventory/camera snapshot; migrate pure predicates; audit
  `IsVoidVaultEnabled` writers; route its setter as a command; add bounds and fallback tests.
- Dependency impact: consumes PlayerInput, PlayerInventory, WorldStorage, camera, mount and lifecycle
  facts; it must not mutate inventory or storage through `HeldItem`.
- Scheduler contract: input/inventory/camera commit -> query snapshot -> interaction consumers.
- Rollback: preserve legacy accessors if selection, vault, or camera fallback parity is not proven.

- Implementation checkpoint: add `src/Player/PlayerInteractionAndSelectionPropertiesInput.cs`,
  `PlayerInteractionAndSelectionPropertiesSnapshot.cs`,
  `PlayerInteractionAndSelectionPropertiesQuery.cs`, and `PlayerVoidVaultStateChangeCommand.cs`.
  The query reads explicit inventory references without copying mutable item state, returns
  `ItemEntityRef.None` for an invalid selected slot, uses position when no network camera target is
  supplied, and leaves vault mutation to the explicit command owner.

### C15 PlayerAbilityAndPresentationProperties

- Source files saved: `src/Player/PlayerAbilityAndPresentationPropertiesInput.cs`,
  `PlayerAbilityAndPresentationPropertiesSnapshot.cs`, and
  `PlayerAbilityAndPresentationPropertiesQuery.cs`; no authority component is added for these
  mixed getters.
- Sequence: define composed input snapshots; migrate pure reads by owner domain; audit both setters
  and private-set `talkNPC`; route command writes; add combination truth-table tests.
- Dependency impact: P06 combat, P07 mobility, P03 mount/vehicle, PlayerEquipment, PlayerRest,
  Teleportation, and presentation are input/owner boundaries. This query cannot write any of them.
- Scheduler contract: all owner facts commit -> composed query -> interaction/draw consumers.
- Rollback: retain legacy facade when formulas or cross-domain gating differ; do not cache mixed facts.

- Implementation checkpoint: the pure query preserves all twelve report properties, uses explicit
  progression/combat/mobility/presentation/rest/teleport/mount/weather inputs, and leaves writes to
  existing owner commands. `talkNPC` is copied from an explicit compatibility input; no query
  mutation or global `Main`/`WorldGen` access was added.
- Actual modified source files: `src/Player/PlayerAbilityAndPresentationPropertiesInput.cs`,
  `PlayerAbilityAndPresentationPropertiesSnapshot.cs`, and
  `PlayerAbilityAndPresentationPropertiesQuery.cs`.
- Core behavior: Version4 damage formulas and ability/draw/rest/portal/Fishron predicates are
  represented as deterministic calculations. Invalid or integration-dependent source facts are not
  silently synthesized.
- Dependency impact: no progression owner, combat/mobility/mount/rest/teleport owner, legacy Player
  API, renderer, persistence/network adapter, project registration, or cross-partition source was
  changed.
- Unverified items: zero-denominator `rangedMultDamage` policy, complete call-site and owner audit,
  `talkNPC` lifecycle, affected-project build, focused formula/gating truth tables, behavior
  equivalence, and scheduler integration.
- Verification status: `not-run` for C15; source is saved and both documents are synchronized before
  starting C16.

### C16 PlayerItemMountAndRuntimeProperties

- Source files saved: `src/Player/PlayerSceneMetricsSnapshot.cs`,
  `PlayerSpectatingCameraTargetSnapshot.cs`, `PlayerItemMountAndRuntimePropertiesInput.cs`,
  `PlayerItemMountAndRuntimePropertiesSnapshot.cs`, and
  `PlayerItemMountAndRuntimePropertiesQuery.cs`; focused verifier is
  `src/PlayerItemMountRuntimeVerification/Program.cs`.
- Sequence: define separate item, mount, camera, scene, and pose inputs; implement pure facade
  methods; test delegate precedence and nullable output; migrate call sites by owner domain.
- Dependency impact: PlayerItemUse, PlayerInventory, PlayerMount, MountVehicle, WorldSession scene
  metrics, PlayerPose, and camera snapshots are inputs. No static `Main` lookup remains in the query.
- Scheduler contract: all owner facts commit -> composed runtime query -> interaction/weapon/draw
  consumers.
- Rollback: keep legacy getters if formulas, fallback, or mount delegate precedence changes; no
  cache is introduced before invalidation evidence.

- Implementation checkpoint: all eleven report properties are represented as deterministic outputs;
  scene metrics and external pose/mount facts remain explicit inputs. The query does not access
  `Main`, inventory object graphs, renderer globals, or mount delegate objects.
- Actual modified source files: `src/Player/PlayerSceneMetricsSnapshot.cs`,
  `PlayerSpectatingCameraTargetSnapshot.cs`, `PlayerItemMountAndRuntimePropertiesInput.cs`,
  `PlayerItemMountAndRuntimePropertiesSnapshot.cs`,
  `PlayerItemMountAndRuntimePropertiesQuery.cs`, and
  `src/PlayerItemMountRuntimeVerification/Program.cs` plus its project file.
- Core behavior: item timing/reuse, rest target, spectator camera offset, slime hyper-jump,
  breathing-reed, track, and nullable mouth/hand output predicates match the inspected Version4
  conditions; mount overrides are honored only for active mounts.
- Dependency impact: no item/mount/scene/camera owner, legacy Player API, renderer, persistence/
  network adapter, project registration, or cross-partition source was changed.
- Unverified items: full scene-metrics fields, spectating registry lifecycle, mount delegate and
  content-set lookup, hand offset table/fallback formula parity, behavior equivalence, and scheduler
  integration.
- Verification status: RED build exited 1 with expected missing-type diagnostics; GREEN build via
  `Invoke-SerialDotnet.ps1 -DotnetArguments` exited 0 with 0 warnings and 0 errors, and produced
  `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` plus the verifier DLL. Focused
  no-build/no-restore run exited 0 with `PASS: player item, mount and runtime derived query`.

### C17 PlayerEyeAnimationState

- Proposed files: `src/Player/Animation/PlayerEyeAnimationComponent.cs` and
  `src/Player/Animation/PlayerEyeAnimationSystem.cs`; `EyeFrameToShow` is returned in an output
  snapshot rather than written by a query.
- Sequence: characterize the state machine; add pure transition/frame calculation fixtures; route
  the update call and hurt event; preserve legacy helper as an adapter until all tests pass.
- Dependency impact: consumes PlayerCombat health/hurt events, PlayerRest sleep, StatusEffects,
  PlayerZone/Weather facts, and item animation; none of those systems depend on the eye component.
- Scheduler contract: committed facts -> hurt event ingestion -> eye state transition -> frame output
  -> client draw projection.
- Rollback: retain `PlayerEyeHelper` compatibility if precedence, timer, or early-return behavior
  changes; do not make the output persistent.

### C18 PlayerPresentationMessagesAndArms

- Saved files: reuse the C03 arm snapshot and add
  `src/Player/Presentation/PlayerOverheadMessageInput.cs`,
  `PlayerOverheadMessageSnapshot.cs`, `PlayerOverheadMessageProjection.cs`,
  `PlayerOverheadMessageStateComponent.cs`, and `PlayerOverheadMessageStateSystem.cs`; keep chat
  protocol conversion in an adapter and use one core public type per file.
- Sequence: isolate arm value data; model message replacement and expiry in the explicit state
  owner; copy/immutably wrap snippet data; add output and lifecycle tests; migrate chat/draw readers;
  remove legacy writes only after ownership review.
- Dependency impact: pose/presentation owns arms; chat/network/UI owns message input/output. The
  projection must not parse packets or mutate `TextSnippet[]`/player authority.
- Scheduler contract: arm/message command -> short-lived presentation state -> expiry update ->
  immutable chat/draw projection.
- Rollback: restore direct legacy message/arm reads if expiry, replacement, snippet ownership, or
  spawn cleanup changes. The saved projection has no legacy registration or write path to remove.

## 6. Component Source-to-Target Map

| component | proposed target | owner/seam | verification gate |
|---|---|---|---|
| C01 | `src/Player/PlayerPoseAndAnimationStateComponent.cs`, `PlayerPoseAndAnimationStateSystem.cs` | mobility/gameplay facts -> pose owner | frame/reset parity |
| C02 | `src/Player/PlayerNetworkCameraStateComponent.cs`, `src/Player/Presentation/PlayerNetworkCameraProjection.cs` | movement/camera snapshot -> adapter | offset/packet/reset parity |
| C03 | `src/Player/Presentation/PlayerShadowAndArmPresentationComponent.cs`, `PlayerShadowAndArmPresentationSystem.cs` | pose/mount/item facts -> draw snapshot | array/arm immutability |
| C04 | `src/Player/Presentation/PlayerVisualAndShaderEffectsProjection.cs` | appearance/equipment/status -> visual adapter | invalidation and one-way output |
| C05 | `src/Player/Presentation/PlayerFootballPresentationStateComponent.cs`, `PlayerFootballPresentationProjection.cs` | item/event command -> draw output | acquire/loss/reset parity |
| C06 | `src/Player/PlayerAppearanceCustomizationComponent.cs`, future `src/Player/Adapters/` | validated appearance command -> persistence/network adapters | format and round-trip evidence |
| C07 | `src/Player/Presentation/PlayerTraversalColorProjection.cs` | mount/mobility facts -> renderer | stale-ID and mapping parity |
| C08 | `src/Player/Presentation/PlayerAppearanceCompanionAndEffectProjection.cs` | equipment/buff/pet facts -> client output | invalidation/despawn parity |
| C09 | `src/Player/PlayerSpatialDerivedPropertiesQuery.cs` | explicit geometry snapshot | deterministic pure query |
| C10 | `src/Player/PlayerIdentityAndDerivedPropertiesQuery.cs` | identity/appearance snapshot plus command seam | variant transition parity |
| C11 | `src/Player/PlayerBiomeZonePropertiesQuery.cs` | shared world zone snapshot | slot mapping and writer audit |
| C12 | `src/Player/PlayerVerticalAndWeatherZonePropertiesQuery.cs` | shared weather/height snapshot | threshold and slot parity |
| C13 | `src/Player/PlayerEventAndShoppingZonePropertiesQuery.cs` | event/zone snapshot plus command seam | truth table and event reset |
| C14 | `src/Player/PlayerInteractionAndSelectionPropertiesQuery.cs` | input/inventory/camera snapshot | bounds/fallback/purity |
| C15 | `src/Player/PlayerAbilityAndPresentationPropertiesQuery.cs` | composed owner queries | formula/gating combinations |
| C16 | `src/Player/PlayerItemMountAndRuntimePropertiesQuery.cs` | item/mount/scene/pose snapshots | delegate/nullable output parity |
| C17 | `src/Player/Animation/PlayerEyeAnimationComponent.cs`, `PlayerEyeAnimationSystem.cs` | combat/rest/weather facts + hurt event | state machine/timer parity |
| C18 | `src/Player/Presentation/PlayerOverheadMessageStateComponent.cs`, `PlayerOverheadMessageStateSystem.cs`, `PlayerOverheadMessageProjection.cs` plus C03 arm snapshot | chat/message command -> client output | expiry/snippet ownership |

This source-to-target table records the original proposed mapping. Later implementation checkpoints
record the isolated C# files actually saved for C01-C18; no Version4 source file was moved, and no
runtime registration or cross-partition authority was changed.

## 7. Checkpoint Audit

The table below is the consolidated current audit. Older checkpoint sections retain the state that
was true when each checkpoint was written; their historical `not-run` labels are superseded by the
verification records in checkpoints 29, 30, 32, and 34.

## 8. Implementation Checkpoint (2026-09-12)

- Current component: none; C01-C18 isolated implementation is saved.
- Added source files:
  `src/Player/Animation/PlayerEyeAnimationState.cs`,
  `src/Player/Animation/PlayerEyeAnimationComponent.cs`,
  `src/Player/Animation/PlayerEyeAnimationInput.cs`,
  `src/Player/Animation/PlayerEyeHurtPresentationEvent.cs`,
  `src/Player/Animation/PlayerEyeAnimationSnapshot.cs`, and
  `src/Player/Animation/PlayerEyeAnimationSystem.cs`.
- The system has explicit input and output boundaries and keeps state writes inside the component
  owner. It does not integrate with legacy PlayerEyeHelper or introduce network/persistence APIs.
- C17 is in `completedComponents` because its isolated source implementation is saved and its
  focused build/smoke checks passed. This does not claim behavior equivalence or legacy integration.
- C01-C16 and C18 are now saved as isolated implementation checkpoints. They remain integration
  limited where their current owner and scheduler evidence is partial.
- Actual verification status: `partial` for the isolated C01-C18 seams. Focused verifier evidence
  is green for C01, C02-C15, C16, C17, and C18; cross-domain integration remains unverified.
- Verification record:
  - Build command: `& .\\Build\\Tools\\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\\src\\Player\\Terraria.Player.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')`.
  - Build result: exit code `0`, `0` warnings, `0` errors; artifact
    `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` confirmed.
  - Focused smoke result: exit code `0`; normal state, 20-tick hurt hold, blindness precedence,
    sleep timer, and back-wall storm suppression passed.
- P11-wide legacy integration, network, persistence, and scheduler verification remains unverified.

Each row was saved to both documents before the next component was analyzed. The implementation pass
has saved C01-C18; focused verifier evidence is now green for C01, C02-C15, C16, C17, and C18,
while cross-domain owners, legacy integration, and full runtime behavior remain partial.

| checkpoint | completed/current transition | evidence-gap | blocking-decision | verificationStatus |
|---|---|---|---|---|
| C01 | source saved -> C02 | pose readers/writers, mobility owner, scheduler, and legacy equivalence incomplete | pose owner and reset scheduler | partial |
| C02 | source saved -> C03 | camera/network packet ownership, local/remote selection, and reset parity incomplete | net offset and camera authority | partial |
| C03 | source saved -> C04 | shadow/arm readers, advanced-shadow adapter, and renderer schedule incomplete | fixed arrays and arm owner | partial |
| C04 | source saved -> C05 | shader/audio/fishing consumers, invalidation, and client/server ownership incomplete | client projection versus authority | partial |
| C05 | source saved -> C06 | football event/replication ownership and legacy transition parity incomplete | item/event source owner | partial |
| C06 | source saved -> C07 | Version4 snapshot serialization is empty in the inspected source; persistence/network policy incomplete | appearance persistence/network policy | partial |
| C07 | completed -> C08 | traversal color writer, invalidation, and registry mapping incomplete | mount/mobility color owner | partial |
| C08 | source saved -> C09 | companion/effect lifecycle, invalidation, and client ownership incomplete | effect projection boundary | partial |
| C09 | source saved -> C10 | geometry authority, cache invalidation, and setter compatibility incomplete | spatial snapshot authority | partial |
| C10 | source saved -> C11 | `Male` setter and skin variant owner incomplete | command versus query compatibility | partial |
| C11 | source saved -> C12 | zone1/zone2 writers, invalidation, and world scheduler incomplete | shared zone-state owner | partial |
| C12 | source saved -> C13 | non-contiguous zone3 mapping, weather/depth writers, and invalidation incomplete | shared zone-state owner | partial |
| C13 | source saved -> C14 | zone4/zone5 writers, shimmer/world-surface ownership, and event reset incomplete | event/liquid integration owner | partial |
| C14 | source saved -> C15 | inventory bounds, mount/camera/vault owners, and input lifecycle incomplete | inventory/storage/input owners | partial |
| C15 | completed -> C16 | mixed ability writers, zero-denominator behavior, talkNPC lifecycle, and order incomplete | cross-domain query facade | partial |
| C16 | completed -> C17 | mount delegates, scene metric authority, and nullable output parity incomplete | composed query inputs | partial |
| C17 | source saved -> focused verifier passed | eye lifecycle/network ownership incomplete | combat event and eye system remain outside this isolated seam | partial |
| C18 | source saved -> none | rich-text message protocol, tick/cleanup owner, and renderer integration incomplete | chat/output lifecycle owner | partial |

## 9. Implementation Checkpoint (2026-09-11T17:56:15Z)

- Current transition: C01 source saved; current component is C02.
- Actual modified source files: `src/Player/IPlayerPoseInputSnapshot.cs`,
  `src/Player/PlayerPoseInputSnapshot.cs`,
  `src/Player/PlayerPoseAndAnimationStateComponent.cs`,
  `src/Player/PlayerPoseAndAnimationSystem.cs`, and `src/Player/PlayerPoseSnapshot.cs`.
- Core behavior: the owner system applies explicit input values, performs the three-segment
  position/rotation update with `0.1f` vertical acceleration and `0.99f` horizontal damping, and
  supports explicit spawn/teleport reset. Snapshot output is immutable value data.
- Dependency impact: no project file, registration key, legacy Player API, or other partition file
  was changed. `stepSpeed`, `gfxOffY`, and `fartKartCloudDelay` are input seams because their
  Version4 writers cross mobility, collision, and mount paths.
- Unverified items: affected-project build, focused pose progression/reset smoke, legacy reader and
  writer parity, scheduler order, and integration with the old Player aggregate.
- `executionStatus: in-progress`, `implementationStatus: in-progress`, and
  `verificationStatus: partial` remain accurate. C01 is counted as completed only for the saved
  isolated implementation checkpoint, not for integration or behavior-equivalence completion.

This execution document records two saved isolated C# implementations, C17 and C01. It claims only
the previously recorded C17 build/smoke evidence and the C01 source checkpoint; runtime, network,
persistence, scheduler, and C01 behavior-equivalence verification remain unrun.

## 10. Implementation Checkpoint (2026-09-11T18:04:00Z)

- Current transition: C02 source saved; next pending component is C03.
- Actual modified source files: `src/Player/PlayerNetworkCameraStateComponent.cs`,
  `src/Player/PlayerNetworkCameraInputSnapshot.cs`, `src/Player/PlayerCameraSnapshot.cs`,
  `src/Player/PlayerNetworkCameraStateSystem.cs`, and
  `src/Player/Presentation/PlayerNetworkCameraProjection.cs`.
- Core behavior: explicit net/camera input, offset threshold/decay, collision-adjusted movement
  input, fake offset override, camera-target synchronization marker, and spawn/teleport reset.
- Dependency impact: no legacy Player API, protocol encoder, renderer, project registration, or
  cross-partition source was changed.
- Unverified items: affected-project build, focused decay/reset smoke, packet parity, local/remote
  selection, scheduler order, and legacy integration.
- `executionStatus: in-progress`, `implementationStatus: in-progress`, and
  `verificationStatus: partial` remain accurate. C02 is counted as completed only for the saved
  isolated implementation checkpoint, not for integration or behavior-equivalence completion.

## 11. Implementation Checkpoint (2026-09-11T18:12:00Z)

- Current transition: C03 source saved; next pending component is C04.
- Actual modified source files: `src/Player/Presentation/PlayerArmStretchAmount.cs`,
  `PlayerCompositeArmSnapshot.cs`, `PlayerAdvancedShadowSlot.cs`, `PlayerShadowSnapshot.cs`,
  `PlayerShadowAndArmPresentationInput.cs`, `PlayerShadowAndArmPresentationComponent.cs`, and
  `PlayerShadowAndArmPresentationSystem.cs`.
- Core behavior: fixed three-slot social-shadow sampling, fixed sixty-slot advanced-shadow ring,
  count cap, arm value input, presentation flags, reset seam, and copied snapshot output.
- Dependency impact: no legacy Player API, renderer/audio/UI adapter, project registration, network,
  persistence, or cross-partition source was changed.
- Unverified items: affected-project build, focused array/ring/arm immutability smoke, shadow readers,
  advanced-shadow conversion, renderer schedule, and legacy arm rotation parity.
- `executionStatus: in-progress`, `implementationStatus: in-progress`, and
  `verificationStatus: partial` remain accurate. C03 is counted as completed only for the saved
  isolated implementation checkpoint, not for integration or behavior-equivalence completion.

## 12. Implementation Checkpoint (2026-09-11T18:20:00Z)

- Current transition: C04 source saved; next pending component is C05.
- Actual modified source files: `src/Player/Presentation/PlayerVisualAndShaderEffectsInput.cs`,
  `PlayerVisualAndShaderEffectsSnapshot.cs`, and `PlayerVisualAndShaderEffectsProjection.cs`.
- Core behavior: one-way immutable projection of all 21 report fields; no renderer globals, shader
  registry, audio, fishing, network, persistence, or gameplay writes.
- Dependency impact: no legacy Player API, project registration, or cross-partition source was
  changed. `musicBox` and `overrideFishingBobber` remain explicit external inputs.
- Unverified items: affected-project build, focused projection immutability, invalidation, client/server
  split, shader/audio/fishing adapter parity, and legacy consumer parity.
- `executionStatus: in-progress`, `implementationStatus: in-progress`, and
  `verificationStatus: partial` remain accurate. C04 is counted as completed only for the saved
  isolated implementation checkpoint, not for integration or behavior-equivalence completion.

## 13. Implementation Checkpoint (2026-09-11T18:28:00Z)

- Current transition: C05 source saved; next pending component is C06.
- Actual modified source files: `src/Player/Presentation/PlayerFootballPresentationInput.cs`,
  `PlayerFootballPresentationStateComponent.cs`, `PlayerFootballPresentationStateSystem.cs`,
  `PlayerFootballPresentationSnapshot.cs`, and `PlayerFootballPresentationProjection.cs`.
- Core behavior: possession/draw separation, animation suppression, item-loss/spawn reset, and
  immutable draw output.
- Dependency impact: no inventory/equipment owner, renderer, network, persistence, project
  registration, legacy Player API, or cross-partition source was changed.
- Unverified items: affected-project build, focused transition/reset smoke, event duplication,
  replication, renderer schedule, and legacy draw parity.
- `executionStatus: in-progress`, `implementationStatus: in-progress`, and
  `verificationStatus: partial` remain accurate. C05 is counted as completed only for the saved
  isolated implementation checkpoint, not for integration or behavior-equivalence completion.

## 14. Implementation Checkpoint (2026-09-11T18:36:00Z)

- Current transition: C06 source saved; next pending component is C07.
- Actual modified source files: `src/Player/PlayerAppearanceColor.cs`,
  `PlayerAppearanceCustomizationInput.cs`, `PlayerAppearanceCustomizationComponent.cs`,
  `PlayerAppearanceCustomizationSystem.cs`, and `PlayerAppearanceCustomizationSnapshot.cs`.
- Core behavior: explicit in-memory update and immutable snapshot for all 10 appearance fields,
  including documented Version4 default colors.
- Dependency impact: no persistence/network adapter, renderer, identity/equipment owner, project
  registration, legacy Player API, or cross-partition source was changed.
- Unverified items: build, focused value/snapshot checks, input validation, spawn/clone behavior,
  serialization round-trip, packet compatibility, and renderer parity.
- `executionStatus: in-progress`, `implementationStatus: in-progress`, and
  `verificationStatus: partial` remain accurate. C06 is counted as completed only for the saved
  isolated implementation checkpoint, not for persistence or integration completion.

## 17. Implementation Checkpoint (2026-09-12T03:30:00Z)

- Current transition: C10 source saved; next pending component is C11.
- Actual modified source files: `src/Player/PlayerIdentityAndDerivedPropertiesInput.cs`,
  `PlayerIdentityAndDerivedPropertiesSnapshot.cs`, `PlayerMaleChangeCommand.cs`, and
  `PlayerIdentityAndDerivedPropertiesQuery.cs`.
- Core behavior: pure `miscCounter / 300` normalization and current identity snapshot; the `Male`
  setter is represented as an explicit idempotent command intent that returns the supplied alternate
  gender reference only when the requested value differs. No query mutation is performed.
- Dependency impact: no PlayerVariantID catalog, appearance owner, persistence/network adapter,
  renderer, project registration, legacy Player API, or other partition source was changed.
- Unverified items: catalog mapping, call-site inventory, command application owner, persistence,
  packet compatibility, behavior equivalence, and scheduler integration.
- Verification status: `not-run` for the C10 build and focused query/command smoke; source files are
  saved and C11 must not begin until this checkpoint is persisted.

## 18. Implementation Checkpoint (2026-09-11T18:43:00Z)

- Current transition: C11 source saved; next pending component is C12.
- Actual modified source files: `src/Player/PlayerZoneSnapshot.cs`,
  `PlayerBiomeZonePropertiesSnapshot.cs`, `PlayerBiomeZonePropertiesQuery.cs`, `PlayerZoneFlag.cs`,
  and `PlayerZoneFlagChangeCommand.cs`.
- Core behavior: explicit immutable 16-slot zone input and pure output mapping for every C11 report
  property; zone changes are represented as a command intent and do not write `zone1` or `zone2`.
- Dependency impact: no world/environment authority, zone-array owner, cache, persistence/network
  path, legacy Player API, project registration, or cross-partition source was changed.
- Unverified items: complete zone writer inventory, lifecycle invalidation, setter compatibility,
  world scheduler order, affected-project build, and focused repeated-query/mapping verification.
- Verification status: `not-run` for C11; source is saved and both documents are synchronized before
  starting C12.

## 19. Implementation Checkpoint (2026-09-11T18:52:00Z)

- Current transition: C12 source saved; next pending component is C13.
- Actual modified source files: `src/Player/PlayerVerticalAndWeatherZoneInput.cs`,
  `PlayerVerticalAndWeatherZonePropertiesSnapshot.cs`,
  `PlayerVerticalAndWeatherZonePropertiesQuery.cs`, `PlayerVerticalAndWeatherZoneFlag.cs`, and
  `PlayerVerticalAndWeatherZoneFlagChangeCommand.cs`.
- Core behavior: explicit immutable six-slot weather/height input and pure output mapping for all C12
  properties, preserving Version4's non-contiguous `zone3` indices `0, 1, 4, 5, 6, 7`; the command
  is an explicit write intent and does not mutate weather or zone state.
- Dependency impact: no world/weather owner, zone-array state, cache, persistence/network adapter,
  legacy Player API, project registration, or cross-partition source was changed.
- Unverified items: exact slot/threshold characterization, writer and lifecycle inventory,
  invalidation, scheduler order, affected-project build, and focused C12 query verification.
- Verification status: `not-run` for C12; source is saved and both documents are synchronized before
  starting C13.

## 20. Implementation Checkpoint (2026-09-11T19:02:00Z)

- Current transition: C13 source saved; next pending component is C14.
- Actual modified source files: `src/Player/PlayerEventAndShoppingZoneInput.cs`,
  `PlayerEventAndShoppingZonePropertiesSnapshot.cs`, `PlayerEventAndShoppingZonePropertiesQuery.cs`,
  `PlayerEventZoneFlag.cs`, and `PlayerEventZoneFlagChangeCommand.cs`.
- Core behavior: explicit event/biome/position/world-surface input; pure projection of the five event
  slots, Version4-compatible `ShoppingZone_AnyBiome` truth calculation, and
  `ShoppingZone_BelowSurface` threshold calculation. Event and shimmer changes are command intents
  only and do not mutate world state.
- Dependency impact: no world-event, shimmer/liquid, zone-array, persistence/network, renderer,
  legacy Player API, project registration, or cross-partition source was changed.
- Unverified items: complete zone4/zone5 writers, event reset ordering, shimmer ownership, world-surface
  authority, invalidation, affected-project build, and focused truth-table verification.
- Verification status: `not-run` for C13; source is saved and both documents are synchronized before
  starting C14.

## 21. Implementation Checkpoint (2026-09-11T19:12:00Z)

- Current transition: C14 source saved; next pending component is C15.
- Actual modified source files: `src/Player/PlayerInteractionAndSelectionPropertiesInput.cs`,
  `PlayerInteractionAndSelectionPropertiesSnapshot.cs`,
  `PlayerInteractionAndSelectionPropertiesQuery.cs`, and `PlayerVoidVaultStateChangeCommand.cs`.
- Core behavior: explicit pure calculations for directions, selection, held-item identity,
  floating/talk/hover predicates, vault output, and camera fallback. Invalid selected slots return
  `ItemEntityRef.None`; no mutable inventory or global camera state is accessed.
- Dependency impact: no inventory/storage owner, mount owner, camera adapter, vault writer, renderer,
  persistence, legacy Player API, project registration, or other partition source was changed.
- Unverified items: selection bounds and call sites, mount type semantics, vault writer, camera target
  lifecycle, invalidation, affected-project build, and focused interaction truth-table verification.
- Verification status: `not-run` for C14; source is saved and both documents are synchronized before
  starting C15.

## 22. Implementation Checkpoint (2026-09-11T20:58:26Z)

- Current transition: C18 source saved and focused-verified; no pending P11 component remains.
- Top-level state is now `implementationStatus: completed`, `currentComponent: none`, and
  `pendingComponents: []`. `executionStatus: in-progress` and `verificationStatus: partial`
  remain accurate because this checkpoint closes isolated source implementation, not legacy
  integration or behavior-equivalence verification.
- Actual modified source files for C18:
  `src/Player/Presentation/PlayerOverheadMessageInput.cs`,
  `src/Player/Presentation/PlayerOverheadMessageSnapshot.cs`, and
  `src/Player/Presentation/PlayerOverheadMessageProjection.cs`.
  The verifier files are `src/PlayerPresentationMessageVerification/Program.cs` and
  `src/PlayerPresentationMessageVerification/Terraria.PlayerPresentationMessageVerification.csproj`.
  C03's existing `src/Player/Presentation/PlayerCompositeArmSnapshot.cs` is reused and was not
  duplicated or changed in this checkpoint.
- Core behavior: the projection passes arm snapshots, chat text, parsed snippets, size, expiry,
  and color into a value-oriented output. The output defensively copies snippets into a read-only
  collection, preserves replacement messages, reports visibility only for positive `TimeLeft`,
  and performs no expiry mutation, chat parsing, renderer work, or authority write.
- Dependency impact: only the existing Player presentation types and `System.Numerics` are used;
  no legacy API, project registration, protocol encoder, Content dependency, persistence/network
  adapter, or other partition source changed.
- Verification evidence:
  - Build used the serial wrapper with the C18 verifier project, `-m:1`, `-nr:false`,
    `UseSharedCompilation=false`, `MSBuildNodeReuse=false`, and `BuildInParallel=false`; exit code
    `0`, `0` warnings, `0` errors. Artifacts were confirmed under
    `Build/bin/Terraria.Player/Debug/net10.0/` and
    `Build/bin/Terraria.PlayerPresentationMessageVerification/Debug/net10.0/`.
  - Focused executable command:
    `& '.\Build\bin\Terraria.PlayerPresentationMessageVerification\Debug\net10.0\Terraria.PlayerPresentationMessageVerification.exe'; $code=$LASTEXITCODE; 'EXIT_CODE=' + $code; exit $code`.
    Result: exit code `0`; `PASS: player overhead message and arm projection`.
  - The direct executable run was selected after a process check found unrelated orphan
    `/nodeReuse:true` MSBuild nodes in the shared checkout; no process was terminated and no
    concurrent compile was launched.
- Unverified items: Version4 `TextSnippet` rich-text protocol fidelity, chat/network adapter and
  message lifecycle ownership, expiry tick/cleanup on spawn/removal, renderer integration, legacy
  output parity, and scheduler order. These remain `evidence-gap`/`blocking-decision` entries and
  are not represented as completed behavior.
- Verification status for C18: `partial`; isolated implementation and focused verifier passed.

## 23. Implementation Checkpoint (2026-09-11T21:09:38Z)

- Current transition: C18 lifecycle state source saved; no additional P11 component is pending.
  `completedComponents` remains `[C17, C01, C02, C03, C04, C05, C06, C07, C08, C09, C10, C11,
  C12, C13, C14, C15, C16, C18]`, `currentComponent: none`, and `pendingComponents: []`.
- Actual modified source files:
  `src/Player/Presentation/PlayerOverheadMessageStateComponent.cs`,
  `src/Player/Presentation/PlayerOverheadMessageStateSystem.cs`,
  `src/Player/Presentation/PlayerOverheadMessageProjection.cs`, and
  `src/PlayerPresentationMessageVerification/Program.cs`. No project registration or project file
  change was required.
- Core behavior: `ReplaceMessage` defensively copies the message text and snippet sequence;
  `AdvancePresentationTick` decrements positive `timeLeft` once and stops at zero;
  `ResetForSpawn` and `ResetForRemoval` clear all short-lived message fields. The projection reads
  the state and emits the existing immutable snapshot while the arm values remain supplied by C03.
- Dependency impact: only the Player presentation value types, `System.Numerics`, and BCL
  collection copying are used. No chat parser, `TextSnippet` protocol, network/persistence adapter,
  renderer, registration key, legacy Player API, or cross-partition source was changed.
- Verification status at this checkpoint: `partial`; the current lifecycle source required a fresh
  serial build and focused run at that time. The later 21:24:42Z checkpoint records the fresh build
  and focused result. The verifier source covers replacement, defensive ownership, tick decrement,
  expiry, spawn reset, and removal reset.
- Evidence gap and blocking decision: rich-text protocol fidelity, external chat adapter ownership,
  renderer integration, legacy parity, and scheduler integration remain unverified. The current
  lifecycle implementation stays an isolated in-memory seam until those owners are confirmed.

## 24. Implementation Checkpoint (2026-09-11T21:24:42Z)

- C18 lifecycle source is now serially rebuilt and focused-verified. The verifier correction only
  restores the original caller-owned snippet list before `ReplaceMessage`, then mutates that list
  after replacement to prove the state component copied its input; no production source behavior
  changed in this checkpoint.
- Verification evidence:
  - Initial lifecycle verifier run failed with exit code `1` at `Program.cs:42`,
    `The state must retain snippet order.` Root cause: the verifier mutated the same input list used
    for state replacement before asserting the old values. This was a test-fixture ordering error,
    not a production failure.
  - Serial build command:
    `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\src\PlayerPresentationMessageVerification\Terraria.PlayerPresentationMessageVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); exit $LASTEXITCODE"`.
  - Build result: exit code `0`, `0` warnings, `0` errors. Confirmed output paths:
    `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` and
    `Build/bin/Terraria.PlayerPresentationMessageVerification/Debug/net10.0/Terraria.PlayerPresentationMessageVerification.dll`.
  - Focused command:
    `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('run', '--project', '.\src\PlayerPresentationMessageVerification\Terraria.PlayerPresentationMessageVerification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); exit $LASTEXITCODE"`.
  - Focused result: exit code `0`; output `PASS: player overhead message state and arm projection`.
- C18 isolated evidence now covers arm value-copy, message value preservation, replacement,
  defensive snippet ownership at both projection and state boundaries, one-tick expiry decrement,
  zero-expiry behavior, and spawn/removal cleanup. Rich-text `TextSnippet` fidelity, chat/network
  adapter ownership, renderer integration, legacy parity, and scheduler integration remain
  explicitly unverified.
- Verification status for C18: `partial`; the isolated implementation and focused verifier are
  green, while cross-domain integration remains open.

## 16. Implementation Checkpoint (2026-09-12T03:24:00Z)

- Current transition: C09 source saved; next pending component is C10.
- Actual modified source files: `src/Player/PlayerSpatialSnapshot.cs`,
  `PlayerSpatialHitbox.cs`, and `PlayerSpatialDerivedPropertiesQuery.cs`.
- Core behavior: explicit pure-query input and value outputs for all nine C09 report properties;
  mount precedence, portable-stool offsets, bestiary hitbox inflation, standing-still threshold,
  visual offset, and crowd-control predicate match the inspected Version4 bodies. The
  `MountedCenter` setter is intentionally outside this query boundary because it writes position.
- Dependency impact: no mount/mobility/status owner, world-global lookup, renderer, project
  registration, legacy Player API, or other partition source was changed; no cache was introduced.
- Unverified items: full geometry snapshot authority, all readers/writers, setter compatibility,
  mount/stool edge cases, behavior equivalence, and scheduler integration.
- Verification status: `not-run` for the C09 build and focused query verifier; source files are
  saved and C10 must not begin until this checkpoint is persisted.

## 15. Implementation Checkpoint (2026-09-12T03:18:00Z)

- Current transition: C08 source saved; next pending component is C09.
- Actual modified source files: `src/Player/Presentation/PlayerAppearanceCompanionAndEffectInput.cs`,
  `PlayerAppearanceCompanionAndEffectSnapshot.cs`, and
  `PlayerAppearanceCompanionAndEffectProjection.cs`.
- Core behavior: one-way eleven-slot projection for the report fields `cPet`, `cLight`, `cYorai`,
  `cPortableStool`, `cUnicornHorn`, `cAngelHalo`, `cBeard`, `cMinion`, `cLeinShampoo`,
  `cFlameWaker`, and `cCoat`; output order and values are preserved.
- Dependency impact: no equipment/buff/pet owner, content catalog, renderer/effect service,
  network, persistence, project registration, legacy Player API, or other partition source was
  changed.
- Unverified items: source writers, invalidation/despawn timing, duplicate-event behavior,
  client/server split, legacy output parity, and scheduler integration.
- Verification status: `not-run` for C08 build and focused projection; the source checkpoint is
  saved and must be verified before broader integration claims.

## 25. Verification Checkpoint (2026-09-11T21:41:57Z)

- C01 focused verifier source is saved under `src/PlayerPoseVerification/`:
  `Program.cs` and `Terraria.PlayerPoseVerification.csproj`.
- The verifier covers confirmed pose progression, early-return, spawn reset, and teleport reset;
  it does not claim ownership of `stepSpeed`, `gfxOffY`, or `fartKartCloudDelay` writers.
- Verification status: `not-run` for this new C01 verifier pending the serial build and focused
  run. No C02 work may begin until this checkpoint is verified and persisted.

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

- C02's focused verifier now covers projection immutability: it captures a camera snapshot, changes
  the component, and checks that the captured offset remains unchanged.
- Actual modified verifier file: `src/PlayerPresentationDerivedVerification/Program.cs`.
- Verification status remains `partial` pending the serial build and focused run. Packet duplicate
  rejection and local/remote player selection remain unverified because their authority is outside
  this isolated boundary.

## 26. Verification Checkpoint (2026-09-11T21:49:00Z)

- C01 focused verifier build command:
  `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\src\PlayerPoseVerification\Terraria.PlayerPoseVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); exit $LASTEXITCODE"`.
- Build result: exit code `0`, `0` warnings, `0` errors. Confirmed artifacts:
  `Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll` and
  `Build/bin/Terraria.PlayerPoseVerification/Debug/net10.0/Terraria.PlayerPoseVerification.dll`.
- C01 focused run command:
  `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('run', '--project', '.\src\PlayerPoseVerification\Terraria.PlayerPoseVerification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); exit $LASTEXITCODE"`.
- Focused result: exit code `0`; output `PASS: player pose progression and reset`.
- C01 focused verification is green; external writers, scheduler ordering, legacy behavior
  equivalence, and cross-domain integration remain unverified.

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
  in Section 4 of this execution document.
- The consolidated execution state remains:
  `completedComponents=[C17,C01,C02,C03,C04,C05,C06,C07,C08,C09,C10,C11,C12,C13,C14,C15,C16,C18]`,
  `currentComponent=none`, and `pendingComponents=[]`; `implementationStatus=completed` and
  `verificationStatus=partial` remain accurate.
- Focused evidence is green only for the declared isolated scopes. Legacy integration, writer
  uniqueness across partitions, scheduler ordering, renderer/chat protocol parity, network and
  persistence compatibility, and cross-domain authority ownership remain unverified. The
  canonical target-index omission recorded in checkpoint 33 remains unchanged and was not edited.

## 35. Implementation Checkpoint (2026-09-12T01:44:46Z)

- Current transition: C17 lifecycle reset sub-unit saved and verified; C01-C18 remain in
  completedComponents, currentComponent=none, and pendingComponents=[].
- Actual modified source files:
  src/Player/Animation/PlayerEyeAnimationComponent.cs,
  src/Player/Animation/PlayerEyeAnimationSystem.cs, and
  src/PlayerEyeAnimationVerification/Program.cs.
- Core behavior: the component reset owner restores NormalBlinking, TimeInState = 0, and
  EyeFrameToShow = 0; the system exposes ResetForSpawn and ResetForRemoval; the focused verifier
  proves that a hurt-induced closed-eye state cannot leak across either lifecycle reset.
- Dependency impact: no legacy Player API, PlayerEyeHelper, registration key, project file,
  renderer, network/persistence adapter, or cross-partition source was changed. The lifecycle
  methods are isolated seams and are not wired into the actual legacy Player call sites.
- TDD and verification evidence:
  - RED build, before the production edit: serial wrapper build exited 1 with exactly two CS1061
    errors for the missing ResetForSpawn and ResetForRemoval methods.
  - GREEN build: serial wrapper build of
    src/PlayerEyeAnimationVerification/Terraria.PlayerEyeAnimationVerification.csproj exited 0,
    with 0 warnings and 0 errors. Artifacts are under
    Build/bin/Terraria.Player/Debug/net10.0/ and
    Build/bin/Terraria.PlayerEyeAnimationVerification/Debug/net10.0/.
  - Focused run: serial wrapper invocation with run --project
    src/PlayerEyeAnimationVerification/Terraria.PlayerEyeAnimationVerification.csproj
    --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false
    -p:MSBuildNodeReuse=false -p:BuildInParallel=false exited 0 and emitted
    PASS: player eye animation state, precedence, hurt hold, frame projection, and lifecycle
    reset.
- lastCheckpointUtc is 2026-09-12T01:44:46Z. verificationStatus remains partial because only
  the isolated C17 seam is verified; legacy spawn/removal wiring, scheduler order, network
  ownership, persistence, renderer integration, and behavior equivalence remain unverified.

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
  parity remain unverified. `executionStatus=completed` records that no P11 component remains
  pending; `verificationStatus=partial` is retained for the documented integration gaps.
