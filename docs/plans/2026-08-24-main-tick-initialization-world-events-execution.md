# Main Tick、初始化和世界事件执行计划

> **For Codex:** REQUIRED SUB-SKILL: Use `executing-plans` to implement this plan task-by-task.
> 只迁移有明确服务器责任、权威状态、命令/事件边界和可执行验证的 Main 行为。

**Goal:** 将 Version4 `Terraria/Main.cs` 的服务器 Tick、启动初始化、世界时钟、天气、进度和世界事件责任迁移为可确定重放的 Dome Simulation/Server 流程，并保留无法证明的旧主线程和延迟队列行为为显式 deferred。

**Architecture:** `Terraria.Dome.Simulation` owns immutable world metadata, clock, rules, progression, typed commands/events, deterministic phase ordering, and snapshots. `Terraria.Dome.Server` owns process startup, session lifecycle, persistence orchestration, and protocol fan-out. Protocol remains a projection adapter. 不引入聚合 `InitializeAlmostEverything`、任意 delegate queue 或旧 `Main` 全局。

**Tech Stack:** .NET 10、C#、Arch ECS、deterministic serial MSBuild、executable verification projects、immutable snapshots、V1456 compatibility projections。

---

## 1. 范围和完成门禁

### 1.1 Legacy source scope

事实源是 `D:\TRbackup\Version4物理删除了某些文件\Terraria\Main.cs`。Task 0 必须刷新 hash 和行号。

| Legacy region | Responsibility | Target owner |
| --- | --- | --- |
| `Main.cs:2179-2600` | server/world startup | `Terraria.Dome.Server` startup/bootstrap |
| `Main.cs:3656-3980` | entity/definition initialization | domain-owned registries/bootstrap steps |
| `Main.cs:11559-12064` | main update loop | explicit `SimulationTickSchedule` |
| `Main.cs:12458-12600` | weather/environment | `WorldWeatherSystem` and rule state |
| `Main.cs:12958-13132` | invasion/event update | progression/event systems |
| `Main.cs:13132-14600` | time/event transitions | clock, rate, progression systems |
| `Main.cs:11541-11577` | main-thread actions | caller-specific typed host commands or deferred |
| `Main.cs:242-244,11629-11739` | delayed `IEnumerator` lists | deferred until serializable caller contract exists |

客户端 renderer、UI、camera、audio、XNA、input UX、assets、social integrations 和 presentation-only events 不属于 Simulation 分母；必须标记 `ExcludedWithEvidence`，不能当作服务器欠账或完成项。

### 1.2 Completion definition

计划只有在以下条件全部满足时完成：

1. Main responsibility ledger 中每个 ServerRelevant 符号都有 owner，或有 source-backed `Deferred`/`ExcludedWithEvidence` 理由。
2. 正常 Tick 有冻结 phase trace；相同初始 snapshot 和输入序列产生相同 clock、rule、progression、entity 和 event snapshot。
3. 启动是确定性的、幂等的、可恢复的，不依赖静态 Main state。
4. Clock/weather/event 顺序在 pause、dawn、dusk、rate change、warning、completion、cancellation 和 same-tick suppression 边界有验证。
5. Persistence 恢复 clock/rule/progression/random state，不重复事件，也不在首个 Tick 前推进。
6. MainBoundary、相关 verifiers 和 serial Release build 通过并保存新证据。

这不是完整 Terraria event parity 声明。随机开始、NPC table-driven spawn、客户端 chat/sound/ambience、未解析 static tables、任意 `Action` caller 和未知 delayed process 继续 deferred。

## 2. 硬边界和共享契约

### 2.1 写集

| Area | Allowed write set |
| --- | --- |
| Simulation | `src/Terraria.Dome.Simulation/World`, `Tick`, domain systems/commands/events/snapshots |
| Server | `src/Terraria.Dome.Server/Startup`, `Sessions`, `Persistence`, `Replication` |
| Verification | `Test/Terraria.Dome.MainBoundary.Verification`, `TickOrder`, `WorldClock`, `WorldRules`, `World.Server` |
| Evidence | `Build/diagnostics/main-tick/<task>/<timestamp>/` |
| Docs | `docs/migrations`, `docs/research`, `docs/server-completion`, `progress.md` |

Task 0-11 不修改 Version4 reference，也不物理删除 Main 相关旧文件。

### 2.2 Tick contract

当前 schedule 是：

```text
BeginTick -> ApplyWorldClock -> ApplyPlayerInputs -> ApplyPlayerControl
-> ResolveTileCollision -> SelectNpcTargets -> ApplyNpcAi -> MoveEntities
-> AdvanceProjectiles -> ResolveCombat -> CommitDomainCommands
-> PublishSnapshot -> EndTick
```

