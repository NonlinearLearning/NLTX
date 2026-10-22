# 简单游戏组件分级审核

逻辑域：reviews。产物类型：audit。状态：分级建议，等待用户审核。

审核日期：2026-10-08。源码基准：`5bbdd78b78fb5bc8bfd926063de6e3aa542db503`，分支 `main`。

范围：`D:/TRbackup/NLTX/src/NSSLC/Component` 下 **487 个以 Component.cs 结尾的文件**，逐项按实际声明类型登记。关联检查覆盖该目录的 208 个 `*System.cs` 文件，并扫描 `src/`、`Test/` 的 3,538 个 C# 文件；排除 bin、obj 和只读分类参考目录。

这是完整的静态分级清单。分级按所选简单游戏的需求提出，源码类型引用用于判断改动影响。它不等于逐项行为验收或可直接删除证明。本次没有删除／修改生产代码，没有执行 .NET 编译、游戏运行或删减后的行为验证。

## 1. 审核基线与结论

按二维简单游戏评估：玩家走跑跳与碰撞、少量简单 NPC、直接伤害和死亡重生、主背包、地面掉落／拾取、一种基础射弹、预置小地图。装备、合成、Buff、液体和城镇可以按需开启；首领、入侵、特殊种子、完整泰拉瑞亚世界生成、钓鱼、宠物和拴系实体默认退出最小版。

外部参考位于 `D:/Component/小众`。扫描了 103 个参考文件，其中 77 个项目组件有同名参考文件；这里只作为线索，不按外部目录位置定级。

| 等级 | 定义 | 文件数量 | 最小版建议 |
| --- | --- | ---: | --- |
| L0 基础运行 | 当前链路使用的身份、空间权威状态和世界加载基础 | 6 | 保留核心 |
| L1 基本玩法 | 所选移动、玩家、NPC、战斗和物品闭环 | 62 | 保留基本能力，裁混合字段／分支 |
| L2 通用可选玩法 | 装备、容器、合成、状态效果、液体等 | 145 | 初版默认关闭，明确选用再保留 |
| L3 专用扩展玩法 | 特定内容、首领事件、特殊生成与复杂子玩法 | 190 | 最小版移除候选 |
| L4 表现、诊断与候选模型 | 呈现、仪表、调试及未接入的平行表示 | 84 | 退出主运行，按兼容需求迁投影或归档 |
| 合计 | 每个范围内组件文件恰好登记一次 | 487 | 用户逐项审核 |

**L0＋L1 共 68 个文件，是当前保留审核起点，不是最终组件数量上限。** 当算法参数、初始化、恢复和协议仍要求某个 L2～L4 类型时，先调整入口再删除类型。分级低只表示初版需求低，不能证明当下删除后仍可编译／运行。

当前 237 个组件没有发现生产类型消费者。它们分散在多个等级；未发现引用不等于无用途或可安全删除。经现用表达对照，41 个文件被单列为“未接入的候选／平行模型”，避免把同一能力的多种表达都纳入最小运行时。

L4 的“表现／诊断”与“候选模型”是两种不同处理原因，清单中的“系统能力”“分级理由”保留此区别。L1 也不宣称 62 个组件已形成网络服务器的完整权威循环；它表示审核后的目标能力。

## 2. L0：基础组件与目录外的必要支撑

以下等级以当前空间模拟和联网主体为范围。去掉它们且没有等价替代，会破坏现有运行实体或加载链路。

| 组件 | 主要作用 | 相关入口 | 建议 |
| --- | --- | --- | --- |
| `PlayerIdentityComponent` | 保存玩家持久身份、名字、队伍、难度和连接身份。 | [NetworkPlayerOwner.cs](D:/TRbackup/NLTX/src/NSSLC.Application/Network/NetworkPlayerOwner.cs:645) | 保留身份与连接核心，裁剪可选元数据 |
| `ColliderComponent` | 保存实体碰撞形状的尺寸、偏移和启用状态。 | [RuntimePlayerEntity.cs](D:/TRbackup/NLTX/src/NSSLC.Tools.Simulation/RuntimePlayerEntity.cs:127) | 保留 |
| `EntityIdentityComponent` | 保存 ECS 实体的全局 UUID 身份。 | [EntityRuntime.cs](D:/TRbackup/NLTX/src/NSSLC/Component/Share/Entity/System/EntityRuntime.cs:115) | 保留 |
| `LocationComponent` | 保存实体的二维世界位置。 | [RuntimePlayerEntity.cs](D:/TRbackup/NLTX/src/NSSLC.Tools.Simulation/RuntimePlayerEntity.cs:78) | 保留 |
| `VelocityComponent` | 保存实体的二维速度。 | [RuntimePlayerEntity.cs](D:/TRbackup/NLTX/src/NSSLC.Tools.Simulation/RuntimePlayerEntity.cs:81) | 保留 |
| `WorldLoadLifecycleComponent` | 保存世界加载或生成的状态、失败恢复和重置要求。 | [LoadedWorldSession.cs](D:/TRbackup/NLTX/src/NSSLC.Application/WorldStorage/Loading/LoadedWorldSession.cs:22) | 保留 |

