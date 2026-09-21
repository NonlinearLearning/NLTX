# Version4 非权威组件拆分分区 P09：物品、库存与容器 - public-decomposition 专属提示词

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
- 必须先按 skill 使用 ClaimNext（或用户明确指定时使用 Claim -PartitionId P09），只写本文件的 outputReport，并用领取返回的同一 sessionId 完成 Complete/Fail/Abandon；不得手工编辑 ledger 或 lock。
- 本任务属于 20 个并行会话中的固定分区；currentTaskCount: 20。公共协议中遗留的 19 会话数量说明在本任务包中不适用。

## 任务元数据

- promptId: P09-non-authoritative-public-decomposition-20260911
- partitionId: P09
- inputPartitionReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\09-item-inventory-containers.md
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P09-item-inventory-containers-public-decomposition.md
- reportType: public-decomposition
- executionMode: independent-read-only-partition
- currentTaskCount: 20
- formalParentSubsystems: SharedRuntimeMechanisms, WorldStorage
- leafSubsystemCount: 14
- fieldCount: 152
- propertyCount: 39
- memberCount: 191
- sourceReportSha256: b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196
- sourceInventoryStatus: source-inventory-confirmed; member-level owner/lifecycle/side-effect evidence still requires this session to close

## 专属范围锁定

- 本会话只审查输入分区报告中的 14 个叶子子系统和 191 条成员记录。以下清单是本会话唯一的成员范围；不得读取或复制其他 P 分区的成员清单。
- 细分子系统仍只是库存边界，不等价于一个组件；允许在本分区内部按访问模式拆成多个 proposed Component、System、Query、Command、Adapter 或 Projection。
- 来源序号、路径、行号和声明是检索线索；必须回到 Version4 重新定位，不得把输入报告直接当作已确认的读者、写者、生命周期或 owner 证据。

| 叶子子系统 | 父级 | 原章节 | 边界角色 | 字段 | 属性 | 成员 | 细分职责 |
|---|---|---|---|---:|---:|---:|---|
| `ChestContainerStorage` | `WorldStorage` | `4.5.1` | `authoritative snapshot` | 23 | 0 | 23 | Chest 容器和槽位字段。 |
| `SharedItemIdentityAndStackState` | `SharedRuntimeMechanisms` | `4.9.16` | `state` | 9 | 5 | 14 | 物品身份、实例数量、堆叠和变体身份。 |
| `SharedItemProgressionAndWorldInteractionState` | `SharedRuntimeMechanisms` | `4.9.17` | `definition/state` | 9 | 0 | 9 | 任务、特殊使用、钓鱼、工具和 NPC 生成能力。 |
| `SharedItemBuffMountAndConsumableEffects` | `SharedRuntimeMechanisms` | `4.9.19` | `definition/state` | 6 | 0 | 6 | Buff、坐骑、宠物和特殊消耗效果状态。 |
| `SharedItemDerivedQueries` | `SharedRuntimeMechanisms` | `4.9.20` | `derived/query` | 0 | 3 | 3 | 从内容目录或实例状态派生的物品查询属性。 |
| `ItemQuickStackingState` | `SharedRuntimeMechanisms` | `4.9.67` | `state/query` | 17 | 0 | 17 | 快速堆叠源、目标和匹配缓存状态。 |
| `ItemTransferSettings` | `SharedRuntimeMechanisms` | `4.9.68` | `definition/state` | 16 | 0 | 16 | 物品领取和转移行为设置。 |
| `SharedWorldItemLifecycleState` | `SharedRuntimeMechanisms` | `4.9.126` | `state` | 15 | 1 | 16 | 世界物品保留、拾取、传送带和生命周期状态。 |
| `SharedItemEquipmentSlotState` | `SharedRuntimeMechanisms` | `4.9.132` | `definition/state` | 21 | 0 | 21 | 装备栏位、染料栏位和穿戴槽状态。 |
| `SharedWorldItemIdentityAndEconomyPayloadState` | `SharedRuntimeMechanisms` | `4.9.188` | `state/projection` | 0 | 9 | 9 | 世界物品身份、堆叠、价值和经济投影载荷。 |
| `SharedWorldItemUseAndPresentationPayloadState` | `SharedRuntimeMechanisms` | `4.9.189` | `state/projection` | 0 | 19 | 19 | 世界物品使用、伤害、工具和表现投影载荷。 |
| `SharedItemEmergencyStackingPolicyState` | `SharedRuntimeMechanisms` | `4.9.190` | `state/query` | 6 | 0 | 6 | 紧急堆叠候选、距离和策略配置。 |
| `SharedItemEmergencyStackingTransferState` | `SharedRuntimeMechanisms` | `4.9.191` | `state/query` | 19 | 2 | 21 | 紧急堆叠 Group、StackableItem 和 Transfer 运行态。 |
| `SharedItemToolPlacementCapabilityState` | `SharedRuntimeMechanisms` | `4.9.215` | `definition/state` | 11 | 0 | 11 | 物品采掘、放置、弹药和工具能力。 |

