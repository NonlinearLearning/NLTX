# WorldCalendarAndEventOrchestration Component-only Design

> designStatus: decision-required  
> subsystemId: WorldCalendarAndEventOrchestration  
> taskNumber: 05  
> evidenceStatus: partial  
> nltxStatus: partial  
> verificationStatus: not-run  
> sourceResearchReport: `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-05-version4-world-calendar-and-event-orchestration-public-decomposition.md`
> outputPath: `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-world-calendar-and-event-orchestration-component-design.md`  
> scope: Component-only; all names in this document are design candidates, not implemented types

## 1. 设计元数据

本文件依据 Version4 真实源码、完整参考源码中与 Version4 同路径同签名的补证、tModLoader v2026.07 公共生命周期文档、有限的 Space Station 14 ECS 结构参考，以及当前 NLTX 局部源码形成。

本设计只裁决候选 Component 的责任、字段、默认值、不变量、生命周期、实体范围、组合关系和 evidence-gap。所有候选 Component 都明确标记为 `status: proposed`。`designStatus: decision-required` 表示时间默认值、跨子系统 owner、事件实例身份、随机状态和部分 Version4 事件实现尚未闭合。

字段状态分类使用以下含义：

| 状态分类 | 含义 |
| --- | --- |
| `authoritative` | 该 Component 中应保留的权威事实；当前 Version4 成员存在直接证据，但最终写入 owner 仍可能跨域待裁决 |
| `derived` | 由同一 Component 的权威字段计算得到，不应成为第二份独立权威事实 |
| `cache` | 为生命周期边界或兼容观察保留的暂存事实；必须能从证据中说明其失效边界 |
| `snapshot` | 用于保存、恢复、复制或兼容读取的字段形态；不因此取得规则写入权 |
| `compatibility` | 为保留 Version4 字段语义、旧格式或当前 NLTX 局部 API 而暂存的字段 |

`default` 写的是设计候选的初始值。若写为 `unresolved`，表示不能从现有证据安全推断默认值，不允许在实现阶段静默选择一个值。

## 2. 范围与排除

### 2.1 本文范围

本文覆盖以下 Component 候选：

- 世界时钟事实和昼夜边界所需的日历事实；
- 通用事件实例身份；
- Birthday Party、Lantern Night、Sandstorm、Slime Rain 状态；
- 普通入侵的运行时状态和完成历史；
- 待处理世界事件意图；
- 日历模式及时间控制覆盖状态；
- DD2 的持久进度、单次运行状态和波次运行状态；
- 事件随机状态及其与世界种子的关系。

### 2.2 明确排除

本文不定义、不实现也不裁决以下运行时结构：

- System、Query、Command、Adapter、Projection、Coordinator、Pipeline、Scheduler 或 Verifier；
- 调度顺序、主循环、网络流程、存档流程、测试计划或迁移计划；
- NPC 生成算法、战斗伤害算法、战利品算法、地图生成算法和 UI 表现；
- `.cs`、`.csproj` 或其他可运行实现文件；
- 任何跨子系统共享 ID、快照格式或提交端口的最终 owner。

Version4 的网络和存档字段只在本文中用于识别 Component 的 `snapshot` 或 `compatibility` 边界，不会被改写为网络或存档运行时设计。WorldSession、NPC、WorldFile、网络消息和空间区域之间的关系只记录为待整合的关系或 evidence-gap。

## 3. 设计依据

### 3.1 Version4 依据

| 依据 | 直接事实 | 在 Component 设计中的用途 | 状态 |
| --- | --- | --- | --- |
| `D:\TRbackup\Version4\Terraria\Main.cs:402`、`:594-606` | `dayRate`、`dayTime`、`time`、`moonPhase` 及日历/天气标志的初始成员 | 识别 `CalendarClock` 和 `WorldCalendarOverrideState` 字段 | confirmed / partial |
| `D:\TRbackup\Version4\Terraria\Main.cs:548-550` | `slimeRainTime`、`slimeRain`、`slimeRainKillCount` | 识别 `SlimeRainState` 的活动、计时和计数事实 | confirmed |
| `D:\TRbackup\Version4\Terraria\Main.cs:1059-1077` | 普通入侵类型、位置、规模、延迟、警告和进度字段 | 识别 `WorldInvasionState` | confirmed |
| `D:\TRbackup\Version4\Terraria\Main.cs:12542-12684` | 普通入侵结束、位置移动和启动时对字段的写入 | 识别入侵不变量和 lifecycle 边界 | confirmed / partial |
| `D:\TRbackup\Version4\Terraria\Main.cs:12972-13565` | 时间推进、昼夜边界、Slime Rain 和日历事件相关事实变化 | 识别 `CalendarClock` 与事件状态之间的事实边界 | confirmed / partial |
| `D:\TRbackup\Version4\Terraria.GameContent.Events\BirthdayParty.cs:13-21` | Party 的手动、自然、冷却、NPC 列表和前一状态字段 | 识别 `BirthdayPartyState` | confirmed |
| `D:\TRbackup\Version4\Terraria.GameContent.Events\LanternNight.cs:8-16` | Lantern Night 的手动、自然、下一晚、冷却和前一状态字段 | 识别 `LanternNightState` | confirmed |
| `D:\TRbackup\Version4\Terraria.GameContent.Events\Sandstorm.cs:14-20` | Sandstorm 的活动、剩余时间、当前强度和目标强度 | 识别 `SandstormState` | confirmed |
| `D:\TRbackup\Version4\Terraria.GameContent.Events\DD2Event.cs:45-75` | DD2 持久标志、单次运行标志、竞技场、难度、死点和波次等待字段 | 识别三个 DD2 Component | confirmed / partial |
| `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:98-178`、`:1073-1097` | 存档临时字段与运行时字段之间的来回复制 | 将字段标记为 `snapshot` 或 `compatibility`，不建立 Temp Component | confirmed |
| `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1343-1422`、`:2168-2383` | 时间、入侵、Slime Rain、Party、Sandstorm、DD2、Lantern Night 的写入和版本读取 | 识别持久性证据边界和版本缺口 | confirmed / partial |
| `D:\TRbackup\Version4\Terraria\NetMessage.cs:221-403` | WorldData 中发送的时间、事件标志、入侵类型和 Sandstorm 目标强度 | 识别 `snapshot` 字段，不证明规则 owner | confirmed |

Version4 `MessageBuffer` 的 `case 7` 在当前覆盖中为空（`D:\TRbackup\Version4\Terraria\MessageBuffer.cs:385-386`）。因此事件网络恢复和入站字段恢复不能被本文假定为已闭合行为。

### 3.2 当前 NLTX 依据

| 依据 | 观察 | 设计影响 |
| --- | --- | --- |
| `D:\TRbackup\NLTX\src\WorldSession\WorldSessionComponents.cs:59-85` | 已有 `BirthdayPartyState`、`LanternNightState`、`SandstormState`、`WorldTimeWeatherState`、`InvasionRuntimeState`、`Dd2ProgressState` 等局部状态 | 只能作为映射证据，不能视为目标 Component 已完成 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\WorldClock.cs` | 当前 Dome 时钟使用 `dayLengthTicks = 54000`、`nightLengthTicks = 32400`、`timeOfDay = 0`、`isDayTime = true` | 与 Version4 `Main.time = 13500.0` 存在默认值冲突 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\WorldProgressionState.cs` | 当前局部模型把多个日历、入侵、Slime Rain 和 Lantern Night 字段放在一个不可变状态中 | 支持拆分边界，但不证明目标 owner 或完整行为等价 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\WorldEventRandomState.cs` | Dome 有可持久化的 `uint Value` 随机状态 | 与 Version4 `Main.rand` 的来源和消费关系未闭合 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Server\Persistence\DomeStatePersistenceFormat.cs:21` | 当前 `CurrentFormatVersion = 37` | 记录 `version-drift`；不采用旧摘要中的其他版本值 |

### 3.3 外部结构参考的边界

tModLoader v2026.07 的 `ModSystem` 公共页面只用于确认公开生命周期和 WorldData 读写边界，不能证明 Version4 私有算法。Space Station 14 的 `GameRuleComponent`、活动/结束状态 Component 和随机依赖显式传递只用于 Component 粒度、生命周期状态互斥和随机依赖的结构参考。Space Station 14 无法确认 Terraria 私有行为；仅用于 Component 粒度、生命周期状态和 ID 分离参考。

## 4. Version4 成员到 Component 归属表

下表是字段归属候选，不是迁移清单，也不表示当前 NLTX 已有同等 Component。一个 Version4 成员如果同时出现在保存或网络数据中，仍只归属于一个逻辑 Component；保存/网络形态仅作为该 Component 的 `snapshot` 或 `compatibility` 表现。

