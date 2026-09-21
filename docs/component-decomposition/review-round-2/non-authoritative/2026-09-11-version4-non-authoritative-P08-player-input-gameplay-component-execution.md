# P08 非权威组件实施计划：玩家输入与玩法

> 本文是 P08 的实施记录与后续计划。剩余目标文件、类型、System、Query、Command、Adapter 和 Projection 均为 proposed；全部 26 个组件单元已保存到 `src2`。生产项目和 verifier 项目已完成串行构建，但现有 verifier 在 Equipment 非 Component 断言处失败，尚未完成行为等价验证。

partitionId: P08
sessionId: dc9ebd50d1dd49e39b074ca3b7f5001d
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\08-player-input-gameplay.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P08-player-input-gameplay-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P08-player-input-gameplay-component-execution.md
designStatus: proposed
executionStatus: planned
implementationStatus: in-progress
verificationStatus: failed
completedComponents:
- MainInputAndThreadScheduling
- MainScreenAndInputState
- MainPlayerAndSpawnState
- MainMenuAndInputSettings
- MainInputAndEventFlags
- MainDerivedInputAndPresentationQueries
- SharedGolfState
- SharedControlFocusHelpers
- SharedCreativePowerRuntimeManager
- SharedCreativeUnlockProgress
- SharedSmartInteractionQueries
- PlayerItemPickupAndRespawnState
- PlayerPreviewAndRejectionState
- DoorOpeningInteractionState
- SmartCursorInteractionState
- PressurePlateInteractionState
- CursorAndChestInteractionState
- InputProfilesAndConfiguration
- InputTriggerState
- PlayerInputRuntimeState
- EquipmentLoadoutState
- SharedCreativePowerContracts
- PlayerMovementCapabilityState
- PlayerIntentAndInteractionState
- SharedCreativePerPlayerPowerState
- SharedCreativeSharedPowerState
currentComponent: verification
pendingComponents: []
lastCheckpointUtc: 2026-09-12T07:13:45.1036844Z
implementationCheckpoint:
- component: SharedCreativePerPlayerPowerState and SharedCreativeSharedPowerState
- sourceStatus: saved-under-src2
- sourceRoot: D:\TRbackup\NLTX\src2
- files:
  - D:\TRbackup\NLTX\src2\PlayerInputGameplay\Terraria.PlayerInputGameplay.csproj
  - D:\TRbackup\NLTX\src2\PlayerInputGameplay\Runtime\MainInputFrameComponent.cs
  - D:\TRbackup\NLTX\src2\PlayerInputGameplay\Runtime\MainFrameTimingComponent.cs
  - D:\TRbackup\NLTX\src2\PlayerInputGameplay\Runtime\MainThreadActionQueue.cs
  - D:\TRbackup\NLTX\src2\PlayerInputGameplay\Runtime\RuntimeDiagnosticsSettingsComponent.cs
  - D:\TRbackup\NLTX\src2\PlayerInputGameplay\Runtime\RenderRequestComponent.cs
  - D:\TRbackup\NLTX\src2\PlayerInputGameplay\Runtime\TownNpcSpawnEligibilityProjection.cs
  - D:\TRbackup\NLTX\src2\PlayerInputGameplay\Runtime\MainInputFrameSystem.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Presentation\ScreenViewportComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Presentation\ScreenInputSettingsComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Presentation\InputTextCaptureAdapter.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Presentation\CursorPresentationStateComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Presentation\PointerTransitionStateComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Interaction\CursorItemSnapshot.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Interaction\CursorItemPreviewComponent.cs
  - D:\TRbackup\NLTX\src2\PlayerInputGameplay\Presentation\ScreenInputStateSystem.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Player\PlayerRegistryAdapter.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Player\LocalPlayerSelectionComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Player\GameplayHostEligibilityComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Player\PlayerSpawnPointComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Input\InputPreferenceComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Input\SmartCursorPreferenceComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Input\RuntimeAllocationSettingsAdapter.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Runtime\InputGateComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Runtime\FramePhaseComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Runtime\GameplayEventFlagsComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Runtime\FallingStarEventStateComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Runtime\WeatherEventModifierComponent.cs
  - D:\TRbackup\NLTX\src2\PlayerInputGameplay\Presentation\MeteorFallPresentationProjection.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Presentation\ViewportScaleComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Presentation\CursorCoordinateQuery.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Presentation\WeatherPresentationQuery.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Presentation\NetDiagnosticsPresentationAdapter.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Presentation\SceneMetricsSnapshot.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Presentation\PlayerSceneMetricsProjection.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Presentation\MainDerivedPresentationQuery.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Golf\GolfRulesDefinition.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Golf\GolfBallTrackRecord.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Golf\GolfLocalTrackingComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Golf\GolfTrackingSystem.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Golf\GolfCameraProjection.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Focus\WindowFocusAdapter.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Focus\GameplayActivityQuery.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Focus\MouseVisibilityPort.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Creative\CreativePowerPermissionLevel.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Creative\CreativePowerContract.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Creative\CreativePowerNetworkAdapter.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Creative\CreativePowerRegistryComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Creative\CreativePowerRegistrySystem.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Creative\CreativeSacrificeCatalogComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Creative\CreativeUnlockProgressComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Creative\CreativeUnlockProjection.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Interaction\SmartInteractionScanSettings.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Interaction\SmartInteractionCandidateSnapshot.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Interaction\SmartInteractionCandidateBuffer.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Interaction\SmartInteractionTileTargets.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Interaction\SmartInteractionProviderRegistry.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Interaction\SmartInteractionSystem.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Pickup\MinionRespawnComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Pickup\PlayerItemPickupLogEntry.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Pickup\PlayerItemPickupLogComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Presentation\PlayerRejectionPresentationComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Presentation\CharacterPreviewSettingsComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplayVerification\Terraria.PlayerInputGameplayVerification.csproj
  - D:\TRbackup\NLTX\src2\PlayerInputGameplayVerification\Program.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Doors\DoorOpeningTypes.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Doors\DoorOpeningStateComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Doors\DoorOpeningHandlerAdapter.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Doors\DoorOpeningSystem.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\SmartCursor\SmartCursorUsageInfo.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\SmartCursor\SmartCursorTargetBuffer.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\SmartCursor\SmartCursorSystem.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\PressurePlates\PressurePlateTypes.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\PressurePlates\PressurePlateOccupancyComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\PressurePlates\PressurePlateEntityCreationAdapter.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\PressurePlates\PressurePlateSystem.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Interaction\PositionedChestSnapshot.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Interaction\FakeCursorItemQuery.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Input\InputMode.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Input\KeyBindingConfigurationComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Input\PlayerInputProfileConfigurationComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Input\InputTriggerFrameComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Input\PlayerInputRuntimeComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Equipment\EquipmentLoadoutComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Equipment\EquipmentLoadoutSwapSystem.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Equipment\EquipmentDropCommand.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Movement\PlayerMovementCapabilityComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Movement\MountMovementRestoreSnapshot.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Movement\PortableStoolUsageComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Intent\PlayerIntentionStateComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Intent\PlayerInteractionAnchorComponent.cs
  - D:\TRbackup\NLTX\src2\PlayerInputGameplay\Intent\IntentionTrackingSystem.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Creative\CreativePowerIconLocation.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Creative\PerPlayerCreativePowerDefinition.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Creative\PerPlayerCreativePowerStateComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Creative\CreativeNpcStrengthMultiplierQuery.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Creative\SharedCreativePowerDefinition.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Creative\SharedCreativePowerStateComponent.cs
- D:\TRbackup\NLTX\src2\PlayerInputGameplay\Creative\CreativePowerSyncProjection.cs
- verification: failed; production and verifier projects built successfully, but the existing verifier failed before the Creative scenarios at `PlayerInputGameplayVerification/Program.cs:172` with `Loadout swap should preserve both armor values.`
evidence-gap:
- P08 第一轮专属 outputReport 不存在；实施计划不能把第一轮研究当作已完成前置条件。
- MainInputAndThreadScheduling 的首批实现源码已保存到 src2，但尚未编译、测试或行为等价验证；implementationStatus 只能保持 in-progress。
- MainScreenAndInputState 的实现源码已保存到 src2；CursorItemPreview 是本地快照边界，尚未编译、测试或与 Version4 行为等价验证。
- MainPlayerAndSpawnState、MainMenuAndInputSettings 和 MainInputAndEventFlags 的实现源码已保存到 src2；稳定实体 ID、持久化语义和事件清理调用者仍是 evidence-gap。
- MainDerivedInputAndPresentationQueries、SharedGolfState、SharedControlFocusHelpers、SharedCreativePowerRuntimeManager、SharedCreativeUnlockProgress、SharedSmartInteractionQueries、PlayerItemPickupAndRespawnState 和 PlayerPreviewAndRejectionState 的实现源码及 focused verifier 已保存到 src2；本次生产项目构建成功，但统一 verifier 在更早的 Equipment 断言处停止。
- DoorOpeningInteractionState、SmartCursorInteractionState、PressurePlateInteractionState、CursorAndChestInteractionState、InputProfilesAndConfiguration、InputTriggerState、PlayerInputRuntimeState、EquipmentLoadoutState、SharedCreativePowerContracts、PlayerMovementCapabilityState 和 PlayerIntentAndInteractionState 的实现源码已保存到 src2；统一 verifier 未能越过 Equipment 断言。
- SharedCreativePerPlayerPowerState 和 SharedCreativeSharedPowerState 的实现源码已保存到 src2；所有 26 个叶子单元均有 src2 实现。两个受影响项目构建成功，但 Creative 专项断言未执行。
- 串行构建 `src2/PlayerInputGameplay/Terraria.PlayerInputGameplay.csproj` 与 `src2/PlayerInputGameplayVerification/Terraria.PlayerInputGameplayVerification.csproj` 均退出码 0，0 warning、0 error；产物位于 `Build/bin/Terraria.PlayerInputGameplay/Debug/net10.0/` 和 `Build/bin/Terraria.PlayerInputGameplayVerification/Debug/net10.0/`。
- verifier 命令经 `Build/Tools/Invoke-SerialDotnet.ps1` 使用 `run --no-build --no-restore` 执行，退出码 `-532462766`；失败属于既有 EquipmentLoadout 非 Component 路径，本轮禁止修改该路径，因此 P08 Creative verification 保持未闭合。
- 本次实现会话按用户授权接续旧 checkpoint，并将本文件绑定到当前 runner session dc9ebd50d1dd49e39b074ca3b7f5001d；旧 sessionId 仅作为历史记录，不再阻止当前 Component 实施。
- Version4 SmartCursor、SmartInteract provider、Door handler、PressurePlate movement methods、Creative unlock/network 和 Minion respawn 的行为证据不完整；实施必须先补 verifier 或明确兼容行为。
- stable entity ID、network/persistence ID、Player/World/Tile owner 和调度顺序跨分区，仍由 integration-review 决定。
blocking-decision:
- 在接线前锁定 PlayerEntityId/InputIntent/交互目标/CreativePower 的 ID 和旧数组槽位转换表。
- 在接线前锁定 Equipment drop、pressure/door/tile mutation、Creative permission/network/save、item pickup/minion respawn 的唯一写 owner。
- 在接线前锁定 Main input -> intent -> interaction/movement 的顺序、失败归属、重试和兼容窗口。

## 1. 实施约束

只允许未来修改受影响项目中与 P08 直接相关的领域文件；不得现在创建生产文件。目标目录按领域/能力组织，遵守一个核心公开类型一个同名 PascalCase 文件。Query 无写入，Projection 不反向写模拟，Adapter 隔离 I/O、平台、时钟、随机、网络和旧对象。

不使用文件或目录顺序定义运行时顺序。所有顺序在显式 scheduler contract 中表达。实施期间不做 broad cleanup，不覆盖其他会话文件，不修改第一轮报告、其他 P 分区报告、ledger 或 lock。

## 2. Proposed 目标文件、目录和命名空间

| boundary | proposed target files and namespace |
|---|---|
| Main input/runtime | src2/Input/Runtime/MainInputFrameComponent.cs, MainThreadActionQueue.cs, MainFrameTimingComponent.cs; Terraria.Input.Runtime |
| Screen/cursor | src2/Input/Presentation/ScreenViewportComponent.cs, src2/WorldInteraction/Interaction/CursorItemPreviewComponent.cs; Terraria.Input.Presentation / Terraria.WorldInteraction.Interaction |
| Player registry/spawn | src2/Player/Runtime/PlayerRegistryAdapter.cs, src2/Player/PlayerSpawnPointComponent.cs; Terraria.Player.Runtime |
| Input preferences/settings | src2/Input/Configuration/InputPreferenceComponent.cs, InputProfileConfigurationComponent.cs, RuntimeSettingsAdapter.cs; Terraria.Input.Configuration |
| Main flags/queries | src2/Input/Runtime/InputGateComponent.cs, InputEventFlagsComponent.cs, src2/Input/Queries/MainInputProjectionQueries.cs; Terraria.Input.Runtime / Terraria.Input.Queries |
| Golf | src2/Golf/GolfRulesDefinition.cs, GolfLocalTrackingComponent.cs, GolfTrackingSystem.cs, GolfCameraProjection.cs; Terraria.Golf |
| Focus/platform | src2/Platform/Focus/FocusStateAdapter.cs, GameplayActivityQuery.cs; Terraria.Platform.Focus |
| Creative registry | src2/Creative/Registry/CreativePowerRegistryComponent.cs, CreativePowerRegistrySystem.cs; Terraria.Creative.Registry |
| Creative unlocks | src2/Creative/Unlocks/CreativeSacrificeCatalogComponent.cs, CreativeUnlockProgressComponent.cs, CreativeUnlockProjection.cs; Terraria.Creative.Unlocks |
| Smart interaction | src2/WorldInteraction/SmartInteraction/SmartInteractionScanQuery.cs, SmartInteractionCandidateBuffer.cs, SmartInteractionSystem.cs; Terraria.WorldInteraction.SmartInteraction |
| Pickup/respawn | src2/Player/Pickup/MinionRespawnComponent.cs, PlayerItemPickupLogComponent.cs, PlayerItemPickupLogEntry.cs; Terraria.Player.Pickup |
| Preview/rejection | src2/Player/Presentation/CharacterPreviewSettingsComponent.cs, PlayerRejectionPresentationComponent.cs; Terraria.Player.Presentation |
| Doors | src2/WorldInteraction/Doors/DoorOpeningStateComponent.cs, DoorOpeningSystem.cs, DoorOpeningHandlerAdapter.cs; Terraria.WorldInteraction.Doors |
| SmartCursor | src2/WorldInteraction/SmartCursor/SmartCursorUsageQuery.cs, SmartCursorTargetBuffer.cs, SmartCursorSystem.cs; Terraria.WorldInteraction.SmartCursor |
| Pressure plates | src2/WorldInteraction/PressurePlates/PressurePlateOccupancyComponent.cs, PressurePlateSystem.cs, PressurePlateEntityAdapter.cs; Terraria.WorldInteraction.PressurePlates |
| Chest/cursor | src2/WorldInteraction/Interaction/PositionedChestSnapshot.cs, CursorItemPreviewComponent.cs, FakeCursorItemQuery.cs; Terraria.WorldInteraction.Interaction |
| Input profiles/triggers | src2/Input/Configuration/InputProfileComponent.cs, src2/Input/Runtime/InputTriggerFrameComponent.cs, InputEdgeSystem.cs; Terraria.Input.Configuration / Terraria.Input.Runtime |
| Player input runtime | src2/Input/Runtime/PlayerInputRuntimeComponent.cs, ViewportAdapter.cs; Terraria.Input.Runtime |
| Equipment | src2/Player/Equipment/EquipmentLoadoutComponent.cs, EquipmentLoadoutSwapSystem.cs, EquipmentDropCommand.cs; Terraria.Player.Equipment |
| Creative contract | src2/Creative/Contracts/CreativePowerContract.cs, CreativePowerNetworkAdapter.cs; Terraria.Creative.Contracts |
| Movement | src2/Player/Movement/PlayerMovementCapabilityComponent.cs, MountMovementRestoreSnapshot.cs, PortableStoolUsageComponent.cs; Terraria.Player.Movement |
| Intent/anchor | src2/Player/Intent/PlayerIntentionStateComponent.cs, IntentionTrackingSystem.cs, PlayerInteractionAnchorComponent.cs; Terraria.Player.Intent |
| Per-player powers | src2/Creative/Powers/PerPlayerCreativePowerDefinition.cs, PerPlayerCreativePowerStateComponent.cs; Terraria.Creative.Powers |
| Shared powers | src2/Creative/Powers/SharedCreativePowerDefinition.cs, SharedCreativePowerStateComponent.cs, CreativePowerSyncProjection.cs; Terraria.Creative.Powers |

