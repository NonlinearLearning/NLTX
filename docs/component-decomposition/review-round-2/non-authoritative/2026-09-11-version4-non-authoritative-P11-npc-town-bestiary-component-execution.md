# Version4 非权威组件第二轮实施计划：P11 NPC、城镇与图鉴

partitionId: P11
sessionId: c822e256a5e34730bb0ab0ad5a6a094b
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\11-npc-town-bestiary.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P11-npc-town-bestiary-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P11-npc-town-bestiary-component-execution.md
designStatus: proposed
executionStatus: completed
implementationStatus: completed
verificationStatus: independently-verified
completedComponents:
- SharedConditionalDialogueSupport
- SharedTownRoomState
- SharedNpcPersonalityCatalog
- BestiaryCatalogAndEntries
- BestiaryUnlockTracking
- BestiaryFiltersAndSorting
- SharedBestiaryInfoElementState
- SharedBestiaryCollectionProviderState
currentComponent: verification-and-documentation
pendingComponents: []
savedButUnverifiedComponents: []
lastCheckpointUtc: 2026-09-12T08:37:48.2712187Z
previousSessionId: de06dd0a16934d8c92ff67a0130cff30
handoffId: p11-npc-town-bestiary-handoff-20260912-02
handoffStatus: completed
src2CodeModified: yes
src2SourceRoot: D:\TRbackup\NLTX\src2\NpcTownBestiary
src2CSharpFileCount: 106
productionSrcModified: no
evidence-gap:
- 第一轮 P11 public-decomposition 研究报告缺失，不能作为已完成设计输入。
- Version4 条件对话 clear/consume、Lucy 初始化/触发、Bestiary NPC population、BestiaryEntry 工厂、InfoElement UI 和 NetBestiaryModule.Deserialize 未闭合。
- TownRoomManager 使用 NPC type key；当前 NLTX 有重复 registry/assignment 草稿，key mode、唯一 writer 和持久化迁移未裁决。
- Personality/biome/profile 资源边界、HelperInfo 实体查询端口、ItemDropDatabase drop merge、NPC stats refresh 和 UICollection policy 仍是 partial/evidence-gap。
- Bestiary credit ID、NpcNetId、PersistentEntityId、NPC type、snapshot、network payload、WorldFile 版本和调度顺序的最终 owner 为 crossSubsystemOwner: integration-review。
- src2 已保存上述 8 个组件范围的候选实现；最新 focused verifier 已覆盖 8 个场景并通过，但该证据只证明 src2 候选实现的局部契约，不证明当前生产运行时行为等价、网络/存档闭合或跨分区 owner 已裁决。
blocking-decision:
- TownRoomRegistryComponent、TownHousingRegistry、NpcHousingAssignmentComponent、TownHousingRelationComponent 的唯一状态 owner、NPC type/instance key mode 和 WorldFile migration format。
- Personality catalog、Town NPC profile catalog、ContentPresentationIndex/NpcDefinitionCatalog 的边界及 HelperInfo 的 Player/NPC/nearby read ports。
- Bestiary content registration、drop merge、filter/sort registration、entry population 和 initialization order。
- Kill/sight/chat authority、Bestiary credit ID 映射、网络 Deserialize、存档版本和 player-join snapshot 协议。
- InfoElement/UICollection snapshot owner、stats refresh source、UI compatibility seam 和 integration System order。
- Conditional dialogue consume owner、Lucy cooldown authority/replication，以及所有跨分区类型、ID、snapshot、network payload 和顺序。
- SharedTownRoomState 的最终 key mode 和 assignment projection owner 仍未裁决；当前 src2 实现只提供 NPC type key 的未验证候选。
- BestiaryUnlockTracking 的持久化版本、网络 Deserialize、player-join authority 和 Bestiary credit ID/NpcNetId 映射仍未验证。

## 1. 实施声明和前置条件

本文件记录 P11 非权威组件的实施顺序、候选实现路径、兼容策略、回滚条件和 verifier 证据。目标类型、命名空间、目录和文件仍为 proposed；候选 C# 已保存到 `D:\TRbackup\NLTX\src2\NpcTownBestiary` 并通过局部 focused verifier。本文件不声称已完成生产迁移、行为等价或网络/存档闭合。

P11 处理输入分区的 8 个叶子范围和 93 条成员。NPC 战斗/死亡、ItemDropDatabase 规则、网络协议格式、WorldFile 总调度、UI 渲染、资源加载、Achievement 副作用和跨分区共享类型的最终 owner 由 `crossSubsystemOwner: integration-review` 裁决。

实施前置条件：

- 只从 Version4 证据和当前 NLTX 的 proposed/partial 草稿建立兼容 fixture；不得把完整参考或 tModLoader 文档当作 Version4 私有行为实现。
- 先确定 Town room key、Bestiary identity mapping、discovery authority 和 snapshot/network owner，再添加跨模块接口。
- 每个步骤先补 focused verifier，再迁移单一边界；未通过时保留旧只读 adapter，禁止旧/new 双写。
- 任何 compile-capable 命令将来必须从仓库根目录经 `Build/Tools/Invoke-SerialDotnet.ps1` 串行执行；本次不执行 compile-capable 命令。

## 2. proposed 目标目录、文件和命名空间

下面的路径只表示后续目标位置，均为 proposed，不代表文件已经存在。

