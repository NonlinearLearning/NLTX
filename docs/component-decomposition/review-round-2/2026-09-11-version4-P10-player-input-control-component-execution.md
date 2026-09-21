# Version4 P10 玩家输入、控制、选择与建造交互执行计划

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
proposedTypeStatus: C01/C02 isolated cores, C03 UI/name/item-reuse/NPC-pressure query cores, C04 item-use/tile-intent core, C05 channel-cancellation expectation adapter core, C06 instant-movement accumulator core, C07 navigation-instrument capability-fact core, C08 detection-instrument capability-fact core, C09 definitions/query, C10 selection-state pure query core, C11 input-sync projection core, C12 projection core, and C13 item-space eligibility query/DashControl settings adapter/explicit-facts evaluation query cores implemented with focused verification; protocol/scheduler integration and C03-C08/C10-C11/C13 runtime boundaries remain proposed
designCompletedComponents: [C01_PlayerRawControlInput, C02_PlayerReleaseAndRepeatState, C03_PlayerInteractionRuntimeState, C04_PlayerItemUseIntent, C05_PlayerChannelAndActionContext, C06_PlayerMovementAndWorldInteraction, C07_PlayerNavigationAndTimeInstruments, C08_PlayerDetectionAndWiringInstruments, C09_PlayerBuilderInteractionDefinitions, C10_PlayerSelectionState, C11_PlayerInputSyncAndMatchAdapters, C12_PlayerBuilderOverlayProjection, C13_PlayerItemSpaceAndSettings]
completedComponents: [C01_PlayerRawControlInput, C02_PlayerReleaseAndRepeatState, C02_PlayerReleaseAndRepeatFocusedVerifier, C03_PlayerInteractionUiStateCore, C03_PlayerItemReuseQuery, C03_PlayerItemReuseStateComponent, C03_PlayerNameDefinition, C03_PlayerNpcPressureQuery, C04_PlayerItemUseIntentCore, C04_PlayerEntityInteractionLockStateComponent, C05_PlayerChannelCancellationCore, C06_PlayerInstantMovementAccumulatorCore, C06_PlayerActuationRodLockStateComponent, C06_PlayerStepSoundProjectionCore, C07_PlayerNavigationInstrumentCore, C08_PlayerDetectionInstrumentCore, C08_PlayerDetectionInstrumentComponent, C09_PlayerBuilderInteractionDefinitions, C10_PlayerSelectionQueryCore, C11_PlayerInputSyncProjectionCore, C12_PlayerBuilderOverlayProjection, C13_PlayerItemSpaceEligibilityQueryCore, C13_PlayerSettingsAdapterCore, C13_PlayerItemSpaceEvaluationQueryCore]
currentComponent: C04_PlayerEntityInteractionLockState_integration
pendingComponents: [C03_PlayerInteractionRuntimeState_crossSubsystem, C04_PlayerEntityInteractionLockState_integration, C05_PlayerChannelStateComponent_crossSubsystem, C06_PlayerActuationRodLockState_integration, C07_PlayerWatchTimeProjection_dependency, C08_PlayerWiringOverlayProjection_crossSubsystem, C10_PlayerSelectionState_crossSubsystem, C11_PlayerInputSyncAndMatchAdapters_crossSubsystem, C13_PlayerItemSpaceAndSettings_crossSubsystem]
currentProgress: C01 原始控制隔离核心、C02 释放/重复隔离核心、C02 release/repeat focused verifier、C09 建造交互定义/查询、C12 建造 overlay 纯投影核心、C03 UI/交互门控核心、C03 `nameLen` 定义/范围 Query、C03 ItemUse 复用资格 Query、C03 `PlayerItemReuseStateComponent`、C03 ItemUse 重用状态组件、C03 NPC 压力贡献 Query、C04 ItemUse/tile intent core、C04 `PlayerEntityInteractionLockStateComponent` 字段、C05 channel cancellation expectation Adapter core、C06 instant-movement accumulator core、C06 step-sound projection core、C06 `PlayerActuationRodLockStateComponent`、C07 导航仪器能力事实 reset/apply core、C08 检测仪器能力事实 reset/apply core、C08 `PlayerDetectionInstrumentComponent.ThirdEyeCounter` 字段、C10 选择状态纯 Query core、C11 输入同步纯 projection core、C13 ItemSpace eligibility pure Query core、C13 DashControl settings adapter core 和 C13 ItemSpace explicit-facts evaluation Query core 已实现并保存；C02、C03、C04、C05、C06、C07、C08、C09、C10、C11、C12、C13 focused verifier 已通过。C01/C02/C03/C04/C05/C06/C07/C08/C09/C10/C11/C12/C13 未完成完整运行时接入；C02、C03、C04、C05、C06、C07、C08、C10、C11、C13 仍有跨分区成员待集成审查。
lastCheckpointUtc: 2026-09-12T09:35:12.4903762Z
evidence-gap: C01 的协议 sequence/replay envelope、匹配权限、持久化、跨分区唯一 writer 和 focused verifier 仍需 integration-review 闭合。C02 的 focused verifier 已覆盖显式输入下的 reset、release 派生、hover 传递和方向 timer 边界，但真实上一 tick 输入来源、Item/Tile release 消费、空批/非法批次清理、spawn/teleport/disconnect 调度、重复包策略和完整行为等价仍未闭合。C03 的 `team/sign/aggro/nearbyActiveNPCs` 跨分区 owner、`reuseDelay/pendingItemReuse` ItemUse 唯一 writer、`changeItem` C10 handoff，以及 UI gate 的真实 client input writer 尚未闭合；NPC Query 的真实 Main/NPC 清零累加 writer、active-range 接入、slot cost 来源、NPC 类型注册范围、aggro targeting owner、调度顺序和行为等价仍缺证据。C04 的 `controlUseItem/controlUseTile` intent 虽有纯 reset/advance core，但协议映射、C02 release gate、Item/Tile/Wiring/Entity action owner、`tileInteractAttempted`、entity lock、success projection、`autoReuseAllWeapons`、`altFunctionUse` 和 `delayUseItem` 的生命周期及行为等价仍未闭合；未创建第二个 ItemUse authority。C05 的 channel cancellation expectation Adapter core 已按 Version4 规则验证，但 `manaCost` 的完整装备/能力输入、`channel` 唯一 Item/Projectile writer、Tag/intention/rabbit helper 生命周期、creative permission、网络 schema 和 channel 行为等价仍未闭合；未创建 `channel` authority 或持有 Projectile 引用。C06 的 accumulator 和 step-sound projection core 已闭合显式输入/常量映射，但 movement scheduler 的真实唯一 writer、rope/pulley 顺序、accumulator 消费点、step-sound cooldown/播放消费、装备 writer、hostile 权限/网络 owner、lastCreatureHit Combat/NPC/Projectile owner、ActuationRodLock Wiring/World owner 和行为等价仍未闭合。C07 的 `accWatchTime` 没有真实时间/world snapshot 生成路径证据，Equipment/ability 唯一 writer、跨玩家聚合、ResetEffects/lifecycle 调度、UI/网络读取和行为等价仍未闭合；本次只验证显式 accessory type 到六项能力事实的纯 reset/apply core。C08 的 `accThirdEyeCounter` 字段现已落在 `PlayerDetectionInstrumentComponent`，但递增/上限/消费 writer 仍未闭合；`InfoAccMechShowWires` 的 Wiring/UI owner、World detection snapshot、Equipment/ability writer、Combat/DPS owner、生命周期和行为等价仍未闭合。C09 的真实 Builder/Wiring consumer、内容版本兼容策略和 `builderAccStatus` 唯一 writer 仍未闭合。C12 的 focused verifier 已通过，但 Equipment/ResetEffects defaults、renderer reader、生命周期和网络/持久化边界尚未验证。
evidence-gap-addendum: C10 的 P09 `SelectedItemComponent` 唯一 authority、buffer/override 提交、slot/radial validation、selectedKite equipment writer、entity reference、network/UI command 去重和生命周期仍未闭合；本检查点仅验证显式选择快照上的纯派生语义。
evidence-gap-addendum: C11 的 per-session sequence/replay/permission、duplicate/乱序拒绝、channel expectation 生命周期、SetMatch 外观 owner 和网络失败语义仍未闭合；本检查点仅验证五个 committed control facts 的纯同步 projection 与 `PressingAnyInput` query。
evidence-gap-addendum: C13 的真实 item/content catalog 到 `PlayerItemSpaceCandidate` 的映射、背包/弹药/VoidVault snapshot owner、Item 类型与 prefix/stack/ammo/unique-item facts 的生产者、VoidVault capability writer、个人库存 commit owner、Settings/profile 来源与 `DashControl` 持久化仍未闭合；本检查点已在显式 facts 上验证 Version4 `ItemSpace` 的扫描顺序和结果派生，但没有创建 Item/Inventory/VoidVault/Settings authority。
evidence-gap-addendum: C13 的 `Settings.DashControl` 真实 profile/config reader、写入/持久化边界、非法枚举值策略和 dash runtime consumer 仍未闭合；本检查点只验证显式可选设置到本地 `PlayerDashControlPreference` 的默认/往返适配，不创建静态 Settings authority，也不与 C01 `controlDash` 双写。
evidence-gap-addendum: C02 focused verifier 的首轮运行发现并修正了 verifier 自身对第二帧右方向 timer 的错误期望；最终绿测只证明当前显式输入核心的确定性边界，不证明 C01 writer、真实 tick 调度、release 消费或跨系统行为等价。
blocking-decision: integration-review

