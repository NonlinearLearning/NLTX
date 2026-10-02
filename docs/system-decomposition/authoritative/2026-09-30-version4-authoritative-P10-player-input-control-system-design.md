# Version4 权威 P10 玩家输入与交互 System 设计

文档 ID：DESIGN-2026-09-30-AUTH-SYS-P10  
逻辑域：system-decomposition  
产物类型：design  
状态：draft  
范围：authoritative P10 玩家输入与交互，11 个叶子组、118 个成员  
证据入口：[P10 System 拆分报告](../reports/2026-09-18-system-decomposition-authoritative-P10-player-input-control.md)、Version4 `Player.cs` 与 CPG Query API manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`  
canonical 路径：`docs/system-decomposition/authoritative/2026-09-30-version4-authoritative-P10-player-input-control-system-design.md`  
partitionId: P10  
taskNumber: AUTH-SYS-P10  
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P10-Player-Input-Control.md  
systemReport: [2026-09-18 P10 System 拆分报告](../reports/2026-09-18-system-decomposition-authoritative-P10-player-input-control.md)  
designStatus: proposed  
evidenceStatus: partial  
nltxStatus: partial  
verificationStatus: partial  
sourceModified: true

## 1. 目的与范围

本文把 P10 报告中的 11 个叶子组、110 个字段、8 个属性，共 118 个成员，归纳为候选 System、Query、Adapter 和 Projection 边界。叶子组只定义本分区库存，不等于运行时 System。本文不确认最终 Owner、全局调度顺序或旧新行为等价。

证据优先级：

1. Version4 目标源码是待迁移行为的主依据。
2. CPG Query API 提供选定路径中的符号和关系候选；索引未绑定源码快照，查询零命中不表示无关系。
3. 用户指定的完整参考项目用于回读相邻源码和辨识版本差异，不覆盖 Version4 事实。
4. Space Station 14 只参考 ECS 中 System、Query、Component 与 Projection 的组织方式，不证明 Terraria 行为。
5. 当前 NLTX 隔离核心用于描述现状，不证明 production 集成或调度已经接通。

## 2. 证据基线

| 来源 | 本次核对内容 | 可支持的结论 | 状态 |
| --- | --- | --- | --- |
| Version4 Player.cs | SHA-256 E5B301E3401F61E37BF3291754670BCC123D6593EDD94597C4E9109C4030FE86 | P10 主体行为、Player.Update 局部顺序、ItemCheck、channel、selection | 当前源码已回读 |
| Version4 MessageBuffer.cs / NetMessage.cs | SHA-256 0F474225FA9D98C539F61249619273A44E73F69A4CF388B44258433A2D15D5EE / 87B596BD8467B9C470DD1F2AD6963EBADE257689F87395163AB136C149BEBC9A | Packet 13 编解码包含 P10 与非 P10 字段 | 当前源码已回读 |
| Version4 Projectile.cs | SHA-256 97C3D68586AEF862411699E428F5C4B18CEE1F8051B684BE9F86FA7B60FAE83B | Projectile 创建/销毁路径向 Player 交接 channel 信息 | 当前源码已回读 |
| Version4 CPG Query API | D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite；manifest 6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364；SourceSnapshotId=null | 选定源码 shard 中的符号、成员使用和静态调用候选 | partial |
| 完整参考项目 | D:\TRbackup\无任何删减通过编译；无 .git；TerrariaServer.csproj 声明 net40 / x86 | 交叉回读调用关系和版本差异 | 辅助参考，不作 Version4 真值 |
| 完整参考项目源码 | Player.cs SHA-256 367E6C3249F064DD9CCF8E421EBEBF6EE3AA745B6521F07A8BEBA4A3C2FF612C；MessageBuffer.cs 48AABBBF4E0967964D97598E0168E9252ADD3DD868C66C6DA46AA87670EACAAB；NetMessage.cs F3066C50715D7C49B8BF2CC852B015303AD7F4C12ADC191C72FC0FE46181E7E2；Projectile.cs 8D6428FA1D098B26ADFDC1D6CC028251814B40CD3ACE576F0BFF462FB19D6038 | 对照 Player、Packet 13 收发和 Projectile 方法形状 | 辅助参考，不作 Version4 真值 |
| 当前 NLTX | D:\TRbackup\NLTX\src\NSSLC\Component\Player | 若干输入、release/repeat、raw-control 到 release/repeat 输入投影、Packet 13 身份路由 Query、ItemCheck 局部转换、tile-release、selection Query、SetMatch 单次与 PlayerFrame 顺序组合 Query、instrument、projection 与 item-space 隔离核心存在 | partial；Packet 13 全协议 Adapter 和唯一 writer、SetMatch PlayerFrame 接线、ItemCheck 后续同步执行体与 selection writer 未实现 |
| SS14 movement input | `Content.Shared/Movement/Systems/SharedMoverController.Input.cs`：CommandBinds 将方向键映射到 handler；handler 解析 session attached entity 后提交方向、subtick 和按下状态；controller 更新 `InputMoverComponent` 并发布局部事件 | 参考“输入适配 -> 领域状态提交”职责可以分离的工程形态 | 不证明 Terraria 的 session、tick、release 或网络契约 |

CPG API 的定向补查覆盖 8 个选定源码 shard。controlUseItem 返回 15 条成员使用，其中 MessageBuffer 写入和 Player 写入可确认，其他方向仍有 partial / unknown；releaseUseItem 返回 9 条，其中 4 条为确认写入、5 条方向未知；releaseUseTile 返回 5 条，其中 3 条为确认写入、2 条方向未知；selectedItemState 返回 5 条，其中 1 条为确认写入；nearbyActiveNPCs 返回 12 条，包含 Main 确认写入和 NPC 确认读写，其余多为方向未知。instantMovementAccumulatedThisFrame 在所选范围零命中且为 partial，不是无使用证明。

Find-CpgCallSites 对 ItemCheck 返回 complete、2 条 Player.cs 调用；StartChanneling 的无参与 Item 重载各返回 1 条 Player.cs 调用；TryCancelChannel 返回 1 条 Projectile.cs 调用；SetMatch 返回 3 条 Player.cs 调用。TryUpdateChannel 的 API 查询在选定的 Player.cs / Projectile.cs 范围零命中并带 `NoMatchingFactInScannedScope` gap，状态为 partial；Version4 Projectile.cs:10514 仍能直接读到其创建侧调用。此零命中只记录为 CPG 缺口，不当作无调用证明。CPG 对 `_channelShotCache` 只返回一条 assignment-left 使用和两条通过 TryTracking / Matches 的值流候选；Version4 Player.cs:25767、:25777 源码实际有两个赋值点，后两条访问方向与被调方法效果也保持 partial。该差异说明这组 CPG 事实不闭合字段 writer。查询状态 complete 只代表该索引查询完成，不代表动态入口、别名、所有调用者或运行时顺序闭合。

`SetMatchRequest` 是 P10 inputReport 的成员，System 报告把 `SetMatch` 归为非权威的查询候选；它不能因为调用出现在 `PlayerFrame` 就从 P10 清单中移除。Version4 的三个调用点位于 Player.cs:20086、:20099、:20112，依次将返回值应用到 legs、legs、head，后续调用会读取前一步更新后的 legs。完整参考项目对应调用也保持该次序。两项目 `SetMatch` 方法段均为 240 行且逐行比较无差异；方法使用 `CallTracker`、通过 request 的 `Player.mount` 读取 mount 状态，并在匹配成功时更新 `ref bool somethingSpecial`。CPG 选定 Player.cs shard 的 call-sites 为 complete、3 条，callable facts 为 partial。当前隔离单次与组合 Query 已存在；诊断效果、输入引用边界、可能的其他调用范围及纯度仍为 partial/unknown，不能将隔离实现写成生产接入或强纯查询。

Release/Repeat 追查将只读查询范围限制为 `Player.cs`、`MessageBuffer.cs`、`NetMessage.cs` 与 `Main.cs`。`controlLeft/right/up/down/jump` 存在同名的多个 `Player.cs` 符号候选，成员使用中有确认写入也有方向未知项；`releaseLeft/right` 的 CPG 使用方向为 unknown，但 Version4 移动更新源码可见相应赋值（17057-17073）；`releaseUp` 查询返回两个确认赋值点；`leftTimer/rightTimer` 查询零命中且为 partial，但同一源码区段可见其赋值和递减（17064-17090）。`UpdateControlHolds` 的调用点查询也是 partial / 零命中，但当前 Version4 源码直接可见三个 Player 调用点（16073、16317、16974）。Version4 还有 `releaseJump` 的其他局部赋值（12548、12553、13765、13770、13778、15243、16497）及 `releaseDash` 赋值（13001），因此六字段投影不闭合 legacy writers。该索引的 `SourceSnapshotId=null`，字段身份、缺失关系与完整写者闭包仍保留缺口。

本轮为 ItemCheck 局部 gate/animation/tail step 使用只读 CPG Query API 复核：`ItemCheck` 在 `Player.cs` shard 的 call-sites 查询为 complete、2 条；同 shard 的 member-use 查询返回 `itemAnimation` 71 条（8 write confirmed、63 access direction unknown）、`controlUseItem` 10 条（2 write confirmed、8 unknown）、`releaseUseItem` 9 条（4 write confirmed、5 unknown）、`pendingItemReuse` 4 条（3 write confirmed、1 unknown）、`itemTime` 20 条（5 write confirmed、15 unknown）。`reuseDelay` 名称解析同时命中 Player.cs 与 Item.cs 字段；限定 Player 字段后，Player.cs member-use 查询返回 5 条（2 write confirmed、3 access direction unknown）。这些是单 shard 的索引 facts；其他 writer、跨 shard readers、控制方向不明项、被调 effects 与 SourceSnapshotId=null 的源码绑定 gap 不因此闭合。

### 2.1 完整参考项目的版本差异

完整参考项目与 Version4 存在必须保留的差异，不能从参考源码直接复制行为：

- ItemCheckWrapped：完整参考项目会执行 gamepad smart cursor 与 smart-select 逻辑；Version4 Player.cs:19603 附近的对应区块被注释。
- ItemCheck：完整参考项目 Player.cs:43133 在 ItemCheck_HandleMount 前增加本地 ShouldFastUseItem 及 cursor item icon 逻辑；Version4 Player.cs:23435 的对应入口没有该段。
- 完整参考项目在 animation 到零后另调用 `PlayerInput.TryEndingFastUse()`（Player.cs:43328）；Version4 Player.cs:23552-23558 没有该客户端输入副作用，不能并入 `AdvanceAnimationFrame`。
- 完整参考项目在 `itemTime` 递减到零后还会针对本地玩家及四种 item type 执行 `EmitMaxManaEffect()`（Player.cs:43337-43350）；Version4 Player.cs:23565-23568 没有该分支。`CompleteFrameTail` 只返回清理请求、pending reuse、release edge 与 itemTime 下一值，不调用 `TurnToAir`，也不移植该 presentation effect。
- Packet 13：完整参考项目 `D:\TRbackup\无任何删减通过编译\Terraria\MessageBuffer.cs:949-958` 在自回环判断后仅当 `Main.netMode == 2` 才把 payload slot 映射为 `whoAmI`；Version4 `D:\TRbackup\Version4\Terraria\MessageBuffer.cs:652-660` 在同样的自回环判断后无条件赋 `whoAmI`。两项目 `NetMessage.cs` case 13（Version4 :440 起、完整参考 :444 起）的字段写入顺序与条件字段块一致。目标路由遵从 Version4；完整参考只用于识别差异，不推断 Terraria session 映射或网络权限。
- UpdateReleaseUseTile 的已读主体，以及 channel expected type / projectile index 的局部交接形状在两处源码中相符。此相符只用来佐证可疑关系，不提升 Version4 之外的语义。
- Channel 局部行为已回读 Version4 Player.cs:25762-25798：无参 `StartChanneling()` 设置 channel 并清 expectation；Item 重载只在 `item.channel` 为真时设置 channel 和 `item.shoot` 对应的 expected type，projectile index 使用默认值；`TryUpdateChannel` 通过 type 匹配后跟踪 index；`TryCancelChannel` 仅在 key 匹配时清 channel。Version4 Projectile.cs:10514 和 :46443 分别在本地 owner 条件下向 Player 交接创建与销毁通知。完整参考项目对应局部结构位于 Player.cs:53535-53573、Projectile.cs:10524 和 :67881，已观察到的 value matching / tracking 规则相符。此处不闭合其他调用者、重复或乱序通知、同帧顺序及 channel state 唯一写者。
- 完整参考项目还在 Player.cs、Main.cs、Mount.cs、GameContent.ObjectInteractions/AHoverInteractionChecker.cs 与 UI.Gamepad/UILinkPointNavigator.cs 有额外 releaseUseTile 清零点；Version4 全仓当前可见的直接赋值仅为 Player.cs:17978、19663、25758。参考项目额外路径不移植到 Version4 目标模型，也不据此宣称 Version4 唯一 writer 已闭合。
- 完整参考项目在本地输入复制后另写 `releaseLeft/right = !controlLeft/right`（Player.cs:17359-17369），并在 `UpdateControlHolds` 中加入 flexible tile wand 的 releaseUp/down 消费与循环偏移修改（Player.cs:29334-29357）；Version4 没有前一写入段，且 `UpdateControlHolds` 只更新 releaseUp（Player.cs:17877-17889）。这些额外 writer/effect 不进入 Version4 投影或目标状态。
- 完整参考项目的 ItemCheck channel continuation 在 kite 判断后额外检查 `whoAmI == Main.myPlayer && Main.LocalPlayerHasPendingInventoryActions()` 并清除 `flag5`（Player.cs:43307-43310）；Version4 Player.cs:23529-23545 没有该分支。`ShouldKeepChanneling` 只表达 Version4 顺序，不把参考项目的本地 inventory-action 条件移入目标规则。
- `ApplyReuseDelay` 是额外的方法体缺口：Version4 Player.cs:25649 的方法体为空，ItemCheck 在 Player.cs:23466-23469 以 `itemAnimation == 0 && reuseDelay > 0` 调用；完整参考项目 Player.cs:53407-53414 的对应方法把 `reuseDelay` 赋给 `itemAnimation` 与 `itemTime` 后清零，并在 Player.cs:43208-43210 使用相同调用条件。CPG `Find-CpgCallSites` 在 Version4 `Player.cs` shard 返回 complete、1 个调用点；`Get-CpgCallableFacts` 为 partial，目标方法体仅有入口/出口，不能从该索引恢复被省略行为。`reuseDelay` 符号名有 Player 与 Item 两个字段候选；限定 Version4 Player 字段后，member-use 返回 5 条，其中 2 条写方向 confirmed、3 条 access direction unknown。当前 `ApplyReuseDelay` 是参考源码支持的 proposed 局部值转换，证据为 corroborated/partial，不是 Version4 confirmed，也未接入生产 ItemCheck。
- 完整参考项目还显示该局部方法只改三个 timer 字段；完整 ItemCheck 的调用顺序、外部效果与错误/部分提交边界仍未迁移。新转换只描述返回值，不直接写共享状态。

本次未编译或测试完整参考项目。目录名中的“通过编译”不作为本次验证结果。

当前 NLTX 的 `PlayerChannelCancellationAdapter` 增加了 `Begin(projectileType)` 与 `Reset()` 两个纯值工厂，分别表示创建带 expected type / 默认 index 的 key，以及清空 key。`PlayerItemUseExecutionSystem.ShouldKeepChanneling` 也只计算 Version4 的输入覆盖结果，不写 channel 状态。Item.channel 条件、生产调用路由与 Projectile 生命周期 owner 仍由未闭合的 G-Channel / G-Schedule 阻塞。

## 3. 11 个叶子组到目标职责的追踪

| P10 叶子组 | 目标职责 | 目标角色 | Owner 状态 |
| --- | --- | --- | --- |
| PlayerInteractionInputState | UI / 交互门控、ItemUse 重用、selection、NPC/combat 等拆分处理 | Query、ItemUse、Selection、跨分区交接 | 混合组；未确认项 integration-review |
| PlayerControlAndReleaseInput | 原始控制提交与方向/jump/hover/dash 边沿、计时器 | Raw Input System、Release/Repeat System | 候选 |
| PlayerItemUseAndChannelIntent | ItemCheck 执行、tile interaction 与 Projectile channel 生命周期 | ItemUse Execution、Tile Owner、Adapter | ItemUse 边界候选；跨界项 unknown |
| PlayerInformationWorldAndMovementState | 移动帧累积、PVP、声音、命中、执行器锁 | Movement / Combat / Audio / integration-review | 不由 P10 catch-all System 持有 |
| PlayerInformationNavigationAndTimeState | 导航和时间仪表能力事实 | Navigation capability System / time Query | capability 候选；时间输入与消费者 unknown |
| PlayerInformationDetectionAndWiringState | 探测能力事实与 Wiring 显示 | Detection capability System、Wiring/UI seam | 拆分；Wiring owner integration-review |
| PlayerBuilderInteractionDefinitions | 固定 toggle ID 与 Count | Catalog / Definition Query | 非调度节点 |
| PlayerSelectionState | SelectedItemState 转换与 SelectionRadial UI 绑定 | Selection Owner、UI-local state | 与 P09 决策前不得新建唯一 authority |
| PlayerInputSyncAndMatch | 输入快照、SetMatch 请求/结果 | Projection、Adapter、Query | 非权威状态；SetMatch purity partial |
| PlayerBuilderOverlayState | ruler overlay 输出 | One-way Projection | 非调度节点 |
| PlayerItemSpaceAndSettings | ItemSpace 结果与 DashControl 设置 | Query、Settings Adapter | inventory/VoidVault/settings scope unknown |

该映射覆盖的是 11 个叶子组的职责去向。118 个原始成员的名称、原声明位置和成员级状态仍以 inputReport 与 System report 为准；本文不改写成员库存。

## 4. 候选 System 与 API 角色

| 候选边界 | 决策 | 拥有/提交的候选状态 | 明确不拥有 | 候选 API 与现有核心 |
| --- | --- | --- | --- | --- |
| PlayerRawControlInputSystem | keep，保持窄输入提交边界 | 验证后的 player-scoped P10 raw control facts | session 身份、Packet 13 版本、序列/重放策略、非 P10 facing/direction、movement 结果 | 现有 PlayerRawControlInputSystem 可隔离提交；Packet 13 Adapter 与生产 writer 未观察到 |
| PlayerReleaseAndRepeatSystem | separate 于 raw input | releaseJump/Up/Down/Left/Right/Dash、hover 持续意图、left/right repeat timer | releaseUseItem 与 releaseUseTile 的独占写入；ItemUse/tile side effects | `PlayerReleaseAndRepeatInput.FromRawControls` 纯投影六个 control facts，hover 意图仍显式传入；不推导 `ControlDownHold`、不提交权威状态。生产 writer、consumer 与 schedule 未接入，仍受 G-Release/G-Schedule 约束 |
| PlayerItemUseExecutionSystem | keep 单个同步执行边界 | ItemCheck 的有序状态转换与 item-use 可观察结果候选 | 全部控制字段、tile interaction、selection、Projectile 生命周期 authority | `BeginCheck` 仅实现 pending-reuse reset 与 crowd-control early-return；`ShouldEnterStartUseBranch` 计算使用尝试 gate；`ShouldKeepChanneling` 计算 channel continuation；`AdvanceAnimationFrame` 与 `CompleteFrameTail` 返回局部计时/清理转换；均未接入完整同步执行体，现有 PlayerItemUseIntentSystem 不是 ItemCheck 替代品 |
| Selection Owner | separate 独立状态机，但条件依赖 P09 | Selected/Hotbar/Buffered/Overridden 转换 | SelectionRadial UI binding 的长期 authority | 现有 PlayerSelectionQuery 只读；Selection writer / 唯一 Component 尚未观察到 |
| Navigation Instrument System | keep | watch/compass/depth/weather/calendar/stopwatch capability facts | clock/time source、UI consumer、equipment 全局调度 | 现有 PlayerNavigationInstrumentSystem 为隔离核心；ResetEffects 与 equipment 顺序须集成 |
| Detection Instrument System | keep，与 navigation 分开 | fish/third-eye/jar/critter/ore/dream-catcher capability facts | Wiring visibility、NPC/Projectile 的最终行为 owner | 现有 PlayerDetectionInstrumentSystem 为隔离核心；counter 与消费者闭包 unknown |
| Channel lifecycle | partial，作为 ItemUse/Projectile 间 value Adapter | expected projectile type/index 的匹配交接候选 | 第二份 channel authority、持有活 Projectile 引用 | 复用 PlayerChannelCancellationAdapter；生命周期通知、重入和重复语义 unknown |
| Builder definition / overlay | keep 为 Query + one-way Projection | 无新增权威状态 | UI 反写 builderAccStatus | 现有 catalog / definition Query / overlay Projection；builder writer 与 consumer seam unknown |
| Input sync / SetMatch | keep 为 Projection、Adapter、Query | immutable snapshot 与短时 request/result | 原始输入 writer、network schema authority | 现有 InputSyncQuery/Projection；SetMatch purity 和完整 callers partial |
| ItemSpace / DashControl | keep 为 Query + Settings Adapter | 明确事实的计算结果、设置访问转换 | Inventory/VoidVault commit、用户/World preference persistence | 现有 PlayerItemSpaceQuery、PlayerSettingsAdapter；snapshot producer 与 settings scope unknown |

### 4.1 不接受的拆分

- 不建立一个 PlayerInputSystem 包揽全部 P10 成员。P10 同时包含输入、ItemUse、选择、movement、网络、装备能力、UI 投影和 settings，缺少共同不变量。
- 不把 11 个叶子组机械拆成 11 个调度 System。Catalog、Query、Projection、DTO 和 Adapter 不因类型数变成 tick 节点。
- 不因现有类名带 System 就判定它已注册到 production scheduler 或成为唯一 writer。
- 不把 ItemCheck 变成延迟事件队列；源码观察到的是同步执行，延迟会改变同帧可见性和 side-effect 顺序。
- 不让通用 Release System 独占 releaseUseItem 或 releaseUseTile。ItemCheck 与 tile interaction 均写这些事实。

## 5. Legacy 入口到新 API 组合

| Version4 入口/行为 | 候选组合 | 必须保留的契约 | 未闭合项 |
| --- | --- | --- | --- |
| MessageBuffer packet 13 decode / NetMessage encode | Packet 13 route Query -> protocol Adapter -> raw input、selection 与其他真实 Owner；Net projection 保持兼容 envelope | Version4 self-echo skip 与 sender-slot route；packet 字段、分支和输入可见前顺序 | session/authentication/active-slot 校验、权限、seq/replay/重复包、失败策略 |
| Player.Update -> ItemCheckWrapped -> ItemCheck | 后续应同步调用 PlayerItemUseExecutionSystem；当前隔离 gate、animation 与 frame-tail 转换只返回值/待执行 action | Version4 先处理 CC gate；start-use gate 位于前置 helper 后（Player.cs:23502）；channel 覆盖顺序位于 Player.cs:23529-23545；animation/pending-reuse 位于 :23546-23558；frame tail 依次请求空物品清理、更新 release edge、递减正 itemTime（:23559-23568） | 所有局部 gate/值转换均未接入；mana-regeneration delay、`TurnToAir` effect、release/item-time 提交、旧 facade route、其余 effects、global scheduler 和同帧其他 readers 仍 unknown |
| Version4 movement control facts -> release/repeat state | `PlayerReleaseAndRepeatInput.FromRawControls` 投影六个 raw controls，再由隔离核心 `Advance` 计算 release 与 repeat timer | hover up/down 必须作为显式输入；不从同名或相关控制字段猜导出；保持 Version4 左右 7 tick timer 的边沿转换 | raw snapshot 采集/提交点、release 多写入者、reset 与同帧 consumer 顺序、production schedule unknown |
| ItemCheck / ItemCheck_AutoReuseLogic | ItemUse System + Inventory/Item/Tile/Entity ports | CCed early return、selected/buffer gate、reuse、mana、animation/time、release、item effects | 效果提交点、失败/部分提交、完整 helper closure unknown；`ApplyReuseDelay` 只按完整参考形成 partial 值转换 |
| UpdateReleaseUseTile | `PlayerTileInteractionReleaseSystem.Advance` 更新现有 interaction-lock state；Release/Repeat 不持有 tile release | lock timer、mouseInterface、tileInteractAttempted 对 releaseUseTile 的影响 | 隔离转换核心已实现；生产提交点、完整 writer 闭包及相对 Player.Update 阶段外的顺序 unknown |
| SelectedItemState.Select | Selection Command -> 唯一 selection owner；查询使用 Selection Query | local/remote 差异、空 slot、buffer/override、sound、AFK counter | P09 共同唯一 owner、radial -> inventory 命令路径 |
| StartChanneling / TryUpdateChannel / TryCancelChannel | ItemUse 记录 expectation；Projectile Adapter 传 immutable lifecycle fact；一个 channel owner 判定 | expected projectile type/index 匹配及 aiStyle 99 / ai[0] == -3 例外 | callback 完整闭包、重复通知与全局相对时序 |
| accessory application / ResetEffects | 导航与探测各自更新候选能力 facts；消费者读取快照 | reset-before-apply 及旧 equipment frame 可见性 | accessory 遍历顺序、clock 与全部消费者 unknown |
| BuilderAccToggleIDs / rulerLine / rulerGrid | catalog Query + one-way overlay Projection | ID 值、Count、投影确定性 | builderAccStatus writer、Wiring/UI consumer 与 definition versioning |
| PlayerInputSyncCache / PressingAnyInput | immutable InputSync Projection -> network adapter | 五字段派生结果和 Packet 13 兼容形状 | projection writer 接线与 snapshot commit barrier |
| SetMatch(request, ref bool) | 单次 Query + PlayerFrame 顺序组合 Query behind compatibility facade | 返回 slot 与两个独立 ref 输出；body -> legs -> head 顺序已回读 | purity、CallTracker/Player.mount 边界、全部调用范围与边界输入仍 partial |
| ItemSpace overloads / DashControl | ItemSpace Query；Settings Adapter | personal inventory 与 VoidVault 的区分、原 enum/default | inventory/VoidVault 写 Owner、设置持久化 scope unknown |

### 5.1 ItemCheck frame-tail 转换

Version4 `Player.cs:23559-23568` 的提交顺序是：当 animation 为零、item 为空且 type 非零时执行 `TurnToAir` 并清 pending reuse；随后令 `releaseUseItem = !controlUseItem`；最后仅在 `itemTime > 0` 时递减。隔离的 `CompleteFrameTail` 以值结果表达同一顺序：清理条件为 `animation == 0 && item.IsAir && item.type != 0`；清理请求成立时下一 `HasPendingReuse` 为 false；下一 `ReleaseUseItem` 为 `!controlUseItem`；下一 `ItemTimeRemainingTicks` 只在正值时减一。它不执行 item cleanup，也不写共享状态；未来调用方必须在同一 ItemUse 执行边界按上述次序提交。

### 5.2 ApplyReuseDelay 局部转换

完整参考项目的 `ApplyReuseDelay` 方法只把当前 `reuseDelay` 写入 `itemAnimation` 与 `itemTime`，然后清零 `reuseDelay`。Version4 保留相同调用前置条件，但目标方法体为空，因此 NSSLC 中的 `ApplyReuseDelay` 仅以显式值输入/结果表达该参考行为，CPG writer 方向部分 unknown；它没有共享状态写权限，也没有生产调用点。该转换是 `proposed`、证据 `corroborated/partial`，不能单独表示 Version4 已实现或完整 ItemCheck 已迁移。

该 API 的状态转换、生产调用点、`TurnToAir` 的外部效果、itemTime 与 releaseUseItem 的全局 writer 闭包及同帧 readers 均未验证。CPG Player.cs 单 shard 查询返回 itemTime 20 条（5 条确认写、15 条方向 unknown）、releaseUseItem 9 条（4 条确认写、5 条方向 unknown）、pendingItemReuse 4 条（3 条确认写、1 条方向 unknown）；CPG `SourceSnapshotId=null`，这些结果不闭合全局读写关系。

### 5.3 SetMatch Query 候选的静态契约

`SetMatchRequest` 保留在 P10 inputReport 的 `PlayerInputSyncAndMatch` 成员范围内。当前 NSSLC 已有单次匹配 Query 和 body -> legs -> head 顺序组合 Query 的隔离实现；它们仍是非权威候选，没有替换旧 facade 或接入生产 PlayerFrame，也不能据此提升为已确认纯函数或行为等价。

| 项目 | 静态证据 | 设计约束 | 状态 |
| --- | --- | --- | --- |
| 输入字段 | CPG `Get-CpgTypeSurface` 对 `Terraria.Player.SetMatchRequest` 返回 `Player`、`Head`、`Body`、`Legs`、`ArmorSlotRequested`、`Male` 六个成员；`PlayerSetMatchInput` 改为显式值并增加 mount 快照和初始 `SomethingSpecial` | `PlayerSetMatchQuery` 不接收 `Player` 引用；输入包括装备槽值、请求槽、性别、mount active/type 与调用方原 special 值 | CPG surface confirmed；隔离 API 已实现，兼容性 partial |
| 单次匹配结果 | `PlayerSetMatchResult` 返回 `MatchedSlot`、`Matched` 与 `SomethingSpecial`；slot 1 读 Body、slot 2 读 Legs，其他请求读 Head | 未命中返回 `-1`、`Matched=false`，并保留输入 `SomethingSpecial`；不得替换成默认装备 | Version4/完整参考源码 corroborated；完整 switch 组合未验证 |
| Head 201 | 旧请求通过 `request.Player.mount.Active/Type` 读取 mount；mount type 54 时保持 201，否则按 `Male` 返回 201/202 | 值 DTO 显式接收 `MountActive` 与 `MountType` | 局部规则 confirmed；生产快照来源 unknown |
| Body 81 / gender | Body 81 仅在 `Legs == -1 || Legs == 0` 时映射 169；腿部 gender 分支按 `Male` 选择结果 | 保留每个 slot 的显式分支，不折叠为通用装备表 | corroborated |
| 两个 ref 输出 | PlayerFrame 的 body 步骤以独立 `wearsRobe` 调用；legs/head 步骤共用 `somethingSpecial`。Body 166 匹配令 body special 为 false；未匹配保留调用方原值 | 组合结果分别返回 `WearsRobe` 与 `SomethingSpecial`，两者不可合并；只在匹配时应用单步返回的 special 值 | 源码已回读；局部断言覆盖，完整观察兼容仍 partial |
| PlayerFrame 组合顺序 | Version4 CPG `Find-CpgCallSites` 在 `Terraria/Player.cs` 返回 3 个 `internal-static-exact` 调用点；源码和完整参考项目均按 body -> legs -> head 执行，body 命中后更新的 legs 进入后续步骤 | `PlayerSetMatchCompositionQuery` 顺序调用三个单次 Query；命中才更新 legs/head；返回更新后的 head/body/legs 与两个独立 flag | 调用点查询 complete；生产 PlayerFrame 接线 not-run |
| 副作用与纯度 | 旧方法使用 `CallTracker`；CPG `Get-CpgCallableFacts` 为 `partial`，带 `CalleeEffectsNotExpanded`；`SourceSnapshotId=null` | 隔离 Query 未实现旧 `CallTracker` 诊断。Query 纯度、诊断兼容、完整 caller 范围及运行时重入语义保持 `partial/unknown` | partial |

当前隔离 API 为 `PlayerSetMatchQuery.Evaluate(in PlayerSetMatchInput) -> PlayerSetMatchResult` 与 `PlayerSetMatchCompositionQuery.Evaluate(in PlayerSetMatchCompositionInput) -> PlayerSetMatchCompositionResult`。组合 Query 初始化 `WearsRobe` 和 `SomethingSpecial`，先做 body 匹配并将命中 slot 写入 legs，再用更新后的 legs 做 legs 匹配，最后做 head 匹配；body 的 special 输出只更新 `WearsRobe`，legs/head 共用另一 `SomethingSpecial` 值。该组合只计算并返回值，不写 Player 或组件状态。旧 facade 的 `CallTracker`、对 Player.mount 的权威读取边界、装备表版本及生产 PlayerFrame 调用路由仍交 integration review；因此这只是隔离 API 存在和局部核心验证，不是生产接入或完整语义等价。

### 5.4 Packet 13 identity route Query

Version4 `D:\TRbackup\Version4\Terraria\MessageBuffer.cs:652-660` 的 case 13 先读取 payload player byte；当它等于 `Main.myPlayer` 且 `ServerSideCharacter` 为 false 时直接忽略该自回环包，否则目标 slot 无条件设为 `whoAmI`。完整参考项目 `D:\TRbackup\无任何删减通过编译\Terraria\MessageBuffer.cs:949-958` 仅在 `Main.netMode == 2` 时覆盖 payload slot。两项目 `NetMessage.cs` case 13 的字段顺序和条件字段布局在对应源码段相同；这只核对了静态序列化形状，不构成完整 wire compatibility 验证。路由 Query 以 Version4 为目标行为，不移植参考项目的 server 条件。

当前隔离实现 `PlayerPacket13RouteQuery.Evaluate(in PlayerPacket13RouteInput)` 只表达 self-echo skip 和 route-to-sender 两个决策。调用方必须显式提供 sender slot；Query 不解析包字节、不认证 session、不校验 slot 范围/active 状态、不处理权限、sequence/replay/duplicate/error，也不提交输入或其他 P10 状态。它不关闭 G-Identity/G-Protocol，不是完整 Packet 13 Adapter。

本轮通过 `.agents/skills/ecs-system/tools/CpgEvidence.ps1` 只读 API，在 `Player.cs`、`MessageBuffer.cs`、`NetMessage.cs` 三个选定 shard 查询成员使用；符号和 member-use 查询状态均为 `complete`，覆盖仅限这三个 shard。`controlUseItem` / `controlUseTile` / `controlDownHold` / `tryKeepingHoveringUp` 分别返回 12 / 6 / 3 / 3 个 facts，且各在 `MessageBuffer.cs` 有一个 confirmed direct-write fact；其他方向事实不用于闭合 writer 集合。`selectedItemState` 返回 5 个 facts，MessageBuffer 中 `Select` invocation flow 的访问方向为 partial。数据库 manifest 为 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`、project fingerprint 为 `521CBD4C11E7EB731256C232350A54099F431D0E59EBDF3C38880281F169C11B`，`SourceSnapshotId=null`；查询 complete 仅表示所选索引范围完成，调用者、源码绑定与效果闭包仍 unknown。SS14 的 `SharedMoverController.Input.cs` 由 session attached entity 确定 movement target 后提交输入，只佐证“身份绑定在协议/handler 边界”的结构选择，不定义 Terraria 的 session 映射。

