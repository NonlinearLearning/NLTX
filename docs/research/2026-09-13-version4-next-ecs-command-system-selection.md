# Version4 下一步 ECS Command/System 候选筛选调研

> 历史调研快照：本报告形成于 2026-09-13。P20 已于 2026-10-02 退出当前非权威分区范围，其组件设计/执行资料已移除；本报告涉及 P20 的判断只描述当时调研，不构成当前工作项。

日期：2026-09-13  
范围：只读代码、账本、既有报告和参考资料；本报告未实施迁移。  
证据标签：`confirmed` 表示单一源码或账本直接证明；`corroborated` 表示多个来源相互印证；`inferred` 表示基于证据的架构判断；`proposed` 表示尚未实施的设计；`unknown` 表示当前材料无法证明。

## 1. 执行摘要

### 唯一主推荐

**条件性主推荐：`AmbientWindSystem.Update` 的墓地资格、局部计数器和 30 tick 节拍子范围。**

- 推荐类型：`System`，客户端环境表现调度系统；不是 `Command`，也不是 `Command + System`。
- 推荐置信度：**中等偏低，条件性**。`Main` 的实例化和每 tick 调用、资格门、计数器和节拍直接由 Version4 源码证明；但是完整扫描和生成行为不能由当前源码证明。
- 立即开始的理由：当前真实调用图只有一个 `Main` 实例字段和一个每 tick 调用点；当前实际写集主要是 `_updatesCounter`，没有已确认的网络、存档、世界规则或实体权威写入；可先用影子系统验证“进入/离开墓地时计数和每 30 tick 触发”的旧行为。
- 主要阻塞条件：`GetTileWorkSpace`、`TrySpawningWind`、`SpawnAirborneWind` 在 Version4 均为空；reset、暂停、世界切换和表现端口失败语义未知；第一轮报告和第二轮非权威 P02 已经提出相同方向的 proposed 边界。因此不得把完整参考版本的 Tile 扫描、随机概率、Gore 或风效果直接当成 Version4 行为。

没有候选完全满足“低风险、无证据缺口、重复度低”的全部标准。主推荐是当前风险最低且边界最容易闭合的**条件性**选择，不是“已完成迁移”或“行为等价”声明。若上述阻塞条件不能先关闭，应暂缓该方向，继续保留旧实现。

## 2. 当前迁移基线

### 2.1 账本和进度

| 证据 | 直接事实 | 状态解释 |
| --- | --- | --- |
| `D:\TRbackup\NLTX\Context\progress.md:3-23` | 上下文已清理；没有 active migration plan/task；该文件明确声明不是迁移完成、行为等价或删除门禁结论 | `confirmed`；`docs/flowstate/README.md` 被引用但当前不存在，生命周期规则在本调研中为 `unknown` |
| `D:\TRbackup\NLTX\docs\migration\ledgers\Version4-member-migration-map.json` 的 `decisions` | 506 条决策；按 `disposition` 为 `deferred=425`、`move=68`、`split=3`、`merge=5`、`compatibility=5`；按 `verificationStatus` 全部为 `not-run`；`workItems` 共 64 条，当前记录为 `ready` | `confirmed`；结构处置不等于运行时迁移 |
| `D:\TRbackup\NLTX\docs\migration\ledgers\Version4-member-migration-quick-reference.json` 的 `completedSummary`、`recoveryHeader` | `migrated=76`、`verified=0`、`deferred=425`、`open=506`；`selectedWorkItemId=work-item-015`；`activeWorkItems` 的审计摘要为 0 | `confirmed`；“migrated”是账本视图，不等于当前 Version4 运行时已替换 |
| `D:\TRbackup\NLTX\docs\migration\ledgers\version4-member-migration-audit.json` 的 `ok`、`summary`、`findings` | `ok=true`、`errors=0`、`findings=[]`、`summary.activeWorkItems=0` | `confirmed`；审计通过只证明账本结构一致，不能证明行为、构建或测试 |
| `D:\TRbackup\NLTX\docs\migration\ledgers\Version4源码覆盖.tsv` | 967 个 Version4 源文件记录，首行为表头，文件记录数为 967 | `confirmed`；它是覆盖索引，不是调用图 |

### 2.2 与候选直接相关的已完成、进行中和历史状态

