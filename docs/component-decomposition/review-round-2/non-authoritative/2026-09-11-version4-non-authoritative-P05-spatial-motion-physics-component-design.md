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

# Version4 非权威 P05：空间移动与物理组件设计

本文件仍是第二轮非权威组件设计草案。组件边界、接口、System、Query、Command、Adapter、Projection 和最终 owner 仍为 proposed；对应的 P05 src2-only C# 实现已经保存并通过本分区 focused verifier，但这不证明生产 `src` 已迁移、当前 NLTX 已具备完整能力，或与 Version4 行为等价。

## 1. 范围、证据和边界

### 1.1 当前分区

输入分区包含 10 个叶子子系统、99 个字段和 20 个属性，共 119 条成员记录。来源报告 SHA-256 为 b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196。本轮只覆盖以下成员：

| 检查点 | 叶子库存 | 处理原则 |
|---|---|---|
| C01 | EntityBoundsAndFluidState 中 Entity 的 width、height 及几何属性 | 尺寸是候选实体状态；几何属性改为纯 Query，setter 改为显式 proposed Command |
| C02 | Entity 的湿润字段和 AnyWet | 液体接触是候选实体状态；AnyWet 是派生 Query |
| C03 | BallCollisionEvent、BallPassThroughEvent、PhysicsProperties | 只保留为短期只读 payload/value object，不注册为持久组件 |
| C04 | RandomTeleportationAttemptSettings、ChaseResults、MaxCoins | 前两者是 Query 输入/输出；MaxCoins 延后到 integration-review |
| C05 | PortalHelper 的 8 条字段 | 传送门几何、pair cache、cooldown 和提交事务分开 |
| C06 | TeleportPylonInfo 的 2 条字段 | 作为 registry snapshot 条目，不作为实体组件 |
| C07 | ShimmerUnstuckHelper 的 2 个字段和 1 个属性 | 是短生命周期实体状态和派生 Query |
| C08 | MinecartMotionAndTrackState 的 17 条字段 | 轨道表和标志定义属于 Catalog/Query，不复制为每个实体的巨型组件 |
| C09 | MinecartDecorationAndSwitchState 的 16 条字段 | 装饰、切换和纹理表属于表现/定义 Catalog |
| C10 | Main 的 projFrames、projPet | 候选 Projectile definition catalog |
| C11 | TrackedProjectileReference 的 5 个属性 | 关系/协议边界；区分槽位、owner、identity、type 和 tracking flag |
| C12 | DontStarveDarknessDamageDealer 的 5 条字段 | 计时状态与消息/亮度投影分开；tick 写入证据仍不完整 |
| C13 | ExtraSeatInfo.IsAToilet | 短期座位元数据，不成为通用物理组件 |
| C14 | Main 的 checkForSpawns、helpText、BartenderHelpTextIndex、autoGen、demonTorch | 启动、刷怪、UI 和光照交叉边界，暂不合成 MainRuntimeComponent |

排除范围：本文件不裁决其他 P 分区成员、不修改生产 `src`、测试体系或其他项目、不修改 ledger 或 lock，也不把第一轮 prompt 的 outputReport 当作本轮输出。本轮实现只写入 `D:\TRbackup\NLTX\src2`；实际文件清单和验证记录见对应执行文档。

### 1.2 已读取的 Version4 证据

| 证据 | 事实和用途 | 状态 |
|---|---|---|
| D:\TRbackup\Version4\Terraria\Entity.cs:22-180 | width、height、wet 系列、AnyWet、VisualPosition、中心/边/角/Size/Hitbox 及 setter 写回 position 或尺寸 | 声明 confirmed；position owner 和写入调度 partial |
| D:\TRbackup\Version4\Terraria.GameContent\PortalHelper.cs:10-215 | portal pair cache、玩家/NPC cooldown、边缘和斜坡几何、更新和穿越流程；穿越会检查碰撞并改变位置/速度和网络状态 | 声明和主要副作用 confirmed；最终事务 owner partial |
| D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonsSystem.cs:13-90 | 从 TileEntity.ByPosition 重建 pylon 列表，按差异广播 add/remove，玩家加入时发送快照 | registry/projection 事实 confirmed；最终网络 owner partial |
| D:\TRbackup\Version4\Terraria.GameContent\ShimmerUnstuckHelper.cs:5-64 | TimeLeftUnstuck、IndefiniteProtectionActive、ShouldUnstuck、Update、StartUnstuck、Clear | 状态和派生关系 confirmed；粒子 adapter partial |
| D:\TRbackup\Version4\Terraria.GameContent\DontStarveDarknessDamageDealer.cs:8-36 | 黑暗计时、命中计时、提示标志和上一帧亮度标志及清理路径 | 声明 confirmed；实际 tick 写入 missing/partial |
| D:\TRbackup\Version4\Terraria.GameContent\ExtraSeatInfo.cs:3-5、Terraria\PlayerSittingHelper.cs:94-133、Terraria\Player.cs:11131 | IsAToilet 在坐姿设置和玩家消费之间传递短期座位信息 | 生命周期 confirmed；最终座位 owner integration-review |
| D:\TRbackup\Version4\Terraria.DataStructures\TrackedProjectileReference.cs:5-103 | Set 从 Projectile 复制引用身份，Clear 用 -1 sentinel，Write/TryReading 处理协议字段 | 字段语义和协议形状 confirmed；FindMatchingProjectile、Equals/GetHashCode 存在 stub |
| D:\TRbackup\Version4\Terraria.Physics\BallCollisionEvent.cs、BallPassThroughEvent.cs、PhysicsProperties.cs、Terraria.GameContent.Golf\GolfHelper.cs | 碰撞/穿透 readonly payload 以及 Tile、Entity 外部引用 | 短期 payload confirmed；Tile/Entity owner crossSubsystemOwner: integration-review |
| D:\TRbackup\Version4\Terraria\Minecart.cs:7-1361、Terraria\Main.cs:3369、Terraria\Player.cs:13722、17493、17503 | Customization、轨道表、boost/frame/连接定义、TrackCollision/FrameTrack/GetOnTrack/HitTrackSwitch 读写边界 | 声明和初始化 confirmed；部分 Tile extension/stub、实体运动 owner partial |
| D:\TRbackup\Version4\Terraria\Utils.cs:38-82、288-465 | 随机传送 settings、ChaseResults 和调用者 Player/NPC/Projectile | Query 输入/输出 confirmed；specializedConditions 外部 Tile seam partial；MaxCoins 不属空间域 |
| D:\TRbackup\Version4\Terraria\Main.cs:626-638、Terraria\MessageBuffer.cs:582、Terraria\WorldItem.cs:1277 | Main runtime 字段、projPet 网络读取和 demonTorch 光照读取 | 读取边界 confirmed；最终模块 owner integration-review |

第一轮 P05 public-decomposition 报告缺失：期望路径 D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P05-spatial-motion-physics-public-decomposition.md 不存在。因此下面的设计以输入库存和当前已复核证据为基础，并显式保留 partial/missing。

当前 NLTX 状态仅作为设计输入，不作能力声明：现有树中存在多套 Liquid、CollisionResult、PhysicsState、PylonRegistry 和 Projectile identity 候选，尚未由本分区单方面裁决 owner。设计表中的边界和嵌套目标路径仍标注 proposed；本轮实际保存的 P05 src2 文件不等于生产迁移或最终 owner 裁决。

## 2. Proposed 边界总表

下表的路径、类型和接口都是 proposed。相同概念跨分区使用时，owner 只能由 integration-review 决定。

