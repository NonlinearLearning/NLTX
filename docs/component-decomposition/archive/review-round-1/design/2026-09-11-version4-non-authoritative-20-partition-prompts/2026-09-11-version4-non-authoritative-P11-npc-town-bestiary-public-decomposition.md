# Version4 非权威组件拆分分区 P11：NPC、城镇与图鉴 - public-decomposition 专属提示词

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
- 必须先按 skill 使用 ClaimNext（或用户明确指定时使用 Claim -PartitionId P11），只写本文件的 outputReport，并用领取返回的同一 sessionId 完成 Complete/Fail/Abandon；不得手工编辑 ledger 或 lock。
- 本任务属于 20 个并行会话中的固定分区；currentTaskCount: 20。公共协议中遗留的 19 会话数量说明在本任务包中不适用。

## 任务元数据

- promptId: P11-non-authoritative-public-decomposition-20260911
- partitionId: P11
- inputPartitionReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\11-npc-town-bestiary.md
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P11-npc-town-bestiary-public-decomposition.md
- reportType: public-decomposition
- executionMode: independent-read-only-partition
- currentTaskCount: 20
- formalParentSubsystems: SharedRuntimeMechanisms
- leafSubsystemCount: 8
- fieldCount: 71
- propertyCount: 22
- memberCount: 93
- sourceReportSha256: b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196
- sourceInventoryStatus: source-inventory-confirmed; member-level owner/lifecycle/side-effect evidence still requires this session to close

## 专属范围锁定

- 本会话只审查输入分区报告中的 8 个叶子子系统和 93 条成员记录。以下清单是本会话唯一的成员范围；不得读取或复制其他 P 分区的成员清单。
- 细分子系统仍只是库存边界，不等价于一个组件；允许在本分区内部按访问模式拆成多个 proposed Component、System、Query、Command、Adapter 或 Projection。
- 来源序号、路径、行号和声明是检索线索；必须回到 Version4 重新定位，不得把输入报告直接当作已确认的读者、写者、生命周期或 owner 证据。

| 叶子子系统 | 父级 | 原章节 | 边界角色 | 字段 | 属性 | 成员 | 细分职责 |
|---|---|---|---|---:|---:|---:|---|
| `SharedConditionalDialogueSupport` | `SharedRuntimeMechanisms` | `4.9.27` | `definition/query` | 8 | 1 | 9 | 条件对话和 Lucy 交互消息。 |
| `SharedTownRoomState` | `SharedRuntimeMechanisms` | `4.9.28` | `state` | 3 | 0 | 3 | 城镇房间和 NPC 房屋状态。 |
| `SharedNpcPersonalityCatalog` | `SharedRuntimeMechanisms` | `4.9.52` | `definition/catalog` | 18 | 1 | 19 | NPC 个性偏好和城镇档案。 |
| `BestiaryCatalogAndEntries` | `SharedRuntimeMechanisms` | `4.9.64` | `definition/catalog` | 6 | 2 | 8 | 图鉴数据库和图鉴条目目录。 |
| `BestiaryUnlockTracking` | `SharedRuntimeMechanisms` | `4.9.65` | `state/query` | 14 | 1 | 15 | 图鉴击杀、接近、对话和解锁进度跟踪。 |
| `BestiaryFiltersAndSorting` | `SharedRuntimeMechanisms` | `4.9.66` | `query` | 1 | 11 | 12 | 图鉴筛选器和排序步骤。 |
| `SharedBestiaryInfoElementState` | `SharedRuntimeMechanisms` | `4.9.192` | `definition/presentation` | 14 | 3 | 17 | 图鉴信息元素、显示事实和元素索引。 |
| `SharedBestiaryCollectionProviderState` | `SharedRuntimeMechanisms` | `4.9.193` | `definition/presentation` | 7 | 3 | 10 | 图鉴集合信息、UICollection provider 和集合接口。 |

来源成员的分区内序号线索范围：1357..3849；序号不定义领域边界，必须逐条回到输入表和 Version4 覆盖。

## 分区专属目标

围绕“NPC、城镇与图鉴”完成独立只读的 Version4 成员审查和 ECS 拆分设计。
- 围绕条件对话、Lucy 交互、城镇房间/NPC 个性档案、图鉴目录/条目、解锁跟踪、筛选排序、信息元素和 UICollection provider，核对内容定义、进度状态、查询与客户端投影的边界。
- 回到各对话、Town、Bestiary、集合 provider 和直接调用者，确认 NPC/图鉴数据的创建、注册、解锁、更新、持久化、网络和 UI 使用路径。
- 分别评估对话条件 Query、城镇房间权威/缓存、NPC personality catalog、Bestiary definition/catalog、Bestiary unlock progress、sorting/filter Query 和 UI Projection。
- 核对击杀/接近/对话触发图鉴进度时的事件方向、重复解锁、存档恢复、多人同步和客户端集合构建，避免把展示模型当成模拟状态。

