# Version4 权威游戏模拟系统主要子系统

> 本文是对《[Version4 权威游戏模拟系统拆分设计报告](Version4权威游戏模拟系统拆分设计报告.md)》的主干提炼，用于快速理解目标架构的主要职责域。
>
> 它不是源码迁移完成报告，也不表示这些系统已经编译、运行或通过回归验证。

## 1. 总体定位

Version4 的目标不是把 `Main`、`Player`、`NPC` 等大类机械地改名为 ECS 组件，而是建立一个实时、持久化、服务器权威的多人沙盒游戏模拟运行时。

其数据根、行为层和外部边界应保持如下关系：

```text
WorldSession + WorldStorage + ContentCatalog
        ↓
领域 System / Query / Command
        ↓
Network / Persistence / Presentation Projection
```

- **组件/State**：只拥有内聚的权威状态。
- **System**：执行确定性的状态转换。
- **Query**：只读地计算资格、几何、规则或派生结果。
- **Command**：表达跨实体或结构性的权威变更，并在规定阶段提交。
- **Adapter/Projection**：隔离网络、存档和客户端表现，不得直接写入权威玩法状态。

## 2. 主要子系统

| 子系统 | 核心职责 | 关键边界与典型内容 |
| --- | --- | --- |
| **1. 运行时编排与 Tick 调度** | 定义每个 Tick 的阶段、暂停、世界就绪屏障和结构变更提交时机。 | `RuntimeComposition`、`SimulationScheduler`、`WorldUpdateBarrier`、`MainThreadCommandQueue` |
| **2. 世界会话与进度** | 保存世界级规则、时间、天气、事件、难度、特殊种子、Boss 与入侵进度。 | `WorldSession`、`WorldRulesState`、`WorldTimeWeatherState`、`WorldEventProgressState`、事件时钟、生态扩散、入侵积分 |
| **3. 世界存储与身份** | 管理 Tile、Wall、液体、TileEntity、实体槽位、容器与区段；区分槽位、网络 ID、持久化 ID 和实体身份。 | `WorldStorage`、`TileMapStore`、`TileEntityStore`、`EntitySlotStore<T>`、`ContainerStore`、`WorldSectionState` |
| **4. 内容定义目录** | 提供只读的物品、NPC、投射物、Buff、Tile、Wall、配方和掉落规则定义；不保存运行时实例状态。 | `ContentCatalog`、`ItemDefinitionCatalog`、`NpcDefinitionCatalog`、`ProjectileDefinitionCatalog`、`RecipeAndDropCatalog` |
| **5. 命令入口、输入与关系验证** | 接收玩家输入和网络命令，验证序号、权限、距离、频率、队伍/PvP/创造权限，再形成意图。 | `MovementIntent`、`InteractionIntent`、`ControlRateState`、`TeamAndPvpRelations`、`CreativeWorldMutation` |
| **6. 空间、移动、碰撞与液体** | 将玩家、NPC 和投射物意图转为权威位置、速度与接触结果；处理 Tile、斜坡、门、矿车与液体。 | `MovementPhysics`、`Collision` Query、`LiquidFlow`、`MountAndMinecart`、`DoorTraversalAndTileMutation` |
| **7. 世界交互与结构变更** | 处理 Tile 放置/破坏、重框架、压力板、电线、逻辑门、机关和 TileEntity 生命周期。 | `TilePlacementAndFraming`、`WiringInteraction`、`PressurePlateAndAnchors`、`TileEntityRuntime` |
| **8. 玩家玩法与能力** | 管理玩家生命、魔力、Buff、装备、物品使用、库存、死亡/复活、出生、睡眠、坐骑、钓鱼和召唤容量。 | `PlayerGameplay`、`PlayerSpawnAndRespawn`、`PlayerRestAndStacking`、`FishingSimulation`、`MinionAndSentryCapacity` |
| **9. NPC 模拟与城镇** | 管理 NPC 刷新、AI、目标、战斗、生命周期、城镇住房、幸福度、对话、服务和任务。 | `NpcSpawnAiTown`、`TownEconomyAndPersonality`、`NpcInteractionAndCommerce` |
| **10. 投射物与专用实体行为** | 管理投射物归属、轨迹、行为、伤害、穿透、命中免疫与寿命；容纳领域系统选择性使用的专用行为状态。 | `ProjectileBehavior`、`LeashedEntitySimulation` |
| **11. 战斗、状态与归因** | 处理伤害资格、减伤、治疗、击退、免疫、Buff、死亡和伤害来源归因。 | `CombatAndStatus`、`DamageResolutionSystem`、`DeathResolutionSystem`、`RandomnessAndAttribution`、Boss/入侵伤害贡献 |
| **12. 物品、容器与经济事务** | 管理物品实例、堆叠、背包、装备、世界掉落、箱子、银行、商店、配方、制作、交易和掉落提交。 | `ItemGameplay`、`WorldObjects`、`InventoryTransferTransactions`、`CommerceTransactionSystem`、`LootResolutionSystem` |
| **13. 世界生成、Biome 与生态** | 将新世界创建、加载、地形/地牢/结构生成、Biome 查询、城镇住房、环境扫描和生态扩散与常规 Tick 分离。 | `WorldGenerationAndBiomes`、`TerrainGenerationSystem`、`BiomeRuleQuery`、`SceneSensing`、`WorldEcologyAndSpread` |
| **14. 生命周期与外部边界** | 处理生成、销毁、掉落、清理、复制和存档；网络与客户端表现只能读取权威状态。 | `SpawnLifetimeLoot`、`NetworkSessionAndReplication`、`PersistenceProjection`、`ClientPresentationProjection` |
| **15. 旅行与传送** | 处理端点网络、旅行资格、落点、位置迁移、冷却和旅行结果复制。 | `TeleportationAndTraversal`、Pylon/Portal registry、landing Query、travel Command |
| **16. 钓鱼与渔获** | 处理钓鱼能力与环境资格、浮标时序、渔获决策及物品/NPC 结果提交。 | `FishingAndCatchSimulation`、FishingAttempt、Catch command |
| **17. 世界日历与事件编排** | 推进日夜、天气、随机资格和活动实例，并向 NPC、刷怪和生态发布事件事实。 | `WorldCalendarAndEventOrchestration` |
| **18. 世界进程与转换** | 协调 Hardmode 等长期世界转换，管理计划、提交屏障、世界规则变化和区段重同步。 | `WorldProgressionAndTransition` |
| **19. 模拟规则覆写** | 将 Journey/Creative 权限收敛为时间、天气、难度、生态和刷怪使用的只读规则快照。 | `SimulationRuleOverrides` |
| **20. 世界知识与解锁** | 保存会影响权威资格的长期知识和解锁进度，而非成就 UI 或社交投影。 | `WorldProgressionAndUnlocks` |

