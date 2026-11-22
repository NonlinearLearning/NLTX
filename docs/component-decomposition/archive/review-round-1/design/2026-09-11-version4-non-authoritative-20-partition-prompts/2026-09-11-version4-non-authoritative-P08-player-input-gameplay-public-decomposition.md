# Version4 非权威组件拆分分区 P08：玩家输入与玩法 - public-decomposition 专属提示词

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
- 必须先按 skill 使用 ClaimNext（或用户明确指定时使用 Claim -PartitionId P08），只写本文件的 outputReport，并用领取返回的同一 sessionId 完成 Complete/Fail/Abandon；不得手工编辑 ledger 或 lock。
- 本任务属于 20 个并行会话中的固定分区；currentTaskCount: 20。公共协议中遗留的 19 会话数量说明在本任务包中不适用。

## 任务元数据

- promptId: P08-non-authoritative-public-decomposition-20260911
- partitionId: P08
- inputPartitionReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\08-player-input-gameplay.md
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P08-player-input-gameplay-public-decomposition.md
- reportType: public-decomposition
- executionMode: independent-read-only-partition
- currentTaskCount: 20
- formalParentSubsystems: RuntimeComposition, SharedRuntimeMechanisms
- leafSubsystemCount: 26
- fieldCount: 232
- propertyCount: 51
- memberCount: 283
- sourceReportSha256: b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196
- sourceInventoryStatus: source-inventory-confirmed; member-level owner/lifecycle/side-effect evidence still requires this session to close

## 专属范围锁定

- 本会话只审查输入分区报告中的 26 个叶子子系统和 283 条成员记录。以下清单是本会话唯一的成员范围；不得读取或复制其他 P 分区的成员清单。
- 细分子系统仍只是库存边界，不等价于一个组件；允许在本分区内部按访问模式拆成多个 proposed Component、System、Query、Command、Adapter 或 Projection。
- 来源序号、路径、行号和声明是检索线索；必须回到 Version4 重新定位，不得把输入报告直接当作已确认的读者、写者、生命周期或 owner 证据。

