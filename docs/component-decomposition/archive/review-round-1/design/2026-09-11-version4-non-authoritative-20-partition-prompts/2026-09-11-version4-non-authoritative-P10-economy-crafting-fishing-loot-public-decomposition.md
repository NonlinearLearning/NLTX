# Version4 非权威组件拆分分区 P10：经济、配方、钓鱼与掉落 - public-decomposition 专属提示词

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
- 必须先按 skill 使用 ClaimNext（或用户明确指定时使用 Claim -PartitionId P10），只写本文件的 outputReport，并用领取返回的同一 sessionId 完成 Complete/Fail/Abandon；不得手工编辑 ledger 或 lock。
- 本任务属于 20 个并行会话中的固定分区；currentTaskCount: 20。公共协议中遗留的 19 会话数量说明在本任务包中不适用。

## 任务元数据

- promptId: P10-non-authoritative-public-decomposition-20260911
- partitionId: P10
- inputPartitionReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\10-economy-crafting-fishing-loot.md
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P10-economy-crafting-fishing-loot-public-decomposition.md
- reportType: public-decomposition
- executionMode: independent-read-only-partition
- currentTaskCount: 20
- formalParentSubsystems: RuntimeComposition, SharedRuntimeMechanisms
- leafSubsystemCount: 24
- fieldCount: 296
- propertyCount: 25
- memberCount: 321
- sourceReportSha256: b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196
- sourceInventoryStatus: source-inventory-confirmed; member-level owner/lifecycle/side-effect evidence still requires this session to close

## 专属范围锁定

- 本会话只审查输入分区报告中的 24 个叶子子系统和 321 条成员记录。以下清单是本会话唯一的成员范围；不得读取或复制其他 P 分区的成员清单。
- 细分子系统仍只是库存边界，不等价于一个组件；允许在本分区内部按访问模式拆成多个 proposed Component、System、Query、Command、Adapter 或 Projection。
- 来源序号、路径、行号和声明是检索线索；必须回到 Version4 重新定位，不得把输入报告直接当作已确认的读者、写者、生命周期或 owner 证据。

