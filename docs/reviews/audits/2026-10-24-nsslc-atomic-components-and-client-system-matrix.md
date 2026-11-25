# NSSLC 原子组件与主要游戏系统盘点

- 日期：2026-10-24
- 范围：src/NSSLC/Components/Share、src/NSSLC/Components/Npc、src/NSSLC/Terraria.Components.csproj
- 客户端参考：D:/TRbackup/最新的客户端/Terraria
- 相关设计：`2026-10-24-projectile-ecs-system-design.md`（2026-11-24 删除；继任见 `2026-10-25-projectile-implementation.md` 与 `src/NSSLC/Projectile/README.md`）
- 结论状态：confirmed、partial、proposed 混合；不代表全部运行时已接入。

这里的“原子组件”指一个实体能力或一个权威状态的一份最小组件。它不等于把客户端字段
逐个翻译成字段，也不等于为每个方法创建组件。组件保存状态或能力声明，系统维护规则和
提交不变量，快照、命令、查询、网络 DTO 和表现数据不作为第二份权威组件。

## 1. 盘点结论

当前 NSSLC Share 与 NPC 领域共有约 306 个 C# 文件，主要领域如下：

| 领域 | 文件数 | 当前性质 |
| --- | ---: | --- |
| AI | 151 | HTN/GOAP 计算、绑定和示例；没有完整 Arch Tick 接入 |
| Physics | 20 | 空间快照、Tile 运动、接触、碰撞和提交合同 |
| Entity | 21 | 身份、定义、ECS 组织和基础空间状态 |
| Movement | 16 | 速度、移动能力、意图和重力 |
| Attributes | 24 | 属性值、限时修饰器、Buff 定义、接收准入和 Buff 生命周期 |
| World | 12 | 实体世界交互请求和端口 |
| Inventory | 10 | 槽位、持有关系和跨实体转移 |
| Item | 8 | 物品定义、堆状态和世界物品状态 |
| Liquid | 4 | 液体接触事实和交互能力 |
| Combat | 5 | 生命、减伤、击退、生命恢复、氧气状态 |
| Projectile | 29+ | Share/Projectile 已有基础组件、AI 适配、纯模拟、生命周期、碰撞候选和命中提交；宿主 Tick 装配仍 partial |
| Npc | 6 | 行为选择、可观察行为状态、NPC AI 意图和共享 AI 适配器 |

项目文件编译 Component/**/*.cs 和 Components/**/*.cs。实际恢复的共享源码集中在
Components/Share；不要因为历史路径存在过，就假设旧 Projectile 或旧系统仍在当前编译范围。

当前已经存在的运行组织入口是 EcsServer。它按 Preparation、Simulation、Commit、Projection
组织已注册系统，不自动发现系统，也不替代本地世界时钟、世界文件或持久化。本次最小闭环
不包含网络会话、协议处理和出站复制。

## 2. 当前 NSSLC 原子组件

### 2.1 Entity、身份和基础几何

| 当前组件或类型 | 作用 | 状态 |
| --- | --- | --- |
| EntityIdentityComponent | 保存 EntityUuid 根身份；只读身份输入，不由空间 Tick 改写 | confirmed |
| EntityRuntimeHandle | world scope + generation 的运行时引用 | confirmed；不是实体组件 |
| EntityDefinitionId、EntityDefinition | 定义身份和初始组合 | confirmed |
| EntityDefinitionRegistry | 不可变定义目录 | confirmed |
| EntityFactionComponent | 阵营事实；供关系/碰撞筛选 | confirmed |
| EntitySizeComponent | 逻辑尺寸；可转换为空间查询尺寸 | confirmed |
| LocationComponent | 权威位置；空间协调系统唯一常规写入者 | confirmed |
| Movement.VelocityComponent | 每秒线速度；空间协调系统唯一常规写入者 | confirmed |
| PositionComponent、Entity/VelocityComponent | 旧调用方投影 | partial/deprecated，不应加入新定义 |
| SpatialRevisionComponent | 空间提交版本 | removed；Arch 组合查询不再挂载实体级 revision 组件 |
| EcsServerCommandBuffer | 结构变化缓冲 | confirmed，不是组件 |

当前没有通用生命周期/处置状态、通用跨领域实体 revision、owner session、来源关系和协议
投影组件。Projectile 设计需要的 disposition、owner、source、network 状态必须归属明确
领域，不能用 Arch Entity 或整数槽位代替。

### 客户端状态剔除边界

NSSLC 当前不把客户端控制和表现状态建模为实体组件。以下候选已从 Player 专属组件和系统
清单中移除：

- `LocalPlayerControlComponent`、`PlayerInputStateComponent`、`PlayerAimComponent`；
- `LocalPlayerControlSystem`、`PlayerInputPreparationSystem`、`PlayerAimSystem`。

它们属于客户端设备、界面或输入适配边界，不属于服务器实体状态。服务器只接收宿主已经
验证过的动作命令或 `MovementIntent`，再由 Player/Movement 系统检查 Tick、能力、范围、
资源和生命状态。输入、鼠标、键盘、摄像机、UI、预测位置和表现目标都不进入 Arch Entity。
`PlayerTargetingStateComponent` 只有在目标选择由服务器权威维护时才保留；客户端锁定框、
光标目标和表现选择不写入该组件。

### 2.2 Movement 和 Physics

