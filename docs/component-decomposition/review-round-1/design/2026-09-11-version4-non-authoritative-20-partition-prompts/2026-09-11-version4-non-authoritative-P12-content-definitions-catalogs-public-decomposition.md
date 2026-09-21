# Version4 非权威组件拆分分区 P12：内容定义与目录 - public-decomposition 专属提示词

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
- 必须先按 skill 使用 ClaimNext（或用户明确指定时使用 Claim -PartitionId P12），只写本文件的 outputReport，并用领取返回的同一 sessionId 完成 Complete/Fail/Abandon；不得手工编辑 ledger 或 lock。
- 本任务属于 20 个并行会话中的固定分区；currentTaskCount: 20。公共协议中遗留的 19 会话数量说明在本任务包中不适用。

## 任务元数据

- promptId: P12-non-authoritative-public-decomposition-20260911
- partitionId: P12
- inputPartitionReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\12-content-definitions-catalogs.md
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P12-content-definitions-catalogs-public-decomposition.md
- reportType: public-decomposition
- executionMode: independent-read-only-partition
- currentTaskCount: 20
- formalParentSubsystems: ContentCatalog, RuntimeComposition, SharedRuntimeMechanisms
- leafSubsystemCount: 19
- fieldCount: 229
- propertyCount: 56
- memberCount: 285
- sourceReportSha256: b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196
- sourceInventoryStatus: source-inventory-confirmed; member-level owner/lifecycle/side-effect evidence still requires this session to close

## 专属范围锁定

- 本会话只审查输入分区报告中的 19 个叶子子系统和 285 条成员记录。以下清单是本会话唯一的成员范围；不得读取或复制其他 P 分区的成员清单。
- 细分子系统仍只是库存边界，不等价于一个组件；允许在本分区内部按访问模式拆成多个 proposed Component、System、Query、Command、Adapter 或 Projection。
- 来源序号、路径、行号和声明是检索线索；必须回到 Version4 重新定位，不得把输入报告直接当作已确认的读者、写者、生命周期或 owner 证据。

| 叶子子系统 | 父级 | 原章节 | 边界角色 | 字段 | 属性 | 成员 | 细分职责 |
|---|---|---|---|---:|---:|---:|---|
| `MainContentAbilityTables` | `RuntimeComposition` | `4.1.10` | `catalog reference` | 12 | 0 | 12 | 投射物和 Buff 能力分类表。 |
| `MainContentCatalogAndSimulationServices` | `RuntimeComposition` | `4.1.29` | `catalog reference` | 10 | 0 | 10 | 掉落、鱼、图鉴、传送、商店和高尔夫服务引用。 |
| `ContentSamplesCatalog` | `ContentCatalog` | `4.6.1` | `definition/catalog` | 21 | 0 | 21 | 内容样本、创意分类和排序索引。 |
| `ContentSetFactory` | `ContentCatalog` | `4.6.2` | `definition/catalog` | 6 | 0 | 6 | 按内容类型构造集合的工厂状态。 |
| `ColorAndShaderSetCatalog` | `ContentCatalog` | `4.6.3` | `definition/catalog` | 24 | 1 | 25 | 颜色和染料着色器集合。 |
| `TileObjectDefinitionInheritanceState` | `ContentCatalog` | `4.6.4` | `definition/catalog` | 10 | 3 | 13 | TileObjectData 的父对象、alternates 和写时复制关系。 |
| `TileObjectDrawGeometryState` | `ContentCatalog` | `4.6.5` | `definition/catalog` | 2 | 18 | 20 | TileObjectData 的绘制偏移、尺寸、坐标、原点和几何布局。 |
| `TileObjectAnchorAndLiquidPlacementState` | `ContentCatalog` | `4.6.6` | `definition/catalog` | 10 | 15 | 25 | TileObjectData 的锚点、有效瓦片、液体死亡和液体放置规则。 |
| `TileObjectPlacementHookAndBaseState` | `ContentCatalog` | `4.6.7` | `definition/catalog` | 8 | 5 | 13 | TileObjectData 的放置 hook、基础对象、坐标模块和子瓦片。 |
| `TileObjectStyleDefinitionCatalogState` | `ContentCatalog` | `4.6.8` | `definition/catalog` | 24 | 4 | 28 | TileObjectData 的固定样式定义、样式常量和样式布局目录。 |
| `TileObjectStyleSelectionAndOverrideState` | `ContentCatalog` | `4.6.9` | `definition/query` | 2 | 6 | 8 | TileObjectData 的样式覆盖、随机选择和样式步进状态。 |
| `SharedContentValidation` | `SharedRuntimeMechanisms` | `4.9.60` | `definition/query` | 12 | 0 | 12 | 内容安全、拒绝规则和合法性校验。 |
| `SharedContentPresentationCatalog` | `SharedRuntimeMechanisms` | `4.9.61` | `definition/presentation` | 21 | 2 | 23 | 档案、字体、发型和视觉配置定义。 |
| `ItemVariantDefinitions` | `SharedRuntimeMechanisms` | `4.9.90` | `definition/catalog` | 14 | 1 | 15 | 物品变体、变体条件和变体条目定义。 |
| `LegacyItemPrefixCatalog` | `SharedRuntimeMechanisms` | `4.9.92` | `definition/catalog` | 16 | 0 | 16 | 旧物品前缀和前缀物品集合目录。 |
| `ArmorSetBonusDefinitions` | `SharedRuntimeMechanisms` | `4.9.109` | `definition/catalog` | 21 | 1 | 22 | 套装加成效果、Builder 和查询上下文定义。 |
| `ArmorSetBonusCatalog` | `SharedRuntimeMechanisms` | `4.9.110` | `definition/catalog` | 2 | 0 | 2 | 套装加成集合和按物品查询目录。 |
| `WingStatsDefinition` | `SharedRuntimeMechanisms` | `4.9.111` | `definition/value object` | 7 | 0 | 7 | 翅膀飞行时间、速度和悬停属性定义。 |
| `SharedItemStaticCapabilityRules` | `SharedRuntimeMechanisms` | `4.9.134` | `definition/catalog` | 7 | 0 | 7 | 物品静态能力表、装备槽类型和相位颜色目录。 |