| 检查点 | proposed 状态/值对象 | proposed 行为边界 | proposed 目标路径/实现边界 | 所有权备注 |
|---|---|---|---|---|
| C01 | EntityBoundsComponent | EntityGeometryQuery、BoundsMutationCommand | src2/Entity/Spatial/EntityBoundsComponent.cs；src2/Entity/Spatial/Queries/EntityGeometryQuery.cs；src2/Entity/Spatial/Commands/BoundsMutationCommand.cs | position writer 和跨实体 geometry owner: integration-review |
| C02 | EntityLiquidContactComponent | EntityLiquidContactQuery、LiquidContactSystem | src2/Entity/Spatial/EntityLiquidContactComponent.cs；src2/Entity/Spatial/Queries/EntityLiquidContactQuery.cs；src2/Entity/Spatial/Systems/LiquidContactSystem.cs | Tile/liquid owner: integration-review |
| C03 | BallCollisionPayload、BallPassThroughPayload、PhysicsPropertiesValue | CollisionQuery、CollisionResolutionSystem | src2/Physics/Collision/BallCollisionPayload.cs；src2/Physics/Collision/BallPassThroughPayload.cs；src2/Physics/Collision/PhysicsPropertiesValue.cs | Tile、Entity、damage sink crossSubsystemOwner: integration-review |
| C04 | RandomTeleportationAttemptInput、InterceptionResult | RandomTeleportCandidateQuery、InterceptionQuery | src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs；src2/Spatial/Teleport/InterceptionResult.cs；src2/Spatial/Teleport/Queries/TeleportCandidateQuery.cs | random source、Tile predicate and target owner partial |
| C05 | PortalTraversalRuntimeCache、PortalCooldownState candidate | PortalPointUpdateSystem、PortalTraversalQuery、TeleportCommitCommand | src2/Spatial/Portal/PortalTraversalRuntimeCache.cs；src2/Spatial/Portal/Systems/PortalTraversalSystem.cs；src2/Spatial/Teleport/Commands/TeleportCommitCommand.cs | portal transaction and cooldown owner: integration-review |
| C06 | TeleportPylonSnapshotEntry、PylonRegistrySnapshot | PylonRegistryProjection、PylonRegistryAdapter | src2/World/Teleport/PylonRegistrySnapshot.cs；src2/World/Teleport/Adapters/PylonRegistryAdapter.cs；src2/World/Teleport/Projections/PylonRegistryProjection.cs | TileEntity and network owner: integration-review |
| C07 | ShimmerUnstuckStateComponent | ShimmerUnstuckSystem、ShimmerUnstuckQuery、ParticleProjection | src2/Entity/Spatial/ShimmerUnstuckStateComponent.cs；src2/Entity/Spatial/Systems/ShimmerUnstuckSystem.cs；src2/Entity/Spatial/Queries/ShimmerUnstuckQuery.cs | Player owner and particle side effect partial |
| C08 | MinecartTrackDefinitionCatalog、MinecartMotionFlagDefinition | MinecartTrackQuery、MinecartMotionSystem | src2/Vehicle/Minecart/Definitions/MinecartTrackDefinitionCatalog.cs；src2/Vehicle/Minecart/Queries/MinecartTrackQuery.cs；src2/Vehicle/Minecart/Systems/MinecartMotionSystem.cs | track table and Tile owner: integration-review |
| C09 | MinecartPresentationDefinitionCatalog | MinecartPresentationQuery、TrackSwitchQuery | src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs；src2/Vehicle/Minecart/Queries/MinecartPresentationQuery.cs | renderer and Tile owner: integration-review |
| C10 | ProjectileDefinitionCatalog | ProjectileDefinitionCatalogAdapter | src2/Projectile/Definitions/ProjectileDefinitionCatalog.cs；src2/Projectile/Definitions/ProjectileDefinitionCatalogAdapter.cs | ProjectileID owner crossSubsystemOwner: integration-review |
| C11 | TrackedProjectileReferenceValue candidate | TrackedProjectileReferenceQuery、ReferenceProtocolAdapter | src2/Projectile/Tracking/TrackedProjectileReferenceValue.cs；src2/Projectile/Tracking/Queries/TrackedProjectileReferenceQuery.cs；src2/Projectile/Tracking/Adapters/ReferenceProtocolAdapter.cs | relation owner and all ID domains integration-review |
| C12 | DarknessHazardStateComponent | DarknessHazardSystem、DarknessMessageProjection | src2/World/Environment/DarknessHazardStateComponent.cs；src2/World/Environment/Systems/DarknessHazardSystem.cs；src2/World/Environment/Projections/DarknessMessageProjection.cs | timer tick writer and damage owner partial |
| C13 | SeatMetadataValue | SeatMetadataQuery、SittingProjectionAdapter | src2/Player/Interaction/SeatMetadataValue.cs；src2/Player/Interaction/Queries/SeatMetadataQuery.cs；src2/Player/Interaction/Adapters/SittingProjectionAdapter.cs | Player sitting owner crossSubsystemOwner: integration-review |
| C14 | SpawnCadenceState, AutoGenerationStartupOptions candidate | SpawnCadenceSystem, HelpTextProjection, LightingDefinitionAdapter | src2/WorldSession/Runtime/SpawnCadenceState.cs；src2/WorldSession/Runtime/Systems/SpawnCadenceSystem.cs；src2/WorldSession/Runtime/Adapters/MainRuntimeBoundaryAdapter.cs | UI, startup and lighting owners integration-review |

设计表中的 capability-first 路径是边界提案；本轮为保持 P05 隔离，实际源码集中在 `D:\TRbackup\NLTX\src2\SpatialMotionPhysics`，验证器集中在 `D:\TRbackup\NLTX\src2\SpatialMotionPhysicsVerification`。因此设计表中的 proposed 路径不表示这些嵌套目录已经创建，也不改变最终 integration-review 的归属决策。

## 3. 逐成员 proposed 归属

证据标记约定：confirmed-declaration 表示输入表中的声明和 Version4 位置已复核；partial-owner 表示调用者或最终写入 owner 尚未闭合；derived 表示不保存为独立权威字段；transient 表示短期 payload；deferred 表示本分区不裁决。

### C01 EntityBoundsAndGeometry

proposed 组合：EntityBoundsComponent 只保存 width 和 height；EntityGeometryQuery 计算 VisualPosition、Center、四边四角、Size 和 Hitbox；所有 setter 语义转为 BoundsMutationCommand，由唯一 position/bounds writer 提交。position 本身不在本分区成员清单内。

| 序号 | 成员 | 类型 | Version4 来源 | proposed 归属 | 证据 |
|---:|---|---|---|---|---|
| 3067 | width | field int | D:\TRbackup\Version4\Terraria\Entity.cs:22 | EntityBoundsComponent.width；proposed src2/Entity/Spatial/EntityBoundsComponent.cs | confirmed-declaration; partial-owner |
| 3068 | height | field int | D:\TRbackup\Version4\Terraria\Entity.cs:24 | EntityBoundsComponent.height；proposed src2/Entity/Spatial/EntityBoundsComponent.cs | confirmed-declaration; partial-owner |
| 3945 | VisualPosition | property Vector2 | D:\TRbackup\Version4\Terraria\Entity.cs:48 | derived EntityGeometryQuery.VisualPosition；proposed src2/Entity/Spatial/Queries/EntityGeometryQuery.cs | confirmed-declaration; derived; partial-owner |
| 3946 | Center | property Vector2 | D:\TRbackup\Version4\Terraria\Entity.cs:50 | derived EntityGeometryQuery.Center；setter input to proposed BoundsMutationCommand | confirmed-declaration; derived; partial-owner |
| 3947 | Left | property Vector2 | D:\TRbackup\Version4\Terraria\Entity.cs:62 | derived EntityGeometryQuery.Left；setter input to proposed BoundsMutationCommand | confirmed-declaration; derived; partial-owner |
| 3948 | Right | property Vector2 | D:\TRbackup\Version4\Terraria\Entity.cs:74 | derived EntityGeometryQuery.Right；setter input to proposed BoundsMutationCommand | confirmed-declaration; derived; partial-owner |
| 3949 | Top | property Vector2 | D:\TRbackup\Version4\Terraria\Entity.cs:86 | derived EntityGeometryQuery.Top；setter input to proposed BoundsMutationCommand | confirmed-declaration; derived; partial-owner |
| 3950 | TopLeft | property Vector2 | D:\TRbackup\Version4\Terraria\Entity.cs:98 | derived EntityGeometryQuery.TopLeft；setter input to proposed BoundsMutationCommand | confirmed-declaration; derived; partial-owner |
| 3951 | TopRight | property Vector2 | D:\TRbackup\Version4\Terraria\Entity.cs:110 | derived EntityGeometryQuery.TopRight；setter input to proposed BoundsMutationCommand | confirmed-declaration; derived; partial-owner |
| 3952 | Bottom | property Vector2 | D:\TRbackup\Version4\Terraria\Entity.cs:122 | derived EntityGeometryQuery.Bottom；setter input to proposed BoundsMutationCommand | confirmed-declaration; derived; partial-owner |
| 3953 | BottomLeft | property Vector2 | D:\TRbackup\Version4\Terraria\Entity.cs:134 | derived EntityGeometryQuery.BottomLeft；setter input to proposed BoundsMutationCommand | confirmed-declaration; derived; partial-owner |
| 3954 | BottomRight | property Vector2 | D:\TRbackup\Version4\Terraria\Entity.cs:146 | derived EntityGeometryQuery.BottomRight；setter input to proposed BoundsMutationCommand | confirmed-declaration; derived; partial-owner |
| 3955 | Size | property Vector2 | D:\TRbackup\Version4\Terraria\Entity.cs:158 | derived EntityGeometryQuery.Size；setter input to proposed BoundsMutationCommand | confirmed-declaration; derived; partial-owner |
| 3956 | Hitbox | property Rectangle | D:\TRbackup\Version4\Terraria\Entity.cs:171 | derived EntityGeometryQuery.Hitbox；setter input to proposed BoundsMutationCommand | confirmed-declaration; derived; partial-owner |

