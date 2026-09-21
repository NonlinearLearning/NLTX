# Version4 权威游戏模拟子系统审查报告

## 1. 结论

`NLTX` 当前已经建立了若干有意义的领域状态边界，但还不是一个可运行的“权威游戏模拟
子系统集合”。现状更准确地说是：**按领域组织的状态模型和少量局部规则，正在为权威模拟
运行时准备数据边界**。最缺少的不是再增加组件，而是把现有领域接入同一套运行时编排、
提交、复制和持久化边界。

本次初次整理建议采用下列五层、二十个初始责任面作为后续讨论的稳定粒度。它们按独立职责、
权威状态所有权、输入输出和生命周期划分；不是类、字段或单个 ECS 组件的清单。

```text
1. 运行时编排
   RuntimeComposition

2. 权威数据根与世界规则
   WorldSession | WorldCalendarAndEventOrchestration
   WorldProgressionAndTransition | WorldProgressionAndUnlocks
   SimulationRuleOverrides | WorldStorage | ContentCatalog

3. 命令入口与世界交互
   IntentAndInteraction

4. 领域模拟
   SpatialSimulation（MovementPhysicsAndLiquid + WorldInteractionAndStructures）
   PlayerGameplay | NpcAndTownSimulation | ProjectileSimulation
   TeleportationAndTraversal | FishingAndCatchSimulation
   CombatAndStatus | ItemContainerAndEconomy
   WorldGenerationAndEcology | SpawnLifecycleAndLoot

5. 外部边界
   ExternalBoundaries（Network/Replication + Persistence + ClientPresentation）
```

前四层决定权威游戏结果；第五层只读取已提交的权威状态。这里的“二十个”是初始管理
边界，不是迁移待办数量，也不意味着需要创建二十个项目或二十个目录。

## 2. 审查范围与判定方法

### 2.1 资料范围

本报告综合下列本地一手材料：

| 材料 | 用途 | 限制 |
| --- | --- | --- |
| `D:\TRbackup\Version4` | 原始 Terraria 服务器实现和实际调度/存储/协议证据 | 旧式聚合实现；不能把静态字段位置等同为目标边界 |
| `docs/Version4权威游戏模拟系统拆分设计报告.md` | 既有的权威模拟拆分设计和证据索引 | 报告包含后续细查的细粒度候选，不应直接当作初次子系统目录 |
| `src/` 与 `Test/` | NLTX 当前代码、项目依赖和已有 focused verifier | 当前主要实现状态模型，不能据此声称已形成完整运行时 |
| `D:\TRbackup\tmodloader-api-docs-stable` | tModLoader 生命周期、同步和存档扩展点的边界交叉验证 | API 文档不替代 Version4 的实际行为证据 |

报告只分析服务器权威玩法和持久化世界结果。渲染、UI、音频、相机、粒子以及单一客户端
视角不作为权威子系统状态。该范围与既有拆分设计的权威范围和排除范围一致，见
[`Version4权威游戏模拟系统拆分设计报告.md`](Version4权威游戏模拟系统拆分设计报告.md)。

### 2.2 子系统成立标准

初次报告仅在同时具备或明确需要下列多数条件时才列出一个子系统：

1. 有独立的业务责任和稳定的状态生命周期。
2. 能指出权威状态的唯一写入根，或说明目前该写入根缺失。
3. 与相邻领域之间能以命令、事件、只读查询或快照交接。
4. 能独立定义最小 verifier，而不是只能通过全局 `Main` 行为间接验证。

因此，`Velocity`、`BuffSlot`、某一 NPC AI style、某一 TileEntity 类型、Tile 位字段和单个
网络消息都不是本报告的一级子系统。它们只在证明某个子系统边界时作为例子出现。

## 3. 参考实现的架构事实

### 3.1 Version4 证明需要显式运行时编排

Version4 的旧入口已经显示出一条实际依赖链：`Main.DoUpdate` 先在主线程处理网络，随后
推进天气和世界更新，并由世界准备状态和生成状态决定是否更新实体
（`D:\TRbackup\Version4\Terraria\Main.cs:11244-11258`、`:11344-11409`）。世界更新中，
玩家更新、游戏计数和刷怪等路径又有固定前后关系（`:11420-11472`）。

这证明目标需要一个显式的 Tick 阶段表，但**不**证明 Version4 当前循环就是可直接复制的
目标顺序。既有设计报告给出的目标顺序是：命令入口、世界会话、刷怪/AI、玩家、物理、
Tile/电线、投射物、战斗、物品、生命周期/掉落、提交、复制、持久化。该顺序应由
`RuntimeComposition` 所有，而不应由目录或文件枚举顺序暗示。

