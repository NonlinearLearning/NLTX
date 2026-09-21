---
partitionId: P05
sessionId: dfb1d7e5d4734d2d96d9bda4e65f92a9
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\05-spatial-motion-physics.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P05-spatial-motion-physics-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P05-spatial-motion-physics-component-execution.md
designStatus: proposed
executionStatus: completed (src2-only implementation checkpoint)
implementationStatus: completed
verificationStatus: partial (serial build and focused verifier passed; integration and behavior-equivalence verification remain)
completedComponents: C01 EntityBoundsAndGeometry; C02 EntityLiquidContact; C03 CollisionContactPayloads; C04 TeleportQueries; C05 PortalTraversal; C06 PylonRegistry; C07 ShimmerUnstuck; C08 MinecartTrackDefinitions (including MinecartCustomization); C09 MinecartPresentationDefinitions; C10 ProjectileDefinitionCatalog; C11 TrackedProjectileReference; C12 DarknessHazardState; C13 SeatMetadata; C14 MainRuntimeBoundary (src2 source saved and focused-verified)
currentComponent: none (src2 implementation and focused verification complete)
pendingComponents: none for the P05 src2 implementation; integration-review and broader behavior-equivalence verification remain
lastCheckpointUtc: 2026-09-12T06:59:52Z
evidence-gap: First-round P05 public-decomposition output is absent at D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P05-spatial-motion-physics-public-decomposition.md; common constraint file and the cited Version4-difference appendix are also absent at their expected paths; darkness timer tick writer, complete network/persistence/client projection paths, and final cross-partition owners remain partial or missing; the focused verifier does not prove full Version4 behavior equivalence or production integration.
blocking-decision: integration-review must still decide the owner of entity position/size writers, Tile and liquid access, portal and teleport transactions, pylon network projection, Minecart track catalogs, projectile IDs, and final scheduler order; these decisions block production wiring and equivalence acceptance, not the saved src2 implementation.
---

# Version4 非权威 P05：空间移动与物理实施计划

本文件记录 P05 的 src2-only 实施结果和后续整合边界。组件边界、接口、System、Query、Command、Adapter、Projection 和最终 owner 仍为 proposed；C01-C14 的实际 C# 源码已经保存，受影响项目已通过仓库串行 build，focused verifier 已通过。本记录不宣称生产 `src` 迁移、完整行为等价或跨分区 owner 已闭合。

## 1. 实施前置和目标目录

输入报告包含 119 条成员（99 field、20 property），SHA-256 为 b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196。以下嵌套目录和文件仍是 proposed 生产边界，不应视为生产 `src` 已存在；本轮实际 src2 文件集中在后面的 implementation record。生产接入仍需先由 integration-review 解决 owner/blocking-decision：

| 能力 | proposed 目标文件 | 预期命名空间/边界 | 单一写入 owner 候选 |
|---|---|---|---|
| Entity bounds | src2/Entity/Spatial/EntityBoundsComponent.cs；src2/Entity/Spatial/Queries/EntityGeometryQuery.cs；src2/Entity/Spatial/Commands/BoundsMutationCommand.cs | proposed Terraria.Entity.Spatial | proposed BoundsCommitSystem；position owner integration-review |
| Liquid contact | src2/Entity/Spatial/EntityLiquidContactComponent.cs；src2/Entity/Spatial/Queries/EntityLiquidContactQuery.cs；src2/Entity/Spatial/Systems/LiquidContactSystem.cs | proposed Terraria.Entity.Spatial | proposed LiquidContactSystem |
| Collision payload | src2/Physics/Collision/BallCollisionPayload.cs；src2/Physics/Collision/BallPassThroughPayload.cs；src2/Physics/Collision/PhysicsPropertiesValue.cs | proposed Terraria.Physics.Collision | proposed CollisionResolutionSystem consumes only |
| Teleport queries | src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs；src2/Spatial/Teleport/InterceptionResult.cs；src2/Spatial/Teleport/Queries/TeleportCandidateQuery.cs；src2/Spatial/Teleport/Adapters/TileConditionAdapter.cs | proposed Terraria.Spatial.Teleport | proposed TeleportCandidateQuery is read-only |
| Portal traversal | src2/Spatial/Portal/PortalTraversalRuntimeCache.cs；src2/Spatial/Portal/Definitions/PortalGeometryDefinition.cs；src2/Spatial/Portal/Systems/PortalTraversalSystem.cs；src2/Spatial/Teleport/Commands/TeleportCommitCommand.cs | proposed Terraria.Spatial.Portal | proposed PortalTraversalSystem enqueues; integration-review commits |
| Pylon registry | src2/World/Teleport/TeleportPylonSnapshotEntry.cs；src2/World/Teleport/PylonRegistrySnapshot.cs；src2/World/Teleport/Adapters/PylonRegistryAdapter.cs；src2/World/Teleport/Projections/PylonRegistryProjection.cs | proposed Terraria.World.Teleport | proposed PylonRegistryAdapter reads; Projection sends |
| Shimmer unstuck | src2/Entity/Spatial/ShimmerUnstuckStateComponent.cs；src2/Entity/Spatial/Systems/ShimmerUnstuckSystem.cs；src2/Entity/Spatial/Queries/ShimmerUnstuckQuery.cs | proposed Terraria.Entity.Spatial | proposed ShimmerUnstuckSystem |
| Minecart track | src2/Vehicle/Minecart/Definitions/MinecartMotionFlagDefinition.cs；src2/Vehicle/Minecart/Definitions/MinecartTrackDefinitionCatalog.cs；src2/Vehicle/Minecart/Queries/MinecartTrackQuery.cs；src2/Vehicle/Minecart/Systems/MinecartMotionSystem.cs | proposed Terraria.Vehicle.Minecart | proposed MinecartMotionSystem for motion command |
| Minecart presentation | src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs；src2/Vehicle/Minecart/Queries/MinecartPresentationQuery.cs | proposed Terraria.Vehicle.Minecart | proposed catalog bootstrap; renderer owner integration-review |
| Projectile catalog | src2/Projectile/Definitions/ProjectileDefinitionCatalog.cs；src2/Projectile/Definitions/ProjectileDefinitionCatalogAdapter.cs | proposed Terraria.Projectile.Definitions | proposed content bootstrap; ProjectileID owner integration-review |
| Projectile reference | src2/Projectile/Tracking/TrackedProjectileReferenceValue.cs；src2/Projectile/Tracking/Queries/TrackedProjectileReferenceQuery.cs；src2/Projectile/Tracking/Adapters/ReferenceProtocolAdapter.cs | proposed Terraria.Projectile.Tracking | proposed adapter only; relation owner integration-review |
| Darkness hazard | src2/World/Environment/Definitions/DarknessHazardDefinition.cs；src2/World/Environment/DarknessHazardStateComponent.cs；src2/World/Environment/Systems/DarknessHazardSystem.cs；src2/World/Environment/Projections/DarknessMessageProjection.cs | proposed Terraria.World.Environment | proposed DarknessHazardSystem after tick writer is found |
| Seat metadata | src2/Player/Interaction/SeatMetadataValue.cs；src2/Player/Interaction/Queries/SeatMetadataQuery.cs；src2/Player/Interaction/Adapters/SittingProjectionAdapter.cs | proposed Terraria.Player.Interaction | proposed sitting system/adapter; Player owner integration-review |
| Main boundary | src2/WorldSession/Runtime/SpawnCadenceState.cs；src2/WorldSession/Runtime/Systems/SpawnCadenceSystem.cs；src2/WorldSession/Runtime/Projections/HelpTextProjectionState.cs；src2/WorldSession/Runtime/Adapters/MainRuntimeBoundaryAdapter.cs；src2/WorldSession/Lighting/Adapters/LightingDefinitionAdapter.cs | proposed Terraria.WorldSession | proposed adapters; UI/startup/lighting owner integration-review |