## 6. 候选依赖图与顺序

    Packet 13 receive
      -> Protocol/Identity Adapter [authority and validation unknown]
      -> validated input facts
      -> Raw Input Owner
           -> Movement / Interaction / ItemUse readers

    Player.Update observed local path
      -> movement and collision
      -> ItemCheckWrapped
      -> synchronous ItemCheck / ItemUse effects
      -> PlayerFrame
      -> UpdateReleaseUseTile

    Projectile lifecycle
      -> value-only channel adapter
      -> channel owner candidate

    ResetEffects
      -> equipment application
           -> Navigation facts
           -> Detection facts
      -> consumers [incomplete]

    Committed owner state
      -> Query / Projection / protocol adapter
      -> read-only UI or network output

图中只有 Player.Update 的局部顺序、同步 ItemCheck 调用，以及源码中明确存在的 Projectile-to-Player channel 交接属于静态观察。Protocol 到 tick 的全局先后、System 注册、barrier、跨 world/session 顺序和 parallel access 声明均 unknown。该图不是 scheduler DAG。

## 7. 状态 Owner 与 Integration Handoff

| 状态/契约 | Owner 候选 | 已知写入或读取关系 | 状态 |
| --- | --- | --- | --- |
| 原始控制事实 | 一个经协议/本地输入验证的 Raw Input Owner | MessageBuffer 写 control 字段；ItemCheck auto-reuse 也会写 controlUseItem | controlUseItem 双写协调未决 |
| 方向/jump/hover/dash release 与 repeat timer | Release/Repeat System | Player update 和本分区输入路径 | 候选；timer exact tick 语义须验收 |
| releaseUseItem | ItemUse 行为 Owner | ItemCheck 与多个 Player item action 写入 | 不属于通用 release writer |
| releaseUseTile | Tile interaction Owner 候选；状态暂存于 `PlayerInteractionLockStateComponent` | `PlayerTileInteractionReleaseSystem` 实现逐帧转换与本地 gamepad lock commit；其他写入者仍未闭合 | 隔离核心，不是唯一生产 writer；不属于通用 release writer |
| selected item | P09/P10 共同决定的 selection Owner | MessageBuffer 可调用 Select；local/remote 选择路径不同 | integration-review |
| channel / Projectile identity | channel 唯一 Owner 与 Projectile Adapter | Projectile 创建/销毁会调用 Player 相关入口 | channel 单写者和事件闭包 unknown |
| nearbyActiveNPCs | Main/NPC 协作 | Main reset、NPC 确认读写 | integration-review；全局阶段 unknown |
| lastCreatureHit | Combat/Projectile owner | Player 与 Projectile 都有确认写入 | integration-review |
| movement accumulator、PVP、step sound、ActuationRodLock | Movement / Combat / Audio 对应 owner | 源码位于多个 Player/Projectile/系统职责 | integration-review |
| wiring visibility、team/hostile/aggro/selectedKite、accWatchTime | 尚未指定 | CPG/源码覆盖不足或跨域 | unknown / integration-review |
| ItemSpace、VoidVault、DashControl settings | inventory/container 与 settings adapters | 现有 Query/Adapter 核心存在 | 写入者、scope、持久化 unknown |