## 3. 推荐的数据流

```text
客户端输入 / 网络请求
        ↓
命令验证与意图
        ↓
世界规则、日历事件、实体 AI、交互/旅行/钓鱼资格 Query
        ↓
移动/碰撞/液体 → 战斗/状态 → 物品/交易/掉落
        ↓
Spawn、Despawn、Tile、TileEntity、容器等 Command 提交
        ↓
WorldStorage 中的唯一权威状态
        ↓
网络复制 / 世界存档 / 客户端表现投影
```

## 4. 必须保持的架构边界

### 4.1 世界状态与实体状态分离

- `downedBoss`、入侵和世界事件属于 `WorldEventProgressState`，不能附着到某个 NPC 实例。
- `TileMap` 属于世界存储，不能伪装为实体组件。

### 4.2 内容定义与运行时实例分离

- 物品、NPC、投射物的默认参数属于 `ContentCatalog`。
- 当前生命、堆叠、AI 进度、寿命、前缀和归属属于实例状态。

### 4.3 意图、资格与结果分离

- 输入或 AI 只能表达移动、攻击或交互意图。
- Query 决定意图是否合法或可否执行。
- 位置、伤害、物品转移和世界变更只能由 System/Command 写入。

### 4.4 权威状态与外部投影分离

- 网络包、存档 DTO、UI、动画、粒子、音频、镜头和轨迹表现历史都不是权威玩法状态。
- 网络发送失败、客户端视野变化或渲染节流不得反向改变生命、位置、目标、寿命或容器内容。

## 5. 实施骨架的建议优先级

若需要从设计进入实施，先建立以下六个骨架子系统，再逐步接入细分领域能力：

1. **`RuntimeComposition`**：固定 Tick 顺序与 Command 提交点。
2. **`WorldSession`**：承载世界规则、时间、事件与长期进度。
3. **`WorldStorage`**：成为 Tile、实体、TileEntity 和容器的唯一权威写入根。
4. **`ContentCatalog`**：彻底分离定义数据与实例状态。
5. **`MovementPhysics + CombatAndStatus`**：建立跨实体规则层的核心闭环。
6. **`NetworkSessionAndReplication + PersistenceProjection`**：将网络和存档固定为权威状态的边界投影。

随后按风险逐步接入 Player、NPC、Projectile、物品/交易、世界生成和复杂世界交互；迁移期间应仅允许单向适配，不允许新旧权威字段双写。

## 6. 来源与状态说明

- 详细证据、候选组件、System/Query/Command 以及每项的 `confirmed`、`partial`、`missing` 状态，见《[Version4 权威游戏模拟系统拆分设计报告](Version4权威游戏模拟系统拆分设计报告.md)》。
- “玩家玩法与能力”第 8 项的文件级证据闭包、组件所有权、System/Query/Command 边界和迁移 verifier，见《[Version4 玩家玩法与能力系统组件拆分报告](Version4玩家玩法与能力系统组件拆分报告.md)》。
- 该设计报告明确说明：尚未完成字段级读写者闭合、focused verifier、编译验证或运行时回归。因此本文不得被用作“系统已实现”的证明。
- 本轮新增六个子系统的 Version4/tModLoader 交叉证据与明确排除项，见《[Version4 与 tModLoader 扩展面补充子系统审查](research/2026-09-05-version4-tmodloader-additional-authoritative-subsystems.md)》和《[tModLoader 与 Version4 子系统补充审查：旅行与传送](research/2026-09-05-tmodloader-travel-and-subsystem-boundaries.md)》。
