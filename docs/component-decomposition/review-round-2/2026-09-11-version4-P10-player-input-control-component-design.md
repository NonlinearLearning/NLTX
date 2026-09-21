# Version4 P10 玩家输入、控制、选择与建造交互组件拆分设计

partitionId: P10
sessionId: dfcf6281bf7d4ac2804704e61407153d
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P10-Player-Input-Control.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P10-player-input-control-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P10-player-input-control-component-execution.md
designStatus: proposed
executionStatus: blocked
implementationStatus: partial
evidenceStatus: partial
currentNltxStatus: partial
verificationStatus: not-run
designCompletedComponents: [C01_PlayerRawControlInput, C02_PlayerReleaseAndRepeatState, C03_PlayerInteractionRuntimeState, C04_PlayerItemUseIntent, C05_PlayerChannelAndActionContext, C06_PlayerMovementAndWorldInteraction, C07_PlayerNavigationAndTimeInstruments, C08_PlayerDetectionAndWiringInstruments, C09_PlayerBuilderInteractionDefinitions, C10_PlayerSelectionState, C11_PlayerInputSyncAndMatchAdapters, C12_PlayerBuilderOverlayProjection, C13_PlayerItemSpaceAndSettings]
completedComponents: [C01_PlayerRawControlInput, C02_PlayerReleaseAndRepeatState, C02_PlayerReleaseAndRepeatFocusedVerifier, C03_PlayerInteractionUiStateCore, C03_PlayerItemReuseQuery, C03_PlayerItemReuseStateComponent, C03_PlayerNameDefinition, C03_PlayerNpcPressureQuery, C04_PlayerItemUseIntentCore, C04_PlayerEntityInteractionLockStateComponent, C05_PlayerChannelCancellationCore, C06_PlayerInstantMovementAccumulatorCore, C06_PlayerActuationRodLockStateComponent, C06_PlayerStepSoundProjectionCore, C07_PlayerNavigationInstrumentCore, C08_PlayerDetectionInstrumentCore, C08_PlayerDetectionInstrumentComponent, C09_PlayerBuilderInteractionDefinitions, C10_PlayerSelectionQueryCore, C11_PlayerInputSyncProjectionCore, C12_PlayerBuilderOverlayProjection, C13_PlayerItemSpaceEligibilityQueryCore, C13_PlayerSettingsAdapterCore, C13_PlayerItemSpaceEvaluationQueryCore]
currentComponent: C04_PlayerEntityInteractionLockState_integration
pendingComponents: [C03_PlayerInteractionRuntimeState_crossSubsystem, C04_PlayerEntityInteractionLockState_integration, C05_PlayerChannelStateComponent_crossSubsystem, C06_PlayerActuationRodLockState_integration, C07_PlayerWatchTimeProjection_dependency, C08_PlayerWiringOverlayProjection_crossSubsystem, C10_PlayerSelectionState_crossSubsystem, C11_PlayerInputSyncAndMatchAdapters_crossSubsystem, C13_PlayerItemSpaceAndSettings_crossSubsystem]
currentProgress: C01 原始控制隔离核心、C02 释放/重复隔离核心、C02 release/repeat focused verifier、C09 建造交互定义/查询、C12 建造 overlay 纯投影核心、C03 UI/交互门控核心、C03 `nameLen` 定义/范围 Query、C03 ItemUse 复用资格 Query、C03 `PlayerItemReuseStateComponent`、C03 ItemUse 重用状态组件、C03 NPC 压力贡献 Query、C04 ItemUse/tile intent core、C04 `PlayerEntityInteractionLockStateComponent` 字段、C05 channel cancellation expectation Adapter core、C06 instant-movement accumulator core、C06 step-sound projection core、C06 `PlayerActuationRodLockStateComponent`、C07 导航仪器能力事实 reset/apply core、C08 检测仪器能力事实 reset/apply core、C08 `PlayerDetectionInstrumentComponent.ThirdEyeCounter` 字段、C10 选择状态纯 Query core、C11 输入同步纯 projection core、C13 ItemSpace eligibility pure Query core、C13 DashControl settings adapter core 和 C13 ItemSpace explicit-facts evaluation Query core 已实现并保存；C02、C03、C04、C05、C06、C07、C08、C09、C10、C11、C12、C13 focused verifier 已通过。C01/C02/C03/C04/C05/C06/C07/C08/C09/C10/C11/C12/C13 尚未接入完整运行时协议、调度器、ItemCheck、Tile/Wiring、Equipment、Projectile、Combat、音频播放、时间快照、World detection、P09 selection authority、sequence/replay policy、renderer、Inventory/VoidVault owner 或 Settings owner；C02、C03、C04、C05、C06、C07、C08、C10、C11、C13 仍有跨分区成员待集成审查。
lastCheckpointUtc: 2026-09-12T09:35:12.4903762Z
evidence-gap: C01 的 sequence/replay envelope、协议 adapter、现有 NLTX 唯一 writer、生命周期调度和行为等价 verifier 仍未闭合。C02 的 focused verifier 已覆盖显式输入下的 reset、release 派生、hover 传递和方向 timer 边界，但真实上一 tick 输入来源、Item/Tile release 消费、空批/非法批次清理、spawn/teleport/disconnect 调度、重复包策略和完整行为等价仍缺证据。C03 的 `team/sign/aggro/nearbyActiveNPCs` 跨分区 owner、`reuseDelay/pendingItemReuse` ItemUse 唯一 writer、`changeItem` C10 handoff，以及 UI gate 的真实 client input writer 尚未闭合；NPC Query 的真实 Main/NPC 清零累加 writer、active-range 接入、slot cost 来源、NPC 类型注册范围、aggro targeting owner、调度顺序和行为等价仍缺证据。C04 的 `controlUseItem/controlUseTile` intent 虽有纯 reset/advance core，但协议映射、C02 release gate、Item/Tile/Wiring/Entity action owner、`tileInteractAttempted`、entity lock、success projection、`autoReuseAllWeapons`、`altFunctionUse` 和 `delayUseItem` 的生命周期及行为等价仍未闭合；未创建第二个 ItemUse authority。C05 的 channel cancellation expectation Adapter core 已按 Version4 规则验证，但 `manaCost` 的完整装备/能力输入、`channel` 唯一 Item/Projectile writer、Tag/intention/rabbit helper 生命周期、creative permission、网络 schema 和 channel 行为等价仍未闭合；未创建 `channel` authority 或持有 Projectile 引用。C06 的 accumulator 和 step-sound projection core 已闭合显式输入/常量映射，但 movement scheduler 的真实唯一 writer、rope/pulley 顺序、accumulator 消费点、step-sound cooldown/播放消费、装备 writer、hostile 权限/网络 owner、lastCreatureHit Combat/NPC/Projectile owner、ActuationRodLock Wiring/World owner 和行为等价仍未闭合。C07 的 `accWatchTime` 没有真实时间/world snapshot 生成路径证据，Equipment/ability 唯一 writer、跨玩家聚合、ResetEffects/lifecycle 调度、UI/网络读取和行为等价仍未闭合；本次只验证显式 accessory type 到六项能力事实的纯 reset/apply core。C08 的 `accThirdEyeCounter` 只有缺少第三眼能力时清零的证据，递增/上限/消费 writer 未闭合；`InfoAccMechShowWires` 的 Wiring/UI owner、World detection snapshot、Equipment/ability writer、Combat/DPS owner、生命周期和行为等价仍未闭合；本次只验证六项显式检测能力事实的纯 reset/apply core。C09 的 Builder/Wiring 实际消费者、内容版本兼容策略和 `builderAccStatus` 唯一 owner 尚未闭合。C12 的纯投影 focused verifier 已通过，但 Version4 ResetEffects 默认值、Equipment writer、renderer reader、生命周期和网络/持久化边界仍未接入或验证。
evidence-gap-addendum: C10 的 P09 `SelectedItemComponent` 唯一 authority、buffer/override 提交、slot/radial validation、selectedKite equipment writer、entity reference、network/UI command 去重和生命周期仍未闭合；本检查点仅验证显式选择快照上的纯派生语义。
evidence-gap-addendum: C11 的 per-session sequence/replay/permission、duplicate/乱序拒绝、channel expectation 生命周期、SetMatch 外观 owner 和网络失败语义仍未闭合；本检查点仅验证五个 committed control facts 的纯同步 projection 与 `PressingAnyInput` query。
evidence-gap-addendum: C13 的真实 item/content catalog 到 `PlayerItemSpaceCandidate` 的映射、背包/弹药/VoidVault snapshot owner、Item 类型与 prefix/stack/ammo/unique-item facts 的生产者、VoidVault capability writer、个人库存 commit owner、Settings/profile 来源与 `DashControl` 持久化仍未闭合；本检查点已在显式 facts 上验证 Version4 `ItemSpace` 的扫描顺序和结果派生，但没有创建 Item/Inventory/VoidVault/Settings authority。
evidence-gap-addendum: C13 的 `Settings.DashControl` 真实 profile/config reader、写入/持久化边界、非法枚举值策略和 dash runtime consumer 仍未闭合；本检查点只验证显式可选设置到本地 `PlayerDashControlPreference` 的默认/往返适配，不创建静态 Settings authority，也不与 C01 `controlDash` 双写。
evidence-gap-addendum: C02 focused verifier 的首轮运行发现并修正了 verifier 自身对第二帧右方向 timer 的错误期望；最终绿测只证明当前显式输入核心的确定性边界，不证明 C01 writer、真实 tick 调度、release 消费或跨系统行为等价。
evidence-gap-addendum: C06 的 `PlayerActuationRodLockStateComponent` 已保存 `ActuationRodLock` 的独立 `bool` 字段，默认值为 `false`；Wiring/World 唯一 writer、action begin/end、拒绝/完成、death/disconnect 清理和行为等价仍为 `integration-review`。
evidence-gap-addendum: C08 的 `PlayerDetectionInstrumentComponent.ThirdEyeCounter` 已保存为 `byte` 字段，默认值为 `0`；递增、上限、消费和与 `ThirdEyeEnabled` 的生命周期仍未闭合，不能把字段落地解释为运行时检测行为完成。
blocking-decision: integration-review

## 1. 范围与不变量

本设计覆盖正式父级 `PlayerGameplay` 下 P10 权威报告的 11 个叶子子系统、110 个字段、8 个属性，共 118 个成员。它是 ECS 组件边界、命令方向和迁移前验证计划，不是 C# 实现、行为等价证明、网络闭合证明或持久化闭合证明。所有 proposed 类型均保持 `status: proposed`。

源叶子组是成员库存边界，不等于运行时组件边界。组件按共同读写者、生命周期和状态语义拆分；一次性输入、网络包、内部 helper、查询结果、定义表、UI/网络输出和日志资源不得因为声明在 `Player` 内就变成权威持久组件。

迁移期间遵守以下不变量：

- 原始输入只能经协议 Adapter 和验证后的显式 Command 进入权威模拟；Query、DTO、Projection 不得直接写 Player 或 World。
- 每个 proposed 权威状态只有一个 Owner System/CommitPort。跨 P09 Inventory、P11 Presentation、P12/P13 Combat/NPC、P16 World、Wiring 或 Protocol 的共享语义标记 `crossSubsystemOwner: integration-review`。
- 释放边沿、重复窗口、channel 取消期望、选择缓冲和网络匹配请求分别建模，不能合并成一个泛化的 `PlayerInputComponent`。
- `PlayerInputSyncCache`、`SetMatchRequest`、`ChannelCancelKey`、`ItemSpaceStatus`、`BuilderAccToggleIDs` 和 `SelectionRadial` 先按其真实的 projection/adapter/query/definition 角色处理，不默认登记为持久组件。
- 文件或目录顺序不表达运行时执行顺序；顺序必须由 scheduler contract、显式 Command 和 focused verifier 固定。

## 2. 证据登记

| 来源 | 已核对事实 | 支持的设计结论 | 状态 |
|---|---|---|---|
| P10 权威报告 | 11 个叶子组、110 字段、8 属性、118 成员，来源序号 391..1426 | 本文必须逐条覆盖且不改变源成员类型与叶子归属 | confirmed |
| `D:\TRbackup\Version4\Terraria\Player.cs:43-430` | `BuilderAccToggleIDs`、`PlayerInputSyncCache`、`ChannelCancelKey`、`SetMatchRequest`、`ItemSpaceStatus`、`SelectedItemState`、`SelectionRadial` 是不同的嵌套类型 | definition、projection/cache、短期 payload、query value 和选择状态不能合并 | confirmed |
| `D:\TRbackup\Version4\Terraria\Player.cs:978-1011,1219-1331` | 交互字段、控制/释放字段、ItemUse/channel 字段在源中共存但访问语义不同 | 原始控制、释放窗口、ItemUse 意图和交互状态分开 | confirmed |
| `D:\TRbackup\Version4\Terraria\Player.cs:1781-1783,1968-2016` | builder overlay、PvP/移动帧、导航/检测字段的声明位置与类型 | overlay、movement/world、instrument state 分开；共享 owner 需交接 | confirmed |
| `D:\TRbackup\Version4\Terraria\Player.cs:10272-10660,14789-14980,15740-15810,17058-17091` | ResetEffects、Update、释放边沿和计时器路径会重置或消费多组字段 | 生命周期与每帧 reset 必须由 Owner System 显式表达 | confirmed |
| `D:\TRbackup\Version4\Terraria\Player.cs:23435-23564,25762-25798,26518-26578` | ItemCheck、channel 清理、StartChanneling/TryCancelChannel、构造初始化形成行为边界 | ItemUse/Channel 只能通过显式 command、结果和短期上下文交接 | confirmed |
| `D:\TRbackup\Version4\Terraria\MessageBuffer.cs:652-738,1440-1465,1765-1784` | 控制包反序列化、hostile 写入、item animation/channel 网络路径直接写 Player | 协议 Adapter 与领域 owner 分离；包去重/乱序策略尚未闭合 | confirmed/partial |
| `D:\TRbackup\Version4\Terraria\Main.cs:11462`；`NPC.cs:212,576,5792,64233,64464,64468` | `nearbyActiveNPCs` 在全局/NPC 路径清零、累加和消费 | 该成员是跨 NPC/Player 的聚合事实，`crossSubsystemOwner: integration-review` | confirmed |
| 当前 NLTX Dome | 已有 `ControlInputComponent`、`PlayerInputApplySystem`、`PlayerControlSystem`、`MovementIntentComponent`、`PlayerInput` 和 `SimulationInputBatch` | C01 应收敛已有输入边界，不能新建第二条输入 authority route | confirmed |
| 当前 NLTX Server/Protocol | `DomeNetworkUpdateBridge`、`LegacyPlayerControlsProjection`、`TerrariaSession.AcceptPlayerControls` 已存在 | 网络兼容层只做 Adapter/Projection；sequence/replay 仍需补设计 | confirmed/partial |
| tModLoader `v2026.07` `class_player.html` | `controlUseItem`、`mouseInterface`、`hostile`、`rulerGrid`/`rulerLine`、`selectedItem`、`ItemCheck` 等公开边界 | 仅作公开 API 交叉检查，不替代 Version4 私有 owner 证据 | partial |
| SS14 `InputMoverComponent`、`ActiveInputMoverComponent`、`SharedSpriteMovementSystem` | 输入时间/按键、活动缓存、移动表现按组件和系统分离 | 只参考组织粒度，不复制命名或 Terraria 语义 | existing-evidence |

## 3. 数据流与边界模型

```text
protocol/client input
        | decode + ownership + range/sequence validation
        v
validated PlayerInputCommand
        | one owner commit per tick
        +--> PlayerRawControlInputComponent / ReleaseRepeatState
        | explicit queries and action commands
        +--> item/tile/world/combat/inventory owners
        | immutable output only
        +--> selection, sync, builder overlay, network/UI projections
```