新 phase 必须同时修改 `SimulationTickPhase`、`SimulationTickSchedule`、context assertions 和 TickOrder verifier。不能为了模仿 legacy loop 随意重排；未证明的关系保持 `partial`/`unknown`。

### 2.3 State/mutation contract

- Clock、`WorldRuleState`、`WorldProgressionState`、world metadata 和 event random stream 是 Simulation authority。
- 外部 input 只能形成 validated typed command，不能直接改变 state 或 event flag。
- System 读取 snapshot，输出 command/fact event；commit system 只应用一次 command。
- Event 是 committed fact，不替代 durable state；identity 至少含 `WorldTick`、`Sequence`、`Kind` 和稳定 domain identity。
- Server projection 只消费 immutable snapshot/event，不能在 projection 中回调 Simulation。

### 2.4 Initialization contract

`Initialize_AlmostEverything` 是 accounting entry point，不是目标类型。每个 child family 必须有一个 owner、一个 registration contract 和一个 verifier。不得添加 `Simulation.InitializeAlmostEverything`。

未恢复 caller identity、phase、cancellation、persistence、restart 语义的 `IEnumerator` 和 arbitrary `Action` 保持 deferred；不得用 generic coroutine/action queue 掩盖缺口。

## 3. 执行协议

每个 Task 固定执行：

1. 在 fresh evidence directory 记录 `git status`、`git diff --check`、source hash 和 resolved MSBuild properties。
2. 读取指定 Version4 anchor 和当前 Dome owner。
3. 增加 accepted/rejected executable cases；新行为先运行 RED。
4. 实现最小 authority path。
5. 运行 focused verifier、受影响 loopback verifier 和 serial Release build。
6. 运行 MainBoundary，检查 forbidden dependency 和 project references。
7. 记录 exit code、warnings/errors、artifact path 和剩余 deferred rows。
8. 只提交当前 Task 写集。

```powershell
git status --short
git diff --check
dotnet sln Terraria.Dome.sln list
dotnet msbuild src\Terraria.Dome.Simulation\Terraria.Dome.Simulation.csproj -nologo -getProperty:TargetFramework -getProperty:BaseOutputPath -getProperty:BaseIntermediateOutputPath -p:UseSharedCompilation=false
dotnet build Terraria.Dome.sln -c Release -p:UseSharedCompilation=false
```

## 4. 任务拆分

### Task 0：冻结 Main source baseline 和 responsibility ledger

**Files:** `docs/migrations/main-server-responsibility-ledger.md`、`docs/research/2026-08-20-main-tick-phase-coverage-matrix.md`、`Build/diagnostics/main-tick/task-0-baseline/<timestamp>/`。

**Steps:**

1. 记录 Main 和直接依赖的 size、line count、timestamp、SHA-256。
2. 刷新 `DoUpdate`、`UpdateTime`、`UpdateWeather`、`UpdateInvasion`、`UpdateServer`、`QueueMainThreadAction` 和 delayed-process anchors。
3. 将 server symbols 分类为 `ServerState`、`Definition`、`SimulationSystem`、`CommandInput`、`Snapshot`、`Projection`、`ExcludedClient` 或 `Unknown`。
4. 每个 unresolved row 记录下一项证据；缺失源码不能当作空实现。
5. 运行 MainBoundary，保存 baseline；本任务不修改生产代码。

**Acceptance:** ledger 和 phase matrix 使用同一 source hash，不作 Main 完成声明。

### Task 1：冻结启动输入和 bootstrap result

**Files:** `src/Terraria.Dome.Simulation/World/WorldMetadata.cs`、`WorldSeed.cs`、`Bootstrap/WorldBootstrapRequest.cs`、`Bootstrap/WorldBootstrapResult.cs`、`src/Terraria.Dome.Server/Startup/**`、`Test/Terraria.Dome.World.Server.Verification/**`。

**Steps:**

1. 写非法 dimensions、spawn 越界、unsupported difficulty、seed mismatch、duplicate bootstrap 和 bootstrap-after-restore 的 RED cases。
2. 实现 immutable bootstrap input：world identity、dimensions、seed、rules、spawn、generation status 和 entity limits。
3. 使相同 bootstrap 幂等，不重复注册 definitions/entities，也不推进 clock 或 event revision。
4. 将 Server socket/session startup 与 Simulation world bootstrap 分离。
5. 输出 accepted/rejected reason、initial snapshot revision 和 first scheduled Tick。