## 1. 计划边界

本文件记录 P10 的 118 个成员如何在现有 NLTX 输入、玩家、库存、移动、协议和交互边界中逐批迁移。实现阶段仍须保持 Version4 公共 API、命名空间和兼容 Adapter；C01/C02 仅完成隔离核心，C09 仅完成静态 definition/query 核心，C12 仅保存纯投影核心，尚未完成完整运行时接入。

推荐实现顺序是 C01 -> C02 -> C03/C04/C05 -> C06 -> C07/C08 -> C09 -> C10 -> C11 -> C12 -> C13。该顺序是依赖草案，不允许通过文件名或目录顺序表达运行时顺序；最终顺序要由 scheduler contract 和集成 verifier 固定。

## 2. 建议目标文件与依赖影响

表中所有 proposed 目标文件、接口和类型均保持 `status: proposed`；它们是执行阶段的候选路径，不代表当前文件已经创建或已迁移。

| 检查点 | 计划文件/边界 | 源到目标 | 依赖影响与回滚 |
|---|---|---|---|
| C01 | 复用并收敛 `dome/src/Terraria.Dome.Simulation/Components/Player/ControlInputComponent.cs`、`PlayerInputApplySystem.cs`；必要时新增 `PlayerInputCommand` validator | `Player.cs` control fields -> validated tick input | 与 `PlayerControlSystem`、Protocol V1456、Transport 共享；失败时恢复旧 apply route，不双写 |
| C02 | proposed `dome/src/Terraria.Dome.Simulation/Player/Components/PlayerReleaseAndRepeatStateComponent.cs`、`PlayerReleaseAndRepeatSystem.cs`；当前隔离核心位于 `src/Player` | release/hover/timer fields -> edge/repeat state | 依赖 C01 前一 tick；失败时保留 legacy edge adapter |
| C03 | proposed `PlayerInteractionRuntimeComponent`、`PlayerInteractionQuery`、`PlayerMouseInterfaceProjection`、`PlayerItemReuseBridge` | interaction fields -> narrow state/query/projection | 与 P09/P11/P12/P16 交接；失败时只撤新 reader route |
| C04 | proposed `PlayerItemUseIntentComponent`、`PlayerItemUseIntentSystem`、`TileInteractionCommand`、`EntityInteractionCommand` | use/tile flags -> explicit intent/command | 与 Item/Tile/Wiring owners 共享；验证不重复消费后再切 reader |
| C05 | proposed `PlayerChannelStateComponent`、`PlayerItemUseCostQuery`、`PlayerIntentionAdapter`、`ChannelCancelAdapter`、`RabbitOrderAdapter` | channel/helper members -> state/query/short-lived adapter | 与 Projectile/Combat/Creative/P08 integration-review；helper 不能进入持久组件 |
| C06 | proposed `PlayerInstantMovementAccumulatorComponent`、`WorldInteractionState`、`HermesStepSoundProjection` | movement/world fields -> frame state and effects | 与 Movement/Combat/Wiring/Audio owner 交接；失败时恢复旧 frame reads |
| C07 | proposed `PlayerNavigationInstrumentStateComponent`、`NavigationInstrumentSystem` | accCompass/watch/depth/weather/calendar/stopwatch -> instrument facts | 时钟通过 explicit `ITickSource`；未有持久化证据不写存档 |
| C08 | proposed `PlayerDetectionAndWiringStateComponent`、`DetectionInstrumentSystem`、`WiringOverlayQuery` | accessory detection/wiring fields -> capability facts | 与 P09 equipment、Wiring、World query 交接；counter reset verifier 失败则不切换 |
| C09 | `src/Player/PlayerBuilderInteractionCatalog.cs`、`src/Player/BuilderInteractionDefinitionQuery.cs`；focused verifier 位于 `src/PlayerBuilderInteractionVerification` | `BuilderAccToggleIDs` -> read-only definitions | 已实现静态定义和范围 query；不创建实体组件；真实 Builder/Wiring consumer 与定义版本兼容仍需 integration-review |
| C10 | proposed `PlayerSelectionStateComponent`、`SelectionSystem`、`SelectionQuery` | selected item/radial/kite + nested properties -> selection authority/query | 与 P09 `SelectedItemComponent` 做唯一 owner 决策；失败保留兼容 view |
| C11 | proposed `PlayerInputSyncProjection`、`ChannelCancellationExpectation`、`SetMatchCommand`/adapter | sync cache/key/match request -> projection/short payload | Server owns sequence/ownership; network adapter 失败不影响 domain authority |
| C12 | proposed `PlayerBuilderOverlayProjection`、`BuilderOverlayQuery` | ruler booleans -> immutable UI/overlay output | 不能被 UI 反向写回 builder capability |
| C13 | proposed `PlayerItemSpaceQuery`、`PlayerSettingsAdapter` | ItemSpaceStatus/DashControl -> query/config | Item/Container owns capacity result; settings adapter owns user config |

上述路径均为 proposed 目标，不是当前已存在实现。新增文件必须遵循按 capability 组织、一个核心公开类型一个 PascalCase 文件，不创建泛化 `Shared/Components`。

## 3. 全局执行不变量

- 先建立 focused verifier，再接入一个唯一 writer，最后迁移 readers；旧路径只能作为只读兼容 adapter，不允许 legacy 与新类型双写。
- Component 只保存可持续的领域状态；一次性输入、网络帧、日志、流、UI 输出和外部引用放入 Command/Adapter/Projection。
- 时钟、随机数、日志、网络发送、持久化和 UI/音频效果通过显式 port 或 adapter 进入执行边界；系统内业务计算保持确定性。
- 对每个玩家/每个 tick 必须定义重复、乱序、缺失、断线、重连、非法目标和权限变化。`SimulationInputBatch` 的 sequence 字段不能在没有 Server policy 前假装提供 replay protection。
- 任何跨分区共享候选保留 `crossSubsystemOwner: integration-review`，不得因本执行计划替其他分区裁决。

## 4. C01 PlayerRawControlInput

状态：design-checkpoint-complete；C01 隔离核心已实现；`executionStatus: in-progress`、`implementationStatus: partial`、`verificationStatus: not-run`。

成员：`controlLeft`、`controlRight`、`controlUp`、`controlDown`、`controlJump`、`controlTorch`、`controlDash`、`controlDownHold`，来源序号 809, 810, 811, 812, 813, 816, 817, 827。

执行步骤：

1. 为输入命令建立玩家句柄、active、facing、批内唯一性和 sequence policy 的纯验证；验证失败时不能清空现有 `ControlInputComponent`。
2. 对照 V1456 `PlayerControlIntent` 和 `LegacyPlayerControlsProjection`，补齐 `Up`、`Down`、`Torch`、`Dash` 等语义映射的 focused fixture；不把兼容 projection 当作 domain owner。
3. 将 C01 的一个 writer 收敛到 `PlayerInputApplySystem` 或经集成审查指定的同一系统；`PlayerControlSystem` 只读取提交后的状态并产出移动意图。
4. 为空批、重复玩家、未知玩家、非法 facing、重放 sequence 和乱序 sequence 编写 verifier；在 verifier 未通过前不迁移其他输入 readers。

验收：8 个字段逐项 round-trip；批次验证在状态变更前失败；单玩家单 tick 只有一份输入；不存在第二个 raw-input writer；没有新增持久化或网络承诺。

回滚：保留 `ControlInputComponent` 的现有兼容读取和 `LegacyPlayerControlsProjection`，撤回本批新 validator/reader route；不得删除其他会话文件。

### C01 实现检查点

- 实际修改文件：`src/Player/PlayerInputCommand.cs`、`src/Player/PlayerRawControlInputComponent.cs`、`src/Player/PlayerRawControlInputSystem.cs`、`src/Player/PlayerInputRejectionReason.cs`。
- 核心行为：`PlayerRawControlInputSystem.TryApply` 校验单条命令的非负玩家槽位、目标活动状态和左右 facing；`TryApplyBatch` 先校验所有命令、拒绝批内重复玩家、未知玩家和缺少目标组件，再一次性提交 8 个原始控制字段，因此失败批次不会清空或部分更新既有组件。
- 依赖影响：新增类型只依赖 `Terraria.Player` 既有 `LegacyPlayerSlot`/`DirectionKind`；未修改 `InputIntentComponent`、协议、网络、调度器或其他领域 owner，也未添加新的项目引用。
- 未验证项：`Sequence` 仅随命令携带，尚未实现 replay/乱序策略；未验证协议映射、唯一运行时 writer、断线/重生调度和 Version4 行为等价。

