# Project Progress Summary

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