## 专属证据焦点

- 先完整读取本分区输入报告的成员表，再回到 D:\TRbackup\Version4 重新核对字段、属性、声明类型、初始化/注册、直接读者、直接写者、清理路径和副作用。
- 下列路径只作为本分区的检索种子；路径存在不等于成员语义已确认，必须按公共证据协议记录实际行号、符号、调用者、读者、写者、生命周期和 evidenceStatus。
  - `D:\TRbackup\Version4\Terraria.GameContent.Bestiary/BestiaryDatabase.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Bestiary/BestiaryEntry.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Bestiary/BestiaryUICollectionInfo.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Bestiary/BestiaryUnlockProgressReport.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Bestiary/BestiaryUnlocksTracker.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Bestiary/CommonEnemyUICollectionInfoProvider.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Bestiary/CritterUICollectionInfoProvider.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Bestiary/Filters.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Bestiary/FlavorTextBestiaryInfoElement.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Bestiary/IBestiaryEntryDisplayIndex.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Bestiary/IBestiaryEntryFilter.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Bestiary/IBestiarySortStep.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Bestiary/ItemDropBestiaryInfoElement.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Bestiary/NamePlateInfoElement.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Bestiary/NPCKillCounterInfoElement.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Bestiary/NPCKillsTracker.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Bestiary/NPCNetIdBestiaryInfoElement.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Bestiary/NPCPortraitInfoElement.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Bestiary/NPCStatsReportInfoElement.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Bestiary/NPCWasChatWithTracker.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Bestiary/NPCWasNearPlayerTracker.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Bestiary/RareSpawnBestiaryInfoElement.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Bestiary/SortingSteps.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Bestiary/TownNPCUICollectionInfoProvider.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Personalities/AShoppingBiome.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Personalities/BiomePreferenceListTrait.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Personalities/HelperInfo.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Personalities/PersonalityDatabase.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Personalities/PersonalityDatabasePopulator.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Personalities/PersonalityProfile.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/ConditionalDialogue.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/LucyAxeMessage.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/TownNPCProfiles.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/TownRoomManager.cs`

- 输入分区抽取出的声明类型焦点：
  - `Terraria.GameContent.Bestiary.BestiaryDatabase`
  - `Terraria.GameContent.Bestiary.BestiaryEntry`
  - `Terraria.GameContent.Bestiary.BestiaryUICollectionInfo`
  - `Terraria.GameContent.Bestiary.BestiaryUnlockProgressReport`
  - `Terraria.GameContent.Bestiary.BestiaryUnlocksTracker`
  - `Terraria.GameContent.Bestiary.CommonEnemyUICollectionInfoProvider`
  - `Terraria.GameContent.Bestiary.CritterUICollectionInfoProvider`
  - `Terraria.GameContent.Bestiary.Filters.ByBoss`
  - `Terraria.GameContent.Bestiary.Filters.ByInfoElement`
  - `Terraria.GameContent.Bestiary.Filters.ByRareCreature`
  - `Terraria.GameContent.Bestiary.Filters.BySearch`
  - `Terraria.GameContent.Bestiary.Filters.ByUnlockState`
  - `Terraria.GameContent.Bestiary.FlavorTextBestiaryInfoElement`
  - `Terraria.GameContent.Bestiary.IBestiaryEntryDisplayIndex`
  - `Terraria.GameContent.Bestiary.IBestiaryEntryFilter`
  - `Terraria.GameContent.Bestiary.IBestiarySortStep`
  - `Terraria.GameContent.Bestiary.ItemDropBestiaryInfoElement`
  - `Terraria.GameContent.Bestiary.NamePlateInfoElement`
  - `Terraria.GameContent.Bestiary.NPCKillCounterInfoElement`
  - `Terraria.GameContent.Bestiary.NPCKillsTracker`
  - `Terraria.GameContent.Bestiary.NPCNetIdBestiaryInfoElement`
  - `Terraria.GameContent.Bestiary.NPCPortraitInfoElement`
  - `Terraria.GameContent.Bestiary.NPCStatsReportInfoElement`
  - `Terraria.GameContent.Bestiary.NPCWasChatWithTracker`
  - `Terraria.GameContent.Bestiary.NPCWasNearPlayerTracker`
  - `Terraria.GameContent.Bestiary.RareSpawnBestiaryInfoElement`
  - `Terraria.GameContent.Bestiary.SortingSteps.Alphabetical`
  - `Terraria.GameContent.Bestiary.SortingSteps.ByBestiaryRarity`
  - `Terraria.GameContent.Bestiary.SortingSteps.ByBestiarySortingId`
  - `Terraria.GameContent.Bestiary.SortingSteps.ByNetId`
  - `Terraria.GameContent.Bestiary.SortingSteps.ByStat`
  - `Terraria.GameContent.Bestiary.SortingSteps.ByUnlockState`
  - `Terraria.GameContent.Bestiary.TownNPCUICollectionInfoProvider`
  - `Terraria.GameContent.ConditionalDialogue`
  - `Terraria.GameContent.ConditionalDialogue.ItemGroups`
  - `Terraria.GameContent.LucyAxeMessage`
  - `Terraria.GameContent.Personalities.AShoppingBiome`
  - `Terraria.GameContent.Personalities.BiomePreferenceListTrait`
  - `Terraria.GameContent.Personalities.BiomePreferenceListTrait.BiomePreference`
  - `Terraria.GameContent.Personalities.HelperInfo`
  - `Terraria.GameContent.Personalities.PersonalityDatabase`
  - `Terraria.GameContent.Personalities.PersonalityDatabasePopulator`
  - `Terraria.GameContent.Personalities.PersonalityProfile`
  - `Terraria.GameContent.TownNPCProfiles`
  - `Terraria.GameContent.TownRoomManager`