| Version4 成员或成员组 | 候选 Component | 归属理由 | 证据状态 |
| --- | --- | --- | --- |
| `Main.dayRate`、`Main.dayTime`、`Main.time`、`Main.moonPhase` | `CalendarClock` | 共同描述世界日历位置和昼夜事实 | partial；`time` 默认值冲突 |
| `Main.bloodMoon`、`Main.eclipse`、`Main.pumpkinMoon`、`Main.snowMoon`、快进和日晷/夜晷字段 | `WorldCalendarOverrideState` | 属于日历模式或暂时性时间控制，不与基础时间位置混存 | partial |
| `BirthdayParty.ManualParty`、`GenuineParty`、`PartyDaysOnCooldown`、`CelebratingNPCs`、`_wasCelebrating` | `BirthdayPartyState` | Party 有独立资格、活动和结束观察状态 | confirmed / partial |
| `LanternNight.ManualLanterns`、`GenuineLanterns`、`NextNightIsLanternNight`、`LanternNightsOnCooldown`、`_wasLanternNight` | `LanternNightState` | Lantern Night 有独立排期、活动和结束观察状态 | confirmed / partial |
| `Sandstorm.Happening`、`TimeLeft`、`Severity`、`IntendedSeverity` | `SandstormState` | 活动持续时间和强度内聚，且与普通降雨不同 | confirmed |
| `Main.slimeRain`、`slimeRainTime`、`slimeRainKillCount`、`slimeWarningTime` | `SlimeRainState` | Slime Rain 有独立活动标志、带正负语义的计时和警告事实 | confirmed / partial |
| `Main.invasionType`、`invasionX`、`invasionSize`、`invasionSizeStart`、`invasionDelay`、`invasionWarn`、进度显示字段 | `WorldInvasionState` | 普通入侵是一个独立的世界级运行时实例状态 | confirmed |
| `NPC.downedGoblins`、`downedFrost`、`downedPirates`、`downedMartians` 及当前 NLTX 的对应历史标志 | `InvasionHistoryState` | 完成历史与当前入侵运行时分离，生命周期和持久性不同 | partial |
| `WorldGen.spawnEye`、`spawnHardBoss`、`spawnMeteor`、`meteorShowerCount`、临时节日强制标志 | `PendingWorldEventState` | 这些是尚未成为活动实例的待处理世界事件事实 | confirmed / partial |
| `Main.fastForwardTimeToDawn`、`fastForwardTimeToDusk`、`sundialCooldown`、`moondialCooldown` 及未单独建模的日历模式标志 | `WorldCalendarOverrideState` | 外部或临时日历控制与基础时钟位置分离 | partial |
| `DD2Event.DownedInvasionT1/T2/T3` | `Dd2PersistentProgressState` | 明确由 `DD2Event.Save/Load` 读写的世界持久进度 | confirmed |
| `DD2Event.Ongoing`、`LostThisRun`、`WonThisRun`、`OngoingDifficulty`、`LaneSpawnRate`、`ArenaHitbox` | `Dd2RunState` | 单次 DD2 运行的活动和竞技场事实与永久完成标志分离 | confirmed / partial |
| `DD2Event._timeLeftUntilSpawningBegins`、`_deadGoblinSpots`、水晶掉落计数、`NPC.waveNumber`、`waveKills`、`totalInvasionPoints` | `Dd2WaveRuntimeState` | 波次内等待、位置和计分事实不应污染永久进度 | confirmed / partial |
| `Main.rand` 使用点、Dome `WorldEventRandomState.Value`、世界种子关系 | `WorldEventRandomState` | 随机状态和随机来源必须从事件事实中独立出来 | partial；来源冲突 |
| `WorldFile` 临时字段、WorldData 字段和 Dome 状态字段 | 对应逻辑 Component 的 `snapshot` / `compatibility` 字段 | 不建立一个笼统的临时或网络 Component | confirmed / partial |

## 5. Component 定义

以下 14 个 Component 全部是候选设计，统一写作 `status: proposed`。每个 Component 的 `lifecycle` 只描述数据有效期和状态重置边界，不规定任何运行时调度顺序。

### 5.1 CalendarClock

```text
componentId: WORLDCALENDARANDEVENTORCHESTRATION-CALENDAR-CLOCK
name: CalendarClock
status: proposed
componentOwner: WorldSession（候选）
crossSubsystemOwner: integration-review
entityScope: World singleton
lifecycle: world-load -> active clock fact -> world-clear; restore must preserve a valid clock snapshot
```

#### 职责

保存世界在日历中的当前位置、昼夜方向、推进速率和月相。它只保存时间事实，不保存 Blood Moon、Party、入侵或天气活动状态。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
| --- | --- | --- | --- | --- | --- | --- |
| `tickNumber` | `long` | `0` | authoritative / snapshot | 不得小于 0；与跨域 `WorldTime` 的单位和溢出策略必须一致 | partial | Dome `WorldClock.TickNumber`；Version4 无同名单调 tick |
| `timeOfDay` | `double` | `unresolved` | authoritative / compatibility | 必须是有限值；应位于当前昼或夜长度区间内；Version4 初值为 `13500.0`，Dome 构造默认值为 `0.0`，不可静默合并 | partial | `Version4 Main.cs:596`；Dome `WorldClock.cs` 构造函数 |
| `isDayTime` | `bool` | `true` | authoritative / snapshot | 与 `timeOfDay` 使用同一昼夜区间；昼夜转换后不得残留越界时间 | confirmed | `Version4 Main.cs:594`；`WorldClock.IsDayTime` |
| `dayRate` | `int` | `1` | authoritative / compatibility | 不得为负；Version4 的时间推进使用该值，零值行为必须在 owner 决策中闭合 | confirmed | `Version4 Main.cs:402`、`:13111` |
| `dayLengthTicks` | `int` | `54000` | derived / snapshot | 必须大于 0；对应 Version4 白天上限 | partial | Version4 昼夜边界；Dome `DefaultDayLengthTicks` |
| `nightLengthTicks` | `int` | `32400` | derived / snapshot | 必须大于 0；对应 Version4 夜晚上限 | partial | Version4 昼夜边界；Dome `DefaultNightLengthTicks` |
| `moonPhase` | `byte` | `0` | authoritative / snapshot | 只能为 0 到 7 | confirmed | `Version4 Main.cs:606`；`NetMessage.cs:229` |
| `isPaused` | `bool` | `false` | compatibility | 是否允许暂停尚未在 Version4 本责任面闭合；不能改变持久日历的单位定义 | partial | Dome `WorldClock.IsPaused`；Version4 证据缺失 |

#### 字段不变量

- `timeOfDay`、`isDayTime`、`dayLengthTicks` 和 `nightLengthTicks` 必须作为一个可验证快照解释，不能由两个不同来源分别决定。
- `dayRate` 不是事件状态；Party、Lantern Night、Sandstorm 和 Slime Rain 只能读取其事实，不把它复制为自己的速率字段。
- `moonPhase` 的枚举范围有直接证据，但月相更新时点与 `timeOfDay` 初值的整合仍是 evidence-gap。

#### 生命周期

World 初始化时建立一个有效值；恢复时要求全部时钟字段一起通过不变量检查；世界清理时回到初始快照。是否在暂停状态中保存真实时间、暂停时间或两者，仍由 integration-review 决定。

#### Entity/World 范围

这是 World singleton，不附着 NPC、Player 或 WorldSection。任何实体观察到的时间都应能追溯到同一 `WorldTime` 关系，而不是保留实体私有副本。

#### ID 与关系字段

`WorldTime`、`WorldTickSnapshot`、`WorldId` 的具体类型和 owner 为 `crossSubsystemOwner: integration-review`。当前 Version4 只有静态 `Main` 字段，没有可直接确认的 `EntityId` 或 `PersistentEntityId` 关系。

#### 当前 NLTX 映射

主要映射为 `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\WorldClock.cs` 和 `D:\TRbackup\NLTX\src\WorldSession\WorldSessionComponents.cs` 中的 `WorldTimeWeatherState`。Dome 的 `timeOfDay = 0` 与 Version4 的 `Main.time = 13500.0` 形成 `version-drift`，因此不标记为完成。

#### 证据

Version4 `Main.cs:402`、`:594-606`、`:12972`、`:13111`、`:13470-13565`；Dome `WorldClock.cs`。默认时间值和暂停语义仍为 partial。

### 5.2 WorldEventInstanceState

```text
componentId: WORLDCALENDARANDEVENTORCHESTRATION-EVENT-INSTANCE
name: WorldEventInstanceState
status: proposed
componentOwner: WorldCalendarAndEventOrchestration（候选）
crossSubsystemOwner: integration-review
entityScope: World event instance entity（实例实体边界未裁决）
lifecycle: uninitialized -> active -> ended or cancelled -> discarded after world-clear
```

#### 职责

把“事件种类”和“某一次事件实例”分开。该 Component 只保存实例身份、生命周期事实和与世界/实体的关系，不保存某一种事件的专属规则字段。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
| --- | --- | --- | --- | --- | --- | --- |
| `eventInstanceId` | `EventInstanceId`（具体类型 unresolved） | `unresolved` | authoritative / snapshot | 在同一 World 内必须唯一；不得用事件种类或数组位置代替 | evidence-gap | Version4 没有事件实例 ID；当前任务要求单独识别实例身份 |
| `eventKind` | `WorldEventKind` 或兼容枚举 | `None` | authoritative / compatibility | 必须能区分普通入侵、Party、Lantern Night、Sandstorm、Slime Rain、DD2 及日历模式 | partial | Dome `WorldEventKind.cs` 仅包含部分事件；Version4 事件分散在多个类型 |
| `lifecycleState` | `WorldEventLifecycleState`（候选枚举） | `Uninitialized` | authoritative | 结束状态不可重新变为活动状态而不产生新的实例身份 | evidence-gap | Version4 以多个布尔值和私有前一状态字段表达，未提供通用状态枚举 |
| `startedAtTick` | `long?` | `unset` | snapshot / compatibility | 活动实例必须有开始位置；旧数据缺失时必须显式表示缺失 | evidence-gap | Version4 未保存通用开始 tick |
| `endedAtTick` | `long?` | `unset` | snapshot / compatibility | 非结束实例不得伪造结束时间；结束时间不得早于开始时间 | evidence-gap | Version4 未保存通用结束 tick |
| `ownerEntityId` | `EntityId?` | `unset` | authoritative relation | 若实例由实体拥有，引用必须指向存在的实体；owner 类型不可用字符串替代 | evidence-gap | NPC、Player、世界对象 owner 尚未裁决 |
| `persistentEntityId` | `PersistentEntityId?` | `unset` | snapshot / compatibility | 只有真正跨重载稳定的实体才允许使用；不能把运行时 `EntityId` 强转 | evidence-gap | Version4 事件实例没有统一持久实体身份 |
| `networkId` | `NetworkId?` | `unset` | snapshot / compatibility | 仅代表复制身份，不取得规则写入权；未复制实例必须为空 | evidence-gap | `NetMessage` 有事件结果字段但没有通用实例 ID |

#### 字段不变量

`eventInstanceId` 必须独立于 `eventKind`、NPC 数组索引、网络 ID 和存档位置。多个事件状态如果不能被唯一关联，就必须保留 `unresolved`，不能用隐式组合键补齐。

#### 生命周期

实例从无效进入活动、结束或取消，清理时失效。Version4 的 `GenuineParty`、`GenuineLanterns`、`Happening`、`Ongoing` 和 `invasionType` 只能作为实例状态的局部证据，不能被宣称为已经实现了统一生命周期。