不变量：Hitbox 与 Center/边角查询必须由同一 width/height/position 快照计算；Query 不写状态；任何 setter 替代方案都必须保留原始锚点换算和整数截断语义，并在 integration-review 确定 position owner 后才实施。

### C02 EntityLiquidContact

proposed EntityLiquidContactComponent 保存五种接触布尔值和 wetCount；AnyWet 只作为 Query 结果。液体扫描系统负责写入，伤害、移动和表现系统只能读取快照或提交命令，不能直接互写。

| 序号 | 成员 | 类型 | Version4 来源 | proposed 归属 | 证据 |
|---:|---|---|---|---|---|
| 3069 | wet | field bool | D:\TRbackup\Version4\Terraria\Entity.cs:26 | EntityLiquidContactComponent.wet；proposed src2/Entity/Spatial/EntityLiquidContactComponent.cs | confirmed-declaration; partial-owner |
| 3070 | shimmerWet | field bool | D:\TRbackup\Version4\Terraria\Entity.cs:28 | EntityLiquidContactComponent.shimmerWet；proposed src2/Entity/Spatial/EntityLiquidContactComponent.cs | confirmed-declaration; partial-owner |
| 3071 | honeyWet | field bool | D:\TRbackup\Version4\Terraria\Entity.cs:30 | EntityLiquidContactComponent.honeyWet；proposed src2/Entity/Spatial/EntityLiquidContactComponent.cs | confirmed-declaration; partial-owner |
| 3072 | wetCount | field byte | D:\TRbackup\Version4\Terraria\Entity.cs:32 | EntityLiquidContactComponent.wetCount；proposed src2/Entity/Spatial/EntityLiquidContactComponent.cs | confirmed-declaration; partial-owner |
| 3073 | lavaWet | field bool | D:\TRbackup\Version4\Terraria\Entity.cs:34 | EntityLiquidContactComponent.lavaWet；proposed src2/Entity/Spatial/EntityLiquidContactComponent.cs | confirmed-declaration; partial-owner |
| 3944 | AnyWet | property bool | D:\TRbackup\Version4\Terraria\Entity.cs:36 | derived EntityLiquidContactQuery.AnyWet；proposed src2/Entity/Spatial/Queries/EntityLiquidContactQuery.cs | confirmed-declaration; derived; partial-owner |

不变量：AnyWet 等价于 wet 或 lavaWet 或 honeyWet 或 shimmerWet；wetCount 的单位、清零时机和 Tile/liquid 扫描 owner 尚未由本分区闭合。不能缓存 AnyWet，除非明确失效条件和 verifier。

### C03 CollisionContactPayloads

BallCollisionEvent 和 BallPassThroughEvent 是只读、短生命周期的计算结果，不应成为持久 Component。PhysicsProperties 是输入值对象或定义快照。Tile 和 Entity 引用必须在 Adapter seam 内部转换，不能渗入通用组件。

| 序号 | 成员 | 类型 | Version4 来源 | proposed 归属 | 证据 |
|---:|---|---|---|---|---|
| 2880 | Normal | field readonly Vector2 | D:\TRbackup\Version4\Terraria.Physics\BallCollisionEvent.cs:7 | transient BallCollisionPayload.Normal；proposed src2/Physics/Collision/BallCollisionPayload.cs | confirmed-declaration; transient |
| 2881 | ImpactPoint | field readonly Vector2 | D:\TRbackup\Version4\Terraria.Physics\BallCollisionEvent.cs:9 | transient BallCollisionPayload.ImpactPoint；proposed src2/Physics/Collision/BallCollisionPayload.cs | confirmed-declaration; transient |
| 2882 | Tile | field readonly Terraria.Tile | D:\TRbackup\Version4\Terraria.Physics\BallCollisionEvent.cs:11 | transient payload TileHandle through proposed TileCollisionAdapter | confirmed-declaration; transient; crossSubsystemOwner: integration-review |
| 2883 | Entity | field readonly Terraria.Entity | D:\TRbackup\Version4\Terraria.Physics\BallCollisionEvent.cs:13 | transient payload EntityReference through proposed EntityReferenceAdapter | confirmed-declaration; transient; crossSubsystemOwner: integration-review |
| 2884 | TimeScale | field readonly float | D:\TRbackup\Version4\Terraria.Physics\BallCollisionEvent.cs:15 | transient BallCollisionPayload.TimeScale；proposed src2/Physics/Collision/BallCollisionPayload.cs | confirmed-declaration; transient |
| 2885 | Tile | field readonly Terraria.Tile | D:\TRbackup\Version4\Terraria.Physics\BallPassThroughEvent.cs:5 | transient BallPassThroughPayload.TileHandle through proposed TileCollisionAdapter | confirmed-declaration; transient; crossSubsystemOwner: integration-review |
| 2886 | Entity | field readonly Terraria.Entity | D:\TRbackup\Version4\Terraria.Physics\BallPassThroughEvent.cs:7 | transient BallPassThroughPayload.EntityReference through proposed EntityReferenceAdapter | confirmed-declaration; transient; crossSubsystemOwner: integration-review |
| 2887 | Type | field readonly BallPassThroughType | D:\TRbackup\Version4\Terraria.Physics\BallPassThroughEvent.cs:9 | transient BallPassThroughPayload.Type；proposed src2/Physics/Collision/BallPassThroughPayload.cs | confirmed-declaration; transient |
| 2888 | TimeScale | field readonly float | D:\TRbackup\Version4\Terraria.Physics\BallPassThroughEvent.cs:11 | transient BallPassThroughPayload.TimeScale；proposed src2/Physics/Collision/BallPassThroughPayload.cs | confirmed-declaration; transient |
| 2889 | Gravity | field readonly float | D:\TRbackup\Version4\Terraria.Physics\PhysicsProperties.cs:5 | PhysicsPropertiesValue.Gravity；proposed src2/Physics/Collision/PhysicsPropertiesValue.cs | confirmed-declaration; value-object; partial-owner |
| 2890 | Drag | field readonly float | D:\TRbackup\Version4\Terraria.Physics\PhysicsProperties.cs:7 | PhysicsPropertiesValue.Drag；proposed src2/Physics/Collision/PhysicsPropertiesValue.cs | confirmed-declaration; value-object; partial-owner |

行为边界：CollisionQuery 只计算 payload；CollisionResolutionSystem 读取 payload 后提交 DamageCommand、TileInteractionCommand 或 EntityContactCommand。所有提交及其失败/重试顺序由 integration-review 决定。

### C04 TeleportQueries

RandomTeleportationAttemptSettings 是 Query 输入，不是实体状态；ChaseResults 是纯计算输出；specializedConditions 需要显式 TileConditionAdapter。MaxCoins 与空间移动无语义内聚关系，不能放入 TeleportComponent。