来源成员的分区内序号线索范围：123..3821；序号不定义领域边界，必须逐条回到输入表和 Version4 覆盖。

## 分区专属目标

围绕“内容定义与目录”完成独立只读的 Version4 成员审查和 ECS 拆分设计。
- 围绕 Content catalog/sample/set factory、TileObjectData 继承/绘制/锚点/放置/样式、内容验证、Item variant/prefix、Armor set bonus、WingStats 和静态能力规则，核对定义目录与运行时实例的边界。
- 回到 TileObjectData、内容目录、Item prefix/variant、Armor/Wing 和 Main 内容服务引用的源码，确认注册、继承/写时复制、构造、覆盖、查找和生命周期。
- 分别区分静态定义、目录索引、工厂、验证 Query、TileObject placement input、能力定义和运行时实例；外部资产/着色器/字体等只通过 Adapter。
- 核对内容定义如何进入 Tile、Item、Player、UI、网络和持久化，特别关注继承链、alternates、随机样式、注册顺序和缓存失效。

## 专属证据焦点

- 先完整读取本分区输入报告的成员表，再回到 D:\TRbackup\Version4 重新核对字段、属性、声明类型、初始化/注册、直接读者、直接写者、清理路径和副作用。
- 下列路径只作为本分区的检索种子；路径存在不等于成员语义已确认，必须按公共证据协议记录实际行号、符号、调用者、读者、写者、生命周期和 evidenceStatus。
  - `D:\TRbackup\Version4\Terraria.DataStructures/ArmorSetBonus.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/ArmorSetBonuses.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/ColorSlidersSet.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/IConfigKeyHolder.cs`
  - `D:\TRbackup\Version4\Terraria.DataStructures/WingStats.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Items/ItemVariant.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Items/ItemVariantCondition.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Items/ItemVariants.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent.Prefixes/PrefixLegacy.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/ChildSafety.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/ContentRejectionFromSize.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/FontAssets.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/HairstyleUnlocksHelper.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/Profiles.cs`
  - `D:\TRbackup\Version4\Terraria.GameContent/VanillaContentValidator.cs`
  - `D:\TRbackup\Version4\Terraria.ID/Colors.cs`
  - `D:\TRbackup\Version4\Terraria.ID/ContentSamples.cs`
  - `D:\TRbackup\Version4\Terraria.ID/SetFactory.cs`
  - `D:\TRbackup\Version4\Terraria.ObjectData/TileObjectData.cs`
  - `D:\TRbackup\Version4\Terraria/Item.cs`
  - `D:\TRbackup\Version4\Terraria/Main.cs`

- 输入分区抽取出的声明类型焦点：
  - `Terraria.DataStructures.ArmorSetBonus`
  - `Terraria.DataStructures.ArmorSetBonus.Builder`
  - `Terraria.DataStructures.ArmorSetBonus.Builder.Parts`
  - `Terraria.DataStructures.ArmorSetBonus.QueryContext`
  - `Terraria.DataStructures.ArmorSetBonus.QueryResult`
  - `Terraria.DataStructures.ArmorSetBonuses`
  - `Terraria.DataStructures.ColorSlidersSet`
  - `Terraria.DataStructures.IConfigKeyHolder`
  - `Terraria.DataStructures.WingStats`
  - `Terraria.GameContent.ChildSafety`
  - `Terraria.GameContent.ContentRejectionFromSize`
  - `Terraria.GameContent.FontAssets`
  - `Terraria.GameContent.HairstyleUnlocksHelper`
  - `Terraria.GameContent.Items.ItemVariant`
  - `Terraria.GameContent.Items.ItemVariantCondition`
  - `Terraria.GameContent.Items.ItemVariants`
  - `Terraria.GameContent.Items.ItemVariants.VariantEntry`
  - `Terraria.GameContent.Prefixes.PrefixLegacy.ItemSets`
  - `Terraria.GameContent.Prefixes.PrefixLegacy.Prefixes`
  - `Terraria.GameContent.Profiles.LegacyNPCProfile`
  - `Terraria.GameContent.Profiles.StackedNPCProfile`
  - `Terraria.GameContent.Profiles.TransformableNPCProfile`
  - `Terraria.GameContent.Profiles.VariantNPCProfile`
  - `Terraria.GameContent.VanillaContentValidator`
  - `Terraria.GameContent.VanillaContentValidator.TextureMetaData`
  - `Terraria.ID.Colors`
  - `Terraria.ID.ContentSamples`
  - `Terraria.ID.ContentSamples.CreativeHelper`
  - `Terraria.ID.ContentSamples.CreativeHelper.ItemGroupAndOrderInGroup`
  - `Terraria.ID.ContentSamples.DyeShaderIDs`
  - `Terraria.ID.SetFactory`
  - `Terraria.Item`
  - `Terraria.Main`
  - `Terraria.ObjectData.TileObjectData`

