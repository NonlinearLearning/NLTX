# 世界生成、Biome 与生态：世界会话及进度公共拆分审计报告

> 范围：本文落实《[Version4 权威游戏模拟系统主要子系统](Version4权威游戏模拟系统主要子系统.md)》中第 2 项“世界会话与进度”和第 13 项“世界生成、Biome 与生态”的交界。它是对真实 Version4 源码的只读审计及目标组件拆分，不是“迁移已经完成”的声明。
>
> 方法：按 `public-decomposition` 的成员盘点、权威性标记、访问模式分组、组件/System/Query/Adapter/Projection 边界及 focused verifier 输出要求执行。`C:\Users\shan\Downloads\ECS\space-station-14-master` 仅用来学习 ECS 组织粒度；本文不复制它的代码、命名或游戏语义。

## 1. 结论

`Terraria.Main`、`Terraria.NPC`、`Terraria.WorldGen` 和若干事件静态类把四种生命周期不同的状态放在了一起：

- 世界身份、尺寸、种子、地层和规则；
- 可存档的 Boss、入侵、NPC 解锁和世界改造进度；
- 每 Tick 变化的昼夜、天气、入侵运行态、生态采样游标和刷怪压力；
- 仅在加载/生成期间存在的阶段、生成 pass、进度条和房屋扫描 scratch 数据。

它们不能归入一个“世界状态”巨型组件。建议一个 `WorldSession` 根持有 **7 个内聚状态组件**，并让世界生成、Biome、生态、住房和事件以 System/Query 读取这些组件。Tile、Wall、液体、实体槽位、房屋占用关系和网络连接均不属于会话组件。

```text
WorldSession (一世界一根；持久化世界 ID，而非网络 ID/实体槽位)
├─ WorldDescriptorState              身份、边界、地层、出生点、地牢锚点
├─ WorldRulesState                   模式、Hardmode、种子旗标、世界邪恶、矿脉规则
├─ WorldClockWeatherState            昼夜、时间、月相、雨、风及其可持久化运行态
├─ WorldEventProgressState           Boss/入侵/NPC 解锁/四柱/DD2 的长期与当前进度
├─ WorldAlterationProgressState      暗影球、祭坛、流星、待处理世界改造事件
├─ WorldEcologyScheduleState         生态传播许可、采样游标、生态 Tick/刷怪调度
└─ WorldGenerationLifecycleState     Loading/Generating/Ready/Failed；非存档 pass 生命周期

WorldStorage（并列，不是子组件）
├─ TileMap / WallMap / LiquidMap
├─ TileEntity、Chest、TownHousingRegistry
└─ Player/NPC/Projectile/WorldItem 槽位
```

`BiomeRuleQuery`、`SceneSensingQuery`、`TownHousingQuery` 不保存权威会话状态；它们根据 TileMap、规则和位置计算结果。`GenerationProgress`、云/雨视觉 alpha、UI 入侵条淡入、摄像机场景度量、网络包和 WorldFile 二进制字段顺序都是投影、缓存或适配器细节，禁止回流为会话权威字段。

## 2. 审计范围与证据状态

### 2.1 真实代码根与检索边界

用户给出的 `D:\TRbackup\Version4参考` 不存在；实际只读代码根为 `D:\TRbackup\Version4`。下列集合构成“世界会话与进度”的完整**直接拥有者/写入链/边界适配器**范围；大量 `Main.*`/`NPC.*` 命中文件（NPC AI、掉落、Chest、Player、UI）是消费者，不应被误拆为会话拥有者。

| 角色 | 真实代码 | 已确认职责 |
| --- | --- | --- |
| 会话字段与 Tick 写者 | `Terraria/Main.cs:148-176,495-624,652-674,1059-1081,12050-13647` | 种子旗标、规则、时钟天气、入侵、事件推进 |
| 进度字段与击杀写者 | `Terraria/NPC.cs:6201-6291,65237-65267,65693-65964` | Boss/入侵/塔/NPC 解锁旗标，以及击杀后的幂等式设置入口 |
| 生成、生态、住房 scratch | `Terraria/WorldGen.cs:4113-4290,59400-62723` | 世界邪恶、改造进度、生成门控、生态传播、采样和住房检查 |
| 生成工作流 | `Terraria/WorldGen.cs:6285-6305,6387-6524,10108-10147`，`Terraria.WorldBuilding/WorldGenerator.cs:262-333` | 创建/清理/生成、有序 pass、控制器与 finally 清理 |
| 持久化边界 | `Terraria.IO/WorldFile.cs:658-760,878-980,1263-1454,1808-2190` | 版本化 header 的保存、加载、旧版本修复及临时保存快照 |
| 网络边界 | `Terraria/NetMessage.cs:221-403`，`Terraria/MessageBuffer.cs:2114-2247,2498-2508` | 包 7 世界快照、包 78 入侵进度，以及入站事件请求 |
| 事件子域 | `Terraria.GameContent.Events/BirthdayParty.cs:11-23,155-166`、`LanternNight.cs:6-18,80-98`、`Sandstorm.cs:14-123`、`DD2Event.cs:45-91` | 各事件仍有独立静态运行态；迁移前需逐个收口 |
| 住房持久关系 | `Terraria.GameContent/TownRoomManager.cs:9-15,65-142` | 住房占用、保存/加载/清理；不是世界进度旗标 |

状态标记含义：`confirmed` 表示已核对声明、至少一个写入链及持久化或网络边界；`partial` 表示声明和部分读写已确认，但实际 Version4 实现有空体或链路未闭合；`missing` 表示不能据此设计权威字段。