### 3.2 版本兼容约束要求保留受控存储门面

Version4 把 Tile、Player、NPC、Projectile 和世界物品放在固定数组中
（`D:\TRbackup\Version4\Terraria\Main.cs:928-946`），初始化时将 `whoAmI` 设为槽位索引
（`:3465-3484`）。所以，槽位、网络 ID、持久化 ID 和领域实体身份不能混为一种标识。
初期应以 `WorldStorage` 门面和提交队列保护这些实现细节，而不是立即把固定槽位拆为许多
组件或替换底层协议格式。

入站网络同样不是第二权威。`MessageBuffer.GetData` 在分派前检查包号、连接阶段和握手状态
（`D:\TRbackup\Version4\Terraria\MessageBuffer.cs:125-178`）；目标结构应把它转换为经过
验证的命令或意图，再由模拟阶段写入世界。

### 3.3 tModLoader 只用于校验外部边界

本地 API 文档说明 `ModSystem.PreUpdateEntities` 位于玩家、NPC、投射物和 Tile 更新之前，且
仅在完整更新帧调用（`class_mod_system.html:1020-1044`）。`ModSystem.NetSend` 是服务器向
客户端发送世界数据的入口，`NetReceive` 是客户端收到世界数据后的入口
（`class_mod_system.html:839-896`）；`SaveWorldData` 用于保存世界特有进度，例如 Boss 击败
后解锁的世界事实（`:1078-1105`）。这些扩展点支持“权威状态向网络/存档投影”的方向，
但不能作为 Version4 行为已实现或可兼容的证明。

## 4. 目标子系统总表

| 层级 | 子系统 | 核心责任 | 权威写入根 | 当前 NLTX 状态 |
| --- | --- | --- | --- | --- |
| 运行时 | RuntimeComposition | Tick 阶段、准备门控、命令提交和失败可见性 | 尚缺少统一根 | 缺口明显 |
| 数据根 | WorldSession | 世界规则、时间天气、长期进度和生成准备 | 世界会话状态 | 已有较多状态模型 |
| 数据根 | WorldCalendarAndEventOrchestration | 日夜时钟、事件资格、事件实例和跨领域事件提交 | 世界日历与事件状态 | 状态模型存在，调度器缺失 |
| 数据根 | WorldProgressionAndTransition | Hardmode 等长期世界转换的计划、提交屏障与区段重同步 | 世界转换事务 | 缺少执行链 |
| 数据根 | WorldProgressionAndUnlocks | 影响权威资格的长期知识/解锁进度 | 世界进度存储 | 缺少模型 |
| 数据根 | SimulationRuleOverrides | Journey/Creative 的权限校验和只读规则快照 | 规则覆写状态 | 缺少模型 |
| 数据根 | WorldStorage | Tile、区段、槽位、容器、TileEntity 的唯一可变存储 | `WorldStorageRoot` | 已有骨架，写接口未闭合 |
| 数据根 | ContentCatalog | 只读定义、ID 映射、配方和掉落规则 | 经验证的内容快照 | 现有基础最完整 |
| 入口 | IntentAndInteraction | 输入/网络命令验证及 Tile、线路交互意图 | 命令入口和交互提交 | 只有局部状态，未形成入口层 |
| 模拟 | SpatialSimulation | 位移、碰撞、接触、液体流动、放置、破坏、电线、机关和 TileEntity 交互 | 模拟阶段，经 WorldStorage 提交 | 状态较多，规则/查询不足；空间与结构写集仍需分开 |
| 模拟 | PlayerGameplay | 玩家资源、装备、使用、出生/死亡/复活、能力 | 玩家实体状态 | 模型存在，编排缺失 |
| 模拟 | NpcAndTownSimulation | NPC 生成、AI、目标、生命周期、住房和城镇规则 | NPC 状态和世界进度命令 | 模型存在，System 缺口 |
| 模拟 | ProjectileSimulation | 归属、轨迹、命中、寿命和专用投射行为 | Projectile 状态 | 状态模型起步，规则缺口 |
| 模拟 | TeleportationAndTraversal | 端点、资格、落点、位置迁移、冷却和旅行结果复制 | 旅行提交 | 有状态模型，执行链未闭合 |
| 模拟 | FishingAndCatchSimulation | 钓鱼资格、浮标时序、渔获决策和 Item/NPC 结果提交 | 钓获提交 | 有能力/规则模型，状态机缺失 |
| 模拟 | CombatAndStatus | 伤害资格、结算、免疫、死亡、归因和状态效果 | 战斗结算命令 | 有局部可验证规则 |
| 模拟 | ItemContainerAndEconomy | 物品实例、背包、容器、制作、商店、交易和掉落结果 | 物品/容器事务 | 模型较多，原子事务缺失 |
| 模拟 | WorldGenerationAndEcology | 生成、Biome、住房扫描、生态传播和世界就绪 | 生成计划/世界存储提交 | 世界状态存在，执行链未形成 |
| 模拟 | SpawnLifecycleAndLoot | 生成、销毁、回收、掉落和清理的跨领域提交 | 统一结构变更提交 | 尚未形成独立闭环 |
| 边界 | ExternalBoundaries | 网络会话/复制、世界/玩家存档、客户端表现三个单向适配面 | 读取已提交状态；加载只经受控恢复入口 | 顶层项目/实现均缺失 |

