# Version4 非权威组件拆分分区 P14：持久化、恢复与配置 - public-decomposition 专属提示词

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
- 必须先按 skill 使用 ClaimNext（或用户明确指定时使用 Claim -PartitionId P14），只写本文件的 outputReport，并用领取返回的同一 sessionId 完成 Complete/Fail/Abandon；不得手工编辑 ledger 或 lock。
- 本任务属于 20 个并行会话中的固定分区；currentTaskCount: 20。公共协议中遗留的 19 会话数量说明在本任务包中不适用。

## 任务元数据

- promptId: P14-non-authoritative-public-decomposition-20260911
- partitionId: P14
- inputPartitionReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\14-persistence-recovery-configuration.md
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P14-persistence-recovery-configuration-public-decomposition.md
- reportType: public-decomposition
- executionMode: independent-read-only-partition
- currentTaskCount: 20
- formalParentSubsystems: PersistenceAndRecovery, RuntimeComposition, SharedRuntimeMechanisms
- leafSubsystemCount: 11
- fieldCount: 146
- propertyCount: 14
- memberCount: 160
- sourceReportSha256: b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196
- sourceInventoryStatus: source-inventory-confirmed; member-level owner/lifecycle/side-effect evidence still requires this session to close

## 专属范围锁定

- 本会话只审查输入分区报告中的 11 个叶子子系统和 160 条成员记录。以下清单是本会话唯一的成员范围；不得读取或复制其他 P 分区的成员清单。
- 细分子系统仍只是库存边界，不等价于一个组件；允许在本分区内部按访问模式拆成多个 proposed Component、System、Query、Command、Adapter 或 Projection。
- 来源序号、路径、行号和声明是检索线索；必须回到 Version4 重新定位，不得把输入报告直接当作已确认的读者、写者、生命周期或 owner 证据。

| 叶子子系统 | 父级 | 原章节 | 边界角色 | 字段 | 属性 | 成员 | 细分职责 |
|---|---|---|---|---:|---:|---:|---|
| `MainSaveFavoritesAndSessionRefs` | `RuntimeComposition` | `4.1.5` | `session state` | 10 | 0 | 10 | 收藏、世界文件元数据、成就和会话对象引用。 |
| `MainWorldPersistenceAndMetadata` | `RuntimeComposition` | `4.1.17` | `session state` | 15 | 0 | 15 | 回滚、地牢锚点、保存校验、路径和世界元数据。 |
| `PlayerFileMetadataAndSession` | `PersistenceAndRecovery` | `4.2.1` | `snapshot state` | 4 | 3 | 7 | 玩家存档元数据、路径和活动文件状态。 |
| `WorldFileMetadataIdentityState` | `PersistenceAndRecovery` | `4.2.2` | `snapshot state` | 14 | 0 | 14 | 世界尺寸、创建时间、种子原文和世界标识元数据。 |
| `WorldFileSessionAndValidityState` | `PersistenceAndRecovery` | `4.2.3` | `snapshot state` | 14 | 8 | 22 | 世界加载状态、模式开关、有效性和地图路径状态。 |
| `WorldFileTileHeaderCoreState` | `PersistenceAndRecovery` | `4.2.4` | `adapter state` | 19 | 0 | 19 | 世界 Tile 核心压缩头位和基础布局。 |
| `WorldFileTileHeaderExtensionState` | `PersistenceAndRecovery` | `4.2.5` | `adapter state` | 16 | 0 | 16 | 世界 Tile 扩展压缩头位和高阶标记布局。 |
| `WorldFileRecoveryVersionState` | `PersistenceAndRecovery` | `4.2.6` | `adapter state` | 5 | 0 | 5 | 世界文件锁、版本和云端恢复异常状态。 |
| `WorldFileTemporaryEventState` | `PersistenceAndRecovery` | `4.2.7` | `adapter state` | 23 | 0 | 23 | 世界文件恢复期间的天气、节日和事件临时值。 |
| `SharedSaveAndConfigurationAdapters` | `SharedRuntimeMechanisms` | `4.9.57` | `adapter` | 22 | 3 | 25 | 存档元数据、收藏和配置文件适配。 |
| `SharedGeneralFilePlatformUtilities` | `SharedRuntimeMechanisms` | `4.9.83` | `adapter` | 4 | 0 | 4 | 文件浏览、文件操作和运行时平台辅助工具。 |

来源成员的分区内序号线索范围：38..3892；序号不定义领域边界，必须逐条回到输入表和 Version4 覆盖。

## 分区专属目标

围绕“持久化、恢复与配置”完成独立只读的 Version4 成员审查和 ECS 拆分设计。
- 围绕玩家/世界文件元数据、保存会话引用、世界 Tile header、版本/锁/云恢复异常、临时事件值、配置/收藏适配和文件平台工具，核对持久化权威、恢复工作流与运行时状态。
- 回到世界/玩家文件数据、Main 保存字段、Save/Configuration Adapter、FileUtilities 和平台调用者，确认序列化入口、临时文件、原子提交、加载校验、回滚和清理。
- 分别评估 persisted component/snapshot、file metadata value object、header encoding、recovery state、temporary load context、configuration Adapter 和 validation Query。
- 核对存档字段与运行时权威状态、网络快照、Tile/Item/Player 领域的映射，记录版本漂移、损坏文件、云恢复失败和重试语义。

## 专属证据焦点