对以下字段集合执行递归源码搜索，得到 31 个直接命中 C# 文件：`Main.(hardMode|dayTime|time|moonPhase|raining|rainTime|maxRaining|windSpeedCurrent|windSpeedTarget|bloodMoon|pumpkinMoon|snowMoon|eclipse|slimeRain|slimeRainTime|invasion*)` 与 `NPC.(downed*|saved*|unlocked*)`。这是“世界会话/进度字段集”的全量命中，不声称是整个 Version4 仓库中一切间接相关代码的全集：

```text
拥有者/调度：Terraria/Main.cs, Terraria/NPC.cs, Terraria/WorldGen.cs
适配器：Terraria.IO/WorldFile.cs, Terraria/NetMessage.cs,
         Terraria/MessageBuffer.cs, Terraria/Netplay.cs
事件状态：Terraria.GameContent.Events/CultistRitual.cs,
         DangerousDungeonCurse.cs, DD2Event.cs, LanternNight.cs, Sandstorm.cs
玩法消费者：Terraria/Player.cs, Projectile.cs, Item.cs, WorldItem.cs, Chest.cs,
           Mount.cs, Wiring.cs, DelegateMethods.cs, Utils.cs
表现/工具：Terraria/Cloud.cs, Rain.cs, Lang.cs,
           Terraria.GameContent.Ambience/AmbienceServer.cs,
           Terraria.GameContent.LootSimulation/SimulatorInfo.cs
规则消费者：Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs,
             Terraria.GameContent.ItemDropRules/CommonCode.cs, Conditions.cs,
             MechBossSpawnersDropRule.cs,
             Terraria.GameContent/CoinLossRevengeSystem.cs,
             Terraria.GameContent.Tile_Entities/TELogicSensor.cs
```

字段模式不会命中但本报告已纳入审计的关键补集是 `Terraria.IO/WorldFileData.cs`、`Terraria/SceneMetrics.cs`、`Terraria.GameContent.Events/BirthdayParty.cs`、`Terraria.GameContent/TownRoomManager.cs`、`Terraria.WorldBuilding/{WorldGenerator,GenerationProgress,GenPass,Passes,WorldGenConfiguration,GenVars}.cs`、`Terraria.GameContent.Biomes/**` 与 `Terraria.GameContent.Generation/**`。它们分别解释世界元数据、派生环境扫描、事件状态、住房关系或世界生成的执行模型。

### 2.2 SS14 的组织证据（仅模式，不是领域来源）

| SS14 只读证据 | 可迁移的组织结论 |
| --- | --- |
| `Content.Shared/GameTicking/SharedGameTicker.cs:14-60` | 回合 ID/开始时间是可读状态，时长是可重复计算的派生值。会话的 `IsReady`、难度等也应提供只读 View，而不是复制到消费者。 |
| `Content.Server/GameTicking/GameTicker.cs:32-135` | 生命周期由 System 的 Initialize/Update/Shutdown 编排，状态组件不持有服务或网络句柄。 |
| `Content.Server/GameTicking/GameTicker.GameRule.cs:26-40,88-238` 及 `Content.Shared/GameTicking/Components/GameRuleComponent.cs:12-37` | 事件创建、延迟启动、开始和结束由显式 System/事件处理；运行事件与长期完成旗标不是同一个字段集合。 |
| `Content.Shared/Parallax/Biomes/BiomeComponent.cs:9-81` 与 `SharedBiomeSystem.cs:68-147` | 地图附着的 Seed/Layers/已加载区块是地图状态；坐标到 Tile 的选择可由 Query 计算。对 Version4 而言，Biome 规则不能被塞入玩家或 NPC。 |
| `Content.Server/Parallax/BiomeSystem.cs:80-178` | 初始化、seed/template 写入、配置和标脏在 System；临时 active-chunk 集合是 System 工作集，不等同于持久化游戏进度。 |

### 2.3 本地 tModLoader API 来源日志

本节遵循 `public-decomposition/references/tmodloader-documentation-retrieval.md`。该 API 镜像只能说明公开成员的已文档化语义；Version4 本地源码才是本报告关于实际写者、二进制持久化、网络包和调度顺序的依据。

| source | version | query / hits | evidence | gaps | stop reason |
| --- | --- | --- | --- | --- | --- |
| `D:\TRbackup\tmodloader-api-docs-stable` | `tModLoader v2026.07`；`index.html` 标题为 `tModLoader: Main Page` | `Main.dayTime`、`Main.time`；`classes.html` 的 `Main -> class_main.html` 唯一命中 | `class_main.html#aa5b5ad648e5affabcd4429c7d67f9791` 说明 `dayTime` 与 `time` 一起代表世界时间；`class_main.html#a286e04821e5e48abf3d683972a668976` 说明 `time` 每 Tick 以 `dayRate` 增加且受昼/夜上限约束 | 不说明 Version4 私有 Tick 顺序 | 获取公开语义；写者回退至 Version4 `Main.cs` |
| 同上 | `v2026.07` | `Main.hardMode`；类型页精确成员唯一命中 | `class_main.html#a99f898cc8face9be1a31a18e4e48409a`：世界进入 Hardmode，且与 Wall of Flesh 击败有关 | 不说明保存/网络格式 | 获取公开语义；格式回退至 `WorldFile.cs`/`NetMessage.cs` |
| 同上 | `v2026.07` | `NPC.downedBoss1/2/3`、`downedGoblins`；`NPC -> class_n_p_c.html` 唯一成员命中 | `class_n_p_c.html#ad25f939434ba648f28a26d6d53eca16b`、`#ad33d16816ea63e8ce25691078bcba793`、`#a0be5d55e22116b2c8ea68f78942fd4ce`、`#ab7eca7cdb38140d2f5f6221ffc1affc1` 均明确为“当前世界至少一次”完成状态 | 部分特殊事件旗标未逐个有摘要 | 确认世界级而非 NPC 实例级语义 |
| 同上 | `v2026.07` | `WorldGen.crimson`、`shadowOrbSmashed`、`altarCount`、`generatingWorld`；`WorldGen -> class_world_gen.html` | `class_world_gen.html#a8abf0ca117f4635fa2b505c6981197d8` 将 `crimson` 定义为所选世界邪恶；`#a2a77ba7e45224574806d7447c54b1e77`、`#afcc56c4c5d715ea8c65af87a5b219686`、`#a3f232441bc1384ad75317840a6f6bf66` 确认字段存在和类型 | `generatingWorld` 无公开生命周期摘要；不证明 Version4 的并发行为 | 静态公开成员确认后，回退 Version4 生成入口/`finally` |