| 方向 | 当前状态 | 证据和边界 |
| --- | --- | --- |
| 世界环境/天气组件 | 部分组件已保存；System、Query、Command、Adapter、Projection、注册、网络、持久化和行为等价工作仍未完成 | 第二轮 P02 执行记录 `D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P02-world-environment-events-component-execution.md:10-14,56-72` 将状态写为 `proposed`、`src2-only Component implementation checkpoint`、`verificationStatus: partial`；这是历史文档记录，不是本次验证 |
| `AmbientWindStateComponent` | 已有 `src2` 组件文件，但它表达的是天气风速、目标速度和两个风计时器，不等同于 `AmbientWindSystem._updatesCounter` | `D:\TRbackup\NLTX\src2\WorldSession\Calendar\AmbientWindStateComponent.cs:5-41`；P02 对 `_updatesCounter` 另行提出 `AmbientWindWorkBuffer.UpdateCounter`，见 `D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P02-world-environment-events-component-design.md:1019-1034` |
| `AmbientWindSystem` 拆分 | 已被既有报告覆盖，尚无生产运行时迁移 | 第一轮 `D:\TRbackup\NLTX\docs\component-decomposition\baseline\Version4权威游戏模拟系统拆分设计报告.md:4927-4947` 已提出资格 Query、工作区 Query、生成 Query、节拍 System 和 Projection；第二轮 P02 execution `:1223-1257,2038-2069` 仍把这些非 Component 边界列为 proposed |
| Chat/Console/ChatCommand | 已审查并归入外部输入适配，不升格为独立一级 System | `D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-07-version4-second-round-review-merged.md:904-915`；当前状态是已有审查结论，不是 rejected 运行时实现 |
| Debug command protocol | 当时有 proposed 设计，非 Component 边界 deferred；历史 verifier 记录为失败 | P20 组件设计/执行资料已于 2026-10-02 从当前范围移除；此项仅作历史调研记录 |
| Smart interaction | 只有注册和候选接口的 partial 边界，尚未证明实际扫描/选择生命周期 | `D:\TRbackup\NLTX\docs\component-decomposition\baseline\Version4权威游戏模拟系统拆分设计报告.md:4073-4143`；P08 字段记录为 proposed Query/buffer，不能当作已实现迁移 |
| Teleportation、Town、PressurePlate、Currency | 已有第一轮或第二轮方向覆盖，分别存在网络/TileEntity、NPC/存档/锁、全局数组/Tile/存档或交易/容器依赖 | 代表性证据见本报告第 8 节；这些方向不能因类名短或包含 `System`/`Manager` 就重新推荐 |

本次没有把任何方向标为 `rejected`，因为现有材料通常记录为 `deferred`、`partial`、`proposed` 或“归入已有子系统”，而不是统一的 rejected 枚举。明确的实际结论是：**未完成的方向仍有 active work item 语义风险，但账本当前 `activeWorkItems=0`；既有报告中的 proposed/partial 不能写成已实现。**

状态归类：本报告当时记录的 P02/P20 组件切片不能扩展为已实现 System；P20 已于 2026-10-02 从当前范围和源码移除。P02 的非 Component 边界属于“仅有设计/执行计划”；Chat、SmartInteract 等属于“已审查但未实现或行为未闭合”；账本中的 `deferred` 仍是 deferred；没有找到正式标为 `rejected` 的本批候选；该次审计没有 active work item，64 个 work item 记录为 `ready`。这些分类只描述 2026-09-13 的资料状态，不描述当前运行时行为。

## 3. 候选清单和评分表

### 3.1 评分方法

每项 0 至 5 分，越高越适合立即开始。依次为：

`R` 责任范围小且内聚；`G` 调用图简单；`O` 状态所有权明确；`C` 读写集合可闭合；`E` 副作用可隔离；`T` 不依赖复杂 Tick 顺序；`X` 不涉及网络、存档或渲染主流程；`V` 验证路径清晰；`D` 与既有报告重复度低；`U` 能提供可复用边界。总分为 50 分。评分是本调研的 `inferred` 研究判断，不是运行时度量。

### 3.2 候选比较