| 当前组件或类型 | 作用 | 状态 |
| --- | --- | --- |
| MovementCapabilityComponent | 速度、加速度、跳跃和移动模式能力 | confirmed |
| MovementIntent | 本 Tick 的移动请求 | confirmed；一次性输入，不是结果 |
| GravityCapabilityComponent | 是否参与重力 | confirmed |
| GravityConfigurationComponent | 基础重力和默认方向 | confirmed |
| GravityDirectionStateComponent | 当前离散重力方向 | confirmed |
| GravityModifierCollectionComponent | 带来源和期限的修饰器 | confirmed |
| GravityStateComponent | 本帧重力投影 | confirmed |
| ColliderComponent | 形状、碰撞尺寸和偏移 | confirmed |
| GroundingComponent | 上一空间提交的接地和法线 | confirmed |
| TileCollisionCapabilityComponent | 是否采用 Tile 碰撞 | confirmed |
| EntityCollisionCapabilityComponent | 实体碰撞层和过滤 | confirmed |
| LiquidContactComponent | 本帧液体接触事实 | confirmed |
| LiquidInteractionCapabilityComponent | 液体影响能力 | confirmed |
| SpatialFrame、SpatialEntitySnapshot | 短期脱离 ECS 输入 | confirmed；不是长期组件 |
| SpatialSimulationResult、SpatialCommit | 计算结果和完整写入值 | confirmed；不是实体状态 |
| SpatialWorldRevision | 世界版本 | confirmed |
| SpatialContactPair | 碰撞事实 | confirmed；不是伤害结果 |

基础空间组件足够作为 Projectile、Player、NPC 的共同几何底座。`SpatialSimulationCoordinatorSystem`
已提供本地 Arch 组合查询和唯一空间写入路径；完整 Tile sweep、平台/斜坡规则、实体 broadphase
空间索引和世界 revision 适配器仍是后续工作。

### 2.3 Combat、Attributes 和 Buff

| 当前组件或类型 | 作用 | 状态 |
| --- | --- | --- |
| LifeComponent | 当前、最大、最小生命；Combat owner 通过边界工厂更新 | confirmed |
| `AttributeSetComponent` | Defense、DamageReduction、Endurance 等属性的唯一权威来源；也保存 Player 基础防御和减伤属性 | confirmed |
| `ProtectionWindowComponent` | 共享目标保护窗口 | confirmed |
| `ProtectionWindowSystem` | 每个 Simulation Tick 递减共享保护窗口 | confirmed |
| KnockbackComponent | 通用击退规则输入，不是 Player 防御或抗性状态 | confirmed |
| HealthRegenerationComponent | 生命恢复率状态 | confirmed；文件名仍为 HealthRegenerationStateComponent.cs |
| OxygenBarComponent | 氧气资源 | confirmed |
| AttributeSetComponent | 基础值和属性修饰器 | confirmed |
| AttributeModifier | 属性来源、运算和时限 | confirmed |
| BuffListComponent | 当前共享 Buff 实例、剩余 Tick 和层数；属性贡献不在这里复制 | confirmed |
| BuffImmunityComponent | Buff 免疫集合；数值防御/抗性不在这里保存 | confirmed |
| BuffReceptionCapabilityComponent | 声明实体是否接受共享 Buff，以及允许的接收策略 | confirmed；共享能力，不属于 NPC |
| AttributeSystem、AttributeCalculator | 属性规则 | confirmed |
| AttributeTickSystem | 限时属性推进的 Arch 适配器 | confirmed |
| GravityModifierSystem | 重力规则 | confirmed |
| GravityTickSystem | 重力推进的 Arch 适配器 | confirmed |

数值 Buff/Debuff 统一通过 `AttributeModifier` 提供，Player 不再拥有独立的 Buff 状态组件。
`BuffListComponent` 和 `BuffImmunityComponent` 只在需要表达非数值效果标识、免疫、刷新、
叠层或周期行为时保留为通用状态；它们不复制属性值，也不构成 Player 专属层。

Combat 仍缺少通用的伤害请求入口、无敌帧、受击修饰、死亡处置、击退提交、状态效果
周期、玩家/NPC 战斗闭环和通用命中结果。Projectile 路径已经由
`ProjectileCombatCommitSystem` 局部闭合：候选命中经过准入、共享保护窗口、减伤、Life 提交，
再由 Projectile 消费 accepted receipt。Player 的基础防御、基础抗性和保护窗口由
`AttributeSetComponent` 保存属性；`ProtectionWindowComponent` 保存共享保护窗口；其装备/Buff 数值贡献通过 `Inventory` 和 `AttributeSetComponent`
提供，尚未形成完整的 NPC/Player 通用 Combat。装备/Buff 提供的防御、抗性、耐力等数值修饰由
`AttributeSetComponent` 聚合，Player 基础值仍由 `AttributeSetComponent` 保存；
无敌帧、受击免疫、闪避冷却和护盾消耗等通用规则消费共享保护窗口状态；不能把
`LifeComponent` 的存在写成已有完整战斗系统。

### 2.4 Item、Inventory 和 World

| 当前组件或类型 | 作用 | 状态 |
| --- | --- | --- |
| ItemDefinition、ItemDefinitionId、ItemDefinitionRegistry | 不可变物品定义 | confirmed |
| ItemStack、ItemStackComponent | 值堆和实体堆状态 | confirmed |
| ItemWorldStateComponent | 世界掉落的拾取延迟和寿命 | confirmed |
| InventoryComponent | 槽位权威状态 | confirmed |
| InventorySlotDefinition、InventorySlot | 槽位规则和内容 | confirmed |
| ContainedInInventoryComponent | 物品到容器/槽位的关系 | confirmed |
| InventorySystem | 插入、取出、转移、关系校验 | confirmed；同步领域 API |
| WorldInteractionCapabilityComponent | 世界操作权限 | confirmed |
| WorldInteractionRequestComponent | 一次请求的操作、坐标和 Tick | confirmed |
| WorldInteractionResultComponent | 请求结果 | confirmed |
| WorldInteractionSystem | Commit 阶段消费世界交互请求 | confirmed |
| WorldTileCoordinate、WorldTileState | 世界坐标和值快照 | confirmed |

Item/Inventory 合同相对完整，但没有装备、武器使用、弹药消耗、配方、制作、箱子、世界掉落
Tick、拾取和网络/持久化适配器。WorldInteraction 只提供请求端口，不等于已有完整 Tile、
墙体、线缆、液体、结构物或 WorldFile 系统。

### 2.5 AI