| proposed 目标文件 | proposed 类型/角色 | 依赖边界 |
| --- | --- | --- |
| `src2/Npc/Dialogue/ConditionalDialogueDefinition.cs` | `ConditionalDialogueDefinition`，definition | 稳定 NPC type、definition key、condition port、indicator metadata |
| `src2/Npc/Dialogue/ConditionalDialogueCatalog.cs` | `ConditionalDialogueCatalog`，catalog | 唯一 definition registration owner |
| `src2/Npc/Dialogue/ConditionalDialogueQuery.cs` | `ConditionalDialogueQuery`，Query | 只读 NPC/Player/ItemGroup port |
| `src2/Npc/Dialogue/ConditionalDialogueConsumeCommand.cs` | `ConsumeConditionalDialogueCommand`，Command | NPC entity key、definition key、expected revision；crossSubsystemOwner: integration-review |
| `src2/Npc/Dialogue/ConditionalDialogueConsumeSystem.cs` | `ConditionalDialogueConsumeSystem`，System | 唯一消费 writer、事件输出 |
| `src2/Npc/Dialogue/ConditionalDialogueIndicatorProjection.cs` | `ConditionalDialogueIndicatorProjection`，Projection | UI-neutral indicator view |
| `src2/Npc/Dialogue/ConditionalDialogueItemGroupAdapter.cs` | `ConditionalDialogueItemGroupAdapter`，Adapter | RecipeGroup/ContentSamples 隔离 |
| `src2/Npc/Dialogue/LucyAxeCooldownComponent.cs` | `LucyAxeCooldownComponent`，transient Component | source cooldown；authority unresolved |
| `src2/Npc/Dialogue/LucyAxeCooldownSystem.cs` | `LucyAxeCooldownSystem`，System | explicit WorldTickPort |
| `src2/Town/Rooms/TownRoomRegistryComponent.cs` | `TownRoomRegistryComponent`，world Component | 唯一房屋登记 authority candidate |
| `src2/Town/Rooms/TownRoomAssignmentCommands.cs` | `AssignTownRoomCommand`、`EvictTownResidentCommand`，Commands | command commit boundary |
| `src2/Town/Rooms/TownRoomAssignmentSystem.cs` | `TownRoomAssignmentSystem`，System | registry mutation、revision |
| `src2/Town/Rooms/TownRoomQuery.cs` | `TownRoomQuery`，Query | room/occupant read view |
| `src2/Town/Rooms/TownHousingCompatibilityQuery.cs` | `TownHousingCompatibilityQuery`，Query | housing category read port |
| `src2/Town/Rooms/TownRoomAvailabilityProjection.cs` | `TownRoomAvailabilityProjection`，Projection | derived has-room index |
| `src2/Town/Rooms/TownRoomPersistenceAdapter.cs` | `TownRoomPersistenceAdapter`，Adapter | WorldFile BinaryReader/BinaryWriter |
| `src2/Town/Rooms/TownRoomMutationGateAdapter.cs` | `TownRoomMutationGateAdapter`，Adapter/Port | EntityCreationLock seam |
| `src2/Npc/Personality/NpcPersonalityCatalog.cs` | `NpcPersonalityCatalog`，catalog | immutable profile view |
| `src2/Npc/Personality/NpcPersonalityCatalogBuilder.cs` | `NpcPersonalityCatalogBuilder`，build System | content population |
| `src2/Npc/Personality/NpcPersonalityDefinitions.cs` | `NpcPersonalityDefinition`、`NpcBiomePreference`，definitions | stable affection/biome values |
| `src2/Npc/Personality/NpcPersonalityEvaluationContext.cs` | `NpcPersonalityEvaluationContext`，transient context | entity IDs/read views；crossSubsystemOwner: integration-review |
| `src2/Npc/Personality/NpcPersonalityQuery.cs` | `NpcPersonalityQuery`，Query | pure shopping/personality evaluation |
| `src2/Npc/Personality/TownNpcProfileCatalog.cs` | `TownNpcProfileCatalog`，presentation catalog | asset/variant metadata |
| `src2/Npc/Personality/TownNpcProfileAssetAdapter.cs` | `TownNpcProfileAssetAdapter`，Adapter | asset path/profile third-party boundary |
| `src2/Bestiary/Catalog/BestiaryCatalogComponent.cs` | `BestiaryCatalogComponent`，catalog Component | entries/filter/sort registration refs |
| `src2/Bestiary/Catalog/BestiaryEntryDefinition.cs` | `BestiaryEntryDefinition`，definition | info element/provider refs |
| `src2/Bestiary/Catalog/BestiaryNpcEntryIndex.cs` | `BestiaryNpcEntryIndex`，derived index | NpcNetId -> entry key |
| `src2/Bestiary/Catalog/BestiaryCatalogRegistrationSystem.cs` | `BestiaryCatalogRegistrationSystem`，System | content registration/freeze |
| `src2/Bestiary/Catalog/BestiaryDropMergeAdapter.cs` | `BestiaryDropMergeAdapter`，Adapter | ItemDropDatabase boundary |
| `src2/Bestiary/Unlocks/BestiaryKillCountStateComponent.cs` | `BestiaryKillCountStateComponent`，persistent Component | count by BestiaryCreditId |
| `src2/Bestiary/Unlocks/BestiarySightDiscoveryStateComponent.cs` | `BestiarySightDiscoveryStateComponent`，persistent Component | seen credit IDs |
| `src2/Bestiary/Unlocks/BestiaryChatDiscoveryStateComponent.cs` | `BestiaryChatDiscoveryStateComponent`，persistent Component | chatted credit IDs |
| `src2/Bestiary/Unlocks/BestiaryDiscoverySystem.cs` | `BestiaryDiscoverySystem`，System | kill/chat fact commits |
| `src2/Bestiary/Unlocks/BestiarySightScanSystem.cs` | `BestiarySightScanSystem`，System | sight scan and transient buffer |
| `src2/Bestiary/Unlocks/BestiarySightScanBuffer.cs` | `BestiarySightScanBuffer`，transient state | player bounds and per-scan NpcNetId dedupe |
| `src2/Bestiary/Unlocks/BestiaryUnlockProgressQuery.cs` | `BestiaryUnlockProgressQuery`，Query | progress report |
| `src2/Bestiary/Unlocks/BestiaryUnlockPersistenceAdapter.cs` | `BestiaryUnlockPersistenceAdapter`，Adapter | WorldFile payload |
| `src2/Bestiary/Unlocks/BestiaryPlayerJoinSyncAdapter.cs` | `BestiaryPlayerJoinSyncAdapter`，Adapter | join snapshot |
| `src2/Bestiary/Unlocks/BestiaryDiscoveryNetworkAdapter.cs` | `BestiaryDiscoveryNetworkAdapter`，Adapter | kill/sight/chat network boundary |
| `src2/Bestiary/Filters/BestiaryFilterCatalog.cs` | `BestiaryFilterCatalog`，catalog | filter definitions/metadata |
| `src2/Bestiary/Filters/BestiaryFilterQuery.cs` | `BestiaryFilterQuery`，Query | pure filtering |
| `src2/Bestiary/Filters/BestiarySortCatalog.cs` | `BestiarySortCatalog`，catalog | sort definitions/metadata |
| `src2/Bestiary/Filters/BestiarySortQuery.cs` | `BestiarySortQuery`，Query | pure stable sorting |
| `src2/Bestiary/Info/BestiaryInfoElementState.cs` | `BestiaryInfoElementState`，presentation state | UI-neutral facts |
| `src2/Bestiary/Info/BestiaryStatsRefreshAdapter.cs` | `BestiaryStatsRefreshAdapter`，Adapter | NPC definition/read view -> stats |
| `src2/Bestiary/Info/BestiaryInfoElementProjection.cs` | `BestiaryInfoElementProjection`，Projection | UI adapter input |
| `src2/Bestiary/Collection/BestiaryCollectionProviderDefinitions.cs` | provider definitions and unlock policy, proposed | persistent ID and policy values |
| `src2/Bestiary/Collection/BestiaryCollectionUnlockQuery.cs` | `BestiaryCollectionUnlockQuery`，Query | kill/sight/chat -> UnlockState |
| `src2/Bestiary/Collection/BestiaryUICollectionProjection.cs` | `BestiaryUICollectionProjection`，Projection | immutable client collection snapshot |

