# Version4 组件拆分遗漏复查报告

> reviewId: `version4-component-split-omission-review-20260907`
>
> reviewDate: `2026-09-07`
>
> reviewScope: `2026-09-05-version4-public-decomposition-common-protocol` 规定的 19 个 Version4 游戏模拟子系统
>
> method: `public-decomposition` ECS evidence protocol
>
> verificationStatus: `not-run`

## 1. 执行结论

本次复查的结论不是“19 个子系统都漏了组件”，也不是“详细设计已经完整”。更准确的结论如下：

1. **公共拆分报告的入口粒度普遍偏粗，但其中大部分遗漏已经在后续 Component-only 详细设计中补齐。** Combat、DeathPenaltyAndRevenge、Fishing、NPC/Town、Player、WorldCalendar、Teleportation、WorldGeneration、WorldInteraction、Projectile、SpawnLifecycle、LeashedEntity 等均存在这种“public entry omission → detailed design remediation”链路。这些项目不应再次被报告为最终组件遗漏。
2. **仍然存在字段级的确定性闭合缺口。** 当前最重要的是 `WorldSessionComponents.cs` 中的世界进度/事件字段：详细设计排除了基础 WorldSession 聚合，但没有为全部字段建立唯一、字段级、可持久化和可网络投影的最终 owner。Version4 证据表明这些字段确实是世界权威状态，不是可以静默丢弃的缓存。
3. **存在会改变组件统计、目录和实现批次的阻塞决策。** Combat 的 `EncounterDamageCreditComponent` 在详细报告总览中明确属于 Combat，但第 13 节又只说 Combat 目录实现“前九个 Combat 组件”；`WorldProgressionAndUnlocks` 的公共 `UnlockFactComponent` 也没有在详细设计最终清单中保留同名 owner。两者都需要最终整合裁决，不能靠本报告擅自合并或删除。
4. **当前 NLTX 存在多个重复或竞争的权威状态源。** 典型包括 Spatial/Physics 的碰撞和运动类型、Items 的堆叠/容器/装备/世界物品类型、NPC 的身份/AI/目标/生命类型、Combat 的伤害归因和状态效果类型、WorldSession/WorldGeneration 的同名模型以及 Pylon registry。它们说明“详细设计存在”不能等价为“迁移边界已经闭合”。
5. **本轮没有运行构建或测试。** 报告所有当前 NLTX 的结论保持 `existing`、`partial`、`missing`、`evidence-mismatch` 或 `unresolved` 的证据口径；不把源码文件存在、设计文件存在或历史验证材料解释成当前行为等价或迁移完成。

因此，当前建议的优先级是：

```text
P0  裁决 WorldSession 世界进度字段的最终 owner 和提交/存档/网络边界
P0  裁决 Combat EncounterDamageCreditComponent 的归属与实现批次
P1  关闭当前 src 中重复状态源和旧/新双写风险
P1  补齐 WorldProgressionAndUnlocks 的 UnlockFactComponent 设计链
P2  为各子系统建立字段级 sourceMember → targetComponent → writer → lifecycle → persistence/network evidence manifest
```

## 2. 审计范围、来源和限制

### 2.1 固定范围

本报告只复查以下 19 份公共拆分报告及其同主题详细 Component-only 设计：

```text
CombatAndStatus
DeathPenaltyAndRevenge
FishingAndCatchSimulation
ItemContainerAndEconomy
LeashedEntitySimulation
LiquidSimulation
NpcAndTownSimulation
PlayerGameplay
ProjectileSimulation
SimulationRuleOverrides
SpatialSimulation
SpawnLifecycleAndLoot
TeleportationAndTraversal
WorldCalendarAndEventOrchestration
WorldGenerationAndEcology
WorldInteractionAndStructures
WorldProgressionAndTransition
WorldProgressionAndUnlocks
WorldSession
```

公共范围由 `docs/第一轮审查/设计/2026-09-05-version4-public-decomposition-common-protocol.md` 固定。`docs/迁移参考表/Version4子系统索引.json` 中的 32 个责任域不能扩大本次 19 子系统的审计分母。

### 2.2 证据优先级

本报告沿用协议中的来源角色：

| 优先级 | 来源 | 用途 | 限制 |
|---:|---|---|---|
| 1 | `D:\TRbackup\Version4` | 确认真实字段、方法、调用者、写者、生命周期、存档和网络边界 | Version4 的空实现或删减不能由其他来源静默补回 |
| 2 | `D:\TRbackup\无任何删减通过编译` | 只补 Version4 已存在同路径文件的成员/邻近调用链差异 | 不扩大 Version4 覆盖分母 |
| 3 | `D:\TRbackup\tmodloader-api-docs-stable` | 交叉核对公开 API、生命周期、网络和存档扩展边界 | 不能替代 Terraria 私有实现 |
| 4 | `C:\Users\shan\Downloads\ECS\space-station-14-master` | 只参考 Component、System、Query、事件、快照和身份分离的组织粒度 | 不能推断 Terraria 领域语义 |
| 5 | 当前 NLTX `src`、`Test`、`dome\src` 和既有文档 | 判断当前文件、局部模型、重复 owner 和既有材料状态 | 文件存在不证明接线、行为等价或迁移完成 |

`D:\TRbackup\Version4\Terraria.IO\WorldFile.cs` 是实际存档文件路径；公共材料中曾出现 `Terraria\WorldFile.cs` 的路径差异，本报告统一使用实际路径，并保留该差异为 `evidence-mismatch`。

### 2.3 排除项

本报告不修改以下内容，也不把它们纳入“组件遗漏”统计：

