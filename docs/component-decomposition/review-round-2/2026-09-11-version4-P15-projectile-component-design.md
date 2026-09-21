# Version4 P15 Projectile Component Design

```yaml
partitionId: P15
sessionId: d27d18b95dab4abd9b181453fe1f4916
inputReport: D:\\TRbackup\\NLTX\\docs\\组件文档\\迁移参考表\\Version4权威模拟系统字段属性逐成员源码声明-20分区\\P15-Projectile.md
componentDesignPath: D:\\TRbackup\\NLTX\\docs\\组件文档\\第二轮审查\\2026-09-11-version4-P15-projectile-component-design.md
componentExecutionPath: D:\\TRbackup\\NLTX\\docs\\组件文档\\第二轮审查\\2026-09-11-version4-P15-projectile-component-execution.md
designStatus: proposed
executionStatus: in-progress
implementationStatus: partial
evidenceStatus: partial
currentNltxStatus: partial
verificationStatus: partial
designCompletedComponents: [ProjectileIdentityAndClassificationState, ProjectileAiState, ProjectileLifetimeAndRuntimeState, ProjectileCombatAndImmunity, ProjectileMovementAndCollisionState, ProjectileNetworkReplicationState, ProjectileMinionAndPresentationState, ProjectileDamageAndElementState, ProjectileAnimationAndDirectionState, ProjectileCollisionAndTargetingState, ProjectileCombatScalingState, ProjectileCollisionGeometryCache, ProjectileTargetSelectionCache, ProjectileFishingAndMiningQueryState, ProjectileKiteAndLightningRules, ProjectileDerivedProperties, ProjectileStormDefinition]
completedComponents: [ProjectileStormDefinition, ProjectileAiState, ProjectileLifetimeAndRuntimeState, ProjectileDispositionStateComponent, ProjectileTrailCacheComponent, ProjectileCollisionPolicyComponent, ProjectileNetworkStateComponent, ProjectileAnimationStateComponent, ProjectilePresentationStateComponent, ProjectileMinionCapabilityComponent, ProjectileBobberCapabilityComponent, ProjectileCounterweightCapabilityComponent, ProjectileSentryCapabilityComponent, ProjectileTrapCapabilityComponent, ProjectileDamagePolicyComponent, ProjectileDefinitionReferenceComponent, ProjectileBehaviorStateComponent, ProjectileLifetimeStateComponent, ProjectileDamagePayloadComponent, ProjectileGeometryStateComponent, ProjectileSourceMetadataComponent, ProjectileTrajectoryStateComponent]
completedSourceUnits: [ProjectileStormDefinition, ProjectileStormDefinitionFactory, ProjectileIdentityComponent, ProjectileAiState, ProjectileBehaviorStateComponent, ProjectileLifetimeAndRuntimeState, ProjectileLifetimeStateComponent, ProjectileEffectCooldownStateComponent, ProjectileDispositionStateComponent, ProjectilePenetrationStateComponent, ProjectileHitImmunityPolicyComponent, ProjectileHitImmunityStateComponent, ProjectileReflectionStateComponent, ProjectileTrailCacheComponent, ProjectileCollisionPolicyComponent, ProjectileNetworkStateComponent, ProjectileAnimationStateComponent, ProjectileDamagePayloadComponent, ProjectileDamagePolicyComponent, ProjectileDirectionPolicyComponent, ProjectileUpdateCadenceComponent, ProjectilePresentationStateComponent, ProjectileMinionCapabilityComponent, ProjectileBobberCapabilityComponent, ProjectileCounterweightCapabilityComponent, ProjectileSentryCapabilityComponent, ProjectileTrapCapabilityComponent, ProjectileGeometryStateComponent, ProjectileTrajectoryStateComponent, ProjectileSourceMetadataComponent]
currentComponent: ProjectileDerivedProperties
pendingComponents: [ProjectileIdentityAndClassificationState, ProjectileCombatAndImmunity, ProjectileMovementAndCollisionState, ProjectileNetworkReplicationState, ProjectileMinionAndPresentationState, ProjectileDamageAndElementState, ProjectileAnimationAndDirectionState, ProjectileCollisionAndTargetingState, ProjectileCombatScalingState, ProjectileCollisionGeometryCache, ProjectileTargetSelectionCache, ProjectileFishingAndMiningQueryState, ProjectileKiteAndLightningRules, ProjectileDerivedProperties]
lastCheckpointUtc: 2026-09-12T17:14:10Z
evidence-gap: All 17 leaf groups and 126 members remain mapped. Narrow source boundaries with current build evidence include ProjectileStormDefinition, ProjectileIdentityComponent, ProjectileDefinitionReferenceComponent, ProjectileAiState, ProjectileBehaviorStateComponent, ProjectileLifetimeAndRuntimeState, ProjectileLifetimeStateComponent, ProjectileEffectCooldownStateComponent, ProjectileDispositionStateComponent, ProjectilePenetrationStateComponent, ProjectileHitImmunityPolicyComponent, ProjectileHitImmunityStateComponent, ProjectileReflectionStateComponent, ProjectileTrailCacheComponent, ProjectileCollisionPolicyComponent, ProjectileNetworkStateComponent, ProjectileAnimationStateComponent, ProjectileDamagePayloadComponent, ProjectileDamagePolicyComponent, ProjectileDirectionPolicyComponent, ProjectileUpdateCadenceComponent, ProjectilePresentationStateComponent, ProjectileMinionCapabilityComponent, ProjectileBobberCapabilityComponent, ProjectileCounterweightCapabilityComponent, ProjectileSentryCapabilityComponent, ProjectileTrapCapabilityComponent, ProjectileGeometryStateComponent, ProjectileTrajectoryStateComponent, and ProjectileSourceMetadataComponent. The three capability marker components are compiled and reflection-checked with value-type layout, bool marker fields, default `false`, and explicit `true` construction; no runtime attachment, Player ownership, world interaction, or cleanup behavior is claimed. The existing focused verifier still covers the earlier state boundaries; no new test was added for the four newly hardened payload/geometry/trajectory/source constructors, the new presentation/capability-marker fields, DirectionPolicy, UpdateCadence, or the three capability marker constructors. A read-only reflection check now also exercises valid constructor values and rejection of invalid scale, step speed, knockback, and null source-text inputs. The identity value boundary, immutable projectile type/catalog reference, and runtime disposition state boundary are implemented, but lifecycle/classification integration remains unresolved: active ownership, static NPC immunity registry, owner-hit distance policy integration, arrow classification writer, capability-specific writers, network importance integration, no-drop source metadata integration, scale/rotation runtime integration, and alpha/glowMask projection writers. Player/fishing capability ownership, enchantment evaluation, and client presentation projection remain external. ProjectileDispositionSystem, ProjectilePenetrationSystem, ProjectileHitImmunitySystem, ProjectileReflectionSystem, ProjectileTrailCacheSystem, ProjectileCollisionPolicySystem, ProjectileNetworkStateSystem, and ProjectileAnimationStateSystem remain narrow source writers only. None of these systems decides target eligibility, writes NPC/Player health, owns the static NPC registry, computes reflected trajectory or bounce physics, publishes packet bytes, performs connection/section eligibility checks, selects AI-specific frame timing, accesses textures, or terminates a projectile. ProjectileAiState deliberately does not hold aiStyle; the behavior key is stored by ProjectileDefinitionReferenceComponent. The lifetime implementation is not yet connected to the legacy/root ProjectileLifetimeComponent, an entity store, identity-map cleanup, network tombstones, owner-loss/environment checks, or a runtime scheduler. Behavior dispatch, network ingress/identity matching, packet serialization, lifecycle ordering, specialized trail behavior, world collision, automatic direction rules, texture projection, and raw/local synchronization fixtures remain unverified. ProjectileUpdateCadenceComponent has no negative-value validation because MaxUpdates writer ordering and zero/negative compatibility are not closed. The current component-only scope is exhausted for independently determinable state; remaining P15 units require non-component systems, queries, commands, adapters, projections, or cross-domain owners and remain pending.
blocking-decision: Treat each listed source component as complete only for its stored fields, constructor defaults, and compile evidence. This includes the eight presentation fields, `IsMinion`, `IsBobber`, `NoEnchantments`, and `NoEnchantmentVisuals`; it does not close their runtime writers or external consumers. Keep runtime integration, the unresolved identity members, Player/fishing ownership, enchantment evaluation, client projection, static NPC immunity, packet transport, world/owner checks, and AI dispatch pending. Do not add registry cleanup, protocol writes, world/owner checks, or presentation effects until their owners and evidence are closed; do not fill missing Version4 behavior by inference.
  capability-marker-evidence-gap: The three capability marker source files are compiled and reflection-checked in `Build/bin/Terraria.Projectile/Debug/net10.0/Terraria.Projectile.dll`; each is a value type with a bool marker, default `false`, and explicit `true` construction. No runtime attachment, Player ownership, world interaction, or cleanup behavior is claimed.
  evidence-gap-update: ProjectileDamagePayloadComponent now exposes read-only IsMelee, IsRanged, and IsMagic compatibility views derived from the single DamageClass authority; no second writable boolean authorities were added. The fresh build, existing focused verifier, and read-only reflection smoke check now pass. The duplicate Reflected and Ai/LocalAi storage surfaces remain preserved for public compatibility pending caller and writer audit.
  blocking-decision-update: Keep the DamageClass-derived legacy views read-only and retain the existing duplicate Reflected and Ai/LocalAi fields until their consumers are migrated and a focused compatibility contract is verified. Do not remove or redirect ProjectileCollisionPolicyComponent.ManualDirectionChange because the existing ProjectileCombatVerification uses it.
  derived-properties-evidence-gap-update: ProjectilePresentationStateComponent now exposes read-only Opacity as the legacy Alpha-derived view `1.0f - Alpha / 255.0f`; ProjectileUpdateCadenceComponent now exposes read-only MaxUpdates as the legacy ExtraUpdates-derived view `ExtraUpdates + 1`. The focused component smoke evidence passes for the documented boundary values. These properties add no second writable authority. Setter ownership, runtime writer ordering, substep/lifetime/cooldown integration, network or persistence projection, and client presentation behavior remain unresolved.
  derived-properties-blocking-decision-update: Treat Opacity and MaxUpdates as compatibility views only. Do not add setters, duplicate opacity/cadence storage, or claim the full ProjectileDerivedProperties group until the Alpha and ExtraUpdates writers, legacy conversion behavior, and cross-domain consumers are recovered and verified.
  component-boundary-audit: The remaining report members were reconciled against the current `src/Projectile` component files. No independently determinable component file remains safe to add without duplicating an existing authority: owner-hit policy is already stored by ProjectileCollisionPolicyComponent, bonus crit is already stored by ProjectileDamagePayloadComponent, and the remaining scaling curve, static immunity registry, geometry/target/fishing workspaces, kite/lightning rules, and derived properties belong to definitions, queries, adapters, or projections. A second TargetingPolicyComponent would create duplicate owner-hit state until its callers are migrated. This audit added no source code and does not advance implementation or verification status.
```

### Current source checkpoint update

`ProjectilePresentationStateComponent` now stores `Alpha`, `GlowMask`, `Light`, `IsPreviewDummy`,
`IsPreviewDisplayDoll`, `DrawLayer`, `UsesOwnerLight`, and `Hide` without claiming rendering or
lighting ownership. `ProjectileMinionCapabilityComponent` and `ProjectileBobberCapabilityComponent`
now store explicit `IsMinion` and `IsBobber` markers alongside their existing accounting/type
values. `ProjectileDamagePolicyComponent` now stores both `NoEnchantments` and
`NoEnchantmentVisuals`. `ProjectileGeometryStateComponent`, `ProjectileTrajectoryStateComponent`,
`ProjectileDamagePayloadComponent`, and `ProjectileSourceMetadataComponent` now enforce only their
documented constructor invariants: finite/non-negative scale, finite AI/rotation/step values and
non-negative substep count, finite knockback and non-negative armor penetration, and non-null source
text/non-negative minion item prefix. `ProjectileDirectionPolicyComponent` and
`ProjectileUpdateCadenceComponent` remain storage-only policy/value boundaries; the cadence
component intentionally does not claim negative-value validation. The affected Projectile project
and its existing focused verifier were run after these source changes. The build completed with
exit code `0`, `0` warnings, and `0` errors; the verifier passed with exit code `0`. A reflection
check confirmed all listed public fields/properties in
`Build/bin/Terraria.Projectile/Debug/net10.0/Terraria.Projectile.dll`.
Alpha/glowMask writers, Player/fishing ownership, enchantment evaluation, cadence scheduling, and
client projection remain unresolved.

The source checkpoint also includes the storage-only `ProjectileCounterweightCapabilityComponent`,
`ProjectileSentryCapabilityComponent`, and `ProjectileTrapCapabilityComponent`, with explicit
`IsCounterweight`, `IsSentry`, and `IsTrap` markers. Their post-edit build and reflection evidence
are verified: the Projectile build exited `0` with `0` warnings and `0` errors, and the read-only
reflection check confirmed value-type layout, bool fields, default `false`, and explicit `true`
construction for all three markers.

### Source implementation checkpoint: counterweight capability marker

The following source file was saved for the narrow `counterweight` capability boundary:

- `src/Projectile/ProjectileCounterweightCapabilityComponent.cs`

`ProjectileCounterweightCapabilityComponent` stores only the explicit `IsCounterweight` marker and
defaults it to `false`. It does not infer attachment ownership, player accounting, projectile
spawning, rendering, network replication, or cleanup. The subsequent serial Projectile build and
read-only reflection check passed; the marker remains storage-only.

### Source implementation checkpoint: sentry capability marker

The following source file was saved for the narrow `sentry` capability boundary:

- `src/Projectile/ProjectileSentryCapabilityComponent.cs`

`ProjectileSentryCapabilityComponent` stores only the explicit `IsSentry` marker and defaults it to
`false`. It does not infer DD2 state, Player ownership, lifetime persistence, placement, rendering,
network replication, or cleanup. The subsequent serial Projectile build and read-only reflection
check passed; the marker remains storage-only.

