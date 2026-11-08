# 聊天系统引入记录

日期：2026-11-07
范围：`src/NSSLC/Components/Chat/**`、`src/NSSLC/Components/Communication/GameCommunicationAdapter.cs`、
`src/NSSLC.Network/ChatNetworkProjectionSystem.cs`、`src/NSSLC.Server/Program.cs`、
`src/NSSLC.Server/Registration/PlayerEntityDefinition.cs`、`Test/Terraria.Chat.Verification`

本文记录文字聊天在 NLTX ECS 中的落地形态。断言分两类：**已构建/已运行**（有命令与输出）与
**静态检视/推导**（读过参考代码或推导了布局，未用真实客户端联调）。

## 1. 领域目录

独立领域目录 `src/NSSLC/Components/Chat/`，`namespace Terraria.Chat`：

| 文件 | 角色 |
| --- | --- |
| `ChatSenderComponent.cs` | 能力组件：声明它的实体就可以发送文字聊天 |
| `ChatSystem.cs` | 校验 → 命令分派 → 广播事实（`ChatMessage` / `ChatCommandProcessor` 的服务器侧） |
| `ChatCommandCatalog.cs` | 封存后只读的命令表 |
| `ChatCommandDefinition.cs` | 命令名、说明与操作员权限位 |
| `ChatStore.cs` | 有界历史窗口（容量 200） |
| `ChatNetTextFrame.cs` | 文本模块帧的编解码 |
| `ChatMessageRequestQueue*.cs` | Communication → Chat 的意图队列 |
| `ChatBroadcastQueue*.cs` | Chat → Network 的广播事实队列 |
| `Model/ChatChannel.cs` | Say / Party / Announce / Server / Private |
| `Model/ChatSpeakerKind.cs` | Player / Npc / System |
| `Model/ChatColor.cs` | RGB 三元组与参考 `ChatColors` 预设（取值逐字节对齐） |
| `Model/ChatMessage.cs` | 历史条目 |
| `Model/ChatMessageRequest.cs` | 一条待判定的输入 |
| `Model/ChatBroadcast.cs` | 一条待投递的输出（含 `OnlyFor` 私有回复） |
| `Solver/ChatTextRules.cs` | 归一、单行化、控制字符剔除、长度与命令拆分 |
| `Solver/ChatCommandRules.cs` | 帮助/在线数格式化与确定性 `/roll` |
| `Solver/ChatChannelRules.cs` | 文本命令名 → 频道（`Say` / `Party`），未知命令名拒绝 |

**能力组件即准入**：`ChatSystem` 只检查说话实体是否声明 `ChatSenderComponent`，不查实体类别、
会话状态或 AI。玩家定义（`PlayerEntityDefinition`）声明该组件，因此玩家天然可发言。

## 2. 参考来源与移植点

| 参考位置 | 采纳内容 |
| --- | --- |
| `Terraria.Chat.ChatMessage` | 一条聊天消息 = 一段文本 + 一个命令标识 |
| `Terraria.Chat.ChatColors` | `BossOrEvent(175,75,255)`、`World(50,255,130)`、`NPCTravel(50,125,255)`、`ServerMessage(255,240,20)`、`Death(255,25,25)` 原值 |
| `Terraria.Chat.ChatHelper.BroadcastChatMessageAs` | 广播对象是"已登录"玩家（`Netplay.Clients[i].State == 10`）；作者字节 255 表示非单一玩家 |
| `Terraria.Chat.ChatCommandProcessor` / `ChatCommandAttribute` | 命令以名字注册、可带别名；服务器侧保留名字 + 权限位 |
| `Terraria.Chat.Commands.*` | 命令集合命名：`SayChatCommand`、`HelpCommand`、`ListPlayersCommand`、`RollCommand` |
| `Terraria.ID.MessageID.NetModules = 82` | 聊天不是独立 message id，而是网络模块包 |
| `NetworkInitializer` 注册顺序 | `NetLiquidModule`=0、`NetTextModule`=1，故文本模块 id 是 1 |
| `NetTextModule.SerializeServerMessage` | 出站帧 = 模块 id(ushort) + 作者字节 + `NetworkText`(字面量：7 位长度 + UTF-8) + RGB 三字节 |
| `NetworkText.Serialize` | 模式字节 + 文本 + 替换表；服务器只发字面量模式，替换表为空 |
| 本仓库早期的 `Protocol/Version4ModuleCodecs.cs`（工作树已删除，`git show HEAD:` 可查） | **入站**文本帧 = 模块 id + `CommandName` 字符串 + `Text` 字符串；出站 = 作者字节 + `NetworkText` + RGB，与本次出站编码一致 |

