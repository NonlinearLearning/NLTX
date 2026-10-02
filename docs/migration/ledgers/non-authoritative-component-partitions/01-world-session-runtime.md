# Version4 非权威组件拆分分区 01/20：世界会话与运行时

> 来源报告：`docs/migration/ledgers/Version4非权威模拟系统字段属性逐成员源码声明-更细子系统拆分-去除ID类文件.md`
> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
> 本文件是并行组件拆分工作包，不是新的权威成员清单；成员声明事实直接保留来源报告原文。

## 1. 分区定位与评估

- 分区范围：启动、世界会话、帧时钟、规则控制、随机种子和运行时宿主状态。
- 本分区组件化重点：确认会话状态、规则状态和派生查询的权威所有权、生命周期及系统顺序。
- 本分区包含 12 个完整细分子系统、149 条成员记录（字段 104、属性 45）。来源序号覆盖区间 `1..3921`，区间可能与其他分区交错，不以序号定义领域边界。
- 组件边界判定：细分子系统只是并行处理的最小库存边界，不等价于一个组件。必须继续按共同读者/写者、变更原因、生命周期、权威性和副作用分为 Component、System、Query、Command、Adapter 或 Projection。
- 证据状态：成员声明和统计已由来源报告校验；逐成员读者、写者、初始化/清理路径、持久化/网络语义和系统顺序仍需本分区会话补证，不得从名称或数量推断。
- 总体评估：来源报告作为源码库存导航是可用的。现有验证已确认 `1..4542` 序号闭合、`4017/525` 字段/属性计数、349 个细分组和父级汇总一致；但它尚未达到可直接生成 ECS 组件的证据门槛，尤其缺少逐成员 owner/writer/lifecycle/side-effect 闭合。
- 已知风险：报告中存在混合语义组（例如图形与生成、相机与液体、网络发布与声音、场景装饰与音频），并行拆分时必须允许一个细分组进一步分成多个组件或非组件角色。

## 2. 分区统计

| 父级子系统 | 细分子系统数 | 字段 | 属性 | 合计 |
|---|---:|---:|---:|---:|
| `RuntimeComposition` | 9 | 75 | 22 | 97 |
| `SharedRuntimeMechanisms` | 3 | 29 | 23 | 52 |

| 原报告章节 | 父级 | 细分子系统 | 边界角色 | 字段 | 属性 | 合计 | 组件化处理 |
|---|---|---|---|---:|---:|---:|---|
| `4.1.1` | `RuntimeComposition` | `MainFrameActivityState` | runtime state | 4 | 0 | 4 | 待按成员访问模式拆分 |
| `4.1.3` | `RuntimeComposition` | `MainBootstrapAndWorldRules` | runtime state | 23 | 0 | 23 | 待按成员访问模式拆分 |
| `4.1.6` | `RuntimeComposition` | `MainClockAndFrameScheduling` | runtime state | 13 | 0 | 13 | 待按成员访问模式拆分 |
| `4.1.13` | `RuntimeComposition` | `MainFrameAndWorldRuleControl` | runtime state | 9 | 0 | 9 | 待按成员访问模式拆分 |
| `4.1.22` | `RuntimeComposition` | `MainRandomAndSeedState` | runtime state | 4 | 0 | 4 | 待按成员访问模式拆分 |
| `4.1.30` | `RuntimeComposition` | `MainSaveAndWorldSessionState` | session state | 11 | 0 | 11 | 待按成员访问模式拆分 |
| `4.1.36` | `RuntimeComposition` | `MainWindowAndShutdownState` | runtime state | 7 | 0 | 7 | 待按成员访问模式拆分 |
| `4.1.37` | `RuntimeComposition` | `MainTimeSkipState` | runtime state | 4 | 0 | 4 | 待按成员访问模式拆分 |
| `4.1.45` | `RuntimeComposition` | `MainDerivedWorldAndSessionQueries` | derived/query | 0 | 22 | 22 | 待按成员访问模式拆分 |
| `4.9.4` | `SharedRuntimeMechanisms` | `SharedDifficultyAndRuleMetadata` | definition/query | 16 | 0 | 16 | 待按成员访问模式拆分 |
| `4.9.101` | `SharedRuntimeMechanisms` | `WorldSeedAndExploitRules` | state/query | 1 | 12 | 13 | 待按成员访问模式拆分 |
| `4.9.223` | `SharedRuntimeMechanisms` | `SharedStartupAndRuntimeHostState` | diagnostics/adapter | 12 | 11 | 23 | 待按成员访问模式拆分 |

### 2.1 当前保留的 16 个并行分区

| 分区 | 领域 | 细分组 | 字段 | 属性 | 合计 | 文件 |
|---:|---|---:|---:|---:|---:|---|
| 01 | `世界会话与运行时` | 12 | 104 | 45 | 149 | [01-world-session-runtime.md](01-world-session-runtime.md) |
| 02 | `世界环境与事件` | 17 | 212 | 13 | 225 | [02-world-environment-events.md](02-world-environment-events.md) |
| 03 | `世界生成与地牢` | 35 | 456 | 29 | 485 | [03-world-generation-dungeons.md](03-world-generation-dungeons.md) |
| 04 | `地块、液体与世界存储` | 23 | 310 | 10 | 320 | [04-world-tiles-storage.md](04-world-tiles-storage.md) |
| 05 | `空间移动与物理` | 10 | 99 | 20 | 119 | [05-spatial-motion-physics.md](05-spatial-motion-physics.md) |
| 06 | `实体生命周期与归因` | 3 | 43 | 1 | 44 | [06-entity-lifecycle-attribution.md](06-entity-lifecycle-attribution.md) |
| 07 | `战斗、伤害与状态效果` | 6 | 91 | 6 | 97 | [07-combat-status-effects.md](07-combat-status-effects.md) |
| 08 | `玩家输入与玩法` | 26 | 232 | 51 | 283 | [08-player-input-gameplay.md](08-player-input-gameplay.md) |
| 09 | `物品、库存与容器` | 14 | 152 | 39 | 191 | [09-item-inventory-containers.md](09-item-inventory-containers.md) |
| 10 | `经济、配方、钓鱼与掉落` | 24 | 296 | 25 | 321 | [10-economy-crafting-fishing-loot.md](10-economy-crafting-fishing-loot.md) |
| 11 | `NPC、城镇与图鉴` | 8 | 71 | 22 | 93 | [11-npc-town-bestiary.md](11-npc-town-bestiary.md) |
| 12 | `内容定义与目录` | 19 | 229 | 56 | 285 | [12-content-definitions-catalogs.md](12-content-definitions-catalogs.md) |
| 13 | `网络协议与会话` | 18 | 238 | 15 | 253 | [13-network-protocol-session.md](13-network-protocol-session.md) |
| 14 | `持久化、恢复与配置` | 11 | 146 | 14 | 160 | [14-persistence-recovery-configuration.md](14-persistence-recovery-configuration.md) |
| 15 | `外部平台与协议边界` | 7 | 51 | 8 | 59 | [15-external-platform-boundaries.md](15-external-platform-boundaries.md) |
| 18 | `地图、相机与绘制` | 29 | 274 | 57 | 331 | [18-map-camera-rendering.md](18-map-camera-rendering.md) |

零成员父级 `ContentLifecycleAndRegistration` 和 `IntentAndInteraction` 保留在来源报告的父级统计中，不生成伪造成员分区；对应边界说明分别放在内容目录分区和玩家输入与玩法分区。

