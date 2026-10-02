# P16 世界生命周期、住房与指标 System 设计

~~~yaml
documentType: system-design
partitionId: P16
taskId: AUTH-SYS-P16
originalSessionId: b65120e249874094ac5069b99b2fb9c7
inputReport: D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P16-world-lifecycle-housing-metrics.md
outputReport: D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P16-world-lifecycle-housing-metrics.md
inputMembers: 133/133
sourceRoot: D:\TRbackup\Version4
referenceRoot: D:\TRbackup\无任何删减通过编译
comparisonReferenceRoot: C:\Users\shan\Downloads\ECS\space-station-14-master
targetRoot: D:\TRbackup\NLTX\src\NSSLC
designStatus: proposed
implementationStatus: partial
executionStatus: partial
verificationStatus: not-run
historicalVerificationStatus: partial
migrationStatus: not-claimed
deliveryMode: proposed-static-report
documentRevisionReviewedAt: 2026-10-01
historicalSourceModified: true
historicalTestsRun: true
thisTurnSourceModified: true
thisTurnBuild: passed
thisTurnTest: passed
thisTurnVerification: partial
~~~

范围：authoritative P16 的 13 个叶子组、130 个字段和 3 个属性；不扩展分区成员范围。关联执行计划：[P16 System 执行计划](2026-09-30-p16-world-lifecycle-housing-metrics-execution-plan.md)。

上游 claim：`AUTH-SYS-P16`；输入成员 `133/133`；`sessionId` 使用原值 `b65120e249874094ac5069b99b2fb9c7`；`outputReport` 为上述 P16 System 拆分报告。runner `completed` 只表示该报告任务已结算。

本文件是 proposed 静态设计，不是实现完成、行为等价证明或迁移成功证明。这里记录此前住房评分候选代码及其 focused verifier 结果；局部结果不提升 P16 全量验证状态。

元数据中的 `verificationStatus: not-run` 指 P16 必需行为矩阵尚未验证；`thisTurnVerification: partial` 只记录住房评分候选切片已有的 focused verifier，不代表 P16 全量验收。

## 1. 设计结论

当前状态覆盖：设计 proposed；P16 全量验证 not-run；load recovery、housing validation、housing registry 各有 3 项局部核心验证通过，housing score 有 5 项 focused verifier 场景通过（2026-10-01）。

局部验证仅支持各自候选状态模型、显式输入规则或 codec。住房评分 5/5 通过不证明真实 Tile 映射、occupancy、NPC assignment、feedback 闭包或旧入口接入；P16 设计仍为 proposed，全量验证仍为 not-run。

P16 不形成一个包揽 WorldGen 状态的 WorldGenerationSystem。候选边界按不变量和提交责任拆为世界会话生命周期、住房验证、Town 住房登记、分列环境指标和 TreeTops 样式状态。尺寸派生、住房规则和只读世界派生值由 Query 角色表达；跨域的进度、天气、Tile mutation、网络和存档工作保留在各自真实 owner，最终归属交 integration-review。

System 名称和 API 是候选设计，不代表当前已实现、注册、接入旧入口或行为等价。P16 runner 的 completed 只表示分区报告已结算。后续迁移仍须通过目标行为验收。

## 2. 范围与证据

P16 输入清单是本设计的唯一成员范围来源。13 个输入组是库存分组，不直接等同于 13 个 Component 或 System。字段和属性明细保留在权威输入 ledger 与 P16 报告中。

| 证据源 | 身份与范围 | 本设计可支持的判断 |
|---|---|---|
| Version4 目标源码 | D:\TRbackup\Version4；本轮对关键源码计算逐文件 SHA-256 | 当前被检查文件的源码形态及局部调用关系；没有 Git commit 或统一源码快照 ID |
| CPG 查询 | 只读数据库 D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite；manifest SHA-256 为 6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364；project fingerprint 为 521CBD4C11E7EB731256C232350A54099F431D0E59EBDF3C38880281F169C11B；967 shards，import complete | 选定索引路径内的符号、直接调用点和成员使用；不证明动态分派、运行时入口、唯一写入者或副作用闭包 |
| 完整参考源码 | D:\TRbackup\无任何删减通过编译；按相关文件计算 SHA-256 | 作为候选行为和调用结构的对照材料；不是 Version4 的同一源码快照，也不自动决定迁移目标行为 |
| SS14 ECS 结构参考 | C:\Users\shan\Downloads\ECS\space-station-14-master；读取 `Content.Server/Animals/Systems/EggLayerSystem.cs` | 仅参考 System 聚合领域行为、显式依赖和效果边界；不把 RobustToolbox API、EntitySystem 调度或组件注册约定移植为 NSSLC 运行时要求 |
| 当前 NSSLC | D:\TRbackup\NLTX\src\NSSLC\Component\WorldSession\WorldGeneration | 已有数据类型和局部实现的形状；类型存在不证明 loader、主循环、存档或网络路径已接入 |

### CPG 查询记录

查询使用仓库只读入口 .agents/skills/ecs-system/tools/CpgEvidence.ps1，先初始化上述 SQLite 数据库并启动只读 reader。WorldGen 的索引 shard 约 9.3 GB；查询走 SQLite index，ScannedShardCount 为 0。complete 表示选定路径查询完成，不表示全程序闭包完成。

