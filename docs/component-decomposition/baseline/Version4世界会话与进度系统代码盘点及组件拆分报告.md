# Version4 世界会话与进度系统代码盘点及组件拆分报告

> 范围：`docs/Version4权威游戏模拟系统主要子系统.md` 的第 2 项“世界会话与进度”，并依据该文档第 11 项“战斗、状态与归因”划定交接边界。
>
> 方法：遵循 `public-decomposition` 的成员盘点、状态所有权、访问模式分组、组件/System/Query/Command/Adapter/Projection 边界与 verifier 规划流程。`D:\TRbackup\Version4` 和 `C:\Users\shan\Downloads\ECS\space-station-14-master` 均只读。用户指定的 `D:\TRbackup\Version4参考` 不存在，故以实际存在的 `D:\TRbackup\Version4` 为 Version4 源码根。

## 1. 结论

不要把 `Main` 或 `NPC` 静态字段整体搬进一个“世界组件”。世界会话是一个生命周期根，包含五个按共同写者、持久化边界和更新频率划分的权威状态组件：

```text
WorldSessionRoot
├── WorldDescriptorState
│   └── 身份、尺寸、边界、地层、出生/地牢锚点、准备阶段
├── WorldRulesState
│   └── 难度、Hardmode、特殊种子、邪恶类型、感染和矿脉/祭坛规则
├── WorldTimeWeatherState
│   └── 时钟、昼夜、月相、雨、风和临时天气/昼夜事件时钟
├── WorldEventProgressState
│   └── Boss/救援 NPC/解锁、入侵、塔/Lunar、DD2 和待消费事件
└── WorldSpawnPressureState
    └── 刷怪周期、城镇刷新、事件预算；人口与区域资格为派生 Query
```

`Tile[,]`、液体、TileEntity、实体数组槽位、箱子和区段不归这里，仍归第 3 项 `WorldStorage`。生命、伤害、治疗、免疫、Buff、击退、死亡判定和伤害贡献账本也不归这里，归第 11 项 `CombatAndStatus` / `RandomnessAndAttribution`。世界会话只在命中或死亡已得到权威结论后接收明确的进度提交命令。

## 2. 真实代码证据与范围

### 2.1 主拥有者与边界代码

| 现有代码 | 事实和已确认路径 | 候选边界 | 状态 |
| --- | --- | --- | --- |
| `Terraria/Main.cs:148-176,495-624,652-674,1059-1081` | 特殊种子、Hardmode、世界描述、时间、天气、Slime Rain、入侵运行态均为静态字段 | Descriptor、Rules、TimeWeather、EventProgress 的字段来源 | confirmed |
| `Terraria/Main.cs:11344-11354,11575-11587,12050-13623` | tick 中驱动天气、时间、昼夜事件、城镇刷新和入侵 | Clock、Weather、EventScheduler、Invasion System 的顺序来源 | confirmed |
| `Terraria/NPC.cs:6151-6293` | `saved*`、`unlocked*`、`downed*`、塔盾、Lunar Apocalypse 在 NPC 类型中以静态字段存在 | WorldEventProgressState；不能留在 NPC 实例 | confirmed |
| `Terraria/NPC.cs:64763-65219,65681-65960` | 击杀推进入侵、Boss、塔和长期旗标 | ProgressCommitCommand 的事实来源 | confirmed |
| `Terraria/WorldGen.cs:4113-4193,59400-59525,6387-6667` | 邪恶类型、感染、祭坛/暗影球、待办事件、刷怪节流、生态 tick 和世界清理 | Rules、SpawnPressure、Ecology、Lifecycle | confirmed / partial |
| `Terraria.IO/WorldFileData.cs:15-118` | 世界 ID/GUID、尺寸、种子、生成器版本、模式的持久化元数据 | Descriptor + Persistence identity | confirmed |
| `Terraria.IO/WorldFile.cs:98-180,658-760,878-980,1263-1455` | 加载/保存、temp staging、版本化头和会话字段编解码 | Persistence Adapter + Snapshot migration | confirmed |
| `Terraria/NetMessage.cs:221-403,1158-1163` | 包 7 投影世界状态，包 78 投影入侵进度 | Network Projection；只读快照输出 | confirmed |
| `Terraria/MessageBuffer.cs:2114-2247` | 入站路径可直接改血月、日食和入侵字段 | Network Command Adapter；必须验证后再提交 | confirmed |
| `Terraria.GameContent.Events/BirthdayParty.cs`、`LanternNight.cs`、`Sandstorm.cs`、`DD2Event.cs`、`CultistRitual.cs` | 各自持有活动状态、冷却或时钟 | Event 子状态 + Event System；若干实现为空 | partial |

### 2.2 当前递归扫描的完整路径集合

为避免“仅搜索 `Main.cs`”漏掉消费者，以下命令当前命中 **50** 个 C# 文件：

```powershell
rg -l --glob '*.cs' 'Main\.(hardMode|dayTime|time|moonPhase|raining|rainTime|maxRaining|windSpeedCurrent|windSpeedTarget|bloodMoon|pumpkinMoon|snowMoon|eclipse|slimeRain|slimeRainTime|invasion(Type|X|Size|Delay|Warn|Progress))|NPC\.(downed|saved|unlocked)|DD2Event|BirthdayParty|LanternNight|Sandstorm|CultistRitual|WorldFile|WorldGen\.UpdateWorld|WorldGen\.clearWorld' D:\TRbackup\Version4
```

