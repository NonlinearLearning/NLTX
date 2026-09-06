# WorldProgressionAndTransition：Version4 独立只读审查与 ECS 拆分设计

> taskNumber: `06`  
> subsystemId: `WorldProgressionAndTransition`  
> layer: `authoritative-simulation`  
> currentNltxStatus: `partial`  
> reportPath: `D:\TRbackup\NLTX\docs\research\2026-09-05-version4-world-progression-and-transition-public-decomposition.md`  
> verificationStatus: `not-run`

## 1. 执行摘要

本报告只读审查 `WorldProgressionAndTransition`，范围是 Hardmode 等长周期世界过渡的触发、资格判断、计划、后台执行、Tile/生态变更、规则提交、存档、网络区段重同步、失败恢复和客户端结果投影。它不把常规世界 Tick、单个感染 Tile、单个矿阶选择、单个网络消息或视觉效果升格为一级子系统。

Version4 证据确认了以下控制链：Wall of Flesh 的 NPC 死亡事件路径先创建砖墙，再保存 Hardmode 之前的事件标志，然后调用 `WorldGen.StartHardmode()`；`StartHardmode` 在进入后台任务前把 `Main.hardMode` 置为 `true`，保护已经生成的世界物品，随后通过 `TransformWorldOnBackgroundThread` 执行 `initializeHardMode`，完成后广播、报告成就、重置网络区段并解除物品保护。该控制链的入口、状态写入、后台执行和区段重置边界可确认，但 Version4 的 `initializeHardMode` 为空，`EligibleForSpawnProtection` 也是空体，因此感染、墙体、生态与矿阶转换的真实实现不能仅凭 Version4 这一份源快照闭合。

完整可编译参考源码在同路径同名的 `WorldGen.cs` 中补出了 `initializeHardMode`，其内容包含随机方向、Remix 世界分支、感染 runner 和感染墙生成；同时补出服务端/客户端守卫和完整的 Hardmode 任务体。这只能作为 Version4 已有文件的成员级补证，不能替换 Version4 主覆盖基线。两端行号与守卫存在差异，已标记为 `full-reference-supplemented` 或 `version-drift`，没有宣称 Version4 与完整参考行为等价。

当前 NLTX 已有 Hardmode 布尔状态、矿阶状态、感染传播开关、世界生成生命周期、Hardmode 矿阶策略和感染/Tile 命令提交边界；但这些局部模型尚未形成一个从资格到计划、预算执行、提交屏障、一次性结果发布和恢复重试的 Hardmode transition executor。`WorldProgressionSystem` 当前推进的是血月、日食、灯笼夜、入侵和史莱姆雨，不能代替 Hardmode 长事务执行器。当前结论为 `partial`。

本报告提出但不落地以下 `status: proposed` 类型：`ProgressionTransitionState`、`TransitionPlan`、`EligibilityQuery`、生成/变更 Command、`CommitBarrier`、`ResyncAdapter` 与 `TransitionResultProjection`。所有跨 `WorldGenerationAndEcology`、`WorldStorage`、`ExternalBoundaries`、`WorldSession` 的共享 ID 与值对象均标记 `crossSubsystemOwner: integration-review`。

## 2. 子系统范围与不负责的内容

### 2.1 负责内容

| responsibilityId | 责任 | 证据状态 |
| --- | --- | --- |
| `HardmodeTrigger` | 接收 Wall of Flesh 等权威事件形成过渡请求，而不是直接在 NPC 中执行整条变更链 | `confirmed` |
| `TransitionEligibility` | 判断当前世界是否允许启动、是否已经启动、是否处于加载/生成/另一个变换中 | `partial` |
| `TransitionPlanning` | 根据世界规则、随机源、世界大小、地牢位置和当前区段版本形成可追踪计划 | `partial`；Version4 计划对象缺失 |
| `WorldMutationExecution` | 执行感染、墙体、生态和矿阶相关 pass，包含后台、预算、暂停与失败语义 | `partial`；核心方法在 Version4 为空 |
| `ControlledCommit` | 在预期世界/区段版本仍然成立时提交 Tile、矿阶与规则状态 | `partial`；NLTX 有局部 commit boundary，无 Hardmode 总屏障 |
| `ProgressionPersistence` | 保存和恢复 Hardmode、矿阶、感染及过渡恢复所需的长期状态 | `partial`；Version4 有字段，事务记录不足 |
| `SectionResynchronization` | 在过渡结果提交后使网络客户端重新获得一致的 WorldData 与 Tile 区段 | `partial`；ResetSections 可见，Version4 具体 Tile resync 方法为空 |
| `TransitionResultProjection` | 把已提交、不可变的结果投影到网络、存档、聊天和成就边界 | `partial`；投影分散在旧式静态入口 |

### 2.2 不负责内容

`WorldGenerationAndEcology` 仍负责生成 pass 的领域规则与 Tile 变更能力候选；本报告不宣布其最终 owner。`WorldStorage` 负责 Tile、区段版本和持久化快照的实际写入候选；本报告不把生成算法直接归给存储。`ExternalBoundaries` 负责网络、存档、聊天、成就和平台适配；本报告只定义单向输入/输出 seam，不重设计其完整协议。

常规天气、血月、日食、灯笼夜、入侵和史莱姆雨属于 `WorldCalendarAndEventOrchestration` 或既有 `WorldProgressionSystem` 的相邻责任；只有当它们成为 Hardmode 资格输入或结果投影时才作为只读依赖出现。

## 3. 证据优先级、来源角色与检索状态

本报告遵循以下优先级：

1. `D:\TRbackup\Version4`：确认 Terraria 私有实现、真实字段、写入者、调用方向和生命周期。
2. `D:\TRbackup\无任何删减通过编译`：只补证 Version4 已有文件的空实现、删减实现和相邻调用链。
3. `D:\TRbackup\tmodloader-api-docs-stable`：只交叉验证公开 Hook、存档、Hardmode 任务序列和网络边界。
4. `C:\Users\shan\Downloads\ECS\space-station-14-master`：只参考 Component、System、事件、迁移和结果投影的组织粒度，不推断 Terraria 行为。
5. `D:\TRbackup\NLTX\src`、`Test`、`dome\src`：只判定当前 NLTX 映射，不反向证明 Version4 行为。

本次读取并复核的 NLTX 约束包括 `AGENTS.md`、`progress.md`、公共审查协议、ECS 文件组织约束、C# 风格约束、副作用隔离约束、`dome\docs\flowstate\README.md`、公共拆分参考、Version4 子系统索引、覆盖表、差异附录和覆盖校验脚本。仓库中不存在根路径 `docs\flowstate\README.md`；实际权威副本是 `dome\docs\flowstate\README.md`，该路径差异记为 `evidence-mismatch`，不影响本报告主证据。

未读取其他并行子系统的中间报告，未启动子代理，未运行编译或测试命令。

## 4. Version4 真实代码证据表

