# WorldSession Component Design

## 1. 设计元数据

```text
subsystemId: WorldSession
taskNumber: 04
sourceReport: D:\TRbackup\NLTX\docs\research\2026-09-05-version4-world-session-public-decomposition.md
outputDesignPath: D:\TRbackup\NLTX\docs\design\2026-09-05-version4-world-session-component-design.md
designScope: component-only
designStatus: decision-required
evidenceStatus: partial
nltxStatus: partial
verificationStatus: not-run
selectionMethod: 仅使用当前会话明确产生的 WorldSession 研究报告；按 Version4 优先、完整参考补缺、tModLoader 公开边界交叉核对、SS14 组件粒度参考的顺序，重新定位与组件字段直接相关的证据；未读取其他 research 报告
inputSelectionStatus: confirmed-current-session-report
evidenceMismatch: 任务材料给出的 Terraria\WorldFile.cs 不存在，实际证据路径为 D:\TRbackup\Version4\Terraria.IO\WorldFile.cs
```

本设计的 `decision-required` 不是实现阻塞声明，而是对 Hardmode、事件进度、readiness 转移、共享快照和跨子系统字段仍需整合裁决的如实标记。`baseline` 不适用于当前证据状态。

## 2. 设计范围与排除范围

本文件只定义 `WorldSession` 的 Component 候选及其字段、状态分类、所有权、生命周期、作用域、ID 分离和组合关系。组件的直接责任面限定为：

- 世界描述与持久世界身份；
- 基础世界规则事实；
- 世界时钟事实；
- 基础天气事实及其明确标记为缓存的成员；
- 世界加载/生成 readiness 门控事实；
- 在单一提交点形成的不可变世界会话快照。

以下内容不在本设计中作为运行时结构或一级 Component 展开：

- System、Query、Command、Event、Adapter、Projection、Port、Coordinator、Pipeline、Scheduler、Verifier；
- 调度阶段、执行顺序、主循环、网络发送流程、存档流程、客户端渲染流程；
- Tile、Liquid、Section、Chest、Sign、TileEntity、Player、NPC、Projectile、WorldItem 及其行为；
- 昼夜边界事件、Blood Moon、Eclipse、Pumpkin Moon、Snow Moon、Slime Rain、Lantern Night、Birthday Party、Sandstorm 的完整事件归属；
- Hardmode 世界转换事务、NPC Boss/解锁/入侵进度的完整归属；
- Creative/Journey 覆写的完整归属；
- 迁移计划、实现步骤、`.cs`/`.csproj` 计划、测试计划和 focused verifier 设计。

存档临时字段和包 7 字段只作为兼容字段或快照字段的证据，不被复制成核心 Component 的协议镜像。所有跨两个或以上子系统使用的字段、类型、ID 或关系都保留 `crossSubsystemOwner: integration-review`，不在本文件宣布最终 owner。

## 3. 组件设计依据

### 3.1 直接复核的 Version4 事实

| 证据 | 复核到的组件字段事实 | 对本设计的限制 | evidenceStatus |
|---|---|---|---|
| `D:\TRbackup\Version4\Terraria\Main.cs:594-624,652-664` | `dayTime`、`time`、`moonPhase`、雨、雨计时器、雨强、云、风和天气计数器均位于 `Main` 静态状态中 | 不能按单个静态字段机械建 Component；必须区分时钟、天气事实、缓存和表现输入 | confirmed |
| `D:\TRbackup\Version4\Terraria\Main.cs:99-104,1035,1763-1769,11400-11409` | readiness 只有 `AwaitingData`、`ProcessingData`、`Ready`；当前方法会把状态设为 `Ready`；完整实体更新还依赖 `!WorldGen.generatingWorld` | `Failed`/`Unloading` 需要当前 NLTX 与加载边界整合；不能把更丰富枚举写成 Version4 已确认事实 | partial / unresolved |
| `D:\TRbackup\Version4\Terraria\Main.cs:11193-11361,11411-11620` | 天气更新位于实体更新之前；`UpdateTime` 位于玩家、NPC、Projectile、WorldItem 和绳系实体之后，WorldGen 更新之前 | Component 只记录状态；不能用组件拆分暗示新的时序或提前提交 | confirmed |
| `D:\TRbackup\Version4\Terraria\Main.cs:12050-12252,12820-12924` | 风当前值向雨强调整后的目标值插值；雨启动/停止/变化写入雨事实；天气计数器和云数量存在缓存/表现性质 | 风当前值、风目标、雨事实和云表现不能合并成无分类的天气巨型 Component | confirmed / partial |
| `D:\TRbackup\Version4\Terraria\Main.cs:12972-13564` | `UpdateTime` 推进时钟并处理雨/Slime Rain，昼夜边界写入昼夜、月相、Blood Moon、Eclipse 等状态 | 基础时钟与跨域事件字段必须拆开；边界事件 owner 保留整合裁决 | confirmed |
| `D:\TRbackup\Version4\Terraria\WorldGen.cs:26084-26125` | `StartHardmode` 先写 `Main.hardMode=true`，再进行后台转换、I/O 锁和主线程后续处理 | `HardMode` 可作为规则快照字段，但不能由 WorldSession Component 单独表达整个转换事务 | confirmed |
| `D:\TRbackup\Version4\Terraria\WorldGen.cs:59400-59470,6431-6501` | 世界更新受生成/加载门控；清理会重置事件、天气、Hardmode、临时字段和生成计数 | readiness、规则、天气缓存的清理生命周期必须可分辨 | confirmed |
| `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:98-180,1035-1099` | `_temp*` 字段捕获并恢复时钟、天气和事件；它们是保存事务内 staging，不是运行时长期权威状态 | 不创建持久的临时 staging Component，不让保存临时值反向成为核心 owner | confirmed |
| `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1263-1385,2008-2251` | 世界头保存/加载身份、尺寸、规则、时钟、天气和跨域进度，加载包含版本分支和默认值 | 组件字段要与文件协议解耦；版本默认只能作为兼容映射证据，不能替代运行时不变量 | confirmed |
| `D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs:15-118,291-300,325-349` | 元数据包含 `WorldId`、`UniqueId`、种子、生成器版本、模式、加载状态/异常；无效世界使用明确失败数据 | 世界身份、加载失败状态和异常诊断要分离；`LoadException` 不进入核心 Component | confirmed |
| `D:\TRbackup\Version4\Terraria\NetMessage.cs:221-403` | 包 7 混合时钟、天气、世界描述、规则和进度字段 | 包 7 只能证明跨域快照消费边界，不能成为 WorldSession Component 布局；`crossSubsystemOwner: integration-review` | confirmed |

### 3.2 补充来源的角色

完整参考中 `D:\TRbackup\无任何删减通过编译\Terraria.IO\WorldFileData.cs:229-337` 补足了种子选项和秘密种子解析的实现，而 Version4 对应方法存在空体或默认返回。该内容只能作为补证和迁移风险，不能改写为 Version4 当前行为；相关字段 evidenceStatus 保持 `partial`。

`D:\TRbackup\tmodloader-api-docs-stable\index.html` 显示标题 `tModLoader: Main Page`、版本 `tModLoader v2026.07`。`D:\TRbackup\tmodloader-api-docs-stable\class_mod_system.html` 的 `PostUpdateTime`（锚点 `aef069f82d408785fbde30b0327579879`）、`PostWorldLoad`（锚点 `a420a22b570f31900387ca75e60c5256e`）、`NetReceive`（锚点 `a144ef598fa0b3bcc2a0ac6bd1c5467aa`）、`NetSend`（锚点 `af9ebfea8b152b555b030265946cace70`）和 `SaveWorldData`（锚点 `a926129e278ac9c460685bd2a326cd998`）只用于交叉确认公开生命周期和边界，不替代 Version4 私有字段语义。

Space Station 14 的 `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Clock\GlobalTimeManagerComponent.cs`、`C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\GameTicking\SharedGameTicker.cs`、`C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Server\GameTicking\GameTicker.RoundFlow.cs` 和 `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Client\GameTicking\Managers\ClientGameTicker.cs` 只支持“小而内聚的时间/round 状态、服务器 readiness 与客户端只读状态分离”的结构参考。该参考无 Terraria 直接对应语义；不复制其代码、命名、目录或领域模型。

## 4. Version4 成员到 Component 归属表

