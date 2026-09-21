# Version4 权威游戏模拟系统拆分设计报告

## 1. 目的与范围

本报告对 `D:\TRbackup\Version4` 的整个 C# 项目进行一次面向“权威游戏模拟系统”的职责盘点，并给出可以逐步落地的组件、System、Query、Command、Adapter 和 Projection 边界。

本报告遵循 `public-decomposition` 的拆分方法：按共同读写者、变更原因、生命周期和不变量划分边界，不按旧类的字段顺序或文件数量机械拆分。

### 1.1 证据范围

| 项目 | 说明 |
| --- | --- |
| 目标实现 | `D:\TRbackup\Version4` |
| 工程入口 | `Version4\TerrariaServer.csproj` |
| 源码规模 | 排除 `bin/obj` 后约 967 个 C# 文件；其中 `Terraria`、`Terraria.DataStructures`、`Terraria.GameContent`、`Terraria.WorldBuilding` 等命名空间共同构成运行时 |
| 旧式核心聚合 | `Terraria.Main`、`Player`、`NPC`、`Projectile`、`Item`、`Chest`、`WorldItem` |
| 参考组织模式 | `C:\Users\shan\Downloads\ECS\space-station-14-master`，只用于观察组件粒度和 System/Query 边界，不复制其命名或领域语义 |
| 明确排除 | `D:\TRbackup\NLTX\src` 中已有组件；本报告不复用、不评价、不重复设计这些组件 |
| 历史实现 | `dome` 不作为当前 Version4 行为证据 |

### 1.2 证据等级

- `confirmed`：在 Version4 中找到声明、写入口和至少一个真实消费者。
- `partial`：找到声明和部分读写者，但完整生命周期、网络或存档语义仍未闭合。
- `missing`：当前源码不足以确认；不得据此创建新的权威状态。

本报告是设计报告，不是代码迁移报告。没有执行源码改造，也没有声称编译、运行时回归或协议兼容验证已经通过。

## 2. 系统级结论

Version4 的权威模拟不是一个单独的 `Entity` 模块，而是由以下协作面组成的实时持久世界运行时：

> **以世界会话状态为根、以实体交互为核心、由 Tick 调度推进、受内容规则驱动并向网络/存档/表现输出快照的多人沙盒游戏模拟系统。**

旧代码将多个层次集中到静态 `Main` 和几个超大实体类中：

```text
输入 / 网络命令 / 世界生成配置
          |
          v
意图与资格判断 -> 行为与规则 -> 空间/碰撞/液体 -> 战斗/效果
          -> 生成/死亡/复活/消失/掉落 -> 世界提交
          -> 网络复制、存档投影、客户端表现投影
```

目标拆分应保留唯一的权威写入路径。组件只保存内聚状态；System 执行状态转换；Query 只做纯计算或资格判断；Command 表达结构变化和跨实体意图；Adapter/Projection 隔离网络、存档和客户端表现。

## 3. 权威范围与排除范围

### 3.1 纳入权威模拟

纳入所有决定服务器世界结果、实体行为或持久化游戏进度的代码：

- Tick、暂停、世界准备和实体更新调度；
- 世界描述、时间、天气、难度、特殊种子、事件和长期进度；
- Tile/Wall/液体/电线等世界可变状态及其交互；
- Player、NPC、Projectile、WorldItem、Chest 等实体实例；
- 生成、AI、目标、移动、碰撞、战斗、伤害、Buff、掉落和生命周期；
- Item 定义输入、物品实例、库存、装备、使用、配方、商店和容器；
- 世界生成、Biome、城镇住房和刷怪资格；
- 权威网络会话、命令验证、复制快照和存档边界。

### 3.2 只作为外部投影或适配器

以下代码不拥有权威玩法状态：

- `Terraria.Graphics*`、`Terraria.UI*`、`Terraria.Audio*`、相机、粒子、拖尾、动画历史；
- `Main` 的 `CurrentFrameFlags`、`SceneMetrics` 等可从本帧重新计算的派生值；
- `Main.myPlayer` 这类客户端视角选择；
- `CombatText`、`Dust`、`Gore`、`Rain`、`Star` 等表现或短期视觉对象；
- `Terraria.Chat`、`Social`、纯本地化和成就显示逻辑（成就若写入世界进度，必须经事件投影接入）；
- 网络包序列化、socket、压缩和握手的传输细节；它们只能通过 typed Projection/Adapter 访问权威状态。

## 4. 全项目证据地图

| 领域 | 主要 Version4 证据 | 结论 |
| --- | --- | --- |
| 运行时组合与 Tick | `Terraria/Main.cs:11151-11173`、`:11193-11411` | `Update -> DoUpdate -> DoUpdateInWorld` 是总调度入口，存在暂停、世界准备和服务器/客户端分支 |
| 世界会话 | `Terraria/Main.cs:495-632`、`:585-624`、`:1059-1081`、`:12972-13647` | 世界规则、时间天气、入侵和城镇刷新混在 `Main`，但生命周期均为世界或会话级 |
| 实体存储 | `Terraria/Main.cs:928-944`、`:3452-3506` | Tile、Player、NPC、Projectile、Item、Chest 等由固定数组/槽位保存，槽位可能参与网络协议 |
| Player | `Terraria/Player.cs:1013-1083`、`:1134-1142`、`:1219-1369`、`:4300`、`:6865`、`:9948`、`:23435` | 输入、资源、效果、装备、死亡、物品使用、表现和网络状态集中在一个实例类 |
| NPC | `Terraria/NPC.cs:185-377`、`:5152-5785`、`:5897-6345`、`:7119-7183`、`:8133`、`:18208-18229` | 生成、AI、目标、生命/防御、城镇、缩放、变形和生命周期集中在一个实例类 |
| Projectile | `Terraria/Projectile.cs:90-238`、`:500-530`、大量 `AI_*` 方法 | 定义、AI、归属、伤害、寿命、穿透、免疫、轨迹和复制状态混合 |
| Item/库存 | `Terraria/Item.cs:122-204`、`:317-335`、`:1190-1205`、`:1212`；`Player.cs:1013-1083`、`:23435` | 类型定义、堆叠、前缀、武器、装备、弹药和实例状态混合 |
| 世界对象/容器 | `Terraria/Chest.cs:17-26`、`:122-140`、`:459-475`；`WorldItem.cs:15-170` | Chest、银行、商店、世界掉落和槽位注册具有独立世界生命周期 |
| 物理/碰撞 | `Terraria/Collision.cs:74-211`、`:608-1001`、`:1472-1633`、`:2027-2348`、`:2736-2802` | 几何查询、Tile 碰撞、液体碰撞、台阶和伤害 Tile 混合，适合纯 Query + Physics System |
| 液体 | `Terraria/Liquid.cs:88-159`、`:470-520`、`:996-1040`、`:1190-1427` | 世界格子液体有独立流动、合并、网络和生成流程，不应与实体接触状态合并 |
| 电线/机关 | `Terraria/Wiring.cs:151-267`、`:387-468`、`:665-833`、`:2694-2779` | 机关触发、逻辑门、传输液体、传送和爆炸属于世界交互 System |
| 世界生成/生物群系 | `Terraria/WorldGen.cs:6221-6388`、`Terraria.WorldBuilding/*`、`Terraria.GameContent.Generation*` | 创建、加载、清理、树/地牢/生物群系生成与运行时刷新应与普通 Tick 分开 |
| 掉落/配方/内容规则 | `Terraria.GameContent.ItemDropRules/*`、`FishDropRules/*`、`Recipe.cs`、`RecipeGroup.cs` | 规则定义/资格判断与实例化掉落是不同边界 |
| 网络会话/复制 | `Terraria/Netplay.cs:209-323`、`:470-511`；`MessageBuffer.cs:125`；`NetMessage.cs:80-95`、`:2230-2494` | 收包验证、快照生成、槽位同步和传输必须是权威状态的外部边界 |

## 5. 目标系统拓扑

```text
RuntimeComposition
  ├─ WorldSession
  │   ├─ WorldDescriptor
  │   ├─ WorldRules
  │   ├─ WorldTimeWeather
  │   └─ WorldEventProgress
  ├─ WorldStorage
  │   ├─ TileMap / TileEntities
  │   ├─ EntityStore (Player/NPC/Projectile/WorldItem)
  │   └─ Containers (Chest/Bank/Shop)
  ├─ ContentCatalog
  │   ├─ Tile/Wall definitions
  │   ├─ NPC/Projectile/Buff definitions
  │   └─ Item/Recipe/Drop definitions
  ├─ IntentAndInteraction
  │   ├─ PlayerInput
  │   ├─ TileInteraction
  │   └─ WiringInteraction
  ├─ Simulation
  │   ├─ Movement / Physics / Collision
  │   ├─ LiquidFlow
  │   ├─ PlayerGameplay
  │   ├─ NpcSpawnAiTown
  │   ├─ ProjectileBehavior
  │   ├─ Combat / StatusEffects
  │   ├─ ItemUse / Inventory / Crafting
  │   └─ Lifetime / Spawn / Loot
  └─ Boundaries
      ├─ NetworkSession / ReplicationProjection
      ├─ PersistenceProjection
      └─ ClientPresentationProjection
```

依赖方向必须是：

```text
ContentCatalog -> Simulation rules
WorldSession + WorldStorage -> Simulation Systems
Simulation Systems -> Commands/Events -> WorldStorage
Authority State -> Network/Persistence/Presentation projections
Network/Persistence/Presentation -X-> direct authority writes
```

## 6. 子系统与组件拆分设计

下面的组件是针对 Version4 的目标设计名称，不引用或复用 `D:\TRbackup\NLTX\src` 中已有组件。真正实施时必须再次检查目标仓库的命名和公共 API，避免同名双权威。

### 6.1 RuntimeComposition：运行时组合与 Tick 调度

**代码证据**：`Main.Update` 调用 `DoUpdate`（`Main.cs:11151-11173`）；`DoUpdate` 处理自动保存、暂停和天气，再进入 `DoUpdateInWorld`（`:11193-11411`）；`ShouldUpdateEntities` 检查世界准备状态和 `WorldGen.generatingWorld`（`:11400-11409`）。

| 目标构件 | 权威状态 | 对外能力 | 不负责 |
| --- | --- | --- | --- |
| `TickClockState` | tick 序号、暂停/快进决策输入 | `CurrentTick`、`ShouldAdvanceWorld` | 时间天气具体规则 |
| `SimulationScheduler` | 显式阶段表、系统启用状态 | 按阶段运行 System，记录失败 | 不拥有 Player/NPC 状态 |
| `WorldUpdateBarrier` | 世界准备/生成中/可更新状态 | 阻止未就绪实体更新 | 不加载内容定义 |
| `MainThreadCommandQueue` | 待提交的结构变化命令 | 批量提交 Spawn/Despawn/Tile 变更 | 不让任意回调直接改实体 |

**系统顺序**：`Input/NetworkCommand -> Intent -> WorldRules/Time -> Spawn/AI -> Movement/Physics -> Combat/Effects -> Lifetime/Loot -> Replication/Persistence`。表现投影在权威提交后读取快照。

**Seam**：`ISimulationSystem.Update(in SimulationContext, CommandBuffer)`；可用固定 tick、空世界和记录型 CommandBuffer 做 focused verifier。

### 6.2 WorldSession：世界描述、规则、时间天气和事件进度

**代码证据**：世界尺寸、名称和地层字段位于 `Main.cs:495-589`；`hardMode`、特殊种子和事件开关在 `Main.cs:495-624`；入侵进度在 `Main.cs:1059-1081`；`UpdateTime`、`UpdateWeather`、`UpdateInvasion` 和城镇刷新在 `Main.cs:12050-13647`；NPC 文件还直接写入 `downed*` 等世界进度（`WorldGen.cs:6500-6524`）。

| 目标组件 | 典型状态 | 生命周期 | 主要写者 |
| --- | --- | --- | --- |
| `WorldDescriptorState` | 世界名、尺寸、Tile/Section 边界、地牢和地层 | 世界加载至卸载 | World load/generation |
| `WorldRulesState` | hardmode、特殊种子、难度覆盖、生成规则开关 | 世界存档 | World load / rule commands |
| `WorldTimeWeatherState` | day/night、time、moon、rain、wind、eclipse | 每 tick；部分字段可持久化 | `UpdateTime` / `UpdateWeather` |
| `WorldEventProgressState` | invasion、moon events、slime rain、downed/saved/unlocked 进度 | 事件期间或世界存档 | Event systems / lifecycle |
| `WorldSpawnPressureState` | 刷怪延迟、事件压力、人口和区域限制 | 每 tick/事件 | Spawn scheduler |

**不变量**：世界进度不能挂在某一个 NPC 实例上；`Main.myPlayer` 是客户端视角，不属于世界会话；每帧派生的 `CurrentFrameFlags` 不得持久化。

**接口**：`IWorldSessionView`（只读规则/时间/进度）；`IWorldProgressCommand`（事件开始、事件推进、Boss 击败）；`IWorldSessionPersistenceAdapter`（读写 typed snapshot）。

### 6.3 ContentCatalog：内容定义与规则目录

**代码证据**：`Main.Initialize_Items`、`Initialize_TileAndNPCData1/2`、`InitializeItemAnimations` 位于 `Main.cs:3507-3572`、`:5209-...`；定义数据在 `Main` 的按 ID 数组中；`Item.SetDefaults`/`SetDefaults1` 从 `Item.cs:1186-1212` 起填充大量类型属性；NPC/Projectile 的 `SetDefaults` 入口分别在 `NPC.cs:8133` 和 `Projectile.cs` 的初始化区域。

| 目标目录/组件 | 提供的只读查询 | 不拥有 |
| --- | --- | --- |
| `TileDefinitionCatalog` | 固体、光照、工具、合并、容器、帧要求 | 当前 Tile 实例和动画计数器 |
| `WallDefinitionCatalog` | 房屋墙、地牢墙、光照和合并 | 世界墙体存储 |
| `NpcDefinitionCatalog` | 默认生命/伤害/防御、帧、可捕获、AI 风格 | NPC 实例生命和 AI 进度 |
| `ProjectileDefinitionCatalog` | 默认形状、敌我、帧、Hook/Pet 等能力标签 | Projectile 当前寿命和行为进度 |
| `BuffDefinitionCatalog` | 减益、持久、PVP、免疫和保存规则 | 实体当前 Buff 时间 |
| `ItemDefinitionCatalog` | 使用方式、伤害、防御、装备槽、弹药和最大堆叠 | Item 实例数量、前缀和持有关系 |
| `RecipeAndDropCatalog` | 配方、掉落条件、掉落结果定义 | 本次随机结果和已生成掉落实体 |

定义目录在初始化后只读；如果源码存在初始化后写入点，先标 `partial`，不能过早冻结。

### 6.4 WorldStorage：TileMap、TileEntity 与实体槽位存储

**代码证据**：`Main.tile`、`Main.player`、`Main.npc`、`Main.projectile` 等数组在 `Main.cs:928-944`；实体初始化在 `Initialize_Entities`（`:3452-3506`）；`Tile` 的字段在 `Tile.cs:6-24`；TileEntity 注册在 `TileEntitiesManager.cs:30-67`；`WorldSections` 管理区段状态（`WorldSections.cs:6-69`）。

| 目标构件 | 拥有状态 | 关键接口 |
| --- | --- | --- |
| `TileMapStore` | Tile 二维数组、边界和区段 | `TryGetTile`、`SetTile`、`GetSectionSnapshot` |
| `TileEntityStore` | TileEntity ID、坐标、注册和删除 | `Register`、`TryResolve`、`Remove` |
| `EntitySlotStore<T>` | 固定槽位、容量、active/空槽、提交队列 | `TryAllocate`、`Get`、`Release`、`EnumerateActive` |
| `ContainerStore` | Chest/Sign/Bank/Shop 世界对象索引 | `TryGetContainer`、`AllocateChest`、`Release` |
| `WorldSectionState` | 区段加载/同步标记 | `IsLoaded`、`MarkLoaded` |

槽位索引是存储协议，不等于实体身份；`whoAmI`、网络 ID、存档 ID 和账户 ID 必须各自建模。

### 6.5 MovementIntent：控制意图与输入验证

**代码证据**：Player 控制字段在 `Player.cs:1219-1268`；`ItemCheckWrapped` 和 `ItemCheck` 在 `Player.cs:19603-19640`、`:23435-23445`；NPC 运动由其 AI 和更新循环直接修改速度；网络输入在 `MessageBuffer.GetData`（`MessageBuffer.cs:125`）分派。

| 目标组件/命令 | 状态 | 写者 | 读者 |
| --- | --- | --- | --- |
| `PlayerInputIntent` | 左右、跳跃、使用、瞄准、交互意图 | 本地输入/网络命令验证 | Player control system |
| `NpcMovementIntent` | AI 期望方向、目标速度和跳跃意图 | NPC AI | Movement system |
| `InteractionIntent` | Tile、Chest、NPC、机关交互请求 | 输入/网络命令 | Interaction systems |
| `ControlRateState` | 去抖、重复使用、输入序号 | Session/command validator | Intent systems |

意图组件不能直接成为速度或位置；失能、碰撞、液体和环境力可以拒绝或修正意图。

### 6.6 MovementPhysics：移动、几何查询和碰撞提交

**代码证据**：AABB/视线/命中资格 Query 在 `Collision.cs:74-414`；液体/Tile 接触 Query 在 `:829-1085`、`:1472-1633`；实体碰撞和台阶提交在 `:2027-2348`、`:2736-2802`；`Entity` 的位置、速度、方向、宽高在 `Entity.cs:8-24`。

| 目标组件 | 状态类别 | 备注 |
| --- | --- | --- |
| `KinematicState` | 权威位置、速度、重力、移动锁定 | 由 Movement/Physics 写入 |
| `CollisionPolicyState` | 是否穿平台、Tile 碰撞、门/斜坡策略 | 定义或实体能力输入 |
| `ContactResultBuffer` | 本次 Tile/Entity 接触结果 | 每 tick 临时结果，不持久化 |
| `SpatialQueryContext` | 查询范围、碰撞过滤和快照 | 只读 Query 输入 |

`CheckAABBvAABBCollision`、`CanHit`、`TileCollision` 等应成为纯 Query；实际位置/速度改变只能由 Physics System 通过命令或明确写入口完成。几何、碰撞策略和碰撞结果不得合并为一个巨型组件。

### 6.7 LiquidFlow：世界液体与实体液体交互

**代码证据**：液体初始化和快速流动在 `Liquid.cs:88-159`；每帧更新在 `:470-520`、`:996-1040`；放置、检查、合并和删除在 `:1190-1427`；实体碰撞在 `Collision.cs:829-1085`。

| 目标构件 | 权威状态 | 运行频率 |
| --- | --- | --- |
| `TileLiquidState` | Tile 中 liquid type/amount/settling 标记 | 流动 tick/世界生成 |
| `LiquidFlowSystem` | 流动、沉降、液体合并和 panic 模式 | 每 tick 或批量 |
| `LiquidContactEffect` | 实体接触水/岩浆/蜂蜜/微光后的规则结果 | 实体 tick |
| `LiquidNetworkProjection` | 区域液体同步 | 网络发送 |

Tile 液体是世界网格状态，实体湿润/液体接触是实例状态；一个不能替代另一个。

### 6.8 WiringInteraction：电线、逻辑门和机关

**代码证据**：机关更新 `Wiring.UpdateMech` 在 `Wiring.cs:151-267`；开关和逻辑门在 `:267-395`、`:625-833`；大规模操作、传送和爆炸在 `:429-468`、`:2694-2779`。

| 目标构件 | 责任 | 输出 |
| --- | --- | --- |
| `WireNetworkState` | 当前机关扫描、跳线和处理队列 | 待处理 WireCommand |
| `LogicGateSystem` | 逻辑灯/门状态评估 | Actuate 命令 |
| `MechanismInteractionSystem` | 门、灯、陷阱、传送器、爆炸和抽取机 | Tile/Entity/WorldEvent 命令 |
| `WireAccessPolicy` | 当前使用者、范围和权限 | 接受/拒绝结果 |

机关 System 可以写 TileMap、生成实体或触发事件，但不能绕过 WorldStorage 直接修改网络/存档投影。

### 6.9 PlayerGameplay：玩家规则、资源、效果和生命周期

**代码证据**：库存/装备数组在 `Player.cs:1009-1083`；死亡字段在 `:1134-1142`；生命、魔力、防御在 `:1355-1373`；Buff 更新在 `:4300`；装备更新在 `:6865`；死亡更新在 `:9948`；主实例更新在 `:15092-15105`、`:15234-15240`、`:15598-15602`。

| 目标组件 | 拥有状态 | 生命周期 |
| --- | --- | --- |
| `PlayerVitalState` | 当前/最大生命、魔力、防御、再生 | 玩家实例；死亡时转换但不被生命周期拥有 |
| `PlayerLifecycleState` | active/dead/ghost/respawn/spectating | 连接、死亡、复活、断开 |
| `PlayerEffectState` | Buff、免疫、装备临时效果和再生修正 | 每 tick Reset/重建 |
| `PlayerInventoryState` | inventory、armor、dye、misc 槽、trash、selected slot | 存档和实例 |
| `PlayerUseState` | itemAnimation、itemTime、channel、pending reuse | 每次使用/每 tick |
| `PlayerInteractionState` | talk NPC、打开 Chest、Tile interaction | 交互开始至结束 |
| `PlayerMountState` | mount、坐骑计时和移动修正 | 装备/使用/移动期间 |

`Health`、`Defense`、`Immunity`、资源、Buff、库存、手部/选择关系和生命周期必须分开。`UpdateDead` 不应与 `UpdateLifeRegen` 共享写入口；`ItemCheck` 只能通过使用命令改变资源、库存或生成请求。

### 6.10 NpcSpawnAiTown：NPC 生成、AI、目标和城镇

**代码证据**：生成入口和刷怪率在 `NPC.cs:185-377`；Tile 生成资格在 `:5330-5785`；实例默认值和重置在 `:8095-8133`；目标、生命、防御和时间字段在 `:6313-6345`；缩放在 `:17617-18229`；城镇生成/住房相关代码在 `WorldGen.cs:4558-5510`。

| 目标组件 | 所有权 | 不应包含 |
| --- | --- | --- |
| `NpcDefinitionRef` | NPC 类型和默认定义引用 | 当前生命/AI |
| `NpcAiState` | ai/localAI、阶段、计时和行为内部状态 | 世界 downed 进度 |
| `NpcTargetState` | 当前目标实体引用和目标重选时间 | Projectile owner |
| `NpcCombatState` | 伤害、防御、生命、无敌和攻击资格 | 绘制帧/住房 |
| `NpcLifecycleState` | active、timeLeft、despawn、transform/death 状态 | 玩家复活 |
| `NpcTownState` | townNPC、home、住房检查、对话资格 | 普通敌怪 AI |
| `SpawnPressureState` | 区域刷怪压力、人口和事件约束 | 单个 NPC 的生命 |

`NPC.downed*`、`saved*`、塔状态等是 `WorldEventProgressState`，不是 NPC 实例字段。`target` 是动态关系，不能与 Projectile owner 合并。

### 6.11 ProjectileBehavior：投射物定义、行为、伤害和寿命

**代码证据**：核心字段在 `Projectile.cs:90-238`；初始化会重置 `netUpdate`、`identity`、`penetrate`、`timeLeft` 等（`:500-530`）；文件包含大量 `AI_*` 行为方法，并在 `Main` 更新中有前后处理（`Main.cs:11385-11394`）。

| 目标组件 | 状态 |
| --- | --- |
| `ProjectileDefinitionRef` | 类型、默认大小、敌我、AI 风格、能力标签 |
| `ProjectileBehaviorState` | ai/localAI、extraUpdates、当前行为阶段 |
| `ProjectileTrajectoryState` | 速度、轨迹方向、旋转和专用方向 |
| `ProjectileOwnerState` | Player/NPC 归属和伤害归因 |
| `ProjectileDamageState` | 当前攻击值、伤害类型、击退、友伤策略 |
| `ProjectilePenetrationState` | 剩余穿透、命中计数、穿透后停伤规则 |
| `ProjectileLifetimeState` | timeLeft、结束原因、sentry/minion 持续规则 |
| `ProjectileHitImmunityState` | local/static NPC 免疫键和 cooldown |

`identity` 仅在确认协议作用域后作为网络/实体投影字段；不能默认等同于槽位或实体身份。表现历史 `oldPos/oldRot/oldSpriteDirection` 排除在权威组件之外。运行时行为对攻击值的衰减不能回写不可变 Definition。

### 6.12 CombatAndStatus：伤害、治疗、免疫、Buff 和命中事件

**代码证据**：Player 的生命/防御/再生字段在 `Player.cs:1355-1373`，NPC 的战斗字段在 `NPC.cs:6111-6345`，Projectile 的 damage/penetrate/免疫字段在 `Projectile.cs:142-202`；Player Buff 更新在 `Player.cs:4300`；NPC Buff 区域位于 `NPC.cs:6065` 附近。

| 目标构件 | 输入 | 输出 |
| --- | --- | --- |
| `HealthState` | Damage/Heal command | 生命变化、濒死候选 |
| `DefenseState` | 装备/NPC 定义/效果 | 伤害减免输入 |
| `ImmunityState` | 命中来源、冷却键 | 接受/拒绝命中 |
| `StatusEffectsState` | Buff 定义、持续时间、来源 | 效果增删和属性重算 |
| `DamageResolutionSystem` | 攻击输出、目标、碰撞结果 | DamageEvent/Knockback |
| `DeathResolutionSystem` | 生命归零和实体类型 | Player/NPC/Projectile 生命周期命令 |

Health 与 ProjectileDamage、Defense 与 Immunity、实体 Buff 集合与单条 TimedEffect 不得合并。Player 与 NPC 可以共享伤害计算协议，但死亡/复活/消失流程必须分开。

### 6.13 ItemGameplay：物品定义、实例、库存、装备、使用、配方与掉落

**代码证据**：Item 类型和使用字段在 `Item.cs:122-184`；装备槽位在 `:186-204`；`active`/名称派生属性在 `:317-335`；默认定义入口在 `:1186-1212`；玩家库存和装备在 `Player.cs:1009-1083`；物品使用链 `ItemCheck` 在 `Player.cs:23435-23574`，资格和魔力支付在 `:25165-25650`；掉落规则位于 `Terraria.GameContent.ItemDropRules`。

| 目标组件 | 责任 |
| --- | --- |
| `ItemDefinitionRef` | 类型、静态使用/攻击/装备能力、最大堆叠 |
| `ItemInstanceState` | 前缀、染料、名称覆盖、变体和持久实例标记 |
| `ItemStackState` | 当前数量、堆叠合并/拆分 |
| `InventoryLayoutState` | 玩家槽位、装备/弹药/垃圾槽和选择槽 |
| `EquipmentState` | 穿戴关系、装备槽和效果来源 |
| `ItemUseState` | 使用计时、channel、冷却、当前使用目标 |
| `WeaponAbilityState` | 攻击输出、弹药消耗、Projectile spawn 请求 |
| `CraftingState` | 配方资格、材料锁定和制作结果 |
| `LootRollState` | 掉落条件、随机结果和掉落命令 |

Item Definition 与 Stack、Equipment 与 Weapon、Inventory 与 Container 必须保持独立。`ItemCheck` 应拆为资格 Query、资源支付 Command、使用效果 System 和生成请求，而不是让 Item 实例直接调用全局数组。

### 6.14 WorldObjects：WorldItem、Chest、Bank、Shop 和 TileEntity 交互

**代码证据**：`Chest` 声明、类型和内容在 `Chest.cs:17-26`；银行/商店/越界容器构造在 `:122-140`；放置和世界 Chest 注册在 `:459-475`；`WorldItem` 保存保留玩家、时间和物品代理字段（`WorldItem.cs:15-170`）；TileEntity 管理在 `TileEntitiesManager.cs:30-67`。

| 目标组件 | 责任 |
| --- | --- |
| `ContainerCapacityState` | 容量、槽位和容器类型 |
| `ContainerContentsState` | 槽位内 Item 实例和 revision |
| `ContainerAccessState` | 位置、锁定、访问者、银行/商店权限 |
| `WorldItemState` | 世界掉落位置、保留玩家、拾取保护和过期时间 |
| `TileEntityBindingState` | TileEntity 类型、坐标、持久 ID |

Chest 不是 Player Inventory 的别名；银行和商店可以复用最小容器协议，但权限、价格、容量和生命周期独立。WorldItem 的 `timeLeft`/reservation 不能与 Projectile lifetime 合并。

### 6.15 WorldGenerationAndBiomes：世界生成、地形、Biome 与城镇住房

**代码证据**：世界尺寸设置和创建/加载在 `WorldGen.cs:6221-6388`；清理重置大量世界进度在 `:6387-6524`；树、地牢、生物群系生成分布于 `Terraria.GameContent.Generation*`、`Terraria.GameContent.Biomes*` 和 `Terraria.WorldBuilding`；住房评分和 NPC 房间检查在 `WorldGen.cs:5267-5773`。

| 目标构件 | 责任 | 生命周期 |
| --- | --- | --- |
| `WorldCreationSystem` | 新世界阶段、种子和生成提交 | 世界创建期间 |
| `WorldLoadSystem` | 存档读取、版本修复和失败回退 | 加载期间 |
| `TerrainGenerationSystem` | 地形、矿物、树、地牢和结构 | 创建期间 |
| `BiomeRuleQuery` | 根据 Tile/Wall/世界规则判断区域 | 运行时纯查询 |
| `TownHousingSystem` | 房间有效性、占用、搬迁和旅行 NPC | 世界运行期间 |

世界生成不是普通实体 Tick 的一个可重入子步骤；生成期间 `ShouldUpdateEntities` 必须阻止权威实体更新。生成写入 TileMap 和世界进度，随后由 Spawn/AI 系统消费只读快照。

### 6.16 SpawnLifetimeLoot：生成、生命周期、掉落与清理

**代码证据**：NPC 生成和刷怪入口在 `NPC.cs:185-377`、`:5152-5785`；Projectile 初始化和寿命字段在 `Projectile.cs:90-202`、`:500-530`；Player 死亡更新在 `Player.cs:9948`；Chest/WorldItem 创建入口在 `Chest.cs:459-475`、`WorldItem.cs`；掉落规则位于 `Terraria.GameContent.ItemDropRules`。

| 目标构件 | 关键不变量 |
| --- | --- |
| `SpawnCommandBuffer` | 结构变化在阶段末提交，避免遍历中修改槽位 |
| `PlayerLifecycleSystem` | 死亡、复活、幽灵和断开不转化为 NPC/Projectile 生命周期 |
| `NpcLifecycleSystem` | 击杀、自然消失、变形和掉落按 NPC 规则处理 |
| `ProjectileLifetimeSystem` | 命中、穿透耗尽、timeLeft、sentry/minion 结束原因分开 |
| `LootResolutionSystem` | 根据来源、条件和随机源产生 Item spawn 命令 |
| `DespawnAndCleanupSystem` | 释放槽位、解除引用、清理复制和 TileEntity 关联 |

`active` 只是槽位可用性，不能代替生命周期阶段；`timeLeft` 也不能泛化为所有实体的通用寿命。

### 6.17 NetworkSessionAndReplication：权威边界适配器

**代码证据**：连接接受和服务器循环在 `Netplay.cs:209-323`；主线程更新和区段状态在 `:470-511`；消息解码入口在 `MessageBuffer.cs:125`；发送入口和 Tile/NPC/Chest 同步在 `NetMessage.cs:80-95`、`:1861-2494`。

该层不新增权威玩法组件，只定义边界类型：

- `ClientSessionState`：连接、权限、玩家槽位和握手阶段；
- `InboundCommand`：经验证的玩家/世界命令；
- `ReplicationCursorState`：每客户端区段、实体快照游标和限流；
- `NetworkIdentityProjection`：协议 ID、槽位和实体快照键；
- `PersistenceProjection`：玩家/世界存档 DTO。

网络发送失败、视野变化或复制节流不能改变 Health、Location、Target、Lifetime 或 Container 内容。解析失败必须返回 typed error，不能静默绑定到另一个实体。

## 7. 成员归属总表

| 当前成员簇 | 语义所有者 | 目标边界 | 状态类别 | 证据状态 |
| --- | --- | --- | --- | --- |
| `Main.tile`、`WorldSections` | WorldStorage | `TileMapStore`/`WorldSectionState` | 世界存储 | confirmed |
| `Main.player/npc/projectile/item/chest` | WorldStorage | `EntitySlotStore`/`ContainerStore` | 世界存储 | confirmed |
| `Main.worldName/maxTilesX/maxTilesY/dungeon*` | WorldSession | `WorldDescriptorState` | 世界描述 | confirmed |
| `Main.hardMode`、特殊种子 | WorldSession | `WorldRulesState` | 世界规则 | confirmed |
| `Main.time/dayTime/raining/wind*` | WorldSession | `WorldTimeWeatherState` | 每 tick/世界 | confirmed |
| `Main.invasion*`、`NPC.downed*/saved*` | WorldSession | `WorldEventProgressState` | 事件/存档 | partial |
| `Player.control*` | PlayerGameplay | `PlayerInputIntent` | 会话输入 | confirmed |
| `Player.statLife/statMana/statDefense` | CombatAndStatus | `PlayerVitalState` | 实例权威 | confirmed |
| `Player.buffType/buffTime/buffImmune` | CombatAndStatus | `PlayerEffectState` | 每 tick/实例 | confirmed |
| `Player.inventory/armor/dye` | ItemGameplay | `InventoryLayoutState` | 实例/存档 | confirmed |
| `Player.dead/respawnTimer/spectating` | PlayerGameplay | `PlayerLifecycleState` | 会话/实例 | confirmed |
| `NPC.ai/localAI/target` | NpcSpawnAiTown | `NpcAiState`/`NpcTargetState` | 实例 | confirmed |
| `NPC.life/lifeMax/damage/defense` | CombatAndStatus | `NpcCombatState` | 实例 | confirmed |
| `NPC.timeLeft/active` | SpawnLifetimeLoot | `NpcLifecycleState` | 实例/槽位 | confirmed |
| `Projectile.ai/localAI/extraUpdates` | ProjectileBehavior | `ProjectileBehaviorState` | 实例 | confirmed |
| `Projectile.owner/damage/penetrate/timeLeft` | ProjectileBehavior/Combat | Owner/Damage/Penetration/Lifetime | 实例 | confirmed |
| `Projectile.identity/netUpdate/netSpam` | Network boundary | typed projection state | 网络会话 | partial |
| `Item.type/use*/damage/defense` | ContentCatalog + ItemGameplay | Definition/Use/Ability | 定义/实例 | confirmed |
| `Item.stack/prefix/name override` | ItemGameplay | `ItemStackState`/`ItemInstanceState` | 实例/存档 | confirmed |
| `Chest.item/maxItems/位置` | WorldObjects | Container components | 世界对象/存档 | confirmed |
| `WorldItem` reservation/time fields | WorldObjects | `WorldItemState` | 实例/临时 | confirmed |
| `Collision.*` | MovementPhysics | Query + Physics System | 派生/提交 | confirmed |
| `Liquid` 静态流动状态 | LiquidFlow | `TileLiquidState`/Flow System | 世界网格 | confirmed |
| `Wiring` 队列/机关状态 | WiringInteraction | Wire/Logic/Mechanism systems | 世界交互 | confirmed |

## 8. System 顺序与交接契约

| 阶段 | System | 读取 | 写入/输出 | 约束 |
| --- | --- | --- | --- | --- |
| 0 | `CommandIngressSystem` | 网络/本地输入 | 已验证 Intent/Command | 不直接改实体字段 |
| 1 | `WorldSessionSystem` | 时钟、世界规则 | 时间、天气、事件快照 | 先于刷怪和事件规则 |
| 2 | `SpawnAndAiSystem` | 世界快照、目标、定义 | AI/Target/Spawn 命令 | 不直接改网络投影 |
| 3 | `PlayerControlSystem` | PlayerIntent、效果、坐骑 | MovementIntent、UseCommand | 失能状态可拒绝意图 |
| 4 | `MovementPhysicsSystem` | Intent、Kinematic、TileMap、液体 | 位置、速度、ContactResult | Query 纯读；提交顺序固定 |
| 5 | `WiringAndTileInteractionSystem` | InteractionIntent、TileMap、WireState | Tile/机关/实体命令 | 结构变化入 CommandBuffer |
| 6 | `ProjectileBehaviorSystem` | Projectile Definition/Behavior/Owner | 轨迹、伤害候选、Lifetime | 运行时攻击值不改 Definition |
| 7 | `CombatAndStatusSystem` | Damage candidates、Health、Defense、Immunity | Health、Effects、DamageEvent | 拒绝伤害与减免伤害分开 |
| 8 | `ItemUseInventorySystem` | UseCommand、Inventory、Definition | 资源支付、库存、SpawnProjectile/Item | 先资格后支付再效果 |
| 9 | `LifecycleAndLootSystem` | Health/Death/TimeLeft/Events | Despawn/Respawn/Loot/Spawn | 不混用三类实体生命周期 |
| 10 | `CommitAndIndexSystem` | CommandBuffer | EntityStore、TileMap、TileEntity 索引 | 统一处理槽位和引用清理 |
| 11 | `ReplicationProjectionSystem` | 已提交权威快照 | 网络 DTO/区段更新 | 只读投影 |
| 12 | `PersistenceProjectionSystem` | 世界/玩家/容器快照 | 存档 DTO | 不在保存时修正玩法状态 |

并行化只允许发生在没有共享写集且不破坏上述依赖的阶段；不能以文件或目录顺序暗示执行顺序。

## 9. 不拆分项与不合并项

### 9.1 暂不拆分的语义单元

- `ItemDefinitionCatalog` 的紧凑 ID 表在第一阶段保留数组内部存储，外部只读查询；不要立即改成对象图。
- `NPC.ai/localAI` 暂不按每个 AI style 生成不同组件；先保留行为状态容器和 typed AI System。
- `Tile` 的位字段保持值对象语义，先从 TileMapStore 提供受控读写，避免改变压缩/网络格式。
- 固定槽位数组保留为内部实现，先增加门面和命令提交，不直接替换为可变列表。
- `Player` 的高频兼容访问先使用只读/受控门面，等读写者清单闭合后再迁移字段。

### 9.2 明确禁止合并

| 组合 | 原因 |
| --- | --- |
| Player/NPC/Projectile lifecycle | 终止原因、恢复路径和网络语义不同 |
| Health / ProjectileDamage | 目标受击结果与攻击输出相反 |
| Defense / Immunity | 减免伤害与是否允许伤害不同 |
| InputIntent / Velocity | 意图与物理结果不同 |
| ItemDefinition / ItemStack | 静态能力与实例数量不同 |
| Inventory / Container | 角色槽位组织与世界容器容量不同 |
| Target / Owner | 动态选择关系与生成归属关系不同 |
| TileLiquid / EntityLiquidContact | 网格储存与实体接触不同 |
| Domain state / replication state | 权威玩法与传输游标不同 |
| Simulation state / presentation history | 规则状态与客户端视觉缓存不同 |

## 10. 接口、Seam 和可测试性

| 边界 | 最小接口方向 | focused verifier |
| --- | --- | --- |
| WorldSession -> Spawn/AI | `IWorldRulesView`、`IWorldTimeView` | 固定时间/天气/事件输入下的刷怪资格和事件推进 |
| TileMap -> Physics | `ITileQuery`（只读） | 边界、斜坡、平台、液体和越界查询纯度 |
| Input -> PlayerControl | `IInputIntentSource` | 相同输入产生相同 MovementIntent；失能输入被拒绝 |
| Physics -> Combat | `ContactResultBuffer`/`DamageCandidate` | 碰撞候选与命中资格不修改位置之外的状态 |
| Definition -> Instance spawn | `IContentCatalog` | 默认值复制一次，Definition 不被实例 tick 改写 |
| Combat -> Lifecycle | `DamageEvent`/`DeathEvent` | 生命归零只触发一次正确的 Player/NPC 终止流程 |
| Inventory -> ItemUse | `UseCommand`/`PaymentResult` | 资格失败不扣资源；成功只扣一次并产生预期生成命令 |
| EntityStore -> Network | `IEntitySnapshotReader` | 槽位释放后旧快照不可解析到新实体 |
| WorldSession -> Persistence | typed snapshot adapter | 保存/加载保持事件进度和世界描述，不写入表现缓存 |

当前这些 verifier 尚未实现或运行，状态为 `未验证`。设计阶段不能把编译通过当成行为验证。

## 11. 迁移与兼容策略（设计层）

1. 先建立 `WorldSession`、`EntitySlotStore` 和只读 `ContentCatalog` 门面，保留旧 `Main.*` 转发入口，但只允许一个权威写者。
2. 再拆 Player/NPC 的生命、防御、免疫、效果和生命周期，先围绕 `DamageEvent`/`DeathEvent` 建 focused verifier。
3. 收敛 Projectile Definition、Behavior、Owner、Damage、Penetration、Lifetime；禁止 behavior 通过 `with` 或别名回写定义。
4. 抽出 InputIntent、Movement、Physics、Collision Query；确认平台、斜坡、液体和门的顺序。
5. 拆 Item/Inventory/Equipment/Use/Container/WorldItem；保持槽位、堆叠和网络包布局。
6. 将 NPC 世界进度、入侵和 Boss 状态迁至 `WorldEventProgressState`，这是高风险步骤，需同时核对 WorldFile 和生成规则。
7. 最后接入 Network/Persistence/Presentation Projection，删除迁移完成后的重复权威字段。

兼容规则：短期允许单向 Adapter，不允许新旧字段双写；任何解析失败、槽位冲突、重复实体或未知内容 ID 必须显式返回错误。

## 12. 风险和未决证据

| 风险 | 当前状态 | 补证据要求 |
| --- | --- | --- |
| `NPC.downed*` 等字段的完整存档写入点 | partial | 继续检查 `Terraria.IO.WorldFile` 的读写和加载修复路径 |
| Projectile `identity` 的协议作用域 | partial | 对照 `MessageBuffer`/`NetMessage` 的所有生成、查找和同步分支 |
| `Main` 内容数组初始化后是否被改写 | partial | 对每个数组执行写入点清单，再冻结目录接口 |
| `Player` 输入的服务端验证和预测边界 | partial | 完整追踪 MessageBuffer 到 Player 更新的命令链 |
| TileEntity 与 Chest 的持久化/网络 ID 关系 | partial | 检查 `TileEntity`、`WorldFile` 和对象放置包的 ID 作用域 |
| 每个 NPC AI style 的共享读写集 | partial | 先按 AI System 聚类，再决定是否需要专用组件 |
| 事件类是否全部影响权威世界状态 | partial | 对 `Terraria.GameContent.Events` 逐类标记存档、网络和表现写入 |

这些缺口不阻止“候选边界设计”，但阻止直接实施字段删除或冻结为不可变组件。补证完成前，目标组件应保持兼容适配层和显式 `partial` 状态。

## 13. 验收清单与当前结果

- [x] 调查范围覆盖 Version4 的核心运行时、世界、实体、物理、液体、机关、生成、内容、网络和存储命名空间。
- [x] 明确只把 `D:\TRbackup\NLTX\src` 当作排除项，不将其组件作为本报告设计输入。
- [x] 每个候选子系统都有真实 Version4 文件/行号证据、责任和排除范围。
- [x] 组件按状态所有权、生命周期、读写者和不变量分组，而不是按旧类字段顺序复制。
- [x] 明确区分权威状态、派生 Query、网络/存档 Projection 和客户端表现状态。
- [x] 给出系统顺序、依赖方向、接口 seam、不拆分项和不应合并项。
- [ ] 字段级读者/写者/存档/网络清单全部闭合（当前仍有 `partial` 缺口）。
- [ ] focused verifier 尚未实现和运行。
- [ ] 未执行 Version4 编译或运行时回归；本交付物是设计报告，不是实现完成声明。

## 14. 最终设计判断

Version4 的权威游戏模拟系统应以 `WorldSession + WorldStorage + ContentCatalog` 为数据根，以 Player/NPC/Projectile/Item/WorldObject 的领域 System 为行为层，以 Physics/Liquid/Wiring/Combat/Lifecycle 为跨实体规则层，并通过明确的 Network/Persistence/Presentation Projection 输出外部视图。

最重要的拆分原则是：

1. **世界状态和实体状态分开**：`NPC.downedBoss*` 是世界进度，不是 NPC 实例；`Main.tile` 是世界存储，不是某个实体组件。
2. **定义和实例分开**：Item/NPC/Projectile 的按 ID 默认值是内容定义，当前生命、寿命、堆叠和 AI 进度是实例状态。
3. **意图和结果分开**：输入/AI 意图经过规则、碰撞和环境后才产生速度、位置和伤害结果。
4. **规则和副作用分开**：Query 负责纯判断，System/Command 负责写状态，Adapter 负责网络和存档。
5. **生命周期按领域分开**：玩家复活、NPC 消失、Projectile 过期和 WorldItem 拾取保护不能被一个“通用 active/timeLeft”组件重新聚合。

这组边界才是对整个 Version4 权威游戏模拟系统的可验证拆分，而不是把若干旧类简单改名为若干 ECS 组件。

## 15. 第二轮细查：补充的权威模拟子系统

上一轮已经覆盖主循环、世界会话、实体、物理、液体、机关、Player/NPC/Projectile、物品、容器、生成、生命周期和网络。本轮继续遍历 `Terraria.GameContent`、`Terraria.DataStructures`、`Terraria.ObjectData`、`Terraria.Physics` 以及 `Mount`/`Minecart` 等目录，发现以下边界同样会改变权威世界结果，不能被 UI 或表现层吸收。

### 15.1 MountAndMinecart：坐骑、飞行能力和矿车轨道

**证据**：`Terraria/Mount.cs:129-263` 定义速度、跳跃、飞行、疲劳、能力冷却、矿车和碰撞相关配置；`Mount.SetMount` 在 `Mount.cs:4782-4825` 改变玩家能力和 Buff；`Mount.UseAbility` 在 `:2773-2805` 可能生成投射物；`Mount.TryDismount` 在 `:4709-4728` 执行受约束的卸载；矿车轨道碰撞和上轨在 `Terraria/Minecart.cs:566-606`、`:1185-1232`，轨道开关在 `:1276-1322`。

| 目标构件 | 所有权 | 不负责 |
| --- | --- | --- |
| `MountDefinitionCatalog` | 坐骑速度、跳跃、飞行上限、Buff 和能力配置 | 当前玩家是否已骑乘 |
| `MountRuntimeState` | 当前 MountType、飞行时间、疲劳、能力充能和卸载条件 | 玩家库存和装备定义 |
| `MinecartTrackState` | 当前轨道模式、轨道碰撞、转向和开关 | 坐骑飞行能力 |
| `MountMovementSystem` | 将坐骑能力转换为玩家 MovementIntent/Physics 输入 | 动画帧和音效 |

坐骑的“配置”和“实例运行状态”必须分开；矿车轨道属于 Tile/Movement 交互，不应塞回 Player 或通用 Velocity 组件。Mount 可以请求 Projectile/Tile 命令，但不能直接成为生成器。

### 15.2 FishingSimulation：钓鱼资格、鱼漂、鱼获和渔获掉落

**证据**：`Player.cs:818-830` 保存钓鱼技能及鱼线、鱼漂、熔岩钓鱼能力；`Player.cs:20988-21016` 根据物品选择鱼漂；`Player.cs:25478-25495` 检查并回收玩家拥有的 bobber；`Projectile.cs:104` 标记 `bobber`；`Terraria.DataStructures/FishingAttempt.cs:3-51` 保存水量、水质、鱼饵、稀有度、环境和已滚动结果；`Terraria.GameContent.FishDropRules/FishingContext.cs:6-26` 与 `GameContentFishDropPopulator.Populate` 建立按环境/世界规则的掉落表。

| 目标组件/System | 责任 |
| --- | --- |
| `FishingSkillState` | 玩家渔力、鱼饵和装备修正 |
| `FishingBobberState` | 鱼漂 Projectile 归属、位置和咬钩状态 |
| `FishingAttemptState` | 一次钓鱼尝试的水体、环境、稀有度和随机结果 |
| `FishingQualificationQuery` | 水量、液体、Biome、世界进度和装备资格判断 |
| `FishingResolutionSystem` | 生成鱼获、敌怪、宝匣或任务鱼的 Item/NPC 命令 |

`FishingContext` 是一次尝试的计算输入，不是持久实体组件；鱼漂是 Projectile 实例，但不能用通用 ProjectileDamage 表达鱼获结果。随机源必须由 System 显式注入并可复现。

### 15.3 TeleportationNetwork：传送、传送门和传送水晶网络

**证据**：`Terraria.GameContent/PortalHelper.cs:12-30` 保存每玩家双传送门和 Player/NPC 冷却；`TryGoingThroughPortals` 在 `:109-214` 进行碰撞、Tile 安全检查、速度修正和实体传送；`TryPlacingPortal` 在 `:216-235` 检查支撑 Tile 并创建门户；`TeleportPylonsSystem.cs:13-82` 维护水晶列表、冷却、重置和加入玩家同步；`Terraria.GameContent.Tile_Entities/TETeleportationPylon.cs:13-21`、`:108-157` 验证放置和类型；Player/NPC 的 Teleport 入口分别在 `Player.cs:21731`、`NPC.cs:67306`。

| 目标组件 | 状态 |
| --- | --- |
| `PortalNetworkState` | 每个玩家的入口/出口、角度、支撑 Tile 和冷却 |
| `PylonRegistryState` | 水晶位置、Biome 类型、有效性和更新 revision |
| `TeleportRequest` | 发起实体、目标、方式、权限和安全检查结果 |
| `TeleportResolutionSystem` | 选择落点、检查碰撞、更新位置/速度并发出同步事件 |

传送是跨实体关系和空间状态的结构化命令，不是简单给 `Location` 赋值。传送水晶的 TileEntity 持久 ID、网络同步 ID 与实体槽位必须分开。

### 15.4 NpcInteractionAndCommerce：NPC 交互、商店、任务和服务

**证据**：`Terraria.GameContent/NPCInteraction.cs:5-12` 从当前玩家和 `talkNPC` 解析交互对象；`NPCInteractions.cs:15-230` 定义开店、净化、Angler 任务、护士治疗、重铸、发型、税收和住房请求等资格与动作；注册表初始化和商店注册在 `NPCInteractions.cs:233-297`；商店库存还通过 `Chest.SetupTravelShop_AddToShop` 写入 `Main.travelShop`（`Chest.cs:558-575`）；网络包通过 `MessageBuffer`/`NetMessage` 传输交互结果。

| 目标构件 | 责任 |
| --- | --- |
| `NpcInteractionIntent` | 玩家对 NPC 的交互请求和目标引用 |
| `NpcServiceEligibilityQuery` | 距离、对话状态、世界进度、住房和资源资格 |
| `ShopInventoryState` | NPC/旅行商店的当前库存和刷新 revision |
| `NpcServiceSystem` | 治疗、重铸、净化、交付任务、收税和住房命令 |
| `CommerceTransactionSystem` | 价格计算、扣款、物品转移和失败回滚 |

交互资格 Query 不能扣钱或改变库存；交易 System 必须具有原子性。商店库存不是 Player Inventory，也不是世界 Chest 的同义类型。

### 15.5 TeamAndPvpRelations：队伍、敌对标志和 PvP 伤害关系

**证据**：Player 的 `team` 在 `Player.cs:978`，`hostile` 在 `:1968`；伤害关系和同队过滤在 `Player.cs:4671-4683`、Paladin Shield 逻辑 `:22201-22294`；PVP 伤害/死亡入口在 `Player.cs:22208-22325`、`:22572-22732`；客户端上传和服务器广播在 `MessageBuffer.cs:614`、`:1817-1828`、`:2968-2970` 以及 `NetMessage.cs:436-...`。

| 目标组件/System | 责任 |
| --- | --- |
| `TeamMembershipState` | 玩家队伍、队伍颜色和队伍变更 revision |
| `HostilityState` | 玩家是否开启 PvP、敌对资格和保护窗口 |
| `RelationQuery` | 同队、敌对、友伤、护盾和距离过滤 |
| `PvpDamageSystem` | PVP DamageCandidate 的合法性、免疫和死亡事件 |

Team/PvP 是战斗关系状态，不应并入通用 Targeting 或 ProjectileOwner。网络字段是输入/投影，服务器必须重新计算关系，不能信任客户端的最终伤害或死亡结果。

### 15.6 CreativeWorldMutation：创造模式能力与世界写入权限

**证据**：`Terraria.GameContent.Creative/CreativePowers.cs` 定义网络反序列化的创造能力；`CreativePowerManager.cs` 管理能力注册和激活；`Main.cs:11362-11377` 更新创造模式速率覆盖；`Terraria.GameContent.NetModules/NetCreativePowersModule.cs`、`NetCreativeUnlocksPlayerReportModule.cs` 接收权限与解锁消息；这些能力会影响时间、刷怪、Tile 或玩家资源。

| 目标构件 | 责任 |
| --- | --- |
| `CreativePermissionState` | 玩家能力权限、解锁和服务器允许范围 |
| `CreativePowerState` | 当前激活能力及其参数/目标 |
| `CreativeMutationCommand` | 经过权限验证的时间、Tile、实体或资源修改意图 |
| `CreativeMutationSystem` | 在正常模拟阶段提交被允许的世界变更 |

Creative 不应绕过 WorldSession、TileMapStore 或 EntityStore 直接写静态数组；权限验证必须位于网络反序列化之后、命令提交之前。创造能力的 UI 请求和服务器权威能力状态必须分开。

### 15.7 PressurePlateAndAnchors：压力板、坐席/睡眠锚点与绑定实体

**证据**：`Terraria.GameContent/PressurePlateHelper.cs:9-17` 保存压力板按玩家的按压状态和上一位置；`Update`、`UpdatePlayerPosition`、`DestroyPlate` 在 `:19-57`、`:102-115` 更新/清理；`Terraria.DataStructures/AnchoredEntitiesCollection.cs` 管理 Player/NPC 锚点；`Main.cs:1197-1199` 创建 sitting/sleeping 管理器，`DoUpdateInWorld` 在 `:11420-11466` 每帧清空锚点；`PlayerInteractionAnchor` 位于 `Player.cs:2292` 并在死亡/重置路径清理。

| 目标组件/System | 责任 |
| --- | --- |
| `PressurePlateState` | Tile 位置、当前按压玩家集合和脏标记 |
| `AnchorBindingState` | Player/NPC 与坐席、床、TileEntity 的关系 |
| `PressurePlateSystem` | 根据移动前后位置触发机关命令 |
| `AnchorLifecycleSystem` | 建立、刷新、解除和死亡/断开时清理锚点 |

压力板状态属于世界 Tile 交互；锚点是关系状态；二者都不能伪装成玩家位置或通用 Collider。清空锚点是每帧结构维护，不应被表现层调用。

### 15.8 GolfSimulation：高尔夫球专用物理和计分

**证据**：`Terraria.GameContent.Golf/GolfHelper.cs:15-40` 定义球碰撞监听、物理参数和球杆资格；`GolfState.cs:8-28` 保存计分计时、最后击球 Projectile、摆杆计数和轨迹记录；`GolfState.Update` 在 `:79-106` 更新球状态和计分；`ProjectileID.Sets.IsAGolfBall` 与 Projectile 的球类型共同决定作用域。

| 目标构件 | 责任 |
| --- | --- |
| `GolfBallState` | Projectile 球的击球次数、滚动/静止状态和球场关联 |
| `GolfCourseState` | 球洞、击球轨迹和计分阶段 |
| `GolfPhysicsSystem` | 弹性碰撞、穿越、速度和球落点 |
| `GolfScoreSystem` | 击球计数、回球罚分和成绩提交 |

高尔夫球仍是 Projectile，但球场计分和专用物理不能塞进所有投射物的公共行为组件；应通过 `GolfBallState` 能力组件选择性附着。

### 15.9 TilePlacementAndFraming：TileObject 放置、锚点和帧提交

**证据**：`Terraria/TileObject.cs:1-180` 以及后续锚点检查读取 `TileObjectData.AnchorTop/Bottom/Left/Right`；`Terraria/ObjectData/TileObjectData.cs:191-473` 保存有效/无效 Tile、墙和替代锚点；`WorldGen.CheckTileAnchors` 在 `WorldGen.cs:44690-44768` 执行合法性判断；`TETeleportationPylon` 的放置入口进一步证明放置会创建 TileEntity 并可能生成物品（`TETeleportationPylon.cs:119-157`）。

| 目标构件 | 责任 |
| --- | --- |
| `TilePlacementIntent` | 玩家/世界生成请求放置的类型、样式、方向和位置 |
| `TileAnchorQuery` | 根据 TileObjectData 和邻接 Tile 判断锚点有效性 |
| `TileMutationSystem` | 原子写入 Tile、重新框架、创建/删除 TileEntity |
| `FrameUpdateState` | 待重框架坐标和帧变更批次 |

放置资格 Query 必须纯读；Tile 写入、重框架和 TileEntity 结构变化必须集中提交。`TileObjectData` 是内容/放置定义，当前 Tile 和帧计数器属于世界存储/运行时状态。

## 16. 补充系统的交接与顺序

补充系统接入上一版 Tick 顺序后，推荐使用以下明确阶段：

```text
CommandIngress
  -> CreativePermission / Team-PvP validation
  -> WorldTime-Weather-Event update
  -> NPC spawn / AI / Town / NPC interaction eligibility
  -> Player control / Mount-Minecart / Fishing intent
  -> Tile placement / PressurePlate / Wiring / Teleport requests
  -> Movement-Physics / Liquid / Portal collision
  -> Projectile behavior / Golf physics
  -> Combat / PvP / Status / Fishing and NPC service resolution
  -> Item transaction / Crafting / Loot
  -> Lifecycle / Despawn / Respawn / Anchor cleanup
  -> TileEntity / EntityStore commit
  -> Network / Persistence / Presentation projections
```

关键交接契约：

| 交接 | 允许内容 | 禁止内容 |
| --- | --- | --- |
| Mount -> Movement | 速度上限、跳跃/飞行意图、碰撞策略 | 直接写网络快照或表现帧 |
| Fishing -> Loot | 经验证的 `FishingAttempt` 和掉落命令 | 直接改 Player inventory |
| Portal/Pylon -> Teleport | `TeleportRequest`、安全落点和冷却结果 | 绕过 Physics 直接覆写位置 |
| NPC Interaction -> Commerce | 资格结果和原子交易命令 | Query 中扣钱或生成副作用 |
| Team/PvP -> Combat | 关系判定和 DamageCandidate 过滤 | 信任客户端伤害结果 |
| Creative -> WorldStorage | 权限验证后的 Tile/Entity/Time 命令 | 网络解码器直接写世界数组 |
| PressurePlate -> Wiring | 位置前后差异和 Poke 命令 | 把压力板状态当玩家持久状态 |
| Golf -> Projectile/Physics | 专用球物理参数和计分事件 | 把高尔夫规则扩散到所有 Projectile |
| TilePlacement -> TileEntity | 原子 Tile 变更和实体注册命令 | 只改 Tile 而遗留孤立 TileEntity |

## 17. 第二轮补充的证据状态与未决项

| 子系统 | 证据状态 | 尚未闭合的关键问题 |
| --- | --- | --- |
| Mount/Minecart | confirmed / partial | 坐骑配置、Player Buff 和网络同步的最终权威写者需逐项核对 |
| Fishing | confirmed / partial | 实际鱼漂咬钩和 `FishingAttempt.rolledItemDrop` 的完整调用链仍需继续追踪 |
| Portal/Pylon | confirmed | 传送冷却、Portal projectile identity 和区段同步的作用域需补齐 |
| NPC Interaction/Commerce | confirmed / partial | 各服务的扣款/回滚和网络请求分支需要逐服务验证 |
| Team/PvP | confirmed | 服务端对客户端 team/hostile 更新的授权和反作弊边界需补证 |
| Creative mutation | partial | 每种 CreativePower 的世界写入目标和权限级别需逐类登记 |
| PressurePlate/Anchors | confirmed / partial | 玩家断开、NPC 死亡和 Tile 删除的全部解除路径需补齐 |
| Golf | confirmed / partial | 高尔夫分数持久化及球 Projectile 复用路径需补证 |
| Tile placement/framing | confirmed / partial | 所有 `WorldGen.PlaceTile`/`SquareTileFrame` 调用的批提交边界需清点 |

## 18. 更新后的最终判断

第二轮细查表明，Version4 的权威模拟比“Player/NPC/Projectile + 世界 Tile”更宽：任何能改变玩家或 NPC 的可行动资格、实体位置、Projectile 行为、Tile/TileEntity 结构、物品/货币、世界进度、事件结果或伤害关系的模块，都属于权威模拟边界。

因此目标架构应在上一版基础上增加以下能力层：

```text
PlayerCapabilities
  ├─ MountAndMinecart
  ├─ Fishing
  ├─ Teleportation
  └─ Anchors / PressurePlates
WorldInteraction
  ├─ TilePlacementAndFraming
  ├─ WiringAndMechanisms
  ├─ CreativeWorldMutation
  └─ Pylon/Portal network
RulesAndRelations
  ├─ NpcInteractionAndCommerce
  ├─ TeamAndPvpRelations
  ├─ GolfSimulation
  └─ FishingDropRules
```

这些能力应通过明确的组件和 System 选择性附着、显式传递命令和事件；不能因为它们都从 `Main`、`Player` 或 `Projectile` 读取，就重新制造一个更大的 `GameplayComponent`。本轮仍是源码调查与设计，所有列为 `partial` 的字段在实施前必须完成读者/写者、存档、网络和生命周期补证。

## 19. 第三轮细查：跨领域规则和基础设施型模拟边界

为避免只围绕实体类搜索，本轮又按“谁能改变世界结果”反向搜索 `Update`、`Save/Load`、`Place/Kill`、`Register`、`Strike/Hurt` 和 `Spawn` 调用，确认以下边界。它们有些不应注册为普通实体组件，但必须在权威模拟架构中拥有清楚的 System 或 Query 位置。

### 19.1 SceneSensing：Biome、场景指标和环境感知快照

**证据**：`Terraria/SceneMetrics.cs:20-190` 保存高度、Biome、液体、蜡烛、篝火、Banner 和 NPC 位置指标；`SceneMetrics.Scan` 在 `:196-229` 扫描 Tile、液体和 NPC；`Player.UpdateSceneMetrics` 在 `Player.cs:9926` 调用；`Main.UpdateSceneMetrics` 在 `Main.cs:12265-12298` 协调；NPC 刷怪、钓鱼、商店个性和传送水晶都读取这些区域判断。

| 目标构件 | 状态类别 | 设计边界 |
| --- | --- | --- |
| `SceneScanInput` | 位置、扫描中心、范围和扫描选项 | 只读输入 |
| `SceneSnapshot` | 本 tick 的区域/高度/Biome/液体/设施指标 | 每帧派生快照，不持久化 |
| `BiomeQualificationQuery` | 玩家/NPC 是否满足区域资格 | 纯 Query |
| `SceneSensingSystem` | 批量扫描、缓存失效和快照发布 | 不改变 Tile 或实体 |

`SceneMetrics` 不是世界规则的第二份权威状态；它是由 TileMap、实体和世界会话计算出的派生视图。`NPCBannerBuff` 等指标如果最终影响伤害，应该由 Combat 读取快照而不是由扫描器隐式写 Player/NPC。

### 19.2 TownEconomyAndPersonality：城镇住房、幸福度和商店价格

**证据**：`GameContent/TownRoomManager.cs:9-18` 保存 NPC 房间占用和类型索引，`SetRoom/KickOut/Save/Load` 在 `:44-118`；`GameContent/ShopHelper.cs:10-67` 根据 `PersonalityDatabase`、Biome 偏好和附近 NPC 计算价格调整；`Personalities/PersonalityDatabase.cs:5-16` 与 `PersonalityDatabasePopulator.cs:5-153` 注册 NPC 商店特征；`Player.cs:3393` 读取当前购物设置；`WorldGen.cs:5267-5773` 执行房间评分和住房检查。

| 目标组件/System | 责任 |
| --- | --- |
| `NpcHousingState` | NPC 当前房间、无家状态和住房检查结果 |
| `NpcPersonalityDefinition` | NPC 对 Biome/NPC 的偏好和价格修正定义 |
| `TownPopulationSystem` | 入住、搬迁、驱逐和旅行 NPC 资格 |
| `HappinessQuery` | 根据区域、邻居和住房计算价格/对话派生值 |
| `ShopPricingSystem` | 生成当前商店价格和交易上下文 |

住房位置是世界会话/存档状态，个性偏好是内容定义，价格是每次交易的派生结果；三者不能合并为 NPC 的一个“社交组件”。价格 Query 不扣款，交易 System 才能提交物品和货币变化。

### 19.3 BossProgressionAndCredit：Boss 进度、Banner 击杀计数和伤害归因

**证据**：`GameContent/BannerSystem.cs:52-174` 管理击杀计数、可领取 Banner、清理和网络更新；`WorldFile.cs:577`、`:1379`、`:2256` 对 Banner 进行校验/保存/加载；`NPC.CountKillForBannersAndDropThem` 在 `NPC.cs:66168-66179` 写入击杀；`Player.GetBannerBuffEffect` 在 `Player.cs:11964-12010` 把 Banner 转成战斗修正；`GameContent/NPCDamageTracker.cs:46-244` 记录 Boss/入侵伤害归属，`NPC.cs:65519`、`:67628` 调用击杀和伤害登记。

| 目标构件 | 生命周期 | 对外输出 |
| --- | --- | --- |
| `WorldBossProgressState` | 世界存档 | Boss/事件已击败和阶段解锁 |
| `BannerKillProgressState` | 世界存档/网络同步 | 每种 Banner 击杀与可领取数量 |
| `DamageAttributionState` | Boss/入侵实例到结束后短期保留 | 玩家/世界伤害贡献 |
| `ProgressionResolutionSystem` | 击杀提交时 | Boss 进度、Banner 和生成前置条件事件 |

Boss 的世界进度不能留在 NPC 实例；伤害归因不能成为 Health；Banner 计数也不能写入 Player Inventory。击杀事件应一次性提交，并能区分世界贡献和玩家贡献。

### 19.4 TileEntityRuntime：可更新 TileEntity、逻辑传感器和训练假人

**证据**：`Terraria.DataStructures/TileEntity.cs:19-25` 保存全局更新列表、ID 和位置索引；`PerformUpdates` 在 `:48-56` 每 tick 调用实体更新；注册/放置/删除/网络/存档入口在 `:80-143`、`:171-228`；`Terraria.GameContent.Tile_Entities/TELogicSensor.cs:109` 和 `TETrainingDummy.cs:45` 提供会改变机关或战斗结果的专用更新；`WorldGen.UpdateWorld` 在 `WorldGen.cs:59400-59418` 调用 `TileEntity.PerformUpdates()`。

| 目标构件 | 责任 |
| --- | --- |
| `TileEntityIdentityState` | 持久 ID、类型、坐标和有效性 |
| `TileEntityUpdateState` | 是否需要更新、更新队列和脏状态 |
| `TileEntityStore` | 按 ID/坐标注册、解析、删除和冲突检测 |
| `TileEntitySystem` | 每 tick 更新逻辑传感器、假人、展示物等 |
| `TileEntityPersistenceProjection` | 存档/网络 DTO |

TileEntity 是世界对象，不是普通 Player/NPC 组件；但它必须参与统一 CommandBuffer 和提交阶段，避免 Tile 已删除而实体索引、机关状态或持久 ID 残留。

### 19.5 LeashedEntitySimulation：锚定小动物、风筝和区段实体

**证据**：`Terraria.GameContent/LeashedEntity.cs:94-194` 维护按区段和 `whoAmI` 的实例索引、锚点位置和活动状态；`UpdateEntities` 在 `:234-263` 更新活动区段；生命周期虚方法 `Spawn/Despawn/Update/NetSend/NetReceive` 在 `:298-306`；`Main.Initialize_AlmostEverything` 注册原型，`Main.DoUpdateInWorld` 在 `Main.cs:11572` 调用更新；`Terraria.GameContent.Tile_Entities/TECritterAnchor.cs`/`TEKiteAnchor.cs` 创建锚定实例。

| 目标构件 | 责任 |
| --- | --- |
| `LeashedEntityState` | 锚点、区段、类型、活动状态和专用行为状态 |
| `LeashedEntityRegistry` | 原型注册、区段索引、槽位和清理 |
| `LeashedEntityBehaviorSystem` | 小动物/风筝相对锚点的移动和状态更新 |
| `LeashedEntityReplicationProjection` | 区段内网络同步 |

LeashedEntity 不是 Player/NPC/Projectile 的别名，也不应被纯表现层吞掉：它有独立更新、区段激活和网络生命周期。但其 `Draw`、视觉帧和拖尾仍属于表现投影。

### 19.6 WorldEcologyAndSpread：感染扩散、月球灾变和世界生态更新

**证据**：`WorldGen.UpdateWorld` 在 `WorldGen.cs:59400-59418` 检查世界加载状态、难度和 Creative 停止扩散能力，并依次调用 Wiring、TileEntity、Liquid 和月球灾变；同方法后续按世界更新率执行刷怪、城镇和感染扩散；`UpdateLunarApocalypse` 在 `WorldGen.cs:73052-73115` 根据塔和 NPC 活跃状态改变塔状态、倒计时和世界事件；`WorldGen.clearWorld` 在 `:6387-6524` 清理液体、天气和大量进度。

| 目标构件 | 状态 | 设计边界 |
| --- | --- | --- |
| `InfectionSpreadState` | 扩散开关、扫描游标、难度速率 | 世界运行时 |
| `WorldEcologySystem` | 腐化/猩红/神圣扩散、陨石、生态计数和自然变化 | 只经 TileMutationCommand 写 Tile |
| `LunarApocalypseState` | 塔活动、护盾、倒计时和灾变阶段 | 世界事件存档/运行时 |
| `WorldResetSystem` | 创建/加载失败/清理时的状态重置 | 生命周期边界 |

生态更新不能被普通 NPC Spawn 或 Liquid System 吞并：它决定未来 Tile、刷怪和事件资格，且存在不同更新频率、随机性和存档语义。Creative 的“停止扩散”只能作为显式规则输入。

### 19.7 RandomnessAndAttribution：随机源、运气和实体来源

**证据**：`Terraria.GameContent/Luck.cs:3-71` 提供正/负运气滚动；`Player.cs:17869-17985` 更新玩家运气因素并在掉落/生成中使用；NPC 刷怪在 `NPC.cs` 多处使用 `RollLuck`；`Terraria.DataStructures/EntitySource_*.cs` 区分 ItemUse、Projectile、Mount、Tile、Wiring、WorldEvent、Loot 和 Spawn 来源；`NPCDamageTracker` 和掉落规则需要来源归因。

| 目标接口 | 责任 |
| --- | --- |
| `IRandomSource` | 由 System 注入、可记录/重放的随机序列 |
| `LuckState` | 玩家/世界运气输入和临时修正 |
| `EntityProvenance` | 生成来源、父实体、物品/投射物来源和事件来源 |
| `RandomOutcomeSystem` | 在生成、掉落、钓鱼、刷怪时消费随机源 |

随机数发生器本身不是实体组件；但随机源和来源必须显式传递，否则无法验证掉落、Boss 归因、网络重演和回滚。`EntitySource` 不能被简化成一个字符串或一个裸 `Guid`。

### 19.8 PlayerSpawnAndRespawn：玩家出生点、队伍出生和复活位置

**证据**：`Player.Spawn`、`Spawn_GetPositionAtWorldSpawn`、`Spawn_GetPositionAtSpawn` 和 `Spawn_IsAreaValidSpawn` 位于 `Player.cs:21798-22046`；玩家死亡/复活计时在 `Player.cs:22572-22732`；队伍出生和世界出生点参与位置选择；网络收包和连接流程触发玩家生成。

| 目标组件/System | 责任 |
| --- | --- |
| `SpawnPointState` | 世界出生点、队伍出生点和有效性 |
| `RespawnState` | 复活倒计时、上下文、PVP/普通死亡原因 |
| `SpawnPlacementQuery` | Tile 安全、碰撞、队伍和事件条件判断 |
| `PlayerSpawnSystem` | 位置提交、初始状态、Buff/装备重建和同步 |

出生位置 Query 必须纯读；复活不是把 `dead=false` 写回就结束，而是需要清理效果、恢复位置、同步会话并保留死亡原因。它与 NPC 自然生成和 Projectile 过期保持独立。

### 19.9 MinionAndSentryCapacity：召唤物、哨兵和投射物容量

**证据**：`Player.cs:832-834` 保存 `maxMinions/numMinions`；`UpdateMaxTurrets` 在 `Player.cs:25840` 更新哨兵容量；`Projectile.cs:186-190` 保存 minion、slot 和位置；`ItemCheck` 及 Buff 逻辑会生成/回收宠物、召唤物和哨兵；`Projectile.WipableTurret`（`Projectile.cs:314-323`）根据 Owner、持久性和容量决定清理。

| 目标组件 | 责任 |
| --- | --- |
| `MinionCapacityState` | 玩家召唤槽上限和当前占用 |
| `SentryCapacityState` | 哨兵上限、持久时间和清理策略 |
| `MinionOwnershipState` | 投射物归属、slot 消耗和位置 |
| `SummonCapacitySystem` | 生成前检查、替换/清理和 Buff 联动 |

召唤物归属仍是 ProjectileOwner，但容量是 Player 能力与生命周期约束，不能把所有 Projectile 都当作普通伤害投射物，也不能把 minion slot 当作网络槽位。

## 20. 第三轮补充后的系统层级

```text
WorldSession
  ├─ WorldSeedRuleSet
  ├─ WorldClockAndEventScheduler
  ├─ SceneSensing (derived snapshot)
  ├─ WorldEcologyAndSpread
  ├─ TownEconomyAndPersonality
  ├─ BossProgressionAndCredit
  ├─ InvasionProgressAndCredit
  ├─ CoinLossRevengeMarkers
  └─ TileEntityRuntime
PlayerCapabilities
  ├─ MountAndMinecart
  ├─ Fishing
  ├─ PlayerSpawnAndRespawn
  ├─ TeamBasedSpawnPointNetwork
  ├─ PlayerRestAndStacking
  ├─ MinionAndSentryCapacity
  └─ TeamAndPvpRelations
WorldInteraction
  ├─ TilePlacementAndFraming
  ├─ PressurePlateAndAnchors
  ├─ TeleportationNetwork
  ├─ DoorTraversalAndTileMutation
  ├─ DualDungeonBoundaryQualification
  ├─ WiringInteraction
  └─ CreativeWorldMutation
EntitySpecializations
  ├─ NpcSpawnAiTown
  ├─ ProjectileBehavior
  ├─ LeashedEntitySimulation
  ├─ GolfSimulation
  ├─ ShimmerTransmutation
  └─ DontStarveDarknessDamage
RulesAndResolution
  ├─ CombatAndStatus
  ├─ ItemGameplay / InventoryTransferTransactions / NpcInteractionAndCommerce
  ├─ SpawnLifetimeLoot
  ├─ RandomnessAndAttribution
  ├─ FishingDropRules
  └─ BestiaryProgressPersistence (world knowledge projection)
```

## 21. 全项目“已查找但不纳入权威模拟”的边界

以下模块已在 Version4 中找到，但根据写入方向和生命周期，不应作为服务器权威游戏模拟组件；它们应保留为查询、客户端投影或外部适配器：

| 模块 | 排除原因 | 允许的边界 |
| --- | --- | --- |
| `SceneState` 的光照、云、背景和音频值 | 由场景/世界快照派生，主要服务表现 | Client Presentation Projection |
| `Map`、`MapLayer`、Minimap | 地图显示和探索可见性投影，不决定实体规则 | Map Projection；若探索进度持久化则另设 Player Progress |
| `Bestiary` UI/排序信息 | 大多是图鉴展示/客户端筛选；击杀记录若存档应属于 Player Progress | `BestiaryProgressProjection` |
| `Achievements` 条件与显示 | 条件消费事件，不能反向拥有战斗/世界状态 | Event Subscriber + Progression Projection |
| `CombatText`、`Dust`、`Gore`、`Rain`、`Star` | 短期表现对象，不是玩法实体 | Presentation/Effect queue |
| `Cinematics`、`CreditsRoll`、屏幕遮挡 | 客户端流程和视觉状态 | Client Flow System |
| `Net*Module`、socket、压缩 | 传输机制，不是权威状态 | Network Adapter/Projection |

排除不代表这些模块不重要，而是防止将派生视图、UI 历史或协议缓存注册为可持久化的 ECS 权威组件。

## 22. 扩展后的验收与证据缺口

### 22.1 必须增加的 focused verifier

| 验证目标 | 最小场景 | 结果状态 |
| --- | --- | --- |
| Scene 快照稳定性 | 相同 Tile/实体快照重复扫描，结果一致且不写权威状态 | 未验证 |
| 坐骑/矿车 | 上车、飞行疲劳、卸载、轨道切换和碰撞边界 | 未验证 |
| 钓鱼 | 不同液体、Biome、鱼饵、随机种子和 bobber 回收 | 未验证 |
| 传送 | 门户/水晶合法性、冷却、落点碰撞和断开清理 | 未验证 |
| 商店交易 | 幸福度价格、资格失败无扣款、成功原子转移 | 未验证 |
| Boss/Banner | 多玩家伤害归因、击杀一次性提交、存档重载 | 未验证 |
| TileEntity | 放置、更新、删除、网络/存档 ID 冲突 | 未验证 |
| 世界扩散 | Creative 禁止扩散、更新率和 Tile 命令批提交 | 未验证 |
| 随机重放 | 同一 `IRandomSource` 和 EntityProvenance 产生相同掉落/生成 | 未验证 |
| 召唤物容量 | minion/sentry 槽位超限、替换和 Owner 清理 | 未验证 |

### 22.2 新增证据缺口

1. `SceneMetrics` 的扫描结果哪些被服务器权威逻辑读取，哪些仅用于客户端视觉，需按调用点逐项分离。
2. `ShopHelper` 的价格修正、护士治疗、税收和重铸的实际扣款入口仍需完整追踪。
3. `BannerSystem` 的世界存档字段与客户端 claim 状态需要区分，避免把可领取数量误当作玩家库存。
4. `TileEntity` 子类型的 `OnPlayerUpdate`、`Update` 和网络序列化需要逐类型建立写者清单。
5. LeashedEntity 的服务器/客户端更新分支和区段加载阈值需要从 `Netplay` 与 `ActiveSections` 继续补证。
6. `CreativePower` 每个能力对 WorldSession、TileMap、EntityStore 的具体写集尚未闭合。
7. `EntitySource` 目前只确认类型分层，尚未确认所有调用点是否保留父实体和来源类型到存档/网络投影。

## 23. 更新后的总体结论

对 Version4 的细查不应止于七个大类或上一轮的十七个系统。完整的权威模拟边界至少包含：

- 世界会话、生态扩散、时间天气、事件进度和场景感知；
- TileMap、TileEntity、区段、压力板、锚点、机关、传送门和传送水晶；
- Player 的输入、出生/复活、坐骑/矿车、钓鱼、召唤物容量、队伍和 PvP；
- NPC 的生成、AI、目标、住房、个性、商店服务和 Boss 进度；
- Projectile 的行为、Owner、轨迹、伤害、穿透、寿命、高尔夫和鱼漂专用能力；
- Item 的定义、实例、库存、装备、使用、交易、配方、掉落和随机结果；
- 战斗、效果、Banner、伤害归因、生命周期和结构提交；
- 网络、存档和客户端表现的单向投影边界。

新增系统的共同拆分原则是：

1. **派生感知不拥有世界**：Biome/SceneMetrics 产出快照，不能反向成为第二份世界真相。
2. **能力附着而非类型分支**：坐骑、钓鱼、召唤物、高尔夫和 LeashedEntity 通过选择性能力组件接入，不扩大所有实体的公共状态。
3. **世界对象拥有自己的生命周期**：TileEntity、Chest、Portal、Pylon、PressurePlate 和锚点不能被 Player/NPC 的实例状态代替。
4. **进度、归因和随机性显式化**：Boss/Banner、伤害贡献、EntitySource 和随机源必须成为事件或服务边界，不能隐含在 `StrikeNPC`、`NewNPC` 或 `ItemCheck` 的副作用中。
5. **所有结构变化集中提交**：生成、传送、放置、机关、掉落、复活和清理都通过命令缓冲进入 EntityStore/TileMap/TileEntity 提交阶段。

这使报告从“旧类拆成若干组件”提升为对 Version4 整个权威游戏模拟面的系统设计；实现前仍须逐字段闭合读写、存档、网络和生命周期证据，并为每个高风险边界建立 focused verifier。

第四轮检索进一步把边界扩展到 Shimmer 转化、世界事件时钟/调度、掉钱复仇标记、图鉴知识持久化和 Secret Seed 规则集；详见第 24～26 节。它们不是表现层的附属功能：前四者会改变实体、事件资格或世界持久进度，最后一项则在世界加载后继续约束刷怪、液体、生态和事件规则。

## 24. 第四轮细查：转化、事件调度和持久知识状态

本轮继续从 `Main.UpdateTime`、`WorldFile`、`NetMessage` 和跨实体写入点反向追踪。新增边界均满足“能改变后续游戏结果，或决定服务器保存/恢复的权威状态”的条件；仅产生粒子、音效或 UI 文本的调用仍归入表现投影。

### 24.1 ShimmerFluidAndTransmutation：Shimmer 液体、物品转化和实体变体

**证据**：`Terraria/Liquid.cs:1420-1424` 的 `ShimmerCheck` 把 Shimmer 作为独立液体类型推进 `LiquidCheck`；`WorldItem.cs:25-27` 保存 `shimmered` 与 `shimmerTime`，`WorldItem.UpdateItem` 在 `:498-510` 根据 `shimmerWet` 进入转化、冷却和重新堆叠路径，`WorldItem.cs:625` 的 `Shimmering` 是当前反编译版本未展开的关键写入点；`ItemID.Sets.ShimmerTransformToItem`、`ShimmerCountsAsItem` 和 `ShimmerPostMoonlord`（`Terraria.ID/ItemID.cs:85-91`）提供内容定义；`NPCID.Sets.ShimmerTransformToItem`/`ShimmerTransformToNPC`（`Terraria.ID/NPCID.cs:4836-4840`）和 `NPC.GetShimmered` 调用（`NPC.cs:33293`、`:77658`）说明 NPC 也有独立的 Shimmer 变体转换；玩家在 `Player.cs:17134-17142` 记录 `shimmerWet`，在 `:17353-17361` 清理液体状态，并由 `ShimmerUnstuckHelper` 处理卡住保护。

| 目标构件 | 状态类别 | 设计边界 |
| --- | --- | --- |
| `ShimmerFluidState` | TileMap 中的液体种类、流动队列和邻接传播 | 世界权威状态，由 Liquid System 写入 |
| `ShimmerableItemState` | 世界物品的转化中、已转化、转化计时和堆叠资格 | WorldItem 生命周期状态 |
| `ShimmerVariantState` | NPC/城镇 NPC 的变体和可逆/不可逆转换标记 | NPC 能力/变体组件，不并入普通外观 |
| `ShimmerTransformDefinition` | Item/NPC 转化表和解锁条件 | 只读内容定义 |
| `ShimmerTransmutationSystem` | 液体接触、转化提交、变体生成、冷却和网络事件 | 写入 Item/NPC/Tile 命令 |
| `ShimmerQualificationQuery` | 实体是否可被 Shimmer、是否受 Boss/进度限制 | 纯 Query |
| `ShimmerPresentationProjection` | 透明度、粒子、音效和脱困特效 | 客户端表现，不回写权威状态 |

Shimmer 不能仅作为 `Wet` 布尔值：它同时改变物品类型、NPC 类型、玩家移动/伤害资格和液体传播。实现时应先以 `ShimmerTransformDefinition` 查询转化结果，再由 System 原子地提交“销毁旧实体 + 创建新实体/修改实例 + 发送同步”；`Shimmering()` 和 `GetShimmered()` 在 Version4 中未展开，故具体写集仍标为 `partial`，不得在实现阶段假定旧实现行为。

### 24.2 WorldEventClockAndScheduler：事件状态与时间调度

**证据**：`Main.UpdateTime` 在 `Main.cs:13112-13118` 以固定阶段调用 `CultistRitual.UpdateTime`、`BirthdayParty.UpdateTime`、`LanternNight.UpdateTime`、`Sandstorm.UpdateTime`、`DD2Event.UpdateTime`、`CreditsRollEvent.UpdateTime` 和 `MysticLogFairiesEvent.UpdateTime`；`Main.cs:12972` 先更新 `time/dayRate`，因此这些事件共享世界时钟但拥有不同的资格和生命周期；`DD2Event.cs:93-128` 将三档入侵完成状态写入世界存档，`BirthdayParty`/`Sandstorm`/`LanternNight` 的临时字段由 `WorldFile.cs:1045-1096` 快照、在 `:166-178` 恢复；`NetMessage.cs:318-332` 和 `:401` 将 Party、Sandstorm、DD2 和 Lantern 状态同步给客户端；`CultistRitual.cs:21-77` 展示延迟、重检、硬模式/Boss 前置和 WorldEvent 生成；`BirthdayParty.cs:90-151` 展示冷却、NPC 候选、随机庆祝名单和手动开关；`Sandstorm.cs:36-109` 展示风速资格、持续时间、严重度和开始/停止边界。

| 目标构件 | 权威状态/行为 | 设计边界 |
| --- | --- | --- |
| `WorldClockState` | `time`、`dayTime`、`dayRate`、月相和日夜边界 | 世界会话状态 |
| `EventProgressState` | DD2 三档完成、Cultist 延迟、Party/Lantern 冷却 | 世界存档状态 |
| `EventRuntimeState` | 当前事件是否进行、剩余时间、严重度、参与 NPC/塔 | 事件实例/运行时状态 |
| `WorldEventQualificationQuery` | Boss、天气、时间、NPC 数量和区域资格 | 纯 Query |
| `WorldEventSchedulerSystem` | 按显式顺序推进时钟、检查开始/结束、发布事件命令 | 世界调度 System |
| `EventReplicationProjection` | 状态位、进度条、广播文本和客户端同步 DTO | 网络/表现投影 |

事件不应被压缩成一个 `WorldEventComponent`：Party 的 NPC 名单、Sandstorm 的连续严重度、DD2 的波次伤害归因和 Cultist 的重检定时钟具有不同的存档字段与更新频率。统一的是调度协议（`Precondition -> Start/Advance -> End -> Persistence/Replication`），而不是状态布局。必须显式声明 `WorldClockSystem -> EventScheduler -> NPC/Projectile/WorldEcology` 的依赖，避免依靠文件顺序。

### 24.3 CoinLossRevengeMarkers：掉钱复仇标记与临时敌人重生

**证据**：`Terraria.GameContent/CoinLossRevengeSystem.cs:14-295` 的 `RevengeMarker` 保存地点、NPC 网络类型、死亡时生命百分比、AI 风格、金币价值、雕像来源、过期时间和唯一 ID；`CacheEnemy`（`:317-336`）在 NPC 被击杀/离场时创建标记并广播；`Update`（`:353-357`）推进系统时钟；`CheckRespawns`（`:360-428`）按在线且未死亡玩家的内外矩形、事件状态和 AI 劝退条件触发 `SpawnEnemy`，成功后移除标记；`RemoveExpiredOrInvalidMarkers`（`:430-448`）清理过期、事件失效或强制失效标记；`WriteSelfTo`（`:265-278`）与 `NetMessage.SendCoinLossRevengeMarker`（`NetMessage.cs:2359-2368`）定义网络投影。标记使用 `EntitySource_RevengeSystem` 生成 NPC，而不是伪装成原 NPC 的持久实例。

| 目标构件 | 状态类别 | 设计边界 |
| --- | --- | --- |
| `RevengeMarkerState` | 地点、NPC 类型/AI、生命比例、金币、来源和过期时间 | 世界运行时临时状态 |
| `RevengeMarkerIdentity` | 唯一 ID、重生尝试锁和网络 ID | 关系/协议标识，不能当 NPC `whoAmI` |
| `RevengeEligibilityQuery` | 玩家范围、事件仍在进行、AI 劝退和地图边界 | 纯 Query |
| `CoinLossRevengeSystem` | 缓存、计时、原子重生、失效清理和并发锁 | 世界规则 System |
| `RevengeMarkerReplicationProjection` | 写入/删除标记 DTO 和加入游戏时的补发 | 网络投影 |

该系统与 `Loot`、`NPCSpawn` 和 `PlayerDeath` 通过事件交接：死亡/离场发布 `EnemyEligibleForRevenge`，复仇 System 仅在范围和资格 Query 通过后提交 `SpawnNpcCommand`。金币价值决定标记保存时长，但不应重新扣减玩家库存；标记成功重生后才消费一次，并必须验证同一标记在同一 tick 不会重复生成。

### 24.4 BestiaryProgressPersistence：图鉴观察、对话和击杀知识进度

**证据**：`Terraria.GameContent.Bestiary/BestiaryUnlocksTracker.cs:5-56` 聚合 `NPCKillsTracker`、`NPCWasNearPlayerTracker` 和 `NPCWasChatWithTracker`，实现每世界 `Save/Load/ValidateWorld/Reset` 和 `OnPlayerJoining`；`NPCKillsTracker.cs:23-46` 按 NPC 持久 credit ID 累加击杀并广播 `NetBestiaryModule`，`:57-115` 将字典写入世界存档并在玩家加入时补发；`NPCWasNearPlayerTracker.cs:60-149` 通过扫描区域记录“见过”并持久化；`NPCWasChatWithTracker.cs:54-116` 记录对话解锁；`Main.cs:13625-13641` 将三类解锁汇总成进度报告；NPC 击杀调用点在 `NPC.cs:65323-65360`，玩家/NPC 对话调用点在 `Player.cs:3385`、`NPC.cs:42259`。

| 目标构件 | 生命周期 | 设计边界 |
| --- | --- | --- |
| `BestiaryKillProgressState` | 世界存档/网络同步 | 每种 NPC credit ID 的击杀计数 |
| `BestiarySightProgressState` | 世界存档/扫描更新 | 区域观察到的 NPC 集合 |
| `BestiaryChatProgressState` | 世界存档/网络同步 | 与 NPC 对话解锁集合 |
| `BestiaryCreditIdQuery` | 将 NPC 实例解析为稳定持久 ID | 纯 Query，拒绝 `whoAmI` 作为存档键 |
| `BestiaryProgressSystem` | 订阅击杀、观察、对话事件并原子更新计数 | 领域 System |
| `BestiaryProgressProjection` | 进度报告、UI 列表和 `NetBestiaryModule` DTO | 客户端/网络投影 |

图鉴进度属于世界知识状态，不属于 NPC、Player Inventory 或 UI 排序缓存。`AchievementsHelper.TryGrantingBestiary100PercentAchievement` 只能订阅完成率事件；它不能拥有击杀字典。实现时应验证重复网络包、非法 credit ID、计数上限和存档版本迁移。

### 24.5 SecretSeedRuleSet：世界规则组合与生成/运行时资格

**证据**：`Terraria/WorldGen.cs:40-286` 定义 `SecretSeed` 及其启用条件/组合约束；`WorldFile.cs:1279-1284` 保存 `drunkWorld`、`getGoodWorld`、`tenthAnniversaryWorld`、`dontStarveWorld`、`notTheBeesWorld` 和 `remixWorld`，`:1458-1464` 保存 vampire/infected/team-based/dual-dungeons 等扩展规则，`:2041-2073` 与 `:2527-2561` 负责版本兼容加载；`WorldFile.cs:3373-3375` 根据世界 Seed 文本恢复 SecretSeed 代码；`WorldGen.cs:10172-10185` 将 Seed 解析结果绑定到生成随机源和 `Main.rand`。NPC 刷怪、Tile 生成、天气、生态扩散和事件资格在多个调用点读取这些标志。

| 目标构件 | 状态类别 | 设计边界 |
| --- | --- | --- |
| `WorldSeedIdentityState` | Seed 文本、稳定种子和 SecretSeed 代码 | 世界存档身份 |
| `WorldRuleSetState` | 规则开关、版本和组合校验结果 | 世界权威配置 |
| `WorldRuleQualificationQuery` | 某事件/刷怪/生成行为是否被规则允许 | 纯 Query |
| `WorldGenerationSystem` | 初始地形、矿层、结构和规则驱动的初始实体 | 新世界生命周期 System |
| `WorldRuleRuntimeAdapter` | 将规则集映射为刷怪、生态、液体和事件输入 | 领域 Adapter，不让 `Main.*World` 到处散落读取 |
| `WorldSeedProjection` | 复制种子、网络握手和 UI 显示 | 外部/客户端投影 |

Seed 规则不能被视为一次性生成参数：同一旗标在加载后的刷怪、液体、传送、事件与生态路径持续生效；但生成算法和运行时规则仍应分开，以免重新生成世界时意外改写已存档状态。规则组合冲突必须在世界创建阶段失败，而不是让各个 System 各自解释不同的布尔组合。

## 25. 第四轮新增系统的交接契约和调度顺序

```text
WorldSeedIdentity / WorldRuleSet
          │
          ▼
WorldClock ──► WorldEventScheduler ──► EventProgress / EventRuntime
          │                                  │
          │                                  ├─► NPC spawn / invasion / ecology inputs
          │                                  └─► EventReplicationProjection
          │
          ├─► LiquidSystem ──► ShimmerTransmutation ──► Item/NPC commands
          ├─► PlayerDeath/NpcDespawn ──► CoinLossRevengeMarkers ──► SpawnNpcCommand
          └─► NPC sight/chat/kill events ──► BestiaryProgressSystem ──► World save/network projection
```

必须保持以下方向性不变量：

1. `WorldSeedRuleSet` 只提供规则输入；任何运行时 System 不得修改 Seed 文本或静默切换 SecretSeed。
2. `WorldEventScheduler` 先更新时钟和资格，再提交事件状态；刷怪、生态和网络只读取已提交快照。
3. Shimmer 的液体传播、实体转化和网络同步必须使用同一提交批次；表现粒子不得触发转化。
4. 复仇标记是临时世界关系状态，不能写入 NPC 的持久 ID、玩家钱包或普通掉落表。
5. 图鉴计数以稳定 credit ID 为键，击杀事件只提交一次；UI/成就消费其投影，不拥有计数。

## 26. 第四轮重点验证计划和证据缺口

| 验证目标 | 最小场景 | 结果状态 |
| --- | --- | --- |
| Shimmer 转化原子性 | 物品/ NPC 接触 Shimmer，重复 tick、堆叠和网络重放 | 未验证；`Shimmering`/`GetShimmered` 需补完整写集 |
| 事件调度 | 日夜跨界、暂停/快进、事件互斥和存档恢复 | 未验证 |
| 复仇标记 | 同一标记多玩家同时进入范围、过期、事件结束和断线重连 | 未验证 |
| 图鉴进度 | 击杀/观察/对话重复包、版本存档迁移和加入游戏补发 | 未验证 |
| SecretSeed 规则 | 同一 Seed 重载、规则冲突、运行时刷怪/生态资格一致性 | 未验证 |

新增缺口：

1. `WorldItem.Shimmering` 和 `NPC.GetShimmered` 在 Version4 当前文件中为空体，必须从同版本完整实现或等价运行时路径补证，不能依赖 `ItemID` 数组推断所有副作用。
2. `StartSandstorm`、`StopSandstorm`、`LanternNight.NaturalAttempt`、`BirthdayParty.CanNPCParty` 等事件方法也存在未展开体；应补充它们的实体/网络写集和失败路径。
3. 图鉴三类 tracker 的世界存档区段顺序、版本号和 `NetBestiaryModule` 的服务器授权仍需建立逐字段清单。
4. SecretSeed 在 `WorldGen` 之外的运行时读取点需要收敛到 `WorldRuleQualificationQuery`，并验证客户端不能通过网络包改变规则集。

这些缺口不阻止“新增子系统边界”的设计结论，但阻止直接进行行为迁移；在每个缺口闭合前，候选实现只能标记为 `partial`，并必须先建立 focused verifier。

## 27. 第五轮细查：团队出生、库存事务和自动门

### 27.1 TeamBasedSpawnPointNetwork：按队伍分配的出生点

**证据**：`Terraria.GameContent/ExtraSpawnPointManager.cs:10-38` 保存每个队伍的 `extraSpawnPoints`，并且只有 `Main.teamBasedSpawnsSeed` 开启时才允许查询；`:44-88` 从生成阶段的陆块数据筛选候选，`:99-128` 为 `PlayerTeamID.Count` 个队伍生成随机/回退出生点；`:132-176` 负责清理、网络/存档读写；`WorldGen.cs:676-685` 在 Secret Seed 完成后生成；`WorldGen.cs:21665` 准备出生点周围设施；`Player.cs:21860` 参与玩家复活位置选择；`MessageBuffer.cs:407-483`、`:1833-1838` 在连接/切队时选择队伍出生区段；`NetMessage.cs:376`、`:402` 广播规则位和出生点数组。

| 目标构件 | 状态类别 | 设计边界 |
| --- | --- | --- |
| `TeamSpawnPointState` | 每队坐标、有效性和生成来源 | 世界生成后持久/网络状态 |
| `TeamSpawnRuleState` | 是否启用团队出生、Skyblock/陆块选项 | 世界规则输入 |
| `TeamSpawnPlacementQuery` | 队伍索引、Tile 安全性和区段有效性 | 纯 Query |
| `TeamSpawnPointSystem` | 候选筛选、随机分配、回退和重置 | 世界生成 System |
| `TeamRespawnSystem` | 复活/切队时选择坐标和加载区段 | 玩家生命周期 System |
| `TeamSpawnReplicationProjection` | 初始同步、切队同步和区段预取 | 网络投影 |

该系统不是普通 `SpawnPointState` 的一个布尔分支：出生点数组拥有独立生成、存档/网络格式和按队伍反向查找关系。玩家发送的 team 值只能作为查询输入，服务器必须以规则集和已生成数组重新计算位置，不能信任客户端坐标。

### 27.2 InventoryTransferTransactions：快速堆叠、紧急堆叠和远程制作

**证据**：`Terraria.GameContent/EmergencyStacking.cs:9-140` 定义按物品类型/稀有货币/装备分组的转移优先级；`:152-229` 保存待处理转移、关联 WorldItem、容量释放和所有权释放；`Item.cs:48822-48829` 在世界物品槽位不足时触发紧急堆叠；`Main.cs:12781` 每 tick 提交待处理转移；`Terraria.GameContent/QuickStacking.cs:11-86` 保存箱子目的地、锁定/阻塞状态、来源背包和按物品类型索引；`MessageBuffer.cs:2538-2543` 接收远程快速堆叠并调用服务器处理，`NetMessage.cs:1205` 回传阻塞箱列表；`CraftingRequests.cs:9-32` 定义 `RemoteCraftRequest`（配方、结果、消耗物品和请求项）与待处理队列，`Main.cs:11802` 将队列状态纳入本地库存动作门禁。

| 目标构件 | 状态类别 | 设计边界 |
| --- | --- | --- |
| `InventoryTransferIntent` | 来源槽、目标容器、物品数量和所有权 | 命令输入，不是组件快照 |
| `TransferReservationState` | WorldItem 保留玩家、箱子锁定、阻塞列表 | 世界交互关系状态 |
| `PendingCraftRequestState` | 配方、请求材料、预期结果和快速制作标记 | 玩家会话临时状态 |
| `InventoryTransferQualificationQuery` | 可堆叠、容量、距离、锁定和权限 | 纯 Query |
| `InventoryTransferSystem` | 快速堆叠、紧急堆叠、所有权释放和原子转移 | 事务 System |
| `RemoteCraftValidationSystem` | 服务器重算配方/材料，提交消费与结果 | 网络命令处理 System |
| `InventoryTransferProjection` | 物品槽更新、阻塞箱列表和待处理状态 | 网络/UI 投影 |

快速堆叠和制作不能放进通用 `ItemGameplay` 的一个方法：它们跨越 Player inventory、Chest、WorldItem 和网络请求，并要求“验证后一次性消费/生成”。远程包提供的 `result` 与 `consumed` 只能作为客户端意图或诊断，服务器必须从权威库存和配方重新计算，防止重复消费、负数量和越权箱子写入。当前 `QuickStackToNearbyChests` 与 `CraftingRequests.NetCraftingRequestsModule.Deserialize` 未展开，故完整验证仍为 `partial`。

### 27.3 DoorTraversalAndTileMutation：自动开关门与移动交接

**证据**：`Terraria.GameContent/DoorOpeningHelper.cs:9-95` 维护门类型处理器、正在开启的门列表和速度触发窗口；`Player.cs:2294` 持有 helper，`:12778` 在冲刺中调用 `AllowOpeningDoorsByVelocityAloneForATime`，`:26529` 初始化；`NPC.cs:43973-44002` 依照 `closeDoor` 和离开门区域关闭门，`:44202-44222` 按移动方向尝试开门；`WorldGen.cs:26186-26280` 的 `CloseDoor` 检查空闲空间、门 Tile 类型并批量写入 2×3 Tile；`WorldGen.cs:31144` 的 `OpenDoor` 检查锁、Tile 帧和方向后重写门结构；`Wiring.cs:1286-1301` 也能强制关/开门。

| 目标构件 | 状态类别 | 设计边界 |
| --- | --- | --- |
| `DoorState` | 门类型、开闭帧、锁定和方向 | TileMap 权威状态 |
| `DoorTraversalIntent` | Player/NPC 方向、速度窗口和强制关闭标记 | 移动/机关命令输入 |
| `DoorOpeningQualificationQuery` | 门类型、锁、碰撞空隙、Tile 区域和重力方向 | 纯 Query |
| `DoorTraversalSystem` | 自动开门、延迟关门和实体门区跟踪 | 跨实体行为 System |
| `DoorTileMutationSystem` | 原子重写门 Tile、帧、声音/网络事件 | Tile 命令提交 |
| `DoorPresentationProjection` | 开关门音效、尘埃和动画 | 表现投影 |

门不是纯碰撞查询：开关会改变未来 Tile 碰撞、NPC 路径和机关结果。`DoorOpeningHelper` 的具体处理器方法在 Version4 中未展开，因此“哪些门允许速度触发、何时延迟关闭”的细节为 `partial`；但 `WorldGen.OpenDoor/CloseDoor` 的 Tile 写入边界已确认，必须通过统一 TileMutationCommand 提交，不能在 Physics System 内直接写数组。

## 28. 第五轮新增系统的交接契约

```text
WorldRuleSet
  └─► TeamSpawnPointSystem ──► TeamSpawnPointState
                                  └─► PlayerSpawn/Respawn ──► Section/Network projection

PlayerInventory + Chest + WorldItem
  └─► TransferQualificationQuery
        ├─► InventoryTransferSystem ──► atomic slot/container commands
        └─► RemoteCraftValidation ──► consume materials + create result

MovementIntent / NPC AI / Wiring
  └─► DoorOpeningQualificationQuery
        └─► DoorTraversalSystem ──► DoorTileMutationSystem ──► Tile/Physics/NPC projections
```

新增不变量：

1. Team 出生点按 `team` 反向索引，但坐标只能由服务器保存的 `TeamSpawnPointState` 提供；客户端不能提交任意出生位置。
2. 快速堆叠/制作先锁定来源和目标，再一次性提交转移；任一材料、距离或权限校验失败时不得部分扣除。
3. 紧急堆叠释放出来的槽位只能在同一提交阶段分配给新的 WorldItem，不能被并行生成路径抢占。
4. 门的开关帧、碰撞空隙和网络 Tile 更新必须同批提交；关闭失败时保留门打开状态，不得只改视觉帧。

## 29. 第五轮验证计划与剩余缺口

| 验证目标 | 最小场景 | 结果状态 |
| --- | --- | --- |
| 团队出生点 | 四队生成、切队、复活、重载和恶意坐标包 | 未验证 |
| 快速/紧急堆叠 | 多箱锁定、保留物品、槽位满、断线重试 | 未验证；快速堆叠实现体未展开 |
| 远程制作 | 材料不足、重复请求、配方替换和服务器重算 | 未验证；反序列化实现体未展开 |
| 自动开门 | Player 冲刺、NPC 路径、锁门、机关强制关门和碰撞 | 未验证；DoorAutoHandler 实现体未展开 |

新增证据缺口：

1. `ExtraSpawnPointManager.GenerateExtraSpawns_TryFindSpawnRandomly` 和回退算法当前为空体；需补充完整随机选择和失败时的确定性回退证据。
2. `QuickStacking.QuickStackToNearbyChests`、`ReadNetInventory`、`WriteBlockedChestList` 当前为空体，无法仅凭字段推断服务器授权和部分转移行为。
3. `CraftingRequests.NetCraftingRequestsModule.Deserialize` 当前为空体；必须确认请求队列何时入队、何时消费，以及结果/消耗字段是否只作客户端提示。
4. `DoorOpeningHelper` 的两个 `DoorAutoHandler` 实现方法当前为空体；需补证门类型、锁和碰撞检查的完整语义。

这些系统的组件边界已足够支持架构分层，但上述空体仍禁止直接迁移；应先从完整同版本实现或可运行行为取得证据，再建立 focused verifier。

## 30. 第六轮细查：双地牢墙、入侵贡献、环境伤害和玩家休息状态

### 30.1 DualDungeonBoundaryQualification：不可破坏墙与双地牢边界

**证据**：`Terraria.GameContent/UnbreakableWallScan.cs:7-26` 定义网络模块、扫描距离和八方向射线；`:29-51` 通过八方向 `LineScan` 判断玩家是否被不可破坏墙包围；`:54-76` 检查墙类型 350 和墙漆层级；`Player.cs:2208` 保存 `insideUnbreakableWalls`，`:17741-17758` 按位置/周期重扫并在状态变化时广播；`NPC.cs:317`、`:5384`、`:50006` 将该状态用于双地牢刷怪、玩家目标过滤和 AI 资格；`WorldGen.cs:26128-26130`、`NPC.cs:65282-65298` 按 Boss 进度清除墙漆层级；`Utils.cs:416` 将边界纳入传送安全检查。

| 目标构件 | 状态类别 | 设计边界 |
| --- | --- | --- |
| `UnbreakableWallBoundaryState` | 墙层级、玩家是否在封闭区域 | 世界/玩家权威状态 |
| `BoundaryScanCache` | 上次中心、扫描冷却和八方向命中掩码 | 可失效缓存，不是第二份墙状态 |
| `DualDungeonQualificationQuery` | 墙漆层级、距离、难度和传送安全 | 纯 Query |
| `UnbreakableWallProgressionSystem` | 按 Boss 进度清除墙、重扫玩家和发布变化 | 世界/玩家 System |
| `BoundaryReplicationProjection` | 玩家状态变化和网络模块广播 | 网络投影 |

该边界不能归入普通 Biome：它改变刷怪表、玩家可见/可达区域、传送落点和双地牢事件资格。墙本身属于 TileMap，`insideUnbreakableWalls` 是玩家对墙的派生快照；实现时必须明确“墙漆层级由世界拥有，玩家内外状态由扫描 System 发布”，避免玩家状态反向修改 Tile。

### 30.2 InvasionProgressAndCredit：常规入侵波次、积分与伤害贡献

**证据**：`Main.cs:12542-12692` 的 `UpdateInvasion` 管理入侵类型、延迟、剩余规模、方向和开始/结束；`Main.cs:11807-11846` 将雪月、南瓜月和常规入侵的波次/规模进度投影到网络进度条；`NPC.cs:5937-5939` 保存 `totalInvasionPoints` 与 `waveKills`；`NPC.cs:64763-64796` 根据入侵组累加并报告进度；`NPC.cs:65069-65088`、`:65200-65218` 处理波次击杀和下一波切换；`Main.cs:7444-7462`、`:12682` 在入侵开始时启动 `InvasionDamageTracker`；`Terraria.GameContent/InvasionDamageTracker.cs:6-45` 按入侵组筛选贡献者。

| 目标构件 | 状态类别 | 设计边界 |
| --- | --- | --- |
| `InvasionRuntimeState` | 入侵类型、位置、延迟、剩余规模和方向 | 世界运行时状态 |
| `InvasionWaveProgressState` | 波次、击杀点数、总积分和完成条件 | 事件状态 |
| `InvasionCreditState` | 玩家/世界伤害贡献和入侵组 | 短期归因状态 |
| `InvasionQualificationQuery` | NPC 是否属于当前入侵、波次条件和结束条件 | 纯 Query |
| `InvasionProgressionSystem` | 开始、刷怪、击杀累加、波次切换和结束 | 世界事件 System |
| `InvasionProgressProjection` | 进度条、方向文本和贡献报告 | 网络/UI 投影 |

该系统应与 `BossProgressionAndCredit` 分开：Boss 归因按 Boss 实例/复合类型结束，入侵积分按波次和入侵组推进；两者虽然都使用 `NPCDamageTracker`，但持久化语义、结束条件和网络进度不同。`InvasionDamageTracker.IncludeDamageFor` 在当前版本为空体，故贡献筛选细节标为 `partial`。

### 30.3 DontStarveDarknessDamage：特殊世界黑暗伤害规则

**证据**：`Terraria.GameContent/DontStarveDarknessDamageDealer.cs:8-35` 保存黑暗计时器、命中计时器、提示状态和上一帧亮度，并提供世界重置；`Main.cs:1288` 保存 `disableDontStarveDarknessDamage`，`:12325-12348` 根据世界规则切换该开关；`WorldGen.cs:6415` 在世界清理时重置黑暗伤害状态；`SceneMetrics` 的光照/区域扫描与玩家位置是候选亮度输入，但当前 Version4 检索尚未找到完整的伤害调用链，因此“实际由谁消费计时器”必须标为证据缺口。

| 目标构件 | 状态类别 | 设计边界 |
| --- | --- | --- |
| `DarknessExposureState` | 当前过暗、连续暴露帧和命中冷却 | 玩家/世界运行时状态 |
| `DarknessRuleInput` | Dont Starve 规则、禁用开关和光照快照 | 只读规则输入 |
| `DarknessDamageQualificationQuery` | 亮度、区域、免疫和计时器是否达到伤害门槛 | 纯 Query |
| `DarknessDamageSystem` | 累加暴露、施加环境伤害、提示和重置 | 环境规则 System（当前为 `partial`） |
| `DarknessFeedbackProjection` | 屏幕提示、音效和视觉遮罩 | 表现投影 |

黑暗伤害不是普通 `CombatAndStatus` 的一个 Buff：候选伤害来源是世界光照与特殊种子规则，且具有独立的连续暴露计时和世界清理生命周期。表现层可以显示“太暗”提示，但不能自行推进命中计时器。由于当前实现只暴露重置和状态字段，不能把“已施加环境伤害”当作 confirmed 行为。

### 30.4 PlayerRestAndStacking：坐席、睡眠和玩家锚定关系

**证据**：`Terraria.GameContent/PlayerSittingHelper.cs:6-100` 保存坐席状态、座位偏移、堆叠索引和厕所标志，`UpdateSitting` 检查 Tile、输入、坐席数量和 NPC 特殊行为；`PlayerSleepingHelper.cs:7-147` 保存睡眠状态、完全入睡计时、床偏移和堆叠索引，按输入、装备、床 Tile 和中断理由停止睡眠；`Player.cs:2284-2292` 持有 petting/sitting/sleeping/eye helper，`:15726-15727` 每 tick 更新坐席/睡眠，`:19708-19709` 在状态清理时同时解除；`MessageBuffer.cs:720-725`、`NetMessage.cs:466-474` 同步坐席/睡眠位；`Player.cs:3116-3304` 将休息状态纳入交互、位置和绘制偏移。

| 目标构件 | 状态类别 | 设计边界 |
| --- | --- | --- |
| `PlayerSeatRelation` | 目标 Tile、座位偏移、堆叠索引和厕所标志 | 玩家-世界关系状态 |
| `PlayerSleepState` | 床锚点、入睡计时、完全入睡和中断原因 | 玩家能力/生命周期状态 |
| `RestQualificationQuery` | 座椅/床资格、距离、输入、装备和堆叠上限 | 纯 Query |
| `PlayerRestSystem` | 坐下/起身、入睡/唤醒、位置旋转和关系清理 | 玩家交互 System |
| `RestReplicationProjection` | 状态位、旋转/位置同步和客户端表现 | 网络/表现投影 |

坐席和睡眠不能只作为 `PressurePlateAndAnchors` 的附属布尔值：它们有不同的 Tile 资格、堆叠规则、输入中断条件、完全入睡阈值和网络字段，但共享“玩家与世界锚点关系”的交接协议。`DoesPlayerHaveReasonToActUpInBed` 和部分床偏移函数为空体，故睡眠中断语义保留为 `partial`。

## 31. 第六轮新增系统的交接与验证

```text
TileMap / WorldRuleSet
  └─► UnbreakableWallProgression ──► BoundaryScan ──► NPC spawn / teleport qualification

WorldClock ──► InvasionProgression ──► InvasionCredit ──► wave / progress projections

SceneMetrics + DontStarve rule
  └─► DarknessDamageQualification ──► DarknessDamageSystem ──► Combat resolution

Player input + chair/bed Tile
  └─► RestQualification ──► PlayerRestSystem ──► Player anchor + network state
```

新增不变量：

1. 不可破坏墙的世界漆层级是唯一权威；玩家 `insideUnbreakableWalls` 必须可由扫描重算，不能作为墙状态写入源。
2. 入侵波次积分、Boss 复合类型归因和玩家普通伤害必须使用不同的结束事件与归因键。
3. 黑暗伤害只由服务器/权威环境 System 根据光照快照和规则输入提交，客户端提示不能造成伤害。
4. 坐席/睡眠关系失效时必须同时清理锚点、堆叠索引、旋转/偏移和网络状态。

| 验证目标 | 最小场景 | 结果状态 |
| --- | --- | --- |
| 双地牢边界 | 墙漆升级、玩家跨界、刷怪/传送和网络广播 | 未验证 |
| 入侵贡献 | 多入侵组、波次击杀、玩家/世界伤害和结束重置 | 未验证；筛选实现体未展开 |
| 黑暗伤害 | 光照阈值、连续暴露、禁用规则和死亡/重置 | 未验证；主伤害调用链需补证 |
| 坐席/睡眠 | 多人堆叠、输入中断、床删除、装备使用和重连 | 未验证；部分中断函数未展开 |

本轮确认的空体证据缺口不会改变系统边界判断，但在实现前必须补齐对应写者、存档/网络字段和 focused verifier。

## 32. 子系统识别覆盖率估算（调查口径）

本节回答“当前已经找到多少子系统”，不表示代码迁移或实现完成率。统计基准为本报告截至第六轮的 Version4 源码调查结果。

### 32.1 设计边界口径

- 第六轮基线报告列出 **47 个独立设计边界**（第 6、15、19、24、27、30 节的小节合计）。
- 第七轮又确认约 **13 个可独立调度的顶层边界**，另有约 9 个属于已有系统下的规则族/Query/Projection；因此当前累计约 **60 个独立边界**（加上细分规则族约 69 个可替换单元）。
- 结合已扫描的 `Terraria`、`Terraria.DataStructures`、`Terraria.GameContent.*`、`Terraria.WorldBuilding`、事件/交互/创意/掉落规则等目录，以及仍未完全展开的空体证据，预计 Version4 的“应拆分权威模拟边界”约为 **68～76 个**。
- 因此第七轮后的子系统识别覆盖率估算为：**60 / 68～76 ≈ 79%～88%**；若把规则族也按独立替换单元计入，则覆盖面约为 **80%～90%**。
- 该区间的主要不确定性来自事件调度的细分粒度、TileEntity 类型族、掉落/钓鱼规则是否作为独立运行时边界，以及 `Main`/`Player`/`NPC`/`WorldGen` 巨型类中尚未逐方法归属的规则。不能把 47 个边界解释为最终数量，也不能用它衡量迁移完成度。

### 32.2 Version4 源码扫描口径

| 口径 | 文件 | 行数 | 说明 |
| --- | ---: | ---: | --- |
| Version4 全部 C#（含 `obj`） | 993 | 431,924 | 原始扫描总量，含生成/中间目录，不适合作为迁移分母 |
| Version4 源码 C#（排除 `obj`） | 967 | 407,726 | 推荐的源码基线 |
| `Terraria*` 权威/规则候选目录（排除明显 UI、图形、音频、社交等目录） | 833 | 400,387 | 代码行主要集中在核心巨型类；目录仍包含定义、工具和适配器，不能视为已拆分代码 |

按源码基线计算，当前权威/规则候选扫描面约占 **86.1% 的文件、98.2% 的行数**。这反映“阅读和证据覆盖面”，不是“组件已经实现的比例”；核心类共享多个边界，按文件或行数切分会重复计数。

### 32.3 三种比例的结论

1. **子系统识别覆盖率：约 79%～88%**（第七轮后的当前估算；对外可四舍五入为“约八成到九成”）。
2. **权威代码证据扫描面：约 86% 文件 / 98% 行数**（排除 `obj`、按候选权威目录估算）。
3. **拆分实现完成率：0%**（本报告只完成设计、证据归属和交接契约，未将 Version4 代码迁移为新组件）。

剩余 15%～25% 的识别工作应优先继续检查 `Terraria.GameContent.Events`、`Tile_Entities`、`ObjectInteractions`、`Creative`、`ItemDropRules`、`FishDropRules`、`Generation` 及特殊种子/环境辅助类，并对当前标记为 `partial` 的空体补齐写者、生命周期、存档和网络证据。

## 33. 第七轮细查：事件、TileEntity、交互、掉落、生成与特殊种子

本轮直接阅读 `D:\TRbackup\Version4` 中用户点名的目录。以下边界只在存在独立状态、独立写入原因或独立生命周期时列为权威模拟系统；纯 UI、绘制、音效和临时缓存仍排除。它们不引用 `D:\TRbackup\NLTX\src` 中已有组件。

### 33.1 `Terraria.GameContent.Events`：事件状态族

| 新边界 | Version4 证据 | 状态判断 | 拆分方向 |
| --- | --- | --- | --- |
| `CultistRitualEvent` | `CultistRitual.cs:7-126`，`UpdateTime`、`TrySpawning` | `confirmed`；有倒计时、地牢区域资格和生成写入 | `CultistRitualState`、`CultistSpawnQualificationQuery`、`CultistRitualSystem` |
| `DungeonCurseEvent` | `DangerousDungeonCurse.cs:5-58` | `confirmed`；地牢诅咒资格与事件触发边界独立 | `DungeonCurseState`、`DungeonCurseQualificationQuery` |
| `BirthdayPartyEvent` | `BirthdayParty.cs:11-186`，NPC 庆祝集合、时间更新和网络广播 | `partial`；`CanNPCParty` 等方法存在空体证据 | `PartyParticipantState`、`BirthdayPartySystem`、`PartyNetworkProjection` |
| `LanternNightEvent` | `LanternNight.cs:6-96`，`LanternsCanStart`、`UpdateTime` | `partial`；自然启动方法未完全实现 | `LanternNightState`、`LanternStartQualificationQuery`、`LanternNightSystem` |
| `SandstormWeatherEvent` | `Sandstorm.cs:8-127`，严重度、`UpdateTime`、网络广播 | `partial`；`StartSandstorm`/`StopSandstorm` 为空体 | `SandstormState`、`SandstormQualificationQuery`、`SandstormSystem` |
| `MysticLogFairiesEvent` | `MysticLogFairiesEvent.cs:9-102`，世界/夜晚启动和 `UpdateTime` | `confirmed`；有世界与夜晚两个生命周期入口 | `FairyEventState`、`FairySpawnSystem` |
| `DungeonDefenders2Event` | `DD2Event.cs:15-652`，保存/加载、波次、积分、伤害追踪和入侵网络 | `partial`；`DropStarterCrystals` 等空体，部分逻辑需补证 | 独立于普通入侵的 `DD2EventState`、`DD2WaveSystem`、`DD2DamageCredit` |

`MoonlordDeathDrama` 和 `ScreenObstruction` 只产生爆炸片段、屏幕遮挡等表现/演出状态，本轮不列为权威模拟子系统。

### 33.2 TileEntity 运行时族

`TileEntitiesManager` 的注册表只提供宿主；具体 TileEntity 必须按其状态和更新职责拆开：

| 新边界 | 代码证据 | 权威状态 |
| --- | --- | --- |
| `LogicSensorRuntime` | `TELogicSensor.cs:9-401`，`Update`、玩家盒体缓存、触发点队列、`Wiring.HitSwitch` | 传感器类型、开关状态、触发队列和删除提交；缓存不可成为权威 |
| `TrainingDummyCombatProbe` | `TETrainingDummy.cs:8-145`，`Update` 和命中统计字段 | 假人激活、伤害/命中采样和网络更新；属于战斗观测而非普通 NPC |
| `TeleportationPylonEntity` | `TETeleportationPylon.cs:5-154`，类型检查和放置资格 | 位置绑定、Pylon 类型和放置合法性；与传送网络 System 通过只读投影交接 |
| `DisplayEquipmentEntity` | `TEDisplayDoll.cs:14-475`、`TEHatRack.cs`、`TEWeaponsRack.cs`、`TEFoodPlatter.cs` | 展示槽位、装备/姿态、Item 序列化和破坏返还 |
| `ItemFrameEntity` | `TEItemFrame.cs:6-97` | 单 Item 绑定、前缀/堆叠和放置/取回事务 |
| `CritterAndKiteAnchorEntity` | `TECritterAnchor.cs:7-77`、`TEKiteAnchor.cs:7-45`、父类 `TELeashedEntityAnchorWithItem` | 锚定实体、关联 Item 和解除/销毁关系；与 `LeashedEntitySimulation` 共享关系协议但不共享存储 |
| `DeadCellsDisplayJarEntity` | `TEDeadCellsDisplayJar.cs:6-70` | 展示对象有效性、放置/取回和加载修复；`FixLoadedData` 为空体，加载修复为 `partial` |

`LogicSensorRuntime` 和 `TrainingDummyCombatProbe` 是本轮最明确的新增 Tick System；展示类实体则共享 `TileEntityItemStorage` 最小协议，不应合并成一个包含所有姿态字段的巨型组件。

### 33.3 智能交互与交互资格

`Terraria.GameContent.ObjectInteractions` 的核心不是 UI，而是从玩家位置和世界对象中筛选可执行交互：

- `SmartInteractQualificationSystem`：`SmartInteractSystem.cs:5-16` 作为扫描编排入口；
- `TileInteractionCandidateQuery`：`TileSmartInteractCandidateProvider.cs`，读取 Tile/锚点并产生候选；
- `NpcInteractionCandidateQuery`：`NPCSmartInteractCandidateProvider.cs`；
- `ProjectileInteractionCandidateQuery`：`ProjectileSmartInteractCandidateProvider.cs`；
- `PotionReturnGateQualification`：`PotionOfReturnGateInteractionChecker.cs` 与候选提供器，输出传送请求而不是直接改变玩家位置；
- `InteractionBlockReasonProjection`：`BlockBecauseYouAreOverAnImportantTile.cs`，只输出阻止原因，不拥有 Tile 状态。

扫描设置和候选列表属于派生帧快照；被选中的交互必须转换为 `InteractionIntent`，再由门、传送、NPC 服务或物品 System 提交权威变化。

### 33.4 创意模式：研究进度与世界/玩家能力

| 新边界 | 代码证据 | 状态分类 |
| --- | --- | --- |
| `CreativeResearchProgress` | `CreativeItemSacrificesCatalog.cs:8-87`、`ItemsSacrificedUnlocksTracker.cs:8-35` | 研究/牺牲解锁属于知识进度；当前 `Save/Load/Reset` 多为空体，整体为 `partial` |
| `CreativePowerRuntime` | `CreativePowerManager.cs:8-186`、`CreativePowers.cs:17-737` | 权力 ID、权限等级、世界持久化能力和玩家能力覆盖；管理器的注册、重置、世界存档路径已确认 |
| `CreativePowerCommandBoundary` | `ICreativePower.cs:7-21`、`CreativePowerManager.TryGetPower` | 网络反序列化和权限检查必须是 Command/Adapter，不可让 UI 直接写世界或玩家状态 |

`SortingSteps`、`ItemFilters`、`CreativePowerUIElementRequestInfo` 仅为目录/界面查询，不进入权威模拟层。每个具体 Power 应作为策略实现挂到 `CreativePowerRuntime`，而不是复制成 Player 的永久字段。

### 33.5 掉落与钓鱼规则运行时

#### ItemDropRules

`ItemDropDatabase.cs:7-150` 注册 NPC/全局规则，`ItemDropResolver.cs:5-14` 执行规则，`CommonDrop.cs:5-61` 和 `Chains.cs:7-127` 体现随机、链式结果，`CommonCode.cs:7-80` 将结果转为世界 Item/金币命令。因此新增：

- `ItemDropRuleCatalog`：只读 NPC/全局规则图和链式规则定义；
- `DropQualificationQuery`：难度、条件、玩家参与和来源资格判断；
- `ItemDropRollSystem`：使用显式随机源执行 `TryDroppingItem`，产生 `LootRollResult`；
- `DropAttributionAndDistributionSystem`：处理按玩家、按交互者、本地客户端和金币归零等分配语义；
- `DropSpawnProjection`：把结果转换为 `WorldItemSpawnCommand`，不能回写规则目录。

`CommonDropNotScalingWithLuck`、`CommonDropScalingWithOnlyBadLuck` 的 `TryDroppingItem` 空体使对应运气修正保持 `partial`；不能按类名补写概率。

#### FishDropRules

`FishingContext.cs:6-26` 保存一次尝试的随机源和上下文，`FishDropRuleList.cs:6-...` 管理规则列表，`GameContentFishDropPopulator.cs:3-...` 建立游戏内容规则，`FishRarityCondition.cs` 和 `FishingConditions.cs` 提供资格条件。因此将已有钓鱼实体状态与规则运行时分离为：

- `FishingAttemptContext`（一次尝试的环境、鱼饵、稀有度和随机源）；
- `FishDropRuleCatalog`（按环境/世界规则的只读规则表）；
- `FishDropQualificationQuery`（水体、熔岩、蜂蜜、环境和稀有度资格）；
- `FishDropRollSystem`（从规则表产生渔获结果）；
- `FishCatchProjection`（生成 Item/任务进度/网络结果）。

### 33.6 Generation、特殊种子与环境辅助

#### Generation Pass Pipeline

`WorldGen.GenerateWorld`（`WorldGen.cs:10108-10126`）创建生成流程，`AddGenerationPass`（`:9175-9176`）登记阶段；`:10557-16966` 及后续代码按 Terrain、洞穴、生态、地牢、神庙、箱子、液体和清理阶段写入 TileMap。新增边界为：

- `GenerationPassScheduler`：阶段顺序、进度、失败回退和生成屏障；
- `TerrainAndCaveGenerationSystem`：地形、洞穴、矿物和液体初始形态；
- `DungeonStructureGenerationSystem`：`DungeonCrawler`、双地牢和结构边界；
- `WorldObjectGenerationSystem`：箱子、房屋、神庙、生命水晶和特殊结构；
- `GenerationPostProcessSystem`：帧更新、液体沉降、草/墙传播和清理。

空体 `AddGenerationPass` 只能说明当前反编译实现缺失登记细节；不能据此删除生成阶段，必须以调用顺序和 `GenerationPass` 消费者补证。

#### Secret seed 与 Skyblock 规则

`WorldGen.cs:340-410` 注册大量 `SecretSeed`，`:489-586` 完成输入、启用、禁用和初始化；`:693-3141` 执行种子对世界生成的写入；`Skyblock`（`:3141-3286`）维护禁止生成、Tile/Wall 扫描和活动 Tile 计数。新增：

- `SecretSeedSelectionState`：输入匹配、启用集合、活动计数和变体组合；
- `SecretSeedRuleEvaluationQuery`：运行时资格、生成分支和规则覆盖；
- `SecretSeedWorldMutationSystem`：颜色/墙体、地表、液体、传送器、感染和出生规则写入；
- `SkyblockGenerationConstraintSystem`：禁止生成集合、Tile/Wall 扫描和生成器约束；
- `DontStarveEnvironmentRuleSystem`：`DontStarveSeed.cs:7-15`、`SpecialSeedFeatures.cs:3-107` 与 `WorldGen.UpdateWorld` 的浇水/黑暗规则交接。

`SecretSeedsTracker.cs:6` 目前只有空壳，属于 `partial`；实际权威字段仍在 `WorldGen.SecretSeed` 和 `Main` 的世界规则字段中。`SpelunkerProjectileHelper.cs:6-34` 仅维护待检查点并在投射物更新前处理，适合作为 `SpelunkerSpotProjection`；`VoidLensHelper.cs:11-74` 只产生镜头粒子/透明度，排除出权威模拟。

#### 生态扩散细分

`WorldGen.UpdateWorld`（`WorldGen.cs:59400` 起）同时处理草、藤蔓、树、感染、液体、风暴闪电和城镇检查。除了已有 `WorldEcologyAndSpread` 总边界，本轮确认可以进一步拆为：

- `GrassAndPlantGrowthSystem`：`SpreadGrass`、植物检查、树苗/藤蔓生长；
- `InfectionConversionSystem`：腐化/猩红/神圣扩散与 `CanEvilReplace` 资格；
- `LiquidAndWeatherWorldSystem`：液体检查、降雨安全、风暴闪电和环境水分；
- `TownHousingReevaluationSystem`：住房重评估和附近房屋检查。

这些是 `WorldEcologyAndSpread` 的可替换子系统，不重复计算为四个独立世界会话；共享 `WorldTick`、TileMap 快照和 `SecretSeedRuleSet` 只读输入。

### 33.7 本轮新增边界数量与证据状态

- 新识别的候选边界：约 **22 个**；
- 其中可直接成为顶层调度 System 的边界：约 **13 个**；
- 其余为已有系统下的规则族、Query、Projection 或 TileEntity 专用子系统；
- 明确保持 `partial` 的项目：BirthdayParty、LanternNight、Sandstorm、DD2 部分逻辑、TileEntity 加载修复、Creative 研究存档、两类掉落运气修正、SecretSeedsTracker、部分 Generation pass 登记以及完整 DontStarve 伤害提交链。

本轮不把空体方法当作“无行为”，而是把它们保留为证据缺口；在实现前应为每个 `partial` 边界补充 focused verifier，验证状态写者、世界/玩家存档、网络投影和失败回退。

## 34. 第八轮细查：知识账本、环境实体、物品变体与来源协议

本轮继续阅读未充分展开的 `Bestiary`、`Ambience`、`Items`、`NetModules`、`DataStructures` 和地牢生成数据。结论重点是：有些目录看起来像“内容定义”，但其中的 Tracker、Update、Save/Load 和网络反序列化已经形成独立运行时边界；反之，纯排序、绘制和缓存不应被虚构为子系统。

### 34.1 Bestiary：知识账本与遭遇记录

`BestiaryUnlocksTracker.cs:5-59` 将击杀、看见和对话三个持久 Tracker 组合并提供世界存档、加载、校验、重置和玩家加入同步；具体写入口分别为：

- `NPCKillsTracker.cs:10-110`：`RegisterKill`、保存/加载和重置；
- `NPCWasNearPlayerTracker.cs:11-145`：`RegisterWasNearby`、保存/加载和重置；
- `NPCWasChatWithTracker.cs:10-105`：`RegisterChatStartWith`、保存/加载和重置；
- `NetBestiaryModule.cs:7-41`：击杀数、目击和对话三种网络消息。

因此将原有 `BestiaryProgressPersistence` 细分为：

| 构件 | 所有权 | 类型 |
| --- | --- | --- |
| `BestiaryKnowledgeLedger` | 世界级击杀/目击/对话知识 | 权威持久状态 |
| `BestiaryEncounterCommand` | NPC 击杀、接近、对话事件 | 显式 Command |
| `BestiaryUnlockQualificationQuery` | 根据知识账本计算解锁状态 | 纯 Query |
| `BestiaryReplicationAdapter` | `NetBestiaryModule` 消息编解码 | Adapter/Projection |

`BestiaryDatabase`、InfoElement、Filter 和 SortingSteps 只提供内容目录或 UI 查询，不拥有知识进度。`FillBasedOnVersionBefore210` 为空体，版本迁移规则保持 `partial`。

### 34.2 环境实体：服务器决定、客户端表现

`Terraria.GameContent.Ambience.AmbienceServer.cs:9-220` 维护环境实体生成间隔、强制生成队列、天气/时间/难度条件、玩家高度和生物群系过滤，并通过 `NetAmbienceModule` 广播生成。因此可建立低耦合的：

- `AmbientSpawnBudgetState`：下一次尝试倒计时和强制生成队列；
- `AmbientSpawnQualificationQuery`：天气、昼夜、难度、高度和 Zone 条件；
- `AmbientEntitySpawnSystem`：选择玩家和 `SkyEntityType`，提交生成命令；
- `AmbientReplicationProjection`：`NetAmbienceModule` 只发送生成结果。

该系统属于“服务器权威的环境实体生成”，但其实体通常是背景/氛围表现，不应并入 NPC 战斗状态。`NetParticlesModule`、`NetTextModule` 以及粒子/文字本身仍是表现适配器。

### 34.3 Items：物品变体与鞭子标签效果

#### Item variant qualification

`ItemVariants.cs:5-141` 按物品 ID 保存变体条目和条件，`SelectVariant` 依次检查 `RemixWorld`、`GetGoodWorld`、`MechdusaWorld` 等世界规则后返回变体；这不是 Item 实例库存状态，而是内容规则查询。新增：

- `ItemVariantCatalog`：只读物品—变体—条件图；
- `ItemVariantQualificationQuery`：当前世界/种子条件下选择变体；
- `ItemVariantProjection`：将选择结果应用到物品默认值或 UI 描述，不反向修改世界规则。

#### Whip tag effect runtime

`TagEffectState.cs:8-235` 维护玩家当前标签类型、每 NPC 标签剩余时间、Proc 剩余时间，并在 `Update` 中倒计时，在 `TryApplyTagToNPC`、`TryEnableProcOnNPC`、`ModifyHit`、`OnHit` 中参与战斗；内部网络模块还定义完整状态和 NPC 变化消息。因此新增：

- `WhipTagEffectState`：玩家当前标签和按 NPC 的时间表；
- `WhipTagEffectQualificationQuery`：标签能否应用、是否可 Proc；
- `WhipTagEffectSystem`：倒计时、命中修改和 Proc 消耗；
- `WhipTagEffectReplicationAdapter`：状态/变更消息编解码。

`TagEffectState.NetModule.WriteNPCChange`、`Deserialize`、`ApplyTagToNPC`、`EnableProcOnNPC` 和 `Clear` 存在空体证据，故网络写入和标签应用链为 `partial`，不能假定客户端消息一定会改变服务器状态。

### 34.4 DataStructures：实体来源和交互锚点协议

`Terraria.DataStructures` 中大量 `EntitySource_*` 类型不是独立玩法系统，而是跨系统的来源协议。已确认的来源包括：

- `EntitySource_Loot`、`EntitySource_FishedOut`：掉落/渔获归因；
- `EntitySource_ItemUse`、`EntitySource_ItemUse_WithAmmo`：物品使用和弹药链；
- `EntitySource_OnHit_*`：命中来源；
- `EntitySource_WorldEvent`、`EntitySource_WorldGen`、`EntitySource_RevengeSystem`：世界事件、生成和复仇标记；
- `EntitySource_TileBreak`、`EntitySource_TileInteraction`、`EntitySource_Wiring`：Tile/机关写入来源。

这些类型应统一收敛到 `EntityOriginAttributionProtocol`：

```text
来源实体/世界事件
  └─► IEntitySource（不可变来源描述）
       └─► Spawn / Loot / Damage / Tile Command
```

来源对象不能承载可变 Health、位置或生命周期；它们也不能被当作网络 ID、槽位 ID 或存档 ID。`PlayerInteractionAnchor.cs` 和 `PlacementDetails.cs` 则分别作为交互关系和放置命令的输入 DTO，不拥有最终 Tile/玩家状态。

### 34.5 地牢生成数据与保护边界

`Terraria.GameContent.Generation.Dungeon` 的 `DungeonCrawler.cs:34-741` 提供地牢数据初始化、生成变量、房间/大厅/入口构造和 `MakeDungeon`；`DungeonUtils.cs:15-195` 提供潜在边界、进度和保护判断；`DungeonData.cs:12-49` 保存一次地牢生成的数据集合。它们应从通用 `TerrainAndCaveGenerationSystem` 中抽出：

- `DungeonGenerationContext`：当前地牢、侧别、样式和生成变量；
- `DungeonStructurePlacementSystem`：房间、入口、大厅、平台和门的结构提交；
- `DungeonProtectionQualificationQuery`：不可破坏墙、潜在边界和保护类型判断；
- `DungeonGenerationProjection`：生成进度和网络/调试进度输出。

`DualDungeonUnbreakableWallTiers.cs` 与已有 `DualDungeonBoundaryQualification` 共享保护规则，但墙层级仍由世界规则拥有；地牢生成数据不能反向成为运行时玩家边界状态。

### 34.6 NetModules 的边界审查

`NetCreativePowersModule`、`NetCreativePowerPermissionsModule`、`NetCreativeUnlocksPlayerReportModule`、`NetTeleportPylonModule`、`NetLiquidModule` 和 `NetBestiaryModule` 都是外部协议适配器，不应重复创建一套网络权威状态。它们的统一拆分方式是：

- `InboundModuleCommandAdapter`：读取用户 ID、权限和消息类型，输出 typed Command；
- `SnapshotReplicationProjection`：从权威组件生成包；
- `ProtocolIdentityMapper`：网络 ID、TileEntity ID、玩家槽位与领域实体 ID 显式映射；
- `MalformedPacketRejection`：反序列化失败返回错误，不绑定到其他实体。

`Deserialize` 空体的方法（例如 `NetBestiaryModule.cs:41`、`NetCreativePowersModule.cs` 对应入口）只表示当前反编译证据不完整，不能推断“网络同步没有行为”。

### 34.7 第八轮新增与当前覆盖率

- 新识别的候选边界：约 **14 个**；
- 可作为独立调度 System 的新增边界：约 **6 个**；
- 其余属于已有 Bestiary、ItemGameplay、RandomnessAndAttribution、WorldGeneration 或 NetworkSession 边界下的专用组件/Query/Adapter；
- 当前累计独立边界约 **66 个**，可替换单元约 **78 个**；
- 结合尚未完全展开的事件细节、空体网络处理和规则分支，完整权威边界预计约 **74～84 个**，识别覆盖率约 **79%～89%**。

本轮仍保持 `partial` 的重点包括：Bestiary 版本迁移、WhipTagEffect 网络/标签应用、多个 NetModule `Deserialize`、环境实体客户端投影和地牢生成阶段登记。所有这些都需要 focused verifier 后才能进入实现迁移阶段。

## 35. 第九轮：沿未闭合调用链发现的潜在子系统

本轮不再按目录扩展，而是反向从 P0/P1 空体的调用者继续追踪 `WorldGen`、`Player`、`NPC` 和 `Achievement` 的消费者。以下项目是“尚未完全展开但已经显示独立状态或副作用”的潜在边界；它们暂不计入 `confirmed` 系统数量。

### 36.1 世界危险物与环境事件生成

`WorldGen.UpdateWorld` 在 `WorldGen.cs:59418-59524` 依次调用 `UpdateLunarApocalypse`、`SpawnStormLightning` 和 `SpawnFallingObjects`。`SpawnFallingObjects`（`:59580-59798`）同时维护暴雨巨石、陨石、金币雨、流星/星星和 `meteorShowerCount`，并通过 `EntitySource_ByProjectileSourceId` 生成 Projectile 或 WorldItem；`SpawnStormLightning`（`:59799` 起）按玩家安全区和天气条件生成雷击。

应进一步拆为：

- `WorldHazardScheduler`：危险事件倒计时、数量和暂停/快进清理；
- `FallingHazardSpawnSystem`：巨石、陨石、星星和金币雨生成；
- `StormLightningQualificationQuery`：天气、玩家位置、AFK、钓鱼和安全区过滤；
- `LunarApocalypseProgressionSystem`：`TriggerLunarApocalypse` / `UpdateLunarApocalypse`（`:72983-73110`）维护塔状态、护盾和月球灾变结束。

未闭合证据：`SpawnStormLightningNearPlayer`（`:59849`）、`AttemptToGeneratePlanteraBulbAt`（`:61827`）为空体；因此雷击位置选择和 Plantera Bulb 生成仍为 `partial`。这些危险物不能并入普通 ProjectileLifetime，因为它们的生成预算、世界天气资格和来源归因属于世界级状态。

### 36.2 TileBreak、WallBreak 与容器内容掉落

`WorldGen.KillTile_DropItems`（`:52966`）先通过 `KillTile_GetItemDrops`（`:52980-...`）计算主/次掉落，再用 `EntitySource_TileBreak` 生成 WorldItem；墙体路径在 `KillWall_DropItems`（`:52021-52035`）；罐子路径由 `SpawnThingsFromPot`（`:49679-49684`）触发。

这显示出独立的：

- `TileBreakResolutionSystem`：Tile 类型、帧、斜坡和大型对象掉落计算；
- `WallBreakResolutionSystem`：墙体破坏、墙体掉落和来源归因；
- `PotAndObjectBreakSystem`：罐子、坛子、特殊容器和破坏后生成物；
- `TileBreakDropProjection`：将结果转换成带 `EntitySource_TileBreak` 的 Item spawn 命令。

这些系统与 `ItemDropRuleResolver` 不同：ItemDropRules 以 NPC/事件规则图为输入，而 TileBreak 系统以 Tile/Wall 实例、帧和破坏上下文为输入。`KillWall_DropItems`、`SpawnThingsFromPot` 和部分 `KillTile_GetItemDrops` 分支未完全展开，不能假定所有 Tile 采用同一掉落规则。

### 36.3 植物、特殊物件与生态后处理

`WorldGen` 在生成结束和世界 Tick 中调用：

- `MatureTheHerbPlants`（`:38793`）；
- `GrowGlowTulips`（`:38791`）；
- `PlaceChilletEggs`（`:38792`）；
- `GeneratePlanteraBulbOnAllMechsDefeated`（`:61755-61827`）；
- `UpdateWorld_GrassGrowth`（`:60830` 起）。

建议从 `GrassAndPlantGrowthSystem` 再细分：

- `PlantMaturationSystem`：药草成熟和可采集状态；
- `SpecialPlantSpawnSystem`：Glow Tulip、Chillet Egg 等条件生成；
- `BossTriggerGrowthSystem`：机械 Boss 全部击败后 Plantera Bulb 生成资格；
- `GrassWallSpreadSystem`：草、藤蔓和墙体传播。

其中 Plantera Bulb 的最终放置方法为空体，特殊植物方法也没有完整写集，故这些是候选边界而非已确认行为。

### 36.4 Player ItemUse 资源提交

`Player.ItemCheck`（`:23435` 起）在资格判断后调用 `ApplyPotionDelay`、`ApplyLifeAndOrMana` 和 `ApplyReuseDelay`（`:25163-25649`）。这些方法决定药水延迟、生命/魔力资源支付和重复使用冷却，生命周期与 `ItemUseState` 不同，且必须在资格成功后原子提交。

新增候选：

- `ItemUseQualificationQuery`：能否使用、诅咒、冷却、弹药和环境条件；
- `ResourcePaymentSystem`：生命、魔力、药水延迟和物品消费；
- `ReuseCooldownSystem`：重复使用、Channel 和 `reuseDelay`；
- `ItemUseRollbackBoundary`：资格失败或生成失败时不消费资源。

`ApplyPotionDelay`、`ApplyLifeAndOrMana`、`ApplyReuseDelay` 为空体，故不能把资源支付视为已由 `ItemCheck` 完整实现。

### 36.5 NPC 间伤害和专用 AI 行为族

`NPC.GetHurtByOtherNPCs`（`:78500` 附近）完成 NPC 间碰撞筛选后调用空体 `BeHurtByOtherNPC`（`:78577`）；`UpdateNPC` 按 `aiStyle` 分派到 `AI_109_DarkMage`、`AI_110_Betsy`、`AI_111_DD2LightningBug`、`AI_126_StatueMimic`、`AI_127_Pal` 等空体方法（`:41756-41832`、`:42196-42200`、`:52898-52900`）。

应新增但保持 `partial`：

- `NpcFriendlyFireResolutionSystem`：NPC—NPC 命中资格、伤害归因和免疫；
- `DD2NpcBehaviorSystem`：Dark Mage、Betsy、Lightning Bug 的专用阶段；
- `StatueMimicBehaviorSystem`：雕像生成的 Mimic 行为和生命周期；
- `NpcSpecialAiProjection`：只将专用 AI 的网络字段投影到客户端。

这些 AI 不能仅按 `aiStyle` 合并成一个组件；不同 AI 的状态写集、生成来源、网络更新和死亡掉落不同。`BeHurtByOtherNPC` 的空体尤其阻止直接声明 NPC 间伤害已完成。

### 36.6 Player 环境/移动专用规则

以下 Player 方法被真实更新路径调用，但当前为空体：

- `HurtLifeRegen`（`:11238`）：燃烧、窒息、饥饿和生命再生伤害的最终提交；
- `RollerSkateMovement`（`:17697`）：特殊移动能力和碰撞；
- `VampireSeedSunlightExposure`（`:17774`）：特殊种子光照暴露；
- `Spawn_SetPositionAtTeamSpawn`（`:22083`）：出生点选择、队伍出生和安全落点；
- `ApplyPotionDelay` / `ApplyLifeAndOrMana` / `ApplyReuseDelay`：物品资源提交。

这些方法分别指向 `EnvironmentalDamageResolution`、`MovementAbilitySystem`、`SecretSeedExposureSystem`、`SpawnPlacementSystem` 和 `ResourcePaymentSystem`；不能全部并入 `PlayerGameplay`，因为它们的输入来源、命令输出和失败语义不同。

### 36.7 成就持久化与进度事件

`AchievementsHelper` 定义物品拾取、制作、Tile 破坏、NPC 击杀和进度事件通知；`AchievementManager` 保存本地/云端加密文件（`AchievementManager.cs:16-64`），但 `Save(string path, bool cloud)` 为空体；`AchievementTracker<T>` 的 `ReportAs`、`GetTrackerType`、`Clear`、显式 `Load` 和 `OnComplete` 也有空体。

候选边界：

- `AchievementProgressLedger`：完成状态和条件值；
- `AchievementEventAdapter`：拾取、制作、击杀、Tile 破坏和世界进度事件；
- `AchievementPersistenceAdapter`：本地/云存档、加密和版本恢复；
- `AchievementNotificationProjection`：网络通知、提示和客户端 UI。

成就通常不参与服务器战斗权威，但它拥有独立持久化和外部服务边界，不能简单归入 Bestiary。由于存档和 Tracker 方法未展开，整体保持 `partial`，并暂不计入核心世界模拟完成度。

### 36.8 本轮结论

- 沿未闭合调用链新增候选边界约 **18 个**；
- 可独立调度的新增顶层 System 约 **9 个**；
- 高风险空体集中在 `WorldGen`、`Player`、`NPC` 的最终提交函数，而不是目录级辅助类；
- 当前累计独立边界可暂估为 **75 个左右**，但其中约 25 个仍为 `partial` 或未验证；
- 在这些写入链闭合前，不应把识别覆盖率继续上调到“完成”，当前识别覆盖仍建议报告为约 **80%～90%**，而行为闭合度明显低于该数字。

## 36. 未完全展开访问的系统清单

“未完全展开”表示已经发现系统入口或部分读写者，但尚未闭合全部调用链、字段写集、存档/网络协议或行为验证；它不表示该系统不存在。按当前证据分为四级。

### 35.1 P0：存在空体或关键写入链缺失，禁止冻结组件

| 系统/边界 | 未展开部分 | 直接证据 |
| --- | --- | --- |
| Shimmer 转化 | WorldItem 的 `Shimmering`、NPC 的 `GetShimmered` 及旧实体销毁/新实体创建写集 | `WorldItem.cs:625`、`NPC.cs:33293`/`:77658` |
| 沙尘暴 | 启动和停止时的完整世界/天气写入 | `Sandstorm.cs:122-123` |
| Lantern Night | 自然启动资格和事件开始路径 | `LanternNight.NaturalAttempt` |
| Birthday Party | NPC 参加资格和庆祝者集合写入 | `BirthdayParty.CanNPCParty` |
| DD2 入侵 | 部分掉落、结束和伤害贡献路径 | `DD2Event.cs:666` 等空体；与 `InvasionDamageTracker` 交接 |
| 入侵贡献 | 伤害是否计入入侵组、玩家筛选和结束清理 | `InvasionDamageTracker.IncludeDamageFor`、`CheckActive` |
| 黑暗伤害 | 计时器由谁消费并提交实际环境伤害 | `DontStarveDarknessDamageDealer.cs`；尚未找到完整伤害调用链 |
| 坐席/睡眠 | 床中断、偏移和“是否有理由起床”完整规则 | `PlayerSleepingHelper.DoesPlayerHaveReasonToActUpInBed`、`SetOffsetbyBed` |
| 门自动处理 | 各门型开关、锁定、速度触发和延迟关闭 | `DoorOpeningHelper.CommonDoorOpeningInfoProvider`、`TallGateOpeningInfoProvider` |
| 快速堆叠/远程制作 | 服务器重算、锁定、回滚、请求队列和结果消费 | `QuickStacking.*`、`CraftingRequests.NetCraftingRequestsModule.Deserialize` |
| 额外出生点 | 随机选择和失败回退算法 | `ExtraSpawnPointManager.GenerateExtraSpawns_TryFindSpawnRandomly`、`GetFallbackSpawn` |
| 研究/创意解锁 | 牺牲解锁的存档、加载、重置和队友同步 | `ItemsSacrificedUnlocksTracker.cs:31-35`、`CreativeUnlocksTracker.cs:9-12` |
| WhipTagEffect | 标签应用、Proc 启用、清理和 NPC 变化网络包 | `TagEffectState.cs` 中 `ApplyTagToNPC`、`EnableProcOnNPC`、`Clear`、`Deserialize`、`WriteNPCChange` |
| Bestiary 迁移 | 2.1.0 以前数据填充和网络服务器授权 | `BestiaryUnlocksTracker.FillBasedOnVersionBefore210`、`NetBestiaryModule.Deserialize` |
| SecretSeedTracker | 追踪器只有空壳，实际字段与规则组合仍需完整归属 | `SecretSeedsTracker.cs:6` |
| Generation pass 登记 | `AddGenerationPass` 的真实登记、排序和失败行为 | `WorldGen.cs:9175-9176` |

### 35.2 P1：有主要实现，但生命周期或跨边界写集尚未闭合

- `WorldSession`：`Main.invasion*`、`NPC.downed*`/`saved*` 的完整存档写入和加载修复。
- `ProjectileBehavior`：`identity`、`netUpdate`、`netSpam` 的协议作用域和生成/查找关系。
- `ContentCatalog`：初始化后 `Main` 内容数组是否仍有运行时写入点。
- `MovementIntent`：`MessageBuffer` 到 Player 更新的服务端验证、预测和拒绝路径。
- `WorldStorage` / `TileEntityRuntime`：TileEntity、Chest、槽位 ID 的存档/网络作用域。
- `NpcSpawnAiTown`：各 NPC AI style 的共享读写集和特殊 AI 的生命周期。
- `MountAndMinecart`：坐骑配置、玩家 Buff、矿车轨道切换和网络同步。
- `FishingSimulation`：鱼漂咬钩到 `FishingAttempt.rolledItemDrop` 的完整调用链。
- `TeleportationNetwork`：传送冷却、Portal projectile identity、落点碰撞和区段同步。
- `NpcInteractionAndCommerce`：各 NPC 服务扣款、回滚、价格和网络请求分支。
- `TeamAndPvpRelations`：服务端 team/hostile 授权和反作弊边界。
- `PressurePlateAndAnchors`：玩家断开、NPC 死亡、Tile 删除时的解除路径。
- `GolfSimulation`：分数持久化、球 Projectile 复用和球洞结束条件。
- `TilePlacementAndFraming`：所有 `PlaceTile`/`SquareTileFrame` 的批提交和失败回滚。
- `LeashedEntitySimulation`：服务器/客户端更新分支和区段加载阈值。
- `WorldEcologyAndSpread`：Creative 禁止扩散、世界更新率、感染替换和命令批次。
- `RandomnessAndAttribution`：`EntitySource` 是否在所有生成、掉落、伤害路径保留父实体和来源类型。
- `AmbientEntitySpawnSystem`：环境实体生成已确认，但客户端投影和实体结束语义尚未闭合。
- `DungeonStructurePlacementSystem`：地牢结构数据和通用 Tile 写入的边界尚未逐方法核对。

### 35.3 P2：已经识别，但还没有逐方法阅读或 focused verifier

- `Terraria.GameContent.NetModules` 中除关键模块外的全部反序列化授权和错误路径；尤其 `NetLiquidModule`、`NetCreativePowersModule`、`NetTeleportPylonModule`。
- `Terraria.GameContent.Biomes` 的各 MicroBiome、`JunglePass`、`TerrainPass` 与 `WorldGen` 生成阶段的逐 pass 写集。
- `Terraria.GameContent.Generation.Dungeon.Rooms`、`Features`、`Halls`、`Entrances` 和 `LayoutProviders` 的结构约束及失败回退。
- `Terraria.GameContent.Items` 中除 ItemVariant/WhipTagEffect 外的所有 `UniqueTagEffect` 专用命中副作用。
- `MinionRespawner`、`MinionSpawnInfo` 的召唤物恢复、背包来源和失败重生路径。
- `PlayerIntentionGuesser` 的意图推断结果是否进入服务器权威命令，还是仅用于辅助/表现。
- `LootSimulation` 的模拟器是否仅为工具；尚未确认是否有运行时生产者消费其结果。
- `AchievementsHelper` 与成就条件的持久化/网络边界；目前倾向于知识/表现投影，但尚未逐调用点闭合。

### 35.4 明确排除而非“未阅读完成”的代码

以下代码已经检查到足以确认其不是权威模拟状态，因此不列入剩余子系统缺口：

- `ScreenObstruction`、`MoonlordDeathDrama`、`VoidLensHelper`：屏幕、粒子和死亡演出。
- Bestiary 的 InfoElement、Filter、SortingSteps：目录/UI 查询。
- Creative 的 `SortingSteps`、`ItemFilters`、UI 请求结构：排序和界面查询。
- `NetParticlesModule`、`NetTextModule`：表现协议适配器。
- Item/NPC/Projectile 的静态默认定义数组：内容目录，不是实例权威状态。

### 35.5 阅读完成判定

只有同时满足以下条件，系统才可从“未完全展开”改为 `confirmed`：

1. 找到所有关键字段的真实写者和生命周期入口；
2. 闭合世界/玩家/实体/TileEntity 的存档路径；
3. 闭合服务器收包、权限检查和客户端投影路径；
4. 证明空体不是当前行为的唯一实现，或从同版本可执行证据恢复其语义；
5. 运行 focused verifier，验证重复 Tick、失败回滚、网络重放和实体删除后的引用清理。

## 37. 第十轮：从世界生成、传送、钓鱼和实体关系继续闭合

本轮继续从尚未展开的空体和“只有数据/入口、没有结果消费”的调用链反向阅读。新增边界仍按
权威状态、派生查询、命令提交和外部投影区分；没有恢复出完整写集的部分保留为 `partial`，
不把一个 `aiStyle` 或一个文件机械地当作独立顶层系统。

### 37.1 矿车轨道世界生成

`WorldGen.cs:21024-21105` 创建 `TrackGenerator`，分别按照 `LongTrackCount`/`LongTrackLength`
和 `StandardTrackCount`/`StandardTrackLength` 生成两类轨道；长轨和标准轨都受
`errorWorld`、`dualDungeons`、随机起点、失败重试上限和 `DungeonUtils.InAnyPotentialDungeonBounds`
过滤。`TrackGenerator.Place`（`Terraria.GameContent.Generation/TrackGenerator.cs:37-51`）先执行
`FindSuitableOrigin`、`CreateTrackStart`、`FindPath`，成功后调用 `PlacePath`；后四个路径函数
当前有空体或占位返回（`:53-68`）。

这不是普通 Tile 放置的别名，而是带有路径搜索历史（`_history` 4096 项）、重写历史
（`_rewriteHistory` 25 项）、坡度/隧道模式和生成失败预算的结构生成边界。建议拆为：

- `MinecartTrackGenerationContext`：轨道生成种类、长度范围、起点和路径历史；
- `TrackOriginQualificationQuery`：世界边界、地牢保护区和可用 Tile 的纯筛选；
- `MinecartTrackPathSystem`：坡度、隧道、重写和长度约束的路径计算；
- `TrackTilePlacementCommand`：把成功路径转换成批量 Tile/Frame 写入。

当前状态为 `partial`：`Place` 的入口和调用预算已确认，但 `PlacePath`、`FindPath` 和
`FindSuitableOrigin` 没有提供最终 Tile 帧、网络广播或失败回滚证据，不能声称轨道生成已闭合。

### 37.2 双地牢不可破墙扫描与边界状态

`Player.DoUnbreakableWallScan`（`Player.cs:17741-17760`）仅在 `Main.dualDungeonsSeed` 下按
冷却和移动距离重扫，并把结果写入玩家 `insideUnbreakableWalls`；状态变化时调用
`UnbreakableWallScan.NetModule.BroadcastChange`。`UnbreakableWallScan.InsideUnbreakableWalls`
（`Terraria.GameContent/UnbreakableWallScan.cs:29-50`）对八个方向执行 `LineScan`，扫描最多
250 Tile；`LineScan` 在 `:52-77` 读取 `tile.wall == 350` 和墙色阈值。该结果还被
`Utils` 的安全出生/落点筛选（`Utils.cs:398-421`）使用，网络模块在
`NetworkInitializer.cs:24-29` 注册。

应把它从“不可破坏墙生成”中独立出来：

- `UnbreakableWallBoundaryQuery`：纯扫描和方向环判定；
- `PlayerBoundaryExposureSystem`：冷却、上次位置和玩家状态提交；
- `BoundaryStateReplicationAdapter`：只同步状态变化，不接受客户端直接写入；
- `BoundaryAwareSpawnQualification`：出生/传送候选位置过滤。

`NetModule.Deserialize` 和 `BroadcastChange` 为空体，且墙色/网络包格式未恢复，因此扫描
数学判定较接近 `confirmed`，玩家字段生命周期和网络授权仍是 `partial`。它不能与普通
`DungeonProtectionQualificationQuery` 合并：前者是运行时玩家暴露状态，后者是世界生成/破坏
保护边界。

### 37.3 传送落点安全与 Magic Conch 爬行

`TeleportHelpers.FindClosestTeleportSpotNoSpace`（`TeleportHelpers.cs:7-49`）会清零玩家速度，
在玩家附近 50×50 Tile 范围内筛选非危险、无液体伤害、无实体碰撞且有可靠地面的候选点；
`Player.cs:3814` 使用其结果提交位置。`RequestMagicConchTeleportPosition`（`:50-203`）则沿
海洋方向逐格爬行，考虑 Skyblock 低地、固体/液体、危险 Tile、世界表面和最大步数，
由 `Player.cs:26612-26615` 进行左右方向回退。

两个方法共同体现“传送请求”和“落点安全资格”之间的边界，建议补充：

- `TeleportLandingSafetyQuery`：危险 Tile、液体、碰撞体和地面资格；
- `ConchTraversalQualificationQuery`：海洋方向、Skyblock 和爬行预算；
- `TeleportPlacementCommand`：成功候选点的位置/速度提交和同步事件。

`TileIsDangerous`、`IsInSolidTilesExtended`（`:205-210`）为空体，因而传送安全规则、
失败时是否保持原位以及网络拒绝路径仍为 `partial`。这些 Query 只能输出候选结果，不能让
传送辅助类直接拥有 Player 的权威位置。

### 37.4 钓鱼浮标、Chum 和渔获交付

主循环在 `Main.cs:11391` 调用 `ChumBucketProjectileHelper.OnPreUpdateAllProjectiles`，该类
维护本帧/上一帧按 Tile 坐标聚合的 Chum 计数（`ChumBucketProjectileHelper.cs:6-18`）。
`Projectile.Update` 按 `aiStyle == 163` 分派到空体 `AI_163_Chum`
（`Projectile.cs:32791-32794`、`:34072`），说明 Chum 投射物应把影响写入下一帧的钓鱼上下文，
而不是直接修改渔获结果。

钓鱼浮标也有独立的空体入口：`Projectile.cs:25365` 分派 `AI_061_FishingBobber`，定义和
交付函数位于 `:34073-34074`；浮标销毁路径在 `:47779-47787` 根据 `ai[1]` 调用
`AI_061_FishingBobber_GiveItemToPlayer`。`FishingAttempt` 保存水量、鱼力、Chum 数量、稀有度、
任务鱼、`rolledItemDrop` 和 `rolledEnemySpawn`（`Terraria.DataStructures/FishingAttempt.cs:3-40`），
而 `FishDropRuleList` 只验证规则分母（`FishDropRuleList.cs:6-27`），数据库在
`Main.cs:3361-3363` 初始化。

应从已有 `FishingSimulation` 中细分：

- `ChumWaterInfluenceSystem`：Chum 投射物到水域计数的双缓冲提交；
- `FishingBobberStateSystem`：咬钩、浮标计时、取消和 `FishingAttempt` 生成；
- `FishDropQualificationQuery`：生态、稀有度、任务鱼和特殊种子条件；
- `FishingRewardDeliveryCommand`：将 `rolledItemDrop`/敌怪结果交给库存或实体生成；
- `FishingRuleCatalogAdapter`：只读规则目录与版本校验。

当前为 `partial`：`AI_061_FishingBobber`、`AI_163_Chum` 以及渔获交付函数为空体，且
`FishDropsDB` 的运行时消费点没有在 Version4 中找到；不能把规则表初始化当作渔获行为完成。

### 37.5 条件 NPC 对话与服务交互路由

`ConditionalDialogue` 建立按 NPC 类型索引的条件对话注册表（`ConditionalDialogue.cs:55-93`），
`ItemGroups.PostSetupContent`（`:23-42`）按内容目录填充 Whip/Mount 配方组；`NPC` 实例保存
`nextDialogue`（`NPC.cs:6397-6400`）。但 `FreeCakeDialogue.GetChatAndClearCondition` 在
`:44-53` 为空体，Version4 中也没有找到 `nextDialogue` 的结果消费调用。

`NPCInteractions.Initialize`（`NPCInteractions.cs:235-282`）注册 24 个商店入口及税收、护士、
Dryad 净化、Angler 任务、Guide 反向制作、重铸、住房和幸福度报告等交互；各 `Condition`、
`GetText` 和 `Interact` 大多为空体。这里至少存在两个不同边界：

- `ConditionalDialogueQualificationQuery` / `ConditionalDialogueState`：条件满足、一次性清除和提示标记；
- `NpcServiceInteractionCommandRouter`：商店、治疗、税收、任务和住房命令的授权与事务交接。

它们不能被视为纯 UI：护士治疗、税收、重铸和任务交付可能写 Player/NPC/World 状态；但在
空体实现和缺少网络请求消费证据的情况下，两者均保持 `partial`，暂不增加核心完成度。

### 37.6 Boss 伤害账本的具体复合类型边界

`BossDamageTracker`（`Terraria.GameContent/BossDamageTracker.cs:5-85`）并非只有接口壳：
构造函数按自定义 Boss 定义选取主类型（`:27-35`），`IncludeDamageFor` 按
`NPCDamageTracker.BossTypeForMob` 或 `CustomDefinition.NPCTypes` 聚合复合 Boss（`:37-50`），
`CheckActive`/`IsActive` 依据 `NPC.npcsFoundForCheckActive` 停止账本（`:52-78`），击杀时由
`OnBossKilled` 设置 `_killed`（`:80-85`）。

这支持把已有 `BossProgressionAndCredit` 进一步拆成：

- `BossDamageLedger`：按 Boss 复合类型聚合玩家贡献；
- `BossActivityQuery`：活动性与逃逸/离场判定；
- `BossKillCreditCommand`：击杀/逃逸结果和击杀时间消息；
- `BossDefinitionAdapter`：自定义 Boss 类型列表和名称覆盖。

虽然该类的主要规则已实现，但它依赖 `NPCDamageTracker`、`npcsFoundForCheckActive` 和网络
命令消费；因此应标为“主要实现、跨边界 `partial`”，不能与入侵波次贡献账本合并。

### 37.7 Man Eater 保护点与 Tile 破坏防护

`FixExploitManEaters` 每个世界 Tick 由 `Main.cs:11470` 清空保护点；NPC AI 在
`NPC.cs:21459-21477` 对以 Tile 为锚点的 Man Eater 记录 `ProtectSpot`；Tile 破坏在
`WorldGen.cs:52596-52601` 检查 `SpotProtected`，阻止在保护点掉落物路径继续执行。
该列表是短生命周期、按坐标编码的反利用状态，不属于 NPC 永久组件。

建议归入 Tile 破坏边界的独立守卫：

- `ProtectedAnchorRegistry`：当前 Tick 的锚点集合；
- `AnchoredTileBreakGuard`：Tile 掉落前的纯拒绝 Query；
- `AnchorProtectionResetSystem`：Tick 边界清理。

由于 `ProtectSpot` 只记录坐标、没有存档/网络字段，系统深度有限；但它改变 TileBreak 的
可观察结果，不能作为“纯修复注释”删掉。完整的并发、NPC 删除和异常清理语义仍标为 `partial`。

### 37.8 本轮数量与排除项

- 新发现并可命名的候选边界约 **16 个**，其中适合作为独立调度 System 的约 **8 个**；
- 其中 `MinecartTrackPathSystem`、`UnbreakableWallBoundaryQuery`、`TeleportLandingSafetyQuery`、
  `FishingBobberStateSystem` 和 `ChumWaterInfluenceSystem` 是新增证据最强的边界；
- `BossDamageLedger` 是已有 Boss 进度边界的细化，不应重复计数；`NetLiquidModule` 仍只是
  `LiquidNetworkProjection` 的实现证据（其 `SerializeForPlayer`/chunk 分发在
  `NetLiquidModule.cs:36-115`，`Deserialize` 在 `:61-63` 为空体）；
- `AmbientWindSystem`、`LightningGenerator`、`TilePaintSystemV2` 的状态分别服务于墓地风、
  闪电几何和渲染 RenderTarget，不能据此新增权威天气/战斗系统；
- 当前累计独立边界仍约 **83 个左右**，但约 **30 个**存在空体、网络或存档缺口；识别覆盖率
  仍应维持 **80%～90%**，行为闭合度低于识别覆盖率，不能宣称完成。

## 38. 第十一轮：TileEntity 运行时族与锚定对象继续展开

本轮转向 `Terraria.GameContent.Tile_Entities`，重点检查那些已经有 `Update`、Tile 帧写入、
实体槽位或网络/存档字段的类型。结论是：TileEntity 不能只作为 `WorldStorage` 的一个表，
至少需要按“机关传感器”“战斗观测实体”“展示物品槽位”“锚定生物”分出行为边界。

### 38.1 逻辑传感器的跨帧触发系统

`TELogicSensor` 保存 `logicCheck`、`On` 和液体检查迟滞用的 `CountedData`
（`TELogicSensor.cs:33-37`），通过 `RegisterTileEntityID` 把 `UpdateStartInternal` 与
`UpdateEndInternal` 接入 TileEntity 全局更新阶段（`:39-46`）。开始阶段缓存活跃玩家盒体并清理待移除列表
（`:56-84`）；每个传感器依据昼夜、玩家位置或水/岩浆/蜂蜜/液体状态改变 `On`
（`:109-138`）；结束阶段把排队的 `tripPoints` 转为 `Wiring.HitSwitch` 和网络消息，再删除失效
实体（`:86-107`）。

`GetState` 的水体分支还会在状态关闭后递减 `CountedData` 形成短暂保持时间
（`:217-280`），`Kill` 在实体被拆除时可能再次触发线路（`:343-385`）。因此建议细分为：

- `LogicSensorStateComponent`：检测类型、On 状态和迟滞计数；
- `LogicSensorQualificationQuery`：基于时间、玩家盒体与液体的纯状态判断；
- `LogicSensorTripCommand`：跨实体 Tick 阶段排队的开关触发；
- `LogicSensorLifecycleSystem`：Tile 无效、删除和触发后清理；
- `LogicSensorReplicationProjection`：Tile 帧和开关网络消息。

该边界相对明确，但 `OnPlaced` 为空体，且 `Wiring.HitSwitch` 的服务器授权和重复触发
行为未在本轮闭合，整体仍为“主要实现、网络/生命周期 `partial`”。传感器不能并入普通
`PressurePlateSystem`：压力板以玩家移动前后位置为输入，逻辑传感器还读取世界时钟和液体，
并具有不同的跨帧迟滞与删除触发语义。

### 38.2 训练假人激活与战斗观测

`TETrainingDummy` 保存绑定 NPC 槽位 `npc` 和激活重试冷却
（`TETrainingDummy.cs:16-18`），每 Tick 检查 NPC 是否仍是类型 488 且 AI 坐标仍指向该假人，
否则执行 `Deactivate`（`:45-55`）。没有绑定 NPC 时，它缓存活跃玩家盒体，在假人周围扩大
1600 像素范围并决定是否 `Activate`（`:56-79`、`:81-97`）。`Deactivate` 会关闭 NPC 槽位、
清空绑定并发送 TileEntity 网络消息（`:137-148`）；`WriteExtraData`/`ReadExtraData`
显式序列化 NPC 槽位（`:123-135`）。

建议拆为：

- `TrainingDummyBindingState`：TileEntity ID 与 NPC 槽位关系；
- `TrainingDummyActivationQuery`：玩家邻近和 NPC 槽位容量判断；
- `TrainingDummyLifecycleSystem`：激活、重试、失效解绑和 NPC 关闭；
- `TrainingDummyCombatProbe`：将假人命中/伤害观测接入已有战斗账本，但不把假人当普通 NPC；
- `TrainingDummyReplicationAdapter`：槽位和 TileEntity 变化网络投影。

`NetPlaceEntityAttempt` 和 `Activate` 为空体，假人 NPC 的创建、命中统计和重试冷却消费尚未
恢复，因此除绑定/失活路径外均为 `partial`。它不应并入 `NpcSpawnAiTown`，也不应把观测
结果写回玩家或 Banner 权威状态。

### 38.3 展示物品槽位的事务边界

`TEItemFrame` 直接拥有一个 `Item`，并在 `WriteExtraData`/`ReadExtraData` 中序列化类型、
前缀和堆叠（`TEItemFrame.cs:6-57`）。放置已有物品时，如果槽位已有内容先调用 `DropItem`，
再写入新物品并广播 TileEntity 更新（`:70-93`）；拆除时通过 `EntitySource_TileBreak` 生成
WorldItem，并把槽位重置为空物品（`:62-68`）。

这表明展示槽位至少需要：

- `TileEntityItemSlotState`：展示实体拥有的 Item 实例，不等同于玩家背包；
- `DisplayItemPlacementTransaction`：放置、替换、旧物掉落和失败回滚；
- `DisplayItemBreakDropCommand`：拆除时的 TileBreak 来源归因；
- `DisplayItemNetworkProjection`：槽位网络/存档 DTO 和版本兼容。

`TEDisplayDoll` 还保存装备、染料、杂项数组和姿态字节
（`TEDisplayDoll.cs:31-43`），并以位图标记压缩槽位网络数据（`:91-150`）；其静态姿态注册
`RegisterUsePose` 为空体（`:54-60`），故姿态计算不应在拆分设计中假定完成。`TEHatRack`、
`TEWeaponsRack`、`TEFoodPlatter` 与 `TEDeadCellsDisplayJar` 可以复用最小的“物品槽位 +
破坏返还 + 序列化”协议，但装备位置、染料和实体表现必须保留专用适配器，不能合并为一个
包含全部字段的巨型组件。

### 38.4 Critter/Kite 锚定实体的创建资格

`TECritterAnchor` 继承 `TELeashedEntityAnchorWithItem`，通过分配的 TileEntity 类型 ID
创建锚定生物；`Hook_AfterPlacement` 调用 `PlaceFromPlayerPlacementHook`
（`TECritterAnchor.cs:18-23`、`:38-43`），`Kill` 删除指定坐标的实体（`:31-36`）。
但 `IsTileValidForEntity`、`GenerateInstance`、`FitsItem` 均为空体/占位返回
（`:25-29`、`:45-49`），原型集合初始化也被注释（`:51-71`）。

因此新增的只是一个更精确的资格边界，而不是第二套 LeashedEntity 模拟：

- `CritterAnchorPlacementQuery`：Tile、物品类型和原型是否匹配；
- `CritterAnchorEntityFactory`：从槽位 Item 创建 `LeashedEntity` 实例；
- `CritterAnchorLifecycleAdapter`：Tile 删除、实体销毁和物品返还。

该边界完全为 `partial`，并继续复用 `LeashedEntitySimulation` 的实体更新、区段索引和网络
协议；在原型集合和工厂语义恢复前，不能宣称锚定小动物支持已完成。

### 38.5 本轮证据与数量更新

- 新识别的 TileEntity 细化边界约 **12 个**，适合成为独立调度/事务单元的约 **6 个**；
- `LogicSensorRuntime` 和 `TrainingDummyCombatProbe` 是本轮最强的运行时边界；
- `DisplayItemPlacementTransaction` 属于 `WorldObjects`/`TileEntityRuntime` 的事务细化，
  不重复计为新的顶层世界系统；
- `CritterAnchor*` 只扩展 `LeashedEntitySimulation` 的放置和工厂 seam，不新增独立实体
  更新循环；
- 当前累计独立边界约 **89 个左右**，但至少约 **35 个**仍有空体、网络、存档或结果消费
  缺口；识别覆盖率仍估计 **80%～90%**，行为闭合度保持较低，不能冻结组件设计。

## 39. 第十二轮：MessageBuffer 收包命令与跨领域授权边界

本轮对 `Terraria/MessageBuffer.cs` 的消息分派进行逐段阅读。这里不能只归入
`NetworkSessionAndReplication`：同一个消息入口会直接调用 `WorldGen`、`Wiring`、`Chest`、
`Sign`、`Liquid`、`NPC.StrikeNPC`、`Projectile.Kill` 和 `Player` 状态写入。更合适的拆法是
“协议解码 + 领域授权 Query + 领域 Command”，并让网络层只负责身份和包长度/类型边界。

### 39.1 Tile、Wall、Wire 和 Framing 命令入口

消息类型 17（`MessageBuffer.cs:789-942`）读取 Tile 坐标、操作类型、Tile 类型、帧/前缀和
客户端是否认为操作成功；随后按操作码调用 `WorldGen.KillTile`、`PlaceTile`、`KillWall`、
`PlaceWall`、`PlaceWire`/`KillWire`、`PoundTile`、`PlaceActuator`、`SlopeTile`、
`Minecart.FrameTrack`、`Wiring.PokeLogicGate`、`Wiring.Actuate`、`ReplaceTile` 和
`ReplaceWall`。入口还检查世界边界、区段可见性，累加 `SpamDeleteBlock`/
`SpamAddBlock`，并在结构变化后发送 Tile 同步。

这证明需要比现有 `TilePlacementAndFraming` 更靠外的一层：

- `TileMutationPacketDecoder`：解析操作码和坐标，拒绝越界包；
- `TileMutationAuthorizationQuery`：区段可见性、玩家距离、编辑权限和反垃圾计数；
- `TileMutationCommandRouter`：把操作分派到 Tile、Wall、Wire、Actuator、Slope、Track 和
  LogicGate 命令；
- `TileMutationReplicationProjection`：根据实际提交结果而不是客户端标志广播变更。

当前存在高风险缺口：客户端传入的 `num147`/`num148` 会参与放置和帧参数，部分分支没有在
本段代码中看到完整的玩家距离检查；具体权限可能隐藏在被调用的 `WorldGen` 方法中。因而
网络入口的授权和失败回滚为 `partial`，不能把“收到包并调用方法”视为服务器验证完成。

### 39.2 NPC 伤害与 Projectile 生命周期网络命令

消息类型 28（`:1406-1437`）读取 NPC 槽位、伤害、击退和方向，先调用
`NPC.PlayerInteraction(whoAmI)`，再以 `fromNet: true` 调用 `StrikeNPC`；NPC 生命归零后广播
死亡包，并额外检查 `realLife` 主体。消息类型 29（`:1439-1453`）强制把 projectile owner
重写为 `whoAmI`，只允许该玩家按 `identity` 找到并销毁自己的活动投射物。消息类型 30
（`:1455-1466`）同样把 hostile 玩家槽位重写为收包者，再提交 PvP 敌对状态。

这些分支应拆成：

- `InboundNpcDamageCommandAdapter`：玩家身份、NPC 槽位和攻击上下文转换；
- `ServerDamageAuthorizationQuery`：目标有效性、来源玩家、PVP/队伍规则和反作弊检查；
- `ProjectileOwnerLifetimeCommand`：owner + identity 关系的销毁请求；
- `HostileFlagMutationSystem`：玩家 hostile 状态的服务器权威写入和广播。

消息 28 中负伤害分支可以直接把 NPC `life` 置零并停用（`:1422-1427`），所以该入口不能
被当作普通客户端表现同步；它具有潜在的死亡、掉落和 Boss 进度副作用。当前 `StrikeNPC`
内部的完整权限与来源归因未在本轮完全闭合，以上边界保持 `partial`。

### 39.3 Chest、Dresser 和容器槽位事务

消息类型 31（`:1468-1487`）打开 Chest，检查 `Chest.UsingChest`，同步内容并设置玩家当前
Chest；被线路连接的 Chest 还会调用 `Wiring.HitSwitch`。消息类型 32（`:1489-1507`）写入
Chest 槽位的类型、前缀和堆叠；消息类型 33（`:1509-1536`）修改 Chest 名称和玩家打开的
Chest。消息类型 34（`:1538-1703`）覆盖 Chest、Dresser 和特殊箱子的放置、拆除、失败返还
物品、`Chest.FindChest`、Tile 帧归一化和直接销毁。

建议细分为：

- `ContainerOpenQualificationQuery`：坐标、占用者、区段和玩家交互距离；
- `ContainerSlotMutationCommand`：槽位类型、前缀、堆叠和名称修改；
- `ChestStructureTransactionSystem`：Chest/Dresser/特殊箱放置、拆除、索引分配和失败返还；
- `ContainerWiringInteractionCommand`：被线路连接的容器打开触发；
- `ContainerReplicationProjection`：内容、名称、Tile 和槽位 ID 的定向同步。

该消息入口有明确的槽位上限检查（`num27 < 8000`），但本段仍需继续确认打开 Chest 的
距离验证、槽位所有权、重复写入和断线释放；因此不能把 `Chest` 数组写入视为事务闭合。

### 39.4 Sign 文本与 Liquid 写入

消息类型 46/47（`:1844-1880`）读取 Sign 并把客户端文本写入 `Main.sign`，比较旧文本后
广播变更。消息类型 48（`:1882-1917`）写入 Tile 的 liquid 数量和类型，进行玩家附近
10 像素的垃圾包计数、Tile 锁定、`SquareTileFrame` 和清空液体时的回传。

这两个分支应分开：

- `SignReadProjection`：读取已存在 Sign 的文本快照；
- `SignTextMutationSystem`：文本长度、位置、权限和持久化写入；
- `LiquidMutationCommandAdapter`：液体类型/数量包解析和服务器范围检查；
- `LiquidTileCommitSystem`：Tile 锁、帧更新、LiquidFlow 重新入队和区域同步。

Sign 文本是世界持久数据，不应作为聊天消息处理；Liquid 包也不能直接成为客户端权威，
因为它会影响碰撞、Shimmer、液体流动和生态。消息 48 的完整类型范围及越权位置拒绝仍为
`partial`。

### 39.5 锁、门和玩家 Buff 的命令交接

消息类型 52（`:1982-2005`）按子类型调用 `Chest.Unlock`、`WorldGen.UnlockDoor` 和
`Chest.Lock`，随后广播 Tile 区域。消息类型 53（`:2007-2014`）向 NPC 写入 Buff 类型和
持续时间；消息类型 55（`:2018-2027`）仅在服务器非主机或 `Main.pvpBuff` 允许时转发玩家
Buff 数据。

这些分支显示出三个独立边界：

- `LockStateQualificationQuery`：锁/门对象、玩家交互距离和钥匙资格；
- `LockMutationSystem`：Chest 锁、门锁和 Tile 帧提交；
- `NpcBuffInboundCommand` / `PlayerBuffReplicationProjection`：Buff 权威写入与只读广播。

尤其消息 53 的 `AddBuff` 调用和消息 55 的 `pvpBuff` 条件不能合并成通用“Buff 网络同步”，
因为 NPC Buff 与 PVP 玩家 Buff 的来源、权限和防重放语义不同。

### 39.6 收包阶段的连接、出生和状态同步边界

连接流程会在 `MessageBuffer.cs:530-595` 发送区段、Portal、Item、NPC、Projectile、Bestiary
和 Pylon 快照；在 `:628-648` 设置玩家活动状态并触发 `PlayerConnect`/
`PlayerDisconnect`；在 `:1833-1839` 依据团队出生点检查区段并同步。玩家死亡状态、生命上限
修复和 Spawn 请求还出现在 `:735-785`、`:1919-1925`。

建议把现有网络会话进一步拆分为：

- `ConnectionAdmissionSystem`：槽位、姓名、难度/世界模式和封禁检查；
- `JoinSnapshotProjection`：区段、实体、Portal、Bestiary、Pylon 和事件快照；
- `PlayerConnectionLifecycleSystem`：活动/断开、PlayerHooks 和实体引用清理；
- `SpawnHandshakeCommand`：连接完成后的出生上下文、团队出生点和区段预加载。

这些模块只输出命令或投影，不直接让客户端快照反向覆盖世界权威状态。现有流程的
`PlayerConnect`、`PlayerDisconnect`、实体槽位回收和断线库存处理仍需逐分支核对，整体标为
`partial`。

### 39.7 本轮数量与风险更新

- 从 `MessageBuffer` 细分出约 **17 个**协议/授权/事务边界；适合作为独立领域 System 或
  Adapter 的约 **9 个**；
- 新增证据最强的是 `TileMutationCommandRouter`、`ChestStructureTransactionSystem`、
  `LiquidTileCommitSystem`、`InboundNpcDamageCommandAdapter` 和 `ConnectionAdmissionSystem`；
- 这些边界不能重复计算为新的玩法目录，而是对已有 Tile、WorldObjects、Combat、Liquid 和
  NetworkSession 的外层命令入口细化；
- 当前累计独立边界约 **98 个左右**，至少约 **42 个**仍有授权、回滚、网络、存档或空体缺口；
  识别覆盖率继续估计 **80%～90%**，行为闭合度低于识别覆盖率，不能宣布完成。

## 40. 第十三轮：世界/玩家持久化与可恢复世界生成控制

本轮继续追踪 `Terraria.IO` 与 `Terraria.WorldBuilding` 的实际读写链，而不是按文件名把
所有方法归为“存档工具”。`WorldFile.LoadWorld` 会在载入后重新运行液体结算、生态扫描和
世界特定 NPC 选择；`InternalSaveWorld` 会在写入后再次读取并验证，失败时恢复旧文件；
`Player.SavePlayer` 同时跨越成就、地图和玩家文件，但玩家文件的核心序列化方法在当前
版本为空体。因此这里既有可拆分的权威边界，也有必须保持 `partial` 的高风险缺口。

### 40.1 世界加载编排与迁移修复

`WorldFile.LoadWorld`（`Terraria.IO/WorldFile.cs:658-810`）先处理云存档可用性、自动生成和
世界格式版本，再调用 `LoadWorld_Version2`（`:1808-1895`）按 Header、Tiles、Chests、Signs、
NPCs、TileEntities、压力板、TownManager、Bestiary 和 Creative Powers 的 section pointer
顺序加载。读取成功后还会调用 `CheckSavedOreTiers`（`:811-869`）、`ConvertOldTileEntities`
（`:1101-1184`）、`ClearTempTiles`，执行 `Liquid.QuickWater`、`WorldGen.WaterCheck` 和
最多 100000 次液体 settle，最后重置天气、云、萤火虫机会、Skyblock 扫描和世界特定怪物。

这不是单一的 `WorldLoadSystem`，建议分成以下边界：

| 候选边界 | 权威输入/写集 | 主要证据 | 状态 |
| --- | --- | --- | --- |
| `WorldLoadOrchestrationSystem` | 文件版本、云可用性、自动生成、section 顺序、失败状态 | `WorldFile.cs:658-810`, `:1808-1905` | confirmed |
| `WorldSectionPersistenceAdapter` | Tile、Chest、Sign、NPC、TileEntity、Town、Bestiary、Creative section DTO | `WorldFile.cs:1186-1205`, `:1808-1895` | confirmed |
| `WorldMigrationRepairSystem` | 旧矿石层推断、旧 TileEntity 转换、临时 Tile 清理、旧版本字段补齐 | `WorldFile.cs:811-869`, `:1101-1184`, `:1905-1913` | confirmed |
| `LiquidSettlementAfterLoadSystem` | `Liquid.QuickWater`、quick-settle 标志、流体队列和 WaterCheck | `WorldFile.cs:764-795` | confirmed |
| `WorldActivationProjection` | 天气、云、Skyblock 和世界特定 NPC 的派生运行时状态 | `WorldFile.cs:796-810` | partial |

`WorldMigrationRepairSystem` 的写入必须发生在“section 已经成功读取、但世界正式激活之前”；
否则旧 TileEntity 可能和新 Tile 的尺寸/类型不一致。液体 settle 也不能作为普通渲染刷新，
它会改变可碰撞液体和后续生态状态，应由世界加载调度器显式排序在 Tile 恢复之后、玩家加入
之前。天气与 NPC 选择属于派生激活步骤，当前没有看到独立回滚快照，故标记 `partial`。

### 40.2 世界保存、校验和回滚

`SaveWorld_Version2` 固定写入 Header、Tiles、Chests、Signs、NPCs、TileEntities、
WeightedPressurePlates、TownManager、Bestiary、Creative Powers、Footer，并回填 section pointer
（`WorldFile.cs:1186-1205`）。`InternalSaveWorld`（`:878-991`）在 `IOLock` 内生成内存镜像，
写入目标后重新读取，依据 `ValidateWorld`（`:3080-3239`）决定是否轮换备份；校验失败时把
旧字节写回原路径。`DoRollingBackups`（`:993-1033`）最多保留 9 份滚动备份。

建议拆分为：

- `WorldSaveOrchestrationSystem`：临时状态、昼夜重置、锁、section 编排和写入触发；
- `WorldSaveValidationSystem`：版本、section pointer、Tile 压缩流、NPC/TileEntity/Bestiary/
  Creative section 和 Footer 的纯结构验证；
- `WorldRollbackCommand`：验证失败时恢复旧字节，成功后提交备份轮换；
- `WorldBackupRotationAdapter`：本地备份命名、数量上限和文件 I/O；
- `WorldPersistencePointerProjection`：将 section 结束位置投影回 Header 指针，不把指针当作
  玩法状态。

`ValidateWorld` 不加载完整实体，而是顺序消费二进制并检查位置指针，异常时写入
`client-crashlog.txt` 后返回 `false`（`:3080-3239`）。因此验证器应保持无副作用；当前实现
把错误日志作为直接文件副作用，日志端口和失败原因模型仍为 `partial`。此外，`SaveNPCs`
只保存 ShimmeredTownNPC 位图、活动 Town NPC 的位置/住房/变体，以及满足
`NPCID.Sets.SavesAndLoads` 的非 Town NPC（`:1747-1840`）；它不是完整 NPC 战斗快照，不能让
`NpcSimulation` 依赖存档投影来恢复临时生命、Buff 或投射物。

### 40.3 TileEntity、压力板和 Town 状态的持久化边界

`SaveTileEntities` 在 `TileEntity.EntityCreationLock` 下写入 `ByID` 数量和每个实体；
`LoadTileEntities` 先清空旧集合、重新分配连续 ID，删除越界或 Tile 类型无效的实体，再调用
`OnWorldLoaded`（`WorldFile.cs:3385-3444`）。这形成独立的：

- `TileEntityPersistenceAdapter`：实体类型、持久化 ID、位置和额外数据的编码；
- `TileEntityLoadSanitizationSystem`：越界/无效 Tile 清除、ID 重建和世界加载回调；
- `TileEntityLifecycleProjection`：`OnWorldLoaded` 等生命周期通知，不向文件格式泄漏运行时引用。

`SaveWeightedPressurePlates` 只写被按下的坐标，加载时 `PressurePlateHelper.Reset`、设置
`NeedsFirstUpdate` 并为每个坐标创建 255 长度玩家状态数组（`:3446-3474`）。它保存的是跨帧
触发缓存，不是压力板 Tile 本身，因此应拆为 `PressurePlateActivationCacheAdapter`，并在
Tile/线路恢复后由首帧系统重建；不能将其误并入 `TilePlacementAndFraming`。TownManager、
Bestiary 和 Creative Powers 各自拥有独立 Save/Load（`:3476-3530`），建议保持三个投影端口，
避免一个“世界杂项组件”重新聚合彼此无关的生命周期。

### 40.4 世界文件元数据、Seed 解析与规则激活

`WorldFileData` 同时保存 Seed、尺寸、模式、邪恶类型、秘密种子、Hardmode、Moonlord、
UniqueId/WorldId 和存档时间字段（`Terraria.IO/WorldFileData.cs:13-80`）。
`GetSerializedSeedsSum` 将 9 个秘密/特殊种子压成位标志（`:156-198`），
`TryApplyingCopiedSeed` 解析尺寸/模式/邪恶/种子选项后重置 `WorldGenerationOptions`，清空并
启用 `WorldGen.SecretSeed`（`:212-270`）；`SetAsActive` 只切换 `Main.ActiveWorldFileData`
（`:291-300`）。

建议边界为：

- `WorldSeedParseQuery`：复制 Seed 字符串的纯解析、长度和编码校验；
- `SecretSeedActivationSystem`：清空/启用特殊种子并更新 `WorldGen`/`Main` 规则；
- `WorldRuleSnapshotComponent`：尺寸、模式、邪恶、Hardmode 和胜利进度等权威元数据；
- `WorldMetadataActivationCommand`：通过 `SetAsActive` 提交当前世界；
- `InvalidWorldRecoveryProjection`：`FromInvalidWorld` 生成的不可加载世界视图。

这里的证据不能被过度解释：`TryParseSeedOptionValue` 和 `TryParseSecretSeed` 是空体，
`EnableSeedOptions` 也是空体；`MoveToCloud`/`MoveToLocal` 为空体（`:200-280`, `:401-402`）。
因此规则激活链和云迁移均为 `partial`，不能把 `TryApplyingCopiedSeed` 的调用成功等同于
秘密种子完全生效。`HasCrimson` 通过反转 `HasCorruption`，属于兼容投影而非第二个权威字段。

### 40.5 玩家文件、地图和游玩时间

`PlayerFileData` 的权威内容包括 Player 引用、`ServerSideCharacter`、`LastPlayed`、
`_playTime`、Stopwatch 和计时开关（`Terraria.IO/PlayerFileData.cs:10-101`）。主循环每 Tick
调用 `ActivePlayerFileData.UpdatePlayTimer`（`Terraria/Main.cs:11254-11256`），依据
`FocusHelper.AllowCountingPlayerTime` 在 Start/Pause/Stop 之间转换。

`Player.SavePlayer` 先保存成就，再调用 `InternalSaveMap`，仅在非 ServerSideCharacter 时
调用 `InternalSavePlayerFile`（`Terraria/Player.cs:26394-26417`）；地图保存会吞掉异常并按云/本地
状态创建目录（`:26419-26437`）。这需要分出：

- `PlayerPersistenceOrchestrationSystem`：保存顺序、ServerSideCharacter 分支和失败错误呈现；
- `PlayerStateSerializationAdapter` / `PlayerStateDeserializationSystem`：玩家字段与二进制格式；
- `PlayerPlayTimeLedger`：Focus gating、Stopwatch 累积和 `lastTimePlayerWasSaved` 读取；
- `PlayerMapPersistenceProjection`：地图视图保存，不能成为玩家权威装备状态；
- `PlayerVisualCloneProjection`：`SerializedClone` 的表现/快照隔离；
- `PlayerLoadRepairSystem`：加载后的 respawn 调整与字段修复。

关键缺口必须单列：`InternalSavePlayerFile`、`Serialize`、`Deserialize`、
`AdjustRespawnTimerForWorldJoining`、`FixLoadedData` 均为空体/占位（`Player.cs:26417-26460`），
而 `Player.SerializedClone` 仍依赖这些函数（`:26443-26453`）。`PlayerFileData.SetAsActive`、
`MoveToCloud`、`MoveToLocal` 也为空体（`PlayerFileData.cs:45-51`）。所以玩家存档属于 P0/P1
闭合缺口：只能确认编排、计时和地图投影的边界，不能声称库存、装备、Buff、死亡状态或
云迁移已恢复。`Main.ServerSideCharacter` 还改变了“服务器是否写本地玩家文件”的信任边界，
必须作为命令路由条件保留，不能在 ECS 迁移时删掉。

### 40.6 可恢复世界生成：Pass、Manifest 与快照

`WorldGenerator.Controller` 维护 Pass 列表、`PassResults`、暂停/中止标志、Hash 失配策略、
快照频率和 `WorldManifest` 前一版本（`Terraria.WorldBuilding/WorldGenerator.cs:20-124`）。
`TryCreateSnapshot` 只有在 Manifest 最终 Hash 与当前 `HashWorld` 相等时才创建快照
（`:147-172`）；`TryReset`/`TryResetToSnapshot` 会恢复临时状态、清空或恢复 Tile、更新
Manifest 并暂停（`:177-216`）；`TryRunToEndOfPass` 依据最近未过期快照决定回退或重跑
（`:218-263`）。正式循环 `GenerateWorld` 按 PassResults 顺序加控制锁运行每个 `GenPass`，
完成后调用 `OnPassCompleted`（`:291-333`）。

对应的 ECS 边界是：

| 边界 | 责任 | 证据/状态 |
| --- | --- | --- |
| `WorldGenerationPassScheduler` | Pass 顺序、暂停、继续、Abort、控制锁 | `WorldGenerator.cs:218-333`, confirmed |
| `WorldGenerationPassResultLedger` | 每个 Pass 的 Skip/Duration/Hash 结果 | `WorldManifest.cs:9-63`, `GenPassResult.cs`, confirmed |
| `WorldGenerationPauseAndAbortState` | `Paused`、`PauseAfterPass`、Hash 失配和 `QueuedAbort` | `WorldGenerator.cs:41-80`, confirmed |
| `WorldGenerationSnapshotStore` | Manifest/GenVars/Tile 快照文件创建、删除、加载 | `WorldGenSnapshot.cs:87-164`, confirmed |
| `WorldGenerationSnapshotValidityQuery` | GitSHA、版本和启用 Pass 与快照结果匹配 | `WorldGenSnapshot.cs:76-85`, confirmed |
| `WorldGenerationRestoreSystem` | 恢复 GenVars、Tile、Manifest，并停用 NPC | `WorldGenSnapshot.cs:164-180`, confirmed |
| `WorldGenerationHashValidationQuery` | TileSnapshot 结构哈希，确认 Pass 后世界未被修改 | `WorldGenerator.cs:147-172`, `:347-380`, confirmed |

`WorldManifest.Serialize/Deserialize/Clone` 是 JSON 兼容投影（`WorldManifest.cs:34-65`），
而不是新的世界规则组件。`WorldGenSnapshot` 还保存 `GenVarsJson` 和 Tile 二进制偏移，恢复时
调用 `WorldGen.Reset`、`TileSnapshot.Restore` 并停用全部 NPC（`WorldGenSnapshot.cs:120-180`），
所以恢复顺序必须早于 NPC 重新生成和玩家连接。

但控制器存在决定性的未闭合成员：`SetGenerator`、`OnPaused`、`OnPassCompleted`、
`UpdatePreviousManifest`、`ReportException` 为空体；`RunPass` 直接返回空的 `GenPassResult`，
`SetDebugWorldGenUIVisibility` 为空体（`WorldGenerator.cs:102-103`, `:183-193`, `:218-341`）。
快照 JSON 转换器的 `CanConvert`、`ReadJson`、`WriteJson` 以及 `ToString` 也为空体/占位
（`WorldGenSnapshot.cs:36-85`）。因此只能确认调度和快照文件的设计意图，不能确认正式 Pass
真正执行、进度/调试 UI、快照命名和 GenVars 恢复已闭合；这些边界必须继续标记 `partial`，
并在实现恢复前禁止把世界生成视为可回滚事务。

### 40.7 证据矩阵、覆盖率和后续未闭合项

| 领域 | 已确认边界 | 仍未闭合的关键写者/读者 | 结论 |
| --- | --- | --- | --- |
| 世界加载/保存 | section 编排、校验、备份、迁移、液体重算 | 云 I/O 失败语义、错误日志、部分临时状态回滚 | `partial` |
| TileEntity/压力板 | ID 重建、合法性清理、首帧压力缓存 | 各实体额外数据版本和并发创建 | `partial` |
| Seed/规则元数据 | Seed 位标志、尺寸/模式字段、激活入口 | 三个解析/启用空体、云迁移 | `partial` |
| 玩家持久化 | 保存顺序、地图投影、游玩时间 | Serialize/Deserialize、FixLoadedData、ServerSideCharacter 文件路径 | `partial` |
| 世界生成快照 | Pass 控制、Hash、Manifest、Tile/GenVars 快照 | RunPass、回调、JSON Converter、快照命名 | `partial` |

本轮新增约 **22 个**细化边界，其中约 **14 个**可独立成为 System/Query/Adapter/Projection；
它们覆盖的是既有 WorldPersistence、WorldGeneration、TileEntity、PlayerPersistence 和
Network/Activation 的深层 seam，不应简单相加为 22 个新的顶层玩法系统。累计独立边界更新为
约 **120 个左右**；至少约 **55 个**仍有空体、授权、回滚、网络、存档或结果消费缺口。
按“符号和调用链被识别”的覆盖率约 **85%～92%**，按“权威状态可端到端恢复并验证”的行为
闭合度仍明显低于 **70%**。下一轮应优先追踪 `Player` 真实加载入口、`GenPass` 的实际实现/注册
清单、`TileSnapshot` 的压缩与恢复，以及 Save/Load 失败时所有临时全局字段的回滚消费者；在
这些证据补齐前，报告仍是增量设计盘点，不是完成声明。

## 41. 第十四轮：WorldBuilding 形状、条件与动作原语

继续沿 `WorldGen.AddPasses` 的调用方向追踪后，发现更底层的世界生成 DSL 也没有闭合：
`GenShape`/`GenModShape`、`GenAction`、`GenCondition` 以及 `Conditions`、`Actions`、`Modifiers`、
`ModShapes` 中的大量核心方法是空体或占位返回。它们不是普通数学工具，而是生成 Pass 用来
筛选 Tile、遍历形状、修改墙/块/液体/油漆并串联副作用的执行原语；其缺失会让所有上层 Biome
和 Dungeon Pass 的行为证据降级。

### 41.1 条件 Query 与形状遍历

`GenCondition.CheckValidity` 是坐标资格判断的最小接口（`WorldBuilding/GenCondition.cs:1-8`）。
`Conditions.IsTile`、`IsSolid`、`HasLava`、`InWorld`、`NotNull`、`BoolCheck` 和 `Continue` 均
返回占位布尔值（`Conditions.cs:3-413`），本应只读取 Tile/ShapeData，不得写世界状态。
`GenShape.Perform` 定义以原点和 `GenAction` 遍历形状，`GenModShape` 持有 ShapeData
（`GenShape.cs:1-12`, `GenModShape.cs:1-14`）；`ModShapes.All`、`OuterOutline`、`InnerOutline`
决定全形状、外轮廓、内轮廓和对角线/内部选项，但实现同样为空体（`ModShapes.cs:5-62`）。

建议拆分为：

- `WorldGenCoordinateEligibilityQuery`：边界、Tile 类型、固体/液体等纯条件；
- `WorldGenShapeTraversalSystem`：ShapeData 到坐标流的遍历和 `_quitOnFail` 短路；
- `WorldGenOutlineShapeQuery`：Outer/Inner outline 的邻域判断；
- `WorldGenShapeSelectionComponent`：形状数据和输出集合的临时快照，不拥有 Main.tile。

条件对象不能持有随机数、进度或写入计数；计数应由 Action 的输出端口接收。这样可以在不
改变生成语义的前提下，对资格 Query 做纯函数测试，并显式记录“形状遍历 → Action 提交”的
顺序。

### 41.2 Tile/Wall/Liquid 动作与 Action 链

`GenAction` 公开 `NextAction` 和 `OutputData`，`Apply` 负责对一个坐标提交动作
（`GenAction.cs:1-17`）。`Actions` 中 `Clear`、`ClearTile`、`ClearWall`、`SetTile`、
`SetWall`、`SetTileKeepWall`、`SetSlope`、`SetHalfTile`、各种 Paint、`PlaceTile`、
`PlaceWall`、`SetLiquid`、`SwapSolidTile`、`SetFrames`、`Smooth` 等 `Apply` 均为空体/占位
返回（`Actions.cs:1-342`）。`Chain` 会把动作按顺序链接（`:344-352`），但没有证据表明
动作失败时如何停止、回滚或传播 `OutputData`。

应按写集拆分，而不是创建一个拥有所有字段的 `WorldGenActionComponent`：

- `WorldGenTileMutationCommand`：块、墙、Slope、HalfTile 和邻居 Frame；
- `WorldGenPaintMutationCommand`：Tile/Wall/Rainbow paint；
- `WorldGenLiquidMutationCommand`：液体类型/数量和 LiquidCheck 入队；
- `WorldGenCleanupCommand`：Clear、RemoveWall、ClearMetadata 等破坏性清理；
- `WorldGenActionChainSystem`：NextAction 链、失败短路、OutputData 投影和异常边界；
- `WorldGenGenerationProgressProjection`：仅输出 Pass 进度，不写权威 Tile。

`Actions.DebugDraw` 明确依赖 `SpriteBatch`（`Actions.cs:142-159`），应作为调试 Projection
隔离在生成核心之外；`Actions.Custom` 的委托应通过 Adapter 注入，不能让任意 UI 或文件 I/O
进入权威生成 System。

### 41.3 Modifiers 与形状语义的缺口

`Modifiers.ShapeScale`、`Expand`、`RadialDither`、`Blotches`、`InShape`、`NotInShape`、
`Conditions`、`OnlyWalls`、`OnlyTiles`、`Checkerboard` 都是对坐标集合的筛选/变换
（`Modifiers.cs:1-430`）。它们目前全部以占位布尔值返回，导致“生成算法代码存在但不产生
Tile 写集”的假阳性。建议拆分为：

- `WorldGenShapeModifierQuery`：缩放、扩张、径向抖动和 Checkerboard 的纯坐标变换；
- `WorldGenTileWallFilterQuery`：OnlyTiles/OnlyWalls/Conditions 组合资格；
- `WorldGenRandomizedPlacementPolicy`：Blotches/RadialDither 使用的随机端口和种子；
- `WorldGenShapeModifierAdapter`：将 Modifier 输出接入 `GenAction`，不直接改变全局 GenVars。

随机策略必须显式传入 `_seed`/`genRand`，并把结果记录到 Pass 结果或快照；否则恢复同一个
快照时可能得到不同 Tile。`Modifiers.Conditions` 目前把多个 `GenCondition` 包装成一个
Action，不能在设计上把它误认为纯 Query 之后就允许隐式修改。

### 41.4 本轮新增边界与闭合度影响

| 边界 | 证据 | 当前状态 |
| --- | --- | --- |
| `WorldGenCoordinateEligibilityQuery` | `Conditions.cs:3-413` | `partial`（所有 CheckValidity 占位） |
| `WorldGenShapeTraversalSystem` | `GenShape.cs:1-12`, `ModShapes.cs:5-62` | `partial` |
| `WorldGenTileMutationCommand` | `Actions.cs:1-342` | `partial`（Apply 空体） |
| `WorldGenActionChainSystem` | `GenAction.cs:1-17`, `Actions.cs:344-352` | `partial` |
| `WorldGenShapeModifierQuery` | `Modifiers.cs:1-430` | `partial` |
| `WorldGenRandomizedPlacementPolicy` | `Modifiers.cs` 的 Blotches/RadialDither 字段 | `missing/partial` |
| `WorldGenDebugProjection` | `Actions.DebugDraw`, `WorldGenerator.SetDebugWorldGenUIVisibility` | `partial` |

本轮新增约 **7 个**底层生成原语边界，累计独立边界约 **134 个左右**。由于这些原语被
大量 Pass 依赖，行为闭合度应从上一轮的 60%～68% 下调为约 **55%～65%**；符号/调用链识别
覆盖率仍约 **86%～93%**。下一轮应优先抽取 `ShapeData`、`GenBase` 的真实调用者和每类
`Actions` 的写集，建立“Condition → Shape → Modifier → Action → Tile/GenVars”逐步证据，
否则任何按 Biome 名称统计的子系统数量都只是静态识别，不代表可执行模拟。

## 42. 第十五轮：生成搜索、范围缩放与特殊种子选项状态

进一步阅读 `WorldBuilding` 的基础类型后，还发现三个容易被遗漏的子系统：坐标搜索
（`GenSearch`）、按世界规模缩放的配置范围（`WorldGenRange`）以及特殊种子选项的依赖状态。
它们分别连接“生成 Pass 找点”“配置参数 → 随机数量”和“Seed 文本 → 全局生成规则”；不应
塞入前一轮的 Shape/Action 巨型边界。

### 42.1 GenSearch：从原点到候选坐标的纯查询

`GenSearch` 持有 `GenCondition[]`，通过 `Conditions(...)` 设置过滤条件，并以
`Find(Point origin)` 返回坐标或 `NOT_FOUND`（`WorldBuilding/GenSearch.cs:1-26`）。
`WorldUtils.Find` 只负责调用搜索并将 `NOT_FOUND` 转换成布尔结果（`WorldUtils.cs:39-52`）。
这形成独立的 `WorldGenSearchQuery`：输入原点、条件和世界只读视图，输出坐标/失败原因，
不应在查询中放置 Tile、创建 Chest 或修改 GenVars。当前仓库未发现一个完整的具体
`GenSearch.Find` 实现，且其条件检查本身来自上一节的占位 `Conditions`，所以该边界为
`missing/partial`，不能把 `WorldUtils.Find` 的存在视为搜索行为已实现。

### 42.2 WorldGenRange：配置值、世界尺寸和随机端口

`WorldGenRange` 保存 `Minimum`、`Maximum` 和 `ScalingMode`（`None`、`WorldArea`、
`WorldWidth`），`GetRandom` 使用传入的 `UnifiedRandom` 在缩放范围内取值
（`WorldGenRange.cs:5-46`）。但 `ScaleValue` 是占位返回（`:46`），因此同一生成配置在
小/中/大世界上的数量比例目前无法确认。建议拆分为：

- `WorldGenerationRangeComponent`：不可变的 Min/Max/ScalingMode 配置；
- `WorldGenerationScaleQuery`：依据 `Main.maxTilesX/Y` 计算缩放后的边界；
- `WorldGenerationRandomValueAdapter`：显式注入 `UnifiedRandom`，记录种子和取值；
- `WorldGenerationRangeValidationQuery`：检查 Min ≤ Max、溢出和配置缺省值。

随机 Adapter 的输出应作为 Pass 输入或结果元数据保存，不能把随机数缓存到全局查询对象；
否则快照恢复和重跑 Pass 会出现不可重现的数量差异。

### 42.3 特殊种子选项与依赖传播

`WorldGenerationOptions` 注册 Normal、NotTheBees、Drunk、Anniversary、DontStarve、
ForTheWorthy、NoTraps、Remix、Everything、Skyblock 十种选项，并提供 `Reset`、`SelectOption`、
`GetOptionFromSeedText` 和 `TryEnablingFlagFrom`（`WorldGenerationOptions.cs:17-132`）。
`AWorldGenerationOption.Enabled` 的 setter 会触发 `OnEnabledStateChanged` 和静态
`OnOptionStateChanged` 事件（`AWorldGenerationOption.cs:9-58`），而 Everything/Normal 通过
依赖列表监听其他选项（`WorldSeedOption_Everything.cs:8-54`, `WorldSeedOption_Normal.cs:1-24`）。

建议分为：

- `WorldSeedOptionRegistry`：选项注册、唯一实例和生命周期；
- `WorldSeedOptionSelectionCommand`：Reset/Select/Enable 的显式状态转换；
- `WorldSeedOptionDependencySystem`：Everything 与基础选项的依赖传播；
- `WorldSeedOptionSeedTextQuery`：特殊名称/数值与 Seed 文本规范化匹配；
- `WorldSeedOptionServerConfigAdapter`：`seed_foo=0/1` 行到 AutoGenEnabled 的配置转换；
- `WorldSeedOptionUiProjection`：`ProvideUIElement` 输出 UI，不拥有规则状态。

这里必须区分 Query 和事件副作用：`GetOptionFromSeedText` 是纯匹配；`Enabled` setter 是
命令提交后触发的事件；`TryEnablingFlagFrom` 修改服务器配置状态；`ProvideUIElement` 是
表现投影。当前 `EnableSeedOptions`、`OnEnabledStateChanged`、Everything/Normal 的依赖
更新和 `ProvideUIElement` 多为空体/默认 UI（`WorldFileData.cs:211`, `AWorldGenerationOption.cs:49-58`,
`WorldSeedOption_Everything.cs:44-48`, `WorldSeedOption_Normal.cs:18-19`），所以特殊种子
依赖闭合度仍为 `partial`。

### 42.4 本轮新增边界与统计修正

| 边界 | 主要证据 | 状态 |
| --- | --- | --- |
| `WorldGenSearchQuery` | `GenSearch.cs:1-26`, `WorldUtils.cs:39-52` | `missing/partial` |
| `WorldGenerationRangeComponent` + `WorldGenerationScaleQuery` | `WorldGenRange.cs:5-46` | `partial` |
| `WorldGenerationRandomValueAdapter` | `WorldGenRange.GetRandom` | `confirmed/partial` |
| `WorldSeedOptionRegistry` | `WorldGenerationOptions.cs:17-56` | `confirmed` |
| `WorldSeedOptionDependencySystem` | `WorldSeedOption_Everything.cs:32-48` | `partial` |
| `WorldSeedOptionServerConfigAdapter` | `WorldGenerationOptions.cs:109-132` | `confirmed/partial` |
| `WorldSeedOptionUiProjection` | `AWorldGenerationOption.cs:57-58` | `partial` |

本轮新增约 **7 个**边界，累计独立边界约 **141 个左右**；至少约 **62 个**仍有空体或未
验证调用链。识别覆盖率约 **87%～94%**，但生成参数可重现性和特殊种子依赖闭合后，行为
闭合度仍约 **55%～65%**。下一轮重点应转向 `GenSearch` 的具体派生类、`ShapeData` 的
写者/读取者和 `WorldGenConfiguration` JSON 到 `WorldGenRange` 的适配，形成从配置到实际
Tile 写入的端到端证据。

## 43. 第十六轮：TileSnapshot 快照实现与世界生成 Pass 注册缺口

本轮从快照文件继续下钻到 `Terraria.Utilities.TileSnapshot`，并核对 `WorldGen.AddPasses` 的
实际注册路径。结果显示，Version4 同时存在“快照格式/哈希接口可见”和“真正拷贝/恢复函数
为空体”的断层；世界生成文件虽然包含大量 Pass 配置和算法代码，但注册器与 `RunPass` 的
执行桥接为空体，不能仅凭 `AddPasses` 中的长代码清单宣称这些 Pass 在运行。

### 43.1 TileSnapshot 的状态分层

`TileSnapshot.TileStruct.From` 将 Tile 类型、墙、头部位、帧、液体压缩为三个整数槽，只有
重要 Tile 才保留帧坐标，并按无墙/无液体条件清理派生位（`Terraria.Utilities/TileSnapshot.cs:14-145`）。
`HashWorld` 使用该规范化结构逐列计算 Hash（`Terraria.WorldBuilding/WorldGenerator.cs:347-380`），
因此它是“世界生成一致性查询”的输入规范，而不是完整运行时 Tile 序列化格式。

`TileSnapshot.Create` 保存当前世界引用并调用 `SaveTiles`、`SaveTileEntities`、`SaveChests`；
`Save` 则写入结构大小、所有 TileStruct、TileEntity 列表和 Chest 物品槽位
（`TileSnapshot.cs:155-211`）。`Load` 能读取结构大小并重建 TileStruct、TileEntity 和 Chest
列表（`:223-286`）。但是以下真实状态拷贝/恢复方法均为空体：

- `SaveTiles`、`SaveTileEntities`、`SaveChests`；
- `RestoreTiles`、`RestoreTileEntities`、`RestoreChests`；
- `TileStruct.Equals`、`GetHashCode`、`ToString`。

所以应拆成：

- `TileSnapshotCaptureSystem`：从权威 Tile/TileEntity/Chest 状态生成快照；
- `TileSnapshotBinaryAdapter`：结构大小、压缩 Tile、实体和 Chest 槽位的文件格式；
- `TileSnapshotRestoreSystem`：按 Tile → TileEntity → Chest 顺序提交恢复并触发服务器重同步；
- `TileSnapshotHashQuery`：对规范化 TileStruct 计算可重复 Hash；
- `TileSnapshotContextState`：当前快照上下文、世界文件引用和临时缓冲区所有权。

`Restore` 在服务器上调用整张地图的 `NetMessage.ResyncTiles`（`TileSnapshot.cs:169-181`），
这是网络投影副作用，必须在恢复提交成功后执行，不能由纯 Hash Query 触发。由于 Capture/
Restore 的核心函数为空体，快照目前只能确认二进制外壳和格式检查，`TileSnapshotCaptureSystem`
与 `TileSnapshotRestoreSystem` 均为 `partial`。此外，`TileStruct.GetHashCode` 为空体会使
`HashWorld` 的一致性证据失效，`WorldGenerationHashValidationQuery` 需降级为“接口存在、
结果未验证”。

### 43.2 WorldGen Pass 注册、执行和特殊种子过滤

`WorldGen.GenerateWorld` 建立 `WorldGenerator`、清空并重置世界、调用 `AddPasses`，再调用
`DisablePassesForSpecialSeeds` 和 `_generator.GenerateWorld`（`Terraria/WorldGen.cs:10098-10140`）。
`AddPasses` 从 `:10553` 起注册 Terrain、Skyblock、Dungeon、Biome、液体、清理、出生点和
Starter NPC 等大量 Pass；每个 legacy lambda 都直接写 Tile、GenVars、Liquid、Chest、
TileEntity 或出生点数据。

但两个重载 `AddGenerationPass(string, WorldGenLegacyMethod)` 与 `AddGenerationPass(GenPass)`
均为空体（`WorldGen.cs:9176-9177`），意味着这些 Pass 没有被加入 `_generator._passes` 的
已确认写者。`WorldGenerator.RunPass` 直接返回空 `GenPassResult`（`WorldGenerator.cs:341-342`），
而 `TerrainPass.ApplyPass` 与 `JunglePass.ApplyPass` 也为空体（`GameContent.Biomes/TerrainPass.cs:40`, `JunglePass.cs:18`）。
因此应建立三层而非笼统的“世界生成系统”：

- `WorldGenerationPassRegistrationSystem`：把 legacy method 和 `GenPass` 实例转换为有序 Pass；
- `WorldGenerationPassExecutionSystem`：执行 `ApplyPass`/legacy delegate，收集进度、异常、
  Duration 和 Hash；
- `SpecialSeedPassFilterQuery`：依据 `SecretSeed.dualDungeons` 禁用 Ice、Desert、Jungle、
  Temple、Corruption/Crimson、Shimmer 等 Pass（`WorldGen.cs:21674-21692`）。

`DisablePassesForSpecialSeeds` 本身具有纯资格判断和结构写入两面：匹配名称的 Pass 被调用
`Disable` 改变调度状态，因此 Query 只应计算候选集合，再由 `PassDisableCommand` 提交，避免
在查询期间隐式改变 Pass。特殊种子还会影响 `Main`/`WorldGen` 全局规则，必须在
`WorldRuleSnapshotComponent` 激活后、Pass 列表冻结前运行。

### 43.3 新发现的未闭合生成子系统

| 子系统 | 已确认职责 | 关键空体/缺口 | 状态 |
| --- | --- | --- | --- |
| `TileSnapshotCaptureSystem` | Tile、TileEntity、Chest 快照采集 | 三个 Save* 方法 | `partial` |
| `TileSnapshotRestoreSystem` | 快照恢复与服务器 Tile 重同步 | 三个 Restore* 方法 | `partial` |
| `TileSnapshotHashQuery` | 规范化 Tile 哈希 | `GetHashCode` 空体 | `partial` |
| `WorldGenerationPassRegistrationSystem` | AddPasses 到 `_passes` 的桥接 | 两个 AddGenerationPass 空体 | `partial` |
| `WorldGenerationPassExecutionSystem` | Pass 应用和结果账本 | RunPass、OnPassCompleted 空体 | `partial` |
| `SpecialSeedPassFilterQuery` | 特殊种子 Pass 资格集合 | 需独立提交 Disable | `confirmed/partial` |
| `WorldGenerationProgressProjection` | Message、Weight、调试 UI 和日志 | SetDebug UI、ReportException、ToString 空体 | `partial` |

这轮新增约 **7 个**细化边界，均属于 WorldGeneration/WorldPersistence 的深层 seam；累计
独立边界约 **127 个左右**。由于 Pass 注册和快照 Capture/Restore 都存在空体，本项目当前
“可识别符号覆盖率”可上调到约 **86%～93%**，但“可实际生成、保存、回滚并重同步的行为闭合度”
仍约 **60%～68%**，不能用报告中的 Pass 名称数量替代执行证据。下一步应追踪 `Chest` 和
`TileEntity` 的 Clone/FixLoadedData、`WorldGen.Finish` 的完成消费者、以及所有生成 Pass 对
`GenVars` 的写集，建立逐 Pass 的权威状态矩阵。

## 44. 第十七轮：结构占用图、MicroBiome 放置与生成配置适配

继续追踪 `WorldUtils.Gen` 的实际调用者后，确认 `StructureMap` 与 `GenStructure` 是独立于
Shape/Action 的“结构级事务”边界。它们控制地牢房间、金字塔、蜂巢、花岗岩、陷阱箱等大型
生成物能否占用区域，并通过保护矩形阻止后续 Pass 覆盖；若把它们并入 Tile Mutation，结构
冲突和回滚顺序就会丢失。

### 44.1 StructureMap 的占用与保护状态

`StructureMap` 维护 `_structures` 与 `_protectedStructures` 两个矩形集合，并通过 `_lock`
保护并发访问（`WorldBuilding/StructureMap.cs:9-18`）。`CanPlace` 检查世界边界、padding 后
的保护区域以及现有 Tile 是否属于 `validTiles`（`:21-60`）；`AddProtectedStructure` 同时
登记普通和保护矩形（`:62-71`）。`WorldGen.GenerateWorld`/Reset 路径把它放到
`GenVars.structures`（`Terraria/WorldGen.cs:10186`），多个 `MicroBiome.Place` 直接接收同一
实例（`Terraria.GameContent.Biomes/*Biome.cs`）。

建议拆分为：

- `WorldStructureOccupancyComponent`：结构矩形、保护矩形和生成阶段生命周期；
- `StructurePlacementQualificationQuery`：边界、padding、现有 Tile 和保护区冲突判断；
- `StructureReservationCommand`：原子登记结构/保护矩形，失败时不留下半条记录；
- `StructureMapConcurrencyAdapter`：锁与批量查询的外部适配；
- `StructureProtectionProjection`：供后续 Pass 查询的只读占用视图。

`CanPlace` 是纯资格查询，但它读取 `Main.tile` 和共享集合；`AddProtectedStructure` 是写入
命令，不能让 Query 直接追加矩形。当前未发现删除/回滚保护矩形的接口，若一个 Biome 在
Tile 写入中途失败，已登记的区域可能永久阻塞后续 Pass，因此事务回滚状态为 `partial`。

### 44.2 GenStructure/MicroBiome 的放置事务

`GenStructure` 定义 `Place(Point, StructureMap, GenerationProgress)`，并提供一个默认的
无进度重载（`WorldBuilding/GenStructure.cs:1-12`）；默认重载当前返回占位值。Biome 实例
通过 `WorldGenConfiguration.CreateBiome<T>` 从配置 JSON 创建，找不到配置时回退到 `new T()`
（`WorldBuilding/WorldGenConfiguration.cs:15-33`）。`GraniteBiome.CanPlace` 已明确读取
`WorldGen.BiomeTileCheck` 和目标 Tile 是否 active，但其 `Place` 为空体
（`Terraria.GameContent.Biomes/GraniteBiome.cs:49-64`）；`DeadMansChestBiome` 的放置、
陷阱点搜索、缓存清理也为空体，而 `GetPossibleChestsToTrapify` 会扫描最多 8000 个 Chest、
调用 `StructureMap.CanPlace`（`DeadMansChestBiome.cs:69-150`）。

应拆成：

- `MicroBiomePlacementOrchestrationSystem`：配置实例、候选原点、进度和放置顺序；
- `MicroBiomePlacementQualificationQuery`：Biome 自身 CanPlace、StructureMap 冲突和世界边界；
- `MicroBiomeTileCommitSystem`：Biome 的 Tile/Wall/Liquid/Chest/TileEntity 写入；
- `MicroBiomeStructureReservationCommand`：写入前登记保护矩形，失败时回滚预留；
- `MicroBiomePlacementResultProjection`：成功/失败、结构范围和后续 Pass 可见性。

Biome 的临时缓存（例如 DeadMansChest 的 dart/boulder/wire/explosive placement attempts）
应属于一次放置命令的短生命周期，不能进入持久世界组件。`GetPossibleChestsToTrapify` 产生
的是候选 Chest ID 列表投影，真正的陷阱化必须由后续 Command 提交，避免查询阶段修改 Chest。

### 44.3 配置到生成状态的适配

`WorldGenConfiguration` 持有 `Biomes` 与 `Passes` 两个 JSON 根节点，并通过
`CreateBiome<T>` 将配置对象反序列化为 MicroBiome（`WorldBuilding/WorldGenConfiguration.cs:7-33`）；
`FromEmbeddedPath` 从嵌入资源读取 JSON（`:35-44`）。这形成 `WorldGenerationConfigurationAdapter`：
它负责版本化配置、默认值和类型映射，但不能直接拥有 `Main.tile` 或 `GenVars`。

配置参数最终经 `WorldGenRange`、Biome 字段和 Pass lambda 写入世界；当前 `WorldGenRange.ScaleValue`
为空体，多个 Biome `Place` 为空体，故配置“已读入”不等于配置“已驱动生成”。建议为每个
配置字段记录：JSON 路径、目标 Component、随机端口、写入 Pass 和回滚策略。

### 44.4 本轮新增边界与统计修正

| 边界 | 关键证据 | 状态 |
| --- | --- | --- |
| `WorldStructureOccupancyComponent` | `StructureMap.cs:9-18` | `confirmed` |
| `StructurePlacementQualificationQuery` | `StructureMap.cs:21-60` | `confirmed/partial` |
| `StructureReservationCommand` | `StructureMap.cs:62-71` | `partial`（无删除/回滚） |
| `MicroBiomePlacementOrchestrationSystem` | `GenStructure.cs:1-12`, `WorldGenConfiguration.cs:15-44` | `partial` |
| `MicroBiomePlacementQualificationQuery` | `GraniteBiome.cs:49-57`, `DeadMansChestBiome.cs:102-150` | `partial` |
| `MicroBiomeTileCommitSystem` | 各 `MicroBiome.Place` 与 `WorldUtils.Gen` 调用 | `partial` |
| `WorldGenerationConfigurationAdapter` | `WorldGenConfiguration.cs:7-44` | `confirmed/partial` |

本轮新增约 **7 个**结构/配置边界，累计独立边界约 **148 个左右**；至少约 **68 个**仍有
空体、回滚、并发或实际消费者缺口。静态识别覆盖率约 **88%～95%**，但结构放置事务和
Biome Tile 提交仍未闭合，行为闭合度约 **52%～63%**。下一步应建立结构矩形与 Tile 写集的
逐 Pass 映射，并继续核对 `MicroBiome` 派生类中哪些 `Place` 是真实实现、哪些只是占位，避免
把配置类数量直接当成可运行的世界生成系统数量。

## 45. 第十八轮：液体合并转化与机关传输的未闭合写集

本轮从已记录的 `LiquidFlow`/`WiringInteraction` 顶层边界继续检查其内部写者。发现两条
此前只按“流动/机关”概括、但实际应独立建模的事务链：液体类型合并会生成或摧毁 Tile，
机关网络会转移液体、检查逻辑门并驱动 Extractinator/Hopper 等实体经济行为。它们拥有不同
的队列、失败语义和网络投影，不能合并为一个通用 `WorldInteractionSystem`。

### 45.1 液体合并与环境 Tile 转化

`Liquid.LiquidCheck` 在邻接液体类型不一致时清空邻居液体，依据水/岩浆/蜂蜜/Shimmer 组合
选择合并 Tile，并调用 `CreateLiquidMergeTile`；下方液体不足、容器和可切割 Tile 也会触发
清空或 `WorldGen.KillTile`（`Terraria/Liquid.cs:1238-1320`）。`GetLiquidMergeTypes` 明确把
液体类型映射到 Obsidian、蜂蜜块和其他转换结果（`:1324-1400`），但
`CreateLiquidMergeTile` 为空体，`UndergroundDesertCheck` 也为空体（`:1235`, `:1322-1323`）。

建议拆分为：

- `LiquidMergeQualificationQuery`：邻接类型、数量阈值、容器/可摧毁 Tile 和世界生成条件；
- `LiquidMergeTypeResolutionQuery`：水/岩浆/蜂蜜/Shimmer → 目标 Tile/液体类型映射；
- `LiquidMergeTileCommitSystem`：清空液体、生成合并 Tile、Tile 帧和 LiquidCheck 入队；
- `LiquidHazardTileDestructionCommand`：液体导致的可切割 Tile 摧毁、掉落和网络同步；
- `UndergroundDesertLiquidPolicy`：地下沙漠边界对液体沉降/合并的特殊限制；
- `LiquidMergeReplicationProjection`：服务器 TileSquare/液体变更广播。

`LiquidCheck` 同时读写多个相邻 Tile，并可能触发网络消息；它必须由单线程/区域锁的提交
阶段执行，不能被当成纯 Query。`GetLiquidMergeTypes` 可保持纯函数，但其输出不能直接写入
Tile。当前合并 Tile 生成方法为空体，Shimmer 与地下沙漠分支的行为证据不完整，整体为
`partial`。

### 45.2 Wiring 的液体/逻辑门/机关副作用

`Wiring.MassWireOperation` 统计玩家库存中的 Wire/Actuator，调用内部批处理并通过 NetMessage
返还消耗差额（`Terraria/Wiring.cs:429-468`）；但 `MassWireOperationInner` 为空体。机关检查
`CheckMech` 为空体，液体传输 `XferWater` 为空体，逻辑门检查 `CheckLogicGate` 为空体
（`Wiring.cs:465-467`, `:665`）。Extractinator、Hopper、GeyserTrap 和 DeActive/ReActive
也为空体（`:2368`, `:2399`, `:2703`, `:2777-2780`），而 `Teleport` 会直接改变玩家/NPC
位置并发送网络包（`:2705-2775`）。

建议进一步拆分：

- `WireBatchMutationCommand`：范围内 Wire/Actuator 写入、物品扣除/返还和失败回滚；
- `MechanismCooldownQualificationQuery`：`CheckMech` 的机关时间窗口、防重复触发和区域资格；
- `WireLiquidTransferSystem`：`XferWater` 的液体容器读写、类型和数量不变量；
- `LogicGateEvaluationSystem`：灯/门状态、门槛和 `_GatesDone` 去重队列；
- `ExtractinatorInteractionCommand` / `HopperCollectionSystem`：输入物品、掉落结果和容器；
- `TrapActivationSystem`：Geyser、爆炸矿和激活/反激活 Tile；
- `WiringTeleportCommand`：玩家/NPC 安全位置、免疫规则、区段检查和网络广播。

`Wiring.Teleport` 已是明确的跨实体权威写入，不能留在“电线表现层”；而 `CheckLogicGate`、
`XferWater` 和 `MassWireOperationInner` 的空体使机关网络的传播/去重/库存消费无法闭合。
这些系统必须在 `TileMutation` 之后、实体移动/液体同步之前按顺序调度。

### 45.3 证据状态与统计

| 边界 | 证据 | 状态 |
| --- | --- | --- |
| `LiquidMergeQualificationQuery` | `Liquid.cs:1238-1279` | `confirmed/partial` |
| `LiquidMergeTypeResolutionQuery` | `Liquid.cs:1324-1400` | `confirmed` |
| `LiquidMergeTileCommitSystem` | `Liquid.cs:1279-1323` | `partial`（CreateLiquidMergeTile 空体） |
| `UndergroundDesertLiquidPolicy` | `Liquid.cs:1235`, `:1400-1408` | `missing/partial` |
| `WireBatchMutationCommand` | `Wiring.cs:429-468`, `:2780` | `partial` |
| `WireLiquidTransferSystem` | `Wiring.cs:465-467` | `missing` |
| `LogicGateEvaluationSystem` | `Wiring.cs:625-665` | `partial` |
| `HopperCollectionSystem` | `Wiring.cs:2368-2405` | `partial` |
| `WiringTeleportCommand` | `Wiring.cs:2705-2775` | `confirmed/partial` |

本轮新增约 **9 个**液体/机关细化边界，累计独立边界约 **163 个左右**；至少约 **84 个**
仍有空体、授权、队列、回滚或网络消费者缺口。静态识别覆盖率约 **90%～96%**，但液体
合并和机关传播仍缺少可执行写集，行为闭合度约 **48%～59%**。下一轮应对 `Wiring.HitWire`
的队列去重、`LiquidBuffer` 溢出路径以及 Container/TileEntity 的互相触发建立时序图，确认
是否存在新的跨域循环依赖。

## 46. 第十九轮：方向搜索跨玩法查询与结构放置结果消费

本轮核对 `Searches` 的真实消费者，确认它不仅服务 WorldGen。`NPC.cs` 在特定 NPC AI 中
向下搜索固体 Tile，以修正悬浮/下沉位置；`Projectile.cs` 在投射物命中计算中向下搜索
固体或非空 Tile，以调整特定武器伤害（`Terraria/NPC.cs:39818-39845`,
`Terraria/Projectile.cs:11944-11946`）。因此方向搜索是一个跨 Combat/Movement/WorldGen
的只读查询边界，不能归入 Biome 生成专用工具。

### 46.1 Searches 的方向和条件组合

`Searches.Left/Right/Down/Up` 保存最大搜索距离，`Rectangle` 保存宽高；其 `Find` 当前
全部返回 `new Point()` 占位（`Terraria.WorldBuilding/Searches.cs:5-75`）。`Searches.Chain`
把 `GenCondition` 写入搜索对象（`:77-83`），`WorldUtils.Find` 再把 `NOT_FOUND` 转换为
布尔结果（`WorldUtils.cs:39-52`）。建议拆分为：

- `DirectionalTileSearchQuery`：沿单方向按最大距离遍历 Tile；
- `RectangularTileSearchQuery`：矩形候选点遍历；
- `SearchConditionPipeline`：只读条件组合、短路和失败原因；
- `SearchResultProjection`：将坐标或 `NOT_FOUND` 投影给 NPC、Projectile 或 WorldGen 调用者。

查询结果不能直接修改 NPC/Projectile；例如 NPC 的位置修正应由 `NpcGroundingSystem` 消费，
投射物伤害倍率应由 `ProjectileDamageQualificationQuery` 消费。这样可分别测试“搜索无结果”、
“边界不足”和“固体命中距离”而不触发战斗副作用。

### 46.2 跨域写入风险

NPC 示例在搜索成功后直接修改 `position.Y`，失败时按距离增加偏移；Projectile 示例在无
固体时直接把伤害乘以 1.5。由于 `Find` 当前为空壳，这些上层行为会得到错误的原点或永远
失败，导致看似属于 AI/Combat 的差异实际来自 WorldBuilding 基础查询。迁移时应记录：

| 消费者 | 查询输入 | 权威写入 | 推荐边界 |
| --- | --- | --- | --- |
| 特定 NPC AI | NPC 底部 Tile、Down 距离、`IsSolid` | NPC position/netUpdate | `NpcGroundingSystem` |
| 特定投射物 | 中心 Tile、Down 12、`NotNull`/`IsSolid` | 伤害倍率派生值 | `ProjectileDamageQualificationQuery` |
| WorldGen Pass | 原点、方向、生成条件 | Tile/Wall/结构 | `WorldGenSearchQuery` + Mutation Command |

### 46.3 本轮新增边界与统计

- `DirectionalTileSearchQuery`：`partial`，所有方向 `Find` 占位；
- `RectangularTileSearchQuery`：`partial`，`Find` 占位；
- `SearchConditionPipeline`：`partial`，依赖占位 `GenCondition`；
- `NpcGroundingSystem`：`partial`，上层位置写入存在但查询结果不可信；
- `ProjectileDamageQualificationQuery`：`partial`，伤害倍率分支存在但地形资格未闭合；
- `SearchResultProjection`：`confirmed/partial`，`WorldUtils.Find` 只完成结果转换。

本轮新增约 **6 个**跨域边界，累计独立边界约 **154 个左右**；至少约 **74 个**仍有
空体或未验证结果消费者。静态识别覆盖率约 **89%～95%**，行为闭合度约 **50%～61%**。
这进一步说明报告中的“识别覆盖率”不能只按目录统计：一个空的 `Searches.Down` 会同时
影响 NPC 运动、Projectile 伤害和若干生成 Pass，必须按调用图追踪其真实影响面。

## 47. 第二十轮：Tile Blend 帧拓扑与 Minecart 轨道状态

本轮继续检查已有 `TilePlacementAndFraming`、`MountAndMinecart` 边界的内部写者，发现两个
会直接改变权威碰撞/移动结果的未闭合子系统：`Framing.WillItBlend` 决定相邻 Tile 是否能
连接，`Minecart` 的轨道帧访问器和邻居查找决定矿车轨道的几何状态。它们不应被归入渲染
层，因为结果会被碰撞、放置、轨道切换、压力板和网络同步读取。

### 47.1 Tile Blend 与八方向帧拓扑

`Framing.SelfFrame8Way` 会读取中心 Tile 的 `BlockStyle`，逐方向检查邻居，并通过
`WillItBlend(centerTile.type, neighbor.type)` 决定是否建立上下左右及对角连接
（`Terraria/Framing.cs:196-316`）。连接位随后影响八方向 frame lookup、邻接 Tile 的形态和
放置后的碰撞外观。`WillItBlend` 当前为空体，占位 `false` 会让所有相邻类型的连接资格失真。

建议拆分为：

- `TileBlendQualificationQuery`：Tile 类型、BlockStyle 和邻居兼容性纯判断；
- `TileFrameTopologySystem`：中心 Tile 与邻居的八方向连接位计算；
- `TileFrameLookupProjection`：连接位到 frameX/frameY 的表现投影；
- `TileFrameCommitCommand`：将计算后的帧提交并向邻居传播；
- `TileBlendCompatibilityAdapter`：从 TileID/TileObjectData 内容表取得 blend 规则。

`TileBlendQualificationQuery` 不能写 frame；`TileFrameTopologySystem` 必须在 Tile 结构提交
后、网络 TileSquare 投影前运行。由于 `WillItBlend` 为空体，现有 `SquareTileFrame` 调用
虽然广泛存在，但八方向连接结果尚未验证，故该细化边界为 `partial`，不能把 Framing 文件
中已有 lookup 表当作帧行为已经完成。

### 47.2 Minecart 轨道几何和状态切换

`Minecart.FrameTrack` 读取轨道 Tile 的 front/back track ID，检查邻居轨道并写回压力轨道、
普通轨道和 boost 轨道帧；在 `pound` 且状态变化时还会调用 `WorldGen.KillTile`
（`Terraria/Minecart.cs:953-1178`）。`GetOnTrack`、`TrackCollision`、`TrackRotation` 和
`HitTrackSwitch` 使用这些轨道 ID 决定矿车吸附位置、轮子高度、旋转、转向和 Wiring 开关
触发（`:608-844`, `:1185-1322`）。

但下列基础访问器均为空体/占位：

- `GetNearbyTilesSetLookupIndex` 返回占位整数（`:1183`）；
- `Tile.FrontTrack()` / setter 返回占位或空体（`:1371-1374`）；
- `Tile.BackTrack()` / setter 返回占位或空体（`:1375-1378`）。

因此需要把已有矿车系统继续细分为：

- `MinecartTrackStateComponent`：Tile 上 front/back 轨道 ID、轨道类型和帧状态；
- `MinecartNeighborTopologyQuery`：邻居轨道集合、坡度、转向和轨道连接资格；
- `MinecartTrackFramingSystem`：FrameTrack 的轨道状态重算和必要的 Tile 破坏命令；
- `MinecartTrackCollisionSystem`：轮子位置、轨道高度、速度和旋转的权威运动结果；
- `MinecartTrackSwitchCommand`：压力轨道/开关轨道切换、Wiring 触发和网络同步；
- `MinecartTrackItemProjection`：轨道类型到拆除掉落物的只读映射。

`PlaceTrack` 只设置 Tile 类型和初始 frame（`:1300-1320`），并不等于轨道拓扑已构建；
真正的连接由 `FrameTrack` 及上述访问器完成。`FlipSwitchTrack` 直接交换 front/back 或
重新 Frame，并广播 TileSquare（`:1300-1322`），所以切换必须是显式 Command，不能由碰撞
查询隐式完成。轨道切换还会调用 `Wiring.HitSwitch`，调度顺序应为“轨道状态提交 → Wiring
触发 → 网络投影”，避免客户端先看到错误的轨道帧。

### 47.3 跨域影响和证据状态

| 边界 | 关键证据 | 状态 |
| --- | --- | --- |
| `TileBlendQualificationQuery` | `Framing.cs:196-316` | `partial`（WillItBlend 空体） |
| `TileFrameTopologySystem` | `Framing.cs:199-316` | `partial` |
| `TileFrameCommitCommand` | `WorldGen.SquareTileFrame` 多处调用 | `confirmed/partial` |
| `MinecartTrackStateComponent` | `Minecart.cs:608-645`, `:953-1178`, `:1371-1378` | `partial` |
| `MinecartNeighborTopologyQuery` | `Minecart.cs:1183`, `:795-844` | `partial` |
| `MinecartTrackFramingSystem` | `Minecart.cs:953-1178` | `partial` |
| `MinecartTrackCollisionSystem` | `Minecart.cs:608-844`, `:1185-1270` | `partial` |
| `MinecartTrackSwitchCommand` | `Minecart.cs:1300-1322` | `confirmed/partial` |

本轮新增约 **8 个**细化边界，累计独立边界约 **171 个左右**；至少约 **94 个**仍有
空体、帧一致性、轨道邻居、授权或网络消费者缺口。静态识别覆盖率约 **91%～96%**，但
Tile blend 和轨道访问器会同时影响放置、碰撞、Wiring 和网络，行为闭合度约 **46%～57%**。
下一轮应继续检查 `Tile` 头部位/帧字段的所有权、`SquareTileFrame` 的调用顺序，以及轨道
状态在世界存档和 TileSnapshot 中是否被完整保存，避免轨道恢复后 front/back 状态丢失。

## 48. 第二十一轮：Tile 压缩头部的权威状态分层

本轮对 `Terraria/Tile.cs` 做字段级阅读，确认 `Tile` 不是一个可以原样迁移的单一组件。
它的公开字段和位操作方法把多个领域的权威状态压进四组 Header：Tile 活跃/非活跃、
Slope、四路 Wire、液体类型、墙色、帧号、液体检查队列、不可见/全亮标志以及颜色都共用
同一组字节。存档压缩、网络比较、液体流动、Wiring、Framing 和 Minecart 都直接读取这些
位，因此需要“领域状态组件 + PackedTileAdapter”双层设计。

### 48.1 packed header 与逻辑组件映射

`Tile` 的基础权威字段为 `type`、`wall`、`liquid`、`frameX`、`frameY`；
`sTileHeader` 通过 `active`、`inActive`、`wire`、`wire2`、`wire3` 和 slope/halfBrick 相关
方法承载结构与机关状态（`Terraria/Tile.cs:14-20`, `:633-760`）。`bTileHeader` 同时承载
wallColor、lava/honey/shimmer、水类型和第四路 Wire（`:220-245`, `:323-466`）；
`bTileHeader2` 承载 wallFrameX、frameNumber、wallFrameNumber，`bTileHeader3` 承载
wallFrameY、checkingLiquid、skipLiquid、invisibleBlock/invisibleWall 和 fullbright 标志
（`:467-613`）。

建议拆分为以下逻辑组件，但保留一个仅负责二进制兼容的适配器：

- `TileMaterialStateComponent`：type、wall、active/inActive、frameX/frameY；
- `TileSlopeStateComponent`：slope、halfBrick 和 BlockStyle；
- `TileWireStateComponent`：四路 Wire、Actuator/逻辑相关可读标志；
- `TileLiquidStateComponent`：数量、液体类型、checking/skip 队列标记；
- `TilePaintAndVisibilityComponent`：Tile/Wall color、不可见和全亮标志；
- `TileFrameStateComponent`：Tile/Wall frame number 与压缩帧辅助字段；
- `PackedTileHeaderAdapter`：逻辑组件与 `sTileHeader`/`bTileHeader*` 的位级编码转换。

`PackedTileHeaderAdapter` 是唯一允许直接改 packed 字节的边界；上层 Liquid、Wiring、Framing
和 Network/Persistence System 应通过逻辑字段或显式 Command 访问，避免两个系统互相清除
对方的位。`Tile.ClearEverything`、`ClearTile`、`ResetToType` 和 `CopyFrom` 是结构变更命令，
会同时清理多个组件（`Tile.cs:108-153`, `:259-269`），不能被当作简单字段赋值。

### 48.2 比较、存档和网络投影

`Tile.isTheSameAs` 只在特定条件下比较 frame、wallColor、wire4 和可见性，并根据是否存在
液体选择比较 `bTileHeader` 或独立颜色/电线字段（`Tile.cs:155-200`）。`WorldFile` 用它
决定 Tile 存档压缩批次（`Terraria.IO/WorldFile.cs:1617`），`NetMessage` 用它判断网络压缩
是否可复用前一 Tile（`Terraria/NetMessage.cs:1918`）。这形成独立的：

- `TileEqualityQuery`：面向存档/网络的规范化相等判断；
- `TileSaveCompressionProjection`：连续 Tile 的批量压缩投影；
- `TileNetworkDeltaProjection`：只输出客户端需要的 Tile 差异；
- `TileHeaderCompatibilityAdapter`：旧版本 Header 位和新逻辑组件之间的转换。

这些投影不能回写权威 Tile。尤其 `TileSnapshot.TileStruct.From` 会主动丢弃非重要 Tile 的
帧和无液体/无墙时的部分位（`Terraria.Utilities/TileSnapshot.cs:53-104`），它是生成一致性
快照的规范化视图，不是完整 Tile 备份；因此 TileSnapshot 恢复不能替代完整世界存档。

### 48.3 领域写入顺序和风险

建议的最小顺序为：

1. `TileMaterialMutationCommand` 提交 type/wall/active 结构变化；
2. `TileSlopeAndWireCommand` 提交坡度、半砖、Wire 和 Actuator 位；
3. `LiquidTileCommitSystem` 更新液体数量/类型及 checking/skip 队列；
4. `TileFrameTopologySystem` 依据邻居重算 frame；
5. `TileSaveCompressionProjection` 或 `TileNetworkDeltaProjection` 输出外部视图。

这不是按目录排序，而是由读写不变量决定：液体不能在 Frame 之前把邻居状态当作最终值，
网络也不能在结构提交前广播旧帧。`Tile.isTheSameAs` 在液体非零时比较整个 `bTileHeader`，
说明 liquid type、检查标志和其他位的清理顺序会直接影响网络/存档压缩；这是当前报告中
需要补充的跨域一致性风险。

### 48.4 本轮新增边界与统计

| 边界 | 关键证据 | 状态 |
| --- | --- | --- |
| `TileMaterialStateComponent` | `Tile.cs:14-20`, `:155-200` | `confirmed` |
| `TileSlopeStateComponent` | `Tile.cs:206-320` | `confirmed` |
| `TileWireStateComponent` | `Tile.cs:451-760` | `confirmed` |
| `TileLiquidStateComponent` | `Tile.cs:220-245`, `:512-550` | `confirmed/partial` |
| `TilePaintAndVisibilityComponent` | `Tile.cs:323-613` | `confirmed` |
| `PackedTileHeaderAdapter` | `Tile.cs:108-153`, `:220-760` | `confirmed/partial` |
| `TileEqualityQuery` | `Tile.cs:155-200` | `confirmed` |
| `TileSaveCompressionProjection` | `WorldFile.cs:1617`, `NetMessage.cs:1918` | `confirmed/partial` |
| `TileNetworkDeltaProjection` | `NetMessage.cs:1918` | `confirmed/partial` |

本轮新增约 **9 个**底层 Tile 状态/投影边界，累计独立边界约 **180 个左右**；至少约
**103 个**仍有 packed 位兼容、帧顺序、快照丢字段或网络/存档消费者缺口。静态识别覆盖率
约 **92%～97%**，行为闭合度约 **45%～56%**。这一轮没有新增空体，但确认了多个已记录
系统共享同一位级存储，后续 ECS 拆分必须以 `PackedTileHeaderAdapter` 为唯一编码边界，不能
让各系统复制位掩码逻辑。

## 49. 第二十二轮：TileObject 原型目录、锚点资格与多 Tile 提交

本轮继续追踪 `TileObjectData` 与 `TileObject` 的完整调用链。这里的 `TileObjectData` 不是
静态内容表那么简单：它同时定义多 Tile 尺寸、Style/Alternate、四向 Anchor、液体放置/死亡
规则、CustomPlace、CanPlace Hook 和 PostPlace Hook；`TileObject.CanPlace` 会据此读取邻居
Tile、计算预览、选择最佳 Alternate，`TileObject.Place` 再一次性写入多格 Tile，并可能先
摧毁可破坏 Tile、压平锚点和触发 TileEntity/Chest Hook。这个链条应单独作为对象放置领域，
不能并入普通单格 Tile 命令。

### 49.1 原型目录与继承覆盖

`TileObjectData` 以 `_data`、`_baseObject`、SubTiles、Alternates 和多个模块保存原型
（`Terraria.ObjectData/TileObjectData.cs:12-124`）。`GetTileData(type, style, alternate)`
先取类型，再解析 Style 子项和 Alternate；`GetTileData(Tile)` 则从 frameX/frameY 反推
Style/Alternate（`:5104-5209`）。`CopyFrom`、`FullCopyFrom`、`addTile`、`addSubTile`、
`addAlternate` 和 `Initialize` 构建继承/覆盖树，并在启动后通过 `readOnlyData` 禁止继续修改
（`:1698-1784`, `:2022-2144`）。

建议拆分为：

- `TileObjectPrototypeCatalog`：类型、Style、SubTile、Alternate 原型目录；
- `TileObjectPrototypeInheritanceSystem`：Base → SubTile → Alternate 的复制/覆盖规则；
- `TileObjectStyleResolutionQuery`：Style、Alternate、随机样式到 frame 坐标的纯解析；
- `TileObjectRuntimeLookupAdapter`：Tile 运行时 frame 到原型的反向查找；
- `TileObjectCatalogLifecycleCommand`：初始化、锁定和只读生命周期。

原型目录是内容权威配置，不是每个实体的运行时状态；Style/Alternate 结果应进入一次放置
命令或读取投影，不能让 TileObjectData 保存玩家或世界可变状态。

### 49.2 CanPlace 的资格查询与预览投影

`TileObject.CanPlace` 先检查类型、原点和世界边界，再逐 Alternate 评估方向、占用 Tile、
液体规则、墙锚点、底/顶/左/右 Anchor、平台/桌面/树干/PlanterBox 和 `AllFlatHeight`
约束（`Terraria/TileObject.cs:176-755`）。`onlyCheck` 模式会填充
`TileObjectPreviewData.placementCache`，输出尺寸、起点、Alternate 和每个格的有效/无效
标记；Hook 失败还会把预览标记为全部无效（`:318-355`, `:725-744`）。

对应边界为：

- `TileObjectPlacementQualificationQuery`：边界、占用、液体和锚点资格；
- `TileObjectAlternateSelectionQuery`：方向、Style、随机值和最佳 Alternate 选择；
- `TileObjectAnchorValidationQuery`：SolidTile、Platform、Table、Tree、Wall、EmptyTile 等锚点；
- `TileObjectPlacementPreviewProjection`：将候选结果投影为 UI/编辑器预览；
- `TileObjectPlacementHookAdapter`：调用 `HookCheckIfCanPlace` 并转换 badReturn/badResponse。

`TileObjectData.LiquidPlace`、`isValidTileAnchor`、`isValidWallAnchor` 和
`isValidAlternateAnchor` 是可独立测试的纯资格函数（`TileObjectData.cs:1864-2007`）；
`HookCheckIfCanPlace` 则可能读取 Chest 空槽、Pylon 资格、植物规则或自定义世界状态，必须
由 Adapter 调用并记录失败原因，不应把 Hook 当作 Query 内部的隐式写入。

### 49.3 多 Tile 结构提交与后置 Hook

`TileObject.Place` 根据解析后的 Style/Alternate 计算 frame 坐标，必要时先调用
`WorldGen.KillTile` 清理可破坏对象，再逐格写入 active/type/frameX/frameY；随后依据
`FlattenAnchors` 对四侧锚点执行 `WorldGen.SlopeTile`（`Terraria/TileObject.cs:29-174`）。
`WorldGen.PlaceObject` 将 `CanPlace`、`Place`、`SquareTileFrame` 和声音串起来
（`Terraria/WorldGen.cs:44298-44318`）。

建议拆分为：

- `TileObjectPlacementTransactionSystem`：多格 Tile 写入、旧物清理、压平锚点和失败回滚；
- `TileObjectFrameCommitSystem`：写入后的 SquareTileFrame/邻居帧提交；
- `TileObjectPostPlacementCommand`：PostPlace Hook 的 TileEntity、Chest、Pylon 等注册；
- `TileObjectPlacementReplicationProjection`：结构变化、TileSquare 和物品返还网络投影；
- `TileObjectPlacementSourceAdapter`：玩家、WorldGen、NPC、Projectile 等来源归因。

`TileObject.Place` 当前没有看到对“部分多格写入后 Hook 失败”的统一回滚协议；如果
PostPlace Hook 创建实体失败，Tile 已经写入的多格结构可能残留。因此事务必须先形成完整
写集和 Hook 预检，再提交结构，或在失败时恢复每个 Tile 的旧快照。

### 49.4 Hook 注册与领域消费者

`TileObjectData.Initialize` 注册了多个真实 Hook：Chest 的空槽检查和放置、FoodPlatter、
DeadCellsDisplayJar、TrainingDummy、DisplayDoll、TeleportationPylon、ItemFrame、植物
检查、LogicSensor、Kite/CritterAnchor、ProjectilePressurePad 和 WeaponsRack
（`TileObjectData.cs:2393-4843`）。这些 Hook 显示一条明确边界：TileObject 负责结构资格和
写入，Hook 负责在结构成功后创建或绑定独立 TileEntity，不能让 TileObject 直接拥有
TileEntity 内部状态。

`PlacementHook` 保存委托、badReturn、badResponse 和坐标处理标记
（`Terraria.DataStructures/PlacementHook.cs:1-49`），但 `Equals`/`GetHashCode` 为空体，
导致原型比较、缓存失效和调试差异无法确认。建议把它拆为：

- `PlacementHookDescriptorComponent`：不可变 Hook 元数据；
- `PlacementHookInvocationAdapter`：坐标转换、来源身份和错误码适配；
- `PlacementHookResultProjection`：成功/失败/全部无效结果；
- `PlacementHookEqualityQuery`：用于原型缓存的结构化比较。

### 49.5 本轮新增边界与统计

| 边界 | 关键证据 | 状态 |
| --- | --- | --- |
| `TileObjectPrototypeCatalog` | `TileObjectData.cs:12-124`, `:5104-5209` | `confirmed/partial` |
| `TileObjectPrototypeInheritanceSystem` | `TileObjectData.cs:1698-1784`, `:2022-2144` | `confirmed` |
| `TileObjectStyleResolutionQuery` | `TileObjectData.cs:2009-2018`, `:5104-5209` | `confirmed/partial` |
| `TileObjectPlacementQualificationQuery` | `TileObject.cs:176-755` | `confirmed/partial` |
| `TileObjectAnchorValidationQuery` | `TileObjectData.cs:1864-2007`, `TileObject.cs:350-710` | `confirmed` |
| `TileObjectPlacementPreviewProjection` | `TileObject.cs:318-355`, `:725-744` | `confirmed/partial` |
| `TileObjectPlacementTransactionSystem` | `TileObject.cs:29-174` | `partial`（失败回滚未闭合） |
| `TileObjectPostPlacementCommand` | `TileObjectData.cs:2393-4843` | `confirmed/partial` |
| `PlacementHookInvocationAdapter` | `PlacementHook.cs:1-49`, `TileObject.cs:38-53`, `:725-744` | `confirmed/partial` |

本轮新增约 **9 个**对象放置边界，累计独立边界约 **189 个左右**；至少约 **115 个**仍有
Hook 失败回滚、原型比较、网络投影或实体创建消费者缺口。静态识别覆盖率约 **93%～97%**，
行为闭合度约 **43%～54%**。下一步应继续核对每个 Hook 的实体创建事务、`TileObjectPreviewData`
的客户端/服务器分界，以及 `TileObjectData.CustomPlace` 对液体死亡和自定义放置的授权影响。

## 50. 第二十三轮：放置预览缓存与 PostPlace 实体注册

本轮继续核对 `TileObjectPreviewData` 和 Hook 的真实消费，确认预览状态本身也是一个需要
隔离的短生命周期投影：`placementCache` 保存最佳 Alternate 的候选结果，`randomCache` 保存
随机样式选择，`objectPreview` 则向 UI/放置流程输出当前网格。它们不能进入世界存档或作为
服务器权威 Tile 状态，但会影响下一次 `CanPlace` 的选择和客户端看到的结果。

### 50.1 预览状态和随机样式缓存

`TileObjectPreviewData` 保存 Type、Style、Alternate、Random、Size、Coordinates、ObjectStart、
二维有效性矩阵和有效百分比；`Size` 扩容时复制旧矩阵，`Reset` 清理状态，`CopyFrom` 复制
候选，`AllInvalid` 将非空格标记为无效（`Terraria.DataStructures/TileObjectPreviewData.cs:5-236`）。
`TileObject.CanPlace` 在逐 Alternate 评分时把最佳候选写入 `placementCache`，在随机样式
范围或 SpecificRandomStyles 下复用 `randomCache`（`Terraria/TileObject.cs:763-845`）。

建议拆分为：

- `TileObjectPlacementPreviewState`：一次 UI/放置请求的临时预览状态；
- `TileObjectPlacementScoreQuery`：占用/锚点/液体通过率和最佳 Alternate 评分；
- `TileObjectRandomStyleCache`：仅限同一类型/坐标的随机样式复用；
- `TileObjectPreviewLifecycleSystem`：Reset、Copy、AllInvalid 和请求结束时清理；
- `TileObjectPreviewProjection`：客户端 UI 或编辑器视图输出。

`randomCache` 不能跨玩家、跨世界或跨请求复用；当前是静态字段，生命周期和并发所有权
未显式建模。服务器接受客户端放置命令时也必须重新运行 `CanPlace`，不能信任客户端的
`objectPreview` 或随机结果。

### 50.2 PostPlace 实体注册和失败语义

`TileObjectData.Initialize` 将 PostPlace Hook 绑定到 Chest、食品盘、DisplayDoll、TrainingDummy、
Pylon、ItemFrame、LogicSensor、Kite/CritterAnchor、WeaponsRack 等对象。以 Chest 为例，
`Chest.AfterPlacement_Hook` 先把坐标转换为多 Tile 左上角，再寻找空 Chest 槽位并创建世界
Chest（`Terraria/Chest.cs:459-474`）；若槽位已满返回 `-1`。Pylon Hook 则将坐标偏移后调用
`TileEntityType<TETeleportationPylon>.Place`，放置前检查同类型 Pylon 是否已存在
（`Terraria.GameContent.Tile_Entities/TETeleportationPylon.cs:119-145`）。

建议拆分为：

- `PostPlaceEntityRegistrationCommand`：结构成功后的实体创建/绑定命令；
- `PostPlaceEntitySlotQualificationQuery`：Chest 槽位、Pylon 类型和实体坐标资格；
- `PostPlaceCoordinateAdapter`：原点、Origin 偏移和 processedCoordinates 转换；
- `PostPlaceFailureCompensationSystem`：实体创建失败时恢复 Tile、返还物品或清理残留；
- `PostPlaceEntityReplicationProjection`：实体 ID、TileEntity 更新和 Tile 网络同步。

`TETeleportationPylon.NetPlaceEntityAttempt`、`OnPlaced`、`OnRemoved` 和 `ToString` 为空体/占位
（`TETeleportationPylon.cs:7-25`），说明 Pylon 的实体注册、生命周期通知和网络尝试仍未闭合。
Chest Hook 虽然创建数组对象，但未在此处提供多 Tile 写入失败后的反向补偿。因此 PostPlace
不能简单被视为“回调”；它是结构事务的第二阶段，需要和上一节的 Tile 写集绑定。

### 50.3 预览、服务器验证与实体生命周期顺序

建议的调用顺序为：

1. 客户端生成 `TileObjectPlacementPreviewProjection`，只显示候选结果；
2. 服务器重新执行 `TileObjectPlacementQualificationQuery` 和 Hook 资格；
3. 服务器提交 `TileObjectPlacementTransactionSystem` 的完整多格写集；
4. 提交 `PostPlaceEntityRegistrationCommand`，创建 Chest/TileEntity；
5. 若注册失败，执行补偿或恢复旧 Tile；
6. 成功后执行 Frame、Liquid/Wiring 触发和网络投影。

这样可以阻止客户端预览缓存成为权威状态，也能避免先广播一个没有对应 TileEntity 的多格
对象。Pylon 资格还依赖 `Main.PylonSystem.HasPylonOfType`，因此属于世界规则/实体注册的
跨域 Query，不能只按 Tile 类型判断。

### 50.4 本轮新增边界与统计

| 边界 | 关键证据 | 状态 |
| --- | --- | --- |
| `TileObjectPlacementPreviewState` | `TileObjectPreviewData.cs:5-236` | `confirmed/partial` |
| `TileObjectPlacementScoreQuery` | `TileObject.cs:176-845` | `confirmed/partial` |
| `TileObjectRandomStyleCache` | `TileObject.cs:763-845` | `partial`（静态缓存生命周期不明） |
| `TileObjectPreviewLifecycleSystem` | `TileObjectPreviewData.cs:179-236` | `confirmed` |
| `PostPlaceEntityRegistrationCommand` | `TileObjectData.cs:2393-4843` | `confirmed/partial` |
| `PostPlaceEntitySlotQualificationQuery` | `Chest.cs:459-474`, `TETeleportationPylon.cs:126-145` | `confirmed/partial` |
| `PostPlaceFailureCompensationSystem` | `TileObject.cs:29-174`, 各 PostPlace Hook | `missing` |
| `PostPlaceCoordinateAdapter` | `PlacementHook.cs:1-49`, `TileObject.cs:38-53` | `confirmed/partial` |
| `PostPlaceEntityReplicationProjection` | TileEntity Hook 与 NetMessage 调用 | `partial` |

本轮新增约 **9 个**边界，累计独立边界约 **198 个左右**；至少约 **124 个**仍有静态缓存、
Hook 失败补偿、Pylon 生命周期或网络消费者缺口。静态识别覆盖率约 **94%～98%**，行为闭合度
约 **41%～52%**。下一步应继续检查 `TileObjectPreviewData` 是否被网络包直接读取、各
 TileEntity `NetPlaceEntityAttempt` 的服务器授权，以及多 Tile 对象拆除时实体/物品返还顺序。

## 51. 第二十四轮：TileEntity 类型注册、实体索引与网络放置

本轮从 TileObject 的 PostPlace Hook 继续向下追踪 TileEntity 的真正运行时骨架。这里的
`TileEntity` 不是一个单一组件：它同时承载类型注册、持久化 ID、空间索引、更新调度、交互
占用、存档/网络编码和实体生命周期。拆分时应把这些关系分别建模，否则一个网络包或一次
世界加载就可能绕过资格检查，直接改变客户端的实体集合。

### 51.1 类型目录和 ID 生命周期

`TileEntity.InitializeAll` 创建 `TileEntitiesManager` 并调用 `RegisterAll`；后者按固定顺序
注册 TrainingDummy、ItemFrame、LogicSensor、DisplayDoll、WeaponsRack、HatRack、FoodPlatter、
TeleportationPylon、DeadCellsDisplayJar、KiteAnchor 和 CritterAnchor（
`Terraria.DataStructures/TileEntity.cs:131-135`, `TileEntitiesManager.cs:30-45`）。
`Register` 用 `_nextEntityID++` 分配类型 ID，把一个样例对象放入 `_types`，再回调
`RegisterTileEntityID`（`TileEntitiesManager.cs:47-53`）。`TileEntityType<T>` 把该 ID 保存在
静态 `EntityTypeID`，而 Kite/Critter Anchor 还各自保存 `_myEntityID`（
`TileEntityType.cs:3-18`, `TEKiteAnchor.cs:9-26`, `TECritterAnchor.cs:9-29`）。

建议拆分为：

- `TileEntityRegistryComponent`：不可变的 type-id → 原型/工厂目录；
- `TileEntityTypeRegistrationSystem`：启动时登记类型、验证重复和注册完成状态；
- `TileEntityIdentityAllocationSystem`：分配运行时实体 ID，并区分类型 ID、持久化 ID 和网络
  传输中的临时引用；
- `TileEntityFactoryAdapter`：将 type ID 转换成具体实例，拒绝非法或未注册类型；
- `TileEntityRegistryProjection`：向存档、调试和网络握手输出已注册类型，而不是暴露可变的
  `_types` 字典。

当前 `_nextEntityID`（类型目录计数）与 `TileEntitiesNextID`（世界实体计数）名称相近但
生命周期不同；二者必须禁止混用。`TileEntityType<T>.EntityTypeID` 是静态进程级状态，重建
世界或测试并行运行时还会受到注册顺序影响，属于 `partial` 的全局目录，而不是实体组件。

### 51.2 实体索引、更新调度和占用关系

`TileEntity` 保存 `ID`、`Position`、`type` 和 `RequiresUpdates` 四个不同语义的字段，并
维护三张全局集合：`ByID` 按运行时 ID 查找，`ByPosition` 按左上角坐标查找，
`UpdateEntities` 只收集需要每帧更新的实体（`TileEntity.cs:11-35`）。`Add` 在
`EntityCreationLock` 下同时写入这些集合，`Remove` 则先删除更新列表、ID 索引和位置索引，
再调用 `OnRemoved`（`:80-128`）。这个“先解除可见性、后执行生命周期回调”的顺序是权威
语义，不能在拆分时改为回调先行。

`PerformUpdates` 触发 `_UpdateStart`，顺序遍历 `UpdateEntities` 调用 `Update`，最后触发
`_UpdateEnd`（`:48-78`）。LogicSensor 在 Start 阶段缓存玩家碰撞盒并清空待触发/待删除
列表，在 End 阶段批量 `Wiring.HitSwitch`、发 59 号网络包、再删除标记实体
（`TELogicSensor.cs:56-107`）；TrainingDummy 则在 Start 阶段清空玩家盒缓存并在 Update
中尝试激活 NPC（`TETrainingDummy.cs:20-81`）。因此建议边界为：

- `TileEntitySpatialIndexComponent`：`ByPosition` 正向空间索引；
- `TileEntityIdentityIndexComponent`：`ByID` 持久化/运行时 ID 索引；
- `TileEntityUpdateScheduleComponent`：需要更新实体的关系集合；
- `TileEntityLifecycleCommand`：Add、Remove、Clear 和世界切换；
- `TileEntityUpdatePhaseSystem`：Start → entity update → End 的显式调度；
- `TileEntityOccupancyQuery`：根据玩家 `tileEntityAnchor.interactEntityID` 判断交互占用；
- `PlayerInteractionAnchorComponent`：玩家到实体的交互关系，不能与实体空间索引合并。

`IsOccupied` 当前扫描全部 255 个玩家（`TileEntity.cs:229-246`），而
`PlayerInteractionAnchor.GetTileEntity` 又通过 ID 索引反查（`PlayerInteractionAnchor.cs:42-59`）。
这形成实体 → 玩家和玩家 → 实体两条方向不同的关系；若未来需要批量查询，应增加反向
占用关系或快照投影，不能让 Query 直接持有玩家全局数组。

### 51.3 本地放置、网络放置和实体创建命令

本地 `TileEntityType<T>.Place` 最终调用基类 `TileEntity.Place`，但基类方法目前只返回
`new int()`（`TileEntity.cs:75-78`, `TileEntityType.cs:24-30`）。这意味着 Hook 虽然会
写入多 Tile 结构，实际本地实体创建、ID 分配和 `Add` 链并未由源码闭合；这是
`TileEntityPlacementCommand` 的 `missing` 核心实现。

网络放置的路径是：消息 87 读取坐标和 type → `WorldGen.InWorld`/`ByPosition` 检查 →
`TileEntity.PlaceEntityNet` → Manager 的 `CheckValidTile` → 具体实体的
`NetPlaceEntityAttempt`（`MessageBuffer.cs:2564-2573`, `TileEntity.cs:137-145`,
`TileEntitiesManager.cs:67-75`）。但基类、`TileEntityType<T>`、LeashedAnchor、TrainingDummy、
WeaponsRack 和 Pylon 的 `NetPlaceEntityAttempt` 多为空体；服务端授权、创建实例、分配 ID、
广播 86 号完整实体包的闭环因而仍为 `partial/missing`。

消息 86 的发送端写入实体 ID 和存在标记，存在时调用 `TileEntity.Write(networkSend: true)`；
接收端读取实体、强制把 `tileEntity.ID` 设置为包中的 ID，再 `TileEntity.Add`
（`NetMessage.cs:1208-1217`, `MessageBuffer.cs:2545-2561`）。接收端必须额外校验：type 已注册、
坐标在世界内、位置没有冲突、ID 未被占用、包来源拥有放置权限。否则客户端包可以直接污染
`ByID`/`ByPosition`，不能把 `Add` 当成网络授权本身。

需要特别保留一个源码风险：消息 86 分支在读取 `num150` 之前出现无条件 `break`（
`MessageBuffer.cs:2545-2549`）。若 Version4 中该语句是实际执行代码而非反编译残留，则整个
实体删除/同步分支不可达；报告只能标记为 `partial`，需要运行时或原始版本证据确认。

建议边界为：

- `TileEntityPlacementCommand`：服务器权威的本地/脚本放置，返回新实体 ID 或失败原因；
- `TileEntityNetPlacementAuthorizationQuery`：世界边界、位置占用、类型注册和来源权限；
- `TileEntityNetworkReplicationAdapter`：86/87 包的编解码，区分“删除实体”和“同步实体”；
- `TileEntityNetworkApplySystem`：通过授权后才调用工厂、索引和生命周期命令；
- `TileEntityPlacementFailureProjection`：将非法 type、冲突坐标、容量限制等转成拒绝包/日志。

### 51.4 持久化、加载修复与实体索引重建

`WorldFile.SaveTileEntities` 在 `EntityCreationLock` 下写出 `ByID.Count`，逐实体调用
`TileEntity.Write`；`LoadTileEntities` 先 `Clear`，逐条读取实体并以顺序 `num2++` 重写 ID，
遇到同坐标实体先移除旧值，再 `Add`，最后将 `TileEntitiesNextID = num`（
`Terraria.IO/WorldFile.cs:3385-3440`）。随后它遍历 `ByPosition`，删除越界或
`manager.CheckValidTile` 失败的实体，再调用所有剩余实体的 `OnWorldLoaded`。

这条链应拆成：

- `TileEntityPersistenceAdapter`：type、ID（仅存档）、位置和 ExtraData 编码；
- `TileEntityIndexRebuildSystem`：清空并重建 ID/位置/更新索引；
- `TileEntityLoadedDataSanitizationSystem`：边界、Tile 类型、重复坐标和实体容量校验；
- `TileEntityWorldLoadedLifecycleSystem`：清理完成后按稳定顺序发出 `OnWorldLoaded`；
- `TileEntityPersistenceProjection`：将实体状态投影成存档记录，不直接写全局集合。

`TileEntity.Read` 在非法 type 时可能从 `manager.GenerateInstance` 得到 `null`，随后仍访问
`tileEntity.type`/`ReadInner`（`TileEntity.cs:180-188`）；这是加载和网络解码的空引用风险，
应由 Adapter 先返回显式 `UnknownType`，不能依赖具体实体默认构造。加载时把存档实体 ID
重写为 `0..num-1`，说明存档 ID 更像本次世界加载的临时稳定序号，而非跨世界永久身份；
任何外部引用都应在加载后重新映射。

各实体的 ExtraData 还存在不同程度缺口：ItemFrame/FoodPlatter/WeaponsRack 有物品类型、
前缀、堆叠的编码；DisplayDoll 有装备/染料/姿势位图和版本兼容分支；LogicSensor 只在
非网络存档写入 `logicCheck`/`On`；DeadCellsDisplayJar、LeashedAnchorWithItem 则是空读写。
因此要把 `TileEntityStateCodec` 按实体类型分派，并把 `networkSend` 与存档字段的差异写成
版本化协议测试，不能让所有实体共享一个“可选字段”大组件。

### 51.5 具体实体族的独立系统

从消费者和更新路径看，至少还能独立出以下领域系统：

| 实体族 | Component / System | 关键行为和证据 | 状态 |
| --- | --- | --- | --- |
| LogicSensor | `LogicSensorStateComponent`、`LogicSensorEvaluationSystem`、`LogicSensorTripBatchCommand` | Day/Night/PlayerAbove/Liquid 资格、帧更新、End 阶段批量触发 Wiring（`TELogicSensor.cs:109-154`, `:217-319`） | `confirmed/partial` |
| TrainingDummy | `TrainingDummyActivationComponent`、`TrainingDummyActivationSystem`、`TrainingDummyNpcLinkQuery` | 玩家范围命中盒、NPC 槽位和 `npc` 关联；`Activate` 为空体（`TETrainingDummy.cs:45-79`, `:137-138`） | `partial/missing` |
| ItemFrame/FoodPlatter/DeadCellsJar | `DisplayItemSlotComponent`、`DisplayItemMutationCommand`、`DisplayItemDropProjection` | TryPlacing、DropItem、TileBreak 返还和 86 号同步（各实体 `TryPlacing`/`DropItem`） | `confirmed/partial` |
| DisplayDoll/HatRack | `MannequinEquipmentComponent`、`MannequinPoseSystem`、`MannequinInteractionCommand`、`MannequinNetworkProjection` | 装备/染料/姿势、玩家更新和 121/124 包；多个交互方法及 `ToString` 为空体 | `partial` |
| Leashed Anchor/Kite/Critter | `LeashedAnchorItemComponent`、`LeashedEntityRespawnSystem`、`LeashedAnchorPlacementCommand` | 物品类型驱动实体重生、拆除掉落；`RespawnLeashedEntity`、`PlaceFromPlayerPlacementHook`、派生资格/工厂多为空体 | `missing` |
| TeleportationPylon | `PylonTypeComponent`、`PylonUniquenessQuery`、`PylonFramingDestructionSystem`、`PylonRegistryLifecycleSystem` | 3×4 结构校验、风格到物品映射、唯一性检查；`NetPlaceEntityAttempt`/`OnPlaced`/`OnRemoved` 为空体 | `partial` |

这些实体族不能只按目录拆成同一个 `TileEntitySystem`：物品展示对象的权威状态是 Item
槽位，LogicSensor 的权威状态是逻辑类型/开关/计时，TrainingDummy 的核心是 NPC 关联，
Leashed Anchor 的核心是物品到 LeashedEntity 的派生关系，Pylon 还受世界规则/实体注册表约束。

### 51.6 交互网络命令和拆除顺序

消息 121 只允许 DisplayDoll 读取指定物品/姿势数据，找不到实体时调用
`ReadDummySync`；消息 122 设置或清除玩家 `PlayerInteractionAnchor`，并在设置前通过
`IsOccupied` 检查占用；消息 123/133/156 分别更新 WeaponsRack、FoodPlatter 和
LeashedAnchor 的物品；消息 124 更新 HatRack；消息 125 仅回传玩家来源和槽位信息
（`MessageBuffer.cs:2985-3058`, `:3116-3124`, `:3310-3317`, `NetMessage.cs:1445-1500`）。

建议拆分为：

- `TileEntityInteractionAuthorizationQuery`：玩家距离、拥有权、占用和目标实体类型；
- `TileEntityItemMutationCommand`：服务端校验后修改单个物品槽位；
- `TileEntityInteractionAnchorSystem`：维护玩家 Anchor 的设置、清除和实体移除联动；
- `TileEntityInteractionNetworkProjection`：121–125、133、156 的最小增量投影；
- `TileEntityBreakCompensationSystem`：先保存实体物品，再移除实体，最后生成掉落并广播 Tile/实体变化。

当前 `WorldGen` 在多个 Tile 拆除分支中先检查实体物品再 DropItem，然后 Kill 实体或继续
清 Tile（`WorldGen.cs:36841-36895`, `39446-39452`, `39782-39804`, `52706-52903`）。
这些调用没有统一事务边界，物品掉落、实体索引删除、TileSquare 广播和失败重试的顺序只
能从各分支局部推断；应标记为 `partial`，并要求一个跨对象的拆除命令保证“实体状态快照 →
索引解除 → 物品投影 → Tile 破坏 → 网络广播”的可重复顺序。

### 51.7 本轮新增边界与统计

| 边界 | 关键证据 | 状态 |
| --- | --- | --- |
| `TileEntityRegistryComponent` / `TileEntityTypeRegistrationSystem` | `TileEntity.cs:131-135`, `TileEntitiesManager.cs:30-53` | `confirmed/partial` |
| `TileEntityIdentityAllocationSystem` | `TileEntitiesManager.cs:12-22`, `WorldFile.cs:3409-3419` | `partial`（类型 ID 与实体 ID 生命周期易混） |
| `TileEntitySpatialIndexComponent` / `TileEntityIdentityIndexComponent` | `TileEntity.cs:11-35`, `:80-128` | `confirmed` |
| `TileEntityUpdatePhaseSystem` | `TileEntity.cs:48-78`, `TELogicSensor.cs:56-107` | `confirmed/partial` |
| `TileEntityPlacementCommand` | `TileEntity.cs:75-78`, `TileEntityType.cs:24-30` | `missing` |
| `TileEntityNetPlacementAuthorizationQuery` | `MessageBuffer.cs:2564-2573`, `TileEntitiesManager.cs:58-75` | `partial` |
| `TileEntityNetworkReplicationAdapter` / `TileEntityNetworkApplySystem` | `NetMessage.cs:1208-1223`, `MessageBuffer.cs:2545-2573` | `partial`（86 分支存在无条件 break 风险） |
| `TileEntityPersistenceAdapter` / `TileEntityIndexRebuildSystem` | `WorldFile.cs:3385-3440` | `confirmed/partial` |
| `TileEntityLoadedDataSanitizationSystem` | `WorldFile.cs:3420-3439` | `confirmed/partial` |
| `TileEntityInteractionAnchorSystem` | `PlayerInteractionAnchor.cs:20-59`, `MessageBuffer.cs:3001-3021` | `confirmed/partial` |
| `TileEntityItemMutationCommand` | `MessageBuffer.cs:3019-3058`, `:3116-3124`, `:3310-3317` | `confirmed/partial` |
| `TileEntityBreakCompensationSystem` | `WorldGen.cs:36841-36895`, `:39446-39452`, `:52706-52903` | `missing` |
| `LogicSensorEvaluationSystem` / `TrainingDummyActivationSystem` | `TELogicSensor.cs:109-154`, `TETrainingDummy.cs:45-138` | `confirmed/partial` |
| `MannequinEquipmentComponent` / `LeashedAnchorItemComponent` / `PylonRegistryLifecycleSystem` | `TEDisplayDoll.cs`, `TELeashedEntityAnchorWithItem.cs`, `TETeleportationPylon.cs` | `partial/missing` |

本轮新增约 **14 个**边界，累计独立边界约 **212 个左右**；至少约 **136 个**仍存在本地
Place 空体、网络 86/87 授权、实体 ExtraData 空读写、交互包消费者、静态类型 ID、Anchor
重生和拆除补偿缺口。静态识别覆盖率约 **95%～98%**，权威行为闭合度约 **39%～50%**；
这仍是代码证据报告，不代表 Version4 已具备可运行的完整 TileEntity 子系统。
这仍是代码证据报告，不代表 Version4 已具备可运行的完整 TileEntity 子系统。

## 52. 第二十五轮：传送水晶登记与拴系实体分区运行时

继续追踪 TileEntity 的消费者后，发现 Pylon 和 Kite/Critter Anchor 并不是普通的附属对象：
前者把 TileEntity 投影为可传送目录，后者把 TileEntity 的物品槽位投影为按网络分区激活的
LeashedEntity。两条链都依赖独立的注册表、区域生命周期和网络模块，不能并入一般的
`TileEntityUpdateSystem`。

### 52.1 Pylon 目录、唯一性和网络投影

`TeleportPylonsSystem.Update` 以 `int.MaxValue` 冷却为变更触发器，交换 `_pylons`/
`_pylonsOld`，扫描 `TileEntity.ByPosition` 中的 `TETeleportationPylon`，通过
`TryGetPylonType` 生成 `TeleportPylonInfo(PositionInTiles, TypeOfPylon)`，再用集合差异广播
PylonWasAdded/PylonWasRemoved（`Terraria.GameContent/TeleportPylonsSystem.cs:15-71`）。
玩家加入时只发送当前 `_pylons` 快照（`:80-89`），WorldGen 清理时调用 `PylonSystem.Reset`
（`Terraria/WorldGen.cs:6565-6568`），主循环每帧调用 `PylonSystem.Update`
（`Terraria/Main.cs:13119`）。

建议拆分为：

- `PylonDirectoryComponent`：当前已确认的 Pylon 位置/类型集合；
- `PylonDirectoryRefreshSystem`：TileEntity 索引 → 目录快照和 old/new 差异；
- `PylonUniquenessQuery`：同类型唯一性和放置前拒绝；
- `PylonDirectoryLifecycleCommand`：Reset、玩家加入快照和世界切换；
- `PylonNetworkProjection`：NetTeleportPylonModule 的 add/remove/teleport 子包；
- `PylonMapProjection`：地图层读取目录而不是直接扫描实体。

`TeleportPylonInfo.Equals` 当前返回默认 `bool`（`TeleportPylonInfo.cs:12-15`），因此
`Except` 的增删差异判断并未由源码闭合；`NetTeleportPylonModule.Deserialize` 也是默认
返回值（`NetTeleportPylonModule.cs:29-31`），客户端目录收到网络包后的应用路径缺失。
这两个缺口使 Pylon 目录至少为 `partial`。此外，`TETeleportationPylon.OnPlaced`/
`OnRemoved` 和 `NetPlaceEntityAttempt` 为空体，目录只能靠周期扫描，无法保证实体变化到
目录广播之间的即时一致性。

### 52.2 LeashedEntity 原型、区域激活和网络流

`LeashedEntity.Registry.RegisterAll` 建立原型列表：Kite、Walker/Crawler/Snail/Runner、
Flyer 及多种鸟、鱼、蝴蝶和水生 Critter（`Terraria.GameContent/LeashedEntity.cs:44-93`）。
运行时实体同时被放入按区块索引的 `BySection`、按网络 `whoAmI` 索引的 `ByWhoAmI`，每个
Section 最多维护 32 槽并记录 `count/emptySlots/sectionSlot`（`:96-181`）。区域激活时
逐实体 `Spawn`，停用时 `Despawn`；`UpdateEntities` 先依据 `ActiveSections` 重算活动区，
再更新实体、发送流式网络变化，最后移除失活实体（`:135-160`, `:236-288`）。

建议拆分为：

- `LeashedPrototypeRegistryComponent`：原型 → Type ID 目录；
- `LeashedEntityIdentityComponent`：Type、whoAmI、AnchorPosition 和 SectionCoordinates；
- `LeashedSectionIndexComponent`：按网络区块的槽位关系和压缩策略；
- `LeashedSectionActivationSystem`：活动区进入/离开时 Spawn/Despawn；
- `LeashedEntityUpdateSystem`：活动区批处理、失活移除和帧更新；
- `LeashedEntityNetworkAdapter`：全量/增量 NetSend、NetReceive 和按区块广播；
- `LeashedEntityProjection`：绘制和客户端可见实体视图。

当前 `LeashedEntity.NetModule.Deserialize`、`Remove`、`StreamNetUpdates`、基类
`NewInstance`/`Spawn`/`Despawn`/`Update`/`NetSend`/`NetReceive` 多为空体或默认实现
（`:18-20`, `:233`, `:290`, `:298-306`）。`BySection`、`ByWhoAmI` 的静态初始化和新增实体
写入路径也未在 Version4 中找到完整消费者。因此区域激活、失活回收、网络接收和类型工厂
均标记为 `partial/missing`，不能把 Anchor 的 `CreateLeashedEntity` 视为已完成实体链。

### 52.3 Anchor 与 LeashedEntity 的两阶段状态

`TELeashedEntityAnchorWithItem.itemType` 是 TileEntity 内的权威输入；`InsertItem` 写入它并
调用 `RespawnLeashedEntity`，拆 Tile 时 `DropItemForTileBreak` 生成物品并清零
（`TELeashedEntityAnchorWithItem.cs:12-29`）。但 `RespawnLeashedEntity`、
`PlaceFromPlayerPlacementHook`、Anchor 的 `IsTileValidForEntity`、`GenerateInstance`、
`FitsItem` 和 `CreateLeashedEntity` 在 Kite/Critter 路径上多为空体/默认对象
（`TELeashedEntityAnchor.cs:8-12`, `TELeashedEntityAnchorWithItem.cs:34-36`,
`TEKiteAnchor.cs:23-48`, `TECritterAnchor.cs:25-49`）。

因此应明确两阶段顺序：

1. `LeashedAnchorItemQualificationQuery` 验证物品是否属于 Kite/Critter 原型；
2. `LeashedAnchorItemMutationCommand` 写入 `itemType`；
3. `LeashedEntitySpawnCommand` 从原型工厂创建实体并登记 Section/whoAmI；
4. `LeashedEntitySectionActivationSystem` 根据区块状态 Spawn/Despawn；
5. `LeashedAnchorBreakCompensationSystem` 先移除派生实体，再掉落物品和 Tile；
6. `LeashedEntityNetworkProjection` 广播全量或增量包。

Anchor 的 ItemData、LeashedEntity 的运行时运动状态、NPC/Projectile dummy 和客户端绘制
状态必须分开；否则网络重放或区域切换会把表现实体误当成持久化 TileEntity。

### 52.4 调度顺序和未闭合边界

启动顺序是 `TileEntity.InitializeAll` → `LeashedEntity.Registry.RegisterAll` →
`PylonSystem = new TeleportPylonsSystem`（`Main.cs:3347-3370`）。主更新路径先处理普通
实体和玩家，再执行 `LeashedEntity.UpdateEntities`，之后推进时间；Pylon 则在主循环另一处
单独调用 `Update`。建议在 ECS 调度器中声明：

`TileObject/PostPlace → TileEntity Add → Leashed spawn / Pylon refresh → section update →
network projection`。

目前缺少可证明的 Add→OnPlaced、Anchor→Leashed spawn、Pylon Add→目录刷新和网络包接收→
实体工厂的闭合链；这些应继续列入 `partial/missing`，并以 focused verifier 覆盖：类型
注册顺序、非法 type、重复坐标、区块激活边界、实体移除后目录差异、网络全量/增量重放。

### 52.5 本轮新增边界与统计

| 边界 | 关键证据 | 状态 |
| --- | --- | --- |
| `PylonDirectoryComponent` / `PylonDirectoryRefreshSystem` | `TeleportPylonsSystem.cs:15-71` | `partial`（Equals 未实现） |
| `PylonUniquenessQuery` / `PylonDirectoryLifecycleCommand` | `TETeleportationPylon.cs:119-136`, `TeleportPylonsSystem.cs:76-89` | `partial` |
| `PylonNetworkProjection` | `NetTeleportPylonModule.cs:16-31` | `partial/missing`（Deserialize 默认） |
| `LeashedPrototypeRegistryComponent` | `LeashedEntity.cs:44-93` | `confirmed/partial` |
| `LeashedEntityIdentityComponent` / `LeashedSectionIndexComponent` | `LeashedEntity.cs:96-181` | `partial` |
| `LeashedSectionActivationSystem` / `LeashedEntityUpdateSystem` | `LeashedEntity.cs:135-160`, `:236-288` | `confirmed/partial` |
| `LeashedEntityNetworkAdapter` | `LeashedEntity.cs:18-31`, `:290-306` | `missing` |
| `LeashedAnchorItemMutationCommand` / `LeashedEntitySpawnCommand` | `TELeashedEntityAnchorWithItem.cs:12-36` | `missing` |
| `LeashedAnchorBreakCompensationSystem` | `WorldGen.cs:52726-52903` | `partial/missing` |

本轮新增约 **9 个**边界，累计独立边界约 **221 个左右**；至少约 **145 个**仍存在
Pylon 比较/网络反序列化、LeashedEntity 索引新增与移除、Anchor 重生、实体工厂、拆除补偿
和客户端投影缺口。静态识别覆盖率约 **96%～98%**，权威行为闭合度约 **36%～47%**；
Version4 仍应视为“已识别大量子系统、但关键运行时链路未完全展开”的代码证据快照。
Version4 仍应视为“已识别大量子系统、但关键运行时链路未完全展开”的代码证据快照。

## 53. 第二十六轮：安全传送落点、海洋回溯与危险 Tile 查询

本轮继续沿 Pylon/玩家传送消费者追踪 `TeleportHelpers`。该模块不是单纯数学工具：它读取
玩家碰撞盒、重力、液体、危险 Tile、世界边界、Skyblock 特殊规则，并直接修改玩家速度或
由调用方执行 Teleport/网络广播。因此应拆成纯落点查询、世界规则策略和传送提交三个层次。

### 53.1 最近安全落点查询

`FindClosestTeleportSpotNoSpace` 以玩家当前位置为中心扫描约 50×50 Tile，先把玩家速度
清零，再要求候选上方无固体/液体、下方有可站立地面，同时排除危险 Tile、熔岩、伤害 Tile
和 `Collision.SolidCollision`，按到原位置的距离选择最近点（`TeleportHelpers.cs:7-47`）。
它返回候选位置，但已经执行了 `player.velocity = Vector2.Zero`，把纯 Query 与状态修改混在
一起。唯一消费者是 `Player.cs:3814-3831`，随后直接写入玩家位置/传送效果。

建议拆分为：

- `TeleportSearchBoundsQuery`：根据玩家位置和世界尺寸计算扫描矩形；
- `TeleportCandidateQualificationQuery`：固体、液体、熔岩、伤害 Tile 和碰撞资格；
- `NearestTeleportSpotQuery`：按距离和稳定平局规则选择落点；
- `TeleportVelocityPolicy`：决定传送前后速度和重力处理；
- `PlayerTeleportCommand`：提交位置、无敌帧、抓钩清理和 65 号网络投影；
- `TeleportVisualEffectsProjection`：Dust、声音和客户端特效。

`TileIsDangerous` 与 `IsInSolidTilesExtended` 当前均为默认 `false`（
`TeleportHelpers.cs:205-212`），所以危险环境和扩展碰撞的真实拒绝条件未闭合；该查询只能
标记 `partial/missing`，不能作为安全传送权威实现。

### 53.2 Magic Conch 海洋落点策略

`RequestMagicConchTeleportPosition` 在左右海洋边界建立起点，按 `crawlOffsetX` 在 X 方向
爬行、按重力方向在 Y 方向搜索，结合 Skyblock `lowTiles`、世界表面、液体、危险 Tile、
墙体和十 Tile 范围内的支撑地面，最多尝试 5000 步/400 次横向移动（`TeleportHelpers.cs:50-203`）。
失败时返回 false；成功返回海洋/天空岛边界附近的 Tile 点。消费者
`Player.cs:26612-26622` 在左右海洋依次尝试，再用该点构造世界坐标并发起传送。

建议拆分为：

- `OceanTeleportBoundaryQuery`：左右海洋初始 X、世界表面和 Skyblock 偏移；
- `ConchLandingTraversalSystem`：沿 X/Y 的有界搜索状态机；
- `ConchLandingSupportQuery`：支撑地面、液体和危险 Tile 判定；
- `ConchTeleportFallbackCommand`：首选海洋失败后的反向海洋回退；
- `ConchLandingProjection`：将 Tile 落点转换为玩家世界坐标。

该实现依赖 `WorldGen.Skyblock.lowTiles`、`Main.worldSurface` 和玩家 `gravDir` 等跨域全局
状态；若把它并入普通 `NearestTeleportSpotQuery`，会丢失特殊种子和重力语义。两个内部
危险/碰撞函数为空体，且没有看到针对最大步数、边界夹紧和异常 Tile 的 focused verifier，
所以海洋传送链保持 `partial`。

### 53.3 传送提交与网络权威边界

Recall、Magic Conch、Demon Conch、Shellphone 等入口最终在 `Player` 内执行 Teleport、
重置速度、清理抓钩、更新无敌/传送样式、检查网络 Section，并发送 65 号消息
（`Player.cs:3814-3838`, `:26612-26660`）。这些提交动作同时包含权威状态、外部 Section
激活和表现效果，建议声明以下顺序：

`落点 Query → 服务器权限/冷却 Query → PlayerTeleportCommand → SectionActivationCommand
→ 65 号网络投影 → Dust/声音 Projection`。

客户端预测落点不能直接改变服务器玩家坐标；服务端必须重新运行候选资格，尤其是危险 Tile
和液体检查。`TeleportHelpers` 当前没有来源身份或冷却参数，说明授权应留在上层命令而不是
查询内部。

### 53.4 本轮新增边界与统计

| 边界 | 关键证据 | 状态 |
| --- | --- | --- |
| `TeleportSearchBoundsQuery` / `TeleportCandidateQualificationQuery` | `TeleportHelpers.cs:7-47` | `confirmed/partial` |
| `NearestTeleportSpotQuery` / `TeleportVelocityPolicy` | `TeleportHelpers.cs:14-45`, `Player.cs:3814-3838` | `partial` |
| `OceanTeleportBoundaryQuery` / `ConchLandingTraversalSystem` | `TeleportHelpers.cs:50-203` | `confirmed/partial` |
| `ConchLandingSupportQuery` | `TeleportHelpers.cs:128-212` | `missing`（危险/扩展碰撞函数默认值） |
| `PlayerTeleportCommand` / `SectionActivationCommand` | `Player.cs:3814-3838`, `:26612-26660` | `partial` |
| `TeleportNetworkProjection` / `TeleportVisualEffectsProjection` | `Player.cs:3814-3838`, `:26630-26660` | `confirmed/partial` |

本轮新增约 **6 个**边界，累计独立边界约 **227 个左右**；至少约 **151 个**仍存在危险
Tile 查询、扩展碰撞、特殊种子回退、传送授权/冷却和网络 Section 联动缺口。静态识别覆盖率
约 **96%～98%**，权威行为闭合度约 **34%～45%**。本轮没有修改 Version4 源码，仅更新
拆分设计报告。
本轮没有修改 Version4 源码，仅更新
拆分设计报告。

## 54. 第二十七轮：NPC 住房洪泛检查、房间评分与居住关系

进一步核对 `TownRoomManager` 与 `WorldGen` 的住房调用，发现 NPC 住房是独立的权威模拟域，
并非简单的 NPC 属性。它把 Tile/墙体拓扑、房间可达性、家具需求、邪恶环境评分、NPC 共居
偏好、房间占用和持久化关系串成多个阶段。

### 54.1 房间拓扑与资格查询

`StartRoomCheck` 重置 `roomX1..roomY2`、`roomTiles`、`houseTile`、臭虫标记和失败原因，
以栈执行 8 邻域洪泛；`CheckRoom` 检查世界边界、最大尺寸/Tile 数、实心阻挡、开门状态、
墙体连续性和危险臭虫，并把已扫描 Tile 加入 `roomTiles`（`WorldGen.cs:5699-5893`）。
`RoomNeeds` 再从 `houseTile` 判断椅子、桌子、门和光源是否齐全（`:5310-5358`）。

建议拆分为：

- `HousingRoomTopologyComponent`：一次检查的边界、BitSet 房间掩码、扫描计数和失败原因；
- `HousingFloodFillSystem`：有界 8 邻域扫描与墙体/阻挡判定；
- `HousingFurnitureRequirementQuery`：家具类别到 `houseTile` 的纯资格计算；
- `HousingRoomFailureProjection`：将边界、墙洞、尺寸、开门和臭虫原因输出给 UI/日志；
- `HousingRoomSnapshotProjection`：把成功房间的边界和 Tile 集合提供给评分与 NPC 生成。

当前洪泛使用 `WorldGen` 静态 `_roomCheckStack`、`roomTiles`、`canSpawn` 等共享可变状态，
反馈接口还可能要求继续扫描或遇到失败立即停止。并行检查两个 NPC 会互相覆盖这些字段，
因此只能标记为 `partial`；ECS 迁移时每个房间检查必须拥有独立上下文。

### 54.2 评分、占用和共居规则

`ScoreRoom` 先检查已有 NPC 是否占用房间，再统计区域 Tile 类别，对腐化/猩红/神圣环境调整
基础分，遍历可作为 Home Spot 的地板，依据墙体、家具、箱子、门和共享房间距离计算分数，
最后选出 `hiScore/bestX/bestY`（`WorldGen.cs:5447-5640`）。`IsRoomConsideredOccupiedForNPCIndex`
与 `TownRoomManager.CanNPCsLiveWithEachOther` 依据 `housingCategory` 判定是否允许共居。

`TownRoomManager` 维护 `_roomLocationPairs` 和 `_hasRoom[NPCID]`：`SetRoom` 按 NPC 类型
替换旧位置，`KickOut` 删除关系并清除快速索引，`AddOccupantsToList` 按房间坐标反查住户，
`HasRoomQuick/HasRoom` 提供正向查询（`TownRoomManager.cs:19-100`）。建议拆分为：

- `HousingAssignmentComponent`：NPC 类型 → 房间坐标关系；
- `HousingOccupancyRelation`：房间坐标 → NPC 类型反向关系；
- `HousingAssignmentCommand`：SetRoom、KickOut、搬家和占用替换；
- `HousingScoreQuery`：环境/家具/距离评分和最佳 Home Spot；
- `NpcCohabitationQualificationQuery`：housingCategory 共居资格；
- `HousingStatusProjection`：无家、已分配房间、住户列表和失败原因。

`_hasRoom` 以 NPC 类型而非 NPC 实例索引，意味着同类型 NPC 共享一个快速布尔值；实际位置
列表也只保留每类型一条关系。该语义必须在拆分中保留，否则会错误地把多个同类型 NPC 当成
独立房间分配。`SetRoom(int, Point)` 在锁内替换列表，但 `SetRoom(int, int, int)` 先在锁外
写 `_hasRoom[npcID]`，存在观察到半更新状态的窗口，需标记为 `partial`。

### 54.3 持久化和 NPC 生成顺序

`TownRoomManager.Save/Load` 只序列化 NPC 类型和房间坐标；`WorldGen.moveRoom` 先把 NPC 标记
为 homeless、重新生成，再调用 `SetRoom`；`QuickFindHome` 则是“房间洪泛 → 特殊 NPC 条件
→ 家具需求 → 评分 → 共居检查 → 写回 NPC homeTileX/Y 与 homeless”顺序
（`TownRoomManager.cs:104-145`, `WorldGen.cs:4558-4570`, `:5068-5138`）。

建议使用显式流水线：

`HousingRoomCheckCommand → HousingFurnitureRequirementQuery → HousingScoreQuery →
NpcCohabitationQualificationQuery → HousingAssignmentCommand → TownNpcSpawnProjection`。

加载时若房间位置越界、NPC 类型非法或关系重复，当前 `Load` 直接写入数组/列表，没有看到
统一清理和版本校验；这属于 `HousingPersistenceSanitizationSystem` 的缺口。住房状态变化
还会影响 NPC 生成、商店快乐度和 Pylon 邻居规则，不能让 UI 投影反向修改 `_hasRoom`。

### 54.4 本轮新增边界与统计

| 边界 | 关键证据 | 状态 |
| --- | --- | --- |
| `HousingRoomTopologyComponent` / `HousingFloodFillSystem` | `WorldGen.cs:5699-5893` | `confirmed/partial`（共享静态扫描上下文） |
| `HousingFurnitureRequirementQuery` | `WorldGen.cs:5310-5358` | `confirmed` |
| `HousingScoreQuery` | `WorldGen.cs:5447-5640` | `confirmed/partial` |
| `HousingAssignmentComponent` / `HousingOccupancyRelation` | `TownRoomManager.cs:11-100` | `confirmed/partial` |
| `HousingAssignmentCommand` | `TownRoomManager.cs:65-100`, `WorldGen.cs:4558-4570` | `partial` |
| `NpcCohabitationQualificationQuery` | `TownRoomManager.cs:157-176`, `WorldGen.cs:5427-5464` | `confirmed/partial` |
| `HousingPersistenceSanitizationSystem` | `TownRoomManager.cs:104-145` | `missing` |
| `TownNpcSpawnProjection` | `WorldGen.cs:5068-5138` | `partial` |

本轮新增约 **8 个**边界，累计独立边界约 **235 个左右**；至少约 **159 个**仍有共享房间
扫描状态、同类型 NPC 快速索引、住房加载清理、评分副作用和生成顺序缺口。静态识别覆盖率
约 **97%～98%**，权威行为闭合度约 **33%～44%**。
约 **33%～44%**。

## 55. 第二十八轮：SceneMetrics 区域扫描、环境聚合与多消费者投影

对未完全展开的跨域入口继续追踪后，`SceneMetrics` 是一个新的核心边界。它被玩家、主循环、
NPC 生成、Pylon 列表、世界物品、音乐/背景、生态条件和 UI 同时读取；其字段既有 Tile/液体
计数，也有区域布尔值、最近 NPC 位置、光源/蜡烛/纪念碑状态和玩家视角效果。它不应作为
一个巨型 ECS 组件，而应视为“扫描快照 + 多个领域投影”。

### 55.1 扫描快照与刷新缓存

`SceneMetrics.Scan` 以 `LastScanTime` 和扫描中心判断缓存是否仍有效；失效时执行
`Reset → ScanTiles → ScanOnScreenTiles(可选) → ScanNPCPositions(可选) → AggregateTileCounts
→ CalculateZones → AddPlayerEffects(可选)`，最后由 ActiveMusicBox 计算
`CanPlayCreditsRoll`（`Terraria/SceneMetrics.cs:191-231`）。主循环维护独立的
`_playerSceneMetrics` 与 `_cameraSceneMetrics`，在摄像机跟踪对象时切换 `Main.SceneMetrics`
投影（`Terraria/Main.cs:640-644`, `:1440-1450`, `:12265-12313`）；玩家还可单独调用
`UpdateSceneMetrics`（`Player.cs:9926-9946`）。

建议拆分为：

- `SceneScanContextComponent`：中心 Tile、世界坐标、扫描时间和选项；
- `SceneTileCountSnapshot`：Tile/液体分类计数；
- `SceneNpcPositionSnapshot`：最近 NPC 位置和 Town NPC 数量；
- `SceneScanCacheSystem`：按时间/中心失效并协调扫描阶段；
- `SceneMetricsResetCommand`：清理所有派生字段和缓存；
- `PlayerSceneMetricsProjection` / `CameraSceneMetricsProjection`：分别供玩家规则和摄像机/UI 使用。

当前 `ScanTiles`、`ScanOnScreenTiles`、`AggregateTileCounts`、`CalculateZones`、
`ScanNPCPositions` 和 `AddPlayerEffects` 全部为空体（`SceneMetrics.cs:218-223`）。因此所有
Zone*、TileCount、蜡烛/纪念碑、旗帜增益、最近 NPC 和最佳矿石字段的计算链未闭合；
`SceneMetrics.Scan` 只能标记 `partial/missing`，不能把其输出当作已验证环境权威。

### 55.2 环境区域和游戏规则消费者

玩家的 `ZoneDungeon`、`ZoneCorrupt`、`ZoneHallow`、`ZoneJungle`、`ZoneSnow`、`ZoneDesert`、
`ZoneGlowshroom`、`ZoneBeach`、`ZoneRain`、`ZoneSandstorm`、`ZoneShimmer` 等属性直接读取
`Main.PlayerSceneMetrics`（`Player.cs:2617-2940`）。NPC 生成依据这些 Zone 和计数选择敌怪、
史莱姆、蠕虫、墓地、蘑菇和特殊种子行为（`NPC.cs:4410-5119`）；主循环依据 Zone 与
`BloodTileCount/EvilTileCount/HolyTileCount/HoneyBlockCount` 选择背景/音乐
（`Main.cs:12499-12531`）；住房评分又读取蘑菇 Tile 计数阈值（`WorldGen.cs:4630-4643`）。

建议按输出语义再分层：

- `BiomeZoneProjection`：高度层、腐化/猩红/神圣、沙漠/雪地/丛林/海滩等布尔区域；
- `HazardAndLiquidMetricsProjection`：熔岩、蜂蜜、微光、伤害和液体计数；
- `LightAndFurnitureMetricsProjection`：营火、太阳花、花园侏儒、蜡烛、音乐盒、喷泉、纪念碑；
- `NpcProximityMetricsProjection`：最近 NPC、旗帜增益和 Town NPC 数量；
- `WorldRuleEligibilityQuery`：从快照计算 NPC 生成、Pylon、住房和传送资格；
- `SceneMusicBackgroundProjection`：把区域快照映射为音乐/背景选择和演职员表资格。

这些投影都应只读同一帧快照；NPC 生成或背景选择不能回写扫描缓存。玩家视角和摄像机视角
拥有不同中心与扫描范围，禁止用静态单例覆盖彼此结果。

### 55.3 SceneState 与表现副作用

`SceneState.Update(SceneMetrics)` 进一步把快照传给光照衰减、微光、墓地和 RGB 外设探针；其中
`ApplyVisuals`、`UpdateLightDecay`、`UpdateShimmer`、`UpdateGraveyard` 和
`UpdateRGBPeriheralProbe` 存在空体或表现副作用（`Terraria/SceneState.cs:43-68`）。这说明
环境模拟和表现系统已经有潜在边界：SceneMetrics 应停留在纯快照，SceneState 才能消费快照
并发出光照/着色/外设效果命令。

### 55.4 本轮新增边界与统计

| 边界 | 关键证据 | 状态 |
| --- | --- | --- |
| `SceneScanContextComponent` / `SceneScanCacheSystem` | `SceneMetrics.cs:191-231`, `Main.cs:12265-12313` | `confirmed/partial` |
| `SceneTileCountSnapshot` / `SceneNpcPositionSnapshot` | `SceneMetrics.cs:80-190` | `confirmed/partial` |
| `BiomeZoneProjection` | `SceneMetrics.cs:28-86`, `Player.cs:2617-2940` | `missing`（计算阶段为空体） |
| `HazardAndLiquidMetricsProjection` | `SceneMetrics.cs:106-190` | `missing` |
| `LightAndFurnitureMetricsProjection` | `SceneMetrics.cs:126-176` | `missing` |
| `NpcProximityMetricsProjection` | `SceneMetrics.cs:100-190`, `SceneMetrics.cs:320-331` | `missing` |
| `WorldRuleEligibilityQuery` | `NPC.cs:4410-5119`, `WorldGen.cs:4630-4643` | `partial` |
| `SceneMusicBackgroundProjection` | `Main.cs:12499-12531` | `partial` |
| `SceneStateEffectSystem` | `SceneState.cs:43-68` | `partial/missing` |

本轮新增约 **9 个**边界，累计独立边界约 **244 个左右**；至少约 **168 个**仍有扫描阶段
空体、玩家/摄像机缓存分裂、区域计数、NPC 生成资格、表现副作用和静态失效条件缺口。静态
识别覆盖率约 **97%～99%**，权威行为闭合度约 **29%～40%**。这进一步解释了为什么仅按
目录或类名统计会高估 Version4 的“已完成系统”数量。
目录或类名统计会高估 Version4 的“已完成系统”数量。

## 56. 第二十九轮：WorldItem 生命周期、拾取保留与槽位回收

`WorldItem` 仍有一条未完全展开的权威对象链。它不是 `Item` 的简单包装，而是负责世界中
掉落物的实体位置、物理、玩家保留、敌怪拾取、熔岩销毁、微光转换、合并、过期、区段同步和
槽位回收；`Item.NewItem` 又负责把内容原型转成一个可广播的 WorldItem 实例。

### 56.1 状态所有权和生成命令

`WorldItem` 以 `inner` 保存物品内容，以 `ownTime`、`playerIndexTheItemIsReservedFor`、
`ownIgnore`、`keepTime`、`timeSinceItemSpawned`、`timeLeftInWhichTheItemCannotBeTakenByEnemies`、
`beingGrabbed`、`onConveyor` 和 `shimmered/shimmerTime` 保存世界掉落生命周期状态
（`Terraria/WorldItem.cs:15-57`）。`Item.NewItem` 先拒绝世界生成/加载阶段和非法堆叠，
选择 0–399 槽位，必要时触发旧物品替换或 `EmergencyStacking`，再设置类型、前缀、堆叠、
位置、速度、湿润状态、保护计时和网络 21 号广播（`Terraria/Item.cs:48688-48806`）。

建议拆分为：

- `WorldItemStateComponent`：位置、速度、物理介质、保留玩家和生命周期计时；
- `WorldItemContentComponent`：Item 类型、前缀、堆叠、收藏和表现标志；
- `WorldItemSlotAllocationCommand`：400 槽位选择、替换、复用延迟和紧急堆叠交接；
- `WorldItemSpawnCommand`：从 `IEntitySource` 创建掉落实体并初始化广播字段；
- `WorldItemReservationSystem`：FindOwner、保留玩家切换、敌怪/玩家拾取权限；
- `WorldItemContentProjection`：21/22/39/145/148/151/160 号网络和区段快照。

`WorldItem.OverrideWith` 直接替换 `inner`，但没有同步所有生命周期字段；`TurnToAir`、
`SetDefaults` 和槽位替换则会重置不同子集。迁移时必须分别定义“清空内容”和“释放实体槽位”，
不能用单一 `Active` 布尔覆盖这两个操作。

### 56.2 每帧更新、物理和拾取关系

`UpdateItem` 按顺序处理槽位复用延迟、实例物品清除、重力/湿润介质、own/keep 计时、雨水
转换、微光状态、邻近合并、敌怪拾取、世界边界、特殊过期、物理移动、熔岩死亡、网络和
视觉效果（`WorldItem.cs:357-558`）。`FindOwner` 扫描全部玩家，结合 `CanPullItem`、
磁铁范围、距离和 Hopper 规则选择保留对象，并发送 22/39 号变化（`:257-356`）。

建议拆分为：

- `WorldItemReservationQuery`：候选玩家、磁铁范围、Hopper 和保留权限的纯计算；
- `WorldItemReservationCommitSystem`：写入保留玩家与 own/keep 计时并广播；
- `WorldItemPhysicsSystem`：重力、湿润、传送带、TileCollision 和斜坡碰撞；
- `WorldItemMergeSystem`：同内容/前缀/微光状态邻近堆叠和位置速度加权；
- `WorldItemPickupSystem`：玩家、特殊 NPC 和金钱敌怪的拾取写集；
- `WorldItemExpirySystem`：越界、昼夜、事件结束和时间阈值销毁；
- `WorldItemVisualProjection`：光照、尘埃、声音和客户端表现。

`CheckLavaDeath` 和 `Shimmering` 当前为空体（`WorldItem.cs:624-625`），虽然调用顺序已
存在，但熔岩销毁、微光转换、旧实体清理/新实体创建和网络广播未闭合。`TryGrantingMakeAWishSet`
也为空体，属于特殊掉落消费缺口。因而 WorldItem 的物理主体可识别，但微光/熔岩和特殊事件
行为只能标记 `partial/missing`。

### 56.3 网络同步、区段激活与客户端权威边界

消息 21/145/148 接收位置、速度、堆叠、前缀、保留和微光字段；服务器对槽位 400 的特殊
情况会重新调用 `Item.NewItem`，普通槽位则保留原有保留计时后重置内容，再将包转发
（`MessageBuffer.cs:1082-1160`）。`WorldItem` 静态构造订阅 `RemoteClient.NetSectionActivated`，
按 200×150 Tile 区段计算矩形并发送 160 号项目快照（`WorldItem.cs:165-170`, `:1557-1570`）。

建议拆分为：

- `WorldItemNetworkDecodeAdapter`：解码 21/145/148/151/160 字段并做范围校验；
- `WorldItemNetworkApplySystem`：服务端重算槽位、保留和内容，拒绝客户端越权写入；
- `WorldItemSectionProjection`：区段激活时发送当前活动掉落快照；
- `WorldItemNetworkConflictQuery`：槽位占用、复用延迟、保留玩家和重复包判断。

客户端发送的速度、保留玩家和微光字段只能作为意图/同步输入，不能直接成为服务器世界掉落
状态。特别是 `FindOwner` 会根据服务器玩家库存能力和 Hopper 规则重新计算保留对象，网络
投影应由该权威结果生成。

### 56.4 本轮新增边界与统计

| 边界 | 关键证据 | 状态 |
| --- | --- | --- |
| `WorldItemStateComponent` / `WorldItemContentComponent` | `WorldItem.cs:15-157` | `confirmed` |
| `WorldItemSlotAllocationCommand` / `WorldItemSpawnCommand` | `Item.cs:48688-48806` | `confirmed/partial` |
| `WorldItemReservationQuery` / `WorldItemReservationCommitSystem` | `WorldItem.cs:257-356` | `confirmed/partial` |
| `WorldItemPhysicsSystem` / `WorldItemMergeSystem` | `WorldItem.cs:357-558`, `:628-930` | `confirmed/partial` |
| `WorldItemPickupSystem` | `WorldItem.cs:945-1064` | `confirmed/partial` |
| `WorldItemExpirySystem` | `WorldItem.cs:560-624` | `partial` |
| `WorldItemShimmerConversionSystem` | `WorldItem.cs:500-505`, `:624-625` | `missing` |
| `WorldItemNetworkDecodeAdapter` / `WorldItemNetworkApplySystem` | `MessageBuffer.cs:1082-1160` | `partial` |
| `WorldItemSectionProjection` | `WorldItem.cs:165-170`, `:1557-1570` | `confirmed/partial` |

本轮新增约 **9 个**边界，累计独立边界约 **253 个左右**；至少约 **177 个**仍存在微光/熔岩
转换、槽位抢占、网络冲突、区段快照、特殊掉落和清空/释放语义缺口。静态识别覆盖率约
**97%～99%**，权威行为闭合度约 **27%～38%**。
**97%～99%**，权威行为闭合度约 **27%～38%**。

## 57. 第三十轮：PortalGun 门户配对、实体穿越与支撑 Tile 约束

上一版报告只把 PortalHelper 作为传送网络概览。本轮沿 Player、NPC、Projectile 和
RemoteClient 的实际调用继续展开，确认 PortalHelper 自身已经形成四个独立边界：门户索引
与冷却、穿越资格与出口几何、Projectile 命中后的门户放置、以及区段/玩家加入同步。

### 57.1 门户索引和冷却状态

静态数组 `FoundPortals[256,2]` 按玩家 owner 和 portal slot 保存两个门户的 Projectile
索引；`PortalCooldownForPlayers[256]`、`PortalCooldownForNPCs[Main.maxNPCs]` 分别保存
玩家/NPC 冷却，`anyPortalAtAll` 是快速短路标记（`Terraria.GameContent/PortalHelper.cs:8-24`）。
`UpdatePortalPoints` 每帧清空门户索引、递减两套冷却，再扫描 1000 个 Projectile，只有
type 602、`ai[1]` 为 0/1 且 owner 合法时才登记一对门户（`:64-101`）。NPC 槽位复用时
`ResetNPCSlotData` 清除对应冷却（`:104-107`）。

建议拆分为：

- `PortalPairIndexComponent`：owner → 两个门户 Projectile 引用；
- `PortalCooldownComponent`：Player/NPC 分离的冷却状态；
- `PortalIndexRefreshSystem`：Projectile 集合到门户索引的帧快照；
- `PortalPairQualificationQuery`：门户是否成对、owner/slot/type 是否合法；
- `PortalSlotReuseCommand`：NPC 槽位回收时清理冷却和旧引用。

`FoundPortals` 和两套冷却都是进程级静态数组，Reset、世界切换和并行场景的所有权没有
显式建模。更重要的是，NPC 穿越分支在 `TryGoingThroughPortals` 中写入
`PortalCooldownForPlayers[i] = 10`（`:201-206`），而不是 NPC 冷却数组；如果该代码不是
反编译误差，NPC 与玩家共享了错误的冷却通道，应作为待运行时确认的状态一致性风险。

### 57.2 穿越资格、出口几何与速度转换

`TryGoingThroughPortals` 对每个成对门户检查实体的 AABB/速度路径是否与门户边线相交，按
出口门户角度计算落点和 `bonusX/bonusY`，再用四个方向的 `Collision.TileCollision` 确认
出口周围有空间；之后最小化速度、沿出口法线重定向，并分别调用 Player/NPC 的 Teleport。
玩家写入 `lastPortalColorIndex` 和玩家冷却，NPC 还广播 100/23 号网络消息
（`PortalHelper.cs:109-212`）。

建议拆分为：

- `PortalIntersectionQuery`：实体 AABB、速度和门户线段相交；
- `PortalExitGeometryQuery`：出口中心、法线、尺寸偏移和 bonus 方向；
- `PortalExitClearanceQuery`：四向 TileCollision 空间验证；
- `PortalVelocityTransformSystem`：最小速度、法线和重力方向修正；
- `PortalTraversalCommand`：Player/NPC 传送、冷却、颜色状态和网络投影；
- `PortalTraversalVisualProjection`：颜色、尘埃、声音等表现效果。

`GetPortalEdges`、`GetPortalOutingPoint` 当前均返回默认向量，导致线段相交和出口几何链
无法从源码闭合（`PortalHelper.cs:282-291`）。因此即使外层四向碰撞检查存在，实际出口
位置和速度方向仍必须标记 `missing`。`TryGoingThroughPortals` 还在同一个循环中直接
修改 Entity 位置/速度并写全局冷却，不能当作纯 Query。

### 57.3 Projectile 命中、门户放置和支撑 Tile

PortalGun Projectile 在命中时调用 `TryPlacingPortal`（`Terraria/Projectile.cs:16053`）。
该方法沿 Projectile 速度寻找碰撞 Tile，把普通、半砖和斜坡边缘转换为候选门户方向，调用
`FindValidLine` 找到连续支撑线，最后通过 `AddPortal` 建立/替换门户（
`PortalHelper.cs:216-260`）。Projectile 更新还周期调用 `SupportedTilesAreFine`，若原支撑
Tile 被破坏则清除门户（`Projectile.cs:30359-30381`）。

建议拆分为：

- `PortalImpactTileQuery`：Projectile 轨迹到 Tile 的首次碰撞；
- `PortalSurfaceOrientationQuery`：普通/半砖/斜坡表面方向；
- `PortalSupportLineQuery`：沿表面方向搜索连续可放置线；
- `PortalPlacementCommand`：分配门户 Projectile、owner/slot/角度并更新网络区段；
- `PortalSupportValidationSystem`：每帧检查门户支撑 Tile，失效时撤销；
- `PortalDestructionCompensationCommand`：门户撤销后的 Projectile、区段和客户端状态清理。

`FindValidLine`、`FindCollision`、`AddPortal` 为空体/默认返回（`:247-260`），
`SupportedSlope`、`SupportedHalfbrick`、`SupportedNormal` 也为空体（`:379-387`）。
这使 Portal 放置、替换、支撑校验和撤销均为 `partial/missing`；不能仅凭 Projectile 类型
和 `ai` 字段推断门户系统已完成。

### 57.4 玩家加入与区段同步

`SyncPortalsOnPlayerJoin` 扫描所有活动的 PortalGun/相关 Projectile（type 602/601），将
门户周围的区段加入待发送列表；`SyncPortalSections` 对所有活动玩家调用
`RemoteClient.CheckSection`（`PortalHelper.cs:293-333`）。`MessageBuffer` 在连接流程中
调用前者并把区段列表纳入加入快照（`MessageBuffer.cs:524-529`）；Portal Projectile 生成
时调用后者（`Projectile.cs:30359`）。

建议拆分为：

- `PortalJoinSnapshotProjection`：玩家加入时的门户和区段列表；
- `PortalSectionActivationCommand`：门户附近区段激活请求；
- `PortalProjectileReplicationAdapter`：门户 Projectile 的全量/增量字段；
- `PortalClientVisibilityQuery`：目标客户端是否已加载门户区段。

门户状态依附 Projectile，但 PortalHelper 又维护独立索引和冷却；Projectile 被替换、跨区段
或 owner 离线时，索引、区段和冷却必须按明确顺序失效。当前没有找到统一的撤销事件；这与
前述 `AddPortal` 空体共同构成网络同步和资源回收缺口。

### 57.5 本轮新增边界与统计

| 边界 | 关键证据 | 状态 |
| --- | --- | --- |
| `PortalPairIndexComponent` / `PortalCooldownComponent` | `PortalHelper.cs:8-107` | `confirmed/partial` |
| `PortalIndexRefreshSystem` / `PortalPairQualificationQuery` | `PortalHelper.cs:64-101` | `confirmed/partial` |
| `PortalIntersectionQuery` / `PortalExitGeometryQuery` | `PortalHelper.cs:109-150`, `:282-291` | `partial/missing` |
| `PortalExitClearanceQuery` / `PortalVelocityTransformSystem` | `PortalHelper.cs:151-212` | `confirmed/partial` |
| `PortalTraversalCommand` | `PortalHelper.cs:190-212`, `Player.cs`/`NPC.cs` Teleport | `partial` |
| `PortalImpactTileQuery` / `PortalSurfaceOrientationQuery` | `PortalHelper.cs:216-246` | `confirmed/partial` |
| `PortalSupportLineQuery` / `PortalPlacementCommand` | `PortalHelper.cs:247-260` | `missing` |
| `PortalSupportValidationSystem` | `PortalHelper.cs:336-387`, `Projectile.cs:30359-30381` | `missing` |
| `PortalJoinSnapshotProjection` / `PortalSectionActivationCommand` | `PortalHelper.cs:293-333`, `MessageBuffer.cs:524-529` | `confirmed/partial` |

本轮新增约 **9 个**边界，累计独立边界约 **262 个左右**；至少约 **186 个**仍存在门户
几何、支撑线、Projectile 替换、NPC 冷却、区段失效和网络加入快照缺口。静态识别覆盖率约
**97%～99%**，权威行为闭合度约 **25%～36%**。

## 58. 第三十一轮：Old One's Army 入侵阶段、波次积分与竞技场边界

### 58.1 事件状态不是一个布尔开关

`Terraria.GameContent.Events/DD2Event.cs` 同时持有三类不同生命周期的状态：世界进度
`DownedInvasionT1/T2/T3`（`:45-49`，由 `Save/Load` 在 `:93-115` 持久化）、当前运行状态
`Ongoing/LostThisRun/WonThisRun/OngoingDifficulty`（`:51-66`）以及波次运行时计数
`NPC.waveNumber/NPC.waveKills/NPC.totalInvasionPoints`（`StartInvasion` 的 `:177-196`、
`CheckProgress` 的 `:236-337`）。三者不能合并成 `DD2EventState.IsActive`：前者跨世界保存，
中者在一次入侵结束时清理，后者又由 NPC 击杀即时累积并影响下一波资格。

建议拆分为：

| 边界 | 职责 | 状态 |
| --- | --- | --- |
| `DD2WorldProgressComponent` | 三档难度通关位、旧版本 `savedBartender` 迁移结果 | `confirmed/partial` |
| `DD2RunStateComponent` | 当前是否进行、胜/负终态、难度、起始/等待阶段 | `confirmed` |
| `DD2WaveLedgerComponent` | 波次、击杀积分、总入侵积分和当前波次门槛 | `confirmed/partial` |
| `DD2CrystalDropLedgerComponent` | 每波应掉落、已掉落及上一波游标 | `confirmed` |
| `DD2ArenaBoundaryComponent` | 竞技场包围盒、60 tick 重算冷却和建造禁区 | `confirmed/partial` |
| `DD2GoblinDeathProjectionBuffer` | 被击杀敌人的底部位置，供胜利/掉落演出消费 | `confirmed` |

`ResetProgressEntirely`（`:118-128`）只清理部分运行状态，未显式重置
`LostThisRun/WonThisRun/OngoingDifficulty` 和水晶掉落游标；这些字段是否由新的开始流程
覆盖，不能由“Reset”名称推断。实现时应区分 `ResetWorldProgress`、`AbortRun`、`FinishRun` 三个
命令，避免把世界通关位和一次运行的失败状态一起清空。

### 58.2 开始、等待、波次完成和停止是有顺序的命令链

`SummonCrystalDirect` 先验证 466 号基座 Tile、计算多 Tile 偏移，再调用 `StartInvasion`、
创建 548 号水晶 NPC、掉落起始水晶（`:386-405`）。`StartInvasion` 随后清除炮塔、重置 NPC
入侵积分、启动 `NPCDamageTracker`、广播开始消息、写入 `Main.ReportInvasionProgress`，并
把生成暂停设置为 300 tick、擦除旧实体（`:175-203`）。`UpdateTime` 在计时归零时广播波次、
可能召唤 Betsy、写入 UI/网络进度（`:138-170`）；`CheckProgress` 在击杀时推进积分，波次完成
后设置 1800 tick 暂停、按难度发放勋章（`:236-337`）。

这条链应显式分成：

- `DD2CrystalSummonQualificationQuery`：基座 Tile、已有水晶 NPC、请求玩家和竞技场位置；
- `DD2RunStartCommand`：清理旧实体、初始化波次账本、启动伤害归属会话；
- `DD2WaveGateTimerSystem`：递减等待计时并产生“波次开始/Betsy 召唤”事件；
- `DD2KillCreditCommand`：把 NPC 击杀映射为怪物积分，拒绝重复或非入侵敌人；
- `DD2WaveAdvanceSystem`：按门槛推进波次、设置下一阶段等待、发放难度相关勋章；
- `DD2RunFinishCommand`：胜利/失败终止、通关位写入、实体擦除和伤害追踪器关闭；
- `DD2InvasionProgressProjection`：`Main.ReportInvasionProgress` 和消息 78 的网络 DTO。

`WinInvasionInternal`、`FindProperDifficulty`、`GetInvasionStatus`、`GetEnemiesForWave`、
`GetMonsterPointsWorth`、`SetEnemySpawningOnHold` 当前均为空体或默认返回（`:225`、`:233`、
`:349-359`、`:667`）。因此波次门槛、敌人列表、难度选择、积分价值和胜利写入仍是
`partial/missing`；不能把 `CheckProgress` 中的计数加法当作完整入侵规则。

### 58.3 生成门、竞技场限制和实体清理是三个不同写集

NPC 的 Old One's Army AI 在 `NPC.cs:41654-41699` 检查 `EnemySpawningIsOnHold`、
`LaneSpawnRate` 和 `ai[0]`，再按左右门调用 `SpawnMonsterFromGate`；后者仅根据
`OngoingDifficulty` 分派三个难度函数（`DD2Event.cs:361-376`）。三个
`Difficulty_*_SpawnMonsterFromGate` 均为空体（`:668-670`），所以出生位置、敌人类型、
生成失败重试和门两侧公平性没有证据闭合。

`FindArenaHitbox` 每 60 tick 扫描 548/549 NPC 的包围盒并外扩 50 个 Tile（`:499-544`），
玩家放置路径在 `Player.cs:15585-15589` 读取它并通过 `ShouldBlockBuilding` 拒绝竞技场内建造。
这不是普通碰撞：应拆为 `DD2ArenaExtentQuery`、`DD2BuildPermissionQuery` 和
`DD2ArenaRefreshSystem`，并规定水晶死亡、实体撤销或客户端加入时的失效行为。

`WipeEntities`（`:430-440`）再分别清除 DD2 炮塔 Projectile、入侵 NPC 和打开箱子中的 3822
能量水晶（`:442-488`），其中 NPC 直接写 `active = false` 后广播 23，箱子只清理当前打开
的 Chest。这形成三个不同的所有权/同步事务：

- `DD2TurretDespawnCommand`：仅撤销 `ProjectileID.Sets.IsADD2Turret`；
- `DD2HostileDespawnCommand`：按 `BelongsToInvasionOldOnesArmy` 清除并发送 NPC 删除；
- `DD2EnergyCrystalReservationCommand`：锁定/清理 Chest 槽位并发送物品更新；
- `DD2RunCleanupSystem`：以明确顺序提交以上命令，再广播事件结束投影。

当前 `ClearAllDD2EnergyCrystalsInChests` 依赖 `Chest.GetCurrentlyOpenChests()`，因此离线玩家、
未打开箱子、并发打开/关闭和重复消息的行为未闭合；`DropStarterCrystals`、`SummonBetsy` 也
为空体，水晶经济和最终 Boss 生成必须继续标记 `partial`。

### 58.4 伤害归属、掉落和客户端进度投影

`DD2Event.DamageTracker` 继承 `NPCDamageTracker`，但 `IncludeDamageFor` 为空体（`:25-26`）；
`StartInvasion` 创建追踪器，`ReportLoss` 关闭失败会话（`:339-346`），`StopInvasion(win: true)`
则调用缺失的胜利内部处理（`:205-222`）。建议将其拆为
`DD2DamageAttributionPolicy`（只接收属于入侵的 NPC）、`DD2RunOutcomeCommand` 和
`DD2KillTimeProjection`，不要让通用 Boss 伤害追踪器直接拥有 DD2 通关位。

`AnnounceGoblinDeath` 只把 NPC 底部坐标放入 `_deadGoblinSpots`（`:492-496`），没有看到消费
调用；这属于 `partial` 的演出/掉落投影，而非已确认的权威奖励。`ShouldDropCrystals` 根据
波次、难度、专家倍率和当前击杀比例递增掉落游标（`:566-656`），但若
`requiredKillCount` 为默认 0，将出现除零风险，且多个调用位于 `NPC.cs:65723-65754`，需要
`DD2CrystalDropQualificationQuery` 在提交前保护门槛和重复 tick。

网络上，`NetMessage` 将 `Ongoing` 与三档通关位压入世界状态位（`NetMessage.cs:325-328`），
消息 78 传输波次进度，`Main.SendDataForEachPlayer` 在 `Main.cs:11831-11854` 维护客户端
进度缓存。建议把这些拆成 `DD2WorldFlagsProjection` 与 `DD2ProgressReplicationAdapter`；
不能把 `Main.invasionProgress*` 当作服务器账本。

### 58.5 本轮新增边界与统计

| 边界 | 关键证据 | 状态 |
| --- | --- | --- |
| `DD2WorldProgressComponent` / `DD2RunStateComponent` | `DD2Event.cs:45-66`, `:93-128`, `WorldFile.cs:1412`, `:2337` | `confirmed/partial` |
| `DD2WaveLedgerComponent` / `DD2WaveAdvanceSystem` | `DD2Event.cs:138-170`, `:236-337`, `NPC.cs:64759` | `partial` |
| `DD2CrystalSummonQualificationQuery` / `DD2RunStartCommand` | `DD2Event.cs:379-405`, `MessageBuffer.cs:2932-2936` | `confirmed/partial` |
| `DD2WaveGateTimerSystem` / `DD2InvasionProgressProjection` | `DD2Event.cs:138-170`, `Main.cs:11831-11854` | `partial` |
| `DD2SpawnGateCommand` / `DD2SpawnDifficultyPolicy` | `DD2Event.cs:361-376`, `NPC.cs:41654-41699` | `missing` |
| `DD2ArenaExtentQuery` / `DD2BuildPermissionQuery` | `DD2Event.cs:499-546`, `Player.cs:15585-15589` | `confirmed/partial` |
| `DD2TurretDespawnCommand` / `DD2HostileDespawnCommand` | `DD2Event.cs:442-466` | `confirmed/partial` |
| `DD2EnergyCrystalReservationCommand` | `DD2Event.cs:469-488`, `WorldItem.cs:607` | `partial` |
| `DD2DamageAttributionPolicy` / `DD2RunOutcomeCommand` | `DD2Event.cs:15-34`, `:205-222`, `:339-346` | `missing/partial` |
| `DD2CrystalDropQualificationQuery` / `DD2CrystalDropProjection` | `DD2Event.cs:566-656`, `NPC.cs:65723-65754` | `partial` |
| `DD2WorldFlagsProjection` / `DD2ProgressReplicationAdapter` | `NetMessage.cs:325-328`, `Main.cs:11831-11854` | `confirmed/partial` |

本轮新增约 **14 个**边界，累计独立边界约 **276 个左右**；至少约 **201 个**仍存在
波次门槛、难度生成、胜利写入、伤害归属、起始/终止清理、能量水晶和网络授权缺口。
静态识别覆盖率仍约 **97%～99%**，但加入 DD2 的多个决定性空体后，权威行为闭合度应下调
为约 **24%～35%**。这不是运行实现完成结论。

## 59. 第三十二轮：压力板玩家进出检测与机关脉冲边界补证

`PressurePlateHelper` 的状态不只是 Tile 的 `active/type`：它维护
`PressurePlatesPressed : Dictionary<Point,bool[]>`、每玩家上一帧位置和
`NeedsFirstUpdate`（`Terraria.GameContent/PressurePlateHelper.cs:9-17`）。玩家更新路径在
`Player.cs:17657`、`:21764-21782` 调用 `UpdatePlayerPosition`；服务器主循环在
`Main.cs:11459` 调用 `Update`；断开/死亡在 `Player.cs:295`、`:302` 调用 `ResetPlayer`；
世界加载由 `WorldFile.cs:3450-3472` 恢复坐标并设置首帧重新触发。Tile 被破坏时
`WorldGen.cs:54348` 调用 `DestroyPlate`。

应拆分为：

- `PressurePlateOccupancyComponent`：按坐标保存玩家占用位图；
- `PlayerTileOverlapQuery`：比较上一帧/当前帧收集的 Tile 集合和收缩后的 hitbox；
- `PressurePlateEnterCommand` / `PressurePlateLeaveCommand`：仅提交边沿变化；
- `PressurePlatePulseSystem`：在 `NeedsFirstUpdate` 首帧和破坏 Tile 时发出一次 Poke；
- `PressurePlateWiringAdapter`：把 Poke 转成 `Wiring.HitSwitch/TripWire`，隔离机关副作用；
- `PressurePlatePersistenceProjection`：只存被按下坐标和玩家位图，不把上一位置当世界状态。

`MoveInto`、`MoveAwayFrom`、`PokeLocation` 全为空体（`PressurePlateHelper.cs:113-115`），
因此占用位图如何增删、同一坐标多玩家的边沿语义、Wiring 去重和脉冲目标均为 `missing`。
`ResetPlayer` 只遍历当前字典键并调用空的离开操作，玩家离线后的机关释放无法证实；
`WorldFile` 直接在共享静态字典上加锁，保存期间与移动更新的并发顺序也未定义。

Minecart 通过 `Wiring.HitSwitch`（`Minecart.cs:1283`）独立触发开关，不能复用玩家占用位图；
压力板、绊线、开关和逻辑门应以 `MechanismPulse` 命令汇合，而不是让压力板直接修改 Tile。
本轮补证 **6 个**边界（累计约 **282 个**），其中压力板核心写入和断开清理仍为
`missing/partial`；总体权威行为闭合度保持约 **24%～35%**。

## 60. 第三十三轮：地牢布局提供器、房间/大厅生成阶段与全局特性写集

### 60.1 `DungeonCrawler.MakeDungeon` 实际是一个多阶段生成管线

`Terraria.GameContent.Generation.Dungeon/DungeonCrawler.cs:329-500` 把一次地牢生成拆为
多个有先后依赖的阶段，而不是单次 `WorldGen` Tile 绘制：

1. 设置 `GeneratingDungeon`、裂砖可破坏性、入口强度和初始边界；
2. 按 `DungeonType` 选择 `DualDungeonLayoutProvider` 或 `LegacyDungeonLayoutProvider`
   （`:355-369`）；
3. 从已生成房间计算地牢顶部，生成通往地表的入口大厅（`:371-446`）；
4. 生成入口后，先逐房间、再逐大厅计算门和平台（`:452-466`）；
5. 按固定阶段依次生成早期双地牢特性、尖刺、门、墙变体、平台、生物群系箱、书架、普通箱、
   灯光、陷阱、地面家具、画作、旗帜和晚期双地牢特性（`:469-498`）；
6. 最后将 `GeneratingDungeon` 置为 false（`:498-500`）。

应拆为 `DungeonGenerationContextComponent`、`DungeonLayoutCommand`、
`DungeonEntranceHallCommand`、`DungeonConnectionPlanningSystem`、
`DungeonGlobalFeaturePipeline` 和 `DungeonGenerationCompletionCommand`。阶段顺序必须保留：
门/平台计算依赖房间和大厅已经生成，箱子/家具又依赖墙体和可放置面，晚期双地牢特性不能
提前写入。

### 60.2 布局数据、内容样式和保护边界需要分层

`DungeonData.cs:12-86` 将房间、大厅、特性、门、平台、保护边界、墙变体、家具样式、各种
标量和最后大厅位置放在同一对象中；`DungeonGenVars` 则提供砖块/墙体类型、潜在范围、样式
集合和特殊种子派生表。`SetupDungeonGenVarVariables`（`DungeonCrawler.cs:56-198`）还会
根据随机颜色、Drunk/Remix、双地牢和 `surfaceIsInSpace` 选择墙体集合与入口类型。

建议拆分：

- `DungeonStyleDefinition`：砖块、墙、玻璃、门、平台、书架、灯笼和旗帜内容定义；
- `DungeonLayoutState`：房间/大厅节点、连接线、迭代编号和当前生成位置；
- `DungeonProtectionBoundsComponent`：内/外潜在范围、已保护范围和裂砖保护层；
- `DungeonGenerationSeedPolicy`：颜色、特殊种子、双地牢样式列表和可复现随机种子；
- `DungeonPlacementLedger`：门、平台、家具和需要后续特性消费的候选位置。

`DungeonUtils.IsConsideredDungeonTile/Wall`（`DungeonUtils.cs:74-121`）可以按当前地牢或
所有地牢迭代查询 Tile/Wall；`InAnyPotentialDungeonBounds` 与
`IntersectsAnyPotentialDungeonBounds`（`:145-195`）返回迭代编号，说明保护范围是跨地牢
查询边界，而不是某个房间的 UI 信息。实现时应禁止把 `CurrentDungeon` 全局索引直接复制
到运行时玩家权限；生成期范围和运行期不可破坏墙扫描是两个投影。

### 60.3 房间/大厅/入口是不同的几何提交器

抽象 `DungeonRoom`（`Rooms/DungeonRoom.cs:9-80`）把 `CalculateRoom`、`GenerateRoom`、
`CalculatePlatformsAndDoors`、房间内特性、箱子、保护类型、包含判断和洪水操作混在一个
继承接口中；`DungeonHall`（`Halls/DungeonHall.cs:8-59`）则独立拥有起止点、方向、裂砖
状态、门/平台计算和 Tile 放置/移除资格；`DungeonEntrance`（`Entrances/DungeonEntrance.cs:6-31`）
再定义入口几何、Old Man 生成点和入口特性资格。

推荐拆分为：

| 边界 | 纯职责 | 当前证据 |
| --- | --- | --- |
| `DungeonRoomGeometryQuery` | 房间内/外边界、中心、保护类型和连接点 | `DungeonRoom.cs:46-74`；`partial` |
| `DungeonRoomTileCommand` | 由具体房间形状向 TileMap 写入实体墙/空腔 | 各 `*DungeonRoom.cs`；`partial` |
| `DungeonHallGeometryQuery` | 起止点、方向、走廊和楼梯线 | `DungeonHall.cs:17-45`、`DualDungeonLayoutProvider.cs:17-68`；`partial` |
| `DungeonHallTileCommand` | 走廊墙体、裂砖和可移除 Tile 写入 | `DungeonHall.cs:49-59`；`partial` |
| `DungeonEntranceGeometryQuery` | Dome/Tower/Legacy 入口边界和 Old Man 点 | `DungeonEntrance.cs:14-29`；`partial` |
| `DungeonEntrancePlacementCommand` | 入口地形、墙体、入口大厅和生成点 | `DungeonCrawler.cs:394-446`；`missing` |
| `DungeonFloodCommand` | 特定房间液体类型及洪水数量 | `DungeonRoom.cs:75-79`；`missing` |

`LegacyDungeonLayoutProvider.ProvideLayout` 和 `DualDungeonLayoutProvider.ProvideLayout`
当前为空体（各自 `:17-18`、`:77-78`）；多种房间实现的 `CalculateRoom`、`GenerateRoom`、
`FloodRoom`、`IsInsideRoom`、`GetProtectionTypeFromPoint` 也为空体或默认返回。Dome、Tower
和 Legacy 入口的 `CalculateEntrance/GenerateEntrance` 同样未展开。故虽然 `MakeDungeon`
的调用顺序可确认，实际房间图、走廊图、Tile 几何和保护写集仍不能视为已闭合。

### 60.4 全局地牢特性是阶段化写集，而不是一个大 Feature

`DungeonCrawler.MakeDungeon` 连续构造 14 个 `DungeonGlobal*` 对象；每个构造函数都把自己
加入 `DungeonData.dungeonFeatures`，但 `GenerateFeature` 当前均为空体（例如
`Features/DungeonGlobalDoors.cs:8-18`、`DungeonGlobalBasicChests.cs:8-15`、
`DungeonGlobalTraps.cs:8-15`）。这些特性具有不同冲突集合和前置条件，建议保持独立边界：

- `DungeonDoorPlacementSystem`：消费门候选，依赖房间/大厅平台计算；
- `DungeonWallVariantSystem`：墙样式和裂砖变体，必须尊重保护范围；
- `DungeonPlatformPlacementSystem`：平台候选与门冲突消解；
- `DungeonChestPlacementSystem` / `DungeonBiomeChestPlacementSystem`：箱体占用、战利品和
  生物群系宝箱资格；
- `DungeonFurniturePlacementSystem`：书架、灯、地面家具和家具样式；
- `DungeonTrapPlacementSystem` / `DungeonSpikePlacementSystem`：陷阱、尖刺和压力板/电线
  后续交接；
- `DungeonPaintingBannerProjection`：画作和旗帜内容写入及地图/物品投影；
- `DualDungeonEarlyLateFeatureSystem`：双地牢专用前后阶段，不能与普通特性无序合并。

每个特性都应返回“未生成/部分生成/已生成”的结果并记录写集；当前的 `bool` 返回值和
`generated` 字段没有看到失败回滚、占用图提交或重复运行保护，因此重复执行 `MakeDungeon`
可能产生不可验证的家具/箱体冲突。`DungeonFeature.CanGenerateFeatureAt` 的默认返回也
不能代替房间/大厅/入口三套资格查询。

### 60.5 本轮新增边界与统计

| 边界 | 关键证据 | 状态 |
| --- | --- | --- |
| `DungeonGenerationContextComponent` / `DungeonGenerationSeedPolicy` | `DungeonCrawler.cs:56-198`, `DungeonData.cs:12-86` | `confirmed/partial` |
| `DungeonLayoutCommand` / `DungeonLayoutProviderAdapter` | `DungeonCrawler.cs:355-369`, `LegacyDungeonLayoutProvider.cs:10-18`, `DualDungeonLayoutProvider.cs:72-78` | `partial/missing` |
| `DungeonEntranceHallCommand` / `DungeonEntrancePlacementCommand` | `DungeonCrawler.cs:394-446`, `Entrances/DungeonEntrance.cs:6-31` | `partial/missing` |
| `DungeonRoomGeometryQuery` / `DungeonRoomTileCommand` | `Rooms/DungeonRoom.cs:9-80`, 各具体房间实现 | `partial/missing` |
| `DungeonHallGeometryQuery` / `DungeonHallTileCommand` | `Halls/DungeonHall.cs:8-59`, `DualDungeonLayoutProvider.cs:17-68` | `partial` |
| `DungeonProtectionBoundsComponent` / `DungeonProtectionQuery` | `DungeonUtils.cs:74-195`, `DungeonData.cs:38-49` | `confirmed/partial` |
| `DungeonConnectionPlanningSystem` | `DungeonCrawler.cs:452-466`, `DualDungeonLayoutProvider.HallwayCalculator` | `partial` |
| `DungeonGlobalFeaturePipeline` | `DungeonCrawler.cs:469-498` | `confirmed/partial` |
| `DungeonDoor/Wall/PlatformPlacementSystem` | `DungeonCrawler.cs:473-477`, `DungeonGlobal*.cs` | `missing` |
| `DungeonChest/Furniture/TrapPlacementSystem` | `DungeonCrawler.cs:479-492`, `DungeonGlobal*.cs` | `missing` |
| `DungeonPaintingBannerProjection` / `DualDungeonEarlyLateFeatureSystem` | `DungeonCrawler.cs:469`, `:494-498` | `missing` |

本轮新增约 **15 个**地牢生成边界，累计独立边界约 **297 个**；至少约 **216 个**仍有
房间/大厅/入口几何、布局提供器、特性写入、失败回滚和保护查询缺口。静态识别覆盖率维持
约 **97%～99%**，权威行为闭合度进一步调整为约 **23%～34%**。上述结论只描述
`Version4` 当前代码证据，不表示已恢复可运行的完整地牢生成器。

## 61. 第三十四轮：SmartInteract 候选优先级、悬停交互与实际调用缺口

### 61.1 目录中存在候选编排，但没有已证实的执行入口

`Terraria.GameContent.ObjectInteractions/SmartInteractSystem.cs:5-21` 的构造函数固定注册四
类候选提供器，顺序为 Potion of Return、Projectile、NPC、Tile，并额外注册
`BlockBecauseYouAreOverAnImportantTile`。`SmartInteractScanSettings.cs:5-22` 将玩家、鼠标世界
坐标、扫描矩形和“只允许零距离/完整交互”作为一次扫描输入。

候选接口将“发现候选”和“获选后的副作用”分开：`ISmartInteractCandidateProvider` 要求先
`ClearSelfAndPrepareForCheck`，再通过 `ProvideCandidate` 返回候选；`ISmartInteractCandidate`
只暴露 `DistanceFromCursor` 和 `WinCandidacy`（`ISmartInteractCandidate.cs:3-9`）。建议拆成：

- `SmartInteractScanContextComponent`：一次扫描的玩家/鼠标/范围快照；
- `SmartInteractCandidateQuery`：按优先级筛选 NPC、Projectile、Tile 和 Potion Gate；
- `SmartInteractBlockQualificationQuery`：重要 Tile、距离和完整交互条件；
- `SmartInteractWinnerCommand`：只对最终候选调用 `WinCandidacy`；
- `SmartInteractHoverProjection`：悬停状态、光标提示和视觉效果；
- `SmartInteractActionAdapter`：把候选获选转换为 NPC 对话、Projectile 操作或 Tile/传送命令。

### 61.2 各候选提供器目前均未展开

`TileSmartInteractCandidateProvider.cs:6-29` 保存可复用候选和 `targets` 坐标列表，但
`ClearSelfAndPrepareForCheck`、`ProvideCandidate`、`ReusableCandidate.WinCandidacy` 均为空体；
`TileID.Sets.DisableSmartInteract`（`Terraria.ID/TileID.cs:209`）只给出禁用集合，不能证明
距离、TileObject 原点、Anchor 或服务器权限检查存在。

`NPCSmartInteractCandidateProvider.cs:6-24`、`ProjectileSmartInteractCandidateProvider.cs:6-26`
同样只有候选容器，没有 NPC/Projectile 过滤和获选提交。它们不能直接访问客户端可见实体并
改变服务器状态；应由 `NPCInteractionQualificationQuery`、`ProjectileInteractionQualificationQuery`
先返回纯资格，再由服务器命令写入实体或交互 Anchor。

`PotionOfReturnSmartInteractCandidateProvider.cs:5-25` 和
`PotionOfReturnGateInteractionChecker.cs:5-18` 还引入了独立的 Gate 交互边界：悬停状态、
阻塞条件和执行动作被抽象到 `AHoverInteractionChecker`。但四个重写方法全部为空体，且
`Player.cs:15163` 的 SmartInteract 调用仍是注释，当前未找到 `SmartInteractSystem` 的实际
扫描/获选消费者。这使整个目录更接近未接入的输入适配层，而不是已经运行的权威子系统。

### 61.3 客户端候选与服务器命令必须分离

建议的闭合链是：

```text
输入帧/鼠标位置
  -> SmartInteractScanContext
  -> Block 资格查询
  -> 候选提供器（NPC/Projectile/Tile/Potion Gate）
  -> 距离和优先级排序
  -> 客户端悬停投影
  -> 用户确认/交互包
  -> 服务器重新验证目标、距离、Tile/实体权限
  -> NPC/Projectile/Tile/传送 Command
```

客户端 `ReusableCandidate` 的距离值不能成为服务器授权凭据；`WinCandidacy` 也不应直接
写世界。Potion of Return 的原始位置和回家位置虽然通过 `Player.cs:1899-1901` 保存并由
`NetMessage.cs:470-495` 同步、`MessageBuffer.cs:710-716` 接收，但那是传送过程状态，不能
反向证明悬停 Gate 已经完成资格检查或执行动作。

### 61.4 本轮新增边界与统计

| 边界 | 关键证据 | 状态 |
| --- | --- | --- |
| `SmartInteractScanContextComponent` | `SmartInteractScanSettings.cs:5-22` | `confirmed` |
| `SmartInteractCandidateQuery` / `SmartInteractBlockQualificationQuery` | `SmartInteractSystem.cs:5-21`, `ISmartInteractCandidateProvider.cs:3-9` | `partial` |
| `SmartInteractWinnerCommand` | `ISmartInteractCandidate.cs:3-9`, 各 `ReusableCandidate` | `missing` |
| `SmartInteractHoverProjection` / `SmartInteractActionAdapter` | `AHoverInteractionChecker.cs:7-18`, Potion Gate checker | `missing` |
| `TileInteractionQualificationQuery` | `TileSmartInteractCandidateProvider.cs:6-29`, `TileID.cs:209` | `missing` |
| `NPCInteractionQualificationQuery` | `NPCSmartInteractCandidateProvider.cs:6-24` | `missing` |
| `ProjectileInteractionQualificationQuery` | `ProjectileSmartInteractCandidateProvider.cs:6-26` | `missing` |
| `PotionReturnGateQualification` / `PotionReturnCommand` | `PotionOfReturn*`, `Player.cs:1899-1901`, `MessageBuffer.cs:710-716` | `partial/missing` |

本轮新增约 **8 个**边界，累计独立边界约 **305 个**；至少约 **224 个**仍有候选扫描、
获选副作用、服务器重新验证和 Potion Gate 执行缺口。由于当前尚未找到实际
`SmartInteractSystem` 调用者，静态识别覆盖率仍约 **97%～99%**，但该目录的运行时接入闭合度
只能评为 **missing/partial**，总体权威行为闭合度维持约 **23%～34%**。

## 62. 第三十五轮：NPC 掉落规则图、链式解析与实例化写集

### 62.1 掉落数据库与运行时 Resolver

ItemDropDatabase.cs:7-150 维护全局规则、按 NPC NetID 的规则列表以及“类型 ID 到多个
NetID”的展开表。Populate（:113-150）按全局、食物、城镇 NPC、DD2、Boss、困难模式地牢、
Mimic、月相和入侵等阶段注册规则，最后调用 TrimDuplicateRulesForNegativeIDs 去重。
RegisterToNPC（:57-77）同时注册原始 type 和对应的负 NetID 变体，因此规则键不是简单的
NPC 类型整数。

建议拆分为 ItemDropRuleCatalog、NpcRuleBindingSystem、DropAttemptContextComponent、
ItemDropResolutionSystem、DropSpawnCommand 和 DropRateProjection。数据库/规则图只读，
随机滚动和实例化掉落必须由运行时系统提交。

### 62.2 一次 NPC 死亡的权威路径

NPC.NPCLoot_DropItems（NPC.cs:65420-65431）以最近交互玩家、NPC 实例、专家/大师标志、
IsInSimulation=false 和 Main.rand 创建 DropAttemptInfo，再调用 Main.ItemDropSolver。
Main.cs:3356-3365 在初始化时先执行 ItemDropDatabase.Populate，再创建 ItemDropResolver，
并将同一数据库合并到 Bestiary 掉落信息。Resolver 路径为：

GetRulesForNPCID（全局 + NPC 规则）
  -> CanDrop
  -> TryDroppingItem
  -> ResolveRuleChains
  -> CommonCode.DropItemFromNPC / 每玩家掉落 / Boss Bag
  -> WorldItem 或客户端投影

ItemDropResolver.ResolveRule（ItemDropResolver.cs:28-64）在条件失败时仍解析
TryIfDoesntFillConditions 链，在规则成功/随机失败后分别解析后续链；Success、
FailedRandomRoll、DoesntFillConditions 和 DidNotRunCode 是领域状态，不能用一个布尔值替代。
LeadingConditionRule.TryDroppingItem（LeadingConditionRule.cs:31-33）返回默认结果；
DropBasedOnExpertMode.TryDroppingItem 的非嵌套重载返回 DidNotRunCode（:35-42）。

### 62.3 条件、随机、链式规则和掉落实例化

CommonDrop.TryDroppingItem（CommonDrop.cs:31-50）使用玩家 RollLuck 决定分母/分子，再使用
info.rng 生成数量并调用 CommonCode.DropItemFromNPC；ReportDroprates（:52-62）只计算概率
并递归报告链。运气修正、数量随机、世界物品生成和概率展示是四个不同边界。

Chains.cs:7-127 定义失败随机、成功和条件失败三种链；TryIfDoesntFillConditions.CanChainIntoRule
为空体（:72-81）。Conditions.cs 的大量 CanDrop、CanShowItemDropInUI 和描述方法为空体或
默认返回（例如 :7-42、:472-519、:852-906），包括月相、波次、Boss 进度、特殊种子、雕像
来源和玩家交互。条件缺失会同时破坏实际掉落和 Bestiary 概率报告。

CommonCode.DropItemLocalPerClientAndSetNPCMoneyTo0（CommonCode.cs:22-54）为已交互玩家发送
本地掉落，销毁临时 WorldItem 并把 NPC 金币清零；DropItemForEachInteractingPlayerOnThePlayer
（:56-83）则按玩家位置生成掉落并清零金币。建议分别拆为 LocalPerClientDropCommand、
PerPlayerDropCommand 与 NpcMoneySettlementCommand，并保证一次击杀事务内金币不会被重复清零。

### 62.4 难度、Boss Bag、重掷和特殊规则族

ItemDropRule 工厂（ItemDropRule.cs:3-140）将 Normal/Expert、Master、Boss Bag、重掷、只负
运气缩放、选项池和条件规则组装成树；具体注册函数还按 Boss、Mimic、Eclipse、DD2、Blood
Moon Fishing 等内容族建立规则。建议拆为 DifficultyDropPolicy、RerollDropPolicy、
PerPlayerDropPolicy、BossBagDropPolicy、SpecialEventDropPolicy 和 DropOptionSelectionQuery。

OneFromRulesRule、FromOptionsWithoutRepeatsDropRule、StatueMimicItemDropRule、
SlimeBodyItemDropRule 和 MechBossSpawnersDropRule 拥有不同随机/条件语义；多个
TryDroppingItem、CanDrop 或 ReportDroprates 为空体，不能用 CommonDrop 的分母逻辑替代。

### 62.5 本轮新增边界与统计

| 边界 | 关键证据 | 状态 |
| --- | --- | --- |
| ItemDropRuleCatalog / NpcRuleBindingSystem | ItemDropDatabase.cs:7-150 | confirmed/partial |
| DropAttemptContextComponent | NPC.cs:65420-65431、DropAttemptInfo.cs:3-17 | confirmed |
| ItemDropResolutionSystem / DropChainState | ItemDropResolver.cs:14-64、Chains.cs:7-127 | partial |
| DropConditionQuery / DropRateProjection | Conditions.cs:7-1054、CommonDrop.cs:52-62 | missing/partial |
| DropSpawnCommand | CommonCode.cs:7-83 | confirmed/partial |
| LocalPerClientDropCommand / PerPlayerDropCommand | CommonCode.cs:22-83 | partial |
| DifficultyDropPolicy / RerollDropPolicy | ItemDropRule.cs:22-140、DropBasedOnExpertMode.cs:7-66 | partial |
| BossBagDropPolicy / NpcMoneySettlementCommand | ItemDropRule.cs:17-42、CommonCode.cs:22-54 | partial |
| SpecialEventDropPolicy / DropOptionSelectionQuery | ItemDropDatabase.cs:150-...、特殊 DropRule 类 | partial/missing |

本轮新增约 8 个边界，累计独立边界约 325 个；至少约 247 个仍存在条件实现、链式状态、
运气/重掷、玩家分配、金币结算和特殊规则族缺口。静态识别覆盖率约 97%～99%，权威行为
闭合度下调为约 21%～32%。当前报告仍是基于 Version4 的证据快照，不能宣称 NPC 掉落规则
已经具备完整可运行的重演或回滚能力。

## 63. 第三十六轮：FishDropRules 规则目录与浮标结果消费缺口

### 63.1 规则目录的真实结构

FishingContext.cs:5-21 将一次钓鱼计算的随机源、FishingAttempt、玩家和已滚动的环境标志
放在同一上下文；这是一次尝试的计算快照，不是玩家持久组件。FishDropRule.cs:3-17
保存候选物品数组、分子/分母、AFishingCondition[] 和 FishRarityCondition；FishDropRuleList.cs:6-29
只在加入时验证分母为正数，没有暴露规则枚举、执行或移除接口。

GameContentFishDropPopulator.Populate（GameContentFishDropPopulator.cs:8-29）按顺序添加敌人
阻断、熔岩、蜂蜜、垃圾、宝匣、稀有物、Remix、地牢、腐化/猩红、神圣、蘑菇、雪地、丛林、
海洋、沙漠、浮空岛和地表规则。AFishDropRulePopulator 的 Add/AddWithHardmode 会把环境、
Hardmode/EarlyMode、任务鱼和稀有度组合成规则节点；IsHardmode、IsOriginalOcean 和委托
条件的 Matches 当前为空体或默认返回（AFishDropRulePopulator.cs:5-17、:227-232）。

建议拆分为 FishingRuleCatalog、FishingConditionQuery、FishingRarityQuery、
FishingRuleSelectionSystem、FishingAttemptResultComponent 和 FishingResultDeliveryCommand。

### 63.2 规则已初始化，但当前未找到生产消费方

Main.cs:3361-3364 创建 FishDropRuleList，执行 GameContentFishDropPopulator.Populate，保存到
Main.FishDropsDB；但对 Version4 全量搜索只找到该字段的声明与初始化，没有找到任何
FishDropsDB 读取或 FishDropRuleList 遍历调用。浮标分派在 Projectile.cs:25365 调用
AI_061_FishingBobber；咬钩结果在 Projectile.cs:34073-34074 的两个方法均为空体。死亡
更新虽在 Projectile.cs:47784 尝试交付 ai[1] 物品，但没有证据说明该字段由 FishDropRules
规则计算写入。

因此当前只能确认“规则内容目录已注册”，不能确认浮标咬钩到 FishingAttempt 结果和玩家
交付/敌怪生成的完整链路。规则列表没有执行器，FishDropsDB 也没有生产消费调用者；迁移
设计必须先补 FishingRuleSelectionSystem 的调用入口，再谈结果投影。

### 63.3 任务鱼、宝匣和停止规则的独立边界

AddQuestFish/AddQuestFishForRemix 将 QuestFishCondition 或 QuestFishConditionRemix 追加到
条件数组；两类 Matches 均为空体（FishingConditions.cs:3-18）。AddStopper 用空候选项数组
建立阻断后续规则的哨兵，先后出现在敌怪、海洋、宝匣、垃圾、蜂蜜和熔岩规则中；这不是普通
掉落的 DropNothing，应建模为 FishingStopRuleState。

FishRarityCondition 暴露 FrequencyOfAppearanceForVisuals 和 HackedIsAny
（FishRarityCondition.cs:3-9），视觉频率不能回写权威随机结果。FishingAttempt 的鱼力、
水量、Chum、稀有度、已滚动物品和敌怪生成字段应保留为结果快照，由交付命令一次性消费，
避免同一浮标 tick 重复发放。

### 63.4 本轮新增边界与统计

| 边界 | 关键证据 | 状态 |
| --- | --- | --- |
| FishingRuleCatalog / FishingRuleSelectionSystem | GameContentFishDropPopulator.cs:8-29, FishDropRuleList.cs:6-29 | confirmed/missing |
| FishingContextComponent / FishingConditionQuery | FishingContext.cs:5-21, AFishingCondition.cs:3-7 | confirmed/partial |
| FishingRarityQuery | FishRarityCondition.cs:3-9, AFishDropRulePopulator.cs:30-74 | partial |
| FishingStopRuleState | AFishDropRulePopulator.cs:194-202, 各 AddStopper 调用 | partial/missing |
| FishingResultDeliveryCommand | Projectile.cs:25365, :34073-34074, :47784 | missing |
| QuestFishQualificationQuery | FishingConditions.cs:3-18 | missing |
| FishDropReplicationProjection | Main.FishDropsDB 声明/初始化，无读取消费者 | missing |

本轮新增约 7 个边界，累计独立边界约 332 个；至少约 256 个仍有规则选择器、条件执行、
停止哨兵、任务鱼资格、浮标结果写入和交付调用链缺口。静态识别覆盖率约 97%～99%，权威
行为闭合度下调为约 20%～31%。本节结论只基于 Version4 当前代码搜索和空体证据，不代表
FishDropRules 已经接入可运行钓鱼模拟。

## 64. 第三十七轮：Creative Power 注册、权限、玩家覆盖与世界写入边界

### 64.1 PowerManager 同时承担注册表、权限和存档协议

`Terraria.GameContent.Creative/CreativePowerManager.cs:8-190` 维护按 `ushort` ID 和配置名
索引的 Power 注册表。`Initialize` 注册 15 个能力（`:89-113`），`Register` 分配递增 ID、
设置默认/当前权限并写入两个字典（`:35-50`）；`TryListingPermissionsFrom` 从
`journeypermission_*` 配置行解析 0–2 权限级别并覆盖默认值（`:66-87`）。这至少包含三种
不同责任：内容注册、服务器配置解析和当前会话权限，不应合成一个可变单例组件。

建议拆分为：

- `CreativePowerDefinitionCatalog`：能力 ID、名称、类型和默认权限的只读目录；
- `CreativePermissionStateComponent`：每个能力的默认/当前权限级别；
- `CreativePowerRegistryAdapter`：泛型类型与 ID/名称索引之间的适配；
- `CreativePermissionConfigCommand`：解析服务器配置并提交权限变更；
- `CreativePowerLifecycleSystem`：初始化、世界重置、玩家加入同步；
- `CreativeWorldPowerPersistenceProjection`：按 ID 写入/读取/校验每个世界能力状态。

`LoadFromWorld` 和 `ValidateWorld` 遇到未知 ID 时直接 `break`（`:148-161`、`:164-177`），
没有跳过未知能力的 payload；旧版本或扩展能力顺序变化可能导致后续世界区段错位。报告应
将此标为 `partial`，实现时需携带长度或版本化 DTO，不能只依赖 `bool + ushort ID`。

### 64.2 四类基类表达了不同的权威写集

`CreativePowers.cs` 将能力分为四种形态：

- `APerPlayerTogglePower`（`:19-86`）：每玩家布尔位图，加入玩家时按字节发送 255 个槽位；
- `APerPlayerSliderPower`（`:89-191`）：每玩家浮点缓存，接收网络值后重算本地派生值；
- `ASharedButtonPower`（`:194-220`）：共享世界按钮，调用 `UsePower` 产生一次性变化；
- `ASharedTogglePower` / `ASharedSliderPower`（`:223-307`）：共享世界开关/滑块，并可持久化。

这些形态不应都映射为 `Player.CreativePowerFlags`：共享能力写世界时钟、天气、生态或 NPC
强度；每玩家能力只影响特定玩家的碰撞/刷怪资格。建议把 UI 缓存、服务器权威值和派生值
分成 `CreativePowerInputProjection`、`CreativePowerAuthorityComponent` 与
`CreativePowerDerivedEffectSystem`。

### 64.3 已确认的消费者与未闭合能力

`Main.cs:3169-3170` 读取 `FreezeTime.Enabled` 和 `ModifyTimeRate.TargetTimeRate` 参与主循环；
`Main.cs:11369-11372` 读取 `DifficultySliderPower.StrengthMultiplierToGiveNPCs` 覆盖 NPC 难度；
`WorldGen.cs:59410` 读取 `StopBiomeSpreadPower.Enabled` 控制生态扩散，`:59855` 读取冻结时间；
`NPC.cs:266-267`、`:670`、`:5804` 读取每玩家刷怪率禁用状态；`Player.cs:19883`、`:20027`
读取远程放置范围；这些调用证明 Creative Power 已进入权威模拟，而不只是 UI。

但 `GodmodePower.GetIsUnlocked`、`FarPlacementRangePower.GetIsUnlocked`、多个共享按钮的
`UsePower`、风/雨滑块的 `UpdateInfoFromSliderValueCache`、`SpawnRateSliderPerPlayerPower`
的派生计算以及大量 `Save/Load/ApplyLoadedDataToOutOfPlayerFields` 为空体或默认返回
（`CreativePowers.cs:311-405`、`:526-737`）。因此“能力注册成功”不等于“能力行为已闭合”。

应拆出以下命令/查询：

- `CreativePowerAvailabilityQuery`：玩家是否解锁、服务器当前权限是否允许；
- `CreativePowerNetInputAdapter`：读取玩家槽位/滑块值并限制 userId，拒绝客户端替换目标玩家；
- `CreativePowerMutationCommand`：共享按钮或切换能力的服务器权威提交；
- `CreativeTimeMutationSystem`、`CreativeWeatherMutationSystem`、`CreativeEcologyMutationSystem`：
  分别写时钟、风雨和生态扩散规则；
- `CreativePlayerModifierProjection`：Godmode、放置距离和刷怪率向 Player/NPC 查询的派生投影。

### 64.4 网络包和玩家/世界持久化分界

`NetCreativePowersModule.Deserialize`（`Terraria.GameContent.NetModules/NetCreativePowersModule.cs:20-31`）
读取 Power ID 后直接调用 `power.DeserializeNetMessage(reader, userId)`；APerPlayerSlider 会
读取一个客户端提供的槽位字节和浮点值，再调用 `CreativePowersHelper.IsAvailableForPlayer`
（`CreativePowers.cs:127-145`）。共享 Toggle/Slider 的 `DeserializeNetMessage` 为空体，
`NetCreativePowerPermissionsModule.Deserialize` 和 `NetCreativeUnlocksPlayerReportModule.Deserialize`
也为空体。网络层缺少明确的“服务器只接受本人槽位”“共享能力需要权限”“重复包幂等”证据。

`CreativePowerManager.SyncThingsToJoiningPlayer` 在 `MessageBuffer.cs:593` 连接阶段发送所有
权限，再调用每个 `IOnPlayerJoining`（`CreativePowerManager.cs:180-190`）。这应拆为
`CreativePermissionJoinProjection` 和 `CreativePowerJoinStateProjection`；两者不能共用一份
客户端缓存作为权威状态。

研究进度另属知识账本：`CreativeUnlocksTracker` 和 `ItemsSacrificedUnlocksTracker` 的
`Save/Load/ValidateWorld/Reset/OnPlayerJoining` 全部为空体，尽管
`CreativeItemSacrificesCatalog.Initialize`（`:16-75`）会读取嵌入的 `Sacrifices.tsv` 并建立
物品到所需牺牲数的定义表。建议拆为 `CreativeResearchDefinitionCatalog`、
`CreativeResearchLedgerComponent`、`CreativeSacrificeCommand` 和
`CreativeResearchReplicationProjection`，不要把静态目录误当成玩家/世界解锁进度。

### 64.5 本轮新增边界与统计

| 边界 | 关键证据 | 状态 |
| --- | --- | --- |
| `CreativePowerDefinitionCatalog` / `CreativePowerRegistryAdapter` | `CreativePowerManager.cs:35-113` | `confirmed/partial` |
| `CreativePermissionStateComponent` / `CreativePermissionConfigCommand` | `CreativePowerManager.cs:66-87`, `:116-130` | `confirmed/partial` |
| `CreativePowerLifecycleSystem` | `CreativePowerManager.cs:89-190`, `MessageBuffer.cs:593` | `confirmed/partial` |
| `CreativeWorldPowerPersistenceProjection` | `CreativePowerManager.cs:132-177`, `WorldFile.cs:3284`, `:3517-3525` | `partial` |
| `CreativePowerAuthorityComponent` / `CreativePowerDerivedEffectSystem` | `CreativePowers.cs:19-307`, `Main.cs:3169-3170`, `WorldGen.cs:59410` | `partial` |
| `CreativePowerAvailabilityQuery` / `CreativePowerNetInputAdapter` | `CreativePowers.cs:127-145`, `NetCreativePowersModule.cs:20-31` | `partial/missing` |
| `CreativeTime/Weather/EcologyMutationSystem` | `Main.cs:11369-11372`, `WorldGen.cs:59410`, `:59855` | `partial/missing` |
| `CreativePlayerModifierProjection` | `NPC.cs:266-267`, `:670`, `:5804`, `Player.cs:19883`, `:20027` | `partial` |
| `CreativeResearchDefinitionCatalog` / `CreativeResearchLedgerComponent` | `CreativeItemSacrificesCatalog.cs:16-75`, `CreativeUnlocksTracker.cs:5-13` | `confirmed/missing` |
| `CreativeSacrificeCommand` / `CreativeResearchReplicationProjection` | `ItemsSacrificedUnlocksTracker.cs:8-35`, `NetCreativeUnlocksPlayerReportModule.cs:7-10` | `missing` |

本轮新增约 **12 个**边界，累计独立边界约 **317 个**；至少约 **237 个**仍存在权限网络
反序列化、共享/个人能力写集、未知 ID 存档跳过、研究账本和玩家派生状态缺口。静态识别
覆盖率仍约 **97%～99%**，权威行为闭合度维持约 **22%～33%**。这些结论仅基于当前
`Version4` 代码证据，不能宣称 Creative Power 已具备完整可运行实现。

## 65. 第三十八轮：GameContent 网络模块注册、区段投影与反序列化授权

### 65.1 网络模块不是单一同步系统

NetworkInitializer.cs:14-28 注册所有 GameContent NetModule；这些模块的生命周期、所有权和写集差异很大，应按领域 Adapter/Projection 拆分，不能放进一个通用 NetworkSystem。

建议分组为：

 - WorldTileReplicationAdapter：液体和区段变化；
 - PlayerProgressReplicationAdapter：图鉴、创造解锁和权限；
 - EntityRegistryReplicationAdapter：传送水晶、LeashedEntity、TileEntity；
 - PresentationReplicationAdapter：天空实体、粒子和服务器文本；
 - DomainCommandAdapter：创造能力、制作请求、旗帜和标签效果；
 - PacketValidationSystem：长度、枚举、玩家身份、权限和重复包检查。

### 65.2 已有发送方与缺失接收方

NetLiquidModule.cs:8-105 按区段收集脏液体坐标，根据客户端 TileSections 过滤并序列化；Liquid.cs:1184 在液体更新后调用 CreateAndBroadcastByChunk。其 Deserialize（:57-60）为空体，客户端应用和非法坐标拒绝未闭合。

NetTextModule.cs、NetPingModule 和 NetAmbienceModule 的 Deserialize 为空体；AmbienceServer.cs:228 是天空实体发送方。NetParticlesModule.Deserialize（:20-30）读取粒子设置后广播，但发送者权限、设置长度和来源校验未展开。

### 65.3 领域状态网络包的授权缺口

NetBestiaryModule.cs:7-43 的击杀/目击/对话 DTO、NetCreativePowerPermissionsModule.Deserialize、NetCreativeUnlocksPlayerReportModule.Deserialize、NetTeleportPylonModule.cs:7-31、BannerSystem.NetBannersModule 和 CraftingRequests.NetCraftingRequestsModule.Deserialize 均存在空体或未验证接收入口。TagEffectState.NetModule 与 LeashedEntity.NetModule 也属于同类边界。

### 65.4 区段可见性与网络状态提交顺序

建议固定：服务器验证 -> 领域组件提交 -> 版本/长度化 Projection -> 按客户端区段/加入阶段选择接收者 -> 客户端拒绝越界或过期序列。当前模块缺少统一序列号、幂等键和错误返回，同一状态由 NetMessage、NetModule 和领域类同时广播时可能顺序反转。

### 65.5 本轮新增边界与统计

| 边界 | 关键证据 | 状态 |
| --- | --- | --- |
| WorldTileReplicationAdapter / LiquidSectionProjection | NetLiquidModule.cs:8-105, Liquid.cs:1184 | confirmed/partial |
| PresentationReplicationAdapter | NetTextModule.cs, NetAmbienceModule.cs, NetParticlesModule.cs | partial/missing |
| PlayerProgressReplicationAdapter | NetBestiaryModule.cs, Creative network modules | partial/missing |
| EntityRegistryReplicationAdapter | NetTeleportPylonModule.cs, LeashedEntity.NetModule | partial/missing |
| BannerReplicationAdapter / CraftingRequestAdapter | BannerSystem.cs, CraftingRequests.cs | missing |
| PacketValidationSystem | 各模块 Deserialize 与 NetworkInitializer.cs:14-28 | missing |

本轮新增约 6 个边界，累计独立边界约 338 个；至少约 264 个仍存在网络接收、权限、区段可见性、序列幂等和领域状态提交缺口。静态识别覆盖率约 97%～99%，权威行为闭合度下调为约 19%～30%。本节只描述 Version4 当前网络代码证据，不能宣称这些模块已形成完整可验证的客户端/服务器协议。

## 66. 第三十九轮：AmbienceServer 环境实体资格、强制队列与天空投影

### 66.1 环境实体生成是“资格查询 + 选择命令”，不是客户端随机特效

`AmbienceServer` 把环境实体候选注册在 `_spawnConditions` 和
`_secondarySpawnConditionsPerPlayer` 两张字典中（
`Terraria.GameContent.Ambience/AmbienceServer.cs:22-28,63-90`）。前者读取世界级
时间、天气、风力、Hardmode 和事件状态，后者读取玩家的沙漠、神圣、海滩、腐化、猩红、
丛林区域。`SkyEntityType` 当前列出 BirdsV、Wyvern、Airship、AirBalloon、Eyeball、
Meteor、BoneSerpent、Bats、Butterflies、LostKite、Vulture、PixiePosse、Seagulls、
SlimeBalloons、Gastropods、Pegasus、EaterOfSouls、Crimera、Hellbats 共 19 个枚举值
（`SkyEntityType.cs:3-23`）。这些是服务器发出的意图，不应直接成为客户端权威实体表。

建议拆分为：

| 边界 | 所有权/输入输出 | 证据与状态 |
| --- | --- | --- |
| `AmbientSpawnConditionCatalog` | 保存实体类型到世界级资格函数的定义；只读 `Main` 天气、时间和事件状态 | `AmbienceServer.cs:22-82`；`confirmed/partial` |
| `AmbientPlayerVisibilityQuery` | 纯查询玩家是否处于天空/地狱可见高度及区域条件 | `AmbienceServer.cs:93-101,187-221`；`confirmed` |
| `AmbientSpawnCandidateQuery` | 合并全局条件、玩家区域条件和高度过滤，输出候选类型集合 | `AmbienceServer.cs:114-136`；`confirmed/partial` |
| `AmbientSpawnClockComponent` | 保存下次尝试倒计时；周年世界减半间隔 | `AmbienceServer.cs:18-20,141-150`；`confirmed` |
| `AmbientSpawnSelectionSystem` | 消费倒计时和候选集合，按随机数选择目标玩家/类型并提交生成命令 | `AmbienceServer.cs:104-139,187-205`；`partial` |
| `AmbientForcedSpawnQueueComponent` | 保存 `AmbienceSpawnInfo(skyEntityType,targetPlayer)` 强制请求 | `AmbienceServer.cs:11-15,28,152-157`；`confirmed/partial` |
| `AmbientForcedSpawnDrainSystem` | 逐项解析目标玩家、验证高度、发送生成命令并移除请求 | `AmbienceServer.cs:159-181`；`partial` |
| `AmbientSkyEntitySpawnCommand` | 领域命令：向指定玩家或可见玩家请求一种天空实体 | `AmbienceServer.cs:224-229`；`partial` |
| `AmbientReplicationProjection` | 将玩家 ID、随机种子、类型编码为网络投影 | `NetAmbienceModule.cs:11-22`；`confirmed/partial` |

当前实现有几个必须显式保留的语义：`Update` 先排空强制队列，再处理计时器；倒计时按
`Main.dayRate` 扣减；候选为空或找不到可见玩家时本轮直接返回；带区域条件的候选先以
约 60% 概率优先选择，再退回所有满足高度的候选（`AmbienceServer.cs:104-139`）。
拆分时应以 `Query` 返回不可变候选集，避免查询内部持有随机数或修改队列。

### 66.2 强制生成队列的生命周期和身份边界未闭合

`ForceEntitySpawn` 只把结构体追加到列表；`SpawnForcedEntities` 对每项直接索引
`Main.player[targetPlayer]`，目标为 `-1` 时重新随机可见玩家，验证成功后广播，随后无论
成功与否都 `RemoveAt`（`AmbienceServer.cs:152-181`）。因此以下行为没有证据：目标玩家索引
越界/失效时是否应丢弃、请求是否需要来源权限、同一请求是否可重复、网络失败是否重试、
实体类型枚举越界如何处理。`AmbientForcedSpawnQueueComponent` 应保存请求 ID、来源、过期帧和
重试策略；`AmbientForcedSpawnDrainSystem` 应在移除前产生成功/拒绝结果，不能把“已出队”
当作“已生成”。

### 66.3 客户端天空实体消费方与生命周期缺失

`NetAmbienceModule.SerializeSkyEntitySpawn` 写入目标玩家 `whoAmI`、随机种子和
`SkyEntityType`（`NetAmbienceModule.cs:11-22`），但 `Deserialize` 是空体
（`:23-27`）。在当前代码中没有找到按枚举创建天空实体、绑定玩家、按随机种子初始化、
超时销毁或断线清理的消费链。该缺口不能由 `AmbienceServer` 的广播证据替代，故应增加：

* `AmbientSkyEntityRegistryAdapter`：把 19 个外部枚举映射到客户端表现工厂，并拒绝未知值；
* `AmbientSkyEntityLifetimeComponent`：保存目标玩家、生成序列、出生时间、随机种子和销毁原因；
* `AmbientSkyEntityLifetimeSystem`：按帧更新、越界/断线/超时清理；
* `AmbientPresentationProjection`：只读生命周期组件生成绘制/声音输入，不反向写服务器状态。

这四个边界目前均为 `missing` 或 `partial`，不能把天空实体当作已实现的可运行模拟实体。

### 66.4 SkyManager/EffectManager 只是注册壳，Credits Roll 是独立事件投影

`EffectManager<T>` 维护字符串到效果实例的字典、加载状态和 `Bind` 操作
（`Terraria.Graphics.Effects/EffectManager.cs:6-41`）；`SkyManager` 另外声明
`_activeSkies`，但 `OnActivate` 为空（`SkyManager.cs:7-12`）。`OverlayManager` 分配按
`EffectPriority` 分组的链表，然而 `OnActivate` 也为空且未见更新/绘制消费
（`OverlayManager.cs:8-22`）。这说明效果注册、激活集合、优先级排序和卸载之间没有闭合
调用链，不能把这些容器作为权威世界状态。

`CreditsRollEvent` 保存全局剩余帧数，启动时固定为 28,800，若 `SkyManager["CreditsRoll"]`
存在则读取 `CreditsRollSky.AmountOfTimeNeededForFullPlay`，并通过 `NetMessage.SendData`
广播；每帧 `UpdateTime` 递减并钳制到 0，世界重置时清零
（`Terraria.GameContent.Events/CreditsRollEvent.cs:8-47`）。建议拆为：

| 边界 | 责任 | 状态 |
| --- | --- | --- |
| `CreditsRollStateComponent` | 保存剩余帧、最大时长、启动来源和版本 | `CreditsRollEvent.cs:8-10,12-22`；`partial` |
| `CreditsRollCommandAdapter` | 将 NPC/事件触发转换为启动、重置、向单玩家补发命令 | `CreditsRollEvent.cs:12-32`；`partial` |
| `CreditsRollTickSystem` | 以固定帧递减并在 0 时产生结束事件 | `CreditsRollEvent.cs:34-47`；`partial` |
| `CreditsRollReplicationProjection` | 向全体或加入中的单玩家投影剩余帧 | `CreditsRollEvent.cs:21-32`；`partial` |
| `CreditsRollProjection` | 把事件状态转换为客户端天空/动画表现 | `CreditsRollSky.cs:10-35`；`missing` |
| `CreditsRollAnimationSystem` | 维护 segment、绘制、激活/停用和可见性 | `CreditsRollSky.cs:22-35`、`CreditsRollComposer.cs:13-15`；`missing` |

`CreditsRollSky` 的 `Update`、`Draw`、`Reset`、`Activate`、`Deactivate`、
`EnsureSegmentsAreMade` 均为空或默认返回，`CreditsRollComposer` 只有空类壳；因此
`CreditsRollEvent` 的服务器计时与客户端动画之间仅有“剩余帧网络消息”这一条细线，缺失
动画段生成、播放进度确认、重复启动仲裁和断线重放。该事件应保持为表现投影，不能写回
世界天气、NPC 或其他权威组件。

### 66.5 建议的调用方向与验证 seam

```text
World/Player 状态
  -> AmbientSpawnConditionCatalog
  -> AmbientPlayerVisibilityQuery
  -> AmbientSpawnCandidateQuery
  -> AmbientSpawnSelectionSystem / AmbientForcedSpawnDrainSystem
  -> AmbientSkyEntitySpawnCommand
  -> AmbientReplicationProjection
  -> 客户端 RegistryAdapter
  -> SkyEntityLifetimeSystem -> PresentationProjection

CreditsRollCommandAdapter
  -> CreditsRollStateComponent
  -> CreditsRollTickSystem
  -> CreditsRollReplicationProjection
  -> CreditsRollProjection / AnimationSystem
```

最小 focused verifier 应覆盖：天气/时间边界下候选集合的纯函数快照；天空与地狱高度的
边界像素；强制队列的无效玩家、未知枚举、重复请求和过期请求；环境包的玩家 ID/随机种子/
类型 round-trip 与未知类型拒绝；Credits Roll 的 0/1/28,800 帧钳制、重复启动和新玩家
补发。当前没有这些 verifier 的运行证据，均标记为 `未验证`。

### 66.6 本轮新增边界与统计

本轮新增约 **14 个**边界（环境资格目录、可见性/候选查询、时钟、选择、强制队列及排空、
生成命令/投影、客户端注册与生命周期、Credits Roll 状态/命令/计时/投影/动画）。累计
独立边界约 **352 个**；至少约 **278 个**仍存在空反序列化、玩家身份/枚举校验、强制请求
生命周期、天空实体消费、效果激活集合和 Credits Roll 动画缺口。静态识别覆盖率约
**98%～99%**，权威行为闭合度约 **18%～29%**。这些数字是基于当前 `Version4` 源码和
本报告已记录边界的工程估计，不代表运行时完整度或可交付模拟覆盖率。

## 67. 第四十轮：高尔夫专用物理、地图投影存储与电影胶片调度

### 67.1 GolfState 是本地表现状态，球 Projectile 仍是实体真相

`Main` 在初始化时创建 `LocalGolfState`，并在世界清理、每帧更新和摄像机跟踪路径调用它
（`Terraria/Main.cs:1030,3284,11338,12386`；`WorldGen.cs:6458`）。`GolfState` 保存
计分计时器（最大 3600 帧、延迟 90 帧）、最后击球时间/位置、等待球静止标志、最后击球
Projectile、摆杆计数及最多 1000 个轨迹槽位（`Terraria.GameContent.Golf/GolfState.cs:8-26`）。
`GetLastHitBall` 只有在 Projectile 仍 active、类型被 `ProjectileID.Sets.IsAGolfBall` 标记、
归属本地玩家且 `ai[1]` 与摆杆计数一致时才返回；`TryGetCameraTrackingPosition` 随后将
镜头跟踪球中心或最近两秒内的记录位置（`GolfState.cs:38-55,68-77`）。这说明该状态是
客户端镜头/计分投影，不是服务器球体权威的替代物。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `GolfBallCapabilityComponent` | 仅给高尔夫球 Projectile 附着摆杆计数、静止和球场关联 | `GolfState.cs:20-26,68-77`；`partial` |
| `GolfBallIdentityQuery` | 验证 active、球类型、本地 owner 和摆杆序列 | `GolfState.cs:68-77`；`confirmed` |
| `GolfCameraTrackingProjection` | 输出球中心或最近记录位置供摄像机使用 | `GolfState.cs:38-55`；`confirmed/partial` |
| `GolfScoreTimerComponent` / `GolfScoreSystem` | 维护计分阶段与 3600 帧上限 | `GolfState.cs:8-14,28-36,79-104`；`partial` |
| `GolfHitTrackComponent` | 保存击球位置轨迹，按球洞/回球规则聚合 | `GolfState.cs:16-26`、`GolfBallTrackRecord.cs:6-10`；`missing` |
| `GolfPhysicsAdapter` | 将球碰撞/液体穿越事件转换为高尔夫规则输入 | `GolfHelper.cs:15-20`、`Terraria.Physics/IBallContactListener.cs:5-10`；`missing` |

`GolfHelper.ContactListener.OnCollision` 和 `OnPassThrough` 为空，`BallCollision` 也是空类
（`Terraria.GameContent.Golf/GolfHelper.cs:15-20`；`Terraria.Physics/BallCollision.cs:8-10`）。
虽然 `PhysicsProperties` 明确给出重力 `0.3` 与阻力 `0.99`，但没有碰撞解析、法线/冲击点
处理、液体类型分支或角速度衰减证据。`GolfPhysicsAdapter` 不应让 `GolfState` 直接修改
Projectile；应由物理系统产生不可变 `BallCollisionEvent`/`BallPassThroughEvent`，再由规则
System 提交球状态或成绩命令。

### 67.2 地图是客户端探索投影，更新队列和持久化均未闭合

`WorldMap` 声明固定宽高、`MapTile[,]` 存储及索引器；`Clear` 遍历所有格并清零
（`Terraria.Map/WorldMap.cs:10-20,36-48`），但构造函数没有初始化 `MaxWidth`、`MaxHeight`
和 `_tiles`，`QueueUpdate` 直接返回默认 `false`（`:22-28`）。`MapUpdateQueue.Add` 在服务端、
世界生成中或地图禁用时返回；单点更新还依赖 `Main.Map.QueueUpdate(x,y)`，但这项能力当前
无法证明会把区域加入队列（`MapUpdateQueue.cs:17-35`）。`WorldGen` 和 `MessageBuffer` 确实
会提交区域/坐标更新（`WorldGen.cs:67215,68029`；`MessageBuffer.cs:820`），形成“写 Tile ->
标脏 -> 地图重绘/保存”的预期边界，但消费端缺失。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `MapTileComponent` | 类型、亮度、颜色、changed/queued 标志 | `MapTile.cs:3-67`；`confirmed` |
| `WorldMapStore` | 宽高、二维 tile 存储、边界检查和清空 | `WorldMap.cs:10-48`；`partial/missing` |
| `MapUpdateQueueComponent` | 区域更新队列、容量 262,144、并发锁 | `MapUpdateQueue.cs:9-15`；`partial` |
| `MapDirtyMarkSystem` | 从世界 Tile/网络变更产生地图更新请求 | `MapUpdateQueue.cs:17-35`、`WorldGen.cs:67215,68029`；`partial` |
| `MapProjectionCapture` | 从 `SceneMetrics` 捕获场景区域和雪量派生值 | `MapHelper.cs:89-95`；`confirmed` |
| `MapPersistenceAdapter` | 在本地/云存档条件下压缩、写入和错误记录 | `MapHelper.cs:97-124`；`partial/missing` |
| `MapOverlayProjection` | 将出生点、队伍出生点、传送水晶、Ping 转成绘制层 | `IMapLayer.cs:3-6`、各 `*MapLayer.cs`；`missing` |

`MapHelper.SaveMap` 虽有云存档和 `IOLock` 门槛，但 `InternalSaveMap` 为空；异常记录写入
固定 `client-crashlog.txt`，没有版本、原子替换或损坏恢复证据（`MapHelper.cs:97-124`）。
`PingMapLayer` 保存最多 100 个带 15 秒时间戳的 Ping，但 `Draw` 为空且没有添加 Ping 的
公开入口（`PingMapLayer.cs:11-33`）。出生点/队伍出生点/传送水晶层的 `Draw` 同样为空，
因此这些层只能标为 `missing`，不能算已实现地图 UI。

### 67.3 CinematicManager/Film 提供时间轴内核，但没有内容注册和渲染边界

`CinematicManager` 维护 FIFO `_films`，每帧仅更新第一个 Film：未激活时调用 `OnBegin`，
在 `FocusHelper.UpdateVisualEffects` 为真时调用 `OnUpdate`，结束后调用 `OnEnd` 并移除
（`Terraria.Cinematics/CinematicManager.cs:6-27`）。`Film` 把事件按起始帧和持续时间存入
私有 `Sequence`，支持并行序列、追加序列、关键帧和逐帧 `FrameEventData`；空时间轴立即
返回 false，正常结束条件是 `_frame == _frameCount`（`Film.cs:7-129`）。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `CinematicTimelineComponent` | 只保存序列起始帧、持续时间、当前帧和激活状态 | `Film.cs:7-41`；`confirmed/partial` |
| `CinematicSequenceCommand` | 添加/追加序列或关键帧，更新时间轴末端 | `Film.cs:43-109`；`confirmed` |
| `CinematicTickSystem` | 依时间推进，发出当前帧事件和开始/结束事件 | `CinematicManager.cs:12-27`、`Film.cs:111-129`；`partial` |
| `CinematicQueueComponent` | 管理 FIFO 胶片队列及取消/跳过策略 | `CinematicManager.cs:10-25`；`partial` |
| `CinematicPresentationAdapter` | 将 `FrameEventData` 转换为摄像机、音频、特效和 UI 命令 | `FrameEvent.cs:3`、`Main.cs:11168`；`missing` |

当前找不到向 `_films` 添加 Film 的公开入口；`CinematicManager.Update` 也只在主循环被调用，
没有网络同步、暂停恢复、跳过、异常回收或场景切换清理证据。`Film.OnBegin/OnEnd` 默认空体，
所以时间轴回调虽可执行，内容层仍不能视为闭合的过场系统。

### 67.4 统一数据流与验证 seam

```text
Projectile/Golf input -> BallCollision events -> GolfPhysicsAdapter
  -> GolfBallCapability / GolfScoreSystem -> Camera/Score projections

World Tile mutation -> MapDirtyMarkSystem -> MapUpdateQueue
  -> WorldMapStore -> MapOverlayProjection / MapPersistenceAdapter

CinematicSequenceCommand -> CinematicQueue/Timeline -> CinematicTickSystem
  -> FrameEventData -> PresentationAdapter
```

最小 focused verifier 应覆盖：高尔夫球 owner/type/摆杆序列失效、球静止和最近两秒镜头
跟踪、碰撞/液体事件不可变输入；地图构造后的宽高与边界索引、重复区域合并、队列容量和
云存档失败恢复；Film 空时间轴、并行序列、关键帧、暂停和结束回调。当前代码没有这些
verifier 的运行证据，均标记为 `未验证`。

### 67.5 本轮新增边界与统计

本轮新增约 **17 个**边界（高尔夫能力/查询/计时/轨迹/物理适配、地图存储/Tile/更新/投影/
持久化/图层、电影时间轴/命令/队列/表现适配）。累计独立边界约 **369 个**；至少约 **294 个**
仍存在空物理处理、地图构造/队列消费/保存、图层绘制、Cinematic 内容注册和表现适配缺口。
静态识别覆盖率约 **98%～99%**，权威行为闭合度约 **17%～28%**。统计继续排除
`D:\TRbackup\NLTX\src` 已有组件，不代表 Version4 可运行覆盖率。

## 68. 第四十一轮：输入触发快照、锁定目标查询与 Gamepad 交互适配

### 68.1 输入目录应停留在客户端意图边界

`PlayerInput` 当前只声明 `LockGamepadTileUseButton` 和原始屏幕尺寸
（`Terraria.GameInput/PlayerInput.cs:18-29`），但 `Main` 初始化中原本的输入初始化、
动作事件和 Minimap 绑定均被注释（`Terraria/Main.cs:3279-3309`）。因此不能把当前类当作
已经闭合的输入系统；它最多是未来 `InputAdapter` 的壳。`TriggersSet` 则明确区分当前/上一帧
按键、最新输入模式、移动键使用标志、热键滚动冷却和长按计时，并提供 `Reset` 与
`CloneFrom`（`Terraria.GameInput/TriggersSet.cs:7-48`）；`TriggersPack` 聚合 Current、Old、
JustPressed、JustReleased 四份集合（`TriggersPack.cs:5-22`）。建议把它们建模为不可变帧快照
和输入边沿计算，而不是让 Player/NPC 直接读全局按键字典。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `RawInputAdapter` | 从键盘、鼠标、Gamepad 产生原始动作样本 | `PlayerInput.cs:18-29`、`Main.cs:3279-3309`；`missing` |
| `InputTriggerSnapshotComponent` | 保存 Current/Old/JustPressed/JustReleased 及输入模式 | `TriggersPack.cs:5-22`、`TriggersSet.cs:7-48`；`confirmed/partial` |
| `InputEdgeSystem` | 由相邻帧快照计算按下、释放、长按和滚动冷却 | `TriggersSet.cs:21-48`；`partial` |
| `PlayerIntentProjection` | 把客户端输入投影为移动、使用、交互、瞄准意图 | `Player.cs:19608-19665`；`partial` |
| `InputSettingsComponent` | 保存各平台键位、死区、反转和库存移动冷却 | `PlayerInputProfile.cs:9-63`；`confirmed/partial` |
| `GamepadTileInteractionLock` | 防止同一帧 Gamepad Tile 使用重复触发 | `Player.cs:19665-19672`、`PlayerInput.cs:22`；`partial` |

`TriggersSet.Reset` 只清空 `KeyStatus` 布尔值，不重置 `LatestInputMode`、冷却或长按字段；
`CloneFrom` 又清空并复制部分字段但没有复制 `LatestInputMode`（`TriggersSet.cs:21-48`）。
因此输入边沿状态的生命周期和清理顺序需要单独验证，不能假定 `Reset` 等价于完整帧重置。

### 68.2 LockOnHelper 是纯目标/弹道查询，但 GPU 光标写入缺失

`LockOnHelper` 保存锁定开关、NPC 槽位列表和选中索引，目标范围常量为 2000，锁定保持寿命
为 40 帧（`Terraria.GameInput/LockOnHelper.cs:10-21`）。`AimedTarget` 通过 `_targets` 读取
`Main.npc`；`PredictedPosition` 会调用 `NPC.GetNPCLocation`，按目标速度和距离预测 45 帧，
再根据武器的 `LockOnAimAbove` 与 `LockOnAimCompensation` 调整瞄准点，并用
`WorldGen.InWorld/SolidTile` 检查上方空间（`LockOnHelper.cs:23-76`）。`Player.ItemCheckWrapped`
和 `Main` 的 NPC/Projectile 更新前后调用 `SetUP/SetDOWN`（`Player.cs:19633-19642`；
`Main.cs:11528-11552`），说明该查询位于输入到物品使用之间的表现 seam。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `LockOnTargetSetComponent` | 当前候选 NPC 槽位、选中索引、启用状态 | `LockOnHelper.cs:17-21`；`partial` |
| `LockOnEligibilityQuery` | active/距离/可见性/目标类型资格 | `LockOnHelper.cs:23-48`、`NPC.GetNPCLocation`；`partial` |
| `AimPredictionQuery` | 速度、距离、重力和武器补偿后的预测点 | `LockOnHelper.cs:35-76`；`confirmed/partial` |
| `LockOnCursorProjection` | 将预测点投影到反重力屏幕坐标并在使用前设置光标 | `LockOnHelper.cs:78-100`；`missing` |
| `LockOnLifetimeSystem` | 40 帧保持、目标死亡/断开和武器切换清理 | `LockOnHelper.cs:13-21`；`missing` |

`SetLockPosition` 和 `ResetLockPosition` 均为空体（`LockOnHelper.cs:99-100`），没有发现
候选列表填充、`_pickedTarget` 边界检查、锁定保持计时或目标失效清理。`AimedTarget` 在
`_pickedTarget` 非 `-1` 但超出 `_targets` 长度时也没有证据保护。故 Lock-On 只能作为客户端
瞄准投影；服务器必须重新验证目标槽位、距离、武器权限和伤害命令，不能信任客户端预测点。

### 68.3 SmartSelect 和 Minimap 是未闭合表现适配，不应提升为权威子系统

`SmartSelectGamepadPointer`、`MinimapFrame`、`MinimapFrameManager` 和
`MinimapFrameTemplate` 当前均为空壳；初始化绑定被注释（`Terraria.GameContent.UI.Minimap/*`；
`Main.cs:3288-3309`）。`Player.ItemCheckWrapped` 中 Smart Select/Forward Cursor 逻辑被
整段注释，`ForceForwardCursor` 与 `ForceSmartSelectCursor` 为空体（`Player.cs:19608-19631,
19647-19648`）。这形成明确边界但没有权威行为证据：

* `SmartCursorProjection`：从 TileReach/物品类型推导游标候选，仅输出 UI 位置；
* `GamepadPointerState`：保存指针、半径和选择确认的短期客户端状态；
* `MinimapFrameRegistryAdapter`：加载框架模板和配置，不持有探索真相；
* `MinimapOverlayProjection`：从 `WorldMap`/Ping/传送水晶层输出绘制数据。

上述四项均标记 `missing`；探索可见性和地图 Tile 仍由地图投影/玩家进度边界负责。不得
因为空壳类型存在，就把 UI 选择状态写回服务器实体或世界 Tile。

### 68.4 输入到权威命令的方向与验证 seam

```text
RawInputAdapter
  -> InputTriggerSnapshot / InputEdgeSystem
  -> LockOnEligibilityQuery + AimPredictionQuery
  -> PlayerIntentProjection
  -> ServerCommandValidation
  -> Player/Tile/Projectile systems

WorldMap + Ping/Pylon progress -> MinimapOverlayProjection
Input snapshot -> SmartCursorProjection / GamepadPointerState
```

最小 focused verifier 应覆盖：Current/Old 快照的按下/释放边沿、Reset 后冷却与输入模式、
Gamepad Tile 锁定的同帧重复输入；锁定目标列表为空、索引越界、NPC 死亡、距离超过 2000、
武器补偿与固体 Tile 阻挡；客户端预测点被服务器重算而非直接采信；Minimap 空数据、配置
切换和表现层不写权威状态。当前没有这些 verifier 的运行证据，均标记为 `未验证`。

### 68.5 本轮新增边界与统计

本轮新增约 **15 个**边界（原始输入、触发快照/边沿、玩家意图、输入设置、Gamepad 锁定、
Lock-On 候选/预测/光标/生命周期、Smart Cursor、Gamepad Pointer、Minimap 注册/投影）。
累计独立边界约 **384 个**；至少约 **307 个**仍存在输入初始化、帧边沿清理、锁定候选填充、
光标写入、目标失效处理及 Minimap 表现链缺口。静态识别覆盖率约 **98%～99%**，权威行为
闭合度约 **16%～27%**。本轮仍只读取 `D:\TRbackup\Version4`，并排除
`D:\TRbackup\NLTX\src` 已有组件。

## 69. 第五十三轮：成就持久化、平台社交生命周期与音频实例资源

### 69.1 AchievementManager 是独立的本地/云端持久化边界

`AchievementManager` 保存成就字典、序列化设置、图标索引、保存路径、云端标志和加密密钥；
构造时根据 `SocialAPI.Achievements` 选择平台保存路径/密钥，否则使用
`Main.SavePath/achievements.dat` 和固定 ASCII 密钥（`Terraria.Achievements/AchievementManager.cs:16-52`）。
`Save()` 通过 `FileUtilities.ProtectedInvoke` 进入私有 `Save(path, cloud)`，但该私有实现为空
（`:54-64`），所以文件格式、加密、原子替换和云端错误恢复均未闭合。

`Achievement` 以静态递增 ID、名称、本地化文本和条件字典组成内容/进度对象
（`Achievement.cs:10-42`）。`AchievementCondition` 具有 `Completed` 持久字段、Tracker
引用和 `Load/Clear/Complete/CreateAchievementTracker` 生命周期，但这些虚方法全部为空或
返回默认值（`AchievementCondition.cs:7-31`）。`AchievementTracker<T>` 虽然在
`SetValue` 中有更新/完成判断，却把接口 `ReportAs/GetTrackerType/Clear/Load` 和
`OnComplete` 留为空；`ConditionFloatTracker.ReportUpdate/Load` 也为空
（`AchievementTracker.cs:5-51`、`ConditionFloatTracker.cs:5-19`）。

游戏内事件入口是真实存在的：Chest、Main、Mount、Player、NPC、Projectile、WorldGen 等多处
调用 `AchievementsHelper.NotifyProgressionEvent`、`NotifyNPCKilled` 或
`HandleSpecialEvent`，退出/死亡流程调用 `Main.Achievements.Save()`
（`Terraria/Player.cs:26400` 及上述事件调用点）。因此成就应与 Bestiary、战斗和世界进度
分离，作为事件订阅后的持久化投影。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `AchievementDefinitionCatalog` | 成就名称、文本、条件定义和稳定内容 ID | `Achievement.cs:10-42`；`partial`，注册入口未完全展开 |
| `AchievementProgressLedger` | 条件完成状态、Tracker 当前值和上限 | `AchievementCondition.cs:11-18`、`AchievementTracker.cs:7-18`；`partial` |
| `AchievementEventAdapter` | 将拾取、制作、击杀、Tile/世界事件转换成进度命令 | `AchievementsHelper` 调用点；`partial` |
| `AchievementPersistenceAdapter` | 本地/云端路径、序列化、加密、版本迁移和原子保存 | `AchievementManager.cs:24-64`；`missing` |
| `AchievementPlatformProjection` | 向 `AchievementsSocialModule` 报告完成/统计 | `AchievementsSocialModule.cs:3-20`；`partial` |
| `AchievementNotificationProjection` | 完成提示、图标和客户端 UI 更新 | `GetIconIndex`、事件入口；`missing` |

成就事件不能直接修改战斗或世界权威状态；Tracker 的 `SetValue` 只能产生进度事件，
`Complete` 应在持久账本提交成功后再通知平台。当前没有存档加载入口的证据，不能宣称重启后
进度可恢复。

### 69.2 SocialAPI 是模块编排器，平台实现和加入请求消费未闭合

`SocialAPI.Initialize` 选择 None/Steam/WeGame，创建 `_modules` 和
`ServerJoinRequestsManager`，把 `JoinRequests.Update` 挂到内部 tick，再调用每个模块的
`Initialize`；`Shutdown` 反向关闭模块（`Terraria.Social/SocialAPI.cs:10-73`）。然而
`LoadSteam` 与 `LoadWeGame` 都为空（`:75-76`），因此当前源码不能证明任何平台模块真正加入
模块列表。

抽象模块接口已划出不同外部能力：`AchievementsSocialModule` 提供加密密钥、保存路径、统计和
完成上报；`CloudSocialModule` 提供文件枚举、读写、删除和 Forget；`NetSocialModule` 提供
连接、收发、邀请和监听；`FriendsSocialModule` 提供用户名/加入界面；`WorkshopSocialModule`
提供世界/资源包发布与导入；`OverlaySocialModule` 提供 Gamepad 文本输入
（`Terraria.Social.Base/*.cs`）。这些应是 Adapter 端口，不能把平台句柄放进 WorldSession。

`ServerJoinRequestsManager` 保存只读请求集合并逐 tick 调用 `IsValid()`，但失效请求的
`RemoveRequestAtIndex` 为空（`ServerJoinRequestsManager.cs:6-32`）。`UserJoinToServerRequest`
只提供显示名、完整标识、有效性和 wrapper 文本抽象；因此请求进入、批准、拒绝、过期和通知
链条仍缺失。`RichPresenceState.Equals` 也返回默认布尔值，状态变化检测无法确认。

WeGame 的 `IPCBase` 虽声明 producer/consumer 缓冲、PipeStream、取消令牌和数据到达事件，
但 `Reset`、读写回调、`BeginReadData`、`Send` 和 `SendCallback` 均为空或默认返回；
`IPCClient.ReadCallback` 亦为空（`Terraria.Social.WeGame/IPCBase.cs:11-59`）。这属于外部
协议适配器，不得作为权威联机状态。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `SocialModuleRegistryComponent` | 当前模式、模块集合和初始化/关闭阶段 | `SocialAPI.cs:12-73`；`partial` |
| `PlatformSocialAdapter` | Steam/WeGame 模块加载与生命周期 | `SocialAPI.LoadSteam/LoadWeGame`；`missing` |
| `CloudSaveAdapter` | 云端文件读写、大小、删除和冲突策略 | `CloudSocialModule.cs:6-48`；`partial` |
| `AchievementPlatformAdapter` | 平台统计、完成状态和密钥/路径 | `AchievementsSocialModule.cs:3-20`；`partial` |
| `ServerJoinRequestComponent` | 待批准请求及有效期 | `ServerJoinRequestsManager.cs:6-29`；`confirmed` |
| `JoinRequestLifecycleSystem` | 过期清理、批准/拒绝和回调 | `RemoveRequestAtIndex`、`UserJoinToServerRequest`；`missing` |
| `ExternalIpcTransportAdapter` | Pipe 缓冲、取消、异步读写和错误恢复 | `IPCBase.cs:11-59`；`missing` |
| `RichPresenceProjection` | 菜单/建角/建世/单人/多人状态投影 | `RichPresenceState.cs:6-28`；`partial` |

系统顺序应为：平台加载 → 模块初始化 → 外部事件适配 → 权威命令/持久化投影 → 反向关闭；
当前 `SocialAPI.Shutdown` 未清除 tick 委托，也没有空模块列表保护，关闭顺序和重复初始化都
需要 focused verifier。

### 69.3 音频是客户端资源生命周期，不应进入权威模拟

`ActiveSound` 保存声音样式、位置、音量、音调、全局标志、循环播放条件和
`SoundEffectInstance`；构造器区分空间声音/全局声音/循环声音，并调用 `UseOverrides`、
`Play` 或 `PlayLooped`，但这三个方法均为空；只有 `Stop` 会停止已有实例
（`Terraria.Audio/ActiveSound.cs:6-80`）。因此声音请求和实际播放必须分开建模：前者是表现
命令，后者是客户端资源适配器。

`LegacySoundPlayer` 预分配大量 Drip、Liquid、Dig、Thunder、Item、NPC、Door、Menu、Run、
Chat 等 `Asset<SoundEffect>` 与实例数组，并维护 `_trackedInstances`；但 `LoadAll` 为空，
`PlaySound` 无论类型都返回 null，无法证明资源加载、实例复用、位置衰减或跟踪回收
（`Terraria.Audio/LegacySoundPlayer.cs:12-170`）。`DoesSoundScaleWithAmbientVolume` 仅有一组
类型白名单，不能替代播放实现。

`SoundInstanceGarbageCollector` 是空静态类型，没有发现回收入口；这意味着播放实例泄漏、
停机释放、音频设备重建和异常资源路径均未闭合。音频不应写入 Player/NPC/World 的权威字段，
只消费事件和客户端场景状态。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `SoundRequestProjection` | 将战斗、UI、环境事件转成样式/位置/优先级请求 | `ActiveSound` 构造器；`partial` |
| `AudioInstanceAdapter` | 创建、播放、循环条件、音量/音调覆盖和停止 | `ActiveSound.cs:36-80`；`missing` |
| `LegacySoundAssetCatalog` | 旧声音类型到 Asset/实例数组的映射 | `LegacySoundPlayer.cs:14-151`；`confirmed`（目录） |
| `LegacySoundLoadSystem` | 加载资源、验证数组长度和设备重建 | `LegacySoundPlayer.LoadAll`；`missing` |
| `AudioInstanceLifetimeSystem` | 跟踪、复用、停止和垃圾回收 | `_trackedInstances`、`SoundInstanceGarbageCollector`；`missing` |
| `AmbientVolumeQuery` | 计算环境音量缩放而不修改声音权威状态 | `DoesSoundScaleWithAmbientVolume`；`partial` |

### 69.4 调用方向与验证计划

```text
Gameplay/World events -> AchievementEventAdapter -> AchievementProgressLedger
  -> AchievementPersistenceAdapter + AchievementPlatformProjection

Social mode -> PlatformSocialAdapter -> SocialModuleRegistry
  -> Cloud/Achievement/Network/Workshop adapters
  -> JoinRequestLifecycleSystem / RichPresenceProjection

Gameplay/UI/Scene events -> SoundRequestProjection -> AudioInstanceAdapter
  -> AudioInstanceLifetimeSystem / LegacySoundAssetCatalog
```

最小 focused verifier 应覆盖：成就条件加载/清理/完成、Tracker 类型和上限、加密存档原子写入、
云端失败回退、重复平台完成事件；SocialAPI 重复初始化/关闭、空平台加载、请求过期删除、
批准拒绝和 IPC 截断/断管；ActiveSound 循环条件、播放失败、停止幂等、旧声音资源缺失和
实例回收。当前均没有运行证据，标记为 `未验证`。

### 69.5 本轮新增边界与统计

本轮新增约 **22 个**边界（成就定义/账本/事件/持久化/平台/通知、社交模块注册/平台加载/
云存档/成就平台/加入请求/生命周期/IPC/Rich Presence、声音请求/实例/旧资源目录/加载/回收/
环境音量）。以报告上一节登记的约 453 个为基线，累计独立边界约 **475 个**；至少约 **395 个**
仍存在成就存档、平台加载、加入请求消费、IPC 异步读写、音频播放和实例回收缺口。静态识别
覆盖率约 **98%～99%**，权威行为闭合度约 **13%～23%**。这些是源码阅读估计，不代表
Version4 已形成完整可运行模拟。本轮只读取 `D:\TRbackup\Version4`，未修改其源码，并继续
排除 `D:\TRbackup\NLTX\src` 中已有组件。

## 70. 第五十六轮：雷暴闪电、墓地风与瀑布表现的环境事件链

### 80.1 StormLightning 的“世界选择 → 伤害投射物”链可见，但落点生成缺失

`WorldGen.SpawnStormLightning` 只在 `Main.IsItStorming` 时运行；它先为所有存活玩家构建
24×24 Tile 的安全矩形，并根据玩家水平速度扩展矩形，再仅对处在雨区、非雪区、未 AFK 的
玩家按危险状态、睡眠和钓鱼状态调整随机频率（`Terraria/WorldGen.cs:59799-59848`）。这证明
雷暴并非纯客户端视觉：天气和玩家活动资格驱动一次世界事件选择。但真正的
`SpawnStormLightningNearPlayer` 是空体，尚未验证安全区排除、Tile 吸引规则、投射物创建、
随机种子或网络来源。

Projectile 类型 1091 的命中表现已经消费该结果：它请求本地 StormLightning 粒子，使用
`ai[2]` 作为种子调用 `LightningGenerator.StormLightning.GenerateMainBoltPath`，若闪电与 Tile
相撞则将 Projectile 中心移到路径末端；随后根据液体设定本地状态、可能执行
`DoLightningKillLambda`，构建 `MultiPointHitbox` 并调用 `Damage()`
（`Terraria/Projectile.cs:46520-46558`）。`AI_203_StormLightning` 本身为空，因此持续行为、
存活时间和命中后的清理链未闭合。

`LightningGenerator` 的 StormLightning 配置包含步长、层数、叉枝阈值、深度、长度和实体 Tile
碰撞开关（`Terraria.GameContent/LightningGenerator.cs:24-46`）；入口通过确定性 `LCG32Random`
由 `seed` 推导起始方向（`:48-64`）。但 `GenerateBolt` 只返回空 `Bolt`、`CalcRotations` 返回
默认值、`SmoothRotations` 为空（`:86-94`）。因此 `Bolt.positions`、碰撞结果、分叉列表和旋转
都没有被建立，Projectile 对路径末端和 MultiPointHitbox 的访问不能被视为可运行的权威链。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `StormEligibilityQuery` | 检查风暴、雨区/雪区、存活、AFK、睡眠与钓鱼修正 | `WorldGen.SpawnStormLightning`；`confirmed/partial` |
| `StormSafetyZoneSnapshot` | 所有玩家的当前位置、速度预测与安全 Tile 矩形 | `WorldGen.cs:59809-59825`；`confirmed` |
| `StormTargetSelectionQuery` | 在安全区、Tile 吸引定义和随机种子下选择落点 | `SpawnStormLightningNearPlayer`、`TileID.Sets.AttractsStormLightning`；`missing` |
| `StormLightningSpawnCommand` | 目标、种子、伤害/owner 与生成原因 | WorldGen 到 Projectile 预期边界；`missing` |
| `LightningPathDefinition` | 步长、分层、分叉、碰撞和长度配置 | `LightningGenerator.StormLightning.Generator`；`confirmed` |
| `LightningPathGenerationQuery` | 给定 seed 生成不可变折线路径、碰撞与分叉 | `GenerateBolt`、`CalcRotations`；`missing` |
| `StormDamageProjectionSystem` | 路径终点、液体交互、多点命中盒与伤害触发 | `Projectile.cs:46520-46558`；`partial` |
| `StormVisualProjection` | 粒子、旋转和颜色表现，不改变伤害结论 | `ParticleOrchestrator`、`AI_203_GetLightningColor`；`partial` |

`StormTargetSelectionQuery` 与 `LightningPathGenerationQuery` 应以服务器可复现 seed 为输入；客户端
只接收已决定的命令或只读路径投影。路径生成不可在 Draw 或粒子线程中反向决定伤害目标。

### 80.2 AmbientWindSystem 是每帧墓地环境扫描入口，生成与工作区仍为空

`Main.DoUpdateInWorld` 每 tick 调用 `_ambientWindSys.Update()`，随后更新相机和
`SceneState`（`Terraria/Main.cs:11631-11655`）。`AmbientWindSystem.Update` 仅在本地玩家处于
墓地区域时继续，递增计数器、取得 Tile 工作区、逐 Tile 尝试生成风，并每 30 tick 生成一次
空中风（`Terraria.GameContent/AmbientWindSystem.cs:7-38`）。其随机源、候选点列表和计数器
证明这是客户端环境效果调度器，而不是全局风速/天气权威状态。

不过 `GetTileWorkSpace` 返回空 `Rectangle`，`TrySpawningWind` 与 `SpawnAirborneWind` 均为空，
尚不存在 Tile 可见性、实体/墙体资格、产物容量、随机种子隔离或场景卸载清理证据。它必须
消费已存在的 `SceneMetrics`/玩家墓地快照，不能扫描结果写回 `ZoneGraveyard` 或天气规则。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `AmbientEffectEligibilityQuery` | 从本地场景快照判断墓地环境效果是否允许 | `AmbientWindSystem.Update`；`confirmed` |
| `AmbientTileWorkspaceQuery` | 计算相机/玩家附近可扫描 Tile 矩形并作边界裁剪 | `GetTileWorkSpace`；`missing` |
| `AmbientWindSpawnQuery` | 根据 Tile/墙、遮挡、随机源和容量挑选生成点 | `TrySpawningWind`、`_spotsForAirboneWind`；`missing` |
| `AmbientWindCadenceSystem` | 维护 tick 计数、30 tick 空中生成节拍和场景重置 | `_updatesCounter`、`Update`；`partial` |
| `AmbientWindProjection` | 生成 Dust/Particle/音效等本地表现对象 | `SpawnAirborneWind`；`missing` |

### 80.3 WaterfallManager 和 BackgroundChangeFlashInfo 都是表现缓存，不能承担地形权威性

`WaterfallManager` 预分配最多 1,000 条 `WaterfallData`（x/y/type/stopAtStep）和 28 张贴图，
并可由 `BindTo` 订阅 `Preferences.OnLoad`（`Terraria/WaterfallManager.cs:11-44`）。但
`Configuration_OnLoad` 为空，`Main` 虽会创建管理器，绑定偏好设置的调用已被注释
（`Terraria/Main.cs:3269,3308`）；没有发现瀑布扫描、流向推进、绘制或清理消费者。故它只能
拆成表现缓存与偏好 Adapter，不能从数组容量推断出液体模拟已完成。

`BackgroundChangeFlashInfo` 维护 13 个背景变体和闪光强度；`UpdateCache` 读取树、腐化、丛林、
雪、神圣、猩红、沙漠、海洋、蘑菇、地狱等 WorldGen 背景索引，`UpdateFlashValues` 每 tick 将
闪光以 0.05 衰减并钳制到 0..1（`BackgroundChangeFlashInfo.cs:5-40`）。WorldGen 在加载/生成
路径调用 `UpdateCache`，Main 每 tick 调用 `UpdateFlashValues`（`WorldGen.cs:7409`、
`Main.cs:11337`），但核心 `UpdateVariation` 为空，未发现闪光绘制消费者。因此背景变化仅有
输入采样和衰减，变化检测、触发强度与输出投影均为缺口。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `WaterfallVisualCacheComponent` | 瀑布位置、类型、停止步数、容量和贴图引用 | `WaterfallManager.cs:11-35`；`partial` |
| `WaterfallPreferenceAdapter` | 从客户端偏好加载瀑布数量/质量 | `BindTo`、`Configuration_OnLoad`；`partial/missing` |
| `WaterfallScanAndDrawProjection` | 从只读 Tile/Liquid 快照构建瀑布并绘制 | 当前无消费者；`missing` |
| `BackgroundVariationSnapshot` | 13 类背景选择的上一帧缓存 | `BackgroundChangeFlashInfo.cs:7-29`；`confirmed/partial` |
| `BackgroundChangeDetectionSystem` | 比较新旧变体并建立闪光强度 | `UpdateVariation`；`missing` |
| `BackgroundFlashDecaySystem` | 逐帧衰减、钳制和场景切换清理 | `UpdateFlashValues`；`confirmed/partial` |
| `BackgroundFlashProjection` | 将强度输入天空/背景绘制而不修改世界 | 当前无读取消费者；`missing` |

### 80.4 调用方向与验证 seam

```text
Weather + player snapshots -> StormEligibilityQuery + StormSafetyZoneSnapshot
  -> StormTargetSelectionQuery -> StormLightningSpawnCommand
  -> LightningPathGenerationQuery -> StormDamageProjectionSystem
  -> particle / visual projection

Local SceneMetrics -> AmbientEffectEligibilityQuery -> workspace/spawn queries
  -> AmbientWindCadenceSystem -> AmbientWindProjection
World background IDs -> BackgroundVariationSnapshot -> change detection -> flash decay
  -> background draw projection
```

最小 focused verifier 应覆盖：同一服务器 seed 和快照得到相同闪电路径；安全区、AFK、雪区、
睡眠/钓鱼倍率和 Tile 吸引集合的边界；空路径、碰撞路径和液体路径不会访问空数组；墓地进入/
退出时的环境效果清理；背景变体切换只触发表现闪光且强度不越界；瀑布偏好缺失、容量耗尽与
世界切换不泄漏贴图或旧坐标。当前无运行证据，均标记为“未验证”。

本轮新增约 **20 个**边界（雷暴资格/安全区/选择/命令/配置/路径/伤害/视觉 8，墓地风资格/
工作区/生成/节拍/投影 5，瀑布缓存/偏好/扫描绘制 3，背景快照/检测/衰减/投影 4）。累计
独立边界约 **528 个**；至少约 **450 个**仍有闪电落点/路径、环境生成、瀑布扫描、背景变体
检测、网络或表现消费链缺口。静态识别覆盖率约 **98%～99%**，权威行为闭合度约
**12%～21%**。本轮只读取 `D:\TRbackup\Version4`，未修改其源码，并继续排除
`D:\TRbackup\NLTX\src` 中已有组件。

### 附录 A 摘要：CLI 世界删除与世界会话清理

本轮发现已在“附录 A”完整记录：CLI 删除世界命令确实到达 `Main.EraseWorld`，但该方法为空体，
本地/云存储删除、活动世界保护、备份清理和列表刷新均未闭合。拆分为
`WorldSelectionComponent`、`WorldDeleteAuthorizationQuery`、`EraseWorldCommand`、
`WorldStorageAdapter` 与 `WorldListProjection`；核心提交状态为 `missing/partial`。

### 附录 B 摘要：暂停更新门控与退出生命周期

“附录 B”显示 `CanPauseGame` 恒返回 false、`DoUpdate_WhilePaused`、`QuitGame` 和
`Main_Exiting` 为空。应拆为 `PauseEligibilityQuery`、`PausedSessionComponent`、
`PausedUpdateSystem`、`QuitGameCommand`、`SessionShutdownAdapter` 和状态 Projection；
暂停/退出的权威提交顺序当前为 `missing`。

### 附录 C 摘要：世界文件元数据、种子选项与持久化迁移

“附录 C”完整记录了 `WorldFile` 版本化保存/加载主链及其缺口：
`EnableSeedOptions`、`TryParseSeedOptionValue`、`TryParseSecretSeed`、`MoveToCloud` 与
`MoveToLocal` 均为空或默认返回，且元数据异常被压成 null。建议拆为
`WorldPersistenceComponent`、`SeedOptionParseQuery`、`WorldSaveAdapter`、`WorldLoadAdapter`、
`CloudLocalMigrationCommand` 和 `MetadataFailurePolicy`，状态为 `confirmed/partial/missing`。

本轮新增约 **17 个**边界。累计独立边界约 **781 个**；仍有 `missing/partial` 的主要集中在
暂停/退出、世界删除、种子解析、云本地迁移、快照保存恢复、世界哈希、生成执行控制、地牢/轨道/生态
生成、事件提交、实体网络授权和光照传播。静态识别覆盖率约 **98%～99%**，权威行为闭合度约
**12%～21%**。本轮未重新展开用户已要求停止的特殊刷怪分支。

## 附录 A：CLI 世界删除与世界会话清理（待编入连续章节）

### 102.1 删除命令存在真实入口，但清理 Command 为空

服务器 CLI 在列出世界后解析 `DeleteWorld_Command`，校验索引并要求用户确认，随后直接调用
`Main.EraseWorld(num2)`（`Terraria/Main.cs:2270-2271`）。然而 `EraseWorld` 的实现是空体
（`Main.cs:1817`），没有读取 `WorldList` 条目、区分本地/云存储、删除 `.wld`/`.bak`/`.twld`
临时文件、更新列表，或处理活动世界与失败回滚。因此删除世界不是已闭合的文件 I/O，而是
一个可到达但无副作用的占位 Command。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `WorldSelectionComponent` | 世界列表条目、路径、云标志、活动状态 | CLI 使用 `WorldList[num2]`；`confirmed/partial` |
| `WorldDeleteAuthorizationQuery` | 索引范围、确认文本、活动世界保护 | `Main.cs:2250-2271`；`partial` |
| `EraseWorldCommand` | 删除世界主文件及关联备份/临时文件 | `Main.EraseWorld` 空体；`missing` |
| `WorldStorageAdapter` | 本地 `FileUtilities` 与 `SocialAPI.Cloud` 删除语义 | 云/本地列表存在，删除未接通；`partial/missing` |
| `WorldListProjection` | 删除成功后的排序、刷新和错误提示 | CLI 未见成功/失败结果投影；`partial` |

建议把删除拆成“解析并授权 → 活动世界保护 → 存储删除 → 列表刷新”四个阶段；Command
必须返回可判定的成功/失败结果，不能依靠 catch 后继续循环掩盖 I/O 错误。

## 附录 B：暂停更新门控与退出生命周期（待编入连续章节）

### 103.1 主循环有暂停 seam，但暂停 Query/Update/退出 Command 未实现

`Main.Update` 在 `CanPauseGame()` 为真时调用 `DoUpdate_WhilePaused()`、设置 `gamePaused`
并提前返回（`Terraria/Main.cs:11316-11320`）。当前 `CanPauseGame` 仅返回局部变量初始化值
`false`（约 `:11376-11383`），`DoUpdate_WhilePaused` 是空体（`:11385`），因此暂停状态下
输入、网络心跳、菜单时间、延迟过程和必要的 UI 投影没有权威实现。退出路径同样由
`QuitGame()`（`:11857`）和 `Main_Exiting`（`:11858`）占位；代码中存在退出入口，但没有保存、
断开网络、释放实体/图形资源或设置终止状态的可见提交顺序。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `PauseEligibilityQuery` | 菜单、联机、服务器、世界生成和输入焦点条件 | `CanPauseGame` 恒 false；`missing` |
| `PausedSessionComponent` | paused 标志、暂停时间基线、待处理输入/网络 tick | `Main.Update` 写 `gamePaused`；`partial` |
| `PausedUpdateSystem` | 暂停期间处理允许的输入、网络和安全 UI 状态 | `DoUpdate_WhilePaused` 空体；`missing` |
| `QuitGameCommand` | 记录退出意图、触发保存/断线/终止 | `QuitGame` 空体；`missing` |
| `SessionShutdownAdapter` | 进程、网络、存档和资源释放副作用 | `Main_Exiting` 空体；`missing` |
| `Pause/Quit Projection` | 向 UI/服务器报告暂停或退出状态 | 调用点存在但无提交；`partial/missing` |

推荐顺序是“资格 Query → 会话状态变换 → 暂停专用更新；退出则先保存快照，再断开同步，最后释放资源”。
不能把 `gamePaused = true` 当成暂停系统完成，也不能把 `QuitGame` 空体当成进程已经安全退出。

## 附录 C：世界文件元数据、种子选项与持久化迁移（待编入连续章节）

### 104.1 世界主序列化可达，但种子解析和存储迁移造成断链

`WorldFile.LoadWorld`/`SaveWorld`/`InternalSaveWorld` 实际连接文件头、Tile、箱子、NPC、TileEntity、
压力板、TownManager、Bestiary 和 Creative Powers（`Terraria.IO/WorldFile.cs:658-958,1186-1808,1808-3063`）。
不过 `WorldFileData.TryApplyingCopiedSeed` 调用的 `EnableSeedOptions`、`TryParseSeedOptionValue`、
`TryParseSecretSeed` 均为空或默认返回（`WorldFileData.cs:211-213,254-264`），复制种子和特殊种子不能可靠还原生成参数。

`WorldFileData.MoveToCloud`/`MoveToLocal` 为空（约 `:401-402`），而 `Main.LoadWorlds` 同时枚举本地与
`SocialAPI.Cloud` 世界，导致路径迁移和云/本地一致性未闭合；`GetFileMetadata` 异常时吞掉错误并返回 null
（`WorldFile.cs:3310-3345`），调用者必须区分无文件、格式过旧和 I/O 失败。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `WorldPersistenceComponent` | 世界 ID、尺寸、种子、模式、生成版本和元数据 | `WorldFileData` 与 `SetAsActive`；`confirmed/partial` |
| `SeedOptionParseQuery` | 用户种子、尺寸/模式/邪恶类型和 secret seed 解析 | 三个解析/启用方法默认实现；`missing` |
| `WorldSaveAdapter` | 版本化写入头、Tile、实体和尾部 | `SaveWorld_Version2` 主链真实存在；`confirmed/partial` |
| `WorldLoadAdapter` | 版本探测、恢复、校验和兼容修复 | `LoadWorld_Version2`、`ValidateWorld` 有实现；`confirmed/partial` |
| `CloudLocalMigrationCommand` | 本地↔云文件搬迁、冲突、回滚和活动世界保护 | `MoveToCloud/MoveToLocal` 空体；`missing` |
| `MetadataFailurePolicy` | null/损坏/未来版本/权限异常的可观察分类 | `GetFileMetadata` catch 后 null；`partial/missing` |

最小 verifier：普通和复合 secret seed 往返、非法种子拒绝、不同版本头、TileEntity/压力板/Town/Bestiary/
Creative Powers 保存恢复、云本地迁移冲突、损坏文件与未来版本错误分类。当前解析和迁移 seam 无运行 verifier，
相关能力标记为“未验证”。

本轮新增约 **17 个**边界（世界删除 5、暂停/退出 6、持久化与迁移 6）。累计独立边界约 **781 个**；仍有
`missing/partial` 的主要集中在暂停/退出、世界删除、种子解析、云本地迁移、世界快照保存恢复、世界哈希、
生成执行控制、地牢/轨道/生态生成、事件提交、实体网络授权和光照传播等。静态识别覆盖率约 **98%～99%**；
权威行为闭合度约 **12%～21%**。本轮仍只读 `D:\TRbackup\Version4`，未修改源码，也未重新展开已停止的特殊刷怪分支。

## 71. 第五十五轮：聊天命令路由、表情气泡与自定义货币资格链

### 79.1 ChatMessage/ChatCommandProcessor 是跨客户端的命令边界

`ChatMessage` 将文本和 `ChatCommandId` 绑定，普通消息默认指向 `SayChatCommand`，并保留
`IsConsumed` 消费位（`Terraria.Chat/ChatMessage.cs:5-30`）。`ChatCommandId.FromType<T>` 从
`ChatCommandAttribute` 生成稳定命令名；未知属性时返回空名称
（`ChatCommandId.cs:8-24`）。`ChatCommandProcessor` 维护本地化命令表、命令实例、别名和默认
命令；`AddCommand` 通过反射属性注册本地化键，`PrepareAliases` 让 `EmojiCommand` 等扩展别名
（`ChatCommandProcessor.cs:8-67`）。但 `CreateOutgoingMessage` 返回 `default`，
`ProcessIncomingMessage` 为空，所有命令的 `ProcessIncomingMessage/ProcessOutgoingMessage`
（死亡、PVP、BossDamage、Roll、Party、Help、Say、Emoji、Emote、RPS 等）也为空
（`Terraria.Chat.Commands/*.cs`）。因此这里有明确的协议命令目录，却没有完成参数解析、权限、
消费状态和副作用提交。

聊天显示路径本身是可见的投影：`ChatHelper.SendChatMessageToClientAs` 在服务器构造
`NetTextModule` 并发送到客户端，本地玩家则调用 `DisplayMessage`；后者写入玩家头顶聊天、
缓存或 `Main.NewTextMultiline`（`ChatHelper.cs:10-91`）。`CacheMessage` 与
`ShouldCacheMessage` 仍为空，故缓存生命周期和菜单/暂停期间的显示策略未闭合。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `ChatCommandCatalog` | Attribute 到命令 ID、本地化键和别名索引 | `ChatCommandProcessor.cs:16-53`；`confirmed/partial` |
| `ChatMessageComponent` | 文本、命令 ID、消费状态和来源会话 | `ChatMessage.cs:5-30`；`partial` |
| `ChatCommandParseQuery` | 从输入解析命令/参数并拒绝未知或歧义命令 | `CreateOutgoingMessage`、`ProcessIncomingMessage`；`missing` |
| `ChatCommandAuthorizationQuery` | 根据 clientId、服务器模式和命令类型判断权限 | 各命令接收入口；`missing` |
| `ChatCommandDispatchSystem` | 调用命令处理器、一次性消费和错误结果 | `IChatCommand` 实现为空；`missing` |
| `ChatNetworkAdapter` | NetTextModule 编解码、服务器广播和来源玩家 | `ChatHelper.cs:20-55`；`partial` |
| `ChatDisplayProjection` | 头顶聊天、缓存和多行文本输出 | `ChatHelper.DisplayMessage`；`partial` |

命令处理器只能产生显式游戏命令（例如表情或掷骰意图），不得直接把客户端显示文本当作
世界状态；服务器应重新验证玩家、目标和冷却后再提交玩法 System。

### 79.2 EmoteBubble 是实体锚定的社交表现状态，但包含 NPC 反应副作用

`WorldUIAnchor` 支持 Entity、Tile、Pos、None 四种锚点，并保存实体引用、世界位置和尺寸
（`Terraria.GameContent.UI/WorldUIAnchor.cs:5-45`）。`EmoteBubble` 以静态 `byID` 字典保存
气泡、ID 分配器和临时实例；`NewBubble`/`NewBubbleNPC` 创建生命周期、序列化锚点并发送
消息 91，`MessageBuffer` 可删除或更新同一 ID（`EmoteBubble.cs:10-114`、
`MessageBuffer.cs:2660-2694`）。`OnBubbleChange` 会缩短同一玩家旧气泡的寿命；
`CheckForNPCsToReactToEmoteBubble` 则遍历城镇 NPC，修改 ai、方向和 `netUpdate`，这是少数
由社交表现触发 NPC 行为的真实写者（`EmoteBubble.cs:116-132`）。

NPC 自动表情 `PickNPCEmote` 根据 Boss 存在、随机数、玩家环境、物品、事件、天气和其它 NPC
选择候选；但 `ProbeCombat/Weather/Events/Debuffs/Items/TownNPCs/Biomes/Critters/Emotions/
Bosses/Exceptions` 全为空（`EmoteBubble.cs:169-229`）。所以“表情候选资格”不能视为完整
生态或情绪模拟，当前最多是表现目录和随机选择壳。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `WorldUIAnchorComponent` | 实体/Tile/位置锚点及尺寸快照 | `WorldUIAnchor.cs:5-45`；`confirmed` |
| `EmoteBubbleComponent` | ID、表情、metadata、寿命和动画帧 | `EmoteBubble.cs:10-39,135-141`；`confirmed/partial` |
| `EmoteEligibilityQuery` | 从战斗、天气、事件、玩家环境产生候选表情 | `PickNPCEmote` 与 Probe 方法；`missing` |
| `EmoteReactionSystem` | 玩家表情引发 NPC ai/方向反应并标记网络更新 | `CheckForNPCsToReactToEmoteBubble`；`partial` |
| `EmoteBubbleLifecycleSystem` | 同玩家旧气泡过期、寿命递减、清理 | `OnBubbleChange`、`toClean`；`partial/missing` |
| `EmoteNetworkAdapter` | 锚点类型/索引、消息 91 的范围校验与同步 | `SerializeNetAnchor`、`DeserializeNetAnchor`、`MessageBuffer.cs:2660-2694`；`partial` |
| `EmoteDrawProjection` | 根据锚点把表情帧绘制到世界/UI | `WorldUIAnchor` 字段；当前无 Draw 消费者；`missing` |

网络接收端必须检查实体索引、锚点类型和寿命上限；反应 System 只允许写入 NPC 的明确反应
意图，不能由客户端直接注入任意 `ai` 值。

### 79.3 CustomCurrency 是商店经济的目录/资格接口，实际扣款链未实现

`CustomCurrencyManager.Initialize` 注册 Defender Medals 货币，维护递增 ID 和
`Dictionary<int, CustomCurrencySystem>`；`IsCustomCurrency` 遍历注册系统调用 `Accepts`
（`CustomCurrencyManager.cs:8-43`）。`CustomCurrencySystem` 保存物品到单位价值的映射和
货币上限，声明 `CountCurrency`、`CombineStacks`、`TryPurchasing`、`Accepts`、价格文本和
预期买卖价等接口，但除 `Include`/`SetCurrencyCap` 外均返回默认值
（`CustomCurrencySystem.cs:8-55`）。`CustomCurrencySingleCoin` 只在构造器登记一个硬币
物品和上限，其购买、储蓄绘制和价格文本覆盖仍为空（`CustomCurrencySingleCoin.cs:12-39`）。
Version4 中未发现商店实际调用这些扣款方法的闭合路径，因此不能把“Defender Medals 已注册”
误报为经济系统完成。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `CurrencyDefinitionCatalog` | 货币 ID、可接受物品、单位价值和上限 | `CustomCurrencyManager.cs:12-29`、`CustomCurrencySystem.cs:10-25`；`confirmed` |
| `CurrencyBalanceQuery` | 从多个库存槽统计余额、忽略槽位并报告溢出 | `CountCurrency`、`CombineStacks`；`missing` |
| `CurrencyPurchaseQualificationQuery` | 价格、余额、库存空间和商店条件检查 | `TryPurchasing` 参数列表；`missing` |
| `CurrencyPurchaseCommitSystem` | 原子扣款、找零/合并堆叠和失败回滚 | `CustomCurrencySingleCoin.TryPurchasing`；`missing` |
| `CurrencyPriceProjection` | 本地化价格、储蓄金额和买卖价展示 | `GetPriceText`、`DrawSavingsMoney`、`GetItemExpectedPrice`；`missing` |
| `CurrencyNetworkAdapter` | 商店请求、服务器重算和扣款结果同步 | 当前未发现自定义货币专用网络包；`missing` |

该经济边界必须复用既有库存/商店权威状态，但不能把 UI 中的价格字符串或客户端余额作为
消费依据；货币扣款应与物品转移处于同一原子提交内。

### 79.4 调用方向、缺口与增量统计

```text
Client text -> ChatCommandParseQuery -> Authorization -> DispatchSystem
  -> Gameplay command (Emote/Roll/Death/etc.) -> NetText/Chat projection
NPC/player event -> EmoteEligibilityQuery -> EmoteBubbleComponent
  -> EmoteReactionSystem -> NPC intent + EmoteNetworkAdapter -> DrawProjection
Shop definition -> CurrencyDefinitionCatalog -> Balance/PurchaseQualification
  -> CurrencyPurchaseCommitSystem -> inventory mutation + price projection
```

最小 focused verifier 应覆盖：未知命令和别名冲突、消息重复消费、clientId 越权、NetText
来源伪造；气泡锚点索引越界、寿命上限、同玩家替换和 NPC 反应只在服务器发生；货币单位
价值溢出、重复物品扣款、库存空间不足回滚和自定义货币未注册时的拒绝。当前没有这些
verifier 的运行证据，均标记为“未验证”。

本轮新增约 **19 个**边界（聊天目录/消息/解析/授权/分发/网络/显示 7，锚点/气泡/资格/
反应/生命周期/网络/绘制 7，货币目录/余额/购买资格/提交/价格/网络 6，另含一个跨域原子
提交 seam）。累计独立边界约 **508 个**；至少约 **430 个**仍有命令处理、Probe 候选、
气泡生命周期、货币统计与扣款、网络权限或 UI 消费链缺口。静态识别覆盖率约 **98%～99%**，
权威行为闭合度约 **12%～21%**。本轮只读取 `D:\TRbackup\Version4`，未修改其源码，并继续
排除 `D:\TRbackup\NLTX\src` 中已有组件。

## 72. 第五十一轮：玩家移动能力快照、便携凳偏移、翅膀能力与护甲套装派生

### 70.1 坐骑覆盖下的移动能力快照是临时事务，而不是玩家持久组件

`PlayerMovementAccsCache` 保存坐骑可能覆盖的飞行与额外跳跃状态：`rocketTime`、
`rocketDelay`、`rocketDelay2`、`wingTime`，以及 Cloud、Sandstorm、Blizzard、Fart、Sail、
Unicorn 六组 `canJumpAgain` 标志；另外记录 `mount.CanUseWings` 和 `mount.BlockExtraJumps`
在缓存时的反向值（`Terraria.DataStructures/PlayerMovementAccsCache.cs:3-29`）。`CopyFrom`
仅在 `_readyToPaste=false` 时建立一次快照，`PasteInto` 只在 ready 时恢复，并在恢复后清除
ready 标志（`:31-78`）。

唯一调用链位于玩家逐帧更新：坐骑激活时调用 `CopyFrom(this)`，否则调用 `PasteInto(this)`；
随后若坐骑阻止额外跳跃，直接把 Cloud、Sandstorm、Blizzard、Fart、Sail、Unicorn、Santank、
WallOfFleshGoat、Basilisk 全部置为 false（`Terraria/Player.cs:15636-15657`）。因此快照并
不是完整的“所有移动能力”快照：它没有保存后三种坐骑跳跃标志，也没有保存 `wings`、
`wingsLogic`、`wingTimeMax` 或其他移动派生值。`_mountPreventedFlight` 为 false 时即使缓存过
火箭/翅膀计时也不会恢复，这个条件依赖坐骑定义且没有独立验证。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `PlayerMovementCapabilitySnapshot` | 保存坐骑覆盖前、且仅限一次事务的飞行计时和额外跳跃快照 | `PlayerMovementAccsCache.cs:5-29`；`confirmed`（临时快照） |
| `MountMovementOverrideQuery` | 判断坐骑是否禁止翅膀或额外跳跃，以及是否需要回滚 | `PlayerMovementAccsCache.cs:38-39`；`partial` |
| `MovementAccessoryRestoreSystem` | 坐骑进入/退出边沿的 Copy/Paste 与条件恢复 | `Player.cs:15638-15657`；`partial`，缺少边沿/重复退出验证 |
| `ExtraJumpCapabilityComponent` | 维护全部额外跳跃资格、可再次使用状态及坐骑屏蔽关系 | `Player.cs:15646-15657`；`partial`，缓存字段未覆盖全部能力 |

调用方向应为：

```text
Mount state -> MountMovementOverrideQuery
           -> MovementAccessoryRestoreSystem
           -> PlayerMovementCapabilitySnapshot / ExtraJumpCapabilityComponent
           -> jump and flight systems
```

不能把该 cache 当作存档或网络组件。它没有序列化字段、没有版本号，也没有在玩家死亡、断线
或异常退出时的清理入口；`PasteInto` 的恢复条件和缺少的后三个跳跃字段必须作为未闭合风险保留。

### 70.2 便携凳同时影响权威碰撞、跳跃和视觉/地图投影

`PortableStoolUsage` 只有五个字段：是否拥有凳子、是否正在使用、高度提升、视觉 Y 偏移和
地图 Y 偏移（`Terraria.DataStructures/PortableStoolUsage.cs:3-13`）。`SetStats` 目前只在
`Player.ApplyEquipFunctional` 处理物品 4341/5126 时写入 `(26,26,26)`（`Terraria/Player.cs:8259-8266`），
`ResetEffects` 每个玩家更新周期调用 `Reset`，并随后调用 `ResizeHitbox`
（`Player.cs:10747-10749`）。

`UpdatePortableStoolUsage` 只允许同时满足拥有凳子、按上、非重力控制、非坐骑、水平/垂直速度
为零、非滑轮且未抓钩时使用，并通过 `CanFitSpace(HeightBoost)` 检查空间；成功后设
`IsInUse=true` 并重设碰撞箱（`Player.cs:17930-17943`）。`HeightOffsetHitboxCenter` 使用
`HeightBoost-VisualYOffset`，`HeightOffsetBoost` 使用 `HeightBoost`，`BaseHeight`、
`MountedCenter` 和 `ResizeHitbox` 随之改变（`Player.cs:2507-2575`）。跳跃开始时会清除
`IsInUse` 并把位置 Y 下移 `HeightBoost`（`Player.cs:12297-12301`）；凳子状态还会阻止
`FindPulley`（`Player.cs:11355-11377`），并在 `UpdateJumpHeight` 中额外增加 5 点跳跃高度
（`Player.cs:6460-6483`）。

`MapYOffset` 在 `Version4` 中只有声明和写入，未发现消费者；它不能据此宣称地图投影已闭合。
同时 `UpdatePortableStoolUsage` 成功路径之外没有显式 `IsInUse=false`，实际清理依靠每帧
`ResetEffects` 或跳跃分支，属于隐式生命周期。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `PortableStoolCapabilityComponent` | 拥有凳子及三种偏移定义 | `PortableStoolUsage.cs:5-13`、`Player.cs:8265`；`confirmed` |
| `StoolUseQualificationQuery` | 按输入、速度、坐骑、抓钩和空间判断是否可坐 | `Player.cs:17934-17938`；`confirmed` |
| `StoolUsageMutationSystem` | 开始使用、退出使用、碰撞箱重建及跳跃解除 | `Player.cs:17939-17943,12297-12301`；`partial` |
| `PlayerHeightProjection` | 将凳子/坐骑偏移投影到碰撞中心、基础高度和地图/视觉坐标 | `Player.cs:2507-2575`；`partial`，`MapYOffset` 无消费者 |

`ResizeHitbox` 与坐骑 `OverrideSizeMethod` 共用写集，顺序必须显式约束为“清理旧偏移 → 计算
凳子/坐骑偏移 → 原子调整 position/width/height”，否则重复应用会漂移。该边界不应并入通用
`PlayerMovementComponent`，因为凳子退出还会触发碰撞和跳跃结构变化。

### 70.3 翅膀能力目录、装备选择和空中移动系统是三层边界

`WingStats` 定义飞行时间、陆地加速速度覆盖、加速度倍率、下压悬停开关及其速度/加速度倍率
（`Terraria.DataStructures/WingStats.cs:3-27`）。`WingStatsInitializer.Load` 创建长度为
`ArmorIDs.Wing.Count` 的数组，按翅膀 ID 写入 25/100/130/150/160/170/180 等飞行时间和
3～9 的速度档位，并为 22、30、31、37、45 等翅膀设置下压悬停参数
（`Terraria.Initializers/WingStatsInitializer.cs:8-64`）；启动时由 `Main` 调用
`WingStatsInitializer.Load()`（`Terraria/Main.cs:3337`）。

这里有两个重要缺口：第 56 行构造了一个 `new WingStats(...)` 却没有赋给数组元素，形成
“定义被丢弃”的空项；数组默认项是全零结构，而 `WingStats` 构造器默认值本应是
`FlyTime=100, AccRunSpeedOverride=-1, AccRunAccelerationMult=1`，所以未注册 ID 得到的
`default(WingStats)` 与构造器默认语义不一致。`Player.GetWingStats` 只做 ID 范围检查后返回
数组项，越界返回默认结构（`Player.cs:8103-8112`），没有未初始化项或数值范围校验。

装备更新从可用装备槽扫描 `wingSlot`，分别写入表现用 `wings` 和逻辑用 `wingsLogic`
（`Player.cs:6957-6966`）。`WingAirLogicTweaks` 依据 `TryingToHoverDown`、跳跃输入、
剩余 `wingTime` 和当前翅膀统计覆盖 `accRunSpeed`、`runAcceleration`，并为翅膀 45 修改
`runSlowdown`（`Player.cs:17996-18022`）。`GetWingsFunctionalityForVisuals` 则把可悬停、
水平悬停加速和 boost 能力投影到客户端表现（`Player.cs:18025-18032`）；这些布尔值不能反向
写入权威飞行状态。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `WingCapabilityDefinitionCatalog` | 翅膀 ID 到飞行/悬停/加速参数的只读目录 | `WingStatsInitializer.cs:12-64`；`partial`，存在丢弃构造和默认项问题 |
| `WingStatsQuery` | 按 ID 获取安全的能力定义并处理未知/未初始化项 | `Player.GetWingStats:8103-8112`；`partial` |
| `EquippedWingSelectionSystem` | 从可用装备槽选择逻辑翅膀与表现翅膀 | `Player.cs:6957-6966`；`confirmed` |
| `WingAirMovementSystem` | 将翅膀能力应用于空中速度、悬停和 boost | `Player.cs:17996-18022`；`confirmed/partial` |
| `WingVisualCapabilityProjection` | 输出客户端悬停/水平 boost/翅膀 boost 布尔视图 | `Player.cs:18025-18032`；`confirmed` |

应在启动调度中先完成 `WingCapabilityDefinitionCatalog`，再执行装备选择和空中移动；当前
`WingStatsInitializer.Load` 没有公开完成标志、重复加载保护或数组逐项验证，故权威飞行行为仍
只能评为 `partial`。

### 70.4 护甲套装资格查询与派生效果应用不应继续共用 Player 巨型写集

`ArmorSetBonus.QueryContext` 从 `player.armor[0..2]` 提取头、身、腿 Item ID；`QueryCount`
只统计非零部位的需要数和匹配数，`Complete` 为两者相等
（`Terraria.DataStructures/ArmorSetBonus.cs:18-49,168-191`）。`Builder.Set` 支持单组合和
选项笛卡尔积，`Add` 把组合、效果委托、本地化描述和 PrimaryPart 写入静态
`ArmorSetBonuses.All`（`:55-143`）。`Initialize` 注册木、金属、骨、Beetle、Wizard、
Chlorophyte、Hallowed、Tiki、Moon 系列及 DD2 等大量组合（`ArmorSetBonuses.cs:516-614`）；
`BuildLookup` 按头/身/腿 Item ID 构建 `SetsContaining`，`GetCompleteSet` 先查头部候选再查身体
候选并返回首个完整匹配（`ArmorSetBonuses.cs:616-664`）。

玩家更新顺序先在 `ResetEffects` 将 `maxMinions`/`maxTurrets`、伤害倍率、暴击、耐性、套装
标志、Buff 资格和大量其他派生字段重置为基础值（`Player.cs:10272-10749`），再由
`UpdateEquips` 对可用装备槽执行 `GrantPrefixBenefits` 与 `GrantArmorBenefits`
（`Player.cs:6894-6904`），最后 `UpdateArmorSets` 查询完整套装并调用效果委托，再运行
Beetle、Solar、Stardust、Chlorophyte、Vortex 的 always 清理和声音/尘埃逻辑
（`Player.cs:15296-15360,9547-9558`）。这证明套装效果是逐帧重算的派生状态，而非装备变更
时一次性持久化结果。

`ArmorSetBonuses.Benefits` 的写集跨越多个子系统：普通效果修改防御、伤害、暴击、法力、
移速、召唤槽、鞭范围和击退；Solar/Beetle 维护计数、护盾、轨道位置、Buff、尘埃和光效；
Nebula 递减冷却；Forbidden 调用锁定逻辑并直接 `Lighting.AddLight`；Bee 调用成就；
Chlorophyte/Solar 调用 `AddBuff`；Hallowed/Titanium 等写入命中免疫或 Proc 标志。
例如 `Benefits.Tiki/Spooky/Bee/Solar/Nebula/Forbidden` 位于
`ArmorSetBonuses.cs:12-84`，`ApplySetBonus_Solar` 还会每 180 tick 生成护盾 Buff、尘埃并
更新护盾速度（`Player.cs:9607-9643`）。因此不能把 `ArmorSetEffect(Player)` 当作单一
“套装组件”；它是混合的命令入口。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `ArmorSetDefinitionCatalog` | 套装三部位组合、PrimaryPart、描述键和效果标识 | `ArmorSetBonus.Builder`、`ArmorSetBonuses.Initialize`；`confirmed` |
| `ArmorSetQualificationQuery` | 根据头/身/腿纯判断完整套装及候选优先级 | `QueryContext/QueryCount/GetCompleteSet`；`confirmed/partial`，缺少重复组合与初始化校验 |
| `ArmorDerivedStatsComponent` | 保存重算后的防御、伤害、暴击、召唤槽、移速、耐性和套装标志 | `Player.ResetEffects`、`GrantArmorBenefits`、`Benefits.*`；`confirmed` |
| `ArmorSetDerivedStatsSystem` | 在装备授予后应用纯派生修改并执行 always 清理 | `Player.cs:6894-6904,9547-9605`；`partial`，写集与 Buff/计数耦合 |
| `ArmorSetCombatCommand` | 触发 Solar/Beetle/Forbidden 等需要命中、护盾、Proc 或冷却的权威意图 | `ApplySetBonus_Solar/Beetle*`、`UpdateForbiddenSetLock`；`partial` |
| `ArmorSetBuffAndAchievementAdapter` | 隔离 AddBuff、Lighting、Dust、Sound 和 AchievementsHelper | `ArmorSetBonuses.cs:27-35,77-84`、`Player.cs:9617-9639`；`partial` |
| `ArmorSetDescriptionProjection` | 将 Description、PrimaryPart 和装备部位提示输出到 UI | `ArmorSetBonus.cs:145-166`；`missing`，未找到实际显示消费链 |

关键调度约束为：

```text
ResetDerivedStats
  -> UpdateEquips (单件护甲/前缀)
  -> ArmorSetQualificationQuery
  -> ArmorSetDerivedStatsSystem
  -> ArmorSetCombatCommand / BuffAndAchievementAdapter
  -> always-cleanup systems (Solar/Beetle/Stardust/Chlorophyte/Vortex)
```

`SetsContaining` 是静态进程级索引，不应成为实体状态；初始化前调用 `GetCompleteSet`、
重复 `Initialize`、Item ID 越界、多个组合同时完整及 `All` 被外部修改都没有保护。旧套装派生
字段依靠 `ResetEffects` 清除，若未来改成装备变更事件，必须保留同等的失效和回滚语义。

### 70.5 房间检查反馈与物品动画注册是未闭合的两个投影边界

`NoRoomCheckFeedback` 同时实现 `IRoomCheckFeedback`、Spread 和 Scoring 接口，提供
`WithText`/`WithoutText` 两个单例并固定 `StopOnFail=true`、`DisplayText`；但
`BeginSpread/EndSpread`、`BeginScoring/EndScoring` 以及所有错误/评分回调均为空体
（`Terraria.DataStructures/NoRoomCheckFeedback.cs:3-95`）。`WorldGen.StartRoomCheck` 和
`WorldFile` 确实把它作为房屋/房间检查反馈传入（`Terraria/WorldGen.cs:5516,5705`、
`Terraria.IO/WorldFile.cs:1938-1945`），所以它是一个“主动丢弃诊断”的投影，而不是房屋
资格系统本身。建议拆为 `RoomCheckFeedbackProjection`（`partial`）和
`RoomCheckDiagnosticEventAdapter`（`missing`）；房间合法性仍归 `RoomQualificationQuery`，
不能由反馈对象反向改变结果。

`Main.itemAnimations` 是按 Item ID 索引的 `DrawAnimation` 注册表；启动注册了多组
`DrawAnimationVertical` 和 `DrawAnimationScryingOrb`（`Terraria/Main.cs:954,1546-1604`）。
但是基类 `DrawAnimation.Update/GetFrame`、垂直动画和 ScryingOrb 覆盖都为空或返回空
`Rectangle`（`Terraria.DataStructures/DrawAnimation.cs:6-16`、
`DrawAnimationVertical.cs:6-25`、`DrawAnimationScryingOrb.cs:6-13`）。因此目录注册是
`confirmed`，帧推进和纹理帧投影为 `missing`，不能把它当作可运行的物品动画系统。建议拆为
`ItemAnimationDefinitionCatalog`、`ItemAnimationUpdateSystem`（`missing`）和
`ItemAnimationFrameProjection`（`missing`），并将动画状态保持在表现实体，不写回 Item 的
权威库存字段。

### 70.6 本轮新增边界与统计

本轮新增约 **20 个**边界（移动快照/坐骑覆盖/额外跳跃、便携凳资格/碰撞/高度投影、翅膀
目录/查询/装备选择/空中移动/视觉投影、套装目录/资格/派生统计/战斗命令/副作用适配/描述
投影、房间反馈和物品动画目录/更新/帧投影）。以报告上一节登记的约 409 个为基线，累计独立
边界约 **429 个**；至少约 **349 个**仍有缓存字段不完整、翅膀初始化丢项、套装副作用拆分、
房间诊断消费和动画帧推进缺口。静态识别覆盖率约 **98%～99%**，权威行为闭合度约
**15%～25%**。这些是基于源码证据的工程估计，不代表 Version4 已形成完整可运行模拟。
本轮只读取 `D:\TRbackup\Version4`，未修改其源码，并继续排除 `D:\TRbackup\NLTX\src` 中
已有组件。

## 73. 第五十二轮：配方/制作请求、快速堆叠、紧急转移与装备套装切换

### 71.1 Recipe/RecipeGroup 只有数据形状，制作权威链仍未展开

`RecipeGroup` 保存本地化名称函数、`ValidItems` 集合、`Items` 列表、解构物品 ID 和静态
`recipeGroups` 注册表（`Terraria/RecipeGroup.cs:9-27`）。构造函数把传入物品加入集合，
`Add` 同时写入 `HashSet` 和 `List`，但 `isPreferred` 参数完全未消费；`RegisteredId` 初始
为 -1，`GetGroupFakeItemId` 直接以 `RegisteredId + 1_000_000` 生成虚拟物品 ID，`ToString`
为空实现（`:29-70`）。因此配方组的注册分配、重复物品、首选材料和稳定 ID 还没有闭合。

`Recipe.RequiredItemEntry` 允许真实 Item ID 或 `RecipeGroup` 虚拟 ID，`RecipeGroup` 属性
直接索引静态字典；`Recipe` 记录创建物品、最多 15 项材料、必需 Tile、接受组、快速查找项、
水/岩浆/蜂蜜/雪地/墓地/Mechdusa 条件、炼金和不可逆制作标志
（`Terraria/Recipe.cs:12-87`）。但 `Recipe` 当前只有构造器，`ToString` 返回默认值，未发现
配方注册、资格计算、材料锁定、消耗、生成结果或解构提交实现；`Main` 中原本的
`SetupRecipeGroups`、`SetupRecipes` 初始化调用也被注释（`Terraria/Main.cs:3401-3415`）。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `RecipeGroupDefinitionCatalog` | 组名称、合法材料、首选项和稳定组 ID | `RecipeGroup.cs:9-70`；`partial`，ID 分配/首选逻辑缺失 |
| `RecipeDefinitionComponent` | 结果、材料、必需 Tile、环境条件及解构标志 | `Recipe.cs:42-87`；`confirmed`（数据形状） |
| `RecipeQualificationQuery` | 从权威库存、附近制作站和世界条件计算是否可制作 | `Recipe.cs:47-84`；`missing` |
| `RecipeMaterialReservationCommand` | 原子锁定材料/组材料，拒绝负数量和重复消费 | 当前无提交入口；`missing` |
| `CraftingCommitSystem` | 验证后消费材料并生成结果/自定义 Shimmer 结果 | 当前无实现；`missing` |
| `CraftingProjection` | 将可制作列表、消耗预览和失败原因输出到 UI/网络 | `CraftingRequests.cs:10-22`；`missing` |

不能因为 `Recipe` 字段已经定义就宣称制作系统存在。尤其 `RequiredItemEntry.RecipeGroup`
对未知或越界虚拟 ID 没有安全分支；配方初始化被注释意味着 `recipeGroups` 可能为空，制作
请求必须在服务器从内容目录和玩家/箱体状态重新计算，而不能信任网络携带的 `recipe`、
`result` 或 `consumed`。

### 71.2 远程制作请求是网络意图队列，但当前只有门禁字段

`CraftingRequests.RemoteCraftRequest` 把 `Recipe`、客户端结果 `Item result`、已消费列表
`consumed`、请求材料 `requested` 和 `quickCraft` 聚合在一个结构中
（`Terraria.GameContent/CraftingRequests.cs:9-22`）。静态 `_pendingCrafts` 队列只暴露
`HasPendingRequests`；`NetCraftingRequestsModule.Deserialize` 直接返回 `new bool()`，没有
读取、长度限制、发送者校验、入队或消费（`:24-34`）。`Main.PendingInventoryActions` 只把
该布尔值作为库存动作门禁（`Terraria/Main.cs:11802`），因此“有待处理请求”并不等于请求
已经进入制作事务。

`NetworkInitializer` 注册了该模块（`Terraria.Initializers/NetworkInitializer.cs:25`），
但未发现对应的服务器处理器或 `Dequeue` 调用。建议将其建模为：

```text
ClientCraftIntentAdapter
  -> CraftRequestValidationQuery (player, recipe id, station, conditions)
  -> CraftMaterialReservationCommand
  -> CraftingCommitSystem
  -> Inventory/WorldItem result projection
```

`result`/`consumed` 只能是客户端预测或诊断字段；服务器必须从 `RecipeDefinitionCatalog` 和
权威容器重新计算。focused verifier 应覆盖空包、超长材料列表、未知 RecipeGroup、负堆叠、
重复 request、断线重放和提交失败回滚；当前均为 `未验证`。

### 71.3 QuickStacking 是跨背包/箱体的结构事务，关键算法为空体

`QuickStacking` 预留了目的地缓存 `DestinationHelper`、按物品类型索引的
`MatchingItemTypeDestinationList`、来源库存 `SourceInventory`、阻塞箱列表和两个 scratch
结构（`Terraria.GameContent/QuickStacking.cs:11-78`）。但 `QuickStackToNearbyChests`、
`ReadNetInventory` 和 `WriteBlockedChestList` 均为空实现（`:80-86`）；`MatchingItemType...Reset`
只清空数组，未发现目的地扫描、距离排序、锁定、容量计算、堆叠提交或回滚。

消息类型 121/对应远程处理会调用 `QuickStackToNearbyChests(player, inventory, smartStack)`
（`Terraria/MessageBuffer.cs:2538-2543`），并由 `NetMessage` 回传阻塞箱列表，但由于读取和
写回方法为空，网络请求无法证明服务器端完成了快速堆叠。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `QuickStackSourceSnapshot` | 玩家可转移槽位、SlotReference、位置和传输阻塞标志 | `QuickStacking.cs:64-78`；`partial` |
| `NearbyContainerQuery` | 按距离、锁定、权限和物品类型筛选目的地 | `DestinationHelper`、`Main`/Chest 调用；`missing` |
| `QuickStackPlanQuery` | 纯计算每个来源到目的地的可堆叠数量和顺序 | scratch 类型声明；`missing` |
| `QuickStackCommitSystem` | 原子修改 Player/Chest/WorldItem 槽位并处理容量失败 | `QuickStackToNearbyChests`；`missing` |
| `QuickStackNetworkAdapter` | 读取来源库存、校验槽位和回传阻塞箱 DTO | `ReadNetInventory/WriteBlockedChestList`；`missing` |

快速堆叠不能与 `EmergencyStacking` 合并：前者是玩家主动选择附近箱，后者是世界掉落槽位
不足时的自动保全转移；两者的触发原因、所有权和失败策略不同。

### 71.4 EmergencyStacking 已有排序/所有权状态，但转移实现仍不闭合

`EmergencyStacking.Group` 以谓词组成 Fallen Stars、铜/银币、装备、稀有货币和 Default
分组，并为每组提供距离步长；`PreservationOrder` 定义稀有货币→装备→银币→铜币→星星→
默认的保全顺序（`Terraria.GameContent/EmergencyStacking.cs:9-99`）。`Transfer` 保存源/目标
WorldItem、距离排序、保全排序和距离，并通过 `HasOwnership`、`NumToTransfer` 计算所有权与
可转移数量（`:101-149`）。

触发链是明确的：Item 新建世界物品时若槽位接近上限，调用
`EmergencyStackItemsToMakeSpace`；主循环每 tick 调用 `ProcessPendingTransfers`
（`Terraria/Item.cs:48822-48829`、`Terraria/Main.cs:12781`），WorldItem 更新和销毁还会检查
`HasPendingTransferInvolving/ClearPendingTransfersInvolving`
（`Terraria/WorldItem.cs:272`）。然而 `Transfer.CompareTo`、`UpdateDestinationFromPreviousTransfers`、
`MemoStackableItems`、`FindBestTransfers`、`DoTransfer` 和
`RequestOwnershipReleaseForPendingTransfers` 为空或默认返回（`EmergencyStacking.cs:145-229`）。
所以当前只能确认“保全策略和 pending 标记的状态模型”，不能确认实际转移、槽位释放、所有权
释放或并发安全。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `EmergencyStackPolicyCatalog` | 分组谓词、距离步长和保全优先级 | `EmergencyStacking.cs:9-99`；`confirmed` |
| `WorldItemStackCandidateQuery` | 收集可堆叠世界物品及屏幕/距离/年龄信息 | `StackableItem`、`MemoStackableItems`；`missing` |
| `EmergencyTransferPlanQuery` | 计算排序、数量、来源/目标和每批上限 | `Transfer`、`FindBestTransfers`；`partial` |
| `EmergencyTransferCommitSystem` | 修改两个 WorldItem、更新槽位和 pending 标记 | `ProcessPendingTransfers/DoTransfer`；`missing` |
| `WorldItemOwnershipReleaseCommand` | 在转移完成或失败时释放保留者并广播 | `RequestOwnershipReleaseForPendingTransfers`；`missing` |

`HasPendingTransfer` 使用固定 401 长度数组并直接以 `whoAmI` 索引，没有范围校验；
`Transfer.NumToTransfer` 依赖 `Item.CanStack` 和目标 `maxStack`，需要在提交阶段再次验证，
否则目的地在计划后变化会造成负容量或物品丢失。

### 71.5 装备套装切换是已存在的结构变更与网络投影边界

`EquipmentLoadout` 为三个玩家套装槽各自保存 20 个 `Armor`、10 个 `Dye` 和 10 个
`Hide` 标志；构造器为每个槽位创建独立 `Item` 对象，`Swap` 与玩家当前 `armor/dye/
hideVisibleAccessory` 数组逐项交换（`Terraria/EquipmentLoadout.cs:6-53`）。
`Player.TrySwitchingLoadout` 在玩家不处于使用物品、控制、死亡状态且索引合法时，先后交换当前
与目标套装并更新 `CurrentLoadoutIndex`（`Terraria/Player.cs:3797-3808`）。装备效果不会在
此处直接重算，而是依赖下一次逐帧 `ResetEffects → UpdateEquips → UpdateArmorSets`，因此
切换命令与派生统计系统必须保持明确的先后关系。

网络侧 `PlayerItemSlotID` 为三个 Loadout 的 Armor/Dye 分配独立槽位，并以 `CanRelay=true`
标记（`Terraria.ID/PlayerItemSlotID.cs:162-200`）；`NetMessage.SyncOnePlayer` 发送当前索引
及三套完整 Armor/Dye 数组（`Terraria/NetMessage.cs:2640-2677`），消息 147 接收方强制使用
`whoAmI` 作为玩家索引，读取新索引和可见性并转发（`Terraria/MessageBuffer.cs:3238-3255`）。
但 `EquipmentLoadout.FixLoadedData` 为空，死亡时 `TryDroppingItems` 可能把三个备用套装全部
转成世界掉落，而网络槽位边界、重复切换、断线中途交换和投影失败回滚未闭合。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `EquipmentLoadoutComponent` | 单套 Armor/Dye/Hide 数据及槽位生命周期 | `EquipmentLoadout.cs:6-20`；`confirmed` |
| `LoadoutSwitchQualificationQuery` | 索引、玩家状态和动作门禁判断 | `Player.cs:3797-3802`；`confirmed` |
| `LoadoutSwapCommand` | 原子交换当前装备与目标套装并更新索引 | `EquipmentLoadout.Swap`、`TrySwitchingLoadout`；`partial` |
| `LoadoutDerivedStatsRebuildSystem` | 切换后触发装备、前缀和套装派生重算 | `Player.cs:15296-15360`；`partial` |
| `LoadoutNetworkAdapter` | 147 消息、槽位路由和 Armor/Dye 投影 | `PlayerItemSlotID.cs:52-80`、`NetMessage.cs:2640-2677`；`partial` |
| `LoadoutDeathDropCommand` | 死亡时处理当前与备用套装掉落 | `Player.cs:26355-26358`；`partial` |

### 71.6 本轮调用方向与验证计划

```text
RecipeDefinitionCatalog + RecipeGroupDefinitionCatalog
  -> RecipeQualificationQuery
  -> CraftIntentAdapter / MaterialReservationCommand
  -> CraftingCommitSystem -> inventory/container projection

Player/Chest/WorldItem -> QuickStackSourceSnapshot -> NearbyContainerQuery
  -> QuickStackPlanQuery -> QuickStackCommitSystem -> blocked-chest projection

EmergencyStackPolicyCatalog -> WorldItemStackCandidateQuery
  -> EmergencyTransferPlanQuery -> EmergencyTransferCommitSystem
  -> OwnershipReleaseCommand

LoadoutSwitchIntent -> LoadoutSwitchQualificationQuery -> LoadoutSwapCommand
  -> LoadoutDerivedStatsRebuildSystem -> network/death-drop projections
```

最小 focused verifier 应覆盖：RecipeGroup 虚拟 ID/重复材料/组首选项；制作请求重放、截断、
服务器重算和失败回滚；快速堆叠的锁定箱、距离、权限、堆叠上限和阻塞回传；紧急转移的槽位
重用、所有权释放、计划后目标变化和 `whoAmI` 越界；套装切换期间的动作门禁、三组数组独立性、
网络槽位路由和死亡掉落顺序。当前没有这些 verifier 的运行证据，均标记为 `未验证`。

### 71.7 本轮新增边界与统计

本轮新增约 **24 个**边界（配方/配方组目录与资格、材料预留/制作提交、远程制作意图与投影、
快速堆叠快照/目的地/计划/提交/网络适配、紧急堆叠策略/候选/计划/提交/所有权释放、装备
套装组件/切换资格/交换命令/派生重算/网络适配/死亡掉落）。以报告上一节登记的约 429 个为
基线，累计独立边界约 **453 个**；至少约 **373 个**仍存在配方初始化、制作提交、快速堆叠、
紧急转移和 Loadout 网络/掉落调用链缺口。静态识别覆盖率约 **98%～99%**，权威行为闭合度约
**14%～24%**。这些是源码阅读估计，不代表 Version4 已形成完整可运行模拟；本轮只读取
`D:\TRbackup\Version4`，未修改其源码，并继续排除 `D:\TRbackup\NLTX\src` 已有组件。

## 74. 第四十五轮：跨战斗/掉落/事件的难度曲线策略目录

### 72.1 GameDifficultyData 是共享只读策略，不是单个实体属性

`GameDifficultyData` 定义 `LinearCurve`，用有序 `Key(input, output)` 节点对难度值进行线性
插值（`Terraria.DataStructures/GameDifficultyData.cs:3-47`）。静态曲线包括敌人最大生命、
敌人伤害、敌对 Projectile 伤害、对敌人击退、敌人金币掉落、城镇 NPC 伤害、Debuff 持续时间
和雷击玩家伤害（`:49-67`）。实际消费者跨越多个系统：Player 使用 Debuff 曲线，NPC 使用
敌人生命/伤害/击退/金币/城镇伤害曲线，Projectile 使用敌对 Projectile 和雷击曲线，掉落
规则还读取敌人金币倍率（`Player.cs:3692`、`NPC.cs:6909-6952,17650-17651`、
`Projectile.cs:270,554,9911`、`GameContent.ItemDropRules/Conditions.cs:461-462`）。

这说明难度倍率应由共享的只读策略目录提供，不能复制到 Player、NPC、Projectile 各自的
组件中，否则 Journey/Classic/Expert/Master/Legendary 覆盖规则会发生漂移。

### 72.2 建议的难度边界

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `DifficultyCurveDefinitionCatalog` | 保存每种结果的输入/输出节点及版本 | `GameDifficultyData.cs:49-67`；`confirmed` |
| `DifficultyCurveQuery` | 对给定难度采样并执行插值/端点策略 | `GameDifficultyData.cs:25-47`；`confirmed/partial` |
| `DifficultyPolicyProjection` | 向战斗、Debuff、掉落和事件系统提供只读倍率 | `Player.cs:3692`、`NPC.cs:6909-6952,17650-17651`、`Projectile.cs:270,554,9911`；`confirmed` |
| `DifficultyOverrideComponent` | 保存 Journey/Creative/事件临时覆盖，不修改静态曲线 | `Main.cs:11369-11372`、`CreativePowers.cs`；`partial` |
| `DifficultyValidationSystem` | 检查节点非空、单调排序、重复输入和倍率范围 | `LinearCurve` 构造函数/`Sample`；`missing` |
| `DifficultyPolicyReplicationProjection` | 将服务器难度和允许覆盖投影给客户端，不传输可篡改倍率 | 网络/创造能力入口；`partial/missing` |

### 72.3 曲线采样的隐藏不变量和风险

`LinearCurve` 构造函数直接访问 `keys[0]`，空数组会抛出异常；随后仅读取各节点 input，
没有排序、重复输入或输出范围检查（`GameDifficultyData.cs:19-31`）。`Sample` 从第一个
节点开始，遇到 `value <= key2.input` 停止；若输入低于首节点或高于末节点，会按当前首/末
两点继续外推，而不是明确钳制（`:32-47`）。重复 input 通过 `num == 0` 返回前一节点
输出，具体优先级未记录。`Key.ToString`、`LinearCurve.ToString` 也返回默认值，调试/报告
投影无法直接说明当前策略内容。

因此 `DifficultyCurveQuery` 的接口必须显式声明：是否钳制到端点、节点是否要求升序、重复
输入如何拒绝、负倍率/过大倍率如何拒绝，以及每个难度枚举缺失时的回退策略。曲线采样应是
纯函数，随机性和实体写入留在调用方 System。

### 72.4 难度覆盖不能绕过权威策略

创造能力可在主循环中读取 `DifficultySliderPower.StrengthMultiplierToGiveNPCs` 覆盖 NPC
强度（`Main.cs:11369-11372`），而 `CreativePowerManager` 还负责能力权限和激活。建议将
覆盖建模为 `DifficultyOverrideComponent`，由 `DifficultyPolicyQuery` 先合并“基础曲线 +
服务器允许的临时覆盖”，再向 NPC/Projectile/掉落系统提供结果。客户端不能通过网络直接
写倍率；覆盖失效、世界重载和权限撤销都应产生版本化事件。

推荐调用方向：

```text
WorldRules/Difficulty -> DifficultyCurveDefinitionCatalog
  -> DifficultyPolicyQuery (+ validated server override)
  -> NPC/Projectile/Debuff/Loot/Lightning systems
  -> combat/drop/event results
```

### 72.5 验证 seam

最小 focused verifier 应覆盖：空曲线、单节点、升序/乱序节点、重复输入、首尾之外输入、
负值和超范围倍率；Journey 到 Legendary 的所有静态曲线快照；Creative 临时倍率权限、
撤销、世界重载和网络只读投影；同一难度下 Player Debuff、NPC 伤害、Projectile 伤害和
金币掉落使用同一策略版本。当前没有这些 verifier 的运行证据，均标记为 `未验证`。

### 72.6 本轮新增边界与统计

本轮新增约 **6 个**边界（曲线目录、采样 Query、策略投影、覆盖组件、验证系统、网络投影）。
累计独立边界约 **429 个**；至少约 **349 个**仍存在曲线输入校验、端点策略、创造覆盖和
跨系统策略版本一致性缺口。静态识别覆盖率约 **98%～99%**，权威行为闭合度约 **14%～23%**。
本轮只读取 `D:\TRbackup\Version4`，未修改其源码，并排除 `D:\TRbackup\NLTX\src`。

## 75. 第四十六轮：网络模块注册、数据包生命周期与服务器会话传输

### 73.1 NetManager 是传输编排边界，不应拥有领域状态

`NetManager` 维护模块 ID 到 `NetModule` 实例的注册表，并为每种模块保存静态
`PacketTypeStorage<T>.Id/Module`（`Terraria.Net/NetManager.cs:7-24,28-43`）。`Register`
按调用顺序递增 `_moduleCount`，没有重复类型、ID 溢出或注册阶段冻结检查。`Read` 先读取
模块 ID，再直接调用 `Deserialize(reader,userId)`，只在调用后更新诊断计数
（`:46-66`）；没有验证 `readLength`、包内剩余长度、未知模块拒绝结果或 reader 是否消费
完毕。这是第 65 节各领域模块缺口之上的传输级边界，应单独建模。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `NetModuleRegistryComponent` | 模块类型、稳定 ID、注册版本和冻结状态 | `NetManager.cs:9-43`；`partial` |
| `NetPacketIngressAdapter` | 长度、魔数、模块 ID、userId 和剩余字节验证 | `NetManager.cs:46-66`、`NetPacket.cs:9-33`；`missing` |
| `NetModuleDispatchSystem` | 将已验证帧路由到领域 Adapter 并处理 Deserialize 结果 | `NetModule.cs:5-14`、`NetManager.cs:53-66`；`partial` |
| `NetBroadcastRecipientQuery` | 按连接状态、忽略客户端和领域可见性选择接收者 | `NetManager.cs:68-93`；`partial` |
| `NetSendFailurePolicy` | 发送异常、断线、重试和包回收策略 | `NetManager.cs:126-145`；`missing` |

`BroadcastOrLoopback` 和 `SendToServerOrBroadcast` 当前都只调用 `Broadcast`，没有实现
客户端回环或客户端到服务器分支（`NetManager.cs:96-108`）。`SendToServer` 直接访问
`Netplay.Connection.Socket`，但没有连接/客户端模式检查；`SendToClient` 直接索引
`Netplay.Clients[playerId]`，没有槽位范围或连接状态验证（`:110-124`）。这些路径应返回
显式的 `SendResult`，不能吞掉失败后仍把领域命令视为已提交。

### 73.2 NetPacket/BufferPool 是资源生命周期边界

`NetPacket` 固定 5 字节头，构造时把总长度写入缓冲区并通过 `BufferPool.Request` 分配；
`ShrinkToFit` 根据写入位置回写长度，`Recycle` 将缓存归还池
（`Terraria.Net/NetPacket.cs:9-48`）。`SendData` 先缩包，再调用 socket 异步发送；异常被
空 `catch` 吞掉（`NetManager.cs:126-139`）。如果同一 `NetPacket` 被多个异步发送者共享，
发送尚未完成时立即 `Recycle` 的所有权也没有契约。

建议增加 `PacketLeaseComponent`、`PacketLengthValidationSystem`、`PacketSendQueue` 和
`PacketRecycleAdapter`：每次发送建立不可变 buffer lease，所有异步回调完成或明确失败后
才回收；长度必须限制在 5～65,535，模块负载必须不越界，重复回收必须可检测。

### 73.3 Netplay 会话拥有客户端槽位和断开状态机

`OnConnectionAccepted` 选择可用槽位、重置 `RemoteClient`、绑定 socket；满员时调用
`KickClient`，但 `KickClient` 和 `StopListening` 为空（`Terraria/Netplay.cs:209-239`）。
`InitializeServer` 建立 256 个客户端、读缓冲和服务器线程；`ServerLoop` 反复监听、更新
客户端并依赖 `Disconnect` 退出（`:250-320`）。`UpdateConnectedClients` 对 pending termination
执行 reset/断开同步，对活跃但已断开的客户端设置终止标记，并将 `Main.player[i].active`
置为 false 后调用 `Player.Hooks.PlayerDisconnect`（`:323-364`）。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `ConnectionSlotComponent` | 槽位、socket、连接阶段、读缓冲和玩家绑定 | `Netplay.cs:209-300`；`partial` |
| `ConnectionAdmissionSystem` | 满员、握手、封禁、服务器模式和槽位分配 | `Netplay.cs:209-239,388-400`；`partial` |
| `ConnectionTerminationState` | Pending、批准、reset、断开同步和玩家清理 | `Netplay.cs:323-364`；`confirmed/partial` |
| `ServerLoopSystem` | 监听、连接更新、广播线程和有序停机 | `Netplay.cs:307-320,366-385`；`partial` |
| `PlayerConnectionProjection` | 将连接状态投影为 Player active/disconnect 事件 | `Netplay.cs:346-360`；`partial` |

当前没有看到握手超时、半开连接、socket 关闭顺序、玩家物品保存失败和槽位复用保护的
完整证据；`PlayerDisconnect` 事件不能代替网络状态机本身。

### 73.4 TcpSocket/DebugNetworkStream 仍有关键空实现

`TcpSocket` 提供异步读写、监听线程、远端地址和 `IsConnected`，但显式接口
`Connect` 与 `StopListening` 为空（`Terraria.Net.Sockets/TcpSocket.cs:56-70,156-187`）。
监听循环在任意异常时吞掉异常并继续/退出，没有区分正常关闭、拒绝连接和协议错误
（`:187-201`）。`DebugNetworkStream` 增加延迟、入站/出站队列和异常字段，但其异步结果、
队列刷新和关闭路径需要与 socket 所有权配合，不能作为生产网络语义的替代。

建议拆分 `SocketTransportAdapter`、`AcceptLoopSystem`、`ReceiveFramingSystem`、
`SendCompletionSystem` 和 `NetworkFaultProjection`。调试延迟只能包装传输，不得改变
领域 tick 顺序或把未发送包视为权威状态。

### 73.5 Terraria.Server.Game 是宿主生命周期壳

`Terraria.Server.Game` 实现 XNA `Game` 形状，但 `Components`、`Content`、`GraphicsDevice`、
`Services`、`Window` 等属性返回 null/默认值；`Run`、`Update`、`Draw`、`Initialize`、
`LoadContent`、`Dispose` 和退出回调均为空或占位（`Terraria.Server/Game.cs:8-104`）。
因此服务器进程宿主、模拟调度和图形设备初始化不能从该类得到闭合证据。应将其拆为
`ServerHostLifecycleAdapter`、`SimulationHostLoop`、`ServerShutdownCommand` 和
`HostResourceProjection`，并明确无图形服务器不依赖 `GraphicsDevice`/`Content`。

### 73.6 传输到权威命令的数据流与验证 seam

```text
SocketTransport -> PacketFraming/LengthValidation
  -> NetModuleRegistry -> DomainCommandAdapter
  -> ServerCommandValidation -> authoritative systems
  -> state projection -> recipient query -> PacketSendQueue
  -> send completion -> packet recycle

Connection admission -> slot lifecycle -> Player active/disconnect projection
ServerHostLifecycle -> SimulationHostLoop -> ordered shutdown
```

最小 focused verifier 应覆盖：未知模块、长度不足/超长、模块未消费剩余字节、重复注册、
ID 溢出、Broadcast/SendToServer/SendToClient 模式错误、包回收时机、异步发送失败、槽位
越界、满员/握手超时/半开连接、断开后玩家清理和槽位复用；同时验证服务器宿主不需要
图形资源。当前没有这些 verifier 的运行证据，均标记为 `未验证`。

### 73.7 本轮新增边界与统计

本轮新增约 **15 个**边界（模块注册/收包/分发/接收者查询/失败策略、Packet lease/长度、
连接槽位/准入/终止/服务器循环、socket 传输/收发完成/故障投影、服务器宿主生命周期）。
累计独立边界约 **444 个**；至少约 **364 个**仍存在模块注册冻结、包长度与回收、回环
发送、socket 连接/停机、握手/断开和无图形宿主闭合缺口。静态识别覆盖率约 **98%～99%**，
权威行为闭合度约 **13%～22%**。本轮只读取 `D:\TRbackup\Version4`，未修改其源码，
并排除 `D:\TRbackup\NLTX\src`。

## 76. 第四十七轮：双地牢诅咒资格、屏幕遮挡投影与 Moon Lord 终结表现

### 74.1 DangerousDungeonCurse 是进度门槛 Query，不是视觉状态

`DangerousDungeonCurse.GetProgressPlayerNeedsToMatch` 根据玩家当前区域（Lihzahrd Temple、
Hallow、Dungeon、Jungle、Crimson/Corruption 或早期区域）返回所需的双地牢不可破墙进度；
`GetProgressPlayerCanSafelyMatch` 根据已击败 Boss 和 Hardmode 返回世界当前安全进度
（`Terraria.GameContent.Events/DangerousDungeonCurse.cs:5-58`）。NPC 更新会比较这两个值，
设置 `tresspassingDualDungeon`，从而影响 NPC 行为/资格（`Terraria/NPC.cs:318`）。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `DualDungeonProgressQuery` | 从玩家区域推导需要达到的进度层级 | `DangerousDungeonCurse.cs:7-32`；`confirmed` |
| `WorldSafeProgressQuery` | 从 Boss/Hardmode 世界进度推导可安全层级 | `DangerousDungeonCurse.cs:34-55`；`confirmed` |
| `DungeonTrespassQualification` | 比较需求与安全层级，输出玩家/NPC 越级资格 | `NPC.cs:318`；`confirmed/partial` |
| `DungeonCursePresentationProjection` | 将越级状态投影为屏幕遮挡/提示，不修改进度 | `ScreenObstruction.cs:6-35`；`partial` |

该 Query 读取世界进度和区域派生值，但不应写回 `NPC.downed*` 或玩家区域；双地牢进度的
枚举值、边界区域重叠、Boss 同帧击败后的刷新时机需要由调度顺序明确。

### 74.2 ScreenObstruction 只拥有平滑视觉缓存

`ScreenObstruction` 保存 `lastSpeed` 和 `screenObstruction`，在 `Update` 中根据玩家是否
处于不可破坏墙内、诅咒进度差和 `headcovered` 计算目标遮挡量，再通过
`SceneState.MoveTowards` 平滑逼近（`Terraria.GameContent.Events/ScreenObstruction.cs:6-35`）。
它是从 `SceneMetrics`/诅咒 Query 到客户端渲染的投影，不能成为影响 NPC、碰撞或玩家移动的
权威组件。建议使用 `ScreenObstructionStateComponent`、`ObstructionTargetQuery` 和
`ObstructionVisualProjection`，并在世界切换/玩家切换时清理缓存；当前没有显式清理入口，
标记为 `partial`。

### 74.3 MoonlordDeathDrama 是短生命周期表现实体集合

`MoonlordDeathDrama` 维护 `MoonlordPiece` 碎片、`MoonlordExplosion` 爆炸和光源位置列表，
每帧更新速度、重力、旋转、爆炸帧和边界死亡条件；`RequestLight` 将请求光强钳制到 1，
由 `Update` 根据场景中心 2000 像素范围决定是否保留并逐渐改变 `whitening`
（`Terraria.GameContent.Events/MoonlordDeathDrama.cs:9-166`）。NPC 多个 Moon Lord AI 阶段
调用 `RequestLight`（`Terraria/NPC.cs:35472-35735,41491,41587`），但该类不创建或修改
NPC/Projectile，属于事件后的客户端表现投影。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `MoonlordDeathDramaPieceComponent` | 碎片位置、速度、旋转和纹理句柄 | `MoonlordDeathDrama.cs:11-57`；`confirmed/partial` |
| `MoonlordExplosionComponent` | 爆炸帧、帧速和边界寿命 | `MoonlordDeathDrama.cs:59-103`；`confirmed/partial` |
| `MoonlordDramaLifetimeSystem` | 更新、距离裁剪和 Dead 项移除 | `MoonlordDeathDrama.cs:115-150`；`confirmed/partial` |
| `DramaLightRequestBuffer` | 收集场景光源和最大泛白请求 | `MoonlordDeathDrama.cs:105-166`；`confirmed` |
| `MoonlordDramaPresentationProjection` | 输出纹理绘制、光照和泛白，不反向写战斗状态 | 当前无明确 Draw 消费者；`missing` |

`MoonlordPiece.Dead`/`MoonlordExplosion.Dead` 依赖世界边界和纹理帧速，空纹理、零帧速、
场景切换和重复 `RequestLight` 的处理没有验证。不能因为 NPC AI 调用了 `RequestLight`，
就把碎片/爆炸列表合并进 NPC 的权威生命周期。

### 74.4 调用方向与验证 seam

```text
World progress + Player/NPC zone
  -> DualDungeonProgressQuery + WorldSafeProgressQuery
  -> DungeonTrespassQualification
  -> NPC behavior / ScreenObstruction projection

Moon Lord AI event -> DramaPiece/Explosion commands
  -> DramaLifetimeSystem + LightRequestBuffer
  -> client presentation projection
```

最小 focused verifier 应覆盖：双地牢各区域层级、Boss 进度边界、同帧 Hardmode 更新、
玩家/NPC 越级资格；头部遮挡和诅咒差值的遮挡目标及平滑速度；碎片/爆炸边界死亡、零帧速、
纹理缺失、距离裁剪、光强钳制和场景清理。当前没有这些 verifier 的运行证据，均标记为
`未验证`。

### 74.5 本轮新增边界与统计

本轮新增约 **9 个**边界（双地牢需求/安全进度/越级资格、遮挡状态/目标/投影、碎片/爆炸/
生命周期/光源缓冲/表现投影）。累计独立边界约 **453 个**；至少约 **371 个**仍存在双地牢
进度刷新、遮挡缓存清理、表现消费、纹理/帧边界和场景切换缺口。静态识别覆盖率约 **98%～99%**，
权威行为闭合度约 **13%～22%**。本轮仍只读取 `D:\TRbackup\Version4`，未修改其源码，
并排除 `D:\TRbackup\NLTX\src`。

## 77. 第四十八轮：召唤物恢复、Projectile 引用、无人机镜头与击杀结果事件

### 75.1 MinionSpawnInfo 是召唤能力的恢复命令，而非 Projectile 行为字段

`Projectile` 在生成时通过 `TrackMinionSpawnSource` 检查 `minion` 和
`ProjectileID.Sets.TrackMinionSpawnFromItemUse[type]`，从 `EntitySource_ItemUse` 保存
`MinionSpawnFromInventoryItem`（`Terraria/Projectile.cs:10560-10572`）。该对象只复制
原始物品的 `ItemType` 与 `ItemPrefix`（`Terraria.DataStructures/MinionSpawnFromInventoryItem.cs:3-15`），
并实现 `ItemMatches` 与 `TryRespawn`；两者当前为空体。`MinionRespawner` 仅持有私有
`List<MinionSpawnInfo>`，未找到添加、遍历、死亡触发或玩家重生消费入口
（`MinionRespawner.cs:5-8`）。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `MinionSpawnProvenanceComponent` | 召唤物对应的物品类型和前缀快照 | `Projectile.cs:208,548,10560-10572`；`confirmed/partial` |
| `MinionRespawnIntent` | 描述从背包物品恢复一个召唤物的命令 | `MinionSpawnInfo.cs:3-6`、`MinionSpawnFromInventoryItem.cs:3-15`；`partial` |
| `MinionRespawnQueueComponent` | 保存待恢复召唤物及顺序/失败信息 | `MinionRespawner.cs:5-8`；`missing` |
| `MinionRespawnSystem` | 在玩家重生或召唤物失效后验证物品并重新生成 | `MinionSpawnFromInventoryItem.cs:11-15`；`missing` |

该恢复路径必须区分“持有物品快照”和“当前背包实例”，验证物品类型、前缀是否仍可用、
玩家权限、召唤槽位和重复恢复；不能因为 `MinionSpawnInfo` 继承自抽象类，就让其直接
修改 Projectile 或绕过 `MinionCapacityState`。

### 75.2 TrackedProjectileReference 以 owner + identity + type 解析网络/槽位关系

玩家的 `piggyBankProjTracker` 与 `voidLensChest` 使用 `TrackedProjectileReference`
（`Terraria/Player.cs:2266-2268`）。该值对象保存 Projectile 本地槽位、owner、网络 identity、
类型和跟踪标志；`Set`/`Clear` 管理生命周期，`Write` 只序列化 owner、identity 和 type，
`TryReading` 再通过三元组查找匹配 Projectile（`TrackedProjectileReference.cs:5-63`）。
`FindMatchingProjectile`、`Equals(object)` 和 `GetHashCode` 当前为空/默认实现
（`:65-93`），因此不能确认槽位重用、网络伪造和哈希容器行为安全。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `ProjectileRelationRefComponent` | owner、identity、type 与本地槽位的非权威引用 | `TrackedProjectileReference.cs:5-17`；`confirmed` |
| `ProjectileReferenceResolver` | 按 owner/identity/type 查找 active Projectile，拒绝槽位重用 | `TrackedProjectileReference.cs:53-79`；`missing` |
| `ProjectileReferenceReplicationAdapter` | 版本化编码和读入边界检查 | `TrackedProjectileReference.cs:41-63`；`partial` |
| `ProjectileReferenceLifecycleSystem` | Projectile 销毁、owner 离线或类型变化时清理引用 | `TrackedProjectileReference.cs:19-39`；`missing` |

该引用不是持久实体 ID，也不是直接可写的 Projectile 指针；解析失败必须返回空引用并让
上层关闭对应 UI/容器，而不是继续使用过期槽位。

### 75.3 DroneCameraTracker 是客户端相机投影，权威状态仅提供资格输入

`Main.DroneCameraTracker` 在世界清理时重置，并在摄像机选择路径中调用 `TryTracking`
（`Main.cs:1032,12382`；`WorldGen.cs:6457`）。它保存被跟踪 Projectile 和上次类型，只有
Projectile active、类型未变化、owner 为本地玩家且 `remoteVisionForDrone` 为真时，才输出
Projectile 中心，否则清除引用并返回 false（`DroneCameraTracker.cs:5-35`）。

建议拆分为 `DroneCameraTargetRefComponent`、`DroneCameraEligibilityQuery`、
`DroneCameraProjection` 和 `DroneCameraCleanupSystem`。这些组件均属客户端表现层；服务器
仍需验证无人机 Projectile 的 owner、视野权限和可见范围，不能把相机位置作为玩家位置或
移动命令。

### 75.4 NPCKillAttempt 是击杀结果快照，OnKillNPC 消费链为空

玩家和 Projectile 命中 NPC 前分别创建 `NPCKillAttempt`，快照保存目标引用、`netId` 和
命中前 active 状态；`DidNPCDie` 通过目标当前 `active` 判断死亡
（`Player.cs:11970-12006`；`Projectile.cs:12510`；`NPCKillAttempt.cs:3-24`）。玩家路径
在 `StrikeNPC` 后广播伤害、更新 Banner 命中，再在死亡时调用 `OnKillNPC`，但
`Player.OnKillNPC` 当前为空体（`Player.cs:11988-11993`）。因此击杀归因、掉落、成就、
复仇标记和事件通知的提交顺序不能从该类单独确认。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `NpcKillAttemptSnapshot` | 保存命中前 NPC 身份和 active 快照 | `NPCKillAttempt.cs:3-24`；`confirmed` |
| `NpcDeathResultQuery` | 比较 Strike 前后 active/生命状态并输出死亡结果 | `NPCKillAttempt.cs:20-24`；`partial` |
| `NpcKillAttributionCommand` | 记录玩家/Projectile、Banner、外部来源和网络上下文 | `Player.cs:11970-12006`、`Projectile.cs:12510`；`partial` |
| `NpcDeathResolutionSystem` | 在权威死亡后提交掉落、成就、复仇和进度事件 | `Player.cs:11988-11993`；`missing` |

`NPCKillAttempt` 不应拥有 NPC 的生命写集；死亡判定必须在服务器 `StrikeNPC` 完成后执行，
并保证同一 NPC 槽位不会因重用而重复触发死亡事件。

### 75.5 PlayerGetItemLogger 是临时事务审计，不是背包权威状态

`Player.GetItemLogger` 是静态实例；`Player` 的多个获取物品路径记录目标数组、槽位、上下文
和堆叠数量（`Player.cs:2477,22967-23000`）。`PlayerGetItemLogger.Add` 仅在私有
`_enabled` 为真时追加日志项，当前未找到启用、消费、清空或失败回滚入口
（`PlayerGetItemLogger.cs:5-38`）。

建议拆分为 `ItemTransferIntent`、`InventoryMutationAuditComponent`、
`InventoryTransactionSystem` 和 `ItemTransferAuditProjection`。日志必须保存槽位上下文的
不可变快照，不能持有可变 `Item[]` 引用作为长期状态；物品转移的真正权威写入仍归
`InventoryTransactionSystem`，审计只在提交成功或显式失败时输出。

### 75.6 统一关系数据流与验证 seam

```text
ItemUse -> MinionSpawnProvenance -> RespawnIntent/Queue -> Capacity validation -> Projectile spawn
Network/player state -> ProjectileReferenceResolver -> relation projection -> cleanup on despawn
Drone Projectile -> eligibility query -> camera projection
StrikeNPC -> KillAttemptSnapshot -> DeathResult -> attribution/drop/achievement events
Inventory command -> transaction commit -> audit projection
```

最小 focused verifier 应覆盖：召唤物前缀快照与当前库存不一致、重复恢复和容量不足；
Projectile owner/identity/type 三元组解析、槽位重用、未知类型和网络截断；无人机 owner/权限
失效；NPC 命中前后 active 变化、重复死亡事件和 Banner/掉落顺序；物品获取日志启用边界、
提交失败回滚和数组引用不泄漏。当前没有这些 verifier 的运行证据，均标记为 `未验证`。

### 75.7 本轮新增边界与统计

本轮新增约 **14 个**边界（召唤物来源/恢复、Projectile 关系引用/解析/复制/生命周期、
无人机相机、NPC 击杀快照/结果/归因/死亡解析、物品转移审计）。累计独立边界约 **423 个**；
至少约 **344 个**仍存在召唤物恢复消费者、引用解析、击杀消费和物品事务审计缺口。静态
识别覆盖率约 **98%～99%**，权威行为闭合度约 **14%～24%**。本轮只读取
`D:\TRbackup\Version4`，未修改其源码，并排除 `D:\TRbackup\NLTX\src`。

## 78. 第四十九轮：物品变体、前缀滚动、鞭标签效果与战利品模拟隔离

### 76.1 ItemVariants 是世界规则驱动的只读内容目录

`ItemVariants` 为每个物品 ID 保存 `VariantEntry` 列表；每个条目包含一个
`ItemVariant` 和若干 `ItemVariantCondition`，`AnyConditionMet` 只调用条件函数，
`SelectVariant` 按注册顺序返回第一个满足条件的变体（`Terraria.GameContent.Items/ItemVariants.cs:5-121`）。
静态构造函数注册 Remix、GetGoodWorld 和 Mechdusa 世界下的强化、削弱、重平衡、启用及
禁用 Boss 召唤变体（`ItemVariants.cs:123-155`）。`Item.SetDefaults` 和大量物品分支读取
`Variant`，生成/加载时也会调用 `SelectVariant` 或 `HasVariant`
（`Terraria/Item.cs:2543,3097,3653,7386,7700-7845,48136-48138,48915-48922`）。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `ItemVariantDefinitionCatalog` | 物品 ID 到变体及条件的只读注册表 | `ItemVariants.cs:5-155`；`confirmed` |
| `ItemVariantQualificationQuery` | 读取世界种子/规则并按注册顺序选择变体 | `ItemVariants.cs:32,99-121`；`confirmed/partial` |
| `ItemVariantProjection` | 把变体选择投影到 Item 默认值、描述和 UI | `Item.cs:2543-48922`；`partial` |
| `ItemVariantStateComponent` | 保存实例当前变体，区分定义结果与库存实例字段 | `Item.cs:48136-48138`；`partial` |

`ItemVariant` 与 `ItemVariantCondition.ToString` 都返回默认值（`ItemVariant.cs:13-16`；
`ItemVariantCondition.cs:17-20`），而 `VariantEntry` 条件集合可被追加但没有重复注册/排序
校验。变体条件不应写世界状态；同一物品在世界规则切换时是否重新投影，需要明确重建
策略，避免现有 Item 实例的前缀、堆叠和价格被隐式重置。

### 76.2 PrefixLegacy + Item.Prefix 形成独立的实例变异流水线

`PrefixLegacy` 按武器/饰品类别提供前缀 ID 白名单，并用 `SetFactory(ItemID.Count)` 建立
Magic、Summon、GunsBows、近战、回旋镖等能力集合（`Terraria.GameContent.Prefixes/PrefixLegacy.cs:5-113`）。
`Item.GetRollablePrefixes` 按这些集合选择可滚动列表；`CanHavePrefixes`、`CanRollPrefix`、
`BestPrefixValue` 和 `TryGetPrefixStatMultipliersForItem` 共同决定前缀资格、最佳品质及
伤害/速度/击退/大小/射速/法力/暴击/标签伤害/护甲穿透/价值修正
（`Terraria/Item.cs:375-512,880-1018`）。`Prefix` 还根据 `WorldGen.isGeneratingOrLoadingWorld`
切换 `WorldGen.genRand` 或 `Main.rand`，把修正写入 Item 字段并钳制稀有度
（`Item.cs:383-497`）。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `PrefixEligibilityCatalog` | 类别到合法前缀 ID 的映射 | `PrefixLegacy.cs:9-113`、`Item.cs:921-951`；`confirmed` |
| `PrefixRollQuery` | 纯计算可滚动前缀、品质和修正倍率 | `Item.cs:501-512,952-1018`；`partial` |
| `ItemPrefixMutationSystem` | 消费随机源，将倍率写入 Item 实例并更新稀有度/价值 | `Item.cs:383-497`；`confirmed/partial` |
| `PrefixRandomSourceAdapter` | 区分世界生成随机源与运行时随机源 | `Item.cs:386-413`；`confirmed` |
| `PrefixReplicationProjection` | 将 prefix 字节投影到 WorldItem、Chest、TileEntity 和网络包 | `NetMessage.cs:212,649,875,1296,1472,1541,1628`、`MessageBuffer.cs:332-350,1091-1135`；`partial` |

`RollAPrefix` 当前为空体（`Item.cs:963-964`），`TryGetPrefixStatMultipliersForItem` 的
完整倍率分支也不能由类别白名单推断。网络接收方多处读取单字节 prefix 后直接调用
`Prefix`，因此必须在 Adapter 中验证 Item 类型、前缀白名单、随机策略和发送者权限，不能
把客户端提供的 prefix 直接写入容器或 TileEntity。

### 76.3 WhipTagEffect 的权威写集跨越玩家、NPC 和 Projectile 命中

`TagEffectState` 为一个玩家保存当前效果类型、每个 NPC 的标签剩余时间和 Proc 剩余时间
数组（`Terraria.GameContent.Items/TagEffectState.cs:101-124`）。`Update` 逐槽递减；
`TryApplyTagToNPC`、`TryEnableProcOnNPC`、`ModifyHit`、`OnHit` 分别负责资格、标记、伤害
修改和 Proc 消费（`TagEffectState.cs:125-235`）。`UniqueTagEffect` 定义标签持续时间、
NetSync、Proc 同步和命中回调；六个具体 Whip 效果类覆盖标签或 Proc 副作用，但实现多为空体
（`UniqueTagEffect.cs:3-25`；`WhipTagEffect*.cs`）。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `WhipTagDefinitionCatalog` | Item 类型到唯一标签效果、持续时间和同步策略 | `UniqueTagEffect.cs:3-25`、`ItemID.Sets.UniqueTagEffects`；`partial` |
| `NpcTagTimerComponent` | 玩家- NPC 标签/Proc 的时间表 | `TagEffectState.cs:109-124`；`confirmed` |
| `TagQualificationQuery` | 判断物品、NPC 类型和当前效果是否可应用/可触发 | `TagEffectState.cs:143-164`；`partial` |
| `TagEffectMutationSystem` | 写入标签计时、修改命中、消费 Proc 并清理 NPC 槽位 | `TagEffectState.cs:125-235`；`partial` |
| `TagEffectReplicationAdapter` | 全状态/活动效果/NPC 变化消息编码与权限验证 | `TagEffectState.cs:8-99`；`missing` |

`ApplyTagToNPC`、`EnableProcOnNPC`、`Clear`、`WriteNPCChange` 和网络 `Deserialize` 均为空或
默认返回；`WriteSparseNPCTimeArray` 用单字节结束哨兵，却没有数组长度/索引范围/重复索引
校验（`TagEffectState.cs:16-33,45-55`）。因此标签效果只能标为 `partial/missing`，不能
假设客户端消息能改变服务器的 NPC 命中规则。

### 76.4 LootSimulation 只能是隔离的工具投影，当前没有生产消费闭环

`SimulatorInfo` 创建独立 `Player`，保存原始时间、白天标志、玩家位置、Expert 开关、
物品计数器和 NPC 受害者引用（`Terraria.GameContent.LootSimulation/SimulatorInfo.cs:5-28`）。
`LootSimulationItemCounter` 仅声明普通/Expert 两组 `ItemID.Count` 长整型计数数组，未提供
累加、导出或重置 API（`LootSimulationItemCounter.cs:7-11`）。当前 `Version4` 未找到
生产者或运行时消费者调用这些类型，故它们不应直接修改 `Main.player`、世界掉落或服务器
战利品结果。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `LootSimulationScenarioComponent` | 模拟玩家、受害 NPC、Expert 标志和时间快照 | `SimulatorInfo.cs:5-28`；`partial` |
| `LootSimulationSandboxAdapter` | 提供隔离 Item/NPC/World 依赖，禁止写真实世界 | `SimulatorInfo.cs:21-27`；`missing` |
| `LootSimulationAccumulator` | 普通/Expert 物品计数、统计和导出 | `LootSimulationItemCounter.cs:7-11`；`missing` |
| `LootSimulationProjection` | 将模拟结果转成工具/UI 报告，不反向生成掉落 | 当前无消费者；`missing` |

### 76.5 调用方向与验证 seam

```text
WorldRules -> ItemVariantQualificationQuery -> ItemVariantProjection -> Item defaults
Item type -> PrefixEligibilityCatalog -> PrefixRollQuery -> ItemPrefixMutationSystem
  -> PrefixReplicationProjection (WorldItem/Chest/TileEntity/network)
Whip item/NPC hit -> TagQualificationQuery -> TagEffectMutationSystem
  -> Combat result + TagEffectReplicationAdapter
LootSimulationScenario -> isolated Sandbox -> Accumulator -> Tool Projection
```

最小 focused verifier 应覆盖：变体条件优先级、世界规则切换后的实例重投影；类别白名单、
`prefix` 越界/未知值、生成随机源与运行时随机源隔离、前缀倍率和稀有度钳制；标签计时递减、
NPC 槽位重置、Proc 一次性消费、网络稀疏数组哨兵和重复索引拒绝；LootSimulation 不触碰
真实 `Main` 状态且普通/Expert 计数独立。当前没有这些 verifier 的运行证据，均标记为
`未验证`。

### 76.6 本轮新增边界与统计

本轮新增约 **17 个**边界（变体目录/资格/投影/实例状态、前缀目录/查询/变异/随机源/复制、
Whip 标签目录/计时/资格/变异/复制、LootSimulation 场景/沙箱/累加/投影）。累计独立边界约
**401 个**；至少约 **324 个**仍存在前缀滚动、变体描述、标签网络/命中副作用和战利品
模拟消费链缺口。静态识别覆盖率约 **98%～99%**，权威行为闭合度约 **15%～26%**。
本轮没有修改 `D:\TRbackup\Version4` 源码，统计继续排除 `D:\TRbackup\NLTX\src`。

## 79. 第五十轮：TileObjectData 配置模块、锚点约束与放置 Hook 组合

### 77.1 `Terraria.Modules` 是 TileObject 内容定义层，不是运行时 Tile 状态

`Terraria.Modules` 将多对象 Tile 的定义拆成若干可复制模块：
`TileObjectBaseModule` 保存宽高、原点、方向、随机样式和锚点展平；`TileObjectCoordinatesModule`
保存每列高度、padding、样式尺寸、绘制偏移和帧偏移；`TileObjectStyleModule` 保存样式、横向
排列、换行上限、倍率和行跳过；`TileObjectDrawModule` 保存偏移、翻转和下沉步长
（`Terraria.Modules/TileObjectBaseModule.cs:5-55`、`TileObjectCoordinatesModule.cs:5-80`、
`TileObjectStyleModule.cs:3-54`、`TileObjectDrawModule.cs:3-39`）。这些模块是内容目录的
可变配置副本，不应直接承载当前世界中某个 Tile 的帧、位置或生命状态。

另外四个模块表达跨 Tile/Wall 的结构约束：

* `AnchorDataModule` 为上下左右保存 `AnchorData`，并带 `wall` 标志；
* `AnchorTypesModule` 保存允许/禁止/替代 Tile 和允许 Wall 的 ID 数组；
* `LiquidDeathModule` 声明水/岩浆接触是否致死；
* `LiquidPlacementModule` 声明水/岩浆是否允许放置；

证据位于 `AnchorDataModule.cs:5-37`、`AnchorTypesModule.cs:5-60`、
`LiquidDeathModule.cs:3-25`、`LiquidPlacementModule.cs:3-25`。它们应被纯资格 Query 消费，
不能让 `TilePlacementSystem` 把定义数组当作世界 Tile 存储。

### 77.2 建议的配置边界

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `TileObjectGeometryDefinition` | 宽高、原点、方向、随机样式和锚点展平配置 | `TileObjectBaseModule.cs:5-55`；`confirmed` |
| `TileObjectCoordinateDefinition` | 子 Tile 高度、padding、样式网格和帧偏移 | `TileObjectCoordinatesModule.cs:5-80`；`confirmed/partial` |
| `TileObjectStyleDefinition` | 样式索引、横排/换行和视觉覆盖参数 | `TileObjectStyleModule.cs:3-54`；`confirmed` |
| `TileObjectDrawDefinition` | 绘制偏移、翻转和下沉步长 | `TileObjectDrawModule.cs:3-39`；`confirmed` |
| `TileAnchorDefinition` | 四向锚点、允许/禁止 Tile/Wall 类型 | `AnchorDataModule.cs:5-37`、`AnchorTypesModule.cs:5-60`；`confirmed` |
| `LiquidInteractionDefinition` | 液体放置与液体致死资格 | `LiquidDeathModule.cs:3-25`、`LiquidPlacementModule.cs:3-25`；`confirmed` |
| `TilePlacementHookAdapter` | 检查、全体/本地放置回调、覆盖放置和样式回调 | `TilePlacementHooksModule.cs:5-42`；`partial` |
| `TileObjectAlternateCatalog` | 同一 Tile 的 alternate/sub-TileObjectData 副本 | `TileObjectAlternatesModule.cs:5-40`、`TileObjectSubTilesModule.cs:5-33`；`partial` |

### 77.3 复制构造是配置快照 seam，但存在未闭合克隆路径

各模块通过可选 `copyFrom` 构造函数复制值类型和数组；`AnchorTypesModule` 对四组数组做深
拷贝，`TileObjectAlternatesModule` 对 `TileObjectData` 列表逐项复制，坐标模块也复制帧偏移
二维数组（上述各模块的 copy constructor）。这形成 `TileObjectDefinitionSnapshot` 的自然
替换 seam：注册阶段产生不可变快照，运行时 Query 只读快照，编辑器/加载器不应共享可变列表。

但 `TileObjectSubTilesModule` 在非空 `copyFrom` 路径中创建新列表后以
`for (int i = 0; i < data.Count; i++)` 迭代新列表（`TileObjectSubTilesModule.cs:17-31`），
因此永远不会复制源数据；其 `newData` 参数也没有被消费。该模块只能标为 `partial/missing`，
不能假定 alternate 和 sub-Tile 的完整结构在加载后存在。`TileObjectAlternatesModule` 和
坐标模块也没有发现版本/空数组校验，需由注册 Adapter 在快照提交前验证。

### 77.4 Hook 与结构变更的调用方向

`TilePlacementHooksModule` 同时保存 `check`、`postPlaceEveryone`、`postPlaceMyPlayer`、
`placeOverride` 和 `getStyleMethod`（`TilePlacementHooksModule.cs:5-42`）。这些回调具有不同
副作用边界：检查/样式应是 Query，覆盖放置是 Command 生产者，post-place 回调是结构提交后
事件。不能把它们作为任意委托集合直接在配置层执行，否则会绕过锚点、液体和 TileEntity
原子提交。

建议顺序：

```text
TileObjectDefinitionSnapshot
  -> TileAnchorQuery + LiquidInteractionQuery + StyleQuery
  -> PlacementHook check / placeOverride
  -> TilePlacementCommand
  -> TileMutationSystem (Tile/Frame/TileEntity)
  -> postPlaceEveryone / postPlaceMyPlayer events
```

最小 focused verifier 应覆盖：模块深拷贝不共享数组/列表；sub-Tile 克隆保留源条目；空/越界
Tile 与 Wall ID 被拒绝；液体放置/致死规则组合；alternate 样式与方向计算；Hook 执行顺序、
异常隔离和 post-place 仅在原子提交成功后触发。当前没有这些 verifier 的运行证据，均标记为
`未验证`。

### 77.5 本轮新增边界与统计

本轮新增约 **8 个**边界（几何、坐标、样式、绘制、锚点、液体交互、Hook 适配、alternate/
sub-Tile 目录）。累计独立边界约 **409 个**；至少约 **331 个**仍存在 sub-Tile 克隆、Hook
副作用隔离、配置版本校验和放置提交顺序缺口。静态识别覆盖率约 **98%～99%**，权威行为
闭合度约 **15%～25%**。本轮只读取 `D:\TRbackup\Version4`，未修改其源码，并排除
`D:\TRbackup\NLTX\src` 已有组件。

## 80. 第五十四轮：临时 Tile 动画、粒子编排与地平线表现投影

### 78.1 Animation 是电线/Tile 触发的短生命周期表现状态

`Terraria.Animation` 在启动时创建四个静态容器：活动动画列表 `_animations`、按
`Point16` 索引的临时动画字典 `_temporaryAnimations`、待移除坐标列表和待加入动画列表
（`Terraria/Animation.cs:6-28`）。`NewTemporaryAnimation` 对世界边界做检查，创建动画对象，
调用 `SetDefaults(type)`，写入 Tile 类型和坐标，将对象放入待加入队列，并发送
`NetMessage.SendTemporaryAnimation`（`:31-45`）。但 `SetDefaults` 为空，未找到待加入/待移除
队列提交、逐帧推进、字典替换、过期清理或绘制消费；四个静态容器在 `Version4` 中只有
初始化和入队证据。

调用方并非纯 UI：`Wiring` 在机关、压力板或 Tile 变化时触发类型 0/1 等临时动画，
`MessageBuffer` 接收网络消息后再次调用 `NewTemporaryAnimation`
（`Terraria/Wiring.cs:1524-1556,2188-2321`、`MessageBuffer.cs:2495`）。因此它是
“权威 Tile/机关事件 → 客户端表现”的投影边界，网络消息不能反向决定 Tile 状态。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `TemporaryTileAnimationComponent` | 坐标、Tile 类型、动画类型和表现生命周期 | `Animation.cs:6-20`；`partial` |
| `TileAnimationTriggerAdapter` | 将 Wiring/Tile 事件转换成临时动画命令 | `Animation.NewTemporaryAnimation` 调用点；`confirmed/partial` |
| `TemporaryAnimationQueueSystem` | 去重、待加入/待移除批处理和过期清理 | `_awaitingAddition/_awaitingRemoval`；`missing` |
| `TileAnimationFrameSystem` | 根据类型推进帧/时间并更新活动字典 | `SetDefaults`、`_animations`、`_temporaryAnimations`；`missing` |
| `TileAnimationNetworkProjection` | 广播/接收坐标、Tile 类型和动画类型 | `NetMessage.SendTemporaryAnimation`、`MessageBuffer.cs:2495`；`partial` |

坐标边界检查只能证明请求位置合法，不能证明 Tile 仍然存在、类型匹配或动画已成功加入；
这些条件应由提交前 Query 和客户端投影系统分别验证。

### 78.2 ParticleOrchestrator 是多来源表现命令路由，但直接生成路径为空

`ParticleOrchestraType` 枚举包含武器命中、坐骑火焰、Shimmer、传送、护盾、闪电、地牢生成、
Moon Lord 和多个特殊种子表现类型（`Terraria.GameContent.Drawing/ParticleOrchestraType.cs:3-75`）。
`ParticleOrchestraSettings` 传输世界位置、运动向量、唯一信息和调用玩家索引，并提供完整的
二进制序列化/反序列化（`ParticleOrchestraSettings.cs:5-37`）。

`ParticleOrchestrator` 暴露三种入口：客户端直接请求、广播和“发送服务器或广播”；入口会
填充 `IndexOfPlayerWhoInvokedThis`，广播入口调用 `NetParticlesModule.Serialize`，但
`SpawnParticlesDirect` 为空（`ParticleOrchestrator.cs:13-63`）。`NetParticlesModule.Deserialize`
读取类型和设置后直接广播，未检查枚举范围、设置长度、玩家索引或发送者权限
（`Terraria.GameContent.NetModules/NetParticlesModule.cs:7-30`）。

`RepelAt` 会遍历 `Main.ParticleSystem_World_BehindPlayers` 和
`Main.ParticleSystem_World_OverPlayers`，对实现 `IParticleRepel` 的粒子调用排斥；这证明
粒子系统有两个渲染层和每帧交互，但它们属于表现状态。`Main` 每 tick 更新两个
`ParticleRenderer`，`WorldGen.clearWorld` 会清空两个粒子列表
（`Terraria/Main.cs:1190-1192,1759-1760`、`WorldGen.cs:6417-6418`）。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `ParticleEffectDefinitionCatalog` | 枚举类型到粒子表现和参数解释 | `ParticleOrchestraType.cs:3-75`；`partial` |
| `ParticleSpawnCommand` | 位置、向量、唯一信息和调用玩家意图 | `ParticleOrchestraSettings.cs:5-37`；`confirmed` |
| `ParticleSpawnRoutingSystem` | 本地生成、广播/回环选择和来源玩家赋值 | `ParticleOrchestrator.cs:13-63`；`partial` |
| `ParticleReplicationAdapter` | NetParticles 包编解码、权限和长度验证 | `NetParticlesModule.cs:7-30`；`partial/missing` |
| `ParticleRendererComponent` | BehindPlayers/OverPlayers 粒子集合与设置 | `Main.cs:1190-1192`、`ParticleRenderer.cs:6-15`；`confirmed` |
| `ParticleUpdateAndPoolSystem` | 更新、移除、对象池归还和场景清理 | `ParticleRenderer.Update:20-43`、`WorldGen.cs:6417-6418`；`partial` |
| `ParticleRepulsionQuery` | 计算粒子与实体位置的排斥输入 | `ParticleOrchestrator.RepelAt`；`confirmed/partial` |

粒子命令必须由玩法事件产生，不能让粒子更新写回 NPC、Projectile、Tile 或玩家生命状态。
`clientOnly` 参数当前未形成真正分支，故“本地-only”与服务器广播的权限边界仍需补证。

### 78.3 ParticleRenderer 只闭合了移除/更新循环，清空和渲染路径缺失

`ParticleRenderer` 保存 `ParticleRendererSettings` 和 `IParticle` 列表；构造器初始化设置，
`Update` 会遍历粒子，先检查 `ShouldBeRemovedFromRenderer`，必要时归还
`IPooledParticle.RestInPool()` 并移除，否则调用 `IParticle.Update(ref Settings)`
（`Terraria.Graphics.Renderers/ParticleRenderer.cs:6-43`）。但 `Clear()` 为空，文件中没有
绘制方法；渲染器如何把粒子列表提交到 SpriteBatch、何时调用 Clear、对象池的容量/线程边界
均未闭合。建议保留 `ParticleRendererComponent`、`ParticleUpdateAndPoolSystem` 和
`ParticleDrawProjection` 三个边界，禁止把 `ParticleRenderer.Settings` 当作世界规则。

### 78.4 NextHorizonRenderer 是完整接口壳，不是天空/地平线模拟系统

`NextHorizonRenderer` 实现 `IHorizonRenderer`，保存约 200 个 `DrawData` 缓存，并声明地平线、
表层、光照、太阳、云、镜头光晕的绘制接口；所有方法均为空
（`Terraria.GameContent.Drawing/NextHorizonRenderer.cs:8-24`）。它只能拆为客户端投影边界：
`HorizonDrawDataComponent`、`HorizonLayerProjection` 和 `HorizonRendererAdapter`，不能由空的
绘制接口推导天气、云或世界光照权威状态。真实天气/云规则仍应由既有 World/Scene 系统提供
不可变输入。

### 78.5 调用方向与验证计划

```text
Wiring/Tile event -> TileAnimationTriggerAdapter
  -> TemporaryAnimationQueueSystem -> TileAnimationFrameSystem
  -> TileAnimationNetworkProjection / visual projection

Gameplay event -> ParticleSpawnCommand -> ParticleSpawnRoutingSystem
  -> ParticleReplicationAdapter -> Behind/OverPlayers ParticleRenderer
  -> ParticleUpdateAndPoolSystem -> ParticleDrawProjection

SceneMetrics/Weather snapshot -> HorizonLayerProjection -> HorizonRendererAdapter
```

最小 focused verifier 应覆盖：临时动画坐标/Tile 类型校验、同坐标重复入队、队列批处理和
过期清理、网络截断/未知动画类型；粒子 `clientOnly` 分支、调用玩家越界、枚举未知值、包
长度、广播权限、对象池归还和场景清空；地平线空缓存、绘制顺序和天气快照只读性。当前
均没有运行证据，标记为 `未验证`。

### 78.6 本轮新增边界与统计

本轮新增约 **14 个**边界（临时 Tile 动画触发/队列/帧推进/网络投影、粒子目录/命令/路由/
复制/渲染器/更新对象池/排斥查询/绘制投影、地平线绘制适配）。以报告上一节登记的约 475 个
为基线，累计独立边界约 **489 个**；至少约 **411 个**仍有临时动画消费、粒子直接生成、
网络权限与长度校验、粒子清空/绘制、地平线渲染调用链缺口。静态识别覆盖率约 **98%～99%**，
权威行为闭合度约 **13%～22%**。本轮只读取 `D:\TRbackup\Version4`，未修改其源码，并继续
排除 `D:\TRbackup\NLTX\src` 中已有组件。

## 81. 第五十七轮：Tile 命中裂纹与拾取 PopupText 的只读反馈边界

### 81.1 HitTile 是每名玩家的短期裂纹池，而非 Tile 耐久或破坏权威状态

`Player` 为自身持有 `hitTile` 与 `hitReplace` 两个 `HitTile` 实例，并在初始化时创建它们
（`Terraria/Player.cs:1158-1160,26567-26568`）。每个 `HitTile` 预分配 501 个
`HitTileObject` 及对应顺序数组；单个对象保存 Tile 坐标、累计 damage、目标类型（Tile/Wall）、
寿命、裂纹样式、动画时间和方向（`Terraria/HitTile.cs:9-80`）。`Clear` 会重置主要字段，并
以随机源挑选一个不同于上次的四种裂纹样式（`:22-55`）。这证明其数据描述的是“命中视觉
历史”，并非 Tile 的持久生命值。

Projectile 频繁调用 `Collision.HitTiles/HitTilesInACircle`；二者枚举碰撞范围中的实体固体 Tile，
经矩形或圆形相交检查后调用 `WorldGen.KillTile(i, j, fail: true, effectOnly: true)`
（`Terraria/Collision.cs:2250-2344`、`Projectile.cs` 多处调用）。`effectOnly: true` 明确说明
该路径应仅产生敲击/碎裂反馈，不能把它当作破坏 Tile、掉落物或地图脏标记的提交入口。
本版未找到 `HitTile.data` 的登记、伤害累计、寿命推进、排序、回收或绘制消费者；因此即使
对象池和碰撞触发存在，裂纹显示链仍不闭合。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `TileHitFeedbackComponent` | 玩家局部的坐标、类型、裂纹样式、寿命和动画数据 | `HitTileObject` 字段；`confirmed/partial` |
| `TileHitFeedbackPool` | 501 槽容量、顺序分配、重置与随机样式去重 | `HitTile.cs:57-80`、`Clear`；`partial` |
| `CollisionContactQuery` | 从 Projectile 位移、碰撞盒和 Tile 几何得出受击 Tile 集 | `Collision.HitTiles*`；`confirmed/partial` |
| `TileHitFeedbackCommand` | 将 effect-only 命中变成局部裂纹请求，而非 Tile 破坏命令 | `WorldGen.KillTile(... effectOnly: true)`；`partial` |
| `TileHitFeedbackLifecycleSystem` | 累积伤害、TTL、淘汰、排序与世界切换清理 | 当前无消费者；`missing` |
| `TileHitCrackProjection` | 在 Tile 上绘制裂纹/碎屑，不写回 Tile 状态 | `animationDirection`、`crackStyle`；`missing` |

真正的 Tile 破坏仍应沿既有 `TileMutationSystem`/掉落/网络路径提交。这里的随机裂纹样式
必须是客户端表现随机性，不能影响破坏判定、掉落或世界存档。

### 81.2 PopupText 是库存结果的客户端投影池，创建与动画实现被截断

`PopupText` 静态管理 20 个槽位和活跃数，单条保存位置、速度、透明度、寿命、物品名/堆叠、
硬币价值、专家/大师/声纳标志、逐字符颜色偏移与效果样式
（`Terraria/PopupText.cs:10-72`）。`Main` 初始化每个槽位；世界清理时 `WorldGen` 调用
`PopupText.ClearAll`，后者重建 20 个对象并重置活跃数
（`Terraria/Main.cs:3503`、`WorldGen.cs:6622`、`PopupText.cs:370-378`）。

`Player` 仅在已有物品拾取/堆叠流程之后调用 `PopupText.NewText`，例如 `GetItem` 将物品加入
库存后以 `RegularItemPickup` 上报（`Terraria/Player.cs:22970-23003`）。这确立了单向关系：
库存提交是权威结果，PopupText 只是结果通知。可惜 `NewText` 在可见性与名称早退检查后直接
返回 -1，其原有的合并、选槽、位置、寿命、硬币换算和效果准备逻辑被整体注释；
`PrepareEffects`、`PrepareDisplayText`、`AddToCoinValue`、`FindNextItemTextSlot` 和实例
`ValueToName` 也为空（`PopupText.cs:106-310`）。唯一闭合的格式化是静态
`ValueToName(long)`，它把铜币数拆为铂金/金币/银币/铜币本地化文本（`:312-367`）。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `PickupFeedbackComponent` | 物品/硬币文本、位置、速度、寿命、颜色与上下文 | `PopupText.cs:10-72`；`confirmed/partial` |
| `PickupFeedbackPool` | 20 槽限制、活跃计数、声纳专用槽和世界清理 | `popupText`、`ClearSonarText`、`ClearAll`；`partial` |
| `PickupFeedbackMergeQuery` | 同物品/硬币的 NoStack、可合并与寿命延长判定 | `NewText` 注释逻辑；`missing` |
| `PickupFeedbackSpawnSystem` | 从已提交的库存结果选择/初始化槽位 | `Player.cs:22970-23003`、`NewText`；`partial/missing` |
| `CoinDisplayFormatQuery` | 只读铜币数到本地化面额文本转换 | `PopupText.ValueToName(long)`；`confirmed` |
| `PickupFeedbackAnimationSystem` | 位移、淡入淡出、字符效果和 TTL 更新 | 字段存在但无更新消费者；`missing` |
| `PickupFeedbackDrawProjection` | 将活动文本绘制为 UI/世界提示 | 当前无 Draw 消费者；`missing` |

### 81.3 调用方向与最新统计口径

```text
Projectile collision -> CollisionContactQuery -> TileHitFeedbackCommand
  -> TileHitFeedbackPool/Lifecycle -> TileHitCrackProjection
  (不得进入 TileMutationSystem)

Inventory pickup commit -> PickupFeedbackSpawnSystem -> PickupFeedbackPool
  -> merge/animation -> PickupFeedbackDrawProjection
  (不得回写库存、掉落或商店货币)
```

最小 focused verifier 应覆盖：高速 Projectile 的 Tile 范围裁剪、圆形和矩形接触差异、
`effectOnly` 不改变 Tile/掉落/地图队列；裂纹池满载时的淘汰、TTL 和随机样式不连续重复；
PopupText 关闭显示时不占槽、同类硬币合并不溢出、NoStack 不合并、世界切换清空、格式化大额
货币无损，以及所有反馈失败均不影响已提交库存。当前没有运行 verifier 证据，均标记为
“未验证”。

本轮新增约 **13 个**边界（Tile 命中反馈状态/池/接触/命令/生命周期/投影 6，拾取反馈状态/
池/合并/生成/格式化/动画/绘制 7）。结合第 70、71 节的新增发现，当前累计独立边界约
**541 个**；至少约 **463 个**仍有生成/消费、权限校验、网络复制或副作用提交缺口。静态
识别覆盖率保持约 **98%～99%**；权威行为闭合度约 **12%～21%**。这些数字是静态源码阅读
估计，不代表 `Version4` 已可作为完整可运行的权威模拟。本轮只读取
`D:\TRbackup\Version4`，未修改其源码，并继续排除 `D:\TRbackup\NLTX\src` 中已有组件。

## 82. 第五十八轮：玩家伤害/死亡归因、死亡结果与跨网络文本投影

### 82.1 PlayerDeathReason 是可复制的归因快照，不是可变伤害状态

`PlayerDeathReason` 在同一对象中可携带来源 Player、NPC、Projectile 本地索引、其它死亡类型、
Projectile 类型、选中物品类型/Prefix 和自定义文字；`ByNPC`、`ByOther`、`ByProjectile`
将这些字段组装为死亡归因（`Terraria.DataStructures/PlayerDeathReason.cs:6-72`）。其中
`ByProjectile` 会在来源玩家索引为 0..255 时读取其当前选中物品类型与 Prefix，因此它是一次
创建时的审计快照，不能在死后再从可变 inventory 反推攻击来源。

该类型使用一个 `BitsByte` 写出字段存在位，按位顺序序列化可选字段，`FromReader` 以相同顺序
恢复（`:84-165`）；`GetDeathText` 将自定义文本或结构化来源交给 `Lang.CreateDeathMessage`
（`:73-82`）。这已经是具体的网络 DTO/文本投影输入，但当前反序列化没有索引范围、未知
other 类型、字符串长度或 Projectile 类型有效性校验，不能把它直接作为服务器授权的伤害证据。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `DamageAttributionComponent` | 攻击者/目标类型、Projectile/物品快照和其它原因码 | `PlayerDeathReason` 字段与工厂；`confirmed` |
| `DamageAttributionSnapshotFactory` | 在命中时冻结 Projectile、玩家物品与 Prefix | `ByNPC/ByOther/ByProjectile`；`confirmed/partial` |
| `DeathReasonCodecAdapter` | BitsByte 编解码、长度/范围/版本校验 | `WriteSelfTo`、`FromReader`；`partial` |
| `DeathMessageProjection` | 将已提交死亡归因转换为本地化 NetworkText | `GetDeathText`；`confirmed` |

### 82.2 Hurt 具有真实资源提交，但资格和表现副作用仍相互缠绕

`Player.Hurt` 先处理 Shimmer 闪避、旅途无敌、一般或专属免疫冷却；随后计算防御、暴击、
endurance、Solar/Beetle 减伤和 Paladin 团队分摊，写入生命、免疫计时、再生计时、击退、
状态/坐骑变化，并触发 CombatText、音效、Dust 和眼睛眨动
（`Terraria/Player.cs:22208-22505`）。这说明真实权威写入是生命、免疫、Buff、坐骑和速度，
而 CombatText、粒子、音效均应被提取为提交后的 Projection。

Paladin 分摊在当前方法内遍历所有玩家、按距离选择同队护盾持有者，并向本地守护者再次调用
`Hurt(ByOther(20), ..., ImmunityCooldownID.PaladinsShield)`；它还生成粒子
（`:22280-22319`）。这不能被简化成“受伤时固定减伤”：它需要明确的队友资格、距离选择、
递归冷却隔离和二次伤害命令。`AllowShimmerDodge` 为空，故 Shimmer 闪避的实际资格规则仍为
`missing`。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `PlayerDamageQualificationQuery` | 无敌、免疫槽、PVP、Shimmer 闪避与可闪避条件 | `Hurt` 前置分支、`AllowShimmerDodge`；`partial/missing` |
| `PlayerDamageCalculationQuery` | 防御、暴击、耐久、Solar/Beetle 减伤和最小伤害 | `Hurt` 计算段；`confirmed/partial` |
| `PaladinDamageShareQuery` | 同队/存活/血量/距离资格和最近守护者选择 | `CanDefendWithPaladinsShield`、`Hurt`；`confirmed/partial` |
| `PlayerDamageCommitSystem` | 原子写生命、免疫、再生、击退、Buff/坐骑结果 | `Hurt`；`confirmed/partial` |
| `SharedDamageCommand` | 以独立冷却槽提交守护者的二次伤害 | `Hurt(ByOther(20), ...)`；`partial` |
| `DamageFeedbackProjection` | CombatText、声音、Dust、眼睛与粒子表现 | `Hurt`、`PlayHurtSound`；`partial` |

### 82.3 KillMe 同时改变死亡、掉落、复活和社交通知，需分阶段提交

`Player.KillMe` 防止重复死亡/练习模式重置后，写 PVP/PVE 死亡计数、最后死亡位置和时间；它
计算金币、执行 `DropItems`、设 `dead`、设置复活计时、广播死亡聊天、根据难度掉币、返还
特殊槽物品并放置墓碑（`Terraria/Player.cs:22572-22691`）。`GetRespawnTime` 还会根据附近 Boss、
专家模式、特殊种子及其他玩家状态调整时间（`:22722` 起）。这是一条多阶段状态转换，不能由
聊天或网络接收回调直接零散修改。

其中 Dust/Gore、死亡音效、`ChatHelper.BroadcastChatMessage` 都是外部投影；掉落、墓碑、死亡
计数、复活计时才是权威提交。`DropItems`、`DropCoins`、`DropTombstone` 的原子次序、失败补偿、
断线重连恢复和存档边界本轮尚未沿所有调用点闭合，保留为 `partial`。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `PlayerDeathTransitionSystem` | 防重入、死亡标志、统计、最后死亡位置与复活计时 | `KillMe`；`confirmed/partial` |
| `RespawnDelayQuery` | Boss 距离、难度、特殊种子和在线玩家下的重生时间 | `GetRespawnTime`；`partial` |
| `DeathInventoryDropCommand` | 物品、硬币、特殊槽返还和墓碑生成的事务意图 | `DropItems/DropCoins/DropTombstone`；`partial` |
| `DeathWorldCommitSystem` | 原子提交掉落/墓碑与世界实体索引 | `KillMe` 调用链；`partial` |
| `DeathPresentationProjection` | 死亡文本、音效、Dust/Gore 和成就通知 | `KillMe`、`KillMe_DustExplosion`；`partial` |
| `RespawnLifecycleSystem` | 计时推进、出生点选择、死亡状态解除与网络同步 | `respawnTimer`、既有出生流程；`partial` |

### 82.4 消息 117/118 是授权风险 seam，不应把客户端归因直接提交

`NetMessage.SendPlayerHurt/SendPlayerDeath` 将全局当前 `PlayerDeathReason` 写进 117/118 包，
随后写受害者索引、伤害、方向和 PVP/暴击等字段（`Terraria/NetMessage.cs:1420-1440,2338-2356`）。
`MessageBuffer` 接收 117 后仅以来源玩家等于目标、或双方 hostile 为条件，反序列化归因并直接
调用 `Main.player[num24].Hurt(...)` 再广播；接收 118 时将目标改为发送者本人，但同样直接读取
归因、伤害、方向和 PVP 位后执行 `KillMe`/广播（`Terraria/MessageBuffer.cs:2941-2972`）。

所以现有实现将客户端提供的伤害数值、死亡归因和部分 PVP 语义直接带入状态变更。即便这里的
索引条件并不等价于“任意伤害目标”，服务器仍需要从权威碰撞、Projectile/NPC、团队和冷却
状态重新计算结果，至少拒绝不存在的来源索引、超限伤害、无命中链、重放序号和不匹配的
Projectile/物品快照。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `InboundDamageIntentAdapter` | 解码 117/118，限制字段大小并保留原始会话来源 | `MessageBuffer.cs:2941-2972`；`partial` |
| `DamageAuthorityQuery` | 对来源、目标、PVP、命中、冷却和伤害值做服务器重算 | 当前直接调用 `Hurt/KillMe`；`missing` |
| `DamageReplicationProjection` | 只广播已提交的最终伤害/死亡快照 | `SendPlayerHurt/SendPlayerDeath`；`partial` |
| `DamageReplayProtectionSystem` | 包序列、tick 窗口和重复命中拒绝 | 当前未发现；`missing` |

### 82.5 调用方向与验证 seam

```text
NPC/Projectile/environment hit -> AttributionSnapshotFactory
  -> DamageQualification + Calculation (+ PaladinDamageShareQuery)
  -> PlayerDamageCommitSystem -> DamageReplication / Feedback projections
life <= 0 -> PlayerDeathTransitionSystem -> DeathInventoryDropCommand
  -> DeathWorldCommitSystem -> RespawnLifecycle + chat/audio/visual projections
network 117/118 -> InboundDamageIntentAdapter -> DamageAuthorityQuery
  -> same commit systems; never direct client values to state writes
```

最小 focused verifier 应覆盖：所有归因字段组合与编解码往返、截断/超长文本/未知枚举拒绝；
无敌、专属冷却、Shimmer、Solar/Beetle、Paladin 最近目标和二次伤害不递归；死亡重入、
掉落失败回滚、复活计时与墓碑一致性；以及伪造来源、越界索引、超大伤害、无碰撞伤害和重放
117/118 包均不能改变权威状态。当前没有这些运行 verifier 证据，均标记为“未验证”。

本轮新增约 **20 个**边界（归因状态/快照/编解码/文本 4，伤害资格/计算/分摊/提交/二次命令/
反馈 6，死亡/复活/掉落/世界提交/表现 6，网络意图/授权/复制/重放 4）。累计独立边界约
**561 个**；至少约 **481 个**仍有来源校验、授权重算、掉落原子性、复活或网络重放缺口。
静态识别覆盖率保持约 **98%～99%**；权威行为闭合度约 **12%～21%**。本轮只读取
`D:\TRbackup\Version4`，未修改其源码，并继续排除 `D:\TRbackup\NLTX\src` 中已有组件。

## 83. 第五十九轮：NPC Debuff 免疫目录、状态槽提交与同步授权

### 83.1 DebuffImmunitySets 是按 NPC 类型的内容策略目录，不是实例 Buff 数组

`NPCID.Sets.DebuffImmunitySets` 是 `Dictionary<int, NPCDebuffImmunityData>`；每个 NPC type 可
声明“免疫所有鞭类 Debuff”“免疫所有非鞭 Debuff”或具体 Buff ID 数组。目录中可见大量按
NPC type 的显式条目，例如 type 1 对 Buff 20 免疫、type 4–12 对 Buff 31 免疫
（`Terraria.ID/NPCID.cs:390` 起）。`NPCDebuffImmunityData.ApplyToNPC` 先根据 `BuffID.Sets.
IsAnNPCWhipDebuff` 为所有 Buff 分类赋值，再叠加具体免疫 ID（`Terraria.DataStructures/
NPCDebuffImmunityData.cs:5-37`）。这应被建模为不可变内容策略；`npc.buffImmune[]` 才是某个
NPC 实例的运行时资格表。

NPC 初始化在 SetDefaults/生成配置完成、生命和默认属性写入之后读取该目录；无目录项时重置
整个 `buffImmune` 数组，之后建立 Buff 20→30/375、69→36 等派生免疫，并从
`NPCID.Sets.ShimmerImmunity[type]` 写入 353（`Terraria/NPC.cs:17518-17548`）。特殊种子调整、
可捕获状态和难度 ScaleStats 随后才运行。因此“type 免疫定义”“实例运行时免疫”和“世界/
种子临时免疫”不能合并成一张可变表；重置次序也是 NPC 转换、池槽复用和网络重放的重要 seam。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `NpcDebuffImmunityDefinitionCatalog` | Type 到全类/具体 Buff 免疫策略的只读目录 | `NPCID.Sets.DebuffImmunitySets`；`confirmed` |
| `NpcDebuffClassMembershipCatalog` | 判断 Buff 是否属于鞭类 Debuff | `BuffID.Sets.IsAnNPCWhipDebuff`；`confirmed/partial` |
| `NpcRuntimeImmunityComponent` | 当前实例的 `buffImmune[]` 资格位图 | `NPC.cs:17526-17548`；`confirmed` |
| `NpcImmunityInitializationSystem` | SetDefaults 时重置、应用目录、派生免疫与 Shimmer 覆盖 | `NPC.cs:17518-17548`；`confirmed/partial` |
| `NpcImmunityOverrideCommand` | 种子、转换或临时玩法规则对运行时免疫的显式覆盖 | seed 调整/Transform 调用链；`partial` |

目录加载/注册阶段应拒绝负数、超过 `BuffID.Count` 的 ID 和相互矛盾的定义；当前
`ApplyToNPC` 对 `SpecificallyImmuneTo` 未做范围检查，错误目录会直接访问实例数组。

### 83.2 AddBuff/DelBuff 是有序槽位事务，并同时承担网络副作用

`NPC.FindBuffIndex` 先用 `buffImmune[type]` 拒绝，再查找现有槽位；`AddBuff` 若已有相同 Buff
且剩余时间更长则保持原状，否则查找/腾出槽位、写入 `buffType/buffTime`，并在非 quiet 时发送
消息 54（`Terraria/NPC.cs:76372-76440`）。当没有空位时，它寻找第一个非 Debuff 槽位并删除，
再重新分配；`DelBuff` 清零后前移压缩全部槽位并同样发送消息 54（`:76462-76480`）。

这不是简单的“给 NPC 加 Buff”：槽位替换优先级、相同 Buff 的时间单调性、压缩顺序与复制
时机都会影响权威状态。网络和声音/粒子不能嵌在数组写入中；建议事务先返回完整差异，再由
复制 Adapter 发送最终快照。当前 `NetMessage` 的 54 号序列化/接收内容未在此实现展开，故
完整稀疏槽位复制仍只能标记 `partial`。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `NpcBuffQualificationQuery` | 根据免疫、Buff ID、时长及现有槽位决定是否接受 | `FindBuffIndex`、`AddBuff`；`confirmed/partial` |
| `NpcBuffSlotSelectionQuery` | 同类覆盖、空槽查找、非 Debuff 驱逐与顺序压缩决策 | `AddBuff/DelBuff`；`confirmed/partial` |
| `NpcBuffMutationSystem` | 原子写入/续时/清除 Buff 槽，并生成变更差异 | `AddBuff/DelBuff`；`confirmed/partial` |
| `NpcBuffRemovalQualificationQuery` | 限制可由远程请求移除的 Buff 类型 | `RequestBuffRemoval`、`BuffID.Sets.CanBeRemovedByNetMessage`；`confirmed` |
| `NpcBuffReplicationAdapter` | 消息 54 的最终状态快照和客户端应用 | `AddBuff/DelBuff` 的 SendData 54；`partial` |

### 83.3 消息 53/137 证明了两个不同的信任边界

消息 53 解码 NPC 索引、Buff 类型和持续时间后，直接执行
`Main.npc[num176].AddBuff(type17, time2, quiet: true)` 并广播 54；当前代码在访问数组前没有对
`num176`、`type17` 或时间做范围与权限验证（`Terraria/MessageBuffer.cs:2007-2017`）。消息 137
则先验证 NPC 索引，交给 `RequestBuffRemoval`，后者会验证 Buff ID 位于 `BuffID.Count` 内且
`CanBeRemovedByNetMessage` 允许（`MessageBuffer.cs:3166-3176`、`NPC.cs:76442-76460`）。

两者不能合为一个泛化“Buff 网络同步”：消息 53 是外部施加意图，应由服务器重新验证命中、
来源、免疫、时长和管理员/世界事件权限；消息 137 是受限删除请求，必须额外验证该客户端是否
有权修改此 NPC，而不仅是 Buff 类型可删除。消息 55 又是玩家 PVP Buff 的独立协议，且只在
服务器或 `Main.pvpBuff[type]` 时转发（`MessageBuffer.cs:2018-2029`），不属于 NPC Buff
生命周期。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `InboundNpcBuffIntentAdapter` | 解码 53，检查索引/类型/时长/包长度并绑定来源会话 | `MessageBuffer.cs:2007-2017`；`partial/missing` |
| `NpcBuffAuthorityQuery` | 从命中、物品/Projectile、世界事件或管理员权限重算施加资格 | 当前直接 AddBuff；`missing` |
| `InboundNpcBuffRemovalAdapter` | 解码 137 并保留客户端、目标和请求类型 | `MessageBuffer.cs:3166-3176`；`partial` |
| `NpcBuffRemovalAuthorityQuery` | 校验请求者对该 NPC 和该 Buff 的修改权 | `RequestBuffRemoval` 仅验证 Buff 类型；`missing` |
| `NpcBuffReplicationProjection` | 仅传播已提交的槽位差异/快照 | 54 号回传；`partial` |

### 83.4 旁路审计：意图猜测和花包目录尚不足以成为权威子系统

`Player.IntentionGuesser.Update` 每 tick 对本地玩家调用，但除“仅 Main.myPlayer”过滤外没有
实现；`HarvestTreasure/HarvestTrees` 枚举、鼠标/位置历史和 SmartCursor 使用代理只是未消费的
本地预测壳（`PlayerIntentionGuesser.cs:6-40`、`Player.cs:14980`）。
`PlayerGetItemLogger` 也只在拾取已提交后、`_enabled` 为真时记录目标数组、槽位、上下文和
堆叠数，而当前未发现开启/导出路径（`PlayerGetItemLogger.cs:5-38`、`Player.cs:22967-23000`）。
两者应暂留为 `missing` 的调试/客户端诊断 Projection，不能用于服务器采集、掉落或智能游标
权威判定。

同理，`ItemID.Sets.flowerPacketInfo` 已登记不同花包在 Purity/Corruption/Crimson/Hallow 的样式
列表（`Terraria.ID/ItemID.cs:171-226`），但 Version4 内未找到这些四个列表的实际消费者；它是
未闭合的内容目录，不能在缺少放置、生态资格与 Tile 提交证据时另立完整生态系统。

### 83.5 调用方向与验证 seam

```text
Npc type + world rules -> ImmunityDefinitionCatalog -> InitializationSystem
  -> NpcRuntimeImmunityComponent
Buff hit/event -> NpcBuffQualificationQuery + SlotSelectionQuery
  -> NpcBuffMutationSystem -> NpcBuffReplicationAdapter
network 53/137 -> inbound adapter -> authority query -> same mutation system
```

最小 focused verifier 应覆盖：全类/具体/派生/Shimmer 免疫的初始化顺序和池槽复用；未知 Buff ID、
负时间、最大时长、满槽驱逐、同 Buff 更短时间和删除压缩；53/137 的越界索引、客户端越权、
未知类型、重放和 54 快照一致性。还应证明 IntentionGuesser、GetItemLogger 和花包目录在未闭合
前绝不写回玩家、NPC、Tile、掉落或网络权威状态。当前没有这些运行 verifier 证据，均标记为
“未验证”。

本轮新增约 **16 个**边界（免疫目录/分类/实例/初始化/覆盖 5，Buff 资格/选槽/变异/删除/
复制 5，入站/授权/删除入站/删除授权/投影 5，另含调试投影隔离 seam）。累计独立边界约
**577 个**；至少约 **496 个**仍有目录校验、槽位复制、网络授权、命中归因或未消费内容路径。
静态识别覆盖率保持约 **98%～99%**；权威行为闭合度约 **12%～21%**。本轮只读取
`D:\TRbackup\Version4`，未修改其源码，并继续排除 `D:\TRbackup\NLTX\src` 中已有组件。

## 84. 第六十轮：光照源收集、局部传播缓存与客户端亮度投影

### 84.1 Lighting 是客户端派生光场 Facade，不能反向充当世界规则

`Lighting` 持有 Color 模式的 `LightingEngine` 与 White/Retro/Trippy 模式的
`LegacyLighting`，通过 `Mode` 选择活跃实现、调整 `OffScreenTiles` 并重置渲染标志
（`Terraria/Lighting.cs:10-58`）。三个 `AddLight` 重载会把世界坐标转换为 Tile 坐标或查询
`TorchID.TorchColor`；`GetColor` 则读取活动引擎的 `Vector3`，乘 `GlobalBrightness`、钳制为
RGB 并输出 `Color`（`:61-122`）。Main 在渲染/场景更新路径调用 `Lighting.Clear()`
（`Terraria/Main.cs:12311`）。

WorldItem、Player、NPC、Projectile、装甲套装与临时效果的大量更新分支调用
`Lighting.AddLight`；这些调用提供位置和颜色，属于已提交实体状态的表现输入，不能向实体
反向写生命、Tile、天气、Buff 或掉落。即使某些玩法规则使用“黑暗”概念，也应消费独立的
服务器场景/危险快照，而不能从本地色彩模式或屏幕外裁剪后的光图推导权威伤害。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `LightRenderingModeComponent` | 当前光照模式、全局亮度和屏幕外 Tile 范围 | `Lighting.Mode`、`GlobalBrightness`；`confirmed` |
| `DynamicLightSourceCommand` | 已提交实体生成的 Tile 坐标、RGB 和本帧来源 | `Lighting.AddLight` 的大量 Player/NPC/Projectile/Item 调用；`confirmed/partial` |
| `DynamicLightSourceCollector` | 同帧收集、合并与容量限制，和世界状态隔离 | `LightingEngine._perFrameLights`；`partial` |
| `LightColorProjection` | 将光图颜色、亮度和菜单规则投影为渲染颜色 | `Lighting.GetColor`；`confirmed/partial` |

### 84.2 新旧引擎都保存局部缓存结构，但核心传播算法缺失

`ILightingEngine` 明确要求 `Rebuild`、`AddLight`、`ProcessArea`、`GetColor`、`Clear` 五个
生命周期操作（`Terraria.Graphics.Light/ILightingEngine.cs:5-17`）。Color 引擎持有当前/工作
两张 `LightMap`、本帧与旧帧光源列表、Tile 扫描器，以及 28 Tile 的区域 padding
（`LightingEngine.cs:8-40`）。Legacy 引擎另持有 2,000 个临时光的设计常量、扫描器、光图和
模式字段（`LegacyLighting.cs:8-69`）。

但 `LightingEngine` 的 `AddLight/Clear/GetColor/ProcessArea/Rebuild` 均为空或默认返回；
`LegacyLighting` 的对应实现也为空（`LightingEngine.cs:42-49`、`LegacyLighting.cs:71-76`）。
`TileLightScanner` 目前只保留随机源，没有 Tile/Liquid/Wall 扫描逻辑
（`TileLightScanner.cs:8-11`）。因此有清晰的接口、双缓冲与光源提交者，却没有光传播、异步
作业、交换时机或区域失效的完成证据。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `LightMapComponent` | 局部 RGB 数组、遮罩和介质衰减常量 | `LightMap.cs:7-55`；`confirmed/partial` |
| `LightMapIndexQuery` | 局部坐标到数组索引的边界验证 | `LightMap.IndexOf`；`missing` |
| `TileLightMaterialQuery` | 从只读 Tile/Wall/Liquid 解析遮蔽、裂砖、湿润/蜂蜜与自发光 | `TileLightScanner`、`LightMaskMode`；`missing` |
| `LightPropagationSystem` | 扫描区、衰减、临时光合成与颜色传播 | `ProcessArea/Rebuild`；`missing` |
| `LightBufferSwapSystem` | 工作/活动光图交换、旧光源回收与线程可见性 | `_activeLightMap/_workingLightMap/_oldPerFrameLights`；`missing` |
| `LightCacheClearSystem` | 场景/模式/世界切换时安全清理所有局部缓存 | `Lighting.Clear`、引擎 `Clear`；`partial/missing` |

`LightMap` 已声明空气、固体、裂砖、水和蜂蜜的衰减系数及默认 203×203 容量；但
`IndexOf` 返回默认整数，任何坐标边界、正确 stride 和掩码一致性都未闭合
（`LightMap.cs:18-64`）。这些系数是表现算法配置，不能与液体权威数量或 Tile 实体状态合并。

### 84.3 光照模式切换是客户端 Adapter；不应复制到服务器模拟

Color 模式选择新引擎且设置 `LegacyEngine.Mode=0`，其它模式都选 Legacy 引擎并修改其模式；
每次切换重置 `Main.renderCount/renderNow`（`Lighting.cs:23-53`）。它代表可选的客户端渲染
策略，而非世界会话字段：两个客户端可以使用不同模式而看到不同画面，但不能因而得到不同
碰撞、可挖掘性、NPC 行为或掉落。

建议保留如下隔离：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `LightingModePreferenceAdapter` | 读取本地设置并切换 Color/White/Retro/Trippy 后端 | `Lighting.Mode`；`confirmed/partial` |
| `LightingRenderInvalidationProjection` | 仅请求重新绘制，不能重算世界规则 | `Main.renderCount/renderNow`；`confirmed` |
| `LightingReadbackQuery` | 向 Tile/实体绘制读取局部 RGB，不暴露可变光图 | `Lighting.GetColor`、`ILightingEngine.GetColor`；`partial` |

### 84.4 调用方向与验证 seam

```text
Committed Player/NPC/Projectile/Item snapshot
  -> DynamicLightSourceCommand -> DynamicLightSourceCollector
  -> TileLightMaterialQuery + LightPropagationSystem
  -> LightBufferSwapSystem -> LightColorProjection -> renderer

Local preference -> LightingModePreferenceAdapter -> render invalidation
```

最小 focused verifier 应覆盖：世界坐标到 Tile 坐标的负值/边界裁剪、同 Tile 多源颜色合成、
不同模式切换不写世界状态、世界卸载后无旧光源、203×203 索引不越界、双缓冲不会读到半帧，
以及黑暗玩法判定完全不读取客户端 `GetColor`。当前无光传播或线程运行证据，均标记为
“未验证”。

本轮新增约 **15 个**边界（模式/动态源/收集/颜色投影 4，光图/索引/材料/传播/交换/清理 6，
偏好/失效/只读查询 3，另含本地与权威隔离 seam）。累计独立边界约 **592 个**；至少约
**511 个**仍有光图索引、Tile 扫描、传播、缓存交换、网络授权或调用消费缺口。静态识别覆盖率
保持约 **98%～99%**；权威行为闭合度约 **12%～21%**。本轮只读取
`D:\TRbackup\Version4`，未修改其源码，并继续排除 `D:\TRbackup\NLTX\src` 中已有组件。

## 85. 第六十一轮：洞穴房屋配额、地下结构候选与 FloodFill 空腔写入

### 85.1 CaveHouseBiome 是地下房屋的结构命令生产者，但 Place 本体缺失

`CaveHouseBiome` 持有跨放置的 `HouseBuilderContext`，并从配置读取 Ice/Jungle/Gold/Granite/
Marble/Mushroom/Desert 七类宝箱概率（`Terraria.GameContent.Biomes/CaveHouseBiome.cs:8-36`）。
`HouseBuilderContext` 当前只保存 Sharpener 与 Extractinator 已放置数
（`Terraria.GameContent.Biomes.CaveHouse/HouseBuilderContext.cs:3-7`），因此它是生成阶段的配额
状态，而不是世界中每幢房子的实体组件。`CaveHouseBiome.Place` 却直接返回默认值，没有进行
候选扫描、结构预留、房间/走廊写入、墙/家具/宝箱提交、战利品选择或计数递增。

`UndergroundHousesAndBuriedChests` 世界生成 pass 确实创建此 Biome，从配置读取 CaveHouse、
UnderworldChest、CaveChest 与 AdditionalDesertHouse 数量，在每种候选失败时消耗重试预算并
重新尝试（`Terraria/WorldGen.cs:16090-16225`）。普通地下房屋的候选深度会避开海洋、地牢墙
与 dual-dungeon 边界；特殊种子可按 `HouseUtils.GetMaxPossibleRoomsInABigAbandonedHouse()`
提高最小深度。因而调度与空间排除真实存在，但“洞穴房屋成功”不能从该循环推断。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `CaveHouseGenerationConfig` | 七类宝箱概率及房屋/宝箱数量范围 | `CaveHouseBiome`、WorldGen passConfig；`confirmed/partial` |
| `CaveHouseGenerationQuotaComponent` | Sharpener/Extractinator 已放置数及一次生成过程的限制 | `HouseBuilderContext`；`partial` |
| `UndergroundStructureCandidateQuery` | 深度、海洋、地牢墙、dual-dungeon 与特殊种子边界资格 | `WorldGen.cs:16118-16210`；`confirmed/partial` |
| `UndergroundStructureRetryPolicy` | 每种结构的尝试上限、失败消耗与进度计算 | `num5`、循环及 `progress.Set`；`confirmed/partial` |
| `CaveHousePlacementPlanQuery` | 房间尺寸、材料、家具、宝箱类型、配额与 Structures 预检 | `CaveHouseBiome.Place`；`missing` |
| `CaveHouseStructureCommitSystem` | 原子写 Tile/Wall/家具/Chest、登记结构并递增配额 | `CaveHouseBiome.Place`；`missing` |
| `CaveHouseLootCommand` | 依据环境概率提交宝箱内容而非直接写 inventory | 七类 chest chance；`missing` |

生成失败必须不留下半写 Tile、已占 Structures 矩形、空 Chest 索引或错误配额。`HouseUtils`
中的黑名单/蜂类 Tile 集合目前没有发现消费者；它们只能作为候选规则目录，不能据此假定
蜂巢/房屋冲突规则已经生效。

### 85.2 ShapeFloodFill 为自然空腔/墙体生成提供边界探索，但核心遍历为空

`ShapeFloodFill` 是 `GenShape`，持有最大动作数（默认 100），却在 `Perform(origin, action)`
中直接返回默认值（`Terraria.GameContent.Generation/ShapeFloodFill.cs:8-19`）。WorldGen 在地下
草/墙生成阶段调用 `WorldUtils.Gen`，以 `ShapeFloodFill(1000)` 从空 Tile 开始，经
`Modifiers.IsNotSolid`、`Actions.Blank().Output(shapeData)`、邻接 Tile 条件和自定义无效标志
生成空腔集合；只有集合超过 50、FloodFill 成功且未碰到禁用邻居时，才对外轮廓写入墙
（`Terraria/WorldGen.cs:15830-15872`）。

所以 WorldGen 已把 FloodFill 输出作为“候选空腔快照”，再用 `OuterOutline` 与 `PlaceWall`
提交；遍历本身不能写世界。由于 `Perform` 缺失，当前既无法证明四/八邻接策略、访问去重、
边界裁剪、最大动作耗尽语义，也无法证明 `shapeData.Count` 对大洞穴的真实性。

建议拆分：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `FloodFillTraversalBudgetComponent` | 最大动作数、已访问数和耗尽结果 | `ShapeFloodFill._maximumActions`；`partial` |
| `FloodFillCandidateQuery` | 在只读 TileMap 上寻找连通非实体空腔 | `WorldUtils.Gen` + `IsNotSolid`；`partial/missing` |
| `FloodFillNeighborQuery` | 邻接方向、边界、去重及禁用 Tile 触达判断 | `Perform`、`IsTouching` 条件；`missing` |
| `CavityShapeSnapshot` | 通过 `ShapeData` 传递的候选坐标集合 | `Actions.Blank().Output(shapeData)`；`confirmed/partial` |
| `CavityWallPlacementCommand` | 仅对通过资格的 Outline 写墙，跳过指定墙类型 | `OuterOutline`、`SkipWalls`、`PlaceWall`；`partial` |

### 85.3 洞穴房屋和 FloodFill 的写入顺序必须通过 StructureMap 交接

洞穴房屋把 `GenVars.structures` 传入 `CaveHouseBiome.Place`，自然空腔墙体阶段则只对候选
`ShapeData` 作后续写入。两条路径都应遵循：候选 Query 只读世界；`StructureMap` 预留先于
Tile/Wall/Chest 写入；所有写入成功后才消费重试预算或配额。当前 CaveHouse 的结构预留和
FloodFill 的遍历都缺失，不能把 WorldGen 外层循环理解为原子保证。

```text
Generation config + seed -> UndergroundStructureCandidateQuery
  -> CaveHousePlacementPlanQuery -> Structure reservation
  -> CaveHouseStructureCommitSystem + CaveHouseLootCommand

TileMap snapshot -> FloodFillCandidateQuery + FloodFillNeighborQuery
  -> CavityShapeSnapshot -> CavityWallPlacementCommand
```

最小 focused verifier 应覆盖：所有配置概率归一化、重试预算终止、地牢/海洋/特殊种子候选
拒绝、结构冲突回滚、Sharpener/Extractinator 配额上限；FloodFill 的原点边界、连通形状、
循环去重、动作预算、禁用邻居、少于 50 格不写墙及 Outline 不覆盖保留墙。当前均无运行
verifier 证据，标记为“未验证”。

本轮新增约 **12 个**边界（洞穴房屋配置/配额/候选/重试/计划/提交/战利品 7，FloodFill
预算/候选/邻居/快照/墙写入 5）。累计独立边界约 **604 个**；至少约 **523 个**仍有结构计划、
预留回滚、FloodFill 遍历、光照传播、网络授权或调用消费缺口。静态识别覆盖率保持约
**98%～99%**；权威行为闭合度约 **12%～21%**。本轮只读取 `D:\TRbackup\Version4`，未修改
其源码，并继续排除 `D:\TRbackup\NLTX\src` 中已有组件。

## 86. 第六十二轮：TreeTops 区域变体、世界元数据持久化与加入快照

### 86.1 TreeTopsInfo 是 13 个生态区域的持久化表现元数据，不是每棵树的 Tile 状态

`TreeTopsInfo.AreaId` 固定定义四个森林、腐化、丛林、雪、神圣、猩红、沙漠、海洋、蘑菇和
地狱，共 13 个区域；实例只保存同长度的 `_variations` 数组
（`Terraria.GameContent/TreeTopsInfo.cs:7-40`）。`GetTreeStyle(areaId)` 读取区域变体，而非
某棵树或某块 Tile。因此它是世界会话中的低频、持久化表现元数据，不能塞进 TileMap 或实体
组件。

`Save` 写长度和每一项，`Load` 对低于 211 的世界改走 `CopyExistingWorldInfo`，否则只读取
`min(serializedLength, 13)` 项；`SyncSend` 则压为 13 个 byte（`TreeTopsInfo.cs:42-78`）。
WorldFile 的写入/读取路径调用 Save/Load（`Terraria.IO/WorldFile.cs:1423,2384`），NetMessage
7 号世界信息快照也调用 `SyncSend`（`Terraria/NetMessage.cs:236-276`）。这已经闭合了
“存档与加入快照共享的世界元数据”边界，而非每 tick 复制的模拟状态。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `TreeTopAreaVariationComponent` | 13 个 areaId 到当前树冠样式的世界级数组 | `TreeTopsInfo._variations`；`confirmed` |
| `TreeTopAreaCatalog` | 区域 ID、数量与每区允许的样式域 | `AreaId`、`RandomizeTreeStyle`；`confirmed/partial` |
| `TreeTopWorldPersistenceAdapter` | 存档长度/版本兼容、保存与加载 | `Save/Load`、`WorldFile.cs:1423,2384`；`confirmed/partial` |
| `TreeTopJoinSnapshotProjection` | 7 号世界信息包中的 byte 变体投影 | `SyncSend`、`NetMessage.cs:236-276`；`partial` |
| `TreeTopVariationReadQuery` | 向树冠/背景绘制提供区域样式 | `GetTreeStyle`；`confirmed/partial` |

旧版本迁移的 `CopyExistingWorldInfo` 为空，且加载后没有明确尾随数据策略；因此旧世界迁移和
格式鲁棒性只能保持 `partial/missing`。

### 86.2 新世界和树事件有真实变体写入入口，但需要显式区域校验和节流

新世界初始化调用 `RandomizeTreeStyle`、背景随机化和
`TreeTops.CopyExistingWorldInfoForWorldGeneration`（`Terraria/WorldGen.cs:10376-10385`）。
`RandomizeTreeStyleBasedOnWorldPosition` 用地面 Tile 类型、海洋深度和森林分区把世界位置映射
到区域，再请求随机化（`TreeTopsInfo.cs:86-140`）。`RandomizeTreeStyle` 在各区域样式域中保证
新值不同于旧值，变化后发送 7 号世界信息消息（`:142-198`）；Projectile 的树相关路径也会
按位置随机化（`Terraria/Projectile.cs:49684`）。

但 `RandomizeTreeStyle` 在读 `_variations[areaId]` 前没有通用范围拒绝；位置映射的 `-1` 有
保护，外部直接调用仍需要验证。变更会广播整个世界信息包，但当前没有变更原因、区域差异或
节流记录。建议的附加边界如下：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `TreeTopAreaClassificationQuery` | 从地面 Tile、海洋深度和森林分区计算 areaId | `RandomizeTreeStyleBasedOnWorldPosition`；`confirmed/partial` |
| `TreeTopStyleRollQuery` | 在区域样式域中生成不同于当前值的候选 | `RandomizeTreeStyle`；`confirmed/partial` |
| `TreeTopStyleMutationCommand` | 校验 areaId，提交变体与变化原因/随机源 | 当前直接写数组；`partial` |
| `TreeTopMetadataReplicationAdapter` | 合并/节流变体变化后投影最终快照 | `NetMessage.SendData(7)`；`partial/missing` |
| `TreeTopWorldGenerationInitializationSystem` | 创建世界时初始化样式并处理旧世界迁移 | `WorldGen.cs:10376-10385`；`partial/missing` |

### 86.3 与背景闪光的调用方向

`BackgroundChangeFlashInfo` 复用 13 区计数，却读取 `WorldGen.treeBG*` 等背景字段；它是短期
过渡缓存。`TreeTopsInfo` 则是树冠元数据并参与存档和加入快照。二者都服务表现，但不能合并
为同一可变环境状态，更不能让闪光衰减回写 TreeTop 样式。

```text
World seed / tree event -> TreeTopAreaClassificationQuery -> TreeTopStyleRollQuery
  -> TreeTopStyleMutationCommand -> persistence + join snapshot projection
  -> tree/background read projection
```

最小 focused verifier 应覆盖：13 个区域样式域、随机化不保留旧样式、未知 areaId 拒绝、全部
Tile 映射、旧存档迁移、短/长序列化长度、截断网络包和连发变化合并；同时证明树冠元数据变化
绝不写 Tile、实体、生态扩散或战利品。当前没有这些运行 verifier 证据，均标记为“未验证”。

本轮新增约 **10 个**边界（区域状态/目录/存档/加入/读取 5，分类/滚动/命令/复制/初始化 5）。
累计独立边界约 **614 个**；至少约 **533 个**仍有旧世界迁移、区域校验、变体复制、结构计划、
光照传播、网络授权或调用消费缺口。静态识别覆盖率保持约 **98%～99%**；权威行为闭合度约
**12%～21%**。本轮只读取 `D:\TRbackup\Version4`，未修改其源码，并继续排除
`D:\TRbackup\NLTX\src` 中已有组件。

## 87. 第六十三轮：Shimmer 防卡保护、玩家液体状态与伤害规避缺口

### 87.1 `ShimmerUnstuckHelper` 是玩家级临时保护组件，但启动链没有闭合

`ShimmerUnstuckHelper` 只保存 `TimeLeftUnstuck` 与 `IndefiniteProtectionActive` 两个字段，
`ShouldUnstuck` 根据无限保护或剩余 tick 暴露只读资格（`Terraria.GameContent/ShimmerUnstuckHelper.cs:5-20`）。
`Update(Player)` 在玩家不再 shimmering/shimmerWet 时清除无限保护；若仍有剩余时间且仍在
闪耀液体中则再次调用 `StartUnstuck`，否则递减计时并在到期时请求 ShimmerTownNPC 粒子
（`:22-47`）。`StartUnstuck` 固定写入无限保护和 120 tick，`Clear` 清零两项
（`:49-64`）。

玩家主循环每 tick 调用 `shimmerUnstuckHelper.Update(this)`（`Terraria/Player.cs:15309`），
死亡更新清理它（`:9954`）。但是唯一应当触发保护的入口位于液体检测代码中且整段被注释
（`Player.cs:17138-17146`）；当前源码中没有任何对 `StartUnstuck` 的外部调用。因此组件有
状态变换实现，却没有从“卡在可穿越 Shimmer Tile”到“启动保护”的可达权威链，必须标记为
`partial/missing`，不能声称已提供 Shimmer 脱困玩法。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `ShimmerContactState` | 玩家 shimmering、shimmerWet、液体接触和清理 | `Player.cs:17132-17361`；`confirmed/partial` |
| `ShimmerUnstuckProtectionComponent` | 无限保护标志与 120 tick 临时预算 | `ShimmerUnstuckHelper.cs:5-20,49-64`；`confirmed` |
| `ShimmerUnstuckTickSystem` | 根据接触状态续期/递减、到期投影、死亡清理 | `ShimmerUnstuckHelper.Update`、`Player.UpdateDead`；`confirmed/partial` |
| `ShimmerEscapeQualificationQuery` | 玩家、Tile、深度、shimmerImmune 与保护状态的脱困资格 | `Player.cs:17138-17146`（当前注释）；`missing` |
| `ShimmerProtectionStartCommand` | 在资格通过后原子启动保护并避免重复触发 | 应由被注释入口提交；`missing` |
| `ShimmerDodgeQualificationQuery` | 伤害来源、冷却、可闪避标志与 shimmering 的规避资格 | `Player.cs:22212,22568-22571`；`missing`（方法恒返 false） |
| `ShimmerUnstuckParticleProjection` | 保护到期时的粒子请求 | `ParticleOrchestrator.BroadcastOrRequestParticleSpawn`；`confirmed/partial` |

`Hurt` 虽然在 shimmering 时调用 `AllowShimmerDodge`，但该方法体为 `return new bool ()`，
等价于始终不允许规避；不能把 `shimmering` 标志本身当作伤害免疫。建议将服务器上的
环境接触、脱困保护和伤害规避分成三个可验证阶段，并由客户端粒子作为单向 Projection。

### 87.2 调用方向与验证 seam

```text
Authoritative Tile/liquid snapshot -> ShimmerContactState
  -> ShimmerEscapeQualificationQuery -> ShimmerProtectionStartCommand
  -> ShimmerUnstuckTickSystem -> Player movement/damage policy
  -> particle projection
```

最小 verifier 应覆盖：离开液体立即撤销无限保护、120 tick 到期且只产生一次粒子、重复
接触不无限累加预算、死亡清理、地狱层/固体 Tile/免疫条件、`AllowShimmerDodge` 的来源
和冷却矩阵；并证明客户端 `shimmering` 位不能单独授予伤害免疫。当前启动入口和规避方法
均未闭合，全部相关玩法标记为“未验证”。

## 88. 第六十四轮：玩家宠物抚摸关系、目标生命周期与网络意图

### 88.1 `PlayerPettingInfo` 只保存关系快照，未发现激活命令

`PlayerPettingInfo` 记录 `isPetting`、NPC/Projectile 索引、类型、坐标偏移、宠物大小和
mount 标志；三个构造函数分别从 NPC、Projectile 或 mount 建立目标身份快照
（`Terraria.GameContent/PlayerPettingInfo.cs:5-53`）。`TryGetTarget` 会再次读取全局实体数组，
检查 active 与 type 是否仍匹配；mount 目标返回空实体但直接视为有效（`:55-78`），因此它是
防止索引复用的目标资格 Query，而不是实体组件本身。

玩家更新阶段在 `UpdatePettingAnimal` 中先拒绝无激活标志，再检查目标有效性、NPC 对话对象、
玩家与目标距离；移动输入、跳跃、滑轮、坐骑变化或方向不一致都会调用 `StopPettingAnimal`
（`Terraria/Player.cs:19712-19742`）。停止只把 `petting.isPetting` 置为 false，没有恢复
目标状态或发出独立命令。网络 13 号玩家快照只传输 `isPetting` 与 `isPetSmall`
（`MessageBuffer.cs:720-725`、`NetMessage.cs:466-474`）。

全仓库搜索没有找到 `isPetting = true`、`new PlayerPettingInfo(...)` 或其它激活写入点；因此
当前只能确认“停止/复核/快照”三段，不能确认任何 NPC、投射物或坐骑真正开始抚摸。该链应以
`partial` 记录，不能从构造函数推导完整的宠物互动系统。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `PlayerPettingRelationComponent` | 目标身份、偏移、大小与抚摸标志 | `PlayerPettingInfo.cs:5-53`；`confirmed/partial` |
| `PettingTargetQualificationQuery` | active/type、索引复用、NPC 对话绑定与 mount 特例 | `TryGetTarget`、`Player.UpdatePettingAnimal`；`confirmed/partial` |
| `PettingStartCommand` | 由 SmartInteract/输入选择目标并提交关系快照 | 未发现 `isPetting=true` 或构造函数消费者；`missing` |
| `PettingLifecycleSystem` | 每 tick 距离/输入/方向复核及停止 | `Player.cs:19719-19742`；`confirmed` |
| `PettingStopCommand` | 解除关系并清理坐骑/输入冲突 | `StopPettingAnimal`、`StopVanityActions`；`confirmed/partial` |
| `PettingIntentSnapshotProjection` | 向其它客户端同步抚摸位和大小标志 | 玩家 13 号包；`partial` |

距离阈值、NPC 对话绑定和索引复核属于权威规则；抚摸姿势、声音和粒子若在其它绘制代码中
出现，应作为 Projection，不能反向写入 `isPetting`。建议后续沿 SmartInteract 的动作提交
入口追踪 StartCommand；若仍不存在，应把该功能归入未实现的 vanity interaction，而不是
扩大现有 `PlayerRestSystem` 的职责。

## 89. 第六十五轮：Lucy 斧头情境消息、冷却状态与孤立协议包

### 89.1 消息源目录和冷却器存在，但生产与展示链断裂

`LucyAxeMessage.MessageSource` 定义 Idle、Storage、ThrownAway、PickedUp、ChoppedTree、
ChoppedGemTree、ChoppedCactus 七种情境；静态数组按来源保存 7 个冷却计时器
（`Terraria.GameContent/LucyAxeMessage.cs:10-24`）。`Main.Update` 每帧只调用
`UpdateMessageCooldowns` 递减正值（`Terraria/Main.cs:11351`、`LucyAxeMessage.cs:25-37`）。

网络层为 141 号包预留了来源、字节、速度、两个整数参数；客户端收包后不解释内容，只将
原始字段转发给其它客户端（`Terraria/MessageBuffer.cs:3200-3208`），发送端序列化位于
`NetMessage.cs:1588-1595`。初始化处的 `LucyAxeMessage.Initialize()` 被注释
（`Terraria/Main.cs:3442`），全仓库也没有发现 `SendData(141)`/`TrySendData(141)` 的生产者。
因此这是“协议外壳 + 全局冷却计时器”，不是已闭合的 Lucy 斧头情境玩法。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `LucyAxeMessageSourceCatalog` | 斧头情境枚举与来源到冷却槽映射 | `LucyAxeMessage.cs:12-24`；`confirmed` |
| `LucyAxeMessageCooldownComponent` | 每来源的剩余消息冷却 | `_messageCooldownsByType`、`UpdateMessageCooldowns`；`confirmed/partial` |
| `LucyAxeEventQualificationQuery` | 存储、拾取、丢弃、砍树等事件与玩家/物品资格 | 仅有枚举，未发现事件消费者；`missing` |
| `LucyAxeMessageCommand` | 按来源、冷却和上下文生成一次情境消息 | `Initialize` 与发送生产者均缺失；`missing` |
| `LucyAxeNetworkAdapter` | 141 号包编解码及服务端转发 | `MessageBuffer.cs:3200-3208`、`NetMessage.cs:1588-1595`；`partial` |
| `LucyAxeTextProjection` | 本地化文本、声音/UI 提示或世界聊天 | `LucyAxeMessage.cs` 引用 Localization/UI 但无实现；`missing` |

141 号包的来源、字节和整数必须由服务器根据真实 Item/Tile 事件重建，不能把客户端发送的
速度或整数直接视作授权结果。当前没有任何发送生产者，故不能把协议转发误报为玩法完成。

## 90. 第六十六轮：厕所坐席的生理效果分支与空实现提交点

### 90.1 `ExtraSeatInfo.IsAToilet` 已从 Tile 资格传播到生命回复阶段

`PlayerSittingHelper.GetSittingTargetInfo` 先检查 `TileID.Sets.CanBeSatOnForPlayers` 和
Tile active，再按 Tile 类型、frameX/frameY 计算座位坐标、朝向与偏移；类型 15 的特定 frame
以及类型 497 会把 `extraInfo.IsAToilet` 设为 true（`Terraria.GameContent/PlayerSittingHelper.cs:94-135`）。
`UpdateSitting` 每 tick 重新计算目标，检查输入、坐骑、堆叠座位数量，并把 `details` 更新为
最新标志（`:25-61`）。这说明厕所不是单纯绘制属性，而是玩家-设施关系中的 gameplay tag。

`Player.UpdateLifeRegen` 在坐下且 `details.IsAToilet` 时调用 `TryToPoop()`
（`Terraria/Player.cs:11126-11134`），但 `TryToPoop` 方法体为空（`:10846`）。因此已经
闭合了“Tile → SeatRelation → 每 tick 生理触发”资格链，却没有任何掉落、粒子、音效、计时器、
Buff 或网络提交。不能声称厕所会产生实际物品或状态效果。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `ToiletSeatQualificationQuery` | 可坐 Tile、frame、朝向、偏移与厕所标签 | `PlayerSittingHelper.GetSittingTargetInfo`；`confirmed` |
| `PlayerSeatRelationComponent` | 当前座位、堆叠索引和 `IsAToilet` 关系状态 | `PlayerSittingHelper`、`ExtraSeatInfo`；`confirmed/partial` |
| `ToiletPhysiologyTickSystem` | 坐下期间按生命回复阶段触发生理动作 | `Player.UpdateLifeRegen`；`confirmed/partial` |
| `ToiletOutputQualificationQuery` | 冷却、权限、空间和输出类型判定 | `TryToPoop` 空体；`missing` |
| `ToiletOutputCommitCommand` | 原子生成物品/粒子/Buff/声音并可广播 | `TryToPoop`；`missing` |
| `ToiletEffectProjection` | 把已提交结果投影到粒子、音效、CombatText/UI | 当前无消费者；`missing` |

该分支应作为 `PlayerRestSystem` 的子能力保留，但输出提交必须与生命回复计算解耦：
`LifeRegenCalculation` 只产生“满足厕所触发”的确定性事件，随后由 `ToiletOutputCommand`
一次性消费，避免在每帧生命回复中重复生成结果。`TryToPoop` 为空使整个输出阶段保持
`missing`，需要后续从物品、粒子和网络消费者反向补证。

## 91. 第六十七轮：PlayerEyeHelper 的伤害/环境视觉状态机（表现投影边界）

### 91.1 眼睛状态有完整本地状态转移，但不应进入权威模拟

`PlayerEyeHelper` 保存 `EyeState`、状态计时器和输出帧；`Update` 先按玩家信息选状态，再
按状态与时间计算 EyeOpen/EyeHalfClosed/EyeClosed（`Terraria.GameContent/PlayerEyeHelper.cs:3-79`）。
状态优先级为失明、刚受伤、睡眠、低生命、醉酒、中毒/饥饿、沙尘暴/雪雨和普通眨眼
（`:81-131`）；`BlinkBecausePlayerGotHurt` 强制切换到 JustTookDamage 并重置计时
（`:143-153`）。

该状态机只读取 `blackout/blind/statLife/sleeping/tipsy/poisoned/venom/starving/Zone*` 等
已存在玩家状态，输出 `EyeFrameToShow`，没有网络、存档或世界写入。因此应拆成纯 Projection：

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `EyeStateSelectionQuery` | 按玩家状态和环境选择眼睛状态 | `SetStateByPlayerInfo`；`confirmed` |
| `EyeBlinkStateComponent` | 当前状态与持续 tick | `_state`、`_timeInState`；`confirmed` |
| `EyeFrameProjection` | 将状态映射为三种眼睛帧 | `UpdateEyeFrameToShow`、`EyeFrameToShow`；`confirmed` |
| `EyeDamageTriggerProjection` | 受伤时触发短暂闭眼表现 | `BlinkBecausePlayerGotHurt`；`confirmed` |

该模块不应被计入生命、Buff、黑暗伤害或失明规则的权威实现；反向依赖只能来自已提交的
玩家状态快照。验证重点是状态优先级、睡眠计时覆盖、受伤 20 tick 窗口和低生命阈值，
而不是把眼睛帧同步给服务器。

本轮新增约 **31 个**边界（Shimmer 接触/保护/脱困/伤害规避 7，宠物抚摸关系与网络 6，
Lucy 斧头情境消息 6，厕所生理分支 6，眼睛表现状态机 4，另含跨模块授权与投影 seam）。
累计独立边界约 **645 个**；至少约 **564 个**仍有 Shimmer 启动与规避、宠物激活、Lucy
消息生产者、厕所输出、旧世界迁移、结构计划、FloodFill、光照传播或网络授权缺口。静态
识别覆盖率保持约 **98%～99%**；权威行为闭合度约 **12%～21%**。本轮只读取
`D:\TRbackup\Version4`，未修改其源码，并继续排除 `D:\TRbackup\NLTX\src` 中已有组件。

## 92. 第六十八轮：微型生态生成器、陷阱化宝箱与双地牢几何调度

### 92.1 多个 `MicroBiome.Place` 为空，但 WorldGen 已把它们当作原子结构命令调用

`CampsiteBiome`、`DesertBiome`、`DunesBiome`、`EnchantedSwordBiome`、`HiveBiome`、
`HoneyPatchBiome`、`MahoganyTreeBiome`、`MarbleBiome`、`MiningExplosivesBiome` 和
`ThinIceBiome` 的 `Place` 当前都直接返回默认 `bool`，没有候选扫描、StructureMap 预留、
Tile/Wall/Chest 写入或失败回滚。`GraniteBiome` 还提供了真实的 `CanPlace`：先调用
`WorldGen.BiomeTileCheck`，再检查起点 Tile 是否为空（`Terraria.GameContent.Biomes/GraniteBiome.cs:42-52`），
但其 `Place` 同样为空。`DeadMansChestBiome` 的 `GetPossibleChestsToTrapify` 则会遍历最多
8000 个 Chest，按 `IsAGoodSpot`、陷阱缓存和 `StructureMap.CanPlace` 过滤候选
（`DeadMansChestBiome.cs:74-124`），但 `IsAGoodSpot`、三类陷阱位置查找、数量检查和最终
`Place` 都是空体（`:25-72,126-128`）。

这些不是“未被引用”的死代码：`WorldGen` 明确在生成 pass 中反复调用它们，并依据返回值
递减配额或重试。例如 Desert 循环会在 `!desertBiome.Place(...)` 时重新抽样
（`Terraria/WorldGen.cs:11573-11580`）；Granite 成功后递增已放置数量
（`:11997-12001`）；Hive 成功后再围绕原点布置多个 HoneyPatch
（`:15055-15074`）；地下房屋、Dead Man's Chest、Thin Ice、Enchanted Sword、Campsite、
Mining Explosives、Mahogany Tree 均依赖各自 `Place` 的成功布尔值
（`:16207-16232,20827-21020`）。因此外层的重试/进度并不等于结构已经生成。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `MicroBiomeCandidateQuery` | 原点深度、生态、地牢/海洋/结构冲突与 Tile 资格 | 各 Biome `Place`、`GraniteBiome.CanPlace`；`partial` |
| `MicroBiomePlacementPlan` | 计算结构尺寸、材料、家具、Chest/Item 与配额 | `Place` 空体；`missing` |
| `MicroBiomeStructureReservation` | 在写入前以 StructureMap 原子预留区域 | `StructureMap` 参数、Dead Man's `CanPlace`；`partial/missing` |
| `MicroBiomeTileCommitSystem` | Tile/Wall/液体/家具/宝箱一次性提交 | 各 `Place`；`missing` |
| `MicroBiomeRetryQuotaSystem` | 失败重抽、配额递减、进度与终止 | `WorldGen` 外层循环；`confirmed/partial` |
| `GraniteMagmaFieldComponent` | 200×200 源/目标岩浆压力与阻力缓存 | `GraniteBiome._sourceMagmaMap/_targetMagmaMap`；`partial` |
| `DeadMansTrapCandidateQuery` | 现有 Chest 的安全位置、飞镖/巨石/爆炸陷阱可行性 | `GetPossibleChestsToTrapify`；`partial` |
| `DeadMansTrapCommitCommand` | 接线、陷阱 Tile、爆炸物和 Chest 变换 | `Place`、`Find*TrapSpots` 空体；`missing` |

### 92.2 DitherSnake 与 DungeonControlLine 已有几何 Query，但双地牢生成 pass 断开

`DungeonControlLine` 保存线段起止点、切线/法线、半径、ProgressionStage 和样式；
`CanPaint` 能按点积计算线段内外、距离及归一化进度（`Terraria.GameContent.Biomes/DungeonControlLine.cs:7-104`）。
`DitherSnake` 能找最近控制线、递归寻找包含点的线段，并把位置映射到 snake 进度
（`DitherSnake.cs:8-68`）。这些属于只读几何 Query，可为双地牢的墙体样式渐变提供稳定输入。

然而 `DitherSnakePass.ApplyPass` 是空体（`DitherSnakePass.cs:13-25`），而 `WorldGen` 仅在
双地牢开关开启时把该 pass 注册到生成序列（`WorldGen.cs:13840-13843`）。当前没有看到
控制线构建、Prev/Next 链接、样式写入、StructureMap 交接或失败回滚，因此不能由几何
辅助类推断双地牢已经生成。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `DungeonControlLineComponent` | 控制线拓扑、半径、样式与 progression stage | `DungeonControlLine` 字段；`confirmed/partial` |
| `DitherSnakeGeometryQuery` | 最近线、包含线、距离和归一化进度 | `DitherSnake`、`CanPaint`；`confirmed` |
| `DualDungeonControlLineBuildSystem` | 根据地牢布局生成并链接控制线 | `DitherSnakePass.ApplyPass`；`missing` |
| `DualDungeonDitherPaintCommand` | 将样式/墙体渐变提交到 TileMap | `DitherSnakePass` 空体；`missing` |
| `DualDungeonGenerationGate` | Secret Seed 开关、pass 顺序和进度 | `WorldGen.cs:13840-13843`；`confirmed/partial` |

### 92.3 Terrain/Jungle pass 只有注册元数据，没有地形提交证据

`TerrainPass` 只实现 `SurfaceHistory` 环形高度缓存和 pass 名称/权重，`ApplyPass` 为空；
`JunglePass` 同样只声明 pass 名称/权重，`ApplyPass` 为空。两者都由 `WorldGen.AddPasses`
注册（`TerrainPass.cs:8-40`、`JunglePass.cs:10-18`、`WorldGen.cs:10555-10563,11525-11528`）。
这意味着当前版本能证明“生成管线有这些阶段”，但不能证明地表高度、丛林泥土/植被、
液体和结构已经由这些 pass 写入。建议将生成阶段拆成高度场 Query、生态材料规则、结构
提交和进度适配器；不要把 pass 权重误当作完成度。

```text
World seed + generation config
  -> MicroBiomeCandidateQuery / DungeonControlLine geometry
  -> PlacementPlan + StructureMap reservation
  -> Tile/Wall/Chest/Trap commit
  -> retry quota + GenerationProgress

Terrain/Jungle/DualDungeon pass registration
  -> (currently missing ApplyPass implementation)
```

最小 focused verifier 应覆盖：每类 Biome 的原点边界、生态与特殊种子过滤、StructureMap
冲突回滚、失败重试配额、Granite 岩浆缓存交换、Dead Man's 陷阱数量与接线、双地牢控制线
拓扑和样式连续性，以及空 `ApplyPass` 不得报告生成成功。当前上述 Place/ApplyPass 均无
运行 verifier 证据，标记为“未验证”。

本轮新增约 **19 个**边界（微型生态候选/计划/预留/提交/重试 5，Granite 岩浆 1，Dead
Man's 陷阱 2，DitherSnake/双地牢 5，Terrain/Jungle pass 3，另含跨 pass 进度与回滚 seam）。
累计独立边界约 **664 个**；至少约 **583 个**仍有微型 Biome Place、陷阱提交、双地牢
ApplyPass、Terrain/Jungle 生成、Shimmer 启动与规避、宠物激活、厕所输出、结构计划、
FloodFill、光照传播或网络授权缺口。静态识别覆盖率保持约 **98%～99%**；权威行为闭合度约
**12%～21%**。本轮只读取 `D:\TRbackup\Version4`，未修改其源码，并继续排除
`D:\TRbackup\NLTX\src` 中已有组件。

## 102. 第七十八轮：CLI 世界删除与世界会话清理

详见本报告附录 A 摘要及其调用证据：`Main` 的 CLI 删除确认最终到达空体 `EraseWorld`，
本地/云删除、活动世界保护、备份清理和列表刷新均为 `missing/partial`。

## 103. 第七十九轮：暂停更新门控与退出生命周期

详见附录 B：`CanPauseGame` 恒 false，`DoUpdate_WhilePaused`、`QuitGame`、`Main_Exiting`
为空；暂停更新、退出保存/断线/资源释放应拆为 Query、System、Command 和 Adapter，当前未闭合。

## 104. 第八十轮：世界文件元数据、种子选项与持久化迁移

详见附录 C：世界文件版本化保存/加载主链可达，但三个种子解析函数及云/本地迁移方法为空，
元数据异常被压成 null；应拆为持久化 Component、种子 Query、Save/Load Adapter、迁移 Command
和失败策略，状态为 `confirmed/partial/missing`。

本轮新增约 **17 个**边界。累计独立边界约 **781 个**；仍有 `missing/partial` 的主要集中在
暂停/退出、世界删除、种子解析、云本地迁移、快照保存恢复、世界哈希、生成执行控制、地牢/轨道/生态
生成、事件提交、实体网络授权和光照传播。静态识别覆盖率约 **98%～99%**，权威行为闭合度约
**12%～21%**。本轮未重新展开用户已要求停止的特殊刷怪分支。

## 93. 第六十九轮：地牢样式能力矩阵与 GlobalDungeonFeature 提交断链

### 93.1 样式数据是内容目录，不等于家具、门和陷阱已经写入世界

`DungeonGenerationStyleData` 为每种地牢样式保存砖块/裂砖/墙、窗、液体、锁定生物群系
宝箱、普通宝箱、门、平台、灯具、家具、书架、床、旗帜、边缘抖动和房间类型等完整内容
映射，并允许 `SubStyles` 嵌套样式（`Terraria.GameContent.Generation.Dungeon/DungeonGenerationStyleData.cs:8-80`）。
`DungeonGenerationStyles` 注册 Cavern、Snow、Desert、Corruption、Crimson、Hallow、
GlowingMushroom、Beehive、Shimmer、LivingWood 等样式实例；部分样式覆写
`CanGenerateFeatureAt` 或书架尺寸，但这些覆写在当前版本返回默认值。该目录应建模为
`DungeonStyleCatalog`，不能直接视作 Tile/Wall/家具组件。

### 93.2 14 个全局特征按顺序被调度，但各 `GenerateFeature` 仍为空

`DungeonCrawler.MakeDungeon` 在进度 0.70～0.99 依次创建并调用早期双地牢特征、尖刺、门、
墙变体、平台、生物群系箱、书架、普通箱、灯光、陷阱、地面家具、画作、旗帜和晚期双地牢
特征（`Terraria.GameContent.Generation.Dungeon/DungeonCrawler.cs:469-498`）。这些对象的
构造函数会把自己注册到 `DungeonCrawler.CurrentDungeonData.dungeonFeatures`，但基类
`GlobalDungeonFeature` 只保存 `settings/generated`，具体子类 `GenerateFeature` 多数直接
返回默认 `bool`；对 15 个 `DungeonGlobal*.cs` 文件做同模式检索均命中空体。故当前可确认
“调度顺序和进度标签”，不能确认任何特征产生 Tile、墙、Chest、液体、灯或家具。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `DungeonStyleCatalog` | 样式到 Tile/Wall/Item/房间类型的内容映射 | `DungeonGenerationStyleData`、`DungeonGenerationStyles`；`confirmed/partial` |
| `DungeonFeatureCapabilityQuery` | 样式是否允许某特征及书架尺寸资格 | `CanGenerateFeatureAt`、`GetBookshelfMinMaxSizes`；`missing`（默认值） |
| `DungeonFeatureScheduleComponent` | 特征顺序、进度权重和当前生成上下文 | `DungeonCrawler.MakeDungeon`；`confirmed` |
| `DungeonFeatureRegistrationSystem` | 构造特征并登记到当前 DungeonData | `GlobalDungeonFeature` 构造函数；`confirmed/partial` |
| `DungeonFeaturePlacementCommand` | 门/墙/平台/箱/家具/灯/陷阱等各类 Tile 提交 | 各 `DungeonGlobal*.GenerateFeature`；`missing` |
| `DungeonFeatureAtomicCommitSystem` | 预检 StructureMap、批量写入、失败回滚和 `generated` 状态 | `GlobalDungeonFeature.generated` 无提交消费者；`missing` |
| `DungeonFeatureProgressProjection` | 把阶段进度映射为生成 UI/日志 | `DungeonUtils.UpdateDungeonProgress` 调用；`partial` |

`DungeonDoorData` 虽定义门位置、砖/墙覆盖、方向、走廊与空间检查开关
（`DungeonDoorData.cs:3-27`），但没有证据表明 `DungeonGlobalDoors` 消费这些字段；同理，
平台、颜色、液体和家具类型数组也只是配置输入。实现时必须让每个特征先通过样式能力与
房间资格 Query，再产生单一 PlacementCommand，最后统一提交并更新 `generated`；不能由
进度达到 1.0 推断地牢已可玩。

```text
DungeonStyleCatalog + DungeonData
  -> DungeonFeatureCapabilityQuery
  -> ordered DungeonFeatureSchedule
  -> per-feature PlacementCommand
  -> StructureMap/Tile/Chest atomic commit
  -> generated flag + progress projection
```

最小 focused verifier 应覆盖：样式数组长度/空数组处理、SubStyles 继承、非法 Item/Tile
类型拒绝、14 个阶段严格顺序、特征失败时不污染前一阶段、门的空间/方向检查、箱子与战利品
唯一性、陷阱接线、灯光与墙体提交以及 `generated` 只在成功后置位。当前所有具体
`GenerateFeature` 仍无运行 verifier，均标记为“未验证”。

本轮新增约 **12 个**边界（样式目录/能力 2，调度/注册/进度 3，门/墙/平台/箱/家具/灯/陷阱
提交与原子性 7）。累计独立边界约 **676 个**；至少约 **595 个**仍有地牢特征提交、
微型 Biome Place、双地牢 ApplyPass、Terrain/Jungle 生成、Shimmer 启动与规避、宠物激活、
厕所输出、结构计划、FloodFill、光照传播或网络授权缺口。静态识别覆盖率保持约 **98%～99%**；
权威行为闭合度约 **12%～21%**。本轮只读取 `D:\TRbackup\Version4`，未修改其源码，并继续
排除 `D:\TRbackup\NLTX\src` 中已有组件。

## 94. 第七十轮：地牢房间生命周期、入口类型与房间内资格 Query

### 94.1 房间抽象同时承担几何计算、特征资格和结构提交边界

`DungeonRoom` 为每个房间保存 `DungeonRoomSettings`、`calculated/generated` 状态、
`InnerBounds/OuterBounds`，并以 `Processed` 区分“未计算但已生成”和“已计算”状态
（`Terraria.GameContent.Generation.Dungeon.Rooms/DungeonRoom.cs:8-31`）。基类公开
`CalculateRoom`、`GenerateRoom`、大厅连接点、房间中心、房间内点判断、保护类型、淹没 Tile
计数、FloodRoom、家具计数和两类 Chest 生成入口（`:32-80`）。这些方法并非只读几何：
房间生成会决定后续门/平台/箱/家具能否放置，`generated` 也应只有提交成功后才置位。

当前 `RegularDungeonRoom`、`BiomeSquareDungeonRoom`、`BiomeStructuredDungeonRoom`、
`BiomeRuggedDungeonRoom`、`GenShapeDungeonRoom`、`LegacyDungeonRoom`、
`LivingTreeDungeonRoom` 和 `WormlikeDungeonRoom` 的 `CalculateRoom`/`GenerateRoom` 或
`IsInsideRoom` 等关键实现均为空或返回默认值；`DungeonRoom` 基类的 Chest、保护类型和
洪水统计默认实现也未闭合。`DungeonCrawler` 在房间循环中确实调用生成并随后生成入口大厅
（`DungeonCrawler.cs:430-446`），因此这里是可达的生成链，不应归类为纯模板数据。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `DungeonRoomLifecycleComponent` | settings、Inner/Outer bounds、calculated/generated/Processed | `DungeonRoom.cs:8-31`；`confirmed/partial` |
| `DungeonRoomGeometryQuery` | 房间中心、边界内点、走廊连接点和保护类型 | 基类与各 Room override；`partial/missing` |
| `DungeonRoomCalculationSystem` | 按样式/房间类型计算形状、尺寸、预留空间 | `CalculateRoom`；各实现空体；`missing` |
| `DungeonRoomStructureCommitCommand` | 写入房间砖墙、液体、平台、家具并置位 generated | `GenerateRoom`；`missing` |
| `DungeonRoomChestCommand` | 普通/生物群系箱资格、唯一性和战利品提交 | `TryGenerateChestInRoom`、`DualDungeons_*`；`missing` |
| `DungeonRoomFloodSystem` | 统计可淹没 Tile、提交液体并维护房间状态 | `GetFloodedRoomTileCount`、`FloodRoom`；`partial/missing` |
| `DungeonRoomFeatureQualificationQuery` | 特征在房间内某坐标的可放置资格 | `CanGenerateFeatureAt`；`missing` |

### 94.2 入口类型有独立生命周期，但 Calculate/Generate/Feature 检查同样断裂

`DungeonEntrance` 保存入口 settings、`calculated/generated`、Bounds 和 Old Man 出生点，
定义 `CalculateEntrance`、`GenerateEntrance` 以及入口范围内的特征资格检查
（`Terraria.GameContent.Generation.Dungeon.Entrances/DungeonEntrance.cs:5-29`）。
`LegacyDungeonEntrance`、`DomeDungeonEntrance` 和 `TowerDungeonEntrance` 的计算、生成或
特征资格方法当前为空/默认返回（对应文件 `:16-22`）。`DungeonCrawler` 根据预生成设置
选择入口类型并调用 `GenerateEntrance`（`DungeonCrawler.cs:446`），所以入口失败会直接影响
地牢边界、老人 NPC 出生点、入口大厅和后续特征坐标。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `DungeonEntranceLifecycleComponent` | 入口 settings、Bounds、OldManSpawn 和生成状态 | `DungeonEntrance.cs:8-20`；`confirmed/partial` |
| `DungeonEntranceTypeSelectionQuery` | Legacy/Dome/Tower/预生成入口类型选择 | `DungeonCrawler.MakeDungeon_GetEntrance*`、入口 settings；`partial` |
| `DungeonEntranceGeometryQuery` | 入口尺寸、边界、地面/大厅衔接 | `CalculateEntrance`；实现空体；`missing` |
| `DungeonEntrancePlacementCommand` | 入口 Tile/Wall、出生点和大厅结构提交 | `GenerateEntrance`；`missing` |
| `DungeonEntranceFeatureQualificationQuery` | 入口区域内门、墙、家具、陷阱等特征资格 | `CanGenerateFeatureAt`；`missing` |

入口与普通房间必须共享 StructureMap 预留协议，但不能共享同一个几何组件：入口还拥有
Old Man 出生点和类型特有的地面/大厅连接规则。`generated=true` 只能在 Bounds、Tile、墙体、
出生点和必要的 StructureMap 记录全部提交后设置；当前默认返回值不构成任何成功证据。

```text
DungeonData + room/entrance settings
  -> Room/Entrance geometry calculation
  -> feature qualification queries
  -> StructureMap reservation
  -> room/entrance placement command
  -> generated state -> global feature pipeline
```

最小 focused verifier 应覆盖：每类房间的 bounds 与中心、走廊连接点连续性、房间内外点
分类、保护类型、淹没统计和液体提交、普通/生物群系箱唯一性、入口类型选择、Old Man
出生点、Dome/Tower/Legacy 边界、StructureMap 冲突回滚以及 `generated` 置位时机。当前
上述 Room/Entrance 实现没有运行 verifier，均标记为“未验证”。

本轮新增约 **14 个**边界（房间生命周期/几何/计算/提交/箱/淹没/特征资格 7，入口状态、
类型、几何、提交、特征资格 5，另含统一预留与生成状态 seam 2）。累计独立边界约 **690 个**；
至少约 **609 个**仍有房间/入口生成、地牢特征提交、微型 Biome Place、双地牢 ApplyPass、
Terrain/Jungle 生成、Shimmer 启动与规避、宠物激活、厕所输出、FloodFill、光照传播或网络
授权缺口。静态识别覆盖率保持约 **98%～99%**；权威行为闭合度约 **12%～21%**。本轮只读取
`D:\TRbackup\Version4`，未修改其源码，并继续排除 `D:\TRbackup\NLTX\src` 中已有组件。

## 95. 第七十一轮：地牢布局提供器、房间排序与走廊连接规划

### 95.1 Legacy/Dual LayoutProvider 是生成主干上的空实现

`DungeonLayoutProvider` 只保存 `DungeonLayoutProviderSettings.StyleData`，并要求实现
`ProvideLayout(DungeonData, GenerationProgress, UnifiedRandom, ref roomDelay)`
（`Terraria.GameContent.Generation.Dungeon/DungeonLayoutProvider.cs:6-17`）。
`LegacyDungeonLayoutProvider.ProvideLayout` 和 `DualDungeonLayoutProvider.ProvideLayout` 当前
均为空（对应文件 `:10-18`、`:68-79`）。`DungeonCrawler` 会根据 `DungeonType` 选择二者，
设置样式、步数/最大步数或双地牢配置后调用 `ProvideLayout`，紧接着读取
`currentDungeonData.dungeonRooms[0].InnerBounds`（`DungeonCrawler.cs:354-371`）。因此布局提供器
并非可选优化：当前空实现无法证明房间列表、Bounds、入口顺序或 `roomDelay` 已经产生。

### 95.2 DualDungeon 的 HallwayCalculator 只完成排序输入，未完成连线提交

`DualDungeonLayoutProvider.HallwayCalculator` 将房间包装为 `RoomEntry`，按
`dungeonDitherSnake.GetPositionAlongSnake(room.Center)` 排序，并计算平均控制线长度与允许的
最大进度差（`DualDungeonLayoutProvider.cs:13-65`）。字段还预留前后链接、`DungeonHall` 列表
和 stairwell 列表，但当前没有生成 Hall、连接点、楼梯或写入 `DungeonData` 的后续方法。它
应被视为“排序/几何规划组件”，不能误报为走廊已经连接。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `DungeonLayoutProviderSettingsComponent` | 样式、Legacy 步数/上限及布局参数 | `DungeonLayoutProviderSettings`、`LegacyDungeonLayoutProviderSettings`；`confirmed/partial` |
| `DungeonLayoutSelectionQuery` | 按 DungeonType/Secret Seed 选择 Legacy 或 Dual provider | `DungeonCrawler.cs:354-369`；`confirmed` |
| `LegacyDungeonLayoutSystem` | 随机房间、房间 bounds、步进和 roomDelay 生成 | `LegacyDungeonLayoutProvider.ProvideLayout`；`missing` |
| `DualDungeonRoomLayoutSystem` | 双地牢房间列表、样式子区和蛇形进度布局 | `DualDungeonLayoutProvider.ProvideLayout`；`missing` |
| `DungeonRoomOrderingQuery` | 将房间映射到 DitherSnake 进度并排序 | `HallwayCalculator`；`confirmed/partial` |
| `DungeonHallwayPlanningSystem` | 前后链接、Hall、stairwell 与连接点规划 | `HallwayCalculator` 预留字段但无提交方法；`missing` |
| `DungeonLayoutCommitCommand` | 将房间/大厅/连接关系原子写入 DungeonData 与 StructureMap | `ProvideLayout` 调用后直接读取 `dungeonRooms[0]`；`missing` |
| `DungeonLayoutDelayPolicy` | 生成间隔、失败重试与进度反馈 | `ref int roomDelay`；`partial/missing` |

布局提供器必须先产出可验证的 Room/Hall 计划，再由独立 Commit 写入 `DungeonData`；不能让
`DungeonCrawler` 在 provider 返回后直接假定 `dungeonRooms[0]` 存在。失败时应回滚新增
房间、连接和 StructureMap 预留，并保留可复现随机种子，避免 Legacy 与 Dual 两条路径
产生不同的隐式副作用。

```text
DungeonType + style/settings
  -> DungeonLayoutSelectionQuery
  -> Legacy/Dual room planning
  -> RoomOrdering + HallwayPlanning
  -> LayoutCommit (DungeonData + StructureMap)
  -> entrance/room generation -> global feature pipeline
```

最小 focused verifier 应覆盖：两种 provider 的空世界、单房间和多房间布局、步数上限、
roomDelay 单调性、双地牢蛇形排序、前后链接无环、Hall/stairwell 连接点落在房间边界、
入口始终连接到首房间、随机种子复现、StructureMap 冲突回滚以及 `dungeonRooms[0]` 的
前置存在性。当前两种 `ProvideLayout` 均无运行 verifier，相关布局和连接能力标记为
“未验证”。

本轮新增约 **10 个**边界（provider 配置/选择 2，Legacy/Dual 布局 2，房间排序 1，走廊
规划 1，布局提交 1，延迟/重试 1，另含入口与全局特征的交接 seam 2）。累计独立边界约
**700 个**；至少约 **619 个**仍有布局提供器、房间/入口生成、地牢特征提交、微型 Biome
Place、双地牢 ApplyPass、Terrain/Jungle 生成、Shimmer 启动与规避、宠物激活、厕所输出、
FloodFill、光照传播或网络授权缺口。静态识别覆盖率保持约 **98%～99%**；权威行为闭合度约
**12%～21%**。本轮只读取 `D:\TRbackup\Version4`，未修改其源码，并继续排除
`D:\TRbackup\NLTX\src` 中已有组件。

## 96. 第七十二轮：地下矿车轨道生成、路径回写与轨道 Tile 提交

### 96.1 `TrackGenerator` 有完整的阶段外壳，但路径与写入步骤为空

`TrackGenerator` 保存 4096 项主历史和 25 项重写历史；每个 `TrackHistory` 记录坐标、上/直/
下坡状态与 Normal/Tunnel 模式（`Terraria.GameContent.Generation/TrackGenerator.cs:10-48`）。
`Place(origin, minLength, maxLength)` 依次调用 `FindSuitableOrigin`、`CreateTrackStart`、
`FindPath` 和 `PlacePath`，只有四步都成功才返回 true（`:50-67`）。但 `FindSuitableOrigin`
恒返默认 false，`FindPath` 恒返默认 false，`CreateTrackStart` 与 `PlacePath` 为空
（`:69-83`），所以轨道生成在当前版本无法达到写 Tile 阶段。

这条链被 WorldGen 当作真实配额生成器使用：轨道 pass 读取 `LongTrackCount`、
`LongTrackLength` 和世界尺寸，反复抽样原点并依据 `trackGenerator.Place` 成功与否递增数量、
清零失败计数（`Terraria/WorldGen.cs:21028-21055,21088-21094`）。轨道 Tile 的其它生命周期
（`Minecart.PlaceTrack`、`FrameTrack`、拆除掉落）存在于 WorldGen 的 Tile 操作中，但不能替代
生成器缺失的路径碰撞、坡度、隧道和回写语义。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `TrackGenerationBudgetComponent` | 长轨数量、长度范围、失败重试与历史容量 | `WorldGen.cs:21028-21055`、`TrackGenerator` 数组；`confirmed/partial` |
| `TrackOriginQualificationQuery` | 起点 Tile、边界、地下深度和既有轨道冲突 | `FindSuitableOrigin`；`missing` |
| `TrackPathSearchSystem` | 坡度/直线/隧道模式、邻接可行性和长度范围搜索 | `FindPath`、`TrackHistory`；`missing` |
| `TrackRewriteBacktrackSystem` | 25 步重写历史、死路回退和路径修正 | `_rewriteHistory`；`missing` |
| `TrackPlacementCommand` | 将路径转换为轨道 Tile、frame 和坡度状态 | `PlacePath`；`missing` |
| `TrackTileFramingSystem` | 轨道放置/拆除后的 Tile frame 与绳端联动 | `WorldGen.cs:51862,71997-71999`；`partial` |
| `TrackGenerationRetryProjection` | 将成功数量和进度写入世界生成进度 | `WorldGen` 外层循环；`confirmed/partial` |

轨道生成必须先完成只读路径搜索，再一次性写入轨道 Tile；失败时不得留下半条轨道或已
消耗的数量配额。`TrackHistory` 的数组大小也不能被视为最大世界轨道长度，必须由
`minLength/maxLength` 与边界 Query 共同约束。矿车运行时的碰撞和加速属于运行时 Physics，
不应与生成阶段的 `TrackPlacementCommand` 合并。

```text
WorldGen track config + seed
  -> TrackOriginQualificationQuery
  -> TrackPathSearch + backtrack/rewrite
  -> TrackPlacementCommand
  -> Tile framing / rope-end update
  -> generation quota + progress
```

最小 focused verifier 应覆盖：起点边界、长度上下限、坡度转换、隧道模式、死路回退、历史
溢出、既有轨道冲突、路径原子写入、失败回滚、轨道 frame 与绳端更新，以及 WorldGen 失败
重试不会错误递增成功配额。当前 `FindSuitableOrigin`、`FindPath` 和 `PlacePath` 均无运行
verifier，相关轨道生成能力标记为“未验证”。

本轮新增约 **9 个**边界（轨道预算/起点/路径/回退/提交/Framing/进度 7，另含生成与运行时
Physics 的隔离 seam 2）。累计独立边界约 **709 个**；至少约 **628 个**仍有轨道生成、
布局提供器、房间/入口生成、地牢特征提交、微型 Biome Place、双地牢 ApplyPass、Terrain/
Jungle 生成、Shimmer 启动与规避、宠物激活、厕所输出、FloodFill、光照传播或网络授权
缺口。静态识别覆盖率保持约 **98%～99%**；权威行为闭合度约 **12%～21%**。本轮只读取
`D:\TRbackup\Version4`，未修改其源码，并继续排除 `D:\TRbackup\NLTX\src` 中已有组件。

## 97. 第七十三轮：地牢大厅几何、楼梯连接与门平台资格

### 97.1 `DungeonHall` 的生命周期独立于房间和全局特征

`DungeonHall` 保存 `settings`、`calculated/generated`、Bounds、起止点、起止方向和
`CrackedBrick`，并通过 `Processed` 暴露计算/生成状态（`Terraria.GameContent.Generation.Dungeon.Halls/DungeonHall.cs:7-46`）。
基类要求每种大厅实现 `CalculateHall`、`CalculatePlatformsAndDoors` 和 `GenerateHall`，并
提供家具数量、可放置砖块和可移除裂砖判断（`:48-64`）。这些状态与房间连接直接相关：
大厅先计算边界和方向，随后才能决定平台/门，再提交墙体、砖块和液体。

`LegacyDungeonHall`、`LegacyEntranceDungeonHall`、`RegularDungeonHall`、`SineDungeonHall`
和 `StairwellDungeonHall` 的关键 Calculate/Generate 方法当前均为空；基类
`CanPlaceTileAt`、`CanRemoveTileAt` 和家具数量也为空/默认值。`DungeonCrawler` 在入口大厅、
预计算入口走廊以及普通地牢循环中确实调用这些方法（`DungeonCrawler.cs:427-427,457-465,512-534`），
并按 HallType 构造 Legacy/Regular/Stairwell/Sine 四类实现（`:552-572,713-720`）。

### 97.2 资格检查不能与大厅 Tile 写入混合

门/平台的资格需要读取 DungeonData、房间边界、裂砖类型和已有 Tile；写入则会改变后续
连接、碰撞和全局特征可放置面。因此应将 `CanPlaceTileAt`/`CanRemoveTileAt` 建模为纯
Query，将 `GenerateHall` 建模为结构 Commit。`StairwellDungeonHall` 的砖块资格方法也
为空，不能根据类名推断楼梯会自动避开关键墙体或门口。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `DungeonHallLifecycleComponent` | 大厅 settings、Bounds、起止点/方向、裂砖与 Processed | `DungeonHall.cs:7-46`；`confirmed/partial` |
| `DungeonHallGeometryQuery` | Regular/Sine/Stairwell/Legacy 的中心线、边界和方向 | 各 `CalculateHall`；`missing` |
| `DungeonHallConnectionPlanningSystem` | 房间连接、入口走廊、楼梯和蜿蜒/正弦路径 | `DungeonCrawler.cs:512-534`、Hall 类型；`partial/missing` |
| `DungeonHallPlatformDoorQuery` | 平台、门、空间、方向和裂砖资格 | `CalculatePlatformsAndDoors`、`CanPlaceTileAt`；`missing` |
| `DungeonHallStructureCommitCommand` | 墙/砖/液体/平台/门的批量写入 | `GenerateHall`；`missing` |
| `DungeonHallTileMutationGuard` | 防止移除受保护砖、覆盖房间边界或污染已有结构 | `CanRemoveTileAt`；`missing` |
| `DungeonHallFurnitureCountQuery` | 按大厅类型和样式计算家具预算 | `GetFurnitureCount`；`missing` |
| `DungeonHallGenerationStateSystem` | 仅在计算与提交成功后置位 calculated/generated | `DungeonHall.Processed`；`partial` |

大厅、房间和全局特征必须遵守同一条交接顺序：Hall 几何计划完成后才可计算门/平台，
StructureMap 预留成功后才写 Tile，写入成功后才让房间和全局特征看到大厅。当前空实现
使得 `Processed` 可能与实际世界状态脱节，不能把它当作生成成功证明。

```text
Room/Entrance connection points
  -> Hall geometry calculation
  -> platform/door + tile qualification queries
  -> StructureMap reservation
  -> hall tile/wall/liquid commit
  -> calculated/generated state
  -> room/global feature stages
```

最小 focused verifier 应覆盖：四种 HallType 的起止点和边界、Sine/Step/Stairwell 参数、
入口大厅连续性、门平台空间检查、裂砖替换/保护、液体和墙体写入、房间边界不越界、
StructureMap 冲突回滚、家具预算以及 `Processed` 状态时序。当前各 Hall 的关键方法均无
运行 verifier，相关连接能力标记为“未验证”。

本轮新增约 **10 个**边界（大厅生命周期/几何/连接/门平台 Query/结构提交/保护/家具/状态
8，另含房间与全局特征交接 seam 2）。累计独立边界约 **719 个**；至少约 **638 个**仍有
大厅连接、轨道生成、布局提供器、房间/入口生成、地牢特征提交、微型 Biome Place、双地牢
ApplyPass、Terrain/Jungle 生成、Shimmer 启动与规避、宠物激活、厕所输出、FloodFill、
光照传播或网络授权缺口。静态识别覆盖率保持约 **98%～99%**；权威行为闭合度约 **12%～21%**。
本轮只读取 `D:\TRbackup\Version4`，未修改其源码，并继续排除 `D:\TRbackup\NLTX\src` 中已有组件。

## 98. 第七十四轮：WorldGen 快照、Tile 压缩与生成回滚一致性

### 98.1 `WorldGenSnapshot` 不是普通日志，而是生成 pass 的可恢复状态

`WorldGenSnapshot.Create` 克隆 `WorldGen.Manifest`，序列化静态 `GenVars`，记录与当前结果
匹配的生成 pass，并调用 `TileSnapshot.Create` 后将 Manifest、GenVars JSON 和 Tile 快照写入
世界专属的 `_gensnapshots/*.gensnapshot` 文件（`Terraria.WorldBuilding/WorldGenSnapshot.cs:110-133`）。
`Load` 读取数据偏移后的 Tile 快照；`Restore` 还会恢复临时 WorldGen 状态、Manifest、GenVars、
Tile/TileEntity/Chest，并把全部 NPC 标记为 inactive（`:149-178`）。`WorldGenerator.Controller`
实际维护每个 `GenPass` 的快照字典、删除旧快照，并在 pass 完成且世界哈希匹配时创建快照
（`Terraria.WorldBuilding/WorldGenerator.cs:28-29,108-125,159-164`）；`TryResetToSnapshot`
会拒绝不存在或 `Outdated` 的快照（`:194-202`）。因此这是一条影响生成可恢复性和确定性
验证的 Adapter/System 边界，不应归入普通存档。

### 98.2 TileSnapshot 的序列化路径存在明显空实现缺口

`TileSnapshot.TileStruct.From(Tile)` 会按 `Main.tileFrameImportant`、保存斜坡集合、墙体和液体
标志压缩 Tile 字段，`Write/Read` 采用固定 12 字节结构；`Save` 还序列化 TileEntity 与
Chest 内容（`Terraria.Utilities/TileSnapshot.cs:17-50,51-132,183-219`）。然而 `SaveTiles`、
`SaveTileEntities`、`SaveChests`、`RestoreTiles`、`RestoreTileEntities`、`RestoreChests` 都
为空（`:153-182`），这意味着 Create/Save 虽能写头部和列表格式，但当前无法证明快照实际
包含或恢复世界数组。`TileStruct.Equals`、`GetHashCode`、`ToString` 也返回默认值
（`:104-114`）；而 `WorldGenerator.HashWorld` 会对每个 TileStruct 调用 `GetHashCode`
并滚动合成行哈希（`WorldGenerator.cs:355-362`），因此确定性哈希在当前实现下不可信。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `WorldGenSnapshotManifestComponent` | Git SHA、版本、pass 跳过状态和快照路径 | `WorldGenSnapshot.Manifest/Path`、`WorldGenerator.Controller`；`confirmed/partial` |
| `GenVarsSnapshotAdapter` | 静态 GenVars 可写字段的 JSON 序列化/恢复 | `SnapshotGenVars.Serialize/DeserializeAndApply`；`confirmed/partial` |
| `TileStructCompressionQuery` | Tile header、frame、墙、液体和斜坡的规范化快照值 | `TileStruct.From`；`confirmed` |
| `TileSnapshotCaptureSystem` | 捕获 Tile、TileEntity、Chest 的一致性视图 | `TileSnapshot.Create`、`Save*`；`missing` |
| `TileSnapshotPersistenceAdapter` | 固定结构写入、版本/尺寸校验和读取 | `TileSnapshot.Save/Load`；`partial` |
| `TileSnapshotRestoreCommand` | 原子恢复 Tile/TileEntity/Chest 并清理 NPC | `WorldGenSnapshot.Restore`、`TileSnapshot.Restore`；`missing` |
| `WorldGenSnapshotValidityQuery` | Manifest、版本、pass 启用状态和世界哈希新鲜度 | `WorldGenSnapshot.Outdated`、`TryResetToSnapshot`；`partial` |
| `WorldDeterminismHashQuery` | 由规范化 TileStruct 生成稳定世界哈希 | `WorldGenerator.HashWorld` + `GetHashCode`；`missing` |
| `SnapshotResyncProjection` | Dedicated Server 恢复后向客户端重同步全图 Tile | `TileSnapshot.Restore` 中 `NetMessage.ResyncTiles`；`partial` |

快照捕获必须在单一生成阶段边界内完成，不能一边写 Tile 一边读取未冻结的数组；恢复应
一次性替换 Tile/TileEntity/Chest，再清理 NPC、刷新世界哈希并广播重同步。`GetHashCode`
和 Equals 不能依赖运行时对象地址，否则同一世界在不同进程中会产生不同 pass 快照判定。
当前 `Save*`/`Restore*` 空体和序列化转换器的默认返回值使“可恢复生成”保持
`partial/missing`。

```text
Completed GenPass + stable world state
  -> Manifest/GenVars snapshot + TileStruct normalization
  -> atomic Tile/TileEntity/Chest capture
  -> snapshot file adapter
  -> validity/hash query
  -> restore command -> NPC reset + server tile resync
```

最小 focused verifier 应覆盖：不同 Tile header 的规范化、FrameImportant/斜坡/墙/液体组合、
TileEntity 与 Chest 深拷贝、快照文件截断/尺寸版本拒绝、Manifest 过期判断、同世界跨进程
哈希稳定性、恢复后 NPC/实体清理、Dedicated Server 全图重同步，以及恢复过程中不暴露
半恢复世界。当前快照保存/恢复核心方法均无运行 verifier，相关回滚和确定性能力标记为
“未验证”。

本轮新增约 **12 个**边界（Manifest/GenVars 2，Tile 压缩/捕获/持久化/恢复 4，快照有效性、
世界哈希、重同步 3，另含原子冻结与跨进程一致性 seam 3）。累计独立边界约 **731 个**；至少
约 **650 个**仍有快照保存/恢复、世界哈希、Hall 连接、轨道生成、布局提供器、房间/入口
生成、地牢特征提交、微型 Biome Place、双地牢 ApplyPass、Terrain/Jungle 生成、Shimmer
启动与规避、宠物激活、厕所输出、FloodFill、光照传播或网络授权缺口。静态识别覆盖率保持
约 **98%～99%**；权威行为闭合度约 **12%～21%**。本轮只读取 `D:\TRbackup\Version4`，未
修改其源码，并继续排除 `D:\TRbackup\NLTX\src` 中已有组件。

## 99. 第七十五轮：WorldGenerator 控制器、Pass 执行与暂停/中止状态机

### 99.1 生成主循环存在，但关键执行钩子为空

`WorldGenerator.GenerateWorld` 初始化控制器、计算启用 pass 的总权重，循环检查
`QueuedAbort`/`Paused`，在控制锁内取当前 pass，调用 `RunPass`，把结果追加到
`WorldGen.Manifest.GenPassResults`，然后通知 `OnPassCompleted`（`Terraria.WorldBuilding/WorldGenerator.cs:226-285`）。
这定义了生成系统的并发边界、暂停点、顺序和结果提交点。

但是 `_controller.SetGenerator(this)`、`OnPaused`、`OnPassCompleted`、`UpdatePreviousManifest`、
`ReportException`、`SetDebugWorldGenUIVisibility` 和 `RunPass` 均为空或默认返回
（`WorldGenerator.cs:88-90,194-194,286-287,308-309`）。尤其 `RunPass` 恒返新的空
`GenPassResult`，当前不能证明任何 `GenPass.Apply`、耗时统计、跳过状态、哈希或异常处理
实际发生；生成循环“追加结果”不等于世界已经执行生成。

### 99.2 暂停、快照和上一份 Manifest 需要显式状态转换

`Controller` 公开 `PauseAfterPass`、`PauseOnHashMismatch`、`PausedDueToHashMismatch`、
`SnapshotFrequency`、`QueuedAbort` 和 `Passes/CurrentPass/LastCompletedPass`；
`TryRunToEndOfPass` 会根据现有快照或已完成结果决定恢复、重置或设置下一个暂停点
（`WorldGenerator.cs:44-81,240-285`）。但 `OnPassCompleted` 与 `UpdatePreviousManifest`
为空，因而无法确认何时创建自动快照、何时比较哈希、何时保存上一份 Manifest、何时清理
`PauseAfterPass`，也无法证明 HashMismatch 会真正暂停而不是仅设置字段。

`GenerationProgress` 能够计算当前 pass 权重和总进度，`WorldManifest` 能序列化/克隆
`GenPassResult`；但 `GenPassResult.ToString` 为空，`RunPass` 未填充 `DurationMs`、`Hash`、
`Skipped`，所以 UI/日志和恢复选择缺少可信结果。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `WorldGenerationControllerComponent` | pass 列表、当前/最近 pass、暂停、中止和快照策略 | `WorldGenerator.Controller`；`confirmed/partial` |
| `GenerationExecutionLock` | 控制锁、当前 pass 独占和暂停/恢复并发边界 | `_controlLock`、`lock(_currentPass)`；`confirmed/partial` |
| `GenPassExecutionSystem` | 调用 Apply、计时、跳过、哈希和异常转译为结果 | `RunPass`；`missing` |
| `PassCompletionStateSystem` | 结果追加、上一 Manifest、自动快照和暂停点转换 | `OnPassCompleted`、`UpdatePreviousManifest`；`missing` |
| `GenerationAbortCommand` | 排队中止、停止当前调度并清理当前状态 | `QueuedAbort`、GenerateWorld loop；`partial` |
| `GenerationPausePolicy` | PauseAfterPass、手动暂停和 HashMismatch 暂停 | `TryRunToEndOfPass`、`OnPaused`；`partial/missing` |
| `GenerationProgressProjection` | 当前 pass/总权重进度、消息和调试 UI | `GenerationProgress`、`ForceUpdateProgress`；`partial` |
| `GenPassResultComponent` | DurationMs、Hash、Skipped 结果记录 | `GenPassResult`；`partial`（ToString 空） |
| `GenerationExceptionAdapter` | 将 pass 异常记录、通知并决定继续/中止 | `ReportException`；`missing` |

生成主循环应采用显式状态机：`Idle -> RunningPass -> Completed/Skipped -> SnapshotOrPause -> Next`
或 `Abort/Failed`。`RunPass` 不能吞掉异常后返回空结果；必须在同一控制锁内决定
`GenPassResult`、Manifest、快照和暂停状态。`WorldManifest.FinalHash` 只有在最后一个
结果拥有可信 Hash 时才可用于快照有效性，不能以空 `Hash` 或默认结果绕过校验。

```text
Controller command
  -> lock + pass selection
  -> RunPass (Apply + timing + hash + exception policy)
  -> GenPassResult + Manifest update
  -> completion/snapshot/hash-mismatch pause
  -> next pass or abort/failure
```

最小 focused verifier 应覆盖：空 pass 列表、禁用 pass、单 pass 成功/跳过/异常、暂停后
恢复、中止前后清理、HashMismatch 暂停、自动/手动快照频率、上一 Manifest 更新、控制锁
重入、权重进度单调性、`DurationMs/Hash/Skipped` 填充以及失败结果不被误认作完成。当前
`RunPass` 和多个 Controller 钩子为空，相关生成执行闭环标记为“未验证”。

本轮新增约 **14 个**边界（控制器/锁/执行/完成/中止/暂停/进度/结果/异常 9，另含
快照、Manifest 与哈希交接 seam 5）。累计独立边界约 **745 个**；至少约 **664 个**仍有
生成执行控制、快照保存/恢复、世界哈希、Hall 连接、轨道生成、布局提供器、房间/入口生成、
地牢特征提交、微型 Biome Place、双地牢 ApplyPass、Terrain/Jungle 生成、Shimmer 启动与
规避、宠物激活、厕所输出、FloodFill、光照传播或网络授权缺口。静态识别覆盖率保持约
**98%～99%**；权威行为闭合度约 **12%～21%**。本轮只读取 `D:\TRbackup\Version4`，未修改
其源码，并继续排除 `D:\TRbackup\NLTX\src` 中已有组件。

## 100. 第七十六轮：Mystic Log 仙灵事件、枯木扫描与地下仙灵刷怪资格

### 100.1 事件拥有真实的世界/夜晚/清理生命周期

`MysticLogFairiesEvent` 保存 `_canSpawnFairies`、下次尝试延迟和枯木坐标列表
（`Terraria.GameContent.Events/MysticLogFairiesEvent.cs:9-16`）。`WorldClear` 清空状态；
`StartWorld`、`StartNight` 和 `FallenLogDestroyed` 会扫描世界；`UpdateTime` 在允许刷怪且
通过时间资格时按 `Main.dayRate` 递减计时，每 60 tick 调用一次尝试
（`:19-68`）。`WorldGen` 在世界加载、每日夜晚、世界清理和枯木 Tile 被破坏时接入这些
入口（`Terraria/WorldGen.cs:3351-3354,6452-6455,42751-42754`）；`Main.UpdateTime` 每帧
调用 `UpdateTime`（`Terraria/Main.cs:13111-13118`）。

扫描实现会按普通/Remix 世界的地表-岩层范围遍历 Tile，收集 active、类型 488、无液体的
枯木，并设置 `NPC.Spawner.fairyLog`（`:70-103`）。NPC 刷怪器的
`CheckToSpawnUndergroundFairy` 会读取该标志，再应用稀有度、世界深度、Hardmode、已有
Helpful Fairy 等资格（`Terraria/NPC.cs:5703-5732`）。但是 `IsAGoodTime` 和
`TrySpawningFairies` 为空，`GetStumpTopLeft` 返回默认点（`MysticLogFairiesEvent.cs:59-62,105-107`），
所以扫描标志和 NPC 刷怪资格存在，具体“按哪根枯木、何时生成、生成何种仙灵”的事件提交
仍未闭合。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `MysticFairyEventStateComponent` | 夜晚允许标志、60 tick 尝试延迟和枯木坐标 | `MysticLogFairiesEvent.cs:9-16,35-57`；`confirmed/partial` |
| `FallenLogScanQuery` | 普通/Remix 世界地表范围内的枯木 Tile 候选 | `ScanWholeOverworldForLogs`；`confirmed/partial` |
| `StumpAnchorQuery` | 从检测点换算枯木左上角/可生成锚点 | `GetStumpTopLeft`；`missing` |
| `FairySpawnTimeQualificationQuery` | 时间、事件状态、世界难度与玩家环境资格 | `IsAGoodTime`；`missing` |
| `UndergroundFairySpawnQualificationQuery` | fairyLog、深度、稀有度和已有仙灵检查 | `NPC.Spawner.CheckToSpawnUndergroundFairy`；`confirmed/partial` |
| `MysticFairySpawnCommand` | 选择枯木、生成仙灵 NPC、扣除尝试并发布结果 | `TrySpawningFairies`；`missing` |
| `FairyLogWorldFlagComponent` | 向 NPC Spawner 暴露是否存在有效枯木 | `NPC.Spawner.fairyLog`；`confirmed/partial` |
| `MysticFairyWorldLifecycleSystem` | 世界加载/夜晚/破坏枯木/世界清理的状态交接 | `WorldGen`、`Main.UpdateTime` 调用；`confirmed/partial` |

`fairyLog` 只是候选存在标志，不应被当成已经生成仙灵；NPC Spawner 的随机资格也不应
直接修改枯木列表。推荐顺序是“扫描快照 → 时间/深度 Query → 选择锚点 → NPC Spawn
Command → 清理/重扫”，并确保客户端不能通过同步 `fairyLog` 自行生成 NPC。

```text
World load/night/log destruction
  -> FallenLogScanQuery -> FairyLogWorldFlagComponent
  -> FairySpawnTime + UndergroundFairy qualification
  -> StumpAnchorQuery -> MysticFairySpawnCommand
  -> NPC spawn + event state/cooldown update
```

最小 focused verifier 应覆盖：普通/Remix 扫描范围、Tile 类型 488 和液体排除、多个枯木
去重、枯木左上角换算、夜晚/白天边界、Main.dayRate 变化、Hardmode 稀有度、深度限制、
Helpful Fairy 冲突、生成失败后的冷却与重扫、枯木破坏后的标志清理，以及服务端权威 NPC
生成。当前 `IsAGoodTime`、`TrySpawningFairies` 和 `GetStumpTopLeft` 无运行 verifier，
相关事件提交标记为“未验证”。

本轮新增约 **10 个**边界（事件状态/扫描/锚点/时间资格/NPC 资格/生成/标志/生命周期 8，
另含服务器生成授权与扫描-生成交接 seam 2）。累计独立边界约 **755 个**；至少约 **674 个**
仍有仙灵事件提交、生成执行控制、快照保存/恢复、世界哈希、Hall 连接、轨道生成、布局
 提供器、房间/入口生成、地牢特征提交、微型 Biome Place、双地牢 ApplyPass、Terrain/Jungle
 生成、Shimmer 启动与规避、宠物激活、厕所输出、FloodFill、光照传播或网络授权缺口。静态
 识别覆盖率保持约 **98%～99%**；权威行为闭合度约 **12%～21%**。本轮只读取
 `D:\TRbackup\Version4`，未修改其源码，并继续排除 `D:\TRbackup\NLTX\src` 中已有组件。

## 101. 第七十七轮：NPC 特殊刷怪资格（蜘蛛、岩石巨人与熔岩诱饵）

### 101.1 刷怪分支有明确调用者，但资格与生成函数未闭合

NPC Spawner 在不同刷怪上下文中调用三个特殊分支：墙体类型 62 或 `spawnSpider` 时调用
`CheckToSpawnSpider`，成功后按深度、世界种子和区域继续选择蜘蛛 NPC（`Terraria/NPC.cs:1575-1586`）；
地下/熔岩环境的随机分支调用 `SpawnLavaBaitCritters`（`:2486-2493,4787-4791`）；
普通地下分支调用 `CheckToSpawnRockGolem`，成功后生成类型 631（`:4827-4830`）。

但 `CheckToSpawnSpider` 与 `CheckToSpawnRockGolem` 直接返回默认 `false`，
`SpawnLavaBaitCritters` 直接返回 `new NPC()`，没有设置类型、位置、来源、active、网络同步或
生成失败处理（`NPC.cs:5697-5704,5737-5740`）。这说明普通刷怪器的上下文和后续分支仍在，
但三类特殊刷怪的核心资格/提交边界缺失，不能仅凭调用点推断蜘蛛、岩石巨人或熔岩诱饵已可生成。

`CheckToSpawnUndergroundFairy` 则是对照组：它真实检查 `fairyLog`、深度、Hardmode 稀有度和
已有 Helpful Fairy（`NPC.cs:5705-5735`），证明本文件中不同特殊刷怪分支的实现完整度并不一致，
不能把一个已实现 Query 的存在扩散到其它空方法。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `SpecialSpawnContextComponent` | 当前生成坐标、地表/岩层/熔岩环境、墙体和区域标志 | NPC Spawner 调用上下文；`confirmed/partial` |
| `SpiderSpawnQualificationQuery` | 蜘蛛墙体、spawnSpider、深度/种子/区域和已有 NPC 限制 | `CheckToSpawnSpider`；`missing` |
| `SpiderSpawnCommand` | 选择蜘蛛类型、设置来源/位置并注册 NPC | `NPC.cs:1575-1586` 后续分支；`partial/missing` |
| `RockGolemSpawnQualificationQuery` | Tile 类型、地下深度、难度、事件和已有数量限制 | `CheckToSpawnRockGolem`；`missing` |
| `RockGolemSpawnCommand` | 生成类型 631 并同步 NPC | `NPC.cs:4827-4830`；`partial` |
| `LavaBaitCritterQualificationQuery` | 熔岩、诱饵/钓鱼环境、稀有度和可生成空间 | 调用分支 `NPC.cs:2486-2493,4787-4791`；`missing` |
| `LavaBaitCritterSpawnCommand` | 创建实际 Critter NPC、设置 active/来源/网络状态 | `SpawnLavaBaitCritters`；`missing` |
| `SpecialSpawnFailurePolicy` | 资格失败、NPC 槽满、位置无效时的回退与重试 | Spawner 外层分支；`partial/missing` |

这些 Query 必须读取服务器权威 Tile/玩家环境快照，不能接受客户端指定 NPC 类型或把
`new NPC()` 当作已经提交的实体。SpawnCommand 应在实体数组、位置碰撞和来源归因通过后一次性
写入，并与 `NPC.Spawner` 的全局 spawn rate/slot 预算保持一致。

```text
Spawner context + authoritative Tile/player snapshot
  -> Spider/RockGolem/LavaBait qualification query
  -> special spawn command
  -> NPC slot + source attribution + network projection
  -> spawn budget/retry policy
```

最小 focused verifier 应覆盖：蜘蛛墙体 62 与 `spawnSpider` 开关、深度/特殊种子分支、岩石
巨人 Tile/难度/事件条件、熔岩诱饵的液体和空间资格、NPC 槽满、位置碰撞、重复生成抑制、
实体来源、active 位、网络同步以及失败时不消耗成功配额。当前三类特殊刷怪的关键方法均无
运行 verifier，相关能力标记为“未验证”。

本轮新增约 **9 个**边界（特殊刷怪上下文、蜘蛛 Query/Command、岩石巨人 Query/Command、
熔岩诱饵 Query/Command、失败策略 8，另含实体槽位与服务器授权 seam 1）。累计独立边界约
**764 个**；至少约 **683 个**仍有特殊刷怪、仙灵事件提交、生成执行控制、快照保存/恢复、
世界哈希、Hall 连接、轨道生成、布局提供器、房间/入口生成、地牢特征提交、微型 Biome
Place、双地牢 ApplyPass、Terrain/Jungle 生成、Shimmer 启动与规避、宠物激活、厕所输出、
FloodFill、光照传播或网络授权缺口。静态识别覆盖率保持约 **98%～99%**；权威行为闭合度约
**12%～21%**。本轮只读取 `D:\TRbackup\Version4`，未修改其源码，并继续排除
`D:\TRbackup\NLTX\src` 中已有组件。

> 续记：本轮新增的世界删除、暂停/退出、世界持久化边界已在第 102～104 节登记；后续继续避开特殊刷怪分支。

## 105. 第八十一轮：地图缓存构造、脏区队列与客户端地图投影

`Main.Map` 持有 `WorldMap`，`WorldGen` 在世界清理后调用 `Main.Map.Clear`，网络
`MessageBuffer` 和地形修改路径调用 `MapUpdateQueue.Add`（`WorldGen.cs:6428,67215,68029`、
`MessageBuffer.cs:820`）。但是 `WorldMap` 构造函数没有分配 `_tiles`，`QueueUpdate` 恒返回
默认 false；因此索引访问和脏区更新无法形成有效地图状态。`MapUpdateQueue.Add` 虽检查服务器、
世界生成和地图开关，却没有把区域加入 `_areaUpdateQueue`，更新预算与消费系统均未闭合。

`MapHelper.SaveMap` 具备云端可用性、锁和异常日志外壳，但 `InternalSaveMap` 为空
（`Terraria.Map/MapHelper.cs:97-122`），`WorldMap.Save` 与玩家存档确实会到达它。该链是
客户端地图投影/持久化 Adapter，不应被当作服务器权威 Tile 状态；但其缺失会让探索进度、
颜色、液体和 Shimmer 标志无法可靠保存。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `WorldMapTileComponent` | 每格 Type、Light、Color、changed/queued 位 | `MapTile` 字段与属性；`confirmed` |
| `WorldMapAllocationSystem` | 按世界尺寸分配并清理二维地图缓存 | `WorldMap` 构造空体；`missing` |
| `MapDirtyRegionQueue` | 坐标/矩形去重、容量上限和消费顺序 | `MapUpdateQueue.Add` 不入队；`missing` |
| `MapTileProjectionSystem` | 从权威 Tile/光照快照计算 MapTile | `QueueUpdate` 无提交；`partial/missing` |
| `MapPersistenceAdapter` | 本地/云地图压缩、版本头、增量写入 | `InternalSaveMap` 空体；`missing` |
| `MapResetCommand` | 世界重置时清空地图并同步投影 | `WorldGen` 调 `Main.Map.Clear`；`partial` |

```text
authoritative tile/light change -> MapDirtyRegionQueue -> MapTileProjectionSystem
 -> WorldMapTileComponent -> MapPersistenceAdapter / minimap Projection
```

最小 verifier：构造不同尺寸地图、越界坐标、区域去重/容量上限、Tile/Wall/Liquid/Color/光照
更新、世界生成期间抑制更新、客户端地图保存恢复以及云端不可用时的本地回退。当前队列、
构造和保存核心均无运行 verifier，标记为“未验证”。

## 106. 第八十二轮：玩家文件序列化与地图保存耦合

`Player.SavePlayer` 在保存玩家前调用成就保存和 `InternalSaveMap`，随后通过
`InternalSavePlayerFile` 写入玩家文件（`Terraria/Player.cs:26396-26417`）。但
`InternalSavePlayerFile`、`Serialize`、`Deserialize` 都为空或只返回默认输出；
`SerializedClone` 仍会调用这条序列化/反序列化链（约 `:26449-26456`）。因此库存、装备、
Buff、任务、位置、死亡/重生和玩家配置没有可证明的持久化提交，地图保存与玩家文件也缺少
一致性边界。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `PlayerPersistenceComponent` | 玩家身份、位置、库存、装备、Buff 和进度 | `Player` 状态被 Save/Clone 读取；`partial` |
| `PlayerSerializeAdapter` | 版本化字段写入、临时文件和原子替换 | `InternalSavePlayerFile/Serialize` 空体；`missing` |
| `PlayerDeserializeAdapter` | 版本探测、字段校验、兼容修复 | `Deserialize` 空体；`missing` |
| `PlayerMapSaveCoordinator` | 玩家文件与地图文件顺序、失败回滚和云标志 | `SavePlayer` 先地图后玩家但无事务；`partial/missing` |
| `SerializedCloneQuery` | 生成 UI/预览副本而不改变权威玩家 | 调用完整但底层序列化为空；`partial` |

建议把玩家快照编码与地图投影分离，以明确“玩家权威状态 → 玩家存档”和“Tile/光照 → 地图
投影”两条 Adapter；失败时保留旧文件并返回结构化错误，不要由空 catch 掩盖损坏。

## 107. 第八十三轮：地图覆盖层投影与网络更新消费

`SpawnMapLayer`、`TeamBasedSpawnMapLayer`、`PingMapLayer`、`TeleportPylonsMapLayer` 的
`Draw` 方法均为空，而这些层由地图覆盖层渲染路径实例化；它们属于 Projection，不应被误报
为权威出生点、队伍、Ping 或传送晶塔状态。当前 `MapOverlayDrawContext` 只提供绘制接口，
没有可见的文本/图标提交，因此网络收到的地图更新即便进入 `MapUpdateQueue`，也缺少最终
消费与表现验证。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `MapOverlayLayerQuery` | 从玩家/网络快照选择可见覆盖层实体 | 各 Layer `Draw` 空体；`partial` |
| `SpawnOverlayProjection` | 出生点图标与文本 | `SpawnMapLayer.Draw`；`missing` |
| `TeamSpawnOverlayProjection` | 队伍颜色/出生点投影 | `TeamBasedSpawnMapLayer.Draw`；`missing` |
| `PingOverlayProjection` | Ping 生命周期、距离和文本 | `PingMapLayer.Draw`；`missing` |
| `PylonOverlayProjection` | 晶塔资格/名称/图标投影 | `TeleportPylonsMapLayer.Draw`；`missing` |
| `MapUpdateConsumerSystem` | 消费脏区队列并触发地图重绘/网络投影 | 未找到消费实现；`missing` |

本轮新增约 **18 个**边界（地图缓存/队列/保存 6、玩家持久化 5、覆盖层与消费 7）。累计
独立边界约 **799 个**；主要未闭合领域进一步包括地图构造与保存、玩家序列化、覆盖层消费、
暂停/退出、世界删除、种子解析、云本地迁移、快照恢复、世界哈希、生成执行控制、地牢/轨道/生态
生成、事件提交、实体网络授权和光照传播。静态识别覆盖率约 **98%～99%**，权威行为闭合度约
**12%～21%**。本轮只读 `D:\TRbackup\Version4`，未修改源码，也未重新展开特殊刷怪分支。

## 108. 第八十四轮：权威实体更新调度与异常隔离策略

`Main.DoUpdateInWorld` 对玩家、NPC、Projectile 和 WorldItem 使用固定顺序逐槽更新，随后推进时间、世界更新和入侵状态（`Terraria/Main.cs:11411-11600`）。这证明它是权威实体模拟的调度 System，而不是单纯的渲染循环。但在 `ignoreErrors` 模式下，NPC 更新异常会把对应槽替换为 `new NPC()`，Projectile 异常替换为 `new Projectile()`，WorldItem 异常替换为 `new WorldItem()` 并重设索引；异常原因、实体来源、网络同步、掉落回滚和重试预算均未保留。玩家异常则仅吞掉后继续下一槽，可能留下半更新状态。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `EntityUpdateOrderComponent` | 玩家→NPC→Projectile→Item→世界时间的顺序 | `DoUpdateInWorld` 固定循环；`confirmed` |
| `EntityUpdateSystem` | 每帧调用各实体状态变换并收集 FrameFlags | 调度和计数存在；`confirmed/partial` |
| `EntityFaultPolicy` | 捕获异常、隔离实体、记录原因和决定重试/销毁 | 直接 new 空实体或吞异常；`partial/missing` |
| `EntityReplacementCommand` | 槽位替换后的 UUID、来源、网络和资源清理 | 替换未提交元数据；`missing` |
| `FrameConsistencyQuery` | 检测半更新玩家、Boss 标志和实体计数不一致 | 仅部分 FrameFlags；`partial/missing` |

## 109. 第八十五轮：Projectile AI 状态机与命中副作用提交

`Projectile.Update` 对每个活动投射物执行越界、局部无敌计时、召唤物预算、碰撞液体检测、`AI()`、Kill 和网络更新（`Terraria/Projectile.cs:14693` 起）。统一 AI 状态机按类型分派到大量 `AI_*` 方法，其中多个方法为空，例如 `AI_151_SuperStar`、`AI_152_SuperStarSlash`、`AI_197_HandleTileCollision`、`AI_203_StormLightning`、`AI_205_RemoteControlCar`（`Projectile.cs:18612-18666,32965-33040`）。这些方法由统一 Update 的类型分派可达，对应移动、碰撞、伤害、生成子弹、粒子和 `netUpdate` 提交均不能证明完整。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `ProjectileStateComponent` | 位置、速度、AI 参数、寿命、穿透和免疫计时 | `Projectile.Update` 读取/写入；`confirmed` |
| `ProjectileTypeAIQuery` | 按类型选择 AI 状态处理器 | `AI()` 分派入口；`partial` |
| `ProjectileMotionSystem` | 移动、重力、风、液体和边界行为 | Update 外壳存在，多个 AI 空体；`partial/missing` |
| `ProjectileHitCommand` | 碰撞目标、伤害、穿透、免疫和 Kill | 命中/副作用分支不完整；`partial/missing` |
| `ProjectileSpawnCommand` | 子投射物、尘埃、光照和来源归因 | 多个类型 AI 空体；`missing` |
| `ProjectileNetworkProjection` | `netUpdate`、Kill 和重要状态同步 | 字段被写但消费链未闭合；`partial` |

## 110. 第八十六轮：WorldItem 环境生命周期与 Shimmer/Lava 清理

`Main.DoUpdateInWorld` 每帧调用 400 个 `WorldItem.UpdateItem`。该方法真实处理所有权计时、抓取、重力、液体、越界、自动消失、网络广播和 Shimmer 表现；但 `CheckLavaDeath`、`Shimmering` 和 `TryGrantingMakeAWishSet` 为空（`Terraria/WorldItem.cs:580,624-625`）。这些方法位于 WorldItem 更新的环境/奖励生命周期中，缺失会导致熔岩销毁、Shimmer 转化/消耗、特殊掉落和对应 NetMessage 不可证明。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `WorldItemStateComponent` | 类型、堆叠、所有权、抓取、液体和 Shimmer 状态 | `UpdateItem` 状态字段；`confirmed` |
| `ItemEnvironmentQuery` | 越界、熔岩、蜂蜜、水和 Shimmer 资格 | `UpdateItem`/空清理方法；`partial` |
| `LavaItemDespawnSystem` | 不可恢复物品销毁、重要物品回收和同步 | `CheckLavaDeath` 空体；`missing` |
| `ShimmerTransformationSystem` | Shimmer 转化、时间、位置和网络提交 | `Shimmering` 空体；`missing` |
| `MakeAWishRewardCommand` | 特定物品的奖励、去重和来源 | `TryGrantingMakeAWishSet` 空体；`missing` |
| `WorldItemNetworkProjection` | TurnToAir/位置/堆叠的服务器广播 | 部分 `NetMessage.SendData(21)`；`partial` |

本轮新增约 **18 个**边界（实体调度/故障隔离 5、Projectile 状态与命中 6、WorldItem 环境生命周期 7）。累计独立边界约 **817 个**；主要未闭合领域包括实体异常策略、投射物 AI、WorldItem Shimmer/Lava、地图与玩家持久化、暂停/退出、世界删除、种子解析、云本地迁移、快照恢复、世界哈希、生成执行控制、地牢/轨道/生态生成、事件提交、网络授权和光照传播。静态识别覆盖率约 **98%～99%**，权威行为闭合度约 **12%～21%**。本轮仍未重新展开特殊刷怪分支。

## 111. 第八十七轮：液体传播、缓冲区与网络提交

`Liquid.UpdateLiquid`、`AddWater`、`DelWater`、`LiquidCheck` 和 `LiquidBuffer` 构成真实液体传播主链（`Terraria/Liquid.cs:1015-1184,1192-1529`），并由 Tile 修改、世界生成和 `NetLiquidModule.CreateAndBroadcastByChunk` 触发。主算法包含容量、延迟、跳过标记和卡死检测；但 `CreateLiquidMergeTile` 为空（约 `Liquid.cs:1322-1324`），因此四种液体接触时的合并 Tile 提交不完整。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `LiquidCellComponent` | 液体量、类型、checking/skip/kill/delay 状态 | `Liquid`/Tile 字段；`confirmed` |
| `LiquidPropagationSystem` | 重力扩散、缓冲区、容量和卡死恢复 | `UpdateLiquid` 主链；`confirmed/partial` |
| `LiquidMergeQuery` | 四种液体接触与合并 Tile 资格 | `GetLiquidMergeTypes`；`confirmed/partial` |
| `LiquidMergeCommand` | 创建合并 Tile 并清理液体 | `CreateLiquidMergeTile` 空体；`missing` |
| `LiquidNetworkProjection` | 按 Chunk 广播液体变化 | `NetSendLiquid`/`NetLiquidModule`；`partial` |
| `LiquidFailurePolicy` | 满缓冲、异常、卡死和重试配额 | `StartPanic`/`stuckAmount`；`partial` |

## 112. 第八十八轮：光照引擎选择、区域处理与缓存生命周期

`Lighting` 的静态入口被玩家、NPC、Projectile、Mount、WorldItem 和 Tile 扫描广泛调用，包括 `AddLight`、`GetColor` 和 `Clear`。但 `LightingEngine.AddLight/Clear/ProcessArea/Rebuild` 以及 `LegacyLighting.Rebuild/AddLight/ProcessArea/Clear` 全部为空（`Terraria.Graphics.Light`），导致光照缓存没有区域重建、光源累积或清理提交。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `LightSourceComponent` | 坐标、RGB 强度、来源和生命周期 | 实体调用 `Lighting.AddLight`；`partial` |
| `LightingEngineSelector` | Legacy/新引擎选择与世界切换 | `Lighting` 静态入口；`partial` |
| `LightAccumulationSystem` | 合并光源、衰减、边界裁剪和优先级 | 两个引擎方法空体；`missing` |
| `LightRegionRebuildSystem` | Tile 区域扫描、传播和缓存重建 | `ProcessArea/Rebuild` 空体；`missing` |
| `LightQuery` | 向碰撞、实体和绘制读取稳定颜色 | `GetColor` 消费者存在但缓存无来源；`partial/missing` |
| `LightCacheResetCommand` | 世界清理/时间切换时失效旧缓存 | `Lighting.Clear` 调用点存在；`partial/missing` |

## 113. 第八十九轮：液体/光照与实体碰撞的跨域交接

Projectile、NPC、Player 和 WorldItem 更新会查询 `Collision.WetCollision`、`LavaCollision`、`Collision.shimmer` 并调用 `Lighting.AddLight`；液体传播又会修改 Tile 并触发网络广播。这形成“Tile → 液体 → 碰撞快照 → 实体状态 → 光照/网络投影”的跨域链。当前各模块直接共享 `Main.tile` 和静态标志，没有只读快照或统一提交边界；异常时 Projectile 会直接失活，实体异常则可能替换对象，液体广播仍可能已发送。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `TileEnvironmentSnapshot` | 单帧 Tile、液体、墙体和光照输入 | 多实体直接读取 `Main.tile`；`missing` |
| `CollisionEnvironmentQuery` | Wet/Lava/Honey/Shimmer/固体资格 | `Collision` 与实体 Update；`partial` |
| `EnvironmentStateCommit` | 将碰撞结果写回实体和 Tile | 各模块分散写入；`partial/missing` |
| `EnvironmentProjectionCoordinator` | 液体网络、光照缓存和实体网络的一致顺序 | 无统一协调器；`missing` |
| `CrossDomainFailurePolicy` | 碰撞异常、越界、网络失败的回滚与重试 | 多处 catch 后失活/吞异常；`partial/missing` |

最小 verifier：同一帧液体传播与实体碰撞读取一致、液体合并后光照重建、Shimmer/Lava 状态改变时网络顺序、越界和异常回滚、服务器与客户端投影不分叉。当前跨域链无运行 verifier，标记为“未验证”。

本轮新增约 **18 个**边界（液体传播/合并/网络 6、光照引擎与缓存 6、液体-光照-碰撞交接 6）。累计独立边界约 **835 个**；主要未闭合领域包括液体合并、光照引擎、跨域环境快照、实体异常策略、投射物 AI、WorldItem 环境生命周期、地图与玩家持久化、暂停/退出、世界删除、种子解析、云本地迁移、快照恢复、世界哈希、生成执行控制、地牢/轨道/生态生成、事件提交与网络授权。静态识别覆盖率约 **98%～99%**，权威行为闭合度约 **12%～21%**。本轮仍未重新展开特殊刷怪分支。

## 114. 第九十轮：连接接受、满服拒绝与服务器监听生命周期

`Netplay.OnConnectionAccepted` 是真实连接入口：它寻找客户端槽位、重置 `RemoteClient`、绑定 Socket，并在无空槽位时调用 `KickClient`；所有槽位占满后还调用 `StopListening` 并清除 `IsListening`（`Terraria/Netplay.cs:234-246`）。但 `KickClient` 和 `StopListening` 均为空体，因此满服客户端不一定收到拒绝包，监听器资源和底层回调也没有可证明地关闭。`StartListening` 与 `StartServer` 真实启动网络线程，形成明确的生命周期断链。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `ConnectionSlotComponent` | 槽位、Socket、握手状态和超时计时 | `Clients[num].Reset/Socket`；`confirmed/partial` |
| `ConnectionAcceptanceQuery` | 空槽位、服务器容量和网络后端资格 | `FindNextOpenClientSlot`；`confirmed` |
| `RejectConnectionCommand` | 满服/非法连接的拒绝原因和发送 | `KickClient` 空体；`missing` |
| `ListenerLifecycleSystem` | 启动、停止、回调注销和 `IsListening` 状态 | `StopListening` 空体；`partial/missing` |
| `ConnectionFailureProjection` | 客户端可见错误、日志和诊断计数 | 仅部分 `LogHandshake`；`partial` |

## 115. 第九十一轮：网络临时 AI 缓冲与实体同步一致性

`MessageBuffer` 在读取 Projectile/NPC 同步包时使用可复用临时 AI 数组；`ReUseTemporaryProjectileAI`
会清空并返回缓冲区，而 `ReUseTemporaryNPCAI` 直接返回默认值（`Terraria/MessageBuffer.cs:113-123`）。
这条路径由 `GetData` 的实体同步包解析可达，可能让 NPC AI 参数为空、旧值泄漏或读取偏移错误。
由于同步包随后会更新 `Main.npc` 并转发 `NetMessage`，该缺口同时影响服务器权威状态和客户端投影。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `TemporaryProjectileAIComponent` | 临时 Projectile AI 参数清零与复用 | `ReUseTemporaryProjectileAI`；`confirmed` |
| `TemporaryNPCAIComponent` | 临时 NPC AI 参数分配、清零和生命周期 | `ReUseTemporaryNPCAI` 默认返回；`missing` |
| `EntitySyncDecodeSystem` | 包长度、字段版本和 AI 参数解码 | `GetData` 实体包分支；`partial` |
| `EntitySyncCommitCommand` | 将解码状态原子写入实体并设置网络标志 | 直接写入实体，缺少事务边界；`partial/missing` |
| `SyncDecodeFailurePolicy` | 短包、非法索引、异常和旧缓冲回收 | 多处分支仅 Boot/吞异常；`partial/missing` |

## 116. 第九十二轮：Tile 区域重同步与客户端世界一致性

`NetMessage.ResyncTiles(Rectangle area)` 遍历活动客户端并调用 `ResyncTiles(clientId, area)`；
外层调用可由 Tile 修改、液体变化和世界修复触发（`Terraria/NetMessage.cs:2431`）。但实际
区域实现为空体，因此客户端不会收到区域 Tile、墙体、液体、TileEntity 或相关 NPC/箱子重同步。
对照 `SendSection` 会设置 `TileSections`、发送 200×150 区块并同步 NPC/箱子，说明重同步是独立
的权威网络 Command，不应由普通地图 Projection 代替。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `TileResyncRequestComponent` | 脏区域、请求来源和优先级 | `ResyncTiles(area)`；`partial` |
| `TileRegionSnapshotQuery` | 收集 Tile/墙体/液体/实体数据 | `ResyncTiles` 空体；`missing` |
| `TileRegionResyncCommand` | 分块编码、发送和客户端确认 | 私有实现空体；`missing` |
| `TileEntityRegionProjection` | 区域内 TileEntity/NPC/箱子补发 | `SendSection` 有对照链，Resync 无；`partial/missing` |
| `ClientWorldConsistencyPolicy` | 丢包、重复包、确认超时和再次请求 | 未见闭合确认协议；`missing` |

最小 verifier：满服拒绝和监听停止、NPC/Projectile AI 临时缓冲清零、短包/非法索引处理、
Tile 区域修改后的多客户端重同步、液体和 TileEntity 一致性、断线重连后的区块补发。当前
网络生命周期与重同步核心均无运行 verifier，标记为“未验证”。

本轮新增约 **17 个**边界（连接生命周期 5、临时 AI 缓冲与实体同步 5、Tile 区域重同步 5，
另含跨域失败/确认 seam 2）。累计独立边界约 **852 个**；主要未闭合领域包括连接拒绝/监听
关闭、NPC 同步缓冲、Tile 重同步、液体与光照跨域、实体异常策略、投射物 AI、WorldItem 环境
生命周期、地图与玩家持久化、暂停/退出、世界删除、种子解析、云本地迁移、快照恢复、世界哈希、
生成执行控制、地牢/轨道/生态生成与事件提交。静态识别覆盖率约 **98%～99%**，权威行为闭合度
约 **12%～21%**。本轮仍未重新展开特殊刷怪分支。

## 117. 第九十三轮：Bestiary 发现、击杀与网络加入同步

`Main.DoUpdateInWorld` 每帧调用 `BestiaryTracker.Sights.ScanWorldForFinds`；NPC 更新、网络包和玩家交互会调用 `RegisterKill` 与 `RegisterChatStartWith`（`Terraria/Main.cs:11471`、`NPC.cs:45909,65328`、`MessageBuffer.cs:643,1971`）。这证明 Bestiary 是由实体事件驱动的玩家进度状态，而非纯 UI；但多个信息提供器返回默认值，加入同步和持久化增量链未闭合。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `BestiaryProgressComponent` | 发现、击杀、聊天解锁集合 | Tracker 被 Main/NPC/MessageBuffer 持有；`confirmed/partial` |
| `BestiarySightSystem` | 扫描可见 NPC/环境并登记发现 | `ScanWorldForFinds`；`partial` |
| `BestiaryKillCommand` | 击杀事件去重、归因和计数 | `RegisterKill`；`partial` |
| `BestiaryChatCommand` | NPC 聊天解锁和重复抑制 | `RegisterChatStartWith`；`partial` |
| `BestiaryInfoProjection` | 统计、稀有生成、掉落和名称信息 | 多个 provider 默认返回；`partial/missing` |
| `BestiaryJoinSyncAdapter` | 玩家加入时增量同步、版本和确认 | `OnPlayerJoining` 调用但协议不闭合；`partial/missing` |

## 118. 第九十四轮：Creative 解锁追踪、世界持久化与权限网络

世界重置调用 `CreativePowerManager.Instance.Reset`，初始化调用 `Initialize`，玩家持有 `CreativeUnlocksTracker` 并在联网时同步 Creative 权限（`WorldGen.cs:6566`、`Main.cs:3331`、`MessageBuffer.cs:592`）。但 `CreativeUnlocksTracker` 与 `ItemsSacrificedUnlocksTracker` 的 Save/Load/ValidateWorld/Reset/OnPlayerJoining 全部为空；多个 Creative Power 的 `DeserializeNetMessage`、`UsePower` 和玩家数据 Save/Load 也为空。世界文件保存壳存在，实际权限和研究状态仍未闭合。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `CreativeUnlockComponent` | 玩家研究、牺牲数量和解锁集合 | tracker 字段存在；`partial` |
| `SacrificeProgressSystem` | 物品研究累计、阈值和重复提交 | tracker 空体；`missing` |
| `CreativePowerStateComponent` | FreezeTime、天气、难度、放置范围等状态 | Main/NPC 查询；`partial` |
| `CreativePowerCommand` | 权限校验、使用 Power 和状态变更 | 多个 Use/反序列化空体；`missing` |
| `CreativeWorldPersistenceAdapter` | 世界级 Power 保存、加载和版本校验 | `WorldFile` 调用存在，tracker 空体；`partial/missing` |
| `CreativePermissionNetworkAdapter` | 玩家权限同步、拒绝和确认 | `SyncThingsToJoiningPlayer` 可达但子模块缺实现；`partial/missing` |

## 119. 第九十五轮：成就条件追踪与完成提交

`Player.SavePlayer` 调用 `Main.Achievements.Save`，Projectile 命中路径调用 `GetCondition("TO_INFINITY_AND_BEYOND", "Do").Complete()`（`Player.cs:26400`、`Projectile.cs:46564`）。但 `CustomFloatCondition.Clear/Load/GetValue/Complete` 均为空或默认返回，成就 tracker 的加载、清除、完成通知与社交平台 Adapter 没有完整证据；完成调用不能直接视为解锁已提交。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `AchievementConditionComponent` | 条件值、阈值、完成和版本状态 | 条件由实体事件读取；`partial` |
| `AchievementEventSystem` | 击杀、移动、物品和世界事件转为条件增量 | `GetCondition(...).Complete`；`partial` |
| `FloatConditionQuery` | 浮点进度读取、比较和边界 | `CustomFloatCondition` 默认值；`missing` |
| `AchievementCompletionCommand` | 原子完成、重复抑制和通知 | `Complete` 空体；`missing` |
| `AchievementPersistenceAdapter` | 玩家文件/平台存取和失败恢复 | `Achievements.Save` 可达但加载链不闭合；`partial/missing` |
| `AchievementProjection` | UI、声音和社交平台提示 | `AchievementsSocialModule` 存在但结果未验证；`partial` |

最小 verifier：Bestiary 发现/击杀/聊天去重、玩家加入同步、Creative 研究与权限拒绝、世界保存加载、成就浮点条件边界、重复完成和平台失败回滚。上述进度核心没有运行 verifier，标记为“未验证”。

本轮新增约 **18 个**边界（Bestiary 6、Creative 6、成就 6）。累计独立边界约 **870 个**；未闭合重点包括 Bestiary/Creative/成就进度提交、连接与 Tile 重同步、液体与光照跨域、实体异常策略、投射物 AI、WorldItem 环境生命周期、地图与玩家持久化、暂停/退出、世界删除、种子解析、云本地迁移、快照恢复、世界哈希、生成执行控制、地牢/轨道/生态生成和事件提交。静态识别覆盖率约 **98%～99%**，权威行为闭合度约 **12%～21%**。本轮仍未重新展开特殊刷怪分支。

## 120. 第九十六轮：TileEntity 注册、生命周期与更新集合

`TileEntity.InitializeAll` 在主初始化阶段创建并注册全部 TileEntity 类型；网络包和 WorldGen 会调用 `PlaceEntityNet`、`Add`、`Remove`、`Kill`，世界加载会通过 `LoadTileEntities` 恢复实体（`Main.cs:3344`、`MessageBuffer.cs:2551-2571`、`WorldFile.cs:3402`）。基础容器的 ID/位置字典和 `RequiresUpdates` 集合有实现，但基类 `Place` 返回默认值，多个实体的 `OnPlaced`、`OnRemoved`、`NetPlaceEntityAttempt` 和 `OnWorldLoaded` 为空，导致放置、销毁、加载回调与实体更新资格不完整。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `TileEntityIdentityComponent` | ID、坐标、类型和更新资格 | `ByID/ByPosition/UpdateEntities`；`confirmed` |
| `TileEntityPlacementQuery` | Tile 合法性、占用和服务器权限 | `PlaceEntityNet`/各 `IsTileValidForEntity`；`partial` |
| `TileEntityPlacementCommand` | 分配 ID、登记字典、执行 OnPlaced | 基类 `Place` 和多个回调空体；`partial/missing` |
| `TileEntityUpdateSystem` | 逐帧更新 RequiresUpdates 实体 | `PerformUpdates` 调用 `Update`；`partial` |
| `TileEntityRemovalCommand` | 删除字典、更新集合、掉落和 OnRemoved | `Remove/Kill` 主链存在，子类回调空；`partial/missing` |
| `TileEntityWorldLoadAdapter` | 版本读取、坐标校验和 OnWorldLoaded | `LoadTileEntities` 有清理/校验，回调不完整；`partial` |

## 121. 第九十七轮：容器与展示实体的物品交互提交

WorldGen 和网络包会对箱子、物品框、武器架、食物盘、展示人偶和展示罐执行创建、销毁、取出
与同步。多个 TileEntity 的交互资格方法返回默认值，`TEDisplayDoll`/`TEHatRack` 的
`OnPlayerUpdate` 和 `OnInventoryDraw` 为空，部分 `WriteExtraData/ReadExtraData` 也为空。
因此实体虽然能被保存或加入字典，物品放置、装备效果、取出副作用、掉落回滚和网络广播不能
证明完整；展示绘制本身是 Projection，但物品转移是权威状态。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `ContainerItemComponent` | 展示槽物品、堆叠、前缀和所有权 | 各 TE 持有 Item；`partial` |
| `ContainerInteractionQuery` | 空槽、占用、距离和可取出资格 | 网络分支/默认返回；`partial/missing` |
| `ContainerInsertCommand` | 放入物品、消耗来源并同步 | 多 TE 交互方法缺失；`missing` |
| `ContainerExtractCommand` | 取出/掉落物品和失败回滚 | WorldGen/MessageBuffer 可达；`partial/missing` |
| `DisplayEquipmentSystem` | 展示人偶/帽架装备对玩家属性影响 | `OnPlayerUpdate` 空体；`missing` |
| `ContainerPersistenceAdapter` | ExtraData 版本化保存、加载和校验 | 多个 ExtraData 空体；`partial/missing` |

## 122. 第九十八轮：逻辑传感器、训练假人与晶塔生命周期

`TELogicSensor`、`TETrainingDummy` 和 `TETeleportationPylon` 都由 TileEntity 管理器注册，
并在 WorldGen、Player、NPC 和网络包路径中被查询或操作。`TETrainingDummy.Activate` 为空；
晶塔的 `OnPlaced`、`OnRemoved`、`NetPlaceEntityAttempt` 为空；逻辑传感器的放置回调也为空。
这些缺口分别影响压力/伤害统计、传感器触发、晶塔可用性与网络同步，不能只按装饰实体处理。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `LogicSensorStateComponent` | 感应类型、冷却、激活和输出坐标 | `TELogicSensor` 字段/查询；`partial` |
| `LogicSensorTriggerSystem` | 时间/玩家/液体条件检测并触发 Wiring | 网络/WorldGen 调用，回调空；`partial/missing` |
| `TrainingDummyStateComponent` | 受击窗口、累计伤害和 DPS 统计 | NPC 查询训练假人；`partial` |
| `TrainingDummyActivationCommand` | 激活、清零统计和广播 | `Activate` 空体；`missing` |
| `PylonAvailabilityQuery` | NPC 房间、距离、晶塔数量和事件限制 | 晶塔查询有部分实现；`partial` |
| `PylonLifecycleCommand` | 放置/移除后注册、失效和网络同步 | 回调空体；`missing` |

最小 verifier：TileEntity ID/位置冲突、网络放置/移除、世界加载校验、容器物品往返、展示装备
属性、逻辑传感器触发、训练假人统计、晶塔放置/失效与多客户端同步。当前 TileEntity 交互核心
没有运行 verifier，标记为“未验证”。

本轮新增约 **18 个**边界（TileEntity 注册/生命周期 6、容器交互 6、传感器/训练假人/晶塔 6）。
累计独立边界约 **888 个**；未闭合重点包括 TileEntity 交互和更新、Bestiary/Creative/成就进度、
连接与 Tile 重同步、液体与光照跨域、实体异常策略、投射物 AI、WorldItem 环境生命周期、地图与
玩家持久化、暂停/退出、世界删除、种子解析、云本地迁移、快照恢复、世界哈希、生成执行控制、
地牢/轨道/生态生成和事件提交。静态识别覆盖率约 **98%～99%**，权威行为闭合度约 **12%～21%**。
本轮仍未重新展开特殊刷怪分支。

## 123. 第九十九轮：Wiring 线路传播与机制触发

`Wiring.HitSwitch`、`TripWire`、`HitWire`、`PokeLogicGate`、`Actuate` 和 `MassWireOperation`
由碰撞、网络包、玩家工具和 WorldGen 真实调用；`Wiring.UpdateMech` 还被世界更新调用并维护
机械计时器。线路队列、跳过标记和多色 wire 外壳存在，但 `CheckMech` 返回默认值，`XferWater`
为空，导致泵转移和机械触发资格未闭合。`PokeLogicGate` 可达但逻辑灯检查方法 `CheckLogicGate`
为空，逻辑门状态不应视为已计算。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `WireNetworkComponent` | 四色线路、跳过集合、传播队列和运行标志 | `Wiring` 静态字段/`TripWire`；`confirmed/partial` |
| `WirePropagationSystem` | BFS 传播、方向、去重和触发顺序 | `HitWire` 主链；`partial` |
| `MechanismQualificationQuery` | 机械计时、Tile 类型和世界边界资格 | `CheckMech` 默认返回；`missing` |
| `PumpTransferCommand` | 水泵输入/输出配对、液体移动和网络广播 | `XferWater` 空体；`missing` |
| `LogicGateSystem` | 灯状态收集、逻辑门计算和输出 wire | `CheckLogicGate` 空体；`missing` |
| `WireNetworkProjection` | Tile 帧、液体、实体和声音同步 | 部分 `NetMessage`/Sound 调用；`partial` |

## 124. 第一百轮：机关副作用与实体生成交接

线路触发器会进入灯、火把、营火、传送、地雷、喷泉、发射器、提取器和 Hopper 等副作用；
其中 `Extractinator`、`Hopper`、`GeyserTrap`、`DeActive`、`ReActive` 为空，且
`MassWireOperationInner` 返回默认值。它们的调用来自 `HitWire`、玩家工具和网络包，属于
权威 Tile/Item/Projectile 状态变更而非纯表现。缺失会影响物品转化、机关激活、执行器状态和
由线路生成的实体来源归因。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `MechanismStateComponent` | 激活、帧、冷却、执行器状态 | `UpdateMech`/Tile 帧写入；`partial` |
| `ExtractinatorCommand` | 输入物品、随机产物、消耗和掉落来源 | `Extractinator` 空体；`missing` |
| `HopperTransferSystem` | 范围检测、物品转移和防重复 | `Hopper` 空体，WorldItem 有调用；`missing` |
| `TrapActivationCommand` | Geyser/地雷等触发、伤害和网络 | `GeyserTrap` 空体，`ExplodeMine` 有对照；`partial/missing` |
| `ActuatorStateCommand` | DeActive/ReActive Tile 可见性与碰撞 | 两方法空体；`missing` |
| `WireToolTransaction` | 玩家 wire/actuator 消耗、权限和回滚 | `MassWireOperation` 有扣除广播，Inner 空；`partial/missing` |

## 125. 第一百零一轮：压力板与线路系统的玩家位置交接

`PressurePlateHelper.Update` 在世界更新中执行，Player 的多条移动/交互路径调用
`UpdatePlayerPosition`，并在玩家重置时调用 `ResetPlayer`；WorldGen 删除世界时调用 `Reset`。
但压力板状态同时写入 `PressurePlatesPressed` 和每玩家布尔数组，线路触发又依赖 `Wiring`
的全局当前用户，缺少单帧快照和原子提交。玩家移动、多人同时踩压、离开区域与断线重置可能
产生重复触发或遗留激活状态。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `PressurePlateOccupancyComponent` | 玩家-压力板占用、每玩家触发位 | `PressurePlatesPressed`/`UpdatePlayerPosition`；`partial` |
| `PressurePlateQualificationQuery` | 玩家 Hitbox、Tile 类型、冷却和权限 | Player/Helper 调用；`partial` |
| `PressurePlateTriggerCommand` | 边沿触发、Wiring 用户归因和广播 | `Update`→Wiring seam；`partial/missing` |
| `PressurePlateResetSystem` | 玩家重置、世界清理、断线和区域卸载 | `Reset/ResetPlayer` 调用；`partial` |
| `PressurePlateConsistencyPolicy` | 多人同时触发、重复包和失败回滚 | 无统一策略；`missing` |

最小 verifier：四种 wire 传播顺序、逻辑门真值、泵液体转移、提取器/Hopper 物品事务、执行器
碰撞变化、压力板多人边沿触发、断线重置、机关副作用网络同步和实体来源归因。当前 Wiring 与
压力板核心无运行 verifier，标记为“未验证”。

本轮新增约 **18 个**边界（线路传播 6、机关副作用 6、压力板交接 6）。累计独立边界约
**906 个**；未闭合重点包括 Wiring 传播/逻辑门/泵、机关副作用、压力板多人一致性、TileEntity
交互、Bestiary/Creative/成就进度、网络重同步、液体与光照跨域、实体异常策略、投射物 AI、
WorldItem 环境生命周期、地图与玩家持久化、暂停/退出、世界删除、种子解析、云本地迁移、快照
恢复、世界哈希、生成执行控制、地牢/轨道/生态生成和事件提交。静态识别覆盖率约 **98%～99%**，
权威行为闭合度约 **12%～21%**。本轮仍未重新展开特殊刷怪分支。

## 126. 第一百零二轮：钓鱼浮标、资格计算与渔获提交

玩家装备鱼竿和鱼饵后，`Player` 物品检查路径会更新浮标并进入 Projectile 类型 61 的 `AI_061_FishingBobber`（`Terraria/Player.cs:8048,8153,24677,25180`、`Projectile.cs:25365`）。但 `AI_061_FishingBobber` 与 `AI_061_FishingBobber_GiveItemToPlayer` 均为空（`Projectile.cs:34073-34074`），后者在 Projectile 渔获分支中被真实调用（约 `:47784`）。因此咬钩、等待时间、液体/环境资格、鱼饵消耗、FishDropRule 选择、物品生成、任务进度和网络同步没有可证明的权威提交。

`Main` 初始化 `FishDropRuleList` 并通过 `GameContentFishDropPopulator.Populate` 建立规则库（`Main.cs:3361-3363`），说明掉落规则是独立的 Query/数据边界；规则表初始化不等于已经完成渔获选择和发放。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `FishingRodStateComponent` | 鱼竿、鱼饵、浮标 Projectile 和蓄力状态 | Player 物品检查/Projectile owner；`partial` |
| `FishingQualificationQuery` | 水体、深度、液体类型、天气、时间和玩家技能 | `FishingContext`/`PlayerFishingConditions`；`partial` |
| `BobberStateSystem` | 浮标位置、漂浮、咬钩窗口和拉线状态 | `AI_061_FishingBobber` 空体；`missing` |
| `FishDropSelectionQuery` | `FishDropRuleList`、稀有度、任务鱼和环境规则 | 规则库初始化存在，实际选择未接通；`partial/missing` |
| `FishCatchCommand` | 消耗鱼饵、创建物品、来源归因和任务进度 | `GiveItemToPlayer` 空体；`missing` |
| `FishingNetworkProjection` | 浮标、咬钩、渔获和声音/粒子同步 | 网络字段存在，提交链未闭合；`partial/missing` |

最小 verifier：不同液体和深度资格、鱼饵消耗与失败不消耗、咬钩/拉线边界、任务鱼优先级、规则稀有度、多人归因、物品堆叠、渔获重复广播和断线恢复。当前浮标 AI 与渔获提交没有运行 verifier，标记为“未验证”。

本轮新增约 **6 个**边界。累计独立边界约 **912 个**；未闭合重点包括钓鱼浮标与渔获提交、Wiring 传播/逻辑门/泵、机关副作用、压力板一致性、TileEntity 交互、Bestiary/Creative/成就进度、网络重同步、液体与光照跨域、实体异常策略、投射物 AI、WorldItem 环境生命周期、地图与玩家持久化、暂停/退出、世界删除、种子解析、云本地迁移、快照恢复、世界哈希、生成执行控制、地牢/轨道/生态生成和事件提交。静态识别覆盖率约 **98%～99%**，权威行为闭合度约 **12%～21%**。本轮仍未重新展开特殊刷怪分支。

## 127. 第一百零三轮：矿车轨道状态、路径碰撞与玩家轨道交互

`Minecart` 的轨道算法在路径、碰撞、压力轨和切换中反复读取 `Tile.FrontTrack()`/`BackTrack()` 并写回对应 setter（`Terraria/Minecart.cs:608,644,795-846,972-985,1126-1173,1300-1312`）。但两个 getter 始终返回默认 `short`，两个 setter 为空（约 `:1371-1378`），因此轨道前后分支、压力轨类型、左右加速轨和轨道切换状态无法持久化。玩家的 `TryInteractingWithMinecartTrackInNearbyArea` 也为空（`Player.cs:19677`），而玩家移动路径真实调用矿车 `GetOnTrack`、`TrackCollision`、`HitTrackSwitch`，导致上车、碰撞和切换操作缺少输入到 Tile 提交的完整链。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `TrackTopologyComponent` | 前轨/后轨 ID、压力轨和加速轨类型 | `FrontTrack/BackTrack` getter/setter；`missing` |
| `TrackPathQuery` | 轨道高度、方向、分叉和连续路径 | 多处读取轨道 ID；`partial/missing` |
| `MinecartCollisionSystem` | 轨道碰撞、速度、转向和脱轨 | `TrackCollision`/`TrackRotation`；`partial` |
| `TrackSwitchCommand` | 压力轨/手动切换、Tile 帧和网络广播 | `HitTrackSwitch` 可达，底层状态空；`partial/missing` |
| `MinecartMountComponent` | 玩家挂载、矿车样式、旋转和能力 | Player/mount 字段与移动分支；`partial` |
| `MinecartInteractionCommand` | 附近轨道交互、上车/下车和工具意图 | `TryInteractingWithMinecartTrackInNearbyArea` 空体；`missing` |

最小 verifier：轨道前后分支往返、压力轨/加速轨切换、轨道断裂、碰撞和脱轨、多人同时切换、玩家上车/下车、轨道 Tile 网络同步和存档恢复。当前轨道状态 getter/setter 与玩家交互没有运行 verifier，标记为“未验证”。

本轮新增约 **6 个**边界。累计独立边界约 **918 个**；未闭合重点包括矿车轨道状态与玩家交互、钓鱼浮标、Wiring 传播/逻辑门/泵、机关副作用、压力板一致性、TileEntity 交互、Bestiary/Creative/成就进度、网络重同步、液体与光照跨域、实体异常策略、投射物 AI、WorldItem 环境生命周期、地图与玩家持久化、暂停/退出、世界删除、种子解析、云本地迁移、快照恢复、世界哈希、生成执行控制、地牢/生态生成和事件提交。静态识别覆盖率约 **98%～99%**，权威行为闭合度约 **12%～21%**。本轮仍未重新展开特殊刷怪分支。

## 128. 第一百零四轮：玩家物品拾取、Void Vault 与 Tile 交互

`Player.GrabItems` 每帧扫描 400 个 `WorldItem`，根据所有权、范围、背包容量、Shimmer 状态和
Creative 权限选择直接拾取、磁力拉取或 Void Vault 路径（`Terraria/Player.cs:19833-19920`）。
但 `PickupItem` 与 `PullItem_ToVoidVault` 均为空；同一文件的 `TileInteractionsCheckLongDistance`、
`TileInteractionsUse`、`TileInteractionsMouseOver` 也为空，而 `Player.Update`/输入路径真实调用
它们（约 `:19696-19759`）。因此物品堆叠、钱币/磁力特殊处理、虚空保险库转移、Tile 使用、
范围验证、声音和网络同步无法证明形成权威提交。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `ItemPickupQualificationQuery` | 范围、所有权、容量、禁拾取和权限 | `GrabItems`/`CanAcceptItemIntoInventory`；`partial` |
| `ItemPickupCommand` | 堆叠、前缀、消耗 WorldItem 和广播 | `PickupItem` 空体；`missing` |
| `VoidVaultTransferCommand` | 虚空保险库转移、失败回滚和网络 | `PullItem_ToVoidVault` 空体；`missing` |
| `TileInteractionQualificationQuery` | 距离、Tile 类型、方向和冷却 | 空的长距离/使用方法；`missing` |
| `TileInteractionCommand` | 开关、容器、机关和物品使用的权威变更 | `TileInteractionsUse` 空体；`missing` |
| `TileInteractionProjection` | 鼠标悬停、提示、声音和网络结果 | `MouseOver` 空体；`partial/missing` |

最小 verifier：普通/磁力/Void Vault 拾取、堆叠溢出、多人所有权、禁拾取、失败回滚、长距离
Tile 使用、交互冷却、容器/机关网络广播和断线恢复。当前拾取与 Tile 交互核心无运行 verifier，
标记为“未验证”。

本轮新增约 **6 个**边界。累计独立边界约 **924 个**；未闭合重点包括玩家拾取与 Tile 交互、
矿车轨道、钓鱼浮标、Wiring、机关副作用、压力板、TileEntity、Bestiary/Creative/成就进度、
网络重同步、液体与光照跨域、实体异常策略、投射物 AI、WorldItem 环境生命周期、地图与玩家
持久化、暂停/退出、世界删除、种子解析、云本地迁移、快照恢复、世界哈希、生成执行控制、地牢/
生态生成和事件提交。静态识别覆盖率约 **98%～99%**，权威行为闭合度约 **12%～21%**。本轮
仍未重新展开特殊刷怪分支。

## 141. 第一百一十七轮：玩家移动能力刷新、物品复用冷却与药水病

`Player` 的装备/状态刷新路径会调用 `RefreshMovementAbilities`（`Terraria/Player.cs:13681,13788`），
而物品使用路径在 `ItemCheck` 中调用 `ApplyReuseDelay`（`Player.cs:23468,25649`）。两者都属于
玩家下一帧行为的权威状态：前者应根据装备、坐骑、翅膀、双跳和环境重建移动能力；后者应决定
物品复用时间、动画锁和连续使用限制。当前两个方法均为空，导致属性字段可能保留旧值，且物品
使用没有可证明的冷却提交顺序。

玩家更新还会调用 `AdjustRemainingPotionSickness`（`Player.cs:15305,17844`），此前第 131 节
只覆盖了药水使用和恢复；该方法负责药水病随时间/状态变化的调整，空体使恢复系统的“应用冷却 →
递减 → 允许再次使用”链仍不完整。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `MovementAbilityComponent` | 翅膀、双跳、坐骑、抓钩和环境移动能力 | Player 状态字段；`partial` |
| `MovementAbilityRefreshSystem` | 装备/坐骑/状态变化后的能力重建与失效 | `RefreshMovementAbilities` 空体；`missing` |
| `ItemReuseCooldownComponent` | 物品复用帧数、动画锁和连续使用标志 | `ApplyReuseDelay` 调用；`partial` |
| `ItemReuseCooldownSystem` | 使用后写入冷却、逐帧递减和失败清理 | `ApplyReuseDelay` 空体；`missing` |
| `PotionSicknessComponent` | 药水病计时、来源和免疫条件 | Player 药水字段；`partial` |
| `PotionSicknessTickSystem` | 药水病递减、状态修正和可用性提交 | `AdjustRemainingPotionSickness` 空体；`missing` |
| `PlayerAbilityNetworkProjection` | 移动能力/冷却状态的同步和重连恢复 | Player/NetMessage 调用；`partial/missing` |

推荐方向为 `Equipment/Status changes → MovementAbilityRefreshSystem → MovementAbilityComponent`，
以及 `ItemUseQualificationQuery → ItemReuseCooldownSystem/PotionSicknessTickSystem → ItemUseCommand`。
冷却写入应与物品消耗和效果提交保持同一事务；客户端只发送使用意图，不能指定剩余帧数或绕过
药水病。

最小 focused verifier：装备切换、坐骑/翅膀/双跳能力及时刷新；能力失效时旧状态清除；物品
复用冷却在连续输入、动画取消、死亡和断线重连下保持一致；药水病递减、不同药水共享/独立规则、
恢复失败回滚和多人同步。当前无运行 verifier，标记为“未验证”。

本轮新增约 **7 个**边界。累计独立边界约 **1023 个**；静态识别覆盖率仍约 **98%～99%**，
权威行为闭合度约 **12%～21%**。本轮只分析玩家能力与物品冷却，继续停止特殊刷怪分支。

## 140. 第一百一十六轮：锚定实体堆叠、投射物引用与放置钩子身份

`Main` 初始化 `sittingManager` 与 `sleepingManager` 两个 `AnchoredEntitiesCollection`
（`Terraria/Main.cs:3271-3272`）；睡眠逻辑会调用 `GetNextPlayerStackIndexInCoords` 与
`AddPlayerAndGetItsStackedIndexInCoords`。但集合的核心 `GetEntitiesInCoords` 为空，导致同一床/座位
上的玩家或 NPC 堆叠索引永远无法由代码证明正确；清理和添加操作本身存在，说明这是跨玩家位置
与睡眠/坐下状态的关系系统，而不是动画缓存（`Terraria.DataStructures/AnchoredEntitiesCollection.cs:20-78`）。

`TrackedProjectileReference` 被玩家的 Piggy Bank 投射物和 Void Lens Chest 投射物字段持有，
可以写入 owner、identity、type 并从网络读回；但 `FindMatchingProjectile` 返回新建的空
`Projectile`，`Equals(object)` 和 `GetHashCode` 为空（`Terraria.DataStructures/TrackedProjectileReference.cs:3-105`）。
这会破坏“网络引用 → 当前活动投射物”的解析、槽位复用安全和集合去重，必须把本地索引、网络
owner、identity、type 分开建模并以实际实体查询完成绑定。

`PlacementHook` 同样定义了 Tile 放置回调、失败返回值、失败响应和坐标是否已处理；其 `==`/`!=`
已有字段比较，但 `Equals(object)`、`GetHashCode()` 为空（`Terraria.DataStructures/PlacementHook.cs:3-54`）。
由于 `TileObject` 读取 `AnchorData` 进行放置锚定，Hook 的值语义缺失会影响放置缓存、回调注册表
和网络重复请求判定。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `AnchoredEntityComponent` | 实体索引、Tile 坐标、座位/床锚点 | `AnchoredEntitiesCollection`；`partial` |
| `AnchorStackQuery` | 同坐标实体数量与下一堆叠索引 | `GetEntitiesInCoords` 空体；`missing` |
| `AnchorLifecycleSystem` | 清理、添加、玩家/NPC 复用和离开区域 | Clear/Add 方法；`partial` |
| `TrackedProjectileIdentityComponent` | owner、identity、type、本地索引和跟踪标志 | `TrackedProjectileReference`；`partial` |
| `ProjectileReferenceResolveQuery` | 从网络三元组查找活动投射物 | `FindMatchingProjectile` 空体；`missing` |
| `ProjectileReferenceNetworkAdapter` | 引用序列化、反序列化、槽位复用校验 | `Write`/`TryReading`；`partial/missing` |
| `PlacementHookComponent` | 放置回调与失败响应元数据 | `PlacementHook` 字段；`partial` |
| `PlacementHookEqualityAdapter` | 值相等、哈希和回调注册去重 | `Equals`/`GetHashCode` 空体；`missing` |
| `TilePlacementCommand` | AnchorData、Hook 和 TileObject 原子放置 | `TileObject` 锚点读取；`partial` |

推荐方向为 `AnchorStackQuery → AnchorLifecycleSystem → Sleeping/Sitting systems`；
`ProjectileReferenceResolveQuery → ProjectileReferenceNetworkAdapter → Player-held entity state`；
`TilePlacementQuery → PlacementHookComponent → TilePlacementCommand`。所有集合键应使用与
操作符一致的值语义，不能依赖默认 object identity。

最小 focused verifier：多人同床/同座位堆叠和离开清理；NPC/玩家锚点复用；投射物 owner+identity+type
网络解析、槽位复用和失效引用清除；PlacementHook 的 ==、Equals、HashSet/Dictionary 一致性；
AnchorData 与 Hook 组合放置、失败响应和重复网络请求。当前无运行 verifier，标记为“未验证”。

本轮新增约 **9 个**边界。累计独立边界约 **1016 个**；静态识别覆盖率仍约 **98%～99%**，
权威行为闭合度约 **12%～21%**。本轮只分析锚定实体、投射物身份和 Tile 放置语义，继续停止特殊刷怪分支。

## 139. 第一百一十五轮：游戏难度曲线与 Tile 锚点值语义

`GameDifficultyData` 定义了生命、伤害、敌方投射物、击退、金钱掉落、城镇 NPC 伤害、减益时长
和闪电伤害等曲线；`LinearCurve.Sample` 负责按难度输入进行分段线性采样
（`Terraria.DataStructures/GameDifficultyData.cs:3-92`）。`NPC.ScaleStats`、NPC 伤害属性、
`Player.AddBuff`、`Projectile` 初始化等真实读取这些曲线（`Terraria/NPC.cs:6909-6952,17623-17651`；
`Terraria/Player.cs:3692`；`Terraria/Projectile.cs:270,554,9911`），因此这是跨战斗、掉落和
状态效果的共享 Query，不应复制到各个系统中。当前曲线计算本体基本存在，但 `Key.ToString` 与
`LinearCurve.ToString` 为空，仅影响诊断/投影；需要补充边界验证，尤其是 Journey、Classic、Expert、
Master、Legendary 间的输入外推和难度覆盖优先级。

`AnchorData` 是 Tile 放置/实体锚定的值对象，`==`/`!=` 已按 type、tileCount、checkStart
比较，但 `Equals(object)` 和 `GetHashCode()` 为空（`Terraria.DataStructures/AnchorData.cs:3-50`）。
只要锚点进入 Dictionary/HashSet、网络去重或 TileEntity 放置缓存，就会出现“操作符相等但哈希
不相等”的一致性风险；这属于基础关系/身份边界，不应被归入渲染。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `DifficultyCurveComponent` | 难度级别、曲线 Key 和版本/外推策略 | `GameDifficultyData.LinearCurve`；`confirmed/partial` |
| `DifficultyScalingQuery` | 生命、伤害、击退、金钱、减益和闪电倍率 | `LinearCurve.Sample` 与 NPC/Player/Projectile 调用；`confirmed` |
| `DifficultyOverrideComponent` | 玩家/多人/NPC 单体难度覆盖及优先级 | `NPCSpawnParams.difficultyOverride`、NPC `difficulty`；`partial` |
| `DifficultyProjection` | 调试文本、诊断和难度显示 | `ToString` 空体；`missing`（非权威） |
| `TileAnchorIdentityComponent` | 锚点类型、跨度、起始偏移和稳定值语义 | `AnchorData` 字段与操作符；`partial` |
| `TileAnchorEqualityAdapter` | object equality、哈希集合和网络去重 | `Equals`/`GetHashCode` 空体；`missing` |
| `TilePlacementQualificationQuery` | 锚点与 Tile/实体布局匹配 | TileEntity/放置调用方；`partial` |

推荐方向为 `DifficultyScalingQuery → NPC/Projectile/Player effect systems`，所有倍率只保留
一个权威曲线来源；难度覆盖按“实例覆盖 > 世界难度 > 默认值”显式解析。锚点则使用稳定的值
对象 Adapter，将 `==`、`Equals`、`GetHashCode` 和网络序列化字段保持同一顺序，避免集合去重
和实体放置缓存分叉。

最小 focused verifier：所有难度级别曲线端点、端点外输入、多人覆盖和实例覆盖优先级；NPC
生命/伤害、投射物伤害、减益时长和金钱掉落跨系统一致；AnchorData 操作符/Equals/HashSet/Dictionary
结果一致；锚点序列化后仍能正确匹配 TileEntity 放置。当前无运行 verifier，标记为“未验证”。

本轮新增约 **7 个**边界。累计独立边界约 **1007 个**；静态识别覆盖率仍约 **98%～99%**，
权威行为闭合度约 **12%～21%**。本轮分析难度缩放与 Tile 身份语义，继续停止特殊刷怪分支。

## 138. 第一百一十四轮：NPC 心情、价格修正与交易资格交接

`ShopHelper.GetShoppingSettings` 是玩家打开 NPC 商店时的真实价格上下文入口：它保存当前
玩家/NPC，调用 `ProcessMood`，再将 `_currentPriceAdjustment` 和 `_currentHappiness` 返回给
`ShoppingSettings`（`Terraria.GameContent/ShopHelper.cs:38-62`；`Player.cs:3389-3395`）。构造函数
还创建并填充 `PersonalityDatabase`，并预置 Corruption、Crimson、Dungeon 等危险生物群系。
但 `ProcessMood(Player,NPC)` 为空，导致 NPC 喜爱/厌恶关系、邻居、环境、危险生物群系以及价格
上下限（常量 0.75～1.50）无法形成权威计算；第 133 节的 `ShopPriceQuery` 不能直接读取
`ShoppingSettings` 结果而绕过此缺口。

该模块应将“心情计算”与“交易提交”分开：心情是纯 Query，可缓存但必须声明玩家位置、NPC
邻居和环境的失效条件；最终价格仍由服务端在购买/出售 Command 中重新计算。`_current*` 字段
是每次调用的临时缓存，不应提升为跨玩家共享的全局权威状态。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `NpcPersonalityContextComponent` | NPC 个性、邻居、玩家关系和生物群系上下文 | `PersonalityDatabase`/`_dangerousBiomes`；`partial` |
| `NpcMoodQuery` | 喜爱/厌恶/憎恨、危险环境与心情文本 | `ProcessMood` 空体；`missing` |
| `NpcPriceAdjustmentQuery` | 价格乘数、上下限和成就阈值 | `ShoppingSettings` 常量与返回值；`partial/missing` |
| `ShopPriceAuthority` | 交易时服务端重算买卖价格并绑定货币类型 | 第 133 节交易命令；依赖心情 Query，`partial` |
| `MoodProjection` | HappinessReport、对话提示和成就展示 | `_currentHappiness` 输出；`missing/partial` |

推荐方向为 `NpcMoodQuery → NpcPriceAdjustmentQuery → ShopPriceAuthority → Purchase/SellCommand`，
其中 Query 只读取上下文，Command 才写入物品和货币；客户端 `MoodProjection` 与价格显示仅供
展示，不能作为请求中的价格或折扣输入。

最小 focused verifier：同一 NPC 在不同邻居/生物群系/玩家关系下得到稳定且有界的乘数；危险
生物群系、距离变化和 NPC 搬家会使缓存失效；多人同时交易时服务端按各自上下文重算；客户端
篡改价格/心情文本不会改变扣款；心情报告与实际交易结果一致。当前无运行 verifier，标记为“未验证”。

本轮新增约 **4 个**边界。累计独立边界约 **1000 个**；静态识别覆盖率仍约 **98%～99%**，
权威行为闭合度约 **12%～21%**。本轮仅补充交易资格计算，继续停止特殊刷怪分支。

## 137. 第一百一十三轮：条件对话、旗帜进度与传送资格

`ConditionalDialogue.Init` 注册了 Free Cake 条件对话，并由内容初始化路径建立 NPC 对话注册表
（`Terraria.GameContent/ConditionalDialogue.cs:18-78`）。`FreeCakeDialogue.GetChatAndClearCondition`
为空，注册表虽可写入但没有“条件满足 → 消费一次性标志 → 返回文本”的提交闭环。该链与前述
`NPCInteractions` 的服务动作不同：它是 NPC 聊天前的条件投影/一次性世界状态消费，应单独建模。

`BannerSystem.AddNPCKillBy` 会把 NPC 类型映射为旗帜、累计击杀数，在达到
`ItemID.Sets.KillsToBanner` 阈值时增加可领取旗帜并广播聊天消息；数组状态还通过 `Save`/`Load`
持久化（`Terraria.GameContent/BannerSystem.cs:84-150`）。但是网络模块 `NetBannersModule.Deserialize`
为空，客户端无法证明能正确应用完整状态、击杀增量或可领取增量；旗帜状态应区分世界持久化和
客户端投影，不能只依赖广播文本。

`TeleportHelpers.FindClosestTeleportSpotNoSpace` 与 `RequestMagicConchTeleportPosition` 是
玩家物品使用和传送落点选择的权威 Query，但 `TileIsDangerous` 与 `IsInSolidTilesExtended`
为空（`Terraria.GameContent/TeleportHelpers.cs:3-210,206-214`），使危险 Tile 排除、固体扩展
碰撞和失败回退失去证明。`TeleportPylonInfo.Equals` 也返回默认值
（`Terraria.GameContent/TeleportPylonInfo.cs:5-14`），会影响晶塔集合去重、网络同步和资格缓存。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `ConditionalDialogueStateComponent` | NPC 条件标志、一次性消费状态和注册条目 | `_registry`、`NPC.freeCake`；`partial` |
| `ConditionalDialogueQuery` | 条件匹配、优先级和显示资格 | `ConditionsMet`/Free Cake 条件；`partial` |
| `ConditionalDialogueCommand` | 清除一次性条件并提交聊天消费 | `GetChatAndClearCondition` 空体；`missing` |
| `BannerProgressComponent` | 每类旗帜击杀数、可领取数量和新旗帜标志 | `killCount/claimableBanners`；`confirmed/partial` |
| `BannerProgressSystem` | NPC 类型映射、阈值、奖励可领取和重置 | `AddNPCKillBy`/`AddClaimableBanner`；`confirmed/partial` |
| `BannerPersistenceAdapter` | 世界保存、加载和版本兼容校验 | `Save`/`Load`/`ValidateWorld`；`confirmed` |
| `BannerNetworkAdapter` | 全量、击杀增量和可领取增量反序列化 | `Deserialize` 空体；`missing` |
| `TeleportLandingQuery` | 安全落点、危险 Tile、液体/固体和边界 | `FindClosest...`/`RequestMagicConch...`；辅助判断 `missing` |
| `TeleportCommand` | 消耗物品、写入位置/速度、失败回滚和广播 | Player 物品使用调用；`partial/missing` |
| `PylonIdentityComponent` | 晶塔位置、类型和稳定身份比较 | `TeleportPylonInfo`；`Equals` 空体，`missing` |
| `PylonQualificationQuery` | 晶塔去重、距离、NPC/环境资格和传送目标 | 使用方依赖 `Equals` 与传送数据；`partial` |

建议方向为 `ConditionalDialogueQuery → ConditionalDialogueCommand → DialogueProjection`，
`BannerProgressSystem → BannerPersistenceAdapter/BannerNetworkAdapter`，以及
`TeleportLandingQuery → TeleportCommand → PlayerPosition + PylonNetworkAdapter`。所有落点
判断必须保持纯查询；晶塔位置应采用值语义比较但不把 UI 列表作为权威集合。

最小 focused verifier：Free Cake 条件只消费一次且重连后状态一致；旗帜阈值、保存/加载版本、
全量和增量网络包一致；传送落点避开危险/固体/液体、世界边界和失败回退；晶塔位置/类型去重、
资格变化和多人同步。当前无运行 verifier，标记为“未验证”。

本轮新增约 **11 个**边界。累计独立边界约 **996 个**；静态识别覆盖率仍约 **98%～99%**，
权威行为闭合度约 **12%～21%**。本轮只处理条件对话、旗帜进度和传送资格，继续停止特殊刷怪分支。

## 136. 第一百一十二轮：紧急堆叠、快速整理与睡眠状态

物品转移链存在真实调度入口但核心操作缺失。`Item` 在多人背包槽位不足时调用
`EmergencyStacking.EmergencyStackItemsToMakeSpace`，主循环随后调用
`ProcessPendingTransfers`（`Terraria/Item.cs:48822-48833`、`Terraria/Main.cs:12781`）；
然而 `Transfer.CompareTo`、`MemoStackableItems`、`FindBestTransfers`、`DoTransfer` 和所有权
释放方法为空，导致跨 WorldItem 堆叠、优先级、距离排序和网络所有权不能闭合。`WorldItem.Update`
还会依据 `HasPendingTransferInvolving` 清理相关转移（`Terraria/WorldItem.cs:272`），说明这是
具有失败回滚和实体生命周期影响的权威系统，而非 UI 辅助。

`QuickStacking` 的网络入口位于 `MessageBuffer`：服务端读取 `ReadNetInventory` 后调用
`QuickStackToNearbyChests`（`Terraria/MessageBuffer.cs:2540-2542`），并通过
`WriteBlockedChestList` 回传阻塞箱子（`Terraria/NetMessage.cs:1205`）。这三个方法及目的地
匹配链均为空，客户端提交的物品槽位、箱子锁定、智能堆叠和失败恢复没有权威实现。

`PlayerSleepingHelper.UpdateState` 会处理床 Tile、睡眠计时、多人堆叠和主动唤醒，但
`DoesPlayerHaveReasonToActUpInBed` 与 `SetOffsetbyBed` 返回默认值；睡眠状态会影响时间推进、
玩家位置/旋转和网络广播 `NetMessage.SendData(13)`（`Terraria.GameContent/PlayerSleepingHelper.cs:45-190`），
因此不应把它归类为纯动画。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `WorldItemStackTransferComponent` | 源/目标 WorldItem、数量、所有权和待处理标记 | `EmergencyStacking.Transfer`/数组；`partial` |
| `EmergencyStackQualificationQuery` | 可堆叠性、距离、屏幕范围和保留优先级 | `MemoStackableItems`/`FindBestTransfers` 空体；`missing` |
| `EmergencyStackCommand` | 原子转移、空槽回收、所有权释放和回滚 | `DoTransfer`/释放方法空体；`missing` |
| `QuickStackDestinationQuery` | 附近箱子、锁定/阻塞、匹配物品和空槽 | `DestinationHelper`/匹配表；`missing` |
| `QuickStackCommand` | 背包到箱子的批量转移与失败恢复 | `QuickStackToNearbyChests` 空体；`missing` |
| `QuickStackNetworkAdapter` | 库存请求解析、服务器重查和阻塞列表同步 | `ReadNetInventory`/`WriteBlockedChestList` 空体；`missing` |
| `SleepingStateComponent` | 床锚点、堆叠索引、睡眠时长和旋转状态 | `PlayerSleepingHelper` 字段；`partial` |
| `SleepWakeQualificationQuery` | 床有效性、输入、持械、坐骑和外部唤醒原因 | `DoesPlayerHaveReasonToActUpInBed` 空体；`partial/missing` |
| `SleepStateSystem` | 睡眠进入/退出、堆叠位置和时间推进 | `UpdateState`/`StopSleeping`；`partial` |
| `SleepNetworkProjection` | 睡眠/唤醒位置、旋转和广播 | `SendData(13)`；`partial` |

推荐方向为 `...QualificationQuery → ...Command → Inventory/WorldItem/Chest`；网络层只提交
槽位意图，不允许客户端指定最终数量或目标箱子状态。睡眠系统应在玩家输入与床状态查询后写入
权威组件，再由时间推进系统读取 `FullyFallenAsleep`，避免把视觉偏移反向当成时间或位置来源。

最小 focused verifier：紧急堆叠优先级、距离和多人所有权；堆叠失败/源物品删除/断线回滚；
Quick Stack 箱子锁定、阻塞列表、重复请求和库存满；床破坏、输入唤醒、武器/坐骑唤醒、多人
同床堆叠、睡眠时间推进和网络重连。当前无运行 verifier，标记为“未验证”。

本轮新增约 **10 个**边界。累计独立边界约 **985 个**；静态识别覆盖率仍约 **98%～99%**，
权威行为闭合度约 **12%～21%**。本轮仅分析物品转移和睡眠状态，继续停止特殊刷怪分支。

## 135. 第一百一十一轮：传送门布置、穿越碰撞与区段同步

`PortalHelper.UpdatePortalPoints` 每帧扫描 1000 个投射物，维护每名玩家的成对传送门索引和
玩家/NPC 冷却；`TryGoingThroughPortals` 被 `Player`、`NPC` 更新路径调用，并负责碰撞线检测、
出口空间验证、速度重定向、Teleport 提交及 NPC 网络广播（`Terraria.GameContent/PortalHelper.cs:55-190`，
`Terraria/NPC.cs:79466`，`Terraria/Player.cs` 更新路径）。但门户放置链中的 `FindValidLine`、
`FindCollision`、`AddPortal`、出口几何 `GetPortalEdges`/`GetPortalOutingPoint` 和支撑检查
`SupportedSlope`/`SupportedHalfbrick`/`SupportedNormal` 全部返回默认值（约 `PortalHelper.cs:245-460`）。
因此传送门生成、合法 Tile 支撑、穿越位置与碰撞安全不能视为闭合。

加入玩家时 `SyncPortalsOnPlayerJoin` 会计算门户所在地图区段并调用 `RemoteClient.CheckSection`，
`SyncPortalSections` 也会广播区段检查（`PortalHelper.cs:350-390`，`MessageBuffer.cs:524`）；这
属于网络 Adapter，不应把绘制颜色 `GetPortalColor` 当成门户状态实现。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `PortalPairComponent` | 施法者、入口/出口投射物 ID、角度、方向和生命周期 | `FoundPortals`/Projectile ai；`partial` |
| `PortalPlacementQuery` | 碰撞线、固体/斜坡、支撑 Tile 和可放置位置 | `FindCollision`/`FindValidLine`/支撑方法空体；`missing` |
| `PortalPlacementCommand` | 创建/替换门户、限制每人数量和同步区段 | `AddPortal` 空体；`missing` |
| `PortalTraversalSystem` | AABB 穿越、出口空间、速度/重力和冷却 | `TryGoingThroughPortals` 主流程；几何依赖缺失，`partial` |
| `PortalCooldownComponent` | 玩家/NPC 穿越冷却及 NPC 槽位复用 | 数组与 `ResetNPCSlotData`；`partial` |
| `PortalNetworkAdapter` | 玩家加入、门户区段检查、NPC 位置/速度广播 | `SyncPortalsOnPlayerJoin`/`SyncPortalSections`；`partial` |
| `PortalProjection` | 门户颜色、粒子和可视反馈 | `GetPortalColor`/绘制调用；`confirmed`（表现层） |

建议调用方向为 `PortalPlacementQuery → PortalPlacementCommand → PortalPairComponent →
PortalTraversalSystem → TeleportCommand → PortalNetworkAdapter`；碰撞查询必须是纯 Query，
Teleport 与冷却写入只能由 System/Command 完成。门户索引、投射物网络 ID 与玩家持久化 ID 要
分离，避免数组槽位复用造成错误冷却或错误广播。

最小 focused verifier：水平/垂直/斜坡/半砖放置、支撑 Tile 破坏后的门户失效、门户替换与每人
两门上限、玩家/NPC 穿越方向和重力、出口被阻挡、连续穿越冷却、多人加入时区段同步，以及 NPC
Teleport 位置广播。当前无运行 verifier，标记为“未验证”。

本轮新增约 **7 个**边界。累计独立边界约 **975 个**；静态识别覆盖率仍约 **98%～99%**，
权威行为闭合度约 **12%～21%**。本轮只分析传送/碰撞/网络状态，继续停止特殊刷怪分支。

## 134. 第一百一十轮：NPC 对话交互、服务动作与任务提交

`NPCInteractions.Initialize` 在 `Main` 初始化期间被调用（`Terraria/Main.cs:3348`），注册 25 个
商店入口以及税收、护士治疗、旧人诅咒、Dryad 净化、Angler 任务、Guide 反向制作、Tinkerer
重铸、染发、稀有植物兑换和 Tavernkeep 建议等交互（`Terraria.GameContent/NPCInteractions.cs:194-244`）。
这些动作不是单纯对话文本：它们会打开商店、改变玩家生命/货币、提交任务、修改世界净化状态或
打开专用 UI。然而所有 `Condition`、`GetText`、`Interact` 实现均返回默认值/空体，
`TryAddCoins` 也返回零和 false；抽象基类只提供当前玩家与 `talkNPC` 解析
（`Terraria.GameContent/NPCInteraction.cs:5-34`）。因此交互资格、服务费用、任务奖励和网络
确认均未形成可证明的权威状态转换。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `NpcDialogueContextComponent` | 当前交谈 NPC、玩家、对话状态和动作选择 | `LocalPlayer/TalkNPC`；`partial` |
| `NpcInteractionRegistrySystem` | 注册动作、按 NPC 类型/优先级筛选候选 | `NPCInteractions.Initialize`；注册 `confirmed`，筛选 `missing` |
| `InteractionQualificationQuery` | 距离、NPC 状态、事件/任务/权限和冷却条件 | 各动作 `Condition` 空体；`missing` |
| `DialogueProjection` | 文本、感叹号、选项和专用 UI 请求 | `GetText`/`ShowExcalmation`；`missing/partial` |
| `NpcServiceCommand` | 商店、治疗、重铸、染发、净化等服务的原子提交 | 各 `Interact` 空体；`missing` |
| `QuestSubmissionCommand` | Angler 任务/奖励、Guide 提示和 Tavernkeep 进度 | `AnglerQuest.Interact`/条件空体；`missing` |
| `TaxCollectionCommand` | 税收余额读取、扣除/转入 NPC、失败回滚 | `TaxCollectorCollectTaxes.TryAddCoins` 空体；`missing` |
| `NursePaymentCommand` | 治疗量、价格、货币扣除与生命提交 | `NurseHeal.TryAddCoins`/`Interact` 空体；`missing` |
| `NpcInteractionNetworkAdapter` | 客户端意图、服务器重查、结果广播和防重放 | 当前未见与动作提交绑定的协议；`missing` |

调用方向应为 `DialogueContext → InteractionQualificationQuery → NpcServiceCommand /
QuestSubmissionCommand → Player/World/Currency`，对话和 UI 只能消费 `DialogueProjection`。
商店动作应转入第 133 节的 `NpcShopCatalogSystem`，但不能直接把客户端选中的 shop index
当作权威商品；护士、税收和兑换动作必须在服务器重新计算费用与余额后一次提交。NPC 类型、
持久化任务状态、网络玩家 ID 和 UI 会话 ID 也应分开建模，避免把 `talkNPC` 数组索引当作稳定实体 ID。

最小 focused verifier：每个 NPC 类型仅出现符合条件的动作；商店入口与库存/价格系统一致；护士
治疗不足余额、满血和并发请求均无部分扣款；税收领取只转移一次；Angler 任务完成、奖励和重复
提交幂等；Dryad 净化、Tinkerer 重铸、Dye Trader 兑换的世界/物品变更可回滚；断线与重连后
对话会话、服务结果和玩家状态一致。当前无运行 verifier，标记为“未验证”。

本轮新增约 **9 个**边界。累计独立边界约 **968 个**；静态识别覆盖率仍约 **98%～99%**，
权威行为闭合度约 **12%～21%**。本轮继续避开特殊刷怪资格和生成分支，仅补充 NPC 对话/服务/任务
提交链；此前经济交易、制作、拾取、Tile 交互、钓鱼、矿车、Wiring、TileEntity、玩家状态、
持久化和世界生成等缺口仍保持原状态。

## 133. 第一百零九轮：NPC 商店库存、CustomCurrency 与交易事务

商店初始化在 `Main` 中确实建立了商店容器：`CustomCurrencyManager.Initialize` 注册
Defender Medals（`Terraria/Main.cs:3336`、`Terraria.GameContent.UI/CustomCurrencyManager.cs:16-20`），
随后创建 `shop[0]`、生成旅行商店并逐个调用 `Chest.SetupShop(m)`（`Terraria/Main.cs:3426-3431`）。
但 `Chest.SetupShop(int type)` 是空体（`Terraria/Chest.cs:1222`），所以普通 NPC 商店的类型到库存
映射、限购槽位和商品初始化没有证据闭合；旅行商店是例外，`SetupTravelShop` 会清空并填充
`Main.travelShop`，且通过 `SetupTravelShop_GetItem`、稀有度调整和去重逻辑生成商品
（`Terraria/Chest.cs:1135-1217`）。这两个库存来源必须拆成不同的生成 System，不能把旅行商店
当作普通商店的实现。

CustomCurrency 的公共抽象暴露了完整交易所需的接口，但权威实现缺失：基类的
`CountCurrency`、`CombineStacks`、`TryPurchasing`、`Accepts`、`GetItemExpectedPrice` 都返回
默认值（`Terraria.GameContent.UI/CustomCurrencySystem.cs:28-52`），单币种实现只在构造函数中
写入币种物品 ID/单位价值/上限，重写的 `TryPurchasing`、价格文本和绘制方法仍为空
（`CustomCurrencySingleCoin.cs:20-35`）。因此 Defender Medals 的余额读取、溢出处理、扣除顺序、
购买失败回滚和价格计算不能由现有代码证明。`NetMessage` 只发送旅行商店槽位 ID
（`Terraria/NetMessage.cs:1131-1133,2391-2398`），未发现与购买扣款原子绑定的确认链。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `NpcShopInventoryComponent` | 商店类型、槽位商品、限购/已售标志和来源 NPC | `Main.shop[]`、`Chest.SetupShop`；普通库存 `missing`，旅行库存 `partial` |
| `NpcShopCatalogSystem` | NPC 类型到商品集合、条件过滤、刷新和商品实例化 | `SetupShop` 空体；`missing` |
| `TravelShopGenerationSystem` | 幸运值、稀有度、去重和旅行商店槽位生成 | `SetupTravelShop*` 调用链；`confirmed/partial` |
| `CurrencyLedgerComponent` | 货币物品、单位价值、上限和跨背包余额 | `CustomCurrencySystem._valuePerUnit/_currencyCap`；`partial` |
| `CurrencyBalanceQuery` | 统计背包/银行货币、溢出和忽略槽位 | `CountCurrency`/`CombineStacks` 空体；`missing` |
| `ShopPriceQuery` | 买入/卖出价、NPC 价格修正、CustomCurrency 选择 | `Item.SetShopValues`、`ShoppingSettings`；`partial` |
| `PurchaseCommand` | 校验商店槽位、扣币、生成商品、库存扣减和回滚 | `TryPurchasing` 空体且无端到端调用证据；`missing` |
| `SellCommand` | 校验玩家物品、计算卖价、移除物品并发放货币 | `GetItemExpectedPrice` 空体；`missing` |
| `TradeNetworkAdapter` | 请求序列化、服务器权威重算、防重放和确认 | `NetMessage` 仅旅行库存广播；交易确认 `missing` |
| `ShopProjection` | 价格文本、余额显示、槽位图标和提示 | `DrawSavingsMoney`/`GetPriceText` 空体；`partial`（纯显示，不得写回权威状态） |

推荐方向为 `NpcShopCatalogSystem → ShopPriceQuery → PurchaseCommand/SellCommand →
CurrencyLedgerComponent + PlayerInventory`，服务器端 `TradeNetworkAdapter` 只接受商品槽位、
数量和请求标识，重新执行资格查询与价格计算后提交一次事务；`ShopProjection` 订阅提交结果，
不得以客户端显示价格或余额作为权威输入。旅行商店生成应在世界/日期刷新边界运行，普通商店
初始化则必须补齐 `SetupShop` 的类型映射，否则所有依赖 NPC 商店的任务、事件奖励和经济进度
均会落入空库存。

最小 focused verifier：普通 NPC 类型初始化后槽位稳定且条件商品正确；旅行商店刷新、去重和
多人同步；Defender Medals 背包/银行合并、上限和溢出；金币与自定义货币混合购买；价格修正
变化时服务器重算；购买材料不足、库存满、槽位失效和重复请求均不产生部分提交；出售失败
回滚；断线重连后商店库存、玩家物品与余额一致。当前没有运行 verifier，标记为“未验证”。

本轮新增约 **10 个**边界。累计独立边界约 **959 个**；静态识别覆盖率仍约 **98%～99%**，
权威行为闭合度约 **12%～21%**。本轮只补充经济/交易方向，**遵守用户指示，停止查找特殊刷怪分支**；
此前已发现但未闭合的制作、拾取、Tile 交互、钓鱼、矿车、Wiring、TileEntity、玩家状态、网络
重同步、液体/光照、持久化和世界生成边界仍需后续分别收口。

## 130. 第一百零六轮：Buff 生命周期、Nebula 层级与条件副作用

`Player.UpdateBuffs` 是玩家每帧状态变换的一部分，真实调用 `UpdateBuffs_NebulaBuffs` 处理 Nebula 增益层级；该方法为空（`Terraria/Player.cs:4300-4843,6227`）。Buff 的添加/删除、装备刷新和网络同步均会到达此路径，因此 Nebula 生命/魔力/伤害层级及对应计数不能仅以 `AddBuff`/`DelBuff` 存在判定为完整。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `PlayerBuffComponent` | Buff 类型、剩余时间、来源和免疫 | `buffType/buffTime` 与 Update；`confirmed/partial` |
| `BuffLifecycleSystem` | 添加、刷新、过期、删除和冲突 | `UpdateBuffs`/`DelBuff`；`partial` |
| `NebulaStackQuery` | 生命/魔力/伤害层级和阈值 | `UpdateBuffs_NebulaBuffs` 空体；`missing` |
| `BuffStatCommitSystem` | 将层级写入玩家属性并清理计数 | 调用点存在，核心方法空；`partial/missing` |
| `BuffNetworkProjection` | Buff 增删和状态同步 | MessageBuffer 有包处理，结果未闭合；`partial` |

## 131. 第一百零七轮：药水使用、冷却与生命/魔力提交

物品使用链在 `ItemCheck` 中真实调用 `ApplyPotionDelay` 和 `ApplyLifeAndOrMana`（`Player.cs:23516,23583,25163-25164`），而生命再生扣血链调用空 `HurtLifeRegen`（约 `:11238`）。这些方法位于药水消费、药水病、恢复量、受伤再生和网络同步的权威路径；空体会使物品消耗与属性变更缺少可证明的原子顺序。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `PotionCooldownComponent` | 生命/魔力药水冷却和药水病 | `ApplyPotionDelay` 空体；`missing` |
| `ConsumableEffectQuery` | 恢复量、增益、过量和禁用条件 | `ItemCheck`/物品字段；`partial` |
| `PotionEffectCommand` | 消耗物品、恢复属性、添加 Buff 和广播 | `ApplyLifeAndOrMana` 空体；`missing` |
| `LifeRegenDamageSystem` | 中毒/饥饿/窒息等再生伤害提交 | `HurtLifeRegen` 空体；`missing` |
| `PotionNetworkProjection` | 消费、属性和 Buff 网络结果 | 调用链存在但未验证；`partial/missing` |

## 132. 第一百零八轮：玩家出生点、重生定位与队伍出生交接

玩家生成和重生路径会读取队伍出生、世界出生点、床和事件上下文；`Spawn_SetPositionAtTeamSpawn` 为空（`Player.cs:22083`），而 `MessageBuffer`、WorldGen 和玩家死亡/加入流程真实触达 Spawn 选择。该缺口影响多人队伍出生点、重生安全区域、碰撞验证、网络位置同步和重生冷却，不能被视为绘制问题。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `PlayerSpawnStateComponent` | 出生点、床、队伍和重生计时 | Player 字段/Spawn context；`partial` |
| `SpawnLocationQuery` | Tile 安全、队伍点、床点和事件限制 | Spawn 调用链；`partial` |
| `TeamSpawnPlacementCommand` | 队伍出生坐标、碰撞修正和提交 | `Spawn_SetPositionAtTeamSpawn` 空体；`missing` |
| `RespawnNetworkProjection` | 重生位置、死亡状态和同步确认 | MessageBuffer/NetMessage 有部分链；`partial/missing` |
| `SpawnFailurePolicy` | 无安全点、越界、重复重生和回退 | 无统一策略；`missing` |

最小 verifier：Buff 过期/冲突、Nebula 层级、药水冷却与恢复、再生伤害、多人队伍出生点、床点
回退、危险 Tile 排除、重生网络同步和失败重试。当前上述玩家状态核心无运行 verifier，标记
为“未验证”。

本轮新增约 **18 个**边界（Buff 5、药水/再生 5、出生/重生 5，另含网络与失败 seam 3）。累计
独立边界约 **949 个**；未闭合重点包括玩家状态变换、配方与制作、物品拾取、矿车轨道、钓鱼浮标、
Wiring、机关副作用、压力板、TileEntity、Bestiary/Creative/成就进度、网络重同步、液体与光照跨域、
实体异常策略、投射物 AI、WorldItem 环境生命周期、地图与玩家持久化、暂停/退出、世界删除、种子
解析、云本地迁移、快照恢复、世界哈希、生成执行控制、地牢/生态生成和事件提交。静态识别覆盖率约
**98%～99%**，权威行为闭合度约 **12%～21%**。本轮仍未重新展开特殊刷怪分支。

## 129. 第一百零五轮：配方数据库、制作资格与远程制作请求

`Recipe` 持有产物、必需物品、RecipeGroup、必需 Tile、液体/生物群系条件和炼金标志等字段；
`RecipeGroup` 维护可接受物品集合。这些类型是制作系统的核心数据边界，但 `Recipe.ToString`
和 `RecipeGroup.ToString` 返回默认值，且 `Main` 初始化阶段将 `Recipe.SetupRecipeGroups`、
配方数组创建和 `Recipe.SetupRecipes` 全部注释掉（`Terraria/Main.cs:3401-3414`）。因此
配方数据库、可制作列表和条件索引没有实际建立。

联网制作模块 `CraftingRequests.NetCraftingRequestsModule` 已在
`NetworkInitializer.Load` 注册（`Terraria.Initializers/NetworkInitializer.cs:25`），但其
`Deserialize` 返回默认 `false`，`_pendingCrafts` 只有计数属性，没有消费/校验/提交路径。
玩家的 `ConsumeItem` 虽存在，不能替代“配方资格 → 原子消耗 → 产物生成 → 网络确认”的制作
事务。

| 边界 | 责任 | 证据与状态 |
| --- | --- | --- |
| `RecipeDefinitionComponent` | 产物、材料、RecipeGroup 和环境条件 | `Recipe` 字段存在；`partial` |
| `RecipeCatalogSystem` | 注册配方组、建立数组和快速索引 | Main Setup 调用被注释；`missing` |
| `CraftingQualificationQuery` | 背包/箱子、Tile、液体、生物群系和玩家权限 | 字段存在但无查找器；`missing` |
| `CraftingConsumptionCommand` | 材料匹配、堆叠扣除、失败回滚 | `Player.ConsumeItem` 可用但无 Recipe 事务；`partial/missing` |
| `CraftingResultCommand` | 产物前缀、数量、来源和溢出处理 | Recipe/制作提交缺失；`missing` |
| `RemoteCraftRequestComponent` | 远程请求、结果、已消耗材料和 quickCraft 标志 | `CraftingRequests` 结构体；`partial` |
| `CraftingNetworkAdapter` | 请求反序列化、权限校验、排队、消费和确认 | `Deserialize` 默认 false、队列无消费者；`missing` |

最小 verifier：配方目录初始化、RecipeGroup 替代材料、工作台/液体/生物群系资格、背包与箱子
材料匹配、堆叠消耗、失败回滚、产物溢出、远程请求防重放、权限和客户端确认。当前制作系统
没有运行 verifier，标记为“未验证”。

本轮新增约 **7 个**边界。累计独立边界约 **931 个**；未闭合重点包括配方数据库与制作事务、
玩家拾取与 Tile 交互、矿车轨道、钓鱼浮标、Wiring、机关副作用、压力板、TileEntity、Bestiary/
Creative/成就进度、网络重同步、液体与光照跨域、实体异常策略、投射物 AI、WorldItem 环境生命周期、
地图与玩家持久化、暂停/退出、世界删除、种子解析、云本地迁移、快照恢复、世界哈希、生成执行控制、
地牢/生态生成和事件提交。静态识别覆盖率约 **98%～99%**，权威行为闭合度约 **12%～21%**。本轮
仍未重新展开特殊刷怪分支。
