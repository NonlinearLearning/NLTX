# Version4 非权威组件拆分分区 P20：诊断、工具与共享机制 - public-decomposition 专属提示词

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
- 必须先按 skill 使用 ClaimNext（或用户明确指定时使用 Claim -PartitionId P20），只写本文件的 outputReport，并用领取返回的同一 sessionId 完成 Complete/Fail/Abandon；不得手工编辑 ledger 或 lock。
- 本任务属于 20 个并行会话中的固定分区；currentTaskCount: 20。公共协议中遗留的 19 会话数量说明在本任务包中不适用。

## 任务元数据

- promptId: P20-non-authoritative-public-decomposition-20260911
- partitionId: P20
- inputPartitionReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\20-diagnostics-tools-shared.md
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P20-diagnostics-tools-shared-public-decomposition.md
- reportType: public-decomposition
- executionMode: independent-read-only-partition
- currentTaskCount: 20
- formalParentSubsystems: RuntimeComposition, SharedRuntimeMechanisms
- leafSubsystemCount: 22
- fieldCount: 226
- propertyCount: 24
- memberCount: 250
- sourceReportSha256: b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196
- sourceInventoryStatus: source-inventory-confirmed; member-level owner/lifecycle/side-effect evidence still requires this session to close

## 专属范围锁定

- 本会话只审查输入分区报告中的 22 个叶子子系统和 250 条成员记录。以下清单是本会话唯一的成员范围；不得读取或复制其他 P 分区的成员清单。
- 细分子系统仍只是库存边界，不等价于一个组件；允许在本分区内部按访问模式拆成多个 proposed Component、System、Query、Command、Adapter 或 Projection。
- 来源序号、路径、行号和声明是检索线索；必须回到 Version4 重新定位，不得把输入报告直接当作已确认的读者、写者、生命周期或 owner 证据。

