# Main Server ECS 迁移执行控制手册

**目标：** 将 `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs` 中仍需保留的
服务器职责，迁移为 `Terraria.Dome.Simulation` 的显式 ECS 状态和系统；`DomeServer` 只负责
进程、会话、持久化协调和协议扇出。旧 `Main.cs` 是行为参考，绝不是可复制的目标类。

**主计划：**
[`2026-08-18-main-server-ecs-migration-implementation.md`](2026-08-18-main-server-ecs-migration-implementation.md)
定义任务范围和逐项行为。本文件定义 AI 如何持续执行、修复和验收这些任务，避免每遇到
一个失败就停下来要求人工决定。

## 1. 迁移边界

迁移的不是 `Main.cs` 的 13,996 行代码，而是它混合在一起的以下服务器职责：

| 旧职责 | 新所有者 | 不可跨越的边界 |
|---|---|---|
| 世界时钟、种子、规则、进度 | `Simulation/World` | 不读墙钟、`Main.time` 或全局随机数 |
| 实体创建、销毁、墓碑、复制 ID | `Simulation/Players`、`Npc`、`Projectile`、`Items` | Protocol 不看到 Arch `Entity` |
| 每 tick 的状态演进 | `Simulation/Tick` 和领域 System | 输入先验证，突变只在指定 phase 提交 |
| 世界加载和默认世界 | `Server/Startup`、`Server/Import` | Bootstrap 返回值对象，不创建 socket |
| 网络帧读取和复制 | `Server/Protocol`、`Server/Replication` | 网络线程不直接写 Simulation |
| 图形、UI、音频、平台和本地输入 | 不迁移 | 不能因“兼容”重新引入 Simulation |

以下名称和依赖是硬拒绝项：`Terraria.Main`、`Main.player`、`Main.npc`、`Main.tile`、
`Main.rand`、XNA、UI、socket、Protocol 类型和磁盘路径进入 Simulation。现有责任映射和
缺失来源记录在
[`main-server-responsibility-ledger.md`](../migrations/main-server-responsibility-ledger.md)。

## 2. 当前批次状态

| Task | 目标 | 当前状态 | 可使用的证据 |
|---:|---|---|---|
| 0 | 冻结 Version4 引用并建立责任账本 | 已接受 | `Build/diagnostics/main-migration/task-0/` |
| 1 | Simulation 的旧 Main/客户端依赖边界 | 已接受 | `task-1/` 的 MainBoundary 与 Simulation build |
| 2 | 显式世界时钟 | 已接受 | `task-2/` 的 clock、rules、boundary 和 root build |
| 3 | 世界元数据、种子、规则和进度快照 | 已接受 | `task-3/` 的 persistence、import、rules 和 root build |
| 4 | 命名且确定性的 tick phase | 已接受其 focused gate | `task-4/`；广义 loopback 失败仍待处理 |
| 5 | 玩家、NPC、投射物、世界物品生命周期所有权 | 已接受 | `task-5/` 的 focused、loopback 和最终串行 Release evidence |
| 6 | Server world bootstrap | 已接受 | `task-6/` 的 bootstrap、import、persistence、client 和 root build |
| 7 | 世界天气、事件和进度状态机 | 进行中：BloodMoon、Eclipse、Invasion、Rain、Slime Rain、Slime Rain warning、Lantern Night same-tick ordering、Meteor impact、Wind target、Wind rain coupling、Lantern Night、Meteor schedule 已接受 | `task-7-blood-moon/`、`task-7-eclipse/`、`task-7-invasion/`、`task-7-rain/`、`task-7-slime-rain/`、`task-7-slime-rain-warning/20260819-043037/`、`task-7-lantern-same-tick-ordering/20260819-044056/`、`task-7-meteor/`、`task-7-wind/`、`task-7-lantern-night/`、`task-7-meteor-schedule/`、`task-7-wind-rain-coupling/20260819-041750/`；其余事件族仍逐项迁移 |
| 8 | 已实际使用的静态定义 | 进行中：Tile Solidity authority audit | `task-8-tile-solidity/20260819-131722/`；禁止整表复制或把 liquid 语义误作 collision 语义 |
| 9 | 纯计算和遗留 Main 读取消除 | 进行中：伤害减免公式已接受 | `task-9-damage-calculation/20260819-122505/` |
| 10 | 将 `DomeSimulation` 收缩为协调门面 | 未开始 | 不能与新玩法混合 |
| 11 | 完整边界与能力验收 | 未开始 | 只接受当前源树的新鲜证据 |

Task 4 的固定执行顺序是：

```text
BeginTick
ApplyWorldClock
ApplyPlayerInputs
ApplyPlayerControl
ResolveTileCollision
SelectNpcTargets
ApplyNpcAi
MoveEntities
AdvanceProjectiles
ResolveCombat
CommitDomainCommands
PublishSnapshot
EndTick
```

`ResolveTileCollision` 必须在 `SelectNpcTargets` 之前，因为目标和接触逻辑消费已解析的
玩家位置。不得把这个顺序还原为旧方案中的 target-before-collision。

## 3. AI 的持续执行循环

每个 AI 工作回合只拥有一个最小的领域批次，例如“PlayerStore 的销毁和快照契约”或
“装备命令从入队到 tick 提交”。它按以下闭环执行，不等待人工逐个解释错误：

1. **读取证据。** 阅读主计划中当前 Task、责任账本、当前实现和该领域 verifier；先确认
   当前源树，不把历史日志当作当前结论。
2. **写出验收命题。** 用一句可证伪的话定义行为，例如“重复销毁不会复用复制 ID，且已
   销毁实体不出现在公开 snapshot 中”。命题必须包括允许与拒绝两种路径。
3. **建立 RED。** 新增或扩展可执行 verifier，先运行并将失败输出保存。若现有 verifier
   已精确覆盖命题，可直接以它的失败为 RED，不重复造测试。
4. **做最小完整变更。** 从 `command -> validation -> named system -> deterministic commit
   -> immutable snapshot` 走通。不得用 Protocol 旁路、可变全局字典或只为测试而设的开关。
5. **分层验证。** 先运行 focused verifier，再运行同领域回归、边界 verifier 和串行
   Release build。任一源文件在验证后改变，之前的 GREEN 自动失效。
6. **归因与修复。** 失败先分为实现失败、测试陈旧、构建并发/引用产物、已有独立回归四类。
   AI 只修复与本批命题同一责任边界的前两类；第三类按串行方式重试；第四类记录为独立
   backlog，不篡改其断言来取得假绿。
7. **更新事实。** 将命题、命令、退出码、警告、失败原因、证据路径和下个最小批次写入
   `progress.md`。只有 focused gate、受影响回归和 root build 同时为当前源树绿色时，才把
   Task 或子批次标记为接受。

AI 可自行继续的情况包括：补齐缺少的领域命令/快照、修复编译错误、扩展已存在 verifier、
修复确定性排序、重跑受污染的构建和记录独立回归。以下才需要人工架构裁决：

- 原始 Version4 文件确实缺失，且没有存档、二进制或可运行服务器可作为行为 oracle；
- 同一行为同时要求两种互斥的权威语义；
- 为兼容必须向 Simulation 引入旧 Main、客户端、UI 或协议依赖；
- 行为会改变保存格式、真实客户端协议或公开 API，且不存在向后兼容策略；
- 连续三次在相同根因上无新证据，继续尝试只会重复。

## 4. 验收不是“测试全绿”

一个迁移切片须同时满足四类证据，任何一类缺失都只能标为 `partial`：

| 证据层 | 要回答的问题 | 本仓库的具体形式 |
|---|---|---|
| 来源证据 | 旧行为究竟是什么？ | 责任账本中的 Main/依赖方法、哈希、行区间和缺失声明 |
| 权威证据 | 谁可以改变状态？ | 命令验证、Store、领域 System、拒绝路径 verifier |
| 行为证据 | 可观察结果是否正确？ | accepted/rejected verifier、固定 fixture、协议/快照断言 |
| 演进证据 | 重放、保存和版本升级是否稳定？ | 相同 seed+snapshot+输入 trace、持久化 round-trip、loopback |

因此“编译成功”只证明类型系统一致；“单个 verifier 通过”只证明一个命题；历史绿色日志
只证明历史源树。完整接受至少需要来源、权威、行为和演进四类证据，加上当前源树的 Release
build 与边界扫描。

## 5. Task 7 / Invasion 已接受批次的可复现验收脚本

从仓库根目录执行。每次用新的 `runId`，不使用 `--no-build` 作为刚改过源文件的证据。
该脚本记录 Task 7 的 `Invasion` 子批次的接受过程，而不是已经接受的 Task 5。

```powershell
$runId = Get-Date -Format "yyyyMMdd-HHmmss"
$evidence = "Build\diagnostics\main-migration\task-7-invasion\$runId"
New-Item -ItemType Directory -Force -Path $evidence | Out-Null

git status --short | Tee-Object "$evidence\worktree.txt"
if ($LASTEXITCODE -ne 0) { throw "git status failed: $LASTEXITCODE" }
git diff --check | Tee-Object "$evidence\diff-check.txt"
if ($LASTEXITCODE -ne 0) { throw "git diff --check failed: $LASTEXITCODE" }
dotnet sln Terraria.Dome.sln list | Tee-Object "$evidence\solution-projects.txt"
if ($LASTEXITCODE -ne 0) { throw "dotnet sln list failed: $LASTEXITCODE" }

function Invoke-RecordedDotnet {
  param(
    [Parameter(Mandatory)]
    [string] $name,
    [Parameter(Mandatory)]
    [string[]] $arguments)

  & dotnet @arguments 2>&1 | Tee-Object "$evidence\$name.txt"
  $exitCode = $LASTEXITCODE
  if ($exitCode -ne 0) {
    throw "dotnet $name failed: $exitCode"
  }
}
```

上面的函数会将每个 gate 的完整 stdout/stderr 写进独立文件，并停止于第一个非零退出码。
若源文件在任一步骤后变动，前面的成功输出立即失效，必须从 focused gate 重跑。

Task 7 / Invasion 的 gate 按下面顺序运行并分别保存完整输出：

```powershell
Invoke-RecordedDotnet "world-rules-final" @(
  "run", "--project", "Test\Terraria.Dome.WorldRules.Verification\Terraria.Dome.WorldRules.Verification.csproj",
  "-c", "Release", "-p:UseSharedCompilation=false", "-p:MSBuildNodeReuse=false")

Invoke-RecordedDotnet "persistence-final" @(
  "run", "--project", "Test\Terraria.Dome.Persistence.Verification\Terraria.Dome.Persistence.Verification.csproj",
  "-c", "Release", "-p:UseSharedCompilation=false", "-p:MSBuildNodeReuse=false")

Invoke-RecordedDotnet "world-rules-loopback-final" @(
  "run", "--project", "Test\Terraria.Dome.WorldRules.Loopback.Verification\Terraria.Dome.WorldRules.Loopback.Verification.csproj",
  "-c", "Release", "-p:UseSharedCompilation=false", "-p:MSBuildNodeReuse=false")

Invoke-RecordedDotnet "protocol-compatibility-final" @(
  "run", "--project", "Test\Terraria.Dome.Protocol.Compatibility.Verification\Terraria.Dome.Protocol.Compatibility.Verification.csproj",
  "-c", "Release", "-p:UseSharedCompilation=false", "-p:MSBuildNodeReuse=false")

Invoke-RecordedDotnet "full-client-bootstrap-final" @(
  "run", "--project", "Test\Terraria.Dome.FullClientBootstrap.Verification\Terraria.Dome.FullClientBootstrap.Verification.csproj",
  "-c", "Release", "-p:UseSharedCompilation=false", "-p:MSBuildNodeReuse=false")

Invoke-RecordedDotnet "main-boundary-final" @(
  "run", "--project", "Test\Terraria.Dome.MainBoundary.Verification\Terraria.Dome.MainBoundary.Verification.csproj",
  "-c", "Release", "-p:UseSharedCompilation=false", "-p:MSBuildNodeReuse=false")

Invoke-RecordedDotnet "root-release-build-final" @(
  "build", "Terraria.Dome.sln", "-c", "Release", "-p:UseSharedCompilation=false",
  "-p:MSBuildNodeReuse=false", "-m:1")
```

若根构建失败，先看 `root-release-build-final.txt` 中首个实际错误：禁止以末尾汇总、旧 DLL
或随后出现的连锁错误作为根因。已经存在的老失败必须用新 run 复现后才能记为 blocker。

## 6. Task 7 / Invasion 的已接受执行记录

本批唯一目标是服务器拥有的、确定性 Invasion 状态机。它从旧代码中提取的参考范围是
`Main.cs:12215-12254`（同步）、`12962-13040`（完成和显示状态）、`13047-13128`
（开始）以及 `NetMessage.cs:case 7`（WorldData 的 invasion type）。`NetMessage.cs:case 78`
只记录为未来的 progress frame 投影参考，不在本批实现。

**本批可证伪命题：** 经验证的服务器请求以 `type=2, size=40` 开始 invasion，并在
`ApplyWorldClock` 的世界进度提交点生效；无效类型、重复开始和完成后的进度扣减均拒绝且不改变
snapshot。合法进度扣减将 `40 -> 25`，扣减到零或以下时规范化为 `(InvasionType=0,
InvasionSize=0)`；保存恢复和 V1456 WorldData 保留 `InvasionType`。

1. **锁定 RED。** `Terraria.Dome.WorldRules.Verification` 必须先在缺少
   `WorldInvasionStartCommand`、`WorldInvasionProgressCommand` 和两个 queue API 时失败；只要
   当前源已包含这些符号，记录该历史 RED 和本次 focused run，不伪造第二次 RED。
2. **完成唯一权威路径。** 使用 `WorldInvasionStartCommand`、
   `WorldInvasionProgressCommand`、`DomeSimulation` 的入队 API 和
   `WorldProgressionSystem`。协议收包、测试或 Server 不得直接改 `WorldProgressionState`。
3. **验证状态机。** 覆盖 `type=2,size=40` 的接受、`type=0` 的拒绝、重复开始的拒绝、
   tick 后的 `(2,40)`、扣减后的 `(2,25)`、清零后的 `(0,0)`，以及清零后再扣减的拒绝。
   每个拒绝分支都断言 progression snapshot 和 revision 不变。
4. **验证演进和投影。** 从 `(2,25)` snapshot 重建 Simulation 后继续保持该状态；
   `DomeServer.CreateWorldDataContext()` 必须投影 type `2`。持久化格式只沿用已有进度字段；
   若需要变更格式版本，必须同时增加旧版本读取 fixture 和明确的兼容策略。
5. **运行第 5 节全部 gate。** focused verifier 后依次跑 Persistence、WorldRules Loopback、
   Protocol Compatibility、FullClientBootstrap、MainBoundary，最后才运行串行 root Release build。
6. **形成证据。** 目录固定为
   `Build/diagnostics/main-migration/task-7-invasion/<runId>/`，至少包含
   `world-rules-red.txt`（或历史 RED 的准确引用）、`world-rules-final.txt`、
   `persistence-final.txt`、`world-rules-loopback-final.txt`、
   `protocol-compatibility-final.txt`、`full-client-bootstrap-final.txt`、
   `main-boundary-final.txt` 和 `root-release-build-final.txt`。
7. **接受边界。** 第 5 节 gate 已在
   `Build/diagnostics/main-migration/task-7-invasion/20260819-013459/` 全部成功。Invasion 已接受，
   但 Task 7 仍为 `进行中`；不得由此声称雨、Slime Rain、Meteor、NPC 波次、消息 78 或 UI
   已迁移。

## 7. Task 7 / Rain 的已接受执行记录

本批唯一目标是服务器拥有的、确定性 Rain 状态机。它从旧代码中提取的参考范围是
`Main.cs:13236-13339`（`StopRain`、`StartRain`、`ChangeRain`）、`Main.cs:13388-13523`
（按 day rate 消耗雨时长）以及 `NetMessage.cs:277-281`（inactive rain 写 `0.0f`，active
rain 写 `maxRaining`）。随机持续时间和强度、自动随机下雨、coin rain、Lantern Night、wind、cloud
rendering、Slime Rain 与 Meteor 均不在本批范围内。

**本批可证伪命题：** 经验证的服务器请求以 `(duration=3, strength=0.5)` 开始 rain，在
`ApplyWorldClock` 中提交；无效 duration、无效 strength 和重复请求均被拒绝且不改变规则状态。已有
rain 每 tick 按 `WorldClockSnapshot.TicksPerUpdate` 消耗 duration，耗尽后规范化为
`(IsRaining=false, RainTimeTicks=0, RainStrength=0.0f)`。保存恢复保留 active rain；v9 的
WorldData 投影只在 active 状态写 `MaximumRaining`。

1. **锁定 RED。** Persistence verifier 先将规则 fixture 扩展为 `(rainTimeTicks=123,
   rainStrength=0.5f)`，并因 v8 format 忽略 rain 字段失败。WorldRules verifier 先要求由 active
   rain snapshot 创建的 `DomeServer` 投影 `Background.MaximumRaining == 0.5f`，并因旧 adapter
   固定使用默认背景失败。
2. **完成唯一权威路径。** `WorldRainStartCommand` 经过 `DomeSimulation.TryQueueWorldRain` 的
   验证后，在 `WorldWeatherSystem` 中于 `ApplyWorldClock` 提交。协议、Server 和测试均不直接修改
   `WorldRuleState`。
3. **完成演进契约。** `DomeStatePersistenceFormat` 升级为 v9，并只在既有四个 WorldRule 字段之后
   附加 `RainTimeTicks` 和 `RainStrength`。v1-v3 继续使用默认规则；v4-v8 按原四字段布局读取并恢复
   canonical clear-rain；v9 读取 rain 字段。Persistence verifier 既检查 v9 round-trip，也手写 v8
   payload 验证旧布局没有被重新解释。
4. **完成协议投影。** `DomeServer.CreateWorldDataContext()` 向 `LegacyWorldDataContext.WithWorldState`
   传递不可变 `WorldRuleState`。adapter 只在 `rules.IsRaining` 时设置既有 V1456
   `Background.MaximumRaining`，不增加 packet 字段或改变既有 field order。真实 TCP loopback 解析
   WorldData 的 wire float 并断言为 `0.5f`。
5. **运行全部 gate。** 以下当前源树证据均在
   `Build/diagnostics/main-migration/task-7-rain/20260819-020437/` 中：
   `world-rules-final.txt`、`persistence-final.txt`、`world-rules-loopback-final.txt`、
   `protocol-compatibility-final.txt`、`full-client-bootstrap-final.txt`、
   `main-boundary-final.txt` 和 `root-release-build-final.txt`。全部退出码为 `0`；根 Release build
   为 `0 warnings, 0 errors`。
6. **接受边界。** Rain 已接受，但 Task 7 仍为 `进行中`。不得由此声称自动 weather、random stream
   parity、Slime Rain、Meteor、风、云、公告、monster spawn、UI 或客户端权威启动已迁移。

## 8. Task 7 / Slime Rain 的已接受执行记录

本批唯一目标是服务器拥有的、确定性 Slime Rain 状态机。它从旧代码中提取的参考范围是
`Main.cs:13341-13386`（开始和停止）、`Main.cs:13415-13430`（按 day rate 消耗和停止）以及
`NetMessage.cs:303`（WorldData `bitsByte8[2]`）。随机启动、随机时长、remix/world-surface 条件、
negative cooldown、warning、公告、NPC slot、slime spawn table 和 King Slime 逻辑均不在本批范围内。

**本批可证伪命题：** 经验证的服务器请求以 `duration=3` 开始 Slime Rain，在
`ApplyWorldClock` 中提交；duration 非正或重复请求均被拒绝。普通 rain 已 active 或待提交时，
Slime Rain 被拒绝；反向的普通 rain 请求同样被拒绝。active Slime Rain 每 tick 按
`WorldClockSnapshot.TicksPerUpdate` 消耗，耗尽后规范化为
`(IsSlimeRaining=false, SlimeRainTimeTicks=0)`。保存恢复和 V1456 WorldData `EventFlags3` 的
bit 2 保留 active 状态。

1. **锁定 RED。** WorldRules verifier 先要求 `WorldSlimeRainStartCommand`、入队 API、
   `IsSlimeRaining`、持续时间和 WorldData projection；缺失类型与成员使 verifier 编译失败。
2. **完成唯一权威路径。** `DomeSimulation.TryQueueWorldSlimeRain` 验证请求及 rain 互斥性；
   `WorldProgressionSystem` 在 `ApplyWorldClock` 中启动或消耗状态。协议、Server、公告和测试
   不直接修改 `WorldProgressionState`。
3. **完成演进契约。** `DomeStatePersistenceFormat` 升级为 v10，在既有 v9 progression 末尾追加
   `SlimeRainTimeTicks`。v1-v9 按原布局读取且默认 clear；v10 读取新时长。格式升级同时将 v8
   item-instance field gate 固定到 `ItemWorldStateFormatVersion=8`，避免 total format version 从 v9
   升级时错误读取旧物品字段。
4. **完成协议投影。** `LegacyWorldDataContext` 保留既有 `EventFlags3` 的其他 bits，仅在 active
   Slime Rain 时设置 bit 2。WorldData packet 的字段数和顺序不变；真实 TCP loopback 同时验证
   BloodMoon flag、`MaximumRaining=0.5f` 和 Slime Rain bit。
5. **运行全部 gate。** 当前源树证据在
   `Build/diagnostics/main-migration/task-7-slime-rain/20260819-021430/`：
   `world-rules-final.txt`、`persistence-final.txt`、`world-rules-loopback-final.txt`、
   `protocol-compatibility-final.txt`、`full-client-bootstrap-final.txt`、
   `main-boundary-final.txt` 和 `root-release-build-final.txt` 均为 exit `0`；根 Release build
   为 `0 warnings, 0 errors`。
6. **接受边界。** Slime Rain 已接受，但 Task 7 仍为 `进行中`。不得由此声称随机概率、旧全局随机流、
cooldown、warning、聊天、NPC waves、King Slime、remix 条件或客户端权威启动已迁移。

## 9. Task 7 / Meteor impact 的已接受执行记录

