# P16 世界生命周期、住房与指标 System 执行计划

~~~yaml
documentType: system-execution-plan
partitionId: P16
taskId: AUTH-SYS-P16
originalSessionId: b65120e249874094ac5069b99b2fb9c7
sourceReport: D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P16-world-lifecycle-housing-metrics.md
outputReport: D:\TRbackup\NLTX\docs\system-decomposition\reports\2026-09-18-system-decomposition-authoritative-P16-world-lifecycle-housing-metrics.md
inputMembers: 133/133
designDocument: D:\TRbackup\NLTX\docs\plans\system-decomposition\2026-09-30-p16-world-lifecycle-housing-metrics-system-design.md
targetRoot: D:\TRbackup\NLTX\src\NSSLC
targetSourceRoot: D:\TRbackup\Version4
completeReferenceRoot: D:\TRbackup\无任何删减通过编译
comparisonReferenceRoot: C:\Users\shan\Downloads\ECS\space-station-14-master
executionStatus: partial
implementationStatus: partial
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

范围：基于 authoritative P16 proposed 静态设计的未来迁移步骤。设计依据：[P16 System 设计](2026-09-30-p16-world-lifecycle-housing-metrics-system-design.md)；`sessionId` 使用原值 `b65120e249874094ac5069b99b2fb9c7`；runner `completed` 仅表示 P16 报告已结算。

本文是 proposed 执行文档，不是执行完成报告、行为等价证明或迁移成功证明。住房评分代码与 focused verifier 属于此前形成的局部候选结果；本轮文档复核没有重跑 build/test。

元数据中的 `verificationStatus: not-run` 指 P16 必需行为矩阵尚未验证；`thisTurnVerification: partial` 只记录住房评分候选切片的 focused verifier 结果。

当前状态覆盖：执行状态 partial；设计 proposed；P16 全量验证 not-run；load recovery、housing validation、housing registry 各有 3 项局部核心验证通过，housing score 有 5 项 focused verifier 场景通过（2026-10-01）。

## 1. 目标与限制

局部记录包括：load recovery 3 项、housing validation 3 项、housing registry 3 项和 housing score 5 项 focused verifier 场景为 passed（2026-10-01）。文档元数据记录局部源码与验证活动；P16 全量仍为 not-run，执行状态仍为 partial，迁移状态仍为 not-claimed。

目标是在 integration-review 决定关键身份、恢复、owner 和协议合同后，按小批次把 P16 概念能力接入 NLTX 的 world/session 路径，保持旧入口可观察行为，并用必需行为验收证明迁移切片。

本计划不是迁移完成报告。局部候选包括 TreeTops state/area/randomization、持久化 adapter 与 network payload projection，CountTiles cadence/alignment/列扫描核心，transform transaction 计数，以及 housing load/validation/score/registry。局部 verifier 数量为 TreeTops 7、tile metrics 5、transform barrier 2、load recovery 3、housing validation 3、housing score 5、housing registry 3；它们不构成旧入口行为等价证据。住房评分使用显式分类 Tile facts，不接真实 Tile adapter、occupancy、NPC assignment 或 feedback。未把候选 API 接入 WorldFile、NetMessage、Main、Task scheduler、IOLock、Skyblock 或 NPC assignment。world-session identity、housing assignment、真实 CountTiles Tile/effect adapters、cadence 到 Main 的接线、跨 session reset、TreeTops 接收/FX 和宿主注册仍未实现或未闭合。实现代码根为 D:\TRbackup\NLTX\src\NSSLC；Version4 与 D:\TRbackup\无任何删减通过编译均为只读参考树。

Command、Query、Adapter、Projection 在计划中表示职责角色，不预设每个角色必须成为独立类型。新增 C# 文件、移动 ECS 文件和组件字段前，应重新读取根 AGENTS.md 指定的 ECS 文件组织、组件命名、C# 风格与副作用约束。

## 2. 阶段与依赖

| 阶段 | 前置 | 范围 | 阶段输出 |
|---|---|---|---|
| E0 决策与调用闭包 | 无 | session identity、失败/取消、housing key、metrics 外部 owner、TreeTops 协议和宿主入口 | 有签署/记录的 integration decisions；阻塞项保持 unknown |
| E1 生命周期与 transform 屏障 | E0 完成 lifecycle 决策 | load 状态、legacy gate、transform worker、save barrier 和恢复路径 | session owner 与兼容 adapter 的候选实现 |
| E2 housing validation 与 registry | E0 完成住房 key / 双写决策 | 搜索 context、住房规则、登记、NPC assignment、持久化恢复 | 分离的评估与提交路径 |
| E3 分列 metrics | E0 完成 metrics / Skyblock / network 决策 | cursor、累计窗口、snapshot、message 57 和 observer 接口 | 旧 tick 点驱动的 metrics 路径 |
| E4 TreeTops state 与协议 | E0 完成旧格式/net 合同 | 13-area state、style 写入入口、save/load、network projection | codec 与 state owner 的边界 |
| E5 宿主组合与跨域交接 | E1–E4 有候选实现且 seam 已定 | Main/NPC/WorldFile 接入点、session 注册、Tile/weather/event handoff | 一条真实旧调用路径进入新组合 |
| E6 行为验收与发布门禁 | E5 完成 | 行为矩阵、协议兼容、隔离与恢复检查 | 有范围、有输出、有结果的验收记录 |

