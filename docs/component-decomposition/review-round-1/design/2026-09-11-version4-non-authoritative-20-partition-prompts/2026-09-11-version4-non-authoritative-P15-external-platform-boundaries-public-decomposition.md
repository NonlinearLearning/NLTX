# Version4 非权威组件拆分分区 P15：外部平台与协议边界 - public-decomposition 专属提示词

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
- 必须先按 skill 使用 ClaimNext（或用户明确指定时使用 Claim -PartitionId P15），只写本文件的 outputReport，并用领取返回的同一 sessionId 完成 Complete/Fail/Abandon；不得手工编辑 ledger 或 lock。
- 本任务属于 20 个并行会话中的固定分区；currentTaskCount: 20。公共协议中遗留的 19 会话数量说明在本任务包中不适用。

## 任务元数据

- promptId: P15-non-authoritative-public-decomposition-20260911
- partitionId: P15
- inputPartitionReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\15-external-platform-boundaries.md
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P15-external-platform-boundaries-public-decomposition.md
- reportType: public-decomposition
- executionMode: independent-read-only-partition
- currentTaskCount: 20
- formalParentSubsystems: ExternalBoundaries, ExternalDependencyOrGenerated, RuntimeComposition, SharedRuntimeMechanisms
- leafSubsystemCount: 7
- fieldCount: 51
- propertyCount: 8
- memberCount: 59
- sourceReportSha256: b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196
- sourceInventoryStatus: source-inventory-confirmed; member-level owner/lifecycle/side-effect evidence still requires this session to close

## 专属范围锁定

- 本会话只审查输入分区报告中的 7 个叶子子系统和 59 条成员记录。以下清单是本会话唯一的成员范围；不得读取或复制其他 P 分区的成员清单。
- 细分子系统仍只是库存边界，不等价于一个组件；允许在本分区内部按访问模式拆成多个 proposed Component、System、Query、Command、Adapter 或 Projection。
- 来源序号、路径、行号和声明是检索线索；必须回到 Version4 重新定位，不得把输入报告直接当作已确认的读者、写者、生命周期或 owner 证据。

| 叶子子系统 | 父级 | 原章节 | 边界角色 | 字段 | 属性 | 成员 | 细分职责 |
|---|---|---|---|---:|---:|---:|---|
| `MainPlatformExecutionAdapter` | `RuntimeComposition` | `4.1.2` | `platform adapter` | 2 | 0 | 2 | 平台线程保持常量和原生调用边界。 |
| `SocialApiRegistry` | `ExternalBoundaries` | `4.8.1` | `adapter` | 6 | 1 | 7 | 社交提供程序注册和全局 API 状态。 |
| `SocialTransportIpc` | `ExternalBoundaries` | `4.8.2` | `adapter` | 8 | 1 | 9 | WeGame IPC 传输边界。 |
| `WorkshopAndJoinBoundaryData` | `ExternalBoundaries` | `4.8.3` | `adapter DTO` | 15 | 2 | 17 | 创意工坊、联机请求和富状态边界数据。 |
| `SharedResourcePackAdapters` | `SharedRuntimeMechanisms` | `4.9.58` | `adapter` | 10 | 0 | 10 | 资源包及资源包列表适配。 |
| `CryptographicDependency` | `ExternalDependencyOrGenerated` | `4.10.1` | `external dependency` | 10 | 0 | 10 | BCrypt 第三方实现字段。 |
| `NatPortMappingInterop` | `ExternalDependencyOrGenerated` | `4.10.2` | `external adapter` | 0 | 4 | 4 | NAT/UPnP 端口映射互操作属性。 |

来源成员的分区内序号线索范围：5..4067；序号不定义领域边界，必须逐条回到输入表和 Version4 覆盖。

## 分区专属目标

围绕“外部平台与协议边界”完成独立只读的 Version4 成员审查和 ECS 拆分设计。
- 围绕平台执行适配、Social API registry、WeGame IPC、Workshop/join boundary、resource pack、BCrypt 和 NAT/UPnP interop，核对外部依赖的生命周期、能力注册和失败隔离。
- 回到 Main 平台调用、SocialAPI、IPC、Workshop/Join、资源包、加密和 NAT/UPnP 相关源码，确认实际接口、初始化/关闭、线程/回调、句柄所有权和异常处理。
- 将第三方对象、原生句柄、外部 ID、回调载荷和资源路径封装在 Adapter/Port；核心 ECS 只接收稳定值对象、Command 或 Projection。
- 核对平台不可用、版本变化、超时、取消、重复回调、资源包加载失败和端口映射失败时的重试/降级/清理策略。

## 专属证据焦点

- 先完整读取本分区输入报告的成员表，再回到 D:\TRbackup\Version4 重新核对字段、属性、声明类型、初始化/注册、直接读者、直接写者、清理路径和副作用。
- 下列路径只作为本分区的检索种子；路径存在不等于成员语义已确认，必须按公共证据协议记录实际行号、符号、调用者、读者、写者、生命周期和 evidenceStatus。
  - `D:\TRbackup\Version4\BCrypt.Net/BCrypt.cs`
  - `D:\TRbackup\Version4\NATUPNPLib/IStaticPortMapping.cs`
  - `D:\TRbackup\Version4\NATUPNPLib/IUPnPNAT.cs`
  - `D:\TRbackup\Version4\Terraria.IO/ResourcePack.cs`
  - `D:\TRbackup\Version4\Terraria.IO/ResourcePackList.cs`
  - `D:\TRbackup\Version4\Terraria.Social.Base/CloudSocialModule.cs`
  - `D:\TRbackup\Version4\Terraria.Social.Base/FoundWorkshopEntryInfo.cs`
  - `D:\TRbackup\Version4\Terraria.Social.Base/RichPresenceState.cs`
  - `D:\TRbackup\Version4\Terraria.Social.Base/ServerJoinRequestsManager.cs`
  - `D:\TRbackup\Version4\Terraria.Social.Base/UserJoinToServerRequest.cs`
  - `D:\TRbackup\Version4\Terraria.Social.Base/WorkshopIssueReporter.cs`
  - `D:\TRbackup\Version4\Terraria.Social.Base/WorkshopItemPublishSettings.cs`
  - `D:\TRbackup\Version4\Terraria.Social.Base/WorkshopTagOption.cs`
  - `D:\TRbackup\Version4\Terraria.Social.WeGame/IPCBase.cs`
  - `D:\TRbackup\Version4\Terraria.Social/SocialAPI.cs`
  - `D:\TRbackup\Version4\Terraria/Main.cs`