**服务器的全部基础不在这 6 个组件里。** 必须同时保留实体创建／销毁、代数与组件访问、世界会话、地图存储、tick／命令调度、玩家会话归属和网络入口。

| 必要能力 | 当前实现／表达 | 说明 |
| --- | --- | --- |
| 实体运行与有效引用 | [EntityRuntime.cs](D:/TRbackup/NLTX/src/NSSLC/Component/Share/Entity/System/EntityRuntime.cs:101)、身份 registry、component store | 生命周期、UUID、引用失效及 owner 线程 |
| 世界描述与时间 | [WorldDescriptorState.cs](D:/TRbackup/NLTX/src/NSSLC/Component/WorldSession/System/WorldDescriptorState.cs:7)、[WorldSessionComponents.cs](D:/TRbackup/NLTX/src/NSSLC/Component/WorldSession/System/WorldSessionComponents.cs:13) | 现用状态不是同名候选 Component |
| 方块地图 | [TileMapStore.cs](D:/TRbackup/NLTX/src/NSSLC/Component/WorldStorage/System/TileMapStore.cs:6) | 固体、边界和地图存储能力必须保留 |
| 会话就绪与回收 | [LoadedWorldSession.cs](D:/TRbackup/NLTX/src/NSSLC.Application/WorldStorage/Loading/LoadedWorldSession.cs:12) | 加载、就绪、切换与失败清理 |
| 权威运算调度 | [WorldSimulationKernel.cs](D:/TRbackup/NLTX/src/NSSLC.Application/Simulation/WorldSimulationKernel.cs:14) | tick、命令、阶段、快照和提交 |
| 物品使用时间状态 | [PlayerItemUseState.cs](D:/TRbackup/NLTX/src/NSSLC/Component/Player/System/PlayerItemUseState.cs:3) | 实际附加状态，避免重复引入无消费者的 timing／reuse 组件 |

这些支撑单列，不计入 487 个组件文件。目录名为 Component 并不意味着目录内每个类都是持久 ECS 组件。

## 3. L1：基本玩法保留范围

| 领域 | 基本玩法组件 |
| --- | --- |
| Combat | `DamageAcceptancePolicyComponent`、`DefenseComponent`、`HealthComponent`、`HitCooldownComponent`、`ImmunityComponent` |
| Items | `ItemDefinitionComponent`、`ItemInstanceComponent`、`ItemInventoryRelationComponent`、`ItemStackComponent`、`WeaponComponent`、`WorldItemComponent`、`WorldItemReservationComponent`、`WorldItemStateComponent` |
| Npc | `NpcNetworkSyncIntentComponent`、`NpcAiStateComponent`、`NpcBehaviorComponent`、`NpcBehaviorStateComponent`、`NpcDefinitionReferenceComponent`、`NpcDirectionComponent`、`NpcEntityIdentityComponent`、`NpcHealthComponent`、`NpcHitStateComponent`、`NpcImmediateEffectStateComponent`、`NpcLegacySlotComponent`、`NpcLifecycleComponent`、`NpcLocalBehaviorStateComponent`、`NpcMovementTickStateComponent`、`NpcNaturalDespawnStateComponent`、`NpcTargetComponent`、`NpcTargetSelectionStateComponent`、`NpcTaskStateComponent` |
| Player | `PlayerMovementPhysicsStateComponent`、`PlayerContactImmunityComponent`、`PlayerInstantMovementAccumulatorComponent`、`PlayerInventoryComponent`、`PlayerInventorySlotsComponent`、`PlayerItemUseIntentComponent`、`PlayerLifecycleComponent`、`PlayerNetworkStateComponent`、`PlayerRawControlInputComponent`、`PlayerReleaseAndRepeatStateComponent`、`PlayerSpawnPointComponent`、`PlayerVitalStateComponent` |
| Projectile | `ProjectileBehaviorStateComponent`、`ProjectileCollisionPolicyComponent`、`ProjectileDamagePayloadComponent`、`ProjectileDefinitionComponent`、`ProjectileDispositionStateComponent`、`ProjectileGeometryStateComponent`、`ProjectileHitImmunityPolicyComponent`、`ProjectileHitImmunityStateComponent`、`ProjectileIdentityComponent`、`ProjectileKinematicsStateComponent`、`ProjectileLifetimeStateComponent`、`ProjectileNetworkStateComponent`、`ProjectilePenetrationStateComponent`、`ProjectileSourceMetadataComponent`、`ProjectileTrajectoryStateComponent`、`ProjectileUpdateCadenceComponent` |
| Share | `DirectionComponent` |
| SpatialSimulation | `MovementGroundedStateComponent`、`MovementStateComponent` |