**Acceptance:** 无 Main static read；重复启动不重复初始化；非法输入不改变 authority state。

### Task 2：按领域实现 initialization registry slices

**Files:** `src/Terraria.Dome.Simulation/*/Definitions/**`、Simulation bootstrap composition、`docs/research/2026-08-22-main-initialize-almost-everything-disposition.md`、现有 Items/Npc/Combat/WorldObjects/Wiring focused verifiers。

**Steps:**

1. 每批只选择一个 bounded family：Tile、Item、NPC、Projectile、Wiring、Framing、Liquid 或 TileEntity。
2. 写 unknown-ID、duplicate-ID、invalid enum 和 registration-order RED cases。
3. 实现 domain-owned immutable registry 与显式 `RegisterDefaults`/builder contract。
4. 从 startup composition 调用 family registration，禁止 universal initializer。
5. 验证 registry identity/content 在 restart 中稳定，协议 input 不能修改 registry。
6. 缺少 defaults、consumer 或 persistence 证据的完整 static table 继续 deferred。

**Acceptance:** 每个 accepted family 有独立 owner/verifier；M-001 仍是 accounting item，不变成一个聚合实现。

### Task 3：使 Tick schedule 和 phase trace 成为权威契约

**Files:** `src/Terraria.Dome.Simulation/Tick/SimulationTickPhase.cs`、`SimulationTickSchedule.cs`、`SimulationTickContext.cs`、Simulation tick orchestration、`Test/Terraria.Dome.TickOrder.Verification/**`。

**Steps:**

1. 写 missing/duplicate/unexpected phase 和 paused Tick RED cases。
2. 冻结 named schedule；只有已有 state model 的 world event/rule phase 才能进入 schedule。
3. 输出含 Tick number、input sequence range、command/event counts 的 phase trace。
4. 断言 paused Tick 在 documented boundary 停止且不发布 gameplay mutation。
5. 更新 source-to-ECS phase matrix；不把未证明 legacy order 标为 accepted。
6. 验证相同 input trace 产生相同 phase trace 和 snapshots。

**Acceptance:** phase 顺序由测试强制，不依赖 reflection、目录枚举或类型名排序。

### Task 4：clock、rate、dawn/dusk 和 persistence

**Files:** `World/WorldClock.cs`、`WorldTimeRatePolicy.cs`、`World/Systems/WorldClockSystem.cs`、`Snapshots/DomeSimulationSnapshot.cs`、`Test/Terraria.Dome.WorldClock.Verification/**`、WorldRules verifier。

**Steps:**

1. 写 day/night boundary、full cycle、pause、rate、overflow、restore 和 incompatible-cycle RED cases。
2. 只从 Tick context 推进 clock，不读取 wall-clock。
3. 将 resolved time rate 固定为当前 Tick immutable input，供所有 time-dependent systems 使用。
4. 持久化 Tick number、time、day/night、moon phase、pause 和 rate。
5. 验证 restore 在首个 accepted Tick 前不推进、不重复 dawn/dusk event。

**Acceptance:** WorldClock 和 persistence verifier 有新鲜 exit-0 证据。

### Task 5：weather 和 environment state

**Files:** `World/WorldRuleState.cs`、`World/Systems/WorldWeatherSystem.cs`、`WorldEnvironmentTickSystem.cs`、rain/wind commands、WorldRules focused/loopback verifiers。

**Steps:**

1. 写 rain start/stop、raw WLD import、strength、wind bounds、pause、rate 和 Lantern Night same-tick suppression RED cases。
2. 从 immutable clock/rule/progression snapshots 计算 weather transition。
3. commit 后发布 `WorldEnvironmentChange` facts。
4. 没有 legacy random trace 的 historical random weather 和 client ambience 保持 deferred。
5. 验证 projection 只含 authoritative values，不冒充 client visual parity。

**Acceptance:** weather transition 可重放，并通过 WorldRules loopback；unsupported behavior 有明确 disposition。

### Task 6：progression state 和 event state machines

**Files:** `World/WorldProgressionState.cs`、`World/Systems/WorldProgressionSystem.cs`、`NormalEventEligibilitySystem.cs`、world event commands/events、WorldRules verifiers。

**Steps:**

1. 按 Main anchors 列出 Blood Moon、Eclipse、Lantern Night、Rain、Slime Rain、Invasion、Meteor 和 progression unlocks。
2. 每次只处理一个 family，写 eligibility/start/active/stop/cooldown/invalid/duplicate RED cases。
3. 用显式 transition table、WorldTick、revision、deterministic event sequence 实现。
4. commit 后发布 event facts；restore 不能重复事实事件。
5. NPC table-driven spawn、global random starts、full boss/event AI 和 client chat/sound 继续 deferred。
6. 更新 `docs/research/2026-08-22-entity-event-lifecycle-gap-matrix.md`。