以下共享契约全部保留 crossSubsystemOwner: integration-review：P09 selection、player slot/whoAmI/session/entity 映射、Packet 13、Movement/dash/hover、Item/Inventory/Tile/Wiring、Projectile/Combat、NPC pressure、equipment/time inputs、Builder/UI overlay、network projection、VoidVault 与 settings persistence、多 world 生命周期。

## 8. 生命周期与副作用约束

- Raw input 仅在身份、active 状态和允许字段校验后提交。Sequence/replay policy 未确认前，不能宣称有防重放能力。
- Release edge 与 timer 必须维持源码可见的消费点；不得通过延迟队列改变同帧 visibility。
- ItemCheck 是同步、effectful 行为。移动到新 System 后仍须保持调用点和 effects 顺序；构造输入 DTO 或纯意图核心本身不构成行为替换。
- selection 本地/远端转换涉及 buffer、override、声音和 AFK 计数；radial binding 独立于 selected inventory item。
- instrument facts 需要同一 equipment frame 的 reset-before-apply；当前项目未确认全局 phase。
- channel Adapter 只能传值，避免保存跨生命周期 Projectile 对象引用。
- Disconnect/reconnect、player slot reuse、spawn/death、world/session unload、异常、网络发送失败、持久化恢复语义均为 unknown。

