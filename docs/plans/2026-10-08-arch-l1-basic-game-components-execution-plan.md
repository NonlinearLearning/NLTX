# L0/L1 基本玩法组件迁移到 Arch 的执行计划

文档 ID：DOC-2026-10-08-ARCH-L1-BASIC-GAME-COMPONENTS-PLAN  
逻辑域：plans  
产物类型：plan  
状态：superseded-components-only；第 3.1 节的 68 个组件清单继续有效，其余运行迁移边界已由 2026-10-09 用户要求覆盖  
日期：2026-10-08  
范围：当前 `codex/l1-basic-runtime` 工作树中保留的 L0/L1 组件、简单游戏系统、世界会话、网络入口和自定义 ECS 退出路径  
证据入口：[简单游戏组件分级审核](../reviews/audits/2026-10-08-simple-game-component-grading.md)、[L1 基本玩法删减实施记录](../reviews/audits/2026-10-08-simple-game-l1-implementation.md)、[Arch 原生 API 覆盖审计](../reviews/audits/2026-10-08-arch-native-api-coverage-audit.md)、[Arch API 研究](../research/2026-10-07-arch-api-migration-research.md)、[全量自定义 ECS 转 Arch 计划](2026-10-07-custom-ecs-to-arch-execution-plan.md)  
canonical 路径：`docs/plans/2026-10-08-arch-l1-basic-game-components-execution-plan.md`

## 2026-10-09 当前范围覆盖

用户要求仅保留本文件第 3.1 节的 68 个组件定义，移除清单外组件、全部系统及其他运行
代码，并放弃所有兼容性；随后明确保护 `src/NSSLC.Infrastructure/` 整棵目录。
此次清理已完成：保护范围外恰好保留 68 个组件及 38 个字段配套定义，活动项目为
`src/NSSLC/Terraria.Components.csproj`。保护目录的 2,232 个文件内容未改变。

本文件后续章节记录的是覆盖前的 Arch 运行迁移设计。尤其第 3.2 节的保留要求、System
接线、宿主接入和旧行为验收不再是当前执行门禁。当前边界、破坏性变更、构建证据及恢复
方式以 [组件定义清理记录](../cr/CR-2026-10-09-components-only-cleanup.md) 为准；最终
源码路径见 [组件定义清单](../reviews/audits/2026-10-09-components-only-inventory.md)。

## 1. 这份计划解决的问题

旧的 [全量自定义 ECS 转 Arch 计划](2026-10-07-custom-ecs-to-arch-execution-plan.md)覆盖所有世界生成、复杂 NPC、城镇、液体、关系、物品和宿主路径。当前工作树已经完成一次面向简单游戏的组件删减，继续按旧计划迁移会把已经退出的能力重新列入生产闭包，也会把仍然包含兼容字段的混合组件误判为可以整块删除。

本计划把 Arch 迁移目标收敛到以下运行能力：

- 世界会话建立、加载完成、切换和失败清理；
- 实体身份、生命周期、位置、速度、碰撞和方向；
- 玩家连接、输入、移动、生命、重生、背包和物品使用；
- 少量 NPC 的目标、行为、移动、伤害、死亡和网络同步；
- 基础伤害、生命、命中冷却和免疫；
- 基础射弹的定义、运动、碰撞、伤害、穿透、生命周期和网络状态；
- 地面物品的生成、保留、拾取和背包归属；
- 地图存储、tick 调度和最小网络投影。

迁移完成的含义是：这些 L0/L1 生产消费者直接使用 Arch 原生 World、Entity、Query 和 System，且不再通过 `EntityRuntime`、`ComponentStore<T>`、`RuntimeEntityHandle` 或通用组件快照访问权威状态。它不代表高级玩法已经迁移，也不代表当前工作树中剩余的低等级混合组件可以立即删除。

## 2. 当前基线

本计划以当前工作树为输入，不以历史 DLL、旧分支或隔离 worktree 的提交代替源码证据。

| 指标 | 当前观察 |
| --- | ---: |
| 原始组件审核范围 | 487 个 `*Component.cs` |
| 执行前工作树剩余组件源码 | 208 个 |
| 执行前工作树剩余系统源码 | 136 个 |
| 当前执行后组件源码 | 197 个 |
| 当前执行后系统源码 | 129 个 |
| L0 基础运行目标组件 | 6 个 |
| L1 基本玩法目标组件 | 62 个 |
| 本计划的目标组件总数 | 68 个 |
| 剩余 L2 组件 | 48 个，仍有生产消费者 |
| 剩余 L3 组件 | 73 个，仍有生产消费者 |
| 剩余 L4 组件 | 19 个，仍有生产消费者 |

L2–L4 的 140 个剩余文件不是本计划的目标组件。它们中有部分是混合模型：当前仍被玩家网络、NPC 生命周期、世界文件恢复、物品存储或兼容投影读取。迁移 L0/L1 时只能切断它们在基本玩法路径上的生产写入，不能仅按审核等级删除整个类型。

当前 L0/L1 组件按领域分布如下：

| 领域 | L0 | L1 | 合计 |
| --- | ---: | ---: | ---: |
| `Combat` | 0 | 5 | 5 |
| `Items` | 0 | 8 | 8 |
| `Npc` | 0 | 18 | 18 |
| `Player` | 1 | 12 | 13 |
| `Projectile` | 0 | 16 | 16 |
| `Share` | 4 | 1 | 5 |
| `SpatialSimulation` | 0 | 2 | 2 |
| `WorldSession` | 1 | 0 | 1 |
| **合计** | **6** | **62** | **68** |

