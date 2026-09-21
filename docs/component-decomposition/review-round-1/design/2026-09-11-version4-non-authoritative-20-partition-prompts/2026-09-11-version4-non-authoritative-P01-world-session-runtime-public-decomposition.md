# Version4 非权威组件拆分分区 P01：世界会话与运行时 - public-decomposition 专属提示词

> 本文件只包含当前分区的专属任务内容，不复制公共提示词正文。
> 使用时：先提供公共提示词，再附加本文件；公共协议中的通用规则、证据门槛、报告结构、禁止项和 Integration Handoff 继续有效。

## 公共前置（仅引用）

- 公共提示词：`D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\design\2026-09-05-version4-public-decomposition-common-protocol.md`
- 必读技能：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\SKILL.md`
- ECS 证据协议：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\ecs-evidence-protocol.md`
- tModLoader 检索协议：`D:\TRbackup\NLTX\.agents\skills\public-decomposition\references\tmodloader-documentation-retrieval.md`
- 本文件不重复上述文件内容；执行前必须实际读取并遵守它们。
- 并行会话保障 skill：D:\\TRbackup\\NLTX\\.agents\\skills\\version4-non-authoritative-partition-session\\SKILL.md
- 并行领取/结算 runner：D:\\TRbackup\\NLTX\\Build\\Tools\\Invoke-Version4NonAuthoritativePartitionSession.ps1
- 必须先按 skill 使用 ClaimNext（或用户明确指定时使用 Claim -PartitionId P01），只写本文件的 outputReport，并用领取返回的同一 sessionId 完成 Complete/Fail/Abandon；不得手工编辑 ledger 或 lock。
- 本任务属于 20 个并行会话中的固定分区；currentTaskCount: 20。公共协议中遗留的 19 会话数量说明在本任务包中不适用。

## 任务元数据

- promptId: P01-non-authoritative-public-decomposition-20260911
- partitionId: P01
- inputPartitionReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\01-world-session-runtime.md
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P01-world-session-runtime-public-decomposition.md
- reportType: public-decomposition
- executionMode: independent-read-only-partition
- currentTaskCount: 20
- formalParentSubsystems: RuntimeComposition, SharedRuntimeMechanisms
- leafSubsystemCount: 12
- fieldCount: 104
- propertyCount: 45
- memberCount: 149
- sourceReportSha256: b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196
- sourceInventoryStatus: source-inventory-confirmed; member-level owner/lifecycle/side-effect evidence still requires this session to close

## 专属范围锁定

- 本会话只审查输入分区报告中的 12 个叶子子系统和 149 条成员记录。以下清单是本会话唯一的成员范围；不得读取或复制其他 P 分区的成员清单。
- 细分子系统仍只是库存边界，不等价于一个组件；允许在本分区内部按访问模式拆成多个 proposed Component、System、Query、Command、Adapter 或 Projection。
- 来源序号、路径、行号和声明是检索线索；必须回到 Version4 重新定位，不得把输入报告直接当作已确认的读者、写者、生命周期或 owner 证据。