- `src\`、`Test\`、`dome\src\` 和 `dome\Test\` 生产/测试源码；
- Version4、完整参考源码、tModLoader 文档和 SS14 参考源码；
- 已有公共拆分报告、详细设计、索引、覆盖表、差异附录和验证脚本；
- System、Query、Command、Adapter、Projection、网络 DTO、存档 reader/writer、UI 表现对象、纯值对象和一次性结果，除非它们被错误地当作权威 Component 或造成 owner 冲突；
- 所有 compile-capable 命令。

## 3. public-decomposition 判定规则

### 3.1 组件统计口径

公共报告中的 `proposed ECS boundary` 经常混合 Component、System、Query、Command、Adapter、Projection 和值对象。因此本报告的 `publicComponentEntryCount` 只统计公共报告明确作为持续状态/组件候选的数据条目：

- 统计 `HealthComponent`、`RuleOverrideState`、`CalendarClockState` 这类权威或行为状态候选；
- 不统计 `DamageRequest`、`EligibilityQuery`、`CommitSystem`、`Projection`、`Adapter`、`Event` 和纯结果值；
- 公开报告只写“CapabilityComponents”或把多个能力合并成一个占位时，记录为 `mixed/placeholder`，不伪造精确组件数；
- 详细设计的最终 Component 清单以其明确的“最终 Component 清单”或等效章节为准；`status: existing/partial` 仅表示当前映射，不表示实现完成。

### 3.2 遗漏分类

| 状态 | 判定 |
|---|---|
| `public-entry-omission` | 公共报告入口没有列出后续详细设计中的组件或只给出过粗占位 |
| `detailed-design-remediated` | 公共入口遗漏，但后续详细设计已明确承接，不能再称为最终遗漏 |
| `missing` | 关键字段/状态没有目标 Component 或唯一 owner |
| `partial` | 有局部字段或类型，但 owner、接线、生命周期、提交、持久化或网络边界未闭合 |
| `evidence-gap` | Version4 关键成员、写入者或调用链证据不足 |
| `evidence-mismatch` | 文档路径、符号或当前工作树与声明不一致 |
| `duplicate-owner` | 当前 NLTX 同一语义有多个状态源或多个潜在写入根 |
| `blocking-decision` | 裁决会改变权威 owner、事务提交边界、Component 数量或系统顺序 |

### 3.3 关键门槛

一个 Component 只有在以下信息基本闭合时才能从“候选”进入实现评审：字段、读者、写者、生命周期、权威/派生/缓存分类、实体范围、持久化/网络边界和唯一提交根。Query 不得写权威状态，Projection 不得反写模拟状态，兼容字段不得在没有 revision 或单写根的情况下和新组件长期双写。

## 4. 19 子系统覆盖矩阵

说明：矩阵中的 `publicComponentEntryCount` 是公共入口的“数据组件候选”计数，不是公共报告所有 proposed 条目的计数；`detailedComponentCount` 来自当前工作树中的同主题详细 Component-only 清单。`public-to-detailed` 的 `remediated` 不代表代码已实现，只表示文档层已经补出组件候选。

| subsystemId | publicComponentEntryCount / public entries | detailedComponentCount / detailedComponentNames | public-to-detailed 判定 | 当前复查结论 |
|---|---|---|---|---|
| `CombatAndStatus` | 3：`HealthComponent`、`ImmunityComponent`、`StatusEffectStateComponent` | 11：`HealthComponent`、`HealthRegenerationStateComponent`、`DefenseComponent`、`DamageReductionComponent`、`DamageAcceptancePolicyComponent`、`ImmunityComponent`、`KnockbackPolicyComponent`、`StatusEffectSlotsComponent`、`StatusEffectImmunityComponent`、`EntityProvenanceComponent`、`EncounterDamageCreditComponent` | `public-entry-omission`；大部分 `detailed-design-remediated` | `blocking-decision`：Encounter 归属/目录批次冲突；状态效果来源字段仍有 owner 风险 |
| `DeathPenaltyAndRevenge` | 1：`RevengeMarkerState` | 4：`RevengeMarkerRegistryComponent`、`RevengeMarkerIdentityComponent`、`RevengeMarkerLifecycleComponent`、`RevengeTargetSnapshotComponent` | `public-entry-omission`；`detailed-design-remediated` | 四个组件已形成候选，但 marker 持久化范围、ID 和 lock owner 未决 |
| `FishingAndCatchSimulation` | 2：`FishingAttemptState`、`BobberTimingComponent` | 6：`FishingAttemptStateComponent`、`BobberTimingComponent`、`FishingEligibilitySnapshotComponent`、`FishingCatchDecisionComponent`、`FishingBaitReservationComponent`、`FishingResultCommitStateComponent` | `public-entry-omission`；`detailed-design-remediated` | reservation 和 result commit 的跨域 owner/提交边界未决 |
| `ItemContainerAndEconomy` | 10 个混合数据候选：`ItemDefinitionRef`、`ItemInstance`、`ItemStackState`、`ContainerContents`、`ContainerAccess`、`Reservation`、`CraftingState`、`CommerceOffer`、`CurrencyBalance`、`CommerceLedger` | 14：`ItemInstanceComponent`、`ItemStackComponent`、`ContainerLayoutComponent`、`ContainerCapacityComponent`、`ContainerContentsComponent`、`ContainerAccessComponent`、`EquipmentRelationComponent`、`WorldItemStateComponent`、`WorldItemReservationComponent`、`CraftingStateComponent`、`CraftingReservationComponent`、`ShopInventoryComponent`、`CurrencyBalanceComponent`、`CommerceLedgerComponent` | `public-entry-omission`；`detailed-design-remediated` | 当前根 `src/Items` 仍有堆叠、容器、装备、WorldItem 多份状态源 |
| `LeashedEntitySimulation` | 5：`LeashedEntityStateComponent`、`LeashedEntityMotionComponent`、`LeashedEntitySectionComponent`、`LeashedEntityAnchorRelationComponent`、`LeashedEntityLifecycleComponent` | 8：上述通用五项、`LeashedCritterBehaviorComponent`、`LeashedKiteBehaviorComponent`、`LeashedEntityLegacySlotComponent` | `public-entry-omission`；`detailed-design-remediated` | Critter/Kite/legacy slot 已补出；Version4 主文件运动字段缺失和 anchor/section owner 仍 partial |
| `LiquidSimulation` | 5：`LiquidStateComponent`、`LiquidWorldStateComponent`、`LiquidWorkQueueComponent`、`LiquidContactComponent`、`LiquidDirtySectionComponent` | 6：`TileLiquidStateComponent`、`LiquidWorkStateComponent`、`LiquidWorldRuntimeStateComponent`、`LiquidContactStateComponent`、`LiquidDirtySectionStateComponent`、`LiquidSequenceStateComponent` | `public-entry-omission`；`detailed-design-remediated` | Sequence 是否单独成为 Component、Tile/Section/World owner 未决；当前 NLTX 还有 LiquidContact 语义差异 |
| `NpcAndTownSimulation` | 5：`NpcIdentityComponent`、`NpcLifecycleComponent`、`NpcBehaviorStateComponent`、`NpcTargetComponent`、`NpcHomeRelationComponent` | 12：`NpcIdentityComponent`、`NpcLegacySlotComponent`、`NpcDefinitionReferenceComponent`、`NpcBehaviorComponent`、`NpcLocalBehaviorStateComponent`、`NpcTargetComponent`、`NpcLifecycleComponent`、`NpcParentRelationComponent`、`NpcHealthComponent`、`TownResidentComponent`、`TownHousingRelationComponent`、`TownHousingRegistryComponent` | `public-entry-omission`；`detailed-design-remediated` | 身份/AI/目标/生命均有竞争类型；NPC health 必须和 Combat health 统一写者 |
| `PlayerGameplay` | 8：`PlayerIdentityComponent`、`PlayerLifecycleComponent`、`PlayerVitalComponent`、`PlayerEquipmentComponent`、`PlayerInventoryView`、`PlayerUseState`、`PlayerAbilityState`、`InputIntentComponent` | 14：`PlayerIdentityComponent`、`InputIntentComponent`、`PlayerLifecycleComponent`、`PlayerVitalComponent`、`PlayerEquipmentComponent`、`PlayerInventoryComponent`、`PlayerUseComponent`、`PlayerAbilityComponent`、`PlayerBuffComponent`、`PlayerRegenerationAndImmunityComponent`、`PlayerSpawnPointComponent`、`PlayerMountComponent`、`PlayerRestComponent`、`PlayerFishingCapabilityComponent` | `public-entry-omission`；`detailed-design-remediated` | 能力组件已补出，但 Player/Item/Combat/Fishing/Mount 的 owner 尚未整合 |
| `ProjectileSimulation` | 8 个混合/占位数据候选：Identity、Definition、Trajectory、Lifetime、Penetration、Damage、CollisionPolicy、`CapabilityComponents` | 18：`ProjectileIdentityComponent`、`ProjectileDefinitionComponent`、`ProjectileTrajectoryStateComponent`、`ProjectileLifetimeComponent`、`ProjectileDamagePayloadComponent`、`ProjectilePenetrationStateComponent`、`ProjectileHitImmunityStateComponent`、`ProjectileHitImmunityPolicyComponent`、`ProjectileCollisionPolicyComponent`、`ProjectileGeometryStateComponent`、`ProjectileNetworkStateComponent`、`ProjectileSourceMetadataComponent`、`ProjectileTrailCacheComponent`、`ProjectileSentryCapabilityComponent`、`ProjectileMinionCapabilityComponent`、`ProjectileTrapCapabilityComponent`、`ProjectileBobberCapabilityComponent`、`ProjectileCounterweightCapabilityComponent` | `public-entry-omission`；`CapabilityComponents` 是明显的粒度占位，`detailed-design-remediated` | AI style、raw AI、UUID、Kill/child/drop、Bobber/Leashed 交接仍为 integration review |
| `SimulationRuleOverrides` | 1：`RuleOverrideState` | 5：`RuleOverrideStateComponent`、`PlayerRuleOverrideStateComponent`、`RuleOverridePermissionStateComponent`、`RuleOverrideSnapshotComponent`、`PlayerRuleOverrideSnapshotComponent` | `public-entry-omission`；`detailed-design-remediated` | 世界/玩家/权限/有效快照边界已补出；基础事实与 override 仍需唯一写者 |
| `SpatialSimulation` | 7：`MovementState`、`CollisionShape`、`SpatialReference`、`MotionHistory`、`LiquidContact`、`CollisionPolicy`、`CollisionResult` | 7：`MovementState`、`CollisionShape`、`SpatialReference`、`MotionHistory`、`LiquidContact`、`CollisionPolicy`、`CollisionResult` | 数量无遗漏，但 `public-entry` 与当前 `src` 的 owner 不一致 | `duplicate-owner`：Physics/Share/SpatialSimulation 有重复类型；不应以数量通过审计 |
| `SpawnLifecycleAndLoot` | 7 个混合 Component/value 候选：`EntityLifecycleState`、`SpawnAdmissionState`、`SpawnIdentityState`、`EntityRelationState`、`NpcDefinitionReferenceState`、`NpcPopulationLifetimeState`、`LootResolutionState`/`LootAttribution` | 8：`EntityLifecycleState`、`SpawnAdmissionState`、`EntityIdentityState`、`EntityRelationState`、`NpcDefinitionReferenceState`、`NpcPopulationLifetimeState`、`LootResolutionState`、`LootAttributionState` | `public-entry-omission`；`detailed-design-remediated` | 通用生命周期/身份候选已补齐；不可夺取 NPC/Projectile/Item/Network/Persistence 的最终 owner |
| `TeleportationAndTraversal` | 1：`TraversalCooldownComponent` | 5：`PylonRegistryComponent`、`PortalEndpointComponent`、`PortalLinkStateComponent`、`TeleportCooldownStateComponent`、`PortalTraversalCooldownStateComponent` | `public-entry-omission`；`detailed-design-remediated` | Pylon registry、端点、配对和两类冷却已补出；当前 registry 有重复状态源 |
| `WorldCalendarAndEventOrchestration` | 3：`CalendarClockState`、`EventInstanceState`、`EventScheduleState` | 14：`CalendarClock`、`WorldEventInstanceState`、`BirthdayPartyState`、`LanternNightState`、`SandstormState`、`SlimeRainState`、`WorldInvasionState`、`InvasionHistoryState`、`PendingWorldEventState`、`WorldCalendarOverrideState`、`Dd2PersistentProgressState`、`Dd2RunState`、`Dd2WaveRuntimeState`、`WorldEventRandomState` | `public-entry-omission`；`detailed-design-remediated` | `CoinRain`、`CultistDelay`、部分事件历史/进度与 WorldSession 的承接仍未闭合 |
| `WorldGenerationAndEcology` | 3：`GenerationPlan`、`PassState`、`BiomeEcologyState`；其余为 Query/Command/Port/Projection | 13：`WorldDescriptorComponent`、`WorldGenerationRulesComponent`、`WorldGenerationLifecycleComponent`、`WorldGenerationPlanComponent`、`WorldGenerationPassStateComponent`、`WorldGenerationRandomStateComponent`、`WorldTerrainStateComponent`、`BiomeEcologyStateComponent`、`EcologyScheduleComponent`、`HousingScanStateComponent`、`TownHousingAssignmentComponent`、`WorldLiquidHandoffComponent`、`ReadyTransitionComponent` | `public-entry-omission`；`detailed-design-remediated` | 生成期状态已细分；全量 Pass、住房执行、Ready barrier、唯一 Tile writer 未闭合 |
| `WorldInteractionAndStructures` | 1：`WiringWorkState` | 29：`TileCellComponent`、`TileFrameComponent`、`TileSignalTopologyComponent`、`TileLiquidWorkStateComponent`、`PressurePlateOccupancyComponent`、`MechanismCooldownComponent`、`WirePropagationScratchComponent`、`PumpTransferScratchComponent`、`InteractionActorContextComponent`、`TileEntityAnchorComponent`、`TileEntityKindComponent`、`TileEntityRuntimeIdComponent`、`TileEntityPersistenceIdentityComponent`、`TileEntityNetworkIdentityComponent`、`TileEntityUpdateScheduleComponent`、`LogicSensorComponent`、`TrainingDummyComponent`、`ItemFrameComponent`、`FoodPlatterComponent`、`WeaponRackComponent`、`HatRackComponent`、`DisplayDollComponent`、`DeadCellsDisplayJarComponent`、`LeashedEntityAnchorComponent`、`StructureFootprintComponent`、`ChestStructureComponent`、`MultiSlotItemPayloadComponent`、`PylonStructureComponent`、`PylonRegistryComponent` | `public-entry-omission`；`detailed-design-remediated` | 公共入口严重过粗；详细清单覆盖多格结构和 TileEntity，但所有权均需 integration review |
| `WorldProgressionAndTransition` | 3 个混合状态候选：`ProgressionTransitionState`、`TransitionPlan`、`TransitionRecoveryState` | 9：`WorldHardmodeRuleState`、`WorldInfectionPolicyState`、`HardmodeOreTierState`、`ProgressionTransitionState`、`TransitionPlanState`、`ProgressionCommitState`、`TransitionRecoveryState`、`RuntimeHardmodeCompatibilityState`、`WorldMetadataHardmodeState` | `public-entry-omission`；`detailed-design-remediated` | Hardmode 早写规则值、后台变换、WorldFile、网络重同步的事务提交边界仍为 blocking |
| `WorldProgressionAndUnlocks` | 2：`UnlockFactComponent`、`ProgressionAggregate` | 2：`ProgressionAggregate`、`BestiaryDiscoveryCache` | 数量相同但发生设计链替换；`UnlockFactComponent` 没有最终同名 owner | `evidence-gap` / `blocking-decision`：需确认 UnlockFact 是否已被 Aggregate 吸收，或确实遗漏 |
| `WorldSession` | 5 个核心 Component + 1 个快照：`WorldDescriptorState`、`WorldRulesState`、`WorldClockState`、`WorldWeatherState`、`SessionReadinessState`、`WorldTickSnapshot` | 6：同上 | 数量无遗漏；字段承接不闭合 | `missing` / `blocking-decision`：`WorldEventProgressState`、`WorldSpawnPressureState` 和多个嵌套字段没有唯一最终 owner |

## 5. 详细设计已经补齐的公共入口遗漏

这些项目在公共报告层确实是粒度不足，但现有详细设计已经给出更细的 Component 候选，因此本复查不把它们列为最终遗漏：

| 子系统 | 公共入口问题 | 详细设计的补齐 | 仍需注意 |
|---|---|---|---|
| Combat | 只有 Health、Immunity、StatusEffectState 三个组件 | 补出再生、防御、减伤、受伤准入、击退、状态槽/免疫、来源和 Encounter credit，共 11 项 | `EncounterDamageCreditComponent` 目录/批次文字冲突未解决 |
| DeathPenaltyAndRevenge | 用一个 `RevengeMarkerState` 表达 registry、marker identity、生命周期和 target snapshot | 拆为 4 个组件，分别表达 World registry、marker identity、过期/锁和死亡时快照 | 持久化与跨重启恢复没有直接 Version4 字段证据 |
| Fishing | 入口主要表达 attempt 和 bobber timing | 补出资格快照、catch decision、bait reservation、result commit，共 6 项 | Bait reservation 和 result commit 的 owner 不能由 Fishing 单独宣布 |
| NPC/Town | 入口只有 identity、lifecycle、behavior、target、home 五个粗项 | 补出 legacy slot、definition、local behavior、parent、health、resident、housing relation/registry，共 12 项 | 当前 src 有多套 AI、目标和生命模型 |
| Player | 入口没有把 use、buff、再生/免疫、mount、rest、fishing capability 展开 | 详细设计为 14 项 | 跨 Player/Combat/Item/Fishing/Mount 的 owner 仍是整合事项 |
| WorldCalendar | 入口只有 clock、event instance、schedule | 详细设计为 14 项，增加历史、pending、override、DD2 和随机状态 | `CoinRain`、`CultistDelay` 尚未形成独立明确承接 |
| SimulationRuleOverrides | 入口主要是一个 RuleOverrideState | 详细设计分为世界/玩家 override、权限和两个快照，共 5 项 | 有效视图不得回写基础 WorldSession/Player 事实 |
| Teleportation | 入口主要只有 TraversalCooldown | 详细设计补出 Pylon registry、Portal endpoint/link 和两类主体冷却，共 5 项 | 当前 Pylon registry 同命名空间有两份状态源 |
| WorldGeneration | 入口只表达 Plan、Pass、Biome 三项组件候选 | 详细设计明确 13 项生成、地形、住房、液体 handoff 和 ready 状态 | `existing` 表示映射材料，不表示 Version4 等价或已接入 |
| WorldInteraction | 入口几乎只列 WiringWorkState | 详细设计展开为 29 个 Tile、TileEntity、结构和交互状态候选 | 多格结构、Pylon 和 TileEntity ID 的最终提交根未决 |
| Projectile | `ProjectileCapabilityComponents` 是多个能力的占位 | 详细设计拆出 sentry、minion、trap、bobber、counterweight 五个 capability，共 18 项 | raw AI、UUID、命中免疫、Kill/child/drop 仍需验证 |
| SpawnLifecycle | 公共层是通用生命周期、生成、身份、掉落混合项 | 详细设计固定为 8 个 Component 候选 | 不能把通用生命周期变成 NPC/Projectile/Item 的领域属性 owner |
| LeashedEntity | 入口没有展开 critter、kite 和 legacy slot | 详细设计补出两个行为组件和兼容 slot，共 8 项 | 主 Version4 运动字段/Remove/StreamNetUpdates 证据不全 |

这些“补齐”只表示设计文档链补齐，**不表示**目标 Component 已创建、已注册、已迁移或已经通过 focused verifier。

## 6. 确定性组件遗漏和未闭合 owner

### 6.1 WorldSession 世界进度字段是本轮最高优先级遗漏

当前 `src/WorldSession/WorldSessionComponents.cs:21-40` 仍把以下多个语义域放在 `WorldEventProgressState` 和 `WorldSpawnPressureState` 中：

```text
BossProgressFlags
InvasionProgressFlags
SavedNpcProgressFlags
NpcSpawnUnlockFlags
NpcWorldUnlockFlags
LunarProgressState
InvasionRuntimeState
Dd2ProgressState
PendingWorldEventsState
WorldSpawnPressureState
```

WorldSession 详细设计在 `docs/第一轮审查/设计/2026-09-05-version4-world-session-component-design.md:461-480` 明确把事件/进度和刷怪压力排除出基础五个 WorldSession Component；代码草案在 `docs/第一轮审查/设计/2026-09-06-version4-world-session-component-code-draft.md:677-690` 只给出粗略交接，其中 `WorldEventProgressState` 指向 `WorldProgression / integration-review`，`WorldSpawnPressureState` 指向 `SpawnLifecycle`。这还不是字段级承接表。

Version4 证明这些字段不能被丢弃：

- `D:\TRbackup\Version4\Terraria\NPC.cs:6151-6291` 声明 saved NPC、NPC spawn unlock、pet purchase、boss defeated、Lunar tower、shield、tower active、`LunarApocalypseIsUp` 等静态世界状态；
- `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:1320-1453` 在 `SaveWorldHeader` 中写入 Boss、saved NPC、入侵、Hardmode、Slime Rain、Cultist delay、Lunar、DD2、购买/解锁和其他世界字段；
- `D:\TRbackup\Version4\Terraria.IO\WorldFile.cs:2134-2160`、`:2278-2295`、`:2409-2425` 和 `:2544-2549` 读取这些状态或保存临时值；
- `D:\TRbackup\Version4\Terraria\WorldGen.cs:6432-6565` 在世界清理时逐项重置事件、Boss、saved NPC、购买/解锁、Lunar 和 DD2 相关字段；
- `D:\TRbackup\Version4\Terraria\Main.cs:542-550,615-628,1059-1069` 声明 Slime Rain、`coinRain`、入侵的类型/位置/规模/延迟等世界运行态。

字段级承接表如下。`candidate owner` 是建议交给最终整合会话裁决的方向，不是本报告宣布的最终 owner。

| 当前字段/字段簇 | Version4 权威/边界证据 | 详细设计目前的承接 | 当前状态 | 必须补齐的 owner/写者/边界 |
|---|---|---|---|---|
| `BossProgressFlags` | `NPC.downedBoss*` 等声明于 `NPC.cs:6211-6263`；保存/读取在 `WorldFile.cs:1320-1330,1381-1398,2134-2147,2278-2287`；网络包 7 由 `NetMessage` 发送 | 可能由 `WorldProgressionAndUnlocks.ProgressionAggregate` 或 WorldCalendar 历史消费，但没有字段级映射 | `missing`、`blocking-decision` | 明确 Boss flag 的唯一 Component、NPC kill 事实输入、WorldFile 字段版本、包 7 projection 和 reset writer |
| `InvasionProgressFlags` | `NPC.downedGoblins/Frost/Pirates/Martians` 声明于 `NPC.cs:6221-6233`；完成时 `Main.cs:12546-12573` 写入，存档在 `WorldFile.cs:1334-1337,2151-2154` | `WorldCalendar.InvasionHistoryState` 已提出部分 Goblin/Frost/Pirate/Martian history | `partial` | 明确 `Clown`、历史 flag 与 `WorldInvasionState` 当前运行态的拆分；一个完成事实 writer |
| `SavedNpcProgressFlags` | `NPC.cs:6151-6165`；保存/读取分散在 `WorldFile.cs:1331-1333,1372-1376,2148-2150,2214-2231`；生成资格读取在 `Main.cs:13929-14159` | WorldSession 排除；NPC/Town 与 WorldProgressionAndUnlocks 都可能消费 | `missing`、`blocking-decision` | saved NPC 是世界历史、NPC 生成资格还是 Town owner；明确 saved flag→spawn eligibility 的只读方向 |
| `NpcSpawnUnlockFlags` | `NPC.cs:6173-6203`；保存/读取在 `WorldFile.cs:1436-1453` 及相应 LoadHeader 分支；清理在 `WorldGen.cs:6538-6553` | WorldSession 排除；WorldProgressionAndUnlocks 仅有 aggregate 粗字段，Npc/Town 只有资格/行为组件 | `missing` | 逐项定义 spawn unlock 的持久 owner、解锁事件、NPC spawn Query 输入以及 reset/load writer |
| `NpcWorldUnlockFlags` | `NPC.cs:6205-6209,6167-6171`；保存/读取 `WorldFile.cs:1418-1419,1430-1432,2411-2419`；消息处理 `MessageBuffer.cs:2198-2206` | WorldSession 排除；`ProgressionAggregate` 的 `ChattedPersistentIds` 等不能自动等价于这些 bool | `missing`、`evidence-gap` | 记录 pet purchase、combat book、satchel 各自的事实来源、外部 ID、幂等和网络/存档投影 |
| `LunarProgressState` | `NPC.cs:6251-6283`；保存/读取 `WorldFile.cs:1390-1398,2280-2287`；清理 `WorldGen.cs:6560-6561`；Lunar 运行逻辑在 `WorldGen.cs:73046-73141` | WorldCalendar 有 DD2/事件组件，但没有完整 Lunar tower/shield/apocalypse Component；WorldProgression 也未明确承接 | `missing`、`blocking-decision` | 明确 tower defeated/active/shield/countdown/apocalypse 的 owner、Hardmode/事件顺序和 persistence revision |
| `CultistDelay` | `WorldFile` 用 `_tempCultistDelay` 保存/恢复 `CultistRitual.delay`：`WorldFile.cs:114,165,1044,1080,1377,2246-2250` | WorldCalendar 的 Pending/Override 候选可以消费，但没有最终字段归属 | `missing` | 明确它是日历事件资格、仪式运行态还是存档 staging；不得继续只藏在 WorldSession 宽状态或 adapter 临时字段 |
| `CoinRain` | `Main.coinRain` 声明 `Main.cs:622`；Start/Stop/递减在 `Main.cs:12829-12854`、`WorldGen.cs:59709-59752`；存档 staging 在 `WorldFile.cs:144,180,1098,1461,2544-2549` | WorldCalendar 设计没有明确列出 `CoinRain` Component 字段；WorldWeather 的 rain 也不能直接吸收金币剩余量 | `missing`、`blocking-decision` | 区分雨天气事实、Coin Rain 事件运行态、金币经济结果和持久化临时值；定义唯一递减 writer |
| `WorldSpawnPressureState` | `WorldGen.npcSpawnDelay`：`WorldGen.cs:4189,62430-62432`；当前 NLTX 聚合字段在 `WorldSessionComponents.cs:36-40` | 代码草案粗略指向 SpawnLifecycle；SpawnLifecycle 详细设计没有把全部压力字段列为独立 canonical Component | `partial`、`blocking-decision` | 明确 `NpcSpawnDelay/Period`、town scan、event budget、slime slots、active counts 的 owner、频率、派生/缓存分类和 Spawn Query 输入 |
| `WorldEventProgressState` 全体嵌套字段 | WorldFile header/load、NetMessage 包 7、WorldGen reset 和 Main event tick 都读写；见上述证据 | 已由 Calendar 部分承接 `InvasionRuntimeState`、`Dd2ProgressState`、`PendingWorldEventsState`、Slime Rain，但 Boss/saved NPC/unlock/Lunar/CoinRain/CultistDelay 未闭合 | `partial`、`blocking-decision` | 建立字段级 `sourceMember → targetComponent → authoritativeWriter → reset/load/save/net projection` 表；禁止继续把聚合对象当作最终 owner |

### 6.2 WorldProgressionAndUnlocks 的 `UnlockFactComponent` 设计链断裂

公共报告 `docs/第一轮审查/2026-09-05-version4-world-progression-and-unlocks-public-decomposition.md:67-74` 提出 `UnlockFactComponent`、`ProgressionAggregate` 和 `UnlockEvent`。但详细设计 `docs/第一轮审查/设计/2026-09-06-version4-world-progression-and-unlocks-component-design.md:311-319` 的最终清单只保留：

```text
ProgressionAggregate
BestiaryDiscoveryCache
```

这有两种可能解释：

1. `UnlockFactComponent` 被有意吸收进 `ProgressionAggregate`，则需要补一条字段级映射和“为什么仍保持一个 Aggregate”的设计记录；
2. `UnlockFactComponent` 本应保留为独立事实组件，则当前详细最终清单遗漏了它，且其 writer、lifecycle、persistence 和 replication owner 未闭合。

该问题不应通过猜测解决，因为选择 1 会影响 World 级组件粒度和事件提交边界，选择 2 会增加 Component 和跨域 writer。当前标记为 `evidence-gap` + `blocking-decision`。

### 6.3 Combat 的 `EncounterDamageCreditComponent` 归属和批次冲突

公共报告只列 `HealthComponent`、`ImmunityComponent`、`StatusEffectStateComponent`，见 `docs/第一轮审查/2026-09-05-version4-combat-and-status-public-decomposition.md:61-72`。详细报告 `docs/第一轮审查/设计/2026-09-06-version4-combat-and-status-component-design-report.md:74-92` 明确列出 11 个顶层组件，并说明 `EncounterDamageCreditComponent` 属于 Combat 但附着在独立遭遇根实体。

然而同一详细报告 `:402-416` 又把目标 Combat 目录描述为只新增或演进“前九个 Combat 组件”。前九项截止 `StatusEffectImmunityComponent`，不包括 `EncounterDamageCreditComponent`。这不是命名瑕疵，而会改变：

- 组件总数和验收清单；
- Encounter root 的 entity scope；
- Combat 与 Spawn/Death/WorldProgression 的提交方向；
- 遭遇贡献账本的持久化、网络和生命周期批次。

可接受的候选解释：

| 方案 | 解释 | 影响 |
|---|---|---|
| A | Combat 域包含十个 Combat 组件候选，九个附着受击实体，一个附着遭遇根 | 组件清单和实现批次最清晰，但需要补 Encounter root/ID/生命周期契约 |
| B | `EncounterDamageCreditComponent` 归属 Combat 但延后到 Attribution 批次 | Combat 当前批次为九个实体组件，跨批次接口必须固定，不能在清单中继续写成已纳入 |
| C | 归入独立的 Encounter/Attribution responsibility | 会改变 19 子系统责任映射，需要最终索引和 owner 变更，不能在本报告单独决定 |

当前状态：`blocking-decision`、`design-chain-inconsistency`、`evidence-mismatch`（详细报告输入路径同时引用不存在的 `docs\design\...` 和不存在/未找到的中文路径；实际可读主题报告是该 `docs\research\2026-09-06...component-design-report.md`）。

## 7. 当前 NLTX 的重复/竞争 owner 证据

以下不是“数量遗漏”，而是组件拆分后更危险的反向问题：同一语义仍有多个可能写入根。若不先裁决，增加更多 Component 会把双写固化。

### 7.1 Spatial / Physics / Entity

| 语义 | 重复路径和实际字段 | 风险 |
|---|---|---|
| 碰撞策略 | `src/Physics/CollisionPolicyComponent.cs:3-39` 与 `src/SpatialSimulation/CollisionPolicyComponent.cs:8-43` 字段和派生 `CanApplyFallThrough` 基本同形 | 两个命名空间都可能被注册/引用；唯一 owner、文件组织和 writer 未闭合 |
| 碰撞结果 | `src/Physics/CollisionResultComponent.cs:8-33` 的可变 `List` 与 `src/SpatialSimulation/CollisionResultComponent.cs:14-90` 的封装列表、`Replace/Clear`、参数验证 | 字段宽度、可变性和提交 API 不同；不能把它们直接视作同名兼容类型 |
| 运动历史 | `src/Share/Entity/Components/MotionHistoryComponent.cs:3-16` 与 `src/SpatialSimulation/MotionHistoryComponent.cs:11-68` | Spatial 版本增加 `PreviousDirection`、有限值/ tick 校验、`Record/Clear`；需要确定共享 Entity 组件还是 Spatial capability |
| 空间引用 | `src/Share/Entity/Components/SpatialReferenceComponent.cs:5-20` 与 `src/SpatialSimulation/SpatialReferenceComponent.cs:12-55` | 字段近似但命名空间不同；`EntityReference`、parent relation 和空间 owner 未定 |
| 液体接触 | `src/Share/Entity/Components/LiquidContactStateComponent.cs:3-14` 与 `src/SpatialSimulation/LiquidContactComponent.cs:10-80` | 前者 `WetTickCount:int`，后者 `byte`，后者还维护连续接触、`Replace/Clear` 和 `DominantLiquidKind`；存在字段宽度和生命周期冲突 |

### 7.2 ItemContainerAndEconomy

| 语义 | 重复路径和实际字段 | 风险 |
|---|---|---|
| Item 实例/堆叠 | `src/Items/ItemState.cs:3-6` 同时保存 `Type/Prefix/Stack`；`src/Items/Instances/ItemInstanceComponent.cs:23-34` 保存 definition/prefix/variant/dye/identity；`src/Items/Instances/ItemStackComponent.cs:17-31` 与 `src/Items/StackableItemComponent.cs:17-24` 都保存 quantity/maximum/stack key | `Stack` 与 `Quantity` 有多个 authority；Item type 也和 DefinitionRef 形成多个 content identity |
| 容器容量/内容 | `src/Items/ContainerComponent.cs:5-14` 同时保存 `Capacity/Contents`；`src/Items/Containers/ContainerCapacityComponent.cs:3-24` 和 `ContainerContentsComponent.cs:5-46` 已分别保存两者 | 聚合组件与拆分组件可能双写；应先定义兼容 façade 或唯一 commit root |
| 装备关系 | `src/Items/EquipmentComponent.cs:5-40` 与 `src/Items/Equipment/EquipmentRelationComponent.cs:5-50` 都保存 functional/vanity/dye/hidden slots | 一个使用 `EntityReference`，一个使用 `RuntimeEntityId?`；关系身份与 owner 不一致 |
| 世界物品 | `src/Items/WorldItemComponent.cs:3-35` 与 `src/Items/WorldDrops/WorldItemStateComponent.cs:3-45` 都保存位置、生成/过期、active/instanced/grabbed/conveyor/revision 的近似状态 | WorldItem 与 SpawnLifecycle/ItemContainer 的生命周期、ReplicationId、SpawnSource 和 active writer 未裁决 |

### 7.3 NpcAndTownSimulation / Combat

| 语义 | 重复路径和实际字段 | 风险 |
|---|---|---|
| NPC identity/slot | `src/Npc/NpcEntityIdentityComponent.cs:3-15` 同时保存 `InstanceId/LegacySlot`；`src/Npc/NpcIdentityComponent.cs:8-22` 单独保存 InstanceId；`src/Npc/NpcLegacySlotComponent.cs:6-20` 单独保存 LegacySlot | 三份组合方式；`InstanceId` 是否稳定持久化 ID、slot 是否兼容字段没有统一关系 |
| NPC AI | `src/Npc/NpcBehaviorComponent.cs:8-33` 保存 `AiStyle/Action/AiSlots`；`src/Npc/NpcBehaviorStateComponent.cs:3-23` 保存 `BehaviorKind/LegacyAiStyle/Action/AuthoritativeAiSlots/LastUpdatedTick`；`src/Npc/NpcAiStateComponent.cs:3-26` 保存 style、四个 state 和 timer | AI style/action/slot/timer 有多个状态源；更新 writer 和 compatibility 期限不明确 |
| NPC target | `src/Npc/NpcTargetComponent.cs:6-67` 保存 target reference、kind、legacy/previous index、selection tick；`src/Npc/TargetingComponent.cs:5-13` 另保存 target | 关系、兼容索引和选择时间可能被不同系统写入；应保留单一关系 owner |
| NPC health | `src/Npc/NpcHealthComponent.cs:5-31` 保存 Current/Maximum/`IsDead`；`src/Combat/HealthComponent.cs:5-18` 保存 Current/Maximum/`IsDepleted` | `IsDead` 与 `IsDepleted` 语义和死亡生命周期不同；不能并行写两份生命 |
| 伤害归因 | `src/Combat/DamageContributionComponent.cs:5-22` 按 `EntityReference` 累加；`src/Combat/EncounterDamageCreditComponent.cs:6-45` 另保存 encounter、world damage、last contributor、时间、lifecycle、revision | 前者不能简单重命名为后者；遭遇根、世界伤害和 NPC/玩家贡献的归属需裁决 |
| 命中冷却/免疫 | `src/Combat/HitCooldownComponent.cs:5-43` 按 target 递减；`src/Combat/ImmunityComponent.cs:6-85` 保存带 key 的窗口并以 `RemainingTicks` 兼容投影 | 作用域可能不同，但两者都可能被命中系统读取/写入；需写明 projectile-local、target-local、general window 的边界 |
| 状态效果 | `src/StatusEffects/StatusEffectsComponent.cs:3-10` 保存可变 `TimedStatusEffect` 列表；`src/Combat/StatusEffectSlotsComponent.cs:6-25` 保存固定槽位和 revision | 前者有 `Source/Stacks`，后者设计明确不保存来源；状态效果实例、槽位、来源和网络投影未统一 |

### 7.4 WorldSession / WorldGeneration / Teleportation

| 语义 | 重复路径和实际字段 | 风险 |
|---|---|---|
| World descriptor | `src/WorldSession/WorldDescriptorState.cs:5-35` 与 `src/WorldSession/WorldGeneration/WorldDescriptorState.cs:3-25` 同名但字段表达不同：根版保存四个边界，Generation 版保存 `WorldBounds` | 同名类型可能造成 namespace/注册/序列化选择漂移；不能依赖目录顺序定 owner |
| World rules | `src/WorldSession/WorldRulesState.cs:3-15` 与 `src/WorldSession/WorldGeneration/WorldRulesState.cs:3-17` 都保存 GameMode/HardMode/SecretSeeds/WorldEvil，但 Ore 字段名称不同 | Hardmode、OreTiers 和 generation rules 的 authority/过渡提交未定 |
| Game mode / evil type | `src/WorldSession/WorldGameMode.cs:1-10` 与 `src/WorldSession/WorldGeneration/WorldGameMode.cs:1-10`；`WorldEvilType.cs` 也有同样两套 | 共享值类型 owner 与公共 namespace 未闭合 |
| Preparation state | `src/WorldSession/WorldSessionComponents.cs:6` 与 `src/WorldSession/WorldGeneration/WorldPreparationState.cs:3-11` | 枚举值目前相似，但 WorldSession readiness 与 WorldGeneration lifecycle 的边界未统一 |
| Pylon registry | `src/WorldStorage/PylonRegistryComponent.cs:5-28` 与 `src/WorldStorage/PylonRegistryState.cs:3-10` 都保存 Current/Previous、Count、refresh cooldown、Revision | 同一命名空间的双状态源；Teleportation 详细设计只承接 `PylonRegistryComponent`，需明确另一类型是兼容投影、快照还是删除候选 |

上述重复项均应先完成“旧字段/新组件单写根”裁决，再进行机械迁移；本报告不判断当前是否已经产生编译错误，因为本轮没有编译。

## 8. 其他组件级遗漏/风险复查

### 8.1 已经覆盖但仍为 partial 的边界

- **LiquidSimulation**：详细设计增加了 `LiquidSequenceStateComponent`，但 sequence 是否独立于 World runtime、Section dirty set 和 Liquid work queue 仍未决；当前 shared `LiquidContactStateComponent` 与 Spatial `LiquidContactComponent` 的 `int`/`byte` 差异必须先裁决。
- **ProjectileSimulation**：详细设计已将 capability placeholder 展开为五类能力组件，但 `ProjectileNetworkStateComponent`、`ProjectileTrailCacheComponent`、`ProjectileSourceMetadataComponent` 不能成为第二份权威 Projectile snapshot；`projUUID` 不能未经身份整合成为持久化主键。
- **WorldGenerationAndEcology**：13 项清单已覆盖生成 lifecycle、plan、pass、random、terrain、biome、housing、liquid handoff 和 ready，但当前 `WorldGenerationPipeline` 只是受限流程。详细设计中的 `existing`/`partial` 不能转译为 Version4 全量行为。
- **WorldInteractionAndStructures**：29 项清单补足了 Tile/TileEntity/结构粒度，但 `TileCell`、`TileFrame`、`TileSignalTopology`、TileEntity ID、Pylon registry 和多格结构的最终 commit root 仍未锁定。
- **SpawnLifecycleAndLoot**：8 项清单补出了通用 lifecycle、admission、identity、relation、NPC definition/lifetime、loot resolution/attribution；但 `EntityIdentityState`、`EntityRelationState`、`LootAttributionState` 都跨域，不能由 SpawnLifecycle 单独宣布 owner。
- **DeathPenaltyAndRevenge**：4 项清单已补出 marker 的结构粒度；没有证据证明 marker 是长期存档实体，因此不能把 marker persistence 当成已确认能力，也不能把 marker identity 当 runtime entity identity。

### 8.2 设计文档自身的路径/状态不一致

已发现并纳入审计的文档可审计性风险：

| 风险 | 证据 | 影响 |
|---|---|---|
| Combat 详细报告的输入路径不存在/不一致 | `docs/第一轮审查/设计/2026-09-06-version4-combat-and-status-component-design-report.md:5-22` 先记录 `docs\design\...` 不存在，又引用未在当前工作树找到的中文设计路径，随后才说明实际主题报告路径 | 读者无法稳定反向追到字段设计源；不能把该链当作闭合 manifest |
| WorldSession 详细设计中的路径拼接错误 | `docs/第一轮审查/设计/2026-09-05-version4-world-session-component-design.md:486-491` 出现 `WorldGeneration/D:\TRbackup\NLTX\...` 的拼接路径 | 当前映射证据仍可由实际文件复核，但该引用本身应标 `evidence-mismatch` |
| public report 的 Component/System 混合计数 | 例如 Combat `:65-72`、Fishing `:65-72`、WorldCalendar `:67-75` 同时列状态、Query、System、Command、Projection/Adapter | 如果直接统计 proposed 行，会把行为/协议对象错误算作 Component |
| detailed `status: existing/partial` 与实现状态混用风险 | WorldGeneration 详细清单 `:917-937` 明确同时使用 existing/partial/proposed，且声明不等于迁移完成 | 汇总工具必须把 design status 与 current implementation status 分列 |

## 9. 证据闭合与 focused verifier 计划

本轮只做文档/源码静态复查，未执行 verifier。后续应按以下顺序建立可重复的证据闭合：

1. **Manifest 级覆盖**：为每个 Version4 关键成员建立 `sourceMemberId`、实际源路径/行号、读者、写者、生命周期、state kind、target component、target path、persistence/network refs 和 evidence status；公共报告只作为入口，不作为最终映射事实。
2. **Owner 唯一性扫描**：对 `WorldSessionComponents.cs`、上述重复 src 类型和所有 detailed Component 清单生成字段名/类型/namespace/写者交叉表；凡同一字段存在两个可变写入口，状态为 `duplicate-owner`，不得自动迁移。
3. **WorldSession 进度闭合 verifier**：验证 Boss、saved NPC、spawn unlock、pet/combat book、Lunar、invasion、DD2、Pending、CoinRain、CultistDelay、spawn pressure 各字段至少有唯一目标 owner 和 Save/Load/Reset/Net projection 证据。
4. **Combat shape/chain verifier**：验证 11 项顶层组件、Encounter component 的批次归属、Health 与 NPC health 的单一 writer、Immunity 与 HitCooldown 的作用域、StatusEffects 来源字段的承接。
5. **重复组件 verifier**：验证 Physics/Spatial/Share、Items、NPC、WorldSession/WorldGeneration、Pylon registry 的 canonical 类型选择、旧类型兼容策略和无双写条件。
6. **跨域提交 verifier**：验证 Player→Movement→Collision→Combat→Death→Loot/Destroy/Respawn→WorldStorage commit→Replication snapshot 的事务方向；验证 WorldSession/WorldCalendar/WorldProgression/WorldGeneration 的 Tick 与长事务不互相隐式写入。

建议命令只在后续实现/验证阶段使用，并必须遵守仓库的 serial dotnet policy；本报告没有运行任何 `dotnet` 命令。

## 10. Integration Handoff

```text
subsystemId: Version4ComponentSplitAudit
taskNumber: 2026-09-07-review
reportPath: D:\TRbackup\NLTX\docs\第一轮审查\2026-09-07-version4-component-split-omission-review.md