| API | 选定范围和结果 | 保留的限制 |
|---|---|---|
| Find-CpgSymbols、Get-CpgTypeSurface | WorldGen 方法符号、TownRoomManager 与 TreeTopsInfo 直接成员面均 complete；TownRoomManager 17 项、TreeTopsInfo 9 项 | 类型面只列直接声明成员 |
| Find-CpgCallSites | StartRoomCheck 6 处、QuickFindHome 3 处、UpdateWorld 2 处、CountTiles 1 处、WorldFile.LoadWorld 4 处、WorldGen.serverLoadWorld 在 Main 1 处、StartHardmode 1 处、TransformWorldOnBackgroundThread 2 处；WorldFile 的 SaveTownManager / LoadTownManager 各 1 处；所选路径查询 complete、无 gap | 范围是 Terraria/WorldGen.cs、Terraria/Main.cs、Terraria/NPC.cs、Terraria.IO/WorldFile.cs 等显式选择的 shard；callback 委托边单独保留 partial |
| Skyblock CPG 面与直接调用 | `Get-CpgTypeSurface(Skyblock)` 在 `Terraria/WorldGen.cs` 返回 18 个直接成员；`Calculate()` 的选定 shard 直接调用点为 2 处，来自 `Skyblock.ScanTiles()` 与 `WorldGen.CountTiles()`；`hasWall` 是 `bool[]`，该 shard 的 5 条 member-use 查询 complete，但事实状态为 partial、`AccessMode=Unknown` | 只闭合所选 WorldGen shard 的已索引静态边；不证明方法效果、跨 shard 使用闭包或 runtime caller |
| TreeTops API 调用点 | Save -> WorldFile 1 处；Load -> WorldFile 1 处；SyncSend -> NetMessage 1 处；均 complete、无 gap | SyncReceive 在 Version4 目标源码中没有方法符号；完整参考树有额外接收逻辑 |
| Get-CpgMemberUses | TownManager 在 6 个所选 shard 有 18 条，TreeTops 有 5 条；查询覆盖 complete | 使用事实为 partial，AccessMode 为 Unknown 或没有赋值证据，alias/callee effects 未展开 |
| Get-CpgCallableFacts | Version4 initializeHardMode 返回 3 个 CFG 节点：entry、return、exit；无直接调用目标；状态 partial | gap 为 CalleeEffectsNotExpanded；结论需与当前目标源码全文对应，不能扩展为运行时行为证明 |

**住房评分调用边核对（2026-10-01）。** 对 Version4 CPG 使用 `Find-CpgSymbols` 定位 `Terraria/WorldGen.cs` 的 `ScoreRoom(int,int,IRoomCheckFeedback)`，再用 `Find-CpgCallSites` 查询 `Terraria/WorldGen.cs`、`Terraria/NPC.cs`、`Terraria/Main.cs`，返回 2 个 confirmed 静态调用点，均在 `WorldGen.cs`：`SpawnTownNPC` 调用 `ScoreRoom(-1, num)`，`QuickFindHome` 调用 `ScoreRoom(npc, Main.npc[npc].type)`。所选查询为 complete、无 gap；manifest 的 `SourceSnapshotId` 为空，因此结论同时按 Version4 源码回读确认，不能推广为动态调用闭包。
再使用 `Get-CpgEvidenceHealth` 提供的全部 967 个 `SourcePath` 重查，`ScoreRoom` 仍为 2 个调用点，`StartRoomCheck` 为 6 个调用点（4 个位于 `WorldGen.cs`，2 个位于 `Terraria.IO/WorldFile.cs`）；两项查询均 complete、无 gap。此处是索引内静态关系，不证明动态调用或行为闭包。

完整参考树 `D:\TRbackup\无任何删减通过编译\Terraria\WorldGen.cs` 有 3 个 `ScoreRoom` 调用点，额外的 `MoveTownNPC` 会传入 `IRoomCheckFeedback` 并读取 `roomOccupied`、`roomEvil`、`roomHasStandingSpace`；这些字段及反馈分支在 Version4 目标 `WorldGen.cs` 中不存在。完整参考树仅用于识别版本差异，不把该第三入口或状态反馈作为 Version4 的兼容合同。

**TreeTops 全索引补查（2026-10-01）。** 使用 `Get-CpgEvidenceHealth` 返回的全部 967 个索引路径查询直接调用点：Save -> WorldFile 1 处、Load -> WorldFile 1 处、SyncSend -> NetMessage 1 处、GetTreeStyle -> WorldGen 1 处、RandomizeTreeStyleBasedOnWorldPosition -> Projectile 1 处、CopyExistingWorldInfoForWorldGeneration -> WorldGen 1 处；RandomizeTreeStyle 的唯一静态调用点在 TreeTopsInfo 内。各调用点查询为 `complete`、无 gap。`_variations` 返回 21 个成员使用，均定位于 TreeTopsInfo.cs，但每项证据仍为 `partial`，AccessMode 为 `Unknown`，不作为写入闭包或外部无调用的证明。全索引结果仍未绑定 SourceSnapshotId，也不覆盖反射、动态调用或运行时注册。

**CountTiles 与 alignment 统计补查（2026-10-01）。** 使用相同 manifest 的只读 CPG 查询确认 `CountTiles` 在已选 WorldGen/Main/NPC shard 中有 1 个直接静态调用点，位于 `Terraria/WorldGen.cs` 的 `UpdateWorld` cadence；调用点查询为 `complete`、无 gap。`tileCounts` 的声明类型为 `int[]`；`totalGood2`、`totalEvil2`、`totalBlood2`、`totalSolid2` 的符号查询为 `complete`，但 `CountTiles` 与 `AddUpAlignmentCounts` 的 callable facts 均为 `partial`，gap 是 `CalleeEffectsNotExpanded`。`tileCounts` 使用结果的查询覆盖 complete，但访问方向证据为 partial/Unknown；不得据此宣称字段闭包或单写者已证明。CPG 的 `SourceSnapshotId` 为空，结论由目标源码逐段回读补足。