E2、E3、E4 在 E0 各自阻塞决策关闭后可并行实现；E1 是 lifecycle/session authority 依赖。E5 不能先于各个 owner contract 与真实宿主入口确认。E6 不可被编译成功、静态映射、类型存在或 runner completed 替代。

## 3. 阶段明细

局部 verifier 记录：TreeTops 7 项、tile metrics 5 项、transform barrier 2 项、load lifecycle 3 项、housing registry 3 项、housing validation 3 项及 housing score 5 项均经仓库串行 runner 构建和运行。下文列出住房评分项目、参数、退出码与产物路径。所有这些结果只覆盖局部状态、codec、显式 Tile-source 规则或候选状态模型，不构成 WorldFile/Main/NPC 旧入口的行为等价证据。

### E0：冻结证据并完成 integration-review 决策

**输入：** P16 报告、本文设计、Version4 目标源码、CPG 只读查询记录、完整参考源码对照。  
**输出：** 有来源的决策记录和调用/读写/持久化/网络缺口清单。

行动：

1. 固定准备实施时的 Version4 文件 hash 或 commit，重新定位本计划引用的源码行。CPG 当前 manifest 未绑定 source snapshot。
2. 指定 authoritative world-session key；区分 world identity、generation revision 和 process lifetime。
3. 决定 load failure、settle failure、retry、cancel、world clear/unload 的状态终态和保留旧 world 策略。静态源码已确认 `serverLoadWorldCallBack` 的两轮尝试与 `.bak` 恢复顺序；完整参考树额外有非 dedicated-server 的菜单错误路由，不能自动移入 Version4 目标。
4. 决定 Town resident key、NPC home 字段与 registry 的单一权威提交者、双写顺序以及 legacy save/load 格式责任。
5. 对 Main 两个 WorldGen.UpdateWorld 调用点、后台 worker、WorldFile.IOLock、main-thread queue 和 shared RNG 消耗次序做 integration review。
6. 指定 CountTiles 对 Tile/Skyblock 的写入责任、metrics snapshot、message 57 的网络方向和消费端；逐段核对 `NetMessage` 序列化与 `MessageBuffer` 接收分支。完整参考树的接收行为只能作为差异证据，Version4 空分支保持 unknown。
7. 对照完整参考树与 Version4 逐项审查 TreeTops 和 hardmode。完整参考树的 initializeHardMode 有转换逻辑，Version4 目标函数体为空；在目标行为决策和实现源确定前，不实现猜测的地形转换。
8. 把 progression、weather、spawn pacing、Tile merge、exploit queue、item protection 与 scratch 的跨分区 owner 逐项交办，不将它们塞进 P16 总括 System。

**完成条件：** 对 E1–E5 必需的 owner、identity、提交、协议和宿主入口都存在有依据的决定。任何未解决项继续标 unknown，并阻止相关旧入口切换。

### E1：实现世界会话生命周期和 transform/save barrier

当前局部实现（2026-10-01）：`WorldLoadLifecycleSystem` 将 Version4 已确认的主档两次尝试、`.bak` 检查/恢复后两次尝试和 load gate 时点表达为 `WorldLoadRecoveryAction`/`WorldLoadRecoveryPhase` 状态机。它只返回文件/宿主 effect 请求，不执行文件复制、Task、锁或宿主注册；session identity、取消和异常清理仍 unknown。

**依赖：** E0 的 session identity、终态、并发和 host queue 决策。  
**候选目标文件：**

- 现有：src/NSSLC/Component/WorldSession/WorldGeneration/WorldGenerationLifecycleComponent.cs
- 现有：src/NSSLC/Component/WorldSession/WorldGeneration/WorldLoadLifecycleComponent.cs
- 候选：src/NSSLC/Component/WorldSession/WorldGeneration/Systems/WorldSessionLifecycleSystem.cs
- 局部候选：src/NSSLC/Component/WorldSession/WorldGeneration/WorldTransformTransactionComponent.cs 与 Systems/WorldTransformTransactionSystem.cs
- 候选：按既有项目边界落地的 storage、worker、lock 和 main-thread queue adapter；具体项目/目录在 E0 盘点后定

行动与验收：

1. 先统一 generationId 与 authoritative session identity；定义 Loading、Ready、Failed、Cleared、Transforming 转换及 revision/并发规则。
2. 用一个 owner 推进生命周期状态。WorldFile 兼容 adapter 负责 decode/repair 结果和旧 load flag 投影，不能从多个 facade 分散写状态。
3. 保留旧读档 gate 的可观察时点、liquid settle 次序、错误码和进度行为；定义失败、取消和 retry 的清理语义。按 Version4 callback 保留首次 `LoadWorld`、一次直接重试、`.bak` 检查/恢复后再最多两次读取的次序。
4. 通过 worker/lock/queue 边界管理 transform 计数与完成消息；确认失败时 follow-up 是否运行、多个 transform 是否允许并行及 save 等待/取消行为。局部 transaction System 只负责并发安全计数，不替代 scheduler、IOLock 和队列。
5. 在未证明真实 hardmode 内容前，只迁移已确认的 transform barrier 机制，不宣称地形转换已迁移。

**验收输出：** 状态转换表、入口映射、失败恢复证据、旧 save gate 与锁合同。异常、取消或状态转移不一致时不切换旧入口。

