# P12 Proposed Component Design: Content Definitions And Catalogs

partitionId: P12
sessionId: 45463d93b7a6426a8a901e48dd9c7b06
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\12-content-definitions-catalogs.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P12-content-definitions-catalogs-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P12-content-definitions-catalogs-component-execution.md
designStatus: proposed
executionStatus: planned
implementationStatus: completed
src2CodeModified: yes
verificationStatus: independently-verified
evidenceStatus: partial
completedComponents: [proposed.ContentAbilityCatalog, proposed.MainContentServiceReferenceBoundary, proposed.ContentSampleIndexCatalog, proposed.SetArrayFactoryBoundary, proposed.ColorAndShaderCatalog, proposed.TileObjectInheritanceDefinition, proposed.TileObjectGeometryDefinition, proposed.TileObjectPlacementDefinition, proposed.TileObjectPlacementHookDefinition, proposed.TileObjectStyleDefinitionAndSelection, proposed.ContentValidationBoundary, proposed.ContentPresentationCatalog, proposed.ItemVariantCatalog, proposed.LegacyItemPrefixCatalog, proposed.ArmorSetBonusDefinitionCatalog, proposed.ArmorSetBonusLookupCatalog, proposed.WingStatsDefinition, proposed.ItemStaticCapabilityCatalog]
implementedComponents: [proposed.ContentAbilityCatalog, proposed.MainContentServiceReferenceBoundary, proposed.ContentSampleIndexCatalog, proposed.SetArrayFactoryBoundary, proposed.ColorAndShaderCatalog, proposed.TileObjectInheritanceDefinition, proposed.TileObjectGeometryDefinition, proposed.TileObjectPlacementDefinition, proposed.TileObjectPlacementHookDefinition, proposed.TileObjectStyleDefinitionAndSelection, proposed.ContentValidationBoundary, proposed.ContentPresentationCatalog, proposed.ItemVariantCatalog, proposed.LegacyItemPrefixCatalog, proposed.ArmorSetBonusDefinitionCatalog, proposed.ArmorSetBonusLookupCatalog, proposed.WingStatsDefinition, proposed.ItemStaticCapabilityCatalog (source saved and aggregate verifier passed)]
currentComponent: none
pendingComponents: []
lastCheckpointUtc: 2026-09-12T01:53:22.5816369Z
implementedSrc2Files: src2/Content/Runtime/ContentAbilityCatalog.cs; src2/Content/Runtime/MainContentServiceReferenceBoundary.cs; src2/Content/Runtime/Primitives.cs; src2/Content/Catalogs/ContentSampleIndexCatalog.cs; src2/Content/Factories/SetArrayFactoryBoundary.cs; src2/Content/Validation/ContentValidationBoundary.cs; src2/Presentation/Definitions/ColorAndShaderCatalog.cs; src2/Presentation/ContentPresentationCatalog.cs; src2/WorldInteraction/Tiles/Definitions/TileObjectInheritanceDefinition.cs; src2/WorldInteraction/Tiles/Definitions/TileObjectGeometryDefinition.cs; src2/WorldInteraction/Tiles/Definitions/TileObjectPlacementDefinition.cs; src2/WorldInteraction/Tiles/Definitions/TileObjectPlacementHookDefinition.cs; src2/WorldInteraction/Tiles/Definitions/TileObjectStyleDefinitionAndSelection.cs; src2/Items/Definitions/ItemVariantCatalog.cs; src2/Items/Definitions/LegacyItemPrefixCatalog.cs; src2/Items/Definitions/ItemStaticCapabilityCatalog.cs; src2/Items/Armor/Definitions/ArmorSetBonusDefinitionCatalog.cs; src2/Items/Armor/Definitions/ArmorSetBonusLookupCatalog.cs; src2/Items/Armor/Definitions/WingStatsDefinition.cs; src2/ContentDefinitions/Terraria.ContentDefinitions.csproj; src2/ContentDefinitionsVerification/Program.cs; src2/ContentDefinitionsVerification/Terraria.ContentDefinitionsVerification.csproj
evidence-gap:
- P12 第一轮 public-decomposition 输出路径 `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P12-content-definitions-catalogs-public-decomposition.md` 在 checkout 中不存在；本轮只使用当前输入报告、Version4 和已读取的交叉证据。
- 285 条成员的声明、关键初始化和部分直接读写已回到 Version4；仍有成员级完整 reader/writer、清理、网络、持久化和调度闭合缺口，成员表以 partial/evidence-gap 表示。
- 仓库引用的 `约束/公共拆分约束.md` 不存在；本轮遵循 AGENTS.md、ECS 文件组织约束、命名约束、C# 风格和副作用隔离规则，不据此宣称代码可实施。
- 当前 NLTX `src/Content` 的 ContentCatalog、ContentCatalogSnapshot、DerivedIndexes 和 PresentationIndexes 只是已有草案/部分结构；没有证据证明已映射 P12 的 285 条成员。
- SS14 没有直接对应的 Terraria 内容目录证据；只参考最小 ECS System/Query 形状，边界由 Version4 和 NLTX 约束决定。
blocking-decision:
- crossSubsystemOwner: integration-review；最终必须裁决 Content ID、local type、persistent ID、network ID、EntityReference、TileCoordinate、ItemInstanceId、snapshot 和资源句柄的共享 owner。
- Tile placement hook、world/tile commit、TileEntity placement、liquid effects 和 System 顺序不能由 P12 单独决定；需明确单一 WorldInteraction/WorldStorage writer、失败重试和幂等语义。
- Main 服务句柄、ArmorSetEffect、WingStats 外部表、Item.Variant/Item prefix、玩家装备效果和 ContentSamples 样本对象的跨域提交/清理/网络/持久化边界待整合会话裁决。
- 版本/协议兼容策略需在 integration-review 选择：A 保留 legacy facade 只读适配，B 先建立 typed snapshot 再切换，C 对 unresolved 字段暂缓迁移；不同选择会改变回滚和删除门槛。

> Checkpoint implementation-completed. All 18 proposed boundary source files are saved under `src2`, and the focused aggregate verifier passed after a serial build. This does not claim production migration, behavior equivalence, network/persistence closure or final cross-partition ownership.

## 1. 范围与排除

本分区覆盖输入报告中的 19 个叶子子系统、229 个字段、56 个属性、285 条成员。19 个库存组按共同读写者、变更原因和生命周期收敛为 18 个 proposed 边界，其中两个 TileObject style 库存组保留为一个边界，但在成员表中区分注册期定义、样式覆盖、随机选择、派生步进和纯 Query。

设计阶段排除生产 C#、Version4、第一轮报告、ledger、lock 和其他会话文档的修改；实现续接只在 `src2` 保存了本设计的 typed source 和 focused verifier。内容目录、TileObjectData、Item variant/prefix、Armor/Wing 定义和能力索引不是运行时实体实例；外部资源、网络、持久化、UI、WorldGrid 和玩家效果仍只通过 proposed Adapter/Projection/Command 交接。

## 2. 证据角色与读取结果

| 来源 | 读取用途 | 状态 |
|---|---|---|
| `D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\12-content-definitions-catalogs.md` | P12 唯一成员库存和 285 条来源序号 | confirmed |
| `D:\TRbackup\Version4\Terraria\Main.cs` | ability arrays、服务句柄、初始化顺序、Item reverse indexes | confirmed/partial |
| `D:\TRbackup\Version4\Terraria.ID\ContentSamples.cs` | sample、persistent/network ID、Creative/Bestiary index、shader index | confirmed/partial |
| `D:\TRbackup\Version4\Terraria.ID\SetFactory.cs` | typed set 创建、默认值、cache 和 lock | confirmed |
| `D:\TRbackup\Version4\Terraria.ID\Colors.cs` | immutable palette 和 CurrentLiquidColor 查询 | confirmed |
| `D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs` | registration、CopyFrom、alternates、geometry、placement、style、read-only 和 GetTileData | confirmed/partial |
| `D:\TRbackup\Version4\Terraria.GameContent.Items\ItemVariants.cs`、`ItemVariant.cs`、`ItemVariantCondition.cs` | variant registration、condition 和 selection | confirmed/partial |
| `D:\TRbackup\Version4\Terraria.GameContent.Prefixes\PrefixLegacy.cs` | prefix arrays 和 item masks | confirmed/partial |
| `D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonus.cs`、`ArmorSetBonuses.cs` | builder、query、definition 和 lookup | confirmed/partial |
| `D:\TRbackup\Version4\Terraria.DataStructures\WingStats.cs`、`Terraria.Initializers\WingStatsInitializer.cs` | WingStats value definition 和外部数组 projection | confirmed |
| `D:\TRbackup\Version4\Terraria.GameContent\ChildSafety.cs`、`ContentRejectionFromSize.cs`、`VanillaContentValidator.cs` | validation definition 和 asset metadata boundary | partial |
| `D:\TRbackup\Version4\Terraria.GameContent\FontAssets.cs`、`HairstyleUnlocksHelper.cs`、`Profiles.cs` | presentation assets/profile fields | partial |
| `D:\TRbackup\tmodloader-api-docs-stable\class_content_samples.html`、`class_set_factory.html`、`class_tile_object_data.html`、`struct_wing_stats.html`、`class_colors.html`、`class_font_assets.html` | v2026.07 公开边界交叉验证 | existing-evidence |
| `C:\Users\shan\Downloads\ECS\space-station-14-master` 最小组件/System 检索 | 粒度和 Query/System 结构参考，不推断 Terraria 语义 | limited reference |

Version4 行号是本次重新读取后的局部证据；输入报告来源序号保留为追溯键。Version4 私有实现优先于公开 API 文档，SS14 仅作结构参考。

## 3. Proposed Boundary Summary

| proposed boundary | input leaf group(s) | proposed target path | responsibility |
|---|---|---|---|
| `proposed.ContentAbilityCatalog` | `MainContentAbilityTables` | `src2/Content/Runtime/ContentAbilityCatalog.cs` | 能力定义、Buff/Projectile 分类索引，以及隔离后的兼容表现标量 |
| `proposed.MainContentServiceReferenceBoundary` | `MainContentCatalogAndSimulationServices` | `src2/Content/Runtime/MainContentServiceReferenceBoundary.cs` | 内容服务句柄和运行时服务适配边界，不是一个巨型组件 |
| `proposed.ContentSampleIndexCatalog` | `ContentSamplesCatalog` | `src2/Content/Catalogs/ContentSampleIndexCatalog.cs` | 样本只读参考视图、persistent/network ID 映射和 Creative/Bestiary 派生索引 |
| `proposed.SetArrayFactoryBoundary` | `ContentSetFactory` | `src2/Content/Factories/SetArrayFactoryBoundary.cs` | 按 content ID 创建 bool/int/ushort/float/custom 集合的工厂和内部缓冲复用 |
| `proposed.ColorAndShaderCatalog` | `ColorAndShaderSetCatalog` | `src2/Presentation/Definitions/ColorAndShaderCatalog.cs` | 颜色常量、瀑布/液体调色板和染料 shader index 的表现定义 |
| `proposed.TileObjectInheritanceDefinition` | `TileObjectDefinitionInheritanceState` | `src2/WorldInteraction/Tiles/Definitions/TileObjectInheritanceDefinition.cs` | TileObjectData parent、alternates、写时复制标志和注册期目录 |
| `proposed.TileObjectGeometryDefinition` | `TileObjectDrawGeometryState` | `src2/WorldInteraction/Tiles/Definitions/TileObjectGeometryDefinition.cs` | TileObject 绘制偏移、尺寸、原点、方向和 frame 坐标几何定义 |
| `proposed.TileObjectPlacementDefinition` | `TileObjectAnchorAndLiquidPlacementState` | `src2/WorldInteraction/Tiles/Definitions/TileObjectPlacementDefinition.cs` | 锚点、有效/无效 tile/wall、液体死亡和液体放置规则 |
| `proposed.TileObjectPlacementHookDefinition` | `TileObjectPlacementHookAndBaseState` | `src2/WorldInteraction/Tiles/Definitions/TileObjectPlacementHookDefinition.cs` | placement hooks、sub-tiles、TileObject base/coordinate module 和显式放置 command seam |
| `proposed.TileObjectStyleDefinitionAndSelection` | `TileObjectStyleDefinitionCatalogState`, `TileObjectStyleSelectionAndOverrideState` | `src2/WorldInteraction/Tiles/Definitions/TileObjectStyleDefinitionAndSelection.cs` | 固定 style presets、style geometry definition、override、visual skip 和 random style selection |
| `proposed.ContentValidationBoundary` | `SharedContentValidation` | `src2/Content/Validation/ContentValidationBoundary.cs` | 儿童安全集合、尺寸拒绝值对象和 vanilla texture metadata validation |
| `proposed.ContentPresentationCatalog` | `SharedContentPresentationCatalog` | `src2/Presentation/ContentPresentationCatalog.cs` | 颜色滑块值、字体/发型配置、NPC profile 变体和 config key 读取边界 |
| `proposed.ItemVariantCatalog` | `ItemVariantDefinitions` | `src2/Items/Definitions/ItemVariantCatalog.cs` | ItemVariant、条件、每 item 的 VariantEntry 和纯 variant selection |
| `proposed.LegacyItemPrefixCatalog` | `LegacyItemPrefixCatalog` | `src2/Items/Definitions/LegacyItemPrefixCatalog.cs` | 按武器类别的前缀数组、SetFactory 生成的 item category masks |
| `proposed.ArmorSetBonusDefinitionCatalog` | `ArmorSetBonusDefinitions` | `src2/Items/Armor/Definitions/ArmorSetBonusDefinitionCatalog.cs` | 套装定义、Builder staging、QueryContext/QueryResult 和纯 Complete qualification |
| `proposed.ArmorSetBonusLookupCatalog` | `ArmorSetBonusCatalog` | `src2/Items/Armor/Definitions/ArmorSetBonusLookupCatalog.cs` | All definition collection and derived SetsContaining lookup |
| `proposed.WingStatsDefinition` | `WingStatsDefinition` | `src2/Items/Armor/Definitions/WingStatsDefinition.cs` | wing FlyTime、acceleration/speed overrides 和 down-hover value definition |
| `proposed.ItemStaticCapabilityCatalog` | `SharedItemStaticCapabilityRules` | `src2/Items/Definitions/ItemStaticCapabilityCatalog.cs` | item spawn-count cache、armor-slot reverse indexes、staff/claw masks 和 phase-color presentation cache |

## 4. Shared invariants and identity rules

- Component 只保存内聚定义或稳定索引；factory cache、服务句柄、Asset/Texture/shader、随机输入、查询结果、放置 payload、网络 DTO 和持久化 DTO 不作为通用 ContentComponent。
- local content type、persistent ID、network ID、entity ID、ItemInstanceId、TileCoordinate、WorldSectionId 和外部 asset key 必须分别建模；任何共享值对象的最终 owner 标记 `crossSubsystemOwner: integration-review`。
- Query 只读取显式 snapshot/definition；System 才能提交状态转换；Command 才能跨到 WorldGrid、TileEntity、玩家装备、网络或持久化 writer。
- ContentSamples 的 `Item`/`NPC`/`Projectile` 是只读参考视图；不能把样本对象当作运行时实体，也不能让运行时实体反向修改样本目录。
- TileObjectData 的注册写入必须先于 read-only barrier；运行时 GetTileData、style/placement/geometry lookup 是只读。Hook effect 由 Adapter/Command 隔离。
- `ContentAbilityCatalog`、`ContentSampleIndexCatalog`、`ArmorSetBonusLookupCatalog` 和 `ItemStaticCapabilityCatalog` 的数组/字典都是派生或注册结果，必须声明失效和重建触发器。