表中二十个名称是初始责任面；“ExternalBoundaries”内部仍要保持网络、持久化和表现三个
不同写集/生命周期的适配面，但不要求初次就拆成三个独立程序集。`SpatialSimulation` 内部
同理：空间物理和世界结构交互共享 TileMap，却必须保持各自的查询与写集。

## 5. 子系统审查

### 5.1 RuntimeComposition：当前最重要的架构缺口

**责任。** 它拥有阶段顺序、暂停/快进决策、世界准备屏障、命令缓冲及提交时机；不拥有
Player、NPC、Tile 或内容定义。

**应有交接。** 入站 Adapter 产生已验证命令；各领域 System 只读取声明的上下文并向
CommandBuffer 写入；阶段末由一个提交器把 Tile、槽位、容器和 TileEntity 的结构变化写入
`WorldStorage`；最后才生成复制和存档快照。

**当前证据。** `src/` 中仅有 `DamageResolutionSystem` 和 `DeathResolutionSystem` 等局部静态
结算器，没有 `ISimulationSystem`、调度器、统一命令缓冲或跨领域阶段表。现有
`Test/Terraria.Combat.Verification/Program.cs` 能验证局部战斗规则，但不能验证完整 Tick
顺序。因此不要以新增更多组件替代本子系统的建立。

### 5.2 WorldSession、WorldStorage、ContentCatalog：应先稳固的三类数据根

这三者不可合并，因为其变化原因不同：世界会话随世界进度变化，世界存储随每次世界操作
变化，内容目录应在加载后只读。

`WorldSession` 已有世界描述、规则、时间天气、事件进度和刷怪压力等状态，主要位于
[`src/WorldSession/WorldSessionComponents.cs`](../../../src/WorldSession/WorldSessionComponents.cs)。
`WorldGeneration/` 下又单独定义了世界生成、规则、描述和生态状态。它们证明该领域已被
识别，但也暴露出同一概念的两套类型，例如两个命名空间中的 `WorldDescriptorState`、
`WorldRulesState`、`WorldPreparationState`、`WorldBounds` 和 `OreTierState`。在没有明确
适配关系前，不能让两套类型同时成为权威来源。

`WorldStorageRoot` 已明确汇集 Tile、实体槽位、投射物身份索引、容器、标牌、TileEntity 和
区段状态，见 [`src/WorldStorage/WorldStorageRoot.cs`](../../../src/WorldStorage/WorldStorageRoot.cs)。
`TileMapStore`、`EntitySlotStore` 和 `TileEntityStore` 目前大多只暴露容量、计数和修订号，
尚未暴露受控的 Allocate/Release/Get/Commit 操作。这正适合先补门面和事务边界，不适合先
把 Tile 的压缩表示或槽位数组继续细分。

`ContentCatalog` 已有 Item、NPC、Projectile、Buff、Tile、Wall、配方、掉落、钓鱼规则、
派生索引和身份目录的只读入口，见 [`src/Content/ContentCatalog.cs`](../../../src/Content/ContentCatalog.cs)。
在当前 `src/` 中，这是最接近完整子系统的部分。后续重点是验证内容快照加载/校验后不变，
而不是把每一种 definition record 提升为一个独立子系统。

### 5.3 IntentAndInteraction：必须成为所有外部写入的入口

