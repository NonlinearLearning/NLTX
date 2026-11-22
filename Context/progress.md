# Project Progress Summary

## 2026-11-22 项目文件组织从 ECS 文件组织约束拆分

`Context/架构设计/ECS文件组织设计约束.md` 收窄为 ECS 领域内部组织；仓库与项目层落位
（根目录分层、生产 vs `分类参考`、`Shared/Server/Client` 端别、命名空间与迁移记录、通用验收）
拆入新文件 `Context/架构设计/项目文件组织设计约束.md`，并按用户选定采用「范围拆分」边界
（原 §3 + §6 + §7 通用迁移规则 + §8 通用验收）。AGENTS.md 新增 Project File Organization 条目，
ECS 条目触发条件收窄；组件命名、Entity 组织、六边形边界三份约束与 `Test/README.md` 的
交叉引用同步更新。原文件新增 §8 章节迁移对照，历史文档中的旧节号按表换算。

纯文档变更，未运行构建或测试。详见
[项目文件组织拆分 CR](../docs/cr/CR-2026-11-22-project-file-organization-split.md)。

## 2026-11-22 Test 目录按生产边界重组

`Test/` 由 13 个平铺的 `*.Verification` 项目改为「生产边界 → 验证项目」两层结构：一级目录
`Test/Terraria.Components/`（12 项）与 `Test/NSSLC.Network/`（1 项），项目名去掉统一后缀，
改为 `Terraria.<生产领域目录>[.<验证目标>]`。领域子目录、验证类型与审计意图都不进路径，
只在 `Test/README.md` 与 `Test/manifest.tsv` 记录；跨边界项目按主要断言对象归属
（`Terraria.NpcSpawn` 留在 Components，`Terraria.Network.OutboundOrdering` 进 Network）。

验证：13 个项目逐个 `dotnet build` 均 0 告警 0 错误，`dotnet run --no-build --no-restore`
全部退出码 0，共 243 条 `PASS`、0 条 `FAIL`，产物位于 `Build/bin/<新项目名>/`，日志在
`Build/diagnostics/test-reorg-20261122/`。本轮只改目录、项目名与 csproj 相对引用，未改任何
`Program.cs` 内容。旧名产物目录仍留在 `Build/bin/` 下，属历史构建残留。

规则与迁移对照见 [Test 验证项目组织](../Test/README.md)，项目索引见
[manifest.tsv](../Test/manifest.tsv)。

## 2026-11-22 NPC 同步包出站优先级隔离（史莱姆抖动）

用户报告服务端权威模式下的史莱姆抖动，并给出方向：出站队列里 Tile 更新包挤压 NPC 同步包。
核对参考客户端 `最新的客户端` 后确认该判断成立：包 23 在 `serverAuthoritative` 时把
`netOffset` 强制置零，`NPC.UpdateNPC` 直接 `FindFrame(); return;`，既无本地 AI 也无插值，
因此画面严格等于最后一次收到的位置快照，包 23 的到达节拍就是抖动源。服务端出站路径此前有两处
跨优先级串行：投递泵只有一个 worker 顺序 await 每次传输写；连接内 normal / realtime 共用一条
发送队列与同一套帧位、字节、准入配额。IPC 之外还有 Tick 内无界遍历（出站意图投影与 Tile 变更
drain），Tile 洪水会把 Tick 拉长，进而推迟下一次位置快照。

落地（详见 docs/cr/CR-2026-11-22-npc-sync-outbound-priority.md）：

- `CommunicationOutboundDeliveryPump` 拆成 realtime / normal 两条独立投递 worker。
- `PacketTcpConnection` 增加 realtime 专用发送队列、帧位信号量、字节预算与准入计数；发送循环
  realtime 先取；`PacketTcpConnectionOptions` 新增 128 KB / 32 帧 / 16 次准入的预留默认值与校验。
- `CommunicationOutboundMessageBufferComponent` 新增 `TryPeek`，`CommunicationOutboundProjectionSystem`
  改为 peek → submit → dequeue，每会话每 tick 至多 256 条，被拒即停止并保持原位。
- `IWorldProjectionSource.DrainPendingMutations(int maximum)` 与 `WorldRuntime` 只取至多 maximum 条，
  其余留在世界侧继续按坐标合并；`WorldNetworkProjectionSystem` 每 tick 至多 128 条。

