# Version4 Non-Authoritative Top-12 Peer Refinement Design

## Goal

继续检查当前最终排行榜前 12 个 peer 的职责边界，并把能够由声明类型、源码路径或稳定成员族确认的混合职责拆成两组；保留全部 `4,017` 个字段、`525` 个属性和 `4,542` 条成员事实不变。

## Scope and Evidence

- 只修改非权威成员报告的生成器、验证器、计划和生成报告。
- `D:\TRbackup\Version4`、tModLoader 文档镜像和 SS14 checkout 只读；它们用于源码事实和组织模式交叉核对，不证明运行时读者、写者、生命周期、网络或持久化闭合。
- 本轮使用 `public-decomposition`：以共同访问边界、声明类型、源码路径和成员族划分，不按数量机械平分。
- 缺少完整 reader/writer/lifecycle 证据的边界保留 `reader/writer/lifecycle-partial`，不宣布 ECS 实现完成。

## Proposed Boundaries

| Current peer | Child A | Child B | Boundary evidence |
|---|---|---|---|
| `SharedTimeLoggerPhaseMetricsState` | `SharedTimeLoggerWorldRenderPhaseMetricsState` | `SharedTimeLoggerEntityAndInterfacePhaseMetricsState` | `Terraria.TimeLogger` phase-name families |
| `TileObjectStyleAndDrawState` | `TileObjectStyleCatalogState` | `TileObjectDrawGeometryState` | `TileObjectData` style vs draw/geometry members |
| `SharedFishingConditionCatalogState` | `SharedFishingRarityConditionCatalogState` | `SharedFishingEnvironmentConditionCatalogState` | `Rarity`/`FishRarityCondition` types vs populator catalog |
| `SharedDropRuleSelectionAndConditionState` | `SharedDropRuleConditionBranchState` | `SharedDropRuleSelectionAndQuantityState` | `Conditions.cs` and condition fields vs selection/quantity fields |
| `SharedDungeonControlAndTrapState` | `SharedDungeonTrapPlacementState` | `SharedDungeonControlLineGeometryState` | `DeadMansChestBiome.cs` vs `DungeonControlLine.cs` |
| `SharedSceneZoneDefinitionState` | `SharedSceneZoneGeometryAndThresholdState` | `SharedSceneBiomeAndEventDefinitionState` | scan geometry/height thresholds vs zone/event flags |
| `TileObjectPlacementRuleState` | `TileObjectAnchorAndLiquidPlacementState` | `TileObjectPlacementHookAndBaseState` | anchors/liquid rules vs hooks/base/subtiles |
| `MainCageTerrestrialCritterAnimationState` | `MainCageMammalAndReptileAnimationState` | `MainCageInsectAndSmallCritterAnimationState` | `Main.cs` animal animation families |
| `SharedAudioLegacySoundCatalogInstanceState` | `SharedAudioLegacyEnvironmentalInstanceState` | `SharedAudioLegacyPlayerAndInterfaceInstanceState` | `SoundInstance*` environmental vs player/UI names |
| `SharedAudioLegacySoundDefinitionCatalogState` | `SharedAudioLegacyGameplaySoundDefinitionCatalogState` | `SharedAudioLegacyInterfaceSoundDefinitionCatalogState` | paired `Sound*` definitions with the same name family |
| `MapEncodingHeaderCatalogState` | `MapEncodingHeaderBitCatalogState` | `MapEncodingOptionLimitState` | `Header*` bit layout vs limits/runtime draw option |
| `UiItemSlotCreativeCraftingAndUtilityContexts` | `UiItemSlotCreativeAndCraftingContextState` | `UiItemSlotHotbarDisplayAndUtilityContextState` | `Creative*`/`NewCraftingUI*` vs remaining context constants |

## Expected Result

The 12 retired peers become 24 active fifth-level children. The final active count changes from `324` to `336`; all member, field, property, parent and baseline totals remain unchanged. The report adds a fifth-level lineage table after the existing fourth-level table, and nested lineage expands `MapEncodingHeaderCatalogState` through its fifth-level children.

