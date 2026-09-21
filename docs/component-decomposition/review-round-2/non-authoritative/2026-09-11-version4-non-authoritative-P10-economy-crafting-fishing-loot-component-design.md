# P10 Proposed Component Design: Economy, Crafting, Fishing And Loot

partitionId: P10
sessionId: 35daf5b5baa2427fa8b7b778c2bce0f5
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\10-economy-crafting-fishing-loot.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P10-economy-crafting-fishing-loot-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P10-economy-crafting-fishing-loot-component-execution.md
handoffId: null
previousSessionId: 0a3c4113c9d546bf998f7d17339ba9e2
previousDocumentSessionId: 0a3c4113c9d546bf998f7d17339ba9e2
previousHandoffId: P10-handoff-20260912-implementation
handoffReason: runner Cleanup reset the prior terminal P10 record and Claim created this new manual session for Component-only scope correction.
designStatus: proposed
executionStatus: completed (Component-only scope correction)
implementationStatus: completed
verificationStatus: serial production build passed; no test or focused verifier was created because the task forbids non-Component verification code
completedComponents:
- proposed.ShopInventorySlotsComponent
- proposed.AnglerQuestStateComponent
currentComponent: none (allowed Component implementation complete)
pendingComponents:
- none (remaining proposed boundaries are excluded non-Component work and are deferred)
lastCheckpointUtc: 2026-09-12T10:27:47.1999522Z
evidence-gap:
- The first-round P10 public-decomposition output named by the task package was not found in the checkout.
- The partition report declares upstream source-report SHA-256 b944441d92d5103f218b766a3e354200794528a19e65a862e44c2b40a36f1196, while the observed SHA-256 of the current partition report is 8B6B00D33DE98A7A1047E7DF05F60E5A114AB270FF38C330995407F236C5874C; provenance must be reconciled before implementation.
- The referenced repository constraint file 约束\公共拆分约束.md is absent.
- Member-level readers, writers, initialization, cleanup, persistence, network semantics, and scheduler ownership are not closed for all 321 rows.
- Version4 contains incomplete or empty behavior in remote craft request deserialization, shop mood processing, fishing condition matching, and portions of item-drop rules; names and fields are not evidence of behavior equivalence.
- Existing NLTX files under src\Content, src\Fishing, src\Items\Crafting, src\Items\Commerce, and src\Items\Loot are candidate/current-state evidence only; they do not prove migration, parity, network closure, or persistence closure.
- The non-authoritative runner records P10 as terminal `completed` under manual session `35daf5b5baa2427fa8b7b778c2bce0f5`; the record was reset through runner `Cleanup`, re-entered through runner `Claim`, and settled through runner `Complete`.
- The P10 `src2` project remains isolated from existing `src2` projects and has no cross-project references.
- The previous non-Component source pass and verifier project were moved to the recoverable temporary quarantine recorded in Checkpoint 14; they are not part of the corrected source tree.
- The corrected P10 source tree contains only `ShopInventorySlotsComponent` and `AnglerQuestStateComponent` C# files; no test or verifier project remains in the P10 implementation scope.
- The corrected production build passed through the repository serial wrapper with 0 warnings and 0 errors; no test or focused verifier was created because the task forbids non-Component verification code.
blocking-decision:
- Final ownership of inventory, currency, item/world/NPC result commits, shop and angler quest state, recipe runtime identity, fishing result ownership, loot result ownership, network snapshots, persistence snapshots, and cross-partition scheduler order remains crossSubsystemOwner: integration-review.
- Only `proposed.ShopInventorySlotsComponent` and `proposed.AnglerQuestStateComponent` are implemented in this session. Every remaining proposed boundary is a non-Component role or an integration decision and is deferred by the task's explicit Component-only scope.
- The corrected source has no focused verifier by design; verification is limited to the recorded serial compilation of the production project.

> Checkpoint 0. This document is a proposed second-round design record for the 24 leaf groups and 321 member rows in P10. No target file, component, migration, API, or verification is claimed to exist.

## Scope And Exclusions

This partition covers shop and angler slots, item commerce and pricing, recipe definitions and crafting requests, fishing conditions and attempts, fish-drop rules, NPC item-drop rules, drop attribution, and loot simulation. The input inventory contains 24 complete leaf groups: 296 fields and 25 properties. The source sequence numbers are not a runtime order and are not contiguous; this document preserves them only as traceability keys.

The proposal excludes final ownership of player inventory and currency, NPC and world-item entity lifecycle, world and tile authority, network transport, persistence storage, UI widgets, localization, external platform services, and global tick scheduling. These boundaries may be consumed through ports or projections, but their owners are `crossSubsystemOwner: integration-review`.

The proposed decomposition deliberately does not create one `EconomyComponent`, one long-lived all-purpose `FishingAttempt` component, or one component containing every drop-rule field. Immutable definitions, transient evaluation context, derived reports, commands, and side-effect commits have different lifecycles and are therefore separate proposed artifacts.

## Version4 Evidence

The direct source evidence used for this proposal is limited to the current P10 report and relevant Version4 source/call sites:

- `Terraria/Main.cs`, `Terraria/Chest.cs`, `Terraria/NetMessage.cs`, `Terraria/MessageBuffer.cs`, and `Terraria/NPCInteractions.cs` for shop, travel-shop, angler-quest, and protocol-facing state.
- `Terraria/Item.cs`, `Terraria/Recipe.cs`, `Terraria/RecipeGroup.cs`, and `Terraria/ShoppingSettings.cs` for item definitions, recipe data, recipe-group registration, and shopping views.
- `Terraria.DataStructures/FishingAttempt.cs`, `Terraria.DataStructures/PlayerFishingConditions.cs`, and the `EntitySource_*` files for attempt snapshots and source attribution.
- `Terraria.GameContent/CraftingRequests.cs`, `Terraria.GameContent/ChumBucketProjectileHelper.cs`, `Terraria.GameContent/ItemShopSellbackHelper.cs`, and `Terraria.GameContent/ShopHelper.cs` for transient queues, frame caches, sellback memory, and pricing context.
- `Terraria.GameContent/FishDropRules/*` for fishing condition catalogs, predicates, rarity, fish possibilities, and fishing resolution.
- `Terraria.GameContent/ItemDropRules/*` and `Terraria.GameContent/LootSimulation/*` for drop-rule contracts, catalog/resolver state, chain rules, result reports, and simulation context.

Important observed facts:

- `FishingAttempt` is a transient one-attempt structure containing coordinates, bobber type, environmental values, water quality, chums, fishing level, rarity flags, quest fish, and rolled item/enemy results. It is not proposed as a persistent entity component.
- `FishingContext` carries a random source, `FishingAttempt`, player references, and biome/environment roll flags. The proposed query receives an explicit RNG port and an immutable context snapshot.
- `Recipe` combines recipe definition data, required items, recipe groups, tile requirements, shimmer results, and environment conditions. `RecipeGroup` is a static registration/catalog boundary using fake item IDs.
- `CraftingRequests.RemoteCraftRequest` is a transient request payload and `_pendingCrafts` is a process queue. `NetCraftingRequestsModule.Deserialize` is empty in Version4, so remote crafting is not behaviorally closed by the source.
- `ShopHelper` combines price bounds, current NPC/player context, happiness, a personality database, biome definitions, and mood weights. `ProcessMood` is empty in Version4. `ShoppingSettings` is a view/result value.
- `Main.shop`, `travelShop`, angler quest fields, and quest item IDs are static or session state. `Main.AnglerQuestSwap`, `Chest.SetupTravelShop`, and network message paths are relevant writers/readers, but final authority is not closed.
- `ChumBucketProjectileHelper` swaps pending and previous dictionaries per frame and clears the new pending dictionary. It is a transient frame cache, not long-lived player state.
- `EntitySource_* .Entity` fields are source-attribution references and must not become entity identity roots.
- `DropAttemptInfo` carries NPC/player references, RNG, simulation mode, expert mode, and master mode. `IItemDropRule` separates qualification/reporting/attempt behavior, while `ItemDropDatabase` and `ItemDropResolver` provide catalog lookup and resolution.
- Several drop rules directly call legacy item-spawn behavior. The proposal separates pure rule evaluation from result commit through an explicit port.
- `LootSimulation.SimulatorInfo` stores temporary player/time/position state and a counter. It remains query context and output projection state rather than persistent gameplay state.

## Current NLTX Evidence

The current checkout contains candidate definitions and components under `src\Content`, `src\Fishing`, `src\Items\Crafting`, `src\Items\Commerce`, and `src\Items\Loot`, including recipe/drop catalogs, fishing attempt and result states, commerce ledger/shop inventory, crafting reservations, and loot attribution/resolution state. Their presence is not treated as proof that any Version4 field has been migrated or that behavior is equivalent. This proposal therefore names domain-first target paths, but every path remains proposed until integration review, source mapping, and focused verification close.

## Proposed Boundary Inventory