除 implementationCheckpoint 列出的已保存文件外，上述文件均为 proposed 目标，不代表本会话已经创建；若实施时发现同名现有类型，必须先按 ECS 文件组织和组件命名约束记录迁移，不得直接覆盖。

## 3. 全部成员映射（283 条，计划归属）

本表与设计文档使用同一 source member ID、Version4 path/line、target 和 evidenceStatus；它是执行顺序和验收覆盖的逐成员清单。除 implementationCheckpoint 列出的首批文件外，成员均未迁移。

| source member ID | leaf subsystem | Version4 declaration and location | proposed role/target | evidenceStatus |
|---:|---|---|---|---|
| 69 | MainInputAndThreadScheduling | field Terraria.Main.verboseNetplay : bool; D:\\TRbackup\\Version4\\Terraria\\Main.cs:297 | proposed Component: RuntimeDiagnosticsSettings | confirmed-declaration; lifecycle partial |
| 70 | MainInputAndThreadScheduling | field Terraria.Main.stopTimeOuts : bool; D:\\TRbackup\\Version4\\Terraria\\Main.cs:299 | proposed Component: RuntimeDiagnosticsSettings | confirmed-declaration; lifecycle partial |
| 71 | MainInputAndThreadScheduling | field Terraria.Main.townNPCCanSpawn : bool[]; D:\\TRbackup\\Version4\\Terraria\\Main.cs:301 | proposed Projection: TownNpcSpawnEligibilityView | confirmed-declaration; lifecycle partial |
| 72 | MainInputAndThreadScheduling | field Terraria.Main.upTimer : float; D:\\TRbackup\\Version4\\Terraria\\Main.cs:303 | proposed Component: MainFrameTimingState | confirmed-declaration; lifecycle partial |
| 73 | MainInputAndThreadScheduling | field Terraria.Main.upTimerMax : float; D:\\TRbackup\\Version4\\Terraria\\Main.cs:305 | proposed Component: MainFrameTimingState | confirmed-declaration; lifecycle partial |
| 74 | MainInputAndThreadScheduling | field Terraria.Main.upTimerMaxDelay : float; D:\\TRbackup\\Version4\\Terraria\\Main.cs:307 | proposed Component: MainFrameTimingState | confirmed-declaration; lifecycle partial |
| 75 | MainInputAndThreadScheduling | field Terraria.Main.renderNow : bool; D:\\TRbackup\\Version4\\Terraria\\Main.cs:309 | proposed Component: RenderRequestState | confirmed-declaration; lifecycle partial |
| 76 | MainInputAndThreadScheduling | field Terraria.Main.mouseX : int; D:\\TRbackup\\Version4\\Terraria\\Main.cs:311 | proposed Component: RawPointerInputState | confirmed-declaration; lifecycle partial |
| 77 | MainInputAndThreadScheduling | field Terraria.Main.mouseY : int; D:\\TRbackup\\Version4\\Terraria\\Main.cs:313 | proposed Component: RawPointerInputState | confirmed-declaration; lifecycle partial |
| 78 | MainInputAndThreadScheduling | field Terraria.Main._mainThreadActions : System.Collections.Concurrent.ConcurrentQueue<System.Action>; D:\\TRbackup\\Version4\\Terraria\\Main.cs:316 | proposed Adapter/Queue: MainThreadActionQueue | confirmed-declaration; lifecycle partial |
| 79 | MainInputAndThreadScheduling | field Terraria.Main.mouseRight : bool; D:\\TRbackup\\Version4\\Terraria\\Main.cs:318 | proposed Component: RawPointerInputState | confirmed-declaration; lifecycle partial |
| 390 | MainScreenAndInputState | field Terraria.Main.screenPosition : Vector2; D:\\TRbackup\\Version4\\Terraria\\Main.cs:959 | proposed Component: ScreenViewportState | confirmed-declaration; input/presentation lifecycle partial |
| 391 | MainScreenAndInputState | field Terraria.Main.screenWidth : int; D:\\TRbackup\\Version4\\Terraria\\Main.cs:961 | proposed Component: ScreenViewportState | confirmed-declaration; input/presentation lifecycle partial |
| 392 | MainScreenAndInputState | field Terraria.Main.screenHeight : int; D:\\TRbackup\\Version4\\Terraria\\Main.cs:963 | proposed Component: ScreenViewportState | confirmed-declaration; input/presentation lifecycle partial |
| 393 | MainScreenAndInputState | field Terraria.Main.multiplayerNPCSmoothingRange : int; D:\\TRbackup\\Version4\\Terraria\\Main.cs:965 | proposed Component: ScreenInputSettings | confirmed-declaration; input/presentation lifecycle partial |
| 394 | MainScreenAndInputState | field Terraria.Main.Setting_UseReducedMaxLiquids : bool; D:\\TRbackup\\Version4\\Terraria\\Main.cs:967 | proposed Component: ScreenInputSettings | confirmed-declaration; input/presentation lifecycle partial |
| 395 | MainScreenAndInputState | field Terraria.Main.PlayerOverheadChatMessageDisplayTime : int; D:\\TRbackup\\Version4\\Terraria\\Main.cs:969 | proposed Component: ScreenInputSettings | confirmed-declaration; input/presentation lifecycle partial |
| 396 | MainScreenAndInputState | field Terraria.Main.CurrentInputTextTakerOverride : object; D:\\TRbackup\\Version4\\Terraria\\Main.cs:971 | proposed Adapter state: InputTextCapture | confirmed-declaration; input/presentation lifecycle partial |
| 397 | MainScreenAndInputState | field Terraria.Main.mouseTextColor : byte; D:\\TRbackup\\Version4\\Terraria\\Main.cs:973 | proposed Projection: CursorPresentationState | confirmed-declaration; input/presentation lifecycle partial |
| 398 | MainScreenAndInputState | field Terraria.Main.mouseRightRelease : bool; D:\\TRbackup\\Version4\\Terraria\\Main.cs:975 | proposed Component: PointerTransitionState | confirmed-declaration; input/presentation lifecycle partial |
| 399 | MainScreenAndInputState | field Terraria.Main.mouseItem : Terraria.Item; D:\\TRbackup\\Version4\\Terraria\\Main.cs:977 | proposed Adapter/Projection: CursorItemPreview | confirmed-declaration; input/presentation lifecycle partial |
| 400 | MainPlayerAndSpawnState | field Terraria.Main.myPlayer : int; D:\\TRbackup\\Version4\\Terraria\\Main.cs:980 | proposed Adapter: LocalPlayerSelection | confirmed-declaration; registry owner partial |
| 401 | MainPlayerAndSpawnState | field Terraria.Main.player : Terraria.Player[]; D:\\TRbackup\\Version4\\Terraria\\Main.cs:982 | proposed Adapter: PlayerRegistry | confirmed-declaration; registry owner partial |
| 402 | MainPlayerAndSpawnState | field Terraria.Main.countsAsHostForGameplay : bool[]; D:\\TRbackup\\Version4\\Terraria\\Main.cs:984 | proposed Component: GameplayHostEligibility | confirmed-declaration; registry owner partial |
| 403 | MainPlayerAndSpawnState | field Terraria.Main.spawnTileX : int; D:\\TRbackup\\Version4\\Terraria\\Main.cs:986 | proposed Component: PlayerSpawnPointState | confirmed-declaration; registry owner partial |
| 404 | MainPlayerAndSpawnState | field Terraria.Main.spawnTileY : int; D:\\TRbackup\\Version4\\Terraria\\Main.cs:988 | proposed Component: PlayerSpawnPointState | confirmed-declaration; registry owner partial |
| 456 | MainMenuAndInputSettings | field Terraria.Main.cInv : string; D:\\TRbackup\\Version4\\Terraria\\Main.cs:1180 | proposed Component: InputPreference | confirmed-declaration; settings persistence partial |
| 457 | MainMenuAndInputSettings | field Terraria.Main.SmartCursorWanted_Mouse : bool; D:\\TRbackup\\Version4\\Terraria\\Main.cs:1182 | proposed Component: SmartCursorPreference | confirmed-declaration; settings persistence partial |
| 458 | MainMenuAndInputSettings | field Terraria.Main.SmartCursorWanted_GamePad : bool; D:\\TRbackup\\Version4\\Terraria\\Main.cs:1184 | proposed Component: SmartCursorPreference | confirmed-declaration; settings persistence partial |
| 459 | MainMenuAndInputSettings | field Terraria.Main.NoPooling : bool; D:\\TRbackup\\Version4\\Terraria\\Main.cs:1186 | proposed Adapter: RuntimeAllocationSettings | confirmed-declaration; settings persistence partial |
| 460 | MainMenuAndInputSettings | field Terraria.Main.CollectGen0EveryFrame : bool; D:\\TRbackup\\Version4\\Terraria\\Main.cs:1188 | proposed Adapter: RuntimeAllocationSettings | confirmed-declaration; settings persistence partial |
| 504 | MainInputAndEventFlags | field Terraria.Main.blockMouse : bool; D:\\TRbackup\\Version4\\Terraria\\Main.cs:1282 | proposed Component: InputGateState | confirmed-declaration; reset/consumer partial |
| 505 | MainInputAndEventFlags | field Terraria.Main._isDrawingOrUpdating : bool; D:\\TRbackup\\Version4\\Terraria\\Main.cs:1285 | proposed Component: FramePhaseState | confirmed-declaration; reset/consumer partial |
| 506 | MainInputAndEventFlags | field Terraria.Main.disableDontStarveDarknessDamage : bool; D:\\TRbackup\\Version4\\Terraria\\Main.cs:1288 | proposed Component: GameplayEventFlags | confirmed-declaration; reset/consumer partial |
| 507 | MainInputAndEventFlags | field Terraria.Main.starGame : bool; D:\\TRbackup\\Version4\\Terraria\\Main.cs:1290 | proposed Component: FallingStarEventState | confirmed-declaration; reset/consumer partial |
| 508 | MainInputAndEventFlags | field Terraria.Main.starsHit : int; D:\\TRbackup\\Version4\\Terraria\\Main.cs:1292 | proposed Component: FallingStarEventState | confirmed-declaration; reset/consumer partial |
| 509 | MainInputAndEventFlags | field Terraria.Main.ladyBugRainBoost : int; D:\\TRbackup\\Version4\\Terraria\\Main.cs:1294 | proposed Component: WeatherEventModifierState | confirmed-declaration; reset/consumer partial |
| 510 | MainInputAndEventFlags | field Terraria.Main._canShowMeteorFall : bool; D:\\TRbackup\\Version4\\Terraria\\Main.cs:1296 | proposed Projection: MeteorFallPresentationState | confirmed-declaration; reset/consumer partial |
| 511 | MainDerivedInputAndPresentationQueries | property Terraria.Main.UIScale : float; D:\\TRbackup\\Version4\\Terraria\\Main.cs:1298 | proposed Query/Projection: ViewportScaleQuery | confirmed-declaration; derived readers partial |
| 512 | MainDerivedInputAndPresentationQueries | property Terraria.Main.IsItRaining : bool; D:\\TRbackup\\Version4\\Terraria\\Main.cs:1314 | proposed Query/Projection: WeatherPresentationQuery | confirmed-declaration; derived readers partial |
| 524 | MainDerivedInputAndPresentationQueries | property Terraria.Main.MouseScreen : Vector2; D:\\TRbackup\\Version4\\Terraria\\Main.cs:1401 | proposed Query/Projection: CursorScreenCoordinateQuery | confirmed-declaration; derived readers partial |
| 525 | MainDerivedInputAndPresentationQueries | property Terraria.Main.MouseWorld : Vector2; D:\\TRbackup\\Version4\\Terraria\\Main.cs:1403 | proposed Query/Projection: CursorWorldCoordinateQuery | confirmed-declaration; derived readers partial |
| 526 | MainDerivedInputAndPresentationQueries | property Terraria.Main.ActiveNetDiagnosticsUI : Terraria.UI.INetDiagnosticsUI; D:\\TRbackup\\Version4\\Terraria\\Main.cs:1416 | proposed Query/Projection: NetDiagnosticsPresentationAdapter | confirmed-declaration; derived readers partial |
| 531 | MainDerivedInputAndPresentationQueries | property Terraria.Main.PlayerSceneMetrics : Terraria.SceneMetrics; D:\\TRbackup\\Version4\\Terraria\\Main.cs:1440 | proposed Query/Projection: PlayerSceneMetricsQuery | confirmed-declaration; derived readers partial |
| 533 | MainDerivedInputAndPresentationQueries | property Terraria.Main.WindForVisuals : float; D:\\TRbackup\\Version4\\Terraria\\Main.cs:1454 | proposed Query/Projection: WindPresentationQuery | confirmed-declaration; derived readers partial |
| 534 | MainDerivedInputAndPresentationQueries | property Terraria.Main.ChatLineWidthLimit : int; D:\\TRbackup\\Version4\\Terraria\\Main.cs:1456 | proposed Query/Projection: ChatLayoutQuery | confirmed-declaration; derived readers partial |
| 541 | MainDerivedInputAndPresentationQueries | property Terraria.Main.BlackFadeDist : float; D:\\TRbackup\\Version4\\Terraria\\Main.cs:1470 | proposed Query/Projection: CameraFadeQuery | confirmed-declaration; derived readers partial |
| 542 | MainDerivedInputAndPresentationQueries | property Terraria.Main.IsRainingForever : bool; D:\\TRbackup\\Version4\\Terraria\\Main.cs:1472 | proposed Query/Projection: WeatherPresentationQuery | confirmed-declaration; derived readers partial |
| 2020 | SharedGolfState | field Terraria.GameContent.Golf.GolfBallTrackRecord._hitLocations : System.Collections.Generic.List<Vector2>; D:\\TRbackup\\Version4\\Terraria.GameContent.Golf\\GolfBallTrackRecord.cs:8 | proposed Component: GolfBallTrackRecord | confirmed-declaration; local tracking/camera behavior partial |
| 2021 | SharedGolfState | field Terraria.GameContent.Golf.GolfHelper.PointsNeededForLevel1 : int; D:\\TRbackup\\Version4\\Terraria.GameContent.Golf\\GolfHelper.cs:23 | proposed Definition: GolfRules | confirmed-declaration; local tracking/camera behavior partial |
| 2022 | SharedGolfState | field Terraria.GameContent.Golf.GolfHelper.PointsNeededForLevel2 : int; D:\\TRbackup\\Version4\\Terraria.GameContent.Golf\\GolfHelper.cs:25 | proposed Definition: GolfRules | confirmed-declaration; local tracking/camera behavior partial |
| 2023 | SharedGolfState | field Terraria.GameContent.Golf.GolfHelper.PointsNeededForLevel3 : int; D:\\TRbackup\\Version4\\Terraria.GameContent.Golf\\GolfHelper.cs:27 | proposed Definition: GolfRules | confirmed-declaration; local tracking/camera behavior partial |
| 2024 | SharedGolfState | field Terraria.GameContent.Golf.GolfHelper.PhysicsProperties : Terraria.Physics.PhysicsProperties; D:\\TRbackup\\Version4\\Terraria.GameContent.Golf\\GolfHelper.cs:29 | proposed Adapter: GolfPhysicsProperties | confirmed-declaration; local tracking/camera behavior partial |
| 2025 | SharedGolfState | field Terraria.GameContent.Golf.GolfHelper.Listener : Terraria.GameContent.Golf.GolfHelper.ContactListener; D:\\TRbackup\\Version4\\Terraria.GameContent.Golf\\GolfHelper.cs:31 | proposed Adapter: GolfCollisionListener | confirmed-declaration; local tracking/camera behavior partial |
| 2026 | SharedGolfState | field Terraria.GameContent.Golf.GolfState.BALL_RETURN_PENALTY : int; D:\\TRbackup\\Version4\\Terraria.GameContent.Golf\\GolfState.cs:8 | proposed Component/System state: GolfLocalTracking | confirmed-declaration; local tracking/camera behavior partial |
| 2027 | SharedGolfState | field Terraria.GameContent.Golf.GolfState.golfScoreTime : int; D:\\TRbackup\\Version4\\Terraria.GameContent.Golf\\GolfState.cs:10 | proposed Component/System state: GolfLocalTracking | confirmed-declaration; local tracking/camera behavior partial |
| 2028 | SharedGolfState | field Terraria.GameContent.Golf.GolfState.golfScoreTimeMax : int; D:\\TRbackup\\Version4\\Terraria.GameContent.Golf\\GolfState.cs:12 | proposed Component/System state: GolfLocalTracking | confirmed-declaration; local tracking/camera behavior partial |
| 2029 | SharedGolfState | field Terraria.GameContent.Golf.GolfState.golfScoreDelay : int; D:\\TRbackup\\Version4\\Terraria.GameContent.Golf\\GolfState.cs:14 | proposed Component/System state: GolfLocalTracking | confirmed-declaration; local tracking/camera behavior partial |
| 2030 | SharedGolfState | field Terraria.GameContent.Golf.GolfState._lastRecordedBallTime : double; D:\\TRbackup\\Version4\\Terraria.GameContent.Golf\\GolfState.cs:16 | proposed Component/System state: GolfLocalTracking | confirmed-declaration; local tracking/camera behavior partial |
| 2031 | SharedGolfState | field Terraria.GameContent.Golf.GolfState._lastRecordedBallLocation : Vector2?; D:\\TRbackup\\Version4\\Terraria.GameContent.Golf\\GolfState.cs:18 | proposed Component/System state: GolfLocalTracking | confirmed-declaration; local tracking/camera behavior partial |
| 2032 | SharedGolfState | field Terraria.GameContent.Golf.GolfState._waitingForBallToSettle : bool; D:\\TRbackup\\Version4\\Terraria.GameContent.Golf\\GolfState.cs:20 | proposed Component/System state: GolfLocalTracking | confirmed-declaration; local tracking/camera behavior partial |
| 2033 | SharedGolfState | field Terraria.GameContent.Golf.GolfState._lastHitGolfBall : Terraria.Projectile; D:\\TRbackup\\Version4\\Terraria.GameContent.Golf\\GolfState.cs:22 | proposed Projection state: GolfCameraTracking | confirmed-declaration; local tracking/camera behavior partial |
| 2034 | SharedGolfState | field Terraria.GameContent.Golf.GolfState._lastRecordedSwingCount : int; D:\\TRbackup\\Version4\\Terraria.GameContent.Golf\\GolfState.cs:24 | proposed Component/System state: GolfLocalTracking | confirmed-declaration; local tracking/camera behavior partial |
| 2035 | SharedGolfState | field Terraria.GameContent.Golf.GolfState._hitRecords : Terraria.GameContent.Golf.GolfBallTrackRecord[]; D:\\TRbackup\\Version4\\Terraria.GameContent.Golf\\GolfState.cs:26 | proposed Component/System state: GolfLocalTracking | confirmed-declaration; local tracking/camera behavior partial |
| 3077 | SharedControlFocusHelpers | field Terraria.FocusHelper.IsSelectedApplication : bool; D:\\TRbackup\\Version4\\Terraria\\FocusHelper.cs:8 | proposed Adapter state: WindowFocus | confirmed-declaration; platform adapter behavior partial |
| 3957 | SharedControlFocusHelpers | property Terraria.FocusHelper.GameplayActive : bool; D:\\TRbackup\\Version4\\Terraria\\FocusHelper.cs:10 | proposed Query: GameplayActivity | confirmed-declaration; platform adapter behavior partial |
| 3958 | SharedControlFocusHelpers | property Terraria.FocusHelper.UpdateVisualEffects : bool; D:\\TRbackup\\Version4\\Terraria\\FocusHelper.cs:22 | proposed Query: GameplayActivity | confirmed-declaration; platform adapter behavior partial |
| 3959 | SharedControlFocusHelpers | property Terraria.FocusHelper.AllowRain : bool; D:\\TRbackup\\Version4\\Terraria\\FocusHelper.cs:24 | proposed Query: GameplayActivity | confirmed-declaration; platform adapter behavior partial |
| 3960 | SharedControlFocusHelpers | property Terraria.FocusHelper.AllowCountingPlayerTime : bool; D:\\TRbackup\\Version4\\Terraria\\FocusHelper.cs:26 | proposed Query: GameplayActivity | confirmed-declaration; platform adapter behavior partial |
| 1462 | SharedCreativePowerRuntimeManager | field Terraria.GameContent.Creative.CreativePowerManager.PowerTypeStorage<T>.Id : ushort; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowerManager.cs:12 | proposed Component: CreativePowerRegistryEntry | confirmed-declaration; registry/save/network lifecycle partial |
| 1463 | SharedCreativePowerRuntimeManager | field Terraria.GameContent.Creative.CreativePowerManager.PowerTypeStorage<T>.Name : string; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowerManager.cs:14 | proposed Component: CreativePowerRegistryEntry | confirmed-declaration; registry/save/network lifecycle partial |
| 1464 | SharedCreativePowerRuntimeManager | field Terraria.GameContent.Creative.CreativePowerManager.PowerTypeStorage<T>.Power : T; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowerManager.cs:16 | proposed Component: CreativePowerRegistryEntry | confirmed-declaration; registry/save/network lifecycle partial |
| 1465 | SharedCreativePowerRuntimeManager | field Terraria.GameContent.Creative.CreativePowerManager.Instance : Terraria.GameContent.Creative.CreativePowerManager; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowerManager.cs:19 | proposed Adapter: CreativePowerRegistryAccess | confirmed-declaration; registry/save/network lifecycle partial |
| 1466 | SharedCreativePowerRuntimeManager | field Terraria.GameContent.Creative.CreativePowerManager._powersById : System.Collections.Generic.Dictionary<ushort, Terraria.GameContent.Creative.ICreativePower>; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowerManager.cs:21 | proposed Component/System: CreativePowerRegistry | confirmed-declaration; registry/save/network lifecycle partial |
| 1467 | SharedCreativePowerRuntimeManager | field Terraria.GameContent.Creative.CreativePowerManager._powersByName : System.Collections.Generic.Dictionary<string, Terraria.GameContent.Creative.ICreativePower>; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowerManager.cs:23 | proposed Component/System: CreativePowerRegistry | confirmed-declaration; registry/save/network lifecycle partial |
| 1468 | SharedCreativePowerRuntimeManager | field Terraria.GameContent.Creative.CreativePowerManager._powersCount : ushort; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowerManager.cs:25 | proposed Component/System: CreativePowerRegistry | confirmed-declaration; registry/save/network lifecycle partial |
| 1469 | SharedCreativePowerRuntimeManager | field Terraria.GameContent.Creative.CreativePowerManager._initialized : bool; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowerManager.cs:27 | proposed Component/System: CreativePowerRegistry | confirmed-declaration; registry/save/network lifecycle partial |
| 1470 | SharedCreativePowerRuntimeManager | field Terraria.GameContent.Creative.CreativePowerManager._powerPermissionsLineHeader : string; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowerManager.cs:29 | proposed Component/System: CreativePowerRegistry | confirmed-declaration; registry/save/network lifecycle partial |
| 1460 | SharedCreativeUnlockProgress | field Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance : Terraria.GameContent.Creative.CreativeItemSacrificesCatalog; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativeItemSacrificesCatalog.cs:10 | proposed Component: CreativeSacrificeCatalog | confirmed-declaration; unlock/network/persistence partial |
| 1461 | SharedCreativeUnlockProgress | field Terraria.GameContent.Creative.CreativeItemSacrificesCatalog._sacrificeCountNeededByItemId : System.Collections.Generic.Dictionary<int, int>; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativeItemSacrificesCatalog.cs:12 | proposed Component: CreativeSacrificeCatalog | confirmed-declaration; unlock/network/persistence partial |
| 1519 | SharedCreativeUnlockProgress | field Terraria.GameContent.Creative.CreativeUnlocksTracker.ItemSacrifices : Terraria.GameContent.Creative.ItemsSacrificedUnlocksTracker; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativeUnlocksTracker.cs:7 | proposed Component: CreativeUnlockProgress | confirmed-declaration; unlock/network/persistence partial |
| 1520 | SharedCreativeUnlockProgress | field Terraria.GameContent.Creative.ItemsSacrificedUnlocksTracker.POSITIVE_SACRIFICE_COUNT_CAP : int; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\ItemsSacrificedUnlocksTracker.cs:10 | proposed Component: CreativeSacrificeProgress | confirmed-declaration; unlock/network/persistence partial |
| 1521 | SharedCreativeUnlockProgress | field Terraria.GameContent.Creative.ItemsSacrificedUnlocksTracker._sacrificeCountByItemPersistentId : System.Collections.Generic.Dictionary<string, int>; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\ItemsSacrificedUnlocksTracker.cs:12 | proposed Component: CreativeSacrificeProgress | confirmed-declaration; unlock/network/persistence partial |
| 1522 | SharedCreativeUnlockProgress | field Terraria.GameContent.Creative.ItemsSacrificedUnlocksTracker._sacrificesCountByItemIdCache : System.Collections.Generic.Dictionary<int, int>; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\ItemsSacrificedUnlocksTracker.cs:14 | proposed Component: CreativeUnlockRevision | confirmed-declaration; unlock/network/persistence partial |
| 1523 | SharedCreativeUnlockProgress | field Terraria.GameContent.Creative.ItemsSacrificedUnlocksTracker._unlockedByTeammate : System.Collections.Generic.Dictionary<int, string>; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\ItemsSacrificedUnlocksTracker.cs:16 | proposed Projection state: CreativeUnlockNetwork | confirmed-declaration; unlock/network/persistence partial |
| 1524 | SharedCreativeUnlockProgress | field Terraria.GameContent.Creative.ItemsSacrificedUnlocksTracker._newlyUnlocked : System.Collections.Generic.HashSet<int>; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\ItemsSacrificedUnlocksTracker.cs:18 | proposed Component/Event: CreativeUnlockNotifications | confirmed-declaration; unlock/network/persistence partial |
| 1525 | SharedCreativeUnlockProgress | field Terraria.GameContent.Creative.ItemsSacrificedUnlocksTracker.AnyNewUnlocksFromTeammates : bool; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\ItemsSacrificedUnlocksTracker.cs:20 | proposed Component/Event: CreativeUnlockNotifications | confirmed-declaration; unlock/network/persistence partial |
| 3776 | SharedCreativeUnlockProgress | property Terraria.GameContent.Creative.ItemsSacrificedUnlocksTracker.LastEditId : int; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\ItemsSacrificedUnlocksTracker.cs:22 | proposed Component: CreativeUnlockRevision | confirmed-declaration; unlock/network/persistence partial |
| 2145 | SharedSmartInteractionQueries | field Terraria.GameContent.ObjectInteractions.NPCSmartInteractCandidateProvider._candidate : Terraria.GameContent.ObjectInteractions.NPCSmartInteractCandidateProvider.ReusableCandidate; D:\\TRbackup\\Version4\\Terraria.GameContent.ObjectInteractions\\NPCSmartInteractCandidateProvider.cs:16 | proposed Query buffer: SmartInteractionCandidatePool | confirmed-declaration; provider algorithm partial |
| 2146 | SharedSmartInteractionQueries | field Terraria.GameContent.ObjectInteractions.PotionOfReturnSmartInteractCandidateProvider._candidate : Terraria.GameContent.ObjectInteractions.PotionOfReturnSmartInteractCandidateProvider.ReusableCandidate; D:\\TRbackup\\Version4\\Terraria.GameContent.ObjectInteractions\\PotionOfReturnSmartInteractCandidateProvider.cs:14 | proposed Query buffer: SmartInteractionCandidatePool | confirmed-declaration; provider algorithm partial |
| 2147 | SharedSmartInteractionQueries | field Terraria.GameContent.ObjectInteractions.ProjectileSmartInteractCandidateProvider._candidate : Terraria.GameContent.ObjectInteractions.ProjectileSmartInteractCandidateProvider.ReusableCandidate; D:\\TRbackup\\Version4\\Terraria.GameContent.ObjectInteractions\\ProjectileSmartInteractCandidateProvider.cs:16 | proposed Query buffer: SmartInteractionCandidatePool | confirmed-declaration; provider algorithm partial |
| 2148 | SharedSmartInteractionQueries | field Terraria.GameContent.ObjectInteractions.SmartInteractScanSettings.player : Terraria.Player; D:\\TRbackup\\Version4\\Terraria.GameContent.ObjectInteractions\\SmartInteractScanSettings.cs:7 | proposed Query input: SmartInteractionScanSettings | confirmed-declaration; provider algorithm partial |
| 2149 | SharedSmartInteractionQueries | field Terraria.GameContent.ObjectInteractions.SmartInteractScanSettings.DemandOnlyZeroDistanceTargets : bool; D:\\TRbackup\\Version4\\Terraria.GameContent.ObjectInteractions\\SmartInteractScanSettings.cs:9 | proposed Query input: SmartInteractionScanSettings | confirmed-declaration; provider algorithm partial |
| 2150 | SharedSmartInteractionQueries | field Terraria.GameContent.ObjectInteractions.SmartInteractScanSettings.FullInteraction : bool; D:\\TRbackup\\Version4\\Terraria.GameContent.ObjectInteractions\\SmartInteractScanSettings.cs:11 | proposed Query input: SmartInteractionScanSettings | confirmed-declaration; provider algorithm partial |
| 2151 | SharedSmartInteractionQueries | field Terraria.GameContent.ObjectInteractions.SmartInteractScanSettings.mousevec : Vector2; D:\\TRbackup\\Version4\\Terraria.GameContent.ObjectInteractions\\SmartInteractScanSettings.cs:13 | proposed Query input: SmartInteractionScanSettings | confirmed-declaration; provider algorithm partial |
| 2152 | SharedSmartInteractionQueries | field Terraria.GameContent.ObjectInteractions.SmartInteractScanSettings.LX : int; D:\\TRbackup\\Version4\\Terraria.GameContent.ObjectInteractions\\SmartInteractScanSettings.cs:15 | proposed Query input: SmartInteractionScanSettings | confirmed-declaration; provider algorithm partial |
| 2153 | SharedSmartInteractionQueries | field Terraria.GameContent.ObjectInteractions.SmartInteractScanSettings.HX : int; D:\\TRbackup\\Version4\\Terraria.GameContent.ObjectInteractions\\SmartInteractScanSettings.cs:17 | proposed Query input: SmartInteractionScanSettings | confirmed-declaration; provider algorithm partial |
| 2154 | SharedSmartInteractionQueries | field Terraria.GameContent.ObjectInteractions.SmartInteractScanSettings.LY : int; D:\\TRbackup\\Version4\\Terraria.GameContent.ObjectInteractions\\SmartInteractScanSettings.cs:19 | proposed Query input: SmartInteractionScanSettings | confirmed-declaration; provider algorithm partial |
| 2155 | SharedSmartInteractionQueries | field Terraria.GameContent.ObjectInteractions.SmartInteractScanSettings.HY : int; D:\\TRbackup\\Version4\\Terraria.GameContent.ObjectInteractions\\SmartInteractScanSettings.cs:21 | proposed Query input: SmartInteractionScanSettings | confirmed-declaration; provider algorithm partial |
| 2156 | SharedSmartInteractionQueries | field Terraria.GameContent.ObjectInteractions.SmartInteractSystem._candidateProvidersByOrderOfPriority : System.Collections.Generic.List<Terraria.GameContent.ObjectInteractions.ISmartInteractCandidateProvider>; D:\\TRbackup\\Version4\\Terraria.GameContent.ObjectInteractions\\SmartInteractSystem.cs:7 | proposed Query buffer: SmartInteractionCandidatePool | confirmed-declaration; provider algorithm partial |
| 2157 | SharedSmartInteractionQueries | field Terraria.GameContent.ObjectInteractions.SmartInteractSystem._blockProviders : System.Collections.Generic.List<Terraria.GameContent.ObjectInteractions.ISmartInteractBlockReasonProvider>; D:\\TRbackup\\Version4\\Terraria.GameContent.ObjectInteractions\\SmartInteractSystem.cs:9 | proposed Query registry: SmartInteractionBlockers | confirmed-declaration; provider algorithm partial |
| 2158 | SharedSmartInteractionQueries | field Terraria.GameContent.ObjectInteractions.SmartInteractSystem._candidates : System.Collections.Generic.List<Terraria.GameContent.ObjectInteractions.ISmartInteractCandidate>; D:\\TRbackup\\Version4\\Terraria.GameContent.ObjectInteractions\\SmartInteractSystem.cs:11 | proposed Query buffer: SmartInteractionCandidatePool | confirmed-declaration; provider algorithm partial |
| 2159 | SharedSmartInteractionQueries | field Terraria.GameContent.ObjectInteractions.TileSmartInteractCandidateProvider.targets : System.Collections.Generic.List<System.Tuple<int, int>>; D:\\TRbackup\\Version4\\Terraria.GameContent.ObjectInteractions\\TileSmartInteractCandidateProvider.cs:16 | proposed Query buffer: SmartInteractionTileTargets | confirmed-declaration; provider algorithm partial |
| 2160 | SharedSmartInteractionQueries | field Terraria.GameContent.ObjectInteractions.TileSmartInteractCandidateProvider._candidate : Terraria.GameContent.ObjectInteractions.TileSmartInteractCandidateProvider.ReusableCandidate; D:\\TRbackup\\Version4\\Terraria.GameContent.ObjectInteractions\\TileSmartInteractCandidateProvider.cs:18 | proposed Query buffer: SmartInteractionCandidatePool | confirmed-declaration; provider algorithm partial |
| 3823 | SharedSmartInteractionQueries | property Terraria.GameContent.ObjectInteractions.ISmartInteractCandidate.DistanceFromCursor : float; D:\\TRbackup\\Version4\\Terraria.GameContent.ObjectInteractions\\ISmartInteractCandidate.cs:5 | proposed Query result: SmartInteractionCandidateSnapshot | confirmed-declaration; provider algorithm partial |
| 3824 | SharedSmartInteractionQueries | property Terraria.GameContent.ObjectInteractions.NPCSmartInteractCandidateProvider.ReusableCandidate.DistanceFromCursor : float; D:\\TRbackup\\Version4\\Terraria.GameContent.ObjectInteractions\\NPCSmartInteractCandidateProvider.cs:11 | proposed Query result: SmartInteractionCandidateSnapshot | confirmed-declaration; provider algorithm partial |
| 3825 | SharedSmartInteractionQueries | property Terraria.GameContent.ObjectInteractions.PotionOfReturnSmartInteractCandidateProvider.ReusableCandidate.DistanceFromCursor : float; D:\\TRbackup\\Version4\\Terraria.GameContent.ObjectInteractions\\PotionOfReturnSmartInteractCandidateProvider.cs:9 | proposed Query result: SmartInteractionCandidateSnapshot | confirmed-declaration; provider algorithm partial |
| 3826 | SharedSmartInteractionQueries | property Terraria.GameContent.ObjectInteractions.ProjectileSmartInteractCandidateProvider.ReusableCandidate.DistanceFromCursor : float; D:\\TRbackup\\Version4\\Terraria.GameContent.ObjectInteractions\\ProjectileSmartInteractCandidateProvider.cs:11 | proposed Query result: SmartInteractionCandidateSnapshot | confirmed-declaration; provider algorithm partial |
| 3827 | SharedSmartInteractionQueries | property Terraria.GameContent.ObjectInteractions.TileSmartInteractCandidateProvider.ReusableCandidate.DistanceFromCursor : float; D:\\TRbackup\\Version4\\Terraria.GameContent.ObjectInteractions\\TileSmartInteractCandidateProvider.cs:13 | proposed Query result: SmartInteractionCandidateSnapshot | confirmed-declaration; provider algorithm partial |
| 1188 | PlayerItemPickupAndRespawnState | field Terraria.DataStructures.MinionRespawner._minions : System.Collections.Generic.List<Terraria.DataStructures.MinionSpawnInfo>; D:\\TRbackup\\Version4\\Terraria.DataStructures\\MinionRespawner.cs:7 | proposed Component: MinionRespawnState | confirmed-declaration; respawn/pickup behavior partial |
| 1189 | PlayerItemPickupAndRespawnState | field Terraria.DataStructures.MinionSpawnFromInventoryItem.ItemType : int; D:\\TRbackup\\Version4\\Terraria.DataStructures\\MinionSpawnFromInventoryItem.cs:5 | proposed Component: MinionRespawnState | confirmed-declaration; respawn/pickup behavior partial |
| 1190 | PlayerItemPickupAndRespawnState | field Terraria.DataStructures.MinionSpawnFromInventoryItem.ItemPrefix : int; D:\\TRbackup\\Version4\\Terraria.DataStructures\\MinionSpawnFromInventoryItem.cs:7 | proposed Component: MinionRespawnState | confirmed-declaration; respawn/pickup behavior partial |
| 1230 | PlayerItemPickupAndRespawnState | field Terraria.DataStructures.PlayerGetItemLogger.GetItemLoggerEntry.TargetArray : Terraria.Item[]; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerGetItemLogger.cs:9 | proposed Value Object: PlayerItemPickupLogEntry | confirmed-declaration; respawn/pickup behavior partial |
| 1231 | PlayerItemPickupAndRespawnState | field Terraria.DataStructures.PlayerGetItemLogger.GetItemLoggerEntry.TargetSlot : int; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerGetItemLogger.cs:11 | proposed Value Object: PlayerItemPickupLogEntry | confirmed-declaration; respawn/pickup behavior partial |
| 1232 | PlayerItemPickupAndRespawnState | field Terraria.DataStructures.PlayerGetItemLogger.GetItemLoggerEntry.TargetItemSlotContext : int; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerGetItemLogger.cs:13 | proposed Value Object: PlayerItemPickupLogEntry | confirmed-declaration; respawn/pickup behavior partial |
| 1233 | PlayerItemPickupAndRespawnState | field Terraria.DataStructures.PlayerGetItemLogger.GetItemLoggerEntry.Stack : int; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerGetItemLogger.cs:15 | proposed Value Object: PlayerItemPickupLogEntry | confirmed-declaration; respawn/pickup behavior partial |
| 1234 | PlayerItemPickupAndRespawnState | field Terraria.DataStructures.PlayerGetItemLogger.Entries : System.Collections.Generic.List<Terraria.DataStructures.PlayerGetItemLogger.GetItemLoggerEntry>; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerGetItemLogger.cs:18 | proposed Component: PlayerItemPickupLog | confirmed-declaration; respawn/pickup behavior partial |
| 1235 | PlayerItemPickupAndRespawnState | field Terraria.DataStructures.PlayerGetItemLogger._enabled : bool; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerGetItemLogger.cs:20 | proposed Component: PlayerItemPickupLog | confirmed-declaration; respawn/pickup behavior partial |
| 1272 | PlayerPreviewAndRejectionState | field Terraria.DataStructures.RejectionMenuInfo.ExitAction : Terraria.DataStructures.ReturnFromRejectionMenuAction; D:\\TRbackup\\Version4\\Terraria.DataStructures\\RejectionMenuInfo.cs:7 | proposed Component/Projection: PlayerRejectionMenuState | confirmed-declaration; presentation behavior partial |
| 1273 | PlayerPreviewAndRejectionState | field Terraria.DataStructures.RejectionMenuInfo.TextToShow : string; D:\\TRbackup\\Version4\\Terraria.DataStructures\\RejectionMenuInfo.cs:9 | proposed Component/Projection: PlayerRejectionMenuState | confirmed-declaration; presentation behavior partial |
| 1274 | PlayerPreviewAndRejectionState | field Terraria.DataStructures.SettingsForCharacterPreview.SelectionBasedSettings.StartFrame : int; D:\\TRbackup\\Version4\\Terraria.DataStructures\\SettingsForCharacterPreview.cs:11 | proposed Component: CharacterPreviewAnimationSettings | confirmed-declaration; presentation behavior partial |
| 1275 | PlayerPreviewAndRejectionState | field Terraria.DataStructures.SettingsForCharacterPreview.SelectionBasedSettings.FrameCount : int; D:\\TRbackup\\Version4\\Terraria.DataStructures\\SettingsForCharacterPreview.cs:13 | proposed Component: CharacterPreviewAnimationSettings | confirmed-declaration; presentation behavior partial |
| 1276 | PlayerPreviewAndRejectionState | field Terraria.DataStructures.SettingsForCharacterPreview.SelectionBasedSettings.DelayPerFrame : int; D:\\TRbackup\\Version4\\Terraria.DataStructures\\SettingsForCharacterPreview.cs:15 | proposed Component: CharacterPreviewAnimationSettings | confirmed-declaration; presentation behavior partial |
| 1277 | PlayerPreviewAndRejectionState | field Terraria.DataStructures.SettingsForCharacterPreview.SelectionBasedSettings.BounceLoop : bool; D:\\TRbackup\\Version4\\Terraria.DataStructures\\SettingsForCharacterPreview.cs:17 | proposed Component: CharacterPreviewAnimationSettings | confirmed-declaration; presentation behavior partial |
| 1278 | PlayerPreviewAndRejectionState | field Terraria.DataStructures.SettingsForCharacterPreview.Offset : Vector2; D:\\TRbackup\\Version4\\Terraria.DataStructures\\SettingsForCharacterPreview.cs:20 | proposed Component: CharacterPreviewAnimationSettings | confirmed-declaration; presentation behavior partial |
| 1279 | PlayerPreviewAndRejectionState | field Terraria.DataStructures.SettingsForCharacterPreview.Selected : Terraria.DataStructures.SettingsForCharacterPreview.SelectionBasedSettings; D:\\TRbackup\\Version4\\Terraria.DataStructures\\SettingsForCharacterPreview.cs:22 | proposed Component: CharacterPreviewAnimationSettings | confirmed-declaration; presentation behavior partial |
| 1280 | PlayerPreviewAndRejectionState | field Terraria.DataStructures.SettingsForCharacterPreview.NotSelected : Terraria.DataStructures.SettingsForCharacterPreview.SelectionBasedSettings; D:\\TRbackup\\Version4\\Terraria.DataStructures\\SettingsForCharacterPreview.cs:24 | proposed Component: CharacterPreviewAnimationSettings | confirmed-declaration; presentation behavior partial |
| 1281 | PlayerPreviewAndRejectionState | field Terraria.DataStructures.SettingsForCharacterPreview.SpriteDirection : int; D:\\TRbackup\\Version4\\Terraria.DataStructures\\SettingsForCharacterPreview.cs:26 | proposed Component: CharacterPreviewAnimationSettings | confirmed-declaration; presentation behavior partial |
| 1282 | PlayerPreviewAndRejectionState | field Terraria.DataStructures.SettingsForCharacterPreview.CustomAnimation : Terraria.DataStructures.SettingsForCharacterPreview.CustomAnimationCode; D:\\TRbackup\\Version4\\Terraria.DataStructures\\SettingsForCharacterPreview.cs:28 | proposed Component: CharacterPreviewAnimationSettings | confirmed-declaration; presentation behavior partial |
| 2374 | DoorOpeningInteractionState | field Terraria.GameContent.DoorOpeningHelper.DoorOpenCloseTogglingInfo.tileCoordsForToggling : Point; D:\\TRbackup\\Version4\\Terraria.GameContent\\DoorOpeningHelper.cs:21 | proposed Component/System: DoorOpeningState and DoorOpeningSystem | confirmed-declaration; handler behavior partial |
| 2375 | DoorOpeningInteractionState | field Terraria.GameContent.DoorOpeningHelper.DoorOpenCloseTogglingInfo.handler : Terraria.GameContent.DoorOpeningHelper.DoorAutoHandler; D:\\TRbackup\\Version4\\Terraria.GameContent\\DoorOpeningHelper.cs:23 | proposed Component/System: DoorOpeningState and DoorOpeningSystem | confirmed-declaration; handler behavior partial |
| 2376 | DoorOpeningInteractionState | field Terraria.GameContent.DoorOpeningHelper.PlayerInfoForOpeningDoors.hitboxToOpenDoor : Rectangle; D:\\TRbackup\\Version4\\Terraria.GameContent\\DoorOpeningHelper.cs:28 | proposed Component/System: DoorOpeningState and DoorOpeningSystem | confirmed-declaration; handler behavior partial |
| 2377 | DoorOpeningInteractionState | field Terraria.GameContent.DoorOpeningHelper.PlayerInfoForOpeningDoors.intendedOpeningDirection : int; D:\\TRbackup\\Version4\\Terraria.GameContent\\DoorOpeningHelper.cs:30 | proposed Component/System: DoorOpeningState and DoorOpeningSystem | confirmed-declaration; handler behavior partial |
| 2378 | DoorOpeningInteractionState | field Terraria.GameContent.DoorOpeningHelper.PlayerInfoForOpeningDoors.playerGravityDirection : int; D:\\TRbackup\\Version4\\Terraria.GameContent\\DoorOpeningHelper.cs:32 | proposed Component/System: DoorOpeningState and DoorOpeningSystem | confirmed-declaration; handler behavior partial |
| 2379 | DoorOpeningInteractionState | field Terraria.GameContent.DoorOpeningHelper.PlayerInfoForOpeningDoors.tileCoordSpaceForCheckingForDoors : Rectangle; D:\\TRbackup\\Version4\\Terraria.GameContent\\DoorOpeningHelper.cs:34 | proposed Component/System: DoorOpeningState and DoorOpeningSystem | confirmed-declaration; handler behavior partial |
| 2380 | DoorOpeningInteractionState | field Terraria.GameContent.DoorOpeningHelper.PlayerInfoForClosingDoors.hitboxToNotCloseDoor : Rectangle; D:\\TRbackup\\Version4\\Terraria.GameContent\\DoorOpeningHelper.cs:39 | proposed Component/System: DoorOpeningState and DoorOpeningSystem | confirmed-declaration; handler behavior partial |
| 2381 | DoorOpeningInteractionState | field Terraria.GameContent.DoorOpeningHelper._handlerByTileType : System.Collections.Generic.Dictionary<int, Terraria.GameContent.DoorOpeningHelper.DoorAutoHandler>; D:\\TRbackup\\Version4\\Terraria.GameContent\\DoorOpeningHelper.cs:77 | proposed Component/System: DoorOpeningState and DoorOpeningSystem | confirmed-declaration; handler behavior partial |
| 2382 | DoorOpeningInteractionState | field Terraria.GameContent.DoorOpeningHelper._ongoingOpenDoors : System.Collections.Generic.List<Terraria.GameContent.DoorOpeningHelper.DoorOpenCloseTogglingInfo>; D:\\TRbackup\\Version4\\Terraria.GameContent\\DoorOpeningHelper.cs:89 | proposed Component/System: DoorOpeningState and DoorOpeningSystem | confirmed-declaration; handler behavior partial |
| 2383 | DoorOpeningInteractionState | field Terraria.GameContent.DoorOpeningHelper._timeWeCanOpenDoorsUsingVelocityAlone : int; D:\\TRbackup\\Version4\\Terraria.GameContent\\DoorOpeningHelper.cs:91 | proposed Component/System: DoorOpeningState and DoorOpeningSystem | confirmed-declaration; handler behavior partial |
| 2519 | SmartCursorInteractionState | field Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo.player : Terraria.Player; D:\\TRbackup\\Version4\\Terraria.GameContent\\SmartCursorHelper.cs:16 | proposed Query input/result: SmartCursorUsageInfo | confirmed-declaration; algorithm partial |
| 2520 | SmartCursorInteractionState | field Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo.item : Terraria.Item; D:\\TRbackup\\Version4\\Terraria.GameContent\\SmartCursorHelper.cs:18 | proposed Query input/result: SmartCursorUsageInfo | confirmed-declaration; algorithm partial |
| 2521 | SmartCursorInteractionState | field Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo.mouse : Vector2; D:\\TRbackup\\Version4\\Terraria.GameContent\\SmartCursorHelper.cs:20 | proposed Query input/result: SmartCursorUsageInfo | confirmed-declaration; algorithm partial |
| 2522 | SmartCursorInteractionState | field Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo.position : Vector2; D:\\TRbackup\\Version4\\Terraria.GameContent\\SmartCursorHelper.cs:22 | proposed Query input/result: SmartCursorUsageInfo | confirmed-declaration; algorithm partial |
| 2523 | SmartCursorInteractionState | field Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo.Center : Vector2; D:\\TRbackup\\Version4\\Terraria.GameContent\\SmartCursorHelper.cs:24 | proposed Query input/result: SmartCursorUsageInfo | confirmed-declaration; algorithm partial |
| 2524 | SmartCursorInteractionState | field Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo.screenTargetX : int; D:\\TRbackup\\Version4\\Terraria.GameContent\\SmartCursorHelper.cs:26 | proposed Query result: SmartCursorTargetUsage | confirmed-declaration; algorithm partial |
| 2525 | SmartCursorInteractionState | field Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo.screenTargetY : int; D:\\TRbackup\\Version4\\Terraria.GameContent\\SmartCursorHelper.cs:28 | proposed Query result: SmartCursorTargetUsage | confirmed-declaration; algorithm partial |
| 2526 | SmartCursorInteractionState | field Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo.reachableStartX : int; D:\\TRbackup\\Version4\\Terraria.GameContent\\SmartCursorHelper.cs:30 | proposed Query result: SmartCursorTargetUsage | confirmed-declaration; algorithm partial |
| 2527 | SmartCursorInteractionState | field Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo.reachableEndX : int; D:\\TRbackup\\Version4\\Terraria.GameContent\\SmartCursorHelper.cs:32 | proposed Query result: SmartCursorTargetUsage | confirmed-declaration; algorithm partial |
| 2528 | SmartCursorInteractionState | field Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo.reachableStartY : int; D:\\TRbackup\\Version4\\Terraria.GameContent\\SmartCursorHelper.cs:34 | proposed Query result: SmartCursorTargetUsage | confirmed-declaration; algorithm partial |
| 2529 | SmartCursorInteractionState | field Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo.reachableEndY : int; D:\\TRbackup\\Version4\\Terraria.GameContent\\SmartCursorHelper.cs:36 | proposed Query result: SmartCursorTargetUsage | confirmed-declaration; algorithm partial |
| 2530 | SmartCursorInteractionState | field Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo.paintLookup : int; D:\\TRbackup\\Version4\\Terraria.GameContent\\SmartCursorHelper.cs:38 | proposed Query result: SmartCursorTargetUsage | confirmed-declaration; algorithm partial |
| 2531 | SmartCursorInteractionState | field Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo.paintCoatingLookup : int; D:\\TRbackup\\Version4\\Terraria.GameContent\\SmartCursorHelper.cs:40 | proposed Query result: SmartCursorTargetUsage | confirmed-declaration; algorithm partial |
| 2532 | SmartCursorInteractionState | field Terraria.GameContent.SmartCursorHelper._targets : System.Collections.Generic.List<Point>; D:\\TRbackup\\Version4\\Terraria.GameContent\\SmartCursorHelper.cs:43 | proposed Query buffer/System state: SmartCursorTargetBuffer | confirmed-declaration; algorithm partial |
| 2533 | SmartCursorInteractionState | field Terraria.GameContent.SmartCursorHelper._grappleTargets : System.Collections.Generic.List<Point>; D:\\TRbackup\\Version4\\Terraria.GameContent\\SmartCursorHelper.cs:45 | proposed Query buffer/System state: SmartCursorTargetBuffer | confirmed-declaration; algorithm partial |
| 2534 | SmartCursorInteractionState | field Terraria.GameContent.SmartCursorHelper._points : System.Collections.Generic.List<Point>; D:\\TRbackup\\Version4\\Terraria.GameContent\\SmartCursorHelper.cs:47 | proposed Query buffer/System state: SmartCursorTargetBuffer | confirmed-declaration; algorithm partial |
| 2535 | SmartCursorInteractionState | field Terraria.GameContent.SmartCursorHelper._endpoints : System.Collections.Generic.List<Point>; D:\\TRbackup\\Version4\\Terraria.GameContent\\SmartCursorHelper.cs:49 | proposed Query buffer/System state: SmartCursorTargetBuffer | confirmed-declaration; algorithm partial |
| 2536 | SmartCursorInteractionState | field Terraria.GameContent.SmartCursorHelper._toRemove : System.Collections.Generic.List<Point>; D:\\TRbackup\\Version4\\Terraria.GameContent\\SmartCursorHelper.cs:51 | proposed Query buffer/System state: SmartCursorTargetBuffer | confirmed-declaration; algorithm partial |
| 2537 | SmartCursorInteractionState | field Terraria.GameContent.SmartCursorHelper._targets2 : System.Collections.Generic.List<Point>; D:\\TRbackup\\Version4\\Terraria.GameContent\\SmartCursorHelper.cs:53 | proposed Query buffer/System state: SmartCursorTargetBuffer | confirmed-declaration; algorithm partial |
| 2467 | PressurePlateInteractionState | field Terraria.GameContent.PressurePlateHelper.EntityCreationLock : object; D:\\TRbackup\\Version4\\Terraria.GameContent\\PressurePlateHelper.cs:9 | proposed Adapter: PressurePlateEntityCreationLock | confirmed-declaration; movement/occupancy behavior partial |
| 2468 | PressurePlateInteractionState | field Terraria.GameContent.PressurePlateHelper.PressurePlatesPressed : System.Collections.Generic.Dictionary<Point, bool[]>; D:\\TRbackup\\Version4\\Terraria.GameContent\\PressurePlateHelper.cs:11 | proposed Component: PressurePlateOccupancy | confirmed-declaration; movement/occupancy behavior partial |
| 2469 | PressurePlateInteractionState | field Terraria.GameContent.PressurePlateHelper.NeedsFirstUpdate : bool; D:\\TRbackup\\Version4\\Terraria.GameContent\\PressurePlateHelper.cs:13 | proposed Component: PressurePlateUpdateLifecycle | confirmed-declaration; movement/occupancy behavior partial |
| 2470 | PressurePlateInteractionState | field Terraria.GameContent.PressurePlateHelper.PlayerLastPosition : Vector2[]; D:\\TRbackup\\Version4\\Terraria.GameContent\\PressurePlateHelper.cs:15 | proposed Component: PressurePlatePlayerPositionCache | confirmed-declaration; movement/occupancy behavior partial |
| 2471 | PressurePlateInteractionState | field Terraria.GameContent.PressurePlateHelper.pressurePlateBounds : Rectangle; D:\\TRbackup\\Version4\\Terraria.GameContent\\PressurePlateHelper.cs:17 | proposed Definition: PressurePlateBounds | confirmed-declaration; movement/occupancy behavior partial |
| 2419 | CursorAndChestInteractionState | field Terraria.GameContent.FakeCursorItem._type : int; D:\\TRbackup\\Version4\\Terraria.GameContent\\FakeCursorItem.cs:5 | proposed Component: CursorItemPreview | confirmed-declaration; cursor/chest lifecycle partial |
| 2420 | CursorAndChestInteractionState | field Terraria.GameContent.FakeCursorItem._stack : int; D:\\TRbackup\\Version4\\Terraria.GameContent\\FakeCursorItem.cs:7 | proposed Component: CursorItemPreview | confirmed-declaration; cursor/chest lifecycle partial |
| 2421 | CursorAndChestInteractionState | field Terraria.GameContent.FakeCursorItem._prefix : int; D:\\TRbackup\\Version4\\Terraria.GameContent\\FakeCursorItem.cs:9 | proposed Component: CursorItemPreview | confirmed-declaration; cursor/chest lifecycle partial |
| 2422 | CursorAndChestInteractionState | field Terraria.GameContent.FakeCursorItem._item : Terraria.Item; D:\\TRbackup\\Version4\\Terraria.GameContent\\FakeCursorItem.cs:11 | proposed Component: CursorItemPreview | confirmed-declaration; cursor/chest lifecycle partial |
| 2465 | CursorAndChestInteractionState | field Terraria.GameContent.PositionedChest.chest : Terraria.Chest; D:\\TRbackup\\Version4\\Terraria.GameContent\\PositionedChest.cs:7 | proposed Snapshot: PositionedChest | confirmed-declaration; cursor/chest lifecycle partial |
| 2466 | CursorAndChestInteractionState | field Terraria.GameContent.PositionedChest.position : Vector2; D:\\TRbackup\\Version4\\Terraria.GameContent\\PositionedChest.cs:9 | proposed Snapshot: PositionedChest | confirmed-declaration; cursor/chest lifecycle partial |
| 3853 | CursorAndChestInteractionState | property Terraria.GameContent.FakeCursorItem.Item : Terraria.Item; D:\\TRbackup\\Version4\\Terraria.GameContent\\FakeCursorItem.cs:13 | proposed Query: FakeCursorItemPreview | confirmed-declaration; cursor/chest lifecycle partial |
| 2600 | InputProfilesAndConfiguration | field Terraria.GameInput.KeyConfiguration.KeyStatus : System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>; D:\\TRbackup\\Version4\\Terraria.GameInput\\KeyConfiguration.cs:8 | proposed Component: InputBindingConfiguration | confirmed-declaration; profile persistence partial |
| 2609 | InputProfilesAndConfiguration | field Terraria.GameInput.PlayerInputProfile.InputModes : System.Collections.Generic.Dictionary<Terraria.GameInput.InputMode, Terraria.GameInput.KeyConfiguration>; D:\\TRbackup\\Version4\\Terraria.GameInput\\PlayerInputProfile.cs:11 | proposed Component: PlayerInputProfileConfiguration | confirmed-declaration; profile persistence partial |
| 2610 | InputProfilesAndConfiguration | field Terraria.GameInput.PlayerInputProfile.Name : string; D:\\TRbackup\\Version4\\Terraria.GameInput\\PlayerInputProfile.cs:31 | proposed Component: PlayerInputProfileConfiguration | confirmed-declaration; profile persistence partial |
| 2611 | InputProfilesAndConfiguration | field Terraria.GameInput.PlayerInputProfile.AllowEditing : bool; D:\\TRbackup\\Version4\\Terraria.GameInput\\PlayerInputProfile.cs:33 | proposed Component: PlayerInputProfileConfiguration | confirmed-declaration; profile persistence partial |
| 2612 | InputProfilesAndConfiguration | field Terraria.GameInput.PlayerInputProfile.HotbarRadialHoldTimeRequired : int; D:\\TRbackup\\Version4\\Terraria.GameInput\\PlayerInputProfile.cs:35 | proposed Component: PlayerInputProfileConfiguration | confirmed-declaration; profile persistence partial |
| 2613 | InputProfilesAndConfiguration | field Terraria.GameInput.PlayerInputProfile.TriggersDeadzone : float; D:\\TRbackup\\Version4\\Terraria.GameInput\\PlayerInputProfile.cs:37 | proposed Component: PlayerInputProfileConfiguration | confirmed-declaration; profile persistence partial |
| 2614 | InputProfilesAndConfiguration | field Terraria.GameInput.PlayerInputProfile.InterfaceDeadzoneX : float; D:\\TRbackup\\Version4\\Terraria.GameInput\\PlayerInputProfile.cs:39 | proposed Component: PlayerInputProfileConfiguration | confirmed-declaration; profile persistence partial |
| 2615 | InputProfilesAndConfiguration | field Terraria.GameInput.PlayerInputProfile.LeftThumbstickDeadzoneX : float; D:\\TRbackup\\Version4\\Terraria.GameInput\\PlayerInputProfile.cs:41 | proposed Component: PlayerInputProfileConfiguration | confirmed-declaration; profile persistence partial |
| 2616 | InputProfilesAndConfiguration | field Terraria.GameInput.PlayerInputProfile.LeftThumbstickDeadzoneY : float; D:\\TRbackup\\Version4\\Terraria.GameInput\\PlayerInputProfile.cs:43 | proposed Component: PlayerInputProfileConfiguration | confirmed-declaration; profile persistence partial |
| 2617 | InputProfilesAndConfiguration | field Terraria.GameInput.PlayerInputProfile.RightThumbstickDeadzoneX : float; D:\\TRbackup\\Version4\\Terraria.GameInput\\PlayerInputProfile.cs:45 | proposed Component: PlayerInputProfileConfiguration | confirmed-declaration; profile persistence partial |
| 2618 | InputProfilesAndConfiguration | field Terraria.GameInput.PlayerInputProfile.RightThumbstickDeadzoneY : float; D:\\TRbackup\\Version4\\Terraria.GameInput\\PlayerInputProfile.cs:47 | proposed Component: PlayerInputProfileConfiguration | confirmed-declaration; profile persistence partial |
| 2619 | InputProfilesAndConfiguration | field Terraria.GameInput.PlayerInputProfile.LeftThumbstickInvertX : bool; D:\\TRbackup\\Version4\\Terraria.GameInput\\PlayerInputProfile.cs:49 | proposed Component: PlayerInputProfileConfiguration | confirmed-declaration; profile persistence partial |
| 2620 | InputProfilesAndConfiguration | field Terraria.GameInput.PlayerInputProfile.LeftThumbstickInvertY : bool; D:\\TRbackup\\Version4\\Terraria.GameInput\\PlayerInputProfile.cs:51 | proposed Component: PlayerInputProfileConfiguration | confirmed-declaration; profile persistence partial |
| 2621 | InputProfilesAndConfiguration | field Terraria.GameInput.PlayerInputProfile.RightThumbstickInvertX : bool; D:\\TRbackup\\Version4\\Terraria.GameInput\\PlayerInputProfile.cs:53 | proposed Component: PlayerInputProfileConfiguration | confirmed-declaration; profile persistence partial |
| 2622 | InputProfilesAndConfiguration | field Terraria.GameInput.PlayerInputProfile.RightThumbstickInvertY : bool; D:\\TRbackup\\Version4\\Terraria.GameInput\\PlayerInputProfile.cs:55 | proposed Component: PlayerInputProfileConfiguration | confirmed-declaration; profile persistence partial |
| 2623 | InputProfilesAndConfiguration | field Terraria.GameInput.PlayerInputProfile.InventoryMoveCD : int; D:\\TRbackup\\Version4\\Terraria.GameInput\\PlayerInputProfile.cs:57 | proposed Component: PlayerInputProfileConfiguration | confirmed-declaration; profile persistence partial |
| 2624 | InputTriggerState | field Terraria.GameInput.TriggersPack.Current : Terraria.GameInput.TriggersSet; D:\\TRbackup\\Version4\\Terraria.GameInput\\TriggersPack.cs:7 | proposed Component: InputTriggerFrameState | confirmed-declaration; frame transition partial |
| 2625 | InputTriggerState | field Terraria.GameInput.TriggersPack.Old : Terraria.GameInput.TriggersSet; D:\\TRbackup\\Version4\\Terraria.GameInput\\TriggersPack.cs:9 | proposed Component: InputTriggerFrameState | confirmed-declaration; frame transition partial |
| 2626 | InputTriggerState | field Terraria.GameInput.TriggersPack.JustPressed : Terraria.GameInput.TriggersSet; D:\\TRbackup\\Version4\\Terraria.GameInput\\TriggersPack.cs:11 | proposed Component: InputTriggerFrameState | confirmed-declaration; frame transition partial |
| 2627 | InputTriggerState | field Terraria.GameInput.TriggersPack.JustReleased : Terraria.GameInput.TriggersSet; D:\\TRbackup\\Version4\\Terraria.GameInput\\TriggersPack.cs:13 | proposed Component: InputTriggerFrameState | confirmed-declaration; frame transition partial |
| 2628 | InputTriggerState | field Terraria.GameInput.TriggersSet.KeyStatus : System.Collections.Generic.Dictionary<string, bool>; D:\\TRbackup\\Version4\\Terraria.GameInput\\TriggersSet.cs:9 | proposed Component: InputTriggerFrameState | confirmed-declaration; frame transition partial |
| 2629 | InputTriggerState | field Terraria.GameInput.TriggersSet.LatestInputMode : System.Collections.Generic.Dictionary<string, Terraria.GameInput.InputMode>; D:\\TRbackup\\Version4\\Terraria.GameInput\\TriggersSet.cs:11 | proposed Component: InputTriggerFrameState | confirmed-declaration; frame transition partial |
| 2630 | InputTriggerState | field Terraria.GameInput.TriggersSet.UsedMovementKey : bool; D:\\TRbackup\\Version4\\Terraria.GameInput\\TriggersSet.cs:13 | proposed Component: InputTriggerFrameState | confirmed-declaration; frame transition partial |
| 2631 | InputTriggerState | field Terraria.GameInput.TriggersSet.HotbarScrollCD : int; D:\\TRbackup\\Version4\\Terraria.GameInput\\TriggersSet.cs:15 | proposed Component: InputTriggerFrameState | confirmed-declaration; frame transition partial |
| 2632 | InputTriggerState | field Terraria.GameInput.TriggersSet.HotbarHoldTime : int; D:\\TRbackup\\Version4\\Terraria.GameInput\\TriggersSet.cs:17 | proposed Component: InputTriggerFrameState | confirmed-declaration; frame transition partial |
| 2606 | PlayerInputRuntimeState | field Terraria.GameInput.PlayerInput.LockGamepadTileUseButton : bool; D:\\TRbackup\\Version4\\Terraria.GameInput\\PlayerInput.cs:22 | proposed Component: PlayerInputRuntimeState | confirmed-declaration; viewport integration partial |
| 2607 | PlayerInputRuntimeState | field Terraria.GameInput.PlayerInput._originalScreenWidth : int; D:\\TRbackup\\Version4\\Terraria.GameInput\\PlayerInput.cs:24 | proposed Component: PlayerInputRuntimeState | confirmed-declaration; viewport integration partial |
| 2608 | PlayerInputRuntimeState | field Terraria.GameInput.PlayerInput._originalScreenHeight : int; D:\\TRbackup\\Version4\\Terraria.GameInput\\PlayerInput.cs:26 | proposed Component: PlayerInputRuntimeState | confirmed-declaration; viewport integration partial |
| 3871 | PlayerInputRuntimeState | property Terraria.GameInput.PlayerInput.OriginalScreenSize : Vector2; D:\\TRbackup\\Version4\\Terraria.GameInput\\PlayerInput.cs:29 | proposed Query/Adapter: OriginalViewportSize | confirmed-declaration; viewport integration partial |
| 3074 | EquipmentLoadoutState | field Terraria.EquipmentLoadout.Armor : Terraria.Item[]; D:\\TRbackup\\Version4\\Terraria\\EquipmentLoadout.cs:8 | proposed Component/System: EquipmentLoadout | confirmed-declaration; swap/drop lifecycle partial |
| 3075 | EquipmentLoadoutState | field Terraria.EquipmentLoadout.Dye : Terraria.Item[]; D:\\TRbackup\\Version4\\Terraria\\EquipmentLoadout.cs:10 | proposed Component/System: EquipmentLoadout | confirmed-declaration; swap/drop lifecycle partial |
| 3076 | EquipmentLoadoutState | field Terraria.EquipmentLoadout.Hide : bool[]; D:\\TRbackup\\Version4\\Terraria\\EquipmentLoadout.cs:12 | proposed Component/System: EquipmentLoadout | confirmed-declaration; swap/drop lifecycle partial |
| 3772 | SharedCreativePowerContracts | property Terraria.GameContent.Creative.ICreativePower.PowerId : ushort; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\ICreativePower.cs:9 | proposed Contract/Adapter: CreativePowerContract | confirmed-declaration; contract/network behavior partial |
| 3773 | SharedCreativePowerContracts | property Terraria.GameContent.Creative.ICreativePower.ServerConfigName : string; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\ICreativePower.cs:11 | proposed Contract/Adapter: CreativePowerContract | confirmed-declaration; contract/network behavior partial |
| 3774 | SharedCreativePowerContracts | property Terraria.GameContent.Creative.ICreativePower.CurrentPermissionLevel : Terraria.GameContent.Creative.PowerPermissionLevel; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\ICreativePower.cs:13 | proposed Contract/Adapter: CreativePowerContract | confirmed-declaration; contract/network behavior partial |
| 3775 | SharedCreativePowerContracts | property Terraria.GameContent.Creative.ICreativePower.DefaultPermissionLevel : Terraria.GameContent.Creative.PowerPermissionLevel; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\ICreativePower.cs:15 | proposed Contract/Adapter: CreativePowerContract | confirmed-declaration; contract/network behavior partial |
| 1250 | PlayerMovementCapabilityState | field Terraria.DataStructures.PlayerMovementAccsCache._readyToPaste : bool; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerMovementAccsCache.cs:5 | proposed Snapshot: MountMovementRestoreState | confirmed-declaration; mount integration partial |
| 1251 | PlayerMovementCapabilityState | field Terraria.DataStructures.PlayerMovementAccsCache._mountPreventedFlight : bool; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerMovementAccsCache.cs:7 | proposed Snapshot: MountMovementRestoreState | confirmed-declaration; mount integration partial |
| 1252 | PlayerMovementCapabilityState | field Terraria.DataStructures.PlayerMovementAccsCache._mountPreventedExtraJumps : bool; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerMovementAccsCache.cs:9 | proposed Snapshot: MountMovementRestoreState | confirmed-declaration; mount integration partial |
| 1253 | PlayerMovementCapabilityState | field Terraria.DataStructures.PlayerMovementAccsCache.rocketTime : int; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerMovementAccsCache.cs:11 | proposed Snapshot: MountMovementRestoreState | confirmed-declaration; mount integration partial |
| 1254 | PlayerMovementCapabilityState | field Terraria.DataStructures.PlayerMovementAccsCache.wingTime : float; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerMovementAccsCache.cs:13 | proposed Snapshot: MountMovementRestoreState | confirmed-declaration; mount integration partial |
| 1255 | PlayerMovementCapabilityState | field Terraria.DataStructures.PlayerMovementAccsCache.rocketDelay : int; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerMovementAccsCache.cs:15 | proposed Snapshot: MountMovementRestoreState | confirmed-declaration; mount integration partial |
| 1256 | PlayerMovementCapabilityState | field Terraria.DataStructures.PlayerMovementAccsCache.rocketDelay2 : int; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerMovementAccsCache.cs:17 | proposed Snapshot: MountMovementRestoreState | confirmed-declaration; mount integration partial |
| 1257 | PlayerMovementCapabilityState | field Terraria.DataStructures.PlayerMovementAccsCache.jumpAgainCloud : bool; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerMovementAccsCache.cs:19 | proposed Snapshot: MountMovementRestoreState | confirmed-declaration; mount integration partial |
| 1258 | PlayerMovementCapabilityState | field Terraria.DataStructures.PlayerMovementAccsCache.jumpAgainSandstorm : bool; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerMovementAccsCache.cs:21 | proposed Snapshot: MountMovementRestoreState | confirmed-declaration; mount integration partial |
| 1259 | PlayerMovementCapabilityState | field Terraria.DataStructures.PlayerMovementAccsCache.jumpAgainBlizzard : bool; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerMovementAccsCache.cs:23 | proposed Snapshot: MountMovementRestoreState | confirmed-declaration; mount integration partial |
| 1260 | PlayerMovementCapabilityState | field Terraria.DataStructures.PlayerMovementAccsCache.jumpAgainFart : bool; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerMovementAccsCache.cs:25 | proposed Snapshot: MountMovementRestoreState | confirmed-declaration; mount integration partial |
| 1261 | PlayerMovementCapabilityState | field Terraria.DataStructures.PlayerMovementAccsCache.jumpAgainSail : bool; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerMovementAccsCache.cs:27 | proposed Snapshot: MountMovementRestoreState | confirmed-declaration; mount integration partial |
| 1262 | PlayerMovementCapabilityState | field Terraria.DataStructures.PlayerMovementAccsCache.jumpAgainUnicorn : bool; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerMovementAccsCache.cs:29 | proposed Snapshot: MountMovementRestoreState | confirmed-declaration; mount integration partial |
| 1267 | PlayerMovementCapabilityState | field Terraria.DataStructures.PortableStoolUsage.HasAStool : bool; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PortableStoolUsage.cs:5 | proposed Component: PortableStoolUsage | confirmed-declaration; mount integration partial |
| 1268 | PlayerMovementCapabilityState | field Terraria.DataStructures.PortableStoolUsage.IsInUse : bool; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PortableStoolUsage.cs:7 | proposed Component: PortableStoolUsage | confirmed-declaration; mount integration partial |
| 1269 | PlayerMovementCapabilityState | field Terraria.DataStructures.PortableStoolUsage.HeightBoost : int; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PortableStoolUsage.cs:9 | proposed Component: PortableStoolUsage | confirmed-declaration; mount integration partial |
| 1270 | PlayerMovementCapabilityState | field Terraria.DataStructures.PortableStoolUsage.VisualYOffset : int; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PortableStoolUsage.cs:11 | proposed Component: PortableStoolUsage | confirmed-declaration; mount integration partial |
| 1271 | PlayerMovementCapabilityState | field Terraria.DataStructures.PortableStoolUsage.MapYOffset : int; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PortableStoolUsage.cs:13 | proposed Component: PortableStoolUsage | confirmed-declaration; mount integration partial |
| 1236 | PlayerIntentAndInteractionState | field Terraria.DataStructures.PlayerIntentionGuesser.LastX : int; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerIntentionGuesser.cs:8 | proposed Component/System: PlayerIntentionState and IntentionTrackingSystem | confirmed-declaration; intention tracking partial |
| 1237 | PlayerIntentAndInteractionState | field Terraria.DataStructures.PlayerIntentionGuesser.LastY : int; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerIntentionGuesser.cs:10 | proposed Component/System: PlayerIntentionState and IntentionTrackingSystem | confirmed-declaration; intention tracking partial |
| 1238 | PlayerIntentAndInteractionState | field Terraria.DataStructures.PlayerIntentionGuesser.LastPosition : Vector2; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerIntentionGuesser.cs:12 | proposed Component/System: PlayerIntentionState and IntentionTrackingSystem | confirmed-declaration; intention tracking partial |
| 1239 | PlayerIntentAndInteractionState | field Terraria.DataStructures.PlayerIntentionGuesser.LastCenter : Vector2; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerIntentionGuesser.cs:14 | proposed Component/System: PlayerIntentionState and IntentionTrackingSystem | confirmed-declaration; intention tracking partial |
| 1240 | PlayerIntentAndInteractionState | field Terraria.DataStructures.PlayerIntentionGuesser.LastMouse : Vector2; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerIntentionGuesser.cs:16 | proposed Component/System: PlayerIntentionState and IntentionTrackingSystem | confirmed-declaration; intention tracking partial |
| 1241 | PlayerIntentAndInteractionState | field Terraria.DataStructures.PlayerIntentionGuesser.LastDirection : int; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerIntentionGuesser.cs:18 | proposed Component/System: PlayerIntentionState and IntentionTrackingSystem | confirmed-declaration; intention tracking partial |
| 1242 | PlayerIntentAndInteractionState | field Terraria.DataStructures.PlayerIntentionGuesser.LastWidth : int; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerIntentionGuesser.cs:20 | proposed Component/System: PlayerIntentionState and IntentionTrackingSystem | confirmed-declaration; intention tracking partial |
| 1243 | PlayerIntentAndInteractionState | field Terraria.DataStructures.PlayerIntentionGuesser.Intention : Terraria.DataStructures.GuessedPlayerIntention; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerIntentionGuesser.cs:22 | proposed Component/System: PlayerIntentionState and IntentionTrackingSystem | confirmed-declaration; intention tracking partial |
| 1244 | PlayerIntentAndInteractionState | field Terraria.DataStructures.PlayerIntentionGuesser.UsageProxy : Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerIntentionGuesser.cs:24 | proposed Component/System: PlayerIntentionState and IntentionTrackingSystem | confirmed-declaration; intention tracking partial |
| 1245 | PlayerIntentAndInteractionState | field Terraria.DataStructures.PlayerIntentionGuesser.TimeWithIntention : int; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerIntentionGuesser.cs:26 | proposed Component/System: PlayerIntentionState and IntentionTrackingSystem | confirmed-declaration; intention tracking partial |
| 1246 | PlayerIntentAndInteractionState | field Terraria.DataStructures.PlayerIntentionGuesser.PlayerActiveActionTimeLeft : int; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerIntentionGuesser.cs:28 | proposed Component/System: PlayerIntentionState and IntentionTrackingSystem | confirmed-declaration; intention tracking partial |
| 1247 | PlayerIntentAndInteractionState | field Terraria.DataStructures.PlayerInteractionAnchor.interactEntityID : int; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerInteractionAnchor.cs:5 | proposed Component: PlayerInteractionAnchor | confirmed-declaration; intention tracking partial |
| 1248 | PlayerIntentAndInteractionState | field Terraria.DataStructures.PlayerInteractionAnchor.X : int; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerInteractionAnchor.cs:7 | proposed Component: PlayerInteractionAnchor | confirmed-declaration; intention tracking partial |
| 1249 | PlayerIntentAndInteractionState | field Terraria.DataStructures.PlayerInteractionAnchor.Y : int; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerInteractionAnchor.cs:9 | proposed Component: PlayerInteractionAnchor | confirmed-declaration; intention tracking partial |
| 3699 | PlayerIntentAndInteractionState | property Terraria.DataStructures.PlayerInteractionAnchor.InUse : bool; D:\\TRbackup\\Version4\\Terraria.DataStructures\\PlayerInteractionAnchor.cs:11 | proposed Query: PlayerInteractionAnchorUsage | confirmed-declaration; intention tracking partial |
| 1471 | SharedCreativePerPlayerPowerState | field Terraria.GameContent.Creative.CreativePowers.APerPlayerTogglePower._powerNameKey : string; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:22 | proposed Component: PerPlayerCreativePowerState | confirmed-declaration; permission/network/persistence partial |
| 1472 | SharedCreativePerPlayerPowerState | field Terraria.GameContent.Creative.CreativePowers.APerPlayerTogglePower._iconLocation : Point; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:24 | proposed Component: PerPlayerCreativePowerState | confirmed-declaration; permission/network/persistence partial |
| 1473 | SharedCreativePerPlayerPowerState | field Terraria.GameContent.Creative.CreativePowers.APerPlayerTogglePower._defaultToggleState : bool; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:26 | proposed Component: PerPlayerCreativePowerState | confirmed-declaration; permission/network/persistence partial |
| 1474 | SharedCreativePerPlayerPowerState | field Terraria.GameContent.Creative.CreativePowers.APerPlayerTogglePower._perPlayerIsEnabled : bool[]; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:28 | proposed Component: PerPlayerCreativePowerState | confirmed-declaration; permission/network/persistence partial |
| 1475 | SharedCreativePerPlayerPowerState | field Terraria.GameContent.Creative.CreativePowers.APerPlayerSliderPower._iconLocation : Point; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:90 | proposed Component: PerPlayerCreativePowerState | confirmed-declaration; permission/network/persistence partial |
| 1476 | SharedCreativePerPlayerPowerState | field Terraria.GameContent.Creative.CreativePowers.APerPlayerSliderPower._sliderCurrentValueCache : float; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:92 | proposed Component: PerPlayerCreativePowerState | confirmed-declaration; permission/network/persistence partial |
| 1477 | SharedCreativePerPlayerPowerState | field Terraria.GameContent.Creative.CreativePowers.APerPlayerSliderPower._powerNameKey : string; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:94 | proposed Component: PerPlayerCreativePowerState | confirmed-declaration; permission/network/persistence partial |
| 1478 | SharedCreativePerPlayerPowerState | field Terraria.GameContent.Creative.CreativePowers.APerPlayerSliderPower._cachePerPlayer : float[]; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:96 | proposed Component: PerPlayerCreativePowerState | confirmed-declaration; permission/network/persistence partial |
| 1479 | SharedCreativePerPlayerPowerState | field Terraria.GameContent.Creative.CreativePowers.APerPlayerSliderPower._sliderDefaultValue : float; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:98 | proposed Component: PerPlayerCreativePowerState | confirmed-declaration; permission/network/persistence partial |
| 1480 | SharedCreativePerPlayerPowerState | field Terraria.GameContent.Creative.CreativePowers.APerPlayerSliderPower._currentTargetValue : float; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:100 | proposed Component: PerPlayerCreativePowerState | confirmed-declaration; permission/network/persistence partial |
| 1481 | SharedCreativePerPlayerPowerState | field Terraria.GameContent.Creative.CreativePowers.APerPlayerSliderPower._nextTimeWeCanPush : System.DateTime; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:102 | proposed Component: PerPlayerCreativePowerState | confirmed-declaration; permission/network/persistence partial |
| 3748 | SharedCreativePerPlayerPowerState | property Terraria.GameContent.Creative.CreativePowers.APerPlayerTogglePower.PowerId : ushort; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:30 | proposed Contract view: CreativePowerMetadata | confirmed-declaration; permission/network/persistence partial |
| 3749 | SharedCreativePerPlayerPowerState | property Terraria.GameContent.Creative.CreativePowers.APerPlayerTogglePower.ServerConfigName : string; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:32 | proposed Contract view: CreativePowerMetadata | confirmed-declaration; permission/network/persistence partial |
| 3750 | SharedCreativePerPlayerPowerState | property Terraria.GameContent.Creative.CreativePowers.APerPlayerTogglePower.CurrentPermissionLevel : Terraria.GameContent.Creative.PowerPermissionLevel; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:34 | proposed Contract view: CreativePowerMetadata | confirmed-declaration; permission/network/persistence partial |
| 3751 | SharedCreativePerPlayerPowerState | property Terraria.GameContent.Creative.CreativePowers.APerPlayerTogglePower.DefaultPermissionLevel : Terraria.GameContent.Creative.PowerPermissionLevel; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:36 | proposed Contract view: CreativePowerMetadata | confirmed-declaration; permission/network/persistence partial |
| 3752 | SharedCreativePerPlayerPowerState | property Terraria.GameContent.Creative.CreativePowers.APerPlayerSliderPower.PowerId : ushort; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:104 | proposed Contract view: CreativePowerMetadata | confirmed-declaration; permission/network/persistence partial |
| 3753 | SharedCreativePerPlayerPowerState | property Terraria.GameContent.Creative.CreativePowers.APerPlayerSliderPower.ServerConfigName : string; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:106 | proposed Contract view: CreativePowerMetadata | confirmed-declaration; permission/network/persistence partial |
| 3754 | SharedCreativePerPlayerPowerState | property Terraria.GameContent.Creative.CreativePowers.APerPlayerSliderPower.CurrentPermissionLevel : Terraria.GameContent.Creative.PowerPermissionLevel; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:108 | proposed Contract view: CreativePowerMetadata | confirmed-declaration; permission/network/persistence partial |
| 3755 | SharedCreativePerPlayerPowerState | property Terraria.GameContent.Creative.CreativePowers.APerPlayerSliderPower.DefaultPermissionLevel : Terraria.GameContent.Creative.PowerPermissionLevel; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:110 | proposed Contract view: CreativePowerMetadata | confirmed-declaration; permission/network/persistence partial |
| 3771 | SharedCreativePerPlayerPowerState | property Terraria.GameContent.Creative.CreativePowers.SpawnRateSliderPerPlayerPower.StrengthMultiplierToGiveNPCs : float; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:700 | proposed Query: CreativeNpcStrengthMultiplier | confirmed-declaration; permission/network/persistence partial |
| 1482 | SharedCreativeSharedPowerState | field Terraria.GameContent.Creative.CreativePowers.ASharedButtonPower._iconLocation : Point; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:188 | proposed Component: SharedCreativePowerState | confirmed-declaration; permission/network/persistence partial |
| 1483 | SharedCreativeSharedPowerState | field Terraria.GameContent.Creative.CreativePowers.ASharedButtonPower._powerNameKey : string; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:190 | proposed Component: SharedCreativePowerState | confirmed-declaration; permission/network/persistence partial |
| 1484 | SharedCreativeSharedPowerState | field Terraria.GameContent.Creative.CreativePowers.ASharedButtonPower._descriptionKey : string; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:192 | proposed Component: SharedCreativePowerState | confirmed-declaration; permission/network/persistence partial |
| 1485 | SharedCreativeSharedPowerState | field Terraria.GameContent.Creative.CreativePowers.ASharedSliderPower._iconLocation : Point; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:261 | proposed Component: SharedCreativePowerState | confirmed-declaration; permission/network/persistence partial |
| 1486 | SharedCreativeSharedPowerState | field Terraria.GameContent.Creative.CreativePowers.ASharedSliderPower._sliderCurrentValueCache : float; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:263 | proposed Component: SharedCreativePowerState | confirmed-declaration; permission/network/persistence partial |
| 1487 | SharedCreativeSharedPowerState | field Terraria.GameContent.Creative.CreativePowers.ASharedSliderPower._powerNameKey : string; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:265 | proposed Component: SharedCreativePowerState | confirmed-declaration; permission/network/persistence partial |
| 1488 | SharedCreativeSharedPowerState | field Terraria.GameContent.Creative.CreativePowers.ASharedSliderPower._syncToJoiningPlayers : bool; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:267 | proposed Component: SharedCreativePowerState | confirmed-declaration; permission/network/persistence partial |
| 1489 | SharedCreativeSharedPowerState | field Terraria.GameContent.Creative.CreativePowers.ASharedSliderPower._currentTargetValue : float; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:269 | proposed Component: SharedCreativePowerState | confirmed-declaration; permission/network/persistence partial |
| 1490 | SharedCreativeSharedPowerState | field Terraria.GameContent.Creative.CreativePowers.ASharedSliderPower._nextTimeWeCanPush : System.DateTime; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:271 | proposed Component: SharedCreativePowerState | confirmed-declaration; permission/network/persistence partial |
| 3756 | SharedCreativeSharedPowerState | property Terraria.GameContent.Creative.CreativePowers.ASharedButtonPower.PowerId : ushort; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:194 | proposed Contract view: CreativePowerMetadata | confirmed-declaration; permission/network/persistence partial |
| 3757 | SharedCreativeSharedPowerState | property Terraria.GameContent.Creative.CreativePowers.ASharedButtonPower.ServerConfigName : string; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:196 | proposed Contract view: CreativePowerMetadata | confirmed-declaration; permission/network/persistence partial |
| 3758 | SharedCreativeSharedPowerState | property Terraria.GameContent.Creative.CreativePowers.ASharedButtonPower.CurrentPermissionLevel : Terraria.GameContent.Creative.PowerPermissionLevel; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:198 | proposed Contract view: CreativePowerMetadata | confirmed-declaration; permission/network/persistence partial |
| 3759 | SharedCreativeSharedPowerState | property Terraria.GameContent.Creative.CreativePowers.ASharedButtonPower.DefaultPermissionLevel : Terraria.GameContent.Creative.PowerPermissionLevel; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:200 | proposed Contract view: CreativePowerMetadata | confirmed-declaration; permission/network/persistence partial |
| 3760 | SharedCreativeSharedPowerState | property Terraria.GameContent.Creative.CreativePowers.ASharedTogglePower.PowerId : ushort; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:217 | proposed Contract view: CreativePowerMetadata | confirmed-declaration; permission/network/persistence partial |
| 3761 | SharedCreativeSharedPowerState | property Terraria.GameContent.Creative.CreativePowers.ASharedTogglePower.ServerConfigName : string; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:219 | proposed Contract view: CreativePowerMetadata | confirmed-declaration; permission/network/persistence partial |
| 3762 | SharedCreativeSharedPowerState | property Terraria.GameContent.Creative.CreativePowers.ASharedTogglePower.CurrentPermissionLevel : Terraria.GameContent.Creative.PowerPermissionLevel; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:221 | proposed Contract view: CreativePowerMetadata | confirmed-declaration; permission/network/persistence partial |
| 3763 | SharedCreativeSharedPowerState | property Terraria.GameContent.Creative.CreativePowers.ASharedTogglePower.DefaultPermissionLevel : Terraria.GameContent.Creative.PowerPermissionLevel; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:223 | proposed Contract view: CreativePowerMetadata | confirmed-declaration; permission/network/persistence partial |
| 3764 | SharedCreativeSharedPowerState | property Terraria.GameContent.Creative.CreativePowers.ASharedTogglePower.Enabled : bool; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:225 | proposed Query/Component: SharedCreativePowerDerivedState | confirmed-declaration; permission/network/persistence partial |
| 3765 | SharedCreativeSharedPowerState | property Terraria.GameContent.Creative.CreativePowers.ASharedSliderPower.PowerId : ushort; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:273 | proposed Contract view: CreativePowerMetadata | confirmed-declaration; permission/network/persistence partial |
| 3766 | SharedCreativeSharedPowerState | property Terraria.GameContent.Creative.CreativePowers.ASharedSliderPower.ServerConfigName : string; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:275 | proposed Contract view: CreativePowerMetadata | confirmed-declaration; permission/network/persistence partial |
| 3767 | SharedCreativeSharedPowerState | property Terraria.GameContent.Creative.CreativePowers.ASharedSliderPower.CurrentPermissionLevel : Terraria.GameContent.Creative.PowerPermissionLevel; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:277 | proposed Contract view: CreativePowerMetadata | confirmed-declaration; permission/network/persistence partial |
| 3768 | SharedCreativeSharedPowerState | property Terraria.GameContent.Creative.CreativePowers.ASharedSliderPower.DefaultPermissionLevel : Terraria.GameContent.Creative.PowerPermissionLevel; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:279 | proposed Contract view: CreativePowerMetadata | confirmed-declaration; permission/network/persistence partial |
| 3769 | SharedCreativeSharedPowerState | property Terraria.GameContent.Creative.CreativePowers.ModifyTimeRate.TargetTimeRate : int; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:408 | proposed Query/Component: SharedCreativePowerDerivedState | confirmed-declaration; permission/network/persistence partial |
| 3770 | SharedCreativeSharedPowerState | property Terraria.GameContent.Creative.CreativePowers.DifficultySliderPower.StrengthMultiplierToGiveNPCs : float; D:\\TRbackup\\Version4\\Terraria.GameContent.Creative\\CreativePowers.cs:463 | proposed Query/Component: SharedCreativePowerDerivedState | confirmed-declaration; permission/network/persistence partial |

