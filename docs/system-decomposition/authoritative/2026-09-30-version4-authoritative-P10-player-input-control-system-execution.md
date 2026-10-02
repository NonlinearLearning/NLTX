# Version4 权威 P10 玩家输入与交互 System 执行计划

文档 ID：EXECUTION-2026-09-30-AUTH-SYS-P10  
逻辑域：system-decomposition  
产物类型：execution  
状态：draft  
范围：authoritative P10 玩家输入与交互的候选迁移工作包、执行门槛和验证计划  
证据入口：[P10 System 拆分报告](../reports/2026-09-18-system-decomposition-authoritative-P10-player-input-control.md)、[P10 System 设计](2026-09-30-version4-authoritative-P10-player-input-control-system-design.md)、Version4 `Player.cs` 与 CPG Query API  
canonical 路径：`docs/system-decomposition/authoritative/2026-09-30-version4-authoritative-P10-player-input-control-system-execution.md`  
partitionId: P10  
taskNumber: AUTH-SYS-P10  
sessionId: 7096c7ad8a2645708782f2ca139820d2  
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P10-Player-Input-Control.md  
designDocument: [P10 System 设计](2026-09-30-version4-authoritative-P10-player-input-control-system-design.md)  
systemReport: [P10 System 拆分报告](../reports/2026-09-18-system-decomposition-authoritative-P10-player-input-control.md)  
targetRoot: D:\TRbackup\NLTX\src\NSSLC  
executionPlanStatus: proposed  
implementationStatus: partial  
verificationStatus: partial  
sourceModified: true

## 1. 执行边界

本计划把 P10 proposed 设计转成后续可执行的工作包，不代表这些工作包已实施。当前报告的 runner session 已用原 sessionId 结算；本文是后续设计/执行文档，不重开或更改原报告结算。

### Claim / settlement 记录

| 字段 | 值 |
| --- | --- |
| partition | `P10` |
| task | `AUTH-SYS-P10` |
| inputReport | `D:\TRbackup\NLTX\docs\migration\ledgers\authoritative-20-partitions\P10-Player-Input-Control.md` |
| outputReport | `D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P10-player-input-control.md` |
| sessionId | `7096c7ad8a2645708782f2ca139820d2` |
| settlement | runner state `completed`，使用上述原 sessionId；本文不重新 claim、不改写 inputReport/runner ledger |
| delivery | `proposed` 静态执行计划；不代表代码迁移、行为等价或迁移成功 |

当前只确认 NSSLC 有输入、release/repeat、ItemCheck crowd-control entry gate、start-use gate、channel-continuation gate、animation/pending-reuse 与 reuse-delay/frame-tail 局部转换、item-use intent、channel value adapter、selection Query、SetMatch 单次与顺序组合 Query、Packet 13 身份路由 Query、navigation/detection、input projection、builder catalog/overlay、ItemSpace Query 和 settings adapter等隔离核心。没有观察到 production scheduler 接线、完整 Packet 13 Adapter、完整 ItemCheck 执行路径、PlayerFrame 对 SetMatch 新 Query 的生产接线或唯一 selection writer。隔离核心不能当成当前 runtime Owner。

此前在 `src/NSSLC/Component/Player` 收窄了 Raw Input 与 Release/Repeat 隔离核心的命令/字段契约，并新增 `PlayerTileInteractionReleaseSystem`。后续增加 `PlayerReleaseAndRepeatInput.FromRawControls`、`PlayerItemUseExecutionSystem.BeginCheck` 和 channel value adapter；近期增加 `ShouldEnterStartUseBranch` 与 `ShouldKeepChanneling`，分别表达 Version4 ItemCheck 的 start-use 与 channel-continuation 局部 gate。未修改 Version4 或完整参考源码，也未接入 production scheduler、Packet 13、完整 ItemCheck 或 Tile owner。原 runner session 仍以此处的 sessionId 作为来源，不重开或更改其结算。

## 2. 执行前门槛

以下契约必须先由对应 integration review 明确。未知项不可由本执行计划代填默认值。

| Gate | 待决定契约 | 未满足时禁止 |
| --- | --- | --- |
| G-Identity | player slot、whoAmI、network session、ECS Entity、active 状态和 slot reuse 的映射 | 将输入写入权威 Entity；宣称多 world/session 隔离 |
| G-Protocol | Packet 13 编解码边界、ServerSideCharacter / server 身份分支、权限、字段默认、seq/replay/duplicate/error 策略 | 接入唯一 network writer；改变 wire schema |
| G-Release | movement release、item release、tile release 的独立 writer 和 commit point | 将当前 PlayerReleaseAndRepeatSystem 直接接入 production |
| G-Selection | P09 唯一 selected-item Owner、SelectionRadial 到 inventory 的 command handoff | 新建或接入第二个 selection authority |
| G-ItemUse | ItemCheck effects、Inventory/Item/Tile/Entity 端口、mana/reuse/error/partial-commit 可观察边界 | 用 PlayerItemUseIntentSystem 替代 ItemCheck；异步化 ItemCheck |
| G-Channel | Projectile 创建/销毁通知的唯一入口、channel state 唯一 writer、重复/乱序策略 | 复制 channel authority 或保存 live Projectile 引用 |
| G-Schedule | legacy Player.Update、equipment/reset、packet apply 与 System phases 的实际接线和 visibility | 声称已保持全局 tick 顺序、barrier 或同帧可见性 |

## 3. 工作包顺序

### WP0：锁定静态基线与成员追踪