回归：按用户新布局 `Test/<边界>/<项目>/` 逐个构建并运行全部 13 个验证项目，13/13 退出码 0、无 FAIL 行。
验证：`NSSLC.Network`、`NSSLC.Server` 均 0 error（Server 的 19 条警告全部来自未改动的
NSSLC.WorldGeneration）。新增 `Test/Terraria.Network.OutboundOrdering.Verification`，7 项检查
全部通过、退出码 0，日志 Build/runtime/network-outbound-ordering-2026-11-22.log；同一模型化传输
下 Realtime p50 由随负载 2.07→4.38 ms 变为恒定 2.1–2.9 ms。服务器 --manual-ai 冒烟到
`ecs.server.ready listen=127.0.0.1:7777`，日志 Build/runtime/server-smoke-outbound-2026-11-22.log。

未闭合：需要用户用最新客户端实际观察抖动是否消失；Realtime 配额与 Tile 限流为保守默认值，可按
实际场景再调。验证项目按用户同期改版的 `Test/<边界>/<项目>` 布局落在
`Test/NSSLC.Network/Terraria.Network.OutboundOrdering`，并已登记 `Test/manifest.tsv`。
## 2026-11-22 属性系统与状态效果边界收敛（B0–B9 收口）

本轮把属性系统收敛为「可计算数值的唯一计算者」，资源、冷却、身份与行为状态按 owner 拆到独立组件，
跨域变更统一走请求组件。计划 docs/plans/2026-11-20-attribute-system-execution-plan.md 已从
in-progress 更新为 completed，Inventory 属性贡献与装备来源身份按用户要求保持 deferred。

主要落地：

- 属性分类与准入：AttributeKindMetadata 区分 ComputedScalar / Capacity / Policy / ResourcePool /
  Cooldown / Identity / StatusFlag；资源池、冷却、身份、状态旗标不能作为普通贡献或基础值写入。
- 资源 owner：LifeComponent、ShieldPoolComponent、CooldownComponent、BreathComponent、
  MountStateComponent 各自持有状态；LifeAttributeSynchronizationSystem 是生命上限与护盾容量的
  唯一同步桥，只夹取当前值、不补满。
- Buff 生命周期：BuffApplyRequestComponent → BuffApplyRequestSystem → BuffAdmissionSystem →
  BuffSystem → BuffTickSystem → StatusEffectBehaviorSystem → StatusEffectPeriodicSystem；时长推进
  单点在 BuffListComponent.AdvanceOneTick，BuffSystem.TryApply 只允许 BuffApplyRequestSystem 调用。
- 行为组件：Invisibility / Stun 由行为系统物化为组件，Invisibility 经 NpcAiSnapshotProvider 投影为
  NpcAiTargetFact.IsVisible=false，从而阻断 NPC 索敌与攻击，不需要重复状态字段。
- 跨域请求：周期性效果经 LifeRestoreRequestQueueComponent 与 CombatDamageRequestQueueComponent 在
  Commit 阶段被真实系统消费；护盾消耗唯一外部入口是 AttributeShieldCommands.Apply。
- Combat 能力显式化：CombatDamageSystem 删除可空属性回退，缺失 AttributeSetComponent 时返回
  DamageRejectionReason.MissingAttributeCapability；Player / NPC / Projectile 定义都显式装配能力。

验证：Terraria.Components、NSSLC.Network、NSSLC.Server 均 0 warning / 0 error。回归矩阵按迁移后的 Test 布局覆盖 13 个
验证项目、243 项检查全部通过（Attributes 37、Inventory 16、Entity.Scheduling 10、Projectile 35、
Entity.Definition 6、NpcSpawn 21、ItemUse 15、Combat.ProtectionWindow 5、Chat 27、
WorldItem 42、TileEntity 22、Npc.Ai、Network.OutboundOrdering 7）。git diff --check 退出码 0。服务器以
--manual-ai 启动到 ecs.server.ready listen=127.0.0.1:7777，日志
Build/diagnostics/attribute-system-smoke-2026-11-22b.log。

技术债：StunComponent 尚无移动消费者；BuffApplyRequestComponent 还没有内容侧生产者；NPC 未装配
Buff 接收能力；即时效果未建模；旧枚举键保留但零读写；Inventory 属性桥接 deferred。