当前生产项目尚未在主工作树加入 `Arch` 或 `Arch.System` 的 `PackageReference`，生产代码中仍可见自定义 ECS 核心类型；这一点与 [Arch 原生 API 覆盖审计](../reviews/audits/2026-10-08-arch-native-api-coverage-audit.md)的观察一致。此前的 Arch API 研究、隔离探针和 T1–T5 交接文档只作为 API 和风险输入，不能被写成当前生产迁移已经完成。

## 3. 迁移边界

### 3.1 纳入 Arch 的组件

以下 68 个类型构成 L0/L1 目标集合。名称来自当前审核清单；实现阶段必须再次按源码声明和生产消费者核对，不能只按文件名推断。

**L0 基础运行：**

- `PlayerIdentityComponent`
- `ColliderComponent`
- `EntityIdentityComponent`
- `LocationComponent`
- `VelocityComponent`
- `WorldLoadLifecycleComponent`

**Combat：**

- `DamageAcceptancePolicyComponent`
- `DefenseComponent`
- `HealthComponent`
- `HitCooldownComponent`
- `ImmunityComponent`

**Items：**

- `ItemDefinitionComponent`
- `ItemInstanceComponent`
- `ItemInventoryRelationComponent`
- `ItemStackComponent`
- `WeaponComponent`
- `WorldItemComponent`
- `WorldItemReservationComponent`
- `WorldItemStateComponent`

**Npc：**

- `NpcNetworkSyncIntentComponent`
- `NpcAiStateComponent`
- `NpcBehaviorComponent`
- `NpcBehaviorStateComponent`
- `NpcDefinitionReferenceComponent`
- `NpcDirectionComponent`
- `NpcEntityIdentityComponent`
- `NpcHealthComponent`
- `NpcHitStateComponent`
- `NpcImmediateEffectStateComponent`
- `NpcLegacySlotComponent`
- `NpcLifecycleComponent`
- `NpcLocalBehaviorStateComponent`
- `NpcMovementTickStateComponent`
- `NpcNaturalDespawnStateComponent`
- `NpcTargetComponent`
- `NpcTargetSelectionStateComponent`
- `NpcTaskStateComponent`

**Player：**

- `PlayerMovementPhysicsStateComponent`
- `PlayerContactImmunityComponent`
- `PlayerInstantMovementAccumulatorComponent`
- `PlayerInventoryComponent`
- `PlayerInventorySlotsComponent`
- `PlayerItemUseIntentComponent`
- `PlayerLifecycleComponent`
- `PlayerNetworkStateComponent`
- `PlayerRawControlInputComponent`
- `PlayerReleaseAndRepeatStateComponent`
- `PlayerSpawnPointComponent`
- `PlayerVitalStateComponent`

**Projectile：**

- `ProjectileBehaviorStateComponent`
- `ProjectileCollisionPolicyComponent`
- `ProjectileDamagePayloadComponent`
- `ProjectileDefinitionComponent`
- `ProjectileDispositionStateComponent`
- `ProjectileGeometryStateComponent`
- `ProjectileHitImmunityPolicyComponent`
- `ProjectileHitImmunityStateComponent`
- `ProjectileIdentityComponent`
- `ProjectileKinematicsStateComponent`
- `ProjectileLifetimeStateComponent`
- `ProjectileNetworkStateComponent`
- `ProjectilePenetrationStateComponent`
- `ProjectileSourceMetadataComponent`
- `ProjectileTrajectoryStateComponent`
- `ProjectileUpdateCadenceComponent`

**Share 与 SpatialSimulation：**

- `DirectionComponent`
- `MovementGroundedStateComponent`
- `MovementStateComponent`

### 3.2 需要保留但不作为 L1 组件附加的状态

以下类型属于当前领域 owner、应用层或基础设施边界，不能因为不在 68 个组件清单中就直接删除：

| 类型或入口 | 处理方式 |
| --- | --- |
| `WorldDescriptorState`、`WorldRulesState`、`WorldTimeWeatherState`、`WorldEventProgressState` | 先作为世界会话的加载/快照输入；迁移世界单例时只保留 L1 tick、昼夜和必要规则字段，扩展事件字段不进入基本玩法调度。 |
| `TileMapStore`、地图布局、边界和固体查询 | 保留为地图存储 owner。地图不是把整张地图复制为 Arch Entity；实体只保存位置、速度和碰撞组件。 |
| `LoadedWorldSession`、`WorldStorageRoot` | 保留会话、存储和失败回滚职责，改为持有 Arch `World` 与 session token。 |
| `WorldSimulationKernel` | 保留命令、tick、快照和提交边界；只替换领域 phase 的存储和系统调用，不改变停止、取消和未提交 tick 语义。 |
| 网络 owner、DTO、协议槽位和玩家/NPC/Projectile 业务 ID | 保留协议身份和投影职责，改为解析完整的 `(session token, Arch.Entity)`。不能用 Arch `Id` 替代 UUID、槽位、网络 ID 或存档键。 |
| `PlayerInventoryComponent` 与 `PlayerInventorySlotsComponent` | 迁移前先确定唯一可写 owner；另一个只能是值投影或兼容读取，禁止在 Arch 外继续维护第二份可写库存。 |
| `MovementStateComponent` | 当前调用图把它作为运动算法输入/输出值组装。除非生产消费者证明需要跨 tick 保存，否则优先保留为短生命周期值，不为同一位置和速度再建立第二份权威状态。 |

### 3.3 明确排除

本计划不把以下内容加入 L1 Arch 组合、查询或默认调度：

- 天气扩展、入侵、Boss、Lunar、DD2、特殊种子和复杂世界生成；
- 城镇住房、救援、Pylon、门、传送门和传送过渡；
- 液体工作队列、电路传播、压力板和装饰 TileEntity；
- 装备效果、Buff、护盾、宠物、召唤物、钓鱼、拴系实体和死亡复仇；
- 专用射弹 AI、拖尾、反射、湿状态、动画例外和其他扩展射弹能力；
- 仅有 L2–L4 消费者的组件和系统。