| Proposed boundary | Responsibility | Component/System/Query/Command/Adapter/Projection | Key source groups |
|---|---|---|---|
| `proposed.ShopInventorySlotsComponent` | Per-shop slot contents and slot capacity view | Component, with shop command port | `MainShopAndQuestSlots` |
| `proposed.TravelShopCatalogState` | Travel-shop registration and generated item catalog | Component/catalog state, catalog system | `MainShopAndQuestSlots` |
| `proposed.AnglerQuestStateComponent` | Angler quest identity, completion, and quest-item catalog | Component plus quest command/query | `MainShopAndQuestSlots` |
| `proposed.FishingCatchAttributionAdapter` | Convert catch attribution to an explicit result-source reference | Adapter/command port | `SharedFishingCatchEffects` |
| `proposed.ChumFrameCache` | Pending and previous-frame chum counts | Transient cache/system state | `SharedFishingCatchEffects` |
| `proposed.ItemCommerceDefinition` | Item shop eligibility, purchase mode, base/custom price, and special currency reference | Definition/catalog state | `SharedItemCommerceState` |
| `proposed.SellbackMemoryState` | Sellback memo keyed by item type/prefix and stack | Component or scoped state, single commerce writer | `SharedItemCommerceAndPricing` |
| `proposed.ShopPricePolicyDefinition` | Price multiplier bounds and policy constants | Definition/query input | `SharedItemCommerceAndPricing` |
| `proposed.ShopMoodEvaluationContext` | Current buyer/seller references and calculated mood/price adjustment | Transient query context | `SharedItemCommerceAndPricing` |
| `proposed.ShopPersonalityCatalog` | NPC personality database reference | Catalog/adapter boundary | `SharedItemCommerceAndPricing` |
| `proposed.ShopBiomeModifierCatalog` | Dangerous-biome price modifier definitions | Definition/catalog | `SharedItemCommerceAndPricing` |
| `proposed.ShopMoodWeightsDefinition` | Like/dislike/love/hate weights | Definition/query input | `SharedItemCommerceAndPricing` |
| `proposed.ShoppingSettingsProjection` | Price, happiness report, and not-in-shop view | Projection/value result | `SharedItemCommerceAndPricing` |
| `proposed.LootSimulationContext` | Temporary simulation player/time/position/mode inputs | Query context | `SharedLootSimulation` |
| `proposed.LootSimulationCounterProjection` | Obtained-item counts for simulation output | Projection | `SharedLootSimulation` |
| `proposed.DropSourceAttribution` | Boss/drop/loot source reference without owning entity identity | Value object/adapter input | `SharedDropSourceAttribution` |
| `proposed.CraftingRequestCommand` | One remote or local craft request payload | Command | `CraftingRequestState` |
| `proposed.CraftingRequestQueueState` | Process-local pending request queue and readiness query | Scoped state/system | `CraftingRequestState` |
| `proposed.DropResolutionContext` | Immutable NPC/player/RNG/mode input for one drop attempt | Query context | `SharedDropRuleResolutionAndCatalogState` |
| `proposed.DropRateProjection` | Drop-rate and chain-feed report data | Projection | `SharedDropRuleResolutionAndCatalogState` |
| `proposed.DropAttemptResult` | Pure attempt outcome before world/inventory commit | Value result | `SharedDropRuleResolutionAndCatalogState` |
| `proposed.DropRuleCatalog` | Global and NPC-keyed drop-rule registration | Catalog state/registration system | `SharedDropRuleResolutionAndCatalogState` |
| `proposed.DropRuleResolver` | Resolve catalog entries by NPC network/type key | Query/adapter | `SharedDropRuleResolutionAndCatalogState` |
| `proposed.DropRuleChainContract` | Chained-rule references and chain-attempt contract | Interface/value contract | `SharedDropRuleResolutionAndCatalogState` |
| `proposed.FishingConditionDisplayMetadata` | Display-skip metadata for fishing conditions | Definition/query input | `SharedFishingConditionContextState` |
| `proposed.FishingQuestConditionDefinition` | Quest-fish checked type definitions | Definition | `SharedFishingConditionContextState` |
| `proposed.FishingConditionEvaluationContext` | Explicit random source, player references, and rolled biome flags | Transient query context | `SharedFishingConditionContextState` |
| `proposed.RecipeIngredientDefinition` | Required item/group identity and stack requirement | Definition | `RecipeDefinitionState` |
| `proposed.RecipeDefinition` | Recipe output, tile/input/environment/shimmer definition data | Definition/catalog state | `RecipeDefinitionState` |
| `proposed.RecipeCraftingRuntimeState` | Owned-item and recipe-chest lookup inputs | Transient query state | `RecipeDefinitionState` |
| `proposed.RecipeGroupCatalog` | Group registration, fake IDs, text, valid items, and decraft mapping | Catalog/registration system | `RecipeGroupCatalogState` |
| `proposed.FishingAttemptConditionSnapshot` | Player fishing-condition snapshot attached to one attempt | Transient value | `FishingAttemptState` |
| `proposed.FishingAttemptLocationContext` | Attempt coordinates and bobber identity | Transient value | `FishingAttemptState` |
| `proposed.FishingRollClassification` | Common/uncommon/rare/legendary/crate/junk roll flags | Transient query result | `FishingAttemptState` |
| `proposed.FishingEnvironmentSnapshot` | Liquid, water amount/quality, chums, and lava/honey state | Transient value | `FishingAttemptState` |
| `proposed.FishingPowerSnapshot` | Fishing level and lava capability | Transient value/query result | `FishingAttemptState` |
| `proposed.FishingWorldPredicateSnapshot` | Atmospheric and height values used by conditions | Transient value | `FishingAttemptState` |
| `proposed.FishingResultDecision` | Rolled item and enemy result references | Query result/command input | `FishingAttemptState` |
| `proposed.PlayerFishingInputSnapshot` | Pole/bait identity and power inputs | Query input | `PlayerFishingConditionState` |
| `proposed.PlayerFishingLevelQueryResult` | Derived final fishing level and multipliers | Query result | `PlayerFishingConditionState` |
| `proposed.DropChainVisibilityPolicy` | Hide-report flags for chain rules | Definition/query input | `SharedDropRuleChainState` |
| `proposed.DropChainAttemptContract` | Rule-to-chain attempt values | Interface/value contract | `SharedDropRuleChainState` |
| `proposed.DropRuleChainDefinition` | Chained rule collections and rule chaining metadata | Definition/catalog state | `SharedDropRuleChainState` |
| `proposed.FishingDropRuleDefinition` | Fish candidates, chance, conditions, and rarity | Definition | `SharedFishingDropResolutionState` |
| `proposed.FishingDropRuleCatalog` | Ordered fish-rule collection | Catalog state | `SharedFishingDropResolutionState` |
| `proposed.FishingPossibilityEntry` | Item type and frequency option | Value definition | `SharedFishingDropResolutionState` |
| `proposed.ItemCurrencyEconomyDefinition` | Coin IDs and critter currency presentation value | Definition/catalog state | `SharedItemEconomyAndValueRules` |
| `proposed.ItemEventPricingDefinition` | Event and boss price constants | Definition/catalog state | `SharedItemEconomyAndValueRules` |
| `proposed.ItemPickupTimingDefinition` | Item pickup ranges | Definition/catalog state | `SharedItemUseTimingAndStackRules` |
| `proposed.ItemBuffDurationDefinition` | Luck/flask duration constants | Definition/catalog state | `SharedItemUseTimingAndStackRules` |
| `proposed.ItemStackAndUseDelayDefinition` | Common stack and consumption delay constants | Definition/catalog state | `SharedItemUseTimingAndStackRules` |
| `proposed.ItemPlacementAndPickupPolicyDefinition` | Food dimensions, wall placement, pickup replacement, and emergency stacking policy | Definition/catalog state | `SharedItemUseTimingAndStackRules` |
| `proposed.FishingRarityPredicate` | Delegate-backed rarity predicate | Query/adapter boundary | `SharedFishingRarityConditionCatalogState` |
| `proposed.FishingRarityCatalog` | Rarity enum-like catalog and compound rarity definitions | Catalog state | `SharedFishingRarityConditionCatalogState` |
| `proposed.FishingRarityConditionDefinition` | Visual frequency and any-rarity condition | Definition | `SharedFishingRarityConditionCatalogState` |
| `proposed.DropConditionDefinition` | Named NPC, AI value, and wave threshold conditions | Definition/query | `SharedDropRuleConditionBranchState` |
| `proposed.DropConditionBranchRuntime` | Conditions attached to per-player/local/leading/mech-boss branches | Transient query/branch state | `SharedDropRuleConditionBranchState` |
| `proposed.FishingConditionDelegateDefinition` | Delegate-backed fishing condition definition | Definition/adapter | `SharedFishingConditionCatalogPopulationState` |
| `proposed.FishingConditionCatalog` | Registered early/hard mode, junk/crate, enemy, and combat-book conditions | Catalog state | `SharedFishingConditionCatalogPopulationState` |
| `proposed.FishingFluidPredicate` | Lava and honey predicates | Query | `SharedFishingEnvironmentPredicateState` |
| `proposed.FishingBiomePredicate` | Dungeon, beach, hallow, mushroom, desert, snow, jungle, and evil biome predicates | Query | `SharedFishingEnvironmentPredicateState` |
| `proposed.FishingDepthPredicate` | Height-band predicates | Query | `SharedFishingEnvironmentPredicateState` |
| `proposed.FishingWorldPredicate` | Rock layer, ocean, water-count, and world-seed predicates | Query | `SharedFishingEnvironmentPredicateState` |
| `proposed.FishingEventPredicate` | Blood Moon and other world-event predicates | Query | `SharedFishingEnvironmentPredicateState` |
| `proposed.CommonDropChanceQuantityDefinition` | Common-drop item, chance, and quantity definition | Definition | `SharedDropRuleChanceAndQuantityState` |
| `proposed.DropRerollPolicy` | Reroll count policy | Definition | `SharedDropRuleChanceAndQuantityState` |
| `proposed.DropOneByOneParameters` | Per-chunk chance, count, stack, and player bonus parameters | Definition value | `SharedDropRuleChanceAndQuantityState` |
| `proposed.DropOneByOneDefinition` | Item and one-by-one parameters | Definition | `SharedDropRuleChanceAndQuantityState` |
| `proposed.DropModeOptionSelector` | Normal/expert/master/extra-gel rule selection | Query/definition | `SharedDropRuleOptionSelectionState` |
| `proposed.DropOptionsWithoutRepeatsState` | Temporary available-option list and selection count | Transient query state | `SharedDropRuleOptionSelectionState` |
| `proposed.DropItemOptionSelector` | Item option IDs and chance | Query/definition | `SharedDropRuleOptionSelectionState` |
| `proposed.DropRuleOptionSelector` | Rule option set and selector chance | Query/definition | `SharedDropRuleOptionSelectionState` |
| `proposed.ItemUseTimingConsumptionDefinition` | Item use style, timing, consumability, and presentation capabilities | Definition/catalog state | `SharedItemUseTimingAndConsumptionState` |

Every name in this table is prefixed with `proposed.` intentionally. A source fine group can map to multiple proposed boundaries when its fields have different lifecycles or side-effect responsibilities.

## Complete Member Ownership Matrix

The following matrix is the complete 321-row source inventory expressed as source-sequence ownership. Each listed sequence maps to the exact class, relative path, absolute path, line, column, member name, type, and original declaration in the input report. A sequence appears exactly once in this matrix; it is a traceability key, never a scheduling key. The target paths below are the actual implementation paths under `src2`; the component contracts and cross-partition ownership decisions remain proposed until integration review closes them.