## 4. 实施顺序、单一写 owner 和兼容策略

### Phase 0: freeze contracts and evidence
Record the Version4 source SHA, confirm the 283-row inventory, resolve the missing first-round report and path-drift notes, and obtain integration-review decisions for stable entity/network/persistence IDs. No C# file is created before this gate.

### Phase 1: pure values, definitions and adapters
Create only proposed immutable values and ports: raw input frame, viewport/cursor queries, input profiles, trigger frame values, golf rules, creative contract, sacrifice catalog, chest snapshot, preview settings and movement restore snapshot. Add pure tests for reset, derived values and bounds.

### Phase 2: local components and single write owners
Add local components and systems in this order: input edge/frame state; player spawn/anchor/intention; equipment loadout; movement capability/stool; golf local tracking; creative registry/unlock/per-player/shared state; smart interaction/cursor buffers; door/pressure occupancy; pickup/respawn diagnostics; preview/rejection projection state.

### Phase 3: compatibility adapters and shadow comparison
Attach one compatibility adapter per legacy boundary. Read old Main/Player/Helper objects into proposed values, compare derived outputs and transition decisions, and keep only one side advancing each state. Do not let old and new systems both decrement timers, append logs, swap loadouts or update occupancy.

### Phase 4: command integration and projections
Connect explicit commands to Player/World/Tile/Item owners. Add one-way network, save, UI, camera and platform projections only after simulation ownership and versioned schemas are approved. Keep network IDs, persistence IDs, content IDs and entity references distinct.