CPG manifest 没有 SourceSnapshotId 或逐文件源码 hash。源码行号只用于定位，不把 CPG 索引自动绑定到当前文件版本。

**宿主与 Tile adapter 边界复核（2026-10-01）。** 本轮用同一只读 CPG API 限定 `Terraria/Main.cs` 查询 `WorldGen.UpdateWorld`，返回 2 个 confirmed `CallTargets`，状态 complete、无 gap；限定 `Terraria/WorldGen.cs` 查询 `CountTiles`，返回 1 个 confirmed 直接调用点，状态 complete、无 gap。两项都是 selected-source-paths 的静态索引事实，不证明 runtime scheduler、线程或 world-session 绑定。NSSLC 的 `WorldGenerationLifecycleComponent` 只携带 `GenerationId`，`WorldSessionComponents` 没有 session identity；`PersistentWorldId` 表达持久化 world 身份，不能据此认定为一次运行 session 的身份。现有 `WorldStorage.TileMapStore` 仅公开 layout、宽高和 mutation revision，没有按坐标读取/创建 Tile 的 API；`TileCellState` 没有显式 active 字段。`WorldInteraction.Tiles.TileCellComponent` 有 `TileType`、`WallType`、`IsActive`，但当前项目中未见 world-coordinate lookup/create port。已有 `IWorldSkyblockGenerationGridReader.Read` 只声明 observation 读取，NSSLC 中没有实现，且不承诺缺失 Tile 时创建，因此不能直接复用于 CountTiles。新增 `IWorldTileMetricsTileSource.ReadOrCreateTile` 明确了局部候选 API 的输入合同，但真实地图 adapter、session reset 和 Main 接线仍为 `unknown`；不能从 header 字段名猜 active 位语义。

**Transform/save barrier 补查（2026-10-01）。** CPG 在选定 `WorldGen.cs`/`WorldFile.cs` paths 中查询 `_transformingWorld` 得到 2 条 member-use、查询 `IOLock` 得到 4 条 member-use，均 complete、无 gap；两者 `AccessMode` 仍为 `Unknown`，不能单凭图事实分类。Version4 与完整参考源码回读确认 transform 入口在 `Task.Factory.StartNew` 前增加静态计数，worker 在 `finally` 中减少计数并排入可选 main-thread follow-up，地形转换在 `WorldFile.IOLock` 内执行；保存入口先等计数归零再获取 I/O lock。新增 `WorldTransformTransactionSystem` 只提供并发安全的 begin/complete 计数和 active 查询；它不拥有 Task 调度、IOLock、save wait 或 main-thread queue。静态计数的进程级 scope 已可见，但 component 的唯一注册点、多个 host 的共享关系和 unload/reset 语义仍为 `unknown`。

**读档调用链补查（2026-10-01）。** 同一只读 CPG 数据库中，`WorldFile.LoadWorld` 在所选 `Terraria/WorldGen.cs` shard 有 4 个直接调用点（complete、无 gap）；`WorldGen.serverLoadWorld` 在 `Terraria/Main.cs` 有 1 个直接调用点（complete、无 gap）。`serverLoadWorldCallBack` 的方法符号存在，但针对所选 WorldGen/Main/WorldFile paths 的 `Find-CpgCallSites` 为 `partial`，0 项并带 `NoMatchingFactInScannedScope` gap。源码明确显示 `Task.Factory.StartNew(serverLoadWorldCallBack)` 方法组传递；CPG 未闭合该 delegate 边，不能把 0 项写成没有调用。`_transformingWorld` 在所选 paths 的 2 条 member-use 均位于 WorldGen 且 `AccessMode=Unknown`；`IOLock` 有 4 条 member-use，事实为 `partial`、方向 `Unknown`。

### 参考源码与目标差异

目标源码与完整参考源码不是可互换的版本基线。重点文件 SHA-256 如下；WorldFile.cs 在两棵源码树中 hash 相同，其余列出的关键文件 hash 不同。

| 文件 | 完整参考源码 | Version4 目标源码 |
|---|---|---|
| Terraria/WorldGen.cs | B9F7834CE1BC68C1DD9C656574A2272DB6F79E1407D934E1ADA33EDC3C930F82 | A06A8463E39EA065441FF1EF702A787D3450ADAE5CD3065CF30BF76784D3EA1D |
| Terraria/Main.cs | E24E61C9903BB7995F47E36C51EDBE43481B643226861B717020877E7E22B63F | 66DBE1D1E6FB89A24A512309BB039DFEC7C06D4678D2A22D7ADECBAD3AFD6520 |
| Terraria/NPC.cs | ED8AA2302730A9E046EF310E543204FBA391BD35B1C9C0B213C8065DFBBE39F0 | 29869B0390D85CB0E627552ECBED929D90508C8EF0DA55D9660133F91DA42D17 |
| Terraria/NetMessage.cs | F3066C50715D7C49B8BF2CC852B015303AD7F4C12ADC191C72FC0FE46181E7E2 | 87B596BD8467B9C470DD1F2AD6963EBADE257689F87395163AB136C149BEBC9A |
| Terraria.IO/WorldFile.cs | 92DF79BB7AE89393327AE9EF6B7B78762909E5B2E197C0F3FCCFE709E152F289 | 92DF79BB7AE89393327AE9EF6B7B78762909E5B2E197C0F3FCCFE709E152F289 |
| Terraria.GameContent/TownRoomManager.cs | CD92AB3DD3CB19CC490876DA7BFFD732C8D887B27627B797DD4BC24864366499 | 8DE25BA7FA27A70E72CAB408F0740580D85CEFD6EE5D50CB1AA2F0ED7C097971 |
| Terraria.GameContent/TreeTopsInfo.cs | 5896E31E0E940F1A8F340C3FCF17014D6D6DB873FB4E979A8143AEF9228569B6 | 7E8C57F4BBD7590C39CDA7D9258343FE69B23B655A9F26EAF6D6A34969A248F5 |