本批将 Meteor 拆成两个不同命题：本批接受的是“一个已授权的确定性落点实际改变世界并复制”，
不把旧的随机调度、流星雨展示或公告伪装成已迁移。参考来源为 Version4
`Main.cs:13750-13764`（`spawnMeteor` 的夜间调度）、`Main.cs:13982-14016`
（白天窗口调用 `dropMeteor`）以及 `WorldGen.cs:5910-6030`（落点搜索）、
`WorldGen.cs:6030-6245`（安全区、保护方块、陨石坑和 Tile ID 37 写入）；
`NetMessage.cs:6217` 是旧实现的 TileSquare 复制参考。

**本批可证伪命题：** `WorldMeteorImpactCommand(x, y, sequence)` 只有在边界、玩家/NPC
安全区、保护方块和陨石数量上限满足时才入队；`WorldMeteorImpactSystem` 在
`CommitDomainCommands` 阶段生成有序 `TileChangeCommand`，中心空腔和外围 Meteorite
（Tile ID 37）均是实际 `WorldGrid` 变化。相同世界快照和相同命令产生完全相同的瓦片集合，
目标 section version 递增；拒绝路径不改变世界。

1. **锁定 RED。** WorldRules verifier 先引用缺失的 `WorldMeteorImpactCommand` 和
   `TryQueueWorldMeteorImpact`，编译失败证明当前树没有只为测试准备的假入口。
2. **完成唯一权威路径。** `DomeSimulation.TryQueueWorldMeteorImpact` 只接受合法命令，
   `WorldMeteorImpactSystem.TryCreateCommands` 负责边界、实体安全区、保护 Tile ID
   `26/226/470/475/488/597` 和陨石上限，提交只发生在 `CommitDomainCommands`；Protocol
   不直接写 `WorldGrid`。
3. **完成确定性世界变化。** 影响系统以固定半径生成 Kill/Place 命令，并由现有
   `TileChangeCommitSystem` 按 sequence/X/Y/kind 排序提交。持久化使用现有 `WorldGridSnapshot`
   和 section versions，不增加未定义的随机全局状态。
4. **运行全部 gate。** 当前源树新鲜证据在
   `Build/diagnostics/main-migration/task-7-meteor/20260819-025654/`：
   `world-rules-final.txt`、`persistence-final.txt`、`world-rules-loopback-final.txt`、
   `protocol-compatibility-final.txt`、`full-client-bootstrap-final.txt`、
   `main-boundary-final.txt` 和 `root-release-build-final.txt` 均 exit `0`；
   MainBoundary 检查 353 个 Simulation 源文件、0 个违规；根 Release build 为
   `0 warnings, 0 errors`。真实 TCP loopback 请求 section 后解析到 Meteorite Tile ID 37。
5. **接受边界。** 本批不声称旧 `Main.rand` 的随机触发顺序、`dropMeteor` 的随机候选搜索、
`StartMeteorShower` 的 projectile/ambience、聊天公告、完整液体/斜坡/墙体 framing 特殊规则、
或客户端权威落点已迁移。下一批必须从这些剩余行为中选择一个独立命题，并重新走 RED、
Simulation authority、行为、演化和全部 gate。

## 10. Task 7 / deterministic wind target 的已接受执行记录

本批从 Version4 `Main.cs:12446-12579` 的 `ResetWindCounter`/`UpdateWeather` 提取出一个
可独立验收的子命题，并保留其余视觉和随机行为为未迁移项。V1456 `NetMessage.cs:258`
已有 `Main.windSpeedTarget` 字段，因此不增加网络字段或改变 WorldData layout。

**本批可证伪命题：** `WorldWindChangeCommand(targetSpeed, sequence)` 只接受有限的
`[-0.8, 0.8]` 目标；重复、NaN 和越界请求拒绝且不改变状态。`WorldWeatherSystem` 在
`ApplyWorldClock` 先提交目标，再按 `TicksPerUpdate` 以确定的步长平滑
`WindSpeedCurrent`。状态快照和 v11 持久化保留两个值；v1-v10 读取旧规则布局时将新增
风字段恢复为零；WorldData 投影只写现有 `Background.WindSpeedTarget`。

1. **锁定 RED。** WorldRules verifier 先引用缺失的命令、队列 API 和风状态成员，编译失败。
2. **完成唯一权威路径。** `DomeSimulation.TryQueueWorldWind` 是唯一入口，
   `WorldWeatherSystem` 是唯一状态转换者；Protocol 只从 `DomeServer.CreateWorldDataContext`
   投影不可变状态。
3. **完成演化契约。** `DomeStatePersistenceFormat` 从 v10 升到 v11，在既有 rain 字段之后
   追加 target/current 两个 `Single`。Persistence verifier 同时写入 v11 非零风值，并把
   当前 v11 payload 去掉两个风字段、改写为 v10，证明旧 payload 仍能恢复雨/Slime Rain 且风为零。
4. **运行全部 gate。** 当前源树证据在
   `Build/diagnostics/main-migration/task-7-wind/20260819-031500/`：
   `world-rules-final.txt`、`persistence-final.txt`、`world-rules-loopback-final.txt`、
   `protocol-compatibility-final.txt`、`full-client-bootstrap-final.txt`、
   `main-boundary-final.txt` 和 `root-release-build-final.txt` 均 exit `0`；
   MainBoundary 检查 357 个 Simulation 源文件、0 个违规；根 Release build 为
   `0 warnings, 0 errors`。
5. **接受边界。** 本批不声称 `Main.rand` 的随机风向换向、wind/极端计数器、云数量、云背景、
   storm/windy-day 音乐、Lantern Night 冻结规则、玩家资格限制或客户端视觉效果已迁移。
   下一批必须单独定义其中一个服务器行为并重新建立 RED 与完整证据。

## 11. Task 7 / Lantern Night 的已接受执行记录

本批只接受 Lantern Night 的最小服务器状态机。Version4 `Main.cs:13392-13399`、
`Main.cs:13756-13767`、`Main.cs:13897-13898` 和 `Main.cs:13530` 提供了昼夜检查、
清理和事件状态读取链；删除的 `LanternNight` 类源码没有恢复，因此不虚构随机资格、奖励、
幸运值或 NPC 行为。V1456 `NetMessage.cs:332` 的 `bitsByte11[1]` 是现有 wire authority，
因此本批通过 `LegacyWorldDataContext` 的 `EventFlags11` bit 1 投影，不改变 WorldData 长度
或字段顺序。

**本批可证伪命题：** 夜间显式 `WorldEventKind.LanternNight` 请求可以启动一次 Lantern Night；
重复请求拒绝；白天 tick 清理状态；`WorldProgressionState.IsLanternNight` 是唯一状态值，
并在 WorldData 的 `EventFlags11 bit 1` 中投影。命令只能从 `DomeSimulation` 入队，状态转换只能
由 `WorldProgressionSystem` 在确定的 `ApplyWorldClock` phase 执行。

1. **锁定 RED。** WorldRules verifier 先引用缺失的事件枚举、状态字段和命令路径，编译失败，
   证明测试不是通过旁路假入口变绿。
2. **完成唯一权威路径。** `DomeSimulation.CanQueueWorldEvent` 只在夜间接受 Lantern Night；
   `WorldProgressionSystem` 负责启动和白天清理；Protocol 只读取不可变 progression 快照。
3. **完成演化契约。** `DomeStatePersistenceFormat` 从 v11 升到 v12，在 `IsEclipse` 后追加
   一个 Lantern Night `Boolean`。v1-v11 读取时默认 `false`。Persistence verifier 证明：
   v12 round-trip 保留 Lantern Night；移除该字节的 v11 payload 保留 Wind 但默认 Lantern Night；
   同时移除 Wind 两个 `Single` 和 Lantern Night 字节的 v10 payload 默认两者为零/false，且保留
   Rain 与 Slime Rain。
4. **运行全部 gate。** 当前源树证据在
   `Build/diagnostics/main-migration/task-7-lantern-night/20260819-032916/`：
   `world-rules-final.txt`、`persistence-final.txt`、`world-rules-loopback-final.txt`、
   `protocol-compatibility-final.txt`、`full-client-bootstrap-final.txt`、
   `main-boundary-final.txt` 和 `root-release-build-final.txt` 均为 exit `0`；
   MainBoundary 检查 358 个 Simulation 源文件、0 个违规；根 Release build 为
   `0 warnings, 0 errors`。
5. **接受边界。** 本批不声称删除的 LanternNight 类中的随机资格算法、奖励/NPC 生成、幸运值、
   云雨风联动、公告、音乐、客户端视觉或客户端权威启动已迁移。下一批必须从这些未知行为中
   选择一个能取得独立 oracle 的命题，并重新建立 source、authority、RED、行为、演化和全部 gate。

## 12. Task 7 / Meteor schedule window 的已接受执行记录

本批只接受旧 `WorldGen.spawnMeteor` 标志的确定性、服务器拥有的窗口状态，不把随机触发和
随机落点伪装成已经迁移。Version4 来源为 `Main.cs:13750-13764` 的夜间初始化、
`Main.cs:13982-14016` 的 `HandleMeteorFall`，以及 `WorldGen.cs:4163` 的标志定义。旧代码在
`time > 16200` 时清除待处理标志，之后才进入 `dropMeteor`/`StartMeteorShower` 分支；
`time < 15000` 和 `15000..16200` 的 ambience 只属于客户端展示，不进入 Simulation。

**本批可证伪命题：** 具备 `DefeatedEaterOrBrain` 的世界只能在夜间接受显式
`WorldMeteorScheduleCommand(sequence)`；非法 sequence、白天请求、未满足 Boss 资格、重复请求
和同 tick 重复请求均拒绝。状态在 `ApplyWorldClock` 进入 `IsMeteorScheduled=true`；日间
`TimeOfDay == 16200` 仍保留，下一 tick 到 `16201` 时由命名 `WorldMeteorScheduleSystem` 清除。
快照恢复保留 pending 状态，Protocol 不新增字段。

1. **锁定 RED。** WorldRules verifier 先引用缺失的 schedule command、入队 API 和 progression
   状态，编译失败；实现后 focused 输出同时覆盖资格、重复、边界和快照 continuation。
2. **完成唯一权威路径。** `DomeSimulation.TryQueueWorldMeteorSchedule` 是唯一入队入口，
   `WorldMeteorScheduleSystem` 在 `ApplyWorldClock` 做截止消费；未引入 `Main.rand`、Protocol
   旁路或磁盘路径。
3. **完成演化契约。** `DomeStatePersistenceFormat` 从 v12 升到 v13，在既有 progression
   字段末尾追加一个 `IsMeteorScheduled` Boolean。v1-v12 读取默认 `false`；Persistence verifier
   证明 v13 round-trip、v12 默认新字段、v11/v10 旧布局仍分别保留既有 Wind/Rain/Slime Rain
   语义并清除新增字段。
4. **运行全部 gate。** 当前源树证据在
   `Build/diagnostics/main-migration/task-7-meteor-schedule/20260819-034057/`：
   `world-rules-final.txt`、`persistence-final.txt`、`world-rules-loopback-final.txt`、
   `protocol-compatibility-final.txt`、`full-client-bootstrap-final.txt`、
   `main-boundary-final.txt` 和 `root-release-build-final.txt` 均为 exit `0`；
   MainBoundary 检查 360 个 Simulation 源文件、0 个违规；根 Release build 为
   `0 warnings, 0 errors`。
5. **接受边界。** 本批不声称旧 `Main.rand.Next(50)` 的自动概率、`WorldGen.dropMeteor` 的
   候选搜索和地形特殊规则、`StartMeteorShower`、ambience、公告、TileSquare 额外投影或客户端
   权威启动已迁移。下一批必须为其中一个行为取得独立 oracle，再建立新的 RED 和完整 gate。

## 13. Task 7 / scheduled Meteor resolution 的已接受执行记录

本批把上一批 pending schedule 接到已经接受的 `WorldMeteorImpactSystem`，但仍不声称已经
实现旧 `WorldGen.dropMeteor` 的随机候选搜索。Version4 `Main.cs:13982-14016` 在
`time > 16200` 后消费 `WorldGen.spawnMeteor`；本批以显式、已授权的
`WorldMeteorImpactCommand(x, y, sequence)` 作为 resolution target，确保 Simulation 只处理
确定性 intent，而不调用旧全局随机数。

**本批可证伪命题：** `TryQueueScheduledWorldMeteorImpact` 只有在 daytime、
`TimeOfDay > 16200`、pending schedule 存在且 impact 命令通过既有边界/实体/保护 Tile 验证时
才接受；cutoff 前、未 schedule、重复 resolution 和直接 impact 旁路均拒绝。下一 tick 在
`ApplyWorldClock` 清除 pending 状态，复用既有 `TileChangeCommand -> WorldGrid.CommitTileChanges`
路径提交 Meteorite Tile ID 37。

1. **锁定 RED。** WorldRules verifier 先引用缺失的 scheduled-resolution API，随后出现的
   编译错误只修复了当前工作树并发的 Item snapshot 命名参数；resolution API 缺失的 RED
   仍由 focused 场景覆盖。
2. **完成唯一权威路径。** `DomeSimulation.TryQueueScheduledWorldMeteorImpact` 做 cutoff、
   pending 和 impact safety 验证；`ResolveScheduledWorldMeteorImpacts` 只把经过验证的命令
   放入现有 impact commit 队列；Protocol/Server 不写 Tile。
3. **运行全部 gate。** 当前源树证据在
   `Build/diagnostics/main-migration/task-7-meteor-resolution/20260819-035804/`：
   `world-rules-final.txt`、`persistence-final.txt`、`world-rules-loopback-final.txt`、
   `protocol-compatibility-final.txt`、`full-client-bootstrap-final.txt`、
   `main-boundary-final.txt` 和 `root-release-build-final.txt` 均为 exit `0`；
   MainBoundary 检查 360 个 Simulation 源文件、0 个违规；根 Release build 为
   `0 warnings, 0 errors`。
4. **接受边界。** 本批不声称 `WorldGen.dropMeteor` 的随机位置候选、地形 framing/liquid 特殊
   规则、`StartMeteorShower`、ambience、公告、额外 TileSquare replication 或客户端权威
   resolution 已迁移。后续只有在取得独立 oracle 后，才能继续收窄这些 Unknown 行为。

## 14. Task 7 / Lantern Night rain suppression 的已接受执行记录

本批迁移 Version4 `Main.cs:13433-13437` 的明确交互：当 `LanternNight.LanternsUp` 为真时，
天气更新调用 `StopRain()`。它只影响普通 Rain；Slime Rain、云背景、音乐和删除的 LanternNight
资格算法不在本批范围。

**本批可证伪命题：** active `WorldProgressionState.IsLanternNight` 时，新的普通
`WorldRainStartCommand` 在 `DomeSimulation` authority 入口被拒绝；已有 Rain 在
`WorldWeatherSystem` 的 `ApplyWorldClock` 转换中规范化为 `(RainTimeTicks=0, RainStrength=0)`。
Wind 演进、Slime Rain 和 WorldData packet layout 不改变。

1. **锁定 RED。** WorldRules verifier 在现有 Lantern Night snapshot 上断言 active Rain 应被
   清除；实现前 focused run 运行到该断言失败，证明旧系统仍会继续 Rain。
2. **完成唯一权威路径。** `WorldWeatherSystem.Advance` 只接收不可变 Lantern Night 状态并
   清除普通 Rain；`TryQueueWorldRain` 拒绝 active Lantern Night，Protocol/Server 不写规则。
3. **运行全部 gate。** 当前源树证据在
   `Build/diagnostics/main-migration/task-7-lantern-rain-suppression/20260819-040231/`：
   `world-rules-final.txt`、`persistence-final.txt`、`world-rules-loopback-final.txt`、
   `protocol-compatibility-final.txt`、`full-client-bootstrap-final.txt`、
   `main-boundary-final.txt` 和 `root-release-build-final.txt` 均为 exit `0`；
   MainBoundary 检查 360 个 Simulation 源文件、0 个违规；根 Release build 为
   `0 warnings, 0 errors`。
4. **接受边界。** 本批不声称同 tick Lantern Night 启动的旧随机/排序语义、Slime Rain、云背景、
   `StopRain` 的客户端视觉副作用、音乐、公告或客户端权威操作已迁移。

## 15. Task 7 / Slime Rain negative cooldown 的已接受执行记录

本批迁移 Version4 `Main.cs:13366-13380` 的显式停止结果和 `Main.cs:13423-13428` 的负值
cooldown 回升行为。旧随机 cooldown 生成仍不在本批范围；服务器接收一个显式、已授权的
`WorldSlimeRainStopCommand(cooldownTicks, sequence)`，因此测试和重放不依赖 `Main.rand`。

**本批可证伪命题：** active Slime Rain 才能接受 stop command；提交 stop 后
`SlimeRainTimeTicks=0`、`SlimeRainCooldownTicks=cooldownTicks`，cooldown 每 tick 按
`TicksPerUpdate` 递减，期间新的 start 请求拒绝，归零后重新允许 start。active duration 和
cooldown 不能同时存在；旧版本 WorldData 不增加字段。

1. **锁定 RED。** WorldRules verifier 先引用缺失的 stop command、cooldown 状态和 API，编译失败；
   实现后的 focused run 覆盖 stop 重复、立即重启拒绝、Persistence continuation、归零重启和
   无 active stop 拒绝。
2. **完成唯一权威路径。** `DomeSimulation.TryQueueWorldSlimeRainStop` 是唯一入口，
   `WorldProgressionSystem` 在 `ApplyWorldClock` 处理 stop、递减和 start 互斥；Protocol/Server
   不直接修改 progression。
3. **完成演化契约。** 当前格式从 v14 升为 v15，在既有 progression 末尾追加一个 cooldown
   `Int32`；v1-v14 默认零。Persistence verifier 证明 v15 cooldown round-trip、v14 保留
   Meteor schedule 且 cooldown 默认零，并继续通过 v13/v12/v11/v10 旧布局 fixture。
4. **运行全部 gate。** 当前源树证据在
   `Build/diagnostics/main-migration/task-7-slime-rain-cooldown/20260819-041221/`：
   `world-rules-final.txt`、`persistence-final.txt`、`world-rules-loopback-final.txt`、
   `protocol-compatibility-final.txt`、`full-client-bootstrap-final.txt`、
   `main-boundary-final.txt` 和 `root-release-build-final.txt` 均为 exit `0`；
   MainBoundary 检查 361 个 Simulation 源文件、0 个违规；根 Release build 为
   `0 warnings, 0 errors`。
5. **接受边界。** 本批不声称旧随机负 cooldown 的确切生成分布、warning/公告计时、NPC waves、
   King Slime、自动随机启动或客户端视觉已迁移。

## 16. Task 7 / Wind rain coupling 的已接受执行记录

本批迁移 Version4 `Main.cs:12464-12479` 中雨强对风速平滑目标的确定性影响。旧逻辑先计算
`effectiveTarget = windSpeedTarget * (1f + 5f / 9f * maxRaining)`，再按当前风速与该目标的差值
推进 `WindSpeedCurrent`。本批只迁移这个可独立验证的耦合；随机风向换向、极端风计数器、云数量、
云背景、音乐和客户端视觉不属于本批，也没有修改 `WorldData` layout。

**本批可证伪命题：** 相同 `WindSpeedTarget` 下，雨强为零与雨强为正时的 `WindSpeedCurrent`
必须产生不同且确定性的平滑结果；目标保持在旧 `[-0.8, 0.8]` 边界，当前值允许达到雨强放大后的
最大值（约 `1.2444`），并且快照仍只投影原始 `WindSpeedTarget`。实现使用
`WorldRuleState.AdvanceWind(ticksPerUpdate, rainStrength)`，由 `WorldWeatherSystem` 传入当前
不可变 `RainStrength`，不引入 `Main.rand`、协议字段或额外持久化字段。

1. **锁定 RED。** 实现前 focused verifier 对 clear/rain 两种状态断言相同风速推进，旧实现无法
   证明雨强耦合，随后实现 `AdvanceWind` 后该断言变为 GREEN；当前 verifier 还覆盖确定性、边界、
   snapshot projection 以及全量 WorldRules 回归。
2. **完成唯一权威路径。** `WorldWeatherSystem` 是唯一调用雨强耦合风速推进的 Simulation
   system；`WindSpeedTarget` 仍由已接受命令和状态边界维护，Protocol/Server 不写天气状态，
   不存在客户端或磁盘旁路。
3. **保持演化契约。** 本批不增加持久化字段，既有 Wind target/current 的保存与恢复格式保持
   不变；Persistence verifier 通过严格恢复、round-trip、原子替换和尾随数据拒绝。WorldData
   layout 不变，协议兼容 verifier 继续通过 V1456 长度契约。
4. **运行全部 gate。** 当前源树证据在
   `Build/diagnostics/main-migration/task-7-wind-rain-coupling/20260819-041750/`：
   `world-rules-final.txt`、`persistence-final.txt`、`world-rules-loopback-final.txt`、
   `protocol-compatibility-final.txt`、`full-client-bootstrap-final.txt`、
   `main-boundary-final.txt` 和 `root-release-build-final.txt` 均为 exit `0`；MainBoundary
   检查 361 个 Simulation 源文件、0 个违规；根 Release 为 `0 warnings, 0 errors`。
5. **接受边界。** 本批不声称随机风向换向、极端风计数器、云数量/云背景、天气随机流顺序、
   音乐、客户端视觉或旧客户端权威天气操作已迁移。删除的 Version4 天气实现没有独立 oracle 的
   部分继续记为 `Unknown`，不得用本批的确定性耦合替代它们。

## 17. Task 7 / Slime Rain warning timer 的已接受执行记录

本批迁移 Version4 `Main.cs:14019-14038` 的确定性 warning 倒计时，以及
`Main.cs:13341-13363`、`Main.cs:13366-13385` 对 start/stop 的 warning 初始化。旧逻辑使用
`slimeWarningDelay = 420`；每次 `UpdateSlimeRainWarning` 调用将正值减一，归零时依据当前
`slimeRainTime > 0` 选择开始中或已结束的公告。公告文本、颜色、本地化资源和
`ChatHelper.BroadcastChatMessage` 的客户端广播不在本批，因为当前 V1456 Dome 没有已接受的
服务端聊天输出契约。