| 序号 | 成员 | 类型 | Version4 来源 | proposed 归属 | 证据 |
|---:|---|---|---|---|---|
| 3627 | teleporteeSize | field Vector2 | D:\TRbackup\Version4\Terraria\Utils.cs:40 | RandomTeleportationAttemptInput.teleporteeSize；proposed src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | confirmed-declaration; query-input |
| 3628 | teleporteeVelocity | field Vector2 | D:\TRbackup\Version4\Terraria\Utils.cs:42 | RandomTeleportationAttemptInput.teleporteeVelocity；proposed src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | confirmed-declaration; query-input |
| 3629 | teleporteeGravityDirection | field float | D:\TRbackup\Version4\Terraria\Utils.cs:44 | RandomTeleportationAttemptInput.teleporteeGravityDirection；proposed src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | confirmed-declaration; query-input |
| 3630 | mostlySolidFloor | field bool | D:\TRbackup\Version4\Terraria\Utils.cs:46 | RandomTeleportationAttemptInput.mostlySolidFloor；proposed src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | confirmed-declaration; query-input |
| 3631 | avoidLava | field bool | D:\TRbackup\Version4\Terraria\Utils.cs:48 | RandomTeleportationAttemptInput.avoidLava；proposed src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | confirmed-declaration; query-input |
| 3632 | avoidAnyLiquid | field bool | D:\TRbackup\Version4\Terraria\Utils.cs:50 | RandomTeleportationAttemptInput.avoidAnyLiquid；proposed src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | confirmed-declaration; query-input |
| 3633 | avoidHurtTiles | field bool | D:\TRbackup\Version4\Terraria\Utils.cs:52 | RandomTeleportationAttemptInput.avoidHurtTiles；proposed src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | confirmed-declaration; query-input |
| 3634 | avoidWalls | field bool | D:\TRbackup\Version4\Terraria\Utils.cs:54 | RandomTeleportationAttemptInput.avoidWalls；proposed src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | confirmed-declaration; query-input |
| 3635 | attemptsBeforeGivingUp | field int | D:\TRbackup\Version4\Terraria\Utils.cs:56 | RandomTeleportationAttemptInput.attemptsBeforeGivingUp；proposed src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | confirmed-declaration; query-input |
| 3636 | maximumFallDistanceFromOrignalPoint | field int | D:\TRbackup\Version4\Terraria\Utils.cs:58 | RandomTeleportationAttemptInput.maximumFallDistanceFromOrignalPoint；proposed src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | confirmed-declaration; query-input |
| 3637 | strictRange | field bool | D:\TRbackup\Version4\Terraria\Utils.cs:60 | RandomTeleportationAttemptInput.strictRange；proposed src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | confirmed-declaration; query-input |
| 3638 | tilesToAvoid | field int[] | D:\TRbackup\Version4\Terraria\Utils.cs:62 | RandomTeleportationAttemptInput.tilesToAvoid；proposed src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | confirmed-declaration; query-input |
| 3639 | tilesToAvoidRange | field int | D:\TRbackup\Version4\Terraria\Utils.cs:64 | RandomTeleportationAttemptInput.tilesToAvoidRange；proposed src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | confirmed-declaration; query-input |
| 3640 | allowSolidTopFloor | field bool | D:\TRbackup\Version4\Terraria\Utils.cs:66 | RandomTeleportationAttemptInput.allowSolidTopFloor；proposed src2/Spatial/Teleport/RandomTeleportationAttemptInput.cs | confirmed-declaration; query-input |
| 3641 | specializedConditions | field Func<Tile,int,int,bool> | D:\TRbackup\Version4\Terraria\Utils.cs:68 | TileConditionAdapter seam；proposed src2/Spatial/Teleport/Adapters/TileConditionAdapter.cs | confirmed-declaration; query-input; external Tile partial |
| 3642 | InterceptionHappens | field bool | D:\TRbackup\Version4\Terraria\Utils.cs:73 | InterceptionResult.InterceptionHappens；proposed src2/Spatial/Teleport/InterceptionResult.cs | confirmed-declaration; query-output |
| 3643 | InterceptionPosition | field Vector2 | D:\TRbackup\Version4\Terraria\Utils.cs:75 | InterceptionResult.InterceptionPosition；proposed src2/Spatial/Teleport/InterceptionResult.cs | confirmed-declaration; query-output |
| 3644 | InterceptionTime | field float | D:\TRbackup\Version4\Terraria\Utils.cs:77 | InterceptionResult.InterceptionTime；proposed src2/Spatial/Teleport/InterceptionResult.cs | confirmed-declaration; query-output |
| 3645 | ChaserVelocity | field Vector2 | D:\TRbackup\Version4\Terraria\Utils.cs:79 | InterceptionResult.ChaserVelocity；proposed src2/Spatial/Teleport/InterceptionResult.cs | confirmed-declaration; query-output |
| 3646 | MaxCoins | field const long | D:\TRbackup\Version4\Terraria\Utils.cs:82 | deferred economy/integration-review constant; no spatial component | confirmed-declaration; deferred |

不变量：Query 不随机写世界、不移动实体、不发包；若保留随机尝试次数，random source 必须是显式输入。追逐结果只描述几何结果，执行由另一个 Command/System 完成。

### C05 PortalTraversal

PortalHelper 的静态数组不能原样成为全局可变组件。proposed PortalTraversalRuntimeCache 按 world/tick 生命周期保存 pair/cooldown 临时数据；PortalGeometryDefinition 只读保存边缘和斜坡几何；TeleportCommitCommand 是唯一候选位置/速度提交边界。

| 序号 | 成员 | 类型 | Version4 来源 | proposed 归属 | 证据 |
|---:|---|---|---|---|---|
| 2457 | PORTALS_PER_PERSON | field const int | D:\TRbackup\Version4\Terraria.GameContent\PortalHelper.cs:10 | PortalGeometryDefinitionCatalog.PORTALS_PER_PERSON；proposed src2/Spatial/Portal/Definitions/PortalGeometryDefinitionCatalog.cs | confirmed-declaration; definition |
| 2458 | FoundPortals | field int[,] | D:\TRbackup\Version4\Terraria.GameContent\PortalHelper.cs:12 | PortalTraversalRuntimeCache.FoundPortals；proposed src2/Spatial/Portal/PortalTraversalRuntimeCache.cs | confirmed-declaration; cache; partial-owner |
| 2459 | PortalCooldownForPlayers | field int[] | D:\TRbackup\Version4\Terraria.GameContent\PortalHelper.cs:14 | PortalCooldownState candidate keyed by EntityId；proposed src2/Spatial/Portal/PortalCooldownState.cs | confirmed-declaration; cache; crossSubsystemOwner: integration-review |
| 2460 | PortalCooldownForNPCs | field int[] | D:\TRbackup\Version4\Terraria.GameContent\PortalHelper.cs:16 | PortalCooldownState candidate keyed by EntityId；proposed src2/Spatial/Portal/PortalCooldownState.cs | confirmed-declaration; cache; crossSubsystemOwner: integration-review |
| 2461 | EDGES | field readonly Vector2[] | D:\TRbackup\Version4\Terraria.GameContent\PortalHelper.cs:18 | PortalGeometryDefinition.Edges；proposed src2/Spatial/Portal/Definitions/PortalGeometryDefinition.cs | confirmed-declaration; definition |
| 2462 | SLOPE_EDGES | field readonly Vector2[] | D:\TRbackup\Version4\Terraria.GameContent\PortalHelper.cs:20 | PortalGeometryDefinition.SlopeEdges；proposed src2/Spatial/Portal/Definitions/PortalGeometryDefinition.cs | confirmed-declaration; definition |
| 2463 | SLOPE_OFFSETS | field readonly Point[] | D:\TRbackup\Version4\Terraria.GameContent\PortalHelper.cs:22 | PortalGeometryDefinition.SlopeOffsets；proposed src2/Spatial/Portal/Definitions/PortalGeometryDefinition.cs | confirmed-declaration; definition |
| 2464 | anyPortalAtAll | field bool | D:\TRbackup\Version4\Terraria.GameContent\PortalHelper.cs:24 | PortalAvailabilityQuery cache result；proposed src2/Spatial/Portal/Queries/PortalAvailabilityQuery.cs | confirmed-declaration; derived/cache; partial-owner |

边界：UpdatePortalPoints 只能更新 runtime cache；TryGoingThroughPortals 只能生成候选并提交 TeleportCommitCommand。碰撞验证、落点验证、Player/NPC teleport 事件和网络发送必须分为 adapter/projection，不能隐藏在 Query 中。

### C06 PylonRegistry

TeleportPylonInfo 是 registry snapshot entry；PositionInTiles 是 Tile 坐标，不是实体 Transform；TypeOfPylon 是内容分类，不是通用物理状态。TeleportPylonsSystem 的 TileEntity 扫描和网络 add/remove 发送必须由 proposed Adapter/Projection 隔离。

