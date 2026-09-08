# Version4 与 tModLoader 扩展面补充子系统审查

## 结论

本笔记只补充第一轮“子系统”清单，不把单个 TileEntity、Hook、消息号、AI style 或状态字段提升
为子系统。对照现有的
[`Version4 权威游戏模拟子系统审查报告`](../Version4权威游戏模拟子系统审查报告.md)，本轮找到
6 个需要显式命名、或从已有宽泛责任面中提升出来的权威责任面：

| 候选子系统 | 结论 | NLTX 当前状态 | 原因 |
| --- | --- | --- | --- |
| `TeleportationAndTraversal` | **新增一级子系统** | `partial` | 位置迁移、落点/晶塔资格、跨实体冷却、区段复制共同构成独立生命周期。 |
| `FishingAndCatchSimulation` | **新增一级子系统** | `partial` | 它不是普通 Projectile 或 Loot 的细节，而是从玩家能力、环境资格、浮标时序到 Item/NPC 结果提交的一条独立业务链。 |
| `WorldProgressionAndTransition` | **从 WorldSession/WorldGenerationAndEcology 中提升为显式子系统** | `partial` | Hardmode 是长事务式世界转换，改写世界规则、地形任务、同步与后续生态，不能只作为 `HardMode` 布尔字段。 |
| `WorldCalendarAndEventOrchestration` | **从 WorldSession 中提升为显式子系统** | `partial` | 日夜边界和定时事件拥有独立时钟、门控、随机资格、NPC/刷怪后果和网络公告。 |
| `SimulationRuleOverrides` | **新增一级子系统** | `missing` | Journey/Creative 权限可以改变 Tick 速率、天气、生态、生成率和难度；这不是 UI 或内容排序。 |
| `WorldProgressionAndUnlocks` | **新增一级子系统** | `missing` | Bestiary 等可持久化进度会反向改变城镇 NPC 可生成资格；成就/社交只作为投影或适配器。 |

建议把这 6 个名称加入主报告的后续总表。`WorldProgressionAndTransition` 和
`WorldCalendarAndEventOrchestration` 可以先仍放在 `WorldSession` 项目中；“显式子系统”表示必须有
自己的阶段契约、写入根和 verifier，不要求立即建立 6 个新程序集。

## 判定方法

候选项目至少需要同时满足以下 3 项，才进入上表：

1. Version4 有独立状态生命周期，不只是读取一个组件。
2. 它的结果跨越至少两个既有领域，例如 Player、Tile、NPC、Item、WorldSession 或网络复制。
3. tModLoader 的服务端 Hook、持久化或复制面证实它需要独立的权限、顺序或恢复边界。

`confirmed` 表示 Version4 的真实调用链已足以证明该子系统存在；`partial` 表示 NLTX 已有状态或
目录，但没有闭合执行链；`missing` 表示 NLTX 只有内容/表现元数据，或没有相应的权威模型。

## 1. TeleportationAndTraversal

**判定：新增；Version4 证据为 confirmed，NLTX 为 partial。**

这不是 `PlayerGameplay` 中的一种移动动作，也不是 `SpatialSimulation` 中的一次坐标写入。它统一了：

- 门户成对登记、失效和每实体冷却；
- 晶塔的 TileEntity 派生登记、资格检查和列表增量；
- 目的地空间可用性、位置/速度变换和失败不提交；
- 玩家加入时的门户/晶塔快照，以及移动后区段同步和位置确认。

Version4 中 `PortalHelper.UpdatePortalPoints` 每 Tick 重建门户配对并递减玩家/NPC 冷却
([`PortalHelper.cs:64-107`](D:/TRbackup/Version4/Terraria.GameContent/PortalHelper.cs:64))；
`TryGoingThroughPortals` 做线段碰撞、出口四向空间验证、速度重定向、Player/NPC 传送和网络广播
([`PortalHelper.cs:109-226`](D:/TRbackup/Version4/Terraria.GameContent/PortalHelper.cs:109))。加入玩家的
`SyncPortalsOnPlayerJoin` 和 `SyncPortalSections` 又明确把地图区段复制作为该链的一部分
([`PortalHelper.cs:293-339`](D:/TRbackup/Version4/Terraria.GameContent/PortalHelper.cs:293))。