| 候选名称 | 源文件和类型 | 领域 / 适合类型 | 规模 | 主要读者和写者 | 依赖与副作用 | 报告重叠 | R/G/O/C/E/T/X/V/D/U | 总分 | 结论 |
| --- | --- | --- | --- | --- | --- | --- | --- | ---: | --- |
| `AmbientWindSystem` 的资格/节拍子范围 | `D:\TRbackup\Version4\Terraria.GameContent\AmbientWindSystem.cs:7-45`，`Terraria.GameContent.AmbientWindSystem` | ClientPresentation/环境表现；`System` | 3 个字段、1 个公共 `Update`、3 个私有辅助方法；`Main` 1 个字段和 1 个调用点 | 读 `Main.LocalPlayer.ZoneGraveyard`；当前写 `_updatesCounter`；当前没有确认的有效候选点或效果写者 | 读全局 `Main` 和本地 Player；当前没有文件、网络、存档、线程或实际随机调用；有 `CallTracker` 观测边界 | 高：第一轮 80.2 和 P02 已覆盖 | `5/5/4/4/5/3/5/4/1/4` | **40** | 唯一主推荐，但必须限定为已确认的 cadence/eligibility slice |
| `BackgroundChangeFlashInfo.UpdateFlashValues` | `D:\TRbackup\Version4\Terraria.GameContent\BackgroundChangeFlashInfo.cs:5-40`，`BackgroundChangeFlashInfo` | ClientPresentation；`System` | 2 个数组、2 个公共方法、1 个空私有方法；`Main` 1 个调用点 | `Main` 调用衰减；`_flashPower` 由该方法写；未找到有效初始化写者或消费者 | 纯本地数组衰减，当前无网络/存档；但效果是否可见未知，数组默认值会使当前运行行为近似惰性 | 高：P02 `:899-945,1639` 已覆盖 | `5/5/4/2/5/4/5/2/1/2` | **35** | 低风险但价值和消费者未闭合，不能作为第一选择 |
| `EmojiCommand`/`ChatCommandProcessor` 注册链 | `D:\TRbackup\Version4\Terraria.Chat.Commands\EmojiCommand.cs:9-47`；`D:\TRbackup\Version4\Terraria.Chat\ChatCommandProcessor.cs:9-76` | IntentAndInteraction/ExternalBoundaries；表面像 `Command`，实际是注册适配 | 1 个字典加注册/别名方法；处理方法为空 | `ChatInitializer.Load` 注册；`EmojiCommand.Initialize` 写 `_byName`；未发现有效消费调用 | 本地化注册、别名委托；解析、入站消费、出站处理均为空；调用图不闭合 | 高：二轮审查明确归外部输入适配 | `3/2/2/2/4/5/5/2/2/2` | **29** | 不推荐；不能把类名 `Command` 当作 ECS Command 证据 |
| `SmartInteractSystem` | `D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions\SmartInteractSystem.cs:5-21` | PlayerInteraction；理论上 Query/System | 3 个列表和构造函数注册；无扫描入口 | 构造函数写 provider/block 列表；候选接口实现分散；实际系统调用未知，`Player.cs:15163` 仅为注释 | Tile/NPC/Projectile/Player 可能跨域；没有发现排序、选择、执行闭包 | 高：第一轮和 P08 已标 partial/proposed | `3/1/2/1/5/4/5/1/1/2` | **25** | 仅为注册声明，不是当前可迁移行为 |
| `PressurePlateHelper.Update` | `D:\TRbackup\Version4\Terraria.GameContent\PressurePlateHelper.cs:7-111` | WorldInteraction/Wiring；`System` + 事件提交 | 5 个静态状态/缓存、5 个公共方法、若干空效果方法 | `Player.UpdatePlayerPosition`、`WorldGen.DestroyPlate`、WorldFile 读写；`Update` 消费静态字典并清空 | 255 玩家位置数组、Tile、Wiring/Tile 结构、存档锁；`MoveInto/MoveAwayFrom/PokeLocation` 为空 | 高：WorldInteraction/Structures 和 P01 已覆盖 | `3/2/2/2/1/2/1/3/2/3` | **21** | 有真实入口但副作用和权威写者未闭合，暂缓 |
| `DebugCommandProcessor.Process` | `D:\TRbackup\Version4\Terraria.Testing.ChatCommands\DebugCommandProcessor.cs:15-98` | Diagnostics/ExternalBoundaries；`Command + Adapter` | 静态文件路径、注册表、反射、权限、回复、广播 | `Main`/`DebugOptions`/memo 路径/`IDebugCommand.Process`；回复和网络广播由 `ChatHelper` 执行 | 文件路径、反射、全局开关、`Main.myPlayer`、权限和网络/UI 广播；当前 `TryProcessMemo` 为空，实际文件读写为 `unknown` | 高：P20 C12/C13 已 proposed/deferred | `2/2/2/2/1/3/0/3/1/3` | **19** | 是真正的 Command 候选形状，但不是低风险起点 |
| `TownRoomManager` | `D:\TRbackup\Version4\Terraria.GameContent\TownRoomManager.cs:9-178` | NpcAndTown/WorldStorage；`System + persistence` | 2 个主要状态集合、锁、10 个公共方法 | `WorldGen.TownManager`、NPC 住房逻辑、WorldFile Save/Load/Clear | NPC 类型、全局 `WorldGen`、`EntityCreationLock`、存档格式、住房关系；多个跨域写者 | 高：NPC/Town 和 P13 已覆盖 | `2/2/4/3/1/1/0/3/1/3` | **20** | 权威关系清晰但迁移面大，暂缓 |
| `TeleportPylonsSystem` | `D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonsSystem.cs:13-161` | Teleportation/WorldInteraction；`System + network projection` | 5 个字段、刷新/查询/Reset/加入同步/视觉方法 | `Main.PylonSystem.Update`；读 `TileEntity.ByPosition` 和 `SceneMetrics`；写本地列表并广播 | TileEntity registry、列表差集、`NetManager`、加入同步、Dust；存在刷新顺序和唯一 registry owner 问题 | 高：P01 和第一轮 Teleportation 已覆盖 | `2/2/2/2/1/1/0/3/1/3` | **17** | 网络和 registry 双写风险过高，暂缓 |

当前没有通过证据确认的低风险 `Command`。`DebugCommandProcessor` 具备命令分发形状，但权限、memo 文件、网络/UI 结果都在同一调用链；普通 Chat command 的解析和处理在当前 Version4 代码中为空。因而主推荐选择 `System`，而不是为了满足数量人为推荐一个 Command。

## 4. 主推荐的代码证据

### 4.1 类型、字段和方法

`D:\TRbackup\Version4\Terraria.GameContent\AmbientWindSystem.cs` 直接证明：