## 5. Component and boundary contracts

### 1. `proposed.ContentAbilityCatalog`
- proposed target: `src2/Content/Runtime/ContentAbilityCatalog.cs`
- responsibility: 能力定义、Buff/Projectile 分类索引，以及隔离后的兼容表现标量。
- owner and lifecycle: proposed ContentAbilityRegistrationSystem；musicPitch 的表现写入由 proposed AudioPitchCompatibilityAdapter 隔离，最终 owner 为 integration-review；进程启动注册；能力表初始化后只读，musicPitch 为运行时表现输入，不进入世界快照。
- contract: 只读能力查询；不得让 Query 修改数组或把 Buff/Projectile 实体状态塞入目录。
- effects: 数组注册是内存效果；音频 pitch、日志、网络和持久化均在边界之外。
- evidence: D:\TRbackup\Version4\Terraria\Main.cs:406-428、3394-3398、5630-5854；Item.cs:974-980、48352。
- focused verifier: 能力表初始化快照、ID 边界、musicPitch 兼容读取和单写者检查。

### 2. `proposed.MainContentServiceReferenceBoundary`
- proposed target: `src2/Content/Runtime/MainContentServiceReferenceBoundary.cs`
- responsibility: 内容服务句柄和运行时服务适配边界，不是一个巨型组件。
- owner and lifecycle: proposed MainContentServiceBootstrapSystem 构造/替换句柄；各服务的 Update/Reset 仍由领域 System 通过 Adapter 执行，最终 owner 为 integration-review；Main.Initialize_AlmostEverything 创建；Ambience、Golf、Bestiary、Pylon 等生命周期不同，当前为 partial。
- contract: 以 typed port 暴露服务能力；禁止保存第三方对象到 ECS 核心。
- effects: 服务更新、实体生成、网络、外部资源和世界写入均为 Adapter/System 副作用。
- evidence: D:\TRbackup\Version4\Terraria\Main.cs:1013-1032、3283-3284、3353-3366、11333-11338、11471、12382-12386、13119；WorldGen.cs:6457-6565。
- focused verifier: 启动/清理生命周期、服务句柄隔离、服务顺序、失败和重试。

### 3. `proposed.ContentSampleIndexCatalog`
- proposed target: `src2/Content/Catalogs/ContentSampleIndexCatalog.cs`
- responsibility: 样本只读参考视图、persistent/network ID 映射和 Creative/Bestiary 派生索引。
- owner and lifecycle: proposed ContentSampleIndexBuildSystem 独占清空、构建、替换和派生索引；shader index 由 proposed ShaderIndexAdapter 处理；ContentSamples.Initialize 清空并重建；FixItemsAfterRecipesAreAdded、Bestiary/Creative sorting rebuild 后更新；世界卸载时失效。
- contract: 按内容类型查询样本和 ID 映射；样本对象不是运行时实体权威状态，persistent、network、local type 分开。
- effects: 样本构造、shader 读取、排序和字典重建是内容构建副作用；不直接持久化或网络复制字典。
- evidence: D:\TRbackup\Version4\Terraria.ID\ContentSamples.cs:13-15、93-122、833-855；Main.cs:3278、3409。
- focused verifier: Initialize 清空/重建、索引双射、排序稳定性、样本只读和 ID 隔离。

### 4. `proposed.SetArrayFactoryBoundary`
- proposed target: `src2/Content/Factories/SetArrayFactoryBoundary.cs`
- responsibility: 按 content ID 创建 bool/int/ushort/float/custom 集合的工厂和内部缓冲复用。
- owner and lifecycle: proposed SetArrayFactoryAdapter 独占 cache/lock；调用者只获得新建集合，不拥有 cache；工厂随 content ID count 初始化；cache 只在构造期间复用，不进入快照或持久化。
- contract: 验证 size、偶数输入和 ID 范围；返回集合由对应目录 owner 管理。
- effects: 锁、数组分配和 cache 回收是局部内存副作用，不设计成 ECS Component。
- evidence: D:\TRbackup\Version4\Terraria.ID\SetFactory.cs:8-18、26-185；PrefixLegacy.cs:67-81；ChildSafety.cs:7-15。
- focused verifier: 默认值/覆盖值、偶数参数、越界、并发 cache 和数组独立性。

### 5. `proposed.ColorAndShaderCatalog`
- proposed target: `src2/Presentation/Definitions/ColorAndShaderCatalog.cs`
- responsibility: 颜色常量、瀑布/液体调色板和染料 shader index 的表现定义。
- owner and lifecycle: proposed ColorCatalogLoadSystem 初始化颜色；proposed LiquidColorQuery 只计算 CurrentLiquidColor；shader ID 由 proposed ShaderIndexAdapter 写入；启动定义加载；CurrentLiquidColor 按显式 liquidAlpha 查询；shader handle 由资源 Adapter 管理。
- contract: 核心只保存稳定 ColorValue；XNA Color、shader handle 和 Main.liquidAlpha 不渗透内容组件。
- effects: CurrentLiquidColor 是纯 Query；shader lookup 是外部表现副作用，不反向写模拟状态。
- evidence: D:\TRbackup\Version4\Terraria.ID\Colors.cs:7-93；ContentSamples.cs:13-20；tModLoader class_colors.html、class_content_samples.html。
- focused verifier: 颜色常量、液体混合顺序、透明度、shader index 导入和句柄隔离。

### 6. `proposed.TileObjectInheritanceDefinition`
- proposed target: `src2/WorldInteraction/Tiles/Definitions/TileObjectInheritanceDefinition.cs`
- responsibility: TileObjectData parent、alternates、写时复制标志和注册期目录。
- owner and lifecycle: proposed TileObjectDefinitionRegistrationSystem 独占 parent/alternate/data/build scratch 写入；proposed TileObjectDefinitionQuery 只读；Main.Initialize_AlmostEverything -> TileObjectData.Initialize 注册；readOnlyData 后写入拒绝；运行时只查询。
- contract: 保持 CopyFrom/FullCopyFrom 的继承与写时复制；AlternatesCount 是派生查询。
- effects: 注册阶段内存构建；WriteCheck 是生命周期闸门；不直接写 WorldGrid。
- evidence: D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:14-76、126-203、1671、1698-1779、1844-1849、2142-2178。
- focused verifier: parent/copy/alternate 继承、read-only 闸门、注册后纯查询和重建幂等性。

### 7. `proposed.TileObjectGeometryDefinition`
- proposed target: `src2/WorldInteraction/Tiles/Definitions/TileObjectGeometryDefinition.cs`
- responsibility: TileObject 绘制偏移、尺寸、原点、方向和 frame 坐标几何定义。
- owner and lifecycle: proposed TileGeometryRegistrationSystem 独占注册写入；proposed TileGeometryQuery/TileDrawProjection 只读；注册期写入，Calculate 后缓存 derived coordinate dimensions；运行时按 tile/style 查询。
- contract: CoordinateFullWidth/Height 和 DrawStyleOffset 由定义与 Calculate 共同决定；几何不成为 Tile 实例状态。
- effects: 纯几何计算和渲染 projection；不触发世界写入。
- evidence: D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:36、60、778-910、1174-1285、1384-1637、5106-5145。
- focused verifier: frame geometry golden cases、origin/direction、coordinate cache 和 tile query。

### 8. `proposed.TileObjectPlacementDefinition`
- proposed target: `src2/WorldInteraction/Tiles/Definitions/TileObjectPlacementDefinition.cs`
- responsibility: 锚点、有效/无效 tile/wall、液体死亡和液体放置规则。
- owner and lifecycle: proposed TilePlacementDefinitionRegistrationSystem 独占注册写入；proposed TilePlacementRuleQuery 只读；注册期定义，运行时对显式 TileReadSnapshot 查询；液体规则不直接修改世界。
- contract: 保持 Water/LavaDeath 与 Water/LavaPlacement 组合；UsesGlobalLiquidChecks 不隐藏 writer。
- effects: LiquidPlace/CheckLavaDeath 只产生资格结果；实际 Tile/液体变更交给 integration command。
- evidence: D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:18-30、144-607、1844-1911；TileObject.cs:33-212；WorldGen.cs:41034、44543。
- focused verifier: anchor/liquid truth table、global/custom check、只读 world snapshot 和 rejected placement。

### 9. `proposed.TileObjectPlacementHookDefinition`
- proposed target: `src2/WorldInteraction/Tiles/Definitions/TileObjectPlacementHookDefinition.cs`
- responsibility: placement hooks、sub-tiles、TileObject base/coordinate module 和显式放置 command seam。
- owner and lifecycle: proposed TilePlacementHookRegistrationSystem 写定义；proposed TilePlacementCommandAdapter 执行 hook/TileEntity/world effects，最终 world writer 为 integration-review；hooks/subtiles 在注册期建立；placement input 短命；post-place 可能触发世界或 TileEntity 副作用。
- contract: Hook* 只能输出资格或 command，不直接改核心 Component；SubTiles 是定义目录。
- effects: PlacementHook 可能调用 WorldGen 或 TileEntity，必须隔离失败、重试和幂等。
- evidence: D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:32-42、54-64、640-750、2030-2140；TileObject.cs:33-212。
- focused verifier: hook invocation order、command emission/no direct write、subtile resolution、failure retry。

### 10. `proposed.TileObjectStyleDefinitionAndSelection`
- proposed target: `src2/WorldInteraction/Tiles/Definitions/TileObjectStyleDefinitionAndSelection.cs`
- responsibility: 固定 style presets、style geometry definition、override、visual skip 和 random style selection。
- owner and lifecycle: proposed TileStyleRegistrationSystem 写固定 definition/override；proposed TileStyleSelectionQuery 读取 explicit random input 并返回 style，不写定义；style preset 在 Initialize 注册；随机/覆盖在请求期间计算；选择结果不写回定义。
- contract: 注册期权威定义、样式覆盖、随机选择、派生步进和纯 Query 分开；SpecificRandomStyles 优先级待闭合。
- effects: 随机源通过显式 port；visual override 只投影表现；不直接写 Tile store。
- evidence: D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:38、62、78-124、728、943-1141、1318-1378、2142-2205。
- focused verifier: style preset、override precedence、random range/specific list、deterministic RNG and no mutation。

### 11. `proposed.ContentValidationBoundary`
- proposed target: `src2/Content/Validation/ContentValidationBoundary.cs`
- responsibility: 儿童安全集合、尺寸拒绝值对象和 vanilla texture metadata validation。
- owner and lifecycle: proposed ContentValidationLoadSystem 载入 safe sets/metadata；proposed ContentValidationQuery 纯校验；reject reason 只作结果；启动读取并冻结安全/metadata 定义；卸载时丢弃 validator；clear/reload 证据 partial。
- contract: 校验输出 Accepted 或 typed Rejection，不写内容目录；纹理尺寸由 adapter 提供。
- effects: embedded resource、纹理元数据和日志由 Adapter 处理；Dictionary/Asset 不进入核心。
- evidence: D:\TRbackup\Version4\Terraria.GameContent\ChildSafety.cs:7-15；ContentRejectionFromSize.cs:8-14；VanillaContentValidator.cs:13-18。
- focused verifier: safe sets、尺寸 mismatch、metadata parse、asset failure and reload cleanup。

### 12. `proposed.ContentPresentationCatalog`
- proposed target: `src2/Presentation/ContentPresentationCatalog.cs`
- responsibility: 颜色滑块值、字体/发型配置、NPC profile 变体和 config key 读取边界。
- owner and lifecycle: proposed PresentationCatalogLoadSystem 写 typed presentation definitions；Font/Texture 由 proposed PresentationAssetAdapter 管理；presentation resource load/unload；ColorSlidersSet 可为 UI payload，不能成为模拟权威。
- contract: NameKey/ConfigKey 是稳定外部键；Asset<T>、Texture2D、profile 和 runtime dictionary 通过 adapter/projection。
- effects: 资源加载、随机 profile、UI rendering 和 localization 是外部副作用。
- evidence: D:\TRbackup\Version4\Terraria.DataStructures\ColorSlidersSet.cs:7-13；FontAssets.cs:9；HairstyleUnlocksHelper.cs:7；Profiles.cs:12-104；IConfigKeyHolder.cs:5-7。
- focused verifier: asset lifetime、variant profile、config key stability、no external type in core。

### 13. `proposed.ItemVariantCatalog`
- proposed target: `src2/Items/Definitions/ItemVariantCatalog.cs`
- responsibility: ItemVariant、条件、每 item 的 VariantEntry 和纯 variant selection。
- owner and lifecycle: proposed ItemVariantRegistrationSystem 内容初始化注册；proposed ItemVariantSelectionQuery 只读显式 WorldRuleSnapshot；static constructor/content load registration；selection per item request；Item.Variant 实例字段由 item domain owner 管理。
- contract: Condition delegate 转换为显式 predicate/adapter，Query 不隐式读取 Main；Conditions 只读枚举视图。
- effects: NetworkText/localization 和 world-rule reads 通过 adapter；选择不写 catalog。
- evidence: D:\TRbackup\Version4\Terraria.GameContent.Items\ItemVariants.cs:12-16、38-54；ItemVariant.cs:7；ItemVariantCondition.cs:9-11。
- focused verifier: registration order、condition truth table、out-of-range、selection determinism and instance separation。

### 14. `proposed.LegacyItemPrefixCatalog`
- proposed target: `src2/Items/Definitions/LegacyItemPrefixCatalog.cs`
- responsibility: 按武器类别的前缀数组、SetFactory 生成的 item category masks。
- owner and lifecycle: proposed LegacyPrefixRegistrationSystem 独占初始化；prefix selection/roll System 只读；content setup static definitions；arrays immutable after setup unless explicit reload; readers/writers partial。
- contract: prefix category IDs 与 item type ID 分开；Factory cache 不属于 prefix state；不写 Item instance prefix。
- effects: 数组构造是内存副作用；随机 prefix selection 使用显式 random port，玩家/网络 owner integration-review。
- evidence: D:\TRbackup\Version4\Terraria.GameContent.Prefixes\PrefixLegacy.cs:9-81；SetFactory.cs:74-185；Item/Player readers partial。
- focused verifier: category membership、array lengths、legacy ordering、random selection compatibility and no instance mutation。

### 15. `proposed.ArmorSetBonusDefinitionCatalog`
- proposed target: `src2/Items/Armor/Definitions/ArmorSetBonusDefinitionCatalog.cs`
- responsibility: 套装定义、Builder staging、QueryContext/QueryResult 和纯 Complete qualification。
- owner and lifecycle: proposed ArmorSetBonusDefinitionRegistrationSystem 构建 immutable definitions；proposed ArmorSetQualificationQuery 计算 result；ArmorSetBonuses.Initialize registration；QueryContext/Result 每次查询短命；LocalizedText/effect execution outside catalog。
- contract: Head/Body/Legs 是 definition references；Complete 只比较 ItemsNeeded/ItemsFound；Effect callback 不在 Query 执行。
- effects: LocalizedText resolution and ArmorSetEffect application are external/player integration effects。
- evidence: D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonus.cs:18-48、59-72、145-172；ArmorSetBonuses.cs:520-616。
- focused verifier: builder normalization、query count、Complete truth table、effect not executed and localization boundary。