The exact namespaces remain proposed and must follow repository ECS organization and naming constraints when implementation is authorized. No `Shared/Components/` directory or generic Manager aggregate is proposed.

## 3. Source-member to proposed target mapping

All source rows below are from the claimed P11 report. The proposed target is a role/file mapping only; none of the targets is implemented.

### 3.1 SharedConditionalDialogueSupport

| Source row | Version4 member | Proposed target file/role |
| ---: | --- | --- |
| 2352 | `ConditionalDialogue.ItemGroups.Ore` | proposed `ConditionalDialogueItemGroupAdapter.cs`, static ItemGroup view |
| 2353 | `ConditionalDialogue.ItemGroups.Bars` | proposed `ConditionalDialogueItemGroupAdapter.cs`, static ItemGroup view |
| 2354 | `ConditionalDialogue.ItemGroups.Anvils` | proposed `ConditionalDialogueItemGroupAdapter.cs`, static ItemGroup view |
| 2355 | `ConditionalDialogue.ItemGroups.Whips` | proposed `ConditionalDialogueItemGroupAdapter.cs`, PostSetup dynamic view |
| 2356 | `ConditionalDialogue.ItemGroups.Mounts` | proposed `ConditionalDialogueItemGroupAdapter.cs`, PostSetup dynamic view |
| 2357 | `ConditionalDialogue._registry` | proposed `ConditionalDialogueCatalog.cs`, registry state |
| 2358 | `ConditionalDialogue.ConditionsMet` | proposed `ConditionalDialogueDefinition.cs`, ConditionPort |
| 2456 | `LucyAxeMessage._messageCooldownsByType` | proposed `LucyAxeCooldownComponent.cs`, RemainingBySource |
| 3849 | `ConditionalDialogue.ShowIndicator` | proposed `ConditionalDialogueIndicatorProjection.cs`, ShowIndicator |

### 3.2 SharedTownRoomState

| Source row | Version4 member | Proposed target file/role |
| ---: | --- | --- |
| 2574 | `TownRoomManager.EntityCreationLock` | proposed `TownRoomMutationGateAdapter.cs`, commit gate |
| 2575 | `TownRoomManager._roomLocationPairs` | proposed `TownRoomRegistryComponent.cs`, AssignedRoomsByResidentKey |
| 2576 | `TownRoomManager._hasRoom` | proposed `TownRoomAvailabilityProjection.cs`, derived index; Query reads it only through a port |

The existing `TownHousingRegistryComponent`, `TownHousingRegistry`, `NpcHousingAssignmentComponent` and `TownHousingRelationComponent` are not mapped as additional writers. They are existing NLTX drafts requiring an integration-review choice of one registry authority and one NPC assignment projection. The proposed compatibility adapter must be read-only during migration; double-write is forbidden.

### 3.3 SharedNpcPersonalityCatalog