```text
Terraria.DataStructures/PlayerMovementAccsCache.cs
Terraria.GameContent.Ambience/AmbienceServer.cs
Terraria.GameContent.Events/BirthdayParty.cs
Terraria.GameContent.Events/CultistRitual.cs
Terraria.GameContent.Events/DangerousDungeonCurse.cs
Terraria.GameContent.Events/DD2Event.cs
Terraria.GameContent.Events/LanternNight.cs
Terraria.GameContent.Events/Sandstorm.cs
Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs
Terraria.GameContent.ItemDropRules/CommonCode.cs
Terraria.GameContent.ItemDropRules/Conditions.cs
Terraria.GameContent.ItemDropRules/ItemDropDatabase.cs
Terraria.GameContent.ItemDropRules/MechBossSpawnersDropRule.cs
Terraria.GameContent.LootSimulation/SimulatorInfo.cs
Terraria.GameContent.Tile_Entities/TELogicSensor.cs
Terraria.GameContent.UI.States/UIWorldSelect.cs
Terraria.GameContent/CoinLossRevengeSystem.cs
Terraria.GameContent/PlayerEyeHelper.cs
Terraria.ID/ArmorIDs.cs; Terraria.ID/DustID.cs; Terraria.ID/ItemID.cs; Terraria.ID/ProjectileID.cs
Terraria.Initializers/LaunchInitializer.cs
Terraria.IO/WorldFile.cs; Terraria.IO/WorldFileData.cs
Terraria.Social.Base/WorkshopSocialModule.cs
Terraria.Utilities/TileSnapshot.cs
Terraria.WorldBuilding/WorldGenerationOptions.cs; Terraria.WorldBuilding/WorldGenerator.cs; Terraria.WorldBuilding/WorldGenSnapshot.cs
Terraria/Chest.cs; Terraria/Cloud.cs; Terraria/DelegateMethods.cs; Terraria/Item.cs; Terraria/Lang.cs
Terraria/Main.cs; Terraria/MessageBuffer.cs; Terraria/Mount.cs; Terraria/NetMessage.cs; Terraria/Netplay.cs
Terraria/NPC.cs; Terraria/Player.cs; Terraria/Projectile.cs; Terraria/Rain.cs; Terraria/SceneMetrics.cs
Terraria/StrayMethods.cs; Terraria/Utils.cs; Terraria/Wiring.cs; Terraria/WorldGen.cs; Terraria/WorldItem.cs
```

命中是审计范围而不是组件所有权。ID 定义、工具、UI、模拟、绘制、掉落和生成文件通常是只读消费者、目录或投影；只有上节的直接拥有者迁移权威字段。

### 2.3 来源日志与离线 API 证据

`public-decomposition` 要求把目标源码、参考 ECS 和公开 API 分别作为不同强度的证据。
SS14 仅支持组织边界结论；离线 tModLoader 文档仅支持公开成员已说明的语义；字段的真实读写、
持久化、网络和清理顺序均以 Version4 源码为准。

| source | version | query / hits | evidence | gaps | stop reason |
| --- | --- | --- | --- | --- | --- |
| `D:\TRbackup\Version4` | `Main.versionNumber` 为 `v1.4.5.6` | Main/NPC/WorldGen/WorldFile/网络/事件定向递归扫描，50 个 C# 命中 | `Main.cs:148-176,495-624,926-952,1059-1081`；`NPC.cs:6151-6293,64763-65219,65681-65960`；`WorldFile.cs:1186-1204,1263-1466` | 若干事件方法为空；这些成员标为 `partial` | 权威字段、存档/网络边界和主要生命周期已取得本地源码证据 |
| `D:\TRbackup\tmodloader-api-docs-stable` | 首页标题 `tModLoader: Main Page`，页眉 `tModLoader v2026.07` | `Main`、`NPC`、`WorldFileData`、`WorldSections`，各 1 个经 `annotated.html`/`classes.html` 实际 href 消歧的类型页 | `class_main.html#aa5b5ad648e5affabcd4429c7d67f9791` 说明 `dayTime` 与 `time` 表示世界时间；`#a7b1590162f4fa618a63b66c8e18321f9` 说明非零 `invasionType` 表示活跃入侵；`#aa44e433e53a98400dacd5260313d7b2b`/`#a879663e005dab5ec98f1e90ebb7236d8` 说明地层边界；`class_n_p_c.html#ad25f939434ba648f28a26d6d53eca16b` 说明 `downedBoss1` 是当前世界进度；`class_world_file_data.html#a6baa7dd2c616a7aca803c1d85fb26627`/`#ab4ce1c80b9d912864dc7f840810c60a4` 给出 GUID/SeedText；`class_world_sections.html#a0e4dda8444fc442ea74648d12c7b0e14` 给出区段加载查询 | API 文档不声明 Version4 私有写者、二进制字段顺序或网络包语义 | 已确认公开成员类型和说明，剩余事实回退到 Version4 |
| `C:\Users\shan\Downloads\ECS\space-station-14-master` | 当前本地 checkout | `GameTicker`、`GameMapManager`、保存命令、自动保存和数据库端口 | 本报告第 6 节及 6.1 的精确文件与行号 | 不能支持 Terraria 字段语义 | 已获得 Component/System/Query/Adapter/Projection 组织证据 |

上表中的 tModLoader 锚点是本地 HTML 路径的一部分。例如 `Main.dayTime` 的完整可复查证据为
`D:\TRbackup\tmodloader-api-docs-stable\class_main.html#aa5b5ad648e5affabcd4429c7d67f9791`。该镜像没有被视为
Version4 运行时或构建输入。

## 3. 成员归属表

下表按 `public-decomposition` 的成员证据模板压缩同生命周期、同主要写者的字段群；这不是按字段数量
机械拆分。`Read By` 和 `Written By` 记录领域写入者而非试图列举 50 个消费者，精确源码位置保留在
`Evidence`。`confirmed` 表示声明、主要写者与边界至少各有一条源码证据；`partial` 表示算法或所有写者
尚未闭合，不能作为迁移完成依据。