### C01 构建验证记录

- 命令：`pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\Player\Terraria.Player.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`
- 项目：`D:\TRbackup\NLTX\src\Player\Terraria.Player.csproj`
- 结果：退出码 `0`；警告 `0`；错误 `0`；生成成功。
- 输出：`D:\TRbackup\NLTX\Build\bin\Terraria.Player\Debug\net10.0\Terraria.Player.dll`，已确认文件存在。
- 限制：本轮没有 focused verifier 项目；没有运行 `test`、运行时、网络、持久化或行为等价验证，因此 `verificationStatus: not-run` 保持不变。

## 5. 后续检查点计划

### C02 PlayerReleaseAndRepeatState

状态：design-checkpoint-complete；隔离核心已实现并保存，focused verifier 已通过；`executionStatus: in-progress`、`implementationStatus: partial`、组件级 `verificationStatus: verified-focused`，分区级 `verificationStatus: not-run`。

覆盖 12 个成员：`releaseJump`、`releaseUp`、`releaseUseItem`、`releaseUseTile`、`releaseLeft`、`releaseRight`、`releaseDown`、`releaseDash`、`tryKeepingHoveringDown`、`tryKeepingHoveringUp`、`leftTimer`、`rightTimer`。

已定义 `PlayerReleaseAndRepeatInput` 作为当前 tick 的值输入，并实现 `PlayerReleaseAndRepeatStateComponent` 与 `PlayerReleaseAndRepeatSystem` 的 reset、释放状态、hover 持续意图和左右方向计时器计算。尚未把它接入 C01 的运行时 writer；验证 teleport/spawn/empty-input 的清理、长按、快速按下释放和重复包仍未完成。`releaseUseItem`/`releaseUseTile` 仍须与 Item/Tile owner 交接，不能由 UI 或协议 projection 直接写回；验收前保留旧兼容 reader，失败时只回滚本检查点的 writer/reader route。

#### C02 实现检查点

- 实际修改文件：`src/Player/PlayerReleaseAndRepeatInput.cs`、`src/Player/PlayerReleaseAndRepeatStateComponent.cs`、`src/Player/PlayerReleaseAndRepeatSystem.cs`、`src/PlayerReleaseRepeatVerification/Program.cs`、`src/PlayerReleaseRepeatVerification/Terraria.PlayerReleaseRepeatVerification.csproj`。
- 核心行为：`CreateResetState` 将释放状态设为 true、悬停意图设为 false、方向计时器设为 0；`Advance` 在 reset 时整体恢复状态，否则按当前输入计算释放状态、传递悬停意图、保留外部 item/tile release 事实，并维护左右方向 7 tick 重复窗口。
- 依赖影响：只依赖本地值类型和 `System.Math.Clamp`；没有修改 C01、协议、Item/Tile、网络、调度器或其他领域 owner，也没有新增项目引用。
- 验证：先以已存在生产核心构建 verifier，首轮运行退出码 `1`，原因是 verifier 对第二帧右方向 timer 的期望错误；修正 verifier 后通过 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\PlayerReleaseRepeatVerification\Terraria.PlayerReleaseRepeatVerification.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`，退出码 `0`、警告 `0`、错误 `0`，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerReleaseRepeatVerification\Debug\net10.0\Terraria.PlayerReleaseRepeatVerification.dll`，同时生成 `D:\TRbackup\NLTX\Build\bin\Terraria.Player\Debug\net10.0\Terraria.Player.dll`；随后通过 wrapper 运行 `run --project .\src\PlayerReleaseRepeatVerification\Terraria.PlayerReleaseRepeatVerification.csproj --no-build --no-restore`，退出码 `0`，输出 `PASS: release and repeat state preserves Version4 isolated edge and timer semantics`。
- 未验证项：真实上一 tick 输入来源、空批/非法批次的旧状态保护、spawn/teleport/disconnect reset、重复包、release 边沿单 tick 单次消费、Item/Tile release 消费和 Version4 完整行为等价尚未验证。

### C09 PlayerBuilderInteractionDefinitions

状态：design-checkpoint-complete；静态 definitions/query 和 focused verifier 已实现并保存；`executionStatus: in-progress`、`implementationStatus: partial`、`verificationStatus: verified-focused`。

覆盖 13 个成员：`BuilderAccToggleIDs.RulerLine`、`RulerGrid`、`AutoActuate`、`AutoPaint`、`WireVisibility_Red`、`WireVisibility_Green`、`WireVisibility_Blue`、`WireVisibility_Yellow`、`HideAllWires`、`WireVisibility_Actuators`、`BlockSwap`、`TorchBiome`、`Count`。

- 实际修改文件：`src/Player/PlayerBuilderInteractionCatalog.cs`、`src/Player/BuilderInteractionDefinitionQuery.cs`、`src/PlayerBuilderInteractionVerification/Program.cs`、`src/PlayerBuilderInteractionVerification/Terraria.PlayerBuilderInteractionVerification.csproj`。
- 核心行为：catalog 将 Version4 的 12 个 ID 保持为 `0..11`，`Count` 保持为 `static readonly int` 值 `12`；`BuilderInteractionDefinitionQuery.IsKnownToggleId` 纯计算接受 `[0, Count)`，拒绝负数和 `Count` 及以上的 ID。
- 依赖影响：生产代码没有项目外依赖，不创建每玩家 component，不读写 `builderAccStatus`、Builder/Wiring/World 或协议；focused verifier 通过反射校验常量形状、精确值、Count 和边界。
- 验证：红测阶段 verifier 构建成功但运行退出码 `1`，报告 catalog 缺失；实现后通过 wrapper 构建 `Terraria.PlayerBuilderInteractionVerification.csproj`，退出码 `0`、警告 `0`、错误 `0`，产物 `Build/bin/Terraria.PlayerBuilderInteractionVerification/Debug/net10.0/Terraria.PlayerBuilderInteractionVerification.dll`；wrapper 运行 `run --project .\src\PlayerBuilderInteractionVerification\Terraria.PlayerBuilderInteractionVerification.csproj --no-build --no-restore` 退出码 `0`，输出 PASS。
- 未验证项：Builder/Wiring consumer 是否只读、未知/重复 toggle command 的最终拒绝路径、内容版本兼容策略、`builderAccStatus` 唯一 writer 和 C12 单向 overlay 仍未验证。

### C03 PlayerInteractionRuntimeState

状态：design-checkpoint-complete；`executionStatus: in-progress`、`implementationStatus: partial`、`verificationStatus: not-run`。

覆盖 `team`、`nameLen`、`sign`、`reuseDelay`、`aggro`、`nearbyActiveNPCs`、`creativeInterface`、`mouseInterface`、`lastMouseInterface`、`noThrow`、`changeItem`、`pendingItemReuse`。其中 `nameLen` 是定义，`nearbyActiveNPCs` 是聚合 query，鼠标/creative/noThrow 字段是 client/input gate 候选，`reuseDelay/pendingItemReuse` 属于 ItemUse 时间状态；`team`/`aggro`/`sign` 标记跨分区 owner。

先闭合直接读写和 reset，再将 item reuse/selection change 变为显式 command；验证 UI 输入不可越权写权威状态、NPC 聚合不被玩家系统双写、pending reuse 只消费一次。实现路径必须拆成 `PlayerInteractionUiStateComponent`、`PlayerItemReuseStateComponent`、`PlayerNpcPressureQuery` 和 team/sign adapter，不能创建一个覆盖全部字段的通用组件。

#### C03 UI/交互门控核心实现检查点