**证据边界结论：** 上述公开文档支持“世界级时间、难度、完成旗标和世界邪恶”的字段语义；不支持把 `Main`/`WorldGen` 的静态布局、`BinaryReader` 顺序、包号或后台任务实现原样搬入目标架构。这些事实均以 Version4 行号为准。

## 3. 成员归属表

### 3.1 会话根组件

| 成员簇 | 现有字段/代码 | 生命周期、读者/写者 | 状态种类 | 目标归属 | 证据 |
| --- | --- | --- | --- | --- | --- |
| 身份、种子、版本、边界、尺寸 | `ActiveWorldFileData`、`worldName`、`left/right/top/bottomWorld`、`maxTilesX/Y` | 创建/加载至卸载；WorldFile、网络、碰撞、区段 | authoritative | `WorldDescriptorState` | `Main.cs:514-528,585-589`；`WorldFile.cs:1267-1278,2013-2035`；confirmed |
| 出生点、地层、地牢锚点 | `spawnTileX/Y`、`worldSurface`、`rockLayer`、`dungeonX/Y` | 生成/加载写入；出生、刷怪、Biome/生态和网络读取 | authoritative | `WorldDescriptorState` | `Main.cs:565-589`；`WorldFile.cs:1308-1318,2121-2133`；confirmed |
| 模式、难度和特殊种子 | `GameMode`、`hardMode`、`drunkWorld`…`dualDungeonsSeed` | 创建/加载后长期有效；生成、刷怪、事件、掉落读取 | authoritative | `WorldRulesState` | `Main.cs:148-176,495,1318-1367`；`WorldFile.cs:1278-1287,1342,2038-2078,2159`；confirmed |
| 世界邪恶、矿脉和传播规则 | `WorldGen.crimson`、`SavedOreTiers`、`AllowedToSpreadInfections` | 生成/存档或生态 Tick；Biome/传播/规则读取 | authoritative | `WorldRulesState`（长期规则）+ `WorldEcologyScheduleState`（Tick gate） | `WorldGen.cs:4113-4115,4185`；`WorldFile.cs:1319,1353-1355`；partial |
| 暗影球、祭坛、流星和待办世界变化 | `shadowOrbSmashed/count`、`altarCount`、`spawnMeteor`、`spawnEye`、`spawnHardBoss` | 长期进度或一次性事件待办；WorldGen/击杀/存档读取写入 | authoritative | `WorldAlterationProgressState` | `WorldGen.cs:4147-4163`；`WorldFile.cs:1338-1341,2155-2158`；confirmed/partial |
| 昼夜、时间和月相 | `dayTime`、`time`、`moonPhase` | 每 Tick；`UpdateTime` 写入，刷怪/事件/网络/存档读取 | authoritative | `WorldClockWeatherState` | `Main.cs:594-600,12972-13647`；`WorldFile.cs:1312-1316,2125-2129`；confirmed |
| 雨与风 | `raining`、`rainTime`、`maxRaining`、`windSpeedTarget/current`、counters | 每 Tick 或天气命令；生态、Sandstorm、表现和网络读取 | target/timer authoritative；视觉插值 derived | `WorldClockWeatherState` | `Main.cs:612-624,652-668,12050-12171,12820-12923`；confirmed/partial |
| 月/天气事件 | `bloodMoon`、`eclipse`、`pumpkinMoon`、`snowMoon`、slime rain | 昼夜事件期间；NPC 生成、网络、存档读取 | authoritative | `WorldClockWeatherState.EventRuntime` | `Main.cs:538-550,606-624,12925-13537`；`NetMessage.cs:225-228,303-308`；confirmed |
| Boss、NPC 解锁、入侵完成旗标 | `NPC.downed*`、`saved*`、`unlocked*` | Boss/NPC 生命周期写入；掉落、刷怪、规则、存档/网络读取 | authoritative | `WorldEventProgressState` 的 `BossProgress`、`NpcUnlockProgress`、`InvasionCompletion` 值对象 | `NPC.cs:6201-6291,65237-65267,65693-65964`；`WorldFile.cs:1320-1337`；confirmed |
| 当前入侵 | `invasionType/X/Size/Delay/Warn/SizeStart` | `StartInvasion`、`UpdateInvasion`、NPC 击杀写入；包 7/78 和存档读取 | authoritative | `WorldEventProgressState.InvasionRuntime` | `Main.cs:1059-1081,12542-12712`；`NPC.cs:64791-64797`；`WorldFile.cs:1344-1347`；confirmed |
| 四柱、月球、DD2 | `downedTower*`、`TowerActive*`、shield、`LunarApocalypseIsUp`、`DD2Event.*` | 事件运行/结算；NPC、网络、存档读取 | 长期进度与运行态分离 | `WorldEventProgressState.Lunar` 与 `.Dd2` | `NPC.cs:6251-6283,65802-65820`；`NetMessage.cs:325-336,1359-1362`；partial |
| 生态采样与刷怪节奏 | `totalX/totalD`、`npcSpawnDelay/Period`、`prioritizedTownNPCType` | 运行期；`UpdateWorld` 及城镇/NPC 刷新写入 | authoritative runtime state | `WorldEcologyScheduleState` | `WorldGen.cs:4189-4193,59418-59448`；confirmed |
| 生成/加载阶段 | `generatingWorld`、`isGeneratingOrLoadingWorld`、`generatingWorldOnThisThread`、`_worldPreparationState` | 创建/加载/清理期间；`ShouldUpdateEntities` 是门控 | lifecycle state，非存档 | `WorldGenerationLifecycleState` | `WorldGen.cs:4151,4287-4290,6285-6305,10108-10147`；`Main.cs:11400-11409`；confirmed |