| 序号 | 成员 | 类型 | Version4 来源 | proposed 归属 | 证据 |
|---:|---|---|---|---|---|
| 2542 | PositionInTiles | field Point16 | D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonInfo.cs:8 | TeleportPylonSnapshotEntry.PositionInTiles；proposed src2/World/Teleport/TeleportPylonSnapshotEntry.cs | confirmed-declaration; snapshot |
| 2543 | TypeOfPylon | field TeleportPylonType | D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonInfo.cs:10 | TeleportPylonSnapshotEntry.TypeOfPylon；proposed src2/World/Teleport/TeleportPylonSnapshotEntry.cs | confirmed-declaration; snapshot |

不变量：snapshot 只能输出查询视图；TileEntity.ByPosition 是外部存储输入，不能反向成为核心 Component owner。列表差异、网络消息、玩家加入快照和失败重试均由 integration-review 确定。

### C07 ShimmerUnstuck

proposed ShimmerUnstuckStateComponent 仅附着于真正拥有脱困生命周期的实体；ShimmerUnstuckSystem 负责 tick、StartUnstuck 和 Clear；ShouldUnstuck 是纯 Query。粒子和音效只能通过 Projection/Adapter 产生。

| 序号 | 成员 | 类型 | Version4 来源 | proposed 归属 | 证据 |
|---:|---|---|---|---|---|
| 2504 | TimeLeftUnstuck | field int | D:\TRbackup\Version4\Terraria.GameContent\ShimmerUnstuckHelper.cs:7 | ShimmerUnstuckStateComponent.TimeLeftUnstuck；proposed src2/Entity/Spatial/ShimmerUnstuckStateComponent.cs | confirmed-declaration; state; clock owner partial |
| 2505 | IndefiniteProtectionActive | field bool | D:\TRbackup\Version4\Terraria.GameContent\ShimmerUnstuckHelper.cs:9 | ShimmerUnstuckStateComponent.IndefiniteProtectionActive；proposed src2/Entity/Spatial/ShimmerUnstuckStateComponent.cs | confirmed-declaration; state; owner partial |
| 3856 | ShouldUnstuck | property bool | D:\TRbackup\Version4\Terraria.GameContent\ShimmerUnstuckHelper.cs:11 | derived ShimmerUnstuckQuery.ShouldUnstuck；proposed src2/Entity/Spatial/Queries/ShimmerUnstuckQuery.cs | confirmed-declaration; derived |

不变量：IndefiniteProtectionActive 为 true 时 Query 结果恒为 true；否则 TimeLeftUnstuck 大于零才为 true。计时递减、粒子副作用和 Player/NPC 受益实体 owner 尚未闭合。

### C08 MinecartTrackDefinitions

本组所有成员都是 Minecart 的静态定义、标志编码、连接表或轨道帧初始化数据。它们不应成为每个矿车实体的字段集合。矿车实体的速度/位置状态若在其他分区出现，必须通过 Query 和 Command 交接。

Minecart customization is an immutable value object: it keeps texture width, wheel offset and magnet offset together, while `Default` is a deterministic definition factory. It is not entity motion state and does not own Tile or renderer effects.

| 序号 | 成员 | 类型 | Version4 来源 | proposed 归属 | 证据 |
|---:|---|---|---|---|---|
| 3313 | MinecartTextureWidth | field float | D:\TRbackup\Version4\Terraria\Minecart.cs:23 | MinecartCustomizationValue.MinecartTextureWidth；proposed src2/Vehicle/Minecart/Definitions/MinecartCustomizationValue.cs | confirmed-declaration; definition |
| 3314 | WheelOffset | field Vector2 | D:\TRbackup\Version4\Terraria\Minecart.cs:25 | MinecartCustomizationValue.WheelOffset；proposed src2/Vehicle/Minecart/Definitions/MinecartCustomizationValue.cs | confirmed-declaration; definition |
| 3315 | MagnetOffset | field Vector2 | D:\TRbackup\Version4\Terraria\Minecart.cs:27 | MinecartCustomizationValue.MagnetOffset；proposed src2/Vehicle/Minecart/Definitions/MinecartCustomizationValue.cs | confirmed-declaration; definition |
| 3975 | Default | property Customization | D:\TRbackup\Version4\Terraria\Minecart.cs:29 | MinecartCustomizationValue.Default；proposed src2/Vehicle/Minecart/Definitions/MinecartCustomizationValue.cs | confirmed-declaration; deterministic definition |
| 3321 | Flag_OnTrack | field const int | D:\TRbackup\Version4\Terraria\Minecart.cs:47 | MinecartMotionFlagDefinition.OnTrack；proposed src2/Vehicle/Minecart/Definitions/MinecartMotionFlagDefinition.cs | confirmed-declaration; definition |
| 3322 | Flag_BouncyBumper | field const int | D:\TRbackup\Version4\Terraria\Minecart.cs:49 | MinecartMotionFlagDefinition.BouncyBumper；proposed src2/Vehicle/Minecart/Definitions/MinecartMotionFlagDefinition.cs | confirmed-declaration; definition |
| 3323 | Flag_UsedRamp | field const int | D:\TRbackup\Version4\Terraria\Minecart.cs:51 | MinecartMotionFlagDefinition.UsedRamp；proposed src2/Vehicle/Minecart/Definitions/MinecartMotionFlagDefinition.cs | confirmed-declaration; definition |
| 3324 | Flag_HitSwitch | field const int | D:\TRbackup\Version4\Terraria\Minecart.cs:53 | MinecartMotionFlagDefinition.HitSwitch；proposed src2/Vehicle/Minecart/Definitions/MinecartMotionFlagDefinition.cs | confirmed-declaration; definition |
| 3325 | Flag_BoostLeft | field const int | D:\TRbackup\Version4\Terraria\Minecart.cs:55 | MinecartMotionFlagDefinition.BoostLeft；proposed src2/Vehicle/Minecart/Definitions/MinecartMotionFlagDefinition.cs | confirmed-declaration; definition |
| 3326 | Flag_BoostRight | field const int | D:\TRbackup\Version4\Terraria\Minecart.cs:57 | MinecartMotionFlagDefinition.BoostRight；proposed src2/Vehicle/Minecart/Definitions/MinecartMotionFlagDefinition.cs | confirmed-declaration; definition |
| 3335 | BoosterSpeed | field const float | D:\TRbackup\Version4\Terraria\Minecart.cs:75 | MinecartTrackDefinitionCatalog.BoosterSpeed；proposed src2/Vehicle/Minecart/Definitions/MinecartTrackDefinitionCatalog.cs | confirmed-declaration; definition |
| 3336 | Type_Normal | field const int | D:\TRbackup\Version4\Terraria\Minecart.cs:77 | MinecartTrackDefinitionCatalog.Normal；proposed src2/Vehicle/Minecart/Definitions/MinecartTrackDefinitionCatalog.cs | confirmed-declaration; definition |
| 3337 | Type_Pressure | field const int | D:\TRbackup\Version4\Terraria\Minecart.cs:79 | MinecartTrackDefinitionCatalog.Pressure；proposed src2/Vehicle/Minecart/Definitions/MinecartTrackDefinitionCatalog.cs | confirmed-declaration; definition |
| 3338 | Type_Booster | field const int | D:\TRbackup\Version4\Terraria\Minecart.cs:81 | MinecartTrackDefinitionCatalog.Booster；proposed src2/Vehicle/Minecart/Definitions/MinecartTrackDefinitionCatalog.cs | confirmed-declaration; definition |
| 3339 | _leftSideConnection | field int[] | D:\TRbackup\Version4\Terraria\Minecart.cs:83 | MinecartTrackDefinitionCatalog.LeftSideConnection；proposed src2/Vehicle/Minecart/Definitions/MinecartTrackDefinitionCatalog.cs | confirmed-declaration; catalog; partial Tile owner |
| 3340 | _rightSideConnection | field int[] | D:\TRbackup\Version4\Terraria\Minecart.cs:85 | MinecartTrackDefinitionCatalog.RightSideConnection；proposed src2/Vehicle/Minecart/Definitions/MinecartTrackDefinitionCatalog.cs | confirmed-declaration; catalog; partial Tile owner |
| 3341 | _trackType | field int[] | D:\TRbackup\Version4\Terraria\Minecart.cs:87 | MinecartTrackDefinitionCatalog.TrackType；proposed src2/Vehicle/Minecart/Definitions/MinecartTrackDefinitionCatalog.cs | confirmed-declaration; catalog; partial Tile owner |
| 3342 | _boostLeft | field bool[] | D:\TRbackup\Version4\Terraria\Minecart.cs:89 | MinecartTrackDefinitionCatalog.BoostLeft；proposed src2/Vehicle/Minecart/Definitions/MinecartTrackDefinitionCatalog.cs | confirmed-declaration; catalog; partial Tile owner |
| 3344 | _firstPressureFrame | field short | D:\TRbackup\Version4\Terraria\Minecart.cs:93 | MinecartTrackDefinitionCatalog.FirstPressureFrame；proposed src2/Vehicle/Minecart/Definitions/MinecartTrackDefinitionCatalog.cs | confirmed-declaration; catalog; initialization partial |
| 3345 | _firstLeftBoostFrame | field short | D:\TRbackup\Version4\Terraria\Minecart.cs:95 | MinecartTrackDefinitionCatalog.FirstLeftBoostFrame；proposed src2/Vehicle/Minecart/Definitions/MinecartTrackDefinitionCatalog.cs | confirmed-declaration; catalog; initialization partial |
| 3346 | _firstRightBoostFrame | field short | D:\TRbackup\Version4\Terraria\Minecart.cs:97 | MinecartTrackDefinitionCatalog.FirstRightBoostFrame；proposed src2/Vehicle/Minecart/Definitions/MinecartTrackDefinitionCatalog.cs | confirmed-declaration; catalog; initialization partial |