**本批可证伪命题：** `WorldSlimeRainStartCommand` 和 `WorldSlimeRainStopCommand` 的
`Announce=true` 在命令被 `WorldProgressionSystem` 接受后设置 420 tick warning，并在同一
`ApplyWorldClock` 调用中完成一次倒计时；warning 从 1 变为 0 时只发布一个
`WorldSlimeRainWarningEvent(IsSlimeRaining)`，active 和 stopped 两种状态都可观察；
`Announce=false` 不创建 warning。warning 是 transient，不增加 Dome persistence 或 WorldData
字段。

1. **锁定 RED。** verifier 先引用缺失的三参数 Slime Rain command、warning 状态和事件读取
   API，当前源树编译失败；RED 输出保存在
   `Build/diagnostics/main-migration/task-7-slime-rain-warning/20260819-043037/world-rules-red.txt`。
2. **完成唯一权威路径。** `WorldProgressionSystem` 在 `ApplyWorldClock` 处理 start/stop、
   warning 设置和每 tick 减一；`DomeSimulation` 只在状态从正数到零时投影一次事件。
   Protocol/Server 不直接改 warning，也没有为公告创建未验证旁路。
3. **保持演化契约。** warning countdown 不写入现有 v15 persistence；Persistence verifier
   明确证明 Slime Rain duration 保留、warning 在 round-trip 后默认零。WorldData layout 和
   V1456 packet length 不变；公告文本/广播继续记录为 unsupported。
4. **运行全部 gate。** 当前源树证据在
   `Build/diagnostics/main-migration/task-7-slime-rain-warning/20260819-043037/`：
   `world-rules-final.txt`、`persistence-final.txt`、`world-rules-loopback-final.txt`、
   `protocol-compatibility-final.txt`、`full-client-bootstrap-final.txt`、
   `main-boundary-final.txt` 和 `root-release-build-final.txt` 均为 exit `0`；MainBoundary
   检查 362 个 Simulation 源文件、0 个违规；根 Release 为 `0 warnings, 0 errors`。
5. **接受边界。** 本批不声称 `Lang.gen[74/75]` 文本、本地化、颜色、客户端聊天广播、
   自动随机 Slime Rain 启动、NPC waves、King Slime 或旧随机流顺序已迁移。下一批必须先取得
   服务端聊天输出 oracle 或独立的同 tick Lantern Night 顺序 oracle。

## 18. Task 7 / Lantern Night same-tick Rain ordering 的已接受执行记录

本批迁移 Version4 `Main.cs:13392-13438` 的 phase ordering：`UpdateTime` 先读取
`LanternNight.LanternsUp` 并处理当前 Rain，之后在 `Main.cs:13530` 调用
`LanternNight.UpdateTime`。因此一个 tick 开始前已接受的 Lantern Night intent 必须在该 tick
的天气 authority 中生效，既清除现有 Rain，也阻止同 tick 的普通 Rain start；事件状态仍由
`WorldProgressionSystem` 在 `ApplyWorldClock` 命名 phase 提交。

**本批可证伪命题：** 夜间已验证的 `WorldEventStartCommand(LanternNight)` pending 时，
`WorldWeatherSystem` 使用“当前 Lantern Night 或 pending Lantern Night”作为 authority；
已有 Rain 不应只减少一个 tick，而应在本 tick 归零；同 tick 排队的普通 Rain start 不应提交。
下一 tick 的已 active Lantern Night 继续清除 Rain。普通无 pending 的天气和原有事件状态机不变。

1. **锁定 RED。** verifier 在实现前让夜间 snapshot 同时拥有 Rain 和 Lantern Night request，
   旧 ECS 结果保留 Rain（从 3 变 2），并在 Rain/Lantern start 冲突中接受 Rain；运行时失败证据
   保存在 `Build/diagnostics/main-migration/task-7-lantern-same-tick-ordering/20260819-044056/world-rules-red.txt`。
2. **完成唯一权威路径。** `DomeSimulation` 只把已通过 `TryQueueWorldEvent` 的 pending Lantern
   request 投影给 `WorldWeatherSystem`；天气仍由 system 修改，随后 progression system 提交
   Lantern state。没有把 pending 状态暴露给 Protocol 或客户端写入口。
3. **保持演化契约。** 本批不修改 WorldProgression persistence layout、WorldData 字段或 V1456
   packet length。Persistence verifier 保留现有 Lantern/Rain 字段兼容，协议 verifier 继续
   验证固定长度和 isolation。
4. **运行全部 gate。** 当前源树证据在
   `Build/diagnostics/main-migration/task-7-lantern-same-tick-ordering/20260819-044056/`：
   `world-rules-final.txt`、`persistence-final.txt`、`world-rules-loopback-final.txt`、
   `protocol-compatibility-final.txt`、`full-client-bootstrap-final.txt`、
   `main-boundary-final.txt` 和 `root-release-build-final.txt` 均为 exit `0`；MainBoundary
   检查 362 个 Simulation 源文件、0 个违规；根 Release 为 `0 warnings, 0 errors`。
5. **接受边界。** 本批不声称 Lantern Night 的随机资格、`NextNightIsLanternNight`、cooldown、
   Genuine/Manual 持久化字段、云背景/音乐、客户端 `NetMessage.SendData(7)` 或 deleted
   `NaturalAttempt` 已迁移；这些仍是独立 source/oracle 批次。

## 19. 任务间的交接契约

下一 Task 只能依赖已接受的公开契约，不得依赖 `DomeSimulation` 的私有字典或测试夹具：

- Task 5 输出 `PlayerStore`、`NpcStore`、`ProjectileStore`、`WorldItemStore` 的所有权和墓碑
  语义，以及稳定公开 snapshot。
- Task 6 输入值对象世界元数据和 snapshot，输出唯一的 server bootstrap 路径。
- Task 7 输入确定性时钟、种子和 progression state，输出逐事件族状态机及保存/重放证据。
- Task 8 仅为已经验收的 system 提供不可变 Definitions；未知 ID 必须显式拒绝或记录为
  opaque compatibility record。
- Task 9 输出参数化纯函数，不得借此从 Main 取全局状态。
- Task 10 只改变 `DomeSimulation` 的内部位置，不得改变任何已验收快照、phase 或协议行为。

## 20. 防止 AI 假修复的规则

- 不删除、放宽或跳过失败断言来获得 GREEN；断言变化必须先证明参考行为本身错误。
- 不用 `try/catch` 吞掉未知命令、无效 ID、保存格式错误或协议异常。
- 不把测试专用开关、延迟初始化和未验证 fallback 作为正式迁移实现。
- 不混合多个领域的重构；每个变更集只回答一个验收命题。
- 不在验证后继续编辑而不重跑验证；证据必须含运行时间、源树状态和退出码。
- 不把删除的 Version4 文件视为“没有行为”；在账本中保留 `Unknown`，直到获得独立 oracle。
- 不使用 `git reset --hard`、`git clean` 或宽范围删除恢复工作区；回滚只限当前 Task 文件。

## 21. 一个切片的完成定义

只有当责任账本为该行为提供 reference 和 destination，Simulation 中存在显式状态/定义/命令，
突变通过命名 System 在确定 phase 执行，成功和拒绝都由 verifier 覆盖，相关 snapshot/持久化/
协议投影已验证，MainBoundary 与当前源树 Release build 通过，并且 `progress.md` 记录余下不支持
行为时，才可以说该切片“完成”。

整个 Main 服务器迁移在 Task 11 前不得称为完成；Task 0-4 的已接受证据不意味着完整 Terraria
服务器逻辑已经等价。

## 22. 下一批执行卡：Lantern Night schedule state

### 22.1 批次目标

下一批只回答一个命题：旧世界中已经被 NPC 事件设置的
`NextNightIsLanternNight` 是否能作为服务器拥有的、可保存恢复的“下一夜预约”状态，且预约
不会在错误的 phase 被消费或被客户端直接写入。

本批使用三个只读 oracle：

| Oracle | 位置 | 关键证据 |
|---|---|---|
| 完整 Lantern Night 实现 | `D:\TRbackup\无任何删减通过编译\Terraria.GameContent.Events\LanternNight.cs` | `NextNightIsLanternNight` 声明、`CheckMorning`、`NaturalAttempt`、`UpdateTime` |
| NPC 事件完成路径 | `D:\TRbackup\无任何删减通过编译\Terraria\NPC.cs` | `OnGameEventClearedForTheFirstTime` 对预约字段的写入 |
| WorldFile 持久化路径 | `D:\TRbackup\Version4物理删除了某些文件\Terraria.IO\WorldFile.cs` | `_tempLanternNightNextNightIsGenuine` 的读、写和恢复 |

### 22.2 明确范围

本批允许迁移：

- 一个命名明确的 ECS 预约状态，例如 `IsNextNightLanternNight`；
- 一个只由 progression/NPC 事件 authority 产生的预约命令；
- 一个确定 phase 的预约提交和夜间消费规则；
- v15 -> v16 的持久化扩展，以及 v15 读取时新字段默认为 `false`；
- 重启、TCP loopback、V1456 固定包长度、MainBoundary 和 root Release 证据。

本批禁止混入：

- `Main.rand.Next(14)`、`Main.rand.Next(5, 11)` 或任何未定义的随机流迁移；
- `NaturalAttempt` 的完整资格条件、`NPC.downedMoonlord` 读取、Boss 活跃判断；
- Genuine/Manual 灯笼状态、奖励、NPC 波次、音乐、公告和客户端视觉；
- 为预约状态新增 WorldData 字段或改变 V1456 `NetMessage` 包布局；
- 通过 Protocol、Server 或测试夹具直接修改 `WorldProgressionState`。

若 RED 无法在“预约产生”和“预约消费”的边界上建立可证伪命题，立即将该行为标记为
`Unknown`，保存 oracle 行号和失败原因，不得用猜测实现随机资格。

### 22.3 逐步执行

**Step 1：冻结 source evidence。**

记录三个 oracle 的行号、文件长度和 SHA-256，写入
`Build/diagnostics/main-migration/task-7-lantern-schedule/<timestamp>/source-manifest.txt`。
记录当前 `WorldProgressionState`、`DomeStatePersistenceFormat` 和现有 Lantern Night verifier
的路径；不修改 oracle 文件。

**Step 2：先写 RED verifier。**

在 `Test/Terraria.Dome.WorldRules.Verification/Program.cs` 增加一个独立场景，至少证明：

1. 夜间接收到已授权预约命令后，预约状态可从公开 snapshot 观察到；
2. 同一预约序列号不能重复提交；
3. 白天不会把预约错误地消费为 Lantern Night；
4. 进入夜间的指定 phase 后只消费一次，下一夜不会再次自动消费；
5. 未授权的直接状态写入不存在，或在 authority API 中被拒绝。

先运行 verifier。预期必须是编译失败或运行失败，失败输出保存为
`world-rules-red.txt`；没有 RED 证据不得开始写生产实现。

**Step 3：实现最小 authority。**

只修改以下边界内的文件：

- `src/Terraria.Dome.Simulation/World/WorldProgressionState.cs`；
- `src/Terraria.Dome.Simulation/World/` 下与预约命令同名的新文件；
- `src/Terraria.Dome.Simulation/World/Systems/WorldProgressionSystem.cs`；
- `src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs` 的入队和 phase 编排；
- 对应 WorldRules verifier。

预约命令必须携带可重放的 `Sequence`，验证正数/非负约束、重复序列和当前世界 phase。命令
进入队列后只能由 `WorldProgressionSystem` 提交状态；消费动作必须产生新的不可变 snapshot，
不能在 Protocol 或 Server 层写字段。

**Step 4：实现持久化兼容。**

若 Step 2 的 RED 明确要求预约跨重启保留，则：

- `src/Terraria.Dome.Server/Persistence/DomeStatePersistenceFormat.cs` 将当前版本从 v15 升到
  v16，并把新字段追加到已有 Lantern/Slime Rain progression 字段之后；
- `Test/Terraria.Dome.Persistence.Verification/Program.cs` 增加 v16 round-trip；
- 更新 v15、v14、v13、v12、v11、v10 fixture 的 offset helper 和严格尾部检查；
- 证明 v15 及更旧 payload 读取时新预约字段为 `false`，而既有 Rain、Slime Rain、Wind、Lantern
  字段不漂移。

若命题只要求运行期预约而不要求保存，必须在 progress.md 写出 archive 中 WorldFile 证据与
当前产品范围的冲突，并把该项标为 `Deferred`，不得静默省略。

**Step 5：跑 focused GREEN 和回归。**

使用新鲜证据目录，按顺序执行：

```powershell
dotnet run --project Test/Terraria.Dome.WorldRules.Verification/Terraria.Dome.WorldRules.Verification.csproj -c Release --no-restore -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false *> world-rules-final.txt
dotnet run --project Test/Terraria.Dome.Persistence.Verification/Terraria.Dome.Persistence.Verification.csproj -c Release --no-restore -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false *> persistence-final.txt
```

两个命令都必须退出 `0`。若失败，先修复最小责任边界并重新生成 RED/GREEN；不得删断言、放宽
异常或把失败转换成日志。

**Step 6：跑集成 gates。**

继续保存以下独立产物，任何一项缺失都只能标 `partial`：

```text
world-rules-loopback-final.txt
protocol-compatibility-final.txt
full-client-bootstrap-final.txt
main-boundary-final.txt
root-release-build-final.txt
```

根 Release 必须使用：

```powershell
dotnet build Terraria.Dome.sln -c Release -m:1 -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:FixtureHostBuild=false
```

必须确认 `MainBoundary` 的违规数为 `0`，Simulation 没有引用 `Terraria.Main`、Protocol、Server、
socket、XNA/UI 或磁盘路径，且 root Release 的 warnings/errors 均为 `0`。

**Step 7：记录接受边界并交接。**

在 `progress.md` 追加本批的 source、authority、RED、GREEN、持久化版本、所有 gate 退出码和
未迁移项。只有所有证据均为新鲜产物且退出码正确，才写“schedule state accepted”；否则写
`PARTIAL` 或 `BLOCKED_EXTERNAL_ORACLE`，并给出下一步唯一可执行动作。

## 23. AI 持续执行协议

AI 每次接手任务时，先读取本文件、`progress.md`、责任账本和当前工作树状态，然后把当前批次
写成一条状态记录。状态只能沿以下方向前进，不能从 `RED_CAPTURED` 直接跳到 `ACCEPTED`：

```text
READY
  -> SOURCE_FROZEN
  -> RED_CAPTURED
  -> IMPLEMENTING
  -> FOCUSED_GREEN
  -> PERSISTENCE_GREEN
  -> LOOPBACK_GREEN
  -> COMPATIBILITY_GREEN
  -> BOUNDARY_GREEN
  -> RELEASE_GREEN
  -> ACCEPTED
```

每一轮必须输出三项短记录：

1. `Now`：当前状态、正在处理的单一命题和写集；
2. `Evidence`：刚运行的命令、退出码、产物路径、失败首因；
3. `Next`：下一步唯一动作，以及继续执行的停止条件。

遇到编译失败时，AI 先读取完整错误并检查相关接口、构造函数、调用方和项目引用，再执行最小
修复；同一错误连续两次仍未缩小，切换到“契约检查”而不是继续扩大 diff。遇到行为失败时，
保留失败输入和 snapshot，先确认 phase、队列顺序和 authority，再修改实现。遇到缺失 oracle、
缺失用户决策或源文件物理删除时，状态设为 `BLOCKED_EXTERNAL_ORACLE`，记录如何恢复证据；
不得捏造 parity，也不得等待用户逐行指导。

一个批次成功后，AI 必须主动从责任账本选择下一条仍为 `Unknown`/`Partial` 的行为，但只能
选择一个行为族，并先生成新的 source-manifest 和 RED。不得因为上一批 GREEN 就顺手重构
`DomeSimulation`、批量改命名或清理无关文件。

## 24. Main.cs 拆分后的固定目标形状

`Main.cs` 不再对应一个等价的大类。拆分只允许沿以下依赖方向发生：

```text
输入命令 -> authority validation -> Simulation phase
                                  -> named domain systems
                                  -> immutable snapshots
                                  -> persistence/protocol projections
```

建议的长期目录边界：

| Main.cs 职责 | ECS 目标 | 允许依赖 |
|---|---|---|
| `Update`/`DoUpdateInWorld` | `SimulationTickSchedule` | 时钟、命令队列、领域 systems |
| `UpdateTime` | `WorldClockSystem` + `WorldProgressionSystem` | `WorldClockSnapshot`、progression snapshot |
| `UpdateWeather` | `WorldWeatherSystem` | weather snapshot、已验收 progression |
| `Start/UpdateInvasion` | `WorldInvasionSystem` | invasion commands、progression snapshot |
| `Start/StopSlimeRain` | Slime Rain systems | Slime Rain commands、weather snapshot |
| `HandleMeteorFall` | Meteor systems | deterministic meteor commands、world grid contract |
| 实体数组与 active slot | domain stores | stable handle、墓碑、公开 snapshots |
| `NetMessage`/`MessageBuffer` 调用 | Server adapters | immutable snapshot、wire compatibility contract |
| save/load | Server persistence | versioned value-only snapshot |

领域 System 不得反向调用 `Main`、`NetMessage`、`WorldFile` 或 socket。若一个旧方法同时包含
服务器状态、客户端效果和网络发送，先拆成“状态突变”和“投影副作用”两个命题；没有独立
oracle 的副作用留在 `Unknown`，不能把它塞进 Simulation 以便通过编译。

## 25. AI 交接模板

每个批次完成或阻塞时，在 `progress.md` 使用以下最小模板：

```text
# YYYY-MM-DD Main ECS migration Task N <behavior> <status>

Source: <exact files and lines, plus archive/baseline hash>
Proposition: <one falsifiable sentence>
Authority: <command -> validation -> system -> snapshot>
RED: <artifact path, exit code, failure reason>
GREEN: <artifact path, exit code>
Persistence: <version, old-layout defaults, strict-tail result>
Loopback: <artifact path, exit code>
Protocol: <layout decision and artifact path>
Boundary: <checked files, violations>
Release: <command, exit code, warnings/errors>
Accepted boundary: <what is migrated>
Deferred/Unknown: <what is explicitly not migrated and why>
Next action: <one concrete source-backed action>
```

该模板是 AI 的交接契约，不是完成声明。`Task 7` 在所有事件族和未知行为完成前始终保持
`IN PROGRESS`；任何单个 slice 的接受都不能提升整个 Main 迁移状态。

## 26. Task 7 / Lantern Night schedule state 的已接受执行记录

本批将完整 archive 中的 `LanternNight.NextNightIsLanternNight` 提取为
`WorldProgressionState.IsNextNightLanternNight`。完整 oracle 的
`LanternNight.cs:91-116` 证明：在可启动的夜间，预约被消费、清为 `false`，并进入 genuine
Lantern Night；`NPC.cs:79968-79984` 证明旧 NPC 首次事件完成会设置预约；Version4
`WorldFile.cs:175-178`、`1093-1096`、`1419-1422`、`2370-2382` 证明它是持久世界状态。

**本批可证伪命题：** 服务器授权的有效预约命令在白天提交后只能保留预约；进入没有 Blood
Moon、Invasion 或 scheduled Meteor 的夜间时，`WorldProgressionSystem` 只消费一次，设定
`IsLanternNight=true` 并清除预约；重复或负序列预约被拒绝；v16 保存恢复预约，v15 及更旧
layout 默认 `false`，V1456 WorldData 不增加字段也不改变长度。

1. **锁定 RED。** WorldRules verifier 先引用不存在的
   `WorldLanternNightScheduleCommand`、`TryQueueWorldLanternNightSchedule` 和
   `IsNextNightLanternNight`。编译以缺少这三个 API 失败；修正 verifier 的 clock 参数名后，
   RED 只保留预约 API 缺失，未修改生产实现。
2. **完成唯一 authority path。** `WorldLanternNightScheduleCommand(Sequence)` 是独立、
   可重放的命令。`DomeSimulation` 验证非负序列、拒绝 pending/已保存预约，然后仅把命令放入
   私有队列；`WorldProgressionSystem` 是唯一提交和消费状态的位置。搜索证明该 API 没有
   `MessageBuffer`、V1456 dispatcher 或 Server 请求调用点，因此客户端没有该写入口。
3. **完成 persistence evolution。** `DomeStatePersistenceFormat` 升为 v16，并在 progression
   payload 尾部追加一个 Boolean。v15 及以前的读取不会读取该字节而默认 `false`；读取器继续
   拒绝 trailing data。Persistence verifier 的 v10-v14 合成 fixture 显式删除 v16 尾字段，
   避免通过放宽 strict reader 伪造兼容。
4. **运行全部 gate。** 当前源树的新鲜证据在
   `Build/diagnostics/main-migration/task-7-lantern-schedule/20260819-050007/`：
   `world-rules-red.txt` 和 `persistence-red.txt` 记录预期失败；
   `world-rules-final.txt`、`persistence-final.txt`、`world-rules-loopback-final.txt`、
   `protocol-compatibility-final.txt`、`full-client-bootstrap-final.txt`、
   `main-boundary-final.txt` 和 `root-release-build-final.txt` 均为 exit `0`。MainBoundary 检查
   366 个 Simulation 源文件、0 个违规；根 Release 为 0 warnings、0 errors。
5. **接受边界。** 本批没有迁移 NPC 的
   `OnGameEventClearedForTheFirstTime` 到 ECS hook，也没有迁移 `NaturalAttempt` 的
   `Main.rand.Next(14)`、`NPC.downedMoonlord`、Boss/节日/事件资格、随机 cooldown、
   Genuine/Manual 区分、奖励、客户端 `SendData(7)`、云/音乐/公告。下一批必须先为 NPC
   first-event authority 提供 source-backed command path，或为 NaturalAttempt 的资格建立独立
   oracle；不能把两者与本批的保存状态混合。

## 27. Task 7 / NPC first-event Lantern Night authority 的已接受执行记录

本批收窄完整 archive `NPC.SetEventFlagCleared` 与
`NPC.OnGameEventClearedForTheFirstTime` 的明确子行为。`NPC.cs:79956-79966` 证明只有一个
event flag 从 false 变为 true 时才进入 first-time handler；`NPC.cs:79972-79994` 证明除
`4`、`21`、`22` 外的 event ID 会设置 Lantern Night 的 next-night state。其他 switch 分支的
credits、Plantera bulb 与 dual-dungeon 墙体效果不属于 Lantern Night authority。