| evidenceId | Version4 证据 | 读者/写者与生命周期 | 副作用 | evidenceStatus |
| --- | --- | --- | --- | --- |
| `V4-Trigger-NPC-WallOfFlesh` | `D:\TRbackup\Version4\Terraria\NPC.cs:65955-65965`，`NPC.NPCLoot` 的 `case 113` 先调用 `CreateBrickBoxForWallOfFlesh()`，保存 `bool eventFlag = Main.hardMode`，调用 `WorldGen.StartHardmode()`，再根据机械 Boss 条件广播并记录事件标志 | Wall of Flesh NPC 死亡路径是入口；写入者转交 `WorldGen.StartHardmode`；发生在游戏运行期 | Tile 墙体变更、全局规则状态改变、异步过渡请求 | `confirmed` |
| `V4-Hardmode-GuardAndRuleWrite` | `D:\TRbackup\Version4\Terraria\WorldGen.cs:26084-26094`，`StartHardmode(bool force = false)` 在 `Main.hardMode && !force` 时返回，随后写入 `Main.hardMode = true`，调用物品保护和后台变换 | `NPC` 读取 `Main.hardMode`；`WorldGen` 是直接写者；生命周期从请求到后台完成 | 共享可变状态写入、物品保护、后台任务启动 | `confirmed` |
| `V4-SpawnProtection` | `D:\TRbackup\Version4\Terraria\WorldGen.cs:26052-26068`，`TryProtectingSpawnedItems`/`UndoSpawnedItemProtection` 扫描 400 个世界物品；`EligibleForSpawnProtection` 在 `:26066-26068` 为空体 | `StartHardmode` 调用前后使用；物品槽位是读写对象 | 直接修改 `timeSinceItemSpawned`；资格逻辑在 Version4 快照中缺失 | `partial` |
| `V4-BackgroundTransform` | `D:\TRbackup\Version4\Terraria\WorldGen.cs:26103-26121`，`TransformWorldOnBackgroundThread` 增加 `_transformingWorld`，启动 `Task.Factory.StartNew`，在 `lock (WorldFile.IOLock)` 中调用 transform，finally 中减少计数并排队主线程 follow-up | `StartHardmode` 提供 `initializeHardMode` 与 follow-up；`WorldFile.SaveWorld` 读取变换状态并共享同一锁 | 后台线程、锁、任务异常传播、主线程回调；没有可见取消、进度、重试或错误结果端口 | `confirmed` |
| `V4-InitializeHardMode-Gap` | `D:\TRbackup\Version4\Terraria\WorldGen.cs:26083` 为 `private static void initializeHardMode(){}`；调用存在但核心世界修改体为空 | 由 `StartHardmode` 后台调用；没有 Version4 内部写者可追踪 | 核心变更算法不可从该快照确认 | `evidence-mismatch` |
| `V4-Followup` | `D:\TRbackup\Version4\Terraria\WorldGen.cs:26094-26100`，后台完成后广播 `Lang.misc[15]`、`AchievementsHelper.NotifyProgressionEvent(9)`、`Netplay.ResetSections()`、解除物品保护 | 主线程 follow-up 读取后台结果但没有显式成功/失败参数；网络和平台为外部边界 | 消息、成就、网络区段状态、物品时间回滚 | `partial`；失败时是否仍执行 follow-up 需要运行时 verifier |
| `V4-WorldUpdateGate` | `D:\TRbackup\Version4\Terraria\WorldGen.cs:59408-59418`，`UpdateWorld` 以 `hardModeWorldUpdates = Main.hardMode || (...)` 决定生态更新门控，并在 `isGeneratingOrLoadingWorld` 时提前返回（`WorldGen.cs:59402-59408`） | 世界 Tick 读取 `Main.hardMode` 与加载/生成状态；Hardmode 结果影响后续生态传播 | 每个世界 Tick 的规则分支、感染/生态更新 | `confirmed`；具体感染转换算法仍 `partial` |
| `V4-WorldReset` | `D:\TRbackup\Version4\Terraria\WorldGen.cs:7228-7240` 在清理时把 `SavedOreTiers` 各阶置为 `-1` 并把 `Main.hardMode` 置为 `false` | 世界创建/重置生命周期写者；后续 WorldFile 加载重新恢复 | 清空权威进度，影响存档恢复 | `confirmed` |
| `V4-WorldFileLockAndSaveWait` | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:96` 定义 `IOLock`；`:878-931` 的 `SaveWorld`/`_SaveWorld` 等待 `WorldGen.TransformingWorld`，再通过 `Monitor.TryEnter(IOLock)` 或 `lock (IOLock)` 保存 | `WorldGen` 与 `WorldFile` 共享锁；存档是后台变换的相邻消费者 | 文件 I/O、阻塞等待、临时文件/异常处理 | `confirmed` |
| `V4-WorldFileHardmode` | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1263-1345` 的 `SaveWorldHeader` 写入 `Main.hardMode`；`:1808-1902` 负责分段加载；`:2155-2163` 将存档值读回 `Main.hardMode` | `WorldFile` 读写长期进度；`Main.hardMode` 是运行时读者 | 二进制持久化、版本兼容、加载恢复 | `confirmed` |
| `V4-WorldFileDataMetadata` | `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:471-484` 把头部 `IsHardMode` 放入 `WorldFileData`；`D:\TRbackup\Version4\Terraria.IO\WorldFileData.cs:39-69` 声明 `WorldId`、`UniqueId`、`LoadStatus`、`LoadException` 和 `IsHardMode` | 世界选择/广播元数据读者；与 `Main.hardMode` 并存 | 元数据读取、错误世界构造、潜在双写/双读不一致 | `partial`；两份 Hardmode 表示的最终权威归属未闭合 |
| `V4-WorldDataPacket` | `D:\TRbackup\Version4\Terraria\NetMessage.cs:236-240` 写世界 ID、名称、模式、唯一 ID和生成器版本；`:270-288` 的 `bitsByte6[4] = Main.hardMode` 把 Hardmode 放入 WorldData | 服务端 `NetMessage` 写入；客户端接收路径在该片段外；外部边界读取已提交事实 | 网络序列化、客户端状态更新 | `confirmed`；客户端应用顺序 `partial` |
| `V4-SectionReset` | `D:\TRbackup\Version4\Terraria\Netplay.cs:134-143` 的 `ResetSections` 清空每个 RemoteClient 的 `TileSections`；`D:\TRbackup\Version4\Terraria\NetMessage.cs:2432-2450` 的 `SendSection` 按 200×150 区段发送 Tile、NPC 和箱子 | Hardmode follow-up 调 `ResetSections`；后续客户端请求/广播触发区段再发送 | 修改连接会话缓存、批量网络发送 | `confirmed` |
| `V4-ResyncTilesGap` | `D:\TRbackup\Version4\Terraria\NetMessage.cs:2418-2431` 的 `ResyncTiles(Rectangle area)` 遍历活动客户端，但私有 `ResyncTiles(int clientId, Rectangle area)` 为空体 | 外部网络边界的直接写者；没有实际 Tile resync 实现可追踪 | 预期网络 I/O 的执行体缺失 | `evidence-mismatch` |
| `V4-ServerDiscoveryHardmode` | `D:\TRbackup\Version4\Terraria\Netplay.cs:525-541` 的广播数据写入 `Main.ActiveWorldFileData.IsHardMode` | 服务器发现/广播读取 `WorldFileData` 元数据；与 WorldData 中 `Main.hardMode` 形成两个读取面 | UDP/广播协议输出 | `partial`；两个字段的同步契约未闭合 |

### 4.1 Version4 真实调用链

```text
NPC.NPCLoot(case WallOfFlesh)
  -> CreateBrickBoxForWallOfFlesh
  -> capture eventFlag = Main.hardMode
  -> WorldGen.StartHardmode()
       -> if Main.hardMode && !force: return
       -> Main.hardMode = true
       -> TryProtectingSpawnedItems
       -> TransformWorldOnBackgroundThread(initializeHardMode, mainThreadFollowup)
            -> increment _transformingWorld
            -> background Task
                 -> lock WorldFile.IOLock
                 -> initializeHardMode
                 -> decrement _transformingWorld
                 -> QueueMainThreadAction(mainThreadFollowup)
       -> mainThreadFollowup
            -> world announcement
            -> achievement progression
            -> Netplay.ResetSections
            -> UndoSpawnedItemProtection
```

该链表明 `StartHardmode` 是独立的长事务入口，而不是普通 `UpdateWorld` 内的一次生态 Tick。它没有在 Version4 代码中把资格、计划、每个 pass 的结果、提交版本、失败原因或重试次数显式化。

### 4.2 关键失败语义

- `force = false` 只提供布尔短路，不提供过渡 ID 或幂等记录；并发调用的竞态、强制重复调用和重复 follow-up 不能由 Version4 片段证明安全。
- `TransformWorldOnBackgroundThread` 的 `finally` 会减少计数并排队 follow-up，但 Version4 片段没有把 transform 异常转换成失败结果；若 `initializeHardMode` 抛错，任务可能故障而 follow-up 仍被排队。这是 `partial` 的高风险行为，必须由 focused verifier 复现或排除。
- `Main.hardMode` 在后台变换完成前已为 `true`，而 Tile/墙体/感染的提交顺序在 Version4 空方法中不可见。
- `WorldFile.SaveWorld` 等待 `TransformingWorld`，但没有证明网络 WorldData、存档写入和 Tile 区段重置共享同一原子提交边界。

## 5. 完整参考源码补证表