- `AmbientWindSystem` 为公开类（第 7 行），命名空间为 `Terraria.GameContent`。
- `_random` 是 `UnifiedRandom` 字段（第 9 行）；`_spotsForAirboneWind` 是 `List<Point>`（第 11 行）；`_updatesCounter` 是 `int`（第 13 行）。当前 `Update` 代码没有实际读取 `_random`，也没有把候选点写入列表；这一点由源码引用关系 `rg` 结果和方法体共同 `corroborated`，不能用完整参考实现补齐。
- `Update()`（第 15-38 行）先建立 `CallTracker`（第 17 行），然后读取 `Main.LocalPlayer.ZoneGraveyard`（第 19 行）。不满足时立即返回（第 20-22 行）。
- 满足时递增 `_updatesCounter`（第 23 行），调用 `GetTileWorkSpace()`（第 24 行），按返回矩形循环（第 25-33 行），每个格子调用 `TrySpawningWind`（第 31 行），每 30 次调用 `SpawnAirborneWind`（第 34-37 行）。
- `SpawnAirborneWind()`（第 40 行）、`GetTileWorkSpace()`（第 41-43 行）和 `TrySpawningWind(int,int)`（第 44-45 行）当前为空；`GetTileWorkSpace` 返回默认 `Rectangle`，因此当前实际循环没有可确认的 Tile 扫描。

因此当前 Version4 的最小可观测行为是：墓地资格为假时不增加计数；墓地资格为真时每次 `Update` 增加一次 `_updatesCounter`，空矩形循环不产生动作，并在计数是 30 的倍数时调用空的 `SpawnAirborneWind`。这是 `confirmed` 的 stub 行为，不是完整风效果。

### 4.2 调用入口和调用链

```text
Main._ambientWindSys = new AmbientWindSystem()
  -> Main.DoUpdateInWorld 每 tick 调用 _ambientWindSys.Update()
  -> ZoneGraveyard gate
  -> _updatesCounter++
  -> default Rectangle scan (当前无实际迭代)
  -> every 30 updates: empty SpawnAirborneWind()
```

证据：

- `D:\TRbackup\Version4\Terraria\Main.cs:1276` 创建私有字段；`D:\TRbackup\Version4\Terraria\Main.cs:11636-11640` 显示调用顺序为 `Chest.UpdateChestFrames()`、`_ambientWindSys.Update()`、`UpdateCameraPan()`。
- 对 Version4 全树搜索只找到上述创建和调用，以及 `AmbientWindSystem` 自身成员声明；没有找到 `Reset`、`Dispose`、世界切换或其他调用者。**没有找到不等于绝对不存在，reset/卸载生命周期仍为 `unknown`。**
- `Main.LocalPlayer` 的属性定义在 `D:\TRbackup\Version4\Terraria\Main.cs:1458`，`Player.ZoneGraveyard` 的访问器在 `D:\TRbackup\Version4\Terraria\Player.cs:2907-2915`。tModLoader 文档也只确认公开成员形状：本地 `D:\TRbackup\tmodloader-api-docs-stable\class_player.html:5236`，类型/成员为 `Player.ZoneGraveyard`。

### 4.3 读写闭包和副作用

| 类别 | 当前源码可确认内容 | 证据状态 |
| --- | --- | --- |
| 读取 | `Main.LocalPlayer.ZoneGraveyard`；默认 `Rectangle` 的局部值；循环边界；计数器自身 | `confirmed`，`AmbientWindSystem.cs:19-35` |
| 写入 | `_updatesCounter`；`CallTracker` 的观测作用域；当前没有实际候选点、Tile、天气、Player、NPC、Projectile 或世界字段写入 | `confirmed`；`CallTracker` 的最终 sink 为 `unknown` |
| 随机 | `_random` 字段存在，但当前 Version4 方法体没有使用它 | `confirmed`；完整参考中的随机调用不能移植为事实 |
| 集合 | `_spotsForAirboneWind` 字段存在，但当前源码没有有效添加、消费或清理路径 | `confirmed` / 其预期生命周期 `unknown` |
| 网络和持久化 | 当前 `AmbientWindSystem` 方法体没有 `NetManager`、`NetMessage`、`BinaryReader`、`BinaryWriter` 或文件调用 | `confirmed`；其他天气/环境网络语义不能从此类推断 |
| 线程和异步 | 未发现线程、Task、Timer 或异步入口 | `confirmed`，但线程亲和性由 Main 主循环约束，额外框架语义为 `unknown` |
| 渲染/表现 | 类型语义和既有报告将其归为客户端环境表现调度；当前三个效果方法为空，没有确认实际 Dust/Gore/粒子消费者 | 归类为 `corroborated`；当前具体效果为 `unknown` |

`D:\TRbackup\无任何删减通过编译\Terraria.GameContent\AmbientWindSystem.cs:40-187` 是完整参考补充。它可帮助识别未来需要验证的 Tile、随机和 Gore 边界，但不是当前 Version4 运行时证据。第一轮报告也明确说工作区、候选生成和表现仍为空，见 `D:\TRbackup\NLTX\docs\component-decomposition\baseline\Version4权威游戏模拟系统拆分设计报告.md:4935-4947`。

### 4.4 生命周期和边界稳定性

边界足够稳定的部分是 `ZoneGraveyard -> counter -> 30 tick cadence`：有单一实例、单一主循环入口、单一当前写字段和明确的早退条件。它适合先做 shadow System。边界不稳定的部分是 Tile workspace、候选点、随机源、Gore/粒子资源、世界切换清理和失败重试，必须留在阻塞列表。