这些能力后续若要重新启用，必须创建独立的分级、组件组合、System 调度和验收计划，不能通过在 L1 迁移中附加“暂时保留”的扩展组件恢复。

## 4. Arch 目标结构

### 4.1 世界、实体和身份

每一个已发布的 `LoadedWorldSession` 创建一个 Arch `World`。会话持有独立的 session token，身份 registry 保存：

```text
EntityUuid -> (session token, Arch.Core.Entity)
```

Arch `Entity.Id`、`WorldId` 和 `Version` 只负责当前进程内的运行时定位。它们不得写入：

- 玩家持久身份或 UUID；
- NPC、Projectile、WorldItem 的业务槽位；
- 网络协议实体 ID；
- TileEntity ID、存档键或持久化关系。

所有外部解析入口遵循同一顺序：

```text
当前 session token
  -> Entity.WorldId 与当前 Arch World 匹配
  -> World.IsAlive(entity)
  -> 领域 lifecycle 为 Published/Running
  -> 组件能力和协议权限
  -> 读取或修改组件
```

Arch 本身不替项目维护世界 epoch、UUID、关系索引、槽位生成代数或失败补偿。世界切换和候选加载失败时，旧 token 必须先拒绝新请求，再清理 registry、业务索引、订阅和缓冲，最后释放 Arch World。

### 4.2 世界单例与地图

世界单例实体至少附加 `WorldLoadLifecycleComponent`，并在同一 Arch World 内承载 L1 必要的时间、规则和就绪事实。加载输入、文件 DTO 和网络快照不能继续持有另一份可写世界状态。

地图保持 `TileMapStore` 的领域存储形式。玩家、NPC、Projectile 和 WorldItem 通过 `LocationComponent`、`VelocityComponent`、`ColliderComponent` 和地图查询完成空间交互；不把地图 tile 全量转换成 Arch Entity，也不把 Arch chunk 顺序当成地图扫描顺序。

### 4.3 组件存储形态

- 当前 `struct` 和 `record struct` 组件直接作为 Arch 值组件，通过短期 `ref` 更新。
- 当前可变 `class` 组件可以继续作为 Arch 组件，但每次创建实体必须创建独立实例；不得跨实体共享数组、集合或可变对象。
- `PlayerIdentityComponent`、NPC 行为对象、玩家库存和 WorldItem 状态等 class 组件不再被 `ComponentStore<T>` 包装。
- `PlayerRawControlInputComponent`、`PlayerItemUseIntentComponent`、`NpcImmediateEffectStateComponent`、`NpcNetworkSyncIntentComponent` 和 `ProjectileDispositionStateComponent` 作为 tick 内意图/结果数据使用，提交后由 owner 清空或推进代次。
- `NpcLegacySlotComponent`、`ProjectileIdentityComponent`、`ItemInventoryRelationComponent` 等领域投影保留业务语义，不替换成 Arch 数值 ID。

## 5. 最小实体组合

以下组合是迁移起点。组合只允许增加经过 L1 清单确认的组件；不得为了兼容旧路径把 L2–L4 组件偷偷附加到默认实体。

| 实体 | 最小组合 | 领域约束 |
| --- | --- | --- |
| Player | `EntityIdentityComponent`、`PlayerIdentityComponent`、`LocationComponent`、`VelocityComponent`、`ColliderComponent`、`DirectionComponent`、`PlayerLifecycleComponent`、输入/移动/生命/库存/网络组件，以及按需要附加的 Combat 组件 | 普通死亡/复活保留领域 UUID；真实销毁后重建必须产生新 UUID。 |
| NPC | `EntityIdentityComponent`、`NpcEntityIdentityComponent`、`NpcLifecycleComponent`、`NpcDefinitionReferenceComponent`、`LocationComponent`、`VelocityComponent`、`ColliderComponent`、方向/AI/行为/移动/目标/任务/生命/命中/网络组件 | 保留 NPC 业务槽位和目标关系清理；不得把 Arch 查询顺序当作旧槽位顺序。 |
| Projectile | `EntityIdentityComponent`、`ProjectileIdentityComponent`、定义/行为/运动/轨迹/几何/碰撞/伤害/生命周期/穿透/免疫/网络/来源/节拍组件 | Projectile identity index 仍是协议投影；销毁先撤销索引，再 Destroy Arch Entity。 |
| WorldItem | `EntityIdentityComponent`、`WorldItemComponent`、`WorldItemStateComponent`、`WorldItemReservationComponent`、Item definition/instance/stack/relation 组件 | 预留 token、数量和失败补偿属于物品业务协议，不能由 Arch Entity 代替。 |
| World singleton | `WorldLoadLifecycleComponent` 与 L1 必要的世界时钟/规则事实 | 世界单例不参与普通玩家/NPC/Projectile 查询；IsFresh 不能只用实体数量判断。 |

创建 owner 必须按下列顺序工作：容量/槽位预留 → `World.Create` → 组件组合初始化 → UUID/协议/关系登记 → 生命周期变为 Published → 进入普通查询。任一步失败都按相反顺序清理，不得留下未发布实体、半占用槽位或过期 registry 记录。

## 6. System 和调度迁移

### 6.1 调度原则

首轮采用 `Arch.System` 的原生 System/Group 能力；不再新增 `IEcsSystem`、`EcsScheduler`、自制 Group 或把 Arch 查询包在新的通用 facade 里。`Arch.Buffer` 只用于适合结构变化的提交点，不能承担玩家输入、命中、掉落和死亡效果的业务 FIFO。

现有 `WorldSimulationKernel` 的命令 drain、世界时钟、快照、runtime projection 和 tick commit 仍由应用层拥有。领域 System 在 owner thread 上按显式组执行，建议顺序为：