| 叶子子系统 | 父级 | 原章节 | 边界角色 | 字段 | 属性 | 成员 | 细分职责 |
|---|---|---|---|---:|---:|---:|---|
| `MainFrameActivityState` | `RuntimeComposition` | `4.1.1` | `runtime state` | 4 | 0 | 4 | 帧级玩家、Boss 和可交互对象活动事实。 |
| `MainBootstrapAndWorldRules` | `RuntimeComposition` | `4.1.3` | `runtime state` | 23 | 0 | 23 | 启动引用、版本、秘密种子和世界规则开关。 |
| `MainClockAndFrameScheduling` | `RuntimeComposition` | `4.1.6` | `runtime state` | 13 | 0 | 13 | 全局时钟、延迟处理、帧率和服务器调度参数。 |
| `MainFrameAndWorldRuleControl` | `RuntimeComposition` | `4.1.13` | `runtime state` | 9 | 0 | 9 | 暂停、世界难度、帧计数和自动加入控制。 |
| `MainRandomAndSeedState` | `RuntimeComposition` | `4.1.22` | `runtime state` | 4 | 0 | 4 | 随机源、月亮类型和实验/种子配置。 |
| `MainSaveAndWorldSessionState` | `RuntimeComposition` | `4.1.30` | `session state` | 11 | 0 | 11 | 世界准备、存档路径、活动文件和世界列表。 |
| `MainWindowAndShutdownState` | `RuntimeComposition` | `4.1.36` | `runtime state` | 7 | 0 | 7 | 窗口、锚点管理、退出和自动生成路径状态。 |
| `MainTimeSkipState` | `RuntimeComposition` | `4.1.37` | `runtime state` | 4 | 0 | 4 | 日晷/月晷快速推进和冷却状态。 |
| `MainDerivedWorldAndSessionQueries` | `RuntimeComposition` | `4.1.45` | `derived/query` | 0 | 22 | 22 | 世界模式、路径、资格和会话对象的只读派生查询。 |
| `SharedDifficultyAndRuleMetadata` | `SharedRuntimeMechanisms` | `4.9.4` | `definition/query` | 16 | 0 | 16 | 难度等级、曲线和规则元数据。 |
| `WorldSeedAndExploitRules` | `SharedRuntimeMechanisms` | `4.9.101` | `state/query` | 1 | 12 | 13 | 特殊种子规则和吞噬者漏洞保护状态。 |
| `SharedStartupAndRuntimeHostState` | `SharedRuntimeMechanisms` | `4.9.223` | `diagnostics/adapter` | 12 | 11 | 23 | 启动参数、运行时宿主、平台句柄和服务依赖状态。 |

来源成员的分区内序号线索范围：1..3921；序号不定义领域边界，必须逐条回到输入表和 Version4 覆盖。

## 分区专属目标

围绕“世界会话与运行时”完成独立只读的 Version4 成员审查和 ECS 拆分设计。
- 围绕 Main 的启动、世界会话、帧时钟、暂停/跳时、规则开关、随机种子和退出生命周期，确认哪些字段是真实权威状态，哪些只是宿主引用或派生查询。
- 回到 Terraria/Main.cs 的静态初始化、世界加载/卸载、帧循环、保存与退出调用者；对 GameDifficultyData、GameDifficultyLevel、特殊种子和漏洞保护实现分别核对读者/写者。
- 对 Program、InitData、Ref<T>、Windows/Server Game 等宿主类型建立 Adapter/Port 边界；不要把平台句柄、启动参数或服务依赖设计成模拟 Component。
- 特别核对时间推进、延迟处理、随机流、世界规则和派生属性之间的系统顺序与跨分区交接，区分单机、服务端和客户端生命周期。

## 专属证据焦点

- 先完整读取本分区输入报告的成员表，再回到 D:\TRbackup\Version4 重新核对字段、属性、声明类型、初始化/注册、直接读者、直接写者、清理路径和副作用。
- 下列路径只作为本分区的检索种子；路径存在不等于成员语义已确认，必须按公共证据协议记录实际行号、符号、调用者、读者、写者、生命周期和 evidenceStatus。
  - `D:\TRbackup\Version4\Terraria.DataStructures/GameDifficultyData.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/GameDifficultyLevel.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/FixExploitManEaters.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/SpecialSeedFeatures.cs`
  - `D:\TRbackup\Version4\Terraria.Server/Game.cs`
  - `D:\TRbackup\Version4\Terraria/InitData.cs`
  - `D:\TRbackup\Version4\Terraria/Main.cs`
  - `D:\TRbackup\Version4\Terraria/Program.cs`
  - `D:\TRbackup\Version4\Terraria/Ref.cs`
  - `D:\TRbackup\Version4\Terraria/WindowsLaunch.cs`

- 输入分区抽取出的声明类型焦点：
  - `Terraria.DataStructures.GameDifficultyData`
  - `Terraria.DataStructures.GameDifficultyData.LinearCurve`
  - `Terraria.DataStructures.GameDifficultyData.LinearCurve.Key`
  - `Terraria.DataStructures.GameDifficultyLevel`
  - `Terraria.GameContent.FixExploitManEaters`
  - `Terraria.GameContent.SpecialSeedFeatures`
  - `Terraria.InitData`
  - `Terraria.Main`
  - `Terraria.Main.CurrentFrameFlags`
  - `Terraria.Program`
  - `Terraria.Ref<T>`
  - `Terraria.Server.Game`
  - `Terraria.WindowsLaunch`