| evidenceId | 完整参考证据 | 对 Version4 的补证范围 | 状态 |
| --- | --- | --- | --- |
| `FR-InitializeHardMode` | `D:\TRbackup\无任何删减通过编译\Terraria\WorldGen.cs:32145-32277`，`initializeHardMode` 初始化随机源，计算转换方向，处理 Remix 分支，调用 `GERunner`，再生成感染墙 | 补出 Version4 `WorldGen.cs:26083` 空方法可能对应的真实成员级实现；能确认算法形状，不把它写成 Version4 已有实现 | `full-reference-supplemented` |
| `FR-StartHardmode` | `D:\TRbackup\无任何删减通过编译\Terraria\WorldGen.cs:32279-32298`，增加 `Main.netMode == 1` 客户端短路，服务器模式下才 `Netplay.ResetSections` | 补证同名方法的完整网络守卫和调用条件 | `version-drift`；Version4 `:26088-26099` 未显示相同守卫 |
| `FR-TransformWorld` | `D:\TRbackup\无任何删减通过编译\Terraria\WorldGen.cs:32301-32325` 与 Version4 `:26103-26121` 对应，均使用 `_transformingWorld`、`WorldFile.IOLock` 和主线程 follow-up | 补证后台/锁/回调的同名调用邻域 | `full-reference-supplemented` |
| `FR-NPC-ServerGuard` | `D:\TRbackup\无任何删减通过编译\Terraria\NPC.cs:80708-80718` 在 Wall of Flesh 路径外层检查 `Main.netMode != 1`，再调用 `StartHardmode` | 说明完整参考的触发入口具有客户端守卫；Version4 `NPC.cs:65955-65965` 的对应片段未显示该守卫 | `version-drift` |
| `FR-WorldFile` | `D:\TRbackup\无任何删减通过编译\Terraria.IO\WorldFile.cs:1342` 写 `Main.hardMode`，`:2159` 读回 `Main.hardMode`，与 Version4 对应位置一致 | 补强二进制存档字段顺序证据 | `full-reference-supplemented` |
| `FR-OreProgression` | `D:\TRbackup\无任何删减通过编译\Terraria\WorldGen.cs:49650-49700` 的 `SmashAltar` 读取和改变 `SavedOreTiers`，并根据过渡周期选择 Cobalt/Mythril/Adamantite | 证明矿阶推进不仅是静态字段，还具有事件驱动的写入逻辑；不把完整逻辑回填到 Version4 | `full-reference-supplemented` |

完整参考源码没有改变 Version4 的主证据结论：Version4 的入口和调度骨架存在，但转换算法、失败传播和网络重同步闭环仍需要以目标运行时或更完整目标源码继续闭合。

## 6. tModLoader 公开 API 交叉验证

来源：`D:\TRbackup\tmodloader-api-docs-stable\index.html`，页面标题 `tModLoader Documentation`，页眉版本 `tModLoader v2026.07`（首页 `:8-10`、`:27-31`）。以下文档只用于公开边界交叉验证，不替代 Version4 私有行为。

| apiEvidenceId | 实际文档路径、标题、锚点 | 明确事实 | 仅用于 |
| --- | --- | --- | --- |
| `TML-ModifyHardmodeTasks` | `D:\TRbackup\tmodloader-api-docs-stable\class_mod_system.html:139-141`，`ModSystem Class Reference`，锚点 `#ab3224849ab5d16570434c13e66a4eb9e` | `ModifyHardmodeTasks(List<GenPass>)` 在游戏内 Hardmode 开始时修改 Hardmode 任务列表；默认有 Good Remix、Good、Evil、Walls、Announcement 五类任务 | 证明“Hardmode 过渡由有序任务序列组成”的公开扩展边界；不证明 Version4 的具体 pass 实现 |
| `TML-WorldLifecycle` | `D:\TRbackup\tmodloader-api-docs-stable\class_mod_system.html:186-192`、`:258-273`，`OnWorldLoad`、`OnWorldUnload`、`PostUpdateWorld`、`PostWorldGen` | 世界加载/卸载、常规世界更新和生成完成各有不同生命周期 Hook；`PostUpdateWorld` 只在单机或服务器调用，`PostWorldGen` 用于生成完成后的 Tile 放置 | 区分启动/生成/运行时 Tick/过渡结果的生命周期；不把 Hook 当作 ECS owner |
| `TML-SaveWorldData` | `D:\TRbackup\tmodloader-api-docs-stable\class_mod_system.html:1078-1105`，锚点 `#a926129e278ac9c460685bd2a326cd998` | `SaveWorldData(TagCompound)` 保存世界特有标志；文档以 Boss 击败事实为例 | 支持把长期进度放入世界会话/持久化投影，而不是 NPC 实例 |
| `TML-NetWorldData` | `D:\TRbackup\tmodloader-api-docs-stable\class_mod_system.html:839-896`，`NetReceive` 锚点 `#a144ef...`、`NetSend` 锚点 `#af9ebf...` | `NetReceive` 在客户端成功收到 `MessageID.WorldData` 后调用；`NetSend` 在服务器发送世界数据时调用，可用于同步已击败 Boss 等世界事实 | 支持 `TransitionResultProjection` 的单向服务器到客户端边界 |
| `TML-WorldDataMessage` | `D:\TRbackup\tmodloader-api-docs-stable\class_message_i_d.html:761-770`，锚点 `#a33925ec118e69fc85aced712e1ca0bb5` | `WorldData = 7` 从服务器向客户端发送时间、天气、事件、世界尺寸、名称、生物群系、Boss 等，并应在世界状态变化时发送 | 交叉验证 Hardmode 结果发布时需要 WorldData/等价结果投影；不证明区段 Tile 重传 |
| `TML-WorldFileData-Hardmode` | `D:\TRbackup\tmodloader-api-docs-stable\class_world_file_data.html:193-202`，`WorldFileData Class Reference`，成员 `IsHardMode` | 公开元数据模型暴露 `IsHardMode` 字段，但没有说明 Version4 私有写入时序 | 说明 `WorldFileData.IsHardMode` 是公开元数据边界；不能用它替代 Version4 `Main.hardMode` 所有权分析 |

## 7. Space Station 14 最小相关 ECS 参考

本次只读取以下实际文件；它们只用于结构粒度参考。Space Station 14 无直接对应 Terraria Hardmode、感染转换或区段提交屏障的领域证据，以下边界不能反推 Terraria 行为。

| ss14EvidenceId | 实际读取文件与类型/方法 | 参考用途 | 结论限制 |
| --- | --- | --- | --- |
| `SS14-GameRule-Component` | `C:\Users\shan\Downloads\ECS\space-station-14-master\Content.Shared\GameTicking\Components\GameRuleComponent.cs:9-31`，`GameRuleComponent`；`GameRuleAddedEvent`、`GameRuleStartedEvent`、`GameRuleEndedEvent` | 一个规则实体保存激活时间、人数门槛和延迟；规则加入、开始、结束是不同事件 | 只参考“状态 + 生命周期事件”粒度，不复制 `GameRule` 命名或语义 |
| `SS14-GameRule-System` | `...\Content.Server\GameTicking\GameTicker.GameRule.cs:76-204`，`AddGameRule`、`StartGameRule`、`EndGameRule` | 添加、延迟开始、激活标志、结束幂等和开始/结束事件由 System 控制；`StartGameRule` 防止已经 active/ended 的重复启动 | 可参考 `Eligibility -> Planned/Active -> Ended` 的显式状态边界；不证明 Hardmode 任务或 Tile 变更 |
| `SS14-RoundCleanup` | `...\Content.Shared\GameTicking\RoundRestartCleanupEvent.cs:1-9`，`RoundRestartCleanupEvent`；并在 `...\Content.Server\Wires\WiresSystem.cs:47-55`、`:869-872` 读取订阅/清理路径 | 通过事件通知不同 System 清理轮次状态，避免所有状态集中到一个清理类 | 只参考跨 System 清理通知；不作为 Terraria rollback 证据 |
| `SS14-MapMigration` | `...\Content.Server\Maps\MapMigrationSystem.cs:12-74`，`BeforeEntityReadEvent`、`OnBeforeReadEvent` | 持久化加载前通过显式适配器处理 prototype 重命名和删除，隔离版本迁移 | 可参考 `PersistenceAdapter` 的恢复/迁移 seam；不证明 WorldFile 的 Hardmode 恢复顺序 |

对 `round transition`、`world generation pass`、`commit barrier`、`rollback`、`network resync` 的检索没有发现可直接对应 Terraria Hardmode 的统一类型。报告中的 `CommitBarrier`、`ResyncAdapter` 和恢复状态因此完全属于基于 Version4 与 NLTX 约束的 `proposed` 设计。

## 8. 成员、字段、方法、读写者和生命周期盘点