| 叶子子系统 | 父级 | 原章节 | 边界角色 | 字段 | 属性 | 成员 | 细分职责 |
|---|---|---|---|---:|---:|---:|---|
| `MainShopAndQuestSlots` | `RuntimeComposition` | `4.1.28` | `runtime state` | 7 | 0 | 7 | 商店容器、旅行商店和渔夫任务槽。 |
| `SharedFishingCatchEffects` | `SharedRuntimeMechanisms` | `4.9.1` | `command/adapter` | 3 | 0 | 3 | 鱼获交付、鱼饵桶辅助和来源归因。 |
| `SharedItemCommerceState` | `SharedRuntimeMechanisms` | `4.9.18` | `definition/state` | 6 | 0 | 6 | 商店资格、买卖价格和特殊货币状态。 |
| `SharedItemCommerceAndPricing` | `SharedRuntimeMechanisms` | `4.9.21` | `definition/state` | 19 | 1 | 20 | 商店价格、回售和购物设置。 |
| `SharedLootSimulation` | `SharedRuntimeMechanisms` | `4.9.48` | `query` | 9 | 0 | 9 | 离线掉落模拟和计数状态。 |
| `SharedDropSourceAttribution` | `SharedRuntimeMechanisms` | `4.9.49` | `value object` | 3 | 0 | 3 | 掉落和 Boss 生成来源值对象。 |
| `CraftingRequestState` | `SharedRuntimeMechanisms` | `4.9.99` | `state/command` | 6 | 1 | 7 | 远程制作请求和待处理制作队列状态。 |
| `SharedDropRuleResolutionAndCatalogState` | `SharedRuntimeMechanisms` | `4.9.114` | `query/adapter` | 19 | 2 | 21 | 掉落尝试、概率结果、数据库和 resolver 解析边界。 |
| `SharedFishingConditionContextState` | `SharedRuntimeMechanisms` | `4.9.115` | `query/state` | 13 | 0 | 13 | 钓鱼上下文、条件对象和任务鱼筛选状态。 |
| `RecipeDefinitionState` | `SharedRuntimeMechanisms` | `4.9.144` | `definition/catalog` | 23 | 2 | 25 | 配方输入、条件、制作材料和环境要求。 |
| `RecipeGroupCatalogState` | `SharedRuntimeMechanisms` | `4.9.145` | `definition/catalog` | 7 | 1 | 8 | 配方组集合、文本格式和注册 ID 目录。 |
| `FishingAttemptState` | `SharedRuntimeMechanisms` | `4.9.150` | `state/query` | 24 | 0 | 24 | 一次钓鱼尝试的地点、概率、环境和结果状态。 |
| `PlayerFishingConditionState` | `SharedRuntimeMechanisms` | `4.9.151` | `query/state` | 6 | 0 | 6 | 玩家鱼竿、鱼饵和最终钓鱼等级条件。 |
| `SharedDropRuleChainState` | `SharedRuntimeMechanisms` | `4.9.168` | `definition/query` | 3 | 18 | 21 | 掉落规则链、链式结果和链隐藏策略。 |
| `SharedFishingDropResolutionState` | `SharedRuntimeMechanisms` | `4.9.170` | `query/adapter` | 8 | 0 | 8 | FishDropRule、可能性条目和鱼获规则解析结果。 |
| `SharedItemEconomyAndValueRules` | `SharedRuntimeMechanisms` | `4.9.178` | `definition/catalog` | 12 | 0 | 12 | 物品货币、价格、稀有度和经济价值规则。 |
| `SharedItemUseTimingAndStackRules` | `SharedRuntimeMechanisms` | `4.9.179` | `definition/catalog` | 18 | 0 | 18 | 物品使用时序、拾取、食物、冷却和堆叠规则。 |
| `SharedFishingRarityConditionCatalogState` | `SharedRuntimeMechanisms` | `4.9.199` | `definition/catalog` | 11 | 0 | 11 | 鱼获稀有度枚举、稀有度条件 delegate 和视觉频率定义。 |
| `SharedDropRuleConditionBranchState` | `SharedRuntimeMechanisms` | `4.9.200` | `definition/query` | 9 | 0 | 9 | 掉落条件类型、条件字段和模式分支资格判断。 |
| `SharedFishingConditionCatalogPopulationState` | `SharedRuntimeMechanisms` | `4.9.206` | `definition/catalog` | 8 | 0 | 8 | 钓鱼条件目录的注册、集合和模式/资格元数据。 |
| `SharedFishingEnvironmentPredicateState` | `SharedRuntimeMechanisms` | `4.9.207` | `definition/query` | 31 | 0 | 31 | 钓鱼液体、深度、生物群落、海洋和世界事件环境条件。 |
| `SharedDropRuleChanceAndQuantityState` | `SharedRuntimeMechanisms` | `4.9.208` | `definition/query` | 16 | 0 | 16 | 掉落规则的概率、重掷、最小/最大数量和逐个掉落参数。 |
| `SharedDropRuleOptionSelectionState` | `SharedRuntimeMechanisms` | `4.9.209` | `definition/query` | 20 | 0 | 20 | 按模式、选项集合和候选规则选择掉落项的状态。 |
| `SharedItemUseTimingAndConsumptionState` | `SharedRuntimeMechanisms` | `4.9.214` | `definition/state` | 15 | 0 | 15 | 物品使用样式、时序、消耗、复用和使用表现能力。 |

来源成员的分区内序号线索范围：405..4023；序号不定义领域边界，必须逐条回到输入表和 Version4 覆盖。

## 分区专属目标

围绕“经济、配方、钓鱼与掉落”完成独立只读的 Version4 成员审查和 ECS 拆分设计。
- 围绕商店/任务槽、价格与货币、配方及配方组、Crafting request、钓鱼环境/尝试/鱼获规则、掉落规则链/概率/数量/选择和离线掉落模拟，核对规则定义、资格 Query 与事务提交。
- 回到商店、Recipe、Fishing、ItemDropRule、Loot、Player/Item/NPC 直接调用链，重新定位注册、条件评估、随机消费、结果提交、物品生成和网络/存档边界。
- 分别建模经济规则/目录、一次交易 Command、配方资格 Query、制作请求队列、FishingAttempt、FishingDropResolution、DropRule resolution 和结果 Projection。
- 核对随机性、重掷、数量、来源归因、任务鱼、环境条件、价格计算和失败/重试/幂等性；不要以名称或历史行号推断 owner。