- 实际修改文件：`src/Player/PlayerInteractionUiStateInput.cs`、`src/Player/PlayerInteractionUiStateComponent.cs`、`src/Player/PlayerInteractionUiStateSystem.cs`、`src/PlayerInteractionUiStateVerification/Program.cs`、`src/PlayerInteractionUiStateVerification/Terraria.PlayerInteractionUiStateVerification.csproj`。
- 核心行为：`PlayerInteractionUiStateSystem.CreateResetState` 将四个 UI/交互门控字段清零；`Advance` 在非 reset 帧逐项复制显式输入，在 reset 帧恢复默认值并丢弃输入。
- 依赖影响：未接入 UI、Main、Projectile、ItemUse、Selection、网络、持久化或调度器；未修改其他 P10/C03 候选 owner。
- 验证：红测 verifier 构建退出码 `0`，运行退出码 `1`，报告生产类型缺失；实现后 wrapper 构建退出码 `0`、警告 `0`、错误 `0`，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerInteractionUiStateVerification\Debug\net10.0\Terraria.PlayerInteractionUiStateVerification.dll`；wrapper 运行 `run --project .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj --no-build --no-restore` 退出码 `0`，输出 `PASS: player interaction UI state advances and resets explicitly`。
- 未验证项：真实 client input writer、Main/Projectile reader、spawn/death/disconnect 生命周期、`noThrow` 计时、C10 `changeItem` handoff、网络/持久化和行为等价仍未验证。

#### C03 `nameLen` 定义/范围 Query 实现检查点

- 实际修改文件：`src/Player/PlayerNameDefinition.cs`、`src/Player/PlayerNameLengthQuery.cs`、`src/PlayerInteractionUiStateVerification/Program.cs`。
- 核心行为：`PlayerNameDefinition.MaximumLength` 保留 Version4 `Player.nameLen = 20`；`PlayerNameLengthQuery.IsWithinLimit` 判断非负长度是否不超过 20，不处理空名策略、字符串规范化或网络拒绝。
- 依赖影响：纯定义和 Query 无实体状态、无 I/O、无网络/持久化写入；协议的空名、重复名、权限和拒绝消息仍由外部 owner 处理。
- 验证：红测运行退出码 `1`，原因是生产定义类型缺失；实现后 wrapper 构建退出码 `0`、警告 `0`、错误 `0`，产物仍为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerInteractionUiStateVerification\Debug\net10.0\Terraria.PlayerInteractionUiStateVerification.dll`；wrapper 运行退出码 `0`，输出 `PASS: player interaction UI state advances and resets explicitly`。
- 未验证项：名称字符串编码、空名/重复名策略、协议字段和持久化边界仍未验证。

#### C03 ItemUse 复用资格 Query 实现检查点

- 实际修改文件：`src/Player/PlayerItemReuseSnapshot.cs`、`src/Player/PlayerItemReuseQuery.cs`、`src/PlayerInteractionUiStateVerification/Program.cs`。
- 核心行为：`PlayerItemReuseQuery.IsUsingOrReusingItem` 判断 `itemAnimation > 0 || reuseDelay > 0 || channel || pendingItemReuse`，保持 Version4 `Player.UsingOrReusingItem` 的纯计算语义；不修改现有 `PlayerUseComponent` 或 `PlayerItemUseState`。
- 依赖影响：只读快照/Query 无实体写入、无 Item/Projectile 调用、无网络/持久化 I/O；现有两个 ItemUse 候选继续由 integration-review 决定唯一 owner。
- 验证：红测运行退出码 `1`，原因是 `Terraria.Player.PlayerItemReuseSnapshot` 生产类型缺失；实现后 wrapper 构建退出码 `0`、警告 `0`、错误 `0`，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerInteractionUiStateVerification\Debug\net10.0\Terraria.PlayerInteractionUiStateVerification.dll`；wrapper 运行退出码 `0`，输出 `PASS: player interaction UI state advances and resets explicitly`。
- 未验证项：`reuseDelay`/`pendingItemReuse` 真实写者、ItemCheck 清理、spawn/teleport/disconnect 生命周期、重复消费和行为等价仍未验证；只读 Query 不代表 C03 ItemUse 状态迁移完成。

#### C03 ItemUse 重用状态组件实现检查点

- 实际修改文件：`src/Player/PlayerItemReuseStateComponent.cs`。
- 核心行为：`PlayerItemReuseStateComponent` 只保存 `reuseDelay` 的剩余 tick 数和一次性 `pendingItemReuse` 事实；`struct` 默认值为 `ReuseDelayRemainingTicks = 0`、`PendingItemReuse = false`。组件不计算复用资格、不消费 pending 标记，也不执行 ItemCheck 或调度行为。
- 依赖影响：组件只依赖 `Terraria.Player` 项目内的值类型边界；没有修改既有 `PlayerUseComponent`/`PlayerItemUseState`，没有创建第二个 ItemUse writer，也没有新增 Item、Projectile、网络或持久化引用。
- 未验证项：ItemCheck 的真实唯一 writer、pending 标记只消费一次、tick 倒计时、spawn/teleport/death/disconnect 清理和旧路径行为等价仍未验证；本检查点只证明组件源码和字段所有权边界已经落地。
- 组件验证状态：`not-verified`。

#### C03 NPC 压力贡献 Query 实现检查点

- 实际修改文件：`src/Player/PlayerNpcPressureContributionInput.cs`、`src/Player/PlayerNpcPressureContribution.cs`、`src/Player/PlayerNpcPressureQuery.cs`、`src/PlayerInteractionUiStateVerification/Program.cs`。
- 核心行为：`PlayerNpcPressureQuery.Evaluate` 只对显式 NPC/player snapshot 计算贡献资格；要求 NPC active、玩家处于 active range、`LifeMaximum > 0`、`ReleaseOwner == 255`，排除类型 `25/30/33`，并在 slime-rain NPC 场景使用 `0.65f` slot multiplier。`SumContributions` 只累加 Query 返回的 slot weight，不写 `nearbyActiveNPCs`、NPC 或 World 状态。
- 依赖影响：生产类型仅依赖 `System` 和显式只读输入；没有接入 `Main`/`NPC` 全局状态、aggro targeting、NPC registry、调度器、网络或持久化，也没有声明 NPC/World 的唯一 writer。
- 验证：通过 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`，退出码 `0`、警告 `0`、错误 `0`；产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerInteractionUiStateVerification\Debug\net10.0\Terraria.PlayerInteractionUiStateVerification.dll`。随后通过 wrapper 运行 `run --project .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj --no-build --no-restore`，退出码 `0`，输出 `PASS: player interaction state, reuse, name, and NPC pressure queries are valid`。
- 未验证项：Version4 `Main`/`NPC` 的真实清零、累加和读取顺序，active-range 的实际判定输入，slot cost 的真实来源，NPC 类型注册范围，`aggro` targeting/combat owner，跨分区唯一 writer、生命周期和行为等价仍未验证；本检查点只证明纯贡献计算。

### C04 PlayerItemUseIntent

状态：design-checkpoint-complete；`executionStatus: in-progress`、`implementationStatus: partial`、`verificationStatus: not-run`。

覆盖 `controlUseItem`、`controlUseTile`、`tileInteractAttempted`、`isOperatingAnotherEntity`、`lastItemUseAttemptSuccess`、`autoReuseAllWeapons`、`altFunctionUse`、`delayUseItem`。ItemUse 资格计算与成功结果分开，Tile/Entity 交互使用命令并定义拒绝/重试边界；不能把 ItemCheck 上下文塞进输入 component。建议拆为 intent、attempt marker、entity lock、result projection 和 transient mode/deferred action 五个窄边界。

#### C04 `PlayerItemUseIntent` core implementation checkpoint

- 实际修改文件：`src/Player/PlayerItemUseIntentInput.cs`、`src/Player/PlayerItemUseIntentComponent.cs`、`src/Player/PlayerItemUseIntentSystem.cs`、`src/PlayerInteractionUiStateVerification/Program.cs`。
- 核心行为：`PlayerItemUseIntentSystem.CreateResetState` 将 `ControlUseItem` 和 `ControlUseTile` 清零；`Advance` 在非 reset tick 逐项复制显式 intent，在 reset tick 丢弃输入并恢复默认值。该核心不消费 release edge，不写 Item/Tile/Entity 状态，也不发布成功结果。
- 依赖影响：生产代码只依赖本地值类型；没有修改既有 `InputIntentComponent`、`PlayerUseComponent`、`PlayerItemUseState` 或 Dome protocol/command writer，避免形成第二条运行时 authority route。
- 验证：red verifier 通过 wrapper 构建退出码 `1`，原因是 6 个待实现 C04 类型/成员缺失；实现后 wrapper build 退出码 `0`、警告 `0`、错误 `0`，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerInteractionUiStateVerification\Debug\net10.0\Terraria.PlayerInteractionUiStateVerification.dll`；随后 wrapper run `--project .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj --no-build --no-restore` 退出码 `0`，输出 `PASS: player interaction state, reuse, name, and NPC pressure queries are valid`。
- 未验证项：真实 client/protocol mapping、C02 release gate、Item/Tile/Wiring/Entity owner、attempt marker、entity lock、result projection、auto-reuse policy、alternate mode、deferred action、生命周期、唯一 writer 和 Version4 行为等价仍未验证；本检查点只证明显式 intent 的 reset/advance 计算。

#### C04 `PlayerEntityInteractionLockStateComponent` implementation checkpoint

- 实际修改文件：`src/Player/PlayerEntityInteractionLockStateComponent.cs`。
- 覆盖成员：`isOperatingAnotherEntity`（P10-828），类型为 `bool`，默认值为 `false`。
- 核心行为：组件只保存实体交互占用事实，不执行 claim/release、ItemCheck、Tile/Wiring/Entity 行为，也不负责成功/拒绝路径或生命周期清理。
- 依赖影响：仅新增 `Terraria.Player` 内的独立值组件；不修改 P09 `PlayerInteractionLockStateComponent` 的 `LockTileInteractionsTimer`，不新增第二个 ItemUse authority，也未修改 System、Query、Command、Adapter、Projection、网络或持久化 owner。
- 未验证项：唯一 writer、成功/拒绝路径、断线/死亡清理、调度顺序、生命周期和 Version4 行为等价仍未验证；跨子系统 owner 保持 `integration-review`。