## 3. 并行组件拆分契约

对本文件中的每个细分子系统和每条成员记录执行以下判断，并把结果写入对应的实现阶段设计/审计记录：

- 盘点声明类型、可见性、读者、写者、创建/更新/清理/持久化生命周期、访问频率和副作用；缺证据标记 `missing`，不可用命名猜测补齐。
- 标记权威状态、派生值、缓存、快照、兼容字段和表现数据；Component 只保存一个内聚概念的权威数据。
- 需要行为转换时放入显式 System/Command；纯资格判断和派生计算放入 Query；外部协议、文件、平台和第三方类型放入 Adapter；网络/持久化/日志/UI 输出放入单向 Projection。
- 分别建模实体引用、持久化 ID、网络 ID 和外部 ID；不得把 ID 类文件或外部对象直接塞入共享组件。
- 记录 Interface、Implementation、Seam、Depth、Leverage、Locality，以及必要的系统先后约束、失败归属和重试策略。

## 4. 逐成员源码声明

以下细分子系统块保持来源报告的成员表、声明、路径、行列和来源序号；只调整 Markdown 标题层级并增加分区追踪元数据。

### 4.1 细分子系统：`MainFrameActivityState`

- 原报告章节：`4.1.1`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`01` / `世界会话与运行时`

- 上一级基线细分子系统：`MainFrameActivityState`
- 细分职责：帧级玩家、Boss 和可交互对象活动事实。
- 边界角色：`runtime state`；最小 seam：frame activity snapshot；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：4；属性：0；合计：4。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1 | field | Terraria.Main.CurrentFrameFlags | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 90 | 3 | ActivePlayersCount | int | `public static int ActivePlayersCount;` | `public static int ActivePlayersCount;` |
| 2 | field | Terraria.Main.CurrentFrameFlags | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 92 | 3 | SleepingPlayersCount | int | `public static int SleepingPlayersCount;` | `public static int SleepingPlayersCount;` |
| 3 | field | Terraria.Main.CurrentFrameFlags | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 94 | 3 | AnyActiveBossNPC | bool | `public static bool AnyActiveBossNPC;` | `public static bool AnyActiveBossNPC;` |
| 4 | field | Terraria.Main.CurrentFrameFlags | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 96 | 3 | HadAnActiveInteractableProjectile | bool | `public static bool HadAnActiveInteractableProjectile;` | `public static bool HadAnActiveInteractableProjectile;` |

#### 属性（0）

无该类型成员记录。


### 4.2 细分子系统：`MainBootstrapAndWorldRules`

- 原报告章节：`4.1.3`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`01` / `世界会话与运行时`

- 上一级基线细分子系统：`MainBootstrapAndWorldRules`
- 细分职责：启动引用、版本、秘密种子和世界规则开关。
- 边界角色：`runtime state`；最小 seam：startup/world-rules view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：23；属性：0；合计：23。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（23）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 7 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 118 | 2 | mapDelay | int | `public static int mapDelay = 2;` | `public static int mapDelay = 2;` |
| 8 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 120 | 2 | Assets | IAssetRepository | `public static IAssetRepository Assets;` | `public static IAssetRepository Assets;` |
| 9 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 122 | 2 | GameAskedToQuit | bool | `private static bool GameAskedToQuit = false;` | `private static bool GameAskedToQuit = false;` |
| 18 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 140 | 2 | versionNumber | string | `public static string versionNumber = "v1.4.5.6";` | `public static string versionNumber = "v1.4.5.6";` |
| 19 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 142 | 2 | versionNumber2 | string | `public static string versionNumber2 = "v1.4.5.6";` | `public static string versionNumber2 = "v1.4.5.6";` |
| 20 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 144 | 2 | AnnouncementBoxDisabled | bool | `public static bool AnnouncementBoxDisabled;` | `public static bool AnnouncementBoxDisabled;` |
| 21 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 146 | 2 | AnnouncementBoxRange | int | `public static int AnnouncementBoxRange = -1;` | `public static int AnnouncementBoxRange = -1;` |
| 22 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 148 | 2 | AutogenSeedName | string | `public static string AutogenSeedName;` | `public static string AutogenSeedName;` |
| 23 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 150 | 2 | drunkWorld | bool | `public static bool drunkWorld = false;` | `public static bool drunkWorld = false;` |
| 24 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 152 | 2 | getGoodWorld | bool | `public static bool getGoodWorld = false;` | `public static bool getGoodWorld = false;` |
| 25 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 154 | 2 | tenthAnniversaryWorld | bool | `public static bool tenthAnniversaryWorld = false;` | `public static bool tenthAnniversaryWorld = false;` |
| 26 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 156 | 2 | dontStarveWorld | bool | `public static bool dontStarveWorld = false;` | `public static bool dontStarveWorld = false;` |
| 27 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 158 | 2 | notTheBeesWorld | bool | `public static bool notTheBeesWorld = false;` | `public static bool notTheBeesWorld = false;` |
| 28 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 160 | 2 | remixWorld | bool | `public static bool remixWorld = false;` | `public static bool remixWorld = false;` |
| 29 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 162 | 2 | noTrapsWorld | bool | `public static bool noTrapsWorld = false;` | `public static bool noTrapsWorld = false;` |
| 30 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 164 | 2 | zenithWorld | bool | `public static bool zenithWorld = false;` | `public static bool zenithWorld = false;` |
| 31 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 166 | 2 | skyblockWorld | bool | `public static bool skyblockWorld = false;` | `public static bool skyblockWorld = false;` |
| 32 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 168 | 2 | vampireSeed | bool | `public static bool vampireSeed = false;` | `public static bool vampireSeed = false;` |
| 33 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 170 | 2 | infectedSeed | bool | `public static bool infectedSeed = false;` | `public static bool infectedSeed = false;` |
| 34 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 172 | 2 | teamBasedSpawnsSeed | bool | `public static bool teamBasedSpawnsSeed = false;` | `public static bool teamBasedSpawnsSeed = false;` |
| 35 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 174 | 2 | dualDungeonsSeed | bool | `public static bool dualDungeonsSeed = false;` | `public static bool dualDungeonsSeed = false;` |
| 36 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 176 | 2 | _gameModeDifficultyOverride | float? | `private static float? _gameModeDifficultyOverride = null;` | `private static float? _gameModeDifficultyOverride = null;` |
| 37 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 178 | 2 | destroyerHB | Vector2 | `public static Vector2 destroyerHB = new Vector2(0f, 0f);` | `public static Vector2 destroyerHB = new Vector2(0f, 0f);` |

#### 属性（0）

无该类型成员记录。


### 4.3 细分子系统：`MainClockAndFrameScheduling`

- 原报告章节：`4.1.6`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`01` / `世界会话与运行时`