来源成员的分区内序号线索范围：765..4053；序号不定义领域边界，必须逐条回到输入表和 Version4 覆盖。

## 分区专属目标

围绕“物品、库存与容器”完成独立只读的 Version4 成员审查和 ECS 拆分设计。
- 围绕 Chest 容器、Item identity/stack、装备槽、世界物品生命周期、快速/紧急堆叠、物品转移、工具放置能力和使用/表现 payload，核对物品实例、槽位、容器和世界掉落的权威所有权。
- 回到 Terraria/Chest.cs、Item、Player、容器/槽位/堆叠相关源码和直接调用者，确认创建、转移、堆叠、拆分、拾取、销毁、保存和网络同步路径。
- 把 ItemInstance、Inventory/Chest storage、Equipment slot、Transfer Command、Stacking Query/Policy、Tool capability、世界物品和客户端 tooltip/表现 Projection 分开评估。
- 核对物品身份、经济价值、网络/持久化快照、容器锁定、失败回滚和并发/重复转移边界；跨分区只提交候选 owner。

## 专属证据焦点

- 先完整读取本分区输入报告的成员表，再回到 D:\TRbackup\Version4 重新核对字段、属性、声明类型、初始化/注册、直接读者、直接写者、清理路径和副作用。
- 下列路径只作为本分区的检索种子；路径存在不等于成员语义已确认，必须按公共证据协议记录实际行号、符号、调用者、读者、写者、生命周期和 evidenceStatus。
  - `D:\TRbackup\Version4\Terraria.GameContent/EmergencyStacking.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/QuickStacking.cs`
  - `D:\TRbackup\Version4\Terraria/Chest.cs`
  - `D:\TRbackup\Version4\Terraria/GetItemSettings.cs`
  - `D:\TRbackup\Version4\Terraria/Item.cs`
  - `D:\TRbackup\Version4\Terraria/WorldItem.cs`

- 输入分区抽取出的声明类型焦点：
  - `Terraria.Chest`
  - `Terraria.GameContent.EmergencyStacking`
  - `Terraria.GameContent.EmergencyStacking.Group`
  - `Terraria.GameContent.EmergencyStacking.StackableItem`
  - `Terraria.GameContent.EmergencyStacking.Transfer`
  - `Terraria.GameContent.QuickStacking`
  - `Terraria.GameContent.QuickStacking.DestinationHelper`
  - `Terraria.GameContent.QuickStacking.MatchingItemTypeDestinationList`
  - `Terraria.GameContent.QuickStacking.MatchingItemTypeDestinationList.LinkedEntry`
  - `Terraria.GameContent.QuickStacking.SourceInventory`
  - `Terraria.GetItemSettings`
  - `Terraria.Item`
  - `Terraria.WorldItem`