最显著的行为缺口是 hardmode：完整参考源码的 WorldGen.initializeHardMode 有地形转换逻辑；Version4 目标源码的同名函数体为空。Version4 仍保留 NPC 到 StartHardmode、StartHardmode 到后台 transform 的结构，并存在 transforming 计数、IOLock 和 main-thread follow-up。参考源码的 StartHardmode 还检查 netMode；Version4 目标源码此 guard 形态不同。因而只能把目标源码中的工作线程/同步屏障视为可见结构，实际 hardmode 转换行为和目标兼容合同标为 unknown。不得从参考源码复制行为后宣称 Version4 行为已恢复。

读档路径也存在需保留的版本差异。两棵树的 `Terraria.IO/WorldFile.cs` SHA-256 相同：云存档不可用时设置 `loadFailed` 并早退；`loadFailed=false` 位于成功打开输入后的读取 try 内；加载阶段先置 `isGeneratingOrLoadingWorld=true`，执行液体 settle 与 `WorldGen.WaterCheck()` 后才清除；后续 catch 设置 `loadFailed=true`，catch 本身没有显式清除 gate。两边 `WorldGen.serverLoadWorldCallBack` 都先读档、失败后重试一次、检查并恢复 `.bak` 后再最多读两次，最终成功才播放声音、恢复临时世界时间并发出 `Hooks.WorldLoaded()`。完整参考树另在非 dedicated-server 失败路径设置 `Main.menuMode` 为 200 或 201 后返回；Version4 目标没有这段 UI 路由。候选实现以 Version4 作为兼容基线，不自动移入参考树菜单分支。gate 在 settle 异常后的保留状态、重试时旧 world 状态、取消及 unload 清理仍为 `unknown`。

TownRoomManager 与 TreeTopsInfo 两个参考文件也与 Version4 hash 不同。参考项目可协助识别数据格式、调用关系和潜在规则；具体序列化、错误、默认值和网络协议仍必须以目标 Version4 源码和行为验收为准。本轮未构建任一源码树；“通过编译”来自目录名称，不是本轮验证结果。

TreeTops 的目标差异已直接回读：Version4 的 `Save` 写入数量和 13 个 `Int32`，`Load` 对 version <211 调用空的 `CopyExistingWorldInfo`，对新版本读取计数并至多更新 13 项；`SyncSend` 写出 13 个 byte。完整参考树另外包含 `SyncReceive`、`DoTreeFX` 和实际填充旧世界背景的 `CopyExistingWorldInfo`，这些逻辑不自动纳入目标行为。

Version4 `CountTiles` 与完整参考树的列计数及 `AddUpAlignmentCounts` 主体相同：从 y=40 扫描到 `worldSurface + 1` 的首区间使用权重 5，随后扫描到 `maxTilesY - 40` 使用权重 1；null Tile 会先写入新 Tile，逐格登记 wall，active Tile 再登记 tile 并累计连续类型 run；每列聚合 alignment 后清空 `tileCounts`，末列才调用 `Skyblock.Calculate()`。这些是目标 Version4 可观察源码行为，迁移不能把 Tile 初始化、逐格 flags 或顺序折叠为只读 Query。

调用 cadence 存在版本差异。两边 `WorldGen.UpdateWorld` 都在 `totalD >= 30` 时扫描一列并 wrap `totalX`，但完整参考树在 cadence 外围有 `Main.netMode != 1` 条件，Version4 目标没有该条件。P16 目标仍以 Version4 为基线，不得从完整参考树把该 guard 移入候选实现；两个 `UpdateWorld` 静态入口的运行时调度与线程条件仍须在 integration review 复核。

X==0 时，Version4 先发布前一窗口的累计值与百分比，处理非零 alignment 被四舍五入到 0 的最小值，再无条件调用 `NetMessage.SendData(57)`，然后清零窗口累计，接着扫描当前列；完整参考树只在 `Main.netMode == 2` 时调用该发送。两个 `NetMessage` writer 都将 57 编码为 `tGood`、`tEvil`、`tBlood` 三个 byte；完整参考树 `MessageBuffer` 在客户端读入这三个值，Version4 对应 `MessageBuffer` 分支为空。目标接收行为和端到端协议闭环因此是 `unknown`，不能把参考树接收器视为 Version4 目标事实。适配顺序按 Version4 源码保留发送调用与累计清零的相对次序，不增加旧实现没有的“发送成功”返回语义。