| Member | Declaring Type / Visibility | Read By | Written By | Lifecycle / Access Pattern | State Kind | Evidence | Status | Candidate |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `WorldId`, `UniqueId`, `SeedText`, generator version, name | `WorldFileData` public fields/properties；`Main.worldName` public static | WorldFile、世界选择 UI、网络世界快照 | 新建/加载与元数据更新流程 | create -> load -> unload；身份读取多、写入极少 | authoritative / persisted | `WorldFileData.cs:21-118`; `WorldFile.cs:599-624,1267-1271`; local API `class_world_file_data.html#a6baa7dd2c616a7aca803c1d85fb26627` and `#ab4ce1c80b9d912864dc7f840810c60a4` | confirmed | `WorldDescriptorState` + `WorldPersistenceIdentity` |
| dimensions, bounds, sections, `worldSurface`, `rockLayer`, spawn/dungeon anchors | `Main` public static | collision, WorldGen, spawn, sections, network | load/generation and world reset | create -> generate/load -> unload; shared immutable-after-ready geometry | authoritative / persisted | `Main.cs:514-528,565-589,986-988`; `WorldFile.cs:1272-1277,1308-1318`; local API `class_main.html#aa44e433e53a98400dacd5260313d7b2b`, `#a879663e005dab5ec98f1e90ebb7236d8` | confirmed | `WorldDescriptorState`; sections stay in `WorldStorage` |
| preparation/generation gate | `Main._worldPreparationState` private static; `WorldGen.isGeneratingOrLoadingWorld` static | entity/world tick gate | load/generation/clear flow | loading -> generating -> ready -> unloading; phase gate | authoritative lifecycle state | `Main.cs:1035`; `WorldFile.cs:747-788`; `WorldGen.cs:59400-59414` | partial | `SessionPhaseState` + `SessionLifecycleSystem` |
| mode, hardmode, secret-seed flags, evil, infection, ore/altar/orb rules | `Main`/`WorldGen` public static, difficulty override private | generation, drops, spawn, event qualification queries | generation and explicit world-rule commands | create plus long-term transitions; high fan-out read | authoritative / persisted | `Main.cs:148-176`; `WorldGen.cs:4113-4193`; `WorldFile.cs:1278-1287,1319,1338-1355` | confirmed | `WorldRulesState`, `IWorldRulesView`, `IWorldRuleCommand` |
| `dayTime`, `time`, moon phase, rain, wind target/counters | `Main` public static | NPC, drops, ecology, presentation, packet 7 | `UpdateTime`, `UpdateWeather`, rain commands | every tick; clock writer with many readers | authoritative; `windSpeedCurrent`/visual alpha are derived or presentation | `Main.cs:594-624,652-668,12050-12171`; `WorldFile.cs:1312-1316,1348-1352,1364-1366`; local API `class_main.html#aa5b5ad648e5affabcd4429c7d67f9791` | confirmed for clock/rain; partial for wind cache split | `WorldTimeWeatherState`, `WorldClockSystem`, `WeatherSystem` |
| Blood Moon, Eclipse, moon/Slime Rain, party/lantern/sandstorm/cultist clocks | `Main` and event-class static fields | clock/event/NPC qualification, persistence and packet 7 | clock boundary/event systems | event start -> tick -> clear/persist; event-specific writers | authoritative event state; some temp staging | `Main.cs:538-550,606-624`; `WorldFile.cs:98-180,1315-1316,1348-1352,1399-1422`; event files in section 2.1 | partial: empty event algorithms remain | `WorldTimeWeatherState` event substate + `WorldEventScheduler` |
| `downed*`, `saved*`, `unlocked*`, pet/book flags | `NPC` public static | spawn, drops, events, town logic, persistence/network | NPC/event resolution and progress commit | world creation -> progress commits -> clear; common persistent reads | authoritative / persisted | `NPC.cs:6151-6293,65681-65960`; `WorldFile.cs:1320-1337,1372-1376,1411,1430-1453`; local API `class_n_p_c.html#ad25f939434ba648f28a26d6d53eca16b` | confirmed | `WorldEventProgressState` with grouped value objects |
| lunar towers, shields, apocalypse, Moon Lord countdown | `NPC` public static | boss/event rules, persistence/network | boss/event resolution, reset | event + long-term mixed lifecycle | authoritative / persisted plus runtime | `NPC.cs:5921-5923,6251-6283`; `WorldFile.cs:1390-1398`; `Main.clearWorld` evidence in `WorldGen.cs:6387-6667` | confirmed | `LunarProgressState` inside `WorldEventProgressState` |
| invasion type/position/size/delay/warn/progress and DD2 state | `Main` public static; `DD2Event` static | invasion NPCs, UI, packet 78, persistence | `StartInvasion`, `UpdateInvasion`, NPC kills, DD2 system | event start -> progress -> complete/reset; UI readers are separate | authoritative runtime + persisted recovery fields; display alpha is presentation | `Main.cs:1059-1081,11807-11855,12542-12616`; `WorldFile.cs:1344-1347,1377`; `NetMessage.cs:1158-1163`; local API `class_main.html#a7b1590162f4fa618a63b66c8e18321f9` | confirmed for standard invasion; partial for DD2 detail | `InvasionRuntimeState`, `Dd2ProgressState`, `InvasionSystem` |
| spawn delay/period, town refresh priority, Slime Rain budget | `WorldGen`/`Main` public static | NPC spawning and event spawners | WorldGen world update / spawn pressure flow | tick/event; shared by spawn systems | authoritative budget; population/area are derived query inputs | `WorldGen.cs:4189-4193,59437-59448`; `Main.cs:542-544,626,13645-13672` | confirmed for schedule; partial for all budget writers | `WorldSpawnPressureState`, `SpawnPressureQuery`, `WorldSpawnPressureSystem` |
| `tile[,]`, map, liquid, entity arrays, chests/signs and section bitmap | `Main` public static plus `WorldSections` private data | physics, interaction, framing, IO, replication | structure/slot commands and storage systems | load -> mutate -> persist/replicate -> unload | authoritative WorldStorage, not WorldSession | `Main.cs:926-952`; `WorldSections.cs:35-75`; `WorldFile.cs:1186-1204,1469-1495`; local API `class_world_sections.html#a0e4dda8444fc442ea74648d12c7b0e14` | confirmed | `TileMapStore`, `EntitySlotStore<T>`, `ContainerStore`, `WorldSectionState` |