- 上一级基线细分子系统：`MainClockAndFrameScheduling`
- 细分职责：全局时钟、延迟处理、帧率和服务器调度参数。
- 边界角色：`runtime state`；最小 seam：clock/frame scheduler port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：13；属性：0；合计：13。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（13）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 48 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 200 | 2 | clientUUID | string | `public static string clientUUID;` | `public static string clientUUID;` |
| 49 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 202 | 2 | GlobalTimeWrappedHourly | float | `public static float GlobalTimeWrappedHourly;` | `public static float GlobalTimeWrappedHourly;` |
| 50 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 204 | 2 | GlobalTimerPaused | bool | `public static bool GlobalTimerPaused = false;` | `public static bool GlobalTimerPaused = false;` |
| 51 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 206 | 2 | gameTimeCache | GameTime | `public static GameTime gameTimeCache = new GameTime();` | `public static GameTime gameTimeCache = new GameTime();` |
| 52 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 210 | 2 | ScreenShaderRef | Asset<Effect> | `public static Asset<Effect> ScreenShaderRef = Asset<Effect>.Empty;` | `public static Asset<Effect> ScreenShaderRef = Asset<Effect>.Empty;` |
| 53 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 212 | 2 | DelayedProcesses | System.Collections.Generic.List<System.Collections.IEnumerator> | `public static List<IEnumerator> DelayedProcesses = new List<IEnumerator>();` | `public static List<IEnumerator> DelayedProcesses = new List<IEnumerator>();` |
| 54 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 214 | 2 | DelayedProcessesInGame | System.Collections.Generic.List<System.Collections.IEnumerator> | `public static List<IEnumerator> DelayedProcessesInGame = new List<IEnumerator>();` | `public static List<IEnumerator> DelayedProcessesInGame = new List<IEnumerator>();` |
| 55 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 216 | 2 | npcStreamSpeed | int | `public static int npcStreamSpeed = 30;` | `public static int npcStreamSpeed = 30;` |
| 56 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 218 | 2 | dedServFPS | bool | `public static bool dedServFPS;` | `public static bool dedServFPS;` |
| 57 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 220 | 2 | dedServCount1 | int | `public static int dedServCount1;` | `public static int dedServCount1;` |
| 58 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 222 | 2 | dedServCount2 | int | `public static int dedServCount2;` | `public static int dedServCount2;` |
| 59 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 224 | 2 | offLimitBorderTiles | int | `public static readonly int offLimitBorderTiles = 40;` | `public static readonly int offLimitBorderTiles = 40;` |
| 60 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 226 | 2 | maxMusic | int | `public static readonly int maxMusic = 105;` | `public static readonly int maxMusic = 105;` |

#### 属性（0）

无该类型成员记录。


### 4.4 细分子系统：`MainFrameAndWorldRuleControl`

- 原报告章节：`4.1.13`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`01` / `世界会话与运行时`

- 上一级基线细分子系统：`MainFrameAndWorldRuleControl`
- 细分职责：暂停、世界难度、帧计数和自动加入控制。
- 边界角色：`runtime state`；最小 seam：pause/world-rule control；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：9；属性：0；合计：9。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（9）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 165 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 495 | 2 | hardMode | bool | `public static bool hardMode;` | `public static bool hardMode;` |
| 166 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 497 | 2 | maxQ | bool | `public static bool maxQ = true;` | `public static bool maxQ = true;` |
| 167 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 500 | 2 | DiscoR | int | `public static int DiscoR = 255;` | `public static int DiscoR = 255;` |
| 168 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 502 | 2 | DiscoB | int | `public static int DiscoB;` | `public static int DiscoB;` |
| 169 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 504 | 2 | DiscoG | int | `public static int DiscoG;` | `public static int DiscoG;` |
| 170 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 506 | 2 | gamePaused | bool | `public static bool gamePaused;` | `public static bool gamePaused;` |
| 171 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 508 | 2 | ReHideCursor | bool | `public bool ReHideCursor;` | `public bool ReHideCursor;` |
| 172 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 510 | 2 | updatesCountedForFPS | int | `public static int updatesCountedForFPS;` | `public static int updatesCountedForFPS;` |
| 173 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 512 | 2 | autoJoin | bool | `public static bool autoJoin;` | `public static bool autoJoin;` |

#### 属性（0）

无该类型成员记录。


### 4.5 细分子系统：`MainRandomAndSeedState`

- 原报告章节：`4.1.22`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`01` / `世界会话与运行时`

- 上一级基线细分子系统：`MainRandomAndSeedState`
- 细分职责：随机源、月亮类型和实验/种子配置。
- 边界角色：`runtime state`；最小 seam：random/seed port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：4；属性：0；合计：4。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 251 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 673 | 2 | rand | Terraria.Utilities.UnifiedRandom | `[ThreadStatic] public static UnifiedRandom rand;` | `[ThreadStatic] public static UnifiedRandom rand;` |
| 252 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 679 | 2 | moonType | int | `public static int moonType = 0;` | `public static int moonType = 0;` |
| 253 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 683 | 2 | UseExperimentalFeatures | bool | `public static bool UseExperimentalFeatures;` | `public static bool UseExperimentalFeatures;` |
| 254 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 685 | 2 | DefaultSeed | string | `public static string DefaultSeed = "";` | `public static string DefaultSeed = "";` |

#### 属性（0）

无该类型成员记录。


### 4.6 细分子系统：`MainSaveAndWorldSessionState`

- 原报告章节：`4.1.30`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`01` / `世界会话与运行时`

- 上一级基线细分子系统：`MainSaveAndWorldSessionState`
- 细分职责：世界准备、存档路径、活动文件和世界列表。
- 边界角色：`session state`；最小 seam：save/world session port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：11；属性：0；合计：11。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 422 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1035 | 2 | _worldPreparationState | Terraria.Main.WorldPreparationState | `private static WorldPreparationState _worldPreparationState = WorldPreparationState.AwaitingData;` | `private static WorldPreparationState _worldPreparationState = WorldPreparationState.AwaitingData;` |
| 423 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1039 | 2 | motd | string | `public static string motd = "";` | `public static string motd = "";` |
| 424 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1041 | 2 | gameMenu | bool | `public static bool gameMenu = true;` | `public static bool gameMenu = true;` |
| 425 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1043 | 2 | lockMenuBGChange | bool | `public static bool lockMenuBGChange = false;` | `public static bool lockMenuBGChange = false;` |
| 426 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1045 | 2 | maxLoadWorld | int | `private static int maxLoadWorld = 1000;` | `private static int maxLoadWorld = 1000;` |
| 427 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1047 | 2 | ActivePlayerFileData | Terraria.IO.PlayerFileData | `public static PlayerFileData ActivePlayerFileData = new PlayerFileData();` | `public static PlayerFileData ActivePlayerFileData = new PlayerFileData();` |
| 428 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1049 | 2 | WorldList | System.Collections.Generic.List<Terraria.IO.WorldFileData> | `public static List<WorldFileData> WorldList = new List<WorldFileData>();` | `public static List<WorldFileData> WorldList = new List<WorldFileData>();` |
| 429 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1051 | 2 | ActiveWorldFileData | Terraria.IO.WorldFileData | `public static WorldFileData ActiveWorldFileData = new WorldFileData();` | `public static WorldFileData ActiveWorldFileData = new WorldFileData();` |
| 430 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1053 | 2 | WorldPath | string | `public static string WorldPath = Path.Combine(SavePath, "Worlds");` | `public static string WorldPath = Path.Combine(SavePath, "Worlds");` |
| 431 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1055 | 2 | CloudWorldPath | string | `public static string CloudWorldPath = "worlds";` | `public static string CloudWorldPath = "worlds";` |
| 432 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1057 | 2 | PlayerPath | string | `public static string PlayerPath = Path.Combine(SavePath, "Players");` | `public static string PlayerPath = Path.Combine(SavePath, "Players");` |

#### 属性（0）

无该类型成员记录。


### 4.7 细分子系统：`MainWindowAndShutdownState`

- 原报告章节：`4.1.36`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`01` / `世界会话与运行时`