| Source row | Version4 member | Proposed target file/role |
| ---: | --- | --- |
| 2161 | `BiomePreference.Affection` | proposed `NpcPersonalityDefinitions.cs`, `NpcBiomePreference.Affection` |
| 2162 | `BiomePreference.Biome` | proposed `NpcPersonalityDefinitions.cs`, stable `BiomeKey` |
| 2163 | `BiomePreferenceListTrait._preferences` | proposed `NpcPersonalityDefinitions.cs`, ordered read-only preferences |
| 2164 | `HelperInfo.player` | proposed `NpcPersonalityEvaluationContext.cs`, PlayerEntityId/read port |
| 2165 | `HelperInfo.npc` | proposed `NpcPersonalityEvaluationContext.cs`, NpcEntityId/read port |
| 2166 | `HelperInfo.NearbyNPCs` | proposed `NpcPersonalityEvaluationContext.cs`, NearbyNpcReadViews |
| 2167 | `HelperInfo.nearbyNPCsByType` | proposed `NpcPersonalityEvaluationContext.cs`, derived type membership view |
| 2168 | `PersonalityDatabase._personalityProfiles` | proposed `NpcPersonalityCatalog.cs`, ProfilesByNpcType |
| 2169 | `PersonalityDatabase._trashEntry` | proposed `NpcPersonalityQuery.cs`, explicit Missing result |
| 2170 | `PersonalityDatabasePopulator._currentDatabase` | proposed `NpcPersonalityCatalogBuilder.cs`, build context only |
| 2171 | `PersonalityProfile.ShopModifiers` | proposed `NpcPersonalityDefinitions.cs`, stable ShopTraits |
| 2567 | `TownNPCProfiles.DefaultNPCFileFolderPath` | proposed `TownNpcProfileAssetAdapter.cs`, DefaultRoot |
| 2568 | `TownNPCProfiles.ShimmeredNPCFileFolderPath` | proposed `TownNpcProfileAssetAdapter.cs`, ShimmeredRoot |
| 2569 | `TownNPCProfiles.CatHeadIDs` | proposed `TownNpcProfileCatalog.cs`, CatHeadIds |
| 2570 | `TownNPCProfiles.DogHeadIDs` | proposed `TownNpcProfileCatalog.cs`, DogHeadIds |
| 2571 | `TownNPCProfiles.BunnyHeadIDs` | proposed `TownNpcProfileCatalog.cs`, BunnyHeadIds |
| 2572 | `TownNPCProfiles._townNPCProfiles` | proposed `TownNpcProfileCatalog.cs`, ProfilesByNpcType |
| 2573 | `TownNPCProfiles.Instance` | proposed `NpcTownContentRegistrationPort`, composition root seam |
| 3828 | `AShoppingBiome.NameKey` | proposed `NpcPersonalityDefinitions.cs`, `ShoppingBiomeDefinition.LocalizationKey` |

### 3.4 BestiaryCatalogAndEntries

| Source row | Version4 member | Proposed target file/role |
| ---: | --- | --- |
| 1357 | `BestiaryDatabase._entries` | proposed `BestiaryCatalogComponent.cs` + `BestiaryEntryDefinition.cs`, entry definitions |
| 1358 | `BestiaryDatabase._filters` | proposed `BestiaryCatalogComponent.cs`, filter registration refs |
| 1359 | `BestiaryDatabase._sortSteps` | proposed `BestiaryCatalogComponent.cs`, sort registration refs |
| 1360 | `BestiaryDatabase._byNpcId` | proposed `BestiaryNpcEntryIndex.cs`, derived NpcNetId index |
| 1361 | `BestiaryDatabase._trashEntry` | proposed `BestiaryEntryDefinition.cs`, explicit NotFound result |
| 1362 | `BestiaryEntry.UIInfoProvider` | proposed `BestiaryEntryDefinition.cs`, CollectionProviderRef |
| 3718 | `BestiaryDatabase.Entries` | proposed `BestiaryCatalogComponent.cs`, read-only catalog query |
| 3719 | `BestiaryEntry.Info` | proposed `BestiaryEntryDefinition.cs`, InfoElementDefinitions |

### 3.5 BestiaryUnlockTracking

| Source row | Version4 member | Proposed target file/role |
| ---: | --- | --- |
| 1365 | `BestiaryUnlockProgressReport.EntriesTotal` | proposed `BestiaryUnlockProgressQuery.cs`, report output |
| 1366 | `BestiaryUnlockProgressReport.CompletionAmountTotal` | proposed `BestiaryUnlockProgressQuery.cs`, report output |
| 1367 | `BestiaryUnlocksTracker.Kills` | proposed `BestiaryKillCountStateComponent.cs` |
| 1368 | `BestiaryUnlocksTracker.Sights` | proposed `BestiarySightDiscoveryStateComponent.cs` |
| 1369 | `BestiaryUnlocksTracker.Chats` | proposed `BestiaryChatDiscoveryStateComponent.cs` |
| 1380 | `NPCKillsTracker._entryCreationLock` | proposed `BestiaryDiscoveryMutationGateAdapter.cs` |
| 1381 | `NPCKillsTracker.POSITIVE_KILL_COUNT_CAP` | proposed `BestiaryKillCountPolicy` in `BestiaryDiscoverySystem.cs` |
| 1382 | `NPCKillsTracker._killCountsByNpcId` | proposed `BestiaryKillCountStateComponent.cs`, CountByBestiaryCreditId |
| 1392 | `NPCWasChatWithTracker._entryCreationLock` | proposed `BestiaryDiscoveryMutationGateAdapter.cs` |
| 1393 | `NPCWasChatWithTracker._chattedWithPlayer` | proposed `BestiaryChatDiscoveryStateComponent.cs`, credit ID set |
| 1394 | `NPCWasNearPlayerTracker._entryCreationLock` | proposed `BestiaryDiscoveryMutationGateAdapter.cs` |
| 1395 | `NPCWasNearPlayerTracker._wasNearPlayer` | proposed `BestiarySightDiscoveryStateComponent.cs`, credit ID set |
| 1396 | `NPCWasNearPlayerTracker._playerHitboxesForBestiary` | proposed `BestiarySightScanBuffer.cs`, transient player bounds |
| 1397 | `NPCWasNearPlayerTracker._wasSeenNearPlayerByNetId` | proposed `BestiarySightScanBuffer.cs`, per-scan NpcNetId dedupe |
| 3720 | `BestiaryUnlockProgressReport.CompletionPercent` | proposed `BestiaryUnlockProgressQuery.cs`, zero-total rule |