| memberId | 当前成员/事实 | 状态类别 | 读者 | 写者 | 生命周期/频率 | 候选归属 | 状态 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `main-hardmode` | `Main.hardMode`，Version4 `Main.cs:495` | 权威规则结果，同时是兼容字段 | `NPC`、`WorldGen.UpdateWorld`、`WorldFile`、`NetMessage`、大量刷怪/玩法路径 | `WorldGen.StartHardmode`、WorldFile load/reset | 世界加载、Wall of Flesh 事件、每 Tick 读取 | `WorldSession` 的规则状态；过渡写入由本子系统协调 | `confirmed` |
| `world-file-hardmode` | `WorldFileData.IsHardMode`，Version4 `WorldFileData.cs:67` | 持久化/广播元数据候选 | `Netplay`、WorldFile header、世界选择 | WorldFile metadata load/create | 世界元数据生命周期 | `WorldStorage` 或 `ExternalBoundaries` 候选 | `partial` |
| `saved-ore-tiers` | Version4 `WorldGen.SavedOreTiers`，清理写入 `WorldGen.cs:7228-7234`，存档读写见 `WorldFile.cs:2395-2408` 附近 | 权威长期进度 | OrePatch、SmashAltar、WorldFile、WorldGen | 生成初始化、SmashAltar、WorldFile | 世界生成、Hardmode 后祭坛事件、存档 | `WorldGenerationAndEcology` 候选，跨域 owner 待整合 | `partial` |
| `infection-spread-gate` | 当前 NLTX `WorldRulesState.InfectionSpreadAllowed`，`src\WorldSession\WorldSessionComponents.cs:40-48`；`WorldEcologyScheduleState.IsInfectionSpreadAllowed`，`src\WorldSession\WorldGeneration\WorldEcologyScheduleState.cs:3-18` | 规则状态 + 生态调度派生值 | 生态系统、世界更新 | 当前模型写入者未形成 Hardmode 事务 | 每个生态更新周期 | 规则归 `WorldSession`，调度归 `WorldGenerationAndEcology` | `partial` |
| `transform-counter` | Version4 `_transformingWorld`，`WorldGen.cs:26107-26119` | 运行时并发/门控状态 | `TransformingWorld` 属性、WorldFile save、世界更新 | 后台任务 finally | 每个后台变换 | `ProgressionTransitionState` 的内部/兼容投影候选 | `confirmed` |
| `item-protection` | `TryProtectingSpawnedItems`/`UndoSpawnedItemProtection`，`WorldGen.cs:26052-26080` | 临时兼容状态/效果保护 | Hardmode follow-up、WorldItem 槽位 | WorldGen | 过渡前后一次 | 过渡执行器的 effect port，不是长期 Component | `partial` |
| `world-generation-lifecycle` | 当前 `WorldGenerationLifecycleState`，`src\WorldSession\WorldGeneration\WorldGenerationLifecycleState.cs:3-15` | 权威准备状态 | 生成管线、世界更新门控 | 生成生命周期 System | 加载/生成/Ready/Failed | `WorldGenerationAndEcology` | `confirmed` |
| `generation-progression` | 当前 `WorldGenerationProgressionState`，`dome\src\Terraria.Dome.Simulation\WorldGeneration\WorldGenerationProgressionState.cs:3-25` | 生成期状态快照 | 生成阶段与 trace | 生成管线 | 一次生成事务 | `WorldGenerationAndEcology` | `confirmed` |
| `world-progression-state` | 当前 `WorldProgressionState.IsHardMode`，`dome\src\Terraria.Dome.Simulation\World\WorldProgressionState.cs:10-20`、`:116-168` | 只读/不可变的事件进度状态 | `WorldProgressionSystem`、日历事件规则 | `With...` 值替换 | 常规事件 Tick | `WorldSession`；不能承担 Tile 过渡执行 | `partial` |
| `world-progression-transition` | 当前 `WorldProgressionTransition`，`dome\src\Terraria.Dome.Simulation\World\WorldProgressionTransition.cs:3-8` | 常规事件的结果记录 | 事件投影/调用方 | `WorldProgressionSystem` 之外的调用方候选 | 单个事件 Tick | `WorldCalendarAndEventOrchestration` | `confirmed`；不是 Hardmode 事务 |
| `world-generation-pipeline` | 当前 `WorldGenerationPipeline` 在 `dome\src\Terraria.Dome.Simulation\WorldGeneration\WorldGenerationPipeline.cs:39-70` 依次生成 Terrain/Cave 等阶段；`:409-425` 进入 Committed 并做验证；`:445-452` 拒绝 `Rules.IsHardmode` | 生成事务执行器/验证器 | 世界生成启动 | `WorldGenerationPipeline` | 新世界创建 | `WorldGenerationAndEcology` | `confirmed`；明确拒绝 Hardmode 规则 |
| `infection-commit-boundary` | `LegacyWorldInfectionConversionCommitBoundary.TryCommit`，`dome\src\Terraria.Dome.Simulation\WorldGeneration\LegacyWorldInfectionConversionCommitBoundary.cs:16-32`、`:47-74`、`:282-294` | 局部 Tile 转换 Command commit | Infection pass、WorldGrid、TileChangeCommitSystem | 过渡/生成调用方 | 每批感染命令 | `WorldGenerationAndEcology` 候选 | `confirmed`；不是 Hardmode 总屏障 |
| `tile-runner-commit-boundary` | `LegacyTileRunnerCommandCommitBoundary.TryCommit`，`dome\src\Terraria.Dome.Simulation\WorldGeneration\LegacyTileRunnerCommandCommitBoundary.cs:23-77`、`:85-149` | 局部 Tile/Liquid 批提交与区段版本检查 | 生成 pass | pass 调用方 | 每批命令 | `WorldStorage` 提交能力候选 | `confirmed`；不包含规则/网络/存档原子性 |
| `section-replication` | `WorldSectionReplication.CreateChangedWorldStream`，`dome\src\Terraria.Dome.Server\Replication\WorldSectionReplication.cs:99-113`；requested stream `:127-150` | 已变化区段的 Projection | 客户端会话 | `WorldGrid` snapshot/version、会话 replication state | 每个复制周期/请求 | `ExternalBoundaries` | `confirmed` |
| `world-save-coordinator` | `WorldSaveCoordinator.Save/TryLoad`，`dome\src\Terraria.Dome.Server\Persistence\WorldSaveCoordinator.cs:12-55`、`:95-106` | 持久化 Adapter | World snapshot | Save/Load 调用方 | 显式存档 | `ExternalBoundaries` / `WorldStorage` integration-review | `confirmed`；未接 Hardmode result |

## 9. 权威状态所有权表

| stateId | 当前/目标状态 | proposed owner | 可确认写入根 | 不能由谁直接写 | 所有权状态 |
| --- | --- | --- | --- | --- | --- |
| `WorldHardmodeRule` | 是否已经进入 Hardmode 的长期规则结果 | `WorldSession` 的规则组件，由 `HardmodeCommitSystem` 唯一写入 | Version4 `WorldGen.StartHardmode` 与 WorldFile load/reset | NPC、网络接收、Projection | `integration-review` |
| `HardmodeTransitionLifecycle` | `Requested`、`Planned`、`Executing`、`ReadyToCommit`、`Committed`、`Failed`、`Recovering` | proposed `ProgressionTransitionState`，本子系统独占 | proposed `TransitionCoordinatorSystem` | 普通 `WorldProgressionSystem`、客户端、存档 Adapter | `proposed` |
| `HardmodeTileMutation` | 感染、墙体、生态和矿阶 Tile 命令/批次 | proposed `TransitionPlan` 产生 Command；`WorldStorage` 负责最终应用 | proposed `HardmodeMutationSystem` + `CommitBarrier` | 生成 pass 不能绕过屏障直接写权威 WorldGrid | `integration-review` |
| `HardmodeOreTier` | Cobalt/Mythril/Adamantite 等持久矿阶 | `WorldGenerationAndEcology` 候选；提交由 `WorldStorage`/WorldSession 协作 | Version4 `SmashAltar` 和 WorldFile | 网络/客户端不能写 | `integration-review` |
| `WorldSectionVersion` | 区段变更版本与 expected version | `WorldStorage` | 当前 NLTX Tile/Liquid commit systems | 计划器不得伪造版本 | `integration-review` |
| `TransitionResult` | 只读的已提交结果，包括 TransitionId、版本、改变区段、规则快照 | proposed Projection 输入 | `CommitBarrier` 产出不可变结果 | Projection 不反写规则 | `proposed` |