| 叶子子系统 | 父级 | 原章节 | 边界角色 | 字段 | 属性 | 成员 | 细分职责 |
|---|---|---|---|---:|---:|---:|---|
| `MainDiagnosticsAndSimulationRates` | `RuntimeComposition` | `4.1.9` | `runtime state` | 7 | 0 | 7 | 网络诊断、启动显示、错误策略和模拟速率配置。 |
| `MainTickAndDiagnosticState` | `RuntimeComposition` | `4.1.39` | `runtime state` | 4 | 0 | 4 | 投射物循环索引、帧目标、更新计时和辅助服务。 |
| `SharedCallTrackingDiagnostics` | `SharedRuntimeMechanisms` | `4.9.12` | `diagnostics` | 3 | 0 | 3 | 调用队列、已记录方法和刷新计时。 |
| `SharedTimeSeriesDataSeriesState` | `SharedRuntimeMechanisms` | `4.9.13` | `diagnostics state` | 10 | 0 | 10 | 帧窗口、分位数、最大值和时间序列聚合状态。 |
| `SharedTimeSeriesEntryState` | `SharedRuntimeMechanisms` | `4.9.14` | `diagnostics state` | 5 | 0 | 5 | 单个计时条目、预算、格式化委托和双缓冲序列。 |
| `SharedTimeSeriesFormattingState` | `SharedRuntimeMechanisms` | `4.9.15` | `diagnostics/query` | 5 | 0 | 5 | 性能计时和 CPU 显示格式缓存。 |
| `SharedRandomSources` | `SharedRuntimeMechanisms` | `4.9.54` | `value object` | 9 | 1 | 10 | 伪随机源和随机流实现。 |
| `SharedBufferAndCollectionPools` | `SharedRuntimeMechanisms` | `4.9.55` | `value object` | 23 | 2 | 25 | 缓冲池、缓存缓冲和集合排序容器。 |
| `SharedRangeAndBitUtilities` | `SharedRuntimeMechanisms` | `4.9.56` | `value object` | 10 | 5 | 15 | 范围、位图和位集合值对象。 |
| `SharedGeneralDiagnosticsUtilities` | `SharedRuntimeMechanisms` | `4.9.84` | `diagnostics` | 0 | 5 | 5 | 异常观察和通用诊断工具状态。 |
| `SharedGeneralDelegateAndMetadataUtilities` | `SharedRuntimeMechanisms` | `4.9.85` | `value object/metadata` | 10 | 1 | 11 | 委托方法、旧属性元数据和秘密值辅助。 |
| `DebugCommandProtocol` | `SharedRuntimeMechanisms` | `4.9.86` | `diagnostics/adapter` | 12 | 8 | 20 | 调试命令接口、属性、消息和处理器协议。 |
| `DebugRuntimeOptions` | `SharedRuntimeMechanisms` | `4.9.87` | `diagnostics/state` | 10 | 0 | 10 | 调试命令开关和运行时调试选项。 |
| `DebugFrameTelemetry` | `SharedRuntimeMechanisms` | `4.9.88` | `diagnostics/state` | 16 | 0 | 16 | 详细 FPS 帧、事件和采样状态。 |
| `DebugBuildStatus` | `SharedRuntimeMechanisms` | `4.9.89` | `diagnostics/adapter` | 1 | 1 | 2 | 源码版本和 Git 构建状态信息。 |
| `SharedTimeLoggerFrameCoordinationState` | `SharedRuntimeMechanisms` | `4.9.113` | `diagnostics state` | 15 | 1 | 16 | 计时器帧边界、条目注册和下一帧控制状态。 |
| `SharedGeneralRandomAndBufferUtilities` | `SharedRuntimeMechanisms` | `4.9.158` | `query/value object` | 8 | 0 | 8 | 随机常量、正则缓存和 flood-fill 工作缓冲区。 |
| `SharedTimeLoggerDisplayFormattingState` | `SharedRuntimeMechanisms` | `4.9.167` | `diagnostics/query` | 9 | 0 | 9 | TimeLogger 的 CPU、百分比、毫秒和显示格式缓存。 |
| `SharedTimeLoggerEntityAndInterfacePhaseMetricsState` | `SharedRuntimeMechanisms` | `4.9.198` | `diagnostics state` | 20 | 0 | 20 | TimeLogger 实体绘制、界面、菜单和诊断阶段指标。 |
| `SharedTimeLoggerTileAndLiquidRenderMetricsState` | `SharedRuntimeMechanisms` | `4.9.204` | `diagnostics state` | 24 | 0 | 24 | TimeLogger 固体、液体、墙体、线和 Tile 附加绘制阶段指标。 |
| `SharedTimeLoggerLightingMapAndBackgroundMetricsState` | `SharedRuntimeMechanisms` | `4.9.205` | `diagnostics state` | 22 | 0 | 22 | TimeLogger 光照、地图、瀑布、天空和背景阶段指标。 |
| `SharedIssueReportCatalogState` | `SharedRuntimeMechanisms` | `4.9.222` | `diagnostics/adapter` | 3 | 0 | 3 | 问题报告集合、报告时间和报告正文。 |

来源成员的分区内序号线索范围：116..4024；序号不定义领域边界，必须逐条回到输入表和 Version4 覆盖。

## 分区专属目标

围绕“诊断、工具与共享机制”完成独立只读的 Version4 成员审查和 ECS 拆分设计。
- 围绕 Main 诊断/模拟速率、CallTracker、TimeLogger 时间序列与阶段指标、随机源、缓冲/集合池、范围/位工具、Debug command/options/telemetry/build status 和 issue report，核对诊断投影、工具值对象与核心状态的边界。
- 回到 Terraria/TimeLogger.cs、CallTracker.cs、Testing/Debug、Utilities、IssueReport、Main 及直接调用者，确认计时器、队列、池、采样、刷新和清理的真实副作用与线程生命周期。
- 分别评估 telemetry/diagnostic snapshot、TimeLogger phase metrics、Debug Adapter/Command、build-status Adapter、utility Query/value object、buffer pool 和 issue report Projection。
- 核对日志、时钟、随机、缓冲池、异常观察、调试命令与模拟系统的依赖方向；诊断默认不拥有领域权威状态，所有未执行 verifier 标 verificationStatus: not-run。

## 专属证据焦点

