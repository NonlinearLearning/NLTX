# System Decomposition Report: authoritative P15

## Scope and status

| Item | Value |
| --- | --- |
| `designStatus` | `proposed` |
| `verificationStatus` | `not-run` |
| `sourceModified` | `false` |
| partition | `P15` |
| task set | `authoritative-system-decomposition` |
| task | `AUTH-SYS-P15` |
| claim mode | `version4-authoritative-partition-session-runner` |
| original `sessionId` | `4426a5ad1b824c2490a832c3bb6726b8` |
| formal parent | `ProjectileSimulation` |
| claimed leaves | `17` |
| claimed members | `126` (`118` fields + `8` properties) |
| input report | `D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P15-Projectile.md` |
| authorized output | `D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P15-projectile.md` |
| source project | `D:\TRbackup\Version4` |
| reference project | `C:\Users\shan\Downloads\ECS\space-station-14-master` |
| inspected target tree | `D:\TRbackup\NLTX\src\NSSLC` |

This is a read-only static boundary proposal for one claimed partition. Settling the runner
session means only that this report was produced for P15. It does not mean that a System was
migrated, that behavior is equivalent, that an API is compatible, or that validation passed.

## Evidence roles

| Evidence | Use and limit |
| --- | --- |
| P15 input ledger | Authoritative member inventory and partition boundary. It does not prove unique writers, lifecycle ownership, or complete call closure. |
| Version4 source | Primary semantic evidence: `Terraria/Projectile.cs`, `Main.cs`, `MessageBuffer.cs`, `NetMessage.cs`, `Player.cs`, `NPC.cs`, and collision call sites. Current line numbers below are source observations, not generated implementation contracts. |
| CPG Query API | Read-only queries against `D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite`; import `complete`, `ReadOnly=true`, manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`, project fingerprint `521CBD4C11E7EB731256C232350A54099F431D0E59EBDF3C38880281F169C11B`. Partial budgets and `CalleeEffectsNotExpanded` remain `unknown`/`partial`, never negative evidence. |
| NLTX tree | Existing candidate components, catalogs, query interfaces, storage indexes and static helpers. They are design material only; no runtime ownership transfer is claimed. |
| SS14 reference | Vocabulary and organization evidence only. `DamageableSystem` is an `EntitySystem` with injected `EntityQuery<DamageableComponent>` and command-like mutation methods (`Content.Shared/Damage/Systems/DamageableSystem.cs:16-26`, `DamageableSystem.API.cs:68-110`). It is not Version4 behavior evidence. |
| tModLoader API docs | Public API cross-check only: `class_projectile.html:159-173,256,282,442-452,566-567,638-644`. Public docs do not replace Version4 private source or prove NLTX compatibility. |

## CPG Query API supplement

The query run resolved `Terraria.Projectile` and selected call sites. Positive findings include
98 selected `active` uses, 9 `identity` uses, 576 `timeLeft` uses, 175 `netUpdate` uses, 3
`projUUID` uses, 206 `damage` uses, 723 `penetrate` uses, 51 `Damage` callers, 5
`Colliding` callers, 240 `Kill` callers, 2 `SetDefaults` callers, 17 vector and 227 scalar
`NewProjectile` callers, one `GetNextSlot` caller, and one `FindOldestProjectile` caller.
`ai` reached the configured 1000-item budget and is `partial`; `localNPCImmunity` access
direction was `Unknown`. Callable facts for core methods were `partial` with
`CalleeEffectsNotExpanded`. These results support existence and selected call relationships,
not a complete read/write, event, virtual-dispatch, persistence, or scheduler closure.

## Version4 source facts

### State and initialization

`Terraria.Projectile` is declared at `Projectile.cs:32`; the P15 inventory covers its fields at
`Projectile.cs:90-310` and properties at `:312-386`. Static immunity arrays are initialized at
`:388-404`. `SetDefaults` is at `:444-556` and is invoked from `Main.cs` and
`MessageBuffer.cs` during local and network creation. The source contains many empty or stub
bodies (about 113 one-line empty methods in the inspected file, including
`AI_149_GolfBall` at `Projectile.cs:17993`, `Shimmer` at `:18676`, and
`AI_203_StormLightning` at `:32969`); behavior depending on those bodies is `unknown`, not
pure and not absent.

### Spawn, slot, and identity

`GetNextSlot` scans the 1000-slot array (`Projectile.cs:10204-10218`). `NewProjectile` first
selects an inactive slot or calls `FindOldestProjectile`, then calls `SetDefaults`, assigns
slot, position, owner, velocity, damage, knockback and `identity`, evaluates
`Collision.WetCollision`, writes `Main.projectileIdentity`, applies source/banner/minion
metadata, initializes AI/UUID and may kill a golf-ball projectile or copy owner immunity
(`Projectile.cs:10220-10518`). `WetCollision` can write shared `Collision.honey` and
`Collision.shimmer`, so it is not a reorderable pure Query.

### Tick order and lifecycle

`Main` calls `PreUpdateAllProjectiles`, then iterates `n=0..999`, sets
`ProjectileUpdateLoopIndex`, and calls `projectile[n].Update(n)`; it resets the index and calls
`PostUpdateAllProjectiles` (`Main.cs:11386-11396,11530-11551`). `Projectile.Update` is at
`Projectile.cs:14693`; it decrements immunity counters, runs extra updates, bounds/minion
handling, water/collision handling, `AI`, damage, trail updates, decrements `timeLeft`, and
calls `Kill` when expired or penetration reaches zero (`:14693-15242`). Spawned projectiles can
therefore be observed later in the same 1000-slot pass, and slot ordering is an invariant.

`Kill` is at `Projectile.cs:46425`. It rejects inactive instances, removes the owner/identity
mapping, sets `timeLeft=0`, conditionally cancels the owner's channel, clears sound state, and
executes subtype effects, spawning, damage and network side effects. It is a lifecycle Command,
not a field setter or Query.

### AI, movement, collision, and combat

`AI` is the large dispatch at `Projectile.cs:18706`; subtype methods and many external Player,
NPC, tile, random and audio calls are not closed by the CPG callable facts. `Damage` and its
qualification path are at `Projectile.cs:11480-11538`; `CanHitWithMeleeWeapon` at `:11465`
consults Player state and `Collision`. `Colliding` is at `:13654` and has type/style-specific
geometry. `Collision` helpers can use shared static result flags. Combat therefore needs an
explicit collision snapshot and a commit owner; the report does not classify these methods as
pure merely because some branches return booleans.

### Network and external owners

Network packet 27 serializes identity, position, velocity, owner, type, optional AI, banner,
damage, knockback, original damage and UUID (`NetMessage.cs:769-843`). `MessageBuffer` packet
27 reads those values, finds by owner+identity, reuses an inactive or oldest slot, calls
`SetDefaults` when needed, writes state and may relay packet 27 (`MessageBuffer.cs:1320-1404`).
Packet 29 kills by slot/owner (`MessageBuffer.cs:1439-1453`). Player projectile scans and
counterweight spawns are in `Player.cs:6269-6303,6349-6409`; NPC and Player call `Damage` and
`Kill` at multiple sites. Owner/player state, NPC damage state, tile collision, audio/effects,
and network recipient selection are `crossSubsystemOwner: integration-review`.

## Complete P15 member coverage

The following is the complete P15 inventory. Names are copied from the claimed input; declaration
types and source coordinates remain in the input ledger. Every row is `status: proposed` for the
design owner and remains `verificationStatus: not-run`.

| Leaf group | Count | Proposed owner | Members |
| --- | ---: | --- | --- |
| `ProjectileIdentityAndClassificationState` | 16 | `ProjectileLifecycleSystem` + identity component | `active`, `perIDStaticNPCImmunity`, `ownerHitCheckDistance`, `arrow`, `numHits`, `bobber`, `netImportant`, `noDropItem`, `counterweight`, `scale`, `rotation`, `type`, `alpha`, `sentry`, `glowMask`, `owner` |
| `ProjectileAiState` | 4 | `ProjectileMotionAndAiSystem` | `maxAI`, `ai`, `localAI`, `aiStyle` |
| `ProjectileLifetimeAndRuntimeState` | 6 | `ProjectileLifecycleSystem` | `SentryLifeTime`, `ArrowLifeTime`, `gfxOffY`, `stepSpeed`, `timeLeft`, `soundDelay` |
| `ProjectileCombatAndImmunity` | 14 | `ProjectileCombatResolutionSystem` | `damage`, `originalDamage`, `spriteDirection`, `hostile`, `reflected`, `knockBack`, `friendly`, `penetrate`, `localNPCImmunity`, `usesLocalNPCImmunity`, `usesIDStaticNPCImmunity`, `appliesImmunityTimeOnSingleHits`, `maxPenetrate`, `identity` |
| `ProjectileMovementAndCollisionState` | 9 | `ProjectileMotionAndAiSystem` | `oldPos`, `oldRot`, `oldSpriteDirection`, `restrikeDelay`, `tileCollide`, `extraUpdates`, `stopsDealingDamageAfterPenetrateHits`, `numUpdates`, `ignoreWater` |
| `ProjectileNetworkReplicationState` | 4 | `ProjectileNetworkAdapter` / one-way projection | `netUpdate`, `netUpdate2`, `netSpam`, `netSyncSkippedForPlayer` |
| `ProjectileMinionAndPresentationState` | 13 | lifecycle/motion for minion facts; `ProjectilePresentationProjection` for render output | `light`, `minion`, `minionSlots`, `minionPos`, `isAPreviewDummy`, `isAPreviewDisplayDoll`, `MinionSpawnInfo`, `drawLayer`, `usesOwnerLight`, `hide`, `ownerHitCheck`, `usesOwnerMeleeHitCD`, `playerImmune` |
| `ProjectileDamageAndElementState` | 13 | `ProjectileCombatResolutionSystem` | `miscText`, `melee`, `ranged`, `magic`, `coldDamage`, `noEnchantments`, `noEnchantmentVisuals`, `trap`, `npcProj`, `originatedFromActivableTile`, `tagEffectType`, `bonusTagDamage`, `armorPenetration` |
| `ProjectileAnimationAndDirectionState` | 3 | `ProjectilePresentationSystem` with motion input | `frameCounter`, `frame`, `manualDirectionChange` |
| `ProjectileCollisionAndTargetingState` | 7 | `ProjectileCollisionTargetingSystem` | `projUUID`, `correctSlopeCollision`, `decidesManualFallThrough`, `shouldFallThrough`, `localNPCHitCooldown`, `idStaticNPCHitCooldown`, `bannerIdToRespondTo` |
| `ProjectileCombatScalingState` | 2 | `ProjectileCombatResolutionSystem` | `bonusCritChance`, `hostileDamageScaling` |
| `ProjectileCollisionGeometryCache` | 8 | `ProjectileGeometryQuery` (cache policy unresolved) | `_cachedConditions_solid`, `_cachedConditions_notNull`, `_javelinsMax6`, `_javelinsMax8`, `_javelinsMax10`, `WhipPointsForCollision`, `_lanceHitboxBounds`, `_lightningCollisionBounds` |
| `ProjectileTargetSelectionCache` | 7 | `ProjectileTargetSelectionQuery` (cache policy unresolved) | `_rainbowBoulderTargetsAny`, `_rainbowBoulderTargetsFar`, `_medusaHeadTargetList`, `_medusaTargetComparer`, `_ai164_blacklistedTargets`, `_ai158_blacklistedTargets`, `_ai156_blacklistedTargets` |
| `ProjectileFishingAndMiningQueryState` | 3 | `ProjectileToolQuery` (scratch ownership unresolved) | `_availableFishTypesToShow`, `_context`, `_miningHelperPointsToSkip` |
| `ProjectileKiteAndLightningRules` | 2 | `ProjectileSpecializedDefinitionQuery` | `StormLightningLiquidDamageRadius`, `MinimumWindStrengthToFlyKite` |
| `ProjectileDerivedProperties` | 8 | `ProjectileDerivedPropertiesQuery` | `Name`, `WipableTurret`, `Opacity`, `MaxUpdates`, `OwnerMinionAttackTargetNPC`, `OwnedBySomeone`, `CareForAttackCD`, `NetSectionCoordinates` |
| `ProjectileStormDefinition` | 7 | `ProjectileStormDefinitionCatalog` | `StartAngle`, `AnglePerBullet`, `BulletsInStorm`, `BulletsProgressInStormStartNormalized`, `BulletsProgressInStormBonusByIndexNormalized`, `StormTotalRange`, `BulletSize` |
| **Total** | **126** |  |  |

## Proposed ECS boundaries

All types in this section are proposals, not existing implementation claims.

### Components and state ownership

- `ProjectileIdentityAndClassificationStateComponent` owns the local slot, owner relation,
  content type, activation and classification flags. `identity`, `projUUID`, and owner-slot
  indexes remain distinct values; `ProjectileIdentityIndex` is an index, not authoritative
  entity state.
- `ProjectileAiStateComponent` owns bounded AI/local-AI arrays and style. It is mutated only by
  `ProjectileMotionAndAiSystem` or an explicitly validated network apply Command.
- `ProjectileLifetimeStateComponent` owns active/time-left/end reason. Expiry and explicit kill
  both enter `ProjectileLifecycleSystem`; no Query may terminate an instance.
- `ProjectileCombatStateComponent` owns damage, penetration, immunity and scaling. Applying a
  hit is a Command that rechecks active identity, target validity, immunity and penetration at
  commit time.
- `ProjectileMotionStateComponent` owns position/velocity, extra-update counters, tile/water
  policy and trail history. Tile collision is consumed as a snapshot; collision helpers do not
  become hidden state writers.
- `ProjectilePresentationStateComponent` owns frame, alpha/light, draw layer, trail and
  preview flags. It is a projection of authoritative state and cannot decide damage or lifetime.
- `ProjectileNetworkStateComponent` is a proposed per-instance send budget/pending state. A
  `ProjectileNetworkProjection` emits packet snapshots; it cannot become a second writer of
  gameplay fields.

### Systems

1. `ProjectileLifecycleSystem` — allocate/reuse a slot, hydrate from a definition, assign owner
   and identity, register/unregister the identity index, advance time-left, and commit recycle.
2. `ProjectileMotionAndAiSystem` — execute the ordered AI/motion step, extra updates, bounds and
   tile/water policy, with explicit random/time inputs. Empty Version4 subtype bodies remain
   `unknown` until source or trace evidence exists.
3. `ProjectileCollisionTargetingSystem` — produce collision/target candidates and apply validated
   hit eligibility. It owns no NPC or Player state.
4. `ProjectileCombatResolutionSystem` — apply damage, penetration and local/static immunity by
   explicit commands to the NPC/Player damage owner. It may call an external damage adapter but
   does not copy NPC/Player components into Projectile.
5. `ProjectilePresentationSystem` — advance trail/frame/opacity and publish render facts only.
6. `ProjectileNetworkAdapter` — parse/serialize packet 27/29 and section visibility; inbound
   data becomes a validated `ApplyProjectileNetworkCommand`.
7. `ProjectileSpecializedRuleSystem` — minion, counterweight, bobber, fishing/mining, kite and
   storm behavior; each capability is opt-in and hands lifecycle/combat changes back through
   commands.

### Commands, adapters, and projections

| Boundary | Proposed contract |
| --- | --- |
| `SpawnProjectileCommand` | Source snapshot + definition ID + position/velocity/damage intent; lifecycle owner rechecks slot and identity invariants before commit. |
| `ApplyProjectileNetworkCommand` | Decoded packet 27/29; parse and validate owner/type/identity/UUID before mutating one instance; duplicate packets are idempotent by owner+identity+UUID policy. |
| `ResolveProjectileHitCommand` | Collision candidate + target snapshot + expected generation; combat owner rechecks active state, immunity and penetration before applying external damage. |
| `TerminateProjectileCommand` | Reason and expected handle; lifecycle owner clears indexes, time-left, channel relation and emits effects through adapters. |
| `ProjectileCollisionAdapter` | Converts tile/world collision to an immutable result. Shared `Collision.honey/shimmer` writes remain an integration risk. |
| `ProjectileDamageAdapter` | Sends a damage intent to NPC/Player damage owner; no reverse ownership of projectile fields. |
| `ProjectileNetworkProjection` | Serializes the authoritative snapshot and recipient/section decision. It never applies inbound state directly. |
| `ProjectileDefinitionCatalogAdapter` | Maps content type/persistent IDs to immutable definitions. Current `IProjectileDefinitionQuery` is a query surface, not proof of legacy `SetDefaults` coverage. |

## Query API and purity decisions

The Query API is a responsibility label with an explicit purity contract: return a value or
immutable snapshot, do not write observable authoritative state, document repeatability and
cache invalidation, and recheck invariants at Command commit.

| Legacy operation | Proposed role | Purity status |
| --- | --- | --- |
| `FindOldestProjectile` | Internal allocation decision in lifecycle Command | Not a standalone Query: separating scan and allocation creates a TOCTOU window. |
| `GetNextSlot` | Internal allocation helper | Same TOCTOU restriction; Query-like scan is not externally exposed. |
| `IsNPCIndexImmuneToProjectileType` | Snapshot qualification Query | Frame/time dependent (`Main.GameUpdateCount`); repeatability only within a captured frame snapshot. |
| `Colliding` | Geometry candidate Query | `unknown` until all `Collision` helpers and type dispatch are closed; no write-back allowed in proposed API. |
| `CanHitWithMeleeWeapon` | Candidate Query | `partial`: reads Player and collision state; exact lazy/callback behavior is not closed. |
| `Collision.WetCollision` | Adapter/mixed operation | It writes shared honey/shimmer flags; not a pure Query and must run in the spawn/update command sequence. |
| storm geometry and derived properties | Snapshot Query / Definition view | Candidate pure reads, but cache lifetime, random/time inputs and lazy initialization are `unknown`. |
| fishing/mining and target caches | Query with explicit scratch snapshot | Existing static lists are mutable scratch; ownership and invalidation are `unknown`. |

## Dependency direction and required order

The proposed dependency DAG is:

`definition/catalog -> SpawnProjectileCommand -> LifecycleSystem -> MotionAndAiSystem ->
CollisionTargetingSystem -> CombatResolutionSystem -> external NPC/Player damage owner`.

`PresentationSystem` and `NetworkProjection` read committed snapshots. `NetworkAdapter` feeds
only validated Commands. `Player`, `NPC`, `Collision`, `WorldGen`, audio/effect and persistence
owners are adapters or integration handoffs. The source order that must be preserved is:

1. `Main.PreUpdateAllProjectiles`.
2. For each slot in ascending order, `Projectile.Update`, including extra updates and AI.
3. Collision/water effects and `Damage` in the same update iteration.
4. Trail/frame updates, `timeLeft--`, penetration termination, and network send decision.
5. `Main.PostUpdateAllProjectiles`.

Spawn and network receive may reuse a slot during the pass; any proposed schedule must preserve
the source's slot index and `ProjectileUpdateLoopIndex` behavior. Kill/recycle is a commit at the
point observed by callers, not a deferred best-effort cleanup.

## Current NLTX state

Directly inspected target material includes `Content/ProjectileDefinition.cs`, the behavior,
combat, capabilities, geometry, penetration, identity and presentation definitions,
`IProjectileDefinitionQuery.cs`, `ProjectileDefinitionCatalog.cs`, and the runtime candidate
components/helpers under `src/NSSLC/Component/Projectile` (identity, lifetime, damage, behavior,
network, immunity, penetration, disposition, reflection, trail and animation). Existing
examples include `ProjectileLifetimeSystem`, `ProjectileNetworkStateSystem`,
`ProjectileIdentityIndex`, and `IProjectileDefinitionQuery`. These are useful proposed
boundaries and local value rules. A search did not provide evidence of a complete authoritative
projectile update coordinator that owns the Version4 `Main` loop, AI dispatch, collision, combat,
network apply and recycle closure. That missing owner is `unknown`; no code is changed here.

The existing content catalog query is intentionally used as a read API:
`ProjectileDefinitionCatalog.TryGet` is a dictionary lookup. It does not prove that all
Version4 `SetDefaults` side effects, subtype AI, source metadata, UUID assignment or network
hydration are represented.

## Persistence, networking, and cross-subsystem ownership

- Persist only stable content ID, owner relation, identity/UUID policy, lifecycle state and
  gameplay state that the accepted design declares durable. Slot indexes and derived caches must
  be rebuildable; exact Version4 persistence coverage is `unknown`.
- Packet 27/29 and section visibility belong to the network adapter/projection. Decode-before-
  commit and owner+identity matching are required. Recipient selection and spam throttling need
  focused evidence before implementation.
- NPC/Player damage, minion capacity, channel cancellation, counterweight/fishing relations,
  tile collision/liquid flags, world sections, audio, dust, particles and spawned NPC/projectile
  effects have `crossSubsystemOwner: integration-review`. P15 does not claim those owners.

## Focused verifier plan (not executed)

1. **Lifecycle and slot verifier:** spawn into free and full pools, oldest replacement, duplicate
   owner+identity packets, UUID mismatch, kill/recycle, index rebuild and repeated termination.
2. **Tick order verifier:** capture `PreUpdate`, ascending slot order, extra-update count, AI,
   collision, `Damage`, trail, expiry and `PostUpdate` events; compare against the source order.
3. **Combat verifier:** friendly/hostile qualification, local/static immunity, penetration,
   owner hit check, NPC/Player damage handoff and target invalidation; assert no second writer.
4. **Network verifier:** packet 27 optional fields, packet 29 kill, section recipient filtering,
   net spam budget, malformed/duplicate packets and idempotent reapply.
5. **Query verifier:** repeatability under a fixed snapshot, no authoritative writes, cache
   invalidation, random/clock consumption, and Command-time invariant rechecks.
6. **Specialized behavior verifier:** minion/counterweight/bobber/fishing/mining/kite/storm
   paths, including currently empty Version4 bodies marked `unknown`.

All verifier items are plans only. `verificationStatus: not-run`; no build, test, runtime,
benchmark, migration, or generated artifact command was run.

## Evidence gaps and blocking decisions

| Gap / blocker | Status | Decision required before implementation |
| --- | --- | --- |
| Complete write/read closure for 126 members, including reflection, event handlers and dynamic subtype dispatch | `partial` / `unknown` | Enumerate all writers and choose one commit owner per invariant. |
| `Projectile.Update` callee effects, random consumption, exception paths and all subtype AI bodies | `partial`; empty bodies present | Obtain accepted complete source or behavior traces for this exact Version4 revision. |
| Collision shared-static flags and lazy tile behavior | `unknown` | Define a snapshot adapter and preserve call ordering before exposing Query APIs. |
| Network receive, duplicate identity, UUID, section visibility and persistence closure | `partial` | Complete packet/state matrix and malformed-input policy. |
| Player/NPC damage, channel, minion, fishing/mining and spawned-effect ownership | `crossSubsystemOwner: integration-review` | Agree integration contracts and event/Command direction. |
| Existing NLTX candidate components versus a single runtime coordinator | `unknown` | Locate or designate the authoritative scheduler before implementation. |
| Source stubs and generated CPG snapshot binding | `unknown` / `partial` | Reconcile source hashes and add focused verifier evidence; no negative conclusion from missing CPG hits. |

## Compatibility and non-split decisions

The proposed migration contract keeps legacy identity, owner, slot, UUID, AI arrays, timing,
collision, damage, network and kill ordering observable through adapters until each owner is
verified. No component is split per field, per frame, per collision result, per target, per
network packet or per presentation value. Definition/catalog data remains immutable and separate
from instance state; target and geometry caches remain Query-owned scratch until invalidation is
proven. No Player or NPC state is copied into Projectile components.

## Integration Handoff

- `ProjectileLifecycleSystem` owns allocation, identity registration, active/time-left transitions
  and recycle commands after integration review of Player channel and storage indexes.
- `ProjectileMotionAndAiSystem` owns AI/motion writes after Collision, WorldGen, random/time and
  subtype behavior contracts are accepted.
- `ProjectileCombatResolutionSystem` owns penetration/immunity decisions and emits damage intents;
  NPC/Player systems own damage application.
- `ProjectileNetworkAdapter` and `ProjectileNetworkProjection` own packet framing and snapshot
  direction; network recipient/section policy remains integration review.
- `ProjectilePresentationSystem` owns only derived/render state and must not write gameplay
  authority.
- All shared IDs, entity references, minion capacity, fishing/mining relations, counterweight
  links, tile flags, persistence keys and effect spawns remain `crossSubsystemOwner:
  integration-review`.

## Final declaration

P15 is a static `proposed` System/Component/Query/Command/Adapter/Projection design based on the
claimed 17 leaves and 126 members. Evidence gaps are explicitly `unknown`, `partial`, or
`evidence-gap`; verification is `not-run`. The Version4 source and NLTX production code were not
modified, and no claim of migration success, behavior equivalence, API compatibility, or passing
verification is made.