### C09 MinecartPresentationDefinitions

装饰帧、连接端点、纹理位置、切换选项和 tile 高度是只读定义/查询表。proposed MinecartPresentationDefinitionCatalog 不拥有矿车运动或 Tile 修改。

| 序号 | 成员 | 类型 | Version4 来源 | proposed 归属 | 证据 |
|---:|---|---|---|---|---|
| 3316 | TotalFrames | field const int | D:\TRbackup\Version4\Terraria\Minecart.cs:37 | MinecartPresentationDefinitionCatalog.TotalFrames；proposed src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | confirmed-declaration; definition |
| 3317 | LeftDownDecoration | field const int | D:\TRbackup\Version4\Terraria\Minecart.cs:39 | MinecartPresentationDefinitionCatalog.LeftDownDecoration；proposed src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | confirmed-declaration; definition |
| 3318 | RightDownDecoration | field const int | D:\TRbackup\Version4\Terraria\Minecart.cs:41 | MinecartPresentationDefinitionCatalog.RightDownDecoration；proposed src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | confirmed-declaration; definition |
| 3319 | BouncyBumperDecoration | field const int | D:\TRbackup\Version4\Terraria\Minecart.cs:43 | MinecartPresentationDefinitionCatalog.BouncyBumperDecoration；proposed src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | confirmed-declaration; definition |
| 3320 | RegularBumperDecoration | field const int | D:\TRbackup\Version4\Terraria\Minecart.cs:45 | MinecartPresentationDefinitionCatalog.RegularBumperDecoration；proposed src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | confirmed-declaration; definition |
| 3327 | NoConnection | field const int | D:\TRbackup\Version4\Terraria\Minecart.cs:59 | MinecartPresentationDefinitionCatalog.NoConnection；proposed src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | confirmed-declaration; definition |
| 3328 | TopConnection | field const int | D:\TRbackup\Version4\Terraria\Minecart.cs:61 | MinecartPresentationDefinitionCatalog.TopConnection；proposed src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | confirmed-declaration; definition |
| 3329 | MiddleConnection | field const int | D:\TRbackup\Version4\Terraria\Minecart.cs:63 | MinecartPresentationDefinitionCatalog.MiddleConnection；proposed src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | confirmed-declaration; definition |
| 3330 | BottomConnection | field const int | D:\TRbackup\Version4\Terraria\Minecart.cs:65 | MinecartPresentationDefinitionCatalog.BottomConnection；proposed src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | confirmed-declaration; definition |
| 3331 | BumperEnd | field const int | D:\TRbackup\Version4\Terraria\Minecart.cs:67 | MinecartPresentationDefinitionCatalog.BumperEnd；proposed src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | confirmed-declaration; definition |
| 3332 | BouncyEnd | field const int | D:\TRbackup\Version4\Terraria\Minecart.cs:69 | MinecartPresentationDefinitionCatalog.BouncyEnd；proposed src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | confirmed-declaration; definition |
| 3333 | RampEnd | field const int | D:\TRbackup\Version4\Terraria\Minecart.cs:71 | MinecartPresentationDefinitionCatalog.RampEnd；proposed src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | confirmed-declaration; definition |
| 3334 | OpenEnd | field const int | D:\TRbackup\Version4\Terraria\Minecart.cs:73 | MinecartPresentationDefinitionCatalog.OpenEnd；proposed src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | confirmed-declaration; definition |
| 3343 | _texturePosition | field Vector2[] | D:\TRbackup\Version4\Terraria\Minecart.cs:91 | MinecartPresentationDefinitionCatalog.TexturePosition；proposed src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | confirmed-declaration; catalog; renderer owner partial |
| 3347 | _trackSwitchOptions | field int[][] | D:\TRbackup\Version4\Terraria\Minecart.cs:99 | MinecartPresentationDefinitionCatalog.TrackSwitchOptions；proposed src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | confirmed-declaration; catalog; Tile owner partial |
| 3348 | _tileHeight | field int[][] | D:\TRbackup\Version4\Terraria\Minecart.cs:101 | MinecartPresentationDefinitionCatalog.TileHeight；proposed src2/Vehicle/Minecart/Definitions/MinecartPresentationDefinitionCatalog.cs | confirmed-declaration; catalog; Tile owner partial |

### C10 ProjectileDefinitionCatalog

projFrames 和 projPet 是按 ProjectileID 索引的内容定义/缓存，不是每个 Projectile entity 的运行时状态。Main 的其余字段留在 C14，避免把刷怪、UI、启动和光照合并到 projectile 组件。

| 序号 | 成员 | 类型 | Version4 来源 | proposed 归属 | 证据 |
|---:|---|---|---|---|---|
| 233 | projFrames | field int[] | D:\TRbackup\Version4\Terraria\Main.cs:634 | ProjectileDefinitionCatalog.Frames；proposed src2/Projectile/Definitions/ProjectileDefinitionCatalog.cs | confirmed-declaration; catalog; ProjectileID owner partial |
| 234 | projPet | field bool[] | D:\TRbackup\Version4\Terraria\Main.cs:636 | ProjectileDefinitionCatalog.IsPet；proposed src2/Projectile/Definitions/ProjectileDefinitionCatalog.cs | confirmed-declaration; catalog; MessageBuffer consumer confirmed |

### C11 TrackedProjectileReference

该类型不能把多个 ID 域混成一个整数。proposed value/adapter 必须分别记录 local slot、owner index、runtime identity、content type，并将 IsTrackingSomething 视为关系存在性 Query 或受协议约束的状态。FindMatchingProjectile、Equals 和 GetHashCode 的 Version4 stub 阻止行为闭合。

| 序号 | 成员 | 类型 | Version4 来源 | proposed 归属 | 证据 |
|---:|---|---|---|---|---|
| 3711 | ProjectileLocalIndex | property int | D:\TRbackup\Version4\Terraria.DataStructures\TrackedProjectileReference.cs:7 | TrackedProjectileReferenceValue.ProjectileLocalIndex；proposed src2/Projectile/Tracking/TrackedProjectileReferenceValue.cs | confirmed-declaration; relation; stub-dependent |
| 3712 | ProjectileOwnerIndex | property int | D:\TRbackup\Version4\Terraria.DataStructures\TrackedProjectileReference.cs:9 | TrackedProjectileReferenceValue.ProjectileOwnerIndex；proposed src2/Projectile/Tracking/TrackedProjectileReferenceValue.cs | confirmed-declaration; relation; crossSubsystemOwner: integration-review |
| 3713 | ProjectileIdentity | property int | D:\TRbackup\Version4\Terraria.DataStructures\TrackedProjectileReference.cs:11 | TrackedProjectileReferenceValue.ProjectileIdentity；proposed src2/Projectile/Tracking/TrackedProjectileReferenceValue.cs | confirmed-declaration; relation; network/persistence owner partial |
| 3714 | ProjectileType | property int | D:\TRbackup\Version4\Terraria.DataStructures\TrackedProjectileReference.cs:13 | TrackedProjectileReferenceValue.ProjectileType；proposed src2/Projectile/Tracking/TrackedProjectileReferenceValue.cs | confirmed-declaration; relation; ProjectileID owner partial |
| 3715 | IsTrackingSomething | property bool | D:\TRbackup\Version4\Terraria.DataStructures\TrackedProjectileReference.cs:15 | derived TrackedProjectileReferenceQuery.IsTrackingSomething；proposed src2/Projectile/Tracking/Queries/TrackedProjectileReferenceQuery.cs | confirmed-declaration; derived; sentinel semantics confirmed |

