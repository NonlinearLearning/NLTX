# 继承任务：Pyramid `Framing.WallFrame` evaluation / typed commit 收尾

**Repository:** `D:\TRbackup\NLTX`  
**Flowstate:** `N6 / in_progress_with_deferred_findings`  
**任务类型:** 跨会话 continuation；不要把本文件当作完整 Terraria parity 的放行凭证。  
**目标批次:** `P9-worldgen/pyramid-wall-frame-evaluation-20260831-08`  
**前置批次:** `P9-worldgen/pyramid-wall-frame-value-mutation-20260831-01`（历史证据，不是当前 coherent gate）

## 接手时的真实状态

08 目录保存了当前树上的 WallFrame 实现与串行构建证据。当前已观察到：

- `Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-wall-frame-evaluation-20260831-08/` 中的 Pyramid focused verifiers、Pyramid full verifier、Simulation、Server、WorldGeneration verifier、Main inventory、style check 和 bounded verifier 均已有绿色日志；bounded 结果为 `PASS=280 CHECK=40 ANCHORED_FAIL_ERROR=0`。
- 08 目录的 `git-diff-check.log` 已由真实 `git diff --check` 生成，退出码为 `0`；输出中的 `LF will be replaced by CRLF` 是 Git 行尾提示，不是 diff failure。
- `model-context.json` 与 `checkpoint.json` 的稳定字段应为 `currentNode=N6`、`status=in_progress_with_deferred_findings`、`canRemoveLegacyWorldGen=false`、`serverRelevantDeferredCount=44`，active batch 为 `Pyramid Framing.WallFrame value/mutation completed_partial`。
- 在本次交接前的多次读取中，两个 state 文件曾被后台 continuation 从 08 写回 `P9-worldgen/pyramid-wall-frame-value-mutation-20260831-01`；因此 08 的旧 `model-context-json-parse.log`、`checkpoint-json-parse.log` 和 `state-invariant-check.log` 可能是 `exit=1`。必须重新解析并在连续读取后确认没有再次回退，不能只看本文件或旧日志。
- `docs/flowstate/task/2026-08-22-server-ecs-convergence.md` 仍可能保留旧的 01 证据句；更新前先区分历史叙述与当前边界，不要删除历史失败证据（特别是 `...-03` 的 stale-fixture failure）。

## 已存在的 source-backed owner

以下文件属于已经实现的 WallFrame value/mutation slice，接手者应先确认存在，再决定是否需要重新构建；不要重新设计 owner 或改变 `WorldTile` ABI：

- `src/Terraria.Dome.Simulation/WorldGeneration/LegacyTruncatingWallTileRegistry.cs`
- `src/Terraria.Dome.Simulation/WorldGeneration/LegacyWallFrameNeighborMask.cs`
- `src/Terraria.Dome.Simulation/WorldGeneration/LegacyWallFrameNeighborResult.cs`
- `src/Terraria.Dome.Simulation/WorldGeneration/LegacyWallFrameNeighborQuery.cs`
- `src/Terraria.Dome.Simulation/WorldGeneration/LegacyWallFrameOffset.cs`
- `src/Terraria.Dome.Simulation/WorldGeneration/LegacyWallFrameLookupRegistry.cs`
- `src/Terraria.Dome.Simulation/WorldGeneration/LegacyWallFrameEvaluationResult.cs`
- `src/Terraria.Dome.Simulation/WorldGeneration/LegacyWallFrameEvaluationQuery.cs`
- `src/Terraria.Dome.Simulation/WorldGeneration/LegacyWallFrameCommand.cs`
- `src/Terraria.Dome.Simulation/WorldGeneration/LegacyWallFrameCommitResult.cs`
- `src/Terraria.Dome.Simulation/WorldGeneration/LegacyPyramidWallFrameCommandProjection.cs`
- `src/Terraria.Dome.Simulation/WorldGeneration/LegacyWallFrameCommandCommitSystem.cs`
- `src/Terraria.Dome.Simulation/World/WorldTile.cs`