#### Entity/World 范围

目标是每个世界事件实例一个可识别的实体边界；是否真的创建独立 ECS Entity、还是作为 World 内的带 ID 记录，属于 integration-review，不在本文决定。

#### ID 与关系字段

`eventInstanceId`、`ownerEntityId`、`persistentEntityId`、`networkId` 都是 `crossSubsystemOwner: integration-review`。与 `NpcEntityId`、`PlayerEntityId`、`WorldSectionId` 的关联不得通过整数重用。

#### 当前 NLTX 映射

当前没有经过本轮验证的统一实例 Component。`WorldEventKind.cs`、`WorldClockTransition.cs`、`WorldProgressionState.cs` 和 `WorldSessionComponents.cs` 只能提供部分枚举、边界和状态证据。

#### 证据

Version4 Party、Lantern Night、Sandstorm、DD2 和 Main 入侵字段的分散状态；`NetMessage.cs:221-403` 的 WorldData。通用实例身份、开始/结束 tick 和多实例能力均为 evidence-gap。

### 5.3 BirthdayPartyState

```text
componentId: WORLDCALENDARANDEVENTORCHESTRATION-BIRTHDAY-PARTY
name: BirthdayPartyState
status: proposed
componentOwner: WorldCalendarAndEventOrchestration（候选）
crossSubsystemOwner: integration-review
entityScope: World singleton; NPC membership is a relation
lifecycle: world-clear -> inactive or manual pending -> active -> ended; cooldown survives only as long as its owner requires
```

#### 职责

保存 Party 的手动标志、自然标志、冷却和参与 NPC 身份，并保留结束边界所需的前一活动状态。它不拥有 NPC 的职业、位置、生成或表现字段。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
| --- | --- | --- | --- | --- | --- | --- |
| `manualParty` | `bool` | `false` | authoritative / compatibility | 手动状态只能表示手动来源，不得伪装为自然 Party | confirmed | `BirthdayParty.cs:13` |
| `genuineParty` | `bool` | `false` | authoritative | 自然 Party 结束时必须能清除；不得与清空后的参与列表产生无法解释的活动状态 | confirmed | `BirthdayParty.cs:15`、`:47-57` |
| `partyDaysOnCooldown` | `int` | `0` | authoritative / snapshot | 不得小于 0；冷却和活动的并存语义必须保持可观察 | confirmed | `BirthdayParty.cs:17`、`:75-78` |
| `celebratingNpcIds` | `IReadOnlyList<NpcEntityId>` | `empty` | authoritative relation / snapshot | 不得重复；每个引用必须能解释为参与 NPC；Version4 的 `whoAmI` 不能直接当作持久 ID | partial | `BirthdayParty.cs:19`、`:103-121` |
| `isUp` | `bool`（derived） | `false` | derived | `genuineParty || manualParty`；不得成为第二份可写事实 | confirmed | `BirthdayParty.PartyIsUp` |
| `wasCelebrating` | `bool` | `false` | cache / compatibility | 只用于识别活动边界；清理后必须复位 | partial | `BirthdayParty.cs:21`、`:169-187` |

#### 字段不变量

- `isUp` 只能由两个来源标志派生；手动 Party 与自然 Party 的来源不能合并丢失。
- `celebratingNpcIds` 是关系集合，不是 NPC 状态的拥有者；NPC 消失或身份不匹配时，清理行为必须可审计。
- 完整参考中的 `CanNPCParty` 和 `NaturalAttempt` 只能作为补证，Version4 当前缺失部分仍标记 partial。

#### 生命周期

世界清理复位全部字段；活动建立参与者集合；自然活动结束时清理自然标志和参与者；手动活动的结束语义必须保留 Version4 的独立来源。

#### Entity/World 范围

World singleton。参与者是指向 NPC 实体的关系，不把 Party Component 附着到每个 NPC 上，也不复制 NPC 的完整状态。

#### ID 与关系字段

`NpcEntityId`、可能的 `PersistentEntityId` 和 `EntityReference` 均为 `crossSubsystemOwner: integration-review`。Version4 `List<int>` 的整数含义是运行时 `whoAmI` 候选，不是已确认的跨重载身份。

#### 当前 NLTX 映射

`D:\TRbackup\NLTX\src\WorldSession\WorldSessionComponents.cs` 中的 `BirthdayPartyState` 已有同名字段和 `CelebratingNpcIds`，但字段类型、owner、持久关系和完整结束边界尚未完成验证。

#### 证据

`BirthdayParty.cs:13-21`、`:44-57`、`:75-137`、`:141-163`、`:169-187`；`WorldFile.cs:166-170`、`:1081-1085`、`:1405-1406`、`:2320` 附近。网络中发送的是 `PartyIsUp`（`NetMessage.cs:318`），不是完整参与者关系。

### 5.4 LanternNightState

```text
componentId: WORLDCALENDARANDEVENTORCHESTRATION-LANTERN-NIGHT
name: LanternNightState
status: proposed
componentOwner: WorldCalendarAndEventOrchestration（候选）
crossSubsystemOwner: integration-review
entityScope: World singleton
lifecycle: world-clear -> inactive -> next-night pending -> active -> morning-ended
```

#### 职责

保存 Lantern Night 的手动活动、自然活动、下一晚预定和冷却状态。它不拥有 Boss、入侵、流星或 NPC 的资格事实，只保留自身看到的状态。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
| --- | --- | --- | --- | --- | --- | --- |
| `manualLanterns` | `bool` | `false` | authoritative / compatibility | 手动活动来源必须与自然活动来源区分 | confirmed | `LanternNight.cs:8` |
| `genuineLanterns` | `bool` | `false` | authoritative | 自然活动结束时必须可清除 | confirmed | `LanternNight.cs:10`、`:35-44` |
| `nextNightIsLanternNight` | `bool` | `false` | authoritative / snapshot | 只能表示下一有效夜晚的预定，不等同当前 `isUp` | confirmed | `LanternNight.cs:12` |
| `lanternNightsOnCooldown` | `int` | `0` | authoritative / snapshot | 不得小于 0；冷却不得与字段语义混淆 | confirmed | `LanternNight.cs:14` |
| `isUp` | `bool`（derived） | `false` | derived | `genuineLanterns || manualLanterns` | confirmed | `LanternNight.LanternsUp` |
| `wasLanternNight` | `bool` | `false` | cache / compatibility | 只用于识别边界；清理后必须为 false | partial | `LanternNight.cs:16`、`:98-99` |

#### 字段不变量

`nextNightIsLanternNight` 不得在当前 Lantern Night 结束后被误读为仍处于活动；`isUp` 不得成为独立写入字段。`LanternsCanStart` 对 Boss、入侵、血月和其他事件的资格约束属于外部事实输入，本 Component 不复制这些事实。

#### 生命周期

世界清理复位；自然或手动来源可以分别进入活动；早晨边界清理当前活动；下一晚预定在消费前保持独立。完整 `NaturalAttempt` 和 `BossIsActive` 在 Version4 覆盖中存在缺口，不能由当前状态表推导完成。

#### Entity/World 范围

World singleton。若将来需要事件实例身份，通过 `WorldEventInstanceState` 关联，不在本 Component 内复制一套通用实例字段。

#### ID 与关系字段

没有 Version4 直接证明的实体 ID。与 `eventInstanceId`、`PlayerEntityId`、`NpcEntityId` 的关系标记为 `crossSubsystemOwner: integration-review`；资格读取不改变本 Component 的 owner。

#### 当前 NLTX 映射

当前 `WorldSessionComponents.cs` 中的 `LanternNightState` 提供了四个主要状态字段，但 Dome `WorldProgressionState` 另有 `LanternNightScheduleSequence`，两者的持久身份和 owner 仍存在 `version-drift`。

#### 证据

`LanternNight.cs:8-16`、`:32-44`、`:66-72`、`:82-99`；`WorldFile.cs:175-178`、`:1093-1096`、`:1419-1422`、`:2370-2383`；`NetMessage.cs:332`。

### 5.5 SandstormState

```text
componentId: WORLDCALENDARANDEVENTORCHESTRATION-SANDSTORM
name: SandstormState
status: proposed
componentOwner: WorldCalendarAndEventOrchestration（候选）
crossSubsystemOwner: integration-review
entityScope: World singleton; desert presentation reads this fact
lifecycle: world-clear -> inactive -> active -> stopped; severity may converge while active or inactive
```

#### 职责

保存沙尘暴活动标志、剩余时间、当前强度和目标强度。它不拥有风速、沙漠区域判定或背景表现，只保存事件状态所需事实。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
| --- | --- | --- | --- | --- | --- | --- |
| `happening` | `bool` | `false` | authoritative / snapshot | `false` 不得被解释为目标强度为零；活动结束必须清除或明确剩余时间 | confirmed | `Sandstorm.cs:14` |
| `timeLeft` | `int` | `0` | authoritative / snapshot | 正常活动时不得小于 0；最大持续时间不超过 `86400` 的 Version4 约束 | confirmed | `Sandstorm.cs:12`、`:16`、`:40-57` |
| `severity` | `float` | `0f` | authoritative / cache | 必须有限且在 `[0,1]`；NaN 必须按 Version4 语义归零 | confirmed | `Sandstorm.cs:18`、`:101-119` |
| `intendedSeverity` | `float` | `0f` | authoritative / snapshot | 必须有限且在 `[0,1]`；目标值与当前值分开 | confirmed | `Sandstorm.cs:20`、`:82-99`、`:109-114` |

#### 字段不变量

`severity` 与 `intendedSeverity` 是两个不同事实；不能把当前强度和目标强度合并为一个值。`Happening`、风速和区域判定之间存在 Version4 的外部条件，不在此 Component 复制。

#### 生命周期

世界清理时 `happening` 复位；活动期间 `timeLeft` 递减；停止时活动标志归零；强度可以在事件边界之外保留目标收敛语义。停止条件的完整调用图仍是 partial。

#### Entity/World 范围

World singleton。沙漠区域或玩家所在区域只是读取关系，不建立每个区域一个沙尘暴 Component。

#### ID 与关系字段