- 上一级基线细分子系统：`MainWindowAndShutdownState`
- 细分职责：窗口、锚点管理、退出和自动生成路径状态。
- 边界角色：`runtime state`；最小 seam：window/shutdown adapter；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：7；属性：0；合计：7。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（7）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 463 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1195 | 2 | _windowMover | Terraria.Graphics.WindowStateController | `private static WindowStateController _windowMover;` | `private static WindowStateController _windowMover;` |
| 464 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1197 | 2 | sittingManager | Terraria.DataStructures.AnchoredEntitiesCollection | `public static AnchoredEntitiesCollection sittingManager;` | `public static AnchoredEntitiesCollection sittingManager;` |
| 465 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1199 | 2 | sleepingManager | Terraria.DataStructures.AnchoredEntitiesCollection | `public static AnchoredEntitiesCollection sleepingManager;` | `public static AnchoredEntitiesCollection sleepingManager;` |
| 466 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1201 | 2 | oldStatusText | string | `public static string oldStatusText = "";` | `public static string oldStatusText = "";` |
| 467 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1203 | 2 | autoGenFileLocation | string | `public static string autoGenFileLocation = null;` | `public static string autoGenFileLocation = null;` |
| 468 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1205 | 2 | autoShutdown | bool | `public static bool autoShutdown;` | `public static bool autoShutdown;` |
| 469 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1207 | 2 | previousExecutionState | uint | `private uint previousExecutionState;` | `private uint previousExecutionState;` |

#### 属性（0）

无该类型成员记录。


### 4.8 细分子系统：`MainTimeSkipState`

- 原报告章节：`4.1.37`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`01` / `世界会话与运行时`

- 上一级基线细分子系统：`MainTimeSkipState`
- 细分职责：日晷/月晷快速推进和冷却状态。
- 边界角色：`runtime state`；最小 seam：time-skip command view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：4；属性：0；合计：4。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（4）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 470 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1209 | 2 | fastForwardTimeToDawn | bool | `public static bool fastForwardTimeToDawn;` | `public static bool fastForwardTimeToDawn;` |
| 471 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1211 | 2 | sundialCooldown | int | `public static int sundialCooldown;` | `public static int sundialCooldown;` |
| 472 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1213 | 2 | fastForwardTimeToDusk | bool | `public static bool fastForwardTimeToDusk;` | `public static bool fastForwardTimeToDusk;` |
| 473 | field | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1215 | 2 | moondialCooldown | int | `public static int moondialCooldown;` | `public static int moondialCooldown;` |

#### 属性（0）

无该类型成员记录。


### 4.9 细分子系统：`MainDerivedWorldAndSessionQueries`

- 原报告章节：`4.1.45`
- 父级子系统：`RuntimeComposition`
- 分区工作包：`01` / `世界会话与运行时`

- 上一级基线细分子系统：`MainDerivedPropertiesAndEvents`
- 细分职责：世界模式、路径、资格和会话对象的只读派生查询。
- 边界角色：`derived/query`；最小 seam：main derived world session port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：1；声明类型数：1；字段：0；属性：22；合计：22。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（0）

无该类型成员记录。

#### 属性（22）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 513 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1316 | 2 | SavePath | string | `public static string SavePath => Program.SavePath;` | `public static string SavePath => Program.SavePath;` |
| 514 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1318 | 2 | GameMode | int | `public static int GameMode { get { if (ActiveWorldFileData == null) { return 0; } return ActiveWorldFileData.GameMode; } set { if (ActiveWorldFileData != null && GameModeID.IsValid(value)) { ActiveWorldFileData.GameMode = value; } } }` | `public static int GameMode { get { if (ActiveWorldFileData == null) { return 0; } return ActiveWorldFileData.GameMode; } set { if (ActiveWorldFileData != null && GameModeID.IsValid(value)) { ActiveWorldFileData.GameMode = value; } } }` |
| 515 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1337 | 2 | IsJourneyMode | bool | `public static bool IsJourneyMode => GameMode == 3;` | `public static bool IsJourneyMode => GameMode == 3;` |
| 516 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1339 | 2 | NoFunctionalSurface | bool | `public static bool NoFunctionalSurface => worldSurface <= 30.0;` | `public static bool NoFunctionalSurface => worldSurface <= 30.0;` |
| 517 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1341 | 2 | surviveHardcoreDeath | bool | `public static bool surviveHardcoreDeath { get { if (dontStarveWorld && tenthAnniversaryWorld) { return !getGoodWorld; } return false; } }` | `public static bool surviveHardcoreDeath { get { if (dontStarveWorld && tenthAnniversaryWorld) { return !getGoodWorld; } return false; } }` |
| 518 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1353 | 2 | onlyShimmerOceanWorlds | bool | `public static bool onlyShimmerOceanWorlds { get { if (drunkWorld && tenthAnniversaryWorld && !remixWorld && !zenithWorld) { return !notTheBeesWorld; } return false; } }` | `public static bool onlyShimmerOceanWorlds { get { if (drunkWorld && tenthAnniversaryWorld && !remixWorld && !zenithWorld) { return !notTheBeesWorld; } return false; } }` |
| 519 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1365 | 2 | masterMode | bool | `public static bool masterMode => Difficulty >= GameDifficultyLevel.Master;` | `public static bool masterMode => Difficulty >= GameDifficultyLevel.Master;` |
| 520 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1367 | 2 | expertMode | bool | `public static bool expertMode => Difficulty >= GameDifficultyLevel.Expert;` | `public static bool expertMode => Difficulty >= GameDifficultyLevel.Expert;` |
| 521 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1369 | 2 | Difficulty | float | `public static float Difficulty { get { float num = GameDifficultyLevel.Classic; if (ActiveWorldFileData != null) { if (_gameModeDifficultyOverride.HasValue) { num = _gameModeDifficultyOverride.Value; } else if (GameMode == 1) { num = GameDifficultyLevel.Expert; } else if (GameMode == 2) { num = GameDifficultyLevel.Master; } if (getGoodWorld) { num += 1f; } } return num; } }` | `public static float Difficulty { get { float num = GameDifficultyLevel.Classic; if (ActiveWorldFileData != null) { if (_gameModeDifficultyOverride.HasValue) { num = _gameModeDifficultyOverride.Value; } else if (GameMode == 1) { num = GameDifficultyLevel.Expert; } else if (GameMode == 2) { num = GameDifficultyLevel.Master; } if (getGoodWorld) { num += 1f; } } return num; } }` |
| 522 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1397 | 2 | Achievements | Terraria.Achievements.AchievementManager | `public static AchievementManager Achievements => instance._achievements;` | `public static AchievementManager Achievements => instance._achievements;` |
| 523 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1399 | 2 | UnpausedUpdateSeed | ulong | `public static ulong UnpausedUpdateSeed { get; private set; }` | `public static ulong UnpausedUpdateSeed { get; private set; }` |
| 527 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1432 | 2 | GameUpdateCount | uint | `public static uint GameUpdateCount => _gameUpdateCount;` | `public static uint GameUpdateCount => _gameUpdateCount;` |
| 528 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1434 | 2 | worldID | int | `public static int worldID => ActiveWorldFileData.WorldId;` | `public static int worldID => ActiveWorldFileData.WorldId;` |
| 529 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1436 | 2 | isThereAWorldSurface | bool | `public static bool isThereAWorldSurface => worldSurface > 50.0;` | `public static bool isThereAWorldSurface => worldSurface > 50.0;` |
| 530 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1438 | 2 | UnderworldLayer | int | `public static int UnderworldLayer => maxTilesY - 200;` | `public static int UnderworldLayer => maxTilesY - 200;` |
| 532 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1442 | 2 | SceneMetrics | Terraria.SceneMetrics | `public static SceneMetrics SceneMetrics { get { if (!_usingSeparateCameraSceneMetrics) { return _playerSceneMetrics; } return _cameraSceneMetrics; } }` | `public static SceneMetrics SceneMetrics { get { if (!_usingSeparateCameraSceneMetrics) { return _playerSceneMetrics; } return _cameraSceneMetrics; } }` |
| 535 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1458 | 2 | LocalPlayer | Terraria.Player | `public static Player LocalPlayer => player[myPlayer];` | `public static Player LocalPlayer => player[myPlayer];` |
| 536 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1460 | 2 | npcShop | int | `public static int npcShop { get; private set; }` | `public static int npcShop { get; private set; }` |
| 537 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1462 | 2 | playerPathName | string | `public static string playerPathName => ActivePlayerFileData.Path;` | `public static string playerPathName => ActivePlayerFileData.Path;` |
| 538 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1464 | 2 | worldPathName | string | `public static string worldPathName => ActiveWorldFileData.Path;` | `public static string worldPathName => ActiveWorldFileData.Path;` |
| 539 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1466 | 2 | IsItAHappyWindyDay | bool | `public static bool IsItAHappyWindyDay => _shouldUseWindyDayMusic;` | `public static bool IsItAHappyWindyDay => _shouldUseWindyDayMusic;` |
| 540 | property | Terraria.Main | Terraria/Main.cs | D:\TRbackup\Version4\Terraria\Main.cs | 1468 | 2 | IsItStorming | bool | `public static bool IsItStorming => _shouldUseStormMusic;` | `public static bool IsItStorming => _shouldUseStormMusic;` |