**本批可证伪命题：** 已由 NPC 上游证明为“首次清除”的有效
`NpcGameEventFirstClearCommand(gameEventId, sequence)`，当 ID 不是 `4/21/22` 时只能预约
一次 Lantern Night；重复或负值被拒绝，例外 ID 不会改变预约。客户端、V1456 dispatcher、
Server 请求和公开 generic schedule API 都不能直接把预约写入 progression state。

1. **锁定 RED。** WorldRules verifier 先引用不存在的 command 和
   `TryQueueNpcGameEventFirstClear`，并断言 ID `1` 预约、ID `4` 不预约、重复/负输入被拒绝。
   `world-rules-red.txt` 以缺少类型和 API 的编译错误退出 `1`。
2. **完成 authority path。** `NpcGameEventFirstClearCommand` 在 `Npc/Commands` 中验证非负
   ID/Sequence，并显式表达 `4/21/22` 的不预约例外。`DomeSimulation` 的公开 NPC-only entry
   验证该 command 后调用 private `TryQueueWorldLanternNightSchedule`；原 public generic
   schedule API 已移除。`WorldProgressionSystem` 仍是唯一的 state 提交/夜间消费 owner。
3. **演进决定。** 此批只转换已存在的预约 command/state，没有新增保存或 V1456 字段；v16
   persistence 用前一批已接受的 state layout 保留预约。Persistence、loopback 和 protocol
   verifier 仍完整运行，确认没有把 NPC command 暴露为 wire input。
4. **运行全部 gate。** 新鲜证据目录为
   `Build/diagnostics/main-migration/task-7-npc-first-event-lantern-schedule/`：
   `world-rules-final.txt`、`persistence-final.txt`、`world-rules-loopback-final.txt`、
   `protocol-compatibility-final.txt`、`full-client-bootstrap-final.txt`、
   `main-boundary-final.txt` 和 `root-release-build-final.txt` 全部 exit `0`。MainBoundary
   检查 369 个 Simulation 源文件、0 个违规；根 Release 为 0 warnings、0 errors。
5. **接受边界。** 该 command 表示“上游已经确认 first clear”，并不等同于已迁移 NPC death、
   boss flag 或通用 event-flag lifecycle。下一批需要把一个具体、已有 ECS NPC death/progression
   原因映射为 first-clear command，或停止在这条已验证的 authority contract；不得将 source 中
   未迁移的 credits、Plantera、dual-dungeon、随机 Lantern Night 资格伪装为已完成。

## 28. NPC producer audit：当前不能建立 death -> first-clear 映射

接受 Task 7 / NPC first-event command 后，审计了当前源树的 producer 候选：

| 当前对象 | 事实 | 结论 |
|---|---|---|
| `NpcDeathSystem` | 输入仅有 handle、health、active、position、loot table；不携带 legacy NPC type 或 game event ID | 不能推断 `NPC.cs` 的 switch case |
| `DomeSimulation` 的 `_npcDeathSystem`/`_pendingNpcDeaths` | 当前没有生产或消费调用点 | 不能作为 first-clear lifecycle hook |
| `NpcDefinition` / 默认 definitions | 仅有 DefinitionId、NetId、行为/阵营/分类；当前仅 fixture/chaser/town 样本，没有 source-backed boss type -> event ID 映射 | 不能将任意 death 映射为 first clear |
| archive `NPC.cs:80441-80718` | `SetEventFlagCleared` 的 producer 映射依赖具体 legacy NPC type、Boss 条件、已有 event flag 和若干额外 side effect | 当前缺少必要 authority/state |

因此 `NpcGameEventFirstClearCommand` 保持为一个由未来 NPC progression subsystem 产生的公开
server command contract，而不是将现有 generic damage/death 误接入。此结论不阻塞 Task 7：下一
候选应回到完整 `LanternNight.NaturalAttempt` oracle，先选择一个不需要缺失 NPC type 表的独立
命题，例如持久化的 cooldown state 与明确的 eligibility-attempt boundary；随机流、Boss 活跃、
Pumpkin/Snow Moon 和 Moon Lord 条件仍必须单列。

## 29. Task 7 / Lantern Night cooldown state 的已接受执行记录

本批只提取 archive 明确保存的 `LanternNightsOnCooldown` 状态，不声称完整
`NaturalAttempt` 已迁移。oracle 为完整 `LanternNight.cs:91-115`（正数 cooldown 在
`LanternsCanStart()` 成功后递减，随机资格和随机新 cooldown 随后执行）以及 Version4
`WorldFile.cs:1419-1422`、`2370-2382`（cooldown 以 `Int32` 保存、旧版本缺失时归零）。

**本批可证伪命题：** `WorldProgressionState.LanternNightCooldownTicks` 是非负的 server
progression state；v17 当前 payload round-trip 保留值，v16 及更旧 payload 默认 `0`，严格尾
部检查和 V1456 wire layout 不改变。此命题不包含 cooldown 自动递减、`Main.rand.Next(5,11)`、
`NPC.downedMoonlord` 或完整 `LanternsCanStart`。

1. **锁定 RED。** Persistence verifier 先引用不存在的 constructor parameter/property；
   `persistence-red.txt` 退出 `1`，只显示 cooldown contract 缺失。
2. **完成 state/persistence。** `WorldProgressionState` 增加非负验证并让所有现有
   `With...` 方法保留 cooldown。`DomeStatePersistenceFormat` 升为 v17，在 v16 schedule
   Boolean 之后追加一个 Int32；format <17 读取时默认 `0`。历史 v10-v16 fixture 显式删除
   新尾部字段，reader 仍拒绝未预期 trailing data。
3. **保留未迁移行为。** 当前 ECS 没有完整 `LanternsCanStart` 的 Boss/事件资格 oracle，
   因此没有编写“每 tick 递减”或随机 cooldown 伪实现。cooldown state 的 producer/consumer
   仍是下一独立命题，不能由 persistence GREEN 推导出来。
4. **运行全部 gate。** 新鲜证据目录为
   `Build/diagnostics/main-migration/task-7-lantern-cooldown/20260819-113103/`：
   `world-rules-final.txt`、`persistence-final.txt`、`world-rules-loopback-final.txt`、
   `protocol-compatibility-final.txt`、`full-client-bootstrap-final.txt`、
   `main-boundary-final.txt` 和 `root-release-build-final.txt` 均 exit `0`。MainBoundary 检查
   370 个 Simulation 文件、0 个违规；根 Release 为 0 warnings、0 errors。
5. **接受边界。** 只接受 cooldown 的显式状态、保存恢复和旧版本默认值。随机 NaturalAttempt、
   eligibility、递减 phase、Genuine/Manual、NPC effects、公告、音乐、云和客户端消息仍为
   `Deferred`/`Unknown`。

## 30. Task 7 / Lantern Night eligibility contract 的已接受执行记录

完整 archive `LanternNight.cs:54-89` 定义了两个可独立提取的纯判断：`LanternsCanPersist`
要求夜晚和 `LanternsCanStart`，后者拒绝 scheduled Meteor、Blood Moon、Pumpkin Moon、Snow
Moon、非零 invasion、非零 Moon Lord countdown，以及 active boss 或 active legacy type
`13..15`。本批将这组条件完整保留为无副作用 Simulation contract，避免将未拥有的世界输入
默认为安全值。

**本批可证伪命题：** 对同一 immutable progression 和 eligibility snapshot，纯 system 只有在
全部 legacy guard clear 时返回 true；白天 persist 返回 false；每个阻断条件单独使结果为 false，
inactive boss 不阻断。

1. **锁定 RED。** WorldRules verifier 先引用不存在的
   `LanternNightEligibilitySystem`、`LanternNightEligibilitySnapshot` 和 NPC snapshot；
   `world-rules-red.txt` exit `1`，失败仅来自缺失纯 contract。
2. **完成 pure contract。** `LanternNightEligibilitySnapshot` 明确承载 Pumpkin/Snow、Moon
   Lord countdown 和不可变 NPC 列表；`LanternNightEligibilitySystem` 从 existing progression
   读取 Meteor/Blood Moon/Invasion，扫描 active `IsBoss` 与 legacy type `13..15`。它不读
   `Main`、世界静态数组、Protocol 或 socket。
3. **不接入 tick。** 当前源树尚未拥有 Pumpkin/Snow、Moon Lord countdown 和完整 boss state
   的 authoritative producer。故本批没有让 system 默认构造 eligibility，也没有改变
   `WorldProgressionSystem` 的 NaturalAttempt/clock 行为。下一批只能在这些值有明确 owner 后，
   构造 snapshot 并接入一个 named attempt phase。
4. **运行全部 gate。** 新鲜证据目录：
   `Build/diagnostics/main-migration/task-7-lantern-eligibility/20260819-114525/`。所有 focused、
   persistence、loopback、protocol compatibility、full bootstrap、MainBoundary 和 root Release
   产物均 exit `0`。MainBoundary 检查 373 个 Simulation 文件、0 个违规；根 Release 为 0
   warnings、0 errors。
5. **接受边界。** 只接受可复用的纯 eligibility 判定；不接受它已经接入实时世界、cooldown
   递减、random attempt、Genuine state 或 Lantern Night 自动启动。输入 producer 和 phase
   ownership 保持 `Unknown`。

## 31. Eligibility input authority audit：暂不接入 NaturalAttempt phase

eligibility contract 接受后，审计当前源树的 input producer：

| archive input | 当前可观察事实 | 结论 |
|---|---|---|
| active / type `13..15` / boss NPC | `DomeSimulation` 可从 lifecycle 和 `NpcDefinitionComponent` 得到 active、DefinitionId、NetId；但 `NpcDefinition` 没有 `IsBoss`，默认 definitions 没有 legacy boss/type 映射 | 不能构造无损 active-boss snapshot |
| Pumpkin / Snow Moon | Simulation 搜索无 state、command 或 system | 没有 owner |
| Moon Lord countdown | Simulation 搜索无 state、command 或 system | 没有 owner |
| Meteor / Blood Moon / invasion | 已接受 `WorldProgressionState` 字段 | 可作为 pure contract 输入，但不足以完成全部 eligibility |

不能因当前无 boss/Pumpkin/Snow/MoonLord 值就将它们固定为 `false/0` 并接入 tick。后续要么迁移
这些世界 event states 及 boss definition contract，要么保持 `NaturalAttempt` phase 为 `Unknown`。
下一独立可迁移行为改为 archive/WorldFile 均明确的 Manual/Genuine Lantern state，仍不依赖这些
实时 eligibility inputs。

## 32. Manual/Genuine Lantern Night 的前置 authority gate

下一批并非直接给 `WorldProgressionState` 增加两个 Boolean。archive 证明
`LanternsUp = GenuineLanterns || ManualLanterns`，`UpdateTime` 只清除 Genuine、`CheckMorning`
清除两者，WorldFile 分别保存两者，而 V1456 WorldData 只投影有效 OR 结果。当前 ECS 的单一
`IsLanternNight` 同时被 generic event command、已接受 schedule 路径、Rain suppression、保存和
wire projection 使用，尚无 source-backed mode provenance。

因此新增执行计划
[`2026-08-19-lantern-night-manual-genuine-migration-execution.md`](2026-08-19-lantern-night-manual-genuine-migration-execution.md)
要求先恢复 `ToggleManualLanterns` 的服务器 producer，并在 `IMPLEMENTABLE_MANUAL_AUTHORITY`、
`MANUAL_AUTHORITY_UNPROVEN`、`CONFLICTING_EXISTING_DOME_SEMANTICS` 三种结论之一中收敛。
未证明 authority 或 v17 compatibility mapping 时，不得新增 Manual toggle API、Protocol dispatcher、
v18 persistence 字段，也不得把旧 `IsLanternNight` 任意映射为 Manual 或 Genuine。

## 33. Manual/Genuine authority gate 已收敛；normal event eligibility pure contract 已接受

`Build/diagnostics/main-migration/task-7-lantern-manual-genuine/20260819-120052/` 的 complete
archive 与 Version4 搜索已收敛为 `MANUAL_AUTHORITY_UNPROVEN`：archive 中
`ToggleManualLanterns` 只有定义本身，Version4 无该符号；`MessageBuffer` 的 Manual bit 读取只在
`netMode == 1` 的 client WorldData 消费路径。故没有新增 client request、Manual command、v18
format 字段，也没有给 v17 的 `IsLanternNight` 猜测 Manual/Genuine provenance。

随后独立提取了 Version4 `Main.ShouldNormalEventsBeAbleToStart` 的完整 pure predicate。
`NormalEventEligibilitySnapshot` 明确承载 Lunar Apocalypse、active legacy NPC type 398 和 Moon
Lord countdown；`NormalEventEligibilitySystem.ShouldBlockNormalEvents` 仅以这三个值和有效 Lantern
Night 值计算 legacy `stopEvents`。它不接入 tick、不持久化、不投影，因为这三个非 Lantern 输入仍
无 Simulation authority owner。RED/全部 GREEN 证据在
`Build/diagnostics/main-migration/task-7-normal-event-eligibility/20260819-120353/`：WorldRules、
Persistence、loopback、Protocol、full bootstrap、MainBoundary 和 serial root Release 均为 exit `0`；
MainBoundary 为 381 files/0 violations，root Release 为 0 warnings/0 errors。

## 34. King Slime readiness pure contract 已接受

Version4 `Main.cs:13710-13722` 的 `AnyPlayerReadyToFightKingSlime` 是一个独立纯谓词：只要有
active player 的 `statLifeMax > 140 && statDefense > 8`，它就返回 true。该结果在普通 slime
event 概率分支和 remix NPC death 分支中被消费；二者还依赖未迁移随机流、NPC 定义和事件 owner，
所以本批只抽取谓词而不接入任一 spawn path。

`KingSlimePlayerSnapshot` 显式承载 active、maximum health 和 defense；
`KingSlimeReadinessSystem.HasReadyPlayer` 保留严格大于而不是将遗产阈值错误改为大于等于。当前
Simulation 没有 source-backed legacy `statDefense` producer，故它既不从 Protocol slot 读取，也不
默认 defense。证据在
`Build/diagnostics/main-migration/task-7-king-slime-readiness/20260819-121052/`：RED 为缺失类型，
随后 WorldRules、Persistence、loopback、Protocol、full bootstrap、MainBoundary 和 serial root
Release 全部 exit `0`；MainBoundary 383 files/0 violations，root Release 0 warnings/0 errors。

### King Slime player-stat input audit

该 pure contract 接受后，审计确认 `HealthComponent.Maximum` 和 `PlayerLifecycleComponent.IsActive`
已有 Simulation owner；`EquipmentStatSystem` 在 commit phase 将已支持、非 vanity 装备 definition
的 defense 总和写入 `DefenseComponent`。但 Version4 `Player.statDefense` 还由 buffs、prefixes、
consumables、stances 等多个路径改变，最后才非负截断；当前装备系统没有这些项。因此
`DefenseComponent` 不能被声称为 legacy `statDefense` 的完整 source-backed 投影，不能据此连接
King Slime 的随机 world-event 或 NPC death spawn。细节与原始搜索记录在
`task-7-king-slime-readiness/20260819-121052/player-stat-authority-audit.txt`。

## 35. Task 9 / Damage calculation 的已接受执行记录

Version4 `Main.cs:14670-14712` 与完整 archive `Main.cs:67056-67098` 都定义相同的三种纯
减伤函数。NPC 和 PvP 使用 `max(1, damage - defense * 0.5)`；普通玩家同样使用半防御，
Expert 使用 `0.75` 倍防御，Master 使用完整防御。`Player.cs` 和 `NPC.cs` 的最终生命写入将
该 double 结果转换为整数；当前 Dome 的显式生命组件采用截断向零的确定性映射。

**本批可证伪命题：** 对 `raw=20, defense=5`，NPC、普通玩家与 PvP 的函数结果为 `17.5`
且生命提交量为 `17`；Expert 为 `16.25 -> 16`，Master 为 `15 -> 15`。任意正伤害在防御后
至少提交 `1`，零或负的原始伤害仍被现有命令验证拒绝。Dome 的 direct NPC、projectile NPC
和 non-PvP player command 都只能经过同一命名的计算契约，不允许 projectile path 旁路防御。

1. **锁定 RED。** `Terraria.Dome.Npc.Verification` 先引用不存在的
   `DamageCalculationSystem` 和 `DamageTargetKind`，并以 `20/5` 断言先前错误的 `15` 应改为
   `17`；`npc-damage-red.txt` 为预期的缺失类型编译错误。
2. **完成深模块。** `Combat/DamageTargetKind.cs` 仅表达 NPC、player、player-PvP 的目标
   分类；`DamageCalculationSystem` 以 `WorldRuleState` 的既有 Expert/Master 权威状态计算并
   返回 legacy double，再以显式 `CalculateAppliedAmount` 截断为 Dome 的 `int` 生命变更。
   `DamageResolutionSystem` 继续拥有免疫、存活检查、生命夹紧和结果事实，不重写这些责任。
3. **完成既有路径接入。** 玩家 damage command 以 `Player` 目标类别接入；direct NPC command
   和 projectile `DamageCommand` 都以 `Npc` 类别接入。没有为 PvP 虚构 queue、协议消息、
   client field 或网络 authority；PvP 只由纯 verifier 覆盖。
4. **验证真实 authority。** Combat verifier 从带有给定 `WorldRuleState` 的持久化 snapshot
   恢复 Simulation，经既有 Item 4 装备命令产生 `DefenseComponent(5)`，再提交同样的玩家伤害。
   普通、Expert、Master 的最终生命分别为 `83`、`84`、`85`，证明难度不是单元函数幻象。
5. **运行全部 gate。** 新鲜证据目录为
   `Build/diagnostics/main-migration/task-9-damage-calculation/20260819-122505/`。NPC、Combat、
   Persistence、WorldRules TCP loopback、Protocol compatibility、FullClientBootstrap、
   MainBoundary 和串行 root Release 均 exit `0`。MainBoundary 检查 386 个 Simulation 文件、
   0 个违规；根 Release 为 0 warnings、0 errors。
6. **接受边界。** 接受 Main 的三种确定性减伤公式、Dome 的显式整数生命映射和上述既有
   non-PvP 提交路径。仍不接受 `DamageVar`、随机数流顺序、PvP 网络命令、击退、invincibility
   全语义、buff/prefix/stance 的完整 defense 组成，或完整 Terraria combat pipeline。Task 9
仍为 `进行中`。

## 36. Task 9 / DamageVar pure contract 的已接受执行记录

Version4 `Main.cs:14639-14675` 与完整 archive `Main.cs:67025-67061` 定义了 `DamageVar`：默认
先消耗一次 `rand.Next(-15, 16)`；正 luck 先消耗一次 chance，命中后再消耗一次 variation 并
取较大值；负 luck 对称地取较小值；最后使用 `Math.Round` 转为整数。`DebugOptions.NoDamageVar`
则直接截断并消耗零个随机值。已知 Projectile 调用点为 Version4 `Projectile.cs:11877` 和
`13238`，分别传入 owner luck 或默认 luck。

**本批可证伪命题：** 纯契约必须保留旧随机消费顺序、正负 luck 的极值选择、禁用分支的零
消耗和最终 `Math.Round`。随机源只能由调用者通过 `IDamageVariationRandom` 提供，不能在系统
内部创建全局随机数。

1. **锁定 RED。** NPC verifier 先引用缺失的 `IDamageVariationRandom`；当前实现并未假设
   `_worldSeed` 是 combat random authority，也没有为 RED 增加 projectile 流程。
2. **完成纯系统。** `Combat/DamageVariationSystem.cs` 实现上述无副作用算法；
   `Combat/IDamageVariationRandom.cs` 仅定义 `NextDamageStep` 与 `NextChance` 的最小接口。
   verifier 使用脚本化随机适配器断言每种分支的确切消费计数。
3. **明确未接入边界。** 当前 Dome 的 `_worldSeed` 固定为 `1`，现有随机实现分别服务于 loot
   或 world generation；没有持久化的 combat stream state、旧 `Main.rand` 调用序列或 player
   luck 组件。因此不把 `DamageVariationSystem` 接到 projectile damage、player damage 或
   protocol path，不将缺失值设为 `0`，也不改变当前 projectile raw damage。
4. **运行全部 gate。** 新鲜证据目录为
   `Build/diagnostics/main-migration/task-9-damage-var/20260819-123700/`。修正 verifier 项目
   路径后，NPC、Combat、Persistence、WorldRules loopback、Protocol compatibility、full
   bootstrap、MainBoundary 和串行 root Release 全部 exit `0`；MainBoundary 388 files/0
   violations，root Release 0 warnings/0 errors。第一次脚本路径错误保留在同目录输出，不作为
   源码失败。
5. **接受边界。** 接受 `DamageVar` 的纯随机消费和数值契约；Task 9 仍为 `进行中`。Deferred/
   Unknown：combat random stream owner/state persistence、旧 `Main.rand` 顺序 parity、player
   luck authority、Projectile DamageVar 接入、NoDamageVar 服务器配置来源、完整命中/暴击链。

## 37. Task 9 / Moon phase and gameplay day classification 的已接受执行记录

Version4 `Main.GetMoonPhase` 只是把 `Main.moonPhase` 转成 `Terraria.Enums.MoonPhase`；其 enum
顺序由 `Terraria.Enums/MoonPhase.cs` 定义为 Full 到 ThreeQuartersAtRight 的 `0..7`。`UpdateTime`
在 dawn 时递增并以 `8` 回绕。`Main.IsItDay` 返回 `false` 当且仅当 remixWorld 为 true，否则
返回 dayTime。后者有大量 NPC/Player 消费者，不能简化为当前时钟的 `IsDayTime`。

**本批可证伪命题：** 输入 `0`、`4`、`7` 分别映射 Full、Empty、ThreeQuartersAtRight；`8`
必须被拒绝。non-remix 的 true/false clock 值原样分类为 gameplay day/night；remix 必定分类为
night。

1. **锁定 RED。** NPC verifier 先引用缺失的 `WorldMoonPhase` 与
   `WorldTimeClassificationSystem`，并覆盖枚举顺序、无效值和 remix 分支。
2. **完成纯分类。** `World/WorldMoonPhase.cs` 保留旧枚举整数顺序；
   `World/Systems/WorldTimeClassificationSystem.cs` 只从显式 `moonPhase`、`isDayTime` 和
   `isRemixWorld` 参数返回分类结果，不持有状态。
