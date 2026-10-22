# 68 个 L0/L1 组件定义清单

文档 ID：DOC-2026-10-09-COMPONENTS-ONLY-INVENTORY  
逻辑域：reviews  
产物类型：audit  
状态：verified-components-only  
日期：2026-10-09  
canonical 路径：`docs/reviews/audits/2026-10-09-components-only-inventory.md`

清单来自 [L0/L1 计划](../../plans/2026-10-08-arch-l1-basic-game-components-execution-plan.md)
第 3.1 节。逐一核对源码声明与 Compile 项：6 个 L0、62 个 L1，共 68 个组件。
活动项目为 [Terraria.Components.csproj](../../../src/NSSLC/Terraria.Components.csproj)。
范围、验证命令和恢复方式见 [清理记录](../../cr/CR-2026-10-09-components-only-cleanup.md)。

用户保护的 `src/NSSLC.Infrastructure/` 全部文件保留，不计入此次组件删除与系统清零范围。
当前库只编译下列组件以及字段必需的 38 个配套定义，没有行为方法或计算属性。

## 组件

| 等级 | 领域 | 类型与当前路径 | 执行前路径 |
| --- | --- | --- | --- |
| L0 | Player | [PlayerIdentityComponent](../../../src/NSSLC/Component/Player/PlayerIdentityComponent.cs) | 同当前路径 |
| L0 | Share | [ColliderComponent](../../../src/NSSLC/Component/Share/Entity/ColliderComponent.cs) | `src/NSSLC/Component/Share/Entity/Components/ColliderComponent.cs` |
| L0 | Share | [EntityIdentityComponent](../../../src/NSSLC/Component/Share/Entity/EntityIdentityComponent.cs) | `src/NSSLC/Component/Share/Entity/Components/EntityIdentityComponent.cs` |
| L0 | Share | [LocationComponent](../../../src/NSSLC/Component/Share/Entity/LocationComponent.cs) | `src/NSSLC/Component/Share/Entity/Components/LocationComponent.cs` |
| L0 | Share | [VelocityComponent](../../../src/NSSLC/Component/Share/Entity/VelocityComponent.cs) | `src/NSSLC/Component/Share/Entity/Components/VelocityComponent.cs` |
| L0 | WorldSession | [WorldLoadLifecycleComponent](../../../src/NSSLC/Component/WorldSession/WorldLoadLifecycleComponent.cs) | `src/NSSLC/Component/WorldSession/WorldGeneration/WorldLoadLifecycleComponent.cs` |
| L1 | Combat | [DamageAcceptancePolicyComponent](../../../src/NSSLC/Component/Combat/DamageAcceptancePolicyComponent.cs) | 同当前路径 |
| L1 | Combat | [DefenseComponent](../../../src/NSSLC/Component/Combat/DefenseComponent.cs) | 同当前路径 |
| L1 | Combat | [HealthComponent](../../../src/NSSLC/Component/Combat/HealthComponent.cs) | 同当前路径 |
| L1 | Combat | [HitCooldownComponent](../../../src/NSSLC/Component/Combat/HitCooldownComponent.cs) | 同当前路径 |
| L1 | Combat | [ImmunityComponent](../../../src/NSSLC/Component/Combat/ImmunityComponent.cs) | 同当前路径 |
| L1 | Items | [ItemDefinitionComponent](../../../src/NSSLC/Component/Items/ItemDefinitionComponent.cs) | 同当前路径 |
| L1 | Items | [ItemInstanceComponent](../../../src/NSSLC/Component/Items/Instances/ItemInstanceComponent.cs) | 同当前路径 |
| L1 | Items | [ItemInventoryRelationComponent](../../../src/NSSLC/Component/Items/Instances/ItemInventoryRelationComponent.cs) | 同当前路径 |
| L1 | Items | [ItemStackComponent](../../../src/NSSLC/Component/Items/Instances/ItemStackComponent.cs) | 同当前路径 |
| L1 | Items | [WeaponComponent](../../../src/NSSLC/Component/Items/WeaponComponent.cs) | 同当前路径 |
| L1 | Items | [WorldItemComponent](../../../src/NSSLC/Component/Items/WorldItemComponent.cs) | 同当前路径 |
| L1 | Items | [WorldItemReservationComponent](../../../src/NSSLC/Component/Items/WorldDrops/WorldItemReservationComponent.cs) | 同当前路径 |
| L1 | Items | [WorldItemStateComponent](../../../src/NSSLC/Component/Items/WorldDrops/WorldItemStateComponent.cs) | 同当前路径 |
| L1 | Npc | [NpcNetworkSyncIntentComponent](../../../src/NSSLC/Component/Npc/NpcNetworkSyncIntentComponent.cs) | `src/NSSLC/Component/Npc/Network/NpcNetworkSyncIntentComponent.cs` |
| L1 | Npc | [NpcAiStateComponent](../../../src/NSSLC/Component/Npc/NpcAiStateComponent.cs) | 同当前路径 |
| L1 | Npc | [NpcBehaviorComponent](../../../src/NSSLC/Component/Npc/NpcBehaviorComponent.cs) | 同当前路径 |
| L1 | Npc | [NpcBehaviorStateComponent](../../../src/NSSLC/Component/Npc/NpcBehaviorStateComponent.cs) | 同当前路径 |
| L1 | Npc | [NpcDefinitionReferenceComponent](../../../src/NSSLC/Component/Npc/NpcDefinitionReferenceComponent.cs) | 同当前路径 |
| L1 | Npc | [NpcDirectionComponent](../../../src/NSSLC/Component/Npc/NpcDirectionComponent.cs) | 同当前路径 |
| L1 | Npc | [NpcEntityIdentityComponent](../../../src/NSSLC/Component/Npc/NpcEntityIdentityComponent.cs) | 同当前路径 |
| L1 | Npc | [NpcHealthComponent](../../../src/NSSLC/Component/Npc/NpcHealthComponent.cs) | 同当前路径 |
| L1 | Npc | [NpcHitStateComponent](../../../src/NSSLC/Component/Npc/NpcHitStateComponent.cs) | 同当前路径 |
| L1 | Npc | [NpcImmediateEffectStateComponent](../../../src/NSSLC/Component/Npc/NpcImmediateEffectStateComponent.cs) | 同当前路径 |
| L1 | Npc | [NpcLegacySlotComponent](../../../src/NSSLC/Component/Npc/NpcLegacySlotComponent.cs) | 同当前路径 |
| L1 | Npc | [NpcLifecycleComponent](../../../src/NSSLC/Component/Npc/NpcLifecycleComponent.cs) | 同当前路径 |
| L1 | Npc | [NpcLocalBehaviorStateComponent](../../../src/NSSLC/Component/Npc/NpcLocalBehaviorStateComponent.cs) | 同当前路径 |
| L1 | Npc | [NpcMovementTickStateComponent](../../../src/NSSLC/Component/Npc/NpcMovementTickStateComponent.cs) | 同当前路径 |
| L1 | Npc | [NpcNaturalDespawnStateComponent](../../../src/NSSLC/Component/Npc/NpcNaturalDespawnStateComponent.cs) | 同当前路径 |
| L1 | Npc | [NpcTargetComponent](../../../src/NSSLC/Component/Npc/NpcTargetComponent.cs) | 同当前路径 |
| L1 | Npc | [NpcTargetSelectionStateComponent](../../../src/NSSLC/Component/Npc/NpcTargetSelectionStateComponent.cs) | 同当前路径 |
| L1 | Npc | [NpcTaskStateComponent](../../../src/NSSLC/Component/Npc/NpcTaskStateComponent.cs) | 同当前路径 |
| L1 | Player | [PlayerMovementPhysicsStateComponent](../../../src/NSSLC/Component/Player/PlayerMovementPhysicsStateComponent.cs) | `src/NSSLC/Component/Player/Movement/PlayerMovementPhysicsStateComponent.cs` |
| L1 | Player | [PlayerContactImmunityComponent](../../../src/NSSLC/Component/Player/PlayerContactImmunityComponent.cs) | 同当前路径 |
| L1 | Player | [PlayerInstantMovementAccumulatorComponent](../../../src/NSSLC/Component/Player/PlayerInstantMovementAccumulatorComponent.cs) | 同当前路径 |
| L1 | Player | [PlayerInventoryComponent](../../../src/NSSLC/Component/Player/PlayerInventoryComponent.cs) | 同当前路径 |
| L1 | Player | [PlayerInventorySlotsComponent](../../../src/NSSLC/Component/Player/PlayerInventorySlotsComponent.cs) | 同当前路径 |
| L1 | Player | [PlayerItemUseIntentComponent](../../../src/NSSLC/Component/Player/PlayerItemUseIntentComponent.cs) | 同当前路径 |
| L1 | Player | [PlayerLifecycleComponent](../../../src/NSSLC/Component/Player/PlayerLifecycleComponent.cs) | 同当前路径 |
| L1 | Player | [PlayerNetworkStateComponent](../../../src/NSSLC/Component/Player/PlayerNetworkStateComponent.cs) | 同当前路径 |
| L1 | Player | [PlayerRawControlInputComponent](../../../src/NSSLC/Component/Player/PlayerRawControlInputComponent.cs) | 同当前路径 |
| L1 | Player | [PlayerReleaseAndRepeatStateComponent](../../../src/NSSLC/Component/Player/PlayerReleaseAndRepeatStateComponent.cs) | 同当前路径 |
| L1 | Player | [PlayerSpawnPointComponent](../../../src/NSSLC/Component/Player/PlayerSpawnPointComponent.cs) | 同当前路径 |
| L1 | Player | [PlayerVitalStateComponent](../../../src/NSSLC/Component/Player/PlayerVitalStateComponent.cs) | 同当前路径 |
| L1 | Projectile | [ProjectileBehaviorStateComponent](../../../src/NSSLC/Component/Projectile/ProjectileBehaviorStateComponent.cs) | 同当前路径 |
| L1 | Projectile | [ProjectileCollisionPolicyComponent](../../../src/NSSLC/Component/Projectile/ProjectileCollisionPolicyComponent.cs) | 同当前路径 |
| L1 | Projectile | [ProjectileDamagePayloadComponent](../../../src/NSSLC/Component/Projectile/ProjectileDamagePayloadComponent.cs) | 同当前路径 |
| L1 | Projectile | [ProjectileDefinitionComponent](../../../src/NSSLC/Component/Projectile/ProjectileDefinitionComponent.cs) | 同当前路径 |
| L1 | Projectile | [ProjectileDispositionStateComponent](../../../src/NSSLC/Component/Projectile/ProjectileDispositionStateComponent.cs) | 同当前路径 |
| L1 | Projectile | [ProjectileGeometryStateComponent](../../../src/NSSLC/Component/Projectile/ProjectileGeometryStateComponent.cs) | 同当前路径 |
| L1 | Projectile | [ProjectileHitImmunityPolicyComponent](../../../src/NSSLC/Component/Projectile/ProjectileHitImmunityPolicyComponent.cs) | 同当前路径 |
| L1 | Projectile | [ProjectileHitImmunityStateComponent](../../../src/NSSLC/Component/Projectile/ProjectileHitImmunityStateComponent.cs) | 同当前路径 |
| L1 | Projectile | [ProjectileIdentityComponent](../../../src/NSSLC/Component/Projectile/ProjectileIdentityComponent.cs) | 同当前路径 |
| L1 | Projectile | [ProjectileKinematicsStateComponent](../../../src/NSSLC/Component/Projectile/ProjectileKinematicsStateComponent.cs) | 同当前路径 |
| L1 | Projectile | [ProjectileLifetimeStateComponent](../../../src/NSSLC/Component/Projectile/ProjectileLifetimeStateComponent.cs) | 同当前路径 |
| L1 | Projectile | [ProjectileNetworkStateComponent](../../../src/NSSLC/Component/Projectile/ProjectileNetworkStateComponent.cs) | 同当前路径 |
| L1 | Projectile | [ProjectilePenetrationStateComponent](../../../src/NSSLC/Component/Projectile/ProjectilePenetrationStateComponent.cs) | 同当前路径 |
| L1 | Projectile | [ProjectileSourceMetadataComponent](../../../src/NSSLC/Component/Projectile/ProjectileSourceMetadataComponent.cs) | 同当前路径 |
| L1 | Projectile | [ProjectileTrajectoryStateComponent](../../../src/NSSLC/Component/Projectile/ProjectileTrajectoryStateComponent.cs) | 同当前路径 |
| L1 | Projectile | [ProjectileUpdateCadenceComponent](../../../src/NSSLC/Component/Projectile/ProjectileUpdateCadenceComponent.cs) | 同当前路径 |
| L1 | Share | [DirectionComponent](../../../src/NSSLC/Component/Share/Entity/DirectionComponent.cs) | `src/NSSLC/Component/Share/Entity/Components/DirectionComponent.cs` |
| L1 | SpatialSimulation | [MovementGroundedStateComponent](../../../src/NSSLC/Component/SpatialSimulation/MovementGroundedStateComponent.cs) | 同当前路径 |
| L1 | SpatialSimulation | [MovementStateComponent](../../../src/NSSLC/Component/SpatialSimulation/MovementStateComponent.cs) | 同当前路径 |