| Source sequence(s) | Proposed role and target path |
|---|---|
| 405 | `proposed.ShopInventorySlotsComponent` -> `src2/EconomyCraftingFishingLoot/Commerce/ShopInventorySlotsComponent.cs` |
| 406, 407 | `proposed.TravelShopCatalogState` -> `src2/EconomyCraftingFishingLoot/Commerce/TravelShopCatalogState.cs` |
| 408, 409, 410, 411 | `proposed.AnglerQuestStateComponent` -> `src2/EconomyCraftingFishingLoot/Fishing/AnglerQuestStateComponent.cs` |
| 1130 | `proposed.FishingCatchAttributionAdapter` -> `src2/EconomyCraftingFishingLoot/Fishing/Adapters/FishingCatchAttributionAdapter.cs` |
| 2350, 2351 | `proposed.ChumFrameCache` -> `src2/EconomyCraftingFishingLoot/Fishing/ChumFrameCache.cs` |
| 3185, 3248, 3252, 3253, 3263, 3264 | `proposed.ItemCommerceDefinition` -> `src2/EconomyCraftingFishingLoot/Content/ItemCommerceDefinition.cs` |
| 2429, 2430, 2431, 2432 | `proposed.SellbackMemoryState` -> `src2/EconomyCraftingFishingLoot/Commerce/SellbackMemoryState.cs` |
| 2506, 2507, 2508 | `proposed.ShopPricePolicyDefinition` -> `src2/EconomyCraftingFishingLoot/Commerce/ShopPricePolicyDefinition.cs` |
| 2509, 2510, 2511, 2512 | `proposed.ShopMoodEvaluationContext` -> `src2/EconomyCraftingFishingLoot/Commerce/ShopMoodEvaluationContext.cs` |
| 2513 | `proposed.ShopPersonalityCatalog` -> `src2/EconomyCraftingFishingLoot/Commerce/ShopPersonalityCatalog.cs` |
| 2514 | `proposed.ShopBiomeModifierCatalog` -> `src2/EconomyCraftingFishingLoot/Commerce/ShopBiomeModifierCatalog.cs` |
| 2515, 2516, 2517, 2518 | `proposed.ShopMoodWeightsDefinition` -> `src2/EconomyCraftingFishingLoot/Commerce/ShopMoodWeightsDefinition.cs` |
| 3485, 3486, 4023 | `proposed.ShoppingSettingsProjection` -> `src2/EconomyCraftingFishingLoot/Commerce/ShoppingSettingsProjection.cs` |
| 2130, 2131 | `proposed.LootSimulationCounterProjection` -> `src2/EconomyCraftingFishingLoot/Loot/LootSimulationCounterProjection.cs` |
| 2132, 2133, 2134, 2135, 2136, 2137, 2138 | `proposed.LootSimulationContext` -> `src2/EconomyCraftingFishingLoot/Loot/LootSimulationContext.cs` |
| 1125, 1129, 1134 | `proposed.DropSourceAttribution` -> `src2/EconomyCraftingFishingLoot/Loot/DropSourceAttribution.cs` |
| 2363, 2364, 2365, 2366, 2367 | `proposed.CraftingRequestCommand` -> `src2/EconomyCraftingFishingLoot/Crafting/CraftingRequestCommand.cs` |
| 2368, 3850 | `proposed.CraftingRequestQueueState` -> `src2/EconomyCraftingFishingLoot/Crafting/CraftingRequestQueueState.cs` |
| 2049, 2050, 2051, 2052, 2053, 2054 | `proposed.DropResolutionContext` -> `src2/EconomyCraftingFishingLoot/Loot/DropResolutionContext.cs` |
| 2076, 2077, 2078, 2079, 2080, 2081, 2082 | `proposed.DropRateProjection` -> `src2/EconomyCraftingFishingLoot/Loot/DropRateProjection.cs` |
| 2086 | `proposed.DropAttemptResult` -> `src2/EconomyCraftingFishingLoot/Loot/DropAttemptResult.cs` |
| 2087, 2088, 2089, 2090 | `proposed.DropRuleCatalog` -> `src2/EconomyCraftingFishingLoot/Content/DropRuleCatalog.cs` |
| 2091 | `proposed.DropRuleResolver` -> `src2/EconomyCraftingFishingLoot/Loot/DropRuleResolver.cs` |
| 3812, 3813 | `proposed.DropRuleChainContract` -> `src2/EconomyCraftingFishingLoot/Loot/DropRuleChainContract.cs` |
| 1652 | `proposed.FishingConditionDisplayMetadata` -> `src2/EconomyCraftingFishingLoot/Content/FishingConditionDisplayMetadata.cs` |
| 1659, 1660 | `proposed.FishingQuestConditionDefinition` -> `src2/EconomyCraftingFishingLoot/Content/FishingQuestConditionDefinition.cs` |
| 1661, 1662, 1663, 1664, 1665, 1666, 1667, 1668, 1669, 1670 | `proposed.FishingConditionEvaluationContext` -> `src2/EconomyCraftingFishingLoot/Fishing/FishingConditionEvaluationContext.cs` |
| 3398, 3399, 3976, 3977 | `proposed.RecipeIngredientDefinition` -> `src2/EconomyCraftingFishingLoot/Content/RecipeIngredientDefinition.cs` |
| 3400, 3401, 3402, 3403, 3404, 3405, 3406, 3407, 3408, 3409, 3410, 3411, 3412, 3413, 3414, 3415, 3416, 3417, 3418 | `proposed.RecipeDefinition` -> `src2/EconomyCraftingFishingLoot/Content/RecipeDefinition.cs` |
| 3419, 3420 | `proposed.RecipeCraftingRuntimeState` -> `src2/EconomyCraftingFishingLoot/Crafting/RecipeCraftingRuntimeState.cs` |
| 3421, 3422, 3423, 3424, 3425, 3426, 3427, 3978 | `proposed.RecipeGroupCatalog` -> `src2/EconomyCraftingFishingLoot/Content/RecipeGroupCatalog.cs` |
| 1141 | `proposed.FishingAttemptConditionSnapshot` -> `src2/EconomyCraftingFishingLoot/Fishing/FishingAttemptConditionSnapshot.cs` |
| 1142, 1143, 1144 | `proposed.FishingAttemptLocationContext` -> `src2/EconomyCraftingFishingLoot/Fishing/FishingAttemptLocationContext.cs` |
| 1145, 1146, 1147, 1148, 1149, 1150, 1151 | `proposed.FishingRollClassification` -> `src2/EconomyCraftingFishingLoot/Fishing/FishingRollClassification.cs` |
| 1152, 1153, 1154, 1155, 1156, 1157 | `proposed.FishingEnvironmentSnapshot` -> `src2/EconomyCraftingFishingLoot/Fishing/FishingEnvironmentSnapshot.cs` |
| 1158, 1159 | `proposed.FishingPowerSnapshot` -> `src2/EconomyCraftingFishingLoot/Fishing/FishingPowerSnapshot.cs` |
| 1160, 1161, 1162 | `proposed.FishingWorldPredicateSnapshot` -> `src2/EconomyCraftingFishingLoot/Fishing/FishingWorldPredicateSnapshot.cs` |
| 1163, 1164 | `proposed.FishingResultDecision` -> `src2/EconomyCraftingFishingLoot/Fishing/FishingResultDecision.cs` |
| 1224, 1225, 1226, 1227, 1228 | `proposed.PlayerFishingInputSnapshot` -> `src2/EconomyCraftingFishingLoot/Fishing/PlayerFishingInputSnapshot.cs` |
| 1229 | `proposed.PlayerFishingLevelQueryResult` -> `src2/EconomyCraftingFishingLoot/Fishing/PlayerFishingLevelQueryResult.cs` |
| 2036, 2037, 2038 | `proposed.DropChainVisibilityPolicy` -> `src2/EconomyCraftingFishingLoot/Loot/DropChainVisibilityPolicy.cs` |
| 3801, 3802, 3803 | `proposed.DropChainAttemptContract` -> `src2/EconomyCraftingFishingLoot/Loot/DropChainAttemptContract.cs` |
| 3804, 3805, 3806, 3807, 3808, 3809, 3810, 3811, 3814, 3815, 3816, 3817, 3818, 3819, 3820 | `proposed.DropRuleChainDefinition` -> `src2/EconomyCraftingFishingLoot/Content/DropRuleChainDefinition.cs` |
| 1653, 1654, 1655, 1656, 1657 | `proposed.FishingDropRuleDefinition` -> `src2/EconomyCraftingFishingLoot/Content/FishingDropRuleDefinition.cs` |
| 1658 | `proposed.FishingDropRuleCatalog` -> `src2/EconomyCraftingFishingLoot/Content/FishingDropRuleCatalog.cs` |
| 1671, 1672 | `proposed.FishingPossibilityEntry` -> `src2/EconomyCraftingFishingLoot/Content/FishingPossibilityEntry.cs` |
| 3149, 3150, 3151, 3152, 3153 | `proposed.ItemCurrencyEconomyDefinition` -> `src2/EconomyCraftingFishingLoot/Content/ItemCurrencyEconomyDefinition.cs` |
| 3154, 3155, 3156, 3157, 3158, 3159, 3160 | `proposed.ItemEventPricingDefinition` -> `src2/EconomyCraftingFishingLoot/Content/ItemEventPricingDefinition.cs` |
| 3140, 3141, 3142, 3143 | `proposed.ItemPickupTimingDefinition` -> `src2/EconomyCraftingFishingLoot/Content/ItemPickupTimingDefinition.cs` |
| 3145, 3146, 3147, 3148 | `proposed.ItemBuffDurationDefinition` -> `src2/EconomyCraftingFishingLoot/Content/ItemBuffDurationDefinition.cs` |
| 3161, 3163, 3164, 3165, 3166 | `proposed.ItemStackAndUseDelayDefinition` -> `src2/EconomyCraftingFishingLoot/Content/ItemStackAndUseDelayDefinition.cs` |
| 3280, 3281, 3282, 3284, 3285 | `proposed.ItemPlacementAndPickupPolicyDefinition` -> `src2/EconomyCraftingFishingLoot/Content/ItemPlacementAndPickupPolicyDefinition.cs` |
| 1605 | `proposed.FishingRarityPredicate` -> `src2/EconomyCraftingFishingLoot/Fishing/FishingRarityPredicate.cs` |
| 1606, 1607, 1608, 1609, 1610, 1611, 1612, 1613 | `proposed.FishingRarityCatalog` -> `src2/EconomyCraftingFishingLoot/Content/FishingRarityCatalog.cs` |
| 1673, 1674 | `proposed.FishingRarityConditionDefinition` -> `src2/EconomyCraftingFishingLoot/Content/FishingRarityConditionDefinition.cs` |
| 2045, 2046, 2047, 2048 | `proposed.DropConditionDefinition` -> `src2/EconomyCraftingFishingLoot/Content/DropConditionDefinition.cs` |
| 2064, 2075, 2092, 2093, 2094 | `proposed.DropConditionBranchRuntime` -> `src2/EconomyCraftingFishingLoot/Loot/DropConditionBranchRuntime.cs` |
| 1604 | `proposed.FishingConditionDelegateDefinition` -> `src2/EconomyCraftingFishingLoot/Content/FishingConditionDelegateDefinition.cs` |
| 1614, 1615, 1616, 1619, 1620, 1621, 1651 | `proposed.FishingConditionCatalog` -> `src2/EconomyCraftingFishingLoot/Content/FishingConditionCatalog.cs` |
| 1617, 1618 | `proposed.FishingFluidPredicate` -> `src2/EconomyCraftingFishingLoot/Fishing/FishingFluidPredicate.cs` |
| 1622, 1623, 1624, 1625, 1626, 1627, 1628, 1629 | `proposed.FishingBiomePredicate` -> `src2/EconomyCraftingFishingLoot/Fishing/FishingBiomePredicate.cs` |
| 1630, 1631, 1632, 1633, 1634, 1635, 1636, 1637, 1638 | `proposed.FishingDepthPredicate` -> `src2/EconomyCraftingFishingLoot/Fishing/FishingDepthPredicate.cs` |
| 1639, 1640, 1641, 1642, 1643, 1644, 1645, 1646, 1647, 1648, 1649 | `proposed.FishingWorldPredicate` -> `src2/EconomyCraftingFishingLoot/Fishing/FishingWorldPredicate.cs` |
| 1650 | `proposed.FishingEventPredicate` -> `src2/EconomyCraftingFishingLoot/Fishing/FishingEventPredicate.cs` |
| 2039, 2040, 2041, 2042, 2043 | `proposed.CommonDropChanceQuantityDefinition` -> `src2/EconomyCraftingFishingLoot/Content/CommonDropChanceQuantityDefinition.cs` |
| 2044 | `proposed.DropRerollPolicy` -> `src2/EconomyCraftingFishingLoot/Content/DropRerollPolicy.cs` |
| 2065, 2066, 2067, 2068, 2069, 2070, 2071, 2072 | `proposed.DropOneByOneParameters` -> `src2/EconomyCraftingFishingLoot/Content/DropOneByOneParameters.cs` |
| 2073, 2074 | `proposed.DropOneByOneDefinition` -> `src2/EconomyCraftingFishingLoot/Content/DropOneByOneDefinition.cs` |
| 2055, 2056, 2057, 2058, 2059, 2060, 2061, 2062, 2063 | `proposed.DropModeOptionSelector` -> `src2/EconomyCraftingFishingLoot/Content/DropModeOptionSelector.cs` |
| 2083, 2084, 2085 | `proposed.DropOptionsWithoutRepeatsState` -> `src2/EconomyCraftingFishingLoot/Loot/DropOptionsWithoutRepeatsState.cs` |
| 2095, 2096, 2097, 2098, 2099, 2100 | `proposed.DropItemOptionSelector` -> `src2/EconomyCraftingFishingLoot/Content/DropItemOptionSelector.cs` |
| 2101, 2102 | `proposed.DropRuleOptionSelector` -> `src2/EconomyCraftingFishingLoot/Content/DropRuleOptionSelector.cs` |
| 3191, 3192, 3193, 3194, 3195, 3196, 3210, 3211, 3212, 3213, 3250, 3251, 3257, 3265, 3277 | `proposed.ItemUseTimingConsumptionDefinition` -> `src2/EconomyCraftingFishingLoot/Content/ItemUseTimingConsumptionDefinition.cs` |