输入：P10 inputReport、System report、本文设计、Version4 源码、当前 NSSLC 核心。

操作：

1. 按 11 个 P10 叶子组为每个成员记录 target role、当前 reader/writer 候选和 owner 状态。
2. 标记 confirmed、partial、unknown、dynamic-risk 和 integration-review，不把旧 Component 执行记录当作 System 接入证据。
3. 固定本轮 Version4 文件 hash；CPG SourceSnapshotId 仍为 null 时保留 snapshot gap。
4. 记录完整参考项目与 Version4 的 Packet 13、ItemCheckWrapped、ItemCheck、releaseLeft/right writer 与 UpdateControlHolds 副作用差异。

退出条件：118 个成员可追溯到 inputReport，跨分区成员不被本工作包决策。

失败处理：若成员无法绑定稳定符号或有新增源码版本差异，保留 unknown 并补证据；不得把成员丢出范围。

状态：proposed；本轮设计核对已查看报告和关键源路径，未生成新的全量成员台账。

### WP1：身份和协议 Adapter

前置：G-Identity、G-Protocol。

提案：在 src/NSSLC/Component/Player 的既有协议边界实现显式输入 Adapter；用 validated command/value facts 调用目标 owner。Packet 13 envelope 内不属于 P10 的 movement、mount、camera 等字段仍由其 owner 协作，不能整包挪给 P10。

验收：

- Version4 packet 13 的字段读写顺序、条件字段和 ServerSideCharacter 分支均有映射。
- 目标 player/session/entity 映射先验证再提交；拒绝输入不会部分修改权威状态。
- sequence、replay、重复包规则来自明确协议决策。
- Packet 13 编解码保持兼容，或任何有意变化有独立版本决策。

回退：保留原协议 route；撤回新 Adapter route 时不删除其他 session 或 owner 的组件。

Version4 case 13 的 route-only 核心已实现为 `PlayerPacket13RouteQuery`。输入是已解码 payload slot、local player slot、ServerSideCharacter 标志及由调用方提供的 sender slot；self echo 且非 ServerSideCharacter 时返回忽略，否则返回 sender slot。Query 不负责 sender/session 认证、slot 有效性与 active 校验、payload 解码、权限、序列/重复包处理、P10 状态提交或 NetMessage 投影，因此 G-Identity/G-Protocol 仍阻止完整 Adapter 与 production writer。

源码核对：Version4 `D:\TRbackup\Version4\Terraria\MessageBuffer.cs:652-660` 先判断 `num210 == Main.myPlayer && !Main.ServerSideCharacter`，随后将 `num210` 无条件设为 `whoAmI`。完整参考项目 `D:\TRbackup\无任何删减通过编译\Terraria\MessageBuffer.cs:949-958` 仅在 `Main.netMode == 2` 时替换为 `whoAmI`；两边 `NetMessage.cs` case 13（Version4 :440 起、完整参考 :444 起）的字段顺序和条件字段布局相同。目标实现遵从 Version4。只读 CPG `Get-CpgMemberUses` 查询 `Player.cs`、`MessageBuffer.cs`、`NetMessage.cs` 三个选定 shard，symbol/member-use 状态均为 `complete`：`controlUseItem` / `controlUseTile` / `controlDownHold` / `tryKeepingHoveringUp` 分别有 12 / 6 / 3 / 3 个 facts，各在 MessageBuffer 有一个 confirmed direct-write fact；`selectedItemState` 有 5 个 facts，MessageBuffer 的 `Select` invocation flow 仍为 partial。manifest 为 `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`、fingerprint 为 `521CBD4C11E7EB731256C232350A54099F431D0E59EBDF3C38880281F169C11B`、`SourceSnapshotId=null`；complete 仅代表所选 shard 查询完成，不闭合全调用者、writer 或效果。SS14 movement handler 使用 session attached entity 作为 movement target，仅作为边界组织参考。

状态：route-query-core-implemented / full-protocol-adapter-blocked-on-contract；G-Identity、G-Protocol 未关闭。

### WP2：唯一 Raw Input 提交

前置：WP1、G-Identity、G-Protocol。

提案：复用 PlayerRawControlInputSystem 的隔离校验核心，在同一个 production boundary 写 raw control facts。旧 reader 先留兼容 view，不能让 legacy 与新 Component 双写。

验收：

- 一玩家一帧最多一次提交；空批、重复玩家、未知/非 active 玩家按协议策略拒绝。
- 批次失败不产生部分提交，原状态保持。
- 每个 raw control 字段只有一个生产 writer；controlUseItem 的 Packet 13 与 ItemCheck auto-reuse 路径经过同一 Owner 或明确协调。
- 本地输入 producer 与 server/remote decoder 均可追溯到同一提交契约。

回退：恢复原始读取 route；保留新核心供隔离分析，不允许双 writer。

状态：core-narrowed / focused-verifier-passed / runtime-integration-not-observed。

### WP3：拆分 Release/Repeat 写入责任

前置：G-Release、G-Schedule。

实施：保留方向/jump/hover/dash release 与左右 repeat timer 的边沿 System；新增 `PlayerReleaseAndRepeatInput.FromRawControls`，只投影 `controlJump/up/left/right/down/dash`，hover up/down 为显式参数。该投影不从 `ControlDownHold` 推导 hover，也不写入 raw-control 或权威状态。`PlayerTileInteractionReleaseSystem` 单独读取 `tileInteractAttempted`、`mouseInterface` 与 `PlayerInteractionLockStateComponent`，更新 tile release 并递减 3 tick lock timer；`ApplyGamepadTileReleaseLock` 只提交组件内的 release=false 与 timer=3。Item release 仍留给 ItemUse Owner。此实现是隔离核心，不是生产接线或唯一 writer 证明。