**当前局部实现（2026-10-01）：** `WorldTransformTransactionSystem` 委托唯一候选 `WorldTransformTransactionComponent` 安全地增加/减少活动计数；并发 begin/complete 都推进 revision，underflow 与计数/revision 上限会在状态变更前拒绝。`HasActiveTransactions` 只报告计数，不等价于可安全写档；目标的 `WorldFile.IOLock`、worker finally 与 main-thread follow-up 尚未接入。该 component 需要覆盖与目标静态 `_transformingWorld` 相同的共享范围，其唯一实例注册仍 unknown；无 active transaction 前不能据此处理 session reset。

**读档源码合同与差异（2026-10-01）：** CPG 在所选 `WorldGen.cs` shard 对 `WorldFile.LoadWorld` 返回 4 个 confirmed 直接调用，`WorldGen.serverLoadWorld` 在 `Main.cs` 有 1 个 confirmed 调用；`serverLoadWorldCallBack` 的 delegate 调用边返回 `partial / NoMatchingFactInScannedScope`，但 `Task.Factory.StartNew(serverLoadWorldCallBack)` 在源码中明确可见。Version4 与完整参考树的 `WorldFile.cs` SHA-256 相同：云不可用会先设置 `loadFailed` 并早退；成功打开输入后的读取 try 才清除旧 `loadFailed`；decode/repair 后开启 `isGeneratingOrLoadingWorld`，liquid settle 与 `WaterCheck` 后清除；可见 catch 设置失败但不显式清 gate。callback 首次失败会直接重试，仍失败时检查 `.bak`、拷贝覆盖并删除备份，再读两次；最终成功才运行音效、临时时间恢复和 `Hooks.WorldLoaded()`。完整参考的 callback 在非 dedicated-server 失败时还设置菜单模式 200/201；Version4 不含该 UI 路由，执行时以目标为准，不补入参考差异。gate 异常恢复、重试中旧 world 保留、取消和 unload/reset 的终态仍 unknown。

**Transform barrier 局部验证记录（2026-10-01，来自既有执行记录，本轮未重跑）：** 使用 SDK `10.0.400` 和 `Build/Tools/Invoke-SerialDotnet.ps1` 串行构建，build 退出码 0、0 warnings、0 errors；verifier run 退出码 0，2 项 `PASS`（事务计数与 underflow guard、并发事务精确计数）。输出目录为 `Build/bin/Terraria.WorldSession.P16TransformBarrier.Verification/Debug/net10.0/`。当前保留的执行摘要未包含可核对的完整 argv，因此命令参数记为 `unknown`，不按相邻 verifier 命令补造。本轮只做只读查询和文档更新，没有运行 build/test；局部 verifier 只覆盖 transaction 状态模型。

### E2：分离 housing evaluation、registry 和 NPC assignment

当前局部实现（2026-10-01）：`HousingValidationSystem` 通过 `IHousingTileSource` 实现 8 邻域 flood-fill、Version4 可确认的边缘/尺寸/solid/open-gate/墙体规则和家具要求；`HousingRoomScoreSystem` 通过 `IHousingRoomScoreTileSource` 接收已分类 Tile，表达善恶平衡惩罚、候选扫描、上方 solid、basic chest/furniture 扣分、shared-room 规则和首个最高分选择；`TownHousingRegistrySystem` 已实现 NPC type key 模式下的 room/homeless 互斥提交、查询、清理和 revision；`TownHousingRegistryPersistenceAdapter` 已保留 Version4 的 `Int32 count` 与 `npcType,x,y` 顺序。ScoreRoom 的候选实现有 5 项 focused verifier 通过；真实 Tile type 映射、feedback 动态副作用、occupancy、special NPC 条件、完整评分/tie-break、NPC home 双写与 restore 后重验仍未实现，因此 E2 仍为 partial。

**Housing score 局部验证记录（2026-10-01）：** 项目：`Test/Terraria.WorldSession.P16HousingScore.Verification/Terraria.WorldSession.P16HousingScore.Verification.csproj`；SDK `10.0.400`。使用 `Build/Tools/Invoke-SerialDotnet.ps1` 串行构建，build 退出码 `0`、`0 warnings`、`0 errors`；随后使用 `run --no-build --no-restore` 运行，退出码 `0`，5 项 `PASS`：无效房间不评分；最佳候选及 chest/furniture 扣分顺序；善恶惩罚与非对称 bounds；shared-room proximity；collision policy 拒绝占用的头部空间。

构建命令：

```powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
$dotnetArguments = @(
  'build',
  './Test/Terraria.WorldSession.P16HousingScore.Verification/Terraria.WorldSession.P16HousingScore.Verification.csproj',
  '-m:1',
  '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
```

Verifier 命令：

```powershell
$dotnetArguments = @(
  'run',
  '--no-build',
  '--no-restore',
  '--project',
  './Test/Terraria.WorldSession.P16HousingScore.Verification/Terraria.WorldSession.P16HousingScore.Verification.csproj',
  '-m:1',
  '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
```

产物：`Build/bin/Terraria.WorldSession.P16HousingScore.Verification/Debug/net10.0/Terraria.WorldSession.P16HousingScore.Verification.dll`。结果只覆盖候选 System 与假 Tile source，不调用 Version4 旧入口。

**依赖：** E0 的 resident key、权威提交、保存格式和 feedback 决策。  
**候选目标文件：**