工作区事件：2026-11-22 20:35-20:37 期间未跟踪的测试项目被外部进程清除（Terraria.Attributes.
Verification、Terraria.EcsScheduling.Verification、Terraria.Communication.Verification 等）。前两项
已按原契约重建并通过，现纳入 git 索引以防再次丢失；Communication.Verification 未恢复。已跟踪的
测试项目用 git checkout 恢复，其中 Inventory / PlayerItemUse / Combat.ProtectionWindow 的未提交修复
按当时结论重新实现（Inventory 删除装备属性桥接、PlayerItemUse 提供 tick CommandBuffer、
ProtectionWindow 显式装配属性）。
 2026-11-20 ECS Commit/Projection 边界收敛

本轮完成 ECS 运行时边界修复：NPC network identity 在 Commit 阶段通过 tick CommandBuffer
绑定 buffered NPC，slot 分配、discard 清理和 playback 后实体解析保持一致；Communication
Preparation 只消费不可变 ingress 并提交 deferred session/player 创建与关闭，Commit playback
后才对查询和下一阶段可见；shutdown 关闭同样使用独立 CommandBuffer。

WorldItemRequestSystem 已成为 Commit 阶段唯一的 WorldItemStore 写入者，Projection 产生的掉落
请求留到下一 tick；PlayerItemUseSystem 删除无 CommandBuffer 时的直接 World.Add 回退。

验证：Terraria.Components、NSSLC.Network、NSSLC.Server 和通信/NPC spawn/PlayerItemUse/
WorldItem/EntityDefinition/EcsScheduling 验证通过；Chat、Inventory、Projectile、TileEntity、
NpcAi 回归通过。构建均输出到 Build/bin；Npc spawn verifier 的唯一警告是 NuGet 漏洞源不可达
的 NU1900，代码编译与运行均成功。相关计划：thread ownership、command buffer、entity lifecycle
已标记 completed；总计划仍 active，WorldRuntime 与项目配置收窄尚未纳入本轮。

Current source baseline: L0/L1 components plus restored L2 sources, updated on 2026-10-13.

- The active project is `src/NSSLC/Terraria.Components.csproj` (`net10.0`).
- Outside the protected infrastructure directory, source retains the 6 L0 and 62 L1
  components in section 3.1 of the [L0/L1 plan](../docs/plans/2026-10-08-arch-l1-basic-game-components-execution-plan.md),
  plus their 38 enums/value definitions. On 2026-10-13, all 145 L2 component files in the
  [grading list](../docs/reviews/audits/2026-10-08-simple-game-component-grading.csv) were restored
  and moved from `src/NSSLC/Component/` to `src/NSSLC/Components/`, grouped by domain.
  The project includes both source directories. Their source content was preserved;
  this was file organization only, without dependency repair or behavior changes.
- L2 recovery used 41 files from the pre-cleanup snapshot and 104 files from grading commit
  `5bbdd78b78fb5bc8bfd926063de6e3aa542db503`. External reference files under `D:/Component/`,
  including `小众` and `当前不好改的`, were not moved or modified.
- Systems, runtime storage, application/host code, compatibility APIs, generators and their old
  verification projects were removed at the user's explicit request.
- `src/NSSLC.Infrastructure/` is protected in full. Its 2,232 files, including existing local
  modifications and reference code, were verified unchanged. Its old projects are outside the
  active build and may retain references to removed projects.
- The 2026-10-09 L0/L1 component library built with 0 warnings and 0 errors. That historical
  result does not cover the restored L2 files. Per the user's instruction, no compilation,
  tests or other verification were performed for the 2026-10-13 organization change.
- Current scope, evidence and recovery:
  [cleanup record](../docs/cr/CR-2026-10-09-components-only-cleanup.md) and
  [component inventory](../docs/reviews/audits/2026-10-09-components-only-inventory.md).

The following cleared-context card is historical and does not describe the current source baseline.

Context status: cleared

The current migration continuation context was cleared on 2026-09-01 at the user's request.
No active batch, checkpoint, deferred-work queue, or migration resume narrative is retained in
this model-facing card.

The following were intentionally not changed:

- source code and runtime behavior under src/;
- verification programs under Test/;
- project/build policy files;
- historical migration, research, protocol, server-completion, and WorldGen documents;
- Build/diagnostics/ evidence.

For repository build rules, use
[构建与验证约束](约束/构建与验证约束.md).
For general documentation lifecycle rules, use
[dome/dome1/docs/flowstate/README.md](../dome/dome1/docs/flowstate/README.md).
There is no active migration plan/task selected by this card.