验收：

- 每个状态的 reset、frame update、consumer 和唯一 writer 均有证据。
- 7 tick direction repeat window、快速按下/释放、相反方向切换与 reset 路径符合 Version4 对照。
- tile interaction lock、mouseInterface、tileInteractAttempted 由独立 tile-release 核心保留；其同帧调用点和完整 tile writer 闭包仍需 integration review。
- releaseUseItem 的 ItemCheck/action 写入仍在正确同步位置；同帧消费者观察不变。

回退：一次只回退本批 release route；不恢复通用 System 对 item/tile 字段的第二写入。

状态：core-split-implemented / raw-control-projection-isolated-core-verified / tile-release-isolated-core-verified / production-integration-blocked。

### WP4：唯一 Selection Owner 与 UI handoff

前置：G-Selection、G-Identity。

提案：先与 P09 决定 selected/hotbar/buffer/override 的唯一 Component 和 writer，再将 PlayerSelectionQuery 作为只读查询接入。SelectionRadial 作为 UI-local state，通过显式 selection command 交接。

验收：

- local 与 remote Select 行为、空 slot、buffer、override、hotbar、sound 和 AFK counters 有完整映射。
- packet decode、UI 和 ItemUse 均依赖同一 selected-item authority。
- radial binding 不直接改权威 selection；P09 不存在并行 writer。

回退：保留旧 selection facade 和只读 Query；未关闭调用者前不删除 SelectedItemState 路径。

状态：blocked-on-P09-owner。

本轮补证仍未关闭此 gate：P09 报告中的 `PlayerEquipmentSelectionSlots` 是 head/body/legs 等装备展示槽，不是 inventory selected/hotbar/buffer/override authority；P09 报告也把读写、网络和调度关系标为 partial/unknown。Version4 CPG 对 `selectedItemState` 在 `Player.cs`、`MessageBuffer.cs`、`Main.cs` 三个 shard 返回 5 条 member use，其中 1 条确认写、4 条方向 unknown；查询 `Status=complete`，但 `SourceSnapshotId=null`，不能据此证明唯一 writer。Version4 `SelectedItemState.Select` 有 local/remote 分支、空槽拒绝、声音和 AFK counter effects；完整参考源码另有 Version4 当前文件没有的 `OverrideSelection`、`Update`、`OnSelectionChanged`，不能把这些额外行为移植成目标事实。

### WP5：同步 ItemUse 执行边界

前置：G-ItemUse、G-Release、G-Selection、G-Schedule。

实施进度：`PlayerItemUseExecutionSystem.BeginCheck` 复现同步 ItemCheck 的入口顺序：先清 pending reuse；CCed 时再清 channel、animation 和 animation max，并返回 `ReturnedForCrowdControl`。正常路径只完成 pending reuse reset 后返回 `Continue`。`ShouldEnterStartUseBranch` 按 Version4 Player.cs:23502 计算 `controlUseItem && releaseUseItem && itemAnimation == 0 && item.useStyle != 0 && !selectedItemState.HasBufferedChange`，输入以显式 `PlayerItemUseStartGateInput` 传入。`ShouldKeepChanneling` 以 `PlayerItemUseChannelContinuationInput` 按 Version4 Player.cs:23529-23545 的 Type 8 mount、kite、buffered selection 顺序计算 channel 是否保留，不写 channel component。`AdvanceAnimationFrame` 对应 Player.cs:23546-23558：仅当 animation 为正时递减一 tick；递减到零且 reuse delay 为零、controlUseItem 与 releaseUseItem 同时为真时设 pending reuse。`CompleteFrameTail` 对应 :23559-23568，返回 `ShouldTurnItemToAir`、下一 pending reuse、`!controlUseItem` release edge 和仅对正 itemTime 递减后的值；它不调用 `TurnToAir` 或写状态。mana-regeneration delay、cleanup effect 与这些结果的生产提交仍未接入。完整参考项目额外的 `LocalPlayerHasPendingInventoryActions` 与 `TryEndingFastUse` 条件不属于 Version4 目标规则。这些仍是隔离核心，不是完整执行面；旧 ItemCheck facade 未改路由，PlayerItemUseIntentSystem 仍只表示 control-use intent。后续实现必须保留 Version4 Player.Update 中 ItemCheckWrapped 的调用位置和 frame-tail 提交次序。

验收：

- CCed early return、selected/buffer 状态、mount/kite、reuse、mana、animation/time、projectile、tile/entity interaction、errors 与所有可观察 effects 按原同步次序发生。
- ItemCheck 内的 releaseUseItem 更新仍由 ItemUse 行为 owner 管理。
- 新路径无隐藏 queue、retry 或补偿行为；若外部契约要求，则先形成独立 decision。
- 旧与新路径切换期间单一写入，不允许双执行。

回退：保留旧 facade 指向旧实现；撤回调用路由，禁止部分 side effect 发生后再回退重跑旧逻辑。

状态：isolated-gates-implemented / focused-frame-tail-core-verifier-passed / full-ItemCheck-and-runtime-integration-not-run。

### WP6：Channel value handoff

前置：G-Channel、G-Schedule。