### 3.6 BestiaryFiltersAndSorting

| Source row | Version4 member | Proposed target file/role |
| ---: | --- | --- |
| 1374 | `Filters.ByInfoElement._element` | proposed `BestiaryFilterQuery.cs`, ElementKey lookup port |
| 3721 | `Filters.BySearch.ForcedDisplay` | proposed `BestiaryFilterCatalog.cs`, Search metadata true |
| 3722 | `Filters.ByUnlockState.ForcedDisplay` | proposed `BestiaryFilterCatalog.cs`, UnlockState metadata true |
| 3723 | `Filters.ByRareCreature.ForcedDisplay` | proposed `BestiaryFilterCatalog.cs`, RareCreature metadata null |
| 3724 | `Filters.ByBoss.ForcedDisplay` | proposed `BestiaryFilterCatalog.cs`, Boss metadata null |
| 3725 | `Filters.ByInfoElement.ForcedDisplay` | proposed `BestiaryFilterCatalog.cs`, InfoElement metadata null |
| 3732 | `SortingSteps.ByNetId.HiddenFromSortOptions` | proposed `BestiarySortCatalog.cs`, NetId metadata true |
| 3733 | `SortingSteps.ByUnlockState.HiddenFromSortOptions` | proposed `BestiarySortCatalog.cs`, UnlockState metadata true |
| 3734 | `SortingSteps.ByBestiarySortingId.HiddenFromSortOptions` | proposed `BestiarySortCatalog.cs`, SortingId metadata false |
| 3735 | `SortingSteps.ByBestiaryRarity.HiddenFromSortOptions` | proposed `BestiarySortCatalog.cs`, Rarity metadata false |
| 3736 | `SortingSteps.Alphabetical.HiddenFromSortOptions` | proposed `BestiarySortCatalog.cs`, Alphabetical metadata false |
| 3737 | `SortingSteps.ByStat.HiddenFromSortOptions` | proposed `BestiarySortCatalog.cs`, Stat metadata false |

### 3.7 SharedBestiaryInfoElementState

| Source row | Version4 member | Proposed target file/role |
| ---: | --- | --- |
| 1375 | `FlavorTextBestiaryInfoElement._key` | proposed `BestiaryInfoElementState.cs`, FlavorText LocalizationKey |
| 1376 | `ItemDropBestiaryInfoElement._droprateInfo` | proposed `BestiaryInfoElementState.cs`, DropRateView via adapter |
| 1377 | `NamePlateInfoElement._key` | proposed `BestiaryInfoElementState.cs`, NamePlate LocalizationKey |
| 1378 | `NamePlateInfoElement._npcNetId` | proposed `BestiaryInfoElementState.cs`, NamePlate NpcNetId |
| 1379 | `NPCKillCounterInfoElement._instance` | proposed `BestiaryInfoElementProjection.cs`, kill-counter read port |
| 1383 | `NPCPortraitInfoElement._filledStarsCount` | proposed `BestiaryInfoElementState.cs`, FilledRarityStars |
| 1384 | `NPCStatsReportInfoElement.NpcId` | proposed `BestiaryInfoElementState.cs`, Stats NpcNetId |
| 1385 | `NPCStatsReportInfoElement.Damage` | proposed `BestiaryInfoElementState.cs`, Stats Damage |
| 1386 | `NPCStatsReportInfoElement.LifeMax` | proposed `BestiaryInfoElementState.cs`, Stats LifeMax |
| 1387 | `NPCStatsReportInfoElement.MonetaryValue` | proposed `BestiaryInfoElementState.cs`, Stats MonetaryValue |
| 1388 | `NPCStatsReportInfoElement.Defense` | proposed `BestiaryInfoElementState.cs`, Stats Defense |
| 1389 | `NPCStatsReportInfoElement.KnockbackResist` | proposed `BestiaryInfoElementState.cs`, Stats KnockbackResist |
| 1390 | `NPCStatsReportInfoElement._instance` | proposed `BestiaryStatsRefreshAdapter.cs`, NpcReadView input |
| 1391 | `NPCStatsReportInfoElement.HideStats` | proposed `BestiaryInfoElementState.cs`, Stats HideStats |
| 3729 | `NPCNetIdBestiaryInfoElement.NetId` | proposed `BestiaryInfoElementState.cs`, NpcIdentityView |
| 3730 | `NPCNetIdBestiaryInfoElement.BestiaryDisplayIndex` | proposed `BestiaryInfoElementProjection.cs`, DisplayIndex query |
| 3731 | `RareSpawnBestiaryInfoElement.RarityLevel` | proposed `BestiaryInfoElementState.cs`, RarityView |

### 3.8 SharedBestiaryCollectionProviderState

| Source row | Version4 member | Proposed target file/role |
| ---: | --- | --- |
| 1363 | `BestiaryUICollectionInfo.OwnerEntry` | proposed `BestiaryUICollectionProjection.cs`, stable OwnerEntryKey |
| 1364 | `BestiaryUICollectionInfo.UnlockState` | proposed `BestiaryUICollectionProjection.cs`, derived UnlockState |
| 1370 | `CommonEnemyUICollectionInfoProvider._persistentIdentifierToCheck` | proposed `BestiaryCollectionProviderDefinitions.cs`, BestiaryCreditId |
| 1371 | `CommonEnemyUICollectionInfoProvider._quickUnlock` | proposed `BestiaryCollectionProviderDefinitions.cs`, QuickUnlock |
| 1372 | `CommonEnemyUICollectionInfoProvider._killCountNeededToFullyUnlock` | proposed `BestiaryCollectionProviderDefinitions.cs`, FullKillCountNeeded |
| 1373 | `CritterUICollectionInfoProvider._persistentIdentifierToCheck` | proposed `BestiaryCollectionProviderDefinitions.cs`, Critter BestiaryCreditId |
| 1398 | `TownNPCUICollectionInfoProvider._persistentIdentifierToCheck` | proposed `BestiaryCollectionProviderDefinitions.cs`, Town NPC BestiaryCreditId |
| 3726 | `IBestiaryEntryDisplayIndex.BestiaryDisplayIndex` | proposed `BestiaryCollectionUnlockQuery.cs`, display-index contract |
| 3727 | `IBestiaryEntryFilter.ForcedDisplay` | proposed `BestiaryCollectionUnlockQuery.cs`, filter metadata contract |
| 3728 | `IBestiarySortStep.HiddenFromSortOptions` | proposed `BestiaryCollectionUnlockQuery.cs`, sort metadata contract |