evidenceStatus: partial
nltxStatus: partial
verificationStatus: not-run

confirmedOwners:
- Version4 `NPC.cs`、`Main.cs`、`WorldGen.cs`、`Terraria.IO\WorldFile.cs` 中已确认世界进度、事件、天气、入侵、Lunar、DD2 和生成清理事实存在。
- 19 份公共报告和同主题详细设计的 Component 候选范围已完成对照；公共入口粗粒度遗漏多数已在详细设计中展开。
- 当前 NLTX 的重复状态源和文档路径 mismatch 已定位到具体文件与行号。

proposedTypes:
- 各详细 Component-only 设计中列出的 Component 候选；它们均不因本报告而变为 existing 或 implemented。
- WorldSession 世界进度字段的唯一 owner/commit/persistence/network manifest。
- Combat Encounter attribution 批次边界和 UnlockFact→ProgressionAggregate 映射记录。

sharedTypesForIntegrationReview:
- EntityReference、RuntimeEntityId、PersistentEntityId、NetworkId、CompatibilitySlot、WorldSectionId、TileCoordinate、WorldTime、ItemInstanceId、WorldTickSnapshot。
- WorldSession、WorldCalendar、WorldProgression、WorldGeneration、WorldStorage 和 ExternalBoundaries 间的 commit/result/projection 类型。