### C05 PlayerChannelAndActionContext

状态：design-checkpoint-complete；`executionStatus: in-progress`、`implementationStatus: partial`、`verificationStatus: not-run`。

覆盖 `manaCost`、`fireWalk`、`channel`、`TagEffectState`、`IntentionGuesser`、`_channelShotCache`、`rabbitOrderFrame`、`creativeGodMode`。`manaCost` 是 query/result，`TagEffectState`/`IntentionGuesser`/`RabbitOrderFrameHelper` 是有行为的 helper adapter，`_channelShotCache` 是短期 channel expectation；`channel`、`fireWalk`、`creativeGodMode` 的最终 owner 需 integration-review。实现时先建 channel cancellation verifier，再决定是否需要任何持久 Component。

#### C05 `PlayerChannelCancellation` core implementation checkpoint

- 实际修改文件：`src/Player/PlayerChannelCancellationExpectation.cs`、`src/Player/PlayerChannelCancellationProjectileSnapshot.cs`、`src/Player/PlayerChannelCancellationAdapter.cs`、`src/PlayerInteractionUiStateVerification/Program.cs`。
- 核心行为：`PlayerChannelCancellationAdapter.Matches` 复刻 Version4 `ChannelCancelKey.Matches`：`AiStyle == 99 && Ai0 == -3f` 的特殊 projectile 直接匹配；否则要求 projectile type 和 index 同时等于 expectation。`Track` 只在 projectile type 与 expected type 相同的时候更新 expected index，并以新值返回，不修改输入对象或 `channel`。
- 依赖影响：核心只使用显式 expectation/projectile snapshot，不保存 Projectile 引用、不访问全局 projectile registry、不切换 `channel`、不执行网络/持久化或日志副作用；现有 `ProjectileIdentityComponent` 和 Item/Projectile systems 未被修改。
- 验证：red verifier 通过 wrapper 构建退出码 `1`，原因是 13 个待实现 C05 类型/成员缺失；实现后 wrapper build 退出码 `0`、警告 `0`、错误 `0`，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerInteractionUiStateVerification\Debug\net10.0\Terraria.PlayerInteractionUiStateVerification.dll`；随后 wrapper run `--project .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj --no-build --no-restore` 退出码 `0`，输出 `PASS: player interaction state, reuse, name, and NPC pressure queries are valid`。
- 未验证项：真实 Projectile entity identity/slot registry、Item/Projectile channel writer、channel start/update/cancel 调度、death/disconnect/CCed/noItems 清理、网络映射、`manaCost` 计算和 Version4 行为等价仍未验证；本检查点只证明显式 expectation 的纯匹配和追踪。

### C06 PlayerMovementAndWorldInteraction

状态：design-checkpoint-complete；`executionStatus: in-progress`、`implementationStatus: partial`、`verificationStatus: not-run`。

覆盖 `hostile`、`hermesStepSound`、`instantMovementAccumulatedThisFrame`、`lastCreatureHit`、`ActuationRodLock`。移动累积器由移动阶段唯一写入；音频只输出 projection；hostile、lastCreatureHit 和 ActuationRodLock 分别与 Combat/Network、Combat、Wiring/World 交接。执行顺序必须显式写入 movement frame contract，不能依赖文件顺序。

#### C06 `PlayerInstantMovementAccumulator` core implementation checkpoint

- 实际修改文件：`src/Player/PlayerInstantMovementAccumulatorInput.cs`、`src/Player/PlayerInstantMovementAccumulatorComponent.cs`、`src/Player/PlayerInstantMovementAccumulatorSystem.cs`、`src/PlayerInteractionUiStateVerification/Program.cs`。
- 核心行为：`PlayerInstantMovementAccumulatorSystem.CreateFrameState` 创建零向量帧状态；`BeginFrame` 将上一帧累积值清零；`Accumulate` 对显式 `Vector2 MovementDelta` 做逐分量累加。该核心不读取 Player、绳索/滑轮、时钟或全局 World，也不消费或发布移动结果。
- 依赖影响：生产类型只依赖 `System.Numerics.Vector2` 和本地值类型；组件是 movement frame accumulator 的唯一局部 writer，未修改现有 MovementIntent/PlayerControl、协议、Combat、音频、Wiring 或 World owner，不新增网络/持久化字段。
- 验证：red verifier 通过 wrapper 构建退出码 `1`，原因是 7 个待实现 C06 类型/成员缺失；实现后 wrapper build 退出码 `0`、警告 `0`、错误 `0`，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerInteractionUiStateVerification\Debug\net10.0\Terraria.PlayerInteractionUiStateVerification.dll`；确认产物存在后，wrapper run `--project .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj --no-build --no-restore` 退出码 `0`，输出 `PASS: player interaction state, reuse, name, and NPC pressure queries are valid`。
- 未验证项：Version4 的真实 movement frame scheduler、rope/pulley 条件与顺序、累加器消费点、跨系统唯一 writer、spawn/teleport/disconnect 清理和行为等价仍未验证；`PlayerActuationRodLockStateComponent.IsActuationRodLocked` 字段已实现但未接入，C06 的 `hostile`、step-sound 播放/装备 writer、`lastCreatureHit`、Wiring/World lock writer 和生命周期仍保持 integration-review/evidence-gap。

#### C06 `PlayerActuationRodLockStateComponent` implementation checkpoint

- Actual modified file: `src/Player/PlayerActuationRodLockStateComponent.cs`.
- Core behavior: `PlayerActuationRodLockStateComponent` stores only the P10 `ActuationRodLock` boolean state. `IsActuationRodLocked` defaults to `false` through struct initialization; the component does not acquire or release the lock and contains no Wiring/World behavior.
- Dependency impact: the component only depends on value-type boundaries in the `Terraria.Player` project; it does not modify Tile/Wiring/World owners, movement systems, network, persistence, or existing Player state, and it does not add a lock System, Adapter, or Command.
- Unverified: the Wiring/World unique writer, action begin/end, rejection/completion paths, death/disconnect cleanup, cross-system scheduling, and Version4 behavior equivalence remain unverified. Component status is `not-verified`; cross-subsystem ownership remains `integration-review`.

#### C06 `PlayerStepSoundProjection` core implementation checkpoint

- 实际修改文件：`src/Player/PlayerStepSoundInput.cs`、`src/Player/PlayerStepSoundProjection.cs`、`src/Player/PlayerStepSoundQuery.cs`、`src/PlayerInteractionUiStateVerification/Program.cs`。
- 核心行为：`PlayerStepSoundQuery.Calculate` 对显式腿部装备 ID生成不可变 projection；普通装备返回 Version4 ResetEffects 的 `SoundType=17`、`SoundStyle=-1`、`IntendedCooldown=9`，腿部装备 ID `140` 返回 `2/24/6`。Query 不播放声音、不更新 cooldown、不读取 Player 或 Equipment 全局状态。
- 依赖影响：生产类型只依赖本地值类型；`PlayerStepSoundProjection` 是音频 payload，不成为 Player 权威状态，不修改 Movement、Equipment、Audio、网络或持久化 owner，也不引入 `SoundPlaySet` 外部类型。
- 验证：red verifier 通过 wrapper 构建退出码 `1`，原因是 6 个待实现 C06 类型/成员缺失；实现后 wrapper build 退出码 `0`、警告 `0`、错误 `0`，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerInteractionUiStateVerification\Debug\net10.0\Terraria.PlayerInteractionUiStateVerification.dll`，确认产物存在后 wrapper run `--project .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj --no-build --no-restore` 退出码 `0`，输出 `PASS: player interaction state, reuse, name, and NPC pressure queries are valid`。
- 未验证项：完整 `SoundPlaySet` 兼容字段、其他装备/Mod 内容映射、step-sound 播放 port、cooldown 消费、movement phase 顺序、网络/持久化和 Version4 行为等价仍未验证；本检查点只验证两组已确认的纯常量映射。

### C07 PlayerNavigationAndTimeInstruments

状态：design-checkpoint-complete；导航仪器能力事实 reset/apply core 已实现并保存；`executionStatus: in-progress`、`implementationStatus: partial`、`verificationStatus: verified-focused`。

覆盖 `accCompass`、`accWatch`、`accWatchTime`、`accDepthMeter`、`accWeatherRadio`、`accCalendar`、`accStopwatch`。用显式 tick/time snapshot 计算，防止 Component 直接调用系统时钟；清理、装备变化和断线生命周期要有 verifier。仪器能力事实与 UI 显示值分离，未有 persistence/network evidence 前不扩展 schema。

#### C07 `PlayerNavigationInstrument` core implementation checkpoint

- 实际修改文件：`src/Player/PlayerNavigationInstrumentInput.cs`、`src/Player/PlayerNavigationInstrumentComponent.cs`、`src/Player/PlayerNavigationInstrumentSystem.cs`、`src/PlayerInteractionUiStateVerification/Program.cs`。
- 核心行为：`CreateResetState`/`BeginFrame` 清零 `CompassLevel`、`WatchLevel`、`DepthMeterLevel` 并关闭三个 boolean capability；`ApplyAccessoryType` 按 Version4 已确认的 watch 别名、单项仪器、组合导航、PDA/phone、天气、日历和秒表装备 ID 生成六项显式能力事实，等级使用最大值合并。
- `accWatchTime` 未实现；没有直接调用系统时钟或创建 Stopwatch 句柄，也没有把 capability facts 接入 Equipment、Player、World、网络或持久化 authority。
- 依赖影响：只新增 `Terraria.Player` 内的值输入、能力组件和 reset/apply system，以及 focused verifier 断言；不修改其他分区文件或现有公共 API。
- 验证：红测 wrapper build 退出码 `1`、警告 `0`、错误 `13`，原因是 C07 类型/成员尚不存在；最终 wrapper build 命令为 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`，退出码 `0`、警告 `0`、错误 `0`；产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerInteractionUiStateVerification\Debug\net10.0\Terraria.PlayerInteractionUiStateVerification.dll`。运行命令为 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 run --project .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj --no-build --no-restore`，退出码 `0`，输出 `PASS: player interaction state, reuse, name, and NPC pressure queries are valid`。
- 未完成/未验证：真实 Equipment/ability writer、跨玩家聚合、ResetEffects 与 spawn/death/disconnect 生命周期、`accWatchTime` 的显式 time/world snapshot projection、UI/网络/持久化读取和 Version4 行为等价仍未验证；因此不能将 C07 父组件或 P10 分区标记为完成。