`Skyblock.Calculate()` 不是只读 observer：它读取 `hasTile`/`hasWall` 汇总，更新 `noDungeon` 等 flags，可能将 `Main.dungeonX/Y` 写成 -1，计算并清零 `currentActiveTiles`，清空两组 flags，并在 `lowTiles` 改变时发送 message 7。完整参考源码还保留一个独立的 `Skyblock.ScanTiles()` 全图扫描入口（扫描 x/y 排除边界后的世界并调用 `Calculate()`）；它不是每列 CountTiles 算法，不得合并或假设可替代。CPG 在所选 `WorldGen.cs` shard 确认 `Calculate()` 有上述两个直接调用点，但效果闭包及 `ScanTiles()` 的完整 runtime 入口仍为 `unknown`。候选 `WorldTileMetricsSystem` 只可返回有序 effects/request；Skyblock flags、Main 写入与消息由 integration review 确认的真实 owner 承接。

Version4 的 Projectile 位置随机化分支在调用 `TreeTopsInfo.RandomizeTreeStyleBasedOnWorldPosition` 后还会无条件调用 `NetMessage.SendData(7)`；TreeTopsInfo 内部则在样式实际变化时也调用一次该发送。迁移组合须分别保留内部条件发送和 Projectile 外层发送的当前可观察次数，除非后续批准行为变更。CPG 全索引直接调用点已定位，但 RNG 顺序、动态入口、FX/接收端与运行时 owner 仍为 `unknown`。

## 3. 候选 System 边界

| 候选能力 | 责任与权威状态 | 不负责 | 结论及证据状态 |
|---|---|---|---|
| WorldSessionLifecycleSystem | 推进一个 world session 的 Loading、Ready、Failed、Cleared 状态；维持旧 load gate 的可观察时点 | 不解析存档格式、不拥有 Tile/住房/指标业务规则或 transform worker | separate / proposed；最终 session key、失败复位、取消及 unload 为 unknown，owner 交 integration-review |
| WorldTransformTransactionSystem | 维护可并行 transform 的活动数与状态 revision，供 worker 完成路径和 save barrier 查询 | 不调度 worker、不取得 `WorldFile.IOLock`、不排队 main-thread follow-up 或自行 reset | separate / proposed；begin/complete 与 active 查询局部实现、2 项核心 verifier passed；真实全局注册、Task/lock/save/queue 接线及 unload 语义仍 unknown |
| HousingValidationSystem | 对一次房间搜索执行扫描、规则评估和候选评分，返回不可变评估结果；正式 NPC 住房变化经授权提交点 | 不拥有常驻居民登记，不把搜索称为纯 Query | separate / proposed；QuickFindHome 与 StartRoomCheck 有明确入口，feedback 动态目标和异常恢复仍 partial |
| HousingRoomScoreSystem | 在已完成房间评估上计算善恶平衡、候选位置、家具/箱子扣分和 shared-room 规则；返回不可变评分快照 | 不读取真实 `Main.tile`、不决定 NPC occupancy/home 提交、不触发 feedback 或网络效果 | separate / proposed；`ScoreRoom` 的显式分类 Tile source 候选已存在但未接入旧入口，真实 Tile 分类、occupancy、特殊 NPC 条件和 tie-break 仍 unknown |
| TownHousingRegistrySystem | 维护 session-scoped resident-key 到 room 的登记、无家状态和快速索引 | 不负责 Tile 房间合法性，不把旧 NPC type key 自动替换成实体 ID | separate / proposed；已实现 NPC type key 的局部提交/查询/清理，权威 session 范围、NPC 双写 owner 和实体 key 迁移仍 unknown |
| WorldTileMetricsSystem | 推进候选 component 内 30 tick 列游标；通过 `IWorldTileMetricsTileSource.ReadOrCreateTile` 遍历 Version4 单列区间、执行 alignment 累计和 scratch 清理；扫描/聚合后返回末列效果请求 | 不直接读取或创建 `Main.tile`，不拥有 Skyblock 状态与 `Calculate()` 副作用、WorldGen.UpdateWorld 调用/调度或 NetMessage 发送 | separate / proposed；cadence、alignment/publication API 与 `ScanColumnTiles` 算法有局部实现，5 项核心 verifier passed；真实 Tile/effect adapters、Main 接线、消息发送、Skyblock owner 调用及 session 注册尚未接入 |
| WorldTreeTopsSystem | 拥有 13-area 样式状态的读写、区域选择和受控随机选择 | 不拥有背景渲染缓存、所有 biome 背景字段、wire format 或消息发送 | separate / proposed；AreaQuery/System、持久化和网络 payload projection 已有局部实现，7 项核心 verifier passed；动态入口、随机流次序、FX/接收端与 owner 接线未闭合 |
| World size / derived queries | 从已提交 world profile 得到尺寸目录、oceanLevel 等不可变派生值；尺寸改变走明确提交角色 | 不接管 world allocation 和地图持久化 owner | Query + explicit mutation role / proposed；SetWorldSize 调用和 allocation 时点仍 partial |
| 进度、天气、Tile mutation 与 scratch | 交由其真实 Progression、Weather、Tile、Item、Generation pass 等能力 owner | 不新增 P16 总括 Environment System | partial / keep；最终 owner、读写闭包均交 integration-review |
| World update host | 保留现有 Main → WorldGen.UpdateWorld 调用位置，由原 composition 局部委托已确认能力 | 不在 P16 新建平行 scheduler 或重新排序整个 world tick | partial；静态调用点不定义 scheduler、线程或随机数合同 |

Command 和 Query 是职责角色，不要求一项能力对应一个新类型。住房搜索带 Tile 读取和反馈副作用；只有从完整效果检查确认无写入的只读投影，才可设计成 Query。genRand 是共享可变随机源，不能作为可任意重排的 Query。

## 4. 状态与组件归属