不应继续把已确认的 cadence 逻辑留在 `Main` 内的理由是 `Main` 同时承担大量世界、玩家、网络和表现阶段；把这段局部状态变成具有读写声明的环境 System 可以建立可测试的输入/输出边界。这个结论是 `inferred`，不表示当前应删除 `Main` facade，也不表示 `Main` 是唯一运行时状态所有者。

## 5. 建议的 ECS 边界

以下均为 `proposed`，本报告没有创建这些文件。

### 5.1 状态、组件和工作缓冲

1. **第一最小批次不新增持久世界组件。** `_updatesCounter` 和候选 Tile 点是客户端短生命周期工作状态，不应写入世界存档或网络快照。
2. 复用 P02 已提出的 `AmbientWindWorkBuffer` 概念，建议路径为 `src2/WorldSession/Environment/AmbientWind/AmbientWindWorkBuffer.cs`；它只保存 `UpdateCounter` 和瞬态 `CandidateSpots`，并由 `AmbientWindSystem` 独占清理/提交。该 proposed 映射见 `D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P02-world-environment-events-component-design.md:1019-1034`。
3. 如果目标 ECS runtime 强制要求所有跨 tick 状态附着实体，才另行评估 `AmbientWindCadenceStateComponent`；它不得与 `AmbientWindWorkBuffer.UpdateCounter` 并存为两个真相源。该名称和注册键目前均为 `proposed`，不能直接当作已定义组件。
4. 现有 `AmbientWindStateComponent`（`D:\TRbackup\NLTX\src2\WorldSession\Calendar\AmbientWindStateComponent.cs:5-41`）属于天气风速/目标/风计时器边界，不应拿来承载 `AmbientWindSystem._updatesCounter`，否则会产生第二个状态所有者。

### 5.2 Query、System、事件和端口

| 边界 | 建议职责 | 读集合 | 写集合/效果 |
| --- | --- | --- | --- |
| `AmbientEffectEligibilityQuery` | 根据显式的本地场景快照判断墓地环境效果是否允许 | `ZoneGraveyard` 快照 | 返回 `eligible`，不写状态 |
| `AmbientWindWorkspaceQuery` 或 P02 的 `AmbientWindScanQuery` | 仅在后续补齐 helper 行为后计算合法 Tile 工作区和候选点 | 玩家/相机位置、世界边界、Tile reader、明确的随机端口 | 返回不可变候选；不写 Tile、Main 或组件 |
| `AmbientWindSystem` | 消费资格和 Query 结果，维护 cadence/work buffer；严格复现当前早退和 30 tick 边界 | `AmbientEffectEligibilityQuery`、`AmbientWindWorkBuffer`、只读输入 | 唯一写 `UpdateCounter` 和 transient candidate buffer；输出显式 presentation intent |
| `AmbientWindPresentationPort` | 在未来证据闭合后执行 Dust/Gore/粒子/音频等本地效果 | 已提交 intent | 只写客户端表现对象；不写天气、Player、World、网络或存档 |
| `IAmbientWindTileReadPort` | 适配 Tile/WorldGen 公开读取 | Tile 坐标和边界 | 只读；tModLoader 的 `WorldGen.InWorld`、`WorldGen.SolidTile`、`Utils.ToTileCoordinates` 仅作为 API 语义参考，见 `class_world_gen.html:836,1484-1493`、`class_utils.html:669-678` |
| `IAmbientWindRandomPort` | 未来完整生成行为需要时提供显式随机流 | seed/stream 输入 | 返回随机结果；不在纯 Query 中隐藏全局随机源 |

本最小批次不增加持久 `AmbientWindEvent`、网络命令或存档字段，因为当前 Version4 没有可确认的事件输出。只有在 `SpawnAirborneWind` 等方法恢复到有实际行为并完成效果消费者调查后，才可以考虑把结果表示为一次性 `AmbientWindPresentationRequest`；它应是 transient effect/request，不是世界长期组件。

### 5.3 执行阶段、顺序和单一写者

建议阶段为：

```text
Main/场景输入快照
  -> AmbientEffectEligibilityQuery
  -> AmbientWindSystem cadence/work-buffer commit
  -> presentation intent buffer
  -> AmbientWindPresentationPort
```

当前 Version4 的兼容阶段应保留 `Main.cs:11636-11640` 的调用位置作为顺序锚点，至少在 shadow verifier 证明相同前不要把它移到任意“文件顺序”或未定义的系统顺序。不能依赖目录或项目文件顺序，符合 `D:\TRbackup\NLTX\Context/架构设计\ECS文件组织设计约束.md:148-151,183-192`。

单一写者约束：

- 只有 `AmbientWindSystem` 写 `UpdateCounter` 和工作缓冲；
- `AmbientEffectEligibilityQuery`、Tile Query 和天气查询只读；
- `AmbientWindStateComponent` 的天气风速仍由其既有 weather owner 处理，不能由该 System 写；
- Presentation adapter 不能反写天气、Player、Tile、网络或存档；
- 不能同时让 legacy `_updatesCounter` 和新 `AmbientWindWorkBuffer.UpdateCounter` 独立推进。兼容窗口只能有一个 writer，另一侧做只读 shadow。

### 5.4 与既有领域的交互和文件归属

