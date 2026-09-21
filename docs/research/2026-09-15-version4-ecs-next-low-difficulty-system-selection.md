# Version4 下一步 ECS 拆分方向调研报告：选择 System

日期：2026-09-15
调研对象：`D:\TRbackup\Version4`
报告仓库：`D:\TRbackup\NLTX`
结论状态：代码阅读和架构调研完成；本报告不声明 Version4 运行时迁移完成。

## 1. 结论

下一步选择 **System**，首个低难度行为切片选择：

> `FixExploitManEaters` 的“每个世界 Tick 重建 Man Eater 保护 Tile 坐标索引，并在 Tile 破坏的掉落路径上执行保护判断”行为。

这不是选择整个 `WorldGen`、整个 NPC AI 或整个 Tile 系统，而是选择一个已经闭合的跨模块行为切片：

```text
Main.DoUpdateInWorld
  -> FixExploitManEaters.Update()       # 清空本 Tick 索引
  -> NPC.UpdateNPC()
  -> aiStyle == 13 的 NPC 记录锚点
  -> WorldGen.KillTile(i, j)
  -> FixExploitManEaters.SpotProtected(i, j)
  -> 命中且允许掉落时提前返回
```

选择理由：

- 有真实的主循环入口、唯一清理点、唯一当前写入点和唯一当前读取点。
- 保护状态只有坐标集合，属于短生命周期空间工作状态，不进入存档或网络快照。
- 规则本身是幂等集合插入和纯成员查询，容易做 focused verifier 和差分验证。
- 不需要引入 Command 作为第一步。当前行为不是外部意图，也不是结构性实体变更；NPC 和 Tile 调用方通过兼容 facade 访问 System 即可。
- 相比 Chat/Debug Command，避免了解析、权限、文件、聊天、网络和反馈副作用；相比天气、压力板、传送柱等 System，避免了随机、持久化、TileEntity registry 或表现链路。

推荐置信度：**高（针对保护索引切片）**；不是对完整 Man Eater AI 或完整 Tile 破坏系统的行为等价承诺。

## 2. 调研范围和证据等级

证据标签：

- `confirmed`：目标源码、仓库文件或本地 API 文档直接证明。
- `corroborated`：多个独立源码位置相互印证。
- `inferred`：根据已确认读写和生命周期推导的架构判断。
- `proposed`：尚未接入 Version4 生产调用链的目标设计。
- `unknown`：当前材料不能证明，必须保留为阻塞项。

已读取和核对：

| 来源 | 用途 | 结果 |
| --- | --- | --- |
| `D:\TRbackup\NLTX\AGENTS.md`、`Context/progress.md` | 仓库、构建、迁移和文档约束 | `confirmed`；当前没有 active migration plan/task；工作树有大量既有改动 |
| `D:\TRbackup\NLTX\Context/架构设计\ECS文件组织设计约束.md` | 领域优先、文件命名、迁移记录和验证要求 | `confirmed` |
| `D:\TRbackup\NLTX\Context/架构设计\组件命名设计约束.md` | Component 类型、文件和注册身份约束 | `confirmed` |
| `D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\ecs-evidence-protocol.md` | 成员、读写者、生命周期和证据门槛 | `confirmed` |
| `D:\TRbackup\Version4` | 唯一完整目标源码基线 | `confirmed` |
| `D:\TRbackup\tmodloader-api-docs-stable` | 公开 API 形状和参数语义 | 本地首页页眉显示 `tModLoader v2026.07`；类型页标题为 `tModLoader: ... Class Reference`，因此只用于公开签名和摘要，不用于私有行为证明 |
| `C:\Users\shan\Downloads\ECS\space-station-14-master` | ECS 组织模式参考 | `confirmed` 组织模式；不作为 Terraria 行为事实 |
| `D:\TRbackup\NLTX\docs\migration\ledgers` 和既有第一/二轮报告 | 既有拆分、账本和原型状态 | `confirmed`；账本结构正确不等于生产迁移完成 |

## 3. 阅读覆盖和口径

### 3.1 Version4 全量静态覆盖

采用 `Get-ChildItem -Recurse -File -Filter '*.cs'`，排除 `bin`/`obj` 后统计；采用 `Get-Content` 行数与 `ReadAllText` 的换行统计交叉核对：

| 指标 | 数值 |
| --- | ---: |
| Version4 C# 文件 | 967 |
| 源码总行数 | 431,364 |
| 源码总字节数 | 10,965,205 |
| `Version4源码覆盖.tsv` 文件记录 | 967 |
| 覆盖表与源码文件差异 | 0 |
| 文件覆盖率 | 100% |
| 60% 行数门槛 | 258,819 行 |

因此本次满足“整体架构和细节处代码阅读量达到 60% 以上”的**静态阅读覆盖门槛**：全树 967/967 文件已建立覆盖索引，并对目标架构、候选类型、调用者、消费者和参考模式做了语义细读。`Version4源码覆盖.tsv` 是覆盖索引，不应被解释为每个文件都已人工逐行等价理解；报告对语义结论仍只引用实际闭环证据。

### 3.2 语义细读范围

本次细读重点包括：

- `Main.DoUpdateInWorld` 的玩家/NPC/世界 Tick 顺序和 `FixExploitManEaters.Update` 位置。
- `NPC.UpdateNPC` 的 `aiStyle == 13` 分支、坐标边界、Tile 初始化、失活分支和 `ProtectSpot` 调用。
- `WorldGen.KillTile` 的坐标边界、Tile 初始化、可破坏性、`isGeneratingOrLoadingWorld`、`effectOnly`、`stopDrops`、`noItem` 和保护判断位置。
- `FixExploitManEaters` 全部字段和三个公共方法，以及全树所有调用者。
- `AmbientWindSystem`、`ChatCommandProcessor`、`DebugCommandProcessor`、`PressurePlateHelper`、`TeleportPylonsSystem`、`SmartInteractSystem`、`BackgroundChangeFlashInfo`、`TownRoomManager`、`BannerSystem` 等候选的入口、空方法和副作用边界。
- NLTX `src`/`src2` 的领域目录、Component/System/Query/Command/Adapter/Projection 组织和已有 Man Eater 原型。
- SS14 `WiresComponent`、`WiresSystem`、`SharedWiresSystem`、`AnnounceCommand` 和 `WireLayoutTest` 的组织/验证模式。
- tModLoader `Main.LocalPlayer`、`Player.ZoneGraveyard`、`WorldGen.KillTile` 的本地 Doxygen 页面和实际成员锚点。

## 4. Version4 目标行为闭环

### 4.1 `FixExploitManEaters` 成员盘点

源码：`D:\TRbackup\Version4\Terraria.GameContent\FixExploitManEaters.cs:1-34`。

| 成员 | 读者 | 写者 | 生命周期 | 状态类型 | 结论 |
| --- | --- | --- | --- | --- | --- |
| `IndexesProtected : List<int>` | `SpotProtected` | `Update` 清空；`ProtectSpot` 添加 | `Main.DoUpdateInWorld` 中一次清空，随后供同 Tick NPC/Tile 逻辑使用 | transient spatial index | `confirmed`；候选 `ManEaterProtectionIndexComponent` |
| `Update()` | `Main.DoUpdateInWorld:11470` | 清空 `IndexesProtected` | 玩家阶段之后、NPC 阶段之前调用 | System 生命周期入口 | `confirmed`；候选 `ManEaterProtectionSystem.BeginWorldTick` |
| `ProtectSpot(int x, int y)` | `NPC.UpdateNPC` | 向集合添加压缩坐标 | NPC AI 运行期间重复调用 | System 写操作 | `confirmed`；重复坐标幂等 |
| `SpotProtected(int x, int y)` | `WorldGen.KillTile` | 无 | Tile 破坏进入掉落判断时查询 | Query/guard | `confirmed`；不得在查询中写状态 |
| `Pack(x,y)` 逻辑 | 上述两个方法 | 无独立状态 | 调用期间计算 | 纯派生值 | `confirmed`；必须保持 16-bit masking 和 `int` 位布局 |