### 4.10 细分子系统：`SharedDifficultyAndRuleMetadata`

- 原报告章节：`4.9.4`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`01` / `世界会话与运行时`

- 上一级基线细分子系统：`SharedDifficultyAndRuleMetadata`
- 细分职责：难度等级、曲线和规则元数据。
- 边界角色：`definition/query`；最小 seam：difficulty/rule metadata view；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：4；字段：16；属性：0；合计：16。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（16）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 1169 | field | Terraria.DataStructures.GameDifficultyData.LinearCurve.Key | Terraria.DataStructures/GameDifficultyData.cs | D:\TRbackup\Version4\Terraria.DataStructures\GameDifficultyData.cs | 9 | 4 | input | float | `public readonly float input;` | `public readonly float input;` |
| 1170 | field | Terraria.DataStructures.GameDifficultyData.LinearCurve.Key | Terraria.DataStructures/GameDifficultyData.cs | D:\TRbackup\Version4\Terraria.DataStructures\GameDifficultyData.cs | 11 | 4 | output | float | `public readonly float output;` | `public readonly float output;` |
| 1171 | field | Terraria.DataStructures.GameDifficultyData.LinearCurve | Terraria.DataStructures/GameDifficultyData.cs | D:\TRbackup\Version4\Terraria.DataStructures\GameDifficultyData.cs | 23 | 3 | keys | Terraria.DataStructures.GameDifficultyData.LinearCurve.Key[] | `public readonly Key[] keys;` | `public readonly Key[] keys;` |
| 1172 | field | Terraria.DataStructures.GameDifficultyData | Terraria.DataStructures/GameDifficultyData.cs | D:\TRbackup\Version4\Terraria.DataStructures\GameDifficultyData.cs | 63 | 2 | EnemyMaxLifeMultiplier | Terraria.DataStructures.GameDifficultyData.LinearCurve | `public static readonly LinearCurve EnemyMaxLifeMultiplier = new LinearCurve(new LinearCurve.Key(GameDifficultyLevel.Journey, 0.5f), new LinearCurve.Key(GameDifficultyLevel.Legendary, 4f));` | `public static readonly LinearCurve EnemyMaxLifeMultiplier = new LinearCurve(new LinearCurve.Key(GameDifficultyLevel.Journey, 0.5f), new LinearCurve.Key(GameDifficultyLevel.Legendary, 4f));` |
| 1173 | field | Terraria.DataStructures.GameDifficultyData | Terraria.DataStructures/GameDifficultyData.cs | D:\TRbackup\Version4\Terraria.DataStructures\GameDifficultyData.cs | 65 | 2 | EnemyDamageMultiplier | Terraria.DataStructures.GameDifficultyData.LinearCurve | `public static readonly LinearCurve EnemyDamageMultiplier = new LinearCurve(new LinearCurve.Key(GameDifficultyLevel.Journey, 0.5f), new LinearCurve.Key(GameDifficultyLevel.Master, 3f), new LinearCurve.Key(GameDifficultyLevel.Legendary, 5.3333335f));` | `public static readonly LinearCurve EnemyDamageMultiplier = new LinearCurve(new LinearCurve.Key(GameDifficultyLevel.Journey, 0.5f), new LinearCurve.Key(GameDifficultyLevel.Master, 3f), new LinearCurve.Key(GameDifficultyLevel.Legendary, 5.3333335f));` |
| 1174 | field | Terraria.DataStructures.GameDifficultyData | Terraria.DataStructures/GameDifficultyData.cs | D:\TRbackup\Version4\Terraria.DataStructures\GameDifficultyData.cs | 67 | 2 | HostileProjectileDamageMultiplier | Terraria.DataStructures.GameDifficultyData.LinearCurve | `public static readonly LinearCurve HostileProjectileDamageMultiplier = new LinearCurve(new LinearCurve.Key(GameDifficultyLevel.Journey, 0.5f), new LinearCurve.Key(GameDifficultyLevel.Master, 3f));` | `public static readonly LinearCurve HostileProjectileDamageMultiplier = new LinearCurve(new LinearCurve.Key(GameDifficultyLevel.Journey, 0.5f), new LinearCurve.Key(GameDifficultyLevel.Master, 3f));` |
| 1175 | field | Terraria.DataStructures.GameDifficultyData | Terraria.DataStructures/GameDifficultyData.cs | D:\TRbackup\Version4\Terraria.DataStructures\GameDifficultyData.cs | 69 | 2 | KnockbackToEnemiesMultiplier | Terraria.DataStructures.GameDifficultyData.LinearCurve | `public static readonly LinearCurve KnockbackToEnemiesMultiplier = new LinearCurve(new LinearCurve.Key(GameDifficultyLevel.Classic, 1f), new LinearCurve.Key(GameDifficultyLevel.Master, 0.8f));` | `public static readonly LinearCurve KnockbackToEnemiesMultiplier = new LinearCurve(new LinearCurve.Key(GameDifficultyLevel.Classic, 1f), new LinearCurve.Key(GameDifficultyLevel.Master, 0.8f));` |
| 1176 | field | Terraria.DataStructures.GameDifficultyData | Terraria.DataStructures/GameDifficultyData.cs | D:\TRbackup\Version4\Terraria.DataStructures\GameDifficultyData.cs | 71 | 2 | EnemyMoneyDropMultiplier | Terraria.DataStructures.GameDifficultyData.LinearCurve | `public static readonly LinearCurve EnemyMoneyDropMultiplier = new LinearCurve(new LinearCurve.Key(GameDifficultyLevel.Classic, 1f), new LinearCurve.Key(GameDifficultyLevel.Expert, 2.5f), new LinearCurve.Key(GameDifficultyLevel.Master, 2.5f), new LinearCurve.Key(GameDifficultyLevel.Legendary, 3.5f));` | `public static readonly LinearCurve EnemyMoneyDropMultiplier = new LinearCurve(new LinearCurve.Key(GameDifficultyLevel.Classic, 1f), new LinearCurve.Key(GameDifficultyLevel.Expert, 2.5f), new LinearCurve.Key(GameDifficultyLevel.Master, 2.5f), new LinearCurve.Key(GameDifficultyLevel.Legendary, 3.5f));` |
| 1177 | field | Terraria.DataStructures.GameDifficultyData | Terraria.DataStructures/GameDifficultyData.cs | D:\TRbackup\Version4\Terraria.DataStructures\GameDifficultyData.cs | 73 | 2 | TownNPCDamageMultiplier | Terraria.DataStructures.GameDifficultyData.LinearCurve | `public static readonly LinearCurve TownNPCDamageMultiplier = new LinearCurve(new LinearCurve.Key(GameDifficultyLevel.Journey, 2f), new LinearCurve.Key(GameDifficultyLevel.Classic, 1f), new LinearCurve.Key(GameDifficultyLevel.Expert, 1.5f), new LinearCurve.Key(GameDifficultyLevel.Legendary, 2f));` | `public static readonly LinearCurve TownNPCDamageMultiplier = new LinearCurve(new LinearCurve.Key(GameDifficultyLevel.Journey, 2f), new LinearCurve.Key(GameDifficultyLevel.Classic, 1f), new LinearCurve.Key(GameDifficultyLevel.Expert, 1.5f), new LinearCurve.Key(GameDifficultyLevel.Legendary, 2f));` |
| 1178 | field | Terraria.DataStructures.GameDifficultyData | Terraria.DataStructures/GameDifficultyData.cs | D:\TRbackup\Version4\Terraria.DataStructures\GameDifficultyData.cs | 75 | 2 | DebuffTimeMultiplier | Terraria.DataStructures.GameDifficultyData.LinearCurve | `public static readonly LinearCurve DebuffTimeMultiplier = new LinearCurve(new LinearCurve.Key(GameDifficultyLevel.Classic, 1f), new LinearCurve.Key(GameDifficultyLevel.Expert, 2f), new LinearCurve.Key(GameDifficultyLevel.Master, 2.5f));` | `public static readonly LinearCurve DebuffTimeMultiplier = new LinearCurve(new LinearCurve.Key(GameDifficultyLevel.Classic, 1f), new LinearCurve.Key(GameDifficultyLevel.Expert, 2f), new LinearCurve.Key(GameDifficultyLevel.Master, 2.5f));` |
| 1179 | field | Terraria.DataStructures.GameDifficultyData | Terraria.DataStructures/GameDifficultyData.cs | D:\TRbackup\Version4\Terraria.DataStructures\GameDifficultyData.cs | 77 | 2 | LightningPlayerDamageScaling | Terraria.DataStructures.GameDifficultyData.LinearCurve | `public static readonly LinearCurve LightningPlayerDamageScaling = new LinearCurve(new LinearCurve.Key(GameDifficultyLevel.Journey, 0.04f), new LinearCurve.Key(GameDifficultyLevel.Classic, 0.08f), new LinearCurve.Key(GameDifficultyLevel.Master, 0.24f), new LinearCurve.Key(GameDifficultyLevel.Legendary, 0.4f));` | `public static readonly LinearCurve LightningPlayerDamageScaling = new LinearCurve(new LinearCurve.Key(GameDifficultyLevel.Journey, 0.04f), new LinearCurve.Key(GameDifficultyLevel.Classic, 0.08f), new LinearCurve.Key(GameDifficultyLevel.Master, 0.24f), new LinearCurve.Key(GameDifficultyLevel.Legendary, 0.4f));` |
| 1180 | field | Terraria.DataStructures.GameDifficultyLevel | Terraria.DataStructures/GameDifficultyLevel.cs | D:\TRbackup\Version4\Terraria.DataStructures\GameDifficultyLevel.cs | 5 | 2 | Journey | float | `public static readonly float Journey = 0.5f;` | `public static readonly float Journey = 0.5f;` |
| 1181 | field | Terraria.DataStructures.GameDifficultyLevel | Terraria.DataStructures/GameDifficultyLevel.cs | D:\TRbackup\Version4\Terraria.DataStructures\GameDifficultyLevel.cs | 7 | 2 | Classic | float | `public static readonly float Classic = 1f;` | `public static readonly float Classic = 1f;` |
| 1182 | field | Terraria.DataStructures.GameDifficultyLevel | Terraria.DataStructures/GameDifficultyLevel.cs | D:\TRbackup\Version4\Terraria.DataStructures\GameDifficultyLevel.cs | 9 | 2 | Expert | float | `public static readonly float Expert = 2f;` | `public static readonly float Expert = 2f;` |
| 1183 | field | Terraria.DataStructures.GameDifficultyLevel | Terraria.DataStructures/GameDifficultyLevel.cs | D:\TRbackup\Version4\Terraria.DataStructures\GameDifficultyLevel.cs | 11 | 2 | Master | float | `public static readonly float Master = 3f;` | `public static readonly float Master = 3f;` |
| 1184 | field | Terraria.DataStructures.GameDifficultyLevel | Terraria.DataStructures/GameDifficultyLevel.cs | D:\TRbackup\Version4\Terraria.DataStructures\GameDifficultyLevel.cs | 13 | 2 | Legendary | float | `public static readonly float Legendary = 4f;` | `public static readonly float Legendary = 4f;` |