`WorldTile` 当前保留独立的 `FrameX`/`FrameY`、`WallFrameNumber`、`WallFrameX`/`WallFrameY`。此前完整构造函数为 24 个 positional parameters；任何 ABI 改动都必须先按 `AGENTS.md#dotnet-build-concurrency-contract` 串行重建 Simulation 与所有 dependent verifier，并比较 root/copy Simulation DLL。

## 已验证的窄边界

当前实现/证据覆盖：

- truncating wall 集合 `54, 328, 459, 748`；cardinal mask 位序 `Above=1, Left=2, Right=4, Below=8`；显式 invisible-wall 开关。
- `WallID.Count=367` 的 invalid-wall normalization；zero/invalid wall 的 paint/coating 清理。
- phlebas/lazure/20x4 lookup、full-mask center offset、ordinary reset RNG、wall 21 的第二次 `Next(2)` 与 forced frame `2`、large-frame wall 不消耗 RNG、non-reset `WallFrameNumber & 3`。
- 独立 wall frame 坐标写入，同时保留 tile-object `FrameX`/`FrameY`。
- Pyramid 完整 3x3 envelope、x-major 顺序、source metadata、duplicate target、mask/lookup 一致性和 typed command atomic commit。

Legacy source authority hashes：

```text
Framing.cs  93EDE9CEC249AC2E30FA580BBA35FA9E1311AE872912BB186403E9B0FC85F97A
WorldGen.cs C7C2F2196CEA0F6E56C20863A27824917391B74B34232D50590D1DDF4FF321AC
TileID.cs   686F5340E5C71F940BCB3C5E30AB9125D032BAFDE991A34129FAB26662528296
WallID.cs   B03BCF66A8C5D496FC8720FDB2C8AD918CEB4B6B4321AD874A3253A75A86B38C
```

## 接手执行顺序

### 1. 先读取入口和 state，不要从旧聊天推断完成度

从 repository root 执行：

```powershell
Get-Content -Raw .\AGENTS.md
Get-Content -Raw .\progress.md
Get-Content -Raw .\docs\flowstate\README.md
Get-Content -Raw .\docs\flowstate\plan\2026-08-22-server-ecs-convergence.md
Get-Content -Raw .\docs\flowstate\task\2026-08-22-server-ecs-convergence.md
Get-Content -Raw .\.agent-workplace\state\model-context.json
Get-Content -Raw .\.agent-workplace\state\checkpoint.json
```

随后只打印关键字段，并连续读取两次（间隔数秒）：

```powershell
$m = Get-Content -Raw .\.agent-workplace\state\model-context.json | ConvertFrom-Json
$c = Get-Content -Raw .\.agent-workplace\state\checkpoint.json | ConvertFrom-Json
[pscustomobject]@{
  ModelBatch = $m.worldgenPyramidWallFrameEvaluationBoundary.batch
  CheckpointBatch = $c.worldgenPyramidWallFrameEvaluationBoundary.batch
  ModelActive = $m.activeBatch
  CheckpointActive = $c.activeBatch
  ModelStatus = $m.status
  CheckpointStatus = $c.status
  ModelNode = $m.currentNode
  CheckpointNode = $c.currentNode
  Deferred = $c.serverRelevantDeferredCount
  DeleteGate = $c.canRemoveLegacyWorldGen
} | Format-List
```

若再次看到 01，先记录为 state race；不要覆盖 08 失败历史日志，也不要把 01 路径报告为当前 08 gate。

### 2. 做编译进程 preflight，必要时恢复 state 后再验证

在任何可能编译的命令之前：

```powershell
Get-CimInstance Win32_Process |
  Where-Object {
    $_.Name -eq 'csc.exe' -or
    ($_.Name -eq 'dotnet.exe' -and $_.CommandLine -match
      '(?i)\b(restore|build|rebuild|test|run|publish|pack|watch|msbuild)\b')
  } |
  Select-Object ProcessId, ParentProcessId, Name, CommandLine
```