### 3.1 按访问模式的组件汇总

| 候选组件 | 当前字段群 | 读者 / 唯一写者 | 生命周期和状态类别 | 不放入组件的内容 |
| --- | --- | --- | --- | --- |
| `WorldDescriptorState` | `worldName`、`WorldFileData`、world ID/GUID、种子、`maxTilesX/Y`、边界、`worldSurface`、`rockLayer`、出生/地牢锚点 | WorldFile、WorldGen、Section/网络读取；加载/生成流程写 | 加载至卸载；authoritative / persisted | Tile 数组、实体槽位、Map 和区段位图 |
| `WorldRulesState` | `hardMode`、GameMode/difficulty override、特殊 seed flags、`WorldGen.crimson`、感染开关、矿脉层级、祭坛/暗影球 | 生成、掉落、刷怪和事件 Query 读；规则 Command/生成写 | 世界创建及长期进度；authoritative / persisted | 内容定义表、`Main.rand`（运行环境服务） |
| `WorldTimeWeatherState` | `dayTime`、`time`、`moonPhase`、`raining`、`rainTime`、`maxRaining`、风目标/计数、Blood Moon、Eclipse、Slime Rain | Clock/Weather System 写；NPC、掉落、生态、网络读 | 每 tick；authoritative；视觉当前值可 derived | 云/雨渲染对象、`cloudAlpha`、UI alpha、表现历史 |
| `WorldEventProgressState` | `NPC.downed*`、`saved*`、`unlocked*`、塔/Lunar、`Main.invasion*`、DD2、`spawnEye/spawnMeteor` 等待办 | Event/Progress System 写；规则 Query、存档/网络读 | 事件期间或世界存档；authoritative / persisted | 单个 NPC 的生命、AI、目标、免疫和伤害账本 |
| `WorldSpawnPressureState` | NPC spawn delay/period、城镇刷新计数、Slime Rain 预算、波次预算 | SpawnPressure System 写；Spawner 读 | tick / event；authoritative 预算 + derived 人口 | 活跃玩家数、区域资格等应为纯 Query，不能缓存成真值 |

## 4. 战斗、状态与归因的交接

```text
Attack / Hit intent
  -> DamageResolutionSystem
  -> HealthState + DefenseState + ImmunityState + StatusEffectsState
  -> DeathResolutionSystem
  -> NpcDeathResolved (死亡事实、NPC 定义、来源/归因结果)
  -> ProgressQualificationQuery (只读 Rules + EventProgress)
  -> ProgressCommitCommand
  -> WorldEventProgressState
  -> persistence / network projection
```

| 归属 | 证据 | 约束 |
| --- | --- | --- |
| `CombatAndStatus` | `Player.cs:1355-1373,3541-3729`；`NPC.cs:6111-6345,78614-78621` | 生命、减伤、免疫、Buff、击退和死亡判定都留在实体侧。 |
| `RandomnessAndAttribution` | `NPCDamageTracker.cs:143-286` 将真实 NPC、攻击者和伤害累计为暂态账本 | 归因账本不持久化为 Boss/入侵旗标。`InvasionDamageTracker.IncludeDamageFor` 与 `CheckActive` 在 `:32-35` 为空，故入侵贡献筛选仅为 `partial`。 |
| `WorldEventProgressState` | `NPC.cs:65681-65960` 写入 `downed*`/塔状态；`Main.cs:12542-12684` 推进或完成入侵 | 只接受幂等 `ProgressCommitCommand`；不得由投射物、伤害或网络包直接改 `Main.invasion*` / `NPC.downed*`。 |

最小接口：

```text
ProgressQualificationQuery.Evaluate(NpcDeathResolved, IWorldSessionView)
IWorldProgressCommand.Commit(ProgressCommitCommand)
```

前者必须是纯 Query；后者在提交屏障原子写入旗标和投影事件。命令需包含会话 ID、实体身份、NPC 定义 ID、事件/波次 ID 和幂等键。实体槽位、网络 ID、玩家账户 ID、持久化世界 ID 不能互相代用。

### 4.1 与“投射物与专用实体行为”的交接

主系统文档的第 10 项不是世界会话的子目录。它拥有短生命周期实体的归属、位置、速度、AI、寿命、穿透、命中免疫和伤害资格；世界会话只提供只读规则、时间/天气、事件进度和 Ready/区段边界。反向写入必须通过已解析的领域事实，而不能让投射物或专用实体直接改 Boss、救援 NPC 或入侵字段。

```text
Projectile intent / movement / collision
  -> ProjectileDamageResolution (hit fact only)
  -> DamageResolutionSystem + DeathResolutionSystem
  -> NpcDeathResolved
  -> ProgressQualificationQuery (read-only WorldSession)
  -> ProgressCommitCommand
  -> WorldEventProgressState
```

