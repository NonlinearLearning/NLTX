# P14 NPC 生成 System 执行说明

```yaml
documentType: system-execution
partitionId: P14
sessionId: cae65855b1f9451ba3241f48fe20d1c8
sourceReport: ../reports/2026-09-18-system-decomposition-authoritative-P14-npc-spawn-eligibility.md
designDocument: 2026-09-30-P14-npc-spawn-system-design.md
executionStatus: proposed
implementationStatus: partial
verificationStatus: not-run
focusedCoreHarnessStatus: limited-passed
latestSlotBoundaryStatus: limited-passed
latestSlotSentinelMappingStatus: limited-passed
latestNpcSlotSelectionProtectionStatus: limited-passed
latestNpcSpawnSlotAcquisitionStatus: limited-passed
latestNpcSpawnSlotGenerationGuardStatus: limited-passed
latestNpcSpawnPreCommitStatus: compiled-only
latestSelectedSlotStorageStatus: focused-passed
latestTargetSelectionQueryStatus: limited-passed
latestNpcTypeResolutionStatus: limited-passed
latestNaturalSpawnOrderStatus: limited-passed
latestNpcSpawnTowerSelectionStatus: focused-passed
latestNpcSpawnSkyMobSelectionStatus: limited-passed
latestNpcSpawnInvasionSelectionStatus: limited-passed
latestNpcSpawnGraveyardDualDungeonSelectionStatus: limited-passed
latestNpcSpawnCritterSelectionStatus: compiled-only
latestNetworkPacketApiStatus: declaration-compiled-only
latestTileSpaceQueryStatus: limited-passed
historicalFocusedCoreHarnessStatus: historical-partial
```

## 1. 范围与执行状态

本文件把 P14 proposed 设计落实为分阶段迁移顺序与 gate，并记录当前 partial 实现；它不是
实现完成证明。当前已在 `src/NSSLC/Component/Npc` 增加玩家资格 Query、接收 rate snapshot 与
显式随机端口的 rate System、隔离的自然生成逐槽协调器、per-player spawn-flag preparation port、
纯 area Query、有序 tile-search System、screen exclusion Query、chosen-tile post-check System 和相应 port contract。pass 在 tile search
成功后依序执行 screen gate、post-check 与 chosen-tile flags System，将已捕获的 rate、tile-search、
post-check 和 chosen-tile flags 值封装成
`NpcSpawnAcceptedCandidate`，再调用 `void ContinueSpawnAttempt`；handoff 调用后结束当前自然
pass 的玩家循环，不把它的调用当作创建成功。塔区、SkyMob、入侵、graveyard/dual-dungeon 与 type 244
critter 均有独立的候选分支 selector；它们只返回 pre-commit request 或显式状态，type 624 的 timeLeft
倍率仅作为后置效果意图。port 尚无生产 adapter 或 caller；
chosen-tile flags 仍只有显式 port 驱动的隔离 core，没有运行时 adapter；NPC 创建提交、
target/network/persistence 也没有接线。当前源码状态
不代表迁移成功。

本次继续补入 `NpcSpawnSlotSelectionQuery`：仅对调用方传入的 NPC slot facts 与类型元数据做纯选择，
保持先找未使用槽、再找可替换槽的两遍顺序，并返回 replacement-fallback 标记或无槽结果。
它没有连接 `NPC.NewNPC`，不写 slot protection，不创建或初始化实体；当前可称为选择 core `partial`，
真实数据捕获、slot owner 和提交链仍为 `unknown`。GoodWorld type remap 已由独立的
`NpcSpawnTypeResolutionSystem` 表达：GoodWorld 下恰好消费一次 `Next(3)`，包括 type 不匹配 46/62 的路径；
非零 roll 将 46 替换为 614、62 替换为 66。该 System 不执行 `NPCID.FromNetId` metadata 选择或后续
slot/commit 操作，也尚未接入 `NPC.NewNPC` caller。

本轮新增 `INpcSpawnEntryPort` 并扩展 `NpcSpawnSystem.ProcessEntry`，将 Version4 静态入口中
`noSpawnCycle` 清除、respawn 检查和自然 pass 调用顺序表达为隔离协调；focused verifier 增加相应
短路与顺序断言。本轮又增加 `ProcessEntryDetailed` / `ProcessNaturalSpawnPassDetailed`，以及
`NpcSpawnEntryResult` / `NpcSpawnPassResult`；这些结果只记录端口可观察的入口顺序和 legacy
loop-control，并以 `ContinuationWasInvoked` 记录候选 handoff 是否发生；accepted candidate 的实体创建观察仍标为 `unknown`。它们没有生产 adapter、旧入口 facade 或 runtime caller。迁移级行为/运行时验收仍为
`not-run`。下文先保留早期工作树的历史验证记录，再记录当前工作树结果。当前局部通过只证明
explicit snapshot 和 fake port 下的核心分支，不证明真实 adapter、runtime caller、spawn commit、
网络或存档行为等价。

### 1.1 已有的局部核心验证记录（历史证据）

此前实现批次使用 .NET SDK `10.0.400`，经 `Build/Tools/Invoke-SerialDotnet.ps1` 串行构建并运行单个
核心 verification 项目。最终构建退出码为 0，0 warning、0 error；验证退出码为 0，8 组
核心断言通过。构建产物位于 `Build/bin/Terraria.Npc.SpawnEligibility.Verification/Release/net10.0/`。

```powershell
$env:PATH = 'D:\TRbackup\dotnet-sdk-10.0.400;' + $env:PATH
$dotnetArguments = @(
  'build',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '-c', 'Release', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false',
  '--nologo', '-v:minimal'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments

$dotnetArguments = @(
  'run', '--project',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '-c', 'Release', '--no-build', '--no-restore', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false',
  '--no-launch-profile'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
```

八组通过内容为 eligibility gate、rate 默认值/Hardmode/零附近 NPC 倍率/town-NPC 随机次序、
area zoom/Dual Dungeons/clamp、tile search 的 x/y 与 tile 读取顺序/near-sky 随机/sticky `SkyMob`/50 次拒绝上限、
capacity gate 顺序、玩家槽顺序与首次 true 退出、全部 255 槽均 eligible 且逐槽完成 search，
以及 slime-rain 状态逐玩家重新读取。该结果为历史工作树的验证记录，不证明当前工作树或新增
screen/post-check 检查通过。首次 verifier run 因 fake 玩家 Y 坐标使 spawn footprint 越过世界边界而未命中 continuation；将 fixture 移入世界有效区后重建并重跑通过。Version4 的静态默认值为 rate `600`、cap `5`；测试 fixture
使用 `100/5` 便于核对整数倍率。第一轮 rate 基线断言暴露 fixture 的
`NearbyActiveNpcSlots=0` 会按旧规则将 rate 乘以 `0.6`；之后把默认倍率场景与零槽位场景分开，
并分别验证。最终通过结果只覆盖显式输入和注入测试 port，不覆盖真实 Terraria 输入 adapter、
Main/NPC 运行时入口、spawn commit 或网络/存档观察；完整行为差分及 runtime verifier 仍为
`not-run`；这些历史局部断言只提供有限的 core 证据，不改变当前工作树 focused harness 与迁移级
`verificationStatus: not-run`。

### 1.2 当前工作树的有限核心验证

本轮只构建并验证 `Terraria.Npc.SpawnEligibility.Verification` 一个项目，没有运行其它 P14 verifier
或项目级测试。默认 PATH 指向的 host 没有发现仓库要求的 SDK；检查发现 `10.0.400` 已安装在
`C:\Users\shan\.dotnet`。将该本地 host 放到本次 PowerShell 进程 PATH 前方并设置 `DOTNET_ROOT`，
保持仓库 `global.json` 不变，通过 serial runner 构建：

```powershell
$dotnetRootForNpcSpawn = 'C:\Users\shan\.dotnet'
$env:DOTNET_ROOT = $dotnetRootForNpcSpawn
$env:PATH = $dotnetRootForNpcSpawn + ';' + $env:PATH
$dotnetArgumentsForNpcSpawn = @(
  'build',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgumentsForNpcSpawn
```

构建使用 SDK `10.0.400`，退出码 `0`，`0` warning、`0` error；输出位于
`Build/bin/Terraria.Npc.SpawnEligibility.Verification/Debug/net10.0/`，包含验证器及其项目依赖。
确认产物位于仓库规定的 `Build/bin/`。随后经相同 serial runner 只运行候选/入口 focused 分支：

```powershell
$dotnetArgumentsForNpcSpawn = @(
  'run', '--project',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '--no-build', '--no-restore', '--', '--spawn-candidate-checks-only'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgumentsForNpcSpawn
```

focused run 退出码 `0`，stdout 为 `PASS: NPC spawn screen and chosen-tile candidate checks`。
该分支检查屏幕排除与 post-check 的短路，也检查 `NpcSpawnSystem.ProcessEntry` 在 no-spawn-cycle 时清除
标记并跳过后续工作，以及标记未设置时按 `consume -> respawns -> capture:0` 顺序进入自然 pass；
同一 focused 分支还断言 `ProcessEntryDetailed` 返回 respawn 已执行、
`ContinuationWasInvoked == true` 且 `CreationObservation == Unknown`；并确认 continuation 恰好收到一个
首个合格候选，其中 player index、rate inputs、tile-search result、post-check facts 与 chosen-tile flags
均来自本次 attempt 的捕获/计算值；非 sky 候选还验证 `NoGroundWorms` 会映射为 `NoWormsForSpawn`。首次该断言运行时，
fixture 的 tile 落在 world-surface 上方，实际被旧 sky-mob gate 选为 sky candidate；将 fixture 调整到
非 sky 区并提供下一格 solid ground 后，focused run 退出码 `0` 并输出上述 PASS 行。该结果只证明
recording fake 下的局部契约。
这些断言使用 recording fake；没有真实 world/NPC adapter、legacy facade、runtime 注册或 caller，
也不覆盖 spawn commit、网络或存档观察。因此 migration-level `verificationStatus` 保持 `not-run`。

### 1.3 本次 chosen-tile flags core 验证（limited）

本次只构建 `Terraria.Npc.SpawnEligibility.Verification` 项目并运行其 focused 分支。现有 Marble
chosen-tile 场景检查 liquid/Marble/区域 flags 进入 candidate 且 Marble 快路没有额外 chosen-tile RNG
调用；新增普通 tile 场景检查 chosen tile 与 player 两段 near-Marble/Granite 扫描的随机范围顺序，
共 66 次有界随机请求。它使用 recording fake，不证明真实 Tile/random adapter 的来源绑定、完整
`SpawnAnNPC` 分支或实体创建结果。

仓库 `global.json` 指定 SDK `10.0.400`。系统级 `dotnet.exe` 未提供该 SDK；本次将用户级
`C:\Users\shan\.dotnet` 仅置于该命令进程的 `PATH` 前端，使用仓库串行 wrapper，未改动 SDK 配置。
构建退出码 `0`、0 warning、0 error；产物位于
`Build/bin/Terraria.Npc.SpawnEligibility.Verification/Release/net10.0/`，NPC library 同样输出于
`Build/bin/Terraria.Npc/Release/net10.0/`。

```powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
$dotnetArgumentsForNpcSpawn = @(
  'build',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '-c', 'Release', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgumentsForNpcSpawn
```

Focused verification 精确命令如下，退出码 `0`，stdout 为
`PASS: NPC spawn screen and chosen-tile candidate checks`。只运行了该 focused 分支；
完整 verifier、真实入口、Version4 runtime 差分、网络与存档验证均未运行。

```powershell
$dotnetArgumentsForNpcSpawn = @(
  'run', '--project',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '--no-build', '--no-restore', '--', '--spawn-candidate-checks-only'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgumentsForNpcSpawn
```

### 1.4 slot-selection Query 核心状态（先前验证与后续边界）

focused verifier 分支现有六组边界断言：空闲槽优先于更早的可替换槽；受保护的 inactive
槽视为 in-use 并只在替换回退阶段可选；反向遍历按 Version4 的交换与排除终点语义选择；
`CannotSpawnInSlot0` 且起点为零时从槽 1 开始；所有槽均 in-use 且不可替换时返回无槽。
正向起点恰为 capacity 时也必须返回无槽。前五组在此前 focused run 中通过；截至本节原始记录时，
第六组是其后的代码与 verifier 断言改动，尚未重新构建或运行。后续 focused run 对六组均已通过，
见第 1.5 节。所有断言只使用显式 facts，
不验证真实 `NPC[]`/`spawnSlotProtected` 快照的一致性，也不运行创建或替换副作用。