### Source implementation checkpoint: trap capability marker

The following source file was saved for the narrow `trap` capability boundary:

- `src/Projectile/ProjectileTrapCapabilityComponent.cs`

`ProjectileTrapCapabilityComponent` stores only the explicit `IsTrap` marker and defaults it to
`false`. It does not infer world ownership, NPC damage, tile interaction, rendering, network
replication, or cleanup. The subsequent serial Projectile build and read-only reflection check
passed; the marker remains storage-only.

## 1. Scope and Status

This document covers only the 17 leaf groups and 126 members listed in the P15 authoritative
report under the formal parent `ProjectileSimulation`. It is a proposed decomposition and an
implementation input, not a code migration, behavior-equivalence claim, API-compatibility claim,
or network/persistence closure claim.

All types, paths, interfaces, systems, queries, commands, adapters, and projections proposed here
remain `status: proposed` unless explicitly listed in the source-only implementation checkpoint.
Existing NLTX files are evidence of current partial coverage only. The implementation checkpoint
does not replace a legacy API, registration key, network field, persistence field, or runtime writer.

All 17 report leaf groups and 126 members have now been reviewed in report order. The checkpoint
fields at the top of this file were updated together with the execution document after each group;
the full design remains proposed and the implementation is partial because only narrow source units
have been added; behavior-equivalence verification was not run.

## 2. Evidence Register

| Source | Actual evidence | Fact supported | evidenceStatus |
|---|---|---|---|
| Version4 | `D:\TRbackup\Version4\Terraria\Projectile.cs:32-386` | `Projectile`, nested storm value type, 118 field declarations, 8 properties, and their defaults/types | confirmed |
| Version4 | `D:\TRbackup\Version4\Terraria\Projectile.cs:388-418` | static NPC immunity initialization and NPC-slot reset touch every projectile's local immunity | confirmed |
| Version4 | `D:\TRbackup\Version4\Terraria\Projectile.cs:444-556` | `SetDefaults` resets arrays, flags, identity, lifetime, collision, damage, source and network state | confirmed |
| Version4 | `D:\TRbackup\Version4\Terraria\Projectile.cs:10204-10343` | slot reuse, `whoAmI`, owner/identity/UUID initialization, source application and AI initialization | confirmed |
| Version4 | `D:\TRbackup\Version4\Terraria\Projectile.cs:11465-11900` | owner hit checks, damage eligibility, local/static immunity, collision and hit-side writes | confirmed |
| Version4 | `D:\TRbackup\Version4\Terraria\Projectile.cs:13654-14220` | specialized collision reads raw AI, rotation, scale, trail geometry and world collision | confirmed |
| Version4 | `D:\TRbackup\Version4\Terraria\Projectile.cs:14693-15290` | tick/substep update, AI, lifetime/environment checks, network skipped-section recovery | confirmed |
| Version4 | `D:\TRbackup\Version4\Terraria\Projectile.cs:15292-15314` | player and local NPC immunity countdown writers | confirmed |
| Version4 | `D:\TRbackup\Version4\Terraria\Projectile.cs:18525-18548` | owner-scoped UUID lookup and desperation handling | confirmed |
| Version4 | `D:\TRbackup\Version4\Terraria\Projectile.cs:18616-18664` | source adapters for child projectile, item drop and on-hit provenance | confirmed |
| Version4 | `D:\TRbackup\Version4\Terraria\Projectile.cs:46425-46450` | `Kill` clears owner/identity registry and starts terminal effects | confirmed |
| Version4 | `D:\TRbackup\Version4\Terraria\Main.cs:944-946,3468-3483` | fixed projectile slot array, owner/identity map, and slot identity initialization | confirmed |
| Version4 | `D:\TRbackup\Version4\Terraria\MessageBuffer.cs:1320-1490` | projectile spawn/update/kill network input matches owner plus identity and restores raw fields | confirmed |
| Version4 | `D:\TRbackup\Version4\Terraria\NetMessage.cs:771-841,1762-1808,2689-2692` | serialized projectile fields, section visibility, skip flags, and owner resync | confirmed |
| Version4 | `D:\TRbackup\Version4\Terraria\Player.cs:6275-6386,19752-19754,25661-25679` | player-owned scans, minion respawn/kill, bobber ownership and projectile creation | confirmed |
| Version4 | `D:\TRbackup\Version4\Terraria\NPC.cs:20047-20052,23360-23379,52907` | NPC-side lifetime/AI/friendly writes and reflection reads | confirmed |
| Current NLTX | `D:\TRbackup\NLTX\src\Projectile\` | root component prototypes exist, with identity, definition, behavior, lifetime, damage, immunity and network partial coverage | existing-evidence |
| Current NLTX | `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Projectile\Systems\ProjectileSpawnSystem.cs:18-231` | current ECS spawn composes broad definition, identity, behavior, lifetime, damage, capability and network types | existing-evidence |
| Current NLTX | `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Projectile\Systems\ProjectileBehaviorSystem.cs:11-79` | current behavior system dispatches by behavior ID and writes location/velocity/behavior state | existing-evidence |
| Current NLTX | `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Projectile\Systems\ProjectileReplicationSystem.cs:31-152` | current snapshot is one-way but still reads the broad definition and multiple partial components | existing-evidence |
| Current NLTX | `D:\TRbackup\NLTX\dome\Test\Terraria.Dome.Combat.Verification\Program.cs:779-8257` | existing focused verifier scenarios for identity, immunity, lifetime, collision, damage, network and specialized projectile paths | existing-evidence |
| tModLoader stable mirror | `D:\TRbackup\tmodloader-api-docs-stable\index.html` and `class_projectile.html` | page version `v2026.07`; public `Projectile`/`ModProjectile` API describes raw AI synchronization, source stat transfer, identity, damage and lifetime boundaries | confirmed-public-boundary |
| SS14 reference | `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Weapons\Ranged\Components\TargetedProjectileComponent.cs:1-11` and `Content.Server\Projectiles\ProjectileSystem.cs:15-105` | organization reference: narrow component data, event/system-owned collision, damage and penetration side effects | organization-only |

The source search found Version4 rather than a missing declaration, so the report is not blocked by
a missing authoritative type. It remains `partial` because a complete writer/lifecycle matrix for
every cache, presentation field, persistence field and external protocol field was not run in this
document session.

## 3. Boundary Decision

The legacy `Terraria.Projectile` object combines five different ownership kinds: per-entity
authority, immutable content definition, pure/derived queries, protocol projection state and
presentation compatibility state. The proposed boundary is therefore capability-first and access-
pattern-first, not one component per legacy field cluster.

```text
spawn intent/source
  -> ProjectileSpawnSystem
  -> identity + definition reference + initial behavior/lifetime/damage state
  -> ProjectileBehaviorSystem / ProjectileMovementSystem
  -> collision and target queries
  -> ProjectileCombatSystem
  -> explicit hit, child-spawn, item-drop or destroy commands
  -> terminal commit
  -> network/persistence/presentation projections