#### 属性（0）

无该类型成员记录。


### 4.11 细分子系统：`WorldSeedAndExploitRules`

- 原报告章节：`4.9.101`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`01` / `世界会话与运行时`

- 上一级基线细分子系统：`WorldSeedAndExploitRules`
- 细分职责：特殊种子规则和吞噬者漏洞保护状态。
- 边界角色：`state/query`；最小 seam：world seed exploit rules；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：2；声明类型数：2；字段：1；属性：12；合计：13。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（1）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 2423 | field | Terraria.GameContent.FixExploitManEaters | Terraria.GameContent/FixExploitManEaters.cs | D:\TRbackup\Version4\Terraria.GameContent\FixExploitManEaters.cs | 7 | 2 | IndexesProtected | System.Collections.Generic.List<int> | `private static readonly List<int> IndexesProtected = new List<int>();` | `private static readonly List<int> IndexesProtected = new List<int>();` |

#### 属性（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3857 | property | Terraria.GameContent.SpecialSeedFeatures | Terraria.GameContent/SpecialSeedFeatures.cs | D:\TRbackup\Version4\Terraria.GameContent\SpecialSeedFeatures.cs | 5 | 2 | ShouldDropExtraGel | bool | `public static bool ShouldDropExtraGel { get { if (Main.tenthAnniversaryWorld && Main.drunkWorld && !Main.remixWorld) { return !Main.notTheBeesWorld; } return false; } }` | `public static bool ShouldDropExtraGel { get { if (Main.tenthAnniversaryWorld && Main.drunkWorld && !Main.remixWorld) { return !Main.notTheBeesWorld; } return false; } }` |
| 3858 | property | Terraria.GameContent.SpecialSeedFeatures | Terraria.GameContent/SpecialSeedFeatures.cs | D:\TRbackup\Version4\Terraria.GameContent\SpecialSeedFeatures.cs | 17 | 2 | ShouldDropExtraWood | bool | `public static bool ShouldDropExtraWood { get { if (Main.tenthAnniversaryWorld && Main.drunkWorld && !Main.remixWorld) { return !Main.notTheBeesWorld; } return false; } }` | `public static bool ShouldDropExtraWood { get { if (Main.tenthAnniversaryWorld && Main.drunkWorld && !Main.remixWorld) { return !Main.notTheBeesWorld; } return false; } }` |
| 3859 | property | Terraria.GameContent.SpecialSeedFeatures | Terraria.GameContent/SpecialSeedFeatures.cs | D:\TRbackup\Version4\Terraria.GameContent\SpecialSeedFeatures.cs | 29 | 2 | DungeonEntranceHasATree | bool | `public static bool DungeonEntranceHasATree { get { if (Main.drunkWorld) { return !NoDungeonGuardian; } return false; } }` | `public static bool DungeonEntranceHasATree { get { if (Main.drunkWorld) { return !NoDungeonGuardian; } return false; } }` |
| 3860 | property | Terraria.GameContent.SpecialSeedFeatures | Terraria.GameContent/SpecialSeedFeatures.cs | D:\TRbackup\Version4\Terraria.GameContent\SpecialSeedFeatures.cs | 41 | 2 | DungeonEntranceIsBuried | bool | `public static bool DungeonEntranceIsBuried { get { if (WorldGen.SecretSeed.surfaceIsDesert.Enabled) { return !DungeonEntranceIsUnderground; } return false; } }` | `public static bool DungeonEntranceIsBuried { get { if (WorldGen.SecretSeed.surfaceIsDesert.Enabled) { return !DungeonEntranceIsUnderground; } return false; } }` |
| 3861 | property | Terraria.GameContent.SpecialSeedFeatures | Terraria.GameContent/SpecialSeedFeatures.cs | D:\TRbackup\Version4\Terraria.GameContent\SpecialSeedFeatures.cs | 53 | 2 | DungeonEntranceIsUnderground | bool | `public static bool DungeonEntranceIsUnderground { get { if (!Main.drunkWorld) { return WorldGen.SecretSeed.noSurface.Enabled; } return true; } }` | `public static bool DungeonEntranceIsUnderground { get { if (!Main.drunkWorld) { return WorldGen.SecretSeed.noSurface.Enabled; } return true; } }` |
| 3862 | property | Terraria.GameContent.SpecialSeedFeatures | Terraria.GameContent/SpecialSeedFeatures.cs | D:\TRbackup\Version4\Terraria.GameContent\SpecialSeedFeatures.cs | 65 | 2 | NoDungeonGuardian | bool | `public static bool NoDungeonGuardian => Main.onlyShimmerOceanWorlds;` | `public static bool NoDungeonGuardian => Main.onlyShimmerOceanWorlds;` |
| 3863 | property | Terraria.GameContent.SpecialSeedFeatures | Terraria.GameContent/SpecialSeedFeatures.cs | D:\TRbackup\Version4\Terraria.GameContent\SpecialSeedFeatures.cs | 67 | 2 | BossesKeepSpawning | bool | `public static bool BossesKeepSpawning { get { if (Main.getGoodWorld && Main.dontStarveWorld) { return !Main.tenthAnniversaryWorld; } return false; } }` | `public static bool BossesKeepSpawning { get { if (Main.getGoodWorld && Main.dontStarveWorld) { return !Main.tenthAnniversaryWorld; } return false; } }` |
| 3864 | property | Terraria.GameContent.SpecialSeedFeatures | Terraria.GameContent/SpecialSeedFeatures.cs | D:\TRbackup\Version4\Terraria.GameContent\SpecialSeedFeatures.cs | 79 | 2 | ShimmerSpawnHalfOfWorld | bool | `public static bool ShimmerSpawnHalfOfWorld => Main.onlyShimmerOceanWorlds;` | `public static bool ShimmerSpawnHalfOfWorld => Main.onlyShimmerOceanWorlds;` |
| 3865 | property | Terraria.GameContent.SpecialSeedFeatures | Terraria.GameContent/SpecialSeedFeatures.cs | D:\TRbackup\Version4\Terraria.GameContent\SpecialSeedFeatures.cs | 81 | 2 | RainbowSandAndBlackSandWalls | bool | `public static bool RainbowSandAndBlackSandWalls => Main.onlyShimmerOceanWorlds;` | `public static bool RainbowSandAndBlackSandWalls => Main.onlyShimmerOceanWorlds;` |
| 3866 | property | Terraria.GameContent.SpecialSeedFeatures | Terraria.GameContent/SpecialSeedFeatures.cs | D:\TRbackup\Version4\Terraria.GameContent\SpecialSeedFeatures.cs | 83 | 2 | SpawnOnBeach | bool | `public static bool SpawnOnBeach { get { if (Main.tenthAnniversaryWorld && !Main.remixWorld) { return !Main.dontStarveWorld; } return false; } }` | `public static bool SpawnOnBeach { get { if (Main.tenthAnniversaryWorld && !Main.remixWorld) { return !Main.dontStarveWorld; } return false; } }` |
| 3867 | property | Terraria.GameContent.SpecialSeedFeatures | Terraria.GameContent/SpecialSeedFeatures.cs | D:\TRbackup\Version4\Terraria.GameContent\SpecialSeedFeatures.cs | 95 | 2 | SpawnOnBeachOnDungeonSide | bool | `public static bool SpawnOnBeachOnDungeonSide { get { if (SpawnOnBeach) { return Main.onlyShimmerOceanWorlds; } return false; } }` | `public static bool SpawnOnBeachOnDungeonSide { get { if (SpawnOnBeach) { return Main.onlyShimmerOceanWorlds; } return false; } }` |
| 3868 | property | Terraria.GameContent.SpecialSeedFeatures | Terraria.GameContent/SpecialSeedFeatures.cs | D:\TRbackup\Version4\Terraria.GameContent\SpecialSeedFeatures.cs | 107 | 2 | Mechdusa | bool | `public static bool Mechdusa { get { if (Main.remixWorld) { return Main.getGoodWorld; } return false; } }` | `public static bool Mechdusa { get { if (Main.remixWorld) { return Main.getGoodWorld; } return false; } }` |