## 10. proposed ECS 组件拆分

以下全部是设计草图，均为 `status: proposed`，不表示当前路径或类型已经存在。

| proposedTypeId | 类型/建议路径 | 最小数据 | 读写边界与理由 |
| --- | --- | --- | --- |
| `ProgressionTransitionState` | `World/Progression/ProgressionTransitionState.cs`，`status: proposed` | `TransitionId`、`Phase`、`TriggerKind`、`RequestedAtTick`、`PlanRevision`、`GenerationRevision`、`CommitSequence`、`RetryCount`、`FailureCode`、`ExpectedWorldRevision` | 只保存长事务生命周期，不复制全部 `WorldRulesState`、Tile 或网络游标；由过渡 System 写，Query 只读 |
| `TransitionPlan` | `World/Progression/TransitionPlan.cs`，`status: proposed` | `PlanId`、`PlanRevision`、`WorldRevision`、`Seed/RandomCursor`、`MutationBatches`、`OreSelection`、`SectionPreconditions`、`Budget`、`RequiresResync` | 只读不可变计划；把随机选择、区域和批次从执行器中分离，便于重放、审计和重试 |
| `WorldProgressionRuleState` | `World/Progression/WorldProgressionRuleState.cs`，`status: proposed` | Hardmode 最终规则、规则版本、最后提交序列；其他长期事件通过相邻组件保留 | 避免将 Hardmode 标志、过渡阶段、临时任务和复制状态再次合并成巨型组件 |
| `TransitionSectionPrecondition` | `World/Progression/TransitionSectionPrecondition.cs`，`status: proposed`, `crossSubsystemOwner: integration-review` | `WorldSectionId`、expected version、预计变更数量 | 表达计划生成时看到的区段版本；不拥有区段内容 |
| `TransitionRecoveryState` | `World/Progression/TransitionRecoveryState.cs`，`status: proposed` | `LastSafePhase`、未提交批次、可重试原因、是否结果未知、恢复次数 | 只保存可恢复元数据；不把失败的部分 Tile 重新伪装成成功 |

建议不把后台 Task、`WorldFile.IOLock`、网络连接对象、随机实例或可变集合直接塞进 Component。它们应留在 System 的显式依赖或 Port 中，结果以不可变值返回。

## 11. proposed Query、System、Command、Adapter、Projection 契约

### 11.1 Query

```text
status: proposed
EligibilityQuery.Evaluate(
  WorldProgressionRuleState ruleState,
  WorldGenerationLifecycleState generationState,
  WorldRevisionSnapshot worldRevision,
  TransitionRequest request)
  -> EligibilityDecision
```

`EligibilityQuery` 必须是纯计算：不读文件、时间、随机源、网络或可变全局；输入中显式提供当前 Tick、随机游标、生成状态、Hardmode 规则、`force` 意图和是否已有活动过渡。返回 `Eligible`、`AlreadyCommitted`、`Busy`、`ClientOnly`、`InvalidWorld` 或 `UnsupportedRules` 等稳定结果。`WorldGen.StartHardmode` 的短路事实是 Version4 证据，但查询化是 `proposed`。

### 11.2 Commands

| proposed command | 输入 | 输出/失败 | 写入者 |
| --- | --- | --- | --- |
| `RequestWorldProgressionTransitionCommand` | `TransitionId`、触发事件、请求 Tick、force、来源实体/事件 ID | 进入资格判断；重复 ID 返回幂等结果 | `TransitionRequestIngressSystem` |
| `GenerateHardmodeMutationBatchCommand` | `PlanId`、区段快照、矿阶选择、预算、随机游标 | Tile/墙/生态命令集合；不可直接写 WorldGrid | `HardmodeMutationSystem` |
| `ApplyHardmodeTileBatchCommand` | `PlanId`、批序号、Tile/Liquid commands、expected section versions | `Applied/Conflict/Rejected/CapacityFailure` | `CommitBarrier` 调用 `WorldStorage` seam |
| `CommitHardmodeRuleCommand` | `TransitionId`、已完成批次、WorldRevision、矿阶结果 | `Committed` 或冲突/失败 | `HardmodeCommitSystem` |
| `RetryTransitionCommand` | 原 `TransitionId`、失败类别、恢复点、重试预算 | 新批次或终止结果；不得无界重试 | `TransitionRecoverySystem` |

### 11.3 Systems

| proposed system | Interface / Implementation | Seam | Depth / Leverage / Locality | 输入 -> 输出 |
| --- | --- | --- | --- | --- |
| `TransitionRequestIngressSystem` | `IWorldProgressionRequestSink` / `TransitionRequestIngressSystem`，`status: proposed` | NPC 事件、Boss 事件到 Command | 深度低、杠杆高、局部性高；只转换意图 | 事件 -> request Command |
| `TransitionEligibilitySystem` | `IEligibilityEvaluator` / `EligibilityQuery`，`status: proposed` | 纯函数 Query | 深度中、杠杆高、局部性高；不写状态 | request + snapshots -> decision |
| `TransitionPlanningSystem` | `ITransitionPlanner` / `HardmodeTransitionPlanningSystem`，`status: proposed` | 随机源、世界规则、WorldGeneration pass registry | 深度中、杠杆高；计划是可重放 seam | decision -> `TransitionPlan` |
| `HardmodeMutationSystem` | `ITransitionMutationExecutor` / `HardmodeMutationSystem`，`status: proposed` | 后台执行器、预算时钟、取消/故障端口 | 深度高、杠杆高、局部性中；只产出批次 | plan -> mutation batch/result |
| `CommitBarrierSystem` | `ICommitBarrier` / `HardmodeCommitBarrierSystem`，`status: proposed`, `crossSubsystemOwner: integration-review` | WorldStorage version compare-and-commit | 深度高、杠杆高；是唯一跨域写入门 | batch + preconditions -> committed storage result |
| `HardmodeRuleCommitSystem` | `IProgressionRuleWriter` / `HardmodeRuleCommitSystem`，`status: proposed` | WorldSession 权威状态 | 深度中、杠杆高、局部性高 | committed mutation -> rule state |
| `TransitionRecoverySystem` | `ITransitionRecoveryPolicy` / `TransitionRecoverySystem`，`status: proposed` | 持久化恢复记录、可重试错误分类 | 深度中、杠杆中；必须明确至少一次/至多一次语义 | failure -> retry/terminal result |
| `TransitionResultProjectionSystem` | `ITransitionResultProjector` / `TransitionResultProjectionSystem`，`status: proposed` | network、persistence、chat、achievement ports | 深度低、杠杆高、局部性高 | immutable result -> projections |

### 11.4 Adapter 与 Projection

```text
status: proposed
interface IWorldSectionResyncPort
{
  ResyncReceipt Publish(WorldTransitionResult result);
}

status: proposed
interface IWorldProgressionPersistencePort
{
  PersistenceReceipt Persist(WorldTransitionResult result);
  RecoverySnapshot? LoadRecovery(WorldIdentity identity);
}

status: proposed
interface IProgressionAnnouncementPort
{
  void Publish(WorldTransitionResult result);
}
```

`ResyncAdapter` 负责把已提交的 `TransitionResult` 转成区段版本失效、WorldData 和区段快照请求；它不直接改 `WorldProgressionRuleState`。`TransitionResultProjection` 只读取一次提交产生的不可变结果，并携带 `TransitionId`、`CommitSequence` 和 section version，外部端按 ID 去重。聊天和成就属于同一结果的观察性投影，不能成为规则提交的触发器。

## 12. 系统顺序、调用方向与提交屏障

### 12.1 建议顺序

```text
WallOfFlesh / authoritative event
  -> TransitionRequestIngressSystem
  -> TransitionEligibilitySystem / EligibilityQuery
  -> TransitionPlanningSystem
  -> HardmodeMutationSystem
       -> WorldGenerationAndEcology pass capability
       -> Tile/Liquid mutation commands
  -> CommitBarrierSystem
       -> WorldStorage version check
       -> apply Tile/Liquid batches
       -> persist ore-tier result in the same logical commit record
  -> HardmodeRuleCommitSystem
       -> WorldSession Hardmode rule state
  -> TransitionResultProjectionSystem
       -> ResyncAdapter / WorldData projection
       -> PersistenceAdapter
       -> announcement/achievement adapters
```

必须显式保持以下顺序：