3. **明确未接入边界。** 当前 `WorldClockSnapshot` 没有 moonPhase，`WorldMetadata.SeedVariant`
   是未解释字符串，且当前 Simulation 无 source-backed remix flag producer。因此没有扩展 clock
   或 persistence，没有按字符串猜测 remix，没有替换现有 `DomeSimulation.IsDayTime`，也没有改写
   NPC event/AI 判断。
4. **运行全部 gate。** 新鲜证据目录为
   `Build/diagnostics/main-migration/task-9-moon-phase/20260819-131056/`。NPC、WorldRules、
   Persistence、WorldRules loopback、Protocol compatibility、full bootstrap、MainBoundary 与
   serial root Release 全部 exit `0`；MainBoundary 390 files/0 violations，root Release 0
   warnings/0 errors。
5. **接受边界。** 接受无状态的 legacy 月相与 gameplay-day 分类。Deferred/Unknown：月相状态、
   dawn phase transition、WorldFile/WorldData projection、remix flag 的 metadata/import producer、
   以及任何 NPC/Player 行为接入。Task 9 仍为 `进行中`。

## 38. 当前执行卡：Task 8 / Tile Solidity authority

### 38.1 本批目标和已知事实

本批不是把 Version4 的 `Main.tileSolid`、`Main.tileSolidTop` 或 `Collision` 整表复制到新项目。
唯一目标是消除当前 `TileCollisionSystem` 将任何 active tile 都视为实体阻挡物的错误 authority。

已冻结的当前源树事实如下：

| 事实 | 当前位置 | 对本批的含义 |
|---|---|---|
| Version4 的 753 个 tile definition 已在 Dome 中建模 | `WorldModel/Definitions/TileDefinitionRegistry.cs` | 复用唯一 registry；禁止再建平行表 |
| `BlocksLiquid` 来自 legacy `tileSolid`，`IsPlatform` 是独立字段 | `TileDefinition.cs` | 不得用 `BlocksLiquid` 直接声称完整 collision 语义 |
| Liquid 已消费 registry | `Liquid/Systems/LiquidPropagationSystem.cs` | 修改 collision 时不得破坏已有 liquid 契约 |
| physics collision 仅以 `WorldTile.IsActive` 判断 | `Physics/Systems/TileCollisionSystem.cs` | active Type 4 等非 solid tile 会被错误当成墙，这是本批第一个缺口 |
| world generation 已有更丰富的 tile query | `World/TileStateQuery.cs`、`World/TileNeighborhoodQuery.cs` | 可评估复用，但不能未审计就把 generation 语义挪作 runtime collision |

本批 source oracle 至少包括：

- `D:\TRbackup\Version4物理删除了某些文件\Terraria\Collision.cs` 中 player/tile
  collision、platform 和 slope 判断；
- `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs` 中 `tileSolid`、
  `tileSolidTop` 和其初始化/消费者；
- `D:\TRbackup\Version4物理删除了某些文件\Terraria\WorldGen.cs` 中已有 tile-state
  语义的参考。

### 38.2 明确的第一切片

第一切片只迁移 axis-aligned collider 的基础 block collision，且只在能够由 source 和现有
definition 共同证明的范围内工作：

1. active、非 platform、`BlocksLiquid=true` 的 tile 阻挡水平和垂直移动；
2. active、`BlocksLiquid=false` 且非 platform 的 tile 不阻挡基础移动；
3. active platform 不充当水平侧墙；它是否在向下穿越时阻挡，必须先由 `Collision.cs` 的
   方向和落点条件建立独立 RED，不能由本条默认推断；
4. map 外的 x/y 继续作为阻挡边界；
5. unknown tile ID 必须经过 registry 的显式失败/拒绝路径，不能静默当 solid 或 air。

本切片明确不声称实现：half block、slope、actuator、door/platform 特例、fall-through 输入、
grapple、液体、NPC 专用碰撞尺寸、tile paint/frame，或完整 Terraria `Collision` parity。它们在
基础 block collision 获得独立证据后，按一个行为族一张执行卡继续。

### 38.3 必须先建立的 RED 命题

在修改 production 前，为 `TileCollisionSystem` 所在的 physics verifier 新增互相独立的场景。
每个 rejected/allowed 场景都要断言最终位置、速度和 `IsGrounded`，而不是只断言“不抛异常”。

| 场景 | 预期 |
|---|---|
| active Type 1，向右或向下穿越 | 在 tile 边界停止，对应轴速度清零 |
| active Type 4，向右或向下穿越 | 不停止，速度保留 |
| active Type 19 platform，水平穿越 | 不停止；platform 不能自动成为侧墙 |
| 空 tile | 不停止 |
| world 负坐标/宽高之外 | 仍停止，避免 map escape |
| registry 不含的 active tile type | 明确抛出/拒绝，world 和 physics state 不部分提交 |

若 `Collision.cs` 证明 Type 19 的向下方向/落点条件与当前 movement coordinate convention
不一致，先记录该 RED 和坐标换算证据；禁止为取得 GREEN 把 platform 标成 ordinary solid。

### 38.4 实施边界

实现优先选择注入现有 `TileDefinitionRegistry` 的最小路径，并维持 `DomeSimulation` 的唯一
composition root。若为了 source-compatible 构造器需要保留当前无参构造器，它只能委托给
Version4 base registry，不能继续保留 `IsActive => solid` 的第二语义。

`TileCollisionSystem` 的 tile lookup 应区分以下状态：

```text
outside-world       -> blocking boundary
inactive tile       -> non-blocking
known ordinary solid -> base collider blocking rule
known non-solid     -> non-blocking
known platform      -> dedicated directional rule; never horizontal wall by default
unknown active type -> reject before position/velocity commit
```

不得修改 `LiquidPropagationSystem`、registry 的 753-ID 数据、Protocol packet、persistence
format 或 client bootstrap，除非 focused RED 证明它们是本行为的实际消费者。不得以
`BlocksLiquid` 的名称为由重命名字段或扩大其含义。

### 38.5 分层验收和停止条件

使用新的证据目录：

```powershell
$runId = Get-Date -Format "yyyyMMdd-HHmmss"
$evidence = "Build\diagnostics\main-migration\task-8-tile-solidity\$runId"
New-Item -ItemType Directory -Force -Path $evidence | Out-Null
```

按顺序保存 `physics-red.txt`、`physics-final.txt`、已有 liquid verifier、MainBoundary 和
serial root Release build 的完整输出。若本改动触及 `DomeSimulation` composition，则额外运行
Persistence、WorldRules loopback、Protocol compatibility 和 FullClientBootstrap；未触及时不要
把无关 gate 当作行为正确性的替代品。

所有 `dotnet` 命令从仓库根执行，并携带：

```text
-c Release -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false
```

接受条件是：source manifest 可重现、RED 有保存、六个基础场景为 GREEN、liquid 回归为
GREEN、MainBoundary 为 GREEN、root Release 为 GREEN，且源文件在 final gates 后未再改变。若
platform 的真实方向语义无法从 `Collision.cs` 与现有坐标约定对齐，则接受 ordinary/non-solid
基础切片，并将 platform 标为 `Blocked by source-coordinate contract`；不能用猜测完成 Task 8。

完成后，向 `progress.md` 追加 source 行号/哈希、命题、改动文件、每个 gate 的 exit code 和
剩余 platform/slope backlog。Task 8 在所有实际 consumer 都有此类 source-backed 契约前仍保持
`进行中`。

## 39. Task 8 / basic tile solidity collision 的已接受执行记录

Version4 `Collision.TileCollision`（`Collision.cs:1633-1795`）先从 `Main.tileSolid` 建立普通
blocking，再在 `tileSolidTop`、`frameY`、fall-through 和方向分支中处理 top surface。特别是水平
和反向竖直分支带有 `!Main.tileSolidTop[type]` 条件。因此不能把 active tile、platform 或 liquid
definition 当成同一个 runtime collision 概念。

**本批可证伪命题：** active Type 1 阻挡当前 axis-aligned collider；active Type 4 不阻挡；active
Type 19 不阻挡水平移动；world 外保持阻挡；active Type 753 在任何 position、velocity 或 grounded
mutation前被拒绝。尚未实现的 slope/half-brick 不会因本次 type-classification 变更而变成 passable。

1. **锁定 RED。** `PlayerPhysics.Verification` 先证明 Type 4 仍被旧 `IsActive` 逻辑错误阻挡；
   第二个 RED 证明 Type 753 被静默视作 air；第三个 RED 发现直接复用 generation
   `TileStateQuery.IsSolid` 会使 slope/half-brick passable。这三份输出都保存在
   `task-8-tile-solidity/20260819-132253/`。
2. **完成最小 authority 路径。** `TileCollisionSystem` 接收 `TileDefinitionRegistry`，无参构造器
   只委托给既有 Version4 base registry。active/inactive、outside-world、known ordinary solid、known
   non-solid/platform 与 unknown active type 分别由该系统处理；slope/half-brick 维持现有 whole-tile
   fallback，未将 generation query 的几何语义偷偷引入 physics。
3. **运行受影响回归。** `physics-final.txt`、`npc-final.txt` 和 `liquid-final.txt` 为 exit `0`；
   这既验证 direct player/NPC collision，也确认已有 753-ID liquid definition 契约未回归。
4. **运行最终边界。** `main-boundary-final.txt` 是 400 Simulation files、0 violations；
   `root-release-build-final.txt` 是 serial Release exit `0`、0 warnings、0 errors。没有改动
   persistence、Protocol、server composition 或 bootstrap，因此不把不相关 loopback 当作本批证据。
5. **接受边界。** 已接受 definition-backed ordinary whole-tile collision 与 unknown active tile
   rejection。Task 8 仍为 `进行中`；platform landing/fall-through、frame predicates、slope、half
   brick、actuator、door 和完整 `Collision` parity 仍需各自的 source/oracle + RED 批次。

## 40. Task 8 / platform top landing 的已接受执行记录

Version4 `Collision.TileCollision` 的默认 `fallThrough=false` 路径证明：`tileSolidTop` 且
`frameY == 0` 的平台在从上方到达时承接实体；水平、从下方上行的分支均排除 top surface。
当前 Simulation 没有可追溯的 fall-through 输入/命令 producer，因此本批只实现默认落地，不能
把缺失的输入值猜成 `false` 并声称完整输入迁移。

**本批可证伪命题：** Type 19 从上方负 Y 下落会停在 `tileY + 1`、清零 Y 速度并标记 grounded；
同一平台从侧面和正 Y 上行均不阻挡；ordinary solid、non-solid、boundary、unknown type 和
未迁移 slope fallback 的约束不能回归。

1. `physics-red.txt` 先证明默认落下会穿透平台；修复过程中一次错误分支替换也由同一断言捕获，
   没有放宽 verifier。
2. `IsLandingTile` 只在负 Y 下落/standing 路径使用；它复用 753-ID registry，active known
   platform 仅接受 `FrameY == 0`，不让 platform 进入水平或 upward collision。
3. `physics-final.txt`、`npc-final.txt`、`liquid-final.txt`、`main-boundary-final.txt` 均为
   exit `0`；MainBoundary 为 400 files/0 violations。串行 root Release 为 exit `0`、0 warnings、
   0 errors，证据目录为 `Build/diagnostics/main-migration/task-8-platform-top/20260819-133545/`。
4. 接受边界为默认 platform-top landing 与 side/upward pass-through。fall-through command/input、
   `fall2`、反重力方向、复杂 frame、slope/half-brick、actuator、door 和完整 collision parity
   继续保持 backlog，Task 8 仍为 `进行中`。

## 41. Task 8 / fall-through pure contract 的已接受执行记录

Version4 `Collision.cs:1727` 的准确条件是：只有 `tileSolidTop && fallThrough` 且
`Velocity.Y <= 1f || fall2` 时跳过平台顶面。`Player.cs` 的 fallThrough 不是普通常量：它来自
`controlDown`，并被 mount、grapple、pulley 与反重力路径强制修改。当前 Dome 的
`PlayerInput`、`ClientInputFrame` 和协议控制意图没有这些字段或 producer。

**本批可证伪命题：** 纯规则必须区分低速 fall-through、超过阈值、fall2、非 platform 和非 top
frame；缺失 authority 时不能把结果接到默认 tick。

1. 先用缺少 `PlatformCollisionRuleSystem` 的 focused verifier 建立 RED；实现后又由 verifier
   捕获错误的 fall2 逻辑方向，修正后才获得 GREEN。
2. `PlatformCollisionRuleSystem.ShouldCollideFromAbove` 只重现旧布尔谓词，不读世界、实体、
   `Main` 或协议，也不产生默认输入。它为未来的 Player input authority 提供可测试的边界。
3. `physics-final.txt`、`npc-final.txt`、`liquid-final.txt`、`main-boundary-final.txt` 均为
   exit `0`；MainBoundary 为 404 files/0 violations。串行 root Release 为 exit `0`、0 warnings、
   0 errors，证据目录为 `Build/diagnostics/main-migration/task-8-fall-through-contract/20260819-134513/`。
4. 接受边界是纯 fall-through predicate；`Down/controlDown` wire compatibility、mount/grapple/
   pulley、per-player state、tick integration、fall2 lifecycle 和反重力仍为后续 authority 批次。
   Task 8 继续保持 `进行中`。

## 42. Task 8 / V1456 controlDown authority 的已接受执行记录

Version4 `MessageBuffer.cs:665-671` 已证明 PlayerControls 固定 control flags 的 bit 1 是
`controlDown`；`NetMessage.cs:445-449` 对称写入同一 bit。因此这不是新增协议字段：Dome 之前
读取了 byte 却遗漏该 bit 的 domain projection。

**本批可证伪命题：** 固定 14-byte V1456 PlayerControls frame 保持长度和顺序，同时 bit 1 在
decode/encode 后映射 `Down`；Server 将它传至 `SimulationInputBatch`，玩家在同 tick 通过平台
顶面。未发送 Down 时保持已接受的默认落地。

1. `protocol-red.txt` 先因 Intent 无 `Down` 失败。实现只将 optional `Down` 附加到 public input
   value types，固定 wire payload 不增字段；compatibility verifier 既检查 existing client-style
   frame 的 bit 1 decode，也检查 encoder 输出 bit 1 与 14-byte 长度。
2. `PlayerInputApplySystem` 写入 `PlayerInputComponent.Down`；`DomeSimulation` 只在玩家碰撞
   调用提供该值，NPC 继续采用默认 false。physics verifier 证明 `Down=true` 在同 tick 到达
   platform top skip，而无输入的既有 landing 不回归。
3. 当前 `ClientInputFrame` 在生产树没有 consumer，不构成第二条输入 authority，故不为表面一致性
   写入无效字段。mount/grapple/pulley/gravDir 与 fall2 仍没有 Simulation producer，不能虚构。
4. 新鲜 evidence 为 `Build/diagnostics/main-migration/task-8-down-input/20260819-135600/`：
   Protocol、Physics、PlayerLifecycle TCP、WorldRules TCP、FullClientBootstrap、NPC、Liquid 和
   MainBoundary 均 exit `0`；MainBoundary 413 files/0 violations；serial root Release 为 exit `0`、
   0 warnings、0 errors。
5. 接受边界为 source-compatible controlDown wire authority 和默认 player platform fall-through。
   Task 8 仍为 `进行中`，复杂 forced fall-through、反重力、slope、half-brick、actuator、door 与
   完整 `Collision` parity 继续按独立 source/RED 批次推进。

## 44. Task 8 / player top-slope contact 的已接受执行记录

旧 Player 的调用顺序不是单一 `Collision.TileCollision`：`Player.cs:14294-14308` 先执行普通
tile collision，随后 `Player.cs:14324-14355` 调用 `SlopingCollision`；该方法消费
`Collision.SlopeCollision`（`Collision.cs:1228-1469`）的斜面接触结果。当前 Dome 原先只有
axis-separated `TileCollisionSystem`，因此本批先建立了独立的玩家 top-slope seam，而不是把
三角形条件塞进普通 AABB 方法。来源片段和 SHA-256 在
`Build/diagnostics/main-migration/task-8-player-top-slope/20260819-143832/source-manifest.txt`。

**本批可证伪命题：** ordinary known solid `Slope=1` 与 `Slope=2` 的玩家 collider 在普通 tile
移动后，沿各自 source-backed diagonal top surface 校正位置、清除 downward velocity 并设为
grounded；真实 `DomeSimulation.Tick` 玩家路径必须执行该接触模块。active unknown slope 不能
静默作为 air；Type 1/4、platform、half-brick、fall-through 和 nonzero-slope fallback 的旧
断言不能回归。

1. **锁定 RED。** PlayerPhysics verifier 新增 slope 1/2 场景；旧 whole-tile fallback 在 Slope 1
   顶面接触断言失败，输出保存在
   `Build/diagnostics/main-migration/task-8-player-top-slope/20260819-143146/physics-red.txt`。
2. **完成深模块。** 新增 `Physics/Systems/TopSlopeContactSystem.cs`，接口只接受 world、
   transform、velocity、physics 和 collider；它只解析 active known ordinary slope 1/2，并对
   unknown active tile fail closed。`TileCollisionSystem.MoveAndResolve` 增加可选的内部调用
   语义：玩家路径延迟 slope 1/2 的 whole-tile AABB，NPC 路径保持既有 fallback。随后
   `DomeSimulation.ResolvePlayerTileCollision` 调用 `TopSlopeContactSystem`，形成与旧 Player
   `TileCollision -> SlopingCollision` 对应的显式 phase。
3. **保持边界。** 本批只覆盖正重力玩家的 top slopes 1/2。没有声称 slope 3/4 bottom/ceiling、
   `StepUp`/`StepDown`、hoik、stairFall、平台 slope、逆重力、mount/grapple/pulley、NPC slope
   或完整 `Collision.SlopeCollision` parity；未知 tile 拒绝不改变已有 prevalidation。
4. **最终证据。** 新鲜目录为
   `Build/diagnostics/main-migration/task-8-player-top-slope/20260819-143832/`：
   `physics-final.txt`、`player-lifecycle-loopback-final.txt`、`npc-final.txt`、
   `liquid-final.txt`、`main-boundary-final.txt` 和 `root-release-build-final.txt` 均 exit `0`；
   MainBoundary 扫描 432 个 Simulation source files、0 violations；serial root Release 为
   0 warnings、0 errors；`diff-check-final.txt` exit `0`。
5. **接受边界。** 接受玩家正重力 top-slope 1/2 surface contact 与实际 Dome tick 编排。Task 8
   仍为 `进行中`；bottom slopes 3/4、NPC 专属 slope、special platform/door/actuator collision、
   forced fall-through/fall2 与完整 Terraria collision parity 继续各自建立 source + RED。

## 45. Task 8 / bottom-slope whole-tile defer pure contract 的已接受执行记录

Version4 `Collision.TileCollision` 的 `Collision.cs:1697-1706` 对 slope `3/4` 先依据
`previousPosition.Y + abs(Velocity.X)` 和 collider 的左右边界设置 `flag3`，再跳过普通 AABB
分支；这不是一个可以脱离 `SlopeCollision`、`StepUp` 和进入方向直接猜出的完整底面几何。

**本批可证伪命题：** slope `3` 在左边界满足 source 条件时、slope `4` 在右边界满足 source
条件时返回 defer；速度/位置条件不满足或 slope 非 `3/4` 时返回 false。该契约不接入当前
runtime movement，避免将未迁移的 bottom-surface/ceiling phase 伪装成完整碰撞。

1. **锁定 RED。** PlayerPhysics verifier 先引用缺少的 `BottomSlopeCollisionRuleSystem`；
   `Build/diagnostics/main-migration/task-8-bottom-slope-contract/20260819-144152/physics-red.txt`
   以编译错误 exit `1`，没有伪造行为失败。
2. **完成纯契约。** 新增
   `Physics/Systems/BottomSlopeCollisionRuleSystem.cs`，只实现 `Slope=3/4` 的 source predicate，
   不读世界、实体、Main、Protocol 或 Server，也不改变 `TileCollisionSystem` 当前 NPC/player
   bottom-slope fallback。
3. **最终证据。** 新鲜目录为
   `Build/diagnostics/main-migration/task-8-bottom-slope-contract/20260819-144319/`：
   PlayerPhysics、NPC、Liquid、MainBoundary 和 serial root Release 全部 exit `0`；
   MainBoundary 扫描 434 个 Simulation source files、0 violations；root Release 为 0 warnings、
   0 errors；`diff-check-final.txt` exit `0`。
4. **接受边界。** 仅接受 bottom-slope whole-tile defer 的纯 source contract。bottom-slope
   runtime geometry、ceiling correction、StepUp/StepDown、hoik、reverse gravity、NPC slope
   behavior 和完整 `Collision.SlopeCollision` 仍为 deferred，Task 8 保持 `进行中`。

## 46. Task 8 / active-inactive collision candidate pure contract 的已接受执行记录

Version4 `Collision.TileCollision` 在几何和 definition 分类之前以
`tile == null || !tile.active() || tile.inActive()` 排除候选（`Collision.cs:1661-1665`）。
这证明 `inActive` 是 collision participation 的直接状态；`actuator` bit 本身并不等于在此方法
中无条件跳过，不能在缺少 producer/state-transition authority 时被猜成 air。

**本批可证伪命题：** active 且非 inactive 的 tile 才参与碰撞候选；not active 或 inactive tile
均必须在几何/definition 解析前排除。这个纯 contract 不改变已有 `TileCollisionSystem` 的
runtime 行为，因为该系统已经使用 `!IsActive || IsInactive`；它固定了该规则的来源与后续
actuator/door 迁移边界。

1. **锁定 RED。** focused PlayerPhysics verifier 对缺少
   `TileCollisionActivationRuleSystem` 产生编译 RED，保存在
   `Build/diagnostics/main-migration/task-8-tile-activation-contract/20260819-144857/physics-red.txt`。
2. **完成纯契约。** `Physics/Systems/TileCollisionActivationRuleSystem.cs` 只暴露
   `ShouldParticipate(isActive, isInactive)`，不读取 Main、world、entity、Protocol 或 Server。
   Actuator producer、closed door table 和 `ignoreDoors` caller authority 都未被虚构。
