# Fuck My Shit Mountain Audit Report

**Project:** NLTX Terraria/Version4 migration project
**Audit mode:** full
**Date:** 2026-09-15
**Reviewer:** Codex GPT-5

---

## 1. Executive Summary

本报告评估当前仓库 `D:\TRbackup\NLTX`，并以 `D:\ProjectItem\SourceCode` 中的既有项目作为能力基线。结论不是“缺少编程能力”：既有项目显示你已经具备复杂系统拆分、静态分析工具链、测试分层、嵌入式实时调度和文档治理能力。当前真正的学习缺口是多人游戏服务器的运行时语义，以及如何用端到端行为证据证明迁移没有改变服务器权威状态。

当前迁移工程已经建立了较完整的领域组件、身份投影、迁移账本、工作项和 focused verifier 体系；`DomeSimulation.Tick` 也已经显式表达了世界时钟、玩家输入、碰撞、NPC、Projectile、战斗、命令提交和快照发布的阶段顺序。但账本中的 506 条决策全部仍为 `verificationStatus: not-run`，76 条 `migrated` 不能等同于 76 条已验证的运行时替换；今天抽查的服务器主干还无法编译。因此，项目目前适合继续做“可运行垂直切片”，不适合宣称迁移完成或进入稳定发布。

最高优先级是：先补权威模拟/tick/实体生命周期，再补网络复制与客户端不可信边界；随后建立 ECS 查询和性能预算、持久化恢复、故障可观测性与服务器安全知识。最有效的学习方式不是继续横向增加组件，而是完成一条 `Spawn -> Tick -> Replicate -> Disconnect -> Restore` 闭环，并让它产生真实的构建、运行和重放证据。

### Score Dashboard

```
Security        ██████░░░░  6.0  B   已有协议边界和密码字段隔离，但监听地址、认证/暴露配置和生产安全边界尚未形成可发布证据。
Stability       ████░░░░░░  4.0  C   服务器主干当前无法编译，且模拟循环异常会退出而不让入口进程感知。
Performance     █████░░░░░  5.0  B   有固定阶段与命令上限意识，但 16ms 延迟循环、尾延迟、分配和实体规模预算尚未验证。
Testing         █████░░░░░  5.0  B   loopback verifier 质量较好，但账本验证为 0，且关键项目存在构建阻断。
Maintainability ██████░░░░  6.0  B   领域拆分和文档治理很强，但三棵代码树、超大核心类和工程引用漂移增加变更成本。
Design          ██████░░░░  6.0  B   Registry/Adapter、typed identity 和显式 tick 阶段方向正确，运行时权威边界仍需闭环证明。
Release         ███░░░░░░░  3.0  C   未形成稳定的主干编译、统一 CI 入口、健康状态与故障/回滚证据；发布不可接受。
─────────────────────────────────────
Overall         █████░░░░░  5.0  B   设计和工程治理有基础，但当前仍是迁移开发态，稳定发布风险显著。
```

每个维度按 0.0–10.0 评分，10 分最好；评分是结合证据的判断，不是按问题数量机械扣分。由于前端、AI/LLM 等表面不适用于当前服务器迁移，相关维度在覆盖矩阵中标记为未评估，并不影响总体判断。

### Finding Statistics

| Severity | Count | Confirmed | Suspected |
|----------|-------|-----------|-----------|
| Critical | 0 | 0 | 0 |
| High | 3 | 3 | 0 |
| Medium | 4 | 4 | 0 |
| Low | 1 | 1 | 0 |
| Info | 2 | 2 | 0 |
| **Total** | **10** | **10** | **0** |

## 2. Project Map

当前仓库同时包含三类主要代码面：根目录 `src/` 的权威 ECS/领域实现，`src2/` 的非权威或新边界原型，以及 `dome/dome1/` 下包含 Simulation、Server、Protocol、Transport、WorldFile 和大量验证器的可运行服务器试验场。`DomeServer` 负责 TCP 接入、会话生命周期、协议命令排队、模拟循环和各类复制；`DomeSimulation` 持有世界与实体状态，并通过显式 tick 阶段推进状态、提交命令和生成快照。持久化通过 `DomeSimulationSnapshot`、`DomeStatePersistenceFormat` 及若干 sidecar coordinator 保存世界、玩家和部分 NPC 状态。

典型数据流是：TCP frame -> `TerrariaProtocolSessionHost` 解码/分发 -> inbound envelope 或协议命令队列 -> `DomeServer.SimulationLoopAsync` 每 tick 处理 -> `DomeSimulation.Tick` 计算并提交状态 -> `LatestSnapshot` 与各复制 assembler 写回客户端。安全边界主要位于外部 TCP frame、玩家输入、tile/entity 操作、玩家身份 bootstrap 和配置；当前需要特别区分协议 slot、运行时 EntityUuid、ReplicationId、AccountUuid 与持久化 ID。

旧项目集合覆盖 `Net/TerrariaTools/Isolation`、`Net/NL`、`DesktopPet`、`Kernel`、`SmartWatch`、`MCUGUI` 和 `Python/DataCrawler` 等。它们证明了静态分析、领域建模、测试治理、FreeRTOS/硬件节拍和跨层调试能力；它们没有直接证明多人服务器的权威模拟、复制一致性、反作弊、重连和恢复语义。

### Coverage Matrix

