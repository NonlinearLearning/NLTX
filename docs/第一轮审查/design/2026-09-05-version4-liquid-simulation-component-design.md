# LiquidSimulation Component Design

## 1. 设计元数据

```text
subsystemId: LiquidSimulation
taskNumber: 01
sourceReport: D:\TRbackup\NLTX\docs\research\2026-09-05-version4-liquid-simulation-public-decomposition.md
outputDesignPath: D:\TRbackup\NLTX\docs\design\2026-09-05-version4-liquid-simulation-component-design.md
designScope: component-only
designStatus: candidate
evidenceStatus: partial
nltxStatus: evidence-mismatch
verificationStatus: not-run
selectionMethod: 当前会话明确指定的研究报告；不扫描 research 目录，不选择其他报告
```

本文件只定义组件边界、承载范围、字段语义、默认值、生命周期和组合关系。`candidate` 表示组件形状已经收敛到可审查的候选方案，但跨子系统 owner、共享值对象和少数字段的最终归属仍需裁决。

## 2. 设计范围与排除范围

设计对象是可独立承载一组液体状态的组件。每个组件必须回答四个问题：它附着在哪种实体或世界范围上、它拥有哪类状态、字段的权威来源是什么、缺失时代表什么。组件不因当前旧代码中的数组、全局字段或文件名而自动获得 owner。

纳入范围：

- TileCell 范围的液体数量与类型权威状态。
- TileCell 或世界工作表范围的处理关联状态。
- World singleton 范围的容量、预算、计数、稳定性和保护状态。
- Player、NPC、Projectile 等实体范围的液体接触派生状态。
- WorldSection 范围的变更发布标记。
- 为恢复稳定顺序而提出的世界级序列状态；该项仍是候选项，不冒充 Version4 原生字段。
- 组件之间的组合约束、字段不变量、默认值和持久化生命周期。

排除范围：

- 不定义任何执行单元、读取筛选、写入命令、事件载荷、适配层或表现投影。
- 不定义调度顺序、调用图、主循环、网络发送流程、存档流程或客户端渲染流程。
- 不把网络快照、渲染快照、溢出数组或临时局部变量定义成组件。
- 不把反应规则本身或一次性反应结果提升为持续组件；现有证据不足以支持该持久状态。
- 不给出测试计划、迁移计划、实现步骤、项目文件变更或可运行代码。
- 不裁决跨子系统共享类型的最终 owner；这些决策保留在第 11 节。

## 3. 组件设计依据

下表只记录本设计实际采用的证据。`strong` 表示字段和生命周期在指定源码中直接可见；`partial` 表示字段存在但语义、owner 或当前 NLTX 覆盖不完整；`proposed` 表示为了表达组件级状态而新增的候选字段。