| 状态族 | 候选 owner | 目标状态形状 | 生命周期与缺口 |
|---|---|---|---|
| load/clear/failure/transform 标记 | WorldSessionLifecycleSystem | 现有 WorldGenerationLifecycleComponent、WorldLoadLifecycleComponent 作为候选输入 | `WorldLoadLifecycleSystem` 已实现目标源码可确认的主档两次尝试、`.bak` 检查/恢复后两次尝试和 gate 时点；generationId 不等同已确认的 world session identity，宿主接线与取消/异常清理仍 unknown |
| background transform active count | WorldTransformTransactionSystem | WorldTransformTransactionComponent | Version4 与完整参考树的计数为静态进程级；component 注册共享范围、save barrier 和 reset/unload 仍 unknown |
| 搜索坐标、tile mask、评分暂存 | HousingValidationSystem / HousingRoomScoreSystem 的单次调用上下文 | 仅在确需跨 tick 时才使用 Component | 默认 call-scoped；评分使用分类后的显式 Tile sample，真实映射、重入、异常清理和回调重入需行为验证 |
| 居民 room map 与 homeless key | TownHousingRegistrySystem | 现有 TownHousingRegistryComponent、TownHousingRegistryPersistenceAdapter | 目标 Version4 序列化布局已落地并以 NPC type key 保存；registry 与 NPC home 的单一权威双写 owner、越界/坏数据兼容和实体 key 方案仍 unknown |
| tile alignment 累计与已发布指标 | WorldTileMetricsSystem | WorldTileMetricsComponent 与不可变 WorldTileMetricsSnapshot | 组件不复用 WorldLayerMetricsComponent；实例应由后续 world-session composition 持有，但 identity、注册和 unload 清理仍 unknown；Tile scratch 仍作为旧扫描输入 |
| TreeTops 13-area variation | WorldTreeTopsSystem | Terrain/TreeTops/WorldTreeTopsStateComponent 与 snapshot | 已有 13 项状态、区域 Query、随机变更和 projection；不含 Version4 未实现的旧版复制、WorldFile/NetMessage 接线、接收端或 FX |
| 生成 scratch、缓存和 mixed effects | 实际生成 pass 或相邻 capability owner | 每次调用局部值或已有真实 owner 状态 | 是否跨 tick、跨线程、跨世界恢复均逐项判定；当前 unknown |

## 5. 旧入口到候选组合

| 旧入口 / 观察行为 | 候选 API 组合 | 必须保持的合同 | 当前缺口 |
|---|---|---|---|
| Main → WorldGen.serverLoadWorld → serverLoadWorldCallBack → WorldFile.LoadWorld | Lifecycle: BeginLoad → persistence decode/repair → compatibility gate → settle → validate/finalize → MarkReady 或 MarkLoadFailed；failure adapter 按目标 callback 保留两轮 LoadWorld 尝试与 `.bak` 恢复 | gate 可见时点、WorldGen.UpdateWorld 短路、进度/版本返回码、错误消息和旧 world 状态；不得移入完整参考树独有的非 dedicated-server 菜单路由 | CPG 未闭合 callback delegate 边；settle 异常后的 gate 清理、cloud 早退状态、cancel/unload 与旧 world 保留策略 unknown |
| NPC → StartHardmode → TransformWorldOnBackgroundThread | Lifecycle/transform intent → worker/lock adapter → completion/failure → main-thread follow-up | hardMode 和 item protection、计数、锁、finally 与 follow-up 次序 | Version4 initializeHardMode 为空；真正地形效果 unknown；force/netMode 分支需显式兼容评审 |
| WorldFile._SaveWorld 等 transformation 后取得 IOLock | Save intent → lifecycle barrier → storage adapter 在一致锁/快照上写入 | save 不与 transform 并行；保持错误、临时文件和 cloud save 行为 | 新 barrier 的取消、异常和锁顺序等价未验证 |
| QuickFindHome / StartRoomCheck / RoomNeeds / ScoreRoom | HousingValidationSystem.EvaluateRoom → HousingRoomScoreSystem.ScoreRoom → immutable result → occupancy/assignment commit → feedback/achievement adapters | bounds、扫描顺序、special NPC 条件、善恶平衡、家具/箱子扣分、shared-room、评分 tie-break、NPC home/homeless 可观察结果 | `ScoreRoom` 只接受分类后的 Tile source；临时 Main.tileSolid[379] 恢复不是 finally，真实 Tile 映射、feedback 动态调用闭包和 assignment 仍 unknown |
| WorldGen.TownManager 与 WorldFile SaveTownManager / LoadTownManager | Registry read/write roles → persistence adapter → load 后 room revalidation → assignment commit | 旧数据计数、resident key、room coordinates、occupancy 和 kick-out 次序 | key mode、并发保护、截断/非法值和 owner 未决 |
| Main → WorldGen.UpdateWorld → CountTiles | 原 tick cadence → `WorldTileMetricsSystem.AdvanceCadence` → `BeginColumn` → message 57 adapter → `CompleteColumnStart` → `ScanColumnTiles`（read-or-create Tile、逐格 wall/active/tile effects）→ alignment 聚合与 `CompleteColumn` → 末列 Skyblock effect adapter | 30 次 cadence、totalX wrap、X=0 先发布/发送再重置、CountTiles 累计公式及末列效果顺序 | 组件内 cadence 与 `ScanColumnTiles` 是局部候选算法；Main caller、真实 Tile/effect adapters、message 57 consumer、Skyblock owner 及跨 session reset 仍 unknown |
| Version4 TreeTopsInfo Save/Load/SyncSend（完整参考树另有 SyncReceive） | TreeTops state → persistence adapter / immutable network projection | 13-area 次序、版本 <211 fallback、count 与 byte layout | Version4 的旧版复制方法为空；接收协议和 FX 只在完整参考树出现，不能默认移植 |
| SetWorldSize / oceanLevel / TransformingWorld / genRand | dimensions Query + explicit commit role；纯派生 Query；lifecycle read；random source port | 数值、输入错误和 RNG 调用顺序兼容 | allocation caller、非有限输入和多 world scope unknown |