```text
输入与网络 ingress
  -> 玩家输入、移动和生命
  -> NPC 目标、AI、移动和战斗
  -> Projectile 准备、运动、碰撞和伤害
  -> Combat damage/death 与实体生命周期
  -> WorldItem 运动、预留、拾取和库存提交
  -> 网络/快照 egress
```

这只是 Arch Group 的组织顺序，不能改变当前 kernel 对命令、取消、世界切换、同 tick 可见性和未提交 tick 的既有合同。若当前入口依赖槽位扫描顺序，必须由领域调度显式保存该顺序，不能依赖 Arch archetype/chunk 遍历顺序。

### 6.2 L1 系统接线表

| 领域 | 首批接线的现有入口 | 迁移重点 | 暂不接线 |
| --- | --- | --- | --- |
| World | `WorldLoadLifecycleSystem`、`WorldSimulationClockSystem`、`TileMapRestoreSystem` | 世界单例、加载门禁、tick/昼夜、地图查询和失败清理 | 复杂世界生成、天气扩展、事件和液体队列 |
| Player | `PlayerRawControlInputSystem`、`PlayerMobilitySystem`、`PlayerTraversalPhysicsSystem`、`PlayerLifecycleSystem`、`PlayerNetworkStateSystem`、`PlayerInventoryCommitSystem`、`PlayerItemUseExecutionSystem` | 从实体组合读取/写入输入、位置、速度、生命、库存和网络状态；移除长期聚合包装 | 装备、Buff、宠物、坐骑、传送和表现投影系统 |
| Combat | `DamageResolutionSystem`、`DeathResolutionSystem`、`PlayerCombatResolutionSystem`、`NpcCombatSystem` | 统一 Damage/Defense/Health/Immunity/HitCooldown 的组件访问和死亡提交 | 复杂伤害效果、装备修正和状态效果扩展 |
| NPC | `NpcTargetSelectionSystem`、`NpcAiSystem`、`NpcMovementSystem`、`NpcCombatSystem`、`NpcDeathLifecycleSystem`、`NpcTaskLifecycleSystem`、基础 `NpcSpawnSystem` | 基本目标、行为、移动、生命、死亡、网络意图；生成只保留简单类型 | Invasion、Tower、SkyMob、Graveyard、Mother Slime、特殊生物群系和事件选择分支 |
| Projectile | `ProjectileDefinitionHydrationSystem`、`ProjectileMotionAndAiSystem`、`ProjectileCollisionPolicySystem`、`ProjectileDamageHitboxSystem`、`ProjectilePenetrationSystem`、`ProjectileLifetimeSystem`、`ProjectileLifecycleSystem`、`ProjectileNetworkStateSystem` | 基础射弹组合、短期 ref、碰撞/伤害/穿透/生命周期和 identity index | 专用 AI、拖尾、反射、湿状态、动画帧例外和召唤物路径 |
| Items | `WorldItemMotionSystem`、`WorldItemReservationSystem`、`PlayerInventoryPickupSystem`、`PlayerInventoryCommitSystem` | WorldItem 与 Item instance/stack/relation 的单一 owner；过期预留和失败恢复 | 商店、合成、装备、容器和钓鱼 |

### 6.3 System、Query、Command 和 Projection 的职责

这里的 Query、Command 和 Projection 是职责分类，不要求为每一行新建类型。现有入口同时包含规则判断和状态提交时，保留在拥有该不变量的 System 中；不能仅因方法名带有 `Query` 就把它当成纯函数。

| 职责 | L1 代表路径 | 读写合同 |
| --- | --- | --- |
| 只读 Query | 地图固体/碰撞查询、Projectile 几何和伤害候选查询、NPC 目标候选、库存容量/预留可用性检查 | 只读取当前 Arch World、地图或明确的值快照；不得推进计时、写缓存、发布网络或修改关系。若现有实现有缓存或计数副作用，保持 `partial`，由 owner 接管。 |
| 领域 Command | 玩家输入提交、Damage/Death resolution、NPC/Projectile/WorldItem spawn/release、库存预留/拾取/提交、World load/commit | 负责最终写入、顺序、拒绝原因和补偿；可以调用 Query，但不能让 Arch `CommandBuffer` 取代业务命令队列。 |
| System orchestration | Player/NPC/Projectile/Items 的 tick System 和 Arch Group | 规定查询、命令和结构变化的阶段边界；持有必要的短期游标，不保存跨世界的 Arch ref。 |
| Adapter/Projection | Network owner、WorldFile DTO、`TileMapStore`、runtime snapshot | 转换外部格式与领域值；不拥有第二份可写组件状态，恢复输入必须回到领域 owner 校验后提交。 |

System 拆分的证据顺序是：先确认实际入口和状态写入，再确认调度/生命周期和外部副作用，最后决定是否需要独立 Query、Command 或 Adapter。静态零命中、旧 facade 可调用或局部编译不能证明职责已经闭合。

## 7. 执行批次

所有批次先在当前迁移分支中执行。每批只改一个可回滚边界，完成后记录源码 hash、受影响项目、编译命令、退出码、警告/错误数和 `Build/bin/` 输出。