命名遵循 capability-first、同名 public type 与 proposed 文件一一对应的方向；不得创建 Shared/Components 等泛化目录，也不得用文件顺序暗示调度顺序。

### 1.1 实际 implementation record

本轮实际修改范围仅为 `D:\TRbackup\NLTX\src2`。P05 生产候选项目为 `src2/SpatialMotionPhysics/Terraria.SpatialMotionPhysics.csproj`，focused verifier 项目为 `src2/SpatialMotionPhysicsVerification/Terraria.SpatialMotionPhysicsVerification.csproj`。以下是本轮保存的实际 src2 文件：

**P05 library（68 个 C# 文件）**

```text
src2/SpatialMotionPhysics/BallCollisionPayload.cs
src2/SpatialMotionPhysics/BallPassThroughPayload.cs
src2/SpatialMotionPhysics/BallPassThroughType.cs
src2/SpatialMotionPhysics/BoundsAnchor.cs
src2/SpatialMotionPhysics/BoundsMutationCommand.cs
src2/SpatialMotionPhysics/BoundsMutationSystem.cs
src2/SpatialMotionPhysics/CollisionResolutionSystem.cs
src2/SpatialMotionPhysics/DarknessHazardDefinition.cs
src2/SpatialMotionPhysics/DarknessHazardStateComponent.cs
src2/SpatialMotionPhysics/DarknessHazardSystem.cs
src2/SpatialMotionPhysics/DarknessHazardTickResult.cs
src2/SpatialMotionPhysics/EntityBoundsComponent.cs
src2/SpatialMotionPhysics/EntityGeometryQuery.cs
src2/SpatialMotionPhysics/EntityGeometrySnapshot.cs
src2/SpatialMotionPhysics/EntityHitbox.cs
src2/SpatialMotionPhysics/EntityLiquidContactComponent.cs
src2/SpatialMotionPhysics/EntityLiquidContactQuery.cs
src2/SpatialMotionPhysics/EntityMotionState.cs
src2/SpatialMotionPhysics/EntityReference.cs
src2/SpatialMotionPhysics/EntitySpatialState.cs
src2/SpatialMotionPhysics/HelpTextProjectionState.cs
src2/SpatialMotionPhysics/ICollisionEffectSink.cs
src2/SpatialMotionPhysics/InterceptionQuery.cs
src2/SpatialMotionPhysics/InterceptionResult.cs
src2/SpatialMotionPhysics/LightingDefinitionAdapter.cs
src2/SpatialMotionPhysics/LiquidContactSample.cs
src2/SpatialMotionPhysics/LiquidContactSystem.cs
src2/SpatialMotionPhysics/MainRuntimeBoundaryAdapter.cs
src2/SpatialMotionPhysics/MinecartCustomizationValue.cs
src2/SpatialMotionPhysics/MinecartMotionFlagDefinition.cs
src2/SpatialMotionPhysics/MinecartMotionState.cs
src2/SpatialMotionPhysics/MinecartMotionSystem.cs
src2/SpatialMotionPhysics/MinecartPresentationDefinitionCatalog.cs
src2/SpatialMotionPhysics/MinecartPresentationQuery.cs
src2/SpatialMotionPhysics/MinecartTrackDefinitionCatalog.cs
src2/SpatialMotionPhysics/MinecartTrackQuery.cs
src2/SpatialMotionPhysics/MinecartTrackSample.cs
src2/SpatialMotionPhysics/MinecartTrackType.cs
src2/SpatialMotionPhysics/PhysicsPropertiesValue.cs
src2/SpatialMotionPhysics/Point16.cs
src2/SpatialMotionPhysics/PortalCooldownState.cs
src2/SpatialMotionPhysics/PortalGeometryDefinitionCatalog.cs
src2/SpatialMotionPhysics/PortalTraversalCandidate.cs
src2/SpatialMotionPhysics/PortalTraversalRuntimeCache.cs
src2/SpatialMotionPhysics/PortalTraversalSystem.cs
src2/SpatialMotionPhysics/ProjectileDefinitionCatalog.cs
src2/SpatialMotionPhysics/ProjectileDefinitionCatalogAdapter.cs
src2/SpatialMotionPhysics/PylonRegistryAdapter.cs
src2/SpatialMotionPhysics/PylonRegistryProjection.cs
src2/SpatialMotionPhysics/PylonRegistrySnapshot.cs
src2/SpatialMotionPhysics/RandomTeleportationAttemptInput.cs
src2/SpatialMotionPhysics/ReferenceProtocolAdapter.cs
src2/SpatialMotionPhysics/SeatMetadataQuery.cs
src2/SpatialMotionPhysics/SeatMetadataValue.cs
src2/SpatialMotionPhysics/ShimmerUnstuckQuery.cs
src2/SpatialMotionPhysics/ShimmerUnstuckStateComponent.cs
src2/SpatialMotionPhysics/ShimmerUnstuckSystem.cs
src2/SpatialMotionPhysics/SittingProjectionAdapter.cs
src2/SpatialMotionPhysics/SpawnCadenceState.cs
src2/SpatialMotionPhysics/SpawnCadenceSystem.cs
src2/SpatialMotionPhysics/TeleportCandidateQuery.cs
src2/SpatialMotionPhysics/TeleportCandidateResult.cs
src2/SpatialMotionPhysics/TeleportCommitCommand.cs
src2/SpatialMotionPhysics/TeleportCommitSystem.cs
src2/SpatialMotionPhysics/TeleportPylonSnapshotEntry.cs
src2/SpatialMotionPhysics/TileHandle.cs
src2/SpatialMotionPhysics/TrackedProjectileReferenceQuery.cs
src2/SpatialMotionPhysics/TrackedProjectileReferenceValue.cs
```

