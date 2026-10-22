# P07 生产实现进度记录

日期：2026-10-15；续接更新：2026-10-16（Asia/Shanghai）  
范围：`07-NetModule社交与事件计划.md` 的 P07 方向；本记录不推进总控a根目标。

> 说明：本文前半段保留各增量当时的历史验证结果；文末的“continuation final serialized
> build”是 2026-10-15 continuation 的最后一次串行构建记录，不覆盖其后的 2026-10-16
> 静态源码增量。当前 Invasion token 与 Angler/WorldEvent/Emote request replay continuation
> 均尚无本轮构建或运行证据；下文历史 PASS 不覆盖这些增量。

## 本次交付

- 保留并复核现有 P07 自有切片：module 1 Text/Party chat、module 3 Ambience、module 4 Bestiary、module 5 CreativePowers、module 6 CreativeUnlocksPlayerReport、module 8 Particles、module 9 CreativePowerPermissions、module 10 Banners，以及对应的有界 codec、状态 owner、快照和 outbound intent producer。
- 新增 `Modules/ModuleRegistrationProposal.cs`：按 Version4 `NetworkInitializer` 固定顺序给出 0–14 的数据型接入提案，明确 P07 自有模块与 P02/P03/P04/P05/P06 委托模块；不冒充总控运行时 registry。
- 新增 `Modules/Events/WorldEventCoordinator.cs`：P07 单写入的 Party、Crystal、Invasion progress、Tower shield 事件事实；使用显式请求/外部 owner commit、`CommitVersion`、epoch+sequence 重放窗口、阶段/世界前置、范围校验和 typed effect。该类不访问时钟、文件、网络、玩家/NPC/Tile 全局，也不直接写 socket。
- 补齐 `Modules/Events/WorldEventCoordinator.Restore`：恢复时校验 crystal/invasion/tower 的边界并保留 P05 source commit version，避免重启后旧战斗提交重新覆盖状态。
- 新增 `Modules/Events/AnglerQuestState.cs`：按 P03 world-day key 管理 quest 发布与玩家名完成 ledger，保留 74/75 typed effect、epoch+sequence replay、断线 epoch 清理、snapshot/persistence 映射和旧 day-source version 拒绝；不读取玩家/世界/文件全局。
- 修正 `Modules/ModuleDispatcher.cs`：在进入 replay cache 前先检查 envelope 阶段，避免同一 epoch+sequence 在不同阶段复用已缓存成功结果。
- 新增 `Modules/Social/Chat/ChatInputBudget.cs` 并接入 `ChatCommandProcessor`：命令 id 64 字节、正文 4096 字节的严格 UTF-8 预算；非法 UTF-8/超限返回结构化 `MalformedFrame`，不进入命令 owner。
- SC081 验证器继续只走一条合法 Party chat 流程，并额外确认 P07 的 15-module 顺序提案完整。

## 验证证据

环境使用根 `global.json` 选择的 .NET SDK `10.0.400`。

| 验证 | 命令 | 结果 |
| --- | --- | --- |
| Modules 定向构建 | `dotnet build src/MinimalImplementation/Modules/MinimalImplementation.Modules.csproj --configuration Debug --no-restore --verbosity minimal` | exit 0；0 warning；0 error；DLL：`Build/bin/MinimalImplementation.Modules/Debug/net10.0/MinimalImplementation.Modules.dll` |
| SC081 验证项目基线构建（历史） | `dotnet build src/MinimalImplementation/Verification/Modules/MinimalImplementation.Verification.Modules.csproj --configuration Debug --no-restore --verbosity minimal` | 先前基线 exit 0；本轮重新构建 exit 1，阻断为 `World/WorldLiquidSettle.cs:10,32,45` 的 `CommitVersion` 命名空间错误；不能把旧 DLL 当成本轮构建结果 |
| SC081 | `dotnet Build/bin/MinimalImplementation.Verification.Modules/Debug/net10.0/MinimalImplementation.Verification.Modules.dll --scenario SC081 --output-root Build/diagnostics/MinimalImplementation/P07` | exit 0；`PASS`；证据：[SC081-result.md](../../../../Build/diagnostics/MinimalImplementation/P07/SC081-result.md) |

此前增量回归曾覆盖 Modules、Verification/Modules 和 SC081；本轮 Emoji/Emote 改动后 Modules 构建为 0 warning/0 error，Verification/Modules 重建被 World 依赖阻断，SC081 仅使用现有 verifier DLL 做回归。新增 Angler/WorldEvent restore 与本轮 Social projection 目前只有编译覆盖，未把 SC081 扩展成未批准的 SC082–090 运行场景。

SC081 的实际断言是：Active 已认证 sender 的 module 1 C2S wire 只含 `Party` command id 与文本；服务器从 transport sender 绑定作者；S2C module 1 使用真实 author/NetworkText/RGB；sender 与同队可见目标收到 intent，异队目标被排除；结果只证明 `OutboundPacketIntent` 已生成，不声称 socket 已送达或客户端已处理。

## 未运行与剩余接入门

- SC082–SC090 均未运行，未扩大 10% 预算；没有启动服务器、真实 TCP 或客户端。
- `ModuleRegistrationProposal` 尚未接入总控最终 registry；`InboundPacketEnvelope`、`OutboundPacketIntent`、`SendReceipt` 和 Host/ServerComposition 仍由总控/P01统一维护，本方向未修改这些共享文件。
- `WorldEventCoordinator` 的 typed effects 仍需总控把普通包 78/101/111/113/143 等接到最终出站 producer，并由 P03/P05/P08提供真实环境、实体、战斗和存档合同；本次只完成 P07 owner 边界与静态可构建切片。
- module 0/2/7/11/12/13/14 的实际行为仍分别由 P03/P02/P06/P04/P05/P06/P03 等方向负责；P07没有复制它们的状态，也未把空 codec 或转发记录标成完成。
- 31 个普通包的其余算法迁移、断线/重同步和非空状态存档仍需后续批次；本次交付不宣称 P07 全方向完成。

## 本轮增量：Emoji/Emote projection（2026-10-15）

- 新增 `Modules/Social/EmoteCoordinator.cs`，覆盖普通包 120 `Emoji` 的 P07 自有短期投影：使用完整同版 `EmoteID.Count=151` 边界，忽略客户端 claimed player 并绑定认证 slot，要求 `Active` 阶段和有效 epoch/sequence。
- 服务端成功请求生成 Packet 91 `SyncEmoteBubble` 的 Packet 91 玩家锚点（`anchorKind=1`、认证 player slot、360 tick、emote），并按 P02 已过滤的可见目标去重；不访问 NPC/玩家/世界全局，不执行 NPC reaction，不直接写 socket。
- 增加 epoch+sequence replay、151 范围无效果兼容、最多4096个活动气泡、360 tick 删除、断线清理和重连当前投影重同步；拥有气泡的连接断开时为其他剩余目标生成 Packet 91 removal intent。
- 新增 `Modules/Social/SocialPresentationProducer.cs`，覆盖普通包 58 `InstrumentSound` 与 152 `ItemUseSound` 的有限 S2C relay：认证 sender rebinding、Active 阶段、finite pitch、可见目标去重和 sender 排除；只编码 Packet body，不调用音频/UI 或读取选中物品。
- 仅修改 P07 Modules 项目 Compile Include 和上述自有文件，未修改共享 Contracts、Host、总 registry、Infrastructure 或其他方向源文件。

### 本轮验证

| 验证 | 命令/证据 | 结果 |
| --- | --- | --- |
| Modules 增量构建 | `dotnet build src/MinimalImplementation/Modules/MinimalImplementation.Modules.csproj --configuration Debug --no-restore --verbosity minimal` | exit 0；0 warning；0 error；`Build/bin/MinimalImplementation.Modules/Debug/net10.0/MinimalImplementation.Modules.dll` |
| Verification/Modules 构建尝试 | `dotnet build src/MinimalImplementation/Verification/Modules/MinimalImplementation.Verification.Modules.csproj --configuration Debug --no-restore --verbosity minimal` | exit 1；3 个既有边界错误：`World/WorldLiquidSettle.cs:10,32,45` 找不到 `CommitVersion`；本轮未修改该 P03/World 文件 |
| SC081 回归 | 使用现有 `Build/bin/MinimalImplementation.Verification.Modules/Debug/net10.0/MinimalImplementation.Verification.Modules.dll`，并在 Modules 增量构建后执行固定 SC081 | exit 0；PASS；仅证明既有 Party chat intent 回归，不等同 Verification 项目本轮重新构建，也不证明 SC089 |

本轮仍未运行 SC082–SC090、真实服务器、TCP、客户端或 P07 最终 registry；Emoji/Emote 代码只有 Modules 编译覆盖，SC089 仍按固定测试预算保持未运行。

## 2026-10-15 continuation：dirty/receipt 与 owner retirement contract

- 新增 `Modules/ModuleDelivery.cs`：校验 projection intent batch 的统一 commit version、目标
  epoch、receipt 数量与唯一 sequence；空 batch 只接受零 receipt。`Queued` 和
  `LocallySent` 才能本地 settle，`Failed`、`OutcomeUnknown`、`NotSubmitted` 保持 dirty；该
  contract 明确不是客户端 acknowledgement。
- `BestiaryState`、`BannerState` 新增按目标 epoch 的 dirty capture、完整 receipt collection
  settlement、版本水印和 `RetireEpoch`；Bestiary 多 intent batch 必须整批 settle 后才推进
  watermark，Banner full-state batch 只有一个 intent。两者 handler 均实现
  `IModuleEpochRetirable`。
- `WorldEventCoordinator` 与 `AnglerQuestState` 的既有 replay cleanup 现在也声明
  `IModuleEpochRetirable`。它们仍是非 handler owner，当前 Host close path 尚未调用；Emote 的
  disconnect cleanup 现在通过 owner-held pending removal 与 `CapturePendingRemovalDelivery()`
  交付，不能被降级成丢弃 intent 的 void 调用。生产生命周期接线仍待 G2。

### Continuation final serialized build

| 项目 | 命令 | 结果与输出 |
| --- | --- | --- |
| Creative modules | `dotnet build src/MinimalImplementation/Modules/Creative/MinimalImplementation.Modules.Creative.csproj --configuration Debug --no-restore --verbosity minimal` | exit 0；0 warning；0 error；`Build/bin/MinimalImplementation.Modules.Creative/Debug/net10.0/MinimalImplementation.Modules.Creative.dll` |
| P07 modules | `dotnet build src/MinimalImplementation/Modules/MinimalImplementation.Modules.csproj --configuration Debug --no-restore --verbosity minimal` | exit 0；0 warning；0 error；`Build/bin/MinimalImplementation.Modules/Debug/net10.0/MinimalImplementation.Modules.dll` |
| Server | `dotnet build src/MinimalImplementation/MinimalImplementation.Server.csproj --configuration Debug --no-restore --verbosity minimal` | exit 0；0 warning；0 error；`Build/bin/MinimalImplementation.Server/Debug/net10.0/MinimalImplementation.Server.dll` |
| Modules verifier | `dotnet build src/MinimalImplementation/Verification/Modules/MinimalImplementation.Verification.Modules.csproj --configuration Debug --no-restore --verbosity minimal` | exit 0；0 warning；0 error；`Build/bin/MinimalImplementation.Verification.Modules/Debug/net10.0/MinimalImplementation.Verification.Modules.dll` |

SC081 的既有 PASS 证据保留；本 continuation 未运行 SC082–SC090、服务器、TCP 或客户端 ack
场景。当前仍未闭合的事实是：Host 普通 outbound path 未把 `SendReceipt` 回传给上述 P07
owner API，且非 handler owner 的 epoch cleanup 尚未接入 Host close callsite；详见
`Build/diagnostics/MinimalImplementation/P07/p07-closure-audit-20261015.md`。

## 2026-10-16 Emote removal receipt continuation（静态检查；未构建）

- `EmoteCoordinator.AdvanceTicks` 与 `RetireEpoch` 现将 Packet 91 anchor 255 removal 保留在
  owner 的 pending 集合；活动与 pending projection 共用 4096 项上限。`CapturePendingRemovalDelivery()`
  产生按目标拆分、带 `OutboundDeliveryToken` 的单目标 intent 与唯一 in-flight settlement。
- 仅 `Queued` / `LocallySent` 清除对应目标；失败、未提交、未知或缺 receipt 保留，可在前批
  settle 后重新 capture。来源更正（批次18）：指定 MessageBuffer.cs:2655 的无条件break
  使按bubble ID删除的代码不可达，不能据此证明客户端清理或未知重试幂等性，相关行为及
  来源等价仍open。待删除ID不参与分配，避免重试窗口内的迟到清理删掉新bubble。
