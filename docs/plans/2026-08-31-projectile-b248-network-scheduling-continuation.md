# Projectile B248 Network-Update Scheduling 继承执行计划

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan
> task-by-task.

**Goal:** 在不触碰 world-object 原子提交阻塞边界的前提下，完成并验收 B248 的
`netUpdate/netUpdate2/netSpam` 本 tick 复制调度交接，并把剩余证据交给下一会话继续。

**Architecture:** `ProjectileNetworkUpdatePolicy` 是唯一的 cadence 决策点；ECS 组件保留
本 tick 是否允许发送的决定，Simulation 将它投影为内部 `NetworkUpdateReady` 标志，Server
的 session cursor 只据此决定已发送活动实体是否跳过。首次可见复制、PVS 重入和 projectile
tombstone 不受该跳过门控影响；这个切片不新增 wire 字段，也不声称实现完整 V1456 cadence。

**Tech Stack:** C# `record struct`、Arch ECS、`ProjectileReplicationSnapshot`、
`CombatReplicationAssembler`、V1456/Server focused verifier、PowerShell
`Invoke-SerialDotnet.ps1`；所有构建使用 `-p:UseSharedCompilation=false`。

---

## 0. 新会话入口和当前状态

新会话必须从仓库根 `D:\TRbackup\NLTX` 开始，并按以下顺序读取：

1. [`AGENTS.md`](/D:/TRbackup/NLTX/AGENTS.md)；
2. [`progress.md`](/D:/TRbackup/NLTX/progress.md)；
3. [`docs/flowstate/README.md`](/D:/TRbackup/NLTX/docs/flowstate/README.md)；
4. `docs/flowstate/plan/2026-08-22-server-ecs-convergence.md` 和它指向的 task；
5. [`projectile-lifecycle-task.json`](/D:/TRbackup/NLTX/.agent-workplace/state/projectile-lifecycle-task.json)；
6. [`2026-08-30-projectile-world-object-command-blocked-execution.md`](/D:/TRbackup/NLTX/docs/plans/2026-08-30-projectile-world-object-command-blocked-execution.md)；
7. 本文件。

当前 Flowstate/Projectile 卡片：

| 项目 | 当前值 |
| --- | --- |
| Flowstate 节点 | `N182` |
| 当前批次 | `B248-projectile-network-update-scheduling` |
| 批次状态 | `completed_partial`；最终新目录串行复跑可能仍在等待共享互斥锁 |
| 前一批次 | `B247-projectile-type1091-damage-eligibility`，已 `completed_partial` |
| 总体 Projectile 状态 | `partial`，不能标记 complete |
| world-object 边界 | `blocked`，未获得 atomic command/commit 批准 |
| ID 处理 | 只从统计排除；`Projectile.identity`、UUID、tombstone identity 必须保留 |

工作树本来就包含大量其他 NPC/WorldGen/Projectile 改动。下一会话不得使用
`git reset --hard`、`git checkout --`、广泛清理或覆盖其他代理的修改；只检查和延续本批
写集。

## 1. B248 的 source contract

Oracle：
`D:\TRbackup\Version4物理删除了某些文件\Terraria\Projectile.cs`，SHA-256
`97C3D68586AEF862411699E428F5C4B18CEE1F8051B684BE9F86FA7B60FAE83B`。

锚点 `Projectile.cs:15245-15269` 的语义顺序是：

1. `netUpdate2` 先提升为 `netUpdate`；
2. 仅当 `netSpam < 60` 时发送，并把 `netSpam` 增加 `5`；
3. 达到饱和时保留 deferred pending（等价于下一次重试）；
4. 每 tick 对正 `netSpam` 递减 `1`；
5. tick 尾部清除 `netUpdate`。

本批只把“本 tick 是否允许 authoritative replication”接入现有 cursor。以下仍然明确
不在范围内：

- 完整 255-slot `netSyncSkippedForPlayer` 状态和 `RecheckSectionsForSkippedUpdates`；
- 所有真实 V1456 packet cadence、重连恢复和客户端 cadence；
- 完整 NPC-projectile extension policy；
- 完整 Projectile type/AI、immunity、specialized mechanics、VFX/audio 和 parity；
- `TileObject.CanPlace -> 多格 tile placement -> Sign.TextSign -> object placement
  replication -> projectile tombstone` 的 atomic world-object command/commit 契约。

## 2. 已经写入的 B248 代码写集

不要重做这些修改；先用 `git diff` 和源码确认它们仍在工作树中。

### Simulation

- `src/Terraria.Dome.Simulation/Components/ProjectileNetworkUpdateComponent.cs`
  - 新增 `SendRequested`；构造函数默认值为 `false`。
  - 保留已有 `SecondaryUpdatePending`、`NetSpam`、`PrimaryUpdatePending` 语义。