The matrix has 321 entries by source sequence, 296 source fields, and 25 source properties. It is intentionally a proposed role allocation: a later implementation may split a target file further after reader/writer evidence, but it must preserve this traceability and record any change as an integration decision.

## State, Invariants, And Composition

### Definitions And Catalogs

Recipe, recipe-group, item commerce/value/use, fishing condition/rarity/environment, fish-drop, and item-drop definitions are immutable after registration. Registration systems own validation and duplicate detection. Runtime components store references or compact snapshots, not static catalogs or external `Terraria` objects. Fake item IDs, network IDs, entity IDs, persistence IDs, and external IDs remain distinct values.

### Transient Context And Queries

Fishing attempts, player fishing conditions, drop attempts, shop mood context, craft requests, and loot simulation context are scoped to one request, attempt, frame, or simulation. Queries are deterministic with respect to their explicit inputs and RNG port. Queries do not enqueue, consume inventory, modify currency, spawn entities, or mutate catalog state.

### Commands, Reservations, And Commits

Crafting, purchase/sale, fishing catch delivery, and loot result delivery are commands. Each command must carry an operation or request identity where retries are possible. Reservation and commit ownership is outside this partition until integration review assigns inventory/currency/world-item/NPC owners. A result decision is not a committed world effect.

### Drop And Fishing Invariants

- A chance roll is made by one explicit RNG port owned by the evaluating system; no rule object silently obtains global randomness.
- A drop rule may produce a decision, rate report, or chain feed, but cannot directly mutate inventory or spawn a world item in the proposed model.
- A fishing attempt expires after its result decision and commit command are emitted; it is not persisted as an ongoing entity component.
- Chum pending counts are frame-scoped and are swapped/cleared by one cache system; no other system writes the pending or previous-frame maps.
- Option selectors do not mutate their catalog arrays. Temporary no-repeat availability is local to one evaluation and is cleared on completion or failure.
- Shop projections are views. Pricing policy and mood context do not write item definitions or player/NPC components.

### Crafting And Commerce Invariants

- Recipe definitions and recipe-group catalogs are read-only after registration.
- Material qualification is a query; reservation and consumption are commands with one inventory owner.
- A purchase or sellback operation is idempotent by transaction identity. `buyOnce`, special currency, and custom price are inputs to the offer/transaction decision, not direct balance writes.
- `ShoppingSettings` is a derived projection and cannot become the owner of shop inventory, currency, or NPC happiness.

## Readers, Writers, And Lifecycle

The source report confirms declarations and the direct source review confirms selected call sites, but it does not close every member-level reader/writer. The following are proposed ownership contracts that must be verified before implementation:

| State or artifact | Proposed single writer | Readers | Initialization/cleanup |
|---|---|---|---|
| Shop and travel-shop slots | Shop generation/transaction system after integration owner is assigned | Shop query, network projection, UI projection | Session/NPC shop open; clear on close or regeneration |
| Angler quest state | Angler quest command system | Fishing query, quest UI, network/persistence projections | World/session start and day rollover; explicit reset on quest swap |
| Catalog definitions | Registration system | Pure queries and projections | Content bootstrap; immutable for the session |
| Craft request queue | Craft request queue system | Craft scheduler and diagnostics | Enqueue on command; dequeue on accepted/rejected/expired request |
| Chum frame cache | Chum frame system | Fishing condition query | Swap at frame boundary; clear after consumption |
| Fishing attempt state | Fishing attempt system | Fishing conditions, resolver, result command builder | Create at bobber resolution; expire after decision or cancellation |
| Drop resolution context | Drop resolver system | Rule queries and report projection | Create per attempt; release after result/report |
| Loot simulation context/counter | Simulation runner | Simulation rules and report projection | Create per simulation; restore external view and discard after completion |
| Sellback memory | Commerce system | Sellback query and projection | Load on shop scope; clear/expire under explicit policy |
| Result commit | Integration-owned inventory/world/NPC adapters | Network, persistence, UI projections | Idempotent apply; retry or dead-letter behavior must be assigned by integration review |

## Dependency Direction And Explicit System Order

The proposed dependency direction is:

`catalog registration -> request/attempt context snapshot -> pure eligibility query -> one explicit RNG resolution -> result decision -> reservation/commit through a port -> idempotency/completion update -> network/persistence/UI projection`

The detailed proposed order is:

1. `CatalogRegistrationSystem` validates recipe, recipe-group, item, fishing, and drop definitions.
2. `ShopAndQuestStateSystem`, `FishingAttemptSnapshotSystem`, `CraftRequestIngressSystem`, `DropAttemptContextSystem`, or `LootSimulationSystem` creates a scoped input snapshot.
3. `EligibilityQuerySystem` evaluates conditions without side effects.
4. `RandomResolutionSystem` consumes the explicit RNG port once per documented roll and records the decision inputs.
5. `ResultDecisionSystem` produces craft, purchase, fishing, or loot decisions and chain feeds.
6. `ReservationAndCommitSystem` calls integration-owned inventory, currency, world-item, NPC-spawn, and quest ports.
7. `CompletionAndIdempotencySystem` records accepted/rejected/expired state keyed by operation identity.
8. `ProjectionSystem` emits network snapshots, persistence records, UI/shop views, loot reports, and diagnostics.

No file or directory order may define this sequence. Cross-partition order is `crossSubsystemOwner: integration-review`.

## ID And Boundary Rules

- Entity identity is an entity/runtime reference and is not the same as `itemId`, `npc.netID`, recipe/group ID, or a source reference.
- Persistent IDs for items, containers, transactions, reservations, and operations are persistence concerns and are not inferred from Version4 integer fields.
- Network IDs are protocol keys for snapshots or routing. They are not persistence IDs and do not define catalog ownership.
- External IDs identify platform/content integrations and remain in adapters.
- Recipe-group fake item IDs and item type IDs are catalog keys. They must not be used as entity identity or persistence identity.
- `EntitySource_* .Entity` is attribution input; it does not own the referenced entity and must not be converted into a universal identity component.

All shared cross-partition IDs, result ports, snapshots, network records, persistence records, and ordering contracts carry `crossSubsystemOwner: integration-review` until an integration decision is recorded.

## Network, Persistence, And Client Projection

Network adapters should serialize versioned projections of shop slots, travel-shop entries, angler quest state, craft request acknowledgements, and committed fishing/loot results. They must not serialize RNG objects, live `Player`/`NPC` references, delegate instances, temporary frame caches, or unresolved source objects.

Persistence adapters should store durable catalog keys, accepted transaction/request identities, committed inventory/currency/result effects, and explicitly durable quest state only after integration review assigns owners. Attempt contexts, drop-rate reports, `ShoppingSettings`, loot simulation counters, and temporary condition flags are not durable by default.

Client projections are read-only. They may build shop views, happiness reports, loot reports, and fishing displays from snapshots, but cannot write catalog definitions, authoritative balances, reservations, or world results.

## Evidence Gaps And Blocking Decisions

The missing first-round P10 report, source hash mismatch, missing common constraint file, incomplete Version4 methods, and unclosed member-level access/lifecycle evidence are release-blocking for implementation claims. In particular, remote crafting cannot be treated as protocol-closed while deserialization is empty; shop mood cannot be treated as behaviorally complete while `ProcessMood` is empty; and direct drop-rule side effects cannot be migrated without a result-commit contract.