### 16. `proposed.ArmorSetBonusLookupCatalog`
- proposed target: `src2/Items/Armor/Definitions/ArmorSetBonusLookupCatalog.cs`
- responsibility: All definition collection and derived SetsContaining lookup。
- owner and lifecycle: proposed ArmorSetBonusLookupBuildSystem writes All/lookup during setup; proposed ArmorSetQualificationQuery reads lookup；Initialize then BuildLookup; lookup only explicit content reload; no per-player writes。
- contract: SetsContaining derives from All by head/body/legs and index 0 is empty; GetCompleteSet is pure search。
- effects: array allocation/grouping only; player equipment effect commit is outside P12。
- evidence: D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonuses.cs:516-656；ArmorSetBonus.cs:168-190。
- focused verifier: lookup rebuild、duplicate/zero IDs、result ordering、missing index and player integration projection。

### 17. `proposed.WingStatsDefinition`
- proposed target: `src2/Items/Armor/Definitions/WingStatsDefinition.cs`
- responsibility: wing FlyTime、acceleration/speed overrides 和 down-hover value definition。
- owner and lifecycle: proposed WingStatsRegistrationSystem writes typed catalog; proposed ArmorWingCompatibilityAdapter projects external wing table, final owner integration-review；WingStatsInitializer.Load during setup; immutable after load; Default is value-definition fallback。
- contract: six instance values travel together；HasDownHoverStats gates down-hover overrides；不含 Player movement state。
- effects: projection into ArmorIDs.Wing.Sets.Stats and movement consumption are external。
- evidence: D:\TRbackup\Version4\Terraria.DataStructures\WingStats.cs:3-19；WingStatsInitializer.cs:6-70。
- focused verifier: all wing IDs、default fallback、hover gate、projected table equality and no player-state write。

### 18. `proposed.ItemStaticCapabilityCatalog`
- proposed target: `src2/Items/Definitions/ItemStaticCapabilityCatalog.cs`
- responsibility: item spawn-count cache、armor-slot reverse indexes、staff/claw masks 和 phase-color presentation cache。
- owner and lifecycle: proposed ItemStaticCapabilityRegistrationSystem writes reverse indexes/staff/claw; proposed ItemSpawnCacheAdapter owns count cache; proposed PhaseColorProjection owns lazy colors；registration in Main.Initialize_Items; cachedItemSpawnsByType and _phaseColors have runtime/lazy invalidation; reset semantics partial。
- contract: reverse maps and masks are content indexes; spawn counts and phase colors are derived caches; none is Item instance authority。
- effects: Item.NewItem updates spawn cache; GetPhaseColor reads Main.LocalPlayer and lazily builds colors; cross-domain commit integration-review。
- evidence: D:\TRbackup\Version4\Terraria\Item.cs:68-88、311、333-337、48502-48546、48756-48758；Main.cs:3470-3566；WorldGen.cs:52635-52643。
- focused verifier: index initialization、display-doll reverse lookup、cache invalidation、phase interpolation and single-writer guard。

## 6. Dependency and proposed scheduler order

```text
ContentIdentity/ID inputs (crossSubsystemOwner: integration-review)
  -> proposed SetArrayFactoryBoundary
  -> proposed ContentAbilityCatalog / proposed LegacyItemPrefixCatalog / proposed ContentValidationBoundary
  -> proposed ContentSampleIndexCatalog
  -> proposed TileObjectInheritanceDefinition
       -> proposed TileObjectGeometryDefinition
       -> proposed TileObjectPlacementDefinition
       -> proposed TileObjectPlacementHookDefinition
       -> proposed TileObjectStyleDefinitionAndSelection
  -> proposed ItemVariantCatalog / proposed ArmorSetBonusDefinitionCatalog
       -> proposed ArmorSetBonusLookupCatalog / proposed WingStatsDefinition / proposed ItemStaticCapabilityCatalog
  -> proposed ContentPresentationCatalog / proposed ColorAndShaderCatalog
  -> projections and explicit commands to WorldInteraction, Item, Player, UI, Network and Persistence
```

这是 P12 的 proposed 顺序候选，不是全局裁决。文件/目录顺序不能承担调度语义；最终 System phase、world commit、player equipment commit、network snapshot 和 persistence commit 均为 integration-review。

## 7. Complete member mapping

下列 19 个章节逐条覆盖输入报告全部 285 条成员。每行保留来源序号、成员类型、声明类型、成员名、C# 类型、Version4 实际路径/行号和 proposed 归属。

### MainContentAbilityTables -> `proposed.ContentAbilityCatalog`

成员数：12。所有成员均逐条映射；Version4 证据根路径：`D:\TRbackup\Version4\Terraria\Main.cs`。

| Seq | Source member | Version4 evidence | proposed target role | Evidence/ownership state |
|---:|---|---|---|---|
| 123 | field Terraria.Main.MaxWorldViewSize : Microsoft.Xna.Framework.Point | D:\TRbackup\Version4\Terraria\Main.cs:406 | proposed.ContentAbilityCatalog / immutable view-size definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 124 | field Terraria.Main.musicPitch : float | D:\TRbackup\Version4\Terraria\Main.cs:408 | proposed.ContentAbilityCatalog / presentation/audio compatibility scalar via proposed adapter | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 125 | field Terraria.Main.projHostile : bool[] | D:\TRbackup\Version4\Terraria\Main.cs:410 | proposed.ContentAbilityCatalog / projectile capability index | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 126 | field Terraria.Main.projHook : bool[] | D:\TRbackup\Version4\Terraria\Main.cs:412 | proposed.ContentAbilityCatalog / projectile capability index | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 127 | field Terraria.Main.pvpBuff : bool[] | D:\TRbackup\Version4\Terraria\Main.cs:414 | proposed.ContentAbilityCatalog / Buff capability/index rule | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 128 | field Terraria.Main.persistentBuff : bool[] | D:\TRbackup\Version4\Terraria\Main.cs:416 | proposed.ContentAbilityCatalog / Buff capability/index rule | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 129 | field Terraria.Main.vanityPet : bool[] | D:\TRbackup\Version4\Terraria\Main.cs:418 | proposed.ContentAbilityCatalog / Buff capability/index rule | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 130 | field Terraria.Main.lightPet : bool[] | D:\TRbackup\Version4\Terraria\Main.cs:420 | proposed.ContentAbilityCatalog / Buff capability/index rule | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 131 | field Terraria.Main.meleeBuff : bool[] | D:\TRbackup\Version4\Terraria\Main.cs:422 | proposed.ContentAbilityCatalog / Buff capability/index rule | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 132 | field Terraria.Main.debuff : bool[] | D:\TRbackup\Version4\Terraria\Main.cs:424 | proposed.ContentAbilityCatalog / Buff capability/index rule | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 133 | field Terraria.Main.buffNoSave : bool[] | D:\TRbackup\Version4\Terraria\Main.cs:426 | proposed.ContentAbilityCatalog / Buff capability/index rule | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 134 | field Terraria.Main.buffNoTimeDisplay : bool[] | D:\TRbackup\Version4\Terraria\Main.cs:428 | proposed.ContentAbilityCatalog / Buff capability/index rule | declaration confirmed; member-level reader/writer/lifecycle closure partial |

### MainContentCatalogAndSimulationServices -> `proposed.MainContentServiceReferenceBoundary`

成员数：10。所有成员均逐条映射；Version4 证据根路径：`D:\TRbackup\Version4\Terraria\Main.cs`。

| Seq | Source member | Version4 evidence | proposed target role | Evidence/ownership state |
|---:|---|---|---|---|
| 412 | field Terraria.Main.AmbienceServer : Terraria.GameContent.Ambience.AmbienceServer | D:\TRbackup\Version4\Terraria\Main.cs:1013 | proposed.MainContentServiceReferenceBoundary / service adapter reference | confirmed locally; external effect owner partial; crossSubsystemOwner: integration-review |
| 413 | field Terraria.Main.ItemDropsDB : Terraria.GameContent.ItemDropRules.ItemDropDatabase | D:\TRbackup\Version4\Terraria\Main.cs:1015 | proposed.MainContentServiceReferenceBoundary / service adapter reference | confirmed locally; external effect owner partial; crossSubsystemOwner: integration-review |
| 414 | field Terraria.Main.FishDropsDB : Terraria.GameContent.FishDropRules.FishDropRuleList | D:\TRbackup\Version4\Terraria\Main.cs:1017 | proposed.MainContentServiceReferenceBoundary / service adapter reference | confirmed locally; external effect owner partial; crossSubsystemOwner: integration-review |
| 415 | field Terraria.Main.BestiaryDB : Terraria.GameContent.Bestiary.BestiaryDatabase | D:\TRbackup\Version4\Terraria\Main.cs:1019 | proposed.MainContentServiceReferenceBoundary / service adapter reference | confirmed locally; external effect owner partial; crossSubsystemOwner: integration-review |
| 416 | field Terraria.Main.ItemDropSolver : Terraria.GameContent.ItemDropRules.ItemDropResolver | D:\TRbackup\Version4\Terraria\Main.cs:1021 | proposed.MainContentServiceReferenceBoundary / service adapter reference | confirmed locally; external effect owner partial; crossSubsystemOwner: integration-review |
| 417 | field Terraria.Main.BestiaryTracker : Terraria.GameContent.Bestiary.BestiaryUnlocksTracker | D:\TRbackup\Version4\Terraria\Main.cs:1023 | proposed.MainContentServiceReferenceBoundary / service adapter reference | confirmed locally; external effect owner partial; crossSubsystemOwner: integration-review |
| 418 | field Terraria.Main.PylonSystem : Terraria.GameContent.TeleportPylonsSystem | D:\TRbackup\Version4\Terraria\Main.cs:1026 | proposed.MainContentServiceReferenceBoundary / service adapter reference | confirmed locally; external effect owner partial; crossSubsystemOwner: integration-review |
| 419 | field Terraria.Main.ShopHelper : Terraria.GameContent.ShopHelper | D:\TRbackup\Version4\Terraria\Main.cs:1028 | proposed.MainContentServiceReferenceBoundary / service adapter reference | confirmed locally; external effect owner partial; crossSubsystemOwner: integration-review |
| 420 | field Terraria.Main.LocalGolfState : Terraria.GameContent.Golf.GolfState | D:\TRbackup\Version4\Terraria\Main.cs:1030 | proposed.MainContentServiceReferenceBoundary / service adapter reference | confirmed locally; external effect owner partial; crossSubsystemOwner: integration-review |
| 421 | field Terraria.Main.DroneCameraTracker : Terraria.DataStructures.DroneCameraTracker | D:\TRbackup\Version4\Terraria\Main.cs:1032 | proposed.MainContentServiceReferenceBoundary / service adapter reference | confirmed locally; external effect owner partial; crossSubsystemOwner: integration-review |

### ContentSamplesCatalog -> `proposed.ContentSampleIndexCatalog`

成员数：21。所有成员均逐条映射；Version4 证据根路径：`D:\TRbackup\Version4\Terraria.ID\ContentSamples.cs`。

| Seq | Source member | Version4 evidence | proposed target role | Evidence/ownership state |
|---:|---|---|---|---|
| 842 | field Terraria.ID.ContentSamples.DyeShaderIDs.TeamDyeShaderIndex : int | D:\TRbackup\Version4\Terraria.ID\ContentSamples.cs:13 | proposed.ContentSampleIndexCatalog / shader index projection | confirmed for clear/rebuild path; complete consumer matrix partial |
| 843 | field Terraria.ID.ContentSamples.DyeShaderIDs.ColorOnlyShaderIndex : int | D:\TRbackup\Version4\Terraria.ID\ContentSamples.cs:15 | proposed.ContentSampleIndexCatalog / shader index projection | confirmed for clear/rebuild path; complete consumer matrix partial |
| 844 | field Terraria.ID.ContentSamples.CreativeHelper.ItemGroupAndOrderInGroup.ItemType : int | D:\TRbackup\Version4\Terraria.ID\ContentSamples.cs:93 | proposed.ContentSampleIndexCatalog / creative sort value object | confirmed for clear/rebuild path; complete consumer matrix partial |
| 845 | field Terraria.ID.ContentSamples.CreativeHelper.ItemGroupAndOrderInGroup.Group : Terraria.ID.ContentSamples.CreativeHelper.ItemGroup | D:\TRbackup\Version4\Terraria.ID\ContentSamples.cs:95 | proposed.ContentSampleIndexCatalog / creative sort value object | confirmed for clear/rebuild path; complete consumer matrix partial |
| 846 | field Terraria.ID.ContentSamples.CreativeHelper.ItemGroupAndOrderInGroup.OrderInGroup : int | D:\TRbackup\Version4\Terraria.ID\ContentSamples.cs:97 | proposed.ContentSampleIndexCatalog / creative sort value object | confirmed for clear/rebuild path; complete consumer matrix partial |
| 847 | field Terraria.ID.ContentSamples.CreativeHelper._manualEventItemsOrder : System.Collections.Generic.List<int> | D:\TRbackup\Version4\Terraria.ID\ContentSamples.cs:106 | proposed.ContentSampleIndexCatalog / creative manual ordering definition | confirmed for clear/rebuild path; complete consumer matrix partial |
| 848 | field Terraria.ID.ContentSamples.CreativeHelper._manualBossSpawnItemsOrder : System.Collections.Generic.List<int> | D:\TRbackup\Version4\Terraria.ID\ContentSamples.cs:108 | proposed.ContentSampleIndexCatalog / creative manual ordering definition | confirmed for clear/rebuild path; complete consumer matrix partial |
| 849 | field Terraria.ID.ContentSamples.CreativeHelper._manualCraftingStations : System.Collections.Generic.List<int> | D:\TRbackup\Version4\Terraria.ID\ContentSamples.cs:114 | proposed.ContentSampleIndexCatalog / creative manual ordering definition | confirmed for clear/rebuild path; complete consumer matrix partial |
| 850 | field Terraria.ID.ContentSamples.CreativeHelper._manualGolfItemsOrder : System.Collections.Generic.List<int> | D:\TRbackup\Version4\Terraria.ID\ContentSamples.cs:122 | proposed.ContentSampleIndexCatalog / creative manual ordering definition | confirmed for clear/rebuild path; complete consumer matrix partial |
| 851 | field Terraria.ID.ContentSamples.NpcsByNetId : System.Collections.Generic.Dictionary<int, Terraria.NPC> | D:\TRbackup\Version4\Terraria.ID\ContentSamples.cs:833 | proposed.ContentSampleIndexCatalog / persistent/network ID mapping index | confirmed for clear/rebuild path; complete consumer matrix partial |
| 852 | field Terraria.ID.ContentSamples.ProjectilesByType : System.Collections.Generic.Dictionary<int, Terraria.Projectile> | D:\TRbackup\Version4\Terraria.ID\ContentSamples.cs:835 | proposed.ContentSampleIndexCatalog / content sample reference index | confirmed for clear/rebuild path; complete consumer matrix partial |
| 853 | field Terraria.ID.ContentSamples.ItemsByType : System.Collections.Generic.Dictionary<int, Terraria.Item> | D:\TRbackup\Version4\Terraria.ID\ContentSamples.cs:837 | proposed.ContentSampleIndexCatalog / content sample reference index | confirmed for clear/rebuild path; complete consumer matrix partial |
| 854 | field Terraria.ID.ContentSamples.ItemNetIdsByPersistentIds : System.Collections.Generic.Dictionary<string, int> | D:\TRbackup\Version4\Terraria.ID\ContentSamples.cs:839 | proposed.ContentSampleIndexCatalog / persistent/network ID mapping index | confirmed for clear/rebuild path; complete consumer matrix partial |
| 855 | field Terraria.ID.ContentSamples.ItemPersistentIdsByNetIds : System.Collections.Generic.Dictionary<int, string> | D:\TRbackup\Version4\Terraria.ID\ContentSamples.cs:841 | proposed.ContentSampleIndexCatalog / persistent/network ID mapping index | confirmed for clear/rebuild path; complete consumer matrix partial |
| 856 | field Terraria.ID.ContentSamples.CreativeResearchItemPersistentIdOverride : System.Collections.Generic.Dictionary<int, int> | D:\TRbackup\Version4\Terraria.ID\ContentSamples.cs:843 | proposed.ContentSampleIndexCatalog / persistent/network ID mapping index | confirmed for clear/rebuild path; complete consumer matrix partial |
| 857 | field Terraria.ID.ContentSamples.NpcNetIdsByPersistentIds : System.Collections.Generic.Dictionary<string, int> | D:\TRbackup\Version4\Terraria.ID\ContentSamples.cs:845 | proposed.ContentSampleIndexCatalog / persistent/network ID mapping index | confirmed for clear/rebuild path; complete consumer matrix partial |
| 858 | field Terraria.ID.ContentSamples.NpcPersistentIdsByNetIds : System.Collections.Generic.Dictionary<int, string> | D:\TRbackup\Version4\Terraria.ID\ContentSamples.cs:847 | proposed.ContentSampleIndexCatalog / persistent/network ID mapping index | confirmed for clear/rebuild path; complete consumer matrix partial |
| 859 | field Terraria.ID.ContentSamples.NpcBestiarySortingId : System.Collections.Generic.Dictionary<int, int> | D:\TRbackup\Version4\Terraria.ID\ContentSamples.cs:849 | proposed.ContentSampleIndexCatalog / bestiary derived index | confirmed for clear/rebuild path; complete consumer matrix partial |
| 860 | field Terraria.ID.ContentSamples.NpcBestiaryRarityStars : System.Collections.Generic.Dictionary<int, int> | D:\TRbackup\Version4\Terraria.ID\ContentSamples.cs:851 | proposed.ContentSampleIndexCatalog / bestiary derived index | confirmed for clear/rebuild path; complete consumer matrix partial |
| 861 | field Terraria.ID.ContentSamples.NpcBestiaryCreditIdsByNpcNetIds : System.Collections.Generic.Dictionary<int, string> | D:\TRbackup\Version4\Terraria.ID\ContentSamples.cs:853 | proposed.ContentSampleIndexCatalog / persistent/network ID mapping index | confirmed for clear/rebuild path; complete consumer matrix partial |
| 862 | field Terraria.ID.ContentSamples.ItemCreativeSortingId : System.Collections.Generic.Dictionary<int, Terraria.ID.ContentSamples.CreativeHelper.ItemGroupAndOrderInGroup> | D:\TRbackup\Version4\Terraria.ID\ContentSamples.cs:855 | proposed.ContentSampleIndexCatalog / content sample reference index | confirmed for clear/rebuild path; complete consumer matrix partial |