1. 入口事件只形成 Command，不直接写 `Main.hardMode` 或 Tile。
2. 资格判断先于计划，计划先于变更批次；纯 Query 不得产生副作用。
3. 变更执行先读取不可变世界/区段快照，批次带 expected version。
4. `CommitBarrier` 先确认所有必需批次完成、世界未重新加载、expected version 未冲突，再提交 Tile/Liquid 与矿阶事实。
5. Hardmode 规则结果只能由 `HardmodeRuleCommitSystem` 写入；网络/存档投影读取 committed result。
6. 区段失效和 WorldData 投影在规则提交之后；同一个 `TransitionId` 不得重复应用。
7. 失败时进入 `Failed`/`Recovering`，不能发送成功公告或把失败批次作为 committed result。

### 12.2 Version4 行为保持的提交时序决策

Version4 在后台变换开始前就写 `Main.hardMode = true`。因此不能在迁移设计中无声地把规则写入延后到所有 Tile 变更完成；那会改变刷怪、生态和其他 `Main.hardMode` 读者的观察时序。最终整合需在以下方案中裁决：

| optionId | 方案 | 好处 | 风险 |
| --- | --- | --- | --- |
| `PreserveEarlyRule` | 保留早期兼容字段为 `true`，新增 `TransitionPhase` 区分“规则已宣布”和“世界变更已提交” | 最接近 Version4 已观察时序 | 业务读者必须迁移到 phase-aware Query，避免把早期字段当作完整完成 |
| `CommitAfterMutation` | 等所有批次通过后才写权威 Hardmode | 提交一致性更简单 | 可能改变原版 Hardmode 读者、刷怪和保存时序 |
| `DualPhaseProjection` | 内部使用两阶段规则事实，旧字段只作为兼容投影，明确其早期/最终语义 | 可同时保留时序并表达一致性 | 需要禁止新旧字段双写，并增加恢复与协议版本复杂度 |

该选择改变权威状态所有权和 System 顺序，标记为 `blocking-decision`，本报告不自行裁决。

## 13. 持久化、网络和客户端投影边界

### 13.1 持久化

Version4 的 `WorldFile.SaveWorld` 使用 `WorldGen.TransformingWorld` 和 `WorldFile.IOLock` 保护保存时机，但它没有一个可审计的 `TransitionId`、计划版本、已应用批次列表或结果未知标记。proposed `PersistenceAdapter` 应保存：

- 世界身份：`WorldId`、`UniqueId`、`WorldRevision`；
- `TransitionId`、`PlanRevision`、`CommitSequence`；
- 最终 Hardmode rule snapshot 和矿阶 snapshot；
- 已提交区段版本摘要；
- `Failed`/`Recovering` 的恢复点与错误类别；
- 兼容的 `Main.hardMode`/`WorldFileData.IsHardMode` 投影值。

适配器应采用临时文件、原子替换或已有持久化机制的等价边界，并把写入成功、失败、超时未知和恢复行为写入 verifier。不能仅因两个连续写调用都返回成功就宣称规则、Tile、矿阶和网络已原子提交。

### 13.2 网络

Version4 WorldData 的 Hardmode 位是 `Main.hardMode` 的 bit，区段同步由 `ResetSections` 失效后再 `SendSection`。建议 `ResyncAdapter` 按以下协议工作：

1. `CommitBarrier` 产生一个 immutable `TransitionResult`。
2. Adapter 使受影响区段的旧版本失效；若选择保守兼容模式，可整体重置区段缓存。
3. 服务端发送包含 Hardmode/矿阶/事件结果的 WorldData 或等价 snapshot。
4. 客户端按区段版本请求/接收 Tile、TileEntity、NPC 和箱子投影。
5. Adapter 记录 `TransitionId` 和 `CommitSequence`，重复结果只确认不重复应用。

Version4 `ResyncTiles` 私有实现为空，因此具体“全量失效后按可见区段发送”还是“仅发送变化区段”不能从 Version4 主证据选定。NLTX 现有 `WorldSectionReplication.CreateChangedWorldStream` 可作为 proposed resync seam 的局部参考，但它尚未与 Hardmode transition result 连接。

### 13.3 客户端

客户端只接收 `WorldData`、区段快照和不可变结果，不应持有 `ProgressionTransitionState` 的写权限。网络端不得把客户端收到的 Hardmode 位反向作为服务端资格输入；断线重连以 committed snapshot 和 section version 为准。

## 14. 当前 NLTX 映射