本次 API 证据来自只读 CPG SQLite manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`：
`GetAvailableNPCSlot` 精确符号查找 `complete`，选定 `Terraria/NPC.cs` callable facts 为 `partial`（4 CFG、58 operation、
0 个直接 CallTargets，gap `CalleeEffectsNotExpanded`），该源码文件内直接调用点查询 `complete` 且为 1 个；
`IsSpawnSlotInUse` 符号查找 `complete`，callable facts 为 `partial`（3 CFG、9 operation、0 个直接 CallTargets，
同一 gap），选定文件内直接调用点查询 `complete` 且为 1 个。CPG 不含 callee effect 闭包；因此按源码复核：
Version4 `Terraria/NPC.cs:67066` 调用选择器，`67098-67143` 实现 netId 归一化、reverse/slot-0 元数据、
两遍循环及 `active || spawnSlotProtected > 0` 判定；完整参考 `Terraria/NPC.cs:82032` 与 `82067-82112`
对应逻辑一致。完整参考没有绑定上述 CPG snapshot。Version4 与完整参考 `NewNPC` 的同步标记条件仍不同，
不能据此把整个 commit 行为合并为等价。正向 `startIndex == Main.maxNPCs` 时 Version4 循环不进入并返回 `-1`；
Query 已保留这一结果。Version4 `NewNPC` 在调用 `GetAvailableNPCSlot` 前还会条件性消费
GoodWorld 随机并改写 NPC type；slot helper 内先执行 `NPCID.FromNetId`，再读取 reverse/slot-0 metadata。
`NpcSpawnSlotAcquisitionSystem.Acquire` 现已将该次序组合为 isolated core，并在 facts capture 后交给
selection/protection System；当前 port 仍是 verifier fake，真实 `NPCID`/NPC slot adapter 未实现。
`NpcSpawnSlotLegacyResultAdapter` 已实现纯结果转换：Query 无槽映射到旧 `-1` helper sentinel，再将负
slot 映射到 `Main.maxNPCs`；有效 slot 原样保留，超 capacity 的正索引拒绝。该转换由 focused verifier
覆盖；真实 `NPC[]`/`spawnSlotProtected` facts capture、生产 GoodWorld RNG/`FromNetId` adapter 和 NewNPC
commit 仍为 `unknown`。

此前验证只构建受影响的 `Terraria.Npc.SpawnEligibility.Verification` 项目，SDK `10.0.400` 由
`C:\Users\shan\.dotnet` 提供，并经 serial runner 串行执行。该构建发生在第六组断言加入之前，退出码
`0`，0 warning、0 error；verifier 和 NPC library 输出位于 `Build/bin/Terraria.Npc.SpawnEligibility.Verification/Release/net10.0/`
与 `Build/bin/Terraria.Npc/Release/net10.0/`。

```powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
$env:DOTNET_ROOT = 'C:\Users\shan\.dotnet'
$dotnetArgumentsForNpcSpawn = @(
  'build',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '-c', 'Release', '--no-restore', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false', '--nologo', '-v:minimal'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgumentsForNpcSpawn
```

当时随后只运行 candidate focused 分支，精确命令如下：

```powershell
$dotnetArgumentsForNpcSpawn = @(
  'run', '--project',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '-c', 'Release', '--no-build', '--no-restore', '--', '--spawn-candidate-checks-only'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgumentsForNpcSpawn
```

此前 focused run 退出码 `0`，stdout 为
`PASS: NPC spawn slot selection core cases` 与 `PASS: NPC spawn screen and chosen-tile candidate checks`。
当时 slot 检查覆盖前五组；第六组正向 `startIndex == slotCount` 断言尚未加入。screen/post-check、captured
candidate 和 chosen-tile flags fake-port 检查随后通过。

一次误带 MSBuild switches 的 `dotnet run` 没有命中 focused 参数门，提前在完整程序的玩家顺序断言处退出；
输出指出期望调用序列漏了现有的 `chosen-world:116:48:0` capture，本次已更新该序列，但没有重跑完整 verifier。
正确的 focused run 另发现 candidate fixture 把 Marble/液体放在相对最终选中 `y=54` 偏移一格的位置；已把 fake facts
对齐到 Marble `y=54`、液体 `y=53/52`，最终 focused run 通过。故这里只记有限 core 通过，完整 verifier、真实
adapter/commit、Version4 runtime 行为差分、网络和存档验证仍为 `not-run`；P14 `verificationStatus` 保持 `not-run`。

该次文档补证只使用只读 CPG Query API 与源码读取，没有构建或运行测试；当时第六组边界仍为
`not-run`。之后的 focused 验证记录见第 1.5 节。

### 1.5 target-selection Query core

通过只读 CPG API 在 Version4 manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`
查询 `defaultTarget`：精确 field symbol 查询为 `complete`；限定 `Terraria/NPC.cs` 的 member-use 查询也为
`complete`，但只返回一条 `Write`（ZoneGraveyard 分支赋值），没有闭合它的全部读取关系。Version4 源码补足了
`Spawner.SpawnNPC` 的规则：仅当请求 `Target == 255` 时读取并替换为 `defaultTarget`（`NPC.cs:5167-5171`）；
ZoneGraveyard 分支写入当前 `target`（`NPC.cs:4415-4418`）。完整参考项目对应代码同序（`NPC.cs:4433-4436`、
`5185-5189`），本次只用作源码对照，行为基线仍是 Version4。

据此增加 `NpcSpawnTargetSelectionQuery.Select`，输入为显式请求值和 attempt-local
`NpcSpawnTargetSelectionSnapshot`，返回原始 target index；`255` 选择 captured default，其它值保持原值。
它不写 `NpcTargetComponent`，不做 index/entity identity 转换，也不实现数据包逻辑。defaultTarget 的生产 capture、
ZoneGraveyard writer 的单一 owner、target mutation、网络更新与 runtime caller 仍为 `unknown`。
focused verifier 覆盖默认值、显式值优先和初始 `255` sentinel。只构建受影响的
`Terraria.Npc.SpawnEligibility.Verification` 项目，SDK `10.0.400`，经仓库 serial runner 执行：

```powershell
$env:DOTNET_ROOT = 'C:\Users\shan\.dotnet'
$env:PATH = $env:DOTNET_ROOT + ';' + $env:PATH
$dotnetArguments = @(
  'build',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '-c', 'Release', '--no-restore', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false', '--nologo', '-v:minimal'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
```

构建退出码 `0`，0 warning、0 error；确认验证器产物位于
`Build/bin/Terraria.Npc.SpawnEligibility.Verification/Release/net10.0/`。随后只运行 candidate focused 分支：

```powershell
$dotnetArguments = @(
  'run', '--project',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '-c', 'Release', '--no-build', '--no-restore', '--', '--spawn-candidate-checks-only'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
```

focused run 退出码 `0`，stdout 为 `PASS: NPC spawn slot selection core cases` 与
`PASS: NPC spawn screen and chosen-tile candidate checks`。target-selection 三条断言包含在第二组中。
这里只证明受影响项目和 focused fake-input core；生产 capture/caller、target writer、entity creation、
network/persistence 和 Version4 runtime 行为差分没有运行，P14 迁移级 `verificationStatus` 保持 `not-run`。
后续如需数据包 API，仅增加函数签名和请求/结果契约，不实现编解码、发送或广播逻辑。

### 1.6 NPC sync packet API declaration

同一只读 CPG manifest 对 `SyncNewlySpawnedNPCs:void()` 的精确符号查询为 `complete`；限定
`Terraria/NPC.cs`、`Main.cs`、`MessageBuffer.cs` 的 call-site 查询为 `complete`，返回 `NPC.cs` 中一处。
callable facts 为 `partial`（4 CFG 节点，gap `CalleeEffectsNotExpanded`，无直接调用目标），不能从中推导
调用闭包。`spawnNeedsSyncing` 的 member-use 查询为 `complete`，返回 3 个选定事实（`Write`、`Unknown`、
`Write`）；CPG source snapshot 未绑定当前源码，读取方向与 writer 仍以源码核对为准。

2026-10-01 扩大到 `Terraria/NPC.cs`、`Main.cs`、`MessageBuffer.cs`、`NetMessage.cs` 的同 manifest 复查中，
`SyncNewlySpawnedNPCs` 仍返回 1 个静态 call site（`NPC.cs`，`complete`），其 callable facts 为 `partial`，
gap 仍为 `CalleeEffectsNotExpanded`；`spawnNeedsSyncing` member-use 查询返回 7 条选定事实，状态 `complete`。
这是不同 source-path 范围下的索引结果，不闭合调用、别名或唯一 writer；`SourceSnapshotId` 仍为空，所有源码
语义继续以 Version4 和完整参考的直接源码读取为准。

Version4 `NPC.cs:5185-5197` 扫描 `Main.npc`，对 `active && spawnNeedsSyncing` 调用
`NetMessage.SendData(23, -1, -1, null, i)`；自然生成路径在 `SpawnAnNPC` 后无条件调用 sync helper
（`NPC.cs:251-253`）。`spawnNeedsSyncing` 在 `NPC.NewNPC` 中写 `true`（`NPC.cs:67088`），重置时写 `false`
（`NPC.cs:8182`）。完整参考的 sync helper 仍发送 type 23，但自然生成路径仅在 `Main.netMode == 2` 时调用
（`NPC.cs:251-255`），其 `NewNPC` 也只在 server 模式置 `spawnNeedsSyncing`（`NPC.cs:82054-82057`）。
因此保留 Version4 作为行为基线，net mode 与唯一 writer 裁定仍由 `P14-DEC-06` 处理。

`INpcSpawnSyncPacketPort.SendNpcSyncPacket(int npcLegacySlot)` 仅声明一个 packet-facing 操作；没有
packet codec、transport adapter、caller 或 `NpcSpawnSystem` 接线。`INpcReplicationPacketApi` 中的
Packet 23/28 decode/encode、recipient selection 与 publish 只保留函数声明；该 API 的请求/结果类型为空声明，
不添加编解码、发送、recipient selection 或 publish 逻辑。
它的编译由本轮受影响项目覆盖；focused
harness 不声称验证网络行为。接口加入后重复执行第 1.5 节所列的串行 Release build，退出码 `0`、
0 warning、0 error；验证器 DLL 确认为 `Build/bin/Terraria.Npc.SpawnEligibility.Verification/Release/net10.0/`
下的产物。随后重复第 1.5 节所列的 `--spawn-candidate-checks-only` 命令，退出码 `0`，输出仍为
`PASS: NPC spawn slot selection core cases` 与 `PASS: NPC spawn screen and chosen-tile candidate checks`。
该 focused harness 未验证 packet 编码、发送、server/net mode gate 或 section recipient 行为。
后续数据包相关 API 同样只允许增加函数签名/契约，不添加具体逻辑。

### 1.7 Tile-space Query focused rerun

本次复用第 1.2 节构建的 `Debug/net10.0` verifier 产物，通过仓库串行 runner 运行唯一的候选 focused
分支，使用 `--no-build --no-restore`；本次没有触发编译或 restore。执行前确认 checkout 中没有活动的
`dotnet`/`csc` build 进程。命令如下：

```powershell
$dotnetRootForNpcSpawn = 'C:\Users\shan\.dotnet'
$env:DOTNET_ROOT = $dotnetRootForNpcSpawn
$env:PATH = $dotnetRootForNpcSpawn + ';' + $env:PATH
$dotnetArgumentsForNpcSpawn = @(
  'run', '--project',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '--no-build', '--no-restore', '--', '--spawn-candidate-checks-only'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgumentsForNpcSpawn
```

退出码为 `0`，输出为：

```text
PASS: NPC spawn tile-space solid and lava rules
PASS: NPC spawn slot selection core cases
PASS: NPC spawn screen and chosen-tile candidate checks
```

tile-space Query 对 active/solid/lava 的 8 种组合断言通过；该 focused 分支也执行 slot-selection 与
candidate 检查。此结果只覆盖 verifier 的显式输入和 recording fake，不验证生产 tile facts adapter、
runtime caller、NPC commit、网络传输或 Version4 行为差分。tile/liquid facts 的实际 capture 仍为
`unknown`，整体 P14 `verificationStatus` 继续为 `not-run`，不构成迁移成功结论。

### 1.8 Slot sentinel compatibility mapping

本轮新增 `NpcSpawnSlotLegacyResultAdapter`，仅做源端返回值转换：`NpcSpawnSlotSelectionResult.SlotIndex`
为空时先映射为 `GetAvailableNPCSlot` 的 `-1`；`NewNPC` adapter 再把负值映射为 `maxNpcSlots`，
有效索引原样返回。它不读取或修改 NPC slot 状态，也不代表 NewNPC 创建提交或 caller 已接线。

只构建受影响的 `Terraria.Npc.SpawnEligibility.Verification` 项目，使用 SDK `10.0.400`、仓库串行
runner 和 Debug 配置。命令、退出码与产物如下：

```powershell
$dotnetRootForP14 = 'C:\Users\shan\.dotnet'
$env:DOTNET_ROOT = $dotnetRootForP14
$env:PATH = $dotnetRootForP14 + ';' + $env:PATH
$dotnetArgumentsForP14 = @(
  'build',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '-c', 'Debug', '--no-restore', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgumentsForP14
```

退出码为 `0`，0 warning、0 error；产物位于
`Build/bin/Terraria.Npc.SpawnEligibility.Verification/Debug/net10.0/`。随后仅运行既有的
`--spawn-candidate-checks-only` 分支（`--no-build --no-restore`），退出码为 `0`，输出与第 1.7 节相同的
3 项 PASS；slot-selection 组现同时断言无槽 `null -> -1 -> 255` 与有效索引原样通过。
这仍是有限的 fake-input core 验证，P14 `verificationStatus` 保持 `not-run`。

### 1.9 Per-player spawn flags 顺序（limited）

此前此节记录了 pass port 上的 `PreparePerPlayerSpawnFlags(playerIndex)` 顺序隔离。该 API 后由
`NpcSpawnPerPlayerFlagsSystem.Prepare` 取代；旧 focused 顺序记录只证明当时的 recording fake 调用序列，
不证明新的 flag capture、invasion RNG 或生产输入映射。当前实现和验证范围见 1.10。

先确认没有活动 `dotnet.exe`/`csc.exe` 编译进程，再通过仓库串行 runner 只构建受影响的 verifier 项目：

```powershell
$dotnetRootForP14 = 'C:\Users\shan\.dotnet'
$env:DOTNET_ROOT = $dotnetRootForP14
$env:PATH = $dotnetRootForP14 + ';' + $env:PATH
$dotnetArgumentsForP14 = @(
  'build',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '-c', 'Release', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgumentsForP14
```

构建退出码 `0`，0 warning、0 error；验证器产物位于
`Build/bin/Terraria.Npc.SpawnEligibility.Verification/Release/net10.0/`。随后复查无活动编译进程，
经同一串行 runner 仅运行 focused 分支（无 build/restore）：

```powershell
$dotnetArgumentsForP14 = @(
  'run', '--project',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '--no-build', '--no-restore', '--', '--spawn-candidate-checks-only'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgumentsForP14
```

运行退出码 `0`，输出 `PASS: NPC spawn tile-space solid and lava rules`、
`PASS: NPC spawn slot selection core cases`、`PASS: NPC spawn screen and chosen-tile candidate checks`。
本轮未运行完整 verifier、真实入口、Version4 runtime 差分、网络或存档用例；overall P14
`verificationStatus` 仍为 `not-run`，实现仍为 `partial`，不称迁移成功。

### 1.10 Per-player flags calculation（limited）

`NpcSpawnPerPlayerFlagsSystem.Prepare(playerIndex, INpcSpawnPerPlayerFlagsPort)` 已接入自然
spawn pass，调用位置在可选 slime-rain effect 之后、rate/capacity/rate-roll 之前。它按
prelude、invasion inputs、postlude、rate inputs 的顺序取值；纯计算入口 `Calculate` 将旧
`SetSpawnFlags` 会设置/重置的 attempt-local policy、spatial、biome/dungeon、biome-zone、
event/tower 字段折叠进 `NpcSpawnRateInputs`。现有快照中 `ZoneGranite`、`ZoneMarble` 和旧方法
不写入的其它值保留原值，不更新共享 legacy 字段。

Version4 `Terraria/NPC.cs:279-340` 与完整参考 `Terraria/NPC.cs:282-343` 的 `SetSpawnFlags`
主方法体一致：初始重置/读取、Hard Dungeon 与 dual-dungeon 判定、tower 对 invader/安全墙的
覆盖、player tile wall/AFK/remix/Tim/生命值派生、shadow candle 最后清除三类 flag。两边的
`ShouldSpawnInvasionEnemies` 条件与控制流也一致：先检查 invasion 活动条件；在地表/屏幕条件
满足时优先判断直接 invasion-X 范围，再按 NPC slot 顺序查 town NPC 和 3000 像素距离，只有匹配
slot 才消费 `Next(3)`，抽到 0 时 break，否则返回 true。CPG Query API 查询精确命中一个
`SetSpawnFlags(Player)` 声明及其在 `Terraria/NPC.cs` 的一个静态调用点；`SetSpawnFlags` 与
`ShouldSpawnInvasionEnemies` callable facts 均为 `partial/CalleeEffectsNotExpanded`，分别返回
5 与 3 个 CFG 节点，且都没有直接 call target。查询 manifest 为
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`；
因此方法调用及随机关系以源码核对为准，不把 CPG 结果提升成完整效果或运行时调用证明。完整参考的
dual-dungeon chosen-tile helper 额外写入 Zone flags，Version4 helper 为空，当前实现继续按
Version4 不导入该行为。

本次限量验证实际结果：

- 首次构建解析到 `C:\Program Files\dotnet` 的 SDK `10.0.100`，与 `global.json` 要求的
  `10.0.400` 不匹配而退出；调整当前 shell 的 `DOTNET_ROOT`/`PATH` 到用户级 SDK 后重试成功，
  未修改仓库 SDK 配置。

- 使用用户级 .NET SDK `10.0.400`，经 `Build/Tools/Invoke-SerialDotnet.ps1` 串行构建
  `Test/Terraria.Npc.SpawnEligibility.Verification/Terraria.Npc.SpawnEligibility.Verification.csproj`
  的 Release 配置，参数含 `--no-restore -m:1 -nr:false -p:UseSharedCompilation=false
  -p:MSBuildNodeReuse=false -p:BuildInParallel=false`。退出码 `0`，0 errors、14 warnings；
  warning 来自被引用的 `Terraria.WorldStorage` 未赋值/未使用字段。产物在
  `Build/bin/Terraria.Npc.SpawnEligibility.Verification/Release/net10.0/`。
- 通过同一 runner 以 `run --project ... -c Release --no-build --no-restore --
  --spawn-per-player-flags-checks-only` 运行限定子集，退出码 `0`，输出
  `PASS: NPC per-player spawn flags and invasion ordering`。这只验证 fake inputs 下的 flags
  计算与 invasion 分支，不证明生产 adapter、真实运行时接线或 Version4 行为等价。
- 第一次调用误将 `-c Release` 放在应用参数 `--` 之后，flags-only 参数未被识别，默认 verifier
  已运行至自然生成顺序断言并以非零退出。观察序列为
  `capture:0,capture:1,slime:1,rate:1,capture:2,slime:2,rate:2,...`，与当前断言预期的 flags
  prelude/invasion/postlude capture 顺序不符。核对确认 `-c Release` 被转发给应用后，`dotnet run`
  默认选择 Debug；Debug 下 verifier 与 `Terraria.Npc.dll` 的产物时间为 15:07，NPC coordinator IL
  没有 `NpcSpawnPerPlayerFlagsSystem.Prepare` 调用。Release verifier 与 NPC DLL 时间为 17:01，哈希
  一致，Release coordinator IL 包含该调用；以 Release 程序集执行同一 recording 场景时观察到完整
  flags capture 顺序。故这次非零结果来自旧 Debug 输出，不能作为当前 Release 源码的反证，也不算
  验证通过；完整 Debug/Release verifier 均未运行完成，P14 验证仍为 `not-run`。

`INpcSpawnPerPlayerFlagsPort` 的生产 Player/World/Tile/RNG capture adapter 与旧入口 caller 仍缺失；
packet 相关 API 继续只有函数声明，不实现编解码、recipient selection、publish/send。完整 P14
`implementationStatus` 保持 `partial`，`verificationStatus` 保持 `not-run`。

执行目标是在 `D:\TRbackup\NLTX\src\NSSLC` 中逐步建立与 Version4 自然生成链路对应的
System 组合，并将已确认的特殊 spawn、NPC lifecycle、人口/slot、target、network 与 persistence
交给其唯一权威 owner。任何依赖未确定 owner 或不一致源码版本的批次必须先停在对应 gate；
不通过复制状态或增加第二 writer 绕开问题。

正式分区会话已经结算。此后本计划不再领取其它 P 分区，不手工修改 task ledger，也不把
文档新增当作新的 runner 分区任务。

### 1.11 GoodWorld NPC type resolution core

只读 CPG Query API 对精确 `NewNPC(IEntitySource,int,int,int,int,float,float,float,float,int)`
查询返回一个声明。对全部 967 个索引 shard 的 call-site 查询为 `complete`，命中 158 个静态点；其中
`Terraria/NPC.cs` 有 96 个。manifest 为
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`。
该调用点集合横跨自然生成、特殊入口、event、projectile 与 wiring 路径，只能确认所选索引中的静态
`CallTargets`，不闭合动态调用、运行时入口或创建 owner。

当前 `INpcDefinitionQuery` 同时声明 `TryGetByTypeId` 和 `TryGetByNetId`；按 type 查询由
`NpcDefinitionCatalog` 已有的 frozen dictionary 查找实现。它为最终 type 的 definition lookup 提供公开
只读契约，但当前 pre-commit 与自然生成 pass 尚未调用它。`NpcDefinition` 仅表达 identity、core stats、
movement、spawn、town、capability 和 presentation 数据。
回读 Version4 `NPC.cs:67051-67097` 与完整参考 `NPC.cs:82017+` 可见，slot 选中后先写 protection=2，再执行
对象替换、`ResetForNewNPC`、`SetDefaults`、town unique data、bottom 位置、active、
`timeLeft=(int)(750*1.25)=937`、条件 wet collision、四个 AI 值、target、同步意图和 Type 50 公告。
完整参考只在 server mode 设置同步意图，Version4 无条件设置。当前 definition schema 不覆盖完整 defaults、
wet/town unique data，也没有稳定 `NpcInstanceId` 的签发 owner；故本轮只公开 type lookup，不实现不完整的
hydrated entity 或 commit。
本地源码与 `Test` 中未找到 definition 实例或 NPC hydration system；`ContentCatalogSnapshot` 接收预建
catalog，不能排除外部装载定义，但没有源码关系把这些值转成完整 NPC 初始组件。Infrastructure 的
`NpcEntitySlotStore` 仅管理 runtime ID、slot、generation；`EntitySpawnCommitSystem` NPC 分支只分配首个
空闲槽，不接收 selected slot 或 NPC state。`EntityPoolInitializationSystem` 可构造这些 pool/store，但全仓未找到
该初始化器或 `EntitySpawnCommitSystem` 的运行时调用点；它们也不能闭合 NewNPC 的 replace、
activation、population、target 与 sync 关系。当前 `WorldStorageRoot.Npcs` 是另一种通用
`EntitySlotStore<WorldEntityState,NpcSlot>`，不应与 infrastructure store 合并成已确认 owner。

Version4 `Terraria/NPC.cs:67051-67067` 与完整参考 `Terraria/NPC.cs:82017-82033` 都在 slot lookup
前执行 GoodWorld type remap：GoodWorld 为真时先调用 `Main.rand.Next(3)`；非零结果将 type 46 替换为
614、type 62 替换为 66。随机调用先于类型匹配，所以其他 type 也会消费一次。`NpcSpawnTypeResolutionSystem`
通过显式 random port 保留该顺序并返回 resolved type 与 draw 观察；不读取全局 RNG、不负责 `FromNetId`
metadata、slot facts、实体创建或网络 effect。生产 adapter 与 caller/commit owner 仍为 `unknown`。

### 1.12 NPC slot protection command core

`NpcSpawnSlotProtectionSystem.AdvanceTick` 对 port 给出的 slots 按索引升序执行：先读 NPC active；
active 时直接写入 2，不读取旧 protection；inactive 时读取旧值并写入
`max(oldValue - 1, 0)`。`ProtectSelectedSlot` 只写 2，供 slot selection 成功后、NPC 替换/初始化前调用；
`ResetSlotProtection` 只写 0，供对应 NPC slot 初始化后调用。这三个方法通过
`INpcSpawnSlotProtectionPort` 操作外部权威状态，属于命令式写入，不是 Query。

`NpcSpawnSlotSelectionSystem.SelectAndProtect` 组合 `NpcSpawnSlotSelectionQuery.Select` 与选中槽
protection 写入。它要求调用方提供的 slot facts 长度与 protection port 容量一致；选中空闲槽或
replacement fallback 槽后，在把结果返回给调用方之前写入 protection=2；没有槽时不写入。该 System
消费调用方已捕获的 facts，不负责 `NPC[]` capture、槽位 owner、实体替换或初始化。

全索引 CPG 查询与完整源码位置见第 2.4 节：method 静态调用在 Version4 `Main.cs:11278`；field
member-use aggregate 为 `complete`，但每条 assignment 分类为 `Unknown`。Version4 和完整参考源码都显示
refresh 位于 `SpawnNPC()` 之前、`NewNPC` 在 slot 选中后先保护再替换/初始化，以及 WorldGen 新建各 slot
时清零。NLTX 尚无 Main/NewNPC/WorldGen production adapter、实际 owner 或 System 注册；本 core 不能单独
闭合自然生成 commit。

构建命令通过 `Build/Tools/Invoke-SerialDotnet.ps1` 串行执行，SDK 为 `10.0.400`：

```powershell
$dotnetRootForP14 = 'C:\Users\shan\.dotnet'
$env:DOTNET_ROOT = $dotnetRootForP14
$env:PATH = $dotnetRootForP14 + ';' + $env:PATH
$dotnetArgumentsForP14 = @(
  'build',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '-c', 'Release', '--no-restore', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false', '--nologo', '-v:minimal'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgumentsForP14
```

退出码 `0`，0 warning、0 error。产物位于
`Build/bin/Terraria.Npc.SpawnEligibility.Verification/Release/net10.0/`。

首次 focused 尝试由 serial runner 执行以下 `dotnet run` 参数：

```powershell
$dotnetArgumentsForP14 = @(
  'run', '--project',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '-c', 'Release', '--no-build', '--no-restore', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false', '--', '--spawn-type-resolution-checks-only'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgumentsForP14
```

该调用没有进入 filter 分支而运行默认 verifier，退出码 `1`；程序在 `Program.cs:394` 的自然生成顺序
断言退出。观察到 player 1 的 rate roll 为非零后跳过 area/screen/post，而旧 expectedCalls 仍期待这些
步骤。随后将该顺序场景抽到 `RunNaturalSpawnOrderingCases`，修正为 rate miss 后直接进入下一玩家的预期，
并增加 `--spawn-natural-order-checks-only` 入口；完整默认 verifier 本轮未重跑。

随后用 serial runner 直接执行已构建 Release DLL 的 focused 分支：

```powershell
$dotnetArgumentsForP14 = @(
  '.\Build\bin\Terraria.Npc.SpawnEligibility.Verification\Release\net10.0\Terraria.Npc.SpawnEligibility.Verification.dll',
  '--spawn-type-resolution-checks-only'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgumentsForP14
```

退出码 `0`，输出 `PASS: NPC spawn type resolution core cases`。覆盖 GoodWorld 开关、零/非零 roll、
46/62 两个映射和未映射类型仍消费随机的显式 fake-port 场景。

随后重新构建上述 Release 项目，退出码 `0`，0 warning、0 error。直接 DLL 调用
`--spawn-natural-order-checks-only`，退出码 `0`，输出
`PASS: NPC natural-spawn player ordering and first-true break`。该单场景验证非零 rate roll 后跳过候选尾段、
随后零 roll 候选继续到 continuation，以及第一个 true loop-control 后结束玩家循环；不证明真实输入 adapter、
运行时 caller、NPC commit 或 Version4 行为等价。整体 P14 `verificationStatus` 仍为 `not-run`。

本轮为 `NpcSpawnSlotProtectionSystem` 增加 `--spawn-slot-protection-checks-only`。在上述 Release 构建后，
使用 serial runner 直接执行 verifier DLL 与该参数，退出码 `0`，输出
`PASS: NPC slot protection ordering and lifecycle writes`。断言覆盖 active slot 刷新为 2 且不读取旧值、
inactive protection 递减并夹到 0、slot 升序读写，以及 selected-slot protect/reset 两个写值命令。

focused 调用命令为：

```powershell
$dotnetArgumentsForP14 = @(
  '.\Build\bin\Terraria.Npc.SpawnEligibility.Verification\Release\net10.0\Terraria.Npc.SpawnEligibility.Verification.dll',
  '--spawn-slot-protection-checks-only'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgumentsForP14
```

同一二进制曾经一次经 `dotnet run --no-build` 启动，但过滤参数未进入程序，默认 verifier 输出了默认用例组的
PASS（进程退出码 `0`）；这次运行不算 slot-protection focused 证据，且超出了本轮预定的 focused 范围。
随后改用直接 DLL 参数成功命中筛选分支；不再重复默认入口。迁移级 `verificationStatus` 仍为 `not-run`，
因为真实 scheduler、NPC commit、WorldGen adapter、网络与 Version4 行为差分均未验证。

### 1.13 Slot selection/protection composition

Version4 `NPC.NewNPC` 的静态顺序是先运行 `GetAvailableNPCSlot` 选择 slot，再在替换和初始化之前写入
`spawnSlotProtected[slot] = 2`；完整参考源码相同。现在该已确认的 core 关系由
`NpcSpawnSlotSelectionSystem.SelectAndProtect` 表达：先在传入的 `NpcSpawnSlotFact` snapshot 上调用纯
Query，再对选中的 legacy slot 调用 `NpcSpawnSlotProtectionSystem.ProtectSelectedSlot`。事实容量与
protection port 容量不一致时抛出参数异常，避免向另一套 slot domain 写入。System 不从共享状态捕获
facts，也不声称接入 NPC replacement/initialization 或生产 caller。

将组合断言加入既有 slot-protection focused 分支后，只构建受影响的 verifier 项目：

```powershell
$dotnetRootForP14 = 'C:\Users\shan\.dotnet'
$env:DOTNET_ROOT = $dotnetRootForP14
$env:PATH = $dotnetRootForP14 + ';' + $env:PATH
$dotnetArgumentsForP14 = @(
  'build',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '-c', 'Release', '--no-restore', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false', '--nologo', '-v:minimal'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgumentsForP14
```

退出码 `0`，0 warning、0 error；产物为
`Build/bin/Terraria.Npc.SpawnEligibility.Verification/Release/net10.0/Terraria.Npc.SpawnEligibility.Verification.dll`。
然后通过 serial runner 直接传入 DLL 和 focused 参数：

```powershell
$dotnetArgumentsForP14 = @(
  '.\Build\bin\Terraria.Npc.SpawnEligibility.Verification\Release\net10.0\Terraria.Npc.SpawnEligibility.Verification.dll',
  '--spawn-slot-protection-checks-only'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgumentsForP14
```

退出码 `0`，输出 `PASS: NPC slot protection ordering and lifecycle writes`。新增断言覆盖：
selection 进入 replacement fallback 后，在返回 selected slot 前只写该 slot 的 protection=2；所有槽均
active/protected 且不可替换时不产生 protection 写入；slot facts 与 protection port 容量不一致时在
任何写入前拒绝。此证据仅覆盖 core 组合和 fake port。权威 slot
owner、真实 facts capture、`NewNPC` replacement/init、scheduler/runtime registration、WorldGen/reset
接线、network 及 Version4 runtime differential 均未验证，P14 `verificationStatus` 继续为 `not-run`。

### 1.14 NewNPC slot acquisition prelude

Version4 `NPC.NewNPC` (`Terraria/NPC.cs:67051`) 在调用 `GetAvailableNPCSlot` 前执行 GoodWorld 条件
随机和 46/62 type remap；`GetAvailableNPCSlot` (`NPC.cs:67098`) 随后调用 `NPCID.FromNetId`、读取
reverse policy，仅在 `startIndex == 0` 时读取 slot-0 policy，之后扫描 slots。完整参考对应入口/辅助方法为
`NPC.cs:82017` 与 `NPC.cs:82067`，这一前置顺序一致。CPG 对这两个静态方法的 facts 与调用闭包限制见
第 1.4、2.2 节；未将完整参考视为 Version4 的索引来源。

`NpcSpawnSlotAcquisitionSystem.Acquire` 现在组合此分配前路径：`NpcSpawnTypeResolutionSystem.Resolve`
先按 GoodWorld 输入消费随机并生成 resolved type；port 再按序执行 `FromNetId`、reverse policy 读取、
条件性 slot-0 policy 读取、slot facts capture；最后调用 `NpcSpawnSlotSelectionSystem.SelectAndProtect`
选择并在结果返回前写 protection=2。Facts capture 是 effectful port 边界，不称作 Query。当前没有
生产 port 实现；NPCID metadata、Main RNG、真实 NPC array/protection capture、实际 commit/初始化仍未接通。

focused fake-port trace 输入 requested type 46、GoodWorld roll=1、reverse=true、slot-0 policy=true，断言
调用 trace 为：`random:3`、`from-net-id:614`、`reverse:614`、`slot-zero:614`、`capture-slot-facts`、
`write:2:2`；选择结果为 slot 2，且 type resolution 为 614。另以 nonzero start 和 GoodWorld=false 断言
trace 为 `from-net-id:20`、`reverse:20`、`capture-slot-facts`、`write:1:2`，证明该路径不消费随机且短路
slot-0 metadata 读取。验证只运行现有 `--spawn-slot-protection-checks-only` 分支。

受影响 verifier 的 Release 构建通过 `Build/Tools/Invoke-SerialDotnet.ps1` 执行，SDK `10.0.400`：

```powershell
$dotnetRootForP14 = 'C:\Users\shan\.dotnet'
$env:DOTNET_ROOT = $dotnetRootForP14
$env:PATH = $dotnetRootForP14 + ';' + $env:PATH
$dotnetArgumentsForP14 = @(
  'build',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '-c', 'Release', '--no-restore', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false', '--nologo', '-v:minimal'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgumentsForP14
```

退出码 `0`，0 warning、0 error；产物为
`Build/bin/Terraria.Npc.SpawnEligibility.Verification/Release/net10.0/Terraria.Npc.SpawnEligibility.Verification.dll`。

随后经同一 runner 直接运行 DLL 和 focused 参数：

```powershell
$dotnetRootForP14 = 'C:\Users\shan\.dotnet'
$env:DOTNET_ROOT = $dotnetRootForP14
$env:PATH = $dotnetRootForP14 + ';' + $env:PATH
$verificationDllForP14 = '.\Build\bin\Terraria.Npc.SpawnEligibility.Verification\Release\net10.0\Terraria.Npc.SpawnEligibility.Verification.dll'
$dotnetArgumentsForP14 = @(
  $verificationDllForP14,
  '--spawn-slot-protection-checks-only'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgumentsForP14
```

退出码 `0`，输出 `PASS: NPC slot protection ordering and lifecycle writes`。没有运行完整 verifier、完整测试套件或完整参考项目构建；
P14 整体 `verificationStatus` 仍为 `not-run`。

### 1.15 Spawner 请求准备与 pre-commit 组合

Version4 `NPC.Spawner.SpawnNPC` 位于 `Terraria/NPC.cs:5152-5182`，完整参考对应方法位于
`NPC.cs:5170-5200`。两处方法体顺序均为：调用 `NPCID.FromNetId(Type)` 判断 net ID 是否为 slime；
slime 消费第一次 `RollLuck(180)` 并可能改为 `-4`；周年世界再消费第二次相同范围的 roll，命中时覆盖为
`667`；请求 target 为 `255` 时读取 `defaultTarget`；最后调用 `NewNPC`。之后 `NewNPC` 才执行 GoodWorld
`Next(3)`、slot lookup 和选中槽 protection 写入。

新增的 `NpcSpawnEntityPreparationSystem.Prepare` 对应前一段请求准备；`NpcSpawnPreCommitSystem.Prepare`
再依序调用它与 `NpcSpawnSlotAcquisitionSystem.Acquire`。这只产生 prepared request、slot 选择和 protection
观察，不调用 `NewNPC`、不分配或初始化 NPC，也不接网络 API。`INpcSpawnEntityPreparationPort` 与
`INpcSpawnPreCommitPort` 是端口声明，生产 adapter 仍缺失。

CPG Query API 在 2026-10-01 使用只读 SQLite manifest
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364` 查询上述边界；
`SourceSnapshotId=null`。`Find-CpgSymbols` 对 `SpawnNPC` 返回三个精确方法符号，对 `NewNPC` 返回一个
`IEntitySource` overload。两个方法的 `Get-CpgCallableFacts` 均为 `partial` 并带
`CalleeEffectsNotExpanded`；`SpawnNPC` facts 未返回调用 operation，`NewNPC` facts 只显示
`GetAvailableNPCSlot` operation，direct targets 为空。CPG 不闭合该关系，因此调用顺序以 Version4 和完整参考
源码为证据；不把空 direct-target 列表解释为没有调用。

focused slot-protection verifier 加入 entity preparation / pre-commit trace。周年 slime 输入的 fake-port
调用顺序为 `FromNetId(1) -> RollLuck(180) -> RollLuck(180) -> Next(3) -> FromNetId(667) -> reverse -> slot-zero -> capture-slot-facts -> protect(slot 1, 2)`；prepared request 的 type 为 `667`，target
sentinel `255` 被 snapshot default `42` 替换。普通世界 slime 用例只消费一个 variant roll，并保持显式
target `17`。数据包相关的 `INpcSpawnSyncPacketPort` 与 `INpcReplicationPacketApi` 保持函数声明和空
request/result 类型声明；未加入编解码、发送、接收、recipient selection 或 publish 逻辑。

本轮构建使用 SDK `10.0.400`，经仓库串行 runner 只构建受影响 verifier 项目：

```powershell
$dotnetRootForP14 = 'C:\Users\shan\.dotnet'
$env:DOTNET_ROOT = $dotnetRootForP14
$env:PATH = $dotnetRootForP14 + ';' + $env:PATH
$dotnetArgumentsForP14 = @(
  'build',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '-c', 'Release', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false',
  '--nologo', '-v:minimal'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgumentsForP14
```

退出码 `0`，0 warning、0 error；产物位于
`Build/bin/Terraria.Npc.SpawnEligibility.Verification/Release/net10.0/`。随后直接运行该项目 apphost 的
`--spawn-slot-protection-checks-only` 分支，退出码 `0`，输出
`PASS: NPC slot protection ordering and lifecycle writes`。该分支验证 fake-port 顺序和 slot protection
组合，不是 Version4 行为差分。未运行默认 verifier、完整测试套件或完整参考项目构建；生产接线、实体 commit、
网络行为仍为 `unknown`，P14 `implementationStatus: partial`、`verificationStatus: not-run`，不称迁移成功。

### 1.16 最终 NPC spawn request 传递

复核 Version4 `NPC.NewNPC` (`Terraria/NPC.cs:67051-67067`) 与完整参考对应入口
(`Terraria/NPC.cs:82017-82033`) 后，确认 GoodWorld type remap 会在 slot metadata 查询前更新局部 `Type`，
并以该类型继续传入 `SetDefaults(Type)`。因此 `NpcSpawnEntityPreparationResult.PreparedRequest.Type` 只能表示
`Spawner.SpawnNPC` 的请求准备结果，不能单独作为创建 owner 的最终 type。

现 `NpcSpawnPreCommitResult.ResolvedRequest` 使用 `SlotAcquisition.TypeResolution.ResolvedType` 覆盖
prepared request 的 `Type`，并保留位置、start index、AI 和 target 等其它请求字段。后续 owner 仅在
`SlotAcquisition.Found` 时可提交此请求；slot 为空时，本结果不代表创建成功或存在有效 slot。此属性只传递
确定性结果，不执行替换、初始化、激活、人口计账或数据包操作。

继续检索 NLTX 的实际实体接口后，确认 `WorldStorageRoot.Npcs` 是动态扩容的通用
`EntitySlotStore<WorldEntityState, NpcSlot>`：`TryAllocate` 选择首个可分配槽；新增 `TryAllocateAt`
可将状态写入指定空槽，`TryReplace` 通过期望 generation 替换指定已有槽。当前只有
`ProjectileEntityState` 继承 `WorldEntityState`，没有 NPC entity state/hydration System 或已闭合的调用链，
也没有生产 System 调用这些存储操作。
`INpcDefinitionQuery.TryGetByTypeId` 已暴露 catalog 现有查找能力，但尚无消费此 API 的 NPC hydration
System。该存储原语不会被当作 NPC commit owner；状态 hydration、slot facts 同步及正式 commit 仍为
`unknown`。

`INpcSpawnSyncPacketPort` 与 `INpcReplicationPacketApi` 继续只保留函数声明；packet API 的 request/result
保留类型声明，不包含编解码、recipient selection、publish、send 或 receive 实现。未新增 packet 实现，
也未把未确认的 network owner 接入 spawn commit。

受影响项目的 Release 构建通过仓库串行 runner 执行，SDK `10.0.400`：

```powershell
$env:DOTNET_ROOT = 'C:\Users\shan\.dotnet'
$env:PATH = $env:DOTNET_ROOT + ';' + $env:PATH
$dotnetArgumentsForNpcSpawn = @(
  'build',
  '.\src\NSSLC\Component\Npc\Terraria.Npc.csproj',
  '-c', 'Release',
  '-m:1',
  '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false',
  '--nologo',
  '-v:minimal'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgumentsForNpcSpawn
```

退出码 `0`，0 warning、0 error；目标产物为
`Build/bin/Terraria.Npc/Release/net10.0/Terraria.Npc.dll`。本轮未运行 verifier 或测试；没有构建
完整参考项目。该构建发生在本轮 `INpcDefinitionQuery.TryGetByTypeId` 声明加入之前，不能作为新增 API 的编译证据；
本轮按先前静态报告约束未构建或运行测试。故此结果只证明先前受影响项目编译，P14 `verificationStatus` 仍为 `not-run`，不代表行为等价
或迁移完成。

### 1.17 指定 NPC slot 的存储分配原语

`NpcSpawnSlotSelectionSystem` 可按 legacy 规则选出任意 slot index，而 `WorldStorageRoot.Npcs` 使用的
`EntitySlotStore<TState,TSlot>.TryAllocate` 只取首个空槽；其结果无法保证落在上一步选中的 slot。为给后续
NPC commit owner 提供匹配的存储操作，在 `src/NSSLC/Component/WorldStorage/EntitySlotStore.cs` 增加
`TryAllocateAt(TSlot slot, TState state, out uint generation)`。它只允许最大容量范围内的空闲 index，按需扩容，
拒绝已占用/已耗尽槽位，并沿用 store 的 generation 与 active-count 不变量。已占用替换仍使用既有
`TryReplace(slot, expectedGeneration, replacement, out replacementGeneration)`。

本方法目前只是 WorldStorage 的通用存储原语；尚无 `NpcEntityState`、NPC hydration，也没有 spawn commit System
调用它。NPC slot facts、`WorldStorageRoot.Npcs` 与 infrastructure NPC slot pool 的单一权威关系、替换资格和
population/lifecycle 写入仍为 `unknown`。它不执行选槽、slot protection、默认值初始化、激活、同步或持久化。

本轮只在既有 `Test/Terraria.Npc.SpawnEligibility.Verification` 新增指定槽存储分支，并添加对
`Terraria.WorldStorage` 的项目引用；测试覆盖显式 index 2 分配、重复分配拒绝、替换 generation 递增与旧 handle
失效、超容量拒绝无状态变更，以及释放后重用 generation 连续递增。没有运行完整 verifier 或其它测试分支。

受影响 verifier 的 Release 构建使用 SDK `10.0.400`，经仓库串行 runner 执行：

```powershell
$env:DOTNET_ROOT = 'C:\Users\shan\.dotnet'
$env:PATH = $env:DOTNET_ROOT + ';' + $env:PATH
$dotnetArgumentsForSelectedSlot = @(
  'build',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '-c', 'Release',
  '-m:1',
  '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false',
  '--nologo',
  '-v:minimal'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgumentsForSelectedSlot
```

退出码 `0`，0 warning、0 error；验证器产物为
`Build/bin/Terraria.Npc.SpawnEligibility.Verification/Release/net10.0/Terraria.Npc.SpawnEligibility.Verification.dll`。
只运行新增核心分支：

```powershell
$dotnetArgumentsForSelectedSlot = @(
  '.\Build\bin\Terraria.Npc.SpawnEligibility.Verification\Release\net10.0\Terraria.Npc.SpawnEligibility.Verification.dll',
  '--specified-slot-allocation-checks-only'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgumentsForSelectedSlot
```

退出码 `0`，输出 `PASS: specified-slot allocation and generation guards`。这只验证通用 slot store，不证明
NPC slot facts 映射、replace eligibility、NPC 状态创建或 Version4 spawn 行为；P14 整体
`verificationStatus` 仍为 `not-run`，`implementationStatus` 仍为 `partial`。

### 1.18 替换候选 generation 传递

Version4 `NPC.NewNPC` 先从 `GetAvailableNPCSlot` 得到槽，再在同一同步调用中执行
`spawnSlotProtected[slot] = 2`、替换该槽对象并初始化。当前 NLTX 的 slot selection 与 store commit
尚未形成调用闭包；通用 `EntitySlotStore.TryReplace` 已要求 `expectedGeneration`，但此前
`NpcSpawnSlotFact` / `NpcSpawnSlotSelectionResult` 没有跨选择步骤携带 occupant generation。

本轮为 `NpcSpawnSlotFact` 增加可空 `Generation`，只在 fact 表示可替换 occupant 且 generation 大于零时暴露
`ExpectedGeneration`；replacement fallback 会把它复制进 `NpcSpawnSlotSelectionResult`。generation 缺失或为零时，
纯 slot query 仍返回 Version4 顺序选中的候选，保持选择语义；新增 `IsCommitReady` 明确区分候选与可提交槽。
`NpcSpawnSlotSelectionSystem.SelectAndProtect` 对缺少 guard 的 replacement 不写 protection，
`NpcSpawnSlotAcquisitionResult.Found` 对其返回 false，因此后续 definition lookup/commit 不会把它当作可用槽。
有 guard 的 replacement 仍在返回前保护已选槽；空槽 allocation 仍由 `TryAllocateAt` 的占用检查兜底。

这只是 pre-commit 值对象的并发/过期检查材料，不实现 NPC state hydration 或 commit owner。替换端还必须重新读取
当前 occupant、确认仍可替换、用该 expected generation 调用 `TryReplace`，并维护 generation 与 entity handle
一致；生产 slot facts adapter 与 world/session 单 writer 仍为 `unknown`。Version4 `NPC.cs:67051-67097` 中的
`ResetForNewNPC`、完整 `SetDefaults`、town unique data 和 spawn 后状态均不在本轮实现范围内；按用户指示，不实现
`SetDefaults`。

只读 CPG Query API 对精确 `NewNPC(IEntitySource,...)` 符号返回 `complete`，但
`Get-CpgCallableFacts` 为 `partial/CalleeEffectsNotExpanded`，仅显示 `GetAvailableNPCSlot` invocation，
`DirectCallTargets` 为空。相关关系继续以 Version4 源码顺序为准；CPG 结果不证明 slot facts 或 lifecycle owner。
完整参考 `D:\TRbackup\无任何删减通过编译\Terraria\NPC.cs` 的 `NewNPC` 主体保留相同 slot→protection→
create/reset/defaults 顺序；它只作关系参考，目标语义仍以 `D:\TRbackup\Version4` 为准。

本轮只运行 focused verifier 分支，SDK `10.0.400`，并经仓库串行 runner 构建：

```powershell
$dotnetRootForP14 = 'C:\Users\shan\.dotnet'
$env:DOTNET_ROOT = $dotnetRootForP14
$env:PATH = $dotnetRootForP14 + ';' + $env:PATH
$dotnetArgumentsForNpcSlotGeneration = @(
  'build',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '-c', 'Release', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false', '--nologo', '-v:minimal'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgumentsForNpcSlotGeneration
```

退出码 `0`，0 warning、0 error；目标产物为
`Build/bin/Terraria.Npc.SpawnEligibility.Verification/Release/net10.0/Terraria.Npc.SpawnEligibility.Verification.dll`。
只运行 replacement generation 与 slot protection focused 分支：

```powershell
$dotnetArgumentsForNpcSlotGenerationCheck = @(
  '.\Build\bin\Terraria.Npc.SpawnEligibility.Verification\Release\net10.0\Terraria.Npc.SpawnEligibility.Verification.dll',
  '--spawn-slot-protection-checks-only'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgumentsForNpcSlotGenerationCheck
```

退出码 `0`，输出 `PASS: NPC slot protection ordering and lifecycle writes`。未运行完整 verifier、完整参考项目构建、
Version4 runtime 差分、NPC hydration/commit、网络或存档验证；这次成功只覆盖 isolated core，整体
`verificationStatus` 继续为 `not-run`。

数据包相关 API 维持函数声明及 request/result 类型声明，不编写编解码、收发、recipient selection 或 publish。
因此 P14-B4 仍为 `blocked-by-unknown`，整体 `implementationStatus: partial`、
`verificationStatus: not-run`；本记录不称迁移成功。

### 1.19 最终 NPC type 的 definition lookup

`NpcSpawnPreCommitResult.ResolvedRequest` 已把 `Spawner.SpawnNPC` 的 variant 与 GoodWorld type remap 串成最终请求，
但此前没有消费者按该 final type 查询 content profile。本轮新增 `NpcSpawnDefinitionResolutionSystem.Resolve`，
仅在 slot acquisition 成功时查询 definition：正 type ID 使用 `INpcDefinitionQuery.TryGetByTypeId`，负 net-ID
variant 使用 `TryGetByNetId`；零值返回 `InvalidType`，无槽、未命中、identity 不匹配分别返回具名 status。结果
携带原 final request 与 definition 引用，不建立 NPC entity state，不提交到 `WorldStorageRoot.Npcs`，不应用
`SetDefaults`、town unique data、位置/碰撞初始化、生命周期、target 或 network effect。

目标源码 Version4 `Terraria/NPC.cs:8133-8145` 在 `SetDefaults(int Type, ...)` 中对 `Type < 0` 调用
`SetDefaultsFromNetId(Type, ...)` 后返回；`SetDefaultsFromNetId` 先经 `NPCID.FromNetId(id)` 还原基类 type，再按
variant 分支应用 scale override。CPG Query API 对 `SetDefaultsFromNetId` 精确符号与 `Terraria/NPC.cs` 中一个
静态 `CallTargets` 调用点均返回 `complete`；manifest 为
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`。因此本地解析器
用负 net ID 查 `TryGetByNetId`，不把 `-4` 误作普通正 type lookup。完整参考 `D:\TRbackup\无任何删减通过编译\Terraria\NPC.cs`
中同样的负 Type 分支仅用于核对关系；行为目标仍是 Version4。

只增加 focused verifier 分支：final positive type 614、negative net-ID variant -4、无可用 slot 和缺失 definition。
构建使用 SDK `10.0.400`，通过仓库串行 runner：

```powershell
$dotnetRootForP14 = 'C:\Users\shan\.dotnet'
$env:DOTNET_ROOT = $dotnetRootForP14
$env:PATH = $dotnetRootForP14 + ';' + $env:PATH
$dotnetArgumentsForNpcDefinitionResolution = @(
  'build',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '-c', 'Release', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false', '--nologo', '-v:minimal'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgumentsForNpcDefinitionResolution
```

退出码 `0`，0 warning、0 error；输出包括
`Build/bin/Terraria.Content/Release/net10.0/Terraria.Content.dll`、
`Build/bin/Terraria.Npc/Release/net10.0/Terraria.Npc.dll` 与 verifier DLL。
只运行 `--spawn-definition-resolution-checks-only`，经同一串行 runner 执行 verifier DLL，退出码 `0`，输出
`PASS: NPC spawn definition lookup by final type and net ID`。该分支不覆盖定义 hydration 或 Version4 spawn 行为差分。

`NpcSpawnDefinitionResolutionSystem` 只完成最终 type 到 content query 的组合；按用户指示不实现 `SetDefaults`。
NPC aggregate/hydration、replacement revalidation、唯一 population/lifecycle writer、runtime caller、网络与存档仍为
`unknown`/`blocked-by-unknown`。P14 整体维持 `implementationStatus: partial`、`verificationStatus: not-run`，
不称迁移成功。

### 1.20 反向 slot-0 元数据边界

CPG Query API 对 `GetAvailableNPCSlot(int,int)` 符号及 `Terraria/NPC.cs` 中的静态调用点查询均为
`complete`，该文件内返回一个精确调用点。查询使用 Version4 manifest
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`；
完整参考源码不是该 CPG snapshot 的绑定源。

源码 `Version4/Terraria/NPC.cs:67098-67124` 与完整参考
`无任何删减通过编译/Terraria/NPC.cs:82067-82093` 在 `startIndex == 0 && CannotSpawnInSlot0`
时先把 `startIndex` 改为 1，再于反向搜索中将其作为排他的停止边界。因此反向扫描会跳过 slot 1，
不只是 slot 0；NLTX `NpcSpawnSlotSelectionQuery` 当前顺序与这段源码一致。

本轮给 `--spawn-slot-protection-checks-only` 增加该边界断言：slot 0 空闲、slot 1 可替换、slot 2/3
占用不可替换，使用 `startIndex: 0`、反向搜索和禁止 slot 0 元数据时必须返回无槽。串行构建
`Terraria.Npc.SpawnEligibility.Verification.csproj` 使用 SDK `10.0.400`，退出码 `0`、0 warning、
0 error，产物位于 `Build/bin/Terraria.Npc.SpawnEligibility.Verification/Release/net10.0/`；随后仅运行
该 focused 分支，退出码 `0`，输出 `PASS: NPC slot protection ordering and lifecycle writes`。
该结果只验证 slot-selection/protection core 与 fake port，不证明生产 facts adapter 或运行时等价；
整体 `verificationStatus` 仍为 `not-run`、`implementationStatus` 仍为 `partial`。

### 1.21 Replacement candidate 缺少 generation 时禁止提交

继续收紧替换槽的 pre-commit 边界：`NpcSpawnSlotSelectionQuery` 仍按 Version4 顺序返回替换候选，
即使 fact 未带 occupant generation，也不改写纯 Query 的候选选择结果。`NpcSpawnSlotSelectionResult.IsCommitReady`
要求 replacement candidate 具有正 generation；`NpcSpawnSlotSelectionSystem.SelectAndProtect` 只保护
commit-ready 槽；`NpcSpawnSlotAcquisitionResult.Found` 也只对 commit-ready 槽为 true。因而缺少 generation
时，候选索引保留用于诊断，但不会写 protection，也不能继续通过 acquisition 交给提交链。

focused verifier 覆盖有 generation 的候选仍写 protection，以及缺少 generation 时 Query 保留候选、
`IsCommitReady` 为 false、protection 不写且 acquisition 不可提交。该 guard 不证明真实 occupant
映射、生产 facts adapter、replacement revalidation 或唯一 commit owner；这些关系仍为 `unknown`，
不改变 P14 `implementationStatus: partial` 与 migration-level `verificationStatus: not-run`。

构建使用仓库 `global.json` 指定的 SDK `10.0.400`。系统 PATH 下的 dotnet 未包含该 SDK；本次只在
PowerShell 进程内将已安装的 `C:\Users\shan\.dotnet` 前置到 PATH，未改 SDK 配置。通过仓库 serial
runner 构建目标 verifier：

```powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
$dotnetArgumentsForSlotProtectionGuard = @(
  'build',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false', '-v:minimal'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgumentsForSlotProtectionGuard
```

构建退出码 `0`，0 warning、0 error；产物位于
`Build/bin/Terraria.Npc.SpawnEligibility.Verification/Debug/net10.0/Terraria.Npc.SpawnEligibility.Verification.dll`，
依赖项目产物也位于仓库 `Build/bin/`。随后经相同 serial runner 只运行 slot-protection focused 分支：

```powershell
$dotnetArgumentsForSlotProtectionGuardCheck = @(
  'run', '--project',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '--no-build', '--no-restore', '--', '--spawn-slot-protection-checks-only'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgumentsForSlotProtectionGuardCheck
```

focused run 退出码 `0`，输出 `PASS: NPC slot protection ordering and lifecycle writes`。本轮未运行完整
verifier、完整参考项目构建、Version4 runtime 差分、NPC hydration/commit、网络或存档验证；该结果只覆盖
isolated core 与 recording fake，不称迁移成功。按用户指示仍不实现 `SetDefaults`；数据包相关 API 仅保留
函数及 request/result 类型声明，不写编解码、收发、recipient selection 或 publish 逻辑。

### 1.22 Tower-zone spawn selection

新增 `NpcSpawnTowerSelectionSystem.Select`，将 `SpawnAnNPC` 开头的四个 tower-zone 分支映射为
attempt-local NPC type selection，并返回已有 `NpcSpawnEntityRequest`，供后续 pre-commit API 消费。
输入只读取 tower-zone snapshot 和选中的 tile 坐标；随机与 active NPC count 通过
`INpcSpawnTowerSelectionPort` 显式取得。优先级为 Nebula、Vortex、Stardust、Solar；重复候选保留
各自权重。Nebula 对 424/423/420 的上限为 3，Vortex 对 425/426 的上限为 3、429 为 4，Solar 对
518 的上限为 2、412 为 1；触顶后按源码重新抽取。Solar 先抽到 418 时仍按源码消费 `Next(2)`，
返回 0 才从 `[415, 416, 419, 417]` 再抽一次。每个 tower request 使用
`X = spawnTileX * 16 + 8`、`Y = spawnTileY * 16`、`StartIndex = 1`、四个零 AI 值和
legacy `Target = 255` sentinel。

Version4 目标分支位于 `Terraria/NPC.cs:1190-1295`，随机 helper 位于 `Terraria/Utils.cs:1633`，
population read 为 `Terraria/NPC.cs:76287`；完整参考对应 `Terraria/NPC.cs:1208-1313`。
两份源码的候选、权重、上限和顺序一致。CPG Query API manifest 为
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`；
`SpawnAnNPC` 精确符号查询 `complete`（1 个声明），callable facts 为 `partial`，gap 是
`CalleeEffectsNotExpanded` 且未给出 `DirectCallTargets`。因此调用分支关系以 Version4 和完整参考源码
直接核对；CPG 零目标不作为没有调用的证据。

本 System 只返回 pre-commit 请求，不调用 `SpawnNPC`/`NewNPC`，不分配 slot、不创建/激活实体，也不写人口。
target sentinel 留给后续 `NpcSpawnEntityPreparationSystem` 按现有 snapshot 解析；实际 RNG、population
adapter 与自然生成 continuation 的连接仍 `unknown`，其它 `SpawnAnNPC` 分支也未迁入。本轮 focused
verifier 只覆盖 Solar 的 518/412 上限重抽、随机范围与 count 查询顺序，以及最终请求坐标、slot、AI 和
target 字段。

受影响 verifier 构建使用 SDK `10.0.400`、仓库 serial runner，退出码 `0`、0 warning、0 error；产物位于
`Build/bin/Terraria.Npc.SpawnEligibility.Verification/Release/net10.0/`。focused 分支退出码 `0`，输出
`PASS: NPC solar tower weighted selection and population rerolls`。首次运行命令误将 MSBuild 参数放在
`dotnet run` 的 `--` 前，意外进入默认 verifier 分支并输出了 8 组既有 core PASS；该运行没有覆盖新分支。
随后移除这些参数并明确传入分支参数，才得到上述 focused 结果。该误路由范围超出本轮计划的单分支验证，
已记录；未运行其它 focused 分支、完整参考项目构建或 Version4 runtime 差分。P14 整体仍为
`implementationStatus: partial`、`verificationStatus: not-run`，不称迁移成功。

```powershell
$env:DOTNET_ROOT = 'C:\Users\shan\.dotnet'
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
$dotnetArguments = @(
  'build',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '-c', 'Release', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false', '--nologo', '-v:minimal'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments

$dotnetArguments = @(
  'run', '--project',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '-c', 'Release', '--no-build', '--no-restore', '--',
  '--spawn-tower-selection-checks-only'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
```

### 1.23 SkyMob spawn branch selection

新增 `NpcSpawnSkyMobSelectionSystem.Select`，将 `SpawnAnNPC` 的 `else if (skyMob)` 分支隔离为一次
NPC type 选择并返回 `NpcSpawnEntityRequest`。输入显式携带 tile 坐标/world width、`skyBehindPlayer`、
Water Candle、invasion、hardmode/progression 与 `noWorms`/purple-slime unlock 状态；`AnyDanger`、
active NPC presence、`RollLuck` 和 `Next` 经 `INpcSpawnSkyMobSelectionPort` 读取。请求保持源码的
`X = spawnTileX * 16 + 8`、`Y = spawnTileY * 16`、默认 `StartIndex = 0`、零 AI 与 `Target = 255`。

Version4 分支位于 `Terraria/NPC.cs:1296-1338`，完整参考对应 `NPC.cs:1314-1356`；两处条件、
顺序和请求参数一致。相关 helper 在 Version4 `NPC.cs:5268-5274`（Luck roll）、`66553+`
（`AnyDanger`）及 `76320-76333`（active/type `AnyNPCs`）。只读 CPG Query API 的
`SpawnAnNPC` 符号查询为 `complete`（1 个声明），callable facts 为 `partial`，带
`CalleeEffectsNotExpanded` gap 且没有 `DirectCallTargets`；`AnyNPCs` 与 `AnyDanger` 各有一个选定
shard 声明，`RollLuck` 查询返回 3 个方法节点。故 helper 行为和调用顺序以两份源码方法体复核，
CPG facts 不用于推断 callee effects。查询使用 manifest
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`。

保留两组带 Water Candle 的近似重复 `else if`。前一个分支的 random roll 失败后，后一个分支会重新
执行条件并可再消费 `Next`; 合并条件会改变随机序列。`skyMob` 为 false 时结果为 `NotApplicable`；
适用时即使特殊条件都不满足也生成默认 type 48 的请求。此 System 不调用 `SpawnNPC`、`NewNPC`，
不做 `SetDefaults`，也不持有 target/population/slot/network 写入权。生产 snapshot/effect adapter、
continuation 与 commit owner 仍为 `unknown`。

该 selector 初始代码批次只做编译检查，未运行测试或 verifier；本轮后续 branch-selection focused run 见 §1.25。
原编译命令经仓库 serial runner 使用 SDK `10.0.400`
构建 `src/NSSLC/Component/Npc/Terraria.Npc.csproj`（Release），退出码 `0`、0 warning、0 error；
产物为 `Build/bin/Terraria.Npc/Release/net10.0/Terraria.Npc.dll`。该编译不验证 SkyMob 行为等价，
P14 `verificationStatus` 继续为 `not-run`。

```powershell
$dotnetArguments = @(
  'build',
  'D:\TRbackup\NLTX\src\NSSLC\Component\Npc\Terraria.Npc.csproj',
  '-c', 'Release', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false', '--nologo', '-v:minimal'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
```

### 1.24 Invasion spawn branch selection

新增 `NpcSpawnInvasionSelectionSystem.Select`，将 `SpawnAnNPC` 的 `else if (invaders)` 分支隔离为
一次 invasion NPC type 选择，并返回 `NpcSpawnEntityRequest` 或可区分的状态。输入显式携带
`invaders`、invasion type、tile 坐标、hardmode 和 invasion size；active NPC presence、随机抽取与
Pirate invasion 的 solid-tile 区域判断经 `INpcSpawnInvasionSelectionPort` 提供。

Version4 `Terraria/NPC.cs:1339-1483` 与完整参考 `NPC.cs:1357-1501` 的 type 1–4 分支逐段核对一致：
Goblin、Frost、Martian 与 Pirate 分支的 `else if` 顺序、短路次序和 `Next` 上界保持原序；type 3
先检查 Martian Saucer 的 invasion-size/random/presence/solid-area 条件，SolidTiles 区域坐标为
`[spawnTileX-20, spawnTileX+20] × [spawnTileY-40, spawnTileY-10]`，命中后生成 Y 坐标上移 10 tiles；
type 4 在 `Next(7)` 后计算 captain 条件，再消费 `Next(45)`，符合 `num7 >= 6` 时继续消费
`Next(20)`，并按原分支范围选择其余类型。type 1–3 请求使用 `StartIndex=0`，type 4 使用
`StartIndex=1`；请求的像素位置、零 AI 和默认 `Target=255` 与 `SpawnNPC` 默认参数一致。

只读 CPG Query API 对 `Collision.SolidTiles` 符号返回 `complete`，识别两个 overload，四整数参数签名为
`bool(int,int,int,int)`；选定 `Terraria/NPC.cs` shard 的 call-site 查询返回 `complete`、15 个静态
调用点、0 gaps。`SpawnAnNPC` 符号查询为 `complete`（1 个声明），callable facts 为 `partial`，包含
`CalleeEffectsNotExpanded` gap，且没有 `DirectCallTargets`。因此 CPG 命中只用于核对符号/静态调用事实，
分支中的实际调用顺序、具体参数与效果仍以目标源码和完整参考源码复核；查询不证明 callee effects。
数据集 manifest 为
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`。

`invaders=false` 返回 `NotApplicable`；类型 1–4 均标记为分支已处理，未产生类型时返回
`HandledWithoutRequest`；未知 invasion type 返回 `UnknownInvasionTypeEarlyReturn`，保留 legacy
`SpawnAnNPC` 的早退语义。该 System 仅构造请求，不调用 `SpawnNPC`、`NewNPC` 或 `SetDefaults`，也不
分配 NPC slot、不写实体/population 状态或实现网络/存档效果。生产 snapshot/RNG/presence/tile
adapter、旧入口接线、continuation 和创建提交 owner 仍为 `unknown`；本项 core 不构成迁移成功证据。

该 selector 初始代码批次只编译受影响项目，没有运行测试或 verifier；本轮后续 branch-selection focused run 见 §1.25。
仓库 `global.json` 锁定 SDK `10.0.400`；默认
`dotnet` host 未枚举用户级 SDK，因此通过 `PATH` 优先选择已有的
`C:\Users\shan\.dotnet\dotnet.exe`，未修改 SDK 配置。串行 runner 编译
`src/NSSLC/Component/Npc/Terraria.Npc.csproj`（Release）退出码 `0`、0 warning、0 error；
`Build/bin/Terraria.Npc/Release/net10.0/Terraria.Npc.dll` 已生成。此结果只说明项目可编译，不验证
入侵分支行为或运行时接线；整体 `verificationStatus` 继续为 `not-run`。

```powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
$dotnetArguments = @(
  'build',
  '.\src\NSSLC\Component\Npc\Terraria.Npc.csproj',
  '-c', 'Release', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false', '--nologo', '-v:minimal'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
```

### 1.25 Graveyard / dual-dungeon spawn branch selection

新增 `NpcSpawnGraveyardDualDungeonSelectionSystem.Select`，输入是 pass 已筛选的
`NpcSpawnAcceptedCandidate`，从中读取 `DownedBoss3`、graveyard zone、
`TresspassingDualDungeon`、hardmode、chosen-tile `NoWormsForSpawn` 与选中 tile 坐标。
`INpcSpawnGraveyardDualDungeonSelectionPort` 暴露 `RollBadLuckExtreme`、active NPC presence、
Statue Mimic 地形检查和 `RollBadLuck`；这些操作保留为 effectful 外部读取/随机端口，不能按纯 Query
重排或重复调用。selector 只返回 pre-commit `NpcSpawnEntityRequest`，不调用创建入口。

Version4 `Terraria/NPC.cs:1484-1498` 与完整参考 `Terraria/NPC.cs:1502-1516` 条件顺序一致：
先依次短路检查 `downedBoss3`、graveyard、`!noWorms`、`RollBadLuckExtreme(25) == 0`、
`!AnyNPCs(690)`、`IsThisAGoodPlaceForAStatueMimic`，成功时请求 type 690；该条件链未命中才检查
`tresspassingDualDungeon && RollBadLuck(15) == 0`，再按 hardmode 选择 type 82 或 316。三种请求都使用
`X = tileX * 16 + 2`、`Y = tileY * 16`、默认 `StartIndex = 0`、零 AI 与 `Target = 255`。
两份源码中的 `IsThisAGoodPlaceForAStatueMimic` 方法体相同：读取两格地面 `SolidTile2` 与上方
3 层、横向两格 tile 的 active 状态。此 helper 的 tile 读取经 port 提供；没有实现其生产 adapter。

Version4 CPG Query API 对 `IsThisAGoodPlaceForAStatueMimic` 的精确符号和所选 `NPC.cs` 静态 call-site
查询均为 `complete`，callable facts 为 `partial/CalleeEffectsNotExpanded`。对
`tresspassingDualDungeon` 的符号与选定 `NPC.cs` member-use 查询均为 `complete`，返回字段声明和 3 个
静态使用事实；源码逐项核对定位到 flags capture `NPC.cs:318`、rate adjustment `NPC.cs:566` 与生成分支
`NPC.cs:1488`。字段 uses 的 AccessMode 包含 `Unknown`，不能由索引推断完整写入闭包或唯一 owner。查询使用
manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`、
`SourceSnapshotId=null`；CPG callable facts 不替代目标和完整参考源码。

此前该批六个 fake-port 核心场景通过：未知 invasion 类型早退、Martian Saucer 坐标及 SolidTiles 参数、
Pirate Captain 顺序与 `StartIndex=1`、SkyMob Water Candle 重抽、Statue Mimic 请求及分支短路、
以及 Statue Mimic presence gate 后的 dual-dungeon fallback。它们只覆盖隔离 selector 的局部请求和调用顺序，
不覆盖真实 `SpawnAnNPC` 优先级组合、生产 adapter、NPC 提交/同步、网络/存档或 Version4 runtime 差分。
本 selector 未实现 `SetDefaults`，也未调用数据包 API；数据包相关 API 仍只有声明。P14 整体维持
`implementationStatus: partial`、`verificationStatus: not-run`，不称迁移成功。

验证使用 SDK `10.0.400`，从仓库根目录经 serial runner 编译受影响 verifier 项目，退出码 `0`、0 warning、
0 error；产物位于 `Build/bin/Terraria.Npc.SpawnEligibility.Verification/Release/net10.0/`。随后只运行
`--spawn-branch-selection-core-checks-only`，退出码 `0`，输出
`PASS: NPC spawn branch selection core cases`。未运行默认 verifier、全量测试或完整参考项目构建。

```powershell
$env:PATH = 'C:\Users\shan\.dotnet;' + $env:PATH
$dotnetArguments = @(
  'build',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '-c', 'Release', '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false', '--nologo', '-v:minimal'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments

$dotnetArguments = @(
  'run', '--project',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '-c', 'Release', '--no-build', '--no-restore', '--',
  '--spawn-branch-selection-core-checks-only'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
```

### 1.26 Type 244 critter spawn branch selection

新增 `NpcSpawnCritterSelectionSystem.Select`，将 `SpawnAnNPC` 中 `num == 244 && !Main.remixWorld`
分支映射为隔离的 pre-commit selector。`NpcSpawnCritterSelectionInputs` 显式承载 remix/water、tile Y、
world surface、gold critter/gnome chance 与 Halloween、Christmas、Birthday Party 状态；
`INpcSpawnCritterSelectionPort` 将旧 `RollLuck(range)` 与 `Main.rand.Next(max)` 保留为有序 effect calls。
系统不读取全局 `Main`，不调用 `SpawnNPC`，也不创建或初始化实体。

Version4 `Terraria/NPC.cs:1499-1569` 与完整参考 `Terraria/NPC.cs:1517-1587` 的该分支内部条件、顺序及
请求类型相同。水域按一次 `RollLuck(goldCritterChance)` 选择 592/55；地下分支保留
`Next(3)`、可选 gold roll、`Next(2)`、第二个 gold roll、最后 `Next(3)` 的短路顺序。所有条件失败时返回
`HandledWithoutRequest`，不会补造默认 NPC。地表分支保留 gnome、两次 gold chance、依次检查三类节日状态、
双 gem 物种选择及普通 critter 的顺序；节日状态为 false 时不消费对应随机值。type 624 的请求将
`PostSpawnTimeLeftMultiplier` 设为 10，表达原调用 `SpawnNPC(..., 624).timeLeft *= 10` 的后置效果；
该效果仍需未来由实际创建/lifecycle owner 在实体创建后应用。

分支入口仍未接线。两份源码都先以 `Main.tile[spawnTileX, spawnTileY - 1].wall` 初始化局部 `num`，
再在 `Main.tile[spawnTileX, spawnTileY - 2].wall == 244` 或
`Main.tile[spawnTileX, spawnTileY].wall == 244` 时把 `num` 设为 244（Version4 `NPC.cs:1194-1198`；
完整参考 `NPC.cs:1212-1216`）。因此未来 dispatcher 必须捕获这三个位置的 wall facts 并重现覆盖顺序；
当前候选的 `NpcSpawnPostCheckInputs.SpawnWallType` 是 `GetProperGroundSpawnTileTypeAndWallType`
返回的选中生成 tile wall，不足以替代 legacy `num`。当前 `NpcSpawnCritterSelectionInputs` 不含这项
分类事实，selector 只在调用方已选中 type 244 分支时才有意义。

该分支在源码 `else if` 链中位于 Graveyard/Statue Mimic 与 Dual Dungeon 条件之后
（Version4 `NPC.cs:1484-1500`；完整参考 `NPC.cs:1502-1518`）。Tower、SkyMob、Invasion 及其后大量
biome/event 分支也先于该分支；本轮已有独立 selector 不构成覆盖这些分支的完整 dispatch。分支优先级、
前序条件和输入 adapter 均保持 `unknown`，不得把 selector 的存在描述成已接入或已迁移。

所有请求沿用三参数 `SpawnNPC(X, Y, Type)` 的默认值：`X = tileX * 16 + 8`、`Y = tileY * 16`、
`StartIndex = 0`、零 AI、`Target = 255`。Version4 `NPC.cs:5152-5182` 显示该 overload 的目标 sentinel
随后由 `defaultTarget` 替换，创建及 Dual Dungeons particle effect 都在 selector 之外。
`Utils.SelectRandom` 的 Version4 源码 `Terraria/Utils.cs:1633-1638` 与完整参考
`Terraria/Utils.cs:2866-2872` 均为 `choices[random.Next(choices.Length)]`，因此双 gem 分支保留
`Next(3)` 成功后额外消费一次 `Next(2)`。`Spawner.RollLuck(int)` 的 Version4 源码
`NPC.cs:5268-5273` 与完整参考 `NPC.cs:5286-5292` 均委托 `Luck.RollLuck(luck, range)`；端口需沿用
当前 Spawner/Player luck 上下文，生产 adapter 仍为 `unknown`。

Version4 CPG Query API 使用 manifest
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`。
`Utils.SelectRandom<T>` 的符号查询为 `complete/confirmed`；其在选定 `Terraria/NPC.cs` shard 的 call-site
查询为 `partial`、0 命中并带 `NoMatchingFactInScannedScope`，callable facts 带
`CalleeEffectsNotExpanded`。据此不从零命中推断无调用，改以两份源码确认该分支的 `SelectRandom` 调用和
其单次 `random.Next(choices.Length)` 行为。`NPC.Spawner.RollLuck(int)` 符号查询为 `complete`；选定
`NPC.cs` shard 返回 56 个 `RollLuck` 静态调用点，数量覆盖全文件而非只代表 type 244；分支关系由源码行确认。

本轮未运行 focused verifier 或测试。selector 的生产源码已通过 `Terraria.Npc.csproj` 编译，相关
verifier 项目也仅完成编译；这不验证随机调用 trace 或断言结果。selector 仍无生产 effect adapter、真实
事件/tile capture、旧 `SpawnAnNPC` 分支 dispatch、实体 commit、timeLeft 后置效果接线或 continuation caller。
P14 仍为 `implementationStatus: partial`、`verificationStatus: not-run`，不能称迁移成功；没有实现
`SetDefaults`，数据包 API 仍只保留声明。

本次文档补正另用只读 CPG Query API 查到 `SpawnAnNPC` 精确符号 `void(int,int,int,bool,int)`，
`Find-CpgSymbols` 为 `complete`；`Get-CpgCallableFacts` 为 `partial`，返回 7 个 CFG 节点、57 个
operation 节点、0 个 direct call targets，gap 为 `CalleeEffectsNotExpanded`。在选定的
`Terraria/NPC.cs`、`Terraria/Main.cs`、`Terraria/MessageBuffer.cs` 范围查询到 1 个直接调用点，位于
`NPC.cs` 的 `TrySpawnAnNPC`；该结果不闭合 dispatcher 内部动态/间接关系。manifest 为
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`；
CPG 的 `spanStart` 是字符偏移，不作源码行号。随后直接读取 Version4 与完整参考的 `NPC.cs` 方法体，
确认上述 wall 派生和分支优先级。此次仅修改设计/执行文档，没有修改源码或 verifier，没有执行构建或测试。

### 1.27 Type 244 变更的编译检查（未运行测试）

编译前按仓库约束检查了活动的 `dotnet.exe`/`csc.exe` 进程，未发现占用进程。使用用户级 SDK
`10.0.400`，仅通过仓库串行 runner 编译受影响的 NPC 生产项目和新增/修改断言所在的 verification
项目；没有运行 `dotnet test`、`dotnet run` 或验证器进程。两次构建均退出码 `0`，各为 `0` warning、
`0` error，restore 状态为所有项目均已是最新。

生产项目命令：

```powershell
$env:DOTNET_ROOT = 'C:\Users\shan\.dotnet'
$env:PATH = $env:DOTNET_ROOT + ';' + $env:PATH
$dotnetArguments = @(
  'build',
  '.\src\NSSLC\Component\Npc\Terraria.Npc.csproj',
  '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false',
  '--nologo', '-v:minimal'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
```

验证项目命令（仅编译）：

```powershell
$env:DOTNET_ROOT = 'C:\Users\shan\.dotnet'
$env:PATH = $env:DOTNET_ROOT + ';' + $env:PATH
$dotnetArguments = @(
  'build',
  '.\Test\Terraria.Npc.SpawnEligibility.Verification\Terraria.Npc.SpawnEligibility.Verification.csproj',
  '-m:1', '-nr:false',
  '-p:UseSharedCompilation=false',
  '-p:MSBuildNodeReuse=false',
  '-p:BuildInParallel=false',
  '--nologo', '-v:minimal'
)
& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArguments
```

产物均位于 `Build/bin/`：`Terraria.Npc.dll` 位于
`Build/bin/Terraria.Npc/Debug/net10.0/`，verification assembly 位于
`Build/bin/Terraria.Npc.SpawnEligibility.Verification/Debug/net10.0/`。此证据只说明当前代码可编译；
type 244 focused assertions、运行时入口、分支 dispatch、实体创建及 P14 行为验收都仍为 `not-run` 或
`unknown`。

## 2. 当前证据与版本核对

### 2.1 来源角色

| 来源 | 本计划角色 | 当前状态 |
| --- | --- | --- |
| `D:\TRbackup\Version4` | 用户指定的目标源码与只读 CPG 项目，也是本次迁移应保留的行为基线 | 只读 CPG SQLite `D:\TRbackup\Version4-cpg-export\out-dop8-interproc.sqlite`；manifest `6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`；未绑定 per-file source snapshot 或精确 source revision。 |
| `D:\TRbackup\无任何删减通过编译` | 用户指定的完整 Terraria 源码参考 | 本轮直接读了 `Terraria/NPC.cs` 的自然生成、候选搜索、tile gate、特殊入口和双地牢 helper，以及 `Main.cs`、`MessageBuffer.cs` caller；目录含 `TerrariaServer.sln`/`.csproj`，`AssemblyInfo.cs` 标为 `1.4.5.6`。完整性与通过编译为用户提供的信息，本轮未自行构建或复验；参考源码补足关系，不自动覆盖 Version4 行为。 |
| `C:\Users\shan\Downloads\ECS\space-station-14-master` | System/EntitySystem/Query 的组织参考 | 阅读了 `SpawnerSystem`、`StationSpawningSystem`、`NPCSystem`；不用于证明 Version4 兼容性。 |
| P14 System report | 已领取 117 成员与边界决策的本任务依据 | `designStatus: proposed`、`verificationStatus: not-run`。 |

上述两个 Terraria 源码根目录的 `NPC.cs`、`Main.cs`、`MessageBuffer.cs` SHA-256 均不同。
`Version4` 已按用户指定确认为目标行为基线；迁移前仍须锁定目标和参考的精确 source revision，
并逐项裁定是否提出行为变更。CPG Query API 是对 `Version4` 的 SQLite 索引查询，不可当作完整
源码参考目录的调用图；当前 CPG source binding 为 `unknown`。

完整参考项目中，本轮直接核对 `Terraria/NPC.cs:81433-81444` 的静态入口、`NPC.cs:185-256`
的玩家循环、`NPC.cs:855-968` 的 area/tile search/post-check、`NPC.cs:5348-5413` 的
footprint 与 screen check、`NPC.cs:5987-6028` 的 Faelings 入口、`NPC.cs:81756-81918` 的
SpawnOnPlayer 路径；入口调用上下文见 `Terraria/Main.cs:18049-18059` 和
`Terraria/MessageBuffer.cs:2223,2779`。这些源码片段用来补足 CPG 索引缺口，不把它们混作
Version4 的 CPG facts。

### 2.2 已取得的关系证据

| 关系/范围 | Query API 结果或直接源码 | 使用限制 |
| --- | --- | --- |
| 静态 `NPC.SpawnNPC()` | `Find-CpgSymbols` 精确区分 static definition；`Find-CpgCallSites` 在选定 scope 返回 `Main.cs` 1 处，`complete` | 只说明索引范围内的静态边；完整副本的 `Main.cs:18054` 已手工复核，但不能绑定到该 CPG manifest。 |
| `Spawner.SpawnNPC()` | 方法符号可解析；选定调用点查询为 `partial`、0 个匹配并带 `NoMatchingFactInScannedScope` | Version4 源码在 `NPC.cs:66519` 由静态 wrapper 直接调用；完整参考 `NPC.cs:81443` 也显示该直接调用。零命中不等于无调用者。 |
| `TrySpawnAnNPC` / `GetSpawnRate` | 两者符号及所选 `NPC.cs` call-site 查询均 `complete`，各 1 个直接调用点；`TrySpawnAnNPC` callable facts 为 `partial`，3 个 CFG 节点、36 个 operation 节点、6 个 invocation 节点 | Version4 源码确认 `Spawner.SpawnNPC -> TrySpawnAnNPC -> GetSpawnRate`；`TrySpawnAnNPC` 与 `GetSpawnRate` facts 均有 `CalleeEffectsNotExpanded` gap。 |
| `SetSpawnFlagsForChosenTile` | 精确 symbol 与所选 `NPC.cs` call-site 查询 `complete`，1 个静态调用点；callable facts 为 `partial`，8 个 CFG 节点、133 个 operation 节点、2 个 invocation 节点 | CPG gap 为 `CalleeEffectsNotExpanded`；Version4 `NPC.cs:952-1189` 与完整参考 `NPC.cs:970-1207` 的主方法体逐段核对一致。完整参考只在双地牢 helper 上有额外 9 个 Zone 写入；当前 core 遵循 Version4 空 helper。 |
| `SpawnAnNPC` | 精确 `void(int,int,int,bool,int)` 符号及所选 `NPC.cs` call-site 查询 `complete`，1 个静态调用点；callable facts 为 `partial`，7 个 CFG 节点、57 个 operation 节点、2 个 invocation 节点，`DirectCallTargets` 为空，gap 为 `CalleeEffectsNotExpanded` | 空目标列表不是“无调用/无副作用”证据。完整参考 `NPC.cs:1208-5169` 与 Version4 `NPC.cs:1190-5151` 的源码区间各含 669 个 `SpawnNPC(` 调用表达式；这是静态文本计数，不能推断实际执行分支或创建数量。当前 continuation 只传递通过筛选时已捕获的输入，不假设能完整重建该入口。 |
| Statue Mimic / dual-dungeon 分支 | `IsThisAGoodPlaceForAStatueMimic` 符号及 `NPC.cs` call-site 查询 `complete`，各 1 项；callable facts `partial/CalleeEffectsNotExpanded`。`tresspassingDualDungeon` 符号/member-use 查询 `complete`，找到 1 个声明、3 个静态使用点 | Version4 源码 `NPC.cs:1484-1498` 与完整参考 `NPC.cs:1502-1516` 逐条件核对；字段 member-use 中部分 `AccessMode=Unknown`，不证明完整 owner/write closure。两份 helper 源码相同，仍需通过 tile port 表示其读效果。 |
| 静态 `SpawnNPC()` 到 `Main.cs` | 精确 `void()` overload 的选定 call-site 查询 `complete`，返回 1 个 `Main.cs` call site | 不包括动态/反射 caller；与上面的 `Spawner.SpawnNPC()` 实例方法分开解析。 |
| `NPC.SpawnNPC()` 外层步骤 | Version4 源码 `NPC.cs:66509-66520` 确认先检查并清除 `noSpawnCycle`，然后 `RevengeManager.CheckRespawns()`，最后创建 `Spawner` 并进入 pass；完整参考 `NPC.cs:81433-81445` 同序 | CPG 对静态入口到 `Main.cs` 的边为 `complete`；`noSpawnCycle` member-use 查询返回两个写引用但没有完整 read/write closure，`CheckRespawns` 选定 caller 查询为 `partial`/零命中；系统通过端口表达顺序，不代表 adapter/owner 已接通。 |
| `SetSpawnFlags` / `SyncNewlySpawnedNPCs` | 每个方法在所选 `NPC.cs` scope 返回 1 个直接 call site，`complete` | 只确认索引中的静态边；callee 的共享状态、network 和实体效果须读源码追踪。 |
| `CanSpawnEnemiesNear` | 4 个选定静态 call sites，`complete` | `Version4/Main.cs` 3 处、`NPC.cs` 1 处；动态调用范围仍未闭合。 |
| `SpawnFaelings` | 1 个 `MessageBuffer.cs` call site，`complete` | 完整副本对应调用可见于 `MessageBuffer.cs:2223`。 |
| `FindSpawnTile` / `GetSpawnArea` / `HasTileSpawnSpace` | 分别 1 / 3 / 2 个选定调用点，均 `complete` | `GetSpawnArea`、`HasTileSpawnSpace` 还有特殊生成 caller；不展开 callee effects 或闭合 alias。 |
| `CanSpawnInTiles` / `CanSpawnInTile` / `CheckNotSpawningOnScreen` | 分别 1 / 1 / 3 个选定调用点，均 `complete` | 本次 Query API 对 `HasTileSpawnSpace`、`CanSpawnInTiles`、`CanSpawnInTile` 的 Version4 `NPC.cs` shard 分别返回 2/1/1 个静态 call sites，无 call-site gap；三个 callable facts 均为 `partial/CalleeEffectsNotExpanded`（4/29、3/20、4/11 个 CFG/operation 节点），不闭合 callee effects。目标源码与完整参考源码复核确认：footprint bounds 与默认 `WorldGen.InWorld(Rectangle)` 条件一致；扫描顺序为 X 外层、Y 内层；tile predicate 拒绝 active solid 或 lava。`NpcSpawnTileSpaceQuery` 已编码该纯决策；port 现在捕获逐 tile 的 active/solid/lava facts。CPG manifest 的 `SourceSnapshotId=null`；NLTX 仍无生产 tile adapter，实际 facts capture 等价保持 `unknown`。 |
| `SetSpawnFlagsForChosenTile_ForDualDungeon` | 精确 symbol 与选定 `NPC.cs` 单个 call site 查询为 `complete`；callable facts 为 `partial`，7 个 CFG 节点、无直接调用目标，gap 为 `CalleeEffectsNotExpanded` | Version4 方法体为空；调用边不证明有 zone flags 写入。完整参考方法体及 helper 写入闭包通过源码复核。 |
| `NPCSpawningFlagsForDualDungeons` 类型 | 全索引精确类型查询 0 命中、状态 `partial`，gap 为 `NoMatchingFactInScannedScope` | CPG 零命中不证明类型不存在；直接搜索 `Version4/Terraria` 源树未找到该声明，完整参考中存在独立源码文件。 |
| `SyncNewlySpawnedNPCs` | 1 个选定 `NPC.cs` 调用点，`complete` | 两个 Terraria 副本的调用条件不同，需按选定基线验收。 |
| `SpawnOnPlayer` | 使用同一 manifest 对 967 个索引 shard 查询，精确 overload 返回 18 个静态 call sites，`complete`；端点在 `Main.cs`、`MessageBuffer.cs`、`NPC.cs`、`WorldGen.cs` | 仅确认索引中的静态 `CallTargets` 边；不证明动态/反射调用闭合，也不证明特殊入口可共享自然生成 commit。 |
| `NewNPC(IEntitySource,int,int,int,int,float,float,float,float,int)` | 使用同一 manifest 对全部 967 个索引 shard 查询，精确 symbol 的静态调用点合计 158 个，`complete`；端点包括 `Main.cs`、`NPC.cs`、`MessageBuffer.cs`、`WorldGen.cs`、`Projectile.cs`、`Wiring.cs`、`CoinLossRevengeSystem.cs`、`Terraria.GameContent.Events/CultistRitual.cs` 与 `DD2Event.cs` | CPG 只确认选定索引中的静态 `CallTargets`；Version4 `NPC.cs:67051-67097` 仍需作为创建顺序的源码证据。完整参考 `NPC.cs:82017+` 仅在 `Main.netMode == 2` 设置 `spawnNeedsSyncing`，与 Version4 无条件写入不同；创建、slot、target、network 与 persistence owner 仍未闭合。 |
| `GetAvailableNPCSlot` / `IsSpawnSlotInUse` | Version4 `NPC.cs` 选定源文件各有 1 个静态调用点查询命中，均 `complete`；两个 callable facts 均 `partial/CalleeEffectsNotExpanded`，分别为 4/58 与 3/9 个 CFG/operation 节点，均没有直接 CallTargets | CPG 仅确认索引事实；Version4 完整方法体 `NPC.cs:67098-67143` 及完整参考 `NPC.cs:82067-82112` 已回源码核实选择顺序与槽位 in-use 判定相同。CPG source tree 缺失，且 manifest 不绑定当前源码 revision。 |

本轮以同一只读 manifest 重查限定在 `NPC.cs`、`Main.cs`、`MessageBuffer.cs` 的静态调用：`NewNPC`
返回 98 个 call sites（`NPC.cs` 96、`Main.cs` 1、`MessageBuffer.cs` 1），查询状态 `complete`；
该精确 overload 的 callable facts 状态仍为 `partial`，带 `CalleeEffectsNotExpanded` gap；
`SpawnAnNPC` 与 `SyncNewlySpawnedNPCs` 在该范围各返回 `NPC.cs` 1 个 call site。另对 `NPC.cs` 的成员使用
查询得到 `npcSlots` 285 项（230 `Write`、54 `ReadWrite`、1 `Unknown`）、`dontCountMe` 31 项（均为
`Write`）、`spawnNeedsSyncing` 3 项（2 `Write`、1 `Unknown`），查询均 `complete`。这是限定静态索引内的
字段访问分类，不闭合 callee/dynamic effects 或唯一 writer；全索引 `NewNPC` 158 个调用点仍是更广范围的既有结果。

API 包含 `Find-CpgSymbols`、`Get-CpgTypeSurface`、`Find-CpgCallSites`、
`Get-CpgMemberUses`、`Get-CpgCallableFacts`。CPG source excerpt 对 `NPC.cs` 为 `unknown`，
因为 SQLite 旁的 source tree 缺失；所有影响设计的源码关系已回到两个 Terraria 源码目录复核。
索引 `complete` 只代表指定静态查询在其覆盖范围内完成。

### 2.3 已确认且尚未裁决的源码差异

| 差异 | `Version4` | 完整源码参考 | 后续动作 |
| --- | --- | --- | --- |
| Main spawn 调用 | `Main.cs:11454` 位于 try/catch 中；当前局部上下文未显示紧邻的 `netMode != 1` 守卫 | `Main.cs:18050-18055` 调用外有 `netMode != 1` 条件 | Version4 为目标基线；展开两处完整更新方法以确认各运行模式影响，参考守卫仅作为差异，不能自动引入。 |
| 新 NPC 同步调用 | `NPC.cs:251-253` 直接调用同步扫描 | `NPC.cs:251-256` 只在 `Main.netMode == 2` 调用同步扫描 | adapter 默认保留 Version4 调用条件；先追完 net mode、section 与 owner 闭包，若改用参考条件须作为显式行为变更。 |
| per-player 自然生成循环 | 遍历 255 槽并在首个 `TrySpawnAnNPC == true` 后 break | 已检查片段同序 | 保留顺序；还要保留 helper 返回值与实际创建结果的区分。 |
| 双地牢 chosen-tile zone flags | `NPC.cs:341-343` 的 `SetSpawnFlagsForChosenTile_ForDualDungeon` 为空，Version4 Terraria 源树未定义 `NPCSpawningFlagsForDualDungeons`；调用仍在 `NPC.cs:1185`，位于 dual-dungeon 地下区域 `Next(7)` 之后，并受 `!tile.active() || tile.type != 48` 保护 | `NPC.cs:344-360` 以 `NPCSpawningFlagsForDualDungeons.ScanZonesFor(false, ..., true)` 派生、忽略其 bool 返回值并写回 `ZoneDungeon`、`ZoneSnow`、`ZoneGlowshroom`、`ZoneCorrupt`、`ZoneCrimson`、`ZoneJungle`、`ZoneHallow`、`ZoneLihzhardTemple`、`spawnUndergroundDesert`；类型实现见 `NPCSpawningFlagsForDualDungeons.cs:5-240`，caller 位于 `NPC.cs:1203` 且有相同调用保护 | 以 Version4 空方法作为当前目标行为；将完整参考实现移入 NLTX 会补入目标树当前没有的状态写入，须作为显式行为变更裁定。 |
| `GetSpawnRate` 方法体 | `NPC.cs` 文件哈希不同 | Version4 与完整参考项目的对应方法去除空白后长度均为 9,387 字符且逐字符相同 | 仅证明该方法局部文本一致，不绑定 CPG source snapshot，也不证明整文件、运行时或项目级行为等价。 |

任何差异都不能由命名相同、行号对应或“完整副本通过编译”消解。这里不复用旧
Component execution 文档里的历史编译结果作为新 System 批次的验证。

### 2.4 NLTX 下游 owner 检查

`src/NSSLC/Component/Npc` 中现在有 `NpcSpawnAreaQuery`、`NpcSpawnTileSearchSystem`、
`NpcSpawnScreenExclusionQuery`、`NpcSpawnPostCheckSystem` 与 `NpcSpawnChosenTileFlagsSystem`。自然
pass 在 rate roll 成功后取得 area inputs、执行至多 50 轮有序候选搜索，再依次运行 screen exclusion、
chosen-tile post-check 和 flags calculation，之后才把成功结果及 flags 交给
`INpcSpawnPassPort.ContinueSpawnAttempt`；该 `void` handoff 后自然 pass 立即结束玩家循环。
rate/pass/tile/screen/post-check/chosen-tile port 仍只有 verifier fake，没有生产 adapter。未找到把随机、
player、tile 和 event facts 绑定到真实世界的 adapter；NPC creation 或 `SyncNewlySpawnedNPCs` 也没有
adapter/commit 接线。

本轮只读 CPG Query API 对精确 `NPC.UpdateProtectedSpawnSlots:void()` 查询命中一个符号；在全部
967 个索引 shard 上运行 `Find-CpgCallSites` 返回 `complete`、一个 `Main.cs` 静态调用点。
限定 `Terraria/NPC.cs` 的调用点查询为 `partial`、零命中并带局部范围限制，不能据此否定 `Main.cs`
调用或推断无其它调用路径。查询使用 manifest
`6d6bdf09a70e7b9ec3e22e5c4f32aa8d5f447f1a9bde5cd444b9d427a715a364`，`SourceSnapshotId=null`；
完整参考源码不绑定此 CPG 快照。

直接源码核对显示 Version4 `Main.cs:11278` 与完整参考 `Main.cs:17154` 都在各自自然生成调用之前
调用刷新；Version4 `NPC.cs:67041-67049`、完整参考 `NPC.cs:82007-82015` 每 tick 遍历全部 slot，
将 protection 重算为 `Math.Max(active ? 2 : previous - 1, 0)`。Version4 `NPC.cs:67069`、完整参考
`NPC.cs:82035` 在成功选中 slot 后，先写 `spawnSlotProtected[slot] = 2`，再替换并初始化 NPC。
两个 `WorldGen.cs` 又在创建各个 NPC slot 时写 `spawnSlotProtected[slot] = 0`
（Version4 `WorldGen.cs:6642`，完整参考 `WorldGen.cs:7392`）。源码搜索还看到 slot 选择以
`active || spawnSlotProtected > 0` 判定占用。全 967 shard 的 `Get-CpgMemberUses` 返回 aggregate
`complete`、5 条访问（`NPC.cs` 4 条、`WorldGen.cs` 1 条）；每条 fact 都是
`AccessMode=Unknown`、`AccessClassification=NoAssignmentEvidence`、`EvidenceStatus=partial`。
因此 CPG 确认了静态引用范围，却没有闭合读写方向。统一权威 owner、动态调用闭包及 WorldGen/reset
生命周期 owner 仍为 `unknown`；源码可见写入点分布在 tick refresh、NewNPC slot 选择和 WorldGen slot
初始化，不能声称单一 writer 已闭合。

设计上的候选边界由 `NpcSpawnSlotAcquisitionSystem`、`NpcSpawnSlotSelectionSystem` 与
`NpcSpawnSlotProtectionSystem` 组成：前者保持 NewNPC 分配前的 RNG/type/metadata/facts 顺序，纯 Query
决定槽位，Command 写入 protection。它们不另建权威数组；但当前缺少能够同时服务 selection、create/
replacement 和 WorldGen/reset 的 slot storage owner，因此尚未新增生产 adapter 或声称接线完成。
候选 System 的运行时注册、调度与 owner integration review 均未完成。

`WorldStorageRoot.Npcs` 是 `EntitySlotStore<WorldEntityState, NpcSlot>`，提供 slot/state 的
allocate/replace/release 和 generation 记录；`src/NSSLC` 没有调用 `.Npcs.TryAllocate`。该通用
store 本身不构造 NPC 的完整组件状态，也不表明谁负责 spawn admission、target、网络或持久化。
`NpcSpawnSlotSelectionQuery` 只选择显式 slot facts 中的索引，不分配 `EntitySlotHandle`，也不调整
`spawnSlotProtected` 或 population accounting；上游 metadata/facts capture 及与实际 commit 的原子边界仍为
`unknown`。
另有 `src/NSSLC.Infrastructure/EntityLifecycleAttribution/EntitySpawnCommitSystem`，其 NPC 分支只将
既有 `RuntimeEntityId` 交给 `NpcEntitySlotStore.TryAllocate` 取得 slot handle；全仓没有找到它的构造
或调用点，也没有从它到 P14 continuation 的关系。它不建立完整 NPC entity/component 状态，不能当作
自然生成 commit owner。
本轮新增的 `NpcSpawnEntryResult`、`NpcSpawnPassResult` 和 `NpcSpawnCreationObservation` 不写入上述
owner；它们是 attempt-local 观察值，`Unknown` 明确表示当前 port 没有实体创建结果契约。
`NpcLifecycleComponent` 的可见 transition 由 `NpcDeathLifecycleSystem` 用于 despawn；没有找到
自然生成路径构造它的 caller。`SpawnAdmissionState` 在 P14 pass 中也无使用点。故不得把上述
类型拼成推测的 commit owner；tile 到 spawn/sync 的跨域关系保持 `unknown`，B1/B4 未关闭。

## 3. 开始代码批次前必须关闭的决策

以下决定有任一未关闭时，依赖它的接入任务保持 `blocked-by-unknown`，其它不依赖它的源码
分析或纯文档工作可以继续：

| ID | 决策 | 最小证据与结果 |
| --- | --- | --- |
| `P14-DEC-01` | 固定 Version4 目标快照，并裁定哪些完整参考差异要作为有意的目标行为变更 | 两边 revision/hash、完整调用上下文、差异清单；默认保持 Version4，新增/不同语义须单独裁定。 |
| `P14-DEC-02` | `Main` spawn tick、slot-protection tick refresh、`noSpawnCycle` 和 revenge respawn 的现有调度 owner | 入口和 refresh 相对 spawn pass 的时序、调用范围、置位/清零、world/session 生命周期与唯一写者。 |
| `P14-DEC-03` | slot protection、`npcSlots`、`dontCountMe`、nearby/active NPC 与 invasion cap 的唯一权威 owner | NPC create/replacement/despawn 和 WorldGen/reset 全部写点及计账 API；`crossSubsystemOwner: integration-review`。 |
| `P14-DEC-04` | `SpawnAnNPC` 实际调用的创建 API、一次尝试可能产生的 entity 集合及失败语义 | 每类事件/biome/tower 分支的直接创建、return、slot 与 effect 闭包。 |
| `P14-DEC-05` | 自然生成、`SpawnFaelings`、`SpawnOnPlayer` 的共用或分离 commit API | 独立 entrypoint、唯一性策略、target/announce 顺序；不得由名称推断可以合并。 |
| `P14-DEC-06` | `SyncNewlySpawnedNPCs` 的 authority、net mode、section 与 `spawnNeedsSyncing` 所有者 | Network/section writer、消息 type 23、重试和提交顺序；`integration-review`。 |
| `P14-DEC-07` | `defaultTarget`、legacy player/NPC index 与实际 target writer 的映射 | 转换规则、失效和网络更新来源；`integration-review`。 |
| `P14-DEC-08` | `safeRangeX/Y`、tile snapshot、random source 的读取 API 及版本一致性 | 证明 Query 可重排/重复，或将其留在 effectful coordinator/adapter。 |
| `P14-DEC-09` | event/day/cooldown/town/biome 数据由哪些 owner 提供、何时 reset | 读写/生命周期/存档恢复闭包；`integration-review`。 |

## 4. 分阶段实施计划

### Phase 0：基线绑定与范围确认

**目的：** 使源端事实能绑定到具体源码，而不是把两个不同文件树合并成一个版本。

**工作：**

- 固定 `Version4` 目标 revision 和完整源码参考 revision/hash；记录项目配置、条件编译符号与
  文件哈希。行为基线按用户指示为 Version4。
- 对 `Main` 调用守卫、TrySpawn 后同步、特殊入口和 P14 关键方法逐项比较两份源码；把差异
  标为行为差异候选或纯版本差异，不能静默归一化；完整参考的双地牢 flags 写入不得未经裁定导入。
- 按 Version4 确认此次实际迁入 `src/NSSLC` 的目标语义、server/client 范围和兼容入口。

**退出条件：** `P14-DEC-01` 已裁定，来源差异有逐项 owner/决定/待办；仍有影响主路径的
`unknown` 时，后续只允许做不依赖该差异的设计工作。

### Phase 1：写入闭包与跨系统 API 定案

**目的：** 在任何新 System 接触权威状态前，确认谁提供输入、谁提交状态。

**工作：**

- 按 P14 十二组追踪读者、写者、构造、reset、销毁、网络与存档用途；从源成员延伸到相邻
  lifecycle、population、event、target 和 network owner，但不把相邻成员移入 P14。
- 使用源码声明/实现/调用点及 CPG Query API 持续补证；CPG partial、零命中、callee effect
  未展开和完整副本未绑定都保留为 `unknown`。
- 对 `NpcSpawnSystem`、NPC creation/lifecycle、slot accounting、target、network、persistence
  列出输入、输出、失败、重复请求及可见时点；拿不到真实 API 时不编造 port 名或调用顺序。
- `NpcSpawnEligibilityQuery` 已实现 Version4 `CanSpawnEnemiesNear` 的纯资格门；它只接收每次尝试快照，
  不负责捕获 Player/Creative Power/NPC proximity 输入，也未接入调度入口。

**退出条件：** 所有接入 API 至少有明确 source/target owner、输入身份和副作用契约；未确认
项被隔离在独立批次，不做双写。

### Phase 2：一次性输入值与可证纯规则

**目的：** 形成能够独立审查的 attempt-local 数据边界，不把筛选快照变成长期实体状态。

**工作：**

- 核对当前 `NpcSpawnContextSnapshot`、spatial/biome/policy/event snapshots 与实际 C# 类型、
  构造来源和项目引用；它们当前是 partial/source-only 材料。
- 仅把值对象作为每次 spawn attempt 的输入/结果；记录 player/entity identity、world/tile
  revision、capacity/event revision 和何时失效。没有可靠 revision API 就标 `unknown`，不要
  自创与真实写者不相连的 version number。
- 对已闭合的纯谓词做小范围 Query 设计：输入不可变值、返回 decision/reason；不接 `Main`、
  `Main.rand`、Tile storage、clock、logger、network、存档或 mutable cache。
- 自然生成路径现由 `NpcSpawnAreaQuery` 显式返回 spawn/safe area 与 safe extents，不修改旧
  `GetSpawnArea`；继续确认 `SpawnFaelings`、`SpawnOnPlayer` 等 caller 是否可以迁移，未确认前保留旧 helper。

**退出条件：** 每个抽出的 Query 有纯度与重复调用契约；其余规则继续留在 coordinator 中，
不为了形成接口而拆分。

### Phase 3：自然生成协调 System 与兼容入口

**目的：** 在一个 owner 内保留自然生成调度、逐玩家尝试、tile 候选和随机消费顺序。

**建议领域落点：** 先沿当前项目组织放在 `src/NSSLC/Component/Npc/`，例如一个候选
`NpcSpawnSystem`；文件名、注册方式和 namespace 必须在 Phase 1 按实际项目验证。不要先建空的
`Systems/`/`Queries/` 通用目录，也不要一次为十二个叶子组创建十二个类型。

**工作：**

- 旧 `NPC.SpawnNPC()` 入口暂由兼容 Adapter 调用候选 System；直到新路径真实接入并有行为证据，
  不删旧 facade 或旧 writer。
- 已在 `NpcSpawnSystem` 增加 `ProcessEntry(INpcSpawnEntryPort)`，保留 Version4 的
  consume-no-cycle、respawn check、natural pass 顺序；`INpcSpawnEntryPort` 目前没有生产实现，因此
  该组合是可验证的隔离 core，不是已接入的兼容 Adapter。`noSpawnCycle` 字段 writer 和 respawn owner
  仍由外部 owner 持有。
- 按已裁定基线维持 `noSpawnCycle`、`RevengeManager.CheckRespawns()`、255 player slot 顺序、
  slime-rain 触发和 first-true break；对版本不同的 net-mode/catch 上下文显式处理。
- `NpcSpawnPerPlayerFlagsSystem.Prepare(playerIndex, INpcSpawnPerPlayerFlagsPort)` 表达旧
  `SetSpawnFlags(player)`：在可选 slime-rain effect 后、rate/capacity/rate-roll 前依序捕获
  prelude、invasion 输入、postlude 和 rate 输入。Version4 与完整参考源码确认 invasion
  eligibility 可按 NPC slot 顺序消费 RNG；`Calculate` 返回 attempt-local rate snapshots，不写共享
  legacy 字段。这和 tile 命中、post-check 之后的 `SetSpawnFlagsForChosenTile` 分开；生产 capture
  adapter/runtime caller 仍缺失。House-wall capture 必须保留源码 `WorldGen.InWorld` 短路边界，
  实际 tile 索引/墙读取映射尚 `unknown`。
- 已增加 `NpcSpawnChosenTileFlagsSystem.Calculate`，在 post-check 通过后读取 chosen tile、竖向液体、
  附近 marble/granite、蜘蛛墙、地下沙漠墙、ocean-depth 与 world flags，并将完整
  `NpcSpawnChosenTileFlagsResult` 放入 `NpcSpawnAcceptedCandidate`。此 System 经 port 消费随机数及
  Tile facts，保留源顺序和短路，不当作可重复调用的纯 Query。当前只有隔离 core，没有生产 adapter。
- 两份 `NPC.cs` 的 `SetSpawnFlagsForChosenTile` 主体（Version4 `NPC.cs:952-1189`、完整参考
  `NPC.cs:970-1207`）逐段核对一致；双地牢子 helper 不同：Version4 方法体为空，完整参考调用
  `NPCSpawningFlagsForDualDungeons.ScanZonesFor` 后写回 9 个 Zone flags。当前仍按 Version4 基线不实现
  这组额外写入；若纳入完整参考行为，须先作为显式行为变更并闭合 tile/wall 输入与后续分支关系。
- 已建立 `NpcSpawnRateSystem.Calculate`：显式接收 context/policy/biome/event/player/world
  快照，通过 `INpcSpawnRateRandomPort` 在对应条件处消费 town-NPC 和 luck 随机数。当前 pass 在
  slime-rain effect 和 per-player flag preparation 之后 capture/calculate rate，之后执行
  capacity gate 和 rate roll；该输入仍
  只由 verifier port 提供，不能视为生产 adapter 或完整的 RNG 等价证明。
- rate roll 命中后由 `NpcSpawnAreaQuery.Calculate` 计算 attempt-local 区域，再由
  `NpcSpawnTileSearchSystem.Find` 按 legacy 顺序逐次请求 x/y 与 near-sky 随机值、读取 solid/wall/tile
  facts；保留 50 次上限、ground scan、footprint gate 和 sticky `SkyMob` 输出。候选命中后，当前
  coordinator 用 area 的 safe extents 与 screen-player snapshots 运行 screen exclusion；通过后
  capture chosen-tile facts、运行 post-check 与 chosen-tile flags System，再把结果交给 continuation。
  `NpcSpawnAcceptedCandidate` 传递当前 captured rate inputs/result、tile-search result、post-check
  facts 与完整 `ChosenTileFlags`；`NoWormsForSpawn` 派生自其中的 `NoWorms`。这不是 commit API。
  真实 player/tile/event/random adapter、NPC commit 和 sync 仍未接通。
- 不批量预抽、并发候选、缓存过期空间规则或重排拒绝次序。
- `NpcSpawnTileSpaceQuery` 按 Version4/完整参考拒绝 active solid 或任意 lava；边界条件与
  `WorldGen.InWorld(Rectangle)` 对齐。未来生产 adapter 仍须证明其 active、solid 与 lava
  facts 正确映射 `Tile.nactive()`、`Main.tileSolid[type]` 和 `Tile.anyLava()`；fixture 不证明该映射。
  house-wall、随机与 world-bound 输入也没有生产 adapter。
- Version4 `NPC.cs:5330-5376` / `WorldGen.cs:8962` 与完整参考 `NPC.cs:5348-5394` /
  `WorldGen.cs:9943` 的 tile-footprint 关系已回源码核对：`WorldGen.InWorld(Rectangle)` 使用
  默认 `fluff=0`，边界拒绝条件为 `Left/Top < 0` 或 `Right/Bottom >= maxTiles`；
  `CanSpawnInTiles` 按 X 外层、Y 内层扫描并遇首个 false 短路，`CanSpawnInTile` 拒绝
  `nactive() && tileSolid[type]` 或 `anyLava()`。当前 `NpcSpawnTileSearchSystem` 的边界条件、
  矩形扫描次序与该源码相符；`INpcSpawnTileSearchPort.CaptureTileSpaceFacts` 仍没有生产 adapter，
  所以 NLTX 实际 tile/liquid 映射保持 `unknown`，verifier fake 不作运行时等价证据。
- 保留 `TrySpawnAnNPC` 的外部 loop-control 语义。true 仅用于现有循环退出；需独立记录实际
  创建集合，不能从返回值推导恰好一只新 NPC。
- `ProcessNaturalSpawnPassDetailed` 用 `NpcSpawnPassResult` 记录 loop-control player index；
  `void` continuation 被调用后记录 `CreationObservation.Unknown`，没有候选时为 `NotRequested`。
  `ProcessEntryDetailed` 另外记录 `ConsumeNoSpawnCycle` 是否消费以及 respawn check 是否执行，
  不把这些观察字段提升为 runtime state 或 spawn commit。
- 通过候选后，`NpcSpawnAcceptedCandidate` 将 player index、rate input/result、tile-search result、
  post-check facts 与 chosen-tile flags 一并交给 continuation，避免未来接线再从共享可变 world state
  重新读取这些已捕获值。`NoWormsForSpawn` 来自 flags 结果，其中保留 Version4 与完整参考共有的
  `!SkyMob && NoGroundWorms => noWorms = true` 规则。该 handoff 仍缺生产输入 adapter、创建分支和
  lifecycle/slot owner，不能直接执行完整 `SpawnAnNPC` 语义。

**退出条件：** 一次 per-tick attempt 的状态/effect trace 与选定源码基线可比较；单一自然生成
协调 writer 已确认；特殊生成、network 或 target owner 未接通前不会被该批次代写。

### Phase 4：NPC 创建、人口与网络交接

**目的：** 让 accepted candidate 经唯一既有 NPC lifecycle/population 边界提交，再投影网络与
存档结果。

**工作：**

- 等 `P14-DEC-03/04/06` 关闭后，再将分支选择结果交给实际创建/lifecycle/slot API；不得由
  `SpawnAdmissionState` 或 snapshot presence 推断提交 API 已存在。
- 映射 `SpawnedFromStatue`、replacement、release owner、NPC activation 和 `npcSlots` 变化；将
  `dontCountMe` 排除规则并入唯一人口 owner。
- 将 slot protection 的 tick refresh、selected-slot mark 和 WorldGen/reset clear 汇入一个可追踪的
  authoritative state owner；保持刷新在 spawn pass 前、选中槽标记在实体替换/初始化前的源码顺序。
  当前跨 `Main`、`NPC.NewNPC`、`WorldGen` 的 owner 与生产适配仍为 `unknown`，不得开启 shadow writer。
- 记录单次 `SpawnAnNPC` 产生的零/一个/多个 entity 结果、失败和遗留状态。旧 `void` 边界若
  不报告结果，则 adapter 需要由真正的 commit owner提供可观察结果，不在 P14 伪造 success。
- 在已裁定的 server/net mode 下接入同步投影；验证 section visibility、message type 23、顺序、
  重试和 `spawnNeedsSyncing` 清理。持久化边界由真正 owner 决定。

**退出条件：** 唯一 lifecycle/slot writer 有静态与运行证据；提交与 network/persistence 的
顺序、失败、重放和结果 cardinality 有测试覆盖。

### Phase 5：特殊入口与 target 交接

**目的：** 独立整合非自然生成路径，不破坏消息触发、boss/event 唯一性与 target 语义。

**工作：**

- 分开追踪并适配 `SpawnFaelings`、`SpawnOnPlayer` 与 Main 中各特殊 boss/event 调用。
- 保留各自的 AnyNPCs 检查、位置搜索、失败返回、announcement、target 指派和网络次序；仅在
  API/commit 行为证据证明相同时共享底层步骤。
- `defaultTarget` 保留 legacy sentinel `255`，转换至实体 identity/target handle 的映射须由
  target owner 提供；P14 的 Query 只产生 target value，不直接改 target component。

**退出条件：** 每类特殊入口均有 source-to-API 映射和独立行为用例；无特殊路径绕过唯一
entity creation、target 或 network owner。

### Phase 6：回归、行为验收与旧入口裁定

**目的：** 以真实结果证明新组合满足选定 Version4 行为，而不是以静态映射或编译替代。

**观察向量：**

```text
return/error
authoritative state delta
created entity set and identity
events and external effects
random draw sequence/count
ordering and same-tick visibility
lifecycle and population accounting
target/network/persistence projections
retry, duplicate and failure behavior
```

**场景至少包含：**

- 255 player 槽顺序、首个可尝试/首个 true、inactive/dead/Journey suppression。
- capacity 边界、spawn rate 与整数舍入、boss/invasion/event cap、`npcSlots` 与 `dontCountMe`。
- 50 次 tile 搜索边界、solid/wall/safe/screen、液体、Dungeon/dual-Dungeon、remix、ocean/beach、
  sky、biome、tower/event 的拒绝和接受分支。
- 固定 random seed/记录的 RNG 输入，验证短路情况下 draw 次序与数量。
- tower/事件分支的创建数量；spawn commit 失败/槽位耗尽；已有 pending sync NPC 的扫描范围。
- `SpawnFaelings`、各 `SpawnOnPlayer` 调用源、server/client/single-player、重复请求和异常捕获。
- target sentinel、legacy index、entity invalidation、network section 和 save/load/recovery。

**退出条件：** 必需行为差异均有批准说明或差分通过；自动门禁和真实入口均指向新 System；
无未处理动态/配置入口。仅在上述证据完备后进入项目级 migration-success / 旧实现删除评审。

## 5. 批次状态与门禁

| 批次 | 内容 | 进入条件 | 验收产物 | 本轮状态 |
| --- | --- | --- | --- | --- |
| `P14-B0` | 源码 revision/hash 与关键路径差异核对 | 当前已可读源树 | source baseline matrix、差异项和权威语义裁定 | 证据已收集；差异裁定待完成。 |
| `P14-B1` | owner 与 API 写入闭包 | `B0` 有选定基线 | owner/handoff 表、读写和生命周期关系、失败/重复策略 | `blocked-by-unknown`。 |
| `P14-B2` | ephemeral snapshot 与纯规则设计 | 只依赖已闭合输入 | Query contract、snapshot revision/失效规则、纯度证据 | `partial`；资格、rate、area、screen exclusion、slot-selection、slot sentinel compatibility、slot-protection command、tile-space predicate、replacement expected-generation 传递与 final-type definition lookup core 已存在；`NpcSpawnSlotSelectionSystem` 组合 slot Query 和 selected-slot protect command，`NpcSpawnSlotAcquisitionSystem` 组合类型改写、metadata 顺序及 slot facts handoff。生产输入 adapter、source revision binding 和运行时证据仍未闭合。Effectful tile-search、post-check、chosen-tile flags 与 slot-protection writes 不声明为纯 Query。 |
| `P14-B3` | 自然生成 coordinator 与 legacy facade 接入 | `B1` 决策支持单一 owner，`B2` 契约确定 | source mapping、单 writer、随机和顺序 trace | `partial`；isolated coordinator 覆盖 eligibility、slime-rain 后的 per-player flag preparation、rate capture 到 chosen-tile flags 顺序；slot-selection query、replacement generation handoff、final positive type/negative net-ID definition lookup、纯 sentinel mapping、GoodWorld→`FromNetId`→metadata→facts 获取，以及 slime variant/target preparation 到 slot protection 的 pre-commit trace 有 focused fake-port 断言；`NpcSpawnAcceptedCandidate` 传递 captured attempt facts 与完整 flags。Tower、SkyMob、Invasion、Graveyard/dual-dungeon 与 type 244 critter branch selector 均只表达 pre-commit 结果；此前 `--spawn-branch-selection-core-checks-only` 的六个 fake-port 场景通过，Tower 原有 Solar focused verifier 另有历史记录。新增 critter selector 已通过 NPC 生产项目编译，新增 verifier 代码已编译但未运行；selector 尚未接入 continuation。生产 adapter、真实 slot facts、实体 commit/sync、旧入口 facade 与 runtime caller 仍为 `blocked-by-unknown`。 |
| `P14-B4` | creation/lifecycle/slot/network/persistence handoff | 对应 owner API 与副作用闭包已确认 | 单一 commit 链、创建结果、网络/存档投影证据 | `blocked-by-unknown`；`INpcSpawnSyncPacketPort` 与 `INpcReplicationPacketApi` 保持函数声明；仅后者的 request/result 是空类型声明。按范围不编写 packet 编解码、recipient selection、publish/send 逻辑。commit、lifecycle、slot 和 caller 接线未实现。 |
| `P14-B5` | special spawn/target 路径 | 特殊 API 与 owner 定案 | per-entrypoint mapping 与 target/network 行为证据 | `blocked-by-unknown`，未实现。 |
| `P14-B6` | 行为差分与旧入口最终裁定 | 新路径真实接入；全部必需行为可运行 | build/test/verifier/trace 命令与观察向量记录 | `not-run`。 |

未来每个包含代码的批次都应按仓库构建约束串行运行受影响项目构建和 focused behavior
verification，并记录精确命令、配置、退出码及 `Build/` 输出路径。任何 gate 没运行或失败，
批次都不得标为完成。本轮单项目核心验证不覆盖真实运行时接入；完整 System 接入、Version4 runtime
行为差分、网络及存档验证仍为 `not-run`，不能据此宣称迁移成功。

## 6. 失败处理、兼容与回滚

- 源文件 hash 或 assembly/source revision 改变时，回到 Phase 0；旧 CPG 结果只能留作旧快照证据。
- 出现新 writer、动态 caller、world/client 作用域或特殊 spawn 分支时，更新证据 gap 与 API
  映射，回到对应 owner/设计 gate，不把它折叠成“边缘情况”。
- 当前 writer 尚未关闭时，不能并行运行 shadow writer；避免新旧路径同时创建 NPC、扣 slot、发包。
- 保留旧 facade 和入口兼容路径，直到最终行为 gate 满足；回滚必须切回一个 writer，不保留
  双写状态。
- 测试失败要保存输入、seed、状态快照与 effect trace，并先确认失败来自规则差异、来源版本、
  owner API 还是调度顺序；不能通过减少测试场景消除未覆盖范围。

## 7. 最终交付检查

- 12 个 P14 leaf group 的所有实现、测试和证据都仍能追溯到 117 成员 report。
- `NpcSpawnSystem` 的实际注册和入口命中已证明；candidate type 名和文件存在不算接入。
- Query、随机/tile 候选、commit、entity lifecycle、人口、target、网络和 persistence 各有明确
  写边界；跨分区 owner 已由 integration review 裁定。
- 源码 baseline、CPG manifest、工具/配置、代码 revision 和验证输入属于同一可复核证据链。
- build、focused verifier、行为比较、失败/重放及网络/存档场景均有实际执行证据。未执行项目
  继续标 `not-run`；缺失项继续标 `unknown`。
- 只有项目级验收 gate 完整后，才能另行决定 `migration-success` 或删除旧实现；本计划和旧
  P14 runner `Complete` 都不产生该结论。
