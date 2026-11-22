# Version4 非权威组件拆分分区 P02：世界环境与事件 - public-decomposition 专属提示词

> 本文件只包含当前分区的专属任务内容，不复制公共提示词正文。
> 使用时：先提供公共提示词，再附加本文件；公共协议中的通用规则、证据门槛、报告结构、禁止项和 Integration Handoff 继续有效。

## 公共前置（仅引用）

- 公共提示词：`D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-public-decomposition-common-protocol.md`
- 必读技能：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\SKILL.md`
- ECS 证据协议：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\ecs-evidence-protocol.md`
- tModLoader 检索协议：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\tmodloader-documentation-retrieval.md`
- 本文件不重复上述文件内容；执行前必须实际读取并遵守它们。
- 并行会话保障 skill：D:\\TRbackup\\NLTX\\.agents\\skills\\version4-non-authoritative-partition-session\\SKILL.md
- 并行领取/结算 runner：D:\\TRbackup\\NLTX\\Build\\Tools\\Invoke-Version4NonAuthoritativePartitionSession.ps1
- 必须先按 skill 使用 ClaimNext（或用户明确指定时使用 Claim -PartitionId P02），只写本文件的 outputReport，并用领取返回的同一 sessionId 完成 Complete/Fail/Abandon；不得手工编辑 ledger 或 lock。
- 本任务属于 20 个并行会话中的固定分区；currentTaskCount: 20。公共协议中遗留的 19 会话数量说明在本任务包中不适用。

## 任务元数据

- promptId: P02-non-authoritative-public-decomposition-20260911
- partitionId: P02
- inputPartitionReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\02-world-environment-events.md
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P02-world-environment-events-public-decomposition.md
- reportType: public-decomposition
- executionMode: independent-read-only-partition
- currentTaskCount: 20
- formalParentSubsystems: RuntimeComposition, SharedRuntimeMechanisms
- leafSubsystemCount: 17
- fieldCount: 212
- propertyCount: 13
- memberCount: 225
- sourceReportSha256: b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196
- sourceInventoryStatus: source-inventory-confirmed; member-level owner/lifecycle/side-effect evidence still requires this session to close

## 专属范围锁定

- 本会话只审查输入分区报告中的 17 个叶子子系统和 225 条成员记录。以下清单是本会话唯一的成员范围；不得读取或复制其他 P 分区的成员清单。
- 细分子系统仍只是库存边界，不等价于一个组件；允许在本分区内部按访问模式拆成多个 proposed Component、System、Query、Command、Adapter 或 Projection。
- 来源序号、路径、行号和声明是检索线索；必须回到 Version4 重新定位，不得把输入报告直接当作已确认的读者、写者、生命周期或 owner 证据。

| 叶子子系统 | 父级 | 原章节 | 边界角色 | 字段 | 属性 | 成员 | 细分职责 |
|---|---|---|---|---:|---:|---:|---|
| `MainSlimeRainState` | `RuntimeComposition` | `4.1.15` | `runtime state` | 8 | 0 | 8 | 史莱姆雨警告、槽位、计时和击杀进度。 |
| `MainCalendarWeatherState` | `RuntimeComposition` | `4.1.18` | `runtime state` | 16 | 0 | 16 | 昼夜、月相、雨、血月和日食事实。 |
| `MainWeatherAndAmbientState` | `RuntimeComposition` | `4.1.21` | `runtime state` | 11 | 0 | 11 | 星体、云层、风和环境对象状态。 |
| `MainInvasionState` | `RuntimeComposition` | `4.1.31` | `runtime state` | 12 | 0 | 12 | 入侵类型、位置、波次和进度事实。 |
| `MainSeasonalAndTitleState` | `RuntimeComposition` | `4.1.44` | `presentation state` | 7 | 0 | 7 | 节日开关、强制节日和标题切换状态。 |
| `SharedWorldEventPresentationState` | `SharedRuntimeMechanisms` | `4.9.25` | `presentation state` | 21 | 2 | 23 | 月总死亡戏剧、制作人员名单和屏幕遮挡表现状态。 |
| `SharedInvasionAndBossTracking` | `SharedRuntimeMechanisms` | `4.9.26` | `state/query` | 10 | 4 | 14 | 入侵、Boss 伤害和旗帜进度跟踪。 |
| `SharedLightningGenerationState` | `SharedRuntimeMechanisms` | `4.9.29` | `state/query` | 23 | 0 | 23 | 闪电生成参数、分叉递归和 Tile 碰撞状态。 |
| `SharedWaterfallState` | `SharedRuntimeMechanisms` | `4.9.30` | `state/presentation` | 11 | 0 | 11 | 瀑布槽位、长度限制和瀑布绘制资源状态。 |
| `WorldEnvironmentScanHelpers` | `SharedRuntimeMechanisms` | `4.9.103` | `query` | 9 | 0 | 9 | 洞察扫描、不可破坏墙扫描和虚空镜辅助查询。 |
| `SharedAmbientSkyCatalogState` | `SharedRuntimeMechanisms` | `4.9.152` | `definition/presentation` | 17 | 0 | 17 | 树冠区域、天空变体和背景闪烁定义。 |
| `SharedAmbientSpawnAndWindState` | `SharedRuntimeMechanisms` | `4.9.153` | `state/query` | 11 | 0 | 11 | 环境实体生成条件、风点和尝试计时状态。 |
| `SharedCelebrationAndLanternEvents` | `SharedRuntimeMechanisms` | `4.9.159` | `state/definition` | 14 | 2 | 16 | 派对、灯笼夜和环境精灵季节事件状态。 |
| `SharedRitualAndStormEvents` | `SharedRuntimeMechanisms` | `4.9.160` | `state/definition` | 12 | 0 | 12 | 邪教仪式和沙尘暴事件计时及强度状态。 |
| `SharedSceneWeatherAndEventZoneState` | `SharedRuntimeMechanisms` | `4.9.217` | `query/input` | 9 | 0 | 9 | 场景天气、微光、蜡烛和事件小游戏区域定义。 |
| `SharedInvasionDamageTrackingState` | `SharedRuntimeMechanisms` | `4.9.220` | `runtime state` | 2 | 2 | 4 | DD2 入侵伤害跟踪器及其胜利/击杀时间投影。 |
| `SharedInvasionWaveAndArenaState` | `SharedRuntimeMechanisms` | `4.9.221` | `runtime state` | 19 | 3 | 22 | DD2 入侵波次、竞技场、生成暂停和掉落进度状态。 |

来源成员的分区内序号线索范围：109..3855；序号不定义领域边界，必须逐条回到输入表和 Version4 覆盖。

## 分区专属目标

围绕“世界环境与事件”完成独立只读的 Version4 成员审查和 ECS 拆分设计。
- 围绕昼夜/月相、天气、史莱姆雨、入侵、节日、仪式、沙尘暴、闪电、瀑布、环境生成和场景区域，核对事件事实、计时器、资格条件、生成结果与表现状态的真实边界。
- 回到 Terraria/Main.cs、事件类型、LightningGenerator、WaterfallManager、SceneMetrics 和环境扫描辅助的初始化、Tick、清理及调用者；不要仅按类型名推断事件 owner。
- 分别追踪入侵、Boss、DD2 的进度和伤害跟踪、环境实体生成、天气表现、天空目录以及事件小游戏区域输入，区分权威模拟、派生查询和客户端投影。
- 核对事件状态向网络、持久化、UI、音频和粒子表现的单向交接，明确系统顺序、重复触发、过期和恢复策略。

## 专属证据焦点

- 先完整读取本分区输入报告的成员表，再回到 D:\TRbackup\Version4 重新核对字段、属性、声明类型、初始化/注册、直接读者、直接写者、清理路径和副作用。
- 下列路径只作为本分区的检索种子；路径存在不等于成员语义已确认，必须按公共证据协议记录实际行号、符号、调用者、读者、写者、生命周期和 evidenceStatus。
  - `D:\TRbackup\Version4\Terraria.GameContent.Ambience/AmbienceServer.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Events/BirthdayParty.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Events/CreditsRollEvent.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Events/CultistRitual.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Events/DD2Event.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Events/LanternNight.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Events/MoonlordDeathDrama.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Events/MysticLogFairiesEvent.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Events/Sandstorm.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Events/ScreenObstruction.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/AmbientWindSystem.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/BackgroundChangeFlashInfo.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/BannerSystem.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/BossDamageTracker.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/InvasionDamageTracker.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/LightningGenerator.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/SpelunkerProjectileHelper.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/TreeTopsInfo.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/UnbreakableWallScan.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/VoidLensHelper.cs`
  - `D:\TRbackup\Version4\Terraria/Main.cs`
  - `D:\TRbackup\Version4\Terraria/SceneMetrics.cs`
  - `D:\TRbackup\Version4\Terraria/WaterfallManager.cs`

- 输入分区抽取出的声明类型焦点：
  - `Terraria.GameContent.Ambience.AmbienceServer`
  - `Terraria.GameContent.Ambience.AmbienceServer.AmbienceSpawnInfo`
  - `Terraria.GameContent.AmbientWindSystem`
  - `Terraria.GameContent.BackgroundChangeFlashInfo`
  - `Terraria.GameContent.BannerSystem`
  - `Terraria.GameContent.BossDamageTracker`
  - `Terraria.GameContent.Events.BirthdayParty`
  - `Terraria.GameContent.Events.CreditsRollEvent`
  - `Terraria.GameContent.Events.CultistRitual`
  - `Terraria.GameContent.Events.DD2Event`
  - `Terraria.GameContent.Events.DD2Event.DamageTracker`
  - `Terraria.GameContent.Events.LanternNight`
  - `Terraria.GameContent.Events.MoonlordDeathDrama`
  - `Terraria.GameContent.Events.MoonlordDeathDrama.MoonlordExplosion`
  - `Terraria.GameContent.Events.MoonlordDeathDrama.MoonlordPiece`
  - `Terraria.GameContent.Events.MysticLogFairiesEvent`
  - `Terraria.GameContent.Events.Sandstorm`
  - `Terraria.GameContent.Events.ScreenObstruction`
  - `Terraria.GameContent.InvasionDamageTracker`
  - `Terraria.GameContent.LightningGenerator`
  - `Terraria.GameContent.LightningGenerator.Bolt`
  - `Terraria.GameContent.LightningGenerator.StormLightning`
  - `Terraria.GameContent.SpelunkerProjectileHelper`
  - `Terraria.GameContent.TreeTopsInfo`
  - `Terraria.GameContent.TreeTopsInfo.AreaId`
  - `Terraria.GameContent.UnbreakableWallScan`
  - `Terraria.GameContent.VoidLensHelper`
  - `Terraria.Main`
  - `Terraria.SceneMetrics`
  - `Terraria.WaterfallManager`
  - `Terraria.WaterfallManager.WaterfallData`

- 若路径、类型或行号漂移，记录 version-drift 或 evidence-mismatch；不得静默沿用库存报告行号。Version4 是 Terraria 私有实现的事实来源，完整源码只补证 Version4 已存在文件，tModLoader 只交叉验证公开边界，SS14 只参考结构粒度。

## 专属审查问题

- 天气、日历、入侵和季节事件的权威状态是否由同一系统写入，还是应按独立生命周期拆分？触发条件、持续计时和结束清理分别归谁？
- 环境扫描、天空/背景目录、瀑布和闪电生成是 Query、定义目录、临时工作集还是权威状态？缓存失效与重新计算由谁负责？
- 入侵、Boss 和 DD2 伤害/波次跟踪哪些字段是可恢复进度，哪些只是统计投影或表现计数？网络和存档边界在哪里？
- 同一事件同时驱动实体生成、Tile 变化、UI、音频和天气表现时，如何通过事件/Command/Projection 保持依赖方向和提交顺序？

## 专属不拆分边界

- 不要把天气、天空、入侵、闪电、瀑布和场景扫描重新聚合成一个 EnvironmentComponent。
- 不要把单次闪电分叉、瀑布槽位、扫描结果、Boss 伤害条目或表现事件 payload 当作长期权威组件。
- 不要因为多个事件都写入 Main 就宣布共同 owner；必须按真实写入根与生命周期拆分，跨域归属留给整合审查。

专属跨域提醒：重点记录与世界生成、Tile/液体、NPC/战斗、网络、地图绘制、音频粒子和 UI 的 integration-risk；事件计时、区域几何、天气表现和进度共享候选全部标 crossSubsystemOwner: integration-review。

## 输出要求

- 将完整研究报告只写入：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P02-world-environment-events-public-decomposition.md
- 不要写入输入分区报告、公共提示词、其他分区报告、源代码、测试、项目文件或迁移文件。
- 报告必须逐条覆盖本分区成员，给出成员归属表、状态所有权、Component/System/Query/Command/Adapter/Projection 边界、接口契约、依赖方向、顺序约束、不拆分项、兼容策略、evidence-gap、blocking-decision、focused verifier 计划和 Integration Handoff。
- 本轮只读审查不运行编译型命令；没有实际执行的 verifier 必须写 verificationStatus: not-run。
- proposed 类型、路径和接口必须明确标记 proposed；不得写成当前 NLTX 已存在或已经迁移。
- 跨两个或以上分区使用的类型、接口、ID、快照、值对象和 System 顺序只能提出候选，必须写 crossSubsystemOwner: integration-review，不得在本报告单方面裁决。
- 不得修改任何生产代码、测试、项目文件、生成器、公共协议、分区报告或其他并行会话输出。

## 分区完成检查

- [ ] 逐条覆盖本分区全部 225 条成员，成员事实来自输入表但关键语义已回到 Version4 复核。
- [ ] 只使用本分区叶子清单；没有复制、重新设计或宣布其他 P 分区成员。
- [ ] Component 只承载内聚权威状态；派生值、缓存、查询、短期 payload、日志、网络/存档/UI 输出按正确角色隔离。
- [ ] 所有共享候选写 crossSubsystemOwner: integration-review；没有把 proposed 设计写成已实现能力。
- [ ] 未执行的构建、测试和 verifier 均明确 verificationStatus: not-run；证据缺口、冲突和 blocking-decision 已单列。
- [ ] 输出文件路径唯一为：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P02-world-environment-events-public-decomposition.md

## 会话结束回报

最终消息只回报以下字段：
- partitionId: P02
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P02-world-environment-events-public-decomposition.md
- evidenceSourcesRead: 实际读取的 Version4 文件/类型、必要的完整源码补证、tModLoader 页面和 SS14 参考
- evidenceGaps: 仍缺失或冲突的关键证据
- blockingDecisions: 需要最终整合会话裁决的 owner/顺序/事务边界
- verificationStatus: not-run
- productionCodeModified: no

本提示词只定义本分区的专属范围和审查抓手；公共协议负责共同证据规则、报告结构、禁止项和 Integration Handoff。