| Dimension | Coverage | Evidence inspected | Exclusions / limits |
|-----------|----------|--------------------|---------------------|
| Architecture | High | `src/`、`src2/`、`dome/dome1/src` 工程引用、DomeServer/Simulation、迁移 PRD/ADR、账本 | 未对每个 2,700+ C# 文件逐类审阅；构建漂移限制了集成结论 |
| Security | Medium | TCP/session/protocol/config/password/host token 搜索，frame 解码与输入入口 | 未进行外部端口扫描、模糊测试、真实部署渗透和密码学审查 |
| Stability | High | 服务器生命周期、异常捕获、Dispose、simulation loop、loopback verifier、5 个定向构建 | 未运行完整服务器长时间故障注入 |
| Performance | Medium | tick 阶段、命令上限、复制路径、Task.Delay、旧项目性能测试结构 | 未采集 P99 tick、GC、带宽和最大实体数实测 |
| Testing | High | 111 个 dome 验证工程清单、loopback verifier、账本 verificationStatus、定向构建/运行 | 未运行全部 verifier；部分历史 verifier 文件在工作树删除态 |
| Maintainability | High | 三棵代码树规模、工程引用、DomeSimulation/DomeServer、文档和旧项目结构 | 未进行完整静态复杂度扫描或逐文件重复率统计 |
| Design | High | ECS 约束、组件命名约束、typed identity ADR、tick schedule、Registry/Adapter 设计 | 行为等价和边界压测仍未闭合 |
| Release | Medium | `global.json`、AGENTS build contract、Build wrapper、产物路径、仓库 CI 入口检索 | 未检查外部部署脚本、托管平台和真实回滚演练 |
| Documentation | High | `Context/progress.md`、CONTEXT、PRD/ADR、迁移账本、第二轮审查、旧项目 DDD 文档 | 历史文档存在口径漂移，未逐份校正文档 |
| Configuration | Medium | ServerLaunchOptions、ServerHostState、NetworkSessionConfigurationInput、Start 配置路径 | 未验证所有生产配置来源和环境覆盖 |
| Observability | Medium | `SimulationFault`、`LastSessionFault`、tick trace、现有日志与 verifier 输出搜索 | 未见统一指标、健康端点、告警和运行手册的完整闭环 |
| Data Integrity | Medium | snapshot/persistence format、sidecar save coordinator、AccountUuid/EntityUuid 设计 | 未做写入中断、磁盘损坏、版本迁移和恢复引用故障注入 |
| Privacy | Low | 玩家 AccountUuid、profile、密码/host token 字段和持久化路径 | 未进行法规适用性、留存、删除、导出和日志脱敏的全面审查 |
| Accessibility | Not assessed | 当前仓库无适用的终端 UI 产品表面 | 服务器迁移不适用；客户端表现层仅做存在性识别 |
| Supply Chain | Low | `global.json`、PackageReference、Build/packages、仓库状态 | 未完成依赖漏洞、签名、SBOM、CI 权限和制品来源审计 |
| Cost | Medium | 实体/命令/复制规模结构、旧项目性能预算测试、服务器循环 | 未有运行时容量、云资源、网络流量和日志成本数据 |
| AI Safety | Not assessed | 未发现当前迁移运行时的模型调用、提示词或 RAG 工具链 | AI/LLM 文件名信号来自仓库辅助工具/缓存，不属于服务器运行时边界 |
| Fallback | Medium | 服务器 catch、协议异常处理、持久化 recovery result 搜索 | 未对所有 defensive branch 做自动分类和逐分支行为测试 |
| Testing Authenticity | Medium | loopback 与 focused verifier 结构、测试输出、账本状态 | 未运行完整测试矩阵，无法判定所有绿灯的真实性 |
| Type Safety | Medium | typed identity、frame length、nullable C#、Registry/command 类型边界 | 未运行完整 analyzer；未逐项检查所有 stringly protocol 字段 |
| Frontend State | Not assessed | 当前目标是服务器和迁移内核，无前端组件运行面 | 不适用 |
| Backend API | Medium | TCP protocol/session host、命令队列、输入验证和错误路径 | 不是 HTTP API；未完成外部协议兼容性全量测试 |
| Dependency Weight | Low | 主要 `.csproj` 的 PackageReference/ProjectReference 和 Build/packages | 未测包体积、传递依赖和发布裁剪 |
| Code Consistency | Medium | C# 工程文件、命名/目录约束、服务器异常与验证器模式 | 工作树大且混合历史迁移状态，未做全量风格扫描 |
| Comment Coverage | Medium | AGENTS/设计文档/服务器关键注释和旧项目 docs-as-code | 未对 public API XML docs 和注释新鲜度做全量统计 |

## 3. Top Risks

1. **[High] 模拟循环故障后静默停止**：`SimulationLoopAsync` 捕获异常后只写入 `SimulationFault` 并退出，`Program` 仅等待 Ctrl+C，可能留下仍监听端口但不再推进 tick 的假活服务。
2. **[High] 服务器主干工程引用已漂移，无法编译**：`Terraria.Dome.Simulation.csproj` 指向不存在的 `dome/src/Share/Entity` 路径，导致 Dome Server 构建产生 203 个级联错误。
3. **[High] 迁移完成度没有运行时证据**：506 条账本决策全部 `not-run`，76 条 `migrated` 不能证明行为等价；当前主干也没有稳定集成验证基础。
4. **[Medium] tick 调度使用固定 16ms 延迟而非 deadline/单调时钟**：负载超过 16ms 时会累积漂移并隐藏 tick budget 超时，影响战斗和复制时序。
5. **[Medium] 认证与网络暴露配置未形成发布边界证据**：服务器固定绑定 loopback，配置模型包含密码/host token，但没有证明生产监听地址、认证强度和部署安全配置已闭合。
6. **[Medium] 持久化有格式和 sidecar，但缺少故障一致性实证**：当前能读写 snapshot，却未证明中断写入、版本迁移、重复恢复和引用修复在异常路径下安全。
7. **[Medium] 大型核心类和三棵代码树提高变更回归风险**：`DomeSimulation.cs` 约 9,581 行，`DomeServer.cs` 约 2,345 行，`src`/`src2`/`dome/dome1` 的集成边界尚未稳定。
8. **[Low] 验证入口和历史状态口径仍有漂移**：旧审查已记录 `completed`、`in-progress`、`failed`、`not-run` 等文档/账本不一致，降低迁移队列的可读性。

## 4. Detailed Findings

### Finding: 模拟循环异常会造成假活服务

- Severity: High
- Confidence: High
- Category: Stability
- Status: Confirmed
- Affected area: `dome/dome1/src/Terraria.Dome.Server` 服务器生命周期
- Evidence:
  - File: `dome/dome1/src/Terraria.Dome.Server/DomeServer.cs:1034-1086`
  - Function / Module: `SimulationLoopAsync`
  - Relevant behavior: 模拟循环捕获通用 `Exception` 后只设置 `SimulationFault = exception`，随后任务结束；没有重新抛出、取消主机或触发入口进程退出。
  - File: `dome/dome1/src/Terraria.Dome.Server/Program.cs:18-37`
  - Function / Module: `Program` top-level entry point
  - Relevant behavior: 入口在 `shutdown.Wait()` 上只等待 Ctrl+C，没有观察 `server.SimulationFault` 或模拟任务完成状态。
- Problem: 任何未预期的模拟异常都可能终止唯一的 simulation task，而 TCP listener 和进程仍然存在。外部探针看到端口可连接，但玩家输入不再推进，形成比进程直接退出更难诊断的故障状态。
- Why it matters: 服务器权威状态停止推进时，客户端会持续接收不到正常快照；会话、重连和持久化行为也可能停在半完成状态。对游戏服务器而言，liveness 不能只等于端口仍打开。
- Realistic failure scenario: 某个实体状态或世界命令触发未捕获异常 -> `SimulationLoopAsync` 写入 `SimulationFault` 并结束 -> `AcceptLoopAsync` 仍可接受新连接 -> 新连接等待 world/player 状态 -> 运维只看到进程和端口在线。
- Minimal fix: 将 simulation task 完成/故障接入 host lifecycle，故障时取消服务器 token、停止 listener，并让 `Program` 返回非零退出码；同时提供可轮询的 readiness/liveness 状态。
- Better long-term fix: 将 simulation loop 包装成显式 supervisor，使用单调时钟、故障分类、崩溃快照和结构化 health state；区分 session fault、recoverable command rejection 与 fatal simulation fault。
- Regression test suggestion: 注入一个只在指定 tick 抛出的 fake simulation fault，断言 `SimulationFault` 可见、listener 停止、主入口任务完成且退出状态非零；另测普通 session fault 不会停止全局模拟。
- Estimated effort: 1-2 days

### Finding: Dome Server 主干工程引用漂移导致无法编译