- `src/Terraria.Dome.Simulation/Projectile/Systems/ProjectileNetworkUpdatePolicy.cs`
  - `RequestPrimaryUpdate`/`RequestSecondaryUpdate` 保留当前 tick 的发送决定。
  - `Tick` 将 `ShouldSend` 写入 `SendRequested`，下一次无 pending tick 会清零。
- `src/Terraria.Dome.Simulation/Snapshots/ProjectileReplicationSnapshot.cs`
- `src/Terraria.Dome.Simulation/Snapshots/NpcProjectileReplicationSnapshot.cs`
  - 末尾新增 `NetworkUpdateReady`，默认 `true`，不改变既有调用者的初始复制行为。
- `src/Terraria.Dome.Simulation/Projectile/Systems/ProjectileReplicationSystem.cs`
- `src/Terraria.Dome.Simulation/Projectile/Systems/NpcProjectileReplicationSystem.cs`
  - 将 `SendRequested` 投影为 `NetworkUpdateReady`；`revision == 1` 的首次投影保持可发送。
  - NPC tombstone 投影显式保持可发送。
- `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
  - 普通/NPC 移动后的快照读取 `SendRequested`。
  - `MarkProjectileInactive` 对普通/NPC tombstone 设 `NetworkUpdateReady = true`。
  - 未改变 Projectile identity 分配、tombstone reason 或 world-object placement command。

### Server

- `src/Terraria.Dome.Server/Replication/CombatReplicationAssembler.cs`
  - 对已发送的活动普通/NPC projectile：`NetworkUpdateReady == false` 时跳过本次 revision。
  - 未发送的首次可见实体仍发送；tombstone 不受活动实体 cadence gate 阻断。
  - 没有把该内部标志编码进 V1456 wire packet。

### Verifiers

- `Test/Terraria.Dome.Combat.Verification/Program.cs`
  - network focused assertions 覆盖 `SendRequested` 成功、饱和 deferred、重试和下一 tick
    清零；tombstone reset 同时要求 `NetworkUpdateReady`。
- `Test/Terraria.Dome.Combat.Protocol.Verification/Program.cs`
  - 新参数 `--projectile-network-scheduling-only`。
  - 验证普通/NPC projectile 的首次发送、未获发送决定时的 revision 跳过和获准后的发送。

本批没有修改 `.csproj` 或 `TileChangeCommand`，也没有把现有 placement 类型当作已批准的
atomic world-object contract。

## 3. 已有 RED/GREEN 证据

### RED

生产代码前先加入了协议 focused verifier。其初始编译失败记录在：

`Build/diagnostics/projectile-b248-network-scheduling-20260831-01/red-verifier-build.log`

关键错误是 `CS0117: ProjectileReplicationSnapshot 未包含 NetworkUpdateReady 的定义`，证明
原有 snapshot/assembler 没有本 tick 调度合同。该 RED 不能被删除或改写成绿色历史。

`-01` 中第一次 NPC fixture 还曾因缺少有效 identity 而在编码器处失败；随后补入
`Identity`/`ProjectileUuid`，最终 fixture 通过。不要把该一次性 fixture 错误归因于生产代码。

### 已通过的 focused 运行

以下证据已在 `-01` 或后续日志中出现，新的会话应重新核对文件，而不是只相信本段文字：

- Combat `--network-update-only`：policy cadence、primary cadence、tombstone reset 通过；
- Combat.Protocol `--projectile-network-scheduling-only`：普通/NPC session gating 通过；
- 完整 Combat.Protocol smoke：在修正 NPC fixture 后通过；
- `Simulation`、`Server`、`Protocol.V1456`、Combat verifier 和 Combat.Protocol verifier 的
  既有 Release 编译曾以 0 warnings/0 errors 通过。

## 4. 下一会话必须完成的执行步骤

每一步完成后都把日志写入新鲜目录
`Build/diagnostics/projectile-b248-network-scheduling-20260831-02/`，不要覆盖 `-01`。

### Task 1：先收敛可能仍在运行的串行构建

**Files:** 只读诊断；不改源码。

1. 检查以下进程和日志，不要终止其他会话的进程：
   - `Build/diagnostics/server-ecs-convergence/P9-worldgen/pyramid-tunnel-opening-20260831-02/`；
   - `Build/diagnostics/projectile-b248-network-scheduling-20260831-02/Terraria.Dome.Simulation-build.log`。
2. 如果共享互斥锁仍被其他会话占用，等待它完成；如果命令已退出但只有
   `Waiting for serialized dotnet mutex`，仅重跑缺失的 B248 gate。
3. 用 `Get-CimInstance Win32_Process` 确认自己的旧命令是否已结束；不要用 broad kill。

### Task 2：重新执行五个串行 Release build

**Files:** 只生成 `Build/diagnostics/...` 日志。

从仓库根逐个执行（每个命令等待完成后再开始下一个）：

```powershell
$diag = 'Build/diagnostics/projectile-b248-network-scheduling-20260831-02'
& .\Build\Tools\Invoke-SerialDotnet.ps1 build `
  src/Terraria.Dome.Simulation/Terraria.Dome.Simulation.csproj `
  -c Release '-p:UseSharedCompilation=false' *> "$diag/Simulation-build.log"