- 修正了 `CreateResyncIntents` 的超 100 列行。静态格式扫描无超列宽/tab/尾随空白，API 文本
  搜索仍无 Emote 生产调用点。此前 2026-10-15 的 Modules 构建早于本续接，不能作为当前源码
  构建证据；本续接未构建、未运行验证，G2 仍须接入 tick/disconnect/pump 并验证。

## 2026-10-16 Invasion presentation delivery token continuation（静态检查；未构建）

- `InvasionEffectDeliverySettlement` 现将每条已绑定的多目标 presentation intent 展开成
  按效果顺序、目标 epoch 升序排列的单目标 intents；保留原 packet、payload 和 commit version，
  并附带同一 batch 的 `OutboundDeliveryToken(owner/batch/target/index)`。没有有效目标的效果
  仍由原 binding 表示，不产生零目标 intent；`Started` 仍是本地效果，不纳入网络批次。
- 单目标 token 使 Host 的无 receipt 回退能为每个未提交目标构造合法 `NotSubmitted` receipt；
  单目标发送也避免一个断线目标阻断同广播中其他目标的 receipt 对应关系。
- settlement 按 token 和原 intent 引用/顺序校验每个目标的 receipt。单目标的零 receipt
  明确视为未提交并保持 unsettled（覆盖 delivery pump 队列拒绝/无连接返回空集合的路径）；
  多余或错配的 receipt 仍拒绝。只有 `Queued` / `LocallySent` receipt 的 `(epoch, sequence)`
  参与唯一性校验；失败、未提交及未知结果保持 unsettled，允许这些结果使用 transport
  未分配/不可靠的 sequence 值。
- 未修改 Host/Contracts/Composition；当前仍无 Invasion 生产调用点。Host 必须保留首次
  Applied 结果及其 settlement/intents，处理未结算批次重试，并接入真实目标和 packet-7 同版
  world snapshot；此改动不构成效果 outbox、自动重试或生产闭环。
- 静态复核：检查 `Version4InboundRouter.CreateNotSubmittedReceipt`、连接发送回执与
  `ServerOutboundDeliveryPump` 的结果形态；只对本文件执行列宽/tab/尾随空白扫描，无命中。
  未构建、未运行 verifier、DLL、服务端、网络或场景；G2 仍负责串行构建与验证。

## 2026-10-16 request replay continuation（静态检查；未构建）

- `AnglerQuestState` 的 Publish/RecordFinished 与 `WorldEventCoordinator` 的
  ToggleParty/StartCrystal/SkipCrystalWait 现在同时保存原请求与结果。同 epoch/sequence
  重用时，比较 sealed record 的具体类型和全部字段；改变操作、Stage、day key、任务字段、
  玩家名或坐标返回 `Conflict`，不替换原缓存、不再次提交领域状态。
- `EmoteCoordinator` 缓存 Stage、claimed slot、authenticated slot、emote 四个不可变输入
  标量；任一变化返回 `Conflict`。`VisibleTargets` 是首次执行时捕获的路由事实，不属于
  请求重放身份；同一请求重放保留原 intent/targets，不持有请求的可变目标集合。
- 三个 owner 的 `AsDuplicate()` 均保留原 `Rejected`；Emote 还保留原 `Ignored`。
  成功重放保留原 commit version 和 effect/intent，不重新推进 owner。独立传入的
  `WorldEventEnvironment`、worldLoaded/canSkip 只在首次执行时参与判断，重放不重评环境。
- 保留既有 4096 项缓存与 epoch last-sequence 游标；缓存淘汰后的旧序号仍拒绝。
  `RetireEpoch` 清理缓存和游标，不能替代 Host 每次准入的 live epoch/current stage 检查。
- 静态复核确认三个 owner 的 6 个入口和 27 个 Remember 调用均携带原 request；源码哈希、
  搜索命令和范围详见
  `Build/diagnostics/MinimalImplementation/ParallelResume-20261016/P07-handoff.md` 批次12。
  格式扫描仅命中 Angler 原有第409行（106列）和第418行（102列），无 tab/尾随空白；
  WorldEvent/Emote 无超100列/tab/尾随空白。本批未运行构建或行为测试。

当前生产接线仍开放：Angler 缺读档/P03 day fact 初始化及按认证玩家名生成 Packet74；
WorldEvent 已有 composition 实例与 close retirement，但本次文本搜索未发现上述三个请求
入口的生产调用；Emote 缺 tick/disconnect/pump 接线。Bestiary/Banner 的
`ModulePeriodicSyncOwner` 已消费 capture API 并交 pump，默认九阶段仍未装配调用它。
Creative module6 已接真实 typed report；module5 的 Host 仍直接编码 `effect.Message`，
尚未消费 `CaptureBroadcastMessages()` 的 action0 展开及 Button/PerPlayerSlider 不广播语义。
Invasion 首次 Applied 效果保留、失败/未知/部分成功重试、P05 四 owner 独立结算和真实
roster-wide `QualifiedPlayersRevision` 仍待 G2/对应来源 owner 接线。

本批没有启动 dotnet、编译器、DLL、服务端、网络或场景；SC082–SC090 未运行，SC081 未扩充。
截至该批回读，G2 P06 build-10-green / SC071 run-09-green 只覆盖其 P06 局部断言，不覆盖本批源码；
Server/Integration 未重跑，SC091 run-01 仍为 FAIL。P07 总目标保持 active，不宣称生产闭环。

## 2026-10-16 Invasion pending retry continuation（批次13；静态检查；未构建）

- `InvasionEffectDeliverySettlement` 现在由原实例保留首次 Applied result、原 bindings 和
  单目标 intents，并以原 intent 索引记录 pending。整批报告校验通过后，仅移除有
  `Queued` / `LocallySent` 回执的目标；空回执、Failed、NotSubmitted、OutcomeUnknown
  继续 pending，后续捕获不再包含已确认成功的项。
- 原实例及其 `Intents` 始终表示首批，只入队一次。前批报告结果后，
  `CapturePendingDelivery()` 才能返回新的不可变 settlement handle，包含尚未完成的项，
  按原索引保留效果/目标顺序，使用新 batch ID/token index。每个返回 handle 也只入队一次；
  保留原 packet、payload、target 和各自 commit version，不重读领域状态或重跑命令。
- 当前批引用检查先于释放 in-flight；旧批次或重复结算不能释放新批次。当前批的非 null
  terminal report 即使非法也释放该批次供重捕获，但校验完整列表、intent 身份/顺序、token、
  target、receipt 后才统一移除成功项，后部非法不会造成前部成功项被部分清除。
- 实现 `IModuleEpochRetirable`；`RetireEpoch` 仅移除该 epoch 的 pending 项，迟到回执
  不会重新加入目标。已捕获的在途 intents 保持不可变，退休不代表取消其网络投递。
  捕获、结算、退休均要求在 server commit 边界执行。
- 静态反证覆盖部分成功、非法报告、旧批次、退休和混合 commit version 的源码分支；
  这是源码审阅，未执行行为测试。保留原 settlement 和共用的 `CreateTargetIntent`，
  不新增通用 retry/outbox。`ModuleDeliveryReceiptEvaluator` 要求统一版本，不能直接用于
  packet7/P03 version 与其他 Invasion version 混合的效果批次。
- 最终源码 371 行，列宽超100/tab/尾随空白共0项；2026-10-16 06:12:54 +08:00
  回读 SHA256 为 `D0B9A05A5A18192E59636086AE9577ED563A5FAD87FEBF079A79DDC5EEBC3E6B`。
  批次12三个 owner 哈希未变。实际静态命令、行号和限制已写入
  `Build/diagnostics/MinimalImplementation/ParallelResume-20261016/P07-handoff.md` 批次13。

G2 仍须在 Host 保留该原对象，首批只入队一次，消费 retry capture，并接入断线退休、真实
目标、packet7 同版 world snapshot 及 roster-wide revision/current guard。重试时机、退避、
上限与 OutcomeUnknown 的处理策略尚待接线；未知结果重投仍可能重复展示，各批次内部顺序
不保证失败重试后的绝对网络时序。本地成功回执也不证明客户端已收到或完成展示。

截至本批回读，G2 已另记 P06 build-11 通过、SC071 run-10 FAIL，随后收到 verifier FIX READY，
build-12-green/run-11-green 尚待记录；前述 run-09-green 仅是历史局部结果。这些窗口均不
覆盖当前 P07。本批没有运行 dotnet、编译器、DLL、服务端、网络或场景，未扩充 SC081，
SC082–SC090 未运行；Server/Integration 与 SC091 未重跑。构建及运行验证仍由 G2 串行执行，
P07 总目标保持 active。

## 2026-10-16 Creative 权限快照生命周期（批次14；静态检查；未构建）

- 唯一生产改动为 `Modules/Creative/CreativePowerStateOwner.cs`。沿用 Bestiary/Banner
  的 owner-held capture 模式，按目标 epoch 保存确切的权限批次引用；同目标在途时不再
  捕获第二批。已确认和在途 epoch 的并集计入原有255目标容量，退休释放目标记录。
- `AcknowledgePermissionSnapshot` 先校验当前 pending 引用。旧批、重复结算、手工构造
  或其他 owner 的批次返回 StaleVersion，不能释放新批或推进水位。当前批的非 null 结算
  释放 pending，再执行原版本、内容和 receipts 校验；失败、未知、空或非法回执不推进
  确认版本，允许后续重捕获。只确认捕获版本，期间的新提交继续 dirty。
- `RetireEpoch` 同时删除权限 pending 和确认版本，退休前的迟到回执不能重新挂回水位。
  未改15条权限 wire entries、原 receipt evaluator 或领域状态提交；effect delivery 是
  独立路径，本批未修改，也不据此声明其生命周期闭合。
- 源码分支审阅核对了重复 capture、失败后重捕获/旧批迟到、期间版本推进、退休后迟到、
  外来批次与容量释放；未执行这些行为。最终源码708行，2026-10-16 06:22:39 +08:00
  回读 SHA256 为 `B84D876AEF2A7CDDA8D4976A8B31D1E190800FF2773AB87900D7DE8D003D2600`。
  改动块格式问题0项；全文件仅原有605行101列，无 tab/尾随空白。
  关键行号、实际静态命令与限制见
  `Build/diagnostics/MinimalImplementation/ParallelResume-20261016/P07-handoff.md` 批次14。

生产接线仍开放：Host module9 wrapper 只处理入站拒收；尚未发现权限 capture/ack 的 Host
消费或 Creative owner retirement 的生产调用。G2 须在 server commit 边界接 capture、发送、
结算与关闭退休，并在每次调用前检查 live epoch。当前 ACK 接口仅接裸 SendReceipt，未校验
具体 intent/token/payload 关联，调用方必须保留确切批次与对应发送回执。owner 的退休清理
不能替代当前会话准入，也不禁止调用方重新捕获已退休的 epoch。

G2 最新独立证据补记：P06 Verification/Mechanisms build-12-green 于06:12:28–06:12:33
完成，exit0、0 warnings/errors；SC071 run-11-green 于06:12:45–06:12:47完成，exit0/PASS。
结果 `P06/SC071-run-11-green/SC071-result.md` SHA256 为
`27A3931B27D6E9B096CB25F3D22799C14CBCA9F2FA5E65CE0549655E5EB18381`。
该窗口只修正 inactive tile 协议投影断言，复用 Mechanisms build-11；不覆盖当前 P07。
批次13的“尚待记录”保留为当时观察，run-10 FAIL 仍保留；Server/Integration、SC091未重跑，
SC091 run-01仍为严格FAIL。本批未运行构建、DLL、服务端、网络或场景，SC081未扩充，
SC082–SC090未运行；G2仍独占串行构建/运行验证，P07 goal保持active。

批次14后续静态审阅（未新增生产修改）：Creative effect delivery 的退休边界仍有独立缺口。
`CreateEffectDeliveryBatch` 只构造批次，`AcknowledgeEffectDelivery` 只检查版本和回执，未验证
owner捕获身份或退休后的批次有效期。源码反例为：创建含目标E的批次A，退休E，再提交A的
完整成功回执；仍可走到 `_acknowledgedEffectVersions[E] = batch.Version`。这不是运行重现。
生产Host在 `CompleteModuleHandlers.cs:97` 创建批次，在
`Version4InboundRouter.cs:607,624,629` 分开发送和commit结算；当前既缺Creative退休调用，
结算处也未见target live校验。仅增加Host退休调用仍不足以消除上述owner反例。
effect允许多目标、多消息及多个提交版本；下一批须明确owner批次身份、失败重试和局部成功
语义，并保留原effect/intent关联，不能直接套用权限快照的“每目标只允许一批”。证据/哈希见handoff，
本轮没有改动这条路径或启动运行验证。

## 2026-10-16 Creative effect 批次归属与退休生命周期（批次15；未构建）

本批实施上节的 owner 反例修复。生产变更限于 Modules/Creative 下的
CreativePowerStateOwner、CreativeModuleHandler、CreativePowerEffectDeliverySettlement，
另修正 CreativePowerEffectDeliveryResult 的重试契约注释；无新文件、项目或通用 outbox。

