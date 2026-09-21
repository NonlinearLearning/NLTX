# Version4 非权威组件拆分分区 P17：UI 物品、排序与本地化 - public-decomposition 专属提示词

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
- 必须先按 skill 使用 ClaimNext（或用户明确指定时使用 Claim -PartitionId P17），只写本文件的 outputReport，并用领取返回的同一 sessionId 完成 Complete/Fail/Abandon；不得手工编辑 ledger 或 lock。
- 本任务属于 20 个并行会话中的固定分区；currentTaskCount: 20。公共协议中遗留的 19 会话数量说明在本任务包中不适用。

## 任务元数据

- promptId: P17-non-authoritative-public-decomposition-20260911
- partitionId: P17
- inputPartitionReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\17-ui-item-localization.md
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P17-ui-item-localization-public-decomposition.md
- reportType: public-decomposition
- executionMode: independent-read-only-partition
- currentTaskCount: 20
- formalParentSubsystems: ClientPresentationAndTools, SharedRuntimeMechanisms
- leafSubsystemCount: 22
- fieldCount: 279
- propertyCount: 17
- memberCount: 296
- sourceReportSha256: b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196
- sourceInventoryStatus: source-inventory-confirmed; member-level owner/lifecycle/side-effect evidence still requires this session to close

## 专属范围锁定

- 本会话只审查输入分区报告中的 22 个叶子子系统和 296 条成员记录。以下清单是本会话唯一的成员范围；不得读取或复制其他 P 分区的成员清单。
- 细分子系统仍只是库存边界，不等价于一个组件；允许在本分区内部按访问模式拆成多个 proposed Component、System、Query、Command、Adapter 或 Projection。
- 来源序号、路径、行号和声明是检索线索；必须回到 Version4 重新定位，不得把输入报告直接当作已确认的读者、写者、生命周期或 owner 证据。