建议把新边界放在 `src2/WorldSession/Environment/AmbientWind/`，因为它表达的是客户端环境表现能力，并且 P02 已提出同一领域路径。若实际文件数量仍少于形成稳定边界的规模，按仓库约束保持该小领域扁平，不预建 `Components/`、`Queries/`、`Adapters/` 空目录，参见 `D:\TRbackup\NLTX\Context/架构设计\ECS文件组织设计约束.md:17-57,106-149`。

建议保留原 `Terraria.GameContent.AmbientWindSystem` 命名空间和 `Update()` 公共 API 作为兼容 facade，除非后续迁移任务明确要求 API 变更。组件名若最终采用，须遵循 `D:\TRbackup\NLTX\Context/架构设计\组件命名设计约束.md:42-69,89-120`，由实际注册机制确认运行时键，不在报告中猜测注册键。

## 6. 迁移范围估算

| 项目 | 最小 cadence 批次估算 | 说明 |
| --- | --- | --- |
| 预计涉及的 Version4 源文件 | 2 个：`D:\TRbackup\Version4\Terraria.GameContent\AmbientWindSystem.cs`、`D:\TRbackup\Version4\Terraria\Main.cs` | `Main` 只涉及字段/调用兼容边界；Player/Tile/天气只作为只读适配输入，是否需要实际改动为 `unknown` |
| 预计新增文件 | 2-4 个：`AmbientWindWorkBuffer`、`AmbientEffectEligibilityQuery`、目标 `AmbientWindSystem`、可选的输入/Presentation port | `AmbientWindRandomPort`、Tile port 和 Presentation adapter 应在确认 helper 行为后再加入；均为 `proposed` |
| 预计修改调用点 | 1 个运行时调用点 `Main.cs:11637`；可能再有 1 个组合根注册点 | 当前没有其他调用者证据；组合根位置为 `unknown` |
| 公共 API 兼容性 | 目标是保留 `AmbientWindSystem.Update()`；`_random`、列表和三个私有 helper 不属于公共 API | `inferred`；完整 API 依赖实际 NLTX 目标项目，未验证 |
| 注册、序列化、网络、持久化 | cadence 批次不应需要；不新增注册键、网络包、存档字段或协议 | `proposed`，前提是只迁移当前已确认的 transient cadence |
| 最小批次 | 先冻结 stub 行为表；再做 read-only 输入 facade；然后 shadow cadence；差异为零后切换单一 writer；最后才评估完整扫描/表现 | 各阶段可独立回滚 |
| 回滚边界 | 关闭新 System/恢复 legacy `Main` 调用；只允许一个 counter writer；发现 helper 语义差异时回到 stub facade | 不删除旧类、不删除字段、不进行双写 |

不得把“预计涉及 2 个源文件”理解为已经修改。当前调研只写本报告，没有修改任何 `.cs`。

## 7. 风险、验证和验收

### 7.1 风险登记

| 风险 | 当前判断 | 验收前置条件 |
| --- | --- | --- |
| 行为一致性 | 当前 stub 的早退、计数和空矩形语义可精确复现；完整风行为未知 | 用 zone 序列和 tick trace 比较 legacy/shadow；30、60、离开墓地、重新进入墓地都要覆盖 |
| 时序 | `Main` 调用位于 `Chest.UpdateChestFrames` 后、`UpdateCameraPan` 前；未来表现结果可能依赖该位置 | 先保留调用位；禁止用目录顺序替代显式阶段；记录 camera/scene 输入快照边界 |
| reset/世界切换 | 未找到 `AmbientWindSystem.Reset` 或世界卸载调用 | 必须补足世界创建、清理、暂停、客户端切换和本地 Player 替换证据；在此之前不能宣称生命周期闭合 |
| 随机性 | `_random` 字段存在，但当前行为未调用；完整参考的随机流和 seed owner 不可确认 | 先把随机端口留在未启用边界；若恢复完整 helper，必须单独建立随机流和回放测试 |
| Tile/渲染副作用 | 当前 helper 为空；完整参考可能读 Tile 并创建 Gore | Tile 读取通过 port；Gore/Dust/音频通过 Presentation port；禁止 Query 直接写世界或表现资源 |
| 失败和重试 | 当前没有有效外部提交，因此没有可证明的 retry 语义 | 未来 presentation port 失败时，必须先决定丢弃、记录或重试；不得把未知结果默认当成功或安全重试 |
| 第二个写入者 | `AmbientWindStateComponent` 已存在，容易把天气风速和本地墓地风 cadence 混合 | 逐字段列出 writer；`AmbientWindSystem` 不得写 `WindSpeedCurrent/Target`，weather owner 不得写 cadence buffer |
| 报告重复 | 第一轮和 P02 已有相同设计边界 | 本任务只复用、审计和收缩既有边界，不重复创建第二个设计 owner；实现任务必须引用 P02 并更新状态，而不是另起同义组件 |

### 7.2 建议的测试和静态验收

建议在后续实现任务中新增或复用：