| 批次 | 内容 | 主要产物 | 前置 | 状态 |
| --- | --- | --- | --- | --- |
| L1-A0 | 工作树、组件、System、caller 和混合字段基线 | `Build/diagnostics/ArchMigration/L1-A0/` 基线快照 | 无 | 已执行（静态快照） |
| L1-A1 | 固定 Arch 2.1.0、Arch.System 1.1.0 和最小 API 探针 | 包版本、World/Entity/Query/System 编译记录 | A0 | 部分执行（包与核心 API 已编译） |
| L1-A2 | `LoadedWorldSession`、World owner、session token 和 UUID registry | 单一 Arch World/identity 合同 | A1 | 部分执行（会话复用唯一 World） |
| L1-A3 | Player/NPC/Projectile/WorldItem 创建与销毁 | 四类实体最小组合、发布/回滚路径 | A2 | 进行中（共享 Arch 存储切片） |
| L1-A4 | L1 领域 System 和 Arch Group | 输入、移动、战斗、射弹、物品和网络调用闭包 | A3 | 进行中（基础 Projectile 路径已裁剪） |
| L1-A5 | 网络、库存预留、槽位和世界存储投影 | 过期引用、失败补偿和单一可写 owner | A3–A4 | 进行中（WorldStorage 共享 runtime 边界） |
| L1-A6 | 应用宿主和 NetworkServer 接线 | kernel/Arch group/会话切换的编译闭包 | A4–A5 | 部分执行（受影响宿主已增量编译） |
| L1-A7 | L1 行为验收与静态退出审计 | verifier 结果、旧 ECS 引用清单、未覆盖项 | A6 | 未执行（按用户要求不做测试） |
| L1-A8 | 清理 L1 路径中的旧 ECS 类型 | 删除或隔离 `EntityRuntime` 等旧入口 | A7 且有明确批准 | 部分执行（已删除零生产引用的旧 ComponentStore；其余 facade 因仍有 caller 保留） |

### 7.1 L1-A0：重新建立组件和调用基线

1. 以当前工作树重新生成 L0/L1 组件清单，核对审核 CSV/JSON 与实际声明。
2. 按生产、测试、只读 `src/NSSLC.Infrastructure/分类参考/` 三类扫描 `EntityRuntime`、`ComponentStore`、`RuntimeEntityHandle`、`EntityComponentSnapshot`、`TryEdit`、`TryInspect` 和 `Match`。
3. 为每个 68 个组件记录唯一写入 owner、创建组合、查询消费者、网络投影、持久化投影和清理责任。
4. 为混合类型记录必须保留的 L1 字段和待拆分的 L2–L4 字段；不在此批次修改这些字段。
5. 生成 `Build/diagnostics/ArchMigration/L1-A0/` 输入快照；不修改源代码和存档。

完成条件：每个目标组件都有 owner 和处理方式；每个 L1 System 都能定位到当前 caller；不存在因文件名相似而把已删除能力列入目标集合。

### 7.2 L1-A1：固定 Arch API

固定版本以 [Arch API 研究](../research/2026-10-07-arch-api-migration-research.md) 为准：核心 `Arch 2.1.0`，系统组织使用经过编译核对的 `Arch.System 1.1.0`。核心命名空间为 `Arch.Core`，命令缓冲命名空间为 `Arch.Buffer`；不采用已从 Arch 2.x 移除的独立 `EntityReference`。

本批只验证 `World.Create/Destroy`、`Entity.Id/WorldId/Version`、`World.IsAlive`、`Get/Set/TryGet/TryGetRef`、`QueryDescription`、普通 Query、`Arch.System` 生命周期以及 `CommandBuffer.Playback` 的稳定签名。探针编译通过不证明生产行为，不能直接把隔离 probe 当作 L1 迁移完成。

### 7.3 L1-A2：世界与身份切换

1. `LoadedWorldSession` 创建并拥有唯一 Arch `World`；不得让 Player/NPC/Projectile store 自己 new 第二个 World。
2. `EntityIdentityRegistry` 的 live value 切换为完整 Arch Entity 加 session token；UUID issuer 保留领域身份职责。
3. `WorldStorageRoot` 接受 World/session/registry owner，不再以 `EntityRuntime` 作为权威存储参数。
4. 将世界单例和 `WorldLoadLifecycleComponent` 接入加载、发布、失败恢复、切换和 Dispose 门禁。
5. 所有外部入口按 token → WorldId → Version/alive → lifecycle 的顺序解析；旧会话请求必须具名拒绝。

完成条件：Application、WorldSession、WorldStorage 和 EntityEcs 的生产构造闭包可以增量编译；没有任何第二份可写 ECS 状态被引入。

### 7.4 L1-A3：实体纵向迁移

按 Player、NPC、Projectile、WorldItem 四条纵向路径逐条迁移，不把四类实体合并成一个没有 owner 的通用工厂。

- Player：保留玩家 UUID、连接状态、基础输入、移动、生命、库存和物品使用；普通复活保 UUID，真实销毁/重建换 UUID。
- NPC：保留简单类型、目标、AI、移动、生命、死亡和网络意图；旧槽位只作为容量/协议投影，不能替代 Arch Entity。
- Projectile：保留基础定义、位置/速度、碰撞、伤害、穿透、生命周期和网络 identity；结束时先清理 identity index 和源引用。
- WorldItem：保留物品实例、数量、背包关系、WorldItem 状态和预留 token；重复拾取、过期请求、数量变化和失败恢复由 Items owner 校验。

每条路径都必须有构造失败、发布失败、重复释放、容量耗尽、槽位复用、目标先销毁和 World dispose 的清理分支。`CommandBuffer` 不承担带 UUID 或跨实体关系的发布；需要延迟结构变化时先保存领域意图，再由 owner 校验后提交。

### 7.5 L1-A4：系统和查询迁移

1. 先迁移 L1 caller 的 `TryEdit`/`TryInspect`/`Match`，使用短期 Arch ref 或显式值快照。
2. 任何会 Add/Remove/Create/Destroy 的 System 在结构变化前结束其他组件 ref，提交后重新取得 ref。
3. 组件查询必须显式排除 Constructing、Terminating 和未发布实体；组件存在不能代替生命周期检查。
4. 依赖旧槽位顺序、NPC 扫描顺序或同 tick 可见性的路径由领域调度保存确定性，不使用 Archetype 遍历顺序。
5. 先采用手写原生 Query；只有重复样板形成真实热点时才引入 SourceGenerator，不在首批同时启用并行。