### C12 DarknessHazardState

proposed DarknessHazardStateComponent 只承载会影响下一次判定的计时状态；定义常量与 UI message/brightness transition 分开。darknessTimer 的实际 tick writer 尚未被找到，不能假定它是 entity、world 或 player owner。

| 序号 | 成员 | 类型 | Version4 来源 | proposed 归属 | 证据 |
|---:|---|---|---|---|---|
| 2369 | DARKNESS_HIT_TIMER_MAX_BEFORE_HIT | field const int | D:\TRbackup\Version4\Terraria.GameContent\DontStarveDarknessDamageDealer.cs:10 | DarknessHazardDefinition.HitTimerMaxBeforeHit；proposed src2/World/Environment/Definitions/DarknessHazardDefinition.cs | confirmed-declaration; definition |
| 2370 | darknessTimer | field static int | D:\TRbackup\Version4\Terraria.GameContent\DontStarveDarknessDamageDealer.cs:12 | DarknessHazardStateComponent.DarknessTimer candidate；proposed src2/World/Environment/DarknessHazardStateComponent.cs | confirmed-declaration; partial; tick writer missing |
| 2371 | darknessHitTimer | field static int | D:\TRbackup\Version4\Terraria.GameContent\DontStarveDarknessDamageDealer.cs:14 | DarknessHazardStateComponent.DarknessHitTimer candidate；proposed src2/World/Environment/DarknessHazardStateComponent.cs | confirmed-declaration; partial; tick writer missing |
| 2372 | saidMessage | field static bool | D:\TRbackup\Version4\Terraria.GameContent\DontStarveDarknessDamageDealer.cs:16 | DarknessMessageProjection state；proposed src2/World/Environment/Projections/DarknessMessageProjection.cs | confirmed-declaration; presentation; owner partial |
| 2373 | lastFrameWasTooBright | field static bool | D:\TRbackup\Version4\Terraria.GameContent\DontStarveDarknessDamageDealer.cs:18 | DarknessBrightnessTransitionSnapshot；proposed src2/World/Environment/Projections/DarknessBrightnessTransitionSnapshot.cs | confirmed-declaration; derived/presentation; owner partial |

### C13 SeatMetadata

ExtraSeatInfo.IsAToilet 是 PlayerSittingHelper 创建/设置、Player 消费的短期元数据。它不表达碰撞、重力或实体尺寸，不能塞进 EntityBoundsComponent 或通用 PhysicsState。

| 序号 | 成员 | 类型 | Version4 来源 | proposed 归属 | 证据 |
|---:|---|---|---|---|---|
| 2409 | IsAToilet | field bool | D:\TRbackup\Version4\Terraria.GameContent\ExtraSeatInfo.cs:5 | SeatMetadataValue.IsAToilet；proposed src2/Player/Interaction/SeatMetadataValue.cs | confirmed-declaration; transient; Player owner integration-review |

### C14 MainRuntimeBoundary

这些 Main 字段不能合成一个 shared runtime component。每个字段需按变化原因和外部副作用分别处理；Main 只作为兼容 adapter 的候选入口。

| 序号 | 成员 | 类型 | Version4 来源 | proposed 归属 | 证据 |
|---:|---|---|---|---|---|
| 229 | checkForSpawns | field int | D:\TRbackup\Version4\Terraria\Main.cs:626 | SpawnCadenceState.CheckForSpawns；proposed src2/WorldSession/Runtime/SpawnCadenceState.cs | confirmed-declaration; state; spawn owner partial |
| 230 | helpText | field int | D:\TRbackup\Version4\Terraria\Main.cs:628 | HelpTextProjectionState.HelpText；proposed src2/WorldSession/Runtime/Projections/HelpTextProjectionState.cs | confirmed-declaration; UI boundary; integration-review |
| 231 | BartenderHelpTextIndex | field int | D:\TRbackup\Version4\Terraria\Main.cs:630 | HelpTextProjectionState.BartenderHelpTextIndex；proposed src2/WorldSession/Runtime/Projections/HelpTextProjectionState.cs | confirmed-declaration; UI boundary; integration-review |
| 232 | autoGen | field bool | D:\TRbackup\Version4\Terraria\Main.cs:632 | AutoGenerationStartupOptions.AutoGen；proposed src2/WorldSession/Runtime/Adapters/MainRuntimeBoundaryAdapter.cs | confirmed-declaration; startup config; integration-review |
| 235 | demonTorch | field float | D:\TRbackup\Version4\Terraria\Main.cs:638 | LightingDefinitionAdapter.DemonTorch；proposed src2/WorldSession/Lighting/Adapters/LightingDefinitionAdapter.cs | confirmed-declaration; lighting boundary; WorldItem consumer confirmed |

## 4. 接口契约和依赖方向

下列接口均为 proposed seam，不代表当前代码中已经存在。

| proposed seam | 输入 | 输出 | 允许的写入 | 禁止事项 | Depth/Leverage/Locality |
|---|---|---|---|---|---|
| EntityGeometryQuery | entity position snapshot、EntityBoundsComponent | immutable geometry view | 无 | 不改变 position、bounds 或 Tile | deep query / high leverage / entity-local |
| BoundsMutationCommand | EntityId、anchor、new size/position intent | commit result/event | 唯一 bounds/position writer | Query 直接 setter、隐式网络发送 | deep command / high leverage / cross-domain |
| EntityLiquidContactQuery | EntityLiquidContactComponent | AnyWet/liquid-kind view | 无 | 不扫描或修改 Tile | shallow query / medium leverage / entity-local |
| CollisionQuery | physics input、Tile/Entity adapter view | transient collision payload | 无 | 不伤害、不移动、不发包 | deep query / high leverage / physics-local |
| PortalTraversalSystem | portal definitions、geometry、cooldown、entity snapshot | TeleportCandidate or command | runtime cache and command enqueue | 直接改变外部 Entity position | deep system / high leverage / cross-domain |
| TeleportCommitCommand | validated target、velocity policy、EntityId | success/failure event | position/velocity/teleport transaction | Query 生成副作用 | deep command / high leverage / integration boundary |
| PylonRegistryProjection | pylon snapshot diff | network/UI projection | adapter-owned outgoing message | 不修改 TileEntity registry | shallow projection / medium leverage / world-boundary |
| MinecartTrackQuery | tile snapshot、track catalog | connection/type/frame result | 无 | 不修改 Tile 或 vehicle motion | deep query / high leverage / tile-boundary |
| MinecartMotionSystem | track query result、vehicle state | motion command/event | proposed vehicle motion owner only | 不读取 static arrays directly from unrelated code | deep system / high leverage / vehicle-boundary |
| ReferenceProtocolAdapter | reference value、network reader/writer | protocol snapshot or reference | serialization adapter only | 不把 network ID 当 entity ID | deep adapter / high leverage / protocol-boundary |
| DarknessHazardSystem | clock、brightness sample、DarknessHazardStateComponent | damage/message events | hazard timer state | 不让 projection own timer | deep system / high leverage / world-boundary |
| SeatMetadataQuery | sitting snapshot | immutable SeatMetadataValue | 无 | 不把 toilet flag变成 physics state | shallow query / low leverage / player-boundary |

依赖方向应为：Definition/Adapter 输入 -> Query -> System -> Command/Event -> Projection/Adapter。Component 不调用网络、文件、时钟或随机；需要时钟、随机、日志、Tile、UI、网络和持久化时，经 port 注入并在 verifier 中替换。

## 5. Entity、World、Tile 和 ID 边界

- EntityId 只标识 ECS 实体；不得以 ProjectileLocalIndex、ProjectileOwnerIndex 或数组槽位替代。
- ProjectileIdentity 是运行时投射物关系字段；ProjectileType 是内容定义 ID；是否持久化以及是否跨网络必须由 integration-review 决定。
- Network ID、持久化 ID、外部 TileEntity/Point16 坐标和 EntityId 分开建模；TeleportPylonSnapshotEntry 只携带 Tile 坐标和 pylon type 快照。
- Ball payload 中的 Tile、Entity 引用必须转换为稳定的 TileHandle/EntityReference proposed value object；引用失效、实体删除和跨世界读取必须有失败结果。
- position 不在本分区成员清单内。EntityGeometryQuery 可读 position snapshot，但 BoundsMutationCommand 的 writer 和网络/客户端 projection 归属 integration-review。
- Minecart track arrays 的索引语义依赖 Tile/frame 数据，不能在 Vehicle component 中复制一份可变表。