既有隔离实现：`PlayerChannelCancellationAdapter` 保留 Version4 的 type/index 比较、按 type 更新 index 和 `aiStyle == 99 && ai[0] == -3` 特例；历史记录中的 `Begin(projectileType)` 与 `Reset()` 纯值工厂表示 Item channel key 初始化及无参 channel start 的 key 清理。历史 focused verifier 覆盖 key 创建/重置、tracking、unrelated projectile 与特殊匹配，本轮未修改该核心。

提案：在 G-Channel、G-Schedule 闭合后，由唯一 owner 调用值 Adapter。Projectile 创建/销毁只交 immutable snapshot/key，由该 owner 修改 channel state。当前 helper 没有接入旧 facade，也不判断 `item.channel` 条件，不拥有 `channel` 布尔值。

验收：

- expected projectile type 与 index 一致时才按既有规则匹配。
- aiStyle 99 且 ai[0] == -3 的例外按 Version4 行为覆盖。
- unrelated projectile 不改变 channel；spawn/kill、channel reset 和重复通知的顺序策略有明确来源。
- 不保存跨生命周期的 Projectile 对象引用。

回退：保持原 Projectile -> Player facade；删除新交接 route 前确认不会丢失唯一取消写者。

状态：value-adapter-factories-implemented / focused-core-verifier-passed / lifecycle integration not-run；唯一 writer、创建销毁完整入口、重复/乱序通知和 production schedule 仍 unknown。

### WP7：能力 facts、Queries、Projection 与设置 Adapter

前置：G-Schedule；ItemSpace 的 snapshot producer 另需 Inventory/VoidVault review。

提案：分开接入 Navigation 与 Detection frame facts；Catalog/ItemSpace/Selection/InputSync 使用只读 Query；Builder overlay 和网络快照只单向 Projection；DashControl 通过 Settings Adapter 访问。此工作包不为每个 Query 或 Projection 注册独立 tick node。

验收：

- ResetEffects / accessory application 顺序与 Version4 同一 equipment frame 的可见性一致。
- navigation 与 detection 不合并；accWatchTime、third-eye counter、clock source、消费者和 reset 各自有证据。
- Query 不写权威 state、不触发未声明 effect；Projection 没有回写路径。
- ItemSpace 个人库存/VoidVault 输入快照与 commit owner 已关闭；DashControl 保存 scope 与 static/default 行为已确认。

回退：回退单一 Query/Projection consumer route；权威写入继续由原 owner 管理。

状态：隔离核心部分存在；生产 consumers 与 schedule 未观察到。

### WP8：全分区 writer/readers 闭合与旧 facade 退出

前置：WP1-WP7 对应 gate 已满足，跨分区 Integration Handoff 有 owner 接受。

提案：扫描直接/间接 writer、网络/序列化、events/delegates、reflection/configuration、save/restore、spawn/reset/unload 和动态调用；为所有旧 API 记录 caller route。只有每条路径均能到新唯一 owner 后，才考虑移除旧写者或 facade。

验收：

- controlUseItem、releaseUseItem、releaseUseTile、selected item、nearbyActiveNPCs、lastCreatureHit、ActuationRodLock 等共享成员无未决写者冲突。
- Packet 13 非 P10 字段和相邻分区 API handoff 有明确 owner。
- 所有 legacy callers 有映射；零搜索命中不当成无 caller。
- 没有被误认为 scheduler DAG 的调用图循环；未知边保留并有下一步证据。

回退：保留兼容 facade，逐条恢复受影响 consumer route；任何尚未闭合的入口都阻止旧 writer 删除。

状态：尚未开始；跨分区决策未闭合。

### WP9：SetMatch Query 与 PlayerFrame 顺序组合（隔离核心已实现）

证据：P10 inputReport 中 `SetMatchRequest` 的成员范围；目标源码 `D:\TRbackup\Version4\Terraria\Player.cs`；完整参考项目 `D:\TRbackup\无任何删减通过编译\Terraria\Player.cs`；Version4 CPG Query API 的 type surface、call-sites 与 callable facts。

当前隔离实现位于 `src/NSSLC/Component/Player`：`PlayerSetMatchQuery.Evaluate(in PlayerSetMatchInput)` 以显式 head/body/legs、请求 slot、male、mount active/type 和初始 `SomethingSpecial` 计算 `PlayerSetMatchResult`；结果包含 `MatchedSlot`、`Matched` 与 `SomethingSpecial`。它不接收 `Player` 引用、不写共享状态，也未复制旧方法中的 `CallTracker`。

`PlayerSetMatchCompositionQuery` 复现 PlayerFrame 的局部调用组合：先请求 body slot，命中后更新后续输入的 legs；再请求 legs slot；最后请求 head slot。body 步骤的 ref 输出独立保存在 `WearsRobe`，legs 与 head 共用 `SomethingSpecial`；结果返回最终 head/body/legs 和两个 flag。它只返回值，不接管 legacy facade，也未接入生产 PlayerFrame。

静态规则必须保留：Head 201 的 mount type 54/gender 分支；Body 81 仅在 `Legs == -1 || Legs == 0` 时映射；Body 166 命中时将 body 的 special 输出设为 false；任何未匹配分支返回 `-1` 并保留调用方原 `somethingSpecial`。Version4 `Player.cs:21491` 与完整参考 `Player.cs:37662` 起的 `SetMatch` 方法段各 240 行，逐行相同，只作为局部规则佐证。