### 4.12 细分子系统：`SharedStartupAndRuntimeHostState`

- 原报告章节：`4.9.223`
- 父级子系统：`SharedRuntimeMechanisms`
- 分区工作包：`01` / `世界会话与运行时`

- 上一级基线细分子系统：`SharedStartupAndIssueReporting`
- 上一级 peer 细分子系统：`SharedStartupAndIssueReporting`
- 细分职责：启动参数、运行时宿主、平台句柄和服务依赖状态。
- 边界角色：`diagnostics/adapter`；最小 seam：startup runtime host port；跨组通过 Query、事件、命令或适配器交接，禁止隐式修改别组权威字段。
- 成员文件数：5；声明类型数：5；字段：12；属性：11；合计：23。
- 归属证据状态：`source-inventory-confirmed`；完整读者/写者、生命周期、持久化、网络和运行时调度仍需实现阶段单独闭合。

#### 字段（12）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3137 | field | Terraria.InitData | Terraria/InitData.cs | D:\TRbackup\Version4\Terraria\InitData.cs | 5 | 2 | MaxNPCs | int | `public static readonly int MaxNPCs = 200;` | `public static readonly int MaxNPCs = 200;` |
| 3382 | field | Terraria.Program | Terraria/Program.cs | D:\TRbackup\Version4\Terraria\Program.cs | 19 | 2 | IsXna | bool | `public static bool IsXna = true;` | `public static bool IsXna = true;` |
| 3383 | field | Terraria.Program | Terraria/Program.cs | D:\TRbackup\Version4\Terraria\Program.cs | 21 | 2 | IsMono | bool | `public static bool IsMono = Type.GetType("Mono.Runtime") != null;` | `public static bool IsMono = Type.GetType("Mono.Runtime") != null;` |
| 3384 | field | Terraria.Program | Terraria/Program.cs | D:\TRbackup\Version4\Terraria\Program.cs | 23 | 2 | LaunchParameters | System.Collections.Generic.Dictionary<string, string> | `public static Dictionary<string, string> LaunchParameters = new Dictionary<string, string>();` | `public static Dictionary<string, string> LaunchParameters = new Dictionary<string, string>();` |
| 3385 | field | Terraria.Program | Terraria/Program.cs | D:\TRbackup\Version4\Terraria\Program.cs | 25 | 2 | SavePath | string | `public static string SavePath;` | `public static string SavePath;` |
| 3386 | field | Terraria.Program | Terraria/Program.cs | D:\TRbackup\Version4\Terraria\Program.cs | 27 | 2 | TerrariaSaveFolderPath | string | `public const string TerrariaSaveFolderPath = "Terraria";` | `public const string TerrariaSaveFolderPath = "Terraria";` |
| 3387 | field | Terraria.Program | Terraria/Program.cs | D:\TRbackup\Version4\Terraria\Program.cs | 29 | 2 | ThingsToLoad | int | `private static int ThingsToLoad;` | `private static int ThingsToLoad;` |
| 3388 | field | Terraria.Program | Terraria/Program.cs | D:\TRbackup\Version4\Terraria\Program.cs | 31 | 2 | ThingsLoaded | int | `private static int ThingsLoaded;` | `private static int ThingsLoaded;` |
| 3389 | field | Terraria.Program | Terraria/Program.cs | D:\TRbackup\Version4\Terraria\Program.cs | 33 | 2 | LoadedEverything | bool | `public static bool LoadedEverything;` | `public static bool LoadedEverything;` |
| 3390 | field | Terraria.Program | Terraria/Program.cs | D:\TRbackup\Version4\Terraria\Program.cs | 35 | 2 | JitForcedMethodCache | nint | `public static IntPtr JitForcedMethodCache;` | `public static IntPtr JitForcedMethodCache;` |
| 3428 | field | Terraria.Ref<T> | Terraria/Ref.cs | D:\TRbackup\Version4\Terraria\Ref.cs | 5 | 2 | Value | T | `public T Value;` | `public T Value;` |
| 3666 | field | Terraria.WindowsLaunch | Terraria/WindowsLaunch.cs | D:\TRbackup\Version4\Terraria\WindowsLaunch.cs | 22 | 2 | _handleRoutine | Terraria.WindowsLaunch.HandlerRoutine | `private static HandlerRoutine _handleRoutine;` | `private static HandlerRoutine _handleRoutine;` |