AI 目录提供 AiComponent、AiSnapshot、AiOutput、AiResult、AiSystem、AiEntitySystem、
HTN/GOAP 规划和示例行为。它能表达“快照 → 意图 → 领域系统提交”。Projectile、NPC 和
Player 的行为都属于 AI 系统的具体行为实现；领域组件只保存行为配置和可观察状态，不再
各自建立一套重复的 BehaviorSystem。当前仍没有通用 EcsServer AI Tick 适配器、Projectile
行为实现和 NPC 适配器已经提供，但正式目标关系组件和生产感知系统仍缺失；AI 适配器必须由
宿主显式注册，不能把文件存在误写成完整服务器接入。

行为选择分三层：`AiComponent.BehaviorType` 绑定 C# 行为类型，领域组件的 `BehaviorId`
选择稳定内容行为键，`ProfileId` 选择不可变参数目录项；行为通过 `AiRequirements` 声明
快照输入。Definition/Lifecycle Resolver 负责验证三者兼容，`AiSystem<TIntent>` 负责绑定
和计算，领域适配器负责提交状态，空间/战斗系统负责提交运动和生命结果。追踪、反弹、重力
等能力由具体组件是否存在表达，Arch 查询直接按组件组合筛选。

## 3. 当前实际存在的系统和适配器

| 系统/规则 | 读写范围 | 是否进入 EcsServer Tick | 结论 |
| --- | --- | --- | --- |
| EcsServer | 系统注册、阶段、Tick 合法性、故障状态 | 是，组织器 | 不是玩法总系统 |
| AttributeSystem | AttributeSet 内部修饰器 | 由调用方使用 | 规则已存在 |
| AttributeTickSystem | 查询 AttributeSet，推进时限 | 可注册 Simulation | confirmed |
| BuffAdmissionSystem | 接收能力、免疫、来源、阵营和叠层准入 | 由 BuffSystem 调用 | confirmed；共享 Buff 领域 |
| BuffSystem | BuffList 生命周期、属性贡献和周期效果意图 | 由宿主或 BuffTickSystem 调用 | confirmed；尚未接入完整内容周期效果 |
| GravityModifierSystem | 计算 GravityState、推进修饰器 | 由 Tick 系统使用 | 规则已存在 |
| GravityTickSystem | 查询重力组合并写状态 | 可注册 Simulation | confirmed |
| ProjectileCombatCommitSystem | 消费 Projectile 命中候选，读取 Player 防御/保护窗口与 Attributes，提交目标 Life 并交回 Projectile 回执 | 可注册 Commit | confirmed；Projectile 局部 Combat 闭环 |
| AiSystem | 通用行为工厂、绑定创建和计算入口；Projectile/NPC/Player 行为作为具体实现注册 | 否 | 运行时服务 |
| AiEntitySystem | entity 到 binding、计算、停止、重绑定、释放 | 否 | 宿主适配器 |
| SpatialSimulationSystem | detached movement、Tile Move、液体接触 | 否 | 纯模拟规则 |
| SpatialSimulationCoordinatorSystem | Arch 组合查询、空间计算与唯一空间写入 | 是 | 空间协调系统 |
| EntityCollisionSystem | detached AABB 接触候选 | 否 | 纯碰撞查询，不扣血 |
| InventorySystem | 库存/物品转移和关系提交 | 否 | 请求型同步领域系统 |
| InventoryAttributeContributionSystem | 从 Inventory 槽位和 ItemDefinition 重建 Equipment/Accessory/Weapon 属性来源 | 是，可注册 Commit 或 Simulation 前的确定阶段 | 共享属性桥接 |
| WorldInteractionSystem | Commit 阶段读取请求并调用世界端口 | 是，可注册 Commit | confirmed 局部接入 |
| EntityDefinitionSystem | 由定义创建/销毁组合 | 否 | 生命周期外观 |
| MovementIntentResolver | 按 Tick、来源、优先级选择意图 | 否 | 规则已存在，无运动 Tick owner |

当前 NSSLC 的“已有系统”主要是规则计算器、适配器和局部提交端口；Projectile 的命中/生命
提交闭环已有局部实现，完整 Player、NPC、通用 Combat、世界时间和物品 Tick 闭环仍缺失。
网络系统属于本次范围之外的可选适配层。

## 4. 最新客户端的主要权威游戏系统

客户端参考目录有 1,549 个 C# 文件。最大入口约为：

| 文件 | 约行数 | 主要行为 |
| --- | ---: | --- |
| NPC.cs | 96,179 | NPC 生成、AI、移动、战斗、Buff、Boss 和死亡 |
| WorldGen.cs | 87,337 | 世界生成、Tile 读写、结构、掉落和世界更新 |
| Projectile.cs | 81,411 | Projectile 定义、Spawn、AI、碰撞、命中、终止和投影字段 |
| Main.cs | 67,477 | 主循环、服务器/客户端更新、时间、天气、事件 |
| Player.cs | 57,766 | 输入、装备、移动、资源、战斗、死亡、重生和召唤物 |
| Item.cs | 49,964 | 物品定义、前缀、使用和状态 |
| Recipe.cs | 16,846 | 配方列表、材料消耗和制作条件 |
| Mount.cs | 6,722 | 坐骑能力和移动 |
| Collision.cs | 4,622 | AABB、线段、Tile 和实体几何 |
| MessageBuffer.cs | 4,538 | 入站协议解析，包含 Packet 27、28、29、142 等分支 |
| Chest.cs | 3,668 | 箱子内容和结构更新 |
| Wiring.cs | 3,435 | 线缆、机关和信号 |
| NetMessage.cs | 3,006 | 出站协议、区段同步和玩家/NPC/Projectile 投影 |
| Liquid.cs | 1,679 | 液体流动、接触和 Tile 联动 |
| WorldItem.cs | 1,897 | 世界掉落、拾取、寿命、Shimmer 和同步 |

