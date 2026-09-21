# Version4 Non-Authoritative Top-13 Peer Refinement Design

## Goal

继续检查用户指定的非权威模拟系统排行榜前 13 个即时 peer，并为每个 peer 建立一次
有证据的职责拆分。这里的“前 13”是本轮拆分前的即时排行榜前缀；拆分完成后，最终
349 个 peer 会重新排序，不能把这 13 个输入排名误读为最终排名位置。

本轮保持 `4,017` 个字段、`525` 个属性和 `4,542` 条成员事实不变，把 13 个退休 peer
替换为 26 个非空第六层子组。最终活跃 peer 数从 `336` 变为 `349`。

## Scope And Evidence

- 只修改非权威成员报告的生成器、聚焦验证器、完整报告验证器、计划记录和生成报告。
- `src/`、Version4 参考源码、SS14 checkout 和 tModLoader 文档镜像保持只读；它们只用于
  源码声明、类型/路径和组件边界模式交叉核对。
- 使用 `public-decomposition` 的访问模式：按声明类型、源码路径和稳定成员族划分，
  不按字段数量机械平分。
- 每个最终子组保留原始基线和直接上一级 peer；不改变成员来源序号、声明类型、源码
  路径、声明文本或字段/属性分类。
- 当前证据确认的是源码库存归属和可导航边界。读者、写者、生命周期、持久化、网络
  所有权和运行时调度仍标为 `reader/writer/lifecycle-partial`，不宣称 ECS 运行时实现完成。

## Proposed Boundaries

| 拆分前即时 peer | 原统计 | 两个第六层子组 | 边界证据 |
|---|---:|---|---|
| `SharedTimeLoggerWorldRenderPhaseMetricsState` | 46 | `SharedTimeLoggerTileAndLiquidRenderMetricsState`（24）；`SharedTimeLoggerLightingMapAndBackgroundMetricsState`（22） | TimeLogger 固体、液体、墙体和 Tile 附加绘制阶段 vs 光照、地图、天空和背景阶段 |
| `SharedFishingEnvironmentConditionCatalogState` | 39 | `SharedFishingConditionCatalogPopulationState`（8）；`SharedFishingEnvironmentPredicateState`（31） | 条件目录/填充字段 vs 液体、深度、生物群落、海洋和世界事件谓词 |
| `SharedDropRuleSelectionAndQuantityState` | 36 | `SharedDropRuleChanceAndQuantityState`（16）；`SharedDropRuleOptionSelectionState`（20） | 掉落概率/数量参数类型 vs 选项选择状态 |
| `TileObjectStyleCatalogState` | 36 | `TileObjectStyleDefinitionCatalogState`（28）；`TileObjectStyleSelectionAndOverrideState`（8） | TileObjectData 风格定义属性 vs 风格选择、覆盖和随机范围成员 |
| `SharedDungeonRoomGeometryState` | 29 | `SharedDungeonRoomShapeGeometryState`（13）；`SharedDungeonRoomPlacementGeometryState`（16） | 房间形状/尺寸几何 vs 位置、起止点和布置几何 |
| `UiItemSortingCombatAndEquipmentCatalogState` | 29 | `UiItemSortingWeaponAndToolCatalogState`（21）；`UiItemSortingArmorAndAccessoryCatalogState`（8） | 武器/工具排序目录 vs Armor/Equip 装备排序目录 |
| `SharedDungeonStyleFurnitureAndRoomState` | 27 | `SharedDungeonStyleFurnitureCatalogState`（25）；`SharedDungeonStyleRoomVariantState`（2） | 家具/风格目录 vs 房间类型和子风格变体 |
| `SharedAudioLegacyEnvironmentalInstanceState` | 26 | `SharedAudioLegacyWorldEnvironmentInstanceState`（16）；`SharedAudioLegacyEntityFeedbackInstanceState`（10） | 世界环境声音实例 vs 实体反馈声音实例 |
| `SharedItemUseAndToolCapabilityState` | 26 | `SharedItemUseTimingAndConsumptionState`（15）；`SharedItemToolPlacementCapabilityState`（11） | 使用时序/消耗状态 vs 工具和 Tile 放置能力 |
| `SharedSceneBiomeAndEventDefinitionState` | 26 | `SharedSceneBiomeZoneDefinitionState`（17）；`SharedSceneWeatherAndEventZoneState`（9） | 生物群落区域定义 vs 天气和事件区域定义 |
| `SharedTilePaintState` | 26 | `SharedTilePaintRenderTargetState`（12）；`SharedTilePaintVariationAndColorState`（14） | 绘制目标/渲染状态 vs 变化、颜色和变体状态 |
| `SharedInvasionEventState` | 26 | `SharedInvasionDamageTrackingState`（4）；`SharedInvasionWaveAndArenaState`（22） | 入侵伤害跟踪 vs 波次、竞技场和事件进度 |
| `SharedStartupAndIssueReporting` | 26 | `SharedIssueReportCatalogState`（3）；`SharedStartupAndRuntimeHostState`（23） | 问题报告目录 vs 启动参数、运行时宿主和平台服务状态 |

其中前三类来自第五层子组的继续拆分，其他子组按当前直接 peer lineage 继续向下细化。
生成器保留每个成员的原始基线和 `PreviousPeer`，因此嵌套子组不会丢失从原始基线到
最终终端 peer 的路径。

## Boundary And Data Flow

```text
input member inventory
        |
        v
historical fine subsystem and baseline
        |
        v
existing second/fourth/fifth-level peer lineage
        |
        v
sixth-level mapping for 13 retired peers
        |
        v
349 final peers + ranking + per-member projection
```

`Get-SixthLevelFineSubsystemId` 是唯一成员归属入口。映射按 peer、声明类型、源码路径
或稳定成员族选择子组；任何未覆盖的目标成员都必须抛错。第六层定义只替换退休 peer
在最终定义集合中的位置，不创建运行时组件、System、Query 或持久化适配器。

## Non-Splits And Compatibility

- 不再对本轮 26 个子组按数量继续拆分；当前目标是确认第六层边界，不制造没有独立
  owner、生命周期或访问模式证据的原子组。
- 不修改 `src/` 和任何 C# API，因而不触发 ECS 文件移动、命名空间或运行时调度迁移。
- 不重排或重写输入成员事实；报告只改变最终 peer 归属、统计聚合和 lineage 展示。
- 仍保留 ID 类文件排除、输入 SHA-256 追踪、来源序号闭合和字段/属性总量不变量。

## Acceptance Contract

聚焦验证器必须确认 13 个退休 peer、26 个子组、每个子组非空、每个子组恰有一个直接
上级、每个退休 peer 的 rollup 与原统计相等，以及最终 `349` 个活跃 peer。完整验证器
还必须确认 `1..4,542` 来源序号、`4,017/525/4,542` 三项总量、最终完整排行榜、父级
汇总、所有历史 lineage 和 ID 类文件排除。