The integration owner must decide the final owner and protocol for inventory/currency reservation and commit, item/world/NPC result spawning, shop/angler session state, recipe runtime identity, fishing result ownership, loot result ownership, network snapshots, persistence snapshots, retry/idempotency, and cross-partition scheduler order. Until then these are proposed seams only.

## Focused Verifier Plan

No verifier has been run in this session. The later implementation should add focused checks for:

- source sequence coverage: all 321 input rows map once, with 296 fields and 25 properties;
- catalog immutability and duplicate registration rejection;
- deterministic condition and qualification queries with injected RNG and no state mutation;
- one-roll and retry semantics for common, option, chain, reroll, and one-by-one drop rules;
- fishing attempt expiry, rarity/environment predicate combinations, chum frame swap/clear, and result decision idempotency;
- recipe-group fake-ID mapping, material reservation/consumption, and remote craft request acknowledgement;
- shop price/mood projection purity, sellback memory isolation, purchase/sale idempotency, and special-currency routing;
- network/persistence projection schemas that exclude live references, RNG, delegates, and transient caches;
- adapter tests for legacy source attribution and world-item/NPC result ports;
- scheduler tests proving the explicit order and single-writer assertions.

For compile-capable verification, use the affected project only through `Build/Tools/Invoke-SerialDotnet.ps1`, inspect active `dotnet.exe`/`csc.exe` processes first, use `-p:UseSharedCompilation=false`, verify the artifact under `Build/bin/`, and run the verifier with `--no-build --no-restore`. No such command was run here because this session changed only documentation.

## Implementation Checkpoint 1: proposed.RecipeGroupCatalog

Implemented files:

- `src2/EconomyCraftingFishingLoot/Content/RecipeGroupId.cs`
- `src2/EconomyCraftingFishingLoot/Content/RecipeGroupDefinition.cs`
- `src2/EconomyCraftingFishingLoot/Content/RecipeGroupCatalog.cs`
- `src2/EconomyCraftingFishingLoot/Terraria.EconomyCraftingFishingLoot.csproj`
- `src2/EconomyCraftingFishingLootVerification/Program.cs`
- `src2/EconomyCraftingFishingLootVerification/Terraria.EconomyCraftingFishingLootVerification.csproj`

`RecipeGroupDefinition` validates IDs and snapshots item membership. `RecipeGroupCatalog` builds immutable indexes by group ID and fake item ID, rejects duplicate keys, and exposes pure lookup and membership queries. It has no external I/O, randomness, inventory writes, network writes, or persistence writes.

Verification evidence:

- Red build before implementation: serial wrapper build exited `1`, with 0 warnings and 1 error (`CS0234`, missing `NLTX.EconomyCraftingFishingLoot` namespace).
- Green build: serial wrapper build exited `0`, with 0 warnings and 0 errors. Artifacts are under `Build/bin/Terraria.EconomyCraftingFishingLoot/Debug/net10.0/` and `Build/bin/Terraria.EconomyCraftingFishingLootVerification/Debug/net10.0/`.
- Focused no-build verifier exited `0` and printed `P10 verifier passed.`.

No behavior-equivalence, network closure, persistence closure, or cross-partition owner verification has been performed.

## Integration Handoff

P10 hands off the following proposed contracts to integration review:

- `ShopInventorySlots`, `TravelShopCatalog`, and `AnglerQuestState` ownership and network/persistence shape.
- `CraftingRequestCommand`, reservation/consumption port, queue retry policy, and missing remote deserialization behavior.
- `FishingResultDecision`, catch attribution, result commit port, and quest completion ownership.
- `DropAttemptResult`, chain contracts, result commit port, simulation projection, and NPC/world-item spawn ownership.
- Item commerce/economy/use definitions versus item instance/inventory ownership.
- Durable IDs, network IDs, snapshot versions, idempotency keys, failure ownership, and scheduler order.

All handoff items remain `crossSubsystemOwner: integration-review`. This document does not claim C# migration, compilation, tests, runtime behavior equivalence, network closure, or persistence closure.

## Implementation Checkpoint 2: proposed.RecipeDefinition (source saved; verification pending)

Implemented source and focused-test files under the allowed `src2` root:

- `src2/EconomyCraftingFishingLoot/Content/RecipeId.cs`
- `src2/EconomyCraftingFishingLoot/Content/RecipeIngredientDefinition.cs`
- `src2/EconomyCraftingFishingLoot/Content/RecipeResultDefinition.cs`
- `src2/EconomyCraftingFishingLoot/Content/RecipeConditionKind.cs`
- `src2/EconomyCraftingFishingLoot/Content/RecipeConditionDefinition.cs`
- `src2/EconomyCraftingFishingLoot/Content/RecipeDefinition.cs`
- `src2/EconomyCraftingFishingLoot/Content/RecipeQualificationContext.cs`
- `src2/EconomyCraftingFishingLoot/Content/RecipeQualificationFailureReason.cs`
- `src2/EconomyCraftingFishingLoot/Content/RecipeQualificationResult.cs`
- `src2/EconomyCraftingFishingLoot/Content/RecipeQualificationQuery.cs`
- `src2/EconomyCraftingFishingLootVerification/Program.cs`

The definition snapshots ingredients, result data, shimmer results, and recipe conditions into immutable collections. The qualification query checks ordinary item materials and recipe-group materials, then required tiles and conditions, returning a typed failure result without mutating the supplied context or performing external I/O. Inventory consumption, recipe runtime identity, network/persistence snapshots, and cross-partition ownership remain deferred to integration review.

Focused tests added for input snapshots, ordinary and recipe-group material qualification, missing-material precedence, required-tile and condition rejection, and repeated pure evaluation.

Verification evidence:

- Red build before the implementation: serial wrapper build exited `1`, with 0 warnings and 63 errors caused by the intentionally missing recipe-definition API.
- Fresh green build: not yet established. The latest serial precheck timed out with exit `2` because the unrelated active MSBuild node `PID=2776` remained owner-unclear; it was not terminated.
- Focused no-build verifier after this implementation: not yet run.
- Existing `Build/bin` DLL timestamps are not treated as proof of this source version because no complete post-implementation build output was captured.

This checkpoint does not mark `proposed.RecipeDefinition` completed or verified. The runner remains terminal `completed`; no ledger or lock operation was performed.

## Implementation Checkpoint 3: proposed.RecipeDefinition and proposed.ItemEconomyAndUseDefinitions

The previous source-save checkpoint is superseded by the recorded verification evidence below. The
following `src2` files are implemented and remain isolated from production `src`:

- `src2/EconomyCraftingFishingLoot/Content/RecipeId.cs`
- `src2/EconomyCraftingFishingLoot/Content/RecipeIngredientDefinition.cs`
- `src2/EconomyCraftingFishingLoot/Content/RecipeResultDefinition.cs`
- `src2/EconomyCraftingFishingLoot/Content/RecipeConditionKind.cs`
- `src2/EconomyCraftingFishingLoot/Content/RecipeConditionDefinition.cs`
- `src2/EconomyCraftingFishingLoot/Content/RecipeDefinition.cs`
- `src2/EconomyCraftingFishingLoot/Content/RecipeQualificationContext.cs`
- `src2/EconomyCraftingFishingLoot/Content/RecipeQualificationFailureReason.cs`
- `src2/EconomyCraftingFishingLoot/Content/RecipeQualificationResult.cs`
- `src2/EconomyCraftingFishingLoot/Content/RecipeQualificationQuery.cs`
- `src2/EconomyCraftingFishingLoot/Content/ItemCurrencyEconomyDefinition.cs`
- `src2/EconomyCraftingFishingLoot/Content/ItemEventPricingDefinition.cs`
- `src2/EconomyCraftingFishingLoot/Content/ItemPickupTimingDefinition.cs`
- `src2/EconomyCraftingFishingLoot/Content/ItemBuffDurationDefinition.cs`
- `src2/EconomyCraftingFishingLoot/Content/ItemStackAndUseDelayDefinition.cs`
- `src2/EconomyCraftingFishingLoot/Content/ItemPlacementAndPickupPolicyDefinition.cs`
- `src2/EconomyCraftingFishingLoot/Content/ItemUseTimingConsumptionDefinition.cs`
- `src2/EconomyCraftingFishingLootVerification/Program.cs`

Dependency impact: these are immutable content definitions and a pure qualification query. They do
not own inventory, currency mutation, item instances, network transport, persistence, world-item
spawning, or scheduler order. The item definitions preserve the source constants and the verifier
covers the recipe qualification and item economy/use snapshots.

Verification evidence:

- Item-economy/use red build before implementation: serial wrapper build exited `1`, with 0 warnings
  and 7 expected missing-definition errors.
- Green serial build of
  `src2/EconomyCraftingFishingLootVerification/Terraria.EconomyCraftingFishingLootVerification.csproj`
  exited `0`, with 0 warnings and 0 errors.
- The verified artifacts were
  `Build/bin/Terraria.EconomyCraftingFishingLoot/Debug/net10.0/Terraria.EconomyCraftingFishingLoot.dll`
  and
  `Build/bin/Terraria.EconomyCraftingFishingLootVerification/Debug/net10.0/Terraria.EconomyCraftingFishingLootVerification.dll`.
- The serial no-build verifier exited `0` and printed `P10 verifier passed.`

This checkpoint does not claim behavior equivalence, network or persistence closure, runtime
recipe identity ownership, or a final cross-partition owner. `proposed.FishingConditionAndRarityCatalogs`
is the next active component.

## Implementation Checkpoint 4: proposed.FishingConditionAndRarityCatalogs

Implemented source and focused verifier files under the allowed `src2` root:

- `src2/EconomyCraftingFishingLoot/Fishing/IFishingRandomSource.cs`
- `src2/EconomyCraftingFishingLoot/Fishing/FishingConditionEvaluationContext.cs`
- `src2/EconomyCraftingFishingLoot/Fishing/FishingRarityPredicate.cs`
- `src2/EconomyCraftingFishingLoot/Content/FishingConditionDisplayMetadata.cs`
- `src2/EconomyCraftingFishingLoot/Content/FishingConditionDelegateDefinition.cs`
- `src2/EconomyCraftingFishingLoot/Content/FishingConditionCatalog.cs`
- `src2/EconomyCraftingFishingLoot/Content/FishingQuestConditionDefinition.cs`
- `src2/EconomyCraftingFishingLoot/Content/FishingRarityConditionDefinition.cs`
- `src2/EconomyCraftingFishingLoot/Content/FishingRarityCatalog.cs`
- `src2/EconomyCraftingFishingLootVerification/Program.cs`