- 现有：src/NSSLC/Component/WorldSession/WorldGeneration/TownHousingRegistryComponent.cs
- 现有：src/NSSLC/Component/WorldSession/WorldGeneration/TownHousingRegistry.cs
- 现有：TownHousingResidentKey.cs、TownHousingKeyMode.cs、TownHousingAssignmentComponent.cs、HousingScanStateComponent.cs
- 候选：src/NSSLC/Component/WorldSession/WorldGeneration/Systems/HousingValidationSystem.cs
- 局部候选：src/NSSLC/Component/WorldSession/WorldGeneration/Housing/HousingRoomScoreTileSample.cs、IHousingRoomScoreTileSource.cs、Systems/HousingRoomScoreSystem.cs
- 候选：src/NSSLC/Component/WorldSession/WorldGeneration/Systems/TownHousingRegistrySystem.cs
- 候选：housing evaluation result 与 WorldFile/NPC/feedback 兼容 adapter，具体路径由 NSSLC 项目边界盘点决定

行动与验收：

1. 将 QuickFindHome、StartRoomCheck、RoomNeeds、ScoreRoom 分解成明确输入、请求级 scratch、规则评估结果和最终 assignment commit。
2. 先确认扫描边界、顺序、NPC 特例、score tie-break、tile solidity 替代、occupancy 和共享房间规则，再决定哪些读取可成为纯 Query。
3. 对 Main.tileSolid[379] 建立范围受限的保存/恢复；行为契约必须覆盖异常时恢复，不能依赖当前非 finally 路径。
4. Registry 保存 room 与 homelessness 的唯一权威状态；读档 restore 后重验真实房间，只有完成验证才经 assignment 提交 NPC home/homeless。
5. TownRoomManager 旧数据以 NPC type 为 key；在 key contract 和旧 schema mapping 未确定时，保持兼容入口且不声明 registry 是最终 owner。
6. 将 IRoomCheckFeedback、achievement 和消息输出作为明确效果边界；对 callback 动态目标及重入行为保留来源证据。
7. 保留现有 sample-backed `HousingRoomScoreSystem.ScoreRoom` 的 5 场景 focused verifier；在真实 Tile 分类、occupancy 和 special-NPC 条件确定后扩展行为覆盖，不得把显式 sample 结果写成旧 `ScoreRoom` 等价。

**验收输出：** housing 评估/提交分界、key mapping、存档 round-trip 结果、房间复核/驱逐次序和异常无半提交证据。

### E3：把 CountTiles 变成旧 tick 点上的指标能力

**依赖：** E0 的 Main 注入点、Tile/Skyblock owner、snapshot 和 message 57 合同。  
**候选目标文件：**

- 现有：src/NSSLC/Component/WorldSession/WorldGeneration/WorldLayerMetricsComponent.cs
- 现有：src/NSSLC/Component/WorldSession/WorldGeneration/Systems/WorldLayerMetricsSystem.cs
- 已有局部候选：WorldTileMetricsComponent、WorldTileMetricsSystem；含 component 内 30 tick/列回绕 cursor、显式 Tile/effect ports 驱动的列扫描和 alignment 累计/发布算法，不覆盖真实宿主适配
- unknown / pending integration: metrics component 与 authoritative world session 的绑定、跨 world 重建/reset/unload 时点；不得把现有 layer metrics 直接改名视作已迁移
- 候选：NetMessage 57 projection 和 Skyblock state/effect adapter，项目路径由 E0 盘点确认

行动与验收：

1. 对照 CountTiles 完整算法梳理各 Tile 区间、权重、字段累加、Skyblock 写入和边界 Tile 创建行为。
2. 独立保存 totalD、totalX、周期累计和一个 world/session 的窗口；处理 X wrap 与跨 world 清零。
3. 在原 UpdateWorld cadence 的同一入口推进 30 tick 逻辑，保持调用次数和 Liquid/weather/random pass 的先后关系。
4. X == 0 时以不可变 snapshot 提交百分比、特殊最小值处理、累计清零与单次 message 57；具体客户端/服务端方向按确认的协议执行。
5. 不允许查询、HUD 读取或 snapshot 创建推进扫描游标。

**验收输出：** 列扫描算法映射、29/30/31 tick cadence、world width wrap、X=0 snapshot 与网络/Skyblock 交接证据。

**当前局部实现：**

- `src/NSSLC/Component/WorldSession/WorldGeneration/WorldTileMetricsComponent.cs` 持有候选 tick/cursor、alignment 周期计数与已发布快照；尚未和真实 world-session identity、创建/清理生命周期绑定。
- `src/NSSLC/Component/WorldSession/WorldGeneration/Systems/WorldTileMetricsSystem.cs` 的 `AdvanceCadence` 按 component 计数到第 30 tick 后返回下一列并按 `maxTilesX` 回绕；`BeginColumn` 在 X==0 发布已累计值并返回 message 57 请求；`CompleteColumnStart` 表达 adapter 完成旧发送调用后的清零提交点，不声称 `NetMessage.SendData` 提供发送成功结果。`ScanColumnTiles` 按 Version4 的两个垂直范围和 5/1 权重扫描 sample，通过 `IWorldTileMetricsTileSource.ReadOrCreateTile` 获取逐格 sample，并按原顺序调用 wall、active-count 与 tile-flag effects；`AccumulateAlignmentCounts` 按 Version4 `AddUpAlignmentCounts` 集合与 remix 分支更新计数并清空输入 scratch；`CompleteColumn` 在聚合后返回末列 `Skyblock.Calculate()` 效果请求。
- 该候选核心不直接读取 `Main.tile`、创建 Terraria `Tile`、拥有 Skyblock flags、触发 `UpdateWorld`、发送网络消息或调用 `Skyblock.Calculate()`。真实 caller/adapter 必须按 `AdvanceCadence` → `BeginColumn` → message 57 → reset → `ScanColumnTiles` → alignment aggregation/CompleteColumn → 末列 Skyblock effect 的顺序接入；当前没有该接线，也未证明跨 session reset、重入或中途异常等价。`AdvanceCadence` 要求正的世界宽度。
- Version4 与完整参考树的 `CountTiles`/`AddUpAlignmentCounts` 主体一致；message 57 条件不同，目标 Version4 无条件发送，完整参考树只在 netMode==2 时发送。实现按 Version4 返回发送请求。

