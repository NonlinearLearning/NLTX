# Version4 非权威 P02 世界环境与事件：proposed Component Design

partitionId: P02
sessionId: e8b587c9624249b99352262ee7d52f49
previousSessionId: e6d8f732513b4ac6947b01343c0dad30
handoffId: P02-world-environment-events-component-implementation-20260912-1859
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\02-world-environment-events.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P02-world-environment-events-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P02-world-environment-events-component-execution.md
designStatus: proposed
executionStatus: completed (src2-only Component implementation checkpoint)
implementationStatus: completed
verificationStatus: partial (serial build passed; focused verifier and behavior-equivalence not run)
src2CodeModified: yes
completedComponents:
- SlimeRainStateComponent
- SlimeRainPolicyComponent
- SlimeRainProgressComponent
- CalendarClockComponent
- CalendarVisualPresentationStateComponent
- WorldCalendarOverrideStateComponent
- WeatherPresentationStateComponent
- CoinRainEventStateComponent
- CloudFieldStateComponent
- AmbientWindStateComponent
- WindPhysicsPolicyComponent
- SkyPresentationStateComponent
- InvasionProgressPresentationStateComponent
- SeasonalWorldStateComponent
- SeasonalOverridePolicyComponent
- TitleRefreshRequestStateComponent
- CreditsRollPresentationStateComponent
- ScreenObstructionPresentationStateComponent
- BannerKillProgressStateComponent
- BannerClaimableCountStateComponent
- BannerClaimNotificationStateComponent
- BossEncounterOutcomeStateComponent
- TreeTopVariationStateComponent
- BackgroundChangeFlashStateComponent
- AmbientSpawnScheduleStateComponent
- BirthdayPartyStateComponent
- LanternNightStateComponent
- MysticFairyEventStateComponent
- CultistRitualStateComponent
- SandstormStateComponent
- Dd2InvasionProgressionStateComponent
- Dd2InvasionRunStateComponent
- Dd2InvasionWaveStateComponent
- Dd2InvasionCrystalDropStateComponent
currentComponent: none
pendingComponents: []
deferredComponents:
- WorldInvasionStateComponent: requires unresolved InvasionType from prohibited production source.
- Dd2InvasionArenaStateComponent: requires an unresolved arena rectangle or coordinate value type.
lastCheckpointUtc: 2026-09-12T11:10:07.0292606Z
evidence-gap: 已实现组件仅包含可独立表达的原始值状态；WorldInvasionStateComponent 依赖未解析的 InvasionType，Dd2InvasionArenaStateComponent 依赖未闭合的竞技场几何值类型，二者 deferred；credits/ScreenObstruction caller/reset/render/packet receive、Banner catalog ID mapping/claim consumer/full-state receive/save/network writer、claimable count consumption/notification consumer、Boss tracker session/lifecycle owner、localization/message projection、TreeTop area catalog/variation writer/packet receive/save fallback、BackgroundChangeFlash trigger/consumer/resource lifecycle、ambient schedule reset/consumer and forced-request buffer、BirthdayParty/LanternNight roster/transition/natural-attempt/packet owner、MysticFairy tile-scan/spawn/coordinate/Version4-empty-method owner、CultistRitual recheck persistence/reset and spawn transaction、Sandstorm duration/start-stop/random/packet/reset owner、DD2 run/wave/crystal writer and event/session identity remain open.
implementationCheckpoint: Component-only implementation completed for all independently expressible P02 Components under src2/WorldSession; this checkpoint was continued after runner Handoff. System, Query, Command, Adapter, Projection, registration, network, persistence, scheduler, test and behavior-equivalence work remains excluded and unimplemented.
blocking-decision: crossSubsystemOwner: integration-review；不得让组件补齐未实现的 System、Query、Command、Adapter、Projection、注册、网络、持久化或测试边界；需另行裁决 InvasionType、DD2 竞技场几何、跨组件 writer、协议 DTO 和快照 owner。

## 1. 会话边界

本文件是 P02 的第二轮非权威组件设计草案。它只覆盖输入分区中的 17 个叶子子系统、225
条成员记录（212 fields、13 properties）。本轮只提出 proposed Component、System、Query、
Command、Adapter 和 Projection；不表示任何类型、路径、接口或调度已经创建、迁移、注册或
验证完成。

排除范围：其他 P01-P20 分区的成员清单和设计、Version4 之外的新增覆盖、生产 C#、测试、
csproj、ledger、lock、第一轮报告和权威第二轮文档。跨分区共享的状态、ID、快照、值对象及
系统顺序只保留为 `crossSubsystemOwner: integration-review` 候选。

## 2. 证据与状态语义

### 2.1 已读取的直接证据

Version4 事实来源：

- `D:\TRbackup\Version4\Terraria\Main.cs`：P02 的 Main 字段声明位于实际行 378-390、
  536-550、594-624、649-671、1059-1081；日夜边界和事件顺序位于实际方法
  `UpdateTime_StartDay`（约 13470-13563）、`UpdateTime_StartNight`（约 13319-13468）及
  `UpdateTime` 的事件调用链（约 12972-13128）；史莱姆雨入口位于
  `StartSlimeRain`/`StopSlimeRain`（约 12925-12971），警告位于
  `UpdateSlimeRainWarning`（约 13603-13624）。
- `D:\TRbackup\Version4\Terraria.GameContent.Events\BirthdayParty.cs`、
  `LanternNight.cs`、`CultistRitual.cs`、`Sandstorm.cs`、`CreditsRollEvent.cs`、
  `MoonlordDeathDrama.cs`、`MysticLogFairiesEvent.cs`、`ScreenObstruction.cs`：事件状态、
  计时、清理、临时工作集和客户端表现的直接实现。
- `D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs`：DD2 运行时、波次、
  竞技场、清场、存档进度和伤害跟踪启动/停止边界。
- `D:\TRbackup\Version4\Terraria\Main.cs:1059-1081,11807-11855,12542-12714,13483-13485`：入侵字段声明、进度同步/报告、
  启动/移动/完成/警告/恢复计算以及日出延迟递减；`D:\TRbackup\Version4\Terraria\NPC.cs:344-369,64759-64798`：
  入侵生成资格和 NPC 击杀进度提交；`D:\TRbackup\Version4\Terraria\MessageBuffer.cs:2163-2166,2232-2245,2498-2499`、
  `D:\TRbackup\Version4\Terraria\NetMessage.cs:392,1158-1162`：外部启动命令、packet 78 的空接收分支、world-info 中的类型及
  packet 78 写入宽度。
- `D:\TRbackup\Version4\Terraria.GameContent\BannerSystem.cs`、`BossDamageTracker.cs`、
  `InvasionDamageTracker.cs`、`LightningGenerator.cs`、`AmbientWindSystem.cs`、
  `BackgroundChangeFlashInfo.cs`、`SpelunkerProjectileHelper.cs`、`TreeTopsInfo.cs`、
  `UnbreakableWallScan.cs`、`VoidLensHelper.cs`：账本、生成算法、扫描、缓存和表现副作用。
- `D:\TRbackup\Version4\Terraria\SceneMetrics.cs`、`WaterfallManager.cs`：场景区域派生值和
  瀑布临时绘制资源边界。

当前 NLTX 事实来源：

- `D:\TRbackup\NLTX\src\WorldSession\WorldClockState.cs`、`WorldWeatherState.cs`、
  `WorldClockSnapshotValue.cs`、`WorldWeatherSnapshotValue.cs`、`WorldTickSnapshot.cs`。