这些入口混合了服务端权威逻辑、客户端预测、渲染、音频、UI、全局数组和协议。下面只抽取
服务器需要的大系统；粒子、绘制、灯光、音效、UI、地图渲染属于 Projection 或客户端专属。

### 4.1 世界会话、Tick、时间、天气和事件

**入口证据**

- Main.UpdateServer、Main.UpdateClient；
- Main.UpdateTime、UpdateTime_StartDay、UpdateTime_StartNight；
- Main.UpdateWeather；
- Main.UpdateInvasion；
- WorldGen.UpdateWorld、UpdateLunarApocalypse；
- WorldSections、WorldSize。

**已有的基础原子组件**

WorldSessionStateComponent、WorldTimeStateComponent、WorldWeatherStateComponent、
WorldEventStateComponent、WorldSpawnRuleStateComponent、WorldSectionRevisionComponent。

**缺少的领域系统**

WorldSessionLifecycleSystem、WorldClockSystem、WorldWeatherSystem、WorldEventSystem、
NpcSpawnDirectorSystem、WorldSectionProjectionSystem。

当前只有 WorldInteraction 和 spatial world revision 合同；时间、天气、事件、刷怪和区段
系统全部缺失，优先级 P0/P1。它们属于本地世界模拟，不依赖网络。

### 4.2 实体生命周期、定义和身份

**入口证据**

Projectile.NewProjectile/Kill、NPC.SpawnNPC/SpawnBoss、Player.Spawn/死亡重生、
WorldItem.SpawnShimmeredItem、Main.projectile/npc/player 槽位。

**实体生命周期原子组件**

EntityLifecycleStateComponent、EntityRevisionComponent、EntitySpawnSourceComponent、
EntityOwnerReferenceComponent、EntityTargetReferenceComponent。
EntityRuntimeHandle 仍应是短期引用，不应和根身份合并。Owner/target/network/persistence 也可
在 Projectile、NPC、Player 领域内拥有；网络投影不属于本次最小闭环，不应提前加入通用实体组件。

**缺少的领域系统**

EntityLifecycleSystem、EntityDefinitionHydrationSystem、EntityReferenceResolutionSystem、
EntityCleanupSystem、EntityProjectionIdentitySystem。

当前已有 EntityDefinition、EntityUuid、EntityRuntimeHandle 和 CommandBuffer，但没有完整
Arch 组件创建/撤销、索引登记、generation 失效和批量清理闭环。

### 4.3 空间、移动、Tile 碰撞和实体碰撞

**入口证据**

Collision.CheckAABBvAABBCollision、CheckAABBvLineCollision、Projectile.Colliding、
Player.Update_NPCCollision、WorldGen.SolidTile、WorldGen.InWorld、各实体 Update。

**已有原子组件**

LocationComponent、Movement.VelocityComponent、ColliderComponent、GroundingComponent、
MovementCapabilityComponent、MovementIntent、Gravity*、TileCollisionCapabilityComponent、
EntityCollisionCapabilityComponent。

**缺少的原子组件**

SpatialSweepPolicyComponent、SpatialLayerComponent（仅当现有 collision capability 不够）、
TeleportRequestComponent、ExternalForceRequestComponent、SpatialParticipationComponent。

**缺少的领域系统**

MovementIntentSystem、TileSweepCollisionSystem、EntityBroadphaseSystem、
ExternalForceCommitSystem、
SpatialParticipationSystem。

SpatialSimulationSystem、SpatialSimulationCoordinatorSystem、EntityCollisionSystem 是可复用规则/协调系统；
协调器已进入生产项目，完整 Tile sweep、broadphase 和输入链仍缺失。P0。

### 4.4 Projectile

**入口证据**

Projectile.SetDefaults、NewProjectile、Update、AI、Colliding、Kill；
MessageBuffer Packet 27、29；NetMessage Projectile 同步。

**Projectile 原子组件**

ProjectileOwnerComponent、ProjectileSourceMetadataComponent、
AiBehaviorSelectionComponent（只保存 BehaviorId/ProfileId）、ProjectileBehaviorStateComponent、
ProjectileHomingCapabilityComponent、ProjectileTargetFact、
ProjectileBounceCapabilityComponent、ProjectileBounceStateComponent、
ProjectileKinematicsStateComponent、ProjectileLifetimeStateComponent、
ProjectilePenetrationStateComponent、ProjectileCollisionPolicyComponent、
DamagePayloadComponent、ProjectileHitImmunityStateComponent、ProjectileDispositionStateComponent、
ProjectileTerminationRequestComponent。

重力复用 `GravityCapabilityComponent` 和 `GravityStateComponent`。Wet、Reflection、Trail、
Minion、Sentry、Trap 是可选能力，不应合并为 ProjectileDataComponent；它们只有在实际内容
需要时才增加独立能力/状态组件。

**已有/缺少的领域系统**

`ProjectileSimulationSystem`（消费 AI 意图）、行为目录/AI 注册、
`ProjectileLifecycleSystem`、`ProjectileRuntimeRegistry`、`ProjectileCollisionSystem`、
`ProjectileHitResolutionSystem`、
`ProjectileLifetimeTickSystem` 和 `ProjectileTerminationCommitSystem` 已实现；
Definition 查找由 `ProjectileLifecycleSystem` 内的不可变 catalog 解析完成。
Combat 侧的 `DamageAdmissionSystem`、`DamageMitigationSystem`、`DamageCommitSystem` 和
`ProjectileCombatCommitSystem` 已提供 Projectile 命中准入、减伤、Life 提交和 Projectile
回执边界；这条闭环仍只覆盖 Projectile 候选，不等于通用 Player/NPC 攻击请求系统。

Projectile 行为本身由共享 `AiSystem<ProjectileAiIntent>` 承载，不新增独立
`ProjectileBehaviorSystem`；Projectile 只提供行为实现、`BehaviorId`/`ProfileId` 映射、
`AiRequirements`、快照适配器和 intent 类型。Arch 查询通过能力组件组合筛选实体，行为系统不
读取一组布尔能力字段。