crossSubsystemReaders:
- PlayerGameplay、NpcAndTownSimulation、ProjectileSimulation、FishingAndCatchSimulation、SpawnLifecycleAndLoot、WorldCalendarAndEventOrchestration、WorldProgressionAndTransition、WorldProgressionAndUnlocks、WorldGenerationAndEcology、WorldInteractionAndStructures、TeleportationAndTraversal、SimulationRuleOverrides、SpatialSimulation。

crossSubsystemWriters:
- Version4 中由 NPC/Main/WorldGen/WorldFile/NetMessage/MessageBuffer/事件系统共同形成的写入边界；NLTX 目标写者必须由 integration-review 收敛为显式 System/Command/Adapter 根。

orderingConstraints:
- WorldSession readiness/generation barrier 必须先于可用世界实体 Tick。
- 事件/进度事实提交必须先于依赖该事实的资格 Query、NPC spawn 和 WorldData/persistence projection。
- Hardmode 长事务必须与普通 WorldGeneration Tick 分离，并在规则、Tile、存档、区段重同步和结果投影之间建立提交屏障。
- Player/NPC/Projectile/WorldItem/LeashedEntity 的空间更新顺序必须由调度契约显式表达，不能由文件顺序推断。

boundaryChallenges:
- 不要把公共报告的混合候选计数当最终 Component 计数。
- 不要把 WorldSession 的事件/进度聚合重新扩成巨型 Component；先逐字段确定 owner。
- 不要在重复 src 类型之间双写；先选择 canonical 类型或明确 compatibility façade。
- 不要把 Projection、Persistence Adapter、Network ID 或 legacy slot 当作模拟 authority。