**责任。** 将客户端输入、网络包、Tile 操作、容器访问和线路触发转化为带来源、序号、权限、
距离和频率信息的意图或命令；它不直接改写生命、位置、Tile 或库存。

**当前证据。** `src/Player/InputIntentComponent.cs` 和
[`src/Physics/MovementIntentComponent.cs`](../../../src/Physics/MovementIntentComponent.cs) 已记录来源、
序号和发出 tick；`WorldInteraction` 中已有压力板、电线、Tile 与 TileEntity 状态。但没有
统一的请求验证、命令类型、授权策略或提交器。应先形成“网络/输入 -> 验证 -> Intent/Command”
通道，再接入更多玩家或机关功能。

### 5.4 MovementPhysicsAndLiquid 与 WorldInteractionAndStructures：同一世界、不同写集

两者均依赖 TileMap，但不能合并。

`MovementPhysicsAndLiquid` 负责实体意图到位置/速度/接触结果的确定性转换，以及 Tile 液体
流动和实体液体接触。`Physics` 已有碰撞策略、碰撞结果、重力、门穿越和移动意图；
`WorldStorage` 已有液体队列、调度、恢复与复制 dirty 集。这些是正确的状态归属信号。
但 `CollisionResultComponent` 直接持有交互层 `TileCoordinate`，`Physics` 项目也引用
`WorldInteraction` 项目，说明当前查询接口仍未独立出来。应以只读 Tile/空间查询收敛该依赖，
而不是把碰撞结果、Tile 液体和实体湿润状态合为一类状态。

`WorldInteractionAndStructures` 负责 Tile 放置/破坏/重框架、压力板、电线、逻辑门、机关和
TileEntity 生命周期。`src/WorldInteraction` 已按 `Tiles`、`Wiring`、`PressurePlates`、
`TileEntities` 组织，这是领域内部按需细分，而不是四个一级模拟子系统。它们应共同经
WorldStorage 提交 Tile/TileEntity/实体生成命令。当前 Tile 交互层自己的 `TileCoordinate`
与 WorldStorage 的 `TileCoordinate` 并存，必须在提交接口处明确转换或收敛，否则坐标身份会
成为跨子系统歧义。

### 5.5 PlayerGameplay、NpcAndTownSimulation、ProjectileSimulation：按实体领域运行，不按类拆散

这三项各自拥有不同的生命周期和规则集合，应保持独立的领域 System，但共享空间、战斗、
内容和存储服务。

- `PlayerGameplay`：`src/Player` 已覆盖身份、生命/魔力、Buff、装备、库存、物品使用、
  出生、休息、坐骑、召唤容量和钓鱼能力状态。它还没有将输入、装备派生、使用、死亡/复活
  与存档/复制组织成完整调用链。
- `NpcAndTownSimulation`：`src/Npc` 已有定义引用、身份、生命、目标、行为、生命周期、父子
  关系与复制 dirty 状态；`src/Town` 补充住房、服务和居民能力。应先建立生成、目标、AI、
  战斗、死亡和城镇住房之间的固定交接，再讨论某个 AI style 的内部拆分。
- `ProjectileSimulation`：当前有定义、方向、寿命、穿透、归属/伤害相关状态，但没有可见的
  投射物更新、命中和销毁调度。它应依赖内容定义、空间 Query、战斗结算和生命周期提交，
  不应把每种专用投射物行为升格为体系结构边界。

### 5.6 CombatAndStatus、ItemContainerAndEconomy、SpawnLifecycleAndLoot：跨实体结果层

`CombatAndStatus` 是当前唯一已有明确局部规则和 verifier 的领域。`DamageEligibilityQuery`、
`DamageResolutionSystem` 与 `DeathResolutionSystem` 在 [`src/Combat`](../../../src/Combat) 中形成了
“资格 -> 结算 -> 死亡结果”的小闭环，`Terraria.Combat.Verification` 覆盖伤害、来源-目标
冷却和死亡归因。下一步应让它接受运行时上下文和命令输出，不能把局部静态规则误称为完整
战斗系统。

`ItemContainerAndEconomy` 汇集物品实例、堆叠、背包、装备、世界掉落、Chest、制作、商店和
交易。`src/Items` 的 `ContainerAccessComponent`、`ContainerContentsComponent`、
`CraftingComponent`、`CommerceLedgerComponent` 和 `WorldItemComponent` 已表达事务所需状态，
但尚未证明原子转移、失败回滚、权限和事件提交。`WorldStorage` 反向引用 Items 的
`ItemState`，因此应特别避免让 Items 又直接接管世界容器数组；以 ContainerStore 的受控事务
接口划清所有权。