- owner 用标准库 ConditionalWeakTable 记录确切 batch 引用及其目标生命周期快照；
  手工构造、其他 owner 创建或已成功结算的 batch 无有效登记，返回 StaleVersion。
  这只验证 batch 是经该 owner 的 CreateEffectDeliveryBatch 登记；该入口仍信任调用方
  传入所属 owner 的已提交 effect 与正确编码，未新增 effect 来源或 payload 真实性证明。
- 当前目标生命周期表最多255个 epoch，capture 先完整检查容量再写入；多个批次和版本
  可同时在途。弱键不强持有被调用方丢弃的失败批次，也不承担失败批次保存或重试调度。
- RetireEpoch 同时移除 effect 水位和生命周期标记。同 epoch 后续重新 capture 使用新标记，
  旧批不能借此恢复有效性。全部目标退休返回 stale；混合批次中退休目标的空、Failed 或
  OutcomeUnknown 回执不阻止有效目标结算，但畸形关联仍拒绝整份报告。
- 生产 settlement 原样转交逐 intent 的 OutboundIntentDelivery，不再合并裸 receipts。
  owner 先验证确切 intent 引用/顺序、回执位置、目标和唯一 sequence，再在全部有效目标
  成功后统一写水位。token/版本/单目标约束沿用 batch 构造器与不可变 intent 契约。
  有效目标缺回执返回 NotSubmitted，不会借退休目标的空记录移动后续回执位置。
- 多版本交错时，旧批仍须凭自己的成功回执结算；水位取已确认值与本批版本的较大者。
  新版本成功不代表旧 effect 已发送。成功后移除 batch 登记，重复 ACK 返回 stale。
  有效目标失败/未知/缺失/非法报告仍整批重试；尚无逐目标成功保留或自动重试，重发已成功
  目标可能重复展示。退休仅改变 owner 结算资格，不撤销已交给网络的 intents。

以上为逐分支静态审阅，未执行行为测试。修改前后文本比较仅归一化 CRLF/LF 和文件末尾换行，
确认权限 capture/ACK、权限 receipt evaluator、领域 Apply/Snapshot 和 effect Commit 区域未变。
四份修改源码共新增136/删除124行；owner为760行，其余109/37/40行；列宽/tab/尾随空白0项。
2026-10-16 06:41:05 +08:00 最终源码 SHA256：

| Modules/Creative 文件 | SHA256 |
| --- | --- |
| CreativePowerStateOwner.cs | 63E267A7E556E7629A12F6171AE2C62E96E71AFF693385FF07833CEB097222C3 |
| CreativeModuleHandler.cs | F9517050D15F4EC6A48863C31C3B941463D13CEB1CB4A672B8B91D372B80F8DA |
| CreativePowerEffectDeliverySettlement.cs | 7BBC9497AA66C75FA1F2EA15CA18FE62F589CED95B22018164F93CE83A868184 |
| CreativePowerEffectDeliveryResult.cs | 75217D56CBFFE217123EC2CA127B53E2E6853B51E8C19B42B991F55AE730F6B5 |

具体源码行号、静态命令、哈希和未执行反例清单见 handoff 批次15。源码与 progress 仍未跟踪，
diagnostics 仍被忽略；未暂存或清理。本批未启动 dotnet、编译器、DLL、服务端、网络、交互或
任何场景；SC081未扩充，SC082–SC090未运行，构建及行为验证仍由G2串行执行。

G2生产接线仍开放：Host wrappers 未实现 Creative retirement；关闭路径仅调用既有 coordinator/
dispatcher。须在同一 commit 边界检查 live epoch、接入 RetireEpoch，保留原 effect batch 和
settlement，定义失败、未知和部分成功的重试时机/上限；不能重跑领域命令补发。
Host仍直接编码effect.Message，尚未消费CaptureBroadcastMessages的255条有序action1及
Button/PerPlayerSlider不广播规则。owner修复不代表这些Host行为已闭合。

G2独立状态补记：P06 build-13/SC071 run-12曾PASS；build-14通过后run-13为严格FAIL，
结果SHA256 `0A6D767616A9C5CA82955B153E43176A5A4DF83B3DB65562E42962AC48BD384A`。
随后verifier FIX的build-15-green（06:35:05–06:35:09，exit0，0w/0e）和run-14-green
（06:35:21–06:35:23，exit0/PASS）已入G2账本；本轮只读复核结果SHA256为
`F7B24048E5D5A85720520C604983059B745B0C9D8B53E2F643CC41FF033A6C73`。
这些P06窗口不覆盖本批P07；旧FAIL/PASS均保留。Server/Integration、SC091未重跑，
SC091 run-01仍严格FAIL，P07完整目标保持active。

## 2026-10-16 Creative effect 部分成功与重试句柄（批次16；未构建）

本批承接批次15，四份生产修改仍限于 Modules/Creative 下的 owner、handler、settlement
与 result；未修改 batch、Host、Contracts、Protocol、Composition 或其他方向源码。
批次15的“尚无逐目标成功保留”是历史状态，当前已由下述逐 intent 进度替代。

- `CreativePowerStateOwner.cs:10` 的 EffectDeliveryProgress 保留原始不可变 batch、捕获
  生命周期、pending 原始索引、当前 attempt 及其索引映射。任一保留的 attempt 经弱键表
  找到同一进度；owner 不替调用方保存已丢弃的工作，也不提供持久化 outbox。
- `SettleEffectDelivery:415` 只接纳当前确切 attempt。完整校验报告后，保留所有有效目标的
  Queued/LocallySent 成功项，失败、未知或缺失项继续 pending；后部非法关联使本报告
  不接受任何新成功，之前合法报告的成功不回退。同目标全部原始 intents 成功才推进水位，
  按 max 保留较新水位，较新 effect 的成功不能代替较旧 effect 自身的回执。
- `CapturePendingEffectDelivery:478` 在当前 attempt 已报告后，按原始索引捕获仍有效且
  未成功的 intents，保留原 effect/message/payload/version，使用新 batch ID 和连续 token
  index。旧 handle 迟到不能释放或结算新捕获；在途时再次捕获返回 null。
- evaluator（665行）记录首个未完成结果后继续检查后部；成功回执要求目标/sequence唯一，
  失败、未知、未提交回执允许共用 transport 占位 sequence0。HasQueuedIntents 跨 attempt
  保留较低确认级别，首轮只有 Queued 的项不会因其他项补发 LocallySent 而被提升。
- `CreativePowerEffectDeliverySettlement.cs:29,36` 公开 IsSettled 与
  CapturePendingDelivery；`CreativeModuleHandler.cs:87` 返回具体 settlement 类型，现有
  接口赋值仍源码兼容，受影响调用方须重建。Host 必须保存原具体 settlement，首次 handle
  与每个新 handle 各只提交一次，之后按重试策略在 commit 边界捕获；不能重放领域 Apply。

原始空 batch 仍须完成首次提交/结算，不能因 IsSettled 已为 true 而跳过。关联 settlement
报告会终结当前 attempt；裸 receipts 兼容 API 的非空数量错误在转换前拒绝，尚未终结
attempt，null 参数也直接抛出。RetireEpoch 使旧生命周期失效但不撤回网络中已有 intents，
Host 每次仍须核验 live epoch；同 epoch 再 capture 不能复活旧进度。

静态复核：四份源码新增178/删除46行，最终869/118/50/41行；列宽>100、tab、尾随空白
均0项。归一化换行后，Apply/Snapshot与权限capture/ACK、权限evaluator、领域Apply helpers/
Commit三个未改区域文本完全相同。多消息部分成功、后部非法、旧handle、退休、Queued跨次
确认及空batch均只做源码分支审阅，未执行行为测试。06:53:51 +08:00 再次读取源码哈希与
06:51:44记录一致：

| Modules/Creative 文件 | SHA256 |
| --- | --- |
| CreativePowerStateOwner.cs | 1D4A11C65C1D49017F3D56FA267B86ACB7288661009E3C2610C2C53C8BFFBBD6 |
| CreativeModuleHandler.cs | 0828B6E7CFEE75C3CD4CE232BDF86CE2722919CDB808DAB406932356436F5E02 |
| CreativePowerEffectDeliverySettlement.cs | 5C7784692428E9844C5F2A420893DE8F731679442CEF8774345BC487FE4E2328 |
| CreativePowerEffectDeliveryResult.cs | 885ACCC91781643A08934B2F96195BB3B1D1FF62D812A70BDF2A2085B421564E |

完整前后哈希、静态命令、未执行分支和 G2 保留root/补发调用示意见 handoff 批次16。
Host尚未保留Creative失败root或调用新capture；仍直接编码effect.Message，未消费
CaptureBroadcastMessages的action0展开及Button/PerPlayerSlider不广播语义，关闭路径也未
接Creative退休。未知结果重投可重复，多消息失败后补发仍可能改变客户端绝对观察顺序；
只保留原索引顺序不能恢复已经发生的网络顺序。重试时机、退避、上限仍由G2接线。

G2独立证据更新：P06 build-16两个项目均exit0/0w/0e，SC071 run-15于06:45:36–06:45:38
exit0/PASS，结果哈希已独立复核为
`C9660BE6F2FB559E163CFCB82A49A3E5BE3B9A87F281BC685B2E8AE1046E7524`，仅覆盖P06。
Server在06:38增量构建exit1/0w/1e，P04 WorldItemDropCoordinator.cs(336,5)的CS0118
阻断仍由G2账本保留；Host最终源码尚无成功编译，不能据旧DLL断言当前P07可编译。
Pylon另缺运行态owner的DownedPlantBoss捕获/revision guard；Modules搜索仅见
InvasionSnapshot保留existing.DownedPlantBoss，没有真实运行态更新/捕获入口，本批未实施。

本批未启动dotnet、编译器、DLL、服务端、网络、交互或任何场景，未扩充SC081，未运行
SC082–SC090；Server/Integration与SC091未重跑，SC091 run-01仍FAIL/open。
源码与progress仍未跟踪，diagnostics仍被忽略；未暂存、清理或强制加入。G2仍独占串行
构建/运行验证，Queued/LocallySent不是客户端ACK，P07完整goal保持active。

## 2026-10-16 Banner 缓存重放效果抑制（批次17；未构建）

本批三份生产修改仅限Modules的ModuleDispatchResult、ModuleDispatchReplayCache及
Banners/BannerModuleHandler，未修改BannerState领取规则、wire、Host、Combat或验证程序。
三份源码已在handoff中给出READY与哈希，暂不再改，供G2串行接收。

原dispatcher缓存避免再次调用owner，却返回含首次action4领取响应的结果，Host会把旧
intents再次发送。本批让Handled显式选择replayOutboundEffects:false；cache保存
ForReplay副本，首次仍返回原result。Banner采用该选项后，同一epoch/sequence/raw frame
命中返回同提交版本、零intents和空settlement，不再从缓存产生一次领取效果。
其他module默认true，保留现有返回引用与effects；原始帧冲突、sequence高水位、缓存淘汰、
异常前消费序号及退休逻辑未改。复用现有cache，没有增加Banner专用去重owner。

该保证不覆盖直接重调handler/state、以新transport sequence重发，或重复发送首次intents。
首次效果丢失时，重放不会恢复它；没有可靠outbox、自动重试或补偿。未知结果重投action4
仍可能重复发物品；计数dirty snapshot不能补发领取响应，不作客户端奖励恰好一次声明。
既有RetireEpoch释放缓存后，Host仍须拒绝旧会话。

逐分支源码审阅和修改前后内存文本LCS比较完成：三文件新增31/删除5行，最终128/114/96行，
列宽>100、tab、尾随空白均0项。07:04–07:05 +08:00再次读取最终SHA256：

| Modules文件 | SHA256 |
| --- | --- |
| ModuleDispatchResult.cs | 1778A41E6F8A0A2B9E62D25EB4BBCAAFE70D2CF1D496EE402EA14F11FF16FEB5 |
| ModuleDispatchReplayCache.cs | 42C3AAAD5A012382DD5DEB4FDF8C3CE51A0D75497259A3CF1B0446381276EB3C |
| Banners/BannerModuleHandler.cs | 105AB8EDE0D6A076C99713B99322D48022D6AF4BB01DC9AA8AA8920DAD18F57F |

完整前后哈希、实际静态命令、未执行反例、Host路径和工具错误见P07-handoff批次17。
G2须串行重建Modules及Verification/Modules，并执行既有SC081；公开Handled增加可选
参数，调用程序集须重建。固定SC081只覆盖Party chat，不能证明本批Banner行为；未扩充
该场景，未执行任何行为测试。