| 子域 | 实际代码证据 | 所有权与最小 seam | 与世界会话的边界 / 证据状态 |
| --- | --- | --- | --- |
| 投射物核心 | `Terraria/Projectile.cs:96-270` 声明 `owner`、`ai`、`timeLeft`、`damage`、`penetrate` 和本地命中免疫；`:10220-10367` 分配槽位并写入初始状态；`:11519-11801` 做伤害资格、NPC 遍历、命中和 `Kill()` | `ProjectileBehavior` 持有实体状态；`ProjectileHitResolved` 是给战斗层的事实 seam | 不能直接写 `NPC.downed*` 或 `Main.invasion*`。tModLoader API 的 `Projectile` 公共字段需在实际迁移批次按成员锚点单独核对；此处以 Version4 真实写入路径确认所有权。`confirmed`。 |
| 高尔夫 | `Terraria/Main.cs:1030,11338,12386` 把 `LocalGolfState` 作为本地 Tick 和镜头跟踪调用；`Terraria.GameContent.Golf/GolfState.cs:7-88` 仅持有最后击中的球、计分计时和镜头跟踪缓存 | `GolfSimulation` 的球物理应读取投射物/空间状态；现有 `LocalGolfState` 应归 `GolfCameraTrackingProjection`，而非权威会话状态 | 世界清理时可消费 `WorldCleared` 以释放本地跟踪缓存，但计分缓存、`Main.myPlayer` 和镜头位置不能进入存档、网络或 `WorldSession`。`confirmed` 为本地表现归属；球物理完整迁移为 `partial`。 |
| 鱼漂与钓鱼 | `Projectile.cs:102-104,4250-4253,9154-9158` 把 `bobber` 设为投射物实例状态；`:25363-25366` 由 `aiStyle == 61` 路由到鱼漂 AI；`:47779-47785` 将已判定物品交给 owner | `FishingBobberState` 是投射物/玩家交互状态；`FishingCatchResolved` 才能交给物品事务边界 | `AI_061_FishingBobber` 在该 Version4 副本中为空体（`:34073-34074`），故不得把鱼获概率、天气资格或奖励写入会话迁移的已确认集合。世界时间/天气和规则仅作为只读 Query 输入。`partial`。 |
| 传送门 | `Terraria.GameContent/PortalHelper.cs:12-66` 每 tick 从投射物槽位重建 portal 索引并递减实体冷却；`:111-216` 用 Tile 碰撞、实体速度和冷却执行穿越；`Projectile.cs:16051-16054,30357-30381` 放置/同步区段并检查支撑 Tile | `PortalNetworkState` 是由投射物、`TileMapStore`、实体位置与短冷却组成的空间规则；`PortalTraversalCommand` 是对移动系统的显式输出 | `SyncPortalSections`（`PortalHelper.cs:295-335`）是网络/区段投影效果，不能反向成为会话真值；portal 索引和冷却是每 tick 派生/暂态，不能放进 `WorldEventProgressState`。`confirmed` 的边界，具体放置实现因 `AddPortal` 为空为 `partial`。 |
| 牵引实体 | `Terraria.GameContent/LeashedEntity.cs:15-172` 以区段维护实体列表、激活/反激活和全量同步；`:202-253` 只更新活跃区段的实体，并按失活移除；`Main.cs:3347,11572` 注册原型并在 Tick 调用 | `LeashedEntitySimulation` 应拥有按 `WorldSectionCoordinate` 的注册关系、锚点、活跃状态和实体行为；网络序列化留在 Adapter/Projection | `Clear` 可响应 `WorldCleared`，但区段列表、`whoAmI`、网络包和个体行为不属于会话长期进度。`Remove`、`StreamNetUpdates`、`Spawn`、`Despawn`、`Update` 等在该副本为空体，故行为迁移为 `partial`。 |

这条边界也解释了为何 `projectileIdentity[,]` 必须留在 `WorldStorage` 的实体槽位/身份层：它帮助定位短生命周期投射物，却既不是持久化世界身份，也不是 Boss 或入侵进度。任何专用实体因死亡、交互或事件条件确实改变世界时，都必须发布带实体身份、会话 ID、定义 ID、原因和幂等键的领域事实，再由对应的资格 Query 与 Command 提交器决定是否更改会话。

## 5. 模块契约与调用方向

| 模块 | 最小接口 / seam | 写入者 | 外部边界 |
| --- | --- | --- | --- |
| 会话生命周期 | `IWorldSessionView`、`SessionReadyQuery`、`WorldLoaded/WorldCleared` | `SessionLifecycleSystem` | 生成/加载门控；失败时不半写 |
| 描述 | `IWorldDescriptor`、`ValidateBounds` | 生命周期/加载 | WorldFile、Section、WorldStorage 只读 |
| 规则 | `IWorldRulesView`、`IWorldRuleCommand` | Rule Command Handler | 生成/掉落/刷怪 Query 只读 |
| 时钟天气 | `IWorldClockView`、`AdvanceTick`、`StartRain`、`StopRain` | Clock/Weather System | 网络、UI、表现仅投影 |
| 事件进度 | `IWorldProgressView`、`ProgressCommitCommand` | EventScheduler、InvasionSystem、ProgressCommitSystem | WorldFile/NetMessage 仅快照 |
| 刷怪压力 | `ISpawnPressureView`、`CanSpawn`、`ConsumeWaveBudget` | SpawnPressure System | 人口和区域资格经纯 Query 计算 |
| 存档 | `Load(WorldSessionSnapshot)`、`Save(snapshot)` | `WorldFileSessionAdapter` | 版本迁移和 temp staging 在 Adapter 内 |
| 网络 | `WorldStateSnapshot`、`InvasionProgressMessage` | Projection；入站为 validator | Command 先验会话、权限、序号与速率 |

### 5.1 模块深度、替换 seam 与数据流

这里的 `Interface` 是迁移期间的最小稳定端口，不表示 Version4 当前已经存在这些接口；`Implementation`
是建议的唯一副作用拥有者。`Depth` 描述端口后隐藏的规则和一致性，`Leverage` 描述可替换实现或测试
替身带来的收益，`Locality` 描述状态是否能在本模块内闭合。