### 3.2 字段、行为和边界成员 ledger

本表是对上表成员簇的可执行审计展开。它特别区分“字段属于何处”与“当前方法应该迁到哪个行为边界”；同一行的 `Read By`/`Written By` 是已核对的主要路径，不暗示没有其他消费者。

| Member | Declaring type / visibility | Read By | Written By | Lifecycle / frequency | External dependencies / side effect | State kind | Evidence | Status | Candidate |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `worldName`、World ID/GUID、seed、尺寸与边界 | `Main` / `WorldFileData`，public static / public | WorldFile、包 7、碰撞和区段消费者 | 创建、`LoadHeader`、生成 | create -> load -> unload；加载后稳定 | 多读者；存档/网络输出 | authoritative | `Main.cs:514-528,585-589`; `WorldFile.cs:1267-1278,2013-2035` | confirmed | `WorldDescriptorState` + persistence/network projections |
| `spawnTile*`、`worldSurface`、`rockLayer`、`dungeon*` | `Main`，public static | 出生、生成、刷怪资格、网络 | world load/generation | create/load -> unload；通常不随 Tick 改变 | WorldGen 写入，其他系统只读 | authoritative | `WorldFile.cs:1308-1318,2121-2133` | confirmed | `WorldDescriptorState` |
| `GameMode`、`hardMode`、秘密种子旗标 | `Main`，public static/property | 生成、事件、掉落、NPC 刷怪 | create/load、规则命令 | create/load；长期 | 规则输入，不应由玩家/NPC直接改写 | authoritative | `Main.cs:148-176,495,1318-1367`; `WorldFile.cs:1278-1287,1342,2038-2078,2159`; local API `class_main.html#a99f898cc8face9be1a31a18e4e48409a` | confirmed | `WorldRulesState` + `IWorldRulesView` |
| `crimson`、ore tiers | `WorldGen`，public static / nested state | Biome/生态、生成和规则资格 | generation/load；生态读取 | create/load；ore tiers 长期 | `crimson` 与 seed/规则共同解释世界；ore tiers 为存档数据 | authoritative | `WorldGen.cs:4113-4115`; `WorldFile.cs:1319,1353-1355`; local API `class_world_gen.html#a8abf0ca117f4635fa2b505c6981197d8` | confirmed | `WorldRulesState` |
| `dayTime`、`time`、`moonPhase` | `Main`，public static | NPC、事件、掉落、包 7、WorldFile | `UpdateTime` | every tick；world lifetime | 时钟推进和昼夜边界事件 | authoritative | `Main.cs:594-600,12972-13647`; `WorldFile.cs:1312-1316,2125-2129`; local API anchors in 2.3 | confirmed | `WorldClockWeatherState`; `WorldClockSystem` |
| rain/wind 字段与 Slime Rain | `Main`，public static | 生态、Sandstorm、NPC、表现、包 7 | `UpdateWeather`、Start/Stop rain/slime rain | every tick / event lifetime | target/timer 为规则状态；插值与表现字段须分离 | authoritative + presentation | `Main.cs:538-550,606-624,652-668,12050-12171,12820-13014`; `NetMessage.cs:223-258` | partial | `WorldClockWeatherState`; presentation projection |
| `downed*`、`saved*`、`unlocked*` | `NPC`，public static | 掉落、刷怪、WorldGen、存档、包 7 | `SetEventFlagCleared`、NPC death/rescue path | world lifetime；不因 NPC despawn 消失 | 完成/解锁命令必须幂等 | authoritative | `NPC.cs:6201-6291,65237-65267,65693-65964`; `WorldFile.cs:1320-1337`; local API NPC anchors in 2.3 | confirmed | `WorldEventProgressState`; `ProgressCommitSystem` |
| tower、Lunar 与 DD2 | `NPC` 与事件静态类，public static | NPC、事件、网络、存档 | boss/event lifecycle | event lifetime + long-term completion | 当前运行态与永久完成态有不同清理规则 | authoritative | `NPC.cs:6251-6283,65802-65820`; `NetMessage.cs:325-336,1359-1362`; `DD2Event.cs:45-91` | partial | `WorldEventProgressState.Lunar` / `.Dd2`; event systems |
| `invasion*` 与 `ReportInvasionProgress` | `Main`，public static / public method | NPC death、UI/包 78、WorldFile | `StartInvasion`、`UpdateInvasion`、NPC death | each event tick | Size、progress、completion 必须在一个提交点原子变化 | authoritative + presentation | `Main.cs:1059-1081,11846-11854,12542-12712`; `NPC.cs:64791-64797`; `NetMessage.cs:384-392` | confirmed | `InvasionRuntimeState`; `InvasionSystem` + projection |
| `shadowOrb*`、`altarCount`、`spawnMeteor`、`spawnEye`、`spawnHardBoss` | `WorldGen`，public static | Boss/事件、生成、WorldFile | world interaction/boss/event paths | long-term or one-shot pending event | 长期改造进度与等待消费的命令不同 | authoritative | `WorldGen.cs:4147-4163`; `WorldFile.cs:1338-1341,2155-2158`; local API anchors in 2.3 | confirmed/partial | `WorldAlterationProgressState`; `MarkWorldAltered` command |
| `AllowedToSpreadInfections`、`totalX/totalD`、`npcSpawnDelay/Period` | `WorldGen`，public static | `UpdateWorld`、城镇/NPC spawn | `UpdateWorld`、creative power input | every ecology tick | schedule cursor/gate；Creative power 是外部权限输入 | authoritative runtime | `WorldGen.cs:4185,4189-4193,59404-59448` | confirmed | `WorldEcologyScheduleState`; ecology/spawn systems |
| `CreateNewWorld`、`GenerateWorld`、`clearWorld` | `WorldGen`，public static methods | load/create composition | create workflow | load/generation only | creates background task, mutates global generation flags, clears storage | behavior + lifecycle effect | `WorldGen.cs:6285-6305,6387-6524,10108-10147` | confirmed | `WorldSessionLifecycleSystem`; `TerrainGenerationSystem` |
| `WorldGen.Hooks.OnWorldGenConfigProcess` / `.OnWorldLoad` | `WorldGen.Hooks`，public static C# events | config subscribers; ambience, seasonal and mystic-log subscribers | `ProcessWorldGenConfig` / `WorldLoaded` invoke events | config processing / post-load | external callback boundary; subscribers can introduce order coupling | adapter event, not state | `WorldGen.cs:3335-3375`; subscribers `Main.cs:3281-3284,3381` | confirmed | `WorldGenerationLifecycleSystem` publishes typed lifecycle events after commit |
| `Main.OnEngine*` / `OnTick*` | `Main`，public static C# events | engine integrations and third-party callbacks | `DoUpdate` invokes events | engine process lifetime / every tick | host callback extension point; unrelated to one world's persistence | external integration | `Main.cs:1474-1480,11155-11160,11240-11242,11321-11323,11356-11358` | confirmed | host adapter; explicitly outside `WorldSession` |
| `MessageBuffer.OnTileChangeReceived` | `MessageBuffer`，public static C# event | tile-change receivers | packet case invokes event | inbound network message | transport notification following tile input, not a world state transition | adapter event | `MessageBuffer.cs:69,995-997` | confirmed | network/tile adapter; requires separate Tile command authority review |
| `ShouldUpdateEntities` | `Main`，public instance method | top-level Tick scheduler | reads phase/generation flags | every tick | readiness gate; no direct world mutation | derived query | `Main.cs:11400-11409` | confirmed | `SessionPhaseQuery` / `WorldReadyBarrier` |
| `UpdateWorld`、grass/infection/housing calls | `WorldGen`，public static method + private helpers | main simulation | ecology scheduler | every world tick after lifecycle check | writes Tile/Liquid/TileEntity/Wiring via many calls | behavior + external writes | `WorldGen.cs:59400-59448,61830-62723` | confirmed/partial | `WorldEcologyAndSpreadSystem`; `TownHousingSystem`; Tile commands |
| `WorldGenerator._passes`、`GenerationProgress` | `Terraria.WorldBuilding`，private/projection type | generation UI/controller | generation job | generation job only | ordered pass execution and progress display; no durable session truth | scratch + presentation | `WorldGenerator.cs:262-333`; `WorldGen.cs:10108-10147` | confirmed | `IWorldGenerationJob` internal state; progress projection |
| `roomTiles`、room bounds/flags; `TownRoomManager` relations | `WorldGen` scratch / `TownRoomManager` | housing validation and NPC housing | room scan / registry methods | per query vs world lifetime | query scratch differs from persisted NPC-room relationship | scratch + authoritative relation | `WorldGen.cs:4213-4269,5267-5773`; `TownRoomManager.cs:9-15,65-142` | partial | `TownHousingQuery`; `TownHousingRegistry` |
| `SaveWorldHeader`/`LoadHeader` | `WorldFile`，public static methods | save/load orchestration | binary reader/writer | persistence boundary | version branches and temporary snapshots; I/O side effect | adapter | `WorldFile.cs:1263-1454,2008-2190` | confirmed | `IWorldSessionPersistenceAdapter` |
| package 7/78 serialization and inbound mutations | `NetMessage` / `MessageBuffer` | clients; server message dispatcher | outgoing packets / incoming packet cases | replication/event request | protocol I/O; inbound authority must be validated | projection + adapter | `NetMessage.cs:221-403`; `MessageBuffer.cs:2114-2247,2498-2508` | confirmed | `WorldSessionNetworkProjection`; `WorldEventCommandHandler` |