## 专属证据焦点

- 先完整读取本分区输入报告的成员表，再回到 D:\TRbackup\Version4 重新核对字段、属性、声明类型、初始化/注册、直接读者、直接写者、清理路径和副作用。
- 下列路径只作为本分区的检索种子；路径存在不等于成员语义已确认，必须按公共证据协议记录实际行号、符号、调用者、读者、写者、生命周期和 evidenceStatus。
  - `D:\TRbackup\Version4\Terraria.DataStructures/EntitySource_BossSpawn.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/EntitySource_DropAsItem.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/EntitySource_FishedOut.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/EntitySource_Loot.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/FishingAttempt.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/PlayerFishingConditions.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.FishDropRules/AFishDropRulePopulator.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.FishDropRules/AFishingCondition.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.FishDropRules/FishDropRule.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.FishDropRules/FishDropRuleList.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.FishDropRules/FishingConditions.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.FishDropRules/FishingContext.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.FishDropRules/FishPossibilityEntry.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.FishDropRules/FishRarityCondition.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/Chains.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/CommonDrop.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/CommonDropWithRerolls.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/Conditions.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/DropAttemptInfo.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/DropBasedOnExpertMode.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/DropBasedOnExtraGel.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/DropBasedOnMasterAndExpertMode.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/DropBasedOnMasterMode.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/DropLocalPerClientAndResetsNPCMoneyTo0.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/DropNothing.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/DropOneByOne.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/DropPerPlayerOnThePlayer.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/DropRateInfo.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/DropRateInfoChainFeed.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/FromOptionsWithoutRepeatsDropRule.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/IItemDropRule.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/IItemDropRuleChainAttempt.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/ItemDropAttemptResult.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/ItemDropDatabase.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/ItemDropResolver.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/ItemDropWithConditionRule.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/LeadingConditionRule.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/MechBossSpawnersDropRule.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/OneFromOptionsDropRule.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/OneFromOptionsNotScaledWithLuckDropRule.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/OneFromRulesRule.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/SlimeBodyItemDropRule.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.ItemDropRules/StatueMimicItemDropRule.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.LootSimulation/LootSimulationItemCounter.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.LootSimulation/SimulatorInfo.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/ChumBucketProjectileHelper.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/CraftingRequests.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/ItemShopSellbackHelper.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/ShopHelper.cs`
  - `D:\TRbackup\Version4\Terraria/Item.cs`
  - `D:\TRbackup\Version4\Terraria/Main.cs`
  - `D:\TRbackup\Version4\Terraria/Recipe.cs`
  - `D:\TRbackup\Version4\Terraria/RecipeGroup.cs`
  - `D:\TRbackup\Version4\Terraria/ShoppingSettings.cs`