Projectile 基础组件、不可变行为目录、共享 AI 行为适配、脱离 ECS 的纯模拟、Arch 生命周期
创建/终止、实体 broadphase 候选、命中准入、目标 Life 提交、穿透/免疫和寿命提交已实现。
仍需宿主把 AI/空间/碰撞/Combat 系统注册到真实 Tick，并为 NPC/Player 提供统一 runtime handle
解析；Tile 影响策略和完整 Combat intent 仍是后续接入工作。完整契约见 Projectile ECS 系统
设计文档和实现说明。

### 4.5 NPC 生成、AI、目标、移动、战斗和死亡

**入口证据**

NPC.SpawnNPC、SpawnOnPlayer、SpawnBoss、AI、UpdateNPC、UpdateFoundActiveNPCs、
StrikeNPC、StrikeNPCNoInteraction、UpdateNPC_BuffSetFlags、UpdateHomeTileState。

**缺少的原子组件**

NpcSpawnStateComponent、AiBehaviorSelectionComponent、
NpcBehaviorStateComponent、NpcTargetComponent、NpcHomeComponent、NpcCombatStateComponent、
NpcImmunityStateComponent、NpcDropPolicyComponent、NpcRelationshipComponent。

`NpcBuffStateComponent` 从 NPC 专属组件中移除。NPC 的属性和 Buff 能力通过共享组件组合表达：

```text
AttributeSetComponent
BuffListComponent
BuffImmunityComponent
BuffReceptionCapabilityComponent（需要显式接收策略时）
```

其中 `AttributeSetComponent` 声明实体拥有属性能力，`BuffListComponent` 保存当前共享 Buff
实例、剩余 Tick 和层数，`BuffImmunityComponent` 保存 Buff 免疫；三者由共享属性/Buff 系统维护。
数值 Buff/Debuff 直接进入 `AttributeModifier`，不在 Buff 列表中复制。NPC 仍可拥有
NPC 专属的战斗免疫或处置状态，但那不等同于 Buff 免疫，也不应重新包装成 NPC Buff 状态。
可复用 EntityFaction、EntitySize、Life、Collider、AI、Movement、Physics 和 Combat 基础。

`AiBehaviorSelectionComponent` 只保存 `BehaviorId` 和 `ProfileId`。`NpcBehaviorStateComponent` 只
保存跨 Tick 的可观察行为运行态，例如当前阶段、进入阶段的 Tick、下次决策时间、动作阶段和
有限的行为槽位；Planner、HTN Context、GOAP Agent、World、Arch Entity 和服务引用由共享
`AiBinding` 管理，不进入组件。NPC 行为通过 `AiComponent + AiBehaviorSelectionComponent +
NpcBehaviorStateComponent` 绑定到共享 `AiSystem<NpcAiIntent>`。

**已有/缺少的领域系统**

`NpcAiIntentAdapter`（共享 `AiSystem<NpcAiIntent>` 的 NPC 行为适配器）已实现；
NpcLifecycleSystem、NpcSpawnDirectorSystem、NpcPerceptionSystem、NpcTargetSelectionSystem、
NpcMovementSystem、NpcCombatSystem、NpcDeathAndDropSystem 仍缺失。

共享 Buff 接收和运行不建立 `NpcBuffSystem`。`BuffAdmissionSystem` 负责检查目标是否具有
接收能力、免疫、来源和叠层规则；`BuffSystem` 已实现更新 `BuffListComponent`、刷新/叠层/过期
和属性修饰器同步，周期效果仍由后续 Combat/Movement 系统消费。NPC、Player、Projectile
以及其他实体通过组件组合进入同一套查询。
周期伤害或治疗只能产生 Combat intent，由 Combat 系统更新目标 `LifeComponent`。

NPC AI 必须走 snapshot → intent → NPC 适配器提交行为状态 → spatial/combat commit，不能直接
写位置、速度或生命。`NpcAiIntentAdapter` 不是第二套 AI 引擎；它只负责快照投影、消费共享
AI 结果和提交 NPC 自己拥有的行为状态。优先级 P0/P1。

### 4.6 Player 移动、资源、战斗、死亡和重生

**入口证据**

Player.Update、UpdateEquips、UpdateArmorSets、UpdateBuffs、UpdateImmunity、UpdateLifeRegen、
UpdateManaRegen、Update_NPCCollision、Spawn、KillMe、UpdateMinionTarget、ConsumeItem、
PlaceThing、DropItems。

**Player 专属原子组件**

PlayerResourceStateComponent、PlayerDeathStateComponent、PlayerRespawnStateComponent、
PlayerTargetingStateComponent、PlayerInteractionCapabilityComponent、
PlayerCraftingContextComponent、AttributeSetComponent + ProtectionWindowComponent。

以上七个组件已在 `src/NSSLC/Components/Player` 实现；它们只承载 Player 独有的资源、生命
转换、目标关系、交互能力、制作上下文和服务器防御事实。

Player 不再声明 `PlayerEquipmentComponent` 或 `PlayerBuffStateComponent`；
`AttributeSetComponent + ProtectionWindowComponent` 保留为服务器权威的 Player 防御状态：

- 背包、武器、护甲、饰品和弹药由 `InventoryComponent` 的 `InventorySlotDefinition`、
  `InventorySlot`、`ItemStackComponent` 和物品实体关系表达；槽位不是 Player 私有组件。
- `AttributeSetComponent + ProtectionWindowComponent` 保存 Player 的基础 `Defense`、`Resistance` 和保护窗口剩余
  Tick；`ProtectionWindowSystem` 只负责保护窗口倒计时。
- Inventory 的护甲、饰品和武器贡献，以及数值 Buff/Debuff，仍通过
  `AttributeSetComponent`/`AttributeModifier` 叠加到 Combat 结算输入；它们不复制到
  `AttributeSetComponent + ProtectionWindowComponent`。