原始实现的关键代码语义：

```text
item = ((x & 0xFFFF) << 16) | (y & 0xFFFF)
if (!IndexesProtected.Contains(item))
    IndexesProtected.Add(item)
return IndexesProtected.Contains(item)
```

### 4.2 主循环顺序

`D:\TRbackup\Version4\Terraria\Main.cs:11411-11526` 证明：

1. 更新玩家 `player[i].Update(i)`（`11422-11447`）。
2. 计算当前帧玩家计数并增加 `_gameUpdateCount`（`11448-11450`）。
3. 调用 `FixExploitManEaters.Update()` 清空保护集合（`11470`）。
4. 遍历 `npc[l].UpdateNPC(l)`（`11505-11525`）。
5. NPC 阶段之后继续更新 Projectile 等世界对象。

因此迁移必须保留：

```text
玩家阶段 -> 清空保护索引 -> NPC 锚点登记 -> 后续 Tile/Projectile/Wiring 破坏查询
```

不能把清空操作不加证明地提前到整个世界 Tick 的最开始。当前源码没有证明玩家阶段是否会使用该保护索引；对此最保守的行为保持方案是保持旧入口相同的相对位置。

### 4.3 NPC 写入路径

`D:\TRbackup\Version4\Terraria\NPC.cs:21459-21477`：

- 仅在 `aiStyle == 13` 分支进入保护逻辑。
- 先检查 `ai[0]` 和 `ai[1]` 是否小于 0 或超出 `Main.maxTilesX`；越界直接返回。
- 空 Tile 被初始化为 `new Tile()`。
- Tile 非 active 时，NPC 设置 `life = -1`、执行 `HitEffect()`、设置 `active = false` 并返回。
- 只有 Tile active 时才执行 `FixExploitManEaters.ProtectSpot((int)ai[0], (int)ai[1])`。

这意味着新 System 不应复制“所有 aiStyle=13 NPC 都保护坐标”的粗略规则；NPC 的坐标范围、Tile 初始化和 active 检查仍属于 NPC/Tile 上游行为，保护 System 只接收已经通过上游条件的坐标。

### 4.4 Tile 消费路径

`D:\TRbackup\Version4\Terraria\WorldGen.cs:52561-52606`：

- `KillTile` 先做世界边界检查、Tile 初始化、active 检查和 breakability 检查。
- 世界生成/加载时将 `noItem = true`（`52592-52595`）。
- 只有 `!effectOnly && !stopDrops` 时进入掉落相关保护段（`52596`）。
- 只有 `!noItem && FixExploitManEaters.SpotProtected(i,j)` 时提前 `return`（`52598-52601`）。

精确结论是：保护命中时，Version4 在该条件下直接终止 `KillTile` 的后续逻辑；不是单纯把掉落物数量改成零。若 `effectOnly`、`stopDrops` 或 `noItem` 阻止进入/触发该判断，则不应人为扩大保护范围。

同一 `KillTile` 有多类调用者，包括 `Player.Spawn_ForceClearArea`、NPC AI、Wiring 和 Projectile。全树搜索只发现一处 `SpotProtected` 读取，因此这些调用者都通过同一个 Tile 入口间接受影响。

### 4.5 全树调用闭包

全树搜索结果：

```text
Main.cs:11470       FixExploitManEaters.Update()
NPC.cs:21476        FixExploitManEaters.ProtectSpot(...)
WorldGen.cs:52598   FixExploitManEaters.SpotProtected(...)
FixExploitManEaters.cs 内部声明和实现
```

没有发现第二个清理入口、第二个保护集合写者或第二个保护查询者。该事实支持把它作为一个小型 System/Index/Query 切片迁移；它不证明世界卸载、线程亲和性或未来 mod hook 语义已经闭合。

## 5. 成员归属和目标边界

以下为目标设计，尚未替换 Version4 生产调用链。

| 目标模块 | 归属 | 读集 | 写集/输出 | 不负责 |
| --- | --- | --- | --- | --- |
| `ManEaterProtectionIndexComponent` | Component / transient spatial state | 无 | 当前保护坐标集合 | NPC AI、Tile、存档、网络、线程 |
| `ManEaterProtectionSystem` | System / lifecycle and mutation | `ProtectionIndex`、已验证 Tile coordinate | `Clear`、幂等 `Protect` | 不读取 NPC，不决定 aiStyle，不销毁 Tile |
| `ManEaterProtectionQuery` 或 `SpotProtectionQuery` | Query / pure membership | Index + `(x,y)` | `bool` 或明确结果 | 不清空、不添加、不写全局状态 |
| `NpcManEaterProtectionAdapter` | Adapter / compatibility seam | NPC 已确认的锚点坐标 | 调用 System 的 protect 接口 | 不重新判断 NPC 语义 |
| `TileBreakProtectionAdapter` | Adapter / legacy facade | Tile break context + Index | 将 query 结果映射成 `Blocked`/`Continue` | 不改变 `KillTile` 的其他参数 |
| `ManEaterProtectionResetSystem` | 可与上述 System 合并的阶段入口 | world tick phase | 一次清空 | 不依赖文件/目录顺序定义阶段 |
| Snapshot/Projection | 暂不创建 | 无 | 无 | 该状态不持久化、不网络复制 |
| Command | 第一批不创建 | 无 | 无 | `ProtectSpot` 是内部规则写入，不是外部意图 |

### 5.1 推荐文件落位

遵循“能力/领域优先”和“小领域扁平化”：

```text
src/WorldInteraction/
  ManEaterProtectionIndexComponent.cs
  ManEaterProtectionSystem.cs
  ManEaterProtectionQuery.cs       # 只有查询复杂度需要独立文件时创建
  ManEaterProtectionAdapter.cs      # 兼容入口；若已有兼容边界则复用
```

如果生产项目的当前组织仍采用 `src2/WorldInteraction/Components/`，应先记录 source/target 和项目边界，再决定是否把 System 从 `Components` 子目录移出。System 不应长期放在 `Components` 目录中；当前 `src2` 原型属于已有历史工作，不应借本报告未经审查地重命名或移动。

推荐类型和注册身份：

- `ManEaterProtectionIndexComponent`：表达“临时空间保护索引”，名称符合组件命名约束。
- `ManEaterProtectionSystem`：表达行为和唯一写者。
- `ManEaterProtectionQuery`：仅当调用边界需要独立 Query 类型时使用；否则可以将纯成员判断作为 System 的只读方法，避免过度拆分。
- 不使用 `NpcComponent`、`WorldDataComponent`、`SharedComponent`、`ManagerComponent` 或 `FixExploitManEatersComponent` 作为泛化收容名称。

## 6. System 顺序和所有权规则

最小目标调度图：

```text
WorldTick.PlayerUpdate
  -> ManEaterProtectionSystem.BeginWorldTick
  -> WorldTick.NpcUpdate
       -> NpcManEaterProtectionAdapter.ProtectSpot
  -> WorldTick.TileBreak / Wiring / Projectile / Player clear-area
       -> ManEaterProtectionQuery.IsProtected
       -> TileBreak legacy implementation
  -> WorldTick.WorldReset / Unload
       -> clear and release transient index
```

规则：