| 叶子子系统 | 父级 | 原章节 | 边界角色 | 字段 | 属性 | 成员 | 细分职责 |
|---|---|---|---|---:|---:|---:|---|
| `SharedAchievementProgressSupport` | `SharedRuntimeMechanisms` | `4.9.2` | `state/query` | 7 | 2 | 9 | 成就条件、跟踪和完成进度支持。 |
| `LocalizationCultureAndLanguageState` | `SharedRuntimeMechanisms` | `4.9.69` | `projection/state` | 11 | 5 | 16 | 当前语言、文化注册表和语言管理器状态。 |
| `LocalizedTextValueState` | `SharedRuntimeMechanisms` | `4.9.70` | `projection/value object` | 18 | 4 | 22 | 本地化文本、变量文本和网络文本值对象。 |
| `LegacyLanguageCatalogState` | `SharedRuntimeMechanisms` | `4.9.71` | `projection/catalog` | 20 | 2 | 22 | 旧 Lang 文本目录和前缀文本组合状态。 |
| `SharedItemAppearanceAndTooltipState` | `SharedRuntimeMechanisms` | `4.9.133` | `presentation` | 16 | 1 | 17 | 物品颜色、声音、工具提示和外观表现状态。 |
| `SharedCreativePowerUiLayoutState` | `SharedRuntimeMechanisms` | `4.9.154` | `presentation` | 2 | 0 | 2 | 创意能力 UI 元素请求尺寸和布局参数。 |
| `SharedCreativePowerIconLocationCatalogState` | `SharedRuntimeMechanisms` | `4.9.196` | `presentation/catalog` | 23 | 0 | 23 | 创意能力图标分类、位置和图标资源目录。 |
| `SharedCreativePowerIconLayoutState` | `SharedRuntimeMechanisms` | `4.9.197` | `presentation` | 3 | 0 | 3 | 创意能力图标行列、布局尺寸和选中颜色状态。 |
| `AchievementPresentationState` | `ClientPresentationAndTools` | `4.11.1` | `presentation` | 21 | 1 | 22 | 成就定义、跟踪和客户端展示状态。 |
| `UiItemSlotTransferState` | `ClientPresentationAndTools` | `4.11.5` | `presentation/command` | 12 | 0 | 12 | 替代点击、物品转移和槽位交互载荷。 |
| `UiItemTooltipState` | `ClientPresentationAndTools` | `4.11.6` | `presentation` | 6 | 0 | 6 | 物品工具提示文本和校验状态。 |
| `UiItemSortingExecutionState` | `ClientPresentationAndTools` | `4.11.14` | `presentation state` | 7 | 0 | 7 | 一次排序批次、缓存槽位和弹药填充工作集。 |
| `UiItemSlotStorageAndCraftingContexts` | `ClientPresentationAndTools` | `4.11.15` | `presentation/query` | 13 | 0 | 13 | 背包、容器、商店、鼠标和制作上下文定义。 |
| `UiItemSlotPulseAndHighlightState` | `ClientPresentationAndTools` | `4.11.21` | `presentation state` | 16 | 1 | 17 | 物品槽高亮、发光颜色和脉冲动画状态。 |
| `UiItemSlotDisplayAndInteractionState` | `ClientPresentationAndTools` | `4.11.22` | `presentation state` | 10 | 0 | 10 | 物品槽显示选项、槽位引用和激活状态。 |
| `UiItemSortingRegistryAndRankingState` | `ClientPresentationAndTools` | `4.11.23` | `presentation/query` | 8 | 1 | 9 | 排序层注册表、伤害类型排名和排序层索引状态。 |
| `UiItemSortingConsumableAndMiscCatalogState` | `ClientPresentationAndTools` | `4.11.25` | `definition/catalog` | 25 | 0 | 25 | 药剂、消耗品、杂项和通用类别的排序层目录。 |
| `UiItemSlotEquipmentAndDisplayContexts` | `ClientPresentationAndTools` | `4.11.29` | `presentation/query` | 2 | 0 | 2 | 装备、染料、展示架和展示相关槽位上下文。 |
| `UiItemSlotCreativeAndCraftingContextState` | `ClientPresentationAndTools` | `4.11.35` | `presentation/query` | 6 | 0 | 6 | 创意无限、牺牲和新制作界面槽位上下文。 |
| `UiItemSlotHotbarDisplayAndUtilityContextState` | `ClientPresentationAndTools` | `4.11.36` | `presentation/query` | 24 | 0 | 24 | 快捷栏、聊天、装备展示、旗帜和通用槽位上下文。 |
| `UiItemSortingWeaponAndToolCatalogState` | `ClientPresentationAndTools` | `4.11.37` | `definition/catalog` | 21 | 0 | 21 | 物品排序的武器与工具层目录及其通用层定义。 |
| `UiItemSortingArmorAndAccessoryCatalogState` | `ClientPresentationAndTools` | `4.11.38` | `definition/catalog` | 8 | 0 | 8 | 物品排序的护甲、时装和装备层目录。 |

来源成员的分区内序号线索范围：1342..4537；序号不定义领域边界，必须逐条回到输入表和 Version4 覆盖。

## 分区专属目标

围绕“UI 物品、排序与本地化”完成独立只读的 Version4 成员审查和 ECS 拆分设计。
- 围绕语言/文化/LocalizedText/Lang、成就展示、物品外观与 tooltip、Creative Power UI、ItemSlot transfer/tooltip/sorting/storage/context/highlight 和排序目录，核对本地化值、UI 工作集、物品模拟状态与投影。
- 回到 Localization、Language、Lang、Item tooltip/slot/sorting、Achievement 和 Creative UI 源码，确认语言切换、文本网络值、排序批次、槽位生命周期、缓存失效和客户端更新。
- 分别评估本地化目录/值对象、物品 tooltip Projection、槽位交互 Command、排序 Query/执行工作集、Creative UI layout/catalog、成就定义与进度投影。
- 核对物品实例与 UI slot/context 的关系，语言/文本与网络/持久化的边界，以及异步资源/文化切换时的重建和失败策略。