- 受击准入、最终减伤、闪避、护盾消耗和其他实体通用保护规则由 Combat owner 消费这个状态；
  不再建立第二个 Player 防御组件或客户端防御状态。
- 数值 Buff/Debuff 通过 `AttributeModifier` 表达；非数值状态效果若仍需要标识、免疫、
  刷新或周期行为，使用共享状态效果/Combat 组件，不建立 Player Buff 组件。

Player 不再增加 `PlayerBuffStateComponent` 作为 Buff 权威状态；需要属性或 Buff 能力时复用
`AttributeSetComponent`、`BuffListComponent`、`BuffImmunityComponent` 和共享的 Buff 接收策略。
`PlayerResourceStateComponent` 只保存 Mana 等真正由 Player 拥有的资源；弹药是
`InventoryComponent` 中的物品堆或武器自身的 Ammo inventory，不复制到 Player 资源组件。

MinionCapabilityComponent、SentryCapabilityComponent 只有接入召唤物时再添加。

**Player 专属领域系统**

PlayerResourceSystem、ProtectionWindowSystem、PlayerDeathRespawnSystem、PlayerTargetingSystem。

`PlayerResourceSystem`、`ProtectionWindowSystem`、`PlayerDeathRespawnSystem` 和 `PlayerTargetingSystem` 已提供 Arch
适配实现。MovementIntent 由宿主直接提交给共享空间系统；Player Combat 和交互请求仍需要
宿主输入、Combat owner 和 World/Inventory 提交协议后接入，不创建没有实际闭包的占位系统。

装备关系、装备属性贡献和属性修正不由 Player 私有系统拥有，而由共享链路处理：

```text
InventorySystem 提交物品/关系
→ Inventory/Item 属性贡献重建
→ AttributeSetComponent / AttributeModifier
→ Combat、Movement 或 Resource 系统消费
```

数值 Buff/Debuff 由 `AttributeSystem` 管理其 `AttributeModifier` 生命周期；非数值状态效果
需要免疫、刷新、叠层或周期行为时，再由共享 Status/Combat 系统处理，Player 领域不复制一套
Buff 系统。

已有 Inventory、AttributeSet、Life、Oxygen、Location、Velocity、Gravity、AI 和 WorldInteraction
可复用；Player 领域的专属状态和四个基础系统已有共享实现，完整行为接入仍缺失。客户端控制、
原始输入和瞄准不进入实体组件；P0 使用宿主提供的
已验证 MovementIntent → spatial commit。

### 4.7 Combat 伤害、治疗、击退、免疫和状态效果

**入口证据**

Projectile 命中和 Kill、NPC.StrikeNPC、Player.OnHit、KillMe、UpdateLifeRegen、UpdateImmunity、
Update_NPCCollision 和 Buff/装备影响。

**缺少的原子组件**

DamageProfileComponent、DamageAdmissionPolicyComponent、DamageMitigationStateComponent、
HitImmunityStateComponent、InvincibilityStateComponent、KnockbackStateComponent、
StatusEffectStateComponent、DeathDispositionStateComponent、DamageRevisionComponent、
CombatContributionComponent（多人归属和 credit 需要时）。

已有 Life、Knockback、HealthRegeneration、AttributeSet、BuffList 和 BuffImmunity
覆盖输入或部分状态；Projectile 命中的原子提交和失败语义由 Damage* 与
`ProjectileCombatCommitSystem` 局部覆盖，NPC/Player 的完整通用战斗规则仍未闭合。

**已实现的局部系统**

`DamageAdmissionSystem`、`DamageMitigationSystem`、`DamageCommitSystem` 和
`ProjectileCombatCommitSystem` 已形成 Projectile 候选的局部命中提交闭环；其中
`DamageCommitSystem` 是目标 `LifeComponent` 的唯一常规写入者，Projectile 免疫会在 Life
提交前检查，避免同一 Projectile 在免疫窗口内重复扣血。

**缺少的通用领域系统**

通用请求入口、`KnockbackSystem`、`HitImmunitySystem`、`StatusEffectSystem`、
`LifeRegenerationSystem`、`DeathDispositionSystem`、`CombatContributionSystem`，以及
NPC/Player 的攻击规则和完整 Combat 调度仍缺失。

DamageCommitSystem 必须成为目标 Life 的唯一写入者。Projectile、NPC、Player 只能产生候选
或意图，不能直接修改另一领域的 Life。优先级 P0。

### 4.8 World Tile、WorldGen、TileEntity、Wiring 和持久化

**入口证据**

WorldGen.GenerateWorld、UpdateWorld、PlaceTile、KillTile、SolidTile、InWorld；
Wiring；Chest.DestroyChest、UpdateChestFrames；WorldSections。

**建议的原子组件或世界状态**

WorldTileRevisionComponent、WorldGenerationStateComponent、WorldTileInteractionQueueComponent、
WorldSectionDirtyStateComponent、TileEntityAnchorComponent、ChestContainerRelationComponent、
WireSignalStateComponent、WorldPersistenceIdentityComponent。

世界不应把每个 Tile 做成 ECS 实体。Tile map、WorldFile、液体缓冲和区段数据应留在专用世界
存储，ECS 只保存请求、能力和投影。

**缺少的领域系统**

WorldGenerationSystem、TileStateQuerySystem、TileMutationCommitSystem、TileEntityLifecycleSystem、
ChestContainerSystem、WireSignalSystem、WorldSectionStreamingSystem、WorldPersistenceSystem。

现有 WorldInteractionRequest/Result、WorldTileCoordinate/State、Inventory、Item 只能复用为
边界合同，不能证明 WorldGen、TileEntity、Chest、Wire、Section 和 Save/Load 已接入。

### 4.9 Liquid

**入口证据**

Liquid.Update、Liquid.UpdateLiquid、Liquid.NetSendLiquid、WorldGen.AnyLiquidAt、液体与 Tile
联动。

**已有原子组件**