- Severity: High
- Confidence: High
- Category: Release
- Status: Confirmed
- Affected area: `dome/dome1/src/Terraria.Dome.Simulation/Terraria.Dome.Simulation.csproj`
- Evidence:
  - File: `dome/dome1/src/Terraria.Dome.Simulation/Terraria.Dome.Simulation.csproj:8`
  - Function / Module: MSBuild project reference
  - Relevant behavior: `ProjectReference` 使用 `..\..\..\src\Share\Entity\Terraria.EntityEcs.csproj`，从 `dome/dome1/src` 解析为不存在的 `dome/src/Share/Entity`。
  - Test: 串行构建 `dome/dome1/src/Terraria.Dome.Server/Terraria.Dome.Server.csproj` 退出码 1，1 warning、203 errors；未生成服务器 DLL。
- Problem: 服务器工程不能通过基础编译，后续服务端 loopback 验证无法建立可信的当前产物。大量 `EntityEcs`、`LocationComponent`、`VelocityComponent` 等错误是同一引用错误的级联结果。
- Why it matters: 没有可复现的主干构建，就无法把设计、测试源码和实际运行时连接起来；任何“迁移已工作”的判断都只能停留在局部或历史产物层面。
- Realistic failure scenario: 新开发者按当前工程入口构建服务器 -> MSBuild 先找不到 EntityEcs project -> 编译器报告数百个类型缺失 -> 无法运行 loopback 或发布服务器。
- Minimal fix: 根据当前仓库实际所有权修正 `ProjectReference` 到根 `src/Share/Entity/Terraria.EntityEcs.csproj`，或将 dome 所需共享工程以明确的迁移路径纳入当前工程；随后重新串行构建受影响项目。
- Better long-term fix: 统一解决方案/项目根目录和迁移目录布局，增加 project-reference path verifier，并让 CI 在任何 focused verifier 前先构建其真实依赖闭包。
- Regression test suggestion: 增加工程引用路径验证器，加载所有 dome csproj 并解析每个 `ProjectReference`；CI 中串行 build Server，断言 `Build/bin/.../Terraria.Dome.Server.dll` 存在。
- Estimated effort: 2-4 hours for path repair; 1 day for guard verifier

### Finding: 迁移账本的“migrated”尚未转化为行为等价证据

- Severity: High
- Confidence: High
- Category: Testing
- Status: Confirmed
- Affected area: Version4 member migration ledger and verification workflow
- Evidence:
  - File: `docs/migration/ledgers/Version4-member-migration-map.json:1-20` and ledger aggregates
  - Function / Module: `decisions` / `verificationStatus` / `workItems`
  - Relevant behavior: 506 条 decision；430 条 `deferred`、76 条 `migrated`；506 条 `verificationStatus` 全部为 `not-run`；64 条 work item 全为 `ready`。
  - File: `docs/component-decomposition/review-round-2/2026-09-13-version4-组件实现情况审计报告.md:232-236`
  - Function / Module: 第二轮账本审计摘要
  - Relevant behavior: 明确记录 `migrated / verified = 76 / 0`，并说明 audit 只证明账本结构一致，不证明行为。
- Problem: 当前状态机把结构处置、源码存在和运行时行为验证分成了不同字段，但项目推进信号仍容易被 `migrated` 数量主导。没有至少一条稳定的端到端 verified 垂直切片，无法判断旧行为是否被新系统保留。
- Why it matters: 迁移最危险的错误通常不是类型缺失，而是 tick 顺序、实体生命周期、网络可见性、槽位复用、持久化恢复或边界拒绝发生细微偏差；静态映射和账本审计捕获不了这些问题。
- Realistic failure scenario: 某成员被标为 migrated -> 目录和类型检查通过 -> 玩家断线重连或 projectile 槽位复用时出现旧实体状态泄漏 -> 项目直到真实联机才发现行为不等价。
- Minimal fix: 暂停扩大横向迁移，选择 Player + 一个 Projectile 建立 `Spawn -> Tick -> Replicate -> Disconnect -> Restore`，并把构建、运行、重放和状态断言结果写回 ledger 的 verified evidence。
- Better long-term fix: 为每个迁移单元规定“结构、编译、focused behavior、loopback、replay、persistence”六层证据门槛，并让 `migrated` 不能自动进入完成统计。
- Regression test suggestion: 双客户端 loopback 场景覆盖输入、Projectile 命中、销毁、断线、重连和全量重同步；以 tick/state hash 和实体身份映射做断言，成功后才更新一条 decision 为 verified。
- Estimated effort: 3-5 days for first vertical slice; ongoing per domain

### Finding: 固定 16ms 延迟不能提供稳定 tick deadline

- Severity: Medium
- Confidence: High
- Category: Performance
- Status: Confirmed
- Affected area: `DomeServer` simulation scheduling
- Evidence:
  - File: `dome/dome1/src/Terraria.Dome.Server/DomeServer.cs:1034-1077`
  - Function / Module: `SimulationLoopAsync`
  - Relevant behavior: 完成输入处理、`_simulation.Tick`、多组 replication flush 后统一执行 `await Task.Delay(TimeSpan.FromMilliseconds(16), cancellationToken)`。
- Problem: 该循环按“工作时间 + 16ms”推进，而不是按单调时钟计算下一次 tick deadline。只要一次 tick 和复制超过预算，实际 tick 周期就会变长并产生漂移；代码也没有记录超预算次数或跳 tick/追赶策略。
- Why it matters: 服务器时间、输入延迟、Projectile 碰撞与复制节奏会随负载变化；平均运行正常不能证明高实体数和多连接下的尾延迟可接受。
- Realistic failure scenario: 某 tick 的世界 section 复制耗时 30ms -> 再等待 16ms -> 周期约 46ms -> 后续输入和战斗相对客户端延迟增加，负载持续时服务器 tick rate 下降。
- Minimal fix: 使用 `Stopwatch.GetTimestamp()` 或 `PeriodicTimer` 建立固定 deadline，记录每 tick 的耗时和超预算；明确超预算时是合并输入、限制复制还是进入 degraded mode。
- Better long-term fix: 将 simulation 与网络复制预算分开，建立 tick budget policy、P50/P95/P99 指标和可重放压力场景；必要时将非权威复制工作移出 tick 临界路径。
- Regression test suggestion: fake clock 下运行 1,000 tick，注入 5/20/40ms 工作时间，断言 tick number、deadline 漂移、超预算计数和 degraded policy 符合约定。
- Estimated effort: 1-3 days

### Finding: 服务器网络暴露与认证配置尚未形成发布级边界

- Severity: Medium
- Confidence: Medium
- Category: Security
- Status: Confirmed
- Affected area: server listener and protocol session configuration
- Evidence:
  - File: `dome/dome1/src/Terraria.Dome.Server/DomeServer.cs:772-779`
  - Function / Module: `DomeServer.Start`
  - Relevant behavior: listener 固定使用 `IPAddress.Loopback`，没有使用 launch options 中的可配置监听地址。
  - File: `src2/Network/Session/NetworkSessionConfigurationInput.cs:3-15` and `src2/NetworkProtocolSessionVerification/Program.cs:128-145`
  - Function / Module: network configuration contract/verifier
  - Relevant behavior: 配置模型包含 `ServerPassword`、`HostToken`、`ServerIp`、`UseUpnp` 等敏感或暴露相关字段；verifier 使用测试密码值，但当前没有证明这些字段已接入实际 server listener/auth policy。
  - File: `dome/dome1/src/Terraria.Dome.Protocol.V1456/LegacyReference/MessageBuffer.cs:193-205,1740`
  - Function / Module: legacy handshake/password path
  - Relevant behavior: 协议存在 password-required 和密码比较路径。