完成条件：L1 生产 System 可通过 Arch Group 调度访问唯一组件状态；L2–L4 系统不会因默认注册而重新进入基本玩法 tick。

### 7.6 L1-A5：网络、库存和存储边界

- 网络 owner 保存领域 UUID、协议 slot 和 session token，并在进入 System 前解析 Arch Entity；不把 Arch Entity 直接写入 DTO。
- `PlayerInventoryComponent`、`PlayerInventorySlotsComponent`、`ItemInventoryRelationComponent` 只允许一个可写 owner；提交使用数量/预留/revision 等物品业务 token。
- WorldItem 的 reservation、部分拾取、失败恢复和重建必须在同一 owner 中闭合；旧请求不能覆盖新实体。
- `TileMapStore` 和 WorldFile DTO 继续由基础设施 owner 管理；L1 迁移不把完整地图或高级 TileEntity 能力塞进 Arch World。
- `WorldStorageRoot` Dispose 时先停止新发布，再清理 L1 registry、槽位、关系、网络订阅和缓冲，最后释放 Arch World。

### 7.7 L1-A6：宿主汇合

将 `WorldSimulationKernel` 的 kernel-owned 阶段与 Arch System Group 汇合。NetworkServer 必须使用当前 live tick 和当前 session owner，不能继续使用固定 `() => 0L` 占位时间。宿主切换、取消、零 tick、加载失败和 World dispose 的调用闭包只做增量编译，行为验收由 A7 单独执行。

### 7.8 L1-A7/A8：验收和旧 ECS 退出

A7 先确认静态边界和编译闭包，再执行已存在的 L1 verifier：

- `NSSLC.Application.Simulation.Verification`；
- `Terraria.Player.InputControl.Verification`；
- `Terraria.Player.LifecycleInteraction.Verification`；
- `Terraria.NpcDamageCombatVerification`；
- `Terraria.ProjectileCombatVerification` 的 hydration、lifecycle、network 和 tick-coordinator 场景；
- `Terraria.Items.Verification`；
- `Terraria.SpatialSimulation.Verification`。

如果当前变更控制继续采用编译-only 门禁，则 A7 的运行项保持 `not-run`，只记录受影响项目的编译结果，不把编译通过写成行为通过。待运行门禁恢复后，必须重新用当前源码编译并执行，不能复用旧 DLL。

A8 只有在所有 L1 caller 已切换且 A7 的适用证据通过后才能删除 L1 路径中的 `EntityRuntime`、`ComponentStore<T>`、`IComponentStore`、`RuntimeEntityHandle`、通用 `EntityComponentSnapshot<T>` 和旧 phase 接口。仍被 L2–L4 兼容尾部使用的类型不得被 A8 误删；应先建立独立拆分批次。

本轮 A8 执行遵循该边界：`ComponentStore<T>` 与 `IComponentStore` 已确认没有生产调用者并删除；
`EntitySnapshotProjection<T>` 已迁移到独立文件继续服务 `EntityRuntime`。`EntityRuntime`、
`RuntimeEntityHandle`、`EntityComponentSnapshot<T>`、`ComponentAccess`、
`IWorldSimulationTickPhase` 及 `TryEdit`/`TryInspect`/`TryCapture`/`Match` 仍被生产或兼容尾部
调用，保留并列入后续 caller 切换清单。详细证据见
`Build/diagnostics/ArchMigration/L1-A8/README.md` 和
`docs/cr/CR-2026-10-08-arch-a8-old-ecs-cleanup.md`。

2026-10-09 的阶段调度切片已完成：`IWorldSimulationTickPhase` 不再存在于生产或测试源码，
现有阶段实现使用 `Arch.System.ISystem<WorldSimulationTickContext>`，由应用宿主保留显式
`WorldSimulationPhase` 顺序、会话失效检查和 tick 提交边界。剩余 `EntityRuntime`、
`RuntimeEntityHandle`、通用快照和访问委托仍按下述 caller 清单继续迁移。

随后对剩余自造 ECS 框架进行第二次静态清理，删除了无外部引用的提案型
`EntityLifecycleState`、`EntityProvenanceKind`、`NetworkEntityId`、`SpatialSpaceId`、
`WorldEventSourceKind` 和 `SpawnAdmission*` 类型。核心 facade 的生产 caller 仍存在，清单见
`Build/diagnostics/ArchMigration/L1-A8/custom-ecs-exit/README.md`；因此 A8 仍为 partial。

### 7.9 当前执行证据（2026-10-08）

本轮按编译-only 门禁执行，没有运行测试项目、`dotnet test`、`dotnet run`、verifier、simulation、host smoke、benchmark 或行为探针。已完成的源码切片包括：