**Acceptance:** 每个 accepted event 有 source anchor、authority、transition table 和 executable verifier。

### Task 7：invasion travel、warning、progress 和 clear ordering

**Files:** `World/Systems/WorldInvasion*.cs`、`World/WorldInvasion*.cs`、`World/Events/WorldInvasionCompletedEvent.cs`、WorldRules focused/loopback verifiers。

**Steps:**

1. 以 `Main.cs:12958-13033` 为 post-clock rate/travel ordering source anchor。
2. 写 start eligibility、town fallback、bounded size、delay、position clamp、warning、progress、clear 和 completion RED cases。
3. 保证 travel 消费 current Tick resolved rate，并在 ApplyWorldClock 后运行。
4. commit 后发布 progress/completion facts。
5. 验证每个 transition 的 restart 不重复 warning/completion、不丢 clear flags。
6. random invasion starts 和 NPC spawn-table selection 继续 deferred。

**Acceptance:** supported invasion chain deterministic、loopback verified；不宣称完整 NPC-driven invasion parity。

### Task 8：Meteor、Lantern Night、Slime Rain 等事件切片

**Files:** `World/Systems/WorldMeteor*.cs`、`World/Systems/WorldSlimeRain*.cs`、progression systems、world commands、WorldRules focused/loopback verifiers。

**Steps:**

1. 每批冻结一个 event family 的 Main source lines 和当前 owner。
2. 写 eligibility、schedule、cancel、day/night、overflow、restart RED cases。
3. 只实现 server-owned state transition；impact/tile mutation 通过 WorldGen/Tile commit owner。
4. 验证相对 ApplyWorldClock、CommitDomainCommands、PublishSnapshot 的顺序。
5. 没有 reproducible legacy random trace 时不得实现 random trigger。
6. client-only message、ambience、UI 记录为 projection/deferred。

**Acceptance:** event family 独立验收，不提供 aggregate “all events” 结论。

### Task 9：为 Main-thread actions 建立 typed replacement

**Files:** `docs/research/2026-08-23-main-thread-action-contracts.md`、caller-specific Server/Simulation owner、typed commands、caller verifier。

**Steps:**

1. 盘点 actual caller，确认 identity、phase、cancellation、retry、persistence、protocol visibility。
2. 对 arbitrary `Action`/delegate 输入增加 Simulation boundary RED test。
3. 只有 contract 完整的 caller 才实现一个 typed command，并由正确 domain/Server owner 持有。
4. 定义 deterministic sequence 和 exactly-once/retry 语义。
5. WorldGen section-manager follow-up 等 host/client caller 若证据不足则保持 deferred。

**Acceptance:** Simulation 没有 generic main-thread action queue；每个 accepted replacement typed、replayable、caller-specific。

### Task 10：delayed process contract 决策

**Files:** `docs/research/2026-08-23-delayed-process-contract-matrix.md`、Main responsibility ledger、caller-specific verifier if recovered。

**Steps:**

1. 搜索 retained source/runtime artifact 中两个 delayed collections 的实际 add callers。
2. 若 caller identity、phase、cancellation、restart state 无法恢复，保持两个集合 deferred。
3. 只有 caller 完整时才实现 named serializable state machine，禁止 `IEnumerator` storage。
4. 生产 queue 之前必须有 persistence/replay tests。

**Acceptance:** 缺失 contract 继续记录为 deferred，不新增 generic coroutine abstraction。

### Task 11：Server host、persistence 和 protocol timing boundary

**Files:** `src/Terraria.Dome.Server/Startup/**`、`Sessions/**`、`Persistence/**`、`src/Terraria.Dome.Transport/**`、World.Server、FullClientBootstrap、NetworkIsolation verifiers。

**Steps:**

1. 冻结顺序：load/restore -> bootstrap definitions -> create Simulation -> attach sessions -> Tick -> publish snapshot。
2. 验证 socket accept、protocol decode、outbound flush 不会推进 Simulation。
3. 持久化 clock/rules/progression/event random state/pending typed commands，并验证边界。
4. reconnect 获取 coherent snapshot，不重复已提交 event facts。
5. protocol 只编码 authority values；unsupported event message reject 或 compatibility-project，不修改 Simulation。