- 若路径、类型或行号漂移，记录 version-drift 或 evidence-mismatch；不得静默沿用库存报告行号。Version4 是 Terraria 私有实现的事实来源，完整源码只补证 Version4 已存在文件，tModLoader 只交叉验证公开边界，SS14 只参考结构粒度。

## 专属审查问题

- Content sample/set factory、TileObjectData 父对象与 alternates、样式选择/覆盖分别拥有何种状态？哪些只在注册阶段存在？
- 内容目录、静态能力表、前缀/变体、套装加成和翅膀属性是 immutable definition、索引 Query 还是玩家实例状态？
- 放置几何、锚点、液体规则和 placement hook 的输入/输出如何与 Tile 世界写入分离？
- 内容定义的加载、验证、网络/存档表示、UI 展示和运行时缓存之间哪些边界需 integration-review？

## 专属不拆分边界

- 不要把所有内容目录、TileObjectData、Item variants、prefix、Armor/Wing 定义塞进通用 ContentComponent。
- 不要把注册表索引、样式覆盖、工厂缓存、验证结果或一次放置输入当作运行时权威实例状态。
- 不要把外部资源类型、shader/字体句柄或目录 ID 直接渗透到核心模拟组件。

专属跨域提醒：重点记录与 Tile/世界存储、物品、玩家装备、UI、本地化、网络和持久化的 integration-risk；共享定义/索引/目录 owner 只作 candidate 并标 crossSubsystemOwner: integration-review。

## 输出要求

- 将完整研究报告只写入：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P12-content-definitions-catalogs-public-decomposition.md
- 不要写入输入分区报告、公共提示词、其他分区报告、源代码、测试、项目文件或迁移文件。
- 报告必须逐条覆盖本分区成员，给出成员归属表、状态所有权、Component/System/Query/Command/Adapter/Projection 边界、接口契约、依赖方向、顺序约束、不拆分项、兼容策略、evidence-gap、blocking-decision、focused verifier 计划和 Integration Handoff。
- 本轮只读审查不运行编译型命令；没有实际执行的 verifier 必须写 verificationStatus: not-run。
- proposed 类型、路径和接口必须明确标记 proposed；不得写成当前 NLTX 已存在或已经迁移。
- 跨两个或以上分区使用的类型、接口、ID、快照、值对象和 System 顺序只能提出候选，必须写 crossSubsystemOwner: integration-review，不得在本报告单方面裁决。
- 不得修改任何生产代码、测试、项目文件、生成器、公共协议、分区报告或其他并行会话输出。

## 分区完成检查

- [ ] 逐条覆盖本分区全部 285 条成员，成员事实来自输入表但关键语义已回到 Version4 复核。
- [ ] 只使用本分区叶子清单；没有复制、重新设计或宣布其他 P 分区成员。
- [ ] Component 只承载内聚权威状态；派生值、缓存、查询、短期 payload、日志、网络/存档/UI 输出按正确角色隔离。
- [ ] 所有共享候选写 crossSubsystemOwner: integration-review；没有把 proposed 设计写成已实现能力。
- [ ] 未执行的构建、测试和 verifier 均明确 verificationStatus: not-run；证据缺口、冲突和 blocking-decision 已单列。
- [ ] 输出文件路径唯一为：D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P12-content-definitions-catalogs-public-decomposition.md

## 会话结束回报

最终消息只回报以下字段：
- partitionId: P12
- outputReport: D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P12-content-definitions-catalogs-public-decomposition.md
- evidenceSourcesRead: 实际读取的 Version4 文件/类型、必要的完整源码补证、tModLoader 页面和 SS14 参考
- evidenceGaps: 仍缺失或冲突的关键证据
- blockingDecisions: 需要最终整合会话裁决的 owner/顺序/事务边界
- verificationStatus: not-run
- productionCodeModified: no

本提示词只定义本分区的专属范围和审查抓手；公共协议负责共同证据规则、报告结构、禁止项和 Integration Handoff。