| 模块 | Interface | Implementation | Seam | Input / Output | Command / Event Direction | Depth / Leverage / Locality |
| --- | --- | --- | --- | --- | --- | --- |
| `SessionLifecycleSystem` | `IWorldSessionView`, `SessionReadyQuery` | 生命周期状态机和 `WorldLoaded/WorldCleared` 发布器 | `IWorldLoadPort`, `IWorldResetPort` | 输入：load/generate/reset 结果；输出：phase view、生命周期事件 | 外部结果 -> System；System -> session event；禁止 Adapter 直接写组件 | 深：门控、失败回滚和逐字段清理；高：可用 fake load/reset 测完整生命周期；局部：只拥有 phase 和会话根 |
| `WorldDescriptorState` | `IWorldDescriptor`, `ValidateBounds` | 描述值对象与不可变 Ready 快照 | `IWorldDescriptorSource` | 输入：WorldFileData/生成结果；输出：尺寸、边界、锚点和身份视图 | load/generation command -> descriptor；descriptor -> storage/查询只读事件 | 深：边界和尺寸不变量；高：可替换文件头/生成器；局部：几何和身份元数据闭合 |
| `WorldRulesState` | `IWorldRulesView`, `IWorldRuleCommand` | 规则命令处理器 | `IRulesPersistencePort`, `IRulesRandomSource` | 输入：难度/种子/感染命令；输出：规则快照和资格查询输入 | command -> rule system -> `RulesChanged`；Query 只读 | 深：hardmode、邪恶、感染和生成规则一致性；高：可注入规则来源；局部：不持有实体或 Tile |
| `WorldTimeWeatherState` | `IWorldClockView`, `AdvanceTick`, `StartRain`, `StopRain` | `WorldClockSystem`、`WeatherSystem` | `ITimeSource`, `IRandomSource` | 输入：tick、暂停、天气命令；输出：时间/天气快照 | tick/input -> system -> `DayBoundary`/`WeatherChanged`；投影只读 | 深：昼夜边界、雨风和重放；高：确定性时钟/随机替身；局部：权威时间与天气缓存分层 |
| `WorldEventProgressState` | `IWorldProgressView`, `ProgressCommitCommand` | `WorldEventScheduler`、`ProgressCommitSystem`、`InvasionSystem` | `IProgressPersistencePort`, `IProgressEventSink` | 输入：昼夜边界、死亡事实、入侵命令；输出：幂等进度快照、进度事件 | `NpcDeathResolved`/validated command -> qualification -> commit -> projection | 深：Boss/塔/入侵/解锁不变量；高：可单测重复提交和网络重投；局部：不拥有实体生命或归因账本 |
| `WorldSpawnPressureSystem` | `ISpawnPressureView`, `CanSpawn`, `ConsumeWaveBudget` | 刷怪周期/预算系统 | `IPopulationQuery`, `IRegionEligibilityQuery` | 输入：tick、事件、人口/区位 Query；输出：预算和可刷资格 | Query -> spawn system -> `SpawnBudgetChanged`；不缓存人口真值 | 中：节流和预算规则；中高：可替换人口/区域查询；局部：只拥有预算，不拥有实体集合 |
| `WorldFileSessionAdapter` | `Load(WorldSessionSnapshot)`, `Save(snapshot)` | 版本化存档适配器与 temp staging | `IWorldFileCodec`, `ISnapshotMigration` | 输入：文件/快照；输出：版本化快照或失败；不直接返回可变全局 | file I/O -> adapter -> load command；session -> immutable snapshot -> file I/O | 深：旧字段顺序、迁移和原子失败；高：可用截断/旧版本 codec 测试；局部：I/O 副作用隔离 |
| `WorldNetworkProjection` / `NetworkCommandAdapter` | `WorldStateSnapshot`, `InvasionProgressMessage`, `ValidateCommand` | 包 7/78 投影器和入站验证器 | `INetworkTransport`, `IAuthorizationPolicy`, `ISequenceClock` | 输入：不可变会话快照/网络包；输出：投影消息或已验证命令 | session -> projection -> transport；transport -> validator -> command，禁止反向直写 | 深：协议兼容、权限、序号和限流；高：可用无网络 projection 测试；局部：网络类型停留在 Adapter 边界 |

调用方向固定为：`外部输入 -> Adapter/Validator -> Command -> System -> Component`，以及
`Component -> Query/Projection -> 外部输出`。任何 `Query` 都不能持有可变状态；任何 `Projection`
都不能成为下一 tick 的权威输入。

建议显式顺序（来自现有 `Main` 的可观察更新链，不用文件顺序表达）：

1. `SessionLifecycleSystem` 门控 Ready/Loading/Generating。
2. 实体 AI、输入、移动和战斗产生 intent/事实，不能直接改会话。
3. `WorldClockSystem`、`WeatherSystem` 推进时间、昼夜、雨和风。
4. `WorldEventScheduler` 消费昼夜边界和事件时钟。
5. `WorldSpawnPressureSystem` 更新周期和预算；人口仍按 Query 计算。
6. `WorldEcologySystem` 调用生态/感染/Tile 相关适配边界。
7. `InvasionSystem` 和 `ProgressCommitSystem` 提交入侵、Boss、解锁等进度。
8. 提交屏障后，Persistence、Network、Presentation 从不可变快照投影；失败不得回写玩法状态。

## 6. SS14 仅作组织证据

| SS14 文件 | 可迁移的组织模式 | 不迁移的内容 |
| --- | --- | --- |
| `Content.Server/GameTicking/GameTicker.cs:32-135` | 生命周期、Update、Shutdown 和依赖是 System 所有物 | 不采用回合语义或名称 |
| `Content.Shared/GameTicking/SharedGameTicker.cs:14-60` | 只读时间/ID 和纯时长查询分离 | 不复制网络协议 |
| `Content.Server/GameTicking/GameTicker.RoundFlow.cs:53-72,363-418` | 私有阶段状态经单一 setter 发事件；启动顺序可观察 | 不将 Terraria 世界变成 SS14 round |
| `Content.Server/GameTicking/GameTicker.GameRule.cs:26-40,88-212` | 临时规则有独立组件、生命周期事件和 System | Boss 旗标不变成 GameRuleComponent |
| `Content.Shared/Station/Components/StationDataComponent.cs:7-31` | 内聚状态、实体关系和写入限制集中 | 不迁移 station 字段 |

由此仅得到：组件按访问模式/生命周期分组；System 拥有副作用；Query 纯读；网络/存档是投影；实体、地图、网络和账户身份分离。SS14 不能证明 Terraria 字段语义，后者只由 Version4 源码确认。

### 6.1 SS14 世界会话、进度与存储的直接代码索引

为满足“查找所有相关代码”的证据要求，下面列出本次在参考仓库中实际阅读的直接入口；其余命中项是这些入口的消费者或 UI，不承担会话状态所有权。