- Problem: 当前代码同时存在配置层、legacy 协议认证语义和固定 loopback listener，但它们的生产边界没有统一证据。loopback 对本地验证安全，却不能证明未来绑定公网/局域网时的身份验证、暴露控制和 secret 生命周期。
- Why it matters: 错误的监听地址或默认配置可能造成服务不可达，也可能在以后开放监听时把弱认证协议直接暴露给不可信客户端；密码和 host token 还必须避免进入快照、日志和错误输出。
- Realistic failure scenario: 运维以为配置的 ServerIp 会生效 -> 实际只监听 loopback，远程玩家无法连接；或修复监听地址后未同步强制认证和限流 -> 未授权客户端可持续消耗 session/command 资源。
- Minimal fix: 明确 listener bind address 的来源和默认值；启动时拒绝不安全的公网配置组合；补充 password/host-token 不出现在 snapshot、日志和 exception 的断言。
- Better long-term fix: 将 transport bind、authentication policy、authorization/host privilege、rate limit 和 secret provider 分成显式端口，并为公网部署建立 threat model 与配置验证。
- Regression test suggestion: 配置矩阵测试 loopback/指定地址/无密码/有密码/host token；用未认证、错误密码、重复握手和超时客户端验证拒绝、限流和日志脱敏。
- Estimated effort: 2-4 days

### Finding: 持久化写入和恢复路径缺少故障一致性证明

- Severity: Medium
- Confidence: Medium
- Category: Data Integrity
- Status: Confirmed
- Affected area: server snapshot persistence and player/NPC sidecars
- Evidence:
  - File: `dome/dome1/src/Terraria.Dome.Server/Persistence/DomeStateSaveCoordinator.cs:7-69`
  - Function / Module: `DomeStateSaveCoordinator.Save/TryLoad`
  - Relevant behavior: 负责文件写入与读取，并返回 recovery result；当前审计未找到写入中途终止、临时文件替换、校验和/备份策略的运行证据。
  - File: `dome/dome1/src/Terraria.Dome.Server/DomeServer.cs:690-768`
  - Function / Module: `DomeServer.Dispose`
  - Relevant behavior: 关闭时等待任务并随后保存 NPC projectile cursor、NPC given name sidecar；多个保存操作按顺序执行，异常处理和原子性未形成统一发布契约。
  - File: `dome/dome1/src/Terraria.Dome.Server/Persistence/DomeStatePersistenceFormat.cs:101-115,709-805`
  - Function / Module: persistence format readers
  - Relevant behavior: 依格式版本读取 world/player/chest/sign/tile entity 等状态，并对旧版本采用条件分支。
- Problem: 存在格式版本和恢复模型，但尚未证明主快照与 sidecar 在部分写入、重复启动、旧版本升级和引用失效时保持一致；“能读回一个文件”不足以证明崩溃恢复安全。
- Why it matters: 服务器崩溃或机器断电是正常运行风险。恢复不一致可能造成玩家物品、世界对象、NPC 身份或引用丢失，且难以从单次线上现象复现。
- Realistic failure scenario: Dispose 保存主快照后在 sidecar 写入中断 -> 下次启动主状态和 cursor/name sidecar 来自不同 tick -> 恢复实体引用或复制状态出现错配。
- Minimal fix: 写入临时文件并原子替换，保存统一 generation/tick/checksum；启动时验证所有 sidecar generation，失败则显式进入 recovery/backup 路径而不是静默接受。
- Better long-term fix: 定义 snapshot manifest、版本迁移器、备份保留、引用修复策略和故障注入矩阵；区分运行时 EntityUuid 与跨重启持久化身份。
- Regression test suggestion: 在每个写入阶段注入中断，重启并断言选择上一份完整 generation；覆盖旧 format、重复恢复、未知引用和新的运行时 UUID。
- Estimated effort: 3-5 days

### Finding: Dome 核心类与多代码树边界造成高回归半径

- Severity: Medium
- Confidence: High
- Category: Maintainability
- Status: Confirmed
- Affected area: `dome/dome1/src/Terraria.Dome.Simulation` and repository integration layout
- Evidence:
  - File: `dome/dome1/src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs:5002-5194`
  - Function / Module: `DomeSimulation.Tick`
  - Relevant behavior: 一个核心类串联世界时钟、玩家输入/控制、tile collision、NPC pipeline、Projectile、combat、wiring/liquid、命令提交和 snapshot 发布。
  - File: `dome/dome1/src/Terraria.Dome.Simulation/Simulation/DomeSimulation.cs`
  - Function / Module: entire simulation module
  - Relevant behavior: 文件约 9,581 行；服务器文件约 2,345 行；仓库同时维护 `src`、`src2` 与 `dome/dome1` 三个主要实现面。
- Problem: 当前已有阶段化边界，但核心类仍承载大量 orchestration 与状态访问；三棵代码树又使类型、项目引用和行为状态容易漂移。规模本身不是 bug，但已经在本次编译失败和历史文档口径漂移中表现为实际维护风险。
- Why it matters: 一个领域修改可能触发网络、持久化和 verifier 的大范围回归；新贡献者也难以判断哪棵树是权威实现，导致重复实现或错误引用。
- Realistic failure scenario: 新增一个 Player 状态 -> 同时修改 root src 与 dome implementation -> verifier 引用另一棵树 -> 局部构建通过但实际 server 使用旧类型或无法解析 project reference。
- Minimal fix: 固定每个可运行 slice 的唯一 source-of-truth 和依赖闭包；将 Tick orchestration 仅保留阶段编排，按已有边界逐步提取 world/player/combat/replication adapters。
- Better long-term fix: 为 `src`/`src2`/`dome` 定义生命周期与迁移关系，建立统一 solution graph 和 architecture test，禁止未声明的跨树 ProjectReference。
- Regression test suggestion: 工程图测试断言每个 server verifier 只引用指定 source-of-truth；对 Player/Projectile vertical slice 做变更影响构建和 replay regression。
- Estimated effort: 2-5 days for boundary/documentation guard; weeks for gradual extraction

### Finding: 迁移状态的历史文档与账本口径存在漂移

- Severity: Low
- Confidence: High
- Category: Documentation
- Status: Confirmed
- Affected area: migration reports, ledger summaries, partition execution records
- Evidence:
  - File: `docs/component-decomposition/review-round-2/2026-09-13-version4-组件实现情况审计报告.md:146-149,159-178,232-236`
  - Function / Module: second-round audit and ledger summary
  - Relevant behavior: 记录多个 partition 的 `completed`、`in-progress`、`failed`、`partial`、`not-run` 之间的字段或文档冲突，同时 ledger aggregate 为 506 条 `not-run`。