This is a context cleanup only. It is not a migration-completion, parity, deletion-gate, or
runtime-behavior claim.

## 2026-11-06 服务器运算系统引入（刷怪 / 世界物品 / Tile 实体 / 物品使用）

本轮在 ECS 服务器中落地四项 Version4 服务器运算能力，并在 `src/NSSLC.Server/Program.cs`
的 Simulation 阶段注册；全部早于 Projection，事实同 tick 可见。

- 世界物品 tick：`Components/WorldItem/WorldItemTickSystem.cs`（合并 → 运动 → 拾取 → 出界退役），
  常量取参考像素/tick 值，不乘 deltaTime。
- NPC 刷怪：`Components/NpcSpawn/NpcSpawnSystem.cs` +
  `Components/NpcSpawn/Solver/NpcSpawn{BudgetRules,AreaRules,Random}.cs`；
  `NSSLC.Network/NpcNetworkIdentitySystem.cs` 分配网络槽位。
- Tile 实体：`Components/TileEntity/WorldTileEntity{RecordDecoder,Store,RequestSystem}.cs`、
  `Components/TileEntity/Model/World{Chest,Sign}State.cs`；`ServerWorldLoader` 读取
  `world.chests` / `world.signs` / `world.tile-entities` 三段。
- 物品使用：`Components/ItemUse/Player/PlayerItemUseSystem.cs` +
  `Components/ItemUse/Content/ItemUseDefinitionCatalog.cs`，门闸、弹药消耗、通道标记。

验证：`dotnet build src\NSSLC.Server\NSSLC.Server.csproj` 0 错误（19 条警告来自未改动的
`NSSLC.WorldGeneration` 既存代码）；
四个验证程序（WorldItem / NpcSpawnEligibility / WorldSession.TileEntity / PlayerItemUse）全 PASS；
`科研.wld` 启动日志 `chests=493 signs=0 tileEntities=0 pylons=0`。

未覆盖：传送塔传送动作、展示人偶/帽架/锚类解码、蓝量消耗、全物品使用表、刷怪生物群系分区。
详见 [服务器运算系统引入记录](../docs/reviews/audits/2026-11-06-server-systems-introduction.md)。

## 2026-11-07 目录重组：四个系统独立成域

`WorldItem/`（14）、`TileEntity/`（15）、`NpcSpawn/`（12）、`ItemUse/`（13）四个领域目录从
`World` / `Npc` / `Player` / `Share/Item` 中独立出来；文件内容与命名空间均未改动，属纯目录迁移。
`Components/World/Model/WorldTileCoordinate.cs`、`WorldTileArea.cs`、`Communication/SessionPlayer*.cs`、
`Share/Inventory/InventoryPickupRequest*.cs` 与 `NSSLC.Network/NpcNetworkIdentitySystem.cs` 按能力/项目
边界留在原处。