晶塔并非单一 TileEntity 细节。`TeleportPylonsSystem` 由 TileEntity 列表派生晶塔注册表、对集合差集
发出添加/移除网络包，并在玩家加入时推送当前快照
([`TeleportPylonsSystem.cs:25-90`](D:/TRbackup/Version4/Terraria.GameContent/TeleportPylonsSystem.cs:25))。
tModLoader 同样把 `ModPylon.ValidTeleportCheck` 明确为有顺序的资格流程
([`class_mod_pylon.html:703-705`](D:/TRbackup/tmodloader-api-docs-stable/class_mod_pylon.html:703))，其中 NPC
数量、危险事件和 Biome 分别属于不同验证阶段
([`:1308-1339`](D:/TRbackup/tmodloader-api-docs-stable/class_mod_pylon.html:1308)、
[`class_mod_pylon.html:1353-1379`](D:/TRbackup/tmodloader-api-docs-stable/class_mod_pylon.html:1353)、
[`class_mod_pylon.html:1504-1539`](D:/TRbackup/tmodloader-api-docs-stable/class_mod_pylon.html:1504))。

NLTX 已有 `PortalNetworkState`、`PortalEndpointComponent`、`TeleportCooldownState`
([`src/Teleportation`](../../src/Teleportation)) 与 `PylonRegistryState`
([`src/WorldStorage/PylonRegistryState.cs`](../../src/WorldStorage/PylonRegistryState.cs))，证明领域已被识别；
但没有资格查询、落点查询、传送命令、门户遍历 System 或复制 Adapter。因此状态为 `partial`。

最小边界为：

```text
Teleport intent
  -> qualification and landing queries
  -> teleport/traversal command
  -> Player or NPC spatial state + cooldown + pylon/portal registry commit
  -> section and entity replication projection
```

## 2. FishingAndCatchSimulation

**判定：新增；Version4 证据为 confirmed 的调用位置与 missing 的核心实现，NLTX 为 partial。**

钓鱼应独立于 `ProjectileSimulation` 和 `ItemContainerAndEconomy`：浮标是它的时间载体，但钓获结果同时
可能生成物品或 NPC，并依赖玩家能力、液体、天气、时间、Biome、任务和内容掉落规则。

Version4 初始化 `FishDropsDB`，并每日轮换任务鱼
([`Main.cs:1612-1674`](D:/TRbackup/Version4/Terraria/Main.cs:1612)、
[`Main.cs:3359-3364`](D:/TRbackup/Version4/Terraria/Main.cs:3359))。Projectile 更新明确进入
`AI_061_FishingBobber`，并在钓获分支调用 `AI_061_FishingBobber_GiveItemToPlayer`，但这两个核心方法在
当前参考代码为空
([`Projectile.cs:25365`](D:/TRbackup/Version4/Terraria/Projectile.cs:25365)、
[`Projectile.cs:34073-34074`](D:/TRbackup/Version4/Terraria/Projectile.cs:34073)、
[`Projectile.cs:47784`](D:/TRbackup/Version4/Terraria/Projectile.cs:47784))。这表示功能链的边界明确，
但 Version4 不能证明完整算法。

tModLoader 的 `ModPlayer.ModifyFishingAttempt` 明确区分“已收集资格但未决定结果”的阶段和最终
`CatchFish` 结果阶段
([`class_mod_player.html:2604-2632`](D:/TRbackup/tmodloader-api-docs-stable/class_mod_player.html:2604)、
[`class_mod_player.html:1636-1692`](D:/TRbackup/tmodloader-api-docs-stable/class_mod_player.html:1636))。这支持将
“资格计算”和“结果提交”保留为同一子系统内的两个阶段，而不是让 ContentCatalog 或 Projectile 直接写库存。

NLTX 已有 `PlayerFishingCapabilityState`
([`src/Player/PlayerFishingCapabilityState.cs`](../../src/Player/PlayerFishingCapabilityState.cs)) 和
`FishingDropRuleCatalog` ([`src/Content/FishingDropRuleCatalog.cs`](../../src/Content/FishingDropRuleCatalog.cs))，
但无浮标状态机、环境资格查询、鱼饵/耐久事务、Catch 命令或 Item/NPC 结果提交，所以为 `partial`。

## 3. WorldProgressionAndTransition

**判定：从既有世界生成/生态责任面提升；Version4 证据为 confirmed，NLTX 为 partial。**

`HardMode` 不是普通 `WorldRulesState.HardMode` 修改。Version4 的 `WorldGen.StartHardmode` 先改变
世界模式和已生成物品保护，再执行后台世界转换，结束后广播、触发进度、重置区段并撤销保护
([`WorldGen.cs:26084-26100`](D:/TRbackup/Version4/Terraria/WorldGen.cs:26084))。它的后果还会进入
后续事件、掉落和生态更新。tModLoader 将这件事暴露为可修改的 Hardmode 任务序列，而不是一般 Tick
([`class_mod_system.html:139-140`](D:/TRbackup/tmodloader-api-docs-stable/class_mod_system.html:139))。