| 成员 | 声明类型 | 字段含义 | 权威/派生/缓存/快照/兼容 | 生命周期 | proposed Component | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|---|
| `worldName` | `Terraria.Main` | 世界显示名称 | 权威、持久化 | 创建、加载、保存 | `WorldDescriptorState`（status: proposed；`crossSubsystemOwner: integration-review`） | confirmed | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1267,2013` |
| `WorldId`、`UniqueId` | `Terraria.IO.WorldFileData` | 世界持久身份与 GUID | 权威、持久化、跨边界 ID | 元数据创建/加载/激活 | `WorldDescriptorState`（status: proposed；`crossSubsystemOwner: integration-review`） | confirmed | `D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs:39-41,291-300` |
| `SeedText`、`WorldGeneratorVersion` | `Terraria.IO.WorldFileData` | 用户种子文本和生成器版本 | 权威、兼容、持久化 | 创建、加载、保存 | `WorldDescriptorState`（status: proposed；`crossSubsystemOwner: integration-review`） | confirmed / partial | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1267-1271,2013-2019`；完整参考种子解析补证为 partial |
| `leftWorld`、`rightWorld`、`topWorld`、`bottomWorld`、`maxTilesX`、`maxTilesY` | `Terraria.Main` | 世界边界和尺寸 | 权威、持久化、跨边界值 | 生成/加载后稳定，清理时失效 | `WorldDescriptorState`（status: proposed；`crossSubsystemOwner: integration-review`） | confirmed | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1272-1277,2029-2034` |
| `worldSurface`、`rockLayer`、`spawnTileX`、`spawnTileY`、`dungeonX`、`dungeonY` | `Terraria.Main` | 地层和世界锚点 | 权威、持久化、跨边界关系 | 生成/加载后稳定 | `WorldDescriptorState`（status: proposed；`crossSubsystemOwner: integration-review`） | confirmed | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1308-1318,2121-2132` |
| `GameMode`、秘密种子布尔标志 | `Terraria.Main` / `WorldFileData` | 基础模式和秘密种子规则 | 权威、持久化、跨域读取 | 创建/加载/规则恢复 | `WorldRulesState`（status: proposed；`crossSubsystemOwner: integration-review`） | confirmed / partial | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1278-1287,2038-2078`；秘密种子解析 partial |
| `WorldGen.crimson` | `Terraria.WorldGen` | 世界邪恶类型事实 | 权威、持久化、跨域读取 | 创建/加载，特殊世界规则可能改变 | `WorldRulesState`（status: proposed；`crossSubsystemOwner: integration-review`） | confirmed | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1319,2133`；`D:\TRbackup\Version4\Terraria\Main.cs:13510-13513` |
| `Main.hardMode` | `Terraria.Main` | Hardmode 已进入事实 | 权威快照、跨域进度 | 加载/转换/清理 | `WorldRulesState.HardMode`（status: proposed；`crossSubsystemOwner: integration-review`） | confirmed | `D:\TRbackup\Version4\Terraria\WorldGen.cs:26088-26100`；`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1342,2159` |
| `WorldGen.SavedOreTiers.*` | `Terraria.WorldGen.SavedOreTiers` | 矿石层兼容/生成规则值 | 兼容、持久化、跨域 | 生成/加载/清理 | `WorldRulesState.SavedOreTiers`（status: proposed；`crossSubsystemOwner: integration-review`） | confirmed | `D:\TRbackup\Version4\Terraria\WorldGen.cs:6485-6491`；`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1353-1355,2184-2186` |
| `dayTime`、`time`、`moonPhase` | `Terraria.Main` | 昼夜、相位内时间和月相 | 权威世界事实、持久化、跨域读取 | 创建/加载、每完整世界 Tick、昼夜边界、清理 | `WorldClockState`（status: proposed；`crossSubsystemOwner: integration-review`） | confirmed | `D:\TRbackup\Version4\Terraria\Main.cs:594-600,12972-13509` |
| `raining`、`rainTime`、`maxRaining` | `Terraria.Main` | 降雨、剩余时间和目标强度 | 权威天气事实、持久化、跨域读取 | 天气启动/变化/停止、加载/清理 | `WorldWeatherState`（status: proposed；`crossSubsystemOwner: integration-review`） | confirmed | `D:\TRbackup\Version4\Terraria\Main.cs:614-620,12820-12923` |
| `windSpeedTarget`、`windSpeedCurrent` | `Terraria.Main` | 风目标和当前插值值 | 目标为权威事实；当前值为模拟状态/可缓存 | 天气更新、加载/清理 | `WorldWeatherState`（status: proposed；`crossSubsystemOwner: integration-review`） | confirmed / partial | `D:\TRbackup\Version4\Terraria\Main.cs:658-660,12054-12072`；完整边界仍 partial |
| `weatherCounter`、`windCounter`、`extremeWindCounter`、`oldMaxRaining` | `Terraria.Main` | 天气更新与变化缓存 | 缓存，不自动升级为持久权威事实 | 天气更新、清理/重新初始化 | `WorldWeatherState` 内的缓存候选（status: proposed；`crossSubsystemOwner: integration-review`） | partial | `D:\TRbackup\Version4\Terraria\Main.cs:616,652,662-664,12180-12250` |
| `cloudAlpha`、`numClouds`、`numCloudsTemp`、`cloudBGActive` | `Terraria.Main` | 云和视觉/环境表现输入 | 派生、缓存或表现输入 | 高频天气更新、客户端表现、加载初始化 | 不进入核心 Component；见第 8 节 | partial | `D:\TRbackup\Version4\Terraria\Main.cs:612,649-656,12084-12221` |
| `bloodMoon`、`eclipse`、`pumpkinMoon`、`snowMoon`、`slimeRain*` | `Terraria.Main` | 跨域日历/事件运行态 | 事件事实、持久化/网络兼容；owner 未决 | 昼夜边界、事件持续期、清理 | 不进入 WorldSession 基础 Component；`crossSubsystemOwner: integration-review` | confirmed / unresolved | `D:\TRbackup\Version4\Terraria\Main.cs:606-610,624,12925-12970,13191-13537` |
| `NPC.downed*`、saved NPC、invasion、tower、DD2、spawn unlocks | `Terraria.NPC` / `Main` / `WorldGen` | 世界进度与解锁 | 权威进度、持久化/网络兼容；跨域写集 | 玩法触发、加载/清理 | 不进入 WorldSession 基础 Component；`crossSubsystemOwner: integration-review` | partial | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1320-1348,2155-2174`；`D:\TRbackup\Version4\Terraria\NetMessage.cs:282-378` |
| `WorldPreparationState`、`_worldPreparationState` | `Terraria.Main` | 当前世界 readiness 阶段 | 权威生命周期门控 | 进入世界前、完整更新期间、清理 | `SessionReadinessState`（status: proposed；`crossSubsystemOwner: integration-review`） | confirmed | `D:\TRbackup\Version4\Terraria\Main.cs:99-104,1035,1763-1769` |
| `generatingWorld`、`isGeneratingOrLoadingWorld`、`loadFailed` | `Terraria.WorldGen` | 生成/加载屏障和失败标志 | 权威门控/失败事实；转移来源 unresolved | 加载/生成/失败/清理 | `SessionReadinessState`（status: proposed；`crossSubsystemOwner: integration-review`） | confirmed / unresolved | `D:\TRbackup\Version4\Terraria\WorldGen.cs:4151,4165,4287-4290,6290-6292`；`D:\TRbackup\Version4\Terraria\Main.cs:11404-11408` |
| `LoadStatus`、`LoadException`、`SetAsActive` | `Terraria.IO.WorldFileData` | 元数据激活和加载诊断 | `LoadStatus` 为生命周期事实；异常为边界诊断 | 元数据创建/加载失败/激活 | `SessionReadinessState` 只接收稳定失败码；异常对象不进入 Component | confirmed | `D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs:35-39,291-300,325-349` |
| `WorldFile._temp*` | `Terraria.IO.WorldFile` | 保存期间的时钟/天气/事件临时副本 | 兼容 staging，不是权威、不是长期 Component | 保存事务内、失败/恢复边界 | 不进入核心 Component；只保留为兼容映射 | confirmed | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:98-180,1035-1099` |
| 包 7 的时间、天气、描述和规则字段 | `Terraria.NetMessage` | 跨域世界数据传输值 | 快照/兼容；`crossSubsystemOwner: integration-review` | 服务器发送、客户端接收 | `WorldTickSnapshot` 只提供不可变输入，不复刻 wire layout | confirmed / unresolved | `D:\TRbackup\Version4\Terraria\NetMessage.cs:221-403`；客户端 case 7 证据 unresolved |

## 5. Component 定义

以下每个一级 Component 都是设计提案，均明确写为 `status: proposed`。字段表中的“默认值”区分 Version4 观察到的初始/清理值、加载结果值和尚未由 Version4 统一规定的值；不把不确定值伪装成固定默认值。

### 5.1 WorldDescriptorState

#### 职责

保存一个世界的稳定描述、生成元数据、几何边界和进入该世界所需的锚点。它不保存 Tile/Section 内容、实体槽位、网络 ID 或连接状态。