不要终止 owner 不明的进程。所有后续 `dotnet` 命令必须通过 `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1`，并使用 `-m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`。

### 3. 只在确认 state 可控后把当前边界统一到 08

使用 `apply_patch` 更新 `.agent-workplace/state/model-context.json`、`.agent-workplace/state/checkpoint.json`（以及需要同步的正式 plan/task/progress 文本），至少保证：

```text
currentNode=N6
status=in_progress_with_deferred_findings
worldgenPyramidWallFrameEvaluationBoundary.batch=P9-worldgen/pyramid-wall-frame-evaluation-20260831-08
coherentGate 或等价当前 gate 指向 .../pyramid-wall-frame-evaluation-20260831-08/
activeBatch=Pyramid Framing.WallFrame value/mutation ... completed_partial
canRemoveLegacyWorldGen=false
serverRelevantDeferredCount=44
```

checkpoint 的该 boundary verification list 应指向 08 目录的 20 个 evidence entries，而不是旧 01 的 `*-rerun-current` 列表。修补后连续读取两次；若外部 continuation 立即回退，保留回退证据并把任务状态留在 `in_progress_with_deferred_findings`，不要宣称 gate 已稳定。

### 4. 重新生成 state parse、统一不变量和 scoped artifact audit

08 目录中应 fresh 生成并写入：

```text
model-context-json-parse.log/.exit
checkpoint-json-parse.log/.exit
state-invariant-check.log/.exit
artifact-membership-audit.log/.exit
```

解析检查必须报告 model/checkpoint 的 node、status、batch、active batch、44 和删除闸门。artifact audit 的范围仅包括：

1. checkpoint 当前 08 verification list 中的日志/summary 是否存在；
2. 上述全部 WallFrame typed owner（`WorldTile.WallFrameNumber/X/Y` 需在 `WorldTile.cs` 中进行符号存在性检查）；
3. 当前 research、design plan、execution plan、flowstate plan/task 和 `progress.md`；
4. `checkpoint.artifacts` 的历史条目。对于 `path|sha256=...`，先剥离 `|sha256=...` 再检查路径；历史缺失条目必须单独计数为 `historicalIgnoredMissing`，不得把它们冒充当前 08 batch failure。

audit 完成后必须有一行类似：

```text
PASS scoped artifact membership: current08Missing=0 ownerMissing=0 docsMissing=0 metadataStripped=4
historicalMissing=... (ignored; outside current 08 scope)
```

### 5. 重新跑文档和 diff gate

```powershell
& .\Build\Tools\GenerateFlowstateManifest.ps1 *> .\Build\diagnostics\server-ecs-convergence\P9-worldgen\pyramid-wall-frame-evaluation-20260831-08\flowstate-manifest-generation.log
$LASTEXITCODE | Set-Content .\Build\diagnostics\server-ecs-convergence\P9-worldgen\pyramid-wall-frame-evaluation-20260831-08\flowstate-manifest-generation.exit

& .\Build\Tools\VerifyFlowstateDocs.ps1 *> .\Build\diagnostics\server-ecs-convergence\P9-worldgen\pyramid-wall-frame-evaluation-20260831-08\flowstate-docs-verification.log
$LASTEXITCODE | Set-Content .\Build\diagnostics\server-ecs-convergence\P9-worldgen\pyramid-wall-frame-evaluation-20260831-08\flowstate-docs-verification.exit

git diff --check *> .\Build\diagnostics\server-ecs-convergence\P9-worldgen\pyramid-wall-frame-evaluation-20260831-08\git-diff-check.log
$LASTEXITCODE | Set-Content .\Build\diagnostics\server-ecs-convergence\P9-worldgen\pyramid-wall-frame-evaluation-20260831-08\git-diff-check.exit
```