1. `AmbientWindEligibilityQuery` 纯测试：`ZoneGraveyard=false/true`、空输入、边界状态；只验证结果，不接触 `Main`。
2. cadence 状态测试：连续 29、30、60 次有效更新，非墓地 tick 不改变计数；离开再进入墓地时验证当前源码的“早退但不重置”语义，除非有新证据允许改变。
3. shadow trace：记录每 tick 的资格、counter、workspace、candidate 数和 presentation intent。当前 Version4 预期 workspace/candidate/intent 均为空或零；这是基于当前 stub 的 parity fixture。
4. lifecycle verifier：世界初始化、世界清理、客户端切换、本地 Player 不存在或无效时的行为必须先获得源码证据；未知时只能报告拒绝/不执行，不能猜默认行为。
5. 静态检查：新 System 的读写集合、所有 `Main`/静态随机/Tile/网络/存档访问；确认没有第二个 counter writer、没有组件双写、没有把 Tile 坐标放进持久快照。
6. 若参考完整风行为，使用 tModLoader 文档交叉检查 API 语义：`WorldGen.InWorld`、`WorldGen.SolidTile`、`Utils.ToTileCoordinates` 和 `Gore.NewGorePerfect`；文档只能确认 API 语义，不能补 Version4 私有调用顺序。

后续项目确定后，编译/测试命令应遵守仓库根目录的串行包装器；本报告阶段**不得执行**。建议命令模板为：

```powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  build .\path\AffectedProject.csproj `
  -m:1 -nr:false `
  -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false `
  -p:BuildInParallel=false

pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  test .\path\AffectedProject.csproj --no-build --no-restore `
  -m:1 -nr:false `
  -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false `
  -p:BuildInParallel=false
```

`AffectedProject.csproj` 当前尚未因本调研确定，命令只是后续建议。未来验收必须记录实际命令、项目、退出码、warning/error 数和 `Build/bin/` artifact；本报告没有 build/test/restore/run 结果。

### 7.3 Space Station 14 参考的限定用途

Space Station 14 只用于确认成熟 ECS 的组织和测试边界，不作为 Terraria 事实：

- `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Client\Commands\ZoomCommand.cs:12-76` 展示了 Command 读取显式依赖，再调用 `ContentEyeSystem.RequestZoom` 或写客户端 eye 状态；它不能证明 Terraria Chat command 的语义。
- `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Server\Wires\WiresComponent.cs:6-70` 与 `WiresSystem.cs:25-58,61-226` 展示组件保存实体状态、System 订阅事件并集中随机/交互副作用；它不能替代 Terraria Tile/Wiring 证据。
- `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.IntegrationTests\Tests\Chat\ChatHighlightTest.cs:18-94` 展示集成测试对输入、状态保留和结果进行断言；它只提供测试组织参考。

这些参考支持“输入/状态/效果边界显式化”的 `inferred` 原则，不支持把 SS14 的框架 API、组件注册或运行时调度直接复制到 Version4。

## 8. 备选和排除项

### 8.1 `BackgroundChangeFlashInfo.UpdateFlashValues`：低风险但不选

`D:\TRbackup\Version4\Terraria.GameContent\BackgroundChangeFlashInfo.cs:31-39` 每次把 `_flashPower` 按 `Clamp(value - 0.05f, 0f, 1f)` 衰减，`Main.cs:11333-11337` 每 tick 调用它。它比 AmbientWind 更确定性，但没有找到 `_flashPower` 的有效触发写者和消费者；`UpdateVariation` 仍为空。P02 已将其列为表现缓存，见 `D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P02-world-environment-events-component-design.md:916-919`。因此它适合未来做小型 presentation System 骨架，不适合当前作为有价值的迁移起点。

### 8.2 `EmojiCommand`/普通 Chat command：排除为 Command 起点

`ChatInitializer.Load` 只负责注册（`D:\TRbackup\Version4\Terraria.Initializers\ChatInitializer.cs:9-32`）；`ChatCommandProcessor` 的注册、别名和默认命令管理在 `:19-71`，`CreateOutgoingMessage` 和 `ProcessIncomingMessage` 在 `:73-76` 为空；`EmojiCommand` 的两个处理方法在 `D:\TRbackup\Version4\Terraria.Chat.Commands\EmojiCommand.cs:46-47` 为空。`ChatMessage` 虽然有 `CommandId`、`Text` 和 `IsConsumed`（`D:\TRbackup\Version4\Terraria.Chat\ChatMessage.cs:8-24`），但没有证据证明完整解析、授权、消费或效果提交链。二轮审查已将 Chat/Console/ChatCommand 归入外部输入适配。因此不能因接口叫 `IChatCommand` 就新增 ECS Command。

### 8.3 `DebugCommandProcessor`：排除为当前 Command 起点

`D:\TRbackup\Version4\Terraria.Testing.ChatCommands\DebugCommandProcessor.cs:15-44` 使用静态 `Main.SavePath` 派生 memo 文件路径并通过反射注册命令；`:63-90` 读取 `DebugOptions`、`Main.myPlayer`，调用权限检查、命令执行、回复和 `ChatHelper.BroadcastChatMessage`。当前 `TryProcessMemo` 为空，所以 memo 的实际文件读写是 `unknown`，但文件路径已经是显式外部边界。`ChatHelper` 在 `D:\TRbackup\Version4\Terraria.Chat\ChatHelper.cs:21-91` 处理 `NetManager` 广播、客户端显示和缓存。P20 已把它拆成 catalog/dispatcher/request/file/network boundary 的 proposed 组合，非 Component 工作仍 deferred，且历史 execution 记录 verifier failed。它是未来可做的 Command + Adapter 方向，不是低风险起点。