**P05 verifier（2 个文件）**

```text
src2/SpatialMotionPhysicsVerification/Program.cs
src2/SpatialMotionPhysicsVerification/Terraria.SpatialMotionPhysicsVerification.csproj
```

这份清单描述实际保存路径；设计表中的嵌套目标路径仍是 capability-first 的 proposed integration shape。没有向 `src` 写入 C# 或项目文件。

## 2. 119 条源成员到 proposed role/file 映射

以下是执行阶段的逐成员映射。设计目标路径和 compatibility action 仍描述 proposed 边界或未来生产接入；本轮实际保存的 src2 文件集中列在下面的 implementation record，不把这些记录解释为生产 call-site 已切换。

### C01 EntityBoundsAndGeometry

| 序号 | 源成员和 Version4 行 | proposed role | proposed target file | compatibility action |
|---:|---|---|---|---|
| 3067 | Entity.width；Entity.cs:22 | component state width | src2/Entity/Spatial/EntityBoundsComponent.cs | legacy read adapter；单一 writer |
| 3068 | Entity.height；Entity.cs:24 | component state height | src2/Entity/Spatial/EntityBoundsComponent.cs | legacy read adapter；单一 writer |
| 3945 | Entity.VisualPosition；Entity.cs:48 | derived query | src2/Entity/Spatial/Queries/EntityGeometryQuery.cs | retain legacy read facade |
| 3946 | Entity.Center；Entity.cs:50 | geometry query plus bounds command input | src2/Entity/Spatial/Queries/EntityGeometryQuery.cs；src2/Entity/Spatial/Commands/BoundsMutationCommand.cs | translate setter intent, no double write |
| 3947 | Entity.Left；Entity.cs:62 | geometry query plus bounds command input | src2/Entity/Spatial/Queries/EntityGeometryQuery.cs；src2/Entity/Spatial/Commands/BoundsMutationCommand.cs | translate setter intent, verify anchor |
| 3948 | Entity.Right；Entity.cs:74 | geometry query plus bounds command input | src2/Entity/Spatial/Queries/EntityGeometryQuery.cs；src2/Entity/Spatial/Commands/BoundsMutationCommand.cs | translate setter intent, verify anchor |
| 3949 | Entity.Top；Entity.cs:86 | geometry query plus bounds command input | src2/Entity/Spatial/Queries/EntityGeometryQuery.cs；src2/Entity/Spatial/Commands/BoundsMutationCommand.cs | translate setter intent, verify anchor |
| 3950 | Entity.TopLeft；Entity.cs:98 | geometry query plus bounds command input | src2/Entity/Spatial/Queries/EntityGeometryQuery.cs；src2/Entity/Spatial/Commands/BoundsMutationCommand.cs | translate setter intent, verify anchor |
| 3951 | Entity.TopRight；Entity.cs:110 | geometry query plus bounds command input | src2/Entity/Spatial/Queries/EntityGeometryQuery.cs；src2/Entity/Spatial/Commands/BoundsMutationCommand.cs | translate setter intent, verify anchor |
| 3952 | Entity.Bottom；Entity.cs:122 | geometry query plus bounds command input | src2/Entity/Spatial/Queries/EntityGeometryQuery.cs；src2/Entity/Spatial/Commands/BoundsMutationCommand.cs | translate setter intent, verify anchor |
| 3953 | Entity.BottomLeft；Entity.cs:134 | geometry query plus bounds command input | src2/Entity/Spatial/Queries/EntityGeometryQuery.cs；src2/Entity/Spatial/Commands/BoundsMutationCommand.cs | translate setter intent, verify anchor |
| 3954 | Entity.BottomRight；Entity.cs:146 | geometry query plus bounds command input | src2/Entity/Spatial/Queries/EntityGeometryQuery.cs；src2/Entity/Spatial/Commands/BoundsMutationCommand.cs | translate setter intent, verify anchor |
| 3955 | Entity.Size；Entity.cs:158 | derived size plus bounds command input | src2/Entity/Spatial/Queries/EntityGeometryQuery.cs；src2/Entity/Spatial/Commands/BoundsMutationCommand.cs | preserve int cast semantics |
| 3956 | Entity.Hitbox；Entity.cs:171 | derived hitbox plus bounds command input | src2/Entity/Spatial/Queries/EntityGeometryQuery.cs；src2/Entity/Spatial/Commands/BoundsMutationCommand.cs | preserve position/size atomicity |

### C02 EntityLiquidContact

| 序号 | 源成员和 Version4 行 | proposed role | proposed target file | compatibility action |
|---:|---|---|---|---|
| 3069 | Entity.wet；Entity.cs:26 | liquid contact state | src2/Entity/Spatial/EntityLiquidContactComponent.cs | shadow-read, then switch single writer |
| 3070 | Entity.shimmerWet；Entity.cs:28 | liquid contact state | src2/Entity/Spatial/EntityLiquidContactComponent.cs | shadow-read, then switch single writer |
| 3071 | Entity.honeyWet；Entity.cs:30 | liquid contact state | src2/Entity/Spatial/EntityLiquidContactComponent.cs | shadow-read, then switch single writer |
| 3072 | Entity.wetCount；Entity.cs:32 | liquid contact count | src2/Entity/Spatial/EntityLiquidContactComponent.cs | compare reset/overflow behavior |
| 3073 | Entity.lavaWet；Entity.cs:34 | liquid contact state | src2/Entity/Spatial/EntityLiquidContactComponent.cs | shadow-read, then switch single writer |
| 3944 | Entity.AnyWet；Entity.cs:36 | derived query | src2/Entity/Spatial/Queries/EntityLiquidContactQuery.cs | retain legacy facade; no cached duplicate |

### C03 CollisionContactPayloads