## 3. 参考 dump 未保留、按仓库先例推导的部分（未联调）

- **客户端 → 服务器文本帧布局**：`NetTextModule.Deserialize` 在 dump 中被裁剪（只剩
  `return default`），所以入站布局取自本仓库早期协议层的 `TextModuleCodec.Read`：模块 id 之后是
  **两个** 7 位长度前缀 UTF-8 字符串（`CommandName`、`Text`）。`ChatNetTextFrame.TryDecodeClientMessage`
  按此解码，`ChatChannelRules` 再把 `Say`/`Party` 映射成频道；这是**仓库先例**而非 dump 证据，
  真实客户端联调仍需确认。
- **服务器侧命令执行**：dump 里只有客户端 `ChatCommandProcessor`，没有服务器分派器。命令名统一
  小写，`/say`、`/help`、`/playing`、`/roll` 由 `ChatSystem` 自己实现。
- **玩家昵称**：玩家名在 player-data 包中，该包布局不在 dump 内。当前 `ChatSenderComponent` 以
  `"Player"` 占位（`FallbackName`），等 packet 4 解码后写入即可，Chat 域不用改。
- **作者字节**：NLTX 还没有"实体 → 玩家索引"映射，因此所有出站行使用 255（服务器文本），并用
  "昵称: 文本"自行拼名字；等玩家索引建模后改为真实索引，客户端就会自己显示玩家名。
- **队伍频道**：协议里的 `Party` 命令名会被识别，但队伍状态尚未建模，`ChatSystem` 对
  `ChatChannel.Party` 只回一条私有说明，绝不退化成公开广播。
- **长度上限**：`ChatTextRules.MaximumLength = 512` 是**服务器策略**，不是参考常量（参考只约束
  客户端编辑框）。

## 4. 接入与注册

- `Program.cs` 创建 `ChatMessageRequestQueueComponent` 与 `ChatBroadcastQueueComponent` 两个世界级
  队列实体，并在 Simulation 阶段注册 `ChatSystem`、在 Projection 阶段注册
  `ChatNetworkProjectionSystem`（紧邻 `NpcNetworkProjectionSystem`）。
- `Program.cs::CreateChatCommands()` 注册 `/say`、`/help`、`/playing`、`/roll` 并封存表。
- `GameCommunicationAdapter` 新增 `NetModulesMessageId = 82` 分支：解出文本后投入 Chat 请求队列；
  实体没有 `ChatSenderComponent` 时直接丢弃。
- `PlayerEntityDefinition` 为玩家声明 `ChatSenderComponent.Player()`。
- `ChatNetworkProjectionSystem` 把广播事实编码为文本模块包，按 `OnlyFor` 定向或广播给
  `CommunicationSessionStage.Active` 的会话（对应参考的"已登录"过滤）。

## 5. 验证证据

| 命令 | 结果 |
| --- | --- |
| `dotnet build src\NSSLC.Server\NSSLC.Server.csproj -v q --nologo` | 0 错误 |
| `dotnet run --project Test\Terraria.Chat.Verification` | PASS（27 项：文本规则、命令拆分、文本命令名→频道、帧逐字节布局、入站双字符串解码与拒绝路径、能力组件准入、公开/私有投递、队伍频道私有回绝、操作员权限、确定性掷骰、历史窗口） |
| `dotnet Build\bin\NSSLC.Server\Debug\net10.0\NSSLC.Server.dll src\World\科研.wld` | 启动到 `ecs.server.ready`（`ecs.world.tile-entities` / `ecs.npc.spawned` 正常），聊天域接线不影响加载 |

尚未用真实 Version4 客户端跑过"输入文字 → 全服显示"的端到端链路：入站帧布局来自仓库先例、
作者字节仍是 255，两者都需要联调确认，联调前不应视为完整。

## 6. 后续待办

- 用真实客户端校验入站文本帧，必要时修正 `ChatNetTextFrame.TryDecodeClientMessage`；
- 解码 player-data 包，把昵称写入 `ChatSenderComponent`，并建立实体 → 玩家索引映射以改用真实作者字节；
- 队伍频道、`/kick` 等管理命令需要队伍与管理动作域先行；
- 聊天历史对客户端的分发（进服时补发最近 N 条）需要客户端侧文本模块订阅支持。