验证：`dotnet build src\NSSLC.Server\NSSLC.Server.csproj` 0 错误；WorldItem / NpcSpawnEligibility /
WorldSession.TileEntity / PlayerItemUse 四个验证程序在移动后全部 PASS。
详见 [服务器运算系统引入记录](../docs/reviews/audits/2026-11-06-server-systems-introduction.md#目录重组四个系统各自独立成域2026-11-07)。

## 2026-11-07 聊天系统（独立领域目录 `Components/Chat/`）

新增领域目录 `src/NSSLC/Components/Chat/`（namespace `Terraria.Chat`，16 个文件）：
能力组件 `ChatSenderComponent`（声明即可发言）、`ChatSystem`（校验 → 命令分派 → 广播事实）、
`ChatCommandCatalog` / `ChatCommandDefinition`、`ChatStore`（200 条有界历史）、
`ChatNetTextFrame`（网络模块文本帧）、两个队列（请求/广播）、`Model/` 六个模型、
`Solver/ChatTextRules` 与 `Solver/ChatCommandRules`。

协议：聊天走 `MessageID.NetModules = 82` + `NetTextModule`（模块 id 1，因 `NetworkInitializer`
先注册 `NetLiquidModule`）；出站帧按 `SerializeServerMessage` 布局逐字节对齐（模块 id、作者字节、
字面量 `NetworkText`、RGB）。入站帧布局取自仓库早期协议层的 `TextModuleCodec`（模块 id + `CommandName` + `Text` 两个字符串），玩家昵称仍为占位，二者都需联调确认，已在文档标注。

接线：`GameCommunicationAdapter` 新增 82 分支投递 `ChatMessageRequest`；`PlayerEntityDefinition`
声明 `ChatSenderComponent`；`ChatSystem` 在 Simulation、`ChatNetworkProjectionSystem` 在 Projection
注册；广播只投给 `CommunicationSessionStage.Active` 会话（对齐参考的"已登录"过滤）。

命令：`/say`、`/help`、`/playing`、`/roll`（确定性掷骰），操作员权限位已接通。

验证：`dotnet build src\NSSLC.Server\NSSLC.Server.csproj` 0 错误；
`Test\Terraria.Chat.Verification` 25 项全 PASS；服务器启动到 `ecs.server.ready`。
未联调：真实客户端"输入 → 全服显示"链路。
详见 [聊天系统引入记录](../docs/reviews/audits/2026-11-07-chat-system-introduction.md)。

## 2026-11-09 ECS command buffer deferred entity lifecycle

本轮继续完成 Commit barrier 的实体创建边界：

- `EcsServerCommandBuffer` 增加 after-playback / discard callbacks，并修复多 buffered entity
  解析、创建后销毁、重复 playback、failed playback、disposed 使用和重复 destroy 的语义。
- 新增 `EcsServerEntityCreationTicket` 与 `IDeferredEntityDefinitionHost<TEntity>`；定义宿主在
  playback 后才把真实 Arch entity 绑定到 runtime handle，命令被取消或失败时释放预分配身份。
- `NpcSpawnSystem` 的服务器 tick 路径改为 deferred 创建，playback 后再挂 AI 并发布真实 NPC fact；
  `NpcNetworkIdentitySystem` 调整到 Projection，在 Commit 后为新 NPC 分配 slot。
- `ProjectileLifecycleSystem` 的 command-context 路径返回 `Deferred` 结果并通过 ticket 解析，
  同步调用保持兼容。服务器投射物从下一 Simulation tick 起进入空间驱动。
- 通信 session open 仍保留 Preparation 的同步低层适配器，因为同一 ingress 批次内 open 后的
  packet 需要立即看到 live session；该边界和 shutdown fallback 已补充注释。

验证：`Terraria.Components`、`NSSLC.Network` 构建均 0 警告/0 错误；`NSSLC.Server` 构建 0 错误，
19 条警告全部来自未修改的 `src/NSSLC.Infrastructure/WorldGeneration`。实体定义、共享 API、
ECS scheduling、Communication、Inventory、PlayerItemUse、Projectile、NpcSpawnEligibility 等
验证均通过，`git diff --check` 无 whitespace 错误。

## 2026-11-17 手写 NPC AI：史莱姆主动状态与跳跃计时

手写史莱姆 AI 原先把“找到玩家”直接当作主动追击，导致白天、满血、地表条件下仍持续向玩家
移动，跳跃计时也按主动速度推进。对照 `MinimalImplementation` 的 `NpcManager.Slime.cs` 和
Version4 的 `NPC.AI_001_Slimes` 后，现按昼夜、当前/最大生命、相对世界地表位置计算主动状态；
落地持续减速，普通状态每帧计时一次、主动状态两次，空中仍朝目标转向。World runtime 的初始
昼夜标记从已加载世界读取。史莱姆雨状态尚未接入运行时，当前手写快照将其明确设为 false。

补充验证覆盖安静落地不追走、空中转向、夜间/受伤/地表下/史莱姆雨主动条件、普通跳速度、
抖动与穿地回归，以及史莱姆免伤和向导仍可受伤。服务器构建 0 错误，19 条既存警告来自未修改
的 `NSSLC.WorldGeneration`；手写 AI 和共享 AI 验证程序均通过。服务器已用新构建重启于 `127.0.0.1:7777`
（PID 19764，`ai=manual`）。史莱姆实体定义已恢复为原生 25/25 并保留免伤。Version4 客户端不识别
服务器权威扩展位，原版客户端画面验收仍待完成。
详见 [NPC AI / 伤害验证 CR](../docs/cr/CR-2026-10-22-npc-ai-damage-test.md) 和
[任务记录](../.agent-workplace/docs/task/2026-10-22-npc-ai-damage-test.md)。