```text
componentId: WS-COMP-01
name: WorldDescriptorState
status: proposed
componentOwner: WorldSession
crossSubsystemOwner: integration-review
entityScope: 一个活动 WorldSession 根；不是每个玩家、NPC 或 Tile 的 Component
lifecycle: 世界元数据创建时建立；世界头加载或生成完成时填充；激活失败时不提交为有效描述；卸载/清理时失效
currentNltxStatus: partial
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `WorldId` | `int` | `0` 仅作为无效世界数据的哨兵；有效值由加载/生成提供 | 权威持久 ID；`crossSubsystemOwner: integration-review` | 不得当作 Entity ID、Network ID 或文件路径键的隐式替代；`crossSubsystemOwner: integration-review` | confirmed | `D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs:41,325-340`；`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1271,2028` |
| `UniqueId` | `Guid` | `Guid.Empty` 仅用于无效世界；旧版本加载可能生成新 GUID | 权威 GUID；`crossSubsystemOwner: integration-review` | 与 `WorldId` 分离；有效加载的 GUID 读取/生成规则必须保持版本兼容；`crossSubsystemOwner: integration-review` | confirmed | `D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs:39,325-340`；`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1270,2022-2027` |
| `Name` | `string` | `""`；保存边界可将空名规范为 `World` | 权威显示元数据；`crossSubsystemOwner: integration-review` | 非空规范化只能发生在边界；不以名称代替 ID | confirmed | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1267,2013`；`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:901-904` |
| `SeedText` | `string` | `""`（`WorldFileData` 字段初始值） | 权威/兼容元数据；`crossSubsystemOwner: integration-review` | 长度和秘密种子解析规则不能在本组件中臆造；完整参考补证仍不代表 Version4 当前行为 | partial | `D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs:31,84`；完整参考 `:242-298` |
| `WorldGeneratorVersion` | `ulong` | `0` 表示无有效种子/无效元数据 | 权威兼容元数据；`crossSubsystemOwner: integration-review` | 版本值用于兼容判断，不等同于当前运行时版本 | confirmed | `D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs:29,104,332`；`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1269,2018` |
| `SizeX`、`SizeY` | `int` | 有效世界由加载/生成提供；无效世界数据为 `1,1` | 权威几何元数据；`crossSubsystemOwner: integration-review` | 有效尺寸必须与世界头和边界一致；`crossSubsystemOwner: integration-review` | confirmed / partial | `D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs:25-27,334`；`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1276-1277,2033-2034` |
| `LeftWorld`、`RightWorld`、`TopWorld`、`BottomWorld` | `double` | 加载/生成前未由本设计规定；无效数据沿用其无效边界 | 权威边界值；`crossSubsystemOwner: integration-review` | 不能混入 Tile/Section 位图；有效边界关系需要整合会话补充验证；`crossSubsystemOwner: integration-review` | confirmed / partial | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1272-1275,2029-2032` |
| `SurfaceLayer`、`RockLayer` | `double` | 加载/生成结果；无统一的 Version4 默认值 | 权威世界锚点；`crossSubsystemOwner: integration-review` | 必须与同一世界描述和尺寸匹配，不得由表现层改写 | confirmed / partial | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1310-1311,2123-2124` |
| `SpawnTileX`、`SpawnTileY` | `int` | 加载/生成结果；无统一的 Version4 默认值 | 权威关系锚点；`crossSubsystemOwner: integration-review` | 坐标必须属于当前世界有效边界；`crossSubsystemOwner: integration-review` | confirmed / partial | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1308-1309,2121-2122` |
| `DungeonTileX`、`DungeonTileY` | `int` | 加载/生成结果；无统一的 Version4 默认值 | 权威关系锚点；`crossSubsystemOwner: integration-review` | 坐标必须与当前世界描述关联，不得当作 Entity/Section ID | confirmed / partial | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1317-1318,2131-2132` |
| `SectionCountX`、`SectionCountY` | `int` | 由尺寸计算，不单独存储 | 派生值；`crossSubsystemOwner: integration-review` | 不能成为 Section 内容所有者；计算规则需与 WorldStorage 约定 | partial | 当前 NLTX `D:\TRbackup\NLTX\src\WorldSession\WorldSessionComponents.cs:20-21`；Version4 包 7/世界头尺寸证据 |

#### 字段不变量

- `WorldId`、`UniqueId`、持久化文件键、`WorldSectionId`、Entity ID 和 Network ID 是不同身份类别；所有跨子系统身份类型均为 `crossSubsystemOwner: integration-review`。
- 有效描述的尺寸、边界、地层和锚点必须来自同一次世界创建/加载提交；不能从客户端传输值反向覆盖权威描述。
- `SectionCountX`、`SectionCountY` 和任何边界视图都是派生值，不得反向修改尺寸或边界。
- 详细几何合法性（边界是否严格递增、锚点是否始终在边界内）在当前证据中未形成统一验证契约，保持 `partial`。

#### 生命周期

组件在世界元数据对象建立时可以存在，但在描述尚未验证前只能表示未激活状态。加载头或生成结果必须一次性提供身份、尺寸、边界和锚点；加载失败不得留下可被完整世界使用的半有效描述。卸载/清理后，旧描述不能继续被新的 WorldSession 根复用。

#### Entity/World 范围

作用域为单个活动 WorldSession 根，语义上是 World 级而非 Entity 级。它可以被多个相邻领域读取，但共享只读描述的最终值对象 owner 仍为 `crossSubsystemOwner: integration-review`。

#### ID 与关系字段

`WorldId` 是世界文件/协议使用的持久整数 ID，`UniqueId` 是世界 GUID；两者都不是 Entity ID、Network ID、账户 ID 或 Section ID。出生点和地牢点是指向世界坐标的关系锚点，不是实体引用。任何 `WorldSectionId`、`EntityReference` 或网络关系都必须在最终整合中单独裁决，`crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

根 `D:\TRbackup\NLTX\src\WorldSession\WorldSessionComponents.cs:6-27` 有同名描述类；`D:\TRbackup\NLTX\src\WorldSession\WorldGeneration/D:\TRbackup\NLTX\src\WorldSession\WorldGeneration\WorldDescriptorState.cs:3-25` 还有不同命名空间的同名类。两者均只能标为 `currentNltxStatus: partial`：状态记录存在，但 ownership、项目闭合和运行时接线未由本轮编译验证。`WorldBounds` 当前是值记录，可作为字段值而不是一级 Component。

#### 证据

主要证据为实际路径 `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1263-1385,2008-2034` 和 `D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs:15-118,291-349`。任务材料中的 `D:\TRbackup\Version4\Terraria\WorldFile.cs` 是 `evidence-mismatch`，不能作为有效路径引用。

### 5.2 WorldRulesState

#### 职责

保存稳定的世界模式、秘密种子规则、世界邪恶类型以及可被世界头恢复的规则事实。`HardMode` 只作为规则事实快照字段候选；Hardmode 转换本身不属于该 Component 的单字段写入语义。

```text
componentId: WS-COMP-02
name: WorldRulesState
status: proposed
componentOwner: WorldSession candidate
crossSubsystemOwner: integration-review
entityScope: 一个活动 WorldSession 根的 World 级规则
lifecycle: 世界创建/加载时初始化；规则恢复或受控转换完成后提交；清理时恢复未激活值
currentNltxStatus: partial
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `GameMode` | `int` | 有效世界由创建/加载提供；无效世界为 `0` | 权威规则；`crossSubsystemOwner: integration-review` | 不把 Difficulty override 混入基础 GameMode；有效范围需由规则整合确认 | confirmed / partial | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1278,2038-2094`；`D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs:45,330` |
| `SecretSeedFlags` | `WorldSecretSeedFlags`（proposed value type） | `None`，对应 Version4 各秘密种子布尔值均为 false 的候选映射 | 权威/兼容规则；`crossSubsystemOwner: integration-review` | 不得把解析算法或有效覆写值藏在 flags 中；Version4 空体解析保持 partial | partial | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1279-1287,2041-2078`；完整参考 `D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs:229-337` |
| `WorldEvil` | `WorldEvilType`（proposed value type） | `Corruption` 作为 `crimson=false` 的候选默认 | 权威规则；`crossSubsystemOwner: integration-review` | 只能有一个基础邪恶类型事实；`WorldGen.crimson` 与该值不得双向无主同步 | confirmed / partial | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1319,2133`；当前 NLTX `D:\TRbackup\NLTX\src\WorldSession\WorldSessionComponents.cs:34-35` |
| `HardMode` | `bool` | `false`（清理和无效世界路径可确认） | 权威规则快照；转换 owner 未决；`crossSubsystemOwner: integration-review` | `true` 只能代表转换事实已提交；不能用普通 bool 写入替代后台转换、锁、区段重置和主线程后续处理 | confirmed / unresolved | `D:\TRbackup\Version4\Terraria\WorldGen.cs:26088-26100,6497`；`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1342,2159` |
| `SavedOreTiers` | `OreTierState`（proposed value type） | 每个层级为 `-1` 的清理候选 | 兼容/生成规则；`crossSubsystemOwner: integration-review` | 不把矿石层内容、Tile 数据或转换事务放入规则 Component | confirmed / partial | `D:\TRbackup\Version4\Terraria\WorldGen.cs:6485-6491`；`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1353-1355,2184-2186` |

#### 字段不变量

- `GameMode` 是基础世界规则；Creative/Journey 的本轮有效覆写不能回写基础字段，覆写 owner 为 `crossSubsystemOwner: integration-review`。
- `HardMode` 是可被保存/网络读取的事实字段候选，不是 Hardmode 转换的完整状态机；转换完成/失败边界需由 `WorldProgressionAndTransition` 与整合会话共同裁决，`crossSubsystemOwner: integration-review`。
- `WorldEvil` 不得同时由规则 Component 和跨域生成状态无条件双写。
- `SavedOreTiers` 是兼容/生成边界数据，不得借此取得 Tile 或 Section 的所有权。

#### 生命周期

创建和加载时由经过验证的世界元数据填充。Hardmode 事实只能在相应转换边界被确认后更新；清理时恢复未激活世界的规则值。秘密种子解析的 Version4 当前行为存在空体差异，加载失败时不能以完整参考实现静默替换行为。

#### Entity/World 范围

作用域为 World 级规则，不附着到玩家、NPC 或单个 Tile。跨域消费者只应读取稳定值；`SimulationRuleOverrides`、Calendar、Progression、WorldGen、Persistence 和 Network 之间的共享规则视图均为 `crossSubsystemOwner: integration-review`。

#### ID 与关系字段

本 Component 不拥有 Entity ID、Network ID、WorldSectionId 或账户 ID。Hardmode 与进度系统的关系通过整合裁决的跨域值/提交边界表达，不把关系字段伪装成规则布尔值；`crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