#### 属性（11）

| 来源序号 | 成员类型 | 类 | 相对路径 | 绝对路径 | 行 | 列 | 成员 | C# 类型 | 成员声明 | 原始声明 |
|---:|---|---|---|---|---:|---:|---|---|---|---|
| 3911 | property | Terraria.Server.Game | Terraria.Server/Game.cs | D:\TRbackup\Version4\Terraria.Server\Game.cs | 10 | 2 | Components | GameComponentCollection | `public GameComponentCollection Components => null;` | `public GameComponentCollection Components => null;` |
| 3912 | property | Terraria.Server.Game | Terraria.Server/Game.cs | D:\TRbackup\Version4\Terraria.Server\Game.cs | 12 | 2 | Content | ContentManager | `public ContentManager Content { get { return null; } set { } }` | `public ContentManager Content { get { return null; } set { } }` |
| 3913 | property | Terraria.Server.Game | Terraria.Server/Game.cs | D:\TRbackup\Version4\Terraria.Server\Game.cs | 23 | 2 | GraphicsDevice | GraphicsDevice | `public GraphicsDevice GraphicsDevice => null;` | `public GraphicsDevice GraphicsDevice => null;` |
| 3914 | property | Terraria.Server.Game | Terraria.Server/Game.cs | D:\TRbackup\Version4\Terraria.Server\Game.cs | 25 | 2 | InactiveSleepTime | System.TimeSpan | `public TimeSpan InactiveSleepTime { get { return TimeSpan.Zero; } set { } }` | `public TimeSpan InactiveSleepTime { get { return TimeSpan.Zero; } set { } }` |
| 3915 | property | Terraria.Server.Game | Terraria.Server/Game.cs | D:\TRbackup\Version4\Terraria.Server\Game.cs | 36 | 2 | IsActive | bool | `public bool IsActive => true;` | `public bool IsActive => true;` |
| 3916 | property | Terraria.Server.Game | Terraria.Server/Game.cs | D:\TRbackup\Version4\Terraria.Server\Game.cs | 38 | 2 | IsFixedTimeStep | bool | `public bool IsFixedTimeStep { get { return true; } set { } }` | `public bool IsFixedTimeStep { get { return true; } set { } }` |
| 3917 | property | Terraria.Server.Game | Terraria.Server/Game.cs | D:\TRbackup\Version4\Terraria.Server\Game.cs | 49 | 2 | IsMouseVisible | bool | `public bool IsMouseVisible { get { return false; } set { } }` | `public bool IsMouseVisible { get { return false; } set { } }` |
| 3918 | property | Terraria.Server.Game | Terraria.Server/Game.cs | D:\TRbackup\Version4\Terraria.Server\Game.cs | 60 | 2 | LaunchParameters | LaunchParameters | `public LaunchParameters LaunchParameters => null;` | `public LaunchParameters LaunchParameters => null;` |
| 3919 | property | Terraria.Server.Game | Terraria.Server/Game.cs | D:\TRbackup\Version4\Terraria.Server\Game.cs | 62 | 2 | Services | GameServiceContainer | `public GameServiceContainer Services => null;` | `public GameServiceContainer Services => null;` |
| 3920 | property | Terraria.Server.Game | Terraria.Server/Game.cs | D:\TRbackup\Version4\Terraria.Server\Game.cs | 64 | 2 | TargetElapsedTime | System.TimeSpan | `public TimeSpan TargetElapsedTime { get { return TimeSpan.Zero; } set { } }` | `public TimeSpan TargetElapsedTime { get { return TimeSpan.Zero; } set { } }` |
| 3921 | property | Terraria.Server.Game | Terraria.Server/Game.cs | D:\TRbackup\Version4\Terraria.Server\Game.cs | 75 | 2 | Window | GameWindow | `public GameWindow Window => null;` | `public GameWindow Window => null;` |


## 5. 追溯与验收

> 来源报告 SHA-256：`b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196`
- 本分区细分组数：12；成员数：149；字段：104；属性：45。
- 预期不变量：本文件只出现完整细分子系统；不跨分区复制成员；所有成员的来源序号、原始声明、路径、行列和 C# 类型必须与来源报告一致。
- 历史全量生成校验：当时读取原始 20 个分区文件，检查细分组唯一性、来源序号恰好覆盖 `1..4542`、字段/属性合计和逐成员行文本一致。该校验针对完整来源基线；当前工作范围保留 P01-P15、P18，共 16 个分区。

生成器：`Build/Tools/Generate-Version4NonAuthoritativeComponentPartitionReports.ps1`。