候选系统顺序为：输入 Adapter 解码与批次验证 -> C01 原始控制提交 -> C02 释放/重复窗口派生 -> C03/C04/C05 交互与 ItemUse 资格/意图 -> 移动、物品、Tile、Wiring、Combat 等外部 owner 提交 -> C07/C08 仪器状态 -> C09 definition/query -> C10 选择 commit/query -> C11/C12/C13 projection/query。该顺序只是 proposed scheduler contract，不能通过文件顺序实现；在 integration review 前不得宣布为最终运行时顺序。

## 4. 组件检查点总览

| 检查点 | proposed 边界 | 成员 | 角色 | 状态 |
|---|---|---:|---|---|
| C01 | `PlayerRawControlInputComponent` + `PlayerRawControlInputSystem` | 8 | Component/System/Command | completed design checkpoint |
| C02 | `PlayerReleaseAndRepeatStateComponent` + edge/timer system | 12 | Component/System | completed isolated core |
| C03 | interaction state、UI input projection、item reuse bridge、NPC pressure query 的窄边界 | 12 | Component/Query/Projection/Command | partial isolated checkpoints |
| C04 | `PlayerItemUseIntentComponent` + tile/entity interaction commands | 8 | Component/System/Command | partial isolated intent checkpoint |
| C05 | channel/action context、cost query、helper adapters | 8 | Component/Query/Adapter | partial isolated cancellation checkpoint |
| C06 | movement frame accumulator、world interaction/audio/combat seams | 5 | Component/Projection/Adapter/Query | planned |
| C07 | navigation/time instrument state | 7 | Component/System/Query | planned |
| C08 | detection/wiring instrument state | 8 | Component/System/Query | planned |
| C09 | builder toggle definition/catalog | 13 | Definition/Query | completed |
| C10 | selected item/radial/kite selection state | 17 | Component/System/Query | planned |
| C11 | input sync cache, channel cancellation and match request | 14 | Projection/Adapter/Command | planned |
| C12 | builder ruler overlay | 2 | Projection/Query | implementation partial; verification pending |
| C13 | item-space query and dash setting | 4 | Query/Adapter/Definition | planned |

## 5. 成员覆盖规则

组件章节按权威报告来源序号列出成员。成员在 P10 内只出现一次；源叶子归属保留在表中，即使一个叶子的成员因生命周期不同进入多个 proposed 边界。`source-inventory-confirmed` 只证明来源库存，不证明最终 owner。未列出的读者、写者、持久化和网络闭合均记入 `evidence-gap`。

## 6. C01 PlayerRawControlInput

- Proposed types: `PlayerRawControlInputComponent`, `PlayerRawControlInputSystem`, `PlayerInputCommand`; C01 隔离核心已实现，协议/调度接入仍为 proposed。
- 覆盖成员：`controlLeft`、`controlRight`、`controlUp`、`controlDown`、`controlJump`、`controlTorch`、`controlDash`、`controlDownHold`；均来自 `PlayerControlAndReleaseInput`。
- 来源库存：序号 809, 810, 811, 812, 813, 816, 817, 827；类型均为 `bool`；声明 `Player.cs:1219-1235,1268`。
- Owner 规则：协议层只产生 `PlayerInputCommand`；`PlayerRawControlInputSystem` 在整批校验通过后唯一写入组件。组件不引用网络包、时钟、日志或 UI。当前 Dome 的 `ControlInputComponent`/`PlayerInputApplySystem` 是现有候选 authority route；本设计不声称 C01 已迁移。
- 输入校验：玩家句柄、活动状态、facing 范围、批内重复玩家、来源 sequence/replay policy 必须在组件变更前处理。旧兼容协议对 `Up`、`UseTile`、`Dash` 等字段的映射缺口必须由 focused verifier 暴露，不能静默丢弃。
- 生命周期：玩家实体创建时初始化为全 false；每个模拟 tick 由有效命令提交；断线、重生和实体销毁清理。release edge 不在本组件内保存，交给 C02。
- 依赖方向：Protocol Adapter -> Command -> C01 System -> C01 Component -> Mobility/Item/Tile/Combat read-only Query。C01 不反向调用 `PlayerControlSystem` 或网络发送。
- 证据状态：Version4 声明、控制包写入和当前 NLTX 输入边界已确认；客户端 release/timing、持久化和 replay envelope 为 partial。
- focused verifier（planned, `verificationStatus: not-run`）：完整字段 round-trip；非法 facing/未知玩家/批内重复输入在清空旧状态前拒绝；空批只生成 release 前置状态；`Up/Down/Torch/Dash` 不被兼容 Adapter 丢失；唯一 writer 检查；同 sequence 重放和乱序处理遵循最终 server policy。
- 回滚：保留现有 `ControlInputComponent` 和协议 Projection 的旧读路径，撤销本批新增的输入 commit route；不得双写掩盖差异。

### C01 成员归属表

| 来源序号 | 权威叶子 | 成员 | C# 类型 | proposed 归属 | 证据 |
|---:|---|---|---|---|---|
| 809 | `PlayerControlAndReleaseInput` | `controlLeft` | `bool` | `PlayerRawControlInputComponent` | `Player.cs:1219` |
| 810 | `PlayerControlAndReleaseInput` | `controlRight` | `bool` | `PlayerRawControlInputComponent` | `Player.cs:1221` |
| 811 | `PlayerControlAndReleaseInput` | `controlUp` | `bool` | `PlayerRawControlInputComponent` | `Player.cs:1223` |
| 812 | `PlayerControlAndReleaseInput` | `controlDown` | `bool` | `PlayerRawControlInputComponent` | `Player.cs:1225` |
| 813 | `PlayerControlAndReleaseInput` | `controlJump` | `bool` | `PlayerRawControlInputComponent` | `Player.cs:1227` |
| 816 | `PlayerControlAndReleaseInput` | `controlTorch` | `bool` | `PlayerRawControlInputComponent` | `Player.cs:1235` |
| 817 | `PlayerControlAndReleaseInput` | `controlDash` | `bool` | `PlayerRawControlInputComponent` | `Player.cs:1241` |
| 827 | `PlayerControlAndReleaseInput` | `controlDownHold` | `bool` | `PlayerRawControlInputComponent` | `Player.cs:1268` |

### C01 实现检查点

- 实际修改文件：`src/Player/PlayerInputCommand.cs`、`src/Player/PlayerRawControlInputComponent.cs`、`src/Player/PlayerRawControlInputSystem.cs`、`src/Player/PlayerInputRejectionReason.cs`。
- 核心行为：`PlayerRawControlInputSystem.TryApply` 校验单条命令的非负玩家槽位、目标活动状态和左右 facing；`TryApplyBatch` 先校验所有命令、拒绝批内重复玩家、未知玩家和缺少目标组件，再一次性提交 8 个原始控制字段，因此失败批次不会清空或部分更新既有组件。
- 依赖影响：新增类型只依赖 `Terraria.Player` 既有 `LegacyPlayerSlot`/`DirectionKind`；未修改 `InputIntentComponent`、协议、网络、调度器或其他领域 owner，也未添加新的项目引用。
- 未验证项：`Sequence` 仅随命令携带，尚未实现 replay/乱序策略；未验证协议映射、唯一运行时 writer、断线/重生调度和 Version4 行为等价。

## 7. 跨分区交接与暂缓决策

- `team`、`aggro`、`nearbyActiveNPCs`、`hostile`、`lastCreatureHit`、`fireWalk`、`creativeGodMode`、`sign`、`ActuationRodLock` 和 `DashControl` 可能分别由 Combat、NPC、WorldInteraction、Wiring、Creative 或 Settings owner 读写；P10 只能提供交接候选，统一标记 `crossSubsystemOwner: integration-review`。
- `selectedItemState` 与现有 `dome/src/Terraria.Dome.Simulation/Inventory/Components/SelectedItemComponent.cs` 以及 P09 Inventory/Equipment 需要 integration review；P10 不重复宣布选择 authority。
- `PlayerInputSyncCache`、`SetMatchRequest`、`ChannelCancelKey` 是 projection/短期 payload，不能作为持久 Player component；输入 sequence、重放拒绝、断线重连和网络确认协议属于 Server/Protocol integration seam。
- Version4 `Player.SavePlayer`/`Serialize`/`Deserialize` 的本快照不能支撑 P10 成员持久化承诺；除明确证据外，所有输入、计时器、UI、cache 和 query value 均按 transient 处理。

### C02 实现检查点

- 实际修改文件：`src/Player/PlayerReleaseAndRepeatInput.cs`、`src/Player/PlayerReleaseAndRepeatStateComponent.cs`、`src/Player/PlayerReleaseAndRepeatSystem.cs`、`src/PlayerReleaseRepeatVerification/Program.cs`、`src/PlayerReleaseRepeatVerification/Terraria.PlayerReleaseRepeatVerification.csproj`。
- 核心行为：`PlayerReleaseAndRepeatSystem.CreateResetState` 将 10 个释放/悬停状态和两个方向计时器初始化为已释放、无悬停、计时器为 0；`Advance` 在 reset 时整体恢复该状态，在正常 tick 中根据当前控制快照计算 jump/up/left/right/down/dash 释放状态，保留外部已验证的 item/tile release 事实，传递持续悬停意图，并集中维护左右方向计时器。
- 计时器规则：方向松开时设置为 `DirectionRepeatWindowTicks - 1`；按住时先将既有值限制到 `0..7`，再递减至 0 或回到 7。`DirectionRepeatWindowTicks` 为 7；focused verifier 已覆盖从 0、1、释放后的 6、超范围 timer 输入到下一状态的确定性结果。
- 依赖影响：C02 只依赖稳定的 `PlayerReleaseAndRepeatInput` 值类型和 `System.Math.Clamp`；未修改 C01、InputIntentComponent、Item/Tile/协议/网络/调度器或其他领域 owner，没有新增项目引用。
- 验证：先以已存在生产核心构建 verifier，首轮运行退出码 `1`，原因是 verifier 对第二帧右方向 timer 的期望错误；修正 verifier 后通过 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\PlayerReleaseRepeatVerification\Terraria.PlayerReleaseRepeatVerification.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`，退出码 `0`、警告 `0`、错误 `0`，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerReleaseRepeatVerification\Debug\net10.0\Terraria.PlayerReleaseRepeatVerification.dll`，同时生成 `D:\TRbackup\NLTX\Build\bin\Terraria.Player\Debug\net10.0\Terraria.Player.dll`；随后通过 wrapper 运行 `run --project .\src\PlayerReleaseRepeatVerification\Terraria.PlayerReleaseRepeatVerification.csproj --no-build --no-restore`，退出码 `0`，输出 `PASS: release and repeat state preserves Version4 isolated edge and timer semantics`。focused checkpoint `verificationStatus: verified-focused`。
- 未验证项：当前输入结构由调用方提供，尚未证明来自 C01 的已验证 tick 快照；`releaseUseItem`/`releaseUseTile` 尚未由 Item/Tile owner 消费；尚未验证空批、非法批次不清空旧状态、spawn/teleport/disconnect reset、重复包、左右同时按、释放边沿单 tick 单次消费和 Version4 完整行为等价。

### C09 实现检查点

- 实际修改文件：`src/Player/PlayerBuilderInteractionCatalog.cs`、`src/Player/BuilderInteractionDefinitionQuery.cs`、`src/PlayerBuilderInteractionVerification/Program.cs`、`src/PlayerBuilderInteractionVerification/Terraria.PlayerBuilderInteractionVerification.csproj`。
- 成员覆盖：C09 的 `RulerLine`、`RulerGrid`、`AutoActuate`、`AutoPaint`、`WireVisibility_Red`、`WireVisibility_Green`、`WireVisibility_Blue`、`WireVisibility_Yellow`、`HideAllWires`、`WireVisibility_Actuators`、`BlockSwap`、`TorchBiome` 和 `Count` 13 个成员全部由 `PlayerBuilderInteractionCatalog` 对应；值保持 Version4 的 `0..11` 与 `Count=12`。
- 核心行为：catalog 只暴露静态定义；`BuilderInteractionDefinitionQuery.IsKnownToggleId` 只接受 `0 <= toggleId < Count`，不写入玩家或 World，不注册实体组件，也不携带 `builderAccStatus`。
- 依赖影响：生产代码只依赖 `Terraria.Player` 项目内的基础语言类型；focused verifier 通过项目引用检查常量形状、精确值和 ID 范围。没有修改 Builder/Wiring consumer、协议、网络、存档或 P09 状态 owner。
- 未验证项：尚未验证真实 Builder/Wiring consumer 的只读使用、未知/重复 toggle command 的最终拒绝路径、内容版本兼容策略、`builderAccStatus` 唯一 writer 和 C12 overlay 单向接入。
- 验证：先运行 focused verifier 的红测，退出码 `1`，原因是生产 catalog 缺失；实现后串行构建 `Terraria.PlayerBuilderInteractionVerification.csproj`，退出码 `0`、警告 `0`、错误 `0`；产物为 `Build/bin/Terraria.PlayerBuilderInteractionVerification/Debug/net10.0/Terraria.PlayerBuilderInteractionVerification.dll`，同时生成 `Terraria.Player.dll`；随后通过 wrapper 运行 `run --project .\src\PlayerBuilderInteractionVerification\Terraria.PlayerBuilderInteractionVerification.csproj --no-build --no-restore`，退出码 `0`，输出 `PASS: builder interaction catalog definitions and pure ID query`。
- 回滚：删除本检查点新增 catalog、query 和 focused verifier 即可恢复原有定义查找；不删除或改写其他会话的 builder 状态、Wiring owner 或文档。

## 7. C02 PlayerReleaseAndRepeatState

- Proposed types: `PlayerReleaseAndRepeatStateComponent`, `PlayerReleaseAndRepeatSystem`; isolated core implemented, runtime integration remains `status: proposed`。
- 覆盖成员：`releaseJump`、`releaseUp`、`releaseUseItem`、`releaseUseTile`、`releaseLeft`、`releaseRight`、`releaseDown`、`releaseDash`、`tryKeepingHoveringDown`、`tryKeepingHoveringUp`、`leftTimer`、`rightTimer`；均来自 `PlayerControlAndReleaseInput`。
- 来源库存：序号 818, 819, 820, 821, 822, 823, 824, 825, 831, 832, 834, 835；类型为 `bool`（前 10 项）和 `int`（两个 timer）；声明 `Player.cs:1245-1264,1276-1290`。
- 语义边界：C01 保存本 tick 的按住事实；C02 保存由上一 tick 与当前 tick 算出的释放/重复窗口状态。外部 packet 不得直接设置 release flags，Item/Tile/Movement systems 只能消费查询或显式 event。
- Owner 规则：`PlayerReleaseAndRepeatSystem` 是 proposed 唯一 writer；它接收上一帧快照、当前已验证控制快照和显式 tick，不直接读取系统时钟。`leftTimer`/`rightTimer` 的递减、饱和与 reset 规则必须集中在此 system。
- 生命周期：实体创建、spawn、teleport、disconnect 和空输入批次都必须定义初始化/清理；同一 tick 内重复控制包在 Server 层拒绝或合并后只能产生一次边沿。`tryKeepingHovering*` 是持续意图，不等同于 `release*`。
- 交接：`releaseUseItem`/`releaseUseTile` 通过 `ItemUseReleaseEvent`/`TileUseReleaseEvent` 候选 seam 交给 C04 和 Tile/Wiring owner；左右 timer 交给移动/绳索/平台逻辑的只读 Query。共享 owner 尚未裁决。
- 证据状态：Version4 `Player.Update`、pulley/移动条件和 MessageBuffer 控制同步路径已确认；客户端输入采样粒度、重复包序号和完整 Reset/Spawn 调度仍 partial。
- focused verifier（`verificationStatus: verified-focused`）：已验证 reset 默认值、控制到 release 的派生、Item/Tile release 外部事实保留、hover 持续意图、左右方向重复窗口从 0/1/释放后状态的递减、timer clamp，以及 reset 丢弃当前输入。空批/非法批次保护、spawn/teleport/disconnect 调度、重复包、release edge 消费和真实运行时行为等价仍未验证。
- 回滚：保留现有 `PlayerInputEdges`/`ControlInputComponent` 兼容状态（如调用方仍使用）；新 system 只可作为只读计算，验证失败时撤销新 reader route，不保留双 writer。