3. **最终证据。** 同目录中的 `physics-final.txt`、`main-boundary-final.txt`、
   `root-release-build-final.txt` 和 `diff-check-final.txt` 均 exit `0`；MainBoundary 扫描
   436 个 Simulation source files、0 violations；root Release 为 0 warnings、0 errors。
4. **接受边界。** 接受 active/inactive collision candidate 的 source contract。完整 actuator
   toggle lifecycle、closed-door definitions and ignoreDoors authority、special platform flags、
   door geometry、bottom-slope runtime geometry、NPC-specific collision 和完整 Terraria collision
   parity 仍 deferred；Task 8 保持 `进行中`。

## 47. Task 8 / actuator collision authority audit (deferred, no implementation)

**Outcome:** the current Dome actuator path is not a valid implementation of the Version4
actuator state transition, so it must not be connected to the accepted `IsInactive` collision gate
or used as actuator-collision acceptance evidence.

### Authority trace

The Version4 source oracle is `Terraria/Wiring.cs:395-427`. `Actuate` first requires the tile's
`actuator()` bit, then toggles the existing tile between `inActive()` and its reactivated state:

```text
tile.actuator && tile.inActive -> ReActive(i, j)
tile.actuator && !tile.inActive -> DeActive(i, j)
```

`Collision.cs:1661-1665` independently proves the consumer rule: inactive tiles do not become
collision candidates. The physical-deletion Version4 checkout leaves the bodies of
`DeActive`/`ReActive` as stubs, so it cannot itself prove the complete mutation payload
(frame recalculation, networking, multi-tile behaviour, liquid effects, and section update order).

The actual Dome trace is:

```text
wire / pressure plate / logic gate
  -> MechanismActivationSystem (toggles MechanismComponent.IsActive)
  -> ActuatorCommandSystem.CreateCommands
  -> TileChangeCommand(Place when enabled, Kill when disabled)
  -> WorldGrid.CommitTileChanges
  -> TileChangeCommitSystem
  -> new active tile / default empty tile
```

This is observably a create/delete projection, not `WorldTile.IsInactive` mutation:

1. `ActuatorCommandSystem` never reads the target `WorldTile`, does not require
   `WorldTile.IsActuated`, and emits `Place` or `Kill`.
2. `TileChangeCommitSystem` turns `Place` into `new WorldTile(IsActive: true, TileType)` and
   `Kill` into `default`; both discard the pre-existing frame, slope, half-brick, actuator,
   inactive, paint, wall and liquid-preservation state unless a different generic command happens
   to request preservation.
3. Existing Wiring verifier coverage only proves that a logic-gate reaches the generic actuator
   command boundary and results in `IsActive == true`; it does not prove inactive-state toggling,
   collision pass-through, restoration, idempotence, or Version4 multi-tile semantics.

### Recovered source and next executable batch

After this audit, the local `D:\TRbackup\无任何删减通过编译` oracle was recovered and matched its
`Main.cs` version string to Version4: `v1.4.5.6`. Its `Wiring.cs:3302-3345` supplies the missing
method bodies (SHA-256 `704BFD8029E29FCF64BFDF4150A14D599D7489D3F58462F485B47A557BDF3FDE`).
It proves that reactivation preserves the tile and sets `inActive=false`; deactivation sets
`inActive=true` only after these additional predicates:

1. target is active;
2. type `226` below world surface is rejected unless Plantera is defeated;
3. target is solid and not `TileID.Sets.NotReallySolid`;
4. types `314, 379, 386, 387, 388, 389, 476` are excluded from that solid path;
5. target-above is inactive, or it does not have `PreventsActuationUnder` and `WorldGen.CanKillTile`
   accepts the target.

No production code or verifier was changed in this audit. The prior active/inactive collision
contract remains accepted, but actuator runtime collision remains **deferred**. The next batch may
now freeze this local source excerpt and define an explicit, tile-preserving transition contract.
It must model every source predicate it claims. Until `NotReallySolid`,
`PreventsActuationUnder`, `CanKillTile`, type-226 world/progression authority, and the special-tile
set have an owner, they are rejection/deferred conditions, not defaults to infer from the misleading
existing `BlocksLiquid` field.

The mandatory RED/GREEN acceptance matrix for that future batch is:

| Case | Required assertion |
| --- | --- |
| Valid active actuator tile | Toggle to `IsInactive=true` while retaining authoritative tile fields; next collision phase skips it. |
| Second valid activation | Toggle back to `IsInactive=false`; original collision participation returns. |
| Tile without actuator bit | Reject without tile mutation or section revision change. |
| Unsupported/unknown footprint | Reject before partial mutation; do not silently Kill or Place a replacement tile. |
| Replay/sequence conflict | Deterministic result with no duplicate revision mutation. |
| Persistence/replication | Persist and project the two existing flags only through already-authoritative world paths. |

Until that command contract exists, `ActuatorCommandSystem` is an unrelated generic
wiring projection and cannot be cited as Terraria actuator parity. Task 8 remains `进行中`.

## 48. Task 8 / Type 1 actuator inactive-state slice accepted

**Source oracle:** the no-deletion companion checkout has the same `Main.cs` version,
`v1.4.5.6`, as the Version4 authority source. Its `Terraria/Wiring.cs:3302-3345` is frozen with
SHA-256 `704BFD8029E29FCF64BFDF4150A14D599D7489D3F58462F485B47A557BDF3FDE` in
`Build/diagnostics/main-migration/task-8-actuator-state/20260819-153058/source-manifest.txt`.
The source shows two relevant branches: `ReActive` preserves the tile and clears `inActive`; the
`DeActive` branch sets it only when its eligibility predicate holds.

**Accepted proposition:** a pre-existing, active Type 1 tile with `IsActuated=true` and no active
tile immediately above can be toggled through the authoritative wiring pipeline. Deactivation must
preserve every `WorldTile` field while setting only `IsInactive=true`; reactivation clears only that
field. Once committed, the existing runtime collision consumer excludes the inactive tile; after
reactivation it again collides. This maps the source's inactive-above short-circuit and Type 1's
known solid/non-special conditions without evaluating the unported active-above `CanKillTile` path.

1. **RED.** Before implementation, `Wiring.Verification` named the required `world` argument,
   `TileChangeKind.SetInactive`, and command inactive payload. Its focused run failed with exit `1`
   because all three contract members were absent; the clean failure is
   `Build/diagnostics/main-migration/task-8-actuator-state/20260819-153058/wiring-red.txt`.
2. **State-preserving command boundary.** `TileChangeKind.SetInactive` and the optional
   `TileChangeCommand.IsInactive` payload are internal world commands. `TileChangeCommitSystem`
   uses a `with { IsInactive = ... }` update, rather than the generic Place/Kill projection. Thus
   it retains type, frames, wall, actuator bit, paint, shape, liquid and all other existing fields.
3. **Wiring authority.** `ActuatorCommandSystem` now receives `WorldGrid`, requires an active
   `IsActuated` target, derives the next state from the target's current `IsInactive` state, and
   emits no command for any unsupported coordinate. A component footprint is prevalidated before
   emission, so a mixed supported/unsupported footprint produces no partial tile mutation. The
   `DomeSimulation` wiring phase remains the sole producer and commits the command at the existing
   deterministic world commit boundary.
4. **Focused and cross-domain GREEN.** `wiring-final.txt` proves deactivate/restore, retained tile
   fields, non-actuator rejection without section revision mutation, active-above rejection, and
   atomic footprint rejection. `physics-final.txt` proves committed inactive tiles are passable and
   restored tiles block again. `wiring-loopback-final.txt` proves real
   pressure-plate/logic-gate/`DomeSimulation.Tick` composition reaches the tile-preserving commit.
   All are exit `0` in the same evidence directory.
5. **Regression and boundary evidence.** Persistence and NPC verifiers exit `0`; MainBoundary exits
   `0` after scanning 445 Simulation source files with no forbidden dependencies. The serial root
   Release build exits `0` with 0 warnings and 0 errors. `git diff --check` exits `0` (the printed
   LF/CRLF notices are pre-existing worktree conversion notices, not check failures).
6. **Explicit non-claims.** This is not full actuator parity. It deliberately rejects Type 226,
   every non-Type-1 solid/not-really-solid classification, special types
   `314/379/386/387/388/389/476`, any active-above target requiring
   `PreventsActuationUnder` plus `CanKillTile`, multi-tile definition-specific behavior, framing
   consequences beyond existing retained fields, and legacy TileSquare timing. Those predicates
   require their own source-derived definition/world-rule owners before they can become accepted
behavior. Task 8 remains `进行中`.

## 49. Task 8 / actuator deactivation eligibility pure rule accepted

`ActuatorDeactivationRuleSystem.ShouldDeactivate` now preserves the recovered Version4
`Wiring.DeActive` boolean predicate as a pure contract: active and actuator bits, the Type 226
world/progression guard, solid/not-really-solid classification, seven special tile IDs, and the
active-above `PreventsActuationUnder`/`CanKillTile` branch.

The Type 1 runtime producer uses this rule for its supported inactive-above path. When the tile
above is active, `CanKillTile` is not yet owned by Dome and is passed as false, so the branch rejects
instead of guessing permission. The focused verifier caught the earlier placeholder
`canKillTile=true`; the corrected producer now fails closed without mutation.

Fresh evidence is under
`Build/diagnostics/main-migration/task-8-actuator-rule/` (current run directory): Wiring,
PlayerPhysics, WiringLiquidChest loopback, Persistence and NPC verifiers pass; MainBoundary scans
451 Simulation files with 0 violations; serial root Release passes with 0 warnings and 0 errors;
diff check passes. Full Type 226/world-surface ownership, definition registries, `CanKillTile`,
special tile behavior, multi-tile framing and TileSquare timing remain deferred.

## 50. Task 8 / CanKillTile protection contract accepted

The no-deletion v1.4.5.6 oracle's `WorldGen.cs:63226-63313` is frozen in
`Build/diagnostics/main-migration/task-8-can-kill-rule/20260819-155734/source-manifest.txt`
with SHA-256 `B9F7834CE1BC68C1DD9C656574A2272DB6F79E1407D934E1ADA33EDC3C930F82`.
It proves `WorldGen.CanKillTile` is not simply an active-tile predicate: it rejects out-of-world,
missing/inactive target, wall 350, protected active-above tree/special/frame relations,
boulder-chest protection, locked doors, multi-tile blockers and chest blockers.

`ActuatorCanKillTileRuleSystem.CanKill` is a pure contract with one explicit boolean input for each
of those source-proven protection classes. The focused verifier proves the eligible base case, all
base rejections, and rejection of every protection class individually. This prevents future producer
work from flattening `CanKillTile` to `true` or a generic `IsActive` check.

This rule is intentionally **not** connected to the active-above actuator producer yet. Current Dome
does not have authoritative adapters for every tree/frame/boulder/multi-tile/chest relation, so its
existing producer remains fail-closed. Evidence: Wiring, PlayerPhysics, Persistence and NPC verifiers
all exit `0`; MainBoundary scans 455 Simulation files with 0 violations; serial root Release exits
`0` with 0 warnings/errors; diff check exits `0`.

Next work must add one source-backed owner at a time, beginning with the already available chest and
door state, then prove its projection into this pure contract before allowing the active-above
actuator branch. Task 8 remains `进行中`.

## 51. Task 8 / CanKillTile world-object protection input contract accepted

The current batch adds `ActuatorCanKillTileRuleSystem.CanKill`, a pure rule matching the recovered
`WorldGen.CanKillTile` base guards and protection categories. The source excerpt is frozen from the
same v1.4.5.6 no-deletion oracle at
`Build/diagnostics/main-migration/task-8-can-kill-rule/20260819-155734/source-manifest.txt`.

The rule deliberately receives protection facts as inputs rather than reaching into Chest, Door,
Tree, Main or WorldGen state. This keeps the authority boundary explicit: an eligible target is
accepted only when every protection fact is false; missing/unowned object facts must be supplied as
true protection (or otherwise cause rejection), never guessed away.

The focused Wiring verifier covers the eligible base case, world/tile/active/wall-350 rejection,
and each of the seven protection categories individually. PlayerPhysics, Persistence and NPC
regressions pass; MainBoundary scans 455 Simulation files with 0 violations; serial root Release
passes with 0 warnings/errors; diff check passes.

This is a pure contract only. Active-above actuator runtime remains fail-closed until existing Dome
Chest/door state and the missing tree/frame/boulder/multi-tile relations are projected with their own
source-backed tests. Task 8 remains `进行中`.

## 52. Task 8 / Chest.CanDestroyChest inventory rule accepted

The no-deletion v1.4.5.6 `Chest.cs:648-665` source is frozen at
`Build/diagnostics/main-migration/task-8-chest-destruction/20260819-160641/source-manifest.txt`.
Its rule is exact and narrower than a generic lock/access rule: absent chest is destroyable; an
existing chest is destroyable only when every slot has no item with both positive type and positive
stack. Lock state and active opener are not `Chest.CanDestroyChest` conditions.

`ChestDestructionRuleSystem.CanDestroy` reads only the authoritative `ChestComponent` slot state.
The Wiring verifier proves null/absent and empty chest acceptance, populated chest rejection, and
the corresponding `ActuatorCanKillTileRuleSystem.isChestBlocked` rejection. It does not introduce a
coordinate lookup because current Dome needs an explicit legacy tile footprint/origin adapter before
chest state can authoritatively answer an arbitrary Tile ID 21/467/88 `CanKillTile` query.

PlayerPhysics, Persistence and NPC regressions pass; MainBoundary scans 458 Simulation files with
0 violations; serial root Release passes with 0 warnings/errors; diff check passes. This accepts the
Chest inventory predicate only. It does not enable active-above actuator runtime, chest footprint
mapping, locked-door semantics, boulder protection, tree/frame relations or multi-tile parity.
Task 8 remains `进行中`.

## 53. Task 8 / legacy chest origin query accepted

The recovered V1456 oracle establishes a coordinate-based chest protection path. `WorldGen.cs`
`CanKillTile` uses `x - (frameX / 18 % 2), y - frameY / 18` for Types 21/467 and
`x - (frameX / 18 % 3), y - frameY / 18` for Type 88. `MessageBuffer.cs:2047-2135` independently
uses the matching 2-wide and 3-wide frame offsets for destruction requests, while
`WorldFile.cs:3357-3359` records Type 88 top-left creation at frame multiples of 54 and 36.

`WorldObjects/Chest/Systems/LegacyChestOriginQuery` is a pure, read-only projection from an active
`WorldTile` and tile coordinates to that exact source-derived origin. It accepts only Types 21, 467
and 88 with non-negative 18-pixel-aligned frames, and rejects inactive, unsupported, negative or
misaligned frame inputs. It creates no chest, mutates no tile, accesses no index, and has no
Protocol, Server, Main or WorldGen dependency.

The focused Wiring verifier was a genuine compile RED before the query existed and now proves Type
21, Type 467, Type 88, plus all rejection branches. Evidence is under
`Build/diagnostics/main-migration/task-8-chest-footprint/20260819-162117/`; Wiring, Wiring/Liquid/
Chest loopback, Persistence, PlayerPhysics and NPC verifiers exit 0. MainBoundary scans 465
Simulation source files with 0 violations; serial root Release exits 0 with 0 warnings and 0 errors;
`git diff --check` exits 0.

This accepts only the legacy frame-to-origin contract. Dome's current `CreateChest(x, y)` index has
not yet been proven to be populated with imported legacy origins, so this batch deliberately does
not project a `ChestComponent` into `isChestBlocked`, change persistence/import semantics, or enable
the active-above actuator branch. All other `CanKillTile` protection categories remain fail-closed.
Task 8 remains `进行中`.

## 54. Task 8 / chest destruction protection projection accepted

The current WLD import chain proves that Dome chest coordinates preserve legacy chest origins:
`WldChestReader` reads a `LegacyChest(X,Y)`, `WldToCompatibilityProjection` preserves the values,
`CompatibilityToDomeProjection` writes them to `ChestPersistentState`, and
`DomeSimulation.RestoreChest` builds the exact `ChestIndexSystem` entry from those same fields.
This closes the ownership evidence that was intentionally absent from the prior origin-query slice.

`LegacyChestDestructionQuery.TryGetIsChestBlocked` combines the accepted frame-origin query with
the exact chest index and `ChestDestructionRuleSystem`. It returns a protection fact only for a
supported tile/frame. No chest at the exact origin is allowed; an empty origin chest is allowed; a
populated origin chest blocks. A populated adjacent chest cannot affect the result. A broken index
to-chest relation, unsupported tile or malformed frame returns no result rather than permission.

The Wiring verifier recorded a compile RED before the query existed and final coverage proves empty,
populated, adjacent and unsupported cases. Evidence:
`Build/diagnostics/main-migration/task-8-chest-protection-projection/20260819-162655/`. Wiring,
Wiring/Liquid/Chest loopback, Persistence, PlayerPhysics and NPC verifiers exit 0. MainBoundary
scans 466 Simulation source files with 0 violations; serial root Release exits 0 with 0 warnings and
0 errors; `git diff --check` exits 0.

This accepts the source-backed chest protection projection only. It is not yet invoked by the
active-above actuator producer because tree/special/frame, boulder, locked-door and multi-tile facts
remain unowned. No unsupported fact is converted to `false`, and no actuator behavior is widened.
Task 8 remains `进行中`.

## 55. Task 8 / locked-door tile rule accepted

The recovered v1.4.5.6 `WorldGen.IsLockedDoor(Tile)` source is exact and self-contained:
`WorldGen.cs:70324-70340` returns true only for `type == 10`, `frameY >= 594 && frameY <= 646`,
and `frameX < 54`. `WorldGen.CanKillTile` calls this predicate for Type 10 at
`WorldGen.cs:63297-63305`. It does not inspect `DoorSnapshot`, opener state, mechanism state or
player inventory.

`Wiring/Systems/LockedDoorRuleSystem.IsLocked` preserves those three source conditions as a pure
`WorldTile` rule. It intentionally does not add an active check because the recovered method itself
does not contain one; the enclosing `CanKillTile` active guard remains a separate contract.

The Wiring verifier recorded a real compile RED before the rule existed and now covers the inclusive
frame boundaries, frame-X boundary, wrong type and inactive tile. Evidence is under
`Build/diagnostics/main-migration/task-8-locked-door-rule/20260819-163225/`; Wiring,
Wiring/Liquid/Chest loopback, Persistence, PlayerPhysics and NPC verifiers exit 0. MainBoundary
scans 469 Simulation source files with 0 violations; serial root Release exits 0 with 0 warnings and
0 errors; `git diff --check` exits 0.

This accepts the locked-door tile/frame predicate only. The current Dome `DoorSnapshot` is not
claimed to be a legacy locked-door owner, and the predicate is not yet wired into the active-above
actuator producer. Tree/special/frame, boulder, multi-tile and remaining `CanKillTile` facts remain
fail-closed. Task 8 remains `进行中`.

## 43. Task 8 / half-brick runtime collision geometry 的已接受执行记录

Version4 `Collision.TileCollision` 的 `Collision.cs:1683-1691` 将 active half-brick 的候选矩形
从完整 tile 改为 `vector4.Y += 8f`、`num7 -= 8`；后续 downward、horizontal 和 upward 分支均使用
该同一矩形。Dome 使用单位 tile 与负 Y 下落，故无 slope 的 half-brick 物理区间是
`[tileY, tileY + 0.5)`，其可站立顶面为 `tileY + 0.5`，而不是此前 whole-tile fallback 的
`tileY + 1`。SHA-256 和冻结来源片段位于
`Build/diagnostics/main-migration/task-8-half-brick/20260819-142432/source-manifest.txt`。

**本批可证伪命题：** active、known、ordinary solid 且 `IsHalfBrick=true, Slope=0` 的 tile 只占
物理下半格。collider 从上方下落时停在 half-brick 顶面并 grounded；仅重叠上半格时可横向通过；
重叠下半格时横向阻挡；从下方上行保留正确的完整 tile 下边缘；精确站在 half-brick 顶面时保持
grounded。完整 Type 1、Type 4、Type 19、unknown rejection 和已有 nonzero-slope whole-tile
fallback 不能回归。

1. **锁定 RED。** `PlayerPhysics.Verification` 新增上述 half-brick scenario，现有 whole-tile
   fallback 在落地断言失败，完整输出为
   `task-8-half-brick/20260819-141923/physics-red.txt`。这是实现前的真实错误，不是编译型占位
   RED。
2. **完成局部几何路径。** `TileCollisionSystem` 以内部 bounds lookup 统一 ordinary solid 的
   横向 overlap、向上底边、下落顶面和 standing 检查。无 slope half-brick 将 top 降为
   `tileY + 0.5`，而 upward bottom 保持 `tileY`；outside-world 和 unknown active tile 的
   prevalidation/rejection 顺序不变。新增命名容差只处理 grid-boundary standing 判断，未引入
   WorldGeneration 的 `TileStateQuery` 或公开几何模块。
3. **保持边界。** platforms 继续走已有 top-surface rule，非 zero slope（包括标记 half-brick 的
   slope）继续保留此前已接受的 whole-tile fallback。本批没有修改 tile registry、Player input、
   protocol、persistence、Server 或 NPC fall-through authority。
4. **最终证据。** 新鲜目录为
   `Build/diagnostics/main-migration/task-8-half-brick/20260819-142432/`：
   `physics-final.txt`、`npc-final.txt`、`liquid-final.txt`、`main-boundary-final.txt` 和
   `root-release-build-final.txt` 均 exit `0`；MainBoundary 扫描 428 个 Simulation source files、
   0 violations；serial root Release 为 0 warnings、0 errors。`diff-check-final.txt` exit `0`。
5. **接受边界。** 已接受无 slope half-brick 的 runtime axis-aligned collision geometry。Task 8
   仍为 `进行中`：四种 slope geometry、actuator、doors/special collision、特殊 platform frame、
   forced fall-through、`fall2` lifecycle、NPC-specific fall-through 与完整 Terraria `Collision`
   parity 仍需各自的 source/oracle + RED 批次。

## 56. Task 8 / boulder chest protection relation accepted

The recovered V1456 `WorldGen.CheckBoulderChest` source (`WorldGen.cs:49518-49538`) establishes a
two-part contract. It first calculates the boulder's two above coordinates from its frame, then
blocks when either coordinate satisfies `CheckTileBreakability_HasReasonToReturnEarly` with
`scanForContainer=true`. The x calculation is order-sensitive: it corrects the negative frame
offset before adding `tileX`; it is not equivalent to correcting a post-subtraction world coordinate.