The source ledger covers the relevant fields, methods, scratch stores, C# events and boundary adapters. The currently observed events are host/plugin or transport callbacks; they do **not** provide a complete domain-event log or a transaction boundary. Consequently, the target typed domain events (`WorldReady`, `WorldFailed`, `BossDefeated`, `InvasionCompleted`) remain a proposal, rather than a claim that Version4 already has them.

### 3.3 明确不应进入上述组件的成员

| 当前成员 | 正确边界 | 不拆入会话的理由 |
| --- | --- | --- |
| `Main.tile`、Wall、Liquid、TileEntity、Chest、实体数组/槽位 | `WorldStorage` | 它们是大量可变世界对象及协议槽位，而非小型会话状态。 |
| `WorldGen.tileCounts`、总量、`SceneMetrics` | `SceneSensingQuery` / 失效可控 cache | 前者由扫描生成，后者含客户端/表现用途；Version4 的部分扫描 helper 为空，不能当作已确认的权威生态状态。 |
| `roomTiles`、房间 bounds、桌椅/门/光标志 | `TownHousingQuery` 的调用内 scratch | 一次房屋检查的临时工作集；持久占用关系归 `TownRoomManager`/`TownHousingRegistry`。 |
| `WorldGenerator._passes`、`GenerationProgress`、controller | `TerrainGenerationSystem` 的作业内部状态和 UI Projection | 仅生成过程存活，且不应在重新载入后成为玩法真相。 |
| `invasionProgressDisplayLeft`、`invasionProgressAlpha`、云/雨 alpha、camera metrics | Client Presentation Projection | 包含 UI 淡入、相机或渲染缓存；可由权威 snapshot 重建。 |
| `Main.myPlayer`、网络连接/客户端槽位 | NetworkSession / Player identity | 本地视角、网络 ID、持久化世界 ID、ECS 实体 ID 必须保持不同类型。 |