这份列表保留当前必要表示，后续仍可合并重复聚合体。比如 `PlayerInventoryComponent` 和 `PlayerInventorySlotsComponent` 同时有生产引用，需要先明确选中槽位和背包内容的唯一归属；`NpcAiState` 与行为状态也不能仅按字段相似删除。泛型 `HealthComponent` 与玩家／NPC 专用生命组件并存，是否统一是后续实现选择，不把当前分级作为统一 owner 的证明。

`MovementStateComponent` 是一次算法调用的输入／输出值，现实体从位置、速度和接地状态组装它。保留移动运算不要求把这个值再附加为持久组件。

## 4. 各领域分级数量

| 领域 | 总数 | L0 | L1 | L2 | L3 | L4 |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Combat | 13 | 0 | 5 | 7 | 1 | 0 |
| DeathPenaltyAndRevenge | 6 | 0 | 0 | 0 | 6 | 0 |
| Fishing | 6 | 0 | 0 | 0 | 6 | 0 |
| Items | 28 | 0 | 8 | 16 | 0 | 4 |
| LeashedEntity | 10 | 0 | 0 | 0 | 10 | 0 |
| Npc | 31 | 0 | 18 | 7 | 1 | 5 |
| Physics | 5 | 0 | 0 | 1 | 0 | 4 |
| Player | 167 | 1 | 12 | 51 | 69 | 34 |
| Projectile | 38 | 0 | 16 | 6 | 6 | 10 |
| Share | 10 | 4 | 1 | 4 | 0 | 1 |
| SimulationRuleOverrides | 5 | 0 | 0 | 0 | 5 | 0 |
| SpatialSimulation | 8 | 0 | 2 | 1 | 0 | 5 |
| StatusEffects | 1 | 0 | 0 | 1 | 0 | 0 |
| Teleportation | 6 | 0 | 0 | 2 | 4 | 0 |
| Town | 9 | 0 | 0 | 6 | 3 | 0 |
| WorldInteraction | 31 | 0 | 0 | 11 | 11 | 9 |
| WorldSession | 101 | 1 | 0 | 24 | 64 | 12 |
| WorldStorage | 12 | 0 | 0 | 8 | 4 | 0 |

## 5. 必须按依赖处理的边界

下表列的是改变删减顺序的实际源码点。详细类型消费者及行号见 CSV／JSON；`*System.cs` 之外的 owner、coordinator、query 和 adapter 同样可能持有或读写状态。