Plantera来源调查已明确：Combat/Npcs/NpcManager.Death.cs:49只允许NPC1/2/3，现有真实
死亡提交不能产生Plantera击败事实；Host范围未找到CapturePendingDeaths/AcknowledgeDeath/
NpcDeathRecord消费者。Modules只有InvasionSnapshot.cs:20保留加载值，Pylon实时owner与
revision guard仍缺；未添加任意bool入口伪装闭环。指定Version4的BannerSystem.cs:48
Deserialize为空接收方法，本批不据其证明action3/4同版语义，其他完整树的版本对应仍开放。

P06 SC071-run-16结果哈希本批独立复核匹配A1BC2648A2203DCB75320E4C865FC9668C78C3E990793AE1102B913248E2DB71，
该PASS只覆盖P06。G2的ServerTick新夹具修正仍未构建；P04历史CS0118、Host尚无最终源码
成功编译、SC091 run-01 FAIL/open和P07生产接线缺口继续保留。

本批未启动dotnet、编译器、DLL、服务端、网络、交互或任何场景，未扩充SC081，未运行
SC082–SC090。G2独占串行构建/运行；源码及progress仍未跟踪，diagnostics被忽略，
未暂存、提交、清理或强制加入，P07完整goal保持active。

## 2026-10-16 Emote 旧气泡寿命与来源更正（批次18；未构建）

本批唯一生产修改为Modules/Social/EmoteCoordinator.cs，新增10行、删除0行。112行新增
常量6，183–190行在新气泡编码成功后、加入活动集合前，将同owner epoch及认证玩家槽的
旧气泡RemainingTicks直接设6，新气泡仍360。claimed slot不参与匹配；没有新API、类型、
字典或框架。批次17三份冻结源码哈希未变；本批Emote源码READY，供G2串行接收，暂不再改。
READY只表示可审阅，不能记为构建、行为测试或生产闭环PASS。

指定来源链可达：Version4/Terraria/MessageBuffer.cs:2973的Packet120以whoAmI覆盖玩家槽，
2980行调用EmoteBubble.NewBubble(...,360)；指定EmoteBubble.cs:97调用OnBubbleChange，
142–158行对同一Player实体的其他气泡直接赋lifeTime=6。当前模型用epoch+认证槽表示
玩家绑定，真实身份与断线仍依赖Host。不能用Math.Min替换，因为旧寿命仅剩1也应重设6。

重放/冲突及阶段、槽、emote、targets、容量拒绝均在赋值前返回；_version.Next及编码也在
赋值之前，编码失败不会先调整旧气泡。现有序号消费/ID分配未改，不宣称所有异常下owner
完全不变。到期沿用AdvanceTicks:209、pending capture:242及settlement:373；resync:288
使用实际剩余寿命，未新增tick/disconnect/pump或NPC reaction装配。

尚未执行的行为清单：同玩家新请求使所有旧气泡设6；剩1也设6；其他玩家/epoch不变；
重复/冲突/拒绝不再调整；编码失败不先调整；累计6 tick后旧气泡进入现有清理而新气泡
剩354；resync使用当前剩余寿命。仅逐分支源码审阅，没有执行C#或新增/修改测试。

本批更正三处来源边界，既有实现保留：

- Emote Packet91：指定MessageBuffer.cs:2655无条件break，后面的按ID删除不可达。
  已更正上文“同版客户端幂等”声明，真实客户端删除及未知重投幂等性保持open；此证据
  限制也不能反推实际客户端一定不处理。Packet120服务端可达来源不能补齐客户端缺口。
- Lantern：指定LanternNight.cs:76的BossIsActive为默认false，79行NaturalAttempt为空，
  没有ToggleManualLanterns。现有boss扫描、1/14、预约/5..10冷却及手动切换对应
  D:/TRbackup/无任何删减通过编译的同名文件76–133行；两个树版本/行为等价未证明。
  指定树CheckMorning、LanternsCanStart外围条件、WorldClear及UpdateTime仍可单独引用。
- Creative：指定CreativePowers.cs:49/206/250/281的四类接收方法为空；127–145行的
  PerPlayerSlider才有ReadSingle、sender rebind、权限及原值保存。module5 lookup/委托
  与部分join编码不能证明全部接收/action1广播；module9 Deserialize:22仅返回new bool()。
  handoff批次9声明已收窄，兼容实现、权限策略及Host生产缺口保留，本批未改这些源码。

最终Emote源码728行；列宽>100、tab、尾随空白0项。纯JavaScript LCS比较修改前缓存与
当前文件，仅归一化换行及末尾换行，确认只新增上述10行。07:13两次及07:16 +08:00的
READY SHA256一致，为E3DC9381897A853AC940D51F493D4BEA7BABA2D9DF36CD25CCA1B4B9D82601E8；
修改前为6DF48BA2A76FED8A348A33B492F4D3C593554294A7F69D59888CCA4FEAABA480。
完整参考哈希、当前Host哈希、实际静态命令和未运行清单见P07-handoff批次18。

共享router在本批期间由FE980968…变为07:19末次只读快照的
4DEACD0DECC541DF433EC716869BB952A17713D9524EE1A105738985C717B434；对既有P02快照
的差异含loadout处理/发送拆分及停止观察输出准备。本批未审计P02行为，未修改共享源码。
该快照552–560行收集module结果，
589–594发送普通intents，602–630发送关联intents并commit结算。关闭路径191–201未接
Emote/Creative退休；Host仍未找到Emote capture及Creative retry capture/广播展开消费。
本任务未改共享源码，不把旧行号作为当前接线证明。

G2末项仍为P06 Wiring来源及SC094静态修正，无覆盖批次17/18的新构建或运行。P06 PASS
不覆盖P07；P04历史CS0118、Host最终源码无成功编译和SC091 run-01 FAIL/open继续保留。
本批未运行dotnet、编译器、DLL、服务端、网络、交互或任何场景；SC081未扩充且只覆盖
Party chat，SC082–SC090未运行。G2独占串行构建/验证；本批代码采用Karpathy的局部改动
与code-humanizer Guard的既有路径复用。源码及progress未跟踪、diagnostics被忽略，
未暂存、提交、清理或强制加入，P07完整goal仍active。

## 只读消费者审计补充（不新增实施批次）

2026-10-16 07:28 +08:00回读，承接上一轮只读调查；本次没有生产源码修改，没有新增
READY或PASS，批次17/18继续冻结。完整路径、行号、静态命令及工具错误已补入既有
P07-handoff的同名小节。

- Bestiary/Banner的ModulePeriodicSyncOwner→pump→commit settlement路径存在；两种
  batch有逐目标token。join直接快照不确认独立periodic水位，不能混为同一ACK。
- P05 CapturePendingDeaths按source version排序，AcknowledgeDeath要求当前同引用且
  由调用者保证掉落与贡献已结算；Host未找到死亡capture/ACK或三种贡献Apply/ACK调用。
- Host未找到AnglerQuestState/AdvanceWorldDay及EmoteCoordinator/清理capture调用。
  认证名/Packet74、day初始化、tick/disconnect/resync与死亡pump仍是外部装配边界。
- Creative首批关联投递及commit结算存在，但Host未消费公开CapturePendingDelivery，
  也未找到权限dirty capture、广播展开或Creative退休调用。Particles可沿现有dispatcher
  退休，不能由此推断所有P07 owner均已接入关闭生命周期。
- ModuleDelivery空receipt/未知sequence分类未见导致dirty丢失的证据；Particles极值
  与Banner threshold公告亦未形成可在Modules内闭合的已证修改，本次保留实现。

07:28:13 router SHA256已变为39CC180B90C6AFFBE1626982CC4FD27AF4E73B663C752835FAFF4C30FFD8CD8C；
共享Host变化未作本任务交付。批次17三份和批次18 Emote SHA256仍与冻结值一致。
未编译、测试或运行，SC081未扩充，SC082–SC090未运行；没有覆盖批次17/18的新G2验证。
P04历史CS0118、Host最终源码未获成功编译与SC091 FAIL/open不变，完整goal保持active。
07:30后置静态核对Modules全部74份文件SHA256，无新增、删除或内容变化；未执行代码。

## Invasion 回执与事件来源审计补充（不新增实施批次）

承接前轮未落盘的 Events 调查，本次仅维护既有 handoff/progress，无生产修改、新
READY 或 PASS；批次17/18继续冻结。文档修改前，Modules 全部74份文件逐路径 SHA256
与前轮缓存一致，无新增、删除或内容变化。完整来源定位、参考哈希和工具纠正见
P07-handoff“Invasion 回执契约与事件来源”小节。

- PacketSequence.cs:5 明确 Value>=0，sequence0合法；pump:94–100 的未知结果使用0。
  InvasionEffectDeliverySettlement.cs:272 不会因0拒绝回执，279–292 仅对成功回执做
  (epoch,sequence)唯一性检查；合法混合报告会保留成功项、未知项继续pending。先前
  “0使混合报告整体无效”疑点已排除，未改结算器。ModuleDelivery:140–150 对所有
  receipt做唯一性检查，多个未知0仍可能被判重复，未证明dirty丢失，不能混为0无效。
- handoff批次10旧Main:65153等行号实际属于另一棵完整树，旧“同版依据”已就地更正。
  指定Version4的Main:11846、12542、12619、12631、13470分别有实际progress/update/
  warning/start/morning delay方法体，NPC:64762有死亡信用、64830起有group映射。
  指定Main未找到CanStartInvasion；完整树Main:65245–65267才有现有admission方法。
  保留兼容行为及两树版本/行为等价限制，不把指定树实际方法体称为空。
- 指定NPC:6273、6634–6645的实际规则为LunarShieldPowerNormal=100，属性
  ShieldStrengthTowerMax在downedMoonlord时减半。WorldGen:73047、WorldFile:2288–2302、
  Projectile:48812–48846均实际使用动态max，NetMessage:1358按四塔顺序写ushort。
  当前WorldEventCoordinator固定100仅为宽上界，错误文本的LunarShieldPowerMax来源名
  不准确；producer已有max参数。尚无真实运行态owner/消费者，不直接改50或增设事实API。
- Host搜索仍只有WorldEventCoordinator构造/属性/退休，未找到Invasion owner/settlement、
  progress/tower apply、P05死亡capture/ACK或贡献Apply/ACK调用。既有revision/去重/
  retry实现保留；生产生命周期及跨owner闭环仍开放。

本次用rg --files定位并完整读取真实P07-module-implementation-plan.md，纠正前轮猜测
中文文件名；CreativePowerOwner.cs的错误路径亦已纠正为CreativePowerStateOwner.cs。
其他工具截断/无匹配及假设纠正记录于原handoff，不另建学习文件或更改AGENTS。
G2末项仍为P06 Wiring来源/SC094静态回执，无批次17/18的新编译或运行；P04历史CS0118、
Host最终源码无成功编译与SC091 FAIL/open保持。未执行dotnet、编译器、DLL、服务端、
网络、交互或场景；SC081未扩充且仅覆盖Party chat，SC082–SC090未运行，goal仍active。

07:39 +08:00后置核对：文档更新后，Modules全部74份文件SHA256与修改前及前轮缓存
一致，新增、删除及内容变化均0；批次17/18四份冻结源码哈希保持原值。

## 批次19：Bestiary 饱和计数修正（READY，未构建/未运行）

2026-10-16 07:46 +08:00，仅修改Modules/Bestiary/BestiaryState.cs:297–306。指定Version4
NPC.cs:65328的真实死亡路径继续调用RegisterKill；NPCKillsTracker.cs:29–41先递增，
存储夹在999999999，再广播原递增值。现有P07达到cap后直接Conflict，与该路径不同。
本批移除饱和拒绝，保留entry容量检查；reportedKillCount用于增量，Math.Min后的值用于
存储。已达cap时新死亡仍Applied并保留P05 receipt，推进owner/source revision及dirty；
增量为1000000000，存储、join/periodic快照保持999999999。同一death key仍Duplicate。
codec支持该Int32的7-bit形状；指定Deserialize为空，未证明客户端行为。

源码SHA256：修改前A4B5103AEF40F39B8CC5BFE6D3B07C1D077A4238F0A1687C40383899AF22B48A，
修改后02A914404BCAA60AAA47EDF1E90E0EE44AC957CC85FD2F48BBAAAC20583BA293。
静态对照74份Modules文件仅此文件变化，无新增或删除；文本差分仅Kill分支6删6增，
列宽、tab、行尾空白无异常。复用既有producer/revision/receipt，无新API或旁支清理。
源码冻结供G2接收，批次17/18四份冻结哈希保持；完整参考哈希、预期计数关系、副作用、
静态命令及工具纠正见P07-handoff批次19。

07:45只读wait_threads(timeoutMs:0)确认G2 idle、latestTurn completed、revision162；
账本末项仍为P06 Wiring来源/SC094静态回执，无覆盖批次17/18/19的新编译或运行。
没有live验证句柄，不记verified wait，未发跨任务消息。独立Host死亡capture/ACK与
贡献Apply/ACK搜索无命中；死亡pump及其他既有生产/来源缺口继续开放。