- Problem: 历史执行记录、组件实现状态和统一账本有不同时间点或不同口径，阅读者需要人工判断哪一个字段代表当前事实。
- Why it matters: 状态口径不清会影响工作项选择、验证优先级和完成度汇报，尤其容易把“源码存在”误报为“行为已验证”。
- Realistic failure scenario: 下一批次按旧报告的 completedComponents 选取任务 -> 账本仍显示 deferred/not-run -> 重复实现或跳过真正缺口。
- Minimal fix: 为文档声明 snapshot date/source-of-truth，并用脚本生成汇总；所有 migrated/verified 统计只从 ledger 计算。
- Better long-term fix: 将 partition report、verifier result 和 ledger revision 通过唯一 run ID 关联，废止手工复制的终态摘要。
- Regression test suggestion: 文档/账本一致性检查，随机选取 partition 验证状态、组件路径和 ledger aggregate 必须可解释；状态变更必须带 revision 和证据链接。
- Estimated effort: 1-2 days

### Finding: 当前验证体系有高价值 loopback，但缺少统一可执行入口

- Severity: Info
- Confidence: High
- Category: Testing
- Status: Confirmed
- Affected area: dome verification projects and repository build workflow
- Evidence:
  - File: `dome/dome1/Test/Terraria.Dome.Combat.Loopback.Verification/Program.cs`, `dome/dome1/Test/Terraria.Dome.SessionReplication.Verification/Program.cs`, `dome/dome1/Test/Terraria.Dome.Hardening.Loopback.Verification/Program.cs`
  - Function / Module: loopback verification programs
  - Relevant behavior: 覆盖 TCP 客户端、玩家激活、战斗复制、可见性、重连和 hardening 场景。
  - File: repository project inventory
  - Function / Module: dome Test tree
  - Relevant behavior: 约 111 个验证工程，形成大量 focused coverage，但本次未运行完整矩阵。
- Problem: 验证器数量和质量都已有基础，但缺少一个从项目依赖构建、no-build verifier 运行、结果归档到 ledger evidence 更新的统一入口。
- Why it matters: 单个验证器通过不等于当前主干可发布；统一入口能降低漏跑、跑错代码树和使用过期产物的风险。
- Realistic failure scenario: 开发者运行某个已有 DLL 的 verifier -> 结果通过 -> 未发现当前 Server project 无法编译或 verifier 实际来自旧构建产物。
- Minimal fix: 提供按 slice 的串行脚本，强制 build affected project、检查 `Build/bin` artifact、再执行 `--no-build --no-restore` verifier 并保存 exit code。
- Better long-term fix: 将 slice manifests、project graph、verifier result 和 ledger evidence 接入 CI/审计报告生成器。
- Regression test suggestion: 在空/过期 artifact、错误 project reference 和 verifier failure 时统一入口必须失败并报告路径、退出码和 warning/error 统计。
- Estimated effort: 2-3 days

### Finding: 旧项目经验可以直接迁移，但不能替代游戏服务器语义

- Severity: Info
- Confidence: High
- Category: Maintainability
- Status: Confirmed
- Affected area: developer capability assessment
- Evidence:
  - File: `D:\ProjectItem\SourceCode\Net\TerrariaTools\Isolation\docs\DDD\01-统一语言.md`, `...\09-分层与代码映射.md`, `...\10-测试与校验.md`
  - Function / Module: DDD/architecture/test documentation and tests
  - Relevant behavior: 有统一语言、上下文地图、分层映射、架构/文档测试和证据模型。
  - File: `D:\ProjectItem\SourceCode\Net\NL\tests\NLISSN.UnitTests`, `...\NLISSN.ContractTests`, `...\NLISSN.HostTests`
  - Function / Module: test suite layers
  - Relevant behavior: 有 Unit、Contract、Host、Performance 等分层和性能事实聚合。
  - File: `D:\ProjectItem\SourceCode\DesktopPet\Transplant\FreeRTOS-KernelV11.3.0`, `D:\ProjectItem\SourceCode\Kernel\docs\scheduler.md`
  - Function / Module: embedded/runtime scheduling
  - Relevant behavior: 接触 FreeRTOS、任务队列、SysTick、PendSV、SVC、任务调度和受限资源时序。
- Problem: 这些经验已经覆盖“如何拆系统、如何验证工具、如何处理节拍和资源”，但多人服务器还增加了客户端不可信、网络丢包/乱序、快照一致性、实体生命周期和跨重启身份等语义。
- Why it matters: 认知迁移若过度类比，可能把嵌入式任务节拍或静态迁移证据直接当成服务器权威与网络正确性的证明。
- Realistic failure scenario: 用单机/RTOS 式“任务按时运行”推断联机状态正确 -> 未测试迟到输入和重连 -> 服务器状态与客户端视图产生不可恢复偏差。
- Minimal fix: 学习路线强制每一项旧经验都绑定一个服务器特有验证：tick deadline 对应输入迟到，队列对应限流/背压，快照对应重连/重同步。
- Better long-term fix: 建立游戏服务器术语词典和端到端实验仓，把旧项目能力作为实现杠杆，而不是完成度指标。
- Regression test suggestion: 学习闭环用双客户端、丢包/乱序、实体销毁/复用和恢复重放测试，输出可比较的 tick/state hash。
- Estimated effort: 1 day to establish study matrix; ongoing

## 5. Architecture Concerns

- Coverage: High
- Inspected evidence: `src`/`src2`/`dome/dome1` project graph, `DomeServer`, `DomeSimulation.Tick`, identity/Registry/Adapter documents, ECS organization constraints, migration ledger.
- Exclusions / limits: 未对所有类型逐一做依赖图分析；本次重点覆盖服务器主路径和迁移边界。

当前架构的正向事实是：Registry/Adapter 分层、typed identity projection、命令提交点和显式 tick schedule 都是适合服务器迁移的方向。主要架构风险不是“没有分层”，而是三棵代码树的权威关系和可运行集成闭包尚未锁定；核心 orchestration 类仍然很大，但已有可逐步提取的阶段边界。对应详细问题见“Dome 核心类与多代码树边界造成高回归半径”。

## 6. Security Concerns

- Coverage: Medium
- Inspected evidence: TCP listener/session host、frame length validation、password/host-token paths、network configuration input、session and command queue boundaries。
- Exclusions / limits: 未进行真实网络攻击、端口扫描、协议 fuzzing、密钥存储和依赖 CVE 全量检查。

已观察到 frame 长度、session timeout、最大连接数、待处理命令上限和配置快照不暴露 secrets 的设计意图；但监听地址和认证配置仍需要统一的生产策略。当前没有足够证据报告远程代码执行或凭据泄露，因此不提高为 Critical。

## 7. Stability Concerns

- Coverage: High
- Inspected evidence: `SimulationLoopAsync`, `Program`, `Dispose`, session exception boundaries, `SimulationFault`, `LastSessionFault`, persistence recovery coordinators, loopback fault output。
- Exclusions / limits: 未运行长时间故障注入和断电模拟。

最大稳定性问题是全局 simulation fault 的生命周期传播；单个 session fault 已尝试隔离，说明局部故障边界方向正确。服务器必须把“进程仍在”与“模拟仍在推进”分开建模。

## 8. Performance Concerns

- Coverage: Medium
- Inspected evidence: tick phase list, `MaximumProtocolCommandsPerTick`, replication calls, fixed delay loop, component/query/system structure, old `NL` performance tests.
- Exclusions / limits: 没有实体规模 × 玩家数 × projectile 数的真实基准，也没有 P99/GC/带宽结果。