- 先完整读取本分区输入报告的成员表，再回到 D:\TRbackup\Version4 重新核对字段、属性、声明类型、初始化/注册、直接读者、直接写者、清理路径和副作用。
- 下列路径只作为本分区的检索种子；路径存在不等于成员语义已确认，必须按公共证据协议记录实际行号、符号、调用者、读者、写者、生命周期和 evidenceStatus。
  - `D:\TRbackup\Version4\CallTracker.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/BufferPool.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/CachedBuffer.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/DoubleStack.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/EntrySorter.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/GeneralIssueReporter.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/IssueReport.cs`
  - `D:\TRbackup\Version4\Terraria.Testing.ChatCommands/DebugCommandAttribute.cs`
  - `D:\TRbackup\Version4\Terraria.Testing.ChatCommands/DebugCommandProcessor.cs`
  - `D:\TRbackup\Version4\Terraria.Testing.ChatCommands/DebugMessage.cs`
  - `D:\TRbackup\Version4\Terraria.Testing.ChatCommands/IDebugCommand.cs`
  - `D:\TRbackup\Version4\Terraria.Testing/DebugOptions.cs`
  - `D:\TRbackup\Version4\Terraria.Testing/DetailedFPS.cs`
  - `D:\TRbackup\Version4\Terraria.Testing/GitStatus.cs`
  - `D:\TRbackup\Version4\Terraria.Utilities.Terraria.Utilities/FloatRange.cs`
  - `D:\TRbackup\Version4\Terraria.Utilities/Bits64.cs`
  - `D:\TRbackup\Version4\Terraria.Utilities/BitSet2D.cs`
  - `D:\TRbackup\Version4\Terraria.Utilities/CrashWatcher.cs`
  - `D:\TRbackup\Version4\Terraria.Utilities/FastRandom.cs`
  - `D:\TRbackup\Version4\Terraria.Utilities/IntRange.cs`
  - `D:\TRbackup\Version4\Terraria.Utilities/LCG32Random.cs`
  - `D:\TRbackup\Version4\Terraria.Utilities/OldAttribute.cs`
  - `D:\TRbackup\Version4\Terraria.Utilities/Secrets.cs`
  - `D:\TRbackup\Version4\Terraria.Utilities/UnifiedRandom.cs`
  - `D:\TRbackup\Version4\Terraria.Utilities/Vertical64BitStrips.cs`
  - `D:\TRbackup\Version4\Terraria/BitsByte.cs`
  - `D:\TRbackup\Version4\Terraria/DelegateMethods.cs`
  - `D:\TRbackup\Version4\Terraria/Main.cs`
  - `D:\TRbackup\Version4\Terraria/TimeLogger.cs`
  - `D:\TRbackup\Version4\Terraria/Utils.cs`

- 输入分区抽取出的声明类型焦点：
  - `Terraria.BitsByte`
  - `Terraria.CallTracker`
  - `Terraria.DataStructures.BufferPool`
  - `Terraria.DataStructures.CachedBuffer`
  - `Terraria.DataStructures.DoubleStack<T1>`
  - `Terraria.DataStructures.EntrySorter<TEntryType, TStepType>`
  - `Terraria.DataStructures.GeneralIssueReporter`
  - `Terraria.DataStructures.IssueReport`
  - `Terraria.DelegateMethods`
  - `Terraria.DelegateMethods.Minecart`
  - `Terraria.Main`
  - `Terraria.Testing.ChatCommands.DebugCommandAttribute`
  - `Terraria.Testing.ChatCommands.DebugCommandAttribute.InternalDebugCommand`
  - `Terraria.Testing.ChatCommands.DebugCommandProcessor`
  - `Terraria.Testing.ChatCommands.DebugMessage`
  - `Terraria.Testing.ChatCommands.IDebugCommand`
  - `Terraria.Testing.DebugOptions`
  - `Terraria.Testing.DetailedFPS`
  - `Terraria.Testing.DetailedFPS.Frame`
  - `Terraria.Testing.DetailedFPS.Frame.Event`
  - `Terraria.Testing.GitStatus`
  - `Terraria.TimeLogger`
  - `Terraria.TimeLogger.DataSeries`
  - `Terraria.TimeLogger.FormatPool`
  - `Terraria.TimeLogger.TimeLogData`
  - `Terraria.Utilities.Bits64`
  - `Terraria.Utilities.BitSet2D`
  - `Terraria.Utilities.CrashWatcher`
  - `Terraria.Utilities.FastRandom`
  - `Terraria.Utilities.IntRange`
  - `Terraria.Utilities.LCG32Random`
  - `Terraria.Utilities.OldAttribute`
  - `Terraria.Utilities.Secrets`
  - `Terraria.Utilities.Terraria.Utilities.FloatRange`
  - `Terraria.Utilities.UnifiedRandom`
  - `Terraria.Utilities.Vertical64BitStrips`
  - `Terraria.Utils`