1. `ManEaterProtectionIndexComponent` 只有一个写者：`ManEaterProtectionSystem`。
2. `BeginWorldTick` 必须先清空，再允许 NPC 写入；不允许用旧集合兜底。
3. `IsProtected` 必须是只读查询；重复保护必须幂等。
4. `(x,y)` 的压缩和解压/比较必须集中在一个类型或方法中，避免多个实现产生碰撞差异。
5. 不得把保护坐标写入世界存档、网络快照、NPC 永久组件或共享天气状态。
6. 任何必须早于/晚于本 System 的主循环接入都要显式记录 phase/barrier；不能用文件顺序、注册枚举顺序或目录顺序代替。
7. 世界卸载、异常清理和多世界并行语义当前为 `unknown`；在生产接入前必须明确索引是单世界实例、世界实体组件还是主机级状态。

## 7. 当前 NLTX 原型状态

已经存在的工作树原型：

- `D:\TRbackup\NLTX\src2\WorldInteraction\Components\ManEaterProtectionIndexComponent.cs`
- `D:\TRbackup\NLTX\src2\WorldInteraction\Components\ManEaterProtectionSystem.cs`
- `D:\TRbackup\NLTX\src2\WorldSessionVerification\Program.cs:162-171`
- `D:\TRbackup\NLTX\src2\WorldSession\Terraria.WorldSession.csproj:10-11`

原型已表达并验证：

- `HashSet<int>` 作为去重索引。
- `BeginFrame` 清空。
- `ProtectSpot` 重复写入不产生重复成员。
- `SpotProtected` 可读查询。
- 清空后上一帧坐标不可查询。

但它目前不能被写成生产迁移完成：

- `src2` 原型没有接入 `D:\TRbackup\Version4\Terraria\Main.cs`、`NPC.cs` 或 `WorldGen.cs` 的实际运行调用链。
- 原型的命名空间和项目链接属于现有工作树状态，不能推断正式 `src` 生产项目已经注册或接入。
- 当前 verifier 只验证索引局部行为，不验证 `KillTile` 的 `effectOnly/stopDrops/noItem` 条件、NPC Tile active 前置、主循环顺序、异常清理或差分结果。
- 既有 P01 文档把该原型记录为 Component checkpoint；该记录和本报告都不替代生产行为等价验证。

## 8. Command 与 System 候选比较

评分：每项 0-5，越高越适合下一低难度批次。维度为责任内聚、调用闭环、状态所有权、读写闭合、副作用隔离、顺序风险、外部依赖、验证清晰度、与现有报告重复度、可复用边界。总分为调研判断，不是运行时指标。

| 候选 | 类型 | 主要证据 | 总分 | 结论 |
| --- | --- | --- | ---: | --- |
| `FixExploitManEaters` 保护索引切片 | System | `FixExploitManEaters.cs:5-34`；`Main.cs:11470`；`NPC.cs:21459-21476`；`WorldGen.cs:52596-52601` | **46/50** | **首选**；闭环小，副作用少 |
| `AmbientWindSystem` 当前资格/节拍切片 | System | `Main.cs:11637`；`AmbientWindSystem.cs:15-45` | 34/50 | 关键 helper 为空，完整行为缺证据；暂缓 |
| `BackgroundChangeFlashInfo.UpdateFlashValues` | System | `BackgroundChangeFlashInfo.cs` 及 Main 调用 | 32/50 | 触发写者和消费者未闭合 |
| `ChatCommandProcessor` / Emoji command 链 | Command/Adapter | `ChatCommandProcessor.cs`；各 ChatCommand | 25/50 | 解析和入站处理为空，调用链不闭合 |
| `DebugCommandProcessor` | Command + Adapter | `DebugCommandProcessor.cs:15-98` | 22/50 | 权限、反射、memo 文件、聊天/网络反馈混杂 |
| `PressurePlateHelper` | System | `PressurePlateHelper.cs`；Player/Wiring/WorldFile | 21/50 | Tile、玩家数组、存档、Wiring 和空效果方法跨边界 |
| `SmartInteractSystem` | System/Query | `SmartInteractSystem.cs`；ObjectInteractions 调用者 | 20/50 | 目前主要是 provider 注册，运行入口未闭合 |
| `TeleportPylonsSystem` | System + Projection | `TeleportPylonsSystem.cs`；TileEntity/NetManager | 17/50 | registry、网络广播、视觉刷新顺序风险高 |
| `BannerSystem` | System + Network/Persistence | Banner/NPC/Net/WorldFile 调用链 | 14/50 | 规模和跨域副作用不适合第一批 |

为什么不选 Command：当前 Version4 可见的 Chat Command 多数 `ProcessIncomingMessage` 为空；`ChatCommandProcessor.CreateOutgoingMessage` 返回 `default`，入站处理为空。`DebugCommandProcessor` 虽有命令形状，但同时依赖反射、权限、文件路径、聊天回复和网络广播。它们都不能在本次证据下提供低风险、完整闭环的独立 Command。

## 9. 参考项目和 tModLoader 证据

### 9.1 Space Station 14：只借组织模式

已直接读取：

- `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Server\Wires\WiresComponent.cs`
- `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Server\Wires\WiresSystem.cs`
- `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\Wires\SharedWiresSystem.cs`
- `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Server\Announcements\AnnounceCommand.cs`
- `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.IntegrationTests\Tests\Wires\WireLayoutTest.cs`

可迁移的组织经验：

- `WiresComponent` 保存实体的布局、线状态和队列等事实；它不负责执行完整交互流程。
- `WiresSystem` 通过 `SubscribeLocalEvent` 处理生命周期和交互事件，并把查询/写入行为集中到 System。
- `AnnounceCommand` 负责参数数量、颜色和资源路径校验，再调用聊天服务；Command 是边界入口，不是把服务引用塞进 Component。
- `WireLayoutTest` 以实际 System、实体原型和创建流程验证行为，而不是只验证类型能否编译。

对本方向的影响：把坐标集合放在 Component，把清空/插入放在 System，把保护判断放在只读 Query，把旧 `FixExploitManEaters` API 留作 facade，符合组织模式；但 SS14 不能证明 Terraria 坐标编码或 `KillTile` 早退语义。

### 9.2 tModLoader 本地文档

本地镜像：`D:\TRbackup\tmodloader-api-docs-stable`。

- `class_main.html` 的 `Main.LocalPlayer` 成员条目说明它返回本地用户 Player，且文档给出与 `Main.player[Main.myPlayer]` 的关系。
- `class_player.html` 的 `Player.ZoneGraveyard` 条目确认公开属性签名为 `bool ZoneGraveyard { get, set; }`。
- `class_world_gen.html#aab8789310665bd013cbc7cbc955137ed` 的 `WorldGen.KillTile(int i, int j, bool fail=false, bool effectOnly=false, bool noItem=false)` 条目说明 `effectOnly`、`noItem` 等公开参数语义。

这些公开文档只确认公开类型/成员和参数摘要，不能证明 Version4 私有 `FixExploitManEaters`、主循环顺序或保护集合，因此本报告的行为结论优先采用 Version4 源码。

## 10. 风险和未决问题

必须在生产接入前关闭：

| 风险 | 当前状态 | 关闭条件 |
| --- | --- | --- |
| 主循环阶段改变 | `confirmed` 旧顺序；迁移接入未做 | 验证玩家阶段、清理、NPC 阶段、Tile/Projectile/Wiring 查询的相对顺序 |
| 坐标压缩差异 | `confirmed` 旧实现使用两个低 16 位 | 针对负值、`65535`、越界输入和碰撞样例做表格化差分；不要改为未经验证的 tuple/hash |
| `KillTile` 条件扩大或缩小 | `confirmed` 保护只在 `!effectOnly && !stopDrops && !noItem` 段内触发 | 对四种上下文和 `isGeneratingOrLoadingWorld` 做 focused verifier |
| NPC 上游资格被重复实现 | `confirmed` 上游有范围、Tile active 和失活逻辑 | Adapter 只接收已通过上游判断的坐标，不重复复制 NPC AI |
| 单/多世界所有权 | `unknown` | 明确索引实例绑定 world/session/entity 的方式 |
| World unload/异常路径 | `unknown` | 增加清空和释放验证；异常后不得沿用上一帧集合 |
| 现有原型与生产项目关系 | `proposed/partial` | 生产项目接入、注册、构建和 runtime smoke 完成前保持 facade |
| 多线程/并行 | `unknown` | 主循环线程亲和性或 ECS 调度约束必须显式写入 phase contract |