`SpawnLifecycleAndLoot` 不是再建一个“实体组件目录”，而是所有 Spawn、Despawn、掉落、
槽位回收和清理的统一提交协议。它连接 Player/NPC/Projectile/WorldItem/TileEntity，但不应
拥有这些领域的长期状态。当前 `NpcLifetimeComponent`、`ProjectileLifetimeComponent` 和
`WorldItemComponent` 各有过期信息，仍缺少同一个结构变更提交点，所以该边界应优先于继续
增加更多生命周期字段。

### 5.7 WorldGenerationAndEcology：与常规 Tick 分离，和 WorldSession 交接

世界生成、加载、Biome、结构放置、住房扫描和生态传播共享世界范围、种子、规则和 TileMap，
但其预算、失败恢复和准备门控与常规实体 Tick 不同。`src/WorldSession/WorldGeneration/` 已有
`WorldGenerationLifecycleState`、生态排程和住房登记；其中 `CanUpdateSimulation` 明确把
准备状态作为普通模拟的门控。这应演化为“生成计划/查询 -> 原子世界提交 -> Ready”的独立
流程，不能把生成步骤混入 NPC 或 Tile 交互 System。

### 5.8 三个外部边界：当前应声明而非伪造实现

`NetworkSessionAndReplication`、`PersistenceProjection`、`ClientPresentationProjection` 在
顶层 `src/` 还没有对应项目。这个缺口必须可见：当前 Npc 的 replication dirty 状态和世界
存储的 liquid dirty 集只能被视作未来投影的输入，不能自行证明存在快照、传输或确认机制。

外部层的硬性方向如下：

```text
验证后的网络命令 ─┐
持久化加载适配器 ─┼─> RuntimeComposition / 领域 System / Command 提交
                  │                         │
                  │                         v
                  └────────────────> WorldSession + WorldStorage
                                            │
              Network replication <────────┼────────> Persistence snapshot
              Client presentation <────────┘
```

网络收包和加载可以在受控入口恢复状态；网络发送、存档写出和表现投影只能读已提交快照，
不得直接写回游戏状态。迁移期尤其禁止新旧字段双写。

### 5.9 六个补充边界：增加责任面，不增加组件清单

本轮查看 Version4 实际源码和 tModLoader 稳定 API 文档后，将初始责任面由十四个调整为二十个。
它们是独立的事务、时钟或规则根，不是把现有组件另起名字：

- `TeleportationAndTraversal`：接收已验证旅行意图，读取端点、环境和世界条件，原子提交位置、
  速度/冷却与旅行结果；Tile/TileEntity 仍由 `WorldStorage` 和世界交互维护。
- `FishingAndCatchSimulation`：统一玩家能力、环境资格、浮标时序、渔获决策和 Item/NPC 结果提交；
  浮标只是其时间载体，不是该子系统本身。
- `WorldCalendarAndEventOrchestration`：从 `WorldSession` 显式分出日夜边界、天气/活动资格和事件
  生命周期，使 NPC、刷怪和生态只消费已提交事件事实。
- `WorldProgressionAndTransition`：从常规生成/生态循环中分出 Hardmode 等长事务式世界转换，负责计划、
  Tile 提交屏障、世界规则改变和区段重同步。
- `SimulationRuleOverrides`：将 Journey/Creative 对时间、天气、难度、生态和刷怪的权限控制收敛为
  每 Tick 可采样的只读规则快照，禁止领域 System 直接操作可变 Power 管理器。
- `WorldProgressionAndUnlocks`：保存会影响权威资格的知识和解锁进度，例如 Bestiary 对城镇 NPC
  生成资格的影响；成就 UI、平台社交和通知仍是投影。

该判断的详细文件级证据、NLTX 状态和排除项见
[`2026-09-05-version4-tmodloader-additional-authoritative-subsystems.md`](../review-round-1/reports/2026-09-05-version4-tmodloader-additional-authoritative-subsystems.md)
与 [`2026-09-05-tmodloader-travel-and-subsystem-boundaries.md`](../review-round-1/reports/2026-09-05-tmodloader-travel-and-subsystem-boundaries.md)。
二者均将 Golf、`SceneMetrics`、单一 TileEntity、协议/Hook 注册、UI、音频和 Social API 保留在
现有子系统或投影边界内。

## 6. 当前架构风险与处理顺序