```

The following boundaries are mandatory:

- `EntityReference`, legacy owner slot, owner-scoped identity, `projUUID`, runtime entity ID,
  replication ID and any future persistent ID remain distinct. `ProjectileIdentityComponent` does
  not become a global reverse index.
- `ProjectileDefinition` is immutable per catalog revision. Mutable `friendly`/`hostile`, damage,
  penetration, immunity counters and lifetime are not silently written back to the definition.
- Query code is read-only and deterministic. Static mutable lists and hitbox workspaces in the
  legacy class become per-call workspaces or explicit cache owners; they do not become global ECS
  query state.
- Collision and damage systems emit commands or committed events. They do not directly mutate
  Player, NPC, Item, Tile or network transport internals through a projectile component.
- Network, persistence, localization, audio and rendering use adapters/projections. Serialized
  packets, byte writers, file DTOs and UI types do not enter authoritative components.

## 4. First Checkpoint: Identity and Classification

`ProjectileIdentityAndClassificationState` is complete as a source-member checkpoint. The report's
16 members are all mapped below; the proposed owner is intentionally split where the legacy class
clusters identity, capability, definition and presentation data together.

| Member | C# type | Proposed classification | Proposed owner/seam | Evidence status |
|---|---|---|---|---|
| `active` | `bool` | authoritative lifecycle state | `ProjectileLifetimeStateComponent`; only lifecycle/terminal commit writes | confirmed |
| `perIDStaticNPCImmunity` | `uint[][]` | shared authority registry, not entity component | `ProjectileNpcImmunityRegistryAdapter` behind combat seam; reset by explicit NPC-slot lifecycle | partial |
| `ownerHitCheckDistance` | `float` | authoritative targeting policy | `ProjectileTargetingPolicyComponent`; read by owner-hit query, written by definition/spawn or explicit policy command | partial |
| `arrow` | `bool` | definition/capability classification | `ProjectileDefinitionCatalog` or immutable classification value; not mutable identity | partial |
| `numHits` | `int` | authoritative combat counter | `ProjectilePenetrationStateComponent`; written only by combat commit | partial |
| `bobber` | `bool` | optional capability marker | `ProjectileBobberCapabilityComponent`; fishing system owns behavior | partial |
| `netImportant` | `bool` | network policy state | `ProjectileNetworkReplicationState` projection input; transport owns delivery | confirmed |
| `noDropItem` | `bool` | source/drop compatibility metadata | `ProjectileSourceMetadataComponent`; drop system reads it, projectile does not create Item directly | confirmed |
| `counterweight` | `bool` | optional capability marker | `ProjectileCounterweightCapabilityComponent`; attachment owner is integration-review | partial |
| `scale` | `float` | per-instance geometry state | `ProjectileGeometryStateComponent`; collision query consumes it, shared collider remains separate | confirmed |
| `rotation` | `float` | authoritative motion/geometry state | `ProjectileTrajectoryStateComponent`; movement/AI system writes | confirmed |
| `type` | `int` | definition/catalog reference | `ProjectileDefinitionReferenceComponent`; catalog owns type semantics | confirmed |
| `alpha` | `int` | presentation/compatibility state | `ProjectilePresentationStateProjection` or deferred compatibility state; no final authority yet | partial |
| `sentry` | `bool` | optional capability marker | `ProjectileSentryCapabilityComponent`; Player/DD2 integration-review | partial |
| `glowMask` | `short` | presentation metadata | presentation adapter/projection; never a combat component | partial |
| `owner` | `int` | legacy owner reference | `ProjectileIdentityComponent` through a validated `ProjectileOwnerReference` value; 255/NPC/trap/world cases require integration-review | confirmed |

The first checkpoint does not merge `owner` with `identity` or `projUUID`, and it does not make
`active` a second identity flag. `alpha` and `glowMask` are intentionally deferred from the
authoritative component set until their runtime writers and client compatibility contract are
confirmed.

### Source implementation checkpoint: owner and identity value boundary

The following source files now implement the evidence-closed identity value boundary:

- `src/Projectile/ProjectileOwnerReference.cs`
- `src/Projectile/ProjectileIdentityComponent.cs`

`ProjectileOwnerReference` keeps the ECS `EntityReference` and the legacy owner slot distinct. The
slot accepts the Version4 player/NPC-world sentinel range `0..255`; `255` remains an explicit
unassigned/compatibility value and is not converted into an entity reference. `ProjectileIdentityComponent`
keeps projectile slot, owner-scoped identity, and optional `projUUID` separate, validates their
sentinel/range contracts, and exposes pure `MatchesOwnerAndIdentity` and
`MatchesNetworkIdentity` checks. These checks do not read or write a global reverse index and do not
perform terminal cleanup.

The implementation deliberately does not claim the following members as completed: `active`,
`perIDStaticNPCImmunity`, `ownerHitCheckDistance`, `arrow`, `numHits`, `bobber`, `netImportant`,
`noDropItem`, `counterweight`, `scale`, `rotation`, `type`, `alpha`, `sentry`, `glowMask`, and the
runtime allocation/cleanup writer for `owner`.

The immutable type/catalog boundary is also implemented in
`src/Projectile/ProjectileDefinitionReferenceComponent.cs`. It owns only the non-negative projectile
type and catalog revision reference; it does not contain friendly/hostile disposition, mutable AI,
damage, lifetime, network scheduling, or capability flags.

The identity/value checkpoint was built and focused-verified with exit code `0`. It covered
owner/slot/identity/UUID separation, default sentinels, range rejection, owner/identity matching,
optional UUID matching, and immutable type/catalog reference validation. The identity group remains
partial because its lifecycle writers, reverse-index cleanup, duplicate network rejection,
classification capability writers, and presentation projection writers are not implemented.

### Source implementation checkpoint: runtime disposition boundary

The following source files now implement the narrow runtime disposition boundary:

- `src/Projectile/ProjectileDispositionStateComponent.cs`
- `src/Projectile/ProjectileDispositionSystem.cs`

`ProjectileDispositionStateComponent` owns instance `Friendly` and `Hostile` values separately from
the immutable/catalog-facing fields in `ProjectileDefinitionComponent`.
`ProjectileDispositionSystem.InitializeFromDefinition` copies the compatibility definition defaults
once at spawn composition, while `SetFriendly` and `SetHostile` are explicit writers for later
runtime decisions. This source unit does not read global state, perform network ingress, mutate
Player/NPC state, or perform terminal cleanup. It does not claim the remaining identity
classification fields or compatibility projection writers.

### Source implementation checkpoint: penetration state boundary

The following source files now implement the finite penetration state boundary:

- `src/Projectile/ProjectilePenetrationStateComponent.cs`
- `src/Projectile/ProjectilePenetrationSystem.cs`

`ProjectilePenetrationStateComponent` keeps remaining penetration, the initialized maximum,
accepted-hit count, and the stop-damage policy separate from damage payload and target state. It
preserves the observed `-1` unlimited sentinel, rejects values below `-1` and negative hit counts,
and exposes read-only `HasRemainingHits`/`IsUnlimited` views. `ProjectilePenetrationSystem` is the
single writer for initialization, remaining-hit updates, and accepted-hit counting. An accepted hit
increments `HitCount` and decrements finite `RemainingHits`; an already depleted state is rejected
without mutation.

This source unit does not decide whether a target is valid or immune, mutate NPC/Player health,
write local/static immunity, publish a hit event, or commit projectile termination. The
`StopsDealingDamageWhenDepleted` policy is retained as state for a later combat commit boundary;
this unit does not infer the legacy type-specific hit behavior.

### Source implementation checkpoint: reflection result boundary

The following source files now implement the narrow reflection result boundary:

- `src/Projectile/ProjectileReflectionStateComponent.cs`
- `src/Projectile/ProjectileReflectionSystem.cs`

`ProjectileReflectionStateComponent` owns only the instance-level `Reflected` result. The system
provides an explicit mark writer and an explicit compatibility reset writer. No reflected velocity,
target mutation, damage result, NPC rule, network packet, or terminal transition is inferred from
this boolean.

### Source implementation checkpoint: per-projectile immunity state boundary

The following source files now implement the per-projectile immunity state boundary:

- `src/Projectile/ProjectileHitImmunityPolicyComponent.cs`
- `src/Projectile/ProjectileHitImmunityStateComponent.cs`
- `src/Projectile/ProjectileHitImmunitySystem.cs`

`ProjectileHitImmunityPolicyComponent` preserves the Version4 policy sentinels: `-2` means that a
local cooldown policy does not write the per-projectile local array, and `-1` remains a valid
local-immunity value. `ProjectileHitImmunityStateComponent` owns only the local NPC cooldown array,
the player cooldown array, and `RestrikeDelayTicks`; its explicit constructor rejects invalid
capacities. `ProjectileHitImmunitySystem` is the single writer for setting/resetting those arrays,
setting restrike delay, and advancing positive cooldowns. Local `-1` values are retained while
positive values decrement; player cooldowns reject negative values and decrement only while positive.

This source unit deliberately does not model `perIDStaticNPCImmunity`, NPC/Player target state,
owner melee cooldowns, damage, reflection, network messages, or terminal cleanup. Those remain
explicit adapter/commit responsibilities in the combat schedule.

### Source implementation checkpoint: trail cache boundary

The following source files now implement the narrow projectile history-cache boundary:

- `src/Projectile/ProjectileTrailCacheComponent.cs`
- `src/Projectile/ProjectileTrailCacheSystem.cs`

`ProjectileTrailCacheComponent` owns the caller-sized `oldPos`, `oldRot`, and
`oldSpriteDirection` history arrays plus the caller-owned `WhipPoints` workspace. The constructor
rejects negative history lengths. `ProjectileTrailCacheSystem.Record` shifts the three histories
together and writes the newest position, rotation, and sprite direction at index zero;
`ProjectileTrailCacheSystem.Reset` clears the arrays and whip-point workspace. This boundary does
not generate specialized whip control points, smooth trails, compensate player displacement,
query the world, or publish presentation effects. The affected Projectile project build completed
with exit code `0`, `0` warnings, and `0` errors, producing
`Build/bin/Terraria.Projectile/Debug/net10.0/Terraria.Projectile.dll`. The focused verifier ran
through the serial wrapper with `--project` and completed with exit code `0`; it covered history
record/reset and array-boundary behavior. This evidence verifies only the narrow cache boundary,
not specialized trail behavior or collision integration.

### Source implementation checkpoint: collision policy writer boundary

The following source files now implement the narrow collision-policy writer boundary:

- `src/Projectile/ProjectileCollisionPolicyComponent.cs`
- `src/Projectile/ProjectileCollisionPolicySystem.cs`

`ProjectileCollisionPolicyComponent` preserves the twelve policy/instance fields for tile
collision, liquid handling, slope handling, manual fall-through, tile reflection/bounce inputs,
owner self-hit qualification, and behavior-controlled direction changes. Its constructor rejects
negative bounce capacity, non-finite or negative bounce values, and non-finite or negative owner-hit
distance. `ProjectileCollisionPolicySystem` exposes explicit writers for each policy family,
validates numeric updates, and computes effective fall-through only when both
`DecidesManualFallThrough` and `ShouldFallThrough` are true. It does not own Tile, liquid,
WorldGrid, broadphase, target colliders, velocity, rotation, or reflection/bounce physics. The
Projectile project build completed with exit code `0`, `0` warnings, and `0` errors, producing
`Build/bin/Terraria.Projectile/Debug/net10.0/Terraria.Projectile.dll`. The focused verifier ran
through the serial wrapper with explicit `--project` and completed with exit code `0`; it covered
writer validation, fall-through qualification, bounce boundaries, and owner-hit distance
boundaries. This evidence verifies only the policy writer boundary, not world collision, liquid
contact, bounce/reflect physics, or target checks.

### AI state implementation checkpoint

The following source files implement the narrow raw/local AI state boundary:

- `src/Projectile/ProjectileBehaviorStateComponent.cs`
- `src/Projectile/ProjectileBehaviorStateResetSystem.cs`

The component preserves Version4's three synchronized `ai` slots and three local `localAI` slots,
rejects non-finite values, and exposes slot reads without global state. `aiStyle` is deliberately
not copied into this mutable state component; the behavior key is represented by
`ProjectileDefinitionReferenceComponent.BehaviorKey`. The reset system zeroes both arrays through
the component's explicit writer. It does not dispatch behavior, write `netUpdate`, read the world,
or serialize local state.

### Lifetime and runtime state implementation checkpoint

The following source files implement the narrow lifetime/runtime state boundary:

- `src/Projectile/ProjectileLifetimeDefinition.cs`
- `src/Projectile/ProjectileLifetimeStateComponent.cs`
- `src/Projectile/ProjectileLifetimeSystem.cs`
- `src/Projectile/ProjectileEffectCooldownStateComponent.cs`
- `src/Projectile/ProjectileEffectCooldownSystem.cs`
- `src/Projectile/ProjectileTrajectoryStateComponent.cs`

`ProjectileLifetimeDefinition` preserves the confirmed `SentryLifeTime` and `ArrowLifeTime`
definition constants. `ProjectileLifetimeStateComponent` keeps `Active` and `TimeLeft` as separate
fields but enforces the terminal invariant that a non-`None` end reason is inactive with zero time.
`ProjectileLifetimeSystem` is the only writer in this source unit for decrement and terminal commit;
repeated terminal commits are idempotent. The effect cooldown state preserves the Version4 `soundDelay`
domain, including the observed `-1` sentinel, while its system decrements only positive delays.
`ProjectileTrajectoryStateComponent` now carries `GfxOffY` alongside the existing `StepSpeed`; it
does not add a duplicate position, velocity, or lifetime authority.

This checkpoint deliberately does not claim integration with the legacy/root `ProjectileLifetimeComponent`,
entity storage, `Main.projectileIdentity`, owner-loss/environment checks, network tombstones, child/drop/
channel effects, or runtime schedule ordering. `gfxOffY` movement clamping and `soundDelay` behavior
writers beyond the explicit decrement/set boundary remain unverified.

The affected Projectile project was then built serially through the repository wrapper with exit code
`0`, `0` warnings, and `0` errors. The verified artifact is
`Build/bin/Terraria.Projectile/Debug/net10.0/Terraria.Projectile.dll`. A focused reflection verifier
loaded that artifact and passed with exit code `0`; it covered the two definition constants,
active/timeLeft separation, expiry transition, idempotent terminal commit, invalid lifetime and
cooldown rejection, positive cooldown decrement, the `-1` cooldown sentinel, and trajectory
`GfxOffY`/`StepSpeed` retention. This does not verify runtime scheduling, identity cleanup, network
tombstones, owner/environment checks, or full Version4 behavior equivalence.

## 5. Second Checkpoint: AI State

`ProjectileAiState` is complete for its narrow source implementation unit. Raw `ai` is synchronized
state in the Version4 protocol, while `localAI` is local runtime state and `aiStyle` is a
behavior-definition key held by `ProjectileDefinitionReferenceComponent.BehaviorKey`.
`maxAI` is a shared array-shape constant, not a per-entity data column.

| Member | C# type | Proposed classification | Proposed owner/seam | Evidence status |
|---|---|---|---|---|
| `maxAI` | `int` | definition/compatibility cardinality | `ProjectileBehaviorCatalog`; fixed shape validated at spawn and protocol boundary | confirmed |
| `ai` | `float[]` | authoritative, network-visible behavior state | `ProjectileBehaviorStateComponent`; `ProjectileBehaviorSystem` writes, network ingress uses validated command | confirmed |
| `localAI` | `float[]` | authoritative local behavior state | same component or a local-only sibling; never serialized as `ai` | confirmed |
| `aiStyle` | `int` | immutable behavior registry key | `ProjectileDefinitionReferenceComponent` plus `IProjectileBehavior` registry | confirmed |

The Version4 evidence is concrete: `SetDefaults` resets both arrays at lines 485-489; `NewProjectile`
sets the initial AI values and UUID-related substitutions at lines 10301-10343; `Update` consumes
`aiStyle`, `ai`, `localAI` and writes `netUpdate` at lines 14702-14806; `MessageBuffer` restores
`ai` from a packet at lines 1384-1399; `NetMessage` conditionally serializes the three public AI
slots at lines 779-821. The public tModLoader page independently describes `ai` as synchronized and
`localAI` as non-synchronized, but that page is only public-boundary evidence, not private behavior
proof.

The existing NLTX `ProjectileBehaviorSystem` is a useful partial seam: it indexes behavior IDs and
writes location/velocity/behavior state (`dome/src/Terraria.Dome.Simulation/Projectile/Systems/ProjectileBehaviorSystem.cs:11-79`).
It must not treat all raw AI slots as a generic behavior object without preserving the legacy
per-type meaning and reset contract. Unknown behavior IDs fail closed; they must not mutate the
entity or fall back to a different AI style.

## 6. Third Checkpoint: Lifetime and Runtime State

`ProjectileLifetimeAndRuntimeState` is the current implementation unit. The lifecycle owner is the
only writer of terminal activity and remaining lifetime; movement and presentation consume the
other runtime values through explicit seams. The source implementation must preserve the distinction
between `active` and `timeLeft` and must not turn definition constants into per-instance countdowns.

| Member | C# type | Proposed classification | Proposed owner/seam | Evidence status |
|---|---|---|---|---|
| `SentryLifeTime` | `int` constant | definition policy | sentry lifetime definition/catalog | confirmed |
| `ArrowLifeTime` | `int` constant | definition policy | arrow lifetime definition/catalog | confirmed |
| `gfxOffY` | `float` | derived/presentation-compatible movement offset | movement projection seam; no independent lifetime authority | partial |
| `stepSpeed` | `float` | authoritative movement step parameter | `ProjectileMovementStateComponent`; movement system writes or derives | confirmed |
| `timeLeft` | `int` | authoritative lifecycle countdown | `ProjectileLifetimeStateComponent`; lifecycle system only writer | confirmed |
| `soundDelay` | `int` | authoritative transient effect throttle | `ProjectileEffectCooldownStateComponent` or explicit sound adapter state | partial |

`SetDefaults` establishes the default lifetime and resets `soundDelay`, `stepSpeed` and related
state (`Projectile.cs:444-556`). `NewProjectile` sets `gfxOffY` and `stepSpeed` after slot reuse
(`Projectile.cs:10270-10282`). `Update` runs extra substeps, decrements sound delay, performs
environment/owner/lifetime checks and can terminate the projectile (`Projectile.cs:14693-14845`).
The current dome has separate lifetime, step-speed and sound-delay components, which is useful
partial evidence, but the exact terminal ordering and `gfxOffY` presentation semantics are not
closed.

The lifecycle commit must distinguish `active=false`, `timeLeft=0`, identity-map cleanup and
network tombstone publication. `Kill` is a command/commit operation, not a property setter. Sentry
and arrow constants stay in definitions; they do not become mutable entity countdowns.

## 7. Fourth Checkpoint: Combat and Immunity

`ProjectileCombatAndImmunity` is complete for its 14 report members. The report grouping contains
both combat payload and identity, so the assignment below deliberately points `identity` back to
the single identity component rather than creating a second combat identity.

| Member | C# type | Proposed classification | Proposed owner/seam | Evidence status |
|---|---|---|---|---|
| `damage` | `int` | mutable authoritative damage payload | `ProjectileDamagePayloadComponent`; combat commit only | confirmed |
| `originalDamage` | `int` | spawn snapshot/compatibility payload | same damage component; source/spawn initializes, scaling reads | confirmed |
| `spriteDirection` | `int` | authoritative direction state | `ProjectileAnimationAndDirectionComponent`; movement/animation writer | partial |
| `hostile` | `bool` | mutable disposition state | `ProjectileDispositionComponent` proposed; combat eligibility reads | partial |
| `reflected` | `bool` | authoritative combat/trajectory outcome | `ProjectileReflectionStateComponent` or collision state; NPC reflection command writes | confirmed |
| `knockBack` | `float` | damage payload | `ProjectileDamagePayloadComponent`; hit calculation reads | confirmed |
| `friendly` | `bool` | mutable disposition state | `ProjectileDispositionComponent` proposed; never silently aliases catalog default | confirmed |
| `penetrate` | `int` | remaining penetration state | `ProjectilePenetrationStateComponent`; accepted hit/tile commit writes | confirmed |
| `localNPCImmunity` | `int[]` | per-projectile target cooldown state | `ProjectileHitImmunityStateComponent`; immunity system decrements/resets | confirmed |
| `usesLocalNPCImmunity` | `bool` | immunity policy definition | `ProjectileHitImmunityPolicyDefinition` or immutable policy component | confirmed |
| `usesIDStaticNPCImmunity` | `bool` | immunity policy definition | static registry adapter plus policy query | confirmed |
| `appliesImmunityTimeOnSingleHits` | `bool` | immunity policy definition | combat policy; accepted-hit writer | confirmed |
| `maxPenetrate` | `int` | derived/initialized penetration limit | penetration state initialized from definition; no second damage counter | partial |
| `identity` | `int` | owner-scoped identity | existing `ProjectileIdentityComponent`; combat systems query it | confirmed |

Version4 `Damage` first checks eligibility, then reads local/static immunity and target state,
collides, may reflect, calculates armor/critical/knockback effects, and finally mutates combat and
target state (`Projectile.cs:11480-11900`). `DecrementLocalImmuneTimeCounters` writes player and
local NPC counters (`Projectile.cs:15292-15314`); `ResetNPCSlotData` clears all projectile-local
entries (`Projectile.cs:406-418`). NPC reflection reads and writes the projectile from the NPC
boundary (`NPC.cs:52901-52942`). These are separate seams, not permission for arbitrary cross-entity
field access.

The current dome has `ProjectileDamageComponent`, `ProjectilePenetrationComponent`,
`ProjectileFriendlyStateComponent`, `ProjectileReflectionComponent` and a shared
`HitImmunityComponent`. They are existing partial evidence. The dome simulation currently mutates
several of them from one large simulation path, so implementation must first establish one combat
commit port and prevent the broad `ProjectileDefinitionComponent` from becoming a second writer.

The key invariant is: an accepted hit may update target damage, projectile hit/penetration state,
immunity and terminal status as one auditable result; a rejected, immune, reflected or invalid hit
must not partially decrement penetration or apply target damage. `friendly` and `hostile` are
runtime facts and may differ from definition defaults; their final owner remains an integration
decision because NPC and network paths currently write them.

## 8. Fifth Checkpoint: Movement and Collision State

`ProjectileMovementAndCollisionState` is complete for its nine report members. Movement state is
kept separate from generic spatial authority, while collision policy is an input to spatial queries
and not ownership of the world grid.

| Member | C# type | Proposed classification | Proposed owner/seam | Evidence status |
|---|---|---|---|---|
| `oldPos` | `Vector2[]` | trajectory history/cache | `ProjectileTrailHistoryComponent`; movement system writes, presentation/collision queries read | confirmed |
| `oldRot` | `float[]` | trajectory history/cache | same history component; invalidated with lifetime/reset | confirmed |
| `oldSpriteDirection` | `int[]` | trajectory history/cache | same history component; animation projection reads | confirmed |
| `restrikeDelay` | `int` | combat transient cooldown | `ProjectileHitImmunityStateComponent` or separate cooldown component; combat system writes | confirmed |
| `tileCollide` | `bool` | mutable collision policy state | `ProjectileCollisionPolicyComponent`; collision command/system writes | confirmed |
| `extraUpdates` | `int` | update-budget definition/compatibility state | definition reference or lifecycle budget component; scheduler consumes | confirmed |
| `stopsDealingDamageAfterPenetrateHits` | `bool` | combat policy definition | penetration/damage policy, not movement state | confirmed |
| `numUpdates` | `int` | per-tick runtime counter | lifecycle scheduler state; reset at each `Update` pass | confirmed |
| `ignoreWater` | `bool` | environment collision policy | `ProjectileCollisionPolicyComponent`; liquid adapter reads | confirmed |

`SetDefaults` resizes and clears the three history arrays, resets collision/update policy and
`numUpdates` (`Projectile.cs:467-489,519-526`). `Update` copies velocity, runs AI and environment
checks, performs tile/liquid collision and later movement (`Projectile.cs:14795-14806,14820-14946,
15513-15680`). `Colliding` consumes scale, rotation, raw AI and history for specialized shapes
(`Projectile.cs:13654-13760`). The current dome `ProjectileCollisionSystem` is a pure world-grid
query with explicit transforms/colliders (`dome/src/Terraria.Dome.Simulation/Projectile/Systems/ProjectileCollisionSystem.cs:9-160`),
which is the correct direction, but its final seam with Version4 movement and liquid behavior is
not yet behavior-equivalence evidence.

The proposed movement writer must publish a collision result rather than directly editing Tile or
Liquid state. Sweep queries, slope correction, manual fall-through, wet/honey/lava/shimmer effects
and specialized trail geometry require explicit inputs and deterministic invalidation. `extraUpdates`
and `numUpdates` must not be confused: the former is a policy/budget, the latter is a per-tick
execution counter.

## 9. Sixth Checkpoint: Network Replication State

`ProjectileNetworkReplicationState` is complete for its four report members. These fields are
per-projectile replication scheduling state, not serialized protocol objects and not the identity
used to find the entity.

| Member | C# type | Proposed classification | Proposed owner/seam | Evidence status |
|---|---|---|---|---|
| `netUpdate` | `bool` | authoritative dirty/request flag | projectile systems request; network scheduler consumes and clears at send boundary | confirmed |
| `netUpdate2` | `bool` | secondary/deferred dirty flag | network scheduling policy; exact meaning requires compatibility verifier | partial |
| `netSpam` | `int` | network throttling state | network scheduler owns, bounded by explicit policy | confirmed |
| `netSyncSkippedForPlayer` | `bool[]` | section visibility retry state | network/session adapter owns per-player map; array index is not projectile identity | confirmed |

`NewProjectile` resets all four network values (`Projectile.cs:515-518`). `Update` clears
`netUpdate` around AI and later movement, while `RecheckSectionsForSkippedUpdates` sends a retry
only when a player's section becomes active (`Projectile.cs:14795-14806,15278-15290`). `NetMessage`
serializes the projectile identity and selected fields (`NetMessage.cs:771-841`) and separately
handles section visibility and skipped updates (`NetMessage.cs:1762-1808`). `MessageBuffer` matches
network updates by owner plus identity and applies kill by the same pair (`MessageBuffer.cs:1357-1402,
1446-1448`).

The current dome `ProjectileNetworkUpdateComponent` and `ProjectileReplicationSystem` are existing
partial coverage. The replication projection currently reads identity, owner, behavior, damage,
lifetime, definition and network state (`dome/src/Terraria.Dome.Simulation/Projectile/Systems/ProjectileReplicationSystem.cs:31-152`).
It must remain one-way: packet parsing creates validated commands, snapshot projection observes
committed state, and neither is allowed to mutate authoritative Projectile state directly.

The key invariants are: stale or duplicate network input cannot replace a different owner/identity;
section-skipped state is cleared only after a successful eligible send; `netSpam` cannot grow
without a bound; and terminal cleanup publishes a tombstone or equivalent before the entity slot is
reused. Replication ID, packet bytes and connection/session state remain outside this component.

### Source implementation checkpoint: network state writer boundary

The following source files now implement the in-memory network scheduling state boundary:

- `src/Projectile/ProjectileNetworkStateComponent.cs`
- `src/Projectile/ProjectileNetworkStateSystem.cs`

The component validates non-negative player capacity and `netSpam`. The system exposes explicit
primary/secondary update requests, applies the observed `60` send-budget threshold and `+5`
increment, decrements positive spam once per tick, and preserves deferred requests when the budget
is saturated. Per-player section skip flags are bounds-checked and are cleared only through the
explicit clear writer intended for a successful eligible send. Reset clears all state. This unit
does not parse or serialize packets, match owner/identity, inspect active sections or connections,
publish transport effects, or create network tombstones.

## 10. Seventh Checkpoint: Minion and Presentation State

`ProjectileMinionAndPresentationState` is complete for its 13 report members. The legacy grouping
contains summon accounting, presentation compatibility and player-target immunity; these are not a
single component and are intentionally split below.

| Member | C# type | Proposed classification | Proposed owner/seam | Evidence status |
|---|---|---|---|---|
| `light` | `float` | presentation/environment output | lighting/presentation projection; projectile core exposes read-only value | partial |
| `minion` | `bool` | optional authoritative capability | `ProjectileMinionCapabilityComponent`; Player summon system owns accounting | confirmed |
| `minionSlots` | `float` | owner accounting input | minion capability state; PlayerGameplay integration-review | confirmed |
| `minionPos` | `int` | owner-scoped derived/accounting state | minion system; not a target or entity identity | confirmed |
| `isAPreviewDummy` | `bool` | presentation/tool compatibility state | preview adapter/projection; excluded from authoritative simulation | partial |
| `isAPreviewDisplayDoll` | `bool` | presentation/tool compatibility state | preview adapter/projection; excluded from authoritative simulation | partial |
| `MinionSpawnInfo` | `Terraria.DataStructures.MinionSpawnInfo` | source metadata/value object | `ProjectileSourceMetadataComponent` via stable domain value; external type at adapter edge | confirmed |
| `drawLayer` | `int` | presentation state | client presentation projection | partial |
| `usesOwnerLight` | `bool` | presentation policy | presentation/lighting adapter | partial |
| `hide` | `bool` | presentation state | client projection; no combat or lifetime authority | partial |
| `ownerHitCheck` | `bool` | authoritative targeting policy | `ProjectileTargetingPolicyComponent`; owner hit query reads | confirmed |
| `usesOwnerMeleeHitCD` | `bool` | combat policy definition | hit-immunity/combat policy | confirmed |
| `playerImmune` | `int[]` | per-projectile player target cooldown | `ProjectileHitImmunityStateComponent`; combat system decrements and writes | confirmed |

`Update` uses minion/sentry ownership to recalculate damage and minion slots, and can kill or move a
minion when owner capacity or lifecycle changes (`Projectile.cs:14711-14768`). `Player` scans owned
projectiles and handles minion/bobber lifecycle (`Player.cs:6275-6386,19752-19754,25661-25679`).
`UpdateEnchantmentVisuals` and `EmitEnchantmentVisualsAt` read owner and presentation flags and emit
visual/light effects (`Projectile.cs:15327-15406`). `DecrementLocalImmuneTimeCounters` decrements
player immunity (`Projectile.cs:15292-15303`).

The current dome has minion/summoned components, a minion spawn source, owner-hit and player
immunity-related partial paths. They must not make Player's summon counters or target NPC a second
projectile authority. Preview dummy/display-doll flags, draw layer, hide, light and owner-light are
projection/compatibility data; they are explicitly excluded from the authoritative ECS core.

## 11. Eighth Checkpoint: Damage and Element State

`ProjectileDamageAndElementState` is complete for its 13 report members. The group mixes source
provenance, combat class, element/visual restrictions, trap/NPC classification and tag modifiers;
the proposed design keeps those concepts separate while preserving their shared hit-calculation
input boundary.

| Member | C# type | Proposed classification | Proposed owner/seam | Evidence status |
|---|---|---|---|---|
| `miscText` | `string` | source compatibility metadata | `ProjectileSourceMetadataComponent`; source adapter owns validation/encoding | confirmed |
| `melee` | `bool` | damage-class instance/default classification | `ProjectileDamagePayloadComponent` or immutable damage definition; combat query reads | confirmed |
| `ranged` | `bool` | damage-class instance/default classification | same damage-class seam; do not duplicate Item class state | confirmed |
| `magic` | `bool` | damage-class instance/default classification | same damage-class seam | confirmed |
| `coldDamage` | `bool` | element payload | `ProjectileDamagePayloadComponent`; status system consumes committed hit | confirmed |
| `noEnchantments` | `bool` | combat/effect policy | `ProjectileDamagePolicyComponent`; enchantment query reads | confirmed |
| `noEnchantmentVisuals` | `bool` | presentation/effect policy | effect/presentation projection input; no target state | confirmed |
| `trap` | `bool` | optional trap capability/source classification | `ProjectileTrapCapabilityComponent`; world interaction/combat integration-review | confirmed |
| `npcProj` | `bool` | owner/source classification | `ProjectileIdentityComponent` owner kind or source metadata; never inferred only from owner slot | confirmed |
| `originatedFromActivableTile` | `bool` | source provenance | `ProjectileSourceMetadataComponent`; Wiring/WorldInteraction integration-review | confirmed |
| `tagEffectType` | `int` | combat tag-effect payload | `ProjectileDamagePayloadComponent`; combat result command carries applied effect | confirmed |
| `bonusTagDamage` | `int` | combat modifier snapshot | `ProjectileDamagePayloadComponent`; source/spawn initializes, hit calculation reads | confirmed |
| `armorPenetration` | `int` | combat modifier snapshot | `ProjectileDamagePayloadComponent`; damage commit port consumes | confirmed |

`SetDefaults` resets all listed flags and source fields (`Projectile.cs:499-547`), while
`NewProjectile` applies source stats and provenance after initializing the instance
(`Projectile.cs:10270-10292,10536-10571`). `StatusNPC` applies enchantment and tag-related effects
to NPC targets (`Projectile.cs:10573-10630`), and `Damage_PVE_Inner` applies owner, class, armor,
reflection and target eligibility rules (`Projectile.cs:11594-11768`).

The current dome has a broad definition with damage class, tag, armor and source-compatible values,
plus separate friendly and minion-source components. It is existing partial evidence, not final
ownership. `miscText` and tile/NPC source flags must not become logs, network packet fields or world
structure authority. `melee`, `ranged` and `magic` should be represented by one validated damage
class value at the new seam; retaining legacy booleans is a compatibility view only.

The invariant is that combat calculations may read a stable projectile damage payload and emit a
typed hit result; status application, NPC/Player health mutation, tag progression and item effects
are downstream commit ports. No rejected hit may apply an element or tag side effect.

## 12. Ninth Checkpoint: Animation and Direction State

`ProjectileAnimationAndDirectionState` is complete for its three report members. Direction and frame
state are kept separate from damage and raw AI, while their compatibility relationship with movement
and client presentation is explicit.

| Member | C# type | Proposed classification | Proposed owner/seam | Evidence status |
|---|---|---|---|---|
| `frameCounter` | `int` | authoritative animation progression state | `ProjectileAnimationStateComponent`; behavior/animation system writes | confirmed |
| `frame` | `int` | authoritative animation frame | same animation component; presentation projection reads | confirmed |
| `manualDirectionChange` | `bool` | movement/animation policy | `ProjectileDirectionPolicyComponent`; behavior system reads, direction writer owns changes | confirmed |

`SetDefaults` resets frame and frame counter and direction-related policy (`Projectile.cs:455,499,
555-556`). `Update` runs AI and uses movement/direction behavior before the projectile is projected
or collided (`Projectile.cs:14795-14806`). `AutomaticallyChangesDirection` gates automatic changes
by `aiStyle` and `manualDirectionChange` (`Projectile.cs:15316-15325`). The legacy `spriteDirection`
value itself is in the combat grouping, but its ownership is animation/direction, not damage.

Current NLTX has a partial `ProjectileDirectionComponent` and behavior state. The proposed animation
component must not become a second raw-AI store or a rendering-only owner. Frame transitions should
be deterministic for a given committed tick/input; actual texture lookup remains a client projection.

The invariant is that a manual-direction policy suppresses only the automatic direction path required
by the legacy behavior; it does not silently alter hitbox geometry or damage class. Any geometry
coupling must be expressed by a query over committed direction/rotation state.

### Source implementation checkpoint: animation state boundary

The following source files now implement the narrow frame/counter state boundary:

- `src/Projectile/ProjectileAnimationStateComponent.cs`
- `src/Projectile/ProjectileAnimationStateSystem.cs`

`ProjectileAnimationStateComponent` owns only the non-negative `Frame` and `FrameCounter` values.
Its reset writer returns both values to the Version4 initialization state. The system provides
explicit frame/counter setters and a deterministic counter rollover for a caller-supplied positive
limit. It does not select an AI-specific cadence, read raw AI, change `manualDirectionChange`,
change `spriteDirection`, inspect texture layout, or mutate geometry, damage, or presentation state.

### Source implementation checkpoint: presentation state boundary

The following component was saved for the narrow presentation/compatibility state boundary:

- `src/Projectile/ProjectilePresentationStateComponent.cs`

`ProjectilePresentationStateComponent` stores the eight report members `Alpha`, `GlowMask`, `Light`,
`IsPreviewDummy`, `IsPreviewDisplayDoll`, `DrawLayer`, `UsesOwnerLight`, and `Hide`. It has no
rendering, texture, lighting, network, or effect side effects. The values remain component state
only; their runtime writers and client projection contract are not closed. The affected Projectile
project build and existing focused verifier were run after this checkpoint; reflection confirmed
the public fields in the resulting DLL. These checks do not verify presentation writers or client
projection.

## 13. Tenth Checkpoint: Collision and Targeting State

`ProjectileCollisionAndTargetingState` is complete for its seven report members. It is a boundary
group, not a license to merge projectile identity, target entity identity, collision geometry or
combat cooldowns.

| Member | C# type | Proposed classification | Proposed owner/seam | Evidence status |
|---|---|---|---|---|
| `projUUID` | `int` | owner-scoped relation/compatibility identity | `ProjectileIdentityComponent` UUID field; UUID lookup adapter, never slot/replication ID | confirmed |
| `correctSlopeCollision` | `bool` | collision policy definition/instance state | `ProjectileCollisionPolicyComponent`; spatial collision query consumes | confirmed |
| `decidesManualFallThrough` | `bool` | collision policy definition | same collision policy; behavior may issue explicit override command | confirmed |
| `shouldFallThrough` | `bool` | mutable collision policy state | collision/behavior system writes; world grid remains external | confirmed |
| `localNPCHitCooldown` | `int` | local immunity policy parameter | `ProjectileHitImmunityPolicyComponent`; value initializes local immunity rules | confirmed |
| `idStaticNPCHitCooldown` | `int` | static immunity policy parameter | static immunity registry adapter/policy | confirmed |
| `bannerIdToRespondTo` | `int` | source/target association metadata | `ProjectileSourceMetadataComponent` or combat association seam; progression owner integration-review | confirmed |

`NewProjectile` sets UUID when the content catalog requires it and rewrites special AI references
(`Projectile.cs:10330-10343`). `GetByUUID` resolves by owner and UUID (`Projectile.cs:18525-18548`).
`Colliding` reads AI, rotation, scale and old history to choose specialized shapes
(`Projectile.cs:13654-13760`), while damage checks tile visibility, owner hit range, immunity and
target eligibility before applying a result (`Projectile.cs:11594-11667`). `bannerIdToRespondTo`
is initialized and associated from the source (`Projectile.cs:459,10290-10292`).

The current dome has UUID/identity, fall-through, tile collision, restrike, Banner response and
immunity pieces distributed across components and simulation methods. This is partial coverage.
The proposed boundary must expose read-only target eligibility and collision queries and route all
mutations through typed hit, reflection, fall-through or banner commands. The target NPC/Player
identity and health remain owned by their own domains.

Required invariants: UUID lookup is owner-scoped and cannot resolve a stale slot; a fall-through
override is distinct from tile collision enabled; collision geometry never mutates target state; a
target rejected by range, immunity, friendliness, tile visibility or invalid identity produces no
damage/penetration/banner side effect; and Banner progression consumes a committed combat fact.

## 6. Proposed Module Vocabulary

The proposed target is domain-first. The current repository has existing flat `src/Projectile` and
`dome/src/Terraria.Dome.Simulation/Components/Projectile` files; migration may preserve namespaces
and compatibility paths initially, but new ownership boundaries should be organized under the
Projectile capability rather than a new global `Shared/Components` or `Common` directory.

| Module | Kind | Proposed target path | Interface / seam | Depth | Leverage | Locality |
|---|---|---|---|---|---|---|
| `ProjectileIdentityComponent` | Component | `dome/src/Terraria.Dome.Simulation/Projectile/Identity/ProjectileIdentityComponent.cs` | validated owner/slot/identity/UUID value seam | shallow | high | projectile entity |
| `ProjectileDefinitionReferenceComponent` | Component | `.../Projectile/Definition/ProjectileDefinitionReferenceComponent.cs` | catalog revision lookup | shallow | high | immutable per entity |
| `ProjectileBehaviorStateComponent` | Component | `.../Projectile/Behavior/ProjectileBehaviorStateComponent.cs` | behavior system reads/writes raw AI state | medium | high | projectile entity |
| `ProjectileLifetimeStateComponent` | Component | `.../Projectile/Lifecycle/ProjectileLifetimeStateComponent.cs` | lifecycle command/terminal commit | medium | high | projectile entity |
| `ProjectileTrajectoryStateComponent` | Component | `.../Projectile/Movement/ProjectileTrajectoryStateComponent.cs` | movement system and spatial query | medium | high | projectile entity |
| `ProjectileDamagePayloadComponent` | Component | `.../Projectile/Combat/ProjectileDamagePayloadComponent.cs` | damage calculation to hit commit port | deep | high | projectile entity |
| `ProjectilePenetrationStateComponent` | Component | `.../Projectile/Combat/ProjectilePenetrationStateComponent.cs` | accepted-hit command | medium | high | projectile entity |
| `ProjectileHitImmunityStateComponent` | Component | `.../Projectile/Combat/ProjectileHitImmunityStateComponent.cs` | target eligibility query and tick decrement | deep | high | projectile plus target slots |
| `ProjectileCollisionPolicyComponent` | Component | `.../Projectile/Collision/ProjectileCollisionPolicyComponent.cs` | collision query input, no world grid ownership | medium | high | projectile entity |
| capability markers | Components | `.../Projectile/Capability/*.cs` only if scale warrants subdirectory | capability-specific systems/ports | shallow | medium | optional |
| `ProjectileSourceMetadataComponent` | Component | `.../Projectile/Spawn/ProjectileSourceMetadataComponent.cs` | source adapter/drop/child-spawn command | medium | high | projectile entity |
| geometry/target/tool queries | Query | `.../Projectile/Queries/*.cs` | pure inputs to collision/target/fishing systems | deep | high | per call |
| `ProjectileDerivedPropertiesQuery` | Query | `.../Projectile/Queries/ProjectileDerivedPropertiesQuery.cs` | read-only localization, cleanup, cadence, target, ownership and network-section views; legacy setters become explicit commands | medium | high | per call/projection |
| `ProjectileNetworkReplicationProjection` | Projection/Adapter | `.../Projectile/Network/ProjectileNetworkReplicationProjection.cs` | committed snapshot to transport | deep | high | section/session |
| `ProjectilePresentationProjection` | Projection | `.../Projectile/Presentation/ProjectilePresentationProjection.cs` | committed state to client/rendering | shallow | medium | client-facing |
| `ProjectileStormDefinition` | Definition/value | `.../Projectile/Definition/ProjectileStormDefinition.cs` | catalog/behavior query | shallow | medium | immutable catalog |

Each proposed public type gets one same-named PascalCase file. Directory names express Projectile
capabilities; runtime ordering is declared by a schedule, never by path or file enumeration.

## 14. Eleventh Checkpoint: Combat Scaling State

`ProjectileCombatScalingState` is complete for its two report members. The two fields have
different ownership and lifecycle even though both affect combat calculations: `bonusCritChance`
is an instance modifier propagated from a source, while `hostileDamageScaling` is a selected,
immutable difficulty curve used when hostile damage is applied to players. They must not be merged
with Player's live crit stats or with the mutable damage result.

| Member | C# type | Proposed classification | Proposed owner/seam | Lifecycle / access pattern | Evidence status |
|---|---|---|---|---|---|
| `bonusCritChance` | `int` | authoritative per-projectile combat modifier snapshot | `ProjectileDamagePayloadComponent.BonusCritChance`; `ProjectileSpawnSystem` and source adapter initialize/propagate; critical-hit query reads | reset by `SetDefaults`; item and parent source values are added once during spawn; read during accepted PvE hit calculation; not observed in the Version4 network writer | confirmed |
| `hostileDamageScaling` | `GameDifficultyData.LinearCurve` | immutable hostile-player damage policy selected by definition | proposed `ProjectileHostileDamageScalingDefinition` or equivalent immutable field in the definition catalog; `ProjectileHostileDamageScalingSystem` samples it from an explicit difficulty snapshot | reset to `HostileProjectileDamageMultiplier`; type 1091 selects `LightningPlayerDamageScaling`; no in-tick writer or Version4 `Projectile.cs` sampler was observed; persistence/network ownership is unresolved | partial |

The source facts are narrow and should remain explicit. `SetDefaults` resets both members to zero and
`GameDifficultyData.HostileProjectileDamageMultiplier` (`Projectile.cs:548-554`). `ApplyStatsFromSource`
adds the spawning Player's revolver bonus for item type 2269 and copies the parent's bonus when a
child projectile is spawned (`Projectile.cs:10536-10560`). `Damage_PVE_Inner` adds the positive
bonus to the owner's melee, ranged, and magic crit chance before the random roll
(`Projectile.cs:11840-11850`); this is a combat input, not a replacement for Player crit state.
Type 1091 is hostile, trap, and AI style 203 and selects `LightningPlayerDamageScaling`
(`Projectile.cs:9906-9912`). The authoritative Version4 search found no `Sample` call or direct
read of `hostileDamageScaling` in `Projectile.cs`; therefore the field's declaration and selection
are confirmed, but its final application stage remains partial evidence.

`GameDifficultyData.LinearCurve` is a value wrapper over a key array and samples by piecewise linear
interpolation (`D:\\TRbackup\\Version4\\Terraria.DataStructures\\GameDifficultyData.cs:3-49`).
The default curve has Journey `0.5` and Master `3.0` endpoints; the lightning curve has Journey
`0.04`, Classic `0.08`, Master `0.24`, and Legendary `0.4` keys
(`GameDifficultyData.cs:67-77`). A proposed catalog adapter must copy these keys into an immutable
domain value or otherwise prevent mutation through the wrapped array. The current NLTX
`ProjectileHostileDamageScalingSystem` and `ProjectileHostileDamageScaling` enum are useful partial
coverage (`dome/src/Terraria.Dome.Simulation/Projectile/Systems/ProjectileHostileDamageScalingSystem.cs`,
`dome/src/Terraria.Dome.Simulation/Projectile/Definitions/ProjectileHostileDamageScaling.cs`), but
their difficulty mapping, floor-to-one rule, and type-1091 registry remain implementation evidence,
not proof that the missing Version4 consumer has been closed.

### Proposed contract

- `ProjectileDamagePayloadComponent` owns the copied `BonusCritChance` value. A spawn/source command
  may add an item or parent contribution exactly once; hit resolution only reads it.
- `ProjectileHostileDamageScalingDefinition` owns an immutable curve identity and key snapshot. A
  pure `Sample(WorldDifficultySnapshot)` query returns a multiplier; it does not read global
  difficulty state, mutate the projectile, or publish damage.
- `ProjectileCombatSystem` combines the payload, disposition, target kind, owner critical-stat
  snapshot, curve sample, and an explicit random-roll result into a typed damage outcome. The
  Player/NPC health mutation and any terminal transition stay in the combat commit port.
- A duplicate spawn command must not apply source propagation twice. A rejected or immune hit must
  not consume the critical-roll input, change `bonusCritChance`, or apply hostile scaling as a side
  effect. Friendly/NPC-target paths must not accidentally use the hostile-player curve.
- The curve is a definition revision, not per-tick mutable state. If a future compatibility packet
  or save format carries it, the revision/key encoding must be versioned explicitly; no such writer
  was observed in the current evidence.

The current NLTX broad `ProjectileDefinitionComponent` contains both `BonusCritChance` and
`HostileDamageScaling`, while `ProjectileDamagePayloadComponent` also carries both values. This is
partial existing coverage and a duplicate-ownership risk. The migration must choose one authoritative
source for each value before adding consumers: payload for the per-instance bonus and definition
catalog for the curve policy. The legacy adapter may expose compatibility views, but it must not
introduce a second writer.

## 15. Twelfth Checkpoint: Collision Geometry Cache

`ProjectileCollisionGeometryCache` is complete for its eight report members. The declarations are
mostly scratch state for specialized queries, not durable projectile authority. The proposed design
keeps world conditions and rectangular hitbox temporaries outside entity storage, treats the whip
point list as an explicitly versioned option rather than an automatic component, and passes lightning
geometry through a scoped collision context instead of a process-wide mutable field.

| Member | C# type | Proposed classification | Proposed owner/seam | Lifecycle / access pattern | Evidence status |
|---|---|---|---|---|---|
| `_cachedConditions_solid` | `Terraria.WorldBuilding.Conditions.IsSolid` | shared query helper with no projectile state | `ProjectileEnvironmentConditionQuery` backed by a world/tile adapter; caller-owned or stateless adapter instance | read by the type-710 damage adjustment query; no observed mutation or serialization; replace static instance with reentrant query dependency | partial |
| `_cachedConditions_notNull` | `Terraria.WorldBuilding.Conditions.NotNull` | shared query helper with no projectile state | same environment condition seam; query caller owns execution lifetime | read with `_cachedConditions_solid` in the downward world search; no observed mutation or persistence | partial |
| `_javelinsMax6` | `Point[]` | fixed-capacity selection workspace | `JavelinSelectionWorkspace` created by the javelin selection system; capacity `6` is a definition constant | filled with `(projectileIndex, timeLeft)` candidates and consumed in one `KillOldestJavelin` call; never serialized or retained between calls | confirmed |
| `_javelinsMax8` | `Point[]` | fixed-capacity selection workspace | same javelin query with capacity `8` for type 636 | filled and scanned only for one accepted hit; the selected projectile is destroyed through a command, not by the query | confirmed |
| `_javelinsMax10` | `Point[]` | fixed-capacity selection workspace | same javelin query with capacity `10` for type 614 | same call-local lifecycle; no entity or world ownership | confirmed |
| `WhipPointsForCollision` | `List<Vector2>` | mutable specialized geometry workspace / compatibility view | proposed `ProjectileWhipGeometryQuery` returning an immutable snapshot; optional `ProjectileWhipGeometryCacheComponent` only if same-tick reuse is proven | cleared and refilled before status, tile cutting, target collision, and enchantment presentation paths (`Projectile.cs:10630-10632,13533-13538,13887-13894,15348-15351`); invalidated by every input that `FillWhipControlPoints` reads, including AI, owner animation, position, scale, and update tick | confirmed |
| `_lanceHitboxBounds` | `Rectangle` | per-call broad-phase scratch rectangle | local variable inside `ProjectileLanceCollisionQuery`; no component | X/Y are rewritten for each historical/current lance segment and immediately tested (`Projectile.cs:13746-13757,13784-13786,13813-13815`); no serialization or persistence | confirmed |
| `_lightningCollisionBounds` | `Terraria.DataStructures.MultiPointHitbox` | scoped lightning collision context | `ProjectileLightningCollisionContext` passed into the collision/damage query; immutable copied points and bounding rectangle | assigned immediately before `Damage()` and cleared immediately after (`Projectile.cs:46545-46547`); the observed source has no direct consumer, so nested/reentrant behavior and final consumer remain unresolved | partial |

The source evidence shows why a single `ProjectileGeometryComponent` would be wrong. The three
javelin arrays are selected by projectile type and used only as bounded candidate buffers in
`KillOldestJavelin` (`Projectile.cs:12708-12719,13474-13493`); their contents are not projectile
state. `_lanceHitboxBounds` is overwritten before each line-AABB test in `Colliding`
(`Projectile.cs:13654-13815`). `_lightningCollisionBounds` is a temporary object around a damage
call, and `MultiPointHitbox` itself stores a point array plus a derived bounding rectangle
(`D:\\TRbackup\\Version4\\Terraria.DataStructures\\MultiPointHitbox.cs:5-24`).

`WhipPointsForCollision` is different: it is an instance list and four observed paths clear and
populate it, then consume its points. That is a compatibility-shaped workspace, not an authority
for position or damage. The replacement should prefer a pure `FillWhipControlPoints` calculation
over explicit trajectory/owner inputs and return a read-only snapshot. If profiling proves that the
same points must be shared across collision, tile cutting, status, and presentation in one tick,
the cache must carry an input version and owner, be invalidated on every geometry input change, and
be cleared on despawn/slot reuse. It must never be a static list.

The two `Conditions` objects are likewise adapter details. `WorldUtils.Find` is an external world
read and must remain behind a tile/world query port; a reusable condition object must be stateless or
created per invocation. Since the mirror's `Conditions` methods are placeholders, the exact solid
and non-null semantics remain partial evidence and must be verified against the authoritative runtime
before implementation.

### Proposed contract

- `ProjectileJavelinSelectionQuery` accepts a read-only projectile snapshot and a caller-owned
  capacity-specific workspace, returns the oldest eligible candidate, and performs no destruction.
  `ProjectileDestroyCommand` is emitted only after the query result is committed.
- `ProjectileLanceCollisionQuery` uses local `Rectangle` values and explicit historical trajectory
  samples. Reusing a rectangle across calls is an optimization detail, never shared authority.
- `ProjectileLightningCollisionContext` is scoped to one damage evaluation, has immutable point
  data, and supports nesting by value/stack rather than a static singleton. If the final consumer is
  outside `Projectile.cs`, that consumer must receive the context explicitly.
- `ProjectileWhipGeometryQuery` is deterministic for the same committed projectile inputs. A cache
  is permitted only with a documented key `(entity, tick, geometry-input-version, collision-mode)`;
  callers receive a read-only snapshot and cannot mutate internal lists.
- All world reads record the spatial adapter as the effect owner. Geometry queries may calculate
  hitboxes, but they cannot cut tiles, damage targets, or clear a projectile.

The current NLTX has `ProjectileTrailCacheComponent` and a pure-ish collision system, but no
equivalent ownership for these eight legacy members. This is partial coverage. The migration must
not put all arrays, hitboxes, whip points, and world conditions into a broad component merely because
the legacy class stores them together.

## 16. Thirteenth Checkpoint: Target Selection Cache

`ProjectileTargetSelectionCache` is complete for its seven report members as a design checkpoint,
with intentionally partial semantic evidence. These members are selection workspaces, not durable
target authority. A projectile may retain an explicit target lock only through a separate, validated
relation component; a temporary candidate list must not become that lock by accident.

| Member | C# type | Proposed classification | Proposed owner/seam | Lifecycle / access pattern | Evidence status |
|---|---|---|---|---|---|
| `_rainbowBoulderTargetsAny` | `List<NPC>` | temporary candidate workspace | `ProjectileRainbowBoulderTargetQuery` with caller-owned `NpcTargetSnapshot` storage | legacy declaration has no observed reader/writer in the P15 mirror; proposed lifetime is one smart-bounce evaluation, then discard; use stable handles/snapshots, never raw NPC references | partial |
| `_rainbowBoulderTargetsFar` | `List<NPC>` | temporary filtered candidate workspace | same rainbow-boulder query, with an explicit far-range predicate | same missing call-site evidence; classification between `Any` and `Far`, range, line-of-sight, and fallback order require authoritative AI evidence | partial |
| `_medusaHeadTargetList` | `List<Tuple<int, float>>` | scored target candidate workspace | `ProjectileMedusaTargetQuery` returning `NpcTargetScore` values `(NpcHandle, score)` | legacy declaration has no observed reader/writer; proposed score is computed from an explicit target snapshot and query inputs, materialized for one selection pass, never persisted/networked | partial |
| `_medusaTargetComparer` | `NPCDistanceByIndexComparator` | deterministic ordering policy, not state | `ProjectileMedusaTargetOrderingPolicy` with an explicit stable tie-break | comparator implementation and call site are absent/placeholder in the source mirror; do not assume NPC index is the primary semantic order until recovered; return a total order for replay | partial |
| `_ai164_blacklistedTargets` | `List<int>` | behavior-local exclusion workspace | `ProjectileTargetBlacklistWorkspace` keyed by projectile entity, behavior `164`, and decision tick | no observed call site; clear at the end of the behavior decision or when target identity/behavior phase changes; never persist or replicate the list | partial |
| `_ai158_blacklistedTargets` | `List<int>` | behavior-local exclusion workspace | same blacklist seam, behavior `158` | same lifetime and no-global-state rule; entries must be stable NPC handles/identity versions rather than reusable raw slots | partial |
| `_ai156_blacklistedTargets` | `List<int>` | behavior-local exclusion workspace | same blacklist seam, behavior `156` | same lifetime and no-global-state rule; exact reason for exclusion and reset point require missing AI evidence | partial |

The only concrete source evidence for this group is the static declaration block
(`Projectile.cs:288-310`) and the presence of behavior dispatch for AI styles 100, 156, 158, and
164 (`Projectile.cs:29143-29150,32763-32798`). `AI_100_Medusa`, `AI_156_BatOfLight`,
`AI_158_BabyBird`, and `AI_164_StormTigerGem` are empty in the observed mirror, and a repository-wide
search found no reads or writes of these seven names. The design therefore does not invent the
missing target semantics. It records the lists as compatibility evidence and defers exact range,
line-of-sight, faction, immunity, blacklist reason, and tie-break rules to the authoritative AI
implementation or a focused replay fixture.

The proposed target query has a strict direction:

```text
projectile committed state + behavior-specific query inputs
  -> NPC candidate snapshot adapter
  -> pure eligibility/filter/score/order calculation
  -> selected NpcHandle or no-target result
  -> behavior command or explicit target-lock commit
```

The candidate adapter may read active NPC state, hitbox/position, chaseability, health, line of
sight, immunity and stable identity, but it owns those reads and returns a snapshot. Query code must
not write NPC state, projectile AI, blacklists, or target locks. If a behavior needs to remember a
blacklist across several substeps, that is authoritative behavior state and belongs in an explicit
`ProjectileTargetExclusionStateComponent` with a versioned reset/expiry contract, not in a static
query cache.

Current NLTX has `NpcTargetSelectionSystem`, `ProjectileTargetEligibilitySystem`, and deterministic
candidate ordering in the NPC domain. Those are useful organization evidence, but they do not prove
the missing Terraria-specific rainbow-boulder, Medusa, or AI blacklist semantics. The existing NPC
selector's stable-ID tie break is a suitable pattern for replayability; it must be replaced or
extended only after the Version4 comparer behavior is recovered.

## 17. Fourteenth Checkpoint: Fishing and Mining Query State

`ProjectileFishingAndMiningQueryState` is complete for its three report members as a boundary
decision, with partial runtime evidence. A bobber is a projectile capability and may own a phase,
owner handle, and pending-result reference, but it must not own the fishing rules engine, a global
random source, an inventory mutation, or a mined tile set.

| Member | C# type | Proposed classification | Proposed owner/seam | Lifecycle / access pattern | Evidence status |
|---|---|---|---|---|---|
| `_availableFishTypesToShow` | `List<FishPossibilityEntry>` | temporary fish candidate/display workspace | `FishingCandidateQuery` or fishing UI projection; caller-owned `IReadOnlyList<FishCandidate>` snapshot | static declaration only in P15; proposed populated for one sonar/preview or fishing-rule evaluation and discarded after projection; item IDs/frequencies are not projectile authority | partial |
| `_context` | `FishingContext` | external-rule evaluation context with random and Player/environment inputs | `FishingResolutionContext` created by Fishing system; random source, `FishingAttempt` snapshot, Player snapshot, and biome roll state are explicit dependencies | static declaration only; source type contains `Random`, mutable `Fisher`, Player reference, and rolled biome flags (`FishingContext.cs:6-22`); scope one fishing attempt, never shared across projectiles or ticks | partial |
| `_miningHelperPointsToSkip` | `List<Point>` | temporary world/tile query exclusion workspace | `ProjectileMiningTileQuery` or `TileInteractionQuery`, with caller-owned point set keyed to one mining operation | static declaration only in P15 and no observed caller; clear at operation end; points describe world coordinates/visited tiles, not projectile identity or persistent world state | partial |

The authoritative declarations are at `Projectile.cs:292-308`. The related `AI_061_FishingBobber`
and `AI_061_FishingBobber_GiveItemToPlayer` methods are placeholders in the observed mirror, while
`Update` shows the important side-effect boundary: on the owning client, a bobber may clear sonar
text, read `ai[1]`, give an item to the Player, and then reset `ai[1]`
(`Projectile.cs:34073-34074,47779-47787`). This establishes that fish outcome and inventory mutation
must be an explicit committed result, not a mutation hidden in a query cache. `FishingContext` and
`FishingAttempt` also demonstrate that a fishing attempt includes player tool/bait power, location,
water/liquid classification, biome state, and rolled item/enemy outcomes
(`Terraria.GameContent.FishDropRules/FishingContext.cs:6-22`,
`Terraria.DataStructures/FishingAttempt.cs:3-31`).

Current NLTX has a useful split: `FishingBobberStateComponent` stores owner, projectile type,
phase, pending item type, water counts, fishing level, liquid flags, and swing count, while
`PlayerFishingUseSystem` owns active-bobber checks and bait inventory effects
(`dome/src/Terraria.Dome.Simulation/Fishing/FishingBobberStateComponent.cs`,
`dome/src/Terraria.Dome.Simulation/Player/Systems/PlayerFishingUseSystem.cs`). It does not yet
prove parity with Version4's fish-rule context or mining paths. Keep that component as partial
organization evidence; do not add the three legacy static members to it.

### Proposed contract

- `FishingCandidateQuery` accepts a copied player/environment/attempt snapshot and returns an
  immutable fish candidate list for presentation or resolution. It must not consume random input or
  modify a bobber, Player, inventory, or world tile.
- `FishingResolutionSystem` owns the attempt-scoped random source and consumes a candidate/result
  command exactly once. The commit port owns item grant, quest progress, enemy spawn, sonar text, and
  bobber phase transition; duplicate commands require an attempt identity for de-duplication.
- `FishingResolutionContext` is an attempt-local value/reference with explicit lifetime. It must not
  hold a static Player or random instance, and it must not outlive the attempt or be reused by another
  projectile.
- `ProjectileMiningTileQuery` returns read-only tile observations and a visited/skip snapshot. A
  mining command may later mutate Tile through the WorldInteraction adapter; the query cannot kill
  tiles, send network messages, or update achievements.
- `_availableFishTypesToShow`, `_context`, and `_miningHelperPointsToSkip` are excluded from
  projectile network and persistence snapshots unless a future compatibility protocol explicitly
  requires their serialized result. Serialize committed bobber state or fish outcome, not query
  workspaces.

## 18. Fifteenth Checkpoint: Kite and Lightning Rules

`ProjectileKiteAndLightningRules` is complete for its two report members as an immutable
definition/query boundary. Neither value belongs on a mutable projectile instance. The current
source mirror confirms the declarations and the behavior dispatch/collision seams, but it does not
provide enough implementation body to claim a closed wind or liquid-damage algorithm.

| Member | C# type | Proposed classification | Proposed owner/seam | Lifecycle / access pattern | Evidence status |
|---|---|---|---|---|---|
| `StormLightningLiquidDamageRadius` | `int` constant | immutable specialized storm/lightning rule | `ProjectileStormLightningDefinition` or catalog policy; consumed by a pure liquid-area query and a separate combat/world commit port | fixed definition value `500`; read when resolving a lightning event; never copied into per-tick projectile state, network snapshots, or save data | confirmed declaration; partial consumer |
| `MinimumWindStrengthToFlyKite` | `float` constant | immutable kite movement threshold | `ProjectileKiteWindPolicy` / `ProjectileKiteWindQuery`; explicit wind snapshot is supplied by the environment adapter | fixed definition value `0.2f`; read during kite behavior evaluation; no global wind read or mutation inside the query; no network/persistence field | confirmed declaration; partial consumer |

The declaration evidence is `Projectile.cs:296` and `Projectile.cs:304`. Kite definitions call
`DefaultToKite`, which sets `aiStyle = 160` and `extraUpdates = 60` (`Projectile.cs:10151-10161`);
the AI dispatcher invokes `AI_160_Kites` for that style (`Projectile.cs:32779-32782`), but the
observed method body is a placeholder (`Projectile.cs:33983`). This proves a behavior seam, not the
wind vector, wind direction, lift/drag, owner authority, or kill/disable condition. The current
NLTX `LegacyProjectileWindPolicy` preserves the `0.2f` value and rejects non-finite input through
`CanFlyKite(float)`, which is useful organization evidence only; it is not Version4 equivalence
evidence.

For lightning, the Version4 type/AI mapping selects `aiStyle = 203` for type 1091
(`Projectile.cs:9906-9912`). Specialized collision calls `StormLightningCollisionCheck` at
`Projectile.cs:13996-13998`, while that method and `AI_203_StormLightning` are placeholders in the
observed mirror (`Projectile.cs:14180-14182,32969`). The terminal lightning path does construct
temporary collision bounds, call `Damage()`, and clear the bounds
(`Projectile.cs:46521-46547`). That path establishes a collision/combat commit seam but does not
prove that the 500-unit constant is itself a damage radius, which liquid cells are selected, or
whether liquid effects are authoritative, client-side, or projected.

### Proposed contracts

- `ProjectileKiteWindQuery` accepts a finite wind-speed/direction snapshot, kite definition policy,
  current movement state, and an explicit authority context. It returns a deterministic movement
  decision or lift/drag intent. It does not read `Main`/global wind, write velocity, publish
  particles/audio, or alter projectile lifetime. A movement system applies the returned intent.
- `ProjectileStormLightningLiquidQuery` accepts lightning origin/trajectory state, a finite radius
  policy, and a read-only liquid/world snapshot. It returns candidate liquid regions or a typed
  `LightningLiquidDamageIntent`; it does not mutate tiles/liquid, NPCs, projectile state, or the
  network. A world interaction and combat commit port applies accepted effects exactly once.
- The query inputs must include the projectile identity/attempt or event ID needed to de-duplicate a
  repeated lightning resolution. A static bounds workspace is not an authority and may only be
  caller-owned, tick-scoped storage with explicit invalidation.

### Invariants and integration gates

- The constants are revisioned catalog data. A catalog revision is captured at spawn or event
  creation; changing definitions during a live instance is an integration error.
- A wind threshold decision is pure and deterministic for the same wind snapshot. Non-finite input
  is rejected or produces `NoMovementIntent`; it must not silently become a valid wind value.
- A radius is a query bound, not permission to damage every entity in a circle. Liquid selection,
  line-of-effect, faction, immunity, friendly/hostile disposition, and damage commit remain explicit
  world/combat policies.
- Any client-facing lightning geometry or liquid effect is a projection of a committed event. It
  cannot write authoritative damage or liquid state from presentation code.
- Before implementation, integration review must recover the specialized AI bodies or a replay
  fixture covering kite wind direction/threshold transitions and lightning collision/liquid damage
  ordering. Until then, the proposed types remain `status: proposed` and this component stays
  `evidenceStatus: partial`.

## 19. Sixteenth Checkpoint: Derived Properties

`ProjectileDerivedProperties` is complete for its eight report properties. The group is a set of
derived views over authoritative inputs and external projections, not an eighth broad projectile
component. Two properties expose legacy setters; their compatibility surface must be retained by
explicit commands with one writer rather than by making the derived view independently mutable.

| Member | C# type | Proposed classification | Proposed owner/seam | Lifecycle / access pattern | Evidence status |
|---|---|---|---|---|---|
| `Name` | `string` | localized derived/presentation view | `ProjectileNameQuery` plus localization/catalog adapter, keyed by definition `type` and catalog/language revision | resolve on read or client projection; do not persist a localized string on the projectile entity; unknown/missing entries preserve `Lang.GetProjectileName` empty-text behavior | confirmed formula; catalog revision partial |
| `WipableTurret` | `bool` | sentry cleanup eligibility query | `SentryCleanupEligibilityQuery` consuming owner authority, `sentry`, `type`, and DD2 persistence snapshot; lifecycle system commits removal | evaluate only for the local owner and sentry path, then issue a destroy/cleanup command; query does not call `Kill` or mutate Player/DD2 state | confirmed formula; caller/authority partial |
| `Opacity` | `float` | presentation view over alpha with compatibility command | `ProjectileOpacityView` reads authoritative presentation `alpha`; `SetProjectileOpacityCommand` is the only replacement writer | read as `1f - alpha / 255f`; legacy setter clamps transformed alpha to `[0,255]` and casts to `int`; effects/rendering consume the view, not a second opacity field | confirmed formula and setter; writer/network boundary partial |
| `MaxUpdates` | `int` | update-cadence derived view with compatibility command | `ProjectileUpdateCadenceState` owns `extraUpdates`; `SetProjectileMaxUpdatesCommand` preserves the legacy `value - 1` write | read as `extraUpdates + 1`; initial definition setup and behavior may write cadence through the explicit owner before lifetime/substep scheduling; no duplicate derived state | confirmed formula and consumers; writer ordering partial |
| `OwnerMinionAttackTargetNPC` | `Terraria.NPC` | cross-domain target lookup view | `OwnerMinionTargetQuery` returns `NpcHandle?` or copied `NpcTargetSnapshot` from a Player snapshot and NPC adapter | negative Player target index yields no target; later behavior filters `CanBeChasedBy`/line of sight; never expose a raw global `NPC` reference or let the query write target state | confirmed formula; invalid-slot and snapshot contract partial |
| `OwnedBySomeone` | `bool` | classification-derived eligibility view | `ProjectileOwnershipModeQuery` over `npcProj` and `trap` classification flags; owner identity remains a separate query | preserve the exact legacy predicate `!npcProj && !trap`; do not add owner-validity semantics to this property or merge it with `ProjectileIdentityComponent` | confirmed formula; current NLTX adds stricter owner validity and is not equivalence evidence |
| `CareForAttackCD` | `bool` | combat cooldown applicability query | `ProjectileOwnerMeleeCooldownQuery` over `usesOwnerMeleeHitCD`, `OwnedBySomeone`, and legacy owner value; accepted-hit commit port updates Player cooldown | preserve `usesOwnerMeleeHitCD && OwnedBySomeone && owner < 255`; the view only grants eligibility and never decrements or writes Player cooldown | confirmed formula and hit consumer; cross-owner commit partial |
| `NetSectionCoordinates` | `Point` | network-section projection view | `ProjectileNetworkSectionQuery` over committed position and a `NetSectionMap` adapter; network scheduler consumes the result | preserve `(int)position / 16` tile conversion followed by `GetSectionX(tileX)=tileX/200` and `GetSectionY(tileY)=tileY/150`; use for skipped-section retry, not entity authority or persistence | confirmed formula and section consumer; coordinate edge cases partial |

The declaration block is `Projectile.cs:312-386`. `Name` delegates to
`Lang.GetProjectileName(type).Value`; the localization helper returns a cached entry only when the
type is in range and populated, otherwise `LocalizedText.Empty`
(`Lang.cs:245-254`). `WipableTurret` delegates to `TurretShouldPersist`; the observed implementation
uses the DD2 event state for types `663, 665, 667, 677-679, 688-693` and returns false for other
types (`Projectile.cs:420-448`). The property is a local-owner/sentry predicate, not a general
despawn command, and no reader call site was used to assume additional cleanup timing.

`Opacity` is read extensively by presentation/effect code and is also written during defaults and
AI transitions. `MaxUpdates` is used both during definition setup and live behavior, including
derived lifetime/cooldown expressions (`Projectile.cs:5438-5439,6053-6054,6612-6614,
7318-7334,8593-8597,10001-10006,25815-25819`). Therefore the replacement must preserve the
legacy alias to `alpha`/`extraUpdates` while making the single writer explicit; the current NLTX
definition's checked `MaxUpdates` view and update-budget validation are useful partial organization
evidence, not proof of setter-equivalent behavior.

`OwnerMinionAttackTargetNPC` reads the owner's `MinionAttackTargetNPC` index and returns the global
NPC slot without checking chaseability itself (`Projectile.cs:350-360`). Consumers then apply their
own target and line-of-sight rules, for example the minion behavior paths at
`Projectile.cs:24619-24631` and `32991-33002`. The adapter must preserve the no-target sentinel
while replacing a raw `NPC` reference with a stable handle/snapshot. `OwnedBySomeone` does not read
`owner` at all; `CareForAttackCD` is consumed after an accepted PVE hit before calling
`Player.SetMeleeHitCooldown` (`Projectile.cs:11598-11603,13010-13013`). This is a combat commit
boundary, not a Projectile-owned Player cooldown.

`NetSectionCoordinates` uses `position` converted from pixels to tile coordinates and the explicit
network section divisors in `Netplay.GetSectionX/Y` (`Netplay.cs:497-509`). The skipped-update retry
path reads this property before sending a section-resync packet
(`Projectile.cs:15280-15289`). Keep the arithmetic and integer-division behavior as a compatibility
fixture, including positions near section boundaries and negative coordinates; do not normalize or
clamp coordinates in the query without authoritative evidence.

### Derived-view contracts and boundaries

- `ProjectileDerivedPropertiesQuery` accepts a read-only projectile snapshot plus explicit catalog,
  localization, Player, NPC, DD2, and network-section snapshots. It returns values or stable handles
  and never reads mutable global singletons directly.
- `SetProjectileOpacityCommand` and `SetProjectileMaxUpdatesCommand` are compatibility seams, not
  new derived components. Their commits write `alpha` and `extraUpdates` through the existing
  presentation/cadence owners and preserve the legacy conversion formulas until focused fixtures
  approve a change.
- `OwnerMinionAttackTargetNPC` is an input to behavior-specific target queries. Target validity,
  chaseability, line of sight, immunity and target lock ownership remain in the NPC/targeting
  domains; the derived view cannot mutate any of them.
- `WipableTurret` produces a cleanup intent only for the authoritative local owner. The lifecycle
  commit decides whether to destroy the sentry and emits any network/persistence projection.
- `Name`, `Opacity`, and `NetSectionCoordinates` are projections/queries. Persist the definition
  type, raw alpha/cadence inputs, and committed transform as needed; do not persist their localized,
  floating-point, or network-map results as independent authorities.

### Integration gaps

The exact call sites for `WipableTurret` and the catalog/language revision lifecycle are not closed
by the observed P15 mirror. The raw property also assumes valid owner and target indexes; the
replacement adapter must define invalid-slot behavior without silently changing the legacy replay
contract. The current NLTX ownership and cadence helpers add validation beyond the Version4 property
formulas, so equivalence requires focused fixtures rather than structural similarity.

### Kite and lightning network and persistence boundary

`MinimumWindStrengthToFlyKite` and `StormLightningLiquidDamageRadius` are not serialized projectile
instance fields. Network input may carry a validated behavior state or committed lightning result;
the receiver resolves the definition by catalog revision and identity. Persistence stores committed
projectile/behavior state and any accepted effect event or replay token, not the query's temporary
wind/liquid candidates. The exact compatibility shape is still an integration-review item.

## 20. Seventeenth Checkpoint: Storm Definition

`ProjectileStormDefinition` is complete for its seven report fields. The legacy nested
`HallowBossPelletStormInfo` is a mutable public struct, but the proposed replacement is an immutable
value/definition view with pure geometry methods. The four methods below are behavior, not additional
P15 members, so the authoritative ledger remains 7 fields for this group and 126 members overall.

| Member | C# type | Proposed classification | Proposed owner/seam | Lifecycle / access pattern | Evidence status |
|---|---|---|---|---|---|
| `StartAngle` | `float` | immutable per-storm geometry definition | `ProjectileStormDefinition` in the projectile definition/value boundary | created by the storm query/factory for one storm index; read by pure position/hitbox queries; never tick-mutated | confirmed declaration and factory assignment |
| `AnglePerBullet` | `float` | immutable angular spacing definition | same value object | fixed for the returned storm definition; no system writes it during collision | confirmed declaration and factory assignment |
| `BulletsInStorm` | `int` | immutable enumeration cardinality | same value object; collision system owns loop | collision caller iterates `[0, BulletsInStorm)` after obtaining a value snapshot; no global list or projectile state | confirmed declaration and factory assignment |
| `BulletsProgressInStormStartNormalized` | `float` | immutable progress input | same value object, produced from behavior/localAI snapshot by factory | calculated once per storm query from the explicit `localAI[0]` snapshot; not a hidden mutable progress counter | confirmed declaration and factory assignment |
| `BulletsProgressInStormBonusByIndexNormalized` | `float` | immutable per-index progress slope | same value object | consumed by `GetBulletProgress`; source factory currently assigns `0f`; no per-bullet mutation | confirmed declaration and factory assignment |
| `StormTotalRange` | `float` | immutable radial geometry definition | same value object | scales the pure position calculation; not a liquid-damage radius or world query bound | confirmed declaration and factory assignment |
| `BulletSize` | `Vector2` | immutable hitbox geometry definition | same value object; collision query consumes | passed to centered hitbox calculation; not a shared mutable collider or target-owned rectangle | confirmed declaration and factory assignment |

### Pure storm behavior

The source methods are deterministic for a value snapshot and explicit inputs:

- `GetBulletProgress(index)` returns
  `BulletsProgressInStormStartNormalized + BulletsProgressInStormBonusByIndexNormalized * index`.
  It does not validate the index or mutate the value.
- `IsValid(index)` calls `GetBulletProgress` and returns true exactly when the result is between
  `0f` and `1f`, inclusive. It does not independently enforce `BulletsInStorm` bounds; preserving
  that distinction is a compatibility requirement until a focused fixture authorizes validation.
- `GetBulletPosition(index, center)` returns
  `center + UnitX.RotatedBy(StartAngle + AnglePerBullet * index) * StormTotalRange * progress`.
  Rotation, multiplication and translation are pure value operations.
- `GetBulletHitbox(index, center)` creates `Utils.CenteredRectangle(GetBulletPosition(...),
  BulletSize)`. It does not query or mutate world/NPC state.

The exact method bodies are `Projectile.cs:56-88`. The type-871 collision consumer obtains six
storm definitions, loops over each definition's `BulletsInStorm`, checks `IsValid`, then tests the
computed hitbox against the caller's target rectangle (`Projectile.cs:13869-13884`). This makes
the ownership direction explicit:

```text
projectile behavior snapshot (center + localAI[0] + catalog revision)
  -> ProjectileStormDefinitionFactory
  -> immutable storm value(s)
  -> pure progress/position/hitbox query
  -> collision system target-rectangle intersection
  -> combat query/commit boundary
```

`AI_172_GetPelletStormsCount` returns `6`, and the observed
`AI_172_GetPelletStormInfo` factory assigns the concrete formulas at `Projectile.cs:33700-33724`:
the start angle combines `stormIndex * PI/3`, `-PI/2`, and `stormIndex * PI/5`; angle spacing is
`2PI/3`; bullet count is `3`; start progress is `Utils.GetLerpValue(stormIndex * 10,
90 + stormIndex * 10, localAI[0])`; index bonus is `0`; total range is `500`; and bullet size is
`(16,16)`. The enclosing `AI_172_HallowBossRainbowPelletStorm` body is a placeholder in the
observed mirror, so storm spawn timing, localAI writer ownership, server/client authority, and
lifetime are still partial. The factory formula is evidence; it is not a claim that the missing AI
body is recovered.

### Ownership, lifecycle, and cross-domain boundary

- `ProjectileStormDefinition` is immutable catalog/value data. The source-only implementation is
  kept flat at `src/Projectile/ProjectileStormDefinition.cs` to match the current small Projectile
  project; it does not become a component containing mutable per-bullet state.
- `ProjectileStormDefinitionFactory` is a pure query over an explicit `localAI[0]` snapshot. The
  source-only implementation returns caller-owned values from
  `src/Projectile/ProjectileStormDefinitionFactory.cs`; it does not read a static world state,
  retain a static list, or claim a catalog/lifecycle owner that has not been proven.
- The collision system owns target rectangle intersection and emits a typed collision/hit intent.
  The storm value and its geometry methods do not damage NPCs, alter projectile penetration, mutate
  liquid, or publish network packets.
- Center position and `localAI[0]` are authoritative inputs owned by projectile behavior/trajectory
  state. `BulletsProgressInStormStartNormalized` is a derived value snapshot, not a second behavior
  state store.
- A cached storm array is allowed only if keyed by the same committed tick, behavior-state version,
  center/geometry version, and catalog revision; otherwise recompute it. Invalidation must be
  explicit and caller-visible.

### Network, persistence, and presentation boundary

The seven fields and the four pure methods are not independently serialized as projectile network
state. Network input or persistence restores the authoritative behavior state, transform, definition
reference, and catalog revision; the receiver recomputes storm geometry. A committed collision or
combat result may be projected for clients or replay logs, but presentation code cannot use a storm
hitbox to write damage. If compatibility requires serialized storm data, serialize a versioned
definition/result DTO at the protocol adapter, never the mutable query workspace or a raw geometry
cache.

### Final evidence gaps

The remaining integration review must recover the `AI_172_HallowBossRainbowPelletStorm` body or a
deterministic replay fixture for localAI progression, storm lifetime, collision ordering, authority,
and network/persistence behavior. It must also test index values outside the normal loop, progress
at exactly `0` and `1`, negative/greater-than-one progress, rotated positions, centered rectangle
rounding, and catalog revision changes. The source-only value/query unit is not behavior-equivalence
evidence for those unresolved boundaries, and this document's `evidenceStatus` remains `partial`.

## 21. Source-Only Implementation Checkpoint: ProjectileStormDefinition

This checkpoint implements only the evidence-closed pure geometry boundary. It does not reconstruct
`AI_172_HallowBossRainbowPelletStorm`, connect type-871 collision, or write projectile, NPC, combat,
liquid, network, presentation, or persistence state.

### Actual files

- `src/Projectile/ProjectileStormDefinition.cs`
- `src/Projectile/ProjectileStormDefinitionFactory.cs`
- `src/Projectile/Terraria.Projectile.csproj`

### Core behavior

- `ProjectileStormDefinition` is an immutable `readonly record struct` containing the seven
  authoritative report fields.
- `GetBulletProgress` preserves the exact linear formula and does not validate the index.
- `IsValid` accepts progress in the inclusive `[0, 1]` interval and does not add a bullet-count
  range check.
- `GetBulletPosition` reproduces the `Vector2.UnitX` angle rotation and radial scaling using only
  explicit values.
- `GetBulletHitbox` reproduces Version4 centered rectangle truncation and returns the existing
  immutable `EntityEcs.Queries.EntityHitbox` value.
- `ProjectileStormDefinitionFactory.Create` preserves the confirmed per-index angle and progress
  formulas. `CreateAll` returns six caller-owned definitions and retains no mutable cache.

### Dependency impact and exclusions

The Projectile project now references the existing `Share/Entity` project only for the immutable
`EntityHitbox` value. No Player, NPC, world, liquid, transport, persistence, localization, static
mutable state, registration key, or legacy writer was added. The placeholder storm AI and all other
16 P15 implementation groups remain pending. Catalog revision capture, index/lifecycle ownership,
and focused behavior-equivalence fixtures remain evidence gaps.

### Verification checkpoint

The affected Projectile project was built serially through the repository wrapper:

```powershell
& .\Build\Tools\Invoke-SerialDotnet.ps1 `
  -DotnetArguments @(
    'build',
    '.\src\Projectile\Terraria.Projectile.csproj',
    '--no-restore',
    '-m:1',
    '-nr:false',
    '-p:UseSharedCompilation=false',
    '-p:MSBuildNodeReuse=false',
    '-p:BuildInParallel=false'
  )
```

Result: exit code `0`; `Terraria.Relationships`, `Terraria.EntityEcs`, and
`Terraria.Projectile` built without warnings or errors. The verified artifact is
`Build/bin/Terraria.Projectile/Debug/net10.0/Terraria.Projectile.dll`.

A focused reflection verifier then loaded `Terraria.Relationships.dll`, `Terraria.EntityEcs.dll`,
and `Terraria.Projectile.dll` from that output directory and passed with exit code `0`. It covered
six-storm enumeration, per-index angle and progress formulas, progress `0` and `1`, negative-index
compatibility, inclusive validity, centered hitbox `(92,192,16,16)`, rotated position math, and
deterministic repeated queries. This is pure-value evidence only; it does not verify storm AI,
collision integration, lifecycle, authority, network, persistence, or full behavior equivalence.

### DamageClass compatibility-view checkpoint

`src/Projectile/ProjectileDamagePayloadComponent.cs` keeps `DamageClass` as the single writable
damage-classification authority and exposes the read-only compatibility views `IsMelee`,
`IsRanged`, and `IsMagic`. No second set of writable `melee`, `ranged`, or `magic` flags was
introduced. The existing duplicate `Reflected` and `Ai`/`LocalAi` fields remain pending a caller
and writer audit; `ManualDirectionChange` remains because the existing
`ProjectileCombatVerification` consumes it.

This checkpoint changed no System, Query, Command, Adapter, Projection, test, verifier, or
project-file code. The serial Projectile build exited `0` with `0` warnings and `0` errors, and
produced `Build/bin/Terraria.Projectile/Debug/net10.0/Terraria.Projectile.dll`. The existing
`ProjectileCombatVerification` run exited `0` and printed:

```text
PASS: projectile disposition, penetration, immunity, trail, reflection, collision policy, network, and animation state boundaries
```

A read-only reflection smoke check also exited `0`: `Generic` reported all three views as
`false`; `Melee` reported only `IsMelee=true`; `Ranged` reported only `IsRanged=true`; and
`Magic` reported only `IsMagic=true`.

The implementation status remains `partial`. Runtime classification writers, the remaining
implementation groups, and cross-domain behavior are still unimplemented.

## 22. Derived Properties Compatibility-View Checkpoint

This checkpoint adds two read-only projections that are independently determined by already
existing component authorities:

- `src/Projectile/ProjectilePresentationStateComponent.cs`: `Opacity` is calculated from the
  stored `Alpha` value with the legacy formula `1.0f - Alpha / 255.0f`. It does not store a second
  opacity value and does not perform rendering, lighting, or client projection.
- `src/Projectile/ProjectileUpdateCadenceComponent.cs`: `MaxUpdates` is calculated from the stored
  `ExtraUpdates` value with the legacy formula `ExtraUpdates + 1`. It does not add a setter, validate
  negative values, or schedule substeps, lifetime, or cooldown updates.

The affected files are limited to these two component definitions. No System, Query, Command,
Adapter, Projection, test, verifier, or project-file code was added. The full
`ProjectileDerivedProperties` group remains partial: setter ownership, runtime writer ordering,
network/persistence/client projection, and cross-domain consumers are not closed by these getters.

### Verification evidence

The affected Projectile project was built serially through the repository wrapper with
`.\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\Projectile\Terraria.Projectile.csproj --no-restore -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`; it exited `0` with `0` warnings and `0` errors. The verified artifact is
`Build/bin/Terraria.Projectile/Debug/net10.0/Terraria.Projectile.dll`.

The existing `ProjectileCombatVerification` was then built through the same serial wrapper and
exited `0` with `0` warnings and `0` errors. Its `--no-build --no-restore` run exited `0` and
printed `PASS: projectile disposition, penetration, immunity, trail, reflection, collision policy,
network, and animation state boundaries`.

A read-only PowerShell reflection smoke check loaded the verified Projectile artifact and passed
`Opacity(alpha=0) = 1`, `Opacity(alpha=255) = 0`, `Opacity(alpha=127) = 0.5019607543945312`,
`MaxUpdates(extra=0) = 1`, `MaxUpdates(extra=3) = 4`, and the preserved compatibility result
`MaxUpdates(extra=-1) = 0`. This is focused component evidence only; it does not verify setters,
runtime writer ordering, substep scheduling, network/persistence projection, rendering, or full
Version4 behavior equivalence. The P15 partition remains incomplete and the top-level statuses
remain `implementationStatus: partial` and `verificationStatus: partial`.

## 23. Component-Only Continuation Audit

The remaining P15 report groups were reconciled against the actual component files under
`src/Projectile`. The component-owned members are already represented by the existing narrow
boundaries: identity and owner values, disposition, definition reference, raw/local AI, lifetime
and cooldown state, damage payload/policy, penetration and immunity state, trajectory and trail
state, collision policy, network scheduling state, presentation state, capability markers, and
source metadata.

No additional component was added in this continuation because each remaining candidate crosses a
boundary that the design explicitly keeps outside component storage:

- `perIDStaticNPCImmunity` is a shared registry rather than per-projectile state.
- The owner-hit fields already have one storage/writer boundary in
  `ProjectileCollisionPolicyComponent`; adding a separate targeting component before caller
  migration would create duplicate authority.
- `hostileDamageScaling` is an immutable difficulty-curve definition/query, while
  `bonusCritChance` already belongs to `ProjectileDamagePayloadComponent`.
- Geometry, target-selection, fishing/mining, and derived-property gaps are caller-owned queries or
  projections; kite/lightning constants are definition/query values.

This audit changed no `src` file and introduced no System, Query, Command, Adapter, Projection,
test, verifier, or project-file code. Per the current user instruction, no build, test, verifier,
or reflection command was run during this continuation. The P15 implementation and verification
statuses remain partial; the next source change requires an explicit cross-boundary ownership
decision rather than another storage component.

## 24. Current Manual Session Checkpoint

`ProjectileDerivedProperties` is blocked for this component-only session. Its eight members are
derived views, presentation compatibility, network-section mapping, or cross-domain ownership;
none is an independently determinable ECS component state boundary. The existing component
authorities already cover the independently determinable P15 state, including owner-hit policy,
bonus critical chance, alpha, and update cadence.

The unresolved dependencies are `ProjectileNpcImmunityRegistryAdapter`,
`ProjectileTargetingPolicyComponent` ownership integration, localization/catalog resolution,
Player/NPC target snapshots, DD2 cleanup authority, alpha and `extraUpdates` compatibility writers,
network-section mapping, and geometry/fishing/mining/kite/lightning query owners. Implementing any
of these here would add a Query, Command, Adapter, Projection, cross-domain owner, or duplicate
component authority, which is outside the current task. No `src` file was changed, no test or
verification code was added, and the current session has no valid component implementation to
advance. `currentComponentStatus: blocked`; `verificationStatus: not-run` for this session.