当前性能知识学习重点不是先优化单个 System，而是建立 tick budget、尾延迟、分配和复制字节数的测量习惯。固定 16ms delay 是明确的调度风险；具体修复见 finding。

## 9. Testing Gaps

- Coverage: High
- Inspected evidence: dome verification project inventory, combat/session/hardening loopback programs, focused verifier projects, migration ledger statuses, five serial build/run checks。
- Exclusions / limits: 未执行全部 111 个验证工程；历史工作树删除态文件不能视为当前可运行测试。

已有 loopback 测试是本项目的重要资产，尤其适合作为你的服务器领域学习入口。当前测试体系的关键缺口是：真实 Server 主干构建不稳定、账本 verified 为 0、缺少统一入口和完整垂直切片。

## 10. Maintainability Concerns

- Coverage: High
- Inspected evidence: file counts, core class sizes, project references, `Context/progress.md`, migration reports, legacy project structure and docs-as-code tests.
- Exclusions / limits: 未执行全量复杂度/重复代码工具。

你在旧项目中形成的文档、架构测试和静态分析能力明显强于当前游戏服务器领域经验。维护性投资应从“确定 source of truth、统一项目图、把大型编排类的阶段边界变成可测试端口”开始，而不是全面重写。

## 11. Design / Principles Concerns

- Coverage: High
- Inspected evidence: ECS file/component constraints, typed identity design, command/commit structures, tick context validation, server orchestration。
- Exclusions / limits: 原则判断只记录产生真实风险的结构问题，不把格式偏好作为缺陷。

### Principles Violated

| Principle | Violations | Severity | Affected Areas |
|-----------|------------|----------|----------------|
| Fail-Fast / explicit failure propagation | 1 | High | `SimulationLoopAsync` / `Program` |
| Single Responsibility (SRP) | 1 | Medium | `DomeSimulation`, `DomeServer` orchestration |
| KISS / explicit time model | 1 | Medium | server tick scheduling |
| Source-of-truth / boundary ownership | 1 | Medium | `src`, `src2`, `dome/dome1` integration |

### Principles Respected

- 迁移账本、证据 catalog、work item 和 recovery metadata 体现了可追踪性与变更可恢复性。
- Registry 只负责身份关系、冲突和生命周期，领域状态留在对应实体/组件，符合边界隔离方向。
- `DomeSimulation.Tick` 通过 `SimulationTickContext` 检查阶段顺序，避免用文件顺序隐式表达执行顺序。
- 旧项目的 DDD、架构测试、Contract/Host/Performance 分层说明你已经能维护显式工程约束。

## 12. Release Concerns

- Coverage: Medium
- Inspected evidence: `global.json`, `AGENTS.md` build contract, `Invoke-SerialDotnet.ps1`, affected project builds, expected `Build/bin` artifacts, CI入口检索。
- Exclusions / limits: 未检查实际部署编排、签名、容器/主机配置和回滚演练。

当前发布结论为“不具备稳定发布条件”：服务器主干 build 失败，simulation fault 没有传播到进程生命周期，也没有统一 release gate。`WorldSession` 和 `AudioParticlesCinematicsVerification` 的局部 build/run 通过只能证明局部项目可用，不能覆盖 server。

## 13. Documentation Analysis

- Coverage: High
- Inspected evidence: `Context/progress.md`, `docs/architecture/entity-identity-context.md`, PRD/ADR, migration map, second-round audit, `docs/research`, old Isolation DDD docs。
- Exclusions / limits: 未逐份重写或校对历史文档。

文档数量和结构是当前强项，但状态字段存在时间点漂移。报告、账本和 verifier 输出应以 ledger revision/run ID 关联；“audit ok”只能说明结构合法，不能代替编译和行为等价。

## 14. Configuration Safety Analysis

- Coverage: Medium
- Inspected evidence: `ServerLaunchOptions`, `ServerHostState`, `NetworkSessionConfigurationInput`, `DomeServer.Start`, secret-exclusion verifier assertions。
- Exclusions / limits: 未覆盖所有外部环境变量、部署参数和生产 secret provider。

配置模型已经有 validated/frozen snapshot 的意识，但 listener address、password、host token、UPnP 和最大连接数需要一份可执行的配置矩阵，明确哪些组合只允许本地测试，哪些组合可用于生产。

## 15. Observability / Operability Analysis

- Coverage: Medium
- Inspected evidence: `SimulationFault`, `LastSessionFault`, tick trace, server console output, verifier diagnostics。
- Exclusions / limits: 未发现完整 metrics/health/alert/runbook 闭环，未做告警演练。

当前有局部错误对象和 tick trace，适合扩展为运行时诊断；缺口是 liveness/readiness、tick age、fault counter、连接数、队列深度、复制字节数和超预算告警。

## 16. Data Integrity Analysis

- Coverage: Medium
- Inspected evidence: snapshot format versioning, save coordinators, `CreatePersistenceSnapshot`, player/account persistence, sidecar state。
- Exclusions / limits: 未做 crash consistency、磁盘损坏、backup/restore 和引用修复的实际演练。

持久化设计已经超出空白原型，但必须用故障注入证明“只恢复完整 generation”“sidecar 与主快照同代”“跨重启不复用 EntityUuid”等不变量。

## 17. Privacy / Data Governance Analysis

- Coverage: Low
- Inspected evidence: AccountUuid/player profile fields, server password/host token configuration, persistence files and logging searches。
- Exclusions / limits: 未做法规、留存、删除、导出和数据访问控制审查。

当前没有足够信息对隐私合规做强结论。至少应把 AccountUuid、玩家 profile、密码和 host token 分级，禁止 secrets 进入 snapshot、日志和 verifier 产物，并明确保存周期。

## 18. Accessibility / UX Correctness Analysis

- Coverage: Not assessed
- Inspected evidence: 当前目标工程以 server/runtime/protocol 为主，无适用 UI 交互入口。
- Exclusions / limits: 客户端 UI/渲染不在本次迁移服务器评估范围。

不适用。

## 19. Supply Chain / Reproducibility Analysis

- Coverage: Low
- Inspected evidence: `global.json`, `.csproj` package/project references, `Build/packages`, build wrapper and repository status。
- Exclusions / limits: 未完成依赖漏洞、包来源、CI 权限、制品签名、SBOM 和发布可复现性审计。

当前至少应把工程引用路径检查和 SDK/包锁定加入 release gate；本报告不对未检查的依赖 CVE 或 CI 安全做断言。

## 20. Cost / Resource Economics Analysis

- Coverage: Medium
- Inspected evidence: fixed tick loop, protocol command cap, connection cap, replication fan-out, old NL performance facts。
- Exclusions / limits: 未有真实机器规格、网络带宽、GC、磁盘和云成本数据。

成本学习目标应表现为每个 tick 的 CPU budget、每个连接的复制字节、每个实体的内存和每次恢复的 I/O；在这些指标之前，不应凭感觉扩大最大连接数或实体上限。

## 21. AI / LLM Safety Analysis

- Coverage: Not assessed
- Inspected evidence: 未发现迁移运行时模型调用、提示词、RAG 或工具授权面；AI 相关文件名属于辅助工具/缓存信号。
- Exclusions / limits: 不评估仓库外部 AI 服务或开发辅助工具的安全。

不适用。

## 22. Fallback / Defensive Code Analysis