### C02 成员归属表

| 来源序号 | 权威叶子 | 成员 | C# 类型 | proposed 归属 | 证据 |
|---:|---|---|---|---|---|
| 818 | `PlayerControlAndReleaseInput` | `releaseJump` | `bool` | `PlayerReleaseAndRepeatStateComponent` | `Player.cs:1245` |
| 819 | `PlayerControlAndReleaseInput` | `releaseUp` | `bool` | `PlayerReleaseAndRepeatStateComponent` | `Player.cs:1247` |
| 820 | `PlayerControlAndReleaseInput` | `releaseUseItem` | `bool` | `PlayerReleaseAndRepeatStateComponent` | `Player.cs:1249` |
| 821 | `PlayerControlAndReleaseInput` | `releaseUseTile` | `bool` | `PlayerReleaseAndRepeatStateComponent` | `Player.cs:1251` |
| 822 | `PlayerControlAndReleaseInput` | `releaseLeft` | `bool` | `PlayerReleaseAndRepeatStateComponent` | `Player.cs:1254` |
| 823 | `PlayerControlAndReleaseInput` | `releaseRight` | `bool` | `PlayerReleaseAndRepeatStateComponent` | `Player.cs:1256` |
| 824 | `PlayerControlAndReleaseInput` | `releaseDown` | `bool` | `PlayerReleaseAndRepeatStateComponent` | `Player.cs:1260` |
| 825 | `PlayerControlAndReleaseInput` | `releaseDash` | `bool` | `PlayerReleaseAndRepeatStateComponent` | `Player.cs:1264` |
| 831 | `PlayerControlAndReleaseInput` | `tryKeepingHoveringDown` | `bool` | `PlayerReleaseAndRepeatStateComponent` | `Player.cs:1280` |
| 832 | `PlayerControlAndReleaseInput` | `tryKeepingHoveringUp` | `bool` | `PlayerReleaseAndRepeatStateComponent` | `Player.cs:1282` |
| 834 | `PlayerControlAndReleaseInput` | `leftTimer` | `int` | `PlayerReleaseAndRepeatStateComponent` | `Player.cs:1288` |
| 835 | `PlayerControlAndReleaseInput` | `rightTimer` | `int` | `PlayerReleaseAndRepeatStateComponent` | `Player.cs:1290` |

## 8. C03 PlayerInteractionRuntimeState

- Proposed boundaries: `PlayerInteractionUiStateComponent`, `PlayerItemReuseStateComponent`, `PlayerNpcPressureQuery`, `PlayerTeamAndSignAdapter`, `PlayerInteractionRuntimeSystem`; runtime owners remain `status: proposed`, while the pure NPC pressure Query core is saved as an implementation checkpoint。
- 该检查点不建立一个承载全部成员的 `PlayerInteractionComponent`。成员按定义、ItemUse 时间、NPC 聚合、网络/世界关系和 UI 门控拆分。
- `nameLen`（序号 694）是静态定义；`team`（691）和 `sign`（696）分别与网络队伍关系、World sign/interaction 交接，`crossSubsystemOwner: integration-review`。
- `reuseDelay`（697）和 `pendingItemReuse`（705）属于 ItemUse 时间/重用状态；`aggro`（698）属于 NPC targeting/combat pressure；`nearbyActiveNPCs`（699）是 Main/NPC 聚合结果，不由玩家交互 system 双写。
- `creativeInterface`（700）、`mouseInterface`（701）、`lastMouseInterface`（702）、`noThrow`（703）和 `changeItem`（704）是界面/交互门控或选择变更请求；UI 只能产生 Command/Projection，不能直接写权威 Player。`changeItem` 的一次性请求必须与 C10 selection commit 明确消费边界。
- 证据：`Player.cs:978-1009` 声明；`Player.cs:3475,19593,23439-23562` 支持 reuse/reset；`Player.cs:7682-8297,10375,15368-15482` 支持 aggro reset/调整；`Main.cs:11462` 和 `NPC.cs:212-598,5792-5796,64464-64468` 支持 nearby 聚合；`Main.cs:12401`、`Player.cs:15105,17974,21802` 支持 UI/生命周期读写；`MessageBuffer.cs:614,1817-1828,1866-1872` 支持 team/sign 网络/世界交接。
- 生命周期：定义随程序集存在；Item reuse 每 tick 更新并在 spawn/ItemCheck 清理；NPC pressure 在 NPC/World tick 清零、累加、读取；UI gate 在 client input phase reset/commit；team/sign 不由本 checkpoint 声明持久 owner。
- focused verifier（planned, `verificationStatus: not-run`）：C03 各 proposed boundary 唯一 writer；pending reuse 只消费一次；nearby 聚合清零/累加顺序可重复；UI flag 不越过 Command seam；changeItem 与 selection commit 不双写；非法 team/sign network input 在 adapter 拒绝或转为显式 command。
- 回滚：保留现有 ItemUse、NPC targeting、UI 和 network compatibility reader；失败时按窄边界撤回新 writer/reader，不合并回巨型组件。

### C03 UI/交互门控核心实现检查点

- 实际修改文件：`src/Player/PlayerInteractionUiStateInput.cs`、`src/Player/PlayerInteractionUiStateComponent.cs`、`src/Player/PlayerInteractionUiStateSystem.cs`、`src/PlayerInteractionUiStateVerification/Program.cs`、`src/PlayerInteractionUiStateVerification/Terraria.PlayerInteractionUiStateVerification.csproj`。
- 核心行为：`PlayerInteractionUiStateSystem.CreateResetState` 将 `creativeInterface`、`mouseInterface`、`lastMouseInterface` 和 `noThrow` 清零；`Advance` 在非 reset 帧逐项复制显式输入，在 reset 帧丢弃输入并恢复默认值。
- 依赖影响：生产核心只依赖本地值类型；未接入 UI、Main、Projectile、ItemUse、Selection、网络、持久化或调度器，未修改现有 `PlayerUseComponent`。
- 验证：先运行 focused verifier 的红测构建并运行，构建退出码 `0`，运行退出码 `1`，原因是生产类型 `Terraria.Player.PlayerInteractionUiStateComponent` 缺失；补入最小实现后，通过 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`，退出码 `0`、警告 `0`、错误 `0`，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerInteractionUiStateVerification\Debug\net10.0\Terraria.PlayerInteractionUiStateVerification.dll`；随后通过 wrapper 运行 `run --project .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj --no-build --no-restore`，退出码 `0`，输出 `PASS: player interaction UI state advances and resets explicitly`。
- 未验证项：Version4 `Main`/Projectile/UI 的真实 writer/reader 接入、spawn/death/disconnect 生命周期、`noThrow` 计时语义、C10 `changeItem` handoff、网络/持久化边界和行为等价仍未验证；本检查点只验证显式快照核心。

### C03 `nameLen` 定义/范围 Query 实现检查点