因此该子系统应拥有“触发资格 -> 过渡计划 -> 原子世界修改 -> 世界规则提交 -> 区段重同步”的长事务，
并与 `WorldGenerationAndEcology` 共用生成 Pass 和 Tile 提交能力，但不能被其常规采样循环吞没。

NLTX 已有 `WorldRulesState.HardMode`、矿阶和感染规则
([`src/WorldSession/WorldSessionComponents.cs`](../../src/WorldSession/WorldSessionComponents.cs))，以及生成生命周期
([`src/WorldSession/WorldGeneration/WorldGenerationLifecycleState.cs`](../../src/WorldSession/WorldGeneration/WorldGenerationLifecycleState.cs))，
但没有过渡计划、后台/预算语义、提交屏障、区段重同步或 verifier，故为 `partial`。

## 4. WorldCalendarAndEventOrchestration

**判定：从 WorldSession 中提升；Version4 证据为 confirmed，NLTX 为 partial。**

现有主报告把时间、天气和长期进度列入 `WorldSession`，但 Version4 表明日历事件自身已有独立运行时：
`Main.UpdateTime` 更新雨、史莱姆雨和时间，并统一调用 Cultist、生日派对、灯笼夜、沙暴、DD2、
Credits、世界日志及晶塔更新 ([`Main.cs:12972-13125`](D:/TRbackup/Version4/Terraria/Main.cs:12972))；昼夜边界还会
轮换渔夫任务、评估事件、启动入侵和触发城镇 NPC 生成
([`Main.cs:13470-13564`](D:/TRbackup/Version4/Terraria/Main.cs:13470))。

它应拥有世界时钟、边界事件、随机资格和活动实例的生命周期；`NpcAndTownSimulation`、
`SpawnLifecycleAndLoot` 与 `WorldGenerationAndEcology` 只消费经提交的事件事实。tModLoader 的
`PostUpdateTime` 与仅在服务器/单人端运行的 `PostUpdateWorld` 也表明时间和权威世界更新需要分开排序
([`class_mod_system.html:253-259`](D:/TRbackup/tmodloader-api-docs-stable/class_mod_system.html:253))。

NLTX 的 `WorldTimeWeatherState`、`WorldEventProgressState`、入侵/DD2/派对/灯笼夜/沙暴状态已经存在
([`src/WorldSession/WorldSessionComponents.cs`](../../src/WorldSession/WorldSessionComponents.cs))，但没有事件调度器、
资格查询或跨领域提交链，故为 `partial`。

## 5. SimulationRuleOverrides

**判定：新增；Version4 证据为 confirmed，NLTX 为 missing。**

Journey/Creative 不能归入 UI、内容 Presentation 或一般存档适配器。`CreativePowerManager` 注册的权威
Power 包括冻结/加速时间、天气和风、放置范围、难度、生态传播和刷怪率
([`CreativePowerManager.cs:92-110`](D:/TRbackup/Version4/Terraria.GameContent.Creative/CreativePowerManager.cs:92))；
它还拥有按世界保存/加载以及玩家加入权限同步
([`CreativePowerManager.cs:132-197`](D:/TRbackup/Version4/Terraria.GameContent.Creative/CreativePowerManager.cs:132))。

这些规则实际改变模拟：`UpdateTimeRate` 读取时间 Power
([`Main.cs:3159-3175`](D:/TRbackup/Version4/Terraria/Main.cs:3159))，难度滑块写入世界难度覆写
([`Main.cs:11364-11374`](D:/TRbackup/Version4/Terraria/Main.cs:11364))，生态更新读取停止传播 Power
([`WorldGen.cs:59408-59415`](D:/TRbackup/Version4/Terraria/WorldGen.cs:59408))。因此它应提供带权限、来源、
版本与失效语义的只读规则快照，供 Tick 阶段读取；不得允许各领域直接访问可变 Power 管理器。

NLTX 仅有 Creative 排序的 Presentation 元数据
([`src/Content/ContentPresentationIndex.cs`](../../src/Content/ContentPresentationIndex.cs))，没有权限、世界/玩家
覆盖状态、命令、恢复或网络边界，状态为 `missing`。

## 6. WorldProgressionAndUnlocks

**判定：新增；Version4 证据为 confirmed，NLTX 为 missing。**

本子系统只包含会改变权威游戏资格的持久化进度，例如 Bestiary 的击杀、目击与交谈发现；不把纯平台成就、
通知音效或 Social API 作为权威状态。