### C08 PlayerDetectionAndWiringInstruments

状态：design-checkpoint-complete；检测能力事实 reset/apply core 已实现并保存；`executionStatus: in-progress`、`implementationStatus: partial`、`verificationStatus: verified-focused`。

覆盖 `accFishFinder`、`accJarOfSouls`、`accThirdEye`、`accThirdEyeCounter`、`accOreFinder`、`accCritterGuide`、`accDreamCatcher`、`InfoAccMechShowWires`。能力事实、计数器和 wiring overlay 分离；ResetEffects 与装备提交顺序必须可重复。实际检测扫描和接线变更不归本组件 writer 所有。

#### C08 `PlayerDetectionInstrument` core implementation checkpoint

- 实际修改文件：`src/Player/PlayerDetectionInstrumentInput.cs`、`src/Player/PlayerDetectionInstrumentComponent.cs`、`src/Player/PlayerDetectionInstrumentSystem.cs`、`src/PlayerInteractionUiStateVerification/Program.cs`。
- 核心行为：`CreateResetState`/`BeginFrame` 清零六项检测能力事实；`ApplyAccessoryType` 按 Version4 已确认的单项、检测组合、秒表组合、PDA/phone 装备 ID生成 fish finder、third eye、jar of souls、critter guide、ore finder 和 dream catcher 能力。
- `PlayerDetectionInstrumentComponent.ThirdEyeCounter` 已保存为 `byte` 组件字段，默认值为 `0`；本次只落地字段所有权，不实现递增、上限或消费逻辑。`InfoAccMechShowWires` 未实现，仍由 Wiring/UI integration-review owner 决定。`accDreamCatcher` 只保留 capability fact，不写 DPS/Combat 状态。
- 依赖影响：生产代码只新增显式值输入、能力事实组件和 reset/apply system；没有读取 World/NPC/Projectile registry，没有修改 Equipment/Player/Wiring/Combat/网络/持久化 owner。
- 验证：红测 wrapper build 退出码 `1`、警告 `0`、错误 `14`，原因是 C08 类型/成员尚不存在；最终 wrapper build 命令为 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`，退出码 `0`、警告 `0`、错误 `0`；产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerInteractionUiStateVerification\Debug\net10.0\Terraria.PlayerInteractionUiStateVerification.dll`。运行命令为 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 run --project .\src\PlayerInteractionUiStateVerification\Terraria.PlayerInteractionUiStateVerification.csproj --no-build --no-restore`，退出码 `0`，输出 `PASS: player interaction state, reuse, name, and NPC pressure queries are valid`。
 - 未完成/未验证：真实 Equipment/ability writer、third-eye counter 递增/上限/消费、World detection snapshot、Wiring overlay reader、Combat/DPS owner、spawn/death/disconnect 生命周期、网络/持久化读取和 Version4 行为等价仍未验证；`PlayerDetectionInstrumentComponent.ThirdEyeCounter` 字段已保存但没有运行时 writer/consumer，因此 C08 组件只标记为 `partial`，不能将 C08 父组件或 P10 分区标记为完成。

### C09 PlayerBuilderInteractionDefinitions

状态：design-checkpoint-complete；静态 definitions/query 和 focused verifier 已实现并保存；`executionStatus: in-progress`、`implementationStatus: partial`、`verificationStatus: verified-focused`。

覆盖 `BuilderAccToggleIDs.RulerLine`、`RulerGrid`、`AutoActuate`、`AutoPaint`、`WireVisibility_Red`、`WireVisibility_Green`、`WireVisibility_Blue`、`WireVisibility_Yellow`、`HideAllWires`、`WireVisibility_Actuators`、`BlockSwap`、`TorchBiome`、`Count`。保持只读 catalog/definition，不注册每玩家 component；数值稳定性由定义 verifier 保护。不要把未列入 P10 的 `builderAccStatus` 直接扩展为本检查点的成员库存。

### C10 PlayerSelectionState

状态：design-checkpoint-complete；选择状态纯 Query core 已实现并保存；`executionStatus: in-progress`、`implementationStatus: partial`、`verificationStatus: verified-focused`。

覆盖 `selectedItemState`、`selectedKite`、`SelectedItemState` 的 `player`、`selected`、`hotbar`、`buffered`、`overridden`、`SelectionRadial` 的 `_SelectedBinding`、`RadialCount`、`Bindings`、`Mode`，以及 6 个 properties `CanChangeSelectedItemImmediately`、`Selected`、`Hotbar`、`HasActiveOverride`、`HasBufferedChange`、`LastNonOverridenSelection`。选择 authority 与 P09 现有 `SelectedItemComponent` 必须先决策，radial bindings 与 selection command 不得成为网络 DTO 的隐式写者；本检查点不创建第二个库存选择 authority。

#### C10 `PlayerSelectionQuery` core implementation checkpoint

- 实际修改文件：`src/Player/PlayerSelectionStateInput.cs`、`src/Player/PlayerSelectionStateSnapshot.cs`、`src/Player/PlayerSelectionQuery.cs`、`src/PlayerSelectionVerification/Program.cs`、`src/PlayerSelectionVerification/Terraria.PlayerSelectionVerification.csproj`。
- 核心行为：`PlayerSelectionQuery.Evaluate` 只读取显式 selected/hotbar/buffered/overridden 与 ItemUse/time facts，保持 Version4 的 `CanChangeSelectedItemImmediately`、`HasActiveOverride`、`HasBufferedChange` 和 `LastNonOverridenSelection` 纯派生语义；不持有 `Player` 引用、不提交 selection、不修改 Inventory/P09 authority。
- 依赖影响：生产类型只依赖 `Terraria.Player` 本地值类型；没有新增 P09 selection writer、radial registry、selectedKite equipment writer、协议、网络或持久化引用。
- 验证：红测 wrapper build 退出码 `1`、警告 `0`、错误 `14`，原因是待实现 C10 输入/快照/Query 类型缺失；实现后通过 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\PlayerSelectionVerification\Terraria.PlayerSelectionVerification.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`，退出码 `0`、警告 `0`、错误 `0`，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerSelectionVerification\Debug\net10.0\Terraria.PlayerSelectionVerification.dll`；随后 wrapper run `--project .\src\PlayerSelectionVerification\Terraria.PlayerSelectionVerification.csproj --no-build --no-restore` 退出码 `0`，输出 `PASS: player selection query preserves Version4 derived semantics`。
- 未验证项：P09 `SelectedItemComponent` 唯一 writer、slot/radial legality、`SelectionRadial` registry、`selectedKite` equipment/reset writer、Player entity reference、packet/UI duplicate/replay policy、ItemCheck 消费顺序、生命周期、网络/持久化和 Version4 完整行为等价仍未验证。
- 回滚：删除本检查点的三个生产文件和 focused verifier 即可恢复现有 selection authority 及旧 reader；不修改 P09 选择实现。

### C11 PlayerInputSyncAndMatchAdapters

状态：design-checkpoint-complete；输入同步 projection core 已实现并保存；`executionStatus: in-progress`、`implementationStatus: partial`、`verificationStatus: verified-focused`。

覆盖 `PlayerInputSyncCache` 的 5 个 control 字段和 `PressingAnyInput`、`ChannelCancelKey.ProjectileTypeExpected`/`ProjectileIndexExpected`、`SetMatchRequest.Player`/`Head`/`Body`/`Legs`/`ArmorSlotRequested`/`Male`。分别验证 projection snapshot、channel cancellation payload 和 match command 的生命周期、ownership、重复包和网络失败；均不自动成为持久组件。当前 session/projection 没有闭合 replay sequence，执行计划不伪造该能力。

#### C11 `PlayerInputSyncProjection` core implementation checkpoint

- 实际修改文件：`src/Player/PlayerInputSyncCacheInput.cs`、`src/Player/PlayerInputSyncProjection.cs`、`src/Player/PlayerInputSyncQuery.cs`、`src/PlayerInputSyncVerification/Program.cs`、`src/PlayerInputSyncVerification/Terraria.PlayerInputSyncVerification.csproj`。
- 核心行为：`PlayerInputSyncQuery.Evaluate` 只读取五个显式 committed control facts，输出不可变同步 projection，并按 Version4 `PlayerInputSyncCache.PressingAnyInput` 语义计算任一输入；不写回 raw input、不访问网络、不引入 sequence/replay 或 SetMatch authority。
- 依赖影响：生产类型只依赖 `Terraria.Player` 本地值类型；没有修改 `LegacyPlayerControlsProjection`、`TerrariaSession`、`DomeNetworkUpdateBridge` 或其他协议/网络 owner。
- 验证：红测 wrapper build 退出码 `1`、警告 `0`、错误 `11`，原因是待实现 C11 输入/投影/Query 类型缺失；实现后通过 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\PlayerInputSyncVerification\Terraria.PlayerInputSyncVerification.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`，退出码 `0`、警告 `0`、错误 `0`，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerInputSyncVerification\Debug\net10.0\Terraria.PlayerInputSyncVerification.dll`；随后 wrapper run `--project .\src\PlayerInputSyncVerification\Terraria.PlayerInputSyncVerification.csproj --no-build --no-restore` 退出码 `0`，输出 `PASS: player input sync projection preserves Version4 cache semantics`。
- 未验证项：per-session sequence/replay/permission、重复/乱序/重放拒绝、channel expectation 生命周期、SetMatch payload、外观匹配 owner、disconnect/reconnect session 清理、网络失败语义、持久化和 Version4 完整行为等价仍未验证。
- 回滚：删除本检查点的三个生产文件和 focused verifier 即可恢复现有 legacy input projection；不修改协议或网络 authority。

### C12 PlayerBuilderOverlayProjection

状态：design-checkpoint-complete；纯投影核心已实现并通过 focused verifier；`executionStatus: in-progress`、`implementationStatus: partial`、`verificationStatus: verified-focused`。

覆盖 `rulerGrid`、`rulerLine`。从 builder facts 和 selection/input snapshot 生成单向 overlay；UI/renderer 不能写回 C09 definition 或权威输入。两个字段应保持 transient projection 语义，不扩展存档/复制 schema。

#### C12 实现检查点

- 实际修改文件：`src/Player/PlayerBuilderOverlayInput.cs`、`src/Player/PlayerBuilderOverlayProjection.cs`、`src/Player/BuilderOverlayQuery.cs`、`src/PlayerBuilderOverlayVerification/Program.cs`、`src/PlayerBuilderOverlayVerification/Terraria.PlayerBuilderOverlayVerification.csproj`。
- 核心行为：`BuilderOverlayQuery.Calculate` 对显式 builder facts 做确定性、无副作用投影，返回 `rulerGrid`/`rulerLine` 两个不可变输出；不反向写 C09 catalog 或玩家 authority。
- 依赖影响：没有接入 Equipment、ResetEffects、Tile/Wiring、renderer、网络或持久化，生产项目无新增引用。
- 验证：通过 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\PlayerBuilderOverlayVerification\Terraria.PlayerBuilderOverlayVerification.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`；退出码 `0`，警告 `0`，错误 `0`；产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerBuilderOverlayVerification\Debug\net10.0\Terraria.PlayerBuilderOverlayVerification.dll`。随后通过 wrapper 运行 `run --project .\src\PlayerBuilderOverlayVerification\Terraria.PlayerBuilderOverlayVerification.csproj --no-build --no-restore`，退出码 `0`，输出 `PASS: builder overlay projection is deterministic and one-way`。
- 未验证项：Version4 reset defaults、装备 writer、renderer reader、生命周期和网络/持久化边界仍未验证；focused verifier 仅覆盖显式 facts 的确定性传递、默认 false 输入和无写回投影。

### C13 PlayerItemSpaceAndSettings

状态：design-checkpoint-complete；ItemSpace eligibility pure Query core 和 explicit-facts evaluation Query core 已实现并保存；`executionStatus: in-progress`、`implementationStatus: partial`、`verificationStatus: verified-focused`。

覆盖 `ItemSpaceStatus.CanTakeItem`、`ItemIsGoingToVoidVault`、`CanTakeItemToPersonalInventory` 和 `Settings.DashControl`。前三项是 Item/Container 资格结果，后者是 Settings Adapter；纯 Query 在显式 item/container facts 上验证 Version4 void-vault fallback、容量/stack/ammo/unique-item 扫描顺序，settings adapter 验证用户配置默认值，不创建第二个 inventory/dash owner。已有 `PlayerVoidVaultStateComponent` 必须作为 integration input，不复制 authority。

#### C13 `PlayerItemSpaceEligibilityQuery` core implementation checkpoint

- 实际修改文件：`src/Player/PlayerItemSpaceInput.cs`、`src/Player/PlayerItemSpaceSnapshot.cs`、`src/Player/PlayerItemSpaceQuery.cs`、`src/Player/PlayerPersonalInventoryEligibilityQuery.cs`、`src/PlayerItemSpaceVerification/Program.cs`、`src/PlayerItemSpaceVerification/Terraria.PlayerItemSpaceVerification.csproj`。
- 核心行为：`PlayerItemSpaceQuery.Evaluate` 只复制显式 `CanTakeItem`/`ItemIsGoingToVoidVault` 资格事实到不可变快照；`PlayerPersonalInventoryEligibilityQuery.CanTakeItemToPersonalInventory` 严格计算 `CanTakeItem && !ItemIsGoingToVoidVault`，不写库存、不读取 Item/Container/VoidVault 外部状态。
- 依赖影响：生产类型只依赖 `Terraria.Player` 本地值类型；没有创建 `PlayerVoidVaultStateComponent`、Item、Inventory 或 Settings authority，也没有修改现有 inventory/void-vault command、网络、持久化或 C01/C02 input owner。
- 验证：红测 wrapper build 退出码 `1`、警告 `0`、错误 `13`，原因是 C13 输入/快照/Query 类型尚不存在；实现后通过 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\PlayerItemSpaceVerification\Terraria.PlayerItemSpaceVerification.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`，退出码 `0`、警告 `0`、错误 `0`，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerItemSpaceVerification\Debug\net10.0\Terraria.PlayerItemSpaceVerification.dll`；随后 wrapper run `--project .\src\PlayerItemSpaceVerification\Terraria.PlayerItemSpaceVerification.csproj --no-build --no-restore` 退出码 `0`，输出 `PASS: item-space eligibility preserves Version4 derived semantics`。
- 未验证项：Version4 `ItemSpace` 的完整 inventory/ammo/stack/unique-item 扫描、VoidVault capability 与唯一 writer、个人库存 commit、Settings/profile source、`DashControl` default/config round-trip、网络/持久化、运行时调度和完整行为等价仍未验证。
- 回滚：删除本检查点的四个生产 Query/value 文件和 focused verifier 即可恢复现有 Items/Inventory/VoidVault reader 路径；不修改其他分区 owner。

#### C13 `PlayerSettingsAdapter` core implementation checkpoint

- 实际修改文件：`src/Player/PlayerDashControlPreference.cs`、`src/Player/PlayerDashControlSettingsInput.cs`、`src/Player/PlayerDashControlSettingsSnapshot.cs`、`src/Player/PlayerSettingsAdapter.cs`、`src/PlayerItemSpaceVerification/Program.cs`。
- 核心行为：`PlayerSettingsAdapter.ReadDashControl` 对显式可选设置执行确定性适配；缺省值为 Version4 `AllowDoubleTap`，显式 `OnlyThroughHotkeys` 原样往返。适配器不保存静态可变配置，不写 `Settings.DashControl`，不读取或修改 C01 `controlDash`/C02 release state。
- 依赖影响：新增类型仅依赖 `Terraria.Player` 本地 enum/value 类型；没有接入 profile/config I/O、持久化、dash system、网络或其他分区 owner。
- 验证：红测 wrapper build 退出码 `1`、警告 `0`、错误 `12`，原因是 DashControl input/snapshot/preference/adapter 类型尚不存在；实现后复用 C13 focused verifier 构建命令退出码 `0`、警告 `0`、错误 `0`，产物保持 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerItemSpaceVerification\Debug\net10.0\Terraria.PlayerItemSpaceVerification.dll`；wrapper run `--project .\src\PlayerItemSpaceVerification\Terraria.PlayerItemSpaceVerification.csproj --no-build --no-restore` 退出码 `0`，输出 `PASS: item-space eligibility preserves Version4 derived semantics`。
- 未验证项：真实 Settings/profile reader、非法 enum 值策略、配置写入/持久化、dash runtime consumer、C01/C02 与设置 policy 的集成和完整行为等价仍未验证。
- 回滚：删除本检查点的四个 settings adapter/value 文件并移除 verifier 中的设置断言即可恢复现有设置边界；不修改 C01/C02 或其他分区 owner。