### Phase 5: focused verification and serial build
The affected production and verifier projects were built from the repository root through `Build/Tools/Invoke-SerialDotnet.ps1` with `-m:1`, `-nr:false`, `-p:UseSharedCompilation=false`, `-p:MSBuildNodeReuse=false` and `-p:BuildInParallel=false`. Artifacts were confirmed under `Build/bin/`. The no-build verifier was then run through the same wrapper and stopped at the existing EquipmentLoadout assertion before the Creative scenarios.

| state/side effect | single write owner | compatibility rule |
|---|---|---|
| raw input, trigger edges, viewport/cursor query | InputFrame/Trigger/Viewport systems; CursorItemPreview owner for cursor item | legacy Main/PlayerInput is read through adapter; only one frame buffer rolls over. |
| Main-thread actions | MainThreadActionQueue adapter/system | actions execute once on main thread; no secondary queue or background mutation. |
| player registry/spawn/anchor/intention | integration-approved Player registry and P08 components | legacy arrays/objects are compatibility inputs; stable entity mapping is not invented locally. |
| equipment loadout | EquipmentLoadoutSwapSystem | old and new loadouts cannot both swap; shadow compare before cutover; drops use an explicit effect port. |
| movement capability/mount restore/stool | MovementCapabilitySystem and separate restore/stool owners | mount enter/exit snapshot is single-use; no generic cache double-write. |
| smart interaction/cursor/doors/pressure | respective query buffer and validated interaction command owners | queries never mutate world; old helpers are one-way adapters until behavior is proven. |
| Creative registry/power/unlock | registry, per-player/shared state and unlock systems | ID/name/permission and saved progress are versioned; no dictionary/array double owner. |
| item pickup/minion respawn diagnostics | dedicated pickup log/respawn systems | logger remains opt-in diagnostic; no inventory mutation through log replay. |
| UI/camera/network/save/platform | one-way Projection/Adapter | output cannot write simulation; schema and external IDs require integration-review. |