源码调用顺序：Version4 PlayerFrame 的 body、legs、head 调用位于 `Player.cs:20086/:20099/:20112`；完整参考项目对应位置为 `:36236/:36249/:36262`。两者顺序相同，body 命中更新后的 legs 传入后续调用。

CPG 查询复核：`Find-CpgCallSites` 在所选 `Terraria/Player.cs` shard 返回 3 个 `internal-static-exact` 调用点，查询状态 `complete`；`Get-CpgCallableFacts` 为 `partial`，含 `CalleeEffectsNotExpanded` gap。快照 `SourceSnapshotId=null`，故查询不闭合诊断效果、动态调用、完整 callers 或源码绑定。

此前的 focused verifier 覆盖 Head 201 mount/gender、Body 166 special 输出、Body 81 默认/占用 legs 条件与未匹配保留、Legs 83 gender，以及组合 Query 的 body 命中后 legs 传递和 `WearsRobe`/`SomethingSpecial` 区分。记录输出：`PASS: tile release, channel expectations, item-use gates, SetMatch, and PlayerFrame composition preserve Version4 core rules`。该局部验证不覆盖完整 switch、旧诊断效果、完整 callers、生产 PlayerFrame 接线或整分区行为。

状态：isolated-single-query-and-composition-implemented / focused-core-verifier-passed / legacy-facade-and-PlayerFrame-production-integration-not-run。Query 仍为非权威 proposed 边界；不表示完整 P10 迁移。

## 4. 后续验证矩阵

Tile release、ItemUse local core 与 SetMatch 局部 Query 行包含此前 focused verifier 记录；其余矩阵行仍是未来 implementation acceptance 输入。

| 验证面 | 关键场景 | 通过前置 |
| --- | --- | --- |
| Packet 13 route Query | self-echo ignore、ServerSideCharacter exception、sender slot overrides payload slot | 此前 focused verifier 的隔离核心断言通过；仅路由决策，G-Identity/G-Protocol、全字段编解码和 production write 未验证 |
| Packet 13 full adapter | local/remote/server、无效/非 active slot、条件 velocity/mount/camera、重复/乱序帧、编解码兼容 | G-Identity、G-Protocol |
| Raw Input | 空批、批内重复、未知玩家、非法 facing、失败后旧状态不变、唯一 writer | WP1 决策 |
| Release/Repeat | reset、左右快速切换、7 tick repeat、hover/hold、mouseInterface、tile lock、ItemCheck release 消费 | G-Release、G-Schedule |
| Tile release core | 3 tick lock decrement、tile attempt gating、mouse-interface gating | 当前 focused verifier passed；生产 writers 与 schedule 未验证 |
| ItemUse local core | start-use gate；channel continuation override order；animation decrement and pending-reuse edge；frame-tail cleanup/release/itemTime result | 当前 focused verifier passed；production commits、effects 和 scheduler integration 未验证 |
| SetMatch local Query and composition | Head 201 mount/gender、Body 166 special output、Body 81 default/occupied legs、Legs 83 gender、body match updates later legs input、`WearsRobe` and `SomethingSpecial` remain distinct | 此前 focused verifier passed；其余 switch cases、diagnostic effects、全 callers 与生产 PlayerFrame 接线未验证 |
| ItemUse | CCed、成功/失败、buffered selection、mount/kite、mana/reuse、animation/time、item/tile/entity effects、顺序与异常 | G-ItemUse、G-Selection |
| Selection | local/remote、空 slot、buffer/override apply/cancel、sound、AFK reset、radial command handoff | G-Selection |
| Channel | expected type/index、unrelated projectile、aiStyle 99 / ai[0] == -3、spawn/kill 顺序、重复通知 | G-Channel |
| Equipment facts | reset-before-apply、accessory 聚合优先级、clock/time、third-eye counter、消费者可见时点 | G-Schedule |
| Query/Projection | 确定性、snapshot 输入完整、无 authority writeback、packet 与 UI projection 一致 | owner commits 已确定 |
| Owner closure | 重复 writers/readers、nearbyActiveNPCs、lastCreatureHit、ActuationRodLock、保存/恢复/重连路径 | WP8 handoff |

完整测试计划还需补全所有 118 个成员和 source report 中的 behavior scenarios。未补全前，局部 focused verifier 不能代表分区行为验收。

## 5. 变更控制与回退规则

- 每个工作包仅接入一个明确 Owner 的 writer，再迁移 readers；legacy 与新 owner 不得双写。
- compatibility facade 保留到直接和间接 callers 闭合；不以编译通过作为删除条件。
- 涉及跨分区数据、Packet 13、player/session/entity ID、lifecycle、顺序、持久化的变更需要 integration-review 接受。
- 失败后回退本工作包的 writer/reader route，不回滚其他工作包或既有用户变更。
- 未确认的 owner、API effect、调度、失败语义、scope 或兼容性一律标 unknown，不用命名推断补齐。

## 6. 本次执行状态与自检

以下记录当前隔离实现。Packet 13 route Query 与 3 个边界断言，以及 focused build/run，来自此前隔离核心实现轮次；本次文档核对只补充 CPG 与参考源码证据，没有重跑构建或测试。Packet 13 生产路由、PlayerFrame 与生产渲染/输入路由仍未接入。