#### C13 `PlayerItemSpace` explicit-facts evaluation Query implementation checkpoint

- 实际修改文件：`src/Player/PlayerItemSpaceCandidate.cs`、`src/Player/PlayerItemSpaceSlotSnapshot.cs`、`src/Player/PlayerItemSpaceEvaluationInput.cs`、`src/Player/PlayerItemSpaceQuery.cs`、`src/PlayerItemSpaceVerification/Program.cs`。
- 核心行为：新增重载只接收不可变候选物品、完整 inventory slot snapshot、VoidVault slot snapshot 和显式 capability facts；按 Version4 顺序处理 pickup short-circuit、unique-stack rejection、普通槽位 0-49/coin 0-53、ammo 54-57、ammo stack fallback 和 VoidVault fallback，并返回 `CanTakeItem`/`ItemIsGoingToVoidVault` 快照。slot acceptance 保留空槽、favorited-only-one、最大堆叠和 type/prefix stack compatibility 规则；不持有 Item 引用、不修改容器。
- 依赖影响：生产项目仍只依赖 `Terraria.Player`，未把 `Terraria.Items` 或 `Terraria.Content` 直接耦合进 Player；真实 catalog、inventory、ammo 和 VoidVault owner 通过后续 integration adapter 提供显式 facts。
- 验证：先更新 focused verifier 并通过 wrapper build 得到预期红测：退出码 `1`，4 个错误，0 个警告，原因是新增 Query 输入类型不存在；实现后通过 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\PlayerItemSpaceVerification\Terraria.PlayerItemSpaceVerification.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`，退出码 `0`、警告 `0`、错误 `0`，产物为 `D:\TRbackup\NLTX\Build\bin\Terraria.PlayerItemSpaceVerification\Debug\net10.0\Terraria.PlayerItemSpaceVerification.dll`；随后新增 Version4 `HasItem` 的 `0..57` 边界断言，首次 wrapper run 退出码 `1` 并报告 trash slot 被错误计入，修正后再次使用同一串行 build 命令退出码 `0`、警告 `0`、错误 `0`，再以 `run --project .\src\PlayerItemSpaceVerification\Terraria.PlayerItemSpaceVerification.csproj --no-build --no-restore` 运行退出码 `0`，输出 `PASS: item-space eligibility preserves Version4 derived semantics`。
- 未验证项：真实 `ItemID.Sets.IsAPickup`/catalog 映射、`HasItem` 的 inventory snapshot producer、coin/ammo slot layout producer、`CanVoidVaultAccept` writer、个人库存 commit、Settings/profile、网络/持久化、调度和完整行为等价仍未验证；focused verifier 已覆盖显式 facts 的 pickup、unique-stack、coin、ammo、VoidVault 和 trash-slot boundary 分支。
- 回滚：删除本检查点新增的三个输入值文件、移除 `PlayerItemSpaceQuery` 的 evaluation overload 和 focused verifier 新增断言即可恢复此前的 eligibility query core；不修改其他分区 owner。

## 6. 验证、构建与输出策略

所有 compile-capable command 均须在仓库根目录、检查 `dotnet.exe`/`csc.exe` 无并发 owner 后，通过 `Build/Tools/Invoke-SerialDotnet.ps1` 串行执行；C01/C02 的 Player 构建和 C09 的 focused verifier 构建/运行记录在对应检查点。后续组件仍须记录 command、project、exit code、warning/error 数和 `Build/bin` artifact path，并用 `--no-build --no-restore` 运行已构建 verifier。

每个检查点的回滚单位是该检查点新增的 writer、adapter、query 和 reader route；不得 broad cleanup，不得修改其他分区文档或 ledger。用户明确禁止的 `git diff --check -- <两份 P10 文档>` 不执行；验收改用文件存在性、元数据、成员计数、重复序号、路径范围和状态字段检查。

## 7. C01-C13 当前检查点

已保存 C01-C13 的设计与执行计划，并完成 C01、C02 隔离核心、C02 release/repeat focused verifier、C03 UI/交互门控、name definition/query、ItemUse 复用资格 Query 和 NPC 压力贡献 Query、C04 ItemUse/tile intent core、C05 channel cancellation expectation Adapter core、C06 instant-movement accumulator core 和 step-sound projection core、C07 导航仪器能力事实 reset/apply core、C08 检测仪器能力事实 reset/apply core、C09 定义/query 核心、C10 选择状态纯 Query core、C11 输入同步 projection core、C12 纯投影核心及 C13 item-space eligibility、DashControl settings adapter、explicit-facts evaluation Query cores。C03 其余成员、C04 其余边界、C05 的 channel/mana/helper/network 边界、C06 的 movement scheduler/hostile/sound playback/combat/wiring 边界、C07 的 `accWatchTime`/time snapshot/Equipment writer/lifecycle 边界、C08 的 counter/Wiring/World/Combat owner、C10 的 P09 selection authority/radial/selectedKite/网络 command 边界、C11 的 channel expectation/SetMatch/per-session policy 边界、C13 的真实 catalog/snapshot producer、VoidVault/settings owner 和 runtime integration 尚未完成；C01 尚未完成 focused verifier、运行时、网络验证或持久化验证，C02 focused verifier 和 C03、C04、C05、C06、C07、C08、C09、C10、C11、C12、C13 focused verifier 已完成并通过；分区级 `verificationStatus: not-run` 保持不变。当前 `completedComponents` 记录以文档顶部为准，`currentComponent` 为 C13，`pendingComponents` 为 C03-C08、C10-C11、C13。

## 8. 本次继续执行验证记录

- 活动进程检查：执行构建前未发现活动的 `dotnet.exe` 或 `csc.exe` 编译进程。
- 命令：`pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 build .\src\Player\Terraria.Player.csproj /m:1 /nr:false /p:UseSharedCompilation=false /p:MSBuildNodeReuse=false /p:BuildInParallel=false`。
- 项目：`D:\TRbackup\NLTX\src\Player\Terraria.Player.csproj`；目标框架 `net10.0`。
- 结果：退出码 `1`；警告 `0`；错误 `5`。错误来自既有 `src/Player/Progression/PlayerMinionCapacityCommitSystem.cs` 和 `src/Player/Progression/SubmitMinionCapacityDeltaCommand.cs`，缺失 `Terraria.Relationships`、`Terraria.Projectile`、`EntityReference` 和 `ProjectileIdentityComponent`；本次没有修改这些非组件文件。
- 输出：期望产物 `D:\TRbackup\NLTX\Build\bin\Terraria.Player\Debug\net10.0\Terraria.Player.dll` 的路径存在旧 DLL，最后写入时间为 `2026-09-12T02:04:19.6756303Z`，本次失败构建未生成或刷新该文件；因此 `PlayerEntityInteractionLockStateComponent` 没有获得当前编译验证，分区 `verificationStatus` 继续保持 `not-run`。
- 本轮结算：已使用当前绑定 `dfcf6281bf7d4ac2804704e61407153d` 执行 `Fail`；runner 返回 `status=failed`、`partition=P10`、`lockReleased=true`，命令退出码为 `1`。失败原因是组件-only 范围无法闭合实体 claim/release、ItemCheck、Tile/Wiring/Entity 生命周期和唯一 writer，且 `Terraria.Player` 项目仍被上述 5 个既有缺失符号阻塞。

## 9. 本轮重试最终状态

- 当前有效 session 为 `dfcf6281bf7d4ac2804704e61407153d`；runner ledger 已结算为 `P10=failed/manual`，不是旧 session 的状态延续。
- 当前有效组件为 `C04_PlayerEntityInteractionLockState_integration`，已保存 `src/Player/PlayerEntityInteractionLockStateComponent.cs`，仅覆盖 `P10-828 isOperatingAnotherEntity: bool` 的独立事实字段。
- 该组件没有 claim/release、ItemCheck、Tile/Wiring/Entity 行为、成功/拒绝路径、断线/死亡清理或唯一 writer；`executionStatus=blocked`、`implementationStatus=partial`、`verificationStatus=not-run`。