| 序号 | 源成员和 Version4 行 | proposed role | proposed target file | compatibility action |
|---:|---|---|---|---|
| 2880 | BallCollisionEvent.Normal；BallCollisionEvent.cs:7 | transient collision payload | src2/Physics/Collision/BallCollisionPayload.cs | adapter from legacy event |
| 2881 | BallCollisionEvent.ImpactPoint；BallCollisionEvent.cs:9 | transient collision payload | src2/Physics/Collision/BallCollisionPayload.cs | adapter from legacy event |
| 2882 | BallCollisionEvent.Tile；BallCollisionEvent.cs:11 | external Tile handle adapter | src2/Physics/Collision/Adapters/TileCollisionAdapter.cs | no direct Tile in core payload |
| 2883 | BallCollisionEvent.Entity；BallCollisionEvent.cs:13 | external entity reference adapter | src2/Physics/Collision/Adapters/EntityReferenceAdapter.cs | reject stale reference explicitly |
| 2884 | BallCollisionEvent.TimeScale；BallCollisionEvent.cs:15 | transient collision payload | src2/Physics/Collision/BallCollisionPayload.cs | compare float/clock semantics |
| 2885 | BallPassThroughEvent.Tile；BallPassThroughEvent.cs:5 | external Tile handle adapter | src2/Physics/Collision/Adapters/TileCollisionAdapter.cs | no direct Tile in core payload |
| 2886 | BallPassThroughEvent.Entity；BallPassThroughEvent.cs:7 | external entity reference adapter | src2/Physics/Collision/Adapters/EntityReferenceAdapter.cs | reject stale reference explicitly |
| 2887 | BallPassThroughEvent.Type；BallPassThroughEvent.cs:9 | transient pass-through payload | src2/Physics/Collision/BallPassThroughPayload.cs | map enum exhaustively |
| 2888 | BallPassThroughEvent.TimeScale；BallPassThroughEvent.cs:11 | transient pass-through payload | src2/Physics/Collision/BallPassThroughPayload.cs | compare float/clock semantics |
| 2889 | PhysicsProperties.Gravity；PhysicsProperties.cs:5 | physics input value | src2/Physics/Collision/PhysicsPropertiesValue.cs | preserve readonly construction |
| 2890 | PhysicsProperties.Drag；PhysicsProperties.cs:7 | physics input value | src2/Physics/Collision/PhysicsPropertiesValue.cs | preserve readonly construction |

### C04 TeleportQueries

| 序号 | 源成员和 Version4 行 | proposed role | proposed target file | compatibility action |
|---:|---|---|---|---|
| 3627 | RandomTeleportationAttemptSettings.teleporteeSize；Utils.cs:40 | query input | src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | field-by-field adapter |
| 3628 | RandomTeleportationAttemptSettings.teleporteeVelocity；Utils.cs:42 | query input | src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | field-by-field adapter |
| 3629 | RandomTeleportationAttemptSettings.teleporteeGravityDirection；Utils.cs:44 | query input | src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | field-by-field adapter |
| 3630 | RandomTeleportationAttemptSettings.mostlySolidFloor；Utils.cs:46 | query input | src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | field-by-field adapter |
| 3631 | RandomTeleportationAttemptSettings.avoidLava；Utils.cs:48 | query input | src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | field-by-field adapter |
| 3632 | RandomTeleportationAttemptSettings.avoidAnyLiquid；Utils.cs:50 | query input | src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | map to LiquidContactQuery |
| 3633 | RandomTeleportationAttemptSettings.avoidHurtTiles；Utils.cs:52 | query input | src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | map to Tile hazard adapter |
| 3634 | RandomTeleportationAttemptSettings.avoidWalls；Utils.cs:54 | query input | src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | map to collision query |
| 3635 | RandomTeleportationAttemptSettings.attemptsBeforeGivingUp；Utils.cs:56 | query input | src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | inject explicit attempt policy |
| 3636 | RandomTeleportationAttemptSettings.maximumFallDistanceFromOrignalPoint；Utils.cs:58 | query input | src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | preserve source spelling at adapter boundary |
| 3637 | RandomTeleportationAttemptSettings.strictRange；Utils.cs:60 | query input | src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | field-by-field adapter |
| 3638 | RandomTeleportationAttemptSettings.tilesToAvoid；Utils.cs:62 | query input | src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | clone/immutable snapshot |
| 3639 | RandomTeleportationAttemptSettings.tilesToAvoidRange；Utils.cs:64 | query input | src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | validate range |
| 3640 | RandomTeleportationAttemptSettings.allowSolidTopFloor；Utils.cs:66 | query input | src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | field-by-field adapter |
| 3641 | RandomTeleportationAttemptSettings.specializedConditions；Utils.cs:68 | Tile condition adapter seam | src2/Spatial/Teleport/Adapters/TileConditionAdapter.cs | prohibit arbitrary core callback ownership |
| 3642 | ChaseResults.InterceptionHappens；Utils.cs:73 | query output | src2/Spatial/Teleport/InterceptionResult.cs | adapter output only |
| 3643 | ChaseResults.InterceptionPosition；Utils.cs:75 | query output | src2/Spatial/Teleport/InterceptionResult.cs | adapter output only |
| 3644 | ChaseResults.InterceptionTime；Utils.cs:77 | query output | src2/Spatial/Teleport/InterceptionResult.cs | adapter output only |
| 3645 | ChaseResults.ChaserVelocity；Utils.cs:79 | query output | src2/Spatial/Teleport/InterceptionResult.cs | adapter output only |
| 3646 | Utils.MaxCoins；Utils.cs:82 | deferred integration constant | no spatial target; integration-review | leave in source boundary until economy owner decides |

### C05 PortalTraversal

| 序号 | 源成员和 Version4 行 | proposed role | proposed target file | compatibility action |
|---:|---|---|---|---|
| 2457 | PortalHelper.PORTALS_PER_PERSON；PortalHelper.cs:10 | definition constant | src2/Spatial/Portal/Definitions/PortalGeometryDefinitionCatalog.cs | compare initialization value |
| 2458 | PortalHelper.FoundPortals；PortalHelper.cs:12 | runtime pair cache | src2/Spatial/Portal/PortalTraversalRuntimeCache.cs | shadow-read and invalidate per tick |
| 2459 | PortalHelper.PortalCooldownForPlayers；PortalHelper.cs:14 | cooldown state candidate | src2/Spatial/Portal/PortalCooldownState.cs | do not duplicate until owner selected |
| 2460 | PortalHelper.PortalCooldownForNPCs；PortalHelper.cs:16 | cooldown state candidate | src2/Spatial/Portal/PortalCooldownState.cs | do not duplicate until owner selected |
| 2461 | PortalHelper.EDGES；PortalHelper.cs:18 | geometry definition | src2/Spatial/Portal/Definitions/PortalGeometryDefinition.cs | immutable catalog load |
| 2462 | PortalHelper.SLOPE_EDGES；PortalHelper.cs:20 | geometry definition | src2/Spatial/Portal/Definitions/PortalGeometryDefinition.cs | immutable catalog load |
| 2463 | PortalHelper.SLOPE_OFFSETS；PortalHelper.cs:22 | geometry definition | src2/Spatial/Portal/Definitions/PortalGeometryDefinition.cs | immutable catalog load |
| 2464 | PortalHelper.anyPortalAtAll；PortalHelper.cs:24 | derived availability cache | src2/Spatial/Portal/Queries/PortalAvailabilityQuery.cs | shadow compare; no core global mutable read |