### 8.4 `SmartInteractSystem`：排除为当前 System

`D:\TRbackup\Version4\Terraria.GameContent.ObjectInteractions\SmartInteractSystem.cs:5-21` 只有三个列表和构造函数注册；`D:\TRbackup\Version4\Terraria\Player.cs:15163` 的 SmartInteract 调用仍是注释。候选 provider 虽然有接口和各自类型，但没有闭合扫描、排序、选择和执行入口。它是 partial 查询边界，不是可验证的运行时 System。

### 8.5 `TeleportPylonsSystem` 和 `TownRoomManager`：排除为复杂跨域方向

- `TeleportPylonsSystem` 在 `D:\TRbackup\Version4\Terraria.GameContent\TeleportPylonsSystem.cs:25-90` 扫描 `TileEntity.ByPosition`、交换当前/旧列表、计算差集并广播网络包，还在 `:82-90` 处理玩家加入；P01 已明确要求唯一 registry owner 和兼容 shadow 窗口，见 `D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\2026-09-11-version4-P01-liquid-wiring-spatial-death-teleport-component-execution.md:850-873`。
- `TownRoomManager` 在 `D:\TRbackup\Version4\Terraria.GameContent\TownRoomManager.cs:65-142` 写住房关系、使用 `EntityCreationLock`，并 Save/Load；`WorldGen.cs:4562-4576,4582-4606` 将它接入 NPC 房屋生成和驱逐。第一轮 NPC/Town 报告已把它作为世界级住房 registry，不能再以单个小 Manager 处理。

### 8.6 `PressurePlateHelper` 和 `ScreenObstruction`：排除为尚未闭合的表现/结构边界

- `PressurePlateHelper` 的静态 `PressurePlatesPressed`、255 项 `PlayerLastPosition`、Tile 检查和存档边界见 `D:\TRbackup\Version4\Terraria.GameContent\PressurePlateHelper.cs:9-17,21-111`、`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:3450-3472`；其实际 `MoveInto/MoveAwayFrom/PokeLocation` 为空，无法证明提交行为。
- `ScreenObstruction.Update` 虽然在 `D:\TRbackup\Version4\Terraria.GameContent.Events\ScreenObstruction.cs:6-43` 有确定性平滑写入，但依赖 `SceneMetrics.PerspectivePlayer` 和 `DangerousDungeonCurse`，且本次搜索没有找到直接调用者、reset 或 render consumer。P02 已将 caller/reset/render 标为 evidence gap，见 `D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P02-world-environment-events-component-design.md:528-555`。

## 9. 未决问题和下一步

以下问题均来自当前证据缺口，不需要通过询问用户解决：

1. **`AmbientWindSystem` 的完整 helper 语义未知。** 需要确认 Version4 当前构建来源是否有与 `D:\TRbackup\Version4\Terraria.GameContent\AmbientWindSystem.cs:40-45` 不同的生成或替换步骤；在没有证据前，不得采用 `无任何删减通过编译` 的实现。
2. **reset/世界切换语义未知。** 尚未找到 `AmbientWindSystem` 的 reset、世界卸载和本地 Player 生命周期入口；下一任务必须先建立调用者和生命周期清单。
3. **`CallTracker` 的观测 sink 未闭合。** 需要确认迁移后是否通过现有诊断适配器保留方法级观测，以及它是否影响性能或测试输出。
4. **Tile workspace、随机流和表现资源 owner 未裁决。** tModLoader `WorldGen`/`Utils`/`Gore` 页面只确认公开 API；必须由 Version4 目标行为夹具确定边界，不能由 API 文档推断私有算法。
5. **现有 `AmbientWindStateComponent` 与新 work buffer 的最终组合 owner 未裁决。** 需要保证风速 weather owner、墓地环境 cadence owner 和 presentation owner 只有单向读依赖。
6. **目标项目和 verifier 路径未知。** 当前调研没有改变项目文件，也没有确定受影响 `.csproj`；下一实现任务开始前需从目标组合根确认项目、注册方式和 focused verifier 位置。
7. **P02 与本报告的重复治理未知。** 下一任务应把本报告作为选择审查，复用 P02 的 proposed 类型和阻塞项，不创建同义 `AmbientWindSystem`、`AmbientWindStateComponent` 或第二份 owner 记录。

建议的下一实现前置条件：

- 固化当前 Version4 stub 的行为表和调用者快照；
- 为 `ZoneGraveyard`、counter、30 tick cadence 建立纯输入/输出 fixture；
- 明确唯一 writer 和 compatibility facade；
- 先完成 shadow trace，再决定是否切换运行时 writer；
- 在 helper 行为、reset 生命周期和表现端口闭合前，不增加网络、存档、注册键或完整风效果迁移。

本调研阶段没有运行 `dotnet restore`、`dotnet build`、`dotnet test`、`dotnet run`，没有生成源码，没有修改生产代码、测试、账本、JSON、TSV 或既有报告，也没有提交 Git。