| 主题 | 源码观察 | 建议处理 | 证据 |
| --- | --- | --- | --- |
| 全局实体身份 | EntityRuntime.CreateEntity 分配 UUID 并附加共享 EntityIdentity；它与 Npc／Projectile 的业务槽位不是同一层身份。 | 保留全局身份、代数、访问和销毁语义。局部玩法身份随局部玩法裁剪。 | [EntityRuntime.cs](D:/TRbackup/NLTX/src/NSSLC/Component/Share/Entity/System/EntityRuntime.cs:115) |
| 空间权威状态 | 玩家和 NPC 从 Location、Velocity 与 Grounded 组装 MovementState，再把结果提交回这些状态。 | 保留实际附加的数据。MovementState 保留为算法调用值，不能当作重复持久组件增加一份权威状态。 | [RuntimePlayerEntity.cs](D:/TRbackup/NLTX/src/NSSLC.Tools.Simulation/RuntimePlayerEntity.cs:74) |
| 基础世界描述与时钟 | Kernel 使用 WorldDescriptorState 与 WorldTimeWeatherState；同目录的 WorldDescriptorComponent／CalendarClockComponent 当前无生产类型消费者。 | 可以退出候选模型，保留现用世界描述和 tick。文件名不代表实际接入。 | [WorldSimulationKernel.cs](D:/TRbackup/NLTX/src/NSSLC.Application/Simulation/WorldSimulationKernel.cs:224) |
| 世界时钟的扩展耦合 | WorldSimulationClockSystem 同时推进世界时间、月夜事件、入侵冷却和天气。 | 先收窄到基本 tick／时间推进，再把天气与事件作为可选调用移出；不能直接删除整个时钟。 | [WorldSimulationClockSystem.cs](D:/TRbackup/NLTX/src/NSSLC.Application/Simulation/WorldSimulationClockSystem.cs:111) |
| 世界加载与恢复 | LoadedWorldSession 实际持有 WorldLoadLifecycle；加载恢复还涉及液体和完整存档对象。 | 保留加载就绪、失败清理、世界切换与回收。关闭液体／住房后同步缩减恢复步骤和门禁。 | [LoadedWorldSession.cs](D:/TRbackup/NLTX/src/NSSLC.Application/WorldStorage/Loading/LoadedWorldSession.cs:22) |
| 玩家网络状态 | PlayerNetworkState 接收控制、动作、战斗和交互状态，也混有外观、召唤物和传送。 | 保留输入与所选协议同步，逐字段裁专用内容。外部小众目录含同名文件不意味着整块可删。 | [NetworkPlayerOwner.State.cs](D:/TRbackup/NLTX/src/NSSLC.Application/Network/NetworkPlayerOwner.State.cs:145) |
| 物品使用的现用状态 | RuntimePlayerEntity 实际附加 PlayerItemUseState；ItemActionTimingComponent／ItemReuseStateComponent 无生产类型消费者。 | 保留现用使用／重用状态机，候选组件可进入归档审核。 | [RuntimePlayerEntity.cs](D:/TRbackup/NLTX/src/NSSLC.Tools.Simulation/RuntimePlayerEntity.cs:344) |
| 玩家更新中的压力板 | ActivePlayerTickPhase 持有并更新 ActivePressurePlateTickPhase。 | 关闭电路玩法时先移出玩家阶段的压力板调用，再清理压力板索引与关联组件。 | [ActivePlayerTickPhase.cs](D:/TRbackup/NLTX/src/NSSLC.Tools.Simulation/ActivePlayerTickPhase.cs:10) |
| NPC 的完整生成选择 | ActiveNpcTickPhase 在 NPC 更新前执行自然生成，生成选择包含多种环境／事件。 | 初版采用固定生成或少量简单类型，裁掉入侵、首领、特殊群落分支，同时保留生命、移动和目标。 | [ActiveNpcTickPhase.cs](D:/TRbackup/NLTX/src/NSSLC.Tools.Simulation/ActiveNpcTickPhase.cs:12) |
| 射弹初始化的专用状态 | ProjectileInitialComponents 同时组装基础射弹、浮标、配重、召唤物、动画和历史。 | 选一种基础射弹，同步缩减 hydration、initial components、clone、tick 和 network projection。 | [ProjectileInitialComponents.cs](D:/TRbackup/NLTX/src/NSSLC/Component/Projectile/System/ProjectileInitialComponents.cs:61) |
| 轨迹缓存参与碰撞 | ProjectileCollidingGeometryQuery 读取 OldPositions、OldRotations 和 WhipPoints，缓存不全是残影。 | 先裁长轨迹、旋转轨迹和鞭子碰撞，随后移出对应历史缓存。 | [ProjectileCollidingGeometryQuery.cs](D:/TRbackup/NLTX/src/NSSLC/Component/Projectile/System/ProjectileCollidingGeometryQuery.cs:1198) |
| 动画帧参与特定伤害 | ProjectileDamageGateQuery 对 type 759 的宠物例外读取 Frame／FrameCount。 | 关闭宠物伤害例外后再裁动画状态，不能仅根据 Animation 名称迁到客户端。 | [ProjectileDamageGateQuery.cs](D:/TRbackup/NLTX/src/NSSLC/Component/Projectile/System/ProjectileDamageGateQuery.cs:114) |
| 两个方块绑定类型 | TileEntityStore 消费 Terraria.WorldStorage.TileEntityBindingComponent；Items 命名空间的同名类型未发现消费者。 | 按完整类型和构造参数辨认，保留需要的存档／方块实体绑定，退出未接入版本。 | [TileEntityStore.cs](D:/TRbackup/NLTX/src/NSSLC/Component/WorldStorage/System/TileEntityStore.cs:433) |
| 文件名与类型名不一致 | DamageReductionComponent.cs 实际声明 HarmReductionComponent。词法扫描已按真实类型核对。 | 审核和搜索使用 HarmReductionComponent；本次只记录，不改源文件名或类型名。 | [DamageReductionComponent.cs](D:/TRbackup/NLTX/src/NSSLC/Component/Combat/DamageReductionComponent.cs:14) |
| NetworkServer 时钟入口限制 | NetworkServer 对 world item owner 使用 () => 0L，源码注明尚未暴露 live game-tick clock。 | 保留组件清单不能证明网络服务器已接好权威玩法。后续最小实现需接入 tick、阶段和网关输入／输出。 | [Program.cs](D:/TRbackup/NLTX/src/NSSLC.Tools.NetworkServer/Program.cs:78) |