- Coverage: Medium
- Inspected evidence: server/session catch blocks, persistence recovery results, packet length validation, queue rejection paths。
- Exclusions / limits: 未完成所有分支的自动分类。

session 级异常隔离、长度拒绝、队列上限属于有意防御；simulation 级通用 catch 因没有 supervisor 传播而成为稳定性问题。后续应明确每个 fallback 是拒绝、降级、重试还是终止，避免只记录错误而继续假活。

## 23. Testing Authenticity Analysis

- Coverage: Medium
- Inspected evidence: loopback verifier source, direct socket/session flows, focused verifier output, build artifacts, ledger verification fields。
- Exclusions / limits: 未运行全量测试，也未检查所有测试是否依赖旧 DLL。

### Confidence Assessment

| Test Area | Real Confidence | Risk | Action |
|-----------|-----------------|------|--------|
| Combat/session loopback | High for covered paths | 未覆盖全量实体与生产构建 | Keep and extend |
| Protocol focused verifier | Medium | 当前 verifier build 有缺失类型 | Repair build then keep |
| Component focused verifiers | Medium | 可能只证明字段/局部规则 | Add lifecycle and replay |
| Ledger audit scripts | High for ledger structure | 不证明运行时等价 | Keep as metadata gate only |

### Valuable Tests

- 真实 TCP loopback、玩家激活、战斗复制、可见性和断线重连测试。
- 使用 `SimulationTickContext` 断言阶段顺序的局部验证。
- 旧项目 NL 中 Unit/Contract/Host/Performance 的测试分层，可直接借鉴到迁移验证入口。

### Suspicious Tests

- 只验证组件实例存在、字段值或源码路径而不推进真实 tick 的测试，不能替代行为验证。
- 使用过期 Build/bin 产物运行而没有先确认当前项目 build 的测试结果。

### Missing Tests

- simulation fatal fault -> host shutdown/liveness。
- deadline drift、tick budget/P99、连接/命令队列背压。
- crash-consistent persistence、sidecar generation 和恢复引用修复。
- 迁移垂直切片的双客户端 replay/state hash。

## 24. Type Safety Analysis

- Coverage: Medium
- Inspected evidence: typed identity types, `NetworkMessageBufferAdapterState`, frame length checks, typed commands, nullable annotations and verifier compile failure。
- Exclusions / limits: 未运行全量 analyzer 和 boundary type inventory。

typed Entity/Account/Replication projection 是正确方向；风险不在于缺少类型，而在于多个身份作用域必须在真实重连、槽位复用和持久化恢复中保持约束。网络配置仍包含字符串地址和 secret 字段，需避免将配置 DTO 误当成安全状态。

## 25. Frontend State Analysis

- Coverage: Not assessed
- Inspected evidence: 当前范围没有前端组件运行时。
- Exclusions / limits: 不适用于本次服务器迁移审计。

不适用。

## 26. Backend API Analysis

- Coverage: Medium
- Inspected evidence: `TerrariaProtocolSessionHost.HandleAsync`, `ReadFrameAsync`, packet dispatcher, server command queue, session timeout and queue caps。
- Exclusions / limits: 该项目使用 Terraria TCP 协议而非 HTTP；未完成兼容协议全量 fuzzing。

协议入口已有长度校验、session timeout、命令队列背压和异常隔离，是可复用的边界基础。应继续补齐乱序/重复/迟到输入、未知实体、旧包和重同步语义，并把认证、授权、限流和错误响应形成一个可执行契约。

## 27. Dependency Weight Analysis

- Coverage: Low
- Inspected evidence: representative project files and `Build/packages` package references。
- Exclusions / limits: 未统计传递依赖大小、trim/AOT 影响和实际发布包体积。

当前没有足够证据提出删包建议。优先修复项目图和可复现构建，再做依赖重量优化。

## 28. Code Consistency Analysis

- Coverage: Medium
- Inspected evidence: C# naming/organization constraints, representative project files, server/session exception patterns, verifier layout。
- Exclusions / limits: 未做全量格式、命名和异常模式扫描。

仓库已有明确的 ECS 文件/组件命名和 dotnet 串行构建规则；主要一致性问题是状态口径、代码树权威关系和 verifier 入口，而不是格式风格。

## 29. Comment Coverage Analysis

- Coverage: Medium
- Inspected evidence: AGENTS, architecture constraints, server comments, tick trace types, old Isolation docs-as-code。
- Exclusions / limits: 未统计每个 public API 的 XML documentation 覆盖率。

关键复杂边界已有文档基础。新增文档应优先解释“为什么这个身份不能跨作用域”“为什么这个阶段必须在该阶段提交”，避免给简单赋值添加低价值注释。

## 30. Recommended Fix Order

### Fix Immediately

1. 修复 Dome Simulation -> EntityEcs 的工程引用，串行重建 Server，并确认 `Build/bin` 服务器 DLL 产物。
2. 将 simulation fault 接入 host supervisor 和入口退出/健康状态，防止假活服务。
3. 固定一个 Player + Projectile 垂直切片，建立第一条真实 verified evidence。

### Fix Before Stable Release

1. 用 deadline/单调时钟替代“工作后再等 16ms”，记录 tick budget 与 P99。
2. 完成 listener/auth/config 矩阵、限流和 secret 脱敏。
3. 完成原子快照、generation/checksum、sidecar 同代校验与 crash-recovery verifier。
4. 建立统一串行 build -> artifact -> no-build verifier -> evidence 入口。

### Schedule Later

1. 逐步提取 `DomeSimulation`/`DomeServer` 的阶段编排与适配端口。
2. 统一 `src`、`src2`、`dome/dome1` 的 source-of-truth 和项目图。
3. 用 BenchmarkDotNet/EventPipe/`dotnet-trace`/`dotnet-counters` 建立实体规模基准。

### Ignore for Now

1. 低价值格式争议、非运行时注释数量和未影响当前 server slice 的依赖体积优化。
2. 前端可访问性和 AI/LLM 安全维度；它们不属于当前服务器迁移闭环。

## 31. Quick Wins

- 为 `DomeServer` 增加 `IsSimulationHealthy`/`SimulationFault` 的可观察状态，并让 `Program` 感知 simulation task 结束。
- 增加 project reference path verifier，阻止 `dome/src` 这类跨根路径漂移再次进入主干。
- 在 ledger dashboard 同时显示 `migrated / verified / deferred / not-run`，并禁止只显示 migrated。
- 在 server loop 记录 tick elapsed、queue depth、active sessions、snapshot bytes，先用结构化 console/log sink 输出。
- 为 first vertical slice 写一份 replay fixture，固定两客户端输入、Projectile identity、命中、销毁和重连状态 hash。

## 32. Long-term Refactor Plan

1. **建立服务器语义实验仓**：实现单线程固定 tick、双客户端输入、碰撞、Projectile 生命周期、快照和重放；它是学习与设计验证工具，不承担生产代码。
2. **收敛唯一权威运行时**：定义 root `src`、`src2`、`dome/dome1` 的角色，禁止未声明的跨树引用；把每个 slice 的依赖图写入 manifest。
3. **把 tick orchestration 变成可观测 supervisor**：Simulation 只处理权威状态，network replication、persistence 和 metrics 通过显式 ports/adapters 连接，并定义故障所有权和重试策略。
4. **建设迁移证据门**：结构处置 -> 编译 -> focused behavior -> loopback -> replay -> persistence，每层结果带 commit/project/artifact/exit code；只有最后一层完成才写入 verified。
5. **建立容量与恢复工程**：以玩家数、实体数、Projectile 数、复制频率和存档大小为变量，采集 P50/P95/P99 tick、GC、带宽、队列丢弃和恢复时间，形成发布容量表。