- `EntityRuntime` 的权威组件存储改为 Arch `World`，并保留现有领域句柄、身份、生命周期和版本投影作为迁移适配层；
- `LoadedWorldSession` 创建并拥有唯一 Arch `World`，`EntityRuntime` 复用该 World，Dispose 由会话最终负责；
- `EntityIdentityRegistry` 增加 UUID 到 live Arch `Entity` 的绑定和解析入口；
- `WorldStorageRoot` 接收并校验会话 World/registry，同时保留旧构造器作为过渡入口；候选会话构造失败时先清理存储和实体投影，再销毁 Arch World；
- 固定 `Arch 2.1.0` 与 `Arch.System 1.1.0` 包引用；
- Projectile 基础路径删除已退出的动画、反射、拖尾、液体、专用 AI、召唤物、哨兵、陷阱和其他扩展能力，保留 L1 基础定义、运动、碰撞、伤害、穿透、生命周期、命中免疫和网络状态；
- NetworkServer 的 WorldItem owner 已改用单调递增的实时 60 Hz 主机 tick，移除固定 `() => 0L` 时间占位；
- 生成了 A0 静态基线：`Build/diagnostics/ArchMigration/L1-A0/`。
- 当前源码复核仍能定位全部 68 个 L0/L1 目标组件；执行 Projectile/Player 裁剪后工作树中为 197 个组件源码和 129 个系统源码。
- A3 静态组合扫描显示 Player 的 Dodge 兼容组件已移出默认组合，但死亡/休息、Ghost、配件、NPC 目标兼容状态仍由旧路径附加，因此 Player/NPC/WorldItem 的组合尚未达到 A3 接受条件；记录见 `Build/diagnostics/ArchMigration/L1-A3/default-composition-scan.txt`。

当前增量构建记录如下；输出均位于仓库的 `Build/bin/` 路径：

| 项目 | 命令 | 退出码 | warning/error | 输出 |
| --- | --- | ---: | ---: | --- |
| `Terraria.EntityEcs` | `dotnet restore ...; dotnet build src/NSSLC/Component/Share/Entity/Terraria.EntityEcs.csproj --no-restore --nologo -v:q` | 0 | 0 / 0 | `Build/bin/Terraria.EntityEcs/Debug/net10.0/` |
| `Terraria.WorldStorage` | `dotnet build src/NSSLC/Component/WorldStorage/Terraria.WorldStorage.csproj --no-restore --nologo -v:q` | 0 | 6 / 0 | `Build/bin/Terraria.WorldStorage/Debug/net10.0/` |
| `NSSLC.Application` | `dotnet restore ...; dotnet build src/NSSLC.Application/NSSLC.Application.csproj --no-restore --nologo -v:q` | 0 | 0 / 0 | `Build/bin/NSSLC.Application/Debug/net10.0/` |
| `Terraria.Player` | `dotnet build src/NSSLC/Component/Player/Terraria.Player.csproj --no-restore --nologo -v:q` | 0 | 0 / 0 | `Build/bin/Terraria.Player/Debug/net10.0/` |
| `Terraria.Npc` | `dotnet build src/NSSLC/Component/Npc/Terraria.Npc.csproj --no-restore --nologo -v:q` | 0 | 0 / 0 | `Build/bin/Terraria.Npc/Debug/net10.0/` |
| `Terraria.Projectile` | `dotnet build src/NSSLC/Component/Projectile/Terraria.Projectile.csproj --no-restore --nologo -v:q` | 0 | 0 / 0 | `Build/bin/Terraria.Projectile/Debug/net10.0/` |
| `Terraria.Items` | `dotnet build src/NSSLC/Component/Items/Terraria.Items.csproj --no-restore --nologo -v:q` | 0 | 0 / 0 | `Build/bin/Terraria.Items/Debug/net10.0/` |
| `NSSLC.Tools.Simulation` | `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:q` | 0 | 0 / 0 | `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/` |
| `NSSLC.Tools.NetworkServer` | `dotnet build src/NSSLC.Tools.NetworkServer/NSSLC.Tools.NetworkServer.csproj --no-restore --nologo -v:q` | 0 | 0 / 0 | `Build/bin/NSSLC.Tools.NetworkServer/Debug/net10.0/` |

这份记录证明源码切片的编译闭包，不证明运行时行为已经通过；A7 运行项仍为 `not-run`，
A8 已完成零引用旧 store 的 partial 清理，剩余 facade 的退出仍待 L1 caller 切换。

## 8. 编译门禁和证据记录

按仓库的[构建与验证约束](../../Context/项目约束/构建与验证约束.md)，每次只构建受影响项目，不运行 solution 级 Rebuild。以下命令均在 `D:\TRbackup\NLTX` 工作目录执行；它们只写入 `Build/bin/` 和 `Build/obj/`，不修改源码。实际项目集合由 A0 重新计算：

```powershell
dotnet build src/NSSLC/Component/Share/Entity/Terraria.EntityEcs.csproj --no-restore --nologo -v:q
dotnet build src/NSSLC/Component/WorldStorage/Terraria.WorldStorage.csproj --no-restore --nologo -v:q
dotnet build src/NSSLC/Component/Player/Terraria.Player.csproj --no-restore --nologo -v:q
dotnet build src/NSSLC/Component/Npc/Terraria.Npc.csproj --no-restore --nologo -v:q
dotnet build src/NSSLC/Component/Projectile/Terraria.Projectile.csproj --no-restore --nologo -v:q
dotnet build src/NSSLC/Component/Items/Terraria.Items.csproj --no-restore --nologo -v:q
dotnet build src/NSSLC.Application/NSSLC.Application.csproj --no-restore --nologo -v:q
dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:q
dotnet build src/NSSLC.Tools.NetworkServer/NSSLC.Tools.NetworkServer.csproj --no-restore --nologo -v:q
```

每次记录：

- 项目和完整命令；
- SDK、目标框架和是否使用 `--no-restore`；
- exit code；
- warning/error 数；
- `Build/bin/` 下的实际输出路径；
- 编译输入的源文件和项目文件 hash；
- 尚未编译的项目及原因。

静态门禁至少包括：

```text
L1 生产文件不得新增 EntityRuntime/ComponentStore/RuntimeEntityHandle/EntityComponentSnapshot 访问
L1 生产路径不得保存 Entity.Id 作为 UUID、协议 ID、槽位或存档键
默认 Player/NPC/Projectile/WorldItem 组合不得附加 L2–L4-only 组件
每个可变 class 组件必须在实体创建时独立实例化
每个 Create/Destroy/Release caller 必须有失败清理和重复释放处理
Arch World 的创建、切换和 Dispose 必须由 LoadedWorldSession 唯一拥有
```