Version4 的 `BestiaryUnlocksTracker` 统一保存、加载、校验、重置和玩家加入同步三类追踪器
([`BestiaryUnlocksTracker.cs:5-56`](D:/TRbackup/Version4/Terraria.GameContent.Bestiary/BestiaryUnlocksTracker.cs:5))。击杀登记
会广播更新 ([`NPCKillsTracker.cs:23-46`](D:/TRbackup/Version4/Terraria.GameContent.Bestiary/NPCKillsTracker.cs:23))，
而 Bestiary 完成比例会影响动物学家 Town NPC 的生成资格
([`Main.cs:14001-14004`](D:/TRbackup/Version4/Terraria/Main.cs:14001)、
[`Main.cs:14167-14170`](D:/TRbackup/Version4/Terraria/Main.cs:14167))。这足以证明它不只是客户端图鉴。

NLTX 目前只有 Bestiary credit ID 与显示排序等 Content 元数据
([`src/Content/ContentIdentityCatalog.cs`](../../src/Content/ContentIdentityCatalog.cs)、
[`src/Content/ContentPresentationIndex.cs`](../../src/Content/ContentPresentationIndex.cs))，没有进度存储、事件订阅、
进度查询、Town 消费接口或投影，故为 `missing`。

## 已排除的候选

| 候选 | 结论 | 证据 |
| --- | --- | --- |
| `GolfSimulation` | 不新增权威子系统 | `GolfState` 只围绕 `Main.LocalPlayer`、最后命中的本地球和相机跟随运行 ([`GolfState.cs:38-103`](D:/TRbackup/Version4/Terraria.GameContent.Golf/GolfState.cs:38))；它应留在客户端表现或未来的 Projectile 专用规则中。 |
| `SceneMetrics` / `BiomeScene` | 不新增权威状态根 | Version4 的 `SceneMetrics.Scan` 是按屏幕/视角中心的缓存扫描 ([`SceneMetrics.cs:196-230`](D:/TRbackup/Version4/Terraria/SceneMetrics.cs:196))。它可启发 `EnvironmentalQualificationQuery`，但不能作为服务器世界事实。Biome 资格仍应由 `WorldGenerationAndEcology` 的确定性只读查询提供。 |
| 单独的 `TileEntityLifecycle` | 不新增 | tModLoader 要求 TileEntity 的服务端更新、放置合法性、销毁清理和同步 ([`class_mod_tile_entity.html:217-229`](D:/TRbackup/tmodloader-api-docs-stable/class_mod_tile_entity.html:217)、[`class_mod_tile_entity.html:505-540`](D:/TRbackup/tmodloader-api-docs-stable/class_mod_tile_entity.html:505))，但这正是既有 `WorldInteractionAndStructures` + `WorldStorage` 的受控提交责任。 |
| `ProtocolRegistry` 或 `EventRegistry` | 不新增 | 网络模块、消息号、Hook 注册都是 `ExternalBoundaries` / `ContentCatalog` 的实现机制；没有独立的领域状态和业务生命周期。 |
| `Achievement` / `Social` | 不新增权威模拟子系统 | 成就管理器主要写玩家/云文件并连接 Social API；除非某项进度明确参与权威资格，应该从 `WorldProgressionAndUnlocks` 输出的事实派生为投影。 |

## 对主报告的最小修订建议

主报告无需把 14 个责任面拆成几十个组件。下一版只需：

1. 在 `SpatialSimulation` 之后增加 `TeleportationAndTraversal`，其读取空间/环境查询并提交位置、冷却和区段复制意图。
2. 在 `ProjectileSimulation`、`ItemContainerAndEconomy` 之间增加 `FishingAndCatchSimulation`，它输出 Item/NPC 结果给既有事务和生命周期提交器。
3. 从 `WorldSession` 中显式列出 `WorldCalendarAndEventOrchestration`、`WorldProgressionAndTransition` 和 `WorldProgressionAndUnlocks`；它们可暂时共用项目，但不能共用无边界的写入口。
4. 增加 `SimulationRuleOverrides`，让它只产生经过权限校验的不可变规则快照；RuntimeComposition 在 Tick 开始采样一次。
5. 不将 Golf、场景扫描、单一 TileEntity、协议注册、UI、音频或 Social API 上升为权威模拟子系统。

这些补充把初始责任面从 14 个提升到 **20 个**，仍保持“可安排独立写入根、阶段契约和 verifier 的子系统”粒度。

## 验证记录

- 读取并比对了现有主审查报告、Version4 的 `Main`、`WorldGen`、门户/晶塔、钓鱼、Creative、
  Bestiary、事件和 Golf 源码，以及本地 tModLoader API HTML 文档。
- 对 NLTX 使用了 `rg` 搜索 `src/`，并逐项读取已有 Teleportation、WorldStorage、WorldSession、
  Player 和 Content 模型，状态不以文件名推断。
- 本笔记是研究文档；未修改生产代码，因此未运行 `dotnet` 构建或测试。