以上是概念组合，不规定必须创建同名 DTO、Command 或 Adapter 类型。若一次同步调用足够表达顺序和提交点，就不引入额外编排层。

## 6. 执行次序与依赖

| 流程 | 静态可见顺序 | 目标设计约束 |
|---|---|---|
| 读档与 ready | WorldFile decode/repair → 开启 legacy load gate → liquid settle/water check → 清 gate → NPC/世界收尾；失败分支设置 loadFailed | lifecycle owner 可以提前记录内部 attempt，但 legacy projection 时点不得提前；Ready 只能在已确认 settle 与必需验证完成后提交 |
| hardmode transform 与 save | NPC caller → StartHardmode 状态和 item protection → 后台 transform under WorldFile.IOLock → finally 减计数并排主线程 follow-up；SaveWorld 等计数归零后取锁 | 把执行器、锁和主线程队列当作外部协作边界；明确并发数、失败、取消和锁序。具体转换工作在目标 Version4 为 unknown |
| housing assignment | Main/NPC caller → QuickFindHome → StartRoomCheck/规则与评分 → registry occupancy check → NPC assignment/反馈 | 搜索评估与权威登记分开；只在 valid result 与 occupancy 决策通过后提交；异常不能留下半提交 |
| metrics tick | Main 的两个已确认调用点 → WorldGen.UpdateWorld lifecycle gate → wiring/TileEntity/lunar → 30 次计数门槛 → CountTiles(totalX) → 列游标推进 → Liquid/weather/random passes | metrics 必须在旧 tick 的同一点提交，不能通过新 scheduler 延后一帧或改变 RNG 消耗顺序 |

直接依赖候选：Main/WorldFile/NPC 是旧入口适配方；HousingValidationSystem 可只读 Tile 并请求 Registry occupancy；Registry persistence adapter 负责格式转换；WorldTileMetricsSystem 把 snapshot 交给网络与 Skyblock 外部 owner；TreeTops persistence/network 只投影或恢复其 owner 状态。跨系统读共享快照，不授予写权限。

## 7. 输入组覆盖映射

| P16 输入组 | 拟映射的能力切片 | 处理说明 |
|---|---|---|
| WorldGenBiomeBackgroundAndDistanceMetrics | environment profile / derived query、Town registry、Manifest、random source | 原组混合状态，按不变量拆分；不设统一 owner |
| WorldGenTileCountMetrics | WorldTileMetricsSystem | 保留横向列扫描和累计窗口 |
| WorldLifecycleLoadAndTransformState | WorldSessionLifecycleSystem 与 storage/worker ports | load gate、failure、transform 和 save barrier 的恢复合同待定 |
| WorldLifecycleProgressionAndEventState | Progression/event owners | P16 交接，最终 owner unknown |
| WorldLifecycleHousingAndSpawnPacingState | Housing assignment 与 spawn pacing rules | 住房、掉落抑制和生成节奏不得合并为一个组件 |
| WorldLifecycleTileMergeState | Tile mutation / framing owner | 跨分区交接，P16 不定 owner |
| WorldHousingCountersAndScoringState | HousingValidationSystem 的局部 scan/score context | 不提升搜索暂存量为 NPC 权威状态 |
| WorldHousingRoomSearchState | HousingValidationSystem | 请求级状态优先，不共享可变静态 scratch |
| WorldHousingRuleAndDiagnosticState | housing rules、generation rule 与 event/diagnostic owners | 分别归属；日志事件不等于住房 state |
| WorldGenerationDimensionsState | dimension catalog/query 与明确提交角色 | meteor counter 单独交给 event owner |
| WorldGenerationScratchState | generation pass 调用局部 scratch | 只有跨 tick/恢复证据成立时才建 session state |
| WorldTerrainEffectsAndCaches | TreeTops + Tile/weather/item/cache 等真实 owners | 混合组逐项交接，不建 Environment 总括 System |
| WorldGenDerivedProperties | lifecycle query、random source、ocean-level query | 三种不同读取语义 |

## 8. 阻塞决策

以下决策未完成前，不切换 legacy caller，不删除旧实现，也不宣称 owner 唯一：

1. world session 的 authoritative identity，以及跨 generation revision、world reload 和多个 world 的范围。
2. Loading、Ready、Failed、Cleared、Transforming 的终态、retry/cancel、异常恢复与旧 world 保留策略。
3. Town resident 是 NPC type、NPC instance 还是另一种 key；NPC home 字段与 registry 的权威提交和存档兼容。
4. Main 两个 UpdateWorld 注入点、worker/lock/main-thread queue 的调度和随机数调用顺序。
5. CountTiles 的完整 Tile/Skyblock 写入闭包、message 57 的方向/consumer 和 metrics commit schema。
6. TreeTops 两版本的保存/网络格式、<211 copy path、短读行为、全部 style writer 与 FX owner。
7. P16 mixed progression/event/weather/Tile/effect/scratch 字段的 integration-review owner 清单。

## 9. 当前代码状态与边界