### ContentSetFactory -> `proposed.SetArrayFactoryBoundary`

成员数：6。所有成员均逐条映射；Version4 证据根路径：`D:\TRbackup\Version4\Terraria.ID\SetFactory.cs`。

| Seq | Source member | Version4 evidence | proposed target role | Evidence/ownership state |
|---:|---|---|---|---|
| 863 | field Terraria.ID.SetFactory._size : int | D:\TRbackup\Version4\Terraria.ID\SetFactory.cs:8 | proposed.SetArrayFactoryBoundary / factory implementation state; no ECS component | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 864 | field Terraria.ID.SetFactory._intBufferCache : System.Collections.Generic.Queue<int[]> | D:\TRbackup\Version4\Terraria.ID\SetFactory.cs:10 | proposed.SetArrayFactoryBoundary / factory implementation state; no ECS component | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 865 | field Terraria.ID.SetFactory._ushortBufferCache : System.Collections.Generic.Queue<ushort[]> | D:\TRbackup\Version4\Terraria.ID\SetFactory.cs:12 | proposed.SetArrayFactoryBoundary / factory implementation state; no ECS component | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 866 | field Terraria.ID.SetFactory._boolBufferCache : System.Collections.Generic.Queue<bool[]> | D:\TRbackup\Version4\Terraria.ID\SetFactory.cs:14 | proposed.SetArrayFactoryBoundary / factory implementation state; no ECS component | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 867 | field Terraria.ID.SetFactory._floatBufferCache : System.Collections.Generic.Queue<float[]> | D:\TRbackup\Version4\Terraria.ID\SetFactory.cs:16 | proposed.SetArrayFactoryBoundary / factory implementation state; no ECS component | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 868 | field Terraria.ID.SetFactory._queueLock : object | D:\TRbackup\Version4\Terraria.ID\SetFactory.cs:18 | proposed.SetArrayFactoryBoundary / factory implementation state; no ECS component | declaration confirmed; member-level reader/writer/lifecycle closure partial |

### ColorAndShaderSetCatalog -> `proposed.ColorAndShaderCatalog`

成员数：25。所有成员均逐条映射；Version4 证据根路径：`D:\TRbackup\Version4\Terraria.ID\Colors.cs`。

| Seq | Source member | Version4 evidence | proposed target role | Evidence/ownership state |
|---:|---|---|---|---|
| 818 | field Terraria.ID.Colors.RarityAmber : Color | D:\TRbackup\Version4\Terraria.ID\Colors.cs:7 | proposed.ColorAndShaderCatalog / immutable color definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 819 | field Terraria.ID.Colors.RarityTrash : Color | D:\TRbackup\Version4\Terraria.ID\Colors.cs:9 | proposed.ColorAndShaderCatalog / immutable color definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 820 | field Terraria.ID.Colors.RarityBlue : Color | D:\TRbackup\Version4\Terraria.ID\Colors.cs:11 | proposed.ColorAndShaderCatalog / immutable color definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 821 | field Terraria.ID.Colors.RarityGreen : Color | D:\TRbackup\Version4\Terraria.ID\Colors.cs:13 | proposed.ColorAndShaderCatalog / immutable color definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 822 | field Terraria.ID.Colors.RarityOrange : Color | D:\TRbackup\Version4\Terraria.ID\Colors.cs:15 | proposed.ColorAndShaderCatalog / immutable color definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 823 | field Terraria.ID.Colors.RarityRed : Color | D:\TRbackup\Version4\Terraria.ID\Colors.cs:17 | proposed.ColorAndShaderCatalog / immutable color definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 824 | field Terraria.ID.Colors.RarityPink : Color | D:\TRbackup\Version4\Terraria.ID\Colors.cs:19 | proposed.ColorAndShaderCatalog / immutable color definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 825 | field Terraria.ID.Colors.RarityPurple : Color | D:\TRbackup\Version4\Terraria.ID\Colors.cs:21 | proposed.ColorAndShaderCatalog / immutable color definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 826 | field Terraria.ID.Colors.RarityLime : Color | D:\TRbackup\Version4\Terraria.ID\Colors.cs:23 | proposed.ColorAndShaderCatalog / immutable color definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 827 | field Terraria.ID.Colors.RarityYellow : Color | D:\TRbackup\Version4\Terraria.ID\Colors.cs:25 | proposed.ColorAndShaderCatalog / immutable color definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 828 | field Terraria.ID.Colors.RarityCyan : Color | D:\TRbackup\Version4\Terraria.ID\Colors.cs:27 | proposed.ColorAndShaderCatalog / immutable color definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 829 | field Terraria.ID.Colors.CoinPlatinum : Color | D:\TRbackup\Version4\Terraria.ID\Colors.cs:29 | proposed.ColorAndShaderCatalog / immutable color definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 830 | field Terraria.ID.Colors.CoinGold : Color | D:\TRbackup\Version4\Terraria.ID\Colors.cs:31 | proposed.ColorAndShaderCatalog / immutable color definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 831 | field Terraria.ID.Colors.CoinSilver : Color | D:\TRbackup\Version4\Terraria.ID\Colors.cs:33 | proposed.ColorAndShaderCatalog / immutable color definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 832 | field Terraria.ID.Colors.CoinCopper : Color | D:\TRbackup\Version4\Terraria.ID\Colors.cs:35 | proposed.ColorAndShaderCatalog / immutable color definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 833 | field Terraria.ID.Colors.AmbientNPCGastropodLight : Color | D:\TRbackup\Version4\Terraria.ID\Colors.cs:37 | proposed.ColorAndShaderCatalog / immutable color definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 834 | field Terraria.ID.Colors.JourneyMode : Color | D:\TRbackup\Version4\Terraria.ID\Colors.cs:39 | proposed.ColorAndShaderCatalog / immutable color definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 835 | field Terraria.ID.Colors.Mediumcore : Color | D:\TRbackup\Version4\Terraria.ID\Colors.cs:41 | proposed.ColorAndShaderCatalog / immutable color definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 836 | field Terraria.ID.Colors.Hardcore : Color | D:\TRbackup\Version4\Terraria.ID\Colors.cs:43 | proposed.ColorAndShaderCatalog / immutable color definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 837 | field Terraria.ID.Colors.LanternBG : Color | D:\TRbackup\Version4\Terraria.ID\Colors.cs:45 | proposed.ColorAndShaderCatalog / immutable color definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 838 | field Terraria.ID.Colors._waterfallColors : Color[] | D:\TRbackup\Version4\Terraria.ID\Colors.cs:47 | proposed.ColorAndShaderCatalog / liquid/waterfall palette definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 839 | field Terraria.ID.Colors._liquidColors : Color[] | D:\TRbackup\Version4\Terraria.ID\Colors.cs:73 | proposed.ColorAndShaderCatalog / liquid/waterfall palette definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 840 | field Terraria.ID.Colors.InventoryDefaultColor : Color | D:\TRbackup\Version4\Terraria.ID\Colors.cs:89 | proposed.ColorAndShaderCatalog / immutable color definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 841 | field Terraria.ID.Colors.InventoryDefaultColorWithOpacity : Color | D:\TRbackup\Version4\Terraria.ID\Colors.cs:91 | proposed.ColorAndShaderCatalog / immutable color definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 925 | property Terraria.ID.Colors.CurrentLiquidColor : Color | D:\TRbackup\Version4\Terraria.ID\Colors.cs:93 | proposed.ColorAndShaderCatalog / pure liquid color query | declaration confirmed; member-level reader/writer/lifecycle closure partial |

### TileObjectDefinitionInheritanceState -> `proposed.TileObjectInheritanceDefinition`

成员数：13。所有成员均逐条映射；Version4 证据根路径：`D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs`。

| Seq | Source member | Version4 evidence | proposed target role | Evidence/ownership state |
|---:|---|---|---|---|
| 869 | field Terraria.ObjectData.TileObjectData._parent : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:14 | proposed.TileObjectInheritanceDefinition / parent/alternate inheritance definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 870 | field Terraria.ObjectData.TileObjectData._linkedAlternates : bool | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:16 | proposed.TileObjectInheritanceDefinition / parent/alternate inheritance definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 873 | field Terraria.ObjectData.TileObjectData._alternates : Terraria.Modules.TileObjectAlternatesModule | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:22 | proposed.TileObjectInheritanceDefinition / parent/alternate inheritance definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 884 | field Terraria.ObjectData.TileObjectData._hasOwnAlternates : bool | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:44 | proposed.TileObjectInheritanceDefinition / parent/alternate inheritance definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 895 | field Terraria.ObjectData.TileObjectData._data : System.Collections.Generic.List<Terraria.ObjectData.TileObjectData> | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:66 | proposed.TileObjectInheritanceDefinition / tile definition catalog index | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 896 | field Terraria.ObjectData.TileObjectData._baseObject : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:68 | proposed.TileObjectInheritanceDefinition / parent/alternate inheritance definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 897 | field Terraria.ObjectData.TileObjectData.readOnlyData : bool | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:70 | proposed.TileObjectInheritanceDefinition / registration lifecycle barrier | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 898 | field Terraria.ObjectData.TileObjectData.newTile : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:72 | proposed.TileObjectInheritanceDefinition / registration scratch definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 899 | field Terraria.ObjectData.TileObjectData.newSubTile : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:74 | proposed.TileObjectInheritanceDefinition / registration scratch definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 900 | field Terraria.ObjectData.TileObjectData.newAlternate : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:76 | proposed.TileObjectInheritanceDefinition / registration scratch definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 926 | property Terraria.ObjectData.TileObjectData.LinkedAlternates : bool | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:126 | proposed.TileObjectInheritanceDefinition / parent/alternate inheritance definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 929 | property Terraria.ObjectData.TileObjectData.Alternates : System.Collections.Generic.List<Terraria.ObjectData.TileObjectData> | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:170 | proposed.TileObjectInheritanceDefinition / parent/alternate inheritance definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 976 | property Terraria.ObjectData.TileObjectData.AlternatesCount : int | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:1671 | proposed.TileObjectInheritanceDefinition / parent/alternate inheritance definition | registration/read-only boundary confirmed; runtime consumer matrix partial |

### TileObjectDrawGeometryState -> `proposed.TileObjectGeometryDefinition`

成员数：20。所有成员均逐条映射；Version4 证据根路径：`D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs`。