不包含实体 ID。与 `WorldSectionId`、`EntityReference` 和天气 owner 的关系为 `crossSubsystemOwner: integration-review`；`WorldSectionId` 是否需要进入状态模型尚无 Version4 直接证据。

#### 当前 NLTX 映射

`WorldSessionComponents.cs` 中的 `SandstormState` 与 Version4 四字段近似对应，但是否由 WorldSession 或日历责任面写入尚未裁决。

#### 证据

`Sandstorm.cs:12-20`、`:31-80`、`:82-119`；`WorldFile.cs:128-134`、`:171-174`、`:1086-1089`、`:1407-1410`、`:2323-2335`；`NetMessage.cs:324`、`:401`。

### 5.6 SlimeRainState

```text
componentId: WORLDCALENDARANDEVENTORCHESTRATION-SLIME-RAIN
name: SlimeRainState
status: proposed
componentOwner: WorldCalendarAndEventOrchestration（候选）
crossSubsystemOwner: integration-review
entityScope: World singleton
lifecycle: world-clear -> inactive or cooldown -> active -> stopped or cooldown
```

#### 职责

保存 Slime Rain 的活动、带符号计时、击杀计数和警告计时。它不拥有 Slime NPC 槽位、NPC 生成压力或天气显示。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
| --- | --- | --- | --- | --- | --- | --- |
| `active` | `bool` | `false` | authoritative / snapshot | 活动时应与正的活动计时一致；负计时阶段不得宣称活动 | confirmed | `Main.cs:548`、`:12927-12945`、`:12952-12965` |
| `timeState` | `double` | `0.0` | authoritative / compatibility | 正值表示活动计时，负值表示停止后的冷却计时，零表示无计时；必须有限 | confirmed | `Main.cs:546`、`:12937-12942`、`:12963-12964` |
| `killCount` | `int` | `0` | authoritative / snapshot | 不得小于 0；清理或重新开始时按 Version4 语义复位 | confirmed | `Main.cs:550`、`:12941-12942` |
| `warningTime` | `int` | `0` | cache / compatibility | 不得小于 0；警告计时不能成为活动事实的第二来源 | partial | `Main.cs:13611-13617` |
| `warningDelay` | `int` | `420` | compatibility | 不得小于 0；Version4 默认值为 420 | confirmed | `Main.cs:540` |

#### 字段不变量

- Version4 的 `timeState` 具有正/负双重语义，不能直接改成只允许非负的 `timeRemaining` 而丢失冷却信息。
- `active` 与 `timeState` 同时存在时，必须定义不一致的恢复规则；目前只能标记 `decision-required`。
- `SlimeRainNpcSlots`、`slimeRainNPC` 属于生成压力边界，不是本 Component 的字段。

#### 生命周期

世界清理复位；启动时如果无正计时则生成活动计时；停止时写入负冷却计时并清除活动标志；警告计时独立结束。Version4 的随机持续时间和停止冷却使用 `Main.rand`，与 `WorldEventRandomState` 的关系未闭合。

#### Entity/World 范围

World singleton。Slime NPC 的生成实体和槽位集合不附着到此 Component。

#### ID 与关系字段

无直接 Version4 实体 ID。与 `EntityReference`、`WorldSectionId` 和生成压力 owner 的关系为 `crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

`WorldTimeWeatherState` 中有 `SlimeRain`、`SlimeRainTime`、`SlimeRainKillCount`、`SlimeWarningTime`；`WorldProgressionState` 又以 ticks 和 cooldown 拆开表达。两种表示存在 `version-drift`，不能直接合并。

#### 证据

`Main.cs:540-550`、`:12926-13013`、`:13611-13617`；`NetMessage.cs:302-303`；`WorldProgressionState.cs` 的 Slime Rain 字段；Dome 持久化版本 15 的 Slime Rain cooldown 边界。

### 5.7 WorldInvasionState

```text
componentId: WORLDCALENDARANDEVENTORCHESTRATION-WORLD-INVASION
name: WorldInvasionState
status: proposed
componentOwner: WorldCalendarAndEventOrchestration（候选）
crossSubsystemOwner: integration-review
entityScope: World singleton; invasion movement is world-space fact
lifecycle: none -> starting or approaching -> active -> depleted -> none
```

#### 职责

保存普通 Goblin、Frost、Pirate、Martian 入侵的世界级运行时事实：类型、规模、位置、延迟、警告和显示进度。完成历史另由 `InvasionHistoryState` 保存。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
| --- | --- | --- | --- | --- | --- | --- |
| `type` | `InvasionType` | `None` | authoritative / snapshot | `None` 表示没有普通入侵；有效类型必须在 Version4 支持范围内 | confirmed | `Main.cs:1059`、`:12632-12654` |
| `positionX` | `double` | `0.0` | authoritative / snapshot | 必须有限；坐标单位必须保持世界 tile 坐标语义 | confirmed | `Main.cs:1061`、`:12577-12605` |
| `size` | `int` | `0` | authoritative | 不得小于 0；活动普通入侵应大于 0 | confirmed | `Main.cs:1063`、`:12550-12574` |
| `sizeStart` | `int` | `0` | authoritative / snapshot | 活动时不得小于当前 `size`；结束状态的复位语义须明确 | confirmed | `Main.cs:1069`、`:12663-12667` |
| `delay` | `int` | `0` | authoritative / compatibility | 不得小于 0；实际用途和保存恢复必须与 Version4 一致 | confirmed | `Main.cs:1065`、`WorldFile.cs:1344`、`:2168` |
| `warningTimer` | `int` | `0` | cache / compatibility | 不得小于 0；警告显示不能改变入侵类型 | confirmed | `Main.cs:1067`、`:12668` |
| `progress` | `int` | `0` | snapshot / derived | 不得小于 0；与 `progressMax` 的单位必须一致 | confirmed | `Main.cs:1073`、`:11848-11855` |
| `progressMax` | `int` | `0` | snapshot / derived | 不得小于 0；正常活动时应能解释为规模或波次上限 | confirmed | `Main.cs:1075`、`:12664-12667` |
| `progressIcon` | `int` | `0` | snapshot / compatibility | 仅是显示分类，不拥有入侵规则 | confirmed | `Main.cs:1071`、`:12665` |
| `progressWave` | `int` | `0` | snapshot / compatibility | 不得小于 0；显示波次不能反写规模 | confirmed | `Main.cs:1077`、`:12666` |

#### 字段不变量

- `type == None` 时不得存在未解释的活动规模；Version4 的所有结束复位字段需要逐项核对。
- `sizeStart >= size >= 0` 是当前 NLTX 和 Version4 共同可观察的候选约束，但 Version4 所有路径的复位时点仍为 partial。
- `positionX` 是空间位置事实，不是 `WorldSectionId` 或 NPC 的 `EntityId`。

#### 生命周期

世界清理回到空状态；启动写入类型、规模、起始规模、进度和初始位置；活动期间位置和规模变化；规模耗尽后清除类型并保留可审计的完成历史。所有开始、结束和跨域调用图未完整闭合。

#### Entity/World 范围

World singleton。普通入侵不是每个 NPC 一个 Component；NPC 击杀只通过跨域事实影响其规模或进度，NPC 身份不归本 Component 所有。

#### ID 与关系字段

通用 `eventInstanceId`、`WorldSectionId`、`EntityReference`、`NetworkId` 与 `PersistentEntityId` 均为 `crossSubsystemOwner: integration-review`。Version4 目前没有直接的普通入侵实例 ID。

#### 当前 NLTX 映射

`WorldSessionComponents.cs` 的 `InvasionRuntimeState` 已有 `Type`、`PositionX`、`Size`、`SizeStart`、`Delay`、`WarningTimer`、`Progress`、`ProgressMax`、`ProgressIcon`、`ProgressWave`。它证明局部字段形状存在，不证明与所有 Version4 写者一致。

#### 证据

`Main.cs:1059-1081`、`:11809-11842`、`:12542-12684`；`NetMessage.cs:392`；`WorldFile.cs:1344-1347`、`:1377`、`:2168-2174`、`:3677-3681`。

### 5.8 InvasionHistoryState

```text
componentId: WORLDCALENDARANDEVENTORCHESTRATION-INVASION-HISTORY
name: InvasionHistoryState
status: proposed
componentOwner: WorldProgressionAndUnlocks（候选）
crossSubsystemOwner: integration-review
entityScope: World singleton
lifecycle: world-load -> persistent history -> world-clear only when the world is discarded
```

#### 职责

保存入侵是否曾完成的持久历史，与当前正在移动或进行中的入侵分离。它不保存当前入侵的规模、位置或波次。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
| --- | --- | --- | --- | --- | --- | --- |
| `defeatedGoblins` | `bool` | `false` | authoritative / snapshot | 只能由 Goblin 入侵完成事实置真，不能由当前入侵活动推导 | confirmed | `Main.cs:12552-12556` |
| `defeatedFrost` | `bool` | `false` | authoritative / snapshot | 只能由 Frost 入侵完成事实置真 | confirmed | `Main.cs:12557-12561` |
| `defeatedPirates` | `bool` | `false` | authoritative / snapshot | 只能由 Pirate 入侵完成事实置真 | confirmed | `Main.cs:12562-12566` |
| `defeatedMartians` | `bool` | `false` | authoritative / snapshot | 只能由 Martian 入侵完成事实置真 | confirmed | `Main.cs:12567-12571` |
| `defeatedClown` | `bool` | `false` | compatibility | 若保留，必须证明它属于本责任面的持久历史，不得因当前 NLTX 字段存在就宣称 Version4 等价 | partial | 当前 NLTX `InvasionProgressFlags.Clown`；本任务 Version4 直接证据不足 |

#### 字段不变量

历史标志是单向持久事实候选；当前入侵结束但未成功不应自动置真。`defeatedClown` 不能与 Version4 四种普通入侵标志混为已确认基线。

#### 生命周期

世界生成或加载时读取；完成对应入侵后更新；保存时保留；世界被清除时才回到默认值。具体完成 owner 和网络恢复尚未完整确认。

#### Entity/World 范围

World singleton。它不附着到事件实例或 NPC 实体。

#### ID 与关系字段

没有直接实体 ID。与 `eventInstanceId` 的“哪一次入侵完成”审计关系、与 `PersistentEntityId` 的世界身份关系均为 `crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