- `D:\TRbackup\NLTX\src\WorldSession\Calendar\` 下的
  `CalendarClockComponent`、`SlimeRainStateComponent`、`WorldInvasionStateComponent`、
  `SandstormStateComponent`、`BirthdayPartyStateComponent`、`LanternNightStateComponent`、
  `Dd2RunStateComponent`、`Dd2WaveRuntimeStateComponent`、
  `Dd2PersistentProgressStateComponent`、`PendingWorldEventStateComponent`、
  `WorldCalendarOverrideStateComponent`、`WorldEventInstanceStateComponent` 和
  `WorldEventRandomStateComponent`。

结构参考：

- tModLoader stable local mirror `D:\TRbackup\tmodloader-api-docs-stable`，首页页眉为
  `tModLoader v2026.07`。`class_scene_metrics.html` 的 `ScanAndExportToMain` 条目只交叉
  验证 SceneMetrics 是扫描并导出派生场景值的公开边界；`class_tile_entity.html` 的
  `NetSend` 锚点 `a85d2692b4774c6140a968ef92accc6df`、`NetReceive` 锚点
  `a55d31824ac839a08c69b4c9e7c04c486`、`SaveData` 锚点
  `a2799ce9901b2c8efdb060491ea78869f` 和 `Update` 锚点
  `a483e78bf9c57dcc8d265703a67e91167` 只用于说明外部网络/持久化边界的形状，不能确认
  Version4 私有实现。
- Space Station 14 只作为组织参考：`Content.Server/Wires/WiresComponent.cs` 的实体局部
  线路状态、`Content.Server/Wires/WiresSystem.cs` 的初始化/事件/更新/界面副作用，以及
  `Content.Shared/Atmos/Components/MapAtmosphereComponent.cs` 的世界级状态与派生 overlay
  分离。没有使用其代码、命名或领域语义推断 Terraria 行为。

证据标记：`confirmed` 表示实际源码中已看到成员/调用事实；`partial` 表示只确认局部
生命周期或边界；`missing` 表示尚未找到足够的直接读者/写者/恢复证据；`unresolved` 表示
存在多个可能 owner；设计类型、路径和接口全部使用 `proposed` 语义。

## 3. 当前 NLTX 状态

当前 NLTX 已有世界时钟、天气快照、史莱姆雨、常规入侵、沙尘暴、生日派对、灯笼夜和 DD2
部分组件。它们能证明项目已有设计方向和局部不变量，例如 `SlimeRainStateComponent`
区分 `Active`、`TimeState`、`KillCount`、`WarningTime` 和 `WarningDelay`，而
`WorldInvasionStateComponent` 区分入侵类型、位置、规模、延迟和进度。然而当前状态并未
覆盖 P02 的全部 225 个 Version4 成员，也未证明 Version4 的网络、存档、随机、客户端表现、
NPC 生成和 Tile/场景提交语义已经闭合。现有类型不是本轮已实现迁移的证据。

`WorldTickSnapshot` 已有 `Clock`、`Weather` 和 session readiness 的提交快照，但 P02 的
入侵/事件/扫描/表现数据尚未证明可以作为同一快照的一部分；这必须由 integration review
决定，不能把所有 P02 状态重新塞入一个 Environment snapshot。

当前 `WorldInvasionStateComponent` 已有 `Type`、`PositionX`、`Size`、`SizeStart`、`Delay`、
`WarningTimer` 以及 `Progress*` 字段。它的 `Validate()` 拒绝负 `Progress`，而 Version4 的
`startPumpkinMoon`/`startSnowMoon` 会将共用的 `invasionProgress` 写成 `-1`；因此本检查点
不把六个进度/表现字段继续当作普通入侵权威状态，也不在本轮修改现有不变量。普通入侵进度
由已提交的 `SizeStart - Size` 产生，月事件的 `-1` 仅保留在 proposed 兼容适配器/表现投影边界，
直到 integration-review 决定统一表示。

## 4. 第一检查点：proposed 归属

### 4.1 `MainSlimeRainState` -> proposed `SlimeRainStateComponent`

状态类型：持续世界事件状态；`slimeRainNPC` 是按 NPC 类型索引的临时资格/生成工作集，
不是长期事件组件字段。`maxRain` 属于降雨容量/资源上限，不属于史莱姆雨状态。

| Version4 member | proposed 归属 | 状态分类 | 证据与边界 |
| --- | --- | --- | --- |
| `slimeWarningTime` | `proposed SlimeRainStateComponent.WarningRemaining` | 瞬态事件计时 | `Main.UpdateSlimeRainWarning` 递减，归零后广播开始/结束文本；写者候选为 `proposed SlimeRainWarningSystem`。 |
| `slimeWarningDelay` | `proposed SlimeRainPolicyComponent.WarningDelay` | 配置/规则 | `StartSlimeRain`、`StopSlimeRain` 将其用于警告计时；是否持久化待确认。 |
| `slimeRainTime` | `proposed SlimeRainStateComponent.TimeState` | 权威事件时序 | 正值为活动剩余时间，负值为结束后的重试/冷却状态；`UpdateTime`、启动和停止路径共同写入。 |
| `slimeRain` | `proposed SlimeRainStateComponent.Active` | 权威事件状态 | `StartSlimeRain`、`StopSlimeRain` 写入；启动受 remix/world-surface/raining/已有活动条件约束。 |
| `slimeRainKillCount` | `proposed SlimeRainProgressComponent.KillCount` | 权威事件进度 | NPC 击杀路径递增并可能重置为负阈值；不可与普通 NPC 统计重复写入。 |
| `slimeRainNPCSlots` | `proposed SlimeRainPolicyComponent.NpcSlotMultiplier` | 规则配置 | NPC 邻近活跃数量计算读取；不是事件实例状态。 |
| `slimeRainNPC` | `proposed SlimeRainEligibilityQuery` 的输入/索引缓存 | 派生资格缓存 | `NPC` 生成资格读取，`Main` 初始化至少写入一个类型；完整注册/清理未闭合，暂不注册为长期 Component。 |
| `maxRain` | `proposed RainCapacityPolicy` 的配置输入 | 容量配置 | 与雨滴数组容量有关，不能因同一来源类而并入史莱姆雨。 |

事实闭包：`StartSlimeRain`（Version4 `Main.cs:12925-12948`）在合法条件下设置时间、活动和
击杀进度并安排网络通知；`StopSlimeRain`（`Main.cs:12950-12970`）设置负向时间状态、清除
活动并重新安排警告；`UpdateTime`（`Main.cs:12997-13014`、`13109-13128`）递减正/负时间
并调用停止、警告和其他世界事件；`NPC.cs:64462-64464`、`65558-65569` 读取/更新 NPC
资格和击杀计数。因此该组件不能独占 NPC 生成算法或 NPC 实体生命周期。

`proposed SlimeRainSystem` 输入：日历时钟、天气互斥条件、世界规则、随机源和显式启动/停止
Command；输出：`SlimeRainStateComponent` 状态变化、事件事实和供 NPC/Spawn 查询的只读视图。
`proposed SlimeRainWarningSystem` 只写警告计时和已提交的世界消息命令；`proposed
SlimeRainEligibilityQuery` 只判断 NPC 是否受史莱姆雨规则影响，不写入状态。网络/UI/日志通过
单向 Projection/Adapter；消息发送不得隐藏在纯 Query 中。

建议的候选不变量：活动事件的时间状态必须为正；非活动结束/冷却状态可为负但必须由唯一
系统解释；警告倒计时不能由 NPC 系统修改；击杀计数只能由事件进度提交根更新；NPC 类型
资格表的缓存失效必须在世界重置和内容注册完成后显式处理。

### 4.2 已完成组件检查点

`MainCalendarWeatherState` 的 16 个成员已完成逐成员 proposed 归属；其直接 writer、网络
解码、存档恢复和最终快照 owner 仍是实现阶段的 evidence-gap。

### 4.3 `MainCalendarWeatherState` -> proposed calendar, weather and presentation boundaries

状态类型：时钟和昼夜事实、季节性月事件、雨的持续状态、投影缓存及 coin-rain 事件进度。
这 16 个成员不能整体落入一个 `EnvironmentComponent`：`dayTime`/`time`/`moonPhase` 共同
受日历转换驱动；`raining`/`rainTime`/`maxRaining` 是天气状态；`cloudAlpha`、太阳/月偏移和
`oldMaxRaining` 由客户端表现或网络发送节流使用；`coinRain` 还被 WorldGen 的掉落逻辑消费。

Version4 事实：`Main.cs:594-624` 声明全部成员；`UpdateTime`（约 `12972-13292`）先处理
雨和史莱姆雨，再递增 `time`，随后调用事件更新并进入昼夜边界；`UpdateTime_StartNight`
（约 `13319-13468`）清除日食、执行夜间事件资格和血月随机，最后写入夜间时钟；
`UpdateTime_StartDay`（约 `13470-13565`）结束血月、清理月事件、递增并回绕月相、可能开始
日食或普通入侵；`StartRain`/`StopRain`/`ChangeRain`（约 `12817-12923`）共同写入雨持续、
雨强、rain 标志和 coin-rain 初始值。`NetMessage.cs:221-300` 的 world-info packet 发送
`time`、昼夜/血月/日食、月相、雨强和季节月标志；`NetMessage.cs:523-528` 的时间包还发送
`sunModY`/`moonModY`。`WorldGen.cs:6392-6496` 和 `10270-10276` 展示世界清理/初始化对
coin-rain、日食、雨和云透明度的重置；`WorldGen.cs:59709-59757` 展示 coin-rain 的消费、
中断和下溢归零。

逐成员 proposed 归属：

| Version4 member | proposed 归属 | 状态分类 | 主要读写/边界证据 |
| --- | --- | --- | --- |
| `dayTime` | `proposed CalendarClockComponent.IsDayTime` | 权威日历事实 | `UpdateTime_StartDay/Night` 写入；NPC、Player、WorldGen 和网络读取。 |
| `time` | `proposed CalendarClockComponent.TimeOfDay` | 权威日历计时 | `UpdateTime` 按 `dayRate` 递增，昼夜边界归零；网络和大量 gameplay 查询读取。 |
| `timeForVisualEffects` | `proposed CalendarVisualPresentationStateComponent.VisualTime` | 客户端表现时钟/兼容值 | 直接写入者未在当前检索中闭合；DelegateMethods/Projectile 等读取，不能作为权威日历字段。 |
| `moonPhase` | `proposed CalendarClockComponent.MoonPhase` | 周期日历事实 | 日出递增并在 8 回绕；血月随机和网络读取，最终快照/owner 待 integration-review。 |
| `sunModY` | `proposed CalendarVisualPresentationStateComponent.SunOffsetY` | 客户端/网络表现值 | world time packet 发送；直接生成和恢复写者未闭合。 |
| `moonModY` | `proposed CalendarVisualPresentationStateComponent.MoonOffsetY` | 客户端/网络表现值 | world time packet 发送；直接生成和恢复写者未闭合。 |
| `bloodMoon` | `proposed WorldCalendarOverrideStateComponent.BloodMoon` | 互斥事件事实 | 夜间随机设置、日出清除；NPC、Player、Lang、网络和事件资格读取。 |
| `pumpkinMoon` | `proposed WorldCalendarOverrideStateComponent.PumpkinMoon` | 季节月事件事实 | `startPumpkinMoon`/`stopMoonEvent` 写入并和 blood/snow 互斥；网络、spawn 和 DD2 边界读取。 |
| `snowMoon` | `proposed WorldCalendarOverrideStateComponent.SnowMoon` | 季节月事件事实 | `startSnowMoon`/`stopMoonEvent` 写入并和 blood/pumpkin 互斥；网络、spawn 和 DD2 边界读取。 |
| `cloudAlpha` | `proposed WeatherPresentationStateComponent.CloudAlpha` | 派生表现状态 | `Main.Update`、雨启动/停止路径赋值；NPC/音乐/WorldGen 读取，不能由核心天气 Query 隐式写。 |
| `maxRaining` | `proposed WorldWeatherState.MaximumRainStrength` | 权威天气强度 | `ChangeRain` 生成或更新；world-info packet 发送，雨、风和环境逻辑读取。 |
| `oldMaxRaining` | `proposed WeatherNetworkProjectionState.LastPublishedRainStrength` | 网络去重缓存 | `UpdateTime` 比较后发送并更新；不是天气事实，也不应进入持久化组件。 |
| `rainTime` | `proposed WorldWeatherState.RainTime` | 权威天气计时 | `StartRain` 设置、`UpdateTime` 递减、`StopRain` 清零；无限雨阈值影响停止语义。 |
| `raining` | `proposed WorldWeatherState.IsRaining` | 权威天气事实 | `StartRain`/`StopRain` 写入；Player、NPC、WorldGen、Lang 和环境扫描读取。 |
| `coinRain` | `proposed CoinRainEventStateComponent.RemainingValue` | 降雨附带事件进度 | `StartRain` 初始化，`StopRain`/世界重置清零，WorldGen 消费并钳制；不属于一般时钟。 |
| `eclipse` | `proposed WorldCalendarOverrideStateComponent.Eclipse` | 日历事件事实 | 日出随机设置、夜间/世界重置清除、网络命令可触发；NPC/Player/网络读取。 |

建议的 proposed 类型与系统：`CalendarClockComponent` 保存日历事实，`WorldWeatherState`
保存雨事实，`WorldCalendarOverrideStateComponent` 保存互斥月事件，
`CoinRainEventStateComponent` 保存 coin-rain 进度；`CalendarTransitionSystem` 只提交昼夜
边界，`RainWeatherSystem` 只提交雨/雨强/雨计时，`SeasonalMoonTransitionSystem` 只提交
血月、日食和南瓜/雪月互斥结果，`CoinRainSystem` 只提交 coin-rain 进度，
`CalendarVisualProjection` 和 `WeatherNetworkProjection` 只读取已提交状态。所有类型、路径、
接口和调度边均为 `proposed`。

组合不变量：`moonPhase` 必须在 0..7；`pumpkinMoon` 与 `snowMoon` 不得同时为 true，且二者
不能与 `bloodMoon` 并存；`raining=false` 时向客户端发送的雨强应为 0；`rainTime` 的无限雨
阈值不能被普通停止命令错误清除；`oldMaxRaining` 只用于发送去重；coin-rain 消费不得使其
小于 0。`cloudAlpha`、太阳/月偏移和视觉时间不是权威世界事实，不能反向修改日历。

候选顺序：解析世界规则和显式日历命令 -> `CalendarTransitionSystem` -> 雨/季节互斥系统
-> coin-rain 进度提交 -> 只读 gameplay Query -> 网络、存档和客户端 Projection。每个跨
分区快照、随机流、EntityId、NetworkId、PersistentEntityId 及最终调度 owner 均标记
`crossSubsystemOwner: integration-review`。

当前 NLTX 已有 `WorldClockState`、`WorldClockSnapshotValue`、`CalendarClockComponent`、
`WorldWeatherState`、`WorldWeatherSnapshotValue` 和 `WorldCalendarOverrideStateComponent`，
其中存在局部字段形状与互斥校验；`WorldTickSnapshot` 只组合 clock/weather/readiness，尚无
证据表明 16 个 Version4 成员的表现、网络、coin-rain 或恢复语义已闭合。因此这些现有类型
只能作为对照，不能当作迁移完成声明。

证据缺口：`timeForVisualEffects`、`sunModY`、`moonModY` 的直接写者和客户端接收恢复路径未
闭合；世界保存文件是否持久化雨强、雨计时、月事件和 coin-rain 未确认；`MessageBuffer` 的
完整 world-info 解码、版本兼容和重复网络命令处理未闭合。阻塞决策：共享日历/天气快照的
最终 owner、网络包与存档 DTO 的适配器边界、以及 coin-rain 是否属于天气 owner 或独立事件
owner，均为 `crossSubsystemOwner: integration-review`。

focused verifier（未来实现阶段）：验证昼夜边界只提交一次、月相 8 回绕、血月/日食/南瓜月/
雪月互斥；验证冻结雨、无限雨、雨终止和 rain-strength 变化的网络去重；验证 coin-rain 在
停止、风暴、世界重置和消费下溢时归零；验证视觉 Projection 只读已提交状态，不能修改核心
时钟或天气组件。本轮未运行编译、测试或行为等价验证。

### 4.4 `MainWeatherAndAmbientState` -> proposed wind, cloud and sky presentation boundaries

状态类型：风速连续状态、风向/极端风随机计时、云量调整工作集、天空星体数量以及客户端
云对象池和风物理策略。该组不能和雨的权威事实合并为一个环境组件：雨强会影响风速目标的
计算，但风的随机计时、云量调节和投影对象有不同写者与生命周期。

Version4 事实：`Main.cs:649-671` 声明 11 个成员；`Main.ResetWindCounter`（约
`12038-12049`）用独立随机源设置普通/极端风计时；`Main.UpdateWeather`（约 `12050-12250`）
先把 `windSpeedCurrent` 向受雨强影响的目标靠拢，再在未冻结风且非灯笼夜时递减计时、使用
随机源改变 `windSpeedTarget`、按玩家是否存在限制极端风，并把 `numCloudsTemp` 钳制/提交到
`numClouds`；云量增加到雨强要求时还发送 world-info。`WorldGen.RandomizeWeather`
（约 `7229-7241`）在世界初始化时随机设置云量和初始风并调用 `Cloud.resetClouds`；
`Main` 初始化（约 `3490-3496`）只创建固定数量的 `Cloud` 对象。`Star.SpawnStars`
（`Star.cs:54-138`）创建星体、写入 `Main.numStars` 并用该计数避免星体重叠；`Cloud.cs:39-60`
显示云对象重置边界当前被注释，不能假设已存在完整运行时实现。网络 world-info 的
`NetMessage.cs:258-259` 发送 `windSpeedTarget` 和 `numClouds`，但未发送所有计时器、当前风或
云对象。NPC（`NPC.cs:1105,1207,6700,2322`）、Player（约 `Player.cs:11521-11525`）、
Rain（约 `Rain.cs:84-90`）、WorldGen（约 `WorldGen.cs:59625-59657,59903-59915`）及
Projectile（约 `Projectile.cs:14807-14810,18576-18592,18716-18719`）读取风/云量/风物理
值。

逐成员 proposed 归属：

| Version4 member | proposed 归属 | 状态分类 | 主要读写/边界证据 |
| --- | --- | --- | --- |
| `numStars` | `proposed SkyPresentationStateComponent.StarCount` | 客户端天空工作集 | `Star.SpawnStars` 创建星体并提交数量；只读天空/星体绘制和星降落逻辑，不作为世界规则。 |
| `weatherCounter` | `proposed CloudFieldStateComponent.WeatherAdjustmentTimer` | 云量随机调度状态 | `Main.UpdateWeather` 递减并重置 3600..10800；不应由云绘制直接修改。 |
| `numClouds` | `proposed CloudFieldStateComponent.ActiveCount` | 云量表现状态 | `UpdateWeather` 从临时值提交，雨强升高时补足至 200；world-info packet 发送。 |
| `numCloudsTemp` | `proposed CloudFieldStateComponent.TargetCountWorkset` | 瞬态云量工作集 | 每次天气更新随机微调，最终钳制到 0..200 后提交 `ActiveCount`；不建议网络/存档。 |
| `windSpeedCurrent` | `proposed AmbientWindStateComponent.CurrentSpeed` | 连续风状态 | `UpdateWeather` 向雨强修正后的目标插值；Rain、Player、NPC、Projectile 和 WorldGen 读取。 |
| `windSpeedTarget` | `proposed AmbientWindStateComponent.TargetSpeed` | 风向/强度目标状态 | `UpdateWeather` 由随机流和玩家存在条件修改，world-info packet 发送；NPC 生成方向读取。 |
| `windCounter` | `proposed AmbientWindStateComponent.ChangeTimer` | 风随机调度状态 | `ResetWindCounter`/`UpdateWeather` 单写；灯笼夜和冻结风会改变递减条件。 |
| `extremeWindCounter` | `proposed AmbientWindStateComponent.ExtremeChangeTimer` | 极端风调度状态 | `UpdateWeather` 递减、重置并按极端风幅度追加时间；不能被普通天气 Query 隐式写入。 |
| `windPhysics` | `proposed WindPhysicsPolicyComponent.Enabled` | gameplay/物理策略输入 | Projectile 的 `ShouldUseWindPhysics` 和 AI 读取；最终配置来源与网络范围未闭合。 |
| `windPhysicsStrength` | `proposed WindPhysicsPolicyComponent.Strength` | gameplay/物理策略配置 | Projectile 用于速度修正；必须与当前风速分离，不能把策略参数当成天气事实。 |
| `cloud` | `proposed CloudPresentationStateComponent.Slots` | 客户端第三方对象池 | `Main` 初始化固定 200 个 `Cloud`；对象含位置、纹理绘制字段和激活标志，禁止进入核心世界组件。 |

建议的 proposed 类型与系统：`AmbientWindStateComponent` 保存 current/target speed 和两个
风计时器，`CloudFieldStateComponent` 保存 active/target cloud count 与 weather timer，
`SkyPresentationStateComponent` 保存星体数量等 client-only 工作集，
`CloudPresentationStateComponent` 由渲染适配器持有 `Cloud` slot，`WindPhysicsPolicyComponent`
保存是否启用及强度。`AmbientWindSystem` 读取雨强、灯笼夜、冻结能力、玩家存在和显式随机
流后提交风状态；`CloudFieldSystem` 读取云背景/雨表现输入并提交云量；`SkyPresentationSystem`
和 `CloudPresentationAdapter` 只负责对象池与绘制；`WindPhysicsQuery` 只返回对某投射物是否
适用的纯资格结果。

组合不变量：`CurrentSpeed`、`TargetSpeed` 必须有限并保持 Version4 的幅度上限；普通云量提交
范围为 0..200，临时调整可在提交前保留 Version4 的中间下限；`cloud` slot 数量固定且不能
由天气 Query 扩容；风物理强度不得借由投射物系统反向改写风状态；所有随机数都来自明确的
weather/wind stream，不从客户端对象池隐式取随机。

候选顺序：提交雨强/灯笼夜和世界规则输入 -> `AmbientWindSystem` -> `CloudFieldSystem`
-> NPC/Player/Projectile/WorldGen 的只读天气查询 -> `SkyPresentationSystem`、云对象池
适配器和网络投影。`NetworkId`、天气随机流、客户端资产句柄和最终调度 owner 均标记
`crossSubsystemOwner: integration-review`。

当前 NLTX 的 `WorldWeatherState` 已有 `WindSpeedTarget`、`WindSpeedCurrent`、
`WeatherCounter`、`WindCounter`、`ExtremeWindCounter` 和 `PreviousMaximumRainStrength`，
`WorldWeatherSnapshotValue` 已暴露风速，但没有星体/云对象池/风物理策略的证据。该形状与
本组部分重合，不能证明 Version4 云量随机、客户端对象生命周期或网络兼容已经迁移。

证据缺口：`Cloud.resetClouds` 的有效实现已被注释，客户端 `Cloud[]` 的完整 update/draw
owner 未闭合；`numStars` 的世界载入/客户端解码生命周期未闭合；`weatherCounter`、风计时器、
当前风和风物理策略的存档/网络语义未确认；Version4 使用 `FastRandom` 与 `UnifiedRandom`
的随机流是否必须保持独立也未确认。阻塞决策：风/云状态是否由天气 owner 统一提交、星体和
云对象池的客户端 owner、风物理策略的跨 Projectile owner 以及 snapshot/network/save DTO
均为 `crossSubsystemOwner: integration-review`。

focused verifier（未来实现阶段）：验证风速向雨强修正目标收敛、玩家缺席时极端风钳制、普通/
极端风计时重置、冻结能力和灯笼夜的暂停语义；验证 cloud target 到 active 的 0..200 钳制、
雨强补云、world-info 发送去重和固定 200 slot；验证星体计数和对象池只由客户端投影写入；
验证 Projectile/NPC/Player 只读风物理查询且不修改天气状态。本轮未运行编译、测试或行为等价
验证。

### 4.5 `MainInvasionState` -> proposed invasion authority and progress-presentation boundaries

状态类型：持续的世界入侵权威状态、NPC 击杀配额及其网络/UI 表现缓存。12 个 Version4 成员
不能整体继续放在一个组件中：`invasionType`、`invasionX`、`invasionSize`、`invasionDelay`、
`invasionWarn` 和 `invasionSizeStart` 共同构成入侵事实与生命周期；`invasionProgress*`
由 `ReportInvasionProgress`、`SyncAnInvasion`、月事件启动和 packet 78/UI 展示驱动，属于派生
表现、短期缓存或兼容值。

Version4 事实闭包：`UpdateInvasion` 在类型无效时返回，在 `invasionSize <= 0` 时清理对应
事件完成标志、广播完成警告、清零类型和延迟并发送 world-info；活动期间以至少一个 tile 的
步幅向 `spawnTileX` 移动，并按 `invasionWarn` 在抵达或倒计时归零时广播警告。`StartInvasion`
拒绝并行入侵，按满足生命值条件的活跃玩家数计算不同入侵类型的初始规模，初始化起始规模、
进度图标、波次和警告计时，并启动 `InvasionDamageTracker`；`FakeLoadInvasionStart` 从类型与
剩余规模反推起始规模。NPC 生成路径同时要求类型有效、延迟为零且规模为正，并用
`invasionX` 判断区域；NPC 击杀路径按入侵组递减 `invasionSize`、钳制到零、调用
`ReportInvasionProgress` 并发送 packet 78。`SyncAnInvasion` 对普通入侵用
`invasionSizeStart - invasionSize` 生成进度，而南瓜月/雪月走另一套 wave 进度；两者启动时
仍会复用 `invasionProgress = -1` 的共享槽位。

| Version4 member | proposed 归属 | 状态分类 / evidence status | 主要读写与边界 |
| --- | --- | --- | --- |
| `invasionType` | `proposed WorldInvasionStateComponent.Type` in `src2/WorldSession/Calendar/WorldInvasionStateComponent.cs` | 权威事件种类 / confirmed | `StartInvasion`、完成清理和外部 network command 写入；NPC spawn、world-info 与进度同步读取；`int` 到 `InvasionType` 的内容映射须由适配器验证。 |
| `invasionX` | `proposed WorldInvasionStateComponent.PositionX` | 权威空间进度 / confirmed | `StartInvasion` 从边缘或 Martian 特例初始化，`UpdateInvasion` 向 `spawnTileX` 移动；NPC 生成资格读取；边界必须保留有限值和到达钳制。 |
| `invasionSize` | `proposed WorldInvasionStateComponent.Size` | 权威剩余配额 / confirmed, cross-subsystem writer | `StartInvasion` 初始化，NPC 击杀路径递减并钳制；`UpdateInvasion` 以零判断完成；最终唯一写入根和 NPC 命令交接为 `crossSubsystemOwner: integration-review`。 |
| `invasionDelay` | `proposed WorldInvasionStateComponent.Delay` | 瞬态激活计时 / confirmed | 外部启动路径清零，日出路径递减，NPC spawn query 要求为零；日历边界事件与入侵 writer 的调度归属待 integration-review。 |
| `invasionWarn` | `proposed WorldInvasionStateComponent.WarningTimer` | 警告倒计时 / confirmed | `StartInvasion` 初始化，`UpdateInvasion` 递减并在抵达/归零时重置为 3600；只由 proposed `InvasionLifecycleSystem` 写入，消息通过 projection 输出。 |
| `invasionSizeStart` | `proposed WorldInvasionStateComponent.SizeStart` | 权威基线/恢复元数据 / confirmed | `StartInvasion` 设置，`FakeLoadInvasionStart` 重建，普通 packet 78 进度计算读取；存档是否保留该基线仍是 evidence-gap。 |
| `invasionProgressIcon` | `proposed InvasionProgressPresentationStateComponent.ProgressIcon` in proposed client/event presentation module | 派生表现缓存 / confirmed | `StartInvasion`、`ReportInvasionProgress` 和 packet 78/UI 读取；不作为 NPC 生成或完成判断的输入。 |
| `invasionProgress` | `proposed InvasionProgressPresentationStateComponent.Progress` | 派生表现/兼容 sentinel / confirmed, partial | 普通入侵由 `SizeStart - Size` 或 `ReportInvasionProgress` 得到；南瓜月/雪月启动写入 `-1`；不得写入当前拒绝负值的 `WorldInvasionStateComponent`，sentinel 的协议表示待 integration-review。 |
| `invasionProgressMax` | `proposed InvasionProgressPresentationStateComponent.ProgressMax` | 派生表现缓存 / confirmed | `StartInvasion` 和 `ReportInvasionProgress` 写入，packet 78/UI 读取；普通入侵应与起始规模保持一致，月事件使用独立 wave 上限。 |
| `invasionProgressWave` | `proposed InvasionProgressPresentationStateComponent.ProgressWave` | 客户端波次表现 / confirmed | 普通入侵初始化为零，moon/DD2/NPC wave 报告可更新；不能反向改变 `WorldInvasionStateComponent.Size`。 |
| `invasionProgressDisplayLeft` | `proposed InvasionProgressPresentationStateComponent.DisplayFramesRemaining` | 短期 UI 计时 / confirmed, partial | `ReportInvasionProgress` 设置为 160，月事件启动清零；完整 UI 消费/递减路径未闭合，不能进入持久化快照。 |
| `invasionProgressAlpha` | `proposed InvasionProgressPresentationStateComponent.Alpha` | 客户端表现值 / missing direct writer | 声明与月事件启动清零已确认，但当前检索未找到普通入侵直接写者；保持在 client presentation adapter，直到 UI 更新路径补证。 |

建议的 proposed 边界如下：`WorldInvasionStateComponent` 只保存前六个权威字段；
`InvasionProgressPresentationStateComponent` 保存后六个短期表现字段，并明确它是 projection
输入/缓存而不是世界事件事实。`InvasionLifecycleSystem` 是入侵状态的唯一提交根，应用
`StartInvasionCommand`、日历边界/延迟 tick、位置移动、完成清理和经验证的进度提交；
`InvasionProgressSystem` 只把 NPC 击杀分组和数量转换为 `InvasionProgressCommand`，不得直接
写组件。`InvasionSpawnQuery` 纯计算类型、延迟、剩余规模、位置与场景条件，返回资格而不修改
状态。`InvasionWarningProjection` 负责本地化警告和 world-message adapter，
`InvasionProgressProjection` 负责 packet 78/UI 进度，`InvasionNetworkAdapter` 隔离 packet 7
的 `sbyte` 类型和 packet 78 的 `int,int,sbyte,sbyte` 字段宽度，`InvasionPersistenceAdapter`
等待存档语义确认。所有类型、路径、接口、命令和调度名称均为 `proposed`。

建议生命周期：启动命令先检查无其他活动入侵并统计合格玩家，再提交类型/规模/位置/基线；日历
边界只发布事件供入侵 writer 消费 `invasionDelay`；每 tick 先提交位置/警告转换，NPC 击杀
通过显式 progress command 提交剩余规模，达到零后由同一 authority writer 清理类型/延迟并输出
完成事实；完成状态之后才允许 spawn、warning、network、UI 和 persistence projections 读取。
`InvasionDamageTracker` 的启动/停止及与 `SharedInvasionDamageTrackingState` 的关联不在本组
单方面裁决，标记 `crossSubsystemOwner: integration-review`。

候选不变量：`PositionX` 必须有限；`Size`、`SizeStart`、`Delay`、`WarningTimer` 非负且活动时
`SizeStart >= Size`；类型为空时完成清理不得保留非零剩余规模；`Delay != 0` 或 `Size <= 0`
时 spawn query 必须返回 false；普通进度只由已提交规模派生且不超过基线。`-1` 只允许存在
于兼容适配器/表现协议的明确 sentinel，不得绕过当前 NLTX 的普通入侵组件校验。

当前 NLTX 的 `WorldInvasionStateComponent` 已提供局部类型、位置、规模、延迟、警告、基线和
进度形状及非负校验，但没有证据表明 Version4 的 Start/Update/NPC/packet 78/存档语义已迁移。
本 checkpoint 只提出拆分目标，不改变该现有类、`WorldEventProgressState` 或任何生产代码。

证据缺口和阻塞决定：`MessageBuffer` 的 world-info 完整解码和 packet 78 接收分支目前未闭合；
入侵字段的保存/恢复、`invasionProgressAlpha` 的 UI writer、NPCDamageTracker 的生命周期和
NPC 击杀进度的最终提交根仍未闭合；`invasionProgress = -1` 与当前非负组件不变量需要兼容
表示；跨系统快照、EntityId、PersistentEntityId、NetworkId、外部本地化 ID 以及调度顺序均为
`crossSubsystemOwner: integration-review`。

focused verifier（未来实现阶段）：验证每种入侵的玩家资格与规模公式、重复启动拒绝、边缘/火星
初始位置、至少一步移动和到达钳制；验证延迟递减、警告重置/完成清理、NPC 资格三条件和击杀
数量/组映射的单次提交；验证普通入侵进度的 `SizeStart - Size`、packet 78 四字段宽度、packet
7 类型宽度、重复/过期网络命令、月事件 `-1` sentinel 以及 projection 只读已提交状态。存档
往返、NPCDamageTracker 和完整客户端 alpha 更新在证据闭合前保持未验证。本轮未运行编译、测试
或行为等价验证。

### 4.6 `MainSeasonalAndTitleState` -> proposed seasonal facts, override policy and title effect boundaries

状态类型：现实日期/世界规则计算出的节日事实、临时与永久强制策略，以及一次性的客户端标题
刷新请求。七个成员不应继续作为一个无区分的 presentation state：`xMas` 和 `halloween`
是由日期与 force policy 重算的派生世界事实；`forceXMasForToday`、`forceHalloweenForToday`
是可保存的每日覆盖输入；`forceXMasForever`、`forceHalloweenForever` 是 secret-seed/世界
规则级持久化策略；`changeTheTitle` 只表达一次性 UI/平台副作用。

Version4 事实闭包：`checkXMas` 依据本地日期（12 月 15 日起）并叠加 today/forever 强制值
计算 `xMas`；`checkHalloween` 依据 Halloween 日期窗口并叠加对应强制值计算 `halloween`。
昼夜边界在 `UpdateTime_StartDay` 先调用 `CheckForMoonEventsStartingTemporarySeasons`，该方法
清除 today flags、根据南瓜月/雪月达到波次 15 的结果重新设置它们，并在 forever policy 生效时
抑制对应的临时 flag，同时广播切换文本。`WorldGen.InitializeSecretSeeds` 从 endless
Halloween/Christmas secret seed 设置 forever flags；世界初始化注册 `checkHalloween`/`checkXMas`
并在 reset 中清理四个 force flags。`WorldFile` 在版本 212+ 保存/恢复 today flags，在版本 287+
保存/恢复 forever flags。world-info packet 的 bit field 发送 today flags；当前检索到的
`MessageBuffer` packet-7 分支为空，因此客户端接收闭包仍是 evidence-gap。`changeTheTitle`
在主循环中被消费后清零并调用 `SetTitle`，但当前 Version4 检索未找到可靠的直接写入者。

| Version4 member | proposed 归属 | 状态分类 / evidence status | 主要读写与边界 |
| --- | --- | --- | --- |
| `xMas` | `proposed SeasonalWorldStateComponent.IsChristmasActive` | 派生世界事实 / confirmed | `checkXMas` 按日期、today/forever policy 重算；NPC、Item、WorldGen 和内容条件读取；不应由查询直接写入。 |
| `halloween` | `proposed SeasonalWorldStateComponent.IsHalloweenActive` | 派生世界事实 / confirmed | `checkHalloween` 按日期、today/forever policy 重算；NPC、Item、WorldGen 和掉落条件读取；与 Christmas 事实可同时为 true，不能套用月事件互斥规则。 |
| `forceXMasForToday` | `proposed SeasonalOverridePolicyComponent.ForceChristmasToday` | 每日覆盖输入 / confirmed | 月事件波次和外部/存档路径写入，day-start 消费并清除；WorldFile version 212+ 与 world-info bit 发送；最终命令/网络 owner 为 `crossSubsystemOwner: integration-review`。 |
| `forceHalloweenForToday` | `proposed SeasonalOverridePolicyComponent.ForceHalloweenToday` | 每日覆盖输入 / confirmed | 与 Christmas today flag 对称；day-start 清除/重新计算，WorldFile version 212+ 持久化，world-info bit 发送；不得与当前 PendingWorldEventStateComponent 独立双写。 |
| `forceXMasForever` | `proposed SeasonalOverridePolicyComponent.ForceChristmasForever` | 永久世界规则 / confirmed | `InitializeSecretSeeds` 从 endless Christmas 设置，WorldFile version 287+ 持久化；world reset 清除；secret-seed policy adapter 负责输入。 |
| `forceHalloweenForever` | `proposed SeasonalOverridePolicyComponent.ForceHalloweenForever` | 永久世界规则 / confirmed | `InitializeSecretSeeds` 从 endless Halloween 设置，WorldFile version 287+ 持久化；world reset 清除；secret-seed policy adapter 负责输入。 |
| `changeTheTitle` | `proposed TitleRefreshRequestStateComponent.Pending` 或客户端 effect command | 一次性客户端 effect / missing direct writer | 主循环消费后清零并调用 `SetTitle`；未确认直接写者，不应进入世界快照、存档或普通查询。 |

建议的 proposed 边界如下：`SeasonalWorldStateComponent` 只保存两个已提交的 active facts；
`SeasonalOverridePolicyComponent` 保存 today/forever 四个 policy inputs；`SeasonalCalendarSystem`
读取时钟、日期、月事件结果和 policy 后提交 active facts，并在日界线提交 today flag 的
consume/reset；`SeasonalOverrideCommand` 与 `SecretSeedSeasonalPolicyAdapter` 隔离外部命令和
secret-seed 配置；`SeasonalWorldProjection` 输出 world-info/save DTO，`TitleRefreshProjection`
只把显式 title-refresh command 转换为 `SetTitle` effect。当前 `PendingWorldEventStateComponent`
已有 `ForceHalloweenForToday`/`ForceChristmasForToday` 形状，只能作为现有重叠证据；最终是否
复用它、迁移到 policy component 或由日历 owner 持有必须标记 `crossSubsystemOwner: integration-review`。

候选不变量：`xMas` 与 `halloween` 的 active 值只能由统一日期/policy reducer 提交；today flag
在 day-start consume 后不得由 NPC/Item 查询写回；forever flag 只能由 secret-seed/world-rule
command 或恢复适配器提交；world reset 清理四个 force inputs；`changeTheTitle` 是最多一次
消费的 pending effect，消费后清零；title effect 不能改变季节事实。EntityId、NetworkId、
PersistentEntityId、secret-seed ID 和平台标题 API ID 必须分离，最终共享 owner 为
`crossSubsystemOwner: integration-review`。

当前 NLTX 的 `PendingWorldEventStateComponent` 已有两个 daily force 字段，但没有 xMas/halloween
active facts、forever policy 或 title effect 的闭合组件；`WorldSecretSeedSeasonalRules*`
只提供 secret-seed 定义/选择形状，不能证明 Version4 world-file、world-info 和日界线行为已迁移。
本 checkpoint 不修改这些生产类型。

证据缺口和阻塞决定：`changeTheTitle` 的直接 writer、`SetTitle` 平台边界、world-info packet-7
接收、today/forever flags 的完整版本兼容、日期时区/clock port 和 world reset 后的派生事实时序
仍需实现阶段闭合；`PendingWorldEventStateComponent` 的两个 daily fields 与 proposed policy
之间不得形成双写。跨系统快照、EntityId、NetworkId、PersistentEntityId、secret-seed ID 和
最终日历调度均为 `crossSubsystemOwner: integration-review`。

focused verifier（未来实现阶段）：验证日期窗口、today/forever policy precedence、南瓜/雪月
波次 15 的临时 flag 生成、forever 对 today 的抑制、day-start consume/reset、world reset 清理、
WorldFile version 212/287 round-trip、world-info bit encoding/receive，以及 active seasonal
facts 对 NPC/Item/WorldGen 查询的只读可见性。验证 `changeTheTitle` 至多一次消费、清零和
平台 adapter 调用；当前 packet receive、时区、title writer 和行为等价均未验证。

### 4.7 `SharedWorldEventPresentationState` -> proposed credits, Moon Lord drama and screen-obstruction boundaries

状态类型：一个可同步的制作人员名单剩余计时、一个完全客户端化的月总死亡戏剧表现适配器，以及一个
由场景扫描输入驱动的屏幕遮挡平滑状态。23 条记录不能合并成一个世界环境组件：credits 计时有
`Main` tick、NPC 触发和 packet 140 投影边界；Moon Lord 的纹理、位置、动画帧和光照请求是 XNA
客户端临时资源；ScreenObstruction 只消费 `SceneMetrics`/`SceneState` 并输出本地表现量。

Version4 事实闭包：`CreditsRollEvent.TryStartingCreditsRoll` 在 `NPC.cs:65260` 被触发，默认把
剩余时间设为 `28800`，若 `SkyManager.Instance["CreditsRoll"]` 是 `CreditsRollSky` 则采用其
`AmountOfTimeNeededForFullPlay`，随后发送 packet 140；`Main.UpdateTime` 在 `Main.cs:13117`
每 tick 调用 `CreditsRollEvent.UpdateTime`，`WorldGen.cs:6455` 的世界清理调用 `Reset`，新连接
同步路径在 `NetMessage.cs:2557` 调用 `SendCreditsRollRemainingTimeToPlayer`。packet 140 的
发送宽度在 `NetMessage.cs:1584-1587` 是 `byte` 子类型加 `int`，但 `MessageBuffer.cs:3179-3198`
的接收分支只处理 transform 子类型，子类型 0 没有 credits 状态写入，因此接收闭包仍为缺口。

`MoonlordPiece` 的 `Update` 在 `MoonlordDeathDrama.cs:47-56` 做重力、旋转、角速度衰减和位置推进；
`MoonlordExplosion.Update` 在 `:95-102` 推进帧；外边界和动画帧完成条件由两个 `Dead` 属性计算。
`MoonlordDeathDrama.Update` 在 `:115-155` 清理死亡对象、按 `SceneMetrics.Center` 判断 2000 像素
范围内的光源、平滑 `whitening` 并清空请求。NPC 的六类 Moon Lord 路径在 `NPC.cs:35472,35496,
35607,35718,35735,41491,41587` 附近调用 `RequestLight`；针对两个构造函数、列表添加、绘制和
`Update` 直接调用者的定向检索没有找到闭合证据。因而这些对象应留在 proposed 客户端 adapter/
object pool，不能成为核心世界组件字段。

`ScreenObstruction.Update`（`ScreenObstruction.cs:12-43`）根据 `insideUnbreakableWalls`、
`DangerousDungeonCurse` 的进度差和 `headcovered` 计算目标值，使用 `0.01`、`0.3` 或前一次速度
调用 `SceneState.MoveTowards`。定向检索只确认该方法内部写入 `lastSpeed` 和 `screenObstruction`，
没有找到网络、存档、直接调用者或 reset 入口；这两个值只能作为客户端平滑状态和策略记忆保留。

逐成员 proposed 归属：

| Version4 member | proposed 归属 | 状态分类 / evidence status | 主要读写与边界 |
| --- | --- | --- | --- |
| `MAX_TIME_FOR_CREDITS_ROLL_IN_FRAMES` | `proposed CreditsRollPresentationPolicy.MaxFrames` | 常量策略 / confirmed | `CreditsRollEvent.TryStartingCreditsRoll` 和 `UpdateTime` 使用 `28800`；不是可变 Component 字段，需保留 clamp 上限语义。 |
| `_creditsRollRemainingTime` | `proposed CreditsRollPresentationStateComponent.RemainingFrames` | 可同步表现事件计时 / confirmed | `TryStartingCreditsRoll` 写入、`UpdateTime` 每 tick 递减、`Reset` 清零；packet 140 向全体和新连接投影，接收写者未闭合。 |
| `MoonlordPiece._texture` | `proposed MoonlordPiecePresentation` 的外部纹理句柄 | 客户端资源 / confirmed | 构造函数接收 `Texture2D`；由 proposed `MoonlordDeathDramaPresentationAdapter` 持有，不进入核心组件或存档。 |
| `MoonlordPiece._position` | `proposed MoonlordPiecePresentation.Position` | 客户端临时动画状态 / confirmed | 构造时写入，`MoonlordPiece.Update` 推进；`Dead` 以世界边界判断，世界尺寸依赖 `crossSubsystemOwner: integration-review`。 |
| `MoonlordPiece._velocity` | `proposed MoonlordPiecePresentation.Velocity` | 客户端临时动画状态 / confirmed | 由构造参数初始化，Update 加重力并累加到位置；没有网络/存档证据。 |
| `MoonlordPiece._origin` | `proposed MoonlordPiecePresentation.Origin` | 客户端绘制元数据 / confirmed | 构造函数接收纹理原点；仅供未来绘制 projection 使用，XNA `Vector2` 不应渗入核心组件。 |
| `MoonlordPiece._rotation` | `proposed MoonlordPiecePresentation.Rotation` | 客户端绘制状态 / confirmed | 构造初始化、Update 按角速度推进；没有独立权威语义。 |
| `MoonlordPiece._rotationVelocity` | `proposed MoonlordPiecePresentation.RotationVelocity` | 客户端动画状态 / confirmed | 构造初始化，Update 每 tick 乘以 `0.99`；由对象 adapter 单一持有。 |
| `MoonlordPiece.Dead` | `proposed MoonlordPieceLifetimeQuery.IsDead` | 纯生命周期谓词 / confirmed | 位置离任一世界边界 480 像素内即死亡；不建模为可写字段，列表清理由 proposed drama system 执行。 |
| `MoonlordExplosion._texture` | `proposed MoonlordExplosionPresentation` 的外部纹理句柄 | 客户端资源 / confirmed | 构造函数接收 `Texture2D` 并用于 `Frame`；第三方资源由 presentation adapter 隔离。 |
| `MoonlordExplosion._position` | `proposed MoonlordExplosionPresentation.Position` | 客户端临时动画状态 / confirmed | 构造初始化，`Dead` 同时检查世界边界；没有网络/存档证据。 |
| `MoonlordExplosion._origin` | `proposed MoonlordExplosionPresentation.Origin` | 客户端绘制元数据 / confirmed | 从首帧 `Rectangle.Size()/2` 计算；不应作为世界位置或持久化值。 |
| `MoonlordExplosion._frame` | `proposed MoonlordExplosionPresentation.Frame` | 客户端精灵帧 / confirmed | 由纹理七帧切片和 `_frameCounter / _frameSpeed` 更新；`Rectangle` 保持在 adapter/projection 边界。 |
| `MoonlordExplosion._frameCounter` | `proposed MoonlordExplosionPresentation.FrameCounter` | 客户端动画计时 / confirmed | 构造为 0、Update 每 tick 加一，达到七帧寿命后由 `Dead` 清理。 |
| `MoonlordExplosion._frameSpeed` | `proposed MoonlordExplosionPresentation.FrameSpeed` | 客户端动画策略 / partial | 构造函数接收外部值；创建者未定位，必须在实现前确认正值约束，避免除零和错误帧寿命。 |
| `MoonlordExplosion.Dead` | `proposed MoonlordExplosionLifetimeQuery.IsDead` | 纯生命周期谓词 / confirmed | 越界或 `FrameCounter >= FrameSpeed * 7` 时为 true；不成为 mutable Component 字段。 |
| `MoonlordDeathDrama._pieces` | `proposed MoonlordDeathDramaPresentationAdapter.PiecePool` | 客户端临时对象池 / partial | `Update` 遍历并移除 `Dead` 对象；构造/添加/绘制调用者未在定向 Version4 检索中找到。 |
| `MoonlordDeathDrama._explosions` | `proposed MoonlordDeathDramaPresentationAdapter.ExplosionPool` | 客户端临时对象池 / partial | `Update` 遍历并移除 `Dead` 对象；不进入网络、存档或核心世界快照，创建入口仍缺证。 |
| `MoonlordDeathDrama._lightSources` | `proposed MoonlordLightRequestBuffer.Sources` | 每帧请求工作集 / confirmed | `RequestLight` 添加位置，Update 以 `SceneMetrics.Center` 做 2000 像素邻近判断后清空；不能由 Query 隐式写入。 |
| `MoonlordDeathDrama.whitening` | `proposed client-only MoonlordDramaVisualStateComponent.Whitening` | 客户端平滑表现值 / partial | `SceneState.MoveTowards` 以 `requestedLight`、步长 `0.02` 平滑写入；`SceneState` 的最终消费和 reset 未闭合。 |
| `MoonlordDeathDrama.requestedLight` | `proposed MoonlordLightRequestBuffer.MaxRequestedLight` | 每帧聚合缓存 / confirmed | `RequestLight` 将输入钳制到 1 并取最大值；Update 消费后清零，不是持久化或网络权威。 |
| `ScreenObstruction.lastSpeed` | `proposed ScreenObstructionPresentationStateComponent.LastTransitionSpeed` | 平滑策略记忆 / confirmed | `ScreenObstruction.Update` 在墙内/遮挡头部时写入 `0.01`/`0.3`，恢复时复用；未发现外部写者或持久化。 |
| `ScreenObstruction.screenObstruction` | `proposed ScreenObstructionPresentationStateComponent.CurrentAmount` | 客户端表现状态 / confirmed | `ScreenObstruction.Update` 以目标 `0..0.95` 和上次速度调用 `MoveTowards`；网络、存档和直接渲染消费者未闭合。 |

建议的 proposed 边界如下：`CreditsRollPresentationStateComponent` 只保存已提交剩余帧，
`CreditsRollPresentationPolicy` 保存上限和 Sky 覆盖规则，`CreditsRollSystem` 是唯一状态写者，
`CreditsRollNetworkProjection`/`CreditsRollNetworkAdapter` 负责 packet 140 但暂不假设接收闭合。
`CreditsRollSkyAdapter` 隔离 `SkyManager`、`CreditsRollSky` 和 XNA 资源。

Moon Lord 部分不创建核心世界 Component 来承载纹理、`Vector2`、`Rectangle` 或对象列表：
`MoonlordDeathDramaPresentationAdapter` 持有 piece/explosion object pool 和逐帧 light-request buffer，
`MoonlordDramaSystem` 负责更新/清理，`MoonlordPieceLifetimeQuery` 与 `MoonlordExplosionLifetimeQuery`
只做纯生命周期判断，`MoonlordDeathDramaProjection` 才能绘制纹理并应用 whitening。NPC 的
`RequestLight` 调用应改为显式 presentation request port；它不能直接写世界事件权威状态。

ScreenObstruction 单独使用 `ScreenObstructionPresentationStateComponent`、
`ScreenObstructionTargetQuery`、`ScreenObstructionSystem` 和 `ScreenObstructionProjection`。
Query 只接受已经扫描的玩家输入并计算目标/速度，System 才提交平滑值，Projection 只观察已提交值。
它不得与 Moon Lord drama 共享可变字段。

组合不变量：credits 剩余帧在 `0..28800` clamp 语义内递减，启动/重置必须由一个 writer 提交；
Moonlord piece/explosion 的生命周期谓词不得改变对象；`FrameSpeed` 必须为正；light request 每帧
消费并清空，聚合强度不超过 1，距离判断使用 2000；screen obstruction 目标值遵循墙内进度上限
`0.9`、headcovered 优先值 `0.95` 和旧速度恢复规则。所有世界边界、Sky 资产、SceneState、
SceneMetrics、实体/网络/持久化 ID 与调度顺序均保留 `crossSubsystemOwner: integration-review`。

证据缺口和阻塞决定：packet 140 subtype 0 的客户端接收没有写入 credits 状态；CreditsRollSky
是否在目标客户端激活、月总对象的构造/添加/绘制/Update 调用者、动画资源释放、Moonlord reset、
`SceneState` 的 whitening 消费、ScreenObstruction 的直接调用者和 reset 尚未闭合；三类状态的
网络/存档 DTO 也未确认。不得在这些证据闭合前把客户端对象池、light buffer 或 screen obstruction
写入世界快照，也不得把 presentation projection 作为权威写者。

focused verifier（未来实现阶段）：验证 credits 默认 28800、Sky 时间覆盖、逐 tick clamp、NPC
触发、world clear、非零新连接同步和 packet 140 `byte + int` 编码；验证 packet subtype 0 的
接收缺口不会被误写成已闭合。验证 piece 的重力/旋转/角速度衰减/480 边界、explosion 七帧和
正 `FrameSpeed`、light 最大值/2000 范围/每帧清空/whitening 0.02 平滑，以及对象池只在 Dead
时清理。验证 screen obstruction 的墙内进度 clamp、headcovered 优先级、速度恢复和
`MoveTowards` 结果；网络、存档、绘制调用者、资源生命周期和行为等价在证据闭合前保持未验证。

### 4.8 `SharedInvasionAndBossTracking` -> proposed banner ledger, boss definition/outcome and invasion-label boundaries

本检查点覆盖输入报告中的 14 条成员：BannerSystem 4 条、BossDamageTracker 5 条、
InvasionDamageTracker 5 条。它把 Banner 的持久击杀进度、可领取数量和瞬态通知分开；把
Boss tracker 的不可变定义与一次性击杀结果分开；把 Invasion tracker 的分组定义和本地化
投影分开。`NPCDamageTracker` 基类的完整 credit entry、active/recent 列表和计时字段不在
本组输入成员内，本检查点只把它们作为生命周期边界证据，不复制为本组成员。

直接 Version4 证据：

- `D:\TRbackup\Version4\Terraria.GameContent\BannerSystem.cs:13-50,52-67,68-113,133-173`
  显示 full-state/kill-count/claim-count packet writer、容量、两个数组、clear、world-file
  save/load、289 版本门槛和 NPC 击杀提交；`D:\TRbackup\Version4\Terraria\NPC.cs:66168-66179`
  是 NPC 死亡到 Banner 提交的调用点；`D:\TRbackup\Version4\Terraria\WorldGen.cs:6410`
  是世界清理调用点；`D:\TRbackup\Version4\Terraria\MessageBuffer.cs:587` 向加入玩家发送
  full state。`NetBannersModule.Deserialize` 在当前 Version4 文件中没有提交接收状态。
- `D:\TRbackup\Version4\Terraria.GameContent\BossDamageTracker.cs:7-85` 显示 `_type`、
  `_overrides`、`_killed`、名称回退、击杀/逃脱消息、复合 Boss 类型匹配、active 检查和击杀
  结果写入；`D:\TRbackup\Version4\Terraria.GameContent\NPCDamageTracker.cs:80-128,164-257,276-310`
  显示定义注册、tracker 创建、damage 路由、`BossKilled`、Update、Reset 和 recent 结果边界；
  `D:\TRbackup\Version4\Terraria\Main.cs:11472` 调用 Update，`WorldGen.cs:6568` 调用 Reset，
  `NPC.cs:65519` 提交 BossKilled。
- `D:\TRbackup\Version4\Terraria.GameContent\InvasionDamageTracker.cs:8-36` 显示六个
  vanilla group/name-key 映射、readonly group/name、Name 投影和始终为 null 的 KillTimeMessage。
  `Main.cs:7444,7462,12682` 分别创建南瓜月、雪月和普通入侵 tracker。当前 Version4 的
  `IncludeDamageFor` 与 `CheckActive` 是 stub/空实现；完整参考版
  `D:\TRbackup\无任何删减通过编译\Terraria.GameContent\InvasionDamageTracker.cs` 仅作为补充
  证据显示预期的 `NPC.GetNPCInvasionGroup` 匹配和活动停止逻辑，不能覆盖 Version4 行为基线。

逐成员 proposed 归属：

| Version4 member | proposed 归属 | 状态分类 / evidence status | 读者、写者和边界 |
| --- | --- | --- | --- |
| `BannerSystem.MaxBannerTypes` | `proposed BannerCatalogPolicy.MaxBannerTypes` | 目录容量/配置；confirmed | `BannerSystem` 数组分配、`SceneMetrics.NPCBannerBuff` 和映射函数读取；初始化时固定为 `293`，不是可变 Component 字段。 |
| `BannerSystem.killCount` | `proposed BannerKillProgressStateComponent.KillCounts` | 持久世界进度；confirmed | `AddKill` 唯一直接递增，`Clear`/`Load` 写入，`Save` 和 packet 1 投影读取；索引是 `BannerId`，不是 EntityId。 |
| `BannerSystem.claimableBanners` | `proposed BannerClaimableCountStateComponent.Counts` | 持久未领取数量；confirmed | `AddClaimableBanner` 递增，`Clear`/`Load` 写入，`Save` 和 packet 2 投影读取；领取/消费读者未闭合，不能与 kill count 合并。 |
| `BannerSystem.AnyNewClaimableBanners` | `proposed BannerClaimNotificationStateComponent.HasNewClaimableBanners` | 瞬态通知 latch；confirmed/partial | `AddClaimableBanner` 置 true，`Clear` 置 false；不在 `Save` 或 full-state writer 中，外部消费/清除者未定位。 |
| `BossDamageTracker._type` | `proposed BossTrackingDefinition.BossType` | 不可变 tracker 定义；confirmed | 构造时从普通 type 或复合定义首个 type 固定，`IncludeDamageFor`/active 查询读取；不作为可变战斗进度字段。 |
| `BossDamageTracker._overrides` | `proposed BossTrackingDefinition.Override` | 不可变定义引用；confirmed | 构造时保存 `CustomDefinition`，其 NPC type 集合和可选名称覆盖匹配/投影读取；`LocalizedText` 由本地化 adapter 隔离。 |
| `BossDamageTracker._killed` | `proposed BossEncounterOutcomeStateComponent.WasKilled` | tracker 结果；confirmed | `OnBossKilled` 置 true，`KillTimeMessage` 读取；仅随 active/recent tracker 生命周期存在，没有 Version4 存档/网络证据。 |
| `BossDamageTracker.Name` | `proposed BossTrackingNameQuery` / `proposed BossNameProjection` | 派生本地化投影；confirmed | 有 override name 时返回它，否则按 `_type` 调 `Lang.GetNPCName`；Query 不写状态，输出不进入核心组件。 |
| `BossDamageTracker.KillTimeMessage` | `proposed BossTrackingMessageProjection.KillTimeMessage` | 派生消息投影；confirmed | 根据 `WasKilled` 选择 `BossDamageCommand.KillTime` 或 `BossDamageCommand.KillTimeEscaped`；消息 key 和发送端属于外部 adapter。 |
| `InvasionDamageTracker.VanillaInvasionNameKeys` | `proposed InvasionDefinitionCatalog.NameKeys` | 不可变目录；confirmed | 枚举 `1/2/3/4/-2/-1` 到 Bestiary localization key；构造默认名称读取，未知 group 的异常/兼容策略仍需 integration review。 |
| `InvasionDamageTracker._invasionGroup` | `proposed InvasionTrackingSessionScope.InvasionGroup` | tracker 范围身份；confirmed | 构造时固定，普通/南瓜月/雪月启动路径提供；它是 InvasionGroup，不是 NPC type、NetworkId 或 PersistentEntityId。 |
| `InvasionDamageTracker._name` | `proposed InvasionTrackingNameProjection.ResolvedName` | 构造期本地化值；confirmed | 使用显式 `LocalizedText` 或目录 key 解析一次；第三方本地化对象不进入权威快照。 |
| `InvasionDamageTracker.Name` | `proposed InvasionTrackingNameProjection` | 只读本地化投影；confirmed | 直接返回 `_name`；只服务 tracking/UI/log 输出，不反向写入入侵状态。 |
| `InvasionDamageTracker.KillTimeMessage` | `proposed InvasionTrackingMessageProjection.KillTimeMessage` | 空消息投影；confirmed | Version4 明确返回 `null`；不得为了复用 Boss 消息而生成击杀/逃脱文本。 |

Banner 的 proposed 边界是四部分：`BannerCatalogPolicy` 只提供容量和 ID 目录；
`BannerKillProgressStateComponent` 只保存持久击杀数；`BannerClaimableCountStateComponent`
只保存未领取数量；`BannerClaimNotificationStateComponent` 只保存本次运行的通知 latch。
`BannerProgressSystem` 接收 NPC death command、完成 NPC-to-banner 映射、递增 kill count，
在 `KillsToBanner` 阈值命中时递增 claimable count 并产生 notification event。它是三个可变
值的唯一 proposed writer；`BannerPersistenceAdapter` 负责 world-file version 289 的读写，
`BannerNetworkProjection` 负责 packet 0/1/2 的写出，`BannerNetworkAdapter` 的接收写者保持
evidence-gap，因为当前 `Deserialize` 没有恢复状态。

Boss 的 proposed 边界是 tracker session 而非全局 Boss 组件：
`BossTrackingDefinition` 保存 `_type` 与 `_overrides` 的不可变定义，
`BossEncounterOutcomeStateComponent` 保存 `_killed` 这一 session outcome，
`BossTrackingNameQuery` 和 `BossTrackingMessageProjection` 只从定义、结果和 localization
port 计算输出。`NPCDamageTracker` 的 active/recent 生命周期由 proposed
`BossTrackingLifecycleAdapter` 接到既有 damage/kill/update/reset 端口；credit entry、player
name、world credit 和 recent list 的最终 owner 不由本检查点裁决。

Invasion 的 proposed 边界是定义目录、tracking session 和只读投影：
`InvasionDefinitionCatalog` 保存 group/name-key 表，`InvasionTrackingSessionScope` 保存
group，`InvasionTrackingNameProjection` 负责显式名称或本地化名称，
`InvasionTrackingMessageProjection` 对 Version4 的 `KillTimeMessage == null` 保持空结果。
`InvasionDamageEligibilityQuery` 和 `InvasionTrackingLifetimeSystem` 是行为边界，不是这 5 条
成员的隐藏写者：Version4 当前 `IncludeDamageFor`/`CheckActive` stub 必须先决定是否兼容保持；
只有 integration review 明确采用完整参考版语义后，才可加入 group 匹配和 active-stop 行为。

组合不变量和生命周期：

- `BannerId` 是固定 catalog index；`NPC type`、`InvasionGroup`、`NetworkId`、
  `PersistentEntityId` 和外部 item ID 必须分别建模。所有 Banner 数组长度必须匹配
  `MaxBannerTypes`，无效 ID 不得写入进度。
- `killCount` 只由 NPC death -> banner mapping -> `BannerProgressSystem` 递增；
  `claimableBanners` 只在达到 `KillsToBanner` 阈值时递增；通知 latch 在同一提交中置位，
  不得通过读取 projection 再次改变计数。世界 clear 先清两个数组和通知，load 只在对应版本
  数据存在时恢复 claimable counts。
- Banner 提交后再发送 packet 1/2；full-state join packet 只观察已提交数组。packet subtype 0
  的 current Version4 receive path 不可假定存在，禁止把 writer evidence 当作 receive equivalence。
- Boss definition 在 session 创建后不可变；`WasKilled` 只允许 false -> true 的击杀提交，
  active 检查停止 tracker 后才进入 recent result；Name/message projection 不写 tracker。
- Invasion group 必须来自目录或显式自定义名称；Version4 的六个默认 key 和 null kill-time
  message 必须保持。完整参考版 predicate 不是当前 Version4 行为证据，stub 兼容决定前不得
  改变 damage attribution 或 tracker stop 时机。

候选顺序为：内容目录初始化 -> world-file load/网络 full-state projection -> NPC death 或
damage/kill command -> Banner/Boss/Invasion tracking state commit -> active/threshold/cleanup
reducer -> localization/message/network projection。World reset 清理 Banner state 和
`NPCDamageTracker.Reset` tracker session；所有 event snapshot、NPC identity、NetworkId、
PersistentEntityId 和最终调度 owner 标记 `crossSubsystemOwner: integration-review`。

网络和持久化边界：Banner `Save` 先写 kill-count length 和每个 `int`，Version4 world version
小于 289 时不读 claimable 数组；版本达到 289 才读 `ushort` claimable length/count。packet
full-state subtype 0 复用 `Save`，kill update subtype 1 写 `short BannerId + int`，claim update
subtype 2 写 `short BannerId + ushort`。这些是已确认的写出形状；当前 `NetBannersModule.Deserialize`
为空，接收 owner、stale revision、客户端 claim 消费和 `AnyNewClaimableBanners` 的网络语义
仍未确认。Boss/Invasion 这 10 个字段没有直接 save/network 写出证据，因此只允许 local
tracker/session 和 message projection 设计，不得放入 world persistence 或 shared network
snapshot。`LocalizedText`、localization key 和外部 player/NPC IDs 也保持 adapter 边界。

focused verifier（未来实现阶段）：验证 Banner v288/v289 round-trip、数组长度兼容、NPC
击杀映射、阈值命中一次、clear 后通知和 full-state join；验证 packet subtype 与 `short/int/ushort`
宽度，并单独记录 subtype 0 receive 未闭合。验证 Boss 普通/复合 type 注册、override name
回退、`WasKilled` 消息选择、active-stop/recent lifecycle 和 world reset。验证 Invasion 六个
默认 group/name key、显式名称、unknown group 策略和 null kill-time message；用 fixture 区分
Version4 stub 行为与完整参考版补充行为，禁止无裁决自动采用后者。当前均未运行。

证据缺口和阻塞决定：Banner claim 的消费/领取读者、notification 消费者、packet full-state
接收、revision/stale policy；Boss/NPCDamageTracker credit entry 和 tracker session 的最终
跨分区 owner；Invasion stub 是否必须保持、NPC group mapping 和 active-stop 是否可迁移；
BannerId/NPC type/InvasionGroup 的共享值对象及其 network/persistence DTO 均需
`crossSubsystemOwner: integration-review` 裁决。

### 4.9 `SharedLightningGenerationState` -> proposed lightning policy, bolt payload and generation-port boundaries

本检查点覆盖 23 个字段：`Bolt` 的 5 个单次生成 payload 字段、`StormLightning` 的 3 个
生成器/常量字段，以及 `LightningGenerator` 的 15 个生成策略字段。它们不是世界事件事实：
配置字段描述确定性算法参数，`Bolt` 字段描述一次生成调用的短期几何/分叉结果，
`collidedWithTile` 是生成过程输出。不得把 `Vector2[]`、旋转数组、随机 seed 或 Tile 碰撞
结果注册为长期共享 Component。

直接 Version4 证据：`D:\TRbackup\Version4\Terraria.GameContent\LightningGenerator.cs:9-21`
声明 `Bolt` payload；`:24-55` 声明 `StormLightning.Generator`、`SourceRotationLimit=PI/9`
和 `Length=1000`，并从 seed 生成主路径；`:63-93` 声明生成器的碰撞、步长、层数、旋转、
偏差、分叉和进度范围参数；`:95-118` 显示 `Generate` 将递归生成和旋转计算交给
`GenerateBolt`/`CalcRotations`/`SmoothRotations`。当前 Version4 这三个方法是 stub/空实现，
因此参数和调用方向已确认，但几何算法、Tile collision writer、fork 数量及 rotations 输出
仍为 evidence-gap。`D:\TRbackup\Version4\Terraria\Projectile.cs:46530` 调用
`StormLightning.GenerateMainBoltPath`，说明结果被 Projectile 表现/行为路径消费；本检查点不把
Projectile entity 或网络 ID 吸收进闪电组件。

逐成员 proposed 归属：

| Version4 member | proposed 归属 | 状态分类 / evidence status | 边界 |
| --- | --- | --- | --- |
| `LightningGenerator.Bolt.positions` | `proposed LightningBoltPresentationPayload.Positions` | 单次几何 payload；confirmed/partial | 递归生成结果；由 Projectile/Lightning projection 读取，`Vector2[]` 只在 payload/adapter 边界。 |
| `LightningGenerator.Bolt.rotations` | `proposed LightningBoltPresentationPayload.Rotations` | 单次表现 payload；confirmed/partial | `CalcRotations`/`SmoothRotations` 输出，当前算法 stub，不能作为权威状态。 |
| `LightningGenerator.Bolt.progressRange` | `proposed LightningBoltSegmentPayload.ProgressRange` | 分段绘制范围；confirmed | 根范围 `0..1` 与 fork 范围由递归传入；仅控制 payload 可见区间。 |
| `LightningGenerator.Bolt.forkDepth` | `proposed LightningBoltSegmentPayload.ForkDepth` | 递归工作状态；confirmed | 由生成递归深度控制，不能跨 Tick 保存或作为世界进度。 |
| `LightningGenerator.Bolt.collidedWithTile` | `proposed LightningBoltCollisionResult.CollidedWithTile` | 派生生成结果；partial | 需要 `TileCollisionPort` 的直接写者证据；不得由 Projection 反向写 Tile。 |
| `StormLightning.Generator` | `proposed LightningGenerationPolicy.Default` | 共享配置对象；confirmed | 初值包含 `RotationStrength=.9`、`StepSize=8`、`Layers=4`、`MaxForksPerBolt=2`、`MaxForkDepth=2`、`ForkProgressRange=.3.. .8` 等；策略对象非 ECS entity state。 |
| `StormLightning.SourceRotationLimit` | `proposed LightningGenerationPolicy.SourceRotationLimit` | 算法常量；confirmed | 主路径 seed 偏转上限 `PI/9`；保留为配置/纯函数输入。 |
| `StormLightning.Length` | `proposed LightningGenerationPolicy.SourceLength` | 算法常量；confirmed | 主路径源点距目标 `1000`；世界单位语义和 coordinate value object 需 integration review。 |
| `LightningGenerator.SolidTileCollision` | `proposed LightningGenerationPolicy.SolidTileCollision` | 生成策略；confirmed | 控制递归是否检查 solid tile；实际 collision reader/writer 未闭合。 |
| `LightningGenerator.RotationStrength` | `proposed LightningGenerationPolicy.RotationStrength` | 生成策略；confirmed | 根递归旋转强度；fork 会按 multiplier 派生。 |
| `LightningGenerator.StepSize` | `proposed LightningGenerationPolicy.StepSize` | 生成策略；confirmed | 根递归采样步长；必须大于零，验证未运行。 |
| `LightningGenerator.Layers` | `proposed LightningGenerationPolicy.Layers` | 生成策略；confirmed | 迭代/细化层数；与 fork recursion 独立，不能用文件顺序表达。 |
| `LightningGenerator.LayerStrengthFactor` | `proposed LightningGenerationPolicy.LayerStrengthFactor` | 生成策略；confirmed | 各层强度因子；范围/数值校验待 verifier。 |
| `LightningGenerator.PerpendicularDeviationFactor` | `proposed LightningGenerationPolicy.PerpendicularDeviationFactor` | 生成策略；confirmed | 法线方向扰动因子；依赖显式 random port。 |
| `LightningGenerator.ReduceRandomnessAfter` | `proposed LightningGenerationPolicy.ReduceRandomnessAfter` | 生成策略；confirmed | 路径进度阈值后的随机性切换；范围与边界待 verifier。 |
| `LightningGenerator.ForkGenerationThresholdAngleFraction` | `proposed LightningGenerationPolicy.ForkAngleThresholdFraction` | 生成策略；confirmed | 分叉角阈值比例；不能由 Bolt payload 的 `forkDepth` 反推。 |
| `LightningGenerator.ForkReflectAngleMultiplier` | `proposed LightningGenerationPolicy.ForkReflectAngleMultiplier` | 生成策略；confirmed | 分叉反射角倍率。 |
| `LightningGenerator.ForkRotationStrengthMultiplier` | `proposed LightningGenerationPolicy.ForkRotationStrengthMultiplier` | 生成策略；confirmed | 分叉旋转强度倍率。 |
| `LightningGenerator.ForkStepSizeMultiplier` | `proposed LightningGenerationPolicy.ForkStepSizeMultiplier` | 生成策略；confirmed | 分叉步长倍率。 |
| `LightningGenerator.ForkLengthMultiplier` | `proposed LightningGenerationPolicy.ForkLengthMultiplier` | 生成策略；confirmed | 分叉长度倍率。 |
| `LightningGenerator.MaxForksPerBolt` | `proposed LightningGenerationPolicy.MaxForksPerBolt` | 生成策略；confirmed | 每根主 Bolt 分叉上限；递归终止条件需 verifier。 |
| `LightningGenerator.MaxForkDepth` | `proposed LightningGenerationPolicy.MaxForkDepth` | 生成策略；confirmed | 递归深度上限；必须与 `Bolt.ForkDepth` 保持不变量。 |
| `LightningGenerator.ForkProgressRange` | `proposed LightningGenerationPolicy.ForkProgressRange` | 生成策略；confirmed | fork 的可见进度范围；与 segment payload 分离。 |

建议的 proposed 边界：`LightningGenerationPolicy` 是 immutable/configuration 值，不属于
world snapshot；`LightningGenerationPort` 接受 seed、source/target coordinate 和 policy，
返回一次性的 `LightningBoltPayload` 集合；`LightningBoltGenerationSystem` 负责调用纯生成
函数并把 `collidedWithTile` 标为结果；`LightningProjectionAdapter` 将 positions/rotations
交给 Projectile 或客户端绘制。随机性必须通过显式 `ILightningRandomPort` 输入，Tile 查询通过
`ILightningTileCollisionPort` 输入/输出，不能让 Query 隐式改变世界 Tile。

组合不变量和生命周期：`StepSize > 0`、`Layers >= 0`、`MaxForksPerBolt >= 0`、
`MaxForkDepth >= 0`；`ForkDepth <= MaxForkDepth`；所有 position/rotation 数组长度关系
必须由 generator 统一建立；`ProgressRange` 必须为合法有序区间；生成调用结束后 Bolt payload
可丢弃，不能进入存档、网络或跨 Tick tracker。`StormLightning.Generator` 的静态可变对象
只能在内容初始化/配置边界写入；同一 Tick 不得同时由 Projectile 和天气系统改写策略。

网络、持久化和跨域：当前没有闪电字段的 save/network writer 证据；seed、source/target、
Tile coordinate、Projectile entity、NetworkId、PersistentEntityId 和 asset/render handle
必须分开。若将来网络同步闪电，只能投影已提交的事件/seed 或重新生成所需的明确输入，不能
序列化 `Vector2[]` 和 XNA payload 作为世界权威快照。完整算法 stub、Tile collision 细节、
payload 清理和绘制消费仍需 evidence-gap。

focused verifier（未来实现阶段）：固定 seed 下主路径长度/起始偏转、step/layers、rotation
平滑、fork threshold/multipliers、深度/数量上限、progress range、solid-tile collision 和
`collidedWithTile` 输出；验证 `calcPositions=false`/`calcRotations=false`、空/null bolt list
策略和 payload 生命周期。当前未运行，也不声称算法行为等价。

### 4.10 `SharedWaterfallState` -> proposed waterfall policy, transient slot buffer and client-resource boundaries

本检查点覆盖输入报告中的 11 个成员：`WaterfallData` 的 4 个槽位字段、4 个容量/长度/类型
策略常量、有效容量字段，以及槽位数组和纹理资源数组。直接 Version4 证据来自
`D:\TRbackup\Version4\Terraria\WaterfallManager.cs:13-47`；`Main.cs:194` 只声明 manager
引用，`Main.cs:3269` 只负责 `new WaterfallManager()`，`BindTo` 只向 `Preferences.OnLoad`
注册 `Configuration_OnLoad`。当前文件没有发现填充、清空、更新或绘制瀑布的方法，且
`Configuration_OnLoad` 是空实现。因此本检查点只确认字段形状和初始化边界，不声称已闭合
瀑布算法、纹理加载、Tile/液体扫描或调用调度。

逐成员 proposed 归属：

| Version4 member | proposed 归属 | 状态分类 / evidence status | 边界 |
| --- | --- | --- | --- |
| `WaterfallManager.WaterfallData.x` | `proposed WaterfallSlotPayload.TileX` | 单次槽位 payload；confirmed/partial | 保存该瀑布槽位的 Tile X 坐标；坐标值对象和世界尺寸语义由 `crossSubsystemOwner: integration-review` 决定，不进入长期世界快照。 |
| `WaterfallManager.WaterfallData.y` | `proposed WaterfallSlotPayload.TileY` | 单次槽位 payload；confirmed/partial | 保存该瀑布槽位的 Tile Y 坐标；与 `TileX` 一起只在一次生成/绘制工作集中存在。 |
| `WaterfallManager.WaterfallData.type` | `proposed WaterfallSlotPayload.WaterfallTypeId` | 单次槽位 payload；partial | 保存瀑布类型索引/内容 ID；类型表、未知类型回退和是否等同液体 ID 尚无直接 writer 证据。 |
| `WaterfallManager.WaterfallData.stopAtStep` | `proposed WaterfallSlotPayload.StopAtStep` | 单次遍历/绘制结果；partial | 保存该槽位的停止步数；其生成条件、是否包含 `maxLength` 边界和清理时机仍需调用者证据。 |
| `WaterfallManager.minWet` | `proposed WaterfallGenerationPolicy.MinWetness` | 不可变规则常量；confirmed value / missing use | 保留 `160` 的默认值作为湿度/液体阈值候选；没有可确认的读取者，不能将其解释为当前天气状态。 |
| `WaterfallManager.maxWaterfallCountDefault` | `proposed WaterfallCapacityPolicy.DefaultCapacity` | 配置默认值；confirmed value | 保留默认容量 `1000`；它描述资源上限，不是世界事件进度，也不应进入网络事件快照。 |
| `WaterfallManager.maxLength` | `proposed WaterfallGenerationPolicy.MaxLength` | 生成/遍历策略；confirmed value / missing use | 保留长度上限 `100`；具体单位和 `stopAtStep` 的关系待实际生成调用者闭合。 |
| `WaterfallManager.maxTypes` | `proposed WaterfallGenerationPolicy.MaxWaterfallTypes` | 内容容量策略；confirmed value | 保留纹理/类型槽位上限 `28`；不能由数组长度推断网络或持久化 ID 语义。 |
| `WaterfallManager.maxWaterfallCount` | `proposed WaterfallCapacityPolicy.ActiveCapacity` | 有效运行时容量；partial | 初值为 `1000`，可能受 Preferences/configuration 调整；唯一配置写者和越界回退策略仍未找到。 |
| `WaterfallManager.waterfalls` | `proposed WaterfallSlotBuffer.Slots` | 临时工作集/槽位池；confirmed shape / missing lifecycle | `WaterfallData[1000]` 只适合作为按场景重建和复用的工作缓冲，不注册为 ECS Component、存档字段或网络 payload。 |
| `WaterfallManager.waterfallTexture` | `proposed WaterfallTextureCatalog` 的客户端资源缓存 | 外部资源缓存；confirmed shape / missing lifecycle | `Asset<Texture2D>[28]` 必须由 proposed texture/resource adapter 获取、释放和失效；XNA `Asset`/`Texture2D` 不进入核心组件或世界快照。 |

建议的 proposed 边界：`WaterfallGenerationPolicy` 保存 `minWet`、`maxLength` 和 `maxTypes`；
`WaterfallCapacityPolicy` 保存默认容量与经配置后的有效容量；二者都是配置/策略值，不是世界
事件状态。`WaterfallSlotPayload` 描述一次槽位结果，`WaterfallSlotBuffer` 负责场景内的有限
临时工作集，`WaterfallTextureCatalog` 只保存客户端资源句柄。`WaterfallGenerationQuery`
可以读取 Tile/液体/湿度输入并计算候选槽位，`WaterfallGenerationSystem` 在未来证据闭合后
成为唯一的槽位写者，`WaterfallPresentationAdapter` 只读取已提交槽位并交给绘制端。
`WaterfallConfigurationAdapter` 隔离 `Preferences.OnLoad`；不得让配置回调隐式修改世界事件
组件。以上类型、路径、接口和调度均为 `proposed`。

组合不变量和生命周期：有效容量必须为非负且不超过实际 buffer 容量；槽位计数不能超过
`ActiveCapacity`；`WaterfallTypeId` 必须在已加载内容目录或明确的未知类型策略内；长度和
`StopAtStep` 的边界必须由同一个生成策略解释。每次场景/世界重建前必须清空或重新初始化
`WaterfallSlotBuffer`，绘制消费后只能复用或丢弃槽位 payload。纹理资源的获取、换图、失效和
释放由客户端资源端口负责，不能由纯 Query 或世界 reset 直接操作 XNA 对象。

候选调度契约为：读取已提交的世界/Tile/液体输入 -> `proposed WaterfallGenerationQuery`
计算候选 -> `proposed WaterfallGenerationSystem` 写入一次性 `WaterfallSlotBuffer` ->
`proposed WaterfallPresentationAdapter` 绘制并只读消费 -> 清空/复用 buffer -> 网络、存档和
日志 Projection。目录顺序不能替代该调度。`TileX`/`TileY`、液体强度、`WaterfallTypeId`、
`EntityId`、`NetworkId`、`PersistentEntityId` 和纹理资源 ID 必须保持不同；共享坐标值对象、
液体输入快照和客户端资源端口标记为 `crossSubsystemOwner: integration-review`。

网络和持久化边界：当前没有 11 个成员的 Version4 save/network writer 证据。不要序列化
`WaterfallData[]`、`Asset<Texture2D>[]` 或 XNA 句柄作为世界权威状态；若未来需要重建客户端
瀑布，应同步已批准的 Tile/液体输入、确定性生成种子或事件 revision，并在接收端重建临时
槽位。`maxWaterfallCount` 是否是可持久化配置仍需 integration review；不能因为字段是 public
就把它当作世界状态。

focused verifier（未来实现阶段）：验证四个 `WaterfallData` 字段的槽位 round-trip、Tile 坐标
和 `StopAtStep` 边界、`MinWetness=160`、`MaxLength=100`、`MaxWaterfallTypes=28`、默认和
配置后容量 `1000` 的限制；验证超容量截断/拒绝、未知类型、空 buffer、世界/场景 reset 后
复用、纹理加载/释放以及绘制只读消费。单独验证 Preferences 回调只更新配置、不会隐式写入
世界状态。实际 writer、clear/update/draw caller、类型注册、资源路径、网络/存档 DTO 和
行为等价结果仍为 evidence-gap，当前未运行。

### 4.11 `WorldEnvironmentScanHelpers` -> proposed transient scan, pure wall-query and Void Lens presentation boundaries

本检查点覆盖输入报告中的 9 个成员：`SpelunkerProjectileHelper` 的位置/Tile 去重缓存、
地图边界和帧计数器，`UnbreakableWallScan` 的扫描距离与方向目录，以及 `VoidLensHelper` 的
位置、透明度和帧号。直接 Version4 证据来自
`D:\TRbackup\Version4\Terraria.GameContent\SpelunkerProjectileHelper.cs:8-39`、
`D:\TRbackup\Version4\Terraria.GameContent\UnbreakableWallScan.cs:16-76` 和
`D:\TRbackup\Version4\Terraria.GameContent\VoidLensHelper.cs:13-74`；调用边界来自
`Main.cs:1262,11390`、`Player.cs:17741-17758,24450`、`Projectile.cs:34033` 和
`Utils.cs:416`。这些证据只支持提出边界，不支持当前 NLTX 已经存在扫描组件或表现实现。

逐成员 proposed 归属：

| Version4 member | proposed 归属 | 状态分类 / evidence status | 边界 |
| --- | --- | --- | --- |
| `SpelunkerProjectileHelper._positionsChecked` | `proposed SpelunkerScanWorkBuffer.CheckedPositions` | 瞬态去重缓存；confirmed shape / partial use | `AddSpotToCheck` 按 `Vector2` 去重后才调用 `CheckSpot`；不注册为世界 Component、存档字段或网络字段。 |
| `SpelunkerProjectileHelper._tilesChecked` | `proposed SpelunkerScanWorkBuffer.CheckedTiles` | 瞬态 Tile 去重缓存；confirmed shape / missing writer | 当前 `CheckSpot` 为空，谁写入该集合和扫描输出仍未闭合；不能从字段名推断矿物结果 owner。 |
| `SpelunkerProjectileHelper._clampBox` | `proposed SpelunkerScanWorkBuffer.ClampBounds` | 每帧派生扫描边界；confirmed assignment / missing consumer | `OnPreUpdateAllProjectiles` 每帧按 `Main.maxTilesX/maxTilesY` 写入 `Rectangle(2,2,width,height)`；XNA Rectangle 和世界尺寸值对象由 `crossSubsystemOwner: integration-review` 决定。 |
| `SpelunkerProjectileHelper._frameCounter` | `proposed SpelunkerScanWorkBuffer.FrameCounter` 与 `proposed SpelunkerScanPolicy.ResetInterval=10` | 瞬态调度状态；confirmed assignment | 预更新递增，达到 10 时清空两个集合并归零；不把它解释为世界时间或持久化 tick。 |
| `UnbreakableWallScan.ScanDistance` | `proposed UnbreakableWallScanPolicy.MaxDistance=250` | 不可变查询策略；confirmed value | 仅限制 `LineScan` 的步数；不是玩家冷却、墙体实体距离或网络字段。 |
| `UnbreakableWallScan.Directions` | `proposed UnbreakableWallScanPolicy.Directions` | 不可变方向目录；confirmed value | 八个相邻方向按源码顺序保留；目录为查询输入，不是可变组件状态。 |
| `VoidLensHelper._position` | `proposed VoidLensPresentationPayload.Position` | 一次性表现输入；confirmed construction / partial consumer | Projectile 构造读取 `Center`，世界坐标构造先执行 `Y -= 2f`；坐标值对象、Projectile entity ID 与外部绘制坐标必须分开。 |
| `VoidLensHelper._opacity` | `proposed VoidLensPresentationPayload.Opacity` | 一次性表现输入；confirmed construction / missing draw consumer | 从 Projectile 或调用者输入读取；当前 `Update` 不直接使用它，最终绘制/粒子消费仍是 evidence-gap。 |
| `VoidLensHelper._frameNumber` | `proposed VoidLensPresentationPayload.FrameNumber` | 一次性动画帧输入；confirmed construction / partial consumer | Projectile 构造读取 `Projectile.frame`；世界坐标构造按 `Main.tileFrameCounter[491]` 与坐标计算 `(mod 40)/5`，资源帧目录 owner 未闭合。 |

`SpelunkerScanWorkBuffer` 只保存一次扫描窗口内的四个瞬态值，`SpelunkerScanPolicy` 只保存
清理间隔等策略值。`SpelunkerScanFrameBoundarySystem` 是 proposed 的帧边界入口，负责重建
clamp、递增计数并在第 10 次预更新清理缓存；`SpelunkerScanSystem` 或等价的显式 command
边界负责接收位置检查请求。`AddSpotToCheck` 是缓存写操作，不能伪装成纯 Query；未来的
`SpelunkerSpotQuery` 必须只读取显式 Tile/世界端口并返回候选结果，不能隐式修改
`CheckedTiles`。由于 `CheckSpot` 当前是空实现，矿物发现、高亮或投影输出不得在本检查点臆造。

`UnbreakableWallScanQuery` 是 proposed 纯查询：`LineScan` 最多执行 250 步，越界或 Tile
为空返回 false，遇到 `tile.wall == 350` 时返回 `tile.wallColor() >= 16`，否则按方向继续；
`InsideUnbreakableWalls` 对八方向设置 bit mask，再按源码旋转检查连续五位。Tile 读取必须
经过显式 `IUnbreakableWallTileReadPort`，查询不写 Player。`Player.DoUnbreakableWallScan`
仍是当前直接调用边界，负责 dual-dungeon 条件、冷却、上次位置、`insideUnbreakableWalls`
状态及变化通知；这些玩家状态属于 P08/player owner，P02 只提出查询 seam。`Utils.cs:416`
只应读取查询结果，不应成为墙体扫描状态 writer。`NetModule.Deserialize` 与
`BroadcastChange` 目前为空，玩家结果的网络接收、权限和 stale/duplicate 策略继续标记
`crossSubsystemOwner: integration-review`。

`VoidLensPresentationPayload` 只存在于一次 Projectile/世界坐标表现请求中，不是长期 ECS
Component、存档状态或网络快照。`VoidLensPresentationSystem` 或
`VoidLensProjectionAdapter` 可以把 payload 交给显式的 `IVoidLensLightingPort`、
`IVoidLensRandomPort` 和 `IVoidLensDustPort`；随机选择、`Lighting.AddLight`、
`Dust.NewDustDirect`、速度/缩放和 `customData` 归资源/效果适配器，不得进入纯 payload 或
世界权威状态。`ProjectileId`/`EntityId`、`NetworkId`、`PersistentEntityId` 和
`Vector2`/Tile 坐标继续保持不同，最终 Dust consumer、draw/resource 生命周期和客户端帧
目录由 `crossSubsystemOwner: integration-review` 决定。

候选生命周期与调度契约为：

```text
world tile dimensions
  -> proposed SpelunkerScanFrameBoundarySystem
  -> proposed SpelunkerScanSystem accepts position requests and writes one transient work buffer
  -> proposed SpelunkerSpotQuery reads an explicit Tile port