## 5. 网络、快照、存档和 UI 计划

- raw input and trigger state are client/local frame state unless a separate input command protocol is approved. Do not serialize legacy object references.
- Player spawn/session, pressure occupancy, door toggles, equipment changes and Creative shared state require authoritative command/result schemas. Network IDs are transport fields, not EntityReference or persistence IDs.
- Creative sacrifice counts and power values require versioned persistence records. UI metadata, unlock notifications, FocusHelper flags, Golf camera state, SmartCursor target lists, cursor previews and character-preview delegates are local/projection state by default.
- Snapshot/restore distinguishes durable, session, frame, compatibility and presentation state. A failed projection/network send does not roll back a committed component unless its owner explicitly defines that transaction.

## 6. 回滚、失败和风险

- Roll back an adapter cutover if input edges, MouseWorld gravity inversion, Main queue execution, loadout swap/drop, mount restore, anchor clearing, provider priority, occupancy transitions, Creative permission, sacrifice count or Golf settle decisions differ from focused Version4 evidence.
- On command rejection, return a typed failure and leave authority unchanged. On retry, use a version/sequence or idempotency rule approved by integration-review; never re-append pickup logs or re-apply unlocks blindly.
- Keep the legacy path connected until focused verifiers pass and shadow comparisons are stable. Rollback disconnects the new adapter/system and restores the old call path; it does not delete source or persisted data.
- Main risks are missing Version4 behavior, hidden direct writers in Player/Main, old array slot reuse, mutable external candidate objects, DateTime/random side effects and cross-partition transaction ordering.