The mapping covers all 93 source rows: 2352-2358, 2456, 3849; 2574-2576; 2161-2171, 2567-2573 and 3828; 1357-1362 and 3718-3719; 1365-1369, 1380-1382, 1392-1397 and 3720; 1374, 3721-3725 and 3732-3737; 1375-1379, 1383-1391 and 3729-3731; and 1363-1364, 1370-1373, 1398 and 3726-3728.

## 3.9 SharedTownRoomState implementation checkpoint

Saved candidate implementation source files under `D:\TRbackup\NLTX\src2\NpcTownBestiary` (106 C# files):

- `TownRoomRegistryComponent.cs`, `AssignTownRoomCommand.cs`, `EvictTownResidentCommand.cs`, `TownRoomAssignmentSystem.cs`, `TownRoomMutationResult.cs`
- `TownRoomQuery.cs`, `TownRoomAvailabilityProjection.cs`, `TownHousingCompatibilityQuery.cs`, `NpcHousingCategory.cs`
- `TownRoomMutationGateAdapter.cs`, `TownRoomPersistenceAdapter.cs`, `TownRoomTilePoint.cs`, `TownHousingKeyMode.cs`
- `NpcTypeId.cs`, `NpcNetId.cs`, `NpcEntityId.cs`, `PlayerEntityId.cs`, `BestiaryCreditId.cs`, `BestiaryEntryKey.cs`

The implementation keeps the Version4 NPC-type room key and replacement/removal semantics, makes revision conflicts explicit, keeps `_hasRoom` as a derived projection, and isolates binary persistence and mutation synchronization behind adapters. It does not claim that the integration-review owner has selected NPC type versus instance identity. The latest src2 build and focused verifier provide local evidence only; they do not prove production behavior equivalence or integration closure.

## 4. Implementation order and single write owners

The following order is proposed. It is a dependency contract, not a claim about current runtime scheduling; final cross-subsystem order is `crossSubsystemOwner: integration-review`.

1. Establish read-only identity/content adapters for NPC type, NpcNetId, PersistentId, BestiaryCreditId, item group, sorting/rarity and profile asset keys.
2. Establish proposed TownRoom registry and key fixture. Select one existing NLTX registry draft and one NPC assignment projection; keep other drafts read-only compatibility inputs.
3. Add proposed room commands/System/Query and mutation gate. Add Save/Load/Clear adapter only after Version4 byte-order fixtures exist.
4. Build proposed personality and Town NPC profile catalogs. Keep HelperInfo as transient query input and isolate Player/NPC references behind read ports.
5. Register proposed Bestiary entries/filter/sort refs and derive NpcNetId index. Run proposed drop merge adapter after content/drop setup; freeze catalog before consumers.
6. Establish kill/sight/chat persistent Components and explicit discovery writers. Add scan buffer separately from sight facts.
7. Add persistence, validation, reset, player-join and network adapters only after identity and payload version decisions; do not assume empty `Deserialize` is complete behavior.
8. Add pure filter/sort Query and stable tie-breaks against immutable entry/presentation/unlock snapshots.
9. Add InfoElement facts and stats refresh adapter; expose only UI-neutral snapshots.
10. Add collection provider definitions, unlock Query and UICollection Projection; then compute progress from projection/query output.
11. Add compatibility contract tests and only then consider replacing legacy read paths. No deletion gate is implied by this plan.

Single write owners:

- proposed `ConditionalDialogueCatalog` registration System writes dialogue definitions; proposed consume System writes one-time consumption; proposed Lucy cooldown System/trigger command writes cooldown.
- proposed `TownRoomAssignmentSystem` writes room registry and assignment event. Persistence adapter writes only during load/restore boundary, never during ordinary Query. Availability projection is derived.
- proposed `NpcPersonalityCatalogBuilder` writes personality definitions once during content setup; profile asset adapter does not write shopping definitions.
- proposed `BestiaryCatalogRegistrationSystem` writes catalog during content setup; drop merge adapter contributes through that System only; filter/sort catalogs own their own registrations.
- proposed kill/sight/chat discovery Systems write their respective persistent Components; progress, collection, InfoElement, filter and sort Query are read-only.
- proposed persistence/network/join adapters perform external effects from explicit snapshots/events and cannot modify authority directly.

## 5. Compatibility, persistence, network and snapshot migration

### 5.1 Compatibility and dual-read policy

- Begin with read-only adapters from old Version4-shaped data to proposed stable views. No old registry/new registry or old tracker/new tracker dual-write is permitted.
- During Town migration, preserve Version4 `count -> npcType -> X -> Y` payload order in a proposed legacy adapter. If key mode changes, require an explicit save version and deterministic conversion; do not infer instance identity from a type-only row.
- During Bestiary migration, keep three independent payload groups in Kills/Sights/Chats order. `BestiaryCreditId` remains the persistence key candidate; `NpcNetId` is resolved only at network/content boundaries.
- For collection snapshots, replace object references with proposed stable `BestiaryEntryKey`/OwnerEntryKey. Keep `UnlockState` derived from current discovery facts and policy; do not persist a second unlock truth.
- Treat `BestiaryDiscoveryCache.PlayerBestiaryBounds` and `SeenNpcNetworkIds` as transient scan inputs. They cannot be used as persistent sight facts without an explicit conversion test.
- Content identity conversion must distinguish `NpcEntityId`, `PersistentEntityId`, NPC type, `NpcNetId` and `BestiaryCreditId`. Every conversion port and snapshot field remains `crossSubsystemOwner: integration-review`.

### 5.2 External effects

- WorldFile binary I/O, network package serialization/deserialization, player-join send, UI element construction, audio/chat messages, logging, clocks and resource loads belong only to adapters or projections.
- Queries do not call Save/Load, send network messages, grant achievements, consume dialogue, mutate registry, or update caches except for explicitly scoped local calculation state.
- `EntityCreationLock` is represented as a proposed mutation gate adapter. It is not copied into a Component field, and lock ownership is not inferred from file order.
- `NPCStatsReportInfoElement` refresh is represented as a proposed adapter from NPC read/definition data. It must not retain a live NPC object in presentation state.

### 5.3 Rollback

1. Disable the proposed registration or command adapter while leaving legacy data readable.
2. Stop new writes at the proposed System boundary; discard uncommitted projection/snapshot output.
3. Restore the previous read-only adapter for the affected subsystem.
4. Retain the original persistence payload until a verified conversion exists; never rewrite world data during rollback.
5. Record the failing fixture and affected identity/key mode before retrying.

Rollback conditions include duplicate room ownership, key mode drift, room Save/Load byte mismatch, repeated kill/sight/chat discovery, credit/net ID mismatch, duplicate network events, unexpected UnlockState transition, unstable sort order, provider snapshot mutation, stats refresh order change, or a projection writing authority.

## 6. Focused verifier and static-check plan

The latest src2 candidate implementation was compiled and exercised by the focused verifier. The result is local contract evidence, not a production parity or cross-subsystem closure claim. The planned checks remain:

- Dialogue: static/dynamic ItemGroup snapshots, registration isolation, Query purity, consume idempotency and revision conflict, cooldown tick and source independence.
- Town: SetRoom replacement, KickOut deletion, empty/duplicate rows, revision conflicts, type/instance key fixtures, compatibility Query purity, availability projection rebuild, Save/Load/Clear byte order and command ordering.
- Personality: preference order/duplicate registration, missing profile result, biome query purity, HelperInfo transient lifetime, nearby type index invalidation, profile asset variants and path ownership.
- Catalog: entry order, NPC index collisions, explicit NotFound result, catalog freeze, drop merge range and missing-entry behavior, filter/sort registration order.
- Unlocks: kill cap, sight/chat first-discovery idempotency, scan buffer reset/dedupe, identity mapping, Save/Load/Validate order, world reset, player join snapshot, network payload and Deserialize behavior, zero-entry progress rule.
- Filters/sorting: metadata values, search normalization, missing InfoElement, unlock snapshot read-only behavior, comparer transitivity/stability and explicit tie-breaks.
- Info elements: localization/drop/range conversion, no NPC object retention, stats refresh ordering, kill counter read-only behavior, display/rarity mapping and HideStats projection.
- Collection: all unlock-state thresholds, quick unlock, critter sight, Town NPC chat, missing persistent ID, stable OwnerEntryKey, immutable snapshot and progress aggregation.

Static checks before any implementation claim:

- confirm all proposed types use capability/domain-first paths and one core public type per same-named file;
- check no proposed Query writes Component/catalog state and no Projection becomes authority;
- check external Version4/Terraria types are isolated behind adapters/ports;
- check one writer per registry, assignment, discovery fact and catalog;
- check IDs and snapshots are not collapsed into one generic integer/string;
- check all cross-subsystem readers/writers and order constraints are marked `crossSubsystemOwner: integration-review`;
- check generated output, binaries and intermediates remain under `Build/bin/`, `Build/obj/`, `Build/generated/` or `Build/packages/` when implementation is later authorized.

The executed compile-capable verification used only the repository runner from the root:

```powershell
pwsh -NoProfile -Command "& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('build', '.\src2\NpcTownBestiaryVerification\Terraria.NpcTownBestiaryVerification.csproj', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"
```

Build result: exit code `0`, `0` warnings, `0` errors. Artifacts:

- `D:\TRbackup\NLTX\Build\bin\Terraria.NpcTownBestiary\Debug\net10.0\Terraria.NpcTownBestiary.dll`
- `D:\TRbackup\NLTX\Build\bin\Terraria.NpcTownBestiaryVerification\Debug\net10.0\Terraria.NpcTownBestiaryVerification.dll`

The subsequent no-build verifier command was:

```powershell
pwsh -NoProfile -Command "& .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '--project', '.\src2\NpcTownBestiaryVerification\Terraria.NpcTownBestiaryVerification.csproj', '--no-build', '--no-restore', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')"
```

Verifier result: exit code `0`; `P11 focused verifier passed: 8 scenarios`.

## 7. Risks, evidence gaps and integration handoff

Risks that remain open:

- The first-round P11 report is missing, so this plan cannot treat first-round decomposition decisions as completed evidence.
- Version4 is a reduced/partial source copy for several Bestiary and dialogue paths; complete-reference behavior must not be copied without Version4 compatibility evidence.
- Town housing currently has several NLTX draft states with overlapping fields. Selecting a registry and assignment owner is a prerequisite, not an implementation detail.
- Network receive behavior, persistence versioning, NPC population, item-drop ownership, resource profile lifecycle and stats refresh are not closed.
- The final System order across Content, WorldFile, NPC, Player, Network and UI is not owned by P11.

Integration Handoff:

subsystemId: SharedRuntimeMechanisms / P11 NPC-Town-Bestiary
taskNumber: P11-second-round-component-plan
partitionId: P11
sessionId: c822e256a5e34730bb0ab0ad5a6a094b
inputReport: D:\TRbackup\NLTX\docs\migration\ledgers\non-authoritative-component-partitions\11-npc-town-bestiary.md
componentDesignPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P11-npc-town-bestiary-component-design.md
componentExecutionPath: D:\TRbackup\NLTX\docs\component-decomposition\review-round-2\non-authoritative\2026-09-11-version4-non-authoritative-P11-npc-town-bestiary-component-execution.md
evidenceStatus: partial
nltxStatus: partial; src2 candidate implementation is executable and focused-verified, but no authoritative current-NLTX P11 runtime or parity claim is made
verificationStatus: independently-verified
src2CodeModified: yes
src2SourceRoot: D:\TRbackup\NLTX\src2\NpcTownBestiary
src2CSharpFileCount: 106
productionSrcModified: no
confirmedOwners:
- Version4 TownRoomManager currently owns room list, NPC type quick index and Save/Load surface.
- Version4 PersonalityDatabase/TownNPCProfiles currently own their definition/profile source surfaces.
- Version4 BestiaryDatabase currently owns entry/filter/sort registration and drop merge surface.
- Version4 BestiaryUnlocksTracker and its three trackers currently own persistence method surfaces.
proposedImplementationOwners:
- proposed TownRoomAssignmentSystem for room authority mutation, after key mode decision
- proposed NpcPersonalityCatalogBuilder for personality definition registration
- proposed BestiaryCatalogRegistrationSystem for entry/filter/sort registration and drop merge coordination
- proposed BestiaryDiscoverySystem and BestiarySightScanSystem for three discovery fact transitions
- proposed persistence/network/join adapters for external effects only
sharedTypesForIntegrationReview:
- NPC type, NpcEntityId, PlayerEntityId, PersistentEntityId, NpcNetId, BestiaryCreditId
- TownRoomTilePoint, room key mode, BestiaryEntryKey, WorldFile snapshot, collection snapshot
- ItemDrop/RecipeGroup view, WorldTick, network payload, UI/audio request and System order
boundaryChallenges:
- choose one town registry and one NPC assignment owner across current NLTX drafts
- preserve three independent Bestiary facts while separating persistent IDs, network IDs and transient scan buffers
- keep catalog, unlock authority, presentation facts, filter/sort Query and UICollection snapshot one-way
- close missing entry population, network Deserialize, stats refresh, persistence versioning and UI provider behavior
blockingDecisions:
- town key/assignment owner and WorldFile migration format
- personality/profile catalog owner and Player/NPC/nearby query ports
- Bestiary content registration/drop merge/filter/sort owner and schedule
- discovery authority, network/persistence version and identity mapping
- InfoElement/UICollection projection owner and stats refresh source
- conditional dialogue/Lucy authority and all cross-subsystem types/order
implementedInSrc2:
- all 8 P11 component ranges listed in `completedComponents` have saved candidate source under `src2\NpcTownBestiary`
unverifiedOrDeferred:
- authoritative current-NLTX integration, behavior equivalence, network receive closure, persistence-version closure, UI/resource ownership and cross-partition scheduling remain unverified or deferred
verifierPlan:
- focused pure Query, state transition, persistence byte-order, network payload, catalog freeze, ID mapping, projection one-way and compatibility checks; executed as recorded in section 6

本文件已保存 8 个 P11 候选组件范围的 src2 实现并通过 focused verifier；没有执行生产 `src` 迁移、行为等价验证或两份第二轮 Markdown 的 `git diff --check`。当前文档保持 proposed/evidence-gap 边界，不宣称网络闭合、存档闭合或跨分区 owner 已裁决。

## 8. Handoff 后 Component 实施检查点

handoffStatus: completed
previousSessionId: de06dd0a16934d8c92ff67a0130cff30
sessionId: c822e256a5e34730bb0ab0ad5a6a094b
handoffId: p11-npc-town-bestiary-handoff-20260912-02
implementationStatus: completed
verificationStatus: independently-verified
src2SourceRoot: D:\\TRbackup\\NLTX\\src2\\NpcTownBestiary
productionSrcModified: no

本次接手实际修改的唯一 Component 文件：

- `src2/NpcTownBestiary/BestiaryKillCountStateComponent.cs`
  - `Add` 对负数候选值收敛到 `0`，对超过 `PositiveCap` 的候选值收敛到上限，避免 Component 持有负击杀事实或整数溢出结果。
  - 未新增或修改 System、Query、Command、Adapter、Projection、测试、测试项目、注册或装配代码。

本次验证证据：

- 非权威 runner contract：`Test-Version4NonAuthoritativePartitionSession.ps1`，exit code `0`，输出 `PASS: non-authoritative partition session contract tests passed.`
- Component build：通过 `Build/Tools/Invoke-SerialDotnet.ps1` 构建 `src2/NpcTownBestiary/Terraria.NpcTownBestiary.csproj`，exit code `0`，warnings `0`，errors `0`。
- build artifact：`D:\\TRbackup\\NLTX\\Build\\bin\\Terraria.NpcTownBestiary\\Debug\\net10.0\\Terraria.NpcTownBestiary.dll`
- focused verifier：通过 `Build/Tools/Invoke-SerialDotnet.ps1` 以 `--no-build --no-restore` 运行 `src2/NpcTownBestiaryVerification/Terraria.NpcTownBestiaryVerification.csproj`，exit code `0`，`P11 focused verifier passed: 8 scenarios`。

本检查点仍不宣称生产 `src` 迁移、行为等价、网络/存档闭合或跨分区 owner 裁决；现有 `evidence-gap`、`blocking-decision` 和非 Component 排除范围继续有效。