- 实际修改文件：`src/Player/PlayerNameDefinition.cs`、`src/Player/PlayerNameLengthQuery.cs`、`src/PlayerInteractionUiStateVerification/Program.cs`。
- 核心行为：`PlayerNameDefinition.MaximumLength` 保留 Version4 `Player.nameLen = 20`；`PlayerNameLengthQuery.IsWithinLimit` 只判断非负长度是否不超过 20，不处理空名策略、字符串规范化或网络拒绝。
- 依赖影响：纯定义和 Query 无实体状态、无 I/O、无网络/持久化写入；Version4 `MessageBuffer` 的空名、重复名、权限和拒绝消息仍由协议 owner 处理。
- 验证：红测 verifier 构建退出码 `0`，运行退出码 `1`，原因是 `PlayerNameDefinition` 生产类型缺失；实现后通过 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`，退出码 `0`、警告 `0`、错误 `0`；随后 wrapper 运行 `run --project .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj --no-build --no-restore`，退出码 `0`，输出 `PASS: player interaction UI state advances and resets explicitly`。
- 未验证项：名称字符串编码、空名/重复名策略、协议字段和持久化边界仍未验证；`nameLen` 仅按 Version4 声明与长度比较实现。

### C03 ItemUse 复用资格 Query 实现检查点

- 实际修改文件：`src/Player/PlayerItemReuseSnapshot.cs`、`src/Player/PlayerItemReuseQuery.cs`、`src/PlayerInteractionUiStateVerification/Program.cs`。
- 核心行为：`PlayerItemReuseQuery.IsUsingOrReusingItem` 对显式快照判断 `itemAnimation > 0 || reuseDelay > 0 || channel || pendingItemReuse`，对应 Version4 `Player.UsingOrReusingItem` 的纯计算语义；不修改 `PlayerUseComponent` 或 `PlayerItemUseState`。
- 依赖影响：只读值类型和 Query 无实体写入、无 Item/Projectile 调用、无网络/持久化 I/O；现有 `PlayerUseComponent` 与 `PlayerItemUseState` 继续作为待集成审查的兼容候选，不新增同义 authority。
- 验证：红测 verifier 构建退出码 `0`，运行退出码 `1`，原因是 `Terraria.Player.PlayerItemReuseSnapshot` 生产类型缺失；实现后通过 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`，退出码 `0`、警告 `0`、错误 `0`，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerInteractionUiStateVerification\Debug\net10.0\Terraria.PlayerInteractionUiStateVerification.dll`；随后通过 wrapper 运行 `run --project .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj --no-build --no-restore`，退出码 `0`，输出 `PASS: player interaction UI state advances and resets explicitly`。
- 未验证项：`reuseDelay`/`pendingItemReuse` 的真实写者、ItemCheck 清理、spawn/teleport/disconnect 生命周期、重复消费和行为等价仍未验证；只读 Query 不代表 C03 ItemUse 状态迁移完成。

#### C03 ItemUse 重用状态组件实现检查点

- 实际修改文件：`src/Player/PlayerItemReuseStateComponent.cs`。
- 核心行为：`PlayerItemReuseStateComponent` 只保存 `reuseDelay` 的剩余 tick 数和一次性
  `pendingItemReuse` 事实；`struct` 默认值为 `ReuseDelayRemainingTicks = 0`、
  `PendingItemReuse = false`。组件不计算复用资格、不消费 pending 标记，也不执行 ItemCheck
  或调度行为。
- 依赖影响：组件只依赖 `Terraria.Player` 项目内的值类型边界；没有修改既有
  `PlayerUseComponent`/`PlayerItemUseState`，没有创建第二个 ItemUse writer，也没有新增
  Item、Projectile、网络或持久化引用。
- 未验证项：ItemCheck 的真实唯一 writer、pending 标记只消费一次、tick 倒计时、spawn/
  teleport/death/disconnect 清理和旧路径行为等价仍未验证；本检查点只证明组件源码和字段
  所有权边界已经落地。

### C03 NPC 压力贡献 Query 实现检查点

- 实际修改文件：`src/Player/PlayerNpcPressureContributionInput.cs`、`src/Player/PlayerNpcPressureContribution.cs`、`src/Player/PlayerNpcPressureQuery.cs`、`src/PlayerInteractionUiStateVerification/Program.cs`。
- 核心行为：`PlayerNpcPressureQuery.Evaluate` 只对显式 NPC/player snapshot 计算贡献资格；要求 NPC active、玩家处于 active range、`LifeMaximum > 0`、`ReleaseOwner == 255`，排除类型 `25/30/33`，并在 slime-rain NPC 场景使用 `0.65f` slot multiplier。`SumContributions` 只累加 Query 返回的 slot weight，不写 `nearbyActiveNPCs`、NPC 或 World 状态。
- 依赖影响：生产类型仅依赖 `System` 和显式只读输入；没有接入 `Main`/`NPC` 全局状态、aggro targeting、NPC registry、调度器、网络或持久化，也没有声明 NPC/World 的唯一 writer。
- 验证：通过 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`，退出码 `0`、警告 `0`、错误 `0`；产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerInteractionUiStateVerification\Debug\net10.0\Terraria.PlayerInteractionUiStateVerification.dll`。随后通过 wrapper 运行 `run --project .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj --no-build --no-restore`，退出码 `0`，输出 `PASS: player interaction state, reuse, name, and NPC pressure queries are valid`。
- 未验证项：Version4 `Main`/`NPC` 的真实清零、累加和读取顺序，active-range 的实际判定输入，slot cost 的真实来源，NPC 类型注册范围，`aggro` targeting/combat owner，跨分区唯一 writer、生命周期和行为等价仍未验证；本检查点只证明纯贡献计算。

### C03 成员归属表

| 来源序号 | 权威叶子 | 成员 | C# 类型 | proposed 归属 | 证据/备注 |
|---:|---|---|---|---|---|
| 691 | `PlayerInteractionInputState` | `team` | `int` | `PlayerTeamAndSignAdapter`; `crossSubsystemOwner: integration-review` | `Player.cs:978`; `MessageBuffer.cs:614,1817` |
| 694 | `PlayerInteractionInputState` | `nameLen` | `int` | `PlayerNameDefinition` / `Query` | `Player.cs:984`; static definition |
| 696 | `PlayerInteractionInputState` | `sign` | `int` | `PlayerSignInteractionAdapter`; `crossSubsystemOwner: integration-review` | `Player.cs:988`; sign/world paths |
| 697 | `PlayerInteractionInputState` | `reuseDelay` | `int` | `PlayerItemReuseStateComponent` | `Player.cs:991,3475,19593` |
| 698 | `PlayerInteractionInputState` | `aggro` | `int` | `PlayerNpcPressureQuery`; `crossSubsystemOwner: integration-review` | `Player.cs:993`; NPC reads/Player adjustments |
| 699 | `PlayerInteractionInputState` | `nearbyActiveNPCs` | `float` | `PlayerNpcPressureQuery`; `crossSubsystemOwner: integration-review` | `Main.cs:11462`; `NPC.cs:64464-64468` |
| 700 | `PlayerInteractionInputState` | `creativeInterface` | `bool` | `PlayerInteractionUiStateComponent` | `Player.cs:997,21802` |
| 701 | `PlayerInteractionInputState` | `mouseInterface` | `bool` | `PlayerInteractionUiStateComponent` | `Player.cs:999,15105,17974` |
| 702 | `PlayerInteractionInputState` | `lastMouseInterface` | `bool` | `PlayerInteractionUiStateComponent` | `Player.cs:1001`; UI gate read |
| 703 | `PlayerInteractionInputState` | `noThrow` | `int` | `PlayerInteractionUiStateComponent` / interaction gate | `Player.cs:1003`; `Main.cs:12401` |
| 704 | `PlayerInteractionInputState` | `changeItem` | `int` | `PlayerSelectionChangeCommand` / `PlayerInteractionRuntimeSystem` | `Player.cs:1005,10029`; C10 handoff |
| 705 | `PlayerInteractionInputState` | `pendingItemReuse` | `bool` | `PlayerItemReuseStateComponent` | `Player.cs:1007,23439-23562` |

## 9. C04 PlayerItemUseIntent

- Proposed types: `PlayerItemUseIntentComponent`, `PlayerTileInteractionAttemptState`, `PlayerItemUseResultProjection`, `PlayerItemUseModeCommand`; intent core is saved as an isolated implementation checkpoint, while the remaining runtime boundaries stay `status: proposed`。
- 覆盖成员：`controlUseItem`、`controlUseTile`、`tileInteractAttempted`、`isOperatingAnotherEntity`、`lastItemUseAttemptSuccess`、`autoReuseAllWeapons`、`altFunctionUse`、`delayUseItem`；来源叶子 `PlayerItemUseAndChannelIntent`，序号 814, 815, 826, 828, 829, 830, 833, 836。
- 语义拆分：`controlUseItem`/`controlUseTile` 是已验证的持续使用意图；`altFunctionUse` 是短期 action mode；`delayUseItem` 是待执行的一次性延迟；`tileInteractAttempted` 是本 tick 交互尝试 marker；`isOperatingAnotherEntity` 是交互锁/占用事实；`lastItemUseAttemptSuccess` 是结果反馈；`autoReuseAllWeapons` 是 Item/Settings policy 输入。它们不应成为一个同时承载输入、结果和锁的组件。
- Owner 规则：Protocol Adapter 只产生 use/tile intent command；ItemUseSystem 读取 C01/C02/C10/C13 的只读事实，经过 Item/Tile/Wiring/Entity owner 的资格查询后提交 action command。成功结果由 ItemUseResultCommit/Projection 写入或发布；UI、网络包和 Query 不得直接写结果字段。
- Version4 证据：`MessageBuffer.cs:670,726,728-731` 直接解码/写入 control/use 相关位；`Player.cs:23435-23564` 的 `ItemCheck` 清理 pending、检查 selection/release、处理 alt mode、retry/reuse 和 release；`Player.cs:15105,17690,17969` 读取 UI/tile interaction；`Player.cs:25700-25750` 处理 auto reuse 和 use/channel 交接。`ItemCheck` 既消费输入又写反馈，迁移时必须显式切开阶段。
- 当前 NLTX 证据：已有 `ControlInputComponent`、`PlayerControlSystem`、`UseItemCommand`、`TileManipulationIntent`、`TileInteractionValidator` 和 `DomeNetworkUpdateBridge`；但 `DomeServer` 映射没有闭合 P10 的所有 use/tile/alt/reuse 语义，不能宣布 C04 已迁移。
- 生命周期：use/tile intent 每 tick 更新；attempt marker 在 action phase 清零；delay 只允许被明确 owner 消费一次；success 只表示本次尝试结果，不跨 tick 无限制保留；entity lock 在 claim/release 或拒绝路径清理。持久化不在本组件范围。
- focused verifier（planned, `verificationStatus: not-run`）：use/tile intent round-trip；release gate 与 C02 一致；altFunctionUse 只在合法 action window 消费并回到默认；delay 不重复执行；tile/entity lock 在成功、拒绝、断线和死亡路径释放；success 不能被客户端伪造；auto-reuse 不绕过 cooldown/permission；TileManipulation 与 UseItem command 不重复应用同一动作。
- 回滚：继续由现有 `ControlInputComponent`/`UseItemCommand`/`TileInteractionValidator` 提供兼容路径；撤销新 ItemUse reader/commit route 时保留旧行为，不同时写新旧结果。

### C04 成员归属表

| 来源序号 | 权威叶子 | 成员 | C# 类型 | proposed 归属 | 证据/备注 |
|---:|---|---|---|---|---|
| 814 | `PlayerItemUseAndChannelIntent` | `controlUseItem` | `bool` | `PlayerItemUseIntentComponent` | `Player.cs:1229,23435` |
| 815 | `PlayerItemUseAndChannelIntent` | `controlUseTile` | `bool` | `PlayerItemUseIntentComponent` | `Player.cs:1231,23435` |
| 826 | `PlayerItemUseAndChannelIntent` | `tileInteractAttempted` | `bool` | `PlayerTileInteractionAttemptState` | `Player.cs:1268,17969` |
| 828 | `PlayerItemUseAndChannelIntent` | `isOperatingAnotherEntity` | `bool` | `PlayerEntityInteractionLockState`; `crossSubsystemOwner: integration-review` | `Player.cs:1272,MessageBuffer.cs:728` |
| 829 | `PlayerItemUseAndChannelIntent` | `lastItemUseAttemptSuccess` | `bool` | `PlayerItemUseResultProjection` | `Player.cs:1274,23435,MessageBuffer.cs:731` |
| 830 | `PlayerItemUseAndChannelIntent` | `autoReuseAllWeapons` | `bool` | `PlayerAutoReusePolicyAdapter`; `crossSubsystemOwner: integration-review` | `Player.cs:1276,25700-25750,MessageBuffer.cs:726` |
| 833 | `PlayerItemUseAndChannelIntent` | `altFunctionUse` | `int` | `PlayerItemUseModeCommand` / transient context | `Player.cs:1284,23471-23506` |
| 836 | `PlayerItemUseAndChannelIntent` | `delayUseItem` | `bool` | `PlayerItemUseDeferredAction` | `Player.cs:1292,15107` |

### C04 `PlayerItemUseIntent` core implementation checkpoint

- 实际修改文件：`src/Player/PlayerItemUseIntentInput.cs`、`src/Player/PlayerItemUseIntentComponent.cs`、`src/Player/PlayerItemUseIntentSystem.cs`、`src/PlayerInteractionUiStateVerification/Program.cs`。
- 核心行为：`PlayerItemUseIntentSystem.CreateResetState` 将 `ControlUseItem` 和 `ControlUseTile` 清零；`Advance` 在非 reset tick 逐项复制显式 intent，在 reset tick 丢弃输入并恢复默认值。该核心不消费 release edge，不写 Item/Tile/Entity 状态，也不发布成功结果。
- 依赖影响：生产代码只依赖本地值类型；没有修改既有 `InputIntentComponent`、`PlayerUseComponent`、`PlayerItemUseState` 或 Dome protocol/command writer，避免形成第二条运行时 authority route。
- 验证：red verifier 通过 wrapper 构建退出码 `1`，原因是 6 个待实现 C04 类型/成员缺失；实现后通过 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`，退出码 `0`、警告 `0`、错误 `0`；产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerInteractionUiStateVerification\Debug\net10.0\Terraria.PlayerInteractionUiStateVerification.dll`。随后通过 wrapper 运行 `run --project .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj --no-build --no-restore`，退出码 `0`，输出 `PASS: player interaction state, reuse, name, and NPC pressure queries are valid`。
- 未验证项：真实 client/protocol mapping、C02 release gate、Item/Tile/Wiring/Entity owner、attempt marker、entity lock、result projection、auto-reuse policy、alternate mode、deferred action、生命周期、唯一 writer 和 Version4 行为等价仍未验证；本检查点只证明显式 intent 的 reset/advance 计算。

#### C04 `PlayerEntityInteractionLockStateComponent` implementation checkpoint

- 实际修改文件：`src/Player/PlayerEntityInteractionLockStateComponent.cs`。
- 覆盖成员：`isOperatingAnotherEntity`（P10-828），类型为 `bool`，默认值为 `false`。
- 核心行为：组件只保存实体交互占用事实，不执行 claim/release、ItemCheck、Tile/Wiring/Entity 行为，也不负责成功/拒绝路径或生命周期清理。
- 依赖影响：仅新增 `Terraria.Player` 内的独立值组件；不修改 P09 `PlayerInteractionLockStateComponent` 的 `LockTileInteractionsTimer`，不新增第二个 ItemUse authority，也未修改 System、Query、Command、Adapter、Projection、网络或持久化 owner。
- 未验证项：唯一 writer、成功/拒绝路径、断线/死亡清理、调度顺序、生命周期和 Version4 行为等价仍未验证；跨子系统 owner 保持 `integration-review`。

## 10. C05 PlayerChannelAndActionContext

- Proposed boundaries: `PlayerChannelStateComponent`, `PlayerItemCostQuery`, `PlayerTagEffectAdapter`, `PlayerIntentionAdapter`, `PlayerChannelCancellationAdapter`, `PlayerRabbitOrderAdapter`, `PlayerCreativeModeAdapter`; the channel cancellation expectation Adapter core is saved, while channel state and other runtime boundaries remain `status: proposed`。
- 覆盖成员：`manaCost`、`fireWalk`、`channel`、`TagEffectState`、`IntentionGuesser`、`_channelShotCache`、`rabbitOrderFrame`、`creativeGodMode`；来源叶子 `PlayerItemUseAndChannelIntent`，序号 844-849, 851-852。
- 语义拆分：`manaCost` 是装备/能力派生的使用成本结果；`fireWalk` 是环境能力事实；`channel` 是跨 Item/Projectile 的持续动作状态；`TagEffectState` 是 Combat/Projectile 关联状态；`IntentionGuesser` 是有更新行为的意图 helper；`_channelShotCache` 是 projectile cancellation expectation；`rabbitOrderFrame` 是短期交互 helper；`creativeGodMode` 是权限/世界规则状态。它们没有共同生命周期，不能进入单一 PlayerGameplay component。
- Owner 规则：`PlayerChannelStateComponent` 只保存经 ItemUse/Projectile integration review 确认的 channel committed fact；`manaCost` 通过纯 Query 计算；Tag、intention、rabbit 和 creative 通过 Adapter/Command/Projection 交接。`ChannelCancelKey` 不保存完整 Projectile 引用，只保存可验证的期望键；网络层不能直接切换 `channel`。
- Version4 证据：`Player.cs:1314-1331` 声明；`Player.cs:10333,10353,10753,14979-14980,20966,21823` 支持 reset/update 生命周期；`Player.cs:23442,23544,25762-25798` 支持 ItemCheck/StartChanneling/TryCancelChannel；`Projectile.cs:11876,12460,21095,37976,45128` 支持 Tag/channel 的跨域读写；`Player.cs:26575-26577` 支持 helper 初始化。
- 跨分区 owner：`fireWalk` 由 P08/P09 能力或环境系统共同影响，`creativeGodMode` 与 Creative/World 权限共同影响，`channel` 与 Projectile owner 共同影响，`TagEffectState` 与 Combat/Projectile owner 共同影响；全部标记 `crossSubsystemOwner: integration-review`。
- 生命周期：mana cost 每次使用前从显式 facts 计算；fireWalk/creative flags 按 ResetEffects/permission phase 重建；channel 在 action start/stop、CCed、ItemCheck 和 projectile cancellation 清理；helper 按实体创建/update/spawn reset；未确认持久化和网络 schema 前均 transient。
- focused verifier（planned, `verificationStatus: not-run`）：channel start/update/cancel 只匹配同一 projectile expectation；Item release、CCed、noItems、death 和 disconnect 清理；mana cost clamp/重复计算确定性；Tag state 不被 Player input writer 双写；IntentionGuesser/Rabbit helper 无隐藏时钟或网络副作用；creative permission 不能由客户端输入伪造。
- 回滚：保留现有 ItemUse/Projectile/Creative compatibility adapters；失败时撤销 C05 新 channel writer 或 helper bridge，不把原 helper 复制为持久 ECS 状态。

### C05 成员归属表

| 来源序号 | 权威叶子 | 成员 | C# 类型 | proposed 归属 | 证据/备注 |
|---:|---|---|---|---|---|
| 844 | `PlayerItemUseAndChannelIntent` | `manaCost` | `float` | `PlayerItemCostQuery` | `Player.cs:1314,6058-9228,25506-25531` |
| 845 | `PlayerItemUseAndChannelIntent` | `fireWalk` | `bool` | `PlayerFireWalkCapability`; `crossSubsystemOwner: integration-review` | `Player.cs:1316,10353,4319-4352` |
| 846 | `PlayerItemUseAndChannelIntent` | `channel` | `bool` | `PlayerChannelStateComponent`; `crossSubsystemOwner: integration-review` | `Player.cs:1318,25762-25798` |
| 847 | `PlayerItemUseAndChannelIntent` | `TagEffectState` | `Terraria.GameContent.Items.TagEffectState` | `PlayerTagEffectAdapter`; `crossSubsystemOwner: integration-review` | `Player.cs:1321`; `Projectile.cs:11876,12460` |
| 848 | `PlayerItemUseAndChannelIntent` | `IntentionGuesser` | `Terraria.DataStructures.PlayerIntentionGuesser` | `PlayerIntentionAdapter` | `Player.cs:1323,14980,26577` |
| 849 | `PlayerItemUseAndChannelIntent` | `_channelShotCache` | `Terraria.Player.ChannelCancelKey` | `PlayerChannelCancellationAdapter` | `Player.cs:1325,25767-25798` |
| 851 | `PlayerItemUseAndChannelIntent` | `rabbitOrderFrame` | `Terraria.Player.RabbitOrderFrameHelper` | `PlayerRabbitOrderAdapter` | `Player.cs:1329,20966,21823` |
| 852 | `PlayerItemUseAndChannelIntent` | `creativeGodMode` | `bool` | `PlayerCreativeModeAdapter`; `crossSubsystemOwner: integration-review` | `Player.cs:1331,14758,22216,22576` |

### C05 `PlayerChannelCancellation` core implementation checkpoint

- 实际修改文件：`src/Player/PlayerChannelCancellationExpectation.cs`、`src/Player/PlayerChannelCancellationProjectileSnapshot.cs`、`src/Player/PlayerChannelCancellationAdapter.cs`、`src/PlayerInteractionUiStateVerification/Program.cs`。
- 核心行为：`PlayerChannelCancellationAdapter.Matches` 复刻 Version4 `ChannelCancelKey.Matches`：`AiStyle == 99 && Ai0 == -3f` 的特殊 projectile 直接匹配；否则要求 projectile type 和 index 同时等于 expectation。`Track` 只在 projectile type 与 expected type 相同的时候更新 expected index，并以新值返回，不修改输入对象或 `channel`。
- 依赖影响：核心只使用显式 expectation/projectile snapshot，不保存 Projectile 引用、不访问全局 projectile registry、不切换 `channel`、不执行网络/持久化或日志副作用；现有 `ProjectileIdentityComponent` 和 Item/Projectile systems 未被修改。
- 验证：red verifier 通过 wrapper 构建退出码 `1`，原因是 13 个待实现 C05 类型/成员缺失；实现后通过 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`，退出码 `0`、警告 `0`、错误 `0`；产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerInteractionUiStateVerification\Debug\net10.0\Terraria.PlayerInteractionUiStateVerification.dll`。随后通过 wrapper 运行 `run --project .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj --no-build --no-restore`，退出码 `0`，输出 `PASS: player interaction state, reuse, name, and NPC pressure queries are valid`。
- 未验证项：真实 Projectile entity identity/slot registry、Item/Projectile channel writer、channel start/update/cancel 调度、death/disconnect/CCed/noItems 清理、网络映射、`manaCost` 计算和 Version4 行为等价仍未验证；本检查点只证明显式 expectation 的纯匹配和追踪。

## 11. C06 PlayerMovementAndWorldInteraction

- Proposed boundaries: `PlayerInstantMovementAccumulatorComponent`, `PlayerHostilityAdapter`, `PlayerCreatureHitProjection`, `PlayerActuationRodLockState`, `PlayerStepSoundProjection`; all `status: proposed`。
- 覆盖成员：`hostile`、`hermesStepSound`、`instantMovementAccumulatedThisFrame`、`lastCreatureHit`、`ActuationRodLock`；来源叶子 `PlayerInformationWorldAndMovementState`，序号 1166-1167, 1168, 1177, 1186。
- `instantMovementAccumulatedThisFrame` 是每帧移动阶段的可清零累加器，不是持久运动能力；`hermesStepSound` 是音频 effect payload，不是玩家权威状态；`lastCreatureHit` 是 Combat/NPC 结果引用；`hostile` 是网络/PvP 权限事实；`ActuationRodLock` 是 Wiring/World interaction lock。五者生命周期和副作用完全不同。
- Owner 规则：移动系统唯一写 accumulator，清零发生在明确的 frame start；音频由不可变 StepSoundProjection 发布；Combat/NPC owner 决定 last hit；协议/权限 owner 决定 hostile；Wiring/World owner 决定 actuation lock。P10 不宣布共享 owner。
- Version4 证据：`Player.cs:1968-2012` 声明；`Player.cs:10768` reset ActuationRodLock；`Player.cs:11980` 和 `Projectile.cs:12266` 写 lastCreatureHit；`Player.cs:14810,15801-15805` 清零/累加 movement vector；`Player.cs:20267-20283` 更新 SoundPlaySet；`MessageBuffer.cs:1440-1465` 写 hostile；`Player.cs:4664-4671,21899` 消费 hostile/team。
- 当前 NLTX 证据：Dome 已有 MovementIntent/PlayerControl systems 和 protocol compatibility hostile projection 路径，但没有证明 Version4 的 sound payload、creature-hit result、actuation lock 或 hostile authority 已完整迁移。
- 生命周期：accumulator 每 tick 创建/清零/消费；step sound 仅在 movement effect phase 输出；last hit 在 combat event 设置并按定义清除；hostile 受 session/permission/world reset；Actuation lock 在 action begin/end、disconnect、death 清理；不写 P10 玩家存档，除非另有证据。
- focused verifier（planned, `verificationStatus: not-run`）：accumulator 清零与 rope/pulley movement 顺序；step sound projection 无 I/O in component 且 cooldown 可重放；hostile 权限/网络广播与 team 关系；lastCreatureHit 只由 Combat/NPC event 写；ActuationRodLock 在拒绝/完成/断线路径释放；五个边界无双写。
- 回滚：维持现有 Movement/Combat/Wiring/Protocol adapters；单一 boundary 验证失败时撤回该 boundary 的新 reader route，不将 effect payload 和 authority state 合并。

### C06 成员归属表

| 来源序号 | 权威叶子 | 成员 | C# 类型 | proposed 归属 | 证据/备注 |
|---:|---|---|---|---|---|
| 1166 | `PlayerInformationWorldAndMovementState` | `hostile` | `bool` | `PlayerHostilityAdapter`; `crossSubsystemOwner: integration-review` | `Player.cs:1968`; `MessageBuffer.cs:1440-1465` |
| 1167 | `PlayerInformationWorldAndMovementState` | `hermesStepSound` | `Terraria.DataStructures.SoundPlaySet` | `PlayerStepSoundProjection` | `Player.cs:1970,20267-20283` |
| 1168 | `PlayerInformationWorldAndMovementState` | `instantMovementAccumulatedThisFrame` | `Vector2` | `PlayerInstantMovementAccumulatorComponent` | `Player.cs:1972,14810,15801-15805` |
| 1177 | `PlayerInformationWorldAndMovementState` | `lastCreatureHit` | `int` | `PlayerCreatureHitProjection`; `crossSubsystemOwner: integration-review` | `Player.cs:1990,11980`; `Projectile.cs:12266` |
| 1186 | `PlayerInformationWorldAndMovementState` | `ActuationRodLock` | `bool` | `PlayerActuationRodLockState`; `crossSubsystemOwner: integration-review` | `Player.cs:2012,10768` |

### C06 `PlayerInstantMovementAccumulator` core implementation checkpoint

- 实际修改文件：`src/Player/PlayerInstantMovementAccumulatorInput.cs`、`src/Player/PlayerInstantMovementAccumulatorComponent.cs`、`src/Player/PlayerInstantMovementAccumulatorSystem.cs`、`src/PlayerInteractionUiStateVerification/Program.cs`。
- 核心行为：`PlayerInstantMovementAccumulatorSystem.CreateFrameState` 创建零向量帧状态；`BeginFrame` 将上一帧累积值清零；`Accumulate` 对显式 `Vector2 MovementDelta` 做逐分量累加。该核心不读取 Player、绳索/滑轮、时钟或全局 World，也不消费或发布移动结果。
- 依赖影响：生产类型只依赖 `System.Numerics.Vector2` 和本地值类型；组件是 movement frame accumulator 的唯一局部 writer，未修改现有 MovementIntent/PlayerControl、协议、Combat、音频、Wiring 或 World owner，不新增网络/持久化字段。
- 验证：red verifier 通过 wrapper 构建退出码 `1`，原因是 7 个待实现 C06 类型/成员缺失；实现后通过 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`，退出码 `0`、警告 `0`、错误 `0`，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerInteractionUiStateVerification\Debug\net10.0\Terraria.PlayerInteractionUiStateVerification.dll`；确认产物存在后，通过 wrapper 运行 `run --project .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj --no-build --no-restore`，退出码 `0`，输出 `PASS: player interaction state, reuse, name, and NPC pressure queries are valid`。
- 未验证项：Version4 的真实 movement frame scheduler、rope/pulley 条件与顺序、累加器消费点、跨系统唯一 writer、spawn/teleport/disconnect 清理和行为等价仍未验证；`PlayerActuationRodLockStateComponent.IsActuationRodLocked` 字段已实现但未接入，C06 的 `hostile`、step-sound 播放/装备 writer、`lastCreatureHit`、Wiring/World lock writer 和生命周期仍保持 integration-review/evidence-gap。

### C06 `PlayerActuationRodLockStateComponent` implementation checkpoint

- 实际修改文件：`src/Player/PlayerActuationRodLockStateComponent.cs`。
- 核心行为：`PlayerActuationRodLockStateComponent` 只保存 P10 成员 `ActuationRodLock` 的 `bool` 状态；`IsActuationRodLocked` 通过 `struct` 默认初始化为 `false`。组件不执行 Wiring/World 交互，不获取或释放锁。
- 依赖影响：组件只依赖 `Terraria.Player` 项目内的值类型边界；未修改 Tile/Wiring/World owner、移动系统、网络、持久化或既有 Player 状态，也未创建锁的 System、Adapter 或 Command。
- 未验证项：Wiring/World 唯一 writer、action begin/end、拒绝/完成路径、death/disconnect 清理、跨系统调度和 Version4 行为等价仍未验证；组件状态为 `not-verified`，跨分区 owner 保持 `integration-review`。

### C06 `PlayerStepSoundProjection` core implementation checkpoint

- 实际修改文件：`src/Player/PlayerStepSoundInput.cs`、`src/Player/PlayerStepSoundProjection.cs`、`src/Player/PlayerStepSoundQuery.cs`、`src/PlayerInteractionUiStateVerification/Program.cs`。
- 核心行为：`PlayerStepSoundQuery.Calculate` 对显式腿部装备 ID生成不可变 projection；普通装备返回 Version4 ResetEffects 的 `SoundType=17`、`SoundStyle=-1`、`IntendedCooldown=9`，腿部装备 ID `140` 返回 `2/24/6`。Query 不播放声音、不更新 cooldown、不读取 Player 或 Equipment 全局状态。
- 依赖影响：生产类型只依赖本地值类型；`PlayerStepSoundProjection` 是音频 payload，不成为 Player 权威状态，不修改 Movement、Equipment、Audio、网络或持久化 owner，也不引入 `SoundPlaySet` 外部类型。
- 验证：red verifier 通过 wrapper 构建退出码 `1`，原因是 6 个待实现 C06 类型/成员缺失；实现后 wrapper build 退出码 `0`、警告 `0`、错误 `0`，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerInteractionUiStateVerification\Debug\net10.0\Terraria.PlayerInteractionUiStateVerification.dll`，确认产物存在后 wrapper run `--project .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj --no-build --no-restore` 退出码 `0`，输出 `PASS: player interaction state, reuse, name, and NPC pressure queries are valid`。
- 未验证项：完整 `SoundPlaySet` 兼容字段、其他装备/Mod 内容映射、step-sound 播放 port、cooldown 消费、movement phase 顺序、网络/持久化和 Version4 行为等价仍未验证；本检查点只验证两组已确认的纯常量映射。