| 参考代码 | 直接观察到的职责 | Version4 拆分映射 |
| --- | --- | --- |
| `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\GameTicking\SharedGameTicker.cs:14-60` | 只读 `RoundId`、`RoundStartTimeSpan`、`RoundDuration()` 和录制元数据 | `WorldSessionIdentityState`、`SessionClockQuery`、只读投影 |
| `...\Content.Server\GameTicking\GameTicker.cs:32-131` | System 的依赖注入、Initialize/Update/Shutdown 生命周期 | `SessionLifecycleSystem` |
| `...\Content.Server\GameTicking\GameTicker.RoundFlow.cs:49-83,351-469,478-506,650-785` | 阶段状态 setter 发事件；回合开始、结束、重启、地图预加载和清理顺序 | `SessionPhaseState`、`WorldLoaded/WorldCleared`、显式调度依赖 |
| `...\Content.Server\GameTicking\GameTicker.Lobby.cs:12-180` | `NetUserId -> PlayerGameStatus` 表、暂停/倒计时和网络状态消息 | `SessionLobbyState`；账户 ID 不与实体/网络 ID 合并 |
| `...\Content.Server\GameTicking\GameTicker.GamePreset.cs:19-180` | 待用/当前/伪装 preset、地图池资格和启动失败回退 | `WorldRulesSelectionState` + 资格 Query |
| `...\Content.Server\GameTicking\GameTicker.GameRule.cs:26-40,88-212,220-403` | 动态规则实体、Added/Started/Ended 生命周期、相对回合时间历史 | `WorldEventProgressState.ActiveRules` 关系，不做巨型旗标 |
| `...\Content.Server\Maps\IGameMapManager.cs:8-76` | 地图列举、资格、选择、轮换和存在性最小接口 | `WorldMapSelectionPort` |
| `...\Content.Server\Maps\GameMapManager.cs:16-244` | 配置地图、选中地图、轮换队列；`UsePersistence` 将保存路径绑定到地图原型 | `WorldStorageMapCatalogAdapter`，与 Tile 存储分离 |
| `...\Content.Server\Administration\Commands\PersistenceSaveCommand.cs:11-50` | 校验 `MapId` 和路径后调用 `MapLoaderSystem.TrySaveMap` | `PersistenceCommandAdapter`，命令不能直写组件 |
| `...\Content.Server\Mapping\MappingSystem.cs:16-129` | 自动保存调度器；明确将计时缓存留在 System，按 `EntityUid` 保存 map/grid | `WorldAutosaveSystem` 私有缓存，不进入存档/网络 |
| `...\Content.Server\GameTicking\GameTicker.Player.cs:178-213` | 以 `RoundId` 与 `NetUserId` 记录回合参与者 | `RoundParticipantProjection` |
| `...\Content.Server\Database\ServerDbManager.cs:29-62,137-174` | 用户 ID、偏好、游玩时间、连接日志的异步数据库端口 | `AccountIdentityAdapter`，数据库类型不渗透会话组件 |
| `...\Content.IntegrationTests\Tests\SaveLoadMapTest.cs`、`SaveLoadSaveTest.cs`、`RestartRoundTest.cs` | 地图 Save/Load 与回合重启集成验证入口 | Version4 focused verifier 的参考形态 |

SS14 代码中 `EntityUid`（实体/地图拥有者）、`MapId`（地图身份）、`NetEntity`（网络实体）和 `NetUserId`（账户）由不同 API 传递；这与 Version4 `Main` 中数组槽位、`projectileIdentity[,]`、`WorldFileData.WorldId/UniqueId` 的分离要求一致。该观察只用于身份建模和副作用隔离，不迁移 SS14 的 station 或 round 语义。

## 7. 世界存储与身份交叉边界

主系统文档第 3 项 `WorldStorage` 必须与本报告的 `WorldSession` 分根：

| 现有数据 | Version4 证据 | 归属 | 会话交互 |
| --- | --- | --- | --- |
| `WorldMap`、`Tile[,]`、物品/NPC/箱子/标牌数组、`projectileIdentity[,]` | `Terraria/Main.cs:926-952` | `TileMapStore`、`EntitySlotStore<T>`、`ContainerStore` | 会话只读提供尺寸/区段边界 |
| 区段 Loaded/Framed/MapDrawn/NeedsRefresh 位和 `BitsByte[]` | `Terraria/WorldSections.cs:35-75` | `WorldSectionState` + `SectionStreamingSystem` | 由 Ready/Unload 门控，不进入 Boss/天气快照 |
| `WorldId`、`UniqueId`、种子、文件键和元数据 | `Terraria.IO/WorldFileData.cs:21-118` | `WorldPersistenceIdentity` + `WorldDescriptorState` | 持久化 ID 不得冒充网络实体 ID |
| header、tiles、chests、signs、NPC、TileEntity 存档块 | `Terraria.IO/WorldFile.cs:1186-1204,1263-1466` | `WorldSnapshotProjection` + 版本化 Adapter | Snapshot 只读收集，加载失败原子回滚 |

禁止 `WorldFile`/`NetMessage` 适配器直接写 `Main.tile`、`Main.npc` 或 `NPC.downed*`；结构变更经 Command 提交到 `WorldStorage`，时间/事件/进度经对应 System 写入 `WorldSession`，提交后再生成网络和持久化投影。

### 7.1 身份分层建议

下表中的值对象是拆分建议，不声称 Version4 当前已经定义同名类型。SS14 的 `EntityUid`、`MapId`、
`NetEntity` 和 `NetUserId` 只证明身份应分层，不能替代 Version4 的字段语义。