player cooldown/position gate
  -> proposed UnbreakableWallScanQuery reads Tile/wall facts
  -> player-owned result commit and optional network projection

Projectile/world-position input
  -> proposed VoidLensPresentationPayload
  -> explicit random/lighting/dust ports
  -> read-only client presentation adapter
```

网络和持久化边界：不要序列化 Spelunker 的 HashSet、Rectangle、Tile 扫描缓存或未闭合的
`CheckSpot` 结果；不要把 `ScanDistance`/`Directions` 当作世界快照字段；不要序列化 Void
Lens 的 XNA 坐标、随机 Dust、光照请求或 `customData`。若未来需要同步墙体结果，只能由
Player/integration owner 提供版本化、类型明确的结果或输入，不能把 `insideUnbreakableWalls`
的玩家缓存反向变成 P02 世界组件。当前 NLTX 搜索未发现这些 helper、查询或表现适配器的
对应实现，只有不相关的 `UnbreakableWallProgressionTier` 定义；因此本 checkpoint 不声称
已有能力。

focused verifier（未来实现阶段）：验证 Spelunker 位置/Tile 去重、clamp 每帧重建、计数器在
第 10 次预更新清空并归零、世界尺寸边界和空 `CheckSpot` 兼容策略；将 `CheckSpot` 仍为空的
Version4 事实与未来矿物结果实现分开测试。验证墙体八方向顺序、250 步上限、越界/空 Tile、
wall 350、`wallColor >= 16`、bit-mask 旋转和纯查询无写入；另测 Player 冷却/上次位置由
外部 owner 提交。验证 Void Lens 两种构造、Y 偏移、frame 计算、透明度传递、确定性随机
端口、光照/Dust 端口调用顺序和 payload/customData 生命周期。所有结果当前未运行。

### 4.12 `SharedAmbientSkyCatalogState` -> proposed tree-top definition/state and background-flash presentation boundaries

本检查点覆盖输入报告中的 17 个成员：`BackgroundChangeFlashInfo` 的两个背景变化缓存，
`TreeTopsInfo.AreaId` 的 13 个稳定区域常量及 `Count`，以及 `TreeTopsInfo` 的 13 槽变体数组。
直接 Version4 证据来自
`D:\TRbackup\Version4\Terraria.GameContent\TreeTopsInfo.cs:7-207` 和
`D:\TRbackup\Version4\Terraria.GameContent\BackgroundChangeFlashInfo.cs:5-40`；静态宿主和
调用边界来自 `D:\TRbackup\Version4\Terraria\WorldGen.cs:4336-4338,7409-7478,7571-7685,10380-10384`、
`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1423,2384`、
`D:\TRbackup\Version4\Terraria\NetMessage.cs:221-281`、
`D:\TRbackup\Version4\Terraria\MessageBuffer.cs:385-386`、
`D:\TRbackup\Version4\Terraria\Main.cs:3370-3379,11333-11337` 和
`D:\TRbackup\Version4\Terraria\Projectile.cs:49684-49687`。这些证据支持拆分存档世界变体、
不可变区域目录和表现缓存，但不支持把 `WorldGen` 静态字段宿主直接当成一个 ECS owner。

逐成员 proposed 归属：

| Version4 member | proposed 归属 | 状态分类 / evidence status | 边界 |
| --- | --- | --- | --- |
| `BackgroundChangeFlashInfo._variations` | `proposed BackgroundChangeFlashStateComponent.VariationsByArea` | 背景变化缓存；confirmed shape / partial semantics | `UpdateCache` 按 13 个 `AreaId` 调用 `UpdateVariation`；该方法在 Version4 为空，不能臆造 flash 触发或绘制消费者。 |
| `BackgroundChangeFlashInfo._flashPower` | `proposed BackgroundChangeFlashStateComponent.FlashPowerByArea` | 表现衰减缓存；confirmed decay / missing trigger and consumer | `UpdateFlashValues` 每次按 `Clamp(value - 0.05f, 0f, 1f)` 衰减；未找到直接触发写者或读取/绘制者，不进入世界存档或网络快照。 |
| `TreeTopsInfo.AreaId.Forest1` | `proposed TreeTopAreaCatalog.Forest1 = 0` | 不可变定义；confirmed | 稳定区域目录值，不是 `NetworkId`、`PersistentEntityId` 或实体字段。 |
| `TreeTopsInfo.AreaId.Forest2` | `proposed TreeTopAreaCatalog.Forest2 = 1` | 不可变定义；confirmed | 稳定区域目录值；保持与数组/存档槽位的索引关系。 |
| `TreeTopsInfo.AreaId.Forest3` | `proposed TreeTopAreaCatalog.Forest3 = 2` | 不可变定义；confirmed | 稳定区域目录值；不与 `Main.treeStyle` 的分区数组合并。 |
| `TreeTopsInfo.AreaId.Forest4` | `proposed TreeTopAreaCatalog.Forest4 = 3` | 不可变定义；confirmed | 稳定区域目录值；不把世界 X 边界本身纳入目录状态。 |
| `TreeTopsInfo.AreaId.Corruption` | `proposed TreeTopAreaCatalog.Corruption = 4` | 不可变定义；confirmed | 稳定区域目录值；背景类型和区域变体保持分离。 |
| `TreeTopsInfo.AreaId.Jungle` | `proposed TreeTopAreaCatalog.Jungle = 5` | 不可变定义；confirmed | 稳定区域目录值；其背景/Tiles 输入由外部端口提供。 |
| `TreeTopsInfo.AreaId.Snow` | `proposed TreeTopAreaCatalog.Snow = 6` | 不可变定义；confirmed | 稳定区域目录值；区域 ID 不是天气状态或网络字段。 |
| `TreeTopsInfo.AreaId.Hallow` | `proposed TreeTopAreaCatalog.Hallow = 7` | 不可变定义；confirmed | 稳定区域目录值；保留与 `GetHollowTreeFoliageStyle` 读取路径的索引语义。 |
| `TreeTopsInfo.AreaId.Crimson` | `proposed TreeTopAreaCatalog.Crimson = 8` | 不可变定义；confirmed | 稳定区域目录值；不拥有 Crimson 世界事实。 |
| `TreeTopsInfo.AreaId.Desert` | `proposed TreeTopAreaCatalog.Desert = 9` | 不可变定义；confirmed | 稳定区域目录值；沙漠背景随机化仍是外部输入/调用边界。 |
| `TreeTopsInfo.AreaId.Ocean` | `proposed TreeTopAreaCatalog.Ocean = 10` | 不可变定义；confirmed | 稳定区域目录值；不等同于海洋实体或 Tile 坐标。 |
| `TreeTopsInfo.AreaId.GlowingMushroom` | `proposed TreeTopAreaCatalog.GlowingMushroom = 11` | 不可变定义；confirmed | 稳定区域目录值；只定义索引，不定义发光表现资源。 |
| `TreeTopsInfo.AreaId.Underworld` | `proposed TreeTopAreaCatalog.Underworld = 12` | 不可变定义；confirmed | 稳定区域目录值；不拥有地狱背景资源生命周期。 |
| `TreeTopsInfo.AreaId.Count` | `proposed TreeTopAreaCatalog.Count = 13` | 不可变目录边界；confirmed | 约束两个数组的槽位数量；不能运行时由背景/天气系统改写。 |
| `TreeTopsInfo._variations` | `proposed TreeTopVariationStateComponent.Variations` | 可恢复世界状态；confirmed save / partial network receive | `Save`/`Load` 直接读写，`SyncSend` 写 13 个 byte；`RandomizeTreeStyle` 及世界坐标变体入口是候选唯一写者。 |

`TreeTopAreaCatalog` 只保存 13 个稳定索引定义，不应注册为可变 ECS Component。`TreeTopVariationStateComponent`
保存每个区域的树冠变体，是本 checkpoint 唯一有明确存档边界的权威状态；`TreeTopVariationSystem`
负责世界生成随机化、由世界坐标和 Tile 判定区域后随机化，以及显式的变体变更命令。
`TreeTopVariationQuery` 只读取 `GetTreeStyle` 语义。随机数、Tile 读取、世界 X 分区和 `Main.treeX`
通过显式 ports/adapters 输入，不能从 Query 内部取得全局可变随机源或修改 Tile。

`TreeTopPersistenceAdapter` 必须保留 Version4 的文件格式边界：`Save` 先写数组长度再写 int
变体；`Load` 在 `loadVersion < 211` 时调用当前为空的 `CopyExistingWorldInfo`，否则读取长度并限制
在本地数组容量内。旧版本 fallback 的真实行为仍是 evidence-gap，不能因完整参考存在实现就改变
Version4 兼容语义。`TreeTopNetworkProjection` 可以表达 `SyncSend` 的 13 个 byte 输出，但
`MessageBuffer` 的 packet 7 case 当前为空；接收、权限和 stale/duplicate 策略必须由
`crossSubsystemOwner: integration-review` 裁决，不能把投影发送当成客户端状态已闭合。

`BackgroundChangeFlashStateComponent` 是 client/presentation 工作集，不是世界规则组件。
`BackgroundChangeFlashSystem` 可以保留 `UpdateCache` 的 13 项背景输入映射和每 tick 的 0.05
衰减，但必须把空的 `UpdateVariation` 当作兼容事实；在找到 flashPower 触发与消费者前，不得
添加背景闪烁、资源句柄或 UI 输出。`WorldGen.BackgroundsCache` 只是旧静态宿主，不能让
`TreeTopVariationSystem`、天气系统或背景 Projection 反向写入世界变体。

候选生命周期与调度契约为：

```text
world generation/save load/explicit tree-style command
  -> proposed TreeTopVariationSystem writes TreeTopVariationStateComponent
  -> proposed TreeTopVariationQuery and world/background readers
  -> proposed persistence adapter or packet 7 projection