| 叶子子系统 | 父级 | 原章节 | 边界角色 | 字段 | 属性 | 成员 | 细分职责 |
|---|---|---|---|---:|---:|---:|---|
| `MainInputAndThreadScheduling` | `RuntimeComposition` | `4.1.8` | `runtime state` | 11 | 0 | 11 | 输入坐标、主线程动作队列和帧级交互计时。 |
| `MainScreenAndInputState` | `RuntimeComposition` | `4.1.26` | `presentation state` | 10 | 0 | 10 | 屏幕尺寸、输入接管、鼠标物品和 UI 颜色。 |
| `MainPlayerAndSpawnState` | `RuntimeComposition` | `4.1.27` | `runtime state` | 5 | 0 | 5 | 本地玩家、玩家池、出生点和玩法归属槽。 |
| `MainMenuAndInputSettings` | `RuntimeComposition` | `4.1.34` | `presentation state` | 5 | 0 | 5 | 菜单快捷键、智能游标和运行时资源设置。 |
| `MainInputAndEventFlags` | `RuntimeComposition` | `4.1.41` | `runtime state` | 7 | 0 | 7 | 鼠标、绘制、陨石和环境伤害开关。 |
| `MainDerivedInputAndPresentationQueries` | `RuntimeComposition` | `4.1.46` | `derived/query` | 0 | 10 | 10 | UI 缩放、鼠标、天气表现和诊断投影查询。 |
| `SharedGolfState` | `SharedRuntimeMechanisms` | `4.9.10` | `state/definition` | 16 | 0 | 16 | 高尔夫状态、轨迹和规则辅助。 |
| `SharedControlFocusHelpers` | `SharedRuntimeMechanisms` | `4.9.42` | `query/adapter` | 1 | 4 | 5 | 焦点和锁定控制辅助。 |
| `SharedCreativePowerRuntimeManager` | `SharedRuntimeMechanisms` | `4.9.50` | `state` | 9 | 0 | 9 | 创意能力注册、按玩家存储和运行时管理。 |
| `SharedCreativeUnlockProgress` | `SharedRuntimeMechanisms` | `4.9.51` | `state/query` | 9 | 1 | 10 | 牺牲目录和创意解锁进度。 |
| `SharedSmartInteractionQueries` | `SharedRuntimeMechanisms` | `4.9.53` | `query` | 16 | 5 | 21 | 智能交互候选和扫描查询。 |
| `PlayerItemPickupAndRespawnState` | `SharedRuntimeMechanisms` | `4.9.78` | `state` | 9 | 0 | 9 | 玩家物品领取日志、召唤物生成和重生状态。 |
| `PlayerPreviewAndRejectionState` | `SharedRuntimeMechanisms` | `4.9.79` | `presentation/state` | 11 | 0 | 11 | 角色预览设置和拒绝菜单载荷。 |
| `DoorOpeningInteractionState` | `SharedRuntimeMechanisms` | `4.9.95` | `state/query` | 10 | 0 | 10 | 门开启/关闭候选、玩家信息和切换状态。 |
| `SmartCursorInteractionState` | `SharedRuntimeMechanisms` | `4.9.96` | `query/state` | 19 | 0 | 19 | 智能游标目标、抓钩目标和使用信息。 |
| `PressurePlateInteractionState` | `SharedRuntimeMechanisms` | `4.9.97` | `state/query` | 5 | 0 | 5 | 压力板检测锁和被按压集合状态。 |
| `CursorAndChestInteractionState` | `SharedRuntimeMechanisms` | `4.9.98` | `state/query` | 6 | 1 | 7 | 虚拟游标物品和定位 Chest 交互状态。 |
| `InputProfilesAndConfiguration` | `SharedRuntimeMechanisms` | `4.9.104` | `adapter/state` | 16 | 0 | 16 | 玩家输入档案和按键配置。 |
| `InputTriggerState` | `SharedRuntimeMechanisms` | `4.9.105` | `adapter/state` | 9 | 0 | 9 | 触发器集合、当前输入模式和触发器打包状态。 |
| `PlayerInputRuntimeState` | `SharedRuntimeMechanisms` | `4.9.106` | `adapter/state` | 3 | 1 | 4 | 玩家输入运行时屏幕、按键和输入接管状态。 |
| `EquipmentLoadoutState` | `SharedRuntimeMechanisms` | `4.9.112` | `state` | 3 | 0 | 3 | 装备栏和染料栏位状态。 |
| `SharedCreativePowerContracts` | `SharedRuntimeMechanisms` | `4.9.122` | `definition/adapter` | 0 | 4 | 4 | 创意能力公开接口、权限和服务器配置契约。 |
| `PlayerMovementCapabilityState` | `SharedRuntimeMechanisms` | `4.9.142` | `state/query` | 18 | 0 | 18 | 飞行、翅膀、跳跃和便携座椅运动能力状态。 |
| `PlayerIntentAndInteractionState` | `SharedRuntimeMechanisms` | `4.9.143` | `state/query` | 14 | 1 | 15 | 玩家意图推断和交互锚点状态。 |
| `SharedCreativePerPlayerPowerState` | `SharedRuntimeMechanisms` | `4.9.172` | `definition/state` | 11 | 9 | 20 | 按玩家创意能力的参数、滑杆和目标值状态。 |
| `SharedCreativeSharedPowerState` | `SharedRuntimeMechanisms` | `4.9.173` | `definition/state` | 9 | 15 | 24 | 共享创意能力的权限、开关和全局目标值状态。 |

来源成员的分区内序号线索范围：69..3960；序号不定义领域边界，必须逐条回到输入表和 Version4 覆盖。

## 分区专属目标