- 若路径、类型或行号漂移，记录 version-drift 或 evidence-mismatch；不得静默沿用库存报告行号。Version4 是 Terraria 私有实现的事实来源，完整源码只补证 Version4 已存在文件，tModLoader 只交叉验证公开边界，SS14 只参考结构粒度。

## 专属审查问题

- NPC 个性、城镇房间、条件对话和图鉴条目分别是定义目录、世界/玩家进度还是纯资格查询？谁是唯一写入根？
- 图鉴解锁的击杀、接近、对话事件如何幂等合并，何时持久化/同步，哪些只是 UI 集合快照？
- 筛选、排序、InfoElement 和 UICollection provider 是否只读；如果缓存存在，失效条件与重建 System 是什么？
- 本分区与实体、战斗/击杀、物品/掉落、网络、存档和 UI 的跨域引用、事件及 owner 如何交接？

## 专属不拆分边界

- 不要把 NPC 个性、城镇房间、图鉴目录、解锁进度、筛选器和 UI 元素合为 Bestiary/Npc 巨型组件。
- 不要把单次对话消息、排序步骤、筛选缓存、InfoElement 或 UICollection provider 误升格为权威进度状态。
- 不要以图鉴 UI 中存在条目推断服务端解锁已持久化或当前 NLTX 已实现。

专属跨域提醒：重点记录与实体、战斗/击杀、物品/掉落、网络、存档和 UI 的 integration-risk；NPC/Bestiary progress、catalog、collection snapshot 的共享 owner 标 crossSubsystemOwner: integration-review。

## 输出要求

- 将完整研究报告只写入：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P11-npc-town-bestiary-public-decomposition.md
- 不要写入输入分区报告、公共提示词、其他分区报告、源代码、测试、项目文件或迁移文件。
- 报告必须逐条覆盖本分区成员，给出成员归属表、状态所有权、Component/System/Query/Command/Adapter/Projection 边界、接口契约、依赖方向、顺序约束、不拆分项、兼容策略、evidence-gap、blocking-decision、focused verifier 计划和 Integration Handoff。
- 本轮只读审查不运行编译型命令；没有实际执行的 verifier 必须写 verificationStatus: not-run。
- proposed 类型、路径和接口必须明确标记 proposed；不得写成当前 NLTX 已存在或已经迁移。
- 跨两个或以上分区使用的类型、接口、ID、快照、值对象和 System 顺序只能提出候选，必须写 crossSubsystemOwner: integration-review，不得在本报告单方面裁决。
- 不得修改任何生产代码、测试、项目文件、生成器、公共协议、分区报告或其他并行会话输出。

## 分区完成检查

- [ ] 逐条覆盖本分区全部 93 条成员，成员事实来自输入表但关键语义已回到 Version4 复核。
- [ ] 只使用本分区叶子清单；没有复制、重新设计或宣布其他 P 分区成员。
- [ ] Component 只承载内聚权威状态；派生值、缓存、查询、短期 payload、日志、网络/存档/UI 输出按正确角色隔离。
- [ ] 所有共享候选写 crossSubsystemOwner: integration-review；没有把 proposed 设计写成已实现能力。
- [ ] 未执行的构建、测试和 verifier 均明确 verificationStatus: not-run；证据缺口、冲突和 blocking-decision 已单列。
- [ ] 输出文件路径唯一为：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P11-npc-town-bestiary-public-decomposition.md

## 会话结束回报

最终消息只回报以下字段：
- partitionId: P11
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P11-npc-town-bestiary-public-decomposition.md
- evidenceSourcesRead: 实际读取的 Version4 文件/类型、必要的完整源码补证、tModLoader 页面和 SS14 参考
- evidenceGaps: 仍缺失或冲突的关键证据
- blockingDecisions: 需要最终整合会话裁决的 owner/顺序/事务边界
- verificationStatus: not-run
- productionCodeModified: no

本提示词只定义本分区的专属范围和审查抓手；公共协议负责共同证据规则、报告结构、禁止项和 Integration Handoff。