background style inputs
  -> proposed BackgroundChangeFlashSystem updates transient cache and decay
  -> read-only background/sky presentation port
```

存档和网络边界：只允许 `TreeTopVariationStateComponent` 通过版本化 persistence adapter 和明确
的 packet projection 交接；`BackgroundChangeFlashInfo` 两个数组、`TreeTopAreaCatalog` 定义、
XNA/资源句柄、随机流和背景闪烁 payload 不进入世界快照。区域索引、Tile 坐标、`NetworkId`、
`PersistentEntityId` 和任何实体 ID 必须保持不同。packet 7 中同时存在背景样式、树冠变体及其他
world-info 字段，不能把所有同包字段重新聚合成一个 `AmbientSkyComponent`。

focused verifier（未来实现阶段）：验证 13 项目录值和数组容量、每个区域的 `GetTreeStyle` 读路径、
世界生成和世界坐标 Tile 区域映射、随机化不重复规则及变更时 packet 7 发送条件；验证 save/load
版本 211 分界、长度/截断和旧版本空 fallback 的字节兼容性，并单独验证 packet 7 接收为空时不会
伪造客户端提交。另测 `UpdateCache` 的 13 项输入顺序、空 `UpdateVariation` 兼容行为、flashPower
每 tick 的 0.05 clamp、无消费者时的资源释放和不写世界状态。所有结果当前未运行。

### 4.13 `SharedAmbientSpawnAndWindState` -> proposed ambient spawn policy, transient request buffer and wind-scan effect boundaries

本检查点覆盖输入报告中的 11 个成员：`AmbienceServer.AmbienceSpawnInfo` 的两个请求字段、
环境生成间隔和两个资格字典、调度计时器与强制请求列表，以及 `AmbientWindSystem` 的随机源、
Tile 候选列表和更新计数器。直接 Version4 证据来自
`D:\TRbackup\Version4\Terraria.GameContent.Ambience\AmbienceServer.cs:11-28,63-91,93-151,152-230`、
`D:\TRbackup\Version4\Terraria.GameContent.Ambience\SkyEntityType.cs:3-23` 和
`D:\TRbackup\Version4\Terraria.GameContent\AmbientWindSystem.cs:7-44`；初始化、调度和调用边界来自
`D:\TRbackup\Version4\Terraria\Main.cs:1013,11333-11337,11637,1276,3281-3285,13580-13584`。
网络序列化边界来自 `D:\TRbackup\Version4\Terraria.GameContent.NetModules\NetAmbienceModule.cs:9-26`。
这些证据确认了字段形状、资格输入、临时工作集和输出方向，但没有闭合网络接收、风生成算法或
资源生命周期。

完整参考补证读取了
`D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Ambience\AmbienceServer.cs`、
`D:\TRbackup\无任何删减通过编译\Terraria.GameContent\AmbientWindSystem.cs`、
`D:\TRbackup\无任何删减通过编译\Terraria.GameContent.NetModules\NetAmbienceModule.cs`。
完整参考包含 `NetAmbienceModule.Deserialize` 的客户端投影、`AmbientWindSystem` 的 Tile 检查、
候选概率和 Gore 生成，但这些实现不是 Version4 当前基线；它们只作为 Version4 空方法的补证，
不能直接升级为已确认行为。tModLoader stable local mirror `D:\TRbackup\tmodloader-api-docs-stable`
（首页页眉 `tModLoader v2026.07`）的 `class_ambience_server.html`、
`struct_ambience_server_1_1_ambience_spawn_info.html`、`class_ambient_wind_system.html`、
`class_net_ambience_module.html` 和 `class_player.html#ZoneGraveyard` 只交叉确认公开签名；
`AmbienceServer`/`AmbientWindSystem` 私有字段与 Version4 调度仍以源码为准。SS14 参考的
`Content.Shared/Weather/SharedWeatherSystem.cs` 与 `Content.Client/Weather/WeatherSystem.cs`
只支持把天气资格、Tile 读取和客户端效果放在显式 System/Port 边界，不能推断 Terraria 行为。