& .\Build\Tools\Invoke-SerialDotnet.ps1 build `
  src/Terraria.Dome.Server/Terraria.Dome.Server.csproj `
  -c Release '-p:UseSharedCompilation=false' *> "$diag/Server-build.log"
& .\Build\Tools\Invoke-SerialDotnet.ps1 build `
  src/Terraria.Dome.Protocol.V1456/Terraria.Dome.Protocol.V1456.csproj `
  -c Release '-p:UseSharedCompilation=false' *> "$diag/ProtocolV1456-build.log"
& .\Build\Tools\Invoke-SerialDotnet.ps1 build `
  Test/Terraria.Dome.Combat.Verification/Terraria.Dome.Combat.Verification.csproj `
  -c Release '-p:UseSharedCompilation=false' *> "$diag/CombatVerifier-build.log"
& .\Build\Tools\Invoke-SerialDotnet.ps1 build `
  Test/Terraria.Dome.Combat.Protocol.Verification/Terraria.Dome.Combat.Protocol.Verification.csproj `
  -c Release '-p:UseSharedCompilation=false' *> "$diag/CombatProtocolVerifier-build.log"
```

为每个命令记录 `$LASTEXITCODE`。预期五个 exit code 都为 `0`，并且日志没有 warning/error。
如果失败，保留失败日志，先修复真实编译问题；不要直接跳到 `--no-build`。

### Task 3：运行 focused 和协议 smoke

```powershell
$diag = 'Build/diagnostics/projectile-b248-network-scheduling-20260831-02'
& .\Build\Tools\Invoke-SerialDotnet.ps1 run `
  --project Test/Terraria.Dome.Combat.Verification/Terraria.Dome.Combat.Verification.csproj `
  -c Release --no-build -- --network-update-only *> "$diag/Combat-network-focused.log"
& .\Build\Tools\Invoke-SerialDotnet.ps1 run `
  --project Test/Terraria.Dome.Combat.Protocol.Verification/Terraria.Dome.Combat.Protocol.Verification.csproj `
  -c Release --no-build -- --projectile-network-scheduling-only *> `
  "$diag/CombatProtocol-network-focused.log"
& .\Build\Tools\Invoke-SerialDotnet.ps1 run `
  --project Test/Terraria.Dome.Combat.Protocol.Verification/Terraria.Dome.Combat.Protocol.Verification.csproj `
  -c Release --no-build *> "$diag/CombatProtocol-smoke.log"
```

预期 focused 输出包含：

- `PASS: projectile network update policy is bounded and deterministic`；
- `PASS: primary projectile network update follows legacy deferred cadence`；
- `PASS: projectile tombstone resets network update state`；
- `PASS: projectile network-update decision gates authoritative replication`；
- `SUMMARY: ... intentionally not run`；
- 完整协议 smoke 以 `PASS: V1456 combat projection and PVS revision cursors` 结束。

### Task 4：记录已知 full Combat baseline

可运行一次完整 Combat verifier，但必须单独命名为 baseline 日志：

```powershell
$diag = 'Build/diagnostics/projectile-b248-network-scheduling-20260831-02'
& .\Build\Tools\Invoke-SerialDotnet.ps1 run `
  --project Test/Terraria.Dome.Combat.Verification/Terraria.Dome.Combat.Verification.csproj `
  -c Release --no-build *> "$diag/Combat-full-baseline.log"
"exit=$LASTEXITCODE" | Set-Content "$diag/Combat-full-baseline-exit.txt"
```

当前预期会在 `VerifyBoundedVitalRegeneration`（约 `Program.cs:654`）之前退出，且不会进入
Projectile 检查。这是既有 baseline failure，不得归因于 B248，也不能为了让它变绿而扩大
本批 write-set。

### Task 5：做窄范围静态和状态检查