evidenceGaps:
- Boss/saved NPC/NPC unlock/Lunar/CoinRain/CultistDelay/WorldSpawnPressureState 的字段级最终 owner、writer、lifecycle、save/load/net 证据未闭合。
- Combat Encounter component 批次和 WorldProgressionAndUnlocks UnlockFactComponent 的设计链未闭合。
- 多个实体/世界/网络/存档 ID 的共享转换与失效语义未闭合。
- 当前 root `src`、`dome` 运行时装配和 verifier 未编译验证。

blockingDecisions:
- WorldSession 世界进度字段由 WorldCalendar、WorldProgressionAndUnlocks、NpcAndTown、SpawnLifecycle 还是跨域 integration owner 承接。
- `EncounterDamageCreditComponent` 是否进入 Combat 本批次、延后 Attribution 批次，或改变责任域。
- `UnlockFactComponent` 是否被 `ProgressionAggregate` 明确吸收。
- Spatial/Physics/Share、Items、NPC、WorldSession/WorldGeneration、Pylon registry 的 canonical owner 和兼容迁移路线。
- Hardmode/WorldData/WorldFile/Tile commit 的原子提交边界。

notImplemented:
- 本报告没有创建或修改任何 `.cs`、`.csproj`、测试、迁移、索引、覆盖文件或既有研究/设计文件。
- 没有实现、注册、接线或迁移任何 proposed Component/System/Query/Command/Adapter/Projection。