## 4. 模块契约、调用方向与顺序

```text
Load / Create command
  -> WorldSessionLifecycleSystem
  -> TerrainGenerationSystem (独占 WorldStorage 写期)
  -> WorldReadyBarrier
  -> WorldClockSystem
  -> WorldEventSchedulerSystem -> InvasionSystem / ProgressCommitSystem
  -> WorldEcologyAndSpreadSystem -> SpawnPressureSystem
  -> TownHousingSystem
  -> CommitBarrier
  -> PersistenceSnapshot + NetworkSnapshot + ClientPresentationProjection
```

| 模块 | Interface（输入/输出） | Implementation（唯一职责/写集） | Seam（替换或测试点） | Depth | Leverage | Locality |
| --- | --- | --- | --- | --- | --- | --- |
| `WorldSessionLifecycleSystem` | `SessionPhaseQuery`; create/load result -> `WorldReady` / `WorldFailed` | 唯一写 `WorldGenerationLifecycleState`；协调 load/create/cleanup，不写 Tile 算法 | 可替换 `IWorldLoader`；phase transition recorder | 封装阶段和失败转移，调用方不接触 bool 组合 | 一次消除实体、生态和生成的多处门控 | 阶段、异常和完成事件在一个模块 |
| `TerrainGenerationSystem` | `IWorldGenerationJob.Run(descriptor, rules, seed)` -> Tile/structure commands | 在独占 WorldStorage 写期执行 pass；提交 descriptor 和初始规则 | fake pass、固定 seed、recording command sink | 隐藏 pass 顺序、RNG 和 rollback | 将所有生成入口统一为一类 job | pass、RNG 与回滚在一个边界 |
| `BiomeRuleQuery` | `TryResolveBiome(in BiomeContext, out BiomeResult)` | 只读 TileMap、规则、坐标；不写任何状态 | deterministic TileMap fixture | 一个调用隐藏多个 Tile/Wall/规则判断 | 刷怪、掉落、环境效果可复用 | 与主要读取的 World/Tile 领域相邻 |
| `SceneSensingQuery` | `Scan(in SceneScanRequest)` -> transient metrics | 只产生派生环境度量；缓存必须记录范围、版本和失效条件 | known-map result fixture；cache invalidation probe | 隔离昂贵扫描和 cache 细节 | 多个表现/资格消费者复用一次扫描 | 缓存随 SceneSensing，不放入 Session |
| `WorldClockSystem` | `AdvanceClock(delta, random)` -> state delta + boundary commands | 唯一写 `WorldClockWeatherState`；玩法时钟与天气转换 | fixed clock/RNG; boundary trace | 将昼夜、雨和事件边界封装为确定性变换 | 所有时间条件读同一个 view | 时钟、天气定时器和边界命令共置 |
| `WorldEventSchedulerSystem` | `AdvanceEvents(view)` -> `Start/Stop/Advance` commands | 事件生命周期调度；不直接改 Boss completion | event clock fixture; idempotent command log | 隐藏事件启动/结束条件 | 复用到 Lantern/Party/Sandstorm/DD2 | 运行事件彼此同属 event scheduling |
| `InvasionSystem` | `ReportInvasionContribution(report)` -> atomic runtime update | 唯一更新 Size/Progress/complete 标志的入侵事务 | duplicate death report and completion fixture | 一个操作保持多个入侵字段一致 | 解耦 NPC death、网络和 UI | 全部 invasion runtime 字段同行 |
| `ProgressCommitSystem` | `MarkBossDefeated` / `UnlockNpc` / `MarkWorldAltered` | 唯一写世界级长期旗标；产生 commit event | duplicate command / save-load snapshot test | 隐藏多旗标、奖励资格和幂等性 | 所有规则消费者转为只读 | progress state 与其命令位于同一领域 |
| `WorldEcologyAndSpreadSystem` | `EcologyTick(snapshot, random)` -> Tile mutation commands | 唯一写生态 schedule；通过 Tile command 申请实际地图改动 | fixed seed, blocked-spread, mutation log | 把采样游标、传播 gate 和变更请求集中 | 避免每个 Tile 交互重复实现传播约束 | ecology schedule 和 system 共置；Tile storage 保持外部 |
| `TownHousingSystem` | `TownHousingQuery` + `AssignRoom` | 验证房屋，原子更新 `TownHousingRegistry`；不写 NPC gameplay | invalid room / duplicate assignment fixture | 分离 scratch scan 与持久关系 | Town NPC、迁移和存档共享关系边界 | query 和 registry 随 TownHousing 领域 |
| `WorldSessionPersistenceAdapter` | `Read/Write(WorldSessionSnapshot)` | 仅此处接触 WLD 版本分支、`BinaryReader/Writer` 和临时保存快照 | V319 fixture, truncation, unknown-version tests | 把二进制顺序和迁移藏在 typed snapshot 后 | 所有会话组件可共同 round-trip | 格式适配独立于领域状态 |
| `WorldSessionNetworkProjection` | `CreateWorldDataSnapshot`; `TryParseWorldCommand` | committed snapshot -> 包 7/78；入站只产生已验证命令 | protocol golden frames; reject unauthorized input | 网络包号、序列和 DTO 不进入领域 | 重连与多人同步消费同一 projection | network DTO/transport 不泄漏到 Session |