另外，继续兼容原泰拉瑞亚客户端／WorldFile 会要求保留一些低等级字段的传输或恢复投影。尤其方块帧既可能用于显示，也可能用于多格结构定位；本次把未接入的 `TileFrameComponent` 列为候选归档，不代表地图中的所有帧字段都能删掉。

## 6. 审核与后续删减顺序

1. 先确认游戏基线以及是否继续使用原客户端／存档。按清单的黄色输入栏填写“审核决定”“调整等级”“审核备注”。可以选择同意建议、保留、移除或待讨论。
2. 先处理无生产消费者的候选表示，并核对动态注册、恢复和协议。它们属于退出旧模型的候选，不能通过批量删除代替核对。
3. 关闭 L3 功能入口：钓鱼、复仇、拴系、宠物／召唤物、坐骑、高级装备、入侵／首领、困难模式、特殊生成、电路。对应组件与系统按同一功能组处理。
4. 按所选玩法关闭 L2：液体、合成、容器、装备、Buff、城镇和传送。保留加载／会话基础，缩减门禁与恢复步骤。
5. 缩减混合组件、coordinator、初始化和协议投影。把基础时间、输入、移动、生命、背包和掉落闭环留住。
6. 最后退出 L4 表现和诊断更新。原客户端／存档仍需的数据通过必要投影保留。

后续实现需要按受影响项目执行增量构建，并验证：实体创建与回收、世界加载失败／重试／切换、tick 单次推进、输入到移动／碰撞、NPC 目标与伤害、死亡重生、掉落拾取与数量守恒、基础射弹命中及同步。预期关闭的功能应没有残余入口。构建和运行输出遵守仓库 `Build/` 约束。

## 7. 逐项清单与证据范围

- [逐组件 CSV](2026-10-08-simple-game-component-grading.csv)：487 行，含等级、作用、分级理由、处理建议、系统、源文件／声明行、生产／测试引用及审核栏。
- [结构化静态证据](2026-10-08-simple-game-component-grading.json)：完整命名空间、文件 SHA256、消费者片段、外部匹配、分级和 208 个 System 文件目录。
- Excel 审核表：分级总览、组件审核、重点边界和源码依赖四页；按等级／领域筛选，在黄色栏审核。

清单按真实声明名登记。唯一的文件名／类型名差异为 `DamageReductionComponent.cs` 中的 `HarmReductionComponent`。

静态扫描按词法标识符搜集类型使用，去掉注释、字符串和同名声明文件；重复简单名另核对现用 namespace／using，并保留原始词法候选。测试引用仍是词法候选。静态命中不证明真实调度，也不穷尽反射、泛型动态注册、事件、委托或外部协议。字段列表只是公开声明摘要，不是完备语义字段清单。

覆盖检查确认 487 个源文件、487 个完整类型身份无重复、等级与领域数量一致，并核对源码 SHA256 与读取时相同。该检查只验证清单完整性与源码绑定，不宣称游戏行为验证通过。