`WorldEventProgressState.Invasions` 和 `WorldProgressionState` 的 `DefeatedGoblins`、`DefeatedFrost`、`DefeatedPirates`、`DefeatedMartians` 提供局部映射。当前模型将历史与运行时放在相邻聚合中，最终 Component owner 尚未裁决。

#### 证据

`Main.cs:12550-12574`；`WorldSessionComponents.cs` 中 `InvasionProgressFlags`；`WorldProgressionState.cs` 中对应只读字段。Version4 全部历史字段的存档、网络和清除调用图仍为 partial。

### 5.9 PendingWorldEventState

```text
componentId: WORLDCALENDARANDEVENTORCHESTRATION-PENDING-WORLD-EVENT
name: PendingWorldEventState
status: proposed
componentOwner: WorldCalendarAndEventOrchestration（候选）
crossSubsystemOwner: integration-review
entityScope: World singleton
lifecycle: clear -> pending intent -> consumed, cancelled, or retained across load
```

#### 职责

保存尚未成为活动事件实例的世界事件意图和临时标志。它不保存事件活动期间的进度，也不持有生成结果。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
| --- | --- | --- | --- | --- | --- | --- |
| `spawnEye` | `bool` | `false` | authoritative / compatibility | 只表示待处理 Eye 生成意图，不表示已生成实体 | partial | `WorldSessionComponents.cs`；Version4 `Main.cs:13197` 附近 |
| `spawnHardBoss` | `int` | `0` | authoritative / compatibility | 允许值必须由跨域 owner 定义；零表示无待处理值 | partial | 当前 NLTX `PendingWorldEventsState`；Version4 Main 字段 |
| `spawnMeteor` | `bool` | `false` | authoritative / compatibility | 待处理与已落地流星事实分离 | partial | 当前 NLTX；Version4 Main 日历边界调用点 |
| `meteorShowerCount` | `int` | `0` | authoritative / snapshot | 不得小于 0；计数用途与持续时间不能混淆 | partial | 当前 NLTX；`WorldFile.cs` 对相关字段的临时读写 |
| `afterPartyOfDoom` | `bool` | `false` | compatibility | 不得从 `BirthdayPartyState.isUp` 推导 | partial | 当前 NLTX `PendingWorldEventsState`；Version4 Main 成员 |
| `forceHalloweenForToday` | `bool` | `false` | authoritative / compatibility | 只作用于指定日期窗口，不成为 Pumpkin Moon 活动事实 | partial | `Main.cs:7360-7369`、`:13496` 附近 |
| `forceChristmasForToday` | `bool` | `false` | authoritative / compatibility | 只作用于指定日期窗口，不成为 Snow Moon 活动事实 | partial | 当前 NLTX；Version4 Main 成员和 `checkXMas` 调用点 |

#### 字段不变量

待处理意图和活动状态必须分开。例如 `spawnMeteor` 为真不表示 `WorldEventInstanceState` 已活动，`forceHalloweenForToday` 为真也不表示 `pumpkinMoon` 为真。

#### 生命周期

世界清理复位；产生意图后保留到被消费、取消或按 Version4 规则失效；保存恢复必须保留是否仍待处理。各字段的完整来源和消费调用图为 partial。

#### Entity/World 范围

World singleton。待处理意图不附着到尚未存在的实体。

#### ID 与关系字段