```powershell
$paths = @(
  'src/Terraria.Dome.Simulation/Components/ProjectileNetworkUpdateComponent.cs',
  'src/Terraria.Dome.Simulation/Projectile/Systems/ProjectileNetworkUpdatePolicy.cs',
  'src/Terraria.Dome.Simulation/Snapshots/ProjectileReplicationSnapshot.cs',
  'src/Terraria.Dome.Simulation/Snapshots/NpcProjectileReplicationSnapshot.cs',
  'src/Terraria.Dome.Simulation/Projectile/Systems/ProjectileReplicationSystem.cs',
  'src/Terraria.Dome.Simulation/Projectile/Systems/NpcProjectileReplicationSystem.cs',
  'src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs',
  'src/Terraria.Dome.Server/Replication/CombatReplicationAssembler.cs',
  'Test/Terraria.Dome.Combat.Verification/Program.cs',
  'Test/Terraria.Dome.Combat.Protocol.Verification/Program.cs'
)
foreach ($path in $paths) {
  $line = 0
  Get-Content $path | ForEach-Object {
    $line++
    if ($_.Contains("`t") -or $_.Length -gt 100) {
      Write-Output ("{0}:{1}" -f $path, $line)
    }
  }
}
git diff --check
Get-Content -Raw .agent-workplace/state/projectile-lifecycle-task.json | ConvertFrom-Json | Out-Null
```

允许已有文件的历史超长行和 LF-to-CRLF notice，但 B248 新增块不得引入 tab 或新的超长行。

### Task 6：补齐证据清单并更新 checkpoint

只有 Task 2–5 的真实 exit code 都记录后，才更新：

- `.agent-workplace/state/projectile-lifecycle-task.json`：保留 `currentNode=N182`、
  `status=completed_partial`，追加 B248 的 RED/GREEN/DEFERRED 条目；
- `progress.md`：保留 B248 的 `completed_partial` 摘要，不提升总体 Projectile 状态；
- `Build/diagnostics/projectile-b248-network-scheduling-20260831-02/`：补充
  `build-exit-codes.json`、focused exit 文件、baseline exit 和 `artifact-paths.txt`。

不要把仍在等待互斥锁的空日志写成 exit 0；必须从实际 wrapper 返回值或完成日志读取状态。

## 5. 设计审查要点

下一会话在声称完成前必须确认：

1. `SendRequested` 只表示 policy 已在本 tick 消费 pending 并允许发送，不是“实体永远需要
   发送”；无 pending 的下一 tick 会清零。
2. `NetworkUpdateReady` 是 Simulation/Server 内部调度元数据，不是 V1456 wire contract；
   `Projectile.identity`/UUID 仍在 snapshot 和现有 packet/tombstone 关联中。
3. 首次 revision（`revision == 1` 或 cursor 尚未发送）必须发送；活动 revision 在
   `NetworkUpdateReady == false` 时可延迟；inactive tombstone 必须发送且不被活动 gate
   阻断。
4. NPC projectile 路径与普通 projectile 路径保持同一内部 gate，但不宣称 NPC extension
   的完整协议语义。
5. 不要在 `ProjectileBehaviorSystem` 中直接写 Server cursor、V1456 packet 或 world grid。
6. 不要通过修改 `TileChangeCommand`、`CreateSign()` 或已有 placement event 来绕过
   atomic world-object boundary。

## 6. Definition of Done

- [ ] 五个受影响项目的最新串行 Release build exit `0`，无 warning/error。
- [ ] 两个 focused verifier 和完整 Combat.Protocol smoke 的最新日志 exit `0`。
- [ ] full Combat baseline 若运行，失败点仍明确标为既有 fixture，不归因 B248。
- [ ] `SendRequested` 的 source → component → snapshot → session cursor 链有代码和 verifier
  证据。
- [ ] 首次发送、活动延迟、NPC 对称路径、PVS 重入、tombstone reset 都有窄断言。
- [ ] `git diff --check`、JSON parse 和 B248 目标块 style 检查通过。
- [ ] checkpoint/progress 保持 `completed_partial`；总体 Projectile migration 仍为 `partial`。
- [ ] world-object 执行计划仍为 `blocked`，没有填写虚构的批准人、日期或 write-set。
- [ ] 最终回复包含新鲜证据目录、实际 exit code、已知 baseline failure 和未完成 deferred
  范围。

## 7. 明确的转向条件

如果负责人明确批准 atomic world-object command/commit，另开批准后的批次，先冻结本文件
写集，不把 placement 代码混入 B248。如果负责人指定另一个已有 Projectile 字段/行为族，
将本批保持 `completed_partial` 并新开批次。没有这两种明确决定时，继续只做本文件的
network scheduling 收尾或其他已经有 Oracle 锚点的窄切片。

**继承结论：** B248 的生产代码和 focused 设计已经落盘，但新会话必须先收敛共享 dotnet
构建证据，再核销 Definition of Done；不要重复 B247，不要删除身份字段，不要解除
`TileObject.CanPlace -> ... -> projectile tombstone` 的 `blocked/deferred` 标记。