- 若路径、类型或行号漂移，记录 version-drift 或 evidence-mismatch；不得静默沿用库存报告行号。Version4 是 Terraria 私有实现的事实来源，完整源码只补证 Version4 已存在文件，tModLoader 只交叉验证公开边界，SS14 只参考结构粒度。

## 专属审查问题

- 启动配置、世界规则、随机种子、会话文件引用和帧调度是否各自有唯一写入根？Main 中同文件字段如何按访问模式进一步拆成组件、System 或 Query？
- 全局时钟、time-skip、延迟协程和派生世界查询的生命周期是否一致；哪些是短期 Command payload、缓存或 Projection，不能升格为持久 Component？
- 难度/规则元数据与世界实例状态是否应分离；特殊种子和 exploit 修复是权威规则、纯资格 Query 还是一次性 System？
- 宿主平台、窗口/退出和服务依赖的失败、重试、清理顺序如何通过 Adapter 隔离，且不让外部句柄渗透核心状态？

## 专属不拆分边界

- 不要把 Main 的全部静态字段合并为 RuntimeComponent；帧计数、时钟缓存、启动参数、世界规则和退出标记必须按写者与生命周期分开评估。
- 不要把只读派生属性、路径、资格判断、随机 helper 或一次性启动 payload 直接设计成权威组件。
- 不要把 Program、平台、窗口引用与世界会话状态合并；宿主边界只保留 Adapter、Command 或 Projection。

专属跨域提醒：重点记录与世界环境、持久化、网络、诊断和内容目录的 integration-risk；跨分区共享的时间、难度、种子、会话和宿主类型统一写 crossSubsystemOwner: integration-review。

## 输出要求

- 将完整研究报告只写入：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P01-world-session-runtime-public-decomposition.md
- 不要写入输入分区报告、公共提示词、其他分区报告、源代码、测试、项目文件或迁移文件。
- 报告必须逐条覆盖本分区成员，给出成员归属表、状态所有权、Component/System/Query/Command/Adapter/Projection 边界、接口契约、依赖方向、顺序约束、不拆分项、兼容策略、evidence-gap、blocking-decision、focused verifier 计划和 Integration Handoff。
- 本轮只读审查不运行编译型命令；没有实际执行的 verifier 必须写 verificationStatus: not-run。
- proposed 类型、路径和接口必须明确标记 proposed；不得写成当前 NLTX 已存在或已经迁移。
- 跨两个或以上分区使用的类型、接口、ID、快照、值对象和 System 顺序只能提出候选，必须写 crossSubsystemOwner: integration-review，不得在本报告单方面裁决。
- 不得修改任何生产代码、测试、项目文件、生成器、公共协议、分区报告或其他并行会话输出。

## 分区完成检查

- [ ] 逐条覆盖本分区全部 149 条成员，成员事实来自输入表但关键语义已回到 Version4 复核。
- [ ] 只使用本分区叶子清单；没有复制、重新设计或宣布其他 P 分区成员。
- [ ] Component 只承载内聚权威状态；派生值、缓存、查询、短期 payload、日志、网络/存档/UI 输出按正确角色隔离。
- [ ] 所有共享候选写 crossSubsystemOwner: integration-review；没有把 proposed 设计写成已实现能力。
- [ ] 未执行的构建、测试和 verifier 均明确 verificationStatus: not-run；证据缺口、冲突和 blocking-decision 已单列。
- [ ] 输出文件路径唯一为：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P01-world-session-runtime-public-decomposition.md

## 会话结束回报

最终消息只回报以下字段：
- partitionId: P01
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P01-world-session-runtime-public-decomposition.md
- evidenceSourcesRead: 实际读取的 Version4 文件/类型、必要的完整源码补证、tModLoader 页面和 SS14 参考
- evidenceGaps: 仍缺失或冲突的关键证据
- blockingDecisions: 需要最终整合会话裁决的 owner/顺序/事务边界
- verificationStatus: not-run
- productionCodeModified: no

本提示词只定义本分区的专属范围和审查抓手；公共协议负责共同证据规则、报告结构、禁止项和 Integration Handoff。