## 12. C07 PlayerNavigationAndTimeInstruments

- Proposed types: `PlayerNavigationInstrumentComponent`, `PlayerNavigationInstrumentSystem`, `PlayerTimeSnapshot`; all `status: proposed`。
- 覆盖成员：`accCompass`、`accWatch`、`accWatchTime`、`accDepthMeter`、`accWeatherRadio`、`accCalendar`、`accStopwatch`；来源叶子 `PlayerInformationNavigationAndTimeState`，序号 1169-1174, 1176, 1180。
- 这些字段表示装备/能力提供的仪器等级或启用事实与时间读数，不是原始按键输入。`accCompass`/`accWatch`/`accDepthMeter` 是等级值，天气/日历/秒表是能力 flags，`accWatchTime` 是依赖时间快照的读数；不要把 UI 显示文本或系统 Stopwatch 句柄放入 Component。
- Owner 规则：装备/能力 Query 产生仪器 capability facts；`PlayerNavigationInstrumentSystem` 以显式 `PlayerTimeSnapshot` 和 world position/biome snapshot 计算确定性读数；UI/网络只消费 immutable projection。`accWatchTime` 不得直接调用 `DateTime.UtcNow`/`Stopwatch`。
- Version4 证据：`Player.cs:1974-1998` 声明；`Player.cs:6628-6662` 的跨玩家仪器聚合注释表明这些值曾参与全局能力汇总；`Player.cs:6704-6756` 按装备类型写等级/flags；`Player.cs:10327-10329,10501,10505-10506` 在 ResetEffects 清空；现有源码没有证明 P10 独立持久化格式。
- 生命周期：每个 tick 的 ResetEffects/ability commit 后重新计算；离开世界、重生、装备变化和 disconnect 清理；读数只在明确时间 snapshot 有效时生成；不将玩家仪器等级写入网络包，除非协议证据确认。
- focused verifier：能力事实 reset/apply core 已 `verified-focused`；完整计划仍覆盖装备提交顺序与 reset 后默认值、watch/compass/depth 等级单调规则、同一 time snapshot 的确定性、服务器与客户端 time projection 不越权，以及时间暂停/重连/world transition 不使用外部 wall clock 污染权威模拟。
- 回滚：保留现有 Equipment/Player effect reader 和 UI compatibility query；验证失败时撤回新的 instrument system，不新建持久化或网络字段。

### C07 成员归属表

| 来源序号 | 权威叶子 | 成员 | C# 类型 | proposed 归属 | 证据/备注 |
|---:|---|---|---|---|---|
| 1169 | `PlayerInformationNavigationAndTimeState` | `accCompass` | `int` | `PlayerNavigationInstrumentComponent` | `Player.cs:1974,6704-6728` |
| 1170 | `PlayerInformationNavigationAndTimeState` | `accWatch` | `int` | `PlayerNavigationInstrumentComponent` | `Player.cs:1976,6704-6728` |
| 1171 | `PlayerInformationNavigationAndTimeState` | `accWatchTime` | `double` | `PlayerNavigationInstrumentSystem` time snapshot output | `Player.cs:1978`; wall-clock policy gap |
| 1172 | `PlayerInformationNavigationAndTimeState` | `accDepthMeter` | `int` | `PlayerNavigationInstrumentComponent` | `Player.cs:1980,6722-6728` |
| 1174 | `PlayerInformationNavigationAndTimeState` | `accWeatherRadio` | `bool` | `PlayerNavigationInstrumentComponent` | `Player.cs:1984,6736` |
| 1176 | `PlayerInformationNavigationAndTimeState` | `accCalendar` | `bool` | `PlayerNavigationInstrumentComponent` | `Player.cs:1988,6740` |
| 1180 | `PlayerInformationNavigationAndTimeState` | `accStopwatch` | `bool` | `PlayerNavigationInstrumentComponent` | `Player.cs:1998,6756` |

### C07 `PlayerNavigationInstrument` core implementation checkpoint

- 实际修改文件：`src/Player/PlayerNavigationInstrumentInput.cs`、`src/Player/PlayerNavigationInstrumentComponent.cs`、`src/Player/PlayerNavigationInstrumentSystem.cs`、`src/PlayerInteractionUiStateVerification/Program.cs`。
- 核心行为：`PlayerNavigationInstrumentSystem.CreateResetState`/`BeginFrame` 将六项能力事实恢复为 Version4 的零值/false；`ApplyAccessoryType` 只接受显式 accessory type，按 `RefreshInfoAccsFromItemType` 已确认的 `15/16/17`、`707/708/709`、`18`、`393`、`395`、`3036/3037/3096/3099/3121/3123/3124/5358-5361` 映射 watch、compass、depth-meter、weather-radio、calendar 和 stopwatch 能力，并以 `Math.Max` 保持等级不会被较低等级覆盖。
- `accWatchTime` 未实现：它仍需要显式 `PlayerTimeSnapshot`/world snapshot 的真实生成证据；本 core 不调用系统时钟、不创建 Stopwatch 句柄，也不把 UI 读数写入能力组件。
- 依赖影响：生产类型只依赖本地值类型和 `System.Math`；没有修改现有 Equipment/Player writer、跨玩家聚合、网络/持久化 schema、UI reader 或 World owner，没有新增第二个时间 authority。
- 验证：先运行 wrapper build 的红测，退出码 `1`、警告 `0`、错误 `13`，错误均为待实现 C07 类型/成员缺失；实现后运行 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`，退出码 `0`、警告 `0`、错误 `0`，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerInteractionUiStateVerification\Debug\net10.0\Terraria.PlayerInteractionUiStateVerification.dll`；随后运行 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 run --project .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj --no-build --no-restore`，退出码 `0`，输出 `PASS: player interaction state, reuse, name, and NPC pressure queries are valid`。
- 未验证项：真实 Equipment/ability 唯一 writer、跨玩家 team aggregation、ResetEffects 与 spawn/death/disconnect 生命周期、`accWatchTime` 的时间/world projection、UI/网络读取、持久化以及 Version4 全行为等价仍未验证；C07 父组件保持 `partial`，分区级 `verificationStatus` 保持 `not-run`。

## 13. C08 PlayerDetectionAndWiringInstruments

- Proposed boundaries: `PlayerDetectionInstrumentComponent`, `PlayerDetectionInstrumentSystem`, `PlayerWiringOverlayProjection`; all `status: proposed`。
- 覆盖成员：`accFishFinder`、`accJarOfSouls`、`accThirdEye`、`accThirdEyeCounter`、`accOreFinder`、`accCritterGuide`、`accDreamCatcher`、`InfoAccMechShowWires`；来源叶子 `PlayerInformationDetectionAndWiringState`，序号 1173, 1175, 1178-1179, 1181-1183, 1187。
- 语义拆分：前七项是由装备/能力提交的检测 capability facts，其中 `accThirdEyeCounter` 是依赖 capability 的短期计数器；`InfoAccMechShowWires` 是接线/界面 overlay capability。检测能力不能直接扫描 World 并写回玩家；Wiring overlay 不能成为 Tile/Wiring authority。
- Owner 规则：Equipment/ability owner 提供只读 capability snapshot；`PlayerDetectionInstrumentSystem` 负责 reset、counter 生命周期和纯检测结果交接；Wiring system 负责实际 wire visibility/actuation，P10 只发布 projection。`accDreamCatcher` 对 DPS/creature 结果的影响需与 Combat integration review。
- Version4 证据：`Player.cs:1982-2016` 声明；`Player.cs:6640-6674,6732-6774` 按装备类型写入；`Player.cs:6995-7000` 清理第三眼计数/梦境检测条件；`Player.cs:10500-10509` ResetEffects 清空；`Player.cs:11972` 消费 dream catcher；`MessageBuffer.cs`/Wiring legacy 路径只证明网络接线有独立 current-user boundary，不能证明 P10 owner。
- 生命周期：每帧 reset 后由装备/能力 commit 重建；third-eye counter 在 capability 缺失时归零；检测输出按 world snapshot 计算；overlay 在 wiring/UI phase 发布；spawn/death/disconnect 清理 transient state；未有持久化/网络证据不扩展 schema。
- focused verifier（planned, `verificationStatus: not-run`）：八项字段唯一 writer；ResetEffects 后能力重建顺序；第三眼 counter 上限、失效和断线清理；ore/critter/fish/dream queries 对相同 world snapshot 确定性；wire overlay 只读且不改变 tile/mechanism；equipment、Wiring、Combat 不双写。
- 回滚：继续消费现有 Equipment/Wiring/Combat compatibility views；验证失败时撤回新的 detection reader/projection，不把 overlay 与能力 authority 合并。

### C08 成员归属表

| 来源序号 | 权威叶子 | 成员 | C# 类型 | proposed 归属 | 证据/备注 |
|---:|---|---|---|---|---|
| 1173 | `PlayerInformationDetectionAndWiringState` | `accFishFinder` | `bool` | `PlayerDetectionInstrumentComponent` | `Player.cs:1982,6732` |
| 1175 | `PlayerInformationDetectionAndWiringState` | `accJarOfSouls` | `bool` | `PlayerDetectionInstrumentComponent` | `Player.cs:1986,6748` |
| 1178 | `PlayerInformationDetectionAndWiringState` | `accThirdEye` | `bool` | `PlayerDetectionInstrumentComponent` | `Player.cs:1992,6744` |
| 1179 | `PlayerInformationDetectionAndWiringState` | `accThirdEyeCounter` | `byte` | `PlayerDetectionInstrumentComponent` | `Player.cs:1994,6995-6997` |
| 1181 | `PlayerInformationDetectionAndWiringState` | `accOreFinder` | `bool` | `PlayerDetectionInstrumentComponent` | `Player.cs:2000,6760` |
| 1182 | `PlayerInformationDetectionAndWiringState` | `accCritterGuide` | `bool` | `PlayerDetectionInstrumentComponent` | `Player.cs:2002,6752` |
| 1183 | `PlayerInformationDetectionAndWiringState` | `accDreamCatcher` | `bool` | `PlayerDetectionInstrumentComponent`; `crossSubsystemOwner: integration-review` | `Player.cs:2006,6764,11972` |
| 1187 | `PlayerInformationDetectionAndWiringState` | `InfoAccMechShowWires` | `bool` | `PlayerWiringOverlayProjection`; `crossSubsystemOwner: integration-review` | `Player.cs:2016,6774`; Wiring/UI seam |

### C08 `PlayerDetectionInstrument` core implementation checkpoint

- 实际修改文件：`src/Player/PlayerDetectionInstrumentInput.cs`、`src/Player/PlayerDetectionInstrumentComponent.cs`、`src/Player/PlayerDetectionInstrumentSystem.cs`、`src/PlayerInteractionUiStateVerification/Program.cs`。
- 核心行为：`PlayerDetectionInstrumentSystem.CreateResetState`/`BeginFrame` 清零六项检测能力事实；`ApplyAccessoryType` 按 `RefreshInfoAccsFromItemType` 已确认的 `3120`、`3036`、`3084`、`3095`、`3118`、`3102`、`3119`、`3122`、`3121`、`3123/3124/5358-5361` 映射 fish finder、jar of souls、third eye、critter guide、ore finder 和 dream catcher。
- `PlayerDetectionInstrumentComponent.ThirdEyeCounter` 已保存为 `byte` 组件字段，默认值为 `0`；本次只落地字段所有权，不实现递增、上限或消费逻辑。`InfoAccMechShowWires` 未实现，保留给 Wiring/UI overlay owner。`accDreamCatcher` 只记录能力事实，不在 P10 写 DPS/Combat 状态。
- 依赖影响：生产类型只依赖本地值类型；没有接入 World 扫描、NPC/creature 查询、Combat/DPS、Wiring/Tile、Equipment/Player writer、网络或持久化，也没有创建第二个检测计数器 authority。
- 验证：先运行 wrapper build 的红测，退出码 `1`、警告 `0`、错误 `14`，错误均为待实现 C08 类型/成员缺失；实现后运行 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`，退出码 `0`、警告 `0`、错误 `0`，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerInteractionUiStateVerification\Debug\net10.0\Terraria.PlayerInteractionUiStateVerification.dll`；随后运行 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 run --project .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj --no-build --no-restore`，退出码 `0`，输出 `PASS: player interaction state, reuse, name, and NPC pressure queries are valid`。
- 未验证项：真实 Equipment/ability writer、third-eye counter 的递增/上限/消费、World detection snapshot、Wiring overlay reader、Combat/DPS owner、spawn/death/disconnect 生命周期、网络/持久化和 Version4 行为等价仍未验证；C08 组件只标记为 `partial`，分区级 `verificationStatus` 保持 `not-run`。