- 输入分区抽取出的声明类型焦点：
  - `Terraria.DataStructures.EntitySource_BossSpawn`
  - `Terraria.DataStructures.EntitySource_DropAsItem`
  - `Terraria.DataStructures.EntitySource_FishedOut`
  - `Terraria.DataStructures.EntitySource_Loot`
  - `Terraria.DataStructures.FishingAttempt`
  - `Terraria.DataStructures.PlayerFishingConditions`
  - `Terraria.GameContent.ChumBucketProjectileHelper`
  - `Terraria.GameContent.CraftingRequests`
  - `Terraria.GameContent.CraftingRequests.RemoteCraftRequest`
  - `Terraria.GameContent.FishDropRules.AFishDropRulePopulator`
  - `Terraria.GameContent.FishDropRules.AFishDropRulePopulator.DelegateFishingCondition`
  - `Terraria.GameContent.FishDropRules.AFishDropRulePopulator.DelegateFishingRarityCondition`
  - `Terraria.GameContent.FishDropRules.AFishDropRulePopulator.Rarity`
  - `Terraria.GameContent.FishDropRules.AFishingCondition`
  - `Terraria.GameContent.FishDropRules.FishDropRule`
  - `Terraria.GameContent.FishDropRules.FishDropRuleList`
  - `Terraria.GameContent.FishDropRules.FishingConditions.QuestFishCondition`
  - `Terraria.GameContent.FishDropRules.FishingConditions.QuestFishConditionRemix`
  - `Terraria.GameContent.FishDropRules.FishingContext`
  - `Terraria.GameContent.FishDropRules.FishPossibilityEntry`
  - `Terraria.GameContent.FishDropRules.FishRarityCondition`
  - `Terraria.GameContent.ItemDropRules.Chains.TryIfDoesntFillConditions`
  - `Terraria.GameContent.ItemDropRules.Chains.TryIfFailedRandomRoll`
  - `Terraria.GameContent.ItemDropRules.Chains.TryIfSucceeded`
  - `Terraria.GameContent.ItemDropRules.CommonDrop`
  - `Terraria.GameContent.ItemDropRules.CommonDropWithRerolls`
  - `Terraria.GameContent.ItemDropRules.Conditions.FromCertainWaveAndAbove`
  - `Terraria.GameContent.ItemDropRules.Conditions.IsUsingSpecificAIValues`
  - `Terraria.GameContent.ItemDropRules.Conditions.NamedNPC`
  - `Terraria.GameContent.ItemDropRules.DropAttemptInfo`
  - `Terraria.GameContent.ItemDropRules.DropBasedOnExpertMode`
  - `Terraria.GameContent.ItemDropRules.DropBasedOnExtraGel`
  - `Terraria.GameContent.ItemDropRules.DropBasedOnMasterAndExpertMode`
  - `Terraria.GameContent.ItemDropRules.DropBasedOnMasterMode`
  - `Terraria.GameContent.ItemDropRules.DropLocalPerClientAndResetsNPCMoneyTo0`
  - `Terraria.GameContent.ItemDropRules.DropNothing`
  - `Terraria.GameContent.ItemDropRules.DropOneByOne`
  - `Terraria.GameContent.ItemDropRules.DropOneByOne.Parameters`
  - `Terraria.GameContent.ItemDropRules.DropPerPlayerOnThePlayer`
  - `Terraria.GameContent.ItemDropRules.DropRateInfo`
  - `Terraria.GameContent.ItemDropRules.DropRateInfoChainFeed`
  - `Terraria.GameContent.ItemDropRules.FromOptionsWithoutRepeatsDropRule`
  - `Terraria.GameContent.ItemDropRules.IItemDropRule`
  - `Terraria.GameContent.ItemDropRules.IItemDropRuleChainAttempt`
  - `Terraria.GameContent.ItemDropRules.ItemDropAttemptResult`
  - `Terraria.GameContent.ItemDropRules.ItemDropDatabase`
  - `Terraria.GameContent.ItemDropRules.ItemDropResolver`
  - `Terraria.GameContent.ItemDropRules.ItemDropWithConditionRule`
  - `Terraria.GameContent.ItemDropRules.LeadingConditionRule`
  - `Terraria.GameContent.ItemDropRules.MechBossSpawnersDropRule`
  - `Terraria.GameContent.ItemDropRules.OneFromOptionsDropRule`
  - `Terraria.GameContent.ItemDropRules.OneFromOptionsNotScaledWithLuckDropRule`
  - `Terraria.GameContent.ItemDropRules.OneFromRulesRule`
  - `Terraria.GameContent.ItemDropRules.SlimeBodyItemDropRule`
  - `Terraria.GameContent.ItemDropRules.StatueMimicItemDropRule`
  - `Terraria.GameContent.ItemShopSellbackHelper`
  - `Terraria.GameContent.ItemShopSellbackHelper.ItemMemo`
  - `Terraria.GameContent.LootSimulation.LootSimulationItemCounter`
  - `Terraria.GameContent.LootSimulation.SimulatorInfo`
  - `Terraria.GameContent.ShopHelper`
  - `Terraria.Item`
  - `Terraria.Main`
  - `Terraria.Recipe`
  - `Terraria.Recipe.RequiredItemEntry`
  - `Terraria.RecipeGroup`
  - `Terraria.ShoppingSettings`