逐成员 proposed 归属：

| Version4 member | proposed 归属 | 状态分类 / evidence status | 主要读写与边界 |
| --- | --- | --- | --- |
| `AmbienceServer.AmbienceSpawnInfo.skyEntityType` | `proposed AmbientSpawnRequest.SkyEntityType` | 一次性请求值；confirmed shape / partial receiver | `ForceEntitySpawn` 入队，`SpawnForcedEntities` 或普通选择路径读取，最终由 `NetAmbienceModule` 序列化；`SkyEntityType` 是定义 ID，不是实体 ID。 |
| `AmbienceServer.AmbienceSpawnInfo.targetPlayer` | `proposed AmbientSpawnRequest.TargetPlayerSlot` | 目标选择输入；confirmed shape / partial invalid-target policy | `-1` 表示重新选择可见玩家，其他值索引 `Main.player`；它是 Player slot/index，不是 `PlayerEntityId`、`NetworkId` 或 `PersistentEntityId`。 |
| `AmbienceServer.MINIMUM_SECONDS_BETWEEN_SPAWNS` | `proposed AmbientSpawnIntervalPolicy.MinimumSeconds = 10` | 不可变策略；confirmed declaration / partial unit contract | Version4 声明了 10 秒下限，但当前方法以 `Main.rand.Next(600, 7200)` tick 范围设置计时；最终时钟换算由 integration-review 确认。 |
| `AmbienceServer.MAXIMUM_SECONDS_BETWEEN_SPAWNS` | `proposed AmbientSpawnIntervalPolicy.MaximumSeconds = 120` | 不可变策略；confirmed declaration / partial unit contract | 与最小间隔一样不应成为可变 Component；不能把间隔常量和请求队列混合。 |
| `AmbienceServer._spawnConditions` | `proposed AmbientSpawnPolicyCatalog.GlobalConditions` + `proposed AmbientSpawnEligibilityQuery` | 资格策略注册表；confirmed shape / partial external owner | 构造函数注册昼夜、雨、日食、硬模式、风和事件条件；字典委托读取 `Main`/`NPC` 事实，不能由 Query 隐式写状态。 |
| `AmbienceServer._secondarySpawnConditionsPerPlayer` | `proposed AmbientSpawnPolicyCatalog.PerPlayerConditions` + `proposed AmbientSpawnEligibilityQuery` | 每玩家区域资格策略；confirmed shape / partial Player-zone semantics | 读取 Desert/Hallow/Beach/Corrupt/Crimson/Jungle 等玩家区域；Player slot、区域快照和最终跨系统 owner 留给 integration-review。 |
| `AmbienceServer._updatesUntilNextAttempt` | `proposed AmbientSpawnScheduleStateComponent.UpdatesUntilNextAttempt` | 短期世界调度状态；confirmed update / no persistence evidence | `Update` 按 `Main.dayRate` 递减，归零后重置 600..7200；不进入世界快照，重置/世界卸载边界尚未确认。 |
| `AmbienceServer._forcedSpawns` | `proposed AmbientSpawnRequestBuffer.Pending` | 一次性命令/工作缓冲；confirmed queue / partial retry semantics | `ForceEntitySpawn` 添加，处理后按倒序移除，即使没有合格玩家也会消费；不能作为长期 Component 或存档字段。 |
| `AmbientWindSystem._random` | `proposed IAmbientWindRandomPort` | 随机效果依赖；confirmed shape / missing seed owner | 完整参考用它驱动 Tile 候选和 Gore 参数，Version4 方法体为空；随机流必须显式注入，不能藏入纯 Query。 |
| `AmbientWindSystem._spotsForAirboneWind` | `proposed AmbientWindWorkBuffer.CandidateSpots` | 瞬态 Tile 扫描工作集；confirmed shape / missing writer in Version4 | 完整参考在候选发现时写入、每 30 次更新后消费并清空；Version4 没有有效写者，不能假定候选已产生。 |
| `AmbientWindSystem._updatesCounter` | `proposed AmbientWindWorkBuffer.UpdateCounter` | 客户端节拍工作状态；confirmed shape / partial caller/reset | `Update` 只在 `Main.LocalPlayer.ZoneGraveyard` 时递增，30 次触发空气风消费；reset、暂停和世界切换语义仍未闭合。 |

`AmbientSpawnPolicyCatalog` 是不可变资格定义，不注册为可变 ECS Component。`AmbientSpawnScheduleStateComponent`
只承载短期的下一次尝试计时；`AmbientSpawnRequestBuffer` 承载强制生成 Command 的至多一次消费，
不承载世界恢复数据。`AmbientSpawnEligibilityQuery` 接收显式时钟、天气、事件、玩家可见性和区域
快照，只返回候选类型/玩家，不写入 `Main`、Player 或网络。`AmbientSpawnSystem` 是 proposed 的
唯一调度/请求提交 owner，`AmbientSpawnNetworkProjection` 只投影已提交请求。

`AmbientWindWorkBuffer` 将候选 Tile 点和更新计数器作为同一短生命周期工作集，避免把 Tile 坐标、
随机句柄和 Gore 资源塞进世界状态。`AmbientWindScanQuery` 只根据显式 Tile reader、局部玩家区域
和随机结果返回候选动作；`AmbientWindSystem` 负责更新计数/清空工作集，
`AmbientWindPresentationAdapter` 负责 Gore/客户端效果。由于 Version4 的 `SpawnAirborneWind`、
`GetTileWorkSpace` 和 `TrySpawningWind` 为空，本设计不确认完整参考的 120x30 工作区、1/120 或
1/120000 概率、Tile solid 判定和 Gore 参数为 Version4 行为，只把它们列为未来兼容验证输入。

候选依赖方向和顺序：

```text
committed world clock/weather/event/player-zone inputs
  -> proposed AmbientSpawnEligibilityQuery
  -> proposed AmbientSpawnSystem and AmbientSpawnScheduleStateComponent
  -> proposed AmbientSpawnRequestBuffer consumption
  -> proposed AmbientSpawnNetworkProjection / client ambience projection

local-player/graveyard gate and read-only Tile port
  -> proposed AmbientWindScanQuery
  -> proposed AmbientWindWorkBuffer
  -> proposed AmbientWindSystem cadence/clear
  -> proposed AmbientWindPresentationAdapter and effect ports
```

Version4 的直接调用顺序只确认：`AmbienceServer` 在 `Main.Update` 的 `11333-11335` 调用，
强制 Meteor 请求在 `HandleMeteorFall` 的 `13580-13584` 入队；`AmbientWindSystem.Update` 在
`Main` 的 `11637` 调用。最终调度不能由文件顺序决定；天气/季节提交、AmbientSpawn 资格、
请求消费、网络投影和 AmbientWind 表现之间的跨分区顺序标记为 `crossSubsystemOwner: integration-review`。

网络和持久化边界：Version4 `NetAmbienceModule.SerializeSkyEntitySpawn` 写入一个 `byte`
`player.whoAmI`、一个随机 `int` seed 和一个 `byte` `SkyEntityType`；当前 Version4
`Deserialize` 为空并返回默认 `bool`，所以不声称接收、权限、重复和 stale 语义已闭合。完整参考的
接收代码读取同样三项并排队 `AmbientSky.Spawn`，但只能作为冲突/补证记录。`targetPlayer`、
`player.whoAmI`、`NetworkId`、`PersistentEntityId`、Tile 坐标和实体 ID 必须分开建模。
`_updatesUntilNextAttempt`、`_forcedSpawns`、`_random`、`CandidateSpots` 和 `UpdateCounter`
均不进入世界存档；任何客户端 Sky/Gore 资源、随机句柄和网络 payload 只能经 Projection/Port 交接。

当前 NLTX 已有 `WorldWeatherState` 的 `WindSpeedTarget`、`WindSpeedCurrent`、天气计时器和
`WorldWeatherSnapshotValue` 的风速字段，但未发现 `AmbienceServer`、环境生成请求缓冲、
`AmbientWindSystem`、Tile/Gore adapter 或 `NetAmbienceModule` 的对应实现。已有天气状态只能作为
跨 checkpoint 输入候选，不能证明本组 11 个成员已迁移。

证据缺口：`AmbienceServer` 的最终随机流/资格 owner、`targetPlayer` 非法或离线目标的消费策略、
强制请求失败后的至少一次/至多一次语义、NetAmbienceModule 空接收的权限和客户端 Sky 生命周期、
AmbientWindSystem Version4 空方法与完整参考算法的兼容策略、Tile reader、Gore 资源 port、随机
seed、工作缓冲清理、暂停/世界 reset 和调用端生命周期均未闭合。阻塞决策：AmbientSpawn 请求
是否属于天气/天空 owner、SkyEntityType 与 Player/Network ID 的边界、Version4 风系统 stub 是否
必须保持、以及网络接收和客户端效果的最终 owner，均为 `crossSubsystemOwner: integration-review`。

focused verifier（未来实现阶段）：验证 10..120 秒策略与 600..7200 tick 范围、tenth-anniversary
缩短、`dayRate` 递减、无可见玩家、昼夜/天气/区域资格和 60/40 secondary/fallback 选择；验证
强制队列的 `-1` 目标、明确 Player slot、处理后移除和重复命令语义。验证 packet payload 的
player slot/seed/type 宽度，并证明 Version4 空接收不会伪造客户端 Sky 提交。另测 graveyard gate、
工作区和 Tile 越界、候选缓冲 30-update 节拍与 clear、确定性随机 port、Gore 资源释放，以及
Version4 空方法与完整参考行为不得被静默合并。所有结果当前未运行。

### 4.14 `SharedCelebrationAndLanternEvents` -> proposed celebration facts, lantern-night policy and fairy-log effect boundaries

本检查点覆盖输入报告中的 16 个成员：`BirthdayParty` 的 5 个字段和 `PartyIsUp` 属性、
`LanternNight` 的 5 个字段和 `LanternsUp` 属性，以及 `MysticLogFairiesEvent` 的 4 个字段。
Version4 直接证据来自：

- `D:\TRbackup\Version4\Terraria.GameContent.Events\BirthdayParty.cs:13-33,35-63,67-140,141-194`；
- `D:\TRbackup\Version4\Terraria.GameContent.Events\LanternNight.cs:8-28,30-104`；
- `D:\TRbackup\Version4\Terraria.GameContent.Events\MysticLogFairiesEvent.cs:11-17,19-107`；
- `D:\TRbackup\Version4\Terraria\Main.cs:13109-13119,13319-13353,13470-13483`；
- `D:\TRbackup\Version4\Terraria\WorldGen.cs:3351-3354,4259,6452-6454,42751-42754`；
- `D:\TRbackup\Version4\Terraria\NetMessage.cs:310-339`；
- `D:\TRbackup\Version4\Terraria\NPC.cs:45,5705-5735,65249-65270`。

完整参考补证显示了 `BirthdayParty.CanNPCParty`、`CheckForAchievement`、`LanternNight.BossIsActive`、
`LanternNight.NaturalAttempt` 和 fairy spawn/时间算法的实现，以及客户端 Sky/网络模式分支；
这些内容不全部存在于 Version4 当前基线，不能被写成已确认的 Version4 行为。tModLoader
`v2026.07` 本地页面 `class_birthday_party.html`、`class_lantern_night.html` 和
`class_mystic_log_fairies_event.html` 只确认公开成员签名与生命周期入口；SS14 无本组直接事件
对应证据，不能用于推断 Terraria 的庆典或精灵语义。

逐成员 proposed 归属：

| Version4 member | proposed 归属 | 状态分类 / evidence status | 主要读写与边界 |
| --- | --- | --- | --- |
| `BirthdayParty.ManualParty` | `BirthdayPartyStateComponent.ManualOverride` | 权威手动覆盖事实；confirmed field / partial command receiver | `ToggleManualParty` 是直接变更入口；客户端请求、服务端权限和 packet 7 接收仍需 integration-review。 |
| `BirthdayParty.GenuineParty` | `BirthdayPartyStateComponent.GenuineActive` | 权威自然事件事实；confirmed field / partial natural writer | `NaturalAttempt`、`CheckNight`、`UpdateTime` 和世界生成周年规则会写入；不能由派生 `PartyIsUp` 反向写入。 |
| `BirthdayParty.PartyDaysOnCooldown` | `BirthdayPartyStateComponent.DaysOnCooldown` | 冷却状态；confirmed decrement/reset / partial persistence | 每次自然尝试递减，成功时随机设为 `5..10`，世界清理归零；是否存档由整合决定。 |
| `BirthdayParty.CelebratingNPCs` | `BirthdayPartyRosterBuffer.LegacyNpcSlots` plus an `NpcEntityId` resolution port | 短期 roster/关系工作集；confirmed list / unresolved identity | Version4 保存 `whoAmI` slot；不得把 slot 当 `NpcEntityId`、`NetworkId` 或 `PersistentEntityId`，NPC 生成/重用时必须校验版本和活动性。 |
| `BirthdayParty._wasCelebrating` | `BirthdayPartyTransitionCache.PreviousActive` | 派生边沿缓存；confirmed update / no persistence evidence | 只用于检测 party active 边沿并触发表现/packet 投影，不是事件权威事实。 |
| `BirthdayParty.PartyIsUp` | `BirthdayPartyActiveQuery` | 纯派生查询；confirmed formula | `GenuineParty || ManualParty` 的兼容视图；Query 不得写入任一状态。 |
| `LanternNight.ManualLanterns` | `LanternNightStateComponent.ManualOverride` | 权威手动覆盖事实；confirmed field / partial command receiver | `ToggleManualLanterns` 是直接变更入口；最终网络命令和权限边界未闭合。 |
| `LanternNight.GenuineLanterns` | `LanternNightStateComponent.GenuineActive` | 权威自然事件事实；confirmed field / partial natural writer | `NaturalAttempt` 与 `UpdateTime`/morning cleanup 共同形成生命周期；完整参考自然尝试不能升级为 Version4 行为。 |
| `LanternNight.NextNightIsLanternNight` | `LanternNightStateComponent.NextNightRequested` | 一次性触发输入；confirmed field / confirmed NPC event writer | `NPC.OnGameEventClearedForTheFirstTime` 可置为 true，夜间尝试消费；必须由单一夜间 reducer 清除。 |
| `LanternNight.LanternNightsOnCooldown` | `LanternNightStateComponent.NightsOnCooldown` | 冷却状态；confirmed field / partial natural writer | 自然尝试递减，成功后随机设为 `5..10`，世界清理归零；持久化边界未确认。 |
| `LanternNight._wasLanternNight` | `LanternNightTransitionCache.PreviousActive` | 派生边沿缓存；confirmed update / no persistence evidence | 只用于一次性 packet 7/客户端效果边沿，不是可恢复事件事实。 |
| `LanternNight.LanternsUp` | `LanternNightActiveQuery` | 纯派生查询；confirmed formula | `GenuineLanterns || ManualLanterns` 的兼容视图；不能被天气、风或 UI projection 写回。 |
| `MysticLogFairiesEvent._canSpawnFairies` | `MysticFairyEventStateComponent.CanAttemptSpawn` | 短期事件资格状态；confirmed field / partial reset | `StartNight` 开启，成功生成后完整参考会关闭，Version4 `TrySpawningFairies` 为空，因此成功关闭语义尚未确认。 |
| `MysticLogFairiesEvent._delayUntilNextAttempt` | `MysticFairyEventStateComponent.DelayUntilNextAttempt` | 短期调度状态；confirmed decrement / partial unit semantics | `UpdateTime` 按 `Main.dayRate` 递减，归零后设为 60；不进入世界快照。 |
| `MysticLogFairiesEvent.DELAY_BETWEEN_ATTEMPTS` | `MysticFairySpawnPolicy.DelayBetweenAttempts = 60` | 不可变策略；confirmed declaration | 不应作为可变 ECS 状态；时间单位及 day-rate 适配由 Calendar integration-review 确认。 |
| `MysticLogFairiesEvent._stumpCoords` | `MysticFairyLogScanWorkBuffer.CandidateStumps` | Tile 扫描工作集；confirmed scan writer / partial coordinate semantics | `ScanWholeOverworldForLogs` 清空并扫描 tile type 488，`GetStumpTopLeft` 在 Version4 为空；坐标和 Tile reader 必须隔离于长期状态。 |

建议的 proposed 类型和路径：

```text
status: proposed
src2/WorldSession/Environment/Celebration/BirthdayPartyStateComponent.cs
src2/WorldSession/Environment/Celebration/BirthdayPartyRosterBuffer.cs
src2/WorldSession/Environment/Celebration/BirthdayPartyPolicyCatalog.cs
src2/WorldSession/Environment/Celebration/BirthdayPartyActiveQuery.cs
src2/WorldSession/Environment/Celebration/BirthdayPartySystem.cs
src2/WorldSession/Environment/Celebration/BirthdayPartyProjection.cs
src2/WorldSession/Environment/Celebration/BirthdayPartyTransitionCache.cs
src2/WorldSession/Environment/Celebration/LanternNightStateComponent.cs
src2/WorldSession/Environment/Celebration/LanternNightPolicyCatalog.cs
src2/WorldSession/Environment/Celebration/LanternNightActiveQuery.cs
src2/WorldSession/Environment/Celebration/LanternNightSystem.cs
src2/WorldSession/Environment/Celebration/LanternNightProjection.cs
src2/WorldSession/Environment/Celebration/LanternNightTransitionCache.cs
src2/WorldSession/Environment/Celebration/MysticFairyEventStateComponent.cs
src2/WorldSession/Environment/Celebration/MysticFairySpawnPolicy.cs
src2/WorldSession/Environment/Celebration/MysticFairyLogScanWorkBuffer.cs
src2/WorldSession/Environment/Celebration/MysticFairyEligibilityQuery.cs
src2/WorldSession/Environment/Celebration/MysticFairySpawnSystem.cs
src2/WorldSession/Environment/Celebration/ICelebrationTileReadPort.cs
src2/WorldSession/Environment/Celebration/ICelebrationNpcSpawnPort.cs
src2/WorldSession/Environment/Celebration/ICelebrationPresentationPort.cs
```

`BirthdayPartySystem` 和 `LanternNightSystem` 分别拥有各自 manual/genuine/cooldown/next-night
状态的提交；`BirthdayPartyActiveQuery` 与 `LanternNightActiveQuery` 只计算兼容派生值。生日 roster
提交必须经过 `NpcEntityId` resolution port，外部 wire 或旧存档中的 NPC slot 只由 adapter 处理。
`MysticFairySpawnSystem` 拥有 `CanAttemptSpawn` 和 delay 的单一写入，`MysticFairyEligibilityQuery`
只读取日历、天气、事件冲突、Tile scan 和玩家可见性快照；真正的 NPC 创建经过
`ICelebrationNpcSpawnPort`，不由 Query 或 Tile adapter 直接生成。

调用方向和调度边界：

```text
world clock/rules + manual/event commands + NPC event-cleared input
  -> proposed BirthdayPartySystem / LanternNightSystem
  -> proposed active queries and celebration state commit
  -> packet-7 / UI / Sky / NPC behavior projections

world load/night transition + read-only Tile snapshot
  -> proposed MysticFairyLogScanWorkBuffer
  -> proposed MysticFairyEligibilityQuery
  -> proposed MysticFairySpawnSystem
  -> ICelebrationNpcSpawnPort and presentation/network adapters
```