## 14. C09 PlayerBuilderInteractionDefinitions

- Types: `PlayerBuilderInteractionCatalog`, `BuilderInteractionDefinitionQuery`; static definition/query core implemented, consumer integration remains `status: proposed`。
- 覆盖成员：`BuilderAccToggleIDs.RulerLine`、`RulerGrid`、`AutoActuate`、`AutoPaint`、`WireVisibility_Red`、`WireVisibility_Green`、`WireVisibility_Blue`、`WireVisibility_Yellow`、`HideAllWires`、`WireVisibility_Actuators`、`BlockSwap`、`TorchBiome`、`Count`；来源叶子 `PlayerBuilderInteractionDefinitions`，序号 391-403。
- 这是 `Player` 内的静态 definition/catalog，不是每玩家状态。`Count` 是数组容量契约；具体每玩家 `builderAccStatus` 不在本分区报告成员内，不能因同一文件出现而扩展 P10 归属。
- Owner 规则：catalog 只读、版本固定、由 Wiring/Builder/Tile query 消费；状态变化通过显式 BuilderToggleCommand 进入实际 owner，definition 不写实体和 World。不得用数组下标作为持久化或网络 identity。
- Version4 证据：`Player.cs:43-71` 定义 0..11 和 `Count=12`；`Player.cs:495` 仅证明存在按 Count 分配的其他字段，不足以证明其运行时 owner、网络 schema 或持久化；`Player.cs:1781-1783` 的 ruler overlay 是独立实例字段，归 C12。
- 生命周期：程序集/内容定义生命周期；内容版本改变必须经过 definition compatibility policy；运行时 query 对未知 toggle id 拒绝而不是越界访问。无独立玩家实体初始化、ResetEffects 或存档责任。
- focused verifier（planned, `verificationStatus: not-run`）：常量值与 Count 连续性；catalog 只读；未知/重复 toggle id 拒绝；Builder/Wiring consumer 不修改定义；C12 overlay 与 definition 的单向依赖；不同内容版本的 adapter 不重排已有 id。
- 回滚：保留 Version4 constants 或兼容 catalog；验证失败时不创建新 runtime component，恢复旧 definition lookup。

### C09 成员归属表

| 来源序号 | 权威叶子 | 成员 | C# 类型 | proposed 归属 | 证据/备注 |
|---:|---|---|---|---|---|
| 391 | `PlayerBuilderInteractionDefinitions` | `BuilderAccToggleIDs.RulerLine` | `int` | `PlayerBuilderInteractionCatalog` | `Player.cs:47` |
| 392 | `PlayerBuilderInteractionDefinitions` | `BuilderAccToggleIDs.RulerGrid` | `int` | `PlayerBuilderInteractionCatalog` | `Player.cs:49` |
| 393 | `PlayerBuilderInteractionDefinitions` | `BuilderAccToggleIDs.AutoActuate` | `int` | `PlayerBuilderInteractionCatalog` | `Player.cs:51` |
| 394 | `PlayerBuilderInteractionDefinitions` | `BuilderAccToggleIDs.AutoPaint` | `int` | `PlayerBuilderInteractionCatalog` | `Player.cs:53` |
| 395 | `PlayerBuilderInteractionDefinitions` | `BuilderAccToggleIDs.WireVisibility_Red` | `int` | `PlayerBuilderInteractionCatalog` | `Player.cs:55` |
| 396 | `PlayerBuilderInteractionDefinitions` | `BuilderAccToggleIDs.WireVisibility_Green` | `int` | `PlayerBuilderInteractionCatalog` | `Player.cs:57` |
| 397 | `PlayerBuilderInteractionDefinitions` | `BuilderAccToggleIDs.WireVisibility_Blue` | `int` | `PlayerBuilderInteractionCatalog` | `Player.cs:59` |
| 398 | `PlayerBuilderInteractionDefinitions` | `BuilderAccToggleIDs.WireVisibility_Yellow` | `int` | `PlayerBuilderInteractionCatalog` | `Player.cs:61` |
| 399 | `PlayerBuilderInteractionDefinitions` | `BuilderAccToggleIDs.HideAllWires` | `int` | `PlayerBuilderInteractionCatalog` | `Player.cs:63` |
| 400 | `PlayerBuilderInteractionDefinitions` | `BuilderAccToggleIDs.WireVisibility_Actuators` | `int` | `PlayerBuilderInteractionCatalog` | `Player.cs:65` |
| 401 | `PlayerBuilderInteractionDefinitions` | `BuilderAccToggleIDs.BlockSwap` | `int` | `PlayerBuilderInteractionCatalog` | `Player.cs:67` |
| 402 | `PlayerBuilderInteractionDefinitions` | `BuilderAccToggleIDs.TorchBiome` | `int` | `PlayerBuilderInteractionCatalog` | `Player.cs:69` |
| 403 | `PlayerBuilderInteractionDefinitions` | `BuilderAccToggleIDs.Count` | `int` | `PlayerBuilderInteractionCatalog` | `Player.cs:71` |

## 15. C10 PlayerSelectionState

- Proposed boundaries: the pure `PlayerSelectionQuery` core is implemented over an explicit snapshot; the `PlayerSelectionStateComponent` or integration bridge over the existing `SelectedItemComponent`, `PlayerSelectionSystem`, `PlayerRadialSelectionRegistry`, and `PlayerKiteSelectionProjection` remains `status: proposed`。
- 覆盖成员：顶层 `selectedItemState`、`selectedKite`，以及 `SelectedItemState` 的 `player`、`selected`、`hotbar`、`buffered`、`overridden`、`CanChangeSelectedItemImmediately`、`Selected`、`Hotbar`、`HasActiveOverride`、`HasBufferedChange`、`LastNonOverridenSelection`，和 `SelectionRadial` 的 `_SelectedBinding`、`RadialCount`、`Bindings`、`Mode`；来源叶子 `PlayerInteractionInputState`/`PlayerSelectionState`，序号 706-707, 431-439, 1421-1426。
- 语义拆分：selected slot authority、buffered/overridden transition、radial binding registry 和 kite selection 是不同边界。properties 是 query，不是独立可写 component。`player` 是实体关联，不是复制整个 Player 对象的理由；`Bindings` 是按选择模式使用的 registry/cache，必须有容量和失效规则。
- 现有 NLTX 交接：`dome/src/Terraria.Dome.Simulation/Inventory/Components/SelectedItemComponent.cs` 与 `ItemSelectionSystem.cs` 已占有一个选择候选；P10 不创建第二个同义 authority。由 P09/P10 integration review 决定保留、扩展或适配现有类型；本设计只定义 Version4 semantics 的兼容 projection/bridge。
- Version4 证据：`Player.cs:319-410` 的 `SelectedItemState` 构造、Select、buffer/override 和 query properties；`Player.cs:413-440` 的 radial state；`Player.cs:1009-1011` 顶层引用；`Player.cs:6814-6816,10430` kite selection/reset；`MessageBuffer.cs:685` 通过 packet 选择；`Player.cs:23435-23564` 在 ItemCheck 中阻止使用期间的 selection change。
- Owner 规则：selection command 经合法 slot/radial validation 后由唯一 selection system commit；Item/Inventory 只提供 slot facts，ItemUse 只读取 query；network packet 是 Adapter，不直接写 component；UI radial input 产生 command。`CanChangeSelectedItemImmediately` 等 properties 必须纯计算。
- 生命周期：player entity create 时建立 selection state；buffer/override 在 action window、item animation、loadout/inventory changes 和 spawn/teleport 按规则清理；radial registry 按 definition/config 生命周期；selectedKite 在 equipment/accessory commit 和 reset path 更新；不默认持久化 buffered/override/radial transient state。
- focused verifier（planned, `verificationStatus: not-run`）：slot range/disabled slot；immediate vs buffered/override transitions；last non-overridden selection；radial binding count/mode/duplicate binding；kite reset and selection; packet/UI duplicate command idempotence；P09 existing SelectedItemComponent 与 P10 bridge 不双写；ItemCheck 不消费 buffered selection 两次。
- 回滚：以现有 `SelectedItemComponent`/`ItemSelectionSystem` 和 legacy packet projection 保持旧读路径；新 bridge 验证失败时撤回 bridge，不保留第二个 selection authority。

### C10 成员归属表

| 来源序号 | 权威叶子 | 类型成员 | C# 类型 | proposed 归属 | 证据/备注 |
|---:|---|---|---|---|---|
| 706 | `PlayerInteractionInputState` | `selectedItemState` | `Terraria.Player.SelectedItemState` | `PlayerSelectionStateComponent` / P09 integration bridge | `Player.cs:1009,319-410` |
| 707 | `PlayerInteractionInputState` | `selectedKite` | `int` | `PlayerKiteSelectionProjection` | `Player.cs:1011,6814-6816,10430` |
| 431 | `PlayerSelectionState` | `SelectedItemState.player` | `Terraria.Player` | `PlayerSelectionEntityRef` | `Player.cs:321`; no Player clone |
| 432 | `PlayerSelectionState` | `SelectedItemState.selected` | `int` | existing selection authority bridge | `Player.cs:323` |
| 433 | `PlayerSelectionState` | `SelectedItemState.hotbar` | `int` | existing selection authority bridge | `Player.cs:325` |
| 434 | `PlayerSelectionState` | `SelectedItemState.buffered` | `int` | `PlayerSelectionTransientState` | `Player.cs:327` |
| 435 | `PlayerSelectionState` | `SelectedItemState.overridden` | `int` | `PlayerSelectionTransientState` | `Player.cs:329` |
| 436 | `PlayerSelectionState` | `SelectionRadial._SelectedBinding` | `int` | `PlayerRadialSelectionRegistry` | `Player.cs:422` |
| 437 | `PlayerSelectionState` | `SelectionRadial.RadialCount` | `int` | `PlayerRadialSelectionRegistry` | `Player.cs:424` |
| 438 | `PlayerSelectionState` | `SelectionRadial.Bindings` | `int[]` | `PlayerRadialSelectionRegistry` | `Player.cs:426` |
| 439 | `PlayerSelectionState` | `SelectionRadial.Mode` | `Terraria.Player.SelectionRadial.SelectionMode` | `PlayerRadialSelectionRegistry` | `Player.cs:428` |
| 1421 | `PlayerSelectionState` | `CanChangeSelectedItemImmediately` | `bool` | `PlayerSelectionQuery` | `Player.cs:331`; pure over ItemUse facts |
| 1422 | `PlayerSelectionState` | `Selected` | `int` | `PlayerSelectionQuery` | `Player.cs:343` |
| 1423 | `PlayerSelectionState` | `Hotbar` | `int` | `PlayerSelectionQuery` | `Player.cs:345` |
| 1424 | `PlayerSelectionState` | `HasActiveOverride` | `bool` | `PlayerSelectionQuery` | `Player.cs:347` |
| 1425 | `PlayerSelectionState` | `HasBufferedChange` | `bool` | `PlayerSelectionQuery` | `Player.cs:349` |
| 1426 | `PlayerSelectionState` | `LastNonOverridenSelection` | `int` | `PlayerSelectionQuery` | `Player.cs:351-367` |

### C10 选择状态纯 Query 实现检查点