## 9. 设计状态与开放决策

| 决策 | 当前结果 | 实施前所需输入 |
| --- | --- | --- |
| 原始输入唯一 writer 与 Packet 13 authority | 未决定 | owner、身份校验、服务器映射、序列/重复/乱序策略 |
| Item/tile release 与 movement release 拆分 | movement release/repeat 与 tile release 的隔离核心已分开；item release 仍留给 ItemUse owner | ItemUse/Tile 的唯一 writer、具体 production commit point 与 G-Schedule |
| P09/P10 selected-item authority | 未决定 | P09 integration review 与 SelectionRadial handoff |
| ItemCheck 新 owner 与 effects 端口 | proposed | helper/effect closure、同步提交 API、错误可见性 |
| Projectile channel lifecycle | adapter 候选 | producer/consumer 入口、重复/乱序回调策略、唯一 channel state |
| Instrument frame 与 consumers | proposed | equipment schedule、clock source、消费快照 owner |
| Player entity/session/world mapping | unknown | 生命周期与身份不变量 |

这些问题不阻止交付 proposed 静态设计；它们阻止相应 production writer 接线和行为等价声明。

## 10. 状态声明

以下状态字段记录本设计成稿时的静态状态；后续实现和有限验证以同目录执行文档为准。

- designStatus: proposed
- evidenceStatus: partial
- nltxStatus: partial
- verificationStatus: partial（SetMatch 局部 Query 经受影响项目 focused build/verifier 验证；整个 P10、PlayerFrame 接线与旧新行为等价仍 not-run）
- 现有隔离状态转换包括 `PlayerInteractionLockStateComponent`、tile release、ItemCheck 局部 gate、animation step、`ApplyReuseDelay`、`CompleteFrameTail`、SetMatch 单次/顺序组合 Query 与 Packet 13 路由 Query；没有修改 Version4 或完整参考项目，也没有接入生产调度。
- `Terraria.Player.InputControl.Verification` 的 focused verifier 覆盖上述局部核心，并新增 Packet 13 自回环忽略、ServerSideCharacter 例外和 sender-slot 优先三个断言。输出为 `PASS: tile release, channel expectations, item-use gates, SetMatch, PlayerFrame composition, and Packet 13 routing preserve Version4 core rules`。
- 此 focused verifier 不覆盖 Packet 13 全字段编解码、session/authentication/slot 校验、replay/duplicate 策略、生产 writer、完整 SetMatch 映射、旧 `CallTracker` 诊断、完整 callers、生产 PlayerFrame 接线、完整 ItemCheck、生产调度、跨分区 writer 闭包、全 P10 行为或旧新行为等价；这些仍为 not-run/unknown。
- 本文没有声明 migration-success、behavior-equivalent、verified 或 deletion-safe。