| Seq | Source member | Version4 evidence | proposed target role | Evidence/ownership state |
|---:|---|---|---|---|
| 880 | field Terraria.ObjectData.TileObjectData._tileObjectDraw : Terraria.Modules.TileObjectDrawModule | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:36 | proposed.TileObjectGeometryDefinition / geometry module ownership flag | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 892 | field Terraria.ObjectData.TileObjectData._hasOwnTileObjectDraw : bool | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:60 | proposed.TileObjectGeometryDefinition / geometry module ownership flag | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 949 | property Terraria.ObjectData.TileObjectData.DrawYOffset : int | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:778 | proposed.TileObjectGeometryDefinition / draw/placement geometry definition or derived query | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 950 | property Terraria.ObjectData.TileObjectData.DrawXOffset : int | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:811 | proposed.TileObjectGeometryDefinition / draw/placement geometry definition or derived query | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 951 | property Terraria.ObjectData.TileObjectData.DrawFlipHorizontal : bool | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:844 | proposed.TileObjectGeometryDefinition / draw/placement geometry definition or derived query | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 952 | property Terraria.ObjectData.TileObjectData.DrawFlipVertical : bool | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:877 | proposed.TileObjectGeometryDefinition / draw/placement geometry definition or derived query | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 953 | property Terraria.ObjectData.TileObjectData.DrawStepDown : int | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:910 | proposed.TileObjectGeometryDefinition / draw/placement geometry definition or derived query | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 961 | property Terraria.ObjectData.TileObjectData.Width : int | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:1174 | proposed.TileObjectGeometryDefinition / draw/placement geometry definition or derived query | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 962 | property Terraria.ObjectData.TileObjectData.Height : int | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:1213 | proposed.TileObjectGeometryDefinition / draw/placement geometry definition or derived query | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 963 | property Terraria.ObjectData.TileObjectData.Origin : Terraria.DataStructures.Point16 | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:1252 | proposed.TileObjectGeometryDefinition / draw/placement geometry definition or derived query | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 964 | property Terraria.ObjectData.TileObjectData.Direction : Terraria.Enums.TileObjectDirection | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:1285 | proposed.TileObjectGeometryDefinition / draw/placement geometry definition or derived query | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 967 | property Terraria.ObjectData.TileObjectData.FlattenAnchors : bool | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:1384 | proposed.TileObjectGeometryDefinition / draw/placement geometry definition or derived query | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 968 | property Terraria.ObjectData.TileObjectData.CoordinateHeights : int[] | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:1417 | proposed.TileObjectGeometryDefinition / draw/placement geometry definition or derived query | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 969 | property Terraria.ObjectData.TileObjectData.DrawFrameOffsets : Rectangle[,] | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:1460 | proposed.TileObjectGeometryDefinition / draw/placement geometry definition or derived query | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 970 | property Terraria.ObjectData.TileObjectData.CoordinateWidth : int | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:1503 | proposed.TileObjectGeometryDefinition / draw/placement geometry definition or derived query | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 971 | property Terraria.ObjectData.TileObjectData.CoordinatePadding : int | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:1537 | proposed.TileObjectGeometryDefinition / draw/placement geometry definition or derived query | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 972 | property Terraria.ObjectData.TileObjectData.CoordinatePaddingFix : Terraria.DataStructures.Point16 | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:1571 | proposed.TileObjectGeometryDefinition / draw/placement geometry definition or derived query | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 973 | property Terraria.ObjectData.TileObjectData.CoordinateFullWidth : int | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:1605 | proposed.TileObjectGeometryDefinition / draw/placement geometry definition or derived query | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 974 | property Terraria.ObjectData.TileObjectData.CoordinateFullHeight : int | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:1621 | proposed.TileObjectGeometryDefinition / draw/placement geometry definition or derived query | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 975 | property Terraria.ObjectData.TileObjectData.DrawStyleOffset : int | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:1637 | proposed.TileObjectGeometryDefinition / draw/placement geometry definition or derived query | registration/read-only boundary confirmed; runtime consumer matrix partial |

### TileObjectAnchorAndLiquidPlacementState -> `proposed.TileObjectPlacementDefinition`

成员数：25。所有成员均逐条映射；Version4 证据根路径：`D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs`。

| Seq | Source member | Version4 evidence | proposed target role | Evidence/ownership state |
|---:|---|---|---|---|
| 871 | field Terraria.ObjectData.TileObjectData._usesCustomCanPlace : bool | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:18 | proposed.TileObjectPlacementDefinition / anchor/liquid module ownership flag | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 872 | field Terraria.ObjectData.TileObjectData._useGlobalLiquidChecks : bool | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:20 | proposed.TileObjectPlacementDefinition / anchor/liquid module ownership flag | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 874 | field Terraria.ObjectData.TileObjectData._anchor : Terraria.Modules.AnchorDataModule | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:24 | proposed.TileObjectPlacementDefinition / anchor/liquid module ownership flag | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 875 | field Terraria.ObjectData.TileObjectData._anchorTiles : Terraria.Modules.AnchorTypesModule | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:26 | proposed.TileObjectPlacementDefinition / anchor/liquid module ownership flag | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 876 | field Terraria.ObjectData.TileObjectData._liquidDeath : Terraria.Modules.LiquidDeathModule | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:28 | proposed.TileObjectPlacementDefinition / anchor/liquid module ownership flag | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 877 | field Terraria.ObjectData.TileObjectData._liquidPlacement : Terraria.Modules.LiquidPlacementModule | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:30 | proposed.TileObjectPlacementDefinition / anchor/liquid module ownership flag | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 885 | field Terraria.ObjectData.TileObjectData._hasOwnAnchor : bool | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:46 | proposed.TileObjectPlacementDefinition / anchor/liquid module ownership flag | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 886 | field Terraria.ObjectData.TileObjectData._hasOwnAnchorTiles : bool | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:48 | proposed.TileObjectPlacementDefinition / anchor/liquid module ownership flag | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 887 | field Terraria.ObjectData.TileObjectData._hasOwnLiquidDeath : bool | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:50 | proposed.TileObjectPlacementDefinition / anchor/liquid module ownership flag | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 888 | field Terraria.ObjectData.TileObjectData._hasOwnLiquidPlacement : bool | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:52 | proposed.TileObjectPlacementDefinition / anchor/liquid module ownership flag | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 927 | property Terraria.ObjectData.TileObjectData.UsesCustomCanPlace : bool | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:144 | proposed.TileObjectPlacementDefinition / anchor validity rule definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 928 | property Terraria.ObjectData.TileObjectData.UsesGlobalLiquidChecks : bool | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:157 | proposed.TileObjectPlacementDefinition / liquid placement rule definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 930 | property Terraria.ObjectData.TileObjectData.AnchorTop : Terraria.DataStructures.AnchorData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:191 | proposed.TileObjectPlacementDefinition / anchor validity rule definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 931 | property Terraria.ObjectData.TileObjectData.AnchorBottom : Terraria.DataStructures.AnchorData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:224 | proposed.TileObjectPlacementDefinition / anchor validity rule definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 932 | property Terraria.ObjectData.TileObjectData.AnchorLeft : Terraria.DataStructures.AnchorData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:257 | proposed.TileObjectPlacementDefinition / anchor validity rule definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 933 | property Terraria.ObjectData.TileObjectData.AnchorRight : Terraria.DataStructures.AnchorData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:290 | proposed.TileObjectPlacementDefinition / anchor validity rule definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 934 | property Terraria.ObjectData.TileObjectData.AnchorWall : bool | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:323 | proposed.TileObjectPlacementDefinition / anchor validity rule definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 935 | property Terraria.ObjectData.TileObjectData.AnchorValidTiles : int[] | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:356 | proposed.TileObjectPlacementDefinition / anchor validity rule definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 936 | property Terraria.ObjectData.TileObjectData.AnchorInvalidTiles : int[] | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:395 | proposed.TileObjectPlacementDefinition / anchor validity rule definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 937 | property Terraria.ObjectData.TileObjectData.AnchorAlternateTiles : int[] | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:434 | proposed.TileObjectPlacementDefinition / anchor validity rule definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 938 | property Terraria.ObjectData.TileObjectData.AnchorValidWalls : int[] | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:473 | proposed.TileObjectPlacementDefinition / anchor validity rule definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 939 | property Terraria.ObjectData.TileObjectData.WaterDeath : bool | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:508 | proposed.TileObjectPlacementDefinition / liquid placement rule definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 940 | property Terraria.ObjectData.TileObjectData.LavaDeath : bool | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:541 | proposed.TileObjectPlacementDefinition / liquid placement rule definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 941 | property Terraria.ObjectData.TileObjectData.WaterPlacement : Terraria.Enums.LiquidPlacement | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:574 | proposed.TileObjectPlacementDefinition / liquid placement rule definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 942 | property Terraria.ObjectData.TileObjectData.LavaPlacement : Terraria.Enums.LiquidPlacement | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:607 | proposed.TileObjectPlacementDefinition / liquid placement rule definition | registration/read-only boundary confirmed; runtime consumer matrix partial |

### TileObjectPlacementHookAndBaseState -> `proposed.TileObjectPlacementHookDefinition`

成员数：13。所有成员均逐条映射；Version4 证据根路径：`D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs`。

| Seq | Source member | Version4 evidence | proposed target role | Evidence/ownership state |
|---:|---|---|---|---|
| 878 | field Terraria.ObjectData.TileObjectData._placementHooks : Terraria.Modules.TilePlacementHooksModule | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:32 | proposed.TileObjectPlacementHookDefinition / placement module ownership flag | confirmed locally; external effect owner partial; crossSubsystemOwner: integration-review |
| 879 | field Terraria.ObjectData.TileObjectData._subTiles : Terraria.Modules.TileObjectSubTilesModule | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:34 | proposed.TileObjectPlacementHookDefinition / sub-tile definition catalog | confirmed locally; external effect owner partial; crossSubsystemOwner: integration-review |
| 882 | field Terraria.ObjectData.TileObjectData._tileObjectBase : Terraria.Modules.TileObjectBaseModule | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:40 | proposed.TileObjectPlacementHookDefinition / tile-object base definition | confirmed locally; external effect owner partial; crossSubsystemOwner: integration-review |
| 883 | field Terraria.ObjectData.TileObjectData._tileObjectCoords : Terraria.Modules.TileObjectCoordinatesModule | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:42 | proposed.TileObjectPlacementHookDefinition / tile-object coordinate module | confirmed locally; external effect owner partial; crossSubsystemOwner: integration-review |
| 889 | field Terraria.ObjectData.TileObjectData._hasOwnPlacementHooks : bool | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:54 | proposed.TileObjectPlacementHookDefinition / placement module ownership flag | confirmed locally; external effect owner partial; crossSubsystemOwner: integration-review |
| 890 | field Terraria.ObjectData.TileObjectData._hasOwnSubTiles : bool | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:56 | proposed.TileObjectPlacementHookDefinition / placement module ownership flag | confirmed locally; external effect owner partial; crossSubsystemOwner: integration-review |
| 891 | field Terraria.ObjectData.TileObjectData._hasOwnTileObjectBase : bool | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:58 | proposed.TileObjectPlacementHookDefinition / placement module ownership flag | confirmed locally; external effect owner partial; crossSubsystemOwner: integration-review |
| 894 | field Terraria.ObjectData.TileObjectData._hasOwnTileObjectCoords : bool | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:64 | proposed.TileObjectPlacementHookDefinition / placement module ownership flag | confirmed locally; external effect owner partial; crossSubsystemOwner: integration-review |
| 943 | property Terraria.ObjectData.TileObjectData.HookCheckIfCanPlace : Terraria.DataStructures.PlacementHook | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:640 | proposed.TileObjectPlacementHookDefinition / placement hook adapter definition | confirmed locally; external effect owner partial; crossSubsystemOwner: integration-review |
| 944 | property Terraria.ObjectData.TileObjectData.HookPostPlaceEveryone : Terraria.DataStructures.PlacementHook | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:662 | proposed.TileObjectPlacementHookDefinition / placement hook adapter definition | confirmed locally; external effect owner partial; crossSubsystemOwner: integration-review |
| 945 | property Terraria.ObjectData.TileObjectData.HookPostPlaceMyPlayer : Terraria.DataStructures.PlacementHook | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:684 | proposed.TileObjectPlacementHookDefinition / placement hook adapter definition | confirmed locally; external effect owner partial; crossSubsystemOwner: integration-review |
| 946 | property Terraria.ObjectData.TileObjectData.HookPlaceOverride : Terraria.DataStructures.PlacementHook | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:706 | proposed.TileObjectPlacementHookDefinition / placement hook adapter definition | confirmed locally; external effect owner partial; crossSubsystemOwner: integration-review |
| 948 | property Terraria.ObjectData.TileObjectData.SubTiles : System.Collections.Generic.List<Terraria.ObjectData.TileObjectData> | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:750 | proposed.TileObjectPlacementHookDefinition / sub-tile definition catalog | confirmed locally; external effect owner partial; crossSubsystemOwner: integration-review |

### TileObjectStyleDefinitionCatalogState -> `proposed.TileObjectStyleDefinitionAndSelection`

成员数：28。所有成员均逐条映射；Version4 证据根路径：`D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs`。

| Seq | Source member | Version4 evidence | proposed target role | Evidence/ownership state |
|---:|---|---|---|---|
| 901 | field Terraria.ObjectData.TileObjectData.StyleSwitch : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:78 | proposed.TileObjectStyleDefinitionAndSelection / fixed style preset/geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 902 | field Terraria.ObjectData.TileObjectData.StyleTorch : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:80 | proposed.TileObjectStyleDefinitionAndSelection / fixed style preset/geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 903 | field Terraria.ObjectData.TileObjectData.Style4x2 : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:82 | proposed.TileObjectStyleDefinitionAndSelection / fixed style preset/geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 904 | field Terraria.ObjectData.TileObjectData.Style2x2 : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:84 | proposed.TileObjectStyleDefinitionAndSelection / fixed style preset/geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 905 | field Terraria.ObjectData.TileObjectData.Style1x2 : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:86 | proposed.TileObjectStyleDefinitionAndSelection / fixed style preset/geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 906 | field Terraria.ObjectData.TileObjectData.Style1x1 : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:88 | proposed.TileObjectStyleDefinitionAndSelection / fixed style preset/geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 907 | field Terraria.ObjectData.TileObjectData.StyleAlch : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:90 | proposed.TileObjectStyleDefinitionAndSelection / fixed style preset/geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 908 | field Terraria.ObjectData.TileObjectData.StyleDye : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:92 | proposed.TileObjectStyleDefinitionAndSelection / fixed style preset/geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 909 | field Terraria.ObjectData.TileObjectData.Style2x1 : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:94 | proposed.TileObjectStyleDefinitionAndSelection / fixed style preset/geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 910 | field Terraria.ObjectData.TileObjectData.Style6x3 : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:96 | proposed.TileObjectStyleDefinitionAndSelection / fixed style preset/geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 911 | field Terraria.ObjectData.TileObjectData.StyleSmallCage : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:98 | proposed.TileObjectStyleDefinitionAndSelection / fixed style preset/geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 912 | field Terraria.ObjectData.TileObjectData.StyleOnTable1x1 : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:100 | proposed.TileObjectStyleDefinitionAndSelection / fixed style preset/geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 913 | field Terraria.ObjectData.TileObjectData.Style1x2Top : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:102 | proposed.TileObjectStyleDefinitionAndSelection / fixed style preset/geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 914 | field Terraria.ObjectData.TileObjectData.Style1xX : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:104 | proposed.TileObjectStyleDefinitionAndSelection / fixed style preset/geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 915 | field Terraria.ObjectData.TileObjectData.Style2xX : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:106 | proposed.TileObjectStyleDefinitionAndSelection / fixed style preset/geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 916 | field Terraria.ObjectData.TileObjectData.Style3x2 : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:108 | proposed.TileObjectStyleDefinitionAndSelection / fixed style preset/geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 917 | field Terraria.ObjectData.TileObjectData.Style3x3 : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:110 | proposed.TileObjectStyleDefinitionAndSelection / fixed style preset/geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 918 | field Terraria.ObjectData.TileObjectData.Style3x4 : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:112 | proposed.TileObjectStyleDefinitionAndSelection / fixed style preset/geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 919 | field Terraria.ObjectData.TileObjectData.Style4x4 : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:114 | proposed.TileObjectStyleDefinitionAndSelection / fixed style preset/geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 920 | field Terraria.ObjectData.TileObjectData.Style5x4 : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:116 | proposed.TileObjectStyleDefinitionAndSelection / fixed style preset/geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 921 | field Terraria.ObjectData.TileObjectData.Style3x3Wall : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:118 | proposed.TileObjectStyleDefinitionAndSelection / fixed style preset/geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 922 | field Terraria.ObjectData.TileObjectData.Style1x1Drip : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:120 | proposed.TileObjectStyleDefinitionAndSelection / fixed style preset/geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 923 | field Terraria.ObjectData.TileObjectData.Style1x1Plant_Height22 : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:122 | proposed.TileObjectStyleDefinitionAndSelection / fixed style preset/geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 924 | field Terraria.ObjectData.TileObjectData.Style1x1Plant_Height34 : Terraria.ObjectData.TileObjectData | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:124 | proposed.TileObjectStyleDefinitionAndSelection / fixed style preset/geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 954 | property Terraria.ObjectData.TileObjectData.StyleHorizontal : bool | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:943 | proposed.TileObjectStyleDefinitionAndSelection / style geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 955 | property Terraria.ObjectData.TileObjectData.Style : int | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:976 | proposed.TileObjectStyleDefinitionAndSelection / fixed style preset/geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 956 | property Terraria.ObjectData.TileObjectData.StyleWrapLimit : int | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:1009 | proposed.TileObjectStyleDefinitionAndSelection / fixed style preset/geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 960 | property Terraria.ObjectData.TileObjectData.StyleMultiplier : int | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:1141 | proposed.TileObjectStyleDefinitionAndSelection / fixed style preset/geometry definition | registration/read-only boundary confirmed; runtime consumer matrix partial |