The implementation converts the current condition and rarity source fields into immutable scalar
attempt/world snapshots, explicit random-source input, delegate-backed definitions, and duplicate-
rejecting catalogs. The default condition catalog covers the evidenced early/hard mode, junk, crate,
enemy, and combat-book conditions. The default rarity catalog preserves the eight evidenced rarity
predicates, visual frequencies, and `HackedIsAny` marker. Quest-fish definitions retain the normal
and remix checked type separately. No catalog owns a live `Player`, `NPC`, random implementation,
inventory, currency, network, persistence, or world result.

Dependency impact: this batch is consumed by later fishing environment and drop-resolution queries.
It does not implement the 31 environment-predicate members, fish-drop resolution, quest ownership,
or result commit ports. The random interface is an explicit proposed boundary and remains
`crossSubsystemOwner: integration-review` until integration assigns its owner.

Verification evidence:

- The focused tests were added before production definitions. A red compiler result was not run
  because an unrelated checkout-wide MSBuild critical section was active at that point.
- Serial green build command:
  `pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1` with `build
  .\src2\EconomyCraftingFishingLootVerification\Terraria.EconomyCraftingFishingLootVerification.csproj
  -m:1 -nr:false -p:UseSharedCompilation=false -p:MSBuildNodeReuse=false
  -p:BuildInParallel=false`.
- Green build exited `0`, with 0 warnings and 0 errors.
- Verified artifacts:
  `Build/bin/Terraria.EconomyCraftingFishingLoot/Debug/net10.0/Terraria.EconomyCraftingFishingLoot.dll`
  and
  `Build/bin/Terraria.EconomyCraftingFishingLootVerification/Debug/net10.0/Terraria.EconomyCraftingFishingLootVerification.dll`.
- Serial no-build verifier command used the same wrapper with `run --project
  .\src2\EconomyCraftingFishingLootVerification\Terraria.EconomyCraftingFishingLootVerification.csproj
  --no-build --no-restore -m:1 -nr:false -p:UseSharedCompilation=false
  -p:MSBuildNodeReuse=false -p:BuildInParallel=false`.
- The no-build verifier exited `0` and printed `P10 verifier passed.`

This checkpoint does not claim behavior equivalence for Version4 methods that are empty or
incomplete, environment predicate coverage, network/persistence closure, or final cross-partition
ownership. `proposed.FishingEnvironmentPredicates` is the next active component.

## Implementation Checkpoint 5: proposed.FishingEnvironmentPredicates

Implemented source and focused verifier files under the allowed `src2` root:

- `src2/EconomyCraftingFishingLoot/Fishing/FishingFluidPredicate.cs`
- `src2/EconomyCraftingFishingLoot/Fishing/FishingBiomePredicate.cs`
- `src2/EconomyCraftingFishingLoot/Fishing/FishingDepthPredicate.cs`
- `src2/EconomyCraftingFishingLoot/Fishing/FishingWorldPredicate.cs`
- `src2/EconomyCraftingFishingLoot/Fishing/FishingEventPredicate.cs`
- `src2/EconomyCraftingFishingLoot/Fishing/FishingConditionEvaluationContext.cs`
- `src2/EconomyCraftingFishingLootVerification/Program.cs`

The five pure predicate groups cover all 31 source members in
`SharedFishingEnvironmentPredicateState`: liquid/capability checks, player and rolled-biome
checks, height bands, rock-layer/ocean/water/world checks, and the blood-moon event check. The
context now carries `rockLayerY` and nullable `IsOriginalOcean` as explicit inputs because
Version4 reads `Main.rockLayer` and an incomplete `IsOriginalOcean` helper; the implementation does
not access those global or incomplete sources. Missing external snapshot values return false rather
than being invented.

Dependency impact: these predicates consume the condition context and are read by later fish-drop
and fishing-attempt queries. They do not consume RNG, mutate catalogs, write player/world state,
spawn items, or own event/world adapters. The rock-layer and original-ocean input adapters remain
`crossSubsystemOwner: integration-review`.

Verification evidence:

- Focused tests were added before production predicates. A red compiler result was not captured
  because an unrelated checkout-wide MSBuild critical section was active while the tests were added.
- Serial wrapper build of
  `src2/EconomyCraftingFishingLootVerification/Terraria.EconomyCraftingFishingLootVerification.csproj`
  exited `0`, with 0 warnings and 0 errors.
- Verified artifacts remain under
  `Build/bin/Terraria.EconomyCraftingFishingLoot/Debug/net10.0/` and
  `Build/bin/Terraria.EconomyCraftingFishingLootVerification/Debug/net10.0/`.
- Serial wrapper no-build verifier exited `0` and printed `P10 verifier passed.`; the wrapper
  reported waiting for the serialized dotnet mutex before running, then completed successfully.

This checkpoint does not claim behavior equivalence for the incomplete Version4 ocean helper,
network/persistence closure, or final cross-partition ownership. `proposed.FishingDropRules` is the
next active component.

## Implementation Checkpoint 6: proposed.FishingDropRules

Implemented source and focused verifier files under the allowed `src2` root:

- `src2/EconomyCraftingFishingLoot/Content/IFishingConditionDefinition.cs`
- `src2/EconomyCraftingFishingLoot/Content/FishingPossibilityEntry.cs`
- `src2/EconomyCraftingFishingLoot/Content/FishingDropRuleDefinition.cs`
- `src2/EconomyCraftingFishingLoot/Content/FishingDropRuleCatalog.cs`
- `src2/EconomyCraftingFishingLoot/Content/FishingConditionDelegateDefinition.cs`
- `src2/EconomyCraftingFishingLoot/Content/FishingQuestConditionDefinition.cs`
- `src2/EconomyCraftingFishingLootVerification/Program.cs`

The rule definition snapshots possible item IDs, numerator/denominator, a read-only condition
collection, and a rarity definition. `IFishingConditionDefinition` is the minimal pure query port
shared by delegate and quest-fish condition definitions; it does not expose or own Version4
`AFishingCondition` instances. `FishingDropRuleCatalog` preserves registration order and rejects
null entries. `FishingPossibilityEntry` preserves item type and frequency without becoming a
runtime entity or inventory item.

Dependency impact: the batch supplies definitions to later probability/chain resolution. It does
not consume random values, mutate a catalog, spawn items, alter inventory/currency, or commit a
fishing result. Result ownership, retry/idempotency, and scheduler order remain
`crossSubsystemOwner: integration-review`.

Verification evidence:

- The focused verifier was extended to cover candidate/order snapshots, chance validation, common
  condition evaluation, rarity attachment, and possibility values. A separate red compiler result
  was not captured for this extension.
- Serial wrapper build of
  `src2/EconomyCraftingFishingLootVerification/Terraria.EconomyCraftingFishingLootVerification.csproj`
  exited `0`, with 0 warnings and 0 errors.
- Verified artifacts were written under
  `Build/bin/Terraria.EconomyCraftingFishingLoot/Debug/net10.0/` and
  `Build/bin/Terraria.EconomyCraftingFishingLootVerification/Debug/net10.0/`.
- Serial wrapper no-build verifier exited `0` and printed `P10 verifier passed.` after waiting for
  the serialized dotnet mutex.

This checkpoint does not claim probability-roll equivalence, chain behavior, result commit,
network/persistence closure, or final cross-partition ownership. `proposed.DropRuleResolutionAndChains`
is the next active component.

## Implementation Checkpoint 7: proposed.DropRuleResolutionAndChains

Implemented source and focused-verifier changes under the allowed `src2` root:

- `src2/EconomyCraftingFishingLoot/Loot/DropAttemptResultState.cs`
- `src2/EconomyCraftingFishingLoot/Loot/DropAttemptResult.cs`
- `src2/EconomyCraftingFishingLoot/Loot/DropRateProjection.cs`
- `src2/EconomyCraftingFishingLoot/Loot/DropRateChainFeed.cs`
- `src2/EconomyCraftingFishingLoot/Loot/DropConditionBranchKind.cs`
- `src2/EconomyCraftingFishingLoot/Loot/DropConditionBranchRuntime.cs`
- `src2/EconomyCraftingFishingLoot/Content/DropRuleCatalog.cs`
- `src2/EconomyCraftingFishingLoot/Loot/DropRuleResolver.cs`
- `src2/EconomyCraftingFishingLoot/Loot/DropChainVisibilityPolicy.cs`
- `src2/EconomyCraftingFishingLoot/Loot/DropChainTriggerKind.cs`
- `src2/EconomyCraftingFishingLoot/Loot/DropRuleChainContract.cs`
- `src2/EconomyCraftingFishingLoot/Loot/DropChainAttemptContract.cs`
- `src2/EconomyCraftingFishingLoot/Content/DropRuleChainDefinition.cs`
- `src2/EconomyCraftingFishingLoot/Content/DropModeOptionSelector.cs`
- `src2/EconomyCraftingFishingLoot/Loot/DropOptionsWithoutRepeatsState.cs`
- `src2/EconomyCraftingFishingLoot/Content/DropItemOptionSelector.cs`
- `src2/EconomyCraftingFishingLoot/Content/DropRuleOptionSelector.cs`
- `src2/EconomyCraftingFishingLoot/Loot/DropRuleResolutionQuery.cs`
- `src2/EconomyCraftingFishingLootVerification/Program.cs`

The implementation keeps drop evaluation as a pre-commit result boundary. It snapshots the rule
catalog and resolves global entries before NPC entries while de-duplicating NPC net IDs. Pure
drop-rate projections and chain feeds preserve condition composition and probability multipliers;
failed-roll and successful chains use `1 - personalDropRate` and `personalDropRate` respectively,
and loot-report visibility is independent of trigger state. Common-drop resolution uses an explicit
`IDropRandomSource`, consumes one chance roll per attempt, applies the documented extra rerolls, and
does not consume randomness when a condition branch fails. Option selectors and temporary
no-repeat state also use explicit randomness. The mechanical-boss condition remains an explicit
non-evaluable integration seam. No result creates an item, changes inventory/currency, resets NPC
money, spawns a world entity, sends a network message, writes persistence, or owns retry/idempotency.

Debugging evidence recorded during this checkpoint:

- The first post-source serial build exited `1`, with 0 warnings and 1 `CS1739` error for the
  verifier's `hideLootReport` named argument; the constructor boundary was corrected.
- The first no-build run then reproduced a deterministic `MoveToImmutable` capacity exception in
  `DropRuleResolver.Resolve`; replacing that operation with a capacity-safe immutable snapshot
  fixed the root cause.

Final verification evidence:

- Before each compile-capable command, no active compile-capable `dotnet.exe` or `csc.exe` process
  was present.
- Serial green build command from the repository root:
  `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\src2\EconomyCraftingFishingLootVerification\Terraria.EconomyCraftingFishingLootVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`.
- The build exited `0`, with 0 warnings and 0 errors.
- Verified artifacts exist at:
  `D:\TRbackup\NLTX\Build\bin\Terraria.EconomyCraftingFishingLoot\Debug\net10.0\Terraria.EconomyCraftingFishingLoot.dll`
  and
  `D:\TRbackup\NLTX\Build\bin\Terraria.EconomyCraftingFishingLootVerification\Debug\net10.0\Terraria.EconomyCraftingFishingLootVerification.dll`.
- Serial no-build verifier command:
  `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('run', '--project', '.\src2\EconomyCraftingFishingLootVerification\Terraria.EconomyCraftingFishingLootVerification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`.
- The verifier exited `0` and printed `P10 verifier passed.`

This checkpoint does not claim Version4 behavior equivalence, network or persistence closure,
result delivery ownership, retry/idempotency closure, scheduler ordering, or a final
cross-partition owner. `proposed.CommerceAndShopState` is the next active component.

## Implementation Checkpoint 8: proposed.CommerceAndShopState

Implemented source and focused-verifier changes under the allowed `src2` root:

- `src2/EconomyCraftingFishingLoot/Commerce/ShopInventorySlotsComponent.cs`
- `src2/EconomyCraftingFishingLoot/Commerce/TravelShopCatalogState.cs`
- `src2/EconomyCraftingFishingLoot/Fishing/AnglerQuestStateComponent.cs`
- `src2/EconomyCraftingFishingLoot/Content/ItemCommerceDefinition.cs`
- `src2/EconomyCraftingFishingLoot/Commerce/SellbackMemoryEntry.cs`
- `src2/EconomyCraftingFishingLoot/Commerce/SellbackMemoryState.cs`
- `src2/EconomyCraftingFishingLoot/Commerce/ShopPricePolicyDefinition.cs`
- `src2/EconomyCraftingFishingLoot/Commerce/ShopMoodEvaluationContext.cs`
- `src2/EconomyCraftingFishingLoot/Commerce/ShopPersonalityCatalog.cs`
- `src2/EconomyCraftingFishingLoot/Commerce/ShopBiomeModifierCatalog.cs`
- `src2/EconomyCraftingFishingLoot/Commerce/ShopMoodWeightsDefinition.cs`
- `src2/EconomyCraftingFishingLoot/Commerce/ShoppingSettingsProjection.cs`
- `src2/EconomyCraftingFishingLootVerification/Program.cs`

The implementation separates shop slot contents, the fixed 40-slot travel-shop catalog, angler
quest state, item commerce flags/prices, sellback memo state, pricing constants, mood input
snapshots, personality/biome key catalogs, mood weights, and the `NotInShop` shopping projection.
Collections are snapshotted and duplicate/invalid keys are rejected. Angler completion is a pure
value-state transition. Live `Terraria.Player`, `Terraria.NPC`, `Chest`, `PersonalityDatabase`, and
currency/inventory objects are not stored. Version4 `ShopHelper.ProcessMood` is empty, so this
checkpoint deliberately exposes mood inputs and source constants without claiming mood behavior
equivalence or implementing a hidden fallback algorithm. No shop transaction, currency change,
inventory write, quest commit, network message, persistence write, or scheduler ownership is
implemented.

Verification evidence:

- TDD red build before the commerce source existed exited `1`, with 0 warnings and the expected
  `CS0234` missing `NLTX.EconomyCraftingFishingLoot.Commerce` namespace error.
- Before each compile-capable command, no active compile-capable `dotnet.exe` or `csc.exe` process
  was present.
- Serial green build command from the repository root:
  `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\src2\EconomyCraftingFishingLootVerification\Terraria.EconomyCraftingFishingLootVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`.
- The build exited `0`, with 0 warnings and 0 errors.
- Verified artifacts exist at:
  `D:\TRbackup\NLTX\Build\bin\Terraria.EconomyCraftingFishingLoot\Debug\net10.0\Terraria.EconomyCraftingFishingLoot.dll`
  and
  `D:\TRbackup\NLTX\Build\bin\Terraria.EconomyCraftingFishingLootVerification\Debug\net10.0\Terraria.EconomyCraftingFishingLootVerification.dll`.
- Serial no-build verifier command:
  `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('run', '--project', '.\src2\EconomyCraftingFishingLootVerification\Terraria.EconomyCraftingFishingLootVerification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`.
- The verifier exited `0` and printed `P10 verifier passed.`

This checkpoint does not claim Version4 behavior equivalence for the empty shop-mood method,
network/persistence closure, final inventory/currency/quest owner, transaction retry/idempotency,
or cross-partition scheduler order. `proposed.CraftingRequestBoundary` is the next active
component.

## Implementation Checkpoint 9: proposed.CraftingRequestBoundary

Implemented source and focused-verifier changes under the allowed `src2` root:

- `src2/EconomyCraftingFishingLoot/Crafting/CraftingItemSnapshot.cs`
- `src2/EconomyCraftingFishingLoot/Crafting/CraftingIngredientRequest.cs`
- `src2/EconomyCraftingFishingLoot/Crafting/CraftingRequestCommand.cs`
- `src2/EconomyCraftingFishingLoot/Crafting/CraftingRequestQueueState.cs`
- `src2/EconomyCraftingFishingLoot/Crafting/RecipeCraftingRuntimeState.cs`
- `src2/EconomyCraftingFishingLootVerification/Program.cs`

The request boundary replaces Version4's live `Recipe`, `Item`, and list references with a stable
request identity, immutable item/ingredient snapshots, the recipe definition reference, and an
explicit quick-craft flag. `CraftingRequestQueueState` owns FIFO pending requests and the readiness
query. `RecipeCraftingRuntimeState` snapshots owned-item and recipe-chest counts and exposes pure
lookup totals; it does not read Version4 static dictionaries or hold `Chest` objects. No network
deserialization, inventory reservation/consumption, currency write, craft-result spawn, persistence
write, retry policy, or cross-partition owner is claimed. Version4's
`NetCraftingRequestsModule.Deserialize` remains an evidence gap because the source body is empty.

Verification evidence:

- TDD red build before the crafting source existed exited `1`, with 0 warnings and the expected
  `CS0234` missing `NLTX.EconomyCraftingFishingLoot.Crafting` namespace error.
- Before each compile-capable command, no active compile-capable `dotnet.exe` or `csc.exe` process
  was present.
- Serial green build command from the repository root:
  `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\src2\EconomyCraftingFishingLootVerification\Terraria.EconomyCraftingFishingLootVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`.
- The build exited `0`, with 0 warnings and 0 errors.
- Verified artifacts exist at:
  `D:\TRbackup\NLTX\Build\bin\Terraria.EconomyCraftingFishingLoot\Debug\net10.0\Terraria.EconomyCraftingFishingLoot.dll`
  and
  `D:\TRbackup\NLTX\Build\bin\Terraria.EconomyCraftingFishingLootVerification\Debug\net10.0\Terraria.EconomyCraftingFishingLootVerification.dll`.
- Serial no-build verifier command:
  `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('run', '--project', '.\src2\EconomyCraftingFishingLootVerification\Terraria.EconomyCraftingFishingLootVerification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`.
- The verifier exited `0` and printed `P10 verifier passed.`

This checkpoint does not claim remote craft protocol closure, inventory/currency ownership,
behavior equivalence, network/persistence schemas, retry/idempotency, or scheduler order.
`proposed.FishingAttemptResolution` is the next active component.

## Implementation Checkpoint 10: proposed.FishingAttemptResolution

Implemented source and focused-verifier changes under the allowed `src2` root:

- `src2/EconomyCraftingFishingLoot/Fishing/FishingAttemptConditionSnapshot.cs`
- `src2/EconomyCraftingFishingLoot/Fishing/FishingAttemptLocationContext.cs`
- `src2/EconomyCraftingFishingLoot/Fishing/FishingAttemptResolutionQuery.cs`
- `src2/EconomyCraftingFishingLoot/Fishing/FishingEnvironmentSnapshot.cs`
- `src2/EconomyCraftingFishingLoot/Fishing/FishingPowerSnapshot.cs`
- `src2/EconomyCraftingFishingLoot/Fishing/FishingResultDecision.cs`
- `src2/EconomyCraftingFishingLoot/Fishing/FishingRollClassification.cs`
- `src2/EconomyCraftingFishingLoot/Fishing/FishingWorldPredicateSnapshot.cs`
- `src2/EconomyCraftingFishingLoot/Fishing/PlayerFishingInputSnapshot.cs`
- `src2/EconomyCraftingFishingLoot/Fishing/PlayerFishingLevelQuery.cs`
- `src2/EconomyCraftingFishingLoot/Fishing/PlayerFishingLevelQueryResult.cs`
- `src2/EconomyCraftingFishingLootVerification/Program.cs`

The batch separates one-attempt location, environment, power, world-condition, and player-input
snapshots from the pure resolution query. Snapshot constructors defensively copy caller-owned
collections and preserve explicit derived fishing-level values instead of inventing missing
Version4 formulas. Resolution evaluates conditions before consuming randomness, uses only the
injected RNG for chance and candidate selection, and returns a decision-only result. It does not
consume bait, mutate inventory or currency, update quest state, spawn items or NPCs, send network
messages, write persistence, or choose a final result owner.

Verification evidence:

- The serial green build command from the repository root was:
  `pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\\src2\\EconomyCraftingFishingLootVerification\\Terraria.EconomyCraftingFishingLootVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`.
- The build exited `0`, with 0 warnings and 0 errors.
- Verified artifacts exist at:
  `D:\TRbackup\NLTX\Build\bin\Terraria.EconomyCraftingFishingLoot\Debug\net10.0\Terraria.EconomyCraftingFishingLoot.dll` and
  `D:\TRbackup\NLTX\Build\bin\Terraria.EconomyCraftingFishingLootVerification\Debug\net10.0\Terraria.EconomyCraftingFishingLootVerification.dll`.
- The serial no-build verifier command was:
  `pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('run', '--project', '.\\src2\\EconomyCraftingFishingLootVerification\\Terraria.EconomyCraftingFishingLootVerification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`.
- The verifier exited `0` and printed `P10 verifier passed.`

This checkpoint does not claim Version4 behavior equivalence, catch delivery, network or
persistence closure, retry/idempotency closure, scheduler ordering, or a final cross-partition
owner. `proposed.LootSimulationAndAttribution` is the next active component.