LiquidComponent、LiquidContactComponent、LiquidContactFlags、LiquidInteractionCapabilityComponent。

**缺少的原子组件**

WorldLiquidStateComponent、LiquidFlowWorkStateComponent、LiquidEffectStateComponent、
LiquidRevisionComponent。

**缺少的领域系统**

LiquidSimulationSystem、LiquidContactResolutionSystem、LiquidTileInteractionSystem。

共享层当前只有接触输入，没有液体流动、Tile 变化、死亡规则和网络投影。Projectile 只消费
接触事实，并按定义解释自身效果。

### 4.10 Item、WorldItem、Inventory、Recipe 和 Chest

**入口证据**

Item.SetDefaults、ResetStats、ResetPrefix；WorldItem.UpdateItem、SyncItem、SpawnShimmeredItem；
Recipe.UpdateRecipeList、ConsumeOwnedItem；Chest.DestroyChest、UpdateChestFrames；
Player.ConsumeItem、PlaceThing、DropItems。

**已有原子组件**

ItemDefinition、ItemStack、ItemStackComponent、ItemWorldStateComponent、Inventory、
ContainedInInventory。

**缺少的原子组件**

ItemPrefixComponent、ItemUseStateComponent、ItemAmmoCapabilityComponent、
ItemDurabilityComponent、RecipeBookComponent、
CraftingStationCapabilityComponent、ChestContainerRelationComponent、WorldDropOwnershipComponent。

装备关系不新增 `ItemEquipmentRelationComponent`：物品是否位于武器、护甲或饰品位置由
`InventoryComponent` 的槽位定义与 `ContainedInInventoryComponent` 的归属关系共同表达。
`InventoryAttributeContributionSystem` 只把位于 `Armor`、`Accessory` 或 `Weapon` 槽位的
物品贡献写入 `AttributeSetComponent`；普通背包、Ammo 和 Chest 槽位中的物品不会产生装备属性。

**缺少的领域系统**

ItemDefinitionSystem、ItemWorldLifecycleSystem、ItemPickupSystem、InventoryAttributeContributionSystem、
AmmoConsumptionSystem、RecipeSystem、CraftingSystem、ChestSystem、WorldDropProjectionSystem。

InventorySystem 已有跨实体转移不变量；`InventoryAttributeContributionSystem` 已提供从槽位到
`AttributeModifier` 的共享重建路径，但仍没有 Player 输入、武器使用、弹药消耗、
配方、箱子和世界掉落接入。

### 4.11 网络、协议和投影（本次范围外）

**入口证据**

MessageBuffer.GetData、Packet 27/28/29/142；NetMessage.SendData、SendSection、
SyncConnectedPlayer、SyncDisconnectedPlayer；RemoteClient、Netplay。

**建议的边界状态**

NetworkSessionComponent、NetworkReplicationStateComponent、ProtocolIdentityProjectionComponent、
InboundCommandStateComponent、OutboundProjectionStateComponent。

网络 DTO 不应直接成为游戏组件。上述状态只表达连接、版本、去重、投影和 ack，不表达玩法
位置、生命或穿透。

**缺少的领域系统**

NetworkSessionSystem、InboundCommandValidationSystem、ProtocolIdentityResolutionSystem、
ReplicationSnapshotSystem、OutboundProjectionSystem、NetworkRetryAndAckSystem。

网络只能把 wire packet 转成 validated command，不能直接写 Location、Velocity、Life、
Projectile penetration 或 NPC 状态。当前 NSSLC 没有 Projectile Packet 27/29/142 适配器；
这部分记录客户端差距，但本次最小闭环不实现也不依赖它。

### 4.12 Mount、Minecart、Minion、Sentry 和特殊关系

客户端有 Mount、Minecart、Minion、Sentry、Whip、Trap、Boss 从属和变身行为。只有在内容
范围需要时，才增加 MountCapabilityComponent、MountStateComponent、MinecartCapabilityComponent、
MinecartTrackStateComponent、MinionCapabilityComponent、MinionOwnerComponent、
SentryCapabilityComponent、SentryLifetimeComponent、ParentEntityReferenceComponent、
ChildEntityCollectionComponent、TransformationStateComponent，以及相应 MountSystem、
MinecartMovementSystem、MinionOwnershipSystem、SentryLifecycleSystem、EntityRelationshipSystem、
TransformationSystem。

这些不是普通 Projectile 垂直切片的 P0，不能因为客户端有对应类就预建空目录或空组件。

## 5. 客户端主要系统到 NSSLC 映射

| 客户端主要系统 | 主要入口 | 需要的 NSSLC 原子组件 | 所需领域系统 | 当前状态 |
| --- | --- | --- | --- | --- |
| 主循环/世界时间 | Main.UpdateServer、UpdateTime | WorldSession、WorldTime、WorldEvent | Session、Clock、Event | 缺失 |
| 世界天气/环境 | Main.UpdateWeather、Rain、SceneMetrics | Weather、Environment、Event | Weather、Environment、SceneMetrics | 缺失 |
| Entity lifecycle | Spawn/Kill、Main 槽位 | Identity、Lifecycle、Revision、Owner/Target | Lifecycle、Hydration、Cleanup | 部分 |
| 通用空间 | Collision、SolidTile、实体 Update | Location、Velocity、Collider、Grounding、能力 | Movement、TileSweep、Broadphase | 部分 |
| Projectile | NewProjectile、Update、AI、Kill | Projectile 基础组件与按需能力组件 | Lifecycle、Simulation、Collision、Hit、Projection | 生命周期/候选/命中提交已实现；宿主 Tick 接入仍 partial |
| NPC | SpawnNPC、AI、UpdateNPC、StrikeNPC | NPC 状态组件和共享空间/战斗 | Spawn、Target、AI、Movement、Combat、Death | 缺失 |
| Player | Update、UpdateEquips、Spawn、KillMe | Player 专属状态、Inventory、Attributes、共享 Combat | Inventory、Attributes、Movement、Combat、Respawn | 部分：状态、资源和死亡/目标清理已实现 |
| Combat | Strike、OnHit、LifeRegen、Immunity | Damage、Mitigation、Immunity、Status、Death | Admission、Mitigation、Commit、Status、Death | Projectile 局部命中/Life/穿透闭环；通用 Player/NPC 规则仍缺失 |
| World Tile | PlaceTile、KillTile、SolidTile | world revision、tile request、section | Tile Query、Mutation、Section、Persistence | 仅交互端口 |
| Liquid | UpdateLiquid、AnyLiquidAt | liquid state、flow、effect | Liquid、Contact、Tile interaction | 仅接触组件 |
| Item/Inventory | Item、WorldItem、Recipe、Chest | use/prefix、Inventory 物品关系、ammo/recipe/chest | Item、Pickup、Inventory、Attributes、Crafting、Chest | 基础转移 |
| Mount/Minion/Sentry | Mount、Minecart、召唤物 | capability、owner/parent、lifetime | Mount、Relationship、Minion/Sentry | 缺失/可延后 |