## 7. Focused verifier 与构建结果

| verifier | planned checks | status |
|---|---|---|
| P08-MAIN-INPUT-FRAME | queue single-consumption, pointer edge/reset, viewport/cursor values, Main flag lifecycle | not-run |
| P08-INPUT-EDGE-PROFILE | profile defaults/deadzone/inversion, trigger rollover, runtime screen size | not-run |
| P08-INTENT-INTERACTION | intention state, anchor set/clear/get, candidate-to-command boundary, ID separation | not-run |
| P08-SMART-INTERACTION | provider priority, block/candidate reuse, SmartCursor bounds/paint, no query writes | not-run |
| P08-MOVEMENT-EQUIPMENT | mount snapshot restore, stool reset/stats, loadout atomic swap/drop port | not-run |
| P08-CREATIVE | registry uniqueness, permission contract, per-player/shared separation, sacrifice persistence/revision, sync | blocked before execution by existing EquipmentLoadout assertion; not verified |
| P08-GOLF-FOCUS | local golf ownership/swing matching, settle timers, focus/pause/rain/time queries | not-run |
| P08-LOCAL-DIAGNOSTICS | pickup logger, minion type/prefix match, preview/rejection, cursor/chest snapshots | not-run |

Actual compile-capable commands:

`& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\src2\PlayerInputGameplay\Terraria.PlayerInputGameplay.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')` exited `0` with 0 warnings and 0 errors.

`& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\src2\PlayerInputGameplayVerification\Terraria.PlayerInputGameplayVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')` exited `0` with 0 warnings and 0 errors.

The verifier command `& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '--project', '.\src2\PlayerInputGameplayVerification\Terraria.PlayerInputGameplayVerification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')` exited `-532462766` at `PlayerInputGameplayVerification/Program.cs:172` with `Loadout swap should preserve both armor values.`

## 8. 明确未执行与交接

- 已在 src2 保存 P08 的 26 个组件单元及项目文件；两个受影响项目均已通过串行 build，且产物已确认位于 `Build/bin/`。现有 verifier 已通过串行 no-build/no-restore 命令启动，但在 EquipmentLoadout 非 Component 断言处退出，未到达 Creative 断言。
- 未运行 git diff --check；二轮 prompt 禁止对这两份二轮文档执行该检查。
- 后续实施必须在仓库根目录读取 AGENTS.md、Context/progress.md、相关 flowstate context 和 ECS 约束，检查 BUILD-CONCURRENCY-1，完成 evidence gap 和 integration-review 后再修改代码。
- Handoff：先审阅逐成员表和唯一写 owner，再批准目标文件；未取得 stable ID、网络/存档 schema、事务边界和调度决策前，不得声称 P08 组件迁移完成。

## 9. 结论

本文件同时记录 planned execution 和当前实现进度。P08 283 条成员已全部列入映射，26 个叶子单元的实现已保存到 src2，两个受影响项目构建成功；现有 verifier 在 Equipment 非 Component 断言处失败，Creative 专项和行为等价仍未验证。implementationStatus 保持 in-progress，verificationStatus 为 failed。