本批未执行dotnet、编译器、DLL、服务端、网络、交互或任何场景。G2独占串行验证；
READY仅指源码与静态证据落盘。SC081未扩充且只覆盖既有Party chat，SC084及其余
SC082–SC090未运行；P04历史CS0118、Host最终源码无成功编译和SC091 FAIL/open不变。
完整goal保持active，本批有可复核的源码进展，不作为整体闭环或PASS。

07:47 +08:00文档更新后静态核对：74份Modules文件仍仅BestiaryState变化，READY哈希
保持，无新增/删除；批次17/18四份冻结源码哈希全部匹配。未编译或执行场景。

## 接续核对：生产装配与异常线索（无源码进展）

2026-10-16 07:51 +08:00，补记前轮只读调查并核对当前消费者。本次仅维护既有
handoff/progress，无生产修改、新实施批次、READY或PASS；批次17/18/19继续冻结。

- 指定Main:13480真实StartDay调用AnglerQuestSwap，1611–1618清名单并选择任务，1752
  发送；MessageBuffer:2467–2475实际按认证槽的玩家名记完成，NetMessage:1140–1145
  按目标名写Packet74 finished。case74接收仅break，未证明客户端处理。现有producer
  与IsFinishedForPlayer保留；Host仍无Angler消费，真实day、逐玩家投影及奖励事务未闭合。
- Creative内层已有RetireEpoch；Host registry注册的三个包装handler均未实现退休接口，
  dispatcher只枚举已注册的可退休对象。仅修改内层无法闭合生产断线，不新增孤立接口。
- CommitVersion.Next在long.MaxValue抛异常；Angler/WorldEvent部分方法先写业务字段。
  Restore虽允许该正版本，尚无这两owner的Host恢复调用或生产可达性证据，本轮保留为
  未分类线索，不实施极值补丁或宣称生产故障。具体路径、行号、参考哈希见handoff新小节。
- 独立Host *.cs搜索仍无死亡capture/ACK、贡献Apply/ACK、Emote、Creative retry/权限
  capture/退休及Invasion消费命中，rg exit1为无匹配；跨owner与来源缺口保持。

G2只读快照仍为idle、completed、revision162、changed:false；账本末项无新READY，
没有覆盖批次17/18/19的新编译/运行或live验证句柄，不记verified wait，未发跨任务消息。
本次仅静态读取与哈希/局部状态核对，SC081未扩充，SC082–SC090未运行；P04历史CS0118、
Host最终源码无成功编译与SC091 FAIL/open保持。Karpathy和code-humanizer Guard未导出
旁支清理或新抽象。完整goal仍active，本轮按无源码进展记录，不作为闭环或PASS。
文档更新后，74份Modules文件与本轮修改前基线逐路径SHA256一致，无新增、删除或变化；
批次17/18/19五份冻结源码哈希保持，未执行C#。

## 阻断复核第2轮（无生产进展）

2026-10-16 07:55 +08:00，read_thread确认批次19后只有一个已完成no-progress turn；
压缩前后调查不能分开计数。本轮再次确认同一Host接线/独占验证阻断，连续计第2轮，
未满足3轮blocked门槛，goal保持active；G2仍idle/completed、revision162，无live句柄。
router有外部修改，当前退休在217/218行，SHA256及实际契约锚点已补入handoff；Creative
包装器/registry未变，缺失消费者搜索仍无命中。Bestiary/Banner周期同步已存在，P05死亡
capture/ACK及Creative/Emote既有API仍待Host消费，不重复新增owner或接口。
74份Modules逐路径SHA256与前轮一致，五份冻结源码保持；只更新既有记录，无生产修改、
新READY、编译或运行。SC081未扩充，SC082–SC090未运行，既有失败/open及来源限制保持。

## 阻断复核第3轮（完整目标未完成）

2026-10-16 08:01 +08:00。实际turn记录确认01a141f7、01a141fc连续两轮仅更新记录，均为
no progress；本轮01a14200仍未改变生产状态或下一步行动，连续第3轮，未重复计算压缩
或checkpoint。Host消费接线及G2独占验证的同一阻断已满足blocked审计门槛；完整目标
保留，最终状态以本轮update_goal返回为准，未标记完成。

G2实际wait_threads(timeoutMs:0, afterCursor=revision162)仍changed:false、idle/completed，
无live验证句柄，不记verified wait；账本末项仍无覆盖批次17–19的新构建/运行。
本轮从注册对象、通用pump与Host外生产调用三条路径反查：Creative包装器没有退休转发，
pump不保留root或重新捕获handle；正确排除Modules/Verification后，死亡capture/ACK及
其他缺口词组只命中NpcManager.Death.cs:93/103定义。Bestiary/Banner周期同步已存在。
pua自检未导出范围外编辑、构建或消息；具体假设、路径/行号、实际静态命令、快照哈希
及工具纠正已写入P07-handoff第3轮。

74份Modules与前轮后置基线逐路径SHA256一致，五份冻结源码保持。仅编辑既有记录，
无新生产补丁、READY、PASS、构建或运行；SC081未扩充，SC082–SC090未运行，P04历史
CS0118、Host最终源码无成功编译及SC091 FAIL/open保持。解除需G2完成外部生产接线和
串行验证，或用户调整当前分工；不以重复静态审计替代完整生产闭环。

## P03 WorldTick/Invasion 接线条件与 G2 probe 回执（2026-10-16 13:25 +08:00）

本节承接先前 blocked 记录后的真实 G2 进展，仍不代表 P07 完整闭合。用户规定的
WorldTick→Invasion 契约已按当前同版源码重新审计：仅 `WorldTickResult.Applied == true`
且 `MorningTransitionCount == 1` 时，才可在同一有序 commit 路径调用一次
`InvasionState.AdvanceMorning(worldScope, result.Version)`；不得使用 `SourceVersion`。
count 0 不调用；count >1 必须在 P07 mutation 前拒绝，或由 P03/G2 提供可运行的一晨/默认
tick 上界证明。原因是 `InvasionState.AdvanceMorning` 以 source version 防重：同一个最终
`result.Version` 的第二次调用将是 `Duplicate`，重复调用无法表达两个实际早晨。

现状尚无生产 bridge：`WorldTickPhaseCompositeOwner` 取得并检查 `WorldTickResult` 后不传递，
`WorldTickScheduler` 不支持 owner 间结果传递；虽已有 `ServerTickPhase.Invasion`，全树仍无
`AdvanceMorning(` 调用者。Host/World/Persistence 均不在 P07 写入范围，已由用户授权 G2 接线。
验收必须覆盖 rejected、Applied/0、Applied/1、Applied/>1 四支；P03 至今未构建或运行，
故不能把 API 形状视为运行证明。详尽行号、哈希与 bridge 禁止项见 P07-handoff 同名小节。

G2 随后产生两个可消费但范围有限的实测回执：

- Creative permission Server/Integration 的两次增量 build 均 exit0/0w/0e，
  loopback TCP probe exit0，实测初始15、变更15、干净0 packet，并覆盖 receipt、queue、
  codec、retire 分支。其 Active admission 与 periodic 调用为 fixture，未覆盖真实 join、
  自主九阶段、客户端 ACK、effect retry 或 SC091。
- runtime-startup Integration build exit0/0w/0e，probe exit0；四类无效配置监听前拒绝，
  两种合法 fixture 接受 TCP 并完成3/2有序 tick，关闭后端口可重绑。运行 DLL 闭包完整性
  已核验；由于禁用引用重建时依赖 DLL 有复制/时间漂移，未将其扩展为当前 Server/World/P03
  源码的编译或 Invasion 行为证明。

本轮仅修改本 progress 与 P07-handoff 记录；未改 Modules 生产源码，未运行 dotnet、测试、
DLL、server、网络或情景。P07 完整 goal 继续 active；SC091继续 FAIL/open，P05 death/ACK、
Invasion 实际事件/effect、Angler、Emote、Pylon 与 Creative root retry 等外部生命周期缺口保留。

## P03 WorldTime 单更新界限更正（2026-10-16 14:24:58 +08:00）

本节追加并 supersede 本文前一节中将 `MorningTransitionCount > 1` 作为正常 bridge consumer
分支的说法；未改写历史记录。当前 `World/WorldTime.cs` 的 canonical
`WorldTimeSystem.CalculateAdvance` 明确将 `elapsedGameTicks` 定义为缩放一次 update，而非
catch-up loop。它允许 endpoint equality，只在 strict overflow 时翻转并丢弃 overflow，且一次
调用只能报告 count `0` 或 `1`：night-to-day 为 `1`，所有其他情况为 `0`。

因此 P07 的有效 bridge 合约为：rejected result 不改变 Invasion；Applied 且没有 morning
transition 不调用；Applied 且有 morning transition 时，恰好一次调用
`InvasionState.AdvanceMorning(worldScope, result.Version)`。不得用 `SourceVersion`，也不得因
一次大的 `elapsedGameTicks` 多次调用。需要多个 host tick 时必须分别走 P03 commit。可保留
Host 防御断言以拒绝 impossible noncanonical count，但它不是受支持的多晨路径。

当前 `Verification/World/Program.cs` 源码也已静态覆盖 endpoint、strict overflow、frozen tick、
huge day 单次切换及 huge night 恰好一个 morning 和 `result.Version`。静态读取时：
`WorldTime.cs` SHA256 为 `BEE32DEC332ED38FF57A88C08C734F79B76810506B7D64E5BEF2C449C1900DCC`，
`Verification/World/Program.cs` SHA256 为
`D2D519C6C73D39FB71F439F07835016EC75536DE3980AB6EE04E3D25DE61FF46`。
这不是构建或 scenario 结果；P03 仍须在更正后 fresh serialized build/run，不能继承先前的
SC026/P03 PASS。P07 未改 Modules 生产源码，goal 保持 active，SC091 保持 FAIL/open；G2 需先
re-read/hash P03 文件后再进行其独占的 Host 接线与验证。

## P03 SC026 owner-preflight 已运行（2026-10-16 14:25 +08:00）

本节追加并更新上节“P03 仍须 fresh serialized build/run”的当时限制，不重写历史。G2 已在
其独占 World 窗口完成 World build-02、World verifier build-04 和一次 SC026 run-04。最终两次
build 均为 exit `0` / `0 warnings` / `0 errors`，run-04 为 exit `0`；权威命令、时间、输入/DLL
来源、日志和哈希见 `Build/diagnostics/MinimalImplementation/ParallelResume-20261016/core-test-results.md`
的 “P03 SC026 owner preflight: RED-02 / RED-03 -> PASS-04” 节。

PASS artifact 是
`P03/SC026-run-04/SC026-world-slice-result.md`（SHA256
`E8219630582949B7B25451AF36F6E36BD86DC44B1D27EBC4C1F35208C2E4AC78`）。它实际覆盖
endpoint equality、strict overflow 单次切换/overflow discard、frozen updates、moon wrap、huge
single update 至多一个 morning、capture identity/version、拒绝零写入、同 tick retry 与成功 tick
去重。当前 `WorldTime.cs` 和 World verifier SHA256 分别保持
`BEE32DEC332ED38FF57A88C08C734F79B76810506B7D64E5BEF2C449C1900DCC` 与
`D2D519C6C73D39FB71F439F07835016EC75536DE3980AB6EE04E3D25DE61FF46`。

这证明 P03 owner 的单更新 bound 已实际运行，不证明 P07 bridge。当前生产搜索仍没有
`AdvanceMorning(` Host caller；`WorldTickPhaseCompositeOwner` 对 Applied `WorldTickResult` 不向
Invasion 委托，SC026 artifact 也明确排除 Host morning delivery 与 P07 attachment capture。因而
G2 仍须在同一有序 Host commit 路径接入且验证一次
`InvasionState.AdvanceMorning(worldScope, result.Version)`；`SourceVersion` 和大 elapsed 的多次
调用仍禁止。P07 未修改 Modules 生产源码，SC091 FAIL/open 与 active goal 不变。

## Host morning bridge 已存在；生产自主调度仍 open（2026-10-16 22:44 +08:00）

本节追加并 supersede 上文“没有 Host AdvanceMorning caller”的现状判断，保留旧采样历史。
22:44:36 的源码确认 `Host/WorldTickInvasionBridge.cs` 使用稳定身份 scope 与 `result.Version`，
对 rejected/Applied-count0 零调用，对 canonical Applied-count1 一次调用；拒绝非 canonical
count，接受 Applied/Duplicate 并上抛 Rejected。composite 提供可选 bridge，manual
`P08BServerRuntime.AdvanceTickAsync` 已调用 composition bridge；恢复时 ReplaceState 更新
其 Invasion owner。六份源码哈希与具体行号见 P07-handoff 本时刻小节。

仍未找到生产 `new WorldTickPhaseCompositeOwner`：默认 runtime 分支没有 scheduler，外部
integration 使用 supplied owners，ComposeTickPhaseOwners 仅补 Projectile/PeriodicSync。
因此不能把可复用 composite/manual 路径视为真实自主 tick 或恢复后生产闭环。