Version4 的明确调用顺序是：`Main.UpdateTime` 先调用两个事件的 `UpdateTime` 和
`mysticLogsEvent.UpdateTime`（`Main.cs:13112-13118`）；入夜阶段调用
`BirthdayParty.CheckNight`、`LanternNight.CheckNight`、`mysticLogsEvent.StartNight`
（`Main.cs:13340-13342`）；入晨阶段调用两个 `CheckMorning`（`Main.cs:13481-13482`）。
世界加载注册 `mysticLogsEvent.StartWorld`，world clear 依次清理生日、灯笼和木桩精灵事件
（`WorldGen.cs:3351-3354,6452-6454`）。这些顺序只能作为 proposed scheduler constraint，不能依靠
文件顺序隐式实现。

网络和持久化边界：Version4 `NetMessage` 的 world-info bits 将 `PartyIsUp` 写入 packet 7 的
`bitsByte9[7]`，将 `LanternsUp` 写入 `bitsByte11[1]`，而 `MessageBuffer` 的 packet 7 receive
保持空白；Birthday manual toggle 使用 packet 111 的 command path。计划只投影已提交的 active
事实，不把 `CelebratingNPCs` roster、冷却、`_was*` cache、fairy stump coordinates 或 Tile
workset 写入 packet 7。完整参考新增的客户端 Sky activation、netMode 分支和 manual state receive
属于补证/冲突记录，必须等 integration-review 决定后才能实施。当前 NLTX 已有
`BirthdayPartyStateComponent`、`LanternNightStateComponent` 和 `WorldTemporaryEventContext` 的
形状，但它们不能证明 Version4 行为、packet 接收或本组 16 个成员已迁移。

证据缺口：`BirthdayParty.CanNPCParty`、`CheckForAchievement`、`LanternNight.BossIsActive`、
`NaturalAttempt`、fairy `IsAGoodTime`/`TrySpawningFairies`/`GetStumpTopLeft` 在 Version4 空实现；
Birthday/Lantern packet 7 receive、manual command 权限、NPC slot 生命周期、roster 持久化、
`NPC.Spawner.fairyLog` 的跨子系统写者、fairy spawn transaction、Sky/achievement/visual consumer
和 Tile coordinate value object 均未闭合。blocking decision 为 `crossSubsystemOwner:
integration-review`：必须先裁决 complete-reference 行为能否补入 Version4 stub、事件 active 与
presentation/天气 owner、packet 7/111 的重复和权限语义，以及 NPC roster/Tile/fairy spawn 的
提交边界。

focused verifier（未来实现阶段）：验证 manual 优先级、genuine/manual active 公式、早晨/夜间
边界、生日 cooldown `5..10`、可庆祝 NPC 最低数量和 roster 清理；验证灯笼夜的禁止事件条件、
`NextNightIsLanternNight` 一次性消费、cooldown 和 `LanternsCanPersist`；验证 packet 7 位宽、
packet 111 command、空 receive 不伪造客户端 authority；验证 fairy log scan 清空/重建、remix
范围、tile type 488、day-rate delay、玩家 LOS、NPC spawn command 和 Version4 空算法不被完整
参考静默替换。所有结果当前未运行。


### 4.15 `SharedRitualAndStormEvents` -> proposed ritual countdown and sandstorm state boundaries

本检查点覆盖输入报告中的 12 个成员：`CultistRitual` 的 6 个字段/常量和
`Sandstorm` 的 6 个字段/常量。Version4 直接证据来自：

- `D:\TRbackup\Version4\Terraria.GameContent.Events\CultistRitual.cs:9-127`：常量、`delay`/
  `recheck` 更新、祭坛破坏后的 delay、资格检查、Tile/LOS 读取、短期 spawn point 工作集和
  NPC type `437` 生成调用。
- `D:\TRbackup\Version4\Terraria.GameContent.Events\Sandstorm.cs:10-139`：持续时间常量、
  四个公开可变字段、风力资格、活动时间扣减、随机启动尝试、强度意图变化、NaN/范围修正以及
  空的 `StartSandstorm`/`StopSandstorm`。
- `D:\TRbackup\Version4\Terraria\Main.cs:12972-13118`：同一 `UpdateTime` 链上
  `CultistRitual.UpdateTime`、Birthday/Lantern、`Sandstorm.UpdateTime` 的调用顺序。
- `D:\TRbackup\Version4\Terraria\NetMessage.cs:310-404`：packet 7 的
  `bitsByte10[3] = Sandstorm.Happening` 和 world-info 中 `Sandstorm.IntendedSeverity` 的
  `float` 写入；`D:\TRbackup\Version4\Terraria\MessageBuffer.cs:385-387,2323-2329`
  表明 packet 7 receive 分支为空，而 `bitsByte10[3]` 只控制后续风暴相关 NPC 读取。
- `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:114-174,1044-1062,1071-1089,1407-1410,
  2323-2336`：`CultistRitual.delay` 及 Sandstorm 四字段的暂存、保存、恢复和旧版本
  `<174` 的 Sandstorm 默认值；没有 `CultistRitual.recheck` 的对应持久化字段。
- `D:\TRbackup\Version4\Terraria\WorldGen.cs:6452-6457`：world clear 明确调用
  `Sandstorm.WorldClear()`，没有发现对应的 `CultistRitual` world-clear 调用。
- `D:\TRbackup\Version4\Terraria\NPC.cs:37449-37528`：NPC 侧读取 `CheckFloor` 并在
  仪式 NPC 路径中触发 `TabletDestroyed`；这使 NPC/战斗与仪式状态之间的提交边界仍为
  `crossSubsystemOwner: integration-review`。

完整参考源码只可补足 Version4 已存在文件中的实现差异；不得用其为 Version4 的空
`StartSandstorm`/`StopSandstorm` 静默补入持续时间赋值、启动或停止副作用。tModLoader 本地
公开文档可用于交叉确认事件成员签名，但不能替代上述私有调用链；SS14 没有本组直接语义证据。

逐成员 proposed 归属：

| Version4 member | proposed 归属 | 状态分类 / evidence status | 主要读写与边界 |
| --- | --- | --- | --- |
| `CultistRitual.delayStart` | `CultistRitualPolicy.DelayStartTicks = 86400` | 不可变策略；confirmed declaration / partial call-graph usage | 当前 Version4 可确认该值为公开常量，但本组调用图未证明运行时直接读取它；不可放入可变组件。 |
| `CultistRitual.respawnDelay` | `CultistRitualPolicy.RespawnDelayTicks = 43200` | 不可变策略；confirmed declaration / partial call-graph usage | `TabletDestroyed` 直接把 `delay` 设为 `43200`；策略值与持久化倒计时分离。 |
| `CultistRitual.timePerCultist` | `CultistRitualPolicy.TimePerCultistTicks = 3600` | 不可变策略；confirmed declaration / evidence-gap | 当前 Version4 直接调用图未找到使用点；保留为策略证据，不声称已接入 ECS 计算。 |
| `CultistRitual.recheckStart` | `CultistRitualPolicy.RecheckStartTicks = 600` | 不可变策略；confirmed declaration / confirmed `UpdateTime` use | `UpdateTime` 在 delay/recheck 均归零时重新设为 `600`，危险 NPC 时乘以 `6`。 |
| `CultistRitual.delay` | `CultistRitualStateComponent.DelayTicks` | 可持久化权威倒计时；confirmed field/update/save-restore | `UpdateTime` 按 `Main.dayRate` 递减并钳制到零，`TabletDestroyed` 重置为 respawn delay；`WorldFile` 有直接保存/恢复证据。 |
| `CultistRitual.recheck` | `CultistRitualStateComponent.RecheckTicks` | 瞬态再检查闸门；confirmed tick/reset / no persistence evidence | 递减到零后触发下一轮尝试；无 `WorldFile` 保存字段，不应未经整合决定进入存档快照。 |
| `Sandstorm.SANDSTORM_DURATION_MINIMUM` | `SandstormPolicy.MinimumDurationTicks = 28800` | 不可变策略；confirmed declaration / no confirmed assignment call | Version4 当前 `StartSandstorm` 为空，不能把该常量推成实际启动时长写入。 |
| `Sandstorm.SANDSTORM_DURATION_MAXIMUM` | `SandstormPolicy.MaximumDurationTicks = 86400` | 不可变策略；confirmed declaration / partial guard use | `UpdateTime` 用 `86400` 作为异常 `TimeLeft` 上限检查；完整 duration assignment 仍是 evidence-gap。 |
| `Sandstorm.Happening` | `SandstormStateComponent.Active` | 可持久化权威活动事实；confirmed field/update/save-restore | `UpdateTime` 读取并通过空 Start/Stop 间接依赖；packet 7 只投影其活动位，world clear 直接置 false。 |
| `Sandstorm.TimeLeft` | `SandstormStateComponent.RemainingTicks` | 可持久化权威计时；confirmed field/update/save-restore | 活动时按 day rate 与风力额外扣减，零风速直接置零并调用空 Stop；不得由投影或查询写入。 |
| `Sandstorm.Severity` | `SandstormStateComponent.Severity` | 可持久化权威/派生表现强度；confirmed field/update/save-restore | `UpdateSeverity` 纠正 NaN，按 `0.003` 逐步趋近目标并限制到 `0..1`；最终 authority 与表现读取者仍需整合确认。 |
| `Sandstorm.IntendedSeverity` | `SandstormStateComponent.TargetSeverity` | 可持久化目标意图；confirmed field/update/save-restore | 随机意图更新后发送 packet 7，world-info 写入 float；packet receive 不可据此推定客户端可回写 authority。 |

建议的 proposed 类型和路径：

```text
status: proposed
src2/WorldSession/Environment/Ritual/CultistRitualStateComponent.cs
src2/WorldSession/Environment/Ritual/CultistRitualPolicy.cs
src2/WorldSession/Environment/Ritual/CultistRitualEligibilityQuery.cs
src2/WorldSession/Environment/Ritual/CultistRitualSystem.cs
src2/WorldSession/Environment/Ritual/ICultistRitualTileReadPort.cs
src2/WorldSession/Environment/Ritual/ICultistRitualNpcSpawnPort.cs
src2/WorldSession/Environment/Storm/SandstormStateComponent.cs
src2/WorldSession/Environment/Storm/SandstormPolicy.cs
src2/WorldSession/Environment/Storm/SandstormSystem.cs
src2/WorldSession/Environment/Storm/SandstormActiveQuery.cs
src2/WorldSession/Environment/Storm/SandstormNetworkProjection.cs
src2/WorldSession/Environment/Storm/SandstormPersistenceAdapter.cs
```

`CultistRitualStateComponent` 只保存 `DelayTicks` 与 `RecheckTicks`，并以字段级持久化标记
区分 delay（confirmed persisted）和 recheck（transient / no persistence evidence）。
`CultistRitualPolicy` 保存四个不可变策略值；其中 `delayStart` 和 `timePerCultist` 当前
只能标记 partial/evidence-gap，不能因为命名而制造新的调用。`CultistRitualSystem` 是
delay/recheck 的单一写入者，并把 `TabletDestroyed` 作为显式命令结果；
`CultistRitualEligibilityQuery` 只根据 hardmode、击杀事实、delay、类型 `437` 存在性、
Tile/碰撞快照和 force policy 返回资格，不创建 NPC。Tile、LOS 和 NPC 创建分别经过
`ICultistRitualTileReadPort`、只读 LOS 输入和 `ICultistRitualNpcSpawnPort`；`CheckFloor` 返回
的四个 `Point` 是一次性工作集，不能放入长期组件。

`SandstormStateComponent` proposed 字段为 `Active`、`RemainingTicks`、`Severity` 和
`TargetSeverity`。`SandstormPolicy` 只保存持续时间上下限、风力阈值、递减步长和随机策略
参数，不把随机数、时钟或 Tile/SceneMetrics 直接藏进组件。`SandstormSystem` 是四字段和
world-clear reset 的单一写入者：活动状态按 day rate/风力递减，非活动状态只在 hardmode、
风力和显式随机输入允许时提交启动请求；`UpdateSeverity` 需要保持 finite、`0..1` 不变量。
由于 Version4 `StartSandstorm` 与 `StopSandstorm` 都为空，建议先以 compatibility seam
保留“调用发生但不自动补赋值”的行为，再由 integration-review 决定是否允许完整参考行为。
`SandstormActiveQuery` 只读取 `Active`/风区快照；`SandstormNetworkProjection` 只能输出
packet 7 活动位和 world-info `TargetSeverity` float；`SandstormPersistenceAdapter` 负责
四字段的兼容读写与版本 `<174` 的 false/zero 默认，不把 packet receive 变成 authority。

调用方向和调度边界：

```text
world clock/rules + dayRate + ritual commands
  -> proposed CultistRitualSystem delay/recheck reducer
  -> proposed CultistRitualEligibilityQuery (read-only Tile/LOS/NPC facts)
  -> ICultistRitualNpcSpawnPort command (NPC type 437)
  -> proposed SandstormSystem active/time/severity reducer
  -> proposed SandstormActiveQuery
  -> packet 7/world-info/persistence projections and adapters
```

Version4 明确将 `CultistRitual.UpdateTime` 放在 `Sandstorm.UpdateTime` 之前；本计划将该顺序
保留为 proposed scheduler constraint，但不会依赖文件顺序。仪式系统在 eligibility query
之后才提交 NPC spawn command；query、Tile/LOS adapter 和 projection 不得写入仪式状态。
Sandstorm reducer 在 committed weather/wind input 可用后运行，packet 7/world-info projection
只观察已提交值。`Sandstorm.WorldClear()` 明确由 WorldGen 调用，而当前证据没有 CultistRitual
对应 world-clear 调用；仪式 reset 应暂留 integration-review，不能擅自添加。

网络与持久化边界：`WorldFile` 直接保存/恢复 `CultistRitual.delay` 和 Sandstorm 四字段；
`recheck` 没有同等证据。Sandstorm packet 7 将 `Happening` 写入 `bitsByte10[3]`，world-info
另外写入 `IntendedSeverity` 的 `float`；`MessageBuffer` packet 7 receive 分支为空，因此
该投影不能作为客户端 authority 或反向写入。Version4 的保存格式在版本 `<174` 将 Sandstorm
四字段恢复为 false/zero；任何新 DTO 必须保留这个 adapter 边界。当前 NLTX 只能作为现状证据，
不能声称上述 proposed 类型已经存在或已迁移。

证据缺口和 blocking decision：`CheckFloor` 的 Tile/碰撞快照、LOS 和 NPC type `437` 生成
事务仍跨 Tile、NPC/战斗和事件子系统；`delayStart`/`timePerCultist` 的实际调用图未闭合；
`recheck` 是否应在恢复后重建仍未决定。Sandstorm 的 duration assignment、风力输入、随机
owner、空 Start/Stop 是否必须保持、world reset 是否也重置 TimeLeft/severity，以及 packet 7
receive/world-info receive 的 stale/duplicate policy 均为 `crossSubsystemOwner:
integration-review`。不得用完整参考静默替换 Version4 空方法。

focused verifier（未来实现阶段）：验证仪式 delay 的 day-rate 递减、零钳制、tablet reset、
recheck 的 `600`/危险 NPC `6x` 门控、force/LOS/边界/硬模式/Boss/类型 `437` 资格、四个
spawn point 的 transient 生命周期和 NPC spawn command 单一提交；验证 `delay` save/load、
`recheck` 不被错误持久化、world reset 边界。另验证 Sandstorm active/time-left 风力分支、
零风速停止请求、NaN 修正、`0..1` finite severity、目标强度随机输入、duration 上限、
packet 7 `bitsByte10[3]`、world-info float、空 receive 和 `<174` 恢复默认，并证明
Start/Stop 的 Version4 stub compatibility 未被完整参考静默替换。所有结果当前未运行。

### 4.16 `SharedSceneWeatherAndEventZoneState` -> proposed scene-derived snapshot and event-zone query boundaries

本检查点覆盖输入报告中的 9 个字段，声明类型为 `Terraria.SceneMetrics`。它们是按场景
扫描中心、帧版本和可选 perspective player 计算出的派生输入，不是世界持久化 authority，
也不是应长期挂在实体上的 `EnvironmentComponent`。Version4 直接证据来自：

- `D:\TRbackup\Version4\Terraria\SceneMetrics.cs:72-88`：九个字段的声明。
- `D:\TRbackup\Version4\Terraria\SceneMetrics.cs:196-230`：`Scan` 以
  `LastScanTime` 和 `BiomeScanCenterPositionInWorld` 判断缓存命中；命中失败时依次
  `Reset`、记录帧/中心、扫描 Tile、可选视觉区域、可选 NPC 位置、聚合 Tile 数量、计算区域，
  最后应用 perspective-player effects。Version4 中这些实际扫描/聚合/区域/玩家效果方法为空，
  因此直接写入算法只能标记为 `partial`/`missing`/`evidence-gap`。
- `D:\TRbackup\Version4\Terraria\SceneMetrics.cs:231-315`：`Reset` 将九个字段和
  相关场景计数、缓存、NPC banner 数组与玩家视角状态清除；这是快照失效边界。
- `D:\TRbackup\Version4\Terraria\Main.cs:12265-12297`：主相机调用
  `SceneMetrics.Scan`，计算视觉扫描区域并传入 biome scan center；有独立 camera/perspective
  player 时先更新 local-player metrics，再选择 perspective player。
- `D:\TRbackup\Version4\Terraria\Player.cs:9926-9935`：Player 实例也会以自身
  中心调用 `SceneMetrics.Scan`，说明不能把 Main camera 的快照错误地当作所有 Player 的共享事实。
- `D:\TRbackup\Version4\Terraria\Player.cs:2701-2941`、
  `D:\TRbackup\Version4\Terraria\NPC.cs:300-316,3865-3866`、
  `D:\TRbackup\Version4\Terraria.GameContent.Events\Sandstorm.cs:128-135` 和
  `D:\TRbackup\Version4\Terraria\SceneState.cs:43-48`：Player、NPC、沙尘暴表现和
  SceneState 读取这些场景结果或其投影；这些读取者不能反向成为场景快照写者。

完整可编译参考中的 `SceneMetrics` zone 公式和 `AddPlayerEffects` 只能补充 Version4 已存在
文件的空实现细节，不能证明 Version4 当前已经具有这些算法。没有发现这九个字段独立的
WorldFile 字段、packet 7/78 DTO 或持久化 owner；网络/存档设计因此保持 `evidence-gap`。

逐成员 proposed 归属：

| Version4 member | proposed 归属 | 状态分类 / evidence status | 主要读写与边界 |
| --- | --- | --- | --- |
| `SceneMetrics.ZoneRain` | `SceneWeatherEventZoneSnapshot.Raining` | 帧级派生快照；声明 `confirmed`，直接 writer `missing` | 由场景扫描和天气输入共同决定；Player、NPC、Rain/天气表现只读投影。 |
| `SceneMetrics.ZoneSandstorm` | `SceneWeatherEventZoneSnapshot.Sandstorm` | 帧级派生快照；声明 `confirmed`，区域计算 `missing` | 与 `SandstormStateComponent.Active` 是不同层次：前者是当前位置区域命中，后者是世界事件 authority。 |
| `SceneMetrics.SurfaceAtmospherics` | `SceneWeatherEventZoneSnapshot.SurfaceAtmospherics` | 帧级派生快照；声明 `confirmed`，计算 `missing` | 依赖 surface/高度和场景扫描输入；不得作为天气事件持久化事实。 |
| `SceneMetrics.UndergroundForShimmering` | `SceneWeatherEventZoneSnapshot.UndergroundForShimmering` | 帧级派生快照；声明 `confirmed`，计算 `missing` | 是微光相关深度条件的场景结果；Tile/坐标值对象 owner 留给整合审查。 |
| `SceneMetrics.ZoneShimmer` | `SceneWeatherEventZoneSnapshot.Shimmer` | 帧级派生快照；声明 `confirmed`，直接 writer `missing` | Player shimmer 读取和 Faeling/表现消费者只读；不得由 Player accessor 写回。 |
| `SceneMetrics.ZoneWaterCandle` | `SceneWeatherEventZoneSnapshot.WaterCandle` | 帧级派生快照；声明 `confirmed`，Tile/player-effect writer `missing` | Tile candle 计数和 perspective-player effect 输入的组合边界未闭合。 |
| `SceneMetrics.ZonePeaceCandle` | `SceneWeatherEventZoneSnapshot.PeaceCandle` | 帧级派生快照；声明 `confirmed`，Tile/player-effect writer `missing` | 与 WaterCandle 的互斥/叠加规则必须由 focused verifier 固定，不能由读者推断。 |
| `SceneMetrics.ZoneShadowCandle` | `SceneWeatherEventZoneSnapshot.ShadowCandle` | 帧级派生快照；声明 `confirmed`，Tile/player-effect writer `missing` | NPC spawn 读取只接受已提交快照；不得把 NPC 的 spawn 调整反写到快照。 |
| `SceneMetrics.InTorchGodMinigame` | `SceneWeatherEventZoneSnapshot.InTorchGodMinigame` | 帧级派生快照；声明 `confirmed`，玩家效果 writer `missing` | Torch God 输入属于 perspective-player effect port；其事件 authority 和区域几何 owner 仍需整合。 |

建议的 proposed 类型和路径：

```text
status: proposed
src2/WorldSession/Environment/Scene/SceneWeatherEventZoneSnapshot.cs
src2/WorldSession/Environment/Scene/SceneWeatherEventZoneScanPolicy.cs
src2/WorldSession/Environment/Scene/SceneWeatherEventZoneScanSystem.cs
src2/WorldSession/Environment/Scene/SceneWeatherEventZoneQuery.cs
src2/WorldSession/Environment/Scene/ISceneMetricsTileReadPort.cs
src2/WorldSession/Environment/Scene/ISceneMetricsPlayerEffectPort.cs
src2/WorldSession/Environment/Scene/SceneWeatherEventZoneProjection.cs
```

`SceneWeatherEventZoneSnapshot` 是不可变、帧范围的 proposed 输出，至少携带 frame version、
scan center、camera/player identity 和九个布尔结果；它不自动持久化、不自动网络化，也不
替代 `WorldCalendar`、`SandstormStateComponent` 或 Tile authority。`SceneWeatherEventZoneScanSystem`
是唯一 proposed writer，负责检查同帧同中心缓存命中、中心变化失效、`Reset` 后扫描和原子提交。
`SceneWeatherEventZoneQuery` 只读已提交快照；`SceneWeatherEventZoneProjection` 只向 Player、
NPC、Rain、SceneState、UI 和表现层输出，不得写入天气/事件 authority。

`ISceneMetricsTileReadPort` 隔离 Tile、wall、liquid、candle 和扫描区域输入；
`ISceneMetricsPlayerEffectPort` 隔离 perspective player 所持 candle/Torch God 等效果输入。
TileCenter、世界坐标 value object、frame version、camera/player identity 和跨子系统快照
所有权均标记 `crossSubsystemOwner: integration-review`。不得把 Player slot、NPC slot、
NetworkId 或 PersistentEntityId 混同为快照身份。

建议调度契约：

```text
world weather/sandstorm/player-camera inputs
  -> SceneMetrics reset/cache check
  -> Tile/liquid and player-effect scan ports
  -> SceneWeatherEventZoneSnapshot commit
  -> SceneWeatherEventZoneQuery
  -> Player/NPC/Rain/SceneState/UI projections
```

同一 frame version 和同一 scan center 必须命中既有快照；中心变化、perspective player 变化、
新帧或 world reset 必须先失效并重建。Main camera metrics 与 Player-owned metrics 维持独立
生命周期，不能通过共享可变快照互相覆盖。`SceneMetrics.Reset` 必须清除九个字段，且提交失败
时不得留下半更新快照。查询、投影、Tile port 和 player-effect port 都是读边界；唯一写入者
仍是 proposed scan system。

网络、持久化和兼容边界：当前没有九个字段的独立 WorldFile 或 packet 7/78 证据，不提出
SceneWeather snapshot 的 save/network DTO。若未来 Player、SceneState 或客户端协议需要值，
只能由显式 projection/adaptor 生成，并保留 frame/center freshness 检查；不能把区域派生值
升级为世界 authority。完整参考算法、Player effects 和 Sandstorm visual 读者必须在实现前
分别验证，不得用完整参考静默替换 Version4 的空方法。

focused verifier（未来实现阶段）：验证同帧同中心 cache hit、中心变化触发 reset/rescan、
visual scan area 和 perspective-player isolation；验证 Reset 清空全部九个字段且不会跨帧泄漏；
验证 rain/sandstorm 区域互斥边界、surface atmosphere、地下微光深度、ZoneShimmer、三类
candle 的 Tile/player-effect 输入和 Torch God 输入；验证 Player/NPC/Rain/SceneState 只能读
projection，不能写 authority；验证空的 Version4 scan/calculation 方法不会被完整参考静默替换。
所有结果当前未运行。

### 4.17 `SharedInvasionDamageTrackingState` -> proposed DD2 damage-tracking session and outcome projection

本检查点覆盖输入报告中的 4 个成员。它描述的是 DD2 事件范围内的伤害跟踪会话和结果
投影，不应把 `NPCDamageTracker` 的全部继承字段复制成一个新的实体 Component。Version4
直接证据来自：`DD2Event.cs:19-27` 的 `_won`、`Name`、`KillTimeMessage` 和空的
`IncludeDamageFor`；`DD2Event.cs:28-35` 的 `Stop(bool won)`；`DD2Event.cs:59` 的静态
`_damageTracker`；`DD2Event.cs:175-203` 的创建/注册；`DD2Event.cs:337-347` 的失败停止；
`NPCDamageTracker.cs:207-275` 的 Start/Reset/Update/StopTracking/Stop 生命周期；以及
`WorldGen.cs:6562-6568` 的世界重置调用。NPC 伤害和死亡入口分别在 `NPC.cs` 的伤害提交与
`BossKilled` 路径中出现。成功停止路径经过 `StopInvasion(win: true)`，但 Version4 的
`WinInvasionInternal` 为空，因此成功时 tracker 是否停止仍是 evidence-gap。