| 概念 | 推荐值对象 / 索引 | 当前证据 | 禁止替代 |
| --- | --- | --- | --- |
| 实体槽位 | `EntitySlot<T>` | `Main.item[]`、`Main.npc[]`、`projectileIdentity[,]`（`Main.cs:926-952`） | 不作为存档或网络身份；槽位复用后旧引用必须失效 |
| 持久化世界 | `WorldPersistentId`（由 `WorldId` / `UniqueId` 承载） | `WorldFileData.cs:21-118`；本地 API `class_world_file_data.html#a6baa7dd2c616a7aca803c1d85fb26627` | 不作为实体、账户或连接身份 |
| 网络世界 / 网络实体 | `NetworkWorldId` / `NetworkEntityId` | `NetMessage.cs:221-403,1158-1163` 是投影边界；具体值对象尚待 Version4 网络架构定义 | 不从数组 slot、持久化 GUID 或账户 ID 推导 |
| 账户 | `AccountId` | SS14 `NetUserId` 组织证据；Version4 玩家会话路径需由目标实现补充 | 不附着世界 Boss/入侵进度，也不作为实体身份 |
| 地图区段 | `WorldSectionCoordinate` | `WorldSections.cs:35-75`；本地 API `class_world_sections.html#a0e4dda8444fc442ea74648d12c7b0e14` | 不使用实体 slot、网络实体 ID 或账户 ID 代替 |

## 8. 兼容、风险与不拆分项

- 第一阶段添加新组件和只读 View；旧 `Main.*` / `NPC.*` 只作为单向 facade，禁止双写。
- 第二阶段把 `WorldFile` 改为 `WorldSessionSnapshot` Adapter，保持旧二进制字段顺序、版本迁移和加载原子性。
- 第三阶段把包 7/78 变为只读 projection；`MessageBuffer` 仅能把已验证命令提交给会话。
- 迁移顺序为 Clock → EventScheduler → SpawnPressure → Ecology → Invasion → Projection；每迁一个边界就移除该边界的旧写入者。
- 不拆每一个 Boss 标志为实体：它们同生命周期、同存档、同清理，保留为 `BossProgressFlags` 子结构。
- 不把当前风速、`cloudAlpha`、`invasionProgressAlpha`、`invasionProgressDisplayLeft` 或其他 UI/帧值持久化。
- `WorldGen.clearWorld` 横跨多类静态状态，须用 `WorldCleared` 统一重置并逐字段验证。
- 事件类和入侵归因存在空实现，报告对这些项只作 `partial/missing` 分级，不宣称行为闭合。

### 8.1 不拆分反例与保留理由

| 不采用的拆法 | 反例 / 增加的耦合 | 保留决策 |
| --- | --- | --- |
| 每个 Boss 旗标一个 Entity 或组件 | 存档、清理、快照和读者集完全重合；三机械 Boss 等聚合规则会变成跨实体事务 | 保留 `BossProgressFlags` 等值结构，由一个 `ProgressCommitSystem` 维护幂等不变量 |
| 把 `WorldStorage` 塞进 `WorldSession` | Tile/slot/section 的容量、更新频率、复制和结构变更边界不同，会让天气或 Boss 更新携带巨大存储依赖 | 两个根分离；Session 只提供尺寸/区段视图，Storage 拥有地图和槽位写入 |
| `worldSurface` 与 `rockLayer` 各自独立组件 | 生成/加载在同一阶段写入，二者共同描述地层边界；独立写入会允许不相容的地层快照 | 合并为 `WorldDescriptorState` 的地层边界值对象 |
| 持久化 `cloudAlpha`、`invasionProgressAlpha`、`windSpeedCurrent` | 这些是表现或缓存值，存在帧级失效条件；存档后会把客户端/历史状态误当世界真值 | 只持久化其权威输入；表现值由 Projection/Query 重建 |
| 缓存玩家人口、区域资格为世界权威状态 | 玩家连接、位置、区段和权限变化频繁；缓存失效会错误放行或拒绝刷怪 | 由 `IPopulationQuery`、`IRegionEligibilityQuery` 每次按需派生 |
| 直接复制 SS14 `GameTicker` / `GameRule` 名称和 station/round 语义 | SS14 的回合和站点生命周期不是 Terraria 世界、Boss 或生成器语义；名称复制会隐藏真实来源 | 仅借用 System/Query/Projection 组织形态，领域词汇保持 Version4/Terraria 语义 |

## 9. Focused verifier 计划与实际结果

| Verifier | 要验证的状态转换 / 错误边界 | 当前实际结果 |
| --- | --- | --- |
| Session lifecycle | loading/generating/ready/unload/reset；加载失败不能留下半写状态 | 未实现、未运行；当前只有源码证据和接口计划 |
| Time / weather | day/night 边界、暂停/快进、雨风随机源重放、缓存失效 | 未实现、未运行；`dayTime/time` 语义已由源码和离线 API 交叉确认 |
| Progress | invasion start/move/wave/complete；Boss/塔/Lunar 重复提交幂等 | 未实现、未运行；标准入侵与公开 Boss 进度语义已确认，DD2/空算法仍为 `partial` |
| Snapshot | Save -> Load round-trip；旧版本、截断文件和 temp staging 原子失败 | 未实现、未运行；仅确认 `WorldFile` 的读写边界 |
| Network | 包 7/78 projection；错误会话 ID、权限、序号、频率和重复命令拒绝 | 未实现、未运行；仅完成投影/入站写者盘点 |
| Attribution handoff | `NpcDeathResolved -> ProgressQualificationQuery -> ProgressCommitCommand`；复合 Boss、重复死亡和未闭合入侵贡献筛选 | 未实现、未运行；`NPCDamageTracker`/`InvasionDamageTracker` 的空实现已记录为风险 |
| Order | `Clock -> Event -> SpawnPressure -> Ecology -> Invasion/Progress -> Projection` 的显式调度约束 | 未实现、未运行；顺序来自 `Main` 可观察调用链，未被测试锁定 |

本次工作只做源码、离线 API 和参考 ECS 的架构证据盘点，未改动 `D:\TRbackup\Version4`、
`C:\Users\shan\Downloads\ECS\space-station-14-master` 或 `D:\TRbackup\tmodloader-api-docs-stable`，
也未运行 Version4 或 SS14 的编译/回归测试。因此这是有证据边界、可实施的组件化拆分报告，不是迁移完成、
编译通过或运行时回归通过的声明。