## 6. 推荐新增顺序

### P0：形成一条权威主循环

1. EntityLifecycleSystem：构造、终止、generation、revision、失败清理；
2. WorldSessionLifecycleSystem：最小 world scope、epoch、clock 输入；
3. SpatialSimulationCoordinatorSystem 和 Tile sweep 适配；
4. 由宿主提交已验证的 MovementIntent，并接入基础 Player 移动；
5. NPC 最小 Definition、Target、AI intent 和基础生命；
6. Combat DamageAdmission、Mitigation、Commit、Death；
7. Projectile Lifecycle、Simulation、Collision、Hit、Penetration；
8. Item world drop、基础拾取和弹药消耗。

P0 垂直验收路径：

Host-provided movement intent
→ spatial commit
→ Player spawn ordinary Projectile
→ Projectile motion
→ Tile/entity collision
→ accepted hit
→ NPC Life commit
→ Projectile penetration/termination
→ local committed-state observation

### P1：补齐大系统持续行为

AttributeModifier 生命周期与来源重建；需要非数值 StatusEffect 时再处理其周期和免疫；Inventory 物品使用/关系提交；NPC spawn director、
区域刷怪和 Boss phase；World clock/weather/event/liquid；Item use/ammo/recipe/crafting/chest；
World section streaming、TileEntity 和 persistence。网络会话、协议验证、复制和 ACK 属于未来
独立适配层，不是本地最小闭环的前置条件。

### P2：按内容范围增加

Mount、Minecart；Minion、Sentry、Whip、Trap；复杂反射和特殊 Projectile AI；完整 Wire/Actuator；
客户端表现、动画、灯光、Dust、Gore、UI、地图渲染。

## 7. 不能照搬客户端的系统形状

| 客户端形状 | NSSLC 设计 |
| --- | --- |
| Main.projectile/npc/player 数组 | ECS 组件组合 + EntityUuid + runtime handle + typed projection |
| Projectile.Update 同时 AI、移动、碰撞、伤害、网络 | Simulation、Collision、Combat、Lifecycle、Projection 分开 |
| NPC.StrikeNPC 直接改 NPC 和附带状态 | Damage candidate → admission → Life/Status commit |
| Player.Update 包含输入、装备、移动、碰撞、表现 | Host command → Inventory/Attributes → Intent → Spatial → Combat |
| MessageBuffer.GetData 解析包并改全局状态 | Network adapter → validated command → authoritative owner |
| WorldGen 同时读写 Tile、掉落、声音、网络 | World query/commit port + Drop/Network projections |
| 静态 perIDStaticNPCImmunity | generation-scoped immunity state |
| 固定整数 owner/identity/slot | EntityUuid、RuntimeHandle、owner epoch、protocol projection 分层 |

## 8. 证据和未决问题

### 已确认

- NSSLC 有组件库基础，主要集中于共享能力、规则计算和局部适配器；
- AttributeTickSystem、GravityTickSystem、WorldInteractionSystem 是明确的 Arch Tick 适配器；
- 空间、碰撞、库存、定义和身份合同存在；
- Projectile 已有正式 Share 领域目录；Player 专属状态目录和基础系统已实现，NPC 仍只有行为适配器；
- 最新客户端主要权威行为集中在 Main、NPC、Projectile、Player、WorldGen、Liquid、Item、
  WorldItem、MessageBuffer 和 NetMessage。

### Partial

- AI 能计算快照到意图，但没有通用 EcsServer AI Tick；
- SpatialSimulation、SpatialCommit、EntityCollision 能独立工作，但没有生产 Tick 链；
- InventorySystem 能执行同步转移，InventoryAttributeContributionSystem 能重建物品属性贡献；
  Player 的资源、死亡/重生和目标清理已接入 Arch，但仍没有完整 Player/Persistence 接入；
- WorldInteractionSystem 能处理请求，但没有完整 Tile/WorldGen/Section/TileEntity owner；
- Projectile 的候选、准入、减伤、Life 提交、共享保护窗口、Attributes 贡献和穿透收尾已有 Arch smoke
  证据；NPC/Player 通用攻击入口、击退、状态效果、死亡处置和完整宿主 Tick 仍未接入。

### Unknown / evidence gap

- 当前工作树恢复后完整 NSSLC 编译和运行闭包；
- Arch component capture/store 的所有生产宿主入口；
- Tile sweep、空间 broadphase 和世界 revision 的生产实现；
- Combat accepted-hit 与 Projectile penetration 的跨 owner 冲突恢复；
- 最新客户端所有 AI profile、Boss phase、特殊 Projectile 内容的行为等价范围；
- 当前项目必须支持的 WorldGen、液体和 Wiring 规则。

本盘点不把文件名、类型数量、局部编译或客户端方法存在等同于运行时完成。实现新增组件或
系统前，应先锁定 P0 垂直路径和对应读写/提交证据。