| currentPath | 已存在内容 | 能证明什么 | 缺口 | status |
| --- | --- | --- | --- | --- |
| `D:\TRbackup\NLTX\src\WorldSession\WorldSessionComponents.cs:40-48` | `WorldRulesState.HardMode`、`InfectionSpreadAllowed`、`SavedOreTiers` | 有世界规则、感染开关、矿阶状态容器 | 无 Hardmode request/plan/phase/commit/retry | `partial` |
| `D:\TRbackup\NLTX\src\WorldSession\WorldGeneration\WorldGenerationLifecycleState.cs:3-15` | Loading/Generating/Ready/Failed 生命周期 | 有生成门控和失败状态 | 未表达 Hardmode 长事务、后台预算和恢复点 | `partial` |
| `D:\TRbackup\NLTX\src\WorldSession\WorldGeneration\WorldEcologyScheduleState.cs:3-18` | 感染传播开关、世界更新率、生态预算 | 有生态调度局部状态 | 没有由 Hardmode 计划驱动的提交屏障 | `partial` |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\WorldGeneration\WorldGenerationPipeline.cs:39-70`、`:409-425` | 有阶段生成、逐阶段 Tile commit、Committed 阶段验证和 trace | 生成流程已具备显式阶段与验证 seam | `EnsureSupportedRules` 在 `:445-452` 明确拒绝 `IsHardmode`；不是 Hardmode executor | `partial` |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\WorldGeneration\WorldGenerationProgressionState.cs:3-25` | 有 `IsTransforming`、`ActiveTransformations`、`GenerationId`、`GenerationCursor` 和阶段 | 可作为生成状态的参考 | 尚未连接 Wall of Flesh、Hardmode plan、commit barrier | `partial` |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\World\Systems\WorldProgressionSystem.cs:12-46`、`:137-166` | 常规事件按时间、事件请求和状态推进 | 有 Query/Command 风格的事件推进 | 只处理血月、日食、灯笼夜、入侵、史莱姆雨；没有 Hardmode Tile 变换 | `partial` |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\WorldGeneration\HardmodeOreTierSelectionPolicy.cs:5-99` | Hardmode 矿阶选择规则、随机选择、Drunk World 切换和状态校验 | 矿阶纯策略已存在 | 没有把选择附加到 Hardmode `TransitionPlan` 或提交序列 | `partial` |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\WorldGeneration\LegacyWorldInfectionConversionCommitBoundary.cs:16-32`、`:47-74` | 感染命令排序、区段版本比较、Tile commit 和 deferred 结果 | 有局部感染批提交和失败分类 | 没有 Hardmode 级别的规则提交、网络投影和恢复日志 | `partial` |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Simulation\WorldGeneration\LegacyTileRunnerCommandCommitBoundary.cs:23-77` | Tile/Liquid 批命令的 validate/apply 与版本检查 | 有局部受控写入边界 | 名称中有 commit 不代表拥有全局 Hardmode commit barrier | `partial` |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Server\Replication\WorldSectionReplication.cs:99-113` | 从 WorldGrid snapshot 计算 changed sections 并编码帧 | 有区段投影能力 | 未接 TransitionResult、WorldData Hardmode 结果和一次性提交 ID | `partial` |
| `D:\TRbackup\NLTX\dome\src\Terraria.Dome.Server\Persistence\WorldSaveCoordinator.cs:12-55`、`:95-106` | 临时文件、写穿、备份轮换和受控恢复读取 | 有独立存档 Adapter | 未保存/恢复 Hardmode transition phase、plan、commit sequence | `partial` |
| `D:\TRbackup\NLTX\Test` | 当前仓库有多个组件验证项目，但未发现本子系统专属 Hardmode transition verifier | 说明有验证基础设施 | 无资格、部分失败、重试、幂等、恢复和网络一致性闭环证据 | `missing` |

当前结论不是“没有任何提交机制”：NLTX 已有生成/感染/Tile/Liquid 的局部提交机制；准确结论是“没有把这些能力编排成 Hardmode 的长事务执行器和跨域提交屏障”。

## 15. Entity ID、关系与跨子系统类型

本报告不创建或宣布共享基础类型最终 owner。下表全部为 `crossSubsystemOwner: integration-review`。

| typeId | proposed 表达 | candidate owner | consumers | writers | unresolved ownership |
| --- | --- | --- | --- | --- | --- |
| `WorldEntityId` | 世界会话根实体的稳定引用 | `WorldSession` / integration | transition、generation、storage、network | 世界启动/恢复 | 是否与现有 `WorldId`/`UniqueId` 一一对应 |
| `TransitionId` | 一次 Hardmode 事务的幂等 ID | `WorldProgressionAndTransition` candidate | recovery、persistence、network projection | ingress/planner | 是否持久化、跨重启是否稳定 |
| `PersistentWorldId` | 存档身份 | `WorldStorage` candidate | persistence、server discovery | world file metadata | `WorldId` 与 `UniqueId` 的协议角色不同 |
| `WorldSectionId` | 200×150 区段的稳定引用 | `WorldStorage` candidate | Tile commit、resync、visibility | world dimensions | 是否可直接复用现有 section coordinates |
| `TileCoordinate` | Tile 位置 | `WorldStorage`/`WorldInteraction` candidate | infection/ore/replication | mutation commit | 当前 src 与 dome 具有多个坐标类型，需整合 |
| `NetworkId` | 客户端会话/实体网络标识 | `ExternalBoundaries` candidate | resync adapter、protocol | network session | 不应渗透核心规则组件 |
| `WorldRevision` | 世界全局版本 | `WorldStorage` candidate | planner、barrier、persistence | commit barrier | 是否与 section version 建立单调关系 |

关系建模建议：过渡与世界是单向 `TransitionId -> WorldEntityId` 关系；计划与区段是多值 `TransitionPlan -> WorldSectionId[]` 关系；不要把每个 Tile 建成持续的 Transition Entity。只有当一个变更批需要独立生命周期、重试或审计时，才可把 batch 作为短生命周期命令记录，而非长期实体。

## 16. focused verifier 设计与本次验证结果

### 16.1 建议 verifier

| verifierId | 目标 | 必须覆盖的断言 | 本次状态 |
| --- | --- | --- | --- |
| `HardmodeEligibilityVerifier` | 资格与幂等 | 非服务端拒绝；已 Hardmode 非 force 幂等；加载/生成中拒绝；规则不支持时返回稳定错误 | `not-run` |
| `HardmodePlanDeterminismVerifier` | 计划可重放 | 固定世界快照、随机种子和游标得到相同 PlanId、矿阶选择、区域和命令顺序 | `not-run` |
| `HardmodePartialFailureVerifier` | 部分失败 | 第 N 批冲突/容量失败不会发布成功结果；失败原因可恢复；已提交批次不重复应用 | `not-run` |
| `HardmodeRetryIdempotencyVerifier` | 重试与幂等 | 相同 `TransitionId`/`CommitSequence` 重放不会二次感染、二次矿阶切换或二次公告 | `not-run` |
| `HardmodeRecoveryVerifier` | 崩溃/重启恢复 | 在执行中、提交前、规则提交后、投影前分别恢复，结果最终只能是明确 committed 或 recoverable failed | `not-run` |
| `HardmodePersistenceVerifier` | 存档一致性 | `Main.hardMode`、WorldFile metadata、矿阶、TransitionId 和区段版本恢复关系明确 | `not-run` |
| `HardmodeResyncVerifier` | 网络一致性 | 新加入/断线重连客户端收到同一 committed WorldData 与区段版本；重复结果被去重 | `not-run` |
| `HardmodeOrderingVerifier` | 调度顺序 | 资格 -> 计划 -> 执行 -> 屏障 -> 规则 -> 投影；禁止 Projection 反写 Simulation | `not-run` |

建议验证器放在对应 `WorldSession`/`WorldGeneration`/`WorldStorage` 的领域测试边界中，而不是新增通用 `Shared/Components/` 或按技术类别建立空目录。所有 proposed 类型落地前应先记录源路径、目标路径、依赖影响与回滚方式。

### 16.2 本次验证结果

本次只读审查没有运行 `dotnet restore`、`dotnet build`、`dotnet test`、`dotnet run` 或任何其他 compile-capable 命令，也没有运行 focused verifier。验证状态统一为 `verificationStatus: not-run`。现有代码和历史材料只用于 evidence mapping，不被写成行为等价、编译通过或迁移完成。

## 17. 明确不拆分项

| objectId | 不拆分理由 |
| --- | --- |
| `HardModeBoolean` | `Main.hardMode` 是过渡结果/兼容字段，不拥有计划、批处理、重试和投影生命周期；应由规则 Component 保存，由过渡 System 写入。 |
| `SingleInfectionTile` | 单个 Tile 是 `WorldStorage` 中的实例数据，转换规则属于策略/Pass，批量提交属于 System/Command。 |
| `SingleGenerationPass` | pass 是 `WorldGenerationAndEcology` 的策略或执行步骤；它不能独立拥有 Wall of Flesh 资格、规则提交、存档和重同步。 |
| `SingleNetworkMessage` | 消息号是 `ExternalBoundaries` 的协议 Adapter 输入/输出，不是权威状态或跨域事务。 |
| `BackgroundEffect` | 广播、成就、音效、粒子和 UI 是已提交结果的 Projection；它们不能成为 Hardmode 写入根。 |
| `SavedOreTierField` | 矿阶字段是长期状态的一列/值对象字段；只有连同选择规则、提交序列和恢复语义才构成过渡的一部分。 |
| `ResetSectionsCache` | 客户端区段有效性是网络会话缓存；它表达重同步策略，不拥有世界 Tile。 |
| `WorldProgressionTransition` | 当前 dome 类型只记录常规事件的 started/stopped 结果；不能因名字相似而承担 Hardmode 长事务。 |

## 18. 兼容策略与行为保持风险

### 18.1 兼容策略

- 初期保留现有 `Main.hardMode`、`WorldRulesState.HardMode` 和协议 bit 作为兼容投影，但禁止新增路径继续直接双写；最终唯一写入根需由整合会话确定。
- 保留 `WorldGen.StartHardmode` 的入口语义，把它改造成向 `RequestWorldProgressionTransitionCommand` 的适配入口，而不是让 NPC 直接调用多个新 System。
- 保留已有 Tile/Liquid commit boundary 的 expected section version 检查；在其上增加 Hardmode 层的 `PlanId`、batch sequence 和全局结果，而不是另造无版本的直接写入路径。
- 保留 `WorldFile.IOLock` 等旧式存档保护作为外层 Adapter 兼容机制；新逻辑不把锁对象渗透到纯 Query 或 Component。
- 网络兼容先生成与当前 WorldData/区段协议可表达的结果；新增 `TransitionId`、commit sequence 或恢复字段时必须版本化并由 `ExternalBoundaries` 统一裁决。

### 18.2 主要风险

1. **算法缺失风险**：Version4 `initializeHardMode` 和物品保护资格为空；若直接依据完整参考移植，会把版本差异、删减体和目标行为混在一起。
2. **早期状态风险**：`Main.hardMode` 在后台变换前置为 true，延后提交会改变生态、刷怪和其他读者；保留早期字段又需要显式区分“已宣布”和“已提交”。
3. **部分成功风险**：Tile、墙、矿阶、规则、存档和网络不是由 Version4 的单一原子事务包住；失败重试可能重复感染、矿阶切换、公告或区段发送。
4. **异常传播风险**：后台 Task 的 finally 会排队 follow-up，但没有显式成功标识；可能出现世界变换失败而客户端收到成功公告/区段失效。
5. **网络一致性风险**：`ResetSections` 只清缓存；`ResyncTiles` 的具体实现为空，不能证明所有客户端最终收到相同 Tile/规则快照。
6. **所有权风险**：WorldGenerationAndEcology 有 pass 与局部 commit 能力，WorldStorage 有 WorldGrid/section 版本能力，哪一方拥有 Hardmode Tile 提交仍未最终决定。
7. **状态重复风险**：`Main.hardMode` 与 `WorldFileData.IsHardMode` 具有不同读者；若无版本化协调，世界选择广播、运行时 WorldData 和存档恢复可能观察不同结果。
8. **目录风险**：落地时不能建立 `Shared/Components/`、`Common/` 或 `Misc/` 收容 proposed 类型；应先按 `World/Progression`、`WorldGeneration`、`WorldStorage`、`ExternalBoundaries` 的真实 owner 决定目录。

## 19. Evidence-gap 与 blocking-decision

### 19.1 普通 evidence-gap

| gapId | 缺口 | 影响 | 状态 |
| --- | --- | --- | --- |
| `GAP-V4-HardmodeBody` | Version4 `initializeHardMode` 空体；感染、墙、随机方向、矿阶/生态 pass 的实际目标版本实现缺失 | 无法从 Version4 单独证明 Tile 变更集合与顺序 | `evidence-gap`, `partial` |
| `GAP-V4-ProtectionEligibility` | Version4 `EligibleForSpawnProtection` 空体 | 无法证明哪些物品被保护、边界和恢复条件 | `evidence-gap`, `partial` |
| `GAP-V4-TaskResult` | 后台 transform 没有显式成功/失败结果，异常传播未由源码闭合 | 无法证明 follow-up 的失败语义 | `evidence-gap`, `unresolved` |
| `GAP-V4-ResyncBody` | `NetMessage.ResyncTiles` 私有实现为空 | 无法确认区域重同步的实际发送集合 | `evidence-gap`, `evidence-mismatch` |
| `GAP-NLTX-TransitionVerifier` | 当前 Test 与 dome 验证材料没有 Hardmode 长事务专项 verifier | 无法把模型、代码存在或历史输出升级为 confirmed | `evidence-gap`, `missing` |
| `GAP-WorldDataReceive` | 本次证据未闭合 Version4 客户端接收 WorldData 后对 Hardmode/区段的完整应用顺序 | 客户端最终一致性只能提出 Adapter 契约 | `evidence-gap`, `partial` |

### 19.2 Blocking-decision

以下事项会改变权威状态、事务边界或必须保持的 System 顺序，必须交给最终整合会话或用户裁决：

| blockingId | 决策问题 | 可选解释及影响 |
| --- | --- | --- |
| `BD-RuleCommitTiming` | Hardmode 规则何时成为唯一权威结果？ | `PreserveEarlyRule` 保留 Version4 时序但增加 phase；`CommitAfterMutation` 简化一致性但改变观察时序；`DualPhaseProjection` 兼顾两者但增加兼容复杂度。 |
| `BD-TileCommitOwner` | WorldGenerationAndEcology 还是 WorldStorage 拥有 Hardmode Tile 的最终提交？ | 由生成域拥有可保持 pass 内聚；由存储域拥有可统一版本/写入/回滚；中间方案是生成只出 Command、存储独占 apply。第三种最符合副作用隔离，但需要 integration-review 批准。 |
| `BD-ResyncScope` | Hardmode 提交后是全量区段失效，还是只投影变化区段？ | 全量更接近 `ResetSections` 的保守兼容行为但开销大；变化区段更高效但依赖完整 mutation footprint、section version 和重连快照证据。 |
| `BD-OreTierAtomicity` | 矿阶选择是否和感染/墙体 Tile 处于同一提交序列？ | 同一序列便于重试/存档一致性但跨域事务更重；分开提交会降低阻塞但允许规则与 Tile 暂时不一致。 |
| `BD-MetadataOwner` | `Main.hardMode` 与 `WorldFileData.IsHardMode` 是否保留双表示？ | 保留需要明确主字段与投影字段、版本及恢复优先级；合并表示可能改变公开元数据和旧协议行为。 |

## 20. Integration Handoff

```text
subsystemId: WorldProgressionAndTransition
taskNumber: 06
reportPath: D:\TRbackup\NLTX\docs\research\2026-09-05-version4-world-progression-and-transition-public-decomposition.md