围绕“玩家输入与玩法”完成独立只读的 Version4 成员审查和 ECS 拆分设计。
- 围绕 Main 输入/线程调度/屏幕状态、本地玩家与出生点、输入档案/触发器、智能交互、门/压力板/箱子、移动能力、装备槽和 Creative Power，核对原始输入、意图、权限、玩法状态和 UI 投影。
- 回到 Terraria/Main.cs、Player/输入配置/交互相关源码、Creative Power 管理器与直接调用者，确认客户端输入读取、服务器验证、Command 生成、玩家状态写入和清理。
- 把 raw input、intent/query、交互 Command、玩家能力 Component、装备状态、Creative per-player/shared state、输入配置 Adapter 和 UI Projection 分开。
- 核对移动、交互、出生/重生、拾取、创意能力、智能游标和焦点之间的依赖方向；记录多玩家、服务端权威和重复输入/重试行为。

## 专属证据焦点

- 先完整读取本分区输入报告的成员表，再回到 D:\TRbackup\Version4 重新核对字段、属性、声明类型、初始化/注册、直接读者、直接写者、清理路径和副作用。
- 下列路径只作为本分区的检索种子；路径存在不等于成员语义已确认，必须按公共证据协议记录实际行号、符号、调用者、读者、写者、生命周期和 evidenceStatus。
  - `D:\TRbackup\Version4\Terraria.DataStructures/MinionRespawner.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/MinionSpawnFromInventoryItem.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/PlayerGetItemLogger.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/PlayerIntentionGuesser.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/PlayerInteractionAnchor.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/PlayerMovementAccsCache.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/PortableStoolUsage.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/RejectionMenuInfo.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/SettingsForCharacterPreview.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Creative/CreativeItemSacrificesCatalog.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Creative/CreativePowerManager.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Creative/CreativePowers.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Creative/CreativeUnlocksTracker.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Creative/ICreativePower.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Creative/ItemsSacrificedUnlocksTracker.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Golf/GolfBallTrackRecord.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Golf/GolfHelper.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Golf/GolfState.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions/ISmartInteractCandidate.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions/NPCSmartInteractCandidateProvider.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions/PotionOfReturnSmartInteractCandidateProvider.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions/ProjectileSmartInteractCandidateProvider.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions/SmartInteractScanSettings.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions/SmartInteractSystem.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions/TileSmartInteractCandidateProvider.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/DoorOpeningHelper.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/FakeCursorItem.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/PositionedChest.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/PressurePlateHelper.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/SmartCursorHelper.cs`
  - `D:\TRbackup\Version4\Terraria.GameInput/KeyConfiguration.cs`
  - `D:\TRbackup\Version4\Terraria.GameInput/PlayerInput.cs`
  - `D:\TRbackup\Version4\Terraria.GameInput/PlayerInputProfile.cs`
  - `D:\TRbackup\Version4\Terraria.GameInput/TriggersPack.cs`
  - `D:\TRbackup\Version4\Terraria.GameInput/TriggersSet.cs`
  - `D:\TRbackup\Version4\Terraria/EquipmentLoadout.cs`
  - `D:\TRbackup\Version4\Terraria/FocusHelper.cs`
  - `D:\TRbackup\Version4\Terraria/Main.cs`