不在第一批处理：完整 Man Eater AI、NPC 组件重构、Tile 破坏重写、网络同步、世界存档、随机、渲染、掉落系统泛化。

## 11. focused verifier 计划

先写验证，再接生产入口；验证程序建议放在受影响的 `WorldInteraction`/`WorldSession` 领域测试项目，不把生成物放入源码目录。

最小用例：

1. 空索引查询返回 false。
2. `ProtectSpot(x,y)` 后查询返回 true。
3. 同坐标重复保护仍只有一个逻辑成员。
4. `BeginWorldTick` 清空上一 Tick 坐标。
5. 不同坐标分别可查且不互相命中。
6. 使用 Version4 的 16-bit masking 样例验证压缩兼容性。
7. `KillTile` context：`effectOnly=true`、`stopDrops=true`、`noItem=true`、世界生成/加载时不错误扩大保护。
8. active Tile、空 Tile、越界 NPC 锚点分别保持 Version4 上游行为。
9. 事件顺序：clear -> NPC protect -> Tile query；在错误/异常后下一 Tick 不残留旧集合。
10. 旧 facade 与新 System 的同输入差分：返回值、调用次数和保护状态一致。

删除旧 facade 的门禁：

- 生产主循环已由显式 System phase 调度，且顺序 verifier 通过。
- NPC 和 Tile 两个生产调用链已通过同一 Index/Query 闭合。
- focused verifier 通过；至少一次 runtime smoke 或等价集成测试通过。
- 无第二个写者、无第二个查询实现、无持久化/网络误注册。
- 账本 decision、source/target path、dependency impact、回滚路径和验证状态同步更新。
- 旧入口保留一个可观测的兼容或回滚路径，直到删除评审通过。

## 12. 实施顺序、回滚和验证记录

### 12.1 建议实施顺序

1. 以本报告为设计基线，登记 `FixExploitManEaters` 三个成员和调用点的 source/target/dependency impact。
2. 保留旧 `FixExploitManEaters` facade，先让它委托到唯一 System/Index，不改变调用方签名。
3. 写并运行纯 Index/System verifier。
4. 接入 `Main` 清理阶段、NPC protect adapter 和 `WorldGen` query adapter。
5. 做旧实现与新实现的差分/调用顺序验证。
6. 只在删除门禁全部满足后，才评估移除旧静态集合和旧 facade。

### 12.2 回滚

- 回滚粒度为整个 Man Eater protection slice；不回滚无关组件或其他用户工作树改动。
- 保留旧 facade，使运行入口可以切回旧 `IndexesProtected` 实现。
- 如果清理失败、阶段顺序无法证明或 query 条件出现差异，禁止删除旧实现，报告为 `blocked`/`deferred`，而不是静默接受行为变化。

### 12.3 本次实际验证

本次只新增本报告，未执行 compile-capable `dotnet` 命令，因此不声称构建/测试通过。

已完成的只读核验：

- `FixExploitManEaters` 全树调用搜索：仅 1 个清理调用、1 个保护写调用、1 个保护读调用。
- Version4 源码文件统计：967 文件、431,364 行、10,965,205 字节。
- `Version4源码覆盖.tsv` 与 Version4 源码清单：967 对 967，缺失 0。
- 目标源码行号、公开 API 文档成员锚点、SS14 参考文件和 NLTX 原型路径均已核对。

后续若实现代码，必须遵守根 `AGENTS.md` 的串行构建命令，记录精确 command、project、exit code、warning/error counts 和 `Build/bin/` artifact path；本报告不把现有原型 verifier 的历史记录升级为本次生产验证。

## 13. 最终建议

立即进入 `System` 方向，但批次必须限定为 **Man Eater protection index**，不要扩大为“NPC 系统拆分”或“Tile 系统拆分”。Command 方向暂缓，直到出现一个具有闭合输入、校验、消费、结果和副作用边界的实际 Version4 命令链。

本切片的价值不在于文件数量，而在于它能先建立一条完整 ECS 迁移样板：

```text
短生命周期 Component
  -> 唯一写者 System
  -> 纯 Query
  -> 两个外部调用 Adapter
  -> 保留旧 facade
  -> focused/differential verifier
```

这条样板完成并通过删除门禁后，再以同样的证据标准选择下一个低耦合 System。

## 14. System/Command 是否适合像 Component 一样全量迁移

### 14.1 直接回答

**不建议一次性迁移全部 System，也不建议一次性迁移全部 Command。**

本项目应采用下面的两层策略：

```text
按类型全量盘点、分类、建立迁移账本
  -> 按领域、生命周期、读写集和副作用分组
  -> 选择一个闭合的垂直行为切片
  -> 一起迁移该切片需要的 Component + Query + System + Adapter/Projection
  -> 运行 focused/differential verifier
  -> 再进入下一批
```

因此，“全做 System 还是全做 Command”不是正确的代码迁移单位。当前建议是：

> **方向选 System，批次不选“全部 System”；第一批仍选 `FixExploitManEaters` 的保护索引切片。Command 只做全量盘点，暂不作为整体迁移批次。**

### 14.2 为什么 Component 可以较大批量，而 System/Command 不可以

Component 的主要职责是承载稳定状态和能力标识。只要类型身份、字段语义、注册键、生命周期和序列化边界已经确认，组件的批量迁移通常可以围绕数据契约进行验证。SS14 官方 ECS 文档也明确把 Component 描述为数据，把实体行为放在 EntitySystem 中；这支持“组件偏数据、系统偏行为”的分工，但不意味着 Version4 可以直接照搬 SS14 的运行时实现。

System 不是单纯的文件搬运对象，它至少携带以下运行时契约：

- 读取哪些组件或外部状态；
- 写入哪些组件、索引或全局状态；
- 在哪个 Tick、阶段、事件或屏障执行；
- 是否允许和其他 System 并行；
- 是否依赖上游资格判断，或向下游提供同步可见的结果；
- 是否包含随机、时间、网络、存档、日志、渲染或其他副作用。

这也是本项目 `FixExploitManEaters` 不能只把 `FixExploitManEaters.cs` 移到 `Systems/` 的原因：清空发生在 `Main.DoUpdateInWorld`，写入发生在 `NPC.UpdateNPC`，查询发生在 `WorldGen.KillTile`。三个位置的相对顺序和保护条件本身就是行为的一部分。

Command 的边界更外层。一个 Command 往往同时涉及输入解析、权限/上下文校验、业务调用、结构变更或外部副作用、结果反馈和错误处理。即使多个类都叫 `*Command`，也不代表它们具有相同的生命周期、线程模型或回滚方式。当前 Version4 的 Chat Command 和 Debug Command 候选还存在空处理、反射、文件、聊天反馈和网络广播等混合边界，不能按名称批量迁移后再补语义。

### 14.3 一手资料对迁移单位的约束

以下资料是网络检索后直接读取的官方/项目一手资料；它们用于确认 ECS 的通用调度和边界事实，不用于替代 Version4 私有行为证据。