## 专属证据焦点

- 先完整读取本分区输入报告的成员表，再回到 D:\TRbackup\Version4 重新核对字段、属性、声明类型、初始化/注册、直接读者、直接写者、清理路径和副作用。
- 下列路径只作为本分区的检索种子；路径存在不等于成员语义已确认，必须按公共证据协议记录实际行号、符号、调用者、读者、写者、生命周期和 evidenceStatus。
  - `D:\TRbackup\Version4\Terraria.Achievements/Achievement.cs`
  - `D:\TRbackup\Version4\Terraria.Achievements/AchievementCondition.cs`
  - `D:\TRbackup\Version4\Terraria.Achievements/AchievementManager.cs`
  - `D:\TRbackup\Version4\Terraria.Achievements/AchievementTracker.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Achievements/AchievementsHelper.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Achievements/CustomFloatCondition.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Creative/CreativePowersHelper.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Creative/CreativePowerUIElementRequestInfo.cs`
  - `D:\TRbackup\Version4\Terraria.Localization/GameCulture.cs`
  - `D:\TRbackup\Version4\Terraria.Localization/Language.cs`
  - `D:\TRbackup\Version4\Terraria.Localization/LanguageManager.cs`
  - `D:\TRbackup\Version4\Terraria.Localization/LocalizedText.cs`
  - `D:\TRbackup\Version4\Terraria.Localization/NetworkText.cs`
  - `D:\TRbackup\Version4\Terraria.Localization/VariableText.cs`
  - `D:\TRbackup\Version4\Terraria.UI/ItemSlot.cs`
  - `D:\TRbackup\Version4\Terraria.UI/ItemSorting.cs`
  - `D:\TRbackup\Version4\Terraria.UI/ItemTooltip.cs`
  - `D:\TRbackup\Version4\Terraria/Item.cs`
  - `D:\TRbackup\Version4\Terraria/Lang.cs`

- 输入分区抽取出的声明类型焦点：
  - `Terraria.Achievements.Achievement`
  - `Terraria.Achievements.AchievementCondition`
  - `Terraria.Achievements.AchievementManager`
  - `Terraria.Achievements.AchievementManager.StoredAchievement`
  - `Terraria.Achievements.AchievementTracker<T>`
  - `Terraria.GameContent.Achievements.AchievementsHelper`
  - `Terraria.GameContent.Achievements.CustomFloatCondition`
  - `Terraria.GameContent.Creative.CreativePowersHelper`
  - `Terraria.GameContent.Creative.CreativePowersHelper.CreativePowerIconLocations`
  - `Terraria.GameContent.Creative.CreativePowerUIElementRequestInfo`
  - `Terraria.Item`
  - `Terraria.Lang`
  - `Terraria.Lang.ItemPrefixCombiner`
  - `Terraria.Localization.GameCulture`
  - `Terraria.Localization.Language`
  - `Terraria.Localization.LanguageManager`
  - `Terraria.Localization.LocalizedText`
  - `Terraria.Localization.NetworkText`
  - `Terraria.Localization.VariableText`
  - `Terraria.Localization.VariableText.Condition`
  - `Terraria.UI.ItemSlot`
  - `Terraria.UI.ItemSlot.AlternateClickAction`
  - `Terraria.UI.ItemSlot.Context`
  - `Terraria.UI.ItemSlot.ItemDisplayKey`
  - `Terraria.UI.ItemSlot.ItemTransferInfo`
  - `Terraria.UI.ItemSlot.Options`
  - `Terraria.UI.ItemSlot.PulseEffect`
  - `Terraria.UI.ItemSorting`
  - `Terraria.UI.ItemSorting.DamageTypeSortingLayerEntry`
  - `Terraria.UI.ItemSorting.ItemSortingLayer`
  - `Terraria.UI.ItemSorting.ItemSortingLayers`
  - `Terraria.UI.ItemTooltip`