根 `D:\TRbackup\NLTX\src\WorldSession\WorldSessionComponents.cs:38-56` 已有规则记录；`D:\TRbackup\NLTX\src\WorldSession\WorldGeneration/WorldRulesState.cs:3-18` 还有不同命名空间的同名规则记录。两者都没有被本轮确认接入唯一写入边界，故 `currentNltxStatus: partial`。根记录里的 `DifficultyOverride`、`EffectiveDifficulty` 和 `InfectionSpreadAllowed` 不自动成为本 Component 的基础权威字段。

#### 证据

Hardmode 证据必须同时引用 `D:\TRbackup\Version4\Terraria\WorldGen.cs:26084-26125` 和世界头加载/保存位置；秘密种子字段的完整参考只标为补证，不提升 Version4 evidenceStatus。

### 5.3 WorldClockState

#### 职责

保存权威世界昼夜事实和月相。它不保存客户端视觉时间、事件实例、玩家时钟或调度引用。

```text
componentId: WS-COMP-03
name: WorldClockState
status: proposed
componentOwner: WorldSession candidate
crossSubsystemOwner: integration-review
entityScope: 一个活动 WorldSession 根的 World 级时钟
lifecycle: 世界创建/加载时初始化；保持 Version4 `UpdateTime` 可见槽位更新；昼夜边界更新月相；清理时重置
currentNltxStatus: partial
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `DayTime` | `bool` | `true`（Version4 字段初始值） | 权威世界事实；`crossSubsystemOwner: integration-review` | 只能表示当前昼/夜；事件实例不得反向成为时钟 owner | confirmed | `D:\TRbackup\Version4\Terraria\Main.cs:594`；`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1313,2126` |
| `Time` | `double` | `13500.0`（Version4 字段初始值） | 权威世界事实；`crossSubsystemOwner: integration-review` | 必须保持 Version4 的昼夜范围、边界归零和 `dayRate` 行为；完整范围契约尚未独立验证 | confirmed / partial | `D:\TRbackup\Version4\Terraria\Main.cs:596,12972-13111,13266-13300,13465-13508` |
| `MoonPhase` | `int` | `0`（Version4 字段默认候选） | 权威世界事实、持久化；`crossSubsystemOwner: integration-review` | 只允许 `0..7`；昼夜边界按 Version4 规则推进并回绕 | confirmed | `D:\TRbackup\Version4\Terraria\Main.cs:600,13505-13508`；`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1314,2127` |
| `ClockRevision` | `long` | `0` | 快照一致性元数据，不是 Terraria 世界事实；`crossSubsystemOwner: integration-review` | 只增不减；必须与 `WorldTickSnapshot` 的提交点一致；若整合不需要可删除 | missing | Version4 未找到该字段；为候选整合元数据 |

#### 字段不变量

- `Time`、`DayTime` 和 `MoonPhase` 必须作为同一时钟事实读取；不允许 Calendar、NPC、网络或客户端直接写其中任一字段，`crossSubsystemOwner: integration-review`。
- `MoonPhase` 的合法值是 `0..7`；时间边界的精确常量和快速前进语义仍须以 Version4 回放验证，当前为 `partial`。
- `ClockRevision` 不是替代 `Time` 的新权威时钟；它只用于区分快照新旧。

#### 生命周期

世界描述验证后初始化，加载头恢复后才能成为可用世界时钟。Version4 的时钟更新事实位于实体更新之后、WorldGen 更新之前的 `UpdateTime` 槽位；本 Component 设计只记录这一兼容约束，不在本文件定义调度结构。昼夜边界由时钟事实变化触发相邻事件读取，但事件状态不并入本 Component。清理时回到未激活默认。

#### Entity/World 范围

World 级，一个活动世界只有一份权威时钟。客户端视觉时间、局部 UI 时间和实体内部计时器不属于该 Component。

#### ID 与关系字段

`ClockRevision` 是快照版本，不是 Entity ID、Network ID 或持久化 ID。`WorldTimeView` 若作为跨域值对象被多个子系统使用，必须写 `crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

根 `D:\TRbackup\NLTX\src\WorldSession\WorldSessionComponents.cs:61-64` 把时钟与天气合在 `WorldTimeWeatherState`；`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation/World/WorldClock.cs:5-180` 有独立时钟模型和快照，但不等于根 `src/WorldSession` 已完成实现，均为 `currentNltxStatus: partial`。根项目尚未确认单独 `WorldClockState` 的唯一写入闭合。

#### 证据

Version4 字段和恢复证据为 `D:\TRbackup\Version4\Terraria\Main.cs:594-600,12972-13509` 与 `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:2125-2129`。tModLoader `PostUpdateTime` 只交叉说明公开时间后边界，不改变私有时序结论。


### 5.4 WorldWeatherState

#### 职责

保存雨和风的世界模拟事实，并把更新计数器明确归类为缓存候选。云 alpha、云数量、粒子、音乐和其他客户端表现输入不进入核心权威天气 Component。