| 证据编号 | 绝对路径与行号 | 支持的设计事实 | 强度 |
| --- | --- | --- | --- |
| BD-E-01 | `D:\TRbackup\Version4\Terraria\Tile.cs:12`；`D:\TRbackup\Version4\Terraria\Tile.cs:220-241`；`D:\TRbackup\Version4\Terraria\Tile.cs:512-549`；`D:\TRbackup\Version4\Terraria\Tile.cs:849-854` | Tile 液体数量是 `byte`；液体类型通过 `liquidType` 访问；处理标志和清理行为与 Tile 状态同处一组。 | strong |
| BD-E-02 | `D:\TRbackup\Version4\Terraria\Liquid.cs:14-54`；`D:\TRbackup\Version4\Terraria\Liquid.cs:88-109`；`D:\TRbackup\Version4\Terraria\Liquid.cs:470-995`；`D:\TRbackup\Version4\Terraria\Liquid.cs:1015-1188`；`D:\TRbackup\Version4\Terraria\Liquid.cs:1190-1234`；`D:\TRbackup\Version4\Terraria\Liquid.cs:1427-1486` | `Liquid.x/y/kill/delay` 属于工作关联；容量、预算、cycles、quick、stuck、panic 和脏集合属于世界运行态。 | strong / partial |
| BD-E-03 | `D:\TRbackup\Version4\Terraria\LiquidBuffer.cs:3-31`；`D:\TRbackup\Version4\Terraria\Main.cs:569-571` | 缓冲项承载溢出工作集，不拥有 Tile 的液体数量或类型；全局数组只是旧宿主。 | strong |
| BD-E-04 | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:750-781`；`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1469-1550`；`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:2570-2662`；`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:3847-3855` | Tile 液体字段随世界 Tile 保存和读取；工作关联与世界运行态不应默认作为世界持久化数据。 | strong / partial |
| BD-E-05 | `D:\TRbackup\Version4\Terraria\Collision.cs:829-1087`；`D:\TRbackup\Version4\Terraria\Player.cs:14144-14302`；`D:\TRbackup\Version4\Terraria\Projectile.cs:10282-10288`；`D:\TRbackup\Version4\Terraria\NPC.cs:961`；`D:\TRbackup\Version4\Terraria\NPC.cs:67081` | 实体会从 Tile 液体得到接触、湿润和液体类型结果；这些结果不等同于 Tile 液体存储。 | strong |
| BD-E-06 | `D:\TRbackup\Version4\Terraria.GameContent.NetModules\NetLiquidModule.cs:61-63`；`D:\TRbackup\Version4\Terraria\MessageBuffer.cs:1882-1917`；`D:\TRbackup\Version4\Terraria\Collision.cs:2111-2113` | 网络与碰撞侧存在液体快照或读取事实，但它们不能直接成为新的组件 owner。 | partial |
| BD-E-07 | `D:\TRbackup\Version4\Terraria\Liquid.cs:1235-1237`；`D:\TRbackup\Version4\Terraria\Liquid.cs:1238-1320`；`D:\TRbackup\Version4\Terraria\Liquid.cs:1322-1323`；`D:\TRbackup\Version4\Terraria\Liquid.cs:1324-1394`；`D:\TRbackup\Version4\Terraria\Liquid.cs:1400-1425` | Version4 的地下沙漠检查和液体合并入口存在空实现或证据缺口；不能从缺口推导持续反应组件。 | strong |
| BD-E-08 | `D:\TRbackup\NLTX\src\WorldInteraction\Tiles\TileCellComponent.cs:3-10`；`D:\TRbackup\NLTX\src\Share\Entity\Components\LiquidComponent.cs:3-35` | 当前根 src 已有 TileCell 字段和实体接触字段，但二者混合或命名重叠，尚未闭合为本设计的 owner。 | partial |
| BD-E-09 | `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\WorldTile.cs:3-27`；`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\WorldGrid.cs:116-137`；`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\WorldGrid.cs:150-188` | Dome 已有 Tile 和世界网格承载，但 TileCell、WorldTile 与液体字段的最终分层仍不一致。 | partial |
| BD-E-10 | `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Liquid\Components\LiquidContactComponent.cs:5-59`；`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Liquid\Components\LiquidWorldStateComponent.cs:5-90`；`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Liquid\Components\LiquidUpdateQueueComponent.cs:8-146`；`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Liquid\Components\LiquidDirtySectionComponent.cs:6-25`；`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Liquid\Components\LiquidSourceComponent.cs:3-8` | Dome 已存在接触、世界、工作队列、脏区和来源相关类型；其中只有部分字段覆盖 Version4，当前状态不能标成完整基线。 | partial |

完整参考源码 `D:\TRbackup\无任何删减通过编译\Terraria\Liquid.cs:1245-1262`、`D:\TRbackup\无任何删减通过编译\Terraria\Liquid.cs:1369-1420` 只用于识别 Version4 的反应证据缺口，不改变 Version4 当前行为，也不为本组件清单增加反应状态。

## 4. Version4 成员到 Component 归属表

| Version4 成员或状态 | 原始范围 | 候选归属 | 归属结论 | 备注 |
| --- | --- | --- | --- | --- |
| `Tile.liquid` | TileCell | `TileLiquidStateComponent.Amount` | 直接归属 | `byte`，默认 `0`；是液体数量权威值。 |
| `Tile.liquidType(int)` / `Tile.liquidType()` | TileCell | `TileLiquidStateComponent.Type` | 直接归属 | 默认 `0`；具体共享类型 owner 见 BD-COMP-02。 |
| `Tile.checkingLiquid`、`Tile.skipLiquid` | TileCell 工作关联 | `LiquidWorkStateComponent.IsPending`、`SkipOnce` | 语义映射 | 属于处理关联，不得与数量和类型混成同一字段组。 |
| `Liquid.x`、`Liquid.y` | 单个液体工作项 | `LiquidWorkStateComponent.Coordinate` | 直接归属 | 坐标值对象 owner 未决。 |
| `Liquid.kill` | 单个液体工作项 | `LiquidWorkStateComponent.KillScore` | 直接归属 | 表达工作项的清理/失败计分，不表达液体数量。 |
| `Liquid.delay` | 单个液体工作项 | `LiquidWorkStateComponent.Delay` | 直接归属 | 默认值需由后续字段语义裁决。 |
| `Liquid.maxLiquidBuffer` | 世界运行态 | `LiquidWorldRuntimeStateComponent.MaxLiquidBuffer` | 直接归属 | Version4 为 `50000`。 |
| `Liquid.maxLiquid` | 世界运行态 | `LiquidWorldRuntimeStateComponent.MaxLiquid` | 直接归属 | 默认 `25000`；`ReInit()` 会调整运行值。 |
| `Liquid.currentMaxLiquid` | 世界运行态 | `LiquidWorldRuntimeStateComponent.CurrentMaxLiquid` | 直接归属 | 当前预算上限；不是 Tile 字段。 |
| `Liquid.cycles` | 世界运行态 | `LiquidWorldRuntimeStateComponent.Cycles` | 直接归属 | 默认 `10`。 |
| `Liquid.numLiquid、LiquidBuffer.numLiquid` | 世界运行态计数 | `LiquidWorldRuntimeStateComponent.CurrentWorkCount` | 语义归并 | 不保留两个互相竞争的权威计数；缓冲区只贡献工作集来源。 |
| `Liquid.quickFall`、`Liquid.quickSettle` | 世界运行态 | `LiquidWorldRuntimeStateComponent.QuickFall`、`QuickSettle` | 直接归属 | 保护/加速状态，非实体状态。 |
| `Liquid.stuck`、`Liquid.stuckAmount`、`Liquid.isStuck` | 世界运行态 | `LiquidWorldRuntimeStateComponent.StuckCount`、`StuckAmount`、`IsStuckCleanup` | 直接归属 | 字段命名按语义收敛，原始计数和清理标志仍需核对。 |
| `Liquid.panicMode`、`Liquid.panicY`、`Liquid.panicCounter` | 世界运行态 | `LiquidWorldRuntimeStateComponent.PanicMode`、`PanicY`、`PanicCounter` | 直接归属 | 保护状态，不创建独立 panic 组件。 |
| `Liquid.wetCounter` | 世界运行态 | `LiquidWorldRuntimeStateComponent.WetCounter` | 暂定归属 | 需确认它是全局保护计数还是接触状态汇总。 |
| `LiquidBuffer` 坐标及溢出工作项 | 世界工作集 | `LiquidWorkStateComponent` 的缓冲关联字段 | 直接归属 | `LiquidBuffer` 本身不是组件；不得作为第二份液体权威存储。 |
| `liquidBuffer` / 脏坐标集合 | WorldSection 变更集合 | `LiquidDirtySectionStateComponent` | 语义映射 | 现有实现是 World 范围字典，Section 附着方式见 BD-COMP-05。 |
| `Player`、`NPC`、`Projectile` 的湿润或液体类型结果 | 实体 | `LiquidContactStateComponent` | 派生归属 | 不拥有 Tile 的 `Amount` 或 `Type`。 |
| `LiquidRenderer` 相关空实现或表现读取 | 客户端表现 | 无组件归属 | 排除 | 没有足够证据形成稳定的领域状态。 |
| `Liquid.Sequence`（候选新增） | World singleton | `LiquidSequenceStateComponent.NextLiquidSequence` | proposed | 不是 Version4 原生字段；是否保留见 BD-COMP-03。 |

## 5. Component 定义

### 5.1 TileLiquidStateComponent

| 项目 | 定义 |
| --- | --- |
| 附着范围 | TileCell；在世界存储中对应一个可寻址的 WorldTile。 |
| 组件职责 | 拥有该格液体的权威数量和液体类型。 |
| 默认存在 | 每个可寻址 TileCell 都可以有该状态；缺省实例等价于 `Amount = 0`、`Type = 0`。是否采用稀疏存储由最终 owner 决定。 |
| 持久化 | `Amount` 和 `Type` 随世界 Tile 数据保存/读取；处理工作字段不随本组件持久化。 |
| 证据状态 | `strong` 字段证据；`partial` owner 证据。 |

| 字段 | 类型 | 默认值 | 语义与不变量 |
| --- | --- | --- | --- |
| `Amount` | `byte` | `0` | 液体数量；Version4 的 `Tile.liquid` 是直接证据。取值为零表示该格没有可用液体数量。 |
| `Type` | `byte` 或 integration-level `LiquidTypeId` | `0` | 液体种类；类型标识不能替代数量，也不能承载处理标志。共享类型的最终 owner 见 BD-COMP-02。 |

设计约束：

- `TileType`、`IsActive`、`WallType` 不属于本组件；它们是 TileCell 的其他领域状态，不能因为同一个 Tile 载体而混入液体状态。
- 当数量被清空时，候选规范要求同步归一化 `Type = 0`，并由工作关联状态清除待处理标志；该归一化与实际写入边界的 owner 仍需在 BD-COMP-01 中确认。
- 本组件不保存 `Liquid.x/y`、`kill`、`delay`、计数器或缓冲区坐标；这些值改变的是工作关联，而不是格子的权威液体。

### 5.2 LiquidWorkStateComponent

| 项目 | 定义 |
| --- | --- |
| 附着范围 | 一个 TileCell 工作关联，或承载同等关联的 World 工作表条目。它不是世界级液体存储。 |
| 组件职责 | 表达某个坐标当前是否有液体处理关联，以及该关联的暂态工作字段。 |
| 默认存在 | 没有待处理关联时可以缺省；缺省等价于 `IsPending = false`、`IsBuffered = false`，其余计数与延迟归零。 |
| 持久化 | 默认不持久化；世界加载后依据 TileLiquidStateComponent 重建。 |
| 证据状态 | Version4 原始字段 `strong`；`RetryCount`、`Sequence` 和统一坐标类型为 `partial/proposed`。 |

| 字段 | 类型 | 默认值 | 语义与不变量 |
| --- | --- | --- | --- |
| `Coordinate` | `TileCoordinate` | 未设置 | 工作关联指向的 TileCell；`TileCoordinate` 的共享 owner 见 BD-COMP-02。 |
| `IsPending` | `bool` | `false` | 对应 `checkingLiquid` 的工作关联语义；不表示该格有多少液体。 |
| `SkipOnce` | `bool` | `false` | 对应 `skipLiquid` 的一次性跳过标志；只属于工作关联。 |
| `KillScore` | `int` | `0` | 对应 `Liquid.kill` 的工作项计分或清理标记。它不能直接改写 `Amount`。 |
| `Delay` | `int` | `0` | 对应 `Liquid.delay` 的暂态延迟值；单位和上限仍需与既有时间语义对齐。 |
| `Sequence` | `long` | `0` | 候选的工作关联标识，用于区分重复出现的关联；它不是运行顺序定义。是否需要独立序列值见 BD-COMP-03。 |
| `RetryCount` | `int` | `0` | 候选的重试/重复处理计数；Version4 的成员到该字段不是一一对应，必须保持 `proposed`。 |
| `IsBuffered` | `bool` | `false` | 标明关联是否位于溢出工作集；不表示 `LiquidBuffer` 成为权威存储。 |

设计约束：

- 同一个工作关联最多指向一个 `Coordinate`；如果同一坐标同时出现在普通工作表和溢出工作集，仍只有一个权威工作状态。
- `IsPending`、`SkipOnce`、`KillScore`、`Delay` 的生命周期短于 TileLiquidStateComponent；世界加载不会直接恢复它们。
- 该组件不包含任何数量、液体类型或实体湿润结果，避免把三个不同生命周期的状态合并。

### 5.3 LiquidWorldRuntimeStateComponent

| 项目 | 定义 |
| --- | --- |
| 附着范围 | World singleton；每个世界最多一个。 |
| 组件职责 | 保存液体运行期间的容量、消费预算、工作计数、稳定性计数和保护状态。 |
| 默认存在 | 世界创建或载入时存在；没有世界时不存在。 |
| 持久化 | 默认不随世界文件持久化；`ReInit()` 语义对应重新初始化运行态。 |
| 证据状态 | 字段证据 `strong/partial`；当前 NLTX 覆盖为 partial。 |

| 字段 | 类型 | 默认值 | 语义与不变量 |
| --- | --- | --- | --- |
| `MaxLiquidBuffer` | `int` | `50000` | 溢出工作集容量上限；只限制工作关联承载能力。 |
| `MaxLiquid` | `int` | `25000` | Version4 默认的液体处理预算上限。 |
| `CurrentMaxLiquid` | `int` | 由初始化状态确定 | 当前世界实际采用的预算上限；与 `MaxLiquid` 的关系必须保持可解释。 |
| `Cycles` | `int` | `10` | 世界运行态的周期配置值；不在本文件定义其消费顺序。 |
| `CurrentWorkCount` | `int` | `0` | 当前工作关联总数的唯一汇总值；不把普通工作表计数和缓冲计数各自视为权威副本。 |
| `IsStuckCleanup` | `bool` | `false` | 是否处于卡滞清理保护状态。 |
| `QuickFall` | `bool` | `false` | 快速下落保护/加速状态。 |
| `QuickSettle` | `bool` | `false` | 快速稳定保护/加速状态。 |
| `StuckCount` | `int` | `0` | 卡滞观测计数。 |
| `StuckAmount` | `int` | `0` | 卡滞关联的数量统计；确切单位仍需与 Version4 语义对齐。 |
| `WetCounter` | `int` | `0` | 当前世界运行态湿润相关计数；它是否应改归实体汇总仍待确认。 |
| `PanicCounter` | `int` | `0` | 保护模式计数。 |
| `PanicMode` | `bool` | `false` | 是否处于 panic 保护状态。 |
| `PanicY` | `int` | `0` | panic 保护关联的高度值；零值不代表一个新的实体状态。 |

设计约束：

- 这些字段共同描述同一个世界的运行态，暂不拆成容量、保护、卡滞等多个组件，以免形成互相竞争的 singleton owner。
- `MaxLiquidBuffer` 不拥有缓冲项本身，`CurrentWorkCount` 也不拥有任何坐标；二者只是世界级约束和汇总。
- `WetCounter` 的最终语义若被证明是实体接触的逐实体状态，则应从本组件移出；当前证据不足以直接完成该移动。

### 5.4 LiquidContactStateComponent

| 项目 | 定义 |
| --- | --- |
| 附着范围 | Player、NPC、Projectile 等可能接触液体的实体；每个实体最多一个。 |
| 组件职责 | 保存从 Tile 液体和实体接触事实解析出的短期派生结果。 |
| 默认存在 | 可按实体能力选择性存在；缺省等价于所有湿润标志为 `false`、类型为 `0`、计数为 `0`。 |
| 持久化 | 默认不持久化；由实体当前状态和液体接触事实重新得出。 |
| 证据状态 | Version4 接触事实 `strong`；当前 NLTX 类型映射与时间/实体引用为 partial。 |

| 字段 | 类型 | 默认值 | 语义与不变量 |
| --- | --- | --- | --- |
| `ResolvedAtTick` | `long` 或 `WorldTime` | `0` | 最近一次接触结果对应的世界时间；`WorldTime` owner 见 BD-COMP-02。 |
| `IsWet` | `bool` | `false` | 实体是否处于一般湿润状态。 |
| `IsLavaWet` | `bool` | `false` | 实体是否带有岩浆湿润结果。 |
| `IsHoneyWet` | `bool` | `false` | 实体是否带有蜂蜜湿润结果。 |
| `IsShimmerWet` | `bool` | `false` | 实体是否带有微光湿润结果。 |
| `WetTickCount` | `int` | `0` | 实体湿润结果的持续计数。 |
| `DominantLiquidType` | `byte` 或 `LiquidTypeId` | `0` | 当前接触结果的主液体类型；不反向拥有 Tile 数量。 |

设计约束：

- 该组件只承载实体侧结果，不复制 TileCell 的 `Amount`、`Type` 或工作标志。
- `EntityReference` 若用于记录来源，只能作为跨域引用，不能成为本组件的液体权威字段；其 owner 见 BD-COMP-02。
- Version4 的网络或碰撞读取是证据来源，不等于本组件需要拥有对应网络载荷或读取结构。

### 5.5 LiquidDirtySectionStateComponent

| 项目 | 定义 |
| --- | --- |
| 附着范围 | WorldSection；每个 Section 最多一个。Section 身份由外部世界分区索引确定。 |
| 组件职责 | 表达该 Section 的液体相关变更发布状态。 |
| 默认存在 | 可以按需创建；缺省表示 `IsDirty = false`。 |
| 持久化 | 默认不持久化；世界载入后从新的 Tile 变化重新建立。 |
| 证据状态 | Version4 脏集合事实 strong；Section 附着方式 partial。 |

| 字段 | 类型 | 默认值 | 语义与不变量 |
| --- | --- | --- | --- |
| `Revision` | `long` | `0` | 该 Section 的液体状态版本；递增语义和并发可见性仍需由共享 owner 确认。 |
| `IsDirty` | `bool` | `false` | 表示该 Section 是否存在尚未发布的液体变更。 |

设计约束：

- 该组件只表达 Section 级变更标记，不存储变更坐标列表、不存储网络快照，也不存储 Tile 液体数量。
- 当前 Dome 的 `LiquidDirtySectionComponent` 使用 World 范围字典；在最终设计中必须选择“每个 Section 一个组件”或“World 范围集合承载”，不能让两者同时成为权威。

### 5.6 LiquidSequenceStateComponent

| 项目 | 定义 |
| --- | --- |
| 附着范围 | World singleton；每个世界最多一个。 |
| 组件职责 | 提供可重复识别液体工作关联的世界级序列值。 |
| 默认存在 | 仅在采用该候选方案时存在；初始值为 `0`。 |
| 持久化 | 默认不持久化；世界加载后可重新从零建立。 |
| 证据状态 | `proposed`、`partial`；不是 Version4 原生成员。 |

| 字段 | 类型 | 默认值 | 语义与不变量 |
| --- | --- | --- | --- |
| `NextLiquidSequence` | `long` | `0` | 下一次分配的工作关联标识来源；它只提供身份稳定性，不定义处理先后规则。 |

该组件目前保留为独立候选，是因为“世界运行计数”与“工作关联身份”可能有不同生命周期。也可以将字段并入 `LiquidWorldRuntimeStateComponent`，或完全拒绝创建该组件；三种方案对组件总数的影响见 BD-COMP-03。

## 6. Entity 与 Component 组合

| 承载对象 | 必选组件 | 可选组件 | 禁止的组合含义 |
| --- | --- | --- | --- |
| TileCell / WorldTile | `TileLiquidStateComponent` | `LiquidWorkStateComponent` | 不得把实体接触字段附着到 TileLiquidStateComponent。 |
| 一个液体工作关联 | `LiquidWorkStateComponent` | 无 | 不得把缓冲项另建成第二份数量/类型权威。 |
| World singleton | `LiquidWorldRuntimeStateComponent` | `LiquidSequenceStateComponent` | 不得把容量、panic、卡滞计数分别建成互相竞争的世界 owner。 |
| WorldSection | `LiquidDirtySectionStateComponent` | 无 | 不得把 Section 变更标记复制为每个 Tile 的版本字段。 |
| Player / NPC / Projectile | `LiquidContactStateComponent` | 无 | 不得让接触结果反向成为 Tile 液体的权威来源。 |

组合不变量：

- `TileLiquidStateComponent` 是唯一候选的 Tile 液体数量/类型 owner；`LiquidWorkStateComponent` 只能引用它所在坐标，不能复制它的值。
- TileCell 没有工作关联时，`LiquidWorkStateComponent` 可以不存在；这不改变 TileLiquidStateComponent 的当前数量。
- 同一实体的接触结果与其所在 TileCell 的液体状态是“派生关系”，不是两个可互写的液体存储。
- World singleton 的运行态与 WorldSection 的变更标记可以同时存在，但 Section 标记不携带世界预算字段，世界运行态也不携带 Section 版本。
- 序列组件若被拒绝，工作关联仍必须有明确的重复识别策略；在 owner 裁决前不能假定由文件顺序、数组下标或目录顺序替代。

## 7. 组件拆分与合并决策

| 决策编号 | 决策 | 理由 | 当前状态 |
| --- | --- | --- | --- |
| BD-DEC-01 | 将 Tile 数量/类型与工作标志拆为两个组件 | 前者随 Tile 持久化，后者随运行态重建；清理和工作消费也有不同生命周期。 | candidate |
| BD-DEC-02 | 将容量、预算、cycles、quick、stuck、panic 和运行计数暂时合并到一个世界组件 | 它们共同属于 World singleton；拆开会制造多个世界级 owner，且当前证据没有稳定边界。 | candidate |
| BD-DEC-03 | 将实体接触结果单独建组件 | 接触结果是实体范围的派生短期状态，不应污染 Tile 权威状态。 | candidate |
| BD-DEC-04 | 将 Section 变更标记单独建组件 | `Revision` 和 `IsDirty` 的范围是 WorldSection，不是 Tile 液体值，也不是世界预算。 | candidate |
| BD-DEC-05 | 不为 `LiquidBuffer` 建立独立液体组件 | 它是溢出工作集；独立建模会使工作关联或数量产生第二份权威。 | candidate |
| BD-DEC-06 | 不为液体反应建立持续组件 | Version4 的相关入口包含空实现，完整参考只能补充证据缺口，不能改写当前实现事实。 | candidate |
| BD-DEC-07 | 暂时保留 `LiquidSequenceStateComponent` 作为独立候选 | 工作身份和世界运行计数可能分属不同生命周期；没有足够证据现在合并或删除。 | decision-required |
| BD-DEC-08 | 不把网络快照或表现快照当作领域组件 | 快照是边界数据形状，不能反过来决定领域状态的 owner。 | candidate |

## 8. 不单独创建 Component 的对象

以下对象在当前证据下不生成新的领域组件：

- `LiquidBuffer`：只作为溢出工作集的承载形式；其坐标和工作关联归入 `LiquidWorkStateComponent` 的语义范围。
- `Main` 中的全局液体数组：它是旧宿主，不是 LiquidSimulation 的领域 owner；数组位置也不能决定组件身份或处理顺序。
- 网络液体快照和 `NetLiquidModule` 载荷：它们是跨边界传输形状，不能替代 TileLiquidStateComponent 或 LiquidContactStateComponent。
- `LiquidRenderer` 及其空表现路径：当前没有足够的稳定领域字段，不创建表现组件。
- `UndergroundDesertCheck`、`CreateLiquidMergeTile` 等规则入口：它们是规则证据或证据缺口，不是可独立持久承载的组件状态。
- 水、岩浆、蜂蜜、微光之间的反应规则：规则本身不等于持续状态；一次性结果若未来需要存储，必须先证明有独立生命周期和 owner。
- `TileType`、`WallType`、`IsActive`：虽然与液体同属 TileCell，但不表达液体状态，不进入 TileLiquidStateComponent。
- 当前 Dome 的 `LiquidSourceComponent`：来源标记与 Version4 当前权威液体字段之间尚未建立完整证据链，本候选设计不把它升级为核心组件。

## 9. 当前 NLTX 组件覆盖

当前代码事实与本设计候选不能直接画等号。下表使用 `existing` 表示同名或近似类型确实存在，使用 `partial` 表示只覆盖子集、范围不一致或 owner 未闭合，使用 `proposed` 表示本文件提出但当前代码没有对应的完整类型。

| 候选组件 | 当前 NLTX 证据 | 状态 | 覆盖结论 |
| --- | --- | --- | --- |
| `TileLiquidStateComponent` | `D:\TRbackup\NLTX\src\WorldInteraction\Tiles\TileCellComponent.cs:3-10`；`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\WorldTile.cs:3-27` | partial | 已有 `LiquidAmount`、`LiquidKind` 或 Tile 承载对象，但 `TileCellComponent` 同时混有 `TileType`、`IsActive`、`WallType`，且最终 owner 未定。 |
| `LiquidWorkStateComponent` | `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Liquid\Components\LiquidUpdateQueueComponent.cs:8-146` | partial | 已有工作队列形状，但尚未证明包含全部 `Coordinate`、跳过、计分、延迟、重试和缓冲关联字段，也不能把队列类型直接视为本组件的最终边界。 |
| `LiquidWorldRuntimeStateComponent` | `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Liquid\Components\LiquidWorldStateComponent.cs:5-90`；`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Simulation\DomeSimulation.cs:562` | partial | 已有世界液体状态，但 Version4 的预算、quick、stuck、panic 和计数覆盖不完整；当前类型不自动获得全部字段。 |
| `LiquidContactStateComponent` | `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Liquid\Components\LiquidContactComponent.cs:5-59`；`D:\TRbackup\NLTX\src\Share\Entity\Components\LiquidComponent.cs:3-35` | existing / partial | Dome 已有接触组件，根 src 也有同名语义的实体组件；类型映射、时间字段和跨域引用 owner 未统一。 |
| `LiquidDirtySectionStateComponent` | `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Liquid\Components\LiquidDirtySectionComponent.cs:6-25` | existing / partial | 已有脏区字段，但当前实现是 World 范围字典；本设计要求明确 Section 附着或 World 集合的唯一权威形状。 |
| `LiquidSequenceStateComponent` | `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Liquid\Components\LiquidUpdateQueueComponent.cs:8-146` | proposed | 当前没有足够证据证明已有独立序列字段；本文件只保留候选，不把现有队列下标当作它的实现。 |

其他现有类型的处理：

- `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Liquid\Components\LiquidSourceComponent.cs:3-8` 只表达来源候选，不足以覆盖 Tile 权威数量/类型，因此不列入最终六项核心清单。
- `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Liquid\Snapshots\LiquidReplicationSnapshot.cs:1-8` 是快照形状，不增加领域组件数量。
- `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\Snapshots\DomeSimulationSnapshot.cs:31-32` 和 `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Server\Persistence\DomeStatePersistenceFormat.cs:276-289` 只证明当前存在快照/持久化承载，不证明这些承载就是六个组件的最终 owner。
- `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\WorldGrid.cs:116-137`、`150-188` 证明网格拥有世界坐标与 Tile 访问入口，但不解决 TileLiquidStateComponent 的最终归属。

因此总体 `nltxStatus` 保持 `evidence-mismatch`：当前代码有若干同名或近似组件，但它们的字段覆盖、范围和 owner 与 Version4 证据尚未完全一致。

## 10. 组件级 evidence-gap

以下八项是阻止本设计从 `candidate` 升为 `baseline` 的组件级证据缺口。它们不是实现任务，也不应在没有新证据时通过重命名类型来假装解决。

| 编号 | 缺口 | 影响的组件 | 当前可确认事实 | 仍需确认的内容 |
| --- | --- | --- | --- | --- |
| BD-GAP-01 | Tile 液体权威 owner 未闭合 | `TileLiquidStateComponent` | Version4 的 `Tile.liquid` 和 `liquidType` 是 Tile 权威字段；NLTX 已有 TileCell/WorldTile 承载。 | 最终由 WorldStorage、LiquidSimulation 自有存储，还是 WorldInteraction 的受控 seam 承载。 |
| BD-GAP-02 | 液体类型值对象映射未闭合 | `TileLiquidStateComponent`、`LiquidContactStateComponent` | Version4 路径可见水、岩浆、蜂蜜、微光；根 src 的 `LiquidKind` 还包含 `Nano`。 | 是否采用 `LiquidTypeId`、如何处理 `Nano`，以及 Tile 类型和接触类型是否共享同一 owner。 |
| BD-GAP-03 | 工作坐标值对象和承载范围未闭合 | `LiquidWorkStateComponent` | Version4 的 `Liquid.x/y` 和 `LiquidBuffer` 坐标是工作关联。 | `TileCoordinate` 的 owner、工作关联附着 TileCell 还是 World 工作表，以及重复坐标的唯一性语义。 |
| BD-GAP-04 | 工作字段的精确类型和生命周期未闭合 | `LiquidWorkStateComponent` | `kill`、`delay`、跳过/检查标志可从 Version4 直接追溯。 | `RetryCount`、`Sequence` 是否真实需要、各字段上限和清理时点，以及 `Delay` 的时间单位。 |
| BD-GAP-05 | 世界运行计数的语义边界未闭合 | `LiquidWorldRuntimeStateComponent` | 容量、cycles、quick、stuck、panic 字段确有 Version4 依据。 | `CurrentMaxLiquid`、`StuckAmount`、`WetCounter` 的确切单位、归零条件，以及 `WetCounter` 是否应移到实体接触侧。 |
| BD-GAP-06 | 接触结果的时间和引用类型未闭合 | `LiquidContactStateComponent` | Player、NPC、Projectile 路径存在湿润或液体类型结果。 | `ResolvedAtTick` 是否统一为 `WorldTime`，是否需要 `EntityReference`，以及这些共享类型的 owner。 |
| BD-GAP-07 | Section 变更的唯一承载形状未闭合 | `LiquidDirtySectionStateComponent` | Version4 有液体脏集合；Dome 有 World 范围字典。 | 采用每个 WorldSection 一个组件，还是 World 范围集合；`Revision` 的递增和可见性语义。 |
| BD-GAP-08 | 稳定工作身份是否属于独立组件未闭合 | `LiquidSequenceStateComponent`、`LiquidWorkStateComponent` | Version4 有工作项和缓冲工作集，但没有足够的原生独立序列字段证据。 | 保留独立序列组件、合并到世界运行态，还是拒绝创建；三者对工作关联字段的影响。 |

## 11. 未决组件 owner

本节保留最终裁决，不替 owner 做未经证据支持的决定。每个决策都必须同时确定字段的唯一写入归属、读取边界和生命周期；仅确定类名不算完成。

### BD-COMP-01：TileLiquidStateComponent 的最终 owner

候选方案：

- **WorldStorage**：Tile 液体与世界 Tile 持久化最接近，保存/读取边界清晰；代价是 LiquidSimulation 需要一个受控访问边界，且 WorldStorage 不能顺带拥有工作状态。
- **LiquidSimulation 自有存储**：液体状态集中且领域隔离更清楚；代价是世界 Tile 与液体状态之间需要一致性维护，持久化必须明确双向边界。
- **WorldInteraction 受控 seam**：便于复用现有 TileCell 访问；代价是交互层可能重新吸收液体领域职责，必须限制为承载/访问而非运行规则 owner。

当前结论：`decision-required`。在裁决前，`TileCellComponent` 的 `LiquidAmount/LiquidKind` 只能标为 partial，不能宣布为最终组件。

### BD-COMP-02：共享值对象的 owner

涉及 `LiquidTypeId`、`TileCoordinate`、`WorldSectionId`、`EntityReference`、`WorldTime` 和可能的 `NetworkId`。

候选方案：

- **各自归入表达其语义的能力域**：类型放液体域，坐标放世界域，实体引用放实体域；边界最清楚，但需要少量跨域引用协议。
- **归入统一的基础值对象域**：复用方便，字段类型稳定；代价是容易形成无语义的共享杂物区，并使 owner 退化为“谁先定义谁拥有”。
- **保留集成层契约、领域侧使用别名**：可缓解循环依赖；代价是需要维护别名映射和版本兼容，不能把契约直接当成组件。

当前结论：`decision-required`。在裁决前，各组件字段可使用 integration-level 类型名作为候选，但不锁定物理目录或命名空间。

### BD-COMP-03：是否保留 LiquidSequenceStateComponent

候选方案：

- **保留独立组件**：明确区分工作身份与世界预算生命周期，适合需要跨缓冲区识别同一关联的方案；组件数量保持六项。
- **并入 LiquidWorldRuntimeStateComponent**：减少 singleton 数量，读取入口简单；代价是世界运行配置与工作身份耦合，序列字段的重建边界更难单独表达。
- **拒绝创建**：只使用已有工作关联身份；组件数量降为五项；代价是必须证明数组下标、坐标或其他已有身份足以区分重复关联，且不能靠顺序约定补足。

当前结论：`decision-required`。本文件暂保留该候选，但不把它描述为 Version4 已有事实。

### BD-COMP-04：根 LiquidComponent 与 Dome LiquidContactComponent 的统一关系

候选方案：

- **统一为 LiquidContactStateComponent 的一个跨层实现**：避免两个“液体接触” owner，字段映射集中；代价是要处理根 src 的 `LiquidKind.Nano` 和 Dome 字段差异。
- **保留两层类型、共享明确契约**：迁移风险较低，各层可保留局部表达；代价是必须防止两个类型同时成为实体接触状态的权威。
- **只保留一层作为领域组件，另一层降为边界映射**：权威唯一；代价是需要明确哪些字段属于边界数据，不能继续把近似同名类型当作现成覆盖。

当前结论：`decision-required`。在裁决前，当前 NLTX 覆盖只能记为 existing/partial。

### BD-COMP-05：LiquidDirtySectionStateComponent 的 Section 承载方式

候选方案：

- **每个 WorldSection 一个组件**：范围与 `Revision/IsDirty` 一致，查询单个 Section 直观；代价是需要稳定的 `WorldSectionId` owner 和 Section 生命周期。
- **World 范围集合承载**：贴近现有 Dome 字典，更新集中；代价是组件附着范围不再直观，必须防止集合和 Section 组件并存为双重权威。
- **WorldSection 只保存版本，World 范围保存发布集合**：可区分本地版本和待发布集合；代价是字段归属变成两级，必须明确两者的一致性规则。

当前结论：`decision-required`。在裁决前，`LiquidDirtySectionComponent` 只能算 existing/partial，不能直接替代最终组件。

## 12. 最终 Component 清单

以下清单是当前证据下的六个候选组件，不是已批准的实现清单。`candidate` 表示来源和范围基本明确，`proposed` 表示需要额外的设计裁决。

| 序号 | Component | 附着范围 | 核心字段 | 状态 | owner 状态 |
| --- | --- | --- | --- | --- | --- |
| 1 | `TileLiquidStateComponent` | TileCell / WorldTile | `Amount`、`Type` | candidate | BD-COMP-01、BD-COMP-02 |
| 2 | `LiquidWorkStateComponent` | TileCell 工作关联 / World 工作表条目 | `Coordinate`、`IsPending`、`SkipOnce`、`KillScore`、`Delay`、`Sequence`、`RetryCount`、`IsBuffered` | candidate | BD-COMP-02、BD-COMP-03 |
| 3 | `LiquidWorldRuntimeStateComponent` | World singleton | 容量、预算、计数、cycles、quick、stuck、panic、wet 运行字段 | candidate | BD-COMP-02；`WetCounter` 受 BD-GAP-05 影响 |
| 4 | `LiquidContactStateComponent` | Player / NPC / Projectile | `ResolvedAtTick`、湿润标志、`WetTickCount`、`DominantLiquidType` | candidate | BD-COMP-02、BD-COMP-04 |
| 5 | `LiquidDirtySectionStateComponent` | WorldSection | `Revision`、`IsDirty` | candidate | BD-COMP-02、BD-COMP-05 |
| 6 | `LiquidSequenceStateComponent` | World singleton | `NextLiquidSequence` | proposed | BD-COMP-02、BD-COMP-03 |

数量说明：当前候选清单为 **6 个**。如果 BD-COMP-03 最终拒绝独立序列组件，清单才会收敛为 5 个；在裁决发生前不能提前减少或合并。

## 13. 最终声明

本文件是基于指定研究报告和已核对源码证据形成的 `component-only` 候选设计。它只定义六个候选组件的状态归属、字段语义、组合不变量和 owner 决策点，不定义任何执行流程、访问筛选、边界传输、表现承载或验证安排。

结论如下：

- `TileLiquidStateComponent` 是 Tile 液体数量/类型的候选权威；`LiquidWorkStateComponent` 只承载工作关联；`LiquidContactStateComponent` 只承载实体侧派生结果。
- 世界级运行字段暂时合并在 `LiquidWorldRuntimeStateComponent`；Section 变更标记单独归入 `LiquidDirtySectionStateComponent`。
- `LiquidSequenceStateComponent` 只作为 proposed 候选保留，是否存在必须由 BD-COMP-03 裁决。
- 当前 NLTX 已有多个近似组件，但字段覆盖和 owner 与 Version4 证据存在 mismatch；因此 `designStatus` 必须保持 `candidate`，`evidenceStatus` 必须保持 `partial`。
- 本文件不授权对生产源码、研究报告或现有组件进行修改；`verificationStatus` 保持 `not-run`。