- 输入分区抽取出的声明类型焦点：
  - `BCrypt.Net.BCrypt`
  - `NATUPNPLib.IStaticPortMapping`
  - `NATUPNPLib.IUPnPNAT`
  - `Terraria.IO.ResourcePack`
  - `Terraria.IO.ResourcePackList`
  - `Terraria.Main.NativeMethods`
  - `Terraria.Social.Base.CloudSocialModule`
  - `Terraria.Social.Base.FoundWorkshopEntryInfo`
  - `Terraria.Social.Base.RichPresenceState`
  - `Terraria.Social.Base.ServerJoinRequestsManager`
  - `Terraria.Social.Base.UserJoinToServerRequest`
  - `Terraria.Social.Base.WorkshopIssueReporter`
  - `Terraria.Social.Base.WorkshopItemPublishSettings`
  - `Terraria.Social.Base.WorkshopTagOption`
  - `Terraria.Social.SocialAPI`
  - `Terraria.Social.WeGame.IPCBase`

- 若路径、类型或行号漂移，记录 version-drift 或 evidence-mismatch；不得静默沿用库存报告行号。Version4 是 Terraria 私有实现的事实来源，完整源码只补证 Version4 已存在文件，tModLoader 只交叉验证公开边界，SS14 只参考结构粒度。

## 专属审查问题

- Social API、IPC、Workshop、ResourcePack、Crypto、NAT/UPnP 的真正 owner 是宿主 Adapter、会话 System 还是领域状态？
- 注册表、提供程序列表、回调结果、原生句柄和外部 ID 的生命周期是否可独立管理？哪些必须在关闭时释放？
- 外部调用失败、超时和重试如何避免把第三方状态写进权威模拟；是否需要 idempotency key 或明确取消边界？
- 平台能力与网络会话、存档配置、内容目录、UI 之间的契约哪些需要整合会话裁决？

## 专属不拆分边界

- 不要把平台句柄、IPC buffer、Workshop response、NAT mapping 或第三方对象放入 ECS 核心 Component。
- 不要把 provider registry、回调 payload、资源列表缓存或 native field 误认为领域权威状态。
- 不要因为一个外部依赖只有少量字段就跳过 Adapter；外部类型渗透必须记录为边界风险。

专属跨域提醒：重点记录与网络会话、持久化配置、内容目录、UI 和宿主运行时的 integration-risk；ExternalId、PlatformHandle、ProviderRegistry owner 必须 crossSubsystemOwner: integration-review。

## 输出要求

- 将完整研究报告只写入：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P15-external-platform-boundaries-public-decomposition.md
- 不要写入输入分区报告、公共提示词、其他分区报告、源代码、测试、项目文件或迁移文件。
- 报告必须逐条覆盖本分区成员，给出成员归属表、状态所有权、Component/System/Query/Command/Adapter/Projection 边界、接口契约、依赖方向、顺序约束、不拆分项、兼容策略、evidence-gap、blocking-decision、focused verifier 计划和 Integration Handoff。
- 本轮只读审查不运行编译型命令；没有实际执行的 verifier 必须写 verificationStatus: not-run。
- proposed 类型、路径和接口必须明确标记 proposed；不得写成当前 NLTX 已存在或已经迁移。
- 跨两个或以上分区使用的类型、接口、ID、快照、值对象和 System 顺序只能提出候选，必须写 crossSubsystemOwner: integration-review，不得在本报告单方面裁决。
- 不得修改任何生产代码、测试、项目文件、生成器、公共协议、分区报告或其他并行会话输出。

## 分区完成检查

- [ ] 逐条覆盖本分区全部 59 条成员，成员事实来自输入表但关键语义已回到 Version4 复核。
- [ ] 只使用本分区叶子清单；没有复制、重新设计或宣布其他 P 分区成员。
- [ ] Component 只承载内聚权威状态；派生值、缓存、查询、短期 payload、日志、网络/存档/UI 输出按正确角色隔离。
- [ ] 所有共享候选写 crossSubsystemOwner: integration-review；没有把 proposed 设计写成已实现能力。
- [ ] 未执行的构建、测试和 verifier 均明确 verificationStatus: not-run；证据缺口、冲突和 blocking-decision 已单列。
- [ ] 输出文件路径唯一为：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P15-external-platform-boundaries-public-decomposition.md

## 会话结束回报

最终消息只回报以下字段：
- partitionId: P15
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P15-external-platform-boundaries-public-decomposition.md
- evidenceSourcesRead: 实际读取的 Version4 文件/类型、必要的完整源码补证、tModLoader 页面和 SS14 参考
- evidenceGaps: 仍缺失或冲突的关键证据
- blockingDecisions: 需要最终整合会话裁决的 owner/顺序/事务边界
- verificationStatus: not-run
- productionCodeModified: no

本提示词只定义本分区的专属范围和审查抓手；公共协议负责共同证据规则、报告结构、禁止项和 Integration Handoff。