- 输入分区抽取出的声明类型焦点：
  - `Terraria.DataStructures.MinionRespawner`
  - `Terraria.DataStructures.MinionSpawnFromInventoryItem`
  - `Terraria.DataStructures.PlayerGetItemLogger`
  - `Terraria.DataStructures.PlayerGetItemLogger.GetItemLoggerEntry`
  - `Terraria.DataStructures.PlayerIntentionGuesser`
  - `Terraria.DataStructures.PlayerInteractionAnchor`
  - `Terraria.DataStructures.PlayerMovementAccsCache`
  - `Terraria.DataStructures.PortableStoolUsage`
  - `Terraria.DataStructures.RejectionMenuInfo`
  - `Terraria.DataStructures.SettingsForCharacterPreview`
  - `Terraria.DataStructures.SettingsForCharacterPreview.SelectionBasedSettings`
  - `Terraria.EquipmentLoadout`
  - `Terraria.FocusHelper`
  - `Terraria.GameContent.Creative.CreativeItemSacrificesCatalog`
  - `Terraria.GameContent.Creative.CreativePowerManager`
  - `Terraria.GameContent.Creative.CreativePowerManager.PowerTypeStorage<T>`
  - `Terraria.GameContent.Creative.CreativePowers.APerPlayerSliderPower`
  - `Terraria.GameContent.Creative.CreativePowers.APerPlayerTogglePower`
  - `Terraria.GameContent.Creative.CreativePowers.ASharedButtonPower`
  - `Terraria.GameContent.Creative.CreativePowers.ASharedSliderPower`
  - `Terraria.GameContent.Creative.CreativePowers.ASharedTogglePower`
  - `Terraria.GameContent.Creative.CreativePowers.DifficultySliderPower`
  - `Terraria.GameContent.Creative.CreativePowers.ModifyTimeRate`
  - `Terraria.GameContent.Creative.CreativePowers.SpawnRateSliderPerPlayerPower`
  - `Terraria.GameContent.Creative.CreativeUnlocksTracker`
  - `Terraria.GameContent.Creative.ICreativePower`
  - `Terraria.GameContent.Creative.ItemsSacrificedUnlocksTracker`
  - `Terraria.GameContent.DoorOpeningHelper`
  - `Terraria.GameContent.DoorOpeningHelper.DoorOpenCloseTogglingInfo`
  - `Terraria.GameContent.DoorOpeningHelper.PlayerInfoForClosingDoors`
  - `Terraria.GameContent.DoorOpeningHelper.PlayerInfoForOpeningDoors`
  - `Terraria.GameContent.FakeCursorItem`
  - `Terraria.GameContent.Golf.GolfBallTrackRecord`
  - `Terraria.GameContent.Golf.GolfHelper`
  - `Terraria.GameContent.Golf.GolfState`
  - `Terraria.GameContent.ObjectInteractions.ISmartInteractCandidate`
  - `Terraria.GameContent.ObjectInteractions.NPCSmartInteractCandidateProvider`
  - `Terraria.GameContent.ObjectInteractions.NPCSmartInteractCandidateProvider.ReusableCandidate`
  - `Terraria.GameContent.ObjectInteractions.PotionOfReturnSmartInteractCandidateProvider`
  - `Terraria.GameContent.ObjectInteractions.PotionOfReturnSmartInteractCandidateProvider.ReusableCandidate`
  - `Terraria.GameContent.ObjectInteractions.ProjectileSmartInteractCandidateProvider`
  - `Terraria.GameContent.ObjectInteractions.ProjectileSmartInteractCandidateProvider.ReusableCandidate`
  - `Terraria.GameContent.ObjectInteractions.SmartInteractScanSettings`
  - `Terraria.GameContent.ObjectInteractions.SmartInteractSystem`
  - `Terraria.GameContent.ObjectInteractions.TileSmartInteractCandidateProvider`
  - `Terraria.GameContent.ObjectInteractions.TileSmartInteractCandidateProvider.ReusableCandidate`
  - `Terraria.GameContent.PositionedChest`
  - `Terraria.GameContent.PressurePlateHelper`
  - `Terraria.GameContent.SmartCursorHelper`
  - `Terraria.GameContent.SmartCursorHelper.SmartCursorUsageInfo`
  - `Terraria.GameInput.KeyConfiguration`
  - `Terraria.GameInput.PlayerInput`
  - `Terraria.GameInput.PlayerInputProfile`
  - `Terraria.GameInput.TriggersPack`
  - `Terraria.GameInput.TriggersSet`
  - `Terraria.Main`