**完整参考树与关系补查（2026-10-01）：** 通过 `CpgEvidence.ps1` 只读 API 在 `WorldGen.cs` shard 查询 `Skyblock` 直接类型面（complete，18 成员）、`Skyblock.Calculate()` 调用点（complete，2 点：`Skyblock.ScanTiles()` 与 `CountTiles()`）及 `hasWall`（`bool[]`）。`hasWall` 的 5 条局部 member-use 查询虽覆盖 complete，但每条访问方向为 `Unknown`、事实状态为 partial；effects 不由 CPG 闭合。源码回读确认 CountTiles 逐格创建 null Tile、先记 wall、active 后递增并记 tile；`Skyblock.Calculate()` 会更新/清零 flags、`currentActiveTiles` 和 dungeon 坐标，并可能发 message 7。独立 `Skyblock.ScanTiles()` 对全图区域扫描后调用 Calculate，不等同 CountTiles 的逐列 cadence。

静态版本差异：Version4 的 `WorldGen.UpdateWorld` 在 30 tick cadence 调 `CountTiles(totalX)` 时没有 multiplayer-client guard，完整参考树在 cadence 外有 `Main.netMode != 1`；Version4 `CountTiles` 在 X==0 无条件发送 message 57，完整参考树只在 `Main.netMode == 2` 发送。两侧 `NetMessage` 都序列化三个 alignment byte，但完整参考树 `MessageBuffer` 在客户端解码，Version4 对应 case 57 为空。因此 Version4 接收/消费语义仍是 `unknown`，不得拿完整参考树补成已确认行为。Skyblock Calculate 的外部状态与 message 7 effects 应交其 owner，不归入 metrics accumulator 的纯聚合职责。

**局部核心验证记录（2026-10-01）：** 只运行 `Test/Terraria.WorldSession.P16TileMetrics.Verification`，未运行其他 P16 验收。SDK 为 `10.0.400`，通过 `Build/Tools/Invoke-SerialDotnet.ps1` 串行构建；构建命令为 `& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', './Test/Terraria.WorldSession.P16TileMetrics.Verification/Terraria.WorldSession.P16TileMetrics.Verification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')`，运行前将 `C:/Users/shan/.dotnet` 加到当前 PowerShell PATH 前端。退出码 0、0 warnings、0 errors；目标产物为 `Build/bin/Terraria.WorldSession.P16TileMetrics.Verification/Debug/net10.0/Terraria.WorldSession.P16TileMetrics.Verification.dll`。随后执行 `& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '--no-build', '--no-restore', '--project', './Test/Terraria.WorldSession.P16TileMetrics.Verification/Terraria.WorldSession.P16TileMetrics.Verification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')`，退出码 0，5 条 `PASS`：alignment/remix publication、clearCounts、末列 `Skyblock.Calculate` handoff、列扫描权重与逐格效果顺序、30 tick cadence 与列回绕。核心场景覆盖 29/30/31 tick、cursor 次序、width wrap、两个 y 区间、边界权重、active type 0/inactive Tile 和 per-cell effect 顺序；不覆盖真实 Terraria Tile adapter、Main 接线、network/Skyblock adapters、跨 session reset 或其他 P16 行为。

**Tile source 端口复核（2026-10-01）：** 将 `ScanColumnTiles` 的 tile 输入改为 `IWorldTileMetricsTileSource.ReadOrCreateTile(x,y)`；原列扫描场景使用测试内存 source 复跑，五条 `PASS` 均通过。该结果验证 API 调用顺序和局部算法，不证明接口已有真实 grid adapter 或创建 Terraria Tile 的实现。

### E4：封装 TreeTops state、持久化和网络投影

**依赖：** E0 的目标版本、格式兼容、所有 style writer 和 FX owner 决策。  
**候选目标文件：**

- 现有：src/NSSLC/Component/WorldSession/WorldGeneration/Terrain/TreeTops/WorldTreeTopsStateComponent.cs
- 现有：src/NSSLC/Component/WorldSession/WorldGeneration/Terrain/TreeTops/WorldTreeTopsStateSnapshot.cs
- 候选：TreeTopsSystem、WorldFile codec adapter、network projection/receive adapter；具体路径由既有项目边界确认

行动与验收：