| 来源 | 直接确认的事实 | 对 Version4 的约束 |
| --- | --- | --- |
| [Bevy 官方 ECS 入门](https://bevy.org/learn/quick-start/getting-started/ecs/) | System 处理匹配的组件集合；System 被加入 Schedule；示例明确提示同一阶段的 System 在可能时默认并行，输出顺序不能靠偶然顺序假设 | System 迁移必须登记读写集、阶段和顺序；不能把文件/注册顺序当作行为保证 |
| [Bevy `Commands` API](https://docs.rs/bevy/latest/bevy/ecs/system/struct.Commands.html) | Command 用于对 World 做结构性变更；命令进入队列，在 `ApplyDeferred` 时按序应用；延迟执行期间实体可能已经被删除 | Command 迁移必须验证排队、提交点、执行顺序、实体存活和错误结果，不能只验证调用方法能编译 |
| [Bevy `ApplyDeferred` API](https://docs.rs/bevy/latest/bevy/ecs/schedule/struct.ApplyDeferred.html) | 延迟参数的变更在调度器的 `ApplyDeferred` 或依赖关系满足时才对后续 System 可见；调整 Schedule 可能改变缓冲区应用顺序 | 任何结构性变更都必须显式记录可见性和屏障，不得把 Command 当成即时函数调用 |
| [Flecs 官方 Systems 文档](https://www.flecs.dev/flecs/Systems.html) | System 是 Query 加执行函数，可纳入 Pipeline；阶段用于排序；只读阶段中的结构性操作会入队，要求即时可见时需要单独的 immediate 语义 | System 的读写冲突、阶段和即时/延迟语义必须成为迁移账本字段 |
| [SS14 官方 ECS 文档](https://docs.spacestation14.com/en/robust-toolbox/ecs.html) | EntitySystem 负责组件行为、事件订阅和依赖；EntitySystem 有独立生命周期和关闭清理要求；组件应保持为数据 | 组件和 System 不能混迁为同一个无边界类；System 的启动、关闭、依赖和外部效果要单独验证 |

网络检索记录：Brave Search 技能脚本在本环境没有 `BRAVE_API_KEY`，因此未将其失败响应当作搜索结果；随后通过官方站点和官方源码/API 页面直接获取并核对上述原文。Google 搜索页触发了访问验证，未采纳其结果；Bing 结果质量不足，也未作为事实来源。报告引用的是可直接访问的官方页面，而不是搜索摘要。

### 14.4 迁移矩阵

| 类型 | 是否适合全量盘点 | 是否适合一次性迁移 | 推荐迁移单位 | 本项目当前动作 |
| --- | --- | --- | --- | --- |
| Component | 是 | 条件适合；仅限稳定数据契约且边界已闭合 | 一个能力/领域的状态契约 | 继续按组件命名、注册、生命周期和序列化边界推进 |
| Query | 是 | 小批量适合；必须保持纯读和结果语义 | 一个资格判断或派生规则集合 | 与所属 System 一起迁移，避免重复查询实现 |
| System | 是 | **通常不适合**；阶段、读写集和副作用不同 | 一个闭合的领域行为切片 | **选择 System 方向；先迁移 Man Eater protection slice** |
| Command | 是 | **通常不适合**；只有同一命令族的输入-校验-消费-结果闭环才适合 | 一个命令族的完整边界流 | 先盘点和分组；暂不全量迁移 |
| Adapter/Projection | 是 | 不适合与无关行为混批 | 一个明确的外部边界或兼容入口 | 与目标切片同时迁移，保留旧 facade 和回滚路径 |

### 14.5 什么时候可以把多个 System 放进同一批

不是绝对禁止多个 System 同批，而是必须先证明它们属于同一个闭合迁移单元。至少需要满足：

1. 属于同一领域或同一生命周期阶段，而不是仅仅共享 `System` 后缀。
2. 读集、写集和唯一所有者已列出，没有隐含的第二写者。
3. 调度顺序、并行限制、屏障和跨阶段可见性已明确。
4. 外部副作用可以被同一组 Adapter/Port 隔离并验证。
5. 可以用一个 focused verifier 或集成测试证明整个闭环，而不是只证明若干类型可以实例化。
6. 失败时可以整体回滚这一行为切片，不会迫使其他领域同时回滚。

按这个标准，`FixExploitManEaters.Update`、`ProtectSpot`、`SpotProtected` 以及两个兼容入口可以放在同一批；`AmbientWindSystem`、`TeleportPylonsSystem`、`BannerSystem` 和 Chat/Debug Command 则不能仅因为同属一个后缀批次就并入。

### 14.6 对本项目的落地决策

本轮不采用以下方案：

```text
方案 A：把 Version4 所有 *System.cs 一次性搬入 ECS
方案 B：把 Version4 所有 *Command.cs 一次性搬入 ECS
方案 C：只按类名/文件名筛选，迁移后再寻找调用链
```

采用：

```text
System 方向
  -> ManEaterProtectionIndexComponent
  -> ManEaterProtectionSystem
  -> 只读保护 Query
  -> NPC 兼容 Adapter
  -> TileBreak 兼容 Adapter
  -> focused/differential verifier
  -> 通过删除门禁后再选择下一切片
```

这里的“选择 System”表示选择行为迁移方向，不表示把所有 System 当成一个事务一次性改完；“Command 暂缓”表示当前没有足够闭合、低副作用、可独立验证的 Command 族，不表示以后不能迁移 Command。

## 15. 网络资料和本地事实的边界

网络资料只能给出 ECS 的通用机制和参考组织模式，不能替 Version4 代码证明以下事实：

- `FixExploitManEaters.Update()` 在 `Main.DoUpdateInWorld` 的具体位置；
- NPC 只在 Tile active 等上游条件满足后调用 `ProtectSpot`；
- `WorldGen.KillTile` 只在 `!effectOnly && !stopDrops && !noItem` 的掉落段查询保护索引；
- 该保护索引当前只有一个清理点、一个写入点和一个读取点。

以上结论仍以 Version4 源码和本报告第 4 节的调用闭包为准。网络资料的作用是解释为什么迁移账本必须包含调度、延迟可见性、生命周期和副作用字段，而不是把参考 ECS 框架的名称直接套进 Terraria 行为。

## 16. 修订后的最终结论

回答本次决策问题：

> **不要像组件一样一步全量迁移所有 System 或所有 Command。可以一次性完成两类的全量盘点和迁移分类，但代码迁移必须按“领域 + 生命周期 + 读写集 + 副作用”形成闭合切片。当前选择 System 方向，第一批只做 `FixExploitManEaters` 保护索引切片；Command 暂缓全量迁移。**

这样既保留了 ECS 的目标架构，也避免把系统调度契约和命令边界副作用隐藏在一次大搬迁中。第一批切片通过 focused/differential verifier 后，再决定下一批 System 或一个真正闭合的 Command 族。

## 17. 四个维度的详细判定方法

“领域 + 生命周期 + 读写集 + 副作用”不是描述性口号，而是判断一个迁移批次是否闭合的四张清单。四张清单中任何一张存在关键 `unknown`，都不能把该切片标记为可生产迁移；最多只能标记为盘点完成或原型完成。

### 17.1 领域：迁移的是能力闭环，不是文件夹或后缀

#### 定义

领域是一个具有稳定业务语义、状态不变量和触发边界的能力单元。它回答的是：

```text
谁在什么条件下发起行为？
这个行为保护/改变什么权威状态？
状态必须满足什么不变量？
谁消费结果？
这个行为在哪里结束？
```

领域边界不等于以下任何一种东西：

- C# 命名空间，例如整个 `Terraria.GameContent`；
- 文件后缀，例如全部 `*System.cs` 或全部 `*Command.cs`；
- 单个类的长度；
- 一个技术设施，例如“所有网络代码”或“所有静态类”。

#### 领域判定清单

| 判定项 | 要查的证据 | 通过标准 |
| --- | --- | --- |
| 能力名称 | 类型、调用点、注释、事件/命令名称 | 能用一个具体动词和对象描述，不依赖“Manager/Shared/System”泛名 |
| 触发入口 | 主循环、事件、命令入口、网络消息 | 能列出入口及其前置条件，不存在未搜索的第二入口 |
| 权威状态 | 字段、组件、集合、世界/实体数据 | 明确谁是唯一事实来源，区分缓存、快照和表现数据 |
| 不变量 | 条件分支、重复写入、清理、错误路径 | 能写成可验证的规则，例如“同坐标重复登记只有一个逻辑成员” |
| 消费者 | 读者、下游 System、返回值、早退分支 | 能列出所有直接和间接消费者，并说明结果如何影响下游 |
| 结束点 | 一次调用、Tick、实体、世界、会话、进程 | 能明确行为何时失效和何时释放 |

#### 对 Version4 的应用

`FixExploitManEaters` 的合理领域名称不是“NPC System”，而是：

> **Man Eater 锚点导致的临时 Tile 保护索引。**

这个领域的闭环是：NPC 经过自身 AI 的坐标和 Tile 有效性检查后登记锚点，Tile 破坏路径查询同 Tick 索引，命中后阻止该条件下的后续掉落/破坏流程。它跨越 `NPC` 和 `WorldGen` 两个旧模块，但仍然只有一个明确不变量：当前 Tick 中被登记的坐标受保护。

反例是把以下内容合并成一个领域批次：

```text
全部 NPC AI
  + 全部 Tile 破坏
  + Banner 计数/存档/网络
  + Ambient Wind
  + Chat Command
```

它们共享“游戏逻辑”这个宽泛主题，却没有共享同一个状态不变量、触发入口、结束点或回滚边界。这样的批次即使文件移动成功，也没有形成可验证的领域闭环。

#### Command 的领域边界

Command 需要再区分两层：

1. **协议入口领域**：文本/网络/调试输入的解析、身份、权限、参数和错误反馈。
2. **业务能力领域**：例如修改玩家状态、查询世界、生成实体或执行调试动作。

Command 类本身通常属于入口 Adapter；它调用的业务能力才属于具体领域。因而不能因为 16 个文件都以 `Command.cs` 结尾，就把它们当作同一 ECS 领域。`/roll`、`/emote`、`/kill` 和调试命令的输入协议可能相似，但随机性、权限、写集、反馈和失败语义完全不同。

### 17.2 生命周期：状态在什么范围内出生、有效、重置和死亡

#### 生命周期分层

迁移前要给状态和行为标出生命周期层级：

| 层级 | 典型范围 | 必须回答的问题 |
| --- | --- | --- |
| 调用级 | 一次函数调用 | 是否纯计算？异常后是否有外部残留？ |
| 事件级 | 一次事件/命令 | 事件是否可重放、重复提交或取消？ |
| Tick 级 | 一帧/一次世界更新 | 在 Tick 的哪个阶段清空、写入和消费？ |
| 实体级 | 实体生成到销毁 | 组件何时附着、移除、迁移或复制？ |
| 世界/会话级 | 世界加载到卸载、客户端连接到断开 | 是否允许多个 World？卸载和异常路径如何清理？ |
| 持久化/网络级 | 存档、快照、同步 | 哪些字段保存/复制？版本和失败如何处理？ |
| 进程级 | 程序启动到退出 | 静态状态是否跨 World 泄漏？如何重置？ |

#### 生命周期闭合的五个时点

每个切片至少要有以下时间线：

```text
创建/注册 -> 激活 -> 正常更新/事件处理 -> reset/clear -> unload/dispose
```

如果存在存档或网络，还要加入：

```text
serialize -> transmit/load -> validate -> restore
```

缺少 `reset/clear` 或 `unload/dispose` 证据时，不能把一个静态集合直接改成 ECS Component 后声称生命周期已正确。Component 的存储范围必须和权威状态范围一致：进程级状态不能无理由变成实体级状态，世界级状态也不能无理由变成全局单例。

#### `FixExploitManEaters` 的生命周期证据

当前 Version4 的状态是：

| 时点 | 旧行为 | 迁移含义 | 状态 |
| --- | --- | --- | --- |
| 初始化 | `IndexesProtected` 是静态 `List<int>`，随类型/进程存在 | 需要决定新索引绑定 World、Session 还是单一宿主 | `confirmed` 旧实现；目标归属 `unknown` |
| Tick 清理 | `Main.DoUpdateInWorld` 在玩家更新、NPC 更新之前调用 `Update()` 清空 | `BeginWorldTick` 必须位于同一相对阶段 | `confirmed` |
| NPC 更新 | 通过 `NPC.UpdateNPC` 的 `aiStyle == 13` 分支登记 | 保护 System 不应提前登记或重复实现 NPC 前置资格 | `confirmed` |
| Tile 消费 | `WorldGen.KillTile` 在满足掉落判断的分支中查询 | Query 必须只对同 Tick 状态可见 | `confirmed` |
| 世界卸载 | 当前已检查闭环没有对应的显式清理调用 | 需要补充单/多世界和异常退出验证 | `unknown` |
| 网络/存档 | 当前切片没有登记为持久化或网络字段 | 不创建 Snapshot/Projection；确认注册表没有误收录 | `partial` |

这就是为什么该切片“适合先做”但还不是“立即删除旧实现”：它的 Tick 生命周期闭合度高，World/Session 生命周期还需要在接入前补证据。

#### System 和 Command 的生命周期差异

System 常见生命周期是“初始化/注册 -> 每 Tick 或事件执行 -> shutdown”；Command 常见生命周期是“接收 -> 解析 -> 校验 -> 执行/入队 -> 反馈 -> 审计/失败处理”。

Command 若涉及结构变更，还要额外记录：

- 命令何时入队；
- 何时提交/回放；
- 结构变更何时对后续 Query 可见；
- 目标实体在提交前消失时返回什么；
- 重复命令是否幂等；
- 失败后队列中其他命令是否继续执行。

因此 Command 不能只按“处理函数有无返回值”判断迁移完成。

### 17.3 读写集：谁能读，谁能写，什么时候读到什么

#### 读写集的四层口径

一个迁移账本里的“读写集”不能只列方法参数，还要分四层：

| 层次 | 内容 | 例子 |
| --- | --- | --- |
| 直接读取 | 方法体直接访问的字段、组件、参数和常量 | `IndexesProtected.Contains(item)`、`ai[0]`、`Main.tile[i,j]` |
| 间接读取 | 调用的 helper、属性、服务、全局状态 | `CheckTileBreakability`、玩家数组、语言/权限服务 |
| 直接写入 | 对权威字段、集合、组件和结构的修改 | `Clear`、`Add`、设置 `active=false` |
| 可观察输出 | 返回值、异常、事件、网络包、日志、文件、UI | `return`、`Reply`、广播、`CallTracker` |

读集和写集还必须标注：

- **权威/派生**：读取的是事实，还是可以重算的缓存；
- **所有者**：谁是唯一允许写者；
- **阶段**：写入在查询之前还是之后；
- **可见性**：写入立即可见还是要等屏障/提交；
- **并行性**：能否和另一个 System 同时读写；
- **粒度**：实体、世界、全局或外部资源。

#### `FixExploitManEaters` 的读写账本

| 操作 | 读集 | 写集 | 唯一所有者 | 阶段/可见性 | 迁移规则 |
| --- | --- | --- | --- | --- | --- |
| `BeginWorldTick` | `ProtectionIndex` | 清空 `ProtectionIndex` | `ManEaterProtectionSystem` | 玩家阶段后、NPC 阶段前；立即影响后续写入 | 不能提前到未证明的阶段 |
| `ProtectSpot(x,y)` | 坐标参数；编码规则 | 向索引幂等添加 | `ManEaterProtectionSystem` | NPC 更新期间立即可供后续 Tile 查询 | Adapter 只接收已通过 NPC 上游检查的坐标 |
| `IsProtected(x,y)` | 索引和坐标编码 | 无 | Query/System 只读接口 | Tile 破坏掉落分支内读取 | 查询不得清理、添加或记录业务状态 |
| NPC 上游资格 | `ai[0]`、`ai[1]`、`Main.tile`、Tile active | 可能设置 `life`、`active`、触发 `HitEffect` | `NPC.UpdateNPC` | 早于 `ProtectSpot` | 不复制到保护 System |
| Tile 下游行为 | `KillTile` 参数、Tile、保护结果 | Tile、掉落、网络/表现等后续状态 | `WorldGen.KillTile` | 保护命中时旧逻辑直接早退 | Adapter 只映射 `Blocked/Continue` |

由此可以得到一个重要边界：保护索引 System 的读写集很小，但它的**时间依赖**很强。不能因为 `ProtectSpot` 和 `SpotProtected` 都只接收两个整数，就把清空、登记、查询重排成任意顺序。

#### 读写闭合的通过标准

一个切片满足读写闭合，至少要证明：

1. 权威状态的所有写者已全树搜索并列出。
2. 每个写者都有一个明确 owner；没有“System A 和旧静态 facade 都能写”的双写期，或双写期有明确过渡策略。
3. Query 的读集不隐藏写入；日志/计数等观测行为要单独标注。
4. 所有跨阶段读写都有显式顺序或屏障，而不是依赖文件顺序。
5. 旧 API 保留期间，旧 API 是 facade/adapter 还是第二个权威实现必须明确。

### 17.4 副作用：除了返回结果，哪些东西被改变或被观察

#### 副作用分类

| 类别 | 例子 | 风险 |
| --- | --- | --- |
| 内存状态 | 修改 Component、集合、缓存 | 所有权、重复调用、并发、清理 |
| 结构变更 | 创建/销毁实体、添加/移除组件 | 延迟可见性、迭代器失效、提交顺序 |
| 世界状态 | Tile、NPC、玩家、掉落、世界注册表 | 行为范围大、回滚困难 |
| 持久化 | 文件读写、存档、memo | I/O、版本、部分失败、重试 |
| 网络 | 广播、客户端同步、协议包 | 顺序、重复、断线、兼容性 |
| 外部服务/UI | 聊天、声音、渲染、平台服务 | 线程亲和性、不可逆观察、测试困难 |
| 非确定性 | 随机、时间、线程调度 | 差分验证不稳定、重放困难 |
| 观测副作用 | 日志、指标、CallTracker、审计 | 不能把“逻辑结果纯”误写成“方法完全无副作用” |

#### 对 `FixExploitManEaters` 的精确判断

这个切片不是“零副作用”，而是“核心状态副作用小且可隔离”：

- `Clear` 和 `Add` 修改短生命周期索引，这是 System 的受控内存状态副作用；
- `SpotProtected` 的核心成员判断是纯计算，但旧方法包裹了 `CallTracker`，因此旧 API 层仍有观测副作用；
- `WorldGen.KillTile` 本身可能继续触发 Tile、掉落、网络和表现效果，保护查询只决定是否早退；
- 保护命中时的**控制流早退**是可观察行为，不能改写成“只把掉落数量设为零”；
- 当前闭环没有证据表明保护索引需要随机、时间、存档或网络复制，因此这些效果不应被新 Component 吸收。

推荐把副作用拆成三层：

```text
纯核心：Pack/Contains/保护结果
  -> 受控状态：Index.Clear/Index.Add
  -> 外部适配：Main/NPC/WorldGen facade、CallTracker、Tile 后续效果
```

这样可以对纯核心做确定性测试，对 System 做状态转换测试，对 Adapter 做顺序/差分测试；如果把三层混在一个“万能 System”中，测试只能依赖完整游戏循环，迁移风险会明显上升。

#### Command 的副作用账本

对 Command 还要增加以下字段：

| 字段 | 示例问题 |
| --- | --- |
| 身份/权限 | 谁能执行？本地玩家、服务器、管理员还是测试 harness？ |
| 解析 | 参数缺失、非法颜色、未知实体、别名如何处理？ |
| 业务写入 | 修改哪些组件/世界状态？是否需要结构变更？ |
| 反馈 | 成功/失败向谁返回？是否广播？返回是否可重试？ |
| 持久化/网络 | 是否写文件或发包？断线/失败如何处理？ |
| 幂等/重放 | 同一命令重复执行会不会重复扣除、生成或广播？ |
| 错误隔离 | 命令失败是否污染前面或后面的队列？ |

`DebugCommandProcessor` 当前同时引用反射、`Main.SavePath`、权限判断、聊天回复和网络广播；它更像协议入口 + 调度器 + 外部效果协调器，而不是一个可直接当作低风险 ECS System 的纯 Command。`ChatCommandProcessor` 的入站处理和出站消息仍有空实现，说明其闭环还不能作为迁移完成证据。

## 18. 四维模型落到第一批切片

### 18.1 第一批的四维卡片

| 维度 | `FixExploitManEaters` 当前结论 | 证据 | 接入前必须补的内容 |
| --- | --- | --- | --- |
| 领域 | 临时 Man Eater 锚点 Tile 保护；不扩大到 NPC/Tile 全域 | `FixExploitManEaters.cs`、NPC 调用点、`KillTile` 查询点 | 写入迁移账本的领域不变量和调用闭包 |
| 生命周期 | Tick 清理/登记/消费已闭合；世界卸载、多世界归属未闭合 | `Main.DoUpdateInWorld:11470`、NPC/WorldGen 顺序 | 明确 Index 是 World resource、Session 状态还是宿主单例；补 unload/异常测试 |
| 读写集 | Index 单写者候选；Query 只读；NPC/Tile 上游下游保留旧 owner | 全树调用只有一个 clear、一个 protect、一个 query | 接入期间禁止第二权威实现；记录 facade 与 System 的读写切换 |
| 副作用 | 核心是受控内存状态；旧入口有 CallTracker；Tile 早退影响后续效果 | `CallTracker` 包裹三个旧 API；`KillTile` 命中后 `return` | 保留观测兼容性；对 `effectOnly/stopDrops/noItem` 做差分验证 |

### 18.2 当前可迁移评分

为方便排批次，可以给四个维度各打 0-2 分：

- `2`：证据闭合，可独立验证；
- `1`：主要路径明确，但存在可控的补证据项；
- `0`：关键边界缺失或与其他领域纠缠。

推荐门槛：总分至少 `6/8`，且领域、读写集不能为 `0`；若生命周期或副作用为 `0`，禁止生产迁移。

| 候选 | 领域 | 生命周期 | 读写集 | 副作用 | 总分 | 当前动作 |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| Man Eater protection index | 2 | 1 | 2 | 2 | **7/8** | 首批；先关闭 World/Session 生命周期缺口 |
| Ambient wind | 1 | 1 | 1 | 1 | 4/8 | 暂缓；helper 和节拍/效果闭环不足 |
| Banner | 1 | 0 | 1 | 0 | 2/8 | 暂缓；存档、网络、计数、表现边界混合 |
| Chat command family | 1 | 0 | 0 | 0 | 1/8 | 暂缓；入站/出站处理不闭合 |
| Debug command processor | 1 | 1 | 1 | 0 | 3/8 | 暂缓；权限、反射、文件、聊天、网络副作用未隔离 |

该评分是批次选择工具，不是运行时性能指标，也不取代第 8 节的 50 分候选比较。两种评分口径不同：第 8 节用于比较候选，第 18 节用于检查四维闭合度。

### 18.3 四维迁移卡片模板

以后每个 System 或 Command 批次都应填写下面的卡片，不能只写“已拆分”：

```text
Slice ID:
Domain:
  capability / invariant / entry points / consumers / explicit non-goals
Lifecycle:
  create / register / active phase / reset / unload / persist / network / multi-world
Read set:
  direct / indirect / temporal visibility / external reads
Write set:
  authoritative owner / derived state / structural changes / compatibility writes
Side effects:
  memory / world / persistence / network / UI / logging / randomness / failure / retry
Ordering:
  must-before / must-after / parallel-safe / barrier or commit point
Adapters:
  legacy facade / external protocol / ports / projections
Verification:
  pure rules / state transitions / order / differential / integration smoke
Rollback:
  old owner / feature switch or facade / deletion gate
Status:
  confirmed / partial / proposed / unknown
```

### 18.4 是否允许多个 System 同批

只有在四维卡片可以合并而不引入新的隐含边界时才允许。具体是：

```text
同一领域不变量
  + 同一生命周期阶段或显式可排序阶段
  + 读写集无未解决冲突且 owner 唯一
  + 副作用可由同一组 Adapter/Port 隔离
  + 一个 verifier 能证明闭环
  + 可整体回滚
```

例如 `BeginWorldTick`、`ProtectSpot`、`IsProtected` 和两个兼容 Adapter 可以作为一个 Man Eater 保护批次；`BannerSystem` 不能因为也有 `Update/Clear/Save/Load` 方法就并入。后者的存档、网络、NPC 计数和玩家效果分别具有不同生命周期和副作用边界。

## 19. 网络搜索结果与本项目的相关性

### 19.1 相关性分级原则

网络资料对本项目的作用分三层：

- **高相关**：直接说明迁移必须记录的通用机制，例如 System 阶段、读写冲突、Command 延迟提交、System 生命周期；可作为设计约束的外部佐证。
- **中相关**：提供成熟 ECS 的组织方式或 C# 实践，但运行时和 Version4 不同；只能支持结构选择，不能证明 Terraria 行为。
- **低相关**：搜索摘要、博客、无法确认版本/实现的二手文章；不作为报告事实依据。

### 19.2 已核对的一手网络资料

| 来源 | 网络原文确认的事实 | 对四个维度的相关性 | 不能推出的结论 |
| --- | --- | --- | --- |
| [Bevy 官方 ECS 入门](https://bevy.org/learn/quick-start/getting-started/ecs/) | System 处理特定组件集合并加入 Schedule；官方示例提示同一阶段的 System 在可能时默认并行 | **高：领域、读写集、顺序** | 不能证明 Version4 的具体 Tick 顺序或可并行性 |
| [Bevy `Commands` API](https://docs.rs/bevy/latest/bevy/ecs/system/struct.Commands.html) | Command 用于结构性 World 变更；命令进入队列并在 `ApplyDeferred` 时按序应用；目标实体可能在执行前消失 | **高：生命周期、读写集、副作用** | Version4 没有因此自动获得 Bevy 的 CommandQueue 语义 |
| [Bevy `ApplyDeferred` API](https://docs.rs/bevy/latest/bevy/ecs/schedule/struct.ApplyDeferred.html) | 延迟缓冲在 `ApplyDeferred` 或依赖满足时对后续 System 可见；调整 Schedule 可能改变缓冲应用顺序 | **高：生命周期、读写集、顺序** | 不能替代 Version4 的实际提交点调查 |
| [Flecs 官方 Systems 文档](https://www.flecs.dev/flecs/Systems.html) | System 是 Query + 执行函数；Pipeline 按 phase 排序；只读阶段中的结构变更会排队，需要即时可见时使用 immediate 语义 | **高：领域、读写集、生命周期** | Flecs 的 pipeline/read-only 实现不等于 NLTX 的调度实现 |
| [Unity Entities System Groups](https://docs.unity.cn/Packages/com.unity.entities@1.3/manual/systems-update-order.html) | System Group 对子 System 排序；`UpdateBefore/After` 约束顺序；System 有创建/销毁顺序；支持多个 World | **高：生命周期、顺序、多世界** | 不能据此推断 Version4 已有多个 World 或相同属性机制 |
| [Unity Entity Command Buffer](https://docs.unity.cn/Packages/com.unity.entities@1.3/manual/systems-entity-command-buffers.html) | ECB 保存线程安全命令队列，记录结构变更并在之后 playback；临时实体要到回放时才完整存在 | **高：Command 生命周期、副作用、提交点** | 不能把 Terraria 的普通 Chat Command 直接等同为 ECB |
| [SS14 官方 ECS 文档](https://docs.spacestation14.com/en/robust-toolbox/ecs.html) | Component 应只保存数据；EntitySystem 承载行为、事件订阅和依赖；EntitySystem 有连接/断开等生命周期清理 | **中到高：领域、生命周期、职责边界** | 只能借组织模式，不能证明 Terraria 字段/顺序/网络语义 |
| [tModLoader API 文档](https://github.com/tModLoader/tModLoader/wiki) | 公开 API 的类型、成员和参数语义可作为 Terraria 公开边界参考 | **中：外部 API 适配** | 不能证明 Version4 私有 `FixExploitManEaters` 或调度顺序 |

网络资料最有价值的相关性，不是告诉我们“应该使用哪个 ECS 框架”，而是证明四个维度确实是成熟 ECS 的运行时问题：System 的执行顺序和访问冲突、Command 的延迟结构变更、System 的创建/销毁与多 World、组件和行为的职责分离。真正决定 Version4 行为的证据仍然是目标源码：`Main.cs`、`NPC.cs`、`WorldGen.cs` 和 `FixExploitManEaters.cs`。

### 19.3 检索方式和证据边界

本次网络检索采用以下顺序：

1. Brave Search 技能脚本：环境未配置 `BRAVE_API_KEY`，脚本返回 `fetch failed`，因此未引用其空结果。
2. 官方 URL 直接抓取：Bevy、docs.rs Bevy API、Flecs、Unity Docs、SS14 Docs 均返回 HTTP `200`，读取页面正文而非搜索摘要。
3. 官方源码补证：读取 Bevy 官方 GitHub `bevy_ecs` 的 `commands/mod.rs` 和 `schedule/mod.rs`，用于确认文档中的队列/延迟语义确实对应实现代码。
4. 搜索引擎回退：Google 触发访问验证，Bing 结果质量不足，未将两者摘要作为报告事实。

因此，网络来源的证据等级是“ECS 通用机制的 `confirmed`，Version4 私有行为的 `inferred` 约束”；不能把官方框架的 API 名称、默认并行策略或队列机制直接复制到 NLTX 生产代码。

## 20. 最终执行规则

### 20.1 先全量盘点，后分批迁移

```text
全量盘点 System/Command
  -> 为每个候选填写四维迁移卡片
  -> 标出 unknown 和跨领域边界
  -> 选择四维得分足够且可整体回滚的切片
  -> Component + Query + System + Adapter/Projection 一起闭合
  -> focused/differential/integration verification
  -> 更新账本和删除门禁
```

### 20.2 本项目当前动作

```text
System 方向
  -> 领域：Man Eater 临时 Tile 保护
  -> 生命周期：补齐 World/Session 归属、unload、异常清理
  -> 读写集：Index 单 owner；Query 只读；保留 NPC/Tile 上下游 owner
  -> 副作用：隔离 Index 状态、CallTracker、KillTile 早退和后续外部效果
  -> 验证：清理/登记/查询顺序 + 编码差分 + KillTile 条件差分
```

Command 方向当前只做全量盘点和四维卡片。只有当某个命令族的输入、权限、解析、业务写集、提交点、结果反馈、失败和重试语义都闭合时，才建立一个 Command 批次；否则不把 Command 类名当作迁移理由。

### 20.3 一句话决策

> **领域决定迁移边界，生命周期决定状态何时有效，读写集决定谁拥有和何时可见，副作用决定能否隔离和验证；四者共同决定批次，而不是 `System`/`Command` 文件后缀决定批次。当前继续选 System，但只迁移 Man Eater 保护索引这一闭合切片。**