### C06 PylonRegistry

| 序号 | 源成员和 Version4 行 | proposed role | proposed target file | compatibility action |
|---:|---|---|---|---|
| 2542 | TeleportPylonInfo.PositionInTiles；TeleportPylonInfo.cs:8 | snapshot entry coordinate | src2/World/Teleport/TeleportPylonSnapshotEntry.cs | convert Point16 to stable snapshot value |
| 2543 | TeleportPylonInfo.TypeOfPylon；TeleportPylonInfo.cs:10 | snapshot entry type | src2/World/Teleport/TeleportPylonSnapshotEntry.cs | validate content type mapping |

### C07 ShimmerUnstuck

| 序号 | 源成员和 Version4 行 | proposed role | proposed target file | compatibility action |
|---:|---|---|---|---|
| 2504 | ShimmerUnstuckHelper.TimeLeftUnstuck；ShimmerUnstuckHelper.cs:7 | entity short-lived state | src2/Entity/Spatial/ShimmerUnstuckStateComponent.cs | shadow-read timer and compare expiry |
| 2505 | ShimmerUnstuckHelper.IndefiniteProtectionActive；ShimmerUnstuckHelper.cs:9 | entity short-lived state | src2/Entity/Spatial/ShimmerUnstuckStateComponent.cs | shadow-read flag and compare start/clear |
| 3856 | ShimmerUnstuckHelper.ShouldUnstuck；ShimmerUnstuckHelper.cs:11 | derived query | src2/Entity/Spatial/Queries/ShimmerUnstuckQuery.cs | compare truth table; no duplicate cache |

### C08 MinecartTrackDefinitions

| 3313 | Minecart.Customization.MinecartTextureWidth；Minecart.cs:23 | immutable customization value | src2/Vehicle/Minecart/Definitions/MinecartCustomizationValue.cs | preserve 50f default |
| 3314 | Minecart.Customization.WheelOffset；Minecart.cs:25 | immutable customization value | src2/Vehicle/Minecart/Definitions/MinecartCustomizationValue.cs | preserve Vector2(12f, 0f) default |
| 3315 | Minecart.Customization.MagnetOffset；Minecart.cs:27 | immutable customization value | src2/Vehicle/Minecart/Definitions/MinecartCustomizationValue.cs | preserve Vector2(25f, 26f) default |
| 3975 | Minecart.Customization.Default；Minecart.cs:29 | deterministic customization factory | src2/Vehicle/Minecart/Definitions/MinecartCustomizationValue.cs | preserve default value composition |

| 序号 | 源成员和 Version4 行 | proposed role | proposed target file | compatibility action |
|---:|---|---|---|---|
| 3321 | Minecart.Flag_OnTrack；Minecart.cs:47 | motion flag definition | src2/Vehicle/Minecart/Definitions/MinecartMotionFlagDefinition.cs | map bit/value exactly |
| 3322 | Minecart.Flag_BouncyBumper；Minecart.cs:49 | motion flag definition | src2/Vehicle/Minecart/Definitions/MinecartMotionFlagDefinition.cs | map bit/value exactly |
| 3323 | Minecart.Flag_UsedRamp；Minecart.cs:51 | motion flag definition | src2/Vehicle/Minecart/Definitions/MinecartMotionFlagDefinition.cs | map bit/value exactly |
| 3324 | Minecart.Flag_HitSwitch；Minecart.cs:53 | motion flag definition | src2/Vehicle/Minecart/Definitions/MinecartMotionFlagDefinition.cs | map bit/value exactly |
| 3325 | Minecart.Flag_BoostLeft；Minecart.cs:55 | motion flag definition | src2/Vehicle/Minecart/Definitions/MinecartMotionFlagDefinition.cs | map bit/value exactly |
| 3326 | Minecart.Flag_BoostRight；Minecart.cs:57 | motion flag definition | src2/Vehicle/Minecart/Definitions/MinecartMotionFlagDefinition.cs | map bit/value exactly |
| 3335 | Minecart.BoosterSpeed；Minecart.cs:75 | track definition value | src2/Vehicle/Minecart/Definitions/MinecartTrackDefinitionCatalog.cs | compare float value and units |
| 3336 | Minecart.Type_Normal；Minecart.cs:77 | track type definition | src2/Vehicle/Minecart/Definitions/MinecartTrackDefinitionCatalog.cs | map type exhaustively |
| 3337 | Minecart.Type_Pressure；Minecart.cs:79 | track type definition | src2/Vehicle/Minecart/Definitions/MinecartTrackDefinitionCatalog.cs | map type exhaustively |
| 3338 | Minecart.Type_Booster；Minecart.cs:81 | track type definition | src2/Vehicle/Minecart/Definitions/MinecartTrackDefinitionCatalog.cs | map type exhaustively |
| 3339 | Minecart._leftSideConnection；Minecart.cs:83 | track catalog table | src2/Vehicle/Minecart/Definitions/MinecartTrackDefinitionCatalog.cs | compare initialization and Tile index |
| 3340 | Minecart._rightSideConnection；Minecart.cs:85 | track catalog table | src2/Vehicle/Minecart/Definitions/MinecartTrackDefinitionCatalog.cs | compare initialization and Tile index |
| 3341 | Minecart._trackType；Minecart.cs:87 | track catalog table | src2/Vehicle/Minecart/Definitions/MinecartTrackDefinitionCatalog.cs | compare initialization and Tile index |
| 3342 | Minecart._boostLeft；Minecart.cs:89 | track catalog table | src2/Vehicle/Minecart/Definitions/MinecartTrackDefinitionCatalog.cs | compare initialization and Tile index |
| 3344 | Minecart._firstPressureFrame；Minecart.cs:93 | track frame definition | src2/Vehicle/Minecart/Definitions/MinecartTrackDefinitionCatalog.cs | verify bootstrap order |
| 3345 | Minecart._firstLeftBoostFrame；Minecart.cs:95 | track frame definition | src2/Vehicle/Minecart/Definitions/MinecartTrackDefinitionCatalog.cs | verify bootstrap order |
| 3346 | Minecart._firstRightBoostFrame；Minecart.cs:97 | track frame definition | src2/Vehicle/Minecart/Definitions/MinecartTrackDefinitionCatalog.cs | verify bootstrap order |