1. 映射 Forest1–Forest4 与 Corruption、Jungle、Snow、Hallow、Crimson、Desert、Ocean、GlowingMushroom、Underworld 的 13 个固定 area。
2. 清点所有读取和写入，包括基于 tile position 的随机样式选择；shared RNG 调用顺序和 message 7/FX 时点纳入行为合同。2026-10-01 的 CPG 全索引补查在 967 个 indexed paths 中找到 Save/Load/SyncSend/GetTreeStyle/位置随机化/旧版复制各一个直接调用点；`_variations` 的 21 个成员使用均位于 TreeTopsInfo.cs，但使用证据为 partial。Version4 Projectile 位置随机化分支在方法调用后还无条件发送 message 7，TreeTopsInfo 内部则仅在样式变化时发送；组合迁移须保留这两层效果与顺序。
3. 精确复刻目标 Version4 的 WorldFile version <211 fallback、当前 count 编码、短读/长 count/default 行为；完整参考树只能用于发现候选规则。
4. 网络接收/发送保持目标协议顺序与 byte 转换。只做 snapshot/projection 的代码不得回写 owner state。
5. 当前 NSSLC 已有 13-area state、区域判定、随机样式更新、persistence adapter 和 network payload projection；它们仍未接入 WorldFile/NetMessage，也不实现目标 Version4 缺失的旧版复制、接收或 FX 行为。

**验收输出：** 13-area 状态覆盖、旧版和当前文件格式往返、短输入/坏 count、网络字节序列及 FX 时点证据。

**当前有限实现：**

- `src/NSSLC/Component/WorldSession/WorldGeneration/Terrain/TreeTops/WorldTreeTopsStateComponent.cs` 提供 13-area 状态及范围检查；`WorldTreeTopsAreaQuery.cs` 与 `WorldTreeTopsSystem.cs` 已实现区域判定和随机样式更新，随机源显式注入。
- `src/NSSLC/Component/WorldSession/WorldGeneration/Terrain/TreeTops/WorldTreeTopsPersistenceAdapter.cs` 实现 Version4 `Save` / `Load` 的可见布局：写入 area count 和顺序化整数；版本 <211 不改状态；新版本按输入 count 至多更新 13 项，未提供项保持既有值。
- `src/NSSLC/Component/WorldSession/WorldGeneration/Terrain/TreeTops/WorldTreeTopsNetworkProjection.cs` 从 state snapshot 生成 13-byte payload，保留 Version4 `(byte)style` 转换；不负责发送或接收。
- Version4 的 `CopyExistingWorldInfo` 函数体为空。完整参考树的复制、`SyncReceive` 与 `DoTreeFX` 不纳入目标实现。
- CPG 全索引直接调用点查询均为 complete 且无 gap，但不能覆盖动态/反射入口或运行时注册；成员使用仍为 partial。当前目标源码中 Projectile 的外层无条件 message 7 与 TreeTopsInfo 内部条件发送均属于需要保留的可观察效果，不据完整参考树推断接收或 FX 行为。
- Version4 `Load` 的短读传播及已读取字段保留行为由实现保持，但本轮未专门测试；超长 count、调用顺序、WorldFile/NetMessage 接入与实际 wire round-trip 也未验证。

**核心验证记录：** `Test/Terraria.WorldSession.P16TreeTops.Verification` 在 SDK `10.0.400` 下通过仓库串行 runner 执行，退出码 0，控制台输出 7 条 `PASS`，未输出 warning/error。产物位于 `Build/bin/Terraria.WorldSession.P16TreeTops.Verification/Debug/net10.0/`。覆盖保存布局、部分计数加载、<211 no-op、byte 投影、区域选择、随机样式变更和 inactive Tile 不消耗随机数；WorldFile/NetMessage 接入、wire round-trip、接收端/FX 和实际随机流顺序仍未验证。这不代表 E4 完成或 P16 全量验证通过。

### E5：接入宿主入口并完成跨域 handoff

**依赖：** E1–E4 对应的 owner 与 API contract 已明确；NSSLC 运行宿主、注册点和项目引用关系经源码确认。

行动与验收：

1. 为 Main 的两个 UpdateWorld 入口分别追踪其状态和时序语境，不通过重复 scheduler 调度同一能力。
2. 为 NPC housing 与 hardmode 的 legacy caller 保留薄兼容入口；入口只转换输入并调用 canonical System，不重复实现业务。
3. WorldFile save/load 适配器必须保持现有版本、文件顺序、锁、失败传播和 rollback 行为；未确定 adapter 注册位置时不得猜目录或新增第二套 serializer。
4. 将 message 57、TreeTops network 与 Skyblock flags/Calculate effects 交给其跨分区 owner，提交合同及消费端必须有证据。
5. 注册一个世界 session 的 Components/Systems，并验证清理、切换、reload 与并发 world 的隔离；类型声明和静态调用搜索不等于运行时注册证明。

**验收输出：** 至少一条从真实 legacy caller 进入新组合的已观察路径；确认不存在第二写者、漏调、重复调度或一帧延迟。

### E6：执行行为验收与发布门禁

**依赖：** E5 已接入候选路径；目标行为 fixture、可观察状态和协议断言由负责代码/测试变更的批准批次实现。

最低行为矩阵：

- lifecycle：正常读档、autogen、版本拒绝、cloud 早退、settle 异常、cancel/retry、world clear/unload。
- transform/save：并发 transform、worker 异常、follow-up 异常、save 等待/取消、计数归零、IOLock 不交错。
- housing：边界 Tile、家具/光源/wall/stinkbug 规则、special NPC、评分 tie-break、互住、restore、KickOut、feedback 抛错及 tile solidity 恢复。
- registry：重复/越界 key、空和多房间、短/未知版本、旧 schema restore 后快速索引一致。
- metrics：29/30/31 tick、X wrap、完整 Tile/Skyblock 扫描、zero solid、百分比特殊值、message 57 一次且时点一致。
- TreeTops：13 个 area、version <211、当前版本、异常 count/短流、13-byte 同步顺序、style random 与接收 FX。
- isolation：重复失败与重试、world 切换、两个 world/session、reset/rebuild/save-load 不串写。
- ordering：Main 两入口、Wiring/TileEntity/Lunar、Liquid/weather/Tile passes 与同 seed RNG 调用顺序一致。