- 若路径、类型或行号漂移，记录 version-drift 或 evidence-mismatch；不得静默沿用库存报告行号。Version4 是 Terraria 私有实现的事实来源，完整源码只补证 Version4 已存在文件，tModLoader 只交叉验证公开边界，SS14 只参考结构粒度。

## 专属审查问题

- TimeLogger/CallTracker/Debug telemetry 的计时器、队列、分位数、格式化缓存和阶段指标是权威状态、派生快照还是外部副作用？谁负责刷新和清理？
- 随机源、范围/位工具、缓冲池、集合排序器和正则缓存哪些是纯工具，哪些带隐式全局可变状态，如何通过 seam 测试？
- Debug command/options、Git/build status、CrashWatcher 和 issue report 如何隔离输入、日志、文件和线程副作用；失败/重试是否可观察？
- 诊断字段被 Main、网络、绘制、Tile/液体和 UI 读取时，如何避免共享工具反向成为跨子系统 owner？

## 专属不拆分边界

- 不要把 TimeLogger 所有阶段指标、Debug 选项、CallTracker 队列、随机源、缓冲池和 issue report 合成 SharedDiagnosticsComponent。
- 不要把格式化缓存、采样帧、计时条目、日志队列、build status 或临时 buffer 当作模拟权威状态。
- 不要把通用工具的全局静态字段默认视为共享组件；先确认写者、线程、生命周期和副作用，再决定 Query/Adapter/Projection。

专属跨域提醒：重点记录与运行时会话、网络、绘制、Tile/液体、外部平台和所有领域系统的 integration-risk；诊断快照、时间源、随机源、BufferPool、IssueReport owner 均标 crossSubsystemOwner: integration-review。

## 输出要求

- 将完整研究报告只写入：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P20-diagnostics-tools-shared-public-decomposition.md
- 不要写入输入分区报告、公共提示词、其他分区报告、源代码、测试、项目文件或迁移文件。
- 报告必须逐条覆盖本分区成员，给出成员归属表、状态所有权、Component/System/Query/Command/Adapter/Projection 边界、接口契约、依赖方向、顺序约束、不拆分项、兼容策略、evidence-gap、blocking-decision、focused verifier 计划和 Integration Handoff。
- 本轮只读审查不运行编译型命令；没有实际执行的 verifier 必须写 verificationStatus: not-run。
- proposed 类型、路径和接口必须明确标记 proposed；不得写成当前 NLTX 已存在或已经迁移。
- 跨两个或以上分区使用的类型、接口、ID、快照、值对象和 System 顺序只能提出候选，必须写 crossSubsystemOwner: integration-review，不得在本报告单方面裁决。
- 不得修改任何生产代码、测试、项目文件、生成器、公共协议、分区报告或其他并行会话输出。

## 分区完成检查

- [ ] 逐条覆盖本分区全部 250 条成员，成员事实来自输入表但关键语义已回到 Version4 复核。
- [ ] 只使用本分区叶子清单；没有复制、重新设计或宣布其他 P 分区成员。
- [ ] Component 只承载内聚权威状态；派生值、缓存、查询、短期 payload、日志、网络/存档/UI 输出按正确角色隔离。
- [ ] 所有共享候选写 crossSubsystemOwner: integration-review；没有把 proposed 设计写成已实现能力。
- [ ] 未执行的构建、测试和 verifier 均明确 verificationStatus: not-run；证据缺口、冲突和 blocking-decision 已单列。
- [ ] 输出文件路径唯一为：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P20-diagnostics-tools-shared-public-decomposition.md

## 会话结束回报

最终消息只回报以下字段：
- partitionId: P20
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P20-diagnostics-tools-shared-public-decomposition.md
- evidenceSourcesRead: 实际读取的 Version4 文件/类型、必要的完整源码补证、tModLoader 页面和 SS14 参考
- evidenceGaps: 仍缺失或冲突的关键证据
- blockingDecisions: 需要最终整合会话裁决的 owner/顺序/事务边界
- verificationStatus: not-run
- productionCodeModified: no

本提示词只定义本分区的专属范围和审查抓手；公共协议负责共同证据规则、报告结构、禁止项和 Integration Handoff。