- `PlayerReleaseAndRepeatInput`、`PlayerReleaseAndRepeatStateComponent` 与 `PlayerReleaseAndRepeatSystem` 只表达 jump/up/left/right/down/dash release、hover 意图和左右方向计时；ItemUse/tile release 不再由通用 Release/Repeat 核心承载。
- 新增 `PlayerItemUseExecutionSystem.BeginCheck`，按 Version4 顺序先清 `HasPendingReuse`；CCed 时清 `IsChanneling`、animation remaining/max 并返回 early-return 结果，未触及 item time、reuse delay、selection、inventory、Projectile 或外部 effects。余下 ItemCheck body 和 facade route 仍未迁移。
- `PlayerItemUseExecutionSystem.ShouldEnterStartUseBranch` 只计算 Version4 start-use gate 的五个显式输入，不能替代 `ItemCheck_TryStartUse`、成功结果分支或后续 effects；该 gate 目前没有生产调用点。
- `PlayerItemUseExecutionSystem.ShouldKeepChanneling` 只返回 channel continuation 判定；Type 8 mount 后由 kite 覆盖，buffered selection 最后强制 false。它没有写 `PlayerUseComponent` 或 `PlayerItemUseState`，不构成 channel authority 或 G-Channel 闭合。
- `SetMatchRequest` 按 P10 inputReport 保留在 InputSync/Match 候选中；`PlayerSetMatchQuery` 与 `PlayerSetMatchCompositionQuery` 及其输入/结果值类型已存在。单次 Query 在完整 switch 映射上接受显式 mount/gender/requested slot 和 `SomethingSpecial` 值；组合 Query 按 body -> legs -> head 顺序执行，body 命中后的 legs 值传入后续步骤，并分离 `WearsRobe` 与 `SomethingSpecial`。Head 201、Body 81、Body 166、Legs 83 及组合顺序已有 focused 边界断言。Version4 与 `D:\TRbackup\无任何删减通过编译\Terraria\Player.cs` 的 240 行方法段逐行相同。旧 `CallTracker` 诊断、`ref bool` facade 保持、PlayerFrame 生产接线尚未移植；Query 纯度及全调用范围仍 partial/unknown。
- `PlayerItemUseExecutionSystem.AdvanceAnimationFrame` 只返回 animation 与 pending-reuse 的下一局部值；不执行 mana regen delay，不清 air item，也不提交 state component。完整参考项目的本地 fast-use 输入效果未复制。
- 新增的 `CompleteFrameTail` 只计算 Version4 :23559-23568 的尾段结果：非默认空 item 在 animation 到零时请求 cleanup 并清 pending reuse；release edge 取 `!controlUseItem`；itemTime 仅在正值时减一。它不执行 `TurnToAir`，不写共享 state；当前未接入生产 ItemCheck 调用路径。
- 新增的 `PlayerItemUseExecutionSystem.ApplyReuseDelay` 根据完整参考项目 `Player.cs:53407-53414` 输出 `itemAnimation = reuseDelay`、`itemTime = reuseDelay`、`reuseDelay = 0` 的下一状态值。Version4 的方法体在 `Player.cs:25649` 为空壳，但调用前置条件 `itemAnimation == 0 && reuseDelay > 0` 与完整参考一致；因此实现依据是 corroborated/partial，只表达 proposed 局部转换，不证明 Version4 方法体行为，也不写入或连接生产 ItemCheck。
- 完整参考项目在 ItemCheck frame-tail 的 `itemTime` 归零路径有额外 `EmitMaxManaEffect()`（Player.cs:43344-43350），Version4 对应段没有该调用；此 effect 不属于新增 reuse-delay 值转换，也不移植进目标逻辑。
- 新增 `PlayerReleaseAndRepeatInput.FromRawControls` 作为纯输入投影：只读取 raw `jump/up/left/right/down/dash`，hover up/down 由参数显式传入；不从 `ControlDownHold` 猜 hover 语义，也不构成生产输入接线。
- Version4 `Player.cs` 的 `releaseJump` 还在其他跳跃/移动分支赋值（12548、12553、13765、13770、13778、15243、16497），`releaseDash` 也有独立赋值点（13001）；此投影和隔离核心没有闭合这些 legacy writers，也未取得唯一生产 Owner 证据。
- `PlayerItemUseExecutionSystem.BeginCheck` 局部复现 ItemCheck 的 pending reuse reset 与 CCed early-return 顺序；focused verifier 覆盖 Continue 和 ReturnedForCrowdControl。它没有接替旧 ItemCheck，也没有执行 selection、item effects、Tile、Projectile 或生产调度。
- 既有 `PlayerChannelCancellationAdapter.Begin` / `Reset` 只创建和清空 expected projectile key；Version4 Player 的 channel 更新/取消与 Projectile 的创建/销毁位置在完整参考项目中也有对应源码形状。CPG `TryUpdateChannel` 零命中仍为 partial，来源源码路径可直接看到创建侧调用；CPG 对 `_channelShotCache` 只报告一个赋值，但源码能看到 reset 与 Item key 初始化两个写点，因此 CPG 事实不闭合 writer；这些缺口均没有被解释为无调用或唯一 writer。本轮未修改该核心。
- 在 `CompleteFrameTail` 及其 verifier 断言增加之前，`Test/Terraria.Player.InputControl.Verification` 的历史 focused verifier 覆盖 tile release、channel expected key、ItemCheck start-use 与 channel-continuation gates，以及 animation/pending-reuse 局部转换。历史构建退出码为 0、0 warning / 0 error，产物路径为 `Build/bin/Terraria.Player.InputControl.Verification/Release/net10.0/`；历史运行输出 `PASS: tile release, channel expectations, and item-use gates preserve Version4 core rules`。该记录不覆盖后加的 frame-tail 边界。
- `PlayerTileInteractionReleaseSystem` 复用 `PlayerInteractionLockStateComponent`，按 Version4 `UpdateReleaseUseTile` 更新 tile release 与 3 tick lock；gamepad lock 只提交该组件的两个状态，没有执行 `PlayerInput.LockGamepadTileUseButton` 外部效果。
- `PlayerInputCommand` 现在只含 P10 的 player slot 与 raw control facts。Packet 13 的 `direction` 属其他 owner，`Sequence` 的排序/重放策略仍由未接入的 Protocol Adapter 决定；不在 Raw Input System 中验证后静默丢弃。
- 未使用的 `InputIntentComponent.ReleaseUseItem` 与 `ReleaseUseTile` 镜像字段已移除。没有接入生产 ItemUse 或 Tile writer；新增 tile-release writer 只作用于隔离核心。Version4 的 ItemCheck、tile attempt、lock timer、`mouseInterface` 与 mount 分支仍需要各自 owner/integration review。
- `PlayerInputSyncVerification` 覆盖 cache projection、raw control 提交、inactive player 拒绝批次无部分写，以及有效批次提交。`PlayerReleaseRepeatVerification` 覆盖 reset、raw-control 到 release/repeat 的组合、显式 hover 意图与 repeat timers。`PlayerItemUseExecutionVerification` 覆盖 ItemCheck 入口 gate 的状态变化与 early return。
- `Terraria.Player.InputControl.Verification` 此前运行记录覆盖既有 tile/channel/ItemUse 核心断言、air/default/occupied item、animation 未结束、release edge、正/零/负 itemTime frame-tail，以及 SetMatch 单次和 PlayerFrame 顺序组合边界断言。完整参考项目的额外路径、ItemUse 生产消费点、唯一生产 writer 与 schedule 未覆盖。
- 历史记录中一次 Release verifier 构建曾遇到 `MountDefinition` namespace 错误；随后两个真实 verifier 项目重新构建通过。本轮没有修改这三个 mount 文件。
- 此前只运行约 10% 的核心切片：focused verifier build 退出码 0、0 warning / 0 error；run 退出码 0，输出 `PASS: tile release, channel expectations, item-use gates, SetMatch, and PlayerFrame composition preserve Version4 core rules`。此次文档同步没有重跑该 verifier。全量 P10、production runtime 与行为等价仍 not-run，focused verifier 不代表迁移成功。