```text
componentId: WS-COMP-04
name: WorldWeatherState
status: proposed
componentOwner: WorldSession candidate
crossSubsystemOwner: integration-review
entityScope: 一个活动 WorldSession 根的 World 级天气状态
lifecycle: 世界创建/加载时初始化；天气边界和天气更新时改变；保存可读取稳定快照；清理时重置事实与缓存
currentNltxStatus: partial
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `IsRaining` | `bool` | `false`（停止/清理候选） | 权威天气事实；`crossSubsystemOwner: integration-review` | `false` 时不应把正的 `MaximumRainStrength` 当作有效降雨；网络无雨时包 7 还会归零输出强度 | confirmed | `D:\TRbackup\Version4\Terraria\Main.cs:620,12824-12834`；`D:\TRbackup\Version4\Terraria\NetMessage.cs:277-281` |
| `RainTime` | `int` | `0`（停止/清理候选） | 权威天气计时；持久化；`crossSubsystemOwner: integration-review` | 非负；达到 `5184000` 或更高时对应无尽雨判定；递减必须与 Version4 `dayRate` 保持一致 | confirmed | `D:\TRbackup\Version4\Terraria\Main.cs:618,12901-12906,13015-13035`；`D:\TRbackup\Version4\Terraria\WorldGen.cs:6494-6496` |
| `MaximumRainStrength` | `float` | `0.0`（停止/清理候选） | 权威天气目标强度；持久化/网络；`crossSubsystemOwner: integration-review` | `StopRain` 归零；完整取值范围和 override 合法性仍需补证，不擅自固定为 `[0,1]` | confirmed / partial | `D:\TRbackup\Version4\Terraria\Main.cs:614,12909-12922`；`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1350-1352,2180-2182` |
| `WindSpeedTarget` | `float` | `0.0` 作为静态默认候选 | 权威天气输入；持久化/网络；`crossSubsystemOwner: integration-review` | Version4 在更新中限制目标幅度；精确边界由实际分支决定，不能只引用 dome 限制 | confirmed / partial | `D:\TRbackup\Version4\Terraria\Main.cs:660,12056-12171`；`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1366,2199` |
| `WindSpeedCurrent` | `float` | `0.0` 作为静态默认候选；加载时设为目标值 | 模拟当前值/缓存；`crossSubsystemOwner: integration-review` | 通过目标值逐步趋近；加载恢复必须与目标值的兼容语义一致 | confirmed / partial | `D:\TRbackup\Version4\Terraria\Main.cs:658,12056-12072`；`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:2200` |
| `WeatherCounter` | `int` | `0` 的静态默认候选；加载后由随机范围重新设定 | 缓存/更新计数；`crossSubsystemOwner: integration-review` | 不能未经确认写入持久化核心事实；失效/重建条件需要明确 | partial | `D:\TRbackup\Version4\Terraria\Main.cs:652,12224-12250`；`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:778` |
| `WindCounter`、`ExtremeWindCounter` | `int` | `0` 的静态默认候选 | 缓存/随机策略状态；`crossSubsystemOwner: integration-review` | 不能被表现层或客户端写入；是否随世界保存未在 Version4 头中确认 | partial | `D:\TRbackup\Version4\Terraria\Main.cs:662-664,12080-12157` |
| `PreviousMaximumRainStrength` | `float` | `0.0` 的静态默认候选 | 缓存，用于变化检测；`crossSubsystemOwner: integration-review` | 不能独立成为天气事实；只在变化检测需要时保留 | partial | `D:\TRbackup\Version4\Terraria\Main.cs:616,13104-13108` |

#### 字段不变量

- `IsRaining`、`RainTime` 和 `MaximumRainStrength` 表示同一雨状态，但并非所有缓存都必须与其共同持久化；跨域天气视图标记 `crossSubsystemOwner: integration-review`。
- `WindSpeedTarget` 与 `WindSpeedCurrent` 必须区分目标和当前插值值；不能将当前值当成长期规则目标。
- `WeatherCounter`、`WindCounter`、`ExtremeWindCounter` 和 `PreviousMaximumRainStrength` 只有在确定所有权、失效条件和一致性检查后才可保留；目前属于缓存候选。
- `cloudAlpha`、`numClouds`、`numCloudsTemp`、`cloudBGActive` 不得从表现缓存反写上述权威字段。

#### 生命周期

天气事实在世界加载恢复后可用。Version4 的风/云更新和雨计时分别位于不同调用邻域；本 Component 不把“天气更新”抽象成新的执行结构，也不允许通过提前写入改变时钟边界可见性。保存时只读取稳定天气快照；清理时雨、雨计时和目标强度归零，缓存按其实际失效规则清除。

#### Entity/World 范围

World 级，一活动世界一份。单个雨滴、云对象、粒子、音乐和客户端天气表现不附着到该 Component。

#### ID 与关系字段

本 Component 不拥有 Entity ID、Network ID、Section ID 或玩家 ID。随机源、Creative/Journey 覆写和事件状态是外部输入关系，相关共享值对象必须标为 `crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

根 `D:\TRbackup\NLTX\src\WorldSession\WorldSessionComponents.cs:61-68` 将时间、雨、风和多个事件字段合并为 `WorldTimeWeatherState`；`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation/World/WorldRuleState.cs:5-237` 也有规则/雨/风不可变材料。它们都只能标为 `currentNltxStatus: partial`，因为根项目没有确认独立天气 Component 与 Version4 写入槽位的闭合。

#### 证据

主要证据为 `D:\TRbackup\Version4\Terraria\Main.cs:12050-12252,12820-12970`、`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1350-1366,2180-2200`。包 7 的 `maxRaining` 在无雨时被改写为 0，是协议兼容行为，不是对核心天气字段所有权的证明。

### 5.5 SessionReadinessState

#### 职责

保存活动世界是否处于可供完整世界使用的生命周期阶段，并表达生成/加载屏障和稳定失败码。它不保存异常对象、线程句柄、文件流、网络连接或其他外部资源。

```text
componentId: WS-COMP-05
name: SessionReadinessState
status: proposed
componentOwner: WorldSession candidate
crossSubsystemOwner: integration-review
entityScope: 一个活动 WorldSession 根的会话生命周期状态
lifecycle: 世界选择前建立；加载/生成期间进入准备阶段；验证完成后才可 Ready；失败/卸载时阻断使用；清理后销毁或回到未激活
currentNltxStatus: partial
```

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `Phase` | `SessionReadinessPhase`（proposed value type） | `AwaitingData`，与 Version4 初始值一致 | 权威生命周期事实；`crossSubsystemOwner: integration-review` | Version4 已确认值只有 `AwaitingData`、`ProcessingData`、`Ready`；`Failed`/`Unloading` 是 NLTX 候选扩展，不得写成 V4 事实 | confirmed / unresolved | `D:\TRbackup\Version4\Terraria\Main.cs:99-104,1035`；当前 NLTX `D:\TRbackup\NLTX\src\WorldSession\WorldSessionComponents.cs:5`、`D:\TRbackup\NLTX\src\WorldSession\WorldGeneration\WorldPreparationState.cs:3-11` |
| `GenerationBarrierActive` | `bool` | `false` 作为底层静态标记的候选；准备阶段的有效值由加载/生成结果提供 | 权威门控事实；`crossSubsystemOwner: integration-review` | 只要生成屏障有效，就不得宣告完整世界可用；`generatingWorld` 与 `isGeneratingOrLoadingWorld` 的组合语义尚未闭合 | confirmed / unresolved | `D:\TRbackup\Version4\Terraria\WorldGen.cs:4151,4287-4290,6290-6292`；`D:\TRbackup\Version4\Terraria\Main.cs:11404-11408` |
| `CanUpdateEntities` | `bool` | `false` | 派生资格值，不是独立写集；`crossSubsystemOwner: integration-review` | 仅在 `Phase == Ready` 且生成屏障清除时为 true；不得被外部组合层直接写入 | confirmed | `D:\TRbackup\Version4\Terraria\Main.cs:11400-11409` |
| `FailureStatusCode` | `int?` | `null` | 稳定失败事实；`crossSubsystemOwner: integration-review` | 有失败码时不能同时宣告 Ready；不保存 `Exception` 引用或第三方异常文本 | partial | `D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs:35-39,325-349`；`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:790-807` |

#### 字段不变量

- `CanUpdateEntities` 是 `Phase` 和生成屏障的纯派生结果，不得成为另一个独立权威写者。
- `Failed`、`Unloading` 和加载中的任一候选阶段都必须使 `CanUpdateEntities=false`；当前 Version4 对完整失败/卸载转移的来源未闭合，标记 `unresolved`。
- `LoadException`、云存档对象、Task、锁和文件流是外部诊断/资源，不得进入 Component。
- RuntimeComposition 只能消费 readiness，不得直接把 `Phase` 写成 Ready；该跨域写入约束为 `crossSubsystemOwner: integration-review`。

#### 生命周期

初始世界会话阻断完整实体使用。读取到合法世界头、完成必要生成/液体恢复并清除相应屏障后，才可以提交 Ready。加载异常、版本过新、云不可用或清理请求进入失败/卸载候选状态时，必须保持阻断。当前 `UpdateWorldPreparationState` 每次直接设置 Ready 的 Version4 事实不能被解释为完整加载转移已经得到证明。

#### Entity/World 范围

作用域为 WorldSession 根，而不是网络连接或客户端用户。客户端可以有 readiness 的投影视图，但该视图不拥有权威 `Phase`；共享 `SessionReadinessView` 标记 `crossSubsystemOwner: integration-review`。

#### ID 与关系字段