本文件加入 `docs/` 后，manifest 的 docs 数量会变化；以本次脚本输出为准，不要硬编码旧的 `DOCS=472`。文档 verifier 必须以 exit `0` 且 `docs=manifest` 为准。

### 6. 扫描 08 证据和 dependent DLL

```powershell
$dir = '.\Build\diagnostics\server-ecs-convergence\P9-worldgen\pyramid-wall-frame-evaluation-20260831-08'
rg -n 'MSB3061|MissingMethodException|^(FAIL|ERROR):|pyramid-wall-frame-value-mutation-20260831-01|pyramid-wall-frame-evaluation-20260831-0[1-7]' $dir
Get-ChildItem $dir -Filter '*.exit' | Sort-Object Name |
  ForEach-Object { "$( $_.Name )=$((Get-Content -Raw $_.FullName).Trim())" }
```

上述 `rg` 应无输出；所有列入当前 verification list 的 exit 应为 `0`。最后比较：

```powershell
Get-FileHash .\Build\bin\Terraria.Dome.Simulation\Release\net10.0\Terraria.Dome.Simulation.dll
Get-FileHash .\Build\bin\Terraria.Dome.PyramidStructure.Verification\Release\net10.0\Terraria.Dome.Simulation.dll
Get-Item .\Build\bin\Terraria.Dome.Simulation\Release\net10.0\Terraria.Dome.Simulation.dll,
  .\Build\bin\Terraria.Dome.PyramidStructure.Verification\Release\net10.0\Terraria.Dome.Simulation.dll |
  Select-Object FullName, LastWriteTime, Length
```

若 hash 不一致或发现 stale dependent DLL，按串行 contract 重建 Simulation，再重建 Pyramid verifier，使用 `--no-build` 运行 focused verifier；不要依赖旧的绿色日志。

## 重新构建时的项目顺序（仅在证据不可信时）

保持单 critical section，按依赖顺序一次只运行一个命令：

1. `src/Terraria.Dome.Simulation/Terraria.Dome.Simulation.csproj`
2. `Test/Terraria.Dome.WorldGeneration.Verification/Terraria.Dome.WorldGeneration.Verification.csproj`
3. `Test/Terraria.Dome.PyramidStructure.Verification/Terraria.Dome.PyramidStructure.Verification.csproj`
4. `src/Terraria.Dome.Server/Terraria.Dome.Server.csproj`
5. `Test/Terraria.Dome.MainFieldPropertyInventory.Verification/Terraria.Dome.MainFieldPropertyInventory.Verification.csproj`

Release build 记录必须包含输出路径、exit code、warnings/errors；verifier 运行必须使用 `--no-build --no-restore`。不要并行启动，也不要直接调用 raw `dotnet`。

## 通过标准与不可关闭范围

只有在 state parse、state invariant、scoped artifact audit、manifest/docs、diff 和所有当前 08 evidence 均 fresh exit `0` 后，才可把本批次记录为 `completed_partial`。这仍不等价于完整 WorldGen parity，且不得把 `canRemoveLegacyWorldGen` 改为 true。

以下范围继续明确 deferred：

- Pyramid tunnel direction、`noTunnel` 完整 side effects、chest/pile/plant/pot 和完整结构 mutation；
- Pyramid 外完整 `SquareWallFrame` integration、client `SceneMetrics`、map/network publication；
- `WallFrameX/Y` 的 persistence/protocol serialization；
- aggregate generation ordering、exact global RNG/checkpoint parity、full WLD/extended-state differential；
- legacy `WorldGen.cs` deletion、`canRemoveLegacyWorldGen`、全部 44 个 `ServerRelevant` rows。

不要调用 `update_goal(status=complete)`，除非更大目标的所有要求已经由当前证据逐项满足；本继承任务的正常结束状态仍是 `N6 / in_progress_with_deferred_findings`。