### 历史验证记录（不属于本次交付）

以下命令是此前记录的历史执行内容，不是本次交付执行；本次不调用 `dotnet`、`Invoke-SerialDotnet.ps1` 或任何编译/测试入口。历史记录曾通过仓库 wrapper 串行执行，`PATH` 首位为 `D:\TRbackup\dotnet-sdk-10.0.400`（`dotnet --version` 为 `10.0.400`）：

本轮使用 `D:\TRbackup\dotnet-sdk-10.0.400`，经 `Build/Tools/Invoke-SerialDotnet.ps1` 串行构建并运行 focused verifier。

```powershell
$env:PATH = 'D:\TRbackup\dotnet-sdk-10.0.400;' + $env:PATH
$dotnetArguments = @(
  'build', '.\src\NSSLC\Component\PlayerInputSyncVerification\Terraria.PlayerInputSyncVerification.csproj',
  '--configuration', 'Release', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
$dotnetArguments = @(
  'run', '--project', '.\src\NSSLC\Component\PlayerInputSyncVerification\Terraria.PlayerInputSyncVerification.csproj',
  '--configuration', 'Release', '--no-build', '--no-restore'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments

$dotnetArguments = @(
  'build', '.\src\NSSLC\Component\PlayerReleaseRepeatVerification\Terraria.PlayerReleaseRepeatVerification.csproj',
  '--configuration', 'Release', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
$dotnetArguments = @(
  'run', '--project', '.\src\NSSLC\Component\PlayerReleaseRepeatVerification\Terraria.PlayerReleaseRepeatVerification.csproj',
  '--configuration', 'Release', '--no-build', '--no-restore'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments

$dotnetArguments = @(
  'build', '.\src\NSSLC\Component\PlayerItemUseExecutionVerification\Terraria.PlayerItemUseExecutionVerification.csproj',
  '--configuration', 'Release', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
$dotnetArguments = @(
  'run', '--project', '.\src\NSSLC\Component\PlayerItemUseExecutionVerification\Terraria.PlayerItemUseExecutionVerification.csproj',
  '--configuration', 'Release', '--no-build', '--no-restore'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments

$env:PATH = 'D:\TRbackup\dotnet-sdk-10.0.400;' + $env:PATH
$dotnetArguments = @(
  'build', '.\Test\Terraria.Player.InputControl.Verification\Terraria.Player.InputControl.Verification.csproj',
  '--configuration', 'Release', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
$dotnetArguments = @(
  'run', '--project', '.\Test\Terraria.Player.InputControl.Verification\Terraria.Player.InputControl.Verification.csproj',
  '--configuration', 'Release', '--no-build', '--no-restore'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
```