G2 revision54 报告 SC093 run-01 exit0/PASS，结果文件已直接读取；当前 verifier 静态包含
bridge 检查。完整命令、时间及 DLL/源码绑定尚待 G2 新账本回执，本轮不扩张为生产验证。
P07 仅追加两份现有记录，未修改生产源码或执行构建/运行；其余事件/ACK/生命周期等开放项、
SC082–SC090 未运行、SC091 FAIL/open 和完整 goal active 均保留。

## G2 定向回执已落账；Server 运行依赖仍待绑定（2026-10-16 23:13 +08:00）

上节“完整来源待 G2 新账本回执”的时间点已过：G2 在 core-test-results.md:1389 起登记
四项目 build exit0/0w/0e、SC093 run-01 exit0/PASS及12/12持久化静态检查。P07 已读取
日志、结果并核对证据哈希；SC093结果 SHA256 为
2B173EE196D860A2DF8C03A464AB08272163CA0857FC5A5F6A5FFD974F7E7450。

但不能据此扩大为当前 Server 源码运行证明：verifier目录的 Server.dll 为296C1F…，
主输出先观察到894033…，随后变为4D3F24…。P02已有回执分别把前两者绑定到22:13及
23:03的不同构建；G2所列主输出894033…不是SC093实际依赖已绑定的充分证据。完整
SHA256、路径、时间和deps事实已追加P07-handoff“P07独立复核”节。SC093 PASS保留，
但运行前后依赖闭包/实际加载证据待G2补充；未自行构建、复制DLL或重跑。

runtime load/restore/save未执行；动态tile保存仍显式拒绝。自主生产scheduler、真实
事件效果与ACK/生命周期等缺口仍open。74份Modules的前后SHA256已核对一致；本轮
仅更新两份现有记录，SC082–SC090未运行、SC091 FAIL/open、完整goal active均不变。

## Creative Host 消费接线复核（2026-10-17 00:26 +08:00）

本节 supersede 本文较早“Host 未消费 Creative broadcast/effect settlement、permission dirty
capture 或 Creative retirement”的现状描述，不重写历史采样。P07 本轮只读复核并追加两份获准
记录，没有修改 `Modules/**`，没有执行 dotnet、编译器、DLL、服务端、网络、交互或 scenario。

- `CompleteModuleHandlers.cs:20,58-110` 现有 `CreativePowerModuleHandler : IModuleHandler,
  IModuleEpochRetirable`。Applied creative owner result 的 broadcast 会展开为 active recipients
  加 sender 的 target-specific Packet 82 intents，并作为 `CreativePowerEffectDeliveryBatch`
  settlement 返回；`RetireEpoch` 转发给内层 owner。因此 dispatcher retirement 到 Creative
  owner 是 confirmed static composition，不是 runtime disconnect 证明。
- `Version4InboundRouter.cs:486-492,521-536,597-637` 收集该 settlement，防止其 intents 落入
  普通无回执发送，以 receipts 发送，并在 `CommitLoop` 内 `Settle`。这确认 Creative 初始
  effect broadcast 已有 Host send/receipt settlement；不 settled 时只记录 packet rejection。
- `ModulePeriodicSyncOwner.cs:24-40` 为每个 active epoch 捕获 Creative permission dirty
  snapshot，通过 `CreativePermissionDeliverySettlement` 和 `ServerOutboundDeliveryPump` 交付；
  其 capture/ack、queue-reject release 与 worker-to-commit-loop settlement 是当前静态路径。
  它仍依赖实际 `PeriodicSync` phase 被完整调度，不能作为自主 runtime scheduler 或行为验证。
- `CreativePowerEffectDeliverySettlement.CapturePendingDelivery()` 仍无生产 Host caller：仅
  Creative 与 Invasion settlement 定义命中。其契约要求 caller retain root、对每个 fresh
  pending handle 只提交一次并自行 schedule retry；Router 未保存 root 或 enqueue replacement
  handle，pump 的 `LastSettlement` 也不是 retry-root store。因此 initial delivery confirmed，
  Creative/Invasion partial、failed、unknown、not-submitted receipt 的 retry closure 仍 open。
- 本轮 targeted production search 未发现 P05 death capture/contribution/ACK composition，也未
  新增 Angler、Emote、Invasion progress/tower/effect 的 Host consumer evidence。该静态结果不
  替代动态调用或完整行为验收。

复核 SHA256：CompleteModuleHandlers `D88E81D491B090EC47EB76BDC154225FF56DE5D08707F22B79CF7E864A73539A`；
Version4InboundRouter `661836951CF7AE864636CF909BADB7688824CF4F9B26249EE17D818F7C2A2B3C`；
ModulePeriodicSyncOwner `7ADF0481F90A7CAF91F7D1576954A163438EDBD3A662CBCA140609DA9AFCEFF6`；
CreativePermissionDeliverySettlement `C099000804ADD31C4B5E4D916510C38F7C54CF6C08E5BB33876F6A5D0977F397`；
ServerOutboundDeliveryPump `41021C1940A6FB98305E62CB20AEF9F3CDFF18E25D5F4BE68B632B53EDB4488E`；
P08BServerRuntime `D556251801FCB14450D4A6CED4C61855D7D00BF21B559F62C9C4938344C2FB88`。Creative
module hash 保持：handler `0828B6E7CFEE75C3CD4CE232BDF86CE2722919CDB808DAB406932356436F5E02`、
effect settlement `5C7784692428E9844C5F2A420893DE8F731679442CEF8774345BC487FE4E2328`、state owner
`1D4A11C65C1D49017F3D56FA267B86ACB7288661009E3C2610C2C53C8BFFBBD6`。

SC093 historical PASS、其 Server dependency/load binding 限制、runtime load/restore/save
未运行、SC082–SC090 未运行与 SC091 FAIL/open 均未改变；完整 P07 goal 继续 active。下一
实现/验证由 G2/Host 在授权串行窗口处理 retry-root retention、P05 composition 和完整 phase
owner runtime，并提供 run-bound dependency/load evidence；P07 不在 Modules 内复制 Host 编排 API。

## P07 module 6 reject policy 与越界 Host registration（2026-10-17 01:22 +08:00）

仅在 `Modules/Creative/CreativeModuleHandler.cs` 作本地 policy enforcement：修改前 SHA256
`0828B6E7CFEE75C3CD4CE232BDF86CE2722919CDB808DAB406932356436F5E02`，修改后
`5B575A3B51CBE3928328BB0E39EB7ECA861B9B46AAC2CBF17A6DAC2E7FEC9D98`。`HandleUnlockReport`
当前始终返回 `NotReady`、无 commit version、无 report，不调用 `CreativeUnlockReportOwner.Record`；
构造函数签名仍保留以兼容只读 Host composition。静态 Host 调用 `CompleteModuleHandlers.cs:293-302`
在非 Applied/no report 时 return reject，不会继续至 `:304-320` relay producer。该结论限于源码控制流。

剩余闭合未完成：`ServerComposition.cs:615` 仍注册 module 6 adapter，
`Protocol/Version4ModuleCodecs.cs:22-23` 仍启用五字节 typed module-6 codec；Host/Protocol 均超出
P07 写范围，需 G2 删除/恢复冻结的空模块 wire 形状并验证 composition。module 7 同样有
Host 注册冲突：pending route 在 `ServerComposition.cs:602-605` 排除 id 7，`:615-617` 注册 Pylon
adapter，而 `Modules/DelegatedModuleRejectionHandler.cs:13-14,47-55` 保存的是拒绝策略；交由 G2
处理，不在此修改 Host。两项均未有运行时验证。

本次 Modules 递归文件计数为74；按相对路径排序并串联逐文件 SHA256 的 inventory hash 为
`3C45299B3EAE65DA718CE98C80C0B5101816CA1A35FB28F80D0BB066C99D7583`。未执行 build、test、
scenario、DLL、server 或网络操作。SC081 未扩充，SC082–SC090 未运行，SC091 继续 FAIL/open；
Creative/Invasion retry-root、P05 death/ACK、生命周期消费者、完整 scheduler 与 runtime
recovery 仍 open，完整 P07 goal 保持 active。细节与只读搜索命令见 P07-handoff 同名节。

## P07 module 5 guard 与 snapshot validation 静态续记（2026-10-17 02:24 +08:00）

本次继续只修改并复核 `src/MinimalImplementation/Modules/**`，未修改 Host、Protocol、Contracts、
项目文件或 verifier；没有执行 build、test、scenario、DLL、server、network 或交互。

| 文件 | 修改前 SHA256 | 当前 SHA256 |
| --- | --- | --- |
| `Modules/Creative/CreativeModuleHandler.cs` | `0828B6E7CFEE75C3CD4CE232BDF86CE2722919CDB808DAB406932356436F5E02` | `5B575A3B51CBE3928328BB0E39EB7ECA861B9B46AAC2CBF17A6DAC2E7FEC9D98` |
| `Modules/Creative/CreativePowerStateOwner.cs` | `1D4A11C65C1D49017F3D56FA267B86ACB7288661009E3C2610C2C53C8BFFBBD6` | `E1CD9FCC540DE9E01E7F8BBCF83B0F8F6382CA6E71380C751131506DFD9B6208` |
| `Modules/Creative/CreativePowerSnapshot.cs` | `8EA830BF32E8DAEEF1EF7C1A86FEBE54D5EEE03A02E198DAEF67F509DEF81C5B` | `E9CCE35D48716EB3D999D5440933384D9D2001CF3C766E76EF46AD1CA8A51795` |

冻结政策来源为 parent plan `:129-138` 与 P07-B `:48-53,108-109`：module 5 只接受 authenticated
power 14 per-player slider，其他 creative registrations 不启用；module 6 仍 rejected/pending。
当前 owner 的 `:34-35,72-95` 对错误 id/value kind 返回 `NotReady`，对非 finite 或越界 slider 返回
`InvalidLength`，permission 在状态写入前检查，只有 power 14 合法输入才推进 revision 并生成 effect。
snapshot 的 `:39-42,67-80,104-129,164-171` 对 shared 与 per-player slider 逐值执行 finite
`[0,1]` 校验；非法 restore 输入在构造阶段拒绝。module 6 handler 的 `:35-47` 保持 `NotReady`、
无 version/report 且不调用 sacrifice owner。以上为静态源码结论，未升级为行为验证。

当前 `Modules/**` 仍为 74 个文件。明确序列化后 inventory SHA256 为
`748DA50CE5D2A7AA10512ACE93B0B939BBE15D652067E815B02B94279B10746E`：相对路径排序，逐行连接
`relativePath + " " + uppercase per-file SHA256`，LF 连接、无尾 LF，再对 UTF-8 字节做 SHA256。

范围外未闭合项保持不变：Host module 6 registration 与 Protocol 五字节 typed module-6 codec
仍在 `ServerComposition.cs:615`、`Version4ModuleCodecs.cs:22-23,74-88`；Host handler 的
`CompleteModuleHandlers.cs:293-302` 只提供本地拒绝后的静态短路。module 7 rejection policy 与
Pylon adapter 的 Host 冲突仍交 G2。Creative/Invasion retry-root、P05 death/ACK、生命周期消费者、
Invasion/Packet 82 producer、完整 scheduler、runtime recovery/load/restore/save，以及 SC091
FAIL/open 仍未闭合；SC081 未扩充，SC082-SC090 未运行，完整 P07 goal 保持 active。

## P07 module 14 producer policy correction 与 G2 wait（2026-10-17 02:36 +08:00）

冻结计划 `P07-module-implementation-plan.md:172-174` 要求 module 14 保持 rejected/pending，禁止
local-only scan notification。此前 `Modules/Effects/UnbreakableWallModuleProducer.cs:17-38` 仍能
在状态变化后创建 module 14 intent；本次在授权 Modules 范围内修正为保留兼容 API 但始终不产生
intent，且移除未再使用的 payload producer field。当前文件 `:6,14-36` 只保留拒绝策略说明、参数
兼容校验和 `null` 返回；全源码搜索没有发现该 producer 的 caller。

| 文件 | 修改前 SHA256 | 当前 SHA256 |
| --- | --- | --- |
| `Modules/Effects/UnbreakableWallModuleProducer.cs` | `91965C86971F2F268C90A611AF948F0E555BA4558014AC0F7230C18CDBB409C5` | `A4BF5444D03BF2D6556177999336828E74088CF7EDE029CBE98325F80AAA7753` |

当前 `Modules/**` 仍为 74 个文件；按相对路径、文件 SHA256 和 LF 连接的明确序列化规则重算，
inventory SHA256 为
`DFB39AD826EB5E043C120925D24CAD06B645314D4597E11749E7F5FA3A2958D5`。当前没有执行 build、test、
scenario、DLL、server、network 或交互。