## 6. 生命周期、顺序和副作用

建议的 proposed 调度契约如下，最终顺序仍为 crossSubsystemOwner: integration-review：

1. 启动阶段加载 Projectile、Portal、Minecart 和 Darkness 的 definition catalog；只读表完成后才开放 Query。
2. 当前 tick 建立 EntityBounds、LiquidContact、Tile 和实体引用的只读快照。
3. 运行纯 Geometry、Collision、TeleportCandidate、PylonRegistry、MinecartTrack 和 TrackedProjectileReference Query。
4. LiquidContactSystem、MinecartMotionSystem、PortalTraversalSystem、ShimmerUnstuckSystem 和 DarknessHazardSystem 根据 Query 结果计算状态转换。
5. 统一提交 BoundsMutationCommand、TeleportCommitCommand、vehicle motion、damage 和 Tile interaction；每个领域只能有一个 writer。
6. 提交成功后生成 network snapshot、pylon diff、UI help/darkness message、lighting 和粒子 Projection。
7. Projection/Adapter 失败不回写核心 Component；按领域定义重试、丢弃或回滚策略。

必须测试的副作用顺序：传送碰撞检查先于位置写入；矿车轨道查询先于速度提交；液体接触先于液体伤害/表现；黑暗计时状态先于伤害事件；网络/客户端输出晚于权威状态提交。

## 7. 兼容、不拆分和风险

- 保留 legacy API 的 adapter 只做读/写转换；迁移期间不得让 legacy writer 和 proposed writer 无条件双写同一权威字段。
- 对 width/height、liquid contact 和 shimmer state 采用读兼容、单写 owner；发现双写时先记录冲突并停止切换。
- 对 PortalHelper、Minecart 和 Main 静态缓存采用 shadow-read 或 snapshot compare，不把旧数组直接暴露给新 System。
- 对 readonly payload、PylonInfo、SeatInfo 和 ChaseResults 采用 value-object adapter；不为短命对象建立持久化 schema。
- 对 TrackedProjectileReference 先固定 sentinel、序列化长度和失败清理语义，再处理 stub 的匹配/相等行为。
- 不拆分项：EntityGeometryQuery 不拆成每个边角一个组件；Minecart constants 不按字段机械建组件；Main 字段不合成 RuntimeComponent；Collision payload 不持久化；MaxCoins 不归入空间域。
- 主要风险是跨分区重复候选、Tile owner 未定、position writer 未定、网络/存档投影未闭合、调度顺序未定，以及 Version4 中的 stub。

## 8. Evidence-gap 与 blocking-decision

### Evidence-gap

1. 第一轮 P05 public-decomposition 报告缺失，无法复用其逐成员读者/写者结论。
2. D:\TRbackup\NLTX\约束\公共拆分约束.md 当前路径不存在；设计遵循已读取的 public-decomposition 和 AGENTS.md 规则，但公共约束仍需整合会话补齐。
3. D:\TRbackup\NLTX\docs\Version4与完整源码差异附录-2026-09-05.md 当前路径不存在。
4. DontStarveDarknessDamageDealer 的 darknessTimer/darknessHitTimer 实际 tick 写入路径未找到。
5. TrackedProjectileReference 的 FindMatchingProjectile、Equals 和 GetHashCode 在 Version4 中有 stub。
6. Minecart 部分 Tile extension 行为存在 stub；track table 初始化和失效条件仍需实现阶段复核。
7. 完整 network、persistence、client projection、clock/random/Tile adapter 和 system scheduler 尚未闭合。
8. 当前 NLTX 有多个重复 Liquid、CollisionResult、PhysicsState、PylonRegistry 和 Projectile identity 候选，本分区不能选最终 owner。

### Blocking-decision

integration-review 必须继续决定 entity position/size writer、Tile/liquid owner、portal/teleport transaction、pylon network owner、Minecart track table owner、Projectile ID owner、darkness timer owner 和最终 System 顺序。未决项不阻止本轮保存 src2 骨架和 focused verifier，但阻止生产 wiring、跨分区合并和行为等价接受。

## 9. Focused verifier 与剩余验证

本轮已运行 P05 focused verifier；它覆盖下列 14 个场景。成员覆盖、静态命名/目录规则、跨分区 owner、完整网络/持久化投影和 Version4 行为等价仍需独立整合验证：

- 逐成员静态覆盖 verifier：输入报告 119 条记录与设计/执行映射逐条相交，检查无遗漏、无跨分区复制和所有 proposed 标记。
- Entity geometry verifier：Center、四边四角、Size、Hitbox 的 getter/setter 换算、整数截断和单 writer 约束。
- Liquid verifier：五种接触字段与 AnyWet 真值表、wetCount 清零和 Tile snapshot 失效。
- Collision verifier：readonly payload 生命周期、Tile/Entity 引用失效、TimeScale、无隐式伤害/移动。
- Teleport/portal verifier：候选纯度、墙/液体/伤害 Tile 过滤、cooldown、portal collision、commit 失败不改变状态。
- Pylon verifier：TileEntity registry diff、add/remove 快照、玩家加入快照和网络 projection 不反写 registry。
- Minecart verifier：轨道连接/boost/frame catalog 初始化、switch query、motion command 单 writer。
- Reference verifier：-1 sentinel、owner/local/identity/type 区分、Write/TryReading 失败清理和 stub 行为决策。
- Darkness/seat/Main verifier：计时边界、message projection、SeatMetadata 短期生命周期、Main adapter 不形成大组件。

已执行并通过的命令记录：

1. `pwsh -NoProfile -Command "& { $dotnetArguments = @('build', '.\\src2\\SpatialMotionPhysicsVerification\\Terraria.SpatialMotionPhysicsVerification.csproj', '-m:1', '-nr:false', '--no-restore', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' @dotnetArguments; exit $LASTEXITCODE }"`；project 为 `src2/SpatialMotionPhysicsVerification/Terraria.SpatialMotionPhysicsVerification.csproj`；exit code `0`；warning `0`；error `0`；产物位于 `Build/bin/Terraria.SpatialMotionPhysics/Debug/net10.0/Terraria.SpatialMotionPhysics.dll` 和 `Build/bin/Terraria.SpatialMotionPhysicsVerification/Debug/net10.0/Terraria.SpatialMotionPhysicsVerification.dll`。
2. `pwsh -NoProfile -Command "& { $dotnetArguments = @('run', '--project', '.\\src2\\SpatialMotionPhysicsVerification\\Terraria.SpatialMotionPhysicsVerification.csproj', '--no-build', '--no-restore', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'); & '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' @dotnetArguments; exit $LASTEXITCODE }"`；exit code `0`；输出 `PASS` 14 次并以 `P05 verifier passed: 14 scenarios.` 结束。

以上记录证明 src2 项目可编译且 focused verifier 通过，不证明生产 `src` 修改、网络/持久化闭合或 Version4 全量行为等价。本轮未对两份第二轮 Markdown 执行 `git diff --check`。

## 10. Integration Handoff

- 本分区交付 14 个文档检查点的 proposed 组件/Query/System/Command/Adapter/Projection 边界，覆盖 119 条成员。
- 需要整合会话处理的共享候选：EntityGeometry、WorldPosition、TileHandle、EntityReference、TeleportCandidate、CollisionResult、PylonRegistry、MinecartTrackCatalog、Projectile identity/reference 和 scheduler order。
- 需要先补证的项目：第一轮报告、公共拆分约束、差异附录、darkness tick writer、TrackedProjectileReference stub、Minecart Tile extension、网络/存档/客户端 projection。
- 本轮已在 `D:\TRbackup\NLTX\src2\SpatialMotionPhysics` 保存 C# 实现，在 `D:\TRbackup\NLTX\src2\SpatialMotionPhysicsVerification` 保存 focused verifier；实现状态为 completed，验证状态为 focused verified。仍不可据此宣布生产迁移、行为等价或最终 owner 已确定。
- 生产 `src`、Version4、其他分区文档、ledger 和 lock 未被本轮 P05 实现修改；实际项目文件和源码清单见执行文档的 implementation record。