- 若路径、类型或行号漂移，记录 version-drift 或 evidence-mismatch；不得静默沿用库存报告行号。Version4 是 Terraria 私有实现的事实来源，完整源码只补证 Version4 已存在文件，tModLoader 只交叉验证公开边界，SS14 只参考结构粒度。

## 专属审查问题

- 物品实例身份、堆叠数量、变体、槽位和容器存储的写入根分别是什么？槽位索引、实例 ID、网络 ID 和持久化 ID 是否混淆？
- 快速堆叠、紧急堆叠、领取、转移、拾取和装备切换是 Query/Policy 还是提交 Command？失败与回滚如何保证不重复扣减/复制？
- 世界物品生命周期、装备槽、工具/弹药能力和使用表现 payload 是否有不同的实体生命周期？
- Chest、Player、Item、Economy、Crafting、Network、Persistence 和 UI 之间的存储/投影边界如何保持单向？

## 专属不拆分边界

- 不要把 Item identity、stack、slot、Chest、equipment、world item、tool capability 和 tooltip 合成一个巨型 ItemComponent。
- 不要把堆叠候选、TransferGroup、鼠标物品、排序缓存、tooltip payload 或单次 pickup 结果当作权威实例状态。
- 不要在本分区宣布 ItemInstanceId、ContainerId、SlotReference 或世界物品 owner；跨域候选必须 crossSubsystemOwner: integration-review。

专属跨域提醒：重点记录与玩家输入、经济/制作/钓鱼、TileEntity 容器、战斗、网络和持久化的 integration-risk；库存事务、世界掉落和客户端物品投影的提交顺序必须明确。

## 输出要求

- 将完整研究报告只写入：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P09-item-inventory-containers-public-decomposition.md
- 不要写入输入分区报告、公共提示词、其他分区报告、源代码、测试、项目文件或迁移文件。
- 报告必须逐条覆盖本分区成员，给出成员归属表、状态所有权、Component/System/Query/Command/Adapter/Projection 边界、接口契约、依赖方向、顺序约束、不拆分项、兼容策略、evidence-gap、blocking-decision、focused verifier 计划和 Integration Handoff。
- 本轮只读审查不运行编译型命令；没有实际执行的 verifier 必须写 verificationStatus: not-run。
- proposed 类型、路径和接口必须明确标记 proposed；不得写成当前 NLTX 已存在或已经迁移。
- 跨两个或以上分区使用的类型、接口、ID、快照、值对象和 System 顺序只能提出候选，必须写 crossSubsystemOwner: integration-review，不得在本报告单方面裁决。
- 不得修改任何生产代码、测试、项目文件、生成器、公共协议、分区报告或其他并行会话输出。

## 分区完成检查

- [ ] 逐条覆盖本分区全部 191 条成员，成员事实来自输入表但关键语义已回到 Version4 复核。
- [ ] 只使用本分区叶子清单；没有复制、重新设计或宣布其他 P 分区成员。
- [ ] Component 只承载内聚权威状态；派生值、缓存、查询、短期 payload、日志、网络/存档/UI 输出按正确角色隔离。
- [ ] 所有共享候选写 crossSubsystemOwner: integration-review；没有把 proposed 设计写成已实现能力。
- [ ] 未执行的构建、测试和 verifier 均明确 verificationStatus: not-run；证据缺口、冲突和 blocking-decision 已单列。
- [ ] 输出文件路径唯一为：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P09-item-inventory-containers-public-decomposition.md

## 会话结束回报

最终消息只回报以下字段：
- partitionId: P09
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P09-item-inventory-containers-public-decomposition.md
- evidenceSourcesRead: 实际读取的 Version4 文件/类型、必要的完整源码补证、tModLoader 页面和 SS14 参考
- evidenceGaps: 仍缺失或冲突的关键证据
- blockingDecisions: 需要最终整合会话裁决的 owner/顺序/事务边界
- verificationStatus: not-run
- productionCodeModified: no

本提示词只定义本分区的专属范围和审查抓手；公共协议负责共同证据规则、报告结构、禁止项和 Integration Handoff。