- 若路径、类型或行号漂移，记录 version-drift 或 evidence-mismatch；不得静默沿用库存报告行号。Version4 是 Terraria 私有实现的事实来源，完整源码只补证 Version4 已存在文件，tModLoader 只交叉验证公开边界，SS14 只参考结构粒度。

## 专属审查问题

- 价格、货币、商店槽、配方目录、钓鱼条件和掉落规则哪些是静态定义，哪些是玩家/世界实例状态，哪些是纯 Query？
- 一次交易、制作、钓鱼交付和掉落解析的原子提交边界是什么？物品扣减、随机消费、结果生成和网络通知如何排序？
- FishingAttempt、DropRuleChain、Chance/Quantity、OptionSelection 和来源归因是否应拆分为短期计算上下文、规则组件和结果 Command？
- 经济/制作/钓鱼/掉落与物品库存、玩家玩法、NPC/Boss、世界环境、网络和持久化之间哪些共享类型必须 integration-review？

## 专属不拆分边界

- 不要把商店、配方、钓鱼、掉落和离线模拟合并为 EconomyComponent；定义目录与运行时事务必须分离。
- 不要把一次 FishingAttempt、DropRule 条目、概率结果、条件上下文、来源值对象或制作请求 payload 当作长期权威组件。
- 不要把随机 helper、价格派生值或 UI 商店/任务投影反向作为经济事实。

专属跨域提醒：重点记录与物品容器、玩家玩法、NPC/Boss、世界环境、网络和持久化的 integration-risk；交易/制作/钓鱼/掉落结果的最终 owner 只给 candidate，并写 crossSubsystemOwner: integration-review。

## 输出要求

- 将完整研究报告只写入：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P10-economy-crafting-fishing-loot-public-decomposition.md
- 不要写入输入分区报告、公共提示词、其他分区报告、源代码、测试、项目文件或迁移文件。
- 报告必须逐条覆盖本分区成员，给出成员归属表、状态所有权、Component/System/Query/Command/Adapter/Projection 边界、接口契约、依赖方向、顺序约束、不拆分项、兼容策略、evidence-gap、blocking-decision、focused verifier 计划和 Integration Handoff。
- 本轮只读审查不运行编译型命令；没有实际执行的 verifier 必须写 verificationStatus: not-run。
- proposed 类型、路径和接口必须明确标记 proposed；不得写成当前 NLTX 已存在或已经迁移。
- 跨两个或以上分区使用的类型、接口、ID、快照、值对象和 System 顺序只能提出候选，必须写 crossSubsystemOwner: integration-review，不得在本报告单方面裁决。
- 不得修改任何生产代码、测试、项目文件、生成器、公共协议、分区报告或其他并行会话输出。

## 分区完成检查

- [ ] 逐条覆盖本分区全部 321 条成员，成员事实来自输入表但关键语义已回到 Version4 复核。
- [ ] 只使用本分区叶子清单；没有复制、重新设计或宣布其他 P 分区成员。
- [ ] Component 只承载内聚权威状态；派生值、缓存、查询、短期 payload、日志、网络/存档/UI 输出按正确角色隔离。
- [ ] 所有共享候选写 crossSubsystemOwner: integration-review；没有把 proposed 设计写成已实现能力。
- [ ] 未执行的构建、测试和 verifier 均明确 verificationStatus: not-run；证据缺口、冲突和 blocking-decision 已单列。
- [ ] 输出文件路径唯一为：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P10-economy-crafting-fishing-loot-public-decomposition.md

## 会话结束回报

最终消息只回报以下字段：
- partitionId: P10
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P10-economy-crafting-fishing-loot-public-decomposition.md
- evidenceSourcesRead: 实际读取的 Version4 文件/类型、必要的完整源码补证、tModLoader 页面和 SS14 参考
- evidenceGaps: 仍缺失或冲突的关键证据
- blockingDecisions: 需要最终整合会话裁决的 owner/顺序/事务边界
- verificationStatus: not-run
- productionCodeModified: no

本提示词只定义本分区的专属范围和审查抓手；公共协议负责共同证据规则、报告结构、禁止项和 Integration Handoff。