| Version4 member | proposed 归属 | 状态分类 / evidence status | 主要读写与边界 |
| --- | --- | --- | --- |
| `DD2Event.DamageTracker._won` | `proposed Dd2InvasionDamageTrackingSession.Won` | 会话结果状态；声明 confirmed、成功提交 partial | 只由 proposed damage-tracking system 在明确的胜负停止命令中写入；`KillTimeMessage` 只读该结果。 |
| `DD2Event._damageTracker` | `proposed Dd2InvasionDamageTrackingSession` 的活动句柄 | 事件范围聚合/会话句柄；创建和清理 confirmed、最终 owner unresolved | 由 DD2 开始路径创建并注册；NPCDamageTracker 的基类更新仍通过显式 port 进入，不复制继承字段或隐式共享静态状态。 |
| `DamageTracker.Name` | `proposed Dd2InvasionDamageTrackingNameQuery.Name` | 本地化只读查询；confirmed | 从固定 Old Ones Army 本地化键读取；Query 不创建、停止或修改 tracker。 |
| `DamageTracker.KillTimeMessage` | `proposed Dd2InvasionDamageOutcomeProjection.KillTimeMessage` | 结果/表现投影；confirmed、投影调用者 partial | 按 `Won` 选择 defeated/lost 文本；消息、日志和 UI 只能观察已提交 outcome。 |

建议的 proposed 类型和路径：

```text
status: proposed
src2/WorldSession/Events/Dd2/Dd2InvasionDamageTrackingSession.cs
src2/WorldSession/Events/Dd2/Dd2InvasionDamageTrackingSystem.cs
src2/WorldSession/Events/Dd2/Dd2InvasionDamageTrackingNameQuery.cs
src2/WorldSession/Events/Dd2/Dd2InvasionDamageOutcomeProjection.cs
src2/WorldSession/Events/Dd2/IDd2InvasionDamageTrackerPort.cs
```

`Dd2InvasionDamageTrackingSession` 是事件范围的 proposed 聚合/会话值，不强行作为实体
Component；它只保留本组明确可见的活动句柄和胜负结果。`Dd2InvasionDamageTrackingSystem`
是唯一 proposed writer，负责 Start、NPCDamageTracker 更新入口、失败/成功 Stop 和 world
reset；`NameQuery` 与 `OutcomeProjection` 都是只读边界。`IncludeDamageFor` 在 Version4 中
明确返回 `false`，不能因为完整参考实现存在过滤逻辑而静默补齐。

生命周期顺序必须是：DD2 开始命令提交运行状态 -> 创建并注册 damage session -> NPC 伤害/击杀
通过显式 tracker port 提交 -> 失败或已确认成功路径提交一次 outcome -> 输出 KillTimeMessage
投影 -> 清理 session。`StopInvasion(win: true)` 调用链和空的 `WinInvasionInternal` 必须在
integration-review 中决定，不能由投影推断成功停止。世界重置必须清理 session 和基类 tracker
工作集；Query、Projection、NPC reader 不得反向写入 `Won` 或活动句柄。

当前没有为这 4 个成员发现独立 WorldFile、packet 7/78 或事件快照 DTO；本检查点不提出
伤害 tracker 的持久化或网络 authority。若未来需要向 UI/日志/网络输出，只能从已提交 outcome
生成单向 projection，并把 `NpcEntityId`、NetworkId、PersistentEntityId 与事件 session
identity 分开，全部标记 `crossSubsystemOwner: integration-review`。

focused verifier（未来实现阶段）：验证 start 只创建一个 tracker、NPC damage/update/kill
提交顺序、Reset 后不残留会话、失败 `Stop(false)` 的消息选择、成功路径的 unresolved
`WinInvasionInternal` 不被猜测补齐、`Name` 查询不产生副作用、空 `IncludeDamageFor` 的
Version4 兼容行为，以及 tracker outcome 不能被 UI、日志或网络投影反向修改。所有结果当前
未运行。

### 4.18 `SharedInvasionWaveAndArenaState` -> proposed DD2 progression, wave, arena and drop boundaries

本检查点覆盖输入报告中的 22 个成员。Version4 直接证据来自 `DD2Event.cs:37-91` 的常量、
持久进度、运行状态、竞技场、死亡位置、波次计时和派生属性；`DD2Event.cs:93-129` 的
Save/Load 与世界重置；`DD2Event.cs:138-173` 的波次暂停计时；`DD2Event.cs:175-203` 的
启动初始化；`DD2Event.cs:234-335` 的进度检查；`DD2Event.cs:497-550` 的竞技场计算；
`DD2Event.cs:564-680` 的水晶、暂停和查询边界；以及 `NPC.cs:41455`、`65723-65730`、
`65754-65757` 的竞技场/掉落调用。`SetEnemySpawningOnHold` 在 Version4 为空，不能使用
完整参考实现替换这一事实。

| Version4 member | proposed 归属 | 状态分类 / evidence status | 主要读写与边界 |
| --- | --- | --- | --- |
| `INFO_NEW_WAVE_COLOR` | `proposed Dd2InvasionPresentationPolicy.NewWaveColor` | 不可变表现策略；confirmed | 只提供新波次消息颜色，不进入运行状态或实体 Component。 |
| `INFO_START_INVASION_COLOR` | `proposed Dd2InvasionPresentationPolicy.StartInvasionColor` | 不可变表现策略；confirmed | 只提供开始消息颜色；最终 ChatColors.World 适配器待 integration-review。 |
| `INFO_FAILURE_INVASION_COLOR` | `proposed Dd2InvasionPresentationPolicy.FailureInvasionColor` | 不可变表现策略；confirmed | 只提供失败消息颜色，不写入 LostThisRun。 |
| `INVASION_ID` | `proposed Dd2InvasionDefinition.InvasionId` | 事件/packet kind 定义；confirmed | 值 `3` 仅是 DD2 入侵类型定义，不能作为 EntityId、NetworkId 或 PersistentEntityId。 |
| `DownedInvasionT1` | `proposed Dd2InvasionProgressionStateComponent.DownedTier1` | 持久世界进度；声明、Save/Load、reset confirmed | 由进度系统唯一写入，WorldFile adapter 负责恢复；不与本次运行状态双写。 |
| `DownedInvasionT2` | `proposed Dd2InvasionProgressionStateComponent.DownedTier2` | 持久世界进度；声明、Save/Load、reset confirmed | 同上，保持三个 tier 的独立存档字段和世界重置语义。 |
| `DownedInvasionT3` | `proposed Dd2InvasionProgressionStateComponent.DownedTier3` | 持久世界进度；声明、Save/Load、reset confirmed | 同上；版本化 WorldFile DTO 仍是 proposed。 |
| `LostThisRun` | `proposed Dd2InvasionRunStateComponent.LostThisRun` | 当前运行事实；声明、失败写入 confirmed | 失败停止路径写入；不能被消息颜色或只读 Query 改变。 |
| `WonThisRun` | `proposed Dd2InvasionRunStateComponent.WonThisRun` | 当前运行事实；成功写入 partial | 成功停止路径和 `WinInvasionInternal` 空实现之间存在 evidence-gap。 |
| `LaneSpawnRate` | `proposed Dd2InvasionWavePolicy.LaneSpawnRate` | 生成策略输入；声明和 NPC 使用 confirmed | 保持 `60` 默认值及 NPC 生成读取；是否是可持久化配置由 integration-review 决定。 |
| `Ongoing` | `proposed Dd2InvasionRunStateComponent.Ongoing` | 当前运行事实；confirmed | DD2 运行开始/停止路径唯一写入；spawn 与 arena Query 只读。 |
| `ArenaHitbox` | `proposed Dd2InvasionArenaStateComponent.ArenaHitbox` | 临时竞技场快照；计算路径 confirmed、完整 writer partial | `FindArenaHitbox` 提交矩形；BuildingBlock Query 只读，不把 Rectangle 当作持久实体位置。 |
| `_arenaHitboxingCooldown` | `proposed Dd2InvasionArenaStateComponent.RefreshCooldown` | 临时刷新节流；confirmed | 由 arena system 递减/清零；不得由 `ShouldBlockBuilding` 反向修改。 |
| `OngoingDifficulty` | `proposed Dd2InvasionRunStateComponent.Difficulty` | 当前运行规则；confirmed | 启动路径设置，wave/spawn Query 只读；跨快照 owner 待 integration-review。 |
| `_deadGoblinSpots` | `proposed Dd2InvasionDeathPositionBuffer.Positions` | 临时工作缓冲；追加边界 confirmed、消费边界 missing | `AnnounceGoblinDeath` 追加位置；消费、清空和 world-reset 时机需显式归属，不作为持久 Component。 |
| `_crystalsDropping_lastWave` | `proposed Dd2InvasionCrystalDropStateComponent.LastWave` | 掉落进度；声明和调用 confirmed | 由 crystal-drop system 唯一写入，NPC 掉落 Query 只读取已提交值。 |
| `_crystalsDropping_toDrop` | `proposed Dd2InvasionCrystalDropStateComponent.ToDrop` | 掉落进度；confirmed | 与 already-dropped 一起原子更新，避免同一 NPC/波次重复掉落。 |
| `_crystalsDropping_alreadyDropped` | `proposed Dd2InvasionCrystalDropStateComponent.AlreadyDropped` | 掉落进度；confirmed | 由掉落提交根更新并在明确波次边界清理；不能由表现或物品投影写回。 |
| `_timeLeftUntilSpawningBegins` | `proposed Dd2InvasionWaveStateComponent.TimeLeftUntilSpawningBegins` | 波次暂停计时；声明、UpdateTime、启动和进度写入 confirmed | Wave system 唯一递减/设置；`EnemySpawningIsOnHold` 是派生查询。 |
| `ReadyToFindBartender` | `proposed Dd2InvasionBartenderReadinessQuery.IsReady` | 派生资格 Query；confirmed | 只读取 `NPC.downedBoss2`；Query 不改变 DD2 进度或 NPC 状态。 |
| `TimeLeftBetweenWaves` | `proposed Dd2InvasionWaveStatusQuery.TimeLeftBetweenWaves` over `Dd2InvasionWaveStateComponent` | 兼容读写边界；getter/setter confirmed | 读取走 Query，设置走 proposed wave command；不暴露任意外部字段写入。 |
| `EnemySpawningIsOnHold` | `proposed Dd2InvasionWaveStatusQuery.EnemySpawningIsOnHold` | 派生资格 Query；confirmed | 由 `TimeLeftUntilSpawningBegins != 0` 计算；不复制成第二个可写 bool。 |

建议的 proposed 类型和路径：

```text
status: proposed
src2/WorldSession/Events/Dd2/Dd2InvasionDefinition.cs
src2/WorldSession/Events/Dd2/Dd2InvasionPresentationPolicy.cs
src2/WorldSession/Events/Dd2/Dd2InvasionProgressionStateComponent.cs
src2/WorldSession/Events/Dd2/Dd2InvasionRunStateComponent.cs
src2/WorldSession/Events/Dd2/Dd2InvasionWaveStateComponent.cs
src2/WorldSession/Events/Dd2/Dd2InvasionWavePolicy.cs
src2/WorldSession/Events/Dd2/Dd2InvasionArenaStateComponent.cs
src2/WorldSession/Events/Dd2/Dd2InvasionCrystalDropStateComponent.cs
src2/WorldSession/Events/Dd2/Dd2InvasionDeathPositionBuffer.cs
src2/WorldSession/Events/Dd2/Dd2InvasionProgressionSystem.cs
src2/WorldSession/Events/Dd2/Dd2InvasionWaveSystem.cs
src2/WorldSession/Events/Dd2/Dd2InvasionArenaSystem.cs
src2/WorldSession/Events/Dd2/Dd2InvasionCrystalDropSystem.cs
src2/WorldSession/Events/Dd2/Dd2InvasionWaveStatusQuery.cs
src2/WorldSession/Events/Dd2/Dd2InvasionArenaQuery.cs
src2/WorldSession/Events/Dd2/Dd2InvasionBuildingBlockQuery.cs
src2/WorldSession/Events/Dd2/Dd2InvasionBartenderReadinessQuery.cs
src2/WorldSession/Events/Dd2/Dd2InvasionNetworkProjection.cs
src2/WorldSession/Events/Dd2/Dd2InvasionPersistenceAdapter.cs
```

`Dd2InvasionProgressionStateComponent` 只保存三个 `DownedInvasionT*` 世界进度字段；
`Dd2InvasionRunStateComponent` 保存本次运行的 `LostThisRun`、`WonThisRun`、`Ongoing` 和
`OngoingDifficulty`；`Dd2InvasionWaveStateComponent` 保存波次暂停计时；Arena、CrystalDrop
和 DeathPositionBuffer 分别承载临时竞技场、掉落进度和死亡位置工作集。颜色放在不可变
presentation policy，`INVASION_ID` 放在 event definition，不能混入上述状态组件。

建议唯一写入方向：progression system -> 三个 downed tier；run system -> run facts；wave
system -> lane policy consumption and pause timer；arena system -> hitbox/cooldown；crystal-drop
system -> three drop counters；death-position buffer owner -> append/consume/clear。所有 Query
只读，`Dd2InvasionNetworkProjection` 和 presentation policy 只观察已提交状态。`SetEnemySpawningOnHold`
在 Version4 为空，proposed system 必须保留该兼容证据并把任何补充行为单独标为 integration-review。

网络、快照和持久化边界：`DownedInvasionT1/T2/T3` 有 Save/Load 和世界重置证据，应通过
proposed `Dd2InvasionPersistenceAdapter` 做版本化恢复；本次运行、ArenaHitbox、cooldown、
dead spots、crystal counters 和 wave pause 没有独立 WorldFile DTO 证据，不提出持久化迁移。
`INVASION_ID` 可以参与事件/packet kind 投影，但不得伪装为实体身份；所有事件快照、NetworkId、
PersistentEntityId 和 NPC identity 仍为 `crossSubsystemOwner: integration-review`。没有证据时，
网络投影只输出已提交的只读视图，不创建第二个波次写者。

focused verifier（未来实现阶段）：验证三档进度 Save/Load、world reset、启动/失败/成功运行
标志、LaneSpawnRate 默认值和 NPC 读取；验证波次暂停递减、零值/非零 hold、skip wait 和日率
为零；验证 `FindArenaHitbox` 的刷新 cooldown、`ShouldBlockBuilding` 只读行为和矩形边界；验证
死亡位置缓冲追加/消费/清空；验证三项水晶计数在 last-wave、to-drop、already-dropped 组合下
不重复提交；验证 bartender readiness、event ID 身份隔离、网络/快照不反写 authority；并单独
验证 Version4 空 `SetEnemySpawningOnHold` 不被完整参考实现静默替换。所有结果当前未运行。

P02 输入报告的 17 个细分组现已全部逐项完成 proposed 映射：212 个字段与 13 个属性合计
225 条成员；没有 pending 组件组。34 个可独立表达的 Component 已保存到
`D:\TRbackup\NLTX\src2\WorldSession`，但这不表示代码注册、网络、存档或行为等价已经闭合。

## 6. 初步依赖契约

当前仅冻结方向，不冻结跨分区 owner：

```text
WorldSession clock/rules (candidate)
  -> proposed SeasonalCalendarSystem
  -> proposed InvasionLifecycleSystem
  -> proposed WorldInvasionStateComponent
  -> proposed InvasionSpawnQuery
  -> NPC/Spawn command or read-only projection
  -> Network / UI / log adapters
```

`crossSubsystemOwner: integration-review` 候选包括 `WorldTime`、`EntityId`、`NpcEntityId`、
`NetworkId`、`PersistentEntityId`、世界事件快照、随机流、NPC progress command 和 NPC spawn result。P02 不单方面
创建、命名或裁决这些共享类型。

## 7. focused verifier 计划（未运行）

- 史莱姆雨启动前置条件：已有雨、remix world、无 surface、重复启动必须保持拒绝语义。
- 正/负 `TimeState` 递减、活动结束、冷却和 warning message 的边界。
- 同一 Tick 的 NPC 击杀计数、阈值重置和事件停止不得重复提交。
- `slimeRainNPC` 资格缓存的初始化、世界清理、内容注册后重建和未知 NPC 类型处理。
- 只读 eligibility query 不得改变组件；消息、网络和日志只观察已提交状态。
- 入侵启动/移动/延迟/完成、NPC 击杀进度和进度表现必须只有一个明确提交方向；验证 packet 7/78
  的宽度、重复命令、普通进度派生和月事件 `-1` sentinel 的兼容边界。

建议 verifier 在未来实施阶段使用 NLTX 既有受影响项目和仓库规定的串行 wrapper；本次交接前未有
本会话的独立验证结果，串行 Component 项目构建结果由 execution 文档记录。测试、行为等价和
跨分区集成仍未验证。

## 8. Integration Handoff

subsystemId: proposed WorldEnvironmentAndEventsNonAuthoritative
taskNumber: second-round-component-plan-P02
reportPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P02-world-environment-events-component-design.md

evidenceStatus: partial; all seventeen P02 checkpoints are written through SharedInvasionWaveAndArenaState and all 225 source members have proposed mappings; unresolved evidence gaps remain
nltxStatus: partial; 34 independently expressible Component files are saved under src2/WorldSession; production src and non-Component integration remain untouched
verificationStatus: partial (serial build passed; focused verifier and behavior-equivalence not run)

confirmedOwners:
- Version4 `Main.StartSlimeRain`/`StopSlimeRain` owns the direct transition of slime-rain active/time/count fields.
- Version4 `Main.UpdateSlimeRainWarning` owns warning countdown and world-message trigger.
- Version4 NPC paths read slime-rain eligibility and update the event kill counter; this is a cross-subsystem write boundary.
- Version4 `Main.StartInvasion` is the direct invasion initialization root and `Main.UpdateInvasion` is the direct movement/completion/warning trigger root.
- Version4 NPC death processing directly decrements invasion remaining size and calls `ReportInvasionProgress`; the final ECS progress commit owner remains `crossSubsystemOwner: integration-review`.
- Version4 `Main.SyncAnInvasion`/`ReportInvasionProgress` are direct progress presentation/projection roots, not proof that progress is independent authority.
- `CreditsRollEvent` owns the direct Version4 credits countdown start/tick/reset operations and emits packet 140; packet subtype 0 receive ownership remains open.
- `MoonlordDeathDrama.RequestLight` owns per-frame light-request accumulation and `MoonlordDeathDrama.Update` owns client-side consumption/clearing and whitening smoothing; piece/explosion creation and draw ownership remain open.
- `ScreenObstruction.Update` owns the direct smoothing writes to `lastSpeed` and `screenObstruction`; its external caller, reset path and render consumer remain open.
- `BannerSystem.AddNPCKillBy` is the direct Banner kill/threshold root; `Clear`, `Save`/`Load`, and the NetBannersModule writers own the observed reset, persistence and outbound packet boundaries. Banner receive and claim-consumption owners remain open.
- `NPCDamageTracker` creates/updates/resets tracker sessions; `BossDamageTracker` owns the direct `_killed` transition and definition/name/message behavior for its subclass fields. Credit-entry and recent-session integration remain open.
- `Main.startPumpkinMoon`, `Main.startSnowMoon` and the ordinary invasion start path create `InvasionDamageTracker`; Version4's `IncludeDamageFor` and `CheckActive` are stub/empty, while the complete-reference implementation is supplementary evidence only.
- `StormLightning.GenerateMainBoltPath` is the direct Version4 entry for the lightning payload; `LightningGenerator.Generate` delegates recursive geometry and optional rotations, while `Projectile` consumes the returned Bolt. The current recursive/rotation methods are stubs, so algorithm equivalence is not claimed.
- `Main` owns the observed `WaterfallManager` reference and constructs it during initialization; `WaterfallManager.BindTo` only registers the Preferences load callback. No direct Version4 waterfall slot writer, clear/update loop, draw caller or texture-load/release owner was found in the inspected evidence.
- `SpelunkerProjectileHelper` owns the observed transient position/Tile de-duplication sets, clamp reconstruction and ten-frame reset boundary; its `CheckSpot` body is empty, so the scan result writer remains open.
- `UnbreakableWallScan` owns the observed 250-step and eight-direction query policy; `LineScan`/`InsideUnbreakableWalls` read Tile facts, while Player cooldown/result commit and empty network methods remain outside this checkpoint.
- `VoidLensHelper` owns one-shot position, opacity and frame inputs used by its update path; lighting, random, Dust and final draw/resource consumers remain explicit effect-port boundaries.
- `TreeTopsInfo` owns the observed 13 stable area IDs, variation array, save/load version-211 boundary and outbound 13-byte sync payload; `RandomizeTreeStyle` and its world-position variant method are direct variation-change roots, while packet 7 receive remains open.
- `BackgroundChangeFlashInfo.UpdateFlashValues` owns the observed per-update `0.05` clamp decay; `UpdateVariation` is empty in Version4, and no save/network owner or confirmed background-flash consumer was found.
- `AmbienceServer` owns the observed ambient spawn policy registries, retry timer and forced-request queue; `AmbienceSpawnInfo` carries a `SkyEntityType` plus a Player slot target, while the final visibility/eligibility and request-commit owner remains open.
- `AmbientWindSystem.Update` is the observed local/graveyard-gated cadence root; its Version4 scan, workspace and spawn methods are empty, so complete-reference Tile/random/Gore behavior remains supplementary rather than confirmed.
- `BirthdayParty` owns the observed manual/genuine party facts, cooldown, celebrating-NPC slot roster and active-edge cache; `PartyIsUp` is a derived `GenuineParty || ManualParty` query rather than an authority.
- `LanternNight` owns the observed manual/genuine lantern facts, next-night request, cooldown and active-edge cache; `LanternsUp` is a derived `GenuineLanterns || ManualLanterns` query rather than an authority.
- `MysticLogFairiesEvent` owns the observed night eligibility flag, retry delay and stump-coordinate scan workset; `NPC.Spawner.fairyLog` and the Version4-empty fairy qualification/spawn/coordinate methods remain cross-subsystem and incomplete.
- `CultistRitual` owns the observed delay/recheck countdown fields and ritual eligibility entry points; `delay` has WorldFile save/restore evidence, `recheck` has no persistence evidence, and Tile/LOS/type `437` NPC creation remain integration-review transaction boundaries.
- `Sandstorm` owns the observed active/time/severity/target-severity fields; WorldFile saves/restores all four, packet 7 writes `bitsByte10[3]`, world-info writes the target-severity float, packet 7 receive is empty, and Version4 `StartSandstorm`/`StopSandstorm` are empty stubs.
- `SceneMetrics.Scan` owns the observed frame/center cache gate and scan ordering, and `SceneMetrics.Reset` owns invalidation and clearing of the nine scene fields; the Version4 Tile scan, aggregation, zone calculation, NPC scan and player-effect methods are empty, so their direct writer remains an explicit evidence gap.
- `DD2Event.DamageTracker` exposes the DD2 name and win-dependent kill-time message; `Stop(bool won)` writes the local outcome before delegating to the base tracker, while Version4 `IncludeDamageFor` returns `false` and the successful stop path remains incomplete.
- `DD2Event.StartInvasion` creates/registers the DD2 damage tracker, failure calls `Stop(false)`, and world reset calls the tracker reset path; the proposed session must not duplicate inherited `NPCDamageTracker` state or infer missing success cleanup.
- `DD2Event` owns the observed DD2 progression save/load/reset, wave pause timer, arena hitbox/cooldown, crystal-drop counters and derived readiness/hold properties; `FindArenaHitbox`, `ShouldDropCrystals`, and NPC drop callers define the observed read/write boundaries, while `SetEnemySpawningOnHold` is empty in Version4.