### TileObjectStyleSelectionAndOverrideState -> `proposed.TileObjectStyleDefinitionAndSelection`

成员数：8。所有成员均逐条映射；Version4 证据根路径：`D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs`。

| Seq | Source member | Version4 evidence | proposed target role | Evidence/ownership state |
|---:|---|---|---|---|
| 881 | field Terraria.ObjectData.TileObjectData._tileObjectStyle : Terraria.Modules.TileObjectStyleModule | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:38 | proposed.TileObjectStyleDefinitionAndSelection / style selection/visual override state | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 893 | field Terraria.ObjectData.TileObjectData._hasOwnTileObjectStyle : bool | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:62 | proposed.TileObjectStyleDefinitionAndSelection / style selection/visual override state | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 947 | property Terraria.ObjectData.TileObjectData.GetStyleOverride : Terraria.DataStructures.GetStyleMethod | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:728 | proposed.TileObjectStyleDefinitionAndSelection / style override adapter/query | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 957 | property Terraria.ObjectData.TileObjectData.StyleWrapLimitVisualOverride : int? | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:1042 | proposed.TileObjectStyleDefinitionAndSelection / style selection/visual override state | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 958 | property Terraria.ObjectData.TileObjectData.styleLineSkipVisualOverride : int? | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:1075 | proposed.TileObjectStyleDefinitionAndSelection / style selection/visual override state | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 959 | property Terraria.ObjectData.TileObjectData.StyleLineSkip : int | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:1108 | proposed.TileObjectStyleDefinitionAndSelection / style selection/visual override state | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 965 | property Terraria.ObjectData.TileObjectData.RandomStyleRange : int | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:1318 | proposed.TileObjectStyleDefinitionAndSelection / random style selection input | registration/read-only boundary confirmed; runtime consumer matrix partial |
| 966 | property Terraria.ObjectData.TileObjectData.SpecificRandomStyles : int[] | D:\TRbackup\Version4\Terraria.ObjectData\TileObjectData.cs:1351 | proposed.TileObjectStyleDefinitionAndSelection / random style selection input | registration/read-only boundary confirmed; runtime consumer matrix partial |

### SharedContentValidation -> `proposed.ContentValidationBoundary`

成员数：12。所有成员均逐条映射；Version4 证据根路径：`D:\TRbackup\Version4\Terraria.GameContent\ChildSafety.cs`。

| Seq | Source member | Version4 evidence | proposed target role | Evidence/ownership state |
|---:|---|---|---|---|
| 2345 | field Terraria.GameContent.ChildSafety.factoryDust : Terraria.ID.SetFactory | D:\TRbackup\Version4\Terraria.GameContent\ChildSafety.cs:7 | proposed.ContentValidationBoundary / safety set factory adapter state | declaration confirmed; asset/config lifecycle partial |
| 2346 | field Terraria.GameContent.ChildSafety.factoryGore : Terraria.ID.SetFactory | D:\TRbackup\Version4\Terraria.GameContent\ChildSafety.cs:9 | proposed.ContentValidationBoundary / safety set factory adapter state | declaration confirmed; asset/config lifecycle partial |
| 2347 | field Terraria.GameContent.ChildSafety.SafeGore : bool[] | D:\TRbackup\Version4\Terraria.GameContent\ChildSafety.cs:11 | proposed.ContentValidationBoundary / content safety set definition | declaration confirmed; asset/config lifecycle partial |
| 2348 | field Terraria.GameContent.ChildSafety.SafeDust : bool[] | D:\TRbackup\Version4\Terraria.GameContent\ChildSafety.cs:13 | proposed.ContentValidationBoundary / content safety set definition | declaration confirmed; asset/config lifecycle partial |
| 2349 | field Terraria.GameContent.ChildSafety.Disabled : bool | D:\TRbackup\Version4\Terraria.GameContent\ChildSafety.cs:15 | proposed.ContentValidationBoundary / validation policy definition | declaration confirmed; asset/config lifecycle partial |
| 2359 | field Terraria.GameContent.ContentRejectionFromSize._neededWidth : int | D:\TRbackup\Version4\Terraria.GameContent\ContentRejectionFromSize.cs:8 | proposed.ContentValidationBoundary / typed rejection value object field | declaration confirmed; asset/config lifecycle partial |
| 2360 | field Terraria.GameContent.ContentRejectionFromSize._neededHeight : int | D:\TRbackup\Version4\Terraria.GameContent\ContentRejectionFromSize.cs:10 | proposed.ContentValidationBoundary / typed rejection value object field | declaration confirmed; asset/config lifecycle partial |
| 2361 | field Terraria.GameContent.ContentRejectionFromSize._actualWidth : int | D:\TRbackup\Version4\Terraria.GameContent\ContentRejectionFromSize.cs:12 | proposed.ContentValidationBoundary / typed rejection value object field | declaration confirmed; asset/config lifecycle partial |
| 2362 | field Terraria.GameContent.ContentRejectionFromSize._actualHeight : int | D:\TRbackup\Version4\Terraria.GameContent\ContentRejectionFromSize.cs:14 | proposed.ContentValidationBoundary / typed rejection value object field | declaration confirmed; asset/config lifecycle partial |
| 2594 | field Terraria.GameContent.VanillaContentValidator.TextureMetaData.Width : int | D:\TRbackup\Version4\Terraria.GameContent\VanillaContentValidator.cs:13 | proposed.ContentValidationBoundary / typed rejection value object field | declaration confirmed; asset/config lifecycle partial |
| 2595 | field Terraria.GameContent.VanillaContentValidator.TextureMetaData.Height : int | D:\TRbackup\Version4\Terraria.GameContent\VanillaContentValidator.cs:15 | proposed.ContentValidationBoundary / typed rejection value object field | declaration confirmed; asset/config lifecycle partial |
| 2596 | field Terraria.GameContent.VanillaContentValidator._info : System.Collections.Generic.Dictionary<string, Terraria.GameContent.VanillaContentValidator.TextureMetaData> | D:\TRbackup\Version4\Terraria.GameContent\VanillaContentValidator.cs:18 | proposed.ContentValidationBoundary / texture metadata validation index | declaration confirmed; asset/config lifecycle partial |

### SharedContentPresentationCatalog -> `proposed.ContentPresentationCatalog`

成员数：23。所有成员均逐条映射；Version4 证据根路径：`D:\TRbackup\Version4\Terraria.DataStructures\ColorSlidersSet.cs`。

| Seq | Source member | Version4 evidence | proposed target role | Evidence/ownership state |
|---:|---|---|---|---|
| 1084 | field Terraria.DataStructures.ColorSlidersSet.Hue : float | D:\TRbackup\Version4\Terraria.DataStructures\ColorSlidersSet.cs:7 | proposed.ContentPresentationCatalog / color slider presentation value | declaration confirmed; asset/config lifecycle partial |
| 1085 | field Terraria.DataStructures.ColorSlidersSet.Saturation : float | D:\TRbackup\Version4\Terraria.DataStructures\ColorSlidersSet.cs:9 | proposed.ContentPresentationCatalog / color slider presentation value | declaration confirmed; asset/config lifecycle partial |
| 1086 | field Terraria.DataStructures.ColorSlidersSet.Luminance : float | D:\TRbackup\Version4\Terraria.DataStructures\ColorSlidersSet.cs:11 | proposed.ContentPresentationCatalog / color slider presentation value | declaration confirmed; asset/config lifecycle partial |
| 1087 | field Terraria.DataStructures.ColorSlidersSet.Alpha : float | D:\TRbackup\Version4\Terraria.DataStructures\ColorSlidersSet.cs:13 | proposed.ContentPresentationCatalog / color slider presentation value | declaration confirmed; asset/config lifecycle partial |
| 2424 | field Terraria.GameContent.FontAssets.MouseText : Asset<DynamicSpriteFont> | D:\TRbackup\Version4\Terraria.GameContent\FontAssets.cs:9 | proposed.ContentPresentationCatalog / font asset adapter handle | declaration confirmed; asset/config lifecycle partial |
| 2425 | field Terraria.GameContent.HairstyleUnlocksHelper.AvailableHairstyles : System.Collections.Generic.List<int> | D:\TRbackup\Version4\Terraria.GameContent\HairstyleUnlocksHelper.cs:7 | proposed.ContentPresentationCatalog / hairstyle presentation catalog | declaration confirmed; asset/config lifecycle partial |
| 2472 | field Terraria.GameContent.Profiles.StackedNPCProfile._profiles : Terraria.GameContent.ITownNPCProfile[] | D:\TRbackup\Version4\Terraria.GameContent\Profiles.cs:12 | proposed.ContentPresentationCatalog / NPC profile presentation definition or asset adapter | declaration confirmed; asset/config lifecycle partial |
| 2473 | field Terraria.GameContent.Profiles.LegacyNPCProfile._rootFilePath : string | D:\TRbackup\Version4\Terraria.GameContent\Profiles.cs:34 | proposed.ContentPresentationCatalog / NPC profile presentation definition or asset adapter | declaration confirmed; asset/config lifecycle partial |
| 2474 | field Terraria.GameContent.Profiles.LegacyNPCProfile._defaultVariationHeadIndex : int | D:\TRbackup\Version4\Terraria.GameContent\Profiles.cs:36 | proposed.ContentPresentationCatalog / NPC profile presentation definition or asset adapter | declaration confirmed; asset/config lifecycle partial |
| 2475 | field Terraria.GameContent.Profiles.LegacyNPCProfile._defaultNoAlt : Asset<Texture2D> | D:\TRbackup\Version4\Terraria.GameContent\Profiles.cs:38 | proposed.ContentPresentationCatalog / NPC profile presentation definition or asset adapter | declaration confirmed; asset/config lifecycle partial |
| 2476 | field Terraria.GameContent.Profiles.LegacyNPCProfile._defaultParty : Asset<Texture2D> | D:\TRbackup\Version4\Terraria.GameContent\Profiles.cs:40 | proposed.ContentPresentationCatalog / NPC profile presentation definition or asset adapter | declaration confirmed; asset/config lifecycle partial |
| 2477 | field Terraria.GameContent.Profiles.TransformableNPCProfile._rootFilePath : string | D:\TRbackup\Version4\Terraria.GameContent\Profiles.cs:64 | proposed.ContentPresentationCatalog / NPC profile presentation definition or asset adapter | declaration confirmed; asset/config lifecycle partial |
| 2478 | field Terraria.GameContent.Profiles.TransformableNPCProfile._defaultVariationHeadIndex : int | D:\TRbackup\Version4\Terraria.GameContent\Profiles.cs:66 | proposed.ContentPresentationCatalog / NPC profile presentation definition or asset adapter | declaration confirmed; asset/config lifecycle partial |
| 2479 | field Terraria.GameContent.Profiles.TransformableNPCProfile._defaultNoAlt : Asset<Texture2D> | D:\TRbackup\Version4\Terraria.GameContent\Profiles.cs:68 | proposed.ContentPresentationCatalog / NPC profile presentation definition or asset adapter | declaration confirmed; asset/config lifecycle partial |
| 2480 | field Terraria.GameContent.Profiles.TransformableNPCProfile._defaultTransformed : Asset<Texture2D> | D:\TRbackup\Version4\Terraria.GameContent\Profiles.cs:70 | proposed.ContentPresentationCatalog / NPC profile presentation definition or asset adapter | declaration confirmed; asset/config lifecycle partial |
| 2481 | field Terraria.GameContent.Profiles.TransformableNPCProfile._defaultCredits : Asset<Texture2D> | D:\TRbackup\Version4\Terraria.GameContent\Profiles.cs:72 | proposed.ContentPresentationCatalog / NPC profile presentation definition or asset adapter | declaration confirmed; asset/config lifecycle partial |
| 2482 | field Terraria.GameContent.Profiles.VariantNPCProfile._rootFilePath : string | D:\TRbackup\Version4\Terraria.GameContent\Profiles.cs:96 | proposed.ContentPresentationCatalog / NPC profile presentation definition or asset adapter | declaration confirmed; asset/config lifecycle partial |
| 2483 | field Terraria.GameContent.Profiles.VariantNPCProfile._npcBaseName : string | D:\TRbackup\Version4\Terraria.GameContent\Profiles.cs:98 | proposed.ContentPresentationCatalog / NPC profile presentation definition or asset adapter | declaration confirmed; asset/config lifecycle partial |
| 2484 | field Terraria.GameContent.Profiles.VariantNPCProfile._variantHeadIDs : int[] | D:\TRbackup\Version4\Terraria.GameContent\Profiles.cs:100 | proposed.ContentPresentationCatalog / NPC profile presentation definition or asset adapter | declaration confirmed; asset/config lifecycle partial |
| 2485 | field Terraria.GameContent.Profiles.VariantNPCProfile._variants : string[] | D:\TRbackup\Version4\Terraria.GameContent\Profiles.cs:102 | proposed.ContentPresentationCatalog / NPC profile presentation definition or asset adapter | declaration confirmed; asset/config lifecycle partial |
| 2486 | field Terraria.GameContent.Profiles.VariantNPCProfile._variantTextures : System.Collections.Generic.Dictionary<string, Asset<Texture2D>> | D:\TRbackup\Version4\Terraria.GameContent\Profiles.cs:104 | proposed.ContentPresentationCatalog / NPC profile presentation definition or asset adapter | declaration confirmed; asset/config lifecycle partial |
| 3689 | property Terraria.DataStructures.IConfigKeyHolder.NameKey : string | D:\TRbackup\Version4\Terraria.DataStructures\IConfigKeyHolder.cs:5 | proposed.ContentPresentationCatalog / stable presentation/config key | declaration confirmed; asset/config lifecycle partial |
| 3690 | property Terraria.DataStructures.IConfigKeyHolder.ConfigKey : string | D:\TRbackup\Version4\Terraria.DataStructures\IConfigKeyHolder.cs:7 | proposed.ContentPresentationCatalog / stable presentation/config key | declaration confirmed; asset/config lifecycle partial |

### ItemVariantDefinitions -> `proposed.ItemVariantCatalog`

成员数：15。所有成员均逐条映射；Version4 证据根路径：`D:\TRbackup\Version4\Terraria.GameContent.Items\ItemVariant.cs`。