## 33. Knowledge Gap Assessment And 90-Day Route

### Priority 1: authoritative simulation, fixed tick, and entity lifecycle

需要掌握固定时间步、单调时间、输入采样、系统顺序、生成/销毁提交点、暂停/慢 tick、确定性边界，以及 Player/NPC/Projectile 在一个 tick 内何时读写和提交。你的 FreeRTOS/Kernel 节拍经验是入口，但服务器还要处理迟到输入、网络背压和客户端观察延迟。

### Priority 2: network replication and trust boundary

需要掌握客户端输入与服务器状态的区别、snapshot/delta、可靠/不可靠消息、序列号、ack、重传、丢包/乱序/重复、重连和全量重同步，以及服务器永远不能接受客户端指定的字段。现有 Terraria protocol/session/loopback 代码适合作为实验材料。

### Priority 3: ECS data-oriented execution and performance

需要掌握查询成本、数据布局、结构变化、对象/缓冲池、分配、锁竞争、tick budget、P99 与 GC。不要只问“功能是否正确”，还要问“最大实体数下每 tick 是否按时完成”。

### Priority 4: persistence, migration, and recovery

需要掌握世界与玩家存档边界、schema version、幂等写入、原子替换、备份、恢复引用修复和持久化实体与临时实体的区别。当前 `EntityUuid` 的作用域设计正确，但必须用跨重启测试证明它不被错误当作持久化主键。

### Priority 5: server security and operations

需要掌握身份认证、权限、客户端不可信、限流、资源耗尽防护、结构化日志、metrics、health/readiness、优雅停机、故障隔离、secret 生命周期和恢复 runbook。

### Priority 6: Terraria domain semantics

建立 Player/NPC/Projectile/World/TileEntity/Item 的生命周期词典：spawn、despawn、AI、owner、伤害来源、碰撞、掉落、世界保存、网络可见性；先做 Player + Projectile，再扩展 NPC 和 World objects。

### Days 1-14: build the mental model

阅读当前 PRD、ADR、ledger 和 Version4 关键实现，画两张图：一张 tick 时序图，一张 runtime identity/account/protocol/persistence 边界图。实现不接真实网络的最小双客户端模拟器，支持迟到输入、Projectile 生成和可重放日志。

### Days 15-30: first vertical slice

只做 Player + 一个 Projectile：权威 spawn、输入、移动、碰撞、伤害、复制、销毁和重连。先修复 Server build，再把 loopback verifier 变成当前依赖闭包下可重复运行的 evidence producer。目标是第一条 `verified`，不是新增更多组件。

### Days 31-60: network and persistence

加入丢包、乱序、重复、迟到输入、槽位复用、全量重同步；加入主快照与 sidecar 的 generation/checksum、故障注入和恢复引用修复。同步记录 P99 tick、GC 分配、复制字节和队列深度。

### Days 61-90: expand and release-gate

把同一模式扩展到 NPC，再到玩家/NPC/Projectile 交互；建立统一 build/verifier/ledger gate、健康检查、故障退出、容量表和灰度/回滚演练。只有垂直切片与发布门稳定后，才继续批量消化 deferred decisions。

### Weekly evidence dashboard

- `verified` decision 数量，以及每条对应的端到端路径和 artifact。
- Spawn、Tick、复制、销毁、重连、恢复各阶段的真实通过数。
- P50/P95/P99 tick、GC 分配、最大实体数、复制字节和队列丢弃。
- `NotFound`、`Expired`、`ScopeMismatch`、`Conflict` 解析失败计数。
- 核心 Simulation 中旧身份字段的读写数量。
- replay state hash 的稳定性和恢复后的身份映射。

## 34. Build Verification Record

所有 compile-capable 命令均先检查活动 `dotnet.exe`/`csc.exe`，并通过 `Build/Tools/Invoke-SerialDotnet.ps1` 串行执行；由于包装脚本直接 `-File` 调用会把 `-p:...` 解析为自身模糊参数，本次使用其 `-DotnetArguments` 数组形式。

| Project | Command shape | Exit code | Warnings / errors | Artifact / result |
|---------|---------------|-----------|------------------|------------------|
| `src/Player/Terraria.Player.csproj` | wrapper `build`, `-m:1`, `-nr:false`, `UseSharedCompilation=false` | 1 | 0 / 5 | no artifact; missing `Relationships` and `Projectile` references used by source |
| `src2/NetworkProtocolSessionVerification/Terraria.NetworkProtocolSessionVerification.csproj` | wrapper `build`, same serial properties | 1 | 0 / 1 | no verifier artifact; `Program.cs:336` references missing `NetworkModuleRegistryState` |
| `src/WorldSession/Terraria.WorldSession.csproj` | wrapper `build`, same serial properties | 0 | 0 / 0 | `Build/bin/Terraria.WorldSession/Debug/net10.0/Terraria.WorldSession.dll` |
| `src2/ClientPresentation/AudioParticlesCinematicsVerification/Terraria.AudioParticlesCinematicsVerification.csproj` | wrapper `build`, same serial properties | 0 | 0 / 0 | `Build/bin/Terraria.AudioParticlesCinematicsVerification/Debug/net10.0/` |
| same verifier | wrapper `run --no-build --no-restore` | 0 | n/a | `P19 verifier passed.` |
| `dome/dome1/src/Terraria.Dome.Server/Terraria.Dome.Server.csproj` | wrapper `build`, same serial properties | 1 | 1 / 203 | no server DLL; invalid `Terraria.EntityEcs.csproj` path cascades into missing types |

局部成功不能抵消 Server 主干失败；本记录只声明上述项目和命令的结果，不声明整个仓库可编译。

## 35. Scope And Limitations

本报告是一次证据驱动的仓库和能力评估，不是完整渗透测试、完整性能基准、法规隐私审查或所有 verifier 的总运行。审计排除了 `Build/bin`、`Build/obj`、`Build/generated` 等可再生输出作为源码证据；项目 inventory 达到 8,000 文件扫描上限，因此规模统计采用针对性目录盘点和代表性工程检查。工作树存在大量用户/迁移中的修改、删除和未跟踪文件，本报告没有清理或回滚任何既有变更，也没有修改业务源码。

旧项目评估基于代表性项目和其文档/测试结构，不能推断每个历史项目的完整质量。外部学习资料可以帮助建立概念，但不能替代本地 Version4 源码、协议回放和 NLTX focused verifier 的行为证据。

---

**Overall conclusion:** 你是“系统工程与迁移治理能力较强、正在补齐多人游戏服务器运行时语义”的工程师。先把服务器主干恢复到可构建，再用一条真实 Player/Projectile 闭环产生第一条 verified evidence；这会比继续扩大组件数量更快地暴露真正知识缺口，也更接近可发布的迁移结果。