## Implementation Checkpoint 11: proposed.LootSimulationAndAttribution

Implemented source and focused-verifier changes under the allowed `src2` root:

- `src2/EconomyCraftingFishingLoot/Loot/LootSimulationCounterProjection.cs`
- `src2/EconomyCraftingFishingLoot/Loot/LootSimulationContext.cs`
- `src2/EconomyCraftingFishingLoot/Loot/DropSourceKind.cs`
- `src2/EconomyCraftingFishingLoot/Loot/DropSourceAttribution.cs`
- `src2/EconomyCraftingFishingLoot/Fishing/Adapters/FishingCatchAttributionAdapter.cs`
- `src2/EconomyCraftingFishingLoot/Fishing/ChumFrameCache.cs`
- `src2/EconomyCraftingFishingLootVerification/Program.cs`

The batch implements the isolated simulation and attribution boundary. Normal and expert loot
counts are defensively snapshotted and updated through value-state projections. The simulation
context records explicit player/time/position/mode keys and references the counter projection;
it does not retain live `Player` or `NPC` objects. `DropSourceAttribution` records only a typed
source kind and source key, and `FishingCatchAttributionAdapter` maps fished-out attribution to
that value without taking entity ownership. `ChumFrameCache` has one writer boundary: pending
counts accumulate for the current frame, `AdvanceFrame` swaps them into the previous snapshot, and
the new pending dictionary is cleared.

The implementation does not run loot rules, spawn entities, mutate inventory/currency, publish
network messages, write persistence, or define retry/idempotency or scheduler ownership. The
available Version4 `SimulatorInfo` and `LootSimulationItemCounter` declarations expose fields but
do not close a complete simulation algorithm, so no behavior-equivalence claim is made.

Verification evidence:

- The TDD red build exited `1`, with 0 warnings and 10 expected missing-type/symbol errors for
  the new simulation, attribution, and chum-cache test references.
- Before the green build, no active compile-capable `dotnet.exe` or `csc.exe` process was present.
- The serial green build command was:
  `pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\\src2\\EconomyCraftingFishingLootVerification\\Terraria.EconomyCraftingFishingLootVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`.
- The build exited `0`, with 0 warnings and 0 errors.
- Verified artifacts exist at:
  `D:\TRbackup\NLTX\Build\bin\Terraria.EconomyCraftingFishingLoot\Debug\net10.0\Terraria.EconomyCraftingFishingLoot.dll` and
  `D:\TRbackup\NLTX\Build\bin\Terraria.EconomyCraftingFishingLootVerification\Debug\net10.0\Terraria.EconomyCraftingFishingLootVerification.dll`.
- The serial no-build verifier command was:
  `pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('run', '--project', '.\\src2\\EconomyCraftingFishingLootVerification\\Terraria.EconomyCraftingFishingLootVerification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`.
- The verifier exited `0` and printed `P10 verifier passed.`

This checkpoint does not claim Version4 behavior equivalence, loot simulation parity, entity
identity ownership, catch/loot result delivery, network or persistence closure, retry/idempotency
closure, scheduler ordering, or a final cross-partition owner. `proposed.IntegrationReview` is the
next active component.

## Implementation Checkpoint 12: P10 source inventory and implementation boundary

Added the static inventory and source-boundary verification files under the allowed `src2` root:

- `src2/EconomyCraftingFishingLootVerification/P10SourceInventoryVerificationResult.cs`
- `src2/EconomyCraftingFishingLootVerification/P10SourceInventoryVerifier.cs`
- `src2/EconomyCraftingFishingLootVerification/Program.cs`

The verifier reads the P10 report and the complete ownership matrix, rejects duplicate or unmapped
source sequences, confirms the expected 321/296/25 inventory counts, checks that every matrix target
maps into `src2`, rejects project references from the isolated P10 production project, and scans the
P10 source tree for forbidden `src`, Version4, and live `using Terraria.` references. This is a
structural and boundary check only; it does not claim behavior equivalence or close any deferred
integration ownership.

Verification evidence:

- The serial green build exited `0`, with 0 warnings and 0 errors, for
  `src2/EconomyCraftingFishingLootVerification/Terraria.EconomyCraftingFishingLootVerification.csproj`.
- The exact serial green build command was:
  `pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\\src2\\EconomyCraftingFishingLootVerification\\Terraria.EconomyCraftingFishingLootVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`.
- Verified artifacts exist at `D:\TRbackup\NLTX\Build\bin\Terraria.EconomyCraftingFishingLoot\Debug\net10.0\Terraria.EconomyCraftingFishingLoot.dll` and
  `D:\TRbackup\NLTX\Build\bin\Terraria.EconomyCraftingFishingLootVerification\Debug\net10.0\Terraria.EconomyCraftingFishingLootVerification.dll`.
- The serial no-build verifier exited `0` and printed `P10 verifier passed.`
- The exact serial no-build verifier command was:
  `pwsh -NoProfile -Command "& '.\\Build\\Tools\\Invoke-SerialDotnet.ps1' -DotnetArguments @('run', '--project', '.\\src2\\EconomyCraftingFishingLootVerification\\Terraria.EconomyCraftingFishingLootVerification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`.

`proposed.IntegrationReview`, `proposed.CSharpMigration`, and
`proposed.BehaviorAndProtocolVerification` remain pending/deferred. The existing evidence gaps and
`crossSubsystemOwner: integration-review` decisions remain in force.

## Component-only Scope Correction Checkpoint 15: production build

The corrected P10 production project was built after the checkout-wide MSBuild critical section
was released. The exact command was:

`pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\src2\EconomyCraftingFishingLoot\Terraria.EconomyCraftingFishingLoot.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`

The serial wrapper exited `0` with `0` warnings and `0` errors. The verified artifact is
`D:\TRbackup\NLTX\Build\bin\Terraria.EconomyCraftingFishingLoot\Debug\net10.0\Terraria.EconomyCraftingFishingLoot.dll`.
The source tree contains exactly the two allowed Component files listed in Checkpoint 14. No
test, verifier, Query, Command, Adapter, Projection, or other non-Component code was created or
retained for this corrected pass. Network, persistence, behavior equivalence, integration owner,
and cross-partition scheduler decisions remain deferred.

## Component-only Scope Correction Checkpoint 16: runner settlement

The active manual session was settled through the non-authoritative runner using the exact session
ID `35daf5b5baa2427fa8b7b778c2bce0f5`. The runner returned `status: completed`, `exitCode: 0`,
and `lockReleased: true`. No further `Claim`, `Handoff`, or `Complete` action is required for
P10.

## Final Component Checkpoint After Handoff

The P10 Handoff continuation is complete for the allowed non-authoritative Component scope. This
handoff did not add any new Component files; the existing P10 Component source remains under
`src2\EconomyCraftingFishingLoot`, and the production `src` tree was not modified. The runner
ledger is terminal `completed` for session `0a3c4113c9d546bf998f7d17339ba9e2`, after transfer from
`367992a8f79f4509b47679635a734aa6` under handoff ID
`P10-handoff-20260912-implementation`.

The serial production/verifier build exited `0` with 0 warnings and 0 errors, the expected
artifacts were verified under `Build\bin`, and the focused no-build verifier exited `0` with
`P10 verifier passed.` This evidence closes only the saved `src2` Component implementation and
its focused structural/behavioral checks. Integration review, C# migration into production,
Version4 behavior equivalence, protocol and persistence closure, cross-partition ownership,
retry/idempotency, and scheduler ordering remain deferred and are not claimed complete.

## Component-only Scope Correction Checkpoint 14

The prior terminal P10 record was safely reset through the non-authoritative runner's `Cleanup`
action with `previousStatus: completed`, then P10 was claimed again through `Claim`. The active
manual session is `35daf5b5baa2427fa8b7b778c2bce0f5`; this document is now bound to that session.

The previous source pass had created non-Component code despite this task's explicit
Component-only boundary. The correction moved 97 non-Component production `.cs` files and the
four-file `EconomyCraftingFishingLootVerification` project to the recoverable temporary
quarantine directory
`C:\Users\shan\AppData\Local\Temp\NLTX-P10-noncomponent-quarantine-c7a95dc97fd64d809cc9a6c84c5634de`.
No files were deleted. The only remaining P10 C# source files are:

- `src2\EconomyCraftingFishingLoot\Commerce\ShopInventorySlotsComponent.cs`
- `src2\EconomyCraftingFishingLoot\Fishing\AnglerQuestStateComponent.cs`

The remaining proposed definitions, states, snapshots, queries, commands, adapters, projections,
contracts, predicates, random ports, integration boundaries, and verification artifacts are
deferred because they are outside the requested Component-only implementation scope. Verification
of the corrected source is pending the checkout-wide serial build critical section; no build or
verifier pass is claimed in this checkpoint.

## Handoff Continuation Checkpoint 13: P10 runner handoff and verification

The failed/manual P10 continuation was transferred through the non-authoritative runner's
`Handoff` action. The previous session was `367992a8f79f4509b47679635a734aa6`, the active session is
`0a3c4113c9d546bf998f7d17339ba9e2`, and the handoff ID is
`P10-handoff-20260912-implementation`. Existing component checkpoints were preserved. This
continuation did not add or modify Component source; `src2CodeModified: no` and
`productionSrcModified: no` for this continuation.

Verification evidence for the handoff continuation:

- The serial wrapper build command was:
  `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('build', '.\src2\EconomyCraftingFishingLootVerification\Terraria.EconomyCraftingFishingLootVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`.
- The build exited `0`, with 0 warnings and 0 errors.
- Verified artifacts exist under `D:\TRbackup\NLTX\Build\bin\Terraria.EconomyCraftingFishingLoot\Debug\net10.0\Terraria.EconomyCraftingFishingLoot.dll` and `D:\TRbackup\NLTX\Build\bin\Terraria.EconomyCraftingFishingLootVerification\Debug\net10.0\Terraria.EconomyCraftingFishingLootVerification.dll`.
- The serial no-build verifier command was:
  `pwsh -NoProfile -Command "& '.\Build\Tools\Invoke-SerialDotnet.ps1' -DotnetArguments @('run', '--project', '.\src2\EconomyCraftingFishingLootVerification\Terraria.EconomyCraftingFishingLootVerification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"`.
- The verifier exited `0` and printed `P10 verifier passed.`

`proposed.IntegrationReview`, `proposed.CSharpMigration`, and
`proposed.BehaviorAndProtocolVerification` remain pending/deferred. The existing evidence gaps and
`crossSubsystemOwner: integration-review` decisions remain in force.