必要顺序约束：生成或加载中时 `WorldReadyBarrier` 关闭所有实体/生态/刷怪推进；Version4 的真实 `Main.ShouldUpdateEntities` 在 Ready 后仍要求 `!WorldGen.generatingWorld`（`Main.cs:11400-11409`），而 `WorldGen.UpdateWorld` 在 `isGeneratingOrLoadingWorld` 为真时立即返回（`WorldGen.cs:59400-59407`）。这两个门控在目标系统中应由同一阶段状态导出，不能保留双写 bool。

## 5. 迁移映射与兼容策略

| 批次 | 源路径/成员 | 目标 | 依赖影响与回滚 |
| --- | --- | --- | --- |
| 1 | `Main` 的 descriptor/rules/time/invasion 字段；`NPC.downed*` | 上述 7 个状态组件及只读 `IWorldSessionView` | 先只读镜像比对；回滚为旧字段单写，禁止双向同步。 |
| 2 | `WorldFile.SaveWorldHeader`/`LoadHeader` | `WorldSessionPersistenceAdapter` | 字段二进制顺序、版本条件和 temp snapshot 必须保持；可用 V319 fixture round-trip 回滚。 |
| 3 | `NetMessage` 包 7/78、`MessageBuffer` 入站写入 | Network projection + validated command handler | 先按旧包格式投影；拒绝任何直接组件写入。 |
| 4 | `UpdateTime`、天气、入侵、击杀完成 | Clock/Event/Invasion/Progress systems | 每次只迁一个写集；固定 Tick trace 比对。 |
| 5 | `CreateNewWorld`、`GenerateWorld`、`UpdateWorld`、住房 | Generation lifecycle、terrain、ecology、housing modules | 生命周期门控先于 Tile 写入迁移；异常/取消必须进入 Failed 或 Cleanup。 |

当前仓库的 [`src/WorldSession/WorldSessionComponents.cs`](../src/WorldSession/WorldSessionComponents.cs) 已存在这些概念的单文件设计骨架，但 `rg` 复核不到生产调用方或测试读写者，故其状态是 **未接线设计**，不是运行时迁移。它还把所有公开类型集中在一个文件，不满足“一核心公开类型一个同名 PascalCase 文件”的目标组织约束；待实施时应按世界领域扁平放置，例如 `src/WorldSession/WorldDescriptorState.cs`、`WorldRulesState.cs`、`WorldClockWeatherState.cs` 等，只有在生态/生成规模和独立验证单元稳定后才建立 `Ecology/` 或 `Generation/` 子目录。

## 6. 证据缺口、暂缓项与不拆分理由

| 项目 | 结论 | 状态与下一步 |
| --- | --- | --- |
| `D:\TRbackup\tmodloader-api-docs-stable\index.html` 本地 stable API 镜像 | 首页页眉为 `tModLoader v2026.07`；按 `annotated.html`、类型页实际 `href` 和成员锚点检索公开 API | 本报告列出的 `Main`/`NPC`/`WorldGen` 锚点为 `confirmed` 的公开语义证据；整个 API 表面及私有实现覆盖仍为 `partial`。Version4 本地源码才确认实际写者、持久化格式、网络包和调度，不能用 API 文档证明它们。 |
| 事件类实现完整度 | Birthday/Lantern/Sandstorm/DD2 状态存在，但一些关联路径跨类或为空 | `partial`；迁移每个事件前补字段级读写闭合。 |
| `SceneMetrics` 生态扫描 | 存在字段与 Scan 路径，但关键 helper 在 Version4 中为空 | `partial/missing`；不得持久化或以其结果驱动权威生态。 |
| 房屋有效性 | 检查 scratch 与持久住房 registry 的边界已确认 | `partial`；迁移时对“检查结果”与“占用提交”建立独立 verifier。 |
| 生成异步与取消 | `CreateNewWorld` 使用后台 Task，`GenerateWorld` finally 清理生成旗标 | `partial`；需明确单线程 WorldStorage 写所有权、失败事件和取消语义。 |
| 每个 Boss 一个实体/组件 | 不拆分 | Boss 击败旗标共同持久化、共同由规则读取且需要原子 snapshot；过度原子化会恶化查询与格式兼容。 |

### 6.1 组件级不变量、缓存和身份约束