### C09 MinecartPresentationDefinitions

| 序号 | 源成员和 Version4 行 | proposed role | proposed target file | compatibility action |
|---:|---|---|---|---|
| 3316 | Minecart.TotalFrames；Minecart.cs:37 | presentation definition | src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | compare frame count |
| 3317 | Minecart.LeftDownDecoration；Minecart.cs:39 | presentation definition | src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | compare frame value |
| 3318 | Minecart.RightDownDecoration；Minecart.cs:41 | presentation definition | src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | compare frame value |
| 3319 | Minecart.BouncyBumperDecoration；Minecart.cs:43 | presentation definition | src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | compare frame value |
| 3320 | Minecart.RegularBumperDecoration；Minecart.cs:45 | presentation definition | src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | compare frame value |
| 3327 | Minecart.NoConnection；Minecart.cs:59 | track presentation definition | src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | preserve sentinel |
| 3328 | Minecart.TopConnection；Minecart.cs:61 | track presentation definition | src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | preserve sentinel |
| 3329 | Minecart.MiddleConnection；Minecart.cs:63 | track presentation definition | src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | preserve sentinel |
| 3330 | Minecart.BottomConnection；Minecart.cs:65 | track presentation definition | src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | preserve sentinel |
| 3331 | Minecart.BumperEnd；Minecart.cs:67 | endpoint definition | src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | preserve sentinel |
| 3332 | Minecart.BouncyEnd；Minecart.cs:69 | endpoint definition | src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | preserve sentinel |
| 3333 | Minecart.RampEnd；Minecart.cs:71 | endpoint definition | src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | preserve sentinel |
| 3334 | Minecart.OpenEnd；Minecart.cs:73 | endpoint definition | src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | preserve sentinel |
| 3343 | Minecart._texturePosition；Minecart.cs:91 | presentation catalog table | src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | renderer adapter reads snapshot |
| 3347 | Minecart._trackSwitchOptions；Minecart.cs:99 | switch query catalog table | src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | compare Tile extension behavior |
| 3348 | Minecart._tileHeight；Minecart.cs:101 | geometry/presentation catalog table | src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | compare Tile extension behavior |

### C10 ProjectileDefinitionCatalog

| 序号 | 源成员和 Version4 行 | proposed role | proposed target file | compatibility action |
|---:|---|---|---|---|
| 233 | Main.projFrames；Main.cs:634 | projectile definition catalog | src2/Projectile/Definitions/ProjectileDefinitionCatalog.cs | shadow-read by ProjectileID and compare |
| 234 | Main.projPet；Main.cs:636 | projectile definition catalog | src2/Projectile/Definitions/ProjectileDefinitionCatalog.cs | shadow-read by ProjectileID and compare MessageBuffer use |

### C11 TrackedProjectileReference

| 序号 | 源成员和 Version4 行 | proposed role | proposed target file | compatibility action |
|---:|---|---|---|---|
| 3711 | TrackedProjectileReference.ProjectileLocalIndex；TrackedProjectileReference.cs:7 | relation value local slot | src2/Projectile/Tracking/TrackedProjectileReferenceValue.cs | preserve sentinel and distinguish slot from identity |
| 3712 | TrackedProjectileReference.ProjectileOwnerIndex；TrackedProjectileReference.cs:9 | relation value owner index | src2/Projectile/Tracking/TrackedProjectileReferenceValue.cs | adapter validates owner EntityId mapping |
| 3713 | TrackedProjectileReference.ProjectileIdentity；TrackedProjectileReference.cs:11 | runtime/network relation identity | src2/Projectile/Tracking/TrackedProjectileReferenceValue.cs | do not map to persistent ID without decision |
| 3714 | TrackedProjectileReference.ProjectileType；TrackedProjectileReference.cs:13 | content type relation | src2/Projectile/Tracking/TrackedProjectileReferenceValue.cs | validate ProjectileID owner |
| 3715 | TrackedProjectileReference.IsTrackingSomething；TrackedProjectileReference.cs:15 | derived tracking query | src2/Projectile/Tracking/Queries/TrackedProjectileReferenceQuery.cs | compare -1 sentinel and clear semantics |

### C12 DarknessHazardState

| 序号 | 源成员和 Version4 行 | proposed role | proposed target file | compatibility action |
|---:|---|---|---|---|
| 2369 | DontStarveDarknessDamageDealer.DARKNESS_HIT_TIMER_MAX_BEFORE_HIT；DontStarveDarknessDamageDealer.cs:10 | hazard definition | src2/World/Environment/Definitions/DarknessHazardDefinition.cs | compare threshold |
| 2370 | DontStarveDarknessDamageDealer.darknessTimer；DontStarveDarknessDamageDealer.cs:12 | hazard state candidate | src2/World/Environment/DarknessHazardStateComponent.cs | first locate tick writer; no migration before evidence |
| 2371 | DontStarveDarknessDamageDealer.darknessHitTimer；DontStarveDarknessDamageDealer.cs:14 | hazard state candidate | src2/World/Environment/DarknessHazardStateComponent.cs | first locate tick writer; no migration before evidence |
| 2372 | DontStarveDarknessDamageDealer.saidMessage；DontStarveDarknessDamageDealer.cs:16 | message projection state | src2/World/Environment/Projections/DarknessMessageProjection.cs | route UI/log side effect through adapter |
| 2373 | DontStarveDarknessDamageDealer.lastFrameWasTooBright；DontStarveDarknessDamageDealer.cs:18 | brightness transition snapshot | src2/World/Environment/Projections/DarknessBrightnessTransitionSnapshot.cs | verify derived/transition owner |

### C13 SeatMetadata

| 序号 | 源成员和 Version4 行 | proposed role | proposed target file | compatibility action |
|---:|---|---|---|---|
| 2409 | ExtraSeatInfo.IsAToilet；ExtraSeatInfo.cs:5 | transient seat metadata | src2/Player/Interaction/SeatMetadataValue.cs | adapter at sitting boundary; no persistence by default |