G2 的只读 `wait_threads(timeoutMs=30000, afterCursor=...:113)` 返回 `changed=false`，revision
仍为 113、thread 仍 active/inProgress，最新工作仍是 P06/P04 B11，尚无 P07 Host/Protocol 接线或
验证回执。因此这是 live handle 的 verified wait，不是 P07 完成证据。module 6/7 wiring、retry-root
consumer、P05 death/ACK、生命周期 producer、完整 registry/scheduler/runtime recovery 继续 open；
SC081 未扩充，SC082-SC090 未运行，SC091 继续 FAIL/open，完整 P07 goal 保持 active。

## P07 Banner claim policy correction（2026-10-17 02:46 +08:00）

静态审计发现 `Modules/Banners/BannerModuleHandler.cs` 仍会把
`Packet82BannerClaimRequestData` 送入 `BannerState.ProcessClaimRequest` 并返回 `Handled`；这与
`P07-module-implementation-plan.md:143-146` 及 `P07-B-stateful-module-owners-plan.md:48-53,108-110`
的冻结策略冲突。Module 10 当前只允许服务端 actions 0–2 投影，历史 claim actions 3/4 未获批准，
客户端 claim 必须拒绝。

当前 `BannerModuleHandler.cs:7-42` 保留 active envelope 校验后统一返回
`ProtocolErrorCode.NotReady`，不再调用 claim owner、不推进 revision、不扣减 claimable count、
不创建 outbound intent。`BannerState.ProcessClaimRequest` owner API 保留给未来新的 root policy
决定；当前 active handler 中已无该调用。

当前文件 SHA256：
`6DAEB20D0743127BD22267D109BC20D7AB80BCFDCE770C75543C377C71F390FC`。
针对该文件的 `ProcessClaimRequest|Packet82BannerClaimRequestData|ModuleDispatchResult\\.Handled`
搜索无命中；逐行格式检查（超过 100 列、Tab、尾随空白）为 0。

`Modules/**` 仍为 74 个文件。按 `/` 规范化相对路径、排序、逐行连接
`relativePath + " " + uppercase per-file SHA256`，使用 LF 且无尾 LF，再对 UTF-8 字节计算，本次
inventory SHA256 为
`E139CFE5E23D2DCE9B940008BFF62B71D8D50B02CA2CD3234E20E62170CC26F4`；首条/末条仍为
`Banners/BannerClaimRequest.cs` / `Social/SocialPresentationProducer.cs`。

本次未执行 build、test、scenario、DLL、server、network 或交互；SC081 未扩充，SC082–SC090 未运行，
SC091 继续 FAIL/open。旧 claim acceptance 描述保留为历史记录，本节是当前源码控制流修正，不是
Host/Protocol/runtime 验证。Module 6/7 wiring、retry-root、P05 death/ACK、生命周期 producer、
完整 registry/scheduler 和 runtime recovery/load/restore/save 仍 open，完整 P07 goal 保持 active。

## G2 serialized wait revision 143（2026-10-17 02:52 +08:00）

对 G2 live handle `01a13b8b-704a-7ee1-85da-9e481fd15ba6`、host `local` 使用
`wait_threads(timeoutMs=30000, afterCursor=c33dfc34-2b23-434e-be6a-fc5372cd8c7f:113)` 做只读等待。
结果虽为 `timedOut=true`，但观察到真实进展：cursor 为
`c33dfc34-2b23-434e-be6a-fc5372cd8c7f:143`，revision 为 `143`，thread 仍 `active`，latest turn
仍 `inProgress`。最新可见工作是 P03 World packet 77 的 Host 编码和 outbound pump 接线，没有
出现 P07 Host/Protocol wiring 或 P07 验证回执。

该结果是 live handle 的 verified wait，不是 P07 完成证据；没有发送跨任务消息，没有修改 Host/P03
范围，也没有执行 build、test、scenario、DLL、server、network 或交互。等待后 Modules 只读复核仍为
74 个文件，inventory 为
`E139CFE5E23D2DCE9B940008BFF62B71D8D50B02CA2CD3234E20E62170CC26F4`，
`BannerModuleHandler.cs` SHA256 仍为
`6DAEB20D0743127BD22267D109BC20D7AB80BCFDCE770C75543C377C71F390FC`。SC081 未扩充，SC082–SC090
未运行，SC091 继续 FAIL/open，完整 P07 goal 保持 active。

## P07 serialized wait revision 155 and local audit（2026-10-17 03:01 +08:00）

已知 G2 live handle `01a13b8b-704a-7ee1-85da-9e481fd15ba6`（host `local`）使用此前 cursor
`c33dfc34-2b23-434e-be6a-fc5372cd8c7f:143` 做一次 timeout 60000 ms 的只读等待。返回
`timedOut=true`，但 cursor 前进到
`c33dfc34-2b23-434e-be6a-fc5372cd8c7f:155`、revision 为 `155`；thread 仍 `active`，latest
turn 仍 `inProgress`。最新可见工作是 P03 packet 77 的 Host 消费窗口，而不是 P07 接线或
验证回执；该状态不证明 P07 闭合。

本轮只读复核 `Modules/**` 的冻结策略，没有修改生产代码，也没有运行 build、test、scenario、
DLL、server、network 或交互。确认 Banner 本地 codec 仍只允许 actions 0–2，Banner handler
仍拒绝客户端 claim；module 6 typed local codec 没有活动 registry caller，module 6 handler
仍不调用 `CreativeUnlockReportOwner.Record`。对 Banner owner 中 `bannerId <= 0` 的候选边界
修正进一步核对了只读 Version4 `BannerSystem.AddNPCKillBy`：`NPCtoBanner` 返回 `<= 0` 时
会直接返回，所以 id 0 是无效哨兵，不能因 293-entry 数组或 codec 接受 `0..292` 就修改 owner
边界；本轮没有生产代码变更。

当前 `Modules/**` 仍为 74 个文件，inventory 仍为
`E139CFE5E23D2DCE9B940008BFF62B71D8D50B02CA2CD3234E20E62170CC26F4`；
`BannerModuleHandler.cs` SHA256 仍为
`6DAEB20D0743127BD22267D109BC20D7AB80BCFDCE770C75543C377C71F390FC`。module 6/7 Host/Protocol
wiring、retry-root、P05 death/ACK、生命周期消费者、完整 registry/scheduler、runtime
recovery/load/restore/save、SC082–SC090 和 SC091 FAIL/open 继续未闭合；完整 P07 goal 保持
active。

## P07 serialized wait revision 164 and fresh consumer audit（2026-10-17 03:06 +08:00）

已知 G2 live handle `01a13b8b-704a-7ee1-85da-9e481fd15ba6`（host `local`）使用 cursor
`c33dfc34-2b23-434e-be6a-fc5372cd8c7f:155` 做一次 timeout 60000 ms 的只读等待。返回
`timedOut=true`，但 cursor 前进到
`c33dfc34-2b23-434e-be6a-fc5372cd8c7f:164`、revision 为 `164`；thread 仍 `active`，latest
turn 仍 `inProgress`。最新可见工作仍是 G2 packet 77 的 commit-loop capture/编码/入队实现，
不是 P07 module 6/7 wiring 或验证回执；该状态不证明 P07 闭合。

本轮对当前源码做 fresh read-only audit，没有修改生产代码，也没有运行 build、test、scenario、
DLL、server、network 或交互。`CapturePendingDelivery()` 仍只有 Creative 与 Invasion settlement
定义，P05 `CapturePendingDeaths`/`AcknowledgeDeath` 仍只在 Combat death owner；Host 仍注册
module 6 `CreativeUnlockReportModuleHandler` 与 module 7 `TeleportPylonModuleHandlerAdapter`，
Protocol 仍将 module 6 表示为 typed `CreativeSacrificeReportData`。因此通用 packet 77 pump
进展尚未提供 P07 retry-root、P05 death/ACK、module 6/7 policy 或完整 Packet 82 registry 的
证据。

当前 `Modules/**` 仍为 74 个文件，inventory 仍为
`E139CFE5E23D2DCE9B940008BFF62B71D8D50B02CA2CD3234E20E62170CC26F4`；本轮没有生产源码哈希
变化。P07 module 6/7 wiring、retry-root、P05 death/ACK、生命周期消费者、完整 registry/scheduler、
runtime recovery/load/restore/save、SC082–SC090 和 SC091 FAIL/open 继续未闭合，完整 P07 goal
保持 active。


## P07 Tower shield source-version idempotency correction and G2 serialized wait revision 167（2026-10-17 03:12 +08:00）

`src/MinimalImplementation/Modules/Events/WorldEventCoordinator.cs:440` 已修正 Tower shield commit
的 source-version 处理：先拒绝无效 source version 和越界 shield 值；同一 `SourceVersion` 且四个
shield 事实相同返回 `Duplicate`，不推进 `_version`、不改状态、不发 `TowerShieldEffect`；同一版本
事实不同返回 `Conflict`；更旧版本拒绝；更高版本才应用并推进 commit version。该语义与
`ApplyInvasionProgress` 现有的同版本幂等/冲突处理保持一致。

本文件当前 SHA256 为
`1134F87D8E7B2DA6842279CCE5512532F08E133B67F601C86263E0D137DA54B4`，共 580 行；超过 100 列、
Tab、尾随空白的逐行静态计数均为 0。`Modules/**` 递归文件数为 74，按 `/` 规范化相对路径、
排序、以 LF 且无尾 LF 连接 `relativePath + " " + uppercase per-file SHA256` 后计算得到 inventory
SHA256：`0C535BD1114127F302995C1A8205CDECA99F7BDD4A7FC2D268633BD1D65C93B5`。

对 G2 live handle `01a13b8b-704a-7ee1-85da-9e481fd15ba6`（host `local`）以 cursor
`c33dfc34-2b23-434e-be6a-fc5372cd8c7f:164` 执行只读 `wait_threads` 快照（`timeoutMs=0`）。工具
返回 `timedOut=true`，但确认 cursor 前进至 `c33dfc34-2b23-434e-be6a-fc5372cd8c7f:167`、revision
为 `167`；thread 仍 `active`，latest turn 仍 `inProgress`。最新工作仍是 packet 77 Host 消费入口、
commit loop 与投递结算签名核对，并未出现 P07 module 6/7 wiring、retry-root consumer、P05
death/ACK composition 或验证回执；该快照是 G2 live progress，不是 P07 闭合证据。

本轮未执行 build、test、scenario、DLL、server、network 或交互；SC081 未扩充，SC082–SC090 未运行，
SC091 继续 FAIL/open。module 6/7 Host/Protocol wiring、Creative/Invasion retry-root、P05 death/ACK、
Angler/Emote 生命周期消费者、Invasion/tower/effect producer consumers、完整 Packet 82 registry、
autonomous scheduler 与 disconnect/recovery/load/restore/save runtime evidence 仍 open；完整 P07 goal
保持 active。

## P07 recipient-boundary hardening and current Modules audit（2026-10-17 03:26 +08:00）

本轮重新核对 P07 parent/B/C 冻结计划与当前 Modules 源码，确认部分 intent 路径只验证 epoch
有效性、却没有统一限制去重后的 recipient 数量到 Version4 的 255-slot 上限；通用 Packet 82
producer 也会原样保留重复 target。

授权的 Modules 范围内已完成以下修正：

- `Protocol/Packet82ModuleIntentProducer.cs` 对 targets 稳定去重并按 epoch 排序，拒绝空集、无效
  epoch 和超过 255 个目标的 intent。
- `Effects/Packet82PresentationValidation.cs` 拒绝超过 255 个或重复的 presentation target，
  同时保留 commit version 和 epoch 检查。
- `Social/SocialPresentationProducer.cs`、`Social/EmoteCoordinator.cs` 对直接生成的 recipient
  集合补上 255-slot 上限，避免绕过通用 producer 生成不可表示的广播。

四个文件当前 SHA256 为：`Packet82ModuleIntentProducer.cs`
`4E1C249027A0A09783A3B1433BBCE9E40FC7A185ED44F720A6DC64E1E65DED36`；
`Packet82PresentationValidation.cs`
`4EE3A874DCF6C3A624458C337984CCA3923BD73C3FA30F25FA8D65E0EAFB9E29`；
`SocialPresentationProducer.cs`
`40F165151360D6A6929DDEFDE2C0252036A03F78C1DDB293C3607E3AFF2D9F64`；
`EmoteCoordinator.cs`
`ADEEC495C5A4CEB01B5D30E703AE541DA1C48D3A4077494304962240A43EF917`。四个文件逐行静态检查的
超过 100 列、Tab、尾随空白计数均为 0。`Modules/**` 仍为 74 个文件，当前 inventory SHA256 为
`8647580D3B1D165D04C624CB0E9161692822B0FFF58BD82AB6A727AC3734CC85`。