verifierPlan:
- 运行前先生成源/目标 snapshot 和 manifest fingerprint。
- 先做 WorldSession progress owner verifier 与 duplicate-owner verifier，再做 Combat chain verifier。
- 使用仓库指定的 `Build/Tools/Invoke-SerialDotnet.ps1` 串行命令；本轮不运行，状态保持 `not-run`。
```

## 11. 最终声明

本报告是对 `2026-09-05-version4-public-decomposition-common-protocol` 规定的 19 个子系统公共拆分入口、后续 Component-only 详细设计和当前 NLTX 局部状态的只读复查。它用于发现组件粒度遗漏、字段 owner 缺口、重复权威状态和文档证据链风险。

本报告不是迁移完成报告，不是行为等价证明，不是 API 兼容证明，也不是当前 NLTX 已实现能力的声明。所有设计候选仍必须遵守 `public-decomposition` 的 Component/System/Query/Command/Adapter/Projection 边界、唯一写者、显式调度和副作用隔离要求。

本轮 `verificationStatus: not-run`。没有运行 `dotnet restore/build/test/run/publish/pack/msbuild`，没有生成或判断 `Build/bin`、`Build/obj` 为可信验证结果。没有修改生产代码、测试代码、Version4、完整参考源码、tModLoader 文档、SS14 参考源码、既有审查报告、详细设计、索引、覆盖表或迁移材料。