### C14 MainRuntimeBoundary

| 序号 | 源成员和 Version4 行 | proposed role | proposed target file | compatibility action |
|---:|---|---|---|---|
| 229 | Main.checkForSpawns；Main.cs:626 | spawn cadence state | src2/WorldSession/Runtime/SpawnCadenceState.cs | shadow-read, then one SpawnCadenceSystem writer |
| 230 | Main.helpText；Main.cs:628 | UI projection state | src2/WorldSession/Runtime/Projections/HelpTextProjectionState.cs | UI adapter only; no simulation owner |
| 231 | Main.BartenderHelpTextIndex；Main.cs:630 | UI projection state | src2/WorldSession/Runtime/Projections/HelpTextProjectionState.cs | UI adapter only; no simulation owner |
| 232 | Main.autoGen；Main.cs:632 | startup configuration | src2/WorldSession/Runtime/Adapters/MainRuntimeBoundaryAdapter.cs | read during startup; no tick mutation |
| 235 | Main.demonTorch；Main.cs:638 | lighting definition adapter | src2/WorldSession/Lighting/Adapters/LightingDefinitionAdapter.cs | compare WorldItem lighting consumer before switch |

## 3. 实施顺序和剩余整合断点

以下是生产接入仍需遵守的顺序。C01-C14 的 src2-only 实现已保存并作为一个本地验证切片完成；下列 integration-review 事项仍未被本会话裁决：

1. C01/C02：确定 EntityBounds 和 LiquidContact 的单一 writer、快照和 geometry truth table；先补 position、Tile/liquid owner。
2. C03：定义 collision payload/adapter 和 resolution command；验证 payload 不持久化、不改变状态。
3. C04/C05：先实现纯 teleport candidate/portal geometry query，再实现 cooldown cache，最后才接入 TeleportCommitCommand。
4. C06/C07：实现 pylon snapshot/Projection 和 shimmer state/query；分别验证 network/presentation 不反写状态。
5. C08/C09：加载 Minecart definition catalogs，建立 TrackQuery，之后才允许 MotionSystem 读取 query result。
6. C10/C11：固定 ProjectileID、local/owner/identity/type 的域模型和协议 adapter；在 stub 决策前不得切换 reference owner。
7. C12/C13：补齐 darkness timer writer 和 seat lifecycle 后再迁移；消息/粒子/坐姿投影最后接入。
8. C14：将 Main 兼容 adapter 按 spawn/UI/startup/lighting 分别切换；禁止一次性替换 Main 全局字段。
9. 整合验证：补充成员覆盖/静态检查和真实边界适配验证；保留已通过的 serial-wrapper build 与 focused verifier 记录，并在任何生产接入前重新验证受影响项目。

## 4. 单一写入 owner 和双写策略

| 状态/数据 | proposed 唯一写入 owner | 读取者 | 迁移策略 |
|---|---|---|---|
| width/height/geometry | proposed BoundsCommitSystem；position owner integration-review | GeometryQuery、collision、teleport | legacy facade read-through；发现双写即停止切换 |
| liquid fields | proposed LiquidContactSystem | damage、movement、presentation | shadow-read then single-write; compare AnyWet |
| portal pair/cooldown | proposed PortalTraversalSystem/cache owner | portal query/commit | per-tick snapshot compare; no global array alias |
| pylon registry | proposed PylonRegistryAdapter reads TileEntity; Projection sends | teleport query/UI/network | snapshot diff; projection failure does not mutate registry |
| shimmer timers | proposed ShimmerUnstuckSystem | query and movement | shadow timer; commit only after owner/lifecycle evidence |
| Minecart tables | proposed CatalogBootstrap | TrackQuery/PresentationQuery | immutable load, no runtime double-write |
| Projectile catalog | proposed catalog adapter | projectile consumers/MessageBuffer boundary | shadow-read indexed by ProjectileID |
| Tracked reference | proposed ReferenceProtocolAdapter plus relation owner | tracking query | preserve -1 and decode failure Clear; owner unresolved |
| darkness timers | proposed DarknessHazardSystem after evidence | damage/message projection | do not double-write while tick writer missing |
| seat metadata | proposed sitting boundary | player interaction/presentation | transient adapter; no persistence default |
| Main fields | proposed separate adapters/systems | old Main callers | field-specific compatibility, no MainRuntimeComponent |

双写只允许用于只读 shadow comparison，并且有明确结束条件；不能让 legacy writer 和 proposed writer 同时提交位置、液体、cooldown、计时器或网络包。

## 5. 网络、快照、持久化和客户端投影

- EntityBounds、LiquidContact、Shimmer 和 darkness 是否网络同步，必须按 authoritative owner 与客户端需求逐字段决定；本分区只提出快照接口，不决定协议。
- Pylon registry 以 immutable snapshot/diff 输出网络消息；add/remove 与 player-join snapshot 由 proposed PylonRegistryProjection 处理，失败需可重试且不能修改 source registry。
- Portal traversal 的 candidate、cooldown 和 commit result 不应直接持久化；若跨 tick 必须保留，先定义 world/session lifecycle 和 version。
- Collision payload、ChaseResults、SeatMetadata 和 Minecart query result 默认不持久化；它们是 transient value object/query output。
- TrackedProjectileReference 的 local index、owner index、runtime identity、content type 和 network/persistence ID 分开编码；TryReading 失败必须恢复清空 sentinel，不得产生半有效引用。
- projFrames/projPet、Minecart tables、portal geometry 和 darkness threshold 是 definition catalog，可由启动快照重建，不作为实体存档字段。
- helpText、BartenderHelpTextIndex、saidMessage、lastFrameWasTooBright、demonTorch 属于 UI/lighting projection 或外部 adapter，不能让客户端投影反向写 authoritative state。

## 6. 回滚和失败条件

回滚按能力独立进行，避免恢复整个空间模块：