这些局部 verifier 只代表各自候选切片。剩余行为测试、运行时检查、持久化/网络往返、完整 CountTiles 调用链、真实 housing score 输入闭包、NPC assignment 以及全量行为矩阵均未运行；局部 verifier 与 build 成功都不能通过 P16 行为等价门禁。

## 4. 当前变更与剩余文件面

既有 `Test/Terraria.WorldSession.P16LoadLifecycle.Verification` 3 项和 `Test/Terraria.WorldSession.P16HousingRegistry.Verification` 3 项核心 verifier 均通过串行 runner，退出码 0、0 warning、0 error。它们只覆盖局部状态模型与 Version4 room codec，不证明 WorldFile/Main/NPC 宿主闭环。

`Test/Terraria.WorldSession.P16HousingValidation.Verification` 3 项通过，覆盖显式 tile source 的封闭房间 flood-fill、家具要求和世界边缘 guard。`Test/Terraria.WorldSession.P16HousingScore.Verification` 5 项通过，覆盖上述分类 Tile 候选评分规则；真实 Tile adapter、feedback、occupancy、special NPC 与 NPC assignment 仍未闭合。

下表标注已实现切片和未来候选改动；E0 决策未关闭的目录/入口仍不得接入。

| 位置 | 现状 / 候选动作 |
|---|---|
| src/NSSLC/Component/WorldSession/WorldGeneration/WorldGenerationLifecycleComponent.cs | 已存在；检查 session identity 和状态转换是否适合作为 canonical state，不因文件存在直接复用为 owner |
| src/NSSLC/Component/WorldSession/WorldGeneration/WorldLoadLifecycleComponent.cs | 已存在 legacy load flags holder；确定与统一 lifecycle 的关系，避免双写 |
| src/NSSLC/Component/WorldSession/WorldGeneration/TownHousingRegistryComponent.cs 与 TownHousingRegistry.cs | 已存在且标 proposed；E0 确定 key/authority 后才修改 |
| src/NSSLC/Component/WorldSession/WorldGeneration/Housing/HousingRoomScoreTileSample.cs、IHousingRoomScoreTileSource.cs、Systems/HousingRoomScoreSystem.cs | 局部候选；只消费外部已分类 Tile facts；5 项 focused verifier 通过；未接入真实 Tile adapter、occupancy、NPC assignment 或 feedback |
| src/NSSLC/Component/WorldSession/WorldGeneration/WorldLayerMetricsComponent.cs 与 Systems/WorldLayerMetricsSystem.cs | 已有层级 metrics 能力；CountTiles cursor/window 需单独核对，不假定同一 invariant |
| src/NSSLC/Component/WorldSession/WorldGeneration/WorldTileMetricsComponent.cs 与 Systems/WorldTileMetricsSystem.cs | 已有 component 内 cadence/cursor、alignment accumulator/publication 与显式 sample/effect ports 驱动的 `ScanColumnTiles` 算法；未接入真实 Tile adapter、Main cadence、NetMessage 或 Skyblock |
| src/NSSLC/Component/WorldSession/WorldGeneration/Adapters/IWorldTileMetricsTileSource.cs | 新增 `ReadOrCreateTile(x,y)` 候选输入端口；定义系统所需 read-or-create 契约，不提供真实 grid lookup/create 实现 |
| E3 Tile/session 接线前置项 | CPG 选定 `Main.cs` shard 有 2 个 `UpdateWorld` 直接调用点，选定 `WorldGen.cs` shard 有 1 个 `CountTiles` 调用点，均 complete、无 gap；NSSLC `TileMapStore` 没有坐标读写 API，`TileCellState` 没有显式 active 状态；`IWorldSkyblockGenerationGridReader` 无实现且没有 create contract；`WorldSessionComponents` 无 session identity，现有 lifecycle 只有 `GenerationId` | 在 world grid 的权威 owner、Tile active 映射和 session 生命周期明确前，不创建猜测性 adapter，不连接 Main cadence，不新增 reset 语义 |
| src/NSSLC/Component/WorldSession/WorldGeneration/Terrain/TreeTops/WorldTreeTopsStateComponent.cs、AreaQuery.cs 与 System.cs | 已实现 13-area holder、区域判定和随机样式变更；没有旧入口接线 |
| src/NSSLC/Component/WorldSession/WorldGeneration/Terrain/TreeTops/WorldTreeTopsPersistenceAdapter.cs | 实现 Version4 可见 Save/Load 编码，尚未接入 WorldFile |
| src/NSSLC/Component/WorldSession/WorldGeneration/Terrain/TreeTops/WorldTreeTopsNetworkProjection.cs | 生成 13-byte payload，尚未接入 NetMessage 或 receiver |
| src/NSSLC/Component/WorldSession/WorldGeneration/WorldTileMetricsTileSample.cs | 新增不可变 Tile sample 值类型，不保存 Tile/session 权威状态 |
| src/NSSLC/Component/WorldSession/WorldGeneration/WorldTransformTransactionComponent.cs 与 Systems/WorldTransformTransactionSystem.cs | 新增并发安全的 transaction 计数/revision System 核心；未接入 Task、IOLock、save wait 或 follow-up queue |
| Test/Terraria.WorldSession.P16TransformBarrier.Verification/ | 两项局部核心场景：计数/underflow 与并发 begin-complete；仅验证候选状态模型 |
| Test/Terraria.WorldSession.P16TileMetrics.Verification/ | 5 个局部核心场景通过，产物在 Build/bin/ |
| src/NSSLC/Component/WorldSession/WorldGeneration/Systems/ | 生命周期、housing 与 registry 候选 System 落点；E0 先确认项目及加载器组织 |
| NSSLC 中现有 persistence、network、NPC、Main adapter/registration 项目 | 具体文件和依赖方向 unknown；通过目标源码与项目引用检索确定后再列为实施文件 |
| Test/Terraria.WorldSession.P16TreeTops.Verification/ | 新增独立七项核心 verifier；未恢复或改写已删除的旧 WorldGeneration 验证项目 |
| Build/bin/Terraria.WorldSession.P16TreeTops.Verification/Debug/net10.0/ | 串行 runner 生成的验证产物；位于约定输出目录 |