| 优先级 | 风险 | 审查判断 | 建议处理 |
| --- | --- | --- | --- |
| P0 | 没有统一 Tick/提交根 | 局部组件与规则没有可验证的全局顺序或唯一写入路径 | 先定义 RuntimeComposition、阶段契约和 CommandBuffer/Commit seam |
| P0 | 外部边界缺失 | replication dirty 不等于复制；状态类不等于持久化 | 先定义只读快照与 Adapter 接口，再接具体协议/文件格式 |
| P1 | WorldSession 概念重复 | 生成目录与聚合文件中存在同名世界概念的两套表示 | 选定一个权威类型族，另一侧必须是迁移模型或显式 Adapter |
| P1 | 坐标与存储身份分裂 | WorldInteraction 和 WorldStorage 各有 TileCoordinate；实体槽位和身份也须区分 | 在跨层 Command/Query 入口使用稳定值对象，禁止隐式换算 |
| P1 | 领域项目之间依赖方向仍松散 | 例如 Physics 依赖 WorldInteraction，WorldStorage 依赖 Items | 用只读 Query 和事务接口收敛，避免继续扩大互引 |
| P2 | 现有 verifier 覆盖面局部 | 当前 focused verifier 主要覆盖 Combat、NPC、世界交互的状态或局部规则 | 在调度器建立后添加阶段顺序、命令原子性和投影只读性验证 |

推荐实施顺序是：

1. 固定 `RuntimeComposition` 的阶段表、世界准备门控和单一提交点。
2. 收敛 `WorldSession` 的重复类型，并为 `WorldStorage` 增加受控存储门面。
3. 让 `ContentCatalog` 成为经校验的只读快照，接入空间/交互查询和命令入口。
4. 依次接入移动/液体、战斗、Player、NPC、Projectile、物品和世界交互；每接入一个领域都以
   focused verifier 证明输入、状态转换、命令输出和拒绝路径。
5. 最后将网络、存档和客户端表现接为单向 Projection，并验证不会反向改写权威状态。

## 7. 本轮明确不拆分的对象

为保持报告处于子系统层级，本轮不建议把下列对象继续拆成一级工作项：

- 每种 NPC AI style、投射物行为、事件变体或 TileEntity 类型。
- `TileCellState` 的位字段、帧字段、液体字段和网络压缩布局。
- `EntitySlotStore` 的固定槽位数组、`whoAmI` 映射或单个 ID 索引。
- Player/NPC/Projectile 中单个资源、Buff、动画、冷却或计时字段。
- 单个 tModLoader Hook、消息号、存档 Tag 键或 UI/音频/粒子投影。

这些内容将来可以在对应子系统的读写者、生命周期、存档和协议证据闭合后再设计；现在提前
拆分只会产生组件目录和跨项目引用，而不会形成更清晰的权威运行时。

## 8. 验收口径与状态

本报告完成的是**初次子系统审查和整理**，不是迁移完成、行为一致或 API 兼容声明。后续某
子系统只有同时满足以下条件，才能从“已识别”提升为“可运行/可迁移”：

- 写入根、只读查询、命令/事件输出和失败语义均已明确。
- 与前后 Tick 阶段的读写约束已记录，并有 deterministic verifier。
- 涉及 WorldStorage 时，提交的原子性、槽位回收和修订语义已验证。
- 涉及网络或存档时，已证明 Adapter 只经受控入口恢复或投影状态。
- 受影响项目按仓库的串行 .NET 构建约束完成构建，输出位于 `Build/bin/`，并运行无构建的
  verifier。

本次为文档审查，没有运行 `dotnet` 编译或测试；不应把文档检查替代代码验证。

## 9. 关联资料

- [Version4 权威游戏模拟系统拆分设计报告](Version4权威游戏模拟系统拆分设计报告.md)：
  文件级证据、详细候选边界和未闭合链。
- [Version4 权威游戏模拟系统主要子系统](Version4权威游戏模拟系统主要子系统.md)：原有主干
  摘要；本报告以更严格的“初次只到子系统”口径对其进行审查和落地排序。
- [Version4 权威游戏模拟子系统证据研究笔记](../review-round-1/reports/2026-09-05-version4-authoritative-simulation-subsystem-findings.md)：
  本轮 Version4 与本地 tModLoader API 的具体交叉证据。
- [ECS 文件组织设计约束](../../../Context/架构设计/ECS文件组织设计约束.md)：后续移动或新增 ECS 文件时的
  目录归属和迁移验收规则。