`Wiring/Systems/BoulderChestProtectionRuleSystem` exposes that source-derived frame projection and
the explicit `isBoulder && (leftBlocked || rightBlocked)` aggregation. It rejects inactive,
negative and non-18-aligned frame inputs. The boulder classification and the two breakability facts
are explicit inputs because current Dome does not yet own Version4 `TileID.Sets.Boulders`,
`IsAContainer`, or every protected-tile relation.

The Wiring verifier first captured a missing-symbol RED, then caught a real source-order bug in the
initial coordinate implementation. The final rule follows the original offset order and passes its
coordinate, left/right aggregation, non-boulder and invalid-frame coverage. Evidence is
`Build/diagnostics/main-migration/task-8-boulder-chest-rule/20260819-163910/`; Wiring,
Wiring/Liquid/Chest loopback, Persistence, PlayerPhysics and NPC verifiers exit 0. MainBoundary
scans 470 Simulation source files with 0 violations; serial root Release exits 0 with 0 warnings and
0 errors; `git diff --check` exits 0.

This accepts the boulder frame relation and fact aggregation only. It does not classify arbitrary
tiles as boulders, approximate containers from `BlocksLiquid`, implement the complete protected-tile
predicate, or enable active-above actuator runtime. Those unknown facts remain fail-closed. Task 8
remains `进行中`.

## 57. Task 8 / tile breakability protection rule accepted

The recovered V1456 `WorldGen.CheckTileBreakability_HasReasonToReturnEarly`
(`WorldGen.cs:63474-63498`) defines a small, source-complete early-return predicate. Provided that
the target type differs from the ignored type, it protects Type 77 outside hardmode and every tile
in `TileID.Sets.PreventsTileRemovalIfOnTopOfIt`. Independently, it protects a source-classified
locked door and, when container scanning is requested, a source-classified container.

`Wiring/Systems/TileBreakabilityProtectionRuleSystem` preserves that predicate with the recovered
V1456 tile-ID sets and delegates locked-door identification to `LockedDoorRuleSystem`. The focused
Wiring verifier captured a genuine missing-symbol RED before the system existed, then covers ignored
type, hardmode, protected-set, locked-door, container-scan and ordinary-tile branches. The rule
intentionally does not test `targetTile.IsActive`, because this source method does not; the enclosing
`CanKillTile` contract owns its separate active, bounds and wall guards.

Evidence is `Build/diagnostics/main-migration/task-8-tile-breakability-rule/20260819-164549/`.
Wiring, Wiring/Liquid/Chest loopback, Persistence, PlayerPhysics and NPC exit 0. MainBoundary scans
471 Simulation files with 0 violations; serial root Release exits 0 with 0 warnings and 0 errors.

This accepts only the pure breakability predicate. Runtime boulder classification, a world-grid
adapter, and the complete active-above actuator path are not accepted. The remaining tree, special,
frame and multi-tile facts continue to fail closed. Task 8 remains `进行中`.

## 58. Task 8 / boulder type classification accepted

The recovered no-deletion V1456 `TileID.Sets.Boulders` source at `TileID.cs:197` lists exactly ten
tile IDs: `138, 484, 664, 665, 711, 712, 713, 714, 715, 716`. The source artifact is frozen in
`Build/diagnostics/main-migration/task-8-boulder-type-rule/20260819-165200/source-manifest.txt`
with SHA-256 `686F5340E5C71F940BCB3C5E30AB9125D032BAFDE991A34129FAB26662528296`.

`Wiring/Systems/LegacyBoulderRuleSystem.IsBoulder` preserves this set as a pure classification
function. The Wiring verifier first recorded a real compile RED for the missing classifier, then
proved all ten IDs and nearby non-boulder IDs. Wiring, Wiring/Liquid/Chest loopback, Persistence,
PlayerPhysics and NPC exit 0. MainBoundary scans 474 Simulation files with 0 violations; serial root
Release exits 0 with 0 warnings and 0 errors; the post-change `git diff --check` exits 0.

This accepts only the tile-ID classifier. It is deliberately not connected to the boulder chest
relation, a world-grid adapter, or active-above actuator runtime: active state, bounds, frame
projection, above-tile breakability and all remaining protection categories still require separate
authority evidence. Unknown facts remain fail closed. Task 8 remains `进行中`.

## 59. Task 8 / boulder chest world-grid protection query accepted

The recovered V1456 `WorldGen.CheckBoulderChest` (`WorldGen.cs:49518-49538`) derives two upper
coordinates from the active boulder's frame, then invokes
`CheckTileBreakability_HasReasonToReturnEarly` for both with the boulder type as `ignoreType` and
`scanForContainer=true`. The source artifact and hash are recorded in
`Build/diagnostics/main-migration/task-8-boulder-protection-query/20260819-165500/source-manifest.txt`.

`Wiring/Systems/LegacyBoulderChestProtectionQuery` now owns this read-only projection. It requires
an in-bounds active tile classified by `LegacyBoulderRuleSystem`, reuses the accepted frame
coordinate relation, bounds-checks both upper cells, and evaluates both through
`TileBreakabilityProtectionRuleSystem`; only the OR result is returned as a known fact. The query
does not mutate the grid, create commands, access Main/Protocol/Server, or enable actuator behavior.

The Wiring verifier captured a missing-query compile RED, then proves a protected container blocks,
an ordinary pair permits, and invalid/out-of-bounds/non-boulder inputs return no fact. Wiring,
Wiring/Liquid/Chest loopback, Persistence, PlayerPhysics and NPC exit 0. MainBoundary scans 475
Simulation files with 0 violations; serial root Release exits 0 with 0 warnings and 0 errors.

This accepts the boulder world-grid read projection only. It is not yet composed into the complete
`CanKillTile` producer or active-above actuator runtime: tree/special/frame/multi-tile ownership and
other unowned protections remain fail closed. Task 8 remains `进行中`.

## 60. Task 8 / tree-trunk type classification accepted

The recovered V1456 `TileID.Sets.IsATreeTrunk` declaration at `TileID.cs:163` contains exactly
`5, 72, 583, 584, 585, 586, 587, 588, 589, 596, 616, 634`. The frozen source manifest is
`Build/diagnostics/main-migration/task-8-tree-trunk-rule/20260819-170000/source-manifest.txt`
with SHA-256 `686F5340E5C71F940BCB3C5E30AB9125D032BAFDE991A34129FAB26662528296`.

`Wiring/Systems/LegacyTreeTrunkRuleSystem.IsTreeTrunk` preserves this set as a pure classifier.
The Wiring verifier recorded a genuine missing-symbol RED, then proved all twelve IDs and nearby
non-trunk IDs. Wiring, Wiring/Liquid/Chest loopback, Persistence, PlayerPhysics and NPC exit 0;
MainBoundary scans 476 Simulation files with 0 violations; serial root Release exits 0 with 0
warnings and 0 errors.

Only the source set is accepted. The `CanKillTile` tree frame exceptions, upper-tile world-grid
projection, special type relations and active-above actuator producer remain separate, unowned
contracts and continue to fail closed. Task 8 remains `进行中`.

## 61. Task 8 / tree-trunk frame protection predicate accepted

The recovered V1456 `CanKillTile` active-above tree branch (`WorldGen.cs:63252-63265`) protects a
candidate when the above tile is a source-classified tree trunk, the candidate type differs, neither
of the two source frame exceptions applies, and `frameY < 198`. The exact frame exceptions are
`frameX == 66 && frameY in [0,44]` and `frameX == 88 && frameY in [66,110]`. The source hash and
line contract are frozen in `Build/diagnostics/main-migration/task-8-tree-frame-rule/20260819-170200/source-manifest.txt`.

`Wiring/Systems/TreeTrunkProtectionRuleSystem.ShouldProtectAbove` preserves that expression as a
pure predicate and delegates the type set to `LegacyTreeTrunkRuleSystem`. The Wiring verifier first
recorded a missing-symbol RED; after correcting two boundary assertions against the strict source
comparison, it proves candidate-type equality, both inclusive exemptions, `frameY=197` protection,
and `frameY=198` rejection. Wiring, Wiring/Liquid/Chest loopback, Persistence, PlayerPhysics and
NPC exit 0; MainBoundary scans 478 Simulation files with 0 violations; serial root Release exits
0 with 0 warnings and 0 errors.

This accepts only the pure tree-frame predicate. It does not project the above tile from the runtime
world, compose `CanKillTile`, or enable active-above actuator behavior. Special types and multi-tile
relations remain separate fail-closed slices. Task 8 remains `进行中`.

## 62. Task 8 / special active-above protection predicate accepted

The recovered V1456 `CanKillTile` special branch (`WorldGen.cs:63260-63291`) has three exact cases.
When the candidate type differs from an active above tile, Type 323 is protected only for
`frameX == 66 || frameX == 220`; Types 21, 26, 72, 77, 88, 467 and 488 are always protected; Type
80 is protected when `frameX / 18` is 0, 1, 4 or 5. The frozen source manifest is
`Build/diagnostics/main-migration/task-8-special-rule/20260819-170400/source-manifest.txt`.

`Wiring/Systems/SpecialTileProtectionRuleSystem.ShouldProtectAbove` preserves those branches as a
pure rule, including active-tile and candidate-type guards. The Wiring verifier recorded a missing
symbol RED, then proves Type 323 frame boundaries, all seven fixed types, Type 80 protected and
unprotected columns, equal candidate types and inactive rejection. Wiring, Wiring/Liquid/Chest
loopback, Persistence, PlayerPhysics and NPC exit 0; MainBoundary scans 481 Simulation files with
0 violations; serial root Release exits 0 with 0 warnings and 0 errors.

This accepts only the pure special predicate. No upper-tile runtime projection, composed
`CanKillTile` producer or active-above actuator behavior is enabled; multi-tile and remaining
authority categories stay fail closed. Task 8 remains `进行中`.

## 63. Task 8 / Type 235 multi-tile protection predicate accepted

The recovered V1456 `CanKillTile` Type 235 branch (`WorldGen.cs:63300-63315`) checks the three
upper cells of the object footprint. It derives the horizontal origin from `frameX % 54`, reads
three cells at `y - 1`, ignores inactive cells, and rejects when any active cell satisfies
`CheckTileBreakability_HasReasonToReturnEarly` with `scanForContainer=true`.

`Wiring/Systems/MultiTileProtectionRuleSystem.IsBlocked` preserves this pure aggregation contract:
it accepts only candidate Type 235 and an exact three-cell input, applies the accepted breakability
rule to active cells, and ORs the results. The Wiring verifier captured a missing-symbol RED, then
proved blocked, ordinary, inactive and incomplete-footprint cases. Wiring, Wiring/Liquid/Chest
loopback, Persistence, PlayerPhysics and NPC exit 0; MainBoundary scans 482 Simulation files with
0 violations; serial root Release exits 0 with 0 warnings and 0 errors.

This accepts only the pure Type 235 rule. Runtime frame-origin projection, world-grid ownership,
composed `CanKillTile` authority and active-above actuator behavior remain deferred and fail closed.
Task 8 remains `进行中`.

## 64. Task 8 / PreventsActuationUnder type classification accepted

The recovered V1456 `TileID.Sets.PreventsActuationUnder` declaration at `TileID.cs:315` contains
exactly `21, 467, 26, 77, 88, 470, 475, 237, 597, 441, 468`. The source manifest and SHA-256 are
recorded in `Build/diagnostics/main-migration/task-8-actuation-set-rule/20260819-170800/source-manifest.txt`.

`Wiring/Systems/LegacyActuationProtectionRuleSystem.PreventsActuationUnder` preserves this set as a
pure classifier. The Wiring verifier captured a missing-symbol RED, then proved all eleven IDs and
nearby ordinary IDs. Wiring, Wiring/Liquid/Chest loopback, Persistence, PlayerPhysics and NPC exit
0; MainBoundary scans 484 Simulation files with 0 violations; serial root Release exits 0 with 0
warnings and 0 errors.

This accepts the exact set only. It is not yet connected to `ActuatorDeactivationRuleSystem`, a
world-grid projection or active-above actuator runtime; unsupported Type 226/world-surface and
other source predicates remain separately fail closed. Task 8 remains `进行中`.

## 65. Task 8 / Type 235 multi-tile world-grid query accepted

The recovered V1456 Type 235 branch derives `originX = x - (frameX % 54) / 18`, then checks the
three tiles from `originX` through `originX + 2` at `y - 1`. The frozen source contract is in
`Build/diagnostics/main-migration/task-8-multitile-query/20260819-171000/source-manifest.txt`.

`Wiring/Systems/LegacyMultiTileProtectionQuery.TryGetIsBlocked` is the corresponding read-only
WorldGrid adapter. It requires an in-bounds active Type 235 tile with a non-negative 18-aligned
frame and three in-bounds upper cells, derives the source origin, then delegates to the accepted
three-cell protection rule. Invalid candidates or unavailable footprint state return no fact rather
than permission. It does not mutate the world, issue commands or enable actuator behavior.

The Wiring verifier recorded a missing-query RED, then proves frame-origin projection, blocked and
ordinary footprints, and out-of-bounds rejection. Wiring, Wiring/Liquid/Chest loopback, Persistence,
PlayerPhysics and NPC exit 0; MainBoundary scans 487 Simulation files with 0 violations; serial
root Release exits 0 with 0 warnings and 0 errors.

This accepts the Type 235 read projection only. It is not yet composed into the complete
`CanKillTile` producer or active-above actuator runtime; all producer integration stays fail closed
until the complete query's inputs and call timing are accepted. Task 8 remains `进行中`.

## 66. Task 8 / composed CanKillTile read query accepted

The recovered V1456 `WorldGen.CanKillTile` source (`WorldGen.cs:63226-63334`) is now represented by
`Wiring/Systems/LegacyCanKillTileQuery.TryGetCanKill`. It validates world bounds, tile existence,
active state and wall 350; reads the active-above tree and special relations; composes the accepted
boulder, locked-door, Type 235 multi-tile and chest projections; and delegates the final boolean to
`ActuatorCanKillTileRuleSystem`. The source manifest is
`Build/diagnostics/main-migration/task-8-can-kill-query/20260819-171200/source-manifest.txt`.

The Wiring verifier captured a missing-query compile RED, then proves ordinary acceptance, active-
above tree rejection, locked-door rejection and bounds rejection. The final focused run after
separating tree and special facts passes; the serial root Release rerun after that refactor exits 0
with 0 warnings and 0 errors. Earlier same-batch regressions passed with MainBoundary at 490 files;
the query itself remains read-only and has no Main/Protocol/Server dependency.

This accepts a composed read query, not an actuator producer. It is not wired into
`ActuatorCommandSystem`; unsupported caller facts, source timing and any missing projection still
fail closed. Task 8 remains `进行中`.

## 67. Task 8 / active-above Type 1 actuator query integration accepted

Recovered V1456 `Wiring.DeActive` (`Wiring.cs:3302-3338`) deactivates a solid tile only when the
tile above is inactive, or when that active upper tile is not in `PreventsActuationUnder` and
`WorldGen.CanKillTile(i,j)` succeeds. The source hash and contract are frozen in
`Build/diagnostics/main-migration/task-8-active-above-actuator/20260819-171500/source-manifest.txt`.

`ActuatorCommandSystem.CreateCommands` now takes an optional explicit `canKillTileQuery`. Its Type 1
path preserves the former fail-closed behavior if no callback exists; for an active upper tile it
checks `LegacyActuationProtectionRuleSystem` before invoking the callback. `DomeSimulation` injects
that callback from its authoritative `WorldGrid`, hardmode progression, chest dictionary and chest
index through `LegacyCanKillTileQuery`. The normal wiring phase still emits `SetInactive` commands
and the existing world commit remains the only mutator.

The Wiring verifier recorded the missing callback parameter RED, then proves callback-authorized
active-above deactivation and `PreventsActuationUnder` rejection. Wiring, Wiring/Liquid/Chest
loopback, Persistence, PlayerPhysics and NPC exit 0; MainBoundary scans 492 Simulation files with
0 violations; serial root Release exits 0 with 0 warnings and 0 errors.

This accepts active-above integration only for the current source-backed Type 1 path. Type 226
world-surface/progression behavior, broader solid/not-really-solid definitions, seven special tile
types, frame/network timing and full legacy actuator parity remain separate work. Task 8 remains
`进行中`.

## 69. Task 8 / Type 226 actuator runtime slice accepted

The V1456 `Wiring.DeActive` target guard is now connected for the source-backed Type 226 slice.
`LegacyActuatorTileDefinitionRuleSystem` owns only the proven actuator definition facts in this
batch: Type 1 and Type 226 are solid, while the V1456 `NotReallySolid` set is explicitly recorded as
`10, 387, 388`; no liquid or collision definition is repurposed as `Main.tileSolid`.

`ActuatorCommandSystem.CreateCommands` receives optional `WorldMetadata` and
`WorldProgressionState`. Type 226 requires an available `WorldSurface`; it rejects when the target
Y is below that surface and Plantera is not defeated, permits it when Plantera is defeated, and
permits it above the surface. Missing surface remains fail closed. `DomeSimulation` stores metadata
when restoring a snapshot or creating a persistence snapshot and passes it with authoritative
progression into the wiring phase. The existing command -> deterministic world commit path remains
the only state mutator.

The Wiring verifier recorded the missing `worldMetadata/progression` API RED, then proves all four
surface/progression branches and preserves existing Type 1, active-above, protected-upper and
atomic multi-tile behavior. Evidence is in
`Build/diagnostics/main-migration/task-8-type226-actuator/20260820-035704/`. Final serial Persistence,
WorldImport, Wiring/Liquid/Chest loopback,
PlayerPhysics, NPC and MainBoundary gates pass; MainBoundary reports 504 Simulation files and 0
violations; serial root Release reports 0 warnings and 0 errors.

This accepts only Type 1/Type 226 actuator definition and Type 226 surface/progression runtime
behavior. The full `Main.tileSolid` table, all actuator tile types, remaining special definitions,
SquareTileFrame behavior, TileSquare timing and full legacy actuator parity remain deferred. Task 8
remains `进行中`.

## 70. Task 8 / special actuator non-actuated set accepted

The explicit V1456 `Wiring.DeActive` switch cases are now represented by
`LegacyActuatorTileDefinitionRuleSystem`: `314, 379, 386, 387, 388, 389, 476` are known
non-actuated targets. The classifier keeps the independent V1456 `NotReallySolid` facts for `387`
and `388` (and `10`) while returning no solid permission for any of the seven switch cases. The
actuator system therefore rejects these targets without mutating the world, while existing Type 1
and Type 226 paths remain unchanged.

The Wiring verifier recorded a real RED because Type 314 was not classified as a known special
target, then proves all seven IDs reject `SetInactive` commands. Persistence, Wiring/Liquid/Chest
loopback, PlayerPhysics and NPC exit 0; MainBoundary scans 506 Simulation files with 0 violations;
serial root Release exits 0 with 0 warnings and 0 errors. Evidence is in
`Build/diagnostics/main-migration/task-8-special-actuator/20260820-040847/`.

This accepts only the seven explicit special-case IDs and their fail-closed actuator behavior. The
complete `Main.tileSolid` table, other actuator definitions, SquareTileFrame behavior, TileSquare
timing and full actuator parity remain deferred. Task 8 remains `进行中`.

## 71. Task 8 / source-backed Type 2 actuator classification accepted

The no-deletion V1456 `Main.cs` initialization path explicitly assigns `tileSolid[2] = true`
(around `Main.cs:8010-8025`). This batch adds only that individual solid fact to
`LegacyActuatorTileDefinitionRuleSystem`; it does not infer a default for unknown tile IDs and does
not merge `tileSolid` with `NotReallySolid`.

The Wiring verifier recorded a real RED when the Type 2 fact was absent, then proves the Type 2
classifier and a Type 2 active, actuated, no-active-above tile producing one state-preserving
`SetInactive` command. Persistence, Wiring/Liquid/Chest loopback, PlayerPhysics and NPC all exit
`0`; MainBoundary scans 507 Simulation files with 0 violations; serial root Release exits `0` with
0 warnings and 0 errors. Evidence is in
`Build/diagnostics/main-migration/task-8-type2-actuator/20260820-041700/`.

This accepts only the Type 2 definition and its existing Type 1-compatible no-active-above path.
The complete `Main.tileSolid`/`tileSolidTop` initialization, runtime rewrites, other tile IDs,
multi-tile framing, TileSquare timing and full actuator parity remain deferred. Task 8 remains
`进行中`.

## 72. Task 8 / V1456 static tile-solid definition accepted

The no-deletion V1456 source initializes the base `Main.tileSolid` array in
`Initialize_TileAndNPCData2` and `Initialize_TileAndNPCData1` (`Main.cs:6952-10727`). The two
methods have 324 final `true` facts and six explicit `false` facts (`3, 4, 5, 11, 110, 634`),
including the three finite assignment loops `255..268`, `435..439`, and `727..732`.
The existing immutable `TileDefinitionRegistry.CreateVersion4Base` already preserves this complete
static base set. `LegacyActuatorTileDefinitionRuleSystem` now consumes its `BlocksLiquid` fact as
the source-equivalent `tileSolid` classification. It keeps `NotReallySolid` independent and retains
the seven `Wiring.DeActive` switch exclusions at the command rule, so a source-solid special type is
still not actuator-permitted.

The Wiring verifier recorded RED when the prior narrow set rejected static Type 0. GREEN enumerates
all 324 source-true types and all six explicit false types. A clean replay of the focused verifier
has byte-identical output. Persistence, Wiring/Liquid/Chest loopback, PlayerPhysics and NPC exit
`0`; MainBoundary scans 508 Simulation files with 0 violations; serial root Release exits `0` with
0 warnings and 0 errors. Evidence is in
`Build/diagnostics/main-migration/task-8-static-solid-definition/20260820-042319/`.

This accepts the initialization-time base definition only. `Main.DoUpdateInWorld` temporarily
sets `tileSolid[379]` false and later true in its own runtime lifecycle; that override is not
represented here. `tileSolidTop`, framing, TileSquare timing, runtime override ownership and full
actuator parity remain deferred. Task 8 remains `进行中`.