## 9. 行为验收矩阵

下表是迁移后的目标，不是本文件写成时已经通过的结果：

| 行为 | 目标断言 | 证据入口 |
| --- | --- | --- |
| 实体引用 | 旧 Version、旧 session token、foreign World 和 terminating entity 均被拒绝 | Entity/生命周期 verifier |
| 玩家移动 | 输入、位置、速度、碰撞和接地状态由同一 Arch 状态观察 | Player input/lifecycle、SpatialSimulation |
| 玩家复活 | 普通复活保 UUID；真实销毁重建换 UUID；旧引用不影响新实体 | Player lifecycle verifier |
| NPC 基础战斗 | 目标选择、移动、伤害、生命减少、死亡和释放顺序稳定 | NPC damage/combat verifier |
| Projectile | hydration、运动、碰撞、伤害、穿透、过期和 identity index 清理闭合 | Projectile verifier |
| WorldItem | 预留、拾取、数量变化、失败恢复和过期请求不会误删重建实体 | Items verifier |
| 世界切换 | 旧 World 停止接受命令，新 World 建立新的 token 和 registry | World/session verifier |
| 调度 | Arch Group 不改变 kernel 的命令 drain、时钟、快照和 tick commit 顺序 | Application simulation verifier |
| 混合组件 | L1 字段可用，L2–L4 字段没有被基本玩法 System 隐式推进 | 静态引用审计和字段级审查 |

## 10. 失败、暂停和回滚规则

- Arch API 探针或包版本不兼容时，停止在 A1，不向生产项目添加猜测性的兼容层。
- 如果某个 L1 caller 仍需要 L2–L4 字段，先为该字段建立明确 projection 或拆分批次；不得通过整块恢复已删除系统来解决编译错误。
- 如果发现 Arch 查询顺序被当作旧槽位顺序使用，暂停 A4，增加显式领域顺序并重新编译。
- 如果发现两个 owner 同时写同一组件或库存/关系事实，暂停 A3–A5，先确定唯一写入源。
- 每个批次保留单独的提交或文件快照；回滚时回滚整个批次，不能只回滚构造器而保留已经切换的消费者。
- 不使用 `git reset --hard`、`git clean` 或覆盖式 checkout 清理当前工作树已有的组件删减、审核文档和未跟踪审核产物。

## 11. 完成定义

L1 Arch 迁移只有在以下条件同时满足时才可标记为 accepted：

1. 68 个目标组件全部有 Arch 存储/投影/瞬时值决定，并且每个决定有当前生产消费者证据。
2. Player、NPC、Projectile、WorldItem 和 World singleton 的创建、发布、销毁、切换和失败回滚都由统一 session/registry 合同管理。
3. L1 System 不再调用自定义 ECS 的存储、句柄、快照和通用 phase 接口；Arch System/Group 的调用闭包可以增量编译。
4. `PlayerInventoryComponent`/`PlayerInventorySlotsComponent`、NPC parent/target、Projectile identity、WorldItem reservation 等混合关系只有一个可写 owner。
5. 受影响项目的构建命令、exit code、warning/error 数和 `Build/bin/` 输出均有记录。
6. 在当前门禁允许的情况下，L1 行为 verifier 通过；若仍为 compile-only，明确标记运行证据为 `not-run`，不提前声称完成。
7. L2–L4 的 140 个兼容尾部文件没有被误接入 L1 默认组合；后续删除或迁移另立计划。
8. 文档 manifest、关联审计和本计划的状态与实际源码、分支和证据保持一致。

本计划完成后，下一份文档应是 A7 的执行证据或 A8 的删除审计，而不是再次创建一个覆盖已退出高级玩法的全量 Arch 计划。

## 2026-10-09：自造 ECS 旧入口边界复核

旧 `EntityRuntime` 类型、`EntityEcs` 命名空间、`Terraria.EntityEcs` 项目名、旧通用快照/状态命名以及 `IWorldSimulationTickPhase` 已从当前 `src/` 与 `Test/` 编译入口退出。唯一组件存储仍为 Arch `World`；`EntityLifecycleRegistry` 仅作为身份、生命周期和迁移期版本投影边界保留。

当前未完成项是迁移期通用组件访问方法（`TryEdit`、`TryCapture`、`TryAttach`、`TryReplace`、`Match` 等）仍被部分 caller 使用。该状态记录为 partial；本批次没有运行测试，只做静态扫描和增量构建。

## 2026-10-09：A8 自造 ECS 框架最终退出

本轮完成了 A8 的源码退出边界。生产组件状态唯一存储为 session-owned Arch `World`；Player、NPC、Projectile、Item、WorldItem、TileEntity、网络 owner 和 Simulation 路径均直接操作 Arch `World/Entity`。旧 `EntityRuntime`、`ComponentStore<T>`、`IComponentStore`、`ComponentAccess`、通用版本快照、旧 phase 接口和 `Terraria.EntityEcs` 项目入口已从生产编译闭包删除。

`EntityLifecycleRegistry` 仍保留为领域身份与生命周期投影：维护 `RuntimeEntityHandle`、UUID、发布/终止状态和 Arch 实体绑定，不保存组件、不执行组件查询、不提供通用编辑或快照 API。这个投影不构成第二套 ECS。旧的 `RuntimeEntityHandle` 也只用于领域引用、网络 epoch 和会话切换，不能作为组件存储句柄。

本轮只做静态扫描、旧忽略构建产物清理和生产项目增量编译，没有运行测试或运行时验证。A7 行为验收状态继续为 `not-run`；编译记录和静态扫描见 `Build/diagnostics/ArchMigration/L1-A8/custom-ecs-exit/`。