| Seq | Source member | Version4 evidence | proposed target role | Evidence/ownership state |
|---:|---|---|---|---|
| 2103 | field Terraria.GameContent.Items.ItemVariant.Description : Terraria.Localization.NetworkText | D:\TRbackup\Version4\Terraria.GameContent.Items\ItemVariant.cs:7 | proposed.ItemVariantCatalog / item variant definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2104 | field Terraria.GameContent.Items.ItemVariantCondition.Description : Terraria.Localization.NetworkText | D:\TRbackup\Version4\Terraria.GameContent.Items\ItemVariantCondition.cs:9 | proposed.ItemVariantCatalog / item variant definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2105 | field Terraria.GameContent.Items.ItemVariantCondition.IsMet : Terraria.GameContent.Items.ItemVariantCondition.Condition | D:\TRbackup\Version4\Terraria.GameContent.Items\ItemVariantCondition.cs:11 | proposed.ItemVariantCatalog / explicit variant condition adapter/predicate | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2106 | field Terraria.GameContent.Items.ItemVariants.VariantEntry.Variant : Terraria.GameContent.Items.ItemVariant | D:\TRbackup\Version4\Terraria.GameContent.Items\ItemVariants.cs:12 | proposed.ItemVariantCatalog / item variant definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2107 | field Terraria.GameContent.Items.ItemVariants.VariantEntry._conditions : System.Collections.Generic.List<Terraria.GameContent.Items.ItemVariantCondition> | D:\TRbackup\Version4\Terraria.GameContent.Items\ItemVariants.cs:14 | proposed.ItemVariantCatalog / variant condition definition list | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2108 | field Terraria.GameContent.Items.ItemVariants._variants : System.Collections.Generic.List<Terraria.GameContent.Items.ItemVariants.VariantEntry>[] | D:\TRbackup\Version4\Terraria.GameContent.Items\ItemVariants.cs:38 | proposed.ItemVariantCatalog / item-to-variant index | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2109 | field Terraria.GameContent.Items.ItemVariants.StrongerVariant : Terraria.GameContent.Items.ItemVariant | D:\TRbackup\Version4\Terraria.GameContent.Items\ItemVariants.cs:40 | proposed.ItemVariantCatalog / item variant definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2110 | field Terraria.GameContent.Items.ItemVariants.WeakerVariant : Terraria.GameContent.Items.ItemVariant | D:\TRbackup\Version4\Terraria.GameContent.Items\ItemVariants.cs:42 | proposed.ItemVariantCatalog / item variant definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2111 | field Terraria.GameContent.Items.ItemVariants.RebalancedVariant : Terraria.GameContent.Items.ItemVariant | D:\TRbackup\Version4\Terraria.GameContent.Items\ItemVariants.cs:44 | proposed.ItemVariantCatalog / item variant definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2112 | field Terraria.GameContent.Items.ItemVariants.EnabledVariant : Terraria.GameContent.Items.ItemVariant | D:\TRbackup\Version4\Terraria.GameContent.Items\ItemVariants.cs:46 | proposed.ItemVariantCatalog / item variant definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2113 | field Terraria.GameContent.Items.ItemVariants.DisabledBossSummonVariant : Terraria.GameContent.Items.ItemVariant | D:\TRbackup\Version4\Terraria.GameContent.Items\ItemVariants.cs:48 | proposed.ItemVariantCatalog / item variant definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2114 | field Terraria.GameContent.Items.ItemVariants.RemixWorld : Terraria.GameContent.Items.ItemVariantCondition | D:\TRbackup\Version4\Terraria.GameContent.Items\ItemVariants.cs:50 | proposed.ItemVariantCatalog / world-rule condition definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2115 | field Terraria.GameContent.Items.ItemVariants.GetGoodWorld : Terraria.GameContent.Items.ItemVariantCondition | D:\TRbackup\Version4\Terraria.GameContent.Items\ItemVariants.cs:52 | proposed.ItemVariantCatalog / world-rule condition definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2116 | field Terraria.GameContent.Items.ItemVariants.MechdusaWorld : Terraria.GameContent.Items.ItemVariantCondition | D:\TRbackup\Version4\Terraria.GameContent.Items\ItemVariants.cs:54 | proposed.ItemVariantCatalog / world-rule condition definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 3821 | property Terraria.GameContent.Items.ItemVariants.VariantEntry.Conditions : System.Collections.Generic.IEnumerable<Terraria.GameContent.Items.ItemVariantCondition> | D:\TRbackup\Version4\Terraria.GameContent.Items\ItemVariants.cs:16 | proposed.ItemVariantCatalog / read-only condition view | declaration confirmed; member-level reader/writer/lifecycle closure partial |

### LegacyItemPrefixCatalog -> `proposed.LegacyItemPrefixCatalog`

成员数：16。所有成员均逐条映射；Version4 证据根路径：`D:\TRbackup\Version4\Terraria.GameContent.Prefixes\PrefixLegacy.cs`。

| Seq | Source member | Version4 evidence | proposed target role | Evidence/ownership state |
|---:|---|---|---|---|
| 2172 | field Terraria.GameContent.Prefixes.PrefixLegacy.Prefixes.PrefixesForSwords : int[] | D:\TRbackup\Version4\Terraria.GameContent.Prefixes\PrefixLegacy.cs:9 | proposed.LegacyItemPrefixCatalog / prefix category order definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2173 | field Terraria.GameContent.Prefixes.PrefixLegacy.Prefixes.PrefixesForSpears : int[] | D:\TRbackup\Version4\Terraria.GameContent.Prefixes\PrefixLegacy.cs:17 | proposed.LegacyItemPrefixCatalog / prefix category order definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2174 | field Terraria.GameContent.Prefixes.PrefixLegacy.Prefixes.PrefixesForGunsBows : int[] | D:\TRbackup\Version4\Terraria.GameContent.Prefixes\PrefixLegacy.cs:23 | proposed.LegacyItemPrefixCatalog / prefix category order definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2175 | field Terraria.GameContent.Prefixes.PrefixLegacy.Prefixes.PrefixesForMagic : int[] | D:\TRbackup\Version4\Terraria.GameContent.Prefixes\PrefixLegacy.cs:31 | proposed.LegacyItemPrefixCatalog / prefix category order definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2176 | field Terraria.GameContent.Prefixes.PrefixLegacy.Prefixes.PrefixesForSummons : int[] | D:\TRbackup\Version4\Terraria.GameContent.Prefixes\PrefixLegacy.cs:39 | proposed.LegacyItemPrefixCatalog / prefix category order definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2177 | field Terraria.GameContent.Prefixes.PrefixLegacy.Prefixes.PrefixesForBoomeransAndChakrums : int[] | D:\TRbackup\Version4\Terraria.GameContent.Prefixes\PrefixLegacy.cs:46 | proposed.LegacyItemPrefixCatalog / prefix category order definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2178 | field Terraria.GameContent.Prefixes.PrefixLegacy.Prefixes.PrefixesForBoomeransAndChakrums_TerrarianYoyo : int[] | D:\TRbackup\Version4\Terraria.GameContent.Prefixes\PrefixLegacy.cs:52 | proposed.LegacyItemPrefixCatalog / prefix category order definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2179 | field Terraria.GameContent.Prefixes.PrefixLegacy.Prefixes.PrefixesForAccessories : int[] | D:\TRbackup\Version4\Terraria.GameContent.Prefixes\PrefixLegacy.cs:58 | proposed.LegacyItemPrefixCatalog / prefix category order definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2180 | field Terraria.GameContent.Prefixes.PrefixLegacy.ItemSets.Factory : Terraria.ID.SetFactory | D:\TRbackup\Version4\Terraria.GameContent.Prefixes\PrefixLegacy.cs:67 | proposed.LegacyItemPrefixCatalog / prefix set factory adapter state | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2181 | field Terraria.GameContent.Prefixes.PrefixLegacy.ItemSets.BoomerangsChakrams : bool[] | D:\TRbackup\Version4\Terraria.GameContent.Prefixes\PrefixLegacy.cs:69 | proposed.LegacyItemPrefixCatalog / item prefix capability mask | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2182 | field Terraria.GameContent.Prefixes.PrefixLegacy.ItemSets.ItemsThatCanHaveLegendary2 : bool[] | D:\TRbackup\Version4\Terraria.GameContent.Prefixes\PrefixLegacy.cs:71 | proposed.LegacyItemPrefixCatalog / item prefix capability mask | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2183 | field Terraria.GameContent.Prefixes.PrefixLegacy.ItemSets.Magic : bool[] | D:\TRbackup\Version4\Terraria.GameContent.Prefixes\PrefixLegacy.cs:73 | proposed.LegacyItemPrefixCatalog / item prefix capability mask | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2184 | field Terraria.GameContent.Prefixes.PrefixLegacy.ItemSets.Summon : bool[] | D:\TRbackup\Version4\Terraria.GameContent.Prefixes\PrefixLegacy.cs:75 | proposed.LegacyItemPrefixCatalog / item prefix capability mask | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2185 | field Terraria.GameContent.Prefixes.PrefixLegacy.ItemSets.GunsBows : bool[] | D:\TRbackup\Version4\Terraria.GameContent.Prefixes\PrefixLegacy.cs:77 | proposed.LegacyItemPrefixCatalog / item prefix capability mask | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2186 | field Terraria.GameContent.Prefixes.PrefixLegacy.ItemSets.SpearsMacesChainsawsDrillsPunchCannon : bool[] | D:\TRbackup\Version4\Terraria.GameContent.Prefixes\PrefixLegacy.cs:79 | proposed.LegacyItemPrefixCatalog / item prefix capability mask | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 2187 | field Terraria.GameContent.Prefixes.PrefixLegacy.ItemSets.SwordsHammersAxesPicks : bool[] | D:\TRbackup\Version4\Terraria.GameContent.Prefixes\PrefixLegacy.cs:81 | proposed.LegacyItemPrefixCatalog / item prefix capability mask | declaration confirmed; member-level reader/writer/lifecycle closure partial |

### ArmorSetBonusDefinitions -> `proposed.ArmorSetBonusDefinitionCatalog`

成员数：22。所有成员均逐条映射；Version4 证据根路径：`D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonus.cs`。

| Seq | Source member | Version4 evidence | proposed target role | Evidence/ownership state |
|---:|---|---|---|---|
| 1041 | field Terraria.DataStructures.ArmorSetBonus.QueryContext.HeadItem : int | D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonus.cs:20 | proposed.ArmorSetBonusDefinitionCatalog / armor qualification query input | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 1042 | field Terraria.DataStructures.ArmorSetBonus.QueryContext.BodyItem : int | D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonus.cs:22 | proposed.ArmorSetBonusDefinitionCatalog / armor qualification query input | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 1043 | field Terraria.DataStructures.ArmorSetBonus.QueryContext.LegItem : int | D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonus.cs:24 | proposed.ArmorSetBonusDefinitionCatalog / armor qualification query input | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 1044 | field Terraria.DataStructures.ArmorSetBonus.QueryResult.ItemsNeeded : int | D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonus.cs:44 | proposed.ArmorSetBonusDefinitionCatalog / armor qualification query result | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 1045 | field Terraria.DataStructures.ArmorSetBonus.QueryResult.ItemsFound : int | D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonus.cs:46 | proposed.ArmorSetBonusDefinitionCatalog / armor qualification query result | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 1046 | field Terraria.DataStructures.ArmorSetBonus.Builder.Parts.Head : int | D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonus.cs:59 | proposed.ArmorSetBonusDefinitionCatalog / armor definition builder staging | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 1047 | field Terraria.DataStructures.ArmorSetBonus.Builder.Parts.Body : int | D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonus.cs:61 | proposed.ArmorSetBonusDefinitionCatalog / armor definition builder staging | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 1048 | field Terraria.DataStructures.ArmorSetBonus.Builder.Parts.Legs : int | D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonus.cs:63 | proposed.ArmorSetBonusDefinitionCatalog / armor definition builder staging | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 1049 | field Terraria.DataStructures.ArmorSetBonus.Builder.Effect : Terraria.DataStructures.ArmorSetBonus.ArmorSetEffect | D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonus.cs:66 | proposed.ArmorSetBonusDefinitionCatalog / armor definition builder staging | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 1050 | field Terraria.DataStructures.ArmorSetBonus.Builder.TextKey : string | D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonus.cs:68 | proposed.ArmorSetBonusDefinitionCatalog / armor definition builder staging | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 1051 | field Terraria.DataStructures.ArmorSetBonus.Builder.PrimaryPart : Terraria.DataStructures.ArmorSetBonus.PartType | D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonus.cs:70 | proposed.ArmorSetBonusDefinitionCatalog / armor definition builder staging | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 1052 | field Terraria.DataStructures.ArmorSetBonus.Builder._sets : System.Collections.Generic.List<Terraria.DataStructures.ArmorSetBonus.Builder.Parts> | D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonus.cs:72 | proposed.ArmorSetBonusDefinitionCatalog / armor definition builder staging | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 1053 | field Terraria.DataStructures.ArmorSetBonus.Effect : Terraria.DataStructures.ArmorSetBonus.ArmorSetEffect | D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonus.cs:145 | proposed.ArmorSetBonusDefinitionCatalog / armor effect command reference | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 1054 | field Terraria.DataStructures.ArmorSetBonus.Description : Terraria.Localization.LocalizedText | D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonus.cs:147 | proposed.ArmorSetBonusDefinitionCatalog / localized armor presentation projection | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 1055 | field Terraria.DataStructures.ArmorSetBonus.Head : int | D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonus.cs:149 | proposed.ArmorSetBonusDefinitionCatalog / armor set definition field | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 1056 | field Terraria.DataStructures.ArmorSetBonus.Body : int | D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonus.cs:151 | proposed.ArmorSetBonusDefinitionCatalog / armor set definition field | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 1057 | field Terraria.DataStructures.ArmorSetBonus.Legs : int | D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonus.cs:153 | proposed.ArmorSetBonusDefinitionCatalog / armor set definition field | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 1058 | field Terraria.DataStructures.ArmorSetBonus.PrimaryPart : Terraria.DataStructures.ArmorSetBonus.PartType | D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonus.cs:155 | proposed.ArmorSetBonusDefinitionCatalog / armor set definition field | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 1059 | field Terraria.DataStructures.ArmorSetBonus.ItemSetBonusEquipped : Terraria.Localization.LocalizedText | D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonus.cs:157 | proposed.ArmorSetBonusDefinitionCatalog / localized armor presentation projection | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 1060 | field Terraria.DataStructures.ArmorSetBonus.ItemSetBonusGeneral : Terraria.Localization.LocalizedText | D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonus.cs:159 | proposed.ArmorSetBonusDefinitionCatalog / localized armor presentation projection | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 1061 | field Terraria.DataStructures.ArmorSetBonus.ItemSetBonusDecidedBy : Terraria.Localization.LocalizedText[] | D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonus.cs:161 | proposed.ArmorSetBonusDefinitionCatalog / localized armor presentation projection | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 3685 | property Terraria.DataStructures.ArmorSetBonus.QueryResult.Complete : bool | D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonus.cs:48 | proposed.ArmorSetBonusDefinitionCatalog / armor qualification query result | declaration confirmed; member-level reader/writer/lifecycle closure partial |

### ArmorSetBonusCatalog -> `proposed.ArmorSetBonusLookupCatalog`

成员数：2。所有成员均逐条映射；Version4 证据根路径：`D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonuses.cs`。