若一个待处理意图需要对应未来实例，`eventInstanceId` 是否提前分配未裁决；该关系以及可能的 `PlayerEntityId` 来源标记为 `crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

`WorldSessionComponents.cs` 中的 `PendingWorldEventsState` 与本候选字段形状接近，但不证明当前实现拥有 Version4 的全部来源、持久性和消费边界。

#### 证据

当前 NLTX `PendingWorldEventsState`；Version4 `Main.cs` 日历边界、临时季节标志和 `WorldFile.cs` 的对应保存/恢复区域。完整调用链是 evidence-gap。

### 5.10 WorldCalendarOverrideState

```text
componentId: WORLDCALENDARANDEVENTORCHESTRATION-CALENDAR-OVERRIDE
name: WorldCalendarOverrideState
status: proposed
componentOwner: WorldCalendarAndEventOrchestration（候选）
crossSubsystemOwner: integration-review
entityScope: World singleton
lifecycle: clear -> no override or active override -> expired or consumed
```

#### 职责

保存不属于基础时钟位置、但会改变日历模式或时间控制的标志。为避免把单个布尔值误当成独立 Component，将 Version4 中未拥有专属候选 Component 的血月、日食、Pumpkin Moon 和 Snow Moon 模式集中在此处；这不表示它们是同一种事件。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
| --- | --- | --- | --- | --- | --- | --- |
| `bloodMoon` | `bool` | `false` | authoritative / snapshot | 与 Pumpkin Moon、Snow Moon 的排他关系至少保持 Version4 已证实部分 | confirmed / partial | `Main.cs:606`、`:13452-13457`、`:13490-13494` |
| `eclipse` | `bool` | `false` | authoritative / snapshot | 只在合法日历边界进入；夜晚开始时按 Version4 语义清除 | confirmed / partial | `Main.cs:624`、`:13191-13195`、`:13521-13527` |
| `pumpkinMoon` | `bool` | `false` | authoritative / snapshot | 与 `snowMoon` 互斥；活动时不得保留 `bloodMoon` | confirmed | `Main.cs:608`、`:7431-7436` |
| `snowMoon` | `bool` | `false` | authoritative / snapshot | 与 `pumpkinMoon` 互斥；活动时不得保留 `bloodMoon` | confirmed | `Main.cs:610`、`:7449-7454` |
| `fastForwardTimeToDawn` | `bool` | `false` | authoritative / compatibility | 只能表达目标边界，不直接修改 `CalendarClock.timeOfDay` | partial | `NetMessage.cs:302`；Version4 Main 成员 |
| `fastForwardTimeToDusk` | `bool` | `false` | authoritative / compatibility | 与 dawn 目标的同时有效性未闭合 | partial | 当前 NLTX `WorldTimeWeatherState`；Version4 Main 成员 |
| `sundialCooldown` | `int` | `0` | authoritative / snapshot | 不得小于 0；只能作为时间控制冷却 | partial | `Main.cs:13458`、`:13522`；Version4 WorldFile 临时状态 |
| `moondialCooldown` | `int` | `0` | authoritative / snapshot | 不得小于 0；只能作为时间控制冷却 | partial | `Main.cs:13458`、`:13522`；Version4 WorldFile 临时状态 |

#### 字段不变量

- `pumpkinMoon` 和 `snowMoon` 不能同时为真；两者活动时 `bloodMoon` 必须为假，这是 Version4 的直接互斥证据。
- `eclipse`、`bloodMoon` 的全部资格条件涉及 Hardmode、玩家、Boss 和天气等外部事实，本文只保留结果，不复制条件输入。
- 快进标志是控制意图，不是时钟位置；`CalendarClock` 仍是时间位置的唯一候选 owner。

#### 生命周期

世界清理复位；日历边界可能开始或结束模式；时间控制标志在消费或过期后清除；部分模式的存档和恢复有 Version4 字段，但完整 writer 竞争尚未闭合。

#### Entity/World 范围

World singleton。模式影响可以被区域或 NPC 读取，但不建立区域级或实体级副本。

#### ID 与关系字段

与 `WorldEventInstanceState.eventInstanceId`、`WorldTime`、`NetworkId` 和 `PersistentEntityId` 的关系全部为 `crossSubsystemOwner: integration-review`。单独的 `EntityId` 不应代表全世界日历模式。

#### 当前 NLTX 映射

字段分散在 `WorldTimeWeatherState`、`PendingWorldEventsState` 和 WorldProgression 局部状态中；当前没有经过本轮确认的独立 `WorldCalendarOverrideState`。

#### 证据

`Main.cs:606-624`、`:7360-7457`、`:12988-12996`、`:13191-13565`；`NetMessage.cs:225-228`、`:298-304`、`:320-332`；`WorldFile.cs:98-112`、`:156-160`、`:1075-1080`。

### 5.11 Dd2PersistentProgressState

```text
componentId: WORLDCALENDARANDEVENTORCHESTRATION-DD2-PERSISTENT-PROGRESS
name: Dd2PersistentProgressState
status: proposed
componentOwner: WorldProgressionAndUnlocks（候选）
crossSubsystemOwner: integration-review
entityScope: World singleton
lifecycle: world creation -> persisted progress -> load -> world discard
```

#### 职责

保存 DD2 三个难度层级是否完成的世界持久进度。它不保存当前一局的胜负、竞技场、波次或临时生成状态。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
| --- | --- | --- | --- | --- | --- | --- |
| `downedInvasionT1` | `bool` | `false` | authoritative / snapshot | 只能表示 Tier 1 已完成，不等同当前运行活动 | confirmed | `DD2Event.cs:45`、`:93-100` |
| `downedInvasionT2` | `bool` | `false` | authoritative / snapshot | 只能表示 Tier 2 已完成 | confirmed | `DD2Event.cs:47`、`:97-100` |
| `downedInvasionT3` | `bool` | `false` | authoritative / snapshot | 只能表示 Tier 3 已完成 | confirmed | `DD2Event.cs:49`、`:97-100` |

#### 字段不变量

三个标志是持久历史，不应因 `ongoing`、`wonThisRun` 或 `lostThisRun` 的临时变化自动回退。Version4 的 `Save`/`Load` 直接读写它们，是本候选少数具有明确持久边界的字段。

#### 生命周期

世界建立时默认为 false；成功完成对应层级后置真；保存和加载保留；`ResetProgressEntirely` 才清除全部进度。版本读取失败时的回滚语义尚未作为本文设计对象裁决。

#### Entity/World 范围

World singleton，不附着到 DD2 敌人、晶塔或玩家。

#### ID 与关系字段

没有直接实体 ID。与 `eventInstanceId` 的完成审计和 `PersistentEntityId` 的世界身份关系为 `crossSubsystemOwner: integration-review`。

#### 当前 NLTX 映射

`WorldSessionComponents.cs` 的 `Dd2ProgressState.DownedTier1/2/3` 提供字段映射；`WorldProgressionState` 当前构造模型没有完整的 DD2 三层持久语义，判为 partial。

#### 证据

`DD2Event.cs:45-50`、`:93-128`；`WorldFile.cs:1411-1412`、`:2337`；`NetMessage.cs:325-328`。完整 DD2 版本兼容读取仍有待核对的分支。

### 5.12 Dd2RunState

```text
componentId: WORLDCALENDARANDEVENTORCHESTRATION-DD2-RUN
name: Dd2RunState
status: proposed
componentOwner: WorldCalendarAndEventOrchestration（候选）
crossSubsystemOwner: integration-review
entityScope: World singleton; arena relation remains world-space
lifecycle: idle -> ongoing -> won or lost -> idle
```

#### 职责

保存一局 DD2 的活动、胜负、难度、车道生成速率和竞技场边界。它不把永久三层完成标志和波次内等待混进来。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
| --- | --- | --- | --- | --- | --- | --- |
| `ongoing` | `bool` | `false` | authoritative / snapshot | 非活动状态不得继续暴露一局内波次进度 | confirmed | `DD2Event.cs:57`、`:179-193`、`:209-219` |
| `lostThisRun` | `bool` | `false` | authoritative / snapshot | 与 `wonThisRun` 的互斥和终结语义必须明确 | confirmed | `DD2Event.cs:51`、`:190-191`、`:342-347` |
| `wonThisRun` | `bool` | `false` | authoritative / snapshot | 与 `lostThisRun` 不得同时表示终结；Version4 的所有胜利路径尚未全覆盖 | confirmed / partial | `DD2Event.cs:53`、`:190-191`、`:256-263` |
| `ongoingDifficulty` | `int` | `0` | authoritative / compatibility | 允许范围必须与 Version4 三种难度一致；零值表示未开始或未解析 | confirmed / partial | `DD2Event.cs:65`、`:184-188` |
| `laneSpawnRate` | `int` | `60` | authoritative / compatibility | 不得为负；Version4 明确默认值为 60 | confirmed | `DD2Event.cs:55` |
| `arenaHitbox` | `Rectangle` 或等价值类型 | `Rectangle.Empty` | cache / snapshot | 必须是合法非负区域；其空间 owner 不能被本 Component 偷占 | confirmed / partial | `DD2Event.cs:61`、`:497-541` |

#### 字段不变量

`ongoing` 为假时 `wonThisRun`/`lostThisRun` 的保留时长必须明确；不能把结束结果和当前活动混成一个布尔值。`arenaHitbox` 由空间事实计算的可能性很高，暂定为 cache/snapshot 而不是地图几何权威事实。

#### 生命周期

开始一局时建立运行状态并清除本局结果；运行中可以进入胜利或失败终结；停止后活动标志归零并清理临时空间状态。完整难度选择、Betsy 和胜利路径只作为 partial evidence，不在本文补写。

#### Entity/World 范围

World singleton。竞技场是 World-space 值；是否需要 `WorldSectionId` 关联未裁决。

#### ID 与关系字段

`eventInstanceId`、`WorldSectionId`、`EntityReference`、`NpcEntityId` 和 `PlayerEntityId` 均是 `crossSubsystemOwner: integration-review`。`arenaHitbox` 不可替代区域 ID。

#### 当前 NLTX 映射

`WorldSessionComponents.cs` 的 `Dd2ProgressState` 包含 `Ongoing`、`WonThisRun`、`LostThisRun`、`OngoingDifficulty` 和 `LaneSpawnRate`，但没有本 Component 的单独竞技场字段；映射为 partial。

#### 证据

`DD2Event.cs:51-67`、`:118-128`、`:179-219`、`:342-347`、`:497-549`；完整参考中的难度和胜利辅助方法仅用于识别缺口，不能写成 Version4 当前完整行为。

### 5.13 Dd2WaveRuntimeState

```text
componentId: WORLDCALENDARANDEVENTORCHESTRATION-DD2-WAVE-RUNTIME
name: Dd2WaveRuntimeState
status: proposed
componentOwner: WorldCalendarAndEventOrchestration（候选）
crossSubsystemOwner: integration-review
entityScope: World singleton; positions and wave facts reference world-space entities
lifecycle: idle -> wave active -> inter-wave hold -> next wave or run end -> cleared
```

#### 职责

保存 DD2 当前波次、击杀/点数、波次间等待、已死亡 Goblin 位置和水晶掉落计数。它不拥有 DD2 永久完成标志或整局胜负。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
| --- | --- | --- | --- | --- | --- | --- |
| `currentWave` | `int` | `0` | authoritative / compatibility | 非活动时可为 0；活动启动后 Version4 从 1 开始 | partial | `DD2Event.cs:192-194`；`NPC.waveNumber` 使用点 |
| `waveKills` | `float` | `0f` | authoritative / compatibility | 必须有限且不小于 0；实际击杀计数 owner 为 integration-review | partial | `DD2Event.cs:193-194`、`:253-261` |
| `totalInvasionPoints` | `float` | `0f` | authoritative / compatibility | 必须有限且不小于 0；不能与 `waveKills` 混淆 | partial | `DD2Event.cs:192-194`；Version4 NPC 使用点 |
| `timeLeftUntilSpawningBegins` | `int` | `0` | authoritative / snapshot | 不得小于 0；为 0 才表示没有波次等待 | confirmed | `DD2Event.cs:75`、`:147-172` |
| `deadGoblinPositions` | `IReadOnlyList<Vector2>` | `empty` | cache / snapshot | 不得包含无法解释的位置；清理运行时必须清空 | confirmed / partial | `DD2Event.cs:67`、`:189`、`:216`、`:494-495` |
| `crystalsDroppingLastWave` | `int` | `0` | cache / compatibility | 不得小于 0；只能描述上一波掉落计算 | partial | `DD2Event.cs:69`、`:179-182` |
| `crystalsDroppingToDrop` | `int` | `0` | cache / compatibility | 不得小于 0；不能超过待结算数量 | partial | `DD2Event.cs:71`、完整 DD2 掉落辅助 |
| `crystalsDroppingAlreadyDropped` | `int` | `0` | cache / compatibility | 不得小于 0；不得重复结算 | partial | `DD2Event.cs:73`、完整 DD2 掉落辅助 |

#### 字段不变量

- `timeLeftUntilSpawningBegins` 只能表示波次间等待，不得重用为整局剩余时间。
- `deadGoblinPositions` 是位置快照，不是 Goblin `EntityId` 列表；若未来需要实体身份，必须另建明确关系。
- `currentWave`、`waveKills` 和 `totalInvasionPoints` 在 Version4 部分由 `NPC` 静态字段承载，当前 owner 不能由文件名或目录顺序推断。

#### 生命周期

开始一局或开始一波时初始化；波次间进入等待；等待归零后进入下一波或结束；运行终结时清除临时波次字段。Version4 的完整波次、Betsy、点数和水晶逻辑为 partial，不在本文补充算法。

#### Entity/World 范围

World singleton，位置值位于 World-space。死亡 Goblin 的实体生命周期不由本 Component 拥有。

#### ID 与关系字段

`NpcEntityId`、`EntityReference`、`WorldSectionId`、`eventInstanceId` 和可能的 `NetworkId` 均为 `crossSubsystemOwner: integration-review`。这些关系不能由 `Vector2` 位置或数组索引替代。

#### 当前 NLTX 映射

当前 `Dd2ProgressState` 只包含部分运行/等待字段；`WorldEventProgressState` 和 NPC 局部状态中没有经过本轮验证的独立波次 Component。判为 partial。

#### 证据

`DD2Event.cs:67-75`、`:147-172`、`:179-194`、`:235-335`、`:491-495`；完整参考的波次和掉落辅助方法仅作为 Version4 缺口标记。

### 5.14 WorldEventRandomState

```text
componentId: WORLDCALENDARANDEVENTORCHESTRATION-EVENT-RANDOM
name: WorldEventRandomState
status: proposed
componentOwner: WorldCalendarAndEventOrchestration（候选）
crossSubsystemOwner: integration-review
entityScope: World singleton; optional named stream relation
lifecycle: world-load or seed initialization -> consumed state -> snapshot/restore -> world-clear
```

#### 职责

保存事件随机决策所需的随机流事实，并明确随机状态与世界种子的关系。它不定义特定算法、不把随机结果直接当作事件状态，也不拥有实体随机性。

#### 字段

| 字段 | 类型 | 默认值 | 状态分类 | 不变量 | evidenceStatus | 证据 |
| --- | --- | --- | --- | --- | --- | --- |
| `state` | `uint` | `unresolved` | authoritative / snapshot | 必须能被恢复为同一随机流位置；零是否为有效状态不得擅自禁止 | partial | Dome `WorldEventRandomState.Value`；Version4 使用 `Main.rand` |
| `worldSeed` | `ulong` 或等价种子值 | `unresolved` | compatibility / snapshot | 必须与世界身份和随机流版本对应；不得把文本种子直接当数值状态 | partial | Dome 持久化从世界种子派生；Version4 WorldFile 世界种子字段 |
| `streamVersion` | `int` | `unresolved` | compatibility / snapshot | 版本改变时必须显式拒绝或迁移；不得静默按当前算法解释旧状态 | evidence-gap | Dome `WorldEventRandomState` 没有版本字段；Version4 `Main.rand` 关系未闭合 |
| `consumedDrawCount` | `long` | `0` | snapshot / compatibility | 不得小于 0；是否需要持久化尚无 Version4 直接证据 | evidence-gap | 事件随机消费可观察，但统一计数未被 Version4 证明 |

#### 字段不变量

随机状态、种子和流版本必须作为同一恢复语义解释。不能因为 Dome 有确定性 LCG 就声称 Version4 使用同一算法；也不能因为 Version4 使用 `Main.rand` 就省略可审计的世界种子关系。

#### 生命周期

世界建立或加载时初始化；事件随机决策消耗后产生新状态；保存或恢复时保持流位置；世界清理时失效。随机状态的具体消费边界和跨客户端复制资格未裁决。

#### Entity/World 范围

默认是 World singleton。若未来存在独立事件流，流名或流 ID 不能直接使用 `EntityId`，必须由 integration-review 定义。

#### ID 与关系字段

`PersistentEntityId`、`WorldId`、`eventInstanceId` 和可能的 stream identity 均为 `crossSubsystemOwner: integration-review`。随机状态不能通过 `NetworkId` 取得规则 owner。

#### 当前 NLTX 映射

`D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\WorldEventRandomState.cs` 有一个 `uint Value`；`DomeStatePersistenceFormat.cs` 当前从世界种子派生状态。Version4 的 `Main.rand` 并未在本任务证据中形成同等持久随机状态，因此为 partial。

#### 证据

Dome `WorldEventRandomState.cs:5-29`；`DomeStatePersistenceFormat.cs` 的随机状态读写；Version4 `Main.cs:12937`、`:12963` 和 Party/Sandstorm/入侵中的 `Main.rand` 使用点。算法、版本和复制边界为 evidence-gap。

## 6. Entity 与 Component 组合

### 6.1 World singleton 组合

下表描述建议的逻辑组合，不定义任何执行者或调度结构：

| World 逻辑组合 | 目的 | 组合约束 |
| --- | --- | --- |
| `CalendarClock` + `WorldCalendarOverrideState` | 将时间位置与日历模式/时间控制分开 | override 不得复制 `timeOfDay`；时间位置只有 `CalendarClock` 一个候选来源 |
| `WorldEventInstanceState` + 一个或多个事件专属状态 | 将通用实例身份与 Party、Lantern、Sandstorm、Slime Rain、入侵或 DD2 专属字段分开 | `eventKind`、`eventInstanceId` 和专属状态的对应关系必须可验证；多实例能力尚未裁决 |
| `WorldInvasionState` + `InvasionHistoryState` | 将当前入侵和完成历史分开 | 当前 `type/size/position` 不得直接置写历史标志 |
| `Dd2PersistentProgressState` + `Dd2RunState` + `Dd2WaveRuntimeState` | 将永久进度、单次运行和波次内事实分开 | 一局失败或波次清除不得自动清除永久三层标志 |
| `WorldEventRandomState` 与上述需要随机决策的 Component | 提供显式随机状态关系 | 随机状态的来源和消费 owner 未裁决，不能按目录或组合顺序假定 |
| `PendingWorldEventState` 与事件实例状态 | 区分待处理意图和已经活动的实例 | pending 标志不等于 active 实例，不得共享同一布尔字段 |

### 6.2 关系字段清单

所有以下关系均明确写为 `crossSubsystemOwner: integration-review`，不能在 Component-only 设计中擅自分配最终 owner：

| 关系或类型 | 关联 Component | 当前证据 | 未决点 |
| --- | --- | --- | --- |
| `WorldTime` / `WorldTickSnapshot` | `CalendarClock` 与全部时间敏感状态 | Version4 `Main.time`、Dome `WorldClockSnapshot` | 时间单位、默认值、快照 owner |
| `EntityId` | `WorldEventInstanceState` | 当前 NLTX 有实体模型，但 Version4 事件没有统一实例实体 | 事件实例是否拥有实体身份 |
| `PersistentEntityId` | 实例和持久历史关系 | Dome 有世界唯一 ID；Version4 有分散世界身份字段 | 运行时 ID 与跨重载 ID 的转换 |
| `NetworkId` | 实例或快照关系 | `NetMessage` 有事件结果字段，没有通用实例 ID | 是否复制实例身份及其有效期 |
| `NpcEntityId` | `BirthdayPartyState`、`Dd2WaveRuntimeState` | Party 使用 `whoAmI` 整数，DD2 使用位置列表 | 运行时 NPC ID、持久 ID、数组索引的分离 |
| `PlayerEntityId` | 事件资格关联 | Version4 资格代码读取玩家状态 | 玩家引用是否进入状态，还是只作为外部事实 |
| `WorldSectionId` | 沙尘暴、DD2 竞技场、位置关系 | 当前 Version4 字段主要是世界坐标或矩形 | 是否需要区域化 Component |
| `EntityReference` | 通用事件实例、NPC/空间关系 | 当前模型没有统一跨域引用类型 | 引用有效期、缺失和恢复语义 |
| `eventInstanceId` | 所有带生命周期的专属事件状态 | Version4 只有分散布尔/计时字段 | 生成时机、唯一性、持久化和多实例能力 |

### 6.3 组合禁忌

- 不把 `WorldTimeWeatherState` 直接作为最终目标 Component；它把时间、天气、Party、Lantern Night、Sandstorm 和多个模式混在一个宽状态中。
- 不把 `WorldEventInstanceState` 与 `WorldInvasionState`、`Dd2RunState` 合并成一个“所有事件”记录；实例身份和专属事实的生命周期不同。
- 不把 `InvasionHistoryState` 与 `WorldInvasionState` 合并；当前运行结束和历史完成是不同事实。
- 不把三种 DD2 状态合并；持久进度、运行结果和波次缓存的恢复边界不同。
- 不为 WorldSection 创建 Component，除非 integration-review 先确认 Version4 空间 owner 和独立生命周期。

## 7. 拆分/合并决策

### 7.1 必须拆分

| 决策 | 结论 | 理由 |
| --- | --- | --- |
| 时钟与日历模式 | 拆分 `CalendarClock` / `WorldCalendarOverrideState` | `Main.time/dayTime/moonPhase` 是位置事实；血月、日食、季节月和快进是模式或控制事实，生命周期和互斥关系不同 |
| 通用事件身份与专属事件字段 | 拆分 `WorldEventInstanceState` 与各专属 Component | 同一种事件可能需要不同专属字段；Version4 没有统一实例 ID，必须先显式暴露缺口 |
| Party、Lantern Night、Sandstorm、Slime Rain | 分别保留四个 Component | 来源字段、冷却/强度/参与者/带符号计时等不变量不同；合并会重新制造宽状态 |
| 普通入侵运行时与历史 | 拆分 `WorldInvasionState` / `InvasionHistoryState` | 当前规模和位置会变化，完成历史具有持久单向语义 |
| DD2 三层状态 | 拆分 `Dd2PersistentProgressState` / `Dd2RunState` / `Dd2WaveRuntimeState` | `DD2Event.Save/Load` 明确只保存三层完成标志，运行和波次字段具有不同有效期 |
| 随机状态与事件状态 | 单独 `WorldEventRandomState` | 随机流是依赖事实，不是事件结果；Version4 与 Dome 的来源不一致，必须独立审查 |

### 7.2 允许合并但当前不作合并

- `bloodMoon`、`eclipse`、`pumpkinMoon`、`snowMoon` 暂集中在 `WorldCalendarOverrideState`，因为当前证据没有足够的统一实例身份；这不是宣称它们规则相同。
- `PendingWorldEventState` 保持单独 Component，即使其成员很少，因为待处理意图和活动实例有不同生命周期。
- `CalendarClock` 保留 `dayLengthTicks` 和 `nightLengthTicks` 作为快照字段，以保留 Dome 当前可恢复的周期参数；这两个字段是否应降为不可持久化常量待裁决。

### 7.3 不因现有文件形状合并

当前 `WorldSessionComponents.cs` 和 `WorldProgressionState.cs` 的聚合形状是已有实现边界证据，不是目标边界命令。文件名、目录顺序、静态类形状和字段相邻关系都不能单独证明 Component owner。

## 8. 不单独创建 Component 的对象

以下对象不单独成为 Component；它们可以作为字段、派生值、值对象、兼容记录或外部关系存在，但本文不为它们定义独立 Component：

| 对象 | 处理方式 | 不单独创建的理由 |
| --- | --- | --- |
| `PartyIsUp` / `LanternsUp` | 由对应活动来源字段派生 | 是两个来源布尔值的组合，不拥有独立生命周期 |
| `IsSlimeRaining`、入侵活动判断、DD2 等待判断 | 由对应 Component 字段派生 | 不能复制权威事实 |
| 单个节日、单个昼夜边界、单个 announcement | 作为事件实例字段、生命周期事实或快照关系 | 没有独立完整状态边界的证据 |
| `WorldClockSnapshot`、`WorldClockTransition` | 作为快照/值对象候选 | 是 `CalendarClock` 的数据形态，不是额外世界状态 |
| `WorldEventKind` | 作为事件种类值 | 枚举不拥有生命周期或写集 |
| `NPC.waveNumber`、`waveKills`、`totalInvasionPoints` 的旧承载位置 | 作为 `Dd2WaveRuntimeState` 的 owner 待裁决字段 | 旧静态承载位置不能直接等于最终 Component owner |
| `ArenaHitbox` | `Dd2RunState` 的空间值或缓存候选 | 矩形值本身不拥有 DD2 运行生命周期 |
| `_deadGoblinSpots` | `Dd2WaveRuntimeState` 的位置集合 | 集合是波次缓存，不是每个位置一个 Component |
| `Main.slimeRainNPC`、`slimeRainNPCSlots` | 留在生成压力边界的关系/外部状态 | 它们描述生成资源，不是 Slime Rain 日历事实 |
| `WorldFile` 临时字段 | 对应 Component 的 `snapshot` / `compatibility` 字段 | 临时存档副本不应成为第二份权威状态 |
| `NetMessage` WorldData 或单个 wire flag | 对应 Component 的快照形态 | 复制编码不是规则状态 owner |
| 单个 NPC 生成策略、单个天气效果、单个结果通知 | 外部实体/天气/表现关系 | 没有本责任面内可独立裁决的 Component 状态边界 |

## 9. 当前 NLTX 覆盖

| 当前路径 | 已观察内容 | 对目标 Component 的覆盖 | 结论 |
| --- | --- | --- | --- |
| `D:\TRbackup\NLTX\src\WorldSession\WorldSessionComponents.cs` | Party、Lantern Night、Sandstorm、WorldTimeWeather、Invasion、DD2、Pending 等局部状态 | 字段形状覆盖较多，但多个责任面仍在宽状态中 | partial；不是目标设计已实现的证明 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\WorldClock.cs` | 可验证的 tick、昼夜长度、暂停和月相不变量 | 对 `CalendarClock` 提供局部结构和默认周期 | partial；`timeOfDay` 与 Version4 冲突 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\WorldClockTransition.cs` | Dawn/Dusk 值对象 | 只能作为时钟边界值参考 | partial；不形成单独 Component |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\WorldEventKind.cs` | 仅有部分事件枚举 | 对通用事件种类提供有限参考 | partial；未覆盖全部 Version4 事件 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\WorldEventRandomState.cs` | `uint Value` 和确定性推进 | 对 `WorldEventRandomState` 提供一个局部字段形状 | partial；不证明 Version4 随机算法 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\WorldProgressionState.cs` | 日历、入侵、Slime Rain 和 Lantern Night 的聚合不可变状态 | 证明拆分时需要保留现有语义映射 | partial；当前聚合不是目标 Component 边界 |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Server\Persistence\DomeStatePersistenceFormat.cs` | 当前 Dome 持久化格式版本为 37，并读写时钟、进度和随机状态 | 仅证明当前兼容快照存在 | partial；存在 `version-drift`，不能宣称 Version4 存档等价 |

当前 NLTX 没有经本轮验证的统一 `WorldEventInstanceState`、独立 `InvasionHistoryState`、独立 `WorldCalendarOverrideState` 或独立 DD2 波次 Component。现有类型不能被改写成目标 Component 已完成，也不能据此声称行为等价或 API 兼容。

## 10. Component evidence-gap

| Component | evidence-gap | 影响 |
| --- | --- | --- |
| `CalendarClock` | Version4 `Main.time = 13500.0` 与 Dome 默认 `timeOfDay = 0.0` 冲突；`dayRate = 0` 的完整语义未闭合 | 无法锁定默认快照和时间 owner |
| `WorldEventInstanceState` | Version4 没有统一事件实例 ID、开始/结束 tick、网络 ID 或持久实体 ID | 无法确认一对多事件关系、恢复身份和生命周期终结 |
| `BirthdayPartyState` | Version4 当前覆盖缺失完整 `CanNPCParty`、`NaturalAttempt` 语义和所有参与者恢复路径 | 无法声称自然 Party 和 NPC 关系完整等价 |
| `LanternNightState` | 完整资格、Boss 活动判断、排期序列和网络/存档恢复关系未闭合 | 无法锁定预定与当前活动的全部边界 |
| `SandstormState` | 停止调用图、风速/区域 owner 和所有保存/恢复路径尚未闭合 | 无法锁定强度与天气输入的最终关系 |
| `SlimeRainState` | Version4 正/负 `slimeRainTime` 语义与 NLTX 非负 ticks/cooldown 表示不同；随机来源不同 | 无法直接合并字段或确认冷却不变量 |
| `WorldInvasionState` | 普通入侵全部启动、结束、进度报告和网络恢复调用图不完整 | 无法锁定单一写入 owner 和结束快照 |
| `InvasionHistoryState` | 所有历史标志的存档、网络和跨域完成 owner 未完整确认 | 无法确认历史标志的完整集合和回滚语义 |
| `PendingWorldEventState` | 待处理事件各来源、消费时点和保存恢复调用图不完整 | 无法确认 pending 是否跨重载、何时失效 |
| `WorldCalendarOverrideState` | Hardmode、Boss、玩家、天气和临时季节标志之间存在 writer 竞争 | 无法锁定全部互斥关系和 override owner |
| `Dd2PersistentProgressState` | DD2 版本读取分支、完成路径和历史恢复尚未全部核对 | 无法声称三层持久进度完整兼容 |
| `Dd2RunState` | 完整难度、胜利、Betsy、ArenaHitbox 空间 owner 和失败路径不完整 | 无法锁定一局终结和竞技场状态恢复 |
| `Dd2WaveRuntimeState` | 波次、击杀、点数、水晶和位置字段横跨 `DD2Event`、`NPC` 及其他路径 | 无法确定波次 Component 的最终写集 |
| `WorldEventRandomState` | Version4 `Main.rand` 与 Dome 世界种子派生流不一致；流版本和消费计数未定义 | 无法保证重复运行、恢复和跨端随机一致性 |

本轮不创建、不运行任何 verifier；上述缺口不是已验证失败，而是未闭合证据。`verificationStatus: not-run` 保持不变。

## 11. 未决 owner

以下问题会改变 Component 的权威 owner、字段类型或跨域关系，不能由本设计替代整合裁决：

| 决策 ID | 未决问题 | 受影响 Component | owner 标记 |
| --- | --- | --- | --- |
| `BD-WCE-01` | `WorldTime` 的提交槽位、`timeOfDay` 初始值和 `dayRate = 0` 语义由谁最终确认 | `CalendarClock`、`WorldCalendarOverrideState` | `crossSubsystemOwner: integration-review` |
| `BD-WCE-02` | 事件实例身份由 World 记录、独立事件实体还是跨域实体引用拥有；NPC spawn owner 是否与事件实例分离 | `WorldEventInstanceState`、Party、入侵、DD2 | `crossSubsystemOwner: integration-review` |
| `BD-WCE-03` | 事件快照是否跨 World、NPC、空间和网络边界聚合，还是每个 Component 独立保存 | 全部 `snapshot` 字段 | `crossSubsystemOwner: integration-review` |
| `BD-WCE-04` | `NpcEntityId`、`PersistentEntityId`、`NetworkId` 和 `EntityReference` 的转换与失效语义 | Party、DD2、通用实例 | `crossSubsystemOwner: integration-review` |
| `BD-WCE-05` | `WorldSectionId` 是否进入 Sandstorm、DD2 Arena 或其他空间关系 | Sandstorm、Dd2Run、Dd2Wave | `crossSubsystemOwner: integration-review` |
| `BD-WCE-06` | Version4 `Main.rand` 与 Dome 世界种子随机流是否兼容、是否需要独立流版本 | `WorldEventRandomState` 及所有随机事件 | `crossSubsystemOwner: integration-review` |
| `BD-WCE-07` | 事件状态与 Hardmode、天气 override、WorldProgression 之间的竞争写者如何归属 | CalendarOverride、入侵、Slime Rain、DD2 | `crossSubsystemOwner: integration-review` |
| `BD-WCE-08` | 当前 Dome 持久化格式 `CurrentFormatVersion = 37` 与 Version4 字段版本边界如何共同解释 | 全部 `snapshot` / `compatibility` 字段 | `crossSubsystemOwner: integration-review` |

## 12. 最终 Component 清单

| # | componentId | name | status | entityScope | 当前判断 |
| --- | --- | --- | --- | --- | --- |
| 1 | `WORLDCALENDARANDEVENTORCHESTRATION-CALENDAR-CLOCK` | `CalendarClock` | `proposed` | World singleton | decision-required |
| 2 | `WORLDCALENDARANDEVENTORCHESTRATION-EVENT-INSTANCE` | `WorldEventInstanceState` | `proposed` | World event instance entity（边界未裁决） | evidence-gap |
| 3 | `WORLDCALENDARANDEVENTORCHESTRATION-BIRTHDAY-PARTY` | `BirthdayPartyState` | `proposed` | World singleton | partial |
| 4 | `WORLDCALENDARANDEVENTORCHESTRATION-LANTERN-NIGHT` | `LanternNightState` | `proposed` | World singleton | partial |
| 5 | `WORLDCALENDARANDEVENTORCHESTRATION-SANDSTORM` | `SandstormState` | `proposed` | World singleton | partial |
| 6 | `WORLDCALENDARANDEVENTORCHESTRATION-SLIME-RAIN` | `SlimeRainState` | `proposed` | World singleton | partial |
| 7 | `WORLDCALENDARANDEVENTORCHESTRATION-WORLD-INVASION` | `WorldInvasionState` | `proposed` | World singleton | partial |
| 8 | `WORLDCALENDARANDEVENTORCHESTRATION-INVASION-HISTORY` | `InvasionHistoryState` | `proposed` | World singleton | partial |
| 9 | `WORLDCALENDARANDEVENTORCHESTRATION-PENDING-WORLD-EVENT` | `PendingWorldEventState` | `proposed` | World singleton | partial |
| 10 | `WORLDCALENDARANDEVENTORCHESTRATION-CALENDAR-OVERRIDE` | `WorldCalendarOverrideState` | `proposed` | World singleton | decision-required |
| 11 | `WORLDCALENDARANDEVENTORCHESTRATION-DD2-PERSISTENT-PROGRESS` | `Dd2PersistentProgressState` | `proposed` | World singleton | partial |
| 12 | `WORLDCALENDARANDEVENTORCHESTRATION-DD2-RUN` | `Dd2RunState` | `proposed` | World singleton | partial |
| 13 | `WORLDCALENDARANDEVENTORCHESTRATION-DD2-WAVE-RUNTIME` | `Dd2WaveRuntimeState` | `proposed` | World singleton | partial |
| 14 | `WORLDCALENDARANDEVENTORCHESTRATION-EVENT-RANDOM` | `WorldEventRandomState` | `proposed` | World singleton | decision-required |

## 13. 最终声明

本文件是 `WorldCalendarAndEventOrchestration` 的 Component-only Design，不是迁移完成报告、行为等价证明、API 兼容证明或当前 NLTX 已完成能力声明。

本文件只定义候选 Component、字段、默认值、不变量、生命周期、Entity/World 范围、组合关系、字段分类和 evidence-gap。所有 14 个候选 Component 都是 `status: proposed`；没有任何候选 Component 被声明为 `baseline`。

本文件没有定义或实现 System、Query、Command、Adapter、Projection、Coordinator、Pipeline、Scheduler、Verifier、调度顺序、主循环、网络流程、存档流程、测试计划、迁移计划、`.cs` 或 `.csproj`。本文也没有把现有 NLTX 宽状态、Dome 局部模型、Version4 网络字段或 WorldFile 临时字段误写成已完成的目标 Component。

证据和 NLTX 状态保持 `partial`，设计状态为 `decision-required`，验证状态为 `not-run`。未决 owner、实例身份、随机状态、时间默认值和 Version4 事件缺口必须在后续整合审查中单独裁决。