| 边界 | 必须保持的不变量 | 缓存/快照政策 | 身份与关系约束 |
| --- | --- | --- | --- |
| `WorldDescriptorState` | `SizeX/Y`、边界、section count、出生/地牢锚点来自同一已验证世界描述；Ready 后不能局部改尺寸 | `SectionCountX/Y` 和 `HasSurface` 只能派生，不能持久化第二份值 | 仅存 `WorldId`/`UniqueId` 这类持久化身份；不存 network ID 或实体槽位 |
| `WorldRulesState` | `WorldEvil`、秘密种子、难度和 ore tiers 与加载的世界版本同属一次规则提交 | `EffectiveDifficulty`、`IsJourney/Expert/Master` 等仅为派生 view | 无实体引用；内容 ID/定义经 catalog/adapter 解析 |
| `WorldClockWeatherState` | `dayTime + time` 表示同一个世界时刻；事件切换只能经 `WorldClockSystem` | 雨/风视觉平滑、cloud alpha 不进入状态；若缓存天气资格，需以 clock revision 失效 | 不存玩家、客户端或网络 ID |
| `WorldEventProgressState` | Boss/解锁完成为单调真值；入侵一次提交同时改变 Size、Progress 与完成旗标 | 包 7/78 DTO 是提交后只读 snapshot，不能回写 | Boss 不是一个实体引用；完成状态不绑定 NPC `whoAmI`/slot |
| `WorldAlterationProgressState` | 长期改造完成与一次性待消费事件分开；消费后必须有明确清除或持久化语义 | 不缓存 Tile 区域副本 | Tile 坐标属于 command target；不转为世界实体 ID |
| `WorldEcologyScheduleState` | 采样游标和传播许可只有生态系统可写；实际 Tile 变化只能经 WorldStorage command | `SceneMetrics`/tile count 仅作带失效条件的 cache；当前证据不足时不驱动玩法 | 无玩家/NPC实体引用；需要区域遍历时由 Query 获取 |
| `WorldGenerationLifecycleState` | 状态机只允许 `Uninitialized -> Loading/Generating -> Ready|Failed -> Unloading` 的批准转移；Ready 前不得推进实体/生态 | `GenerationProgress` 为 UI projection；生成 pass 工作集在 job 结束释放 | 不把后台 Task、线程 ID、controller 放进持久化状态 |
| `TownHousingRegistry` | 同一 NPC 与房屋的占用关系在一次提交内保持双向一致 | 房屋检查 scratch 随 Query 调用释放 | 明确使用稳定 NPC/房屋关系键；不复用网络/slot ID |

**并行规则：** `TerrainGenerationSystem` 与任何 Tile/Liquid/生态写者互斥；`WorldClockSystem`、`WorldEventSchedulerSystem`、`InvasionSystem` 依赖提交顺序，默认串行。只有读集、写集和结构变更均不相交且 verifier 覆盖顺序等价性后，才可并行化。

### 6.2 风险登记与缓解

| 风险 | 当前证据 | 后果 | 缓解与 verifier |
| --- | --- | --- | --- |
| 生成后台任务与 WorldStorage 同时写入 | `CreateNewWorld` 使用 `Task.Factory.StartNew`；生成 flags 为静态 | Tile/状态损坏或未定义可见性 | generation barrier + single-writer job test；未证明前不并行 |
| WLD 版本字段顺序破坏 | 保存/加载依赖大量条件字段和 temp 值 | 旧存档不可读或半更新 | V319 golden fixture、截断输入原子失败、字段顺序 byte comparison |
| 包 7/78 与命令状态分叉 | `NetMessage` 投影和 `MessageBuffer` 可直接改全局字段 | 客户端显示/重连与服务器权威不一致 | committed snapshot golden frame；拒绝入站直接状态写入 |
| 生态 cache 被当权威 | `SceneMetrics` helper 不完整且 `tileCounts` 是扫描产物 | 刷怪/Biome 判断基于陈旧值 | 只让 Query 输出 metrics；地图 revision invalidation test |
| 进度幂等性遗漏 | NPC death 可反复到达或网络重放 | 重复奖励、入侵负数、保存脏状态 | duplicate command 和 repeat death report verifier |
| C# hook 的顺序耦合 | `OnWorldLoad`/tick callbacks 有多个订阅者 | 隐式依赖、加载后状态竞争 | 用显式 lifecycle phase + typed event order；记录订阅迁移顺序 |

## 7. Focused verifier 计划与本次实际结果

本次为只读报告，没有修改生产实现，因此未运行编译或行为测试；不能把静态盘点称为迁移验证。已实际完成的验证是：目标文档读取、Version4 声明/保存/加载/网络/Tick/生成/生态关键路径定位、SS14 组件/System 边界只读核对，以及 NLTX 中 `WorldSession` 现状的读写者搜索。

实施每一批后应运行以下 focused verifier；所有 .NET 命令须遵循仓库的串行 `Invoke-SerialDotnet.ps1` 规则。

1. **会话存档往返：** identity、尺寸、种子、规则、时间天气、Boss/入侵/暗影球/祭坛在 `Save -> Load` 后完全一致；截断/未知版本不产生半更新状态。
2. **网络投影：** 同一 committed snapshot 产生一致的包 7 与包 78；入站客户端请求不能直接改变 Boss、血月、日食或入侵状态。
3. **Tick 边界：** 固定时钟/RNG 下验证昼夜 0、27000、54000 边界、降雨开始/结束、Slime Rain 和入侵完成只提交一次。
4. **生成屏障：** Loading/Generating 时实体、生态和刷怪 System 均不运行；成功/失败/取消后阶段、锁和可见 snapshot 一致。
5. **Biome/生态：** 相同 seed、TileMap、规则与坐标得到相同纯查询结果；生态传播在受控随机源下仅经 Tile command 写入；感染禁用不会修改 Tile。
6. **住房：** 无效房间不改变占用 registry；并发或重复分配保持一个 NPC 与一个房间的关系不变量。

## 8. 交付判定

本报告完成了所请求的组件拆分设计、所有权审计、源/目标映射和验证计划。它明确区分了真实 Version4 证据、SS14 组织模式、现有未接线设计骨架和待实现的运行系统；因此不把文档或类型声明误报为已完成的权威模拟迁移。