| Seq | Source member | Version4 evidence | proposed target role | Evidence/ownership state |
|---:|---|---|---|---|
| 1062 | field Terraria.DataStructures.ArmorSetBonuses.All : System.Collections.Generic.List<Terraria.DataStructures.ArmorSetBonus> | D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonuses.cs:516 | proposed.ArmorSetBonusLookupCatalog / armor definition collection | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 1063 | field Terraria.DataStructures.ArmorSetBonuses.SetsContaining : Terraria.DataStructures.ArmorSetBonus[][] | D:\TRbackup\Version4\Terraria.DataStructures\ArmorSetBonuses.cs:518 | proposed.ArmorSetBonusLookupCatalog / derived item-to-set lookup | declaration confirmed; member-level reader/writer/lifecycle closure partial |

### WingStatsDefinition -> `proposed.WingStatsDefinition`

成员数：7。所有成员均逐条映射；Version4 证据根路径：`D:\TRbackup\Version4\Terraria.DataStructures\WingStats.cs`。

| Seq | Source member | Version4 evidence | proposed target role | Evidence/ownership state |
|---:|---|---|---|---|
| 1335 | field Terraria.DataStructures.WingStats.Default : Terraria.DataStructures.WingStats | D:\TRbackup\Version4\Terraria.DataStructures\WingStats.cs:5 | proposed.WingStatsDefinition / default wing value definition | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 1336 | field Terraria.DataStructures.WingStats.FlyTime : int | D:\TRbackup\Version4\Terraria.DataStructures\WingStats.cs:7 | proposed.WingStatsDefinition / wing movement value field | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 1337 | field Terraria.DataStructures.WingStats.AccRunSpeedOverride : float | D:\TRbackup\Version4\Terraria.DataStructures\WingStats.cs:9 | proposed.WingStatsDefinition / wing movement value field | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 1338 | field Terraria.DataStructures.WingStats.AccRunAccelerationMult : float | D:\TRbackup\Version4\Terraria.DataStructures\WingStats.cs:11 | proposed.WingStatsDefinition / wing movement value field | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 1339 | field Terraria.DataStructures.WingStats.HasDownHoverStats : bool | D:\TRbackup\Version4\Terraria.DataStructures\WingStats.cs:13 | proposed.WingStatsDefinition / wing movement value field | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 1340 | field Terraria.DataStructures.WingStats.DownHoverSpeedOverride : float | D:\TRbackup\Version4\Terraria.DataStructures\WingStats.cs:15 | proposed.WingStatsDefinition / wing movement value field | declaration confirmed; member-level reader/writer/lifecycle closure partial |
| 1341 | field Terraria.DataStructures.WingStats.DownHoverAccelerationMult : float | D:\TRbackup\Version4\Terraria.DataStructures\WingStats.cs:17 | proposed.WingStatsDefinition / wing movement value field | declaration confirmed; member-level reader/writer/lifecycle closure partial |

### SharedItemStaticCapabilityRules -> `proposed.ItemStaticCapabilityCatalog`

成员数：7。所有成员均逐条映射；Version4 证据根路径：`D:\TRbackup\Version4\Terraria\Item.cs`。

| Seq | Source member | Version4 evidence | proposed target role | Evidence/ownership state |
|---:|---|---|---|---|
| 3162 | field Terraria.Item.cachedItemSpawnsByType : int[] | D:\TRbackup\Version4\Terraria\Item.cs:68 | proposed.ItemStaticCapabilityCatalog / derived item spawn-count cache | selected readers/writers confirmed; reset/invalidation owner partial |
| 3168 | field Terraria.Item.headType : int[] | D:\TRbackup\Version4\Terraria\Item.cs:80 | proposed.ItemStaticCapabilityCatalog / armor-slot reverse index | selected readers/writers confirmed; reset/invalidation owner partial |
| 3169 | field Terraria.Item.bodyType : int[] | D:\TRbackup\Version4\Terraria\Item.cs:82 | proposed.ItemStaticCapabilityCatalog / armor-slot reverse index | selected readers/writers confirmed; reset/invalidation owner partial |
| 3170 | field Terraria.Item.legType : int[] | D:\TRbackup\Version4\Terraria\Item.cs:84 | proposed.ItemStaticCapabilityCatalog / armor-slot reverse index | selected readers/writers confirmed; reset/invalidation owner partial |
| 3171 | field Terraria.Item.staff : bool[] | D:\TRbackup\Version4\Terraria\Item.cs:86 | proposed.ItemStaticCapabilityCatalog / item capability mask | selected readers/writers confirmed; reset/invalidation owner partial |
| 3172 | field Terraria.Item.claw : bool[] | D:\TRbackup\Version4\Terraria\Item.cs:88 | proposed.ItemStaticCapabilityCatalog / item capability mask | selected readers/writers confirmed; reset/invalidation owner partial |
| 3283 | field Terraria.Item._phaseColors : Color[] | D:\TRbackup\Version4\Terraria\Item.cs:311 | proposed.ItemStaticCapabilityCatalog / lazy phase-color presentation cache | selected readers/writers confirmed; reset/invalidation owner partial |

## 8. Current NLTX mapping

当前 `src/Content` 存在 `ContentCatalog`、`ContentCatalogSnapshot`、`ContentDerivedIndexCatalog`、`ContentPresentationIndex` 等草案/部分结构；它们没有被本轮证明覆盖 P12 的 285 条来源成员，也没有证明与 Version4 的初始化、clear/rebuild、ID、TileObject、prefix/variant、Armor/Wing 或服务句柄语义等价。当前状态统一为 `partial`，不得视为本提案已经实现。

## 9. Network, persistence and presentation handoff

- 内容定义和注册索引默认不作为运行时 entity snapshot；若网络或存档需要稳定 ID，使用 proposed typed projection，并由 `crossSubsystemOwner: integration-review` 裁决版本、顺序和 unknown-field 策略。
- persistent ID 与 network ID 不能互换；ContentSamples 的双向字典只可作为 proposed lookup projection。网络入站只能生成 command，不能直接替换目录或实体状态。
- Font、Texture、shader、LocalizedText、NetworkText、Asset<T> 和第三方 service handle 只能经 proposed Adapter。Presentation projection 不得反向写模拟状态。
- Tile placement、ArmorSetEffect、WingStats external table、Item variant/prefix instance application、spawn cache 和 phase color 都需要明确失败、重试、幂等和清理策略。

## 10. Evidence gaps and blocking decisions

### Evidence gaps
- P12 第一轮 public-decomposition 输出路径 `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P12-content-definitions-catalogs-public-decomposition.md` 在 checkout 中不存在；本轮只使用当前输入报告、Version4 和已读取的交叉证据。
- 285 条成员的声明、关键初始化和部分直接读写已回到 Version4；仍有成员级完整 reader/writer、清理、网络、持久化和调度闭合缺口，成员表以 partial/evidence-gap 表示。
- 仓库引用的 `约束/公共拆分约束.md` 不存在；本轮遵循 AGENTS.md、ECS 文件组织约束、命名约束、C# 风格和副作用隔离规则，不据此宣称代码可实施。
- 当前 NLTX `src/Content` 的 ContentCatalog、ContentCatalogSnapshot、DerivedIndexes 和 PresentationIndexes 只是已有草案/部分结构；没有证据证明已映射 P12 的 285 条成员。
- SS14 没有直接对应的 Terraria 内容目录证据；只参考最小 ECS System/Query 形状，边界由 Version4 和 NLTX 约束决定。

### Blocking decisions
- crossSubsystemOwner: integration-review；最终必须裁决 Content ID、local type、persistent ID、network ID、EntityReference、TileCoordinate、ItemInstanceId、snapshot 和资源句柄的共享 owner。
- Tile placement hook、world/tile commit、TileEntity placement、liquid effects 和 System 顺序不能由 P12 单独决定；需明确单一 WorldInteraction/WorldStorage writer、失败重试和幂等语义。
- Main 服务句柄、ArmorSetEffect、WingStats 外部表、Item.Variant/Item prefix、玩家装备效果和 ContentSamples 样本对象的跨域提交/清理/网络/持久化边界待整合会话裁决。
- 版本/协议兼容策略需在 integration-review 选择：A 保留 legacy facade 只读适配，B 先建立 typed snapshot 再切换，C 对 unresolved 字段暂缓迁移；不同选择会改变回滚和删除门槛。

## 11. Focused verifier plan

- `P12-MemberCoverage`: 从输入报告解析 285 条，确认每个来源序号、声明类型、成员名、路径和行号在本设计只归属一个 proposed 边界。
- `P12-ContentSampleRebuild`: Initialize clear/rebuild、persistent/network ID 双射、Creative/Bestiary 派生索引和重复初始化。
- `P12-TileObjectRegistration`: CopyFrom/FullCopyFrom、alternate/subtile、readOnly barrier、geometry/style/placement 查询及 no direct world write。
- `P12-PlacementHookBoundary`: hook 顺序、command emission、TileEntity/world writer single-owner、失败/重试/幂等。
- `P12-VariantPrefixArmorWing`: condition/prefix/armor/wing definition truth tables、lookup rebuild、玩家实例与静态定义隔离。
- `P12-StaticCapability`: armor reverse index、staff/claw masks、spawn cache invalidation、phase-color projection。
- `P12-ExternalBoundary`: no Asset/Texture/shader/LocalizedText/service handle/network/persistence type crosses proposed core state.
- `P12-OwnerAndSchedule`: static/runtime proof of one writer per catalog and explicit phase dependencies.

The aggregate `ContentDefinitionsVerification` executable exercised all 18 proposed boundaries after a serial build and passed. The separate 285-row member-coverage parser, static external-type audit, full behavior-equivalence suite and network/persistence closure were not run; `verificationStatus: independently-verified` records only the fresh build and focused runtime evidence.

## 12. Integration Handoff

subsystemId: P12
taskNumber: P12-non-authoritative-public-decomposition-20260911
reportPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P12-content-definitions-catalogs-component-design.md
evidenceStatus: partial
nltxStatus: partial; existing Content catalog files are not mapped-complete evidence
verificationStatus: independently-verified
confirmedOwners:
- Version4 registration/read boundaries for ContentSamples, SetFactory, Colors, TileObjectData definitions, ItemVariants, PrefixLegacy, ArmorSetBonus, ArmorSetBonuses, WingStats and selected Main/Item initialization paths
proposedTypes:
- proposed.ContentAbilityCatalog
- proposed.MainContentServiceReferenceBoundary
- proposed.ContentSampleIndexCatalog
- proposed.SetArrayFactoryBoundary
- proposed.ColorAndShaderCatalog
- proposed.TileObjectInheritanceDefinition
- proposed.TileObjectGeometryDefinition
- proposed.TileObjectPlacementDefinition
- proposed.TileObjectPlacementHookDefinition
- proposed.TileObjectStyleDefinitionAndSelection
- proposed.ContentValidationBoundary
- proposed.ContentPresentationCatalog
- proposed.ItemVariantCatalog
- proposed.LegacyItemPrefixCatalog
- proposed.ArmorSetBonusDefinitionCatalog
- proposed.ArmorSetBonusLookupCatalog
- proposed.WingStatsDefinition
- proposed.ItemStaticCapabilityCatalog
sharedTypesForIntegrationReview:
- Content ID/local type/persistent ID/network ID/entity ID/ItemInstanceId/TileCoordinate/WorldSectionId
- typed network/persistence snapshots and external asset/service handles
crossSubsystemReaders:
- proposed Item, Player, NPC, Projectile, WorldInteraction, WorldStorage, UI, Presentation, Network and Persistence boundaries
crossSubsystemWriters:
- content bootstrap, world/tile placement, item initialization, player equipment, network load and persistence restore candidates; final writers unresolved
orderingConstraints:
- Content ID and SetFactory initialization before dependent indexes; TileObject registration before read-only runtime queries; lookup/index rebuild before consumers; external commands after pure qualification
boundaryChallenges:
- keep service references, caches, hooks, query payloads and presentation handles outside components; keep TileObject style definition and selection conceptually distinct inside one boundary
evidenceGaps:
- P12 第一轮 public-decomposition 输出路径 `D:\TRbackup\NLTX\docs\component-decomposition\review-round-1\2026-09-11-version4-non-authoritative-P12-content-definitions-catalogs-public-decomposition.md` 在 checkout 中不存在；本轮只使用当前输入报告、Version4 和已读取的交叉证据。
- 285 条成员的声明、关键初始化和部分直接读写已回到 Version4；仍有成员级完整 reader/writer、清理、网络、持久化和调度闭合缺口，成员表以 partial/evidence-gap 表示。
- 仓库引用的 `约束/公共拆分约束.md` 不存在；本轮遵循 AGENTS.md、ECS 文件组织约束、命名约束、C# 风格和副作用隔离规则，不据此宣称代码可实施。
- 当前 NLTX `src/Content` 的 ContentCatalog、ContentCatalogSnapshot、DerivedIndexes 和 PresentationIndexes 只是已有草案/部分结构；没有证据证明已映射 P12 的 285 条成员。
- SS14 没有直接对应的 Terraria 内容目录证据；只参考最小 ECS System/Query 形状，边界由 Version4 和 NLTX 约束决定。
blockingDecisions:
- crossSubsystemOwner: integration-review；最终必须裁决 Content ID、local type、persistent ID、network ID、EntityReference、TileCoordinate、ItemInstanceId、snapshot 和资源句柄的共享 owner。
- Tile placement hook、world/tile commit、TileEntity placement、liquid effects 和 System 顺序不能由 P12 单独决定；需明确单一 WorldInteraction/WorldStorage writer、失败重试和幂等语义。
- Main 服务句柄、ArmorSetEffect、WingStats 外部表、Item.Variant/Item prefix、玩家装备效果和 ContentSamples 样本对象的跨域提交/清理/网络/持久化边界待整合会话裁决。
- 版本/协议兼容策略需在 integration-review 选择：A 保留 legacy facade 只读适配，B 先建立 typed snapshot 再切换，C 对 unresolved 字段暂缓迁移；不同选择会改变回滚和删除门槛。
verificationEvidence:
- Build: `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`, where `$dotnetArgs` forwarded `build .\src2\ContentDefinitionsVerification\Terraria.ContentDefinitionsVerification.csproj -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false -p:BuildInParallel=false`; exit code 0, 0 warnings, 0 errors.
- Artifact: `D:\TRbackup\NLTX\Build\bin\Terraria.ContentDefinitions\Debug\net10.0\Terraria.ContentDefinitions.dll` and `D:\TRbackup\NLTX\Build\bin\Terraria.ContentDefinitionsVerification\Debug\net10.0\Terraria.ContentDefinitionsVerification.dll`.
- Verifier: `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments $dotnetArgs`, where `$dotnetArgs` forwarded `run --project .\src2\ContentDefinitionsVerification\Terraria.ContentDefinitionsVerification.csproj --no-build --no-restore`; exit code 0, output `P12 content definitions verifier passed.`
notImplemented:
- Production `src` migration, legacy-writer removal, dual write, network/persistence closure, external asset/service projection and final cross-subsystem owner decisions
- The separate 285-row member-coverage parser, static external-type audit, full behavior-equivalence suite and network/persistence closure
verifierPlan:
- The focused aggregate verifier was executed; the residual static coverage, external-boundary audit, owner/schedule proof and behavior-equivalence items remain evidence gaps

本报告是基于 Version4、tModLoader 公开文档和有限 ECS 结构参考形成的 proposed 内容定义与目录边界设计。它不是迁移完成报告、行为等价证明、API 兼容证明，也不是当前 NLTX 已实现能力的声明。