本轮未执行 build、test、scenario、DLL、server、network 或交互；SC081 未扩充，SC082–SC090 未运行，
SC091 继续 FAIL/open。此修正没有闭合 Host/Protocol registry、module 6/7 route policy、
Creative/Invasion retry-root、P05 death/ACK、生命周期 producer consumer、完整 Packet 82
registry/scheduler 或 recovery/load/restore/save runtime；对应当前证据仍在
`Host/CompleteModuleHandlers.cs:256-320`、`Host/ServerComposition.cs:600-628`、
`Protocol/Version4ModuleCodecs.cs:20-28` 与跨 owner settlement 边界。完整 P07 goal 保持 active。

## G2 serialized wait revision 203（2026-10-17 03:27 +08:00）

对 G2 live handle `01a13b8b-704a-7ee1-85da-9e481fd15ba6`（host `local`）以此前 cursor
`c33dfc34-2b23-434e-be6a-fc5372cd8c7f:167` 执行只读 `wait_threads` 快照（`timeoutMs=0`）。工具
返回 `timedOut=true`，但确认 cursor 前进至 `c33dfc34-2b23-434e-be6a-fc5372cd8c7f:203`、revision
为 `203`；thread 仍 `active`，latest turn 仍 `inProgress`。最新工作是 P03 核对发送前 stale
world-intent 防护与 world retirement 顺序，不是 P07 module 6/7 registry、retry-root consumer、
P05 death/ACK composition 或验证回执；该快照是 G2 live progress，不是 P07 闭合证据。

本次等待没有修改源码或验证范围，也未执行 build、test、scenario、DLL、server、network 或交互；
SC081 未扩充，SC082–SC090 未运行，SC091 继续 FAIL/open。recipient-boundary 修正与当前 Modules
inventory `8647580D3B1D165D04C624CB0E9161692822B0FFF58BD82AB6A727AC3734CC85` 保持不变，完整 P07
goal 保持 active。

## P07 Angler world-day source-version idempotency correction and current Modules audit（2026-10-17 03:34 +08:00）

本轮继续限定在 `src/MinimalImplementation/Modules/**`，针对 `Events/AnglerQuestState.cs:260-280`
完成一个可由源码直接证明的 source-version 幂等修正。原逻辑已经拒绝同一 P03 source version
提交不同 world-day key，但同一 source version 且同一 key 会落入普通 `Applied` 返回；这会把重复的
同一 source revision 错报为新的应用结果。

当前逻辑顺序为：无效或更旧 source version 先拒绝；同一 source version 且 key 不同返回
`ProtocolErrorCode.Conflict`；同一 source version 且 key 相同在第 270 行返回
`AnglerQuestCommitResult.Applied(_version, null).AsDuplicate()`，不推进 `_version`、不清理玩家完成集、
不改变 quest state，也不生成 effect；只有更高 source version 才能在同一 key 上更新
`_lastDaySourceVersion` 并返回 `Applied`。更高版本配合新 key 的原有过渡和 commit-version 推进行为保持不变。
`AsDuplicate()` 本身只将非拒绝结果的 status 改为 `Duplicate`，保留当前 commit version 和空 effect。

本轮实际执行的只读核对命令包括：

- `Get-Content`、`rg -n -C 3`，核对 `AnglerQuestCommitResult.AsDuplicate()`、`AdvanceWorldDay` 和相关字段。
- PowerShell 逐行检查 `AnglerQuestState.cs` 的行数、超过 100 列、Tab、尾随空白和修改行号。
- `Get-FileHash -Algorithm SHA256`，核对 `AnglerQuestState.cs`。
- PowerShell 递归枚举 `Modules/**`，将 `/` 规范化相对路径排序后按既有规则计算 inventory。
- `Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz'`，记录本段时间。

当前证据：`AnglerQuestState.cs` 共 511 行，修改返回位于第 270 行；SHA256 为
`4F3D92BA5C5830BB52AD600BC4658B26AC7E8E641D05441F51C316032735C732`。Tab 和尾随空白计数均为 0；
超过 100 列计数为 2，分别是既有的第 414 行（106 列）和第 423 行（102 列），本轮没有修改这两行。
当前 `Modules/**` 递归文件数仍为 74，首条和末条仍为
`Banners/BannerClaimRequest.cs` 与 `Social/SocialPresentationProducer.cs`；inventory SHA256 更新为
`2B45028E5ED7F1425CDC92CD736CDB13BE77A616928E7E5AB736C52F80B9C2E1`。

本轮没有执行 build、test、scenario、DLL、server、network 或交互命令，也没有执行序列化验证；
SC081 未扩充，SC082–SC090 仍未运行，SC091 仍为 FAIL/open。因此本段只证明 Angler world-day
source-version 的 Modules-local 同版本重复事实处理，不证明 Host module 6/7 registry、Protocol
typed module 6 route、module 7 Pylon adapter policy、Creative/Invasion retry-root consumer、P05
death/ACK composition、生命周期 producer consumer、完整 Packet 82 registry/scheduler 或
disconnect/recovery/load/restore/save runtime 已闭合。

已知未解决证据继续位于 `Host/CompleteModuleHandlers.cs:256-320`、`Host/ServerComposition.cs:600-628`、
`Protocol/Version4ModuleCodecs.cs:20-28`、`Modules/Creative/CreativePowerEffectDeliverySettlement.cs:36`、
`Modules/Events/InvasionEffectDeliverySettlement.cs:188` 和 `Combat/Npcs/NpcManager.Death.cs:186`；
按当前授权边界不修改 Host、Protocol、Combat、World、Persistence、Contracts、shared registry、project
files、verifier 或 G2 artifacts。完整 P07 goal 保持 active。

## P07 Bestiary/Banner recipient-boundary result hardening（2026-10-17 03:39 +08:00）

本轮 fresh audit 发现 `BestiaryState` 与 `BannerState` 的 owner-level target validator 只检查
epoch 有效性；在前一轮 `Packet82ModuleIntentProducer` 已拒绝超过 255 个唯一 target 后，P05 kill
贡献、Banner claim 或其他 owner 调用仍可能把 256 个唯一 recipient 送入 producer，令 producer 以
`ArgumentOutOfRangeException` 退出，而不是返回稳定的业务结果。这会把不可表示的 outbound fan-out
输入变成异常路径，并使 owner 无法向上游报告可识别拒绝。

当前修正严格限定在两个 Modules owner 的 `HasValidTargets`：

- `Modules/Bestiary/BestiaryState.cs:472-491` 现在在检查每个 epoch 有效性的同时，对去重后的 target
  集合应用 `MaximumTrackedRecipients`（255）上限；超过上限返回 `false`，调用方返回已有的
  `BestiaryContributionStatus.Invalid` / `ProtocolErrorCode.InvalidLength`，不推进 Bestiary revision、
  不写入贡献、不生成 intent。
- `Modules/Banners/BannerState.cs:426-445` 使用同一规则；P05 kill 和 claim 路径在超过 255 个
  唯一 target 时返回已有的 `BannerCommitStatus.Rejected` / `ProtocolErrorCode.InvalidLength`，不推进
  Banner revision、不扣减 claimable count、不生成广播或 claim response intent。
- 重复 target 不会被错误拒绝；producer 仍负责稳定去重和排序，去重后不超过 255 个的集合继续按
  既有路径处理。

当前源码证据与哈希：

- `BestiaryState.cs` 共 514 行，validator 修改位于第 487 行；SHA256 为
  `0121310B1F4EFA17596D7DC7684CE492215452B15D4F3A9A954897DEF7838E6F`。
- `BannerState.cs` 共 455 行，validator 修改位于第 441 行；SHA256 为
  `9EA1262E15681926511A4962699681A857F1C93E0018EB88E17C867259686133`。
- 两个文件逐行静态检查的超过 100 列、Tab、尾随空白计数均为 0。
- 当前 `Modules/**` 仍为 74 个文件，首条和末条仍为
  `Banners/BannerClaimRequest.cs` 与 `Social/SocialPresentationProducer.cs`；inventory SHA256 为
  `9C39D0D3E680BB00754FC72ADAA93844548120FC8AAC39754CD052CA255834FF`。

本轮实际只读核对命令包括 `rg -n -C` 搜索 owner validator、producer 和 P07 计划边界，
`Get-Content` 查看 Bestiary/Banner 状态提交路径，PowerShell 逐行静态检查两个文件，
`Get-FileHash -Algorithm SHA256` 计算文件哈希，以及递归枚举 `Modules/**` 后按既有路径/文件哈希规则
计算 inventory。源码编辑使用 `apply_patch`；本轮没有执行 build、test、scenario、DLL、server、
network 或交互命令，也没有执行序列化验证。SC081 未扩充，SC082–SC090 仍未运行，SC091 仍为
FAIL/open。

该修正确认的是 P07 Modules-local recipient-boundary 和 result-based rejection，不证明 Host
module 6/7 registry、Protocol typed module 6 route、module 7 Pylon adapter policy、Creative/Invasion
retry-root consumer、P05 death/ACK composition、生命周期 producer consumer、完整 Packet 82
registry/scheduler 或 disconnect/recovery/load/restore/save runtime 已闭合。已知阻断与当前授权边界
保持不变，完整 P07 goal 保持 active。

## P07 Bestiary same-source contribution fact idempotency（2026-10-17 14:42 +08:00）

本轮按单文件 batch 审计 `Modules/Bestiary/BestiaryState.cs`。之前 `ApplyP05Contribution` 在
`SourceCommitVersion == _lastP05CommitVersion` 时无条件返回 `Duplicate`，没有核对这次贡献是否与
同一 source revision 已接收的事实一致。这允许同一个 P05 revision 被改成另一种贡献、另一个 NPC
网络 ID 或另一个持久 NPC ID 后，仍被误认成重复。

当前单文件修正在 `BestiaryState.cs` 加入最近一次 P05 contribution identity，由 kind、NPC network
ID 和 persistent NPC ID 组成。相同 source revision 只有 identity 完全相同才返回 `Duplicate`；若
identity 不同，返回现有 `BestiaryContributionStatus.Invalid` 与 `ProtocolErrorCode.Conflict`，不写入
Bestiary 状态、不生成 intent。较旧 source revision 仍按原路径拒绝。对已知 Bestiary restore 只保存
source version、没有最近事实 identity 的状态，同版本重放按 Conflict 拒绝，不猜测其事实相同。

已有 sight/chat 状态导致贡献语义上重复时，owner 仍推进已消费的 P05 source cursor，并同时保存该
revision 的 identity；后续相同 revision 的同一事实可被判为 Duplicate，不同事实会 Conflict。新的
已应用贡献也在状态提交后同步记录 identity。P05 visible recipient snapshot 不进入 source identity：
它是外部传入的投影目标事实，不是此 P05 committed Bestiary contribution 的身份字段。

本轮源码定位：`BestiaryState.cs:15-19` 定义 identity，`:36` 保存最近 identity，`:281-297` 执行同版
事实相等性检查，`:324-357` 记录被语义去重的 sight/chat 事实，`:402-405` 记录正常提交事实。文件共
541 行，SHA256 为
`433DFE53DD9BF5A2A0A5E0ED213FC04BD56518207D55B716F64C6D5FFE27A834`。逐行静态检查：超过 100 列
0 行、Tab 0 行、尾随空白 0 行。

当前 `Modules/**` 为 75 个文件。新增于前一次 74-file inventory 记录之外的
`Creative/CreativeWorldPowerState.cs` 已在本轮只读确认存在；它不是本轮创建或编辑的文件。按 `/`
规范化相对路径、排序、以 LF 且无尾 LF 连接 `relativePath + " " + uppercase per-file SHA256` 后计算，
当前 inventory SHA256 为
`63C292A1D8FB24E121D8C56A6ED0748B51DA3F6F57EFBB710E352ED3DED000DE`。

本轮实际核对包括 `rg -n -C` 搜索 Bestiary source-version 分支和调用点、`Get-Content` 查看 Bestiary
owner/贡献类型/snapshot 及 WorldEventCoordinator 同版本事实比较、PowerShell 逐行格式检查、
`Get-FileHash -Algorithm SHA256` 和 `Modules/**` inventory 计算。源码使用 `apply_patch` 修改。本轮没有
构建、测试、运行、scenario、DLL、server、network 或交互操作；SC081 未扩充，SC082–SC090 仍未运行，
SC091 仍为 FAIL/open。

该单文件修正不闭合 Host/Protocol module 6/7 wiring、Creative/Invasion retry-root consumer、P05
death/ACK owner composition、生命周期 producer consumer、完整 Packet 82 registry/scheduler 或
disconnect/recovery/load/restore/save runtime。中断前工作树中另外存在 `PartyChatCommand.cs`、
`PingModuleHandler.cs` 与 `ParticlesModuleHandler.cs` 的 recipient-boundary 修改；本轮只读确认其源码
当前可读，本批没有修改或为其运行编译验证。完整 P07 目标仍未完成。