evidenceStatus:
- confirmed: Version4 Wall of Flesh -> StartHardmode -> background transform -> follow-up -> section reset control skeleton
- partial: Hardmode mutation algorithm, item protection eligibility, persistence dual-state reconciliation, client application
- evidence-mismatch: Version4 initializeHardMode and ResyncTiles private body are empty in the inspected snapshot
- version-drift: complete reference adds Hardmode implementation and server/client guards at different line offsets

nltxStatus: partial
verificationStatus: not-run

confirmedOwners:
- Version4 WorldGen.StartHardmode is the direct Hardmode transition entry and writes Main.hardMode before background work
- Version4 WorldFile owns IOLock, waits for transforming world, and serializes Main.hardMode in world header
- Version4 Netplay.ResetSections owns section-cache invalidation; NetMessage.SendSection owns section packet assembly

proposedTypes:
- proposed Component: ProgressionTransitionState
- proposed Component/value object: TransitionPlan and TransitionRecoveryState
- proposed Query: EligibilityQuery
- proposed System: TransitionRequestIngressSystem
- proposed System: TransitionPlanningSystem
- proposed System: HardmodeMutationSystem
- proposed System: CommitBarrierSystem
- proposed System: HardmodeRuleCommitSystem
- proposed System: TransitionRecoverySystem
- proposed Command: RequestWorldProgressionTransitionCommand
- proposed Command: GenerateHardmodeMutationBatchCommand
- proposed Command: ApplyHardmodeTileBatchCommand
- proposed Adapter: ResyncAdapter
- proposed Adapter: WorldProgressionPersistenceAdapter
- proposed Projection: TransitionResultProjection

sharedTypesForIntegrationReview:
- WorldEntityId
- TransitionId
- PersistentWorldId
- WorldSectionId
- TileCoordinate
- NetworkId
- WorldRevision
- immutable WorldTransitionResult / WorldProgressionSnapshot

crossSubsystemReaders:
- WorldGenerationAndEcology reads Hardmode rule, infection gate, ore-tier selection and lifecycle gate
- WorldStorage reads mutation commands and expected section versions
- ExternalBoundaries reads committed result for persistence, WorldData and section resync
- NpcAndTownSimulation reads Hardmode outcome for spawn/loot and supplies the Wall of Flesh trigger

crossSubsystemWriters:
- NpcAndTownSimulation may issue the trigger command only
- WorldGenerationAndEcology may produce pass commands and ore/infection proposals
- WorldStorage applies Tile/Liquid batches and section versions
- ExternalBoundaries may load a snapshot during startup, but must not directly write active simulation state

orderingConstraints:
- eligibility before plan
- plan before mutation batch
- expected-version validation before Tile/Liquid apply
- Tile/Liquid and ore-tier commit before immutable TransitionResult projection
- Hardmode rule commit before WorldData/section resync projection
- failure or unknown result before retry; no success projection on unverified failure

boundaryChallenges:
- Current NLTX has local infection/Tile commit boundaries but no Hardmode-wide commit barrier
- Version4 sets Main.hardMode before asynchronous world mutation; final timing must be integration-reviewed
- WorldGenerationAndEcology pass ownership and WorldStorage Tile commit ownership remain separate decisions
- Full reset versus changed-section resync is unresolved because Version4 ResyncTiles body is empty

evidenceGaps:
- Version4 initializeHardMode body absent
- Version4 EligibleForSpawnProtection body absent
- Version4 private ResyncTiles body absent
- background exception/follow-up result semantics not explicit
- current NLTX has no focused Hardmode eligibility/partial-failure/retry/recovery/idempotency/network verifier

blockingDecisions:
- BD-RuleCommitTiming
- BD-TileCommitOwner
- BD-ResyncScope
- BD-OreTierAtomicity
- BD-MetadataOwner

notImplemented:
- no actual ProgressionTransitionState, TransitionPlan, EligibilityQuery, Hardmode executor, CommitBarrier, ResyncAdapter or TransitionResultProjection was created
- no production C#, test, project, generated output or migration file was modified

verifierPlan:
- HardmodeEligibilityVerifier
- HardmodePlanDeterminismVerifier
- HardmodePartialFailureVerifier
- HardmodeRetryIdempotencyVerifier
- HardmodeRecoveryVerifier
- HardmodePersistenceVerifier
- HardmodeResyncVerifier
- HardmodeOrderingVerifier
```

## 21. 最终声明

本报告是基于 Version4、完整参考源码、tModLoader 公开文档和有限 ECS 结构参考形成的子系统边界与 ECS 拆分设计。它不是迁移完成报告，不是行为等价证明，不是 API 兼容证明，也不是当前 NLTX 已实现能力的声明。

本次只读审查未运行构建或测试，验证状态为 `not-run`；已记录 Version4 空实现、版本差异、网络重同步缺口和跨子系统 owner 未决项。未修改 `src\`、`Test\` 或 `dome\src\` 生产代码。