NSSLC 已有 WorldGenerationLifecycleComponent、WorldLoadLifecycleComponent、TownHousingRegistryComponent、WorldLayerMetricsComponent 与 WorldLayerMetricsSystem 等局部形状。新增 `WorldLoadLifecycleSystem`/`WorldLoadRecovery*` 将目标源码可见的 load retry、`.bak` 恢复和 gate 时点表达为状态与 effect 请求；新增 `HousingValidationSystem`、`HousingRoomScoreSystem`、`TownHousingRegistrySystem` 与 `TownHousingRegistryPersistenceAdapter` 分别承接显式房间规则、分类 Tile 评分候选、NPC type key 的房间/无家互斥索引和 Version4 `count/type/x/y` 布局。TreeTops state/area/randomization、持久化 adapter 和 network projection，以及 WorldTileMetricsComponent/System 仍是局部候选。读档 verifier 3 项、housing validation 3 项、housing registry 3 项、housing score 5 项，加上 TreeTops 7 项、tile metrics 5 项和 transform barrier 2 项均只覆盖各自局部候选。尚无证据证明这些新类型已由 WorldFile/NetMessage/Main/Skyblock/NPC 调用，也未证明 P16 旧入口、完整 loader、主循环、真实 Tile adapter、TreeTops 接收和 reset/unload 已接入。

SS14 的 `Content.Server/Animals/Systems/EggLayerSystem.cs` 展示由领域 System 持有定时/随机依赖并在边界调用效果服务；GridPreloader 和 Atmosphere processing 可作显式 session/map 状态及游标式长处理的形态参照。这些只用于 ECS 结构对照，不把 RobustToolbox 的注册、调度或 API 当成 NSSLC 的已知合同。本设计的 owner、锁、调度和协议合同全部以 Terraria/Version4 目标源码为准。

已实现代码仍是独立候选切片：读档恢复状态机、NPC type housing registry/codec、TreeTops state/area/randomization/codec/projection 与 CountTiles cadence/column-scan/alignment/publication core；未编辑 Version4 或完整参考树，也没有切换旧入口。authoritative world-session identity、housing evaluation/NPC assignment、真实 CountTiles Tile/effect adapters、cadence 到 Main 的接线、跨 session reset、TreeTops 接收/FX、跨域 owner 与宿主注册仍未实现或未闭合。

## 10. 已实现切片与后续验收入口

新增局部切片：`WorldLoadLifecycleSystem` 仅编排 Version4 源码已确认的 load/retry/backup 顺序和 gate effect 时点；`HousingValidationSystem` 通过 `IHousingTileSource` 执行显式 8 邻域 flood-fill、边缘/尺寸/solid/open-gate/墙体规则和家具要求；`HousingRoomScoreSystem` 通过 `IHousingRoomScoreTileSource` 计算已分类 Tile 上的 ScoreRoom 候选分数；`TownHousingRegistrySystem` 与 `TownHousingRegistryPersistenceAdapter` 仅维护 NPC type key 的 room/homeless 互斥索引并保留 `count/type/x/y` 文件布局。上述切片均未接入 WorldFile、Main、NPC 或运行时 session 注册，不能据此声称行为等价。

Housing validation 的局部 verifier 输出 3 条 `PASS`，覆盖封闭房间遍历、缺少家具要求和世界边缘拒绝。Housing score focused verifier 输出 5 条 `PASS`，覆盖无效房间、候选顺序及箱/家具扣分、善恶惩罚与非对称边界、shared-room proximity 和碰撞端口拒绝。真实 Tile 类型映射、occupancy、NPC 特例、feedback 副作用、完整评分闭包和 NPC assignment 仍为 `unknown` 或 `partial`。

`WorldTreeTopsPersistenceAdapter.Save` 写出 13 项计数和按 area 顺序的 13 个整数。`Load` 对版本 <211 不改状态；新版本读取文件计数并按目标方法的上限更新已有项，未提供的项保持原值，短读异常传播且先前已读取项保持已提交。`WorldTreeTopsNetworkProjection.CreateSyncPayload` 从不可变快照生成 13-byte 数组，并按 Version4 显式 byte 转换截取各样式值。它不发送网络包，也不实现目标树中不存在的接收/FX 行为。

TreeTops 独立 verifier 的 7 项场景覆盖保存布局、部分计数加载、<211 no-op、byte 投影、区域选择、随机样式变化和 inactive Tile 不消耗随机数。tile metrics verifier 的 5 项场景覆盖 alignment/remix 汇总与比例、clearCounts、scratch 清空、末列 Skyblock 请求、列扫描的 5/1 区间权重与逐格效果顺序，以及 29/30/31 tick 和列游标回绕。结果只支持这两个局部候选 API；WorldFile/NetMessage/Main/Skyblock 尚未改为调用这些 API，session reset 与其他 P16 行为仍未验证。原 P16 报告仍为 `designStatus: proposed` / `verificationStatus: not-run`，本设计整体也保持 proposed。

其余行为应按相同 world/session 与输入比较可观察向量：返回/错误、权威状态差异、事件和外部效果、顺序及可见时点、作用域/lifecycle、retry/idempotency。必须覆盖旧入口并通过对应行为验收后，才能提高 P16 整体状态。

完整待验收场景见关联执行计划与 P16 报告 Verification Plan；上述 TreeTops 7 项、tile metrics 5 项、transform barrier 2 项、load recovery 3 项、housing validation 3 项、housing registry 3 项和 housing score 5 项仅为局部检查。真实 Tile 分类、occupancy、special NPC、feedback/tie-break 闭包及旧入口集成验收仍为 not-run；P16 全量行为验证未运行。