## 73. Task 9 / server-owned moon phase state accepted

V1456 `Main.UpdateTime_StartDay` increments `moonPhase` at dawn and wraps values at `8`
(`Main.cs:66264-66301`). `WorldClock` now owns a validated `0..7` byte and changes it only when
the authoritative clock crosses from night to day. It exposes the value through
`WorldClockSnapshot`; paused clocks do not advance it and invalid phase values are rejected.

`DomeStatePersistenceFormat` is v19. Its single phase byte is appended after the v18 world-surface
tail so all v1-v18 layouts remain exact prefixes and restore phase 0. Invalid and truncated v19
phase data is rejected while existing trailing-data rejection remains active. `DomeServer` reads
the immutable Simulation phase and passes it to `LegacyWorldDataContext.WithWorldState`, which fills
the existing V1456 WorldData MoonPhase field without changing packet layout.

The batch recorded a compile RED for the missing clock state, a persistence RED for lost phase, and
a projection RED for default WorldData phase. GREEN proves dawn `7 -> 0`, pause preservation,
invalid phase rejection, v19 round-trip, v18 default recovery, invalid/truncated v19 rejection and
WorldData payload byte projection. The focused replay output is identical. WorldRules, Persistence,
WorldRules TCP loopback, Protocol Compatibility and FullClientBootstrap exit `0`; MainBoundary scans
512 Simulation files with 0 violations; serial root Release exits `0` with 0 warnings and 0 errors.
Evidence is in `Build/diagnostics/main-migration/task-9-moon-phase-state/20260820-043724/`.

This accepts state, dawn transition, persistence and projection only. Remix-world gameplay-day
semantics, moon visuals/types, automatic event eligibility, NPC/player behavior, and the legacy
random stream remain deferred. Task 9 remains `进行中`.

## 74. Task 9 / WLD moon phase import accepted

The legacy WLD Header stores `_tempMoonPhase` as an `Int32` after saved time and day-time state
(`Terraria.IO/WorldFile.cs:1310-1316`, `2123-2129`). Both supported V319 Header readers now read
that exact positional value, reject values outside `0..7`, and carry the validated byte through
`LegacyWorldMetadata -> CompatibilityWorldMetadata -> CompatibilityToDomeProjection` to the
existing immutable `WorldClockSnapshot`. Generated worlds and prior Dome snapshots remain phase 0;
this import path does not invent a seed-derived replacement.

The batch recorded the missing metadata-member RED and then proved valid pointer-table phase
preservation, invalid `-1` and `8` WLD rejection, V1-to-V319 parsing, recorded differential-oracle
parity, compatibility projection, persistence, protocol compatibility, and identical WorldRules
replay output. MainBoundary scans 516 Simulation files with 0 violations; serial root Release exits
0 with 0 warnings and 0 errors. Evidence is in
`Build/diagnostics/main-migration/task-9-wld-moon-phase-import/20260820-045545/`.

The isolated WLD reference copy was restored byte-for-byte to its recorded source hash during this
batch. Its six source-authentic trailing spaces intentionally remain, so a whole-worktree
`git diff --check` reports those lines; the scoped runtime/source/test diff check is clean.

This accepts only moon-phase import. Legacy time-of-day, blood moon, eclipse, moon type, visual
rendering, event eligibility, and NPC/player consumers remain deferred. Task 9 remains `进行中`.

## 75. Task 9 / WLD Blood Moon and Eclipse import accepted

The legacy WLD Header writes `_tempBloodMoon` and `_tempEclipse` immediately after the saved
moon phase (`Terraria.IO/WorldFile.cs:1312-1316`) and restores the same values directly
(`2125-2130`). V319 now carries both values from `LegacyWorldMetadata` through
`CompatibilityWorldMetadata` to `WorldProgressionState`, the existing authoritative owner. It does
not normalize the flags from the current day/night clock: legacy loading restores them directly,
and the existing progression tick retains responsibility for lifecycle transitions.

The version boundary is explicit: legacy v1-v69 has no Eclipse byte and defaults it to `false`;
v70 and later read the serialized value. The batch recorded the missing metadata-member RED, then
proves the v69/v70 branch, v319 flags, parser differential-oracle parity, compatibility projection,
persistence, protocol compatibility and identical WorldRules replay. MainBoundary scans 517
Simulation files with 0 violations; serial root Release exits 0 with 0 warnings and 0 errors.
Evidence is in
`Build/diagnostics/main-migration/task-9-wld-event-flags-import/20260820-133913/`.

WLD saved `time` and `dayTime` remain explicitly deferred. The legacy field is a `Double`, and the
same V1456 source produces fractional values in its local/menu time path, whereas the authoritative
`WorldClock.TimeOfDay` is an `Int32`. No source-backed conversion permits truncation or rounding;
the behavior needs a deliberate clock-representation migration rather than a lossy import shortcut.

This accepts only WLD Blood Moon and Eclipse import. WLD time/day-time, moon type, rendering,
event eligibility and NPC/player consumers remain deferred. Task 9 remains `进行中`.

## 76. Task 9 / WLD hard-mode import accepted

The legacy WLD Header writes `Main.hardMode` after `shadowOrbCount` and `altarCount`
(`Terraria.IO/WorldFile.cs:1338-1344`) and restores it at the same position (`2155-2163`). V319 now
carries that Boolean through `LegacyWorldMetadata -> CompatibilityWorldMetadata` to the existing
authoritative `WorldProgressionState.IsHardMode`. No boss flags, ore tiers, invasion values or later
Header fields were interpreted as part of this batch.

The historical layout boundary is explicit: v1-v22 has no hard-mode field and defaults false; v23
and later read the serialized Boolean after the altar count. The batch recorded the missing
metadata-member RED, then proves v22/v23, v319, differential-oracle parity, compatibility
projection, persistence, protocol compatibility and identical WorldRules replay. MainBoundary scans
518 Simulation files with 0 violations; serial root Release exits 0 with 0 warnings and 0 errors.
Evidence is in `Build/diagnostics/main-migration/task-9-wld-hardmode-import/20260820-134904/`.

This accepts only WLD hard-mode import. WLD time/day-time, boss/progression flags, ore tiers,
invasion state, slime rain, rain, wind, rendering and gameplay consumers remain deferred. Task 9
remains `进行中`.

## 77. Task 9 / WLD source-backed progression facts import accepted

The WLD Header stores the six source facts `downedBoss1`, `downedBoss2`, `downedBoss3`,
`downedMechBossAny`, `downedPlantBoss` and `downedGolemBoss`
(`Terraria.IO/WorldFile.cs:1319-1330`, `2133-2143`). V319 now consumes the full contiguous
Boolean sequence by name and maps only those direct facts to `WorldProgressionState`:
`DefeatedEyeOfCthulhu`, `DefeatedEaterOrBrain`, `DefeatedSkeletron`,
`DefeatedMechanicalBoss`, `DefeatedPlantera` and `DefeatedGolem`.

The batch deliberately skips `downedQueenBee`, individual mechanical-boss flags, Slime King and
any Wall of Flesh inference because the current authority model has no matching source field for
those claims. The V1-V319 parser matrix preserves historical layout alignment; the v319 true-value
fixture, differential Oracle, compatibility projection, persistence, protocol compatibility and
identical WorldRules replay all pass. MainBoundary scans 520 Simulation files with 0 violations;
serial root Release exits 0 with 0 warnings and 0 errors. Evidence is in
`Build/diagnostics/main-migration/task-9-wld-progression-facts-import/20260820-140022/`.

This accepts only the six listed WLD progression facts. WLD time/day-time, Queen Bee, individual
mechanical bosses, Slime King, Wall of Flesh, ore tiers, invasion, weather and remaining gameplay
state remain deferred. Task 9 remains `进行中`.

## 78. Task 9 / WLD Crimson-world import accepted

The source Header writes `WorldGen.crimson` after the dungeon coordinates
(`Terraria.IO/WorldFile.cs:1317-1320`) and restores it directly at the same position
(`2131-2134`). V319 now carries that Boolean through
`LegacyWorldMetadata -> CompatibilityWorldMetadata -> CompatibilityToDomeProjection` into the
existing authoritative `WorldRuleState.IsCrimsonWorld`. The projection does not derive the world
variant from seed, biome tiles, background values, or a boss/progression fact.

The legacy layout boundary is explicit: v1-v55 has no Crimson Boolean and restores `false`; v56
and later read the serialized field after the dungeon coordinates. The batch captured the missing
metadata-member RED, then proves v55/v56, v319 true preservation, V1-to-V319 parser matrix,
recorded differential-oracle parity, compatibility projection, persistence, protocol compatibility
and identical WorldRules replay. MainBoundary scans 522 Simulation files with 0 violations; serial
root Release exits 0 with 0 warnings and 0 errors. Evidence is in
`Build/diagnostics/main-migration/task-9-wld-crimson-import/20260820-000000/`.

This accepts only WLD Crimson-world state import. Crimson biome generation, Corruption/Crimson
selection behavior, background fields, saved time/day-time, and remaining Header fields remain
deferred. Task 9 remains `进行中`.

## 79. Task 9 / WLD invasion size and type import accepted

The source Header writes `Main.invasionSize` and `Main.invasionType`
(`Terraria.IO/WorldFile.cs:1344-1347`) and restores them directly (`2168-2171`). Both current
authority fields already exist in `WorldProgressionState`, so V319 now carries only these two
direct integers through `LegacyWorldMetadata -> CompatibilityWorldMetadata` to that state. The
binary readers still consume the preceding `invasionDelay` and following `invasionX`, but neither
is assigned a guessed owner in this batch.

There is no historical presence branch: the legacy v1 reader restores the same ordered fields
(`3677-3680`). The batch captured missing metadata facts as RED, then proves v1 and v319 positive
facts, compatibility projection, and fail-closed rejection when the existing authoritative
non-negative size invariant is violated. The V1-to-V319 parser matrix, recorded differential
Oracle, persistence, protocol compatibility and identical WorldRules replay all pass.
MainBoundary scans 522 Simulation files with 0 violations; serial root Release exits 0 with 0
warnings and 0 errors. Evidence is in
`Build/diagnostics/main-migration/task-9-wld-invasion-import/20260820-000001/`.

This accepts only WLD invasion size and type. Invasion delay, X position, start size, spawn/recovery
behavior, weather, saved time/day-time and other Header fields remain deferred. Task 9 remains
`进行中`.

## 80. Task 9 / WLD game-mode import accepted

The Header encodes GameMode in three source-backed layouts: v1-v111 has no field and restores
Classic, v112-v208 stores Expert as a Boolean with v208's separate Master Boolean, and v209+
stores an `Int32` (`Terraria.IO/WorldFile.cs:2036-2093`, `3545-3553`). The new immutable
`WorldRuleState.GameMode` preserves the four source identities Classic, Expert, Master and Journey.

The import deliberately does not treat GameMode as the existing numeric difficulty verbatim.
V1456 `Main.Difficulty` maps only modes 1 and 2 to Expert and Master; Journey mode 3 remains at
Classic base difficulty (`Terraria/Main.cs:1729-1753`). Projection therefore maps Journey to
`GameMode=Journey`, `Difficulty=0`, `IsExpertMode=false`, and `IsMasterMode=false`, preventing an
incorrect Master-mode behavior claim. Invalid mode values reject at authoritative projection.

The batch recorded the missing metadata/member RED, then proves v112 Expert, v208 Master, v319
Journey, Journey's non-Master derivation, invalid-mode rejection, V1-to-V319 parser matrix,
recorded differential Oracle, compatibility projection, and persistence v20 Journey round-trip.
The append-only v20 field restores Classic for every v1-v19 Dome state; invalid and truncated v20
values reject. Protocol compatibility and identical WorldRules replay pass. MainBoundary scans 530
Simulation files with 0 violations; serial root Release exits 0 with 0 warnings and 0 errors.
Evidence is in
`Build/diagnostics/main-migration/task-9-wld-game-mode-import/20260820-000003/`.

This accepts WLD game-mode identity and its no-override base derivation only. Journey creative
difficulty overrides, secret-seed effective-difficulty modifiers, difficulty-dependent gameplay
systems not yet migrated, and remaining Header fields remain deferred. Task 9 remains `进行中`.

## 81. Task 9 / WLD pending-meteor schedule import accepted

The WLD Header directly saves `WorldGen.spawnMeteor` (`Terraria.IO/WorldFile.cs:1339`) and both
modern and legacy readers restore that Boolean directly (`2156`, `3670`). V319 now carries only
that source fact through `LegacyWorldMetadata -> CompatibilityWorldMetadata ->
CompatibilityToDomeProjection -> WorldProgressionState.IsMeteorScheduled`. The import does not
enqueue a meteor, consume a random value, choose a landing site or resolve an impact.

The parser verifier proves true preservation in v87 legacy and v319 pointer-table layouts, as well
as the full historical version matrix and recorded differential Oracle. WorldImport proves the
metadata/compatibility projection. Persistence, Protocol Compatibility and two identical
WorldRules replays all exit `0`; MainBoundary scans 534 Simulation files with 0 violations; the
serial root Release build exits `0` with 0 warnings and 0 errors. Evidence is in
`Build/diagnostics/main-migration/task-9-wld-meteor-schedule-import/20260820-000004/`.

This accepts direct restoration of the pending meteor schedule state only. `shadowOrbSmashed`,
orb/altar counters, random scheduling, landing search, impact resolution and client ambience stay
deferred. Task 9 remains `进行中`.

## 82. Task 9 / lossless weather-state representation accepted

`WorldFile.cs:2180-2183` restores independent `raining`, `rainTime` and `maxRaining` facts, but
the former `WorldRuleState` derived rain activity from duration and had no distinct maximum rain
strength. The state model now holds immutable `IsRaining` and `MaximumRainStrength` alongside the
existing duration/current strength. Existing runtime `WithRain` transitions retain their canonical
active/clear behavior; `WithRawRain` is a recovery transformation, not a protocol mutation path.

`DomeStatePersistenceFormat` v21 appends raw activity and maximum strength after the v20 GameMode
tail. v1-v20 layouts remain exact prefixes and recover their documented historical derivation. The
batch recorded the missing parameter/member RED, then proves a raw inactive-but-saved rain state,
v21 round-trip and v20 compatibility. WorldRules replay twice, Persistence, WorldImport, Protocol
Compatibility and MainBoundary all exit `0`; MainBoundary scans 546 Simulation files with 0
violations; serial root Release exits `0` with 0 warnings and 0 errors. Evidence is in
`Build/diagnostics/main-migration/task-9-weather-state-model/20260820-000006/`.

This accepts weather representation and persistence only. WLD rain parsing/projection,
`FixEndlessRainWorlds()` repair branches, weather scheduling and client presentation remain
deferred to separate cards. Task 9 remains `进行中`.

## 83. Task 9 / domain-scoped world-event random stream accepted

The simulation now owns an explicit `WorldEventRandomState` for future world-event probability
decisions. The value is restored from and written to `DomeSimulationSnapshot`; persistence format
v22 stores the exact `UInt32`, while v1-v21 derive the documented world-seed fallback. The stream
advances deterministically without depending on client calls, protocol order or global legacy
`Main.rand` consumption.

The batch recorded v22 fixture fallout as RED (strict trailing-data rejection), then repaired the
historical compatibility fixtures and added exact round-trip, v21 fallback, truncation rejection
and simulation snapshot-continuity assertions. WorldRules was run twice with identical PASS sets;
Persistence, WorldImport and Protocol Compatibility pass; MainBoundary scans 559 Simulation files
with 0 violations; serial root Release passes with 0 warnings and 0 errors. Evidence is in
`Build/diagnostics/main-migration/task-9-random-stream-contract/20260820-000009/`.

This accepts only the random-state ownership contract. It does not claim global `Main.rand` order,
WLD rain/wind import, probabilistic meteor scheduling or replacement of existing item/loot random
paths. Task 9 remains `进行中`.

## 84. Task 9 / modern WLD wind target import accepted

The source has a hard version boundary. For WLD versions `>=62`, the header directly restores
`windSpeedTarget` and sets current equal to target (`WorldFile.cs:3762-3767`, modern load
`2198-2200`). The parser now carries this nullable source fact through legacy metadata and
compatibility projection into `WorldRuleState`, preserving exact `Single` bits and the source
target/current relation. Existing rule validation rejects non-finite or out-of-range values.

For versions `<62`, the source calls `WorldGen.RandomizeWeather()` instead. That branch consumes
legacy `genRand`, repeatedly chooses a nonzero signed value and resets cloud state; no compatible
seed/call-context contract exists yet. It remains explicitly `null`/deferred and is not replaced by
the domain event stream.

Parser version-boundary fixtures, the historical matrix and recorded oracle, WorldImport and
WorldCompatibility all pass. Persistence passes; WorldRules replayed twice with an identical PASS
set; MainBoundary scans 563 Simulation files with 0 violations; serial root Release passes with
0 warnings and 0 errors. Evidence is in
`Build/diagnostics/main-migration/task-9-wld-wind-import/20260820-000010/`.

This accepts only direct modern wind target restoration. Old-layout random weather, cloud count,
rain import and client presentation remain deferred. Task 9 remains `进行中`.

## 85. Task 9 / Main responsibility coverage matrix accepted

The responsibility ledger is now executable at the boundary gate through
`docs/research/2026-08-20-main-member-coverage-matrix.md`. Thirty source-anchored rows classify
server-relevant responsibility groups as `accepted`, `planned`, `unknown` or `excluded`. The gate
rejects missing/invalid rows, empty ownership fields and missing coverage for world clock/rules,
entities, events, server bootstrap, persistence and randomness.

This is transparent accounting, not a parity percentage. Client rendering, UI, local input, asset
loading and presentation effects are explicitly excluded and cannot be satisfied by copying them
into Simulation. MainBoundary passes with 571 Simulation files and 0 violations. Evidence is in
`Build/diagnostics/main-migration/task-9-main-member-coverage/20260820-000012/`.

Task 9 remains `进行中`; entity gap cards, tick-order reconciliation and unresolved/unknown rows
remain open.

## 86. Task 9 / Main tick-order coverage accepted

The source-to-phase matrix now records the proven subset of `Main.Update` ordering and the explicit
unknown boundaries. The legacy source calls `UpdateTime` before `WorldGen.UpdateWorld` and
`UpdateInvasion`, while projectile/item loops occur before `UpdateTime`; the current ECS named
schedule is therefore treated as an authority schedule for supported domains, not as whole-loop
parity. Client rendering, ambient wind, camera and server transport timing remain outside
Simulation.

TickOrder verifier replay 1 and replay 2 pass with identical named phase, pause and deterministic
input assertions. MainBoundary scans 579 Simulation files with 0 violations; serial root Release
passes with 0 warnings and 0 errors. Evidence is in
`Build/diagnostics/main-migration/task-9-tick-order-coverage/20260820-000014/`.

This accepts only the auditable phase coverage and existing local invariants. WorldGen update,
invasion travel, entity death/loot/replication ordering and full legacy loop parity remain open.

## 87. Task 9 / Player lifecycle responsibility coverage accepted

The Main responsibility row for `player[]` and player lifecycle is accepted for the verified
server-authoritative subset: identity, ownership, lifecycle snapshots, tile-authoritative collision,
death/respawn, UUID restore protection and typed V1456 active/life projections. Loopback proves
automatic respawn is authoritative and PVS-limited. Evidence is in
`Build/diagnostics/main-migration/task-9-player-lifecycle-coverage/20260821-000015/`.

Full legacy Player.cs parity, client presentation/input, buffs, mounts, loadouts and remaining
inventory/use behavior remain open.

## 68. Task 8 / source world-surface metadata preservation accepted

The V319 WLD reader already decodes `worldSurface` as an independent `double`
(`Terraria.WorldFile.V319/Format/WldHeaderReader.cs:75`; legacy reference `WorldFile.cs` writes
and reads the same value). This batch carries that source value through
`LegacyWorldMetadata -> CompatibilityWorldMetadata -> CompatibilityToDomeProjection ->
WorldMetadata`. `WorldMetadata.WorldSurface` is `double?`: a WLD import supplies the exact source
value, while generated worlds and evidence-free historical Dome snapshots retain `null` as unknown.
No world-height, spawn-Y, or other guessed fallback is introduced.

`WorldPersistenceFormat` is now v3 and stores a presence bit plus the `double`; it continues to
read v1 and v2 as unknown. `DomeStatePersistenceFormat` is now v18 and appends the same optional
field after the complete v17 progression record, so every v1-v17 layout remains an exact prefix and
restores `null`. The Persistence verifier proves exact `123.5` embedded/outer round-trip, v1 and
v2 embedded compatibility, and v17 outer compatibility without weakening trailing-data rejection.
The WorldImport verifier proves the source chain with an actual `LegacyWorldMetadata` value.

Evidence is in
`Build/diagnostics/main-migration/task-8-world-surface-metadata/20260820-033834/`: both RED
captures, final WorldImport/Persistence/Wiring/loopback/PlayerPhysics/NPC outputs, MainBoundary
(500 files, 0 violations), and serial root Release (0 warnings, 0 errors) all correspond to the
final C# source tree.

This accepts source-backed world-surface preservation only. Type 226 `Wiring.DeActive` runtime
integration remains deferred until the actuator path has both this availability fact and the
remaining source-backed solid/not-really-solid tile-definition authority. Task 8 remains `进行中`.

## 82. Task 9 / WLD Header disposition matrix accepted

The v1-v87 and v88-v319 Header wire-order reads are recorded in
[`2026-08-20-wld-header-disposition-matrix.md`](../research/2026-08-20-wld-header-disposition-matrix.md).
The matrix assigns every audited field exactly one status and records the immutable owner or the
missing semantic prerequisite. It accepts only existing exact routes, keeps source absence explicit,
and blocks fractional time, old-layout random weather, ore tiers, orb/altar counters, invasion travel,
cloud presentation and unaudited later records.

Parser and WorldImport verifiers, MainBoundary (579 Simulation files, 0 violations), and the serial
root Release build (0 warnings, 0 errors) pass for the final source tree. The evidence card is
`Build/diagnostics/main-migration/task-9-wld-header-matrix/20260821-000017/`. This is an audit and
field-disposition card only; it does not import additional WLD state. The next card must establish
a version-boundary RED for exactly one identity field before changing runtime ownership.