- 先完整读取本分区输入报告的成员表，再回到 D:\TRbackup\Version4 重新核对字段、属性、声明类型、初始化/注册、直接读者、直接写者、清理路径和副作用。
- 下列路径只作为本分区的检索种子；路径存在不等于成员语义已确认，必须按公共证据协议记录实际行号、符号、调用者、读者、写者、生命周期和 evidenceStatus。
  - `D:\TRbackup\Version4\Terraria.IO/FavoritesFile.cs`
  - `D:\TRbackup\Version4\Terraria.IO/FileData.cs`
  - `D:\TRbackup\Version4\Terraria.IO/FileMetadata.cs`
  - `D:\TRbackup\Version4\Terraria.IO/GameConfiguration.cs`
  - `D:\TRbackup\Version4\Terraria.IO/PlayerFileData.cs`
  - `D:\TRbackup\Version4\Terraria.IO/Preferences.cs`
  - `D:\TRbackup\Version4\Terraria.IO/WorldFile.cs`
  - `D:\TRbackup\Version4\Terraria.IO/WorldFileData.cs`
  - `D:\TRbackup\Version4\Terraria.Utilities.FileBrowser/ExtensionFilter.cs`
  - `D:\TRbackup\Version4\Terraria.Utilities/FileUtilities.cs`
  - `D:\TRbackup\Version4\Terraria.Utilities/NewRuntimeMethods.cs`
  - `D:\TRbackup\Version4\Terraria/Main.cs`

- 输入分区抽取出的声明类型焦点：
  - `Terraria.IO.FavoritesFile`
  - `Terraria.IO.FileData`
  - `Terraria.IO.FileMetadata`
  - `Terraria.IO.GameConfiguration`
  - `Terraria.IO.PlayerFileData`
  - `Terraria.IO.Preferences`
  - `Terraria.IO.WorldFile`
  - `Terraria.IO.WorldFile.TilePacker`
  - `Terraria.IO.WorldFileData`
  - `Terraria.Main`
  - `Terraria.Utilities.FileBrowser.ExtensionFilter`
  - `Terraria.Utilities.FileUtilities`
  - `Terraria.Utilities.NewRuntimeMethods`

- 若路径、类型或行号漂移，记录 version-drift 或 evidence-mismatch；不得静默沿用库存报告行号。Version4 是 Terraria 私有实现的事实来源，完整源码只补证 Version4 已存在文件，tModLoader 只交叉验证公开边界，SS14 只参考结构粒度。

## 专属审查问题

- 世界/玩家元数据、Tile header、事件临时值、会话引用和配置文件分别是否持久化？写入根、提交点和恢复边界是什么？
- 文件锁、版本、有效性、回滚、云恢复异常和临时文件状态是否是 Adapter 状态而非模拟 Component？
- 序列化快照如何与 Tile、Item、Player、Event、Network ID 和外部文件路径解耦？字段缺失/旧版本如何兼容？
- 保存失败、部分写入、加载重试和恢复成功后的事件发布顺序如何验证，哪些决策需 integration-review？

## 专属不拆分边界

- 不要把所有文件元数据、Tile header、配置、恢复状态和运行时会话合为 PersistenceComponent。
- 不要把文件路径、锁句柄、压缩 header 位、临时恢复值或序列化 buffer 当作领域权威状态。
- 不要以文件存在、历史成功或 NLTX 目录结构替代当前逐字段持久化证据。

专属跨域提醒：重点记录与世界会话、Tile/存储、物品/玩家、网络、外部平台和配置的 integration-risk；PersistentEntityId、snapshot schema、WorldFileMetadata owner 标 crossSubsystemOwner: integration-review。

## 输出要求

- 将完整研究报告只写入：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P14-persistence-recovery-configuration-public-decomposition.md
- 不要写入输入分区报告、公共提示词、其他分区报告、源代码、测试、项目文件或迁移文件。
- 报告必须逐条覆盖本分区成员，给出成员归属表、状态所有权、Component/System/Query/Command/Adapter/Projection 边界、接口契约、依赖方向、顺序约束、不拆分项、兼容策略、evidence-gap、blocking-decision、focused verifier 计划和 Integration Handoff。
- 本轮只读审查不运行编译型命令；没有实际执行的 verifier 必须写 verificationStatus: not-run。
- proposed 类型、路径和接口必须明确标记 proposed；不得写成当前 NLTX 已存在或已经迁移。
- 跨两个或以上分区使用的类型、接口、ID、快照、值对象和 System 顺序只能提出候选，必须写 crossSubsystemOwner: integration-review，不得在本报告单方面裁决。
- 不得修改任何生产代码、测试、项目文件、生成器、公共协议、分区报告或其他并行会话输出。

## 分区完成检查

- [ ] 逐条覆盖本分区全部 160 条成员，成员事实来自输入表但关键语义已回到 Version4 复核。
- [ ] 只使用本分区叶子清单；没有复制、重新设计或宣布其他 P 分区成员。
- [ ] Component 只承载内聚权威状态；派生值、缓存、查询、短期 payload、日志、网络/存档/UI 输出按正确角色隔离。
- [ ] 所有共享候选写 crossSubsystemOwner: integration-review；没有把 proposed 设计写成已实现能力。
- [ ] 未执行的构建、测试和 verifier 均明确 verificationStatus: not-run；证据缺口、冲突和 blocking-decision 已单列。
- [ ] 输出文件路径唯一为：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P14-persistence-recovery-configuration-public-decomposition.md

## 会话结束回报

最终消息只回报以下字段：
- partitionId: P14
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P14-persistence-recovery-configuration-public-decomposition.md
- evidenceSourcesRead: 实际读取的 Version4 文件/类型、必要的完整源码补证、tModLoader 页面和 SS14 参考
- evidenceGaps: 仍缺失或冲突的关键证据
- blockingDecisions: 需要最终整合会话裁决的 owner/顺序/事务边界
- verificationStatus: not-run
- productionCodeModified: no

本提示词只定义本分区的专属范围和审查抓手；公共协议负责共同证据规则、报告结构、禁止项和 Integration Handoff。