proposedTypes:
- proposed `SlimeRainStateComponent`
- proposed `SlimeRainPolicyComponent`
- proposed `SlimeRainProgressComponent`
- proposed `SlimeRainSystem`
- proposed `SlimeRainWarningSystem`
- proposed `SlimeRainEligibilityQuery`
- proposed `SlimeRainNetworkProjection`
- proposed `CalendarClockComponent`, `WorldWeatherState`, `WorldCalendarOverrideStateComponent`
- proposed `CoinRainEventStateComponent`, `CalendarTransitionSystem`, `RainWeatherSystem`
- proposed `SeasonalMoonTransitionSystem`, `CalendarVisualProjection`, `WeatherNetworkProjection`
- proposed `WorldInvasionStateComponent`, `InvasionProgressPresentationStateComponent`
- proposed `InvasionLifecycleSystem`, `InvasionProgressSystem`, `InvasionSpawnQuery`
- proposed `InvasionWarningProjection`, `InvasionProgressProjection`, `InvasionNetworkAdapter`, `InvasionPersistenceAdapter`
- proposed `SeasonalWorldStateComponent`, `SeasonalOverridePolicyComponent`, `TitleRefreshRequestStateComponent`
- proposed `SeasonalCalendarSystem`, `SeasonalOverrideCommand`, `SecretSeedSeasonalPolicyAdapter`, `SeasonalWorldProjection`, `TitleRefreshProjection`
- proposed `CreditsRollPresentationPolicy`, `CreditsRollPresentationStateComponent`, `CreditsRollSystem`, `CreditsRollNetworkProjection`, `CreditsRollNetworkAdapter`, `CreditsRollSkyAdapter`
- proposed `MoonlordPiecePresentation`, `MoonlordExplosionPresentation`, `MoonlordDeathDramaPresentationAdapter`, `MoonlordLightRequestBuffer`, `MoonlordDramaSystem`, `MoonlordPieceLifetimeQuery`, `MoonlordExplosionLifetimeQuery`, `MoonlordDeathDramaProjection`
- proposed `ScreenObstructionPresentationStateComponent`, `ScreenObstructionTargetQuery`, `ScreenObstructionSystem`, `ScreenObstructionProjection`
- proposed `BannerCatalogPolicy`, `BannerKillProgressStateComponent`, `BannerClaimableCountStateComponent`, `BannerClaimNotificationStateComponent`, `BannerProgressSystem`, `BannerPersistenceAdapter`, `BannerNetworkProjection`, `BannerNetworkAdapter`
- proposed `BossTrackingDefinition`, `BossEncounterOutcomeStateComponent`, `BossTrackingNameQuery`, `BossTrackingMessageProjection`, `BossTrackingLifecycleAdapter`
- proposed `InvasionDefinitionCatalog`, `InvasionTrackingSessionScope`, `InvasionTrackingNameProjection`, `InvasionTrackingMessageProjection`, `InvasionDamageEligibilityQuery`, `InvasionTrackingLifetimeSystem`
- proposed `LightningGenerationPolicy`, `LightningBoltPresentationPayload`, `LightningBoltSegmentPayload`, `LightningBoltCollisionResult`, `LightningGenerationPort`, `LightningBoltGenerationSystem`, `LightningProjectionAdapter`
- proposed `WaterfallGenerationPolicy`, `WaterfallCapacityPolicy`, `WaterfallSlotPayload`, `WaterfallSlotBuffer`, `WaterfallTextureCatalog`, `WaterfallGenerationQuery`, `WaterfallGenerationSystem`, `WaterfallConfigurationAdapter`, `WaterfallPresentationAdapter`
- proposed `SpelunkerScanPolicy`, `SpelunkerScanWorkBuffer`, `SpelunkerScanFrameBoundarySystem`, `SpelunkerScanSystem`, `SpelunkerSpotQuery`, `UnbreakableWallScanPolicy`, `UnbreakableWallScanQuery`, `IUnbreakableWallTileReadPort`, `VoidLensPresentationPayload`, `VoidLensPresentationSystem`, `VoidLensProjectionAdapter` and effect ports described in checkpoint 10
- proposed `TreeTopAreaCatalog`, `TreeTopVariationStateComponent`, `TreeTopVariationSystem`, `TreeTopVariationQuery`, `TreeTopPersistenceAdapter` and `TreeTopNetworkProjection`
- proposed `BackgroundChangeFlashStateComponent`, `BackgroundChangeFlashSystem` and `BackgroundPresentationPort`
- proposed `AmbientSpawnRequest`, `AmbientSpawnIntervalPolicy`, `AmbientSpawnPolicyCatalog`, `AmbientSpawnScheduleStateComponent`, `AmbientSpawnEligibilityQuery`, `AmbientSpawnSystem`, `AmbientSpawnRequestBuffer` and `AmbientSpawnNetworkProjection`
- proposed `AmbientWindWorkBuffer`, `AmbientWindScanQuery`, `AmbientWindSystem`, `IAmbientWindRandomPort`, Tile-read port and Gore/presentation adapter
- proposed `BirthdayPartyStateComponent`, `BirthdayPartyRosterBuffer`, `BirthdayPartyPolicyCatalog`, `BirthdayPartyActiveQuery`, `BirthdayPartySystem`, `BirthdayPartyProjection` and celebration ports described in checkpoint 13
- proposed `LanternNightStateComponent`, `LanternNightPolicyCatalog`, `LanternNightActiveQuery`, `LanternNightSystem` and `LanternNightProjection` described in checkpoint 13
- proposed `MysticFairyEventStateComponent`, `MysticFairySpawnPolicy`, `MysticFairyLogScanWorkBuffer`, `MysticFairyEligibilityQuery`, `MysticFairySpawnSystem`, `ICelebrationTileReadPort` and `ICelebrationNpcSpawnPort` described in checkpoint 13
- proposed `CultistRitualStateComponent`, `CultistRitualPolicy`, `CultistRitualEligibilityQuery`, `CultistRitualSystem`, `ICultistRitualTileReadPort` and `ICultistRitualNpcSpawnPort` described in checkpoint 14
- proposed `SandstormStateComponent`, `SandstormPolicy`, `SandstormSystem`, `SandstormActiveQuery`, `SandstormNetworkProjection` and `SandstormPersistenceAdapter` described in checkpoint 14
- proposed `SceneWeatherEventZoneSnapshot`, `SceneWeatherEventZoneScanPolicy`, `SceneWeatherEventZoneScanSystem`, `SceneWeatherEventZoneQuery`, `ISceneMetricsTileReadPort`, `ISceneMetricsPlayerEffectPort` and `SceneWeatherEventZoneProjection` described in checkpoint 15
- proposed `Dd2InvasionDamageTrackingSession`, `Dd2InvasionDamageTrackingSystem`, `Dd2InvasionDamageTrackingNameQuery`, `Dd2InvasionDamageOutcomeProjection` and `IDd2InvasionDamageTrackerPort`
- proposed `Dd2InvasionDefinition`, `Dd2InvasionPresentationPolicy`, `Dd2InvasionProgressionStateComponent`, `Dd2InvasionRunStateComponent`, `Dd2InvasionWaveStateComponent`, `Dd2InvasionWavePolicy`, `Dd2InvasionArenaStateComponent`, `Dd2InvasionCrystalDropStateComponent`, `Dd2InvasionDeathPositionBuffer`, `Dd2InvasionProgressionSystem`, `Dd2InvasionWaveSystem`, `Dd2InvasionArenaSystem`, `Dd2InvasionCrystalDropSystem`, `Dd2InvasionWaveStatusQuery`, `Dd2InvasionArenaQuery`, `Dd2InvasionBuildingBlockQuery`, `Dd2InvasionBartenderReadinessQuery`, `Dd2InvasionNetworkProjection` and `Dd2InvasionPersistenceAdapter`

sharedTypesForIntegrationReview:
- world clock/rules and event random stream
- `NpcEntityId`, NPC eligibility view and spawn result
- world event instance/snapshot
- network and persistent IDs

crossSubsystemReaders:
- NPC and spawn eligibility
- Player, WorldGen and environment weather queries
- calendar UI, world-info network consumers and client visual effects
- Player gameplay readiness
- weather/calendar orchestration
- Network/session and client presentation
- NPC/Player/Projectile wind consumers and Cloud/Star client object lifecycle
- invasion NPC spawn/progress, event warning and packet 78 consumers
- NPC/Item/WorldGen seasonal readers, secret-seed initialization, world-file and world-info seasonal adapters, and client title effect
- NPC Moon Lord animation paths and client sky/scene presentation for credits and death drama
- client scene obstruction consumers and `DangerousDungeonCurse`/player environment inputs
- NPC death/banner mapping, SceneMetrics banner arrays, banner UI/claim consumers and joining-player network session
- NPC damage/kill/update/reset paths, boss definition registration and localization/message consumers
- ordinary/seasonal invasion start paths, NPC invasion-group lookup, invasion progress/UI readers and localized tracking output
- storm lightning seed/target callers, Projectile presentation and Tile collision/query boundary
- Tile/liquid/settled-world inputs, waterfall slot generation and client waterfall texture/presentation boundary
- Projectile/world-position scan inputs, Player wall-scan result boundary, Tile reads and client lighting/Dust/presentation consumers
- tree-top area/variation readers, world-generation Tile region mapping, world-file loading and packet 7 projection/receive consumers
- background-flash presentation consumers and any client resource lifecycle that is confirmed during integration
- visible-player/weather/event eligibility readers, NPC/Projectile ambient consumers, NetAmbience session projection, and AmbientWind Tile/Gore effect consumers
- NPC party eligibility and roster consumers, packet 7/world-info consumers, Lantern Night event consumers, Mystic Log Tile scans and fairy/NPC spawn consumers
- ritual countdown/eligibility readers, Tile/LOS collision inputs, NPC type `437` spawn consumers, WorldFile delay adapter and world-reset integration
- weather/wind queries, Sandstorm visual consumers, packet 7/world-info consumers, WorldFile persistence and the empty packet 7 receive boundary
- SceneMetrics camera/player scan callers, Player zone accessors, NPC spawn readers, Rain/Sandstorm visual readers, SceneState visual consumers, Tile/liquid/candle inputs and Torch God player-effect inputs

crossSubsystemWriters:
- world event/calendar command path
- NPC kill/progress path
- external network command adapter
- rain/coin-rain gameplay command path and world reset lifecycle
- weather randomization, wind timer and client presentation lifecycle
- invasion start/complete command path, NPC kill progress path and warning/progress projection path
- seasonal override command path, secret-seed policy path, day-start reducer, world reset path and title refresh effect path
- NPC credits trigger, calendar/world-clear credits reset, packet 140 projection/receive path and client Sky adapter
- Moon Lord NPC light-request callers, client drama object creation/update/cleanup/draw lifecycle, and ScreenObstruction smoothing path
- NPC death path, Banner threshold/notification path, world reset, world-file persistence and Banner outbound network projection
- NPC damage/kill/update/reset lifecycle, Boss definition registration and localization/message projection
- invasion start/stop facts, NPC invasion-group lookup, tracker active check and invasion tracking projection
- lightning generation command/port, explicit random source, Tile collision input and Projectile/presentation adapter
- waterfall generation/configuration path, scene/world reset path and client texture/presentation adapter; direct Version4 writer remains unconfirmed
- Spelunker scan requests and frame reset, explicit UnbreakableWall Tile reader and Player result command, and Void Lens lighting/random/Dust/presentation ports
- tree-top variation commands/world-generation coordinate queries, save/load adapters and packet 7 projection/receive path; background variation refresh and flash presentation tick
- AmbienceServer schedule/forced-request paths, meteor-triggered request submission, NetAmbience outbound projection, AmbientWind local update/graveyard gate, Tile/random/Gore adapters and world-reset buffer clearing
- BirthdayParty and LanternNight manual commands, day/night transitions, NPC event-cleared input, packet 7 projection/empty receive boundary and world-clear reset; MysticLogFairiesEvent Tile scan, `NPC.Spawner.fairyLog` integration and NPC spawn/presentation ports
- ritual tablet/NPC event paths, day-rate clock, Tile/LOS readers and NPC spawn command path; `CultistRitual.delay` persistence writer and unresolved recheck/reset boundary
- Sandstorm wind/random/start-stop path, world clear, packet 7/world-info projection, WorldFile save/load and unresolved duration assignment
- Main camera and Player `SceneMetrics.Scan` callers, proposed scene scan/reset system, Tile/liquid/player-effect ports and read-only Player/NPC/Rain/SceneState/UI projections; no Version4 direct zone writer is closed
- DD2 NPC damage/kill/update/reset callers, damage-tracker session consumers, DD2 progression/NPC spawn readers, arena building checks, wave timing, bartender readiness, crystal-drop NPC paths and WorldFile progress adapters

orderingConstraints:
- Commit calendar transition before day/night-gated event rolls.
- Commit weather facts before weather-dependent spawn/WorldGen queries and network projection.
- Commit coin-rain consumption through one writer; clear it with rain stop/world reset.
- Commit wind before wind-dependent spawn/physics queries; commit cloud counts before packet projection; presentation objects after committed state.
- Resolve calendar/rules and explicit start/stop commands before slime-rain state transition.
- Commit slime-rain state before NPC eligibility query and before network/UI projection.
- Commit kill progress through one writer before evaluating stop/threshold effects.
- Clear temporary eligibility cache before or atomically with world reset.
- Commit invasion lifecycle state before `InvasionSpawnQuery`; commit validated NPC progress before completion cleanup and packet/UI projections.
- Commit seasonal override policy before active-date reduction; consume daily flags once at day-start before seasonal readers and projections; consume title refresh only through the client effect adapter.
- Commit credits start/reset before packet projection and Sky consumption; per-tick countdown before presentation projection; packet receive must commit through one credits writer if later closed.
- Collect Moon Lord requests before drama update; check light-source proximity before buffer clear; update drama before client draw; evaluate object lifetime before pool removal.
- Resolve scene metrics/player obstruction input before target query; calculate target before smoothing commit; project screen obstruction only after the local value is committed.
- Initialize Banner catalog and load committed counts before NPC death submissions; commit Banner kill/claim/notification values before save or packet projection.
- Register Boss definitions before creating a damage tracker; route damage and kill outcome before active/recent cleanup; project localized name/message only after the outcome is committed.
- Resolve invasion lifecycle facts and NPC group lookup before invasion damage filtering; keep Version4 stub compatibility separate from any complete-reference behavior; reset tracker sessions with world reset.
- Resolve lightning policy and explicit seed/random input before generation; generate the complete Bolt payload before Projectile projection; keep Tile collision as a read-only input/result boundary and discard payloads after their presentation lifetime.
- Resolve waterfall policy and Tile/liquid inputs before slot generation; commit the complete temporary slot buffer before presentation; draw only from read-only payloads and clear/reuse the buffer before the next scene pass.
- Rebuild Spelunker bounds and apply the ten-frame reset before scan requests; run wall queries against read-only Tile input before Player result commit; consume Void Lens payloads through effect ports after Projectile/world-position input and before payload disposal.
- Resolve the immutable tree-top catalog before variation queries; commit all 13 variation values before save/network projection; decay background flash only after an explicit presentation refresh and never write the tree-top authority from the flash cache.
- Commit weather/calendar/event and visible-player facts before ambient eligibility; consume the ambient schedule and forced-request buffer through one request owner before NetAmbience projection; run the local AmbientWind gate and read-only Tile/random query before presentation, then clear transient wind work at its cadence and reset boundaries.
- Commit BirthdayParty/LanternNight manual and natural facts before active queries and packet 7 projection; consume next-night requests and cooldowns in the single night reducer; clear transition caches and fairy scan worksets at their documented morning/world-reset boundaries. Run Mystic Fairy qualification only after read-only Tile scan and visible-player/event facts, and submit NPC creation through the spawn port after the proposed event state commit.
- Run the ritual countdown/recheck reducer after world clock/rules, then the read-only Tile/LOS eligibility query and NPC type `437` spawn command; run the Sandstorm active/time/severity reducer after committed weather/wind input and before packet 7/world-info projection. Keep Sandstorm world clear explicit and leave CultistRitual reset unresolved until integration review.
- Run world weather/sandstorm/player-camera inputs through the SceneMetrics reset/cache check, then Tile/liquid and player-effect scan ports, commit the scene snapshot, expose the read-only query, and only then project to Player/NPC/Rain/SceneState/UI. Preserve same-frame/same-center cache hits and separate camera/player metrics.

boundaryChallenges:
- Close visual time and sun/moon offset writer/receiver ownership.
- Decide whether `oldMaxRaining` remains a projection cache and whether coin-rain has an independent event owner.
- Close wind random-stream ownership, Cloud slot lifecycle and wind-physics policy owner.
- Decide whether negative `slimeRainTime` is persisted event cooldown or deterministic derived retry state.
- Assign the sole writer for `slimeRainNPC` and its content-registration lifecycle.
- Define whether `maxRain` belongs to weather capacity or a separate render/particle owner.
- Define the final owner of invasion remaining-size writes, progress sentinel representation, NPCDamageTracker lifecycle and packet 7/78 receive/persistence adapters.
- Define the final owner of seasonal active facts, PendingWorldEventStateComponent daily-field overlap, secret-seed policy, world-file/world-info DTOs and title refresh effect.
- Define the final owner of packet 140 subtype 0 receive, CreditsRollSky activation, Moonlord client object creation/draw/reset and asset release, SceneState whitening consumption, and ScreenObstruction render/reset boundaries.
- Define Banner claim consumption, notification clear, packet full-state receive and stale/duplicate policy; distinguish BannerId from NPC type and item ID.
- Define NPCDamageTracker credit/session ownership, Boss recent-retention integration and the final localization adapter boundary.
- Decide whether Version4 InvasionDamageTracker stubs are compatibility requirements or incomplete evidence that permits the complete-reference group filter/active-stop behavior.
- Define Lightning seed/random ownership, coordinate value objects, Tile collision semantics, Bolt payload lifetime, and whether the current Version4 generation stubs are an evidence gap requiring a reference implementation before migration.
- Define Waterfall tile coordinate/type IDs, liquid or wetness input, slot-buffer clear/reuse owner, effective capacity configuration, texture resource lifecycle, and the missing Version4 writer/draw call graph.
- Define Spelunker result ownership and Tile reader, scan-buffer field writers and reset timing, UnbreakableWall Player/network result boundary, and Void Lens coordinate/random/lighting/Dust/presentation ownership.
- Define tree-top area/variation authority, packet 7 receive and stale/duplicate policy, save version 211 adapter semantics, old-version fallback, region-specific random input, and BackgroundChangeFlash trigger/consumer/resource lifetime.
- Define AmbientSpawn `targetPlayer=-1` handling, Player-slot versus `SkyEntityType`/`NetworkId`/`PersistentEntityId`, forced-request at-most-once or retry policy, AmbienceServer random/eligibility ownership, NetAmbience empty receive behavior, AmbientWind stub compatibility, Tile/random/Gore ports and work-buffer reset ownership.
- Define BirthdayParty/LanternNight manual-versus-genuine authority, cooldown and next-night persistence, `CelebratingNPCs` NPC-slot to stable `NpcEntityId` resolution, packet 7/111 permission and duplicate semantics, and Mystic Fairy `NPC.Spawner.fairyLog`, Tile scan and NPC spawn transaction ownership; all remain `crossSubsystemOwner: integration-review`.
- Define CultistRitual `delay` versus transient `recheck` recovery/reset, `delayStart`/`timePerCultist` usage, Tile/LOS snapshot and NPC type `437` spawn transaction ownership; all remain `crossSubsystemOwner: integration-review`.
- Define Sandstorm duration assignment, wind/random owner, empty Start/Stop compatibility, four-field authority, packet 7/world-info receive semantics and world-reset scope; all remain `crossSubsystemOwner: integration-review`.
- Define the sole writer and freshness contract for the nine SceneMetrics fields, separate camera/player snapshot identity, TileCenter/world-coordinate value objects, frame invalidation, and the boundary between Version4 empty scan methods and supplementary complete-reference formulas; all remain `crossSubsystemOwner: integration-review`.

evidenceGaps:
- Direct writer and decode/recovery path for `timeForVisualEffects`, `sunModY`, and `moonModY`.
- Persistence and versioning of rain, seasonal moon, eclipse and coin-rain fields.
- Cloud/Star presentation lifecycle, weather timers and wind-physics network/save semantics.
- Full NPC spawn and content-registration lifecycle for `slimeRainNPC` is not yet closed.
- Persistence and network encoding of each slime-rain member is not yet confirmed.
- MainInvasionState packet 7/78 decode, save/load recovery, `invasionProgressAlpha` writer, NPC progress commit and NPCDamageTracker lifecycle are not yet closed.
- The `invasionProgress = -1` moon-event sentinel conflicts with current NLTX non-negative invasion progress validation and needs an integration decision.
- MainSeasonalAndTitleState `changeTheTitle` direct writer, title platform adapter, packet-7 receive, date/time-zone port, full version compatibility and PendingWorldEventStateComponent overlap are not yet closed.
- SharedWorldEventPresentationState packet 140 subtype-0 receive, CreditsRollSky activation, Moonlord object creation/add/draw/update callers, asset lifetime, reset paths and ScreenObstruction caller/consumer are not yet closed; no network/save DTO is claimed for the client-only values.
- SharedInvasionAndBossTracking Banner packet receive, claim/notification consumers, Boss credit/session owner, Invasion group-filter lifecycle and Version4 stub-versus-complete-reference behavior remain open; no Boss/Invasion save/network DTO is claimed.
- SharedLightningGenerationState recursive geometry, rotation smoothing, Tile collision writer, random seed source, payload cleanup and Projectile/client consumption remain open; no lightning save/network DTO is claimed.
- SharedWaterfallState slot writer, clear/reuse/update/draw caller, Preferences key, waterfall type catalog, `stopAtStep` semantics, texture load/release lifecycle and world-reset boundary remain open; no waterfall save/network DTO is claimed.
- WorldEnvironmentScanHelpers Spelunker `CheckSpot` result writer and Tile reader, buffer reset/clear ownership, UnbreakableWall Tile adapter and Player/network result semantics, and Void Lens draw/Dust `customData` lifetime remain open; no scan cache, policy or Void Lens save/network DTO is claimed.
- SharedAmbientSkyCatalogState `BackgroundChangeFlashInfo.UpdateVariation`, flash-power trigger/consumer, `TreeTopsInfo` old-version `CopyExistingWorldInfo`, packet 7 receive and region-random-input boundaries remain open; no background-flash save/network DTO is claimed.
- SharedAmbientSpawnAndWindState `AmbienceServer` forced/ordinary request eligibility, target-player policy, NetAmbience receive, AmbientWind Tile/random/Gore effects, Version4 empty-method compatibility and transient reset boundaries remain open; no ambient spawn/wind save DTO is claimed.
- SharedCelebrationAndLanternEvents Version4 `BirthdayParty`/`LanternNight` packet receive and command permission semantics, roster slot lifetime, natural-attempt conditions, `MysticLogFairiesEvent` qualification/spawn/coordinate stubs, `NPC.Spawner.fairyLog` shared writer, Tile scan workset and transient reset boundaries remain open; no celebration/fairy save DTO is claimed.
- SharedRitualAndStormEvents `recheck` persistence/reset, delayStart/timePerCultist call graph, Tile/LOS/type `437` spawn transaction, Sandstorm duration/start-stop stubs, wind/random owner, packet 7 receive and world-reset boundaries remain open; no behavior-equivalence claim is made.
- SharedSceneWeatherAndEventZoneState direct writers for Tile aggregation, zone formulas, NPC-position scan and player effects are missing in Version4; no independent save/network DTO or complete camera/player projection owner is claimed.
- DD2 damage tracking has no independent save/network DTO evidence; successful tracker stop, inherited tracker ownership and UI/message projection remain open, and current NLTX has no evidence of completed Version4 behavior equivalence.
- DD2 wave/arena has persistence evidence only for the three downed-tier fields; run flags, lane policy, arena/cooldown, death-position buffer, crystal-drop counters, pause timer and hold behavior require focused verification, and Version4 empty methods cannot be silently replaced.

blockingDecisions:
- `crossSubsystemOwner: integration-review` for calendar/weather snapshots, network/persistence DTOs,
  wind/cloud/sky projections, coin-rain owner, NPC eligibility/slot cache, invasion authority/progress
  commit root, NPCDamageTracker and Boss/Invasion tracker session ownership, BannerId/NPC type/item ID
  boundaries, Banner packet receive/claim semantics, seasonal active/policy owner, event snapshots,
  packet adapters, persistence and title effect; credits packet subtype 0 receive, Moonlord client
  object/asset lifecycle, SceneState whitening consumer, ScreenObstruction render/reset boundary and
  final presentation schedule; Lightning seed/random, coordinate values, Tile collision, Bolt payload
   lifetime and Projectile presentation ownership; Waterfall tile/liquid inputs, slot-buffer lifetime,
   capacity configuration, texture resource port and the missing writer/draw call graph; Spelunker
   result/Tile-reader ownership, scan-buffer reset boundary, UnbreakableWall Player/network result and
   wall directory boundary, and Void Lens coordinate values and random/lighting/Dust/presentation ports.
   Tree-top area/variation authority, packet 7 receive and stale/duplicate policy, save version 211
    adapter semantics, old-version fallback, region-specific random input, and BackgroundChangeFlash
    trigger/consumer/flashPower ownership remain `crossSubsystemOwner: integration-review`.
    The Version4 InvasionDamageTracker stub policy and LightningGenerator algorithm-stub policy must be
    explicit before behavior-changing implementation.
  - AmbientSpawn `targetPlayer=-1`, Player slot and SkyEntityType/NetworkId/PersistentEntityId
    distinctions, forced-request consumption, random/eligibility ownership, NetAmbience empty receive,
    AmbientWind Version4 stub compatibility, Tile/random/Gore ports and transient work-buffer reset remain
    `crossSubsystemOwner: integration-review`.
 - BirthdayParty/LanternNight manual/genuine/next-night/cooldown ownership, `CelebratingNPCs` slot-to-entity
   resolution, packet 7/111 receive and duplicate policy, Mystic Fairy qualification and coordinate behavior,
   `NPC.Spawner.fairyLog` shared ownership, Tile scan workset and NPC spawn command boundary remain
   `crossSubsystemOwner: integration-review`; complete-reference behavior cannot silently replace Version4
   empty methods.
- CultistRitual `delay`/`recheck` authority and persistence, `delayStart`/`timePerCultist` usage,
  Tile/LOS/type `437` spawn transaction and reset ordering remain `crossSubsystemOwner:
  integration-review`.
- Sandstorm four-field authority, duration assignment, wind/random ports, empty Start/Stop compatibility,
  packet 7/world-info receive and world-reset scope remain `crossSubsystemOwner: integration-review`;
  complete-reference behavior cannot silently replace Version4 empty methods.
- `SceneWeatherEventZoneSnapshot` freshness/version, TileCenter and world-coordinate value objects,
  camera/player identity, SceneMetrics reset/cache ownership, the nine-field direct writer,
  Tile/player-effect ports and read-only projections remain `crossSubsystemOwner: integration-review`;
  complete-reference formulas cannot silently replace Version4 empty scan methods.
- DD2 damage-session identity, inherited `NPCDamageTracker` fields, successful `WinInvasionInternal` cleanup,
  `IncludeDamageFor` stub compatibility, wave/run authority, arena rectangle and cooldown,
  death-position consumption, crystal-drop transaction, persistent tier DTOs and event/packet identity
  remain `crossSubsystemOwner: integration-review`.

notImplemented:
- No production src C# migration, test, project, registration, network, persistence or scheduler change was made; Component-only C# implementation exists under src2/WorldSession.

## 9. 状态声明

本文件中的系统、查询、命令、适配器、投影、接口、路径和顺序仍为 `proposed`，不是当前 NLTX
已存在能力。上方 implementationCheckpoint 仅表示 Component-only C# 文件已写入 `src2`；不表示
编译、测试、行为等价、API 兼容、注册、网络、持久化、调度或迁移闭合。