本 Component 不拥有网络连接 ID、账户 ID、Entity ID 或持久化文件键。加载结果、生成屏障和活动世界身份之间的关系属于跨域生命周期关系，`crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

根 `D:\TRbackup\NLTX\src\WorldSession\WorldSessionComponents.cs:5` 有六值 `WorldPreparationState`；`D:\TRbackup\NLTX\src\WorldSession\WorldGeneration/D:\TRbackup\NLTX\src\WorldSession\WorldGeneration\WorldPreparationState.cs:3-11` 有同名六值枚举。Version4 只有三值枚举且实际转移来源未闭合，因此当前映射为 `partial`，不能把已有枚举标作完成的 readiness Component。

#### 证据

`D:\TRbackup\Version4\Terraria\Main.cs:99-104,1763-1769,11400-11409` 确认可更新门控；`D:\TRbackup\Version4\Terraria\WorldGen.cs:4151,4165,4287-4290,6290-6292` 和 `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:721-807` 确认加载/生成/失败边界材料，但尚不足以锁定所有 Phase 转移 owner。

### 5.6 WorldTickSnapshot

#### 职责

保存某一已提交点的 WorldSession 只读值，供跨边界消费者获得一致的描述、规则、时钟、天气和 readiness 视图。它是快照 Component 候选，不是第二套权威状态，也不是包 7 的字段镜像。

```text
componentId: WS-COMP-06
name: WorldTickSnapshot
status: proposed
componentOwner: WorldSession candidate
crossSubsystemOwner: integration-review
entityScope: 活动 WorldSession 根的可选只读快照；每个提交点最多一份当前快照
lifecycle: 核心 Component 值提交后创建或替换；跨域消费者读取不可变值；卸载/失败时失效；不作为恢复源单独持久化
currentNltxStatus: partial
```

#### 字段

字段类型中的 `SnapshotValue` 是不可变值类型候选，不是本节之外的一级 Component；其最终公共类型和 owner 需由整合会话裁决，均为 `crossSubsystemOwner: integration-review`。

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
|---|---|---|---|---|---|---|
| `Revision` | `long` | `0` | 快照版本；`crossSubsystemOwner: integration-review` | 同一快照内所有值使用同一 revision；只增不减 | missing | Version4 无统一 revision；研究报告 `BD-WS-03` 记录为整合决策 |
| `IsCommitted` | `bool` | `false` | 快照生命周期标记；`crossSubsystemOwner: integration-review` | 未提交时不供跨域读取；失效快照不得继续作为权威输入 | missing | Version4 无该字段；由快照候选不变量提出 |
| `Descriptor` | `WorldDescriptorSnapshotValue?` | `null` | 不可变快照值；`crossSubsystemOwner: integration-review` | 只能由同一 WorldDescriptorState 值复制形成；不包含 Tile、Section、Entity 或 Network ID | partial | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1267-1318,2013-2132` |
| `Rules` | `WorldRulesSnapshotValue?` | `null` | 不可变快照值；`crossSubsystemOwner: integration-review` | 只反映已提交基础规则；Hardmode 事务完成边界未决 | partial / unresolved | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1278-1287,1319-1348,2038-2174`；`D:\TRbackup\Version4\Terraria\WorldGen.cs:26084-26125` |
| `Clock` | `WorldClockSnapshotValue?` | `null` | 不可变快照值；`crossSubsystemOwner: integration-review` | `DayTime`、`Time`、`MoonPhase` 必须同 revision；不包含客户端视觉时间 | partial | `D:\TRbackup\Version4\Terraria\Main.cs:594-600,12972-13509`；`D:\TRbackup\Version4\Terraria\NetMessage.cs:223-229` |
| `Weather` | `WorldWeatherSnapshotValue?` | `null` | 不可变快照值；`crossSubsystemOwner: integration-review` | 只读雨/风事实和被允许的缓存；不包含粒子、音乐、云 alpha | partial | `D:\TRbackup\Version4\Terraria\Main.cs:614-620,658-664,12050-12252,12820-12924` |
| `Readiness` | `SessionReadinessSnapshotValue?` | `null` | 不可变快照值；`crossSubsystemOwner: integration-review` | `CanUpdateEntities` 必须与 Phase/屏障一致；失败快照不能表示 Ready | partial / unresolved | `D:\TRbackup\Version4\Terraria\Main.cs:99-104,11400-11409`；`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:790-807` |

#### 字段不变量

- `WorldTickSnapshot` 的字段均为只读值副本；任何 Persistence、Network、客户端或相邻子系统都不能通过快照反写六个核心 Component。
- `IsCommitted=true` 只能在 Descriptor、Rules、Clock、Weather 和 Readiness 的值来源于同一提交点时成立；这一原子范围尚未由跨子系统整合裁决，`crossSubsystemOwner: integration-review`。
- 快照不得包含 Entity ID、Network ID、账户 ID、Section 位图、FileStream、异常对象或可变集合引用。
- 包 7 仅是一个跨域协议消费者的证据；快照不复制其字段顺序、位标志或网络兼容实现。

#### 生命周期

快照不先于权威 Component 值存在。一次提交后形成不可变值，消费者在其生命周期内只读取；下一次提交替换当前快照，旧快照失效但不能回写。世界加载失败、卸载或清理时，快照变为未提交状态，不作为恢复源单独保存。

#### Entity/World 范围

作用域为 WorldSession 根的快照 Component。它不是每个 Entity 的复制品，也不把 Section/实体集合包进世界快照。若最终整合决定采用跨域聚合快照，其 owner 必须保留 `crossSubsystemOwner: integration-review`。

#### ID 与关系字段

快照不拥有独立 World ID；它引用的 WorldDescriptor 值中的 `WorldId`/`UniqueId` 仍按身份分离规则处理，`crossSubsystemOwner: integration-review`。`Revision` 不是网络序号或持久化 ID；Network ID、Entity ID、Section ID 和文件版本不得复用它。

#### 当前 NLTX 映射

根 `src/WorldSession` 没有 `WorldTickSnapshot`。`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation/World/WorldRuntimeSnapshot.cs:5-59` 有包含时钟、规则和生成完成信息的不可变快照材料，但其范围较窄且根项目装配关系未编译确认，因此 `currentNltxStatus: partial`，不宣称目标 Component 已存在。

#### 证据

Version4 通过 `Main`、`WorldFile` 和 `NetMessage` 提供快照来源字段，但没有一个同名统一快照类型。快照字段形状、revision 和跨域 owner 是本设计候选，evidenceStatus 保持 `partial`/`missing`。

## 6. Entity 与 Component 组合

| Entity/World 对象 | 必需 Component | 可选 Component | 互斥 Component | 组合理由 |
|---|---|---|---|---|
| 活动 WorldSession 根 | `WorldDescriptorState`、`WorldRulesState`、`WorldClockState`、`WorldWeatherState`、`SessionReadinessState` | `WorldTickSnapshot` | 无；但不能同时挂载两个不同 revision 的有效快照 | 这五个 Component 表达不同 owner、生命周期和访问模式；快照只在提交点存在并保持不可变 |
| 等待加载/生成的 WorldSession 根 | `SessionReadinessState` | 部分填充的 Descriptor/Rules/Clock/Weather；未提交 `WorldTickSnapshot` | `WorldTickSnapshot.IsCommitted=true` | 允许记录准备状态，但未完成屏障不能伪装成可用世界 |
| 已加载且可用的 WorldSession 根 | 五个核心 Component | `WorldTickSnapshot(IsCommitted=true)` | `SessionReadinessState.CanUpdateEntities=false` 与已声明 Ready 的组合 | 完整描述、规则、时钟和天气必须与 readiness 一致；快照是只读副本 |
| 失败或卸载中的 WorldSession 根 | `SessionReadinessState` | 诊断所需的最小失败码；不可用快照 | `CanUpdateEntities=true`、`WorldTickSnapshot.IsCommitted=true` | 失败/卸载阶段保留门控事实，不保留可被实体消费的提交快照 |
| 客户端世界视图 | 无权威 WorldSession Component | 服务端提交的 `WorldTickSnapshot` 的只读副本、表现缓存 | 客户端视图反向写入五个权威 Component | tModLoader 公开 NetReceive 边界和 Version4 包 7 证据支持客户端消费边界；客户端 owner 仍不是权威 WorldSession，`crossSubsystemOwner: integration-review` |

该组合表不定义任何执行顺序、调度阶段或网络流程；它只规定 Component 共存、可选性和互斥不变量。

## 7. 组件拆分与合并决策

| 决策 ID | 组件边界 | 决策 | 理由 | 状态 |
|---|---|---|---|---|
| `BD-COMP-SPLIT-01` | Descriptor 与 Rules | 拆分 | 身份/几何的生命周期和规则事实的变更原因不同；部分消费者只需描述而不需规则 | candidate |
| `BD-COMP-SPLIT-02` | Clock 与 Weather | 拆分 | 天气有雨/风计时和高频缓存，时钟有昼夜/月相不变量；两者在 Version4 的字段和保存恢复边界不同 | candidate |
| `BD-COMP-SPLIT-03` | Weather 事实与云/粒子表现 | 拆分 | 权威天气与表现缓存的所有权、失效条件、持久化和客户端容错不同 | candidate |
| `BD-COMP-SPLIT-04` | Readiness 与五个世界事实 Component | 拆分 | readiness 是生命周期门控；世界已加载但天气/时钟仍可改变，不能把“可更新”混成规则字段 | candidate |
| `BD-COMP-SPLIT-05` | 核心 Component 与 WorldTickSnapshot | 拆分 | 快照是不可变输出值，不应与权威字段形成双向镜像；快照 revision 和失效条件独立 | candidate；`crossSubsystemOwner: integration-review` |
| `BD-COMP-MERGE-01` | `WorldId` 与 `UniqueId` | 不合并成一个字符串 ID | Version4 同时持有整数 WorldId 和 GUID；持久化、地图名和网络边界用途不同，必须保持 ID 分离 | candidate；`crossSubsystemOwner: integration-review` |
| `BD-COMP-MERGE-02` | 时钟的 `DayTime`、`Time`、`MoonPhase` | 保持同一 Component | 它们共同表达世界时钟事实、共同恢复并共享昼夜/月相不变量；拆开会制造镜像同步 | candidate |
| `BD-COMP-MERGE-03` | 雨的 `IsRaining`、`RainTime`、`MaximumRainStrength` | 保持同一 Component | 三者共同表达雨状态，Version4 的启动/停止/计时边界共同读写；风缓存不因此并入同一不变量 | candidate |
| `BD-COMP-MERGE-04` | `WorldBounds` 与 Descriptor | 作为值字段合并，不单独建一级 Component | 边界没有独立生命周期，且与尺寸/锚点共同描述同一世界；它不承载行为或结构内容 | candidate |
| `BD-COMP-REVIEW-01` | `HardMode` | 字段暂留 Rules，事务 owner 不在本文件锁定 | 包 7/世界头需要读取 Hardmode，但 `StartHardmode` 还拥有后台转换和锁；将事务塞回 Rules 会形成巨型边界 | decision-required；`crossSubsystemOwner: integration-review` |
| `BD-COMP-REVIEW-02` | 事件/进度字段 | 不并入基础 WorldSession Component | Blood Moon、Eclipse、NPC downed、入侵和 Slime Rain 由多个类型读写，生命周期和 owner 跨域 | decision-required；`crossSubsystemOwner: integration-review` |
| `BD-COMP-REVIEW-03` | 当前 `WorldSessionComponents.cs` | 不作为长期巨型 Component | 当前文件将 readiness、描述、规则、时间/天气、事件进度和刷怪压力集中在一起；存在不同命名空间的重复类型，语义边界未闭合 | candidate；currentNltxStatus: partial |

## 8. 不单独创建 Component 的对象

| 对象 | 不单独创建 Component 的理由 | 归类 |
|---|---|---|
| 单个 `Main` 静态字段 | 单字段没有独立生命周期、不变量和替换边界；机械拆分会产生重复镜像 | 归入五个核心 Component 的字段 |
| `WorldBounds` | 只表达描述的一组几何值，没有独立世界生命周期 | Descriptor 的值字段 |
| `cloudAlpha`、`numClouds`、`numCloudsTemp`、`cloudBGActive` | 表现输入、缓存或协议兼容值；不能成为雨/风权威事实 | 天气表现/缓存边界；不属于核心 Component |
| 单个雨滴、云、天气粒子、音乐和镜头状态 | 是天气事实的表现实例，生命周期高频且可丢失 | 表现数据；不属于核心 Component |
| 单个事件类型 `BloodMoon`、`Eclipse` 或 `SlimeRain` | 事件实例/资格只是跨域状态的一部分，不拥有世界身份、时钟和 readiness | 跨域日历/进度状态；`crossSubsystemOwner: integration-review` |
| `NPC.downed*`、saved NPC、入侵和解锁旗标 | 多个领域共同读取/写入并在包 7/世界头中混合序列化 | 跨域进度字段；`crossSubsystemOwner: integration-review` |
| Hardmode 转换事务 | 它包含后台转换、I/O 锁、区段重置和主线程后续处理，不能退化为 Component 布尔值 | 跨域事务状态；owner `crossSubsystemOwner: integration-review` |
| `WorldFile._temp*` | 保存事务内的暂存和恢复工作值，不是运行时长期权威；失败时还有回滚/备份语义 | 兼容 staging；不属于核心 Component |
| `WorldFileData.LoadException` | 异常对象是外部诊断，不是可被模拟共同修改的世界事实 | 稳定失败码/诊断边界；不进入 Component |
| 包 7 的 wire layout | 字段混合多个子系统，位标志和顺序属于协议兼容；复制布局会把外部协议渗入核心 | 快照消费的兼容边界；`crossSubsystemOwner: integration-review` |
| `WorldSpawnPressureState` 当前记录 | 它是刷怪压力和实体统计缓存，owner/更新频率属于相邻实体生成领域 | 当前 NLTX partial 材料；不属于本 Component 清单 |
| Entity ID、Network ID、持久化 ID、WorldSectionId、账户 ID | 身份生命周期和消费者不同，合并会造成错误的关系解析和权限边界 | 分离的跨域 ID 候选；`crossSubsystemOwner: integration-review` |

## 9. 当前 NLTX 组件覆盖

| proposed Component | 当前 NLTX 材料 | 当前状态 | 已覆盖内容 | 尚未闭合 |
|---|---|---|---|---|
| `WorldDescriptorState` | `D:\TRbackup\NLTX\src\WorldSession\WorldSessionComponents.cs:6-27`；`D:\TRbackup\NLTX\src\WorldSession\WorldGeneration/D:\TRbackup\NLTX\src\WorldSession\WorldGeneration\WorldDescriptorState.cs:3-25` | partial | 名称、种子、版本、尺寸、边界、地层、出生/地牢锚点记录 | 同名类型/命名空间重复、唯一 owner、加载提交、编译接线、跨域 ID owner |
| `WorldRulesState` | `D:\TRbackup\NLTX\src\WorldSession\WorldSessionComponents.cs:38-56`；`D:\TRbackup\NLTX\src\WorldSession\WorldGeneration/WorldRulesState.cs:3-18` | partial | GameMode、HardMode、秘密种子、邪恶类型、OreTier 和部分派生属性材料 | Hardmode 事务边界、override 所有权、两套类型合并、唯一写入边界 |
| `WorldClockState` | 根 `WorldTimeWeatherState` 的时钟字段；`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation/World/WorldClock.cs:5-180` | partial | 时钟字段、暂停/恢复和相位约束材料 | 根项目独立 Component、Version4 槽位兼容、跨域 WorldTime owner、revision |
| `WorldWeatherState` | 根 `WorldTimeWeatherState` 的雨/风字段；`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation/World/WorldRuleState.cs:5-237` | partial | 雨/风字段、不变量和确定性计算材料 | 云/天气缓存归属、Version4 随机与边界、唯一写入边界、表现隔离 |
| `SessionReadinessState` | 根 readiness 枚举；`D:\TRbackup\NLTX\src\WorldSession\WorldGeneration/D:\TRbackup\NLTX\src\WorldSession\WorldGeneration\WorldPreparationState.cs:3-11`；`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Server/Startup/ServerHostState.cs:6-120` | partial | 阶段枚举和服务器 host readiness 材料 | WorldSession 生命周期转移、生成/加载屏障、失败码、根项目接线；ServerHostState 不等于 WorldSession Component |
| `WorldTickSnapshot` | 根项目无同名类型；`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation/World/WorldRuntimeSnapshot.cs:5-59` | partial | 不可变 clock/rules/generation snapshot 材料 | 完整 descriptor/weather/readiness 范围、revision、跨域 owner、提交一致性 |

当前 NLTX 没有证据证明这些状态记录已经形成一套可运行的、Version4 顺序兼容的唯一 Component 集合；因此全局 `nltxStatus: partial` 保持不变。本节只记录材料覆盖，不声明代码已创建、已迁移或行为等价。

## 10. 组件级 evidence-gap

以下 11 项是仍未解决的组件级 evidence-gap；普通缺口继续以候选设计交付，不静默补造事实。

| gap ID | 影响 Component | 缺口 | 状态 | 对设计的影响 |
|---|---|---|---|---|
| `GAP-COMP-01` | `WorldDescriptorState` | 边界严格关系、尺寸/锚点合法性和共享 descriptor value owner 未由一套独立证据锁定 | partial | 字段保留，但几何不变量与共享 owner 不能宣布 baseline；`crossSubsystemOwner: integration-review` |
| `GAP-COMP-02` | `WorldRulesState` | Hardmode 字段与后台转换事务的最终 owner 未决 | unresolved | HardMode 只能是规则快照候选，不能声明普通字段写入完成；`crossSubsystemOwner: integration-review` |
| `GAP-COMP-03` | `WorldRulesState` | Version4 种子选项/秘密种子解析存在空体，完整参考虽有实现但不是 Version4 当前行为 | partial | SecretSeedFlags 只能作为候选映射，不能静默采用完整参考算法 |
| `GAP-COMP-04` | `WorldClockState` | Version4 没有统一 revision；时间精确范围、快进和客户端视觉边界尚未独立回放验证 | partial / missing | `ClockRevision` 保持可选候选；不能声明快照版本契约已锁定 |
| `GAP-COMP-05` | `WorldWeatherState` | weather/wind counter 是否持久化、失效和唯一 owner 未完全确认；雨强 override 全范围未闭合 | partial | 计数器只作缓存候选；不把 dome 的限制移植为 Version4 事实 |
| `GAP-COMP-06` | `SessionReadinessState` | 加载、生成、失败、卸载全部 Phase 转移来源未闭合；Version4 每帧设 Ready 与屏障关系需整合 | unresolved | `Failed`/`Unloading` 只能是候选扩展；`CanUpdateEntities` 必须保持安全门控 |
| `GAP-COMP-07` | `WorldRulesState` / `WorldWeatherState` | Creative/Journey 的时间、天气、难度和生态有效覆写 owner 未统一 | partial | 基础事实不能被覆写值回写；共享有效视图 `crossSubsystemOwner: integration-review` |
| `GAP-COMP-08` | `WorldRulesState` / `WorldClockState` / `WorldWeatherState` | Blood Moon、Eclipse、Pumpkin/Snow Moon、Slime Rain、入侵和进度字段的跨域 owner 未闭合 | partial | 不将事件/进度并入基础 Component；`crossSubsystemOwner: integration-review` |
| `GAP-COMP-09` | `WorldTickSnapshot` | 快照是否仅含 WorldSession 自有值、是否聚合跨域只读值、revision 及原子提交范围未决 | missing / unresolved | 快照保持候选、不可变和跨域 owner，不能成为最终公共类型 |
| `GAP-COMP-10` | `WorldTickSnapshot` | 包 7 的服务器编码已确认，但 `D:\TRbackup\Version4\Terraria\MessageBuffer.cs:385-386` 的客户端 case 7 未闭合客户端恢复路径 | unresolved | 只能把包 7 当快照消费证据，不能确认客户端恢复写入边界 |
| `GAP-COMP-11` | 全部 Component | 实际 `Terraria.IO\WorldFile.cs` 与任务给定路径不一致；根 `src/WorldSession` 与 `dome` 的运行时装配未编译验证 | evidence-mismatch / unresolved | 记录实际路径；所有 NLTX 映射保持 partial，不能声称项目闭合 |

## 11. 未决组件 owner

以下 6 项必须在最终整合中裁决。每项都说明冲突字段、候选 owner、Component 组成影响和当前不能定案的原因。

### `BD-COMP-01`：WorldDescriptor 与共享世界身份

- 冲突字段：`WorldId`、`UniqueId`、World 尺寸/边界/锚点，以及 `WorldDescriptorView`/`WorldSectionId` 关系；`crossSubsystemOwner: integration-review`。
- 当前候选 owner：`WorldSession` 持有 `WorldDescriptorState`；WorldStorage、WorldGeneration、Persistence、Network 和空间查询消费只读值。
- 组成影响：若 WorldSession 独占，Descriptor 保持一个 World 级 Component；若由跨域整合层维护共享值，需要额外的不可变 descriptor value，但仍不能把 Section 或 Tile 放入 Descriptor。
- 不能定案原因：Version4 的世界头、WorldFileData、WorldGen、网络和地图边界共同使用身份；单一子系统不能宣布共享 ID/关系 owner。

### `BD-COMP-02`：HardMode 字段与转换事务

- 冲突字段：`Main.hardMode`、World header 中的 Hardmode 位、`WorldGen.StartHardmode` 及转换完成状态；`crossSubsystemOwner: integration-review`。
- 当前候选 owner：`WorldRulesState` 保存已提交 HardMode 快照；WorldProgressionAndTransition 持有转换事务候选。
- 组成影响：方案一把 HardMode 和转换状态放同一 Component，边界会吸收后台转换/区段重置；方案二只保留快照字段并把事务状态放相邻领域，Component 更小但需要共享值；方案三建立跨域转换聚合，新增共享 owner。
- 不能定案原因：Version4 在后台转换开始前就写 `Main.hardMode=true`，而保存、WorldGen 和网络又依赖不同可见点；需要整合裁决提交边界。

### `BD-COMP-03`：日历/事件/进度字段

- 冲突字段：`bloodMoon`、`eclipse`、`pumpkinMoon`、`snowMoon`、`slimeRain`、`NPC.downed*`、invasion、Lantern Night/Birthday Party/Sandstorm 相关世界字段；`crossSubsystemOwner: integration-review`。
- 当前候选 owner：WorldSession 只提供 Clock/Weather/Rules 的只读输入；WorldCalendarAndEventOrchestration 或 WorldProgressionAndTransition 各自持有事件/进度 Component 候选。
- 组成影响：若并入 WorldRules/Weather，会形成跨域巨型 Component 和重复写入；若全部拆到相邻 Component，`WorldTickSnapshot` 需要多个只读值组合；若采用跨域事件快照，公共类型范围扩大。
- 不能定案原因：这些字段同时被 `Main`、NPC、WorldGen、WorldFile 和包 7 读取/写入，当前会话不能替相邻固定子系统宣布最终 owner。

### `BD-COMP-04`：SessionReadiness 的转移 owner

- 冲突字段：`WorldPreparationState`、`_worldPreparationState`、`generatingWorld`、`isGeneratingOrLoadingWorld`、`loadFailed`、`LoadStatus`；`crossSubsystemOwner: integration-review`。
- 当前候选 owner：WorldSession 持有 Phase 和 `CanUpdateEntities`；WorldStorage/WorldGeneration 提供加载/生成结果；RuntimeComposition 只消费并编排。
- 组成影响：若加载边界直接持有 readiness，SessionReadiness 可能只剩镜像；若 WorldSession 独占所有转移，组件会渗入存储/生成失败细节；若保留最小 phase，需一个跨域失败码和值对象。
- 不能定案原因：Version4 `UpdateWorldPreparationState` 当前每帧直接设 Ready，但完整加载/生成/卸载路径未闭合；不能以方法现状推导完整生命周期 owner。

### `BD-COMP-05`：基础时钟/天气与共享有效视图

- 冲突字段：`WorldTimeView`、`WorldWeatherView`、天气缓存、Creative/Journey 有效覆写，以及服务端/客户端读取边界；`crossSubsystemOwner: integration-review`。
- 当前候选 owner：WorldSession 分别持有 `WorldClockState` 和 `WorldWeatherState`；SimulationRuleOverrides 提供有效覆写；NPC、WorldGen、Calendar、Persistence 和 Network 消费只读值。
- 组成影响：若时钟/天气合并，字段不变量和缓存失效条件变粗；若全部分开，跨域读取需要明确一致的只读值；若客户端直接持有副本，必须保证副本不回写权威 Component。
- 不能定案原因：Version4 在不同调用邻域更新风/云、雨计时和时间；共享有效视图与覆写 owner 尚未由整合会话锁定。

### `BD-COMP-06`：WorldTickSnapshot 与跨域提交范围

- 冲突字段：`Revision`、Descriptor/Rules/Clock/Weather/Readiness snapshot values，以及是否包含事件/进度只读值；`crossSubsystemOwner: integration-review`。
- 当前候选 owner：WorldSession 维护只含自身五类值的快照；另一候选是由最终整合层维护跨域聚合快照；第三候选是多个独立只读快照而不设总快照。
- 组成影响：WorldSession-only 方案组件小但下游需组合；跨域聚合方案读取简单但扩大公共类型和一致性范围；多快照方案降低耦合但需要共同 revision 约定。
- 不能定案原因：Version4 包 7 和 WorldFile header 都是跨域混合数据，而客户端 decode 路径又未闭合；本会话只能提出不可变快照候选，不能宣布最终公共 owner。

所有上述事项的未决标记均为 `crossSubsystemOwner: integration-review`；本文件不把候选 owner 写成最终架构事实。

## 12. 最终 Component 清单

| componentId | Component | status | componentOwner | entityScope | currentNltxStatus | 关键未决项 |
|---|---|---|---|---|---|---|
| `WS-COMP-01` | `WorldDescriptorState` | proposed | WorldSession candidate | 单个活动 WorldSession 根，World 级 | partial | 共享世界身份、几何不变量、descriptor view owner；`crossSubsystemOwner: integration-review` |
| `WS-COMP-02` | `WorldRulesState` | proposed | WorldSession candidate | 单个活动 WorldSession 根，World 级 | partial | HardMode 事务、秘密种子补证、规则覆写、OreTier owner；`crossSubsystemOwner: integration-review` |
| `WS-COMP-03` | `WorldClockState` | proposed | WorldSession candidate | 单个活动 WorldSession 根，World 级 | partial | revision、时间边界回放、共享 WorldTime owner；`crossSubsystemOwner: integration-review` |
| `WS-COMP-04` | `WorldWeatherState` | proposed | WorldSession candidate | 单个活动 WorldSession 根，World 级 | partial | 缓存失效、雨强范围、有效覆写和共享 Weather view owner；`crossSubsystemOwner: integration-review` |
| `WS-COMP-05` | `SessionReadinessState` | proposed | WorldSession candidate | 单个活动 WorldSession 根，会话级 | partial | Failed/Unloading 转移、生成屏障来源、失败码 owner；`crossSubsystemOwner: integration-review` |
| `WS-COMP-06` | `WorldTickSnapshot` | proposed | WorldSession candidate | 单个活动 WorldSession 根，可选快照 | partial | 快照范围、revision、提交原子性和跨域 owner；`crossSubsystemOwner: integration-review` |

Component 数量：`6`。六个 Component 均为设计提案，均使用 `status: proposed`；不存在 `status: existing` 的已闭合 WorldSession Component 声明。

## 13. 最终声明

本文件是 Component-only Design。

本文件不定义 System、Query、Command、Event、Adapter、Projection、调度顺序、运行时实现、测试计划或迁移计划。

所有 `status: proposed` 的 Component 都只是设计提案。

本文件不声明代码已创建、已迁移、行为等价或验证通过。

`verificationStatus: not-run`。本文件只写入自动推导的设计路径；未修改 Version4、完整参考源码、tModLoader 文档、Space Station 14、NLTX 源码、测试、dome 源码或原研究报告，未运行构建或测试。