- 实际修改文件：`src/Player/PlayerSelectionStateInput.cs`、`src/Player/PlayerSelectionStateSnapshot.cs`、`src/Player/PlayerSelectionQuery.cs`、`src/PlayerSelectionVerification/Program.cs`、`src/PlayerSelectionVerification/Terraria.PlayerSelectionVerification.csproj`。
- 核心行为：`PlayerSelectionQuery.Evaluate` 只读取显式 selected/hotbar/buffered/overridden 与 ItemUse/time facts，保持 Version4 的 `CanChangeSelectedItemImmediately`、`HasActiveOverride`、`HasBufferedChange` 和 `LastNonOverridenSelection` 纯派生语义；不持有 `Player` 引用、不提交 selection、不修改 Inventory/P09 authority。
- 依赖影响：生产类型只依赖 `Terraria.Player` 本地值类型；没有新增 P09 selection writer、radial registry、selectedKite equipment writer、协议、网络或持久化引用。
- 验证：红测 wrapper build 退出码 `1`、警告 `0`、错误 `14`，原因是待实现 C10 输入/快照/Query 类型缺失；实现后通过 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\PlayerSelectionVerification\Terraria.PlayerSelectionVerification.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`，退出码 `0`、警告 `0`、错误 `0`，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerSelectionVerification\Debug\net10.0\Terraria.PlayerSelectionVerification.dll`；随后 wrapper run `--project .\src\PlayerSelectionVerification\Terraria.PlayerSelectionVerification.csproj --no-build --no-restore` 退出码 `0`，输出 `PASS: player selection query preserves Version4 derived semantics`。
- 未验证项：P09 `SelectedItemComponent` 唯一 writer、slot/radial legality、`SelectionRadial` registry、`selectedKite` equipment/reset writer、Player entity reference、packet/UI duplicate/replay policy、ItemCheck 消费顺序、生命周期、网络/持久化和 Version4 完整行为等价仍未验证。
- 回滚：删除本检查点的三个生产文件和 focused verifier 即可恢复现有 selection authority 及旧 reader；不修改 P09 选择实现。

## 16. C11 PlayerInputSyncAndMatchAdapters

- Proposed boundaries: `PlayerInputSyncProjection`, `ChannelCancellationExpectation`, `SetMatchCommandAdapter`, `PlayerSyncAndMatchValidator`; all `status: proposed`。
- 覆盖成员：`PlayerInputSyncCache.controlLeft`、`controlRight`、`controlUp`、`controlDown`、`controlJump`、`PressingAnyInput`，`ChannelCancelKey.ProjectileTypeExpected`、`ProjectileIndexExpected`，`SetMatchRequest.Player`、`Head`、`Body`、`Legs`、`ArmorSlotRequested`、`Male`；来源叶子 `PlayerInputSyncAndMatch`，序号 404-410, 421-426, 1419。
- 这些成员不是一个 authority component：sync cache 是由当前 committed input 产生的兼容/网络 snapshot； `PressingAnyInput` 是纯 query；channel key 是短期 projectile expectation（与 C05 `_channelShotCache` 对接）；SetMatchRequest 是一次性外观匹配 command payload，不能保存完整 Player 引用为持久实体状态。
- Version4 证据：`Player.cs:74-99` cache 构造和 query；`Player.cs:98-135` channel key matching/tracking；`Player.cs:242-254` match payload；`Player.cs:20065-20123` 从 armor/appearance 计算 match request；`MessageBuffer.cs:652-738` 控制/选择网络写入。匹配网络入口和 duplicate/replay policy 的完整闭合仍 partial。
- 当前 NLTX 证据：`LegacyPlayerControlsState`/`LegacyPlayerControlsProjection`、`TerrariaSession.AcceptPlayerControls`、`DomeNetworkUpdateBridge` 和 `PlayerStateProjection` 已提供兼容 adapter/projection；它们尚未提供每玩家 input sequence 去重、乱序拒绝或 replay protection，也未闭合 SetMatch equivalent。
- Owner 规则：Server/Protocol 负责 session ownership、sequence、duplicate/replay 和 permission validation；Simulation 只接收 validated command/snapshot；Projection 单向输出，不能把 sync cache 写回 raw input。SetMatch 的外观结果交 P09/P11 owner，P10 只定义 command contract。
- 生命周期：sync cache 每个网络/模拟快照生成，可丢弃；channel expectation 在 channel start/update/cancel 后清除；match request 在一次 commit/reject 后失效；disconnect/reconnect 清除旧 session identity；不持久化。
- focused verifier（planned, `verificationStatus: not-run`）：cache 与 C01 round-trip；`PressingAnyInput` 空输入/跳跃边界；channel key 类型+projectile index 匹配、重复 packet 幂等；SetMatch 的 slot range、appearance bounds、male/skin compatibility、权限和 stale session 拒绝；乱序/重复/重放 packet 不修改 authority；projection 不反写。
- 回滚：保留 legacy protocol projection and session gate；失败时撤销新 adapter/validator，不将 payload 复制成持久 Player component，也不伪造缺失的 input sequence semantics。

### C11 成员归属表

| 来源序号 | 权威叶子 | 类型成员 | C# 类型 | proposed 归属 | 证据/备注 |
|---:|---|---|---|---|---|
| 404 | `PlayerInputSyncAndMatch` | `PlayerInputSyncCache.controlLeft` | `bool` | `PlayerInputSyncProjection` | `Player.cs:76` |
| 405 | `PlayerInputSyncAndMatch` | `PlayerInputSyncCache.controlRight` | `bool` | `PlayerInputSyncProjection` | `Player.cs:78` |
| 406 | `PlayerInputSyncAndMatch` | `PlayerInputSyncCache.controlUp` | `bool` | `PlayerInputSyncProjection` | `Player.cs:80` |
| 407 | `PlayerInputSyncAndMatch` | `PlayerInputSyncCache.controlDown` | `bool` | `PlayerInputSyncProjection` | `Player.cs:82` |
| 408 | `PlayerInputSyncAndMatch` | `PlayerInputSyncCache.controlJump` | `bool` | `PlayerInputSyncProjection` | `Player.cs:84` |
| 409 | `PlayerInputSyncAndMatch` | `ChannelCancelKey.ProjectileTypeExpected` | `int` | `ChannelCancellationExpectation` | `Player.cs:110,25767-25798` |
| 410 | `PlayerInputSyncAndMatch` | `ChannelCancelKey.ProjectileIndexExpected` | `int` | `ChannelCancellationExpectation` | `Player.cs:112,129-135` |
| 421 | `PlayerInputSyncAndMatch` | `SetMatchRequest.Player` | `Terraria.Player` | `SetMatchCommandAdapter` | `Player.cs:244`; short-lived entity ref |
| 422 | `PlayerInputSyncAndMatch` | `SetMatchRequest.Head` | `int` | `SetMatchCommandAdapter` | `Player.cs:246` |
| 423 | `PlayerInputSyncAndMatch` | `SetMatchRequest.Body` | `int` | `SetMatchCommandAdapter` | `Player.cs:248` |
| 424 | `PlayerInputSyncAndMatch` | `SetMatchRequest.Legs` | `int` | `SetMatchCommandAdapter` | `Player.cs:250` |
| 425 | `PlayerInputSyncAndMatch` | `SetMatchRequest.ArmorSlotRequested` | `int` | `SetMatchCommandAdapter` | `Player.cs:252` |
| 426 | `PlayerInputSyncAndMatch` | `SetMatchRequest.Male` | `bool` | `SetMatchCommandAdapter` | `Player.cs:254` |
| 1419 | `PlayerInputSyncAndMatch` | `PlayerInputSyncCache.PressingAnyInput` | `bool` | `PlayerInputSyncQuery` | `Player.cs:86-96`; pure query |

## 17. C12 PlayerBuilderOverlayProjection

- Types: `PlayerBuilderOverlayInput`, `PlayerBuilderOverlayProjection`, `BuilderOverlayQuery`; pure projection core is implemented and focused-verified, while runtime integration remains `status: proposed`。
- 覆盖成员：`rulerGrid`、`rulerLine`；来源叶子 `PlayerBuilderOverlayState`，序号 1075-1076。
- 这两个成员是 builder UI/overlay 输出状态，不是 `BuilderAccToggleIDs` 定义，也不是 Tile/Wiring mutation authority。它们可能由装备/能力计算阶段设置，并在 reset path 清理/重建，故 proposed 边界应为短期 projection；不要以 UI 反向写 `builderAccStatus` 或 C09 catalog。
- Version4 证据：`Player.cs:1781-1783` 声明；`Player.cs:6778-6782,8826-8830` 由 builder/equipment paths 设置；`Player.cs:10659-10660` 在 ResetEffects 处理默认值；`Player.cs:45-71,495` 证明 definition/status 是独立边界。完整 renderer reader 和网络/持久化 schema 未闭合。
- Owner 规则：Builder/Equipment facts -> pure `BuilderOverlayQuery` -> immutable UI/renderer projection；projection 不调用 UI/renderer、不写 Player authority、不修改 C09 definition。写入旧字段的兼容 adapter 只有在唯一 owner 和 reset verifier 闭合后才可删除。
- 生命周期：每 tick 在 effects/reset 阶段重建，spawn/death/disconnect 清理或重置；overlay 只在当前 frame/view 有效；不持久化，不加入 player replication，除非后续证据明确要求。
- focused verifier（planned, `verificationStatus: not-run`）：ruler defaults and reset ordering；builder toggle/equipment changes invalidate overlay；projection determinism and no write-back；UI hidden/overlay state cannot trigger tile mutation; C09 constants and C12 booleans keep one-way dependency。
- 回滚：继续使用 legacy builder overlay reads；撤回新 projection builder，不创建第二个 builder capability state。

### C12 实现检查点

- 实际修改文件：`src/Player/PlayerBuilderOverlayInput.cs`、`src/Player/PlayerBuilderOverlayProjection.cs`、`src/Player/BuilderOverlayQuery.cs`、`src/PlayerBuilderOverlayVerification/Program.cs`、`src/PlayerBuilderOverlayVerification/Terraria.PlayerBuilderOverlayVerification.csproj`。
- 核心行为：`BuilderOverlayQuery.Calculate` 只读取显式 `PlayerBuilderOverlayInput`，返回不可变 `PlayerBuilderOverlayProjection`，逐项传递 `rulerGrid`/`rulerLine` facts；不调用 UI/renderer，不写 Player authority，不修改 C09 catalog。
- 依赖影响：生产代码只依赖 `Terraria.Player` 内的值类型；没有接入 Equipment、ResetEffects、Tile/Wiring、网络或持久化，也没有增加项目引用。
- 验证：通过 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\PlayerBuilderOverlayVerification\Terraria.PlayerBuilderOverlayVerification.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`；退出码 `0`，警告 `0`，错误 `0`；产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerBuilderOverlayVerification\Debug\net10.0\Terraria.PlayerBuilderOverlayVerification.dll`，同时生成 `Terraria.Player.dll`。随后通过 wrapper 运行 `run --project .\src\PlayerBuilderOverlayVerification\Terraria.PlayerBuilderOverlayVerification.csproj --no-build --no-restore`，退出码 `0`，输出 `PASS: builder overlay projection is deterministic and one-way`。
- 未验证项：Version4 的 `ResetEffects` 默认值（`rulerGrid=false`、`rulerLine=true`）、装备能力 writer、renderer reader、spawn/death/disconnect 生命周期和网络/持久化边界仍未接入或验证；focused verifier 仅覆盖显式 facts 的确定性传递、默认 false 输入和无写回投影。
- 回滚：删除 C12 三个生产文件和 focused verifier 即可恢复旧 overlay reader；不修改 C09 definition 或其他分区的 Equipment/Builder owner。

### C12 成员归属表

| 来源序号 | 权威叶子 | 成员 | C# 类型 | proposed 归属 | 证据/备注 |
|---:|---|---|---|---|---|
| 1075 | `PlayerBuilderOverlayState` | `rulerGrid` | `bool` | `PlayerBuilderOverlayProjection` | `Player.cs:1781,6778,8826,10659` |
| 1076 | `PlayerBuilderOverlayState` | `rulerLine` | `bool` | `PlayerBuilderOverlayProjection` | `Player.cs:1783,6782,8830,10660` |

## 18. C13 PlayerItemSpaceAndSettings

- Proposed boundaries: `PlayerItemSpaceQuery`, `PlayerPersonalInventoryEligibilityQuery`, `PlayerSettingsAdapter`; all `status: proposed`。
- 覆盖成员：`ItemSpaceStatus.CanTakeItem`、`ItemIsGoingToVoidVault`、`CanTakeItemToPersonalInventory`、`Settings.DashControl`；来源叶子 `PlayerItemSpaceAndSettings`，序号 427-428, 430, 1420。
- `ItemSpaceStatus` 是一次物品接纳资格结果，不是玩家持久状态；`CanTakeItemToPersonalInventory` 是纯派生 query。容量、stack、ammo、unique item、void vault 的事实由 P09/Items/Container owner 提供，`PlayerItemSpaceQuery` 只在显式快照上按 Version4 顺序计算。`DashControl` 是用户设置适配值，不能与按键 `controlDash` 或 C02 release state 合并。
- Version4 证据：`Player.cs:257-279` 声明和值构造；`Player.cs:22811-22874` 的 `ItemSpace`/`CanPullItem` 计算 inventory、ammo、stack 和 void-vault fallback；`Player.cs:316` 设置默认值；`Player.cs:12957-12970` dash 逻辑读取 setting 与 input/release。现有 Dome 已有 `PlayerVoidVaultStateComponent`，因此 P10 不另建 void-vault authority。
- Owner 规则：Items/Inventory 提供 explicit item/container snapshot，`PlayerItemSpaceQuery` 纯计算资格；Item pickup command 消费结果并由 P09/Items commit；Settings Adapter 负责读取/验证用户配置；C01/C02 只提供 input facts。Query 不保留 Item 引用、不调用 I/O、不写 inventory。
- 生命周期：资格结果为调用范围内 value，计算后失效；void-vault capability 由现有 P09/Dome owner 提供；DashControl 由 settings/profile 生命周期管理，重连/配置加载有明确默认值；不把资格结果存档或复制。
- focused verifier（`verificationStatus: verified-focused`）：显式 facts 上的 personal inventory/void-vault fallback、unique stack/ammo slot rule 输入、`CanTakeItemToPersonalInventory == CanTakeItem && !ItemIsGoingToVoidVault`、query 无写回和重复调用确定性、DashControl default/config round-trip；C01 dash input 与 settings policy 不双写；existing `PlayerVoidVaultStateComponent` only one owner。真实 catalog/slot snapshot producer、commit、网络、持久化和运行时等价仍未验证。
- 回滚：继续使用现有 Items/Inventory/VoidVault query and settings adapters；验证失败时不创建 P10 component，撤回新的 facade/query reader。

### C13 成员归属表

| 来源序号 | 权威叶子 | 类型成员 | C# 类型 | proposed 归属 | 证据/备注 |
|---:|---|---|---|---|---|
| 427 | `PlayerItemSpaceAndSettings` | `ItemSpaceStatus.CanTakeItem` | `bool` | `PlayerItemSpaceQuery` | `Player.cs:259,22822-22874` |
| 428 | `PlayerItemSpaceAndSettings` | `ItemSpaceStatus.ItemIsGoingToVoidVault` | `bool` | `PlayerItemSpaceQuery`; P09/VoidVault integration | `Player.cs:261,22872` |
| 430 | `PlayerItemSpaceAndSettings` | `Settings.DashControl` | `Terraria.Player.Settings.DashPreference` | `PlayerSettingsAdapter` | `Player.cs:316,12957` |
| 1420 | `PlayerItemSpaceAndSettings` | `ItemSpaceStatus.CanTakeItemToPersonalInventory` | `bool` | `PlayerPersonalInventoryEligibilityQuery` | `Player.cs:263-273` |

### C13 ItemSpace eligibility pure Query implementation checkpoint