**Acceptance:** host timing 是 adapter boundary；fresh FullClientBootstrap 和 NetworkIsolation evidence 通过。

### Task 12：全量 gate、ledger refresh 和 review

**Files:** Main ledger、tick phase matrix、entity/event gap matrix、`docs/server-completion/completion-manifest.json`、`progress.md`、`Build/diagnostics/main-tick/task-12-gate/<timestamp>/`。

**Steps:**

1. 重跑 source inventory；每个 status change 必须有 anchor、owner、verifier 和 evidence path。
2. 运行 MainBoundary、TickOrder、WorldClock、WorldRules、World.Server、persistence、bootstrap 和 loopback verifiers。
3. 运行 `dotnet build Terraria.Dome.sln -c Release -p:UseSharedCompilation=false`。
4. 运行 forbidden dependency scan，检查 resolved project references。
5. 运行 fresh persistence/restart 和 full-client bootstrap checks。
6. 重新核对物理删除 ledger；不要在报告间复制过时的 57/44 数字。
7. 只将满足四列证据的行标记 `Complete`/`ReplacedWithEvidence`；random/client/unknown initializer/arbitrary action/delayed process 保持 deferred。

**Acceptance:** 所有命令 exit 0 且记录 warnings/errors；没有基于行数、文件数或同名方法的 Main 百分比声明。

## 5. 验证矩阵和命令

| Claim | Required evidence |
| --- | --- |
| Tick order | `Terraria.Dome.TickOrder.Verification` exit 0 + phase trace |
| Main boundary | `Terraria.Dome.MainBoundary.Verification` exit 0 + zero forbidden source violations |
| Bootstrap | `Terraria.Dome.World.Server.Verification` exit 0 + duplicate/malformed cases |
| Clock | `Terraria.Dome.WorldClock.Verification` exit 0 + exact boundary/restore cases |
| Rules/events | WorldRules unit and loopback exit 0 |
| Host/protocol | FullClientBootstrap and NetworkIsolation evidence |
| Build | serial root Release exit 0 + warning/error counts |

```powershell
dotnet run --project Test\Terraria.Dome.MainBoundary.Verification\Terraria.Dome.MainBoundary.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.TickOrder.Verification\Terraria.Dome.TickOrder.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.WorldClock.Verification\Terraria.Dome.WorldClock.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.WorldRules.Verification\Terraria.Dome.WorldRules.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.WorldRules.Loopback.Verification\Terraria.Dome.WorldRules.Loopback.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet run --project Test\Terraria.Dome.World.Server.Verification\Terraria.Dome.World.Server.Verification.csproj -c Release -p:UseSharedCompilation=false
dotnet build Terraria.Dome.sln -c Release -p:UseSharedCompilation=false
```

Changed source must be built before using `--no-build`. All output stays under `Build/`.

## 6. Status、并发边界和回滚

### 6.1 Status values

`Unmapped` = no owner；`Mapped` = target exists without proof；`Partial` = bounded proof；`Complete` = reference/authority/projection/execution all present；`ReplacedWithEvidence` = named replacement with proof；`Deferred` = missing contract/evidence；`ExcludedWithEvidence` = outside server denominator。

### 6.2 Parallel write sets

- Tick owner：`Simulation/Tick/**` 和 TickOrder verifier。
- World-state owner：`Simulation/World/**` 和 clock/rule verifiers。
- Startup owner：`Server/Startup/**` 和 World.Server verifier。
- Event owner：每次只拥有一个 `World/Systems/World*` family 及其 verifier。
- Ledger owner：证据产生后才修改 `docs/migrations/**`、`docs/research/**`、`progress.md`。

先冻结 phase enum、snapshot revision、command sequence 和 event identity；worker 不得改写或回滚其他 owner 的文件。

只回滚当前 Task 文件和当前产生的临时 evidence；保留历史 diagnostics、其他迁移变更和 Version4 baseline。禁止 broad cleanup、`git reset --hard` 和物理删除。

## 7. 最终交付物

- refreshed `docs/migrations/main-server-responsibility-ledger.md`；
- refreshed `docs/research/2026-08-20-main-tick-phase-coverage-matrix.md`；
- initializer/event qualification cards；
- current delayed-process and main-thread-action contract cards；
- `Build/diagnostics/main-tick/<task>/<timestamp>/` artifacts；
- updated `docs/server-completion/completion-manifest.json` and `progress.md`；
- verifier exit codes、build warning/error counts 和 remaining deferred rows。

最终报告必须列出已覆盖的具体 server responsibilities 和 remaining deferred families，不得给出一个基于代码量的 Main completion percentage。