- 若路径、类型或行号漂移，记录 version-drift 或 evidence-mismatch；不得静默沿用库存报告行号。Version4 是 Terraria 私有实现的事实来源，完整源码只补证 Version4 已存在文件，tModLoader 只交叉验证公开边界，SS14 只参考结构粒度。

## 专属审查问题

- 原始按键/鼠标、触发器、玩家意图、智能交互候选和实际玩法 Command 的边界在哪里？哪些状态只在客户端存在？
- 玩家移动能力、装备栏、出生点、重生、拾取和本地玩家引用是否有不同生命周期与写者？
- Creative Power 的公开契约、权限、按玩家参数、共享开关和 UI 滑杆如何拆分，服务端是否是唯一权威？
- 门、压力板、箱子、智能游标和玩家输入与 Tile、物品、网络、UI 的交接顺序是什么？

## 专属不拆分边界

- 不要把原始输入、玩家意图、玩家能力、装备、交互候选和 UI 状态合为 PlayerGameplayComponent。
- 不要把鼠标物品、触发器打包、智能游标候选、焦点缓存或一次交互载荷当作权威玩家状态。
- 不要把 Creative Power 契约/权限与客户端 UI 滑杆或输入配置合并；外部输入和 UI 通过 Adapter/Command 进入模拟。

专属跨域提醒：重点记录与实体生命周期、物品容器、Tile 交互、战斗、网络协议、UI 和 Creative 内容的 integration-risk；PlayerEntityId、InputIntent、CreativePower 等共享候选标 crossSubsystemOwner: integration-review。

## 输出要求

- 将完整研究报告只写入：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P08-player-input-gameplay-public-decomposition.md
- 不要写入输入分区报告、公共提示词、其他分区报告、源代码、测试、项目文件或迁移文件。
- 报告必须逐条覆盖本分区成员，给出成员归属表、状态所有权、Component/System/Query/Command/Adapter/Projection 边界、接口契约、依赖方向、顺序约束、不拆分项、兼容策略、evidence-gap、blocking-decision、focused verifier 计划和 Integration Handoff。
- 本轮只读审查不运行编译型命令；没有实际执行的 verifier 必须写 verificationStatus: not-run。
- proposed 类型、路径和接口必须明确标记 proposed；不得写成当前 NLTX 已存在或已经迁移。
- 跨两个或以上分区使用的类型、接口、ID、快照、值对象和 System 顺序只能提出候选，必须写 crossSubsystemOwner: integration-review，不得在本报告单方面裁决。
- 不得修改任何生产代码、测试、项目文件、生成器、公共协议、分区报告或其他并行会话输出。

## 分区完成检查

- [ ] 逐条覆盖本分区全部 283 条成员，成员事实来自输入表但关键语义已回到 Version4 复核。
- [ ] 只使用本分区叶子清单；没有复制、重新设计或宣布其他 P 分区成员。
- [ ] Component 只承载内聚权威状态；派生值、缓存、查询、短期 payload、日志、网络/存档/UI 输出按正确角色隔离。
- [ ] 所有共享候选写 crossSubsystemOwner: integration-review；没有把 proposed 设计写成已实现能力。
- [ ] 未执行的构建、测试和 verifier 均明确 verificationStatus: not-run；证据缺口、冲突和 blocking-decision 已单列。
- [ ] 输出文件路径唯一为：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P08-player-input-gameplay-public-decomposition.md

## 会话结束回报

最终消息只回报以下字段：
- partitionId: P08
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P08-player-input-gameplay-public-decomposition.md
- evidenceSourcesRead: 实际读取的 Version4 文件/类型、必要的完整源码补证、tModLoader 页面和 SS14 参考
- evidenceGaps: 仍缺失或冲突的关键证据
- blockingDecisions: 需要最终整合会话裁决的 owner/顺序/事务边界
- verificationStatus: not-run
- productionCodeModified: no

本提示词只定义本分区的专属范围和审查抓手；公共协议负责共同证据规则、报告结构、禁止项和 Integration Handoff。