只读证据位置包括 Version4 的 Terraria/WorldGen.cs、Terraria/Main.cs、Terraria/NPC.cs、Terraria/NetMessage.cs、Terraria.IO/WorldFile.cs、Terraria.GameContent/TownRoomManager.cs、Terraria.GameContent/TreeTopsInfo.cs，以及完整参考项目对应文件。

## 5. 阻塞与停止条件

验证状态覆盖（2026-10-01）：TreeTops 7 项、tile metrics 5 项、transform barrier 2 项、load recovery 3 项、housing registry 3 项、housing validation 3 项、housing score 5 项均有局部结果。其余行为测试、真实旧入口接线、网络/Tile adapter、住房 occupancy/assignment 闭环和 P16 全量矩阵仍为 not-run。

以下任一条件未解决时，只允许独立、无 legacy caller 切换的候选实现：

| 阻塞项 | 相关阶段 | 解除所需证据 |
|---|---|---|
| world/session authoritative identity unknown | E1–E5 | 明确一个 session 标识和多世界/reload 作用域；`GenerationId` 与 `PersistentWorldId` 均未证明覆盖该生命周期 |
| process-wide transform owner registration unknown | E1、E5 | 证明单个 `WorldTransformTransactionComponent` 被 transform workers 与所有 save barriers 共享；保持与 Version4 静态 `_transformingWorld` 同范围 |
| Tile grid adapter contract unknown | E3、E5 | 定义按坐标读取/创建 tile 的 owner/API，确认 `TileCellComponent.IsActive` 与目标 `Tile.active()` 的映射，并确定 wall/tile/active-count effects 的真实 owner |
| load failure/cancel/retry/reset 部分闭包 unknown | E1、E5 | 保留源码已确认的两轮 retry 与 `.bak` 恢复次序；仍需确定异常后 load gate、旧 world 保留、取消、unload/reset 的唯一终态 |
| housing resident key/双写顺序 unknown | E2、E5 | NPC 与 Registry 单写 owner、旧 key/schema 映射和 assignment 顺序 |
| host update、worker、lock、queue 和 RNG 顺序 unknown | E1、E3、E5 | 精确 caller 与调度合同，含两个 Main 入口 |
| Skyblock、message 57 owner/consumer unknown | E3、E5 | Skyblock flags/Calculate effects 的权威写责任、网络方向/schema/可见时点 |
| TreeTops 接收/FX、动态 style writer 与运行时 owner unknown | E4、E5 | 已知的直接静态调用点不能覆盖动态/反射入口；仍需接收端/FX owner、WorldFile/NetMessage 新旧组合和实际运行时注册证据；`_variations` 成员使用 facts 为 partial |
| 混合事件/天气/Tile/scratch owner unknown | E0、E5 | integration-review 逐 seam 记录最终 owner |

遇到不兼容行为时记录为明确行为变化候选，说明影响与批准人；不得以“清理 API”或参考项目实现掩盖改变。

## 6. 完成标准

- 每个已实现行为有一个权威 state owner 和可追溯 legacy entry composition。
- 失败、取消、重试、reset、session 隔离及存档/网络边界具备明确合同。
- Main/NPC/WorldFile 实际旧入口到新 API 的闭环有源代码与运行/行为证据。
- 必需行为验收通过，未覆盖项与残余 unknown 显式列出。
- 未通过必要行为验证前，P16 仍标 proposed / not-run，不切换旧路径、不删旧实现、不称迁移成功。

当前执行状态为 partial：TreeTops state/area/randomization/codec/projection 与七项核心 verifier、tile metrics cadence/alignment/publication/列扫描算法及五项局部核心 verifier、transform transaction 计数核心及两项局部 verifier、住房评分候选与五项 focused verifier 均有记录。E1 生命周期与 load gate 闭环、E2 housing、真实 CountTiles Tile/effect adapters、cadence 到 Main 的接线、跨 session reset、TreeTops 接收/FX、E5 宿主组合以及 E6 全量行为验收尚未完成。P16 继续保持设计 proposed / 全量验证 not-run；本计划不能作为旧路径已切换或迁移成功的证明。