- 若路径、类型或行号漂移，记录 version-drift 或 evidence-mismatch；不得静默沿用库存报告行号。Version4 是 Terraria 私有实现的事实来源，完整源码只补证 Version4 已存在文件，tModLoader 只交叉验证公开边界，SS14 只参考结构粒度。

## 专属审查问题

- LocalizedText、Lang 目录、文化注册表和网络文本值是定义、可变客户端状态还是协议载荷？语言切换谁负责失效与重建？
- ItemSlot storage、equipment/crafting/creative/hotbar context 与真实库存/装备 owner 如何分离？
- 排序 registry/ranking/catalog 与一次排序工作集、缓存槽位和弹药填充分别是 Query、Projection 还是 Command？
- tooltip、外观、成就和 Creative UI 是否只读；哪些用户操作应通过显式 Command 返回玩家/物品系统？

## 专属不拆分边界

- 不要把本地化目录、tooltip、ItemSlot、排序目录、Creative UI 和真实库存合成一个 UIItemComponent。
- 不要把排序批次、槽位高亮、tooltip 文本、文化缓存或图标位置当作持久化物品状态。
- 不要以客户端显示文本、排序结果或成就 UI 判断服务端解锁/库存/装备已写入。

专属跨域提醒：重点记录与物品库存、玩家输入、内容目录、成就/图鉴、网络和持久化的 integration-risk；LocalizedText、ItemSlotReference、SortContext、AchievementProgress owner 标 crossSubsystemOwner: integration-review。

## 输出要求

- 将完整研究报告只写入：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P17-ui-item-localization-public-decomposition.md
- 不要写入输入分区报告、公共提示词、其他分区报告、源代码、测试、项目文件或迁移文件。
- 报告必须逐条覆盖本分区成员，给出成员归属表、状态所有权、Component/System/Query/Command/Adapter/Projection 边界、接口契约、依赖方向、顺序约束、不拆分项、兼容策略、evidence-gap、blocking-decision、focused verifier 计划和 Integration Handoff。
- 本轮只读审查不运行编译型命令；没有实际执行的 verifier 必须写 verificationStatus: not-run。
- proposed 类型、路径和接口必须明确标记 proposed；不得写成当前 NLTX 已存在或已经迁移。
- 跨两个或以上分区使用的类型、接口、ID、快照、值对象和 System 顺序只能提出候选，必须写 crossSubsystemOwner: integration-review，不得在本报告单方面裁决。
- 不得修改任何生产代码、测试、项目文件、生成器、公共协议、分区报告或其他并行会话输出。

## 分区完成检查

- [ ] 逐条覆盖本分区全部 296 条成员，成员事实来自输入表但关键语义已回到 Version4 复核。
- [ ] 只使用本分区叶子清单；没有复制、重新设计或宣布其他 P 分区成员。
- [ ] Component 只承载内聚权威状态；派生值、缓存、查询、短期 payload、日志、网络/存档/UI 输出按正确角色隔离。
- [ ] 所有共享候选写 crossSubsystemOwner: integration-review；没有把 proposed 设计写成已实现能力。
- [ ] 未执行的构建、测试和 verifier 均明确 verificationStatus: not-run；证据缺口、冲突和 blocking-decision 已单列。
- [ ] 输出文件路径唯一为：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P17-ui-item-localization-public-decomposition.md

## 会话结束回报

最终消息只回报以下字段：
- partitionId: P17
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P17-ui-item-localization-public-decomposition.md
- evidenceSourcesRead: 实际读取的 Version4 文件/类型、必要的完整源码补证、tModLoader 页面和 SS14 参考
- evidenceGaps: 仍缺失或冲突的关键证据
- blockingDecisions: 需要最终整合会话裁决的 owner/顺序/事务边界
- verificationStatus: not-run
- productionCodeModified: no

本提示词只定义本分区的专属范围和审查抓手；公共协议负责共同证据规则、报告结构、禁止项和 Integration Handoff。