- 实际修改文件：`src/Player/PlayerItemSpaceInput.cs`、`src/Player/PlayerItemSpaceSnapshot.cs`、`src/Player/PlayerItemSpaceQuery.cs`、`src/Player/PlayerPersonalInventoryEligibilityQuery.cs`、`src/PlayerItemSpaceVerification/Program.cs`、`src/PlayerItemSpaceVerification/Terraria.PlayerItemSpaceVerification.csproj`。
- 核心行为：`PlayerItemSpaceQuery.Evaluate` 只复制显式 `CanTakeItem`/`ItemIsGoingToVoidVault` 资格事实到不可变快照；`PlayerPersonalInventoryEligibilityQuery.CanTakeItemToPersonalInventory` 严格计算 `CanTakeItem && !ItemIsGoingToVoidVault`，不写库存、不读取 Item/Container/VoidVault 外部状态。
- 依赖影响：生产类型只依赖 `Terraria.Player` 本地值类型；没有创建 `PlayerVoidVaultStateComponent`、Item、Inventory 或 Settings authority，也没有修改现有 inventory/void-vault command、网络、持久化或 C01/C02 input owner。
- 验证：红测 wrapper build 退出码 `1`、警告 `0`、错误 `13`，原因是 C13 输入/快照/Query 类型尚不存在；实现后通过 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\PlayerItemSpaceVerification\Terraria.PlayerItemSpaceVerification.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`，退出码 `0`、警告 `0`、错误 `0`，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerItemSpaceVerification\Debug\net10.0\Terraria.PlayerItemSpaceVerification.dll`；随后 wrapper run `--project .\src\PlayerItemSpaceVerification\Terraria.PlayerItemSpaceVerification.csproj --no-build --no-restore` 退出码 `0`，输出 `PASS: item-space eligibility preserves Version4 derived semantics`。
- 未验证项：Version4 `ItemSpace` 的完整 inventory/ammo/stack/unique-item 扫描、VoidVault capability 与唯一 writer、个人库存 commit、Settings/profile source、`DashControl` default/config round-trip、网络/持久化、运行时调度和完整行为等价仍未验证。
- 回滚：删除本检查点的四个生产 Query/value 文件和 focused verifier 即可恢复现有 Items/Inventory/VoidVault reader 路径；不修改其他分区 owner。

### C13 DashControl settings adapter implementation checkpoint

- 实际修改文件：`src/Player/PlayerDashControlPreference.cs`、`src/Player/PlayerDashControlSettingsInput.cs`、`src/Player/PlayerDashControlSettingsSnapshot.cs`、`src/Player/PlayerSettingsAdapter.cs`、`src/PlayerItemSpaceVerification/Program.cs`。
- 核心行为：`PlayerSettingsAdapter.ReadDashControl` 对显式可选设置执行确定性适配；缺省值为 Version4 `AllowDoubleTap`，显式 `OnlyThroughHotkeys` 原样往返。适配器不保存静态可变配置，不写 `Settings.DashControl`，不读取或修改 C01 `controlDash`/C02 release state。
- 依赖影响：新增类型仅依赖 `Terraria.Player` 本地 enum/value 类型；没有接入 profile/config I/O、持久化、dash system、网络或其他分区 owner。
- 验证：红测 wrapper build 退出码 `1`、警告 `0`、错误 `12`，原因是 DashControl input/snapshot/preference/adapter 类型尚不存在；实现后复用 C13 focused verifier 构建命令退出码 `0`、警告 `0`、错误 `0`，产物保持 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerItemSpaceVerification\Debug\net10.0\Terraria.PlayerItemSpaceVerification.dll`；wrapper run `--project .\src\PlayerItemSpaceVerification\Terraria.PlayerItemSpaceVerification.csproj --no-build --no-restore` 退出码 `0`，输出 `PASS: item-space eligibility preserves Version4 derived semantics`。
- 未验证项：真实 Settings/profile reader、非法 enum 值策略、配置写入/持久化、dash runtime consumer、C01/C02 与设置 policy 的集成和完整行为等价仍未验证。
- 回滚：删除本检查点的四个 settings adapter/value 文件并移除 verifier 中的设置断言即可恢复现有设置边界；不修改 C01/C02 或其他分区 owner。

### C13 `PlayerItemSpace` explicit-facts evaluation Query implementation checkpoint

- 实际修改文件：`src/Player/PlayerItemSpaceCandidate.cs`、`src/Player/PlayerItemSpaceSlotSnapshot.cs`、`src/Player/PlayerItemSpaceEvaluationInput.cs`、`src/Player/PlayerItemSpaceQuery.cs`、`src/PlayerItemSpaceVerification/Program.cs`。
- 核心行为：新增重载只接收不可变候选物品、完整 inventory slot snapshot、VoidVault slot snapshot 和显式 capability facts；按 Version4 顺序处理 pickup short-circuit、unique-stack rejection、普通槽位 0-49/coin 0-53、ammo 54-57、ammo stack fallback 和 VoidVault fallback，并返回 `CanTakeItem`/`ItemIsGoingToVoidVault` 快照。slot acceptance 保留空槽、favorited-only-one、最大堆叠和 type/prefix stack compatibility 规则；不持有 Item 引用、不修改容器。
- 依赖影响：生产项目仍只依赖 `Terraria.Player`，未把 `Terraria.Items` 或 `Terraria.Content` 直接耦合进 Player；真实 catalog、inventory、ammo 和 VoidVault owner 通过后续 integration adapter 提供显式 facts。
- 验证：先更新 focused verifier 并通过 wrapper build 得到预期红测：退出码 `1`，4 个错误，0 个警告，原因是新增 Query 输入类型不存在；实现后通过 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\PlayerItemSpaceVerification\Terraria.PlayerItemSpaceVerification.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`，退出码 `0`、警告 `0`、错误 `0`，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerItemSpaceVerification\Debug\net10.0\Terraria.PlayerItemSpaceVerification.dll`；随后新增 Version4 `HasItem` 的 `0..57` 边界断言，首次 wrapper run 退出码 `1` 并报告 trash slot 被错误计入，修正后再次使用同一串行 build 命令退出码 `0`、警告 `0`、错误 `0`，再以 `run --project .\src\PlayerItemSpaceVerification\Terraria.PlayerItemSpaceVerification.csproj --no-build --no-restore` 运行退出码 `0`，输出 `PASS: item-space eligibility preserves Version4 derived semantics`。
- 未验证项：真实 `ItemID.Sets.IsAPickup`/catalog 映射、`HasItem` 的 inventory snapshot producer、coin/ammo slot layout producer、`CanVoidVaultAccept` writer、个人库存 commit、Settings/profile、网络/持久化、调度和完整行为等价仍未验证；focused verifier 已覆盖显式 facts 的 pickup、unique-stack、coin、ammo、VoidVault 和 trash-slot boundary 分支。
- 回滚：删除本检查点新增的三个输入值文件、移除 `PlayerItemSpaceQuery` 的 evaluation overload 和 focused verifier 新增断言即可恢复此前的 eligibility query core；不修改其他分区 owner。

## 19. 完整性与交接声明

- C01-C13 的 110 字段、8 属性、118 成员均已在本设计中逐项登记；成员只保留 P10 权威报告中的 11 个叶子组，不复制其他分区成员。
- C01 的 `PlayerInputCommand`、`PlayerRawControlInputComponent`、`PlayerRawControlInputSystem` 和 `PlayerInputRejectionReason`，C02 的 release/repeat isolated core，C03 的 UI/交互门控 core、`nameLen` definition/query、ItemUse 复用资格 Query 和 NPC 压力贡献 Query，C04 的 ItemUse/tile intent core，C05 的 channel cancellation expectation Adapter core，C06 的 instant-movement accumulator core，C07/C08 的 instrument capability-fact core，C09 的 builder definition/query core，C10 的 selection pure Query core，C11 的 input sync projection core，C12 的 builder overlay pure projection core，以及 C13 的 item-space eligibility Query、DashControl settings adapter 和 explicit-facts evaluation Query cores 已写入 `src/Player`；C03 其余成员、C04 其余边界、C05 其余边界、C06 其余边界、C07-C08、C10-C11、C13 的 runtime/integration 类型仍未实现。C03、C04、C05、C06、C07、C08、C09、C10、C11、C12、C13 focused verifier 已通过，但这些核心尚未完成完整运行时接入。
- C01 实现只负责对单条命令和整批命令执行玩家槽位、活动状态、facing、批内重复与组件存在性校验，并在整批校验通过后统一写入 8 个原始控制字段；没有接入协议、网络发送、时钟、日志或 UI。
- 本轮除上述 `src/Player` 文件外，新增了 C02 focused verifier 项目 `src/PlayerReleaseRepeatVerification`、C09 focused verifier 项目 `src/PlayerBuilderInteractionVerification`、C10 focused verifier 项目 `src/PlayerSelectionVerification` 和 C12 focused verifier 项目 `src/PlayerBuilderOverlayVerification`，并扩展了既有 C13 focused verifier `src/PlayerItemSpaceVerification`；本次重试新增 `src/Player/PlayerEntityInteractionLockStateComponent.cs`，未修改其他分区文档、权威报告或 ledger。
- C01/C02 编译证据：C01/C02 生产项目曾通过 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\Player\Terraria.Player.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`，退出码 0、0 个警告、0 个错误，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.Player\Debug\net10.0\Terraria.Player.dll`；本轮重试对同一项目执行相同 wrapper 命令，退出码 `1`、警告 `0`、错误 `5`，错误来自既有 Progression 文件的缺失符号；本次 C02 focused verifier 使用上方记录的串行 build/run 命令退出码分别为 0/0，C02 runtime、网络和持久化验证仍未运行。
- C09 focused verifier 先以退出码 1 报告缺少生产 catalog，随后通过 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\PlayerBuilderInteractionVerification\Terraria.PlayerBuilderInteractionVerification.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false` 构建成功，退出码 0、0 个警告、0 个错误；产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerBuilderInteractionVerification\Debug\net10.0\Terraria.PlayerBuilderInteractionVerification.dll`。随后 wrapper 运行 `run --project .\src\PlayerBuilderInteractionVerification\Terraria.PlayerBuilderInteractionVerification.csproj --no-build --no-restore` 退出码 0，输出 PASS。
- `SelectedItemComponent`、`PlayerVoidVaultStateComponent`、Movement/Input systems 和 Protocol projections 是当前 NLTX 事实，不等于 P10 已完成迁移；实现前要选择唯一 authority path。
- Integration handoff 必须解决：P09/P10 selection 与 inventory owner；C04/C05 与 Item/Projectile/Wiring/Combat owner；C03/C06 与 NPC/Combat/World/Network owner；C07/C08 与 Equipment/World/Wiring owner；C11 的 per-session sequence/replay/permission；C12 overlay 的 renderer/UI reader；C13 的 Items/VoidVault/settings owner。
当前实现状态为 `executionStatus: in-progress`、`implementationStatus: partial`、`verificationStatus: not-run`；C01/C02 已有隔离核心源码，C02 另有通过 focused verifier 的 reset/release/hover/timer 边界证据，C03 已有通过 focused verifier 的 UI/交互门控、name definition/query、ItemUse 复用资格 Query 和 NPC 压力贡献 Query 核心，并已保存 `PlayerItemReuseStateComponent`，C04 已有通过 focused verifier 的 ItemUse/tile intent core，C05 已有通过 focused verifier 的 channel cancellation expectation Adapter core，C06 已有通过 focused verifier 的 instant-movement accumulator core 和 step-sound projection core，并已保存 `PlayerActuationRodLockStateComponent`，C07/C08 已有通过 focused verifier 的 instrument capability-fact core，并已保存 `PlayerDetectionInstrumentComponent.ThirdEyeCounter` 字段，C09 已有静态 definition/query 核心，C10 已有通过 focused verifier 的纯选择 Query core，C11 已有通过 focused verifier 的输入同步 projection core，C12 已有通过 focused verifier 的纯投影核心，C13 已有通过 focused verifier 的 item-space eligibility、DashControl settings adapter 和 explicit-facts evaluation Query cores；C01/C02/C03/C04/C05/C06/C07/C08/C09/C10/C11/C12/C13 的完整运行时、网络、持久化与行为等价验证，以及真实 consumer 集成验证仍未完成。当前组件保持为 C13；C03/C06/C08 的跨子系统 writer/lifecycle 与 C13 的真实 catalog/snapshot producer、VoidVault/settings owner 和 runtime integration 尚未取得实现证据。

## 20. 当前状态声明

状态修订：本节早期汇总中的“当前组件为 C11”及“C13 尚未完成”属于历史 checkpoint 文字；当前有效状态以文档顶部元数据和 C13 explicit-facts evaluation Query checkpoint 为准。C02 的 focused verifier 已新增并通过，C13 的三个 focused core 也已保存并通过 focused verifier，但 C02/C13 的真实运行时 owner、跨系统调度、catalog/snapshot producer、VoidVault/settings owner 和运行时集成仍保持 pending/integration-review，分区级 `verificationStatus: not-run` 不变。

本次已完成 C01-C13 设计检查点，并完成 C01/C02 隔离核心、C02 release/repeat focused verifier、C03 UI/交互门控、name definition/query、ItemUse 复用资格 Query 和 NPC 压力贡献 Query 核心、C04 ItemUse/tile intent core、C05 channel cancellation expectation Adapter core、C06 instant-movement accumulator core 和 step-sound projection core、C07/C08 instrument capability-fact core、C09 静态 definition/query 核心、C10 纯选择 Query 核心、C11 输入同步 projection core、C12 纯投影核心以及 C13 三个显式事实核心；C02、C03、C04、C05、C06、C07、C08、C10、C11、C12、C13 focused verifier 已通过。当前已验证/保存的实现组件以顶部 `completedComponents` 为准，当前组件保持为 C13；C03 其余成员、C04 其余边界、C05 其余边界、C06 其余边界、C07-C08、C10-C11、C13 的运行时/集成边界尚未完成。C01-C13 尚未完成完整运行时接入，分区级 `verificationStatus: not-run` 保持不变。

## 21. 本次继续执行验证记录

- 活动进程检查：执行构建前未发现活动的 `dotnet.exe` 或 `csc.exe` 编译进程。
- 命令：`pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\Player\Terraria.Player.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`。
- 项目：`D:\TRbackup\NLTX\src\Player\Terraria.Player.csproj`；目标框架 `net10.0`。
- 结果：退出码 `1`；警告 `0`；错误 `5`。错误来自既有 `src/Player/Progression/PlayerMinionCapacityCommitSystem.cs` 和 `src/Player/Progression/SubmitMinionCapacityDeltaCommand.cs`，缺失 `Terraria.Relationships`、`Terraria.Projectile`、`EntityReference` 和 `ProjectileIdentityComponent`；本次没有修改这些非组件文件。
- 输出：期望产物 `D:\TRbackup\NLTX\Build\bin\Terraria.Player\Debug\net10.0\Terraria.Player.dll` 的路径存在旧 DLL，最后写入时间为 `2026-09-12T02:04:19.6756303Z`，本次失败构建未生成或刷新该文件；因此 `PlayerEntityInteractionLockStateComponent` 没有获得当前编译验证，分区 `verificationStatus` 继续保持 `not-run`。
- 本轮结算：已使用当前绑定 `dfcf6281bf7d4ac2804704e61407153d` 执行 `Fail`；runner 返回 `status=failed`、`partition=P10`、`lockReleased=true`，命令退出码为 `1`。失败原因是组件-only 范围无法闭合实体 claim/release、ItemCheck、Tile/Wiring/Entity 生命周期和唯一 writer，且 `Terraria.Player` 项目仍被上述 5 个既有缺失符号阻塞。

## 22. 本轮重试最终状态

- 当前有效 session 为 `dfcf6281bf7d4ac2804704e61407153d`；runner ledger 已结算为 `P10=failed/manual`，不是旧 session 的状态延续。
- 当前有效组件为 `C04_PlayerEntityInteractionLockState_integration`，已保存 `src/Player/PlayerEntityInteractionLockStateComponent.cs`，仅覆盖 `P10-828 isOperatingAnotherEntity: bool` 的独立事实字段。
- 该组件没有 claim/release、ItemCheck、Tile/Wiring/Entity 行为、成功/拒绝路径、断线/死亡清理或唯一 writer；`executionStatus=blocked`、`implementationStatus=partial`、`verificationStatus=not-run`。