1. 若 geometry verifier 发现 anchor、整数截断或 Hitbox 不等价，停止 Bounds writer 切换，保留 legacy read adapter，撤回 proposed commit adapter。
2. 若 liquid verifier 发现 AnyWet、wetCount 或 Tile 扫描时序差异，停用 LiquidContactSystem 写入，仅保留 shadow comparison。
3. 若 portal/teleport 出现错误穿墙、重复传送、cooldown 泄漏或网络重复提交，回滚 PortalTraversalSystem/TeleportCommitCommand，恢复 legacy adapter 并丢弃 runtime cache。
4. 若 pylon diff 与 join snapshot 不一致，停止 Projection，不回滚 registry snapshot；修复 projection 后重放版本化 diff。
5. 若 Minecart table 初始化、switch/frame 或速度结果不一致，回滚 catalog consumer，保留 definition evidence，不修改 Tile。
6. 若 reference 协议无法区分 ID 域或 stub 结果不明，停止 ReferenceProtocolAdapter，继续使用 legacy value 并清除新建关系。
7. 若 darkness tick writer 无法确认、伤害重复或 message 状态错乱，禁止切换 DarknessHazardSystem；保留 evidence-gap。
8. 若 Main adapter 导致 UI、startup 或 lighting 回归，逐字段回滚对应 adapter，不恢复一个巨型 shared component。

## 7. focused verifier、静态检查和串行 build 记录

本轮已运行一个独立的 P05 focused verifier，覆盖 C01-C14 各一个场景；它不是 Version4 全量行为等价测试，也不是生产集成测试。以下是实际覆盖的场景：

- `entity geometry`：覆盖 Center、边界点、Hitbox mutation 和尺寸不变量。
- `liquid contact`：覆盖接触快照、AnyWet 和 lava 查询。
- `collision payload`：覆盖 payload 值保持和 Tile/Entity effect sink 提交。
- `teleport queries`：覆盖 candidate 筛选和 interception 计算。
- `portal traversal`：覆盖 candidate、位置提交和 cooldown。
- `pylon snapshots`：覆盖 registry diff 的 add/remove projection。
- `shimmer unstuck`：覆盖有限时长状态的启动、递减和过期。
- `minecart definitions`、`minecart presentation`：覆盖轨道/运动定义和表现查询。
- `projectile catalog`、`tracked projectile reference`：覆盖索引定义以及 reference 的序列化往返。
- `darkness hazard`、`seat metadata`、`main runtime boundary`：覆盖伤害阈值、座位投影和 Main 边界值。
- 静态检查：命名空间、capability-first 路径、one core public type per same-named file、无 Query 写入、无外部类型渗透。
- 仍未执行的验证：119 条成员的自动覆盖解析、完整 proposed-role 静态审计、Tile/Entity stale reference 失败矩阵、网络/持久化/client projection、完整边界适配和 Version4 行为等价。

串行 build 记录：先确认没有活动的 `dotnet.exe`/`csc.exe` 编译进程，再从仓库根目录经 `Build/Tools/Invoke-SerialDotnet.ps1` 构建受影响的 verifier project。实际命令为：

```powershell
pwsh -NoProfile -Command "& {
  `$dotnetArguments = @(
    'build',
    '.\src2\SpatialMotionPhysicsVerification\Terraria.SpatialMotionPhysicsVerification.csproj',
    '-m:1',
    '-nr:false',
    '--no-restore',
    '-p:UseSharedCompilation=false',
    '-p:MSBuildNodeReuse=false',
    '-p:BuildInParallel=false'
  )
  & '.\Build\Tools\Invoke-SerialDotnet.ps1' @dotnetArguments
  exit `$LASTEXITCODE
}"
```

结果：exit code `0`，warning `0`，error `0`；产物为 `Build/bin/Terraria.SpatialMotionPhysics/Debug/net10.0/Terraria.SpatialMotionPhysics.dll` 和 `Build/bin/Terraria.SpatialMotionPhysicsVerification/Debug/net10.0/Terraria.SpatialMotionPhysicsVerification.dll`。

随后使用 `--no-build --no-restore` 通过同一 serial wrapper 运行：

```powershell
pwsh -NoProfile -Command "& {
  `$dotnetArguments = @(
    'run',
    '--project',
    '.\src2\SpatialMotionPhysicsVerification\Terraria.SpatialMotionPhysicsVerification.csproj',
    '--no-build',
    '--no-restore',
    '-p:UseSharedCompilation=false',
    '-p:MSBuildNodeReuse=false',
    '-p:BuildInParallel=false'
  )
  & '.\Build\Tools\Invoke-SerialDotnet.ps1' @dotnetArguments
  exit `$LASTEXITCODE
}"
```

结果：exit code `0`；输出 14 个 `PASS` 行，并以 `P05 verifier passed: 14 scenarios.` 结束。本轮没有对两份 P05 第二轮 Markdown 执行 `git diff --check`。

## 8. 风险、证据缺口和 blocking-decision

### Evidence-gap

1. 第一轮 P05 outputReport 不存在，路径为 D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P05-spatial-motion-physics-public-decomposition.md。
2. 约束\公共拆分约束.md 和 docs\Version4与完整源码差异附录-2026-09-05.md 不存在于预期路径。
3. darknessTimer/darknessHitTimer 的实际 tick 写入者未找到；不能确定 world/player/entity 生命周期。
4. TrackedProjectileReference 的 FindMatchingProjectile、Equals、GetHashCode 为 stub；协议和关系迁移不能闭合。
5. Minecart Tile extension 行为部分为 stub；catalog invalidation 和 Tile owner 未闭合。
6. network、persistence、client projection、Tile/liquid adapter 和 scheduler order 尚未形成可审计实现。
7. current NLTX 有重复 Liquid、CollisionResult、PhysicsState、PylonRegistry 和 Projectile identity 候选；本计划不裁决最终 owner。

### Blocking-decision

integration-review 必须裁决 entity position/size writer、Tile/liquid owner、portal/teleport transaction、pylon network owner、Minecart track catalog owner、Projectile ID owner、darkness timer owner、座位生命周期和最终 system order。未决项不否定已保存的 src2 implementation，但仍阻止生产 wiring、跨分区合并和行为等价接受。

## 9. Integration Handoff

- 覆盖范围：C01-C14，源成员 119 条，逐条映射已列出。
- 目标状态：设计表中的路径、类型和接口仍为 proposed；实际 C# 源码已保存于 `D:\TRbackup\NLTX\src2\SpatialMotionPhysics`，focused verifier 已保存于 `D:\TRbackup\NLTX\src2\SpatialMotionPhysicsVerification`。
- 执行状态：src2-only checkpoint completed；`implementationStatus: completed`；`verificationStatus: verified (serial build and focused verifier)`。
- 需要整合会话接手：跨分区 owner、ID 域、Tile/Entity adapter、网络/存档 schema、调度顺序和上述 blocking decisions。
- 生产 `src`、Version4、第一轮报告、其他分区文档、ledger 和 lock 未被本轮修改；本轮仅修改两份 P05 第二轮文档及 `src2` 内列明的项目/源码/verifier 文件。