| 验证 | 退出码 | Warning / Error | 结果 |
| --- | --- | --- | --- |
| `Terraria.PlayerInputSyncVerification.csproj` historical build | 0 | 0 / 0 | 历史产物路径：`Build/bin/Terraria.PlayerInputSyncVerification/Release/net10.0/Terraria.PlayerInputSyncVerification.dll`；不代表当前源码快照已构建。 |
| Player input historical focused verifier | 0 | 不适用 | 历史输出：`PASS: player input sync projection and raw control commits preserve accepted/rejected state`。 |
| `Terraria.PlayerReleaseRepeatVerification.csproj` historical build | 0 | 0 / 0 | 历史产物路径：`Build/bin/Terraria.PlayerReleaseRepeatVerification/Release/net10.0/Terraria.PlayerReleaseRepeatVerification.dll`；不代表当前源码快照已构建。 |
| Player release/repeat historical focused verifier | 0 | 不适用 | 历史输出：`PASS: raw controls project into Version4 release and repeat semantics`。 |
| `Terraria.PlayerItemUseExecutionVerification.csproj` historical build | 0 | 0 / 0 | 历史产物路径：`Build/bin/Terraria.PlayerItemUseExecutionVerification/Release/net10.0/Terraria.PlayerItemUseExecutionVerification.dll`；不代表当前源码快照已构建。 |
| Player ItemCheck entry-gate historical focused verifier | 0 | 不适用 | 历史输出：`PASS: ItemCheck entry gate preserves Version4 crowd-control early return ordering`。 |
| `Terraria.Player.InputControl.Verification.csproj` historical build（CompleteFrameTail 扩展之前） | 0 | 0 / 0 | 历史产物路径：`Build/bin/Terraria.Player.InputControl.Verification/Release/net10.0/Terraria.Player.InputControl.Verification.dll`；不代表当前源码快照已构建。 |
| Player tile-release / channel-value / ItemUse local-transition historical verifier（CompleteFrameTail 扩展之前） | 0 | 不适用 | 历史输出：`PASS: tile release, channel expectations, and item-use gates preserve Version4 core rules`；不包含当前 frame-tail 断言。 |
| 此前 `Terraria.Player.InputControl.Verification.csproj` build（含 SetMatch 单次与组合断言） | 0 | 0 / 0 | 使用 `D:\TRbackup\dotnet-sdk-10.0.400` 与串行 wrapper；产物位于 `Build/bin/Terraria.Player.InputControl.Verification/Release/net10.0/Terraria.Player.InputControl.Verification.dll`。 |
| 此前 focused verifier run（含 SetMatch 单次与组合断言） | 0 | 不适用 | `PASS: tile release, channel expectations, item-use gates, SetMatch, and PlayerFrame composition preserve Version4 core rules`。 |
| Packet 13 route Query 更新后的 `Terraria.Player.InputControl.Verification.csproj` build | 0 | 0 / 0 | 目标项目；产物 `Build/bin/Terraria.Player.InputControl.Verification/Release/net10.0/Terraria.Player.InputControl.Verification.dll`。这是此前隔离实现轮次的记录，本次文档工作未重跑。 |
| Packet 13 route Query 更新后的 focused verifier run | 0 | 不适用 | `PASS: tile release, channel expectations, item-use gates, SetMatch, PlayerFrame composition, and Packet 13 routing preserve Version4 core rules`。只覆盖隔离核心，不证明生产接线或迁移成功。 |

| 自检项 | 结果 |
| --- | --- |
| P10 11 个叶子组是否各自有目标职责映射 | 已在设计文档逐组映射 |
| 现有隔离核心是否被误称为已接入 System | 否；production integration 仍 blocked，implementationStatus 保持 partial |
| 是否把完整参考项目等同 Version4 行为 | 否；它只作辅助佐证，Packet 13 与 ItemCheck 差异仍以 Version4 为目标事实 |
| release/repeat 投影是否吸收参考项目独有行为 | 否；未加入完整参考项目额外的 `releaseLeft/right` 写入和 flexible tile wand 偏移副作用 |
| ItemCheck 局部 gate 是否被误称为完整执行路径 | 否；完整同步 ItemCheck、旧 facade route 与 effects 仍未迁移 |
| CPG 零命中和 SourceSnapshotId 缺失是否被升级成负向证明 | 否；保留 partial / unknown |
| 未知 Owner、顺序、协议和生命周期是否保留缺口 | 是；均有 gate 或 integration-review 标记 |
| 当前新增 frame-tail 断言是否已运行 | 是；与当前 focused verifier 一起通过 |
| reuse-delay 局部转换是否有 focused 断言 | 是；当前 focused verifier 一起通过 |
| SetMatch 映射和 PlayerFrame 顺序组合是否有 focused 边界断言 | 是；Head 201、Body 166、Body 81、Legs 83 与 body -> legs -> head 组合场景通过；完整 switch 组合仍未验证 |
| 当前证据是否覆盖完整 P10 或证明迁移成功 | 否；Packet 13 完整 Adapter、ItemUse effects、Selection、Tile 外部 writer、channel lifecycle 与 production schedule 仍 not-run/unknown |

本计划记录 P10 核心边界拆分和后续工作包；`CompleteFrameTail`、`ApplyReuseDelay`、`PlayerSetMatchQuery`、`PlayerSetMatchCompositionQuery` 与 Packet 13 route Query 均仍是隔离核心，没有接入 production ItemCheck、PlayerFrame 或 Packet 13 writer。本次文档核对没有运行构建或测试；此前约 10% focused verifier 仅证明其断言覆盖的局部核心，整体 `verificationStatus` 保持 `partial`，全量 P10 行为验证为 `not-run`。Version4 `ApplyReuseDelay` 方法体为空壳，完整参考仅支撑部分 proposed 规则；Packet 13 身份、唯一 owner、ItemUse effects、tile writer、调度顺序与生命周期缺口保持 `unknown`，不能称为迁移成功。