## 字段配套定义

以下 38 个文件只声明枚举或值数据。它们不属于额外 ECS 组件；没有 System、
存储、注册、查询、命令、调度或适配实现。每个类型一个同名文件。

| 类型 | 领域路径 |
| --- | --- |
| [HitImmunityKey](../../../src/NSSLC/Component/Combat/HitImmunityKey.cs) | `Combat/HitImmunityKey.cs` |
| [HitImmunityScope](../../../src/NSSLC/Component/Combat/HitImmunityScope.cs) | `Combat/HitImmunityScope.cs` |
| [HitImmunityWindow](../../../src/NSSLC/Component/Combat/HitImmunityWindow.cs) | `Combat/HitImmunityWindow.cs` |
| [ExternalContentId](../../../src/NSSLC/Component/Items/ExternalContentId.cs) | `Items/ExternalContentId.cs` |
| [ItemInventorySlotKind](../../../src/NSSLC/Component/Items/Instances/ItemInventorySlotKind.cs) | `Items/Instances/ItemInventorySlotKind.cs` |
| [ItemDefinitionRef](../../../src/NSSLC/Component/Items/ItemDefinitionRef.cs) | `Items/ItemDefinitionRef.cs` |
| [PersistentItemId](../../../src/NSSLC/Component/Items/PersistentItemId.cs) | `Items/PersistentItemId.cs` |
| [LootSourceRef](../../../src/NSSLC/Component/Items/WorldDrops/LootSourceRef.cs) | `Items/WorldDrops/LootSourceRef.cs` |
| [ReplicationId](../../../src/NSSLC/Component/Items/WorldDrops/ReplicationId.cs) | `Items/WorldDrops/ReplicationId.cs` |
| [ReservationId](../../../src/NSSLC/Component/Items/WorldDrops/ReservationId.cs) | `Items/WorldDrops/ReservationId.cs` |
| [NpcInstanceId](../../../src/NSSLC/Component/Npc/NpcInstanceId.cs) | `Npc/NpcInstanceId.cs` |
| [NpcLifecycleStage](../../../src/NSSLC/Component/Npc/NpcLifecycleStage.cs) | `Npc/NpcLifecycleStage.cs` |
| [NpcNetId](../../../src/NSSLC/Component/Npc/NpcNetId.cs) | `Npc/NpcNetId.cs` |
| [NpcSlot](../../../src/NSSLC/Component/Npc/NpcSlot.cs) | `Npc/NpcSlot.cs` |
| [NpcTargetGeometrySnapshot](../../../src/NSSLC/Component/Npc/NpcTargetGeometrySnapshot.cs) | `Npc/NpcTargetGeometrySnapshot.cs` |
| [NpcTargetKind](../../../src/NSSLC/Component/Npc/NpcTargetKind.cs) | `Npc/NpcTargetKind.cs` |
| [NpcTaskEndReason](../../../src/NSSLC/Component/Npc/NpcTaskEndReason.cs) | `Npc/NpcTaskEndReason.cs` |
| [NpcTaskFailureReason](../../../src/NSSLC/Component/Npc/NpcTaskFailureReason.cs) | `Npc/NpcTaskFailureReason.cs` |
| [NpcTaskKind](../../../src/NSSLC/Component/Npc/NpcTaskKind.cs) | `Npc/NpcTaskKind.cs` |
| [NpcTaskPhase](../../../src/NSSLC/Component/Npc/NpcTaskPhase.cs) | `Npc/NpcTaskPhase.cs` |
| [NpcTypeId](../../../src/NSSLC/Component/Npc/NpcTypeId.cs) | `Npc/NpcTypeId.cs` |
| [LegacyPlayerSlot](../../../src/NSSLC/Component/Player/LegacyPlayerSlot.cs) | `Player/LegacyPlayerSlot.cs` |
| [PersistentPlayerId](../../../src/NSSLC/Component/Player/PersistentPlayerId.cs) | `Player/PersistentPlayerId.cs` |
| [PlayerConnectionState](../../../src/NSSLC/Component/Player/PlayerConnectionState.cs) | `Player/PlayerConnectionState.cs` |
| [PlayerContainerRef](../../../src/NSSLC/Component/Player/PlayerContainerRef.cs) | `Player/PlayerContainerRef.cs` |
| [PlayerDifficulty](../../../src/NSSLC/Component/Player/PlayerDifficulty.cs) | `Player/PlayerDifficulty.cs` |
| [PlayerLifecyclePhase](../../../src/NSSLC/Component/Player/PlayerLifecyclePhase.cs) | `Player/PlayerLifecyclePhase.cs` |
| [VoidVaultState](../../../src/NSSLC/Component/Player/VoidVaultState.cs) | `Player/VoidVaultState.cs` |
| [ProjectileDamageClass](../../../src/NSSLC/Component/Projectile/ProjectileDamageClass.cs) | `Projectile/ProjectileDamageClass.cs` |
| [ProjectileEndReason](../../../src/NSSLC/Component/Projectile/ProjectileEndReason.cs) | `Projectile/ProjectileEndReason.cs` |
| [ProjectileHostileDamageScaling](../../../src/NSSLC/Component/Projectile/ProjectileHostileDamageScaling.cs) | `Projectile/ProjectileHostileDamageScaling.cs` |
| [EntityReference](../../../src/NSSLC/Component/Relationships/EntityReference.cs) | `Relationships/EntityReference.cs` |
| [EntityReferenceScope](../../../src/NSSLC/Component/Relationships/EntityReferenceScope.cs) | `Relationships/EntityReferenceScope.cs` |
| [EntityUuid](../../../src/NSSLC/Component/Relationships/EntityUuid.cs) | `Relationships/EntityUuid.cs` |
| [ItemEntityRef](../../../src/NSSLC/Component/Relationships/ItemEntityRef.cs) | `Relationships/ItemEntityRef.cs` |
| [CollisionShapeKind](../../../src/NSSLC/Component/Share/Entity/CollisionShapeKind.cs) | `Share/Entity/CollisionShapeKind.cs` |
| [GravityDirection](../../../src/NSSLC/Component/SpatialSimulation/GravityDirection.cs) | `SpatialSimulation/GravityDirection.cs` |
| [WorldLoadRecoveryPhase](../../../src/NSSLC/Component/WorldSession/WorldLoadRecoveryPhase.cs) | `WorldSession/WorldLoadRecoveryPhase.cs` |

## 验证结果

- 68 个组件名称与计划第 3.1 节完全一致。
- 保护范围外的组件源码从 197 个收敛到 68 个；129 个范围外组件退出。
- 保护范围外的 129 个 `*System.cs` 全部退出。
- 编译输入精确为 106 个定义文件；项目引用 0、包引用 0。
- `net10.0` 组件库构建退出码 0，0 警告、0 错误。
- `src/NSSLC.Infrastructure/` 的 2,232 个文件路径和内容哈希在执行前后完全一致。
- 本次不验证旧玩法或基础设施项目的编译、运行兼容性。
